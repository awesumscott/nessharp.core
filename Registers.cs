using System;
using System.Collections.Generic;
using static NESSharp.Core.AL;

namespace NESSharp.Core {
	public abstract class RegisterBase {
		public byte? Number;
		public UniquenessState State = new UniquenessState();

		public void Reset() {
			Number = null;
		}
	}
	public abstract class IndexingRegister : RegisterBase {}
	public class RegisterX : IndexingRegister {
		public RegisterX Set(U8 v) {
			if (Number != null && Number == v.Value) return this;
			return Set((object)v);
		}
		//public RegisterX Set(int v) {
		//	byte b = (byte)v; //throw an exception if it's out of range
		//	if (b != v)
		//		throw new ArgumentOutOfRangeException();
		//	return Set((object)b);
		//}
		public RegisterX Set(object o) {
			Number = null;
			if (o is RegisterA)
				CPU6502.TAX();
			else if (o is RegisterY) {
				CPU6502.TYA();
				CPU6502.TAX();
			} else if (o is int i) {
				byte b = (byte)i; //throw an exception if it's out of range
				if (b != i)
					throw new ArgumentOutOfRangeException();
				CPU6502.LDX(b);
			} else
				CPU6502.LDX(o);
			return this;
		}
		public static RegisterX operator ++(RegisterX x) {
			if (x.Number != null)
				x.Number++;
			CPU6502.INX();
			return x;
		}
		public static RegisterX operator --(RegisterX x) {
			if (x.Number != null)
				x.Number--;
			CPU6502.DEX();
			return x;
		}
		//public void CPX(object o) {
		//	CPU6502.CPX(o);
		//}
		public Condition Equals(U8 v) {
			//TODO: CPX if X wasn't last used register
			//if (v != 0)
				CPU6502.CPX(v);
			return Condition.EqualsZero;
		}
		public Condition NotEquals(U8 v) {
			Equals(v);
			return Condition.NotEqualsZero;
		}
		public new Condition Equals(object o) {
			CPU6502.CPX(o);
			return Condition.EqualsZero;
		}
		public Condition NotEquals(object o) {
			Equals(o);
			return Condition.NotEqualsZero;
		}
		public Condition IsPositive() => Condition.IsPositive;
		public Condition IsNegative() => Condition.IsNegative;
		public Condition LessThan(U8 v) {
			CPU6502.CPX(v);
			return Condition.IsLessThan;
		}
	}
	public class RegisterY : IndexingRegister {
		//TODO: consolidate these Set()'s even further in Set(object) using type testing
		public RegisterY Set(U8 v) {
			if (Number != null && Number == v.Value) return this;
			Number = v.Value;
			return LDY(v);
		}
		//public RegisterY Set(int v) {
		//	byte b = (byte)v; //throw an exception if it's out of range
		//	if (b != v)
		//		throw new ArgumentOutOfRangeException();
		//	return LDY(v);
		//}
		public RegisterY Set(object o) {
			Number = null;
			return LDY(o);
		}

		public RegisterY Set(RegisterA a) {
			Number = a.Number;
			return LDY(a);
		}
		public RegisterY LDY(object o) {
			if (o is RegisterA)
				CPU6502.TAY();
			else if (o is RegisterX) {
				CPU6502.TXA();
				CPU6502.TAY();
			} else if (o is int i) {
				byte b = (byte)i; //throw an exception if it's out of range
				if (b != i)
					throw new ArgumentOutOfRangeException();
				CPU6502.LDY(b);
			} else
				CPU6502.LDY(o);
			return this;
		}
		//public RegisterY Set(IResolvable<Address> ra) {
		//	Use(Asm.LDY.Absolute, ra); //TODO: see if this will be used, and if it'll be correct
		//	return this;
		//}

		public static RegisterY operator ++(RegisterY y) {
			if (y.Number != null)
				y.Number++;
			CPU6502.INY();
			return y;
		}
		public static RegisterY operator --(RegisterY y) {
			if (y.Number != null)
				y.Number--;
			CPU6502.DEY();
			return y;
		}
		public Condition Equals(U8 v) {
			//TODO: CPY if X wasn't last used register
			if (v == 0)
				return Condition.EqualsZero;
			throw new NotImplementedException();
			//TODO: CPY in here
		}
		public Condition NotEquals(U8 v) {
			//TODO: CMP if Y wasn't last used register
			if (v == 0)
				return Condition.NotEqualsZero;
			CPU6502.CPY(v);
			return Condition.NotEqualsZero;
			throw new NotImplementedException();
		}
		public Condition Equals(Address addr) {
			throw new NotImplementedException();
			CPU6502.CMP(addr);
			return Condition.EqualsZero;
		}
		public Condition NotEquals(Address addr) {
			//throw new NotImplementedException();
			CPU6502.CPY(addr);
			return Condition.NotEqualsZero;
		}
		public Condition LessThan(object o) {
			CPU6502.CPY(o);
			return Condition.IsLessThan;
		}
	}
	public class RegisterA : RegisterBase {
		public RegisterA Set(U8 u8) {
			if (Number != null && Number == u8.Value) return this;
			Number = u8;
			return Set((object)u8);
		}
		public RegisterA Set(object o) {
			if (!(o is U8))
				Number = null;
			if (o is RegisterX)
				CPU6502.TXA();
			else if (o is RegisterY)
				CPU6502.TYA();
			else
				CPU6502.LDA(o);
			return this;
		}

