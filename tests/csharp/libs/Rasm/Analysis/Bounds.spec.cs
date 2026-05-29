using Rasm.Analysis;
using Rasm.Domain;
using Rasm.TestKit;
using Rhino.Geometry;

namespace Rasm.Tests.Analysis;

// --- [CONSTANTS] ----------------------------------------------------------------------------
// BRIDGE-DEFERRED (*.verify.csx): every Bounds Operation EVALUATION reads live native geometry (Ritter/Welzl fits,
// metrics, PrincipalFrameOf). Static rail owns: the Bounds union catalog + 15 payload-transporting factories and
// Operation<TGeom,TOut> dispatch (pure typeof + CanBound/CanPrincipal reflection); Run rejects at Supported() pre-Apply.
internal static class BoundsGens {
    // Copy a World basis + managed Origin setter; new Plane(origin, normal) axis derivation P/Invokes.
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
        Spec.ForAll(Gens.ManagedPlane, static plane => {
            Spec.Equal(left: Assert.IsType<Bounds.InPlaneCase>(@object: Bounds.Oriented(plane: plane)).Plane, right: plane, what: "Oriented plane");
            Spec.Equal(left: Assert.IsType<Bounds.EnclosingCircleCase>(@object: Bounds.EnclosingCircle(plane: plane, count: BoundsGens.Count)).Plane, right: plane, what: "Circle plane");
        });
        Spec.Equal(left: Assert.IsType<Bounds.TransformedCase>(@object: Bounds.Transformed(transform: BoundsGens.Shear)).Xform, right: BoundsGens.Shear, what: "Transformed xform");
        Spec.Equal(left: Assert.IsType<Bounds.EnclosingCylinderCase>(@object: Bounds.EnclosingCylinder(axis: BoundsGens.Axis, count: BoundsGens.Count)).Axis, right: BoundsGens.Axis, what: "Cylinder axis");
        Assert.True(condition: Assert.IsType<Bounds.CornersCase>(@object: Bounds.Corners(unique: true)).Unique);
        Assert.False(condition: Assert.IsType<Bounds.CornersCase>(@object: Bounds.Corners(unique: false)).Unique);
        Assert.Equal(expected: BoundsGens.Count, actual: Assert.IsType<Bounds.EnclosingSphereCase>(@object: Bounds.EnclosingSphere(count: BoundsGens.Count)).Count);
        Assert.Equal(expected: BoundsGens.Count, actual: Assert.IsType<Bounds.EnclosingCylinderCase>(@object: Bounds.EnclosingCylinder(axis: BoundsGens.Axis, count: BoundsGens.Count)).Count);
    }
    [Fact]
    public void EnclosingCountDefaultsToSixtyFourWhenOmitted() =>
        Assert.Multiple(
            () => Assert.Equal(expected: 64, actual: Assert.IsType<Bounds.EnclosingSphereCase>(@object: Bounds.EnclosingSphere()).Count),
            () => Assert.Equal(expected: 64, actual: Assert.IsType<Bounds.EnclosingCircleCase>(@object: Bounds.EnclosingCircle(plane: BoundsGens.Pose)).Count),
            () => Assert.Equal(expected: 64, actual: Assert.IsType<Bounds.EnclosingCylinderCase>(@object: Bounds.EnclosingCylinder(axis: BoundsGens.Axis)).Count),
            () => Assert.False(condition: Assert.IsType<Bounds.CornersCase>(@object: Bounds.Corners()).Unique));
}

