using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace RegistryExplorer.ViewModels {
	class RegistryTree {
		List<RegistryKey> _roots = new List<RegistryKey> {
			Registry.ClassesRoot,
			Registry.CurrentUser,
			Registry.LocalMachine,
			Registry.CurrentConfig,
			Registry.Users 
		};

		ObservableCollection<FileInfo> _fileRoots = new ObservableCollection<FileInfo>();

		public ObservableCollection<FileInfo> FileRoots {
			get { return _fileRoots; }
		}

		public IEnumerable<RegistryKeyItem> ComputerRoots {
			get { return _roots.Select(root => new RegistryKeyItem(root)); }
		}

	}
}
