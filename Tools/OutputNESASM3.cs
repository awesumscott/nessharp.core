using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NESSharp.Core.Tools {
	public class OutputNESASM3 : IAssemblerOutput, IFileLogTool {
		public class Formatting : INESAsmFormatting {
			public string AddressFormat			=> "${0}";
			public string OperandLow			=> "<{0}";
			public string OperandHigh			=> ">{0}";
			public string ResolveLow			=> "LOW({0})";
			public string ResolveHigh			=> "HIGH({0})";
			public string ExpressionAdd			=> "{0}+{1}";
			public string ExpressionSubtract	=> "{0}-{1}";
			public string ExpressionLShift		=> "{0} << {1}";
			public string ExpressionRShift		=> "{0} >> {1}";
		}
		private Formatting _nesasm3Formatter = new();
		private readonly StringBuilder _output = new();
		private string _fileName = string.Empty;

		public void Setup(string fileName) => _fileName = fileName;
		public void WriteFile(Action<string, string> fileWriteMethod) => fileWriteMethod(_fileName + ".asm", _output.ToString());

		public void AppendBytes(IEnumerable<string> bytes) => _output.Append("\t.db ").Append(string.Join(',', bytes)).Append('\n');
		public void AppendComment(string comment) => _output.Append("; ").Append(comment).Append('\n');
		public void AppendLabel(string name) => _output.Append(name).Append(":\n");
		public void AppendOp(OpCode opCode) {
			var opRef = CPU6502.Asm.OpRefs.Where(x => x.Byte == opCode.Value).First();
			var opString = string.Format(
				opRef.Mode switch {
					CPU6502.Asm.Mode.Immediate			=> "{0} #{{0}}",
					CPU6502.Asm.Mode.Absolute			=> "{0} {{0}}",
					CPU6502.Asm.Mode.ZeroPage			=> "{0} {{0}}",
					CPU6502.Asm.Mode.Implied			=> "{0}",
					CPU6502.Asm.Mode.IndirectAbsolute	=> "{0} ({{0}})",
					CPU6502.Asm.Mode.AbsoluteX			=> "{0} {{0}}, X",
					CPU6502.Asm.Mode.AbsoluteY			=> "{0} {{0}}, Y",
					CPU6502.Asm.Mode.ZeroPageX			=> "{0} {{0}}, X",
					CPU6502.Asm.Mode.ZeroPageY			=> "{0} {{0}}, Y",
					CPU6502.Asm.Mode.IndirectX			=> "{0} ({{0}}, X)",
					CPU6502.Asm.Mode.IndirectY			=> "{0} ({{0}}), Y",
					CPU6502.Asm.Mode.Relative			=> "{0} {{0}}",
					CPU6502.Asm.Mode.Accumulator		=> "{0} A",
					_ => throw new Exception("Invalid addressing mode")
				},
				opRef.Token
			);
			_output	.Append('\t')
					.Append(opCode.Param == null ? opString : string.Format(
								opString, 
								opCode.Param.ToAsmString(_nesasm3Formatter)
							)
					)
					.Append('\n');
		}
	}
}
