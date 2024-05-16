using System;
using static NESSharp.Core.AL;

namespace NESSharp.Core;

public class Bus : Address {
	public Bus(ushort value) : base(value) {}

	public static Bus Ref(ushort addr) => new Bus(addr);

	public void Send(Action dataSection, int len) {
		if (len > 256) throw new Exception("Len > 256! Split up data until longer sends are supported.");
		X.Set(0);
		Loop.Do_old(_ => {
			Set(LabelFor(dataSection)[X]);
			X.Inc();
		}).While(() => X.NotEquals(len == 256 ? 0 : len));

		//TODO: use ptr to send 256 at a time, rebaselining ptr after each pass
	}
	public void Write(Action dataSection) {
		Send(dataSection, Length(dataSection));
	}
	public void Write(params U8[] vals) {
		//TODO: clean this up when LDA duplication is handled in CPU6502
		var prevVal = vals[0] + 1;
		foreach (var val in vals) {
			if (val != prevVal)
				A.Set(val);
			prevVal = val;
			A.STA(this);
		}
	}
	public void Write(params IOperand[] vals) {
		foreach (var val in vals)
			A.Set(val).STA(this);
	}
	public void Write(RegisterA a) => Set(a);
}
