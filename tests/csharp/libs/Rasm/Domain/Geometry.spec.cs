using Rasm.Domain;
using Rasm.TestKit;
using Rhino.Geometry;

namespace Rasm.Tests.Domain;

// --- [MODELS] ----------------------------------------------------------------------------
// BRIDGE-DEFERRED: native GeometryKernel/coercion/closest; static owns Kind/Topology catalogs and capability lattice.
internal static class GeometryGens {
    public static readonly Op Key = Op.Of(name: "geometry-test");
    public static readonly (Rhino.DocObjects.ObjectType ObjectType, Kind Kind)[] ObjectTypeKind =
        [
            (Rhino.DocObjects.ObjectType.Point, Kind.Point), (Rhino.DocObjects.ObjectType.Curve, Kind.Curve), (Rhino.DocObjects.ObjectType.Surface, Kind.Surface), (Rhino.DocObjects.ObjectType.Brep, Kind.Brep),
            (Rhino.DocObjects.ObjectType.Mesh, Kind.Mesh), (Rhino.DocObjects.ObjectType.SubD, Kind.SubD), (Rhino.DocObjects.ObjectType.PointSet, Kind.PointCloud), (Rhino.DocObjects.ObjectType.Hatch, Kind.Hatch),
            (Rhino.DocObjects.ObjectType.Extrusion, Kind.Extrusion),
        ];
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
    public static bool VertexReadableOracle(Type type) =>
        type == typeof(Point3d) || type == typeof(Curve) || type == typeof(Line) || type == typeof(Polyline) || type == typeof(Arc)
        || type == typeof(Brep) || type == typeof(Box) || type == typeof(BoundingBox) || type == typeof(Mesh) || type == typeof(PointCloud) || type == typeof(SubD) || type == typeof(Extrusion);
    public static bool EdgeReadableOracle(Type type) =>
        type == typeof(Line) || type == typeof(Polyline) || type == typeof(BoundingBox) || type == typeof(Box) || type == typeof(Brep) || type == typeof(Mesh) || type == typeof(SubD);
}

// --- [OPERATIONS] ----------------------------------------------------------------------------
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

    [Fact]
    public void OfClimbsNativeBaseTypesAndObjectTypeMapMatchesCatalog() {
        Spec.Cases(items: new[] { typeof(PolylineCurve), typeof(NurbsCurve), typeof(ArcCurve) }, key: static t => t,
            law: static type => Spec.Some(result: Kind.Of(type: type), then: static kind => Assert.Same(expected: Kind.Curve, actual: kind)));
        Spec.Cases(items: GeometryGens.ObjectTypeKind, key: static row => row.ObjectType, law: static row =>
            Assert.Same(expected: row.Kind, actual: Kind.ByObjectType[row.ObjectType]));
        _ = Assert.Throws<ArgumentNullException>(testCode: static () => Kind.Of(type: null!));
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
            Assert.Equal(expected: GeometryGens.VertexReadableOracle(type: kind.Type), actual: kind.CanReadVertices);
            Assert.Equal(expected: GeometryGens.EdgeReadableOracle(type: kind.Type), actual: kind.CanReadEdges);
            Spec.Holds(condition: kind.CanCoerceTo(target: kind.Type), label: $"{kind.Type.Name} must coerce to itself");
        });

    [Fact]
    public void BoundExclusionsTrackPlaneAndSphereSwitch() =>
        Spec.Cases(items: Kind.Items, key: static kind => kind.Type, law: static kind => {
            Assert.Equal(expected: kind.Type != typeof(Plane), actual: kind.CanBound(includeSphere: true));
            Assert.Equal(expected: kind.Type != typeof(Plane) && kind.Type != typeof(Sphere), actual: kind.CanBound(includeSphere: false));
        });
}

public sealed class TopologyAndCurveFormCatalogLaws {
    [Fact]
    public void TopologyCatalogIsDenseAndUnique() =>
        Spec.SmartEnumCatalogMatches(production: Topology.Items, expectedKeys: [.. Enumerable.Range(start: 0, count: 10)], key: static t => t.Key);

