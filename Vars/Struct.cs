using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NESSharp.Core;

public class Struct : Var {
	//TODO: determine if Size should be a static property tallied and cached once. If so, remove tallying from New and use the static prop.
	public override Var Dim(RAMRange ram, string name) => throw new Exception("Lazy-dimming of structs not yet supported. Use Struct.New<T> for now."); //TODO: try to implement this for struct-in-struct support

	public static T New<T>(RAMRange ram, string name) where T : Struct, new() {
		var structInstance = new T();
		var size = 0;
		foreach (var p in structInstance.GetType().GetProperties().Where(x => typeof(Var).IsAssignableFrom(x.PropertyType)).ToList()) {
			var v = (Var?)Activator.CreateInstance(p.PropertyType);
			v.Dim(ram, $"{ (string.IsNullOrEmpty(name) ? structInstance.GetType().Name : name) }_{ p.Name }");
			size += v.Size;
			structInstance.GetType().InvokeMember(p.Name, BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.Public, null, structInstance, new object[]{ v });
		}
		structInstance.Size = size;
		return structInstance;
	}
	public T Copy<T>(T original) where T : Struct, new() {
		var newInstance = new T();
		var newInstanceProperties = newInstance.GetType().GetProperties();

		//TODO: since this is only used in AoS now, and no AoS has used a multi-byte var, ensure this works with mbvs, and then use it in SoA[]
		foreach (var prop in newInstanceProperties) {
			if (prop.PropertyType.IsSubclassOf(typeof(Var))) {
				var v = (Var?)Activator.CreateInstance(prop.PropertyType);
				prop.SetValue(newInstance, v.Copy((Var)prop.GetValue(original)));
			//The following condition is probably not necessary because AoS now explicitly sets and unsets indexes on Var properties
			//} else if (prop.Name == nameof(Index)) {
			//	//don't copy
			//} else {
			//	prop.SetValue(newInstance, prop.GetValue(original));
			}
		}
		return newInstance;
	}
	
	public override Var Copy(IEnumerable<Var> vars) {
		var properties = GetType().GetProperties();
		var i = 0;
		var varArray = vars.ToArray();
		foreach (var prop in properties) {
			if (prop.PropertyType.IsSubclassOf(typeof(Var))) {
				//SoA can copy these references, but in case this method ever gets used elsewhere, it's probably best to leave this extra copy.
				var v8 = (Var?)Activator.CreateInstance(prop.PropertyType);
				prop.SetValue(this, v8.Copy(varArray[i]));	//varArray[i]); //(the direct ref version)
				i++;
			}
		}
		return this;
	}
}
