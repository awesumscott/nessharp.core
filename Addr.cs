using System;
using System.Linq;
using static NESSharp.Core.AL;

namespace NESSharp.Core {
	public class AddrLo : U8 {
		public Address Address;
		//public AddrLo(byte b) : base(b) {}
		public AddrLo(Address a, byte b) : base(b) {
			Address = a;
		}
		public override string ToString() {
			U16 u16 = Address;
			var match = VarRegistry.Where(x => x.Value.Address.Any(x => x.Hi == u16.Hi && x.Lo == this)).Select(x => new {x.Key, Index = Array.IndexOf(x.Value.Address, Address), HasIndex = x.Value.Address.Length > 1}).FirstOrDefault();

			if (match == null || string.IsNullOrEmpty(match.Key))
				return base.ToString();

			return match.Key + (match.HasIndex ? $"[{ match.Index }]" : "") + ".Lo";
		}
	}
	public class AddrHi : U8 {
		public Address Address;
		//public AddrHi(byte b) : base(b) {}
		public AddrHi(Address a, byte b) : base(b) {
			Address = a;
		}
		public override string ToString() {
			var match = VarRegistry.Where(x => x.Value.Address.Any(x => x.Hi == this && x.Lo == Address.Lo)).Select(x => new {x.Key, Index = Array.IndexOf(x.Value.Address, Address), HasIndex = x.Value.Address.Length > 1}).FirstOrDefault();
			
			if (match == null || string.IsNullOrEmpty(match.Key))
				return base.ToString();

			return match.Key + (match.HasIndex ? $"[{ match.Index }]" : "") + ".Hi";
		}
	}
	public class Address : U16, IOperand<Address>, IOperable<Address> {
		public Address(ushort value) : base(value) {}
		public Address Value => this;

		public override U8 Lo => new AddrLo(this, base.Lo);
		public override U8 Hi => new AddrHi(this, base.Hi);

		public bool IsZP() => Hi == 0;
		public static Address operator ++(Address addr) {
			CPU6502.INC(addr);
			return addr;
		}
		//public static Address operator --(Address addr) {
		//	CPU6502.DEC(addr);
		//	return addr;
		//}
		public Address Set(IOperand operand) {
			if (operand is RegisterA)
				A.STA(this);
			else if (operand is RegisterX)
				CPU6502.STX(this);
			else if (operand is RegisterY)
				CPU6502.STY(this);
			else
				A.Set(operand).STA(this);
			return this;
		}
		public virtual Address Set(U8 u8) => Set((IOperand)u8);
		//public Address Set(Func<Address, IOperand> func) => Set(func.Invoke(this));

		public RegisterA ToA() => A.Set(this);

		public static Address New(ushort value) => new Address(value);
		public static implicit operator Address(ushort s) => new Address(s);
		public static implicit operator ushort(Address p) => (ushort)((p.Hi << 8) + p.Lo);
		public override string ToString() {
			var matchName = VarRegistry.Where(x => x.Value.Address.Any(x => x.Hi == Hi && x.Lo == Lo)).FirstOrDefault().Key;

			if (string.IsNullOrEmpty(matchName))
				return base.ToString();

			var matchVar = VarRegistry[matchName];
			var matchByteInstance = matchVar.Address.Where(x => x.Hi == Hi && x.Lo == Lo).FirstOrDefault(); //necessary instead of "this" because instance refs may be different
			int? index = (matchVar.Address.Length > 1  && matchByteInstance != null) ? matchVar.Address.ToList().IndexOf(matchByteInstance) : (int?)null;
			return matchName + (index!=null ? $"[{index}]" : "");
		}

		public new Address IncrementedValue => New((ushort)((U16)this + 1));

		public Condition Equals(RegisterA _) {
			CPU6502.CMP(this);
			return Condition.EqualsZero;
		}
		public Condition NotEquals(RegisterA a) {
			Equals(a);
			return Condition.NotEqualsZero;
		}

		public AddressIndexed this[IndexingRegister r] => new AddressIndexed(this, r);

		//public static implicit operator Address(ushort s) => new Address(s);
		//public static implicit operator ushort(Address p) => (ushort)((p.Hi << 8) + p.Lo);
	}
	public class AddressIndexed : Address {
		public IndexingRegister? Index = null;
		public AddressIndexed(ushort value, IndexingRegister reg) : base(value) => Index = reg;

		public static implicit operator ushort(AddressIndexed p) => (ushort)((p.Hi << 8) + p.Lo);
	}
}
