﻿using System;
using System.Collections.Generic;
using System.Linq;
using static NESSharp.Core.CPU6502;

namespace NESSharp.Core;

public class VarN : Var {
	public override int Size {get;set;}
	public override VarN Dim(RAMRange ram, string name) {
		if (Address != null) throw new Exception("Var already dimmed");
		Address = ram.Dim(Size);
		Name = name;
		DebugFileNESASM.WriteVariable(ram, Address[0], Address[Size - 1], name);
		AL.VarRegistry.Add(name, this);
		return this;
	}

	public static VarN New(RAMRange ram, int len, string name) => new VarN() { Size = len }.Dim(ram, name);
	public static VarN Ref(Address addr, ushort len, string name) => new() {
		Address = Enumerable.Range(addr, len).Select(x => AL.Addr((U16)x)).ToArray(),
		Name = name
	};

	int[] Slice(int start, int length) { 
		var slice = new int[length];
		//Array.Copy(_array, start, slice, 0, length);
		return slice;
	}

	public override VarN Copy(Var v) {
		if (v is not VarN)
			throw new Exception("Type must be derived from VarN");
		var vn = (VarN)v;
		Size = vn.Size;
		Address = vn.Address;
		Name = vn.Name;
		Index = vn.Index;
		return this;
	}

	public override VarN Copy(IEnumerable<Var> v) {
		//if (v is not VarN)
		//	throw new Exception("Type must be derived from VarN");
		//var vns = v.Select(x => (VarN)x).ToList();
		var vns = v.ToList();
		Size = vns.Select(x => x.Size).Sum();
		Address = vns.SelectMany(x => x.Address).ToArray();
		Name = vns[0].Name;//.Substring(0, vns[0].Name.LastIndexOf('_'));
		//Name = Name.Substring(0, Name.LastIndexOf('_'));
		Index = vns[0].Index;
		return this;
	}
	
	public new VByte this[int index] {
		get {
			if (index >= 0 && index < Address.Length)
				return VByte.Ref(Index == null ? Address[index] : Address[index][Index], Index, $"{Name}__{index}");
			throw new Exception("Index out of range");
		}
	}

	public VarN Set(Func<VarN, object> func) => Set(func.Invoke(this));
	public VarN Set(object o) {
		if (o is IOperand u8) {
			this[0].Set(u8);
			for (var i = 1; i < Address.Length; i++)
				this[i].Set(0);
		} else if (o is int i32) {
			var b = (byte)i32;
			if (b != i32) {
				//throw new ArgumentOutOfRangeException();
				
				this[1].Set((i32 - b) >> 8);
				this[0].Set(b);
			} else
				Set((U8)b);
		} else if (o is Address addr) {
			this[0].Set(addr);
			for (var i = 1; i < Address.Length; i++)
				this[i].Set(0);
		} else if (o is Var iva) {
			var srcLen = iva.Address.Length;
			if (srcLen > Size) throw new Exception("Source var length is greater than destination var length");
			for (var i = 0; i < Size; i++) {
				if (i < srcLen) {
					this[i].Set(VByte.Ref(iva[i], iva.Index, $"{iva.Name}__{i}"));
				} else {
					this[i].Set(0);
				}
			}
		} else if (o is IEnumerable<RegisterA> aList) {
			var i = 0;
			foreach(var v in aList) {
				if (i >= Address.Length) throw new Exception("Source var length is greater than destination var length");
				this[i++].Set(v);
			}
			for (; i < Address.Length; i++) {
				this[i].Set(0);
			}
		} else throw new Exception("Type not supported by VarN: " + o.GetType().ToString());
		return this;
	}
	//TODO: combine this with IOperand to avoid specifying/casting from caller
	public IEnumerable<RegisterA> Add(VarN vn) {
		var srcLen = vn.Address.Length;
		if (srcLen > Size) throw new Exception("Source var length is greater than destination var length");
		yield return A.Set(this[0]).Add(vn[0]);
		for (var i = 1; i < Size; i++) {
			if (i < srcLen)
				yield return A.Set(this[i]).ADC(vn[i]);
			else
				yield return A.Set(this[i]).ADC(0);
		}
		yield break;
	}
	public IEnumerable<RegisterA> Add(U8 v) => Add((IOperand)v);
	public IEnumerable<RegisterA> Add(IOperand v) {
		//if (v is IVarAddressArray iva) {
		//	var srcLen = iva.Address.Length;
		//	if (srcLen > Size) throw new Exception("Source var length is greater than destination var length");
		//	yield return A.Set(this[0]).Add(iva[0]);
		//	for (var i = 1; i < Size; i++) {
		//		if (i < srcLen)
		//			yield return A.Set(this[i]).ADC(iva[i]);
		//		else
		//			yield return A.Set(this[i]).ADC(0);
		//	}
		//	yield break;
		//}

		if (v is RegisterA)	yield return A.Add(this[0]);
		else				yield return A.Set(this[0]).Add(v);	//Possible optimization:
		//this Else could test if A's last value is this[0] or v, and prefer one order over the other to remove a redundant LDA
		for (var i = 1; i < Size; i++)
			yield return A.Set(this[i]).ADC(0);
		yield break;
	}
	//public IEnumerable<RegisterA> Add(RegisterA a) {
	//	yield return A.Add(this[0]);
	//	for (var i = 1; i < Size; i++) {
	//		yield return A.Set(this[i]).ADC(0);
	//	}
	//	yield break;
	//}
	public IEnumerable<RegisterA> Subtract(VarN vn) {
		var srcLen = vn.Address.Length;
		if (srcLen > Size) throw new Exception("Source var length is greater than destination var length");
		yield return A.Set(this[0]).Subtract(vn[0]);
		for (var i = 1; i < Size; i++) {
			if (i < srcLen)
				yield return A.Set(this[i]).SBC(vn[i]);
			else
				yield return A.Set(this[i]).SBC(0);
		}
		yield break;
	}
	public IEnumerable<RegisterA> Subtract(U8 v) => Subtract((IOperand)v);
	public IEnumerable<RegisterA> Subtract(IOperand u8) {
		yield return A.Set(this[0]).Subtract(u8);
		for (var i = 1; i < Size; i++)
			yield return A.Set(this[i]).SBC(0);
		yield break;
	}

	public Condition NotEquals(U8 v) {
		if (v == 0) { //Optimization: fast way to check != 0
			A.Set(this[0]);
			for (var i = 1; i < Size; i++)
				A.Or(this[i]);
			return A.NotEquals(0);
		}
		//TODO: Any(Address[0].NotEquals(0), Address[1].NotEquals(v))
		throw new NotImplementedException();
	}
}
