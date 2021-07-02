using System;
using System.Linq;
using System.Threading;
using System.IO;
using System.Threading.Tasks;

namespace Podlonia.Tasks
{
    // Generate deltas of connected devices
    public class MonitorDevices
    {
        const int TimeBetweenPollsMS = 5000;

        public static event Action<DriveInfo> DeviceInserted;
        public static event Action<DriveInfo> DeviceRemoved;
        public static async Task Monitoring( CancellationToken? ct = null )
        {
            var prevdrives = new DriveInfo[0];

            while ( true )
            {
                if ( ct is null )
                {
                    await Task.Delay( TimeBetweenPollsMS );
                }
                else
                {
                    await Task.Delay( TimeBetweenPollsMS, ct.Value );
                }

                if ( ct?.IsCancellationRequested ?? false ) return;

                prevdrives = PollForDrives( prevdrives, ct.GetValueOrDefault() );
            }
        }

        private static DriveInfo[] PollForDrives( DriveInfo[] prevdrives, CancellationToken ct )
        {
            var curdrives = DriveInfo.GetDrives();

            var removeddrives = prevdrives
                    .Where( cd =>
                        !curdrives.Any( pd => pd.Name == cd.Name ) );
            foreach ( var di in removeddrives )
            {
                Program.Log( $"Device {di.Name} removed.");
                DeviceRemoved?.Invoke( di );
            }

            var newdrives = curdrives
                    .Where( cd => 
                        !prevdrives.Any( pd => pd.Name == cd.Name ) );

            foreach ( var di in newdrives )
            {
                Program.Log( $"Device {di.Name} connected.");
                DeviceInserted?.Invoke( di );
            }

            return curdrives;
        }
    }
}
