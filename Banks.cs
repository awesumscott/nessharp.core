using System;
using System.Collections.Generic;
using System.Linq;
using static NESSharp.Core.AL;

namespace NESSharp.Core {
	public class Bank {
		public byte[] Rom;	//PRG bytes
		public U16 Origin;	//Starting address for this chunk
		public bool Fixed;	//Is this chunk always stationary (not swappable)?
		public int Size = 0;
		public List<object> AsmWithRefs = new List<object>();
		//private int _writeIndex = 0;

		public Bank(int size, U16 origin, bool isFixed = false) {
			Origin = origin;
			Fixed = isFixed;
			Rom = new byte[size != 0 ? size : 65536];
			Size = size;
			for (var i = 0; i < size; i++)
				Rom[i] = 0xFF;
		}

		public void WriteContext() {
			var codeLength = Code[CodeContextIndex].Count;
			if (Size != 0 && codeLength > Size) throw new Exception("Overflow in bank " + CodeContextIndex);
			//var outputIndex = 0;
			var offset = 0;
			for (var i = 0; i < codeLength; i++) {
				var op = Code[CodeContextIndex][i];
				if (!(op is OpCode)) {
					//Handle non-code ops
					if (op is Label label) {
						label.Address = Addr((U16)(Origin + offset));

						var name = Labels.NameByRef(label);
						if (!string.IsNullOrEmpty(name))
							ROMManager.AsmOutput += name + ":\n";
					} else if (op is OpRaw raw) {
						AsmWithRefs.AddRange(raw.Value.Cast<object>());
						var bytes = raw.Value.Cast<object>().Select(x => x.ToString()).ToList();
						ROMManager.AsmOutput += $"\t.db { string.Join(',', bytes) }\n";
						offset += raw.Length;
					} else if (op is OpComment comment) {
						ROMManager.AsmOutput += $"; { comment.Text }\n";
						ROMManager.Tools.AssemblerOutput?.AppendComment(comment.Text);
					}
					continue;
				}
				var opCode = (OpCode)op;
				AsmWithRefs.Add(opCode.Value);
				offset++;

				if (opCode.Length == 2) {
					if (opCode.Param?.IsResolvable() ?? false) {
						AsmWithRefs.Add(opCode.Param);
					} else {
						AsmWithRefs.Add(((U8)opCode.Param).Value);
					}
					offset++;
				} else if (opCode.Length == 3) {
					if (opCode.Param is Label lbl) {
						AsmWithRefs.Add(lbl);
					} else if (opCode.Param is LabelIndexed li) {
						AsmWithRefs.Add(li.Label);
					} else { //assume the parameters are for a 2 byte value
						var addr = (U16)opCode.Param;
						AsmWithRefs.Add(((U16)opCode.Param).Lo.Value);
						AsmWithRefs.Add(((U16)opCode.Param).Hi.Value);
					}
					offset += 2;
				}

				//ROMManager.AsmOutput += "\t" + string.Format(Asm.OpRefs.Where(x => x.Byte == opCode.Value).First().Format, opCode.Param) + "\n";
				ROMManager.AsmOutput += "\t" + string.Format(Asm.OpRefs.Where(x => x.Byte == opCode.Value).First().ToAsm(DebugFileNESASM.OpToAsm), opCode.Param) + "\n";
				ROMManager.Tools.AssemblerOutput?.AppendOp(Asm.OpRefs.Where(x => x.Byte == opCode.Value).First(), opCode);
			}
			InitCode();
		}
	}
}
