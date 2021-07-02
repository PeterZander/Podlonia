using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace Podlonia
{
    public static class Misc
    {
        public static byte[] GetBytes( string str )
        {
            byte[] bytes = new byte[str.Length * sizeof( char )];
            System.Buffer.BlockCopy( str.ToCharArray(), 0, bytes, 0, bytes.Length );
            return bytes;
        }

        public static string ToGuidHash( params object[] objs )
        {
            using ( var sham = new SHA1Managed() )
            {
                return string.Concat( 
                        sham.ComputeHash( 
                            objs
                                .SelectMany( o => GetBytes( o.ToString() ) )
                                .ToArray()
                        )
                        .Select( b => b.ToString( "X2" ) )
                    ).Trim();
            }
        }
        public static string MakeValidFileName( string src )
        {
            var chars = Path.GetInvalidPathChars().Union( Path.GetInvalidFileNameChars() );
            string result = src;
            foreach (var one in chars) result = result.Replace( one, '_' );
            result = result.Replace( ':', '_' );
            result = result.Replace( '\\', '-' );
            result = result.Replace( '/', '-' );
            return result;
        }
        public static DateTime ParseSQLiteDT( string datetime )
        {
            DateTime result;

            string[] formats = { "yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd HH:mm:sszzz", "yyyy-MM-dd HH:mm:ss.fff", "yyyy-MM-dd HH:mm:ss.fffzzz" };
            var ok = DateTime.TryParseExact(
                datetime,
                formats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AllowLeadingWhite | DateTimeStyles.AllowTrailingWhite,
                out result );

            if ( ok ) return result;

            return DateTime.Parse( datetime );
        }
    }
}