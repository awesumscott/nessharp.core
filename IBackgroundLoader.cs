using System;
using System.Collections.Generic;
using System.Text;

namespace NESSharp.Core {
	public interface IBackgroundLoader {
		void DrawScreenImmediately(IOperand screenId, U8 ntId);
		void QueueDrawSingleRow(IOperand screenId, IOperand y);
		void QueueDrawTwoRows(IOperand screenId, IOperand y);
		void QueueDrawSingleColumn(IOperand screenId, IOperand x);

	}
}
