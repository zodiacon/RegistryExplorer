using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;
using RegistryExplorer.ViewModels;

namespace RegistryExplorer.Model {
	class RenameKeyCommandContext {
		public RegistryKeyItem Key { get; set; }
		public string OldName { get; set; }
		public string NewName { get; set; }
	}

	class CreateKeyCommandContext {
		public RegistryKeyItem Key { get; set; }
		public string Name { get; set; }
	}

	class DeleteKeyCommandContext {
		public string TempFile { get; set; }
		public RegistryKeyItem Key { get; set; }
		public string Name { get; set; }
	}

	static class Commands {
		public static IAppCommand RenameKey(RenameKeyCommandContext context) {
			return new AppCommand<RenameKeyCommandContext>(context, ctx => {
				var parent = ctx.Key.Parent as RegistryKeyItem;
				Debug.Assert(parent != null);
				using(var key = parent.Root.OpenSubKey(parent.Path ?? string.Empty, true)) {
					int error = NativeMethods.RegRenameKey(key.Handle, ctx.OldName, ctx.NewName);
					if(error != 0)
						throw new Win32Exception(error);
				}

				ctx.Key.Text = ctx.NewName;

				// swap names for undo
				var temp = ctx.OldName;
				ctx.OldName = ctx.NewName;
				ctx.NewName = temp;
			}) { Description = "Rename key" };
		}

		public static IAppCommand CreateKey(CreateKeyCommandContext context) {
			return new AppCommand<CreateKeyCommandContext>(context, ctx => {
				ctx.Key.CreateNewKey(ctx.Name);
			}, ctx => {
				ctx.Key.DeleteKey(ctx.Name);
			}) { Description = "Create key" };
		}

		public static IAppCommand DeleteKey(DeleteKeyCommandContext context) {
			return new AppCommand<DeleteKeyCommandContext>(context, ctx => {
				File.Delete(ctx.TempFile);
				using(var key = ctx.Key.Root.OpenSubKey(ctx.Key.Path, true)) {
					NativeMethods.RegSaveKeyEx(key.Handle, ctx.TempFile, IntPtr.Zero);
				}
				ctx.Name = ctx.Key.Text;
				ctx.Key.Root.DeleteSubKeyTree(ctx.Key.Path);
				ctx.Key.Parent.SubItems.Remove(ctx.Key);
			}, ctx => {
				var parentKey = (RegistryKeyItem)ctx.Key.Parent;
				var item = parentKey.CreateNewKey(ctx.Name);
				using(var parent = item.Root.OpenSubKey(item.Path, true)) {
					NativeMethods.RegRestoreKey(parent.Handle, ctx.TempFile);
				}
				item.Refresh();
				File.Delete(ctx.TempFile);
			}) { Description = "Delete key" };
		}
	}
}
