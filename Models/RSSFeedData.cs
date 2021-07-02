using System;
using System.Collections.Generic;
using System.Xml;

namespace Podlonia.Models
{
    public class RSSFeedData
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string Link { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }

        public List<RSSItem> Items { get; set; } = new List<RSSItem>();

        public RSSFeedData() {}
        public RSSFeedData( XmlDocument doc )
        {
            var titlenode = doc.SelectSingleNode( "//title" );
            if ( titlenode != null ) Title = titlenode.InnerText;

            var linknode = doc.SelectSingleNode( "//link" );
            if ( linknode != null ) Link = linknode.InnerText;

            var categorynode = doc.SelectSingleNode( "//category" );
            if ( categorynode != null ) Category = linknode.InnerText;

            var imagenode = doc.SelectSingleNode( "//image" );
            if ( imagenode != null ) 
            {
                var imageurlnode = imagenode.SelectSingleNode( "//url" );
                if ( imageurlnode != null ) 
                {
                    ImageUrl = imageurlnode.InnerText;
                }
            }

            var itemnodes = doc.SelectNodes( "descendant::item" );

            foreach ( XmlNode one in itemnodes )
            {
                try
                {
                    var item = new RSSItem( this, one );
                    Items.Add( item );
                }
                catch ( Exception ex )
                {
                    Program.Log( ex.Message );
                }
            }
        }
    }
}
