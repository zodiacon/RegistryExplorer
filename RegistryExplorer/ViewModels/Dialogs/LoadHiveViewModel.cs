using Microsoft.Win32;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RegistryExplorer.ViewModels.Dialogs {
	class LoadHiveViewModel : DialogViewModelBase {
		public DelegateCommandBase BrowseCommand { get; }
		public DelegateCommandBase LoadCommand { get; }

		public LoadHiveViewModel(DependencyObject dialog) : base(dialog) {
			BrowseCommand = new DelegateCommand(() => {
				var dlg = new OpenFileDialog {
					Title = "Select File",
					CheckFileExists = true,
					Filter = "Registry Hive Files|*.dat;."
				};
				if(dlg.ShowDialog() == true) {
					FileName = dlg.FileName;
				}

			});

			LoadCommand = new DelegateCommand(() => Close(true), () => !string.IsNullOrWhiteSpace(Name) && !string.IsNullOrWhiteSpace(FileName))
				.ObservesProperty(() => Name).ObservesProperty(() => FileName);
		}

		private string _fileName;

		public string FileName {
			get { return _fileName; }
			set { SetProperty(ref _fileName, value); }
		}

		public IEnumerable<string> LoadHives {
			get {
				yield return "HKEY_LOCAL_MACHINE";
				yield return "HKEY_USERS";
			}
		}

		private int _selectedHive;

		public int SelectedHive {
			get { return _selectedHive; }
			set { SetProperty(ref _selectedHive, value); }
		}

		private string _name;

		public string Name {
			get { return _name; }
			set { SetProperty(ref _name, value); }
		}

		public string Hive => SelectedHive == 0 ? "HKLM" : "HKU";

	}
}
