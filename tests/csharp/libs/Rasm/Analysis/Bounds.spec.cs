using Rasm.Analysis;
using Rasm.Domain;
using Rasm.TestKit;
using Rhino.Geometry;

namespace Rasm.Tests.Analysis;

// --- [CONSTANTS] ----------------------------------------------------------------------------
// Rasm.Analysis.Bounds is bridge-heavy: every Operation EVALUATION reads live native geometry — AxisAligned/
// Center/Corners route GeometryKernel.BoundsOf; InPlane/Transformed/PrincipalFrame/Tightness route
// GetBoundingBox(xform)/MassKind.PrincipalFrameOf + native Box construction; Edges reads BoundingBox.GetEdges;
// Area/Volume/Diagonal/AspectRatio read native Box/BoundingBox metrics; EnclosingSphere/Circle/Cylinder fit via
// the private Ritter/FarthestFrom/AspectOf helpers over GeometryKernel.SamplePoints + Circle.TrySmallestEnclosingCircle.
// Those successes (and the unreachable-from-here private fitters) are owned by *.verify.csx and are NOT faked here.
// The static rail owns the pure-managed surface only: the Bounds union catalog + 15 factories transporting their
// Plane/Transform/Vector3d/bool/int payloads VERBATIM, and the operation-construction DISPATCH —
// Bounds.Operation<TGeometry,TOut>() routes on pure Type-shape predicates (typeof + GeometryKernel.CanBound/Can
// reflection over the frozen Kind catalog, zero Rhino runtime) to a built Operation (IsSupported) or
// key.Unsupported<>(). That decision table is asserted against an INDEPENDENT (geometry × output) support
// specification, and the Reject rail surfaces through Analyze.Run without touching native (Supported() fails first).
internal static class BoundsGens {
    // Plane via Plane.WorldZX (managed static — distinct from WorldXY) with an origin shift through the managed
    // Origin setter; never new Plane(origin, normal), whose native axis derivation P/Invokes outside the bridge.
    public static readonly Op Key = Op.Of(name: "bounds-test");
    public static readonly Plane Pose = new(other: Plane.WorldZX) { Origin = new Point3d(x: 3.0, y: -7.0, z: 11.0) };
    public static readonly Transform Shear = new() { M00 = 1.0, M01 = 2.0, M02 = 0.0, M03 = 5.0, M11 = 1.0, M12 = 3.0, M22 = 1.0, M33 = 1.0 };
    public static readonly Vector3d Axis = new(x: 0.3, y: -0.4, z: 0.5);
    public const int Count = 137;
}

// --- [ALGEBRAIC] ----------------------------------------------------------------------------
public sealed class BoundsUnionCatalogLaws {
    public static readonly (string Label, Bounds Aspect)[] Cases =
        [("AxisAligned", Bounds.AxisAligned), ("Oriented", Bounds.Oriented(plane: BoundsGens.Pose)),
         ("Transformed", Bounds.Transformed(transform: BoundsGens.Shear)), ("Principal", Bounds.Principal),
         ("Center", Bounds.Center), ("Corners", Bounds.Corners(unique: true)), ("Edges", Bounds.Edges),
         ("Area", Bounds.Area), ("Volume", Bounds.Volume), ("Diagonal", Bounds.Diagonal),
         ("AspectRatio", Bounds.AspectRatio), ("Tightness", Bounds.Tightness),
         ("EnclosingSphere", Bounds.EnclosingSphere(count: BoundsGens.Count)),
         ("EnclosingCircle", Bounds.EnclosingCircle(plane: BoundsGens.Pose, count: BoundsGens.Count)),
         ("EnclosingCylinder", Bounds.EnclosingCylinder(axis: BoundsGens.Axis, count: BoundsGens.Count))];
    [Fact]
    public void FifteenFactoriesProjectFifteenDistinctUnionCaseTypes() =>
        Assert.Equal(expected: 15, actual: Cases.Select(static c => c.Aspect.GetType()).Distinct().Count());
    [Fact]
    public void PayloadCarryingFactoriesTransportTheirChannelsVerbatim() {
        Spec.Equal(left: Assert.IsType<Bounds.InPlaneCase>(@object: Bounds.Oriented(plane: BoundsGens.Pose)).Plane, right: BoundsGens.Pose, what: "Oriented plane");
        Spec.Equal(left: Assert.IsType<Bounds.TransformedCase>(@object: Bounds.Transformed(transform: BoundsGens.Shear)).Xform, right: BoundsGens.Shear, what: "Transformed xform");
        Spec.Equal(left: Assert.IsType<Bounds.EnclosingCircleCase>(@object: Bounds.EnclosingCircle(plane: BoundsGens.Pose, count: BoundsGens.Count)).Plane, right: BoundsGens.Pose, what: "Circle plane");
        Spec.Equal(left: Assert.IsType<Bounds.EnclosingCylinderCase>(@object: Bounds.EnclosingCylinder(axis: BoundsGens.Axis, count: BoundsGens.Count)).Axis, right: BoundsGens.Axis, what: "Cylinder axis");
        Assert.True(condition: Assert.IsType<Bounds.CornersCase>(@object: Bounds.Corners(unique: true)).Unique);
        Assert.False(condition: Assert.IsType<Bounds.CornersCase>(@object: Bounds.Corners(unique: false)).Unique);
        Assert.Equal(expected: BoundsGens.Count, actual: Assert.IsType<Bounds.EnclosingSphereCase>(@object: Bounds.EnclosingSphere(count: BoundsGens.Count)).Count);
        Assert.Equal(expected: BoundsGens.Count, actual: Assert.IsType<Bounds.EnclosingCircleCase>(@object: Bounds.EnclosingCircle(plane: BoundsGens.Pose, count: BoundsGens.Count)).Count);
        Assert.Equal(expected: BoundsGens.Count, actual: Assert.IsType<Bounds.EnclosingCylinderCase>(@object: Bounds.EnclosingCylinder(axis: BoundsGens.Axis, count: BoundsGens.Count)).Count);
    }
    [Fact]
    public void EnclosingCountDefaultsToSixtyFourWhenOmitted() {
        Assert.Equal(expected: 64, actual: Assert.IsType<Bounds.EnclosingSphereCase>(@object: Bounds.EnclosingSphere()).Count);
        Assert.Equal(expected: 64, actual: Assert.IsType<Bounds.EnclosingCircleCase>(@object: Bounds.EnclosingCircle(plane: BoundsGens.Pose)).Count);
        Assert.Equal(expected: 64, actual: Assert.IsType<Bounds.EnclosingCylinderCase>(@object: Bounds.EnclosingCylinder(axis: BoundsGens.Axis)).Count);
        Assert.False(condition: Assert.IsType<Bounds.CornersCase>(@object: Bounds.Corners()).Unique);
    }
}

