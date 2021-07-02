using System;
using System.Xml;
using System.Collections.Generic;
using static Podlonia.Misc;

namespace Podlonia.Models
{
    public class RSSItem
    {
        /*
			<title>Planning an Offsite - Part 4</title>
			<link>http://www.manager-tools.com/2015/10/planning-offsite-part-4</link>
			<description>Part 4 of our guidance for how to plan an offsite meeting.</description>
			<enclosure type = "audio/mpeg" url= "https://www.manager-tools.com/system/files/podcast/mp3/manager-tools-2015-10-11.mp3" length= "26976256" />
            < pubDate > Sun, 11 Oct 2015 23:00:00 -0500</pubDate>
			<dc:creator>mauzenne</dc:creator>
			<itunes:author>Michael Auzenne and Mark Horstman</itunes:author>
			<itunes:summary>Part 4 of our guidance for how to plan an offsite meeting.</itunes:summary>
			<guid isPermaLink = "false" > 88E3B222-1C8E-460A-910C-7218D80E3AB8</guid>
        */

        public long Id { get; set; }
        public string Title { get; set; }
        public string RssGuid { get; set; }
        public string Link { get; set; }
        public string Description { get; set; }
        public DateTime PubDate { get; set; }

        public List<RSSEnclosure> Enclosures { get; set; } = new List<RSSEnclosure>();

        public RSSFeedData FeedData { get; set; }

        public RSSItem() {}
        public RSSItem( RSSFeedData feed, XmlNode itemnode )
        {
            FeedData = feed;

            var titlenode = itemnode.SelectSingleNode( "title" );
            if ( titlenode != null )
            {
                Title = titlenode.InnerText.Trim();
            }

            var descnode = itemnode.SelectSingleNode( "description" );
            if ( descnode != null )
            {
                Description = descnode.InnerText;
            }

            if ( titlenode == null && descnode != null )
            {
                Title = Description;
            }

            var linknode = itemnode.SelectSingleNode( "link" );
            if ( linknode != null )
            {
                Link = linknode.InnerText.Trim();
            }

            var pubdatenode = itemnode.SelectSingleNode( "pubDate" );
            if ( pubdatenode != null )
            {
                var dttxt = pubdatenode.InnerText;

                var dt = DateTimeRFC822.TryToParseDateTime( dttxt );
                PubDate = dt == null ? DateTime.UtcNow : dt.Value;
            }
            else
            {
                PubDate = DateTime.UtcNow;
            }

            var guidnode = itemnode.SelectSingleNode( "guid" );
            if ( guidnode != null )
            {
                RssGuid = guidnode.InnerText.Trim();
            }
            else
            {
                RssGuid = ToGuidHash( Title );
            }

            if ( Link is null || ( Link.Length == 0 && guidnode != null &&
                guidnode.Attributes.GetNamedItem( "isPermaLink" ) != null &&
                guidnode.Attributes.GetNamedItem( "isPermaLink" ).InnerText.Trim().ToLower().Equals( "true" ) ) )
            {
                Link = RssGuid;
            }

            var nodes = itemnode.SelectNodes( "enclosure" );

            if ( nodes != null )
            {
                int ix = 0;
                foreach ( XmlNode enc in nodes )
                {
                    try
                    {
                        var renc = new RSSEnclosure( this, enc, $"{RssGuid}{ix}" );
                        Enclosures.Add( renc );
                    }
                    catch ( Exception ex )
                    {
                        Program.Log( ex.Message );
                    }

                    ++ix;
                }
            }
        }
    }
}
