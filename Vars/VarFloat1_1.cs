using System;
using System.Collections.Generic;
using System.Text;
using static NESSharp.Core.AL;

namespace NESSharp.Core {
	public class VarFloat1_1 : VarN {
		private Var8 _int, _frac;
		public Var8 Integer => _int;
		public Var8 Fractional => _frac;
		public override Var Dim(RAM ram, string name) {
			base.Dim(ram, name);
			_int = Var8.Ref(Address[1]);
			_frac = Var8.Ref(Address[0]);
			return this;
		}
		public static VarFloat1_1 New(RAM ram, string name) {
			return (VarFloat1_1)new VarFloat1_1(){Size = 2}.Dim(ram, name);
		}
	}
}