public sealed class BoundsOutputDispatchLaws {
    // INDEPENDENT (geometry × output) support specification — re-derives each case's documented TOut from a
    // closed reflection oracle (CanBound includeSphere:true / GeometryBase assignability / CanPrincipal over the
    // frozen Kind catalog) distinct from production's per-case Switch arm. A swapped TOut or geometry guard is caught.
    [Fact]
    public void SingularOutputCasesPinTheirDocumentedProjectionAndRejectForeignOutput() {
        Assert.True(condition: Bounds.AxisAligned.Operation<Mesh, BoundingBox>().IsSupported);
        Assert.True(condition: Bounds.AxisAligned.Operation<Point3d, BoundingBox>().IsSupported);
        Assert.False(condition: Bounds.AxisAligned.Operation<Plane, BoundingBox>().IsSupported);
        Assert.False(condition: Bounds.AxisAligned.Operation<Mesh, Box>().IsSupported);
        Assert.True(condition: Bounds.Oriented(plane: BoundsGens.Pose).Operation<Mesh, Box>().IsSupported);
        Assert.False(condition: Bounds.Oriented(plane: BoundsGens.Pose).Operation<BoundingBox, Box>().IsSupported);
        Assert.False(condition: Bounds.Oriented(plane: BoundsGens.Pose).Operation<Mesh, BoundingBox>().IsSupported);
        Assert.True(condition: Bounds.Transformed(transform: BoundsGens.Shear).Operation<Brep, BoundingBox>().IsSupported);
        Assert.False(condition: Bounds.Transformed(transform: BoundsGens.Shear).Operation<Point3d, BoundingBox>().IsSupported);
        Assert.True(condition: Bounds.Principal.Operation<Mesh, Box>().IsSupported);
        Assert.True(condition: Bounds.Principal.Operation<Brep, Box>().IsSupported);
        Assert.False(condition: Bounds.Principal.Operation<Point3d, Box>().IsSupported);
        Assert.False(condition: Bounds.Principal.Operation<Mesh, BoundingBox>().IsSupported);
        Assert.True(condition: Bounds.Edges.Operation<BoundingBox, Line>().IsSupported);
        Assert.False(condition: Bounds.Edges.Operation<Mesh, Line>().IsSupported);
        Assert.False(condition: Bounds.Edges.Operation<BoundingBox, Point3d>().IsSupported);
    }
    [Fact]
    public void CenterAndCornersProjectPoint3dForAnyGeometryAndRejectForeignOutput() {
        Spec.Cases(items: AnyGeometryPoint3d, key: static g => g.Label, law: static g => {
            Assert.True(condition: g.Center);
            Assert.True(condition: g.Corners);
        });
        Assert.False(condition: Bounds.Center.Operation<Mesh, Box>().IsSupported);
        Assert.False(condition: Bounds.Corners(unique: true).Operation<Mesh, double>().IsSupported);
    }
    [Fact]
    public void BoxMetricCasesSupportDoubleOnlyForBoxAndBoundingBox() =>
        Spec.Cases(items: Metrics, key: static m => m.Label, law: static m => {
            Assert.True(condition: m.Aspect.Operation<BoundingBox, double>().IsSupported);
            Assert.True(condition: m.Aspect.Operation<Box, double>().IsSupported);
            Assert.False(condition: m.Aspect.Operation<Mesh, double>().IsSupported);
            Assert.False(condition: m.Aspect.Operation<BoundingBox, Point3d>().IsSupported);
        });
    [Fact]
    public void TightnessNeedsGeometryBaseWithPrincipalTopologyAndDoubleOutput() {
        Assert.True(condition: Bounds.Tightness.Operation<Mesh, double>().IsSupported);
        Assert.True(condition: Bounds.Tightness.Operation<Brep, double>().IsSupported);
        Assert.False(condition: Bounds.Tightness.Operation<BoundingBox, double>().IsSupported);
        Assert.False(condition: Bounds.Tightness.Operation<Mesh, Box>().IsSupported);
    }
    [Fact]
    public void EnclosingShapesSupportTheirHullOutputWhenBoundableAndRejectForeignOutput() =>
        Spec.Cases(items: Enclosings, key: static e => e.Label, law: static e => {
            Assert.True(condition: e.MeshHull);
            Assert.False(condition: e.PlaneHull);
            Assert.False(condition: e.ForeignOutput);
        });
    // Center/Corners carry no geometry guard — TOut==Point3d alone supports any geometry; pre-projected per type.
    public static readonly (string Label, bool Center, bool Corners)[] AnyGeometryPoint3d =
        [("Mesh", Bounds.Center.Operation<Mesh, Point3d>().IsSupported, Bounds.Corners(unique: true).Operation<Mesh, Point3d>().IsSupported),
         ("Plane", Bounds.Center.Operation<Plane, Point3d>().IsSupported, Bounds.Corners(unique: false).Operation<Plane, Point3d>().IsSupported),
         ("Point3d", Bounds.Center.Operation<Point3d, Point3d>().IsSupported, Bounds.Corners(unique: true).Operation<Point3d, Point3d>().IsSupported)];
    public static readonly (string Label, Bounds Aspect)[] Metrics =
        [("Area", Bounds.Area), ("Volume", Bounds.Volume), ("Diagonal", Bounds.Diagonal), ("AspectRatio", Bounds.AspectRatio)];
    // Hull TOut is invariant per case (Sphere/Circle/Cylinder), so support truth is pre-projected to bool triples
    // rather than stored through a lossy Operation<_,object> cast that would re-Reject every heterogeneous arm.
    public static readonly (string Label, bool MeshHull, bool PlaneHull, bool ForeignOutput)[] Enclosings =
        [("Sphere", Bounds.EnclosingSphere(count: BoundsGens.Count).Operation<Mesh, Sphere>().IsSupported,
          Bounds.EnclosingSphere(count: BoundsGens.Count).Operation<Plane, Sphere>().IsSupported,
          Bounds.EnclosingSphere(count: BoundsGens.Count).Operation<Mesh, BoundingBox>().IsSupported),
         ("Circle", Bounds.EnclosingCircle(plane: BoundsGens.Pose, count: BoundsGens.Count).Operation<Mesh, Circle>().IsSupported,
          Bounds.EnclosingCircle(plane: BoundsGens.Pose, count: BoundsGens.Count).Operation<Plane, Circle>().IsSupported,
          Bounds.EnclosingCircle(plane: BoundsGens.Pose, count: BoundsGens.Count).Operation<Mesh, BoundingBox>().IsSupported),
         ("Cylinder", Bounds.EnclosingCylinder(axis: BoundsGens.Axis, count: BoundsGens.Count).Operation<Mesh, Cylinder>().IsSupported,
          Bounds.EnclosingCylinder(axis: BoundsGens.Axis, count: BoundsGens.Count).Operation<Plane, Cylinder>().IsSupported,
          Bounds.EnclosingCylinder(axis: BoundsGens.Axis, count: BoundsGens.Count).Operation<Mesh, BoundingBox>().IsSupported)];
}

