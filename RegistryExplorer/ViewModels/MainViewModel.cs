using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using Microsoft.Practices.Unity;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using RegistryExplorer.Model;

namespace RegistryExplorer.ViewModels {
	class MainViewModel : BindableBase {
		public readonly IUnityContainer Container = new UnityContainer();
		public CommandManager CommandManager { get; } = new CommandManager();
		public Options Options { get; } = new Options();

		public DelegateCommandBase ExitCommand { get; private set; }
		public DelegateCommandBase LoadHiveCommand { get; private set; }
		public DelegateCommandBase EditPermissionsCommand { get; private set; }
		public DelegateCommandBase EditNewKeyCommand { get; private set; }
		public DelegateCommandBase BeginRenameCommand { get; }
		public DelegateCommand UndoCommand { get; }
		public DelegateCommand RedoCommand { get; }
		public DelegateCommandBase EndEditingCommand { get; }

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

		public string UndoDescription { get { return "Cactus"; } }

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
				item.IsExpanded = true;
				newKey.IsSelected = true;
				IsEditMode = true;
				IsCreatingNewKey = true;
			}, () => !IsReadOnlyMode && SelectedItem != null && SelectedItem is RegistryKeyItem);

			EditPermissionsCommand = new DelegateCommand(() => {
			}, () => !IsReadOnlyMode && SelectedItem != null && SelectedItem is RegistryKeyItem);

			EndEditingCommand = new DelegateCommand<string>(name => {
				try {
					var item = SelectedItem as RegistryKeyItem;
					Debug.Assert(item != null);
					if(item.Text == name) {
						if(IsCreatingNewKey)
							item.Parent.SubItems.Remove(item);
						return;
					}
					if(item.Parent.SubItems.Any(i => name.Equals(i.Text, StringComparison.InvariantCultureIgnoreCase))) {
						MessageBox.Show(string.Format("Key name '{0}' already exists", name), App.Name);
						return;
					}
					if(IsCreatingNewKey) {
						CommandManager.AddCommand(Commands.CreateKeyCommand(new CreateKeyCommandContext {
						}));
					}
					else {
						CommandManager.AddCommand(Commands.RenameKeyCommand(new RenameKeyContext {
							Key = item,
							OldName = item.Text,
							NewName = name
						}));
					}
				}
				catch(Exception ex) {
					MessageBox.Show(ex.Message, App.Name);
				}
				finally {
					IsEditMode = false;
					IsCreatingNewKey = false;
				}
			});

			BeginRenameCommand = new DelegateCommand<RegistryKeyItem>(item => IsEditMode = true, item => !IsReadOnlyMode && item is RegistryKeyItem && !string.IsNullOrEmpty(item.Path))
				.ObservesProperty(() => IsReadOnlyMode);

			UndoCommand = new DelegateCommand(() => {
				CommandManager.Undo();
				RedoCommand.RaiseCanExecuteChanged();
				UndoCommand.RaiseCanExecuteChanged();
			}, () => !IsReadOnlyMode && CommandManager.CanUndo).ObservesProperty(() => IsReadOnlyMode);

			RedoCommand = new DelegateCommand(() => {
				CommandManager.Redo();
				UndoCommand.RaiseCanExecuteChanged();
				RedoCommand.RaiseCanExecuteChanged();
			}, () => !IsReadOnlyMode && CommandManager.CanRedo).ObservesProperty(() => IsReadOnlyMode);

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
					CommandManager.UpdateChanges();
				}
			}
		}

		private bool _isEditMode;

		public bool IsEditMode {
			get { return _isEditMode; }
			set {
				if(SetProperty(ref _isEditMode, value)) {
				}
			}
		}

		private bool _isCreatingNewKey;

		public bool IsCreatingNewKey {
			get { return _isCreatingNewKey; }
			set { SetProperty(ref _isCreatingNewKey, value); }
		}

	}
}