public sealed class BoundsOutputDispatchLaws {
    // INDEPENDENT support oracle: re-derives each case's documented TOut via closed reflection (CanBound /
    // GeometryBase assignability / CanPrincipal), distinct from production's Switch arm; catches a swapped TOut or guard.
    [Fact]
    public void SingularOutputCasesPinTheirDocumentedProjectionAndRejectForeignOutput() =>
        Spec.SupportMatrix(
            ("AxisAligned Mesh→BoundingBox", static () => Bounds.AxisAligned.Operation<Mesh, BoundingBox>().IsSupported, true),
            ("AxisAligned Point3d→BoundingBox", static () => Bounds.AxisAligned.Operation<Point3d, BoundingBox>().IsSupported, true),
            ("AxisAligned Plane→BoundingBox", static () => Bounds.AxisAligned.Operation<Plane, BoundingBox>().IsSupported, false),
            ("AxisAligned Mesh→Box", static () => Bounds.AxisAligned.Operation<Mesh, Box>().IsSupported, false),
            ("Oriented Mesh→Box", static () => Bounds.Oriented(plane: BoundsGens.Pose).Operation<Mesh, Box>().IsSupported, true),
            ("Oriented BoundingBox→Box", static () => Bounds.Oriented(plane: BoundsGens.Pose).Operation<BoundingBox, Box>().IsSupported, false),
            ("Oriented Mesh→BoundingBox", static () => Bounds.Oriented(plane: BoundsGens.Pose).Operation<Mesh, BoundingBox>().IsSupported, false),
            ("Transformed Brep→BoundingBox", static () => Bounds.Transformed(transform: BoundsGens.Shear).Operation<Brep, BoundingBox>().IsSupported, true),
            ("Transformed Point3d→BoundingBox", static () => Bounds.Transformed(transform: BoundsGens.Shear).Operation<Point3d, BoundingBox>().IsSupported, false),
            ("Principal Mesh→Box", static () => Bounds.Principal.Operation<Mesh, Box>().IsSupported, true),
            ("Principal Brep→Box", static () => Bounds.Principal.Operation<Brep, Box>().IsSupported, true),
            ("Principal Point3d→Box", static () => Bounds.Principal.Operation<Point3d, Box>().IsSupported, false),
            ("Principal Mesh→BoundingBox", static () => Bounds.Principal.Operation<Mesh, BoundingBox>().IsSupported, false),
            ("Edges BoundingBox→Line", static () => Bounds.Edges.Operation<BoundingBox, Line>().IsSupported, true),
            ("Edges Mesh→Line", static () => Bounds.Edges.Operation<Mesh, Line>().IsSupported, false),
            ("Edges BoundingBox→Point3d", static () => Bounds.Edges.Operation<BoundingBox, Point3d>().IsSupported, false));
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
    public void TightnessNeedsGeometryBaseWithPrincipalTopologyAndDoubleOutput() =>
        Spec.SupportMatrix(
            ("Tightness Mesh→double", static () => Bounds.Tightness.Operation<Mesh, double>().IsSupported, true),
            ("Tightness Brep→double", static () => Bounds.Tightness.Operation<Brep, double>().IsSupported, true),
            ("Tightness BoundingBox→double (no principal topology)", static () => Bounds.Tightness.Operation<BoundingBox, double>().IsSupported, false),
            ("Tightness Mesh→Box (foreign output)", static () => Bounds.Tightness.Operation<Mesh, Box>().IsSupported, false));
    [Fact]
    public void EnclosingShapesSupportTheirHullOutputWhenBoundableAndRejectForeignOutput() =>
        Spec.Cases(items: Enclosings, key: static e => e.Label, law: static e => {
            Assert.True(condition: e.MeshHull);
            Assert.False(condition: e.PlaneHull);
            Assert.False(condition: e.ForeignOutput);
        });
    // Center/Corners carry no geometry guard — TOut==Point3d alone suffices.
    public static readonly (string Label, bool Center, bool Corners)[] AnyGeometryPoint3d =
        [("Mesh", Bounds.Center.Operation<Mesh, Point3d>().IsSupported, Bounds.Corners(unique: true).Operation<Mesh, Point3d>().IsSupported),
         ("Plane", Bounds.Center.Operation<Plane, Point3d>().IsSupported, Bounds.Corners(unique: false).Operation<Plane, Point3d>().IsSupported),
         ("Point3d", Bounds.Center.Operation<Point3d, Point3d>().IsSupported, Bounds.Corners(unique: true).Operation<Point3d, Point3d>().IsSupported)];
    public static readonly (string Label, Bounds Aspect)[] Metrics =
        [("Area", Bounds.Area), ("Volume", Bounds.Volume), ("Diagonal", Bounds.Diagonal), ("AspectRatio", Bounds.AspectRatio)];
    // Hull TOut is invariant per case, so pre-project to bool triples; a lossy Operation<_,object> cast would re-Reject every arm.
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
    // Reject rail surfaces WITHOUT native: Run fails at Supported() pre-Apply. Foreign output → Fault.Unsupported with
    // the exact (geometry, output) pair; null aspect → Input category via Aspect's Reject(InvalidInput).
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
