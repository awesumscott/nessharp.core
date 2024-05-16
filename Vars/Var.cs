using System;
using System.Collections.Generic;

namespace NESSharp.Core;

[VarSize(-1)]
public abstract class Var : IIndexable {
	public string				Name	= string.Empty;
	public IndexingRegister?	Index	{ get; set; }
	public Address[]			Address	{ get; set; }

	//public virtual int			Length	{ get => throw new Exception("Var.Length used"); set => throw new Exception("Var.Length used"); }//	= 1;	//TODO: get rid of Length, so it can be used only for arrays
	public virtual int			Size	{ get; set; }	= 1;

	public virtual Var Dim(RAMRange ram, string name) => throw new NotSupportedException();
	public virtual Var Copy(Var v) => throw new NotSupportedException();
	public virtual Var Copy(IEnumerable<Var> v) => throw new NotSupportedException();

	public static implicit operator Address(Var iva) => iva.Index == null ? iva.Address[0] : iva.Address[0][iva.Index];

	public Address this[int index] {
		get {
			if (index >= 0 && index < Address.Length)
				return Index == null ? Address[index] : Address[index][Index];
			throw new Exception("Index out of range");
		}
	}
}
