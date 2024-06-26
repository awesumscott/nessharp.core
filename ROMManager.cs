﻿using NESSharp.Core.Mappers;
using NESSharp.Core.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using static NESSharp.Core.CPU6502;

namespace NESSharp.Core;

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
		public static _ConsoleLogger ConsoleLoggers = new(_Tools.Where(x => x is IConsoleLogTool).Cast<IConsoleLogTool>().ToList());

		public class _AssemblerOutput : IAssemblerOutput {
			private IEnumerable<IAssemblerOutput> _instances;
			public _AssemblerOutput(IEnumerable<IAssemblerOutput> instances) => _instances = instances;
			public void AppendBytes(IEnumerable<string> bytes) => _instances.ForEach(x => x.AppendBytes(bytes));
			public void AppendComment(string comment) => _instances.ForEach(x => x.AppendComment(comment));
			public void AppendLabel(string name) => _instances.ForEach(x => x.AppendLabel(name));
			public void AppendOp(OpCode opCode) => _instances.ForEach(x => x.AppendOp(opCode));
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
		public class _ConsoleLogger : IConsoleLogTool {
			private IEnumerable<IConsoleLogTool> _instances;
			public _ConsoleLogger(IEnumerable<IConsoleLogTool> instances) => _instances = instances;
			public void WriteToConsole() => _instances.ForEach(x => x.WriteToConsole());
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
		PrgBank.Add(new Bank(0, 0, 0));
	}

	public static void SetMapper(IMapper mapper) {
		Header = new HeaderOptions();
		Mapper = mapper;
		Mapper.Init(PrgBank, ChrBank, Header);
	}

	public static string LabelNameFromMethodInfo(MethodInfo methodInfo) => $"{methodInfo.DeclaringType?.Name ?? "???"}_{methodInfo.Name}";
	public static Label ToLabel(this MethodInfo methodInfo) => AL.Labels[LabelNameFromMethodInfo(methodInfo)];

	public static void SetInterrupts(Action NMI, Action Reset, Action IRQ) {
		Interrupts.Add(NMI != null ? AL.LabelFor(NMI) : null);
		Interrupts.Add(Reset != null ? AL.LabelFor(Reset) : null);
		Interrupts.Add(IRQ != null ? AL.LabelFor(IRQ) : null);
	}

	public static void WriteToFile(string fileName) {
		Console.WriteLine("BANKS-----------------");
		var bankId = 0;
		var bytesUsed = 0;
		var bytesTotal = 0;
		var allBanks = PrgBank.Concat(ChrBank).ToList();
		foreach (var bank in allBanks) {
			CurrentBank = bank;

			var outputIndex = 0;
			void addObjectToAssembly(object o) {
				if (o is byte b) {
					bank.Rom[outputIndex++] = b;
				} else if (o is IResolvable<U8> iru8) {
					bank.Rom[outputIndex++] = iru8.Resolve().Value;
				} else if (o is IResolvable<Address> ira) {
					var addr = ira.Resolve();
					bank.Rom[outputIndex++] = addr.Lo.Value;
					bank.Rom[outputIndex++] = addr.Hi.Value;
				} else if (o is IOperand<U8> iopu8) {
					bank.Rom[outputIndex++] = iopu8.Value;
				} else if (o is IOperand<Address> iopaddr) {
					bank.Rom[outputIndex++] = iopaddr.Lo().Value;
					bank.Rom[outputIndex++] = iopaddr.Hi().Value;
				}
			}
			foreach (var item in bank.AsmWithRefs) {
				var op = item;
				if (op is OpRaw raw) {
					raw.Value.Cast<object>().ForEach(addObjectToAssembly);
				} else if (op is OpCode opCode) {
					bank.Rom[outputIndex++] = opCode.Value;

					if (opCode.Length > 1) {
						addObjectToAssembly(opCode.Param);

						//Label tally to determine if labels are actually used
						Label? lbl = null;
						if (opCode.Param is Label l)
							lbl = l;
						else if (opCode.Param is IResolvable r && r.Source is Label resolveSource)
							lbl = resolveSource;
						lbl?.Reference();
					}
				}
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

		
		foreach (var bank in allBanks) {
			foreach (var op in bank.AsmWithRefs) {
				if (op is Label label) {
					var name = AL.Labels.NameByRef(label);
					if (string.IsNullOrEmpty(name)) continue;
					if (!label.IsReferenced && name.StartsWith("_") && int.TryParse(name.Substring(1), out _)) continue;
					Tools.AssemblerOutput.AppendLabel(name);
				} else if (op is OpRaw raw)			Tools.AssemblerOutput.AppendBytes(raw.Value.Cast<object>().Select(x => x.ToString() ?? string.Empty).ToList());
				else if (op is OpComment comment)	Tools.AssemblerOutput.AppendComment(comment.Text);
				else if (op is OpCode opCode)		Tools.AssemblerOutput.AppendOp(opCode);
			}
		}

		if (Interrupts.Any())
			WriteInterrupts();

		foreach (var lbl in AL.Labels)
			DebugFileNESASM.WriteLabel(lbl.Value.Address, lbl.Key); //TODO: pass in bank to add the offset for mesen MLBs

		using (var f = File.Open(fileName + ".nes", FileMode.Create)) {
			var header = _setupHeader();
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
		Tools.ConsoleLoggers.WriteToConsole();
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
	//public static void Merge(U8 id, Bank newBank) {
	//	var mergeOrigin = newBank.Origin;
	//	var targetBank = PrgBank[id];
	//	var targetEnd = targetBank.Origin + targetBank.AsmWithRefs.Count + targetBank.AsmWithRefs.Where(x => x.GetType().GetInterfaces().Contains(typeof(IResolvable<Address>))).Count();

	//	if (targetEnd >= mergeOrigin)
	//		throw new Exception("Data already exists in merge location");

	//	var brkVal = Asm.OC["BRK"][Asm.Mode.Implied].Use().Value;
	//	for (var i = targetEnd; i < mergeOrigin; i++)
	//		targetBank.AsmWithRefs.Add(brkVal);
			
	//	for (var i = 0; i < newBank.AsmWithRefs.Count; i++)
	//		targetBank.AsmWithRefs.Add(newBank.AsmWithRefs[i]);
	//}

	public static void CompileBin(Action PrgBankDef) {
		CurrentBank = PrgBank[0];
		PrgBankDef();
		CurrentBank.WriteContext();
	}

	private static byte[] _setupHeader() {
		if (Header == null) return new byte[0];

		var battery = Header.Battery ? 0b10 : 0;
		var mirroring = Header.Mirroring == MirroringOptions.MapperControlled
			? Header.MapperControlledMirroring
			: Header.Mirroring == MirroringOptions.Vertical
				? 0b1
				: 0b0;
		var trainer = Header.Trainer ? 0b100 : 0;
		return new byte[] {
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
}
