namespace Rasm.Grasshopper;

// --- [TYPES] ----------------------------------------------------------------------------
public sealed record OutputGroup(
    Port<Shape> Input,
    Seq<Port> Ports,
    Func<IDataAccess, Hints, GrasshopperRuntime, Seq<Flow<Shape>>, Seq<object>> RunGroup,
    Func<IDataAccess, Hints, Unit> EmptyGroup);

// --- [MODELS] ---------------------------------------------------------------------------
public readonly record struct Hints(Seq<(Port Port, int Slot)> Inputs) {
    internal static Hints Capture(Seq<BoundPort> ports, Func<IParameter, int> index) =>
        new(Inputs: ports.Choose(bound => index(arg: bound.Parameter) switch {
            >= 0 and int slot => Some((bound.Port, slot)),
            _ => Option<(Port Port, int Slot)>.None,
        }));
    public Option<int> Slot(Port port) =>
        Inputs.Find(predicate: input => ReferenceEquals(objA: input.Port, objB: port)).Map(static input => input.Slot);
}
public readonly record struct OutputSlot<TSource>(
    Port Port,
    Func<IDataAccess, int, GrasshopperRuntime, Seq<Flow<TSource>>, Seq<object>> Write,
    Func<IDataAccess, int, Unit> Empty);

