using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Podlonia.Views
{
    public class FeedSettingsWindow : Window
    {
        public FeedSettingsWindow()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}