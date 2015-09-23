using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Prism.Mvvm;

namespace RegistryExplorer.ViewModels {
	class DataGridViewModel : BindableBase {
		MainViewModel _mainViewModel;

		public DataGridViewModel(MainViewModel vm) {
			_mainViewModel = vm;

			vm.PropertyChanged += vm_PropertyChanged;
		}

		void vm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
			switch(e.PropertyName) {
			case "SelectedItem":
				FilterText = string.Empty;
				OnPropertyChanged(() => Values);
				break;

			case "IsReadOnlyMode":
				OnPropertyChanged(() => IsReadOnlyMode);
				break;
			}
		}

		IEnumerable<RegistryValue> _values;

		public IEnumerable<RegistryValue> Values {
			get {
				var regItem = _mainViewModel.SelectedItem as RegistryKeyItem;
				return (_values = (regItem != null ? regItem.Values : null));
			}
		}

		public bool IsReadOnlyMode {
			get { return _mainViewModel.IsReadOnlyMode; }
		}

		private string _filterText;

		public string FilterText {
			get { return _filterText; }
			set { 
				if(SetProperty(ref _filterText, value)) {
					if(string.IsNullOrEmpty(value)) {
						CollectionViewSource.GetDefaultView(_values).Filter = null;
					}
					else if(_values != null) {
						CollectionViewSource.GetDefaultView(_values).Filter = obj => {
							var theValue = (RegistryValue)obj;
							var lvalue = value.ToLower();
							return theValue.Name.ToLower().Contains(lvalue) || theValue.ValueAsString.ToLower().Contains(lvalue);
						};
					}
				}
			}
		}

		
	}
}
