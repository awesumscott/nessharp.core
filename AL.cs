using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NESSharp.Core {
	public enum CarryState {
		Set,
		Cleared,
		Unknown
	};
	public static class Carry {
		public static CarryState State;
		public static void Clear() => AL.Use(Asm.CLC);
		public static void NewClear() {
			if (State != CarryState.Cleared) {
				AL.Use(Asm.CLC);
				State = CarryState.Cleared;
			}
		}

		public static void Set() => AL.Use(Asm.SEC);
		public static void NewSet() {
			if (State != CarryState.Set) {
				AL.Use(Asm.SEC);
				State = CarryState.Set;
			}
		}
		public static Condition IsClear() => Condition.IsCarryClear;
		public static Condition IsSet() => Condition.IsCarrySet;
	}
	public static class AL {
		public static RegisterA A = new RegisterA();
		public static RegisterX X = new RegisterX();
		public static RegisterY Y = new RegisterY();

		public static Bank									CurrentBank;
		public static List<List<Operation>>					Code;
		public static LabelDictionary						Label				= new LabelDictionary();
		public static SpriteDictionary						Sprites				= new SpriteDictionary();
		public static Dictionary<string, IVarAddressArray>	VarRegistry			= new Dictionary<string, IVarAddressArray>();
		public static ConstantCollection					Constants			= new ConstantCollection();
		public static short									CodeContextIndex;
		
		public static RAM ram =	new RAM(Addr(0), Addr(0x07FF));
		public static RAM zp =	ram.Allocate(Addr(0), Addr(0xFF));
		public static RAM stackRam =	ram.Allocate(Addr(0x0100), Addr(0x01FF)); //eliminate stack page and shadow OAM from possible allocations
		public static RAM OAMRam =		ram.Allocate(Addr(0x0200), Addr(0x02FF)); //eliminate stack page and shadow OAM from possible allocations
		//public static Address[] Temp = zp.Dim(3);
		public static Var8[] Temp = new Var8[] {Var8.New(zp, "Temp0"), Var8.New(zp, "Temp1"), Var8.New(zp, "Temp2")};
		public static Ptr TempPtr0 = new Ptr(null, null, "tempPtr0");

		public static readonly short LOWEST_BRANCH_VAL = -128;
		public static readonly short HIGHEST_BRANCH_VAL = 127;

		static AL() {
			InitCode();
		}
		public static void InitCode() {
			Code = new List<List<Operation>>(); //clear code to prepare for next bank definition
			CodeContextIndex = 0;
			Code.Add(new List<Operation>());
		}

		public static OpLabel LabelFor(Action method) => Label[ROMManager.LabelNameFromMethodInfo(method.GetMethodInfo())];
		public static bool IsResolvable(this object o) => o.GetType().GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IResolvable<>));
		public static bool IsResolvable<T>(this object o) => o.GetType().GetInterfaces().Contains(typeof(IResolvable<T>));

		public static void Use(Operation op) => Code[CodeContextIndex].Add(op);
		public static void Use(Operation op, IResolvable<Address> r) {
			//if (op.Length != 2)
			//	throw new Exception("Invalid parameter length for this opcode");
			op.Param = r;
			Use(op);
		}
		public static void Use(Operation op, IResolvable<U8> r) {
			//if (op.Length != 2)
			//	throw new Exception("Invalid parameter length for this opcode");
			op.Param = r;
			Use(op);
		}
		public static void Use(Operation op, U8 param) {
			if (op.Length != 2)
				throw new Exception("Invalid parameter length for this opcode");
			op.Param = param;
			Use(op);
		}
		public static void Use(Operation op, U16 param) {
			if (op.Length != 3)
				throw new Exception("Invalid parameter length for this opcode");
			op.Param = param;
			Use(op);
		}
		public static void Use(Operation op, OpLabel label) {
			op.Param = label;
			Use(op);
		}
		public static void Raw(params byte[] bytes) => Use(new OpRaw(bytes));
		public static void Raw(params IResolvable<U8>[] u8s) => Use(new OpRaw(u8s));
		public static void Raw(params IResolvable<Address>[] addrs) => Use(new OpRaw(addrs));
		public static void Raw(params object[] objs) => Use(new OpRaw(objs));

		public static void GoTo(OpLabel label) => Use(Asm.JMP.Absolute, label);
		public static void GoTo_Indirect(Ptr p) => Use(Asm.JMP.Indirect, p.Lo);
		public static void GoTo_Indirect(Var16 p) {
			if (p.Lo.Lo.Value == 0xFF) throw new Exception("Var16 used for an indirect JMP has a lo value at the end of a page. Allocate it at a different address for this to work.");
			Use(Asm.JMP.Indirect, p.Lo);
		}

		public static void GoSub(OpLabel label) {
			Use(Asm.JSR, label);
			Reset(); //TODO: possibly get rid of this and only reset based on Clobber/use/preserve attributes
		}
		public static void GoSub(Address addr) {
			Use(Asm.JSR, addr);
			Reset(); //TODO: possibly get rid of this and only reset based on Clobber/use/preserve attributes
		}
		public static void GoSub(Action method) => GoSub(LabelFor(method));
		//public static void Inline(Action method) => method.Invoke();
		public static void Return() => Use(Asm.RTS);



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
		}
		
		//For reference:
		//(byte)(-128)==(byte)0x80
		//(byte)(127)==(byte)0x7F
		
		public interface IOption {}
		public class IfOption : IOption {
			public object Condition;
			public Action Block;
			public Func<Condition> FallThroughCondition;
		}
		public class IfDefault : IOption {
			public Action Block;
		}
		public static Func<Condition> FallThroughIf(Func<Condition> condition) => condition;
		public static IfOption Option(Func<object> condition, Action block, Func<Condition> fallThroughCondition = null)
						=> new IfOption() { Condition = condition, Block = block, FallThroughCondition = fallThroughCondition };
		public static IfOption Option(Func<Condition> condition, Action block, Func<Condition> fallThroughCondition = null)
						=> new IfOption() { Condition = condition, Block = block, FallThroughCondition = fallThroughCondition };

		//TODO: 3rd optional param for Option(): FallThroughIf(condition)--might this be a case to make a Switch block or something similar?

		public static IfDefault Default(Action block) => new IfDefault() { Block = block };
		/// <summary>Multi-condition If block</summary>
		/// <example>
		/// If(
		///		Option(() => A.Equals(1), () => {
		/// 		A.Set(2);
		/// 	}),
		/// 	Option(() => A.Equals(2), () => {
		/// 		A.Set(9);
		/// 	}),
		/// 	Default(() => {
		/// 		A.Set(5);
		/// 	})
		/// );
		/// </example>
		/// <param name="options"></param>
		public static void If(params IOption[] options) {
			var numOptions = options.Length;
			var optionConditions = options.Where(x => x is IfOption).Cast<IfOption>().ToList();
			var optionDefault = options.Where(x => x is IfDefault).Cast<IfDefault>().ToList();
			var hasElse = optionDefault.Any();
			OpLabel lblEnd = null;
			if (numOptions > 1 || hasElse)
				lblEnd = Label.New();
			var lastCondition = optionConditions.Last();
			foreach (var oc in optionConditions) {
				var isLast = oc == lastCondition;
				//var c = oc.Condition.Invoke();
				//_WriteIfCondition(c, oc.Block, hasElse || !isLast ? lblEnd : null, !isLast || hasElse ? oc.FallThroughCondition : null); //don't output "GoTo EndIf" if this is the last condition
				_WriteCondition(oc.Condition, oc.Block, hasElse || !isLast ? lblEnd : null); //don't output "GoTo EndIf" if this is the last condition
			}
			if (numOptions > 1) {
				if (hasElse)
					optionDefault[0].Block.Invoke();
				Use(lblEnd);
			}
		}
		public static void If(Func<object> expression, Action block) {
			_WriteCondition(expression, block);
		}
		public static void If(AdvancedCondition condition, Action block) {
			_WriteCondition(condition, block);
		}
		public static void If(object condition, Action block) {
			_WriteCondition(condition, block);
		}
		public static void If(Func<Condition> condition, Action block) => _WriteCondition(condition, block);
		public static void Branch(Condition condition, U8 len, bool inverted = false) {
			if (inverted) {
				//Opposite of "IF" condition
				switch (condition) {
					case Condition.EqualsZero:		Use(Asm.BNE, len); break;
					case Condition.NotEqualsZero:	Use(Asm.BEQ, len); break;
					case Condition.IsPositive:		Use(Asm.BMI, len); break;
					case Condition.IsNegative:		Use(Asm.BPL, len); break;
					case Condition.IsCarrySet:
					case Condition.IsGreaterThanOrEqualTo:
					case Condition.IsLessThanOrEqualTo:
													Use(Asm.BCC, len); break;
					case Condition.IsCarryClear:
					case Condition.IsGreaterThan:
					case Condition.IsLessThan:
													Use(Asm.BCS, len); break;
					default: throw new NotImplementedException();
				}
			} else {
				//Same as "IF" condition
				switch (condition) {
					case Condition.EqualsZero:		Use(Asm.BEQ, len); break;
					case Condition.NotEqualsZero:	Use(Asm.BNE, len); break;
					case Condition.IsPositive:		Use(Asm.BPL, len); break;
					case Condition.IsNegative:		Use(Asm.BMI, len); break;
					case Condition.IsCarrySet:
					case Condition.IsGreaterThanOrEqualTo:
					case Condition.IsLessThanOrEqualTo:
													Use(Asm.BCS, len); break;
					case Condition.IsCarryClear:
					case Condition.IsGreaterThan:
					case Condition.IsLessThan:
													Use(Asm.BCC, len); break;
					default: throw new NotImplementedException();
				}
			}
		}

		public class AdvancedCondition {
			public enum ConditionType {Any, All, Expression};
			public ConditionType Type;
			public object[] Conditions;
		}
		public static Func<AdvancedCondition> Any(params Func<object>[] conditions) {
			var ac = new AdvancedCondition();
			ac.Type = AdvancedCondition.ConditionType.Any;
			ac.Conditions = conditions;
			return () => ac;
		}
		public static Func<AdvancedCondition> All(params Func<object>[] conditions) {
			var ac = new AdvancedCondition();
			ac.Type = AdvancedCondition.ConditionType.All;
			ac.Conditions = conditions;
			return () => ac;
		}
		private static void _WriteIfCondition(Condition condition, Action block, OpLabel lblEndIf = null, Func<Condition> fallThroughCondition = null, bool invert = false) {
			Context.Push();
			block.Invoke();
			//Skip to "EndIf" if the condition succeeded
			if (lblEndIf != null) {
				if (fallThroughCondition != null)
					Branch(fallThroughCondition.Invoke(), (U8)Asm.JMP.Absolute.Length, invert);
				GoTo(lblEndIf);
			}
			var len = Context.Length;
			if (len <= HIGHEST_BRANCH_VAL) {
				Context.Parent(() => {
					Branch(condition, (U8)len, !invert);
				});
			} else {
				var lblOptionEnd = Label.New();
				Context.Parent(() => {
					Branch(condition, (U8)Asm.JMP.Absolute.Length, invert);
					GoTo(lblOptionEnd);
				});
				Use(lblOptionEnd);
			}
			Context.Pop();
		}

		public static void _WriteAnyCondition(object[] conditions, Action block, OpLabel lblEndIf = null, OpLabel lblShortCircuitSuccess = null) {
			var lblSuccess = lblShortCircuitSuccess ?? Label.New();
			var lblEnd = Label.New();
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
		public static void _WriteAllCondition(object[] conditions, Action block, OpLabel lblEndIf = null, OpLabel lblShortCircuitSuccess = null) {
			var currentCondition = conditions.First();
			_WriteCondition(currentCondition, conditions.Length > 1 ? () => _WriteAllCondition(conditions.Skip(1).ToArray(), block, lblEndIf) : block, conditions.Length == 1 ? lblEndIf : null);
		}
		public static void _WriteCondition(object condition, Action block, OpLabel lblEndIf = null, OpLabel lblShortCircuitSuccess = null, bool invert = false) {
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
}
