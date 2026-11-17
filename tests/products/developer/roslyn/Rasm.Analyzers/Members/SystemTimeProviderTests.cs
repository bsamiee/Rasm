using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.Members;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class SystemTimeProviderTests : CSharpCodeFixVerifier<AmbientClockAnalyzer, EmptyCodeFixProvider> {
    [Fact]
    public Task ReportsTimeProviderSystemInLibrary() => VerifyAnalyzerAsync(
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

    [Fact]
    public Task IgnoresTimeProviderArgumentAndSameNamedMember() => VerifyAnalyzerAsync(
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

    [Theory]
    [InlineData(OutputKind.ConsoleApplication)]
    [InlineData(OutputKind.WindowsApplication)]
    [InlineData(OutputKind.WindowsRuntimeApplication)]
    public Task IgnoresTimeProviderSystemInExecutable(OutputKind kind) => new CSharpCodeFixTest<AmbientClockAnalyzer, EmptyCodeFixProvider> {
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
