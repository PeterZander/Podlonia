using System.Reactive;
using Podlonia.Models;
using Avalonia.Controls;
using ReactiveUI;
using Podlonia.Views;
using Avalonia.Controls.ApplicationLifetimes;
using System.Collections.ObjectModel;
using Podlonia.Tasks;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Podlonia.Provider;
using System;

namespace Podlonia.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        Window MyWindow;
        LogWindow TheLogWindow;

        bool AppIsClosing = false;

        public AppConfiguration ApplicationConfiguration { get; private set; }

        public PodloniaContext UIProvider { get => Program.CreateProvider(); }

        public MainWindowViewModel( Window w )
        {
            MyWindow = w;
            ApplicationConfiguration = Program.Configuration;

            DevicesWindowCommand = ReactiveCommand.CreateFromTask( ShowDevicesWindow );
            EventWindowCommand = ReactiveCommand.Create( ShowEventLogWindow );
            SettingsWindowCommand = ReactiveCommand.Create( ShowSettingsWindow );
            LicencesWindowCommand = ReactiveCommand.Create( ShowLicencesWindow );
            AboutWindowCommand = ReactiveCommand.Create( ShowAboutWindow );

            ShowDeviceSettingsCommand = ReactiveCommand.CreateFromTask<DriveInfo>( ShowDeviceSettings );

            StartRSSScanCommand = ReactiveCommand.CreateFromTask( StartRSSScan );
            StartDownloadCommand = ReactiveCommand.Create( StartDownload );
            StartSyncCommand = ReactiveCommand.Create( StartSync );

            ToggleEnableScanCommand  = ReactiveCommand.Create( ToggleEnableScan );
            ToggleEnableDownloadCommand = ReactiveCommand.Create( ToggleEnableDownload );
            ToggleEnableSyncCommand = ReactiveCommand.Create( ToggleEnableSync );

            OpenFileCommand = ReactiveCommand.Create( OpenMediaFile );
            ToggleForceDownloadCommand  = ReactiveCommand.Create( ToggleForceDownload );
            ToggleForceSyncCommand  = ReactiveCommand.Create( ToggleForceSync );
            FeedSettingsCommand = ReactiveCommand.CreateFromTask<RSSFeed>( FeedSettingsWindow );
            DeleteFeedCommand = ReactiveCommand.CreateFromTask<RSSFeed>( DeleteFeed );
            ResetErrorOnEnclosureDownloadCommand = ReactiveCommand.Create( ResetErrorOnDownloadItem );
            DeleteDownloadedFilesCommand = ReactiveCommand.Create( DeleteDownloadedFiles );

            TheLogWindow = new LogWindow();
            TheLogWindow.DataContext = new LogWindowViewModel( TheLogWindow );
            TheLogWindow.Closing += ( s, e ) =>
            {
                ((Window)s).Hide();
                e.Cancel = !AppIsClosing;
            };            
            MyWindow.Closing += ( s, e ) =>
            {
                AppIsClosing = true;
                TheLogWindow?.Close();
                TheLogWindow = null;
            };          

            Program.NewFeedError += ( feed, msg )  => 
            {
                feed.LatestError = msg;
            };

            OperationSynchronization.OperationStarting += oi => ShowStatusBar = true;
            OperationSynchronization.OperationEnding += oi => 
            {
                ShowStatusBar = false;
                UpdateFeeds();
            };
            OperationSynchronization.OperationProgressing += ( oi, msg, prc ) =>
            {
                StatusBarMessage = msg;
                StatusBarProgress = prc;
            };

            Program.DownloadProgress += dpi => 
            {
                var upd = SelectedFeedDownloadItems.Where( di => di.Id == dpi.EnclosureId );
                foreach( var one in upd )
                {
                    if ( dpi.Finished )
                    {
                        ++one.Info.DownloadCount;
                    }

                    one.DownloadProgressInfo.Finished = dpi.Finished;
                    one.DownloadProgressInfo.Error = dpi.Error;
                    one.DownloadProgressInfo.FileName = dpi.FileName;
                    one.DownloadProgressInfo.Url = dpi.Url;
                    one.DownloadProgressInfo.ProgressPercent = dpi.ProgressPercent;
                }
            };

            _ = TestConfiguration();

            ConnectedDevices.DeviceInserted += async di =>
            {
                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync( 
                            () => OnlineDevices.Add( di ) );
                await Task.Delay( 5000 );
                await DeviceSyncOperations.FullSync();
            };
            ConnectedDevices.DeviceRemoved += async di =>
            {
                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync( 
                            () => OnlineDevices.Remove( 
                                    OnlineDevices.FirstOrDefault( d => 
                                        d.RootDirectory.FullName == di.RootDirectory.FullName ) ) );
            };
            _ = ConnectedDevices.Monitoring();
            _ = RSSScanOperations.StartPeriodicScan( this );
        }

        async Task TestConfiguration()
        {
        again:
            await Task.Delay( 1000 );

            if ( string.IsNullOrWhiteSpace( Program.Configuration.DatabaseFile )
                || string.IsNullOrWhiteSpace( Program.Configuration.DownloadDirectory )
                || !File.Exists( Program.Configuration.DatabaseFile )
                || !Directory.Exists( Program.Configuration.DownloadDirectory )  )
            {
                if ( App.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop )
                {
                    var w = new SettingsWindow();
                    w.DataContext = new SettingsWindowViewModel( w );

                    await w.ShowDialog( desktop.MainWindow );
                    goto again;
                }
            }

            UpdateFeeds();
        }

        public ObservableCollection<RSSFeed> Feeds { get; set; } = new ObservableCollection<RSSFeed>();

        void UpdateFeeds()
        {
            var allfeeds = UIProvider.GetFeeds().ToArray();
            var removed = Feeds
                            .Where( f => !allfeeds.Any( af => af.Id == f.Id ) )
                            .ToArray();
            var added = allfeeds
                            .Where( f => !Feeds.Any( af => af.Id == f.Id ) )
                            .ToArray();

            foreach( var one in removed )
            {
                Feeds.Remove( one );
            }
                    
            foreach( var one in added )
            {
                Feeds.Add( one );
            }
        }

        bool ShowStatusBarField;
        public bool ShowStatusBar
        { 
            get => ShowStatusBarField;
            set => this.RaiseAndSetIfChanged( ref ShowStatusBarField, value );
        }

        decimal StatusBarProgressField;
        public decimal StatusBarProgress
        { 
            get => StatusBarProgressField; 
            set => this.RaiseAndSetIfChanged( ref StatusBarProgressField, value );
        }
        string StatusBarMessageField;
        public string StatusBarMessage
        { 
            get => StatusBarMessageField; 
            set => this.RaiseAndSetIfChanged( ref StatusBarMessageField, value );
        }

        private RSSFeed FeedsSelectedItemField;
        public RSSFeed FeedsSelectedItem
        { 
            get
            {
                return FeedsSelectedItemField;
            }
            set
            {
                FeedsSelectedItemField = value;
                SelectedFeedDownloadItems.Clear();

                if ( FeedsSelectedItemField is null ) return;

                var enclosures = UIProvider
                        .GetEnclosures( FeedsSelectedItemField.Id )
                        .OrderByDescending( e => e.PubDate );

                foreach( var enclosure in enclosures )
                {
                    SelectedFeedDownloadItems.Add( new SelectedFeedItem {
                        Id = enclosure.Id,
                        Info = enclosure,
                        DownloadProgressInfo = new DownloadProgressInfo
                        {
                            EnclosureId = enclosure.Id,
                            Error = false,
                            FileName = "",
                            Finished = true,
                            ProgressPercent = 0f,
                            TotalBytes = 0,
                            TransferedBytes = 0,
                            Url = null,
                        },
                    } );
                }
            }
         }

        public ObservableCollection<DriveInfo> OnlineDevices { get; set; } = new ObservableCollection<DriveInfo>();

        public ObservableCollection<SelectedFeedItem> SelectedFeedDownloadItems { get; set; } = new ObservableCollection<SelectedFeedItem>();
        public ObservableCollection<SelectedFeedItem> SelectedFeedSelectedDownloadItems { get; set; } = new ObservableCollection<SelectedFeedItem>();

        public async Task ShowAddFeedWindow()
        {
            if ( App.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop )
            {
                var w = new AddFeedWindow();
                w.DataContext = new AddFeedWindowViewModel( w );

                await w.ShowDialog( desktop.MainWindow );

                UpdateFeeds();
            }
        }
        public ReactiveCommand<Unit, Unit> EventWindowCommand { get; }
        public void ShowEventLogWindow()
        {
            TheLogWindow.Show();
        }

        public ReactiveCommand<Unit, Unit> LicencesWindowCommand { get; }
        public void ShowLicencesWindow()
        {
            if ( App.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop )
            {
                var w = new LicencesWindow();
                //w.DataContext = new AboutWindowViewModel( w );

                w.ShowDialog( desktop.MainWindow );
            }
        }

        public ReactiveCommand<Unit, Unit> AboutWindowCommand { get; }
        public void ShowAboutWindow()
        {
            if ( App.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop )
            {
                var w = new AboutWindow();
                w.DataContext = new AboutWindowViewModel( w );

                w.ShowDialog( desktop.MainWindow );
            }
        }

        public ReactiveCommand<Unit,Unit> OpenFileCommand { get; }
        public void OpenMediaFile()
        {
            var items = SelectedFeedSelectedDownloadItems.Select( one => $"file://{Uri.EscapeDataString( one.Info?.FullPathAndFileName )}" );
            System.Diagnostics.Process.Start( Program.Configuration.MediaPlayer, items );
        }

        public ReactiveCommand<Unit,Unit> ToggleForceDownloadCommand { get; }
        public void ToggleForceDownload()
        {
            foreach( var sfi in SelectedFeedSelectedDownloadItems )
            {
                var state = !sfi.Info.ForceDownload;
                UIProvider.UpdateForceDownload( sfi.Id, state );
                sfi.Info.ForceDownload = state;
            }
        }
        public ReactiveCommand<Unit,Unit> ToggleForceSyncCommand { get; }

         public void ToggleForceSync()
        {
            foreach( var sfi in SelectedFeedSelectedDownloadItems )
            {
                var state = !sfi.Info.ForceSync;
                UIProvider.UpdateForceSync( sfi.Id, state );
                sfi.Info.ForceSync = state;
            }
        }

        public ReactiveCommand<RSSFeed,Unit> FeedSettingsCommand { get; }

        public async Task FeedSettingsWindow( RSSFeed feed )
        {
            if ( App.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop )
            {
                var w = new FeedSettingsWindow();
                w.DataContext = new FeedSettingsWindowViewModel( w, feed );

                await w.ShowDialog( desktop.MainWindow );
            }
        }
        public ReactiveCommand<RSSFeed,Unit> DeleteFeedCommand { get; }

         public async Task DeleteFeed( RSSFeed feed )
         {
            var reply = await MessageWindow.Show( 
                    "Do you want to remove this feed?\n"
                    + "Downloaded files and synced files on devices will not be removed.",
                    feed.Name,
                    MessageWindow.Buttons.Ok | MessageWindow.Buttons.Cancel );

            if ( reply != MessageWindow.Buttons.Ok ) return;

            UIProvider.RemoveFeed( feed.Id );
            UpdateFeeds();
        }
        public ReactiveCommand<Unit,Unit> ResetErrorOnEnclosureDownloadCommand { get; }

        public void ResetErrorOnDownloadItem()
        {
            foreach( var sfi in SelectedFeedSelectedDownloadItems )
            {
                UIProvider.ResetEnclosureDLError( sfi.Id );
                sfi.Info.DownloadErrorCount = 0;
            }
        }   
        public ReactiveCommand<Unit,Unit> DeleteDownloadedFilesCommand { get; }

        public void DeleteDownloadedFiles()
        {
            foreach( var sfi in SelectedFeedSelectedDownloadItems )
            {
                File.Delete( sfi.Info.FullPathAndFileName );
                sfi.Info.DownloadCount = 0;
            }
        }   
        public ReactiveCommand<Unit,Unit> ToggleEnableScanCommand { get; }

        public void ToggleEnableScan()
        {
            ApplicationConfiguration.EnableScan = !ApplicationConfiguration.EnableScan;
        }   
        public ReactiveCommand<Unit,Unit> ToggleEnableDownloadCommand { get; }

        public void ToggleEnableDownload()
        {
            ApplicationConfiguration.EnableDownload = !ApplicationConfiguration.EnableDownload;
        }   
        public ReactiveCommand<Unit,Unit> ToggleEnableSyncCommand { get; }
        public void ToggleEnableSync()
        {
            ApplicationConfiguration.EnableSync = !ApplicationConfiguration.EnableSync;
        }   
        public ReactiveCommand<Unit, Unit> SettingsWindowCommand { get; }
        public void ShowSettingsWindow()
        {
            if ( App.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop )
            {
                var w = new SettingsWindow();
                w.DataContext = new SettingsWindowViewModel( w );

                w.ShowDialog( desktop.MainWindow );
            }
        }

        public ReactiveCommand<Unit, Unit> DevicesWindowCommand { get; }
        public async Task ShowDevicesWindow()
        {
            if ( App.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop )
            {
                var w = new DevicesWindow();
                w.DataContext = new DevicesWindowViewModel( w );

                await w.ShowDialog( desktop.MainWindow );
                Tasks.ConnectedDevices.Resync();
            }
        }

        public ReactiveCommand<DriveInfo, Unit> ShowDeviceSettingsCommand { get; }
        public async Task ShowDeviceSettings( DriveInfo di )
        {
            var dsw = new DeviceSettingsWindow();
            dsw.DataContext = new DeviceSettingsViewModel( dsw, di );

            await dsw.ShowDialog( MyWindow );
        }

        public ReactiveCommand<Unit, Unit> StartRSSScanCommand { get; }
        public async Task StartRSSScan()
        {
            await RSSScanOperations.FullScan( Feeds.ToArray() );
            UpdateFeeds();
        }
        public ReactiveCommand<Unit, Unit> StartDownloadCommand { get; }
        public void StartDownload()
        {
            _ = EnclosureDownloadOperations.FullDownload();
        }
        public ReactiveCommand<Unit, Unit> StartSyncCommand { get; }
        public void StartSync()
        {
            _ = DeviceSyncOperations.FullSync();
        }
    }
}

