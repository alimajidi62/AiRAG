<!--
SEO KEYWORDS: Azure AI, Retrieval-Augmented Generation, RAG, Cognitive Search, OpenAI, Document Intelligence, Form Recognizer, PDF OCR, Table Extraction, Image Analysis, AI Chatbot, Multi-Agent AI, Azure AI Studio, C#, Python, C++, QML, WPF, Chatbot UI, Document Analysis, Knowledge Base, Enterprise Search, AI Workflow, Microsoft Azure, Large Language Model, LLM, AI Demo, AI Example, AI Tutorial, AI Sample Code
-->

# Azure AI RAG Demo – Multi-Agent Chatbot, Document Analysis, and Cognitive Search

<!--
This repository demonstrates how to build an enterprise-ready AI chatbot using Azure AI, Retrieval-Augmented Generation (RAG), and Azure Cognitive Search. Features include multi-agent document analysis (image, table, and text extraction), PDF and image OCR, and a modern UI in C#, Python, C++, QML, and WPF. Ideal for developers, data scientists, and enterprises looking to implement advanced AI search and document understanding solutions.
-->

---

## 🚀 Overview

**Retrieval-Augmented Generation (RAG)** combines the power of large language models with external data sources. In this repo, Azure Cognitive Search is used to search specific documents and provide relevant context to the AI model for more accurate and grounded answers.  
**Keywords:** Azure AI, RAG, Cognitive Search, OpenAI, Document Intelligence, PDF OCR, Table Extraction, AI Chatbot, Multi-Agent, Enterprise Search.

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

---

## 📑 Using Azure AI Document Intelligence (Form Recognizer)

