using System;

namespace NESSharp.Core;

interface IBankCallTable {
	public void Add(object key, U8 bank, Action method);
	public void Write();
	public U8 IndexOf(object name);
	public void Call(Label lbl);
	public void Call(LabelIndexed oli);
	public void Call(object name);
	public void BSCAR(object name);
}
interface IBankSwitcher {
	public void SwitchTo(object o);
}
public abstract class BankSwitcher {
	public static VByte Bank;

	public BankSwitcher() {
		Bank = VByte.New(NES.zp, "bank_current");
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
