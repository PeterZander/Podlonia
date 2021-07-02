using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Podlonia.Provider;
using Podlonia.ViewModels;
using Podlonia.Views;
using System.Data.Common;
using System.Data.SQLite;
using Podlonia.Tasks;

namespace Podlonia
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow();
                desktop.MainWindow.DataContext = new MainWindowViewModel( desktop.MainWindow );
            }

            base.OnFrameworkInitializationCompleted();
        }

        void SwitchTheme()
        {
            /*
            var light = new StyleInclude(new Uri("resm:Styles?assembly=ControlCatalog"))
            {
                Source = new Uri("resm:Avalonia.Themes.Default.Accents.BaseLight.xaml?assembly=Avalonia.Themes.Default")
            };
            var dark = new StyleInclude(new Uri("resm:Styles?assembly=ControlCatalog"))
            {
                Source = new Uri("resm:Avalonia.Themes.Default.Accents.BaseDark.xaml?assembly=Avalonia.Themes.Default")
            };

            switch (themes.SelectedIndex)
            {
                default:
                case 0:
                    Styles[0] = light;
                    break;
                case 1:
                    Styles[0] = dark;
                    break;
            }
            */
        }
    }
}