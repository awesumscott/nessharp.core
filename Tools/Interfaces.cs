using System;
using System.Collections.Generic;

namespace NESSharp.Core.Tools;

public interface ITool {}
public interface IConsoleLogTool : ITool {
	void WriteToConsole();
}
public interface IFileLogTool : ITool {
	void WriteFile(Action<string, string> fileWriteMethod);
}
public interface IDebugFile : ITool {
	//TODO: method to write a varregistry or (unnamed ram+zp mashup ref) -- this rather than one var at a time,
	//so Scenes can output only their vars, to debug one scene at a time because of address reuse
}
public interface IAssemblerOutput : ITool {
	//TODO: property to register for pings to a method when beginning a bank, or when hitting bank thresholds
	void AppendComment(string comment);
	void AppendOp(OpCode opCode);
	void AppendLabel(string name);
	void AppendBytes(IEnumerable<string> bytes);
}
public interface IROMAnalyzer : ITool {

}
public interface IRAMAnalyzer : ITool {
}

public interface INESAsmFormatting {
	string AddressFormat { get; }
	string OperandLow { get; }
	string OperandHigh { get; }
	string ResolveLow { get; }
	string ResolveHigh { get; }
	string ExpressionAdd { get; }
	string ExpressionSubtract { get; }
	string ExpressionLShift { get; }
	string ExpressionRShift { get; }
}
