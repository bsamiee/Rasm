namespace Radyab.Components;

[IoId("5341EE61-BBDC-4223-BBCA-A4A411CB146A")]
[Nomen(
    name: "Extract Points",
    info: "Vertices, control points, edge midpoints, quadrants, centroid, bbox center and corners, plus geometry kind label and topology.",
    category: Library.Name,
    section: Library.Extraction)]
public sealed class ExtractPoints : Component {
    private static readonly Port<Shape> Geometry = Port.Shape();
    private static readonly OutputGroup Vertices = Output.Single(input: Geometry, port: Port.Tree<Point3d>(name: "Vertices", code: "V", info: "Native vertices, point-cloud locations, polyline corners, curve endpoints, points, or bbox corners."), aspect: new Points.Vertices());
    private static readonly OutputGroup ControlPoints = Output.Single(input: Geometry, port: Port.Tree<Point3d>(name: "Control Points", code: "CP", info: "NURBS control polygon points for curves and surfaces, converting through RhinoCommon when required."), aspect: new Points.ControlPoints());
    private static readonly OutputGroup EdgeMidpoints = Output.Single(input: Geometry, port: Port.Tree<Point3d>(name: "Edge Midpoints", code: "EM", info: "Length midpoints of curves and edges; per-segment for polylines and box-like geometry."), aspect: new Points.EdgeMidpoints());
    private static readonly OutputGroup Quadrants = Output.Single(input: Geometry, port: Port.Tree<Point3d>(name: "Quadrants", code: "Q", info: "World-cardinal extrema (top/bottom/left/right + Z) of a curve. Curve-only output."), aspect: new Points.Quadrants());
    private static readonly OutputGroup Centroid = Output.Single(input: Geometry, port: Port.Tree<Point3d>(name: "Centroid", code: "C", info: "Mass-weighted center where native mass exists; bbox center for bounded primitives and point clouds."), aspect: new Measure.SpatialMidpoint());
    private static readonly OutputGroup BBoxCenter = Output.Single(input: Geometry, port: Port.Tree<Point3d>(name: "BBox Center", code: "B", info: "Axis-aligned bounding box center for bounded geometry and primitives."), aspect: new Bounds.Center());
    private static readonly OutputGroup BoundingCorners = Output.Single(input: Geometry, port: Port.Tree<Point3d>(name: "Bounding Corners", code: "BX", info: "Unique axis-aligned bounding-box corners: 8 for full 3D, 4 for planar, 2 for linear, 1 for point."), aspect: new Bounds.Corners(Unique: true));
    private static readonly OutputGroup Kind = Output.Details<Rasm.Domain.Kind>(
        input: Geometry,
        operation: static _ => Fin.Succ(Rasm.Analysis.Analyze.Kind<object, Rasm.Domain.Kind>()),
        emptyUnsupported: true,
        aspectLabel: nameof(Rasm.Domain.Kind),
        slots: [
            Output.Plain<Rasm.Domain.Kind, string>(port: Port.Tree<string>(name: "Kind", code: "K", info: "Detected geometry kind label."), project: static value => value.ToString(null, System.Globalization.CultureInfo.InvariantCulture)),
            Output.Plain<Rasm.Domain.Kind, Topology>(port: Port.Tree<Topology>(name: "Topology", code: "T", info: "Detected topology family.", kind: PortKind.Enum(initial: Topology.Unknown)), project: static value => value.Topology),
        ]);
    public ExtractPoints() : base(self: typeof(ExtractPoints), spec: ComponentSpec.Of(
        inputs: Seq<IPort>(Geometry),
        outputs: Seq<OutputGroup>(Vertices, ControlPoints, EdgeMidpoints, Quadrants, Centroid, BBoxCenter, BoundingCorners, Kind))) { }
}
