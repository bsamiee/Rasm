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
    public const string Accepted = "Rhino/GH geometry convertible through native RhinoCommon or GH2 brokers";
    private static readonly Atom<Seq<Func<object, Option<Shape>>>> Extensions = Atom(value: Seq<Func<object, Option<Shape>>>());
    private readonly Option<IDisposable> owned;
    private Shape(object inner, Option<IDisposable> owned) { Inner = inner; this.owned = owned; }
    public object Inner { get; }
    internal static Fin<Shape> Create(object? value) => Create(value: value, owned: Option<IDisposable>.None);
    private static Fin<Shape> Create(object? value, Option<IDisposable> owned) =>
        Optional(value)
            .ToFin(new PortFault.MissingInput(Port: nameof(Shape), Hint: Some(Accepted)))
            .Bind(raw => raw switch {
                Shape shape => Fin.Succ(shape),
                object candidate when Domain.Kind.Of(type: candidate.GetType()).IsSome => Op.Create(value: nameof(Shape)).AcceptValue(value: candidate).Map(valid => new Shape(inner: valid, owned: owned.Filter(owner => ReferenceEquals(objA: owner, objB: valid)))),
                object candidate => Fin.Fail<Shape>(new PortFault.UnsupportedSource(Port: nameof(Shape), SourceType: candidate.GetType(), Hint: Some(Accepted))),
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
    internal Unit DisposeUnlessTransferred(Seq<object> outputs) =>
        owned.Filter(disposable => !outputs.Exists(output => ReferenceEquals(objA: output, objB: disposable) || output switch {
            TopologyProjection projection => projection.Transfers(output: disposable),
            GeometryProjection projection => projection.Transfers(output: disposable),
            BrepFace face => ReferenceEquals(objA: face.Brep, objB: disposable),
            _ => false,
        }))
            .Iter(static disposable => disposable.Dispose());
    internal Flow<TSource> Detach<TSource>(Flow<TSource> output) =>
        (Inner, output.Item) switch {
            (GeometryBase source, TopologyProjection projection) => output.Project(item: (TSource)(object)projection.DetachFrom(source: source)),
            (GeometryBase source, GeometryProjection projection) => output.Project(item: (TSource)(object)projection.DetachFrom(source: source)),
            _ => output,
        };
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
        // Invalid unit-scaling pins self-heal to 1.0, preserving shape reads.
        Fin<double> scale = (access.GetUnitScaling(baseSystem: system, unitSystemScaling: out double factor), factor) switch {
            (true, double valid) when Rhino.RhinoMath.IsValidDouble(x: valid) && valid > Rhino.RhinoMath.ZeroTolerance => Fin.Succ(valid),
            (false, _) => Fin.Succ(1.0),
            _ => access.Emit(severity: Severity.Warning, text: "UnitScaling", details: string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"Invalid unit scaling factor {factor}; using 1.0.")) switch { _ => Fin.Succ(1.0) },
        };
        return new(System: system, Scale: scale);
    }
}

// --- [ERRORS] -----------------------------------------------------------------------------
[Union]
[BoundaryAdapter]
internal abstract partial record PortFault : Domain.Expected {
    private PortFault() { }
    public sealed record MissingInput(string Port, Option<string> Hint) : PortFault;
    public sealed record UnsupportedSource(string Port, Type SourceType, Option<string> Hint) : PortFault;
    public sealed record UnsupportedAccess(string Access) : PortFault;
    public sealed record InvalidValue(string Port, string Detail) : PortFault;
    public override string Category => Switch(
        missingInput: static _ => "Input",
        unsupportedSource: static _ => "Input",
        unsupportedAccess: static _ => "Access",
        invalidValue: static _ => "Validation");
    public override string Message => Switch(
        missingInput: static f => f.Hint.Match(Some: h => $"{f.Port} input is required. Connect: {h}.", None: () => $"{f.Port} input is required."),
        unsupportedSource: static f => f.Hint.Match(Some: h => $"{f.Port} input type '{f.SourceType.Name}' is not supported. Connect: {h}.", None: () => $"{f.Port} input type '{f.SourceType.Name}' is not supported."),
        unsupportedAccess: static f => $"Unsupported input access: {f.Access}.",
        invalidValue: static f => $"{f.Port} value rejected: {f.Detail}.");
    // Wiring faults are recoverable; compute/scope faults stay errors.
    internal static Severity SeverityOf(Error error) =>
        error switch {
            MissingInput or UnsupportedSource or UnsupportedAccess => Severity.Warning,
            _ => Severity.Error,
        };
}

