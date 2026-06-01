using System.Globalization;
using Requirement = Grasshopper2.Parameters.Requirement;

namespace Rasm.Grasshopper.Components;

// --- [MODELS] -----------------------------------------------------------------------------
public sealed class OutputBinding {
    private readonly Func<GrasshopperRuntime, Fin<Seq<Flow<object>>>> read;
    private readonly Func<IDataAccess, Hints, GrasshopperRuntime, Seq<Flow<object>>, Seq<object>> run;
    private readonly Func<Seq<Flow<object>>, Seq<object>, Unit> release;
    private readonly Func<IDataAccess, Hints, Unit> empty;
    private OutputBinding(
        Port input,
        Port output,
        Func<GrasshopperRuntime, Fin<Seq<Flow<object>>>> read,
        Func<IDataAccess, Hints, GrasshopperRuntime, Seq<Flow<object>>, Seq<object>> run,
        Func<Seq<Flow<object>>, Seq<object>, Unit> release,
        Func<IDataAccess, Hints, Unit> empty) {
        Input = input;
        Port = output;
        this.read = read;
        this.run = run;
        this.release = release;
        this.empty = empty;
    }
    internal Port Input { get; }
    internal Port Port { get; }
    internal Fin<Seq<Flow<object>>> Read(GrasshopperRuntime runtime) => read(arg: runtime);
    internal Seq<object> Run(IDataAccess access, Hints outputs, GrasshopperRuntime runtime, Seq<Flow<object>> source) =>
        run(arg1: access, arg2: outputs, arg3: runtime, arg4: source);
    internal Unit Release(Seq<Flow<object>> source, Seq<object> transfers) => release(arg1: source, arg2: transfers);
    internal Unit Empty(IDataAccess access, Hints outputs) => empty(arg1: access, arg2: outputs);
    public static OutputBinding Of<TOut>(
        Port<Shape> input,
        IAspect aspect,
        string name,
        string code,
        string info,
        Access access = Access.Item,
        Capability? policy = null) where TOut : notnull {
        ArgumentNullException.ThrowIfNull(argument: input);
        ArgumentNullException.ThrowIfNull(argument: aspect);
        return Make(
            input: input,
            output: Port.Of<TOut>(name: name, code: code, info: info, access: access, requirement: Requirement.MustExist, kind: null, policy: policy, side: Side.Output),
            operation: _ => Fin.Succ(aspect.Operation<object, TOut>()),
            read: runtime => runtime.Shape(port: input).Map(static flows => flows.Map(static flow => flow.Project(item: (object)flow.Item))),
            lift: static flow => ((Shape)flow.Item).Inner,
            detach: static (flow, output) => ((Shape)flow.Item).Detach(output: output),
            release: static (source, transfers) => source.Iter(flow => ((Shape)flow.Item).DisposeUnlessTransferred(outputs: transfers)));
    }
    private static OutputBinding Make<TOut>(
        Port input,
        Port<TOut> output,
        Func<GrasshopperRuntime, Fin<Operation<object, TOut>>> operation,
        Func<GrasshopperRuntime, Fin<Seq<Flow<object>>>> read,
        Func<Flow<object>, object> lift,
        Func<Flow<object>, Flow<TOut>, Flow<TOut>> detach,
        Func<Seq<Flow<object>>, Seq<object>, Unit> release) where TOut : notnull =>
        new(
            input: input,
            output: output,
            read: read,
            run: (access, outputs, runtime, source) => Output.Run(port: output, operation: operation, lift: lift, detach: detach, access: access, outputs: outputs, runtime: runtime, source: source),
            release: release,
            empty: (access, outputs) => outputs.Slot(port: output).Iter(slot => Output.Clear(port: output, access: access, slot: slot)));
}

internal readonly record struct Hints(Seq<(Port Port, int Slot)> Inputs) {
    internal static Hints Capture(Seq<(Port Port, IParameter Parameter)> ports, Func<IParameter, int> index) =>
        new(Inputs: ports.Choose(bound => index(arg: bound.Parameter) switch {
            >= 0 and int slot => Some((bound.Port, slot)),
            _ => Option<(Port Port, int Slot)>.None,
        }));
    public Option<int> Slot(Port port) =>
        Inputs.Find(predicate: input => string.Equals(a: input.Port.Code, b: port.Code, comparisonType: StringComparison.Ordinal) && input.Port.Kind == port.Kind).Map(static input => input.Slot);
}

// --- [SERVICES] ---------------------------------------------------------------------------
internal static class Output {
    internal static Unit Write(IDataAccess access, GrasshopperRuntime runtime, Seq<OutputBinding> bindings, Hints outputs) {
        Seq<OutputBinding> active = bindings.Filter(binding => outputs.Slot(port: binding.Port).IsSome);
        Seq<Port> inputs = active.Map(static binding => binding.Input).Distinct().ToSeq();
        return active.IsEmpty switch {
            true => Unit.Default,
            false => inputs.Iter(input => RunGroup(access: access, outputs: outputs, runtime: runtime, group: active.Filter(binding => ReferenceEquals(objA: binding.Input, objB: input)))),
        };
    }
    internal static Unit Empty(IDataAccess access, Seq<OutputBinding> bindings, Hints outputs) =>
        bindings.Iter(binding => binding.Empty(access: access, outputs: outputs));
    private static Unit RunGroup(IDataAccess access, Hints outputs, GrasshopperRuntime runtime, Seq<OutputBinding> group) =>
        group[0].Read(runtime: runtime).Match(
            Succ: source => group.Bind(binding => binding.Run(access: access, outputs: outputs, runtime: runtime, source: source)) switch {
                Seq<object> transfers => group[0].Release(source: source, transfers: transfers),
            },
            // Wiring faults stay recoverable (Warning); a genuine compute/scope failure surfaces as Error.
            Fail: error => access.Emit(severity: PortFault.SeverityOf(error: error), text: error.Category(), details: error.Message) switch {
                _ => group.Iter(binding => binding.Empty(access: access, outputs: outputs)),
            });

