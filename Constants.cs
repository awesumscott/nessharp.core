using System;
using System.Collections.Generic;
using System.Linq;

namespace NESSharp.Core;

public interface IConstant {
	public string Name { get; set; }
	public object Value { get; set; }
}
public class ConstU8 : IConstant, IResolvable<U8> {
	public string Name { get; set; }
	public object Value { get; set; }

	U8 IOperand<U8>.Value => (U8)Value;

	public ConstU8(string name, U8 val) {
		Name = name;
		Value = val;
	}
	
	public bool CanResolve() => true;
	public object Source => Value;
	public U8 Resolve() => (U8)Value;
	public string ToAsmString(Tools.INESAsmFormatting formats) => ((U8)Value).ToString();
}
public class ConstU16 : IConstant, IResolvable<U16> {
	public string Name { get; set; }
	public object Value { get; set; }

	U16 IOperand<U16>.Value => (U16)Value;

	public ConstU16(string name, U16 val) {
		Name = name;
		Value = val;
	}
	
	public bool CanResolve() => true;
	public object Source => Value;
	public U16 Resolve() => (U16)Value;
	public string ToAsmString(Tools.INESAsmFormatting formats) => ((U16)Value).ToString();
}
public class ConstantCollection {
	private readonly Dictionary<string, IConstant> _consts = new Dictionary<string, IConstant>();
	public IConstant Add(IConstant constant) {
		_consts.Add(constant.Name, constant);
		return constant;
	}
	public bool Contains(string name) => _consts.ContainsKey(name);
	public IResolvable<U8> U8(string name) => _consts.ContainsKey(name) && _consts[name] is ConstU8 ? (IResolvable<U8>)_consts[name].Value : throw new Exception($"{ name } not a U8");
	public IResolvable<U16> U16(string name) => _consts.ContainsKey(name) && _consts[name] is ConstU16 ? (IResolvable<U16>)_consts[name].Value : throw new Exception($"{ name } not a U16");
	public IConstant this[string key] {
		get {
			if (!_consts.ContainsKey(key)) throw new Exception($"Constant doesn't exist: { key }");
			return _consts[key];
		}
		set {
			if (!_consts.ContainsKey(key))
				_consts.Add(key, value);
		}
	}
	public IConstant[] Items => _consts.Select(x => x.Value).ToArray();
}
