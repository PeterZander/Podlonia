using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Diagnostics;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Podlonia.ViewModels;

namespace Podlonia.Views
{
    public class MessageWindow : Window
    {
        [Flags]
        public enum Buttons 
        {
            Ok = 0x01,
            Cancel = 0x02,
            Ignore = 0x04,
            Abort = 0x08,
            Retry = 0x10,
        }
        public MessageWindow()
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

        public static async Task<Buttons> Show( string message, string caption = "", Buttons buttons = Buttons.Ok, Bitmap icon = null )
        {
            if ( App.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop )
            {
                var w = new MessageWindow();
                var wm = new MessageWindowViewModel( w )
                {
                    Caption = caption,
                    Message = message,
                    ShowButtons = buttons,
                };
                w.DataContext = wm;

                await w.ShowDialog( desktop.MainWindow );
                return wm.ClickedButton;
            }

            return Buttons.Cancel;
        }
    }
}