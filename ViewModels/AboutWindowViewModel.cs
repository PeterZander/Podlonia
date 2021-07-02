using System;
using System.Collections.Generic;
using System.Text;
using System.Reactive;
using System.Configuration;
using Podlonia.Provider;
using Podlonia.Models;
using Avalonia.Controls;
using Avalonia.Collections;
using ReactiveUI;
using Podlonia.Views;
using System.Reflection;

namespace Podlonia.ViewModels
{
    public class AboutWindowViewModel : ViewModelBase
    {
        Window MyWindow;

        public AboutWindowViewModel( Window w )
        {
            MyWindow = w;
            ApplicationConfiguration = Program.Configuration;

            OkCommand = ReactiveCommand.Create( CloseAboutWindow );
        }

        public string Caption { get => "About"; }
        public string AppName { get => Program.ApplicationName; }

        public AppConfiguration ApplicationConfiguration { get; private set; }
        public string ConfigurationFileName { get => Program.ConfigurationFileFullPath; }

        public string Version { get =>
                Assembly
                    .GetEntryAssembly()
                    .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                    .InformationalVersion; 
        }
        
        public ReactiveCommand<Unit, Unit> OkCommand { get; }
        public void CloseAboutWindow()
        {
            MyWindow.Close();
        }
    }
}
