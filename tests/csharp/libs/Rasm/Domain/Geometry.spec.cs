using Rasm.Domain;
using Rasm.TestKit;
using Rhino.Geometry;

namespace Rasm.Tests.Domain;

// --- [CONSTANTS] ----------------------------------------------------------------------------
// BRIDGE-DEFERRED: native GeometryKernel/coercion/closest; static owns Kind/Topology catalogs, capability lattice, IntersectionHit construction.
internal static class GeometryGens {
    public static readonly Op Key = Op.Of(name: "geometry-test");
    // Distinct per-channel values so a Start<->End or OverlapA<->OverlapB swap is observable.
    public static readonly Point3d Start = new(x: 2.0, y: 3.0, z: 5.0), End = new(x: 7.0, y: 11.0, z: 13.0);
    public static readonly Interval OverlapA = new(t0: 17.0, t1: 19.0), OverlapB = new(t0: 23.0, t1: 29.0);
    public static readonly Gen<IntersectionHit> PointHit = Gens.Point.Where(static p => p.IsValid).Select(
        Gen.OneOfConst(IntersectionTangency.Transversal, IntersectionTangency.Tangent, IntersectionTangency.Unknown),
        static (Point3d p, IntersectionTangency t) => IntersectionHit.At(point: p, tangency: t));
    public static readonly IntersectionHit Overlap = IntersectionHit.Overlap(start: Start, end: End, overlapA: OverlapA, overlapB: OverlapB);
    // Independent oracle: re-derives the assignable + Brep/Curve/Surface lattice from intent, not Kind.CanCoerceTo's body.
    public static bool CoerceOracle(Kind kind, Type target) {
        bool brepSource = kind.Type == typeof(Brep) || kind.Type == typeof(Surface) || kind.Type == typeof(Box) || kind.Type == typeof(BoundingBox)
            || kind.Type == typeof(Sphere) || kind.Type == typeof(Cylinder) || kind.Type == typeof(Cone) || kind.Type == typeof(Torus) || kind.Type == typeof(Extrusion) || kind.Type == typeof(SubD);
        bool curvePrim = target == typeof(Line) || target == typeof(Circle) || target == typeof(Arc) || target == typeof(Ellipse) || target == typeof(Polyline);
        bool surfacePrim = target == typeof(Plane) || target == typeof(Sphere) || target == typeof(Cylinder) || target == typeof(Cone) || target == typeof(Torus);
        return target.IsAssignableFrom(kind.Type)
            || (target == typeof(Box) && kind.Type == typeof(Brep))
            || (target == typeof(Curve) && kind.Topology == Topology.Curve)
            || (curvePrim && kind.Type == typeof(Curve))
            || (surfacePrim && (kind.Type == typeof(Brep) || kind.Type == typeof(Surface)))
            || (target == typeof(Brep) && brepSource);
    }
}

// --- [ALGEBRAIC] ----------------------------------------------------------------------------
public sealed class KindCatalogLaws {
    [Fact]
    public void CatalogKeysAreUniqueAndDenselyCoverZeroToTwenty() {
        Spec.SmartEnumKeysUnique(items: Kind.Items, key: static kind => kind.Key);
        Assert.Equal(expected: 21, actual: Kind.Items.Count);
        Spec.SmartEnumCatalogMatches(production: Kind.Items, expectedKeys: [.. Enumerable.Range(start: 0, count: 21)], key: static kind => kind.Key);
    }

    [Fact]
    public void EveryCasePairsClrTypeWithTopologyAndRoundtripsThroughOf() =>
        Spec.Cases(items: Kind.Items, key: static kind => kind.Type, law: static kind => {
            Spec.Some(result: Kind.Of(type: kind.Type), then: resolved => Assert.Same(expected: kind, actual: resolved));
            Assert.Same(expected: ExpectedTopology(kind: kind), actual: kind.Topology);
        });

    private static Topology ExpectedTopology(Kind kind) =>
        kind.Type == typeof(Point3d) ? Topology.Point
        : kind.Type == typeof(Line) || kind.Type == typeof(Polyline) || kind.Type == typeof(Circle) || kind.Type == typeof(Arc) || kind.Type == typeof(Ellipse) || kind.Type == typeof(Curve) ? Topology.Curve
        : kind.Type == typeof(Surface) || kind.Type == typeof(Plane) || kind.Type == typeof(Sphere) || kind.Type == typeof(Cylinder) || kind.Type == typeof(Cone) || kind.Type == typeof(Torus) ? Topology.Surface
        : kind.Type == typeof(Brep) || kind.Type == typeof(Box) || kind.Type == typeof(BoundingBox) ? Topology.Brep
        : kind.Type == typeof(Mesh) ? Topology.Mesh
        : kind.Type == typeof(SubD) ? Topology.SubD
        : kind.Type == typeof(PointCloud) ? Topology.PointCloud
        : kind.Type == typeof(Extrusion) ? Topology.Extrusion
        : Topology.Hatch;
}

