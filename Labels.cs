using System;
using System.Collections.Generic;
using System.Linq;
using static NESSharp.Core.AL;

namespace NESSharp.Core {
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
		public string NameByRef(Label lbl) {
			return this.Where(x => x.Value == lbl).Select(x => x.Key).FirstOrDefault();
		}
	}

	public interface IIndexable<T> {
		public IndexingRegister? Index {get;set;}
	}

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
		public Label() {
			Length = 0;
		}
		public Address Resolve() {
			if (Address == null) throw new Exception($"Address not yet resolvable for label {this}");
			return Address;
		}

		public LabelIndexed this[IndexingRegister reg] => new LabelIndexed(this, reg);
		public override string ToString() => Labels.NameByRef(this);
	}

	//TODO: get rid of this and replace it with an implementation using Resolvers--maybe not. Index probably isn't needed by resolving phase--it's built into the opcode
	public class LabelIndexed : IOperand<LabelIndexed> {
		public Label Label;
		public IndexingRegister? Index = null;

		public LabelIndexed Value => this;

		public LabelIndexed(Label label, IndexingRegister reg) {
			Label = label;
			Index = reg;
		}

		//public LabelIndexed Set(RegisterA a) {
		//	CPU6502.STA(this);
		//	return this;
		//}
		public LabelIndexed Set(LabelIndexed oli) {
			A.Set(oli);
			CPU6502.STA(this);
			return this;
		}
		public override string ToString() => $"{ Labels.NameByRef(Label) } [{ Index }]";
	}
}
