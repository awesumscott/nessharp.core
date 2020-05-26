using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using static NESSharp.Core.AL;

namespace NESSharp.Core {
	[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
	public class PrgBankDef : Attribute {
		public int Id { get; private set; }
		public PrgBankDef(int bankId) {
			Id = bankId;
		}
	}
	//[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
	//public class ChrBankDef : Attribute {
	//	public int Id { get; private set; }
	//	public ChrBankDef(int bankId) {
	//		Id = bankId;
	//	}
	//}

	[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
	public class Subroutine : Attribute {}

	[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
	public class Interrupt : Attribute {}

	[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
	public class CodeSection : Attribute {}

	[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
	public class DataSection : Attribute {}

	[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
	public class Dependencies : Attribute {}
	
	[Flags]
	public enum Register {
		None =	0,
		A =		1,
		X =		2,
		Y =		4,
		All =	7
	};
	[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
	public class RegParam : Attribute {
		public Register Register { get; private set; }
		public RegParam(Register reg, string description) {
			Register = reg;
		}
	}

	public static class AttributeHelpers {
		public class TypeAndAttribute<AttributeType> {
			public Type type;
			public AttributeType attribute;
		}
		public class MethodAndAttribute<AttributeType> {
			public MethodInfo method;
			public AttributeType attribute;
		}
		public class PropertyAndAttribute<AttributeType> {
			public PropertyInfo property;
			public AttributeType attribute;
		}
		//public static IEnumerable<TypeAndAttribute<AttributeType>> WithAttribute<AttributeType>(this Type[] classTypes) where AttributeType : Attribute {
		//	return classTypes.Select(x => new TypeAndAttribute<AttributeType>(){ type = x, attribute = (AttributeType)Attribute.GetCustomAttribute(x, typeof(AttributeType))}).Where(x => x.attribute != null);
		//}
		public static IEnumerable<MethodAndAttribute<AttributeType>> WithAttribute<AttributeType>(this MethodInfo[] methodInfos) where AttributeType : Attribute {
			return methodInfos.Select(x => new MethodAndAttribute<AttributeType>(){ method = x, attribute = (AttributeType)Attribute.GetCustomAttribute(x, typeof(AttributeType))}).Where(x => x.attribute != null);
		}
		//public static IEnumerable<PropertyAndAttribute<AttributeType>> WithAttribute<AttributeType>(this PropertyInfo[] propertyInfos) where AttributeType : Attribute {
		//	return propertyInfos.Select(x => new PropertyAndAttribute<AttributeType>(){ property = x, attribute = (AttributeType)Attribute.GetCustomAttribute(x, typeof(AttributeType))}).Where(x => x.attribute != null);
		//}
	}
}
