using Grasshopper2.Types.Assistant;

namespace Rasm.Grasshopper;

public static class Bridge {
    public static Fin<Analyze.Scope> Scope(this IDataAccess access) {
        ArgumentNullException.ThrowIfNull(argument: access);
        return (
            access.GetTolerance(absoluteTolerance: out double absolute, relativeTolerance: out double relative),
            access.GetTolerance(angularTolerance: out Angle angle),
            Units: access.GetUnitSystem(unitSystem: out UnitSystem units),
            Value: units
        ) switch {
            (true, true, _, Grasshopper2.Parameters.Standard.UnitSystem unitSystem) => Fin.Succ(Analyze.In(absolute: absolute, relative: relative, angle: angle.Radians, units: unitSystem.System)),
            _ => Remark(access: access, units: units.System),
        };
    }
    public static Fin<Shape> ReadShape(this IDataAccess access, int slot, IPort port) {
        ArgumentNullException.ThrowIfNull(argument: access);
        ArgumentNullException.ThrowIfNull(argument: port);
        return Read<object>(access: access, slot: slot, port: port)
            .Bind(data => data.Value
                .Bind(raw => NormalizeShape(access: access, slot: slot, raw: raw))
                .ToFin(Error.New(message: $"{port.Name} input is required. Connect: {Shape.Accepted}.")));
    }
    public static Fin<PortData<TVal>> Read<TVal>(this IDataAccess access, int slot, IPort port) {
        ArgumentNullException.ThrowIfNull(argument: access);
        ArgumentNullException.ThrowIfNull(argument: port);
        Coverage coverage = access.CoverageIn(index: slot);
        bool changed = access.HasInputChanged(index: slot);
        return port.Access switch {
            Access.Item => access.GetPear<TVal>(index: slot, pear: out Pear<TVal> pear) switch {
                true => Fin.Succ(new PortData<TVal>(
                    Access: Access.Item,
                    Values: Seq(new PortValue<TVal>(Value: pear.Item, Meta: pear.Meta, IsNull: pear.Item is null, Index: Some(0), Coverage: coverage)),
                    Twig: Option<Twig<TVal>>.None,
                    Tree: Option<Tree<TVal>>.None,
                    Coverage: coverage,
                    Changed: changed)),
                _ => Missing<TVal>(port: port),
            },
            Access.Twig => access.GetPears<TVal>(index: slot, pears: out Pear<TVal>[] pears) switch {
                true when pears.Length > 0 => Fin.Succ(new PortData<TVal>(
                    Access: Access.Twig,
                    Values: Values(pears: pears, coverage: coverage),
                    Twig: access.GetTwig<TVal>(index: slot, twig: out Twig<TVal> twig) ? Some(twig) : Option<Twig<TVal>>.None,
                    Tree: Option<Tree<TVal>>.None,
                    Coverage: coverage,
                    Changed: changed)),
                _ => Missing<TVal>(port: port),
            },
            Access.Tree => access.GetTree<TVal>(index: slot, tree: out Tree<TVal> tree) switch {
                true => Fin.Succ(new PortData<TVal>(
                    Access: Access.Tree,
                    Values: Values(pears: tree.AllPears, coverage: coverage),
                    Twig: Option<Twig<TVal>>.None,
                    Tree: Some(tree),
                    Coverage: coverage,
                    Changed: changed)),
                _ => Missing<TVal>(port: port),
            },
            _ => Fin.Fail<PortData<TVal>>(Error.New(message: $"Unsupported input access: {port.Access}.")),
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
    internal static Fin<Seq<TOut>> Values<TIn, TOut>(IDataAccess access, Analyze.Scope scope, TIn input, Query<TIn, TOut> operation) where TIn : notnull {
        ArgumentNullException.ThrowIfNull(argument: access);
        ArgumentNullException.ThrowIfNull(argument: scope);
        ArgumentNullException.ThrowIfNull(argument: operation);
        return scope.Context.Bind(context => operation.Apply(geometry: input).Run(env: Runtime(access: access, context: context)));
    }
    internal static Analyze.Runtime Runtime(IDataAccess access, Context context) => new(Context: context, Cancellation: access.Solution.Token, Progress: new Progress(access: access));
    internal static Unit Write<TOut>(IDataAccess access, int slot, string name, Access targetAccess, OutputValue<TOut>[] values) {
        Coverage coverage = access.CoverageOut(index: slot);
        switch (targetAccess, values.Length) {
            case (Access.Item, > 0) when values[0].Meta is MetaData meta:
                access.SetPear(index: slot, pear: Pear<TOut>.Create(item: values[0].Value!, meta: meta));
                break;
            case (Access.Item, > 0):
                access.SetPear(index: slot, pear: Pear<TOut>.Create(item: values[0].Value!));
                break;
            case (Access.Twig, _):
                access.SetTwig<TOut>(index: slot, values: [.. values.Select(static value => value.Value)], metas: [.. values.Select(static value => value.Meta ?? MetaData.Empty)], nulls: [.. values.Select(static value => value.IsNull)]);
                break;
            case (Access.Tree, _):
                access.SetTree(index: slot, tree: Garden.TreeFromList(items: values.Select(static value => value.Value), metas: values.Select(static value => value.Meta ?? MetaData.Empty), nulls: values.Select(static value => value.IsNull)).WithPathPrefix(element: coverage.TwigIndex >= 0 ? coverage.TwigIndex : access.Index));
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
    private static Fin<PortData<TVal>> Missing<TVal>(IPort port) =>
        port.Requirement switch {
            Grasshopper2.Parameters.Requirement.MayBeMissing => Fin.Succ(new PortData<TVal>(
                Access: port.Access,
                Values: Seq<PortValue<TVal>>(),
                Twig: Option<Twig<TVal>>.None,
                Tree: Option<Tree<TVal>>.None,
                Coverage: Coverage.CreateFromAccess(access: port.Access),
                Changed: false)),
            _ => Fin.Fail<PortData<TVal>>(Error.New(message: $"{port.Name} input is required.")),
        };
    private static Seq<PortValue<TVal>> Values<TVal>(IEnumerable<Pear<TVal>?> pears, Coverage coverage) =>
        toSeq(pears.Select((pear, index) => new PortValue<TVal>(
            Value: pear is null ? default! : pear.Item,
            Meta: pear?.Meta ?? MetaData.Empty,
            IsNull: pear is null || pear.Item is null,
            Index: Some(index),
            Coverage: coverage)));
    private static Option<Shape> NormalizeShape(IDataAccess access, int slot, object raw) {
        // BOUNDARY ADAPTER — GH2 assistant-aware getters append host messages and throw when
        // no assistant exists, so exception containment stays at the IDataAccess boundary.
        return Shape.Create(value: raw).ToOption().Match(
            Some: static shape => Some(shape),
            None: () => {
                try {
                    return access.GetItemWithCurveAssistant(index: slot, value: out object curveRaw, assistant: out ICurveAssistant curveAssistant) switch {
                        true => Optional(curveAssistant.ConvertToRhinoCurve(curveRaw))
                            .Bind(static curve => Shape.Create(value: curve).ToOption()),
                        false => Option<Shape>.None,
                    };
                } catch (InvalidOperationException) {
                } catch (ArgumentException) {
                }
                try {
                    return access.GetItemWithSurfaceAssistant(index: slot, value: out object surfaceRaw, assistant: out ISurfaceAssistant surfaceAssistant) switch {
                        true => Optional(surfaceAssistant.ConvertToBrep(surfaceRaw))
                            .Bind(static brep => Shape.Create(value: brep).ToOption()),
                        false => Option<Shape>.None,
                    };
                } catch (InvalidOperationException) {
                    return Option<Shape>.None;
                } catch (ArgumentException) {
                    return Option<Shape>.None;
                }
            });
    }
    private sealed class Progress(IDataAccess access) : IProgress<double> {
        public void Report(double value) =>
            access.SetProgress(percentage: (int)Rhino.RhinoMath.Clamp(value: value switch {
                >= 0.0 and <= 1.0 => value * 100.0,
                _ => value,
            }, 0.0, 100.0));
    }
}
