namespace Rasm.Grasshopper;

// --- [TYPES] ----------------------------------------------------------------------------
public interface IOutputGroup {
    public Seq<IPort> Ports { get; }
    public Unit Run(IDataAccess access, int slot, GrasshopperRuntime runtime);
    public Unit Empty(IDataAccess access, int slot);
}

// --- [MODELS] ---------------------------------------------------------------------------
public readonly record struct Hints(
    Seq<(IPort Port, int Slot)> Inputs) {
    public static Hints Capture(Seq<IPort> inputs, IDataAccess access) =>
        new(Inputs: inputs.Map((port, slot) => (Port: port, Slot: slot)));
    public Option<int> Slot(IPort port) => Inputs.Find(predicate: input => input.Port.Equals(port)).Map(static input => input.Slot);
    public Option<int> Index(IDataAccess access, Port<int> port, int limit) {
        ArgumentNullException.ThrowIfNull(argument: access);
        return Slot(port: port).Bind(slot => access.GetIndex(indexParameter: slot, limit: limit, index: out int value) ? Some(value) : Option<int>.None);
    }
    public Option<TVal> Value<TVal>(IDataAccess access, Port<TVal> port) {
        ArgumentNullException.ThrowIfNull(argument: access);
        return Inputs.Find(predicate: input => input.Port.Equals(port))
            .Bind(input => Bridge.Read<TVal>(access: access, slot: input.Slot, port: port).ToOption()
                .Bind(static values => values.Head)
                .Map(static sourced => sourced.Value));
    }
}
public readonly record struct OutputSlot<TSource>(
    IPort Port,
    Func<IDataAccess, int, GrasshopperRuntime, Seq<Sourced<TSource>>, Unit> Write,
    Func<IDataAccess, int, Unit> Empty);

