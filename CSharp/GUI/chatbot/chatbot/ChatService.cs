using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Azure;
using Azure.AI.OpenAI;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using OpenAI.Chat;
using OpenAI.Images;
using System.Net.Http;
using BinaryData = System.BinaryData;

namespace chatbot;

public class ChatService
{
    private readonly string apiKey;
    private readonly ChatClient chatClient;
    private readonly ImageClient imageClient;
    private readonly string docIntelligenceEndpoint;
    private readonly string docIntelligenceKey;
    private readonly string endpoint;
    private readonly string indexName;

    private readonly SearchClient searchClient;
    private readonly string searchEndpoint;
    private readonly string searchKey;

    public ChatService()
    {
        var inputPath = "Appkey.bin";
        var seed = 12345;
        var shuffled = File.ReadAllBytes(inputPath);
        // Generate same shuffle order
        var indices = Enumerable.Range(0, shuffled.Length).ToArray();
        var rng = new Random(seed);
        indices = indices.OrderBy(_ => rng.Next()).ToArray();
        // Reverse shuffle
        var originalBytes = new byte[shuffled.Length];
        for (var i = 0; i < shuffled.Length; i++)
            originalBytes[indices[i]] = shuffled[i];

        var content = Encoding.UTF8.GetString(originalBytes);
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
        imageClient = azureClient.GetImageClient("dall-e-3"); // Add DALL-E 3 client
    }

