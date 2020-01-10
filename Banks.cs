using System;
using System.Collections.Generic;
using System.Text;
using static NESSharp.Core.AL;
using NESSharp.Core;
using System.Linq;

namespace NESSharp.Core {
	public class Bank {
		public byte[] Rom;	//PRG bytes
		public U16 Origin;	//Starting address for this chunk
		public bool Fixed;	//Is this chunk always stationary (not swappable)?
		public int Size = 0;
		public List<object> AsmWithRefs = new List<object>();
		private int _writeIndex = 0;

		public Bank(int size, U16 origin, bool isFixed = false) {
			Origin = origin;
			Fixed = isFixed;
			Rom = new byte[size != 0 ? size : 65536];
			Size = size;
			for (var i = 0; i < size; i++)
				Rom[i] = 0xFF;
		}
		//public void Write(byte[] bytes) {
		//	for (var i = 0; i < bytes.Length && i < Rom.Length; i++)
		//		Rom[_writeIndex++] = bytes[i];
		//}

		public void WriteContext() {
			var codeLength = Code[CodeContextIndex].Count;
			if (Size != 0 && codeLength > Size) throw new Exception("Overflow in bank " + CodeContextIndex);
			//var outputIndex = 0;
			var offset = 0;
			for (var i = 0; i < codeLength; i++) {
				var op = Code[CodeContextIndex][i];
				if (!(op is OpCode)) {
					if (op is OpLabel label) {
						label.Address = Addr((U16)(Origin + offset));

						var name = Label.NameByRef(label);
						if (!string.IsNullOrEmpty(name))
							ROMManager.AsmOutput += name + ":\n";
					} else if (op is OpRaw raw) {
						//var raw = (OpRaw)Code[CodeContextIndex][i];
						//Console.WriteLine(string.Join(',', raw.Value.Select(x => "#$" + x.ToString("X2"))));
						AsmWithRefs.AddRange(raw.Value.Cast<object>());
						//ROMManager.AsmOutput += "\t;[RAW BINARY OMITTED]\n";
						var bytes = raw.Value.Cast<object>().Select(x => x.ToString()).ToList();
						//if (bytes.Count() < 80)
						ROMManager.AsmOutput += $"\t.db { string.Join(',', bytes) }\n";
						//else {
						//	do {
						//		var byteChunk = bytes.Take(80);
						//		ROMManager.AsmOutput += $"\t.db { string.Join(',', byteChunk) }\n";
						//		bytes = bytes.Skip(80).ToList();
						//	} while (bytes.Any());
						//}
						//TODO:	^This was my attempt at getting NESASM3-friendly asm output. This works, but fails due to
						//		needing ".origin" everywhere. maybe revisit this in the future, maybe don't bother.
						offset += raw.Length;
					} else if (op is OpComment comment) {
						//Console.WriteLine("; " + ((OpComment)Code[CodeContextIndex][i]).Text);
						ROMManager.AsmOutput += $"; { comment.Text }\n";
					}
					continue;
				}
				var opCode = (OpCode)op;
				AsmWithRefs.Add(opCode.Value);
				offset++;

				if (opCode.Length == 2) {
					if (opCode.Param.IsResolvable()) {
						AsmWithRefs.Add(opCode.Param);
						offset++;
					}  else {
						AsmWithRefs.Add(((U8)opCode.Param).Value);
						offset++;
					}
				}
				//var varName = string.Empty;
				if (opCode.Length == 3) {
					if (opCode.Param is OpLabel lbl) {
						AsmWithRefs.Add(lbl.Reference());
						offset += 2;
					} else if (opCode.Param is OpLabelIndexed li) {
						AsmWithRefs.Add(li.Label.Reference());
						offset += 2;
					} else { //assume the parameters are for a 2 byte value
						var addr = (U16)opCode.Param;
						//varName = VarRegistry.Where(x => x.Value.Address.Contains(addr)).Select(x => x.Key).FirstOrDefault();
						AsmWithRefs.Add(((U16)opCode.Param).Lo.Value);
						AsmWithRefs.Add(((U16)opCode.Param).Hi.Value);
						offset += 2;
					}
				}
				//Console.Write(string.Format(Asm.OpCodeDecoder[opCode.Value], opCode.Param, opCode.Param2) + "\n");
				
				//ROMManager.AsmOutput += "\t" + string.Format(Asm.OpCodeDecoder[opCode.Value], opCode.Param, opCode.Param2) + (string.IsNullOrEmpty(varName) ? string.Empty : " ;" + varName) + "\n";
				ROMManager.AsmOutput += "\t" + string.Format(Asm.OpRefs.Where(x => x.Byte == opCode.Value).First().Format, opCode.Param, opCode.Param2) /*+ (string.IsNullOrEmpty(varName) ? string.Empty : " ;" + varName)*/ + "\n";
			}
			InitCode();
		}
	}
}
