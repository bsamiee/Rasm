using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.Members;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class VectorLaneComparisonTests : CSharpCodeFixVerifier<PlatformMemberAnalyzer, EmptyCodeFixProvider> {
    [Fact]
    public Task ReportsWidenedVector2AndVector3ComparisonsAndIgnoresMaskFormsAndVector4() => VerifyAnalyzerAsync(
        """
        using System;
        using System.Numerics;

        internal static class Compared {
            internal static bool Above(Vector3 left, Vector3 right) => {|RASM3112:Vector3.GreaterThanAll(left, right)|};
            internal static Func<Vector2, Vector2, bool> Touching => {|RASM3112:Vector2.EqualsAny|};
            internal static bool Masked(Vector3 left, Vector3 right) => Vector3.AllWhereAllBitsSet(Vector3.GreaterThan(left, right));
            internal static bool Uniform(Vector3 vector) => Vector3.All(vector, 1f);
            internal static bool Full(Vector4 left, Vector4 right) => Vector4.GreaterThanAll(left, right);
        }
        """);
}
