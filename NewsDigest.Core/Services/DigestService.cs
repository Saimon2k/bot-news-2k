using System.Text.Json;
using System.Text.Json.Serialization;

namespace NewsDigest.Core.Services;

public class DigestService(
    ILogger<DigestService> logger,
    IOllamaClient ollamaClient,
    IUserbotClient userbotClient) : IDigestService
{
    private const string SystemPrompt = """
        Ты — редактор новостного дайджеста. Ты не выдумываешь новости. Ты работаешь только с входным JSON.

        Ниже JSON-массив новостей. Каждая новость имеет id, channel, date, text, type.
        type: text | media_with_caption | media_without_text.
        Задача:
        1. Убери рекламу, промо, партнёрские ссылки, приглашения, скам.
        2. Убери смысловые повторы. Если несколько новостей об одном и том же — оставь одну, самую раннюю и полную.
        3. Верни строго JSON без текста вокруг. ВАЖНО: отвечай ТОЛЬКО JSON, без пояснений, без markdown, без лишнего текста.
        
        Формат ответа:
        {
          "selected": [{"id": 123, "reason": "важная новость"}],
          "ads": [456],
          "duplicates": [789]
        }
        
        Не добавляй id, которых нет во входе. Не меняй id. Не пиши ничего кроме JSON.
        """;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Converters = { new JsonStringEnumConverter() },
    };

    public async Task<IReadOnlyList<DigestItem>> CreateDigestAsync(int userId, IReadOnlyList<CollectedMessage> messages, CancellationToken ct = default)
    {
        if (messages.Count == 0) return [];

        var input = messages
            .Select(m => new LlmInput(m.Id, string.IsNullOrEmpty(m.ChannelUsername) ? m.ChannelId.ToString() : m.ChannelUsername, m.Date, m.NormalizedText, m.Type))
            .ToList();
        var userPrompt = JsonSerializer.Serialize(input, JsonOptions);

        logger.LogInformation("Обращаюсь к LLM: {Count} канонических новостей для пользователя {UserId}", messages.Count, userId);
        var result = await QueryLlmAsync(messages, userPrompt, ct);

        var byId = messages.ToDictionary(m => m.Id);
        var items = new List<DigestItem>();
        var order = 0;

        foreach (var selection in result.Selected)
        {
            if (!byId.TryGetValue(selection.Id, out var source)) continue;

            items.Add(new DigestItem
            {
                OrderIndex = order++,
                ChannelId = source.ChannelId,
                ChannelUsername = source.ChannelUsername,
                MessageId = source.MessageId,
                MessageUrl = await userbotClient.GetMessageUrlAsync(source.ChannelId, source.ChannelUsername, source.MessageId, ct),
                Type = source.Type,
                PreviewText = source.Text.Length <= 200 ? source.Text : source.Text[..200],
                Reason = selection.Reason,
            });
        }

        logger.LogInformation("Дайджест сформирован: {Count} новостей", items.Count);
        return items;
    }

    private async Task<DigestResult> QueryLlmAsync(IReadOnlyList<CollectedMessage> messages, string userPrompt, CancellationToken ct)
    {
        for (var attempt = 1; attempt <= 2; attempt++)
        {
            try
            {
                var raw = await ollamaClient.GenerateAsync(SystemPrompt, userPrompt, ct);
                var parsed = DigestResponseParser.TryParse(raw);
                if (parsed is not null) return parsed;

                logger.LogWarning("Попытка {Attempt}: LLM вернул невалидный JSON. Ответ: {Response}", attempt, raw.Length > 500 ? raw[..500] + "..." : raw);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Попытка {Attempt}: ошибка вызова LLM", attempt);
            }
        }

        logger.LogWarning("LLM не обработал батч, использую fallback: все канонические новости");
        return Fallback(messages);
    }

    private static DigestResult Fallback(IReadOnlyList<CollectedMessage> messages) =>
        new(messages.Select(m => new DigestSelection(m.Id, "fallback: все новости")).ToList(), [], []);

    private sealed record LlmInput(int id, string channel, DateTime date, string text, MessageType type);
}