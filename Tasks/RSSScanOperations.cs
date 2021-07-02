using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Podlonia.ViewModels;
using Podlonia.Models;
using Podlonia.Provider;

namespace Podlonia.Tasks
{
    public static class RSSScanOperations
    {
        public static async Task StartPeriodicScan( MainWindowViewModel vm )
        {
            await Task.Delay( 8000 );

            while ( true )
            {
                if ( Program.Configuration.EnableScan )
                {
                    await FullScan( vm.Feeds.ToArray() );
                }

                if ( Program.Configuration.EnableDownload )
                {
                    await EnclosureDownloadOperations.FullDownload();
                }

                if ( Program.Configuration.EnableSync )
                {
                    await DeviceSyncOperations.FullSync();
                }

                await Task.Delay( TimeSpan.FromMinutes( Program.Configuration.MinutesBetweenBackgroundScans ) );
            }
        }

        public static async Task FullScan( IEnumerable<RSSFeed> feeds )
        {
            var opid = OperationSynchronization.StartOperation( OperationTypes.RSSScan );
            if ( opid is null ) return;

            var db = Program.CreateProvider();

            try
            {
                int ix = 0;

                var feedcount = feeds.Count();
                var prcbudgetperfeed = 100f / feedcount;

                foreach ( var feed in feeds )
                {
                    var prcbase = ( ix++ * 100 / feedcount );
                    OperationSynchronization.OperationProgress( opid, feed.Name, prcbase );

                    try
                    {
                        await ScanOneFeed( db, feed, opid, prcbase, prcbudgetperfeed );
                    }
                    catch ( WebException ex )
                    {
                        Program.Log( ex.Message );
                        Program.LogFeedError( feed, ex.Message );
                    }
                    catch ( Exception ex )
                    {
                        Program.Log( ex.Message );
                        Program.LogFeedError( feed, ex.Message );
                    }
                }
            }
            finally
            {
                OperationSynchronization.EndOperation( opid );
            }
        }

        private static async Task ScanOneFeed( 
                PodloniaContext db, 
                RSSFeed feed,
                OperationIdentification opid,
                float prcbase,
                float prcbudgetperfeed )
        {
            Program.Log( $"Scanning: {feed.Url}" );

            var data = await RSSRequest.GetRSSFeedData( new Uri( feed.Url ) );
            var progressprc = prcbudgetperfeed / data.Items.Count;
            var ix = 0;
            foreach ( var item in data.Items.OrderBy( it => it.PubDate ) )
            {
                try
                {
                    OperationSynchronization.OperationProgress( opid, feed.Name, (decimal)( prcbase + ( ix++ * progressprc ) ) );

                    await ScanOneItem( db, feed, item );
                }
                catch ( Exception ex )
                {
                    Program.Log( ex.Message );
                }
            }
        }

        static readonly string[] AcceptFormats = { "audio/mpeg", "audio/x-m4a", "audio/mp3", "audio/mp4", "audio/x-mp3" };
        static readonly string[] AcceptMP3Formats = { "application/octet-stream" };

        private static async Task ScanOneItem( PodloniaContext db, RSSFeed feed, RSSItem item )
        {
            var displayed = new HashSet<string>();

            var foundguids = db.GetEnclosuresGuids( feed.Id );
            var newenclosures = item
                                    .Enclosures
                                    .Where( e => !foundguids.Any( fg => fg == e.RssGuid )
                                        &&  (
                                                feed.StoredEnclosureMaxAgeDays == 0
                                                || e.PubDate > DateTime.Now - TimeSpan.FromDays( feed.StoredEnclosureMaxAgeDays )
                                            ) )
                                    .ToArray();

            foreach ( var enc in newenclosures )
            {
                try
                {
                    enc.FeedId = feed.Id;

                    //Program.Log( $"Checking: {enc.RssGuid}" );
                    await Task.Delay( 5 );

                    var ct = enc.ContentType.ToLower();
                    if ( !AcceptFormats.Any( form => ct.Contains( form )
                        && !( enc.FileName.ToLower().Contains( ".mp3" ) && AcceptMP3Formats.Any( fmt => ct.Contains( fmt ) ) ) ) )
                    {
                        var msg = $"Skipping: {enc.ContentType}";
                        if ( !displayed.Contains( msg ) )
                        {
                            Program.Log( msg );
                            displayed.Add( msg );
                        }
                        continue;
                    }

                    var relpath = feed.RelPath( enc );
                    Program.Log( $"Adding: {relpath}" );
                    db.AddEnclosure( enc, relpath );
                }
                catch ( Exception ex )
                {
                    Program.Log( ex.Message );
                }
            }
        }
    }
}
