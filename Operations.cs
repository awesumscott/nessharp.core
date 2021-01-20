using System;
using System.Linq;

namespace NESSharp.Core {
	public interface IOperation {
		public int Length {get;set;}
	}
	public class OpRaw : IOperation {
		public int Length {get;set;}
		public object[] Value;
		public OpRaw(params byte[] v) {
			Length = v.Length;
			Value = v.Cast<object>().ToArray();
		}
		public OpRaw(object[] v) {
			Length = v.Length;
			Value = v;
		}
	}
	public class OpComment : IOperation {
		public int Length {get;set;}
		public string Text;
		public long ID;
		public OpComment(string text) {
			ID = DateTime.Now.Ticks;
			Text = text;
			Length = 0;
		}
	}
	public class OpCode : IOperation {
		public int Length {get;set;}
		public byte Value;
		public IOperand? Param;
		public OpCode(byte opVal, byte len, IOperand? param = null) {
			Value = opVal;
			Length = len;
			Param = param;
		}
	}
}
