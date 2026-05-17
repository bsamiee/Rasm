using Foundation.CSharp.Analyzers.Contracts;

namespace Rasm.Analysis;

// --- [TYPES] ------------------------------------------------------------------------------
public interface IAspect {
    public Operation<TGeometry, TOut> Operation<TGeometry, TOut>() where TGeometry : notnull;
}

[BoundaryAdapter]
public sealed record Env(Context Context, IProgress<double>? Progress, CancellationToken Cancellation) {
    public static readonly Eff<Env, Context> Asks = Eff.runtime<Env>().Map(static env => env.Context).As();
    public static readonly Eff<Env, Env> EnvAsks = Eff.runtime<Env>().As();
}

// --- [SERVICES] ---------------------------------------------------------------------------
public sealed partial record Operation<TGeometry, TOut> where TGeometry : notnull {
    private Operation(Op key, Requirement requirement, bool requiresContext, Body body) {
        Key = key;
        Requirement = requirement;
        RequiresContext = requiresContext;
        Execution = body;
    }
    public Op Key { get; }
    internal Requirement Requirement { get; init; }
    internal bool RequiresContext { get; init; }
    private Body Execution { get; init; }
    internal bool IsSupported => Execution is not Body.Rejected;
    internal bool IsAggregate => Execution is Body.Aggregate;
    internal bool NeedsContext => RequiresContext || !Requirement.IsEmpty;
    public Eff<Env, Seq<TOut>> Apply(Seq<TGeometry> geometry) =>
        Execution.Switch(
            rejected: r => Fin.Fail<Seq<TOut>>(r.Fault).ToEff(),
            perItem: i => geometry.TraverseM(i.Evaluate).As().Map(static chunks => chunks.Bind(static chunk => chunk)),
            aggregate: a => a.Evaluate(arg: geometry));
    internal static Operation<TGeometry, TOut> Build(Op key, Func<TGeometry, Eff<Env, Seq<TOut>>> evaluator, Requirement? requirement = null, bool requiresContext = false, Option<Func<Seq<TGeometry>, Eff<Env, Seq<TOut>>>> aggregate = default) =>
        Build(key: key, state: Unit.Default, evaluator: (_, geometry) => evaluator(arg: geometry), requirement: requirement, requiresContext: requiresContext, aggregate: aggregate);
    internal static Operation<TGeometry, TOut> Build<TState>(Op key, TState state, Func<TState, TGeometry, Eff<Env, Seq<TOut>>> evaluator, Requirement? requirement = null, bool requiresContext = false, Option<Func<Seq<TGeometry>, Eff<Env, Seq<TOut>>>> aggregate = default) {
        Requirement active = requirement ?? Requirement.None;
        return aggregate.Match(
            Some: project => new Operation<TGeometry, TOut>(
                key: key,
                requirement: active,
                requiresContext: requiresContext,
                body: new Body.Aggregate(Evaluate: geometry =>
                    from resolved in geometry.TraverseM(item => Prepare(geometry: item, requirement: active)).As()
                    from result in project(arg: resolved)
                    select result)),
            None: () => new Operation<TGeometry, TOut>(
                key: key,
                requirement: active,
                requiresContext: requiresContext,
                body: new Body.PerItem(Evaluate: geometry =>
                    from prepared in Prepare(geometry: geometry, requirement: active)
                    from value in evaluator(arg1: state, arg2: prepared)
                    select value)));
    }
    internal static Operation<TGeometry, TOut> Reject(Op key, Error fault) =>
        new(key: key, requirement: Requirement.None, requiresContext: false, body: new Body.Rejected(Fault: fault));
    internal Fin<Operation<TGeometry, TOut>> Supported() =>
        Execution switch {
            Body.Rejected rejected => Fin.Fail<Operation<TGeometry, TOut>>(rejected.Fault),
            _ => Fin.Succ(this),
        };
    private static Eff<Env, TGeometry> Prepare(TGeometry geometry, Requirement requirement) =>
        from runtime in Env.EnvAsks
        from ready in (runtime.Cancellation.IsCancellationRequested switch {
            true => Fin.Fail<TGeometry>(new Fault.Cancelled()),
            false => Optional(geometry).ToFin(new Fault.MissingGeometry()),
        }).ToEff()
        from validated in (requirement.IsEmpty, ready) switch {
            (false, GeometryBase native) => from _ in requirement.Apply(context: runtime.Context, value: native, cancel: runtime.Cancellation).ToEff()
                                            select ready,
            _ => Fin.Succ(ready).ToEff(),
        }
        select validated;
    [Union]
    private abstract partial record Body {
        private Body() { }
        internal sealed record Rejected(Error Fault) : Body;
        internal sealed record PerItem(Func<TGeometry, Eff<Env, Seq<TOut>>> Evaluate) : Body;
        internal sealed record Aggregate(Func<Seq<TGeometry>, Eff<Env, Seq<TOut>>> Evaluate) : Body;
    }
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static partial class Analyze {
    public static Validation<Error, Seq<TOut>> Run<TGeometry, TOut>(
        Operation<TGeometry, TOut>? operation,
        params ReadOnlySpan<TGeometry> input) where TGeometry : notnull =>
        Run(
            operation: operation,
            scope: Option<Scope>.None,
            input: input);
    public static Scope From(RhinoDoc? doc) => new(context: Context.Of(doc: doc).ToFin());
    public static Scope In(UnitSystem units) => new(context: Context.Of(units: units).ToFin());
    public static Scope In(double absolute, double relative, double angle, UnitSystem units) =>
        new(context: Context.Of(absolute: absolute, relative: relative, angle: angle, units: units).ToFin());
    public static Scope In(Context context) => new(context: Optional(context).ToFin(Op.Of(name: nameof(Scope)).MissingContext()));
    public sealed record Scope {
        public Fin<Context> Context { get; }
        public IProgress<double>? Progress { get; init; }
        public CancellationToken Cancellation { get; init; }
        internal Scope(Fin<Context> context) => Context = context;
        public Scope With(IProgress<double> progress) => this with { Progress = progress };
        public Scope With(CancellationToken cancellation) => this with { Cancellation = cancellation };
        public Validation<Error, Seq<TOut>> Run<TGeometry, TOut>(
            Operation<TGeometry, TOut>? operation,
            params ReadOnlySpan<TGeometry> input) where TGeometry : notnull => Analyze.Run(
                operation: operation,
                scope: Some(this),
                input: input);
    }
    private static Validation<Error, Seq<TOut>> Run<TGeometry, TOut>(
        Operation<TGeometry, TOut>? operation,
        Option<Scope> scope,
        ReadOnlySpan<TGeometry> input) where TGeometry : notnull {
        TGeometry[] inputValues = input.ToArray();
        (IProgress<double>? progress, CancellationToken cancellation) = scope.Case switch {
            Scope active => (active.Progress, active.Cancellation),
            _ => (null, CancellationToken.None),
        };
        return (
            from active in Optional(operation).ToFin(new Fault.MissingOperation())
            from accepted in active.Supported()
            from context in scope.Map(static s => s.Context).Match(
                Some: provided => provided,
                None: () => accepted.NeedsContext switch {
                    true => Fin.Fail<Context>(accepted.Key.MissingContext()),
                    false => Context.Of(units: UnitSystem.Millimeters).ToFin(),
                })
            from result in accepted.Apply(geometry: inputValues.AsIterable().ToSeq()).Run(env: new Env(Context: context, Progress: progress, Cancellation: cancellation))
            select result).ToValidation();
    }
    public static Operation<TGeometry, TOut> Aspect<TAspect, TGeometry, TOut>(TAspect? aspect, [CallerMemberName] string callerMember = "")
        where TAspect : class, IAspect
        where TGeometry : notnull =>
        aspect?.Operation<TGeometry, TOut>() ?? Operation<TGeometry, TOut>.Reject(key: Op.Of(name: callerMember), fault: Op.Of(name: callerMember).InvalidInput());
    internal static Operation<TGeometry, TOut> Unsupported<TGeometry, TOut>(this Op key) where TGeometry : notnull =>
        Operation<TGeometry, TOut>.Reject(key: key, fault: key.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(TOut)));
    internal static Operation<TGeometry, TOut> As<TGeometry, TOut>(this object operation, Op key) where TGeometry : notnull => operation switch {
        Operation<TGeometry, TOut> typed => typed,
        _ => Operation<TGeometry, TOut>.Reject(key: key, fault: key.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(TOut))),
    };
    internal static Operation<TGeometry, TOut> Native<TGeometry, TOut, TNative, TValue, TState>(Op key, TState state, Func<TState, TNative, Eff<Env, Seq<TValue>>> project, Requirement? requirement = null, bool requiresContext = false) where TGeometry : notnull where TNative : notnull =>
        Operation<TGeometry, TValue>.Build(
            key: key, requirement: requirement, requiresContext: requiresContext, state: (Key: key, State: state, Project: project),
            evaluator: static (state, geometry) => geometry switch {
                TNative native => state.Project(arg1: state.State, arg2: native),
                _ => Fin.Fail<Seq<TValue>>(state.Key.Unsupported(geometryType: geometry.GetType(), outputType: typeof(TValue))).ToEff(),
            }).As<TGeometry, TOut>(key: key);
}

internal static class ValidationLifts {
    internal static Eff<Env, T> ToEff<T>(this Validation<Error, T> validation) => validation.ToFin();
}
