namespace Rasm.Analysis;

public static class Analyze {
    internal sealed record Runtime(Context Context, CancellationToken Cancellation, IProgress<double>? Progress);
    internal static readonly Eff<Runtime, Context> Asks = Eff.runtime<Runtime>().Map(static runtime => runtime.Context).As();
    internal static readonly Eff<Runtime, Runtime> RuntimeAsks = Eff.runtime<Runtime>().As();
    public static Validation<Error, Seq<TOut>> Run<TGeometry, TOut>(
        Query<TGeometry, TOut>? query,
        params ReadOnlySpan<TGeometry> input) where TGeometry : notnull =>
        Run(
            query: query,
            scope: Option<Fin<Context>>.None,
            input: input);
    public static Scope From(RhinoDoc? doc) => new(context: Context.FromDocument(doc: doc).ToFin());
    public static Scope In(UnitSystem units) => new(context: Context.CreateDefault(units: units).ToFin());
    public static Scope In(
        double absolute,
        double relative,
        double angle,
        UnitSystem units) =>
        new(
            context: Context.FromKnownUnits(
                    absoluteTolerance: absolute, relativeTolerance: relative, angleToleranceRadians: angle, units: units)
                .ToFin());
    public static Scope In(Context context) => new(context: Optional(context).ToFin(Query.ScopeKey.MissingContext()));
    public sealed record Scope {
        public Fin<Context> Context { get; }
        internal Scope(Fin<Context> context) => Context = context;
        public Validation<Error, Seq<TOut>> Run<TGeometry, TOut>(
            Query<TGeometry, TOut>? query,
            params ReadOnlySpan<TGeometry> input) where TGeometry : notnull => Analyze.Run(
                query: query,
                scope: Some(Context),
                input: input);
    }
    private static Validation<Error, Seq<TOut>> Run<TGeometry, TOut>(
        Query<TGeometry, TOut>? query,
        Option<Fin<Context>> scope,
        ReadOnlySpan<TGeometry> input) where TGeometry : notnull {
        TGeometry[] inputValues = input.ToArray();
        return Optional(query)
            .ToFin(new OpFault.MissingOperation())
            .ToValidation()
            .Bind(active => active.Rejection.Match(
                Some: error => Fin.Fail<Seq<TOut>>(error).ToValidation(),
                None: () => {
                    Fin<(Query<TGeometry, TOut> Query, Context Context)> ready = ResolveContext(query: active, scope: scope).Map(context => (Query: active, Context: context));
                    TGeometry[] materialized = ready.IsSucc switch {
                        true => inputValues,
                        false => [],
                    };
                    return ready.Match(
                        Succ: state => Execute(query: state.Query, runtime: new Runtime(Context: state.Context, Cancellation: CancellationToken.None, Progress: null), input: materialized), Fail: error => Fin.Fail<Seq<TOut>>(error).ToValidation());
                }));
    }
    private static Fin<Context> ResolveContext<TGeometry, TOut>(
        Query<TGeometry, TOut> query,
        Option<Fin<Context>> scope) where TGeometry : notnull =>
        scope.Match(
            Some: provided => provided,
            None: () => (query.RequiresContext || !query.Requirement.IsEmpty) switch {
                true => Fin.Fail<Context>(query.Key.MissingContext()),
                false => Context.CreateDefault(units: UnitSystem.Millimeters).ToFin(),
            });
    private static Validation<Error, Seq<TOut>> Execute<TGeometry, TOut>(
        Query<TGeometry, TOut> query,
        Runtime runtime,
        TGeometry[] input) where TGeometry : notnull =>
        query.Apply(geometry: input.AsIterable().ToSeq())
            .Run(env: runtime)
            .ToValidation();
}
internal static class ValidationLifts {
    internal static Eff<Analyze.Runtime, T> ToEff<T>(this Validation<Error, T> validation) => validation.ToFin();
}
