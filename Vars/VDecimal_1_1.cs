namespace NESSharp.Core;

[VarSize(2)]
public class VDecimal_1_1 : VarN {
	public VByte Integer => VByte.Ref(Address[1], Index, $"{Name}_Int");
	public VByte Fractional => VByte.Ref(Address[0], Index, $"{Name}_Frac");
	public override VDecimal_1_1 Dim(RAMRange ram, string name) {
		Size = 2;
		base.Dim(ram, name);
		return this;
	}
	public static VDecimal_1_1 New(RAMRange ram, string name) => new VDecimal_1_1().Dim(ram, name);
}
