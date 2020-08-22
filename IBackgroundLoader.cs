using System;
using System.Collections.Generic;
using System.Text;

namespace NESSharp.Core {
	public interface IBackgroundLoader {
		void DrawScreenImmediately(IOperand screenId, U8 ntId);
		void QueueDrawSingleRow(IOperand screenId, IOperand y);
		void QueueDrawTwoRows(IOperand screenId, IOperand y);
		void QueueDrawSingleColumn(IOperand screenId, IOperand x);

		VByte ActiveScreen { get; }
		//Func<IOperand> GetNeighborUp(IndexingRegister reg);
		//Func<IOperand> GetNeighborDown(IndexingRegister reg);
		//Func<IOperand> GetNeighborLeft(IndexingRegister reg);
		//Func<IOperand> GetNeighborRight(IndexingRegister reg);
		IOperand GetNeighborUp(IndexingRegister reg);
		IOperand GetNeighborDown(IndexingRegister reg);
		IOperand GetNeighborLeft(IndexingRegister reg);
		IOperand GetNeighborRight(IndexingRegister reg);
	}
}
