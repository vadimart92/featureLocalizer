using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FeatureLocalizer {
	[DebuggerDisplay("Index: {Index}, ValueStoreGroup: {ValueStoreGroup}")]
 	public class StepDefParameter {
 		public StepDefParameter(ValueStoreGroup valueStoreGroup, UInt16 index) {
 			ValueStoreGroup = valueStoreGroup;
 			Index = index;
 		}

 		private const string ValueStoreGroupAttributename = "vsGroup";

 		public ValueStoreGroup ValueStoreGroup { get; private set; }
 		public UInt16 Index { get; private set; }

 		public static StepDefParameter Load(UInt16 index, XElement node, ValueStore valueStore) {
 			if (node == null || !node.HasAttributes) {
 				return null;
 			}
 			var vsGroupAttr = node.Attribute(ValueStoreGroupAttributename);
			if (vsGroupAttr == null || string.IsNullOrWhiteSpace(vsGroupAttr.Value)) {
 				return null;
 			}
			var vsGroupName = vsGroupAttr.Value;
 			if (!valueStore.StoreGroups.ContainsKey(vsGroupName)) {
 				return null;
 			}
 			var vsGroup = valueStore.StoreGroups[vsGroupName];
			var res = new StepDefParameter(vsGroup, index);
 			return res;
 		}
 	}
}
