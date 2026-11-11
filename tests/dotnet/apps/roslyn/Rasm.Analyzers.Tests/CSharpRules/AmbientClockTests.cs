using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.CSharpRules;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class AmbientClockTests : CSharpCodeFixVerifier<AmbientClockAnalyzer, EmptyCodeFixProvider> {
    [Fact]
    public Task AliasedUtcNow() => VerifyAnalyzerAsync(
        """
        using System;
        using Calendar = System.DateTime;

        internal static class StaticReads {
            internal static DateTime Utc => {|#0:Calendar.UtcNow|};

            internal static int Maximum => int.MaxValue;
        }
        """,
        Diagnostic("RASM3106").WithLocation(0).WithArguments("Calendar.UtcNow", "read `GetUtcNow()` from a `TimeProvider` argument"));
}