// --- [SERVICES] -------------------------------------------------------------------------
public static class GrasshopperRuntimeExtensions {
    public static Fin<Option<TVal>> Read<TVal>(this GrasshopperRuntime runtime, Port<TVal> port) {
        ArgumentNullException.ThrowIfNull(argument: port);
        return runtime.Hints.Slot(port: port).Match(
            Some: slot => Bridge.Read<TVal>(access: runtime.Access, slot: slot, port: port)
                .Map(values => values.Head.Map(static pear => pear.Item) | port.Fallback),
            None: () => Fin.Succ(port.Fallback));
    }
    public static Option<int> Index(this GrasshopperRuntime runtime, Port<int> port, int limit) {
        ArgumentNullException.ThrowIfNull(argument: port);
        return limit switch {
            <= 0 => Option<int>.None,
            _ => runtime.Hints.Slot(port: port)
                .Bind(slot => runtime.Access.GetIndex(indexParameter: slot, limit: limit, index: out int index) ? Some(index) : Option<int>.None)
                | port.Fallback,
        };
    }
}
public static class Output {
    private static Seq<object> RunDetails<TSource>(
        Seq<OutputSlot<TSource>> slots,
        Func<GrasshopperRuntime, Eff<Env, Seq<Flow<TSource>>>> source,
        bool emptyUnsupported,
        string aspectLabel,
        IDataAccess access,
        Hints outputs,
        GrasshopperRuntime runtime) {
        ArgumentNullException.ThrowIfNull(argument: access);
        Seq<(OutputSlot<TSource> Output, int Slot)> active = slots.Choose(output => outputs.Slot(port: output.Port).Map(slot => (Output: output, Slot: slot)));
        Fin<Seq<object>> result = active.IsEmpty switch {
            true => Fin.Succ(Seq<object>()),
            false => from context in runtime.Scope.Context
                     from written in source(arg: runtime).Map(values => values.IsEmpty switch {
                         true => RemarkEmpty(slots: slots, access: access, outputs: outputs) switch { _ => Seq<object>() },
                         false => Drain(values),
                     }).Run(env: new Env(Context: context, Progress: runtime.Progress, Cancellation: runtime.Cancellation))
                     select written,
        };
        return result.Match(
            Succ: static value => value,
            Fail: error => {
                _ = (emptyUnsupported, error) switch {
                    (true, Fault.Unsupported u) => RemarkUnsupported(slots: slots, access: access, aspectLabel: aspectLabel, fault: u),
                    _ => Warn(slots: slots, access: access, error: error),
                };
                _ = EmptyDetails(slots: slots, access: access, outputs: outputs);
                return Seq<object>();
            });
        Seq<object> Drain(Seq<Flow<TSource>> sourced) => active.Bind(pair => pair.Output.Write(arg1: access, arg2: pair.Slot, arg3: runtime, arg4: sourced)) switch {
            Seq<object> transfers => DisposeOwned(values: sourced, outputs: transfers) switch { _ => transfers },
        };
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
    private static Unit DisposeOwned<TSource>(Seq<Flow<TSource>> values, Seq<object> outputs) =>
        values.Choose(static value => value.Item is TopologyProjection projection ? Some(projection) : Option<TopologyProjection>.None)
            .Iter(projection => _ = outputs.Exists(output => ReferenceEquals(objA: output, objB: projection) || projection.Transfers(outputType: output.GetType())) switch { true => unit, false => projection.Dispose() });
    internal static OutputSlot<TSource> Slot<TSource, TOut>(
        Port<TOut> port,
        Func<GrasshopperRuntime, Seq<Flow<TSource>>, Fin<Seq<Flow<TOut>>>> project) =>
        new(
            Port: port,
            Write: (access, slot, runtime, source) => project(arg1: runtime, arg2: source).Match(
                Succ: values => access.Write<TOut>(slot: slot, name: port.Name, targetAccess: port.Access, values: values) switch { _ => values.Map(static value => (object)value.Item!) },
                Fail: error => {
                    access.AddWarning(text: port.Name, details: error.Message);
                    _ = access.Write<TOut>(slot: slot, name: port.Name, targetAccess: port.Access, values: Seq<Flow<TOut>>());
                    return Seq<object>();
                }),
            Empty: (access, slot) => access.Write<TOut>(slot: slot, name: port.Name, targetAccess: port.Access, values: Seq<Flow<TOut>>()));
    public static OutputSlot<TSource> Plain<TSource, TOut>(Port<TOut> port, Func<TSource, TOut> project) =>
        Slot<TSource, TOut>(port: port, project: (_, sources) =>
            Fin.Succ(sources.Map(src => src.Project(item: project(arg: src.Item)))));
    public static OutputSlot<TSource> Choose<TSource, TOut>(Port<TOut> port, Func<TSource, Option<TOut>> project) =>
        Slot<TSource, TOut>(port: port, project: (_, sources) =>
            Fin.Succ(sources.Choose(src => project(arg: src.Item).Map(src.Project))));
    public static OutputSlot<TSource> One<TSource, TOut>(Port<TOut> port, Func<TSource, Context, Fin<TOut>> project) =>
        Slot<TSource, TOut>(port: port, project: (runtime, sources) => runtime.Scope.Context
            .Bind(context => sources.TraverseM(src => project(arg1: src.Item, arg2: context).Map(src.Project)).As()));
    public static Unit Write(IDataAccess access, GrasshopperRuntime runtime, Seq<OutputGroup> groups, Hints outputs) {
        Seq<OutputGroup> active = groups.Filter(group => group.Ports.Exists(port => outputs.Slot(port: port).IsSome));
        Seq<Port<Shape>> inputs = active.Fold(Seq<Port<Shape>>(), (found, group) => found.Exists(input => ReferenceEquals(objA: input, objB: group.Input)) ? found : group.Input.Cons(found)).Rev();
        return active.IsEmpty switch {
            true => Unit.Default,
            false => inputs.Iter(input => RunCached(
                access: access,
                outputs: outputs,
                runtime: runtime,
                groups: active.Filter(group => ReferenceEquals(objA: group.Input, objB: input)),
                source: runtime.Shape(port: input))),
        };
    }
    public static Unit Empty(IDataAccess access, Seq<OutputGroup> groups, Hints outputs) =>
        groups.Iter(group => group.EmptyGroup(arg1: access, arg2: outputs));
    public static OutputGroup Single<TOut>(
        Port<Shape> input, Port<TOut> port,
        Func<GrasshopperRuntime, Fin<Operation<object, TOut>>> operation,
        bool emptyUnsupported = true, string aspectLabel = "Analysis") where TOut : notnull =>
        Details<TOut>(
            input: input,
            operation: operation,
            emptyUnsupported: emptyUnsupported,
            aspectLabel: aspectLabel,
            slots: Slot<TOut, TOut>(port: port, project: static (_, values) => Fin.Succ(values)));
    public static OutputGroup Single<TAspect, TOut>(Port<Shape> input, Port<TOut> port, TAspect aspect)
        where TAspect : IAspect where TOut : notnull =>
        Single<TOut>(input: input, port: port,
            operation: _ => Fin.Succ(aspect.Operation<object, TOut>()),
            aspectLabel: typeof(TAspect).Name);
    public static OutputGroup Single<TAspect, TOut>(Port<Shape> input, Port<TOut> port, Func<GrasshopperRuntime, Fin<TAspect>> aspect)
        where TAspect : IAspect where TOut : notnull =>
        Single<TOut>(input: input, port: port,
            operation: runtime => aspect(arg: runtime).Map(static selected => selected.Operation<object, TOut>()),
            aspectLabel: typeof(TAspect).Name);
    public static OutputGroup Details<TProjection>(
        Port<Shape> input,
        Func<GrasshopperRuntime, Fin<Operation<object, TProjection>>> operation,
        bool emptyUnsupported,
        string aspectLabel,
        params OutputSlot<TProjection>[] slots) where TProjection : notnull {
        Seq<OutputSlot<TProjection>> prepared = toSeq(slots);
        return new OutputGroup(
            Input: input,
            Ports: prepared.Map(static slot => slot.Port),
            RunGroup: (access, outputs, runtime, source) => RunDetails(
                slots: prepared,
                source: runtime => from selected in operation(arg: runtime).ToEff()
                                   from values in ShapeSource(sourced: source, operation: selected)
                                   select values,
                emptyUnsupported: emptyUnsupported,
                aspectLabel: aspectLabel,
                access: access,
                outputs: outputs,
                runtime: runtime),
            EmptyGroup: (access, outputs) => EmptyDetails(slots: prepared, access: access, outputs: outputs));
    }
    public static OutputGroup Details<TAspect, TProjection>(
        Port<Shape> input,
        TAspect aspect,
        bool emptyUnsupported,
        params OutputSlot<TProjection>[] slots) where TAspect : IAspect where TProjection : notnull =>
        Details<TProjection>(
            input: input,
            operation: _ => Fin.Succ(aspect.Operation<object, TProjection>()),
            emptyUnsupported: emptyUnsupported,
            aspectLabel: typeof(TAspect).Name,
            slots: slots);
    public static OutputGroup Details<TAspect, TProjection>(
        Port<Shape> input,
        Func<GrasshopperRuntime, Fin<TAspect>> aspect,
        bool emptyUnsupported,
        params OutputSlot<TProjection>[] slots) where TAspect : IAspect where TProjection : notnull =>
        Details<TProjection>(
            input: input,
            operation: runtime => aspect(arg: runtime).Map(static selected => selected.Operation<object, TProjection>()),
            emptyUnsupported: emptyUnsupported,
            aspectLabel: typeof(TAspect).Name,
            slots: slots);
    internal static Eff<Env, Seq<Flow<TSource>>> ShapeSource<TSource>(Seq<Flow<Shape>> sourced, Operation<object, TSource> operation) =>
        operation.IsAggregate switch {
            true => from items in operation.Apply(geometry: sourced.Map(static src => src.Item.Inner))
                    let result = sourced.Fold(items.Map(item => new Flow<TSource>(
                        Pear: Pear<TSource>.Create(item: item, meta: MetaData.FindCommonData(sourced.Map(static src => src.Meta).AsIterable())),
                        Site: Option<Site>.None)), static (acc, src) => acc.Map(src.Item.Detach))
                    select result,
            false => from values in sourced.TraverseM(src => operation.Apply(geometry: Seq(src.Item.Inner)).Map(items => src.Project(items: items).Map(src.Item.Detach))).As()
                     let result = values.Bind(static value => value)
                     select result,
        };
    private static Unit RunCached(IDataAccess access, Hints outputs, GrasshopperRuntime runtime, Seq<OutputGroup> groups, Fin<Seq<Flow<Shape>>> source) =>
        source.Match(
            Succ: sourced => groups.Bind(group => group.RunGroup(arg1: access, arg2: outputs, arg3: runtime, arg4: sourced)) switch {
                Seq<object> transfers => sourced.Iter(source => source.Item.DisposeUnlessTransferred(outputs: transfers)),
            },
            Fail: error => {
                access.AddWarning(text: error.Category(), details: error.Message);
                return groups.Iter(group => group.EmptyGroup(arg1: access, arg2: outputs));
            });
}
