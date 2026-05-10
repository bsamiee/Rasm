using Grasshopper2.Components;
using Grasshopper2.UI;
using GrasshopperIO;
using LanguageExt;
using Rasm.Analysis;
using Rasm.Domain;
using Rasm.Grasshopper;
using Rhino.Geometry;
using static LanguageExt.Prelude;

namespace Radyab.Components;

// --- [EXPORTS] -------------------------------------------------------------------------

[IoId("0C58A2D3-4709-4D74-85B1-48BC85FA1F69")]
[Nomen(
    name: "Extract Points",
    info: "Edge midpoints, spatial/mass center, bounds center, vertices, bounding corners, quadrants, and geometry kind for Rhino geometry.",
    category: "Radyab",
    section: "Extraction")]
public sealed class ExtractPoints : ShapeComponent<ExtractPoints.Spec> {
    public sealed class Spec : IComponentSpec<Shape, ShapeState> {
        public static Seq<IPort> Inputs =>
            ShapeFoundation.Inputs(controls: Seq<IPort>());
        public static Seq<IOutputGroup<ShapeState>> Outputs { get; } = Seq<IOutputGroup<ShapeState>>(
            ShapeFoundation.Query(port: Port.List<Point3d>(name: "Edge Midpoints", code: "EM", info: "Length midpoints of curves and edges; per-segment for polylines and box-like geometry."), operation: static () => ShapeFoundation.EdgeMidpoints<Point3d>()),
            ShapeFoundation.Query(port: Port.List<Point3d>(name: "Spatial Center", code: "SC", info: "Mass-weighted center where native mass exists; bbox center for bounded primitives and point clouds."), operation: static () => ShapeFoundation.SpatialMidpoint<Point3d>()),
            ShapeFoundation.Query(port: Port.List<Point3d>(name: "Bounds Center", code: "BC", info: "Axis-aligned bounding box center for bounded geometry and primitives."), operation: static () => ShapeFoundation.Bounds<Point3d>(aspect: new Bounds.Center())),
            ShapeFoundation.Query(port: Port.List<Point3d>(name: "Vertices", code: "V", info: "Native vertices, point-cloud locations, polyline corners, curve endpoints, points, or bbox corners."), operation: static () => ShapeFoundation.Vertices<Point3d>()),
            ShapeFoundation.Query(port: Port.List<Point3d>(name: "Control Points", code: "CP", info: "NURBS control polygon points for curves and surfaces, converting through RhinoCommon when required."), operation: static () => ShapeFoundation.Locate<Point3d>(aspect: new Location.ControlPoints())),
            ShapeFoundation.Query(port: Port.List<Point3d>(name: "Bounding Corners", code: "B", info: "Unique axis-aligned bounding-box corners: 8 for full 3D, 4 for planar, 2 for linear, 1 for point."), operation: static () => ShapeFoundation.BoundingCorners<Point3d>()),
            ShapeFoundation.Query(port: Port.List<Point3d>(name: "Quadrants", code: "Q", info: "World-cardinal extrema (top/bottom/left/right + Z) of a curve. Curve-only output."), operation: static () => ShapeFoundation.Quadrants<Point3d>()),
            ShapeFoundation.Query(port: Port.List<GeometryKind>(param: Param.Enum(initial: GeometryKind.Unknown), name: "Kind", code: "K", info: "Detected geometry kind, including primitive Brep and Surface families when Rhino can classify them."), operation: static () => ShapeFoundation.Kind<GeometryKind>()));

        public static Fin<Shape> Read(IDataAccess access) =>
            ShapeFoundation.Read(access: access);
        public static Fin<ShapeState> Prepare(IDataAccess access, Analyze.Scope scope, Hints hints, Shape input) =>
            ShapeFoundation.Prepare(access: access, scope: scope, hints: hints, input: input);
    }

    public ExtractPoints() : base(nomen: ComponentNomen.Of<ExtractPoints>()) { }
    public ExtractPoints(IReader reader) : base(reader: reader) { }
}
