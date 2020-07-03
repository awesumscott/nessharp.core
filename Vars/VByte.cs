using System;
using static NESSharp.Core.AL;

namespace NESSharp.Core {
	[VarSize(1)]
	public class VByte : Var, IU8 {
		public override int Size => 1;

		public VByte() {}
		public static implicit operator Address(VByte v) => v.Index == null ? v.Address[0] : v.Address[0][v.Index];
		//public override int Size_New { get; set; } = 1;

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
		public static VByte Ref(Address addr, IndexingRegister index = null) {
			var v = new VByte();
			v.Address = new Address[]{ addr };
			v.Index = index;
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

		private object _getByte(U8 i) => Index == null ? Address[i] : Address[i][Index];

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
		public VByte Set(IndexingRegister reg) {
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
			return A.Set(_getByte(0)).ADC(v);
		}
		public RegisterA Add(IU8 v) {
			Carry.Clear();
			return A.Set(_getByte(0)).ADC(v);
		}
		public RegisterA Add(RegisterA a) {
			Carry.Clear();
			return A.Add(_getByte(0));
		}
		public RegisterA Add(OpLabelIndexed oli) {
			Carry.Clear();
			return A.Set(_getByte(0)).ADC(oli);
		}
		public RegisterA Subtract(U8 v) {
			Carry.Set();
			return A.Set(_getByte(0)).SBC(v);
		}
		public RegisterA Subtract(IU8 v) {
			Carry.Set();
			return A.Set(_getByte(0)).SBC(v);
		}
		public RegisterA Subtract(RegisterA a) {
			Temp[0].Set(a);
			A.Set(_getByte(0));
			return A.Subtract(Temp[0]);
		}
		public virtual RegisterA And(U8 v) {
			return A.Set(_getByte(0)).And(v);
		}
		public virtual RegisterA And(IVarAddressArray iva) {
			Func<object> operandRhs = () => iva.Index == null ? iva.Address[0] : iva.Address[0][Index];
			return A.Set(_getByte(0)).And(operandRhs());
		}
		public virtual RegisterA And(OpLabelIndexed oli) {
			return A.Set(_getByte(0)).And(oli);
		}
		public virtual RegisterA Or(RegisterA a) {
			Temp[0].Set(A);
			return A.Set(_getByte(0)).Or(Temp[0]);
		}
		public virtual RegisterA Or(U8 v) => A.Set(_getByte(0)).Or(v);
		public virtual RegisterA Or(IU8 v) => A.Set(_getByte(0)).Or(v);
		public virtual RegisterA Xor(U8 v) => A.Set(_getByte(0)).Xor(v);
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
			A.Set(_getByte(0));
			if (v != 0)
				A.CMP(v);
			return Condition.EqualsZero;
		}
		public Condition Equals(VByte v) {
			if (Index == null)
				A.Set(Address[0]);
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
			if (Index == null)
				Temp[0].Set(Address[0]);
			else
				Temp[0].Set(Address[0][Index]);
			A.Set(v);
			A.CMP(Temp[0]);
			return Condition.IsGreaterThan;
		}
		public Condition GreaterThan(VByte v) {
			if (Index == null)
				Temp[0].Set(Address[0]);
			else
				Temp[0].Set(Address[0][Index]);
			A.Set(v);
			A.CMP(Temp[0]);
			return Condition.IsGreaterThan;
		}
		public Condition GreaterThan(RegisterA a) {
			Temp[1].Set(A);
			if (Index == null)
				Temp[0].Set(Address[0]);
			else
				Temp[0].Set(Address[0][Index]);
			A.Set(Temp[1]).CMP(Temp[0]);
			return Condition.IsGreaterThan;
		}
		public Condition GreaterThanOrEqualTo(U8 v) {
			A.Set(_getByte(0)).CMP(v);
			return Condition.IsGreaterThanOrEqualTo;
		}
		public Condition GreaterThanOrEqualTo(VByte v) {
			A.Set(_getByte(0)).CMP(v);
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
			CPU6502.INC(_getByte(0));
			return this;
		}
		public static VByte operator --(VByte addr) => addr.Decrement();
		public VByte Decrement() {
			CPU6502.DEC(_getByte(0));
			return this;
		}

		public AddressIndexed this[IndexingRegister r] => Address[0][r];
	}
}
