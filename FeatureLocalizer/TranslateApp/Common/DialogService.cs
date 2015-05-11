using System.Windows;
using TranslateApp.Views;

namespace TranslateApp.Common {

	public class NewVariantDialogResult {
		public string NameRu { get; set; }
		public string NameEng { get; set; }
	}

	public interface IDialogService {
		NewVariantDialogResult ShowNewVariantDialog(string ruWord, string engWord);

		void RegisterParentwindow(Window parentWindow);
	}

	public class DialogService : IDialogService {
		private NewTranslationVariantWindow TranslationVariantWindow { get; set; }

		public DialogService(NewTranslationVariantWindow newTranslationVariantWindow) {
			TranslationVariantWindow = newTranslationVariantWindow;
		}

		#region Члены IDialogService

		public NewVariantDialogResult ShowNewVariantDialog(string ruWord, string engWord) {

			var vm = TranslationVariantWindow.InjectedDataContext;
			if (vm == null) {
				return null;
			}
			vm.ClearAll();
			vm.RuName = ruWord;
			vm.EnName = engWord;
			var res = TranslationVariantWindow.ShowDialogEx();
			if (res == true) {
				return new NewVariantDialogResult {
					NameRu = vm.RuName,
					NameEng = vm.EnName
				};
			}
			return null;
		}

		public void RegisterParentwindow(Window parentWindow) {
			TranslationVariantWindow.Owner = parentWindow;
		}

		#endregion
	}
}
