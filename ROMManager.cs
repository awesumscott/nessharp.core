using NESSharp.Core.Mappers;
using NESSharp.Core.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using static NESSharp.Core.AL;

namespace NESSharp.Core {
	public static class ROMManager {
		public static IMapper		Mapper;
		public static HeaderOptions	Header;
		public static List<Bank>	PrgBank = new List<Bank>();
		public static List<Bank>	ChrBank = new List<Bank>();
		public static List<Label?>	Interrupts = new List<Label?>();
		public static Bank			CurrentBank;
		public static U8			CurrentBankId;

		internal static class Tools {
			public static _AssemblerOutput AssemblerOutput = new(_Tools.Where(x => x is IAssemblerOutput).Cast<IAssemblerOutput>().ToList());

			//Lists for ConsoleLoggers and FileLoggers separate from the instances of the other interfaces.
			//This allows two tools of the same type to have different output styles. For example, one ROM analyzer
			//that logs a percentage used, and another that outputs a rendered image.
			public static _DebugFile DebugFiles = new(_Tools.Where(x => x is IDebugFile).Cast<IDebugFile>().ToList());
			public static _FileLogger FileLoggers = new(_Tools.Where(x => x is IFileLogTool).Cast<IFileLogTool>().ToList());

			public class _AssemblerOutput : IAssemblerOutput {
				private IEnumerable<IAssemblerOutput> _instances;
				public _AssemblerOutput(IEnumerable<IAssemblerOutput> instances) => _instances = instances;
				public void AppendBytes(IEnumerable<string> bytes) => _instances.ForEach(x => x.AppendBytes(bytes));
				public void AppendComment(string comment) => _instances.ForEach(x => x.AppendComment(comment));
				public void AppendLabel(string name) => _instances.ForEach(x => x.AppendLabel(name));
				public void AppendOp(Asm.OpRef opref, OpCode opcode) => _instances.ForEach(x => x.AppendOp(opref, opcode));
			}
			public class _DebugFile : IDebugFile {
				private IEnumerable<IDebugFile> _instances;
				public _DebugFile(IEnumerable<IDebugFile> instances) => _instances = instances;
			}
			public class _FileLogger : IFileLogTool {
				private IEnumerable<IFileLogTool> _instances;
				public _FileLogger(IEnumerable<IFileLogTool> instances) => _instances = instances;
				public void WriteFile(Action<string, string> fileWriteMethod) => _instances.ForEach(x => x.WriteFile(fileWriteMethod));
			}
		}

		private static readonly List<ITool>	_Tools			= new();
		public static T Tool<T>() where T : ITool {
			var instance = (T?)_Tools.Where(x => x.GetType() == typeof(T)).FirstOrDefault();
			if (instance == null)
				_Tools.Add(instance = Activator.CreateInstance<T>());
			return instance;
		}



		public static void SetInfinite() {
			PrgBank.Add(new Bank(0, 0));
		}

		public static void SetMapper(IMapper mapper) {
			Header = new HeaderOptions();
			Mapper = mapper;
			Mapper.Init(PrgBank, ChrBank, Header);
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
				foreach (var item in bank.AsmWithRefs) {
					if (item is byte b) {
						bank.Rom[outputIndex++] = b;
					} else if (item is IResolvable<U8> iru8) {
						bank.Rom[outputIndex++] = iru8.Resolve();
					} else if (item is IResolvable<Address> ira) {
						var addr = ira.Resolve();
						bank.Rom[outputIndex++] = addr.Lo;
						bank.Rom[outputIndex++] = addr.Hi;
					} else if (item is IOperand<U8> iopu8) {
						bank.Rom[outputIndex++] = iopu8.Value;
					} else if (item is IOperand<Address> iopaddr) {
						bank.Rom[outputIndex++] = iopaddr.Lo().Value;
						bank.Rom[outputIndex++] = iopaddr.Hi().Value;
					} else
						throw new Exception($"Incorrect type in AsmWithRefs: {item.GetType()}");
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
			
			Console.WriteLine($"ZP:\t\t{ NES.zp.Used,-5		} / { NES.zp.Size,-5	}\t{ Math.Round((decimal)NES.zp.Used / NES.zp.Size * 100),-4	}%");
			Console.WriteLine($"RAM:\t\t{ NES.ram.Used,-5	} / { NES.ram.Size,-5	}\t{ Math.Round((decimal)NES.ram.Used / NES.ram.Size * 100),-4	}%");

			if (Interrupts.Any())
				WriteInterrupts();

			foreach (var lbl in Labels)
				DebugFileNESASM.WriteLabel(lbl.Value.Address, lbl.Key); //TODO: pass in bank to add the offset for mesen MLBs

			using (var f = File.Open(fileName + ".nes", FileMode.Create)) {
				f.Write(header, 0, header.Length);
				foreach (var prg in PrgBank)
					f.Write(prg.Rom, 0, prg.Rom.Length);
				foreach(var chrBank in ChrBank)
					f.Write(chrBank.Rom, 0, chrBank.Rom.Length);
			}

			if (Mapper != null) {
				WriteFile(fileName + ".mlb", DebugFileNESASM.Contents);
			}

			//TODO: ConsoleLoggers
			Tools.FileLoggers.WriteFile(WriteFile);
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
			//TODO: throw exception if these bytes are used
			var lenInterrupts = interrupts.Length;
			for (var i = 0; i < interrupts.Length; i++)
				bank.Rom[bank.Rom.Length - 1 - i] = interrupts[lenInterrupts - 1 - i];
		}

		public static void AddPrgBank(U8 id, Action<U8, Bank> bankSetup) {
			CurrentBankId = id;
			CurrentBank = PrgBank[id];
			bankSetup(id, CurrentBank);
			CurrentBank.WriteContext();
		}
		public static void AddChrBank(U8 id, Action<U8, Bank> bankSetup) {
			CurrentBankId = id;
			CurrentBank = ChrBank[id];
			bankSetup(id, CurrentBank);
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
