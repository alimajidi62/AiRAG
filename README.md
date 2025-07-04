# Azure AI RAG Demo

This repository demonstrates three different ways to connect to Azure AI services and implement Retrieval-Augmented Generation (RAG) using Azure Cognitive Search. Code examples are provided in **Python**, **C#**, and **C++**.

---

## 🚀 Overview

**Retrieval-Augmented Generation (RAG)** combines the power of large language models with external data sources. In this repo, Azure Cognitive Search is used to search specific documents and provide relevant context to the AI model for more accurate and grounded answers.

---

## 📁 Structure

- **Python:** [`test.py`](test.py)  
  Uses the `openai` and `azure.identity` libraries to connect to Azure OpenAI and Azure Cognitive Search. Reads configuration from [`Appkey.txt`](Appkey.txt) and demonstrates how to send a chat completion request with data augmentation from Azure Search.

- **C#:** [`CSharp/Program.cs`](CSharp/Program.cs)  
  Uses Azure SDKs (`Azure.AI.OpenAI`, `Azure.Search.Documents`) to perform a search and send the results as context to the OpenAI model.

  **Required NuGet Packages:**
  - `Azure.AI.OpenAI`
  - `Azure.Search.Documents`
  - `Azure.Core`
  - `OpenAI` (for `OpenAI.Chat` if used)

  You can install these packages using the NuGet Package Manager in Visual Studio or with the following commands in the Package Manager Console:

  ```powershell
  Install-Package Azure.AI.OpenAI
  Install-Package Azure.Search.Documents
  Install-Package Azure.Core
  Install-Package OpenAI
  ```
- **C++:** [`Cpp/AiTest/AiTest.cpp`](Cpp/AiTest/AiTest.cpp)  
  Uses `libcurl` for HTTP requests and `nlohmann/json` for JSON handling. Reads keys from [`Appkey.txt`](Appkey.txt), queries Azure Cognitive Search, and sends the context to Azure OpenAI.

  ### 🛠️ C++ Dependencies

  This project uses [vcpkg](https://github.com/microsoft/vcpkg) to manage C++ dependencies.

  **Install vcpkg:**
  ```sh
  git clone https://github.com/microsoft/vcpkg.git
  cd vcpkg
  .\bootstrap-vcpkg.bat   # On Windows PowerShell
  ```

  **Install dependencies:**
  ```sh
  .\vcpkg.exe install curl nlohmann-json
  ```

  **Integrate vcpkg with Visual Studio (optional but recommended):**
  ```sh
  .\vcpkg.exe integrate install
  ```

  After installing, open your Visual Studio solution and ensure it uses the vcpkg toolchain or the installed packages.
---

## 📄 About `world_war2.txt`

The file [`world_war2.txt`](world_war2.txt) is an example document used for indexing in Azure Cognitive Search. It contains information and facts about World War II.  
This file demonstrates how the Retrieval-Augmented Generation (RAG) approach can ground AI responses in specific, trusted content. You can replace or extend this file with your own documents to customize the knowledge base for your application.

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
- **C#:** Azure SDK packages (see [`AiTest.csproj`](CSharp/AiTest.csproj))
- **C++:** `libcurl`, `nlohmann/json`

---
## 🆘 How to Set Up Azure AI and Azure Cognitive Search

To use this repo, you need to create resources in your Azure portal and obtain the required API keys and endpoints.

### 1. Create Azure OpenAI (AI Foundry) Resource

- Go to the [Azure Portal](https://portal.azure.com/).
- Search for **Azure OpenAI** and create a new resource.
- After deployment, open your resource.
- In the left menu, select **Keys and Endpoint**.
  - **Endpoint:** Copy the endpoint URL.
  - **Key:** Copy one of the API keys.

### 2. Create Azure Cognitive Search Resource

- In the Azure Portal, search for **Azure Cognitive Search** and create a new resource.
- After deployment, open your search service.
- In the left menu, select **Keys**.
  - **Admin Key:** Copy one of the admin keys.
- In the left menu, select **Overview**.
  - **URL:** This is your search endpoint.

### 3. Create and Index Your Data

- In your Azure Cognitive Search resource, create an **Index** (e.g., `worldwar2-index`).
- Upload your documents (such as `world_war2.txt`) using the Azure Portal, Azure SDK, or REST API.

### 4. Fill in `Appkey.txt`

Your `Appkey.txt` should look like this (one value per line):

```
<Azure OpenAI Endpoint>
<Azure OpenAI API Key>
<Azure Cognitive Search Admin Key>
<Azure Cognitive Search Endpoint>
<Azure Search Index Name>
```

Example:
```
https://YOUR_OPENAI_RESOURCE.openai.azure.com/
YOUR_OPENAI_API_KEY
YOUR_SEARCH_ADMIN_KEY
https://YOUR_SEARCH_RESOURCE.search.windows.net/
worldwar2-index
```

---

**Tip:**  
If you need more help, see the official documentation:  
- [Azure OpenAI Service documentation](https://learn.microsoft.com/azure/ai-services/openai/)
- [Azure Cognitive Search documentation](https://learn.microsoft.com/azure/search/)

---
## ▶️ Usage

1. Fill in [`Appkey.txt`](Appkey.txt) with your Azure endpoints and keys.
2. Run the desired implementation:
   - **Python:**  
     ```sh
     python test.py
     ```
   - **C#:**  
     Open [`AiTest.sln`](CSharp/AiTest.sln) in Visual Studio and run.
   - **C++:**  
     Open [`Cpp/AiTest/AiTest.sln`](Cpp/AiTest/AiTest.sln) in Visual Studio and run.

---

## ⚠️ Notes

- This repo is for demonstration purposes and does not include production-level error handling or security.
- Make sure your Azure Search index contains the documents you want to query.

---
