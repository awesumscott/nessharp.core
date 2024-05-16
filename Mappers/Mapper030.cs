using System;
using System.Collections.Generic;

namespace NESSharp.Core.Mappers;

public class Mapper030 : IMapper {
	public int Number => 30;

	public Mapper030() {
		 
	}

	public void Init(List<Bank> Prg, List<Bank> Chr, HeaderOptions headerOpts) {
		U8 i;
		for (i = 0; i < 31; i++) {
			Prg.Add(new Bank(i, MemorySizes.KB_16, 0x8000));
		}
		Prg.Add(new Bank(++i, MemorySizes.KB_16, 0xC000, true));
		headerOpts.PrgRomBanks = 32;
		headerOpts.ChrRomBanks = 0;
		
		 //headerOpts.MapperControlledMirroring: 00001001 = 4 screen
	}

	public void WriteInterrupts(List<Bank> Prg, byte[] interrupts, Action<Bank, byte[]> writeFunc) {
		writeFunc(Prg[31], interrupts);
	}
}
