using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;

namespace RegistryExplorer {
	class CommandManager : BindableBase {
		public int UndoLevel { get; private set; }
		List<IAppCommand> _undoList;
		List<IAppCommand> _redoList;

		public CommandManager(int undoLevel = 32) {
			if(undoLevel < 1)
				throw new ArgumentException("Undo level must be at least 1");
			UndoLevel = undoLevel;

			_undoList = new List<IAppCommand>(UndoLevel);
			_redoList = new List<IAppCommand>(UndoLevel);
		}

		public void AddCommand(IAppCommand command, bool execute = true) {
			_undoList.Add(command);
			_redoList.Clear();
			if(UndoLevel > 0 && _undoList.Count > UndoLevel)
				_undoList.RemoveAt(0);
			if(execute)
				command.Execute();
			UpdateChanges();
		}

		public bool CanUndo {
			get {
				return _undoList.Count > 0;
			}
		}

		public bool CanRedo {
			get {
				return _redoList.Count > 0;
			}
		}

		public string UndoDescription {
			get {
				return CanUndo ? _undoList[_undoList.Count - 1].Description : string.Empty;
			}
		}

		public string RedoDescription {
			get {
				return CanRedo ? _redoList[_redoList.Count - 1].Description : string.Empty;
			}
		}

		public virtual void Undo() {
			if(!CanUndo)
				throw new InvalidOperationException("can't undo");
			int index = _undoList.Count - 1;
			_undoList[index].Undo();
			_redoList.Add(_undoList[index]);
			_undoList.RemoveAt(index);
			UpdateChanges();
		}

		public void UpdateChanges() {
			OnPropertyChanged(nameof(CanUndo));
			OnPropertyChanged(nameof(CanRedo));
			OnPropertyChanged(nameof(UndoDescription));
			OnPropertyChanged(nameof(RedoDescription));

		}

		public virtual void Redo() {
			if(!CanRedo)
				throw new InvalidOperationException("Can't redo");
			var cmd = _redoList[_redoList.Count - 1];
			cmd.Execute();
			_redoList.RemoveAt(_redoList.Count - 1);
			_undoList.Add(cmd);
			UpdateChanges();
		}

		public void Clear() {
			_undoList.Clear();
			_redoList.Clear();
			UpdateChanges();
		}
	}
}
