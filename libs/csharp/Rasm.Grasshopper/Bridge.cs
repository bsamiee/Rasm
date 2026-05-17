using System.Globalization;
using System.Runtime.InteropServices;
using Foundation.CSharp.Analyzers.Contracts;
using Grasshopper2.Extensions;

namespace Rasm.Grasshopper;

// --- [ERRORS] ---------------------------------------------------------------------------
[BoundaryAdapter]
public sealed record MissingPortInput(string Port, string? Hint = null) : Rasm.Domain.Expected {
    public override string Message => Hint switch { string h => $"{Port} input is required. Connect: {h}.", _ => $"{Port} input is required." };
    public override string Category => "Input";
}
[BoundaryAdapter]
public sealed record UnsupportedSource(string Port, Type SourceType, string? Hint = null) : Rasm.Domain.Expected {
    public override string Message => Hint switch { string h => $"{Port} input type '{SourceType.Name}' is not supported. Connect: {h}.", _ => $"{Port} input type '{SourceType.Name}' is not supported." };
    public override string Category => "Input";
}
[BoundaryAdapter]
public sealed record UnsupportedAccess(string Access) : Rasm.Domain.Expected {
    public override string Message => $"Unsupported input access: {Access}.";
    public override string Category => "Access";
}

// --- [MODELS] ---------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
public readonly record struct Shape {
    private readonly Option<IDisposable> owned;
    private Shape(object inner, Option<IDisposable> owned) { Inner = inner; this.owned = owned; }
    public object Inner { get; }
    public const string Accepted = "Rhino/GH geometry convertible through native RhinoCommon or GH2 brokers";
    internal Unit DisposeUnlessTransferred(Seq<object> outputs) =>
        owned.Filter(disposable => !outputs.Exists(output => ReferenceEquals(objA: output, objB: disposable) || output switch {
            TopologyProjection { Value: BrepFace face } => ReferenceEquals(objA: face.Brep, objB: disposable),
            TopologyProjection projection => ReferenceEquals(objA: projection.Value, objB: disposable),
            BrepFace face => ReferenceEquals(objA: face.Brep, objB: disposable),
            _ => false,
        }))
            .Iter(static disposable => disposable.Dispose());
    internal Flow<TSource> Detach<TSource>(Flow<TSource> output) =>
        (Inner, output.Item) switch {
            (GeometryBase source, TopologyProjection projection) => output.Project(item: (TSource)(object)projection.DetachFrom(source: source)),
            _ => output,
        };
    internal static Fin<Shape> Create(object? value) => Create(value: value, owned: Option<IDisposable>.None);
    private static Fin<Shape> Create(object? value, Option<IDisposable> owned) =>
        Optional(value)
            .ToFin(new MissingPortInput(Port: nameof(Shape), Hint: Accepted))
            .Bind(raw => raw switch {
                Shape shape => Fin.Succ(shape),
                object candidate when Rasm.Domain.Kind.Of(type: candidate.GetType()).IsSome => Op.Create(value: nameof(Shape)).AcceptValue(value: candidate).Map(valid => new Shape(inner: valid, owned: owned.Filter(owner => ReferenceEquals(objA: owner, objB: valid)))),
                object candidate => Fin.Fail<Shape>(new UnsupportedSource(Port: nameof(Shape), SourceType: candidate.GetType(), Hint: Accepted)),
            });
    internal static Option<Shape> Converted(object raw, GeometryBase? value) =>
        Optional(value).Bind(converted => Create(value: converted, owned: ReferenceEquals(objA: raw, objB: converted) ? Option<IDisposable>.None : Some((IDisposable)converted)).ToOption());
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
    internal static Fin<Seq<Flow<Shape>>> ReadShape(this IDataAccess access, int slot, Port port) {
        ArgumentNullException.ThrowIfNull(argument: access);
        ArgumentNullException.ThrowIfNull(argument: port);
        return Read<object>(access: access, slot: slot, port: port)
            .Bind(values => values.TraverseM(sourced => NormalizeFlow(access: access, source: sourced, port: port)).As());
    }
    internal static Fin<Seq<Flow<TVal>>> Read<TVal>(this IDataAccess access, int slot, Port port) {
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
    private static Fin<Seq<Flow<TVal>>> ReadNative<TVal>(IDataAccess access, int slot, Port port) =>
        port.Access switch {
            Access.Item => access.GetPears<TVal>(index: slot, pears: out Pear<TVal>[] pears) switch {
                true => FlowPears(port: port, pears: pears.Select(static pear => (Pear: pear, Site: Option<Site>.None))),
                _ => MissingFlow<TVal>(port: port),
            },
            Access.Twig => access.GetTwig<TVal>(index: slot, twig: out Twig<TVal> twig) switch {
                true => FlowPears(port: port, pears: twig.Pears.Select(static pear => (Pear: pear, Site: Option<Site>.None))),
                _ => MissingFlow<TVal>(port: port),
            },
            Access.Tree => access.GetTree<TVal>(index: slot, tree: out Tree<TVal> tree) switch {
                true => FlowPears(port: port, pears: tree.EnumerateLeaves().Select(static leaf => (leaf.Pear, Site: Some(leaf.Site)))),
                _ => MissingFlow<TVal>(port: port),
            },
            _ => Fin.Fail<Seq<Flow<TVal>>>(new UnsupportedAccess(Access: port.Access.ToString())),
        };
    private static Unit WriteNative<TOut>(IDataAccess access, int slot, string name, Access targetAccess, Seq<Flow<TOut>> values) =>
        targetAccess switch {
            Access.Item => values.Count switch {
                > 0 => Effect(action: () => access.SetPear(index: slot, pear: values[0].Pear)),
                _ => Effect(action: () => access.SetPear(index: slot, pear: null!)),
            },
            Access.Twig => Effect(action: () => access.SetTwig(index: slot, twig: Garden.TwigFromPears(pears: values.Map(static value => value.Pear).AsIterable()))),
            Access.Tree => Effect(action: () => {
                ITree tree = values.Count switch {
                    0 => Garden.TreeEmpty<TOut>(),
                    _ when values.Exists(static value => value.Site.IsSome) => Garden.TreeFromLeaves(leaves: Leaves(values: values).AsIterable()),
                    _ => Garden.TreeFromPears(pears: values.Map(static value => value.Pear).AsIterable()),
                };
                access.SetTree(index: slot, tree: TreePrefix(access: access, slot: slot) is int prefix ? tree.WithPathPrefix(element: prefix) : tree);
            }),
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
    private static Fin<Analyze.Scope> Remark(IDataAccess access, Rhino.UnitSystem units) {
        access.AddRemark(text: "Tolerance", details: "Host did not supply reliable tolerance; using default tolerance with document units.");
        return Fin.Succ(Analyze.In(units: units == Rhino.UnitSystem.CustomUnits ? Rhino.UnitSystem.Millimeters : units));
    }
    private static Fin<Seq<Pear<TVal>>> Missing<TVal>(Port port) =>
        port.Requirement switch {
            Grasshopper2.Parameters.Requirement.MayBeMissing => Fin.Succ(Seq<Pear<TVal>>()),
            _ => Fin.Fail<Seq<Pear<TVal>>>(new MissingPortInput(Port: port.Name)),
        };
    private static Fin<Seq<Flow<TVal>>> MissingFlow<TVal>(Port port) =>
        Missing<TVal>(port: port).Map(static pears => pears.Map(static pear => new Flow<TVal>(Pear: pear, Site: Option<Site>.None)));
    private static Seq<Leaf<T>> Leaves<T>(Seq<Flow<T>> values) =>
        values.Map((value, index) => new Leaf<T>(pear: value.Pear, site: value.Site.IfNone(new Site(path: new Grasshopper2.Data.Path(0), item: index))));
    private readonly record struct Broker(int Priority, Func<object, Option<Shape>> Convert);
    private static readonly Seq<Broker> Brokers = toSeq(new Broker[] {
        new(Priority: 100, Convert: static raw => AsShape(value: raw)),
        new(Priority:  90, Convert: static raw => Shape.Converted(raw: raw, value: CurveBroker.ToRhinoCurve(raw))),
        new(Priority:  90, Convert: static raw => Shape.Converted(raw: raw, value: SurfaceBroker.ToBrep(raw))),
    }.OrderByDescending(static b => b.Priority));
    private static Option<Shape> NormalizeShape(object raw) => Brokers.Choose(broker => broker.Convert(arg: raw)).Head;
    private static Option<Shape> AsShape(object? value) => Optional(value).Bind(static candidate => Shape.Create(value: candidate).ToOption());
    private static Fin<Flow<Shape>> NormalizeFlow(IDataAccess access, Flow<object> source, Port port) =>
        from scale in UnitScale(access: access)
        from shape in NormalizeShape(raw: source.Item).ToFin(new UnsupportedSource(Port: port.Name, SourceType: source.Item.GetType(), Hint: Shape.Accepted))
        from result in scale switch {
            double factor when Math.Abs(value: factor - 1.0) <= Rhino.RhinoMath.ZeroTolerance => Fin.Succ(source.Project(item: shape)),
            double factor => ScaleShape(access: access, source: source, shape: shape, port: port, factor: factor),
        }
        select result;
    private static Fin<Flow<Shape>> ScaleShape(IDataAccess access, Flow<object> source, Shape shape, Port port, double factor) {
        Pear<object> pear = Pear<object>.Create(item: shape.Inner, meta: source.Meta);
        return Optional(access.TryTransform(value: pear, transformation: Transform.Scale(anchor: Point3d.Origin, scaleFactor: factor))).ToFin(new Fault.ComputationFailed(Label: "UnitScaling"))
            .Bind(transformed => ReferenceEquals(objA: transformed, objB: pear) switch {
                true => Fin.Fail<IPear>(new Fault.ComputationFailed(Label: "UnitScaling")),
                false => Fin.Succ(transformed),
            })
            .Bind(transformed => shape.DisposeUnlessTransferred(outputs: Seq(transformed.Item)) switch {
                _ => NormalizeShape(raw: transformed.Item)
                .ToFin(new UnsupportedSource(Port: port.Name, SourceType: transformed.Item.GetType(), Hint: Shape.Accepted))
                .Map(scaled => new Flow<Shape>(Pear: Pear<Shape>.Create(item: scaled, meta: transformed.Meta), Site: source.Site)),
            });
    }
    private static Fin<double> UnitScale(IDataAccess access) =>
        (access.GetUnitScaling(unitSystemScaling: out double scale), scale) switch {
            (true, double factor) when Rhino.RhinoMath.IsValidDouble(x: factor) => Fin.Succ(factor),
            (false, _) => Fin.Succ(1.0),
            _ => Fin.Fail<double>(new Fault.ComputationFailed(Label: "UnitScaling")),
        };
    internal sealed class Progress(IDataAccess access) : IProgress<double> {
        public void Report(double value) => access.SetProgress(percentage: (int)Rhino.RhinoMath.Clamp(value: value switch {
            >= 0.0 and <= 1.0 => value * 100.0,
            _ => value,
        }, 0.0, 100.0));
    }
    private static Fin<Seq<Flow<T>>> FlowPears<T>(Port port, IEnumerable<(Pear<T> Pear, Option<Site> Site)> pears) {
        Seq<(Pear<T> Pear, Option<Site> Site, int Index)> indexed = toSeq(pears.Select((pear, index) => (pear.Pear, pear.Site, Index: index)));
        Pear<T>[] raw = [.. indexed.Map(static item => item.Pear).AsIterable()];
        bool[] nulls = ArrayEx.ToNullArray(raw);
        return (indexed.Count, ArrayEx.AnyNull(raw)) switch {
            ( > 0, false) => Fin.Succ(indexed.Map(static item => new Flow<T>(Pear: item.Pear, Site: item.Site))),
            ( > 0, true) => Fin.Fail<Seq<Flow<T>>>(new MissingPortInput(Port: port.Name, Hint: $"Null item at index {System.Array.FindIndex(array: nulls, match: static value => value)}.")),
            _ => MissingFlow<T>(port: port),
        };
    }
}
