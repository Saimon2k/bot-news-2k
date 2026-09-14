using System.Threading.Tasks;
using NewsDigest.Core.Interfaces;
using OllamaSharp;
using Serilog;

namespace NewsDigest.Infrastructure.Wrappers
{
    public class OllamaClient : IOllamaClient
    {
        private readonly ILogger _logger;
        private readonly OllamaApiClient _ollamaApiClient;

        public OllamaClient(ILogger logger, string ollamaUrl)
        {
            _logger = logger;
            _ollamaApiClient = new OllamaApiClient(ollamaUrl);
        }

        public async Task<string> GenerateAsync(string prompt)
        {
            try
            {
                var response = await _ollamaApiClient.GenerateAsync(prompt, "llama3");
                
                _logger.Information("Successfully generated response from Ollama");
                return response;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error generating response from Ollama");
                throw;
            }
        }
    }
}