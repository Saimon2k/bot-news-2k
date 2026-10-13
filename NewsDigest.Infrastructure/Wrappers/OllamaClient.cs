using System.Text;
using OllamaSharp;

namespace NewsDigest.Infrastructure.Wrappers;

public class OllamaClient(ILogger<OllamaClient> logger, IOptions<OllamaOptions> options) : IOllamaClient
{
    private readonly OllamaApiClient _ollama = new(new Uri(options.Value.Url)) { SelectedModel = options.Value.Model };

    public async Task<string> GenerateAsync(string systemPrompt, string userPrompt, CancellationToken ct = default)
    {
        logger.LogInformation("Ollama: используется URL={Url}, Model={Model}", options.Value.Url, options.Value.Model);
        
        var prompt = $"{systemPrompt}\n\n{userPrompt}";
        var builder = new StringBuilder();

        await foreach (var chunk in _ollama.GenerateAsync(prompt, cancellationToken: ct))
            builder.Append(chunk?.Response);

        var result = builder.ToString();
        logger.LogInformation("Ollama: получен ответ длиной {Length} символов", result.Length);
        return result;
    }
}