using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using RegistryExplorer.ViewModels;

namespace RegistryExplorer.Behaviors {
	class ValueCellTemplateSelector : DataTemplateSelector {
		public DataTemplate SingleValueDataTemplate { get; set; }
		public override DataTemplate SelectTemplate(object item, DependencyObject container) {
			if(item != null) {
				var value = item as RegistryValue;
				Debug.Assert(value != null);

				switch(value.DataType) {
				case RegistryValueKind.DWord:
				case RegistryValueKind.QWord:
				case RegistryValueKind.String:
				case RegistryValueKind.ExpandString:
					return SingleValueDataTemplate;
				}
			}
			return base.SelectTemplate(item, container);
		}
	}
}
