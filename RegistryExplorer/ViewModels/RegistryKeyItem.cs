using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace RegistryExplorer.ViewModels {
	class RegistryKeyItem : RegistryKeyItemBase {
		RegistryKey _root;
		public string Path { get; private set; }

		public RegistryKeyItem(RegistryKeyItem parent, string text) : base(parent) {
			_root = parent.Root;
			Text = text;
			Path = string.Concat(parent.Path, parent.Path != null ? "\\" : string.Empty, text);
		}

		public RegistryKeyItem(RegistryKey root) : base(null) {
			_root = root;
			Text = root.Name;
			//SubItems = new ObservableCollection<RegistryKeyItemBase>(SubKeys);
		}

		public RegistryKey Root {
			get { return _root; }
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

		private string[] TryGetSubKeyNames(RegistryKey key) {
			try {
				return key.GetSubKeyNames();
			}
			catch(IOException ) {
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
				if(Path == null)
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
							Name ="(Default)",
							DataType = RegistryValueKind.None
						}, 1).Concat(pvalues);
					return pvalues.ToArray();
				}
			}
		}

		public void SetValueName(string oldname, string newname) {
			using(var key = TryOpenSubKey(_root, Path)) {
				if(key == null)
					return;
				key.SetValue(newname, key.GetValue(oldname));
				key.DeleteValue(oldname);
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
	}
}
