using System;
using static NESSharp.Core.AL;

namespace NESSharp.Core {
	public class DoLoop {
		private readonly Action<LoopLabels>? _block;
		private readonly LoopLabels _labels = new LoopLabels();
		public DoLoop(Action<LoopLabels>? block = null) {
			_block = block;
		}
		
		//For reference:
		//(byte)(-128)==(byte)0x80
		//(byte)(127)==(byte)0x7F

		//TODO: implement AL.LastUsedRegister for determining if a CMP is still needed before a branch operation
		public void While(Func<Condition> condition) {
			//An outer context to account for conditions that output some code
			Use(_labels.Continue);
			Context.New(() => {
				_block?.Invoke(_labels);
				var c = condition.Invoke();
				var len = -Context.Length - 2; //-2 is to account for the branch instruction
				if (len >= LOWEST_BRANCH_VAL) {
					Branch(c, (U8)len);
				} else {
					Branch(c, Asm.OC["JMP"][Asm.Mode.Absolute].Length, true);
					CPU6502.JMP(Context.StartLabel); //Use(Asm.JMP.Absolute, Context.StartLabel);
				}
			});
			Use(_labels.Break);
		}
	}

	public class LoopLabels {
		public Label Continue = Labels.New();
		public Label Break = Labels.New();
	}
	//public delegate void LoopAction(LoopLabels? labels = null);

	public static class Loop {
		public static void Infinite(Action<LoopLabels>? block = null) {
			var labels = new LoopLabels();
			//var lbl = Labels.New();
			Use(labels.Continue);
			if (block != null) {
				Context.New(() => block(labels));
			}
			GoTo(labels.Continue);
			Use(labels.Break);
		}
		public static DoLoop Do(Action<LoopLabels>? block = null) => new DoLoop(block);
		public static void Descend(IndexingRegister reg, Action<LoopLabels> block) {
			var labels = new LoopLabels();
			Do(_ => {
				var before = reg.State.Hash;
				block?.Invoke(labels);
				reg.State.Verify(before);
				Use(labels.Continue);
				reg--;
			}).While(() => reg is RegisterX ? X.NotEquals(0) : Y.NotEquals(0));
			Use(labels.Break);
		}
		public static void Descend_Pre(IndexingRegister reg, Action<LoopLabels> block) {
			var labels = new LoopLabels();
			Do(_ => {
				Use(labels.Continue);
				reg--;
				var before = reg.State.Hash;
				block.Invoke(labels);
				reg.State.Verify(before);
			}).While(() => reg is RegisterX ? X.NotEquals(0) : Y.NotEquals(0));
			Use(labels.Break);
		}
		public static void AscendWhile(IndexingRegister reg, Func<Condition> condition, Action<LoopLabels> block) {
			var labels = new LoopLabels();
			Do(_ => {
				var before = reg.State.Hash;
				block.Invoke(labels);
				reg.State.Verify(before);
				Use(labels.Continue);
				reg++;
			}).While(condition);
			Use(labels.Break);
		}
		public static void While(Func<Condition> condition, Action<LoopLabels> block) {
			var labels = new LoopLabels();
			//var lblStart = Labels.New();
			Use(labels.Continue);
			var c = condition.Invoke();
			Context.New(() => {
				block(labels);
				GoTo(labels.Continue);
				var len = Context.Length;
				if (len <= HIGHEST_BRANCH_VAL) {
					Context.Parent(() => {
						Branch(c, (U8)len, true);
					});
				} else {
					var lblEnd = Labels.New();
					var lblOptionEnd = Labels.New();
					Context.Parent(() => {
						Branch(c, Asm.OC["JMP"][Asm.Mode.Absolute].Length);
						GoTo(lblEnd);
					});
					Use(lblEnd);
				}
			});
			Use(labels.Break);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="start">Inclusive</param>
		/// <param name="length">Exclusive</param>
		/// <param name="block"></param>
		public static void Repeat(IndexingRegister reg, int length, Action<LoopLabels> block) {
			var labels = new LoopLabels();
			//X.Reset();
			var lblStart = Labels.New();
			Use(lblStart);
			Context.New(() => {
				var before = reg.State.Hash;
				block.Invoke(labels);
				reg.State.Verify(before);
				Use(labels.Continue);
				reg++;
				if (Context.StartBranchable) {
					if (length < 256) {
						if (reg is RegisterX)	CPU6502.CPX((U8)length);	//TODO: verify (U8)length == (int)length
						else					CPU6502.CPY((U8)length);	//TODO: verify (U8)length == (int)length
					}
					Use(Asm.BNE, Context.Start);
				} else {
					//TODO: verify this works!
					if (length < 256) {
						if (reg is RegisterX)	CPU6502.CPX((U8)length);	//TODO: verify (U8)length == (int)length
						else					CPU6502.CPY((U8)length);	//TODO: verify (U8)length == (int)length
					}
					Use(Asm.BEQ, (U8)3);
					GoTo(lblStart);
				}
			});
			Use(labels.Break);
		}
		
		public static void ForEach<T>(IndexingRegister index, Array<T> items, Action<T> block) where T : Var, new() {
			if (index is RegisterX) {
				AscendWhile(X.Set(0), () => X.NotEquals(items.Length == 256 ? 0 : items.Length), _ => {
					var before = X.State.Hash;
					block?.Invoke(items[X]);
					X.State.Verify(before);
				});
			} else { 
				AscendWhile(Y.Set(0), () => Y.NotEquals(items.Length == 256 ? 0 : items.Length), _ => {
					var before = Y.State.Hash;
					block?.Invoke(items[Y]);
					Y.State.Verify(before);
				});
			}
		}
		public static void ForEach<T>(IndexingRegister index, StructOfArrays<T> items, Action<T> block) where T : Struct, new() {
			if (index is RegisterX) {
				AscendWhile(X.Set(0), () => X.NotEquals(items.Length == 256 ? 0 : items.Length), _ => {
					var before = X.State.Hash;
					block?.Invoke(items[X]);
					X.State.Verify(before);
				});
			} else { 
				AscendWhile(Y.Set(0), () => Y.NotEquals(items.Length == 256 ? 0 : items.Length), _ => {
					var before = Y.State.Hash;
					block?.Invoke(items[Y]);
					Y.State.Verify(before);
				});
			}
		}
	}
}
