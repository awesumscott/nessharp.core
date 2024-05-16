using System;

namespace NESSharp.Core;

#pragma warning disable CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
#pragma warning disable CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
public class U8 : IOperand<U8> {
	public U8(byte value) {
		Value = value;
	}
	public U8(int value) {
		var b = (byte)value;
		if (value > 255 || value < -255) throw new ArgumentOutOfRangeException();
		Value = b;
	}

	public byte Value { get; }
	U8 IOperand<U8>.Value => this;

	public static implicit operator U8(byte s) => new U8(s);
	public static implicit operator byte(U8 p) => p.Value;
	public static implicit operator U8(int i) => new U8(i);
	public static implicit operator int(U8 p) => p.Value;

	public static bool operator ==(U8? a, U8? b) => a?.Value == b?.Value;
	public static bool operator !=(U8? a, U8? b) => a?.Value != b?.Value;
	public override string ToString() => $"${Value:X2}";
	public string ToAsmString(Tools.INESAsmFormatting formats) => $"${Value:X2}";
}
