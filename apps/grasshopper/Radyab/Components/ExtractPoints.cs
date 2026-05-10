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
public sealed class ExtractPoints : Component<ExtractPoints.Spec> {
    public sealed class Spec : IComponentSpec {
        private static readonly Port<Shape> Geometry = Port.Required<Shape>(kind: PortKind.Generic, name: "Geometry", code: "G", info: "Geometry to analyse.");

        public static Seq<IPort> Inputs =>
            Seq<IPort>(Geometry);
        public static Seq<IOutputGroup> Outputs { get; } = Seq<IOutputGroup>(
            ShapeOutput.Query(input: Geometry, port: Port.List<Point3d>(name: "Edge Midpoints", code: "EM", info: "Length midpoints of curves and edges; per-segment for polylines and box-like geometry."), operation: static (_, _) => ShapeQuery.EdgeMidpoints<Point3d>()),
            ShapeOutput.Query(input: Geometry, port: Port.List<Point3d>(name: "Spatial Center", code: "SC", info: "Mass-weighted center where native mass exists; bbox center for bounded primitives and point clouds."), operation: static (_, _) => ShapeQuery.SpatialMidpoint<Point3d>()),
            ShapeOutput.Query(input: Geometry, port: Port.List<Point3d>(name: "Bounds Center", code: "BC", info: "Axis-aligned bounding box center for bounded geometry and primitives."), operation: static (_, _) => ShapeQuery.Bounds<Point3d>(aspect: new Bounds.Center())),
            ShapeOutput.Query(input: Geometry, port: Port.List<Point3d>(name: "Vertices", code: "V", info: "Native vertices, point-cloud locations, polyline corners, curve endpoints, points, or bbox corners."), operation: static (_, _) => ShapeQuery.Vertices<Point3d>()),
            ShapeOutput.Query(input: Geometry, port: Port.List<Point3d>(name: "Control Points", code: "CP", info: "NURBS control polygon points for curves and surfaces, converting through RhinoCommon when required."), operation: static (_, _) => ShapeQuery.Locate<Point3d>(aspect: new Location.ControlPoints())),
            ShapeOutput.Query(input: Geometry, port: Port.List<Point3d>(name: "Bounding Corners", code: "B", info: "Unique axis-aligned bounding-box corners: 8 for full 3D, 4 for planar, 2 for linear, 1 for point."), operation: static (_, _) => ShapeQuery.BoundingCorners<Point3d>()),
            ShapeOutput.Query(input: Geometry, port: Port.List<Point3d>(name: "Quadrants", code: "Q", info: "World-cardinal extrema (top/bottom/left/right + Z) of a curve. Curve-only output."), operation: static (_, _) => ShapeQuery.Quadrants<Point3d>()),
            ShapeOutput.Query(input: Geometry, port: Port.List<GeometryKind>(kind: PortKind.Enum(initial: GeometryKind.Unknown), name: "Kind", code: "K", info: "Detected geometry kind, including primitive Brep and Surface families when Rhino can classify them."), operation: static (_, _) => ShapeQuery.Kind<GeometryKind>()));
    }

    public ExtractPoints() : base(nomen: ComponentNomen.Of<ExtractPoints>()) { }
    public ExtractPoints(IReader reader) : base(reader: reader) { }
}
