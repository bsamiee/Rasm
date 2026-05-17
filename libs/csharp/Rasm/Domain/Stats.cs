using Foundation.CSharp.Analyzers.Contracts;

namespace Rasm.Domain;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class ScalarMetric {
    public static readonly ScalarMetric Magnitude = new(key: 0);
    public static readonly ScalarMetric Gaussian = new(key: 1);
    public static readonly ScalarMetric Mean = new(key: 2);
}

[SmartEnum<int>]
public sealed partial class ExtremumDirection {
    public static readonly ExtremumDirection Maximum = new(key: +1);
    public static readonly ExtremumDirection Minimum = new(key: -1);
}

[Union]
public partial record CurvatureMode {
    public sealed record VectorCase : CurvatureMode;
    public sealed record ScalarCase(ScalarMetric Metric) : CurvatureMode;
    public static CurvatureMode Vector => new VectorCase();
    public static CurvatureMode Scalar(ScalarMetric metric) => new ScalarCase(Metric: metric);
}

[Union]
public partial record StatContext {
    public sealed record NoneCase : StatContext;
    public sealed record MetricCase(ScalarMetric Value) : StatContext;
    public sealed record ToleranceCase(double Value, bool WithinTolerance) : StatContext;
    public static StatContext None => new NoneCase();
    public static StatContext Metric(ScalarMetric metric) => new MetricCase(Value: metric);
    public static StatContext Tolerance(double tolerance, double minimum, double maximum) =>
        new ToleranceCase(Value: tolerance, WithinTolerance: Math.Max(val1: Math.Abs(value: minimum), val2: Math.Abs(value: maximum)) <= tolerance);
}

[Union]
internal partial record ResidualAggregate {
    public sealed record DistancesCase : ResidualAggregate;
    public sealed record SummaryCase(double Tolerance) : ResidualAggregate;
    public sealed record MaximumCase : ResidualAggregate;
    public sealed record DistributionCase(Seq<double> Percentiles) : ResidualAggregate;
    public static ResidualAggregate Distances => new DistancesCase();
    public static ResidualAggregate Summary(double tolerance) => new SummaryCase(Tolerance: tolerance);
    public static ResidualAggregate Maximum => new MaximumCase();
}

