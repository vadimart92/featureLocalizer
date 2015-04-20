using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GherkinParser {
	public class FeatureFileInfo {
		public string FilePath { get; set; }
		public List<GherkinStep> Steps { get; set; }

		public FeatureFileInfo(string filePath, List<GherkinStep> steps) {
			FilePath = filePath;
			Steps = steps;
		}
	}
}
