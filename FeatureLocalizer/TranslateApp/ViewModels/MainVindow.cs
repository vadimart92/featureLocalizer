using System.IO;
using Catel.Data;
using Catel.IoC;
using Catel.MVVM;
using ICSharpCode.AvalonEdit.Document;
using Microsoft.Win32;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using TranslateApp.Common;
using ICSharpCode.AvalonEdit.Folding;

namespace TranslateApp.ViewModels {
	public sealed class MainWindowViewModel : ViewModelBase {

		BraceFoldingStrategy _foldingStrategy = new BraceFoldingStrategy();

		ColorizeAvalonEdit _ce = new ColorizeAvalonEdit();

		public MainWindowViewModel() {
			Title = "Hello!";
			
		}

		public TextEditor TranslationTextEditor {
			get { return _translationTextEditor; }
			set {
				UpdateFoldingStrategy(value, _foldingStrategy);
				value.TextArea.TextView.LineTransformers.Add(_ce);
				_translationTextEditor = value;
			}
		}

		private TextEditor _sourceTextEditor;
		public TextEditor SourceTextEditor {
			get { return _sourceTextEditor; }
			set {
				value.TextArea.Caret.PositionChanged+=SourceTextEditorCaret_PositionChanged;
				UpdateFoldingStrategy(value, _foldingStrategy);
				value.TextArea.TextView.LineTransformers.Add(_ce);
				_sourceTextEditor = value;
			}
		}

		private void SourceTextEditorCaret_PositionChanged(object sender, System.EventArgs e) {
			if (TranslationTextEditor.Text.Length > SourceTextEditor.CaretOffset) {
				TranslationTextEditor.CaretOffset = SourceTextEditor.CaretOffset;
				TranslationTextEditor.ScrollToVerticalOffset(SourceTextEditor.VerticalOffset);
				_ce.LineNum = TranslationTextEditor.TextArea.Caret.Line;
				TranslationTextEditor.TextArea.TextView.Redraw();
				SourceTextEditor.TextArea.TextView.Redraw();
			}
		}

		private void UpdateFoldingStrategy(TextEditor editor, BraceFoldingStrategy foldingStrategy) {
			var foldingManager = (editor.TextArea.GetService(typeof(FoldingManager)) as FoldingManager) ?? FoldingManager.Install(editor.TextArea);
			foldingStrategy.UpdateFoldings(foldingManager, editor.Document);
		}

		#region FirstFileText property

		/// <summary>
		/// Gets or sets the FirstFileText value.
		/// </summary>
		public TextDocument FirstFileText {
			get { return GetValue<TextDocument>(StringProperty); }
			set { SetValue(StringProperty, value); }
		}

		/// <summary>
		/// FirstFileText property data.
		/// </summary>
		public static readonly PropertyData StringProperty = RegisterProperty("FirstFileText", typeof(TextDocument), new TextDocument());

		#endregion

		#region SecondFileText property

		/// <summary>
		/// Gets or sets the SecondFileText value.
		/// </summary>
		public TextDocument SecondFileText {
			get { return GetValue<TextDocument>(SecondFileTextProperty); }
			set { SetValue(SecondFileTextProperty, value); }
		}

		/// <summary>
		/// SecondFileText property data.
		/// </summary>
		public static readonly PropertyData SecondFileTextProperty = RegisterProperty("SecondFileText", typeof (TextDocument), new TextDocument());

		#endregion

		#region FirstFileSelectedText property

		/// <summary>
		/// Gets or sets the FirstFileSelectedText value.
		/// </summary>
		public string FirstFileSelectedText {
			get { return GetValue<string>(FirstFileSelectedTextProperty); }
			set { SetValue(FirstFileSelectedTextProperty, value); }
		}

		/// <summary>
		/// FirstFileSelectedText property data.
		/// </summary>
		public static readonly PropertyData FirstFileSelectedTextProperty = RegisterProperty("FirstFileSelectedText", typeof (string));

		#endregion

		#region OpenFile command

		private Command _openFileCommand;
		private TextEditor _translationTextEditor;


		/// <summary>
		/// Gets the OpenFile command.
		/// </summary>
		public Command OpenFileCommand {
			get { return _openFileCommand ?? (_openFileCommand = new Command(OpenFile)); }
		}

		/// <summary>
		/// Method to invoke when the OpenFile command is executed.
		/// </summary>
		private void OpenFile() {
			var fd = new OpenFileDialog();
			if (fd.ShowDialog() ?? false) {
				var str = File.ReadAllText(fd.FileName);
				FirstFileText.Text = str;
				SecondFileText.Text = str;
				UpdateFoldingStrategy(SourceTextEditor, _foldingStrategy);
				UpdateFoldingStrategy(TranslationTextEditor, _foldingStrategy);
			}
			
		}

		#endregion

	}
}
