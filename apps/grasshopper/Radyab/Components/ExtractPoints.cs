using Analysis;
using Core.Domain;
using Grasshopper;
using Grasshopper2.UI;
using GrasshopperIO;
using LanguageExt;
using Rhino.Geometry;
using static LanguageExt.Prelude;
using Query = Analysis.Query;

namespace Radyab.Components;

// --- [EXPORTS] ----------------------------------------------------------------------------------

[IoId("0C58A2D3-4709-4D74-85B1-48BC85FA1F69")]
[Nomen(
    name: "Extract Points",
    info: "Edge midpoints, intelligent center, vertices, bounding corners, and world-cardinal quadrants for any Rhino geometry.",
    category: "Radyab",
    section: "Extraction")]
public sealed class ExtractPoints : Component<RhinoGeometry> {
    protected override Seq<IOutput<RhinoGeometry>> Slots { get; } = Seq<IOutput<RhinoGeometry>>(
        Output.Of<RhinoGeometry, Point3d>(name: "Edge Midpoints", code: "EM", info: "Length midpoints of curves and edges; per-segment for polylines and box-like geometry.", query: Query.EdgeMidpoints<object, Point3d>()),
        Output.Of<RhinoGeometry, Point3d>(name: "Center", code: "C", info: "Mass-weighted: volume centroid for solids, area for closed planar/open surfaces, length for open curves, bbox for Box.", query: Query.SpatialMidpoint<object, Point3d>()),
        Output.Of<RhinoGeometry, Point3d>(name: "Vertices", code: "V", info: "Native vertices, polyline corners, curve endpoints, or bbox corners.", query: Query.Vertices<object, Point3d>()),
        Output.Of<RhinoGeometry, Point3d>(name: "Bounding Corners", code: "BC", info: "Unique AABB corners: 8 for full 3D, 4 for planar bbox, 2 for linear, 1 for point. Tolerance-aware deduplication.", query: Query.BoundingCorners<object, Point3d>()),
        Output.Of<RhinoGeometry, Point3d>(name: "Quadrants", code: "Q", info: "World-cardinal extrema (top/bottom/left/right + Z) of a curve. Curve-only output.", query: Query.Quadrants<object, Point3d>()));

    public ExtractPoints() : base(nomen: NomenOf<ExtractPoints>()) { }
    public ExtractPoints(IReader reader) : base(reader: reader) { }
}
