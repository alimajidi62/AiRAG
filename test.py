import os
from openai import AzureOpenAI
from azure.identity import DefaultAzureCredential, get_bearer_token_provider

# Read endpoint and api_key from Appkey.txt
def read_keys_from_file(filename="Appkey.txt"):
    with open(filename, "r") as f:
        lines = f.read().splitlines()
        endpoint = lines[0].strip()
        api_key = lines[1].strip()
    return endpoint, api_key

endpoint, api_key = read_keys_from_file()
deployment = os.getenv("DEPLOYMENT_NAME", "gpt-4.1")
      
# Initialize Azure OpenAI client with Entra ID authentication
client = AzureOpenAI(
    azure_endpoint=endpoint,
    api_key=api_key,
    api_version="2025-01-01-preview",
)


# IMAGE_PATH = "YOUR_IMAGE_PATH"
# encoded_image = base64.b64encode(open(IMAGE_PATH, 'rb').read()).decode('ascii')
chat_prompt = [
    {
        "role": "system",
        "content": [
            {
                "type": "text",
                "text": "how is Mahmoud Ahmadinejad in iran."
            }
        ]
    }
]

# Include speech result if speech is enabled
messages = chat_prompt

completion = client.chat.completions.create(
    model=deployment,
    messages=messages,
    max_tokens=800,
    temperature=1,
    top_p=1,
    frequency_penalty=0,
    presence_penalty=0,
    stop=None,
    stream=False
)

print(completion.to_json())