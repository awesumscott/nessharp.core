using System;
using System.Linq;
using static NESSharp.Core.AL;

namespace NESSharp.Core {
	public class Ptr : VWord {
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
		public PtrY this[RegisterY offset] => new PtrY(this);
		public override string ToString() {
			//var loMatch = VarRegistry.Where(x => x.Value.Address.Any(x => x.Hi == Lo.Hi && x.Lo == Lo.Lo)).FirstOrDefault().Key;
			//var hiMatch = VarRegistry.Where(x => x.Value.Address.Any(x => x.Hi == Hi.Hi && x.Lo == Hi.Lo)).FirstOrDefault().Key;
			//if (string.IsNullOrEmpty(loMatch) && string.IsNullOrEmpty(hiMatch))
			//	return "$" + Hi.ToString().Substring(1) + Lo.ToString().Substring(1);
			////if (VarRegistry[match].Address.ToList().IndexOf())
			//if (string.IsNullOrEmpty(loMatch))
			//	return hiMatch + "[1]";
			//return loMatch + "[0]";
			return "pointer";
		}
	}

	public interface IPtrIndexed {}
	public class PtrY : IPtrIndexed, IOperand<PtrY> {
		public Ptr Ptr { get; private set; }
		public PtrY Value => this;
		public PtrY(Ptr p) {
			Ptr = p;
		}
		public PtrY Set(object o) {
			if (o is RegisterA)
				CPU6502.STA(this);
			else
				throw new NotImplementedException();
			return this;
		}
		public override string ToString() {
			//var loMatch = VarRegistry.Where(x => x.Value.Address.Any(x => x.Hi == Ptr.Lo.Hi && x.Lo == Ptr.Lo.Lo)).FirstOrDefault().Key;
			//var hiMatch = VarRegistry.Where(x => x.Value.Address.Any(x => x.Hi == Ptr.Hi.Hi && x.Lo == Ptr.Hi.Lo)).FirstOrDefault().Key;
			//if (string.IsNullOrEmpty(loMatch) && string.IsNullOrEmpty(hiMatch))
			//	return "$" + Ptr.Hi.ToString().Substring(1) + Ptr.Lo.ToString().Substring(1);
			////if (VarRegistry[match].Address.ToList().IndexOf())
			//if (string.IsNullOrEmpty(loMatch))
			//	return hiMatch + "[1]";
			//return loMatch + "[0]";
			return "pointer"; //TODO
		}
	}
}
