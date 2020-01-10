using System;
using System.Collections.Generic;
using System.Text;
using NESSharp.Core;
using static NESSharp.Core.AL;

namespace NESSharp.Core {
	interface IBankCallTable {
		public void Add(object key, U8 bank, Action method);
		public void Write();
		public U8 IndexOf(object name);
		public void Call(OpLabel lbl);
		public void Call(OpLabelIndexed oli);
		public void Call(object name);
		public void BSCAR(object name);
	}
	interface IBankSwitcher {
		public void SwitchTo(object o);
	}
	public abstract class BankSwitcher {
		public static Var8 Bank;

		public BankSwitcher() {
			Bank = Var8.New(zp, "bank_current");
		}
		protected abstract void Load();
		public abstract void Step();
	}
	//public class BankedSubroutine {
	//	public object Key;
	//	public U8 Bank;
	//	public Action Method;
	//	public BankedSubroutine(object key, U8 bank, Action method) {
	//		Key = key;
	//		Bank = bank;
	//		Method = method;
	//	}
	//}
	//public abstract class BankSwitchTable {
	//	public abstract void Add(BankedSubroutine bs);
	
	//}
}
