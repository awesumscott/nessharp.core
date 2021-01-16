using System;
using System.Collections.Generic;
using System.Text;

namespace NESSharp.Core.Tools {
	public class OutputNESASM3 : IAssemblerOutput {
		private readonly StringBuilder _output = new StringBuilder();
		private string _fileName = string.Empty;

		public void Setup(string fileName) => _fileName = fileName;
		public void WriteFile(Action<string, string> fileWriteMethod) => fileWriteMethod(_fileName + ".asm", _output.ToString());

		public void AppendBytes(IEnumerable<string> bytes) => _output.Append("\t.db").Append(string.Join(',', bytes)).Append('\n');
		public void AppendComment(string comment) => _output.Append("; ").Append(comment).Append('\n');
		public void AppendLabel(string name) => _output.Append(name).Append(":\n");

		public void AppendOp(Asm.OpRef opref, OpCode opcode) {
			_output	.Append('\t')
					.Append(string.Format(
								string.Format(
									opref.Mode switch {
										Asm.Mode.Immediate			=> "{0} #{{0}}",
										Asm.Mode.Absolute			=> "{0} {{0}}",
										Asm.Mode.ZeroPage			=> "{0} {{0}}",
										Asm.Mode.Implied			=> "{0}",
										Asm.Mode.IndirectAbsolute	=> "{0} ({{0}})",
										Asm.Mode.AbsoluteX			=> "{0} {{0}}, X",
										Asm.Mode.AbsoluteY			=> "{0} {{0}}, Y",
										Asm.Mode.ZeroPageX			=> "{0} {{0}}, X",
										Asm.Mode.ZeroPageY			=> "{0} {{0}}, Y",
										Asm.Mode.IndirectX			=> "{0} ({{0}}, X)",
										Asm.Mode.IndirectY			=> "{0} ({{0}}), Y",
										Asm.Mode.Relative			=> "{0} {{0}}",
										Asm.Mode.Accumulator		=> "{0} A",
										_ => throw new Exception("Invalid addressing mode")
									},
									opref.Token
								), 
								opcode.Param
							)
					)
					.Append('\n');
		}
	}
}
