using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;
using Prism.Commands;
using Prism.Mvvm;

namespace RegistryExplorer.ViewModels {
	class MainViewModel : BindableBase {
		public DelegateCommandBase ExitCommand { get; private set; }
		public DelegateCommandBase LoadHiveCommand { get; private set; }

		List<RegistryKeyItemSpecial> _roots;

		public IEnumerable<RegistryKeyItemBase> RootItems {
			get {
				if(_roots == null)
					_roots = new List<RegistryKeyItemSpecial> {
						new RegistryKeyItemSpecial(null) {
							Text = "Computer",
							SubItems = {
								new RegistryKeyItem(Registry.ClassesRoot),
								new RegistryKeyItem(Registry.CurrentUser),
								new RegistryKeyItem(Registry.LocalMachine),
								new RegistryKeyItem(Registry.CurrentConfig),
								new RegistryKeyItem(Registry.Users),
							},
							Icon = "/images/workstation2.png",
							IsExpanded = true
						},
						new RegistryKeyItemSpecial(null) {
							Text = "Files",
							Icon = "/images/folder_blue.png"
						},
						new RegistryKeyItemSpecial(null) {
							Text = "Favorites",
							Icon = "/images/favorites.png"
						}
					};
				return _roots;
			}
		}

		private RegistryKeyItemBase _selectedItem;

		public RegistryKeyItemBase SelectedItem {
			get { return _selectedItem; }
			set {
				if(SetProperty(ref _selectedItem, value)) {
					OnPropertyChanged(() => CurrentPath);
				}
			}
		}

		public string CurrentPath {
			get {
				if(SelectedItem == null)
					return string.Empty;

				var item = SelectedItem as RegistryKeyItem;
				if(item != null)
					return item.Root.Name + "\\" + item.Path;
				return SelectedItem.Text;
			}
		}
		public MainViewModel() {
			NativeMethods.EnablePrivilege("SeRestorePrivilege");
			NativeMethods.EnablePrivilege("SeBackupPrivilege");
			ExitCommand = new DelegateCommand(() => Application.Current.Shutdown());

			LoadHiveCommand = new DelegateCommand(() => {
				var dlg = new OpenFileDialog {
					Title = "Select File",
					CheckFileExists = true,
					Filter = "All Files|*.*"
				};
				if(dlg.ShowDialog() == true) {
					var newName = Guid.NewGuid().ToString();
					int error = NativeMethods.RegLoadKey(Registry.LocalMachine.Handle, newName, dlg.FileName);
					if(error > 0) {
						MessageBox.Show("Error opening file: " + error.ToString());
						return;
					}
				}
			});
		}

		private bool _isReadOnlyMode = true;

		public bool IsReadOnlyMode {
			get { return _isReadOnlyMode; }
			set { SetProperty(ref _isReadOnlyMode, value); }
		}

	}
}
