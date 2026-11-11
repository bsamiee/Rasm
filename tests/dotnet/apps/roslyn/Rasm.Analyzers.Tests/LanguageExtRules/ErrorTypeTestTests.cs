using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.LanguageExtRules;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class ErrorTypeTestTests : CSharpCodeFixVerifier<ErrorTypeTestAnalyzer, EmptyCodeFixProvider> {
    [Fact]
    public Task CastAndRuntimeType() => VerifyAnalyzerAsync(
        """
        using System;
        using LanguageExt.Common;

        internal static class CastAndRuntimeType {
            internal static int Code(Error error) => ({|#0:(Expected)error|}).Code;
            internal static Type Kind(Error error) => {|#1:error.GetType()|};
            internal static string Sorted(Error error) {
                switch (error) {
                    case {|#2:Expected|}:
                        return "expected";
                    default:
                        return "other";
                }
            }
        }
        """,
        Diagnostic().WithLocation(0).WithArguments("Narrowing `error` to `Expected`", "error", "Expected"),
        Diagnostic().WithLocation(1).WithArguments("`error.GetType()`", "error", "<case>"),
        Diagnostic().WithLocation(2).WithArguments("Narrowing `error` to `Expected`", "error", "Expected"));

    [Fact]
    public Task TypeTestOnError() => VerifyAnalyzerAsync(
        """
        using LanguageExt.Common;

        internal static class TypeTestOnError {
            internal static bool Tested(Error error) => {|#0:error is Expected|};
            internal static string Named(Error error) => error switch { {|#1:Expected|} => "expected", _ => "other" };
        }
        """,
        Diagnostic().WithLocation(0).WithArguments("Narrowing `error` to `Expected`", "error", "Expected"),
        Diagnostic().WithLocation(1).WithArguments("Narrowing `error` to `Expected`", "error", "Expected"));

    [Fact]
    public Task IteratedLeaf() => VerifyAnalyzerAsync(
        """
        using System.Linq;
        using LanguageExt.Common;

        internal static class IteratedLeaf {
            internal static bool Any(Error error) => error.AsIterable().Any(static leaf => leaf is Expected);
        }
        """);

    [Fact]
    public Task LeafTypeTest() => VerifyAnalyzerAsync(
        """
        using LanguageExt.Common;

        internal static class LeafTypeTest {
            internal static bool Leaf(Error error) => error.Head is Expected;
            internal static string Split(Error error) => error switch { ManyErrors => "many", Expected => "expected", _ => "other" };
        }
        """);
}
