using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static NESSharp.Core.AL;

namespace NESSharp.Core {
	public class AddrLo : U8 {
		public Address Address;
		public AddrLo(byte b) : base(b) {}
		public AddrLo(Address a, byte b) : base(b) {
			Address = a;
		}
		//public static implicit operator AddrHi(byte s) => new AddrHi(s);
		//public static implicit operator byte(AddrHi p) => p.Value;
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
		//public static implicit operator AddrHi(byte s) => new AddrHi(s);
		//public static implicit operator byte(AddrHi p) => p.Value;
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
			if (addr.IsZP())
				Use(Asm.INC.ZeroPage, addr.Lo);
			else
				Use(Asm.INC.Absolute, addr);
			return addr;
		}
		public static Address operator --(Address addr) {
			if (addr.IsZP())
				Use(Asm.DEC.ZeroPage, addr.Lo);
			else
				Use(Asm.DEC.Absolute, addr);
			return addr;
		}
		public virtual Address Set(IResolvable<U8> n) {
			A.Set(n).STA(this);
			return this;
		}
		public virtual Address Set(Address n) {
			A.Set(n).STA(this);
			return this;
		}
		public virtual Address Set(IU8 v) {
			A.Set(v).STA(this);
			return this;
		}
		public virtual Address Set(U8 v) {
			A.Set(v).STA(this);
			return this;
		}
		public virtual Address Set(int v) {
			A.Set((byte)v).STA(this);
			return this;
		}
		public virtual Address Set(RegisterA a) {
			A.STA(this);
			return this;
		}
		public virtual Address Set(RegisterX x) {
			if (IsZP())
				Use(Asm.STX.ZeroPage, Lo);
			else
				Use(Asm.STX.Absolute, this);
			return this;
		}
		public virtual Address Set(RegisterY y) {
			if (IsZP())
				Use(Asm.STY.ZeroPage, Lo);
			else
				Use(Asm.STY.Absolute, this);
			return this;
		}
		public Address Set(Func<Address, RegisterA> func) => Set(func.Invoke(this));
		public virtual Address Set(IPtrIndexed py) => Set(A.Set(py));
		public virtual Address Set(OpLabelIndexed o) {
			A.Set(o).STA(this);
			return this;
		}
		public RegisterA ToA() => A.Set(this);

		public virtual RegisterA SBC(U8 v) => A.Set(this).SBC(v);
		public virtual RegisterA SBC(IU8 v) => A.Set(this).SBC(v);

		[Obsolete]
		public virtual RegisterA BitTest() {
			Use(Asm.BIT.Absolute, this);
			return A;
		}
		
		[Obsolete]
		public virtual Address SetRotateLeft() {
			if (IsZP())
				Use(Asm.ROL.ZeroPage, Lo);
			else
				Use(Asm.ROL.Absolute, this);
			return this;
		}
		[Obsolete]
		public virtual Address SetRotateRight() {
			if (IsZP())
				Use(Asm.ROR.ZeroPage, Lo);
			else
				Use(Asm.ROR.Absolute, this);
			return this;
		}
		public static Address New(ushort value) => new Address(value);
		//public static implicit operator Address(ushort s) => new Address(s);
		public static implicit operator ushort(Address p) => (ushort)((p.Hi << 8) + p.Lo);
		public override string ToString() {
			var matchName = VarRegistry.Where(x => x.Value.Address.Any(x => x.Hi == Hi && x.Lo == Lo)).FirstOrDefault().Key;

			if (string.IsNullOrEmpty(matchName))
				return base.ToString();

			var matchVar = VarRegistry[matchName];
			int? index = matchVar.Address.Count() > 1 ? matchVar.Address.ToList().IndexOf(this) : (int?)null;
			return matchName + (index!=null ? $"[{index}]" : "");
		}

