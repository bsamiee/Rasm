using Rasm.Rhino.Modeling.Solids;
using Xunit;

namespace Rasm.Rhino.Tests.Modeling.Solids;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class AssemblyTests {

    [Fact]
    public async Task PlanarFaceAssemblesIntoAValidBrep() {
        using PlaneSurface surface = new(Plane.WorldXY, new Interval(0, 10), new Interval(0, 10));
        using PolylineCurve boundary = new([new Point3d(0, 0, 0), new Point3d(10, 0, 0), new Point3d(10, 10, 0), new Point3d(0, 10, 0), new Point3d(0, 0, 0)]);
        using Brep built = await RhinoHost.RunAsync(SolidAssembly.Build(new BrepTopology([surface], [], [], [], [new TopologyFace.Planar(0, IterableNE.create((BrepLoopType.Outer, IterableNE.create<Curve>(boundary))))])));
        Assert.True(built.IsValidTopology(out string topology), topology);
        Assert.True(built.IsValidGeometry(out string geometry), geometry);
        Assert.Equal((1, 4), (built.Faces.Count, built.Edges.Count));
        Assert.Equal(100d, built.GetArea(), 100d * ModelGrid.Model.Absolute);
    }

    [Fact]
    public async Task EdgeNamingAnAbsentVertexFailsAsOutsideAtItsIndex() {
        using LineCurve curve = new(Point3d.Origin, new Point3d(1, 0, 0));
        Fault<RhinoReason> fault = await Expect<Fault<RhinoReason>>.FailAsync(SolidAssembly.Build(new BrepTopology([], [curve], [(Point3d.Origin, None), (new Point3d(1, 0, 0), None)], [new TopologyEdge(0, 0, 5)], [])));
        Assert.Equal((RhinoReason.Outside, Some(0)), (fault.Reason, fault.Index));
    }
}
