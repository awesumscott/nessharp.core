using System;
using System.Collections.Generic;
using System.Linq;
using static NESSharp.Core.AL;

namespace NESSharp.Core {
	public static class Context {
		private static readonly Stack<Label> _startLabels = new Stack<Label>();
		public static void New(Action body) {
			Push();
			body.Invoke();
			Pop();
		}
		/// <summary>Append code to scope right before current scope body</summary>
		public static void Parent(Action body) {
			CodeContextIndex--;
			body.Invoke();
			CodeContextIndex++;
		}
		public static void Push() {
			Reset();
			CodeContextIndex++;
			Code.Add(new List<IOperation>());
			var lbl = Labels.New();
			_startLabels.Push(lbl);
			Use(lbl);
		}
		public static void Pop() {
			Code[CodeContextIndex - 1].AddRange(Code[CodeContextIndex]);
			Code.RemoveAt(CodeContextIndex);
			CodeContextIndex--;
			_startLabels.Pop();
			Reset();
		}
		public static void Delete() {
			Reset();
			Code.RemoveAt(CodeContextIndex);
			CodeContextIndex--;
		}
		public static int Length {
			get {
				int len = 0;
				foreach (var o in Code[CodeContextIndex]) {
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
