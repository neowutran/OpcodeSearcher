using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace DamageMeter.UI.Windows {
    public class OpcodeFoundToColor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var opc = (uint)value;
            if (opc == 0) return Brushes.Crimson;
            else return Brushes.MediumSeaGreen;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}