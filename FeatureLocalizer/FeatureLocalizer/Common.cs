using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace FeatureLocalizer {
	public static class ExtensionUtils {
		public static List<string> GetValidNames(this Regex regex) {
			var str = regex.ToString();
			str = str.Replace(">", string.Empty);
			str = str.Replace("<", string.Empty);
			str = str.Replace("&", string.Empty);
			str = str.Replace("!", string.Empty);
			str = str.Replace("\"", string.Empty);
			str = str.Trim();
			var res = regex.GetGroupNumbers().Skip(1).ToList().ConvertAll(n => string.Format("{0}_{1}", str, n));
			return res;
		}
	}
}
