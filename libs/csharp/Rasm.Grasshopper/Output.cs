namespace Rasm.Grasshopper;

// --- [TYPES] ----------------------------------------------------------------------------
public sealed record OutputBinding(
    Port<Shape> Input,
    Port Port,
    Func<IDataAccess, Hints, GrasshopperRuntime, Seq<Flow<Shape>>, Seq<object>> Run,
    Func<IDataAccess, Hints, Unit> Empty);

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
// --- [SERVICES] -------------------------------------------------------------------------
public static class GrasshopperRuntimeExtensions {
    internal static Fin<Option<TVal>> ReadScalar<TVal>(this GrasshopperRuntime runtime, Port<TVal> port) {
        ArgumentNullException.ThrowIfNull(argument: port);
        return port.Access switch {
            Access.Item => runtime.Hints.Slot(port: port)
                .Map(slot => Bridge.Read<TVal>(access: runtime.Access, slot: slot, port: port)
                    .Map(values => values.Head.Map(static value => value.Item) | port.Fallback))
                .IfNone(Fin.Succ(port.Fallback)),
            _ => Fin.Fail<Option<TVal>>(new UnsupportedAccess(Access: port.Access.ToString())),
        };
    }
    internal static Option<int> Index(this GrasshopperRuntime runtime, Port<int> port, int limit) {
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
    public static OutputBinding Of<TAspect, TOut>(Port<Shape> input, Port<TOut> port, TAspect aspect)
        where TAspect : IAspect where TOut : notnull {
        ArgumentNullException.ThrowIfNull(argument: input);
        ArgumentNullException.ThrowIfNull(argument: port);
        ArgumentNullException.ThrowIfNull(argument: aspect);
        Operation<object, TOut> operation = aspect.Operation<object, TOut>();
        return new OutputBinding(
            Input: input,
            Port: port,
            Run: (access, outputs, runtime, source) => Run(
                port: port,
                operation: operation,
                access: access,
                outputs: outputs,
                runtime: runtime,
                source: source),
            Empty: (access, outputs) => outputs.Slot(port: port).Iter(slot => Empty(port: port, access: access, slot: slot)));
    }
    public static Unit Write(IDataAccess access, GrasshopperRuntime runtime, Seq<OutputBinding> bindings, Hints outputs) {
        Seq<OutputBinding> active = bindings.Filter(binding => outputs.Slot(port: binding.Port).IsSome);
        Seq<Port<Shape>> inputs = active.Fold(Seq<Port<Shape>>(), (found, binding) => found.Exists(input => ReferenceEquals(objA: input, objB: binding.Input)) ? found : binding.Input.Cons(found)).Rev();
        return active.IsEmpty switch {
            true => Unit.Default,
            false => inputs.Iter(input => RunCached(
                access: access,
                outputs: outputs,
                runtime: runtime,
                bindings: active.Filter(binding => ReferenceEquals(objA: binding.Input, objB: input)),
                source: runtime.Shape(port: input))),
        };
    }
    public static Unit Empty(IDataAccess access, Seq<OutputBinding> bindings, Hints outputs) =>
        bindings.Iter(binding => binding.Empty(arg1: access, arg2: outputs));
    private static Seq<object> Run<TOut>(
        Port<TOut> port,
        Operation<object, TOut> operation,
        IDataAccess access,
        Hints outputs,
        GrasshopperRuntime runtime,
        Seq<Flow<Shape>> source) where TOut : notnull {
        ArgumentNullException.ThrowIfNull(argument: access);
        return outputs.Slot(port: port)
            .Map(slot => from context in runtime.Scope.Context
                         from projection in ShapeSource(sourced: source, operation: operation).Run(env: new Env(Context: context, Progress: runtime.Progress, Cancellation: runtime.Cancellation))
                         select projection.Values.IsEmpty switch {
                             true when projection.Unsupported.IsEmpty => RemarkEmpty(port: port, access: access, slot: slot) switch { _ => Seq<object>() },
                             true => RemarkUnsupported(port: port, access: access, faults: projection.Unsupported) switch { _ => Empty(port: port, access: access, slot: slot) switch { _ => Seq<object>() } },
                             false => RemarkUnsupported(port: port, access: access, faults: projection.Unsupported) switch { _ => Drain(port: port, slot: slot, values: projection.Values, access: access) },
                         })
            .IfNone(Fin.Succ(Seq<object>()))
            .Match(
            Succ: static values => values,
            Fail: error => {
                _ = error switch {
                    Fault.Unsupported unsupported => RemarkUnsupported(port: port, access: access, faults: Seq(unsupported)),
                    _ => Warn(port: port, access: access, error: error),
                };
                _ = outputs.Slot(port: port).Iter(slot => Empty(port: port, access: access, slot: slot));
                return Seq<object>();
            });
    }
    private static Seq<object> Drain<TOut>(Port<TOut> port, int slot, Seq<Flow<TOut>> values, IDataAccess access) {
        Seq<object> transfers = access.Write<TOut>(slot: slot, name: port.Name, targetAccess: port.Access, values: values);
        _ = values.Choose(static value => value.Item is TopologyProjection projection ? Some(projection) : Option<TopologyProjection>.None)
            .Iter(projection => _ = transfers.Exists(output => ReferenceEquals(objA: output, objB: projection) || projection.Transfers(output: output)) switch { true => unit, false => projection.Dispose() });
        return transfers;
    }
    private static Unit Empty<TOut>(Port<TOut> port, IDataAccess access, int slot) {
        ArgumentNullException.ThrowIfNull(argument: access);
        return access.Write<TOut>(slot: slot, name: port.Name, targetAccess: port.Access, values: Seq<Flow<TOut>>()) switch { _ => Unit.Default };
    }
    private static Unit RemarkUnsupported(Port port, IDataAccess access, Seq<Fault.Unsupported> faults) {
        Seq<Fault.Unsupported> found = faults.Distinct().ToSeq();
        Option<string> details = found.IsEmpty switch {
            true => Option<string>.None,
            false => Some(found.Count switch {
                1 => found.Head.Map(static fault => fault.Message).IfNone("Unsupported source."),
                int count => string.Join(separator: Environment.NewLine, values: $"Unsupported source/output combinations: {count}".Cons(found.Map(static fault => fault.Message)).AsIterable()),
            }),
        };
        _ = details.Iter(details => access.AddRemark(text: port.Name, details: details));
        return Unit.Default;
    }
    private static Unit Warn(Port port, IDataAccess access, Error error) {
        access.AddWarning(text: port.Name, details: error.Message);
        return Unit.Default;
    }
    private static Unit RemarkEmpty<TOut>(Port<TOut> port, IDataAccess access, int slot) {
        access.AddRemark(text: port.Name, details: "No result for sourced input.");
        return Empty(port: port, access: access, slot: slot);
    }
    private readonly record struct Projection<T>(Seq<Flow<T>> Values, Seq<Fault.Unsupported> Unsupported);
    private static Eff<Env, Projection<TSource>> ShapeSource<TSource>(Seq<Flow<Shape>> sourced, Operation<object, TSource> operation) =>
        operation.IsAggregate switch {
            true => from items in operation.Apply(geometry: sourced.Map(static src => src.Item.Inner))
                    let result = sourced.Fold(items.Map(item => new Flow<TSource>(
                        Pear: Pear<TSource>.Create(item: item, meta: MetaData.FindCommonData(sourced.Map(static src => src.Meta).AsIterable())),
                        Site: Option<Site>.None)), static (acc, src) => acc.Map(src.Item.Detach))
                    select new Projection<TSource>(Values: result, Unsupported: Seq<Fault.Unsupported>()),
            false => from values in sourced.TraverseM(src => operation.Apply(geometry: Seq(src.Item.Inner))
                         .Map(items => new Projection<TSource>(Values: src.Project(items: items).Map(src.Item.Detach), Unsupported: Seq<Fault.Unsupported>()))
                         .IfFailEff(error => error switch {
                             Fault.Unsupported unsupported => Fin.Succ(new Projection<TSource>(Values: Seq<Flow<TSource>>(), Unsupported: Seq(unsupported))).ToEff(),
                             _ => Fin.Fail<Projection<TSource>>(error).ToEff(),
                         })).As()
                     select new Projection<TSource>(
                         Values: values.Bind(static value => value.Values),
                         Unsupported: values.Bind(static value => value.Unsupported)),
        };
    private static Unit RunCached(IDataAccess access, Hints outputs, GrasshopperRuntime runtime, Seq<OutputBinding> bindings, Fin<Seq<Flow<Shape>>> source) =>
        source.Match(
            Succ: sourced => bindings.Bind(binding => binding.Run(arg1: access, arg2: outputs, arg3: runtime, arg4: sourced)) switch {
                Seq<object> transfers => sourced.Iter(source => source.Item.DisposeUnlessTransferred(outputs: transfers)),
            },
            Fail: error => {
                access.AddWarning(text: error.Category(), details: error.Message);
                return bindings.Iter(binding => binding.Empty(arg1: access, arg2: outputs));
            });
}
