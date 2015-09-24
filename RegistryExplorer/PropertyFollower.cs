using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RegistryExplorer {
	class PropertyFollower<TSource, TTarget> : IDisposable
		where TSource : INotifyPropertyChanged
		where TTarget : INotifyPropertyChanged {
		List<string> _propertyNames = new List<string>();
		TSource _source;
		TTarget _target;
		MethodInfo _raiseMethod;
		Dictionary<string, Action<string>> _extraWork;

		public PropertyFollower(TSource source, TTarget target, params string[] propertyNames) {
			Debug.Assert(source != null && target != null);
			if(propertyNames == null)
				throw new ArgumentException("At least one property must be provided", "propertyNames");

			source.PropertyChanged += source_PropertyChanged;
			_source = source;
			_target = target;
			if(propertyNames != null)
				_propertyNames.AddRange(propertyNames);

			_raiseMethod = _target.GetType().GetMethod("OnPropertyChanged",
				BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
				null, new[] { typeof(string) }, null);
			if(_raiseMethod == null)
				throw new InvalidOperationException("Target object must have OnPropertyChanged(string) method");
		}

		void source_PropertyChanged(object sender, PropertyChangedEventArgs e) {
			if(_propertyNames.Contains(e.PropertyName)) {
				_raiseMethod.Invoke(_target, new[] { e.PropertyName });
				Action<string> action;
				if(_extraWork != null && _extraWork.TryGetValue(e.PropertyName, out action))
					action(e.PropertyName);
			}
		}

		public PropertyFollower<TSource, TTarget> Add(string propertyName, Action<string> extraWork = null) {
			_propertyNames.Add(propertyName);
			if(extraWork != null) {
				if(_extraWork == null)
					_extraWork = new Dictionary<string, Action<string>>();
				_extraWork.Add(propertyName, extraWork);
			}
			return this;
		}

		public void Dispose() {
			_source.PropertyChanged -= source_PropertyChanged;
		}
	}
}
