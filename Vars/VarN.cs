using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static NESSharp.Core.AL;

namespace NESSharp.Core {
	public class VarN : Var {
		public override int Size {get;set;}
		public override Var Dim(RAM ram, string name) {
			if (Address != null) throw new Exception("Var already dimmed");
			Address = ram.Dim(Size);
			Name = name;
			DebugFile.WriteVariable(ram, Address[0], Address[Size - 1], name);
			VarRegistry.Add(name, this);
			return this;
		}
		
		public static VarN New(RAM ram, int len, string name) {
			return (VarN)new VarN(){Size = len}.Dim(ram, name);
		}
		public static VarN Ref(Address addr, ushort len) {
			var v = new VarN();
			v.Address = Enumerable.Range(addr, len).Select(x => Addr((U16)x)).ToArray();
			return v;
		}
		public VByte this[int index] {
			get {
				if (index >= 0 && index < Address.Length)
					return VByte.Ref(Address[index]);
				throw new Exception("Index out of range");
			}
		}

		int[] Slice(int start, int length) { 
			var slice = new int[length];
			//Array.Copy(_array, start, slice, 0, length);
			return slice;
		}

		public VarN Set(Func<VarN, object> func) => Set(func.Invoke(this));
		public VarN Set(object o) {
			if (o is U8 u8) {
				Address[0].Set(u8);
				foreach (var addr in Address.Skip(1))
					addr.Set(0);
			} else if (o is int i32) {
				var b = (byte)i32;
				if (b != i32) {
					//throw new ArgumentOutOfRangeException();
					Address[1].Set((i32 - b) >> 8);
					Address[0].Set(b);
				} else
					Set((U8)b);
			} else if (o is Address addr) {
				Address[0].Set(addr);
				foreach (var a in Address.Skip(1))
					a.Set(0);
			} else if (o is IVarAddressArray iva) {
				var srcLen = iva.Address.Length;
				if (srcLen > Size) throw new Exception("Source var length is greater than destination var length");
				for (var i = 0; i < Size; i++) {
					if (i < srcLen)
						Address[i].Set(iva.Address[i]);
					else
						Address[i].Set(0);
				}
			} else if (o is IEnumerable<RegisterA> aList) {
				
				var i = 0;
				foreach(var v in aList) {
					if (i >= Address.Length) throw new Exception("Source var length is greater than destination var length");
					Address[i++].Set(v);
				}
				for (; i < Address.Length; i++) {
					Address[i].Set(0);
				}
			} else throw new Exception("Type not supported by VarN: " + o.GetType().ToString());
			return this;
		}
		//public VarN SetAdd(object o) {
		//	if (o is U8 u8) {
		//		Address[0].Set(z => z.ToA().Add(u8));
		//		foreach (var addr in Address.Skip(1))
		//			addr.ToA().ADC(0);
		//	} else if (o is IVarAddressArray iva) {
		//		var srcLen = iva.Address.Length;
		//		if (srcLen > Size) throw new Exception("Source var length is greater than destination var length");
		//		Address[0].Set(z => z.ToA().Add(iva.Address[0]));
		//		for (var i = 1; i < Size; i++) {
		//			if (i < srcLen)
		//				Address[i].Set(z => z.ToA().ADC(iva.Address[i]));
		//			else
		//				Address[i].Set(z => z.ToA().ADC(0));
		//		}
		//	}
		//	return this;
		//}
		public IEnumerable<RegisterA> Add(IVarAddressArray iva) {
			var srcLen = iva.Address.Length;
			if (srcLen > Size) throw new Exception("Source var length is greater than destination var length");
			yield return Address[0].ToA().Add(iva.Address[0]);
			for (var i = 1; i < Size; i++) {
				if (i < srcLen)
					yield return Address[i].ToA().ADC(iva.Address[i]);
				yield return Address[i].ToA().ADC(0);
			}
			yield break;
		}
		public IEnumerable<RegisterA> Add(U8 u8) {
			//var srcLen = iva.Address.Length;
			//if (srcLen > Size) throw new Exception("Source var length is greater than destination var length");
			yield return Address[0].ToA().Add(u8);
			for (var i = 1; i < Size; i++) {
				yield return Address[i].ToA().ADC(0);
			}
			yield break;
		}
		public IEnumerable<RegisterA> Add(RegisterA a) {
			//var srcLen = iva.Address.Length;
			//if (srcLen > Size) throw new Exception("Source var length is greater than destination var length");
			yield return A.Add(Address[0]);
			for (var i = 1; i < Size; i++) {
				yield return Address[i].ToA().ADC(0);
			}
			yield break;
		}
		public IEnumerable<RegisterA> Subtract(IVarAddressArray iva) {
			var srcLen = iva.Address.Length;
			if (srcLen > Size) throw new Exception("Source var length is greater than destination var length");
			yield return Address[0].ToA().Subtract(iva.Address[0]);
			for (var i = 1; i < Size; i++) {
				if (i < srcLen)
					yield return Address[i].ToA().SBC(iva.Address[i]);
				else
					yield return Address[i].ToA().SBC(0);
			}
			yield break;
		}
		public IEnumerable<RegisterA> Subtract(U8 u8) {
			//var srcLen = iva.Address.Length;
			//if (srcLen > Size) throw new Exception("Source var length is greater than destination var length");
			yield return Address[0].ToA().Subtract(u8);
			for (var i = 1; i < Size; i++) {
				yield return Address[i].ToA().SBC(0);
			}
			yield break;
		}
		public VByte this[U8 index] {
			get {
				if (index >= 0 && index < Size)
					return VByte.Ref(Address[index]);
				throw new IndexOutOfRangeException();
			}
		}

		
		public Condition NotEquals(U8 v) {
			if (v == 0) {
				//fast way to check != 0
				A.Set(Address[0]);
				for (var i = 1; i < Size; i++) {
					A.Or(Address[i]);
				}
				return A.NotEquals(0);
			}
			//TODO: Any(Address[0].NotEquals(0), Address[1].NotEquals(v))
			throw new NotImplementedException();
		}
	}
}
