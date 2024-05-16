using NESSharp.Core.Tools;
using static NESSharp.Core.AL;

namespace NESSharp.Core;

public class Address : U16, IOperand<Address> {//, IOperable<Address> {
	public Address(ushort value) : base(value) {}
	public Address Value => this;

	public bool IsZP() => Hi.Value == 0;
	public Address Set(IOperand operand) {
		//These must be in here for things like generic IndexingRegister refs, which wouldn't get picked up by Set(RegisterX/Y)
		if (operand is RegisterA)		A.STA(this);
		else if (operand is RegisterX)	CPU6502.STX(this);
		else if (operand is RegisterY)	CPU6502.STY(this);
		else							A.Set(operand).STA(this);
		return this;
	}
	public virtual Address Set(U8 v) { A.Set(v).STA(this); return this; }

	//public override string ToString() {
		//var matchName = VarRegistry.Where(x => x.Value.Address.Any(x => x.Hi == Hi && x.Lo == Lo)).FirstOrDefault().Key;

		//if (string.IsNullOrEmpty(matchName))
		//	return base.ToString();

		//var matchVar = VarRegistry[matchName];
		//var matchByteInstance = matchVar.Address.Where(x => x.Hi == Hi && x.Lo == Lo).FirstOrDefault(); //necessary instead of "this" because instance refs may be different
		//int? index = (matchVar.Address.Length > 1  && matchByteInstance != null) ? matchVar.Address.ToList().IndexOf(matchByteInstance) : (int?)null;
		//return matchName + (index!=null ? $"[{index}]" : "");
	//}

	public string ToAsmString(INESAsmFormatting formats) => string.Format(formats.AddressFormat, this);

	public Address IncrementedValue => new Address((ushort)((U16)this + 1));

	public static implicit operator Address(ushort s) => new Address(s);
	public static implicit operator ushort(Address p) => (ushort)((p.Hi << 8) + p.Lo);

	public AddressIndexed this[IndexingRegister r] => new AddressIndexed(this, r);
}
public class AddressIndexed : Address, IIndexable {
	//public IndexingRegister? Index = null;
	public AddressIndexed(ushort value, IndexingRegister reg) : base(value) => Index = reg;

	public IndexingRegister? Index { get; set; }

	public static implicit operator ushort(AddressIndexed p) => (ushort)((p.Hi << 8) + p.Lo);
}
