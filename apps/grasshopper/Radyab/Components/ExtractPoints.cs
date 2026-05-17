namespace Radyab.Components;

[IoId("5341EE61-BBDC-4223-BBCA-A4A411CB146A")]
[Nomen(
    name: "Extract Points",
    info: "Vertices, control points, edge midpoints, quadrants, centroid, bbox center and corners, plus geometry kind label and topology.",
    category: Library.Name,
    section: Library.Extraction)]
public sealed class ExtractPoints : Component<ExtractPoints> {
    private static readonly Port<Shape> Geometry = Port.Shape();
    private static readonly OutputGroup Vertices = Output.Single(input: Geometry, port: Port.Tree<Point3d>(name: "Vertices", code: "V", info: "Native vertices, point-cloud locations, polyline corners, curve endpoints, points, or bbox corners."), aspect: Points.Vertices);
    private static readonly OutputGroup ControlPoints = Output.Single(input: Geometry, port: Port.Tree<Point3d>(name: "Control Points", code: "CP", info: "NURBS control polygon points for curves and surfaces, converting through RhinoCommon when required."), aspect: Points.ControlPoints);
    private static readonly OutputGroup EdgeMidpoints = Output.Single(input: Geometry, port: Port.Tree<Point3d>(name: "Edge Midpoints", code: "EM", info: "Length midpoints of curves and edges; per-segment for polylines and box-like geometry."), aspect: Points.EdgeMidpoints);
    private static readonly OutputGroup Quadrants = Output.Single(input: Geometry, port: Port.Tree<Point3d>(name: "Quadrants", code: "Q", info: "World-cardinal extrema (top/bottom/left/right + Z) of a curve. Curve-only output."), aspect: Points.Quadrants);
    private static readonly OutputGroup Centroid = Output.Single(input: Geometry, port: Port.Tree<Point3d>(name: "Centroid", code: "C", info: "Mass-weighted center where native mass exists; bbox center for bounded primitives and point clouds."), aspect: Measure.SpatialMidpoint);
    private static readonly OutputGroup BBoxCenter = Output.Single(input: Geometry, port: Port.Tree<Point3d>(name: "BBox Center", code: "B", info: "Axis-aligned bounding box center for bounded geometry and primitives."), aspect: Bounds.Center);
    private static readonly OutputGroup BoundingCorners = Output.Single(input: Geometry, port: Port.Tree<Point3d>(name: "Bounding Corners", code: "BX", info: "Unique axis-aligned bounding-box corners: 8 for full 3D, 4 for planar, 2 for linear, 1 for point."), aspect: Bounds.Corners(unique: true));
    private static readonly OutputGroup Kind = Output.Details(
        input: Geometry,
        aspect: Topologies.Kind,
        emptyUnsupported: true,
        slots: [
            Output.Aspect<Topologies, string>(port: Port.Tree<string>(name: "Kind", code: "K", info: "Detected geometry kind label.")),
            Output.Aspect<Topologies, Topology>(port: Port.Tree<Topology>(name: "Topology", code: "T", info: "Detected topology family.")),
        ]);
    public ExtractPoints() : base(spec: ComponentSpec.Of(
        inputs: Seq<Port>(Geometry),
        outputs: Seq<OutputGroup>(Vertices, ControlPoints, EdgeMidpoints, Quadrants, Centroid, BBoxCenter, BoundingCorners, Kind))) { }
}
