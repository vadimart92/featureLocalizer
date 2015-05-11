using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace FeatureLocalizer {
	public class ValueStore {
		  
		private Dictionary<string, ValueStoreGroup> _storeGroups;

		private string _configFilesDir;

		public Dictionary<string, ValueStoreGroup> StoreGroups {
			get { return _storeGroups; }
			set { _storeGroups = value; }
		}

		public ValueStore() {
			_storeGroups = new Dictionary<string, ValueStoreGroup>();
		}
		
		public ValueStore(string valueStoresDirectory) : this() {
			if (!Directory.Exists(valueStoresDirectory)) return;
			_configFilesDir = valueStoresDirectory;
			foreach (var file in Directory.EnumerateFiles(valueStoresDirectory, "*.xml", SearchOption.AllDirectories)) {
				var text = File.ReadAllText(file);
				var xml = XElement.Parse(text);
				foreach (var group in xml.Descendants("group")) {
					var g = new ValueStoreGroup(group);
					AddGroup(g);
				}
			}
		}

		public void Save(string valueStoresDirectory, string name) {
			var fileName = string.Format("{0}\\{1}.xml", valueStoresDirectory, name);
			var xEl = new XElement("valueStoreGroups", SerializeGroups());
			xEl.Save(fileName);
		}

		public void Save() {
			var name = "full";
			var fileName = string.Format("{0}\\{1}.xml", _configFilesDir, name);
			var oldFiles = Directory.EnumerateFiles(_configFilesDir);
			var parentDir = Path.Combine(Directory.GetParent(_configFilesDir).FullName, "oldValueStoe");
			if (!Directory.Exists(parentDir))
				Directory.CreateDirectory(parentDir);
			foreach (var file in oldFiles) {
				var oldFi = new FileInfo(file);
				var bakFile = String.Format("{0}\\{1}", parentDir, oldFi.Name);
				File.Delete(bakFile);
				File.Move(file, bakFile);
			}
			var xEl = new XElement("valueStoreGroups", SerializeGroups());
			xEl.Save(fileName);
		}

		public List<XElement> SerializeGroups() {
			var res = new List<XElement>();
			foreach (var @group in _storeGroups) {
				var g = new XElement("group", new XAttribute("name", group.Value.Name));
				if (@group.Value.Items.Count > 0) {
					foreach (var item in @group.Value.Items) {
						g.Add(item.Seriallize());
					}
					res.Add(g);
				}
				
			}
			return res;
		}

		public void AddValuesToGroup(string groupName, IEnumerable<ValueStoreItem> valueItems) {
			ValueStoreGroup group;
			if (!StoreGroups.ContainsKey(groupName)) {
				group = new ValueStoreGroup(groupName);
				StoreGroups.Add(groupName, group);
			} else {
				group = StoreGroups[groupName];
			}
			group.Items.AddRange(valueItems);
		}

		public void AddGroup(ValueStoreGroup group) {
			if (!StoreGroups.ContainsKey(group.Name)) {
				StoreGroups.Add(group.Name, group);
			} else {
				group = StoreGroups[group.Name];
				group.Items.AddRange(group.Items);
			}
		}

		public string GetValueMacros(string groupName, string currentValue) {
			if (!StoreGroups.ContainsKey(groupName)) {
				return string.Empty;
			}
			var group = StoreGroups[groupName];
			var valueItemIndex = group.Items.BinarySearch(new ValueStoreItem(currentValue), ValueStoreItem.RuValueComparer);
			if (valueItemIndex < 0) return string.Empty;
			var value = group.Items[valueItemIndex];
			return value.ToString();
		}
	}
}
