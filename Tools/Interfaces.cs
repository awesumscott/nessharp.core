using System;

namespace NESSharp.Core.Tools {

	public interface ITool {}
	public interface IDebugFile : ITool {
		//TODO: method to write a varregistry or (unnamed ram+zp mashup ref) -- this rather than one var at a time,
		//so Scenes can output only their vars, to debug one scene at a time because of address reuse

		void WriteFile(Action<string, string> fileWriteMethod);
	}
	public interface IAssemblerOutput : ITool {
		//TODO: property to register for pings to a method when beginning a bank, or when hitting bank thresholds

		void AppendComment(string comment);
		void AppendOp(Asm.OpRef opref, OpCode opcode);
	}
}
