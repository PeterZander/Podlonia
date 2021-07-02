using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Podlonia.Views
{
    public class DeviceSettingsWindow : Window
    {
        public DeviceSettingsWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}