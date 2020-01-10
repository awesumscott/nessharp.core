using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static NESSharp.Core.AL;

namespace NESSharp.Core {
	public static class Stack {
		public static void SetPointer(U8 v) {
			//TODO: if x is preserved, throw an exception
			X.Set(v);
			Use(Asm.TXS);
		}
		public static void Reset() {
			SetPointer(0xFF);
		}
		public static void Push(Address addr) {
			A.Set(addr);
			Use(Asm.PHA);
		}
		public static void Pop(Address addr) {
			Use(Asm.PLA);
			addr.Set(A);
		}

		public static void Preserve(RegisterBase reg, Action block) {
			Backup(reg);
			block.Invoke();
			Restore(reg);
		}
		public static void Preserve(IVarAddressArray v, Action block) {
			Backup(v.Address);
			block.Invoke();
			Restore(v.Address);
		}

		public static void Backup(params Address[] addrs) {
			foreach (var addr in addrs) {
				Push(addr);
			}
		}
		public static void Backup(RegisterBase reg) {
			if (reg is RegisterA)
				Use(Asm.PHA);
			else if (reg is RegisterX) {
				Use(Asm.TXA);
				Use(Asm.PHA);
			} else if (reg is RegisterY) {
				Use(Asm.TYA);
				Use(Asm.PHA);
			}
		}
		public static void Backup(Register registers = Register.All, bool statusFlags = false) {
			if (statusFlags)
				Use(Asm.PHP);
			if (registers.HasFlag(Register.A))
				Use(Asm.PHA);
			if (registers.HasFlag(Register.X)) {
				Use(Asm.TXA);
				Use(Asm.PHA);
			}
			if (registers.HasFlag(Register.Y)) {
				Use(Asm.TYA);
				Use(Asm.PHA);
			}
		}
		public static void Restore(params Address[] addrs) {
			addrs.Reverse();
			foreach (var addr in addrs) {
				Pop(addr);
			}
		}
		public static void Restore(RegisterBase reg) {
			if (reg is RegisterA)
				Use(Asm.PLA);
			else if (reg is RegisterX) {
				Use(Asm.PLA);
				X.Set(A);
			} else if (reg is RegisterY) {
				Use(Asm.PLA);
				Y.Set(A);
			}
		}
		public static void Restore(Register registers = Register.All, bool statusFlags = false) {
			if (registers.HasFlag(Register.Y)) {
				Use(Asm.PLA);
				Y.Set(A);
			}
			if (registers.HasFlag(Register.X)) {
				Use(Asm.PLA);
				X.Set(A);
			}
			if (registers.HasFlag(Register.A))
				Use(Asm.PLA);
			if (statusFlags)
				Use(Asm.PLP);
		}
	}
}
