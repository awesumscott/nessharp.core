using System;
using System.Collections.Generic;
using System.Linq;
using static NESSharp.Core.AL;

namespace NESSharp.Core {
	public class LabelDictionary : Dictionary<string, Label> {
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
			Add("_" + lbl.ID.ToString(), lbl);
			return lbl;
		}
		public Label ById(long id) {
			return Values.Where(x => x.ID == id).FirstOrDefault();
		}
		public string NameByRef(Label lbl) {
			return this.Where(x => x.Value == lbl).Select(x => x.Key).FirstOrDefault();
		}
	}

	//TODO: 
	//	DONE-Rename this to Label ffs, and rename AL.Label
	//	DONE-Get rid of Operation as a base class
	//	DONE-Make this an IResolvable<Address>
	//	DONE-Get rid of LabelAddress resolver, because this will be able to take over it's duties completely
	//	-Make an IIndexable interface and use it here and on Address, with an IndexingRegister property
	//	-Get rid of OpLabelIndexed, and use IIndexable to find the right opcodes
	public class Label : IResolvable<Address>, IOperation {
		public int Length {get;set;} = 0;
		public long ID;
		public Address? Address;
		private static long _nextId = 0;
		public Label() {
			ID = _nextId++;
			Length = 0;
		}
		public Address Resolve() {
			if (Address == null) throw new Exception($"Address not yet resolvable for label {this}");
			return Address;
		}

		public OpLabelIndexed this[IndexingRegister reg] => new OpLabelIndexed(this, reg);
		public override string ToString() => Labels.NameByRef(this);
	}

	//TODO: get rid of this and replace it with an implementation using Resolvers--maybe not. Index probably isn't needed by resolving phase--it's built into the opcode
	public class OpLabelIndexed {
		public Label Label;
		public IndexingRegister Index = null;

		public OpLabelIndexed(Label label, IndexingRegister reg) {
			Label = label;
			Index = reg;
		}
		public OpLabelIndexed Set(RegisterA a) {
			CPU6502.STA(this);
			return this;
		}
		public OpLabelIndexed Set(OpLabelIndexed oli) {
			A.Set(oli);
			CPU6502.STA(this);
			return this;
		}
		public override string ToString() => $"{ AL.Labels.NameByRef(Label) } [{ (Index is RegisterX ? "X" : Index is RegisterY ? "Y" : "?") }]";
	}
}
