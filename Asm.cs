using System;
using System.Collections.Generic;
using System.Linq;

namespace NESSharp.Core {
	public interface IOperation {
		public int Length {get;set;}
	}
	public class OpRaw : IOperation {
		public int Length {get;set;}
		public object[] Value;
		//public OpRaw(U8 v) {
		//	Length = 1;
		//	Value = new object[]{ v };
		//}
		public OpRaw(params byte[] v) {
			Length = v.Length;
			Value = v.Cast<object>().ToArray();
		}
		public OpRaw(object[] v) {
			Length = v.Length;
			Value = v;
		}
	}
	public class OpComment : IOperation {
		public int Length {get;set;}
		public string Text;
		public long ID;
		public OpComment(string text) {
			ID = DateTime.Now.Ticks;
			Text = text;
			Length = 0;
		}
		public override string ToString() => $"#{Text}";
	}
	public class OpCode : IOperation {
		public int Length {get;set;}
		public byte Value;
		public object? Param;
		//public string Description = string.Empty;
		public OpCode(byte opVal, byte len = 1) {
			Value = opVal;
			Length = len;
		}
		public byte[] Output() => new byte[] { Value }; //TODO: output args

		public static implicit operator byte(OpCode o) => o.Value;

		public override string ToString() => $"{Value:X} {Param?.ToString()}";
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
			public OpRef(byte b, string token, Mode mode, string format) {
				Byte = b;
				Token = token;
				Mode = mode;
				Format = format;
			}