// --- [SERVICES] ---------------------------------------------------------------------------
[BoundaryAdapter]
internal static partial class Bridge {
    internal static Unit Emit(this IDataAccess access, Severity severity, string text, string details) =>
        severity.Emit(access: access, text: text, details: details);
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
    internal static Fin<Seq<Flow<TVal>>> Read<TVal>(this IDataAccess access, int slot, Port port) {
        ArgumentNullException.ThrowIfNull(argument: access);
        ArgumentNullException.ThrowIfNull(argument: port);
        return (port.Kind.RequiresWire<TVal>()
            ? Channel<int>.For(access: port.Access).Read(access: access, slot: slot, port: port).Bind(wired => wired.TraverseM(port.Kind.Decode<TVal>).As())
            : Channel<TVal>.For(access: port.Access).Read(access: access, slot: slot, port: port))
            .Bind(values => Validate(values: values, port: port));
    }
    internal static Fin<Seq<Flow<Shape>>> ReadShape(this IDataAccess access, int slot, Port port) {
        ArgumentNullException.ThrowIfNull(argument: access);
        ArgumentNullException.ThrowIfNull(argument: port);
        return from factor in UnitPolicy.Read(access: access).Scale
               from values in Read<object>(access: access, slot: slot, port: port)
               from normalized in values.TraverseM(sourced => NormalizeFlow(access: access, source: sourced, port: port, factor: factor)).As()
               select normalized;
    }
    internal static Seq<object> Write<TOut>(this IDataAccess access, int slot, Port<TOut> port, Seq<Flow<TOut>> values) =>
        port.Kind.RequiresWire<TOut>()
            ? Channel<int>.For(access: port.Access).Write(access: access, slot: slot, name: port.Name, values: values.Map(static value => PortKind.Encode(value)))
            : Channel<TOut>.For(access: port.Access).Write(access: access, slot: slot, name: port.Name, values: values);
    internal sealed class Progress(IDataAccess access) : IProgress<double> {
        public void Report(double value) => access.SetProgress(percentage: (int)Rhino.RhinoMath.Clamp(value: value * 100.0, 0.0, 100.0));
    }
    [Union(SwitchMapStateParameterName = "context")]
    internal abstract partial record Channel<T> {
        private Channel() { }
        internal sealed record ItemChannel : Channel<T> {
            internal static readonly ItemChannel Instance = new();
        }
        internal sealed record TwigChannel : Channel<T> {
            internal static readonly TwigChannel Instance = new();
        }
        internal sealed record TreeChannel : Channel<T> {
            internal static readonly TreeChannel Instance = new();
        }
        internal sealed record UnsupportedChannel : Channel<T> {
            internal static readonly UnsupportedChannel Instance = new();
        }
        internal static Channel<T> For(Access access) => access switch {
            Access.Item => ItemChannel.Instance,
            Access.Twig => TwigChannel.Instance,
            Access.Tree => TreeChannel.Instance,
            _ => UnsupportedChannel.Instance,
        };
        internal Fin<Seq<Flow<T>>> Read(IDataAccess access, int slot, Port port) =>
            Switch(
                context: (Access: access, Slot: slot, Port: port),
                itemChannel: static (ctx, channel) => {
                    _ = ctx.Access.GetPears(index: ctx.Slot, pears: out Pear<T>[] pears);
                    return FlowPears(port: ctx.Port, pears: pears.Select(static pear => (Pear: pear, Site: Option<Site>.None)));
                },
                twigChannel: static (ctx, _) => ctx.Access.GetTwig(index: ctx.Slot, twig: out Twig<T> twig)
                    ? FlowPears(port: ctx.Port, pears: twig.Pears.Select(static pear => (Pear: pear, Site: Option<Site>.None)))
                    : MissingFlow(port: ctx.Port),
                treeChannel: static (ctx, _) => ctx.Access.GetTree(index: ctx.Slot, tree: out Tree<T> tree)
                    ? FlowPears(port: ctx.Port, pears: tree.EnumerateLeaves().Select(static leaf => (leaf.Pear, Site: Some(leaf.Site))))
                    : MissingFlow(port: ctx.Port),
                unsupportedChannel: static (ctx, _) => Fin.Fail<Seq<Flow<T>>>(new PortFault.UnsupportedAccess(Access: ctx.Port.Access.ToString())));
        internal Seq<object> Write(IDataAccess access, int slot, string name, Seq<Flow<T>> values) =>
            Switch(
                context: (Access: access, Slot: slot, Name: name, Values: values),
                itemChannel: static (ctx, _) => ctx.Values.Count switch {
                    1 => Commit(set: () => ctx.Access.SetPear(index: ctx.Slot, pear: ctx.Values[0].Pear), values: ctx.Values),
                    > 1 => ctx.Access.Emit(severity: Severity.Error, text: ctx.Name, details: $"Item output received {ctx.Values.Count} values; use twig or tree access.") switch { _ => Seq<object>() },
                    _ => Seq<object>(),
                },
                twigChannel: static (ctx, _) => Commit(set: () => ctx.Access.SetTwig(index: ctx.Slot, twig: Garden.TwigFromPears(pears: ctx.Values.Map(static value => value.Pear).AsIterable())), values: ctx.Values),
                treeChannel: static (ctx, _) => Commit(set: () => {
                    ITree tree = ctx.Values.Count switch {
                        0 => Garden.TreeEmpty<T>(),
                        _ when ctx.Values.Exists(static value => value.Site.IsSome) => Garden.TreeFromLeaves(leaves: Leaves(values: ctx.Values).AsIterable()),
                        _ => Garden.TreeFromPears(pears: ctx.Values.Map(static value => value.Pear).AsIterable()),
                    };
                    ctx.Access.SetTree(index: ctx.Slot, tree: ctx.Access.CoverageOut(index: ctx.Slot) switch { { TwigIndex: >= 0 } coverage => coverage.TwigIndex, _ => (int?)null } is int prefix ? tree.WithPathPrefix(element: prefix) : tree);
                }, values: ctx.Values),
                unsupportedChannel: static (ctx, _) => ctx.Access.Emit(severity: Severity.Error, text: ctx.Name, details: "Unsupported output access.") switch { _ => Seq<object>() });
        // Run the native Set side-effect, then project written values to transfer-evidence objects (one place).
        private static Seq<object> Commit(Action set, Seq<Flow<T>> values) {
            set();
            return values.Map(static value => (object)value.Item!);
        }
        private static Fin<Seq<Flow<T>>> MissingFlow(Port port) =>
            // GH2 contract: Process runs for MayBeNull/MayBeMissing even when the input is absent.
            (port.Requirement switch {
                Grasshopper2.Parameters.Requirement.MayBeMissing or Grasshopper2.Parameters.Requirement.MayBeNull => Fin.Succ(Seq<Pear<T>>()),
                _ => Fin.Fail<Seq<Pear<T>>>(new PortFault.MissingInput(Port: port.Name, Hint: None)),
            }).Map(static pears => pears.Map(static pear => new Flow<T>(Pear: pear, Site: Option<Site>.None)));
        private static Seq<Leaf<T>> Leaves(Seq<Flow<T>> values) =>
            values.Map((value, index) => new Leaf<T>(pear: value.Pear, site: value.Site.IfNone(new Site(path: new Grasshopper2.Data.Path(0), item: index))));
        private static Fin<Seq<Flow<T>>> FlowPears(Port port, IEnumerable<(Pear<T> Pear, Option<Site> Site)> pears) {
            Seq<(Pear<T> Pear, Option<Site> Site, int Index)> indexed = toSeq(pears.Select((pear, index) => (pear.Pear, pear.Site, Index: index)));
            Pear<T>[] raw = [.. indexed.Map(static item => item.Pear).AsIterable()];
            return (indexed.Count, ArrayEx.AnyNull(raw)) switch {
                ( > 0, false) => Fin.Succ(indexed.Map(static item => new Flow<T>(Pear: item.Pear, Site: item.Site))),
                ( > 0, true) => Fin.Fail<Seq<Flow<T>>>(new PortFault.InvalidValue(
                    Port: port.Name,
                    Detail: $"host returned null pear(s) at index {string.Join(separator: ',', values: indexed.Filter(static item => item.Pear is null).Map(static item => item.Index))}; flow cardinality was preserved by rejecting the read")),
                _ => MissingFlow(port: port),
            };
        }
    }
    private static Fin<Seq<Flow<TVal>>> Validate<TVal>(Seq<Flow<TVal>> values, Port port) =>
        port.Policy.Validators.IsEmpty
            ? Fin.Succ(values)
            : values.TraverseM(value => port.Policy.Validators
                .Find(predicate: rule => !rule.Predicate(arg: value.Item!))
                .Map(static rule => rule.Message) switch {
                    { IsSome: true, Case: string message } => Fin.Fail<Flow<TVal>>(new PortFault.InvalidValue(Port: port.Name, Detail: message)),
                    _ => Fin.Succ(value),
                }).As();
    private static Fin<Analyze.Scope> Remark(IDataAccess access, Rhino.UnitSystem units) =>
        access.Emit(severity: Severity.Remark, text: "Tolerance", details: "Host did not supply reliable tolerance; using default tolerance with document units.") switch {
            _ => Fin.Succ(Analyze.In(units: units == Rhino.UnitSystem.CustomUnits ? Rhino.UnitSystem.Millimeters : units)),
        };
    private static Fin<Flow<Shape>> NormalizeFlow(IDataAccess access, Flow<object> source, Port port, double factor) =>
        Optional(source.Item)
            .ToFin(new PortFault.MissingInput(Port: port.Name, Hint: Some(Shape.Accepted)))
            .Bind(raw =>
                from shape in Shape.From(raw: raw).ToFin(new PortFault.UnsupportedSource(Port: port.Name, SourceType: raw.GetType(), Hint: Some(Shape.Accepted)))
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
}
