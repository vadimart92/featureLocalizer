using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Windows;
using System.Windows.Input;
using ICSharpCode.AvalonEdit.Highlighting;
using Microsoft.Practices.Unity;
using TranslateApp.ViewModels;

namespace TranslateApp.Views {
	/// <summary>
	/// Логика взаимодействия для MainWindow.xaml
	/// </summary>
	public partial class MainWindow {
		public MainWindow() {
			InitializeComponent();
		}

		[Dependency]
		public MainWindowViewModel InjectedDataContext {
			set {
				Contract.Requires(value!= null, "value!= null");
				DataContext = value;
				value.SourceTextEditor = SourceTextEditor;
				value.TranslationTextEditor = TranslationTextEditor;
			}
		}
	}
}
