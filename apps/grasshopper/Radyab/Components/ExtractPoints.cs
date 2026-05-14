namespace Radyab.Components;

[IoId("0C58A2D3-4709-4D74-85B1-48BC85FA1F69")]
[Nomen(
    name: "Extract Points",
    info: "Edge midpoints, spatial/mass center, bounds center, vertices, bounding corners, quadrants, and geometry kind for Rhino geometry.",
    category: Library.Name,
    section: Library.Extraction)]
public sealed class ExtractPoints : Component {
    private static readonly Port<Shape> Geometry = Port.Shape();
    private static readonly IOutputGroup EdgeMidpointsOut = Output.Query(input: Geometry, port: Port.List<Point3d>(name: "Edge Midpoints", code: "EM", info: "Length midpoints of curves and edges; per-segment for polylines and box-like geometry."), aspect: new PointSampling.EdgeMidpoints());
    private static readonly IOutputGroup SpatialCenter = Output.Query(input: Geometry, port: Port.List<Point3d>(name: "Spatial Center", code: "SC", info: "Mass-weighted center where native mass exists; bbox center for bounded primitives and point clouds."), aspect: static _ => Rasm.Analysis.Query.SpatialMidpoint<object, Point3d>());
    private static readonly IOutputGroup BoundsCenter = Output.Query(input: Geometry, port: Port.List<Point3d>(name: "Bounds Center", code: "BC", info: "Axis-aligned bounding box center for bounded geometry and primitives."), aspect: new Bounds.Center());
    private static readonly IOutputGroup VerticesOut = Output.Query(input: Geometry, port: Port.List<Point3d>(name: "Vertices", code: "V", info: "Native vertices, point-cloud locations, polyline corners, curve endpoints, points, or bbox corners."), aspect: new PointSampling.Vertices());
    private static readonly IOutputGroup ControlPointsOut = Output.Query(input: Geometry, port: Port.List<Point3d>(name: "Control Points", code: "CP", info: "NURBS control polygon points for curves and surfaces, converting through RhinoCommon when required."), aspect: new PointSampling.ControlPoints());
    private static readonly IOutputGroup BoundingCorners = Output.Query(input: Geometry, port: Port.List<Point3d>(name: "Bounding Corners", code: "B", info: "Unique axis-aligned bounding-box corners: 8 for full 3D, 4 for planar, 2 for linear, 1 for point."), aspect: new Bounds.Corners(Unique: true));
    private static readonly IOutputGroup QuadrantsOut = Output.Query(input: Geometry, port: Port.List<Point3d>(name: "Quadrants", code: "Q", info: "World-cardinal extrema (top/bottom/left/right + Z) of a curve. Curve-only output."), aspect: new PointSampling.Quadrants());
    private static readonly IOutputGroup KindOut = Output.Query(input: Geometry, port: Port.List<Rasm.Domain.Kind>(name: "Kind", code: "K", info: "Detected geometry kind, including primitive Brep and Surface families when Rhino can classify them."), aspect: static _ => Rasm.Analysis.Query.Kind<object, Rasm.Domain.Kind>());
    private static readonly ComponentSpec Definition = new(
        Inputs: Seq(new PortSpec(Port: Geometry)),
        Outputs: Seq(new OutputSpec(Group: EdgeMidpointsOut), new OutputSpec(Group: SpatialCenter), new OutputSpec(Group: BoundsCenter), new OutputSpec(Group: VerticesOut), new OutputSpec(Group: ControlPointsOut), new OutputSpec(Group: BoundingCorners), new OutputSpec(Group: QuadrantsOut), new OutputSpec(Group: KindOut)));
    public ExtractPoints() : base(self: typeof(ExtractPoints), spec: Definition) { }
}
