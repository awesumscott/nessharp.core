using System;
using System.Collections.Generic;
using System.Text;
using static NESSharp.Core.AL;

namespace NESSharp.Core {
	public abstract class IVarAddressArray {
		public IndexingRegister? Index {get;set;}
		public Address[]? Address {get;set;}
	}

	//public class VarRegistry : Dictionary<string, Var> {
	//	public new void Add(string key, Var value) {
	//		base.Add(key, value);
	//		DebugFile.WriteVariable(Address[0], name);
	//	}
	//}

	[VarSize(-1)]
	public class Var : IVarAddressArray {
		public string Name = string.Empty;

		//public virtual static int Size_New { get; set; } = -1;

		//TODO: get rid of Length
		public virtual int Length { get; set; } = 1;
		public virtual int Size { get; set; } = 1;

		public virtual Var Dim(RAM ram, string name) => throw new NotSupportedException();
		public virtual Var Copy(Var v) => throw new NotSupportedException();
		public virtual Var Copy(IEnumerable<Var> v) => throw new NotSupportedException();
	}
}
