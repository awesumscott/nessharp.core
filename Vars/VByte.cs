using System;
using static NESSharp.Core.AL;

namespace NESSharp.Core {
	public class VByte : Var, IU8 {
		public override int Size => 1;

		public VByte() {}
		public static implicit operator Address(VByte v) => v.Index == null ? v.Address[0] : v.Address[0][v.Index];

		public override Var Dim(RAM ram, string name) {
			if (Address != null) throw new Exception("Var already dimmed");
			Address = ram.Dim(1);
			Name = name;
			DebugFile.WriteVariable(ram, Address[0], name);
			VarRegistry.Add(name, this);
			return this;
		}
		public static VByte New(RAM ram, string name) {
			return (VByte)new VByte().Dim(ram, name);
		}
		public static VByte Ref(Address addr) {
			var v = new VByte();
			v.Address = new Address[]{ addr };
			return v;
		}
		public override Var Copy(Var v) {
			if (!(v is VByte))
				throw new Exception("Type must be Var8");
			var v8 = (VByte)v;
			Address = v8.Address;
			Name = v8.Name;
			Index = v8.Index;
			return v8;
		}
		//This and the two commented-out lines below wouldn't use the overrides for AddressIndexed, idk why just yet
		//private Address getVar8Addr(Var8 v) {
		//	if (v.OffsetRegister == null)
		//		return v.Address[0];
		//	return v.Address[0].Offset(v.OffsetRegister);
		//}
		public VByte Set(VByte v) {
			if (Index == null) {
				//Address[0].Set(getVar8Addr(v));
				if (v.Index == null)
					Address[0].Set(v.Address[0]);
				else
					Address[0].Set(v.Address[0][v.Index]);
			} else {
				if (v.Index == null)
					Address[0][Index].Set(v.Address[0]);
				else
					Address[0][Index].Set(v.Address[0][v.Index]);
			}
			return this;
		}
		public VByte Set(Address addr) {
			if (Index == null)
				Address[0].Set(addr);
			else
				Address[0][Index].Set(addr);
			return this;
		}
		public VByte Set(IResolvable<U8> v) {
			if (Index == null)
				Address[0].Set(v);
			else
				Address[0][Index].Set(v);
			return this;
		}
		public VByte Set(OpLabelIndexed oli) {
			if (Index == null)
				Address[0].Set(oli);
			else
				Address[0][Index].Set(oli);
			return this;
		}
		public VByte Set(IPtrIndexed p) {
			if (Index == null)
				Address[0].Set(A.Set(p));
			else {
				if (Index is RegisterX)
					Address[0][Index].Set(A.Set(p));
				else throw new Exception("NYI for var8[Y] = [ptr],y -- possible with stack backup");
			}
			return this;
		}
		public VByte Set(RegisterA a) {
			if (Index == null)
				Address[0].Set(a);
			else
				Address[0][Index].Set(a);
			return this;
		}
		public VByte Set(IndexingRegisterBase reg) {
			if (reg is RegisterX)
				return Set(X);
			//else if (reg is RegisterY)
			return Set(Y);
		}
		public VByte Set(RegisterX x) {
			if (Index == null)
				Address[0].Set(x);
			else if (Index is RegisterY)
				Address[0][Y].Set(X);
			else throw new NotImplementedException(); //do some swapping to preserve X if this is worth it
			return this;
		}
		public VByte Set(RegisterY y) {
			if (Index == null)
				Address[0].Set(y);
			else if (Index is RegisterX)
				Address[0][X].Set(Y);
			else throw new NotImplementedException(); //do some swapping to preserve Y if this is worth it
			return this;
		}
		public VByte Set(U8 v) {
			if (Index == null)
				Address[0].Set(v);
			else
				Address[0][Index].Set(v);
			return this;
		}
		public VByte Set(Func<VByte, RegisterA> func) => Set(func.Invoke(this));
		public RegisterA Add(U8 v) {
			Carry.Clear();
			if (Index == null)
				return Address[0].ToA().ADC(v);
			return Address[0][Index].ToA().ADC(v);
		}
		public RegisterA Add(IU8 v) {
			Carry.Clear();
			if (Index == null)
				return Address[0].ToA().ADC(v);
			return Address[0][Index].ToA().ADC(v);
		}
		public RegisterA Add(RegisterA a) {
			Carry.Clear();
			if (Index == null)
				return A.Add(Address[0]);
			return A.Add(Address[0][Index]);
		}
		public RegisterA Add(OpLabelIndexed oli) {
			Carry.Clear();
			if (Index == null)
				return Address[0].ToA().ADC(oli);
			return Address[0][Index].ToA().ADC(oli);
		}
		public RegisterA Subtract(U8 v) {
			Carry.Set();
			if (Index == null)
				return Address[0].ToA().SBC(v);
			return Address[0][Index].ToA().SBC(v);
		}
		public RegisterA Subtract(IU8 v) {
			Carry.Set();
			if (Index == null)
				return Address[0].ToA().SBC(v);
			return Address[0][Index].ToA().SBC(v);
		}
		public RegisterA Subtract(RegisterA a) {
			Temp[0].Set(a);
			if (Index == null)
				A.Set(Address[0]);
			else
				A.Set(Address[0][Index]);
			return A.Subtract(Temp[0]);
		}
		public virtual RegisterA And(U8 v) {
			if (Index == null)
				return A.Set(Address[0]).And(v);
			return A.Set(Address[0][Index]).And(v);
		}
		public virtual RegisterA And(OpLabelIndexed oli) {
			if (Index == null)
				return A.Set(Address[0]).And(oli);
			return A.Set(Address[0][Index]).And(oli);
		}
		public virtual RegisterA Or(RegisterA a) {
			Temp[0].Set(A);
			if (Index == null) {
				return A.Set(Address[0]).Or(Temp[0]);
			}
			return A.Set(Address[0][Index]).Or(Temp[0]);
		}
		public virtual RegisterA Or(U8 v) {
			if (Index == null)
				return A.Set(Address[0]).Or(v);
			return A.Set(Address[0][Index]).Or(v);
		}
		public virtual RegisterA Or(IU8 v) {
			if (Index == null)
				return A.Set(Address[0]).Or(v);
			return A.Set(Address[0][Index]).Or(v);
		}
		public virtual RegisterA Xor(U8 v) {
			if (Index == null)
				return A.Set(Address[0]).Xor(v);
			return A.Set(Address[0][Index]).Xor(v);
		}
		public virtual VByte SetROL() {
			if (Index == null)
				CPU6502.ROL(Address[0]);
			else if (Index is RegisterX) {
				CPU6502.ROL(Address[0][Index]);
			} else if (Index is RegisterY) {
				Temp[0].Set(Y);
				X.Set(Temp[0]);
				Index = X;
				CPU6502.ROL(Address[0][Index]);
				Index = Y;
			}
			return this;
		}
		public virtual VByte SetROR() {
			if (Index == null)
				CPU6502.ROR(Address[0]);
			else if (Index is RegisterX) {
				CPU6502.ROR(Address[0][Index]);
			} else if (Index is RegisterY) {
				Temp[0].Set(Y);
				X.Set(Temp[0]);
				Index = X;
				CPU6502.ROR(Address[0][Index]);
				Index = Y;
			}
			return this;
		}
		public Condition Equals(U8 v) {
			if (Index == null)
				A.Set(Address[0]);//.Equals(v);
			else
				A.Set(Address[0][Index]);
			if (v != 0)
				A.CMP(v);
			return Condition.EqualsZero;
		}
		public Condition Equals(VByte v) {
			if (Index == null)
				A.Set(Address[0]);//.Equals(v);
			else
				A.Set(Address[0][Index]);
			A.CMP(v);
			return Condition.EqualsZero;
		}
		public Condition Equals(RegisterA a) => Address[0].Equals(a);
		public Condition NotEquals(U8 v) {
			Equals(v);
			return Condition.NotEqualsZero;
		}
		public Condition NotEquals(VByte v) {
			Equals(v);
			return Condition.NotEqualsZero;
		}
		public Condition NotEquals(RegisterA a) => Address[0].NotEquals(a);

		
		public Condition GreaterThan(U8 v) {
			Temp[0].Set(Address[0]);
			A.Set(v);
			A.CMP(Temp[0]);
			return Condition.IsGreaterThan;
		}
		public Condition GreaterThan(VByte v) {
			if (Index != null) throw new NotImplementedException();
			Temp[0].Set(Address[0]);
			A.Set(v);
			A.CMP(Temp[0]);
			return Condition.IsGreaterThan;
		}
		public Condition GreaterThan(RegisterA a) {
			Temp[1].Set(A);
			Temp[0].Set(Address[0]);
			A.Set(Temp[1]).CMP(Temp[0]);
			return Condition.IsGreaterThan;
		}
		public Condition GreaterThanOrEqualTo(U8 v) {
			if (Index != null)
				A.Set(Address[0][Index]);
			else
				A.Set(Address[0]);
			A.CMP(v);
			return Condition.IsGreaterThanOrEqualTo;
		}
		public Condition GreaterThanOrEqualTo(VByte v) {
			if (Index != null)
				A.Set(Address[0][Index]);
			else
				A.Set(Address[0]);
			//if (v.OffsetRegister != null)
				A.CMP(v);
			return Condition.IsGreaterThanOrEqualTo;
		}
		public Condition LessThan(U8 v) {
			GreaterThanOrEqualTo(v);
			return Condition.IsLessThan;
		}
		public Condition LessThan(VByte v) {
			GreaterThanOrEqualTo(v);
			return Condition.IsLessThan;
		}
		public Condition LessThanOrEqualTo(U8 v) {
			GreaterThan(v);
			return Condition.IsLessThanOrEqualTo;
		}
		public Condition LessThanOrEqualTo(VByte v) {
			GreaterThan(v);
			return Condition.IsLessThanOrEqualTo;
		}
		public Condition LessThanOrEqualTo(RegisterA a) {
			GreaterThan(a);
			return Condition.IsLessThanOrEqualTo;
		}



		public static VByte operator ++(VByte addr) => addr.Increment();
		public VByte Increment() {
			if (Index == null)
				CPU6502.INC(Address[0]);
			else
				CPU6502.INC(Address[0][Index]);
			return this;
		}
		public VByte Decrement() {
			if (Index == null)
				CPU6502.DEC(Address[0]);
			else
				CPU6502.DEC(Address[0][Index]);
			return this;
		}
		public static VByte operator --(VByte addr) {
			CPU6502.DEC(addr.Address[0]);
			return addr;
		}

		public AddressIndexed this[IndexingRegisterBase r] => Address[0][r];
	}
}
