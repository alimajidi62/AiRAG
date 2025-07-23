# AI Chatbot Python GUI

This Python application provides a GUI interface for an AI chatbot that uses Azure OpenAI and Azure Cognitive Search for Retrieval Augmented Generation (RAG).

## Features

- **Question & Answer Interface**: Type questions and get AI-powered responses
- **History Panel**: View and interact with previous questions and answers
- **Toggle History**: Show/hide the history panel to maximize chat area
- **Loading Indicator**: Visual feedback while processing requests
- **Persistent History**: Chat history is saved and loaded automatically
- **RAG Integration**: Uses Azure Search to find relevant context before generating answers

## Files

- `chatbot_gui.py` - Main GUI application using tkinter
- `chat_service.py` - Service class that handles Azure OpenAI and Search integration
- `chat_history.json` - Automatically created file to store conversation history
- `Appkey.txt` - Configuration file with Azure credentials (required)

## Requirements

Install the required packages:

```bash
pip install azure-search-documents azure-identity openai
```

## Configuration

Create an `Appkey.txt` file in the same directory with the following format:

```
your_azure_openai_endpoint
your_azure_openai_api_key
your_azure_search_key
your_azure_search_endpoint
your_azure_search_index_name
```

## Usage

Run the application:

```bash
python chatbot_gui.py
```

### Features:
1. **Ask Questions**: Type your question in the input field and click "Ask" or press Enter
2. **View History**: Previous questions appear in the left panel
3. **Load History**: Click any history item to reload that question and answer
4. **Toggle History**: Use the "Hide/Show History" button to maximize the chat area
5. **Clear History**: Remove all previous conversations

## Architecture

The application follows a similar structure to the C# WPF version:

- **ChatService**: Handles Azure OpenAI and Search integration
- **GUI Components**: 
  - Main chat area with question input and answer display
  - Collapsible history panel
  - Loading indicator during processing
- **History Management**: Persistent storage of conversations

## Error Handling

- Connection errors to Azure services are displayed in the chat area
- Invalid input is handled with appropriate warnings
- Loading states prevent multiple simultaneous requests

## Threading

The application uses threading to prevent GUI freezing during API calls:
- Main thread handles UI updates
- Background thread processes Azure OpenAI requests
- Thread-safe communication between threads
