using System;
using System.Collections.Generic;

namespace NESSharp.Core {
	/// <summary>
	/// Central object for operations that indicates states of flags and last used registers to inform proceeding operations
	/// </summary>
	public static class CPU6502 {
		public static void ADC(IOperand o) {					//N V Z C
			GenericAssembler(Asm.OC["ADC"], o);
			Carry.Reset();
			AL.A.State.Alter();
			AL.Flags.Negative.Alter();
			AL.Flags.Overflow.Alter();
			AL.Flags.Zero.Alter();
			AL.Flags.Carry.Alter();
		}
		public static void AND(IOperand o) {					//N Z
			GenericAssembler(Asm.OC["AND"], o);
			if (o is RegisterA) AL.A.State.Alter();
			AL.Flags.Negative.Alter();
			AL.Flags.Zero.Alter();
		}
		public static void ASL(IOperand o) {					//N Z C
			GenericAssembler(Asm.OC["ASL"], o);
			Carry.Reset();
			if (o is RegisterA) AL.A.State.Alter();
			AL.Flags.Negative.Alter();
			AL.Flags.Zero.Alter();
			AL.Flags.Carry.Alter();
		}

		public static void BPL(U8 len) => AL.Use(Asm.OC["BPL"][Asm.Mode.Relative].Use(), len);
		public static void BMI(U8 len) => AL.Use(Asm.OC["BMI"][Asm.Mode.Relative].Use(), len);
		public static void BVC(U8 len) => AL.Use(Asm.OC["BVC"][Asm.Mode.Relative].Use(), len);
		public static void BVS(U8 len) => AL.Use(Asm.OC["BVS"][Asm.Mode.Relative].Use(), len);
		public static void BCC(U8 len) => AL.Use(Asm.OC["BCC"][Asm.Mode.Relative].Use(), len);
		public static void BCS(U8 len) => AL.Use(Asm.OC["BCS"][Asm.Mode.Relative].Use(), len);
		public static void BNE(U8 len) => AL.Use(Asm.OC["BNE"][Asm.Mode.Relative].Use(), len);
		public static void BEQ(U8 len) => AL.Use(Asm.OC["BEQ"][Asm.Mode.Relative].Use(), len);

