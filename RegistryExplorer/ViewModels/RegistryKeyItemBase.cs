using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;

namespace RegistryExplorer.ViewModels {
	abstract class RegistryKeyItemBase : BindableBase {
		private string _name;

		public string Text {
			get { return _name; }
			set {SetProperty(ref _name, value); }
		}
		
		protected RegistryKeyItemBase() {
		}
	}
}
