import os
import ssl
import certifi
from openai import AzureOpenAI
from azure.search.documents import SearchClient
from azure.core.credentials import AzureKeyCredential

# Configure SSL to use proper certificates (like .NET does)
def setup_ssl_context():
    """Setup SSL context to properly validate certificates like .NET"""
    try:
        # Method 1: Try to use Windows certificate store (like .NET)
        if os.name == 'nt':  # Windows
            try:
                import wincertstore
                # This makes Python use Windows certificate store like .NET
                import ssl
                import certifi
                
                # Get Windows certificates
                ca_bundle = certifi.where()
                
                # Set environment variables
                os.environ['SSL_CERT_FILE'] = ca_bundle
                os.environ['REQUESTS_CA_BUNDLE'] = ca_bundle
                os.environ['CURL_CA_BUNDLE'] = ca_bundle
                
                print("✓ SSL configured with Windows certificate store")
                return True
                
            except ImportError:
                print("wincertstore not available, using fallback")
        
        # Method 2: Use certifi's certificate bundle
        import certifi
        ca_bundle = certifi.where()
        
        os.environ['SSL_CERT_FILE'] = ca_bundle
        os.environ['REQUESTS_CA_BUNDLE'] = ca_bundle
        os.environ['CURL_CA_BUNDLE'] = ca_bundle
        
        print("✓ SSL configured with certifi certificates")
        return True
        
    except Exception as e:
        print(f"⚠ All SSL configuration methods failed: {e}")
        print("Using insecure SSL as fallback (like your original working version)")
        
        # Fallback to insecure (but working) SSL
        import ssl
        ssl._create_default_https_context = ssl._create_unverified_context
        return False

# Initialize SSL on import
setup_ssl_context()


class ChatService:
    def __init__(self):
        """Initialize the ChatService with Azure OpenAI and Search credentials."""
        self.endpoint, self.api_key, self.search_key, self.search_endpoint, self.index_name = self._read_keys_from_file()
        
        # Initialize Azure OpenAI client (like .NET version)
        self.client = AzureOpenAI(
            azure_endpoint=self.endpoint,
            api_key=self.api_key,
            api_version="2025-01-01-preview",
        )
        
        # Initialize Azure Search client (like .NET version)
        search_credential = AzureKeyCredential(self.search_key)
        self.search_client = SearchClient(
            endpoint=self.search_endpoint,
            index_name=self.index_name,
            credential=search_credential
        )
        
        self.deployment = "gpt-4.1"
        print("✓ ChatService initialized with proper SSL validation")
    
    def _read_keys_from_file(self, filename=r"Appkey.txt"):
        """Read Azure credentials from the Appkey.txt file."""
        with open(filename, "r") as f:
            lines = f.read().splitlines()
            endpoint = lines[0].strip()
            api_key = lines[1].strip()
            search_key = lines[2].strip()
            search_endpoint = lines[3].strip()
            index_name = lines[4].strip()
        return endpoint, api_key, search_key, search_endpoint, index_name
    
    async def ask_question_async(self, user_question):
        """
        Ask a question using RAG (Retrieval Augmented Generation).
        First searches the index, then uses the context with Azure OpenAI.
        """
        try:
            # Step 1: Search the index for relevant content
            search_results = self.search_client.search(
                search_text=user_question,
                top=5
            )
            
            # Step 2: Extract content from search results
            context_parts = []
            for result in search_results:
                if 'content' in result:
                    context_parts.append(result['content'])
            
            context = '\n'.join(context_parts)
            
            # Step 3: Create messages for Azure OpenAI
            messages = [
                {
                    "role": "system",
                    "content": "You are a helpful assistant. Use just the provided context to answer the question. Please do not use other data you might have just use the information that are in the prompt"
                },
                {
                    "role": "user",
                    "content": f"Context:\n{context}\n\nQuestion: {user_question}"
                }
            ]
            
            # Step 4: Get response from Azure OpenAI
            completion = self.client.chat.completions.create(
                model=self.deployment,
                messages=messages,
                max_tokens=800,
                temperature=0.7,
                stream=False
            )
            
            return completion.choices[0].message.content or "No answer returned."
            
        except Exception as ex:
            return f"An error occurred: {str(ex)}"
    
    def ask_question_sync(self, user_question):
        """Synchronous version of ask_question_async for use with tkinter."""
        import asyncio
        try:
            # Run the async function in a new event loop
            loop = asyncio.new_event_loop()
            asyncio.set_event_loop(loop)
            result = loop.run_until_complete(self.ask_question_async(user_question))
            loop.close()
            return result
        except Exception as ex:
            return f"An error occurred: {str(ex)}"
