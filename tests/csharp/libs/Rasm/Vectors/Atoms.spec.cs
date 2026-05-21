using Rasm.Domain;
using Rasm.TestKit;
using Rasm.Vectors;
using Rhino;
using Rhino.Geometry;
using Xunit.Sdk;

namespace Rasm.Tests.Vectors;

// --- [CONSTANTS] ----------------------------------------------------------------------------
// Direction/VectorSpan/VectorRing tests live in the bridge rail (*.verify.csx) because
// RhinoCommon's Vector3d.IsTiny/IsValid/Unitize/VectorAngle delegate to native code that
// loads only inside a Rhino runtime. The static rail covers pure-managed surfaces below.
public static class AtomGens {
    public static readonly Op Key = Op.Of(name: "atoms-test");
    public static readonly Gen<VectorAngle> Angle = Gens.UnitAngle.Select(static (double radians) =>
        VectorAngle.TryCreate(value: radians, obj: out VectorAngle v) ? v : throw new InvalidOperationException("generator invariant broken: angle"));
    public static readonly Gen<PositiveMagnitude> Magnitude = Gens.Positive.Select(static (double scalar) =>
        PositiveMagnitude.TryCreate(value: scalar, obj: out PositiveMagnitude m) ? m : throw new InvalidOperationException("generator invariant broken: magnitude"));
    public static readonly Gen<SignedAxis> Axis = Gen.OneOfConst(SignedAxis.PositiveX, SignedAxis.NegativeX, SignedAxis.PositiveY, SignedAxis.NegativeY, SignedAxis.PositiveZ, SignedAxis.NegativeZ);
    public static readonly Gen<BoundarySense> Sense = Gen.OneOfConst(BoundarySense.Toward, BoundarySense.Away);
}

// --- [ALGEBRAIC] ----------------------------------------------------------------------------
public sealed class VectorAngleProps {
    [Fact]
    public void ClosureRejectsOutsideUnitCircle() =>
        Spec.ForAll(Gens.Finite.Where(static x => x is < 0.0 or > 7.0), static r => Assert.False(VectorAngle.TryCreate(value: r, obj: out _)));
    [Fact]
    public void ClosureRejectsNonFinite() =>
        Spec.ForAll(Gen.OneOfConst(double.NaN, double.PositiveInfinity, double.NegativeInfinity), static r => Assert.False(VectorAngle.TryCreate(value: r, obj: out _)));
    [Fact]
    public void ClosureAcceptsUnitCircle() =>
        Spec.ForAll(Gens.UnitAngle, static r => Assert.True(VectorAngle.TryCreate(value: r, obj: out _)));
    [Fact]
    public void ValueRoundtripsThroughFactory() =>
        Spec.Roundtrip(AtomGens.Angle, forward: static (VectorAngle a) => a.Value, back: static (double r) =>
            VectorAngle.TryCreate(value: r, obj: out VectorAngle v) ? v : throw new InvalidOperationException("roundtrip lost value"));
}

public sealed class PositiveMagnitudeProps {
    [Fact]
    public void ClosureRejectsZeroOrNegative() =>
        Spec.ForAll(Gens.Finite.Where(static x => x <= RhinoMath.ZeroTolerance), static x => Assert.False(PositiveMagnitude.TryCreate(value: x, obj: out _)));
    [Fact]
    public void ClosureRejectsNonFinite() =>
        Spec.ForAll(Gen.OneOfConst(double.NaN, double.PositiveInfinity, double.NegativeInfinity), static x => Assert.False(PositiveMagnitude.TryCreate(value: x, obj: out _)));
    [Fact]
    public void ClosureAcceptsPositiveFinite() =>
        Spec.ForAll(Gens.Positive, static x => Assert.True(PositiveMagnitude.TryCreate(value: x, obj: out _)));
    [Fact]
    public void ValueRoundtripsThroughFactory() =>
        Spec.Roundtrip(AtomGens.Magnitude, forward: static (PositiveMagnitude m) => m.Value, back: static (double x) =>
            PositiveMagnitude.TryCreate(value: x, obj: out PositiveMagnitude m) ? m : throw new InvalidOperationException("roundtrip lost value"));
}

public sealed class BoundarySenseLaws {
    [Fact] public void TowardCarriesPositiveUnitSign() => Assert.Equal(expected: 1.0, actual: BoundarySense.Toward.Sign);
    [Fact] public void AwayCarriesNegativeUnitSign() => Assert.Equal(expected: -1.0, actual: BoundarySense.Away.Sign);
    [Fact]
    public void SignIsUnitMagnitudeForEveryCase() =>
        Spec.ForAll(AtomGens.Sense, static s => Spec.EqualWithin(left: Math.Abs(s.Sign), right: 1.0, tolerance: 0.0, what: "sense unit magnitude"));
}

public sealed class SignedAxisLaws {
    [Fact] public void CardinalPlanarYieldsFourAxes() => Assert.Equal(expected: 4, actual: SignedAxis.Cardinal(planar: true).Count);
    [Fact] public void CardinalFullYieldsSixAxes() => Assert.Equal(expected: 6, actual: SignedAxis.Cardinal(planar: false).Count);
    [Fact]
    public void WorldAxisIsUnitLength() =>
        Spec.ForAll(AtomGens.Axis, static a => Spec.EqualWithin(left: a.World.Length, right: 1.0, tolerance: 1.0e-12, what: "axis world length"));
    [Fact]
    public void PlanarCardinalIsSubsetOfFullCardinal() {
        Seq<SignedAxis> full = SignedAxis.Cardinal(planar: false);
        Spec.ForAll(Gen.OneOfConst(SignedAxis.Cardinal(planar: true).ToArray()), axis => Assert.Contains(expected: axis, collection: full));
    }
    [Fact]
    public void CrossProductIsPerpendicularToBothInputs() =>
        AtomGens.Axis.Select(AtomGens.Axis).Sample(static (SignedAxis a, SignedAxis b) => {
            Vector3d cross = Vector3d.CrossProduct(a: a.World, b: b.World);
            return Math.Abs(value: cross * a.World) < 1.0e-12 && Math.Abs(value: cross * b.World) < 1.0e-12
                ? true
                : throw new XunitException($"cross={cross} not ⊥ to a={a.World} (dot={cross * a.World}) or b={b.World} (dot={cross * b.World})");
        });
}

