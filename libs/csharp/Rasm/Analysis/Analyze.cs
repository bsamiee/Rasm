namespace Rasm.Analysis;

public static class Analyze {
    public static Validation<Error, Seq<TOut>> Run<TGeometry, TOut>(
        Query<TGeometry, TOut>? query,
        params ReadOnlySpan<TGeometry> input) where TGeometry : notnull =>
        Run(
            query: query,
            scope: Option<Scope>.None,
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
                    absolute: absolute, relative: relative, angle: angle, units: units)
                .ToFin());
    public static Scope InScaled(
        double absolute,
        double relative,
        double angle,
        UnitSystem units,
        double metersPerUnit) =>
        new(
            context: Context.FromKnownScale(
                    absolute: absolute, relative: relative, angle: angle, units: units, metersPerUnit: metersPerUnit)
                .ToFin());
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
            None: () => (query.RequiresContext || !query.Requirement.IsEmpty) switch {
                true => Fin.Fail<Context>(query.Key.MissingContext()),
                false => Context.CreateDefault(units: UnitSystem.Millimeters).ToFin(),
            });
}
internal static class ValidationLifts {
    internal static Eff<Env, T> ToEff<T>(this Validation<Error, T> validation) => validation.ToFin();
}
