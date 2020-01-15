using System;
using System.Collections.Generic;
using System.Text;
using static NESSharp.Core.AL;

namespace NESSharp.Core {
	public class Var8 : Var, IU8 {
		public override int Size => 1;

		public Var8() {}
		public static implicit operator Address(Var8 v) => v.OffsetRegister == null ? v.Address[0] : v.Address[0][v.OffsetRegister];

		public override Var Dim(RAM ram, string name) {
			if (Address != null) throw new Exception("Var already dimmed");
			Address = new Address[]{ ram.Dim() };
			Name = name;
			DebugFile.WriteVariable(Address[0], name);
			VarRegistry.Add(name, this);
			return this;
		}
		public static Var8 New(RAM ram, string name) {
			return (Var8)new Var8().Dim(ram, name);
		}
		public static Var8 Ref(Address addr) {
			var v = new Var8();
			v.Address = new Address[]{ addr };
			return v;
		}
		public override Var Copy(Var v) {
			if (!(v is Var8))
				throw new Exception("Type must be Var8");
			var v8 = (Var8)v;
			Address = v8.Address;
			Name = v8.Name;
			OffsetRegister = v8.OffsetRegister;
			return v8;
		}
		//This and the two commented-out lines below wouldn't use the overrides for AddressIndexed, idk why just yet
		//private Address getVar8Addr(Var8 v) {
		//	if (v.OffsetRegister == null)
		//		return v.Address[0];
		//	return v.Address[0].Offset(v.OffsetRegister);
		//}
		public Var8 Set(Var8 v) {
			if (OffsetRegister == null) {
				//Address[0].Set(getVar8Addr(v));
				if (v.OffsetRegister == null)
					Address[0].Set(v.Address[0]);
				else
					Address[0].Set(v.Address[0][v.OffsetRegister]);
			} else {
				if (v.OffsetRegister == null)
					Address[0][OffsetRegister].Set(v.Address[0]);
				else
					Address[0][OffsetRegister].Set(v.Address[0][v.OffsetRegister]);
			}
			return this;
		}
		public Var8 Set(Address addr) {
			if (OffsetRegister == null)
				Address[0].Set(addr);
			else
				Address[0][OffsetRegister].Set(addr);
			return this;
		}
		public Var8 Set(IResolvable<U8> v) {
			if (OffsetRegister == null)
				Address[0].Set(v);
			else
				Address[0][OffsetRegister].Set(v);
			return this;
		}
		public Var8 Set(OpLabelIndexed oli) {
			if (OffsetRegister == null)
				Address[0].Set(oli);
			else
				Address[0][OffsetRegister].Set(oli);
			return this;
		}
		public Var8 Set(IPtrIndexed p) {
			if (OffsetRegister == null)
				Address[0].Set(A.Set(p));
			else {
				if (OffsetRegister is RegisterX)
					Address[0][OffsetRegister].Set(A.Set(p));
				else throw new Exception("NYI for var8[Y] = [ptr],y -- possible with stack backup");
			}
			return this;
		}
		public Var8 Set(RegisterA a) {
			if (OffsetRegister == null)
				Address[0].Set(a);
			else
				Address[0][OffsetRegister].Set(a);
			return this;
		}
		public Var8 Set(RegisterX a) {
			if (OffsetRegister == null)
				Address[0].Set(a);
			else if (OffsetRegister is RegisterY)
				Address[0][Y].Set(X);
			else throw new NotImplementedException(); //do some swapping to preserve X if this is worth it
			return this;
		}
		public Var8 Set(RegisterY a) {
			if (OffsetRegister == null)
				Address[0].Set(a);
			else if (OffsetRegister is RegisterX)
				Address[0][X].Set(Y);
			else throw new NotImplementedException(); //do some swapping to preserve Y if this is worth it
			return this;
		}
		public Var8 Set(U8 v) {
			if (OffsetRegister == null)
				Address[0].Set(v);
			else
				Address[0][OffsetRegister].Set(v);
			return this;
		}
		public Var8 Set(Func<Var8, RegisterA> func) => Set(func.Invoke(this));
		public RegisterA Add(U8 v) {
			Carry.Clear();
			if (OffsetRegister == null)
				return Address[0].ToA().ADC(v);
			return Address[0][OffsetRegister].ToA().ADC(v);
		}
		public RegisterA Add(IU8 v) {
			Carry.Clear();
			if (OffsetRegister == null)
				return Address[0].ToA().ADC(v);
			return Address[0][OffsetRegister].ToA().ADC(v);
		}
		public RegisterA Add(RegisterA a) {
			Carry.Clear();
			if (OffsetRegister == null)
				return A.Add(Address[0]);
			return A.Add(Address[0][OffsetRegister]);
		}
		public RegisterA Add(OpLabelIndexed oli) {
			Carry.Clear();
			if (OffsetRegister == null)
				return Address[0].ToA().ADC(oli);
			return Address[0][OffsetRegister].ToA().ADC(oli);
		}
		public RegisterA Subtract(U8 v) {
			Carry.Set();
			if (OffsetRegister == null)
				return Address[0].SBC(v);
			return Address[0][OffsetRegister].SBC(v);
		}
		public RegisterA Subtract(IU8 v) {
			Carry.Set();
			if (OffsetRegister == null)
				return Address[0].SBC(v);
			return Address[0][OffsetRegister].SBC(v);
		}
		public RegisterA Subtract(RegisterA a) {
			Temp[0].Set(a);
			if (OffsetRegister == null)
				A.Set(Address[0]);
			else
				A.Set(Address[0][OffsetRegister]);
			return A.Subtract(Temp[0]);
		}
		public virtual RegisterA And(U8 v) {
			if (OffsetRegister == null)
				return A.Set(Address[0]).And(v);
			return A.Set(Address[0][OffsetRegister]).And(v);
		}
		public virtual RegisterA And(OpLabelIndexed oli) {
			if (OffsetRegister == null)
				return A.Set(Address[0]).And(oli);
			return A.Set(Address[0][OffsetRegister]).And(oli);
		}
		public virtual RegisterA Or(RegisterA a) {
			Temp[0].Set(A);
			if (OffsetRegister == null) {
				return A.Set(Address[0]).Or(Temp[0]);
			}
			return A.Set(Address[0][OffsetRegister]).Or(Temp[0]);
		}
		public virtual RegisterA Or(U8 v) {
			if (OffsetRegister == null)
				return A.Set(Address[0]).Or(v);
			return A.Set(Address[0][OffsetRegister]).Or(v);
		}
		public virtual RegisterA Or(IU8 v) {
			if (OffsetRegister == null)
				return A.Set(Address[0]).Or(v);
			return A.Set(Address[0][OffsetRegister]).Or(v);
		}
		public virtual RegisterA Xor(U8 v) {
			if (OffsetRegister == null)
				return A.Set(Address[0]).Xor(v);
			return A.Set(Address[0][OffsetRegister]).Xor(v);
		}
		[Obsolete]
		public virtual Var8 SetRotateLeft() {
			if (OffsetRegister == null)
				Address[0].SetRotateLeft();
			else
				Address[0][OffsetRegister].SetRotateLeft();
			return this;
		}
		[Obsolete]
		public virtual Var8 SetRotateRight() {
			if (OffsetRegister == null)
				Address[0].SetRotateRight();
			else
				Address[0][OffsetRegister].SetRotateRight();
			return this;
		}
		public Condition Equals(U8 v) {
			if (OffsetRegister == null)
				A.Set(Address[0]);//.Equals(v);
			else
				A.Set(Address[0][OffsetRegister]);
			if (v != 0)
				A.CMP(v);
			return Condition.EqualsZero;
		}
		public Condition Equals(Var8 v) {
			if (OffsetRegister == null)
				A.Set(Address[0]);//.Equals(v);
			else
				A.Set(Address[0][OffsetRegister]);
			A.CMP(v);
			return Condition.EqualsZero;
		}
		public Condition Equals(RegisterA a) => Address[0].Equals(a);
		public Condition NotEquals(U8 v) {
			Equals(v);
			return Condition.NotEqualsZero;
		}
		public Condition NotEquals(Var8 v) {
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
		public Condition GreaterThan(Var8 v) {
			if (OffsetRegister != null) throw new NotImplementedException();
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
			if (OffsetRegister != null)
				A.Set(Address[0][OffsetRegister]);
			else
				A.Set(Address[0]);
			A.CMP(v);
			return Condition.IsGreaterThanOrEqualTo;
		}
		public Condition GreaterThanOrEqualTo(Var8 v) {
			if (OffsetRegister != null)
				A.Set(Address[0][OffsetRegister]);
			else
				A.Set(Address[0]);
			if (v.OffsetRegister != null)
				A.CMP(v);
			return Condition.IsGreaterThanOrEqualTo;
		}
		public Condition LessThan(U8 v) {
			GreaterThanOrEqualTo(v);
			return Condition.IsLessThan;
		}
		public Condition LessThan(Var8 v) {
			GreaterThanOrEqualTo(v);
			return Condition.IsLessThan;
		}
		public Condition LessThanOrEqualTo(U8 v) {
			GreaterThan(v);
			return Condition.IsLessThanOrEqualTo;
		}
		public Condition LessThanOrEqualTo(Var8 v) {
			GreaterThan(v);
			return Condition.IsLessThanOrEqualTo;
		}
		public Condition LessThanOrEqualTo(RegisterA a) {
			GreaterThan(a);
			return Condition.IsLessThanOrEqualTo;
		}



		public static Var8 operator ++(Var8 addr) => addr.Increment();
		public Var8 Increment() {
			if (OffsetRegister == null)
				Address[0]++;
			else
				Address[0][OffsetRegister].Increment();
			return this;
		}
		public Var8 Decrement() {
			if (OffsetRegister == null)
				Address[0]--;
			else
				Address[0][OffsetRegister].Decrement();
			return this;
		}
		public static Var8 operator --(Var8 addr) {
			addr.Address[0]--;
			return addr;
		}

		public AddressIndexed this[RegisterBase r] => Address[0][r];
	}
}
