using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.Members;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class VectorLaneComparisonTests : CSharpCodeFixVerifier<PlatformMemberAnalyzer, EmptyCodeFixProvider> {
    [Fact]
    public Task ReportsVector2AndVector3AllOrAnyComparison() => VerifyAnalyzerAsync(
        """
        using System;
        using System.Numerics;

        internal static class WidenedComparisons {
            internal static bool Above(Vector3 left, Vector3 right) => {|#0:Vector3.GreaterThanAll(left, right)|};

            internal static bool Touching(Vector2 left, Vector2 right) => {|#1:Vector2.EqualsAny(left, right)|};

            internal static bool Within(Vector3 left, Vector3 right) => {|#2:Vector3.LessThanOrEqualAll(left, right)|};

            internal static Func<Vector2, Vector2, bool> Grouped => {|#3:Vector2.GreaterThanAny|};
        }
        """,
        Diagnostic("RASM3112").WithLocation(0).WithArguments("Vector3.GreaterThanAll(left, right)"),
        Diagnostic("RASM3112").WithLocation(1).WithArguments("Vector2.EqualsAny(left, right)"),
        Diagnostic("RASM3112").WithLocation(2).WithArguments("Vector3.LessThanOrEqualAll(left, right)"),
        Diagnostic("RASM3112").WithLocation(3).WithArguments("Vector2.GreaterThanAny"));

    [Fact]
    public Task IgnoresLaneMaskFormsEqualityScalarAndVector4Comparisons() => VerifyAnalyzerAsync(
        """
        using System.Numerics;

        internal static class ComponentComparisons {
            internal static bool Above(Vector3 left, Vector3 right) => Vector3.AllWhereAllBitsSet(Vector3.GreaterThan(left, right));

            internal static bool Touching(Vector2 left, Vector2 right) => Vector2.AnyWhereAllBitsSet(Vector2.GreaterThan(left, right));

            internal static bool Same(Vector3 left, Vector3 right) => left == right;

            internal static bool Uniform(Vector3 vector) => Vector3.All(vector, 1f);

            internal static bool Zeroed(Vector2 vector) => Vector2.Any(vector, 0f);

            internal static bool Full(Vector4 left, Vector4 right) => Vector4.GreaterThanAll(left, right);

            internal static string Named => nameof(Vector3.GreaterThanAll);
        }
        """);
}
