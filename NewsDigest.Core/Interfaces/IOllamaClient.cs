namespace NewsDigest.Core.Interfaces;

public interface IOllamaClient
{
    Task<string> GenerateAsync(string systemPrompt, string userPrompt, CancellationToken ct = default);
}