using System.Collections.Generic;
using static NESSharp.Core.AL;

namespace NESSharp.Core {
	public class VInteger : VarN {
		public static VInteger New(RAM ram, ushort intLen, string name) => (VInteger)new VInteger() { Size = intLen }.Dim(ram, name);

		//TODO: verify this works
		/// <summary>Addition of the integer portions of two vars</summary>
		/// <param name="vi"></param>
		/// <returns></returns>
		/// <remarks>Fractional value is not returned</remarks>
		public IEnumerable<RegisterA> Add(VDecimal vd) => Add(vd.Integer);
	}
}
