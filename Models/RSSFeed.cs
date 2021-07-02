using System.Linq;
using System.IO;
using System.ComponentModel;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Podlonia.Models
{
    public class RSSError
    {
        public DateTime Time { get; set; }
        public string Message { get; set; }
    }
    public class RSSFeed: ReactiveObject
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }

        public string LocalPath { get; set; }
        public string ImageSource { get; set; }
        public long DownloadEnclosureMaxAgeDays { get; set; }
        public long StoredEnclosureMaxAgeDays { get; set; }

        [NotMapped]
        public ObservableCollection<RSSError> AllErrors = new ObservableCollection<RSSError>();

        string LatestErrorField;
        [NotMapped]
        public string LatestError
        { 
            get => LatestErrorField; 
            set 
            {
                this.RaiseAndSetIfChanged( ref LatestErrorField, value );
                ShowError = !string.IsNullOrWhiteSpace( LatestErrorField );
                if ( ShowError ) AllErrors.Add( new RSSError { Time = DateTime.Now, Message = value } );
            }
        }
        bool ShowErrorField;

        [NotMapped]
        public bool ShowError
        { 
            get => ShowErrorField; 
            set => this.RaiseAndSetIfChanged( ref ShowErrorField, value );
        }

        public string RelPath( RSSEnclosure enc )
        {
            var ep = LocalPath;
            if ( Path.IsPathRooted( ep ) )
            {
                ep = ep.Split( Path.DirectorySeparatorChar ).Last( s => s.Length > 0 );
            }

            var relpath = Path.Combine( LocalPath, enc.FileName );
            return relpath;
        }
    }
}
