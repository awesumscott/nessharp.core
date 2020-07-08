using System;
using System.Linq;
using static NESSharp.Core.AL;

namespace NESSharp.Core {
	[VarSize(2)]
	public class VWord : VarN {
		//public override int Size_New { get; set; } = 2;
		public VByte Lo => VByte.Ref(Address[0], Index);
		public VByte Hi => VByte.Ref(Address[1], Index);

		public VWord() {
			Size = 2;
		}

		public static VWord New(RAM ram, string name) {
			return (VWord)new VWord(){Size = 2}.Dim(ram, name);
		}
		public override Var Copy(Var v) {
			if (!(v is VWord))
				throw new Exception("Type must be Var16");
			var v16 = (VWord)v;
			Address = v16.Address;
			Name = v16.Name;
			Index = v16.Index;
			return v16;
		}

		/// <summary>
		/// "Refer To". Make this variable a pointer to a particular memory address.
		/// </summary>
		public VWord Ref(Address addr) {
			if (addr is AddressIndexed ai)
				Index = ai.Index;
			Lo.Set(addr.Lo);
			Hi.Set(addr.Hi);
			return this;
		}
		/// <summary>
		/// "Refer To". Make this variable a pointer to a particular memory address.
		/// </summary>
		public VWord Ref(VarN vn) {
			if (vn.Size != 2) throw new NotImplementedException();
			Lo.Set(vn[0]);
			Hi.Set(vn[1]);
			return this;
		}
		//public static VWord Ref(VByte b1, VByte b2) {
		//	return new VWord {
		//		Address = new Address[] { b1, b2 }
		//	};
		//}

		//This format is best to ensure addresses are sequential
		public static new VWord Ref(Address addr, ushort len) {
			//return (VWord)VarN.Ref(addr, len);
			var v = new VWord();
			v.Address = Enumerable.Range(addr, len).Select(x => Addr((U16)x)).ToArray();
			//v.Index = new index param? if needed
			return v;
		}
	}
}
