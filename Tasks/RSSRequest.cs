using System;
using System.Threading.Tasks;
using System.Xml;
using Podlonia.Models;

namespace Podlonia.Tasks
{
    public static class RSSRequest
    {
        public static async Task<RSSFeedData> GetRSSFeedData( Uri uri )
        {
            var wc = Program.CreateHttpClient();
            var hrm = await wc.GetAsync( uri ).ConfigureAwait( false );
            if ( !hrm.IsSuccessStatusCode )
            {
                throw new System.IO.IOException( $"HttpClient: {hrm.StatusCode}" );
            }

            var response = await hrm.Content.ReadAsStringAsync().ConfigureAwait( false );

            if ( response.IndexOf( '<' ) != 0 ) response = response.Substring( response.IndexOf( '<' ) );

            var result = new XmlDocument();
            result.LoadXml( response );
            var feed = new RSSFeedData( result );
            return feed;
        }
    }
}