    public async Task<string> AskQuestionAsync(string userQuestion)
    {
        // Step 1: Search your index
        var searchResults =
            await searchClient.SearchAsync<SearchDocument>(userQuestion, new SearchOptions { Size = 5 });

        // Step 2: Extract content from search results
        var contextBuilder = new StringBuilder();
        await foreach (var result in searchResults.Value.GetResultsAsync())
            if (result.Document.TryGetValue("content", out var content))
                contextBuilder.AppendLine(content.ToString());

        var context = contextBuilder.ToString();

        // Step 3: Use Azure OpenAI with context
        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(
                "You are a helpful assistant. Use just the provided context to answer the question. Please do not use other data you might have just use the information that are in the prompt"),
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
            var imageBytes = await File.ReadAllBytesAsync(imagePath);
            var mimeType = GetMimeType(imagePath);

            // Step 1: Search your index if there's a text question
            var context = "";
            if (!string.IsNullOrWhiteSpace(userQuestion))
            {
                var searchResults =
                    await searchClient.SearchAsync<SearchDocument>(userQuestion, new SearchOptions { Size = 5 });
                var contextBuilder = new StringBuilder();
                await foreach (var result in searchResults.Value.GetResultsAsync())
                    if (result.Document.TryGetValue("content", out var content))
                        contextBuilder.AppendLine(content.ToString());

                context = contextBuilder.ToString();
            }

            // Step 2: Create message with image
            var messageContentParts = new List<ChatMessageContentPart>();

            // Add text part
            var promptText = !string.IsNullOrWhiteSpace(context)
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
                new SystemChatMessage(
                    "You are a helpful assistant that can analyze images and answer questions. Use the provided context if available, and describe what you see in the image."),
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

    public async Task<string> GenerateImageAsync(string prompt, string quality = "standard", string size = "1024x1024")
    {
        try
        {
            var imageGenerationOptions = new ImageGenerationOptions()
            {
                Quality = quality == "hd" ? GeneratedImageQuality.High : GeneratedImageQuality.Standard,
                Size = size switch
                {
                    "1792x1024" => GeneratedImageSize.W1792xH1024,
                    "1024x1792" => GeneratedImageSize.W1024xH1792,
                    _ => GeneratedImageSize.W1024xH1024
                },
                Style = GeneratedImageStyle.Natural,
                ResponseFormat = GeneratedImageFormat.Uri
            };

            var imageResult = await imageClient.GenerateImageAsync(prompt, imageGenerationOptions);
            return imageResult.Value.ImageUri.ToString();
        }
        catch (Exception ex)
        {
            return $"Error generating image: {ex.Message}";
        }
    }

    public async Task<string> DownloadAndSaveImageAsync(string imageUrl, string savePath)
    {
        try
        {
            using var httpClient = new HttpClient();
            var imageBytes = await httpClient.GetByteArrayAsync(imageUrl);
            
            // Ensure directory exists
            var directory = Path.GetDirectoryName(savePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            await File.WriteAllBytesAsync(savePath, imageBytes);
            return savePath;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to download and save image: {ex.Message}");
        }
    }

    public async Task<(bool IsImageRequest, string Response, string ImagePath)> ProcessMessageAsync(string userMessage)
    {
        // Check if the user is requesting an image generation
        if (IsImageGenerationRequest(userMessage))
        {
            try
            {
                // Extract the image description from the message
                var imagePrompt = ExtractImagePrompt(userMessage);
                
                // Generate the image
                var imageUrl = await GenerateImageAsync(imagePrompt);
                
                if (imageUrl.StartsWith("Error"))
                {
                    return (true, imageUrl, string.Empty);
                }
                
                // Create a local path to save the image
                var fileName = $"generated_image_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                var savePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ChatbotImages", fileName);
                
                // Download and save the image
                var localPath = await DownloadAndSaveImageAsync(imageUrl, savePath);
                
                return (true, $"I've generated an image based on your request: \"{imagePrompt}\"", localPath);
            }
            catch (Exception ex)
            {
                return (true, $"Sorry, I couldn't generate the image. Error: {ex.Message}", string.Empty);
            }
        }
        else
        {
            // Handle as regular text question
            var response = await AskQuestionAsync(userMessage);
            return (false, response, string.Empty);
        }
    }

    private bool IsImageGenerationRequest(string message)
    {
        var imageKeywords = new[]
        {
            "generate an image", "create an image", "draw", "make an image", 
            "show me an image", "picture of", "image of", "generate picture",
            "create picture", "draw me", "make a picture", "visualize",
            "give me an image", "provide an image", "can you show me"
        };

        var lowerMessage = message.ToLower();
        return imageKeywords.Any(keyword => lowerMessage.Contains(keyword));
    }

    private string ExtractImagePrompt(string message)
    {
        var lowerMessage = message.ToLower();
        
        // Common patterns to extract the actual image description
        var patterns = new[]
        {
            @"(?:generate an image of|create an image of|draw|make an image of|show me an image of|picture of|image of|give me an image of|provide an image of)\s*(.+)",
            @"(?:can you show me|can you create|can you generate|can you draw|can you make)\s*(?:an image of|a picture of|a drawing of)?\s*(.+)",
            @"(?:i want to see|i want|i need)\s*(?:an image of|a picture of)?\s*(.+)",
            @"(?:visualize|illustrate)\s*(.+)"
        };

        foreach (var pattern in patterns)
        {
            var match = Regex.Match(lowerMessage, pattern, RegexOptions.IgnoreCase);
            if (match.Success && match.Groups.Count > 1)
            {
                return match.Groups[1].Value.Trim();
            }
        }

        // If no pattern matches, return the original message
        return message;
    }

    private string GetMimeType(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
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

    public async Task<(bool Success, string Message)> AddDocumentFromUrlAsync(string blobPdfUrl, IProgress<string> progress = null)
    {
        try
        {
            if (string.IsNullOrEmpty(docIntelligenceEndpoint) || string.IsNullOrEmpty(docIntelligenceKey))
            {
                return (false, "Document Intelligence service is not configured.");
            }

            progress?.Report("Starting document analysis...");

            // Extract PDF content using Document Intelligence
            var docClient = new DocumentAnalysisClient(new Uri(docIntelligenceEndpoint), new AzureKeyCredential(docIntelligenceKey));
            var operation = await docClient.AnalyzeDocumentFromUriAsync(WaitUntil.Completed, "prebuilt-document", new Uri(blobPdfUrl));
            var result = operation.Value;

            progress?.Report("Document analyzed, extracting text...");

            // Extract all text from the PDF
            var allText = new StringBuilder();
            allText.AppendLine(result.Content);
            string extractedText = allText.ToString();

            progress?.Report("Creating search index...");

            // Create or update the Azure Cognitive Search index if needed
            var searchCredential = new AzureKeyCredential(searchKey);
            var indexClient = new SearchIndexClient(new Uri(searchEndpoint), searchCredential);
            
            try
            {
                await indexClient.GetIndexAsync(indexName);
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                // Index doesn't exist, create it
                var definition = new SearchIndex(indexName)
                {
                    Fields =
                    {
                        new SimpleField("id", SearchFieldDataType.String) { IsKey = true, IsFilterable = true },
                        new SearchableField("content") { IsSortable = false, IsFilterable = false },
                        new SimpleField("documentName", SearchFieldDataType.String) { IsFilterable = true, IsFacetable = true },
                        new SimpleField("uploadDate", SearchFieldDataType.DateTimeOffset) { IsFilterable = true, IsSortable = true }
                    }
                };
                await indexClient.CreateOrUpdateIndexAsync(definition);
            }

            progress?.Report("Uploading document to search index...");

            // Upload the extracted text to Azure Cognitive Search
            var documentName = ExtractDocumentNameFromUrl(blobPdfUrl);
            var doc = new
            {
                id = Guid.NewGuid().ToString(),
                content = extractedText,
                documentName = documentName,
                uploadDate = DateTimeOffset.UtcNow
            };

            var uploadResult = await searchClient.UploadDocumentsAsync(new[] { doc });
            var firstResult = uploadResult.Value.Results.FirstOrDefault();
            
            if (firstResult?.Status >= 200 && firstResult?.Status < 300)
            {
                progress?.Report("Document uploaded successfully!");
                return (true, $"Document '{documentName}' has been successfully processed and added to the knowledge base.");
            }
            else
            {
                return (false, $"Failed to upload document. Status: {firstResult?.Status}");
            }
        }
        catch (Exception ex)
        {
            return (false, $"Error processing document: {ex.Message}");
        }
    }

    public async Task<List<DocumentInfo>> GetUploadedDocumentsAsync()
    {
        try
        {
            var searchOptions = new SearchOptions
            {
                Select = { "id", "documentName", "uploadDate" },
                Size = 50,
                OrderBy = { "uploadDate desc" }
            };

            var searchResults = await searchClient.SearchAsync<SearchDocument>("*", searchOptions);
            var documents = new List<DocumentInfo>();

            await foreach (var result in searchResults.Value.GetResultsAsync())
            {
                if (result.Document.TryGetValue("id", out var id) &&
                    result.Document.TryGetValue("documentName", out var name) &&
                    result.Document.TryGetValue("uploadDate", out var uploadDate))
                {
                    documents.Add(new DocumentInfo
                    {
                        Id = id.ToString(),
                        Name = name.ToString(),
                        UploadDate = DateTime.Parse(uploadDate.ToString()).ToString("yyyy-MM-dd HH:mm")
                    });
                }
            }

            return documents;
        }
        catch (Exception ex)
        {
            return new List<DocumentInfo>();
        }
    }

    public async Task<(bool Success, string Message)> RemoveDocumentAsync(string documentId)
    {
        try
        {
            var deleteResult = await searchClient.DeleteDocumentsAsync("id", new[] { documentId });
            var firstResult = deleteResult.Value.Results.FirstOrDefault();

            if (firstResult?.Status >= 200 && firstResult?.Status < 300)
            {
                return (true, "Document removed successfully.");
            }
            else
            {
                return (false, $"Failed to remove document. Status: {firstResult?.Status}");
            }
        }
        catch (Exception ex)
        {
            return (false, $"Error removing document: {ex.Message}");
        }
    }

    private string ExtractDocumentNameFromUrl(string url)
    {
        try
        {
            var uri = new Uri(url);
            var fileName = Path.GetFileName(uri.LocalPath);
            return string.IsNullOrEmpty(fileName) ? "Unknown Document" : fileName;
        }
        catch
        {
            return "Unknown Document";
        }
    }
}

public class DocumentInfo
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string UploadDate { get; set; }
}