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

	[Flags]
	public enum SHGSI : uint {
		SHGSI_ICONLOCATION = 0,
		SHGSI_ICON = 0x000000100,
		SHGSI_SYSICONINDEX = 0x000004000,
		SHGSI_LINKOVERLAY = 0x000008000,
		SHGSI_SELECTED = 0x000010000,
		SHGSI_LARGEICON = 0x000000000,
		SHGSI_SMALLICON = 0x000000001,
		SHGSI_SHELLICONSIZE = 0x000000004
	}

	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct SHSTOCKICONINFO {
		public uint cbSize;
		public IntPtr hIcon;
		public int iSysIconIndex;
		public int iIcon;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
		public string szPath;
	}

	static class NativeMethods {
		[DllImport("Shell32", SetLastError = false)]
		public static extern Int32 SHGetStockIconInfo(int siid, SHGSI uFlags, ref SHSTOCKICONINFO psii);

		[DllImport("user32")]
		public static extern bool DestroyIcon(IntPtr hIcon);

		[DllImport("advapi32", CharSet = CharSet.Unicode, EntryPoint = "RegLoadKeyW", ExactSpelling = true)]
		public static extern int RegLoadKey(SafeRegistryHandle hKey, [MarshalAs(UnmanagedType.LPWStr)] string subKey, [MarshalAs(UnmanagedType.LPWStr)] string file);

		[DllImport("advapi32")]
		public static extern int RegLoadAppKey(string file, out SafeRegistryHandle hKey, RegistryKeyPermissions samDesired, uint options, uint reserved);

		[DllImport("advapi32")]
		public static extern int RegRenameKey(SafeRegistryHandle hKey, [MarshalAs(UnmanagedType.LPWStr)] string oldname, [MarshalAs(UnmanagedType.LPWStr)] string newname);

		[DllImport("advapi32", CharSet = CharSet.Unicode, EntryPoint ="RegCopyTreeW")]
		public static extern int RegCopyTree(SafeRegistryHandle hSourceKey, [MarshalAs(UnmanagedType.LPWStr)] string subKey, SafeRegistryHandle hTarget);

		[DllImport("advapi32", CharSet = CharSet.Unicode, EntryPoint = "RegSaveKeyExW")]
		public static extern int RegSaveKeyEx(SafeRegistryHandle hKey, [MarshalAs(UnmanagedType.LPWStr)] string filename, IntPtr secDesc, uint format = 1);

		[DllImport("advapi32", CharSet = CharSet.Unicode, EntryPoint = "RegSaveKeyW")]
		public static extern int RegSaveKey(SafeRegistryHandle hKey, [MarshalAs(UnmanagedType.LPWStr)] string filename, IntPtr secAttributes);

		[DllImport("advapi32", CharSet = CharSet.Unicode, EntryPoint = "RegRestoreKeyW", ExactSpelling = true)]
		public static extern int RegRestoreKey(SafeRegistryHandle hKey, [MarshalAs(UnmanagedType.LPWStr)] string filename, uint flags = 8);

		static Dictionary<string, object> _privileges = new Dictionary<string, object>();

		public static void EnablePrivilege(string name) {
			object privilege;
			if(!_privileges.TryGetValue(name, out privilege)) {
				Type privilegeType = Type.GetType("System.Security.AccessControl.Privilege");
				privilege = Activator.CreateInstance(privilegeType, name);
				_privileges.Add(name, privilege);
			}
			privilege.GetType().GetMethod("Enable").Invoke(privilege, null);
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
