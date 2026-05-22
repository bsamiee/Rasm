using Rasm.TestKit;
using Rasm.Vectors;

namespace Rasm.Tests.Vectors;

// --- [CONSTANTS] ----------------------------------------------------------------------------
// SupportProjection (14 SmartEnum items) carries pure-managed metadata: Key, ParameterMode,
// Sign. Project<TOut> dispatch routes through SupportSpace.Closest / SignedDistance which
// invoke RhinoCommon, so the actual projection lives in the bridge rail. VectorIntent factories
// likewise hit native validation (AcceptValue<Point3d>); Lerp/Slerp use Math.Clamp on the
// parameter (pure managed) and that clamp invariant is statically verifiable.
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
    public void SignIsUnitMagnitudeForEveryCase() =>
        Spec.ForAll(IntentGens.Projection, p => Spec.EqualWithin(left: Math.Abs(value: p.Sign), right: 1.0, tolerance: 0.0, what: "unit sign"));
    [Fact]
    public void SignedSpanAwayIsTheOnlyNegativeSign() =>
        Spec.ForAll(IntentGens.Projection, p =>
            Assert.Equal(expected: ReferenceEquals(objA: p, objB: SupportProjection.SignedSpanAway) ? -1.0 : 1.0, actual: p.Sign));
    [Fact]
    public void ParameterModeFlagsOnlyParameterCase() =>
        Spec.ForAll(IntentGens.Projection, p =>
            Assert.Equal(expected: ReferenceEquals(objA: p, objB: SupportProjection.Parameter), actual: p.ParameterMode));
}
