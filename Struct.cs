using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static NESSharp.Core.AL;

namespace NESSharp.Core {

	public struct StructField {
		public Type Type;
		public string Name;
		public int Length;
		public StructField(Type type, string name, int arrayLength = 1) {
			Type = type;
			Name = name;
			Length = arrayLength;
		}
	};

	public class Struct : Var {
		//TODO: determine if Size should be a static property tallied and cached once. If so, remove tallying from New and use the static prop.
		public override Var Dim(RAM ram, string name) => throw new Exception("Lazy-dimming of structs not yet supported. Use Struct.New<T> for now."); //TODO: try to implement this for struct-in-struct support

		[Obsolete]
		public static StructField Field(Type type, string name, int arrayLength = 1) {
			return new StructField(type, name, arrayLength);
		}
		public static T New<T>(RAM ram, string name) where T : Struct, new() {
			var structInstance = new T();
			var size = 0;
			foreach (var p in structInstance.GetType().GetProperties().Where(x => typeof(Var).IsAssignableFrom(x.PropertyType)).ToList()) {
				//Console.WriteLine("Property type: " + p.PropertyType.ToString());
				var v = (Var)Activator.CreateInstance(p.PropertyType);
				v.Dim(ram, $"{ (string.IsNullOrEmpty(name) ? structInstance.GetType().Name : name) }_{ p.Name }");
				size += v.Size;
				structInstance.GetType().InvokeMember(p.Name, BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.Public, null, structInstance, new object[]{ v });
			}
			structInstance.Size = size;
			return structInstance;
		}
		public static T Get<T>(IndexingRegisterBase offset) where T : Struct, new() {
			//TODO: figure out a way to set this up for SoA first
			//		-SoA should dim these already, then be able to instantiate one of these with the starting addrs and an offset register
			//		DONE-InvokeMember for setting the OffsetRegister on each var
			var structInstance = new T();
			structInstance.GetType().InvokeMember("OffsetRegister", BindingFlags.SetField | BindingFlags.Instance | BindingFlags.Public, null, structInstance, new object[]{ offset });
			foreach (var p in structInstance.GetType().GetProperties().Where(x => typeof(Var).IsAssignableFrom(x.PropertyType)).ToList()) {//.WithAttribute<FieldDef>()) {
				//Console.WriteLine("Property type: " + p.PropertyType.ToString());
				var v = (Var)Activator.CreateInstance(p.PropertyType);
				//v.Init(ram, $"{ structInstance.GetType().Name }_{ p.Name }");
				structInstance.GetType().InvokeMember(p.Name, BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.Public, null, structInstance, new object[]{ v });
			}
			return structInstance;
		}
		public T Copy<T>(T original) where T : Struct, new() {
			var newInstance = new T();
			var newInstanceProperties = newInstance.GetType().GetProperties();

			foreach (var prop in newInstanceProperties) {
				if (prop.PropertyType.IsSubclassOf(typeof(Var))) {
					var v = (Var)Activator.CreateInstance(prop.PropertyType);
					prop.SetValue(newInstance, v.Copy((Var)prop.GetValue(original)));
				//The following condition is probably not necessary because AoS now explicitly sets and unsets indexes on Var properties
				//} else if (prop.Name == nameof(Index)) {
				//	//don't copy
				} else {
					prop.SetValue(newInstance, prop.GetValue(original));
				}
			}
			return newInstance;
		}
		
	}