// --- [SERVICES] -------------------------------------------------------------------------
public static class GrasshopperRuntimeExtensions {
    public static Option<TVal> Value<TVal>(this GrasshopperRuntime runtime, IDataAccess access, Port<TVal> port) {
        ArgumentNullException.ThrowIfNull(argument: access);
        return runtime.Hints.Value(access: access, port: port);
    }
    public static TVal? Nullable<TVal>(this GrasshopperRuntime runtime, IDataAccess access, Port<TVal> port) where TVal : struct {
        ArgumentNullException.ThrowIfNull(argument: access);
        return runtime.Hints.Value(access: access, port: port).Match<TVal?>(Some: static value => value, None: static () => null);
    }
    public static Option<int> Index(this GrasshopperRuntime runtime, IDataAccess access, Port<int> port, int limit = int.MaxValue) {
        ArgumentNullException.ThrowIfNull(argument: access);
        return runtime.Hints.Index(access: access, port: port, limit: limit);
    }
    public static int? NullableIndex(this GrasshopperRuntime runtime, IDataAccess access, Port<int> port, int limit = int.MaxValue) {
        ArgumentNullException.ThrowIfNull(argument: access);
        return runtime.Hints.Index(access: access, port: port, limit: limit).Match<int?>(Some: static value => value, None: static () => null);
    }
}
internal sealed record PreparedGroup<TSource>(
    Seq<OutputSlot<TSource>> Slots,
    Func<IDataAccess, GrasshopperRuntime, Fin<Seq<Sourced<TSource>>>> Source,
    bool EmptyUnsupported) : IOutputGroup {
    public Seq<IPort> Ports => Slots.Map(static slot => slot.Port);
    public Unit Run(IDataAccess access, int slot, GrasshopperRuntime runtime) {
        ArgumentNullException.ThrowIfNull(argument: access);
        return Source(arg1: access, arg2: runtime).Match(
            Succ: values => values.IsEmpty switch {
                true => RemarkEmpty(access: access, slot: slot),
                false => Slots.Iter((offset, output) => output.Write(arg1: access, arg2: slot + offset, arg3: runtime, arg4: values)),
            },
            Fail: error => {
                _ = (EmptyUnsupported, error is Fault.Unsupported) switch {
                    (true, true) => Unit.Default,
                    _ => Warn(access: access, error: error),
                };
                return Empty(access: access, slot: slot);
            });
    }
    public Unit Empty(IDataAccess access, int slot) {
        ArgumentNullException.ThrowIfNull(argument: access);
        return Slots.Iter((offset, output) => output.Empty(arg1: access, arg2: slot + offset));
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
        Func<GrasshopperRuntime, Seq<Sourced<TSource>>, Fin<Seq<Sourced<TOut>>>> project) =>
        new(
            Port: port,
            Write: (access, slot, runtime, source) => project(arg1: runtime, arg2: source).Match(
                Succ: values => Bridge.Write(access: access, slot: slot, name: port.Name, targetAccess: port.Access, values: values),
                Fail: error => {
                    access.AddWarning(text: port.Name, details: error.Message);
                    return Bridge.Write(access: access, slot: slot, name: port.Name, targetAccess: port.Access, values: Seq<Sourced<TOut>>());
                }),
            Empty: (access, slot) => Bridge.Write(access: access, slot: slot, name: port.Name, targetAccess: port.Access, values: Seq<Sourced<TOut>>()));
    public static OutputSlot<TSource> Plain<TSource, TOut>(Port<TOut> port, Func<TSource, TOut> project) =>
        Slot<TSource, TOut>(port: port, project: (_, sources) =>
            Fin.Succ(sources.Map(src => new Sourced<TOut>(Value: project(arg: src.Value), Meta: src.Meta))));
    public static OutputSlot<TSource> One<TSource, TOut>(Port<TOut> port, Func<TSource, Context, Fin<TOut>> project) =>
        Slot<TSource, TOut>(port: port, project: (runtime, sources) => runtime.Scope.Context
            .Bind(context => sources.Traverse(src => project(arg1: src.Value, arg2: context).Map(value => new Sourced<TOut>(Value: value, Meta: src.Meta))).As()));
    public static OutputSlot<TSource> Many<TSource, TOut>(Port<TOut> port, Func<TSource, Fin<Seq<TOut>>> project) =>
        Slot<TSource, TOut>(port: port, project: (_, sources) => sources.Traverse(src =>
            project(arg: src.Value).Map(values => values.Map(value => new Sourced<TOut>(Value: value, Meta: src.Meta))))
            .Map(static nested => nested.Bind(static x => x)).As());
    private static PreparedGroup<TSource> Prepared<TSource>(
        Func<IDataAccess, GrasshopperRuntime, Fin<Seq<Sourced<TSource>>>> source,
        bool emptyUnsupported = false,
        params OutputSlot<TSource>[] slots) => new(Slots: toSeq(slots), Source: source, EmptyUnsupported: emptyUnsupported);
    public static Unit Write(IDataAccess access, GrasshopperRuntime runtime, Seq<IOutputGroup> groups) =>
        Fold(groups: groups, action: group => slot => group.Run(access: access, slot: slot, runtime: runtime));
    public static Unit Empty(IDataAccess access, Seq<IOutputGroup> groups) =>
        Fold(groups: groups, action: group => slot => group.Empty(access: access, slot: slot));
    private static Unit Fold(Seq<IOutputGroup> groups, Func<IOutputGroup, Func<int, Unit>> action) =>
        groups.Fold(initialState: 0, f: (slot, group) => (action(arg: group)(arg: slot), slot + group.Ports.Count).Item2) switch { _ => Unit.Default };
    public static IOutputGroup Query<TOut>(Port<Shape> input, Port<TOut> port, Func<IDataAccess, GrasshopperRuntime, Query<object, TOut>> operation, bool emptyUnsupported = false) =>
        Prepared(
            source: (access, runtime) => ShapeSource(input: input, access: access, runtime: runtime, project: shape => operation(arg1: access, arg2: runtime).Apply(geometry: shape.Inner)),
            emptyUnsupported: emptyUnsupported,
            slots: [Slot<TOut, TOut>(port: port, project: static (_, values) => Fin.Succ(values))]);
    public static IOutputGroup Query<TAspect, TOut>(Port<Shape> input, Port<TOut> port, TAspect aspect, bool emptyUnsupported = false) where TAspect : notnull =>
        Prepared(
            source: (access, runtime) => ShapeSource(input: input, access: access, runtime: runtime, project: shape => DispatchAspect<TAspect, TOut>(aspect: aspect).Apply(geometry: shape.Inner)),
            emptyUnsupported: emptyUnsupported,
            slots: [Slot<TOut, TOut>(port: port, project: static (_, values) => Fin.Succ(values))]);
    private static Query<object, TOut> DispatchAspect<TAspect, TOut>(TAspect aspect) where TAspect : notnull {
        Op key = Op.Of();
        return aspect switch {
            Curves c => Rasm.Analysis.Query.Curves<object, TOut>(aspect: c),
            Faces f => Rasm.Analysis.Query.Faces<object, TOut>(aspect: f),
            Meshes mesh => Rasm.Analysis.Query.Meshes<object, TOut>(aspect: mesh),
            Location l => Rasm.Analysis.Query.Locate<object, TOut>(aspect: l),
            Bounds b => Rasm.Analysis.Query.Bounds<object, TOut>(aspect: b),
            Measure m => Rasm.Analysis.Query.Measure<object, TOut>(aspect: m),
            _ => Rasm.Analysis.Query<object, TOut>.Reject(
                key: key,
                fault: key.Unsupported(geometryType: typeof(TAspect), outputType: typeof(TOut))),
        };
    }
    public static IOutputGroup Details<TProjection>(
        Port<Shape> input,
        Func<IDataAccess, GrasshopperRuntime, Func<Shape, Eff<Analyze.Env, Seq<TProjection>>>> source,
        bool emptyUnsupported = false,
        params OutputSlot<TProjection>[] slots) where TProjection : notnull =>
        Prepared(
            source: (access, runtime) => ShapeSource(input: input, access: access, runtime: runtime, project: source(arg1: access, arg2: runtime)),
            emptyUnsupported: emptyUnsupported,
            slots: slots);
    internal static Fin<Seq<Sourced<TSource>>> ShapeSource<TSource>(Port<Shape> input, IDataAccess access, GrasshopperRuntime runtime, Func<Shape, Eff<Analyze.Env, Seq<TSource>>> project) =>
        from sourced in runtime.Shape(access: access, port: input)
        from context in runtime.Scope.Context
        from values in project(arg: sourced.Value).Run(env: new Analyze.Env(Context: context, Progress: new Bridge.Progress(access: access), Cancellation: access.Solution.Token))
        select values.Map(value => new Sourced<TSource>(Value: value, Meta: sourced.Meta));
}
