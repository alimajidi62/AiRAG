using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using Newtonsoft.Json;

namespace chatbot
{
    public class HistoryItem
    {
        public string Question { get; set; }
        public string Answer { get; set; }
    }

    public partial class MainWindow : Window
    {
        private ChatService chatService;
        private ObservableCollection<HistoryItem> historyItems = new ObservableCollection<HistoryItem>();
        private readonly string historyFile = "chat_history.json";

        public MainWindow()
        {
            InitializeComponent();
            chatService = new ChatService();
            HistoryItemsPanel.ItemsSource = historyItems;
            LoadHistory();
        }
        private void QuestionTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                AskButton_Click(AskButton, new RoutedEventArgs());
                e.Handled = true;
            }
        }
        private async void AskButton_Click(object sender, RoutedEventArgs e)
        {
            string question = QuestionTextBox.Text;
            if (string.IsNullOrWhiteSpace(question))
            {
                AnswerTextBlock.Text = "Please enter a question.";
                return;
            }

            AskButton.IsEnabled = false;
            AnswerTextBlock.Text = "Thinking...";
            string answer = await chatService.AskQuestionAsync(question);
            AnswerTextBlock.Text = answer;
            AskButton.IsEnabled = true;

            // Add to history
            historyItems.Insert(0, new HistoryItem { Question = question, Answer = answer });
            SaveHistory();
        }

        private void HistoryButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is HistoryItem item)
            {
                QuestionTextBox.Text = item.Question;
                AnswerTextBlock.Text = item.Answer;
            }
        }

        private void LoadHistory()
        {
            if (File.Exists(historyFile))
            {
                var json = File.ReadAllText(historyFile);
                var items = JsonConvert.DeserializeObject<ObservableCollection<HistoryItem>>(json);
                if (items != null)
                {
                    foreach (var item in items)
                        historyItems.Add(item);
                }
            }
        }

        private void SaveHistory()
        {
            var json = JsonConvert.SerializeObject(historyItems);
            File.WriteAllText(historyFile, json);
        }

        protected override void OnClosed(System.EventArgs e)
        {
            SaveHistory();
            base.OnClosed(e);
        }
    }
}