using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RegistryExplorer.Extensions {
	static class DialogHelper {
		public static TViewModel ShowDialog<TViewModel, TDialog>() where TDialog : FrameworkElement, new() {
			var dlg = new TDialog();
			var vm = (TViewModel)Activator.CreateInstance(typeof(TViewModel), dlg);
			dlg.DataContext = vm;
			return vm;
		}
	}
}
