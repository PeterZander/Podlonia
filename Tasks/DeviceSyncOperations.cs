using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Podlonia.Models;
using Podlonia.Provider;

namespace Podlonia.Tasks
{
    public static class DeviceSyncOperations
    {
        public static async Task FullSync()
        {
            var opid = OperationSynchronization.StartOperation( OperationTypes.EnclosureDownload );
            if ( opid is null ) return;

            var db = Program.CreateProvider();
            var feeds = db.GetFeeds();

            try
            {
                var devices = ConnectedDevices.Online;
                if ( devices is null ) return;

                int deviceix = 0;
                int devicecount = devices.Count();
                var prcbudgetperdevice = 100f / devicecount;

                foreach ( var device in devices )
                {
                    var du = db.GetSyncDevice( device.Id );
                    var devicepath = du?.Path;
                    if ( devicepath == null )
                    {
                        Program.Log( $"Sync: Device {device.Id} not found. Failed to get path. Skipping." );
                        continue;
                    }

                    devicepath = PatchDirSeparator( devicepath );

                    Program.Log( $"Syncing: {device.Info.RootDirectory}" );

                    try
                    {
                        var feedix = 0;
                        var prcbudgetperfeed = prcbudgetperdevice / feeds.Count();
                        
                        foreach ( var feed in feeds )
                        {
                            var prcbase = deviceix * prcbudgetperdevice + feedix * prcbudgetperfeed;

                            Program.Log( $"Sync: {feed.Name}" );

                            await SyncOneFeed( db, opid, device, devicepath, feed, prcbase, prcbudgetperfeed );

                            feedix++;
                        }
                    }
                    catch ( Exception ex )
                    {
                        Program.Log( ex.Message );
                    }

                    ++deviceix;
                }
            }
            finally
            {
                OperationSynchronization.EndOperation( opid );
            }
        }

        private static string PatchDirSeparator( string devicepath )
        {
            if ( Path.DirectorySeparatorChar == '\\' )
            {
                devicepath = devicepath.Replace( '/', '\\' );
            }

            if ( Path.DirectorySeparatorChar == '/' )
            {
                devicepath = devicepath.Replace( '\\', '/' );
            }

            return devicepath;
        }

        private static async Task SyncOneFeed( 
                PodloniaContext db,
                OperationIdentification opid,
                SyncDevice device, 
                string devicepath, 
                RSSFeed feed,
                float prcbase,
                float prcbudgetperfeed )
        {
            var ondevice = db.GetEnclosuresOnDevice( device.Id, feed.Id );
            var devicelimits = db.GetSyncDevice( device.Id );
            var latest = db.GetEnclosuresForSync( feed.Id, devicelimits.MaxFilesPerFeed );

            // Device limits
            if ( devicelimits.MaxAgeDays != 0 )
            {
                var now = DateTime.Now;

                latest = latest
                            .Where( di => !di.ForceSync
                                && ( now - di.PubDate ).TotalDays <= devicelimits.MaxAgeDays )
                            .ToList();
            }

            if ( devicelimits.MaxStorageSpacePerFeed != 0 )
            {
                long size = 0;

                var copy = latest.ToArray();
                latest = copy
                            .OrderByDescending( di => di.PubDate )
                            .TakeWhile( di => 
                                {
                                    size += di.Length;
                                    return size < devicelimits.MaxStorageSpacePerFeed;
                                } )
                            .Union( copy.Where( di => di.ForceSync ) )
                            .ToList();
            }

            // Remove old enclosures from device
            RemoveOldFilesFromDevice( db, device, ondevice, latest );

            // Transfer new enclosures to device
            // Sort in publication date order
            await TransferNewFilesToDevice( db, opid, device, devicepath, feed, ondevice, latest, prcbase, prcbudgetperfeed );
        }

        private static async Task TransferNewFilesToDevice( 
                PodloniaContext db,
                OperationIdentification opid,
                SyncDevice device,
                string devicepath,
                RSSFeed feed, 
                IList<SyncEnclosure> ondevice, 
                IList<DownloadItem> latest,
                float prcbase,
                float prcbudgetperfeed )
        {
            var totransfer = latest
                        .Where( enc => !ondevice.Any( ode => ode.EncId.Equals( enc.Id ) ) )
                        .OrderBy( enc => enc.PubDate );

            var ix = 0;
            var budgetpertransfer = prcbudgetperfeed / totransfer.Count();

            foreach ( var transenc in totransfer )
            {
                OperationSynchronization.OperationProgress( opid, feed.Name, (decimal)( prcbase + ix * budgetpertransfer ) );

                await TransferOneFileToDevice( db, device, devicepath, feed, transenc );

                ++ix;
            }
        }

        private static async Task TransferOneFileToDevice(
                PodloniaContext db,
                SyncDevice device,
                string devicepath,
                RSSFeed feed,
                DownloadItem transenc )
        {
            var filename = Path.GetFullPath( transenc.RelFileName, device.Info.RootDirectory + devicepath );

            try
            {
                if ( !File.Exists( transenc.FullPathAndFileName ) ) return;

                if ( !Directory.Exists( Path.GetDirectoryName( filename ) ) )
                {
                    Directory.CreateDirectory( Path.GetDirectoryName( filename ) );
                }

                if ( File.Exists( filename ) ) File.Delete( filename );

                Program.Log( $"Copying {filename}" );
                await CopyFileAsync( transenc.FullPathAndFileName, filename );

                var destwithoutroot = filename.Substring( device.Info.RootDirectory.FullName.Length );
                db.AddEnclosureOnDevice( new SyncEnclosure()
                    { 
                        DeviceId = device.Id,
                        EncId = transenc.Id,
                        FeedId = feed.Id,
                        FullName = destwithoutroot
                    } );
            }
            catch ( Exception ex )
            {
                Program.Log( ex.Message );
            }
        }

        private static void RemoveOldFilesFromDevice( 
                PodloniaContext db,
                SyncDevice device,
                IList<SyncEnclosure> ondevice,
                IList<DownloadItem> latest )
        {
            var toremove = ondevice.Where( syncitem => !latest.Any( le => le.Id.Equals( syncitem.EncId ) ) );
            foreach ( var removeenc in toremove )
            {
                var filename = Path.Combine( 
                        device.Info.RootDirectory.FullName, 
                        removeenc.FullName.TrimStart( Path.DirectorySeparatorChar ) );
                filename = PatchDirSeparator( filename );

                Program.Log( $"Removing {filename}" );
                if ( File.Exists( filename ) ) File.Delete( filename );

                db.RemoveEnclosureOnDevice( removeenc.Id );
            }
        }
        public static async Task CopyFileAsync( string srcfile, string destfile, CancellationToken? ct = null )
        {
            var fileoptions = FileOptions.Asynchronous | FileOptions.SequentialScan;
            var buffersize = 32768;

            using ( var srcstream = 
                new FileStream( srcfile, FileMode.Open, FileAccess.Read, FileShare.Read, buffersize, fileoptions ) )

            using (var deststream = 
                new FileStream( destfile, FileMode.CreateNew, FileAccess.Write, FileShare.None, buffersize, fileoptions ) )
            {
                if ( !ct.HasValue )
                {
                    await srcstream.CopyToAsync( deststream, buffersize )
                            .ConfigureAwait( false );
                }
                else
                {
                    await srcstream.CopyToAsync( deststream, buffersize, ct.Value )
                            .ConfigureAwait( false );
                }
            }
        }
    }
}
