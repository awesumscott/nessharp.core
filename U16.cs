using System;
using System.Collections.Generic;
using System.Text;
using static NESSharp.Core.AL;

namespace NESSharp.Core {
	public interface IU16 {
		public IU8 Lo { get; }
		public IU8 Hi { get; }
	}
	public class U16 : IU16 {
		public virtual U8 Lo { get; private set; }
		public virtual U8 Hi { get; private set; }
		IU8 IU16.Lo => Lo;
		IU8 IU16.Hi => Hi;

		public U16(ushort value) {
			Hi = (byte)(value >> 8);
			Lo = (byte)value;
		}
		public U16(Address a) {
			Hi = a.Hi;
			Lo = a.Lo;
		}

		public U16 IncrementedValue() {
			return new U16((ushort)(this + 1));
		}

		public static implicit operator U16(ushort s) => new U16(s);
		public static implicit operator ushort(U16 p) => (ushort)((p.Hi << 8) + p.Lo);
		public static U16 operator ++(U16 addr) {
			if (addr.Hi == 0)
				Use(Asm.INC.ZeroPage, addr.Lo);
			else
				Use(Asm.INC.Absolute, addr);
			return addr;
		}
		public override string ToString() {
			return "$" + Hi.ToString().Substring(1) + Lo.ToString().Substring(1);
		}
	}
}
