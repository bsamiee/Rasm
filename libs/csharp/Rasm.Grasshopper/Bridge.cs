using Grasshopper2.Components;
using Grasshopper2.Data.Meta;
using Grasshopper2.Parameters;
using Grasshopper2.Parameters.Standard;
using Grasshopper2.Types.Numeric;
using LanguageExt.Common;
using Rasm.Analysis;
using Rasm.Domain;
using Rhino.Geometry;
namespace Rasm.Grasshopper;

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class Bridge {
    private static string Accepted =>
        string.Join(separator: ", ", values: Shape.Items.Map(static type => type.Name));

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
    public static Fin<Shape> ReadShape(this IDataAccess access, int slot, IPort port) {
        ArgumentNullException.ThrowIfNull(argument: access);
        ArgumentNullException.ThrowIfNull(argument: port);
        return (
            access.GetItem(slot, value: out object? raw),
            raw is null ? Option<Shape>.None : NormalizeShape(raw: raw)
        ) switch {
            (true, { IsSome: true } wrapped) when wrapped.Case is Shape shape => Fin.Succ(shape),
            _ => Fin.Fail<Shape>(error: Error.New(message: $"{port.Name} input is required. Connect: {Accepted}.")),
        };
    }
    public static Option<int> Index(this IDataAccess access, int slot) {
        ArgumentNullException.ThrowIfNull(argument: access);
        return (access.GetIndex(indexParameter: slot, limit: int.MaxValue, index: out int value), value) switch {
            (true, int index) => Some(index),
            _ => Option<int>.None,
        };
    }
    public static Unit MissingInput(this IDataAccess access, Error error) {
        ArgumentNullException.ThrowIfNull(argument: access);
        ArgumentNullException.ThrowIfNull(argument: error);
        access.AddError(text: "Input", details: error.Message);
        return Unit.Default;
    }
    internal static Unit Run<TIn, TOut>(IDataAccess access, int slot, Port<TOut> port, Analyze.Scope scope, TIn input, Query<TIn, TOut> query) where TIn : notnull =>
        Write<TOut>(
            access: access,
            slot: slot,
            name: port.Name,
            targetAccess: port.Access,
            values: scope.Context.Bind(context => query.Apply(geometry: input).Run(context)).Match(
                Succ: values => (TOut[])[.. values],
                Fail: error => Warn<TOut>(access: access, name: port.Name, error: error)));
    internal static Unit Write<TOut>(IDataAccess access, int slot, string name, Access targetAccess, TOut[] values) =>
        (targetAccess, values) switch {
            (Access.Item, TOut[] output) => WriteItem(access: access, slot: slot, values: output),
            (Access.Twig, TOut[] output) => WriteTwig(access: access, slot: slot, values: output),
            _ => UnsupportedAccess(access: access, name: name, accessKind: targetAccess),
        };
    private static Unit WriteItem<TOut>(IDataAccess access, int slot, TOut[] values) {
        // Boundary adapter: GH2 item outputs expose a void setter; empty results leave the slot unset.
        switch (values.Length) {
            case > 0:
                access.SetItem(index: slot, value: values[0]);
                break;
        }
        return Unit.Default;
    }
    private static Unit WriteTwig<TOut>(IDataAccess access, int slot, TOut[] values) {
        access.SetTwig<TOut>(index: slot, values: values, metas: values.Length switch { 0 => [], _ => new MetaData[values.Length] }, nulls: values.Length switch { 0 => [], _ => new bool[values.Length] });
        return Unit.Default;
    }
    private static Fin<Analyze.Scope> Remark(IDataAccess access) {
        access.AddRemark(text: "Tolerance", details: "Host did not supply tolerance/units; using millimetres at default tolerance.");
        return Fin.Succ(Analyze.In(units: Rhino.UnitSystem.Millimeters));
    }
    private static Option<Shape> NormalizeShape(object raw) =>
        Shape.From(value: raw).Match(
            Some: static shape => Some(shape),
            None: () => Optional(CurveBroker.ToRhinoCurve(raw)).Bind(static curve => Shape.From(value: curve)).Match(
                Some: static shape => Some(shape),
                None: () => SurfaceBroker.CastOrConvert(data: raw, p1: out Surface surface, p3: out Brep brep, p4: out SubD subd) switch {
                    SurfaceLikeType.Surf => Shape.From(value: surface),
                    SurfaceLikeType.Brep => Shape.From(value: brep),
                    SurfaceLikeType.SubD => Shape.From(value: subd),
                    _ => Option<Shape>.None,
                }));
    private static Unit UnsupportedAccess(IDataAccess access, string name, Access accessKind) {
        access.AddError(text: name, details: $"Unsupported output access: {accessKind}.");
        return Unit.Default;
    }
    private static TOut[] Warn<TOut>(IDataAccess access, string name, Error error) {
        access.AddWarning(text: name, details: error.Message);
        return [];
    }
}
