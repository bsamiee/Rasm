namespace Radyab.Components;

[IoId("5341EE61-BBDC-4223-BBCA-A4A411CB146A")]
[Nomen(
    name: "Extract Points",
    info: "Edge midpoints, spatial/mass center, bounds center, vertices, bounding corners, quadrants, and geometry kind for Rhino geometry.",
    category: Library.Name,
    section: Library.Extraction)]
public sealed class ExtractPoints : Component {
    private static readonly Port<Shape> Geometry = Port.Shape();
    private static readonly IOutputGroup EdgeMidpointsOut = Output.Query(input: Geometry, port: Port.Tree<Point3d>(name: "Edge Midpoints", code: "EM", info: "Length midpoints of curves and edges; per-segment for polylines and box-like geometry."), aspect: new PointSampling.EdgeMidpoints());
    private static readonly IOutputGroup SpatialCenter = Output.Query(input: Geometry, port: Port.Tree<Point3d>(name: "Spatial Center", code: "SC", info: "Mass-weighted center where native mass exists; bbox center for bounded primitives and point clouds."), aspect: static _ => Rasm.Analysis.Query.SpatialMidpoint<object, Point3d>());
    private static readonly IOutputGroup BoundsCenter = Output.Query(input: Geometry, port: Port.Tree<Point3d>(name: "Bounds Center", code: "BC", info: "Axis-aligned bounding box center for bounded geometry and primitives."), aspect: new Bounds.Center());
    private static readonly IOutputGroup VerticesOut = Output.Query(input: Geometry, port: Port.Tree<Point3d>(name: "Vertices", code: "V", info: "Native vertices, point-cloud locations, polyline corners, curve endpoints, points, or bbox corners."), aspect: new PointSampling.Vertices());
    private static readonly IOutputGroup ControlPointsOut = Output.Query(input: Geometry, port: Port.Tree<Point3d>(name: "Control Points", code: "CP", info: "NURBS control polygon points for curves and surfaces, converting through RhinoCommon when required."), aspect: new PointSampling.ControlPoints());
    private static readonly IOutputGroup BoundingCorners = Output.Query(input: Geometry, port: Port.Tree<Point3d>(name: "Bounding Corners", code: "B", info: "Unique axis-aligned bounding-box corners: 8 for full 3D, 4 for planar, 2 for linear, 1 for point."), aspect: new Bounds.Corners(Unique: true));
    private static readonly IOutputGroup QuadrantsOut = Output.Query(input: Geometry, port: Port.Tree<Point3d>(name: "Quadrants", code: "Q", info: "World-cardinal extrema (top/bottom/left/right + Z) of a curve. Curve-only output."), aspect: new PointSampling.Quadrants());
    private static readonly IOutputGroup KindOut = Output.Details<Rasm.Domain.Kind>(
        input: Geometry,
        aspect: static _ => shape => Rasm.Analysis.Query.Kind<object, Rasm.Domain.Kind>().Apply(geometry: shape.Inner),
        emptyUnsupported: true,
        aspectLabel: nameof(Rasm.Domain.Kind),
        slots: [
            Output.Plain<Rasm.Domain.Kind, string>(port: Port.Tree<string>(name: "Kind Label", code: "KL", info: "Detected geometry kind label."), project: static value => value.ToString(null, System.Globalization.CultureInfo.InvariantCulture)),
            Output.Plain<Rasm.Domain.Kind, int>(port: Port.Tree<int>(name: "Kind Key", code: "KK", info: "Detected geometry kind key."), project: static value => value.Key),
            Output.Plain<Rasm.Domain.Kind, Topology>(port: Port.Tree<Topology>(name: "Topology", code: "T", info: "Detected topology family.", kind: PortKind.Enum(initial: Topology.Unknown)), project: static value => value.Topology),
            Output.Plain<Rasm.Domain.Kind, Primitive>(port: Port.Tree<Primitive>(name: "Primitive", code: "P", info: "Detected primitive family.", kind: PortKind.Enum(initial: Primitive.None)), project: static value => value.Primitive),
        ]);
    public ExtractPoints() : base(self: typeof(ExtractPoints), spec: ComponentSpec.Of(
        inputs: Seq<IPort>(Geometry),
        outputs: Seq<IOutputGroup>(EdgeMidpointsOut, SpatialCenter, BoundsCenter, VerticesOut, ControlPointsOut, BoundingCorners, QuadrantsOut, KindOut))) { }
}
