using System;
using System.Collections.Generic;
using System.Text;

namespace NESSharp.Core {
	public enum MirroringOptions {
		Horizontal = 0,
		MapperControlled = 0,
		Vertical = 1
	};
	public enum ConsoleTypeOptions {
		NES = 0,
		VsSystem = 1,
		Playchoice10 = 2,
		Extended = 3
	};
	public enum TimingOptions {
		NTSC = 0,
		PAL = 1,
		Multi = 2,
		Dendy = 3
	};
	public class HeaderOptions {
		public byte PrgRomBanks = 0; //16 KB banks
		public byte ChrRomBanks = 0; //8 KB banks
		public byte ChrRamBanks = 0;
		public MirroringOptions Mirroring;
		public int Mapper;
		public bool FourScreen = false;
		public bool Trainer = false;
		public byte SubMapper = 0;
		public ConsoleTypeOptions ConsoleType = ConsoleTypeOptions.NES;
		public TimingOptions Timing = TimingOptions.NTSC;

	};
}
