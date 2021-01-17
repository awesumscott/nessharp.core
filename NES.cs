using System;
using static NESSharp.Core.AL;

namespace NESSharp.Core {
	public static class NES {
		public static RAM Mem;
		public static RAMRange ram;
		public static RAMRange zp;
		public static RAMRange StackRam; //eliminate stack page from possible allocations

		public static void Init() {
			ram				= new RAMRange(0x0000, 0x07FF, "Main");
			zp				= ram.Allocate(0x0000, 0x00FF, "ZP");
			StackRam		= ram.Allocate(0x0100, 0x01FF, "Stack");
			ShadowOAM.Ram	= ram.Allocate(0x0200, 0x02FF, "Shadow OAM");

			Mem = new RAM(zp, ram);

			PPU.OAM.Init();
		}
		public static class ShadowOAM {
			public static RAMRange Ram; //eliminate shadow OAM from possible allocations
		}
		public static class MemoryMap {
			public static readonly Address Palette		= 0x3F00;
			public static readonly Address Background	= 0x2000;
			public static readonly Address Attributes	= 0x23C0;
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
			public static readonly VByte	Control =		VByte.Ref(0x2000,	$"{nameof(PPU)}_{nameof(Control)}");
			public static readonly VByte	LazyControl =	VByte.New(zp,		$"{nameof(PPU)}_{nameof(LazyControl)}");
			public static readonly VByte	Mask =			VByte.Ref(0x2001,	$"{nameof(PPU)}_{nameof(Mask)}");
			public static readonly VByte	LazyMask =		VByte.New(zp,		$"{nameof(PPU)}_{nameof(LazyMask)}");
			public static readonly VByte	Status =		VByte.Ref(0x2002,	$"{nameof(PPU)}_{nameof(Status)}");
			public static readonly VByte	Scroll =		VByte.Ref(0x2005,	$"{nameof(PPU)}_{nameof(Scroll)}");
			public static readonly VByte	LazyScrollX =	VByte.New(zp,		$"{nameof(PPU)}_{nameof(LazyScrollX)}");
			public static readonly VByte	LazyScrollY =	VByte.New(zp,		$"{nameof(PPU)}_{nameof(LazyScrollY)}");
			public static readonly Bus		Address =		Bus.Ref(0x2006);
			public static readonly Bus		Data =			Bus.Ref(0x2007);
			public static void ScrollTo(IOperand x, IOperand y) {
				Scroll.Set(x);
				Scroll.Set(y);
			}
			public static void ScrollTo(U8 x, U8 y) => ScrollTo((IOperand)x, y);
			public static void Reset() => A.Set(Status); //read PPU status to reset the high / low latch
			public static void SetHorizontalWrite() => Control.Set(LazyControl.Set(z => z.And(0b11111011)));
			public static void SetVerticalWrite() => Control.Set(LazyControl.Set(z => z.Or(0b100)));
			public static void SetAddress(U16 addr) {
				Reset();
				Address.Write(addr.Hi, addr.Lo);
			}
			public static void SetAddress(Var iva) {
				//Reset();
				//if (iva.Address.Length == 1)	Address.Write(iva.Address[0].Hi, iva.Address[0].Lo);
				//if (iva.Address.Length == 2)	Address.Write(iva.Address[1], iva.Address[0]);
				if (iva.Address.Length != 2) throw new ArgumentException();
				Reset();
				Address.Write(iva.Address[1], iva.Address[0]);
			}
			public static class OAM {
				public static ArrayOfStructs<SObject> Object;
				public static void Init() {
					Object = ArrayOfStructs<SObject>.New("OAMObj", 64).Dim(ShadowOAM.Ram);
				}
				public static readonly VByte Address =	VByte.Ref(0x2003,	$"{nameof(OAM)}_{nameof(Address)}");
				public static readonly VByte Data =		VByte.Ref(0x2004,	$"{nameof(OAM)}_{nameof(Data)}");   //Don't worry about this; let OAM_DMA do the work for you.
				public static readonly VByte DMA =		VByte.Ref(0x4014,	$"{nameof(OAM)}_{nameof(DMA)}");
				public static void Write(Address shadowOam) {
					Address.Set(shadowOam.Lo());	//low byte of RAM address
					DMA.Set(shadowOam.Hi());		//high byte of RAM address
				}
				public static void HideAll() {
					Loop.Repeat(X.Set(0), 256, _ => {
						Object[X].Hide();
						X.State.Unsafe(() => {
							X++; X++; X++;
						});
					});
				}
			}
			public static void ClearNametable0(U8 val) {
				SetHorizontalWrite();
				SetAddress(0x2000);
				Loop.Repeat(X.Set(0), 256, _ => Data.Write(val, val, val, val));
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
				Loop.Repeat(X.Set(0), 256, _ => Data.Write(val, val, val, val));
			}
			public static void ClearNametable3(U8 val) {
				SetHorizontalWrite();
				SetAddress(0x2C00);
				Loop.Repeat(X.Set(0), 256, _ => Data.Write(val, val, val, val));
			}
		}
		public static class APU {
			public static class Pulse1 {
				public static readonly VByte Volume =			VByte.Ref(0x4000,	$"{nameof(APU)}_{nameof(Pulse1)}_{nameof(Volume)}");
				public static readonly VByte Sweep =			VByte.Ref(0x4001,	$"{nameof(APU)}_{nameof(Pulse1)}_{nameof(Sweep)}");
				public static readonly VByte Lo =				VByte.Ref(0x4002,	$"{nameof(APU)}_{nameof(Pulse1)}_{nameof(Lo)}");
				public static readonly VByte Hi =				VByte.Ref(0x4003,	$"{nameof(APU)}_{nameof(Pulse1)}_{nameof(Hi)}");
			}
			public static class Pulse2 {
				public static readonly VByte Volume =			VByte.Ref(0x4004,	$"{nameof(APU)}_{nameof(Pulse2)}_{nameof(Volume)}");
				public static readonly VByte Sweep =			VByte.Ref(0x4005,	$"{nameof(APU)}_{nameof(Pulse2)}_{nameof(Sweep)}");
				public static readonly VByte Lo =				VByte.Ref(0x4006,	$"{nameof(APU)}_{nameof(Pulse2)}_{nameof(Lo)}");
				public static readonly VByte Hi =				VByte.Ref(0x4007,	$"{nameof(APU)}_{nameof(Pulse2)}_{nameof(Hi)}");
			}
			public static class Triangle {
				public static readonly VByte Linear =			VByte.Ref(0x4008,	$"{nameof(APU)}_{nameof(Triangle)}_{nameof(Linear)}");
				public static readonly VByte Lo =				VByte.Ref(0x400A,	$"{nameof(APU)}_{nameof(Triangle)}_{nameof(Lo)}");
				public static readonly VByte Hi =				VByte.Ref(0x400B,	$"{nameof(APU)}_{nameof(Triangle)}_{nameof(Hi)}");
			}
			public static class Noise {
				public static readonly VByte Volume =			VByte.Ref(0x400C,	$"{nameof(APU)}_{nameof(Noise)}_{nameof(Volume)}");
				public static readonly VByte Lo =				VByte.Ref(0x400E,	$"{nameof(APU)}_{nameof(Noise)}_{nameof(Lo)}");
				public static readonly VByte Hi =				VByte.Ref(0x400F,	$"{nameof(APU)}_{nameof(Noise)}_{nameof(Hi)}");
			}
			public static class DMC {
				public static readonly VByte Settings =			VByte.Ref(0x4010,	$"{nameof(APU)}_{nameof(DMC)}_{nameof(Settings)}");
				public static readonly VByte LoadCounter =		VByte.Ref(0x4011,	$"{nameof(APU)}_{nameof(DMC)}_{nameof(LoadCounter)}");
				public static readonly VByte SampleAddress =	VByte.Ref(0x4012,	$"{nameof(APU)}_{nameof(DMC)}_{nameof(SampleAddress)}");
				public static readonly VByte SampleLength =		VByte.Ref(0x4013,	$"{nameof(APU)}_{nameof(DMC)}_{nameof(SampleLength)}");

