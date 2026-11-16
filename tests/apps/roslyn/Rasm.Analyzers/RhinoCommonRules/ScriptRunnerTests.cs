using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.Tests.Verifiers;
using Rasm.Rhino.Analyzers;
using Xunit;

namespace Rasm.Analyzers.Tests.RhinoCommonRules;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class ScriptRunnerTests : CSharpCodeFixVerifier<ScriptRunnerAnalyzer, EmptyCodeFixProvider> {
    [Fact]
    public Task RunScriptWithoutStyle() => VerifyAnalyzerAsync(
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
    public Task RunScriptWithStyle() => VerifyAnalyzerAsync(
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
