using Analysis;
using Grasshopper2.Components;
using Grasshopper2.Data.Meta;
using Grasshopper2.Parameters;
using Grasshopper2.UI;
using GrasshopperIO;
using LanguageExt;
using LanguageExt.Common;
using Rhino;
using Rhino.Geometry;

namespace Radyab.Boundary;

// --- [COMPONENT] -------------------------------------------------------------------------------

[IoId("0C58A2D3-4709-4D74-85B1-48BC85FA1F69")]
public sealed class ExtractPointsComponent : Component {
    public ExtractPointsComponent()
        : base(new Nomen(
            "Extract Points",
            "Extract edge midpoints, spatial midpoint, and vertices.")) { }

    public ExtractPointsComponent(IReader reader)
        : base(reader: reader) { }

    protected override void AddInputs(InputAdder inputs) {
        ArgumentNullException.ThrowIfNull(argument: inputs);
        _ = inputs.AddGeneric(
            "Element",
            "E",
            "Geometry to analyse.",
            Access.Item,
            Requirement.MustExist);
    }

    protected override void AddOutputs(OutputAdder outputs) {
        ArgumentNullException.ThrowIfNull(argument: outputs);
        _ = outputs.AddPoint(
            "Edge Midpoints",
            "EM",
            "Length midpoints of curves and edges.",
            Access.Twig);
        _ = outputs.AddPoint(
            "Spatial Midpoint",
            "SM",
            "Native mass-property centroid or box center.",
            Access.Twig);
        _ = outputs.AddPoint(
            "Vertices",
            "V",
            "Native vertices, corners, or curve endpoints.",
            Access.Twig);
    }

    protected override void Process(IDataAccess access) {
        ArgumentNullException.ThrowIfNull(argument: access);
        Analyze.Scope scope = Analyze.From(doc: RhinoDoc.ActiveDoc);
        _ = access.GetItem(index: 0, value: out object? item) switch {
            true => item switch {
                object geometry => (
                    SetPoints(
                        access: access,
                        index: 0,
                        label: "Edge Midpoints",
                        scope: scope,
                        query: Analysis.Query.EdgeMidpoints<object, Point3d>(),
                        geometry: geometry),
                    SetPoints(
                        access: access,
                        index: 1,
                        label: "Spatial Midpoint",
                        scope: scope,
                        query: Analysis.Query.Measure<object, Point3d>(aspect: Measure.SpatialMidpoint),
                        geometry: geometry),
                    SetPoints(
                        access: access,
                        index: 2,
                        label: "Vertices",
                        scope: scope,
                        query: Analysis.Query.Vertices<object, Point3d>(),
                        geometry: geometry)
                ) switch {
                    _ => Unit.Default,
                },
                _ => WriteMissingInput(access: access),
            },
            false => WriteMissingInput(access: access),
        };
    }

    private static Unit SetPoints<TGeometry>(IDataAccess access, int index, string label, Analyze.Scope scope, Query<TGeometry, Point3d> query, TGeometry geometry) where TGeometry : notnull =>
        scope
            .Run(
                query: query,
                input: [geometry])
            .ToFin()
            .Match(
                Succ: (Seq<Point3d> points) => WritePoints(access: access, index: index, points: [.. points]),
                Fail: (Error error) => WriteFailure(
                    access: access,
                    index: index,
                    label: label,
                    error: error));

    private static Unit WritePoints(IDataAccess access, int index, Point3d[] points) {
        access.SetTwig<Point3d>(
            index: index,
            values: points,
            metas: new MetaData[points.Length],
            nulls: new bool[points.Length]);
        return Unit.Default;
    }

    private static Unit WriteFailure(IDataAccess access, int index, string label, Error error) {
        access.AddWarning(
            text: label,
            details: error.Message);
        return WritePoints(access: access, index: index, points: []);
    }

    private static Unit WriteMissingInput(IDataAccess access) =>
        (
            WriteFailure(
                access: access,
                index: 0,
                label: "Edge Midpoints",
                error: Error.New(message: "Element input is required.")),
            WriteFailure(
                access: access,
                index: 1,
                label: "Spatial Midpoint",
                error: Error.New(message: "Element input is required.")),
            WriteFailure(
                access: access,
                index: 2,
                label: "Vertices",
                error: Error.New(message: "Element input is required."))
        ) switch {
            _ => Unit.Default,
        };
}
