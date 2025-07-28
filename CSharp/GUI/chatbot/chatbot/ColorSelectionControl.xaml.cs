using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WinFormsColorDialog = System.Windows.Forms.ColorDialog;
using WinFormsDialogResult = System.Windows.Forms.DialogResult;
using WpfColor = System.Windows.Media.Color;

namespace chatbot
{
    public partial class ColorSelectionControl : UserControl
    {
        // Event to notify when a color theme is selected
        public event EventHandler<ColorThemeSelectedEventArgs> ColorThemeSelected;

        public ColorSelectionControl()
        {
            InitializeComponent();
        }

        private void ClassicThemeButton_Click(object sender, RoutedEventArgs e)
        {
            OnColorThemeSelected(WpfColor.FromRgb(240, 240, 240), "Classic");
        }

        private void BlueThemeButton_Click(object sender, RoutedEventArgs e)
        {
            OnColorThemeSelected(WpfColor.FromRgb(227, 236, 255), "Blue");
        }

        private void GreenThemeButton_Click(object sender, RoutedEventArgs e)
        {
            OnColorThemeSelected(WpfColor.FromRgb(232, 245, 232), "Green");
        }

        private void PurpleThemeButton_Click(object sender, RoutedEventArgs e)
        {
            OnColorThemeSelected(WpfColor.FromRgb(243, 232, 255), "Purple");
        }

        private void OrangeThemeButton_Click(object sender, RoutedEventArgs e)
        {
            OnColorThemeSelected(WpfColor.FromRgb(255, 244, 230), "Orange");
        }

        private void DarkThemeButton_Click(object sender, RoutedEventArgs e)
        {
            OnColorThemeSelected(WpfColor.FromRgb(47, 47, 47), "Dark");
        }

        private void CustomColorButton_Click(object sender, RoutedEventArgs e)
        {
            var colorDialog = new WinFormsColorDialog();
            if (colorDialog.ShowDialog() == WinFormsDialogResult.OK)
            {
                var color = WpfColor.FromRgb(colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B);
                OnColorThemeSelected(color, "Custom");
            }
        }

        private void CloseColorPanelButton_Click(object sender, RoutedEventArgs e)
        {
            // Notify parent to close the panel
            OnColorPanelCloseRequested();
        }

        private void OnColorThemeSelected(WpfColor color, string themeName)
        {
            // Update color preview
            ColorPreview.Fill = new SolidColorBrush(color);
            
            // Raise event to notify parent
            ColorThemeSelected?.Invoke(this, new ColorThemeSelectedEventArgs(color, themeName));
        }

        // Event to notify when close is requested
        public event EventHandler ColorPanelCloseRequested;

        private void OnColorPanelCloseRequested()
        {
            ColorPanelCloseRequested?.Invoke(this, EventArgs.Empty);
        }

        // Method to update color preview (can be called from parent)
        public void UpdateColorPreview(WpfColor color)
        {
            ColorPreview.Fill = new SolidColorBrush(color);
        }
    }

    // Event arguments for color theme selection
    public class ColorThemeSelectedEventArgs : EventArgs
    {
        public WpfColor SelectedColor { get; }
        public string ThemeName { get; }

        public ColorThemeSelectedEventArgs(WpfColor selectedColor, string themeName)
        {
            SelectedColor = selectedColor;
            ThemeName = themeName;
        }
    }
}