				public static void Disable() => Settings.Set(0);
			}
			public static Bus Status =						Bus.Ref(0x4015);
			public static readonly VByte FrameCounter =		VByte.Ref(0x4017,	$"{nameof(APU)}_{nameof(FrameCounter)}");
			public enum Channels : byte {
				Pulse1 =	0b00001,
				Pulse2 =	0b00010,
				Triangle =	0b00100,
				Noise =		0b01000,
				DMC =		0b10000,
			}
			public static void SetChannelsEnabled(Channels chs) => Status.Write((byte)chs);
		}
		public static class Controller {
			public static readonly VByte One =	VByte.Ref(0x4016,	$"{nameof(Controller)}_{nameof(One)}");
			public static readonly VByte Two =	VByte.Ref(0x4017,	$"{nameof(Controller)}_{nameof(Two)}");
			public static void Latch() {
				One.Set(1);
				One.Set(0);
			}
		}
		public static class Button {
			public static U8 Right =>	0b00000001;
			public static U8 Left =>	0b00000010;
			public static U8 Down =>	0b00000100;
			public static U8 Up =>		0b00001000;
			public static U8 Start =>	0b00010000;
			public static U8 Select =>	0b00100000;
			public static U8 B =>		0b01000000;
			public static U8 A =>		0b10000000;
		}
	}
}
