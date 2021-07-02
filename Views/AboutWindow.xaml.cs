using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Podlonia.Views
{
    public class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}