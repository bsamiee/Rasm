using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.LanguageExt;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class ErrorTypeTestTests : CSharpCodeFixVerifier<ErrorTypeTestAnalyzer, EmptyCodeFixProvider> {
    [Fact]
    public Task ReportsCaseTestsOnAWholeErrorAndIgnoresAggregateTestsHeadTestsAndUntypedValues() => VerifyAnalyzerAsync(
        """
        using System;
        using LanguageExt.Common;

        internal static class Classified {
            internal static bool Tested(Error error) => {|#0:error is Expected|};
            internal static int Bound(Error error) => {|#1:error is Expected expected|} ? expected.Code : 0;
            internal static int Cast(Error error) => ({|#2:(Expected)error|}).Code;
            internal static Type Kind(Error error) => {|#3:error.GetType()|};
            internal static bool Aggregate(Error error) => error is ManyErrors;
            internal static bool HeadTested(Error error) => error.Head is Expected;
            internal static bool Boxed(object value) => value is Expected;
        }
        """,
        Diagnostic().WithLocation(0).WithArguments("Narrowing `error` to `Expected`", "error", "Expected"),
        Diagnostic().WithLocation(1).WithArguments("Narrowing `error` to `Expected`", "error", "Expected"),
        Diagnostic().WithLocation(2).WithArguments("Narrowing `error` to `Expected`", "error", "Expected"),
        Diagnostic().WithLocation(3).WithArguments("`error.GetType()`", "error", "<case>"));

    [Fact]
    public Task IgnoresTypeTestsOnLeavesTheErrorIteratesAndReportsThemOnAnotherSequence() => VerifyAnalyzerAsync(
        """
        using System.Linq;
        using LanguageExt.Common;
        using static LanguageExt.Prelude;

        internal static class Iterated {
            internal static bool Leaves(Error error) => error.AsIterable().Where(static leaf => leaf.Code > 0).Any(static leaf => leaf is Expected);
            internal static bool Looped(Error error) {
                foreach (Error leaf in error.AsIterable())
                    if (leaf is Expected)
                        return true;
                return false;
            }
            internal static bool Wrapped(Error error) => Seq(error).Exists(static item => {|#0:item is Expected|});
        }
        """,
        Diagnostic().WithLocation(0).WithArguments("Narrowing `item` to `Expected`", "item", "Expected"));

    [Fact]
    public Task ReportsSwitchArmsBeforeAManyErrorsArmAndIgnoresArmsAfterIt() => VerifyAnalyzerAsync(
        """
        using LanguageExt.Common;

        internal static class Dispatched {
            internal static int Aggregated(Error error) => error switch { ManyErrors => 0, Expected => 1, _ => 2 };
            internal static int Leading(Error error) => error switch { {|#0:Expected|} => 1, ManyErrors => 0, _ => 2 };
            internal static int Labelled(Error error) {
                switch (error) {
                    case {|#1:Expected|}:
                        return 1;
                    case ManyErrors:
                        return 0;
                    default:
                        return 2;
                }
            }
        }
        """,
        Diagnostic().WithLocation(0).WithArguments("Narrowing `error` to `Expected`", "error", "Expected"),
        Diagnostic().WithLocation(1).WithArguments("Narrowing `error` to `Expected`", "error", "Expected"));
}
