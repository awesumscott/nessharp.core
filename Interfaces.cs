﻿using System;
using System.Collections.Generic;
using System.Text;

namespace NESSharp.Core {
	/// <summary>
	/// Operands are used as values for ASM opcodes
	/// </summary>
	public interface IOperand {}

	public interface IOperand<T> : IOperand {
		public T Value {get;}
	}

	/// <summary>
	/// Operables are destinations that can receive new IOperand values
	/// </summary>
	public interface IOperable {}
	public interface IOperable<T> : IOperable {
		T Set(IOperand operand);
	}
}
