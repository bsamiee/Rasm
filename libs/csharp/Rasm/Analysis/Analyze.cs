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
public sealed record Operation<TGeometry, TOut> where TGeometry : notnull {
    internal Operation(Op key, Func<Seq<TGeometry>, Eff<Env, Seq<TOut>>> effect, Requirement? requirement = null, bool requiresContext = false, Option<Func<Seq<TGeometry>, Eff<Env, Seq<TOut>>>> aggregate = default, Option<Error> rejection = default, bool aggregates = false, Unit _ = default)
        : this(key: key, evaluate: effect, requirement: requirement ?? Requirement.None, requiresContext: requiresContext, aggregatePlan: aggregate, rejection: rejection, aggregates: aggregates) { }
    internal Operation(Op key, Func<Seq<TGeometry>, Eff<Env, Seq<TOut>>> evaluate, Requirement requirement, bool requiresContext, Option<Func<Seq<TGeometry>, Eff<Env, Seq<TOut>>>> aggregatePlan, Option<Error> rejection, bool aggregates) {
        Key = key;
        Evaluate = evaluate;
        Requirement = requirement;
        RequiresContext = requiresContext;
        AggregatePlan = aggregatePlan;
        Rejection = rejection;
        Aggregates = aggregates;
    }
    public Op Key { get; }
    internal Func<Seq<TGeometry>, Eff<Env, Seq<TOut>>> Evaluate { get; init; }
    internal Requirement Requirement { get; init; }
    internal bool RequiresContext { get; init; }
    internal Option<Func<Seq<TGeometry>, Eff<Env, Seq<TOut>>>> AggregatePlan { get; init; }
    internal Option<Error> Rejection { get; init; }
    internal bool Aggregates { get; init; }
    internal bool NeedsContext => RequiresContext || !Requirement.IsEmpty;
    public Eff<Env, Seq<TOut>> Apply(TGeometry geometry) => Evaluate(arg: Seq(geometry));
    public Eff<Env, Seq<TOut>> Apply(Seq<TGeometry> geometry) => Evaluate(arg: geometry);
    internal Operation<TIn, TOut> Contramap<TIn>(Func<TIn, TGeometry> map) where TIn : notnull => new(
        key: Key,
        evaluate: input => Evaluate(arg: input.Map(value => map(arg: value))),
        requirement: Requirement,
        requiresContext: RequiresContext,
        aggregatePlan: AggregatePlan.Map<Func<Seq<TIn>, Eff<Env, Seq<TOut>>>>(project => input => project(arg: input.Map(value => map(arg: value)))),
        rejection: Rejection,
        aggregates: Aggregates);
    public Operation<TGeometry, TOut> Aggregate() => AggregatePlan.Match(
        Some: project => this with { Evaluate = project, Aggregates = true },
        None: () => Reject(key: Key, fault: Key.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(TOut))));
    internal static Operation<TGeometry, TOut> Build(Op key, Func<TGeometry, Eff<Env, Seq<TOut>>> evaluator, Requirement? requirement = null, bool requiresContext = false, Option<Func<Seq<TGeometry>, Eff<Env, Seq<TOut>>>> aggregate = default) =>
        Build(key: key, state: Unit.Default, evaluator: (_, geometry) => evaluator(arg: geometry), requirement: requirement, requiresContext: requiresContext, aggregate: aggregate);
    internal static Operation<TGeometry, TOut> Build<TState>(Op key, TState state, Func<TState, TGeometry, Eff<Env, Seq<TOut>>> evaluator, Requirement? requirement = null, bool requiresContext = false, Option<Func<Seq<TGeometry>, Eff<Env, Seq<TOut>>>> aggregate = default) {
        Requirement active = requirement ?? Requirement.None;
        return new(
            key: key,
            requirement: active,
            requiresContext: requiresContext,
            rejection: Option<Error>.None,
            aggregates: false,
            aggregatePlan: aggregate.Map<Func<Seq<TGeometry>, Eff<Env, Seq<TOut>>>>(project => geometry =>
                from resolved in geometry.TraverseM(item => Prepare(geometry: item, requirement: active)).As()
                from result in project(arg: resolved)
                select result),
            evaluate: geometry => from result in geometry.TraverseM(item =>
                                      from prepared in Prepare(geometry: item, requirement: active)
                                      from value in evaluator(arg1: state, arg2: prepared)
                                      select value)
                                  .As()
                                  .Map(static chunks => chunks.Bind(static chunk => chunk))
                                  select result);
    }
    internal static Operation<TGeometry, TOut> Reject(Op key, Error fault) =>
        new(key: key, evaluate: _ => Fin.Fail<Seq<TOut>>(fault).ToEff(), requirement: Requirement.None, requiresContext: false, aggregatePlan: None, rejection: Some(fault), aggregates: false);
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
    public static Scope From(RhinoDoc? doc) => new(context: Context.FromDocument(doc: doc).ToFin());
    public static Scope In(UnitSystem units) => new(context: Context.CreateDefault(units: units).ToFin());
    public static Scope In(double absolute, double relative, double angle, UnitSystem units) =>
        new(context: Context.Create(absolute: absolute, relative: relative, angle: angle, units: units).ToFin());
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
            from accepted in active.Rejection.Match(
                Some: Fin.Fail<Operation<TGeometry, TOut>>,
                None: () => Fin.Succ(active))
            from context in scope.Map(static s => s.Context).Match(
                Some: provided => provided,
                None: () => accepted.NeedsContext switch {
                    true => Fin.Fail<Context>(accepted.Key.MissingContext()),
                    false => Context.CreateDefault(units: UnitSystem.Millimeters).ToFin(),
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
    internal static Operation<TGeometry, TOut> Cast<TGeometry, TOut>(Op key, object operation) where TGeometry : notnull => operation switch {
        Operation<TGeometry, TOut> typed => typed,
        _ => Operation<TGeometry, TOut>.Reject(key: key, fault: key.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(TOut))),
    };
    internal static Operation<TGeometry, TOut> Native<TGeometry, TOut, TNative, TValue>(Op key, Func<TNative, Eff<Env, Seq<TValue>>> project) where TGeometry : notnull where TNative : notnull =>
        Native<TGeometry, TOut, TNative, TValue, Func<TNative, Eff<Env, Seq<TValue>>>>(key: key, state: project, project: static (nativeProject, native) => nativeProject(arg: native));
    internal static Operation<TGeometry, TOut> Native<TGeometry, TOut, TNative, TValue, TState>(Op key, TState state, Func<TState, TNative, Eff<Env, Seq<TValue>>> project, Requirement? requirement = null, bool requiresContext = false) where TGeometry : notnull where TNative : notnull =>
        Cast<TGeometry, TOut>(key: key, operation: Operation<TGeometry, TValue>.Build(
            key: key, requirement: requirement, requiresContext: requiresContext, state: (Key: key, State: state, Project: project),
            evaluator: static (state, geometry) => geometry switch {
                TNative native => state.Project(arg1: state.State, arg2: native),
                _ => Fin.Fail<Seq<TValue>>(state.Key.Unsupported(geometryType: geometry.GetType(), outputType: typeof(TValue))).ToEff(),
            }));
}

internal static class ValidationLifts {
    internal static Eff<Env, T> ToEff<T>(this Validation<Error, T> validation) => validation.ToFin();
}
