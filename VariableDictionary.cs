using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace NESSharp.Core;

//TODO: call methods on an obj implementing a VarWatcher interface to support emulator debug file generation
public class VariableDictionary : IEnumerable {//: IDictionary<string, Var> {
	private Dictionary<string, Var> _dict = new();

	public Var this[string key] { get => ((IDictionary<string, Var>)_dict)[key]; set => ((IDictionary<string, Var>)_dict)[key] = value; }
	public ICollection<string> Keys => ((IDictionary<string, Var>)_dict).Keys;
	public ICollection<Var> Values => ((IDictionary<string, Var>)_dict).Values;
	public int Count => ((ICollection<KeyValuePair<string, Var>>)_dict).Count;
	public bool IsReadOnly => ((ICollection<KeyValuePair<string, Var>>)_dict).IsReadOnly;

	public void Add(string key, Var value) => ((IDictionary<string, Var>)_dict).Add(key, value);
	public void Add(KeyValuePair<string, Var> item) => ((ICollection<KeyValuePair<string, Var>>)_dict).Add(item);

	public void Clear() => ((ICollection<KeyValuePair<string, Var>>)_dict).Clear();
	public bool Contains(KeyValuePair<string, Var> item) => ((ICollection<KeyValuePair<string, Var>>)_dict).Contains(item);
	public bool ContainsKey(string key) => ((IDictionary<string, Var>)_dict).ContainsKey(key);
	public void CopyTo(KeyValuePair<string, Var>[] array, int arrayIndex) => ((ICollection<KeyValuePair<string, Var>>)_dict).CopyTo(array, arrayIndex);
	public IEnumerator<KeyValuePair<string, Var>> GetEnumerator() => ((IEnumerable<KeyValuePair<string, Var>>)_dict).GetEnumerator();
	public bool Remove(string key) => ((IDictionary<string, Var>)_dict).Remove(key);
	public bool Remove(KeyValuePair<string, Var> item) => ((ICollection<KeyValuePair<string, Var>>)_dict).Remove(item);
	public bool TryGetValue(string key, [MaybeNullWhen(false)] out Var value) => ((IDictionary<string, Var>)_dict).TryGetValue(key, out value);
	IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_dict).GetEnumerator();
}