public sealed class KindFactoryLaws {
    [Fact]
    public void OfMapsPointAliasAndRejectsUnknownTypes() {
        Spec.Some(result: Kind.Of(type: typeof(Point)), then: static kind => Assert.Same(expected: Kind.Point, actual: kind));
        Spec.Some(result: Kind.Of(type: typeof(BoundingBox)), then: static kind => Assert.Same(expected: Kind.BoundingBox, actual: kind));
        Spec.Cases(items: new[] { typeof(string), typeof(int), typeof(object), typeof(Guid) }, key: static t => t,
            law: static t => Spec.None(result: Kind.Of(type: t)));
    }
}

public sealed class KindCapabilityLaws {
    [Fact]
    public void CoercionAgreesWithDocumentedLatticeAcrossEveryTargetPair() =>
        Spec.Cases(items: Kind.Items, key: static kind => kind.Type, law: static kind =>
            _ = Kind.Items.AsIterable().Iter(target => Assert.Equal(
                expected: GeometryGens.CoerceOracle(kind: kind, target: target.Type),
                actual: kind.CanCoerceTo(target: target.Type))));

    [Fact]
    public void TopologyDrivenReadCapabilitiesPartitionTheCatalog() =>
        Spec.Cases(items: Kind.Items, key: static kind => kind.Type, law: static kind => {
            Assert.Equal(expected: kind.Topology == Topology.Curve || kind.Topology == Topology.Surface || kind.Topology == Topology.Brep, actual: kind.CanReadControlPoints);
            Assert.Equal(expected: kind.Topology == Topology.Brep || kind.Topology == Topology.Mesh || kind.Topology == Topology.Surface || kind.Topology == Topology.Curve || kind.Topology == Topology.Extrusion, actual: kind.CanPrincipal);
            Spec.Holds(condition: kind.CanCoerceTo(target: kind.Type), label: $"{kind.Type.Name} must coerce to itself");
        });

    [Fact]
    public void BoundExclusionsTrackPlaneAndSphereSwitch() =>
        Spec.Cases(items: Kind.Items, key: static kind => kind.Type, law: static kind => {
            Assert.Equal(expected: kind.Type != typeof(Plane), actual: kind.CanBound(includeSphere: true));
            Assert.Equal(expected: kind.Type != typeof(Plane) && kind.Type != typeof(Sphere), actual: kind.CanBound(includeSphere: false));
        });
}

public sealed class TopologyAndIntersectionCatalogLaws {
    [Fact]
    public void TopologyCatalogIsDenseAndUnique() =>
        Spec.SmartEnumCatalogMatches(production: Topology.Items, expectedKeys: [.. Enumerable.Range(start: 0, count: 10)], key: static t => t.Key);

    [Fact]
    public void IntersectionKindCatalogIsDenseAndUnique() {
        Spec.SmartEnumCatalogMatches(production: IntersectionKind.Items, expectedKeys: [0, 1, 2, 3], key: static k => k.Key);
        Spec.Cases(items: new[] { typeof(CurveForm.LineCase), typeof(CurveForm.CircleCase), typeof(CurveForm.ArcCase), typeof(CurveForm.EllipseCase), typeof(CurveForm.NurbsCase) },
            key: static t => t, law: static t => Assert.True(condition: typeof(CurveForm).IsAssignableFrom(c: t)));
        _ = Assert.IsType<CurveForm.PolylineCase>(@object: new CurveForm.PolylineCase(Value: [], IsClosed: false));
        _ = Assert.IsType<CurveForm.NurbsCase>(@object: new CurveForm.NurbsCase(Degree: 3, IsClosed: false, IsPlanar: true, IsPeriodic: false, SpanCount: 1, Dimension: 3));
    }
}

