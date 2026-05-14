namespace Rasm.Analysis;

// --- [TYPES] ------------------------------------------------------------------------------
public interface IAspect {
    public Query<TGeometry, TOut> ToQuery<TGeometry, TOut>() where TGeometry : notnull;
}
internal enum ProjectionOwnership { Dispose, Transfer }

// --- [SERVICES] ---------------------------------------------------------------------------
public sealed record Query<TGeometry, TOut>(
    Op Key,
    Func<Seq<TGeometry>, Eff<Env, Seq<TOut>>> Evaluate,
    Requirement Requirement,
    bool RequiresContext,
    Option<Func<Seq<TGeometry>, Eff<Env, Seq<TOut>>>> AggregatePlan,
    Option<Error> Rejection) where TGeometry : notnull {
    internal Query(Op key, Func<Seq<TGeometry>, Eff<Env, Seq<TOut>>> effect, Requirement? requirement = null, bool requiresContext = false, Option<Func<Seq<TGeometry>, Eff<Env, Seq<TOut>>>> aggregate = default, Option<Error> rejection = default, Unit _ = default)
        : this(Key: key, Evaluate: effect, Requirement: requirement ?? Requirement.None, RequiresContext: requiresContext, AggregatePlan: aggregate, Rejection: rejection) { }
    internal bool NeedsContext => RequiresContext || !Requirement.IsEmpty;
    public Eff<Env, Seq<TOut>> Apply(TGeometry geometry) => Evaluate(arg: Seq(geometry));
    public Eff<Env, Seq<TOut>> Apply(Seq<TGeometry> geometry) => Evaluate(arg: geometry);
    internal Query<TIn, TOut> Contramap<TIn>(Func<TIn, TGeometry> map) where TIn : notnull => new(
        Key: Key,
        Evaluate: input => Evaluate(arg: input.Map(value => map(arg: value))),
        Requirement: Requirement,
        RequiresContext: RequiresContext,
        AggregatePlan: AggregatePlan.Map<Func<Seq<TIn>, Eff<Env, Seq<TOut>>>>(project => input => project(arg: input.Map(value => map(arg: value)))),
        Rejection: Rejection);
    public Query<TGeometry, TOut> Aggregate() => AggregatePlan.Match(
        Some: project => this with { Evaluate = project },
        None: () => Reject(key: Key, fault: Key.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(TOut))));
    internal static Query<TGeometry, TOut> Build(Op key, Func<TGeometry, Eff<Env, Seq<TOut>>> evaluator, Requirement? requirement = null, bool requiresContext = false, Option<Func<Seq<TGeometry>, Eff<Env, Seq<TOut>>>> aggregate = default) =>
        Build(key: key, state: Unit.Default, evaluator: (_, geometry) => evaluator(arg: geometry), requirement: requirement, requiresContext: requiresContext, aggregate: aggregate);
    internal static Query<TGeometry, TOut> Build<TState>(Op key, TState state, Func<TState, TGeometry, Eff<Env, Seq<TOut>>> evaluator, Requirement? requirement = null, bool requiresContext = false, Option<Func<Seq<TGeometry>, Eff<Env, Seq<TOut>>>> aggregate = default) {
        Requirement active = requirement ?? Requirement.None;
        return new(
            Key: key,
            Requirement: active,
            RequiresContext: requiresContext,
            Rejection: Option<Error>.None,
            AggregatePlan: aggregate.Map<Func<Seq<TGeometry>, Eff<Env, Seq<TOut>>>>(project => geometry =>
                from runtime in Env.EnvAsks
                from resolved in geometry.Traverse(item => Prepare(geometry: item, requirement: active).Run(env: runtime)).As().ToEff()
                from result in project(arg: resolved)
                select result),
            Evaluate: geometry => from runtime in Env.EnvAsks
                                  from result in geometry.Traverse(item => (
                                      from prepared in Prepare(geometry: item, requirement: active)
                                      from value in evaluator(arg1: state, arg2: prepared)
                                      select value).Run(env: runtime))
                                  .Map(static chunks => chunks.Bind(static chunk => chunk))
                                  .As().ToEff()
                                  select result);
    }
    internal static Query<TGeometry, TOut> Reject(Op key, Error fault) =>
        new(Key: key, Evaluate: _ => Fin.Fail<Seq<TOut>>(fault).ToEff(), Requirement: Requirement.None, RequiresContext: false, AggregatePlan: None, Rejection: Some(fault));
    private static Eff<Env, TGeometry> Prepare(TGeometry geometry, Requirement requirement) =>
        from runtime in Env.EnvAsks
        from ready in (runtime.Cancellation.IsCancellationRequested switch {
            true => Fin.Fail<TGeometry>(new Fault.Cancelled()),
            false => Optional(geometry).ToFin(new Fault.MissingGeometry()),
        }).ToEff()
        from validated in (requirement.IsEmpty, ready) switch {
            (false, GeometryBase native) => from _ in runtime.Context.Validate(geometry: native, requirement: requirement, cancel: runtime.Cancellation).ToEff()
                                            select ready,
            _ => Fin.Succ(ready).ToEff(),
        }
        select validated;
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static partial class Analyze {
    public static Validation<Error, Seq<TOut>> Run<TGeometry, TOut>(
        Query<TGeometry, TOut>? query,
        params ReadOnlySpan<TGeometry> input) where TGeometry : notnull =>
        Run(
            query: query,
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
            Query<TGeometry, TOut>? query,
            params ReadOnlySpan<TGeometry> input) where TGeometry : notnull => Analyze.Run(
                query: query,
                scope: Some(this),
                input: input);
    }
    private static Validation<Error, Seq<TOut>> Run<TGeometry, TOut>(
        Query<TGeometry, TOut>? query,
        Option<Scope> scope,
        ReadOnlySpan<TGeometry> input) where TGeometry : notnull {
        TGeometry[] inputValues = input.ToArray();
        (IProgress<double>? progress, CancellationToken cancellation) = scope.Case switch {
            Scope active => (active.Progress, active.Cancellation),
            _ => (null, CancellationToken.None),
        };
        return (
            from active in Optional(query).ToFin(new Fault.MissingOperation())
            from accepted in active.Rejection.Match(
                Some: Fin.Fail<Query<TGeometry, TOut>>,
                None: () => Fin.Succ(active))
            from context in ResolveContext(query: accepted, scope: scope.Map(static s => s.Context))
            from result in accepted.Apply(geometry: inputValues.AsIterable().ToSeq()).Run(env: new Env(Context: context, Progress: progress, Cancellation: cancellation))
            select result).ToValidation();
    }
    private static Fin<Context> ResolveContext<TGeometry, TOut>(
        Query<TGeometry, TOut> query,
        Option<Fin<Context>> scope) where TGeometry : notnull =>
        scope.Match(
            Some: provided => provided,
            None: () => query.NeedsContext switch {
                true => Fin.Fail<Context>(query.Key.MissingContext()),
                false => Context.CreateDefault(units: UnitSystem.Millimeters).ToFin(),
            });
    public static Query<TGeometry, TOut> Aspect<TAspect, TGeometry, TOut>(TAspect? aspect, [CallerMemberName] string callerMember = "")
        where TAspect : class, IAspect
        where TGeometry : notnull =>
        aspect?.ToQuery<TGeometry, TOut>() ?? Query<TGeometry, TOut>.Reject(key: Op.Of(name: callerMember), fault: Op.Of(name: callerMember).InvalidInput());
    internal static Query<TGeometry, TOut> Unsupported<TGeometry, TOut>(this Op key) where TGeometry : notnull =>
        Query<TGeometry, TOut>.Reject(key: key, fault: key.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(TOut)));
    internal static Query<TGeometry, TOut> Cast<TGeometry, TOut>(Op key, object query) where TGeometry : notnull => query switch {
        Query<TGeometry, TOut> typed => typed,
        _ => Query<TGeometry, TOut>.Reject(key: key, fault: key.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(TOut))),
    };
    internal static Query<TGeometry, TOut> Native<TGeometry, TOut, TNative, TValue>(Op key, Func<TNative, Eff<Env, Seq<TValue>>> project) where TGeometry : notnull where TNative : notnull =>
        Native<TGeometry, TOut, TNative, TValue, Func<TNative, Eff<Env, Seq<TValue>>>>(key: key, state: project, project: static (nativeProject, native) => nativeProject(arg: native));
    internal static Query<TGeometry, TOut> Native<TGeometry, TOut, TNative, TValue, TState>(Op key, TState state, Func<TState, TNative, Eff<Env, Seq<TValue>>> project, Requirement? requirement = null, bool requiresContext = false) where TGeometry : notnull where TNative : notnull =>
        Cast<TGeometry, TOut>(key: key, query: Query<TGeometry, TValue>.Build(
            key: key, requirement: requirement, requiresContext: requiresContext, state: (Key: key, State: state, Project: project),
            evaluator: static (state, geometry) => geometry switch {
                TNative native => state.Project(arg1: state.State, arg2: native),
                _ => Fin.Fail<Seq<TValue>>(state.Key.Unsupported(geometryType: geometry.GetType(), outputType: typeof(TValue))).ToEff(),
            }));
    internal static bool Supports(Type geometry, Type output, Type target, params Type[] native) =>
        output == target && Supports(geometry: geometry, native: native);
    internal static bool Supports(Type geometry, params Type[] native) =>
        geometry == typeof(object) || geometry == typeof(GeometryBase) || native.Any(predicate: candidate => candidate.IsAssignableFrom(c: geometry));
    internal static Fin<Seq<TValue>> One<TValue>(this Op key, TValue value) => key.RequireValid(value: value).Map(static candidate => Seq(candidate));
    internal static Fin<Seq<TValue>> Many<TValue>(this Op key, IEnumerable<TValue> values) =>
        Optional(values).ToFin(key.InvalidResult()).Bind(candidates => candidates.AsIterable().ToSeq().Traverse(value => key.RequireValid(value: value)).As());
    internal static Fin<Seq<TValue>> ManyOrEmpty<TValue>(this Op key, IEnumerable<TValue>? values) =>
        Optional(values).Match(Some: candidates => key.Many(values: candidates), None: () => Fin.Succ(Seq<TValue>()));
    internal static Fin<Seq<TValue>> Solved<TValue>(this Op key, bool isSolved, TValue value) =>
        isSolved switch { true => key.One(value: value), false => Fin.Fail<Seq<TValue>>(key.InvalidResult()) };
    internal static Fin<TOut> Bracket<TResource, TOut>(Func<TResource> factory, Func<TResource, Fin<TOut>> body) where TResource : class, IDisposable {
        using TResource resource = factory();
        return body(arg: resource);
    }
    internal static Fin<Seq<TOut>> Results<TValue, TOut>(this Op key, IEnumerable<TValue> values) => typeof(TValue).Equals(typeof(TOut)) switch {
        true => Many(key: key, values: values).Map(static candidates => candidates.Map(static candidate => (TOut)(object)candidate!)),
        false => Fin.Fail<Seq<TOut>>(key.Unsupported(geometryType: typeof(TValue), outputType: typeof(TOut))),
    };
    internal static Fin<Seq<TValue>> ProjectOwned<TProjection, TValue>(
        Seq<TProjection> all, Seq<TProjection> chosen, ProjectionOwnership ownership,
        Func<Seq<TProjection>, Fin<Seq<TValue>>> project) where TProjection : ITopologyProjection {
        Fin<Seq<TValue>> result = project(arg: chosen);
        bool keep = ownership == ProjectionOwnership.Transfer && result.IsSucc;
        _ = all.Filter(value => !keep || !chosen.Exists(c => c.SameAs(other: value))).Iter(static v => v.Dispose());
        return result;
    }
}

