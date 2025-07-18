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
    SpinnerOverlay.Visibility = Visibility.Visible; // Show spinner

    string answer = await chatService.AskQuestionAsync(question);

    SpinnerOverlay.Visibility = Visibility.Collapsed; // Hide spinner
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
private void ToggleHistoryButton_Click(object sender, RoutedEventArgs e)
{
    if (HistoryPanelGrid.Visibility == Visibility.Visible)
    {
        HistoryPanelGrid.Visibility = Visibility.Collapsed;
        ToggleHistoryButton.Content = "Show History";
        HistoryColumn.Width = new GridLength(0);      // Collapse history column
        MainColumn.Width = new GridLength(1, GridUnitType.Star); // Main panel takes all space
    }
    else
    {
        HistoryPanelGrid.Visibility = Visibility.Visible;
        ToggleHistoryButton.Content = "Hide History";
        HistoryColumn.Width = new GridLength(1, GridUnitType.Star);   // 1/3 for history
        MainColumn.Width = new GridLength(2, GridUnitType.Star);      // 2/3 for main panel
    }
}
        private void ClearHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            historyItems.Clear();
            SaveHistory(); // If you have a SaveHistory method for persistence
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