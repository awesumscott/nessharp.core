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
			CPU6502.TXS();
		}
		public static void Reset() {
			SetPointer(0xFF);
		}
		public static void Push(Address addr) {
			A.Set(addr);
			CPU6502.PHA();
		}
		public static void Pop(Address addr) {
			CPU6502.PLA();
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
			foreach (var addr in addrs)
				Push(addr);
		}
		public static void Backup(RegisterBase reg) {
			if (reg is RegisterA)
				CPU6502.PHA();
			else if (reg is RegisterX) {
				CPU6502.TXA();
				//Use(Asm.TXA);
				CPU6502.PHA();
			} else if (reg is RegisterY) {
				CPU6502.TYA();
				//Use(Asm.TYA);
				CPU6502.PHA();
			}
		}
		public static void Backup(Register registers = Register.All, bool statusFlags = false) {
			if (statusFlags)
				CPU6502.PHP();
			if (registers.HasFlag(Register.A))
				CPU6502.PHA();
			if (registers.HasFlag(Register.X)) {
				CPU6502.TXA();
				//Use(Asm.TXA);
				CPU6502.PHA();
			}
			if (registers.HasFlag(Register.Y)) {
				CPU6502.TYA();
				//Use(Asm.TYA);
				CPU6502.PHA();
			}
		}
		public static void Restore(params Address[] addrs) {
			addrs.Reverse();
			foreach (var addr in addrs)
				Pop(addr);
		}
		public static void Restore(RegisterBase reg) {
			if (reg is RegisterA)
				CPU6502.PLA();
			else if (reg is RegisterX) {
				CPU6502.PLA();
				X.Set(A);
			} else if (reg is RegisterY) {
				CPU6502.PLA();
				Y.Set(A);
			}
		}
		public static void Restore(Register registers = Register.All, bool statusFlags = false) {
			if (registers.HasFlag(Register.Y)) {
				CPU6502.PLA();
				Y.Set(A);
			}
			if (registers.HasFlag(Register.X)) {
				CPU6502.PLA();
				X.Set(A);
			}
			if (registers.HasFlag(Register.A))
				CPU6502.PLA();
			if (statusFlags)
				CPU6502.PLP();
		}
	}
}
