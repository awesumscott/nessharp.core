using System;
using System.Collections.Generic;
using System.Text;

namespace NESSharp.Core.Mappers {
	public class Mapper30 : IMapper {
		public int Number => 30;

		public void Init(List<Bank> Prg, List<Bank> Chr, HeaderOptions headerOpts) {
			for (var i = 0; i < 31; i++) {
				Prg.Add(new Bank(BankSizes.KB_16, 0x8000));
			}
			Prg.Add(new Bank(BankSizes.KB_16, 0xC000, true));
			headerOpts.PrgRomBanks = 32;
			headerOpts.ChrRomBanks = 0;
		}

		public void WriteInterrupts(List<Bank> Prg, byte[] interrupts, Action<Bank, byte[]> writeFunc) {
			writeFunc(Prg[31], interrupts);
		}
	}
}
