using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static NESSharp.Core.AL;

namespace NESSharp.Core {

	public interface IWritable {
		void Write();
	}
	public class AnimationModel : IWritable {
		public U8 Offset { get; set; }
		public List<FrameModel> Frames { get; set; }
		public void Write() {}
	}
	public class FrameModel : IWritable {
		public string Name { get; set; }
		public List<SObjectModel> Tiles { get; set; }
		public void Write() {}
	}
	public class SObjectModel : IWritable {
		public U8 Y { get; set; }
		public U8 Tile { get; set; }
		public U8 Attr { get; set; }
		public U8 X { get; set; }
		
		public SObjectModel() {}
		public SObjectModel(U8 y, U8 tile, U8 attr, U8 x) {
			X = x; Y = y; Tile = tile; Attr = attr;
		}

		public void Write() => Raw(Y, Tile, Attr, X);
	}

	public class SObject : Struct {
		public VByte Y { get; set; }
		public VByte Tile { get; set; }
		public VByte Attr { get; set; }
		public VByte X { get; set; }
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
			Loop.RepeatX(0, 255, () => {
				Object[0].Y[X].Set(0xFE);
			});
		}
	}
}
