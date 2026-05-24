using Rasm.Domain;
using Rasm.TestKit;
using Rasm.Vectors;
using Rhino.Geometry;

namespace Rasm.Tests.Vectors;

// --- [CONSTANTS] ----------------------------------------------------------------------------
internal static class SpaceGens {
    public static readonly Op Key = Op.Of(name: "space-test");
    public static readonly Context Model = Spec.SuccValue(Context.Of(absolute: 0.001, relative: 1.0e-8, angle: 0.01, units: Rhino.UnitSystem.Millimeters).ToFin(), label: "space context");
    public static readonly SupportProjection[] All = [
        SupportProjection.Closest, SupportProjection.Direction, SupportProjection.Span, SupportProjection.SignedSpanAway,
        SupportProjection.Normal, SupportProjection.Distance, SupportProjection.Parameter, SupportProjection.Uv,
        SupportProjection.Component, SupportProjection.MeshPoint, SupportProjection.SignedDistance,
        SupportProjection.ContainmentDistance, SupportProjection.Tangent, SupportProjection.Frame,
    ];
    public static readonly Gen<SupportProjection> Projection = Gen.OneOfConst(All);
    public static readonly SupportSpace PointSpace = Spec.SuccValue(SupportSpace.Of(value: new Point3d(x: 1.0, y: 0.0, z: 0.0), key: Key), label: "point space");
    public static ClosestHit Hit(Option<double> parameter = default, Option<Point2d> uv = default, Option<ComponentIndex> component = default, Option<Point3d> point = default) =>
        ClosestHit.At(target: new Point3d(x: 10.0, y: 0.0, z: 0.0), point: point.IfNone(new Point3d(x: 1.0, y: 0.0, z: 0.0)), parameter: parameter, uv: uv, component: component);
}

