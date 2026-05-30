using Foundation.CSharp.Analyzers.Contracts;
using Grasshopper2.Extensions;
using Grasshopper2.Types.Conversion;

namespace Rasm.Grasshopper.Components;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<string>]
internal sealed partial class Severity {
    private delegate Unit EmitDiagnostic(IDataAccess access, string text, string details);
    public static readonly Severity Remark = new(key: nameof(Remark), emit: EmitRemark);
    public static readonly Severity Warning = new(key: nameof(Warning), emit: EmitWarning);
    public static readonly Severity Error = new(key: nameof(Error), emit: EmitError);
    [UseDelegateFromConstructor] internal partial Unit Emit(IDataAccess access, string text, string details);
    private static Unit EmitRemark(IDataAccess access, string text, string details) {
        access.AddRemark(text: text, details: details);
        return unit;
    }
    private static Unit EmitWarning(IDataAccess access, string text, string details) {
        access.AddWarning(text: text, details: details);
        return unit;
    }
    private static Unit EmitError(IDataAccess access, string text, string details) {
        access.AddError(text: text, details: details);
        return unit;
    }
}

// --- [MODELS] -----------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
public readonly record struct Shape {
    private readonly Option<IDisposable> owned;
    private Shape(object inner, Option<IDisposable> owned) { Inner = inner; this.owned = owned; }
    public object Inner { get; }
    public const string Accepted = "Rhino/GH geometry convertible through native RhinoCommon or GH2 brokers";
    internal Unit DisposeUnlessTransferred(Seq<object> outputs) =>
        owned.Filter(disposable => !outputs.Exists(output => ReferenceEquals(objA: output, objB: disposable) || output switch {
            TopologyProjection projection => projection.Transfers(output: disposable),
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
                object candidate when Domain.Kind.Of(type: candidate.GetType()).IsSome => Op.Create(value: nameof(Shape)).AcceptValue(value: candidate).Map(valid => new Shape(inner: valid, owned: owned.Filter(owner => ReferenceEquals(objA: owner, objB: valid)))),
                object candidate => Fin.Fail<Shape>(new UnsupportedSource(Port: nameof(Shape), SourceType: candidate.GetType(), Hint: Accepted)),
            });
    internal static Option<Shape> Converted(object raw, GeometryBase? value) =>
        Optional(value).Bind(converted => {
            Option<IDisposable> lease = ReferenceEquals(objA: raw, objB: converted) ? Option<IDisposable>.None : Some((IDisposable)converted);
            return Create(value: converted, owned: lease).Match(
                Succ: Some,
                Fail: _ => lease.Iter(static disposable => disposable.Dispose()) switch { _ => Option<Shape>.None });
        });
    internal static Option<Shape> From(object raw) => Brokers.Choose(convert => convert(arg: raw)).Head;
    public static Unit RegisterBroker<T>(Func<T, Option<object>> convert) where T : class {
        ArgumentNullException.ThrowIfNull(argument: convert);
        _ = Extensions.Swap(brokers => brokers.Add(value: raw => raw is T typed ? convert(arg: typed).Bind(candidate => Create(value: candidate).ToOption()) : Option<Shape>.None));
        return unit;
    }
    private static readonly Atom<Seq<Func<object, Option<Shape>>>> Extensions = Atom(value: Seq<Func<object, Option<Shape>>>());
    private static Seq<Func<object, Option<Shape>>> Brokers =>
        Seq<Func<object, Option<Shape>>>(
            static raw => AsShape(value: raw),
            static raw => Converted(raw: raw, value: CurveBroker.ToRhinoCurve(raw)),
            static raw => SurfaceShape(raw: raw),
            static raw => ConversionShape(raw: raw)) + Extensions.Value;
    private static Option<Shape> AsShape(object? value) => Optional(value).Bind(static candidate => Create(value: candidate).ToOption());
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000", Justification = "Shape owns converted Rhino geometry and disposes it after output transfer.")]
    private static Option<Shape> SurfaceShape(object raw) =>
        SurfaceBroker.CastOrConvert(data: raw, p1: out Surface surface, p3: out Brep brep, p4: out SubD subd) switch {
            SurfaceLikeType.Surf => Converted(raw: raw, value: surface),
            SurfaceLikeType.Brep => Converted(raw: raw, value: brep),
            SurfaceLikeType.SubD => Converted(raw: raw, value: subd),
            _ => Option<Shape>.None,
        };
    private static Option<Shape> ConversionShape(object raw) =>
        ConversionServer.Convert(source: raw, targetType: typeof(GeometryBase), target: out object converted) switch {
            true when converted is GeometryBase geometry => Converted(raw: raw, value: geometry),
            _ => Option<Shape>.None,
        };
}
[StructLayout(LayoutKind.Auto)]
internal readonly record struct Flow<T>(Pear<T> Pear, Option<Site> Site) {
    public T Item => Pear.Item;
    public MetaData Meta => Pear.Meta;
    internal Flow<TOut> Project<TOut>(TOut item) => new(Pear: Pear<TOut>.Create(item: item, meta: Meta), Site: Site);
    internal Flow<TOut> Project<TOut>(TOut item, int index) => new(Pear: Pear<TOut>.Create(item: item, meta: Meta), Site: Site.Map(site => new Site(path: site.Path.AppendElement(site.Item), item: index)));
    internal Seq<Flow<TOut>> Project<TOut>(Seq<TOut> items) {
        Flow<T> source = this;
        return items.Count == 1
            ? items.Map(value => source.Project(item: value))
            : items.Map((value, index) => source.Project(item: value, index: index));
    }
}
internal readonly record struct TolerancePolicy(double Absolute, double Relative, double Angle, bool Reliable) {
    internal static TolerancePolicy Read(IDataAccess access) {
        bool lengthReliable = access.GetTolerance(absoluteTolerance: out double absolute, relativeTolerance: out double relative);
        bool angleReliable = access.GetTolerance(angularTolerance: out Angle angle);
        return new(Absolute: absolute, Relative: relative, Angle: angle.Radians, Reliable: lengthReliable && angleReliable);
    }
}
internal readonly record struct UnitPolicy(Rhino.UnitSystem System, Fin<double> Scale) {
    internal static UnitPolicy Read(IDataAccess access) {
        _ = access.GetUnitSystem(unitSystem: out UnitSystem units);
        Rhino.UnitSystem system = units.System switch {
            Rhino.UnitSystem.Unset or Rhino.UnitSystem.None => Rhino.UnitSystem.Millimeters,
            Rhino.UnitSystem known => known,
        };
        return new(System: system, Scale: (access.GetUnitScaling(unitSystemScaling: out double scale), scale) switch {
            (true, double factor) when Rhino.RhinoMath.IsValidDouble(x: factor) && factor > Rhino.RhinoMath.ZeroTolerance => Fin.Succ(factor),
            (false, _) => Fin.Succ(1.0),
            _ => Fin.Fail<double>(new Fault.ComputationFailed(Label: "UnitScaling")),
        });
    }
}

// --- [ERRORS] -----------------------------------------------------------------------------
[BoundaryAdapter]
internal sealed record MissingPortInput(string Port, string? Hint = null) : Domain.Expected {
    public override string Message => Hint switch { string h => $"{Port} input is required. Connect: {h}.", _ => $"{Port} input is required." };
    public override string Category => "Input";
}
[BoundaryAdapter]
internal sealed record UnsupportedSource(string Port, Type SourceType, string? Hint = null) : Domain.Expected {
    public override string Message => Hint switch { string h => $"{Port} input type '{SourceType.Name}' is not supported. Connect: {h}.", _ => $"{Port} input type '{SourceType.Name}' is not supported." };
    public override string Category => "Input";
}
[BoundaryAdapter]
internal sealed record UnsupportedAccess(string Access) : Domain.Expected {
    public override string Message => $"Unsupported input access: {Access}.";
    public override string Category => "Access";
}
[BoundaryAdapter]
internal sealed record InvalidPortValue(string Port, string Detail) : Domain.Expected {
    public override string Message => $"{Port} value rejected: {Detail}.";
    public override string Category => "Validation";
}

// --- [SERVICES] ---------------------------------------------------------------------------
[BoundaryAdapter]
internal static class Bridge {
    internal static Fin<Analyze.Scope> Scope(this IDataAccess access) {
        ArgumentNullException.ThrowIfNull(argument: access);
        UnitPolicy units = UnitPolicy.Read(access: access);
        TolerancePolicy tolerance = TolerancePolicy.Read(access: access);
        return Analyze.In(absolute: tolerance.Absolute, relative: tolerance.Relative, angle: tolerance.Angle, units: units.System).Context.Match(
            Succ: context => {
                _ = tolerance.Reliable switch {
                    true => Unit.Default,
                    false => access.Emit(severity: Severity.Remark, text: "Tolerance", details: "Host supplied fallback tolerance values; using them because they are valid."),
                };
                return Fin.Succ(Analyze.In(context: context));
            },
            Fail: _ => Remark(access: access, units: units.System));
    }
    internal static Fin<Seq<Flow<Shape>>> ReadShape(this IDataAccess access, int slot, Port port) {
        ArgumentNullException.ThrowIfNull(argument: access);
        ArgumentNullException.ThrowIfNull(argument: port);
        return from factor in UnitPolicy.Read(access: access).Scale
               from values in Read<object>(access: access, slot: slot, port: port)
               from normalized in values.TraverseM(sourced => NormalizeFlow(access: access, source: sourced, port: port, factor: factor)).As()
               select normalized;
    }
    internal static Fin<Seq<Flow<TVal>>> Read<TVal>(this IDataAccess access, int slot, Port port) {
        ArgumentNullException.ThrowIfNull(argument: access);
        ArgumentNullException.ThrowIfNull(argument: port);
        return (port.Kind.RequiresWire<TVal>()
            ? Channel<int>.For(access: port.Access).Read(access: access, slot: slot, port: port).Bind(wired => wired.TraverseM(port.Kind.Decode<TVal>).As())
            : Channel<TVal>.For(access: port.Access).Read(access: access, slot: slot, port: port))
            .Bind(values => Validate(values: values, port: port));
    }
    internal static Seq<object> Write<TOut>(this IDataAccess access, int slot, Port<TOut> port, Seq<Flow<TOut>> values) =>
        port.Kind.RequiresWire<TOut>()
            ? Channel<int>.For(access: port.Access).Write(access: access, slot: slot, name: port.Name, values: values.Map(static value => PortKind.Encode(value)))
            : Channel<TOut>.For(access: port.Access).Write(access: access, slot: slot, name: port.Name, values: values);
    private static Fin<Seq<Flow<TVal>>> Validate<TVal>(Seq<Flow<TVal>> values, Port port) =>
        port.Policy.Validators.IsEmpty
            ? Fin.Succ(values)
            : values.TraverseM(value => port.Policy.Validators
                .Find(predicate: rule => !rule.Predicate(arg: value.Item!))
                .Map(static rule => rule.Message) switch {
                    { IsSome: true, Case: string message } => Fin.Fail<Flow<TVal>>(new InvalidPortValue(Port: port.Name, Detail: message)),
                    _ => Fin.Succ(value),
                }).As();
    internal abstract record Channel<T> {
        internal abstract Fin<Seq<Flow<T>>> Read(IDataAccess access, int slot, Port port);
        internal abstract Seq<object> Write(IDataAccess access, int slot, string name, Seq<Flow<T>> values);
        internal static Channel<T> For(Access access) => access switch {
            Access.Item => ItemChannel.Instance,
            Access.Twig => TwigChannel.Instance,
            Access.Tree => TreeChannel.Instance,
            _ => UnsupportedChannel.Instance,
        };
        internal sealed record ItemChannel : Channel<T> {
            internal static readonly ItemChannel Instance = new();
            internal override Fin<Seq<Flow<T>>> Read(IDataAccess access, int slot, Port port) =>
                access.GetPears(index: slot, pears: out Pear<T>[] pears)
                    ? FlowPears(port: port, pears: pears.Select(static pear => (Pear: pear, Site: Option<Site>.None)))
                    : MissingFlow<T>(port: port);
            internal override Seq<object> Write(IDataAccess access, int slot, string name, Seq<Flow<T>> values) =>
                values.Count switch {
                    1 => Effect(action: () => access.SetPear(index: slot, pear: values[0].Pear)) switch { _ => values.Map(static value => (object)value.Item!) },
                    > 1 => access.Emit(severity: Severity.Error, text: name, details: $"Item output received {values.Count} values; use twig or tree access.") switch { _ => Seq<object>() },
                    _ => Seq<object>(),
                };
        }
        internal sealed record TwigChannel : Channel<T> {
            internal static readonly TwigChannel Instance = new();
            internal override Fin<Seq<Flow<T>>> Read(IDataAccess access, int slot, Port port) =>
                access.GetTwig(index: slot, twig: out Twig<T> twig)
                    ? FlowPears(port: port, pears: twig.Pears.Select(static pear => (Pear: pear, Site: Option<Site>.None)))
                    : MissingFlow<T>(port: port);
            internal override Seq<object> Write(IDataAccess access, int slot, string name, Seq<Flow<T>> values) =>
                Effect(action: () => access.SetTwig(index: slot, twig: Garden.TwigFromPears(pears: values.Map(static value => value.Pear).AsIterable()))) switch { _ => values.Map(static value => (object)value.Item!) };
        }
        internal sealed record TreeChannel : Channel<T> {
            internal static readonly TreeChannel Instance = new();
            internal override Fin<Seq<Flow<T>>> Read(IDataAccess access, int slot, Port port) =>
                access.GetTree(index: slot, tree: out Tree<T> tree)
                    ? FlowPears(port: port, pears: tree.EnumerateLeaves().Select(static leaf => (leaf.Pear, Site: Some(leaf.Site))))
                    : MissingFlow<T>(port: port);
            internal override Seq<object> Write(IDataAccess access, int slot, string name, Seq<Flow<T>> values) =>
                Effect(action: () => {
                    ITree tree = values.Count switch {
                        0 => Garden.TreeEmpty<T>(),
                        _ when values.Exists(static value => value.Site.IsSome) => Garden.TreeFromLeaves(leaves: Leaves(values: values).AsIterable()),
                        _ => Garden.TreeFromPears(pears: values.Map(static value => value.Pear).AsIterable()),
                    };
                    access.SetTree(index: slot, tree: TreePrefix(access: access, slot: slot) is int prefix ? tree.WithPathPrefix(element: prefix) : tree);
                }) switch { _ => values.Map(static value => (object)value.Item!) };
        }
        internal sealed record UnsupportedChannel : Channel<T> {
            internal static readonly UnsupportedChannel Instance = new();
            internal override Fin<Seq<Flow<T>>> Read(IDataAccess access, int slot, Port port) =>
                Fin.Fail<Seq<Flow<T>>>(new UnsupportedAccess(Access: port.Access.ToString()));
            internal override Seq<object> Write(IDataAccess access, int slot, string name, Seq<Flow<T>> values) =>
                access.Emit(severity: Severity.Error, text: name, details: "Unsupported output access.") switch { _ => Seq<object>() };
        }
    }
    private static Unit Effect(Action action) {
        action();
        return Unit.Default;
    }
    internal static Unit Emit(this IDataAccess access, Severity severity, string text, string details) =>
        severity.Emit(access: access, text: text, details: details);
    private static int? TreePrefix(IDataAccess access, int slot) =>
        access.CoverageOut(index: slot) switch { { TwigIndex: >= 0 } coverage => coverage.TwigIndex, _ => null };
    private static Fin<Analyze.Scope> Remark(IDataAccess access, Rhino.UnitSystem units) =>
        access.Emit(severity: Severity.Remark, text: "Tolerance", details: "Host did not supply reliable tolerance; using default tolerance with document units.") switch {
            _ => Fin.Succ(Analyze.In(units: units == Rhino.UnitSystem.CustomUnits ? Rhino.UnitSystem.Millimeters : units)),
        };
    private static Fin<Seq<Pear<TVal>>> Missing<TVal>(Port port) =>
        port.Requirement switch {
            Grasshopper2.Parameters.Requirement.MayBeMissing => Fin.Succ(Seq<Pear<TVal>>()),
            _ => Fin.Fail<Seq<Pear<TVal>>>(new MissingPortInput(Port: port.Name)),
        };
    private static Fin<Seq<Flow<TVal>>> MissingFlow<TVal>(Port port) =>
        Missing<TVal>(port: port).Map(static pears => pears.Map(static pear => new Flow<TVal>(Pear: pear, Site: Option<Site>.None)));
    private static Seq<Leaf<T>> Leaves<T>(Seq<Flow<T>> values) =>
        values.Map((value, index) => new Leaf<T>(pear: value.Pear, site: value.Site.IfNone(new Site(path: new Grasshopper2.Data.Path(0), item: index))));
    private static Fin<Flow<Shape>> NormalizeFlow(IDataAccess access, Flow<object> source, Port port, double factor) =>
        Optional(source.Item)
            .ToFin(new MissingPortInput(Port: port.Name, Hint: Shape.Accepted))
            .Bind(raw =>
                from shape in Shape.From(raw: raw).ToFin(new UnsupportedSource(Port: port.Name, SourceType: raw.GetType(), Hint: Shape.Accepted))
                from result in factor switch {
                    double scale when Math.Abs(value: scale - 1.0) <= Rhino.RhinoMath.ZeroTolerance => Fin.Succ(source.Project(item: shape)),
                    _ => ScaleShape(access: access, source: source, shape: shape, factor: factor),
                }
                select result);
    private static Fin<Flow<Shape>> ScaleShape(IDataAccess access, Flow<object> source, Shape shape, double factor) {
        Pear<object> pear = Pear<object>.Create(item: shape.Inner, meta: source.Meta);
        return Optional(access.TryTransform(value: pear, transformation: Transform.Scale(anchor: Point3d.Origin, scaleFactor: factor))).ToFin(new Fault.ComputationFailed(Label: "UnitScaling"))
            .Bind(transformed => ReferenceEquals(objA: transformed, objB: pear) switch {
                true => Fin.Fail<IPear>(new Fault.ComputationFailed(Label: "UnitScaling")),
                false => Fin.Succ(transformed),
            })
            .Bind(transformed => shape.DisposeUnlessTransferred(outputs: Seq(transformed.Item)) switch {
                _ => Optional(transformed.Item as GeometryBase)
                    .ToFin(new Fault.ComputationFailed(Label: "UnitScaling"))
                    .Bind(geometry => Shape.Converted(raw: shape.Inner, value: geometry)
                        .ToFin(new Fault.ComputationFailed(Label: "UnitScaling")))
                    .Map(scaled => new Flow<Shape>(Pear: Pear<Shape>.Create(item: scaled, meta: transformed.Meta), Site: source.Site)),
            });
    }
    internal sealed class Progress(IDataAccess access) : IProgress<double> {
        public void Report(double value) => access.SetProgress(percentage: (int)Rhino.RhinoMath.Clamp(value: value switch {
            >= 0.0 and <= 1.0 => value * 100.0,
            _ => value,
        }, 0.0, 100.0));
    }
    private static Fin<Seq<Flow<T>>> FlowPears<T>(Port port, IEnumerable<(Pear<T> Pear, Option<Site> Site)> pears) {
        Seq<(Pear<T> Pear, Option<Site> Site, int Index)> indexed = toSeq(pears.Select((pear, index) => (pear.Pear, pear.Site, Index: index)));
        Pear<T>[] raw = [.. indexed.Map(static item => item.Pear).AsIterable()];
        return (indexed.Count, ArrayEx.AnyNull(raw)) switch {
            ( > 0, false) => Fin.Succ(indexed.Map(static item => new Flow<T>(Pear: item.Pear, Site: item.Site))),
            ( > 0, true) => indexed.Filter(static item => item.Pear is not null) switch {
                Seq<(Pear<T> Pear, Option<Site> Site, int Index)> found when !found.IsEmpty => Fin.Succ(found.Map(static item => new Flow<T>(Pear: item.Pear, Site: item.Site))),
                _ => MissingFlow<T>(port: port),
            },
            _ => MissingFlow<T>(port: port),
        };
    }
}