		public static void BIT(IOperand o) {					//N V Z
			GenericAssembler(Asm.OC["BIT"], o);
			AL.Flags.Negative.Alter();
			AL.Flags.Overflow.Alter();
			AL.Flags.Zero.Alter();
		}
		public static void BRK() {								//B
			AL.Use(Asm.OC["BRK"][Asm.Mode.Implied].Use());
		}
		public static void CLC() {								//C
			AL.Use(Asm.OC["CLC"][Asm.Mode.Implied].Use());
			Carry.State = CarryState.Cleared;
			AL.Flags.Carry.Alter();
		}
		public static void CLD() {								//none
			AL.Use(Asm.OC["CLD"][Asm.Mode.Implied].Use());
		}
		public static void CLI() {								//none
			AL.Use(Asm.OC["CLI"][Asm.Mode.Implied].Use());
		}
		public static void CMP(IOperand o) {					//N Z C
			GenericAssembler(Asm.OC["CMP"], o);
			Carry.Reset();
			AL.Flags.Negative.Alter();
			AL.Flags.Zero.Alter();
			AL.Flags.Carry.Alter();
		}
		public static void CPX(IOperand o) {					//N Z C
			GenericAssembler(Asm.OC["CPX"], o);
			Carry.Reset();
			AL.Flags.Negative.Alter();
			AL.Flags.Zero.Alter();
			AL.Flags.Carry.Alter();
		}
		public static void CPY(IOperand o) {					//N Z C
			GenericAssembler(Asm.OC["CPY"], o);
			Carry.Reset();
			AL.Flags.Negative.Alter();
			AL.Flags.Zero.Alter();
			AL.Flags.Carry.Alter();
		}
		public static void DEC(IOperand o) {					//N Z
			GenericAssembler(Asm.OC["DEC"], o);
			AL.Flags.Negative.Alter();
			AL.Flags.Zero.Alter();
		}
		public static void DEX() {								//N Z
			AL.Use(Asm.OC["DEX"][Asm.Mode.Implied].Use());
			AL.X.State.Alter();
			AL.Flags.Negative.Alter();
			AL.Flags.Zero.Alter();
		}
		public static void DEY() {								//N Z
			AL.Use(Asm.OC["DEY"][Asm.Mode.Implied].Use());
			AL.Y.State.Alter();
			AL.Flags.Negative.Alter();
			AL.Flags.Zero.Alter();
		}
		public static void EOR(IOperand o) {					//N Z
			GenericAssembler(Asm.OC["EOR"], o);
			if (o is RegisterA) AL.A.State.Alter();
			AL.Flags.Negative.Alter();
			AL.Flags.Zero.Alter();
		}
		public static void INC(IOperand o) {					//N Z
			GenericAssembler(Asm.OC["INC"], o);
			AL.Flags.Negative.Alter();
			AL.Flags.Zero.Alter();
		}
		public static void INX() {								//N Z
			AL.Use(Asm.OC["INX"][Asm.Mode.Implied].Use());
			AL.X.State.Alter();
			AL.Flags.Negative.Alter();
			AL.Flags.Zero.Alter();
		}
		public static void INY() {								//N Z
			AL.Use(Asm.OC["INY"][Asm.Mode.Implied].Use());
			AL.Y.State.Alter();
			AL.Flags.Negative.Alter();
			AL.Flags.Zero.Alter();
		}
		public static void JMP(IOperand o) {					//none
			GenericAssembler(Asm.OC["JMP"], o);
			AL.Reset();
		}
		public static void JSR(IOperand o) {					//none
			GenericAssembler(Asm.OC["JSR"], o);
			AL.Reset();
		}
		public static void LDA(IOperand o) {					//N Z
			//this now doesn't catch IOperand values without GetOperandValue
			var opVal = GetOperandValue(o);
			if ((
					(
						AL.A.LastLoaded != null &&
						AL.A.LastLoaded is U8 &&
						AL.A.LastLoaded == opVal
					) || AL.A.LastStored == opVal
				)
				&& AL.A.LastStoredHash == AL.A.State.Hash && AL.A.LastStoredFlagN == AL.Flags.Negative.Hash && AL.A.LastStoredFlagZ == AL.Flags.Zero.Hash) return; //same address, same states for A, N, and Z
			GenericAssembler(Asm.OC["LDA"], o);
			AL.A.State.Alter();
			AL.Flags.Negative.Alter();
			AL.Flags.Zero.Alter();
			AL.A.LastLoaded = o;
		}
		public static void LDX(IOperand o) {					//N Z
			if (AL.X.LastStored == GetOperandValue(o) && AL.X.LastStoredHash == AL.X.State.Hash && AL.X.LastStoredFlagN == AL.Flags.Negative.Hash && AL.X.LastStoredFlagZ == AL.Flags.Zero.Hash) return; //same address, same states for A, N, and Z
			GenericAssembler(Asm.OC["LDX"], o);
			AL.X.State.Alter();
			AL.Flags.Negative.Alter();
			AL.Flags.Zero.Alter();
			AL.X.LastLoaded = o;
		}
		public static void LDY(IOperand o) {					//N Z
			if (AL.Y.LastStored == GetOperandValue(o) && AL.Y.LastStoredHash == AL.Y.State.Hash && AL.Y.LastStoredFlagN == AL.Flags.Negative.Hash && AL.Y.LastStoredFlagZ == AL.Flags.Zero.Hash) return; //same address, same states for A, N, and Z
			GenericAssembler(Asm.OC["LDY"], o);
			AL.Y.State.Alter();
			AL.Flags.Negative.Alter();
			AL.Flags.Zero.Alter();
			AL.Y.LastLoaded = o;
		}
		public static void LSR(IOperand o) {					//N Z C
			GenericAssembler(Asm.OC["LSR"], o);
			Carry.Reset();
			if (o is RegisterA) AL.A.State.Alter();
			AL.Flags.Negative.Alter();
			AL.Flags.Zero.Alter();
			AL.Flags.Carry.Alter();
		}
		public static void NOP() {								//none
			AL.Use(Asm.OC["NOP"][Asm.Mode.Implied].Use());
		}
		public static void PHA() {
			AL.Use(Asm.OC["PHA"][Asm.Mode.Implied].Use());
		}
		public static void PHP() {
			AL.Use(Asm.OC["PHP"][Asm.Mode.Implied].Use());
		}
		public static void PLA() {
			AL.Use(Asm.OC["PLA"][Asm.Mode.Implied].Use());
			AL.A.State.Alter();
		}
		public static void PLP() {
			AL.Use(Asm.OC["PLP"][Asm.Mode.Implied].Use());
			AL.A.State.Alter();
		}
		public static void ORA(IOperand o) {					//N Z
			GenericAssembler(Asm.OC["ORA"], o);
			AL.A.State.Alter();
			AL.Flags.Negative.Alter();
			AL.Flags.Zero.Alter();
		}
		public static void ROL(IOperand o) {					//N Z C
			GenericAssembler(Asm.OC["ROL"], o);
			Carry.Reset();
			if(o is RegisterA) AL.A.State.Alter();
			AL.Flags.Negative.Alter();
			AL.Flags.Zero.Alter();
			AL.Flags.Carry.Alter();
		}
		public static void ROR(IOperand o) {					//N Z C
			GenericAssembler(Asm.OC["ROR"], o);
			Carry.Reset();
			if(o is RegisterA) AL.A.State.Alter();
			AL.Flags.Negative.Alter();
			AL.Flags.Zero.Alter();
			AL.Flags.Carry.Alter();
		}
		public static void RTI() {								//all
			AL.Use(Asm.OC["RTI"][Asm.Mode.Implied].Use());
			AL.Reset();
		}
		public static void RTS() {								//all
			AL.Use(Asm.OC["RTS"][Asm.Mode.Implied].Use());
			AL.Reset();
		}
		public static void SBC(IOperand o) {					//N V Z C
			GenericAssembler(Asm.OC["SBC"], o);
			Carry.Reset();
			AL.A.State.Alter();
			AL.Flags.Negative.Alter();
			AL.Flags.Overflow.Alter();
			AL.Flags.Zero.Alter();
			AL.Flags.Carry.Alter();
		}
		public static void SEC() {								//C
			AL.Use(Asm.OC["SEC"][Asm.Mode.Implied].Use());
			Carry.State = CarryState.Set;
			AL.Flags.Carry.Alter();
		}
		public static void SEI() {								//I
			AL.Use(Asm.OC["SEI"][Asm.Mode.Implied].Use());
			AL.Flags.InterruptDisable.Alter();
		}
		public static void STA(IOperand o) {					//none
			GenericAssembler(Asm.OC["STA"], o);
			AL.A.LastStored = GetOperandValue(o); //TODO: clean all this up with a helper
			AL.A.LastStoredHash = AL.A.State.Hash;
			AL.A.LastStoredFlagN = AL.Flags.Negative.Hash;
			AL.A.LastStoredFlagZ = AL.Flags.Zero.Hash;
		}
		public static void STX(IOperand o) {					//none
			GenericAssembler(Asm.OC["STX"], o);
			AL.X.LastStored = GetOperandValue(o); //TODO: clean all this up with a helper
			AL.X.LastStoredHash = AL.A.State.Hash;
			AL.X.LastStoredFlagN = AL.Flags.Negative.Hash;
			AL.X.LastStoredFlagZ = AL.Flags.Zero.Hash;
		}
		public static void STY(IOperand o) {					//none
			GenericAssembler(Asm.OC["STY"], o);
			AL.Y.LastStored = GetOperandValue(o); //TODO: clean all this up with a helper
			AL.Y.LastStoredHash = AL.A.State.Hash;
			AL.Y.LastStoredFlagN = AL.Flags.Negative.Hash;
			AL.Y.LastStoredFlagZ = AL.Flags.Zero.Hash;
		}
		public static void TAX() {								//N Z
			AL.Use(Asm.OC["TAX"][Asm.Mode.Implied].Use());
			AL.X.State.Alter();
			AL.Flags.Negative.Alter();
			AL.Flags.Zero.Alter();
		}
		public static void TAY() {								//N Z
			AL.Use(Asm.OC["TAY"][Asm.Mode.Implied].Use());
			AL.Y.State.Alter();
			AL.Flags.Negative.Alter();
			AL.Flags.Zero.Alter();
		}
		public static void TSX() {								//?
			AL.Use(Asm.OC["TSX"][Asm.Mode.Implied].Use());
			AL.X.State.Alter();
		}
		public static void TXA() {								//N Z
			AL.Use(Asm.OC["TXA"][Asm.Mode.Implied].Use());
			AL.A.State.Alter();
			AL.Flags.Negative.Alter();
			AL.Flags.Zero.Alter();
		}
		public static void TXS() {								//?
			AL.Use(Asm.OC["TXS"][Asm.Mode.Implied].Use());
		}
		public static void TYA() {								//N Z
			AL.Use(Asm.OC["TYA"][Asm.Mode.Implied].Use());
			AL.A.State.Alter();
			AL.Flags.Negative.Alter();
			AL.Flags.Zero.Alter();
		}
		private static object GetOperandValue(IOperand o) {
			if (o is IOperand operand) {
				if (operand is IResolvable)
					return operand;
				else if (operand is IOperand<U8> u8)
					return u8.Value;
				else if (operand is IOperand<Address> addr)
					return addr.Value;
				else if (operand is IOperand<PtrY> ptrY)
					return ptrY.Value;
				else if (operand is IOperand<Label> lbl)
					return lbl.Value;
				else if (operand is IOperand<LabelIndexed> li)
					return li.Value;
			}
			return o;
		}
		//TODO: convert this to use IOperands with .Value, so other objects can resolve to these options
		private static void GenericAssembler(Dictionary<Asm.Mode, Asm.OpRef> opModes, IOperand operand) {
			object o = GetOperandValue(operand);
			switch (o) {
				case RegisterA _:
					if (opModes.ContainsKey(Asm.Mode.Accumulator))
						AL.Use(opModes[Asm.Mode.Accumulator].Use());
					break;
				case AddressIndexed ai:
					if (ai.Index is RegisterX) {
						if (ai.Hi == 0 && opModes.ContainsKey(Asm.Mode.ZeroPageX))
							AL.Use(opModes[Asm.Mode.ZeroPageX].Use(), ai.Lo);
						else if (opModes.ContainsKey(Asm.Mode.AbsoluteX))
							AL.Use(opModes[Asm.Mode.AbsoluteX].Use(), ai);
						else throw new Exception("Invalid addressing mode");
					} else if (ai.Index is RegisterY && opModes.ContainsKey(Asm.Mode.AbsoluteY))
						AL.Use(opModes[Asm.Mode.AbsoluteY].Use(), ai); //no ZPY mode
					else throw new Exception("Invalid indexing register");
					break;
				case Address addr:
					if (addr.IsZP() && opModes.ContainsKey(Asm.Mode.ZeroPage))
						AL.Use(opModes[Asm.Mode.ZeroPage].Use(), addr.Lo);
					else if (opModes.ContainsKey(Asm.Mode.Absolute))
						AL.Use(opModes[Asm.Mode.Absolute].Use(), addr);
					break;
				case LabelIndexed oli:
					if (oli.Index is RegisterX && opModes.ContainsKey(Asm.Mode.AbsoluteX))
						AL.Use(opModes[Asm.Mode.AbsoluteX].Use(), oli.Label);
					else if (oli.Index is RegisterY && opModes.ContainsKey(Asm.Mode.AbsoluteY))
						AL.Use(opModes[Asm.Mode.AbsoluteY].Use(), oli.Label);
					else throw new Exception("Invalid addressing mode");
					break;
				case Label lbl:
					if (opModes.ContainsKey(Asm.Mode.Absolute))
						AL.Use(opModes[Asm.Mode.Absolute].Use(), lbl);
					break;
				case Ptr _:
					throw new Exception("Pointers must be indexed with X or Y");
				case PtrY ptrY:
					if (opModes.ContainsKey(Asm.Mode.IndirectY))
						AL.Use(opModes[Asm.Mode.IndirectY].Use(), ptrY.Ptr.Lo[0].Lo);
					else
						throw new Exception("No addressing mode for pointers");
					break;
				case U8 u8:
					if (opModes.ContainsKey(Asm.Mode.Immediate))
						AL.Use(opModes[Asm.Mode.Immediate].Use(), u8);
					break;
				case int i:
					U8 u = i;
					if (u != i)
						throw new Exception("Integer value out of range for A");
					if (opModes.ContainsKey(Asm.Mode.Immediate))
						AL.Use(opModes[Asm.Mode.Immediate].Use(), u);
					break;
				case byte b:
					if (opModes.ContainsKey(Asm.Mode.Immediate))
						AL.Use(opModes[Asm.Mode.Immediate].Use(), (U8)b);
					break;
				case IResolvable<Address> ra:
					if (opModes.ContainsKey(Asm.Mode.Absolute))
						AL.Use(opModes[Asm.Mode.Absolute].Use(), ra); //TODO: see if this will be used, and if it'll be correct
					break;
				case IResolvable<U8> ru:
					if (opModes.ContainsKey(Asm.Mode.Immediate))
						AL.Use(opModes[Asm.Mode.Immediate].Use(), ru); //Immediate, because label his/los will be used to set up pointers
					break;
				default:
					throw new Exception($"Type {o.GetType()} not supported for op"); //TODO: elaborate
			}
		}
	}
}
