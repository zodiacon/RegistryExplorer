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
using System.Windows.Data;
using MahApps.Metro.Controls.Dialogs;
using RegistryExplorer.Views.Dialogs;
using RegistryExplorer.ViewModels.Dialogs;
using RegistryExplorer.Extensions;
using System.Windows.Input;
using System.IO.IsolatedStorage;
using RegistryExplorer.Serialization;

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
		public DelegateCommandBase ExportCommand { get; }
		public DelegateCommandBase CopyKeyNameCommand { get; }
		public DelegateCommandBase CopyKeyPathCommand { get; }
		public DelegateCommandBase DeleteCommand { get; }
		public DelegateCommandBase CreateNewValueCommand { get; }
		public DelegateCommandBase AddToFavoritesCommand { get; }
		public DelegateCommandBase RemoveFromFavoritesCommand { get; }
		public DelegateCommandBase RefreshCommand { get; }

		List<RegistryKeyItemBase> _roots;

		public IList<RegistryKeyItemBase> RootItems => _roots;

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

		public IDialogCoordinator DialogCoordinator { get; }

		public MainViewModel(IDialogCoordinator _dialogCoordinator) {
			DialogCoordinator = _dialogCoordinator;

			try {
				NativeMethods.EnablePrivilege("SeBackupPrivilege");
				NativeMethods.EnablePrivilege("SeRestorePrivilege");
				IsAdmin = true;
			}
			catch {
			}

			ActiveView = this;

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
				//new RegistryKeyItemSpecial(null) {
				//	Text = "Files",
				//	Icon = "/images/folder_blue.png"
				//},
				new RegistryKeyItemSpecial(null) {
					Text = "Favorites",
					Icon = "/images/favorites.png",
					IsExpanded = true
				}
			};

			LoadFavorites();

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

			RefreshCommand = new DelegateCommand(() => SelectedItem.Refresh(), () => SelectedItem != null)
				.ObservesProperty(() => SelectedItem);

			EndEditingCommand = new DelegateCommand<string>(async name => {
				try {
					var item = SelectedItem as RegistryKeyItem;
					Debug.Assert(item != null);
					if(name == null || name.Equals(item.Text, StringComparison.InvariantCultureIgnoreCase))
						return;

					if(item.Parent.SubItems.Any(i => name.Equals(i.Text, StringComparison.InvariantCultureIgnoreCase))) {
						await DialogCoordinator.ShowMessageAsync(this, App.Name, string.Format("Key name '{0}' already exists", name));
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

			LoadHiveCommand = new DelegateCommand(async () => {
				var vm = DialogHelper.ShowDialog<LoadHiveViewModel, LoadHiveView>();
				if(vm.ShowDialog() == true) {
					int error = NativeMethods.RegLoadKey(vm.Hive == "HKLM" ? Registry.LocalMachine.Handle : Registry.Users.Handle, vm.Name, vm.FileName);
					if(error != 0) {
						await DialogCoordinator.ShowMessageAsync(this, App.Name, string.Format("Error opening file: {0}", error.ToString()));
						return;
					}

					var item = _roots[0].SubItems[vm.Hive == "HKLM" ? 2 : 4];
					item.Refresh();
					((RegistryKeyItem)item.SubItems.First(i => i.Text == vm.Name)).HiveKey = true;
				}
			}, () => IsAdmin && !IsReadOnlyMode)
			.ObservesProperty(() => IsReadOnlyMode);

			DeleteCommand = new DelegateCommand(() => {
				var item = SelectedItem as RegistryKeyItem;
				Debug.Assert(item != null);

				if(!IsAdmin) {
					if(MessageBox.Show("Running with standard user rights prevents undo for deletion. Delete anyway?",
						App.Name, MessageBoxButton.OKCancel, MessageBoxImage.Exclamation) == MessageBoxResult.Cancel)
						return;
					CommandManager.Clear();
				}
				var tempFile = Path.GetTempFileName();
				CommandManager.AddCommand(Commands.DeleteKey(new DeleteKeyCommandContext {
					Key = item,
					TempFile = tempFile
				}));
				CommandManager.UpdateChanges();
				UndoCommand.RaiseCanExecuteChanged();
				RedoCommand.RaiseCanExecuteChanged();
			}, () => !IsReadOnlyMode && SelectedItem is RegistryKeyItem && SelectedItem.Path != null).ObservesProperty(() => IsReadOnlyMode);

			ExportCommand = new DelegateCommand(() => {
				var dlg = new SaveFileDialog {
					Title = "Select output file",
					Filter = "Registry data files|*.dat",
					OverwritePrompt = true
				};
				if(dlg.ShowDialog() == true) {
					var item = SelectedItem as RegistryKeyItem;
					Debug.Assert(item != null);
					using(var key = item.Root.OpenSubKey(item.Path)) {
						File.Delete(dlg.FileName);
						int error = NativeMethods.RegSaveKeyEx(key.Handle, dlg.FileName, IntPtr.Zero, 2);
						Debug.Assert(error == 0);
					}
				}
			}, () => IsAdmin && SelectedItem is RegistryKeyItem)
			.ObservesProperty(() => SelectedItem);

			CreateNewValueCommand = new DelegateCommand<ValueViewModel>(vm => {

			}, vm => !IsReadOnlyMode)
			.ObservesProperty(() => IsReadOnlyMode);

			AddToFavoritesCommand = new DelegateCommand(() => {
				var item = SelectedItem as RegistryKeyItem;
				Debug.Assert(item != null);

				_roots[1].SubItems.Add(new RegistryKeyItem(item.Parent as RegistryKeyItem, item.Text));
				SaveFavorites();
			}, () => SelectedItem is RegistryKeyItem && !string.IsNullOrEmpty(SelectedItem.Path))
			.ObservesProperty(() => SelectedItem);

			RemoveFromFavoritesCommand = new DelegateCommand(() => {
				_roots[1].SubItems.Remove(SelectedItem);
				SaveFavorites();
			}, () => SelectedItem != null && SelectedItem.Flags.HasFlag(RegistryKeyFlags.Favorite))
			.ObservesProperty(() => SelectedItem);
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
			set { SetProperty(ref _isEditMode, value); }
		}

		public void SaveFavorites() {
			using(var stm = IsolatedStorageFile.GetUserStoreForAssembly().OpenFile("favorites.dat", FileMode.OpenOrCreate, FileAccess.ReadWrite)) {
				Serializer.SaveKeys(stm, _roots[1].SubItems);
			}
		}

		public void LoadFavorites() {
			try {
				using(var stm = IsolatedStorageFile.GetUserStoreForAssembly().OpenFile("favorites.dat", FileMode.Open)) {
					var keys = Serializer.LoadKeys(stm);
					foreach(var key in keys) {
						key.Flags |= RegistryKeyFlags.Favorite;
						_roots[1].SubItems.Add(key);
					}
				}
			}
			catch { }
		}

		public bool IsInFavorites => SelectedItem.Flags.HasFlag(RegistryKeyFlags.Favorite);
		public bool IsNotInFavorites => !SelectedItem.Flags.HasFlag(RegistryKeyFlags.Favorite);
	}
}
