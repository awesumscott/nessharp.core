using System;

namespace NESSharp.Core {
	public class Array : Var {
		private Var[]? Vars;
		public Var BaseVar;
		private Type _varType;
		public override int Length { get; set; } = 0;
		public static Array New<T>() where T : Var, new() {
			var arr = new Array();
			arr._varType = typeof(T);
			return arr;
		}
		public static Array New(Type t) {
			var arr = new Array();
			arr._varType = t;
			return arr;
		}
		public override Var Dim(RAM ram, string name) {
			if (!(Vars is null)) throw new Exception("Array already dimmed");
			if (Length <= 0) throw new Exception("Set Length before dimming");
			Vars = new Var[Length];
			for (var i = 0; i < Length; i++) {
				var v = (Var?)Activator.CreateInstance(_varType);
				if (v != null) {
					v.Dim(ram, $"{ name }_{ i }");
					Vars[i] = v;
				} else throw new Exception("Dim failed");
			}
			BaseVar = Vars[0];
			Name = name;
			return this;
		}
		public Var this[int index] {
			get {
				if (Vars is null) throw new Exception("Array hasn't been dimmed");
				if (index >= 0 && index < Vars.Length)
					return Vars[index];
				throw new Exception("Index out of range");
			}
		}
	}
	public class Array<T> : Var where T : Var, new() {
		private T[]? Vars;

		public Array() {}
		//public Array(RAM ram, int len, string name) {
		//	Vars = new T[len];
		//	for (var i = 0; i < len; i++) {
		//		var v = new T();
		//		v.Dim(ram, $"{ name }_{ i.ToString() }");
		//		Vars[i] = v;
		//	}
		//	Name = name;
		//}

		public static Array<T> New(U8 len, RAM r, string name) {
			var arr = new Array<T>(){Length = len};
			arr.Dim(r, name);
			return arr;
		}

		public override int Size => throw new NotImplementedException();
		public override Var Dim(RAM ram, string name) {
			if (!(Vars is null)) throw new Exception("Array already dimmed");
			Vars = new T[Length];
			for (var i = 0; i < Length; i++) {
				var v = new T();
				v.Dim(ram, $"{ name }_{ i }");
				Vars[i] = v;
			}
			Name = name;
			return this;
		}
		public override Var Copy(Var v) => throw new NotImplementedException();
		//public Array<T> New(RAM ram, int len, string name) {
		//	return new Array<T>(ram, len, name); //TODO
		//}
		public T this[int index] {
			get {
				if (Vars is null) throw new Exception("Array hasn't been dimmed");
				if (index >= 0 && index < Vars.Length)
					return Vars[index];
				throw new Exception("Index out of range");
			}
		}
		public T this[IndexingRegisterBase reg] {
			get {
				var copy = new T();
				copy.Copy(Vars[0]);
				//copy.Address = Vars[0].Address;
				copy.Name = Name;
				copy.Index = reg;
				return copy;
			}
		}
	}
}
