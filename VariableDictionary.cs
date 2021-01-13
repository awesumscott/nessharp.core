using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace NESSharp.Core {
	//TODO: call methods on an obj implementing a VarWatcher interface to support emulator debug file generation
	public class VariableDictionary : IEnumerable {//: IDictionary<string, IVarAddressArray> {
		private Dictionary<string, IVarAddressArray> _dict = new();

		public IVarAddressArray this[string key] { get => ((IDictionary<string, IVarAddressArray>)_dict)[key]; set => ((IDictionary<string, IVarAddressArray>)_dict)[key] = value; }
		public ICollection<string> Keys => ((IDictionary<string, IVarAddressArray>)_dict).Keys;
		public ICollection<IVarAddressArray> Values => ((IDictionary<string, IVarAddressArray>)_dict).Values;
		public int Count => ((ICollection<KeyValuePair<string, IVarAddressArray>>)_dict).Count;
		public bool IsReadOnly => ((ICollection<KeyValuePair<string, IVarAddressArray>>)_dict).IsReadOnly;

		public void Add(string key, IVarAddressArray value) {
			((IDictionary<string, IVarAddressArray>)_dict).Add(key, value);
		}

		public void Add(KeyValuePair<string, IVarAddressArray> item) {
			((ICollection<KeyValuePair<string, IVarAddressArray>>)_dict).Add(item);
		}

		public void Clear() => ((ICollection<KeyValuePair<string, IVarAddressArray>>)_dict).Clear();
		public bool Contains(KeyValuePair<string, IVarAddressArray> item) => ((ICollection<KeyValuePair<string, IVarAddressArray>>)_dict).Contains(item);
		public bool ContainsKey(string key) => ((IDictionary<string, IVarAddressArray>)_dict).ContainsKey(key);
		public void CopyTo(KeyValuePair<string, IVarAddressArray>[] array, int arrayIndex) => ((ICollection<KeyValuePair<string, IVarAddressArray>>)_dict).CopyTo(array, arrayIndex);
		public IEnumerator<KeyValuePair<string, IVarAddressArray>> GetEnumerator() => ((IEnumerable<KeyValuePair<string, IVarAddressArray>>)_dict).GetEnumerator();
		public bool Remove(string key) => ((IDictionary<string, IVarAddressArray>)_dict).Remove(key);
		public bool Remove(KeyValuePair<string, IVarAddressArray> item) => ((ICollection<KeyValuePair<string, IVarAddressArray>>)_dict).Remove(item);
		public bool TryGetValue(string key, [MaybeNullWhen(false)] out IVarAddressArray value) => ((IDictionary<string, IVarAddressArray>)_dict).TryGetValue(key, out value);
		IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_dict).GetEnumerator();
	}
}