	public class StructOfArrays<StructType> : Var where StructType : Struct, new() {
		private Struct _baseInstance;
		private Array[] _arrays;
		public static StructOfArrays<StructType> New(string name, int arrayLength) {
			var soa = new StructOfArrays<StructType>();
			soa.Name = name;
			soa.Length = arrayLength;
			soa._baseInstance = new StructType();
			return soa;
		}
		//TODO: see if the Dim(ram,name) override could just set or use existing name, and call Dim(ram) (or preferably just consolidate them somehow!)
		public StructOfArrays<StructType> Dim(RAM ram) {
			var arrs = new List<Array>();
			foreach (var p in _baseInstance.GetType().GetProperties().Where(x => typeof(Var).IsAssignableFrom(x.PropertyType)).ToList()) {
				//TODO: if property type is derived from VarN, dim one array of VBytes per byte
				//TODO: maybe have Size as a property attribute?

				//var sizeAttr = (VarSize)Attribute.GetCustomAttribute(p.PropertyType, typeof(VarSize));
				var numBytes = VarSize.GetSizeOf(p.PropertyType); //sizeAttr.Size; //(int)p.PropertyType.GetProperty(nameof(Size_New)).GetValue(null);

				if (numBytes == -1) throw new Exception("Variable length types cannot be used in structs--make a new Var type with a fixed length"); //Alternative: a Size attribute on the property (may not work for Decimal, which has two size args)
				if (numBytes > 1) {
					//throw new Exception("in here");
					for (var i = 0; i < numBytes; i++) {
						var a = Array.New(typeof(VByte));
						a.Length = Length;
						arrs.Add((Array)a.Dim(ram, $"{ _baseInstance.GetType().Name }_{ p.Name }_{ i }"));
					}
				} else if (numBytes == 1) {
					var a = Array.New(p.PropertyType);
					a.Length = Length;
					arrs.Add((Array)a.Dim(ram, $"{ _baseInstance.GetType().Name }_{ p.Name }"));
				}
			}
			_arrays = arrs.ToArray();
			return this;
		}
		public StructType this[IndexingRegisterBase index] {
			get {
				var structType = _baseInstance.GetType();
				var newInstance = (Struct)Activator.CreateInstance(structType);
				newInstance.Name = _baseInstance.Name;
				newInstance.Length = _baseInstance.Length;
				//newInstance.Size = _baseInstance.Size; //TODO: NYI
				var i = 0;
				foreach (var p in _baseInstance.GetType().GetProperties().Where(x => typeof(Var).IsAssignableFrom(x.PropertyType)).ToList()) { //user-defined Var properties
					//if type has >1 bytes, grab values from appropriate arrays, create an instance, and set its address array properly
					var numBytes = VarSize.GetSizeOf(p.PropertyType);
					//var v = (Var)Activator.CreateInstance(_arrays[i].BaseVar.GetType());
					var v = (Var)Activator.CreateInstance(p.PropertyType);
					if (numBytes > 1) {
						//throw new Exception("in here");
						var bytes = new List<Var>();
						for (var byteIndex = 0; byteIndex < numBytes; byteIndex++) {
							bytes.Add(_arrays[i].BaseVar);
							i++;
						}
						i--;
						v.Copy(bytes);
					} else if (numBytes == 1) {
						v.Copy(_arrays[i].BaseVar);
					}
					v.Index = index;
					structType.InvokeMember(p.Name, BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.Public, null, newInstance, new object[]{
						v
					});
					i++;
				}
				//TODO: see if copying is possible (if it seems necessary or helpful)
				//var newInstance = _baseInstance.Copy();
				newInstance.Index = index;
				return (StructType)newInstance;
			}
		}
		public StructType this[int index] {
			get {
				var structType = _baseInstance.GetType();
				var newInstance = (Struct)Activator.CreateInstance(structType);
				newInstance.Name = _baseInstance.Name;
				newInstance.Length = _baseInstance.Length;
				//newInstance.Size = _baseInstance.Size; //TODO: NYI
				var i = 0;
				foreach (var p in _baseInstance.GetType().GetProperties().Where(x => typeof(Var).IsAssignableFrom(x.PropertyType)).ToList()) { //user-defined Var properties
					//if type has >1 bytes, grab values from appropriate arrays, create an instance, and set its address array properly
					var numBytes = VarSize.GetSizeOf(p.PropertyType);
					//var v = (Var)Activator.CreateInstance(_arrays[i].BaseVar.GetType());
					var v = (Var)Activator.CreateInstance(p.PropertyType);
					if (numBytes > 1) {
						//throw new Exception("in here");
						var bytes = new List<Var>();
						for (var byteIndex = 0; byteIndex < numBytes; byteIndex++) {
							bytes.Add(_arrays[i].BaseVar); //TODO: verify this works in this particular indexing style
							i++;
						}
						i--;
						v.Copy(bytes);
					} else if (numBytes == 1) {
						v.Copy(_arrays[i][index]);
					}

					structType.InvokeMember(p.Name, BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.Public, null, newInstance, new object[]{
						v
					});
					i++;
				}
				//TODO: see if copying is possible (if it seems necessary or helpful)
				//var newInstance = _baseInstance.Copy();
				return (StructType)newInstance;
			}
		}
		public void Clear(byte clearValue = 0) {
			Loop.Descend_Pre(X.Set(Length), () => {
				var structType = _baseInstance.GetType();
				var newInstance = (Struct)Activator.CreateInstance(structType);
				newInstance.Name = _baseInstance.Name;
				newInstance.Length = _baseInstance.Length;
				var i = 0;
				foreach (var p in _baseInstance.GetType().GetProperties().Where(x => typeof(Var).IsAssignableFrom(x.PropertyType)).ToList()) { //user-defined Var properties
					var vType = _arrays[i].BaseVar.GetType();
					var v = (Var)Activator.CreateInstance(vType);
					v.Copy(_arrays[i].BaseVar);
					v.Index = X;
					vType.InvokeMember("Set", BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.Public, null, v, new object[]{
						(U8)clearValue
					});
					i++;
				}
			});
		}
	}

