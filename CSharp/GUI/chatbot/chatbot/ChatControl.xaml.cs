using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace chatbot
{
    public partial class ChatControl : UserControl
    {
        private ChatService chatService;
        private string selectedImagePath = string.Empty;

        // Events to communicate with parent window
        public event EventHandler<HistoryItem> QuestionAsked;

        public ChatControl()
        {
            InitializeComponent();
            chatService = new ChatService();
        }

        public void SetChatService(ChatService service)
        {
            chatService = service;
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

            // Raise event to notify parent about new question/answer
            var historyItem = new HistoryItem
            {
                Question = question,
                Answer = answer,
                ImagePath = selectedImagePath
            };
            QuestionAsked?.Invoke(this, historyItem);

            // Clear inputs
            QuestionTextBox.Clear();
            ClearSelectedImage();
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

        public void DisplayHistoryItem(HistoryItem item)
        {
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
    }
}
