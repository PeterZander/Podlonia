using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Avalonia;
using Avalonia.Diagnostics;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.ReactiveUI;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Podlonia.Models;

namespace Podlonia
{
    class Program
    {
        public const string ApplicationName = "Podlonia";
        public static readonly string ConfigurationFileName = $"{ApplicationName}.config";
        public static AppConfiguration Configuration { get; private set; }

        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        public static void Main(string[] args)
        {
            ServicePointManager.ServerCertificateValidationCallback -= CB;
            ServicePointManager.ServerCertificateValidationCallback += CB;

            ReadConfiguration();
            
            using( var migrate = new PodloniaContext() )
            {
                migrate.Database.Migrate();
            }

            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        } 
        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .LogToTrace()
                .UsePlatformDetect()
                .UseReactiveUI();

        public static PodloniaContext CreateProvider()
        {
            return new PodloniaContext();
        }

        public static event Action<string> NewLogLine;
        public static event Action<RSSFeed,string> NewFeedError;
        public static string ConfigurationFileFullPath
        {
            get
            {
                var cfgpath = Environment.GetFolderPath( 
                                Environment.SpecialFolder.ApplicationData,
                                Environment.SpecialFolderOption.Create );
                cfgpath = Path.GetFullPath(
                                Program.ApplicationName,
                                cfgpath );
                if ( !Directory.Exists( cfgpath ) ) Directory.CreateDirectory( cfgpath );

                var cfgfile = Path.GetFullPath(
                                ConfigurationFileName,
                                cfgpath );

                return cfgfile;
            }
        }
        internal static void ReadConfiguration()
        {
            // AppConfig file
            if ( !File.Exists( ConfigurationFileFullPath ) )
            {
                Configuration = new AppConfiguration();
            }
            else
            {
                LoadConfiguration();
            }

            Configuration.PropertyChanged += ( s, e ) => SaveConfiguration();
        }
        public static void LoadConfiguration()
        {
            Configuration = JsonConvert.DeserializeObject<AppConfiguration>(
                    File.ReadAllText( ConfigurationFileFullPath ) );
        }
        public static void SaveConfiguration()
        {
            if ( Configuration.NoAutomaticSerialization ) return;

            lock ( Configuration )
            {
                var doc = JsonConvert.SerializeObject( Configuration, Formatting.Indented );
                File.WriteAllText(
                    ConfigurationFileFullPath, 
                    doc );
            }
        }

        public static void Log( string text )
        {
            Avalonia.Threading.Dispatcher.UIThread.InvokeAsync( () => NewLogLine?.Invoke( text ) );
        }
        public static void LogFeedError( RSSFeed feed, string text )
        {
            Avalonia.Threading.Dispatcher.UIThread.InvokeAsync( () => NewFeedError?.Invoke( feed, text ) );
        }
        static bool CB( object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors )
        {
            return true;
        }
        public static event Action<DownloadProgressInfo> DownloadProgress;
        public static void UpdateDownloadProgress( DownloadProgressInfo info )
        {
            Avalonia.Threading.Dispatcher.UIThread.InvokeAsync( () => DownloadProgress?.Invoke( info ) );
        }

        static HttpClient SharedHttpClient;
        static object SharedHttpClientLock = new object();
        public static HttpClient CreateHttpClient()
        {
            lock ( SharedHttpClientLock )
            {
                if ( !( SharedHttpClient is null ) ) return SharedHttpClient;

                SharedHttpClient = new HttpClient
                {
                    Timeout = TimeSpan.FromSeconds( 20 )
                };
                SharedHttpClient.DefaultRequestHeaders.Add( "User-Agent",
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/83.0.4103.116 Safari/537.36" );
                return SharedHttpClient;
            }
        }
    }
}
