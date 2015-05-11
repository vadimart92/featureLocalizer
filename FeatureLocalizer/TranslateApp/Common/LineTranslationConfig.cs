using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Text.RegularExpressions;
using System.Windows.Media;
using Catel.Collections;
using Catel.Data;
using Catel.MVVM;
using FeatureLocalizer;
using ICSharpCode.AvalonEdit;

namespace TranslateApp.Common {
	public class LineTranslationConfig {
		public LineTranslationConfig() {
			Variants = new ObservableCollection<LineTranslationVariant>();
			WordIndex = 0;
		}

		public int WordIndex { get; set; }
		public int LineNumber { get; set; }
		public Regex Regex { get; set; }
		public ObservableCollection<LineTranslationVariant> Variants { get; set; }

		public FastObservableCollection<LineTranslationVariant> CurrentWindowVariants {
			get;
			set;
		}

		public void TryTranslate(TextEditor sourceTextEditor, TextEditor destTextEditor) {
			Contract.Requires(sourceTextEditor!=null, "sourceTextEditor!=null");
			Contract.Requires(destTextEditor != null, "destTextEditor!=null");
			var word = GetLineWord(sourceTextEditor);
			if (word == null) {
				return;
			}
			foreach (var variant in Variants) {
				if (word.Equals(variant.NameRu, StringComparison.OrdinalIgnoreCase)) {
					variant.Replace(sourceTextEditor,destTextEditor);
					SetSelectedItem(variant);
					break;
				}
			}
		}

		public string GetLineWord(TextEditor sourceTextEditor) {
			var doc = sourceTextEditor.Document;
			var line = doc.GetLineByNumber(LineNumber);
			var lineString = doc.GetText(line.Offset, line.Length);
			var lineWords = GetLineWords(lineString);
			if (lineWords.Count <= WordIndex) {
				return null;
			}
			var word = lineWords[WordIndex];
			return word;
		}

		public List<string> GetLineWords(string line) {
			Contract.Requires(line!=null, "line!=null");
			var res = new List<string>();
			var match = Regex.Match(line);
			if (match.Success) {
				for (int i = 1; i < match.Groups.Count; i++) {
					var value = match.Groups[i].Value;
					res.Add(value);
				}
			}
			return res;
		}

		public void SetSelectedItem(LineTranslationVariant item) {
			foreach (var variant in Variants) {
				variant.IsSelected = variant == item;
			}
		}

		public void SortVariants() {
			Variants.Sort((variant, translationVariant) => String.Compare(variant.NameRu, translationVariant.NameRu, StringComparison.OrdinalIgnoreCase));
		}

		public void RefreshVariants() {
			CurrentWindowVariants.Clear();
			CurrentWindowVariants.AddRange(Variants);
		}
	}

	public class LineMatchConfig {
		public bool Success { get { return Match != null && Match.Success; } }
		public Match Match { get; set; }
		public string ResultLine { get; set; }
	}

	public class LineTranslationVariant : ViewModelBase {

		public LineTranslationVariant() {
			Background = new SolidColorBrush(Colors.Black);
		}
		
		public string NameRu { get; set; }
		public string NameEng { get; set; }

		public string MacrosValue{
			get { return string.Format("##RU={0};EN={1}##", NameRu, NameEng); }
		}

		public LineTranslationConfig Config { get; set; }

		public bool IsMatch(TextEditor sourceTextEditor) {
			Contract.Requires(sourceTextEditor != null, "sourceTextEditor!=null");
			var match = GetMatchConfig(sourceTextEditor);
			return match.Success;
		}

		public LineMatchConfig GetMatchConfig(TextEditor sourceTextEditor) {
			Contract.Requires(sourceTextEditor != null, "sourceTextEditor!=null");
			var res = new LineMatchConfig();
			var doc = sourceTextEditor.Document;
			var line = doc.Lines[Config.LineNumber - 1];
			var lineText = doc.GetText(line.Offset, line.Length);
			var match = Config.Regex.Match(lineText);
			res.Match = match;
			if (match.Success) {
				var currentValue = match.Groups[1].Value;
				var str = lineText.Replace(currentValue, MacrosValue);
				res.ResultLine = str;
			}
			return res;
		}

