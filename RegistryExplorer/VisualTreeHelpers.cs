using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace RegistryExplorer {
	static class VisualTreeHelpers {
		public static bool IsChildOf(this FrameworkElement source, FrameworkElement parent) {
			while(source != null) {
				if(source == parent)
					return true;
				source = VisualTreeHelper.GetParent(source) as FrameworkElement;
			}
			return false;
		}
	}
}
