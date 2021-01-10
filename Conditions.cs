using System;
using System.Collections.Generic;
using System.Linq;
using static NESSharp.Core.AL;

namespace NESSharp.Core {
	//public class Condition {

	//}
	public enum Condition {
		EqualsZero,
		NotEqualsZero,
		IsPositive,
		IsNegative,
		IsCarryClear,
		IsCarrySet,
		IsGreaterThan,
		IsLessThan,
		IsGreaterThanOrEqualTo,
		IsLessThanOrEqualTo
	};

	public static class If {
		public static void True(Func<object> condition, Action block) =>		Branching._WriteCondition(condition, block);
		public static void True(Func<Condition> condition, Action block) =>		Branching._WriteCondition(condition, block);
		public static void False(Func<object> condition, Action block) =>		Branching._WriteCondition(condition, block, null, null, true);
		public static void False(Func<Condition> condition, Action block) =>	Branching._WriteCondition(condition, block, null, null, true);

		public static _SingleAdvancedCondition Any(params Func<object>[] conditions) =>		new _SingleAdvancedCondition() { AC = new _AdvancedCondition(_AdvancedCondition.ConditionType.Any, conditions) };
		public static _SingleAdvancedCondition Any(params Func<Condition>[] conditions) =>	new _SingleAdvancedCondition() { AC = new _AdvancedCondition(_AdvancedCondition.ConditionType.Any, conditions) };

		public static _SingleAdvancedCondition All(params Func<object>[] conditions) =>		new _SingleAdvancedCondition() { AC = new _AdvancedCondition(_AdvancedCondition.ConditionType.All, conditions) };
		public static _SingleAdvancedCondition All(params Func<Condition>[] conditions) =>	new _SingleAdvancedCondition() { AC = new _AdvancedCondition(_AdvancedCondition.ConditionType.All, conditions) };

		/// <summary>Multi-condition If block</summary>
		/// <example>
		/// If.Block(c => c
		///		.Option(() => A.Equals(1),	() => A.Set(2))
		/// 	.Option(() => A.Equals(2),	() => A.Set(9))
		/// 	.Default(					() => A.Set(5))
		/// );
		/// </example>
		/// <param name="options"></param>
		public static void Block(Func<_IfBlock, _IfBlock> block) {
			var ifBlock = block.Invoke(new _IfBlock());
			
			var numOptions = ifBlock._options.Count;
			var optionConditions = ifBlock._options.Where(x => x is _IfBlock._Option).Cast<_IfBlock._Option>().ToList();
			var optionDefault = ifBlock._options.Where(x => x is _IfBlock._Default).Cast<_IfBlock._Default>().ToList();
			var hasElse = optionDefault.Any();
			Label? lblEnd = null;
			if (numOptions > 1 || hasElse)
				lblEnd = Labels.New();
			var lastCondition = optionConditions.Last();
			foreach (var oc in optionConditions) {
				var isLast = oc == lastCondition;
				Branching._WriteCondition(oc.Condition, oc.Block, hasElse || !isLast ? lblEnd : null); //don't output "GoTo EndIf" if this is the last condition
			}
			if (numOptions > 1) {
				if (hasElse)
					optionDefault[0].Block?.Invoke();
				if (lblEnd != null) //always true in this block, suppressing nullable complaint
					Use(lblEnd);
			}
			Reset();
		}

		public class _AdvancedCondition {
			public enum ConditionType {Any, All/*, Expression*/};
			public ConditionType Type;
			public object[] Conditions;

			public _AdvancedCondition(ConditionType type, object[] conditions) {
				Type = type;
				Conditions = conditions;
			}
		}

		public class _SingleAdvancedCondition {
			public _AdvancedCondition AC;
			public _IfBlock IfBlock;
			public Action Block;
			public void Then(Action block) {
				Block = block;
				Branching._WriteCondition(AC, block);
				//return IfBlock;
			}
		}
		public class _BlockAdvancedCondition {
			public _AdvancedCondition AC;
			public _IfBlock IfBlock;
			public Action Block;
			public _IfBlock Then(Action block) {
				Block = block;
				IfBlock._options.Add(new _IfBlock._Option(AC, block));
				return IfBlock;
			}
		}

		public class _IfBlock {
			public List<IOption> _options;
		
			public interface IOption {}
			public class _Option : IOption {
				public object Condition;
				public Action Block;
				public Func<Condition>? FallThroughCondition;

				public _Option(object condition, Action block) {
					Condition = condition;
					Block = block;
				}
			}
			public class _Default : IOption {
				public Action Block;

				public _Default(Action block) {
					Block = block;
				}
			}

			public _IfBlock() {
				_options = new List<IOption>();
			}

			public _IfBlock True(Func<object> condition, Action block) {
				_options.Add(new _Option(condition, block));
				return this;
			}

			public _IfBlock True(Func<Condition> condition, Action block) {
				_options.Add(new _Option(condition, block));
				return this;
			}

			public _IfBlock Else(Action block) {
				_options.Add(new _Default(block));
				return this;
			}
			public _BlockAdvancedCondition Any(params Func<object>[] conditions) {
				return new _BlockAdvancedCondition() {
					AC = new _AdvancedCondition(_AdvancedCondition.ConditionType.Any, conditions),
					IfBlock = this
				};
			}
			public _BlockAdvancedCondition Any(params Func<Condition>[] conditions) {
				return new _BlockAdvancedCondition() {
					AC = new _AdvancedCondition(_AdvancedCondition.ConditionType.Any, conditions),
					IfBlock = this
				};
			}

			public _BlockAdvancedCondition All(params Func<object>[] conditions) {
				return new _BlockAdvancedCondition() {
					AC =  new _AdvancedCondition(_AdvancedCondition.ConditionType.All, conditions),
					IfBlock = this
				};
			}
			public _BlockAdvancedCondition All(params Func<Condition>[] conditions) {
				return new _BlockAdvancedCondition() {
					AC =  new _AdvancedCondition(_AdvancedCondition.ConditionType.All, conditions),
					IfBlock = this
				};
			}
		}
	}

	internal static class Branching {
		public static void _WriteIfCondition(Condition condition, Action block, Label? lblEndIf = null, Func<Condition>? fallThroughCondition = null, bool invert = false) {
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
						Branch(condition, len, !invert);
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
			} else if (condition is If._AdvancedCondition ac) {
				switch (ac.Type) {
					case If._AdvancedCondition.ConditionType.Any:
						_WriteAnyCondition(ac.Conditions, block, lblEndIf, lblShortCircuitSuccess);
						break;
					case If._AdvancedCondition.ConditionType.All:
						_WriteAllCondition(ac.Conditions, block, lblEndIf);
						break;
					default:
						throw new Exception("Invalid condition type");
				}
			} else if (condition is AdvancedCondition ac_old) {	//temporary until AL.Any/All are removed
				switch (ac_old.Type) {
					case AdvancedCondition.ConditionType.Any:
						_WriteAnyCondition(ac_old.Conditions, block, lblEndIf, lblShortCircuitSuccess);
						break;
					case AdvancedCondition.ConditionType.All:
						_WriteAllCondition(ac_old.Conditions, block, lblEndIf);
						break;
					default:
						throw new Exception("Invalid condition type");
				}
			} else throw new NotImplementedException();
		}
	}
}
