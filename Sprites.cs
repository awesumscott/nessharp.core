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
			((Var8)_spriteStruct["vert"]).Set(0xFF);
			return this;
		}
		public Var8 X {
			get => (Var8)_spriteStruct["horiz"];
		}
		public Var8 Y {
			get => (Var8)_spriteStruct["vert"];
		}
		public Var8 Tile {
			get => (Var8)_spriteStruct["tile"];
		}
		public Var8 Attr {
			get => (Var8)_spriteStruct["attr"];
		}
	}

	public class SpriteDictionary : Dictionary<U8, Sprite> {
		//public static RAM ShadowOAM = OAMRam.Allocate(Addr(0x200), Addr(0x2FF));
		public static ArrayOfStructs Refs = ArrayOfStructs.New(
				"sprite",
				64,
				//Struct.Field(typeof(Array<Var8>), "VarArray"),
				Struct.Field(typeof(Var8), "vert"),
				Struct.Field(typeof(Var8), "tile"),
				Struct.Field(typeof(Var8), "attr"),
				Struct.Field(typeof(Var8), "horiz")
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
