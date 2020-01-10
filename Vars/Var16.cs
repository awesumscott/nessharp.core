using System;
using System.Collections.Generic;
using static NESSharp.Core.AL;

namespace NESSharp.Core {
	public class Var16 : Var, IU16 {
		public Address Lo => Address[0];
		public Address Hi => Address[1];
		public override int Size => 2;

		IU8 IU16.Lo => Lo;

		IU8 IU16.Hi => Hi;

		public Var16() {}
		public override Var Dim(RAM ram, string name) {
			if (Address != null) throw new Exception("Var already dimmed");
			Address = ram.Dim(2);
			Name = name;
			DebugFile.WriteVariable(Lo, Hi, name);
			VarRegistry.Add(name, this);
			return this;
		}
		public static Var16 New(RAM ram, string name) {
			return (Var16)new Var16().Dim(ram, name);
		}
		public override Var Copy(Var v) {
			if (!(v is Var16))
				throw new Exception("Type must be Var8");
			var v16 = (Var16)v;
			Address = v16.Address;
			Name = v16.Name;
			OffsetRegister = v16.OffsetRegister;
			return v16;
		}
		public Var16 Set(RegisterA a) {
			Lo.Set(a);
			Hi.Set(0);
			return this;
		}
		public Var16 Set(U8 v) {
			Lo.Set(v);
			Hi.Set(0);
			return this;
		}
		public Var16 Set(U16 v) {
			Lo.Set(v.Lo);
			Hi.Set(v.Hi);
			return this;
		}
		public Var16 Set(IU16 v) {
			Lo.Set(v.Lo);
			Hi.Set(v.Hi);
			return this;
		}
		//public Var16 Set(Func<Var16, RegisterA> func) => Set(func.Invoke(this));
		//public RegisterA Add(U8 v) {
		//	Carry.Clear();
		//	if (OffsetRegister == null)
		//		return Address[0].ToA().ADC(v);
		//	return Address[0][OffsetRegister].ToA().ADC(v);
		//}
		public Var16 SetAdd(U8 u8) {
			Carry.Clear();
			Lo.Set(Lo.ToA().ADC(u8));
			Hi.Set(Hi.ToA().ADC(0));
			return this;
		}
		public Var16 SetAdd(RegisterA a) {
			Carry.Clear();
			Lo.Set(A.ADC(Lo));
			Hi.Set(Hi.ToA().ADC(0));
			return this;
		}
		public Var16 SetAdd(IU8 v) {
			Carry.Clear();
			Lo.Set(Lo.ToA().ADC(v));
			Hi.Set(Hi.ToA().ADC(0));
			return this;
		}
		public Var16 SetAdd(IU16 v) {
			Carry.Clear();
			Lo.Set(Lo.ToA().ADC(v.Lo));
			Hi.Set(Hi.ToA().ADC(v.Hi));
			return this;
		}
		
		public Var16 SetSubtract(U8 v) {
			Carry.Set();
			Lo.Set(Lo.ToA().SBC(v));
			Hi.Set(Hi.ToA().SBC(0));
			return this;
		}
		public Var16 SetSubtract(IU8 v) {
			Carry.Set();
			Lo.Set(Lo.ToA().SBC(v));
			Hi.Set(Hi.ToA().SBC(0));
			return this;
		}
		public Var16 SetSubtract(IU16 v) {
			Carry.Set();
			Lo.Set(Lo.ToA().SBC(v.Lo));
			Hi.Set(Hi.ToA().SBC(v.Hi));
			return this;
		}
		
		public Condition Equals(U8 v) {
			if (v == 0)
				return A.Set(Address[0]).Or(Address[1]).Equals(0); //fast way to check != 0
			//TODO: All(Address[0].Equals(0), Address[1].Equals(v))
			throw new NotImplementedException();
		}
		public Condition NotEquals(U8 v) {
			if (v == 0)
				return A.Set(Address[0]).Or(Address[1]).NotEquals(0); //fast way to check != 0
			//TODO: Any(Address[0].NotEquals(0), Address[1].NotEquals(v))
			throw new NotImplementedException();
		}
	}
}
