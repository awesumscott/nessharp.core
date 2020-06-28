using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static NESSharp.Core.AL;

namespace NESSharp.Core {

	public class SObject : Struct {
		public VByte Y { get; set; }
		public VByte Tile { get; set; }
		public VByte Attr { get; set; }
		public VByte X { get; set; }

		public void Hide() {
			Y.Set(0xFE);
		}

		public Func<Condition> IsHidden() {
			return () => Y.Equals(0xFE);
		}
	}

	public class OAMDictionary {
		public ArrayOfStructs<SObject> Object;
		public OAMDictionary(RAM ram) {
			Object = ArrayOfStructs<SObject>.New("OAMObj", 64).Dim(ram);
		}
		public void HideAll() {
			//Refs.ForEach(() => {
			//	Refs.
			//});
			Loop.RepeatX(0, 256, () => {
				Object[0].Y[X].Set(0xFE);
			});
		}
	}
}
