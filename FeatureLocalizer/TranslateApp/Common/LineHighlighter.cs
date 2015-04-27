using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Rendering;

namespace TranslateApp.Common {
	public class ColorizeAvalonEdit : ICSharpCode.AvalonEdit.Rendering.DocumentColorizingTransformer {
		public int LineNum {
			get;
			set;
		}

		protected override void ColorizeLine(ICSharpCode.AvalonEdit.Document.DocumentLine line) {
			if (line.LineNumber == LineNum) {
				base.ChangeLinePart(line.Offset,line.EndOffset, (VisualLineElement element) => {
						element.TextRunProperties.SetBackgroundBrush(Brushes.Red);
					});
			}
		}
	}
}
