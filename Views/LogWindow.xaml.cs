using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Podlonia.Views
{
    public class LogWindow : Window
    {
        public LogWindow()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}