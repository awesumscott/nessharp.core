using System;
using System.Collections.Generic;
using System.Linq;

namespace NESSharp.Core {
	public class UniquenessState {
		private long _state = 0, _nextState = 1;
		public long Hash => _state;
		public RegisterBase? LastReg { get; private set; } //register modified by last op that was responsible for the latest state change
		private readonly Stack<long> _stateStack = new Stack<long>();
		public void Alter(RegisterBase? reg = null) {
			_state = _nextState++;
			LastReg = reg;
		}

		public void Push() {
			_stateStack.Push(_state);
			_nextState = _state + 1;
		}
		public void Pop() => _state = _stateStack.Pop();

		/// <summary>
		/// Ensure the integrity of the value after a code block.
		/// </summary>
		//TODO: check Clobber method attributes for calls to GoSub, and if none are specified, maybe throw an exception anyway to encourage their use for safety with this
		//Maybe a non-exception way would be to have GoSubs/GoTos set an "unsure" bool on the reg states, then Ensure can clear it before and check afterwards to warn if an "unsure" was encountered
		//Reset may already be the way to handle this, it is called in goto/gosub. Or maybe that's just where these changes should be located.
		public void Ensure(Action block) {
			var before = Hash;
			block();
			Verify(before);
		}

		public void Verify(long before) {
			if (Hash != before) throw new Exception($"{GetType().Name} was modified while being used as a loop index! Use Stack.Preserve");
		}

		public void Unsafe(Action block) {
			Push();
			block();
			Pop();
		}
	}
	public class FlagStates {
		public UniquenessState Carry			= new();
		public UniquenessState Zero				= new();
		public UniquenessState InterruptDisable	= new();
		public UniquenessState Decimal			= new();
		public UniquenessState Overflow			= new();
		public UniquenessState Negative			= new();
		public void Reset() {
			Carry.Alter();
			Zero.Alter();
			InterruptDisable.Alter();
			Decimal.Alter();
			Overflow.Alter();
			Negative.Alter();
		}
	}
	public enum CarryState {
		Set,
		Cleared,
		Unknown
	};
	public static class Carry {
		public static CarryState State;
		public static void Clear() =>			CPU6502.CLC();
		public static void Set() =>				CPU6502.SEC();
		public static Condition IsClear() =>	Condition.IsCarryClear;
		public static Condition IsSet() =>		Condition.IsCarrySet;
		public static void Reset() =>			State = CarryState.Unknown;
	}
	/// <summary>
	/// Central object for operations that indicates states of flags and last used registers to inform proceeding operations
	/// </summary>
	public static class CPU6502 {
		public static readonly RegisterA A = new RegisterA();
		public static readonly RegisterX X = new RegisterX();
		public static readonly RegisterY Y = new RegisterY();
		public static readonly FlagStates Flags = new FlagStates();

		public static void ADC(IOperand o) {					//N V Z C
			GenericAssembler(Asm.OC["ADC"], o);
			Carry.Reset();
			A.State.Alter();
			Flags.Negative.Alter(A);
			Flags.Overflow.Alter(A);
			Flags.Zero.Alter(A);
			Flags.Carry.Alter(A);
		}
		public static void AND(IOperand o) {					//N Z
			GenericAssembler(Asm.OC["AND"], o);
			//if (o is RegisterA) 
			A.State.Alter();
			Flags.Negative.Alter(A);
			Flags.Zero.Alter(A);
		}
		public static void ASL(IOperand o) {					//N Z C
			GenericAssembler(Asm.OC["ASL"], o);
			Carry.Reset();
			if (o is RegisterA) A.State.Alter();
			var reg = o is RegisterA ? A : null;
			Flags.Negative.Alter(reg);
			Flags.Zero.Alter(reg);
			Flags.Carry.Alter(reg);
		}

