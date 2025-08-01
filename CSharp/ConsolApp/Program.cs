using Azure;
using Azure.AI.OpenAI;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using OpenAI.Chat;
using System.Text;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;

async Task RunAsync()
{
    // Read all keys and endpoints from Appkey.txt
    string[] lines = File.ReadAllLines("../../../../../Appkey.txt");
    string endpoint = lines[0].Trim();
    string apiKey = lines[1].Trim();
    string searchKey = lines[2].Trim();
    string searchEndpoint = lines[3].Trim();
    string indexName = lines[4].Trim();
    string docIntelligenceEndpoint = lines.Length > 5 ? lines[5].Trim() : "";
    string docIntelligenceKey = lines.Length > 6 ? lines[6].Trim() : "";

    // Optional: Extract PDF content from Azure Blob using Document Intelligence and index it
    if (!string.IsNullOrEmpty(docIntelligenceEndpoint) && !string.IsNullOrEmpty(docIntelligenceKey))
    {
        Console.WriteLine("Do you want to extract and index a PDF from Azure Blob Storage? (y/n)");
        var answer = Console.ReadLine();
        if (answer?.Trim().ToLower() == "y")
        {
            Console.Write("Enter the Azure Blob PDF URL: ");
            string blobPdfUrl = Console.ReadLine()?.Trim();
            if (!string.IsNullOrEmpty(blobPdfUrl))
            {
                var docClient = new DocumentAnalysisClient(new Uri(docIntelligenceEndpoint), new AzureKeyCredential(docIntelligenceKey));
                var operation = await docClient.AnalyzeDocumentFromUriAsync(WaitUntil.Completed, "prebuilt-document", new Uri(blobPdfUrl));
                var result = operation.Value;

                // Extract all text from the PDF
                var allText = new StringBuilder();
                allText.AppendLine(result.Content);
                string extractedText = allText.ToString();

                // Create or update the Azure Cognitive Search index if needed
                var indexClient = new SearchIndexClient(new Uri(searchEndpoint), new AzureKeyCredential(searchKey));
                if (!indexClient.GetIndexNames().Contains(indexName))
                {
                    var definition = new SearchIndex(indexName)
                    {
                        Fields =
                        {
                            new SimpleField("id", SearchFieldDataType.String) { IsKey = true, IsFilterable = true },
                            new SearchableField("content") { IsSortable = false, IsFilterable = false }
                        }
                    };
                    indexClient.CreateOrUpdateIndex(definition);
                }

                // Upload the extracted text to Azure Cognitive Search
                var searchClientForUpload = new SearchClient(new Uri(searchEndpoint), indexName, new AzureKeyCredential(searchKey));
                var doc = new { id = Guid.NewGuid().ToString(), content = extractedText };
                try
                {
                    var uploadResult = await searchClientForUpload.UploadDocumentsAsync(new[] { doc });
                    Console.WriteLine("Upload status: " + uploadResult.Value.Results.FirstOrDefault()?.Status);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Upload error: " + ex.Message);
                }

                Console.WriteLine("PDF content extracted and indexed successfully.");
            }
        }
    }

    var credential = new AzureKeyCredential(apiKey);
    var searchCredential = new AzureKeyCredential(searchKey);

    var searchClient = new SearchClient(new Uri(searchEndpoint), indexName, searchCredential);
    var azureClient = new AzureOpenAIClient(new Uri(endpoint), credential);
    ChatClient chatClient = azureClient.GetChatClient("gpt-4.1");

    Console.WriteLine("Ask a question (type 'exit' to quit):");

    while (true)
    {
        Console.Write("\n> ");
        string userQuestion = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(userQuestion) || userQuestion.ToLower() == "exit")
            break;

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
            Console.WriteLine("\nAnswer:\n" + completion.Content?[0]?.Text);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }

    Console.WriteLine("Goodbye!");
}

await RunAsync();