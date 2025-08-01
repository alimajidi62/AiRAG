using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using Newtonsoft.Json;
using Microsoft.Win32;
using System.Windows.Media;
using System.Threading.Tasks;
using System.Collections.Generic;
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
        private ObservableCollection<DocumentInfo> documentItems = new ObservableCollection<DocumentInfo>();
        private readonly string historyFile = "chat_history.json";
        private WpfColor currentBackgroundColor = WpfColor.FromRgb(240, 240, 240); // Default classic gray

        public MainWindow()
        {
            InitializeComponent();
            chatService = new ChatService();
            DocumentItemsControl.ItemsSource = documentItems;
            LoadSavedColorTheme();
            LoadDocumentList();
            
            // Wire up events for the ColorSelectionControl
            ColorSelectionPanel.ColorThemeSelected += ColorSelectionPanel_ColorThemeSelected;
            ColorSelectionPanel.ColorPanelCloseRequested += ColorSelectionPanel_ColorPanelCloseRequested;
            
            // Wire up events for the HistoryControl
            HistoryPanel.HistoryItemSelected += HistoryPanel_HistoryItemSelected;
            HistoryPanel.HistoryCleared += HistoryPanel_HistoryCleared;
            
            // Wire up events for the ChatControl
            ChatPanel.QuestionAsked += ChatPanel_QuestionAsked;
            ChatPanel.SetChatService(chatService);
        }
        

        // History Control Event Handlers
        private void HistoryPanel_HistoryItemSelected(object sender, HistoryItemSelectedEventArgs e)
        {
            var item = e.SelectedItem;
            ChatPanel.DisplayHistoryItem(item);
        }

        private void HistoryPanel_HistoryCleared(object sender, EventArgs e)
        {
            // Additional cleanup if needed when history is cleared
        }

        // ChatControl Event Handler
        private void ChatPanel_QuestionAsked(object sender, HistoryItem e)
        {
            // Add to history
            HistoryPanel.AddHistoryItem(e);
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

        protected override void OnClosed(System.EventArgs e)
        {
            HistoryPanel.SaveHistory();
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
            HistoryPanel.UpdateThemeColors(new SolidColorBrush(textColor), new SolidColorBrush(buttonColor), 
                                         new SolidColorBrush(borderColor), new SolidColorBrush(panelColor));
            
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
            var answerRichTextBox = FindName("AnswerRichTextBox") as FrameworkElement;
            var displayBorder = answerRichTextBox?.Parent;
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
                    else if (button.Name == "AddDocumentButton")
                    {
                        // Keep the green background for the Add Document button, but update text
                        button.Foreground = Brushes.White;
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
                else if (child is TextBox textBox && textBox.IsReadOnly && 
                         textBox.Name == "DisplayedQuestionTextBox")
                {
                    textBox.Foreground = textBrush;
                }
                else if (child is RichTextBox richTextBox && richTextBox.IsReadOnly &&
                         richTextBox.Name == "AnswerRichTextBox")
                {
                    richTextBox.Foreground = textBrush;
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

        // Document Management Methods
        private void DocumentManagementButton_Click(object sender, RoutedEventArgs e)
        {
            DocumentManagementPanel.Visibility = DocumentManagementPanel.Visibility == Visibility.Visible 
                ? Visibility.Collapsed : Visibility.Visible;
        }

        private void CloseDocumentPanelButton_Click(object sender, RoutedEventArgs e)
        {
            DocumentManagementPanel.Visibility = Visibility.Collapsed;
        }

        private async void AddDocumentButton_Click(object sender, RoutedEventArgs e)
        {
            string url = DocumentUrlTextBox.Text.Trim();
            if (string.IsNullOrEmpty(url))
            {
                ShowDocumentStatus("Please enter a valid URL.", false);
                return;
            }

            if (!Uri.TryCreate(url, UriKind.Absolute, out _))
            {
                ShowDocumentStatus("Please enter a valid URL format.", false);
                return;
            }

            // Show progress
            ProgressSection.Visibility = Visibility.Visible;
            AddDocumentButton.IsEnabled = false;
            DocumentUrlTextBox.IsEnabled = false;

            var progress = new Progress<string>(status => 
            {
                Dispatcher.Invoke(() => ProgressStatusText.Text = status);
            });

            try
            {
                var result = await chatService.AddDocumentFromUrlAsync(url, progress);
                
                if (result.Success)
                {
                    ShowDocumentStatus(result.Message, true);
                    DocumentUrlTextBox.Clear();
                    await LoadDocumentList(); // Refresh the document list
                }
                else
                {
                    ShowDocumentStatus(result.Message, false);
                }
            }
            catch (Exception ex)
            {
                ShowDocumentStatus($"Unexpected error: {ex.Message}", false);
            }
            finally
            {
                // Hide progress and re-enable controls
                ProgressSection.Visibility = Visibility.Collapsed;
                AddDocumentButton.IsEnabled = true;
                DocumentUrlTextBox.IsEnabled = true;
            }
        }

        private async void RemoveDocumentButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string documentId)
            {
                var result = MessageBox.Show("Are you sure you want to remove this document?", 
                    "Confirm Removal", MessageBoxButton.YesNo, MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        var removeResult = await chatService.RemoveDocumentAsync(documentId);
                        
                        if (removeResult.Success)
                        {
                            ShowDocumentStatus(removeResult.Message, true);
                            await LoadDocumentList(); // Refresh the document list
                        }
                        else
                        {
                            ShowDocumentStatus(removeResult.Message, false);
                        }
                    }
                    catch (Exception ex)
                    {
                        ShowDocumentStatus($"Error removing document: {ex.Message}", false);
                    }
                }
            }
        }

        private async Task LoadDocumentList()
        {
            try
            {
                var documents = await chatService.GetUploadedDocumentsAsync();
                documentItems.Clear();
                foreach (var doc in documents)
                {
                    documentItems.Add(doc);
                }
            }
            catch (Exception ex)
            {
                ShowDocumentStatus($"Error loading documents: {ex.Message}", false);
            }
        }

        private void ShowDocumentStatus(string message, bool isSuccess)
        {
            DocumentStatusMessage.Text = message;
            DocumentStatusMessage.Foreground = new SolidColorBrush(isSuccess ? Colors.Green : Colors.Red);
            DocumentStatusMessage.Visibility = Visibility.Visible;

            // Auto-hide after 5 seconds
            var timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(5);
            timer.Tick += (s, e) =>
            {
                DocumentStatusMessage.Visibility = Visibility.Collapsed;
                timer.Stop();
            };
            timer.Start();
        }
        
    }
}