		public RegisterA Add(object o) {
			Carry.Clear();
			if (o is RegisterX) {
				Temp[0].Set(X);
				ADC(Temp[0]);
			} else if (o is RegisterY) {
				Temp[0].Set(Y);
				ADC(Temp[0]);
			} else
				ADC(o);
			return this;
		}
		public RegisterA Subtract(object o) {
			Carry.Set();
			if (o is RegisterX) {
				Temp[0].Set(X);
				SBC(Temp[0]);
			} else if (o is RegisterY) {
				Temp[0].Set(Y);
				SBC(Temp[0]);
			} else
				SBC(o);
			return this;
		}
		public RegisterA And(object o) {
			Number = null;
			CPU6502.AND(o);
			return this;
		}
		public RegisterA Or(object o) {
			Number = null;
			CPU6502.ORA(o);
			return this;
		}
		public RegisterA Xor(object o) {
			Number = null;
			CPU6502.EOR(o);
			return this;
		}




		public RegisterA ADC(object o) {
			Number = null;
			CPU6502.ADC(o);
			return this;
		}
		public RegisterA BIT(object o) {
			Number = null;
			CPU6502.BIT(o);
			return this;
		}
		public RegisterA SBC(object o) {
			Number = null;
			CPU6502.SBC(o);
			return this;
		}
		public RegisterA STA(object o) {
			//Helper function for chaining A operations
			CPU6502.STA(o);
			return this;
		}
		public RegisterA LogicalShiftRight() {
			Number = null;
			CPU6502.LSR(this);
			return this;
		}
		public RegisterA ArithmeticShiftLeft() {
			Number = null;
			CPU6502.ASL(this);
			return this;
		}
		public RegisterA ROR() {
			Number = null;
			CPU6502.ROR(this);
			return this;
		}
		public RegisterA ROL() {
			Number = null;
			CPU6502.ROL(this);
			return this;
		}
		public void CMP(object o) {
			CPU6502.CMP(o);
		}
		public Condition Equals(U8 v) {
			if (v != 0)
				CMP(v);
			return Condition.EqualsZero;
			//throw new Exception("NYI");
			//TODO: CMP in here
		}
		public Condition NotEquals(U8 v) {
			if (v != 0)
				CMP(v);
			return Condition.NotEqualsZero;
			throw new Exception("NYI");
		}
		public Condition Equals(Address addr) {
			CMP(addr);
			return Condition.EqualsZero;
		}
		public Condition NotEquals(Address addr) {
			Equals(addr);
			return Condition.NotEqualsZero;
		}
		public Condition Equals(Ptr p) {
			CMP(p);
			return Condition.EqualsZero;
		}
		public Condition NotEquals(Ptr p) {
			Equals(p);
			return Condition.NotEqualsZero;
		}
		public Condition IsPositive() => Condition.IsPositive;
		public Condition IsNegative() => Condition.IsNegative;
		public Condition GreaterThan(U8 v) {
			return GreaterThan((IU8)v);
		}
		public Condition GreaterThan(IU8 v) {
			Temp[0].Set(A);
			A.Set(v).CMP(Temp[0]);
			return Condition.IsGreaterThan;
		}
		public Condition GreaterThanOrEqualTo(U8 v) {
			return GreaterThanOrEqualTo((IU8)v);
		}
		public Condition GreaterThanOrEqualTo(IU8 v) {
			CMP(v);
			return Condition.IsGreaterThanOrEqualTo;
		}
		//TODO: fix and test this one:
		public Condition GreaterThanOrEqualTo(Func<RegisterA> a) {
			Temp[0].Set(A);
			Temp[1].Set(a.Invoke());
			A.Set(Temp[0]).CMP(Temp[1]);
			return Condition.IsGreaterThanOrEqualTo;
		}
		public Condition LessThan(Func<RegisterA> a) {
			Temp[0].Set(A);
			Temp[1].Set(a.Invoke());
			A.Set(Temp[0]).CMP(Temp[1]);
			return Condition.IsLessThan;
		}
		public Condition LessThan(U8 v) {
			CMP(v);
			return Condition.IsLessThan;
		}
		public Condition LessThan(Address v) {
			CMP(v);
			return Condition.IsLessThan;
		}
		public Condition LessThanOrEqualTo(U8 v) {
			GreaterThan(v);
			return Condition.IsLessThanOrEqualTo;
		}
		public Condition LessThanOrEqualTo(IU8 v) {
			GreaterThan(v);
			return Condition.IsLessThanOrEqualTo;
		}
	}
}
