using static NESSharp.Core.CPU6502;

namespace NESSharp.Core;

public interface IResolvable {
	bool CanResolve();
	object Source { get; }
}
public interface IResolvable<T> : IOperand<T>, IResolvable {
	T Resolve();
}
public static class ResolverExtensions {
	public static High			Hi(this IResolvable<Address> addr) => new(addr);
	public static Low			Lo(this IResolvable<Address> addr) => new(addr);
	public static Offset8		Offset(this IResolvable<U8> b, int offset) => new(b, offset);
	public static Offset16		Offset(this IResolvable<Address> addr, int offset) => new(addr, offset);
	public static ShiftLeft		ShiftLeft(this IResolvable<Address> addr, U8 bits) => new(addr, bits);
	public static ShiftRight	ShiftRight(this IResolvable<Address> addr, U8 bits) => new(addr, bits);
}
public static class OperandExtensions {
	public static High_Operand	Hi(this IOperand<Address> addr) => new(addr);
	public static Low_Operand	Lo(this IOperand<Address> addr) => new(addr);
}


public class High_Operand : IOperand<U8> {
	public U8 Value => _addr.Value.Hi;
	private IOperand<Address> _addr;
	public High_Operand(IOperand<Address> addr) {
		_addr = addr;
	}
	public override string? ToString() => $">{ _addr }";
	public string ToAsmString(Tools.INESAsmFormatting formats) => string.Format(formats.OperandHigh, _addr.ToAsmString(formats));
}
public class Low_Operand : IOperand<U8> {
	public U8 Value => _addr.Value.Lo;
	private IOperand<Address> _addr;
	public Low_Operand(IOperand<Address> addr) {
		_addr = addr;
	}
	public override string? ToString() => $"<{ _addr }";
	public string ToAsmString(Tools.INESAsmFormatting formats) => string.Format(formats.OperandLow, _addr.ToAsmString(formats));
}














public class ShiftLeft : IResolvable<Address>, IOperand<Address> {
	private IResolvable<Address> _addr;
	private U8 _shiftAmt;
	public ShiftLeft(IResolvable<Address> addr, U8 shiftAmt) {
		_addr = addr;
		_shiftAmt = shiftAmt;
	}
	public Address Value => Resolve();
	public bool CanResolve() => _addr.CanResolve();
	public object Source => _addr.Source;
	public Address Resolve() => AL.Addr((U16)((U16)_addr.Resolve() << _shiftAmt));
	public override string? ToString() => $"{ _addr }<<{_shiftAmt}";
	public string ToAsmString(Tools.INESAsmFormatting formats) => string.Format(formats.ExpressionLShift, _addr.ToAsmString(formats), _shiftAmt);
}
public class ShiftRight : IResolvable<Address>, IOperand<Address> {
	private IResolvable<Address> _addr;
	private U8 _shiftAmt;
	public ShiftRight(IResolvable<Address> addr, U8 shiftAmt) {
		_addr = addr;
		_shiftAmt = shiftAmt;
	}
	public Address Value => Resolve();
	public bool CanResolve() => _addr.CanResolve();
	public object Source => _addr.Source;
	public Address Resolve() => AL.Addr((U16)((U16)_addr.Resolve() >> _shiftAmt));
	public override string? ToString() => $"{ _addr }>>{_shiftAmt}";
	public string ToAsmString(Tools.INESAsmFormatting formats) => string.Format(formats.ExpressionRShift, _addr.ToAsmString(formats), _shiftAmt);
}
public class High : IResolvable<U8>, IOperand<U8> {
	private IResolvable<Address> _addr;
	public High(IResolvable<Address> addr) => _addr = addr;
	public U8 Value => Resolve();
	public bool CanResolve() => _addr.CanResolve();
	public object Source => _addr.Source;
	public U8 Resolve() => _addr.Resolve().Hi;
	public override string? ToString() => $"HIGH({ _addr })";
	public string ToAsmString(Tools.INESAsmFormatting formats) => string.Format(formats.ResolveHigh, _addr.ToAsmString(formats));
}
public class Low : IResolvable<U8>, IOperand<U8> {
	private IResolvable<Address> _addr;
	public Low(IResolvable<Address> addr) => _addr = addr;
	public U8 Value => Resolve();
	public bool CanResolve() => _addr.CanResolve();
	public object Source => _addr.Source;
	public U8 Resolve() => _addr.Resolve().Lo;
	public override string? ToString() => $"LOW({ _addr })";
	public string ToAsmString(Tools.INESAsmFormatting formats) => string.Format(formats.ResolveLow, _addr.ToAsmString(formats));
}
public class Offset8 : IResolvable<U8>, IOperand<U8> {
	private IResolvable<U8> _byte;
	private int _offset;
	public Offset8(IResolvable<U8> b, int offset) {
		_byte = b;
		_offset = offset;
	}
	public U8 Value => Resolve();
	public bool CanResolve() => _byte.CanResolve();
	public object Source => _byte.Source;
	public U8 Resolve() => _byte.Resolve() + _offset;
	public override string? ToString() => $"{ _byte }{ (_offset > 0 ? "+" : "") }{_offset }";
	public string ToAsmString(Tools.INESAsmFormatting formats) => _offset > 0
		? string.Format(formats.ExpressionAdd, _byte.ToAsmString(formats), _offset)
		: string.Format(formats.ExpressionSubtract, _byte.ToAsmString(formats), System.Math.Abs(_offset));
}
public class Offset16 : IResolvable<Address>, IOperand<Address> {
	private IResolvable<Address> _addr;
	private int _offset;
	public Offset16(IResolvable<Address> addr, int offset) {
		_addr = addr;
		_offset = offset;
	}
	public Address Value => Resolve();
	public bool CanResolve() => _addr.CanResolve();
	public object Source => _addr.Source;
	public Address Resolve() => AL.Addr((U16)(_addr.Resolve() + _offset));
	public override string? ToString() => $"{ _addr }{ (_offset > 0 ? "+" : "") }{_offset }";
	public string ToAsmString(Tools.INESAsmFormatting formats) => _offset > 0
		? string.Format(formats.ExpressionAdd, _addr.ToAsmString(formats), _offset)
		: string.Format(formats.ExpressionSubtract, _addr.ToAsmString(formats), System.Math.Abs(_offset));
}
