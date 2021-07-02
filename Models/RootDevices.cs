using System;
using System.IO;
using Newtonsoft.Json;
using Avalonia.Controls;
using ReactiveUI;
using Avalonia;
using Podlonia.Views;
using Podlonia.ViewModels;
using System.Threading.Tasks;

namespace Podlonia.Models
{
    public class RootDevices: ReactiveObject
    {
        DriveInfo DriveInfoField;
        public DriveInfo DriveInfo
        { 
            get => DriveInfoField; 
            set => this.RaiseAndSetIfChanged( ref DriveInfoField, value );
        }
        bool IsSelectedField;
        public bool IsSelected
        { 
            get => IsSelectedField; 
            set
            {
                this.RaiseAndSetIfChanged( ref IsSelectedField, value );
                if ( value ) IsReadOnly = false;
            }
        }
        bool IsReadOnlyField;
        public bool IsReadOnly
        { 
            get => IsReadOnlyField;
            set => this.RaiseAndSetIfChanged( ref IsReadOnlyField, value );
        }

        public Thickness BorderThickness { get => IsReadOnly ? new Thickness( 0 ) : new Thickness( 1 ); }
        
        string DownloadDirectoryField;
        public string DownloadDirectory
        { 
            get => DownloadDirectoryField;
            set => this.RaiseAndSetIfChanged( ref DownloadDirectoryField, value );
        }
        public async Task ShowDeviceSettings( DevicesWindow w )
        {
            var dsw = new DeviceSettingsWindow();
            dsw.DataContext = new DeviceSettingsViewModel( dsw, DriveInfo );

            await dsw.ShowDialog( w );
        }
    }
}
