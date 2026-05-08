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
    private static readonly Seq<PointOutput<object>> Outputs = Seq(
        new PointOutput<object>("Edge Midpoints", "EM", "Length midpoints of curves and edges.",
            Analysis.Query.EdgeMidpoints<object, Point3d>()),
        new PointOutput<object>("Center", "C", "Intelligent center: bounding box for boxes, mass centroid for solids and curves.",
            Analysis.Query.Measure<object, Point3d>(aspect: Measure.SpatialMidpoint)),
        new PointOutput<object>("Vertices", "V", "Native vertices, corners, or curve endpoints.",
            Analysis.Query.Vertices<object, Point3d>()));

    public ExtractPointsComponent()
        : base(new Nomen(
            name: "Extract Points",
            info: "Edge midpoints, intelligent center, and vertices for any Rhino geometry.",
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
        _ = Outputs.Iter((PointOutput<object> output) => outputs.AddPoint(
            name: output.Name, code: output.Code, info: output.Description, access: Access.Twig));
    }

    protected override void Process(IDataAccess access) {
        ArgumentNullException.ThrowIfNull(argument: access);
        AnalysisRuntime scope = access.ResolveScope();
        _ = (access.GetItem(index: 0, value: out object? item), item) switch {
            (true, object geometry) => access.RunMany(scope: scope, geometry: geometry, outputs: Outputs),
            _ => access.MissingInput(outputCount: Outputs.Count, label: "Geometry"),
        };
    }
}
