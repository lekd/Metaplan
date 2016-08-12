using System;
using System.Globalization;
using System.Windows.Data;

namespace WhiteboardApp.TimelineControllers
{
    public class PercentageConverter : IValueConverter
    {
        public object Convert(object value,
        Type targetType,
        object parameter,
        System.Globalization.CultureInfo culture)
        {
            return System.Convert.ToDouble(value,CultureInfo.InvariantCulture) *
                   System.Convert.ToDouble(parameter, CultureInfo.InvariantCulture);
        }

        public object ConvertBack(object value,
            Type targetType,
            object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
