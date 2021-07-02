using System.Collections.Generic;
using Podlonia.Models;
using System.Linq;
using System;

namespace Podlonia.Provider
{
    public static class EFProvider
    {
        public static long AddFeed( this PodloniaContext pc, RSSFeed feed )
        {
            pc.Feeds.Add( feed );
            pc.SaveChanges();

            return feed.Id;
        }

        public static long UpdateFeed( this PodloniaContext pc, RSSFeed feed )
        {
            pc.Feeds.Update( feed );
            pc.SaveChanges();

            return feed.Id;
        }

        public static RSSFeed GetFeed( this PodloniaContext pc, long feedid )
        {
            return pc.Feeds.FirstOrDefault( f => f.Id == feedid );
        }

        public static IList<RSSFeed> GetFeeds( this PodloniaContext pc )
        {
            return pc.Feeds.ToList();
        }

        public static IList<RSSFeed> GetFeeds( this PodloniaContext pc, string url )
        {
            return pc.Feeds.Where( f => f.Url == url ).ToList();
        }

        public static bool EnclosureDownloadDone( this PodloniaContext pc, RSSEnclosure enc )
        {
            return pc.Enclosures
                        .FirstOrDefault( ed => ed.Id == enc.Id )
                        .DownloadsComplete > 0;
        }

        public static bool FindEnclosue( this PodloniaContext pc, RSSEnclosure enc )
        {
            return pc.Enclosures.Any( e => e.RssGuid == enc.RssGuid && e.FeedId == enc.FeedId );
        }

        public static void AddEnclosure( this PodloniaContext pc, RSSEnclosure enc, string relpath )
        {
            pc.Enclosures.Add( enc );
            pc.SaveChanges();
        }

        public static void RemoveEnclosuresOlderThan( this PodloniaContext pc, long feedid, long maxagedays )
        {
            var removebefore = DateTime.Now - TimeSpan.FromDays( maxagedays );

            pc.Enclosures.RemoveRange( 
                pc.Enclosures.Where( e => 
                    e.FeedId == feedid
                    && !e.ForceDownload
                    && e.DownloadedAt < removebefore 
                    && e.PubDate < removebefore ) );
            pc.SaveChanges();
        }

        public static IList<DownloadItem> GetEnclosuresOlderThan( this PodloniaContext pc, long feedid, long maxagedays )
        {
            var before = DateTime.Now - TimeSpan.FromDays( maxagedays );

            var q = pc.Enclosures
                    .Where( e => e.FeedId == feedid
                        && e.DownloadedAt < before )
                    .Select( e => new {
                            Enclosure = e,
                            Feed = pc.Feeds.FirstOrDefault( f => f.Id == e.FeedId ),
                    } )
                    .ToArray()
                    .Select( e => new DownloadItem( e.Enclosure, e.Feed ) )
                    .ToList();
            return q;
        }

        public static IList<DownloadItem> GetEnclosures( this PodloniaContext pc, long feedid )
        {
            var q = pc.Enclosures
                    .Where( e => e.FeedId == feedid )
                    .Select( e => new {
                            Enclosure = e,
                            Feed = pc.Feeds.FirstOrDefault( f => f.Id == e.FeedId ),
                    } )
                    .ToArray()
                    .Select( e => new DownloadItem( e.Enclosure, e.Feed ) )
                    .ToList();
            return q;
        }

        public static IList<string> GetEnclosuresGuids( this PodloniaContext pc, long feedid )
        {
            var q = pc.Enclosures
                    .Where( e => e.FeedId == feedid )
                    .Select( e => e.RssGuid )
                    .ToList();
            return q;
        }

        public static IList<DownloadItem> GetEnclosuresMarkedDownloaded( this PodloniaContext pc )
        {
            return pc.Enclosures
                    .Where( e => e.DownloadsComplete > 0
                        && e.DownloadErrors < 50 )
                    .Select( e => new {
                            Enclosure = e,
                            Feed = pc.Feeds.FirstOrDefault( f => f.Id == e.FeedId ),
                    } )
                    .OrderByDescending( e => e.Enclosure.PubDate )
                    .ToArray()
                    .Select( e => new DownloadItem( e.Enclosure, e.Feed ) )
                    .ToList();
        }

        public static IList<DownloadItem> GetEnclosuresToDownload( this PodloniaContext pc )
        {
            var q = pc.Enclosures
                    .Where( e => e.DownloadsComplete <= 0
                            && e.DownloadErrors < 3 )
                    .Select( e => new {
                            Enclosure = e,
                            Feed = pc.Feeds.FirstOrDefault( f => f.Id == e.FeedId ),
                    } )
                    .OrderByDescending( e => e.Enclosure.PubDate )
                    .ToArray()
                    .Select( e => new DownloadItem( e.Enclosure, e.Feed ) )
                    .ToList();
            return q;
        }

