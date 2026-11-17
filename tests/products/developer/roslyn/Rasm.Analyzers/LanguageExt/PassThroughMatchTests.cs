using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.LanguageExt;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class PassThroughMatchTests : CSharpCodeFixVerifier<OptionHandlerAnalyzer, EmptyCodeFixProvider> {
    [Fact]
    public Task ReportsMatchWithIdentityHandlerOnOptionAndFin() => VerifyAnalyzerAsync(
        """
        using LanguageExt;

        internal static class PassedThrough {
            internal static int Present(Option<int> value) => {|#0:value.Match(Some: static held => held, None: static () => 0)|};
            internal static int Succeeded(Fin<int> value) => {|#1:value.Match(Succ: static held => held, Fail: static _ => 0)|};
        }
        """,
        Diagnostic("RASM1008").WithLocation(0).WithArguments("Match", "Some", "value.IfNone(static () => 0)"),
        Diagnostic("RASM1008").WithLocation(1).WithArguments("Match", "Succ", "value.IfFail(static _ => 0)"));

    [Fact]
    public Task IgnoresMatchWithNoIdentityHandler() => VerifyAnalyzerAsync(
        """
        using LanguageExt;

        internal static class BothHandled {
            internal static int Present(Option<int> value) => value.Match(Some: static held => held + 1, None: static () => 0);
            internal static int Succeeded(Fin<int> value) => value.Match(Succ: static held => held * 2, Fail: static error => error.Code);
        }
        """);
}
