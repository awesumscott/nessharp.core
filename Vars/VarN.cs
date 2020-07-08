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
				this[0].Set(u8);
				for (var i = 1; i < Address.Length; i++)
					this[i].Set(0);
			} else if (o is int i32) {
				var b = (byte)i32;
				if (b != i32) {
					//throw new ArgumentOutOfRangeException();
					
					this[1].Set((U8)((i32 - b) >> 8));
					this[0].Set(b);
				} else
					Set((U8)b);
			} else if (o is Address addr) {
				this[0].Set(addr);
				for (var i = 1; i < Address.Length; i++)
					this[i].Set(0);
			} else if (o is IVarAddressArray iva) {
				var srcLen = iva.Address.Length;
				if (srcLen > Size) throw new Exception("Source var length is greater than destination var length");
				for (var i = 0; i < Size; i++) {
					if (i < srcLen) {
						this[i].Set(iva[i]);
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
		public IEnumerable<RegisterA> Add(IVarAddressArray iva) {
			var srcLen = iva.Address.Length;
			if (srcLen > Size) throw new Exception("Source var length is greater than destination var length");
			yield return A.Set(this[0]).Add(iva[0]);
			for (var i = 1; i < Size; i++) {
				if (i < srcLen)
					yield return A.Set(this[i]).ADC(iva[i]);
				else
					yield return A.Set(this[i]).ADC(0);
			}
			yield break;
		}
		public IEnumerable<RegisterA> Add(U8 u8) {
			//var srcLen = iva.Address.Length;
			//if (srcLen > Size) throw new Exception("Source var length is greater than destination var length");
			yield return A.Set(this[0]).Add(u8);
			for (var i = 1; i < Size; i++) {
				yield return A.Set(this[i]).ADC(0);
			}
			yield break;
		}
		public IEnumerable<RegisterA> Add(RegisterA a) {
			//var srcLen = iva.Address.Length;
			//if (srcLen > Size) throw new Exception("Source var length is greater than destination var length");
			yield return A.Add(this[0]);
			for (var i = 1; i < Size; i++) {
				yield return A.Set(this[i]).ADC(0);
			}
			yield break;
		}
		public IEnumerable<RegisterA> Subtract(IVarAddressArray iva) {
			var srcLen = iva.Address.Length;
			if (srcLen > Size) throw new Exception("Source var length is greater than destination var length");
			yield return A.Set(this[0]).Subtract(iva[0]);
			for (var i = 1; i < Size; i++) {
				if (i < srcLen)
					yield return A.Set(this[i]).Subtract(iva[i]);
				else
					yield return A.Set(this[i]).Subtract(0);
			}
			yield break;
		}
		public IEnumerable<RegisterA> Subtract(U8 u8) {
			//var srcLen = iva.Address.Length;
			//if (srcLen > Size) throw new Exception("Source var length is greater than destination var length");
			yield return A.Set(this[0]).Subtract(u8);
			for (var i = 1; i < Size; i++) {
				yield return A.Set(this[i]).SBC(0);
			}
			yield break;
		}

		public Condition NotEquals(U8 v) {
			if (v == 0) {
				//fast way to check != 0
				A.Set(this[0]);
				for (var i = 1; i < Size; i++) {
					A.Or(this[i]);
				}
				return A.NotEquals(0);
			}
			//TODO: Any(Address[0].NotEquals(0), Address[1].NotEquals(v))
			throw new NotImplementedException();
		}
	}
}
