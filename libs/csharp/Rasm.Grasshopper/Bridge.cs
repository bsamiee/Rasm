using System.Collections.Frozen;
using System.Runtime.InteropServices;

namespace Rasm.Grasshopper;

// --- [MODELS] ---------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
public readonly record struct Shape {
    private Shape(object inner) => Inner = inner;
    public object Inner { get; }
    public const string Accepted = "Rhino/GH geometry convertible through native RhinoCommon or GH2 brokers";
    public static Fin<Shape> Create(object? value) =>
        Optional(value)
            .ToFin(new Fault.InputRequired(PortName: nameof(Shape), Hint: Accepted))
            .Bind(static raw => raw switch {
                Shape shape => Fin.Succ(shape),
                _ => Op.Create(value: nameof(Shape)).RequireValid(value: raw).Map(static valid => new Shape(inner: valid)),
            });
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
            (true, true, true, not Rhino.UnitSystem.Unset) => Fin.Succ(Analyze.In(
                absolute: absolute, relative: relative, angle: angle.Radians, units: units.System)),
            _ => Remark(access: access, units: units.System),
        };
    }
    internal static Fin<Pear<Shape>> ReadShape(this IDataAccess access, int slot, IPort port) {
        ArgumentNullException.ThrowIfNull(argument: access);
        ArgumentNullException.ThrowIfNull(argument: port);
        return Read<object>(access: access, slot: slot, port: port)
            .Bind(values => values.Head.Match<Fin<Pear<Shape>>>(
                Some: sourced => NormalizeShape(raw: sourced.Item).Match(
                    Some: shape => Fin.Succ(Pear<Shape>.Create(item: shape, meta: sourced.Meta)),
                    None: () => Fin.Fail<Pear<Shape>>(new Fault.UnsupportedSource(PortName: port.Name, SourceType: sourced.Item.GetType(), Hint: Shape.Accepted))),
                None: () => Fin.Fail<Pear<Shape>>(new Fault.InputRequired(PortName: port.Name, Hint: Shape.Accepted))));
    }
    internal static Fin<Seq<Pear<TVal>>> Read<TVal>(this IDataAccess access, int slot, IPort port) {
        ArgumentNullException.ThrowIfNull(argument: access);
        ArgumentNullException.ThrowIfNull(argument: port);
        return AccessDispatch<TVal>.Readers.GetValueOrDefault(key: port.Access) switch {
            Func<IDataAccess, int, IPort, Fin<Seq<Pear<TVal>>>> reader => reader(arg1: access, arg2: slot, arg3: port),
            _ => Fin.Fail<Seq<Pear<TVal>>>(new Fault.UnsupportedAccess(AccessName: port.Access.ToString())),
        };
    }
    internal static Unit Write<TOut>(IDataAccess access, int slot, string name, Access targetAccess, Seq<Pear<TOut>> values) =>
        AccessDispatch<TOut>.Writers.GetValueOrDefault(key: targetAccess) switch {
            Func<IDataAccess, int, Seq<Pear<TOut>>, Unit> writer => writer(arg1: access, arg2: slot, arg3: values),
            _ => Effect(action: () => access.AddError(text: name, details: $"Unsupported output access: {targetAccess}.")),
        };
    private static Unit Effect(Action action) {
        action();
        return Unit.Default;
    }
    private static int? TreePrefix(IDataAccess access, int slot) =>
        access.CoverageOut(index: slot) switch { { TwigIndex: >= 0 } coverage => coverage.TwigIndex, _ => null };
    private static Fin<Analyze.Scope> Remark(IDataAccess access, Rhino.UnitSystem units) {
        access.AddRemark(text: "Tolerance", details: "Host did not supply reliable tolerance; using default tolerance with document units.");
        return Fin.Succ(Analyze.In(units: units == Rhino.UnitSystem.Unset ? Rhino.UnitSystem.Millimeters : units));
    }
    private static Fin<Seq<Pear<TVal>>> Missing<TVal>(IPort port) =>
        port.Requirement switch {
            Grasshopper2.Parameters.Requirement.MayBeMissing => Fin.Succ(Seq<Pear<TVal>>()),
            _ => Fin.Fail<Seq<Pear<TVal>>>(new Fault.InputRequired(PortName: port.Name)),
        };
    private static readonly Seq<Func<object, Option<Shape>>> Brokers = Seq<Func<object, Option<Shape>>>(
        static raw => AsShape(value: raw),
        static raw => CurveBroker.CastOrConvert(data: raw, p2: out Line line, p3: out Triangle triangle, p4: out Rectangle3d rectangle, pn: out Polyline polyline, a360: out Circle circle, ax: out Arc arc, c: out Curve curve) switch {
            CurveType.Line => AsShape(value: line),
            CurveType.Triangle => AsShape(value: triangle),
            CurveType.Rectangle => AsShape(value: rectangle),
            CurveType.Polyline => AsShape(value: polyline),
            CurveType.Circle => AsShape(value: circle),
            CurveType.Arc => AsShape(value: arc),
            CurveType.Curve => AsShape(value: curve),
            _ => Option<Shape>.None,
        },
        static raw => SurfaceBroker.CastOrConvert(data: raw, p1: out Surface surface, p3: out Brep brep, p4: out SubD subd) switch {
            SurfaceLikeType.Surf => AsShape(value: surface),
            SurfaceLikeType.Brep => AsShape(value: brep),
            SurfaceLikeType.SubD => AsShape(value: subd),
            _ => Option<Shape>.None,
        });
    private static Option<Shape> NormalizeShape(object raw) => Brokers.Choose(b => b(arg: raw)).Head;
    private static Option<Shape> AsShape(object? value) => Optional(value).Bind(static candidate => Shape.Create(value: candidate).ToOption());
    internal sealed class Progress(IDataAccess access) : IProgress<double> {
        public void Report(double value) => access.SetProgress(percentage: (int)Rhino.RhinoMath.Clamp(value: value switch {
            >= 0.0 and <= 1.0 => value * 100.0,
            _ => value,
        }, 0.0, 100.0));
    }
    private static class AccessDispatch<T> {
        internal static readonly FrozenDictionary<Access, Func<IDataAccess, int, IPort, Fin<Seq<Pear<T>>>>> Readers =
            new Dictionary<Access, Func<IDataAccess, int, IPort, Fin<Seq<Pear<T>>>>> {
                [Access.Item] = static (access, slot, port) => access.GetPear<T>(index: slot, pear: out Pear<T> pear) switch {
                    true when pear is { Item: not null } => Fin.Succ(Seq(pear)),
                    _ => Missing<T>(port: port),
                },
                [Access.Twig] = static (access, slot, port) => access.GetPears<T>(index: slot, pears: out Pear<T>[] pears) switch {
                    true when pears.Length > 0 => Fin.Succ(toSeq(pears).Filter(static pear => pear is { Item: not null })),
                    _ => Missing<T>(port: port),
                },
                [Access.Tree] = static (access, slot, port) => access.GetTree<T>(index: slot, tree: out Tree<T> tree) switch {
                    true => Fin.Succ(toSeq(tree.NonNullPears)),
                    _ => Missing<T>(port: port),
                },
            }.ToFrozenDictionary();
        internal static readonly FrozenDictionary<Access, Func<IDataAccess, int, Seq<Pear<T>>, Unit>> Writers =
            new Dictionary<Access, Func<IDataAccess, int, Seq<Pear<T>>, Unit>> {
                [Access.Item] = static (access, slot, values) => values.Count switch {
                    > 0 => Effect(action: () => access.SetPear(index: slot, pear: values[0])),
                    _ => Unit.Default,
                },
                [Access.Twig] = static (access, slot, values) => Effect(action: () => access.SetTwig(index: slot, twig: Garden.TwigFromPears(pears: values.AsIterable()))),
                [Access.Tree] = static (access, slot, values) => Effect(action: () => {
                    ITree tree = Garden.TreeFromPears(pears: values.AsIterable());
                    access.SetTree(index: slot, tree: TreePrefix(access: access, slot: slot) is int prefix ? tree.WithPathPrefix(element: prefix) : tree);
                }),
            }.ToFrozenDictionary();
    }
}