        public static void MarkEnclosureDLAgain( this PodloniaContext pc, long encid )
        {
            var e = pc.Enclosures.FirstOrDefault( e2 => e2.Id == encid );
            e.DownloadsComplete = 0;
            pc.SaveChanges();
        }

        public static void MarkEnclosureDLDone( this PodloniaContext pc, long encid )
        {
            var e = pc.Enclosures.FirstOrDefault( e2 => e2.Id == encid );
            e.DownloadedAt = DateTime.Now;
            e.DownloadsComplete = 1;

            pc.SaveChanges();
        }

        public static void MarkEnclosureDLError( this PodloniaContext pc, long encid, int count = 1 )
        {
            var e = pc.Enclosures.FirstOrDefault( e2 => e2.Id == encid );
            e.DownloadErrors += count;
            pc.SaveChanges();
        }

        public static void ResetEnclosureDLError( this PodloniaContext pc, long encid )
        {
            var e = pc.Enclosures.FirstOrDefault( e2 => e2.Id == encid );
            e.DownloadErrors = 0;
            pc.SaveChanges();
        }

        public static void UpdateForceDownload( this PodloniaContext pc, long encid, bool force )
        {
            var e = pc.Enclosures.FirstOrDefault( e2 => e2.Id == encid );
            e.ForceDownload = force;
            pc.SaveChanges();
        }
        public static void UpdateForceSync( this PodloniaContext pc, long encid, bool force )
        {
            var e = pc.Enclosures.FirstOrDefault( e2 => e2.Id == encid );
            e.ForceSync = force;
            pc.SaveChanges();
        }
        public static void RemoveFeed( this PodloniaContext pc, long feedid )
        {
            pc.Feeds.Remove( pc.Feeds.FirstOrDefault( f => f.Id == feedid ) );
            pc.Enclosures.RemoveRange( pc.Enclosures.Where( e => e.FeedId == feedid ) );
            pc.SyncEnclosures.RemoveRange( pc.SyncEnclosures.Where( se => se.FeedId == feedid ) );
            pc.SaveChanges();
        }

         public static long AddNewSyncDevice( this PodloniaContext pc, SyncUnit newunit )
        {
            pc.SyncUnits.Add( newunit );
            pc.SaveChanges();

            return newunit.Id;
        }

         public static void DeleteSyncDevice( this PodloniaContext pc, long id )
        {
            pc.SyncUnits.Remove( pc.SyncUnits.FirstOrDefault( su => su.Id == id ) );
            pc.SaveChanges();
        }

       public static long AddEnclosureOnDevice( this PodloniaContext pc, SyncEnclosure item )
        {
            pc.SyncEnclosures.Add( item );
            pc.SaveChanges();
            return item.Id;
        }

        public static bool SyncDeviceKnown( this PodloniaContext pc, long id )
        {
            return pc.SyncUnits.Any( su => su.Id == id );
        }

        public static SyncUnit GetSyncDevice( this PodloniaContext pc, long id )
        {
            return pc.SyncUnits.FirstOrDefault( su => su.Id == id );
        }

        public static void UpdateSyncDeviceSettings( this PodloniaContext pc, SyncUnit sd )
        {
            pc.Update( sd );
            pc.SaveChanges();
        }

       public static IList<SyncEnclosure> GetEnclosuresOnDevice( this PodloniaContext pc, long deviceid, long feedid )
        {
            return pc.SyncEnclosures
                    .Where( se => se.DeviceId == deviceid && se.FeedId == feedid )
                    .ToList();
        }

        public static bool RemoveEnclosureOnDevice( this PodloniaContext pc, long senclid )
        {
            pc.SyncEnclosures.Remove( pc.SyncEnclosures.FirstOrDefault( se => se.Id == senclid ) );
            return pc.SaveChanges() > 0;
        }

       public static IList<DownloadItem> GetEnclosuresForSync( this PodloniaContext pc, long feedid, int maxcount )
        {
            var feedpath = pc.Feeds.FirstOrDefault( f => f.Id == feedid )?.LocalPath;
            var q = pc.Enclosures
                    .Where( e => e.DownloadsComplete > 0
                        && e.DownloadErrors < 50
                        && e.FeedId == feedid )
                    .OrderByDescending( e => e.PubDate )
                    .Take( maxcount )
                    .Union(
                        pc.Enclosures
                            .Where( e => e.DownloadsComplete > 0
                                && e.FeedId == feedid
                                && e.ForceSync )
                    )
                    .Select( e => new {
                            Enclosure = e,
                            Feed = pc.Feeds.FirstOrDefault( f => f.Id == e.FeedId ),
                    } );
            return q
                    .ToArray()
                    .Select( e => new DownloadItem( e.Enclosure, e.Feed ) )
                    .ToList();
        }
  }
}