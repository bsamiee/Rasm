using System.Collections.Frozen;
using System.Runtime.InteropServices;
using Foundation.CSharp.Analyzers.Contracts;
using Grasshopper2.Extensions;

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
                object candidate when candidate.GetType().AsKind().IsSome => Op.Create(value: nameof(Shape)).RequireValid(value: candidate).Map(static valid => new Shape(inner: valid)),
                object candidate => Fin.Fail<Shape>(new Fault.UnsupportedSource(PortName: nameof(Shape), SourceType: candidate.GetType(), Hint: Accepted)),
            });
}
[StructLayout(LayoutKind.Auto)]
public readonly record struct Flow<T>(Pear<T> Pear, Option<Grasshopper2.Data.Path> Branch) {
    public T Item => Pear.Item;
    public MetaData Meta => Pear.Meta;
    public Flow<TOut> Project<TOut>(TOut item) => new(Pear: Pear<TOut>.Create(item: item, meta: Meta), Branch: Branch);
}

// --- [SERVICES] -------------------------------------------------------------------------
[BoundaryAdapter]
public static class Bridge {
    public static Fin<Analyze.Scope> Scope(this IDataAccess access) {
        ArgumentNullException.ThrowIfNull(argument: access);
        _ = access.GetUnitSystem(unitSystem: out UnitSystem units);
        Rhino.UnitSystem system = units.System == Rhino.UnitSystem.Unset ? Rhino.UnitSystem.Millimeters : units.System;
        return (access.GetTolerance(absoluteTolerance: out double absolute, relativeTolerance: out double relative), access.GetTolerance(angularTolerance: out Angle angle), system) switch {
            (true, true, Rhino.UnitSystem known) => Fin.Succ(Analyze.In(absolute: absolute, relative: relative, angle: angle.Radians, units: known)),
            _ => Remark(access: access, units: units.System),
        };
    }
    internal static Fin<Seq<Flow<Shape>>> ReadShape(this IDataAccess access, int slot, IPort port) {
        ArgumentNullException.ThrowIfNull(argument: access);
        ArgumentNullException.ThrowIfNull(argument: port);
        return Read<object>(access: access, slot: slot, port: port)
            .Bind(values => values.TraverseM(sourced => NormalizeShape(raw: sourced.Item)
                .ToFin(new Fault.UnsupportedSource(PortName: port.Name, SourceType: sourced.Item.GetType(), Hint: Shape.Accepted))
                .Map(shape => new Flow<Shape>(Pear: Pear<Shape>.Create(item: shape, meta: sourced.Meta), Branch: sourced.Branch))).As());
    }
    internal static Fin<Seq<Flow<TVal>>> Read<TVal>(this IDataAccess access, int slot, IPort port) {
        ArgumentNullException.ThrowIfNull(argument: access);
        ArgumentNullException.ThrowIfNull(argument: port);
        return AccessDispatch<TVal>.Readers.GetValueOrDefault(key: port.Access) switch {
            Func<IDataAccess, int, IPort, Fin<Seq<Flow<TVal>>>> reader => reader(arg1: access, arg2: slot, arg3: port),
            _ => Fin.Fail<Seq<Flow<TVal>>>(new Fault.UnsupportedAccess(AccessName: port.Access.ToString())),
        };
    }
    internal static Unit Write<TOut>(this IDataAccess access, int slot, string name, Access targetAccess, Seq<Flow<TOut>> values) =>
        AccessDispatch<TOut>.Writers.GetValueOrDefault(key: targetAccess) switch {
            Func<IDataAccess, int, Seq<Flow<TOut>>, Unit> writer => writer(arg1: access, arg2: slot, arg3: values),
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
    private static Fin<Seq<Flow<TVal>>> MissingFlow<TVal>(IPort port) =>
        Missing<TVal>(port: port).Map(static pears => pears.Map(static pear => new Flow<TVal>(Pear: pear, Branch: Option<Grasshopper2.Data.Path>.None)));
    private static Seq<Leaf<T>> Leaves<T>(Seq<Flow<T>> values) =>
        toSeq(values.GroupBy(static value => value.Branch.IfNone(new Grasshopper2.Data.Path(0)))
            .SelectMany(static group => group.Select((value, index) => new Leaf<T>(pear: value.Pear, site: new Site(path: group.Key, item: index)))));
    private static readonly Seq<Func<object, Option<Shape>>> Brokers = Seq<Func<object, Option<Shape>>>(
        static raw => AsShape(value: raw),
        static raw => CurveBroker.CastOrConvert(data: raw, p2: out Line line, p3: out Triangle triangle, p4: out Rectangle3d rectangle, pn: out Polyline polyline, a360: out Circle circle, ax: out Arc arc, c: out Curve curve) switch {
            CurveType.Line => AsShape(value: line),
            CurveType.Triangle => AsShape(value: triangle.ToPolyline()),
            CurveType.Rectangle => AsShape(value: rectangle.ToPolyline()),
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
        internal static readonly FrozenDictionary<Access, Func<IDataAccess, int, IPort, Fin<Seq<Flow<T>>>>> Readers =
            new Dictionary<Access, Func<IDataAccess, int, IPort, Fin<Seq<Flow<T>>>>> {
                [Access.Item] = static (access, slot, port) => access.GetPear<T>(index: slot, pear: out Pear<T> pear) switch {
                    true when pear is { Item: not null } => Fin.Succ(Seq(new Flow<T>(Pear: pear, Branch: Option<Grasshopper2.Data.Path>.None))),
                    _ => MissingFlow<T>(port: port),
                },
                [Access.Twig] = static (access, slot, port) => access.GetPears<T>(index: slot, pears: out Pear<T>[] pears) switch {
                    true => toSeq(pears.Select((pear, index) => (Pear: pear, Index: index))).TraverseM(item => item.Pear is { Item: not null }
                        ? Fin.Succ(new Flow<T>(Pear: item.Pear, Branch: Option<Grasshopper2.Data.Path>.None))
                        : Fin.Fail<Flow<T>>(new Fault.InputRequired(PortName: port.Name, Hint: $"Null twig item at index {item.Index}."))).As(),
                    _ => MissingFlow<T>(port: port),
                },
                [Access.Tree] = static (access, slot, port) => access.GetTree<T>(index: slot, tree: out Tree<T> tree) switch {
                    true => toSeq(tree.EnumerateLeaves().Select((leaf, index) => (Leaf: leaf, Index: index))).TraverseM(item => item.Leaf.Pear is { Item: not null }
                        ? Fin.Succ(new Flow<T>(Pear: item.Leaf.Pear, Branch: Some(item.Leaf.Site.Path)))
                        : Fin.Fail<Flow<T>>(new Fault.InputRequired(PortName: port.Name, Hint: $"Null tree item at index {item.Index}."))).As(),
                    _ => MissingFlow<T>(port: port),
                },
            }.ToFrozenDictionary();
        internal static readonly FrozenDictionary<Access, Func<IDataAccess, int, Seq<Flow<T>>, Unit>> Writers =
            new Dictionary<Access, Func<IDataAccess, int, Seq<Flow<T>>, Unit>> {
                [Access.Item] = static (access, slot, values) => values.Count switch {
                    > 0 => Effect(action: () => access.SetPear(index: slot, pear: values[0].Pear)),
                    _ => Unit.Default,
                },
                [Access.Twig] = static (access, slot, values) => Effect(action: () => access.SetTwig(index: slot, twig: Garden.TwigFromPears(pears: values.Map(static value => value.Pear).AsIterable()))),
                [Access.Tree] = static (access, slot, values) => Effect(action: () => {
                    ITree tree = values.Exists(static value => value.Branch.IsSome)
                        ? Garden.TreeFromLeaves(leaves: Leaves(values: values).AsIterable())
                        : Garden.TreeFromPears(pears: values.Map(static value => value.Pear).AsIterable());
                    access.SetTree(index: slot, tree: TreePrefix(access: access, slot: slot) is int prefix ? tree.WithPathPrefix(element: prefix) : tree);
                }),
            }.ToFrozenDictionary();
    }
}
