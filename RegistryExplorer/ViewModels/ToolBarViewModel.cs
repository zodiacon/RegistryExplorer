using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RegistryExplorer.ViewModels {

	class ToolBarViewModel : BindableBase {
		public MainViewModel MainViewModel { get; }

		public ToolBarViewModel() {
			MainViewModel = App.MainViewModel;
		}

	}
}
