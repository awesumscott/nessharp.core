using System;
using System.Collections.Generic;
using System.Text;

namespace NESSharp.Core {
	public static class DebugFile {
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
		public static void WriteVariable(Address addr, string name) {
			Contents += $"R:{ addr.ToString().Substring(1) }:{ name }\n";
		}
		public static void WriteVariable(Address startAddr, Address endAddr, string name) {
			Contents += $"R:{ startAddr.ToString().Substring(1) }-{ endAddr.ToString().Substring(1) }:{ name }\n";
		}
		public static void WriteLabel(Address addr, string name) {
			if (addr == null) return;
			Contents += $"P:{ addr.ToString().Substring(1) }:{ name }\n";
		}
		public static void WriteLabel(Address startAddr, Address endAddr, string name) {
			Contents += $"P:{ startAddr.ToString().Substring(1) }-{ endAddr.ToString().Substring(1) }:{ name }\n";
		}
	}
}
