import tkinter as tk
from tkinter import ttk, scrolledtext, messagebox
import json
import os
import threading
import sys

# Add the Python directory to path
sys.path.insert(0, os.path.join(os.path.dirname(os.path.abspath(__file__)), 'Python'))

from chat_service_flexible import create_development_chat_service


class HistoryItem:
    def __init__(self, question, answer):
        self.question = question
        self.answer = answer
    
    def to_dict(self):
        return {"question": self.question, "answer": self.answer}
    
    @classmethod
    def from_dict(cls, data):
        return cls(data["question"], data["answer"])


class ChatbotGUI:
    def __init__(self, root):
        self.root = root
        self.root.title("AI Chatbot - Python (Works like .NET)")
        self.root.geometry("1000x700")
        self.root.configure(bg="#F5F8FF")
        
        # Initialize chat service with development SSL (works like .NET)
        try:
            self.chat_service = create_development_chat_service()
        except Exception as e:
            messagebox.showerror("Error", f"Failed to initialize chat service: {str(e)}")
            return
        
        # History management
        self.history_items = []
        self.history_file = "chat_history.json"
        self.history_visible = True
        
        # Create the UI
        self.create_widgets()
        self.load_history()
        
        # Bind Enter key to question input
        self.question_entry.bind('<Return>', lambda event: self.ask_question())
    
    def create_widgets(self):
        """Create all UI widgets."""
        # Main container
        main_frame = tk.Frame(self.root, bg="#F5F8FF")
        main_frame.pack(fill=tk.BOTH, expand=True, padx=10, pady=10)
        
        # Top frame for toggle button and SSL info
        top_frame = tk.Frame(main_frame, bg="#F5F8FF")
        top_frame.pack(fill=tk.X, pady=(0, 10))
        
        # SSL info label
        ssl_info = tk.Label(
            top_frame,
            text="🔧 Development SSL Mode (like .NET) - Working!",
            bg="#D4F6D4",
            fg="#2D5A2D",
            font=("Arial", 9),
            padx=10,
            pady=3
        )
        ssl_info.pack(side=tk.RIGHT)
        
        # Toggle history button
        self.toggle_button = tk.Button(
            top_frame,
            text="Hide History",
            bg="#2563EB",
            fg="white",
            font=("Arial", 10, "bold"),
            padx=15,
            pady=5,
            command=self.toggle_history,
            cursor="hand2"
        )
        self.toggle_button.pack(side=tk.LEFT)
        
        # Content frame (contains history and main chat)
        self.content_frame = tk.Frame(main_frame, bg="#F5F8FF")
        self.content_frame.pack(fill=tk.BOTH, expand=True)
        
        # Configure grid weights
        self.content_frame.grid_columnconfigure(0, weight=1)
        self.content_frame.grid_columnconfigure(1, weight=2)
        self.content_frame.grid_rowconfigure(0, weight=1)
        
        # Create history panel
        self.create_history_panel()
        
        # Create main chat area
        self.create_main_chat_area()
    
    def create_history_panel(self):
        """Create the history panel."""
        # History frame
        self.history_frame = tk.Frame(self.content_frame, bg="#E3ECFF")
        self.history_frame.grid(row=0, column=0, sticky="nsew", padx=(0, 10))
        
        # History title
        title_label = tk.Label(
            self.history_frame,
            text="History",
            font=("Arial", 16, "bold"),
            bg="#E3ECFF",
            fg="#1B3A7A"
        )
        title_label.pack(pady=(15, 10))
        
        # Clear history button
        clear_button = tk.Button(
            self.history_frame,
            text="Clear History",
            bg="#2563EB",
            fg="white",
            font=("Arial", 10, "bold"),
            padx=10,
            pady=5,
            command=self.clear_history,
            cursor="hand2"
        )
        clear_button.pack(pady=(0, 10))
        
        # History list frame with scrollbar
        history_list_frame = tk.Frame(self.history_frame, bg="#E3ECFF")
        history_list_frame.pack(fill=tk.BOTH, expand=True, padx=10, pady=(0, 10))
        
        # Scrollable frame for history items
        self.history_canvas = tk.Canvas(history_list_frame, bg="#E3ECFF", highlightthickness=0)
        history_scrollbar = ttk.Scrollbar(history_list_frame, orient="vertical", command=self.history_canvas.yview)
        self.history_scrollable_frame = tk.Frame(self.history_canvas, bg="#E3ECFF")
        
        self.history_scrollable_frame.bind(
            "<Configure>",
            lambda e: self.history_canvas.configure(scrollregion=self.history_canvas.bbox("all"))
        )
        
        self.history_canvas.create_window((0, 0), window=self.history_scrollable_frame, anchor="nw")
        self.history_canvas.configure(yscrollcommand=history_scrollbar.set)
        
        self.history_canvas.pack(side="left", fill="both", expand=True)
        history_scrollbar.pack(side="right", fill="y")
        
        # Bind mousewheel to canvas
        def on_mousewheel(event):
            self.history_canvas.yview_scroll(int(-1*(event.delta/120)), "units")
        self.history_canvas.bind("<MouseWheel>", on_mousewheel)
    
    def create_main_chat_area(self):
        """Create the main chat area."""
        # Main chat frame
        main_chat_frame = tk.Frame(self.content_frame, bg="white", relief=tk.RAISED, bd=1)
        main_chat_frame.grid(row=0, column=1, sticky="nsew")
        
        # Title
        title_label = tk.Label(
            main_chat_frame,
            text="AI Chatbot (Python)",
            font=("Arial", 24, "bold"),
            bg="white",
            fg="#2563EB"
        )
        title_label.pack(pady=(20, 15))
        
        # Input frame
        input_frame = tk.Frame(main_chat_frame, bg="white")
        input_frame.pack(fill=tk.X, padx=30, pady=(0, 15))
        
        # Question entry
        self.question_entry = tk.Entry(
            input_frame,
            font=("Arial", 14),
            bg="#F0F6FF",
            fg="#1B3A7A",
            relief=tk.SOLID,
            bd=2,
            insertbackground="#1B3A7A"
        )
        self.question_entry.pack(side=tk.LEFT, fill=tk.X, expand=True, ipady=8, padx=(0, 10))
        
        # Ask button
        self.ask_button = tk.Button(
            input_frame,
            text="Ask",
            font=("Arial", 12, "bold"),
            bg="#2563EB",
            fg="white",
            padx=20,
            pady=8,
            command=self.ask_question,
            cursor="hand2"
        )
        self.ask_button.pack(side=tk.RIGHT)
        
        # Response area frame
        response_frame = tk.Frame(main_chat_frame, bg="#E3ECFF", relief=tk.SOLID, bd=1)
        response_frame.pack(fill=tk.BOTH, expand=True, padx=30, pady=(0, 20))
        
        # Question display
        question_label = tk.Label(
            response_frame,
            text="Question:",
            font=("Arial", 12, "bold"),
            bg="#E3ECFF",
            fg="#2563EB",
            anchor="w"
        )
        question_label.pack(fill=tk.X, padx=15, pady=(15, 5))
        
        self.displayed_question = tk.Label(
            response_frame,
            text="",
            font=("Arial", 11),
            bg="#E3ECFF",
            fg="#1B3A7A",
            anchor="w",
            wraplength=400,
            justify=tk.LEFT
        )
        self.displayed_question.pack(fill=tk.X, padx=15, pady=(0, 10))
        
        # Answer display
        answer_label = tk.Label(
            response_frame,
            text="Answer:",
            font=("Arial", 12, "bold"),
            bg="#E3ECFF",
            fg="#2563EB",
            anchor="w"
        )
        answer_label.pack(fill=tk.X, padx=15, pady=(0, 5))
        
        # Scrolled text for answer
        self.answer_text = scrolledtext.ScrolledText(
            response_frame,
            font=("Arial", 11),
            bg="#E3ECFF",
            fg="#1B3A7A",
            wrap=tk.WORD,
            height=10,
            relief=tk.FLAT,
            state=tk.DISABLED
        )
        self.answer_text.pack(fill=tk.BOTH, expand=True, padx=15, pady=(0, 15))
        
        # Loading overlay frame
        self.loading_frame = tk.Frame(main_chat_frame, bg="white")
        self.loading_label = tk.Label(
            self.loading_frame,
            text="Waiting for response...",
            font=("Arial", 14, "bold"),
            bg="white",
            fg="#2563EB"
        )
        self.loading_label.pack(expand=True)
    
    def toggle_history(self):
        """Toggle the visibility of the history panel."""
        if self.history_visible:
            self.history_frame.grid_remove()
            self.toggle_button.config(text="Show History")
            self.content_frame.grid_columnconfigure(0, weight=0)
            self.history_visible = False
        else:
            self.history_frame.grid(row=0, column=0, sticky="nsew", padx=(0, 10))
            self.toggle_button.config(text="Hide History")
            self.content_frame.grid_columnconfigure(0, weight=1)
            self.history_visible = True
    
    def ask_question(self):
        """Handle the ask question button click."""
        question = self.question_entry.get().strip()
        if not question:
            messagebox.showwarning("Warning", "Please enter a question.")
            return
        
        # Disable the ask button and show loading
        self.ask_button.config(state=tk.DISABLED)
        self.loading_frame.place(relx=0.5, rely=0.5, anchor=tk.CENTER)
        self.displayed_question.config(text=question)
        
        # Clear the entry
        self.question_entry.delete(0, tk.END)
        
        # Run the chat service in a separate thread
        thread = threading.Thread(target=self._ask_question_thread, args=(question,))
        thread.daemon = True
        thread.start()
    
    def _ask_question_thread(self, question):
        """Thread function to call the chat service."""
        try:
            answer = self.chat_service.ask_question_sync(question)
            # Schedule UI update on main thread
            self.root.after(0, self._update_answer, question, answer)
        except Exception as e:
            error_msg = f"An error occurred: {str(e)}"
            self.root.after(0, self._update_answer, question, error_msg)
    
    def _update_answer(self, question, answer):
        """Update the UI with the answer (called from main thread)."""
        # Hide loading and enable button
        self.loading_frame.place_forget()
        self.ask_button.config(state=tk.NORMAL)
        
        # Update answer display
        self.answer_text.config(state=tk.NORMAL)
        self.answer_text.delete(1.0, tk.END)
        self.answer_text.insert(tk.END, answer)
        self.answer_text.config(state=tk.DISABLED)
        
        # Add to history
        history_item = HistoryItem(question, answer)
        self.history_items.insert(0, history_item)
        self.update_history_display()
        self.save_history()
    
    def update_history_display(self):
        """Update the history display."""
        # Clear existing history buttons
        for widget in self.history_scrollable_frame.winfo_children():
            widget.destroy()
        
        # Add history items
        for i, item in enumerate(self.history_items):
            button = tk.Button(
                self.history_scrollable_frame,
                text=item.question[:50] + "..." if len(item.question) > 50 else item.question,
                font=("Arial", 10),
                bg="white",
                fg="#1B3A7A",
                relief=tk.SOLID,
                bd=1,
                anchor="w",
                command=lambda idx=i: self.load_history_item(idx),
                cursor="hand2",
                wraplength=200
            )
            button.pack(fill=tk.X, pady=2, padx=5)
    
    def load_history_item(self, index):
        """Load a history item into the chat area."""
        if 0 <= index < len(self.history_items):
            item = self.history_items[index]
            self.question_entry.delete(0, tk.END)
            self.question_entry.insert(0, item.question)
            self.displayed_question.config(text=item.question)
            
            self.answer_text.config(state=tk.NORMAL)
            self.answer_text.delete(1.0, tk.END)
            self.answer_text.insert(tk.END, item.answer)
            self.answer_text.config(state=tk.DISABLED)
    
    def clear_history(self):
        """Clear all history items."""
        if messagebox.askyesno("Confirm", "Are you sure you want to clear all history?"):
            self.history_items.clear()
            self.update_history_display()
            self.save_history()
    
    def load_history(self):
        """Load history from file."""
        if os.path.exists(self.history_file):
            try:
                with open(self.history_file, 'r', encoding='utf-8') as f:
                    data = json.load(f)
                    self.history_items = [HistoryItem.from_dict(item) for item in data]
                    self.update_history_display()
            except Exception as e:
                print(f"Error loading history: {e}")
    
    def save_history(self):
        """Save history to file."""
        try:
            data = [item.to_dict() for item in self.history_items]
            with open(self.history_file, 'w', encoding='utf-8') as f:
                json.dump(data, f, ensure_ascii=False, indent=2)
        except Exception as e:
            print(f"Error saving history: {e}")


def main():
    root = tk.Tk()
    app = ChatbotGUI(root)
    
    # Handle window closing
    def on_closing():
        app.save_history()
        root.destroy()
    
    root.protocol("WM_DELETE_WINDOW", on_closing)
    root.mainloop()


if __name__ == "__main__":
    main()
