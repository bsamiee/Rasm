namespace Radyab.Components;

[IoId("0C58A2D3-4709-4D74-85B1-48BC85FA1F69")]
[Nomen(
    name: "Extract Points",
    info: "Edge midpoints, spatial/mass center, bounds center, vertices, bounding corners, quadrants, and geometry kind for Rhino geometry.",
    category: Library.Name,
    section: Library.Extraction)]
public sealed class ExtractPoints : Component {
    public ExtractPoints() : base(self: typeof(ExtractPoints)) { }
    [Input] private readonly Port<Shape> Geometry = Port.Shape();
    [Output] private IOutputGroup EdgeMidpointsOut => Output.Query(input: Geometry, port: Port.List<Point3d>(name: "Edge Midpoints", code: "EM", info: "Length midpoints of curves and edges; per-segment for polylines and box-like geometry."), aspect: new PointSampling.EdgeMidpoints());
    [Output] private IOutputGroup SpatialCenter => Output.Query(input: Geometry, port: Port.List<Point3d>(name: "Spatial Center", code: "SC", info: "Mass-weighted center where native mass exists; bbox center for bounded primitives and point clouds."), aspect: static _ => Rasm.Analysis.Query.SpatialMidpoint<object, Point3d>());
    [Output] private IOutputGroup BoundsCenter => Output.Query(input: Geometry, port: Port.List<Point3d>(name: "Bounds Center", code: "BC", info: "Axis-aligned bounding box center for bounded geometry and primitives."), aspect: new Bounds.Center());
    [Output] private IOutputGroup VerticesOut => Output.Query(input: Geometry, port: Port.List<Point3d>(name: "Vertices", code: "V", info: "Native vertices, point-cloud locations, polyline corners, curve endpoints, points, or bbox corners."), aspect: new PointSampling.Vertices());
    [Output] private IOutputGroup ControlPointsOut => Output.Query(input: Geometry, port: Port.List<Point3d>(name: "Control Points", code: "CP", info: "NURBS control polygon points for curves and surfaces, converting through RhinoCommon when required."), aspect: new PointSampling.ControlPoints());
    [Output] private IOutputGroup BoundingCorners => Output.Query(input: Geometry, port: Port.List<Point3d>(name: "Bounding Corners", code: "B", info: "Unique axis-aligned bounding-box corners: 8 for full 3D, 4 for planar, 2 for linear, 1 for point."), aspect: new Bounds.Corners(Unique: true));
    [Output] private IOutputGroup QuadrantsOut => Output.Query(input: Geometry, port: Port.List<Point3d>(name: "Quadrants", code: "Q", info: "World-cardinal extrema (top/bottom/left/right + Z) of a curve. Curve-only output."), aspect: new PointSampling.Quadrants());
    [Output] private IOutputGroup KindOut => Output.Query(input: Geometry, port: Port.List<Rasm.Domain.Kind>(name: "Kind", code: "K", info: "Detected geometry kind, including primitive Brep and Surface families when Rhino can classify them."), aspect: static _ => Rasm.Analysis.Query.Kind<object, Rasm.Domain.Kind>());
}
