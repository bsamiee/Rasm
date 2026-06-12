using Rasm.Analysis;
using Rasm.Domain;
using Rasm.TestKit;
using Rhino.Geometry;

namespace Rasm.Tests.Analysis;

// --- [MODELS] ----------------------------------------------------------------------------
// BRIDGE-DEFERRED: native Brep face decomposition, centroid, frame, and domain reads; static owns Faces union and dispatch support.
internal static class FacesGens {
    public static readonly Vector3d Axis = new(x: -2.0, y: 3.0, z: 5.0);
    public static readonly (string Label, Faces Aspect)[] Cases =
        [("All", Faces.All), ("Top", Faces.Top(axis: Axis)), ("Bottom", Faces.Bottom(axis: Axis)), ("At", Faces.At(index: 4))];
}

// --- [OPERATIONS] ----------------------------------------------------------------------------
public sealed class FacesUnionCatalogLaws {
    [Fact]
    public void FactoriesProjectBoundedCasesAndCarryPayloads() {
        Assert.Equal(expected: 3, actual: FacesGens.Cases.Select(static c => c.Aspect.GetType()).Distinct().Count());
        _ = Assert.IsType<Faces.AllCase>(@object: Faces.All);
        Faces.RankedCase top = Assert.IsType<Faces.RankedCase>(@object: Faces.Top(axis: FacesGens.Axis));
        Assert.Equal(expected: ExtremumDirection.Maximum, actual: top.Direction);
        Assert.Equal(expected: FacesGens.Axis, actual: top.Axis);
        Faces.RankedCase bottom = Assert.IsType<Faces.RankedCase>(@object: Faces.Bottom(axis: FacesGens.Axis));
        Assert.Equal(expected: ExtremumDirection.Minimum, actual: bottom.Direction);
        Assert.Equal(expected: FacesGens.Axis, actual: bottom.Axis);
        Assert.Equal(expected: 13, actual: Assert.IsType<Faces.AtCase>(@object: Faces.At(index: 13)).Value);
        Assert.Null(@object: Assert.IsType<Faces.AtCase>(@object: Faces.At()).Value);
    }
}

public sealed class FacesDispatchLaws {
    [Fact]
    public void DecomposableFaceGeometrySupportsDeclaredOutputsAndRejectsForeignOutput() =>
        Spec.SupportMatrix(
            ("All Brep→Brep", static () => Faces.All.Operation<Brep, Brep>().IsSupported, true),
            ("All Brep→TopologyProjection", static () => Faces.All.Operation<Brep, TopologyProjection>().IsSupported, true),
            ("All Brep→Plane", static () => Faces.All.Operation<Brep, Plane>().IsSupported, true),
            ("All Brep→Point3d", static () => Faces.All.Operation<Brep, Point3d>().IsSupported, true),
            ("All Brep→Vector3d", static () => Faces.All.Operation<Brep, Vector3d>().IsSupported, true),
            ("All Brep→int", static () => Faces.All.Operation<Brep, int>().IsSupported, true),
            ("All Brep→ComponentIndex", static () => Faces.All.Operation<Brep, ComponentIndex>().IsSupported, true),
            ("All Brep→Interval", static () => Faces.All.Operation<Brep, Interval>().IsSupported, true),
            ("All Brep→Curve", static () => Faces.All.Operation<Brep, Curve>().IsSupported, false));
    [Fact]
    public void NonFaceGeometryAndForeignRanksCollapseToUnsupported() =>
        Spec.SupportMatrix(
            ("Top BrepFace→Point3d", static () => Faces.Top(axis: FacesGens.Axis).Operation<BrepFace, Point3d>().IsSupported, true),
            ("Bottom Brep→Vector3d", static () => Faces.Bottom(axis: FacesGens.Axis).Operation<Brep, Vector3d>().IsSupported, true),
            ("At Brep→ComponentIndex", static () => Faces.At(index: 0).Operation<Brep, ComponentIndex>().IsSupported, true),
            ("All Mesh→Brep", static () => Faces.All.Operation<Mesh, Brep>().IsSupported, false),
            ("All Curve→Brep", static () => Faces.All.Operation<Curve, Brep>().IsSupported, false),
            ("Top Brep→double", static () => Faces.Top(axis: FacesGens.Axis).Operation<Brep, double>().IsSupported, false));
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public sealed class FacesRejectionRailLaws {
    [Fact]
    public void NullAspectAndUnsupportedOutputRejectBeforeNativeEvaluation() {
        Spec.Invalid(Analyze.Run(operation: Analyze.Query<Brep, Brep>(query: null), input: default(Brep)!),
            then: static error => Assert.Equal(expected: "Input", actual: error.Category()));
        Spec.Invalid(Analyze.Run(operation: Faces.All.Operation<Brep, Curve>(), input: default(Brep)!),
            then: static error => {
                Fault.Unsupported fault = Assert.IsType<Fault.Unsupported>(@object: error);
                Assert.Equal(expected: typeof(Brep), actual: fault.GeometryType);
                Assert.Equal(expected: typeof(Curve), actual: fault.OutputType);
            });
    }
}
