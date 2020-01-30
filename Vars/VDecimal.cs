using System;
using System.Collections.Generic;
using System.Text;
using static NESSharp.Core.AL;

namespace NESSharp.Core {
	public class VDecimal : VarN {
		private VarN _int, _frac;
		private ushort _intLen, _fracLen;
		public VarN Integer => _int;
		public VarN Fractional => _frac;
		public override Var Dim(RAM ram, string name) {
			base.Dim(ram, name);
			_int = VarN.Ref(Address[_fracLen], _intLen);
			_frac = VarN.Ref(Address[0], _fracLen);
			return this;
		}
		public static VDecimal New(RAM ram, ushort intLen, ushort fracLen, string name) {
			return (VDecimal)new VDecimal(){Size = intLen + fracLen, _intLen = intLen, _fracLen = fracLen}.Dim(ram, name);
		}

		//TODO: verify this works
		/// <summary>Addition of the integer portions of two vars</summary>
		/// <param name="vi"></param>
		/// <returns></returns>
		/// <remarks>Fractional value is not returned</remarks>
		public IEnumerable<RegisterA> Add(VInteger vi) {
			return Integer.Add(vi);
		}
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
