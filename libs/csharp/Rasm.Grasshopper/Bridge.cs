using System.Runtime.InteropServices;

namespace Rasm.Grasshopper;

// --- [TYPES] ----------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
public readonly record struct Sourced<T>(T Value, MetaData Meta);

// --- [MODELS] ---------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
public readonly record struct Shape {
    private Shape(object inner) => Inner = inner;
    public object Inner { get; }
    public const string Accepted = "Rhino/GH geometry convertible through native RhinoCommon or GH2 brokers";
    public static Fin<Shape> Create(object? value) =>
        Optional(value)
            .ToFin(new BridgeFault.ShapeRequired())
            .Bind(static raw => raw switch {
                Shape shape => Fin.Succ(shape),
                _ => Op.Create(value: nameof(Shape)).RequireValid(value: raw).Map(static valid => new Shape(inner: valid)),
            });
}

// --- [ERRORS] ---------------------------------------------------------------------------
[Union]
internal abstract partial record BridgeFault : Error {
    private BridgeFault() { }
    public override bool IsExpected => true;
    public override bool IsExceptional => false;
    public override ErrorException ToErrorException() => new WrappedErrorExpectedException(this);
    internal abstract string Category { get; }
    internal sealed record ShapeRequired : BridgeFault {
        public override string Message => $"Shape is required. Connect: {Shape.Accepted}.";
        internal override string Category => "Input";
    }
    internal sealed record InputRequired(string PortName, string? Hint = null) : BridgeFault {
        public override string Message => Hint is null ? $"{PortName} input is required." : $"{PortName} input is required. Connect: {Hint}.";
        internal override string Category => "Input";
    }
    internal sealed record UnsupportedAccess(Access Access) : BridgeFault {
        public override string Message => $"Unsupported input access: {Access}.";
        internal override string Category => "Access";
    }
}

// --- [SERVICES] -------------------------------------------------------------------------
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
    internal static Fin<Sourced<Shape>> ReadShape(this IDataAccess access, int slot, IPort port) {
        ArgumentNullException.ThrowIfNull(argument: access);
        ArgumentNullException.ThrowIfNull(argument: port);
        return Read<object>(access: access, slot: slot, port: port)
            .Bind(values => values.Head
                .Bind(sourced => NormalizeShape(raw: sourced.Value).Map(shape => new Sourced<Shape>(Value: shape, Meta: sourced.Meta)))
                .ToFin(new BridgeFault.InputRequired(PortName: port.Name, Hint: Shape.Accepted)));
    }
    internal static Fin<Seq<Sourced<TVal>>> Read<TVal>(this IDataAccess access, int slot, IPort port) {
        ArgumentNullException.ThrowIfNull(argument: access);
        ArgumentNullException.ThrowIfNull(argument: port);
        return port.Access switch {
            Access.Item => access.GetPear<TVal>(index: slot, pear: out Pear<TVal> pear) switch {
                true when pear is { Item: not null } => Fin.Succ(Seq(new Sourced<TVal>(Value: pear.Item, Meta: pear.Meta))),
                true => Fin.Succ(Seq<Sourced<TVal>>()),
                _ => Missing<TVal>(port: port),
            },
            Access.Twig => access.GetPears<TVal>(index: slot, pears: out Pear<TVal>[] pears) switch {
                true when pears.Length > 0 => Fin.Succ(toSeq(pears).Choose(static pear =>
                    pear is { Item: not null } ? Some(new Sourced<TVal>(Value: pear.Item, Meta: pear.Meta)) : Option<Sourced<TVal>>.None)),
                _ => Missing<TVal>(port: port),
            },
            Access.Tree => access.GetTree<TVal>(index: slot, tree: out Tree<TVal> tree) switch {
                true => Fin.Succ(toSeq(tree.NonNullPears).Map(static pear => new Sourced<TVal>(Value: pear.Item, Meta: pear.Meta))),
                _ => Missing<TVal>(port: port),
            },
            _ => Fin.Fail<Seq<Sourced<TVal>>>(new BridgeFault.UnsupportedAccess(Access: port.Access)),
        };
    }
    internal static Unit Write<TOut>(IDataAccess access, int slot, string name, Access targetAccess, Seq<Sourced<TOut>> values) {
        // GH2 SetPear/SetTwig/SetTree are void; Effect adapts void to Unit at the boundary.
        static Pear<TOut> ToPear(Sourced<TOut> src) => Pear<TOut>.Create(item: src.Value!, meta: src.Meta);
        return (targetAccess, values.Count) switch {
            (Access.Item, > 0) => Effect(action: () => access.SetPear(index: slot, pear: ToPear(src: values[0]))),
            (Access.Item, _) => Unit.Default,
            (Access.Twig, _) => Effect(action: () => access.SetTwig(index: slot, twig: Garden.TwigFromPears(pears: values.AsIterable().Select(ToPear)))),
            (Access.Tree, _) => Effect(action: () => access.SetTree(index: slot, tree: Garden.TreeFromPears(pears: values.AsIterable().Select(ToPear)).WithPathPrefix(element: TreePrefix(access: access, slot: slot)))),
            _ => Effect(action: () => access.AddError(text: name, details: $"Unsupported output access: {targetAccess}.")),
        };
    }
    private static Unit Effect(Action action) {
        action();
        return Unit.Default;
    }
    private static int TreePrefix(IDataAccess access, int slot) =>
        access.CoverageOut(index: slot) switch { { TwigIndex: >= 0 } coverage => coverage.TwigIndex, _ => access.Index };
    private static Fin<Analyze.Scope> Remark(IDataAccess access, Rhino.UnitSystem units) {
        access.AddRemark(text: "Tolerance", details: "Host did not supply reliable tolerance; using default tolerance with document units.");
        return Fin.Succ(Analyze.In(units: units));
    }
    private static Fin<Seq<Sourced<TVal>>> Missing<TVal>(IPort port) =>
        port.Requirement switch {
            Grasshopper2.Parameters.Requirement.MayBeMissing => Fin.Succ(Seq<Sourced<TVal>>()),
            _ => Fin.Fail<Seq<Sourced<TVal>>>(new BridgeFault.InputRequired(PortName: port.Name)),
        };
    internal static Seq<Func<object, Option<Shape>>> ShapeBrokers { get; } = Seq<Func<object, Option<Shape>>>(
        static raw => AsShape(value: raw),
        static raw => AsShape(value: CurveBroker.ToRhinoCurve(raw)),
        static raw => AsShape(value: SurfaceBroker.ToBrep(raw)));
    private static Option<Shape> NormalizeShape(object raw) =>
        ShapeBrokers.Choose(broker => broker(arg: raw)).Head;
    private static Option<Shape> AsShape(object? value) =>
        Optional(value).Bind(static candidate => Shape.Create(value: candidate).ToOption());
    internal sealed class Progress(IDataAccess access) : IProgress<double> {
        public void Report(double value) => access.SetProgress(percentage: (int)Rhino.RhinoMath.Clamp(value: value switch {
            >= 0.0 and <= 1.0 => value * 100.0,
            _ => value,
        }, 0.0, 100.0));
    }
}
