using Grasshopper2.Components;
using Grasshopper2.Data;
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
    public static Fin<Analyze.Scope> Scope(this IDataAccess access) {
        ArgumentNullException.ThrowIfNull(argument: access);
        return (
            access.GetTolerance(absoluteTolerance: out double absolute),
            access.GetTolerance(angularTolerance: out Angle angle),
            Units: access.GetUnitSystem(unitSystem: out UnitSystem units),
            Value: units
        ) switch {
            (true, true, _, UnitSystem unitSystem) => Fin.Succ(Analyze.In(absolute: absolute, relative: 0.0, angle: angle.Radians, units: unitSystem.System)),
            _ => Remark(access: access, units: units.System),
        };
    }
    public static Fin<Shape> ReadShape(this IDataAccess access, int slot, IPort port) {
        ArgumentNullException.ThrowIfNull(argument: access);
        ArgumentNullException.ThrowIfNull(argument: port);
        return (
            access.GetItem(slot, value: out object? raw),
            raw is null ? Option<Shape>.None : NormalizeShape(raw: raw)
        ) switch {
            (true, { IsSome: true } wrapped) when wrapped.Case is Shape shape => shape.Validate(),
            _ => Fin.Fail<Shape>(error: Error.New(message: $"{port.Name} input is required. Connect: {Shape.Accepted}.")),
        };
    }
    public static Option<int> Index(this IDataAccess access, int slot, int limit) {
        ArgumentNullException.ThrowIfNull(argument: access);
        return (access.GetIndex(indexParameter: slot, limit: limit, index: out int value), value) switch {
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
    internal static Fin<Seq<TOut>> Values<TIn, TOut>(IDataAccess access, Analyze.Scope scope, TIn input, Query<TIn, TOut> query) where TIn : notnull {
        ArgumentNullException.ThrowIfNull(argument: access);
        ArgumentNullException.ThrowIfNull(argument: scope);
        return scope.Context.Bind(context => query.Apply(geometry: input).Run(env: Runtime(access: access, context: context)));
    }
    internal static Analyze.Runtime Runtime(IDataAccess access, Context context) =>
        new(Context: context, Cancellation: access.Solution.Token, Progress: new Progress(access: access));
    internal static Unit Write<TOut>(IDataAccess access, int slot, string name, Access targetAccess, OutputValue<TOut>[] values) {
        switch (targetAccess, values.Length) {
            case (Access.Item, > 0) when values[0].Meta is MetaData meta:
                access.SetItem(index: slot, value: values[0].Value!, meta: meta);
                break;
            case (Access.Item, > 0):
                access.SetItem(index: slot, value: values[0].Value!);
                break;
            case (Access.Twig, _):
                access.SetTwig<TOut>(index: slot, values: [.. values.Select(static value => value.Value)], metas: [.. values.Select(static value => value.Meta ?? MetaData.Empty)], nulls: []);
                break;
            case (Access.Tree, _):
                access.SetTree(index: slot, tree: Garden.TreeFromList(path: new Grasshopper2.Data.Path(access.Index), items: values.Select(static value => value.Value), metas: values.Select(static value => value.Meta ?? MetaData.Empty), nulls: []));
                break;
            case (Access.Item, _):
                break;
            default:
                access.AddError(text: name, details: $"Unsupported output access: {targetAccess}.");
                break;
        }
        return Unit.Default;
    }
    private static Fin<Analyze.Scope> Remark(IDataAccess access, Rhino.UnitSystem units) {
        access.AddRemark(text: "Tolerance", details: "Host did not supply reliable tolerance; using default tolerance with document units.");
        return Fin.Succ(Analyze.In(units: units));
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
    private sealed class Progress(IDataAccess access) : IProgress<double> {
        public void Report(double value) =>
            access.SetProgress(percentage: (int)Math.Clamp(value: value switch {
                >= 0.0 and <= 1.0 => value * 100.0,
                _ => value,
            }, min: 0.0, max: 100.0));
    }
}
