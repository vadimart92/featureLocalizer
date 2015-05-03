using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using Catel.MVVM;

namespace TranslateApp.Common {
	public class LineTranslationConfig {
		public LineTranslationConfig() {
			Variants = new ObservableCollection<LineTranslationVariant>();
		}
		public int LineNumber { get; set; }
		public Regex Regex { get; set; }
		public ObservableCollection<LineTranslationVariant> Variants { get; set; }
	}

	public class LineTranslationVariant : ViewModelBase {
		
		public string NameRu { get; set; }
		public string NameEng { get; set; }

		public string MacrosValue{
			get { return string.Format("##RU={0};EN={1}##", NameRu, NameEng); }
		}

		public LineTranslationConfig Config { get; set; }

		#region ReplaceText command

		private Command<object> _replaceTextCommand;

		/// <summary>
		/// Gets the ReplaceText command.
		/// </summary>
		public Command<object> ReplaceTextCommand
		{
			get {
				return _replaceTextCommand ?? (_replaceTextCommand = new Command<object>(ReplaceText));
			}
		}

		/// <summary>
		/// Method to invoke when the ReplaceText command is executed.
		/// </summary>
		private void ReplaceText(object value) {
			var textEditorData = TextEditorsParameterConverter.GetEditorsFromParameter(value);
			var doc = textEditorData.SourceTextEditor.Document;
			var line = doc.Lines[Config.LineNumber-1];
			var lineText = doc.GetText(line.Offset, line.Length);
			var match = Config.Regex.Match(lineText);
			if (match.Success) {
				var currentValue = match.Groups[1].Value;
				var str = lineText.Replace(currentValue, MacrosValue);
				var destLine = textEditorData.DestinationTextEditor.Document.GetLineByNumber(Config.LineNumber);
				textEditorData.DestinationTextEditor.Document.Replace(destLine.Offset, destLine.Length, str);
			}
			
		}

		#endregion
	}
}