    // --- [OPERATIONS] -------------------------------------------------------------------------
    private readonly record struct Projection<T>(Seq<Flow<T>> Values, Seq<Fault.Unsupported> Unsupported);
    internal static Seq<object> Run<TOut>(
        Port<TOut> port,
        Func<GrasshopperRuntime, Fin<Operation<object, TOut>>> operation,
        Func<Flow<object>, object> lift,
        Func<Flow<object>, Flow<TOut>, Flow<TOut>> detach,
        IDataAccess access,
        Hints outputs,
        GrasshopperRuntime runtime,
        Seq<Flow<object>> source) where TOut : notnull {
        ArgumentNullException.ThrowIfNull(argument: access);
        return outputs.Slot(port: port)
            .Map(slot => from context in runtime.Scope.Context
                         from resolved in operation(arg: runtime)
                         from projection in Project(source: source, operation: resolved, lift: lift, detach: detach).Run(env: new Env(Context: context, Progress: runtime.Progress, Cancellation: runtime.Cancellation))
                         select projection.Values.IsEmpty switch {
                             true when projection.Unsupported.IsEmpty => access.Emit(severity: Severity.Remark, text: port.Name, details: "No result for sourced input.") switch { _ => Clear(port: port, access: access, slot: slot) switch { _ => Seq<object>() } },
                             true => RemarkUnsupported(port: port, access: access, faults: projection.Unsupported) switch { _ => Clear(port: port, access: access, slot: slot) switch { _ => Seq<object>() } },
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
                _ = outputs.Slot(port: port).Iter(slot => Clear(port: port, access: access, slot: slot));
                return Seq<object>();
            });
    }
    private static Eff<Env, Projection<TOut>> Project<TOut>(
        Seq<Flow<object>> source,
        Operation<object, TOut> operation,
        Func<Flow<object>, object> lift,
        Func<Flow<object>, Flow<TOut>, Flow<TOut>> detach) where TOut : notnull =>
        operation.IsAggregate switch {
            true => from items in operation.Apply(geometry: source.Map(flow => lift(arg: flow)))
                    let merged = items.Map(item => new Flow<TOut>(
                        Pear: Pear<TOut>.Create(item: item, meta: MetaData.FindCommonData(source.Map(static flow => flow.Meta).AsIterable())),
                        Site: Option<Site>.None))
                    // Aggregate output detaches once against the representative (first) source, not N times across all sources.
                    let result = source.Head.Map(head => merged.Map(output => detach(arg1: head, arg2: output))).IfNone(merged)
                    select new Projection<TOut>(Values: result, Unsupported: Seq<Fault.Unsupported>()),
            false => from values in source.TraverseM(flow => operation.Apply(geometry: Seq(lift(arg: flow)))
                         .Map(items => new Projection<TOut>(Values: flow.Project(items: items).Map(output => detach(arg1: flow, arg2: output)), Unsupported: Seq<Fault.Unsupported>()))
                         .IfFailEff(error => error switch {
                             Fault.Unsupported unsupported => Fin.Succ(new Projection<TOut>(Values: Seq<Flow<TOut>>(), Unsupported: Seq(unsupported))).ToEff(),
                             _ => Fin.Fail<Projection<TOut>>(error).ToEff(),
                         })).As()
                     select new Projection<TOut>(
                         Values: values.Bind(static value => value.Values),
                         Unsupported: values.Bind(static value => value.Unsupported)),
        };
    private static Seq<object> Drain<TOut>(Port<TOut> port, int slot, Seq<Flow<TOut>> values, IDataAccess access) {
        Seq<object> transfers = access.Write(slot: slot, port: port, values: values);
        _ = values.Choose(static value => value.Item is TopologyProjection projection ? Some(projection) : Option<TopologyProjection>.None)
            .Iter(projection => _ = transfers.Exists(output => ReferenceEquals(objA: output, objB: projection) || projection.Transfers(output: output)) switch { true => unit, false => projection.Dispose() });
        return transfers;
    }
    internal static Unit Clear<TOut>(Port<TOut> port, IDataAccess access, int slot) {
        ArgumentNullException.ThrowIfNull(argument: access);
        return access.Write(slot: slot, port: port, values: Seq<Flow<TOut>>()) switch { _ => Unit.Default };
    }
    private static Unit RemarkUnsupported(Port port, IDataAccess access, Seq<Fault.Unsupported> faults) {
        Seq<Fault.Unsupported> found = faults.Distinct().ToSeq();
        Option<string> details = found.IsEmpty switch {
            true => Option<string>.None,
            false => Some(found.Count switch {
                1 => found.Head.Map(static fault => fault.Message).IfNone("Unsupported source."),
                int count => string.Join(separator: Environment.NewLine, values: string.Create(CultureInfo.InvariantCulture, $"Unsupported source/output combinations: {count}").Cons(found.Map(static fault => fault.Message)).AsIterable()),
            }),
        };
        return details switch {
            { IsSome: true, Case: string text } => access.Emit(severity: Severity.Remark, text: port.Name, details: text),
            _ => Unit.Default,
        };
    }
    private static Unit Warn(Port port, IDataAccess access, Error error) =>
        access.Emit(severity: Severity.Warning, text: port.Name, details: error.Message);
}
