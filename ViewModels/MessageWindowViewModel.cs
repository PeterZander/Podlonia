using System;
using System.Collections.Generic;
using System.Text;
using System.Reactive;
using System.Configuration;
using Podlonia.Provider;
using Podlonia.Models;
using Avalonia.Controls;
using Avalonia.Collections;
using ReactiveUI;
using Podlonia.Views;
using System.Reflection;

namespace Podlonia.ViewModels
{
    public class MessageWindowViewModel : ViewModelBase
    {
        Window MyWindow;

        public MessageWindowViewModel( Window w )
        {
            MyWindow = w;

            OkCommand = ReactiveCommand.Create( Ok );
            CancelCommand = ReactiveCommand.Create( Cancel );
            IgnoreCommand = ReactiveCommand.Create( Ignore );
            RetryCommand = ReactiveCommand.Create( Retry );
            AbortCommand = ReactiveCommand.Create( Abort );
        }

        double WindowWidthField = 350;
        public double WindowWidth
        { 
            get => WindowWidthField; 
            set => this.RaiseAndSetIfChanged( ref WindowWidthField, value );
        }

        double WindowHeightField = 180;
        public double WindowHeight
        { 
            get => WindowHeightField; 
            set => this.RaiseAndSetIfChanged( ref WindowHeightField, value );
        }

        MessageWindow.Buttons ShowButtonsField;
        public MessageWindow.Buttons ShowButtons
        { 
            get => ShowButtonsField;
            set => this.RaiseAndSetIfChanged( ref ShowButtonsField, value );
        }

        MessageWindow.Buttons ClickedButtonField;
        public MessageWindow.Buttons ClickedButton
        { 
            get => ClickedButtonField;
            set => this.RaiseAndSetIfChanged( ref ClickedButtonField, value );
        }

        public string Caption { get; set; }
        public string Message { get; set; }

        public ReactiveCommand<Unit, Unit> OkCommand { get; }
        public ReactiveCommand<Unit, Unit> CancelCommand { get; }
        public ReactiveCommand<Unit, Unit> IgnoreCommand { get; }
        public ReactiveCommand<Unit, Unit> RetryCommand { get; }
        public ReactiveCommand<Unit, Unit> AbortCommand { get; }
        public void Ok()
        {
            ClickedButton = MessageWindow.Buttons.Ok;
            MyWindow.Close();
        }
        public void Cancel()
        {
            ClickedButton = MessageWindow.Buttons.Cancel;
            MyWindow.Close();
        }
        public void Ignore()
        {
            ClickedButton = MessageWindow.Buttons.Ignore;
            MyWindow.Close();
        }
        public void Retry()
        {
            ClickedButton = MessageWindow.Buttons.Retry;
            MyWindow.Close();
        }
        public void Abort()
        {
            ClickedButton = MessageWindow.Buttons.Abort;
            MyWindow.Close();
        }
    }
}
