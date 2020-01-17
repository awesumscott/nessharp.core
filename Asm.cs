﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NESSharp.Core {
	public abstract class Operation {
		public int Length;
		public object Param;
		public object Param2;
	}
	public class OpRaw : Operation {
		public object[] Value;
		public OpRaw(U8 v) {
			Length = 1;
			Value = new object[]{ v };
		}
		public OpRaw(byte[] v) {
			Length = v.Length;
			Value = v.Cast<object>().ToArray();
		}
		public OpRaw(object[] v) {
			Length = v.Length;
			Value = v;
		}
	}
	public class OpComment : Operation {
		public string Text;
		public long ID;
		public OpComment(string text) {
			ID = DateTime.Now.Ticks;
			Text = text;
			Length = 0;
		}
		public override string ToString() {
			return "#" + Text;
		}
	}
	public class OpCode : Operation {
		public byte Value;
		public string Description;
		public OpCode(byte opVal, byte len = 1) {
			Value = opVal;
			Length = len;
		}
		public byte[] Output() => new byte[] { Value }; //TODO: output args

		public static implicit operator byte(OpCode o) => o.Value;

		public override string ToString() {
			return Value.ToString("X") + " " + Param?.ToString() + " " + Param2?.ToString();
		}
	}
	/// <summary>
	/// Central object for operations that indicates states of flags and last used registers to inform proceeding operations
	/// </summary>
	public static class CPU6502 {
		public static void ADC(object o) {
			GenericAssembler(Asm.OC["ADC"], o);
			Carry.State = CarryState.Unknown;
		}
		public static void AND(object o) {
			GenericAssembler(Asm.OC["AND"], o);
		}
		public static void ASL(object o) {
			GenericAssembler(Asm.OC["ASL"], o);
			Carry.State = CarryState.Unknown;
		}
		public static void BIT(object o) {
			GenericAssembler(Asm.OC["BIT"], o);
		}
		public static void CLC() {
			AL.Use(Asm.OC["CLC"][Asm.Mode.Implied].Use());
			Carry.State = CarryState.Cleared;
		}
		public static void CMP(object o) {
			GenericAssembler(Asm.OC["CMP"], o);
			Carry.State = CarryState.Unknown;
		}
		public static void CPX(object o) {
			GenericAssembler(Asm.OC["CPX"], o);
			Carry.State = CarryState.Unknown;
		}
		public static void CPY(object o) {
			GenericAssembler(Asm.OC["CPY"], o);
			Carry.State = CarryState.Unknown;
		}
		public static void DEC(object o) {
			GenericAssembler(Asm.OC["DEC"], o);
		}
		public static void DEX() {
			AL.Use(Asm.OC["DEX"][Asm.Mode.Implied].Use());
		}
		public static void DEY() {
			AL.Use(Asm.OC["DEY"][Asm.Mode.Implied].Use());
		}
		public static void EOR(object o) {
			GenericAssembler(Asm.OC["EOR"], o);
		}
		public static void INC(object o) {
			GenericAssembler(Asm.OC["INC"], o);
		}
		public static void INX() {
			AL.Use(Asm.OC["INX"][Asm.Mode.Implied].Use());
		}
		public static void INY() {
			AL.Use(Asm.OC["INY"][Asm.Mode.Implied].Use());
		}
		public static void LDA(object o) {
			GenericAssembler(Asm.OC["LDA"], o);
		}
		public static void LDX(object o) {
			GenericAssembler(Asm.OC["LDX"], o);
		}
		public static void LDY(object o) {
			GenericAssembler(Asm.OC["LDY"], o);
		}
		public static void LSR(object o) {
			GenericAssembler(Asm.OC["LSR"], o);
			Carry.State = CarryState.Unknown;
		}
		public static void ORA(object o) {
			GenericAssembler(Asm.OC["ORA"], o);
		}
		public static void ROL(object o) {
			GenericAssembler(Asm.OC["ROL"], o);
			Carry.State = CarryState.Unknown;
		}
		public static void ROR(object o) {
			GenericAssembler(Asm.OC["ROR"], o);
			Carry.State = CarryState.Unknown;
		}
		public static void SBC(object o) {
			GenericAssembler(Asm.OC["SBC"], o);
			Carry.State = CarryState.Unknown;
		}
		public static void SEC() {
			AL.Use(Asm.OC["SEC"][Asm.Mode.Implied].Use());
			Carry.State = CarryState.Set;
		}
		public static void STA(object o) {
			GenericAssembler(Asm.OC["STA"], o);
		}
		public static void STX(object o) {
			GenericAssembler(Asm.OC["STX"], o);
		}
		public static void STY(object o) {
			GenericAssembler(Asm.OC["STY"], o);
		}
		public static void TAX() {
			AL.Use(Asm.OC["TAX"][Asm.Mode.Implied].Use());
		}
		public static void TAY() {
			AL.Use(Asm.OC["TAY"][Asm.Mode.Implied].Use());
		}
		public static void TXA() {
			AL.Use(Asm.OC["TXA"][Asm.Mode.Implied].Use());
		}
		public static void TYA() {
			AL.Use(Asm.OC["TYA"][Asm.Mode.Implied].Use());
		}
		private static void GenericAssembler(Dictionary<Asm.Mode, Asm.OpRef> opModes, object o) {
			switch (o) {
				case RegisterA ra:
					if (opModes.ContainsKey(Asm.Mode.Accumulator))
						AL.Use(opModes[Asm.Mode.Accumulator].Use());
					break;
				case AddressIndexed ai:
					if (ai.Index is RegisterX) {
						if (ai.Hi == 0 && opModes.ContainsKey(Asm.Mode.ZeroPageX))
							AL.Use(opModes[Asm.Mode.ZeroPageX].Use(), ai.Lo);
						else if (opModes.ContainsKey(Asm.Mode.AbsoluteX))
							AL.Use(opModes[Asm.Mode.AbsoluteX].Use(), ai);
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
				case OpLabelIndexed oli:
					if (oli.Index is RegisterX) {
						if (opModes.ContainsKey(Asm.Mode.AbsoluteX))
							AL.Use(opModes[Asm.Mode.AbsoluteX].Use(), oli.Label);
					} else if (oli.Index is RegisterY) {
						if (opModes.ContainsKey(Asm.Mode.AbsoluteY))
							AL.Use(opModes[Asm.Mode.AbsoluteY].Use(), oli.Label);
					} else throw new Exception("Invalid indexing register");
					break;
				case OpLabel lbl:
					if (opModes.ContainsKey(Asm.Mode.Absolute))
						AL.Use(opModes[Asm.Mode.Absolute].Use(), lbl);
					break;
				case Ptr ptr:
					//AL.Use(Asm.LDA.IndirectY, ptr.Lo.Lo);
					throw new Exception("Pointers must be indexed with X or Y");
					break;
				case PtrY ptrY:
					if (opModes.ContainsKey(Asm.Mode.IndirectY))
						AL.Use(opModes[Asm.Mode.IndirectY].Use(), ptrY.Ptr.Lo.Lo);
					break;
				case U8 u8:
					if (opModes.ContainsKey(Asm.Mode.Immediate))
						AL.Use(opModes[Asm.Mode.Immediate].Use(), u8);
					break;
				case int i:
					U8 u = (U8)i;
					if (u != i)
						throw new Exception("Integer value out of range for A");
					if (opModes.ContainsKey(Asm.Mode.Immediate))
						AL.Use(opModes[Asm.Mode.Immediate].Use(), u);
					break;
				case byte b:
					if (opModes.ContainsKey(Asm.Mode.Immediate))
						AL.Use(opModes[Asm.Mode.Immediate].Use(), (U8)b);
					break;
				case IVarAddressArray vaa: //TODO: there's probably a good reason to not support this in here
					if (vaa.OffsetRegister == null)
						GenericAssembler(opModes, vaa.Address[0]);
					else
						GenericAssembler(opModes, vaa.Address[0][vaa.OffsetRegister]);
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
					throw new Exception("type not supported for op"); //TODO: elaborate
			}
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
			public string Format;
			public U8 Length => (U8)(Mode switch {
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
			});
			public OpRef(byte b, string token, Mode mode, string format) {
				Byte = b;
				Token = token;
				Mode = mode;
				Format = format;
			}

			public static OpCode Use(string code, Mode mode) {
				var opcodeRef = OpRefs.FirstOrDefault(x => x.Token == code.ToUpper() && x.Mode == mode);
				if (opcodeRef == null) throw new Exception($"Illegal opcode or addressing mode: { code } { mode.ToString() }");
				return new OpCode(opcodeRef.Byte, opcodeRef.Length);
			}
			public OpCode Use() => new OpCode(Byte, Length);
		}
		//TODO: replace format strings with generic strings per addressing mode with op as another param. Then make format string lists replaceable to support different assembler syntaxes.
		public static readonly List<OpRef> OpRefs = new List<OpRef>{
			new OpRef(	0x00, "BRK", Mode.Implied,		"BRK"),
			new OpRef(	0x01, "ORA", Mode.IndirectX,	"ORA ({0}, X)"),
			new OpRef(	0x05, "ORA", Mode.ZeroPage,		"ORA {0}"),
			new OpRef(	0x06, "ASL", Mode.ZeroPage,		"ASL {0}"),
			new OpRef(	0x08, "PHP", Mode.Implied,		"PHP"),
			new OpRef(	0x09, "ORA", Mode.Immediate,	"ORA #{0}"),
			new OpRef(	0x0A, "ASL", Mode.Accumulator,	"ASL A"),
			new OpRef(	0x0D, "ORA", Mode.Absolute,		"ORA {0}{1}"),
			new OpRef(	0x0E, "ASL", Mode.Absolute,		"ASL {0}{1}"),
			new OpRef(	0x10, "BPL", Mode.Relative,		"BPL {0}"),
			new OpRef(	0x11, "ORA", Mode.IndirectY,	"ORA ({0}), Y"),
			new OpRef(	0x15, "ORA", Mode.ZeroPageX,	"ORA {0}, X"),
			new OpRef(	0x16, "ASL", Mode.ZeroPageX,	"ASL {0}, X"),
			new OpRef(	0x18, "CLC", Mode.Implied,		"CLC"),
			new OpRef(	0x19, "ORA", Mode.AbsoluteY,	"ORA {0}{1}, Y"),
			new OpRef(	0x1D, "ORA", Mode.AbsoluteX,	"ORA {0}{1}, X"),
			new OpRef(	0x1E, "ASL", Mode.AbsoluteX,	"ASL {0}{1}, X"),
			new OpRef(	0x20, "JSR", Mode.Absolute,		"JSR {0}{1}"),
			new OpRef(	0x21, "AND", Mode.IndirectX,	"AND ({0}, X)"),
			new OpRef(	0x24, "BIT", Mode.ZeroPage,		"BIT {0}"),
			new OpRef(	0x25, "AND", Mode.ZeroPage,		"AND {0}"),
			new OpRef(	0x26, "ROL", Mode.ZeroPage,		"ROL {0}"),
			new OpRef(	0x28, "PLP", Mode.Implied,		"PLP"),
			new OpRef(	0x29, "AND", Mode.Immediate,	"AND #{0}"),
			new OpRef(	0x2A, "ROL", Mode.Accumulator,	"ROL A"),
			new OpRef(	0x2C, "BIT", Mode.Absolute,		"BIT {0}{1}"),
			new OpRef(	0x2D, "AND", Mode.Absolute,		"AND {0}{1}"),
			new OpRef(	0x2E, "ROL", Mode.Absolute,		"ROL {0}{1}"),
			new OpRef(	0x30, "BMI", Mode.Relative,		"BMI {0}"),
			new OpRef(	0x31, "AND", Mode.IndirectY,	"AND ({0}), Y"),
			new OpRef(	0x35, "AND", Mode.ZeroPageX,	"AND {0}, X"),
			new OpRef(	0x36, "ROL", Mode.ZeroPageX,	"ROL {0}, X"),
			new OpRef(	0x38, "SEC", Mode.Implied,		"SEC"),
			new OpRef(	0x39, "AND", Mode.AbsoluteY,	"AND {0}{1}, Y"),
			new OpRef(	0x3D, "AND", Mode.AbsoluteX,	"AND {0}{1}, X"),
			new OpRef(	0x3E, "ROL", Mode.AbsoluteX,	"ROL {0}{1}, X"),
			new OpRef(	0x40, "RTI", Mode.Implied,		"RTI"),
			new OpRef(	0x41, "EOR", Mode.IndirectX,	"EOR ({0}, X)"),
			new OpRef(	0x45, "EOR", Mode.ZeroPage,		"EOR {0}"),
			new OpRef(	0x46, "LSR", Mode.ZeroPage,		"LSR {0}"),
			new OpRef(	0x48, "PHA", Mode.Implied,		"PHA"),
			new OpRef(	0x49, "EOR", Mode.Immediate,	"EOR #{0}"),
			new OpRef(	0x4A, "LSR", Mode.Accumulator,	"LSR A"),
			new OpRef(	0x4C, "JMP", Mode.Absolute,		"JMP {0}{1}"),
			new OpRef(	0x4D, "EOR", Mode.Absolute,		"EOR {0}{1}"),
			new OpRef(	0x4E, "LSR", Mode.Absolute,		"LSR {0}{1}"),
			new OpRef(	0x50, "BVC", Mode.Relative,		"BVC {0}"),
			new OpRef(	0x51, "EOR", Mode.IndirectY,	"EOR ({0}), Y"),
			new OpRef(	0x55, "EOR", Mode.ZeroPageX,	"EOR {0}, X"),
			new OpRef(	0x56, "LSR", Mode.ZeroPageX,	"LSR {0}, X"),
			new OpRef(	0x58, "CLI", Mode.Implied,		"CLI"),
			new OpRef(	0x59, "EOR", Mode.AbsoluteY,	"EOR {0}{1}, Y"),
			new OpRef(	0x5D, "EOR", Mode.AbsoluteX,	"EOR {0}{1}, X"),
			new OpRef(	0x5E, "LSR", Mode.AbsoluteX,	"LSR {0}{1}, X"),
			new OpRef(	0x60, "RTS", Mode.Implied,		"RTS"),
			new OpRef(	0x61, "ADC", Mode.IndirectX,	"ADC ({0}, X)"),
			new OpRef(	0x65, "ADC", Mode.ZeroPage,		"ADC {0}"),
			new OpRef(	0x66, "ROR", Mode.ZeroPage,		"ROR {0}"),
			new OpRef(	0x68, "PLA", Mode.Implied,		"PLA"),
			new OpRef(	0x69, "ADC", Mode.Immediate,	"ADC #{0}"),
			new OpRef(	0x6A, "ROR", Mode.Accumulator,	"ROR A"),
			new OpRef(	0x6C, "JMP", Mode.IndirectAbsolute,	"JMP ({0}{1})"),
			new OpRef(	0x6D, "ADC", Mode.Absolute,		"ADC {0}{1}"),
			new OpRef(	0x6E, "ROR", Mode.Absolute,		"ROR {0}{1}"),
			new OpRef(	0x70, "BVS", Mode.Relative,		"BVS {0}"),
			new OpRef(	0x71, "ADC", Mode.IndirectY,	"ADC ({0}), Y"),
			new OpRef(	0x75, "ADC", Mode.ZeroPageX,	"ADC {0}, X"),
			new OpRef(	0x76, "ROR", Mode.ZeroPageX,	"ROR {0}, X"),
			new OpRef(	0x78, "SEI", Mode.Implied,		"SEI"),
			new OpRef(	0x79, "ADC", Mode.AbsoluteY,	"ADC {0}{1}, Y"),
			new OpRef(	0x7D, "ADC", Mode.AbsoluteX,	"ADC {0}{1}, X"),
			new OpRef(	0x7E, "ROR", Mode.AbsoluteX,	"ROR {0}{1}, X"),
			new OpRef(	0x81, "STA", Mode.IndirectX,	"STA ({0}, X)"),
			new OpRef(	0x84, "STY", Mode.ZeroPage,		"STY {0}"),
			new OpRef(	0x85, "STA", Mode.ZeroPage,		"STA {0}"),
			new OpRef(	0x86, "STX", Mode.ZeroPage,		"STX {0}"),
			new OpRef(	0x88, "DEY", Mode.Implied,		"DEY"),
			new OpRef(	0x8A, "TXA", Mode.Implied,		"TXA"),
			new OpRef(	0x8C, "STY", Mode.Absolute,		"STY {0}{1}"),
			new OpRef(	0x8D, "STA", Mode.Absolute,		"STA {0}{1}"),
			new OpRef(	0x8E, "STX", Mode.Absolute,		"STX {0}{1}"),
			new OpRef(	0x90, "BCC", Mode.Relative,		"BCC {0}"),
			new OpRef(	0x91, "STA", Mode.IndirectY,	"STA ({0}), Y"),
			new OpRef(	0x94, "STY", Mode.ZeroPageX,	"STY {0}, X"),
			new OpRef(	0x95, "STA", Mode.ZeroPageX,	"STA {0}, X"),
			new OpRef(	0x96, "STX", Mode.ZeroPageY,	"STX {0}, Y"),
			new OpRef(	0x98, "TYA", Mode.Implied,		"TYA"),
			new OpRef(	0x99, "STA", Mode.AbsoluteY,	"STA {0}{1}, Y"),
			new OpRef(	0x9A, "TXS", Mode.Implied,		"TXS"),
			new OpRef(	0x9D, "STA", Mode.AbsoluteX,	"STA {0}{1}, X"),
			new OpRef(	0xA0, "LDY", Mode.Immediate,	"LDY #{0}"),
			new OpRef(	0xA1, "LDA", Mode.IndirectX,	"LDA ({0}, X)"),
			new OpRef(	0xA2, "LDX", Mode.Immediate,	"LDX #{0}"),
			new OpRef(	0xA4, "LDY", Mode.ZeroPage,		"LDY {0}"),
			new OpRef(	0xA5, "LDA", Mode.ZeroPage,		"LDA {0}"),
			new OpRef(	0xA6, "LDX", Mode.ZeroPage,		"LDX {0}"),
			new OpRef(	0xA8, "TAY", Mode.Implied,		"TAY"),
			new OpRef(	0xA9, "LDA", Mode.Immediate,	"LDA #{0}"),
			new OpRef(	0xAA, "TAX", Mode.Implied,		"TAX"),
			new OpRef(	0xAC, "LDY", Mode.Absolute,		"LDY {0}{1}"),
			new OpRef(	0xAD, "LDA", Mode.Absolute,		"LDA {0}{1}"),
			new OpRef(	0xAE, "LDX", Mode.Absolute,		"LDX {0}{1}"),
			new OpRef(	0xB0, "BCS", Mode.Relative,		"BCS {0}"),
			new OpRef(	0xB1, "LDA", Mode.IndirectY,	"LDA ({0}), Y"),
			new OpRef(	0xB4, "LDY", Mode.ZeroPageX,	"LDY {0}, X"),
			new OpRef(	0xB5, "LDA", Mode.ZeroPageX,	"LDA {0}, X"),
			new OpRef(	0xB6, "LDX", Mode.ZeroPageY,	"LDX {0}, Y"),
			new OpRef(	0xB8, "CLV", Mode.Implied,		"CLV"),
			new OpRef(	0xB9, "LDA", Mode.AbsoluteY,	"LDA {0}{1}, Y"),
			new OpRef(	0xBA, "TSX", Mode.Implied,		"TSX"),
			new OpRef(	0xBC, "LDY", Mode.AbsoluteX,	"LDY {0}{1}, X"),
			new OpRef(	0xBD, "LDA", Mode.AbsoluteX,	"LDA {0}{1}, X"),
			new OpRef(	0xBE, "LDX", Mode.AbsoluteY,	"LDX {0}{1}, Y"),
			new OpRef(	0xC0, "CPY", Mode.Immediate,	"CPY #{0}"),
			new OpRef(	0xC1, "CMP", Mode.IndirectX,	"CMP ({0}, X)"),
			new OpRef(	0xC4, "CPY", Mode.ZeroPage,		"CPY {0}"),
			new OpRef(	0xC5, "CMP", Mode.ZeroPage,		"CMP {0}"),
			new OpRef(	0xC6, "DEC", Mode.ZeroPage,		"DEC {0}"),
			new OpRef(	0xC8, "INY", Mode.Implied,		"INY"),
			new OpRef(	0xC9, "CMP", Mode.Immediate,	"CMP #{0}"),
			new OpRef(	0xCA, "DEX", Mode.Implied,		"DEX"),
			new OpRef(	0xCC, "CPY", Mode.Absolute,		"CPY {0}{1}"),
			new OpRef(	0xCD, "CMP", Mode.Absolute,		"CMP {0}{1}"),
			new OpRef(	0xCE, "DEC", Mode.Absolute,		"DEC {0}{1}"),
			new OpRef(	0xD0, "BNE", Mode.Relative,		"BNE {0}"),
			new OpRef(	0xD1, "CMP", Mode.IndirectY,	"CMP ({0}), Y"),
			new OpRef(	0xD5, "CMP", Mode.ZeroPageX,	"CMP {0}, X"),
			new OpRef(	0xD6, "DEC", Mode.ZeroPageX,	"DEC {0}, X"),
			new OpRef(	0xD8, "CLD", Mode.Implied,		"CLD"),
			new OpRef(	0xD9, "CMP", Mode.AbsoluteY,	"CMP {0}{1}, Y"),
			new OpRef(	0xDD, "CMP", Mode.AbsoluteX,	"CMP {0}{1}, X"),
			new OpRef(	0xDE, "DEC", Mode.AbsoluteX,	"DEC {0}{1}, X"),
			new OpRef(	0xE0, "CPX", Mode.Immediate,	"CPX #{0}"),
			new OpRef(	0xE1, "SBC", Mode.IndirectX,	"SBC ({0}, X)"),
			new OpRef(	0xE4, "CPX", Mode.ZeroPage,		"CPX {0}"),
			new OpRef(	0xE5, "SBC", Mode.ZeroPage,		"SBC {0}"),
			new OpRef(	0xE6, "INC", Mode.ZeroPage,		"INC {0}"),
			new OpRef(	0xE8, "INX", Mode.Implied,		"INX"),
			new OpRef(	0xE9, "SBC", Mode.Immediate,	"SBC #{0}"),
			new OpRef(	0xEA, "NOP", Mode.Implied,		"NOP"),
			new OpRef(	0xEC, "CPX", Mode.Absolute,		"CPX {0}{1}"),
			new OpRef(	0xED, "SBC", Mode.Absolute,		"SBC {0}{1}"),
			new OpRef(	0xEE, "INC", Mode.Absolute,		"INC {0}{1}"),
			new OpRef(	0xF0, "BEQ", Mode.Relative,		"BEQ {0}"),
			new OpRef(	0xF1, "SBC", Mode.IndirectY,	"SBC ({0}), Y"),
			new OpRef(	0xF5, "SBC", Mode.ZeroPageX,	"SBC {0}, X"),
			new OpRef(	0xF6, "INC", Mode.ZeroPageX,	"INC {0}, X"),
			new OpRef(	0xF8, "SED", Mode.Implied,		"SED"),
			new OpRef(	0xF9, "SBC", Mode.AbsoluteY,	"SBC {0}{1}, Y"),
			new OpRef(	0xFD, "SBC", Mode.AbsoluteX,	"SBC {0}{1}, X"),
			new OpRef(	0xFE, "INC", Mode.AbsoluteX,	"INC {0}{1}, X"),
		};

		public static Dictionary<string, Dictionary<Mode, OpRef>> OC = OpRefs.Select(x => x.Token).Distinct().ToDictionary(x => x, x => OpRefs.Where(y => y.Token == x).ToDictionary(y => y.Mode, y => y));

		#region Flag instructions
		//public static OpCode CLI => OC["CLI"][Mode.Implied].Use();
		public static OpCode SEI => OC["SEI"][Mode.Implied].Use();
		//public static OpCode CLV => OC["CLV"][Mode.Implied].Use();
		public static OpCode CLD => OC["CLD"][Mode.Implied].Use();
		//public static OpCode SED => OC["SED"][Mode.Implied].Use();
		#endregion
		#region Stack instructions
		public static OpCode TXS => OC["TXS"][Mode.Implied].Use();
		public static OpCode TSX => OC["TSX"][Mode.Implied].Use();
		public static OpCode PHA => OC["PHA"][Mode.Implied].Use();
		public static OpCode PLA => OC["PLA"][Mode.Implied].Use();
		public static OpCode PHP => OC["PHP"][Mode.Implied].Use();
		public static OpCode PLP => OC["PLP"][Mode.Implied].Use();
		#endregion
		#region Branching
		public static OpCode BPL => OC["BPL"][Mode.Relative].Use();
		public static OpCode BMI => OC["BMI"][Mode.Relative].Use();
		//public static OpCode BVC => OC["BVC"][Mode.Relative].Use();
		//public static OpCode BVS => OC["BVS"][Mode.Relative].Use();
		public static OpCode BCC => OC["BCC"][Mode.Relative].Use();
		public static OpCode BCS => OC["BCS"][Mode.Relative].Use();
		public static OpCode BNE => OC["BNE"][Mode.Relative].Use();
		public static OpCode BEQ => OC["BEQ"][Mode.Relative].Use();
		#endregion
		#region Jumping and returns
		public static OpCode RTI => OC["RTI"][Mode.Implied].Use();
		public static OpCode RTS => OC["RTS"][Mode.Implied].Use();
		public static class JMP {
			public static OpCode Absolute		{get => new OpCode(0x4C,	3);}
			public static OpCode Indirect		{get => new OpCode(0x6C,	3);}
		}
		public static OpCode JSR => OC["JSR"][Mode.Absolute].Use();
		//public static OpCode BRK => OC["BRK"][Mode.Implied].Use();
		//public static OpCode NOP => OC["NOP"][Mode.Implied].Use();
		#endregion
	}
}
