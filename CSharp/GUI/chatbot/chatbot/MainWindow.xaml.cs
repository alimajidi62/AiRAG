using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using Newtonsoft.Json;
using Microsoft.Win32;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Windows.Documents;
using System.Text.RegularExpressions;
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
        private string selectedImagePath = string.Empty;
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
        }

        private void SetAnswerText(string text)
        {
            var flowDocument = new FlowDocument();
            var paragraph = new Paragraph();
            
            // Parse the text for Markdown formatting
            ParseMarkdownToParagraph(text, paragraph);
            
            flowDocument.Blocks.Add(paragraph);
            AnswerRichTextBox.Document = flowDocument;
        }

        private void ParseMarkdownToParagraph(string text, Paragraph paragraph)
        {
            if (string.IsNullOrEmpty(text))
                return;

            // Split text into lines to handle bullet points and other line-based formatting
            var lines = text.Split(new[] { '\n', '\r' }, StringSplitOptions.None);
            
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    paragraph.Inlines.Add(new LineBreak());
                    continue;
                }

                var trimmedLine = line.Trim();
                
                // Handle bullet points (-, *, or •)
                if (trimmedLine.StartsWith("- ") || trimmedLine.StartsWith("* ") || trimmedLine.StartsWith("• "))
                {
                    paragraph.Inlines.Add(new Run("• ") { FontWeight = FontWeights.Bold });
                    var bulletContent = trimmedLine.Substring(2);
                    ParseInlineMarkdown(bulletContent, paragraph);
                    paragraph.Inlines.Add(new LineBreak());
                }
                // Handle numbered lists
                else if (Regex.IsMatch(trimmedLine, @"^\d+\.\s"))
                {
                    var match = Regex.Match(trimmedLine, @"^(\d+\.\s)(.*)");
                    if (match.Success)
                    {
                        paragraph.Inlines.Add(new Run(match.Groups[1].Value) { FontWeight = FontWeights.Bold });
                        ParseInlineMarkdown(match.Groups[2].Value, paragraph);
                        paragraph.Inlines.Add(new LineBreak());
                    }
                }
                // Handle headers (##, ###, etc.)
                else if (trimmedLine.StartsWith("#"))
                {
                    var headerLevel = 0;
                    while (headerLevel < trimmedLine.Length && trimmedLine[headerLevel] == '#')
                        headerLevel++;
                    
                    if (headerLevel < trimmedLine.Length && trimmedLine[headerLevel] == ' ')
                    {
                        var headerText = trimmedLine.Substring(headerLevel + 1);
                        var fontSize = Math.Max(16, 24 - (headerLevel * 2));
                        var run = new Run(headerText)
                        {
                            FontWeight = FontWeights.Bold,
                            FontSize = fontSize
                        };
                        paragraph.Inlines.Add(run);
                        paragraph.Inlines.Add(new LineBreak());
                        paragraph.Inlines.Add(new LineBreak());
                    }
                    else
                    {
                        ParseInlineMarkdown(trimmedLine, paragraph);
                        paragraph.Inlines.Add(new LineBreak());
                    }
                }
                // Regular line
                else
                {
                    ParseInlineMarkdown(trimmedLine, paragraph);
                    paragraph.Inlines.Add(new LineBreak());
                }
            }
        }

        private void ParseInlineMarkdown(string text, Paragraph paragraph)
        {
            if (string.IsNullOrEmpty(text))
                return;

            var index = 0;
            while (index < text.Length)
            {
                // Find next markdown formatting
                var boldMatch = Regex.Match(text.Substring(index), @"\*\*(.*?)\*\*");
                var italicMatch = Regex.Match(text.Substring(index), @"\*(.*?)\*");
                var codeMatch = Regex.Match(text.Substring(index), @"`(.*?)`");

                var nextMarkdown = int.MaxValue;
                var markdownType = "";
                var markdownMatch = default(Match);

                // Find the earliest markdown formatting
                if (boldMatch.Success && boldMatch.Index < nextMarkdown)
                {
                    nextMarkdown = boldMatch.Index;
                    markdownType = "bold";
                    markdownMatch = boldMatch;
                }
                if (italicMatch.Success && italicMatch.Index < nextMarkdown && 
                    (markdownType != "bold" || italicMatch.Index != boldMatch.Index)) // Avoid conflict with bold
                {
                    nextMarkdown = italicMatch.Index;
                    markdownType = "italic";
                    markdownMatch = italicMatch;
                }
                if (codeMatch.Success && codeMatch.Index < nextMarkdown)
                {
                    nextMarkdown = codeMatch.Index;
                    markdownType = "code";
                    markdownMatch = codeMatch;
                }

                if (nextMarkdown == int.MaxValue)
                {
                    // No more markdown formatting, add the rest as plain text
                    paragraph.Inlines.Add(new Run(text.Substring(index)));
                    break;
                }

                // Add text before the markdown formatting
                if (nextMarkdown > 0)
                {
                    paragraph.Inlines.Add(new Run(text.Substring(index, nextMarkdown)));
                }

                // Add the formatted text
                var formattedText = markdownMatch.Groups[1].Value;
                Run formattedRun = new Run(formattedText);

                switch (markdownType)
                {
                    case "bold":
                        formattedRun.FontWeight = FontWeights.Bold;
                        break;
                    case "italic":
                        formattedRun.FontStyle = FontStyles.Italic;
                        break;
                    case "code":
                        formattedRun.FontFamily = new FontFamily("Consolas, Courier New");
                        formattedRun.Background = new SolidColorBrush(Color.FromRgb(240, 240, 240));
                        break;
                }

                paragraph.Inlines.Add(formattedRun);

                // Move index past the markdown formatting
                index += nextMarkdown + markdownMatch.Length;
            }
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
                SetAnswerText("Please enter a question or select an image.");
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
            SetAnswerText(answer);
            DisplayedQuestionTextBox.Text = question;
            AskButton.IsEnabled = true;

            // Add to history
            HistoryPanel.AddHistoryItem(new HistoryItem { 
                Question = question, 
                Answer = answer, 
                ImagePath = selectedImagePath 
            });

            // Clear inputs
            QuestionTextBox.Clear();
            ClearSelectedImage();
        }

        // History Control Event Handlers
        private void HistoryPanel_HistoryItemSelected(object sender, HistoryItemSelectedEventArgs e)
        {
            var item = e.SelectedItem;
            QuestionTextBox.Text = item.Question;
            SetAnswerText(item.Answer);
            DisplayedQuestionTextBox.Text = item.Question;
            
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

        private void HistoryPanel_HistoryCleared(object sender, EventArgs e)
        {
            // Additional cleanup if needed when history is cleared
        }

        private void HistoryButton_Click(object sender, RoutedEventArgs e)
        {
            // This method is now handled by HistoryPanel_HistoryItemSelected
            // Keeping for backward compatibility, but functionality moved to HistoryControl
        }

        private void LoadHistory()
        {
            // History loading is now handled by HistoryControl
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
            // This method is now handled by HistoryControl
            // Keeping for backward compatibility, but functionality moved to HistoryControl
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
