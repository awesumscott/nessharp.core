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
		public static List<Label?> Interrupts = new List<Label?>();
		public static string AsmOutput = string.Empty;

		public static void SetInfinite() {
			PrgBank.Add(new Bank(0, 0));
		}

		public static void SetMapper(IMapper mapper) {
			Header = new HeaderOptions();
			Mapper = mapper;
			Mapper.Init(PrgBank, ChrBank, Header);
		}

		public static void SetInterrupts(Label? NMI, Label? Reset, Label? IRQ) {
			Interrupts.Add(NMI);
			Interrupts.Add(Reset);
			Interrupts.Add(IRQ);
		}

		public static string LabelNameFromMethodInfo(MethodInfo methodInfo) => $"{methodInfo.DeclaringType?.Name ?? "???"}_{methodInfo.Name}";
		public static Label ToLabel(this MethodInfo methodInfo) => Labels[LabelNameFromMethodInfo(methodInfo)];

		public static void SetInterrupts(Action NMI, Action Reset, Action IRQ) {
			Interrupts.Add(NMI != null ? LabelFor(NMI) : null);
			Interrupts.Add(Reset != null ? LabelFor(Reset) : null);
			Interrupts.Add(IRQ != null ? LabelFor(IRQ) : null);
		}

		public static void WriteToFile(string fileName) {
			var header = new byte[0];
			if (Header != null) {
				var battery = Header.Battery ? 0b10 : 0;
				var mirroring = Header.Mirroring == MirroringOptions.MapperControlled
					? Header.MapperControlledMirroring
					: Header.Mirroring == MirroringOptions.Vertical
						? 0b1
						: 0b0;
				var trainer = Header.Trainer ? 0b100 : 0;
				header = new byte[] {
					(byte)'N',(byte)'E',(byte)'S',
					26,
					Header.PrgRomBanks,
					Header.ChrRomBanks, //byte 5
					(byte)(((Mapper.Number & 0x0F) << 4) + (mirroring | battery | trainer)),
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
						throw new Exception($"Incorrect type in AsmWithRefs: {type}");
				}

				//If it's a sizeless bank, remove all unused padding
				if (bank.Size == 0)
					bank.Rom = bank.Rom.Take(outputIndex).ToArray();

				bytesUsed += outputIndex;
				bytesTotal += bank.Rom.Length;
				Console.WriteLine($"Bank { bankId,-2 }:\t{ outputIndex,-5 } / { bank.Rom.Length,-5 }\t{ Math.Round((decimal)outputIndex / bank.Rom.Length * 100),-4 }%");
				bankId++;
			}
			if (bytesTotal != 0)
				Console.WriteLine($"\nTotal:\t\t{ bytesUsed,-2 } / { bytesTotal,-5 }\t{ Math.Round((decimal)bytesUsed / bytesTotal * 100),-4 }%\n");
			
			Console.WriteLine($"ZP:\t\t{ NES.zp.Used,-5 } / { NES.zp.Size,-5 }\t{ Math.Round((decimal)NES.zp.Used / NES.zp.Size * 100),-4 }%");
			Console.WriteLine($"RAM:\t\t{ NES.ram.Used,-5 } / { NES.ram.Size,-5 }\t{ Math.Round((decimal)NES.ram.Used / NES.ram.Size * 100),-4 }%");

			if (Interrupts.Any())
				WriteInterrupts();

			foreach (var lbl in Labels)
				DebugFile.WriteLabel(lbl.Value.Address, lbl.Key); //TODO: pass in bank to add the offset for mesen MLBs

			using (var f = File.Open(fileName + ".nes", FileMode.Create)) {
				f.Write(header, 0, header.Length);
				foreach (var prg in PrgBank)
					f.Write(prg.Rom, 0, prg.Rom.Length);
				if (ChrBank.Any()) {
					foreach(var chrBank in ChrBank)
						f.Write(chrBank.Rom, 0, chrBank.Rom.Length);
					//f.Write(ChrBank[0].Rom, 0, ChrBank[0].Rom.Length);
				}
			}

			if (Mapper != null) {
				WriteFile(fileName + ".mlb", DebugFile.Contents);
			}

			WriteFile(fileName + ".asm", AsmOutput);
		}

		private static void WriteFile(string filename, string contents) {
			using var f = File.Open(filename, FileMode.Create);
			var bytes = Encoding.ASCII.GetBytes(contents);
			f.Write(bytes, 0, bytes.Length);
		}

		private static void WriteInterrupts() {
			var interrupts = new byte[] {
				Interrupts[0] != null ? Interrupts[0].Address.Lo : (U8)0,
				Interrupts[0] != null ? Interrupts[0].Address.Hi : (U8)0,
				Interrupts[1] != null ? Interrupts[1].Address.Lo : (U8)0,
				Interrupts[1] != null ? Interrupts[1].Address.Hi : (U8)0,
				Interrupts[2] != null ? Interrupts[2].Address.Lo : (U8)0,
				Interrupts[2] != null ? Interrupts[2].Address.Hi : (U8)0
			};
			Mapper.WriteInterrupts(PrgBank, interrupts, WriteInterruptsInBank);
		}
		private static void WriteInterruptsInBank(Bank bank, byte[] interrupts) {
			//TODO: raise an error if these bytes are used
			var lenInterrupts = interrupts.Length;
			for (var i = 0; i < interrupts.Length; i++)
				bank.Rom[bank.Rom.Length - 1 - i] = interrupts[lenInterrupts - 1 - i];
		}

		public static void AddPrgBank(U8 id, Action<U8, Bank> bankSetup) {
			CurrentBankId = id;
			CurrentBank = PrgBank[id];
			//Use(mwa.method.ToLabel());
			bankSetup(id, CurrentBank);
			//ROMManager.FillBank();//prgBankLayoutAttr.Id);
			CurrentBank.WriteContext();
		}
		public static void AddChrBank(U8 id, Action<U8, Bank> bankSetup) {
			CurrentBankId = id;
			CurrentBank = ChrBank[id];
			//Use(mwa.method.ToLabel());
			bankSetup(id, CurrentBank);
			//ROMManager.FillBank();//prgBankLayoutAttr.Id);
			CurrentBank.WriteContext();
		}

		//TODO: this sucks and is inefficient as hell. Figure out a better API for including code at a specified offset!
		public static void Merge(U8 id, Bank newBank) {
			var mergeOrigin = newBank.Origin;
			var targetBank = PrgBank[id];
			var targetEnd = targetBank.Origin + targetBank.AsmWithRefs.Count + targetBank.AsmWithRefs.Where(x => x.GetType().GetInterfaces().Contains(typeof(IResolvable<Address>))).Count();

			if (targetEnd >= mergeOrigin)
				throw new Exception("Data already exists in merge location");

			var brkVal = Asm.OC["BRK"][Asm.Mode.Implied].Use().Value;
			for (var i = targetEnd; i < mergeOrigin; i++)
				targetBank.AsmWithRefs.Add(brkVal);
				
			for (var i = 0; i < newBank.AsmWithRefs.Count; i++)
				targetBank.AsmWithRefs.Add(newBank.AsmWithRefs[i]);
		}

		public static void CompileBin(Action PrgBankDef) {
			CurrentBank = PrgBank[0];
			PrgBankDef();
			CurrentBank.WriteContext();
		}
	}
}
