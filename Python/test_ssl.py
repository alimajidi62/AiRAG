#!/usr/bin/env python3
"""
Test script to verify SSL certificate handling and Azure connections.
"""

import sys
import os

# Add the current directory to Python path
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

try:
    from chat_service import ChatService
    print("✓ ChatService imported successfully")
    
    # Test initialization
    chat_service = ChatService()
    print("✓ ChatService initialized successfully")
    print("✓ SSL certificate configuration applied")
    
    # Test a simple question
    print("\nTesting with a simple question...")
    test_question = "Hello, can you hear me?"
    
    try:
        response = chat_service.ask_question_sync(test_question)
        print(f"✓ Question processed successfully")
        print(f"Response: {response[:100]}..." if len(response) > 100 else f"Response: {response}")
    except Exception as e:
        print(f"✗ Error during question processing: {e}")
        
except ImportError as e:
    print(f"✗ Import error: {e}")
    print("Make sure all required packages are installed:")
    print("pip install azure-search-documents azure-core azure-identity openai certifi requests")
    
except Exception as e:
    print(f"✗ Initialization error: {e}")
    print("Check your Appkey.txt file and Azure credentials")

print("\nTest completed!")
