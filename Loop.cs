using System;
using static NESSharp.Core.AL;

namespace NESSharp.Core {
	public class DoLoop {
		private Action? _block;
		public DoLoop(Action? block = null) {
			_block = block;
		}
		
		//For reference:
		//(byte)(-128)==(byte)0x80
		//(byte)(127)==(byte)0x7F

		//TODO: implement AL.LastUsedRegister for determining if a CMP is still needed before a branch operation
		public void While(Func<Condition> condition) {
			//Context.Push(); //An outer context to account for conditions that output some code
			Context.Push();
			if (_block != null)
				_block.Invoke();
			var c = condition.Invoke();
			var len = -Context.Length - 2; //-2 is to account for the branch instruction
			if (len >= LOWEST_BRANCH_VAL) {
				Branch(c, (U8)len);
			} else {
				Branch(c, (U8)Asm.JMP.Absolute.Length, true);
				Use(Asm.JMP.Absolute, Context.StartLabel);
			}
			Context.Pop();
		}
	}
	public static class Loop {
		public static void Infinite(Action block = null) {
			var lbl = Label.New();
			Use(lbl);
			if (block != null) {
				Context.Push();
				block.Invoke();
				Context.Pop();
			}
			GoTo(lbl);
		}
		public static DoLoop Do(Action? block = null) => new DoLoop(block);
		public static void For(Action initialize, Action condition, Action each, Action block) => throw new NotImplementedException();
		public static void Descend(RegisterX reg, Action block) {
			Do(() => {
				block.Invoke();
				reg--;
			}).While(() => reg.NotEquals(0));
		}

		public static void Descend(RegisterY reg, Action block) {
			Do(() => {
				block.Invoke();
				reg--;
			}).While(() => reg.NotEquals(0));
		}
		public static void Descend_Pre(RegisterX reg, Action block) {
			Do(() => {
				reg--;
				block.Invoke();
			}).While(() => reg.NotEquals(0));
		}
		public static void AscendWhile(RegisterX reg, Func<Condition> condition, Action block) {
			Do(() => {
				block.Invoke();
				reg++;
			}).While(condition);
		}

		public static void AscendWhile(RegisterY reg, Func<Condition> condition, Action block) {
			Do(() => {
				block.Invoke();
				reg++;
			}).While(condition);
		}
		public static void While(Func<Condition> condition, Action block) {
			var lblStart = Label.New();
			Use(lblStart);
			var c = condition.Invoke();
			Context.Push();
			block.Invoke();
			GoTo(lblStart);
			var len = Context.Length;
			if (len <= HIGHEST_BRANCH_VAL) {
				Context.Parent(() => {
					Branch(c, (U8)len, true);
				});
			} else {
				var lblEnd = Label.New();
				var lblOptionEnd = Label.New();
				Context.Parent(() => {
					Branch(c, (U8)Asm.JMP.Absolute.Length);
					GoTo(lblEnd);
				});
				Use(lblEnd);
			}
			Context.Pop();
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="start">Inclusive</param>
		/// <param name="length">Exclusive</param>
		/// <param name="block"></param>
		public static void RepeatX(U8 start, U8 length, Action block) {
			//X.Reset();
			X.Set(start);
			var lblStart = Label.New();
			Use(lblStart);
			Context.New(() => {
				block.Invoke();
				X++;
				if (Context.StartBranchable) {
					if (length != 255)
						Use(Asm.CPX.Immediate, length); //(U8)(max + 1)
					Use(Asm.BNE, Context.Start);
				} else {
					//TODO: verify this works!
					if (length != 255)
						Use(Asm.CPX.Immediate, length); //(U8)(max + 1)
					Use(Asm.BEQ, (U8)3);
					Use(Asm.JMP.Absolute, lblStart);
				}
			});
		}
	}
}
