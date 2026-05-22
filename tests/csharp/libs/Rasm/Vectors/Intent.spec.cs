using Rasm.TestKit;
using Rasm.Vectors;

namespace Rasm.Tests.Vectors;

// --- [CONSTANTS] ----------------------------------------------------------------------------
// SupportProjection metadata stays pure-managed; projection behavior itself routes through
// SupportSpace and RhinoCommon, so projection laws belong in the bridge rail.
[System.Diagnostics.CodeAnalysis.SuppressMessage(category: "Design", checkId: "CA1515", Justification = "xUnit discovers public test surface.")]
public static class IntentGens {
    public static readonly SupportProjection[] All = [
        SupportProjection.Closest, SupportProjection.Direction, SupportProjection.Span, SupportProjection.SignedSpanAway,
        SupportProjection.Normal, SupportProjection.Distance, SupportProjection.Parameter, SupportProjection.Uv,
        SupportProjection.Component, SupportProjection.MeshPoint, SupportProjection.SignedDistance,
        SupportProjection.ContainmentDistance, SupportProjection.Tangent, SupportProjection.Frame,
    ];
    public static readonly Gen<SupportProjection> Projection = Gen.OneOfConst(All);
}

// --- [ALGEBRAIC] ----------------------------------------------------------------------------
public sealed class SupportProjectionLaws {
    [Fact]
    public void KeysAreDistinctAcrossFourteenCases() =>
        Assert.Equal(
            expected: IntentGens.All.Length,
            actual: IntentGens.All.Select(static (SupportProjection p) => p.Key).Distinct().Count());
    [Fact]
    public void ProjectionGeneratorEmitsDeclaredCases() =>
        Spec.ForAll(IntentGens.Projection, p => Assert.Contains(expected: p, collection: IntentGens.All));
    [Fact]
    public void SignedSpanAwayIsDistinctFromSpan() =>
        Assert.NotEqual(expected: SupportProjection.Span.Key, actual: SupportProjection.SignedSpanAway.Key);
    [Fact]
    public void ParameterIsDistinctFromDistance() =>
        Assert.NotEqual(expected: SupportProjection.Distance.Key, actual: SupportProjection.Parameter.Key);
}
