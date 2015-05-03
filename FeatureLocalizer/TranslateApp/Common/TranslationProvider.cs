using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using Catel.Collections;
using GherkinParser;

namespace TranslateApp.Common {
	public class TranslationProvider {

		private FeatureLocalizer.FeatureLocalizer _featureLocalizer;

		private Dictionary<GherkinStep, List<LineTranslationConfig>> _fileInfo;

		public TranslationProvider() {
			var configDir = Path.Combine(Directory.GetCurrentDirectory(), "config");
			_featureLocalizer = new FeatureLocalizer.FeatureLocalizer(configDir);
		}

		public List<LineTranslationConfig> GeTranslationConfigs(string fileName) {
			var fileInfo = LexicParcer.ParseFile(fileName);
			var res = new List<LineTranslationConfig>();
			foreach (var step in fileInfo.Steps) {
				var stepDef = _featureLocalizer.StepDefStore.StepDefs.FirstOrDefault(s => s.Exp.IsMatch(step.Text));
				if (stepDef == null) continue;
				foreach (var parameter in stepDef.Params) {
					var config = new LineTranslationConfig {
						LineNumber = step.Line,
						Regex = stepDef.Exp,
						Variants = new ObservableCollection<LineTranslationVariant>()
					};
					List<string> existItems = new List<string>();
					foreach (var storeItem in parameter.ValueStoreGroup.Items) {
						if (existItems.Contains(storeItem.RuRu, StringComparer.OrdinalIgnoreCase)) {
							continue;
						}
						existItems.Add(storeItem.RuRu);
						config.Variants.Add(new LineTranslationVariant {
							Config = config,
							NameRu = storeItem.RuRu,
							NameEng = storeItem.EnUs
						});
					}
					config.Variants.Sort((variant, translationVariant) => String.Compare(variant.NameRu, translationVariant.NameRu, StringComparison.OrdinalIgnoreCase));
					res.Add(config);
				}
			}
			return res;
		}
	}
}
