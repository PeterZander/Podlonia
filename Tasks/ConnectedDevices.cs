using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Podlonia.Models;
using Podlonia.Provider;

namespace Podlonia.Tasks
{
    public static class ConnectedDevices
    {
        public static readonly string SyncedDeviceTagFile = $"{Program.ApplicationName}.id";
        static object SynchronizingInProgressLock = new object();
        static List<SyncDevice> OnlineDrives = new List<SyncDevice>();

        public static event Action<DriveInfo> DeviceInserted;
        public static event Action<DriveInfo> DeviceRemoved;

        public static SyncDevice[] Online
        {
            get
            {
                return OnlineDrives?.ToArray();
            }
        }

        static ConnectedDevices()
        {
            MonitorDevices.DeviceInserted += Monitoring_DeviceInserted;
            MonitorDevices.DeviceRemoved += Monitoring_DeviceRemoved;
        }

        public static async Task Monitoring( CancellationToken? ct = null )
        {
            await MonitorDevices.Monitoring( ct );
        }

        public static async Task DelayedResync()
        {
            await Task.Delay( 15000 );
            Resync();
        }

        public static void Resync()
        {
            var db = Program.CreateProvider();

            lock( SynchronizingInProgressLock )
            {
                var drives = DriveInfo.GetDrives();

                foreach ( var drive in OnlineDrives.ToArray() )
                {
                    if ( !drives.Any( d => d.RootDirectory.FullName == drive.Info.RootDirectory.FullName )
                        || IsDeviceSynced( drive.Info.RootDirectory.FullName ) < 0 )
                    {
                        try
                        {
                            OnlineDrives.Remove( drive );
                            DeviceRemoved?.Invoke( drive.Info );
                        }
                        catch( Exception ex )
                        {
                            Program.Log( ex.Message );
                        }
                    }
                }

                foreach ( var drive in drives )
                {
                    CheckDrive( drive, db );
                }
            }
        }

        static void CheckDrive( DriveInfo drive, PodloniaContext db )
        {
            try
            {
                var id = IsDeviceSynced( drive.RootDirectory.FullName );
                if ( id >= 0 )
                {
                    if ( !db.SyncDeviceKnown( id ) ) return;
                    if ( OnlineDrives.Any( d => d.Id == id ) ) return;

                    OnlineDrives.Add( new SyncDevice( id, drive ) );
                    DeviceInserted?.Invoke( drive );
                }
            }
            catch ( Exception ex )
            {
                Program.Log( ex.Message );
            }
        }

        public static long IsDeviceSynced( string path )
        {
            var filename = Path.GetFullPath( SyncedDeviceTagFile, path );

            if ( File.Exists( filename ) )
            {
                using ( var fs = new FileStream( filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite ) )
                {
                    using ( var br = new BinaryReader( fs ) )
                    {
                        var id = br.ReadInt64();
                        br.Close();
                        return id;
                    }
                }
            }

            return -1;
        }

        public static void MarkDeviceSynced( string path, long id )
        {
            var filename = Path.GetFullPath( SyncedDeviceTagFile, path );

            if ( File.Exists( filename ) ) return;

            using ( var fs = new FileStream( filename, FileMode.CreateNew, FileAccess.Write, FileShare.None ) )
            {
                using ( var br = new BinaryWriter( fs ) )
                {
                    br.Write( id );
                    br.Close();
                }
            }
        }

        public static void UnmarkDeviceSynced( string path )
        {
            var filename = Path.GetFullPath( SyncedDeviceTagFile, path );

            if ( !File.Exists( filename ) ) return;

            File.Delete( filename );
        }

        private static void Monitoring_DeviceInserted( DriveInfo di )
        {
            CheckDrive( di, Program.CreateProvider() );
        }

        private static void Monitoring_DeviceRemoved( DriveInfo di )
        {
            var removed = OnlineDrives.Where( d => d.Info.RootDirectory.FullName == di.RootDirectory.FullName );
            if ( !removed.Any() ) return;

            foreach( var drive in removed.ToArray() )
            {
                try
                {
                    OnlineDrives.Remove( drive );
                    DeviceRemoved?.Invoke( drive.Info );
                }
                catch( Exception ex )
                {
                    Program.Log( ex.Message );
                }
            }
        }
    }
}
