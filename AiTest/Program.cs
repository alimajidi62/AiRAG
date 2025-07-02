////using System;
////using System.IO;
////using System.Threading.Tasks;
////using Azure;
////using Azure.AI.OpenAI;
////using OpenAI.Chat;
////using OpenAI;

////class Program
////{
////    static async Task Main(string[] args)
////    {
////        string[] lines = File.ReadAllLines("Appkey.txt");
////        string endpoint = lines[0].Trim();
////        string apiKey = lines[1].Trim();
////        string searchKey = lines[2].Trim();
////        string searchEndpoint = lines[3].Trim();
////        string indexName = lines[4].Trim();

////        var client = new OpenAIClient(new Uri(endpoint), new AzureKeyCredential(apiKey));

////        var options = new ChatCompletionsOptions()
////        {
////            DeploymentName = "gpt-4.1",
////            MaxTokens = 800,
////            Temperature = 1,
////            TopP = 1,
////            FrequencyPenalty = 0,
////            PresencePenalty = 0,
////        };
////        options.Messages.Add(new ChatMessage(ChatRole.System, "how is Mahmoud Ahmadinejad in iran."));

////        // For RAG, add the Azure Cognitive Search extension (if supported in your SDK version)
////        options.Tools.Add(
////            new AzureCognitiveSearchChatExtensionConfiguration(
////                new Uri(searchEndpoint),
////                indexName,
////                searchKey
////        )
////        );

////        Response<ChatCompletions> response = await client.GetChatCompletionsAsync(options);

////        Console.WriteLine(response.Value.Choices[0].Message.Content);
////    }
////}

//// Install the .NET library via NuGet: dotnet add package Azure.AI.OpenAI --prerelease
//using System;
//using System.Collections.Generic;
//using System.Threading.Tasks;
//using Azure;
//using Azure.AI.OpenAI;
//using Azure.Identity;
//using OpenAI.Chat;

//using static System.Environment;
//using System.Text.Json;

//async Task RunAsync()
//{
//    // Retrieve the OpenAI endpoint from environment variables
//    //var endpoint = GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? "https://searchgeneral.openai.azure.com/";
//    //if (string.IsNullOrEmpty(endpoint))
//    //{
//    //    Console.WriteLine("Please set the AZURE_OPENAI_ENDPOINT environment variable.");
//    //    return;
//    //}

//    // Use DefaultAzureCredential for Entra ID authentication
//    string endpoint = "https://searchgeneral.cognitiveservices.azure.com/openai/deployments/gpt-4.1/chat/completions?api-version=2025-01-01-preview";
//    string key = "8l24KMTFPWrBlCk16Ub5szJKtqXvg7sU1sGXKCpJKxCGJDcqyQmIJQQJ99BGAC5RqLJXJ3w3AAAAACOGhymE";
//    var credential = new AzureKeyCredential(key);

//    // Initialize the AzureOpenAIClient
//    var azureClient = new AzureOpenAIClient(new Uri(endpoint), credential);

//    // Initialize the ChatClient with the specified deployment name
//    ChatClient chatClient = azureClient.GetChatClient("gpt-4.1");

//    // Create a list of chat messages
//    var messages = new List<ChatMessage>
//    {
//        new SystemChatMessage(@"You are an AI assistant that helps people find information."),
//    };

//    // Create chat completion options
//    var options = new ChatCompletionOptions
//    {
//        Temperature = (float)1,
//        MaxOutputTokenCount = 800,

//        TopP = (float)1,
//        FrequencyPenalty = (float)0,
//        PresencePenalty = (float)0
//    };

//    try
//    {
//        // Create the chat completion request
//        ChatCompletion completion = await chatClient.CompleteChatAsync(messages, options);

//        // Print the response
//        if (completion != null)
//        {
//            Console.WriteLine(JsonSerializer.Serialize(completion, new JsonSerializerOptions() { WriteIndented = true }));
//        }
//        else
//        {
//            Console.WriteLine("No response received.");
//        }
//    }
//    catch (Exception ex)
//    {
//        Console.WriteLine($"An error occurred: {ex.Message}");
//    }
//}

//await RunAsync();

// Install the .NET library via NuGet: dotnet add package Azure.AI.OpenAI --prerelease
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure;
using Azure.AI.OpenAI;
using Azure.Identity;
using OpenAI.Chat;

using static System.Environment;
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