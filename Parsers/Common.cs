using NESSharp.Core.Resolvers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NESSharp.Core.Parsers {
	public static class Common {
		public class Param {
			
		}
		public interface ILine {}
		public class ByteList : ILine {
			public List<object> List;
			public ByteList(List<object> list) {
				List = list;
			}
		}
		public class WordList : ILine {
			public List<object> List;
			public WordList(List<object> list) {
				List = list;
			}
		}
		public class Label : ILine {
			public string Name;
			//public boo
			public Label(string name) {
				Name = name;
			}
		}
		public class Instruction : ILine {
			public OpCode Op;
			//public List<Param> Params;
			public Instruction(OpCode op) {
				Op = op;
			}
		}
		public class Constant : ILine {
			public string Name;
			public object Value;
			public Constant(string name, object value) {
				Name = name;
				Value = value;
			}
		}
		public class Hi : ILine {
			public object Value;
			public Hi(object value) {
				Value = value;
			}
		}
		public class Lo : ILine {
			public object Value;
			public Lo(object value) {
				Value = value;
			}
		}
		//public class Offset : ILine {
		//	public object Value;
		//}
		public static List<string> Lines(string asm) => asm.Replace("\r\n", "\n").Replace("\r", "\n").Split("\n").Select(RemoveComment).Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x)).ToList();
		public static List<string> Tokens(string line) => line.Split(' ').SelectMany(x => x.Split(',')).Where(x => !string.IsNullOrEmpty(x)).ToList();
		public static string RemoveComment(string line) => line.Split(';').First();
		public static bool IsLabelLine(string line) => line.Last() == ':';

		//private static List<object> ParseParams(IEnumerable<string> tokens) {
		//	var ps = new List<object>();

		//	return ps;
		//}
		
		
		private static readonly List<string> labels = new List<string>();
		private static bool IsExistingLabel(string s) => labels.Contains(s);
		private static bool IsConstant(string s) => AL.Constants.Contains(s);
		private static object ParseToken(object o) {
			if (o is string s) {
				if (IsExistingLabel(s)) return AL.Label[s].Reference();//throw new Exception("Address cannot be printed as a byte--use HIGH or LOW, or write as a word: " + s);
				if (IsConstant(s)) {
					if (AL.Constants[s].GetType() == typeof(ConstU8))
						return (byte)((U8)AL.Constants[s].Value).Value;
							
					throw new Exception("Value is a word, expected a byte: " + o.ToString());
				}
				//return (byte)0;//
				throw new Exception("Unknown value: " + o.ToString());
			} else if (o is U8 u8) {
				return u8.Value;
			}
			throw new NotImplementedException();
		}
		private static object ParseParam(string token) {
			//TODO: convert to U8/U16 based on length
			if (token.StartsWith("%")) {
				//binary
				return Convert.ToInt32(token.Substring(1), 2);
			} else if (token.StartsWith("$")) {
				//hex
				var hexNum = Convert.ToInt32(token.Substring(1), 16);
				if (token.Length <= 3)
					return (U8)hexNum;
				else if (token.Length <= 5)
					return (U16)hexNum;
				else throw new Exception("Only U8s and U16s allowed");
			} else if (int.TryParse(token, out var num)) {
				//decimal
				if (num <= 255)
					return (U8)num;
				else
					return (U16)num;
			}

			//expressions / functions need more work than this, but this is to expedite song file parsing to test ggsound:
			if (token.EndsWith(")")) {
				if (token.StartsWith("low(")) {
					return new Lo(ParseParam(token[4..^1]));
				} else if (token.StartsWith("high(")) {
					return new Hi(ParseParam(token[5..^1]));
			
				}
			}

			//if (token.Contains(">>")) {
			//	var subTokens = token.Split(">>");
			//	var tokenLeft = ParseToken(subTokens[0]);
			//	if (tokenLeft is OpLabel lbl)
			//		return new ShiftRight(new LabelAddress(lbl), (U8)short.Parse(subTokens[1]));
			//} else if (token.Contains("<<")) {
			//	var subTokens = token.Split("<<");
			//	var tokenLeft = ParseToken(subTokens[0]);
			//	if (tokenLeft is OpLabel lbl)
			//		return new ShiftLeft(new LabelAddress(lbl), (U8)short.Parse(subTokens[1]));
			//}

			return token;
		}
		private static object getByteParam(object o) {
			if (o is string s) {
				if (IsExistingLabel(s)) return AL.Label[s].Reference();//throw new Exception("Address cannot be printed as a byte--use HIGH or LOW, or write as a word: " + s);
				if (IsConstant(s)) {
					if (AL.Constants[s].GetType() == typeof(ConstU8))
						return (byte)((U8)AL.Constants[s].Value).Value;
							
					throw new Exception("Value is a word, expected a byte: " + o.ToString());
				}
				//return (byte)0;
				throw new Exception("Unknown value: " + o.ToString());
			} else if (o is Hi hi) {
				var val = getByteParam(hi.Value);
				if (val is Address a) return (byte)a.Hi;
				if (val is LabelRef lbl) return lbl.Lbl.Hi();
				//return val;
				throw new NotImplementedException();
			} else if (o is Lo lo) {
				var val = getByteParam(lo.Value);
				if (val is Address a) return (byte)a.Lo;
				if (val is LabelRef lbl) return lbl.Lbl.Lo();
				//return val;
				throw new NotImplementedException();
			} else if (o is U8 u8) {
				return u8.Value;
			}
			throw new NotImplementedException();
		}
		private static IEnumerable<object> getWordParam(object o) {
			if (o is string s) {
				if (IsExistingLabel(s)) return new List<object>{AL.Label[s].Lo(), AL.Label[s].Hi()};
				if (IsConstant(s)) {
					if (AL.Constants[s].GetType() == typeof(ConstU8))
						return new List<object>{(byte)AL.Constants[s].Value};
					else if (AL.Constants[s].GetType() == typeof(ConstU8))
						return new List<object>{(byte)((U16)AL.Constants[s].Value).Lo, (byte)((U16)AL.Constants[s].Value).Hi};
							
					throw new Exception("Value is a word, expected a byte: " + o.ToString());
				}
				//if (isConstant(s)) return new List<object>{AL.Constants[s].Value};
				throw new Exception("Unknown value: " + o.ToString());
			} else if (o is U8 u8) {
					return new List<object>{(byte)u8, (byte)0};
			} else if (o is U16 u16) {
					return new List<object>{(byte)u16.Lo, (byte)u16.Hi};
			}
			return new List<object>{o};
		}

		//public static object Categorize

		public static void Parse(string asm) {
			var parsed = new List<ILine>(); //string versions of values before figuring out the real data type
			var constants = new Dictionary<string, string>(); //string versions of values before figuring out the real data type
			foreach (var line in Lines(asm)) {
				if (IsLabelLine(line)) {
					parsed.Add(new Label(line[0..^1]));
				} else {
					var tokens = Tokens(line);
					if (tokens[1] == "=") {
						//probably a constant
						//constants.Add(tokens[0], tokens[2]);
						parsed.Add(new Constant(tokens[0], ParseParam(tokens[2])));
					} else if (tokens[0].StartsWith('.')) {
						switch (tokens[0]) {
							case ".db": case ".byte": case ".byt":
								parsed.Add(new ByteList(tokens.Skip(1).Select(ParseParam).ToList()));
								break;
							case ".dw": case ".word": case ".addr":
								parsed.Add(new WordList(tokens.Skip(1).Select(ParseParam).ToList()));
								break;
						}
					} else {
						if (Asm.OC.TryGetValue(tokens[0], out var options)) {
							if (tokens.Count == 1) {
								//if (options.Count > 1) throw new Exception();
								if (options.TryGetValue(Asm.Mode.Implied, out var opRef)) {
									parsed.Add(new Instruction(opRef.Use()));
								} else throw new Exception($"No Implied mode for instruction { tokens[0] }");
							} else if (tokens.Count == 2) {
								if (tokens[1].StartsWith("#")) {
									if (options.TryGetValue(Asm.Mode.Immediate, out var opRef)) {
										//parsed.Add(new Instruction(){ Op = opRef.Use(), Params = new List<Param>(){ tokens[1].Substring(1) } });
									} else throw new Exception($"No Implied mode for instruction { tokens[0] }");
								}
							} else {
								
							}
						}
					}
				}
			}
			
			//var prefix = "testasm_";
			foreach (var p in parsed) {
				if (p is Label lbl) {
					labels.Add(lbl.Name);
						//AL.Constants.Add(new ConstU8(c.Name, u8));
					//else if (c.Value is U16 u16)
						//AL.Constants.Add(new ConstU16(c.Name, u16));
				}
			}
			foreach (var p in parsed) {
				if (p is Constant c) {
					if (c.Value is int i)
						if (i <= 255)
							AL.Constants.Add(new ConstU8(c.Name, (U8)i));
						else
							AL.Constants.Add(new ConstU16(c.Name, (U16)i));
					else if (c.Value is U8 u8)
						AL.Constants.Add(new ConstU8(c.Name, u8));
					else if (c.Value is U16 u16)
						AL.Constants.Add(new ConstU16(c.Name, u16));
				}
			}
			//bool isLabel(string s) => labels.Contains(s);
			//bool isConstant(string s) => AL.Constants.Contains(s);
			//object getParam(string s) =>
			//			isLabel(s) ? AL.Label[s].Reference() :
			//			isConstant(s) ? AL.Constants[s].Value :
			//			Asm.NOP.Value; //throw new NotImplementedException();
			//object[] getParamArray(List<object> list) =>
			//			list.Select(x => x is string ? getParam((string)x) : x).ToArray();

			foreach (var p in parsed) {
				if (p is ByteList bl) {
					AL.Raw(bl.List.Select(getByteParam).ToArray());
				} else if (p is WordList wl) {
					AL.Raw(wl.List.SelectMany(getWordParam).ToArray());
				} else if (p is Label lbl) {
					AL.Use(AL.Label[lbl.Name]);
				} else if (p is Instruction) {
				
				}
			}
		}
	}
}