internal static class FoldExtensions {
    internal static Seq<TItem> Maxima<TItem>(this Seq<TItem> items, Func<TItem, double> projection, double tolerance) => Extrema(items: items, projection: projection, tolerance: tolerance, direction: +1);
    internal static Seq<TItem> Minima<TItem>(this Seq<TItem> items, Func<TItem, double> projection, double tolerance) => Extrema(items: items, projection: projection, tolerance: tolerance, direction: -1);
    private static Seq<TItem> Extrema<TItem>(Seq<TItem> items, Func<TItem, double> projection, double tolerance, int direction) => items.Fold(
        initialState: (Best: direction > 0 ? double.NegativeInfinity : double.PositiveInfinity, Hits: Seq<TItem>(), Tolerance: tolerance, Projection: projection, Direction: (double)direction),
        f: static (state, item) => state.Projection(arg: item) switch {
            double score when state.Direction * score > (state.Direction * state.Best) + state.Tolerance => state with { Best = score, Hits = Seq(item) },
            double score when state.Direction * score >= (state.Direction * state.Best) - state.Tolerance => state with { Best = state.Direction * score > state.Direction * state.Best ? score : state.Best, Hits = item.Cons(state.Hits) },
            _ => state,
        }).Hits.Rev();
}

internal static class ValidationLifts {
    internal static Eff<Env, T> ToEff<T>(this Validation<Error, T> validation) => validation.ToFin();
}
