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
                internal static TimeProvider Clock => TimeProvider.System;

                internal static DateTimeOffset Started(TimeProvider time) => time.GetUtcNow();
            }
            """,
    }.RunAsync(TestContext.Current.CancellationToken);

    [Fact]
    public Task IgnoresTimeProviderSystemInTypePlugInShellEnters() => VerifyAnalyzerAsync(
        """
        using System;

        namespace Rasm.Rhino {
            public interface IDefined<TDefinition> {
                static abstract TDefinition Definition { get; }
            }
        }

        namespace Rasm.Rhino.PlugIns {
            public sealed record PlugInDefinition(TimeProvider Clock);

            public sealed record EffectDefinition(TimeProvider Clock);

            public static class PlugInShell {
                public static PlugInDefinition PlugIn<TDefinition>(object host) where TDefinition : IDefined<PlugInDefinition> => TDefinition.Definition;
            }
        }

        namespace Hosted {
            using Rasm.Rhino;
            using Rasm.Rhino.PlugIns;

            internal abstract class Entered : IDefined<PlugInDefinition> {
                internal static TimeProvider Clock { get; } = TimeProvider.System;

                public static PlugInDefinition Definition { get; } = new(TimeProvider.System);

                internal static Func<TimeProvider> Deferred => static () => TimeProvider.System;
            }

            internal abstract class Registered : IDefined<EffectDefinition> {
                public static EffectDefinition Definition { get; } = new({|#0:TimeProvider.System|});
            }

            internal static class Inward {
                internal static DateTimeOffset Read() => {|#1:TimeProvider.System|}.GetUtcNow();
            }
        }
        """,
        Diagnostic("RASM3107").WithLocation(0).WithArguments("TimeProvider.System"),
        Diagnostic("RASM3107").WithLocation(1).WithArguments("TimeProvider.System"));
}
