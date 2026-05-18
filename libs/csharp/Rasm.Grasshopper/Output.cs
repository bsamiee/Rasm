namespace Rasm.Grasshopper;

// --- [TYPES] ----------------------------------------------------------------------------
public sealed class OutputBinding {
    private readonly Func<IDataAccess, Hints, GrasshopperRuntime, Seq<Flow<Shape>>, Seq<object>> run;
    private readonly Func<IDataAccess, Hints, Unit> empty;
    internal OutputBinding(
        Port<Shape> input,
        Port port,
        Func<IDataAccess, Hints, GrasshopperRuntime, Seq<Flow<Shape>>, Seq<object>> run,
        Func<IDataAccess, Hints, Unit> empty) {
        ArgumentNullException.ThrowIfNull(argument: input);
        ArgumentNullException.ThrowIfNull(argument: port);
        ArgumentNullException.ThrowIfNull(argument: run);
        ArgumentNullException.ThrowIfNull(argument: empty);
        Input = input;
        Port = port;
        this.run = run;
        this.empty = empty;
    }
    internal Port<Shape> Input { get; }
    internal Port Port { get; }
    internal Seq<object> Run(IDataAccess access, Hints outputs, GrasshopperRuntime runtime, Seq<Flow<Shape>> source) =>
        run(arg1: access, arg2: outputs, arg3: runtime, arg4: source);
    internal Unit Empty(IDataAccess access, Hints outputs) =>
        empty(arg1: access, arg2: outputs);
}

// --- [MODELS] ---------------------------------------------------------------------------
internal readonly record struct Hints(Seq<(Port Port, int Slot)> Inputs) {
    internal static Hints Capture(Seq<(Port Port, IParameter Parameter)> ports, Func<IParameter, int> index) =>
        new(Inputs: ports.Choose(bound => index(arg: bound.Parameter) switch {
            >= 0 and int slot => Some((bound.Port, slot)),
            _ => Option<(Port Port, int Slot)>.None,
        }));
    public Option<int> Slot(Port port) =>
        Inputs.Find(predicate: input => ReferenceEquals(objA: input.Port, objB: port)).Map(static input => input.Slot);
}
// --- [SERVICES] -------------------------------------------------------------------------
public static class Output {
    public static OutputBinding Of<TOut>(
        Port<Shape> input,
        IAspect aspect,
        string name,
        string code,
        string info,
        Access access = Access.Item,
        PortKind? kind = null,
        PortPolicy? policy = null) where TOut : notnull {
        ArgumentNullException.ThrowIfNull(argument: aspect);
        return Of(
            input: input,
            port: Port.Of<TOut>(name: name, code: code, info: info, access: access, kind: kind, policy: policy),
            operation: _ => Fin.Succ(aspect.Operation<object, TOut>()));
    }
    private static OutputBinding Of<TOut>(Port<Shape> input, Port<TOut> port, Func<GrasshopperRuntime, Fin<Operation<object, TOut>>> operation)
        where TOut : notnull {
        ArgumentNullException.ThrowIfNull(argument: input);
        ArgumentNullException.ThrowIfNull(argument: port);
        ArgumentNullException.ThrowIfNull(argument: operation);
        return new OutputBinding(
            input: input,
            port: port,
            run: (access, outputs, runtime, source) => Run(
                port: port,
                operation: operation,
                access: access,
                outputs: outputs,
                runtime: runtime,
                source: source),
            empty: (access, outputs) => outputs.Slot(port: port).Iter(slot => Empty(port: port, access: access, slot: slot)));
    }
    internal static Unit Write(IDataAccess access, GrasshopperRuntime runtime, Seq<OutputBinding> bindings, Hints outputs) {
        Seq<OutputBinding> active = bindings.Filter(binding => outputs.Slot(port: binding.Port).IsSome);
        Seq<Port<Shape>> inputs = active.Map(static binding => binding.Input).Distinct().ToSeq();
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
    internal static Unit Empty(IDataAccess access, Seq<OutputBinding> bindings, Hints outputs) =>
        bindings.Iter(binding => binding.Empty(access: access, outputs: outputs));
    private static Seq<object> Run<TOut>(
        Port<TOut> port,
        Func<GrasshopperRuntime, Fin<Operation<object, TOut>>> operation,
        IDataAccess access,
        Hints outputs,
        GrasshopperRuntime runtime,
        Seq<Flow<Shape>> source) where TOut : notnull {
        ArgumentNullException.ThrowIfNull(argument: access);
        return outputs.Slot(port: port)
            .Map(slot => from context in runtime.Scope.Context
                         from resolved in operation(arg: runtime)
                         from projection in ShapeSource(sourced: source, operation: resolved).Run(env: new Env(Context: context, Progress: runtime.Progress, Cancellation: runtime.Cancellation))
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
        Seq<object> transfers = access.Write<TOut>(slot: slot, port: port, values: values);
        _ = values.Choose(static value => value.Item is TopologyProjection projection ? Some(projection) : Option<TopologyProjection>.None)
            .Iter(projection => _ = transfers.Exists(output => ReferenceEquals(objA: output, objB: projection) || projection.Transfers(output: output)) switch { true => unit, false => projection.Dispose() });
        return transfers;
    }
    private static Unit Empty<TOut>(Port<TOut> port, IDataAccess access, int slot) {
        ArgumentNullException.ThrowIfNull(argument: access);
        return access.Write<TOut>(slot: slot, port: port, values: Seq<Flow<TOut>>()) switch { _ => Unit.Default };
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
            Succ: sourced => bindings.Bind(binding => binding.Run(access: access, outputs: outputs, runtime: runtime, source: sourced)) switch {
                Seq<object> transfers => sourced.Iter(source => source.Item.DisposeUnlessTransferred(outputs: transfers)),
            },
            Fail: error => {
                access.AddWarning(text: error.Category(), details: error.Message);
                return bindings.Iter(binding => binding.Empty(access: access, outputs: outputs));
            });
}
