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
		//public IOperand? Param;
		public object? Param;
		public OpCode(byte opVal, byte len = 1) {
			Value = opVal;
			Length = len;
		}
		//public byte[] Output() => new byte[] { Value }; //TODO: output args
		//public static implicit operator byte(OpCode o) => o.Value;

		public override string ToString() => throw new Exception("OpCode.ToString called");//$"{Value:X} {Param?.ToString()}";
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
			public OpCode Use() => new OpCode(Byte, Length);
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
