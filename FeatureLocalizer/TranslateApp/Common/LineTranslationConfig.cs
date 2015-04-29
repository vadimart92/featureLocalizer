using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Catel.MVVM;
using ICSharpCode.AvalonEdit;

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

		private Command<TextEditor> _replaceTextCommand;

		/// <summary>
		/// Gets the ReplaceText command.
		/// </summary>
		public Command<TextEditor> ReplaceTextCommand
		{
			get {
				return _replaceTextCommand ?? (_replaceTextCommand = new Command<TextEditor>(ReplaceText));
			}
		}

		/// <summary>
		/// Method to invoke when the ReplaceText command is executed.
		/// </summary>
		private void ReplaceText(TextEditor textEditor) {
			var doc = textEditor.Document;
			var line = doc.Lines[Config.LineNumber - 1];
			var lineText = doc.GetText(line.Offset, line.Length);
			var match = Config.Regex.Match(lineText);
			if (match.Success) {
				var currentValue = match.Groups[1].Value;
				var lineStringOffset = lineText.IndexOf(currentValue, StringComparison.OrdinalIgnoreCase);
				textEditor.TextArea.Document.Replace(line.Offset + lineStringOffset, currentValue.Length , MacrosValue);
			}
			
		}

		#endregion
	}
}
