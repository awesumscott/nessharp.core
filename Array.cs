using System;

namespace NESSharp.Core {
	public class Array<T> : Var where T : Var, new() {
		private T[]? Vars;

		public Array() {}
		public static Array<T> New(int len, RAM r, string name) {
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
		public T this[int index] {
			get {
				if (Vars is null) throw new Exception("Array hasn't been dimmed");
				if (index >= 0 && index < Vars.Length)
					return Vars[index];
				throw new Exception("Index out of range");
			}
		}
		public T this[IndexingRegister reg] {
			get {
				var copy = new T();
				copy.Copy(Vars[0]);
				copy.Name = Name;
				copy.Index = reg;
				return copy;
			}
		}
	}
}
