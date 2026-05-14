namespace Rasm.Grasshopper;

// --- [TYPES] ----------------------------------------------------------------------------
public interface IOutputGroup {
    public Seq<IPort> Ports { get; }
    public Unit Run(IDataAccess access, Hints outputs, GrasshopperRuntime runtime);
    public Unit Empty(IDataAccess access, Hints outputs);
}

// --- [MODELS] ---------------------------------------------------------------------------
public readonly record struct Hints(Seq<(IPort Port, int Slot)> Inputs) {
    internal static Hints Capture(Seq<BoundPort> ports, Func<IParameter, int> index) =>
        new(Inputs: ports.Choose(bound => index(arg: bound.Parameter) switch {
            >= 0 and int slot => Some((bound.Port, slot)),
            _ => Option<(IPort Port, int Slot)>.None,
        }));
    public Option<int> Slot(IPort port) =>
        Inputs.Find(predicate: input => input.Port.Equals(port)).Map(static input => input.Slot);
}
public readonly record struct OutputSlot<TSource>(
    IPort Port,
    Func<IDataAccess, int, GrasshopperRuntime, Seq<Flow<TSource>>, Unit> Write,
    Func<IDataAccess, int, Unit> Empty);

// --- [SERVICES] -------------------------------------------------------------------------
public static class GrasshopperRuntimeExtensions {
    public static Fin<Option<TVal>> Read<TVal>(this GrasshopperRuntime runtime, Port<TVal> port) =>
        runtime.Hints.Slot(port: port).Match(
            Some: slot => Bridge.Read<TVal>(access: runtime.Access, slot: slot, port: port)
                .Map(values => values.Head.Map(static pear => pear.Item) | port.Fallback),
            None: () => Fin.Succ(port.Fallback));
    public static Option<TVal> ReadOrInvalid<TVal>(this GrasshopperRuntime runtime, Port<TVal> port, TVal invalid) =>
        runtime.Read(port: port).Match(Succ: static value => value, Fail: _ => Some(invalid));
    public static Option<int> Index(this GrasshopperRuntime runtime, Port<int> port, int limit) =>
        limit switch {
            <= 0 => Option<int>.None,
            _ => runtime.Hints.Slot(port: port)
                .Bind(slot => runtime.Access.GetIndex(indexParameter: slot, limit: limit, index: out int index) ? Some(index) : Option<int>.None)
                | port.Fallback,
        };
}
internal sealed record PreparedGroup<TSource>(
    Seq<OutputSlot<TSource>> Slots,
    Func<GrasshopperRuntime, Fin<Seq<Flow<TSource>>>> Source,
    Func<GrasshopperRuntime, bool> EmptyUnsupported,
    string AspectLabel) : IOutputGroup {
    public Seq<IPort> Ports => Slots.Map(static slot => slot.Port);
    public Unit Run(IDataAccess access, Hints outputs, GrasshopperRuntime runtime) {
        ArgumentNullException.ThrowIfNull(argument: access);
        return Source(arg: runtime).Match(
            Succ: values => values.IsEmpty switch {
                true => RemarkEmpty(access: access, outputs: outputs),
                false => Slots.Choose(output => outputs.Slot(port: output.Port).Map(slot => (output, slot)))
                    .Iter(pair => pair.output.Write(arg1: access, arg2: pair.slot, arg3: runtime, arg4: values)),
            },
            Fail: error => {
                _ = (EmptyUnsupported(arg: runtime), error) switch {
                    (true, Fault.Unsupported u) => RemarkUnsupported(access: access, fault: u),
                    _ => Warn(access: access, error: error),
                };
                _ = Empty(access: access, outputs: outputs);
                return Unit.Default;
            });
    }
    public Unit Empty(IDataAccess access, Hints outputs) {
        ArgumentNullException.ThrowIfNull(argument: access);
        return Slots.Choose(output => outputs.Slot(port: output.Port).Map(slot => (output, slot)))
            .Iter(pair => pair.output.Empty(arg1: access, arg2: pair.slot));
    }
    private Unit RemarkUnsupported(IDataAccess access, Fault.Unsupported fault) {
        access.AddRemark(text: Ports.Head.Map(static port => port.Name).IfNone("Output"), details: $"Unsupported source type '{fault.GeometryType.Name}' for aspect '{AspectLabel}'.");
        return Unit.Default;
    }
    private Unit Warn(IDataAccess access, Error error) {
        access.AddWarning(text: Ports.Head.Map(static port => port.Name).IfNone("Output"), details: error.Message);
        return Unit.Default;
    }
    private Unit RemarkEmpty(IDataAccess access, Hints outputs) {
        access.AddRemark(text: Ports.Head.Map(static port => port.Name).IfNone("Output"), details: "No result for sourced input.");
        return Empty(access: access, outputs: outputs);
    }
}
public static class Output {
    internal static OutputSlot<TSource> Slot<TSource, TOut>(
        Port<TOut> port,
        Func<GrasshopperRuntime, Seq<Flow<TSource>>, Fin<Seq<Flow<TOut>>>> project) =>
        new(
            Port: port,
            Write: (access, slot, runtime, source) => project(arg1: runtime, arg2: source).Match(
                Succ: values => access.Write<TOut>(slot: slot, name: port.Name, targetAccess: port.Access, values: values),
                Fail: error => {
                    access.AddWarning(text: port.Name, details: error.Message);
                    return access.Write<TOut>(slot: slot, name: port.Name, targetAccess: port.Access, values: Seq<Flow<TOut>>());
                }),
            Empty: (access, slot) => access.Write<TOut>(slot: slot, name: port.Name, targetAccess: port.Access, values: Seq<Flow<TOut>>()));
    public static OutputSlot<TSource> Plain<TSource, TOut>(Port<TOut> port, Func<TSource, TOut> project) =>
        Slot<TSource, TOut>(port: port, project: (_, sources) =>
            Fin.Succ(sources.Map(src => src.Project(item: project(arg: src.Item)))));
    public static OutputSlot<TSource> One<TSource, TOut>(Port<TOut> port, Func<TSource, Context, Fin<TOut>> project) =>
        Slot<TSource, TOut>(port: port, project: (runtime, sources) => runtime.Scope.Context
            .Bind(context => sources.Traverse(src => project(arg1: src.Item, arg2: context).Map(src.Project)).As()));
    public static OutputSlot<TSource> Many<TSource, TOut>(Port<TOut> port, Func<TSource, Fin<Seq<TOut>>> project) =>
        Slot<TSource, TOut>(port: port, project: (_, sources) => sources.Traverse(src =>
            project(arg: src.Item).Map(values => values.Map(src.Project)))
            .Map(static nested => nested.Bind(static x => x)).As());
    public static Unit Write(IDataAccess access, GrasshopperRuntime runtime, Seq<IOutputGroup> groups, Hints outputs) =>
        groups.Iter(group => group.Run(access: access, outputs: outputs, runtime: runtime));
    public static Unit Empty(IDataAccess access, Seq<IOutputGroup> groups, Hints outputs) =>
        groups.Iter(group => group.Empty(access: access, outputs: outputs));
    public static IOutputGroup Query<TAspect, TOut>(Port<Shape> input, Port<TOut> port, TAspect aspect) where TAspect : IAspect where TOut : notnull =>
        FromShapes(
            input: input,
            project: _ => shape => aspect.ToQuery<object, TOut>().Apply(geometry: shape.Inner),
            emptyUnsupported: static _ => true,
            aspectLabel: aspect.GetType().Name,
            slots: [Slot<TOut, TOut>(port: port, project: static (_, values) => Fin.Succ(values))]);
    public static IOutputGroup Query<TAspect, TOut>(Port<Shape> input, Port<TOut> port, Func<GrasshopperRuntime, TAspect> aspect) where TAspect : IAspect where TOut : notnull =>
        FromShapes(
            input: input,
            project: runtime => shape => aspect(arg: runtime).ToQuery<object, TOut>().Apply(geometry: shape.Inner),
            emptyUnsupported: static _ => true,
            aspectLabel: typeof(TAspect).Name,
            slots: [Slot<TOut, TOut>(port: port, project: static (_, values) => Fin.Succ(values))]);
    public static IOutputGroup Query<TOut>(Port<Shape> input, Port<TOut> port, Func<GrasshopperRuntime, Query<object, TOut>> aspect, bool emptyUnsupported = false, string aspectLabel = "Query") where TOut : notnull =>
        FromShapes(
            input: input,
            project: runtime => shape => aspect(arg: runtime).Apply(geometry: shape.Inner),
            emptyUnsupported: _ => emptyUnsupported,
            aspectLabel: aspectLabel,
            slots: [Slot<TOut, TOut>(port: port, project: static (_, values) => Fin.Succ(values))]);
    public static IOutputGroup Details<TProjection>(
        Port<Shape> input,
        Func<GrasshopperRuntime, Func<Shape, Eff<Env, Seq<TProjection>>>> aspect,
        bool emptyUnsupported,
        string aspectLabel,
        params OutputSlot<TProjection>[] slots) where TProjection : notnull =>
        FromShapes(
            input: input,
            project: aspect,
            emptyUnsupported: _ => emptyUnsupported,
            aspectLabel: aspectLabel,
            slots: slots);
    private static PreparedGroup<TSource> FromShapes<TSource>(
        Port<Shape> input,
        Func<GrasshopperRuntime, Func<Shape, Eff<Env, Seq<TSource>>>> project,
        Func<GrasshopperRuntime, bool> emptyUnsupported,
        string aspectLabel,
        params OutputSlot<TSource>[] slots) where TSource : notnull =>
        Prepared(
            source: runtime => ShapeSource(input: input, runtime: runtime, project: project(arg: runtime)),
            emptyUnsupported: emptyUnsupported,
            aspectLabel: aspectLabel,
            slots: slots);
    private static PreparedGroup<TSource> Prepared<TSource>(
        Func<GrasshopperRuntime, Fin<Seq<Flow<TSource>>>> source,
        Func<GrasshopperRuntime, bool> emptyUnsupported,
        string aspectLabel,
        params OutputSlot<TSource>[] slots) => new(Slots: toSeq(slots), Source: source, EmptyUnsupported: emptyUnsupported, AspectLabel: aspectLabel);
    internal static Fin<Seq<Flow<TSource>>> ShapeSource<TSource>(Port<Shape> input, GrasshopperRuntime runtime, Func<Shape, Eff<Env, Seq<TSource>>> project) =>
        from sourced in runtime.Shape(port: input)
        from context in runtime.Scope.Context
        from values in sourced.Traverse(src => project(arg: src.Item)
            .Map(values => values.Map(src.Project))
            .Run(env: new Env(Context: context, Progress: runtime.Progress, Cancellation: runtime.Cancellation))).As()
        select values.Bind(static value => value);
}
