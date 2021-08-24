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
			var offset = 0;
			foreach(var op in Context.Operations) {
				AsmWithRefs.Add(op);

				//Determine label addresses by keeping a count of bytes to be output
				if (op is Label label)			label.Address = AL.Addr((U16)(Origin + offset));
				else if (op is OpRaw raw)		offset += raw.Length;
				else if (op is OpCode opCode)	offset += opCode.Length;
			}
			Context.InitCode();
		}

		public void Include(Type classType) => Core.Include.Module(classType);
		public void Include<T>(T obj) => Core.Include.Module(obj);
		public void IncludeAsm(string fileName) => Core.Include.Asm(fileName);
		public void IncludeFile(string fileName) => Core.Include.File(fileName);
	}
}
