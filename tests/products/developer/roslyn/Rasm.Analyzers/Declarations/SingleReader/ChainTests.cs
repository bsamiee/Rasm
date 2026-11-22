using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.CodeFixes;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.Declarations.SingleReader;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class ChainTests : CSharpCodeFixVerifier<SingleReaderAnalyzer, SingleReaderCodeFix> {

    // --- [CYCLES]

    [Fact]
    public Task IgnoresMutuallyRecursivePrivateMethods() => VerifyAnalyzerAsync(
        """
        internal static class RecursiveMembers {
            private static int Read(int value) => ReadRead(value);

            private static int ReadRead(int value) => Read(value);
        }
        """);

    [Fact]
    public Task IgnoresMutuallyRecursivePrivateProperties() => VerifyAnalyzerAsync(
        """
        internal static class RecursiveProperties {
            private static int Read => ReadRead;

            private static int ReadRead => Read;
        }
        """);

    [Fact]
    public Task InlinesSecondCyclePropertyWhenOutsideReadEntersFirst() => VerifyCodeFixAsync(
        """
        internal static class ReadEnteredProperties {
            internal static int Value => Read;

            private static int Read => ReadRead;

            private static int {|#0:ReadRead|} => Read;
        }
        """,
        [
            Diagnostic("RASM3001").WithLocation(0).WithArguments("ReadRead", "Read").WithOptions(DiagnosticOptions.IgnoreAdditionalLocations),
        ],
        """
        internal static class ReadEnteredProperties {
            internal static int Value => Read;

            private static int Read => Read;
        }
        """);

    [Fact]
    public Task InlinesFirstCyclePropertyWhenOutsideReadEntersSecond() => VerifyCodeFixAsync(
        """
        internal static class ReadReadEnteredProperties {
            internal static int Value => ReadRead;

            private static int {|#0:Read|} => ReadRead;

            private static int ReadRead => Read;
        }
        """,
        [
            Diagnostic("RASM3001").WithLocation(0).WithArguments("Read", "ReadRead").WithOptions(DiagnosticOptions.IgnoreAdditionalLocations),
        ],
        """
        internal static class ReadReadEnteredProperties {
            internal static int Value => ReadRead;

            private static int ReadRead => ReadRead;
        }
        """);

    // --- [FIX_ALL]

    [Fact]
    public Task InlinesChainOfSingleReaderMethodsDownToConstant() => VerifyCodeFixAsync(
        """
        internal static class Sample {
            private const int ReadValue = 1;

            internal static int Value => ReadRead();

            private static int {|RASM3001:ReadRead|}() => Read();

            private static int {|RASM3001:Read|}() => ReadReadRead();

            private static int {|RASM3001:ReadReadRead|}() => ReadValue;
        }
        """,
        """
        internal static class Sample {
            private const int ReadValue = 1;

            internal static int Value => ReadValue;
        }
        """);

    [Fact]
    public Task InlinesSingleReaderMethodsAcrossEveryDocumentAndProject() {
        CSharpCodeFixTest<SingleReaderAnalyzer, SingleReaderCodeFix> test = new();
        ProjectState[] states = [test.TestState, test.TestState.AdditionalProjects["Other"]];
        foreach (ProjectState state in states) {
            ProjectState fixedState = state == test.TestState ? test.FixedState : test.FixedState.AdditionalProjects[state.Name];
            foreach (string name in new[] { "First", "Second" }) {
                state.Sources.Add(($"/{state.Name}/{name}.cs", $$"""
                    internal static class {{name}} {
                        internal static int Value => Read(1);

                        private static int {|RASM3001:Read|}(int read) => read;
                    }
                    """));
                fixedState.Sources.Add(($"/{state.Name}/{name}.cs", $$"""
                    internal static class {{name}} {
                        internal static int Value => 1;
                    }
                    """));
            }
        }
        test.NumberOfFixAllInDocumentIterations = states.Sum(static state => state.Sources.Count);
        test.NumberOfFixAllInProjectIterations = states.Length;
        return test.RunAsync(TestContext.Current.CancellationToken);
    }
}
