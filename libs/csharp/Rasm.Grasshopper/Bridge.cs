using System.Collections.Frozen;
using System.Globalization;
using System.Runtime.InteropServices;
using Foundation.CSharp.Analyzers.Contracts;
using Grasshopper2.Extensions;

namespace Rasm.Grasshopper;

// --- [ERRORS] ----------------------------------------------------------------------------
[Union]
internal abstract partial record GrasshopperFault : Rasm.Domain.Expected {
    private GrasshopperFault() { }
    public sealed record InputRequired(string PortName, string? Hint = null) : GrasshopperFault {
        public override string Message => Hint switch { string h => $"{PortName} input is required. Connect: {h}.", _ => $"{PortName} input is required." };
        public override string Category => "Input";
    }
    public sealed record UnsupportedSource(string PortName, Type SourceType, string? Hint = null) : GrasshopperFault {
        public override string Message => Hint switch { string h => $"{PortName} input type '{SourceType.Name}' is not supported. Connect: {h}.", _ => $"{PortName} input type '{SourceType.Name}' is not supported." };
        public override string Category => "Input";
    }
    public sealed record UnsupportedAccess(string AccessName) : GrasshopperFault {
        public override string Message => $"Unsupported input access: {AccessName}.";
        public override string Category => "Access";
    }
}

// --- [MODELS] ---------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
public readonly record struct Shape {
    private Shape(object inner) => Inner = inner;
    public object Inner { get; }
    public const string Accepted = "Rhino/GH geometry convertible through native RhinoCommon or GH2 brokers";
    internal static Fin<Shape> Create(object? value) =>
        Optional(value)
            .ToFin(new GrasshopperFault.InputRequired(PortName: nameof(Shape), Hint: Accepted))
            .Bind(static raw => raw switch {
                Shape shape => Fin.Succ(shape),
                object candidate when KindLookup.Resolve(candidate.GetType()).IsSome => Op.Create(value: nameof(Shape)).AcceptValue(value: candidate).Map(static valid => new Shape(inner: valid)),
                object candidate => Fin.Fail<Shape>(new GrasshopperFault.UnsupportedSource(PortName: nameof(Shape), SourceType: candidate.GetType(), Hint: Accepted)),
            });
}
[StructLayout(LayoutKind.Auto)]
public readonly record struct Flow<T>(Pear<T> Pear, Option<Site> Site) {
    public T Item => Pear.Item;
    public MetaData Meta => Pear.Meta;
    internal Flow<TOut> Project<TOut>(TOut item) => new(Pear: Pear<TOut>.Create(item: item, meta: Meta), Site: Site);
    internal Flow<TOut> Project<TOut>(TOut item, int index) => new(Pear: Pear<TOut>.Create(item: item, meta: Meta), Site: Site.Map(site => new Site(path: site.Path.AppendElement(site.Item), item: index)));
    internal Seq<Flow<TOut>> Project<TOut>(Seq<TOut> items) {
        Flow<T> source = this;
        return items.Count switch {
            1 => items.Map(value => source.Project(item: value)),
            _ => items.Map((value, index) => source.Project(item: value, index: index)),
        };
    }
}

