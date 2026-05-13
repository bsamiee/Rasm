namespace Rasm.Grasshopper;

// --- [TYPES] ----------------------------------------------------------------------------
public interface IOutputGroup {
    public Seq<IPort> Ports { get; }
    public Unit Run(IDataAccess access, int slot, GrasshopperRuntime runtime);
    public Unit Empty(IDataAccess access, int slot);
}

// --- [MODELS] ---------------------------------------------------------------------------
public readonly record struct Hints(Seq<(IPort Port, int Slot)> Inputs) {
    public static Hints Capture(Seq<IPort> inputs) =>
        new(Inputs: inputs.Map((port, slot) => (Port: port, Slot: slot)));
    public Option<int> Slot(IPort port) =>
        Inputs.Find(predicate: input => input.Port.Equals(port)).Map(static input => input.Slot);
}
public readonly record struct OutputSlot<TSource>(
    IPort Port,
    Func<IDataAccess, int, GrasshopperRuntime, Seq<Pear<TSource>>, Unit> Write,
    Func<IDataAccess, int, Unit> Empty);

// --- [SERVICES] -------------------------------------------------------------------------
public static class GrasshopperRuntimeExtensions {
    public static Option<TVal> Read<TVal>(this GrasshopperRuntime runtime, Port<TVal> port) {
        Option<TVal> wired = runtime.Hints.Slot(port: port)
            .Bind(slot => Bridge.Read<TVal>(access: runtime.Access, slot: slot, port: port).ToOption()
                .Bind(static values => values.Head)
                .Map(static pear => pear.Item));
        return wired.IsSome ? wired : port.Fallback;
    }
}
internal sealed record PreparedGroup<TSource>(
    Seq<OutputSlot<TSource>> Slots,
    Func<IDataAccess, GrasshopperRuntime, Fin<Seq<Pear<TSource>>>> Source,
    Func<GrasshopperRuntime, bool> EmptyUnsupported,
    string AspectLabel) : IOutputGroup {
    public Seq<IPort> Ports => Slots.Map(static slot => slot.Port);
    public Unit Run(IDataAccess access, int slot, GrasshopperRuntime runtime) {
        ArgumentNullException.ThrowIfNull(argument: access);
        return Source(arg1: access, arg2: runtime).Match(
            Succ: values => values.IsEmpty switch {
                true => RemarkEmpty(access: access, slot: slot),
                false => Slots.Iter((offset, output) => output.Write(arg1: access, arg2: slot + offset, arg3: runtime, arg4: values)),
            },
            Fail: error => {
                _ = (EmptyUnsupported(arg: runtime), error) switch {
                    (true, Fault.Unsupported u) => RemarkUnsupported(access: access, fault: u),
                    _ => Warn(access: access, error: error),
                };
                return Empty(access: access, slot: slot);
            });
    }
    public Unit Empty(IDataAccess access, int slot) {
        ArgumentNullException.ThrowIfNull(argument: access);
        return Slots.Iter((offset, output) => output.Empty(arg1: access, arg2: slot + offset));
    }
    private Unit RemarkUnsupported(IDataAccess access, Fault.Unsupported fault) {
        access.AddRemark(text: Ports.Head.Map(static port => port.Name).IfNone("Output"), details: $"Unsupported source type '{fault.GeometryType.Name}' for aspect '{AspectLabel}'.");
        return Unit.Default;
    }
    private Unit Warn(IDataAccess access, Error error) {
        access.AddWarning(text: Ports.Head.Map(static port => port.Name).IfNone("Output"), details: error.Message);
        return Unit.Default;
    }
    private Unit RemarkEmpty(IDataAccess access, int slot) {
        access.AddRemark(text: Ports.Head.Map(static port => port.Name).IfNone("Output"), details: "No result for sourced input.");
        return Empty(access: access, slot: slot);
    }
}
public static class Output {
    internal static OutputSlot<TSource> Slot<TSource, TOut>(
        Port<TOut> port,
        Func<GrasshopperRuntime, Seq<Pear<TSource>>, Fin<Seq<Pear<TOut>>>> project) =>
        new(
            Port: port,
            Write: (access, slot, runtime, source) => project(arg1: runtime, arg2: source).Match(
                Succ: values => Bridge.Write(access: access, slot: slot, name: port.Name, targetAccess: port.Access, values: values),
                Fail: error => {
                    access.AddWarning(text: port.Name, details: error.Message);
                    return Bridge.Write(access: access, slot: slot, name: port.Name, targetAccess: port.Access, values: Seq<Pear<TOut>>());
                }),
            Empty: (access, slot) => Bridge.Write(access: access, slot: slot, name: port.Name, targetAccess: port.Access, values: Seq<Pear<TOut>>()));
    public static OutputSlot<TSource> Plain<TSource, TOut>(Port<TOut> port, Func<TSource, TOut> project) =>
        Slot<TSource, TOut>(port: port, project: (_, sources) =>
            Fin.Succ(sources.Map(src => Pear<TOut>.Create(item: project(arg: src.Item), meta: src.Meta))));
    public static OutputSlot<TSource> One<TSource, TOut>(Port<TOut> port, Func<TSource, Context, Fin<TOut>> project) =>
        Slot<TSource, TOut>(port: port, project: (runtime, sources) => runtime.Scope.Context
            .Bind(context => sources.Traverse(src => project(arg1: src.Item, arg2: context).Map(value => Pear<TOut>.Create(item: value, meta: src.Meta))).As()));
    public static OutputSlot<TSource> Many<TSource, TOut>(Port<TOut> port, Func<TSource, Fin<Seq<TOut>>> project) =>
        Slot<TSource, TOut>(port: port, project: (_, sources) => sources.Traverse(src =>
            project(arg: src.Item).Map(values => values.Map(value => Pear<TOut>.Create(item: value, meta: src.Meta))))
            .Map(static nested => nested.Bind(static x => x)).As());
    public static Unit Write(IDataAccess access, GrasshopperRuntime runtime, Seq<IOutputGroup> groups) =>
        Fold(groups: groups, action: group => slot => group.Run(access: access, slot: slot, runtime: runtime));
    public static Unit Empty(IDataAccess access, Seq<IOutputGroup> groups) =>
        Fold(groups: groups, action: group => slot => group.Empty(access: access, slot: slot));
    public static IOutputGroup Query<TAspect, TOut>(Port<Shape> input, Port<TOut> port, TAspect aspect) where TAspect : IAspect =>
        Prepared(
            source: (access, runtime) => ShapeSource(input: input, access: access, runtime: runtime, project: shape => aspect.ToQuery<object, TOut>().Apply(geometry: shape.Inner)),
            emptyUnsupported: _ => aspect.EmptyOnUnsupported,
            aspectLabel: aspect.GetType().Name,
            slots: [Slot<TOut, TOut>(port: port, project: static (_, values) => Fin.Succ(values))]);
    public static IOutputGroup Query<TAspect, TOut>(Port<Shape> input, Port<TOut> port, Func<GrasshopperRuntime, TAspect> aspect) where TAspect : IAspect =>
        Prepared(
            source: (access, runtime) => ShapeSource(input: input, access: access, runtime: runtime, project: shape => aspect(arg: runtime).ToQuery<object, TOut>().Apply(geometry: shape.Inner)),
            emptyUnsupported: runtime => aspect(arg: runtime).EmptyOnUnsupported,
            aspectLabel: typeof(TAspect).Name,
            slots: [Slot<TOut, TOut>(port: port, project: static (_, values) => Fin.Succ(values))]);
    public static IOutputGroup Query<TOut>(Port<Shape> input, Port<TOut> port, Func<GrasshopperRuntime, Query<object, TOut>> aspect, bool emptyUnsupported = false, string aspectLabel = "Query") =>
        Prepared(
            source: (access, runtime) => ShapeSource(input: input, access: access, runtime: runtime, project: shape => aspect(arg: runtime).Apply(geometry: shape.Inner)),
            emptyUnsupported: _ => emptyUnsupported,
            aspectLabel: aspectLabel,
            slots: [Slot<TOut, TOut>(port: port, project: static (_, values) => Fin.Succ(values))]);
    public static IOutputGroup Details<TProjection>(
        Port<Shape> input,
        Func<GrasshopperRuntime, Func<Shape, Eff<Env, Seq<TProjection>>>> aspect,
        bool emptyUnsupported,
        string aspectLabel,
        params OutputSlot<TProjection>[] slots) where TProjection : notnull =>
        Prepared(
            source: (access, runtime) => ShapeSource(input: input, access: access, runtime: runtime, project: aspect(arg: runtime)),
            emptyUnsupported: _ => emptyUnsupported,
            aspectLabel: aspectLabel,
            slots: slots);
    private static PreparedGroup<TSource> Prepared<TSource>(
        Func<IDataAccess, GrasshopperRuntime, Fin<Seq<Pear<TSource>>>> source,
        Func<GrasshopperRuntime, bool> emptyUnsupported,
        string aspectLabel,
        params OutputSlot<TSource>[] slots) => new(Slots: toSeq(slots), Source: source, EmptyUnsupported: emptyUnsupported, AspectLabel: aspectLabel);
    private static Unit Fold(Seq<IOutputGroup> groups, Func<IOutputGroup, Func<int, Unit>> action) =>
        groups.Fold(initialState: 0, f: (slot, group) => (action(arg: group)(arg: slot), slot + group.Ports.Count).Item2) switch { _ => Unit.Default };
    internal static Fin<Seq<Pear<TSource>>> ShapeSource<TSource>(Port<Shape> input, IDataAccess access, GrasshopperRuntime runtime, Func<Shape, Eff<Env, Seq<TSource>>> project) =>
        from sourced in runtime.Shape(access: access, port: input)
        from context in runtime.Scope.Context
        from values in project(arg: sourced.Item).Run(env: new Env(Context: context, Progress: new Bridge.Progress(access: access), Cancellation: access.Solution.Token))
        select values.Map(value => Pear<TSource>.Create(item: value, meta: sourced.Meta));
}
