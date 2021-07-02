using System.Reactive;
using Avalonia.Controls;
using ReactiveUI;
using System.IO;
using System.Linq;
using System;
using System.Threading.Tasks;
using Avalonia.Controls.ApplicationLifetimes;

namespace Podlonia.ViewModels
{
    public class SettingsWindowViewModel : ViewModelBase
    {
        Window MyWindow;

        public SettingsWindowViewModel( Window w )
        {
            MyWindow = w;

            // Download directory
            DownloadDirectory = Program.Configuration.DownloadDirectory;

            if ( string.IsNullOrWhiteSpace( DownloadDirectory ) )
            {
                CanCancel = false;

                var path = Environment.GetFolderPath(
                            Environment.SpecialFolder.MyMusic, 
                            Environment.SpecialFolderOption.Create );
                if ( string.IsNullOrWhiteSpace( path ) ) path = ".";
                path = Path.GetFullPath( Program.ApplicationName, path );
                DownloadDirectory = path;
            }


            // Database file
            DatabaseFile = Program.Configuration.DatabaseFile;

            if ( string.IsNullOrWhiteSpace( DatabaseFile ) )
            {
                CanCancel = false;

                var file = Environment.GetFolderPath( 
                            Environment.SpecialFolder.ApplicationData, 
                            Environment.SpecialFolderOption.Create );
                if ( string.IsNullOrWhiteSpace( file ) ) file = ".";
                file = Path.GetFullPath( Program.ApplicationName, file );
                file = Path.GetFullPath( $"{Program.ApplicationName}.db", file );
                DatabaseFile = file;
            }

            MaxAgeForDownloadDays = Program.Configuration.MaxAgeForDownloadDays;
            MaxAgeForStorageDays = Program.Configuration.MaxAgeForStorageDays;
            MediaPlayer = Program.Configuration.MediaPlayer;

            OkCommand = ReactiveCommand.Create( OkChanges );
            CancelCommand = ReactiveCommand.Create( CancelChanges );
        }

        public string Caption { get => Program.ApplicationName; }
        int MaxAgeForDownloadDaysField;
        public int MaxAgeForDownloadDays
        { 
            get => MaxAgeForDownloadDaysField; 
            set => this.RaiseAndSetIfChanged( ref MaxAgeForDownloadDaysField, value );
        }
        int MaxAgeForStorageDaysField;
        public int MaxAgeForStorageDays
        { 
            get => MaxAgeForStorageDaysField; 
            set => this.RaiseAndSetIfChanged( ref MaxAgeForStorageDaysField, value );
        }
        string DownloadDirectoryField;
        public string DownloadDirectory
        { 
            get => DownloadDirectoryField; 
            set => this.RaiseAndSetIfChanged( ref DownloadDirectoryField, value );
        }
        string DatabaseFileField;
        public string DatabaseFile
        { 
            get => DatabaseFileField; 
            set => this.RaiseAndSetIfChanged( ref DatabaseFileField, value );
        }
        bool CanCancelField = true;
        public bool CanCancel
        {
            get => CanCancelField; 
            set => this.RaiseAndSetIfChanged( ref CanCancelField, value );
        }
        string MediaPlayerField;
        public string MediaPlayer
        { 
            get => MediaPlayerField; 
            set => this.RaiseAndSetIfChanged( ref MediaPlayerField, value );
        }

        public async Task BrowseDownloadDir()
        {
            if ( App.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop )
            {
                var fd = new OpenFolderDialog()
                {
                    Title = Program.ApplicationName,
                    Directory = DownloadDirectory,
                };
                var dresult = await fd.ShowAsync( desktop.MainWindow );

                if ( dresult is null ) return;
                DownloadDirectory = dresult;
            }
        }
        
        public async Task BrowseDatabaseFile()
        {
            if ( App.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop )
            {
                var fd = new OpenFileDialog()
                {
                    Title = Program.ApplicationName,
                    InitialFileName = DatabaseFile,
                    AllowMultiple = false,
                    Filters = new System.Collections.Generic.List<FileDialogFilter>()
                    {
                        new FileDialogFilter { Name = "SQLite", Extensions = { "db" } },
                        new FileDialogFilter { Name = "All", Extensions = { "*" } },
                    },
                };
                var dresult = await fd.ShowAsync( desktop.MainWindow );

                if ( dresult is null || dresult.Length <= 0 ) return;
                DatabaseFile = dresult[0];
            }
        }
        
        public async Task BrowseMediaPlayer()
        {
            if ( App.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop )
            {
                var fd = new OpenFileDialog()
                {
                    Title = Program.ApplicationName,
                    AllowMultiple = false,
                };
                var dresult = await fd.ShowAsync( desktop.MainWindow );

                if ( !dresult?.Any() ?? true ) return;
                MediaPlayer = dresult[0];
            }
        }

        public ReactiveCommand<Unit, Unit> OkCommand { get; }
        public ReactiveCommand<Unit, Unit> CancelCommand { get; }
        public void OkChanges()
        {
            Program.Configuration.NoAutomaticSerialization = true;
            try
            {
                Program.Configuration.DownloadDirectory = DownloadDirectory;
                Program.Configuration.DatabaseFile = DatabaseFile;
                Program.Configuration.MaxAgeForDownloadDays = MaxAgeForDownloadDays;
                Program.Configuration.MaxAgeForStorageDays = MaxAgeForStorageDays;
                Program.Configuration.MediaPlayer = MediaPlayer;
            }
            finally
            {
                Program.Configuration.NoAutomaticSerialization = false;
            }

            if ( !Directory.Exists( DownloadDirectory ) )
            {
                Directory.CreateDirectory( DownloadDirectory );
            }

            if ( !Directory.Exists( Path.GetDirectoryName( DatabaseFile ) ) )
            {
                Directory.CreateDirectory( Path.GetDirectoryName( DatabaseFile ) );
            }
            
            MyWindow.Close();
        }
        public void CancelChanges()
        {
            MyWindow.Close();
        }
    }
}
