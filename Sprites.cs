using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static NESSharp.Core.AL;

namespace NESSharp.Core {
	public class Sprite {
		//private U8 _index;
		private Dictionary<string, Var> _spriteStruct;
		public Sprite(Dictionary<string, Var> spriteStruct) {
			//_index = index;
			//_offset = Addr((U16)(index * 4 + 0x200));
			_spriteStruct = spriteStruct;
		}
		public Sprite Hide() { //vertical position ($EF-$FF = hidden)
			((VByte)_spriteStruct["vert"]).Set(0xFF);
			return this;
		}
		public VByte X {
			get => (VByte)_spriteStruct["horiz"];
		}
		public VByte Y {
			get => (VByte)_spriteStruct["vert"];
		}
		public VByte Tile {
			get => (VByte)_spriteStruct["tile"];
		}
		public VByte Attr {
			get => (VByte)_spriteStruct["attr"];
		}
	}

	public class SpriteDictionary : Dictionary<U8, Sprite> {
		//public static RAM ShadowOAM = OAMRam.Allocate(Addr(0x200), Addr(0x2FF));
		public static ArrayOfStructs Refs = ArrayOfStructs.New(
				"sprite",
				64,
				//Struct.Field(typeof(Array<Var8>), "VarArray"),
				Struct.Field(typeof(VByte), "vert"),
				Struct.Field(typeof(VByte), "tile"),
				Struct.Field(typeof(VByte), "attr"),
				Struct.Field(typeof(VByte), "horiz")
			).Dim(OAMRam);
	
		public new Sprite this[U8 key] {
			get {
				if (!ContainsKey(key)) {
					var item = new Sprite(Refs.FieldsArray[key]);
					Add(key, item);
				}
				return base[key];
			}
		}
		public void HideAll() {
			//Refs.ForEach(() => {
			//	Refs.
			//});
			Loop.RepeatX(0, 255, () => {
				Addr(0x0200)[X].Set(0xFE);
			});
		}
	}
}
