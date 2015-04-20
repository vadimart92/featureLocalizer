using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GherkinParser {
	public class GherkinStep {
		private string _keyword;

		public string Text { get; set; }
		public UInt16 Line { get; set; }
		public UInt16 Column { get; set; }

		public string Keyword {
			get { return _keyword; }
			set {
				_keyword = string.Intern(value);
			}
		}
	}
}
