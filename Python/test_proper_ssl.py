#!/usr/bin/env python3
"""
Test script to verify proper SSL handling (like .NET)
"""

import sys
import os

# Add the Python directory to path
sys.path.insert(0, os.path.join(os.path.dirname(os.path.abspath(__file__)), 'Python'))

try:
    from chat_service import ChatService
    print("✓ ChatService imported successfully")
    
    # Test initialization
    chat_service = ChatService()
    print("✓ ChatService initialized successfully")
    
    # Test a simple question
    print("\nTesting with a simple question...")
    test_question = "Hello, can you tell me about World War 2?"
    
    try:
        response = chat_service.ask_question_sync(test_question)
        print(f"✓ Question processed successfully")
        print(f"Response: {response[:200]}..." if len(response) > 200 else f"Response: {response}")
    except Exception as e:
        print(f"✗ Error during question processing: {e}")
        
except ImportError as e:
    print(f"✗ Import error: {e}")
    
except Exception as e:
    print(f"✗ Initialization error: {e}")

print("\nTest completed!")
