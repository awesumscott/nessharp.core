﻿using System;

namespace NESSharp.Core;

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
		Context.Write(_labels.ContinueLabel);
		Context.New(() => {
			_block?.Invoke(_labels);
			var c = condition.Invoke();
			var len = -Context.Length - 2; //-2 is to account for the branch instruction
			if (len >= AL.LOWEST_BRANCH_VAL) {
				AL.Branch(c, len);
			} else {
				AL.Branch(c, CPU6502.Asm.OC["JMP"][CPU6502.Asm.Mode.Absolute].Length, true);
				CPU6502.JMP(Context.StartLabel);
			}
		});
		Context.Write(_labels.BreakLabel);
	}
}

public class LoopLabels {
	public Label ContinueLabel = AL.Labels.New();
	public Label BreakLabel = AL.Labels.New();
	public Action Continue => () => AL.GoTo(ContinueLabel);
	public Action Break => () => AL.GoTo(BreakLabel);
}
//public delegate void LoopAction(LoopLabels? labels = null);

public static class Loop {
	/*
		Loop.{Ascend|Descend}(reg).{Until|While}(condition).Do(block);
		Loop.{Ascend|Descend}(reg).Do(block).{Until|While}(condition);
		Loop.Do(block).{Ascend|Descend}(reg).{Until|While}(condition);
		Loop.While(condition).Do(block);
		Loop.Until(condition).Do(block);
		Loop.Do(block).While(condition);
		Loop.Do(block).While(Until);
	*/
	//public interface ILoop {}
	//public interface ILoop_Block : ILoop {}
	//public interface ILoop_Iterate : ILoop {
	//	ILoop Ascend(IndexingRegister reg);
	//	ILoop Descend(IndexingRegister reg);
	//}
	//public interface ILoop_Condition : ILoop {
	//	ILoop While(Func<Condition> condition);
	//	ILoop Until(Func<Condition> condition);
	//}
	//public class LoopBlockIteratingCondition : ILoop {
		
	//}
	//public class LoopBlockIterating : ILoop_Condition {
	//	public ILoop Ascend(IndexingRegister reg) {
		
	//	}
	//	public ILoop Descend(IndexingRegister reg) {
		
	//	}
	//}

	public class LoopBlock {
		public enum LoopLocation {
			Post,
			Pre
		}
		private Action<IndexingRegister, LoopLabels>? Block = null;
		private IndexingRegister? Reg = null;

		public LoopBlock(IndexingRegister? reg) => Reg = reg;

		private bool? Ascending = null;
		private bool PostCondition = true;
		private bool Inverted = false;
		private LoopLocation StepSpot = LoopLocation.Post;
		private LoopLocation ConditionSpot = LoopLocation.Post;
		private Func<Condition>? Condition = null;

		//TODO: for each method, test if loop executed already, exception of true

		public LoopBlock Ascend(LoopLocation step = LoopLocation.Post) {
			Ascending = true;
			StepSpot = step;
			return this;
		}
		public LoopBlock Descend(LoopLocation step = LoopLocation.Post) {
			StepSpot = Block == null ? LoopLocation.Pre : LoopLocation.Post;
			Ascending = false;
			StepSpot = step;
			return this;
		}
		public LoopBlock Do(Action<IndexingRegister, LoopLabels>? block = null) {
			Block = block;
			//TODO: if Condition is set, execute loop
			return this;
		}
		public LoopBlock While(Func<Condition> condition) {
			Condition = condition;
			Inverted = false;
			ConditionSpot = Block == null ? LoopLocation.Pre : LoopLocation.Post;
			return this;
		}
		public LoopBlock Until(Func<Condition> condition) {
			Condition = condition;
			Inverted = true;
			ConditionSpot = Block == null ? LoopLocation.Pre : LoopLocation.Post;
			return this;
		}
		public void End() {
			//Write the loop code

			//var labels = new LoopLabels();
			//Do_old(_ => {
			//	var before = Reg.State.Hash;
			//	Block?.Invoke(Reg, labels);
			//	Reg.State.Verify(before);
			//	Context.Write(labels.ContinueLabel);
			//	Reg.Dec();
			//}).While(() => Reg.NotEquals(0));
			//Context.Write(labels.BreakLabel);
		}
	}
	public static LoopBlock Using(IndexingRegister reg) => new LoopBlock(reg);
	//public static LoopBlock Ascend(IndexingRegister reg) => new LoopBlock().Ascend(reg);
	//public static LoopBlock Descend(IndexingRegister reg) => new LoopBlock().Descend(reg);
	//public static LoopBlock Do(Action<LoopLabels>? block = null) => new LoopBlock().Do(block);
	//public static LoopBlock While(Func<Condition> condition) => new LoopBlock().While(condition);
	//public static LoopBlock Until(Func<Condition> condition) => new LoopBlock().Until(condition);









