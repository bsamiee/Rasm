namespace Rasm.Analysis;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class StatKind {
    public static readonly StatKind Curvature = new(key: 0);
    public static readonly StatKind Magnitude = new(key: 1);
    public static readonly StatKind Gaussian = new(key: 2);
    public static readonly StatKind Mean = new(key: 3);
    public static readonly StatKind Residual = new(key: 4);
    internal bool IsScalar =>
        Equals(Magnitude)
        || Equals(Gaussian)
        || Equals(Mean);
}

// --- [MODELS] -----------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
public readonly record struct Stat {
    private Stat(StatKind kind, Stats stats, Option<double> limit = default) { Kind = kind; Stats = stats; Limit = limit; }
    public StatKind Kind { get; }
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
    internal static Fin<Stat> Curvature(Seq<double> values, StatKind kind, Op key) =>
        (Stats.From(values: values, key: key), Fin.Succ((Kind: kind, Key: key)))
            .Apply(static (stats, state) => (Stats: stats, state.Kind, state.Key)).As()
            .Bind(static state => state.Kind.IsScalar
                ? Fin.Succ(new Stat(kind: state.Kind, stats: state.Stats))
                : Fin.Fail<Stat>(state.Key.InvalidInput()));
    internal static Fin<Stat> Residual(Seq<double> values, double tolerance, Op key) =>
        (Stats.From(values: values, key: key), Fin.Succ((Tolerance: tolerance, Key: key)))
            .Apply(static (stats, state) => (Stats: stats, state.Tolerance, state.Key)).As()
            .Bind(static state => Residual(tolerance: state.Tolerance, stats: state.Stats, key: state.Key));
    internal static Fin<Stat> Residual(double tolerance, Stats stats, Op key) =>
        (RhinoMath.IsValidDouble(x: tolerance), tolerance >= 0.0, stats.Minimum >= 0.0, stats.Mean >= 0.0) switch {
            (true, true, true, true) => Fin.Succ(new Stat(kind: StatKind.Residual, stats: stats, limit: Some(tolerance))),
            _ => Fin.Fail<Stat>(key.InvalidResult()),
        };
    internal static Fin<Seq<double>> ResidualDistances(Seq<ResidualSample> samples, Op key) =>
        ResidualSamples(samples: samples, key: key).Map(static values => values.Map(static sample => sample.Distance));
    internal static Fin<Stats> FromResiduals(Seq<ResidualSample> samples, Op key) =>
        (ResidualDistances(samples: samples, key: key), Fin.Succ(key))
            .Apply(static (values, state) => (Values: values, Key: state)).As()
            .Bind(static state => Stats.From(values: state.Values, key: state.Key));
    internal static Fin<ResidualSample> MaximumResidual(Seq<ResidualSample> samples, Op key) =>
        (ResidualSamples(samples: samples, key: key), Fin.Succ(key))
            .Apply(static (values, state) => (Values: values, Key: state)).As()
            .Bind(static state => Stats.Maxima(items: state.Values, projection: static sample => sample.Distance, tolerance: 0.0).Head.ToFin(state.Key.InvalidResult()));
    private static Fin<Seq<ResidualSample>> ResidualSamples(Seq<ResidualSample> samples, Op key) =>
        samples.TraverseM(sample => sample switch {
            { Distance: double distance, Location.IsValid: true } when distance >= 0.0 && RhinoMath.IsValidDouble(x: distance) => Fin.Succ(sample),
            _ => Fin.Fail<ResidualSample>(key.InvalidResult()),
        }).As();
}
