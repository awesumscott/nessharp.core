using System;
using System.Collections.Generic;
using System.Text;
using static NESSharp.Core.AL;

namespace NESSharp.Core {
	public class Bus : Address {
		
		public Bus(ushort value) : base(value) {}

		public static Bus Ref(ushort addr) {
			return new Bus(addr);
		}

		public void Send(Action dataSection, int len) {
			if (len > 256) throw new Exception("Len > 256! Split up data until longer sends are supported.");
			X.Set(0);
			Loop.Do(_ => {
				Set(LabelFor(dataSection)[X]);
				X++;
			}).While(() => X.NotEquals((U8)(len == 256 ? 0 : len)));
		}
		public void Write(Action dataSection) {
			Send(dataSection, Length(dataSection));
		}
		public void Write(params U8[] vals) {
			foreach (var val in vals)
				Set(val);
		}
		public void Write(params IU8[] vals) {
			foreach (var val in vals)
				Set(val);
		}
		public void Write(RegisterA a) {
			Set(a);
		}
	}
}
