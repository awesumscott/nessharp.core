using System;
using System.Collections.Generic;
using System.Text;

namespace NESSharp.Core {
	public interface IU8 {}
	public class U8 : IU8 {
		public U8(byte value) {
			Value = value;
		}

		public byte Value { get; }

		public static implicit operator U8(byte s) => new U8(s);
		public static implicit operator byte(U8 p) => p.Value;

		public static bool operator ==(U8 a, U8 b) => a.Value == b.Value;
		public static bool operator !=(U8 a, U8 b) => a.Value != b.Value;
		public override string ToString() {
			return "$" + Value.ToString("X2");
		}
	}
}
