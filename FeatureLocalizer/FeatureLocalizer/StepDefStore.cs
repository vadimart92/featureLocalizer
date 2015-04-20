using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace FeatureLocalizer {
	public class StepDefStore {
		private const string ItemName = "item";

		public IReadOnlyCollection<StepDef> StepDefs {
			get { return _stepDefs; }
		}

		private readonly ValueStore _valueStore;
		private readonly List<StepDef> _stepDefs;

		public StepDefStore(ValueStore valueStore) {
			_stepDefs = new List<StepDef>();
			_valueStore = valueStore;
		}

		public StepDefStore(string stepDefsDir, ValueStore valueStore) : this(valueStore) {
			var files = Directory.EnumerateFiles(stepDefsDir, "*.xml");
			foreach (var file in files) {
				var stepDefs = ParseStepDef(file);
				_stepDefs.AddRange(stepDefs);
			}
		}

		private List<StepDef> ParseStepDef(string fileName) {
			var xml = XElement.Load(fileName);
			var items = xml.Descendants(ItemName);
			return items.Select(n => StepDef.Load(n,_valueStore)).ToList();
		}

		public void AddStepDef(Regex regex) {
			var stepDef = new StepDef(regex, _valueStore);
			_stepDefs.Add(stepDef);
		}

		public void Save(string stepDefsDir, string name) {
			var fileName = string.Format("{0}\\{1}.xml", stepDefsDir, name);
			var xEl = new XElement("stepDefs", SerializeItems());
			xEl.Save(fileName);
		}

		public List<XElement> SerializeItems() {
			var res = new List<XElement>();
			foreach (var def in _stepDefs) {
				bool thereIsItems = false;
				var g = new XElement("item");
				g.Add(new XElement("re", string.Format(@"/^{0}$/", def.Exp)));
				foreach (var parameter in def.Params) {
					if (parameter.ValueStoreGroup.Items.Count > 0) {
						thereIsItems = true;
					}
					g.Add(new XElement("param", new XAttribute("vsGroup", parameter.ValueStoreGroup.Name)));
				}
				if (thereIsItems) {
					res.Add(g);
				}
			}
			return res;
		} 
	}
} 
