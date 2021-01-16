using static NESSharp.Core.AL;

namespace NESSharp.Core {
	public class VFlags : VByte {
		public static new VFlags New(RAMRange ram, string name) => (VFlags)new VFlags().Dim(ram, name);
		public void SetBit(int position, bool value) {
			if (value)
				Set(Or(0b1 << position));
			else
				Set(And(0b11111110 << position));
		}
		public void ToggleBit(int position) => Set(Xor(0b1 << position));
		public Condition TestBit(int position, bool value) {
			And(0b1 << position);
			if (value)
				return A.NotEquals(0);
			return A.Equals(0);
		}
	}
}
