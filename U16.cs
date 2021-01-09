namespace NESSharp.Core {
	public class U16 {
		public virtual U8 Lo { get; private set; }
		public virtual U8 Hi { get; private set; }

		public U16(ushort value) {
			Hi = (byte)(value >> 8);
			Lo = (byte)value;
		}
		public U16(Address a) {
			Hi = a.Hi;
			Lo = a.Lo;
		}

		public U16 IncrementedValue() => new U16((ushort)(this + 1));

		public static implicit operator U16(ushort s) => new U16(s);
		public static implicit operator ushort(U16 p) => (ushort)((p.Hi << 8) + p.Lo);
		public override string ToString() => "$" + Hi.ToString().Substring(1) + Lo.ToString().Substring(1);
	}
}
