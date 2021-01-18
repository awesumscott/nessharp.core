using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NESSharp.Core {
	public abstract class Module {
		//TODO: turn these into properties to check if initialized
		public U8 Bank;
		public RAM ModuleMem;
		//TODO: possibly remove these
		public RAMRange Ram;
		public RAMRange Zp;

		public void Init(U8 bank, RAM remainder) {
			Bank = bank;
			ModuleMem = remainder;
			Ram = remainder?.Ram ?? NES.ram;
			Zp = remainder?.Zp ?? NES.zp;
		}
	}
	public enum CarryState {
		Set,
		Cleared,
		Unknown
	};
	public static class Carry {
		public static CarryState State;
		public static void Clear() =>			CPU6502.CLC();
		public static void Set() =>				CPU6502.SEC();
		public static Condition IsClear() =>	Condition.IsCarryClear;
		public static Condition IsSet() =>		Condition.IsCarrySet;
		public static void Reset() =>			State = CarryState.Unknown;
	}

	public static class AL {
		//TODO: transition "using static NESSharp.Core.AL" statements to "using static NESSharp.Core.CPU6502" everywhere to get these out of here
		public static readonly RegisterA A		= CPU6502.A;
		public static RegisterX X				= CPU6502.X;
		public static RegisterY Y				= CPU6502.Y;
		public static readonly FlagStates Flags	= CPU6502.Flags;

		public static List<List<IOperation>>				Code;
		public static short									CodeContextIndex;
		public static readonly LabelDictionary				Labels				= new();
		public static readonly Dictionary<string, Var>		VarRegistry			= new();
		public static readonly ConstantCollection			Constants			= new();
		private static readonly Dictionary<Type, Module>	_Modules			= new();
		public static VByte[] Temp;
		public static Ptr TempPtr0, TempPtr1;

		public static readonly short LOWEST_BRANCH_VAL = -128;
		public static readonly short HIGHEST_BRANCH_VAL = 127;

		static AL() {
			NES.Init(); //TODO: get rid of this when it's no longer static
			Temp		= new VByte[] {VByte.New(NES.zp, "Temp0"), VByte.New(NES.zp, "Temp1"), VByte.New(NES.zp, "Temp2")};
			TempPtr0	= Ptr.New(NES.zp, "tempPtr0");
			TempPtr1	= Ptr.New(NES.zp, "tempPtr1");
			InitCode();
		}
		public static void InitCode() {
			Code = new List<List<IOperation>>(); //clear code to prepare for next bank definition
			CodeContextIndex = 0;
			Code.Add(new List<IOperation>());
		}
		public static T Module<T>(RAM? memory = null) where T : Module {
			var instance = (T?)_Modules.Where(x => x.Key == typeof(T)).Select(x => x.Value).FirstOrDefault();
			if (instance == null) {
				instance = Activator.CreateInstance<T>();
				_Modules.Add(typeof(T), instance);
			}
			instance.Init(ROMManager.CurrentBankId, memory ?? NES.Mem);
			return instance;
		}

		public static Label LabelFor(Action method) => Labels[ROMManager.LabelNameFromMethodInfo(method.GetMethodInfo())];
		public static bool IsResolvable(this object o) => o.GetType().GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IResolvable<>));
		public static bool IsResolvable<T>(this object o) => o.GetType().GetInterfaces().Contains(typeof(IResolvable<T>));

		public static void Use(IOperation op) => Code[CodeContextIndex].Add(op);
		public static void Use(OpCode op, IResolvable<Address> r) {
			op.Param = r;
			Use(op);
		}
		public static void Use(OpCode op, IResolvable<U8> r) {
			op.Param = r;
			Use(op);
		}
		public static void Use(OpCode op, IOperand<Address> param) {
			op.Param = param;
			Use(op);
		}
		public static void Use(OpCode op, IOperand<U8> param) {
			op.Param = param;
			Use(op);
		}
		public static void Raw(U16 u16) => Use(new OpRaw(u16.Lo, u16.Hi));
		public static void Raw(params byte[] bytes) => Use(new OpRaw(bytes));
		//public static void Raw(params U8[] u8s) => Use(new OpRaw(u8s.Cast<byte>().ToArray()));
		public static void Raw(params IResolvable<U8>[] u8s) => Use(new OpRaw(u8s));
		public static void Raw(params IResolvable<Address>[] addrs) => Use(new OpRaw(addrs));
		public static void Raw(params object[] objs) => Use(new OpRaw(objs));

		public static void GoTo(Label label) => CPU6502.JMP(label); //Use(Asm.JMP.Absolute, label);
		public static void GoTo_Indirect(Ptr p) => Use(Asm.OC["JMP"][Asm.Mode.IndirectAbsolute].Use(), (IOperand<Address>)p.Lo);//CPU6502.JMP(p.Lo)); //Use(Asm.JMP.Indirect, p.Lo);
		//public static void GoTo_Indirect(Ptr p) => Use(Asm.OC["JMP"][Asm.Mode.IndirectAbsolute].Use(), (IResolvable<Address>)p.Lo);
		public static void GoTo_Indirect(VWord vn) {
			if (vn.Address[0].Lo == 0xFF) throw new Exception("Var16 used for an indirect JMP has a lo value at the end of a page. Allocate it at a different address for this to work.");
			//CPU6502.JMP(vn.Lo);
			//Use(Asm.OpRef.Use("JMP", Asm.Mode.IndirectAbsolute), vn.Lo);
			Use(Asm.OC["JMP"][Asm.Mode.IndirectAbsolute].Use(), (IOperand<Address>)vn.Lo);
			//Use(Asm.OC["JMP"][Asm.Mode.IndirectAbsolute].Use(), (IResolvable<Address>)vn.Lo);
		}

		public static void GoSub(Label label) => CPU6502.JSR(label);
		public static void GoSub(Address addr) => CPU6502.JSR(addr);
		public static void GoSub(Action method) => GoSub(LabelFor(method));
		public static void Return() => CPU6502.RTS();

		public static int Length(Action a) {
			Context.Push();
			a.Invoke();
			var len = Context.Length;
			//if (len > 255) throw new Exception("Length is greater than 255, maybe split it up for now until there are RAMCopy helpers");
			Context.Delete();
			return len;
		}

		public static void Comment(string name) => Use(new OpComment(name));

		public static Address Addr(U16 address) => new Address(address);

		public static void Reset() {
			A.Reset();
			X.Reset();
			Y.Reset();
			Flags.Reset();
		}

		//For reference:
		//(byte)(-128)==(byte)0x80
		//(byte)(127)==(byte)0x7F

		public static void Branch(Condition condition, U8 len, bool inverted = false) {
			if (inverted) {
				//Opposite of "IF" condition
				switch (condition) {
					case Condition.EqualsZero:			CPU6502.BNE(len); break;
					case Condition.NotEqualsZero:		CPU6502.BEQ(len); break;
					case Condition.IsPositive:			CPU6502.BMI(len); break;
					case Condition.IsNegative:			CPU6502.BPL(len); break;
					case Condition.IsCarrySet:
					case Condition.IsGreaterThanOrEqualTo:
					case Condition.IsLessThanOrEqualTo:	CPU6502.BCC(len); break;
					case Condition.IsCarryClear:
					case Condition.IsGreaterThan:
					case Condition.IsLessThan:			CPU6502.BCS(len); break;
					default: throw new NotImplementedException();
				}
			} else {
				//Same as "IF" condition
				switch (condition) {
					case Condition.EqualsZero:			CPU6502.BEQ(len); break;
					case Condition.NotEqualsZero:		CPU6502.BNE(len); break;
					case Condition.IsPositive:			CPU6502.BPL(len); break;
					case Condition.IsNegative:			CPU6502.BMI(len); break;
					case Condition.IsCarrySet:
					case Condition.IsGreaterThanOrEqualTo:
					case Condition.IsLessThanOrEqualTo:	CPU6502.BCS(len); break;
					case Condition.IsCarryClear:
					case Condition.IsGreaterThan:
					case Condition.IsLessThan:			CPU6502.BCC(len); break;
					default: throw new NotImplementedException();
				}
			}
		}

		public class AdvancedCondition {
			public enum ConditionType {Any, All, Expression};
			public ConditionType Type;
			public object[] Conditions;

			public AdvancedCondition(ConditionType type, object[] conditions) {
				Type = type;
				Conditions = conditions;
			}
		}
		public static Func<AdvancedCondition> Any(params Func<object>[] conditions) => () => new AdvancedCondition(AdvancedCondition.ConditionType.Any, conditions);
		public static Func<AdvancedCondition> All(params Func<object>[] conditions) => () => new AdvancedCondition(AdvancedCondition.ConditionType.All, conditions);
		private static void _WriteIfCondition(Condition condition, Action block, Label? lblEndIf = null, Func<Condition>? fallThroughCondition = null, bool invert = false) {
			Context.New(() => {
				block.Invoke();
				//Skip to "EndIf" if the condition succeeded
				if (lblEndIf != null) {
					if (fallThroughCondition != null)
						Branch(fallThroughCondition.Invoke(), Asm.OC["JMP"][Asm.Mode.Absolute].Length, invert);
					GoTo(lblEndIf);
				}
				var len = Context.Length;
				if (len <= HIGHEST_BRANCH_VAL) {
					Context.Parent(() => {
						Branch(condition, (U8)len, !invert);
					});
				} else {
					var lblOptionEnd = Labels.New();
					Context.Parent(() => {
						Branch(condition, Asm.OC["JMP"][Asm.Mode.Absolute].Length, invert);
						GoTo(lblOptionEnd);
					});
					Use(lblOptionEnd);
				}
			});
		}

		public static void _WriteAnyCondition(object[] conditions, Action block, Label? lblEndIf = null, Label? lblShortCircuitSuccess = null) {
			var lblSuccess = lblShortCircuitSuccess ?? Labels.New();
			var lblEnd = Labels.New();
			void successBlock() => GoTo(lblSuccess);
			var last = conditions.Last();
			foreach (var condition in conditions) {
				_WriteCondition(condition, successBlock, null, lblSuccess/*, true*/);//, condition == last ? lblEndIf : null);
			}
			//If this is a nested Any, the GoTo(lblSuccess) used above will skip out to the parent Any's success label and this stuff won't be needed
			//TODO: determine if this lblShortCircuitSuccess business is even needed. Who the hell needs to nest Any() conditions?! they could be merged and have the same effect. Maybe if helpers return an Any(), and that's used within another?
			if (lblShortCircuitSuccess == null) {
				GoTo(lblEnd);
				Use(lblSuccess);
				block.Invoke();
			}
			
			if (lblEndIf != null) {
				//if (fallThroughCondition != null)
				//	Branch(fallThroughCondition.Invoke(), (U8)Asm.JMP.Absolute.Length);
				Comment("right before goto endif");
				GoTo(lblEndIf);
			}
			Comment("after goto endif and before writeany's lblend");
			Use(lblEnd);
		}
		public static void _WriteAllCondition(object[] conditions, Action block, Label? lblEndIf = null) {
			var currentCondition = conditions.First();
			_WriteCondition(currentCondition, conditions.Length > 1 ? () => _WriteAllCondition(conditions.Skip(1).ToArray(), block, lblEndIf) : block, conditions.Length == 1 ? lblEndIf : null);
		}
		public static void _WriteCondition(object condition, Action block, Label? lblEndIf = null, Label? lblShortCircuitSuccess = null, bool invert = false) {
			if (condition is Func<Condition> fc)
				_WriteIfCondition(fc.Invoke(), block, lblEndIf, null, invert);
			else if (condition is Func<object> fo)
				_WriteCondition(fo.Invoke(), block, lblEndIf, lblShortCircuitSuccess, invert);
			else if (condition is RegisterA a)
				_WriteIfCondition(Condition.NotEqualsZero, block, lblEndIf, null, invert);
			else if (condition is Condition c)
				_WriteIfCondition(c, block, lblEndIf, null, invert);
			else if (condition is Func<RegisterA> fa) {
				//This translates an expression into an exp!=0 condition
				fa.Invoke();
				_WriteIfCondition(Condition.NotEqualsZero, block, lblEndIf, null, invert);
			} else if (condition is AdvancedCondition ac) {
				switch (ac.Type) {
					case AdvancedCondition.ConditionType.Any:
						_WriteAnyCondition(ac.Conditions, block, lblEndIf, lblShortCircuitSuccess);
						break;
					case AdvancedCondition.ConditionType.All:
						_WriteAllCondition(ac.Conditions, block, lblEndIf);
						break;
					default:
						throw new Exception("Invalid condition type");
				}
			} else throw new NotImplementedException();
		}

		public static void Duplicate(int times, Action block) {
			for (var i = 0; i < times; i++)
				block.Invoke();
		}
	}

	public static class LinqExtensions {
		public static void ForEach<T>(this IEnumerable<T> source, Action<T> action) {
            foreach (var element in source)
                action(element);
        }
	}
}
