using System.IO;
using ReactiveUI;

namespace Podlonia.Models
{
    public class SelectedFeedItem: ReactiveObject
    {
        public long Id;
        DownloadItem InfoField;
        public DownloadItem Info
        { 
            get => InfoField; 
            set => this.RaiseAndSetIfChanged( ref InfoField, value );
        }
        DownloadProgressInfo DownloadProgressInfoField;
        public DownloadProgressInfo DownloadProgressInfo
        { 
            get => DownloadProgressInfoField; 
            set => this.RaiseAndSetIfChanged( ref DownloadProgressInfoField, value );
        }
    }
}