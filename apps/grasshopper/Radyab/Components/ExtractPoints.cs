using Grasshopper2.Components;
using Grasshopper2.UI;
using GrasshopperIO;
using LanguageExt;
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
    info: "Edge midpoints, intelligent center, vertices, bounding corners, and world-cardinal quadrants for any Rhino geometry.",
    category: "Radyab",
    section: "Extraction")]
public sealed class ExtractPoints : Component<Shape> {
    private static readonly Port<Shape> Geometry = Port.Required<Shape>(
        param: Param.Generic,
        name: "Geometry",
        code: "G",
        info: "Rhino geometry or primitive shape value to analyze.");

    protected override Seq<IPort> Inputs { get; } = Seq<IPort>(Geometry);

    protected override Seq<IOutput<Shape>> Slots { get; } = Seq<IOutput<Shape>>(
        Output.Of<Shape, object, Point3d>(port: Port.List<Point3d>(param: Param.Point, name: "Edge Midpoints", code: "EM", info: "Length midpoints of curves and edges; per-segment for polylines and box-like geometry."), select: static shape => shape.Inner, query: Query.EdgeMidpoints<object, Point3d>()),
        Output.Of<Shape, object, Point3d>(port: Port.List<Point3d>(param: Param.Point, name: "Center", code: "C", info: "Mass-weighted: volume centroid for solids, area for closed planar/open surfaces, length for open curves, bbox for bounded primitives."), select: static shape => shape.Inner, query: Query.SpatialMidpoint<object, Point3d>()),
        Output.Of<Shape, object, Point3d>(port: Port.List<Point3d>(param: Param.Point, name: "Vertices", code: "V", info: "Native vertices, polyline corners, curve endpoints, points, or bbox corners."), select: static shape => shape.Inner, query: Query.Vertices<object, Point3d>()),
        Output.Of<Shape, object, Point3d>(port: Port.List<Point3d>(param: Param.Point, name: "Bounding Corners", code: "BC", info: "Unique AABB corners: 8 for full 3D, 4 for planar bbox, 2 for linear, 1 for point. Tolerance-aware deduplication."), select: static shape => shape.Inner, query: Query.BoundingCorners<object, Point3d>()),
        Output.Of<Shape, object, Point3d>(port: Port.List<Point3d>(param: Param.Point, name: "Quadrants", code: "Q", info: "World-cardinal extrema (top/bottom/left/right + Z) of a curve. Curve-only output."), select: static shape => shape.Inner, query: Query.Quadrants<object, Point3d>()));

    public ExtractPoints() : base(nomen: NomenOf<ExtractPoints>()) { }
    public ExtractPoints(IReader reader) : base(reader: reader) { }

    protected override Fin<Shape> Read(IDataAccess access) =>
        access.ReadShape(slot: 0, port: Geometry);
}
