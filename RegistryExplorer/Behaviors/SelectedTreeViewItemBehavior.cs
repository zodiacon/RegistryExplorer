using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Interactivity;
using RegistryExplorer.ViewModels;

namespace RegistryExplorer.Behaviors {
	class SelectedTreeViewItemBehavior : Behavior<TreeView> {
		protected override void OnAttached() {
			base.OnAttached();

			AssociatedObject.SelectedItemChanged += AssociatedObject_SelectedItemChanged;
		}


		protected override void OnDetaching() {
			AssociatedObject.SelectedItemChanged -= AssociatedObject_SelectedItemChanged;
			base.OnDetaching();
		}

		void AssociatedObject_SelectedItemChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<object> e) {
			SelectedItem = (RegistryKeyItemBase)e.NewValue;
		}

		public RegistryKeyItemBase SelectedItem {
			get { return (RegistryKeyItemBase)GetValue(SelectedItemProperty); }
			set { SetValue(SelectedItemProperty, value); }
		}

		public static readonly DependencyProperty SelectedItemProperty =
			 DependencyProperty.Register("SelectedItem", typeof(RegistryKeyItemBase), typeof(SelectedTreeViewItemBehavior), new PropertyMetadata(null));
		
	}
}
