using System;
using System.Collections.Generic;
using System.Linq;

namespace NESSharp.Core {
	/// <summary>
	/// Create a new scoped block of operations. This keeps track of references needed for branching, and is useful for loops and conditions.
	/// </summary>
	public static class Context {
		private static List<List<IOperation>>	_ops;
		private static short					_scopeIndex;
		public static IEnumerable<IOperation> Operations => _ops[_scopeIndex];
		private static readonly Stack<Label> _startLabels = new();
		public static void InitCode() {
			_ops = new List<List<IOperation>>(); //clear code to prepare for next bank definition
			_scopeIndex = 0;
			_ops.Add(new List<IOperation>());
		}
		public static void New(Action body/*, StateMaintain maintainState = StateMaintain.None*/) {
			Push();
			body.Invoke();
			Pop();
		}
		/// <summary>Append code to scope right before current scope body</summary>
		public static void Parent(Action body) {
			_scopeIndex--;
			body.Invoke();
			_scopeIndex++;
		}
		public static void Push() {
			AL.Reset();
			_scopeIndex++;
			_ops.Add(new List<IOperation>());
			var lbl = AL.Labels.New();
			_startLabels.Push(lbl);
			Write(lbl);
		}
		public static void Pop() {
			_ops[_scopeIndex - 1].AddRange(_ops[_scopeIndex]);
			_ops.RemoveAt(_scopeIndex);
			_scopeIndex--;
			_startLabels.Pop();
			AL.Reset();
		}
		public static void Delete() {
			AL.Reset();
			_ops.RemoveAt(_scopeIndex);
			_scopeIndex--;
		}
		public static int Length {
			get {
				int len = 0;
				foreach (var o in _ops[_scopeIndex]) {
					if (o.GetType().GetInterfaces().Contains(typeof(IOperation)))
						len += o.Length;
				}
				return len;
			}
		}
		public static bool StartBranchable => Length <= 126; //128 - 2, branches are 2 bytes
		public static U8 Start => (byte)(254 - Length); //256 - length - 2 (the branch opcode is included in the jump distance)
		public static Label StartLabel => _startLabels.Peek();
		public static void Write(IOperation op) => _ops[_scopeIndex].Add(op);
	}
}
