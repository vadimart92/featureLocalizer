using System;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Windows;
using System.Windows.Input;
using Microsoft.Practices.Unity;
using TranslateApp.ViewModels;

namespace TranslateApp.Views {
	/// <summary>
	/// Логика взаимодействия для NewTranslationVariantWindow.xaml
	/// </summary>
	public partial class NewTranslationVariantWindow {
		public NewTranslationVariantWindow() {
			InitializeComponent();
			IsVisibleChanged += (s, e) => {
				if (!IsVisible) return;
				var dc = DataContext;
				DataContext = null;
				DataContext = dc;
				FocusManager.SetFocusedElement(this, EngTextBox);
			};
			DialogResultEx = null;
		}

		public bool? DialogResultEx { get; set; }

		private void Cancel_Button_Click(object sender, RoutedEventArgs e) {
			DialogResultEx = false;
			Hide();
		}

		private void OkButton_OnClick(object sender, RoutedEventArgs e) {
			DialogResultEx = true;
			Hide();
		}

		[Dependency]
		public NewTranslationVariantWindowVM InjectedDataContext {
			get {
				return DataContext as NewTranslationVariantWindowVM;
			}
			set {
				Contract.Requires(value != null, "value!= null");
				DataContext = value;
			}
		}

		protected override void OnClosing(CancelEventArgs e) {
			e.Cancel = true;
			Hide();
		}

		public bool? ShowDialogEx() {
			DialogResultEx = null;
			ShowDialog();
			return DialogResultEx;
		}

	}
}
