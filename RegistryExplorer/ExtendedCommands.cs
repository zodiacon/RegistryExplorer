using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Prism.Commands;

namespace RegistryExplorer {
	public interface ICommandEx : ICommand {
		void Undo();
		bool CanUndo { get; }
		void RaiseCanExecuteChanged();
	}

	class DelegateCommandEx : DelegateCommand, ICommandEx {
		Action _undo;
		Func<bool> _canUndo;

		public DelegateCommandEx(Action execute, Action undo, Func<bool> canExecute = null, Func<bool> canUndo = null) : base(execute, canExecute) {
			_undo = undo;
			_canUndo = canUndo;
		}

		public bool CanUndo {
			get {
				return _canUndo == null ? true : _canUndo();
			}
		}

		public void Undo() {
			if(CanUndo)
				_undo();
		}
	}

	class DelegateCommandEx<T> : DelegateCommandEx {
		public T State { get; set; }
		public DelegateCommandEx(T state, Action execute, Action undo, Func<bool> canExecute = null, Func<bool> canUndo = null) : this(execute, undo, canExecute, canUndo) {
			State = state;
		}

		public DelegateCommandEx(Action execute, Action undo, Func<bool> canExecute = null, Func<bool> canUndo = null) : base(execute, undo, canExecute, canUndo) {

		}
	}
}
