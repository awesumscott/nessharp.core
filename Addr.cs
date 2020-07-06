using System;
using System.Linq;
using static NESSharp.Core.AL;

namespace NESSharp.Core {
	public class AddrLo : U8 {
		public Address Address;
		public AddrLo(byte b) : base(b) {}
		public AddrLo(Address a, byte b) : base(b) {
			Address = a;
		}
		public override string ToString() {
			U16 u16 = Address;
			var match = VarRegistry.Where(x => x.Value.Address.Any(x => x.Hi == u16.Hi && x.Lo == this)).Select(x => new {Key = x.Key, Index = System.Array.IndexOf(x.Value.Address, Address), HasIndex = x.Value.Address.Length > 1}).FirstOrDefault();

			if (match == null || string.IsNullOrEmpty(match.Key))
				return base.ToString();

			return match.Key + (match.HasIndex ? $"[{ match.Index }]" : "") + ".Lo";



			//var loMatch = VarRegistry.Where(x => x.Value.Address.Any(x => x.Hi == Address.Lo.Hi && x.Lo == Lo.Lo)).FirstOrDefault().Key;
			//var hiMatch = VarRegistry.Where(x => x.Value.Address.Any(x => x.Hi == Address.Hi.Hi && x.Lo == Hi.Lo)).FirstOrDefault().Key;

			//if (string.IsNullOrEmpty(loMatch) && string.IsNullOrEmpty(hiMatch))
			//	return base.ToString();
			////if (VarRegistry[match].Address.ToList().IndexOf())
			//if (string.IsNullOrEmpty(loMatch))
			//	return hiMatch + "[1].Lo";
			//return loMatch + "[0].Lo";
		}
	}
	public class AddrHi : U8 {
		public Address Address;
		public AddrHi(byte b) : base(b) {}
		public AddrHi(Address a, byte b) : base(b) {
			Address = a;
		}
		public override string ToString() {
			var match = VarRegistry.Where(x => x.Value.Address.Any(x => x.Hi == this && x.Lo == Address.Lo)).Select(x => new {Key = x.Key, Index = System.Array.IndexOf(x.Value.Address, Address), HasIndex = x.Value.Address.Length > 1}).FirstOrDefault();
			
			if (match == null || string.IsNullOrEmpty(match.Key))
				return base.ToString();

			return match.Key + (match.HasIndex ? $"[{ match.Index }]" : "") + ".Hi";
		}
	}
	public class Address : U16, IU8 {

		public Address(ushort value) : base(value) {}

		public override U8 Lo {
			get => new AddrLo(this, base.Lo);
		}

		public override U8 Hi {
			get => new AddrHi(this, base.Hi);
		}

		public bool IsZP() => Hi == 0;
		public static Address operator ++(Address addr) {
			CPU6502.INC(addr);
			return addr;
		}
		//public static Address operator --(Address addr) {
		//	CPU6502.DEC(addr);
		//	return addr;
		//}
		public virtual Address Set(object o) {
			//if (o is int i)
			//	A.Set((byte)i).STA(this);
			//else 
			if (o is RegisterA)
				A.STA(this);
			else if (o is RegisterX)
				CPU6502.STX(this);
			else if (o is RegisterY)
				CPU6502.STY(this);
			else if (o is Func<Address, object> func)
				Set(func.Invoke(this));
			else if (o is IPtrIndexed py)
				Set(A.Set(py));
			else
				A.Set(o).STA(this);
			return this;
		}
		public Address Set(Func<Address, object> func) => Set(func.Invoke(this));
		//public virtual Address Set(IPtrIndexed py) => Set(A.Set(py));
		//public virtual Address Set(OpLabelIndexed o) {
		//	A.Set(o).STA(this);
		//	return this;
		//}
		public RegisterA ToA() => A.Set(this);

		public static Address New(ushort value) => new Address(value);
		//public static implicit operator Address(ushort s) => new Address(s);
		public static implicit operator ushort(Address p) => (ushort)((p.Hi << 8) + p.Lo);
		public override string ToString() {
			var matchName = VarRegistry.Where(x => x.Value.Address.Any(x => x.Hi == Hi && x.Lo == Lo)).FirstOrDefault().Key;

			if (string.IsNullOrEmpty(matchName))
				return base.ToString();

			var matchVar = VarRegistry[matchName];
			var matchByteInstance = matchVar.Address.Where(x => x.Hi == Hi && x.Lo == Lo).FirstOrDefault(); //necessary instead of "this" because instance refs may be different
			int? index = (matchVar.Address.Count() > 1  && matchByteInstance != null) ? matchVar.Address.ToList().IndexOf(matchByteInstance) : (int?)null;
			return matchName + (index!=null ? $"[{index}]" : "");
		}

		public new Address IncrementedValue => New((ushort)((U16)this + 1));
		public Condition Equals(RegisterA a) {
			CPU6502.CMP(this);
			return Condition.EqualsZero;
		}
		public Condition NotEquals(RegisterA a) {
			Equals(a);
			return Condition.NotEqualsZero;
		}
		public AddressIndexed this[IndexingRegister r] => new AddressIndexed(this, r);
	}
	public class AddressIndexed : Address {
		public IndexingRegister Index = null;
		public AddressIndexed(ushort value, IndexingRegister reg) : base(value) => Index = reg;

		public static implicit operator ushort(AddressIndexed p) => (ushort)((p.Hi << 8) + p.Lo);
	}
}
