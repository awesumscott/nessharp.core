using System;
using System.Collections.Generic;
using System.Text;
using static NESSharp.Core.AL;

namespace NESSharp.Core {
	public class VDecimal : VarN {
		//private VarN _int, _frac;
		public ushort IntLen, FracLen;
		public VarN Integer {get; protected set;}// => _int;
		public VarN Fractional {get; protected set;}// => _frac;
		public override VDecimal Dim(RAMRange ram, string name) {
			base.Dim(ram, name);
			Integer = VarN.Ref(Address[FracLen], IntLen);
			Fractional = VarN.Ref(Address[0], FracLen);
			return this;
		}
		public static VDecimal New(RAMRange ram, ushort intLen, ushort fracLen, string name) => new VDecimal() {
			Size = intLen + fracLen,
			IntLen = intLen,
			FracLen = fracLen
		}.Dim(ram, name);

		//TODO: verify this works
		/// <summary>Addition of the integer portions of two vars</summary>
		/// <param name="vi"></param>
		/// <returns></returns>
		/// <remarks>Fractional value is not returned</remarks>
		public IEnumerable<RegisterA> Add(VInteger vi) => Integer.Add(vi);
		//public IEnumerable<RegisterA> Add(VInteger vi) {
		//	var srcLen = vi.Address.Length;
		//	if (srcLen > Size) throw new Exception("Source var length is greater than destination var length");
		//	yield return A.Set(Address[0]).Add(vi.Address[0]);
		//	for (var i = 1; i < Size; i++) {
		//		if (i >= vi.Size)
		//			yield return Address[i].ToA().ADC(0);
		//		yield return Address[i].ToA().ADC(vi.Address[i]);
		//	}
		//	yield break;
		//}
	}
}
