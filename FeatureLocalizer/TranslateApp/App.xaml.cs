using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Catel.IoC;
using Catel.Services;
using Microsoft.Practices.Unity;
using TranslateApp.Common;
using TranslateApp.ViewModels;
using TranslateApp.Views;

namespace TranslateApp {
	/// <summary>
	/// Логика взаимодействия для App.xaml
	/// </summary>
	public partial class App : Application {
		protected override void OnStartup(StartupEventArgs e) {
			base.OnStartup(e);
			IUnityContainer container = new UnityContainer();
			RegisterDependencies(container);
			var window = container.Resolve<MainWindow>();
			window.Show();
			var dialogService = container.Resolve<DialogService>();
			dialogService.RegisterParentwindow(window);
		}

		private void RegisterDependencies(IUnityContainer container) {
			container.RegisterType<IDialogService, DialogService>(new ContainerControlledLifetimeManager());
		}
	}
}
