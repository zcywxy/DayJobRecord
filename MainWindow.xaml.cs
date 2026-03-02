using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using DayJobRecord.ViewModels;

namespace DayJobRecord
{
    public class BoolToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool b)
            {
                return b ? "是" : "否";
            }
            return "否";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is string s)
            {
                return s == "是";
            }
            return false;
        }
    }

    public class StringToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is string colorString && !string.IsNullOrEmpty(colorString))
            {
                try
                {
                    var color = (Color)ColorConverter.ConvertFromString(colorString);
                    return new SolidColorBrush(color);
                }
                catch
                {
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E3F2FD"));
                }
            }
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E3F2FD"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }
    }
}
