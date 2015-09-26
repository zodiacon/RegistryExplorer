using System.ComponentModel;
using System.Diagnostics;
using Microsoft.Win32;
using RegistryExplorer.ViewModels;

namespace RegistryExplorer.Model {
	class RenameKeyContext {
		public RegistryKeyItem Key { get; set; }
		public string OldName { get; set; }
		public string NewName { get; set; }
	}

	class CreateKeyCommandContext {
		public RegistryKey Root { get; set; }
		public string Name { get; set; }
		public string Path { get; set; }
	}

	static class Commands {
		public static IAppCommand RenameKeyCommand(RenameKeyContext context) {
			return new AppCommand<RenameKeyContext>(context, ctx => {
				var parent = ctx.Key.Parent as RegistryKeyItem;
				Debug.Assert(parent != null);
				using(var key = parent.Root.OpenSubKey(parent.Path, true)) {
					int error = NativeMethods.RegRenameKey(key.Handle, ctx.OldName, ctx.NewName);
					if(error != 0)
						throw new Win32Exception(error);
				}

				ctx.Key.Text = ctx.NewName;

				// swap names for undo
				var temp = ctx.OldName;
				ctx.OldName = ctx.NewName;
				ctx.NewName = temp;
			}) { Description = "rename key" };
		}

		public static IAppCommand CreateKeyCommand(CreateKeyCommandContext context) {
			return new AppCommand<CreateKeyCommandContext>(context, ctx => {
				ctx.Root.CreateSubKey(ctx.Path);
			}, ctx => {
				ctx.Root.DeleteSubKey(ctx.Path);
			}) { Description = "create key" };
		}

	}
}
