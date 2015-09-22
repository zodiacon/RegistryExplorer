using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Threading;
using RegistryExplorer.ViewModels;

namespace RegistryExplorer.Behaviors {
	class KeyNavigationTreeViewBehavior : Behavior<TreeView> {
		string _searchterm = string.Empty;
		DateTime _lastSearch = DateTime.Now;
		DispatcherTimer _timer;

		protected override void OnAttached() {
			base.OnAttached();

			AssociatedObject.KeyUp += AssociatedObject_KeyUp;
			AssociatedObject.TextInput += AssociatedObject_TextInput;

			_timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(.5) };
			_timer.Tick += _timer_Tick;
		}

		void _timer_Tick(object sender, EventArgs e) {
			_timer.Stop();

			if(string.IsNullOrEmpty(_searchterm))
				return;

			var item = AssociatedObject.SelectedItem as RegistryKeyItemBase;
			if(item == null) return;

			var found = SearchSubTree(item);
			if(found != null)
				found.IsSelected = true;
		}

		void AssociatedObject_TextInput(object sender, TextCompositionEventArgs e) {
			_timer.Start();
			if((DateTime.Now - _lastSearch).Milliseconds < 300) {
				_searchterm += e.Text;
				return;
			}

			_searchterm = e.Text;
		}


		private RegistryKeyItemBase SearchSubTree(RegistryKeyItemBase item) {
			if(item.IsExpanded && item.SubItems != null) {
				foreach(var subItem in item.SubItems) {
					if(subItem.Text.StartsWith(_searchterm, StringComparison.CurrentCultureIgnoreCase)) {
						return subItem;
					}
					RegistryKeyItemBase newItem;
					if(subItem.IsExpanded && (newItem = SearchSubTree(subItem)) != null) {
						return newItem;
					}
				}
			}
			if(item.Parent != null) {
				foreach(var subItem in item.Parent.SubItems.Skip(item.Parent.SubItems.IndexOf(item) + 1)) {
					if(subItem.Text.StartsWith(_searchterm, StringComparison.CurrentCultureIgnoreCase)) {
						return subItem;
					}
				}

				//var index = item.Parent.SubItems.IndexOf(item);
				//if(index < item.Parent.SubItems.Count)
				//	return SearchSubTree(item.Parent.SubItems[index]);
			}
			return null;
		}

		protected override void OnDetaching() {
			AssociatedObject.KeyUp -= AssociatedObject_KeyUp;
			AssociatedObject.TextInput -= AssociatedObject_TextInput;

			base.OnDetaching();
		}

		void AssociatedObject_KeyUp(object sender, System.Windows.Input.KeyEventArgs e) {
			if(e.Key < Key.A)
				_searchterm = "";
		}
	}
}
