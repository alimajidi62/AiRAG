# Azure AI RAG Demo

This repository demonstrates three different ways to connect to Azure AI services and implement Retrieval-Augmented Generation (RAG) using Azure Cognitive Search. Code examples are provided in **Python**, **C#**, and **C++**.

---

## 🚀 Overview

**Retrieval-Augmented Generation (RAG)** combines the power of large language models with external data sources. In this repo, Azure Cognitive Search is used to search specific documents and provide relevant context to the AI model for more accurate and grounded answers.

---

## 📁 Structure

- **Python:** [`test.py`](test.py)  
  Uses the `openai` and `azure.identity` libraries to connect to Azure OpenAI and Azure Cognitive Search. Reads configuration from [`Appkey.txt`](Appkey.txt) and demonstrates how to send a chat completion request with data augmentation from Azure Search.

- **C#:** [`AiTest/Program.cs`](AiTest/Program.cs)  
  Uses Azure SDKs (`Azure.AI.OpenAI`, `Azure.Search.Documents`) to perform a search and send the results as context to the OpenAI model.

- **C++:** [`Cpp/AiTest/AiTest.cpp`](Cpp/AiTest/AiTest.cpp)  
  Uses `libcurl` for HTTP requests and `nlohmann/json` for JSON handling. Reads keys from [`Appkey.txt`](Appkey.txt), queries Azure Cognitive Search, and sends the context to Azure OpenAI.

---

## ⚙️ How It Works

1. **Read API Keys and Endpoints**  
   All implementations read configuration from [`Appkey.txt`](Appkey.txt):(please use your Api key)
   - Azure OpenAI endpoint and key
   - Azure Search endpoint and key
   - Index name

2. **Search Documents**  
   The code queries Azure Cognitive Search for relevant documents based on the user's question.

3. **Augment AI Prompt**  
   The search results are used as context in the prompt sent to Azure OpenAI, improving the relevance and accuracy of the AI's response.

4. **Get AI Response**  
   The AI model generates an answer using only the provided context.

---

## 🛠 Requirements

- Azure OpenAI resource
- Azure Cognitive Search resource with an indexed document (e.g., [`world_war2.txt`](world_war2.txt))
- API keys and endpoints in [`Appkey.txt`](Appkey.txt)
- **Python:** `openai`, `azure-identity`
- **C#:** Azure SDK packages (see [`AiTest.csproj`](AiTest/AiTest.csproj))
- **C++:** `libcurl`, `nlohmann/json`

---

## ▶️ Usage

1. Fill in [`Appkey.txt`](Appkey.txt) with your Azure endpoints and keys.
2. Run the desired implementation:
   - **Python:**  
     ```sh
     python test.py
     ```
   - **C#:**  
     Open [`AiTest.sln`](AiTest/AiTest.sln) in Visual Studio and run.
   - **C++:**  
     Open [`Cpp/AiTest/AiTest.sln`](Cpp/AiTest/AiTest.sln) in Visual Studio and run.

---

## ⚠️ Notes

- This repo is for demonstration purposes and does not include production-level error handling or security.
- Make sure your Azure Search index contains the documents you want to query.

---
