using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;
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
			if(found != null) {
				var tv = GetTreeViewItemFromObject(found);
				Debug.Assert(tv != null);
				found.IsSelected = true;
			}
			else {
				NativeMethods.MessageBeep(0x30);	// short beep
			}

			_searchterm = string.Empty;
		}

		void AssociatedObject_TextInput(object sender, TextCompositionEventArgs e) {
			_timer.Start();
			if((DateTime.Now - _lastSearch).Milliseconds < 500) {
				_searchterm += e.Text;
				_lastSearch = DateTime.Now;
				return;
			}

			_searchterm = e.Text;
		}

		TreeViewItem GetTreeViewItemFromObject(RegistryKeyItemBase item) {
			var indices = new List<int>(4);
			while(item.Parent != null) {
				indices.Add(item.Parent.SubItems.IndexOf(item));
				item = item.Parent;
			}
			indices.Add(0);

			// search the tree based on indices

			int index = indices.Count;
			var currentTreeView = AssociatedObject.ItemContainerGenerator.ContainerFromIndex(indices[--index]) as TreeViewItem;
			Debug.Assert(currentTreeView != null);
			while(index > 0) {
				var container = currentTreeView;
				currentTreeView = currentTreeView.ItemContainerGenerator.ContainerFromIndex(indices[--index]) as TreeViewItem;
				if(currentTreeView == null) {		// virtualized
					GetPanelForTreeViewItem(container).BringIntoView(indices[index]);
					currentTreeView = container.ItemContainerGenerator.ContainerFromIndex(indices[index]) as TreeViewItem;
				}
				Debug.Assert(currentTreeView != null);
			}

			return currentTreeView;
		}

		VirtualizingStackPanelEx GetPanelForTreeViewItem(TreeViewItem container) {
			container.ApplyTemplate();
			ItemsPresenter itemsPresenter =
				 (ItemsPresenter)container.Template.FindName("ItemsHost", container);
			if(itemsPresenter != null) {
				itemsPresenter.ApplyTemplate();
			}
			else {
				// The Tree template has not named the ItemsPresenter, 
				// so walk the descendants and find the child.
				itemsPresenter = TreeViewHelper.FindVisualChild<ItemsPresenter>(container);
				if(itemsPresenter == null) {
					container.UpdateLayout();

					itemsPresenter = TreeViewHelper.FindVisualChild<ItemsPresenter>(container);
				}
			}

			Panel itemsHostPanel = (Panel)VisualTreeHelper.GetChild(itemsPresenter, 0);

			// Ensure that the generator for this panel has been created.
			UIElementCollection children = itemsHostPanel.Children;

			var virtualizingPanel = itemsHostPanel as VirtualizingStackPanelEx;
			Debug.Assert(virtualizingPanel != null);

			return virtualizingPanel;
		}

		RegistryKeyItemBase BinarySearch(IList<RegistryKeyItemBase> items) {
			int index1 = 0, index2 = items.Count;
			string lower = _searchterm.ToLower();

			while(index1 != index2) {
				int i = (index1 + index2) / 2;
				if(items[i].Text.StartsWith(_searchterm, StringComparison.CurrentCultureIgnoreCase))
					return items[i];
				if(items[i].Text.ToLower().CompareTo(lower) > 0)
					index2 = i;
				else
					index1 = i;
			}
			return null;
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
			if(e.Key < Key.D0) {
				_searchterm = string.Empty;
			}
		}


	}
}
