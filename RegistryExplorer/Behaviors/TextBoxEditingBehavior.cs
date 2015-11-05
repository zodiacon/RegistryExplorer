using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;
using RegistryExplorer.ViewModels;

namespace RegistryExplorer.Behaviors {
	class TextBoxEditingBehavior : Behavior<TextBox> {

		protected override void OnAttached() {
			base.OnAttached();

			AssociatedObject.IsVisibleChanged += AssociatedObject_IsVisibleChanged;
		}

		protected override void OnDetaching() {

			AssociatedObject.IsVisibleChanged -= AssociatedObject_IsVisibleChanged;

			base.OnDetaching();
		}

		private void AssociatedObject_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) {
			if((bool)e.NewValue) {
				AssociatedObject.SelectAll();
				AssociatedObject.Focus();
			}
		}
	}
}
