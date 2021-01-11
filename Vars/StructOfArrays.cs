using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static NESSharp.Core.AL;

namespace NESSharp.Core {
	public class StructOfArrays<StructType> : Var where StructType : Struct, new() {
		private static byte nameSuffix = 0;
		private Struct _baseInstance;
		private Array<VByte>[] _arrays;
		public static StructOfArrays<StructType> New(string name, int arrayLength) {
			var soa = new StructOfArrays<StructType>();
			soa.Name = name;
			soa.Length = arrayLength;
			soa._baseInstance = new StructType();
			return soa;
		}
		//TODO: see if the Dim(ram,name) override could just set or use existing name, and call Dim(ram) (or preferably just consolidate them somehow!)
		public StructOfArrays<StructType> Dim(RAM ram) {
			var arrs = new List<Array<VByte>>();
			foreach (var p in _baseInstance.GetType().GetProperties().Where(x => typeof(Var).IsAssignableFrom(x.PropertyType)).ToList()) {
				var numBytes = VarSize.GetSizeOf(p.PropertyType);
				if (numBytes < 1) throw new Exception("Variable length types cannot be used in structs--make a new Var type with a fixed length"); //Alternative: a Size attribute on the property (may not work for Decimal, which has two size args)

				if (numBytes == 1) {
					arrs.Add(Array<VByte>.New((U8)Length, ram, $"{ _baseInstance.GetType().Name }{nameSuffix++}_{ p.Name }"));
				} else {
					for (var i = 0; i < numBytes; i++)
						arrs.Add(Array<VByte>.New((U8)Length, ram, $"{ _baseInstance.GetType().Name }{nameSuffix++}_{ p.Name }_{ i }"));
				}
			}
			_arrays = arrs.ToArray();
			Size = Length * arrs.Count;//VarSize.GetSizeOf(typeof(StructType));
			return this;
		}
		public StructType this[IndexingRegister index] => _makeCopy(0, index);
		public StructType this[int index] => _makeCopy(index, null);
		private StructType _makeCopy(int offset, IndexingRegister index) {
				var structType = _baseInstance.GetType();
				var newInstance = (Struct)Activator.CreateInstance(structType);
				newInstance.Name = _baseInstance.Name;
				newInstance.Length = _baseInstance.Length;
				//newInstance.Size = _baseInstance.Size; //TODO: NYI
				var i = 0;
				foreach (var p in _baseInstance.GetType().GetProperties().Where(x => typeof(Var).IsAssignableFrom(x.PropertyType)).ToList()) { //user-defined Var properties
					//if type has >1 bytes, grab values from appropriate arrays, create an instance, and copy over properties
					var numBytes = VarSize.GetSizeOf(p.PropertyType);
					var v = (Var)Activator.CreateInstance(p.PropertyType);
					v.Index = index;
					if (numBytes > 1) {
						//Make a duplicate list of bytes of item[0], to prepare to copy into the new Struct/Var instance
						var bytes = new List<Var>();
						for (var byteIndex = 0; byteIndex < numBytes; byteIndex++) {
							//Additional duplication here to set Index properties
							var newB = new VByte();
							newB.Copy(_arrays[i][offset]);
							newB.Index = index;
							bytes.Add(newB);
							i++;
						}
						i--;
						v.Copy(bytes);	//Move the bytes into the struct/var
					} else if (numBytes == 1) {
						v.Copy(_arrays[i][offset]);
					}
					v.Index = index;
					structType.InvokeMember(p.Name, BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.Public, null, newInstance, new object[]{ v });
					i++;
				}
				newInstance.Index = index;
				return (StructType)newInstance;
		}

		public void Clear(byte clearValue = 0) {
			Loop.Descend_Pre(X.Set(Length), _ => {
				var structType = _baseInstance.GetType();
				var newInstance = (Struct)Activator.CreateInstance(structType);
				newInstance.Name = _baseInstance.Name;
				newInstance.Length = _baseInstance.Length;
				var i = 0;
				foreach (var p in _baseInstance.GetType().GetProperties().Where(x => typeof(Var).IsAssignableFrom(x.PropertyType)).ToList()) { //user-defined Var properties
					var vType = _arrays[i][0].GetType();
					var v = (Var)Activator.CreateInstance(vType);
					v.Copy(_arrays[i][0]);
					v.Index = X;
					vType.InvokeMember("Set", BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.Public, null, v, new object[]{ clearValue });
					i++;
				}
			});
		}
	}
}
