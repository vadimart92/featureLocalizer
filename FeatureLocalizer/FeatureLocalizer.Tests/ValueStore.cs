using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FeatureLocalizer.Tests {
	[TestClass]
	public class ValueStoreTest {
		[TestMethod]
		public void CheckExistsData() {
			var valueStore = new ValueStore();
			valueStore.Load(@"F:\DEV\featureLocalizer\config\valueStores");
			Assert.AreEqual("##RU=Контрагенты;EN=Accounts##", valueStore.GetValueMacros("SectionCaption", "Контрагенты"));
		}
		[TestMethod]
		public void CheckNotExistsData() {
			var valueStore = new ValueStore();
			valueStore.Load(@"F:\DEV\featureLocalizer\config\valueStores");
			Assert.AreEqual(string.Empty, valueStore.GetValueMacros("SectionCaption1", "Контрагенты"));
		}
	}
}
