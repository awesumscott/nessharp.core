using System;
using System.Collections.Generic;
using System.Linq;

namespace NESSharp.Core {
	public class Bank {
		public U8 Id;
		public byte[] Rom;	//PRG bytes
		public U16 Origin;	//Starting address for this chunk
		public bool Fixed;	//Is this chunk always stationary (not swappable)?
		public int Size = 0;
		public List<IOperation> AsmWithRefs = new List<IOperation>();
		//private int _writeIndex = 0;

		public Bank(U8 id, int size, U16 origin, bool isFixed = false) {
			Id = id;
			Origin = origin;
			Fixed = isFixed;
			Rom = new byte[size != 0 ? size : 65536];
			Size = size;
			for (var i = 0; i < size; i++)
				Rom[i] = 0xFF;
		}

		public void WriteContext() {
			var codeLength = AL.Code[AL.CodeContextIndex].Count;
			if (Size != 0 && codeLength > Size) throw new Exception("Overflow in bank " + AL.CodeContextIndex);

			var offset = 0;
			for (var i = 0; i < codeLength; i++) {
				var op = AL.Code[AL.CodeContextIndex][i];
				AsmWithRefs.Add(op);

				//Determine label addresses by keeping a count of bytes to be output
				if (op is Label label)			label.Address = AL.Addr((U16)(Origin + offset));
				else if (op is OpRaw raw)		offset += raw.Length;
				else if (op is OpCode opCode)	offset += opCode.Length;
			}
			AL.InitCode();
		}

		public void WriteAsmOutput() {
			foreach (var op in AsmWithRefs) {
				if (op is Label label) {
					var name = AL.Labels.NameByRef(label);
					if (string.IsNullOrEmpty(name)) continue;
					if (!label.IsUsed && name.StartsWith("_") && int.TryParse(name.Substring(1), out _)) continue;
					ROMManager.Tools.AssemblerOutput.AppendLabel(name);
				} else if (op is OpRaw raw)			ROMManager.Tools.AssemblerOutput.AppendBytes(raw.Value.Cast<object>().Select(x => x.ToString() ?? string.Empty).ToList());
				else if (op is OpComment comment)	ROMManager.Tools.AssemblerOutput.AppendComment(comment.Text);
				else if (op is OpCode opCode)		ROMManager.Tools.AssemblerOutput.AppendOp(Asm.OpRefs.Where(x => x.Byte == opCode.Value).First(), opCode);
			}
		}
	}
}
