using System;
using System.Collections.Generic;
using System.Linq;

namespace NESSharp.Core {
	public class ArrayOfStructs<StructType> : Var where StructType : Struct, new() {
		public int			Length	{ get; set; }
		private Struct _baseInstance;
		private StructType[] _structs;
		public static ArrayOfStructs<StructType> New(string name, int arrayLength) {
			var aos = new ArrayOfStructs<StructType>();
			aos.Name = name;
			aos.Length = arrayLength;
			aos._baseInstance = new StructType();
			return aos;
		}
		//TODO: see if the Dim(ram,name) override could just set or use existing name, and call Dim(ram) (or preferably just consolidate them somehow!)
		public ArrayOfStructs<StructType> Dim(RAM ram) {
			var structs = new List<StructType>();
			for (var i = 0; i < Length; i++) {
				structs.Add(Struct.New<StructType>(ram, $"{ typeof(StructType).Name}_{i}" ));
			}
			_structs = structs.ToArray();
			return this;
		}

		//TODO: maybe only allow VByte indexing if struct.length is a power of two? or just do the damn multiply
		/*
			TODO:

			Indexing won't be useful with AOS. Instead, maybe there should be some iteration helpers:

			//Iterate(numTimes, bodyFunc)
			aosInstance.Iterate(5, sObj => {
				sObj.X.Set(5);
			});
			//Iterate(numTimes, initFunc, incrementFunc, bodyFunc)
			aosInstance.Iterate(5, Y.Set(5), () => Y.Increment(), sObj => {
				sObj.X.Set(5);
			});

			Standalone Iterator class?

		*/

		//struct gets copied and not referenced, so Indexes don't hang around
		public StructType this[U8 offset] => _makeCopy(_structs[offset], null);
		public StructType this[IndexingRegister reg] => _makeCopy(_structs[0], reg);

		private StructType _makeCopy(StructType original, IndexingRegister index) {
			var newInstance = ((StructType?)Activator.CreateInstance(_baseInstance.GetType()))?.Copy(original);
			foreach (var p in newInstance.GetType().GetProperties().Where(x => typeof(Var).IsAssignableFrom(x.PropertyType)).ToList()) //user-defined Var properties
				((Var)p.GetGetMethod().Invoke(newInstance, null)).Index = index;
			return newInstance;
		}
	}
}
