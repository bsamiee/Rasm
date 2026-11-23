using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.Members;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class AmbientClockTests : CSharpCodeFixVerifier<AmbientClockAnalyzer, EmptyCodeFixProvider> {

    // --- [RASM3106]

    [Fact]
    public Task ReportsAmbientClockReadsByMemberOrTypeAndIgnoresReadsOfAnInjectedClock() => VerifyAnalyzerAsync(
        """
        using System;
        using System.Diagnostics;
        using System.Threading;
        using Calendar = System.DateTime;

        internal static class Reads {
            internal static DateTime Utc => {|RASM3106:Calendar.UtcNow|};
            internal static Stopwatch Timed => {|RASM3106:Stopwatch.StartNew()|};
            internal static Stopwatch Made => {|RASM3106:new Stopwatch()|};
            internal static long Rate => {|RASM3106:Stopwatch.Frequency|};
            internal static Action<int> Paused => {|RASM3106:Thread.Sleep|};
            internal static DateTimeOffset Injected(TimeProvider time) => time.GetUtcNow();
            internal static DateTime Epoch => DateTime.UnixEpoch;
            internal static string Named => nameof(DateTime.UtcNow);
        }
        """);

    // --- [RASM3107]

    [Fact]
    public Task ReportsTheSystemClockInALibraryAndIgnoresASameNamedMemberOfAnotherProvider() => VerifyAnalyzerAsync(
        """
        using System;

        internal static class Library {
            internal static TimeProvider Defaulted(TimeProvider? time) => time ?? {|RASM3107:TimeProvider.System|};
            internal static TimeProvider Fixed => Frozen.System;
        }

        internal sealed class Frozen : TimeProvider {
            internal static new Frozen System { get; } = new();

            public override DateTimeOffset GetUtcNow() => DateTimeOffset.UnixEpoch;
        }
        """);

    [Fact]
    public Task IgnoresTheSystemClockInACompilationWithAnEntryPointAndStillReportsAmbientReads() => new CSharpCodeFixTest<AmbientClockAnalyzer, EmptyCodeFixProvider> {
        TestState = { OutputKind = OutputKind.ConsoleApplication },
        TestCode = """
            using System;

            _ = Host.Started(TimeProvider.System);

            internal static class Host {
                internal static DateTimeOffset Started(TimeProvider time) => time.GetUtcNow();
                internal static DateTime Now => {|RASM3106:DateTime.UtcNow|};
            }
            """,
    }.RunAsync(TestContext.Current.CancellationToken);

    [Fact]
    public async Task IgnoresTheSystemClockInATypeTheReferencedPlugInShellEntersAndReportsItElsewhere() {
        CancellationToken token = TestContext.Current.CancellationToken;
        CSharpCodeFixTest<AmbientClockAnalyzer, EmptyCodeFixProvider> test = new() {
            TestCode = """
                using System;
                using Rasm.Rhino;
                using Rasm.Rhino.PlugIns;

                namespace Hosted;

                internal abstract class Entered : IDefined<PlugInDefinition> {
                    public static PlugInDefinition Definition { get; } = new(TimeProvider.System);
                }

                internal abstract class Registered : IDefined<EffectDefinition> {
                    public static EffectDefinition Definition { get; } = new({|RASM3107:TimeProvider.System|});
                }
                """,
        };
        test.TestState.AdditionalReferences.Add(AdditionalMetadataReferences.Emitted(await AdditionalMetadataReferences.CompiledAsync("Rasm.Rhino", """
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
                    public static PlugInDefinition PlugIn<TDefinition>(object host) where TDefinition : Rasm.Rhino.IDefined<PlugInDefinition> => TDefinition.Definition;
                }
            }
            """, token), token));
        await test.RunAsync(token);
    }
}
