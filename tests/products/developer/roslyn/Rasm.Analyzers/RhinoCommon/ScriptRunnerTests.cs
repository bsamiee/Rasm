using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.RhinoCommon;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class ScriptRunnerTests : CSharpCodeFixVerifier<ScriptRunnerAnalyzer, EmptyCodeFixProvider> {
    [Fact]
    public Task ReportsRunScriptInCommandWithoutScriptRunnerStyle() => VerifyAnalyzerAsync(
        """
        using Rhino;
        using Rhino.Commands;

        internal sealed class RunScriptWithoutStyle : Command {
            public override string EnglishName => "Plain";
            protected override Result RunCommand(RhinoDoc doc, RunMode mode) => {|#0:RhinoApp.RunScript("_Line", true)|} ? Result.Success : Result.Failure;
        }
        """,
        Diagnostic().WithLocation(0).WithArguments("RunScriptWithoutStyle"));

    [Fact]
    public Task IgnoresRunScriptInCommandWithScriptRunnerStyle() => VerifyAnalyzerAsync(
        """
        using Rhino;
        using Rhino.Commands;

        [CommandStyle(Style.ScriptRunner)]
        internal sealed class RunScriptWithStyle : Command {
            public override string EnglishName => "Scripted";
            protected override Result RunCommand(RhinoDoc doc, RunMode mode) => RhinoApp.RunScript("_Line", true) ? Result.Success : Result.Failure;
        }
        """);
}
