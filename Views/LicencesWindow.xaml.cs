using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.IO;

namespace Podlonia.Views
{
    public class LicencesWindow : Window
    {
        public LicencesWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public string Text 
        {
            get
            {
                using( var resource = this.GetType().Assembly.GetManifestResourceStream( "Podlonia.Resources.licences.txt" ) )
                using ( var reader = new StreamReader( resource ) )
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}