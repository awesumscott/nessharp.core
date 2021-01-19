using System;
using System.Collections.Generic;
using System.Text;

namespace NESSharp.Core.Mappers {
	public class Mapper000 : IMapper {
		public int Number => 0;
		private readonly MirroringOptions _mirroring;
		
		public Mapper000(MirroringOptions mirroring = MirroringOptions.Vertical) {
			_mirroring = mirroring;
		}

		public void Init(List<Bank> Prg, List<Bank> Chr, HeaderOptions headerOpts) {
			Prg.Add(new Bank(0, MemorySizes.KB_32, 0x8000, true));
			Chr.Add(new Bank(0, MemorySizes.KB_8, 0, true));
			headerOpts.PrgRomBanks = 2;
			headerOpts.ChrRomBanks = 1;
			headerOpts.Mirroring = _mirroring;
		}

		public void WriteInterrupts(List<Bank> Prg, byte[] interrupts, Action<Bank, byte[]> writeFunc) {
			writeFunc(Prg[0], interrupts);
		}
	}
}
