using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace TranslateApp.Common {
	public class InvertBooleanToVisibilityConverter : IValueConverter {

		private readonly BooleanToVisibilityConverter _booleanToVisibilityConverter = new BooleanToVisibilityConverter();
		#region Члены IValueConverter

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			var val = (Visibility)_booleanToVisibilityConverter.Convert(value, targetType, parameter, culture);
			return val == Visibility.Visible ? Visibility.Hidden : Visibility.Visible;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			var val = _booleanToVisibilityConverter.Convert(value, targetType, parameter, culture);
			try {
				return !((bool)val);
			} catch (Exception) {
				return val;
			}
		}

		#endregion
	}
}