// --- [MODELS] -----------------------------------------------------------------------------
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct Stat(int Count, double Minimum, double Maximum, double Mean, double Variance, StatContext Context) {
    internal double Rms => Math.Sqrt(d: (Mean * Mean) + Variance);
    internal bool WithinTolerance => Context is StatContext.ToleranceCase t && t.WithinTolerance;
    internal static Fin<Stat> Of(Seq<double> values, Op key, StatContext? context = null) =>
        values.Fold(
            initialState: (Count: 0, Mean: 0.0, M2: 0.0, Minimum: double.PositiveInfinity, Maximum: double.NegativeInfinity, AllFinite: true),
            f: static (state, value) => (Count: state.Count + 1, Delta: value - state.Mean) switch {
                (int count, double delta) => (
                    Count: count, Mean: state.Mean + (delta / count), M2: state.M2 + (delta * (value - (state.Mean + (delta / count)))), Minimum: Math.Min(val1: state.Minimum, val2: value), Maximum: Math.Max(val1: state.Maximum, val2: value), AllFinite: state.AllFinite && RhinoMath.IsValidDouble(x: value)),
            }) switch {
                (0, _, _, _, _, _) or (_, _, _, _, _, false) => Fin.Fail<Stat>(key.InvalidResult()),
                (int count, double mean, double m2, double minimum, double maximum, _) => Fin.Succ(new Stat(
                    Count: count, Minimum: minimum, Maximum: maximum, Mean: mean, Variance: Math.Max(val1: 0.0, val2: m2 / count), Context: context ?? StatContext.None)),
            };
    internal static Seq<TItem> Extrema<TItem>(Seq<TItem> items, Func<TItem, double> projection, double tolerance, ExtremumDirection direction) => items.Fold(
        initialState: (Best: direction.Key > 0 ? double.NegativeInfinity : double.PositiveInfinity, Hits: Seq<TItem>(), Tolerance: tolerance, Projection: projection, Direction: (double)direction.Key),
        f: static (state, item) => state.Projection(arg: item) switch {
            double score when state.Direction * score > (state.Direction * state.Best) + state.Tolerance => state with { Best = score, Hits = Seq(item) },
            double score when state.Direction * score >= (state.Direction * state.Best) - state.Tolerance => state with { Best = state.Direction * score > state.Direction * state.Best ? score : state.Best, Hits = item.Cons(state.Hits) },
            _ => state,
        }).Hits.Rev();
    internal static Fin<TOut> Residuals<TOut>(Seq<ResidualSample> samples, Op key, ResidualAggregate aggregate) =>
        samples.Fold(
            initialState: (Value: Fin.Succ(Seq<ResidualSample>()), Key: key),
            f: static (state, sample) => sample switch {
                ResidualSample valid when OpAcceptance.ValidityOf(source: valid).IfNone(false) => state with { Value = (state.Value, Fin.Succ(sample)).Apply(static (values, accepted) => values.Add(value: accepted)).As() },
                _ => state with { Value = Fin.Fail<Seq<ResidualSample>>(state.Key.InvalidResult()) },
            }).Value.Bind(validated => (aggregate, typeof(TOut)) switch {
                (ResidualAggregate.DistancesCase, Type t) when t == typeof(Seq<double>) => Fin.Succ((TOut)(object)validated.Map(static sample => sample.Distance)),
                (ResidualAggregate.SummaryCase summary, Type t) when t == typeof(Stat) => Of(values: validated.Map(static sample => sample.Distance), key: key)
                    .Map(stat => (TOut)(object)(stat with { Context = StatContext.Tolerance(tolerance: summary.Tolerance, minimum: stat.Minimum, maximum: stat.Maximum) })),
                (ResidualAggregate.MaximumCase, Type t) when t == typeof(ResidualSample) => Extrema(items: validated, projection: static sample => sample.Distance, tolerance: 0.0, direction: ExtremumDirection.Maximum).Head.ToFin(key.InvalidResult()).Map(static sample => (TOut)(object)sample),
                (ResidualAggregate.DistributionCase dist, Type t) when t == typeof(Distribution) => Distribution.Of(values: validated.Map(static sample => sample.Distance), percentiles: dist.Percentiles, key: key).Map(d => (TOut)(object)d),
                _ => Fin.Fail<TOut>(key.Unsupported(geometryType: typeof(ResidualSample), outputType: typeof(TOut))),
            });
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct Distribution(Stat Summary, double Median, double Iqr, Seq<(double Percentile, double Value)> Percentiles) {
    internal static Fin<Distribution> Of(Seq<double> values, Seq<double> percentiles, Op key, StatContext? context = null) =>
        Stat.Of(values: values, key: key, context: context).Map(stat =>
            values.OrderBy(static v => v).AsIterable().ToSeq() switch {
                Seq<double> sorted => new Distribution(
                    Summary: stat,
                    Median: Quantile(sorted: sorted, fraction: 0.5),
                    Iqr: Quantile(sorted: sorted, fraction: 0.75) - Quantile(sorted: sorted, fraction: 0.25),
                    Percentiles: percentiles.Map(p => (Percentile: p, Value: Quantile(sorted: sorted, fraction: p / 100.0)))),
            });
    private static double Quantile(Seq<double> sorted, double fraction) =>
        (sorted.Count - 1) * Math.Clamp(value: fraction, min: 0.0, max: 1.0) switch {
            double idx when idx == Math.Floor(d: idx) => sorted[(int)idx],
            double idx => sorted[(int)Math.Floor(d: idx)] + ((sorted[(int)Math.Ceiling(a: idx)] - sorted[(int)Math.Floor(d: idx)]) * (idx - Math.Floor(d: idx))),
        };
}
