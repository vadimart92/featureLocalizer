using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
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
using Ookii.Dialogs.Wpf;

namespace TranslateApp.ViewModels {
	public sealed class MainWindowViewModel : ViewModelBase {
		readonly BraceFoldingStrategy _foldingStrategy = new BraceFoldingStrategy();
		readonly TranslationProvider _translationProvider = new TranslationProvider();
		private string _currentFile;

		private readonly FastObservableCollection<LineTranslationVariant> _currenTranslationVariants =
			new FastObservableCollection<LineTranslationVariant>();

		private LineTranslationConfig _currentConfig;


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

		private void SourceTextEditorCaret_PositionChanged(object sender, EventArgs e) {
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
			set
			{
				var newVal = value < TranslationConfigs.Count ? value : 0;
				if (TranslationConfigs.Any())
					CurrentConfig = TranslationConfigs[newVal];
				SetValue(CurrentLineIndexProperty, newVal);	
			}
		}

		/// <summary>
		/// CurrentLineIndex property data.
		/// </summary>
		public static readonly PropertyData CurrentLineIndexProperty = RegisterProperty("CurrentLineIndex", typeof (int));

		#endregion

		#region Translation logic

		private void UpdateTranslationConfigs() {
			var config = _translationProvider.GeTranslationConfigs(_currentFile);
			TranslationConfigs.AddItems(config);
		}

		private void OnCurrentLineChanged() {
			CurrentStepInfo = String.Format("Line: {0}, Exp: {1}", CurrentConfig.LineNumber, CurrentConfig.Regex);
			SourceTextEditor.TextArea.Caret.Location = new TextLocation(CurrentConfig.LineNumber, 5);
			SourceTextEditor.TextArea.Caret.BringCaretToView();
		}

		#endregion

		#region CurrentStepInfo property

		/// <summary>
		/// Gets or sets the CurrentStepInfo value.
		/// </summary>
		public string CurrentStepInfo
		{
			get { return GetValue<string>(CurrentStepInfoProperty); }
			set { SetValue(CurrentStepInfoProperty, value); }
		}

		/// <summary>
		/// CurrentStepInfo property data.
		/// </summary>
		public static readonly PropertyData CurrentStepInfoProperty = RegisterProperty("CurrentStepInfo", typeof (string));

		#endregion

		#region PanelLoading property

		/// <summary>
		/// Gets or sets the PanelLoading value.
		/// </summary>
		public bool PanelLoading
		{
			get { return GetValue<bool>(PanelLoadingProperty); }
			set { SetValue(PanelLoadingProperty, value); }
		}

		/// <summary>
		/// PanelLoading property data.
		/// </summary>
		public static readonly PropertyData PanelLoadingProperty = RegisterProperty("PanelLoading", typeof (bool));

		#endregion

		#region PanelMainMessage property

		/// <summary>
		/// Gets or sets the PanelMainMessage value.
		/// </summary>
		public string PanelMainMessage
		{
			get { return GetValue<string>(PanelMainMessageProperty); }
			set { SetValue(PanelMainMessageProperty, value); }
		}

		/// <summary>
		/// PanelMainMessage property data.
		/// </summary>
		public static readonly PropertyData PanelMainMessageProperty = RegisterProperty("PanelMainMessage", typeof (string));

		#endregion

		#region PanelSubMessage property

		/// <summary>
		/// Gets or sets the PanelSubMessage value.
		/// </summary>
		public string PanelSubMessage
		{
			get { return GetValue<string>(PanelSubMessageProperty); }
			set { SetValue(PanelSubMessageProperty, value); }
		}

		/// <summary>
		/// PanelSubMessage property data.
		/// </summary>
		public static readonly PropertyData PanelSubMessageProperty = RegisterProperty("PanelSubMessage", typeof (string));

		#endregion

		#region PanelClose command

		private Command _panelCloseCommandCommand;

		/// <summary>
		/// Gets the PanelClose command.
		/// </summary>
		public Command PanelCloseCommand
		{
			get { return _panelCloseCommandCommand ?? (_panelCloseCommandCommand = new Command(PanelClose)); }
		}

		/// <summary>
		/// Method to invoke when the PanelClose command is executed.
		/// </summary>
		private void PanelClose() {
			PanelLoading = false;
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
				_currentFile = fd.FileName;
				FirstFileText.Text = str;
				SecondFileText.Text = str;
				UpdateFoldingStrategy(SourceTextEditor, _foldingStrategy);
				UpdateFoldingStrategy(TranslationTextEditor, _foldingStrategy);
				UpdateTranslationConfigs();
			}
			
		}

		#endregion

		#region Learn command

		private Command _learnCommand;

		/// <summary>
		/// Gets the Learn command.
		/// </summary>
		public Command LearnCommand
		{
			get { return _learnCommand ?? (_learnCommand = new Command(Learn)); }
		}

		/// <summary>
		/// Method to invoke when the Learn command is executed.
		/// </summary>
		private void Learn() {
			var dialog = new VistaFolderBrowserDialog();
			if (dialog.ShowDialog() ?? false) {
				PanelLoading = true;
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
