using System;
using System.Collections.Generic;
using System.Text;

namespace NESSharp.Core.Mappers {
	public class Mapper0 : IMapper {
		public int Number => 0;

		public void Init(List<Bank> Prg, List<Bank> Chr, HeaderOptions headerOpts) {
			Prg.Add(new Bank(BankSizes.KB_32, 0x8000, true));
			Chr.Add(new Bank(BankSizes.KB_8, 0, true));
			headerOpts.PrgRomBanks = 2;
			headerOpts.ChrRomBanks = 1;
		}

		public void WriteInterrupts(List<Bank> Prg, byte[] interrupts, Action<Bank, byte[]> writeFunc) {
			writeFunc(Prg[0], interrupts);
		}
	}
}
