#include <iostream>
#include <fstream>
#include <sstream>
#include <string>
#include <vector>
#include <curl/curl.h>
#include <nlohmann/json.hpp>

using json = nlohmann::json;

// Helper function to read file lines
std::vector<std::string> readLines(const std::string& path) {
    std::ifstream file(path);
    std::vector<std::string> lines;
    std::string line;
    while (getline(file, line)) {
        lines.push_back(line);
    }
    return lines;
}

// Helper function to handle CURL response
size_t WriteCallback(void* contents, size_t size, size_t nmemb, std::string* output) {
    size_t totalSize = size * nmemb;
    output->append((char*)contents, totalSize);
    return totalSize;
}

// Function to perform HTTP POST
std::string httpPost(const std::string& url, const std::string& apiKey, const json& payload) {
    CURL* curl = curl_easy_init();
    std::string response;

    if (curl) {
        struct curl_slist* headers = nullptr;
        headers = curl_slist_append(headers, ("api-key: " + apiKey).c_str());
        headers = curl_slist_append(headers, "Content-Type: application/json");

        std::string payloadStr = payload.dump();

        curl_easy_setopt(curl, CURLOPT_URL, url.c_str());
        curl_easy_setopt(curl, CURLOPT_POSTFIELDS, payloadStr.c_str());
        curl_easy_setopt(curl, CURLOPT_HTTPHEADER, headers);
        curl_easy_setopt(curl, CURLOPT_WRITEFUNCTION, WriteCallback);
        curl_easy_setopt(curl, CURLOPT_WRITEDATA, &response);

        curl_easy_perform(curl);
        curl_easy_cleanup(curl);
    }

    return response;
}

int main() {
    auto lines = readLines("C:\\Majidi\\Testcode\\Py\\AiRAG\\Appkey.txt");
    std::string openaiEndpoint = lines[0];
    std::string openaiKey = lines[1];
    std::string searchKey = lines[2];
    std::string searchEndpoint = lines[3];
    std::string indexName = lines[4];

    std::string question;
    std::cout << "Ask a question (type 'exit' to quit):\n";

    while (true) {
        std::cout << "\n> ";
        std::getline(std::cin, question);
        if (question == "exit") break;

        // Step 1: Search Azure Cognitive Search
        std::string searchUrl = searchEndpoint + "/indexes/" + indexName + "/docs/search?api-version=2021-04-30-Preview";
        json searchPayload = {
            {"search", question},
            {"top", 5}
        };

        std::string searchResponse = httpPost(searchUrl, searchKey, searchPayload);
        json searchJson = json::parse(searchResponse);

        std::stringstream context;
        for (const auto& doc : searchJson["value"]) {
            if (doc.contains("content")) {
                context << doc["content"].get<std::string>() << "\n";
            }
        }

        // Step 2: Call Azure OpenAI
        std::string chatUrl = openaiEndpoint + "/openai/deployments/gpt-4.1/chat/completions?api-version=2024-02-15-preview";
        json chatPayload = {
            {"messages", {
                {{"role", "system"}, {"content", "You are a helpful assistant. Use just the provided context to answer the question. Please do not use other data."}},
                {{"role", "user"}, {"content", "Context:\n" + context.str() + "\n\nQuestion: " + question}}
            }},
            {"temperature", 0.7},
            {"max_tokens", 800}
        };

        std::string chatResponse = httpPost(chatUrl, openaiKey, chatPayload);
        json chatJson = json::parse(chatResponse);

        std::cout << "\nAnswer:\n" << chatJson["choices"][0]["message"]["content"].get<std::string>() << "\n";
    }

    std::cout << "Goodbye!\n";
    return 0;
}
