using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Media;
using RegistryExplorer.ViewModels;

namespace RegistryExplorer.Behaviors {
	internal static class TreeViewHelper {
		public static TreeViewItem GetTreeViewItem(ItemsControl container, RegistryKeyItemBase item) {
			if(container != null) {
				if(container.DataContext == item) {
					return container as TreeViewItem;
				}

				// Expand the current container
				if(container is TreeViewItem && !((TreeViewItem)container).IsExpanded) {
					container.SetValue(TreeViewItem.IsExpandedProperty, true);
				}

				// Try to generate the ItemsPresenter and the ItemsPanel.
				// by calling ApplyTemplate.  Note that in the 
				// virtualizing case even if the item is marked 
				// expanded we still need to do this step in order to 
				// regenerate the visuals because they may have been virtualized away.

				container.ApplyTemplate();
				ItemsPresenter itemsPresenter =
					 (ItemsPresenter)container.Template.FindName("ItemsHost", container);
				if(itemsPresenter != null) {
					itemsPresenter.ApplyTemplate();
				}
				else {
					// The Tree template has not named the ItemsPresenter, 
					// so walk the descendents and find the child.
					itemsPresenter = FindVisualChild<ItemsPresenter>(container);
					if(itemsPresenter == null) {
						container.UpdateLayout();

						itemsPresenter = FindVisualChild<ItemsPresenter>(container);
					}
				}

				Panel itemsHostPanel = (Panel)VisualTreeHelper.GetChild(itemsPresenter, 0);


				// Ensure that the generator for this panel has been created.
				UIElementCollection children = itemsHostPanel.Children;

				var virtualizingPanel = itemsHostPanel as VirtualizingStackPanelEx;
				Debug.Assert(virtualizingPanel != null);

				int count = container.Items.Count;
				int index1 = 0, index2 = count;
				while(index1 != index2) {
					int newindex = (index1 + index2) / 2;
					TreeViewItem subContainer;
					if(virtualizingPanel != null) {
						// Bring the item into view so 
						// that the container will be generated.
						virtualizingPanel.BringIntoView(newindex);

						subContainer =
							 (TreeViewItem)container.ItemContainerGenerator.
							 ContainerFromIndex(newindex);
					}
					else {
						subContainer =
							 (TreeViewItem)container.ItemContainerGenerator.
							 ContainerFromIndex(newindex);

						// Bring the item into view to maintain the 
						// same behavior as with a virtualizing panel.
						subContainer.BringIntoView();
					}

					if(subContainer.DataContext == item) {
						return subContainer;
					}

					if(item.Text.CompareTo((subContainer.DataContext as RegistryKeyItemBase).Text) > 0)
						index1 = newindex;
					else
						index2 = newindex;
				}

				//if(subContainer != null && subContainer.IsExpanded) {
				//	// Search the next level for the object.
				//	TreeViewItem resultContainer = GetTreeViewItem(subContainer, item);
				//	if(resultContainer != null) {
				//		return resultContainer;
				//	}
				//	else {
				//		// The object is not under this TreeViewItem
				//		// so collapse it.
				//		//subContainer.IsExpanded = false;
				//	}

				//}
			}

			return null;
		}


		public static T FindVisualChild<T>(Visual visual) where T : Visual {
			for(int i = 0; i < VisualTreeHelper.GetChildrenCount(visual); i++) {
				Visual child = (Visual)VisualTreeHelper.GetChild(visual, i);
				if(child != null) {
					T correctlyTyped = child as T;
					if(correctlyTyped != null) {
						return correctlyTyped;
					}

					T descendent = FindVisualChild<T>(child);
					if(descendent != null) {
						return descendent;
					}
				}
			}

			return null;
		}
	}
}