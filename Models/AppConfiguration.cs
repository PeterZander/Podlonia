using System;
using Newtonsoft.Json;
using ReactiveUI;

namespace Podlonia.Models
{
    public class AppConfiguration: ReactiveObject
    {
        bool NoAutomaticSerializationField = false;
        [JsonIgnore] public bool NoAutomaticSerialization
        {
            get => NoAutomaticSerializationField;
            set
            {
                NoAutomaticSerializationField = value;

                if ( !NoAutomaticSerializationField )
                {
                    Program.SaveConfiguration();
                }
            }
        }
        string DatabaseFileField;
        [JsonProperty]
        public string DatabaseFile
        { 
            get => DatabaseFileField; 
            set => this.RaiseAndSetIfChanged( ref DatabaseFileField, value );
        }

        string DownloadDirectoryField;
        [JsonProperty]
        public string DownloadDirectory
        { 
            get => DownloadDirectoryField; 
            set => this.RaiseAndSetIfChanged( ref DownloadDirectoryField, value );
        }

        bool EnableScanField = true;
        [JsonProperty]
        public bool EnableScan
        { 
            get => EnableScanField; 
            set => this.RaiseAndSetIfChanged( ref EnableScanField, value );
        }

        bool EnableSyncField = true;
        [JsonProperty]
        public bool EnableSync
        { 
            get => EnableSyncField; 
            set => this.RaiseAndSetIfChanged( ref EnableSyncField, value );
        }

        bool EnableDownloadField = true;
        [JsonProperty]
        public bool EnableDownload
        { 
            get => EnableDownloadField; 
            set => this.RaiseAndSetIfChanged( ref EnableDownloadField, value );
        }

        int MaxAgeForDownloadDaysField = 300;
        [JsonProperty]
        public int MaxAgeForDownloadDays
        { 
            get => MaxAgeForDownloadDaysField;
            set => this.RaiseAndSetIfChanged( ref MaxAgeForDownloadDaysField, value );
        }
        int MaxAgeForStorageDaysField = 0;
        [JsonProperty]
        public int MaxAgeForStorageDays
        { 
            get => MaxAgeForStorageDaysField;
            set => this.RaiseAndSetIfChanged( ref MaxAgeForStorageDaysField, value );
        }
        int MaxParalellDownloadsField = 3;
        [JsonProperty]
        public int MaxParalellDownloads
        { 
            get => MaxParalellDownloadsField;
            set => this.RaiseAndSetIfChanged( ref MaxParalellDownloadsField, value );
        }
        int MinutesBetweenBackgroundScansField = 90;
        [JsonProperty]
        public int MinutesBetweenBackgroundScans
        { 
            get => MinutesBetweenBackgroundScansField;
            set => this.RaiseAndSetIfChanged( ref MinutesBetweenBackgroundScansField, value );
        }

        string MediaPlayerField;
        [JsonProperty]
        public string MediaPlayer
        { 
            get => MediaPlayerField; 
            set => this.RaiseAndSetIfChanged( ref MediaPlayerField, value );
        }
    }
}
