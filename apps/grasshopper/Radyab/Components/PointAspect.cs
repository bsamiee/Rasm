using Analysis;
using Core.Domain;
using Grasshopper;
using Rhino.Geometry;
using Thinktecture;

namespace Radyab.Components;

// --- [MODELS] ----------------------------------------------------------------------------------

[SmartEnum<string>]
public sealed partial class PointAspect {
    public static readonly PointAspect EdgeMidpoints = new(
        key: nameof(EdgeMidpoints),
        displayName: "Edge Midpoints",
        code: "EM",
        description: "Length midpoints of curves and edges; single midpoint for individual curves; per-segment midpoints for polylines and box-like geometry.",
        factory: static () => Query.EdgeMidpoints<object, Point3d>());
    public static readonly PointAspect Center = new(
        key: nameof(Center),
        displayName: "Center",
        code: "C",
        description: "Mass-weighted center: volume centroid for solid breps/meshes/SubD, area centroid for closed planar geometry and open surfaces, length centroid for open curves, bounding-box center for Box/BoundingBox, identity for Point.",
        factory: static () => Query.SpatialMidpoint<object, Point3d>());
    public static readonly PointAspect Vertices = new(
        key: nameof(Vertices),
        displayName: "Vertices",
        code: "V",
        description: "Native vertices, polyline corners, or curve endpoints; bounding-box corners for primitive geometry that lacks vertices.",
        factory: static () => Query.Vertices<object, Point3d>());
    public static readonly PointAspect BoundingCorners = new(
        key: nameof(BoundingCorners),
        displayName: "Bounding Corners",
        code: "BC",
        description: "Unique corners of the axis-aligned bounding box: 8 for full 3D, 4 for planar bbox, 2 for linear, 1 for point. Tolerance-aware deduplication.",
        factory: static () => Query.BoundingCorners<object, Point3d>());
    public static readonly PointAspect Quadrants = new(
        key: nameof(Quadrants),
        displayName: "Quadrants",
        code: "Q",
        description: "World-cardinal extrema of a curve: top, bottom, left, right in world XY (plus Z extrema for non-planar curves). Uses world axes, not curve parameterization. Curve-only output.",
        factory: static () => Query.Quadrants<object, Point3d>());
    public string DisplayName { get; }
    public string Code { get; }
    public string Description { get; }
    private Func<Query<object, Point3d>> Factory { get; }

    // --- [OPERATIONS] --------------------------------------------------------------------------

    public IBridgeOutput<RhinoGeometry> ToBridgeOutput() =>
        new BridgeOutput<RhinoGeometry, Point3d>(
            Name: DisplayName,
            Code: Code,
            Description: Description,
            Query: Factory());
}
