using System;
using System.Collections.Generic;
using System.Text;
using static NESSharp.Core.AL;

namespace NESSharp.Core {
	[VarSize(2)]
	public class VDecimal_1_1 : VarN {
		//public override int Size_New { get; set; } = 2;
		public VByte Integer => VByte.Ref(Address[1], Index); //{get; protected set;} => VByte.Ref(Address[1]); // => _int;
		public VByte Fractional => VByte.Ref(Address[0], Index); // {get; protected set;}// => _frac;
		public override VDecimal_1_1 Dim(RAM ram, string name) {
			Size = 2;
			base.Dim(ram, name);
			//Integer = VByte.Ref(Address[1]);
			//Fractional = VByte.Ref(Address[0]);
			return this;
		}
		public static VDecimal_1_1 New(RAM ram, string name) => new VDecimal_1_1().Dim(ram, name);
	}
}
