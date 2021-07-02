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
    public class DevicesWindowViewModel : ViewModelBase
    {
        Window MyWindow;

        public DevicesWindowViewModel( Window w )
        {
            MyWindow = w;

            OkCommand = ReactiveCommand.Create( Ok );
            CancelCommand = ReactiveCommand.Create( CloseDevicesWindow );

            IEnumerable<DriveInfo> drives = DriveInfo
                                .GetDrives()
                                .Where( d => d.DriveType != DriveType.Ram );

            var db = Program.CreateProvider();

            foreach ( var drive in drives )
            {
                var deviceid = ConnectedDevices.IsDeviceSynced( drive.RootDirectory.FullName );
                var issynched = deviceid >= 0;

                RootPaths.Add( new RootDevices
                { 
                    DriveInfo = drive,
                    IsSelected = issynched,
                    IsReadOnly = issynched,
                    DownloadDirectory = issynched 
                            ? db.GetSyncDevice( deviceid )?.Path
                            : $"{Path.DirectorySeparatorChar}Podcasts{Path.DirectorySeparatorChar}"
                } );
            }
        }

        public string Caption { get => "Connected devices"; }
        public string AppName { get => Program.ApplicationName; }

        public IList<RootDevices> RootPaths { get; set; } = new ObservableCollection<RootDevices>();

        public AppConfiguration ApplicationConfiguration { get; private set; }
        public string ConfigurationFileName { get => Program.ConfigurationFileFullPath; }

        public ReactiveCommand<Unit, Unit> OkCommand { get; }
        public void Ok()
        {
            var db = Program.CreateProvider();

            foreach ( var one in RootPaths )
            {
                var deviceid = ConnectedDevices.IsDeviceSynced( one.DriveInfo.RootDirectory.FullName );

                if ( deviceid < 0 && one.IsSelected )
                {
                    var id = db.AddNewSyncDevice( new SyncUnit
                    {
                        Path = one.DownloadDirectory,
                        MaxFilesPerFeed = 7,
                    } );
                    ConnectedDevices.MarkDeviceSynced( one.DriveInfo.RootDirectory.FullName, id );
                }

                if ( deviceid >= 0 && !one.IsSelected )
                {
                    ConnectedDevices.UnmarkDeviceSynced( one.DriveInfo.RootDirectory.FullName );
                    db.DeleteSyncDevice( deviceid );
                }
                
            }
            MyWindow.Close();
        }
        public ReactiveCommand<Unit, Unit> CancelCommand { get; }
        public void CloseDevicesWindow()
        {
            MyWindow.Close();
        }
    }
}
