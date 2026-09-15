using System.Text.Json;

namespace NewsDigest.Core.Services;

public static class DigestResponseParser
{
    public static DigestResult? TryParse(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;

        try
        {
            using var doc = JsonDocument.Parse(raw);
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