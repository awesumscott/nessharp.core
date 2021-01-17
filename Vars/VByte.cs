using System;
using static NESSharp.Core.AL;

namespace NESSharp.Core {
	[VarSize(1)]
	public class VByte : Var, IOperand<Address>, IOperable<VByte> {//, IResolvable<Address> {
		public override int Size => 1;

		public VByte() {}
		public Address Value => this[0];
		public bool CanResolve() => true;
		public Address Resolve() => Value;

		private static int vbytecount = 0;	//TODO: num is temporary til I move VarRegistry to RAM instances

		public override VByte Dim(RAMRange ram, string name) {
			if (Address != null) throw new Exception("Var already dimmed");
			Address = ram.Dim(1);
			Name = name;
			DebugFileNESASM.WriteVariable(ram, Address[0], name);
			VarRegistry.Add(name + vbytecount++, this);	//TODO: num is temporary til I move VarRegistry to RAM instances
			return this;
		}
		public static VByte New(RAMRange ram, string name) => new VByte().Dim(ram, name);
		public static VByte Ref(Address addr, IndexingRegister index, string name) {
			var v = new VByte {
				Address = new Address[] { addr },
				Index = index,
				Name = string.IsNullOrEmpty(name) ? addr.ToString() : name
			};
			return v;
		}
		public static VByte Ref(Address addr, string name) {
			var v = new VByte {
				Address = new Address[] { addr },
				Name = string.IsNullOrEmpty(name) ? addr.ToString() : name
			};
			return v;
		}
		public override Var Copy(Var v) {
			if (v is not VByte)
				throw new Exception("Type must be Var8");
			var v8 = (VByte)v;
			Address = v8.Address;
			Name = v8.Name;
			Index = v8.Index;
			return this;
		}
		
		public VByte Set(IOperand operand) {
			if (operand is IOperand<Address> addr)
				A.Set(addr).STA(this);
			else if (operand is PtrY p) {
				if (Index is RegisterY) throw new Exception("NYI for var8[Y] = [ptr],y -- possible with stack backup");
					A.Set(p).STA(this);
			} else if (operand is IndexingRegister reg) {
				if (Index != null && Index == reg) throw new NotImplementedException(); //do some swapping to preserve X if this is worth it
					A.Set(reg).STA(this);
			} else
				A.Set(operand).STA(this);
			return this;
		}
		public VByte Set(Func<VByte, RegisterA> func) => Set(func.Invoke(this));
		public VByte Set(U8 u8) => Set((IOperand)u8);
		public VByte Set(IndexingRegister reg) {
			if (Index != null && Index == reg) throw new NotImplementedException(); //do some swapping to preserve X if this is worth it
				this[0].Set(reg);
			return this;
		}

		public RegisterA Add(IOperand v) =>				A.Set(this).Add(v);
		public RegisterA Add(U8 v) =>					A.Set(this).Add(v);
		public RegisterA Add(RegisterA _) =>			A.Add(this);

		public RegisterA Subtract(IOperand v) =>		A.Set(this).Subtract(v);
		public RegisterA Subtract(U8 v) =>				A.Set(this).Subtract(v);
		public RegisterA Subtract(RegisterA _) {
			Temp[0].Set(A);
			return A.Set(this).Subtract(Temp[0]);
		}

		public RegisterA And(IOperand v) =>				A.Set(this).And(v);
		public RegisterA And(U8 v) =>					A.Set(this).And(v);
		public RegisterA Or(IOperand v) =>				A.Set(this).Or(v);
		public RegisterA Or(U8 v) =>					A.Set(this).Or(v);
		public RegisterA Or(RegisterA _) {
			Temp[0].Set(A);
			return A.Set(this).Or(Temp[0]);
		}
		public RegisterA Xor(IOperand v) =>				A.Set(this).Xor(v);
		public RegisterA Xor(U8 v) =>					A.Set(this).Xor(v);

		public VByte SetROL() {
			if (Index is RegisterY) throw new Exception("Cannot SetROL with index Y");
			CPU6502.ROL(this);
			return this;
		}
		public VByte SetROR() {
			if (Index is RegisterY) throw new Exception("Cannot SetROR with index Y");
			CPU6502.ROR(this);
			return this;
		}
		public VByte SetLSR() {
			if (Index == null || Index is RegisterX)	CPU6502.LSR(this[0]);
			else										throw new NotImplementedException();
			return this;
		}
		public VByte SetASL() {
			if (Index == null || Index is RegisterX)	CPU6502.ASL(this[0]);
			else										throw new NotImplementedException();
			return this;
		}
		public Condition Equals(U8 v) =>						A.Set(this).Equals(v);
		public Condition Equals(IOperand v) =>					A.Set(this).Equals(v);
		public Condition Equals(RegisterA a) =>					A.Equals(this);
		public Condition NotEquals(U8 v) =>						A.Set(this).NotEquals(v);
		public Condition NotEquals(IOperand v) =>				A.Set(this).NotEquals(v);
		public Condition NotEquals(RegisterA a) =>				A.NotEquals(this);

		public Condition GreaterThan(U8 v) =>					A.Set(this).GreaterThan(v);
		public Condition GreaterThan(IOperand v) =>				A.Set(this).GreaterThan(v);
		public Condition GreaterThanOrEqualTo(IOperand v) =>	A.Set(this).GreaterThanOrEqualTo(v);
		public Condition GreaterThanOrEqualTo(U8 v) =>			A.Set(this).GreaterThanOrEqualTo(v);
		public Condition LessThan(IOperand v) =>				A.Set(this).LessThan(v);
		public Condition LessThan(U8 v) =>						A.Set(this).LessThan(v);
		public Condition LessThanOrEqualTo(U8 v) =>				A.Set(this).LessThanOrEqualTo(v);
		public Condition LessThanOrEqualTo(IOperand v) =>		A.Set(this).LessThanOrEqualTo(v);

		public Condition GreaterThan(RegisterA a) {
			Temp[1].Set(A);
			Temp[0].Set(this);
			A.Set(Temp[1]).CMP(Temp[0]);
			return Condition.IsGreaterThan;
		}
		public Condition LessThanOrEqualTo(RegisterA a) {
			GreaterThan(a);
			return Condition.IsLessThanOrEqualTo;
		}

		public static VByte operator ++(VByte addr) => addr.Increment();
		public VByte Increment() {
			CPU6502.INC(this);
			return this;
		}
		public static VByte operator --(VByte addr) => addr.Decrement();
		public VByte Decrement() {
			CPU6502.DEC(this);
			return this;
		}

		public AddressIndexed this[IndexingRegister r] => Address[0][r];

		public override string ToString() {
			return string.IsNullOrEmpty(Name) ? this[0].ToString() : Name;
		}
	}
}
