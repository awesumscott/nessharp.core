﻿using static NESSharp.Core.AL;

namespace NESSharp.Core {
	public interface IResolvable {}
	public interface IResolvable<T> : IOperand<T>, IResolvable {
		T Resolve();
	}
	public static class ResolverExtensions {
		public static High Hi(this IResolvable<Address> addr) => new High(addr);
		public static Low Lo(this IResolvable<Address> addr) => new Low(addr);
		public static Offset8 Offset(this IResolvable<U8> b, int offset) => new Offset8(b, offset);
		public static Offset16 Offset(this IResolvable<Address> addr, int offset) => new Offset16(addr, offset);
		public static ShiftLeft ShiftLeft(this IResolvable<Address> addr, U8 bits) => new ShiftLeft(addr, bits);
		public static ShiftRight ShiftRight(this IResolvable<Address> addr, U8 bits) => new ShiftRight(addr, bits);
	}
	public class ShiftLeft : IResolvable<Address>, IOperand<Address> {
		private IResolvable<Address> _addr;
		private U8 _shiftAmt;
		public ShiftLeft(IResolvable<Address> addr, U8 shiftAmt) {
			_addr = addr;
			_shiftAmt = shiftAmt;
		}
		public Address Value => Resolve();
		public Address Resolve() => Addr((U16)((U16)_addr.Resolve() << _shiftAmt));
		public override string? ToString() => $"{ _addr }<<{_shiftAmt}";
	}
	public class ShiftRight : IResolvable<Address>, IOperand<Address> {
		private IResolvable<Address> _addr;
		private U8 _shiftAmt;
		public ShiftRight(IResolvable<Address> addr, U8 shiftAmt) {
			_addr = addr;
			_shiftAmt = shiftAmt;
		}
		public Address Value => Resolve();
		public Address Resolve() => Addr((U16)((U16)_addr.Resolve() >> _shiftAmt));
		public override string? ToString() => $"{ _addr }>>{_shiftAmt}";
	}
	public class High : IResolvable<U8>, IOperand<U8> {
		private IResolvable<Address> _addr;
		public High(IResolvable<Address> addr) => _addr = addr;
		public U8 Value => Resolve();
		public U8 Resolve() => _addr.Resolve().Hi;
		public override string? ToString() => $"HIGH({ _addr })";
	}
	public class Low : IResolvable<U8>, IOperand<U8> {
		private IResolvable<Address> _addr;
		public Low(IResolvable<Address> addr) => _addr = addr;
		public U8 Value => Resolve();
		public U8 Resolve() => _addr.Resolve().Lo;
		public override string? ToString() => $"LOW({ _addr })";
	}
	public class Offset8 : IResolvable<U8>, IOperand<U8> {
		private IResolvable<U8> _byte;
		private int _offset;
		public Offset8(IResolvable<U8> b, int offset) {
			_byte = b;
			_offset = offset;
		}
		public U8 Value => Resolve();
		public U8 Resolve() => (U8)(_byte.Resolve() + _offset);
		public override string? ToString() => $"{ _byte }{ (_offset > 0 ? "+" : "") }{_offset }";
	}
	public class Offset16 : IResolvable<Address>, IOperand<Address> {
		private IResolvable<Address> _addr;
		private int _offset;
		public Offset16(IResolvable<Address> addr, int offset) {
			_addr = addr;
			_offset = offset;
		}
		public Address Value => Resolve();
		public Address Resolve() => Addr((U16)(_addr.Resolve() + _offset));
		public override string? ToString() => $"{ _addr }{ (_offset > 0 ? "+" : "") }{_offset }";
	}
}
