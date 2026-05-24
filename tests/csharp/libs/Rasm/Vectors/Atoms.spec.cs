using Rasm.Domain;
using Rasm.TestKit;
using Rasm.Vectors;
using Rhino;
using Rhino.Geometry;

namespace Rasm.Tests.Vectors;

// --- [CONSTANTS] ----------------------------------------------------------------------------
// Direction/VectorSpan/VectorFrame/VectorCone/VectorRing tests live in the bridge rail (*.verify.csx) because
// RhinoCommon's Vector3d.IsTiny/IsValid/Unitize/VectorAngle delegate to native code that
// loads only inside a Rhino runtime. The static rail covers pure-managed surfaces below.
internal static class AtomGens {
    public static readonly Op Key = Op.Of(name: "atoms-test");
    public static readonly Context Model = Spec.SuccValue(Context.Of(absolute: 0.001, relative: 1.0e-8, angle: 0.01, units: UnitSystem.Millimeters).ToFin(), label: "atoms context");
    public static readonly Gen<VectorAngle> Angle = Gens.UnitAngle.Select(static (double radians) =>
        VectorAngle.TryCreate(value: radians, obj: out VectorAngle v) ? v : throw new InvalidOperationException("generator invariant broken: angle"));
    public static readonly Gen<PositiveMagnitude> Magnitude = Gens.Positive.Select(static (double scalar) =>
        PositiveMagnitude.TryCreate(value: scalar, obj: out PositiveMagnitude m) ? m : throw new InvalidOperationException("generator invariant broken: magnitude"));
    public static readonly Gen<UnitInterval> Unit = Gens.UnitClosed.Select(static (double scalar) =>
        UnitInterval.TryCreate(value: scalar, obj: out UnitInterval u) ? u : throw new InvalidOperationException("generator invariant broken: unit interval"));
    public static readonly Gen<SignedAxis> Axis = Gen.OneOfConst(SignedAxis.PositiveX, SignedAxis.NegativeX, SignedAxis.PositiveY, SignedAxis.NegativeY, SignedAxis.PositiveZ, SignedAxis.NegativeZ);
    public static readonly Gen<BoundarySense> Sense = Gen.OneOfConst(BoundarySense.Toward, BoundarySense.Away);
}

// --- [ALGEBRAIC] ----------------------------------------------------------------------------
public sealed class VectorAngleProps {
    [Fact]
    public void ClosureRejectsOutsideUnitCircle() =>
        Spec.ForAll(Gens.Finite.Where(static x => x is < 0.0 or > RhinoMath.TwoPI), static r => Assert.False(VectorAngle.TryCreate(value: r, obj: out _)));
    [Fact]
    public void ClosureRejectsNonFinite() =>
        Spec.ValueObjectRejects(invalid: Gens.NonFinite, tryCreate: static r => VectorAngle.TryCreate(value: r, obj: out _));
    [Fact]
    public void ClosureAcceptsUnitCircle() =>
        Spec.ForAll(Gens.UnitAngle, static r => Assert.True(VectorAngle.TryCreate(value: r, obj: out _)));
    [Fact]
    public void ValueRoundtripsThroughFactory() =>
        Spec.Roundtrip(AtomGens.Angle, forward: static (VectorAngle a) => a.Value, back: static (double r) =>
            VectorAngle.TryCreate(value: r, obj: out VectorAngle v) ? v : throw new InvalidOperationException("roundtrip lost value"));
    [Fact]
    public void ProjectionOwnsSelfScalarAndUnsupportedRails() =>
        Spec.ForAll(AtomGens.Angle, static angle => {
            Spec.Succ(angle.Project<VectorAngle>(key: AtomGens.Key), then: actual => Assert.Equal(expected: angle, actual: actual));
            Spec.Succ(angle.Project<double>(key: AtomGens.Key), then: value => Spec.EqualWithin(left: value, right: angle.Value, tolerance: 0.0, what: "angle scalar"));
            Spec.FailCategory(angle.Project<Point3d>(key: AtomGens.Key), category: "Unsupported");
        });
}

