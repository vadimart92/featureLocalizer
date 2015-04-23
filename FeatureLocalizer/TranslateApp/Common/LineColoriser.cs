using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Rendering;

namespace TranslateApp.Common {
	
	class Def : IHighlightingDefinition {

		#region Члены IHighlightingDefinition

		public HighlightingColor GetNamedColor(string name) {
			return new HighlightingColor {
				Background = new SimpleHighlightingBrush(Colors.Coral), Foreground = new SimpleHighlightingBrush(Colors.Red)
			};
		}

		public HighlightingRuleSet GetNamedRuleSet(string name) {
			return new HighlightingRuleSet() {
				Rules = { new HighlightingRule() { Regex = new Regex("/And/g"), Color = GetNamedColor("DarkBlue") } }
			};
		}

		public HighlightingRuleSet MainRuleSet {
			get { return GetNamedRuleSet("main"); }
		}

		public string Name {
			get { return "x"; }
		}

		public IEnumerable<HighlightingColor> NamedHighlightingColors {
			get { return new List<HighlightingColor> {
				new HighlightingColor() { Foreground = new SimpleHighlightingBrush(Colors.DarkBlue), Name = "DarkBlue"}
			}; }
		}

		public IDictionary<string, string> Properties {
			get { return new ConcurrentDictionary<string, string>(); }
		}

		#endregion
	}
}
