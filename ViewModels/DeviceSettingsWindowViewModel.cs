using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using Podlonia.Models;
using Avalonia.Controls;
using ReactiveUI;
using System.IO;
using System.Collections.ObjectModel;
using Podlonia.Tasks;
using Podlonia.Provider;

namespace Podlonia.ViewModels
{
    public class DeviceSettingsViewModel : ViewModelBase
    {
        Window MyWindow;
        DriveInfo Drive;

        public DeviceSettingsViewModel( Window w, DriveInfo drive )
        {
            MyWindow = w;
            Drive = drive;

            OkCommand = ReactiveCommand.Create( Ok );
            CancelCommand = ReactiveCommand.Create( CloseDevicesWindow );

            var db = Program.CreateProvider();
            var deviceid = ConnectedDevices.IsDeviceSynced( drive.RootDirectory.FullName );
            SyncDevice = db.GetSyncDevice( deviceid );
        }

        public string Caption { get => $"Device settings {Drive.RootDirectory}"; }

        SyncUnit SyncDeviceField;
        public SyncUnit SyncDevice
        { 
            get => SyncDeviceField; 
            set => this.RaiseAndSetIfChanged( ref SyncDeviceField, value );
        }

        public ReactiveCommand<Unit, Unit> OkCommand { get; }
        public void Ok()
        {
            var db = Program.CreateProvider();
            db.UpdateSyncDeviceSettings( SyncDevice );

            MyWindow.Close();
        }
        public ReactiveCommand<Unit, Unit> CancelCommand { get; }
        public void CloseDevicesWindow()
        {
            MyWindow.Close();
        }
    }
}
