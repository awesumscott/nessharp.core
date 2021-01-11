using System;
using static NESSharp.Core.AL;

namespace NESSharp.Core {
	public abstract class RegisterBase {
		//public byte? Number;
		public IOperand? LastLoaded = null;
		/// <summary>
		/// Reference to the var to which this register was last stored
		/// </summary>
		public object? LastStored = null;
		public long LastStoredHash = -1;
		public long LastStoredFlagN = -1;
		public long LastStoredFlagZ = -1;
		public UniquenessState State = new UniquenessState();

		public void Reset() {
			//Number = null;
			LastStored = null;
			LastStoredHash = -1;
		}
	}
	//TODO: figure out how to get this to work with IOperand
	public abstract class IndexingRegister : RegisterBase, IOperand<IndexingRegister> {
		public IndexingRegister Value => this;

		public static IndexingRegister operator ++(IndexingRegister reg) {
			if (reg is RegisterX)	X++;
			else					Y++;
			return reg;
		}
		public static IndexingRegister operator --(IndexingRegister reg) {
			if (reg is RegisterX)	X--;
			else					Y--;
			return reg;
		}
	}
	public class RegisterX : IndexingRegister, IOperand<RegisterX> {
		public new RegisterX Value => this;
		public override string ToString() => "X";

		public RegisterX Set(IOperand o) {
			if (o is RegisterA)
				CPU6502.TAX();
			else if (o is RegisterY) {
				CPU6502.TYA();
				CPU6502.TAX();
			} else
				CPU6502.LDX(o);
			return this;
		}
		public RegisterX Set(U8 v) => Set((IOperand)v);

		public static RegisterX operator ++(RegisterX x) {
			CPU6502.INX();
			return x;
		}
		public static RegisterX operator --(RegisterX x) {
			CPU6502.DEX();
			return x;
		}
		public Condition Equals(IOperand o) {
			CPU6502.CPX(o);
			return Condition.EqualsZero;
		}
		public Condition Equals(U8 v) {
			if (v != 0 || Flags.Zero.LastReg != this)
				CPU6502.CPX(v);
			return Condition.EqualsZero;
		}
		public Condition NotEquals(U8 v) {
			if (v != 0 || Flags.Zero.LastReg != this)
				CPU6502.CPX(v);
			return Condition.NotEqualsZero;
		}
		public Condition NotEquals(IOperand o) {
			CPU6502.CPX(o);
			return Condition.NotEqualsZero;
		}

		public Condition IsPositive() => Condition.IsPositive;
		public Condition IsNegative() => Condition.IsNegative;
		public Condition LessThan(U8 u8) => LessThan((IOperand)u8);
		public Condition LessThan(IOperand v) {
			CPU6502.CPX(v);
			return Condition.IsLessThan;
		}
	}
	public class RegisterY : IndexingRegister, IOperand<RegisterY> {
		public new RegisterY Value => this;
		public override string ToString() => "Y";

		public RegisterY Set(U8 v) => LDY(v);
		public RegisterY Set(IOperand o) => LDY(o);
		public RegisterY LDY(IOperand o) {
			if (o is RegisterA)
				CPU6502.TAY();
			else if (o is RegisterX) {
				CPU6502.TXA();
				CPU6502.TAY();
			} else
				CPU6502.LDY(o);
			return this;
		}

		public static RegisterY operator ++(RegisterY y) {
			CPU6502.INY();
			return y;
		}
		public static RegisterY operator --(RegisterY y) {
			CPU6502.DEY();
			return y;
		}
		public Condition Equals(U8 v) {
			if (v != 0 || Flags.Zero.LastReg != this)
				CPU6502.CPY(v);
			return Condition.EqualsZero;
		}
		public Condition NotEquals(U8 v) {
			if (v != 0 || Flags.Zero.LastReg != this)
				CPU6502.CPY(v);
			return Condition.NotEqualsZero;
		}
		public Condition Equals(IOperand addr) {
			CPU6502.CPY(addr);
			return Condition.EqualsZero;
		}
		public Condition NotEquals(IOperand addr) {
			CPU6502.CPY(addr);
			return Condition.NotEqualsZero;
		}
		public Condition LessThan(IOperand o) {
			CPU6502.CPY(o);
			return Condition.IsLessThan;
		}
	}
	public class RegisterA : RegisterBase, IOperand<RegisterA>, IOperable<RegisterA> {
		public RegisterA Value => this;
		public RegisterA Set(IOperand operand) {
			if (operand is RegisterA)		return this; //do nothing, this should be okay to support IOperands this way //throw new Exception("Attempting to set A to A");
			else if (operand is RegisterX)	CPU6502.TXA();
			else if (operand is RegisterY)	CPU6502.TYA();
			else							CPU6502.LDA(operand);
			return this;
		}
		//TODO: figure out how to handle indexing reg's. They should never get into GenericAssembler, but they should also act as operands for higher level stuff.
		//Vars shouldn't have to specify a second Set function just for specifying IndexingRegister params.
		public RegisterA Set(IndexingRegister u8) => Set((IOperand)u8);
		public RegisterA Set(U8 u8) => Set((IOperand)u8);

