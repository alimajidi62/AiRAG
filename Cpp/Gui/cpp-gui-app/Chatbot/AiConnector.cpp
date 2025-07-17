#include "AiConnector.h"
#include <QCoreApplication>
#include <QDir>
#include <fstream>
#include <sstream>
#include <vector>
#include <curl/curl.h>
#include <nlohmann/json.hpp>
// some test to see the git working correctly
using json = nlohmann::json;

namespace {
std::vector<std::string> readLines(const std::string& path) {
    std::ifstream file(path);
    std::vector<std::string> lines;
    std::string line;
    while (getline(file, line)) {
        lines.push_back(line);
    }
    return lines;
}

size_t WriteCallback(void* contents, size_t size, size_t nmemb, std::string* output) {
    size_t totalSize = size * nmemb;
    output->append((char*)contents, totalSize);
    return totalSize;
}

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
}

AiConnector::AiConnector(QObject* parent) : QObject(parent) {}

void AiConnector::askQuestion(const QString& question) {
    QString ddd=QCoreApplication::applicationDirPath();
    // Read keys and endpoints
    auto lines = readLines("../../../../../../Appkey.txt");
    if (lines.size() < 5) {
        setAnswer("Error: Appkey.txt is missing or incomplete.");
        return;
    }
    std::string openaiEndpoint = lines[0];
    std::string openaiKey = lines[1];
    std::string searchKey = lines[2];
    std::string searchEndpoint = lines[3];
    std::string indexName = lines[4];

    // Step 1: Search Azure Cognitive Search
    std::string searchUrl = searchEndpoint + "/indexes/" + indexName + "/docs/search?api-version=2021-04-30-Preview";
    json searchPayload = {
        {"search", question.toStdString()},
        {"top", 5}
    };

    std::string searchResponse = httpPost(searchUrl, searchKey, searchPayload);
    json searchJson;
    try {
        searchJson = json::parse(searchResponse);
    } catch (...) {
        setAnswer("Error: Failed to parse search response.");
        return;
    }

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
                         {{"role", "user"}, {"content", "Context:\n" + context.str() + "\n\nQuestion: " + question.toStdString()}}
                     }},
        {"temperature", 0.7},
        {"max_tokens", 800}
    };

    std::string chatResponse = httpPost(chatUrl, openaiKey, chatPayload);
    json chatJson;
    try {
        chatJson = json::parse(chatResponse);
        setAnswer(QString::fromStdString(chatJson["choices"][0]["message"]["content"].get<std::string>()));
    } catch (...) {
        setAnswer("Error: Failed to parse chat response.");
    }
}

void AiConnector::setAnswer(const QString& ans) {
    if (m_answer != ans) {
        m_answer = ans;
        emit answerChanged();
    }
}
