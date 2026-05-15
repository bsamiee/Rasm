namespace Rasm.Domain;

// --- [TYPES] ------------------------------------------------------------------------------
public enum CurvatureScalar { None = 0, Magnitude = 1, Gaussian = 2, Mean = 3 }

// --- [MODELS] -----------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
public readonly record struct StatProfile {
    private StatProfile(CurvatureScalar scalar, Stats stats, Option<double> limit = default) { Scalar = scalar; Stats = stats; Limit = limit; }
    public CurvatureScalar Scalar { get; }
    public Stats Stats { get; }
    internal Option<double> Limit { get; }
    internal int Count => Stats.Count;
    internal double Minimum => Stats.Minimum;
    internal double Maximum => Stats.Maximum;
    internal double Mean => Stats.Mean;
    internal double Variance => Stats.Variance;
    internal double Rms => Stats.Rms;
    internal double Tolerance => Limit.IfNone(0.0);
    internal bool WithinTolerance => Limit.Case switch { double tolerance => Stats.Maximum <= tolerance, _ => false };
    internal static Fin<StatProfile> Curvature(Seq<double> values, CurvatureScalar scalar, Op key) =>
        (Stats.From(values: values, key: key), Fin.Succ((Scalar: scalar, Key: key)))
            .Apply(static (stats, state) => (Stats: stats, state.Scalar, state.Key)).As()
            .Bind(static state => state.Scalar switch {
                CurvatureScalar.None => Fin.Fail<StatProfile>(state.Key.InvalidInput()),
                _ => state.Key.RequireValid(value: new StatProfile(scalar: state.Scalar, stats: state.Stats)),
            });
    internal static Fin<StatProfile> Residual(Seq<double> values, double tolerance, Op key) =>
        (Stats.From(values: values, key: key), Fin.Succ((Tolerance: tolerance, Key: key)))
            .Apply(static (stats, state) => (Stats: stats, state.Tolerance, state.Key)).As()
            .Bind(static state => Residual(tolerance: state.Tolerance, stats: state.Stats, key: state.Key));
    internal static Fin<StatProfile> Residual(double tolerance, Stats stats, Op key) =>
        key.RequireValid(value: new StatProfile(scalar: CurvatureScalar.None, stats: stats, limit: Some(tolerance)));
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct Stats {
    internal Stats(int count, double minimum, double maximum, double mean, double variance) { Count = count; Minimum = minimum; Maximum = maximum; Mean = mean; Variance = variance; }
    internal int Count { get; }
    internal double Minimum { get; }
    internal double Maximum { get; }
    internal double Mean { get; }
    internal double Variance { get; }
    internal double Rms => Math.Sqrt(d: (Mean * Mean) + Variance);
    internal static Seq<TItem> Maxima<TItem>(Seq<TItem> items, Func<TItem, double> projection, double tolerance) =>
        Extrema(items: items, projection: projection, tolerance: tolerance, direction: +1);
    internal static Seq<TItem> Minima<TItem>(Seq<TItem> items, Func<TItem, double> projection, double tolerance) =>
        Extrema(items: items, projection: projection, tolerance: tolerance, direction: -1);
    internal static Fin<Stats> From(Seq<double> values, Op key) =>
        values.Fold(
            initialState: (Count: 0, Mean: 0.0, M2: 0.0, Minimum: double.PositiveInfinity, Maximum: double.NegativeInfinity, AllFinite: true),
            f: static (state, value) => (Count: state.Count + 1, Delta: value - state.Mean) switch {
                (int count, double delta) => (
                    Count: count, Mean: state.Mean + (delta / count), M2: state.M2 + (delta * (value - (state.Mean + (delta / count)))), Minimum: Math.Min(val1: state.Minimum, val2: value), Maximum: Math.Max(val1: state.Maximum, val2: value), AllFinite: state.AllFinite && RhinoMath.IsValidDouble(x: value)),
            }) switch {
                (0, _, _, _, _, _) => Fin.Fail<Stats>(key.InvalidResult()),
                (_, _, _, _, _, false) => Fin.Fail<Stats>(key.InvalidResult()),
                (int count, double mean, double m2, double minimum, double maximum, _) => Fin.Succ(new Stats(
                    count: count, minimum: minimum, maximum: maximum, mean: mean, variance: Math.Max(val1: 0.0, val2: m2 / count))),
            };
    internal static Fin<Seq<double>> ResidualDistances(Seq<ResidualSample> samples, Op key) =>
        ResidualSamples(samples: samples, key: key).Map(static values => values.Map(static sample => sample.Distance));
    internal static Fin<Stats> FromResiduals(Seq<ResidualSample> samples, Op key) =>
        (ResidualDistances(samples: samples, key: key), Fin.Succ(key))
            .Apply(static (values, state) => (Values: values, Key: state)).As()
            .Bind(static state => From(values: state.Values, key: state.Key));
    internal static Fin<ResidualSample> MaximumResidual(Seq<ResidualSample> samples, Op key) =>
        (ResidualSamples(samples: samples, key: key), Fin.Succ(key))
            .Apply(static (values, state) => (Values: values, Key: state)).As()
            .Bind(static state => Maxima(items: state.Values, projection: static sample => sample.Distance, tolerance: 0.0).Head.ToFin(state.Key.InvalidResult()));
    private static Fin<Seq<ResidualSample>> ResidualSamples(Seq<ResidualSample> samples, Op key) =>
        samples.Fold(
            initialState: (Acc: Fin.Succ(Seq<ResidualSample>()), Key: key),
            f: static (state, sample) => state with {
                Acc = (state.Acc, sample switch {
                    { Distance: double distance, Location.IsValid: true } when distance >= 0.0 && RhinoMath.IsValidDouble(x: distance) => Fin.Succ(sample),
                    _ => Fin.Fail<ResidualSample>(state.Key.InvalidResult()),
                }).Apply(static (acc, value) => acc.Add(value: value)).As(),
            }).Acc;
    private static Seq<TItem> Extrema<TItem>(Seq<TItem> items, Func<TItem, double> projection, double tolerance, int direction) => items.Fold(
        initialState: (Best: direction > 0 ? double.NegativeInfinity : double.PositiveInfinity, Hits: Seq<TItem>(), Tolerance: tolerance, Projection: projection, Direction: (double)direction),
        f: static (state, item) => state.Projection(arg: item) switch {
            double score when state.Direction * score > (state.Direction * state.Best) + state.Tolerance => state with { Best = score, Hits = Seq(item) },
            double score when state.Direction * score >= (state.Direction * state.Best) - state.Tolerance => state with { Best = state.Direction * score > state.Direction * state.Best ? score : state.Best, Hits = item.Cons(state.Hits) },
            _ => state,
        }).Hits.Rev();
}
