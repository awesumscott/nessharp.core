using System;
using static NESSharp.Core.AL;

namespace NESSharp.Core {
	public class DoLoop {
		private readonly Action? _block;
		public DoLoop(Action? block = null) {
			_block = block;
		}
		
		//For reference:
		//(byte)(-128)==(byte)0x80
		//(byte)(127)==(byte)0x7F

		//TODO: implement AL.LastUsedRegister for determining if a CMP is still needed before a branch operation
		public void While(Func<Condition> condition) {
			//An outer context to account for conditions that output some code
			Context.New(() => {
				if (_block != null)
					_block.Invoke();
				var c = condition.Invoke();
				var len = -Context.Length - 2; //-2 is to account for the branch instruction
				if (len >= LOWEST_BRANCH_VAL) {
					Branch(c, (U8)len);
				} else {
					Branch(c, Asm.OC["JMP"][Asm.Mode.Absolute].Length, true);
					CPU6502.JMP(Context.StartLabel); //Use(Asm.JMP.Absolute, Context.StartLabel);
				}
			});
		}
	}
	public static class Loop {
		public static void Infinite(Action? block = null) {
			var lbl = Labels.New();
			Use(lbl);
			if (block != null) {
				Context.New(block);
			}
			GoTo(lbl);
		}
		public static DoLoop Do(Action? block = null) => new DoLoop(block);
		public static void Descend(IndexingRegister reg, Action block) {
			Do(() => {
				var before = reg.State.Hash;
				block.Invoke();
				reg.State.Verify(before);
				if (reg is RegisterX)	X--;
				else					Y--;
			}).While(() => reg is RegisterX ? X.NotEquals(0) : Y.NotEquals(0));
		}
		public static void Descend_Pre(IndexingRegister reg, Action block) {
			Do(() => {
				if (reg is RegisterX)	X--;
				else					Y--;
				var before = reg.State.Hash;
				block.Invoke();
				reg.State.Verify(before);
			}).While(() => reg is RegisterX ? X.NotEquals(0) : Y.NotEquals(0));
		}
		public static void AscendWhile(IndexingRegister reg, Func<Condition> condition, Action block) {
			Do(() => {
				var before = reg.State.Hash;
				block.Invoke();
				reg.State.Verify(before);
				if (reg is RegisterX)	X++;
				else					Y++;
			}).While(condition);
		}
		public static void While(Func<Condition> condition, Action block) {
			var lblStart = Labels.New();
			Use(lblStart);
			var c = condition.Invoke();
			Context.New(() => {
				block.Invoke();
				GoTo(lblStart);
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
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="start">Inclusive</param>
		/// <param name="length">Exclusive</param>
		/// <param name="block"></param>
		public static void RepeatX(U8 start, int length, Action block) {
			//X.Reset();
			X.Set(start);
			var lblStart = Labels.New();
			Use(lblStart);
			Context.New(() => {
				var before = X.State.Hash;
				block.Invoke();
				X.State.Verify(before);
				X++;
				if (Context.StartBranchable) {
					if (length < 256)
						CPU6502.CPX((U8)length);	//TODO: verify (U8)length == (int)length
					Use(Asm.BNE, Context.Start);
				} else {
					//TODO: verify this works!
					if (length < 256)
						CPU6502.CPX((U8)length);	//TODO: verify (U8)length == (int)length
					Use(Asm.BEQ, (U8)3);
					GoTo(lblStart);
				}
			});
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="start">Inclusive</param>
		/// <param name="length">Exclusive</param>
		/// <param name="block"></param>
		public static void Repeat(IndexingRegister reg, U8 start, int length, Action block) {
			//X.Reset();
			X.Set(start);
			var lblStart = Labels.New();
			Use(lblStart);
			Context.New(() => {
				var before = reg.State.Hash;
				block.Invoke();
				reg.State.Verify(before);
				if (reg is RegisterX)	X++;
				else					Y++;
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
		}
		
		public static void ForEach<T>(IndexingRegister index, Array<T> items, Action<T> block) where T : Var, new() {
			if (index is RegisterX) {
				AscendWhile(X.Set(0), () => X.NotEquals((U8)items.Length), () => {
					var before = X.State.Hash;
					block?.Invoke(items[X]);
					X.State.Verify(before);
				});
			} else { 
				AscendWhile(Y.Set(0), () => Y.NotEquals((U8)items.Length), () => {
					var before = Y.State.Hash;
					block?.Invoke(items[Y]);
					Y.State.Verify(before);
				});
			}
		}
		public static void ForEach<T>(IndexingRegister index, StructOfArrays<T> items, Action<T> block) where T : Struct, new() {
			if (index is RegisterX) {
				AscendWhile(X.Set(0), () => X.NotEquals((U8)items.Length), () => {
					var before = X.State.Hash;
					block?.Invoke(items[X]);
					X.State.Verify(before);
				});
			} else { 
				AscendWhile(Y.Set(0), () => Y.NotEquals((U8)items.Length), () => {
					var before = Y.State.Hash;
					block?.Invoke(items[Y]);
					Y.State.Verify(before);
				});
			}
		}
	}
}
