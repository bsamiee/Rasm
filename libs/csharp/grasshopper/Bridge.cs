using Analysis;
using Core;
using Core.Domain;
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

public readonly record struct BridgeOutput<TInput, TValue>(
    string Name,
    string Code,
    string Description,
    Query<object, TValue> Query) : IBridgeOutput<TInput> where TInput : RhinoGeometry {
    public Type ValueType =>
        typeof(TValue);
    public Unit Execute(IDataAccess access, int index, AnalysisRuntime scope, TInput geometry) {
        ArgumentNullException.ThrowIfNull(argument: access);
        ArgumentNullException.ThrowIfNull(argument: geometry);
        return Bridge.RunOne(
            access: access,
            index: index,
            name: Name,
            scope: scope,
            geometry: geometry.Inner,
            query: Query);
    }
    public Unit WriteEmpty(IDataAccess access, int index) {
        ArgumentNullException.ThrowIfNull(argument: access);
        return Bridge.WriteValues<TValue>(access: access, index: index, values: []);
    }
}

// --- [OPERATIONS] ------------------------------------------------------------------------------

public static class Bridge {
    public static Fin<AnalysisRuntime> ResolveScope(this IDataAccess access) {
        ArgumentNullException.ThrowIfNull(argument: access);
        return (
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
            _ => RemarkAndFallback(access: access),
        };
    }
    public static Unit AddMissingInputError(
        this IDataAccess access,
        string label,
        string accepted = Defaults.AcceptedGeometry) {
        ArgumentNullException.ThrowIfNull(argument: access);
        access.AddError(text: label, details: $"{label} input is required. Connect: {accepted}.");
        return Unit.Default;
    }
    private static Fin<AnalysisRuntime> RemarkAndFallback(IDataAccess access) {
        access.AddRemark(
            text: "Tolerance",
            details: "Host did not supply tolerance/units; using millimetres at default tolerance.");
        return Analyze.In(units: Rhino.UnitSystem.Millimeters).Runtime;
    }
    internal static Unit RunOne<TGeometry, TValue>(
        IDataAccess access,
        int index,
        string name,
        AnalysisRuntime scope,
        TGeometry geometry,
        Query<TGeometry, TValue> query) where TGeometry : notnull =>
        query
            .Apply(geometry: geometry)
            .WithStandardResilience()
            .Run(scope)
            .Match(
                Succ: (Seq<TValue> values) => WriteValues<TValue>(access: access, index: index, values: [.. values]),
                Fail: (Error error) => Warn<TValue>(access: access, index: index, name: name, error: error));
    internal static Unit Warn<TValue>(IDataAccess access, int index, string name, Error error) {
        access.AddWarning(text: name, details: error.Message);
        return WriteValues<TValue>(access: access, index: index, values: []);
    }
    internal static Unit WriteValues<TValue>(IDataAccess access, int index, TValue[] values) {
        access.SetTwig<TValue>(
            index: index,
            values: values,
            metas: values.Length switch { 0 => [], _ => new MetaData[values.Length] },
            nulls: values.Length switch { 0 => [], _ => new bool[values.Length] });
        return Unit.Default;
    }
}
