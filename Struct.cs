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
		public override Var Dim(RAM ram, string name) => throw new Exception("Lazy-dimming of structs not yet supported. Use Struct.New<T> for now."); //TODO: try to implement this for struct-in-struct support

		public static StructField Field(Type type, string name, int arrayLength = 1) {
			return new StructField(type, name, arrayLength);
		}
		public static Struct New<T>(RAM ram) where T : Struct, new() {
			var structInstance = new T();
			foreach (var p in structInstance.GetType().GetProperties().Where(x => typeof(Var).IsAssignableFrom(x.PropertyType)).ToList()) {
				Console.WriteLine("Property type: " + p.PropertyType.ToString());
				var v = (Var)Activator.CreateInstance(p.PropertyType);
				v.Dim(ram, $"{ structInstance.GetType().Name }_{ p.Name }");
				structInstance.GetType().InvokeMember(p.Name, BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.Public, null, structInstance, new object[]{ v });
			}
			return structInstance;
		}
		public static Struct Get<T>(RegisterBase offset) where T : Struct, new() {
			//TODO: figure out a way to set this up for SoA first
			//		-SoA should dim these already, then be able to instantiate one of these with the starting addrs and an offset register
			//		DONE-InvokeMember for setting the OffsetRegister on each var
			var structInstance = new T();
			structInstance.GetType().InvokeMember("OffsetRegister", BindingFlags.SetField | BindingFlags.Instance | BindingFlags.Public, null, structInstance, new object[]{ offset });
			foreach (var p in structInstance.GetType().GetProperties().Where(x => typeof(Var).IsAssignableFrom(x.PropertyType)).ToList()) {//.WithAttribute<FieldDef>()) {
				Console.WriteLine("Property type: " + p.PropertyType.ToString());
				var v = (Var)Activator.CreateInstance(p.PropertyType);
				//v.Init(ram, $"{ structInstance.GetType().Name }_{ p.Name }");
				structInstance.GetType().InvokeMember(p.Name, BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.Public, null, structInstance, new object[]{ v });
			}
			return structInstance;
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
				var a = Array.New(p.PropertyType);
				a.Length = Length;
				arrs.Add((Array)a.Dim(ram, $"{ _baseInstance.GetType().Name }_{ p.Name }"));
			}
			_arrays = arrs.ToArray();
			return this;
		}
		public StructType this[RegisterBase offset] {
			get {
				var structType = _baseInstance.GetType();
				var newInstance = (Struct)Activator.CreateInstance(structType);
				newInstance.Name = _baseInstance.Name;
				newInstance.Length = _baseInstance.Length;
				//newInstance.Size = _baseInstance.Size; //TODO: NYI
				var i = 0;
				foreach (var p in _baseInstance.GetType().GetProperties().Where(x => typeof(Var).IsAssignableFrom(x.PropertyType)).ToList()) { //user-defined Var properties
					var v = (Var)Activator.CreateInstance(_arrays[i].BaseVar.GetType());
					v.Copy(_arrays[i].BaseVar);
					v.OffsetRegister = offset;
					structType.InvokeMember(p.Name, BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.Public, null, newInstance, new object[]{
						v
					});
					i++;
				}
				//TODO: see if copying is possible (if it seems necessary or helpful)
				//var newInstance = _baseInstance.Copy();
				newInstance.OffsetRegister = offset;
				return (StructType)newInstance;
			}
		}
		public StructType this[int offset] {
			get {
				var structType = _baseInstance.GetType();
				var newInstance = (Struct)Activator.CreateInstance(structType);
				newInstance.Name = _baseInstance.Name;
				newInstance.Length = _baseInstance.Length;
				//newInstance.Size = _baseInstance.Size; //TODO: NYI
				var i = 0;
				foreach (var p in _baseInstance.GetType().GetProperties().Where(x => typeof(Var).IsAssignableFrom(x.PropertyType)).ToList()) { //user-defined Var properties
					var v = (Var)Activator.CreateInstance(_arrays[i].BaseVar.GetType());
					v.Copy(_arrays[i][offset]);
					//v.OffsetRegister = offset;
					structType.InvokeMember(p.Name, BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.Public, null, newInstance, new object[]{
						v
					});
					i++;
				}
				//TODO: see if copying is possible (if it seems necessary or helpful)
				//var newInstance = _baseInstance.Copy();
				//newInstance.OffsetRegister = offset;
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
					v.OffsetRegister = X;
					vType.InvokeMember("Set", BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.Public, null, v, new object[]{
						(U8)clearValue
					});
					i++;
				}
			});
		}
	}

	public class ArrayOfStructs : Var {
		public List<Dictionary<string, Var>> FieldsArray;
		private StructField[] _fieldDefs;
		private int _length;
		public static ArrayOfStructs New(string name, int length, params StructField[] fields) {
			var s = new ArrayOfStructs();
			s.Name = name;
			s._fieldDefs = fields;
			s._length = length;
			return s;
		}
		public static StructField Field(Type type, string name, int arrayLength = 1) {
			return new StructField(type, name, arrayLength);
		}
		public ArrayOfStructs Dim(RAM ram) {
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
					newVar.Dim(ram, $"{ Name }{ i.ToString() }_{ field.Name }");

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
