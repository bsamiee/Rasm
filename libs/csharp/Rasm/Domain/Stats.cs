namespace Rasm.Domain;

// --- [MODELS] -----------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
public readonly record struct Stats {
    internal Stats(int count, double minimum, double maximum, double mean, double variance) {
        Count = count;
        Minimum = minimum;
        Maximum = maximum;
        Mean = mean;
        Variance = variance;
    }
    internal int Count { get; }
    internal double Minimum { get; }
    internal double Maximum { get; }
    internal double Mean { get; }
    internal double Variance { get; }
    internal double Rms => Math.Sqrt(d: (Mean * Mean) + Variance);
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
}
