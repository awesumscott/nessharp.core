using System;
using System.Collections.Generic;
using System.Linq;

namespace NESSharp.Core {
	/// <summary>
	/// Create a new scoped block of operations. This keeps track of references needed for branching, and is useful for loops and conditions.
	/// </summary>
	public static class Context {
		/*public enum StateMaintain {
			None,
			Entry
		};*/
		private static readonly Stack<Label> _startLabels = new Stack<Label>();
		public static void New(Action body/*, StateMaintain maintainState = StateMaintain.None*/) {
			Push(/*maintainState*/);
			body.Invoke();
			Pop();
		}
		/// <summary>Append code to scope right before current scope body</summary>
		public static void Parent(Action body) {
			AL.CodeContextIndex--;
			body.Invoke();
			AL.CodeContextIndex++;
		}
		public static void Push(/*StateMaintain maintainState = StateMaintain.None*/) {
			/*if (maintainState != StateMaintain.Entry)*/
			AL.Reset();
			AL.CodeContextIndex++;
			AL.Code.Add(new List<IOperation>());
			var lbl = AL.Labels.New();
			_startLabels.Push(lbl);
			AL.Use(lbl);
		}
		public static void Pop() {
			AL.Code[AL.CodeContextIndex - 1].AddRange(AL.Code[AL.CodeContextIndex]);
			AL.Code.RemoveAt(AL.CodeContextIndex);
			AL.CodeContextIndex--;
			_startLabels.Pop();
			AL.Reset();
		}
		public static void Delete() {
			AL.Reset();
			AL.Code.RemoveAt(AL.CodeContextIndex);
			AL.CodeContextIndex--;
		}
		public static int Length {
			get {
				int len = 0;
				foreach (var o in AL.Code[AL.CodeContextIndex]) {
					if (o.GetType().GetInterfaces().Contains(typeof(IOperation)))
						len += o.Length;
				}
				return len;
			}
		}
		public static bool StartBranchable => Length <= 126; //128 - 2, branches are 2 bytes
		public static U8 Start => (byte)(254 - Length); //256 - length - 2 (the branch opcode is included in the jump distance)
		public static Label StartLabel => _startLabels.Peek();
	}
}
