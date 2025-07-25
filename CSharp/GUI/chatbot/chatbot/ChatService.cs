using System.IO;
using Azure;
using Azure.AI.OpenAI;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using OpenAI.Chat;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using System.Text;
using System.Linq;
using BinaryData = System.BinaryData;

namespace chatbot
{
    public class ChatService
    {
        private readonly string endpoint;
        private readonly string apiKey;
        private readonly string searchKey;
        private readonly string searchEndpoint;
        private readonly string indexName;
        private readonly string docIntelligenceEndpoint;
        private readonly string docIntelligenceKey;

        private readonly SearchClient searchClient;
        private readonly ChatClient chatClient;

        public ChatService()
        {
            string inputPath = "Appkey.bin";
            int seed = 12345;
            byte[] shuffled = File.ReadAllBytes(inputPath);
            // Generate same shuffle order
            int[] indices = Enumerable.Range(0, shuffled.Length).ToArray();
            Random rng = new Random(seed);
            indices = indices.OrderBy(_ => rng.Next()).ToArray();
            // Reverse shuffle
            byte[] originalBytes = new byte[shuffled.Length];
            for (int i = 0; i < shuffled.Length; i++)
                originalBytes[indices[i]] = shuffled[i];

            string content = Encoding.UTF8.GetString(originalBytes);
            var lines = content.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            endpoint = lines[0].Trim();
            apiKey = lines[1].Trim();
            searchKey = lines[2].Trim();
            searchEndpoint = lines[3].Trim();
            indexName = lines[4].Trim();
            docIntelligenceEndpoint = lines.Length > 5 ? lines[5].Trim() : "";
            docIntelligenceKey = lines.Length > 6 ? lines[6].Trim() : "";
            var credential = new AzureKeyCredential(apiKey);
            var searchCredential = new AzureKeyCredential(searchKey);
            searchClient = new SearchClient(new Uri(searchEndpoint), indexName, searchCredential);
            var azureClient = new AzureOpenAIClient(new Uri(endpoint), credential);
            chatClient = azureClient.GetChatClient("gpt-4.1");
        }

        public async Task<string> AskQuestionAsync(string userQuestion)
        {
            // Step 1: Search your index
            var searchResults = await searchClient.SearchAsync<SearchDocument>(userQuestion, new SearchOptions { Size = 5 });

            // Step 2: Extract content from search results
            var contextBuilder = new StringBuilder();
            await foreach (var result in searchResults.Value.GetResultsAsync())
            {
                if (result.Document.TryGetValue("content", out var content))
                {
                    contextBuilder.AppendLine(content.ToString());
                }
            }

            string context = contextBuilder.ToString();

            // Step 3: Use Azure OpenAI with context
            var messages = new List<ChatMessage>
            {
                new SystemChatMessage("You are a helpful assistant. Use just the provided context to answer the question. Please do not use other data you might have just use the information that are in the prompt"),
                new UserChatMessage($"Context:\n{context}\n\nQuestion: {userQuestion}")
            };

            var options = new ChatCompletionOptions
            {
                Temperature = 0.7f,
                MaxOutputTokenCount = 800
            };

            try
            {
                ChatCompletion completion = await chatClient.CompleteChatAsync(messages, options);
                return completion.Content?[0]?.Text ?? "No answer returned.";
            }
            catch (Exception ex)
            {
                return $"An error occurred: {ex.Message}";
            }
        }

        public async Task<string> AskQuestionWithImageAsync(string userQuestion, string imagePath)
        {
            try
            {
                // Read image file
                byte[] imageBytes = await File.ReadAllBytesAsync(imagePath);
                string mimeType = GetMimeType(imagePath);

                // Step 1: Search your index if there's a text question
                string context = "";
                if (!string.IsNullOrWhiteSpace(userQuestion))
                {
                    var searchResults = await searchClient.SearchAsync<SearchDocument>(userQuestion, new SearchOptions { Size = 5 });
                    var contextBuilder = new StringBuilder();
                    await foreach (var result in searchResults.Value.GetResultsAsync())
                    {
                        if (result.Document.TryGetValue("content", out var content))
                        {
                            contextBuilder.AppendLine(content.ToString());
                        }
                    }
                    context = contextBuilder.ToString();
                }

                // Step 2: Create message with image
                var messageContentParts = new List<ChatMessageContentPart>();

                // Add text part
                string promptText = !string.IsNullOrWhiteSpace(context) 
                    ? $"Context:\n{context}\n\nQuestion: {userQuestion}\n\nPlease analyze the image and answer based on both the context and what you see in the image."
                    : !string.IsNullOrWhiteSpace(userQuestion) 
                        ? $"Question: {userQuestion}\n\nPlease analyze the image and answer the question."
                        : "Please analyze and describe what you see in this image.";

                messageContentParts.Add(ChatMessageContentPart.CreateTextPart(promptText));

                // Add image part
                messageContentParts.Add(ChatMessageContentPart.CreateImagePart(
                    BinaryData.FromBytes(imageBytes), mimeType));

                var messages = new List<ChatMessage>
                {
                    new SystemChatMessage("You are a helpful assistant that can analyze images and answer questions. Use the provided context if available, and describe what you see in the image."),
                    new UserChatMessage(messageContentParts)
                };

                var options = new ChatCompletionOptions
                {
                    Temperature = 0.7f,
                    MaxOutputTokenCount = 800
                };

                ChatCompletion completion = await chatClient.CompleteChatAsync(messages, options);
                return completion.Content?[0]?.Text ?? "No answer returned.";
            }
            catch (Exception ex)
            {
                return $"An error occurred while processing the image: {ex.Message}";
            }
        }

        private string GetMimeType(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLowerInvariant();
            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".webp" => "image/webp",
                _ => "image/jpeg" // Default fallback
            };
        }
    }
}