using Rasm.Domain;
using Rasm.TestKit;
using Rasm.Vectors;
using Rhino.Geometry;

namespace Rasm.Tests.Vectors;

// --- [CONSTANTS] ----------------------------------------------------------------------------
// SupportProjection metadata stays pure-managed; projection behavior itself routes through
// SupportSpace and RhinoCommon, so projection laws belong in the bridge rail.
[System.Diagnostics.CodeAnalysis.SuppressMessage(category: "Design", checkId: "CA1515", Justification = "xUnit discovers public test surface.")]
public static class IntentGens {
    public static readonly Op Key = Op.Of(name: "intent-test");
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

public sealed class InterpolationIntentLaws {
    [Fact]
    public void LerpRejectsParametersOutsideUnitRange() =>
        Spec.ForAll(Gens.Finite.Where(static t => t is < 0.0 or > 1.0), static t =>
            Spec.Fail(VectorIntent.Lerp(a: Vector3d.XAxis, b: Vector3d.YAxis, t: t, key: IntentGens.Key)));
    [Fact]
    public void LerpAcceptsUnitRange() =>
        Spec.ForAll(Gens.UnitClosed, static t =>
            Spec.Succ(VectorIntent.Lerp(a: Vector3d.XAxis, b: Vector3d.YAxis, t: t, key: IntentGens.Key),
                then: intent => Spec.EqualWithin(
                    left: Assert.IsType<VectorIntent.LerpCase>(@object: intent).Parameter.Value,
                    right: t,
                    tolerance: 0.0,
                    what: "lerp t")));
    [Fact]
    public void PoseValidatesUnitRange() {
        Spec.ForAll(Gens.UnitClosed, t =>
            Spec.Succ(VectorIntent.Pose(from: Plane.WorldXY, to: Plane.WorldZX, t: t, mode: MotionInterpolation.Linear, key: IntentGens.Key), then: intent =>
                Spec.EqualWithin(left: Assert.IsType<VectorIntent.PoseCase>(@object: intent).Parameter.Value, right: t, tolerance: 0.0, what: "pose t")));
        Spec.ForAll(Gens.Finite.Where(static t => t is < 0.0 or > 1.0), t =>
            Spec.Fail(VectorIntent.Pose(from: Plane.WorldXY, to: Plane.WorldZX, t: t, mode: MotionInterpolation.Linear, key: IntentGens.Key)));
    }
}
