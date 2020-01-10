using System;
using System.Collections.Generic;
using System.Text;
using static NESSharp.Core.AL;

namespace NESSharp.Core.Resolvers {
	public class LabelAddress : IResolvable<Address> {
		private OpLabel _lbl;
		private U8 _shiftAmt;
		public LabelAddress(OpLabel lbl) {
			_lbl = lbl;
		}
		public Address Resolve() {
			return _lbl.Address;
		}
	}
	public class ShiftLeft : IResolvable<Address> {
		private IResolvable<Address> _addr;
		private U8 _shiftAmt;
		public ShiftLeft(IResolvable<Address> addr, U8 shiftAmt) {
			_addr = addr;
			_shiftAmt = shiftAmt;
		}
		public Address Resolve() {
			return Addr((U16)((U16)_addr.Resolve() >> 6));
		}
	}
	public class ShiftRight : IResolvable<Address> {
		private IResolvable<Address> _addr;
		private U8 _shiftAmt;
		public ShiftRight(IResolvable<Address> addr, U8 shiftAmt) {
			_addr = addr;
			_shiftAmt = shiftAmt;
		}
		public Address Resolve() {
			return Addr((U16)((U16)_addr.Resolve() >> 6));
		}
	}
	public class High : IResolvable<U8> {
		private IResolvable<Address> _addr;
		public High(IResolvable<Address> addr) {
			_addr = addr;
		}
		public U8 Resolve() {
			return _addr.Resolve().Hi;
		}
	}
	public class Low : IResolvable<U8> {
		private IResolvable<Address> _addr;
		public Low(IResolvable<Address> addr) {
			_addr = addr;
		}
		public U8 Resolve() {
			return _addr.Resolve().Lo;
		}
	}
}