// --- [SERVICES] -------------------------------------------------------------------------
[BoundaryAdapter]
public static class Bridge {
    public static Fin<Analyze.Scope> Scope(this IDataAccess access) {
        ArgumentNullException.ThrowIfNull(argument: access);
        _ = access.GetUnitSystem(unitSystem: out UnitSystem units);
        Rhino.UnitSystem system = units.System switch {
            Rhino.UnitSystem.Unset or Rhino.UnitSystem.None => Rhino.UnitSystem.Millimeters,
            Rhino.UnitSystem known => known,
        };
        bool lengthReliable = access.GetTolerance(absoluteTolerance: out double absolute, relativeTolerance: out double relative);
        bool angleReliable = access.GetTolerance(angularTolerance: out Angle angle);
        return Analyze.In(absolute: absolute, relative: relative, angle: angle.Radians, units: system).Context.Match(
            Succ: context => {
                _ = (lengthReliable && angleReliable) switch {
                    true => Unit.Default,
                    false => Effect(action: () => access.AddRemark(text: "Tolerance", details: "Host supplied fallback tolerance values; using them because they are valid.")),
                };
                return Fin.Succ(Analyze.In(context: context));
            },
            Fail: _ => Remark(access: access, units: system));
    }
    internal static Fin<Seq<Flow<Shape>>> ReadShape(this IDataAccess access, int slot, IPort port) {
        ArgumentNullException.ThrowIfNull(argument: access);
        ArgumentNullException.ThrowIfNull(argument: port);
        return Read<object>(access: access, slot: slot, port: port)
            .Bind(values => values.TraverseM(sourced => ((access.GetUnitScaling(unitSystemScaling: out double scale), scale) switch {
                (true, double factor) when Rhino.RhinoMath.IsValidDouble(x: factor) && Math.Abs(value: factor - 1.0) > Rhino.RhinoMath.ZeroTolerance =>
                    Optional(access.TryTransform(value: sourced.Pear, transformation: Transform.Scale(anchor: Point3d.Origin, scaleFactor: factor))).ToFin(new Fault.ComputationFailed(Label: "UnitScaling")),
                (_, double factor) when Rhino.RhinoMath.IsValidDouble(x: factor) => Fin.Succ<IPear>(sourced.Pear),
                _ => Fin.Fail<IPear>(new Fault.ComputationFailed(Label: "UnitScaling")),
            })
                .Bind(pear => NormalizeShape(raw: pear.Item)
                .ToFin(new GrasshopperFault.UnsupportedSource(PortName: port.Name, SourceType: sourced.Item.GetType(), Hint: Shape.Accepted))
                .Map(shape => new Flow<Shape>(Pear: Pear<Shape>.Create(item: shape, meta: pear.Meta), Site: sourced.Site)))).As());
    }
    internal static Fin<Seq<Flow<TVal>>> Read<TVal>(this IDataAccess access, int slot, IPort port) {
        ArgumentNullException.ThrowIfNull(argument: access);
        ArgumentNullException.ThrowIfNull(argument: port);
        return typeof(TVal).IsEnum switch {
            true => Read<int>(access: access, slot: slot, port: port).Bind(static values => values.TraverseM(EnumFlow<TVal>).As()),
            false => ReadNative<TVal>(access: access, slot: slot, port: port),
        };
    }
    internal static Unit Write<TOut>(this IDataAccess access, int slot, string name, Access targetAccess, Seq<Flow<TOut>> values) =>
        typeof(TOut).IsEnum switch {
            true => Write(access: access, slot: slot, name: name, targetAccess: targetAccess,
                values: values.Map(static value => value.Project(item: Convert.ToInt32(value: value.Item, provider: CultureInfo.InvariantCulture)))),
            false => WriteNative(access: access, slot: slot, name: name, targetAccess: targetAccess, values: values),
        };
    private static Fin<Seq<Flow<TVal>>> ReadNative<TVal>(IDataAccess access, int slot, IPort port) =>
        AccessDispatch<TVal>.Readers.GetValueOrDefault(key: port.Access) switch {
            Func<IDataAccess, int, IPort, Fin<Seq<Flow<TVal>>>> reader => reader(arg1: access, arg2: slot, arg3: port),
            _ => Fin.Fail<Seq<Flow<TVal>>>(new GrasshopperFault.UnsupportedAccess(AccessName: port.Access.ToString())),
        };
    private static Unit WriteNative<TOut>(IDataAccess access, int slot, string name, Access targetAccess, Seq<Flow<TOut>> values) =>
        AccessDispatch<TOut>.Writers.GetValueOrDefault(key: targetAccess) switch {
            Func<IDataAccess, int, Seq<Flow<TOut>>, Unit> writer => writer(arg1: access, arg2: slot, arg3: values),
            _ => Effect(action: () => access.AddError(text: name, details: $"Unsupported output access: {targetAccess}.")),
        };
    private static Fin<Flow<TVal>> EnumFlow<TVal>(Flow<int> value) =>
        Enum.IsDefined(enumType: typeof(TVal), value: value.Item) switch {
            true => Fin.Succ(value.Project(item: (TVal)Enum.ToObject(enumType: typeof(TVal), value: value.Item))),
            false => Fin.Fail<Flow<TVal>>(Op.Of(name: typeof(TVal).Name).InvalidInput()),
        };
    private static Unit Effect(Action action) {
        action();
        return Unit.Default;
    }
    private static int? TreePrefix(IDataAccess access, int slot) =>
        access.CoverageOut(index: slot) switch { { TwigIndex: >= 0 } coverage => coverage.TwigIndex, _ => null };
    private static Grasshopper2.Data.Path TwigPath(IDataAccess access, int slot) =>
        access.CoverageIn(index: slot) switch { { TwigIndex: >= 0 } coverage => new Grasshopper2.Data.Path(coverage.TwigIndex), _ => new Grasshopper2.Data.Path(0) };
    private static Fin<Analyze.Scope> Remark(IDataAccess access, Rhino.UnitSystem units) {
        access.AddRemark(text: "Tolerance", details: "Host did not supply reliable tolerance; using default tolerance with document units.");
        return Fin.Succ(Analyze.In(units: units == Rhino.UnitSystem.CustomUnits ? Rhino.UnitSystem.Millimeters : units));
    }
    private static Fin<Seq<Pear<TVal>>> Missing<TVal>(IPort port) =>
        port.Requirement switch {
            Grasshopper2.Parameters.Requirement.MayBeMissing => Fin.Succ(Seq<Pear<TVal>>()),
            _ => Fin.Fail<Seq<Pear<TVal>>>(new GrasshopperFault.InputRequired(PortName: port.Name)),
        };
    private static Fin<Seq<Flow<TVal>>> MissingFlow<TVal>(IPort port) =>
        Missing<TVal>(port: port).Map(static pears => pears.Map(static pear => new Flow<TVal>(Pear: pear, Site: Option<Site>.None)));
    private static Seq<Leaf<T>> Leaves<T>(Seq<Flow<T>> values) =>
        values.Map((value, index) => new Leaf<T>(pear: value.Pear, site: value.Site.IfNone(new Site(path: new Grasshopper2.Data.Path(0), item: index))));
    private readonly record struct Broker(int Priority, Func<object, Option<Shape>> Convert);
    // Priority sorts at construction (descending); declaration order is irrelevant. CurveBroker and SurfaceBroker handle disjoint inputs and share priority 100; AsShape catches any registered geometry kind at 10.
    private static readonly Seq<Broker> Brokers = toSeq(new Broker[] {
        new(Priority: 100, Convert: static raw => CurveBroker.CastOrConvert(data: raw, p2: out Line line, p3: out Triangle triangle, p4: out Rectangle3d rectangle, pn: out Polyline polyline, a360: out Circle circle, ax: out Arc arc, c: out Curve curve) switch {
            CurveType.Line => AsShape(value: line),
            CurveType.Triangle => AsShape(value: triangle.ToPolyline()),
            CurveType.Rectangle => AsShape(value: rectangle.ToPolyline()),
            CurveType.Polyline => AsShape(value: polyline),
            CurveType.Circle => AsShape(value: circle),
            CurveType.Arc => AsShape(value: arc),
            CurveType.Curve => AsShape(value: curve),
            _ => Option<Shape>.None,
        }),
        new(Priority: 100, Convert: static raw => SurfaceBroker.CastOrConvert(data: raw, p1: out Surface surface, p3: out Brep brep, p4: out SubD subd) switch {
            SurfaceLikeType.Surf => AsShape(value: surface),
            SurfaceLikeType.Brep => AsShape(value: brep),
            SurfaceLikeType.SubD => AsShape(value: subd),
            _ => Option<Shape>.None,
        }),
        new(Priority:  10, Convert: static raw => AsShape(value: raw)),
    }.OrderByDescending(static b => b.Priority));
    private static Option<Shape> NormalizeShape(object raw) => Brokers.Choose(broker => broker.Convert(arg: raw)).Head;
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
                [Access.Item] = static (access, slot, port) => ReadPears(access: access, slot: slot, port: port, site: static (_, _, _) => Option<Site>.None),
                [Access.Twig] = static (access, slot, port) => ReadPears(access: access, slot: slot, port: port, site: static (host, index, item) => Some(new Site(path: TwigPath(access: host, slot: index), item: item))),
                [Access.Tree] = static (access, slot, port) => access.GetTree<T>(index: slot, tree: out Tree<T> tree) switch {
                    true => toSeq(tree.EnumerateLeaves().Select((leaf, index) => (Leaf: leaf, Index: index))).TraverseM(item => item.Leaf.Pear is { Item: not null }
                        ? Fin.Succ(new Flow<T>(Pear: item.Leaf.Pear, Site: Some(item.Leaf.Site)))
                        : Fin.Fail<Flow<T>>(new GrasshopperFault.InputRequired(PortName: port.Name, Hint: $"Null tree item at index {item.Index}."))).As(),
                    _ => MissingFlow<T>(port: port),
                },
            }.ToFrozenDictionary();
        internal static readonly FrozenDictionary<Access, Func<IDataAccess, int, Seq<Flow<T>>, Unit>> Writers =
            new Dictionary<Access, Func<IDataAccess, int, Seq<Flow<T>>, Unit>> {
                [Access.Item] = static (access, slot, values) => values.Count switch {
                    > 0 => Effect(action: () => access.SetPear(index: slot, pear: values[0].Pear)),
                    _ => Effect(action: () => access.SetPear(index: slot, pear: null!)),
                },
                [Access.Twig] = static (access, slot, values) => Effect(action: () => access.SetTwig(index: slot, twig: Garden.TwigFromPears(pears: values.Map(static value => value.Pear).AsIterable()))),
                [Access.Tree] = static (access, slot, values) => Effect(action: () => {
                    ITree tree = values.Count switch {
                        0 => Garden.TreeEmpty<T>(),
                        _ when values.Exists(static value => value.Site.IsSome) => Garden.TreeFromLeaves(leaves: Leaves(values: values).AsIterable()),
                        _ => Garden.TreeFromPears(pears: values.Map(static value => value.Pear).AsIterable()),
                    };
                    access.SetTree(index: slot, tree: TreePrefix(access: access, slot: slot) is int prefix ? tree.WithPathPrefix(element: prefix) : tree);
                }),
            }.ToFrozenDictionary();
        private static Fin<Seq<Flow<T>>> ReadPears(IDataAccess access, int slot, IPort port, Func<IDataAccess, int, int, Option<Site>> site) =>
            access.GetPears<T>(index: slot, pears: out Pear<T>[] pears) switch {
                true when pears.Length > 0 => toSeq(pears.Select((pear, index) => (Pear: pear, Index: index))).TraverseM(item => item.Pear is { Item: not null }
                    ? Fin.Succ(new Flow<T>(Pear: item.Pear, Site: site(arg1: access, arg2: slot, arg3: item.Index)))
                    : Fin.Fail<Flow<T>>(new GrasshopperFault.InputRequired(PortName: port.Name, Hint: $"Null item at index {item.Index}."))).As(),
                _ => MissingFlow<T>(port: port),
            };
    }
}
