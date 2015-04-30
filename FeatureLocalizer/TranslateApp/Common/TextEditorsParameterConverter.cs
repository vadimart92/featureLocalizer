using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using ICSharpCode.AvalonEdit;

namespace TranslateApp.Common {
	class TextEditorsParameterConverter: IMultiValueConverter {
 
		public class TextEditorsData {
			public TextEditor SourceTextEditor { get; set; }
			public TextEditor DestinationTextEditor { get; set; }
		}

		#region Члены IMultiValueConverter

		public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			return new TextEditorsData {
				SourceTextEditor = (TextEditor)values[0],
				DestinationTextEditor = (TextEditor)values[1]
			};
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture) {
			var p = (TextEditorsData) value;
			return new object[] {p.SourceTextEditor, p.DestinationTextEditor};
		}

		public static TextEditorsData GetEditorsFromParameter(object value) {
			return value as TextEditorsData;
		}

		#endregion
	}
}