	public class ArrayOfStructs<StructType> : Var where StructType : Struct, new() {
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
			//var arrs = new List<Array>();
			//foreach (var p in _baseInstance.GetType().GetProperties().Where(x => typeof(Var).IsAssignableFrom(x.PropertyType)).ToList()) {
			//	var a = Array.New(p.PropertyType);
			//	a.Length = Length;
			//	arrs.Add((Array)a.Dim(ram, $"{ _baseInstance.GetType().Name }_{ p.Name }"));
			//}
			//_arrays = arrs.ToArray();
			var structs = new List<StructType>();
			for (var i = 0; i < Length; i++) {
				structs.Add(Struct.New<StructType>(ram, $"{ typeof(StructType).Name}_{i}" ));

			}
			_structs = structs.ToArray();
			return this;
		}


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
		//public StructType this[RegisterBase offset] {
		//	get {
		//		var structType = _baseInstance.GetType();
		//		var newInstance = ((StructType)Activator.CreateInstance(structType)).Copy(_structs[0]);
		//		newInstance.OffsetRegister = offset;
		//		foreach (var p in newInstance.GetType().GetProperties().Where(x => typeof(Var).IsAssignableFrom(x.PropertyType)).Where(x => x.Name == nameof(Var.OffsetRegister)).ToList()) { //user-defined Var properties
		//			structType.InvokeMember(p.Name, BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.Public, null, newInstance, new object[]{
		//				offset
		//			});
		//		}
		//		return newInstance;
		//	}
		//}
		public StructType this[U8 offset] {
			get {
				var structType = _baseInstance.GetType();
				var newInstance = ((StructType)Activator.CreateInstance(structType)).Copy(_structs[offset]);
				foreach (var p in newInstance.GetType().GetProperties().Where(x => typeof(Var).IsAssignableFrom(x.PropertyType)).ToList()) { //user-defined Var properties
					((Var)p.GetGetMethod().Invoke(newInstance, null)).Index = null;
				}
				//foreach (var p in newInstance.GetType().GetProperties().Where(x => typeof(Var).IsAssignableFrom(x.PropertyType)).Where(x => x.Name == nameof(Var.OffsetRegister)).ToList()) { //user-defined Var properties
				//	structType.InvokeMember(p.Name, BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.Public, null, newInstance, new object[]{
				//		offset
				//	});
				//}
				return newInstance;
			}
		}
		public StructType this[IndexingRegisterBase reg] {
			//TODO: maybe only allow VByte indexing if struct.length is a power of two? or just do the damn multiply
			//TODO: property references are the same as item[0]! make sure these get copied and not referenced, so Indexes don't hang around
			get {
				var structType = _baseInstance.GetType();
				var newInstance = ((StructType)Activator.CreateInstance(structType)).Copy(_structs[0]);
				foreach (var p in newInstance.GetType().GetProperties().Where(x => typeof(Var).IsAssignableFrom(x.PropertyType)).ToList()) { //user-defined Var properties
					((Var)p.GetGetMethod().Invoke(newInstance, null)).Index = reg;
					//structType.InvokeMember(p.Name, BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.Public, null, newInstance, new object[]{
					//	reg
					//});
				}
				return newInstance;
			}
		}
	}

	[Obsolete]
	public class ArrayOfStructs_old : Var {
		public List<Dictionary<string, Var>> FieldsArray;
		private StructField[] _fieldDefs;
		private int _length;
		public static ArrayOfStructs_old New(string name, int length, params StructField[] fields) {
			var s = new ArrayOfStructs_old();
			s.Name = name;
			s._fieldDefs = fields;
			s._length = length;
			return s;
		}
		public static StructField Field(Type type, string name, int arrayLength = 1) {
			return new StructField(type, name, arrayLength);
		}
		public ArrayOfStructs_old Dim(RAM ram) {
			if (FieldsArray != null) throw new Exception("Struct already dimmed");
			
			FieldsArray = new List<Dictionary<string, Var>>();
			for (var i = 0; i < _length; i++) {
				var fields = new Dictionary<string, Var>();
				foreach (var field in _fieldDefs) {
					Var newVar;
					if (field.Length == 1) {
						newVar = (Var)Activator.CreateInstance(field.Type);
					} else {
						var newArray = Activator.CreateInstance(field.Type);
						field.Type.InvokeMember("Length", BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty,
							Type.DefaultBinder, newArray, new object[]{field.Length});
						newVar = (Var)newArray;
					}
					newVar.Dim(ram, $"{ Name }{ i }_{ field.Name }");

					fields.Add(field.Name, newVar);
				}
				FieldsArray.Add(fields);
			}

			return this;
		}
		public Dictionary<string, Var> this[U8 key] {
			get {
				return FieldsArray[key];
			}
		}
		public void ForEach(Action block) {
			Y.Set((U8)FieldsArray.Count());
			Loop.Do(() => {
				block.Invoke();
				Y--;
			}).While(() => Y.NotEquals(0));
		}
	}
}
