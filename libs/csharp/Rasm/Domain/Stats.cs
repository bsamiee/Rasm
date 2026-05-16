namespace Rasm.Domain;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class ScalarMetric {
    public static readonly ScalarMetric Magnitude = new(key: 0);
    public static readonly ScalarMetric Gaussian = new(key: 1);
    public static readonly ScalarMetric Mean = new(key: 2);
}

[Union]
public partial record CurvatureMode {
    public sealed record VectorCase : CurvatureMode;
    public sealed record ScalarCase(ScalarMetric Metric) : CurvatureMode;
    public static CurvatureMode Vector => new VectorCase();
    public static CurvatureMode Scalar(ScalarMetric metric) => new ScalarCase(Metric: metric);
}

[SmartEnum<int>]
internal sealed partial class ExtremumDirection {
    public static readonly ExtremumDirection Maximum = new(key: +1);
    public static readonly ExtremumDirection Minimum = new(key: -1);
}

// --- [MODELS] -----------------------------------------------------------------------------
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
        Extrema(items: items, projection: projection, tolerance: tolerance, direction: ExtremumDirection.Maximum);
    internal static Seq<TItem> Minima<TItem>(Seq<TItem> items, Func<TItem, double> projection, double tolerance) =>
        Extrema(items: items, projection: projection, tolerance: tolerance, direction: ExtremumDirection.Minimum);
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
    private static Seq<TItem> Extrema<TItem>(Seq<TItem> items, Func<TItem, double> projection, double tolerance, ExtremumDirection direction) => items.Fold(
        initialState: (Best: direction.Key > 0 ? double.NegativeInfinity : double.PositiveInfinity, Hits: Seq<TItem>(), Tolerance: tolerance, Projection: projection, Direction: (double)direction.Key),
        f: static (state, item) => state.Projection(arg: item) switch {
            double score when state.Direction * score > (state.Direction * state.Best) + state.Tolerance => state with { Best = score, Hits = Seq(item) },
            double score when state.Direction * score >= (state.Direction * state.Best) - state.Tolerance => state with { Best = state.Direction * score > state.Direction * state.Best ? score : state.Best, Hits = item.Cons(state.Hits) },
            _ => state,
        }).Hits.Rev();
}
[StructLayout(LayoutKind.Auto)]
public readonly record struct Stat {
    private Stat(Option<ScalarMetric> metric, Stats stats, Option<double> limit = default) { Metric = metric; Stats = stats; Limit = limit; }
    public Option<ScalarMetric> Metric { get; }
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
    internal static Fin<Stat> Curvature(Seq<double> values, ScalarMetric metric, Op key) =>
        (Stats.From(values: values, key: key), Fin.Succ(metric))
            .Apply(static (stats, m) => new Stat(metric: Some(m), stats: stats))
            .As();
    internal static Fin<Stat> Residual(double tolerance, Stats stats, Op key) =>
        (RhinoMath.IsValidDouble(x: tolerance), tolerance >= 0.0, stats.Minimum >= 0.0, stats.Mean >= 0.0) switch {
            (true, true, true, true) => Fin.Succ(new Stat(metric: Option<ScalarMetric>.None, stats: stats, limit: Some(tolerance))),
            _ => Fin.Fail<Stat>(key.InvalidResult()),
        };
    internal static Fin<Seq<double>> ResidualDistances(Seq<ResidualSample> samples, Op key) =>
        ResidualSamples(samples: samples, key: key).Map(static values => values.Map(static sample => sample.Distance));
    internal static Fin<Stats> FromResiduals(Seq<ResidualSample> samples, Op key) =>
        (ResidualDistances(samples: samples, key: key), Fin.Succ(key))
            .Apply(static (values, k) => (Values: values, Key: k)).As()
            .Bind(static state => Stats.From(values: state.Values, key: state.Key));
    internal static Fin<ResidualSample> MaximumResidual(Seq<ResidualSample> samples, Op key) =>
        (ResidualSamples(samples: samples, key: key), Fin.Succ(key))
            .Apply(static (values, k) => (Values: values, Key: k)).As()
            .Bind(static state => Stats.Maxima(items: state.Values, projection: static sample => sample.Distance, tolerance: 0.0).Head.ToFin(state.Key.InvalidResult()));
    private static Fin<Seq<ResidualSample>> ResidualSamples(Seq<ResidualSample> samples, Op key) =>
        samples.Fold(
            initialState: (Value: Fin.Succ(Seq<ResidualSample>()), Key: key),
            f: static (state, sample) => sample switch {
                { Distance: double distance, Location.IsValid: true } when distance >= 0.0 && RhinoMath.IsValidDouble(x: distance) =>
                    state with { Value = (state.Value, Fin.Succ(sample)).Apply(static (values, valid) => values.Add(value: valid)).As() },
                _ => state with { Value = Fin.Fail<Seq<ResidualSample>>(state.Key.InvalidResult()) },
            }).Value;
}
