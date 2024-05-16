using System;

namespace NESSharp.Core;

public class SObject : Struct {
	public VByte Y { get; set; }
	public VByte Tile { get; set; }
	public VByte Attr { get; set; }
	public VByte X { get; set; }

	public void Hide() {
		Y.Set(0xFE);
	}

	public Func<Condition> IsHidden() {
		return () => Y.Equals(0xFE);
	}

	public SObject SetPosition(IOperand x, IOperand y) {
		X.Set(x);
		Y.Set(y);
		return this;
	}
	public SObject SetPosition(Func<IOperand> x, Func<IOperand> y) {
		X.Set(x());
		Y.Set(y());
		return this;
	}
}
