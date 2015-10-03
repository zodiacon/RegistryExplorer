using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Practices.Unity;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using RegistryExplorer.Model;

namespace RegistryExplorer.ViewModels {
	class MainViewModel : ViewModelBase {
		public readonly IUnityContainer Container = new UnityContainer();
		public CommandManager CommandManager { get; } = new CommandManager();
		public Options Options { get; } = new Options();

		public DelegateCommandBase ExitCommand { get; }
		public DelegateCommandBase LoadHiveCommand { get; }
		public DelegateCommandBase EditPermissionsCommand { get; }
		public DelegateCommandBase EditNewKeyCommand { get; }
		public DelegateCommandBase BeginRenameCommand { get; }
		public DelegateCommand UndoCommand { get; }
		public DelegateCommand RedoCommand { get; }
		public DelegateCommandBase EndEditingCommand { get; }

		public DelegateCommandBase CopyKeyNameCommand { get; }
		public DelegateCommand CopyKeyPathCommand { get; }

		public DataGridViewModel DataGridViewModel { get; }

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

			ActiveView = this;

			DataGridViewModel = new DataGridViewModel(this);

			ExitCommand = new DelegateCommand(() => Application.Current.Shutdown());

			EditNewKeyCommand = new DelegateCommand<RegistryKeyItem>(item => {
				//var item = SelectedItem as RegistryKeyItem;
				Debug.Assert(item != null);

				var name = item.GenerateUniqueSubKeyName();
				CommandManager.AddCommand(Commands.CreateKeyCommand(new CreateKeyCommandContext {
					Key = item,
					Name = name
				}));
				item.IsExpanded = true;
				var newItem = item.GetSubItem<RegistryKeyItem>(name);
				newItem.IsSelected = true;
				IsEditMode = true;
			}, item => !IsReadOnlyMode && item is RegistryKeyItem)
			.ObservesProperty(() => IsReadOnlyMode).ObservesProperty(() => SelectedItem);

			EditPermissionsCommand = new DelegateCommand(() => {
				// TODO
			}, () => !IsReadOnlyMode && SelectedItem != null && SelectedItem is RegistryKeyItem)
			.ObservesProperty(() => IsReadOnlyMode).ObservesProperty(() => SelectedItem);

			CopyKeyNameCommand = new DelegateCommand<RegistryKeyItemBase>(_ => Clipboard.SetText(SelectedItem.Text),
				_ => SelectedItem != null).ObservesProperty(() => SelectedItem);

			CopyKeyPathCommand = new DelegateCommand(() => Clipboard.SetText(((RegistryKeyItem)SelectedItem).Path ?? SelectedItem.Text),
				() => SelectedItem is RegistryKeyItem)
				.ObservesProperty(() => SelectedItem);

			EndEditingCommand = new DelegateCommand<string>(name => {
				try {

					var item = SelectedItem as RegistryKeyItem;
					Debug.Assert(item != null);
					if(name == null || name.Equals(item.Text, StringComparison.InvariantCultureIgnoreCase))
						return;

					if(item.Parent.SubItems.Any(i => name.Equals(i.Text, StringComparison.InvariantCultureIgnoreCase))) {
						MessageBox.Show(string.Format("Key name '{0}' already exists", name), App.Name);
						return;
					}
					CommandManager.AddCommand(Commands.RenameKeyCommand(new RenameKeyContext {
						Key = item,
						OldName = item.Text,
						NewName = name
					}));
				}
				catch(Exception ex) {
					MessageBox.Show(ex.Message, App.Name);
				}
				finally {
					IsEditMode = false;
					CommandManager.UpdateChanges();
					UndoCommand.RaiseCanExecuteChanged();
					RedoCommand.RaiseCanExecuteChanged();
				}
			}, _ => IsEditMode).ObservesCanExecute(_ => IsEditMode);

			BeginRenameCommand = new DelegateCommand(() => IsEditMode = true,
				() => !IsReadOnlyMode && SelectedItem is RegistryKeyItem && !string.IsNullOrEmpty(((RegistryKeyItem)SelectedItem).Path))
				.ObservesProperty(() => SelectedItem).ObservesProperty(() => IsReadOnlyMode);

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

	}
}
