using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Commands;
using Prism.Mvvm;

namespace RegistryExplorer.ViewModels {
	class MenuBarViewModel : BindableBase, IDisposable {
		MainViewModel _mainViewModel;
		PropertyFollower<MainViewModel, MenuBarViewModel> _follower;

		public MenuBarViewModel(MainViewModel vm) {
			_mainViewModel = vm;
			_follower = PropertyFollowerFactory.Create(vm, this, nameof(MainViewModel.IsReadOnlyMode));
		}

		public DelegateCommandBase ExitCommand {
			get { return _mainViewModel.ExitCommand; }
		}

		public void Dispose() {
			_follower.Dispose();
		}
	}
}
