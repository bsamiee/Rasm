using Analysis;
using Core;
using Core.Runtime;
using Grasshopper2.Components;
using Grasshopper2.Data.Meta;
using Grasshopper2.Parameters.Standard;
using Grasshopper2.Types.Numeric;
using LanguageExt.Common;
using Rhino.Geometry;
namespace Grasshopper;

// --- [CONSTANTS] -------------------------------------------------------------------------------

internal static class Defaults {
    internal const string AcceptedGeometry = "Curve, Brep, Mesh, SubD, Surface, Box, BoundingBox, Line, Polyline, Sphere, Cylinder, Cone, Torus, Plane";
}

// --- [TYPES] -----------------------------------------------------------------------------------

public readonly record struct BridgeOutput<TGeometry, TValue>(
    string Name,
    string Code,
    string Description,
    Query<TGeometry, TValue> Query) where TGeometry : notnull;

// --- [OPERATIONS] ------------------------------------------------------------------------------

public static class Bridge {
    public static AnalysisRuntime ResolveScope(this IDataAccess access) {
        ArgumentNullException.ThrowIfNull(argument: access);
        return ResolveRuntime(access: access)
            .Match(
                Succ: static (AnalysisRuntime runtime) => runtime,
                Fail: _ => RemarkAndFallback(access: access));
    }
    public static Unit RunMany<TGeometry, TValue>(
        this IDataAccess access,
        AnalysisRuntime scope,
        TGeometry geometry,
        Seq<BridgeOutput<TGeometry, TValue>> outputs) where TGeometry : notnull {
        ArgumentNullException.ThrowIfNull(argument: access);
        return outputs.Iter((int index, BridgeOutput<TGeometry, TValue> descriptor) => Run(
            access: access,
            scope: scope,
            geometry: geometry,
            index: index,
            descriptor: descriptor));
    }
    public static Unit MissingInput<TValue>(
        this IDataAccess access,
        int outputCount,
        string label,
        string accepted = Defaults.AcceptedGeometry) {
        ArgumentNullException.ThrowIfNull(argument: access);
        access.AddError(text: label, details: $"{label} input is required. Connect: {accepted}.");
        return toSeq(Enumerable.Range(start: 0, count: outputCount)).Iter((int index) =>
            WriteValues<TValue>(access: access, index: index, values: []));
    }
    private static Fin<AnalysisRuntime> ResolveRuntime(IDataAccess access) =>
        (
            access.GetTolerance(absoluteTolerance: out double absolute),
            access.GetTolerance(angularTolerance: out Angle angle),
            access.GetUnitSystem(unitSystem: out UnitSystem units)
        ) switch {
            (true, true, true) => Analyze.In(
                    absolute: absolute,
                    relative: 0.0,
                    angle: angle.Radians,
                    units: units.System)
                .Runtime,
            _ => Fin.Fail<AnalysisRuntime>(error: Error.New(message: "Host did not supply tolerance/units.")),
        };
    private static AnalysisRuntime RemarkAndFallback(IDataAccess access) {
        access.AddRemark(
            text: "Tolerance",
            details: "Host did not supply tolerance/units; using millimetres at default tolerance.");
        // BOUNDARY ADAPTER — Millimetres validates by construction; throw is unreachable
        // and required to satisfy the AnalysisRuntime return type at the GH boundary.
        return Analyze.In(units: Rhino.UnitSystem.Millimeters)
            .Runtime
            .Match(
                Succ: static (AnalysisRuntime runtime) => runtime,
                Fail: static (Error error) => throw new InvalidOperationException(message: error.Message));
    }
    private static Unit Run<TGeometry, TValue>(
        IDataAccess access,
        AnalysisRuntime scope,
        TGeometry geometry,
        int index,
        BridgeOutput<TGeometry, TValue> descriptor) where TGeometry : notnull =>
        descriptor.Query
            .Apply(geometry: geometry)
            .WithStandardResilience()
            .Run(scope)
            .Match(
                Succ: (Seq<TValue> values) => WriteValues<TValue>(access: access, index: index, values: [.. values]),
                Fail: (Error error) => Warn<TValue>(access: access, index: index, name: descriptor.Name, error: error));
    private static Unit Warn<TValue>(IDataAccess access, int index, string name, Error error) {
        access.AddWarning(text: name, details: error.Message);
        return WriteValues<TValue>(access: access, index: index, values: []);
    }
    private static Unit WriteValues<TValue>(IDataAccess access, int index, TValue[] values) {
        access.SetTwig<TValue>(
            index: index,
            values: values,
            metas: values.Length switch { 0 => [], _ => new MetaData[values.Length] },
            nulls: values.Length switch { 0 => [], _ => new bool[values.Length] });
        return Unit.Default;
    }
}
