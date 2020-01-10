using NESSharp.Core.Mappers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using static NESSharp.Core.AL;

namespace NESSharp.Core {
	public static class ROMManager {
		public static IMapper Mapper;
		public static HeaderOptions Header;
		public static List<Bank> PrgBank = new List<Bank>();
		public static List<Bank> ChrBank = new List<Bank>();
		public static List<LabelRef> Interrupts = new List<LabelRef>();
		public static string AsmOutput = string.Empty;

		public static void SetInfinite() {
			PrgBank.Add(new Bank(0, 0));
		}

		public static void SetMapper(IMapper mapper) {
			Header = new HeaderOptions();
			Mapper = mapper;
			Mapper.Init(PrgBank, ChrBank, Header);
		}

		public static void SetInterrupts(OpLabel NMI, OpLabel Reset, OpLabel IRQ) {
			Interrupts.Add(NMI != null ? NMI.Reference() : null);
			Interrupts.Add(Reset != null ? Reset.Reference() : null);
			Interrupts.Add(IRQ != null ? IRQ.Reference() : null);
		}

		public static string LabelNameFromMethodInfo(MethodInfo methodInfo) => $"{methodInfo.DeclaringType.Name}_{methodInfo.Name}";
		public static OpLabel ToLabel(this MethodInfo methodInfo) => Label[LabelNameFromMethodInfo(methodInfo)];

		public static void SetInterrupts(Action NMI, Action Reset, Action IRQ) {
			Interrupts.Add(NMI != null ? LabelFor(NMI).Reference() : null);
			Interrupts.Add(Reset != null ? LabelFor(Reset).Reference() : null);
			Interrupts.Add(IRQ != null ? LabelFor(IRQ).Reference() : null);
		}

		public static void WriteToFile(string fileName) {
			var header = new byte[0];
			if (Header != null) {
				header = new byte[] {
					(byte)'N',(byte)'E',(byte)'S',
					26,
					Header.PrgRomBanks,
					Header.ChrRomBanks, //byte 5
					//(byte)(((Mapper & 0x0F) << 4) + 0b1011), //xxxx1xx1 = 4 screen, xxxxxx1x = "battery", hardcoded until I finish implementing settings
					(byte)(((Mapper.Number & 0x0F) << 4) + 0b0000), //xxxx1xx1 = 4 screen, xxxxxx1x = "battery", hardcoded until I finish implementing settings
					//(byte)((Mapper & 0xF0) + 0b1000), //xxxx10xx = iNES 2.0
					(byte)((Mapper.Number & 0xF0) + 0b0000), //xxxx10xx = iNES 2.0
					0, 0, 0,
					//0b1001, //byte 11, set to 9 for (64 << 9 = 32768 KB CHR-RAM) https://wiki.nesdev.com/w/index.php/NES_2.0
					0b0000, //byte 11, set to 9 for (64 << 9 = 32768 KB CHR-RAM) https://wiki.nesdev.com/w/index.php/NES_2.0
					0, 0, 0, 0
				};
			}

			Console.WriteLine("BANKS-----------------");
			var bankId = 0;
			var bytesUsed = 0;
			var bytesTotal = 0;
			foreach (var bank in PrgBank.Concat(ChrBank)) {
				CurrentBank = bank;

				var outputIndex = 0;
				for (var i = 0; i < bank.AsmWithRefs.Count; i++) {
					var item = bank.AsmWithRefs[i];
					var type = item.GetType();
					if (type == typeof(byte))
						bank.Rom[outputIndex++] = (byte)item;
					//TODO: add condition for U8s
					else if (type.GetInterfaces().Contains(typeof(IResolvable<Address>))) {
						var addr = ((IResolvable<Address>)item).Resolve();
						bank.Rom[outputIndex++] = addr.Lo;
						bank.Rom[outputIndex++] = addr.Hi;
					} else if (type.GetInterfaces().Contains(typeof(IResolvable<U8>))) {
						bank.Rom[outputIndex++] = ((IResolvable<U8>)item).Resolve();
					} else
						throw new Exception("Incorrect type in AsmWithRefs!");
				}

				//If it's a sizeless bank, remove all unused padding
				if (bank.Size == 0)
					bank.Rom = bank.Rom.Take(outputIndex).ToArray();

				bytesUsed += outputIndex;
				bytesTotal += bank.Rom.Length;
				Console.WriteLine(
					string.Format(
						"Bank {0}:\t{1} / {2}\t{3}%",
						bankId.ToString().PadLeft(2),
						outputIndex.ToString().PadLeft(5),
						bank.Rom.Length.ToString().PadLeft(5),
						Math.Round((decimal)outputIndex / bank.Rom.Length * 100).ToString().PadLeft(4)
					)
				);
				bankId++;
			}
			if (bytesTotal != 0)
				Console.WriteLine(
					string.Format(
						"\nTotal:\t\t{0} / {1}\t{2}%\n",
						bytesUsed.ToString().PadLeft(2),
						bytesTotal.ToString().PadLeft(5),
						Math.Round((decimal)bytesUsed / bytesTotal * 100).ToString().PadLeft(4)
					)
				);
			
			Console.WriteLine(
				string.Format(
					"ZP:\t\t{0} / {1}\t{2}%",
					zp.Used.ToString().PadLeft(5),
					zp.Size.ToString().PadLeft(5),
					Math.Round((decimal)zp.Used / zp.Size * 100).ToString().PadLeft(4)
				)
			);
			Console.WriteLine(
				string.Format(
					"RAM:\t\t{0} / {1}\t{2}%",
					ram.Used.ToString().PadLeft(5),
					ram.Size.ToString().PadLeft(5),
					Math.Round((decimal)ram.Used / ram.Size * 100).ToString().PadLeft(4)
				)
			);

			if (Interrupts.Any())
				WriteInterrupts();

			foreach (var lbl in Label) {
				DebugFile.WriteLabel(lbl.Value.Address, lbl.Key); //TODO: pass in bank to add the offset for mesen MLBs
			}

			using (var f = File.Open(fileName + ".nes", FileMode.Create)) {
				f.Write(header, 0, header.Length);
				foreach (var prg in PrgBank)
					f.Write(prg.Rom, 0, prg.Rom.Length);
				if (ChrBank.Any())
					f.Write(ChrBank[0].Rom, 0, ChrBank[0].Rom.Length);
			}

			if (Mapper != null) {
				using (var f = File.Open(fileName + ".mlb", FileMode.Create)) {
					var debugFileBytes = Encoding.ASCII.GetBytes(DebugFile.Contents);
					f.Write(debugFileBytes, 0, debugFileBytes.Length);
				}
			}

			using (var f = File.Open(fileName + ".asm", FileMode.Create)) {
				var asmFileBytes = Encoding.ASCII.GetBytes(AsmOutput);
				f.Write(asmFileBytes, 0, asmFileBytes.Length);
			}
		}