		public virtual void Replace(TextEditor sourceTextEditor, TextEditor destTextEditor) {
			Contract.Requires(sourceTextEditor != null, "sourceTextEditor!=null");
			Contract.Requires(destTextEditor != null, "destTextEditor!=null");
			var match = GetMatchConfig(sourceTextEditor);
			if (match.Success) {
				Config.SetSelectedItem(this);
				var destLine = destTextEditor.Document.GetLineByNumber(Config.LineNumber);
				destTextEditor.Document.Replace(destLine.Offset, destLine.Length, match.ResultLine);
			}
		}

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
		protected virtual void ReplaceText(object textEditors) {
			var textEditorData = TextEditorsParameterConverter.GetEditorsFromParameter(textEditors);
			var match = GetMatchConfig(textEditorData.SourceTextEditor);
			if (match.Success) {
				Config.SetSelectedItem(this);
				var destLine = textEditorData.DestinationTextEditor.Document.GetLineByNumber(Config.LineNumber);
				textEditorData.DestinationTextEditor.Document.Replace(destLine.Offset, destLine.Length, match.ResultLine);
			}
		}

		#endregion

		#region IsSelected property

		/// <summary>
		/// Gets or sets the IsSelected value.
		/// </summary>
		public bool IsSelected
		{
			get { return GetValue<bool>(IsSelectedProperty); }
			set { SetValue(IsSelectedProperty, value); }
		}

		/// <summary>
		/// IsSelected property data.
		/// </summary>
		public static readonly PropertyData IsSelectedProperty = RegisterProperty("IsSelected", typeof (bool));

		#endregion

		#region Background property

		/// <summary>
		/// Gets or sets the Background value.
		/// </summary>
		public SolidColorBrush Background
		{
			get { return GetValue<SolidColorBrush>(BackgroundProperty); }
			set { SetValue(BackgroundProperty, value); }
		}

		/// <summary>
		/// Background property data.
		/// </summary>
		public static readonly PropertyData BackgroundProperty = RegisterProperty("Background", typeof (SolidColorBrush));

		#endregion
	}

	public class NewLineTranslationVariant : LineTranslationVariant {

		private bool _edited = false;
		private readonly IDialogService _dialogService;
		private ValueStoreGroup _valueStoreGroup;
		private FeatureLocalizer.FeatureLocalizer _featureLocalizer;

		public NewLineTranslationVariant(FeatureLocalizer.FeatureLocalizer localizer, ValueStoreGroup vsGroup, IDialogService dialogService) {
			Contract.Requires(dialogService != null, "dialogService != null");
			_dialogService = dialogService;
			_valueStoreGroup = vsGroup;
			_featureLocalizer = localizer;
			Background = new SolidColorBrush(Colors.ForestGreen);
		}

		protected override void ReplaceText(object textEditors) {
			NewVariantDialogResult res;
			if (_edited) {
				res = _dialogService.ShowNewVariantDialog(NameRu, NameEng);
			}
			else {
				var textEditorData = TextEditorsParameterConverter.GetEditorsFromParameter(textEditors);
				var word = Config.GetLineWord(textEditorData.SourceTextEditor);
				res = _dialogService.ShowNewVariantDialog(word, String.Empty);
			}
			if (res != null) {
				NameRu = res.NameRu;
				NameEng = res.NameEng;
				_valueStoreGroup.Items.Add(new ValueStoreItem(res.NameEng, res.NameRu));
				_featureLocalizer.ValueStore.Save();
				base.ReplaceText(textEditors);
				_edited = true;
				Config.RefreshVariants();
			}
			else {
				IsSelected = false;
			}
		}
	}
}
