using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.Practices.Unity;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using RegistryExplorer.Model;

namespace RegistryExplorer.ViewModels {
	class MainViewModel : ViewModelBase, IDisposable {
		public readonly IUnityContainer Container = new UnityContainer();
		public CommandManager CommandManager { get; } = new CommandManager();
		public Options Options { get; } = new Options();

		public DelegateCommandBase LaunchWithAdminRightsCommand { get; }
		public DelegateCommandBase ExitCommand { get; }
		public DelegateCommandBase LoadHiveCommand { get; }
		public DelegateCommandBase EditPermissionsCommand { get; }
		public DelegateCommandBase EditNewKeyCommand { get; }
		public DelegateCommandBase BeginRenameCommand { get; }
		public DelegateCommand UndoCommand { get; }
		public DelegateCommand RedoCommand { get; }
		public DelegateCommandBase EndEditingCommand { get; }

		public DelegateCommandBase CopyKeyNameCommand { get; }
		public DelegateCommandBase CopyKeyPathCommand { get; }
		public DelegateCommandBase DeleteCommand { get; }

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

		public bool IsAdmin { get; private set; }

		public string Title => "Registry Explorer" + (IsAdmin ? " (Admin Privileges)" : string.Empty);

		static ImageSource _shieldIcon;
		public static ImageSource ShieldIcon {
			get {
				if(_shieldIcon == null) {
					var sii = new SHSTOCKICONINFO();
					sii.cbSize = (uint)Marshal.SizeOf(typeof(SHSTOCKICONINFO));

					NativeMethods.SHGetStockIconInfo(77, SHGSI.SHGSI_ICON | SHGSI.SHGSI_SMALLICON, ref sii);

					_shieldIcon = Imaging.CreateBitmapSourceFromHIcon(sii.hIcon, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

					NativeMethods.DestroyIcon(sii.hIcon);
				}
				return _shieldIcon;
			}
		}

		public MainViewModel() {
			try {
				NativeMethods.EnablePrivilege("SeBackupPrivilege");
				NativeMethods.EnablePrivilege("SeRestorePrivilege");
				IsAdmin = true;
			}
			catch {
			}

			ActiveView = this;

			DataGridViewModel = new DataGridViewModel(this);

			ExitCommand = new DelegateCommand(() => Application.Current.Shutdown());

			LaunchWithAdminRightsCommand = new DelegateCommand(() => {
				var pi = new ProcessStartInfo(Process.GetCurrentProcess().MainModule.FileName) { Verb = "RunAs" };
				try {
					if(Process.Start(pi) != null) {
						Environment.Exit(0);
					}
				}
				catch(Exception ex) {
					MessageBox.Show(ex.Message, App.Name);
				}
			}, () => !IsAdmin);

			EditNewKeyCommand = new DelegateCommand<RegistryKeyItem>(item => {
				var name = item.GenerateUniqueSubKeyName();
				CommandManager.AddCommand(Commands.CreateKey(new CreateKeyCommandContext {
					Key = item,
					Name = name
				}));
				item.IsExpanded = true;
				var newItem = item.GetSubItem<RegistryKeyItem>(name);
				newItem.IsSelected = true;
				Dispatcher.CurrentDispatcher.InvokeAsync(() => IsEditMode = true, DispatcherPriority.Background);
			}, item => !IsReadOnlyMode && item is RegistryKeyItem)
			.ObservesProperty(() => IsReadOnlyMode);

			EditPermissionsCommand = new DelegateCommand(() => {
				// TODO
			}, () => SelectedItem is RegistryKeyItem)
			.ObservesProperty(() => SelectedItem);

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
					CommandManager.AddCommand(Commands.RenameKey(new RenameKeyCommandContext {
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

			DeleteCommand = new DelegateCommand(() => {
				var item = SelectedItem as RegistryKeyItem;
				Debug.Assert(item != null);

				if(!IsAdmin) {
					if(MessageBox.Show("Running with standard user rights prevents undo for deletion. Delete anyway?", 
						App.Name, MessageBoxButton.OKCancel, MessageBoxImage.Exclamation) == MessageBoxResult.Cancel)
						return;
				}
				var tempFile = Path.GetTempFileName();
				CommandManager.AddCommand(Commands.DeleteKey(new DeleteKeyCommandContext {
					Key = item, TempFile = tempFile
				})); 
			}, () => !IsReadOnlyMode && SelectedItem is RegistryKeyItem && SelectedItem.Path != null).ObservesProperty(() => IsReadOnlyMode);
		}

		public void Dispose() {
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
