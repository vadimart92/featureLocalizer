using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;

namespace TranslateApp.Common {
	public class TranslateComletionData: ICompletionData {
		private readonly LineTranslationVariant _lineTranslationVariant;
		private readonly int _startOffset;
		private readonly int _length;

		public TranslateComletionData(LineTranslationVariant variant, int startOffset, int length) {
			_lineTranslationVariant = variant;
			_startOffset = startOffset;
			_length = length;
		}

		public System.Windows.Media.ImageSource Image {
			get { return null; }
		}
		
		public string Text { get; private set; }
		
		public object Content {
			get { return _lineTranslationVariant.NameRu; }
		}
		
		public object Description {
			get { return _lineTranslationVariant.MacrosValue; }
		}
		
		public double Priority { get { return 0; } }
		
		public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs) {
			textArea.Document.Replace(_startOffset, _length, _lineTranslationVariant.MacrosValue);
		}
	}
	public class NewTranslateComletionData : ICompletionData {

		public NewTranslateComletionData() {
		}

		public System.Windows.Media.ImageSource Image {
			get {
				return null;
			}
		}

		public string Text {
			get {
				return "<new value>";
			}
		}

		public object Content {
			get {
				return "<new value>";
			}
		}

		public object Description {
			get {
				return "<new value>";
			}
		}

		public double Priority {
			get {
				return 0;
			}
		}

		public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs) {
			textArea.Document.Replace(completionSegment, Text);
		}
	}
}
