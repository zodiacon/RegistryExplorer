using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

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
				SelectedItem = e.NewValue;
		}
		
		public object SelectedItem {
			get { return (object)GetValue(SelectedItemProperty); }
			set { SetValue(SelectedItemProperty, value); }
		}

		public static readonly DependencyProperty SelectedItemProperty =
			 DependencyProperty.Register("SelectedItem", typeof(object), typeof(SelectedTreeViewItemBehavior), new PropertyMetadata(null));

		
	}
}