		public static void BPL(U8 len) => Context.Write(Asm.OC["BPL"][Asm.Mode.Relative].Use(len));
		public static void BMI(U8 len) => Context.Write(Asm.OC["BMI"][Asm.Mode.Relative].Use(len));
		public static void BVC(U8 len) => Context.Write(Asm.OC["BVC"][Asm.Mode.Relative].Use(len));
		public static void BVS(U8 len) => Context.Write(Asm.OC["BVS"][Asm.Mode.Relative].Use(len));
		public static void BCC(U8 len) => Context.Write(Asm.OC["BCC"][Asm.Mode.Relative].Use(len));
		public static void BCS(U8 len) => Context.Write(Asm.OC["BCS"][Asm.Mode.Relative].Use(len));
		public static void BNE(U8 len) => Context.Write(Asm.OC["BNE"][Asm.Mode.Relative].Use(len));
		public static void BEQ(U8 len) => Context.Write(Asm.OC["BEQ"][Asm.Mode.Relative].Use(len));

		public static void BIT(IOperand o) {					//N V Z
			GenericAssembler(Asm.OC["BIT"], o);
			Flags.Negative.Alter();
			Flags.Overflow.Alter();
			Flags.Zero.Alter();
		}
		public static void BRK() {								//B
			Context.Write(Asm.OC["BRK"][Asm.Mode.Implied].Use());
		}
		public static void CLC() {								//C
			Context.Write(Asm.OC["CLC"][Asm.Mode.Implied].Use());
			Carry.State = CarryState.Cleared;
			Flags.Carry.Alter();
		}
		public static void CLD() {								//none
			Context.Write(Asm.OC["CLD"][Asm.Mode.Implied].Use());
		}
		public static void CLI() {								//none
			Context.Write(Asm.OC["CLI"][Asm.Mode.Implied].Use());
		}
		public static void CMP(IOperand o) {					//N Z C
			GenericAssembler(Asm.OC["CMP"], o);
			Carry.Reset();
			Flags.Negative.Alter();
			Flags.Zero.Alter(A);
			Flags.Carry.Alter(A);
		}
		public static void CPX(IOperand o) {					//N Z C
			GenericAssembler(Asm.OC["CPX"], o);
			Carry.Reset();
			Flags.Negative.Alter();
			Flags.Zero.Alter(X);
			Flags.Carry.Alter(X);
		}
		public static void CPY(IOperand o) {					//N Z C
			GenericAssembler(Asm.OC["CPY"], o);
			Carry.Reset();
			Flags.Negative.Alter();
			Flags.Zero.Alter(Y);
			Flags.Carry.Alter(Y);
		}
		public static void DEC(IOperand o) {					//N Z
			GenericAssembler(Asm.OC["DEC"], o);
			Flags.Negative.Alter();
			Flags.Zero.Alter();
		}
		public static void DEX() {								//N Z
			Context.Write(Asm.OC["DEX"][Asm.Mode.Implied].Use());
			X.State.Alter();
			Flags.Negative.Alter(X);
			Flags.Zero.Alter(X);
		}
		public static void DEY() {								//N Z
			Context.Write(Asm.OC["DEY"][Asm.Mode.Implied].Use());
			Y.State.Alter();
			Flags.Negative.Alter(Y);
			Flags.Zero.Alter(Y);
		}
		public static void EOR(IOperand o) {					//N Z
			GenericAssembler(Asm.OC["EOR"], o);
			if (o is RegisterA) A.State.Alter();
			Flags.Negative.Alter(A);
			Flags.Zero.Alter(A);
		}
		public static void INC(IOperand o) {					//N Z
			GenericAssembler(Asm.OC["INC"], o);
			Flags.Negative.Alter();
			Flags.Zero.Alter();
		}
		public static void INX() {								//N Z
			Context.Write(Asm.OC["INX"][Asm.Mode.Implied].Use());
			X.State.Alter();
			Flags.Negative.Alter(X);
			Flags.Zero.Alter(X);
		}
		public static void INY() {								//N Z
			Context.Write(Asm.OC["INY"][Asm.Mode.Implied].Use());
			Y.State.Alter();
			Flags.Negative.Alter(Y);
			Flags.Zero.Alter(Y);
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
						A.LastLoaded != null &&
						A.LastLoaded is U8 &&
						A.LastLoaded == opVal
					) || A.LastStored == opVal
				)
				&& A.LastStoredHash == A.State.Hash && A.LastStoredFlagN == Flags.Negative.Hash && A.LastStoredFlagZ == Flags.Zero.Hash) return; //same address, same states for A, N, and Z
			GenericAssembler(Asm.OC["LDA"], o);
			A.State.Alter();
			Flags.Negative.Alter(A);
			Flags.Zero.Alter(A);
			A.LastLoaded = o;
		}
		public static void LDX(IOperand o) {					//N Z
			if (X.LastStored == GetOperandValue(o) && X.LastStoredHash == X.State.Hash && X.LastStoredFlagN == Flags.Negative.Hash && X.LastStoredFlagZ == Flags.Zero.Hash) return; //same address, same states for X, N, and Z
			GenericAssembler(Asm.OC["LDX"], o);
			X.State.Alter();
			Flags.Negative.Alter(X);
			Flags.Zero.Alter(X);
			X.LastLoaded = o;
		}
		public static void LDY(IOperand o) {					//N Z
			if (Y.LastStored == GetOperandValue(o) && Y.LastStoredHash == Y.State.Hash && Y.LastStoredFlagN == Flags.Negative.Hash && Y.LastStoredFlagZ == Flags.Zero.Hash) return; //same address, same states for Y, N, and Z
			GenericAssembler(Asm.OC["LDY"], o);
			Y.State.Alter();
			Flags.Negative.Alter(Y);
			Flags.Zero.Alter(Y);
			Y.LastLoaded = o;
		}
		public static void LSR(IOperand o) {					//N Z C
			GenericAssembler(Asm.OC["LSR"], o);
			Carry.Reset();
			if (o is RegisterA) A.State.Alter();
			var reg = o is RegisterA ? A : null;
			Flags.Negative.Alter(reg);
			Flags.Zero.Alter(reg);
			Flags.Carry.Alter(reg);
		}
		public static void NOP() {								//none
			Context.Write(Asm.OC["NOP"][Asm.Mode.Implied].Use());
		}
		public static void PHA() {
			Context.Write(Asm.OC["PHA"][Asm.Mode.Implied].Use());
		}
		public static void PHP() {
			Context.Write(Asm.OC["PHP"][Asm.Mode.Implied].Use());
		}
		public static void PLA() {
			Context.Write(Asm.OC["PLA"][Asm.Mode.Implied].Use());
			A.State.Alter();
		}
		public static void PLP() {
			Context.Write(Asm.OC["PLP"][Asm.Mode.Implied].Use());
			A.State.Alter();
		}
		public static void ORA(IOperand o) {					//N Z
			GenericAssembler(Asm.OC["ORA"], o);
			A.State.Alter();
			Flags.Negative.Alter(A);
			Flags.Zero.Alter(A);
		}
		public static void ROL(IOperand o) {					//N Z C
			GenericAssembler(Asm.OC["ROL"], o);
			Carry.Reset();
			if(o is RegisterA) A.State.Alter();
			var reg = o is RegisterA ? A : null;
			Flags.Negative.Alter(reg);
			Flags.Zero.Alter(reg);
			Flags.Carry.Alter(reg);
		}
		public static void ROR(IOperand o) {					//N Z C
			GenericAssembler(Asm.OC["ROR"], o);
			Carry.Reset();
			if(o is RegisterA) A.State.Alter();
			var reg = o is RegisterA ? A : null;
			Flags.Negative.Alter(reg);
			Flags.Zero.Alter(reg);
			Flags.Carry.Alter(reg);
		}
		public static void RTI() {								//all
			Context.Write(Asm.OC["RTI"][Asm.Mode.Implied].Use());
			AL.Reset();
		}
		public static void RTS() {								//all
			Context.Write(Asm.OC["RTS"][Asm.Mode.Implied].Use());
			AL.Reset();
		}
		public static void SBC(IOperand o) {					//N V Z C
			GenericAssembler(Asm.OC["SBC"], o);
			Carry.Reset();
			A.State.Alter();
			Flags.Negative.Alter(A);
			Flags.Overflow.Alter(A);
			Flags.Zero.Alter(A);
			Flags.Carry.Alter(A);
		}
		public static void SEC() {								//C
			Context.Write(Asm.OC["SEC"][Asm.Mode.Implied].Use());
			Carry.State = CarryState.Set;
			Flags.Carry.Alter();
		}
		public static void SEI() {								//I
			Context.Write(Asm.OC["SEI"][Asm.Mode.Implied].Use());
			Flags.InterruptDisable.Alter();
		}
		public static void STA(IOperand o) {					//none
			GenericAssembler(Asm.OC["STA"], o);
			A.LastStored = GetOperandValue(o); //TODO: clean all this up with a helper
			A.LastStoredHash = A.State.Hash;
			A.LastStoredFlagN = Flags.Negative.Hash;
			A.LastStoredFlagZ = Flags.Zero.Hash;
		}
		public static void STX(IOperand o) {					//none
			GenericAssembler(Asm.OC["STX"], o);
			X.LastStored = GetOperandValue(o); //TODO: clean all this up with a helper
			X.LastStoredHash = A.State.Hash;
			X.LastStoredFlagN = Flags.Negative.Hash;
			X.LastStoredFlagZ = Flags.Zero.Hash;
		}
		public static void STY(IOperand o) {					//none
			GenericAssembler(Asm.OC["STY"], o);
			Y.LastStored = GetOperandValue(o); //TODO: clean all this up with a helper
			Y.LastStoredHash = A.State.Hash;
			Y.LastStoredFlagN = Flags.Negative.Hash;
			Y.LastStoredFlagZ = Flags.Zero.Hash;
		}
		public static void TAX() {								//N Z
			Context.Write(Asm.OC["TAX"][Asm.Mode.Implied].Use());
			X.State.Alter();
			Flags.Negative.Alter(A);
			Flags.Zero.Alter(A);
		}
		public static void TAY() {								//N Z
			Context.Write(Asm.OC["TAY"][Asm.Mode.Implied].Use());
			Y.State.Alter();
			Flags.Negative.Alter(A);
			Flags.Zero.Alter(A);
		}
		public static void TSX() {								//?
			Context.Write(Asm.OC["TSX"][Asm.Mode.Implied].Use());
			X.State.Alter();
		}
		public static void TXA() {								//N Z
			Context.Write(Asm.OC["TXA"][Asm.Mode.Implied].Use());
			A.State.Alter();
			Flags.Negative.Alter(X);
			Flags.Zero.Alter(X);
		}
		public static void TXS() {								//?
			Context.Write(Asm.OC["TXS"][Asm.Mode.Implied].Use());
		}
		public static void TYA() {								//N Z
			Context.Write(Asm.OC["TYA"][Asm.Mode.Implied].Use());
			A.State.Alter();
			Flags.Negative.Alter(Y);
			Flags.Zero.Alter(Y);
		}
		private static object GetOperandValue(IOperand o) {
			if (o is IResolvable res && !res.CanResolve())
				return o;
			else if (o is IOperand<U8> u8) {
				return u8.Value;}
			else if (o is IOperand<Address> addr)
				return o;
			else if (o is IOperand<LabelIndexed> li)
				return li.Value;
			else if (o is IOperand<Label> lbl)	//this may not be necessary, since labels shouldn't be resolvable at this point
				throw new Exception("GetOperandValue found a Label"); //return lbl.Value;
			else if (o is IResolvable)	//this may not be necessary, since Constants bypass GenericAssembler
				throw new Exception("GetOperandValue found an IResolvable"); //return operand;
			return o;
		}
		//TODO: convert this to use IOperands with .Value, so other objects can resolve to these options
		private static void GenericAssembler(Dictionary<Asm.Mode, Asm.OpRef> opModes, IOperand operand) {
			object o = GetOperandValue(operand);
			switch (o) {
				case RegisterA _:
					if (opModes.ContainsKey(Asm.Mode.Accumulator))
						Context.Write(opModes[Asm.Mode.Accumulator].Use());
					break;
				case LabelIndexed oli and IIndexable lblInd:
					if (lblInd.Index is RegisterX && opModes.ContainsKey(Asm.Mode.AbsoluteX))
						Context.Write(opModes[Asm.Mode.AbsoluteX].Use(oli.Label));
					else if (lblInd.Index is RegisterY && opModes.ContainsKey(Asm.Mode.AbsoluteY))
						Context.Write(opModes[Asm.Mode.AbsoluteY].Use(oli.Label));
					else throw new Exception("Invalid addressing mode");
					break;
				case Label lbl:
					if (opModes.ContainsKey(Asm.Mode.Absolute))
						Context.Write(opModes[Asm.Mode.Absolute].Use(lbl));
					break;
				case IOperand<Address> addr:
					if (o is IIndexable addrInd) {
						if (addrInd.Index is RegisterX) {
							if (addr.Value.IsZP() && opModes.ContainsKey(Asm.Mode.ZeroPageX))
								Context.Write(opModes[Asm.Mode.ZeroPageX].Use(addr.Lo()));
							else if (opModes.ContainsKey(Asm.Mode.AbsoluteX))
								Context.Write(opModes[Asm.Mode.AbsoluteX].Use(addr));
							else throw new Exception("Invalid addressing mode");
							break;
						} else if (addrInd.Index is RegisterY && opModes.ContainsKey(Asm.Mode.AbsoluteY)) {
							Context.Write(opModes[Asm.Mode.AbsoluteY].Use(addr)); //no ZPY mode
							break;
						}
					}
					if (addr.Value.IsZP() && opModes.ContainsKey(Asm.Mode.ZeroPage))
						Context.Write(opModes[Asm.Mode.ZeroPage].Use(addr.Lo()));
					else if (opModes.ContainsKey(Asm.Mode.Absolute))
						Context.Write(opModes[Asm.Mode.Absolute].Use(addr));
					break;
				case PtrY ptrY:
					if (opModes.ContainsKey(Asm.Mode.IndirectY))
						Context.Write(opModes[Asm.Mode.IndirectY].Use(ptrY.Ptr.Lo.Lo()));
					else
						throw new Exception("No addressing mode for pointers");
					break;
				case U8 u8:
					if (opModes.ContainsKey(Asm.Mode.Immediate))
						Context.Write(opModes[Asm.Mode.Immediate].Use(u8));
					break;
				case IResolvable<U8> ru:
					if (opModes.ContainsKey(Asm.Mode.Immediate))
						Context.Write(opModes[Asm.Mode.Immediate].Use(ru)); //Immediate, because label his/los will be used to set up pointers
					break;
				default:
					throw new Exception($"Type {o.GetType()} not supported for op"); //TODO: elaborate
			}
		}

		public static class Asm {
			public enum Mode {
				Immediate,
				Absolute,
				ZeroPage,
				Implied,
				IndirectAbsolute,
				AbsoluteX,
				AbsoluteY,
				ZeroPageX,
				ZeroPageY,
				IndirectX,
				IndirectY,
				Relative,
				Accumulator
			}
			public class OpRef {
				public byte Byte;
				public string Token;
				public Mode Mode;
				public U8 Length => Mode switch {
					Mode.Immediate			=> 2,
					Mode.Absolute			=> 3,
					Mode.ZeroPage			=> 2,
					Mode.Implied			=> 1,
					Mode.IndirectAbsolute	=> 3,
					Mode.AbsoluteX			=> 3,
					Mode.AbsoluteY			=> 3,
					Mode.ZeroPageX			=> 2,
					Mode.ZeroPageY			=> 2,
					Mode.IndirectX			=> 2,
					Mode.IndirectY			=> 2,
					Mode.Relative			=> 2,
					Mode.Accumulator		=> 1,
					_						=> throw new Exception("Invalid addressing mode")
				};
				public OpRef(byte b, string token, Mode mode) {
					Byte = b;
					Token = token;
					Mode = mode;
				}
				public OpCode Use(IOperand? param = null) => new OpCode(Byte, Length, param);
				public string ToAsm(Func<OpRef, string> formatter) => formatter(this);
			}

			public static readonly List<OpRef> OpRefs = new List<OpRef>{
				new (0x00, "BRK", Mode.Implied),
				new (0x01, "ORA", Mode.IndirectX),
				new (0x05, "ORA", Mode.ZeroPage),
				new (0x06, "ASL", Mode.ZeroPage),
				new (0x08, "PHP", Mode.Implied),
				new (0x09, "ORA", Mode.Immediate),
				new (0x0A, "ASL", Mode.Accumulator),
				new (0x0D, "ORA", Mode.Absolute),
				new (0x0E, "ASL", Mode.Absolute),
				new (0x10, "BPL", Mode.Relative),
				new (0x11, "ORA", Mode.IndirectY),
				new (0x15, "ORA", Mode.ZeroPageX),
				new (0x16, "ASL", Mode.ZeroPageX),
				new (0x18, "CLC", Mode.Implied),
				new (0x19, "ORA", Mode.AbsoluteY),
				new (0x1D, "ORA", Mode.AbsoluteX),
				new (0x1E, "ASL", Mode.AbsoluteX),
				new (0x20, "JSR", Mode.Absolute),
				new (0x21, "AND", Mode.IndirectX),
				new (0x24, "BIT", Mode.ZeroPage),
				new (0x25, "AND", Mode.ZeroPage),
				new (0x26, "ROL", Mode.ZeroPage),
				new (0x28, "PLP", Mode.Implied),
				new (0x29, "AND", Mode.Immediate),
				new (0x2A, "ROL", Mode.Accumulator),
				new (0x2C, "BIT", Mode.Absolute),
				new (0x2D, "AND", Mode.Absolute),
				new (0x2E, "ROL", Mode.Absolute),
				new (0x30, "BMI", Mode.Relative),
				new (0x31, "AND", Mode.IndirectY),
				new (0x35, "AND", Mode.ZeroPageX),
				new (0x36, "ROL", Mode.ZeroPageX),
				new (0x38, "SEC", Mode.Implied),
				new (0x39, "AND", Mode.AbsoluteY),
				new (0x3D, "AND", Mode.AbsoluteX),
				new (0x3E, "ROL", Mode.AbsoluteX),
				new (0x40, "RTI", Mode.Implied),
				new (0x41, "EOR", Mode.IndirectX),
				new (0x45, "EOR", Mode.ZeroPage),
				new (0x46, "LSR", Mode.ZeroPage),
				new (0x48, "PHA", Mode.Implied),
				new (0x49, "EOR", Mode.Immediate),
				new (0x4A, "LSR", Mode.Accumulator),
				new (0x4C, "JMP", Mode.Absolute),
				new (0x4D, "EOR", Mode.Absolute),
				new (0x4E, "LSR", Mode.Absolute),
				new (0x50, "BVC", Mode.Relative),
				new (0x51, "EOR", Mode.IndirectY),
				new (0x55, "EOR", Mode.ZeroPageX),
				new (0x56, "LSR", Mode.ZeroPageX),
				new (0x58, "CLI", Mode.Implied),
				new (0x59, "EOR", Mode.AbsoluteY),
				new (0x5D, "EOR", Mode.AbsoluteX),
				new (0x5E, "LSR", Mode.AbsoluteX),
				new (0x60, "RTS", Mode.Implied),
				new (0x61, "ADC", Mode.IndirectX),
				new (0x65, "ADC", Mode.ZeroPage),
				new (0x66, "ROR", Mode.ZeroPage),
				new (0x68, "PLA", Mode.Implied),
				new (0x69, "ADC", Mode.Immediate),
				new (0x6A, "ROR", Mode.Accumulator),
				new (0x6C, "JMP", Mode.IndirectAbsolute),
				new (0x6D, "ADC", Mode.Absolute),
				new (0x6E, "ROR", Mode.Absolute),
				new (0x70, "BVS", Mode.Relative),
				new (0x71, "ADC", Mode.IndirectY),
				new (0x75, "ADC", Mode.ZeroPageX),
				new (0x76, "ROR", Mode.ZeroPageX),
				new (0x78, "SEI", Mode.Implied),
				new (0x79, "ADC", Mode.AbsoluteY),
				new (0x7D, "ADC", Mode.AbsoluteX),
				new (0x7E, "ROR", Mode.AbsoluteX),
				new (0x81, "STA", Mode.IndirectX),
				new (0x84, "STY", Mode.ZeroPage),
				new (0x85, "STA", Mode.ZeroPage),
				new (0x86, "STX", Mode.ZeroPage),
				new (0x88, "DEY", Mode.Implied),
				new (0x8A, "TXA", Mode.Implied),
				new (0x8C, "STY", Mode.Absolute),
				new (0x8D, "STA", Mode.Absolute),
				new (0x8E, "STX", Mode.Absolute),
				new (0x90, "BCC", Mode.Relative),
				new (0x91, "STA", Mode.IndirectY),
				new (0x94, "STY", Mode.ZeroPageX),
				new (0x95, "STA", Mode.ZeroPageX),
				new (0x96, "STX", Mode.ZeroPageY),
				new (0x98, "TYA", Mode.Implied),
				new (0x99, "STA", Mode.AbsoluteY),
				new (0x9A, "TXS", Mode.Implied),
				new (0x9D, "STA", Mode.AbsoluteX),
				new (0xA0, "LDY", Mode.Immediate),
				new (0xA1, "LDA", Mode.IndirectX),
				new (0xA2, "LDX", Mode.Immediate),
				new (0xA4, "LDY", Mode.ZeroPage),
				new (0xA5, "LDA", Mode.ZeroPage),
				new (0xA6, "LDX", Mode.ZeroPage),
				new (0xA8, "TAY", Mode.Implied),
				new (0xA9, "LDA", Mode.Immediate),
				new (0xAA, "TAX", Mode.Implied),
				new (0xAC, "LDY", Mode.Absolute),
				new (0xAD, "LDA", Mode.Absolute),
				new (0xAE, "LDX", Mode.Absolute),
				new (0xB0, "BCS", Mode.Relative),
				new (0xB1, "LDA", Mode.IndirectY),
				new (0xB4, "LDY", Mode.ZeroPageX),
				new (0xB5, "LDA", Mode.ZeroPageX),
				new (0xB6, "LDX", Mode.ZeroPageY),
				new (0xB8, "CLV", Mode.Implied),
				new (0xB9, "LDA", Mode.AbsoluteY),
				new (0xBA, "TSX", Mode.Implied),
				new (0xBC, "LDY", Mode.AbsoluteX),
				new (0xBD, "LDA", Mode.AbsoluteX),
				new (0xBE, "LDX", Mode.AbsoluteY),
				new (0xC0, "CPY", Mode.Immediate),
				new (0xC1, "CMP", Mode.IndirectX),
				new (0xC4, "CPY", Mode.ZeroPage),
				new (0xC5, "CMP", Mode.ZeroPage),
				new (0xC6, "DEC", Mode.ZeroPage),
				new (0xC8, "INY", Mode.Implied),
				new (0xC9, "CMP", Mode.Immediate),
				new (0xCA, "DEX", Mode.Implied),
				new (0xCC, "CPY", Mode.Absolute),
				new (0xCD, "CMP", Mode.Absolute),
				new (0xCE, "DEC", Mode.Absolute),
				new (0xD0, "BNE", Mode.Relative),
				new (0xD1, "CMP", Mode.IndirectY),
				new (0xD5, "CMP", Mode.ZeroPageX),
				new (0xD6, "DEC", Mode.ZeroPageX),
				new (0xD8, "CLD", Mode.Implied),
				new (0xD9, "CMP", Mode.AbsoluteY),
				new (0xDD, "CMP", Mode.AbsoluteX),
				new (0xDE, "DEC", Mode.AbsoluteX),
				new (0xE0, "CPX", Mode.Immediate),
				new (0xE1, "SBC", Mode.IndirectX),
				new (0xE4, "CPX", Mode.ZeroPage),
				new (0xE5, "SBC", Mode.ZeroPage),
				new (0xE6, "INC", Mode.ZeroPage),
				new (0xE8, "INX", Mode.Implied),
				new (0xE9, "SBC", Mode.Immediate),
				new (0xEA, "NOP", Mode.Implied),
				new (0xEC, "CPX", Mode.Absolute),
				new (0xED, "SBC", Mode.Absolute),
				new (0xEE, "INC", Mode.Absolute),
				new (0xF0, "BEQ", Mode.Relative),
				new (0xF1, "SBC", Mode.IndirectY),
				new (0xF5, "SBC", Mode.ZeroPageX),
				new (0xF6, "INC", Mode.ZeroPageX),
				new (0xF8, "SED", Mode.Implied),
				new (0xF9, "SBC", Mode.AbsoluteY),
				new (0xFD, "SBC", Mode.AbsoluteX),
				new (0xFE, "INC", Mode.AbsoluteX),
			};

			public static Dictionary<string, Dictionary<Mode, OpRef>> OC = OpRefs.Select(x => x.Token).Distinct().ToDictionary(x => x, x => OpRefs.Where(y => y.Token == x).ToDictionary(y => y.Mode, y => y));
		}
	}
}
