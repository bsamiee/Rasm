using System.Runtime.InteropServices;

namespace Rasm.Grasshopper;

[StructLayout(LayoutKind.Auto)]
public readonly record struct Shape {
    private Shape(object inner) => Inner = inner;
    public object Inner { get; }
    public const string Accepted = "Rhino/GH geometry convertible through native RhinoCommon or GH2 brokers";
    public static Fin<Shape> Create(object? value) =>
        Optional(value)
            .ToFin(Error.New(message: $"Shape is required. Connect: {Accepted}."))
            .Bind(static raw => raw switch {
                Shape shape => Create(value: shape.Inner),
                _ => Op.Create(value: nameof(Shape)).RequireValid(value: raw).Map(static valid => new Shape(inner: valid)),
            });
}

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
                absolute: absolute, relative: relative, angle: angle.Radians, units: units.System)),
            _ => Remark(access: access, units: units.System),
        };
    }
    public static Fin<Shape> ReadShape(this IDataAccess access, int slot, IPort port) {
        ArgumentNullException.ThrowIfNull(argument: access);
        ArgumentNullException.ThrowIfNull(argument: port);
        return Read<object>(access: access, slot: slot, port: port)
            .Bind(values => values.Head.Bind(NormalizeShape)
                .ToFin(Error.New(message: $"{port.Name} input is required. Connect: {Shape.Accepted}.")));
    }
    public static Fin<Seq<TVal>> Read<TVal>(this IDataAccess access, int slot, IPort port) {
        ArgumentNullException.ThrowIfNull(argument: access);
        ArgumentNullException.ThrowIfNull(argument: port);
        return port.Access switch {
            Access.Item => access.GetPear<TVal>(index: slot, pear: out Pear<TVal> pear) switch {
                true when pear.Item is not null => Fin.Succ(Seq(pear.Item)),
                true => Fin.Succ(Seq<TVal>()),
                _ => Missing<TVal>(port: port),
            },
            Access.Twig => access.GetPears<TVal>(index: slot, pears: out Pear<TVal>[] pears) switch {
                true when pears.Length > 0 => Fin.Succ(toSeq(pears).Choose(static pear => pear is { Item: not null } ? Some(pear.Item) : Option<TVal>.None)),
                _ => Missing<TVal>(port: port),
            },
            Access.Tree => access.GetTree<TVal>(index: slot, tree: out Tree<TVal> tree) switch {
                true => Fin.Succ(toSeq(tree.NonNullItems)),
                _ => Missing<TVal>(port: port),
            },
            _ => Fin.Fail<Seq<TVal>>(Error.New(message: $"Unsupported input access: {port.Access}.")),
        };
    }
    private static readonly Action NoOp = static () => { };
    internal static Unit Write<TOut>(IDataAccess access, int slot, string name, Access targetAccess, Seq<TOut> values) {
        // BOUNDARY ADAPTER — GH2 SetPear/SetTwig/SetTree are void; dispatched as a single Action invoked once.
        Action effect = (targetAccess, values.Count) switch {
            (Access.Item, > 0) => () => access.SetPear(index: slot, pear: Pear<TOut>.Create(item: values[0]!)),
            (Access.Item, _) => NoOp,
            (Access.Twig, _) => () => access.SetTwig(index: slot, twig: Garden.TwigFromPears(pears: values.AsIterable().Select(static value => Pear<TOut>.Create(item: value!)))),
            (Access.Tree, _) => () => access.SetTree(index: slot, tree: Garden.TreeFromPears(pears: values.AsIterable().Select(static value => Pear<TOut>.Create(item: value!))).WithPathPrefix(element: TreePrefix(access: access, slot: slot))),
            _ => () => access.AddError(text: name, details: $"Unsupported output access: {targetAccess}."),
        };
        effect();
        return Unit.Default;
    }
    private static int TreePrefix(IDataAccess access, int slot) =>
        access.CoverageOut(index: slot) switch { { TwigIndex: >= 0 } coverage => coverage.TwigIndex, _ => access.Index };
    private static Fin<Analyze.Scope> Remark(IDataAccess access, Rhino.UnitSystem units) {
        access.AddRemark(text: "Tolerance", details: "Host did not supply reliable tolerance; using default tolerance with document units.");
        return Fin.Succ(Analyze.In(units: units));
    }
    private static Fin<Seq<TVal>> Missing<TVal>(IPort port) =>
        port.Requirement switch {
            Grasshopper2.Parameters.Requirement.MayBeMissing => Fin.Succ(Seq<TVal>()),
            _ => Fin.Fail<Seq<TVal>>(Error.New(message: $"{port.Name} input is required.")),
        };
    // Append-friendly broker registry. Rhino 9 / GH2 expose only Curve and Surface brokers; future brokers extend the Seq without touching callers.
    internal static Seq<Func<object, Option<Shape>>> ShapeBrokers { get; } = Seq<Func<object, Option<Shape>>>(
        static raw => AsShape(value: raw),
        static raw => AsShape(value: CurveBroker.ToRhinoCurve(raw)),
        static raw => AsShape(value: SurfaceBroker.ToBrep(raw)));
    private static Option<Shape> NormalizeShape(object raw) =>
        ShapeBrokers.Fold(initialState: Option<Shape>.None, f: (acc, broker) => acc.IsSome ? acc : broker(arg: raw));
    private static Option<Shape> AsShape(object? value) =>
        Optional(value).Bind(static candidate => Shape.Create(value: candidate).ToOption());
    internal sealed class Progress(IDataAccess access) : IProgress<double> {
        public void Report(double value) => access.SetProgress(percentage: (int)Rhino.RhinoMath.Clamp(value: value switch {
            >= 0.0 and <= 1.0 => value * 100.0,
            _ => value,
        }, 0.0, 100.0));
    }
}
