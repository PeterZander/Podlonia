using System;
using System.IO;
using ReactiveUI;

namespace Podlonia.Models
{
    public class DownloadItem: ReactiveObject
    {
        public long Id { get; set; }
        string UrlField;
        public string Url
        {
            get => UrlField;
            set => this.RaiseAndSetIfChanged( ref UrlField, value );
        }
        string RelFileNameField;
        public string RelFileName
        {
            get => RelFileNameField;
            set => this.RaiseAndSetIfChanged( ref RelFileNameField, value );
        }
        string ContentTypeField;
        public string ContentType
        {
            get => ContentTypeField;
            set => this.RaiseAndSetIfChanged( ref ContentTypeField, value );
        }
        long LengthField;
        public long Length
        {
            get => LengthField;
            set => this.RaiseAndSetIfChanged( ref LengthField, value );
        }
        DateTime PubDateField;
        public DateTime PubDate
        {
            get => PubDateField;
            set => this.RaiseAndSetIfChanged( ref PubDateField, value );
        }
        long DownloadCountField;
        public long DownloadCount
        {
            get => DownloadCountField;
            set => this.RaiseAndSetIfChanged( ref DownloadCountField, value );
        }
        long DownloadErrorCountField;
        public long DownloadErrorCount
        {
            get => DownloadErrorCountField;
            set => this.RaiseAndSetIfChanged( ref DownloadErrorCountField, value );
        }
        bool ForceDownloadField;
        public bool ForceDownload
        {
            get => ForceDownloadField;
            set => this.RaiseAndSetIfChanged( ref ForceDownloadField, value );
        }
        bool ForceSyncField;
        public bool ForceSync
        {
            get => ForceSyncField;
            set => this.RaiseAndSetIfChanged( ref ForceSyncField, value );
        }

        public string FullPathAndFileName
        {
            get
            {
                var filename = Path.GetFullPath( RelFileName, Program.Configuration.DownloadDirectory );
                return filename;
            }
        }
        long DownloadEnclosureMaxAgeDaysField;
        public long DownloadEnclosureMaxAgeDays
        {
            get => DownloadEnclosureMaxAgeDaysField;
            set => this.RaiseAndSetIfChanged( ref DownloadEnclosureMaxAgeDaysField, value );
        }

        public DownloadItem() {}
        public DownloadItem( RSSEnclosure e, RSSFeed feed ) 
        {
            Id = e.Id;
            Url = e.Url;
            RelFileName = Path.Combine( feed.LocalPath, e.FileName );
            ContentType = e.ContentType;
            Length = e.Length;
            PubDate = e.PubDate;
            DownloadCount = e.DownloadsComplete;
            DownloadErrorCount = e.DownloadErrors;
            ForceDownload = e.ForceDownload;
            ForceSync = e.ForceSync;
            DownloadEnclosureMaxAgeDays = feed.DownloadEnclosureMaxAgeDays;
        }
    }
}
