using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism;
using Prism.Mvvm;

namespace RegistryExplorer {
	class ViewModelBase : BindableBase, IActiveAware {
		bool _isActive;
		public bool IsActive {
			get {
				return _isActive;
			}
			set {
				if(SetProperty(ref _isActive, value))
					OnIsActiveChanged();
			}
		}

		protected virtual void OnIsActiveChanged() {
			var ac = IsActiveChanged;
			if(ac != null)
				ac(this, EventArgs.Empty);
			if(IsActive)
				ActiveView = this;
		}

		public event EventHandler IsActiveChanged;

		static ViewModelBase _activeView;

		public ViewModelBase ActiveView {
			get { return _activeView; }
			set { SetProperty(ref _activeView, value); }
		}

	}
}