    [Fact]
    public void CurveFormCasesRemainTypedAndAssignable() {
        Spec.Cases(items: new[] { typeof(CurveForm.LineCase), typeof(CurveForm.CircleCase), typeof(CurveForm.ArcCase), typeof(CurveForm.EllipseCase), typeof(CurveForm.NurbsCase) },
            key: static t => t, law: static t => Assert.True(condition: typeof(CurveForm).IsAssignableFrom(c: t)));
        _ = Assert.IsType<CurveForm.PolylineCase>(@object: new CurveForm.PolylineCase(Value: [], IsClosed: false));
        _ = Assert.IsType<CurveForm.NurbsCase>(@object: new CurveForm.NurbsCase(Degree: 3, IsClosed: false, IsPlanar: true, IsPeriodic: false, SpanCount: 1, Dimension: 3));
    }
}

public sealed class ClosestHitLaws {
    [Fact]
    public void ValidityRejectsEachIndependentInvalidChannel() {
        Point3d origin = Point3d.Origin;
        Point3d point = new(x: 3.0, y: 4.0, z: 0.0);
        ClosestHit valid = new(
            Point: point,
            Distance: Some(5.0),
            Parameter: Some(0.25),
            Uv: Some(new Point2d(x: 2.0, y: 3.0)),
            Normal: Some(Vector3d.ZAxis),
            Component: Some(new ComponentIndex(type: ComponentIndexType.MeshFace, index: 0)),
            MeshPoint: default,
            Tangent: Some(Vector3d.XAxis),
            Frame: default);
        Spec.Cases(
            items: [
                (Label: "valid", Hit: valid, Expected: true),
                (Label: "invalid-point", Hit: valid with { Point = new Point3d(x: double.NaN, y: 0.0, z: 0.0) }, Expected: false),
                (Label: "missing-distance", Hit: valid with { Distance = default }, Expected: false),
                (Label: "negative-distance", Hit: valid with { Distance = Some(-1.0) }, Expected: false),
                (Label: "nonfinite-distance", Hit: valid with { Distance = Some(double.PositiveInfinity) }, Expected: false),
                (Label: "nonfinite-parameter", Hit: valid with { Parameter = Some(double.NaN) }, Expected: false),
                (Label: "invalid-uv", Hit: valid with { Uv = Some(new Point2d(x: double.NaN, y: 0.0)) }, Expected: false),
                (Label: "zero-normal", Hit: valid with { Normal = Some(Vector3d.Zero) }, Expected: false),
                (Label: "invalid-component-type", Hit: valid with { Component = Some(new ComponentIndex(type: ComponentIndexType.InvalidType, index: 0)) }, Expected: false),
                (Label: "negative-component-index", Hit: valid with { Component = Some(new ComponentIndex(type: ComponentIndexType.MeshFace, index: -1)) }, Expected: false),
                (Label: "zero-tangent", Hit: valid with { Tangent = Some(Vector3d.Zero) }, Expected: false),
                (Label: "factory-distance", Hit: ClosestHit.At(target: origin, point: point), Expected: true),
            ],
            key: static row => row.Label,
            law: static row => Assert.Equal(expected: row.Expected, actual: row.Hit.IsValid));
    }
}

public sealed class GeometryKernelCapabilityLaws {
    [Fact]
    public void NullGuardsFireBeforeCapabilityPredicate() {
        _ = Assert.Throws<ArgumentNullException>(testCode: static () => GeometryKernel.Can(type: null!, predicate: static _ => true));
        _ = Assert.Throws<ArgumentNullException>(testCode: static () => GeometryKernel.Can(type: typeof(Point3d), predicate: null!));
    }

