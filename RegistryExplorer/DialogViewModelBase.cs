using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RegistryExplorer {
	abstract class DialogViewModelBase : ViewModelBase {
		Window _dialog;
		bool? _result;

		public DialogViewModelBase(DependencyObject dialog) {
			if(dialog != null)
				_dialog = Window.GetWindow(dialog);
		}

		public DialogViewModelBase(bool? result) {
			_result = result;
		}

		protected void Close(bool? result = true) {
			if(_dialog != null) {
				_dialog.DialogResult = result;
				_dialog.Close();
			}
		}

		public bool? ShowDialog() {
			return _dialog != null ? _dialog.ShowDialog() : _result;
		}
	}
}
