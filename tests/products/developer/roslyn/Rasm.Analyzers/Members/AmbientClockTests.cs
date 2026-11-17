using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.Members;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class AmbientClockTests : CSharpCodeFixVerifier<AmbientClockAnalyzer, EmptyCodeFixProvider> {
    [Fact]
    public Task ReportsUtcNowReadThroughTypeAlias() => VerifyAnalyzerAsync(
        """
        using System;
        using Calendar = System.DateTime;

        internal static class StaticReads {
            internal static DateTime Utc => {|#0:Calendar.UtcNow|};

            internal static int Maximum => int.MaxValue;
        }
        """,
        Diagnostic("RASM3106").WithLocation(0).WithArguments("Calendar.UtcNow", "read `GetUtcNow()` from a `TimeProvider` argument"));

    [Fact]
    public Task StaysSilentOnTimeProviderReadsOtherDateTimeMembersAndNameof() => VerifyAnalyzerAsync(
        """
        using System;

        internal static class Clocks {
            internal static DateTimeOffset Now(TimeProvider time) => time.GetUtcNow();

            internal static DateTime Epoch => DateTime.UnixEpoch;

            internal static string Named => nameof(DateTime.UtcNow);
        }
        """);
}
