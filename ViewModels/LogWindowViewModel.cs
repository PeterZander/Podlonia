using System;
using System.Reactive;
using Avalonia.Controls;
using Avalonia.Collections;
using ReactiveUI;
using System.Linq;
using System.Collections.ObjectModel;
using System.Text;

namespace Podlonia.ViewModels
{
    public class LogWindowViewModel : ViewModelBase
    {
        public class LogEventLine
        {
            public DateTime EventTime { get; set; }
            public string Message { get; set; }
        }

        Window MyWindow;

        public LogWindowViewModel( Window w )
        {
            MyWindow = w;
            CopyEventItemCommand = ReactiveCommand.Create( CopyEvent );
            RemoveEventItemCommand = ReactiveCommand.Create( RemoveEvent );

            Program.NewLogLine += NewLogEvent;
        }

        const int MaxEventInListCount = 250;
        public ObservableCollection<LogEventLine> Events { get; set; } = new ObservableCollection<LogEventLine>();
        public ObservableCollection<LogEventLine> SelectedEvents { get; set; } = new ObservableCollection<LogEventLine>();
        public void NewLogEvent( string msg )
        {
            Events.Insert( 0, new LogEventLine { EventTime = DateTime.Now, Message = msg } );

            while ( Events.Count > MaxEventInListCount )
            {
                Events.RemoveAt( Events.Count - 1 );
            }
        }
        public ReactiveCommand<Unit, Unit> CopyEventItemCommand { get; }
        public void CopyEvent()
        {
            if ( SelectedEvents.Count <= 0 ) return;

            var text = new StringBuilder();
            foreach( var line in SelectedEvents )
            {
                text.AppendLine( $"{line.EventTime}: {line.Message}" );
            }

            _ = Avalonia.Application.Current.Clipboard.SetTextAsync( text.ToString() );
        }
        public ReactiveCommand<Unit, Unit> RemoveEventItemCommand { get; }
        public void RemoveEvent()
        {
            foreach ( var oneevent in SelectedEvents.ToArray() ) Events.Remove( oneevent );
        }
    }
}
