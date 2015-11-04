using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RegistryExplorer.ViewModels {
	class ValueViewModel {
		public string Text { get; set; }
		public RegistryValueKind Type { get; set; }
		public ICommand Command { get; }

		public ValueViewModel() {
			Command = App.MainViewModel.CreateNewValueCommand;
		}
	}
}
