using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static NESSharp.Core.AL;

namespace NESSharp.Core {
	public class LabelDictionary : Dictionary<string, OpLabel> {
		public new OpLabel this[string key] {
			get {
				if (!ContainsKey(key)) {
					var item = new OpLabel();
					Add(key, item);
				}
				return base[key];
			}
		}
		public OpLabel New() {
			var lbl = new OpLabel();
			Add("_" + lbl.ID.ToString(), lbl);
			return lbl;
		}
		public OpLabel ById(long id) {
			return Values.Where(x => x.ID == id).FirstOrDefault();
		}
		public string NameByRef(OpLabel lbl) {
			return this.Where(x => x.Value == lbl).Select(x => x.Key).FirstOrDefault();
		}
	}

	public class OpLabel : Operation {
		//public string Name;
		public long ID;
		public Address Address;
		private static long _nextId = 0;
		public OpLabel() {
			ID = _nextId++;
			Length = 0;
		}
		public OpLabel(OpLabel other) {
			ID = other.ID;
			Length = other.Length;
		}
		//public OpLabel(string name) {
		//	OpLabel()
		//	//ID = DateTime.Now.Ticks;
		//	Name = name;
		//}
		public LabelRef Reference(int offset = 0) => new LabelRef(this, offset);
		public LabelLo Lo(int offset = 0) => new LabelLo(Reference(offset));
		public LabelHi Hi(int offset = 0) => new LabelHi(Reference(offset));
		public OpLabelIndexed this[IndexingRegister reg] => new OpLabelIndexed(this, reg);
		public override string ToString() => Label.NameByRef(this);
	}

	//TODO: delete all this, and have labels act as addresses, handling Hi, Lo, and offsets through labelrefs to be replaced later //this idea may be obsolete
	public class OpLabelIndexed {
		public OpLabel Label;
		public IndexingRegister Index = null;

		public OpLabelIndexed(OpLabel label, IndexingRegister reg) {
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
		public override string ToString() => $"{ AL.Label.NameByRef(Label) } [{ (Index is RegisterX ? "X" : Index is RegisterY ? "Y" : "?") }]";
	}

	public interface IResolvable<T> {
		T Resolve();
	}
	//TODO:	figure out if this idea is worthwhile: instead of ID, can't this just be a pointer to the label instance?
	//		is the label instance always definitely going to exist at labelref creation time?
	//TODO: prolly make Label an IRes<Addr> and get rid of this
	public class LabelRef : IResolvable<Address> {
		public long ID;
		public int Offset;
		public OpLabel Lbl;
		public LabelRef(OpLabel lbl, int offset = 0) {
			Lbl = lbl; //added for parser to use Hi/Lo
			ID = lbl.ID;
			Offset = offset;
		}

		//public Address Resolve() => Addr((U16)(Label.ById(ID).Address + Offset));
		public Address Resolve() {
			if (Label.ById(ID).Address == null) {
				throw new Exception($"Label { Label.NameByRef(Label.ById(ID)) } is referenced but not used");
				Console.WriteLine($"Label { Label.NameByRef(Label.ById(ID)) } is referenced but not used");
				Environment.Exit(0);
			}
			return Addr((U16)(Label.ById(ID).Address + Offset));
		}

		public override string? ToString() => Label.ById(ID).ToString() + (Offset != 0 ? Offset > 0 ? $"+{Offset}" : $"{Offset}" : string.Empty);
	}
	public class LabelLo : IResolvable<U8> {
		public LabelRef LblRef;
		public LabelLo(LabelRef labelRef) => LblRef = labelRef;
		public U8 Resolve() => LblRef.Resolve().Lo;
		public override string? ToString() => $"LOW({ LblRef.ToString() })";
	}
	public class LabelHi : IResolvable<U8> {
		public LabelRef LblRef;
		public LabelHi(LabelRef labelRef) => LblRef = labelRef;
		public U8 Resolve() => LblRef.Resolve().Hi;
		public override string? ToString() => $"HIGH({ LblRef.ToString() })";
	}
}
