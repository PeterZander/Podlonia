using System.Reactive;
using Avalonia.Controls;
using ReactiveUI;
using Podlonia.Models;
using Podlonia.Provider;

namespace Podlonia.ViewModels
{
    public class FeedSettingsWindowViewModel : ViewModelBase
    {
        readonly Window MyWindow;
        readonly RSSFeed MyFeed;

        public FeedSettingsWindowViewModel( Window w, RSSFeed feed )
        {
            MyWindow = w;
            MyFeed = feed;

            MaxAgeForDownloadDays = MyFeed.DownloadEnclosureMaxAgeDays;
            MaxAgeForStorageDays = MyFeed.StoredEnclosureMaxAgeDays;

            OkCommand = ReactiveCommand.Create( OkChanges );
            CancelCommand = ReactiveCommand.Create( CancelChanges );
        }

        public string Caption { get => Program.ApplicationName; }
        long MaxAgeForDownloadDaysField;
        public long MaxAgeForDownloadDays
        { 
            get => MaxAgeForDownloadDaysField; 
            set => this.RaiseAndSetIfChanged( ref MaxAgeForDownloadDaysField, value );
        }
        long MaxAgeForStorageDaysField;
        public long MaxAgeForStorageDays
        { 
            get => MaxAgeForStorageDaysField; 
            set => this.RaiseAndSetIfChanged( ref MaxAgeForStorageDaysField, value );
        }
        
        public ReactiveCommand<Unit, Unit> OkCommand { get; }
        public ReactiveCommand<Unit, Unit> CancelCommand { get; }
        public void OkChanges()
        {
            MyFeed.DownloadEnclosureMaxAgeDays = MaxAgeForDownloadDays;
            MyFeed.StoredEnclosureMaxAgeDays = MaxAgeForStorageDays;

            Program.CreateProvider().UpdateFeed( MyFeed );
            
            MyWindow.Close();
        }
        public void CancelChanges()
        {
            MyWindow.Close();
        }
    }
}