		private static void WriteInterrupts() {
			var interrupts = new byte[] {
				Interrupts[0] != null ? Label.ById(Interrupts[0].ID).Address.Lo : (U8)0,
				Interrupts[0] != null ? Label.ById(Interrupts[0].ID).Address.Hi : (U8)0,
				Interrupts[1] != null ? Label.ById(Interrupts[1].ID).Address.Lo : (U8)0,
				Interrupts[1] != null ? Label.ById(Interrupts[1].ID).Address.Hi : (U8)0,
				Interrupts[2] != null ? Label.ById(Interrupts[2].ID).Address.Lo : (U8)0,
				Interrupts[2] != null ? Label.ById(Interrupts[2].ID).Address.Hi : (U8)0
			};
			//if (MapperNum == 0) {
			//	WriteInterruptsInBank(PrgBank[0], interrupts);
			//} else if (MapperNum == 30) {
			//	WriteInterruptsInBank(PrgBank[31], interrupts);
			//}
			Mapper.WriteInterrupts(PrgBank, interrupts, WriteInterruptsInBank);
		}
		private static void WriteInterruptsInBank(Bank bank, byte[] interrupts) {
			//TODO: raise an error if these bytes are used
			var lenInterrupts = interrupts.Length;
			for (var i = 0; i < interrupts.Length; i++)
				bank.Rom[bank.Rom.Length - 1 - i] = interrupts[lenInterrupts - 1 - i];
		}

		public static void CompileRom(Type romType) {
			var methods = romType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			foreach (var mwa in methods.WithAttribute<PrgBankDef>()) {
				CurrentBank = PrgBank[mwa.attribute.Id];
				Use(mwa.method.ToLabel());
				mwa.method.Invoke(null, null);
				//ROMManager.FillBank();//prgBankLayoutAttr.Id);
				CurrentBank.WriteContext();
			}
			foreach (var mwa in methods.WithAttribute<ChrBankDef>()) {
				CurrentBank = ChrBank[mwa.attribute.Id];
				Use(mwa.method.ToLabel());
				mwa.method.Invoke(null, null);
				CurrentBank.WriteContext();
			}
		}

		public static void CompileBin(Action PrgBankDef) {
			CurrentBank = PrgBank[0];
			PrgBankDef();
			CurrentBank.WriteContext();
		}
	}
}
