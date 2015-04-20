using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace FeatureLocalizer {
	[DebuggerDisplay("Exp: {Exp}, Params: {Params.Count}")]
	public class StepDef {
		protected StepDef() {
			Params = new List<StepDefParameter>();
		}

		public StepDef(string exp):this() {
			Exp = new Regex(exp);
		}

		public StepDef(Regex exp, ValueStore valueStore) {
			Exp = exp;
			Params = GetStepDefParameters(exp, valueStore);
		}

		private const string ReNodeName = "re";
		private const string ParamNodeName = "param";

		public Regex Exp { get; private set; }
		public List<StepDefParameter> Params {
			get;
			private set;
		}

		public static StepDef Load(XElement node, ValueStore valueStore) {
			var re = node.Descendants(ReNodeName).FirstOrDefault();
			if (re == null) {
				return null;
			}
			var reStr = GetRexpFromReNode(re.Value);
			if (string.IsNullOrEmpty(re.Value)) {
				return null;
			}
			var sd = new StepDef(reStr);
			UInt16 i = 0;
			var sdParams = node.Descendants(ParamNodeName)
				.Select(n => StepDefParameter.Load(i++, n, valueStore))
				.ToList();
			sd.Params.AddRange(sdParams);
			return sd;
		}
		
		private static string GetRexpFromReNode(string reNodeValue) {
			return reNodeValue.Substring(2).Substring(0, reNodeValue.Length - 4);
		}

		private static List<StepDefParameter> GetStepDefParameters(Regex regex, ValueStore valueStore) {
			var res = new List<StepDefParameter>();
			var names = regex.GetValidNames();
			foreach (var name in names) {
				if (valueStore.StoreGroups.ContainsKey(name)) {
					var group = valueStore.StoreGroups[name];
					var param = new StepDefParameter(group, 0);
					res.Add(param);
				}
			}
			return res;
		}
	}
}
