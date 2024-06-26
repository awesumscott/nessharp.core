﻿using System;

namespace NESSharp.Core;

//TODO: require a RAM instance for the option of outputting one debug file per RAM remainder
//possible solution: give auto-incrementing IDs to RAM remainders
//accumulate Contents based on their RAM instance ID
//Make a method to output one file per Contents/ID entry. ROM entries may need to be added to all Contents instances.
//This may need RAM instance names to make the debug files easy to identify.
public static class DebugFileNESASM {
	public static string Contents = string.Join('\n',
		new string[]{	"G:2000:PpuControl_2000",
						"G:2001:PpuMask_2001",
						"G:2002:PpuStatus_2002",
						"G:2003:OamAddr_2003",
						"G:2004:OamData_2004",
						"G:2005:PpuScroll_2005",
						"G:2006:PpuAddr_2006",
						"G:2007:PpuData_2007",
						"G:4000:Sq0Duty_4000",
						"G:4001:Sq0Sweep_4001",
						"G:4002:Sq0Timer_4002",
						"G:4003:Sq0Length_4003",
						"G:4004:Sq1Duty_4004",
						"G:4005:Sq1Sweep_4005",
						"G:4006:Sq1Timer_4006",
						"G:4007:Sq1Length_4007",
						"G:4008:TrgLinear_4008",
						"G:400A:TrgTimer_400A",
						"G:400B:TrgLength_400B",
						"G:400C:NoiseVolume_400C",
						"G:400E:NoisePeriod_400E",
						"G:400F:NoiseLength_400F",
						"G:4010:DmcFreq_4010",
						"G:4011:DmcCounter_4011",
						"G:4012:DmcAddress_4012",
						"G:4013:DmcLength_4013",
						"G:4014:SpriteDma_4014",
						"G:4015:ApuStatus_4015",
						"G:4016:Ctrl1_4016",
						"G:4017:Ctrl2_FrameCtr_4017"}) + "\n";

	private static void Write(Address startAddr, Address endAddr, string name) {
		//TODO: consolidation for the four functions below
	}
	public static void WriteVariable(RAMRange ram, Address addr, string name) {
		Contents += $"R:{ addr.ToString().Substring(1) }:{ name }\n";
	}
	public static void WriteVariable(RAMRange ram, Address startAddr, Address endAddr, string name) {
		Contents += $"R:{ startAddr.ToString().Substring(1) }-{ endAddr.ToString().Substring(1) }:{ name }\n";
	}
	public static void WriteLabel(Address addr, string name) {
		if (addr == null) return;
		Contents += $"P:{ addr.ToString().Substring(1) }:{ name }\n";
	}
	public static void WriteLabel(Address startAddr, Address endAddr, string name) {
		Contents += $"P:{ startAddr.ToString().Substring(1) }-{ endAddr.ToString().Substring(1) }:{ name }\n";
	}

	public static string OpToAsm(CPU6502.Asm.OpRef op) =>
		string.Format(
			op.Mode switch {
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
			op.Token
		);
}
