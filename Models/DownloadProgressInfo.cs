using System;
using ReactiveUI;

namespace Podlonia.Models
{
    public class DownloadProgressInfo: ReactiveObject
    {
        long EnclosureIdField;
        public long EnclosureId
        { 
            get => EnclosureIdField; 
            set => this.RaiseAndSetIfChanged( ref EnclosureIdField, value );
        }

        long TotalBytesField;
        public long TotalBytes
        { 
            get => TotalBytesField; 
            set => this.RaiseAndSetIfChanged( ref TotalBytesField, value );
        }

        long TransferedBytesField;
        public long TransferedBytes
        { 
            get => TransferedBytesField; 
            set => this.RaiseAndSetIfChanged( ref TransferedBytesField, value );
        }

        float ProgressPercentField;
        public float ProgressPercent
        { 
            get => ProgressPercentField; 
            set => this.RaiseAndSetIfChanged( ref ProgressPercentField, value );
        }

        bool FinishedField;
        public bool Finished
        { 
            get => FinishedField; 
            set => this.RaiseAndSetIfChanged( ref FinishedField, value );
        }

        bool ErrorField;
        public bool Error
        { 
            get => ErrorField; 
            set => this.RaiseAndSetIfChanged( ref ErrorField, value );
        }

        Uri UrlField;
        public Uri Url
        { 
            get => UrlField; 
            set => this.RaiseAndSetIfChanged( ref UrlField, value );
        }

        string FileNameField;
        public string FileName
        { 
            get => FileNameField; 
            set => this.RaiseAndSetIfChanged( ref FileNameField, value );
        }
    }
}
