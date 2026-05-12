namespace Rasm.Grasshopper;

public static class Bridge {
    public static Fin<Analyze.Scope> Scope(this IDataAccess access) {
        ArgumentNullException.ThrowIfNull(argument: access);
        return (
            access.GetTolerance(absoluteTolerance: out double absolute, relativeTolerance: out double relative),
            access.GetTolerance(angularTolerance: out Angle angle),
            access.GetUnitSystem(unitSystem: out UnitSystem units),
            units.System
        ) switch {
            (_, _, _, not Rhino.UnitSystem.Unset) => Fin.Succ(Analyze.In(
                absolute: absolute,
                relative: relative,
                angle: angle.Radians,
                units: units.System)),
            _ => Remark(access: access, units: units.System),
        };
    }
    public static Fin<Shape> ReadShape(this IDataAccess access, int slot, IPort port) {
        ArgumentNullException.ThrowIfNull(argument: access);
        ArgumentNullException.ThrowIfNull(argument: port);
        return Read<object>(access: access, slot: slot, port: port)
            .Bind(data => data.Value
                .Bind(NormalizeShape)
                .ToFin(Error.New(message: $"{port.Name} input is required. Connect: {Shape.Accepted}.")));
    }
    public static Fin<PortData<TVal>> Read<TVal>(this IDataAccess access, int slot, IPort port) {
        ArgumentNullException.ThrowIfNull(argument: access);
        ArgumentNullException.ThrowIfNull(argument: port);
        Coverage coverage = access.CoverageIn(index: slot);
        return port.Access switch {
            Access.Item => access.GetPear<TVal>(index: slot, pear: out Pear<TVal> pear) switch {
                true => Fin.Succ(Data(values: Seq(new PortValue<TVal>(Value: pear.Item, Meta: pear.Meta, IsNull: pear.Item is null, Index: Some(0), Coverage: coverage, Path: Option<Grasshopper2.Data.Path>.None)))),
                _ => Missing<TVal>(port: port),
            },
            Access.Twig => access.GetPears<TVal>(index: slot, pears: out Pear<TVal>[] pears) switch {
                true when pears.Length > 0 => Fin.Succ(Data(values: Values(pears: pears, coverage: coverage))),
                _ => Missing<TVal>(port: port),
            },
            Access.Tree => access.GetTree<TVal>(index: slot, tree: out Tree<TVal> tree) switch {
                true => Fin.Succ(Data(values: Values(tree: tree, coverage: coverage))),
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
    internal static Analyze.Runtime Runtime(IDataAccess access, Context context) => new(Context: context, Cancellation: access.Solution.Token, Progress: new Progress(access: access));
    internal static Unit Write<TOut>(IDataAccess access, int slot, string name, Access targetAccess, OutputValue<TOut>[] values) {
        // BOUNDARY ADAPTER — GH2 SetPear/SetTwig/SetTree are void; dispatched as a single Action invoked once.
        Coverage coverage = access.CoverageOut(index: slot);
        Action effect = (targetAccess, values.Length) switch {
            (Access.Item, > 0) => () => access.SetPear(index: slot, pear: values[0].Meta is MetaData meta ? Pear<TOut>.Create(item: values[0].Value!, meta: meta) : Pear<TOut>.Create(item: values[0].Value!)),
            (Access.Twig, _) => () => access.SetTwig<TOut>(index: slot, values: [.. values.Select(static value => value.Value)], metas: [.. values.Select(static value => value.Meta ?? MetaData.Empty)], nulls: [.. values.Select(static value => value.IsNull)]),
            (Access.Tree, _) => () => access.SetTree(index: slot, tree: Tree(access: access, values: values, coverage: coverage)),
            (Access.Item, _) => static () => { }
            ,
            _ => () => access.AddError(text: name, details: $"Unsupported output access: {targetAccess}."),
        };
        effect();
        return Unit.Default;
    }
    private static Fin<Analyze.Scope> Remark(IDataAccess access, Rhino.UnitSystem units) {
        access.AddRemark(text: "Tolerance", details: "Host did not supply reliable tolerance; using default tolerance with document units.");
        return Fin.Succ(Analyze.In(units: units));
    }
    private static Fin<PortData<TVal>> Missing<TVal>(IPort port) =>
        port.Requirement switch {
            Grasshopper2.Parameters.Requirement.MayBeMissing => Fin.Succ(Data(values: Seq<PortValue<TVal>>())),
            _ => Fin.Fail<PortData<TVal>>(Error.New(message: $"{port.Name} input is required.")),
        };
    private static PortData<TVal> Data<TVal>(Seq<PortValue<TVal>> values) => new(Values: values);
    private static Seq<PortValue<TVal>> Values<TVal>(IEnumerable<Pear<TVal>?> pears, Coverage coverage, Option<Grasshopper2.Data.Path> path = default) =>
        toSeq(pears.Select((pear, index) => new PortValue<TVal>(
            Value: pear is null ? default! : pear.Item,
            Meta: pear?.Meta ?? MetaData.Empty,
            IsNull: pear is null || pear.Item is null,
            Index: Some(index),
            Coverage: coverage,
            Path: path)));
    private static Seq<PortValue<TVal>> Values<TVal>(Tree<TVal> tree, Coverage coverage) =>
        toSeq(tree.AllTwigs.Select((twig, index) => (Path: tree.Paths[index], Twig: twig)))
            .Bind(branch => Values(pears: branch.Twig.Pears, coverage: coverage, path: Some(branch.Path)));
    private static Tree<TOut> Tree<TOut>(IDataAccess access, OutputValue<TOut>[] values, Coverage coverage) =>
        (values.Length > 0 && values.All(static value => value.Path.IsSome)) switch {
            true => Tree(values: values),
            false => Garden.TreeFromList(
                items: values.Select(static value => value.Value),
                metas: values.Select(static value => value.Meta ?? MetaData.Empty),
                nulls: values.Select(static value => value.IsNull)).WithPathPrefix(element: coverage.TwigIndex >= 0 ? coverage.TwigIndex : access.Index),
        };
    private static Tree<TOut> Tree<TOut>(OutputValue<TOut>[] values) {
        IGrouping<Grasshopper2.Data.Path, OutputValue<TOut>>[] groups = [.. values.GroupBy(static value => value.Path.IfNone(Grasshopper2.Data.Path.Zero))];
        Paths paths = Paths.FromUnsorted(
            paths: groups.Select(static group => group.Key),
            data: groups,
            sortedData: out IGrouping<Grasshopper2.Data.Path, OutputValue<TOut>>[] sorted);
        return Garden.TreeFromArrays(
            paths: paths,
            items: [.. sorted.Select(static group => group.Select(static value => value.Value).ToArray())],
            metas: [.. sorted.Select(static group => group.Select(static value => value.Meta ?? MetaData.Empty).ToArray())],
            nulls: [.. sorted.Select(static group => group.Select(static value => value.IsNull).ToArray())]);
    }
    private static Option<Shape> NormalizeShape(object raw) =>
        Shape.Create(value: raw).ToOption().Match(
            Some: static shape => Some(shape),
            None: () => Optional(CurveBroker.ToRhinoCurve(raw))
                .Bind(static curve => Shape.Create(value: curve).ToOption())
                .Match(
                    Some: static shape => Some(shape), None: () => Optional(SurfaceBroker.ToBrep(raw))
                        .Bind(static brep => Shape.Create(value: brep).ToOption())));
    private sealed class Progress(IDataAccess access) : IProgress<double> {
        public void Report(double value) => access.SetProgress(percentage: (int)Rhino.RhinoMath.Clamp(value: value switch {
            >= 0.0 and <= 1.0 => value * 100.0,
            _ => value,
        }, 0.0, 100.0));
    }
}
