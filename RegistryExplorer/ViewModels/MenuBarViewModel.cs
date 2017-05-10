using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Commands;
using Prism.Mvvm;
using MahApps.Metro;
using System.Windows;
using Zodiacon.WPF;

namespace RegistryExplorer.ViewModels {
	class MenuBarViewModel : BindableBase, IDisposable {
		public MainViewModel MainViewModel { get; }
		PropertyFollower<MainViewModel, MenuBarViewModel> _follower;

		public MenuBarViewModel(MainViewModel vm) {
			MainViewModel = vm;
			_follower = PropertyFollower<MainViewModel, MenuBarViewModel>.Create(MainViewModel, this, nameof(ViewModels.MainViewModel.IsReadOnlyMode));

			ChangeAccentCommand = new DelegateCommand<AccentViewModel>(accent => {
				ThemeManager.ChangeAppStyle(Application.Current, accent.Accent, ThemeManager.DetectAppStyle().Item1);
			});

		}

		public DelegateCommandBase ExitCommand {
			get { return MainViewModel.ExitCommand; }
		}

		public void Dispose() {
			_follower.Dispose();
		}

		public DelegateCommandBase ChangeAccentCommand { get; }

		public IEnumerable<AccentViewModel> Accents => ThemeManager.Accents.Select(accent => new AccentViewModel(accent)).ToArray();

	}
}
