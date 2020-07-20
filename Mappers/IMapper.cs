using System;
using System.Collections.Generic;
using System.Text;

namespace NESSharp.Core.Mappers {
	public static class MemorySizes {
		public static readonly int KB_1 = 1024;
		public static readonly int KB_2 = KB_1 * 2;
		public static readonly int KB_4 = KB_2 * 2;
		public static readonly int KB_8 = KB_4 * 2;
		public static readonly int KB_16 = KB_8 * 2;
		public static readonly int KB_32 = KB_16 * 2;
		public static readonly int KB_64 = KB_32 * 2;
		public static readonly int KB_128 = KB_64 * 2;
		public static readonly int KB_256 = KB_128 * 2;
		public static readonly int KB_512 = KB_256 * 2;
	}
	public interface IMapper {
		public int Number { get; }
		public void Init(List<Bank> Prg, List<Bank> Chr, HeaderOptions headerOpts);
		public void WriteInterrupts(List<Bank> Prg, byte[] interrupts, Action<Bank, byte[]> writeFunc);
	}
}
