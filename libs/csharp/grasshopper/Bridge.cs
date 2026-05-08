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

public readonly record struct PointOutput<TGeometry>(
    string Name,
    string Code,
    string Description,
    Query<TGeometry, Point3d> Query) where TGeometry : notnull;

// --- [OPERATIONS] ------------------------------------------------------------------------------

public static class Bridge {
    public static AnalysisRuntime ResolveScope(this IDataAccess access) {
        ArgumentNullException.ThrowIfNull(argument: access);
        return ResolveRuntime(access: access)
            .Match(
                Succ: static (AnalysisRuntime runtime) => runtime,
                Fail: _ => RemarkAndFallback(access: access));
    }
    public static Unit RunMany<TGeometry>(
        this IDataAccess access,
        AnalysisRuntime scope,
        TGeometry geometry,
        Seq<PointOutput<TGeometry>> outputs) where TGeometry : notnull {
        ArgumentNullException.ThrowIfNull(argument: access);
        return outputs.Iter((int index, PointOutput<TGeometry> descriptor) => Run(
            access: access,
            scope: scope,
            geometry: geometry,
            index: index,
            descriptor: descriptor));
    }
    public static Unit MissingInput(
        this IDataAccess access,
        int outputCount,
        string label,
        string accepted = Defaults.AcceptedGeometry) {
        ArgumentNullException.ThrowIfNull(argument: access);
        access.AddError(text: label, details: $"{label} input is required. Connect: {accepted}.");
        return toSeq(Enumerable.Range(start: 0, count: outputCount)).Iter((int index) =>
            WritePoints(access: access, index: index, points: []));
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
        return Analyze.In(units: Rhino.UnitSystem.Millimeters)
            .Runtime
            .Match(
                Succ: static (AnalysisRuntime runtime) => runtime,
                Fail: static (Error error) => throw new InvalidOperationException(message: error.Message));
    }
    private static Unit Run<TGeometry>(
        IDataAccess access,
        AnalysisRuntime scope,
        TGeometry geometry,
        int index,
        PointOutput<TGeometry> descriptor) where TGeometry : notnull =>
        descriptor.Query
            .Apply(geometry: geometry)
            .WithStandardResilience()
            .Run(scope)
            .Match(
                Succ: (Seq<Point3d> points) => WritePoints(access: access, index: index, points: [.. points]),
                Fail: (Error error) => Warn(access: access, index: index, name: descriptor.Name, error: error));
    private static Unit Warn(IDataAccess access, int index, string name, Error error) {
        access.AddWarning(text: name, details: error.Message);
        return WritePoints(access: access, index: index, points: []);
    }
    private static Unit WritePoints(IDataAccess access, int index, Point3d[] points) {
        access.SetTwig<Point3d>(
            index: index,
            values: points,
            metas: points.Length switch { 0 => [], _ => new MetaData[points.Length] },
            nulls: points.Length switch { 0 => [], _ => new bool[points.Length] });
        return Unit.Default;
    }
}
