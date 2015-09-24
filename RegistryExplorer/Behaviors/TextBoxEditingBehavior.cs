using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;
using RegistryExplorer.ViewModels;

namespace RegistryExplorer.Behaviors {
	class TextBoxEditingBehavior : Behavior<TextBox> {
		protected override void OnAttached() {
			base.OnAttached();

			AssociatedObject.KeyUp += AssociatedObject_KeyUp;
			AssociatedObject.Loaded += AssociatedObject_Loaded;
		}

		string _originalText;

		private void AssociatedObject_Loaded(object sender, System.Windows.RoutedEventArgs e) {
			_originalText = AssociatedObject.Text;
		}

		private void AssociatedObject_KeyUp(object sender, KeyEventArgs e) {
			switch(e.Key) {
				case Key.Escape:
					AssociatedObject.Text = _originalText;
					goto case Key.Enter;
						
				case Key.Enter:
					AssociatedObject.MoveFocus(new TraversalRequest(FocusNavigationDirection.Previous));
					break;
				default:
					return;
			}
			e.Handled = true;
		}
	}
}
