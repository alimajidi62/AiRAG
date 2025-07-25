using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using Newtonsoft.Json;
using Microsoft.Win32;
using System.Windows.Media.Imaging;

namespace chatbot
{
    public class HistoryItem
    {
        public string Question { get; set; }
        public string Answer { get; set; }
        public string ImagePath { get; set; }
        public bool HasImage => !string.IsNullOrEmpty(ImagePath);
    }

    public partial class MainWindow : Window
    {
        private ChatService chatService;
        private ObservableCollection<HistoryItem> historyItems = new ObservableCollection<HistoryItem>();
        private readonly string historyFile = "chat_history.json";
        private string selectedImagePath = string.Empty;

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
            if (string.IsNullOrWhiteSpace(question) && string.IsNullOrEmpty(selectedImagePath))
            {
                AnswerTextBlock.Text = "Please enter a question or select an image.";
                return;
            }

            AskButton.IsEnabled = false;
            SpinnerOverlay.Visibility = Visibility.Visible; // Show spinner

            string answer;
            if (!string.IsNullOrEmpty(selectedImagePath))
            {
                answer = await chatService.AskQuestionWithImageAsync(question, selectedImagePath);
            }
            else
            {
                answer = await chatService.AskQuestionAsync(question);
            }

            SpinnerOverlay.Visibility = Visibility.Collapsed; // Hide spinner
            AnswerTextBlock.Text = answer;
            DisplayedQuestionTextBlock.Text = question;
            AskButton.IsEnabled = true;

            // Add to history
            historyItems.Insert(0, new HistoryItem { 
                Question = question, 
                Answer = answer, 
                ImagePath = selectedImagePath 
            });
            SaveHistory();

            // Clear inputs
            QuestionTextBox.Clear();
            ClearSelectedImage();
        }

        private void HistoryButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is HistoryItem item)
            {
                QuestionTextBox.Text = item.Question;
                AnswerTextBlock.Text = item.Answer;
                DisplayedQuestionTextBlock.Text = item.Question;
                
                // Handle image if present
                if (item.HasImage && File.Exists(item.ImagePath))
                {
                    selectedImagePath = item.ImagePath;
                    DisplaySelectedImage();
                }
                else
                {
                    ClearSelectedImage();
                }
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

        private void SelectImageButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpg;*.jpeg;*.gif;*.bmp)|*.png;*.jpg;*.jpeg;*.gif;*.bmp|All files (*.*)|*.*",
                Title = "Select an image"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                selectedImagePath = openFileDialog.FileName;
                DisplaySelectedImage();
            }
        }

        private void ClearImageButton_Click(object sender, RoutedEventArgs e)
        {
            ClearSelectedImage();
        }

        private void DisplaySelectedImage()
        {
            if (!string.IsNullOrEmpty(selectedImagePath) && File.Exists(selectedImagePath))
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(selectedImagePath);
                bitmap.DecodePixelWidth = 200; // Limit size for display
                bitmap.EndInit();
                
                SelectedImageDisplay.Source = bitmap;
                SelectedImageDisplay.Visibility = Visibility.Visible;
                ClearImageButton.Visibility = Visibility.Visible;
                ImageStatusText.Text = $"Image: {Path.GetFileName(selectedImagePath)}";
                ImageStatusText.Visibility = Visibility.Visible;
            }
        }

        private void ClearSelectedImage()
        {
            selectedImagePath = string.Empty;
            SelectedImageDisplay.Source = null;
            SelectedImageDisplay.Visibility = Visibility.Collapsed;
            ClearImageButton.Visibility = Visibility.Collapsed;
            ImageStatusText.Visibility = Visibility.Collapsed;
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