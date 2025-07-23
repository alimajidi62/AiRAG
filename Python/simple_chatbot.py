#!/usr/bin/env python3
"""
Simple command-line test for the chatbot without GUI.
"""

import sys
import os

# Add the current directory to Python path
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

from chat_service import ChatService

def main():
    print("=== AI Chatbot Test (Command Line) ===")
    print("SSL certificate verification is disabled for development.")
    print("Type 'quit' to exit.\n")
    
    try:
        # Initialize chat service
        chat_service = ChatService()
        print("✓ ChatService initialized successfully\n")
        
        while True:
            # Get user input
            question = input("Enter your question: ").strip()
            
            if question.lower() in ['quit', 'exit', 'q']:
                print("Goodbye!")
                break
                
            if not question:
                print("Please enter a question.\n")
                continue
            
            print("\nProcessing your question...")
            
            # Get response
            try:
                response = chat_service.ask_question_sync(question)
                print(f"\nAnswer: {response}\n")
                print("-" * 80)
                
            except Exception as e:
                print(f"\nError getting response: {e}\n")
                
    except Exception as e:
        print(f"Failed to initialize chat service: {e}")
        print("Make sure your Appkey.txt file exists and contains valid Azure credentials.")

if __name__ == "__main__":
    main()