public sealed class IntersectionHitLaws {
    [Fact]
    public void PointCaseProjectionOwnsPointKindTangencyAndRejectsForeignChannels() =>
        Spec.ForAll(GeometryGens.PointHit, static hit => {
            Seq<IntersectionHit> hits = Seq(hit);
            IntersectionHit.PointCase point = Assert.IsType<IntersectionHit.PointCase>(@object: hit);
            Assert.True(condition: hit.IsValid);
            Assert.Same(expected: IntersectionKind.Point, actual: hit.Kind);
            Assert.Equal(expected: 1, actual: hit.Points.Count);
            Spec.Holds(condition: hit.Curves.IsEmpty && hit.Intervals.IsEmpty, label: "point hit carries no curves or intervals");
            Spec.Succ(result: IntersectionHit.Project<Point3d>(hits: Seq(hit), key: GeometryGens.Key), then: pts => Spec.Equal(left: pts[index: 0], right: point.Point, what: "projected point"));
            Spec.Succ(result: IntersectionHit.Project<IntersectionKind>(hits: Seq(hit), key: GeometryGens.Key), then: kinds => Assert.Same(expected: IntersectionKind.Point, actual: kinds[index: 0]));
            Spec.Succ(result: IntersectionHit.Project<IntersectionTangency>(hits: Seq(hit), key: GeometryGens.Key), then: ts => Assert.Equal(expected: point.Tangency, actual: ts[index: 0]));
            Spec.Succ(result: IntersectionHit.Project<Curve>(hits: Seq(hit), key: GeometryGens.Key), then: curves => Spec.Holds(condition: curves.IsEmpty, label: "point hit projects no curves"));
            Spec.FailUnsupportedFor(result: IntersectionHit.Project<Plane>(hits: hits, key: GeometryGens.Key), geometryType: typeof(IntersectionHit), outputType: typeof(Plane));
            Spec.FailCategory(result: IntersectionHit.Project<double>(hits: Seq(hit), key: GeometryGens.Key), category: "Unsupported");
        });

    [Fact]
    public void OverlapCaseTransportsDistinctEndpointsAndIntervalsWithoutChannelSwap() {
        IntersectionHit overlap = GeometryGens.Overlap;
        Seq<IntersectionHit> hits = Seq(overlap);
        Spec.Holds(condition: overlap.IsValid, label: "distinct-channel overlap is valid");
        Assert.Same(expected: IntersectionKind.Overlap, actual: overlap.Kind);
        Assert.Equal(expected: 2, actual: overlap.Points.Count);
        Assert.Equal(expected: GeometryGens.Start, actual: overlap.Points[index: 0]);
        Assert.Equal(expected: GeometryGens.End, actual: overlap.Points[index: 1]);
        Assert.Equal(expected: GeometryGens.OverlapA, actual: overlap.Intervals[index: 0]);
        Assert.Equal(expected: GeometryGens.OverlapB, actual: overlap.Intervals[index: 1]);
        Spec.Succ(result: IntersectionHit.Project<Interval>(hits: Seq(overlap), key: GeometryGens.Key), then: ivs => {
            Assert.Equal(expected: 2, actual: ivs.Count);
            Assert.Equal(expected: GeometryGens.OverlapA, actual: ivs[index: 0]);
            Assert.Equal(expected: GeometryGens.OverlapB, actual: ivs[index: 1]);
        });
        Spec.Succ(result: IntersectionHit.Project<Point3d>(hits: Seq(overlap), key: GeometryGens.Key), then: pts => Assert.Equal(expected: Seq(GeometryGens.Start, GeometryGens.End), actual: pts));
        Spec.Succ(result: IntersectionHit.Project<IntersectionTangency>(hits: Seq(overlap), key: GeometryGens.Key), then: ts => Assert.Equal(expected: IntersectionTangency.Unknown, actual: ts[index: 0]));
        Spec.FailUnsupportedFor(result: IntersectionHit.Project<double>(hits: hits, key: GeometryGens.Key), geometryType: typeof(IntersectionHit), outputType: typeof(double));
    }

    [Fact]
    public void InvalidEndpointsBreakValidity() {
        IntersectionHit invalid = IntersectionHit.Overlap(start: new Point3d(x: double.NaN, y: 0.0, z: 0.0), end: GeometryGens.End, overlapA: GeometryGens.OverlapA, overlapB: GeometryGens.OverlapB);
        Assert.False(condition: invalid.IsValid);
        Assert.False(condition: IntersectionHit.At(point: new Point3d(x: 0.0, y: double.PositiveInfinity, z: 0.0)).IsValid);
        Spec.FailCategory(result: IntersectionHit.Project<IntersectionKind>(hits: Seq(invalid), key: GeometryGens.Key), category: "Result");
        Spec.FailCategory(result: IntersectionHit.Project<IntersectionTangency>(hits: Seq(invalid), key: GeometryGens.Key), category: "Result");
    }
}
