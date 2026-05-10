using Analysis;
using Core.Domain;
using Grasshopper2.Components;
using Grasshopper2.Data.Meta;
using Grasshopper2.Parameters.Standard;
using Grasshopper2.Types.Numeric;
using LanguageExt.Common;
using Rhino.Geometry;
namespace Grasshopper;

// --- [OPERATIONS] ------------------------------------------------------------------------------

public static class Bridge {
    private const string Accepted = "Curve, Brep, Mesh, SubD, Surface, Box, BoundingBox, Line, Polyline, Sphere, Cylinder, Cone, Torus, Plane";

    public static Fin<Analyze.Scope> Scope(this IDataAccess access) {
        ArgumentNullException.ThrowIfNull(argument: access);
        return (
            access.GetTolerance(absoluteTolerance: out double absolute),
            access.GetTolerance(angularTolerance: out Angle angle),
            access.GetUnitSystem(unitSystem: out UnitSystem units)
        ) switch {
            (true, true, true) => Fin.Succ(Analyze.In(absolute: absolute, relative: 0.0, angle: angle.Radians, units: units.System)),
            _ => Remark(access: access),
        };
    }
    public static Fin<TIn> Item<TIn>(this IDataAccess access, int slot, IPort port) where TIn : RhinoGeometry {
        ArgumentNullException.ThrowIfNull(argument: access);
        ArgumentNullException.ThrowIfNull(argument: port);
        return (
            access.GetItem(index: slot, value: out object? raw),
            raw is null ? Option<RhinoGeometry>.None : RhinoGeometry.From(value: raw)
        ) switch {
            (true, { IsSome: true } wrapped) when wrapped.Case is TIn input => Fin.Succ(input),
            _ => Fin.Fail<TIn>(error: Error.New(message: $"{port.Name} input is required. Connect: {Accepted}.")),
        };
    }
    /// <summary>
    /// Resolves the index hint from the first <see cref="IPort.IsIndex"/> port. Reads from slot 1
    /// only when the head of <paramref name="ports"/> is the IsIndex port; multi-Auxiliary
    /// components with a non-head IsIndex port are not yet supported (generalise the scan when
    /// the first such layout lands).
    /// </summary>
    public static Option<int> Index(this IDataAccess access, Seq<IPort> ports) {
        ArgumentNullException.ThrowIfNull(argument: access);
        return (ports.Count, ports) switch {
            ( > 0, var ps) when ps[0].IsIndex => Some((access.GetItem(index: 1, value: out int candidate), candidate) switch {
                (true, int value) => value,
                _ => 0,
            }),
            _ => Option<int>.None,
        };
    }
    public static Unit MissingInput(this IDataAccess access, IPort port, Error error) {
        ArgumentNullException.ThrowIfNull(argument: access);
        ArgumentNullException.ThrowIfNull(argument: port);
        ArgumentNullException.ThrowIfNull(argument: error);
        access.AddError(text: port.Name, details: error.Message);
        return Unit.Default;
    }
    internal static Unit Run<TIn, TOut>(IDataAccess access, int slot, string name, Analyze.Scope scope, TIn input, Query<TIn, TOut> query) where TIn : notnull =>
        WriteTwig<TOut>(
            access: access,
            slot: slot,
            values: scope.Context.Match(
                Succ: (GeometryContext context) => query.Apply(geometry: input).Run(context).Match(
                    Succ: (Seq<TOut> values) => (TOut[])[.. values],
                    Fail: (Error error) => Warn<TOut>(access: access, name: name, error: error)),
                Fail: (Error error) => Warn<TOut>(access: access, name: name, error: error)));
    internal static Unit WriteTwig<TOut>(IDataAccess access, int slot, TOut[] values) {
        access.SetTwig<TOut>(
            index: slot,
            values: values,
            metas: values.Length switch { 0 => [], _ => new MetaData[values.Length] },
            nulls: values.Length switch { 0 => [], _ => new bool[values.Length] });
        return Unit.Default;
    }
    private static Fin<Analyze.Scope> Remark(IDataAccess access) {
        access.AddRemark(text: "Tolerance", details: "Host did not supply tolerance/units; using millimetres at default tolerance.");
        return Fin.Succ(Analyze.In(units: Rhino.UnitSystem.Millimeters));
    }
    private static TOut[] Warn<TOut>(IDataAccess access, string name, Error error) {
        access.AddWarning(text: name, details: error.Message);
        return [];
    }
}
