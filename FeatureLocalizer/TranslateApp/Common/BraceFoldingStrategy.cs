using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Folding;

namespace TranslateApp.Common {
	public class BraceFoldingStrategy : AbstractFoldingStrategy {
		/// <summary>
		/// Creates a new BraceFoldingStrategy.
		/// </summary>
		public BraceFoldingStrategy() {

		}
		public Int32 TextHash {
			get;
			set;
		}

		public List<NewFolding> Cashe {
			get;
			set;
		}
		/// <summary>
		/// Create <see cref="NewFolding"/>s for the specified document.
		/// </summary>
		public override IEnumerable<NewFolding> CreateNewFoldings(TextDocument document, out int firstErrorOffset) {
			firstErrorOffset = -1;
			return CreateNewFoldings(document);
		}

		/// <summary>
		/// Create <see cref="NewFolding"/>s for the specified document.
		/// </summary>
		public IEnumerable<NewFolding> CreateNewFoldings(ITextSource document) {
			var textHash = document.Text.GetHashCode();
			if (textHash == TextHash) {
				return Cashe;
			}
			List<NewFolding> newFoldings = new List<NewFolding>();
			var text = document.Text;
			var bg = GetBackgroundFolding(document.Text);
			int indexDiff = 0;
			if (bg != null) {
				newFoldings.Add(bg);
				text = text.Substring(bg.EndOffset);
				indexDiff = bg.EndOffset;
			}
			var sfs = GetScenarioFoldings(text, indexDiff);
			newFoldings.AddRange(sfs);
			newFoldings.Sort((a, b) => a.StartOffset.CompareTo(b.StartOffset));
			TextHash = textHash;
			Cashe = newFoldings;
			return newFoldings;
		}

		NewFolding GetBackgroundFolding(string text) {
			const string BgName = "Background:";
			var bgIndex = text.IndexOf(BgName, StringComparison.InvariantCultureIgnoreCase);
			if (bgIndex > -1) {
				var startIndex = bgIndex + BgName.Length;
				var atIndex = text.IndexOf("@", startIndex, StringComparison.InvariantCultureIgnoreCase);
				var sharpIndex = text.IndexOf("#", startIndex, StringComparison.InvariantCultureIgnoreCase);
				var endIndex = Math.Min(atIndex, sharpIndex);
				if (startIndex > endIndex) {
					return null;
				}
				endIndex = GetEndOffset(text, startIndex, endIndex);
				return new NewFolding(startIndex, endIndex);
			} else {
				return null;
			}
		}

		private static int GetEndOffset(string text, int startIndex, int endIndex) {
			var foundPrevNewLine = false;
			char returnSymbol = "\r"[0];
			char newLineSymbol = "\n"[0];
			bool nexIsNotReturn = false;
			do {
				endIndex -= 1;
				var prevChar = text[endIndex];
				if (!foundPrevNewLine) {
					foundPrevNewLine = prevChar == returnSymbol || prevChar == newLineSymbol;
				} else {
					nexIsNotReturn = prevChar != returnSymbol && prevChar != newLineSymbol;
				}
			} while (!(foundPrevNewLine && nexIsNotReturn) && (endIndex > startIndex));
			return endIndex + 1;
		}

		List<NewFolding> GetScenarioFoldings(string text, int indexDiff) {
			const string ScenarioName = "Scenario";
			var res = new List<NewFolding>();
			var searchText = string.Copy(text);
			var scenarioIndex = searchText.IndexOf(ScenarioName);
			var endIndex = GetEndIndex(ScenarioName, searchText, scenarioIndex);
			while (scenarioIndex > -1 && endIndex > -1) {
				scenarioIndex = searchText.IndexOf("\r", scenarioIndex, StringComparison.OrdinalIgnoreCase);
				endIndex = GetEndOffset(searchText, scenarioIndex, endIndex) - 1;
				res.Add(new NewFolding(scenarioIndex + indexDiff, endIndex + indexDiff));
				searchText = searchText.Substring(endIndex);
				indexDiff += endIndex;
				scenarioIndex = searchText.IndexOf(ScenarioName);
				endIndex = endIndex = GetEndIndex(ScenarioName, searchText, scenarioIndex);
			}
			if (scenarioIndex != -1 && endIndex == -1) {
				scenarioIndex = searchText.IndexOf("\r", scenarioIndex, StringComparison.OrdinalIgnoreCase);
				res.Add(new NewFolding(scenarioIndex + indexDiff, searchText.Length + indexDiff));
			}
			return res;
		}

		private static int GetEndIndex(string ScenarioName, string searchText, int scenarioIndex) {
			var endIndexes = new List<int> { searchText.IndexOf(ScenarioName, scenarioIndex + 1), searchText.IndexOf("@", scenarioIndex + 1) };
			var sharpIndex = searchText.IndexOf("#", scenarioIndex + 1);
			if (searchText.IndexOf("##", scenarioIndex + 1) != sharpIndex) {
				endIndexes.Add(sharpIndex);
			}
			var endIndex = endIndexes.Min();
			return endIndex;
		}
	}
}
