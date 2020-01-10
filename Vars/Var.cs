using System;
using System.Collections.Generic;
using System.Text;
using static NESSharp.Core.AL;

namespace NESSharp.Core {
	public abstract class IVarAddressArray {
		public RegisterBase OffsetRegister {get;set;}
		public Address[] Address {get;set;}
	}

	//public class VarRegistry : Dictionary<string, Var> {
	//	public new void Add(string key, Var value) {
	//		base.Add(key, value);
	//		DebugFile.WriteVariable(Address[0], name);
	//	}
	//}

	public class Var : IVarAddressArray {
		public string Name;

		public virtual int Length { get; set; } = 1;
		public virtual int Size { get; set; } = 1;

		public virtual Var Dim(RAM ram, string name) => throw new NotSupportedException();
		public virtual Var Copy(Var v) => throw new NotSupportedException();
	}
}
