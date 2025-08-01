using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using Newtonsoft.Json;

namespace chatbot
{
    public partial class HistoryControl : UserControl
    {
        private ObservableCollection<HistoryItem> historyItems = new ObservableCollection<HistoryItem>();
        private readonly string historyFile = "chat_history.json";

        // Events to communicate with MainWindow
        public event EventHandler<HistoryItemSelectedEventArgs> HistoryItemSelected;
        public event EventHandler HistoryCleared;

        public HistoryControl()
        {
            InitializeComponent();
            HistoryItemsPanel.ItemsSource = historyItems;
            LoadHistory();
        }

        public void AddHistoryItem(HistoryItem item)
        {
            historyItems.Insert(0, item);
            SaveHistory();
        }

        public void ClearHistory()
        {
            historyItems.Clear();
            SaveHistory();
        }

        private void HistoryButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is HistoryItem item)
            {
                HistoryItemSelected?.Invoke(this, new HistoryItemSelectedEventArgs(item));
            }
        }

        private void ClearHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            ClearHistory();
            HistoryCleared?.Invoke(this, EventArgs.Empty);
        }

        private void LoadHistory()
        {
            if (File.Exists(historyFile))
            {
                try
                {
                    var json = File.ReadAllText(historyFile);
                    var items = JsonConvert.DeserializeObject<ObservableCollection<HistoryItem>>(json);
                    if (items != null)
                    {
                        historyItems.Clear();
                        foreach (var item in items)
                            historyItems.Add(item);
                    }
                }
                catch
                {
                    // Ignore loading errors
                }
            }
        }

        public void SaveHistory()
        {
            try
            {
                var json = JsonConvert.SerializeObject(historyItems);
                File.WriteAllText(historyFile, json);
            }
            catch
            {
                // Ignore save errors
            }
        }

        // Method to update colors when theme changes
        public void UpdateThemeColors(System.Windows.Media.SolidColorBrush textBrush, System.Windows.Media.SolidColorBrush buttonBrush, 
                                     System.Windows.Media.SolidColorBrush borderBrush, System.Windows.Media.SolidColorBrush panelBrush)
        {
            // Update the main border
            if (this.Content is Border mainBorder)
            {
                mainBorder.Background = panelBrush;
                mainBorder.BorderBrush = borderBrush;
            }

            // Update text elements
            UpdateElementColors(this, textBrush, buttonBrush, borderBrush);
        }

        private void UpdateElementColors(System.Windows.DependencyObject parent, 
                                       System.Windows.Media.SolidColorBrush textBrush, 
                                       System.Windows.Media.SolidColorBrush buttonBrush, 
                                       System.Windows.Media.SolidColorBrush borderBrush)
        {
            for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
                
                if (child is TextBlock textBlock)
                {
                    textBlock.Foreground = textBrush;
                }
                else if (child is Button button)
                {
                    button.Background = buttonBrush;
                    button.BorderBrush = borderBrush;
                    button.Foreground = textBrush;
                }
                
                UpdateElementColors(child, textBrush, buttonBrush, borderBrush);
            }
        }
    }

    // Event args for history item selection
    public class HistoryItemSelectedEventArgs : EventArgs
    {
        public HistoryItem SelectedItem { get; }

        public HistoryItemSelectedEventArgs(HistoryItem item)
        {
            SelectedItem = item;
        }
    }
}
