using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interactivity;
using System.Windows.Media;
using Prism;

namespace RegistryExplorer.Behaviors {
	class ActiveViewBehavior : Behavior<FrameworkElement> {
		protected override void OnAttached() {
			base.OnAttached();

			AssociatedObject.PreviewGotKeyboardFocus += AssociatedObject_PreviewGotKeyboardFocus;
			AssociatedObject.PreviewLostKeyboardFocus += AssociatedObject_PreviewLostKeyboardFocus;
		}

		private void AssociatedObject_PreviewLostKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e) {
			var ctx = AssociatedObject.DataContext as IActiveAware;
			if(ctx != null && ctx.IsActive && e.NewFocus != null && !((FrameworkElement)e.NewFocus).IsChildOf(AssociatedObject))
				ctx.IsActive = false;
		}

		protected override void OnDetaching() {
			AssociatedObject.PreviewGotKeyboardFocus -= AssociatedObject_PreviewGotKeyboardFocus;
			AssociatedObject.PreviewLostKeyboardFocus -= AssociatedObject_PreviewLostKeyboardFocus;

			base.OnDetaching();
		}

		private void AssociatedObject_PreviewGotKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e) {
			var ctx = AssociatedObject.DataContext as IActiveAware;
			if(ctx != null && !ctx.IsActive && e.OldFocus != null && !((FrameworkElement)e.OldFocus).IsChildOf(AssociatedObject))
				ctx.IsActive = true;
		}
	}
}
