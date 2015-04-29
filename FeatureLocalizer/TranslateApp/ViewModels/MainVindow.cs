using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
using Catel.Collections;
using Catel.Data;
using Catel.IoC;
using Catel.MVVM;
using ICSharpCode.AvalonEdit.Document;
using Microsoft.Win32;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Highlighting;
using TranslateApp.Common;
using ICSharpCode.AvalonEdit.Folding;

namespace TranslateApp.ViewModels {
	public sealed class MainWindowViewModel : ViewModelBase {

		BraceFoldingStrategy _foldingStrategy = new BraceFoldingStrategy();
		private CompletionWindow completionWindow;

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

		public FastObservableCollection<LineTranslationVariant> CurrenTranslationVariants
		{
			get { return _currenTranslationVariants; }
		}

		public LineTranslationConfig CurrentConfig
		{
			get { return _currentConfig; }
			set
			{
				_currentConfig = value;
				_currenTranslationVariants.Clear();
				_currenTranslationVariants.AddItems(value.Variants);
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

		public FastObservableCollection<LineTranslationConfig> TranslationConfigs
		{
			get { return _translationConfigs; }
			set { _translationConfigs = value; }
		}

		#region CurrentLineIndex property

		/// <summary>
		/// Gets or sets the CurrentLineIndex value.
		/// </summary>
		public int CurrentLineIndex
		{
			get { return GetValue<int>(CurrentLineIndexProperty); }
			set { SetValue(CurrentLineIndexProperty, value); }
		}

		/// <summary>
		/// CurrentLineIndex property data.
		/// </summary>
		public static readonly PropertyData CurrentLineIndexProperty = RegisterProperty("CurrentLineIndex", typeof (int));

		#endregion

		#region Translation logic

		private void UpdateTranslationConfigs() {
			var config = new LineTranslationConfig {
				LineNumber = 17,
				Regex = new Regex("Открыт раздел \"(.*)\"")
					
			};
			config.Variants = new ObservableCollection<LineTranslationVariant> {
				new LineTranslationVariant {NameRu = "Продажи", NameEng = "Opportunities", Config = config},
				new LineTranslationVariant {NameRu = "Заказы", NameEng = "Orders", Config = config}
			};
			TranslationConfigs.Add(config);
		}

		private void OnCurrentLineChanged() {
			if (CurrentLineIndex >= TranslationConfigs.Count) {
				CurrentLineIndex = 0;
				return;
			}
			if (CurrentLineIndex < 0) {
				CurrentLineIndex = TranslationConfigs.Count - 1;
				return;
			}
			CurrentConfig = TranslationConfigs[CurrentLineIndex];
			SourceTextEditor.TextArea.Caret.Line = CurrentConfig.LineNumber;
			SourceTextEditor.TextArea.Caret.Column = 0;
			SourceTextEditor.TextArea.Caret.BringCaretToView();
			
		}

		private void ShowCompletionWindow(LineTranslationConfig currentConfig, int startOffset, int length) {
			completionWindow = new CompletionWindow(TranslationTextEditor.TextArea);
			IList<ICompletionData> data = completionWindow.CompletionList.CompletionData;
			data.Add(new NewTranslateComletionData());
			foreach (var variant in currentConfig.Variants) {
				data.Add(new TranslateComletionData(variant, startOffset, length));
			}
			//completionWindow.CloseAutomatically = false;
			completionWindow.CloseWhenCaretAtBeginning = false;
			completionWindow.StartOffset = startOffset;
			completionWindow.EndOffset = startOffset + length;
			completionWindow.Show();
			completionWindow.Closed += delegate {
				completionWindow = null;
			};
		}

		#endregion

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
		private FastObservableCollection<LineTranslationConfig> _translationConfigs = new FastObservableCollection<LineTranslationConfig>();


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
				UpdateTranslationConfigs();
			}
			
		}

		#endregion

		protected override void OnPropertyChanged(AdvancedPropertyChangedEventArgs e) {
			if (e.PropertyName == "CurrentLineIndex") {
				OnCurrentLineChanged();
			}
			base.OnPropertyChanged(e);
		}

		#region LineUp command

		private Command _lineUpCommand;

		/// <summary>
		/// Gets the LineUp command.
		/// </summary>
		public Command LineUpCommand
		{
			get { return _lineUpCommand ?? (_lineUpCommand = new Command(LineUp)); }
		}

		/// <summary>
		/// Method to invoke when the LineUp command is executed.
		/// </summary>
		private void LineUp() {
			CurrentLineIndex--;
		}

		#endregion

		#region LineDown command

		private Command _lineDownCommand;
		private FastObservableCollection<LineTranslationVariant> _currenTranslationVariants = new FastObservableCollection<LineTranslationVariant> {
			new LineTranslationVariant{NameEng = "eng", NameRu = "ru"}
		};
		private LineTranslationConfig _currentConfig;

		/// <summary>
		/// Gets the LineDown command.
		/// </summary>
		public Command LineDownCommand
		{
			get { return _lineDownCommand ?? (_lineDownCommand = new Command(LineDown)); }
		}

		/// <summary>
		/// Method to invoke when the LineDown command is executed.
		/// </summary>
		private void LineDown() {
			CurrentLineIndex++;
		}

		#endregion
	}
}