// --- [EDGE_CASES] ---------------------------------------------------------------------------
public sealed class BoundsRejectionRailLaws {
    // Reject rail surfaces WITHOUT native: Analyze.Run fails on Supported() (Body.Rejected) before any Apply, so a
    // foreign-output operation carries Fault.Unsupported with the exact (geometry, output) type-pair; a null aspect
    // collapses to the Input category via Analyze.Bounds → Aspect's Reject(InvalidInput). Mirrors Topology's rail.
    [Fact]
    public void ForeignOutputOperationRunRejectsWithUnsupportedTypePair() =>
        Spec.Invalid(Analyze.Run(operation: Bounds.AxisAligned.Operation<Mesh, Box>(), input: default(Mesh)!),
            then: static error => {
                Fault.Unsupported fault = Assert.IsType<Fault.Unsupported>(@object: error);
                Assert.Equal(expected: typeof(Mesh), actual: fault.GeometryType);
                Assert.Equal(expected: typeof(Box), actual: fault.OutputType);
            });
    [Fact]
    public void NullAspectRejectsWithInputCategory() =>
        Spec.Invalid(Analyze.Run(operation: Analyze.Bounds<Mesh, BoundingBox>(aspect: null!), input: default(Mesh)!),
            then: static error => Assert.Equal(expected: "Input", actual: error.Category()));
}
