using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GherkinParser;

namespace FeatureLocalizer {
	public class FeatureLocalizer {

		private const string ValueStoreDirName = @"valueStores";
		private const string StepDefDirName = @"stepDefs";
		private readonly Regex _multiLangRegex = new Regex("##(RU=[\\s\\S]+;EN=[\\s\\S]+)##");

		private Dictionary<Regex, ConcurrentBag<GherkinStep>> _stepDefRegexes = new Dictionary<Regex, ConcurrentBag<GherkinStep>>();

		public void Translate(string configDir, string featuresDir) {
			var vsDir = Path.Combine(configDir, ValueStoreDirName);
			var sdDir = Path.Combine(configDir, StepDefDirName);
			var valueStore = new ValueStore(vsDir);
			var stepDefStore = new StepDefStore(sdDir, valueStore);
		}

		public void Learn(string cucumberProjectDir) {
			GetStepDefList(cucumberProjectDir);
			var featuresData = LexicParcer.ParseFolder(cucumberProjectDir);
			Parallel.ForEach(featuresData, (fileInfo) => {
				foreach (var step in fileInfo.Steps) {
					foreach (var stepDefRegex in _stepDefRegexes) {
						if (!_multiLangRegex.IsMatch(step.Text)) {
							continue;
						}
						var mach = stepDefRegex.Key.Match(step.Text);
						if (!mach.Success)
							continue;
						stepDefRegex.Value.Add(step);
						break;
					}
				}
			});
			var data = _stepDefRegexes.Where(i => i.Value.Count > 0).ToDictionary(i=>i.Key, i=>i.Value);
			_stepDefRegexes = data;
		}

		public void SaveLearnedData(string configDir) {
			var valueStore = new ValueStore();
			var stepDefStore = new StepDefStore(valueStore);
			foreach (var stepDefRegex in _stepDefRegexes) {
				var paramNames = stepDefRegex.Key.GetValidNames();
				for (int paramNumber = 0; paramNumber < paramNames.Count; paramNumber++) {
					var group = new ValueStoreGroup(paramNames[paramNumber]);
					foreach (var gherkinStep in stepDefRegex.Value) {
						var item = ValueStoreItem.FromMacros(gherkinStep.Text, paramNumber);
						if (item == default(ValueStoreItem)) {
							continue;
						}
						var foundItemIndex = group.Items.BinarySearch(item, ValueStoreItem.RuValueComparer);
						if (foundItemIndex < 0) {
							group.Items.Add(item);
						}
					}
					valueStore.AddGroup(group);
					stepDefStore.AddStepDef(stepDefRegex.Key);
				}
			}
			var vsDir = Path.Combine(configDir, ValueStoreDirName);
			var sdDir = Path.Combine(configDir, StepDefDirName);
			valueStore.Save(vsDir, "learned");
			stepDefStore.Save(sdDir, "learned");
		}

		private void GetStepDefList(string cucumberProjectDir) {
			var stepDefRegexes = new List<Regex>();
			var folders = Directory.EnumerateDirectories(cucumberProjectDir, "step_definitions", SearchOption.AllDirectories);
			foreach (var folder in folders) {
				var files = Directory.EnumerateFiles(folder, "*.js", SearchOption.TopDirectoryOnly);
				foreach (var file in files) {
					var fileData = GetStepDefData(file);
					stepDefRegexes.AddRange(fileData);
				}
			}
			ClearEmptyStedDefs(stepDefRegexes);
		}

		private void ClearEmptyStedDefs(List<Regex> stepDefRegexes) {
			var res =  new HashSet<string>();
			foreach (var stepDefRegex in stepDefRegexes) {
				var groupNumbers = stepDefRegex.GetGroupNumbers();
				var text = string.Intern(stepDefRegex.ToString());
				if (!res.Contains(text) && groupNumbers.Count() > 1) {
					res.Add(text);
					_stepDefRegexes[stepDefRegex] = new ConcurrentBag<GherkinStep>();
				}
			}
		}

		private List<Regex> GetStepDefData(string filePath) {
			var res = new List<Regex>();
			var lines = from line in File.ReadAllLines(filePath)
						let trimmedLine = line.TrimStart()
						where trimmedLine.StartsWith("this.When") || trimmedLine.StartsWith("this.Given") || trimmedLine.StartsWith("this.Then")
						select trimmedLine;
			var reStrings = lines.Select(GetReString).Where(s=>!string.IsNullOrWhiteSpace(s));
			var reCollection = reStrings.Select(reStr => new Regex(reStr));
			res.AddRange(reCollection);
			return res;
		}

		private string GetReString(string line) {
			if (line == null) return string.Empty;
			var start = line.IndexOf(@"(/^", StringComparison.Ordinal);
			var end = line.LastIndexOf(@"$/,", StringComparison.Ordinal);
			if (end < 0) {
				end = line.LastIndexOf(@"/,", StringComparison.Ordinal);
			}
			end = end - start;
			if (start > 0 && end < line.Length) {
				return line.Substring(start + 3, end - 3);
			}
			return string.Empty;
		}

	} 
}
