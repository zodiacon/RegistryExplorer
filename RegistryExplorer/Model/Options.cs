using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;

namespace RegistryExplorer.Model {
	class Options : BindableBase {
		private bool _alwaysOnTop;

		public bool AlwaysOnTop {
			get { return _alwaysOnTop; }
			set {
				if(SetProperty(ref _alwaysOnTop, value)) {
					App.Current.MainWindow.Topmost = value;
				}
			}
		}


	}
}
