using System;
using System.Globalization;
using System.Windows.Data;

namespace HlsDumpLib.GuiTestWPF
{
	public class IntStringConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value is int ? (value as int?).ToString() : null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!int.TryParse(value as string, out int n)) { n = 0; }
			return n;
		}
	}
}
