using Grasshopper2.Components;
using Grasshopper2.UI;
using GrasshopperIO;
using LanguageExt;
using Rasm.Analysis;
using Rasm.Domain;
using Rasm.Grasshopper;
using Rhino.Geometry;
using static LanguageExt.Prelude;
using Query = Rasm.Analysis.Query;

namespace Radyab.Components;

// --- [EXPORTS] -------------------------------------------------------------------------

[IoId("0C58A2D3-4709-4D74-85B1-48BC85FA1F69")]
[Nomen(
    name: "Extract Points",
    info: "Edge midpoints, spatial/mass center, bounds center, vertices, bounding corners, quadrants, and geometry kind for Rhino geometry.",
    category: "Radyab",
    section: "Extraction")]
public sealed class ExtractPoints : ShapeComponent {
    protected override Seq<IOutput<Shape>> Slots { get; } = Seq<IOutput<Shape>>(
        ShapeOutput(port: Port.List<Point3d>(name: "Edge Midpoints", code: "EM", info: "Length midpoints of curves and edges; per-segment for polylines and box-like geometry."), query: Query.EdgeMidpoints<object, Point3d>()),
        ShapeOutput(port: Port.List<Point3d>(name: "Spatial Center", code: "SC", info: "Mass-weighted center where native mass exists; bbox center for bounded primitives and point clouds."), query: Query.SpatialMidpoint<object, Point3d>()),
        ShapeOutput(port: Port.List<Point3d>(name: "Bounds Center", code: "BC", info: "Axis-aligned bounding box center for bounded geometry and primitives."), query: Query.Bounds<object, Point3d>(aspect: new Bounds.Center())),
        ShapeOutput(port: Port.List<Point3d>(name: "Vertices", code: "V", info: "Native vertices, point-cloud locations, polyline corners, curve endpoints, points, or bbox corners."), query: Query.Vertices<object, Point3d>()),
        ShapeOutput(port: Port.List<Point3d>(name: "Bounding Corners", code: "B", info: "Unique AABB corners: 8 for full 3D, 4 for planar bbox, 2 for linear, 1 for point. Tolerance-aware deduplication."), query: Query.BoundingCorners<object, Point3d>()),
        ShapeOutput(port: Port.List<Point3d>(name: "Quadrants", code: "Q", info: "World-cardinal extrema (top/bottom/left/right + Z) of a curve. Curve-only output."), query: Query.Quadrants<object, Point3d>()),
        ShapeOutput(port: Port.List<GeometryKind>(name: "Kind", code: "K", info: "Detected geometry kind, including primitive Brep and Surface families when Rhino can classify them."), query: Query.Kind<object, GeometryKind>()));

    public ExtractPoints() : base(nomen: NomenOf<ExtractPoints>()) { }
    public ExtractPoints(IReader reader) : base(reader: reader) { }
}
