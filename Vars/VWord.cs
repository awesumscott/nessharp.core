using System;
using static NESSharp.Core.AL;

namespace NESSharp.Core {
	public class VWord : VarN {
		public VByte Lo => VByte.Ref(Address[0]);
		public VByte Hi => VByte.Ref(Address[1]);
	
		public static VWord New(RAM ram, string name) {
			return (VWord)new VWord(){Size = 2}.Dim(ram, name);
		}
		public override Var Copy(Var v) {
			if (!(v is VWord))
				throw new Exception("Type must be Var16");
			var v16 = (VWord)v;
			Address = v16.Address;
			Name = v16.Name;
			OffsetRegister = v16.OffsetRegister;
			return v16;
		}
		public VarN Ref(Address addr) {
			Lo.Set(addr.Lo);
			Hi.Set(addr.Hi);
			return this;
		}
		public VarN Ref(VarN vn) {
			if (vn.Size != 2) throw new NotImplementedException();
			Lo.Set(vn.Address[0]);
			Hi.Set(vn.Address[1]);
			return this;
		}
	}
}
