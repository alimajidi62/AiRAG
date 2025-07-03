using Azure;
using Azure.AI.OpenAI;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using OpenAI.Chat;
using System.Text.Json;

async Task RunAsync()
{
    string[] lines = File.ReadAllLines(@"C:\Majidi\Testcode\Py\AiRAG\Appkey.txt");
    string endpoint = lines[0].Trim();
    string apiKey = lines[1].Trim();
    string searchKey = lines[2].Trim();
    string searchEndpoint = lines[3].Trim();
    string indexName = lines[4].Trim();

    var credential = new AzureKeyCredential(apiKey);
    var searchCredential = new AzureKeyCredential(searchKey);

    // Step 1: Search your index
    var searchClient = new SearchClient(new Uri(searchEndpoint), indexName, searchCredential);
    //string userQuestion = "what is Case Blue in world war 2?";
    string userQuestion = "who is the winner of the world war 2?";
    var searchResults = await searchClient.SearchAsync<SearchDocument>(userQuestion, new SearchOptions { Size = 5 });

    // Step 2: Extract content from search results
    var contextBuilder = new System.Text.StringBuilder();
    await foreach (var result in searchResults.Value.GetResultsAsync())
    {
        if (result.Document.TryGetValue("content", out var content))
        {
            contextBuilder.AppendLine(content.ToString());
        }
    }

    string context = contextBuilder.ToString();

    // Step 3: Use Azure OpenAI with context
    var azureClient = new AzureOpenAIClient(new Uri(endpoint), credential);
    ChatClient chatClient = azureClient.GetChatClient("gpt-4.1");

    var messages = new List<ChatMessage>
    {
        new SystemChatMessage("You are a helpful assistant. Use just the  provided context to answer the question.please not use the other data you might have"),
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
        Console.WriteLine(completion.Content?[0]?.Text);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred: {ex.Message}");
    }
}

await RunAsync();
