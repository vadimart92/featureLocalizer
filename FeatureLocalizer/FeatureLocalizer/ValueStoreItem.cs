using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FeatureLocalizer {

	[DebuggerDisplay("Ru: {RuRu}, En: {EnUs}")]
	public struct ValueStoreItem {
		public bool Equals(ValueStoreItem other) {
			return string.Equals(_ruRu, other._ruRu);
		}

		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) return false;
			return obj is ValueStoreItem && Equals((ValueStoreItem) obj);
		}

		public override int GetHashCode() {
			return (_ruRu != null ? _ruRu.GetHashCode() : 0);
		}

		private static Regex _macrosRegex = new Regex(@"##(RU=[\s\S]+;EN=[\s\S]+)##");
		private static Regex _ruRegex = new Regex(@"RU=([^;]+)");
		private static Regex _enRegex = new Regex(@"EN=([^##]+)");
		public static RuValueComparer RuValueComparer = new RuValueComparer();
		private string _enUs;
		private string _ruRu;

		public string EnUs {
			get { return _enUs; }
			private set { _enUs = value; }
		}

		public string RuRu {
			get { return _ruRu; }
			private set { _ruRu = value; }
		}

		public ValueStoreItem(string allValues) {
			_ruRu = allValues;
			_enUs = allValues;
		}
		
		public ValueStoreItem(string en, string ru) {
			_ruRu = ru;
			_enUs = en;
		}

		public ValueStoreItem(XElement node) {
			var ruNode = node.Descendants("ru").FirstOrDefault();
			var enNode = node.Descendants("en").FirstOrDefault();
			if (ruNode != null && enNode != null) {
				_enUs = enNode.Value;
				_ruRu = ruNode.Value;
			}
			else {
				_enUs = string.Empty;
				_ruRu = string.Empty;
			}
		}

		public XElement Seriallize() {
			return new XElement("item", new XElement("ru", _ruRu), new XElement("en", _enUs));
		}

		public static ValueStoreItem FromMacros(string stepLineText, int paramNumber) {
			var match = _macrosRegex.Match(stepLineText);
			if (match.Success && paramNumber < match.Groups.Count) {
				var macrosValue = match.Groups[1 + paramNumber].Value;
				var ruMach = _ruRegex.Match(macrosValue);
				var enMach = _enRegex.Match(macrosValue);
				if (ruMach.Groups.Count == 2 && enMach.Groups.Count == 2) {
					var res = new ValueStoreItem(enMach.Groups[1].Value, ruMach.Groups[1].Value);
					return res;
				}	
			}
			return new ValueStoreItem(string.Empty);
		}

		public override string ToString() {
			return string.Format("##RU={0};EN={1}##", RuRu, EnUs);
		}
		
		public static bool operator ==(ValueStoreItem item1, ValueStoreItem item2) {
			if (item1.RuRu == null) {
				item1.RuRu = string.Empty;
			}
			if (item2.RuRu == null) {
				item2.RuRu = string.Empty;
			}
			return RuValueComparer.Compare(item1, item2) == 0;
		}
		public static bool operator !=(ValueStoreItem item1, ValueStoreItem item2) {
			if (item1.RuRu == null) {
				item1.RuRu = string.Empty;
			}
			if (item2.RuRu == null) {
				item2.RuRu = string.Empty;
			}
			return RuValueComparer.Compare(item1, item2) != 0;
		}
	}

	public class RuValueComparer : IComparer<ValueStoreItem>, IEqualityComparer<ValueStoreItem> {

		#region Члены IComparer<ValueStoreItem>

		public int Compare(ValueStoreItem x, ValueStoreItem y) {
			return string.CompareOrdinal(x.RuRu.Trim().ToLowerInvariant(), y.RuRu.Trim().ToLowerInvariant());
		}

		#endregion

		public bool Equals(ValueStoreItem x, ValueStoreItem y) {
			if (x.RuRu != null) {
				return x.RuRu.Equals(y.RuRu, StringComparison.OrdinalIgnoreCase);
			}
			return y.RuRu == null;
		}

		public int GetHashCode(ValueStoreItem obj) {
			if (obj.RuRu != null) {
				return obj.RuRu.GetHashCode();
			}
			return 0;
		}
	} 

}
