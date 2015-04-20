using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Gherkin;
using Gherkin.Ast;

namespace GherkinParser
{
    public class LexicParcer {
		private static List<ErrorInfo> _errorFiles = new List<ErrorInfo>(); 
		private static Regex _exampleRegex= new Regex(@"<[\s\S]+?>");

	    public static FeatureFileInfo ParseFile(string file) {
			var parser = new Parser<object>();
		    Feature feature;
			using (var reader = new StreamReader(file)) {
				var ts = new TokenScanner(reader);
				try {
					feature = (Feature)parser.Parse(ts);
				} catch (CompositeParserException ex) {
					_errorFiles.Add(new ErrorInfo {
						FilePath = file,
						CompositeException = ex
					});
					return null;
				} catch (NullReferenceException ex) {
					_errorFiles.Add(new ErrorInfo {
						FilePath = file,
						Exception = ex
					});
					return null;
				}
				ts = null;
			}
		    var steps = (from scenario in feature.ScenarioDefinitions
						 from step in scenario.Steps
						 let exampleSteps = GetSteps(step, scenario)
						 from s in exampleSteps
						 select s).ToList();
			var featureInfo = new FeatureFileInfo(file, steps);
		    return featureInfo;
	    }

		public static List<GherkinStep> GetSteps(Step step, ScenarioDefinition scenario) {
			var res = new List<GherkinStep>();
			var outline = scenario as ScenarioOutline;
			var stepText = step.Text;
			if (outline != null && stepText.Contains("<") && stepText.Contains(">")) {
				var m = _exampleRegex.Match(stepText);
				if (m.Success) {
					for (int i = 0; i < m.Groups.Count; i++) {
						var exampleValue = m.Groups[i].Value.Replace("<", string.Empty).Replace(">", string.Empty);
						var resStep = GetGherkinStep(step, exampleValue);
						res.Add(resStep);
					}
				}
			}
			else {
				var resStep = GetGherkinStep(step, stepText);
				res.Add(resStep);
			}
			return res;
		}

	    private static GherkinStep GetGherkinStep(Step step, string stepText) {
		    var resStep = new GherkinStep {
			    Text = stepText,
			    Column = (ushort) step.Location.Column,
			    Line = (ushort) step.Location.Line,
			    Keyword = step.Keyword
		    };
		    return resStep;
	    }

	    public static List<FeatureFileInfo> ParseFolder(string directory, bool includeSubdirs = true) {
			_errorFiles.Clear();
			var files = Directory.EnumerateFiles(directory, "*.feature",
			    includeSubdirs ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).ToList();
			var res = new List<FeatureFileInfo>(files.Count);
		    res.AddRange(files.AsParallel().Select(ParseFile).Where(r=>r!= null));
		    return res;
	    }

    }

	public class ErrorInfo {
		public string FilePath { get; set; }
		public CompositeParserException CompositeException { get; set; }
		public Exception Exception { get; set; }
	}
}
