namespace Radyab.Components;

[IoId("5341EE61-BBDC-4223-BBCA-A4A411CB146A")]
[Nomen(
    name: "Extract Points",
    info: "Vertices, control points, edge midpoints, quadrants, centroid, bbox center, and bbox corners.",
    category: Library.Name,
    section: Library.Extraction)]
public sealed class ExtractPoints : Component<ExtractPoints> {
    private static readonly Port<Shape> Geometry = Port.Shape();
    private static readonly OutputBinding Vertices = Output.Of(input: Geometry, port: Port.Tree<Point3d>(name: "Vertices", code: "V", info: "Native vertices, point-cloud locations, polyline corners, curve endpoints, points, or bbox corners."), aspect: Points.Vertices);
    private static readonly OutputBinding ControlPoints = Output.Of(input: Geometry, port: Port.Tree<Point3d>(name: "Control Points", code: "CP", info: "NURBS control polygon points for curve, surface, and Brep inputs."), aspect: Points.ControlPoints);
    private static readonly OutputBinding EdgeMidpoints = Output.Of(input: Geometry, port: Port.Tree<Point3d>(name: "Edge Midpoints", code: "EM", info: "Length midpoints of curves and edges; per-segment for polylines and box-like geometry."), aspect: Points.EdgeMidpoints);
    private static readonly OutputBinding Quadrants = Output.Of(input: Geometry, port: Port.Tree<Point3d>(name: "Quadrants", code: "Q", info: "World-cardinal extrema (top/bottom/left/right + Z) of a curve. Curve-only output."), aspect: Points.Quadrants);
    private static readonly OutputBinding Centroid = Output.Of(input: Geometry, port: Port.Tree<Point3d>(name: "Centroid", code: "C", info: "Mass-weighted center where native mass exists; bbox center for bounded primitives."), aspect: Measure.SpatialMidpoint);
    private static readonly OutputBinding BBoxCenter = Output.Of(input: Geometry, port: Port.Tree<Point3d>(name: "BBox Center", code: "B", info: "Axis-aligned bounding box center for bounded geometry and primitives."), aspect: Bounds.Center);
    private static readonly OutputBinding BoundingCorners = Output.Of(input: Geometry, port: Port.Tree<Point3d>(name: "Bounding Corners", code: "BX", info: "Unique axis-aligned bounding-box corners: 8 for full 3D, 4 for planar, 2 for linear, 1 for point."), aspect: Bounds.Corners(unique: true));
    public ExtractPoints() : base(spec: ComponentSpec.Of(
        inputs: Seq<Port>(Geometry),
        outputs: Seq(Vertices, ControlPoints, EdgeMidpoints, Quadrants, Centroid, BBoxCenter, BoundingCorners))) { }
}
