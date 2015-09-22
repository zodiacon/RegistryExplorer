using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;

namespace RegistryExplorer.ViewModels {
	abstract class RegistryKeyItemBase : BindableBase {
		private string _text;
		protected ObservableCollection<RegistryKeyItemBase> _subItems;

		public RegistryKeyItemBase Parent { get; private set; }

		public virtual ObservableCollection<RegistryKeyItemBase> SubItems {
			get {
				if(_subItems == null)
					_subItems = new ObservableCollection<RegistryKeyItemBase>();
				return _subItems; 
			}
		}

		protected RegistryKeyItemBase(RegistryKeyItemBase parent) {
			Parent = parent;
		}

		public string Text {
			get { return _text; }
			set { SetProperty(ref _text, value); }
		}

		private bool _isExpanded;

		public bool IsExpanded {
			get { return _isExpanded; }
			set { SetProperty(ref _isExpanded, value); }
		}

		private bool _isSelected;

		public bool IsSelected {
			get { return _isSelected; }
			set { SetProperty(ref _isSelected, value); }
		}


		public override string ToString() {
			return _text.ToString();
		}
	}
}
