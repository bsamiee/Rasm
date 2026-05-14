namespace Rasm.Grasshopper;

// --- [TYPES] ----------------------------------------------------------------------------
public sealed record OutputGroup(
    Seq<IPort> Ports,
    Func<IDataAccess, Hints, GrasshopperRuntime, Unit> RunGroup,
    Func<IDataAccess, Hints, Unit> EmptyGroup);

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
    public static Option<int> Index(this GrasshopperRuntime runtime, Port<int> port, int limit) =>
        limit switch {
            <= 0 => Option<int>.None,
            _ => runtime.Hints.Slot(port: port)
                .Bind(slot => runtime.Access.GetIndex(indexParameter: slot, limit: limit, index: out int index) ? Some(index) : Option<int>.None)
                | port.Fallback,
        };
}
public static class Output {
    private static Unit RunDetails<TSource>(
        Seq<OutputSlot<TSource>> slots,
        Func<GrasshopperRuntime, Fin<Seq<Flow<TSource>>>> source,
        bool emptyUnsupported,
        string aspectLabel,
        IDataAccess access,
        Hints outputs,
        GrasshopperRuntime runtime) {
        ArgumentNullException.ThrowIfNull(argument: access);
        Seq<(OutputSlot<TSource> Output, int Slot)> active = slots.Choose(output => outputs.Slot(port: output.Port).Map(slot => (Output: output, Slot: slot)));
        Fin<Unit> result = active.IsEmpty switch {
            true => Fin.Succ(Unit.Default),
            false => source(arg: runtime).Map(values => values.IsEmpty switch {
                true => RemarkEmpty(slots: slots, access: access, outputs: outputs),
                false => fun((Seq<Flow<TSource>> sourced, Seq<(OutputSlot<TSource> Output, int Slot)> writers) => {
                    Unit written = writers.Iter(pair => pair.Output.Write(arg1: access, arg2: pair.Slot, arg3: runtime, arg4: sourced));
                    _ = DisposeOwned(values: sourced, outputs: writers.Map(static pair => pair.Output.Port.ValueType));
                    return written;
                })(values, active),
            }),
        };
        return result.Match(
            Succ: static value => value,
            Fail: error => {
                _ = (emptyUnsupported, error) switch {
                    (true, Fault.Unsupported u) => RemarkUnsupported(slots: slots, access: access, aspectLabel: aspectLabel, fault: u),
                    _ => Warn(slots: slots, access: access, error: error),
                };
                _ = EmptyDetails(slots: slots, access: access, outputs: outputs);
                return Unit.Default;
            });
    }
    private static Unit EmptyDetails<TSource>(Seq<OutputSlot<TSource>> slots, IDataAccess access, Hints outputs) {
        ArgumentNullException.ThrowIfNull(argument: access);
        return slots.Choose(output => outputs.Slot(port: output.Port).Map(slot => (output, slot)))
            .Iter(pair => pair.output.Empty(arg1: access, arg2: pair.slot));
    }
    private static Unit RemarkUnsupported<TSource>(Seq<OutputSlot<TSource>> slots, IDataAccess access, string aspectLabel, Fault.Unsupported fault) {
        access.AddRemark(text: slots.Head.Map(static slot => slot.Port.Name).IfNone("Output"), details: $"Unsupported source type '{fault.GeometryType.Name}' for aspect '{aspectLabel}'.");
        return Unit.Default;
    }
    private static Unit Warn<TSource>(Seq<OutputSlot<TSource>> slots, IDataAccess access, Error error) {
        access.AddWarning(text: slots.Head.Map(static slot => slot.Port.Name).IfNone("Output"), details: error.Message);
        return Unit.Default;
    }
    private static Unit RemarkEmpty<TSource>(Seq<OutputSlot<TSource>> slots, IDataAccess access, Hints outputs) {
        access.AddRemark(text: slots.Head.Map(static slot => slot.Port.Name).IfNone("Output"), details: "No result for sourced input.");
        return EmptyDetails(slots: slots, access: access, outputs: outputs);
    }
    private static Unit DisposeOwned<TSource>(Seq<Flow<TSource>> values, Seq<Type> outputs) =>
        values.Choose(static value => value.Item is TopologyProjection projection ? Some(projection) : Option<TopologyProjection>.None)
            .Iter(projection => Optional(projection).Filter(p => !outputs.Exists(output => p.Transfers(outputType: output))).Iter(static p => p.Dispose()));
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
    public static Unit Write(IDataAccess access, GrasshopperRuntime runtime, Seq<OutputGroup> groups, Hints outputs) =>
        groups.Iter(group => group.RunGroup(arg1: access, arg2: outputs, arg3: runtime));
    public static Unit Empty(IDataAccess access, Seq<OutputGroup> groups, Hints outputs) =>
        groups.Iter(group => group.EmptyGroup(arg1: access, arg2: outputs));
    public static OutputGroup Single<TOut>(
        Port<Shape> input, Port<TOut> port,
        Func<GrasshopperRuntime, Fin<Query<object, TOut>>> query,
        bool emptyUnsupported = true, string aspectLabel = "Analysis") where TOut : notnull =>
        Details<TOut>(
            input: input,
            aspect: runtime => query(arg: runtime).Map<Func<Shape, Eff<Env, Seq<TOut>>>>(selected => shape => selected.Apply(geometry: shape.Inner)),
            emptyUnsupported: emptyUnsupported,
            aspectLabel: aspectLabel,
            slots: Slot<TOut, TOut>(port: port, project: static (_, values) => Fin.Succ(values)));
    public static OutputGroup Single<TAspect, TOut>(Port<Shape> input, Port<TOut> port, TAspect aspect)
        where TAspect : IAspect where TOut : notnull =>
        Single<TOut>(input: input, port: port,
            query: _ => Fin.Succ(aspect.ToQuery<object, TOut>()),
            aspectLabel: aspect.GetType().Name);
    public static OutputGroup Single<TAspect, TOut>(Port<Shape> input, Port<TOut> port, Func<GrasshopperRuntime, Fin<TAspect>> aspect)
        where TAspect : IAspect where TOut : notnull =>
        Single<TOut>(input: input, port: port,
            query: runtime => aspect(arg: runtime).Map(static selected => selected.ToQuery<object, TOut>()),
            aspectLabel: typeof(TAspect).Name);
    public static OutputGroup Details<TProjection>(
        Port<Shape> input,
        Func<GrasshopperRuntime, Fin<Func<Shape, Eff<Env, Seq<TProjection>>>>> aspect,
        bool emptyUnsupported,
        string aspectLabel,
        params OutputSlot<TProjection>[] slots) where TProjection : notnull {
        Seq<OutputSlot<TProjection>> prepared = toSeq(slots);
        return new OutputGroup(
            Ports: prepared.Map(static slot => slot.Port),
            RunGroup: (access, outputs, runtime) => RunDetails(
                slots: prepared,
                source: runtime => aspect(arg: runtime).Bind(next => ShapeSource(input: input, runtime: runtime, project: next)),
                emptyUnsupported: emptyUnsupported,
                aspectLabel: aspectLabel,
                access: access,
                outputs: outputs,
                runtime: runtime),
            EmptyGroup: (access, outputs) => EmptyDetails(slots: prepared, access: access, outputs: outputs));
    }
    internal static Fin<Seq<Flow<TSource>>> ShapeSource<TSource>(Port<Shape> input, GrasshopperRuntime runtime, Func<Shape, Eff<Env, Seq<TSource>>> project) =>
        from sourced in runtime.Shape(port: input)
        from context in runtime.Scope.Context
        from values in sourced.Traverse(src => project(arg: src.Item)
            .Map(src.Project)
            .Run(env: new Env(Context: context, Progress: runtime.Progress, Cancellation: runtime.Cancellation))).As()
        select values.Bind(static value => value);
}