// --- [ALGEBRAIC] ----------------------------------------------------------------------------
// Brep/Mesh containment, surface sampling, and span vector materialization project through
// Rhino native geometry and belong in bridge scenarios. The static rail owns typed admission
// and option-gated failures that return before native dispatch.
public sealed class SupportProjectionLaws {
    [Fact]
    public void KeysAreDistinctAndGeneratorEmitsDeclaredCases() {
        Spec.SmartEnumKeysUnique(items: SpaceGens.All, key: static projection => projection.Key);
        Spec.ForAll(SpaceGens.Projection, projection => Assert.Contains(expected: projection, collection: SpaceGens.All));
    }
    [Fact]
    public void ClosestDistanceParameterUvAndComponentOwnOptionGatedOutputs() {
        ClosestHit rich = SpaceGens.Hit(parameter: Some(0.25), uv: Some(new Point2d(x: 2.0, y: 3.0)), component: Some(new ComponentIndex(type: ComponentIndexType.PointCloudPoint, index: 4)));
        Spec.Succ(SupportProjection.Closest.Project<Point3d>(space: SpaceGens.PointSpace, hit: rich, sample: Point3d.Origin, context: SpaceGens.Model, key: SpaceGens.Key),
            then: point => Spec.NearEqual(left: point, right: rich.Point));
        Spec.Succ(SupportProjection.Distance.Project<double>(space: SpaceGens.PointSpace, hit: rich, sample: Point3d.Origin, context: SpaceGens.Model, key: SpaceGens.Key),
            then: distance => Spec.EqualWithin(left: distance, right: 9.0, tolerance: 1.0e-12, what: "distance"));
        Spec.Succ(SupportProjection.Parameter.Project<double>(space: SpaceGens.PointSpace, hit: rich, sample: Point3d.Origin, context: SpaceGens.Model, key: SpaceGens.Key),
            then: parameter => Spec.EqualWithin(left: parameter, right: 0.25, tolerance: 0.0, what: "parameter"));
        Spec.Succ(SupportProjection.Uv.Project<Point2d>(space: SpaceGens.PointSpace, hit: rich, sample: Point3d.Origin, context: SpaceGens.Model, key: SpaceGens.Key),
            then: uv => Assert.Equal(expected: new Point2d(x: 2.0, y: 3.0), actual: uv));
        Spec.Succ(SupportProjection.Component.Project<ComponentIndex>(space: SpaceGens.PointSpace, hit: rich, sample: Point3d.Origin, context: SpaceGens.Model, key: SpaceGens.Key),
            then: component => {
                Assert.Equal(expected: ComponentIndexType.PointCloudPoint, actual: component.ComponentIndexType);
                Assert.Equal(expected: 4, actual: component.Index);
            });
        ClosestHit sparse = SpaceGens.Hit();
        Spec.FailCategory(SupportProjection.Parameter.Project<double>(space: SpaceGens.PointSpace, hit: sparse, sample: Point3d.Origin, context: SpaceGens.Model, key: SpaceGens.Key), category: "Unsupported");
        Spec.FailCategory(SupportProjection.Uv.Project<Point2d>(space: SpaceGens.PointSpace, hit: sparse, sample: Point3d.Origin, context: SpaceGens.Model, key: SpaceGens.Key), category: "Unsupported");
        Spec.FailCategory(SupportProjection.Component.Project<ComponentIndex>(space: SpaceGens.PointSpace, hit: sparse, sample: Point3d.Origin, context: SpaceGens.Model, key: SpaceGens.Key), category: "Unsupported");
    }
    [Fact]
    public void SpanOwnershipAndUnsupportedOutputAreExplicit() {
        ClosestHit hit = SpaceGens.Hit();
        Assert.True(condition: SupportProjection.Span.CanProjectVector(space: SpaceGens.PointSpace));
        Assert.True(condition: SupportProjection.SignedSpanAway.CanProjectVector(space: SpaceGens.PointSpace));
        ClosestHit zero = SpaceGens.Hit(point: Some(Point3d.Origin));
        ClosestHit nonzero = SpaceGens.Hit(point: Some(new Point3d(x: 3.0, y: 4.0, z: 0.0)));
        Spec.Succ(SupportProjection.Span.Project<Vector3d>(space: SpaceGens.PointSpace, hit: zero, sample: Point3d.Origin, context: SpaceGens.Model, key: SpaceGens.Key),
            then: vector => Spec.NearEqual(left: vector, right: Vector3d.Zero, tolerance: 0.0));
        Spec.Succ(SupportProjection.Span.Project<double>(space: SpaceGens.PointSpace, hit: zero, sample: Point3d.Origin, context: SpaceGens.Model, key: SpaceGens.Key),
            then: distance => Spec.EqualWithin(left: distance, right: 0.0, tolerance: 0.0, what: "zero span distance"));
        Spec.Succ(SupportProjection.Span.Project<Vector3d>(space: SpaceGens.PointSpace, hit: nonzero, sample: Point3d.Origin, context: SpaceGens.Model, key: SpaceGens.Key),
            then: vector => Spec.NearEqual(left: vector, right: new Vector3d(x: 3.0, y: 4.0, z: 0.0), tolerance: 0.0));
        Spec.Succ(SupportProjection.SignedSpanAway.Project<Vector3d>(space: SpaceGens.PointSpace, hit: nonzero, sample: Point3d.Origin, context: SpaceGens.Model, key: SpaceGens.Key),
            then: vector => Spec.NearEqual(left: vector, right: new Vector3d(x: -3.0, y: -4.0, z: 0.0), tolerance: 0.0));
        Spec.Succ(SupportProjection.SignedSpanAway.Project<double>(space: SpaceGens.PointSpace, hit: nonzero, sample: Point3d.Origin, context: SpaceGens.Model, key: SpaceGens.Key),
            then: distance => Spec.EqualWithin(left: distance, right: 5.0, tolerance: 0.0, what: "away span distance"));
        Spec.FailCategory(SupportProjection.Span.Project<VectorFrame>(space: SpaceGens.PointSpace, hit: hit, sample: Point3d.Origin, context: SpaceGens.Model, key: SpaceGens.Key), category: "Unsupported");
        Spec.FailCategory(SupportProjection.Closest.Project<VectorFrame>(space: SpaceGens.PointSpace, hit: hit, sample: Point3d.Origin, context: SpaceGens.Model, key: SpaceGens.Key), category: "Unsupported");
        Spec.FailCategory(SupportSpace.Of(value: new object(), key: SpaceGens.Key), category: "Unsupported");
    }
    [Fact]
    public void SurfaceSpaceAdmissionFailsBeforeProjectionDispatch() =>
        Spec.FailCategory(SurfaceSpace.Of(native: null!, context: SpaceGens.Model, key: SpaceGens.Key), category: "Input");
}
