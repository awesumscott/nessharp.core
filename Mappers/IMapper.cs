using System;
using System.Collections.Generic;
using System.Text;

namespace NESSharp.Core.Mappers {
	public static class BankSizes {
		public static readonly int KB_4 = 4096;
		public static readonly int KB_8 = 8192;
		public static readonly int KB_16 = KB_8 * 2;
		public static readonly int KB_32 = KB_16 * 2;
	}
	public interface IMapper {
		public int Number { get; }
		public void Init(List<Bank> Prg, List<Bank> Chr, HeaderOptions headerOpts);
		public void WriteInterrupts(List<Bank> Prg, byte[] interrupts, Action<Bank, byte[]> writeFunc);
	}
}