			public static OpCode Use(string code, Mode mode) {
				var opcodeRef = OpRefs.FirstOrDefault(x => x.Token == code.ToUpper() && x.Mode == mode);
				if (opcodeRef == null) throw new Exception($"Illegal opcode or addressing mode: { code } { mode }");
				return new OpCode(opcodeRef.Byte, opcodeRef.Length);
			}
			public OpCode Use() => new OpCode(Byte, Length);
		}
		//TODO: replace format strings with generic strings per addressing mode with op as another param. Then make format string lists replaceable to support different assembler syntaxes.
		public static readonly List<OpRef> OpRefs = new List<OpRef>{
			new (	0x00, "BRK", Mode.Implied,		"BRK"),
			new (	0x01, "ORA", Mode.IndirectX,	"ORA ({0}, X)"),
			new (	0x05, "ORA", Mode.ZeroPage,		"ORA {0}"),
			new (	0x06, "ASL", Mode.ZeroPage,		"ASL {0}"),
			new (	0x08, "PHP", Mode.Implied,		"PHP"),
			new (	0x09, "ORA", Mode.Immediate,	"ORA #{0}"),
			new (	0x0A, "ASL", Mode.Accumulator,	"ASL A"),
			new (	0x0D, "ORA", Mode.Absolute,		"ORA {0}"),
			new (	0x0E, "ASL", Mode.Absolute,		"ASL {0}"),
			new (	0x10, "BPL", Mode.Relative,		"BPL {0}"),
			new (	0x11, "ORA", Mode.IndirectY,	"ORA ({0}), Y"),
			new (	0x15, "ORA", Mode.ZeroPageX,	"ORA {0}, X"),
			new (	0x16, "ASL", Mode.ZeroPageX,	"ASL {0}, X"),
			new (	0x18, "CLC", Mode.Implied,		"CLC"),
			new (	0x19, "ORA", Mode.AbsoluteY,	"ORA {0}, Y"),
			new (	0x1D, "ORA", Mode.AbsoluteX,	"ORA {0}, X"),
			new (	0x1E, "ASL", Mode.AbsoluteX,	"ASL {0}, X"),
			new (	0x20, "JSR", Mode.Absolute,		"JSR {0}"),
			new (	0x21, "AND", Mode.IndirectX,	"AND ({0}, X)"),
			new (	0x24, "BIT", Mode.ZeroPage,		"BIT {0}"),
			new (	0x25, "AND", Mode.ZeroPage,		"AND {0}"),
			new (	0x26, "ROL", Mode.ZeroPage,		"ROL {0}"),
			new (	0x28, "PLP", Mode.Implied,		"PLP"),
			new (	0x29, "AND", Mode.Immediate,	"AND #{0}"),
			new (	0x2A, "ROL", Mode.Accumulator,	"ROL A"),
			new (	0x2C, "BIT", Mode.Absolute,		"BIT {0}"),
			new (	0x2D, "AND", Mode.Absolute,		"AND {0}"),
			new (	0x2E, "ROL", Mode.Absolute,		"ROL {0}"),
			new (	0x30, "BMI", Mode.Relative,		"BMI {0}"),
			new (	0x31, "AND", Mode.IndirectY,	"AND ({0}), Y"),
			new (	0x35, "AND", Mode.ZeroPageX,	"AND {0}, X"),
			new (	0x36, "ROL", Mode.ZeroPageX,	"ROL {0}, X"),
			new (	0x38, "SEC", Mode.Implied,		"SEC"),
			new (	0x39, "AND", Mode.AbsoluteY,	"AND {0}, Y"),
			new (	0x3D, "AND", Mode.AbsoluteX,	"AND {0}, X"),
			new (	0x3E, "ROL", Mode.AbsoluteX,	"ROL {0}, X"),
			new (	0x40, "RTI", Mode.Implied,		"RTI"),
			new (	0x41, "EOR", Mode.IndirectX,	"EOR ({0}, X)"),
			new (	0x45, "EOR", Mode.ZeroPage,		"EOR {0}"),
			new (	0x46, "LSR", Mode.ZeroPage,		"LSR {0}"),
			new (	0x48, "PHA", Mode.Implied,		"PHA"),
			new (	0x49, "EOR", Mode.Immediate,	"EOR #{0}"),
			new (	0x4A, "LSR", Mode.Accumulator,	"LSR A"),
			new (	0x4C, "JMP", Mode.Absolute,		"JMP {0}"),
			new (	0x4D, "EOR", Mode.Absolute,		"EOR {0}"),
			new (	0x4E, "LSR", Mode.Absolute,		"LSR {0}"),
			new (	0x50, "BVC", Mode.Relative,		"BVC {0}"),
			new (	0x51, "EOR", Mode.IndirectY,	"EOR ({0}), Y"),
			new (	0x55, "EOR", Mode.ZeroPageX,	"EOR {0}, X"),
			new (	0x56, "LSR", Mode.ZeroPageX,	"LSR {0}, X"),
			new (	0x58, "CLI", Mode.Implied,		"CLI"),
			new (	0x59, "EOR", Mode.AbsoluteY,	"EOR {0}, Y"),
			new (	0x5D, "EOR", Mode.AbsoluteX,	"EOR {0}, X"),
			new (	0x5E, "LSR", Mode.AbsoluteX,	"LSR {0}, X"),
			new (	0x60, "RTS", Mode.Implied,		"RTS"),
			new (	0x61, "ADC", Mode.IndirectX,	"ADC ({0}, X)"),
			new (	0x65, "ADC", Mode.ZeroPage,		"ADC {0}"),
			new (	0x66, "ROR", Mode.ZeroPage,		"ROR {0}"),
			new (	0x68, "PLA", Mode.Implied,		"PLA"),
			new (	0x69, "ADC", Mode.Immediate,	"ADC #{0}"),
			new (	0x6A, "ROR", Mode.Accumulator,	"ROR A"),
			new (	0x6C, "JMP", Mode.IndirectAbsolute,	"JMP ({0})"),
			new (	0x6D, "ADC", Mode.Absolute,		"ADC {0}"),
			new (	0x6E, "ROR", Mode.Absolute,		"ROR {0}"),
			new (	0x70, "BVS", Mode.Relative,		"BVS {0}"),
			new (	0x71, "ADC", Mode.IndirectY,	"ADC ({0}), Y"),
			new (	0x75, "ADC", Mode.ZeroPageX,	"ADC {0}, X"),
			new (	0x76, "ROR", Mode.ZeroPageX,	"ROR {0}, X"),
			new (	0x78, "SEI", Mode.Implied,		"SEI"),
			new (	0x79, "ADC", Mode.AbsoluteY,	"ADC {0}, Y"),
			new (	0x7D, "ADC", Mode.AbsoluteX,	"ADC {0}, X"),
			new (	0x7E, "ROR", Mode.AbsoluteX,	"ROR {0}, X"),
			new (	0x81, "STA", Mode.IndirectX,	"STA ({0}, X)"),
			new (	0x84, "STY", Mode.ZeroPage,		"STY {0}"),
			new (	0x85, "STA", Mode.ZeroPage,		"STA {0}"),
			new (	0x86, "STX", Mode.ZeroPage,		"STX {0}"),
			new (	0x88, "DEY", Mode.Implied,		"DEY"),
			new (	0x8A, "TXA", Mode.Implied,		"TXA"),
			new (	0x8C, "STY", Mode.Absolute,		"STY {0}"),
			new (	0x8D, "STA", Mode.Absolute,		"STA {0}"),
			new (	0x8E, "STX", Mode.Absolute,		"STX {0}"),
			new (	0x90, "BCC", Mode.Relative,		"BCC {0}"),
			new (	0x91, "STA", Mode.IndirectY,	"STA ({0}), Y"),
			new (	0x94, "STY", Mode.ZeroPageX,	"STY {0}, X"),
			new (	0x95, "STA", Mode.ZeroPageX,	"STA {0}, X"),
			new (	0x96, "STX", Mode.ZeroPageY,	"STX {0}, Y"),
			new (	0x98, "TYA", Mode.Implied,		"TYA"),
			new (	0x99, "STA", Mode.AbsoluteY,	"STA {0}, Y"),
			new (	0x9A, "TXS", Mode.Implied,		"TXS"),
			new (	0x9D, "STA", Mode.AbsoluteX,	"STA {0}, X"),
			new (	0xA0, "LDY", Mode.Immediate,	"LDY #{0}"),
			new (	0xA1, "LDA", Mode.IndirectX,	"LDA ({0}, X)"),
			new (	0xA2, "LDX", Mode.Immediate,	"LDX #{0}"),
			new (	0xA4, "LDY", Mode.ZeroPage,		"LDY {0}"),
			new (	0xA5, "LDA", Mode.ZeroPage,		"LDA {0}"),
			new (	0xA6, "LDX", Mode.ZeroPage,		"LDX {0}"),
			new (	0xA8, "TAY", Mode.Implied,		"TAY"),
			new (	0xA9, "LDA", Mode.Immediate,	"LDA #{0}"),
			new (	0xAA, "TAX", Mode.Implied,		"TAX"),
			new (	0xAC, "LDY", Mode.Absolute,		"LDY {0}"),
			new (	0xAD, "LDA", Mode.Absolute,		"LDA {0}"),
			new (	0xAE, "LDX", Mode.Absolute,		"LDX {0}"),
			new (	0xB0, "BCS", Mode.Relative,		"BCS {0}"),
			new (	0xB1, "LDA", Mode.IndirectY,	"LDA ({0}), Y"),
			new (	0xB4, "LDY", Mode.ZeroPageX,	"LDY {0}, X"),
			new (	0xB5, "LDA", Mode.ZeroPageX,	"LDA {0}, X"),
			new (	0xB6, "LDX", Mode.ZeroPageY,	"LDX {0}, Y"),
			new (	0xB8, "CLV", Mode.Implied,		"CLV"),
			new (	0xB9, "LDA", Mode.AbsoluteY,	"LDA {0}, Y"),
			new (	0xBA, "TSX", Mode.Implied,		"TSX"),
			new (	0xBC, "LDY", Mode.AbsoluteX,	"LDY {0}, X"),
			new (	0xBD, "LDA", Mode.AbsoluteX,	"LDA {0}, X"),
			new (	0xBE, "LDX", Mode.AbsoluteY,	"LDX {0}, Y"),
			new (	0xC0, "CPY", Mode.Immediate,	"CPY #{0}"),
			new (	0xC1, "CMP", Mode.IndirectX,	"CMP ({0}, X)"),
			new (	0xC4, "CPY", Mode.ZeroPage,		"CPY {0}"),
			new (	0xC5, "CMP", Mode.ZeroPage,		"CMP {0}"),
			new (	0xC6, "DEC", Mode.ZeroPage,		"DEC {0}"),
			new (	0xC8, "INY", Mode.Implied,		"INY"),
			new (	0xC9, "CMP", Mode.Immediate,	"CMP #{0}"),
			new (	0xCA, "DEX", Mode.Implied,		"DEX"),
			new (	0xCC, "CPY", Mode.Absolute,		"CPY {0}"),
			new (	0xCD, "CMP", Mode.Absolute,		"CMP {0}"),
			new (	0xCE, "DEC", Mode.Absolute,		"DEC {0}"),
			new (	0xD0, "BNE", Mode.Relative,		"BNE {0}"),
			new (	0xD1, "CMP", Mode.IndirectY,	"CMP ({0}), Y"),
			new (	0xD5, "CMP", Mode.ZeroPageX,	"CMP {0}, X"),
			new (	0xD6, "DEC", Mode.ZeroPageX,	"DEC {0}, X"),
			new (	0xD8, "CLD", Mode.Implied,		"CLD"),
			new (	0xD9, "CMP", Mode.AbsoluteY,	"CMP {0}, Y"),
			new (	0xDD, "CMP", Mode.AbsoluteX,	"CMP {0}, X"),
			new (	0xDE, "DEC", Mode.AbsoluteX,	"DEC {0}, X"),
			new (	0xE0, "CPX", Mode.Immediate,	"CPX #{0}"),
			new (	0xE1, "SBC", Mode.IndirectX,	"SBC ({0}, X)"),
			new (	0xE4, "CPX", Mode.ZeroPage,		"CPX {0}"),
			new (	0xE5, "SBC", Mode.ZeroPage,		"SBC {0}"),
			new (	0xE6, "INC", Mode.ZeroPage,		"INC {0}"),
			new (	0xE8, "INX", Mode.Implied,		"INX"),
			new (	0xE9, "SBC", Mode.Immediate,	"SBC #{0}"),
			new (	0xEA, "NOP", Mode.Implied,		"NOP"),
			new (	0xEC, "CPX", Mode.Absolute,		"CPX {0}"),
			new (	0xED, "SBC", Mode.Absolute,		"SBC {0}"),
			new (	0xEE, "INC", Mode.Absolute,		"INC {0}"),
			new (	0xF0, "BEQ", Mode.Relative,		"BEQ {0}"),
			new (	0xF1, "SBC", Mode.IndirectY,	"SBC ({0}), Y"),
			new (	0xF5, "SBC", Mode.ZeroPageX,	"SBC {0}, X"),
			new (	0xF6, "INC", Mode.ZeroPageX,	"INC {0}, X"),
			new (	0xF8, "SED", Mode.Implied,		"SED"),
			new (	0xF9, "SBC", Mode.AbsoluteY,	"SBC {0}, Y"),
			new (	0xFD, "SBC", Mode.AbsoluteX,	"SBC {0}, X"),
			new (	0xFE, "INC", Mode.AbsoluteX,	"INC {0}, X"),
		};

		public static Dictionary<string, Dictionary<Mode, OpRef>> OC = OpRefs.Select(x => x.Token).Distinct().ToDictionary(x => x, x => OpRefs.Where(y => y.Token == x).ToDictionary(y => y.Mode, y => y));
	}
}
