using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;
using RegistryExplorer.ViewModels;

namespace RegistryExplorer.Behaviors {
	class TextBoxEditingBehavior : Behavior<TextBox> {

		protected override void OnAttached() {
			base.OnAttached();

			AssociatedObject.IsVisibleChanged += AssociatedObject_IsVisibleChanged;
			AssociatedObject.PreviewKeyUp += AssociatedObject_KeyUp;
			AssociatedObject.LostFocus += AssociatedObject_LostFocus;
		}

		protected override void OnDetaching() {
			AssociatedObject.PreviewKeyUp -= AssociatedObject_KeyUp;
			AssociatedObject.IsVisibleChanged -= AssociatedObject_IsVisibleChanged;
			AssociatedObject.LostFocus -= AssociatedObject_LostFocus;

			base.OnDetaching();
		}

		string _originalText;
		private void AssociatedObject_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) {
			if(AssociatedObject.IsVisible) {
				_originalText = AssociatedObject.Text;
				AssociatedObject.Focus();
				AssociatedObject.SelectAll();
			}
		}

		private void AssociatedObject_LostFocus(object sender, RoutedEventArgs e) {
			ExecutendEditCommand();
		}

		private void ExecutendEditCommand() {
			if(EndEditCommand != null && EndEditCommand.CanExecute(null))
				EndEditCommand.Execute(AssociatedObject.Text);
		}

		private void AssociatedObject_KeyUp(object sender, KeyEventArgs e) {
			switch(e.Key) {
				case Key.Escape:
					AssociatedObject.Text = _originalText;
					goto case Key.Enter;
						
				case Key.Enter:
					break;

				default:
					return;
			}
			AssociatedObject.MoveFocus(new TraversalRequest(FocusNavigationDirection.Previous));
	
			e.Handled = true;
		}

		public ICommand EndEditCommand {
			get { return (ICommand)GetValue(EndEditCommandProperty); }
			set { SetValue(EndEditCommandProperty, value); }
		}

		public static readonly DependencyProperty EndEditCommandProperty =
			DependencyProperty.Register("EndEditCommand", typeof(ICommand), typeof(TextBoxEditingBehavior), new PropertyMetadata(null));


	}
}
