using System;
using System.Text;

namespace NESSharp.Core.Tools {
	public class OutputNESASM3 : IAssemblerOutput {
		private StringBuilder _output = new StringBuilder();
		public void AppendComment(string comment) {
			_output.Append(';').Append(comment).Append('\n');
		}

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
