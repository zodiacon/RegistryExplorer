using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace RegistryExplorer.ViewModels {
	class RegistryKeyItem {
		RegistryKey _key, _parent;
		string _name;

		public RegistryKeyItem(RegistryKey parent, string name) {
			_parent = parent;
			_name = name;
		}

		public string Text {
			get { return _key == null ? _name : _key.Name; }
		}

		public string[] SubKeyNames {
			get {
				if(_key == null)
					_key = _parent.OpenSubKey(_name);
				return _key.GetSubKeyNames();
			}
		}
	}
}
