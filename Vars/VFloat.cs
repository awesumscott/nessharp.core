using System;
using System.Collections.Generic;
using System.Text;
using static NESSharp.Core.AL;

namespace NESSharp.Core {
	public class VFloat : VarN {
		private VarN _int, _frac;
		private ushort _intLen, _fracLen;
		public VarN Integer => _int;
		public VarN Fractional => _frac;
		public override Var Dim(RAM ram, string name) {
			base.Dim(ram, name);
			_int = VarN.Ref(Address[_fracLen], _intLen);
			_frac = VarN.Ref(Address[0], _fracLen);
			return this;
		}
		public static VFloat New(RAM ram, ushort intLen, ushort fracLen, string name) {
			return (VFloat)new VFloat(){Size = intLen + fracLen, _intLen = intLen, _fracLen = fracLen}.Dim(ram, name);
		}
	}
}
