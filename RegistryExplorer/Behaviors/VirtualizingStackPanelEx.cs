using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace RegistryExplorer.Behaviors {
	public class VirtualizingStackPanelEx : VirtualizingStackPanel {
		/// <summary>
		/// Publically expose BringIndexIntoView.
		/// </summary>
		public void BringIntoView(int index) {

			base.BringIndexIntoView(index);
		}
	}

}
