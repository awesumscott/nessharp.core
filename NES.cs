using System;
using System.Collections.Generic;
using System.Text;
using static NESSharp.Core.AL;

namespace NESSharp.Core {
	public static class NES {
		public static RAM ram;
		public static RAM zp;
		public static RAM StackRam; //eliminate stack page from possible allocations

		public static void Init() {
			ram				= new RAM(Addr(0), Addr(0x07FF));
			zp				= ram.Allocate(Addr(0), Addr(0xFF));
			StackRam		= ram.Allocate(Addr(0x0100), Addr(0x01FF));
			ShadowOAM.Ram	= ram.Allocate(Addr(0x0200), Addr(0x02FF));
		}
		public static class ShadowOAM {
			public static RAM Ram; //eliminate shadow OAM from possible allocations
		}
		public static class MemoryMap {
			public static Address Palette		= Addr(0x3F00);
			public static Address Background	= Addr(0x2000);
			public static Address Attributes	= Addr(0x23C0);
		}
		public static class IRQ {
			public static void Disable() => CPU6502.SEI();
		}
		public static class PPU {
			[Flags]
			public enum ControlFlags : byte {
				NameTable0		= 0b00000000,
				NameTable1		= 0b00000001,
				NameTable2		= 0b00000010,
				NameTable3		= 0b00000011,
				IncAcross		= 0,
				IncDown			= 0b00000100,
				SpritePT0		= 0,
				SpritePT1		= 0b00001000,
				BackgroundPT0	= 0,
				BackgroundPT1	= 0b00010000,
				Sprite8x8		= 0,
				Sprite8x16		= 0b00100000,
				ReadEXT			= 0,
				WriteEXT		= 0b01000000,
				NMIDisabled		= 0,
				NMIEnabled		= 0b10000000,
			};
			public static Address	Control =			Addr(0x2000);
			public static VByte		LazyControl =		VByte.New(zp, "ppuLazyCtrl");
			public static VByte		LazyMask =			VByte.New(zp, "ppuLazyMask");
			public static Address	Mask =				Addr(0x2001);
			public static Address	Status =			Addr(0x2002);
			public static Address	Scroll =			Addr(0x2005);
			public static VByte		LazyScrollX =		VByte.New(zp, "ppuLazyScrollX");
			public static VByte		LazyScrollY =		VByte.New(zp, "ppuLazyScrollY");
			public static Bus	Address =			Bus.Ref(0x2006);
			public static Bus	Data =				Bus.Ref(0x2007);
			//public static void ScrollTo(U8 x, U8 y) {
			//	Scroll.Set(x);
			//	Scroll.Set(y);
			//}
			//public static void ScrollTo(Address x, Address y) {
			//	Scroll.Set(x);
			//	Scroll.Set(y);
			//}
			public static void ScrollTo(IOperand x, IOperand y) {
				Scroll.Set(x);
				Scroll.Set(y);
			}
			public static void ScrollTo(U8 x, U8 y) => ScrollTo((IOperand)x, (IOperand)y);
			public static void Reset() => A.Set(Status); //read PPU status to reset the high / low latch
			public static void SetHorizontalWrite() => Control.Set(LazyControl.Set(z => z.And(0b11111011)));
			public static void SetVerticalWrite() => Control.Set(LazyControl.Set(z => z.Or(0b100)));
			public static void SetAddress(U16 addr) {
				Reset();
				Address.Write(addr.Hi, addr.Lo);
			}
			public static void SetAddress(IVarAddressArray iva) {
				if (iva.Address.Length != 2) throw new ArgumentException();
				Reset();
				Address.Write(iva.Address[1], iva.Address[0]);
			}
			public static class OAM {
				public static Address Address =			Addr(0x2003);
				public static Address Data =			Addr(0x2004);   //Don't worry about this; let OAM_DMA do the work for you.
				public static Address DMA =				Addr(0x4014);
				public static void Write(Address shadowOam) {
					Address.Set(shadowOam.Lo);	//low byte of RAM address
					DMA.Set(shadowOam.Hi);		//high byte of RAM address
				}
			}
			public static void ClearNametable0(U8 val) {
				SetHorizontalWrite();
				SetAddress(0x2000);
				Loop.Repeat(X.Set(0), 256, _ => {
					Data.Set(val);
					Data.Set(val);
					Data.Set(val);
					Data.Set(val);
				});
				//Attribute table is already covered by the above loop
				//NES.PPU.Address.Set(0x23);
				//NES.PPU.Address.Set(0xC0);
				//RepeatX(0, 16, () => {
				//	NES.PPU.Data.Set(0);
				//	NES.PPU.Data.Set(0);
				//	NES.PPU.Data.Set(0);
				//	NES.PPU.Data.Set(0);
				//});
			}
			public static void ClearNametable2(U8 val) {
				SetHorizontalWrite();
				SetAddress(0x2800);
				Loop.Repeat(X.Set(0), 256, _ => {
					Data.Set(val);
					Data.Set(val);
					Data.Set(val);
					Data.Set(val);
				});
			}
		}
		public static class APU {
				
			public static class Pulse1 {
				public static Address Volume =			Addr(0x4000);
				public static Address Sweep =			Addr(0x4001);
				public static Address Lo =				Addr(0x4002);
				public static Address Hi =				Addr(0x4003);
			}
			public static class Pulse2 {
				public static Address Volume =			Addr(0x4004);
				public static Address Sweep =			Addr(0x4005);
				public static Address Lo =				Addr(0x4006);
				public static Address Hi =				Addr(0x4007);
			}
			public static class Triangle {
				public static Address Linear =			Addr(0x4008);
				public static Address Lo =				Addr(0x400A);
				public static Address Hi =				Addr(0x400B);
			}
			public static class Noise {
				public static Address Volume =			Addr(0x400C);
				public static Address Lo =				Addr(0x400E);
				public static Address Hi =				Addr(0x400F);
			}
			public static class DMC {
				public static Address Settings =		Addr(0x4010);
				public static Address LoadCounter =		Addr(0x4011);
				public static Address SampleAddress =	Addr(0x4012);
				public static Address SampleLength =	Addr(0x4013);

				public static void Disable() => Settings.Set(0);
			}
			public static Bus Status =				Bus.Ref(0x4015);
			public static Address FrameCounter =		Addr(0x4017);
			public enum Channels : byte {
				Pulse1 =	0b00001,
				Pulse2 =	0b00010,
				Triangle =	0b00100,
				Noise =		0b01000,
				DMC =		0b10000,
			}
			public static void SetChannelsEnabled(Channels chs) => Status.Set((byte)chs);
		}
		public static class Controller {
			public static Address One =		Addr(0x4016);
			public static Address Two =		Addr(0x4017);
			public static void Latch() {
				One.Set(1);
				One.Set(0);
			}
		}
		public static class Button {
			public static U8 Right =	0b00000001;
			public static U8 Left =		0b00000010;
			public static U8 Down =		0b00000100;
			public static U8 Up =		0b00001000;
			public static U8 Start =	0b00010000;
			public static U8 Select =	0b00100000;
			public static U8 B =		0b01000000;
			public static U8 A =		0b10000000;
		}
	}
}
