using System.Text.Json;
using System.Text.RegularExpressions;

namespace NewsDigest.Core.Services;

public static class DigestResponseParser
{
    private static readonly Regex JsonMarkdownBlockRegex = new(@"```(?:json)?\s*(\{.*?\})\s*```", RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex JsonObjectRegex = new(@"(\{[^{}]*\{[^{}]*\}[^{}]*\})", RegexOptions.Singleline | RegexOptions.Compiled);

    public static DigestResult? TryParse(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;

        // Пробуем напрямую распарсить
        var result = TryParseJson(raw);
        if (result is not null) return result;

        // Пробуем извлечь JSON из markdown-блока ```json ... ```
        var markdownMatch = JsonMarkdownBlockRegex.Match(raw);
        if (markdownMatch.Success)
        {
            result = TryParseJson(markdownMatch.Groups[1].Value);
            if (result is not null) return result;
        }

        // Последняя попытка: найти JSON-объект в тексте
        var jsonMatch = JsonObjectRegex.Match(raw);
        if (jsonMatch.Success)
        {
            result = TryParseJson(jsonMatch.Groups[1].Value);
            if (result is not null) return result;
        }

        return null;
    }

    private static DigestResult? TryParseJson(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            if (root.ValueKind != JsonValueKind.Object) return null;

            var selected = new List<DigestSelection>();
            if (root.TryGetProperty("selected", out var selectedElement) && selectedElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var element in selectedElement.EnumerateArray())
                {
                    if (element.ValueKind != JsonValueKind.Object || !element.TryGetProperty("id", out var idElement)) continue;
                    if (!TryGetInt(idElement, out var id)) continue;
                    var reason = element.TryGetProperty("reason", out var reasonElement) ? reasonElement.GetString() : null;
                    selected.Add(new DigestSelection(id, reason ?? string.Empty));
                }
            }

            var ads = ExtractIds(root, "ads");
            var duplicates = ExtractIds(root, "duplicates");
            return new DigestResult(selected, ads, duplicates);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static List<int> ExtractIds(JsonElement root, string propertyName)
    {
        var ids = new List<int>();
        if (!root.TryGetProperty(propertyName, out var element) || element.ValueKind != JsonValueKind.Array) return ids;

        foreach (var item in element.EnumerateArray())
        {
            if (TryGetInt(item, out var id)) ids.Add(id);
        }

        return ids;
    }

    private static bool TryGetInt(JsonElement element, out int id)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Number:
                id = element.GetInt32();
                return true;
            case JsonValueKind.String when int.TryParse(element.GetString(), out id):
                return true;
            default:
                id = 0;
                return false;
        }
    }
}