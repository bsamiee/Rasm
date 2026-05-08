using Analysis;
using Core.Runtime;
using Grasshopper;
using Grasshopper2.Components;
using Grasshopper2.Parameters;
using Grasshopper2.UI;
using GrasshopperIO;
using LanguageExt;
using Rhino.Geometry;
using static LanguageExt.Prelude;

namespace Radyab.Boundary;

// --- [CONSTANTS] -------------------------------------------------------------------------------

[IoId("0C58A2D3-4709-4D74-85B1-48BC85FA1F69")]
public sealed class ExtractPointsComponent : Component {
    private static readonly Seq<BridgeOutput<object, Point3d>> Outputs = Seq(
        new BridgeOutput<object, Point3d>(
            Name: "Edge Midpoints",
            Code: "EM",
            Description: "Length midpoints of curves and edges; single midpoint for individual curves; per-segment midpoints for polylines and box-like geometry.",
            Query: Analysis.Query.EdgeMidpoints<object, Point3d>()),
        new BridgeOutput<object, Point3d>(
            Name: "Center",
            Code: "C",
            Description: "Mass-weighted center: volume centroid for solid breps/meshes/SubD, area centroid for closed planar geometry and open surfaces, length centroid for open curves, bounding-box center for Box/BoundingBox, identity for Point.",
            Query: Analysis.Query.SpatialMidpoint<object, Point3d>()),
        new BridgeOutput<object, Point3d>(
            Name: "Vertices",
            Code: "V",
            Description: "Native vertices, polyline corners, or curve endpoints; bounding-box corners for primitive geometry that lacks vertices.",
            Query: Analysis.Query.Vertices<object, Point3d>()),
        new BridgeOutput<object, Point3d>(
            Name: "Bounding Corners",
            Code: "BC",
            Description: "Unique corners of the axis-aligned bounding box: 8 for full 3D, 4 for planar bbox, 2 for linear, 1 for point. Tolerance-aware deduplication.",
            Query: Analysis.Query.BoundingCorners<object, Point3d>()),
        new BridgeOutput<object, Point3d>(
            Name: "Quadrants",
            Code: "Q",
            Description: "World-cardinal extrema of a curve: top, bottom, left, right in world XY (plus Z extrema for non-planar curves). Uses world axes, not curve parameterization. Curve-only output.",
            Query: Analysis.Query.Quadrants<object, Point3d>()));

    public ExtractPointsComponent()
        : base(new Nomen(
            name: "Extract Points",
            info: "Edge midpoints, intelligent center, vertices, bounding corners, and world-cardinal quadrants for any Rhino geometry.",
            chapter: "Radyab",
            section: "Extraction")) { }

    public ExtractPointsComponent(IReader reader) : base(reader: reader) { }

    protected override void AddInputs(InputAdder inputs) {
        ArgumentNullException.ThrowIfNull(argument: inputs);
        _ = inputs.AddGeneric(name: "Geometry", code: "G", info: "Geometry to analyse.",
            access: Access.Item, requirement: Requirement.MustExist);
    }

    protected override void AddOutputs(OutputAdder outputs) {
        ArgumentNullException.ThrowIfNull(argument: outputs);
        _ = Outputs.Iter((BridgeOutput<object, Point3d> output) => outputs.AddPoint(
            name: output.Name, code: output.Code, info: output.Description, access: Access.Twig));
    }

    protected override void Process(IDataAccess access) {
        ArgumentNullException.ThrowIfNull(argument: access);
        AnalysisRuntime scope = access.ResolveScope();
        _ = (access.GetItem(index: 0, value: out object? item), item) switch {
            (true, object geometry) => access.RunMany(scope: scope, geometry: geometry, outputs: Outputs),
            _ => access.MissingInput<Point3d>(outputCount: Outputs.Count, label: "Geometry"),
        };
    }
}