		public RegisterA Add(IOperand o) {
			Carry.Clear();
			if (o is RegisterA)
				throw new Exception("Attempting to add A to A");
			else if (o is RegisterX) {
				Temp[0].Set(X);
				ADC(Temp[0]);
			} else if (o is RegisterY) {
				Temp[0].Set(Y);
				ADC(Temp[0]);
			} else
				ADC(o);
			return this;
		}
		public RegisterA Add(U8 u8) => Add((IOperand)u8);

		public RegisterA Subtract(IOperand o) {
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
		public RegisterA Subtract(U8 o) => Subtract((IOperand)o);

		public RegisterA And(IOperand o) {			CPU6502.AND(o);		return this; }
		public RegisterA And(U8 o) => And((IOperand)o);
		public RegisterA Or(IOperand o) {			CPU6502.ORA(o);		return this; }
		public RegisterA Or(U8 o) => Or((IOperand)o);
		public RegisterA Xor(IOperand o) {			CPU6502.EOR(o);		return this; }
		public RegisterA Xor(U8 o) => Xor((IOperand)o);

		public RegisterA ADC(IOperand o) {			CPU6502.ADC(o);		return this; }
		public RegisterA ADC(U8 o) => ADC((IOperand)o);
		public RegisterA BIT(IOperand o) {			CPU6502.BIT(o);		return this; }
		public RegisterA BIT(U8 o) => BIT((IOperand)o);
		public RegisterA SBC(IOperand o) {			CPU6502.SBC(o);		return this; }
		public RegisterA SBC(U8 o) => SBC((IOperand)o);
		public RegisterA STA(IOperand o) {			CPU6502.STA(o);		return this; }
		public RegisterA STA(U8 o) => STA((IOperand)o);
		public RegisterA LSR() {					CPU6502.LSR(this);	return this; }
		public RegisterA ASL() {					CPU6502.ASL(this);	return this; }
		public RegisterA ROR() {					CPU6502.ROR(this);	return this; }
		public RegisterA ROL() {					CPU6502.ROL(this);	return this; }
		public void CMP(IOperand o) => CPU6502.CMP(o);

		public Condition Equals(IOperand addr) {
			CMP(addr);
			return Condition.EqualsZero;
		}
		public Condition Equals(U8 v) {
			if (v != 0 || Flags.Zero.LastReg != this)
				CMP(v);
			return Condition.EqualsZero;
		}
		public Condition NotEquals(IOperand addr) {
			CMP(addr);
			return Condition.NotEqualsZero;
		}
		public Condition NotEquals(U8 v) {
			if (v != 0 || Flags.Zero.LastReg != this)
				CMP(v);
			return Condition.NotEqualsZero;
		}
		//TODO: check LastReg for these
		public Condition IsPositive() => Condition.IsPositive;
		public Condition IsNegative() => Condition.IsNegative;
		public Condition GreaterThan(U8 v) {
			Temp[0].Set(A);
			A.Set(v).CMP(Temp[0]);
			return Condition.IsGreaterThan;
		}
		public Condition GreaterThan(IOperand v) {
			Temp[0].Set(A);
			A.Set(v).CMP(Temp[0]);
			return Condition.IsGreaterThan;
		}
		public Condition GreaterThanOrEqualTo(U8 v) {
			CMP(v);
			return Condition.IsGreaterThanOrEqualTo;
		}
		public Condition GreaterThanOrEqualTo(IOperand v) {
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
		public Condition LessThan(U8 v) => LessThan((IOperand)v);
		public Condition LessThan(IOperand v) {
			CMP(v);
			return Condition.IsLessThan;
		}
		//public Condition LessThanOrEqualTo(U8 v) => LessThanOrEqualTo((IOperand)v);
		public Condition LessThanOrEqualTo(U8 v) => LessThanOrEqualTo((IOperand)v);
		public Condition LessThanOrEqualTo(IOperand v) {
			GreaterThan(v);
			return Condition.IsLessThanOrEqualTo;
		}
	}
}
