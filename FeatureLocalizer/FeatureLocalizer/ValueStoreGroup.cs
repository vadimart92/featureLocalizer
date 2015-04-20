using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;

namespace FeatureLocalizer {
	 [DebuggerDisplay("Name: {Name}, Items: {Items.Count}")]
	public class ValueStoreGroup {
		private string _name = string.Empty;
		private List<ValueStoreItem> _items = new List<ValueStoreItem>();

		public string Name {
			get { return _name; } 
			private set { _name = value; }
		}

		public List<ValueStoreItem> Items {
			get { return _items; }
			private set {
				_items = value;
			}
		}

		public ValueStoreGroup(string name) {
			_name = name;
		}

		public ValueStoreGroup(XElement node) {
			if (node == null) {
				return;
			}
			if (node.HasAttributes) {
				var nameAttr = node.Attribute("name");
				if (nameAttr != null) {
					Name = nameAttr.Value;
					var values = node.Descendants("item").ToList();
					var vsis = values.ConvertAll(n => new ValueStoreItem(n)).ToList();
					Items.AddRange(vsis);
				}
			}
		}

		 public override string ToString() {
			 return string.Format("Name: {0}, Items: {1}", Name, Items.Count);
		 }
	 }
}
