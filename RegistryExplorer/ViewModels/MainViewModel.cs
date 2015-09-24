using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;
using Prism.Commands;
using Prism.Mvvm;

namespace RegistryExplorer.ViewModels {
	class MainViewModel : BindableBase {
		public DelegateCommandBase ExitCommand { get; private set; }
		public DelegateCommandBase LoadHiveCommand { get; private set; }
		public DelegateCommandBase EditPermissionsCommand { get; private set; }
		public DelegateCommandBase EditNewKeyCommand { get; private set; }

		public DataGridViewModel DataGridViewModel { get; private set; }

		List<RegistryKeyItemBase> _roots;

		public IList<RegistryKeyItemBase> RootItems {
			get {
				if(_roots == null) {
					var computer = new RegistryKeyItemSpecial(null) {
						Text = "Computer",
						Icon = "/images/workstation2.png",
						IsExpanded = true
					};
					computer.SubItems.Add(new RegistryKeyItem(computer, Registry.ClassesRoot));
					computer.SubItems.Add(new RegistryKeyItem(computer, Registry.CurrentUser));
					computer.SubItems.Add(new RegistryKeyItem(computer, Registry.LocalMachine));
					computer.SubItems.Add(new RegistryKeyItem(computer, Registry.CurrentConfig));
					computer.SubItems.Add(new RegistryKeyItem(computer, Registry.Users));

					_roots = new List<RegistryKeyItemBase> {
						computer,
						new RegistryKeyItemSpecial(null) {
							Text = "Files",
							Icon = "/images/folder_blue.png"
						},
						new RegistryKeyItemSpecial(null) {
							Text = "Favorites",
							Icon = "/images/favorites.png"
						}
					};

				}
				return _roots;
			}
		}

		private RegistryKeyItemBase _selectedItem;

		public RegistryKeyItemBase SelectedItem {
			get { return _selectedItem; }
			set {
				if(SetProperty(ref _selectedItem, value)) {
					OnPropertyChanged(nameof(CurrentPath));
					EditPermissionsCommand.RaiseCanExecuteChanged();
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
			try {
				NativeMethods.EnablePrivilege("SeRestorePrivilege");
				NativeMethods.EnablePrivilege("SeBackupPrivilege");
			}
			catch {
			}

			DataGridViewModel = new DataGridViewModel(this);

			ExitCommand = new DelegateCommand(() => Application.Current.Shutdown());

			EditNewKeyCommand = new DelegateCommand(() => {
				var item = SelectedItem as RegistryKeyItem;
				Debug.Assert(item != null);
				var newKey = item.CreateNewKey();
				item.SubItems.Add(newKey);
				newKey.IsSelected = true;
				newKey.IsExpanded = true;
				newKey.IsEditing = true;
			}, () => !IsReadOnlyMode && SelectedItem != null && SelectedItem is RegistryKeyItem);

			EditPermissionsCommand = new DelegateCommand(() => {
			}, () => !IsReadOnlyMode && SelectedItem != null && SelectedItem is RegistryKeyItem);

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
			set {
				if(SetProperty(ref _isReadOnlyMode, value)) {
					EditPermissionsCommand.RaiseCanExecuteChanged();
					EditNewKeyCommand.RaiseCanExecuteChanged();
				}
			}
		}

	}
}
