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
				headerOpts.PrgRomBanks = 32; //32 * 16KB/bank = 512
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
					headerOpts.ChrRomBanks = 32; //32 * 8KB/bank = 256
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
			Vertical = 0,
			Horizontal = 1
		};

		public class Module : Core.Module {
			private Ptr _irqSub;
			private VByte	_bankSelect, _bankData,
							_mirroring,
							_irqLatch, _irqReload, _irqDisable, _irqEnable;
			[Dependencies]
			public void Dependencies() {
				_irqSub			= Ptr.New(Zp,		$"{nameof(Mapper004)}.{nameof(Module)}{nameof(_irqSub)}");
				_bankSelect		= VByte.Ref(Addr(0x8000));
				_bankData		= VByte.Ref(Addr(0x8001));
				_mirroring		= VByte.Ref(Addr(0xA000));
				_irqLatch		= VByte.Ref(Addr(0xC000));
				_irqReload		= VByte.Ref(Addr(0xC001));
				_irqDisable		= VByte.Ref(Addr(0xE000));
				_irqEnable		= VByte.Ref(Addr(0xE001));
			}
			[Interrupt]
			public void IRQ() {
				Stack.Backup();
				//TODO: if addr != 0
				GoSub(IRQIndirect);
				Stack.Restore();
			} //Just jump back into regular execution
			[CodeSection]
			public void IRQIndirect() => GoTo_Indirect(_irqSub);
			
			private void SetIrqHandler(Action action) {
				_irqSub.Ref(LabelFor(action));
			}
			private void UnsetIrqHandler() {
				_irqSub.Ref(Addr(0));
			}

			private void Disable() => _irqDisable.Set(1); //turn off IRQ
			private void Enable() => _irqEnable.Set(1); //turn on IRQ

			public void QueueIRQ(Action handler, object lines) {
				SetIrqHandler(handler);
				Disable();
				A.Set(lines);
				_irqLatch.Set(A); //count 20 lines
				_irqReload.Set(A);
				Enable();
			}
			public void SetMirroring(Mirroring m) {
				_mirroring.Set((U8)(int)m);
			}
			public void SetChr_2KB(U8 slot, object v) {
				byte slotId = 0;
				switch (slot) {
					case 0: slotId = 0b0; break;
					case 1: slotId = 0b1; break;
				}
				_bankSelect.Set((U8)(0b01000000 | slotId));
				_bankData.Set(v);
			}
			public void SetPrg_Fixed(object v) {
				_bankSelect.Set(0b01000111);
				_bankData.Set(v);
			}
			public void SetPrg_Variable(object v) {
				_bankSelect.Set(0b01000110);
				_bankData.Set(v);
			}
			public void SetChr_1KB(U8 slot, object v) {
				byte slotId = 0;
				switch (slot) {
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
