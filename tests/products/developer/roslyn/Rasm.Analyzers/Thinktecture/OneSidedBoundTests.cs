using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.Thinktecture;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class OneSidedBoundTests : CSharpCodeFixVerifier<OneSidedBoundAnalyzer, EmptyCodeFixProvider> {
    [Fact]
    public Task ReportsAFloatingValueBoundedOnlyFromBelowOrOnlyFromAbove() => VerifyAnalyzerAsync(
        """
        internal static class Measured {
            private static void ValidateFactoryArguments(ref string? error, ref double {|#0:absolute|}, ref float {|#1:offset|}) =>
                error = absolute > 0d && 1f > offset ? null : "outside";
        }
        """,
        Diagnostic().WithLocation(0).WithArguments("absolute", "below", "+∞", "above"),
        Diagnostic().WithLocation(1).WithArguments("offset", "above", "-∞", "below"));

    [Fact]
    public Task StaysSilentOnTwoSidedFiniteCheckedOrIntegralValuesAndOutsideTheHook() => VerifyAnalyzerAsync(
        """
        internal static class Measured {
            private static void ValidateFactoryArguments(ref string? error, ref double relative, ref double absolute, ref double angle, ref int count) =>
                error = relative is > 0d and < 1d && double.IsFinite(absolute) && absolute > 0d && angle > 0d && angle <= 3.2 && count > 0 ? null : "outside";

            internal static bool Elsewhere(double free) => free > 0d;
        }
        """);
}
