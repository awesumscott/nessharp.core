using System;
using System.Linq;
using static NESSharp.Core.AL;

namespace NESSharp.Core {
	public class Ptr : IVarAddressArray {
		public virtual Address Lo { get; private set; }
		public virtual Address Hi { get; private set; }
		//public Address[] Address { get; set; }

		//public Address[] Address;
		//public U8 Index = null;
		public string Name;
		public Ptr(string name) {
			//Bytes = new Address[2];//Address();
			Name = name;
			Address = zp.Dim(2);
			Lo = Address[0];
			Hi = Address[1];
			VarRegistry.Add(name, this);
		}
		public Ptr(Address pointToAddr = null, U8 index = null, string name = "?") {
			Address = zp.Dim(2);
			Lo = Address[0];
			Hi = Address[1];
			if (pointToAddr != null)
				PointTo(pointToAddr);
			//Index = index ?? 0;
			DebugFile.WriteVariable(Address[0], Address[1], name);
			VarRegistry.Add(name, this);
		}
		public static Ptr New(string name) => new Ptr(name);
		public void PointTo(Address addr) {
			Address[0].Set(addr.Lo);
			Address[1].Set(addr.Hi);
			A.Reset();
		}
		public void PointTo(VarN vn) {
			if (vn.Size != 2) throw new Exception("Value must have a size of 2 bytes");
			Address[0].Set(vn.Address[0]);
			Address[1].Set(vn.Address[1]);
			A.Reset();
		}
		public void PointTo(OpLabel lbl) {
			A.Set(lbl.Lo());
			Address[0].Set(A);
			A.Set(lbl.Hi());
			Address[1].Set(A);
			A.Reset();
		}
		public void PointTo(Action a) {
			PointTo(LabelFor(a)); //TODO: check attribute with an IsSubroutine func
		}
		public IPtrIndexed this[RegisterBase offset] {
			get {
				if (!(offset is RegisterY)) throw new NotImplementedException();
				return new PtrY(this);
			}
		}
		public override string ToString() {
			var loMatch = VarRegistry.Where(x => x.Value.Address.Any(x => x.Hi == Lo.Hi && x.Lo == Lo.Lo)).FirstOrDefault().Key;
			var hiMatch = VarRegistry.Where(x => x.Value.Address.Any(x => x.Hi == Hi.Hi && x.Lo == Hi.Lo)).FirstOrDefault().Key;
			if (string.IsNullOrEmpty(loMatch) && string.IsNullOrEmpty(hiMatch))
				return "$" + Hi.ToString().Substring(1) + Lo.ToString().Substring(1);
			//if (VarRegistry[match].Address.ToList().IndexOf())
			if (string.IsNullOrEmpty(loMatch))
				return hiMatch + "[1]";
			return loMatch + "[0]";
		}
	}



	
	public interface IPtrIndexed {}
	public class PtrY : IPtrIndexed {
		public Ptr Ptr { get; private set; }
		public PtrY(Ptr p) {
			Ptr = p;
		}
		public override string ToString() {
			var loMatch = VarRegistry.Where(x => x.Value.Address.Any(x => x.Hi == Ptr.Lo.Hi && x.Lo == Ptr.Lo.Lo)).FirstOrDefault().Key;
			var hiMatch = VarRegistry.Where(x => x.Value.Address.Any(x => x.Hi == Ptr.Hi.Hi && x.Lo == Ptr.Hi.Lo)).FirstOrDefault().Key;
			if (string.IsNullOrEmpty(loMatch) && string.IsNullOrEmpty(hiMatch))
				return "$" + Ptr.Hi.ToString().Substring(1) + Ptr.Lo.ToString().Substring(1);
			//if (VarRegistry[match].Address.ToList().IndexOf())
			if (string.IsNullOrEmpty(loMatch))
				return hiMatch + "[1]";
			return loMatch + "[0]";
		}
	}
}
