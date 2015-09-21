using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using Prism.Mvvm;

namespace RegistryExplorer.ViewModels {
	class RegistryValue : BindableBase {
		Dictionary<RegistryValueKind, string> _types = new Dictionary<RegistryValueKind, string> {
			{ RegistryValueKind.Binary, "REG_BINARY" },
			{ RegistryValueKind.DWord, "REG_DWORD" },
			{ RegistryValueKind.ExpandString, "REG_EXPAND_SZ" },
			{ RegistryValueKind.MultiString, "REG_MULTI_SZ" },
			{ RegistryValueKind.None, "REG_NONE" },
			{ RegistryValueKind.QWord, "REG_QWORD" },
			{ RegistryValueKind.String, "REG_SZ" },
			{ RegistryValueKind.Unknown, "(Unknown)" }
		};

		RegistryKeyItem _key;
		public RegistryValue(RegistryKeyItem key) {
			Debug.Assert(key != null);
			_key = key;
		}

		public RegistryValueKind DataType { get; set; }

		private object _value;

		public object Value {
			get { return _value; }
			set {
				if(SetProperty(ref _value, value)) {
					OnPropertyChanged(() => ValueAsString);
				}
			}
		}
		

		private string _name;

		public string Name {
			get { return _name; }
			set { 
				var oldname = _name;
				if(SetProperty(ref _name, value) && oldname != null)
					_key.SetValueName(oldname, _name);
			}
		}

		public string DataTypeAsString {
			get {
				return _types[DataType];
			}
		}

		public string ValueAsString {
			get {
				if(Value == null)
					return "(value not set)";

				switch(DataType) {
					case RegistryValueKind.MultiString:
						return FormatMultiString((string[])Value);
					case RegistryValueKind.Binary:
						return FormatBinary(((byte[])Value));
					case RegistryValueKind.String:
					case RegistryValueKind.ExpandString:
						return FormatString((string)Value);
				}
				return string.Format("{0} (0x{1})", Value.ToString(), ((IFormattable)Value).ToString("X", null));
			}
			
		}

		private string FormatString(string value) {
			if(value.Length > 64)
				value = value.Substring(0, 64) + " ...";
			return value + string.Format(" ({0} characters)", value.Length);
		}

		private string FormatMultiString(string[] array) {
			string result = string.Join(" ", array);
			if(result.Length > 64)
				result = result.Substring(0, 64) + "...";
			result += string.Format(" ({0} strings)", array.Length);
			return result;
		}
		
		private string FormatBinary(byte[] data) {
			return string.Join(" ", data.Take(32).Select(n => n.ToString("X2"))) + (data.Length > 32 ? " ..." : string.Empty) 
				+ string.Format(" ({0} bytes)", data.Length);
		}
	}
}