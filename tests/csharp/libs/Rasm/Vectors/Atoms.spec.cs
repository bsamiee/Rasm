using Rasm.Domain;
using Rasm.TestKit;
using Rasm.Vectors;
using Rhino;
using Rhino.Geometry;

namespace Rasm.Tests.Vectors;

// --- [CONSTANTS] ----------------------------------------------------------------------------
// Direction/VectorSpan/VectorFrame/VectorCone tests live in the bridge rail (*.verify.csx) because
// RhinoCommon's Vector3d.IsTiny/IsValid/Unitize/VectorAngle delegate to native code that
// loads only inside a Rhino runtime. The static rail covers pure-managed surfaces below.
internal static class AtomGens {
    public static readonly Op Key = Op.Of(name: "atoms-test");
    public static readonly Gen<VectorAngle> Angle = Gens.UnitAngle.Select(static (double radians) =>
        VectorAngle.TryCreate(value: radians, obj: out VectorAngle v) ? v : throw new InvalidOperationException("generator invariant broken: angle"));
    public static readonly Gen<SignedAxis> Axis = Gen.OneOfConst(SignedAxis.PositiveX, SignedAxis.NegativeX, SignedAxis.PositiveY, SignedAxis.NegativeY, SignedAxis.PositiveZ, SignedAxis.NegativeZ);
    public static readonly BoundarySense[] Senses = [BoundarySense.Toward, BoundarySense.Away];
    public static readonly SignedAxis[] Axes = [SignedAxis.NegativeX, SignedAxis.PositiveX, SignedAxis.NegativeY, SignedAxis.PositiveY, SignedAxis.NegativeZ, SignedAxis.PositiveZ];
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
    public void ClosureAcceptsUnitCircleAndBoundaries() {
        Spec.ForAll(Gens.UnitAngle, static r => Assert.True(VectorAngle.TryCreate(value: r, obj: out _)));
        Spec.Cases(items: [0.0, RhinoMath.ZeroTolerance, Math.PI, RhinoMath.TwoPI - RhinoMath.ZeroTolerance, RhinoMath.TwoPI], key: static x => x,
            law: static x => Assert.True(condition: VectorAngle.TryCreate(value: x, obj: out _)));
        Spec.Cases(items: [-RhinoMath.ZeroTolerance, RhinoMath.TwoPI + RhinoMath.ZeroTolerance], key: static x => x,
            law: static x => Assert.False(condition: VectorAngle.TryCreate(value: x, obj: out _)));
    }
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
    public void ClosureAcceptsOnlyValuesAboveTolerance() {
        Spec.ForAll(Gens.PositiveMagnitudeScalar, static x => Assert.True(PositiveMagnitude.TryCreate(value: x, obj: out _)));
        Spec.Cases(items: [0.0, RhinoMath.ZeroTolerance * 0.5, RhinoMath.ZeroTolerance], key: static x => x,
            law: static x => Assert.False(condition: PositiveMagnitude.TryCreate(value: x, obj: out _)));
        Spec.Cases(items: [RhinoMath.ZeroTolerance * 2.0, 1.0], key: static x => x,
            law: static x => Assert.True(condition: PositiveMagnitude.TryCreate(value: x, obj: out _)));
        Assert.False(condition: PositiveMagnitude.TryCreate(value: default(PositiveMagnitude).Value, obj: out _));
    }
    [Fact]
    public void ValueRoundtripsThroughFactory() =>
        Spec.Roundtrip(Gens.PositiveMagnitude, forward: static (PositiveMagnitude m) => m.Value, back: static (double x) =>
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
    public void ClosureEdgesRejectOutsideAndDefaultIsLowerBound() {
        Spec.Cases(items: [-RhinoMath.ZeroTolerance, -double.Epsilon, 1.0 + RhinoMath.ZeroTolerance], key: static x => x,
            law: static x => Assert.False(condition: UnitInterval.TryCreate(value: x, obj: out _)));
        Spec.Cases(items: [0.0, RhinoMath.ZeroTolerance, 1.0 - RhinoMath.ZeroTolerance, 1.0], key: static x => x,
            law: static x => Assert.True(condition: UnitInterval.TryCreate(value: x, obj: out _)));
        Assert.True(condition: UnitInterval.TryCreate(value: default(UnitInterval).Value, obj: out UnitInterval unit));
        Spec.EqualWithin(left: unit.Value, right: 0.0, tolerance: 0.0, what: "default unit lower bound");
    }
    [Fact]
    public void ValueRoundtripsThroughFactory() =>
        Spec.Roundtrip(Gens.UnitInterval, forward: static (UnitInterval u) => u.Value, back: static (double x) =>
            UnitInterval.TryCreate(value: x, obj: out UnitInterval u) ? u : throw new InvalidOperationException("roundtrip lost value"));
}

public sealed class DimensionProps {
    [Fact]
    public void ClosureRejectsValuesBelowOneAndAcceptsPositiveIntegers() {
        Spec.ForAll(Gen.Int[-100, 0], static value => Assert.False(Rasm.Vectors.Dimension.TryCreate(value: value, obj: out _)));
        Spec.ForAll(Gen.Int[1, 256], static value => Assert.True(Rasm.Vectors.Dimension.TryCreate(value: value, obj: out Rasm.Vectors.Dimension d) && d.Value == value));
        Assert.False(condition: Rasm.Vectors.Dimension.TryCreate(value: default(Rasm.Vectors.Dimension).Value, obj: out _));
    }
}

public sealed class BoundarySenseLaws {
    [Fact]
    public void CatalogKeysAndSignsArePaired() =>
        Spec.Cases(items: AtomGens.Senses, key: static sense => sense.Key, law: static sense => {
            Spec.EqualWithin(left: Math.Abs(value: sense.Sign), right: 1.0, tolerance: 0.0, what: "sense unit magnitude");
            Assert.Equal(expected: sense.Key, actual: (int)sense.Sign);
        });
}

public sealed class SignedAxisLaws {
    [Fact]
    public void CardinalCatalogPreservesSignsKeysAndAxisPairs() {
        Assert.Equal(expected: 4, actual: SignedAxis.Cardinal(planar: true).Count);
        Assert.Equal(expected: 6, actual: SignedAxis.Cardinal(planar: false).Count);
        Seq<SignedAxis> full = SignedAxis.Cardinal(planar: false);
        _ = SignedAxis.Cardinal(planar: true).Iter(axis => Assert.Contains(expected: axis, collection: full));
        Spec.Cases(items: AtomGens.Axes, key: static axis => axis.Key, law: static axis => {
            Spec.EqualWithin(left: axis.World.Length, right: 1.0, tolerance: 1.0e-12, what: "axis world length");
            Spec.EqualWithin(left: axis.World * axis.World, right: 1.0, tolerance: 1.0e-12, what: "axis self dot");
            Assert.Equal(expected: Math.Sign(value: axis.Key), actual: Math.Sign(value: axis.World.X + axis.World.Y + axis.World.Z));
        });
    }
}

public sealed class VectorRelationLaws {
    [Fact]
    public void RelationKeysAreDistinctAndProjectionRejectsForeignOutput() {
        VectorRelation[] all = [VectorRelation.Oblique, VectorRelation.Parallel, VectorRelation.AntiParallel, VectorRelation.Perpendicular];
        Spec.SmartEnumKeysUnique(items: all, key: static relation => relation.Key);
        Spec.ForAll(Gen.OneOfConst(all), static relation => {
            Spec.Succ(relation.Project<VectorRelation>(key: AtomGens.Key), then: actual => Assert.Same(expected: relation, actual: actual));
            Spec.FailCategory(relation.Project<double>(key: AtomGens.Key), category: "Unsupported");
        });
    }
}