You can also use [Azure AI Document Intelligence (formerly Form Recognizer)](https://learn.microsoft.com/azure/ai-services/document-intelligence/) to extract structured data from documents (PDFs, images, etc.) before indexing them in Azure Cognitive Search.

### How to Use Document Intelligence with This Repo

1. **Create a Document Intelligence Resource**
   - Go to the [Azure Portal](https://portal.azure.com/).
   - Search for **Azure AI Document Intelligence** (or **Form Recognizer**) and create a new resource.
   - After deployment, open your resource and find the **Endpoint** and **Key** under the "Keys and Endpoint" section.

2. **Extract Data from Documents**
   - Use the [Azure.AI.FormRecognizer](https://learn.microsoft.com/dotnet/api/overview/azure/ai.formrecognizer-readme) SDK (C#) or [azure-ai-formrecognizer](https://pypi.org/project/azure-ai-formrecognizer/) (Python) to extract text and fields from your documents.
   - Example (Python):
     ```python
     from azure.ai.formrecognizer import DocumentAnalysisClient
     from azure.core.credentials import AzureKeyCredential

     endpoint = "<your-form-recognizer-endpoint>"
     key = "<your-form-recognizer-key>"
     document_path = "path/to/your/document.pdf"

     client = DocumentAnalysisClient(endpoint, AzureKeyCredential(key))
     with open(document_path, "rb") as f:
         poller = client.begin_analyze_document("prebuilt-document", document=f)
         result = poller.result()
         for page in result.pages:
             print("Page content:", page.content)
     ```
   - You can extract the content and save it as text or JSON for indexing.

3. **Index Extracted Data in Azure Cognitive Search**
   - After extracting the content, upload it to your Azure Cognitive Search index (as shown in the setup steps above).
   - This allows you to search and ground your AI responses in the content of scanned documents, PDFs, and more.

---

## 📚 Understanding the Azure Cognitive Search Index

An **index** in Azure Cognitive Search is similar to a database table: it defines the structure of your searchable data and stores the documents you want to search over. Each document in the index is like a row in a table, and each field is like a column.

### Why is the Index Important?

- The index determines what data you can search and retrieve.
- You must define the fields (such as `id` and `content`) before uploading documents.
- The `id` field is required and must be unique for each document. It acts as the primary key.

### How This Repo Uses the Index

- The repo expects an index with at least these fields:
  - `id` (string, key): A unique identifier for each document.
  - `content` (string): The full text extracted from your source (e.g., PDF, text file).
- When you upload data (from a PDF or text file), it is stored in the `content` field of the index.
- When you ask a question, the code searches the `content` field for relevant information.

### How to Create or Update the Index

- The code in this repo will automatically create the index if it does not exist, using the correct schema.
- If you want to create or manage the index manually:
  1. Go to your Azure Portal → Cognitive Search → Indexes.
  2. Click **+ Add Index**.
  3. Add a field named `id` (type: String, Key: Yes).
  4. Add a field named `content` (type: String, Searchable: Yes).
  5. Save the index.

**Note:**  
If you change the fields in your index, you may need to delete and recreate the index for the changes to take effect.

### Example Index Definition (C#)

```csharp
var definition = new SearchIndex(indexName)
{
    Fields =
    {
        new SimpleField("id", SearchFieldDataType.String) { IsKey = true, IsFilterable = true },
        new SearchableField("content") { IsSortable = false, IsFilterable = false }
    }
};
indexClient.CreateOrUpdateIndex(definition);
```

---

**In summary:**  
The index is the core of your search experience. Make sure it has the right fields, and always include an `id` when uploading documents. All search and retrieval in this repo is performed against the `content` field of your index.

**Tip:**  
Document Intelligence is especially useful for digitizing and searching large collections of scanned files, forms, or complex documents.

---

## ⚠️ Notes

- This repo is for demonstration purposes and does not include production-level error handling or security.
- Make sure your Azure Search index contains the documents you want to query.

<!-- SEO: Example, Table, and Chatbot UI for Azure AI, RAG, Cognitive Search, and Document Intelligence -->

---

## 📊 Example: Estimation of Jewish Losses in World War II by Country

When you ask a question such as:

> "What are the estimated Jewish losses in World War II based on country?"

The system retrieves relevant information from your indexed documents and provides a grounded answer. For example, the response may include a table or summary like:

| Country         | Estimated Jewish Losses |
|-----------------|------------------------|
| Poland          | ~3,000,000             |
| Soviet Union    | ~1,000,000             |
| Hungary         | ~569,000               |
| Romania         | ~287,000               |
| Germany         | ~165,000               |
| Czechoslovakia  | ~277,000               |
| Lithuania       | ~143,000               |
| Netherlands     | ~102,000               |
| France          | ~77,000                |
| Other countries | (various)              |

*These numbers are based on historical estimates and may vary by source.*

This demonstrates how Retrieval-Augmented Generation (RAG) can provide detailed, document-grounded answers to complex historical questions using Azure Cognitive Search and Azure OpenAI.

---

## 🖥️ Qt UI: Ask Questions with a Graphical Interface

This repository also includes a **Qt Quick (QML) graphical user interface** for interacting with Azure AI and Cognitive Search, similar in functionality to the C++ console project.

### Features

- **Modern Chatbot UI:**  
  The Qt app provides a clean, user-friendly interface for asking questions and viewing AI-generated answers.
- **Azure Integration:**  
  Connects to Azure OpenAI and Azure Cognitive Search using the same API keys and endpoints as the other implementations.
- **Real-Time Responses:**  
  Enter your question and receive answers grounded in your indexed documents.

### How to Run the Qt UI

1. **Configure API Keys:**  
   Ensure your [`Appkey.txt`](Appkey.txt) file is filled out as described above.
2. **Open the Project:**  
   Open the Qt project (e.g., `Cpp/Gui/cpp-gui-app/Chatbot/`) in [Qt Creator](https://www.qt.io/product/development-tools) or Visual Studio with Qt plugin.
3. **Build and Run:**  
   Build and launch the application.  
   You’ll see a window titled **"Azure AI Chatbot"** where you can type your question and view the AI’s answer.

### Example Screenshot

![Qt Chatbot UI Example](docs/qt_chatbot_example.png) <!-- Add a screenshot if available -->

### File Locations

- **QML UI:** [`Cpp/Gui/cpp-gui-app/Chatbot/Main.qml`](Cpp/Gui/cpp-gui-app/Chatbot/Main.qml)
- **Backend Integration:** C++/QML bridge code connects the UI to Azure services.

### 🖼️ Qt Chatbot UI Overview

The screenshot above shows the **Qt Quick (QML) graphical user interface** for the Azure AI Chatbot. The UI is designed for a modern, intuitive chat experience.

---

## 🤖 Using Azure AI Agents for Multi-Task Document Analysis

<!--
Learn how to use Azure AI agents for parallel document processing, including image analysis, table extraction, and text summarization. This approach enables advanced document understanding and enterprise search solutions.
-->

Azure AI now supports the use of **agents**—specialized components that can each handle a specific type of task within your AI workflow. This enables you to process and explore documents in parallel, assigning each agent a unique role, such as image analysis or table extraction.

### How to Use Agents in Azure AI

1. **Define Specialized Agents**
   - **Image Agent:** Detects and analyzes images within documents (e.g., extracting text from images or identifying objects).
   - **Table Agent:** Extracts and interprets tables, converting them into structured data for further analysis.
   - **Text Agent:** Handles general text extraction, summarization, or question answering.

2. **Configure Your Workflow**
   - Orchestrate these agents so that when a document is uploaded, each agent processes the relevant part:
     - The image agent scans for embedded images and runs OCR or image classification.
     - The table agent locates tables and parses their content.
     - The text agent summarizes or answers questions based on the extracted data.

3. **Parallel and Specialized Processing**
   - Agents can work in parallel, speeding up document analysis and enabling multi-modal understanding (text, tables, images) in a single workflow.
   - Each agent can be optimized or fine-tuned for its specific task, improving accuracy and efficiency.

4. **Integration with Azure AI Studio**
   - Use [Azure AI Studio](https://ai.azure.com/) to configure, deploy, and monitor your agents.
   - You can chain agents together or trigger them based on document content.

### Example Use Case

When a user uploads a PDF containing both scanned images and tables:
- The **image agent** extracts text from images (using OCR).
- The **table agent** converts tables into structured data.
- The **text agent** summarizes the document or answers user questions using all extracted content.

This agent-based approach enables more accurate and comprehensive document analysis, leveraging the strengths of each specialized AI component.

---

<!--
For more Azure AI, RAG, Cognitive Search, and Document Intelligence examples, see the official Microsoft documentation and this repository's code samples.
-->

#### Layout and Features

- **Left Panel (History & Actions):**
  - **History List:**  
    Displays previous questions asked by the user, with the newest at the top. Each entry is clickable, allowing you to reload a previous question and its answer.
  - **Clear History Button:**  
    Removes all previous questions and answers from the history list.
  - **Open Document Button:**  
    Lets you upload a document (such as a PDF or text file) to be indexed and used for future AI responses.

- **Right Panel (Chat Area):**
  - **Header:**  
    Shows the application title, "Azure AI Chatbot," with a chat icon.
  - **Answer Display:**  
    The main area displays the AI’s response, including formatted tables and summaries based on your question and the indexed documents.
  - **Question Input Row:**  
    At the bottom, you can type your question in the text field.  
    The **Ask** button is placed to the right of the input field for quick access.

#### How It Works

1. **Ask a Question:**  
   Type your question in the input field and click **Ask**. The AI will search the indexed documents and provide a grounded answer.
2. **View History:**  
   All previous questions are saved in the left panel. Click any entry to view its answer again.
3. **Upload Documents:**  
   Use the **Open Document** button to add new documents to the search index, expanding the knowledge base for future queries.
4. **Clear History:**  
   Use the **Clear History** button to reset the chat history.

#### Styling

- The interface uses soft colors and rounded corners for a friendly, modern look.
- Buttons are styled with color changes on hover for better interactivity.
- The chat area supports rich text and tables for detailed answers.

#### QML

<img width="1001" height="992" alt="image" src="https://github.com/user-attachments/assets/9e0cf75f-45b1-42ce-b87b-771906752700" />

#### WPF

<img width="787" height="851" alt="image" src="https://github.com/user-attachments/assets/d4c67638-d305-4a1b-9e1b-de8c9da3e4dd" />

---

<!--
SEO: Azure AI, RAG, Cognitive Search, Document Intelligence, AI Chatbot, Multi-Agent, PDF OCR, Table Extraction, Enterprise Search, OpenAI, Microsoft Azure, AI Example, AI Tutorial, AI Sample Code
-->