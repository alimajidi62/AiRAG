using Azure;
using Azure.AI.OpenAI;
using OpenAI.Chat;
using System.Text.Json;

async Task RunAsync()
{
    // Retrieve the OpenAI endpoint from environment variables

    string[] lines = File.ReadAllLines(@"C:\Majidi\Testcode\Py\AiRAG\Appkey.txt");
    string endpoint = lines[0].Trim();
    string apiKey = lines[1].Trim();
    string searchKey = lines[2].Trim();
    string searchEndpoint = lines[3].Trim();
    string indexName = lines[4].Trim();

    var credential = new AzureKeyCredential(apiKey);

    // Initialize the AzureOpenAIClient
    var azureClient = new AzureOpenAIClient(new Uri(endpoint), credential);

    // Initialize the ChatClient with the specified deployment name
    ChatClient chatClient = azureClient.GetChatClient("gpt-4.1");

    // Create a list of chat messages
    var messages = new List<ChatMessage>
    {
        new SystemChatMessage(@"You are an AI assistant that helps people find information."),
    };

    // Create chat completion options
    var options = new ChatCompletionOptions
    {
        Temperature = (float)1,
        MaxOutputTokenCount = 800,

        TopP = (float)1,
        FrequencyPenalty = (float)0,
        PresencePenalty = (float)0
    };

    try
    {
        // Create the chat completion request
        ChatCompletion completion = await chatClient.CompleteChatAsync(messages, options);

        // Print the response
        if (completion != null)
        {
            Console.WriteLine(JsonSerializer.Serialize(completion, new JsonSerializerOptions() { WriteIndented = true }));
        }
        else
        {
            Console.WriteLine("No response received.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred: {ex.Message}");
    }
}

await RunAsync();