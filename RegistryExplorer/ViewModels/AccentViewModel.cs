using MahApps.Metro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace RegistryExplorer.ViewModels {
	class AccentViewModel {
		public Accent Accent { get; }

		public AccentViewModel(Accent accent) {
			Accent = accent;

		}

		public string Name => Accent.Name;
		public Brush Color => Accent.Resources["AccentColorBrush"] as Brush;

	}
}
