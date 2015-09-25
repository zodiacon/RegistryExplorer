using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;

namespace RegistryExplorer {
	[Flags]
	enum RegistryKeyPermissions {
		KEY_ALL_ACCESS = 0xF003F,
		KEY_READ = 0x20019,
		KEY_CREATE_SUB_KEY = 0x0004,
		KEY_ENUMERATE_SUB_KEYS = 0x0008,
		KEY_WRITE = 0x20006,
		KEY_SET_VALUE = 0x0002

	}

	static class NativeMethods {
		[DllImport("advapi32")]
		public static extern int RegLoadKey(SafeRegistryHandle hKey, string subKey, string file);

		[DllImport("advapi32")]
		public static extern int RegLoadAppKey(string file, out SafeRegistryHandle hKey, RegistryKeyPermissions samDesired, uint options, uint reserved);

		[DllImport("advapi32")]
		public static extern int RegRenameKey(SafeRegistryHandle hKey, [MarshalAs(UnmanagedType.LPWStr)] string oldname, [MarshalAs(UnmanagedType.LPWStr)] string newname);

		static Dictionary<string, object> _privileges = new Dictionary<string, object>();

		public static void EnablePrivilege(string name) {
			object privilege;
			if(!_privileges.TryGetValue(name, out privilege)) {
				Type privilegeType = Type.GetType("System.Security.AccessControl.Privilege");
				privilege = Activator.CreateInstance(privilegeType, name);
				_privileges.Add(name, privilege);
			}
			privilege.GetType().GetMethod("Enable").Invoke(privilege,null);
		}

	
		public static void DisablePrivilege(string name) {
			object privilege = _privileges[name];
			privilege.GetType().GetMethod("Revert").Invoke(privilege, null);
		}

		[DllImport("user32")]
		public static extern bool MessageBeep(uint type);

		internal static void Dispose() {
			foreach(var p in _privileges.Keys)
				DisablePrivilege(p);

		}
	}
}
