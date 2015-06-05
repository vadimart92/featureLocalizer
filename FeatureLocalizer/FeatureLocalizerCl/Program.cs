using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeatureLocalizer;
using GherkinParser;

namespace FeatureLocalizerCl {
	class Program {
		static void Main(string[] args) {



			string featureFilePath = @"F:\DEV\Cucumber.js\";


			//var infos = LexicParcer.ParseFolder(featureFilePath);

			var fl = new FeatureLocalizer.FeatureLocalizer("");
			fl.Learn(featureFilePath);
			fl.SaveLearnedData(@"F:\DEV\featureLocalizer\config");
			fl.Translate(@"F:\DEV\featureLocalizer\config", @"F:\DEV\featureLocalizer\sales\features\order");
		}
	}
}
