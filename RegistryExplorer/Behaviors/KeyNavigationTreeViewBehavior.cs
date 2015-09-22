using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;
using RegistryExplorer.ViewModels;

namespace RegistryExplorer.Behaviors {
	class KeyNavigationTreeViewBehavior : Behavior<TreeView> {
		private string _searchterm = "";
		private DateTime _lastSearch = DateTime.Now;

		protected override void OnAttached() {
			base.OnAttached();

			AssociatedObject.KeyUp += AssociatedObject_KeyUp;
			AssociatedObject.TextInput += AssociatedObject_TextInput;
		}

		void AssociatedObject_TextInput(object sender, TextCompositionEventArgs e) {
			if((DateTime.Now - _lastSearch).Milliseconds > 300)
				_searchterm = "";

			var item = AssociatedObject.SelectedItem as RegistryKeyItemBase;
			if(item == null) return;

			Debug.Assert(item != null);

			_lastSearch = DateTime.Now;
			_searchterm += e.Text;

			var found = SearchSubTree(item);
			if(found != null) {
				found.IsSelected = true;
			}
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
