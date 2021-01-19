using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static NESSharp.Core.AL;

/*
	PRG-ROM Sizes:			32-512KB
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
	public class Mapper001 : IMapper {
		public int Number => 4;
		public bool BatteryBacked = false;
		public RAMRange WRAM;
		public Label[] ChrLabel;
		private int _prgRom, _chrRom, _chrRam, _wRam;

		public Mapper001(int PrgROMSize, int ChrROMSize, int ChrRAMSize, int WRAMSize, bool batteryBacked = false) {
			_prgRom = PrgROMSize;
			_chrRom = ChrROMSize;
			_chrRam = ChrRAMSize;
			_wRam = WRAMSize;
			BatteryBacked = batteryBacked;
		}

		public void Init(List<Bank> Prg, List<Bank> Chr, HeaderOptions headerOpts) {
			if (_prgRom == MemorySizes.KB_512) {
				//TODO: two of these loops divided between 0xA000 and {0x8000 | 0xC000}
				U8 i;
				for (i = 0; i < 62; i++) {
					Prg.Add(new Bank(i, MemorySizes.KB_8, 0x8000));
				}
				Prg.Add(new Bank(++i, MemorySizes.KB_8, 0xC000, true));
				Prg.Add(new Bank(++i, MemorySizes.KB_8, 0xE000, true));
				headerOpts.PrgRomBanks = 32; //32 * 16KB/bank = 512
			} else throw new NotImplementedException(); //TODO: wait until 512 works to finish the rest

			if (_chrRom > 0) {
				if (_chrRom == MemorySizes.KB_256) {
					U8 i;
					for (i = 0; i < 256; i++) {
						Chr.Add(new Bank(i, MemorySizes.KB_1, 0x0000));
					}
					headerOpts.ChrRomBanks = 32; //32 * 8KB/bank = 256
				} else throw new NotImplementedException(); //TODO: wait until 512 works to finish the rest
			}

			if (_wRam > 0) {
				WRAM = new RAMRange(Addr(0x6000), Addr(0x7FFF), "WRAM");
			}
			//headerOpts.
		}

		public void WriteInterrupts(List<Bank> Prg, byte[] interrupts, Action<Bank, byte[]> writeFunc) {
			writeFunc(Prg.Last(), interrupts);
		}

		////Call in VBlank
		//public void EnableIRQ() {
		//	CPU6502.CLI();
		//	//during vblank
		//	Addr(0xE000).Set(1); //turn off IRQ
		//	Addr(0xC000).Set(40); //count 20 lines
		//	Addr(0xC001).Set(40);
		//	Addr(0xE001).Set(1); //turn on IRQ
		//}

		public enum Mirroring {
			Horizontal = 0,
			Vertical = 1
		};

		public class Module : Core.Module {
			private VByte	_bankSelect, _bankData,
							_mirroring;
			[Dependencies]
			public void Dependencies() {
				_bankSelect		= VByte.Ref(0x8000, nameof(_bankSelect));
				_bankData		= VByte.Ref(0x8001, nameof(_bankData));
				_mirroring		= VByte.Ref(0xA000, nameof(_mirroring));
			}

			public void SetMirroring(Mirroring m) {
				_mirroring.Set((U8)(int)m);
			}
			public void SetChr_2KB(U8 slot, IOperand v) {
				byte slotId = 0;
				switch ((int)slot) {
					case 0: slotId = 0b0; break;
					case 1: slotId = 0b1; break;
				}
				_bankSelect.Set((U8)(0b01000000 | slotId));
				_bankData.Set(v);
			}
			public void SetPrg_Fixed(IOperand v) {
				_bankSelect.Set(0b01000111);
				_bankData.Set(v);
			}
			public void SetPrg_Variable(IOperand v) {
				_bankSelect.Set(0b01000110);
				_bankData.Set(v);
			}
			public void SetChr_1KB(U8 slot, IOperand v) {
				byte slotId = 0;
				switch ((int)slot) {
					case 0: slotId = 0b10; break;
					case 1: slotId = 0b11; break;
					case 2: slotId = 0b100; break;
					case 3: slotId = 0b101; break;
				}
				_bankSelect.Set((U8)(0b01000000 | slotId));
				_bankData.Set(v);
			}
		}
	}
}