	public static void Infinite(Action<LoopLabels>? block = null) {
		var labels = new LoopLabels();
		//var lbl = Labels.New();
		Context.Write(labels.ContinueLabel);
		if (block != null) {
			Context.New(() => block(labels));
		}
		AL.GoTo(labels.ContinueLabel);
		Context.Write(labels.BreakLabel);
	}
	public static DoLoop Do_old(Action<LoopLabels>? block = null) => new DoLoop(block);
	public static void Descend_PostCondition_PostDec(IndexingRegister reg, Action<LoopLabels> block) {
		var labels = new LoopLabels();
		Do_old(_ => {
			var before = reg.State.Hash;
			block?.Invoke(labels);
			reg.State.Verify(before);
			Context.Write(labels.ContinueLabel);
			reg.Dec();
		}).While(() => reg.NotEquals(0));
		Context.Write(labels.BreakLabel);
	}
	public static void Descend_PostCondition_PreDec(IndexingRegister reg, Action<LoopLabels> block) {
		var labels = new LoopLabels();
		Do_old(_ => {
			Context.Write(labels.ContinueLabel);
			reg.Dec();
			var before = reg.State.Hash;
			block.Invoke(labels);
			reg.State.Verify(before);
		}).While(() => reg.NotEquals(0));
		Context.Write(labels.BreakLabel);
	}
	public static void While_PostCondition_PostInc(IndexingRegister reg, Func<Condition> condition, Action<LoopLabels> block) {
		var labels = new LoopLabels();
		Do_old(_ => {
			var before = reg.State.Hash;
			block.Invoke(labels);
			reg.State.Verify(before);
			Context.Write(labels.ContinueLabel);
			reg.Inc();
		}).While(condition);
		Context.Write(labels.BreakLabel);
	}
	public static void While_PreCondition_NoInc(Func<Condition> condition, Action<LoopLabels> block) {
		var labels = new LoopLabels();
		Context.Write(labels.ContinueLabel);
		var c = condition.Invoke();
		Context.New(() => {
			block.Invoke(labels);
			AL.GoTo(labels.ContinueLabel);
			var len = Context.Length;
			if (len <= AL.HIGHEST_BRANCH_VAL) {
				Context.Parent(() => {
					AL.Branch(c, len, true);
				});
			} else {
				var lblEnd = AL.Labels.New();
				var lblOptionEnd = AL.Labels.New();
				Context.Parent(() => {
					AL.Branch(c, CPU6502.Asm.OC["JMP"][CPU6502.Asm.Mode.Absolute].Length);
					AL.GoTo(lblEnd);
				});
				Context.Write(lblEnd);
			}
		});
		Context.Write(labels.BreakLabel);
	}
	/// <summary>
	/// 
	/// </summary>
	/// <param name="start">Inclusive</param>
	/// <param name="length">Exclusive</param>
	/// <param name="block"></param>
	public static void Repeat_PostInc(IndexingRegister reg, int length, Action<LoopLabels> block) {
		var labels = new LoopLabels();
		//X.Reset();
		var lblStart = AL.Labels.New();
		Context.Write(lblStart);
		Context.New((Action)(() => {
			var before = reg.State.Hash;
			block.Invoke(labels);
			reg.State.Verify(before);
			Context.Write(labels.ContinueLabel);
			reg.Inc();
			if (Context.StartBranchable) {
				if (length < 256) {
					if (reg is RegisterX) CPU6502.CPX((U8)length);
					else CPU6502.CPY((U8)length);
				}
				CPU6502.BNE(Context.Start);
			} else {
				//TODO: verify this works!
				if (length < 256) {
					if (reg is RegisterX) CPU6502.CPX((U8)length);
					else CPU6502.CPY((U8)length);
				}
				CPU6502.BEQ(3);
				AL.GoTo(lblStart);
			}
		}));
		Context.Write(labels.BreakLabel);
	}
	
	public static void ForEach<T>(IndexingRegister index, Array<T> items, Action<T, LoopLabels> block) where T : Var, new() {
		While_PostCondition_PostInc(index.Set(0), () => index.NotEquals(items.Length == 256 ? 0 : items.Length), lbls => {
			var before = index.State.Hash;
			block?.Invoke(items[index], lbls);
			index.State.Verify(before);
		});
	}

	public static void ForEach<T>(IndexingRegister index, StructOfArrays<T> items, Action<T, LoopLabels> block) where T : Struct, new() {
		While_PostCondition_PostInc(index.Set(0), () => index.NotEquals(items.Length == 256 ? 0 : items.Length), lbls => {
			var before = index.State.Hash;
			block?.Invoke(items[index], lbls);
			index.State.Verify(before);
		});
	}
}
