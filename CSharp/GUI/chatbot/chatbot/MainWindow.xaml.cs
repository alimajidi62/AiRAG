using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using Newtonsoft.Json;
using Microsoft.Win32;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using WinFormsColorDialog = System.Windows.Forms.ColorDialog;
using WinFormsDialogResult = System.Windows.Forms.DialogResult;
using WpfColor = System.Windows.Media.Color;

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
        private WpfColor currentBackgroundColor = WpfColor.FromRgb(240, 240, 240); // Default classic gray

        public MainWindow()
        {
            InitializeComponent();
            chatService = new ChatService();
            HistoryItemsPanel.ItemsSource = historyItems;
            LoadHistory();
            LoadSavedColorTheme();
            
            // Wire up events for the ColorSelectionControl
            ColorSelectionPanel.ColorThemeSelected += ColorSelectionPanel_ColorThemeSelected;
            ColorSelectionPanel.ColorPanelCloseRequested += ColorSelectionPanel_ColorPanelCloseRequested;
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
            SaveColorTheme();
            base.OnClosed(e);
        }

        // Color Theme Methods
        private void ColorThemeButton_Click(object sender, RoutedEventArgs e)
        {
            ColorSelectionPanel.Visibility = ColorSelectionPanel.Visibility == Visibility.Visible 
                ? Visibility.Collapsed : Visibility.Visible;
        }

        private void ColorSelectionPanel_ColorThemeSelected(object sender, ColorThemeSelectedEventArgs e)
        {
            ApplyColorTheme(e.SelectedColor, e.ThemeName);
        }

        private void ColorSelectionPanel_ColorPanelCloseRequested(object sender, EventArgs e)
        {
            ColorSelectionPanel.Visibility = Visibility.Collapsed;
        }

        private void ApplyColorTheme(WpfColor backgroundColor, string themeName)
        {
            currentBackgroundColor = backgroundColor;
            
            // Calculate complementary colors based on background
            bool isDark = (backgroundColor.R + backgroundColor.G + backgroundColor.B) < 384; // Average < 128
            
            WpfColor textColor = isDark ? Colors.White : Colors.Black;
            WpfColor panelColor = isDark ? 
                WpfColor.FromRgb((byte)(backgroundColor.R + 30), (byte)(backgroundColor.G + 30), (byte)(backgroundColor.B + 30)) :
                WpfColor.FromRgb((byte)Math.Max(0, backgroundColor.R - 20), (byte)Math.Max(0, backgroundColor.G - 20), (byte)Math.Max(0, backgroundColor.B - 20));
            WpfColor borderColor = isDark ? Colors.Gray : WpfColor.FromRgb(128, 128, 128);
            
            // Calculate button colors (lighter/darker than background)
            WpfColor buttonColor = isDark ?
                WpfColor.FromRgb((byte)Math.Min(255, backgroundColor.R + 40), (byte)Math.Min(255, backgroundColor.G + 40), (byte)Math.Min(255, backgroundColor.B + 40)) :
                WpfColor.FromRgb((byte)Math.Max(0, backgroundColor.R - 30), (byte)Math.Max(0, backgroundColor.G - 30), (byte)Math.Max(0, backgroundColor.B - 30));
            
            // Apply to main window
            this.Background = new SolidColorBrush(backgroundColor);
            
            // Apply to history panel
            var historyBorder = FindName("HistoryPanelGrid") as Grid;
            if (historyBorder?.Children[0] is Border historyBorderElement)
            {
                historyBorderElement.Background = new SolidColorBrush(panelColor);
                historyBorderElement.BorderBrush = new SolidColorBrush(borderColor);
            }
            
            // Apply to main chat area - find it more safely
            var mainGrid = this.Content as Grid;
            if (mainGrid != null)
            {
                foreach (var child in mainGrid.Children)
                {
                    if (child is Border border && border.Name != "HistoryPanelGrid")
                    {
                        // This should be the main chat border
                        border.Background = new SolidColorBrush(backgroundColor);
                        border.BorderBrush = new SolidColorBrush(borderColor);
                        break;
                    }
                }
            }
            
            // Apply to display area
            var answerTextBlock = FindName("AnswerTextBlock") as FrameworkElement;
            var displayBorder = answerTextBlock?.Parent;
            while (displayBorder != null && !(displayBorder is Border))
                displayBorder = ((FrameworkElement)displayBorder).Parent;
            
            if (displayBorder is Border answerBorder)
            {
                answerBorder.Background = isDark ? new SolidColorBrush(WpfColor.FromRgb(64, 64, 64)) : Brushes.White;
                answerBorder.BorderBrush = new SolidColorBrush(borderColor);
            }
            
            // Update text and button colors
            UpdateTextColors(textColor, isDark);
            UpdateButtonColors(buttonColor, borderColor, textColor, isDark);
            
            // Close the color panel
            ColorSelectionPanel.Visibility = Visibility.Collapsed;
        }

        private void UpdateTextColors(WpfColor textColor, bool isDark)
        {
            var textBrush = new SolidColorBrush(textColor);
            
            // Find and update all TextBlocks
            UpdateElementTextColor(this, textBrush, isDark);
        }

        private void UpdateButtonColors(WpfColor buttonColor, WpfColor borderColor, WpfColor textColor, bool isDark)
        {
            var buttonBrush = new SolidColorBrush(buttonColor);
            var borderBrush = new SolidColorBrush(borderColor);
            var textBrush = new SolidColorBrush(textColor);
            
            // Update all buttons
            UpdateElementButtonColors(this, buttonBrush, borderBrush, textBrush);
        }

        private void UpdateElementButtonColors(DependencyObject parent, SolidColorBrush buttonBrush, SolidColorBrush borderBrush, SolidColorBrush textBrush)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                
                if (child is Button button)
                {
                    // Skip color theme buttons in the color picker to keep their preview colors
                    if (button.Name == "ClassicThemeButton" || button.Name == "BlueThemeButton" || 
                        button.Name == "GreenThemeButton" || button.Name == "PurpleThemeButton" || 
                        button.Name == "OrangeThemeButton" || button.Name == "DarkThemeButton")
                    {
                        // Only update text color for theme preview buttons
                        button.Foreground = textBrush;
                    }
                    else
                    {
                        // Update all other buttons
                        button.Background = buttonBrush;
                        button.BorderBrush = borderBrush;
                        button.Foreground = textBrush;
                    }
                }
                
                UpdateElementButtonColors(child, buttonBrush, borderBrush, textBrush);
            }
        }

        private void UpdateElementTextColor(DependencyObject parent, SolidColorBrush textBrush, bool isDark)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                
                if (child is TextBlock textBlock && 
                    textBlock.Name != "ImageStatusText") // Keep status text gray
                {
                    textBlock.Foreground = textBrush;
                }
                else if (child is Button button)
                {
                    button.Foreground = textBrush;
                }
                
                UpdateElementTextColor(child, textBrush, isDark);
            }
        }

        private void SaveColorTheme()
        {
            try
            {
                var colorData = new
                {
                    R = currentBackgroundColor.R,
                    G = currentBackgroundColor.G,
                    B = currentBackgroundColor.B
                };
                var json = JsonConvert.SerializeObject(colorData);
                File.WriteAllText("color_theme.json", json);
            }
            catch
            {
                // Ignore save errors
            }
        }

        private void LoadSavedColorTheme()
        {
            try
            {
                if (File.Exists("color_theme.json"))
                {
                    var json = File.ReadAllText("color_theme.json");
                    var colorData = JsonConvert.DeserializeAnonymousType(json, new { R = (byte)0, G = (byte)0, B = (byte)0 });
                    var savedColor = WpfColor.FromRgb(colorData.R, colorData.G, colorData.B);
                    ApplyColorTheme(savedColor, "Saved");
                }
            }
            catch
            {
                // Use default theme if loading fails
            }
        }
        
    }
}
