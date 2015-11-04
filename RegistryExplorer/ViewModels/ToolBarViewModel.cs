using Microsoft.Win32;
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
			OnPropertyChanged(() => MainViewModel);
		}

		public IEnumerable<ValueViewModel> NewValueTypes {
			get {
				yield return new ValueViewModel { Text = "DWORD", Type = RegistryValueKind.DWord };
				yield return new ValueViewModel { Text = "QWORD", Type = RegistryValueKind.QWord };
				yield return new ValueViewModel { Text = "String", Type = RegistryValueKind.String};
				yield return new ValueViewModel { Text = "Expand String", Type = RegistryValueKind.ExpandString };
				yield return new ValueViewModel { Text = "Multi String", Type = RegistryValueKind.MultiString};
				yield return new ValueViewModel { Text = "Binary", Type = RegistryValueKind.Binary };
			}
		}
	}
}
