using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using RegistryExplorer.ViewModels;
using MahApps.Metro;
using Prism.Mvvm;

namespace RegistryExplorer {
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application {
		internal static readonly string Name = "Registry Explorer";

		internal static readonly MainViewModel MainViewModel = new MainViewModel();

		public App() {
			ViewModelLocationProvider.SetDefaultViewTypeToViewModelTypeResolver(view => Type.GetType(view.FullName.Replace("View", "ViewModel")));
		}
		protected override void OnStartup(StartupEventArgs e) {
			base.OnStartup(e);

			var win = new MainWindow();
			win.DataContext = MainViewModel;
			win.Show();
		}

		protected override void OnExit(ExitEventArgs e) {
			base.OnExit(e);

			NativeMethods.Dispose();
		}
	}
}
