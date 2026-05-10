using Grasshopper2.Components;
using Grasshopper2.UI;
using GrasshopperIO;
using LanguageExt;
using LanguageExt.Common;
using LanguageExt.Traits;
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
public sealed class ExtractPoints : ShapeComponent<ExtractPoints.Spec> {
    public sealed class Spec : IComponentSpec<Shape, ShapeState> {
        public static Seq<IPort> Inputs =>
            ShapeFoundation.Inputs(controls: Seq<IPort>());
        public static Seq<IOutputGroup<ShapeState>> Outputs { get; } = Seq<IOutputGroup<ShapeState>>(
            ShapeQuery(port: Port.List<Point3d>(name: "Edge Midpoints", code: "EM", info: "Length midpoints of curves and edges; per-segment for polylines and box-like geometry."), query: static () => Query.EdgeMidpoints<object, Point3d>()),
            ShapeQuery(port: Port.List<Point3d>(name: "Spatial Center", code: "SC", info: "Mass-weighted center where native mass exists; bbox center for bounded primitives and point clouds."), query: static () => Query.SpatialMidpoint<object, Point3d>()),
            ShapeQuery(port: Port.List<Point3d>(name: "Bounds Center", code: "BC", info: "Axis-aligned bounding box center for bounded geometry and primitives."), query: static () => Query.Bounds<object, Point3d>(aspect: new Bounds.Center())),
            ShapeQuery(port: Port.List<Point3d>(name: "Vertices", code: "V", info: "Native vertices, point-cloud locations, polyline corners, curve endpoints, points, or bbox corners."), query: static () => Query.Vertices<object, Point3d>()),
            ShapeQuery(port: Port.List<Point3d>(name: "Control Points", code: "CP", info: "NURBS control polygon points for curves and surfaces, converting through RhinoCommon when required."), query: static () => Query.Locate<object, Point3d>(aspect: new Location.ControlPoints())),
            ShapeQuery(port: Port.List<Point3d>(name: "Bounding Corners", code: "B", info: "Unique axis-aligned bounding-box corners: 8 for full 3D, 4 for planar, 2 for linear, 1 for point."), query: static () => Query.BoundingCorners<object, Point3d>()),
            ShapeQuery(port: Port.List<Point3d>(name: "Quadrants", code: "Q", info: "World-cardinal extrema (top/bottom/left/right + Z) of a curve. Curve-only output."), query: static () => Query.Quadrants<object, Point3d>()),
            ShapeQuery(port: Port.List<GeometryKind>(param: Param.Enum(initial: GeometryKind.Unknown), name: "Kind", code: "K", info: "Detected geometry kind, including primitive Brep and Surface families when Rhino can classify them."), query: static () => Query.Kind<object, GeometryKind>()));

        public static Fin<Shape> Read(IDataAccess access) =>
            ShapeFoundation.Read(access: access);
        public static Fin<ShapeState> Prepare(IDataAccess access, Analyze.Scope scope, Hints hints, Shape input) =>
            ShapeFoundation.Prepare(access: access, scope: scope, hints: hints, input: input);

        private static IOutputGroup<ShapeState> ShapeQuery<TOut>(Port<TOut> port, Func<Query<object, TOut>> query) =>
            Output.Query<ShapeState, object, TOut>(
                scope: static state => state.Scope,
                select: static state => state.Geometry,
                query: _ => query(),
                slots: Output.Slot<ShapeState, TOut>(port: port));
    }

    public ExtractPoints() : base(nomen: ComponentNomen.Of<ExtractPoints>()) { }
    public ExtractPoints(IReader reader) : base(reader: reader) { }
}
