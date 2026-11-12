using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.CSharpRules;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class AmbientClockTests : CSharpCodeFixVerifier<AmbientClockAnalyzer, EmptyCodeFixProvider> {

    // --- [RASM3106]

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

    // --- [RASM3107]

    [Fact(DisplayName = "Library members naming TimeProvider.System are reported; exemplar SergeyTeplyakov/ErrorProne.NET@b57da22 src/ErrorProne.NET.CoreAnalyzers.Tests/AsyncAnalyzers/DoNotUseThreadSleepAnalyzerTests.cs WarnOnThreadSleepInAsyncMethod")]
    public Task SystemClockInLibrary() => VerifyAnalyzerAsync(
        """
        using System;
        using Clock = System.TimeProvider;

        internal static class SystemClockInLibrary {
            internal static DateTimeOffset Read() => {|#0:TimeProvider.System|}.GetUtcNow();

            internal static TimeProvider Defaulted(TimeProvider? time) => time ?? {|#1:TimeProvider.System|};

            internal static TimeProvider Aliased => {|#2:Clock.System|};

            internal static Func<long> Grouped => {|#3:TimeProvider.System|}.GetTimestamp;
        }
        """,
        Diagnostic("RASM3107").WithLocation(0).WithArguments("TimeProvider.System"),
        Diagnostic("RASM3107").WithLocation(1).WithArguments("TimeProvider.System"),
        Diagnostic("RASM3107").WithLocation(2).WithArguments("Clock.System"),
        Diagnostic("RASM3107").WithLocation(3).WithArguments("TimeProvider.System"));

    [Fact(DisplayName = "A TimeProvider argument, nameof, and a same-named member of another type are clean; exemplar SergeyTeplyakov/ErrorProne.NET@b57da22 src/ErrorProne.NET.CoreAnalyzers.Tests/AsyncAnalyzers/DoNotUseThreadSleepAnalyzerTests.cs NoWarningOnOtherMethodsWithSleepName")]
    public Task CallerHeldClock() => VerifyAnalyzerAsync(
        """
        using System;

        internal static class CallerHeldClock {
            internal static DateTimeOffset Read(TimeProvider time) => time.GetUtcNow();

            internal static string Named => nameof(TimeProvider.System);

            internal static TimeProvider Fixed => Frozen.System;
        }

        internal sealed class Frozen : TimeProvider {
            internal static new Frozen System { get; } = new();

            public override DateTimeOffset GetUtcNow() => DateTimeOffset.UnixEpoch;
        }
        """);

    [Theory(DisplayName = "Executables are composition roots and name TimeProvider.System; exemplar meziantou/Meziantou.Analyzer@27cbef6 tests/Meziantou.Analyzer.Test/Rules/MakeClassStaticAnalyzerTests.cs TopLevelStatement_9")]
    [InlineData(OutputKind.ConsoleApplication)]
    [InlineData(OutputKind.WindowsApplication)]
    [InlineData(OutputKind.WindowsRuntimeApplication)]
    public Task SystemClockInExecutable(OutputKind kind) => new CSharpCodeFixTest<AmbientClockAnalyzer, EmptyCodeFixProvider> {
        TestState = { OutputKind = kind },
        TestCode = """
            using System;

            _ = Host.Started(TimeProvider.System);

            internal static class Host {
                internal static DateTimeOffset Started(TimeProvider time) => time.GetUtcNow();
            }
            """,
    }.RunAsync(TestContext.Current.CancellationToken);
}
