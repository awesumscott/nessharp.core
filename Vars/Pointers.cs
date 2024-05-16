using static NESSharp.Core.AL;

namespace NESSharp.Core;

public class Ptr : VWord {
	public Ptr() {}
	public Ptr(RAMRange Zp, string name) {
		//Bytes = new Address[2];//Address();
		Name = name;
		Address = Zp.Dim(2);
		//Lo = Address[0];
		//Hi = Address[1];
		DebugFileNESASM.WriteVariable(Zp, Address[0], Address[1], name);
		VarRegistry.Add(name, this);
	}

	public static new Ptr New(RAMRange Zp, string name) => new Ptr(Zp, name);
	public static Ptr Ref(VWord word) => new Ptr {
		Name = word.Name,
		Address = word.Address
	};
	public PtrY this[RegisterY offset] => new PtrY(this);
	public string ToAsmString(Tools.INESAsmFormatting formats) => Name;
}

public interface IPtrIndexed {}
public class PtrY : IPtrIndexed, IOperand<PtrY> {
	public Ptr Ptr { get; private set; }
	public PtrY Value => this;
	public PtrY(Ptr p) {
		Ptr = p;
	}
	public PtrY Set(RegisterA _) {
		CPU6502.STA(this);
		return this;
	}
	public string ToAsmString(Tools.INESAsmFormatting formats) => string.Format(formats.OperandLow, Ptr.ToAsmString(formats));
}
