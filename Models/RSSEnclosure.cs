
using System;
using System.Linq;
using System.IO;
using System.Xml;
using System.ComponentModel.DataAnnotations.Schema;
using static Podlonia.Misc;

namespace Podlonia.Models
{
    public class RSSEnclosure
    {
        public long Id { get; set; }
        public long FeedId { get; set; }
        [Column( "Type" )]
        public string ContentType { get; set; }
        public string Url { get; set; }
        public long Length { get; set; }

        public string RssGuid { get; set; }
        public DateTime PubDate { get; set; }

        public long DownloadsComplete { get; set; }
        public DateTime DownloadedAt { get; set; }
        public long DownloadErrors { get; set; }

        public bool ForceDownload { get; set; }
        public bool ForceSync { get; set; }

        public RSSEnclosure() {}
        public RSSEnclosure( RSSItem item, XmlNode enc, string uid )
        {
            RssGuid = uid;
            PubDate = item.PubDate;
            FeedId = item.FeedData.Id;

            ContentType = enc.Attributes.GetNamedItem( "type" ).InnerText;
            Url = enc.Attributes.GetNamedItem( "url" ).InnerText;

            var len = enc.Attributes.GetNamedItem( "length" ).InnerText;
            try
            {
                if ( len.Length > 0 ) Length = long.Parse( len );
            }
            catch ( Exception ex )
            {
                Program.Log( ex.Message );
            }
        }
        
        [NotMapped]
        public string FileName
        {
            get
            {
                var uri = new Uri( Url );
                string ap = uri.AbsolutePath;
                string name = "";

                if ( ap.ToLower().EndsWith( ".mp3" ) && ContentType.Trim().ToLower().Equals( "audio/mpeg" ) )
                {
                    name = ap.Split( '/' ).Last();
                }
                else
                {
                    name = MakeValidFileName( $"{ToGuidHash( Id, FeedId, Url )}.mp3" );
                }

                return name;
            }
        }
    }
}