		public new Address IncrementedValue => New((ushort)((U16)this + 1));
		public Condition Equals(RegisterA a) {
			if (IsZP())
				Use(Asm.CMP.ZeroPage, Lo);
			else
				Use(Asm.CMP.Absolute, this);
			return Condition.EqualsZero;
		}
		public Condition NotEquals(RegisterA a) {
			Equals(a);
			return Condition.NotEqualsZero;
		}
		public AddressIndexed this[RegisterBase r] => new AddressIndexed(this, r);
	}
	public class AddressIndexed : Address {
		public RegisterBase Index = null;
		public AddressIndexed(ushort value, RegisterBase reg) : base(value) => Index = reg;

		public static implicit operator ushort(AddressIndexed p) => (ushort)((p.Hi << 8) + p.Lo);
		
		public override Address Set(int v) {
			return Set((U8)v);
		}
		public override Address Set(U8 v) {
			A.Set(v).STA(this);
			return this;
		}
		public override Address Set(RegisterA a) {
			A.STA(this);
			return this;
		}
		public override Address Set(Address n) {
			A.Set(n).STA(this);
			return this;
		}
		public override Address Set(IPtrIndexed p) => Set(A.Set(p));
		public override Address Set(OpLabelIndexed o) {
			A.Set(o).STA(this);
			return this;
		}
		[Obsolete]
		public Address Increment() {
			if (!(Index is RegisterX)) throw new Exception("INC only allowed with X index");
			if (IsZP())
				Use(Asm.INC.ZeroPageX, Lo);
			else
				Use(Asm.INC.AbsoluteX, this);
			return this;
		}
		[Obsolete]
		public Address Decrement() {
			if (!(Index is RegisterX)) throw new Exception("DEC only allowed with X index");
			if (IsZP())
				Use(Asm.DEC.ZeroPageX, Lo);
			else
				Use(Asm.DEC.AbsoluteX, this);
			return this;
		}
		[Obsolete]
		public override Address SetRotateLeft() {
			void rol() {
				if (IsZP())
					Use(Asm.ROL.ZeroPageX, Lo);
				else
					Use(Asm.ROL.AbsoluteX, this);
			}
			if (Index is RegisterX) {// throw new Exception("Must be indexed by X");
				rol();
			} else if (Index is RegisterY) {
				//TODO: remove all lock stuff, this should be done explicitly in user code
				//if (X.IsLocked) {
				//	Temp[0].Set(X); //preserve X
				//	Temp[1].Set(Y);
				//	X.Set(Temp[1]); //X = Y
				//} else { //use tmp to preserve A
					Temp[0].Set(Y);
					X.Set(Temp[0]);
				//}

				rol();
					
				//if (X.IsLocked) {
				//	X.Set(Temp[0]); //restore X
				//}
			}
			return this;
		}
		[Obsolete]
		public override Address SetRotateRight() {
			void ror() {
				if (IsZP())
					Use(Asm.ROR.ZeroPageX, Lo);
				else
					Use(Asm.ROR.AbsoluteX, this);
			}
			if (Index is RegisterX) {// throw new Exception("Must be indexed by X");
				ror();
			} else if (Index is RegisterY) {
				//if (X.IsLocked) {
				//	Temp[0].Set(X); //preserve X
				//	Temp[1].Set(Y);
				//	X.Set(Temp[1]); //X = Y
				//} else { //use tmp to preserve A
					Temp[0].Set(Y);
					X.Set(Temp[0]);
				//}
				
				ror();
					
				//if (X.IsLocked) {
				//	X.Set(Temp[0]); //restore X
				//}
			}
			return this;
		}
	}

	
	//public class AddressFuture : Address {
	//	private LabelRef _labelRef;
	//	public AddressFuture(ushort value) : base(value) {}

	//	//private Address Resolve() {
	//	//	return Labels[_labelRef.ID].Address;
	//	//}

	//	public override U8 Lo { get => base.Lo; }
	//	public override U8 Hi { get => base.Hi; }
	//}
}