    [Fact]
    public void SupportPredicatesMatchIndependentCapabilityMatrix() =>
        Spec.Cases(
            items: [
                (Type: typeof(object), Bound: true, Surface: true, Topology: true, Closest: true, Normal: true, Tangent: true, Frame: true, Signed: true, Vertices: true, Samples: true),
                (Type: typeof(GeometryBase), Bound: true, Surface: true, Topology: true, Closest: true, Normal: true, Tangent: true, Frame: true, Signed: true, Vertices: true, Samples: true),
                (Type: typeof(string), Bound: false, Surface: false, Topology: false, Closest: false, Normal: false, Tangent: false, Frame: false, Signed: false, Vertices: false, Samples: false),
                (Type: typeof(Point3d), Bound: true, Surface: false, Topology: false, Closest: true, Normal: false, Tangent: false, Frame: false, Signed: false, Vertices: true, Samples: true),
                (Type: typeof(Point), Bound: true, Surface: false, Topology: false, Closest: true, Normal: false, Tangent: false, Frame: false, Signed: false, Vertices: true, Samples: true),
                (Type: typeof(Line), Bound: true, Surface: false, Topology: false, Closest: true, Normal: false, Tangent: true, Frame: true, Signed: false, Vertices: true, Samples: true),
                (Type: typeof(Curve), Bound: true, Surface: false, Topology: false, Closest: true, Normal: false, Tangent: true, Frame: true, Signed: false, Vertices: true, Samples: true),
                (Type: typeof(Plane), Bound: false, Surface: true, Topology: false, Closest: true, Normal: true, Tangent: false, Frame: true, Signed: true, Vertices: false, Samples: true),
                (Type: typeof(Sphere), Bound: false, Surface: true, Topology: true, Closest: true, Normal: true, Tangent: false, Frame: true, Signed: true, Vertices: false, Samples: true),
                (Type: typeof(Box), Bound: true, Surface: false, Topology: true, Closest: true, Normal: false, Tangent: false, Frame: false, Signed: true, Vertices: true, Samples: true),
                (Type: typeof(BoundingBox), Bound: true, Surface: false, Topology: true, Closest: true, Normal: false, Tangent: false, Frame: false, Signed: true, Vertices: true, Samples: true),
                (Type: typeof(Surface), Bound: true, Surface: true, Topology: true, Closest: true, Normal: true, Tangent: false, Frame: true, Signed: true, Vertices: false, Samples: true),
                (Type: typeof(Brep), Bound: true, Surface: true, Topology: true, Closest: true, Normal: true, Tangent: true, Frame: true, Signed: true, Vertices: true, Samples: true),
                (Type: typeof(BrepFace), Bound: true, Surface: true, Topology: true, Closest: true, Normal: true, Tangent: false, Frame: true, Signed: true, Vertices: false, Samples: true),
                (Type: typeof(Mesh), Bound: true, Surface: false, Topology: true, Closest: true, Normal: true, Tangent: false, Frame: true, Signed: true, Vertices: true, Samples: true),
                (Type: typeof(PointCloud), Bound: true, Surface: false, Topology: false, Closest: true, Normal: true, Tangent: false, Frame: false, Signed: true, Vertices: true, Samples: true),
                (Type: typeof(SubD), Bound: true, Surface: false, Topology: true, Closest: false, Normal: false, Tangent: false, Frame: false, Signed: false, Vertices: true, Samples: true),
                (Type: typeof(Extrusion), Bound: true, Surface: true, Topology: true, Closest: true, Normal: true, Tangent: false, Frame: true, Signed: true, Vertices: true, Samples: true),
            ],
            key: static row => row.Type,
            law: static row => {
                Assert.Equal(expected: row.Bound, actual: GeometryKernel.CanBound(source: row.Type, includeSphere: false));
                Assert.Equal(expected: row.Surface, actual: GeometryKernel.CanSurfaceForm(type: row.Type));
                Assert.Equal(expected: row.Topology, actual: GeometryKernel.CanEvaluateTopology(type: row.Type));
                Assert.Equal(expected: row.Topology, actual: GeometryKernel.CanEvaluateSolidTopology(type: row.Type));
                Assert.Equal(expected: row.Closest, actual: GeometryKernel.CanClosest(type: row.Type));
                Assert.Equal(expected: row.Normal, actual: GeometryKernel.CanClosestNormal(type: row.Type));
                Assert.Equal(expected: row.Tangent, actual: GeometryKernel.CanClosestTangent(type: row.Type));
                Assert.Equal(expected: row.Frame, actual: GeometryKernel.CanClosestFrame(type: row.Type));
                Assert.Equal(expected: row.Signed, actual: GeometryKernel.CanSignedDistance(type: row.Type));
                Assert.Equal(expected: row.Vertices, actual: GeometryKernel.CanReadVertices(type: row.Type));
                Assert.Equal(expected: row.Samples, actual: GeometryKernel.CanSamplePoints(type: row.Type));
            });
}
