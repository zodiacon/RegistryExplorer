using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using Prism.Commands;

namespace RegistryExplorer.ViewModels {
	class RegistryKeyItem : RegistryKeyItemBase {
		RegistryKey _root;

		public RegistryKeyItem(RegistryKeyItem parent, string text) : base(parent) {
			_root = parent.Root;
			Text = text;
			Path = string.Concat(parent.Path, parent.Path != null ? "\\" : string.Empty, text);
		}

		public RegistryKeyItem(string rootName, string path) : base(null) {
			switch(rootName) {
				case "HKEY_LOCAL_MACHINE": _root = Registry.LocalMachine; break;
				case "HKEY_CURRENT_USER": _root = Registry.CurrentUser; break;
				case "HKEY_USERS": _root = Registry.Users; break;
				case "HKEY_CLASSES_ROOT": _root = Registry.ClassesRoot; break;
				case "HKEY_CURRENT_CONFIG": _root = Registry.CurrentConfig; break;
			}
			Path = path;
			int index = path.LastIndexOf('\\');
			Text = index < 0 ? path : path.Substring(index);
		}

		public RegistryKeyItem(RegistryKeyItemBase parent, RegistryKey root) : base(parent) {
			_root = root;
			Text = root.Name;
			//SubItems = new ObservableCollection<RegistryKeyItemBase>(SubKeys);
		}

		public RegistryKey Root {
			get { return _root; }
		}

		private bool _hiveKey;

		public bool HiveKey {
			get { return _hiveKey; }
			set { SetProperty(ref _hiveKey, value); }
		}

		public override ObservableCollection<RegistryKeyItemBase> SubItems {
			get {
				if(_subItems == null) {
					RegistryKey key = null;
					string[] names;
					if(Path != null) {
						key = TryOpenSubKey(_root, Path);
						if(key == null)
							return null;
						names = TryGetSubKeyNames(key);
						if(names == null)
							return null;
					}
					else
						names = _root.GetSubKeyNames();
					var items = names.OrderBy(n => n).Select(name => new RegistryKeyItem(this, name));
					if(key != null)
						key.Dispose();
					_subItems = new ObservableCollection<RegistryKeyItemBase>(items);
				}
				return _subItems;
			}
		}

		public void DeleteKey(string name) {
			using(var key = _root.OpenSubKey(Path, true))
				key.DeleteSubKeyTree(name);
			SubItems.Remove(SubItems.First(i => i.Text == name));
		}

		private string[] TryGetSubKeyNames(RegistryKey key) {
			try {
				return key.GetSubKeyNames();
			}
			catch(IOException) {
				return null;
			}
		}

		private RegistryKey TryOpenSubKey(RegistryKey key, string name) {
			try {
				return key.OpenSubKey(name);
			}
			catch(SecurityException) {
				return null;
			}
		}

		public RegistryValue[] Values {
			get {
				if(string.IsNullOrEmpty(Path))
					return null;

				using(var key = TryOpenSubKey(_root, Path)) {
					if(key == null)
						return null;

					IEnumerable<string> values = key.GetValueNames().OrderBy(name => name);
					var defaultMissing = !values.Any() || !string.IsNullOrEmpty(values.First());
					var pvalues = values.Select(name => new RegistryValue(this) {
						Name = string.IsNullOrEmpty(name) ? "(Default)" : name,
						Value = key.GetValue(name),
						DataType = key.GetValueKind(name)
					});
					if(defaultMissing)
						pvalues = Enumerable.Repeat(new RegistryValue(this) {
							Name = "(Default)",
							DataType = RegistryValueKind.None
						}, 1).Concat(pvalues);
					return pvalues.ToArray();
				}
			}
		}

		public override void Refresh() {
			_subItems = null;
			OnPropertyChanged(nameof(SubItems));
		}

		public void SetValueName(string oldname, string newname) {
			using(var key = TryOpenSubKey(_root, Path)) {
				if(key == null)
					return;
				key.SetValue(newname, key.GetValue(oldname));
				key.DeleteValue(oldname);
			}
		}

		public RegistryKeyItem CreateNewKey(string name) {

			using(var key = _root.CreateSubKey(string.Format("{0}\\{1}", Path, name))) {
				var newitem = new RegistryKeyItem(this, name);
				SubItems.Add(newitem);
				return newitem;
			}

		}

		public string GenerateUniqueSubKeyName() {
			int i = 1;

			for(;;) {
				string name = string.Format("NewKey{0}", i);
				if(!SubItems.Any(si => si.Text.Equals(name, StringComparison.InvariantCultureIgnoreCase)))
					return name;
				++i;
			}
		}


		public override bool Equals(object obj) {
			var other = obj as RegistryKeyItem;
			if(other == null) return false;

			return _root.Name == other.Root.Name && Path == other.Path;
		}

		public override int GetHashCode() {
			return _root.Name.GetHashCode() ^ (Path != null ? Path.GetHashCode() : 0);
		}

		public void RenameKey(string oldname, string newname) {
			using(var key = _root.OpenSubKey((Parent as RegistryKeyItem).Path, true)) {
				int error = NativeMethods.RegRenameKey(key.Handle, oldname, newname);
				if(error != 0)
					throw new Win32Exception(error);
			}
		}

		//public DelegateCommandBase BeginRenameCommand { get; } = App.MainViewModel.BeginRenameCommand;
	}
}
