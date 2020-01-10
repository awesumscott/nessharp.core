using System;
using System.Collections.Generic;
using System.Linq;

namespace NESSharp.Core {
	public class ArgDictionary {
		private Dictionary<string, string> _dict;
		public ArgDictionary(string[] args) {
			_dict = new Dictionary<string, string>();
			var i = 0;
			bool hasNext() => i + 1 < args.Length;
			string cur() => args[i];

			for (i = 0; i < args.Length; i++) {
				if (cur().StartsWith("--")) {
					var expr = cur().Substring(2).Split("=").Where(x => !string.IsNullOrEmpty(x)).ToArray();
					if (expr.Length == 2)
						_dict.Add(expr[0], expr[1]);
					else {
						Console.WriteLine("Invalid arg format: " + cur());
						Environment.Exit(1);
					}
				} else if (cur().StartsWith("-")) {
					var key = cur().Substring(1);//.Skip(1).ToArray().ToString();
					if (!hasNext()) {
						Console.WriteLine("Invalid arg format: expected a parameter after flag " + cur());
						Environment.Exit(1);
					}
					i++;
					_dict.Add(key, cur());
				}
			}
		}
		public static ArgDictionary Parse(string[] args) {
			return new ArgDictionary(args);
		}

		public void Handle(string argName, string shortName, Action<string> handler, string defaultValue = null) {
			if (_dict.ContainsKey(argName))
				handler(_dict[argName]);
			else if (_dict.ContainsKey(shortName))
				handler(_dict[shortName]);
			else if (!string.IsNullOrEmpty(defaultValue))
				handler(defaultValue);
		}
	}
}
