using System;
using static NESSharp.Core.AL;

namespace NESSharp.Core {
	[VarSize(1)]
	public class VByte : Var, IU8 {
		public override int Size => 1;

		public VByte() {}
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

		public VByte Set(VByte v) {
			this[0].Set(v[0]);
			return this;
		}
		public VByte Set(Address addr) {
			this[0].Set(addr);
			return this;
		}
		public VByte Set(IResolvable<U8> v) {
			this[0].Set(v);
			return this;
		}
		public VByte Set(LabelIndexed oli) {
			this[0].Set(oli);
			return this;
		}
		public VByte Set(IPtrIndexed p) {
			if (Index is RegisterY) throw new Exception("NYI for var8[Y] = [ptr],y -- possible with stack backup");
			this[0].Set(A.Set(p));
			return this;
		}
		public VByte Set(RegisterA a) {
			this[0].Set(a);
			return this;
		}
		public VByte Set(IndexingRegister reg) {
			if (Index != null && Index == reg) throw new NotImplementedException(); //do some swapping to preserve X if this is worth it
			this[0].Set(reg);
			return this;
		}
		public VByte Set(U8 v) {
			this[0].Set(v);
			return this;
		}
		public VByte Set(Func<VByte, RegisterA> func) => Set(func.Invoke(this));
		public RegisterA Add(U8 v) {
			Carry.Clear();
			return A.Set(this[0]).ADC(v);
		}
		public RegisterA Add(IU8 v) {
			Carry.Clear();
			return A.Set(this[0]).ADC(v);
		}
		public RegisterA Add(RegisterA a) {
			Carry.Clear();
			return A.Add(this[0]);
		}
		public RegisterA Add(LabelIndexed oli) {
			Carry.Clear();
			return A.Set(this[0]).ADC(oli);
		}
		public RegisterA Subtract(RegisterA a) {
			Temp[0].Set(a);
			A.Set(this[0]);
			return A.Subtract(Temp[0]);
		}
		public RegisterA Subtract(object o) {
			Carry.Set();
			return A.Set(this[0]).SBC(o);
		}
		public virtual RegisterA And(object v) {
			return A.Set(this[0]).And(v);
		}
		public virtual RegisterA Or(RegisterA a) {
			Temp[0].Set(A);
			return A.Set(this[0]).Or(Temp[0]);
		}
		public virtual RegisterA Or(U8 v) => A.Set(this[0]).Or(v);
		public virtual RegisterA Or(IU8 v) => A.Set(this[0]).Or(v);
		public virtual RegisterA Xor(U8 v) => A.Set(this[0]).Xor(v);
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
		public virtual VByte SetLSR() {
			if (Index == null || Index is RegisterX)	CPU6502.LSR(this[0]);
			else										throw new NotImplementedException();
			return this;
		}
		public Condition Equals(U8 v) {
			A.Set(this[0]);
			if (v != 0)
				A.CMP(v);
			return Condition.EqualsZero;
		}
		public Condition Equals(VByte v) {
			A.Set(this[0]).CMP(v);
			return Condition.EqualsZero;
		}
		public Condition Equals(RegisterA a) => this[0].Equals(a);
		public Condition NotEquals(U8 v) {
			Equals(v);
			return Condition.NotEqualsZero;
		}
		public Condition NotEquals(VByte v) {
			Equals(v);
			return Condition.NotEqualsZero;
		}
		public Condition NotEquals(RegisterA a) => this[0].NotEquals(a);

		public Condition GreaterThan(U8 v) {
			Temp[0].Set(this[0]);
			A.Set(v);
			A.CMP(Temp[0]);
			return Condition.IsGreaterThan;
		}
		public Condition GreaterThan(VByte v) {
			Temp[0].Set(this[0]);
			A.Set(v);
			A.CMP(Temp[0]);
			return Condition.IsGreaterThan;
		}
		public Condition GreaterThan(RegisterA a) {
			Temp[1].Set(A);
			Temp[0].Set(this[0]);
			A.Set(Temp[1]).CMP(Temp[0]);
			return Condition.IsGreaterThan;
		}
		public Condition GreaterThanOrEqualTo(U8 v) {
			A.Set(this[0]).CMP(v);
			return Condition.IsGreaterThanOrEqualTo;
		}
		public Condition GreaterThanOrEqualTo(VByte v) {
			A.Set(this[0]).CMP(v);
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
			CPU6502.INC(this[0]);
			return this;
		}
		public static VByte operator --(VByte addr) => addr.Decrement();
		public VByte Decrement() {
			CPU6502.DEC(this[0]);
			return this;
		}

		public AddressIndexed this[IndexingRegister r] => Address[0][r];
	}
}
