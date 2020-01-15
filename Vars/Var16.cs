using System;
using static NESSharp.Core.AL;

namespace NESSharp.Core {
	public class Var16 : VarN {
		public Var8 Lo => Var8.Ref(Address[0]);
		public Var8 Hi => Var8.Ref(Address[1]);
	
		public static Var16 New(RAM ram, string name) {
			return (Var16)new Var16(){Size = 2}.Dim(ram, name);
		}
		public override Var Copy(Var v) {
			if (!(v is Var16))
				throw new Exception("Type must be Var16");
			var v16 = (Var16)v;
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
