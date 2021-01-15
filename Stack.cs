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
			reg.State.Push();
			Backup(reg);
			block.Invoke();
			Restore(reg);
			reg.State.Pop();
		}
		public static void Preserve(Var v, Action block) {
			Backup(v.Address);
			block.Invoke();
			Restore(v.Address);
		}

		public static void Backup(params Address[] addrs) {
			foreach (var addr in addrs)
				Push(addr);
		}
		public static void Backup(RegisterBase reg) {
			if (reg is RegisterA) {
				CPU6502.PHA();
				A.State.Push();
			} else if (reg is RegisterX) {
				CPU6502.TXA();
				//Use(Asm.TXA);
				CPU6502.PHA();
				X.State.Push();
			} else if (reg is RegisterY) {
				CPU6502.TYA();
				//Use(Asm.TYA);
				CPU6502.PHA();
				Y.State.Push();
			}
		}
		public static void Backup(Register registers = Register.All, bool statusFlags = false) {
			if (statusFlags)
				CPU6502.PHP();
			if (registers.HasFlag(Register.A)) {
				CPU6502.PHA();
				A.State.Push();
			}
			if (registers.HasFlag(Register.X)) {
				CPU6502.TXA();
				//Use(Asm.TXA);
				CPU6502.PHA();
				X.State.Push();
			}
			if (registers.HasFlag(Register.Y)) {
				CPU6502.TYA();
				//Use(Asm.TYA);
				CPU6502.PHA();
				Y.State.Push();
			}
		}
		public static void Restore(params Address[] addrs) {
			addrs.Reverse();
			foreach (var addr in addrs)
				Pop(addr);
		}
		public static void Restore(RegisterBase reg) {
			if (reg is RegisterA) {
				CPU6502.PLA();
				A.State.Pop();
			} else if (reg is RegisterX) {
				CPU6502.PLA();
				X.Set(A);
				X.State.Pop();
			} else if (reg is RegisterY) {
				CPU6502.PLA();
				Y.Set(A);
				Y.State.Pop();
			}
		}
		public static void Restore(Register registers = Register.All, bool statusFlags = false) {
			if (registers.HasFlag(Register.Y)) {
				CPU6502.PLA();
				Y.Set(A);
				Y.State.Pop();
			}
			if (registers.HasFlag(Register.X)) {
				CPU6502.PLA();
				X.Set(A);
				X.State.Pop();
			}
			if (registers.HasFlag(Register.A)) {
				CPU6502.PLA();
				A.State.Pop();
			}
			if (statusFlags)
				CPU6502.PLP();
		}
	}
}
