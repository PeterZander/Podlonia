using System.Reactive;
using Podlonia.Models;
using Avalonia.Controls;
using ReactiveUI;
using Podlonia.Tasks;
using System;
using System.Threading.Tasks;
using static Podlonia.Misc;
using Podlonia.Provider;

namespace Podlonia.ViewModels
{
    public class AddFeedWindowViewModel : ViewModelBase
    {
        Window MyWindow;

        public AddFeedWindowViewModel( Window w )
        {
            MyWindow = w;

            OkCommand = ReactiveCommand.Create( Ok );
            CancelCommand = ReactiveCommand.Create( CloseDevicesWindow );
        }

        public string Caption { get => "Add feed"; }
        RSSFeedData RSSFeedInfoField;
        public RSSFeedData RSSFeedInfo
        { 
            get => RSSFeedInfoField;
            set
            {
                this.RaiseAndSetIfChanged( ref RSSFeedInfoField, value );
                RSSFeedInfoValid = RSSFeedInfoField != null;

                if ( RSSFeedInfoValid )
                {
                    DownloadDirectory = MakeValidFileName( 
                            RSSFeedInfoField.Title.Replace( '\\', '_' ) );
                }
            }
        }

        bool RSSFeedInfoValidField = false;
        public bool RSSFeedInfoValid
        { 
            get => RSSFeedInfoValidField; 
            set => this.RaiseAndSetIfChanged( ref RSSFeedInfoValidField, value );
        }

        string FeedURLField;
        public string FeedURL
        { 
            get => FeedURLField;
            set => this.RaiseAndSetIfChanged( ref FeedURLField, value );
        }

        string DownloadDirectoryField;
        public string DownloadDirectory
        { 
            get => DownloadDirectoryField;
            set => this.RaiseAndSetIfChanged( ref DownloadDirectoryField, value );
        }

        public async Task GetFeedInfo()
        {
            RSSFeedInfo = await RSSRequest.GetRSSFeedData( new Uri( FeedURL ) );
        }
        public void ClearFeedInfo()
        {
            RSSFeedInfo = null;
        }
        public ReactiveCommand<Unit, Unit> OkCommand { get; }
        public void Ok()
        {
            var newfeed = new RSSFeed {
                Name = RSSFeedInfo.Title,
                Url = FeedURL,
                LocalPath = DownloadDirectory,
                Description = RSSFeedInfo.Description,
                Category = RSSFeedInfo.Category,
                ImageSource = RSSFeedInfo.ImageUrl,
                DownloadEnclosureMaxAgeDays = Program.Configuration.MaxAgeForDownloadDays,
                StoredEnclosureMaxAgeDays = Program.Configuration.MaxAgeForStorageDays,
            };
            Program.CreateProvider().AddFeed( newfeed );

            MyWindow.Close();
        }
        public ReactiveCommand<Unit, Unit> CancelCommand { get; }
        public void CloseDevicesWindow()
        {
            MyWindow.Close();
        }
    }
}
