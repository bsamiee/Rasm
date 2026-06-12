using Access = Grasshopper2.Parameters.Access;

namespace Radyab.Components;

[IoId("5341EE61-BBDC-4223-BBCA-A4A411CB146A")]
[Nomen(
    name: "Extract Points",
    info: "Vertices, control points, edge midpoints, quadrants, centroid, bbox center, and bbox corners.",
    category: Library.Name,
    section: Library.Extraction)]
public sealed class ExtractPoints : Component<ExtractPoints>, IComponentDefinition<ExtractPoints> {
    private static readonly Port<Shape> Geometry = Port.Shape(access: Access.Tree);
    public static ComponentSpec Definition { get; } = ComponentSpec.Define(configure: static builder => builder
        .Output<Point3d>(input: Geometry, query: AnalysisQuery.MeshPointSpatial(Points.Vertices), name: "Vertices", code: "V", info: "Native vertices, point-cloud locations, polyline corners, curve endpoints, points, or bbox corners.", access: Access.Tree)
        .Output<Point3d>(input: Geometry, query: AnalysisQuery.MeshPointSpatial(Points.ControlPoints), name: "Control Points", code: "CP", info: "NURBS control polygon points for curve, surface, and Brep inputs.", access: Access.Tree)
        .Output<Point3d>(input: Geometry, query: AnalysisQuery.MeshPointSpatial(Points.EdgeMidpoints), name: "Edge Midpoints", code: "EM", info: "Length midpoints of curves and edges; per-segment for polylines and box-like geometry.", access: Access.Tree)
        .Output<Point3d>(input: Geometry, query: AnalysisQuery.MeshPointSpatial(Points.Quadrants), name: "Quadrants", code: "Q", info: "World-cardinal extrema (top/bottom/left/right + Z) of a curve. Curve-only output.", access: Access.Tree)
        .Output<Point3d>(input: Geometry, query: AnalysisQuery.Measure(Measure.SpatialMidpoint), name: "Centroid", code: "C", info: "Mass-weighted center where native mass exists; bbox center for bounded primitives.", access: Access.Tree)
        .Output<Point3d>(input: Geometry, query: AnalysisQuery.Bounds(Bounds.Center), name: "BBox Center", code: "B", info: "Axis-aligned bounding box center for bounded geometry and primitives.", access: Access.Tree)
        .Output<Point3d>(input: Geometry, query: AnalysisQuery.Bounds(Bounds.Corners(unique: true)), name: "Bounding Corners", code: "BX", info: "Unique axis-aligned bounding-box corners: 8 for full 3D, 4 for planar, 2 for linear, 1 for point.", access: Access.Tree));
    public ExtractPoints() { }
}
