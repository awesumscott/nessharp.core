using System;
using System.Linq;
using static NESSharp.Core.AL;

namespace NESSharp.Core {
	[VarSize(2)]
	public class VWord : VarN {
		public VByte Lo => VByte.Ref(Address[0], Index, $"{Name}_Lo");
		public VByte Hi => VByte.Ref(Address[1], Index, $"{Name}_Hi");

		public VWord() {
			Size = 2;
		}

		public static VWord New(RAMRange ram, string name) => (VWord)new VWord().Dim(ram, name);
		public override VarN Copy(Var v) {
			if (v is not VWord)
				throw new Exception("Type must be Var16");
			var v16 = (VWord)v;
			Address = v16.Address;
			Name = v16.Name;
			Index = v16.Index;
			return v16;
		}

		/// <summary>
		/// Make this variable a pointer to a particular memory address.
		/// </summary>
		public VWord PointTo(Address addr) {
			if (addr is AddressIndexed ai)
				Index = ai.Index;
			Lo.Set(addr.Lo);
			Hi.Set(addr.Hi);
			return this;
		}
		public VWord PointTo(Label lbl) {
			//if (lbl is LabelIndexed li)
			//	Index = li.Index;
			Lo.Set(lbl.Lo());
			Hi.Set(lbl.Hi());
			return this;
		}
		/// <summary>
		/// Make this variable a pointer to a particular memory address.
		/// </summary>
		public VWord PointTo(VarN vn) {
			if (vn.Size != 2) throw new Exception("Value must have a size of 2 bytes");
			Lo.Set(vn[0]);
			Hi.Set(vn[1]);
			return this;
		}
		public void PointTo(Action a) {
			PointTo(LabelFor(a)); //TODO: check attribute with an IsSubroutine func?
		}

		//This format is best to ensure addresses are sequential
		public static new VWord Ref(Address addr, ushort len, string name) {
			//return (VWord)VarN.Ref(addr, len);
			var v = new VWord {
				Address = Enumerable.Range(addr, len).Select(x => Addr((U16)x)).ToArray(),
				Name = name
			};
			//v.Index = new index param? if needed
			return v;
		}
	}
}
