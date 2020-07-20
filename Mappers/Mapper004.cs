using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static NESSharp.Core.AL;

/*
	PRG-ROM Sizes:			128-512KB
	CHR-ROM or CHR-RAM
	CHR-ROM Sizes:			128-256KB
	CHR-RAM Sizes:			8KB
	WRAM:					8KB				Provide RAM instance for range $6000-$7FFF
	Battery-backed WRAM bool


	PRG-ROM:
		I believe it may be easier to leave bank select bit 6 set to 1, and have 0x8000 fixed and 0xC000 swappable, so DPCM data can go in 0xC000.
		512KB:
			64 banks total. 2 fixed (063 @ 0xE000 and 062 @ {0x8000 | 0xC000}), and 62 divided between two locations (0xA000 and {0x8000 | 0xC000}).
		256KB:
			32 banks total. 2 fixed (031 @ 0xE000 and 030 @ {0x8000 | 0xC000}), and 30 divided between two locations (0xA000 and {0x8000 | 0xC000}).
		128KB:
			16 banks total. 2 fixed (015 @ 0xE000 and 014 @ {0x8000 | 0xC000}), and 14 divided between two locations (0xA000 and {0x8000 | 0xC000}).
	CHR-ROM:
		256KB:
			32 banks total. 2 fixed (031 @ 0xE000 and 030 @ {0x8000 | 0xC000}), and 30 divided between two locations (0xA000 and {0x8000 | 0xC000}).
		128KB:
			16 banks total. 2 fixed (015 @ 0xE000 and 014 @ {0x8000 | 0xC000}), and 14 divided between two locations (0xA000 and {0x8000 | 0xC000}).
*/

namespace NESSharp.Core.Mappers {
	public class Mapper004 : IMapper {
		public int Number => 4;
		public bool BatteryBacked = false;
		public RAM WRAM;
		public Label[] ChrLabel;
		private int _prgRom, _chrRom, _chrRam, _wRam;

		public Mapper004(int PrgROMSize, int ChrROMSize, int ChrRAMSize, int WRAMSize, bool batteryBacked = false) {
			_prgRom = PrgROMSize;
			_chrRom = ChrROMSize;
			_chrRam = ChrRAMSize;
			_wRam = WRAMSize;
			BatteryBacked = batteryBacked;
		}

		public void Init(List<Bank> Prg, List<Bank> Chr, HeaderOptions headerOpts) {
			if (_prgRom == MemorySizes.KB_512) {
				//TODO: two of these loops divided between 0xA000 and {0x8000 | 0xC000}
				for (var i = 0; i < 62; i++) {
					Prg.Add(new Bank(MemorySizes.KB_8, 0x8000));
				}
				Prg.Add(new Bank(MemorySizes.KB_8, 0xC000, true));
				Prg.Add(new Bank(MemorySizes.KB_8, 0xE000, true));
				headerOpts.PrgRomBanks = 32; //32 * 16 = 512
			} else throw new NotImplementedException(); //TODO: wait until 512 works to finish the rest
			/*else if (_prgRom == MemorySizes.KB_256) {
				for (var i = 0; i < 31; i++) {
					Prg.Add(new Bank(MemorySizes.KB_16, 0x8000));
				}
				Prg.Add(new Bank(MemorySizes.KB_16, 0xC000, true));
				headerOpts.PrgRomBanks = 16; //16 * 16 = 256
			} else if (_prgRom == MemorySizes.KB_128) {
				for (var i = 0; i < 31; i++) {
					Prg.Add(new Bank(MemorySizes.KB_16, 0x8000));
				}
				Prg.Add(new Bank(MemorySizes.KB_16, 0xC000, true));
				headerOpts.PrgRomBanks = 16; //16 * 16 = 256
			}*/

			if (_chrRom > 0) {
				if (_chrRom == MemorySizes.KB_256) {
					
					for (var i = 0; i < 256; i++) {
						Chr.Add(new Bank(MemorySizes.KB_1, 0x0000));
					}
					headerOpts.ChrRomBanks = 16; //16 * 16 = 256
				} else throw new NotImplementedException(); //TODO: wait until 512 works to finish the rest
			}

			if (_wRam > 0) {
				WRAM = new RAM(Addr(0x6000), Addr(0x7FFF));
			}
			//headerOpts.
		}

		public void WriteInterrupts(List<Bank> Prg, byte[] interrupts, Action<Bank, byte[]> writeFunc) {
			writeFunc(Prg.Last(), interrupts);
		}
	}
}
