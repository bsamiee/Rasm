using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.LanguageExtRules;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class HandPairedExclusionTests : CSharpCodeFixVerifier<HandPairedExclusionAnalyzer, EmptyCodeFixProvider> {
    [Fact(DisplayName = "SemaphoreSlim and ConcurrentExclusiveSchedulerPair creations are reported; exemplar meziantou/Meziantou.Analyzer@27cbef6 tests/Meziantou.Analyzer.Test/Rules/UseEventArgsEmptyAnalyzerTests.cs ShouldReport")]
    public Task HandPaired() => VerifyAnalyzerAsync(
        """
        using System.Threading;
        using System.Threading.Tasks;

        internal sealed class HandPaired {
            private readonly SemaphoreSlim gate = {|#0:new SemaphoreSlim(1, 1)|};

            private readonly SemaphoreSlim typed = {|#1:new(1)|};

            internal int Free => gate.CurrentCount + typed.CurrentCount;

            internal static ConcurrentExclusiveSchedulerPair Schedulers() => {|#2:new ConcurrentExclusiveSchedulerPair()|};
        }
        """,
        Diagnostic().WithLocation(0).WithArguments("new SemaphoreSlim(1, 1)"),
        Diagnostic().WithLocation(1).WithArguments("new(1)"),
        Diagnostic().WithLocation(2).WithArguments("new ConcurrentExclusiveSchedulerPair()"));

    [Fact(DisplayName = "Signals, cancellation, nameof, and a same-named type of another namespace are clean; exemplar SergeyTeplyakov/ErrorProne.NET@b57da22 src/ErrorProne.NET.CoreAnalyzers.Tests/AsyncAnalyzers/DoNotUseThreadSleepAnalyzerTests.cs NoWarningOnOtherMethodsWithSleepName")]
    public Task OtherPrimitives() => VerifyAnalyzerAsync(
        """
        using System.Threading;

        internal sealed class OtherPrimitives {
            private readonly ManualResetEventSlim signal = new(initialState: false);

            private readonly CancellationTokenSource stop = new();

            internal bool Ready => signal.IsSet && !stop.IsCancellationRequested;

            internal static string Named => nameof(SemaphoreSlim);

            internal static Gates.SemaphoreSlim Local => new Gates.SemaphoreSlim();
        }

        namespace Gates {
            internal sealed class SemaphoreSlim;
        }
        """);

    [Fact(DisplayName = "A compilation without LanguageExt holds no Conduit to run and is clean; exemplar SergeyTeplyakov/ErrorProne.NET@b57da22 src/ErrorProne.NET.CoreAnalyzers.Tests/ExcludeFromCodeCoverageMessageAnalyzerTests.cs NoWarningWhenJustificationNotAvailable_NetStandard20")]
    public Task WithoutLanguageExt() {
        CSharpCodeFixTest<HandPairedExclusionAnalyzer, EmptyCodeFixProvider> test = new() {
            TestCode = """
                using System.Threading;

                internal static class WithoutLanguageExt {
                    internal static SemaphoreSlim Gate() => new SemaphoreSlim(1, 1);
                }
                """,
        };
        test.TestState.AdditionalReferences.Clear();
        return test.RunAsync(TestContext.Current.CancellationToken);
    }
}
