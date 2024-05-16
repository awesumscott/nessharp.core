using System;
using System.Collections.Generic;
using System.Linq;
using static NESSharp.Core.AL;

namespace NESSharp.Core;

public class LabelDictionary : Dictionary<string, Label> {
	private long _nextId = 0; //for naming unnamed labels
	public new Label this[string key] {
		get {
			if (!ContainsKey(key)) {
				var item = new Label();
				Add(key, item);
			}
			return base[key];
		}
	}
	public Label New() {
		var lbl = new Label();
		Add("_" + _nextId, lbl);
		_nextId++;
		return lbl;
	}
	public string NameByRef(Label lbl) => this.Where(x => x.Value == lbl).Select(x => x.Key).FirstOrDefault();
}

public interface IIndexable {
	public IndexingRegister? Index {get;set;}
}
public interface IIndexable<T> : IIndexable {}

//TODO: 
//	DONE-Rename this to Label ffs, and rename AL.Label
//	DONE-Get rid of Operation as a base class
//	DONE-Make this an IResolvable<Address>
//	DONE-Get rid of LabelAddress resolver, because this will be able to take over its duties completely
//	-Make an IIndexable interface and use it here and on Address, with an IndexingRegister property
//	-Get rid of LabelIndexed, and use IIndexable to find the right opcodes
public class Label : IResolvable<Address>, IOperation, IOperand<Label> {
	public int Length {get;set;} = 0;
	public Label Value => this;
	Address IOperand<Address>.Value => Resolve();
	public Address? Address;

	public bool IsReferenced { get; private set; }
	public void Reference() => IsReferenced = true;

	public Label Write() { Context.Write(this); return this; }

	public bool CanResolve() => Address is not null;
	public object Source => this;
	public Address Resolve() {
		if (Address == null) throw new Exception($"Address not yet resolvable for label {this}");
		return Address;
	}

	public LabelIndexed this[IndexingRegister reg] => new LabelIndexed(this, reg);
	public override string ToString() => Labels.NameByRef(this);
	public string ToAsmString(Tools.INESAsmFormatting formats) => Labels.NameByRef(this);
}

public class LabelIndexed : IOperand<LabelIndexed>, IIndexable {
	public Label Label;
	public LabelIndexed Value => this;

	IndexingRegister? IIndexable.Index { get; set; }

	public LabelIndexed(Label label, IndexingRegister reg) {
		Label = label;
		((IIndexable)this).Index = reg;
	}

	public LabelIndexed Set(LabelIndexed oli) {
		A.Set(oli);
		CPU6502.STA(this);
		return this;
	}
	public override string ToString() => $"{ Labels.NameByRef(Label) } [{ ((IIndexable)this).Index }]";
	public string ToAsmString(Tools.INESAsmFormatting formats) => Labels.NameByRef(Label);
}
