using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using Catel.Collections;
using GherkinParser;

namespace TranslateApp.Common {
	public class TranslationProvider {

		private string _configDir;
		private FeatureLocalizer.FeatureLocalizer _featureLocalizer;

		private Dictionary<GherkinStep, List<LineTranslationConfig>> _fileInfo;

		public TranslationProvider() {
			ReloadInfo();
		}

		public void ReloadInfo() {
			_configDir = Path.Combine(Directory.GetCurrentDirectory(), "config");
			_featureLocalizer = new FeatureLocalizer.FeatureLocalizer(_configDir);
		}

		public List<LineTranslationConfig> GeTranslationConfigs(string fileName, IDialogService dialogService) {
			var fileInfo = LexicParcer.ParseFile(fileName);
			var res = new List<LineTranslationConfig>();
			foreach (var step in fileInfo.Steps) {
				var stepDef = _featureLocalizer.StepDefStore.StepDefs.FirstOrDefault(s => s.Exp.IsMatch(step.Text));
				if (stepDef == null) continue;
				foreach (var parameter in stepDef.Params) {
					var config = new LineTranslationConfig {
						LineNumber = step.Line,
						Regex = stepDef.Exp,
						WordIndex = parameter.Index,
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
					config.Variants.Add(new NewLineTranslationVariant(_featureLocalizer, parameter.ValueStoreGroup, dialogService) {
						Config = config,
						NameRu = "+ Добавить",
						NameEng = "Add new"
					});
					config.SortVariants();
					res.Add(config);
				}
			}
			return res;
		}

		public Task Learn(string directory) {
			return Task.Run(() => {
				_featureLocalizer.Learn(directory);
			});
		}

		public Task SaveData() {
			return Task.Run(() => {
				_featureLocalizer.SaveLearnedData(_configDir);
			});
		}

		public string ConfigDir { get { return _configDir; } }
	}
}
