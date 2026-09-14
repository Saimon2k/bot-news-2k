using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NewsDigest.Core.Interfaces;
using NewsDigest.Core.Models;

namespace NewsDigest.Core.Services
{
    public class DigestService : IDigestService
    {
        private readonly IOllamaClient _ollamaClient;
        private readonly string _promptTemplate = @"
Ты — редактор новостного дайджеста. Ты не выдумываешь новости. Ты работаешь только с входным JSON.

Ниже JSON-массив новостей. Каждая новость имеет id, channel, date, text, type.
type: text | media_with_caption | media_without_text.

Задача:
1. Убери рекламу, промо, партнёрские ссылки, приглашения, скам.
2. Убери смысловые повторы. Если несколько новостей об одном и том же — оставь одну, самую раннюю и полную.
3. Верни строго JSON без текста вокруг:
{
  ""selected"": [{""id"": ""..."", ""reason"": ""...""}],
  ""ads"": [""...""],
  ""duplicates"": [""...""]
}
Не добавляй id, которых нет во входе. Не меняй id. Не пиши ничего кроме JSON.";

        public DigestService(IOllamaClient ollamaClient)
        {
            _ollamaClient = ollamaClient;
        }

        public async Task<List<DigestItem>> CreateDigestAsync(int userId, List<CollectedMessage> messages)
        {
            // This method needs to be fully implemented based on actual data flow
            // We'll start with skeleton implementation
            
            // 1. Format the messages into JSON according to prompt
            // 2. Send to Ollama using _promptTemplate  
            // 3. Parse and validate response
            // 4. Create DigestItem list in order returned by LLM
            // 5. Return list of DigestItems
            
            var digestItems = new List<DigestItem>();
            
            return digestItems;
        }
    }
}