public sealed class PositiveMagnitudeProps {
    [Fact]
    public void ClosureRejectsZeroOrNegative() =>
        Spec.ForAll(Gens.Finite.Where(static x => x <= RhinoMath.ZeroTolerance), static x => Assert.False(PositiveMagnitude.TryCreate(value: x, obj: out _)));
    [Fact]
    public void ClosureRejectsNonFinite() =>
        Spec.ValueObjectRejects(invalid: Gens.NonFinite, tryCreate: static x => PositiveMagnitude.TryCreate(value: x, obj: out _));
    [Fact]
    public void ClosureAcceptsPositiveFinite() =>
        Spec.ForAll(Gens.Positive, static x => Assert.True(PositiveMagnitude.TryCreate(value: x, obj: out _)));
    [Fact]
    public void ValueRoundtripsThroughFactory() =>
        Spec.Roundtrip(AtomGens.Magnitude, forward: static (PositiveMagnitude m) => m.Value, back: static (double x) =>
            PositiveMagnitude.TryCreate(value: x, obj: out PositiveMagnitude m) ? m : throw new InvalidOperationException("roundtrip lost value"));
}

public sealed class UnitIntervalProps {
    [Fact]
    public void ClosureRejectsOutsideUnitRange() =>
        Spec.ForAll(Gens.Finite.Where(static x => x is < 0.0 or > 1.0), static x => Assert.False(UnitInterval.TryCreate(value: x, obj: out _)));
    [Fact]
    public void ClosureRejectsNonFinite() =>
        Spec.ValueObjectRejects(invalid: Gens.NonFinite, tryCreate: static x => UnitInterval.TryCreate(value: x, obj: out _));
    [Fact]
    public void ClosureAcceptsUnitRange() =>
        Spec.ValueObjectAccepts(valid: Gens.UnitClosed, tryCreate: static x => UnitInterval.TryCreate(value: x, obj: out _));
    [Fact]
    public void ValueRoundtripsThroughFactory() =>
        Spec.Roundtrip(AtomGens.Unit, forward: static (UnitInterval u) => u.Value, back: static (double x) =>
            UnitInterval.TryCreate(value: x, obj: out UnitInterval u) ? u : throw new InvalidOperationException("roundtrip lost value"));
}

public sealed class DimensionProps {
    [Fact]
    public void ClosureRejectsValuesBelowOneAndAcceptsPositiveIntegers() {
        Spec.ForAll(Gen.Int[-100, 0], static value => Assert.False(Rasm.Vectors.Dimension.TryCreate(value: value, obj: out _)));
        Spec.ForAll(Gens.Dimension, static value => Assert.True(value.Value >= 1));
    }
}

public sealed class BoundarySenseLaws {
    [Fact]
    public void TowardCarriesPositiveUnitSign() => Assert.Equal(expected: 1.0, actual: BoundarySense.Toward.Sign);
    [Fact]
    public void AwayCarriesNegativeUnitSign() => Assert.Equal(expected: -1.0, actual: BoundarySense.Away.Sign);
    [Fact]
    public void SignIsUnitMagnitudeForEveryCase() =>
        Spec.ForAll(AtomGens.Sense, static s => Spec.EqualWithin(left: Math.Abs(s.Sign), right: 1.0, tolerance: 0.0, what: "sense unit magnitude"));
}

public sealed class SignedAxisLaws {
    [Fact]
    public void CardinalPlanarYieldsFourAxes() => Assert.Equal(expected: 4, actual: SignedAxis.Cardinal(planar: true).Count);
    [Fact]
    public void CardinalFullYieldsSixAxes() => Assert.Equal(expected: 6, actual: SignedAxis.Cardinal(planar: false).Count);
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
        Spec.ForAll(AtomGens.Axis.Select(AtomGens.Axis), static p => {
            (SignedAxis a, SignedAxis b) = p;
            Vector3d cross = Vector3d.CrossProduct(a: a.World, b: b.World);
            Spec.Holds(
                condition: Math.Abs(value: cross * a.World) < 1.0e-12 && Math.Abs(value: cross * b.World) < 1.0e-12,
                label: $"cross={cross} not perpendicular to a={a.World} or b={b.World}");
        });
}

public sealed class VectorRelationLaws {
    [Fact]
    public void RelationKeysAreDistinctAndProjectionRejectsForeignOutput() {
        VectorRelation[] all = [VectorRelation.Oblique, VectorRelation.Parallel, VectorRelation.AntiParallel, VectorRelation.Perpendicular];
        Spec.SmartEnumKeysUnique(items: all, key: static relation => relation.Key);
        Spec.ForAll(Gen.OneOfConst(all), static relation => {
            Spec.Succ(relation.Project<VectorRelation>(key: AtomGens.Key), then: actual => Assert.Same(expected: relation, actual: actual));
            Spec.Fail(relation.Project<double>(key: AtomGens.Key));
        });
    }
}
