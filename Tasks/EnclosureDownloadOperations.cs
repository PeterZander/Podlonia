using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Podlonia.Models;
using Podlonia.Provider;

namespace Podlonia.Tasks
{
    public static class EnclosureDownloadOperations
    {
        public static async Task FullDownload()
        {
            var opid = OperationSynchronization.StartOperation( OperationTypes.EnclosureDownload );
            if ( opid is null ) return;

            var db = Program.CreateProvider();

            try
            {
                var finished = db.GetEnclosuresMarkedDownloaded();
                foreach ( var item in finished )
                {
                    if ( !File.Exists( item.FullPathAndFileName ) )
                    {
                        db.MarkEnclosureDLAgain( item.Id );
                        db.MarkEnclosureDLError( item.Id, 1000 );
                    }
                }

                var paralelldownloads = Math.Max( 1, Program.Configuration.MaxParalellDownloads );
                var s = new SemaphoreSlim( paralelldownloads, paralelldownloads );

                var dlitems = db.GetEnclosuresToDownload();

                dlitems = dlitems
                    .Where( e =>
                            e.DownloadEnclosureMaxAgeDays == 0
                            || ( DateTime.Now - e.PubDate ).TotalDays < e.DownloadEnclosureMaxAgeDays
                            || e.ForceDownload )
                    .ToList();

                var ix = 0;

                foreach ( var item in dlitems )
                {
                    OperationSynchronization.OperationProgress( opid, item.RelFileName, ( ++ix * 100 / dlitems.Count() ) );

                    Program.Log( $"Downloading: {item.RelFileName}" );
                    await s.WaitAsync();
                    _ = Task.Run( async () => {
                        try
                        {
                            await Download( item );
                            lock ( s ) db.MarkEnclosureDLDone( item.Id );
                        }
                        catch ( Exception ex )
                        {
                            Program.Log( ex.Message );
                            lock ( s ) db.MarkEnclosureDLError( item.Id );
                        }
                        finally
                        {
                            s.Release();
                        }
                    } );
                }

                for ( int i = 0; i < paralelldownloads; ++i )
                {
                    await s.WaitAsync();
                }

                var feeds = db.GetFeeds();
                foreach( var feed in feeds )
                {
                    if ( feed.StoredEnclosureMaxAgeDays == 0 ) continue;

                    var filestoremove = db.GetEnclosuresOlderThan( feed.Id, feed.StoredEnclosureMaxAgeDays )
                            .Where( e => e.DownloadCount > 0
                                && !e.ForceDownload
                                && File.Exists( e.FullPathAndFileName ) );

                    foreach( var one in filestoremove )
                    {
                        Program.Log( $"Deleting: {one.FullPathAndFileName}" );
                        File.Delete( one.FullPathAndFileName );
                    }

                    db.RemoveEnclosuresOlderThan( feed.Id, feed.StoredEnclosureMaxAgeDays * 2 );
                }
            }
            finally
            {
                OperationSynchronization.EndOperation( opid );
            }
        }
        public static async Task<string> Download( DownloadItem item )
        {
            var filename = item.FullPathAndFileName;
            var dir = Path.GetDirectoryName( filename );

            if ( !Directory.Exists( dir ) )
            {
                Directory.CreateDirectory( dir );
            }

            bool alreadydownloaded = false;

            if ( item.Length != 0 && File.Exists( filename ) )
            {
                var info = new FileInfo( filename );
                if ( info.Length >= item.Length )
                {
                    alreadydownloaded = true;
                }
            }

            var uri = new Uri( item.Url );

            var dlinfo = new DownloadProgressInfo {
                    EnclosureId = item.Id,
                    FileName = filename,
                    Finished = true,
                    TransferedBytes = 0,
                    TotalBytes = 0,
                    ProgressPercent = 100,
                    Url = uri,
                    Error = false,
                };

            if ( alreadydownloaded )
            {
                Program.UpdateDownloadProgress( dlinfo );
                return filename;
            }

            dlinfo.Finished = false;
            dlinfo.ProgressPercent = 0;
            Program.UpdateDownloadProgress( dlinfo );

            var client = Program.CreateHttpClient();

            var error = false;

            try
            {
                var response = await client
                        .GetAsync( uri, completionOption: HttpCompletionOption.ResponseHeadersRead )
                        .ConfigureAwait( false );

                if ( !response.IsSuccessStatusCode )
                {
                    throw new Exception( $"File '{filename}' never downloaded." );
                }

                byte[] buf = new byte[4096];
                int readbytes;
                long bytes = 0;
                using ( var file = new FileStream( filename, FileMode.Create ) )
                {
                    var cl = response.Content.Headers.FirstOrDefault( h => h.Key == "Content-Length" ).Value;

                    var totalsize = cl == null ? 0 : long.Parse( cl.FirstOrDefault() ?? "100000" );
                    dlinfo.TotalBytes = totalsize;

                    var s = await response.Content.ReadAsStreamAsync();

                    var updtime = DateTime.Now;

                    while ( ( readbytes = await s.ReadAsync( buf, 0, buf.Length ) ) > 0 )
                    {
                        await file.WriteAsync( buf, 0, readbytes );
                        bytes += readbytes;

                        if ( ( DateTime.Now - updtime ).TotalSeconds > 2 )
                        {
                            updtime = DateTime.Now;
                            
                            dlinfo.TransferedBytes = bytes;
                            dlinfo.ProgressPercent = ( (float)bytes / totalsize ) * 100f;
                            Program.UpdateDownloadProgress( dlinfo );
                        }
                    }
                    file.Close();
                }

                if ( !File.Exists( filename ) )
                {
                    throw new Exception( $"File '{filename}' never downloaded." );
                }
            }
            catch ( Exception ex )
            {
                Program.Log( ex.Message );
                error = true;
                throw;
            }
            finally
            {
                dlinfo.Finished = true;
                dlinfo.ProgressPercent = 100;
                dlinfo.Error = error;
                Program.UpdateDownloadProgress( dlinfo );
            }

            return filename;
        }
    }
}
