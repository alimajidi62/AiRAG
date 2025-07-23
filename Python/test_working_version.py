#!/usr/bin/env python3
"""
Test the working version that uses development SSL (like .NET)
"""

import sys
import os

# Add the Python directory to path
sys.path.insert(0, os.path.join(os.path.dirname(os.path.abspath(__file__)), 'Python'))

from chat_service_flexible import create_development_chat_service

def main():
    print("=== Testing Python Chatbot (Development SSL - Like .NET) ===\n")
    
    try:
        # Create chat service with development SSL (works like .NET)
        chat_service = create_development_chat_service()
        print("✓ ChatService initialized successfully\n")
        
        # Test question
        test_question = "What can you tell me about World War 2?"
        print(f"Question: {test_question}")
        print("Processing...\n")
        
        # Get response
        response = chat_service.ask_question_sync(test_question)
        
        if response.startswith("An error occurred:"):
            print(f"❌ Error: {response}")
        else:
            print("✅ SUCCESS! The chatbot is working!")
            print(f"Response preview: {response[:150]}...")
            print("\n🎉 Your Python chatbot now works just like your .NET WPF version!")
            
    except Exception as e:
        print(f"❌ Failed: {e}")

if __name__ == "__main__":
    main()
