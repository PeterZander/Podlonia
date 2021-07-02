using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Podlonia.Views;

namespace Podlonia.Converters
{
    public class ButtonMaskConverter: IValueConverter
    {
        public object Convert( object value, Type targettype, object parameter,
            System.Globalization.CultureInfo culture )
        {
            var v = (MessageWindow.Buttons)value;
            string para = parameter as string;

            switch( para.ToLower() )
            {
                case "ok":
                    return v.HasFlag( MessageWindow.Buttons.Ok );

                case "cancel":
                    return v.HasFlag( MessageWindow.Buttons.Cancel );

                case "abort":
                    return v.HasFlag( MessageWindow.Buttons.Abort );

                case "ignore":
                    return v.HasFlag( MessageWindow.Buttons.Ignore );

                case "retry":
                    return v.HasFlag( MessageWindow.Buttons.Retry );

                default:
                    throw new ArgumentException( $"Button type '{para}' is not known." );
            }
        }

        public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
        {
            throw new NotImplementedException();
        }
    }
}