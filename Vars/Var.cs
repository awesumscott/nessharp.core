using System;
using System.Collections.Generic;
using static NESSharp.Core.AL;

namespace NESSharp.Core {
	public abstract class IVarAddressArray {
		public IndexingRegister? Index {get;set;}
		public Address[] Address {get;set;}
		public static implicit operator Address(IVarAddressArray iva) => iva.Index == null ? iva.Address[0] : iva.Address[0][iva.Index];

		public Address this[int index] {
			get {
				if (index >= 0 && index < Address.Length)
					return Index == null ? Address[index] : Address[index][Index];
				throw new Exception("Index out of range");
			}
		}
	}

	[VarSize(-1)]
	public class Var : IVarAddressArray {
		public string Name = string.Empty;

		//TODO: get rid of Length, so it can be used only for arrays
		public virtual int Length { get; set; } = 1;
		public virtual int Size { get; set; } = 1;

		public virtual Var Dim(RAM ram, string name) => throw new NotSupportedException();
		public virtual Var Copy(Var v) => throw new NotSupportedException();
		public virtual Var Copy(IEnumerable<Var> v) => throw new NotSupportedException();
	}
}
