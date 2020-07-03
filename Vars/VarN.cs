using System;
using System.Collections.Generic;
using System.Linq;
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

		public override Var Copy(Var v) {
			if (!(v is VarN))
				throw new Exception("Type must be derived from VarN");
			var vn = (VarN)v;
			Size = vn.Size;
			Address = vn.Address;
			Name = vn.Name;
			Index = vn.Index;
			return this;
		}

		public override Var Copy(IEnumerable<Var> v) {
			//if (!(v is VarN))
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

		public VarN Set(Func<VarN, object> func) => Set(func.Invoke(this));
		public VarN Set(object o) {
			if (o is U8 u8) {
				if (Index == null) {
					Address[0].Set(u8);
					foreach (var addr in Address.Skip(1))
						addr.Set(0);
				} else {
					Address[0][Index].Set(u8);
					foreach (var addr in Address.Skip(1))
						addr[Index].Set(0);
				}
			} else if (o is int i32) {
				var b = (byte)i32;
				if (b != i32) {
					//throw new ArgumentOutOfRangeException();
					
					if (Index == null) {
						Address[1].Set((i32 - b) >> 8);
						Address[0].Set(b);
					} else {
						Address[1][Index].Set((i32 - b) >> 8);
						Address[0][Index].Set(b);
					}
				} else
					Set((U8)b);
			} else if (o is Address addr) {
				if (Index == null) {
					Address[0].Set(addr);
					foreach (var a in Address.Skip(1))
						a.Set(0);
				} else {
					Address[0][Index].Set(addr);
					foreach (var a in Address.Skip(1))
						a[Index].Set(0);
				}
			} else if (o is IVarAddressArray iva) {
				var srcLen = iva.Address.Length;
				if (srcLen > Size) throw new Exception("Source var length is greater than destination var length");
				for (var i = 0; i < Size; i++) {
					if (i < srcLen) {
						if (Index == null) {
							Address[i].Set(iva.Address[i]);
						} else {
							Address[i][Index].Set(iva.Address[i]);
						}
					} else {
						if (Index == null) {
							Address[i].Set(0);
						} else {
							Address[i][Index].Set(0);
						}
					}
				}
			} else if (o is IEnumerable<RegisterA> aList) {
				var i = 0;
				foreach(var v in aList) {
					if (i >= Address.Length) throw new Exception("Source var length is greater than destination var length");
					if (Index == null) {
						Address[i++].Set(v);
					} else {
						Address[i++][Index].Set(v);
					}
				}
				for (; i < Address.Length; i++) {
					if (Index == null) {
						Address[i].Set(0);
					} else {
						Address[i][Index].Set(0);
					}
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
			Func<int, RegisterA> operandLhs = index => Index == null ? Address[index].ToA() : Address[index][Index].ToA();
			Func<int, object> operandRhs = index => iva.Index == null ? iva.Address[index] : iva.Address[index][Index];
			var srcLen = iva.Address.Length;
			if (srcLen > Size) throw new Exception("Source var length is greater than destination var length");
			yield return operandLhs(0).Add(operandRhs(0));
			for (var i = 1; i < Size; i++) {
				yield return operandLhs(i).ADC(i < srcLen ? operandRhs(i) : 0);
			}
			yield break;
		}
		public IEnumerable<RegisterA> Add(U8 u8) {
			//var srcLen = iva.Address.Length;
			//if (srcLen > Size) throw new Exception("Source var length is greater than destination var length");
			Func<int, RegisterA> operandLhs = index => Index == null ? Address[index].ToA() : Address[index][Index].ToA();

			yield return operandLhs(0).Add(u8);
			for (var i = 1; i < Size; i++) {
				yield return operandLhs(i).ADC(0);
			}
			yield break;
		}
		public IEnumerable<RegisterA> Add(RegisterA a) {
			//Func<int, RegisterA> operandLhs = index => Index == null ? Address[index].ToA() : Address[index][Index].ToA();
			Func<int, object> operandRhs = index => Index == null ? Address[index] : Address[index][Index];
			//var srcLen = iva.Address.Length;
			//if (srcLen > Size) throw new Exception("Source var length is greater than destination var length");
			yield return A.Add(operandRhs(0));
			for (var i = 1; i < Size; i++) {
				if (Index == null)
					yield return Address[i].ToA().ADC(0);
				else
					yield return Address[i][Index].ToA().ADC(0);
			}
			yield break;
		}
		public IEnumerable<RegisterA> Subtract(IVarAddressArray iva) {
			Func<int, RegisterA> operandLhs = index => Index == null ? Address[index].ToA() : Address[index][Index].ToA();
			Func<int, object> operandRhs = index => iva.Index == null ? iva.Address[index] : iva.Address[index][Index];
			var srcLen = iva.Address.Length;
			if (srcLen > Size) throw new Exception("Source var length is greater than destination var length");
			yield return operandLhs(0).Subtract(operandRhs(0));
			for (var i = 1; i < Size; i++) {
				yield return operandLhs(i).Subtract(i < srcLen ? operandRhs(i) : 0);
			}
			yield break;
		}
		public IEnumerable<RegisterA> Subtract(U8 u8) {
			Func<int, RegisterA> operandLhs = index => Index == null ? Address[index].ToA() : Address[index][Index].ToA();
			//var srcLen = iva.Address.Length;
			//if (srcLen > Size) throw new Exception("Source var length is greater than destination var length");
			yield return operandLhs(0).Subtract(u8);
			for (var i = 1; i < Size; i++) {
				yield return operandLhs(i).SBC(0);
			}
			yield break;
		}
		public VByte this[U8 index] {
			get {
				if (index >= 0 && index < Size) {
					if (Index == null) {
						return VByte.Ref(Address[index]);
					} else {
						return VByte.Ref(Address[index][Index]);
					}
				}
				throw new IndexOutOfRangeException();
			}
		}

		
		public Condition NotEquals(U8 v) {
			if (v == 0) {
				//fast way to check != 0
				if (Index == null) {
					A.Set(Address[0]);
				} else {
					A.Set(Address[0][Index]);
				}
				for (var i = 1; i < Size; i++) {
					if (Index == null) {
						A.Or(Address[i]);
					} else {
						A.Or(Address[i][Index]);
					}
				}
				return A.NotEquals(0);
			}
			//TODO: Any(Address[0].NotEquals(0), Address[1].NotEquals(v))
			throw new NotImplementedException();
		}
	}
}
