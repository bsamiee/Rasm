# [COMPUTE_MONITOR]

Rasm.Compute stats monitor scores operational streams online: `StreamMonitor` is the closed stateful-capsule family — EWMA control limits, a P²-class quantile sketch, and a bounded window carrying one fitted `Stats/estimator` detector — advanced per sample by `MonitorLane`, every verdict a typed fact the receipt rail folds. Batch changepoint detectors stay `Stats/estimator#ESTIMATOR_LANE` rows (`TemporalSpec` Cusum · BayesianOnline · CorrelatedResidual over `EstimatorModel.Detector`); the detector capsule calls its admitted `FittedModel.Predict` entry and never re-derives a recursion.

`MonitorChannel` rows extract scalar streams from the identical `Seq<ComputeReceipt>` fact stream the `Runtime/receipts#FOLD_PROJECTIONS` views fold, so operational drift detection consumes the standing telemetry with zero new emit path, and a breach lands the `Runtime/receipts#RECEIPT_UNION` `Drift` case the `ComputeInstrumentFan` projects onto `rasm.compute.monitor.breaches`. `MonitorLane.AsDetector` projects a seeded capsule onto the `Solver/clash#CLASH_AND_TWIN` injected-detector slot, so the twin loop gains control-chart discipline through the seam it already holds. `ComputeReceipt`, `CorrelationId`, `AllocationClass`, NodaTime `IClock` (`ClockPolicy` stays at composition), MathNet `Normal.InvCDF`, and `ComparerAccessors.StringOrdinal` arrive settled. Page is HOST-LOCAL.

## [01]-[INDEX]

- [02]-[MONITOR_LANE]: stateful monitor capsules — EWMA control limits, P² quantile sketch, composed estimator detector; receipt-channel extraction rows; the drift verdict and the twin detector projection.

## [02]-[MONITOR_LANE]

- Owner: `StreamMonitor` `[Union]` the stateful capsule family whose cases carry their own advance state; `MonitorVerdict` the per-sample verdict carrier minting the `Drift` receipt case; `MonitorChannel` `[SmartEnum<string>]` the receipt-to-scalar extraction rows; `MonitorLane` the advance fold, stream observation, and the twin-detector projection.
- Cases: `StreamMonitor` ewma (λ-smoothed level against time-varying control limits over an admitted kernel `Stat` baseline) · quantile (five-marker P² sketch tracking one probability against a policy bound) · detector (bounded residual window carrying one fitted `Stats/estimator` detector); `MonitorChannel` solve-residual · factor-residual · remote-seconds · backpressure-depth.
- Entry: `StreamMonitor.OfEwma(string monitorId, double lambda, double falseAlarm, int warmup)`, `OfQuantile(string monitorId, double probability, double limit)`, and `OfDetector(string monitorId, int capacity, FittedModel detector)` — validated mints, `Fin<T>` aborting an out-of-range policy or a non-detector carrier; `MonitorLane.Advance(StreamMonitor monitor, double sample, IClock clock)` — one sample in, the advanced capsule and its `MonitorVerdict` out through the generated total `Switch`; `MonitorLane.Observe(StreamMonitor monitor, MonitorChannel channel, Seq<ComputeReceipt> facts, IClock clock)` — extracts the channel and folds `Advance` across it; `MonitorLane.AsDetector(StreamMonitor.Detector seed, IClock clock)` — the stateful `Func<Matrix<double>, Fin<Prediction>>` projection the clash twin injects, admitting the detector capsule and exactly one evidence column before indexing.
- Auto: the EWMA arm advances the kernel `Stat` moment summary one admitted sample at a time through its warmup — inheriting the sentinel screen a local mean/M2/count triple drops — then smooths `level = λ·x + (1−λ)·level` and derives the time-varying limit `L·σ·√(λ/(2−λ)·(1−(1−λ)^{2t}))` so early samples meet tighter bands and the asymptote is the textbook control limit; `OfEwma` admits a two-sided false-alarm rate and derives `L = Φ⁻¹(1 − α/2)` through `Normal.InvCDF` at the gate, rejecting a non-finite derived limit (a sub-representable rate rounds the quantile argument to 1 and lands `+∞`), so the knob is a probability admitted exactly once on the `Fin` rail, never a bare multiplier; the quantile arm runs the five-marker P² update — sorted seeding over the first five samples, then marker increment, desired-position drift, and parabolic (linear-fallback) height adjustment — tracking the running quantile with O(1) state; the detector arm pushes into its bounded window and delegates scoring to the injected detector, reading the LAST row's score and change flag exactly as the twin does.
- Receipt: `Drift` — monitor id, statistic, optional policy limit, level, breach flag, and window count, minted by `MonitorVerdict.Receipt` under the caller's correlation; a verdict over an unestimated baseline carries `null` because a limit no estimate backs is not a boundary — detector arms because their fitted model owns classification without exposing a scalar, EWMA's rejected arm because the screened sample never advanced the baseline. `Runtime/receipts#RECEIPT_UNION` counts breaches onto `rasm.compute.monitor.breaches`, and `ReceiptFolds.Breaches` is the operational view, so monitor evidence rides the standing stream.
- Packages: MathNet.Numerics (`Normal.InvCDF` static quantile), Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm (project, `Stat` the one admitted moment summary and its sentinel screen), BCL inbox
- Growth: a new online statistic is one `StreamMonitor` case whose arm the `Advance` `Switch` demands at compile time; a new operational stream is one `MonitorChannel` row with its extractor column; a new false-alarm posture is one `OfEwma` rate value at composition; zero new surface — an `EwmaMonitor`/`QuantileTracker`/`DriftDetector` sibling family, a second receipt-scanning loop beside `MonitorChannel`, or a monitor-local CUSUM recursion re-deriving the estimator rows is the rejected form.
- Boundary: capsules are immutable — `Advance` returns the next capsule and the caller owns placement (an `Atom<StreamMonitor>` at a session boundary, a threaded fold in a batch view) per the cell-and-thread law. Inline-array struct fields on the quantile case hold the five markers and positions, so an advance copies fixed state through the stack and allocates no array; a marker set widened past five is a different estimator, not a longer buffer.
- Boundary: changepoint state, thresholds, and anomaly classification for the detector arm live on its fitted `Stats/estimator` model, while the monitor holds only the bounded evidence window. `AsDetector` emits one score and one change flag per evidence row over a shared `Atom` cell that installs the advanced capsule beside the verdicts that transition emitted — the CAS function stays pure and re-runs whole on a losing exchange, where a lock around a mutable local publishes a torn advance the second caller reads as its own.
- Boundary: extraction reads the same `Seq<ComputeReceipt>` the dashboards fold — a monitor that taps `ReceiptSurface.Emit` directly or mints a second fact stream is the deleted form; warmup samples score `Breach: false` because a limit over an unestimated baseline is noise, and the verdict still lands so cadence stays observable. `SolveResidual` and `FactorResidual` are two streams, never one: the first is the physics equilibrium residual a `Solve` fact reports against its convergence target, the second the linear-algebra residual a `Factorization` fact measures against its `ResidualCap`, and a drift in one carries no information about the other.
- Boundary: this lane's P² sketch serves LIVE receipt streams alone. Exact small-sample batch quantiles are the kernel `Rasm/Domain/stats#STATISTICS` `Distribution.Of` fold over a bounded materialized sample, and the branch three-form quantile law binds both — an operational sketch crossing into a bounded-sample reading grades a value no run produced.

```csharp signature
// --- [TYPES] -----------------------------------------------------------------------------------------------

// P² marker state is FIXED at five slots by the estimator itself, so the sketch carries an inline-array struct
// field rather than a collection: an advance copies the markers through the stack, where a builder allocated two
// arrays per sample on the hottest path in the lane. Equality is HAND-WRITTEN over all five slots: the default
// ValueType path reads the single declared field — slot 0 alone — so the containing Quantile record would
// compare one marker of five silently, and Generator.Equals cannot serve (an inline array is no IEnumerable<T>,
// and its lone field is the same one-slot trap).
[InlineArray(5)]
public struct MarkerRow : IEquatable<MarkerRow> {
    private double slot;

    public readonly bool Equals(MarkerRow other) => ((ReadOnlySpan<double>)this).SequenceEqual(other);

    public override readonly bool Equals(object? obj) => obj is MarkerRow other && Equals(other);

    public override readonly int GetHashCode() {
        HashCode hash = new();
        foreach (double value in this) { hash.Add(value); }
        return hash.ToHashCode();
    }
}

// Stateful capsule family: each case carries exactly its own advance state — EWMA level over a Welford
// baseline, P² five-marker sketch, bounded detector window — so a knob record shared across modalities is
// unrepresentable and a new online statistic is one case plus one Advance arm.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record StreamMonitor {
    private StreamMonitor() { }

    public sealed record Ewma(string MonitorId, double Lambda, double ControlL, int Warmup, double Level, Option<Stat> Baseline, long Count) : StreamMonitor;

    public sealed record Quantile(string MonitorId, double Probability, double Limit, MarkerRow Heights, MarkerRow Positions, long Count) : StreamMonitor;

    public sealed record Detector(string MonitorId, int Capacity, Seq<double> Window, FittedModel Model) : StreamMonitor;

    public string Id => Switch(
        ewma: static monitor => monitor.MonitorId,
        quantile: static monitor => monitor.MonitorId,
        detector: static monitor => monitor.MonitorId);

    // EWMA's control knob is a two-sided false-alarm rate admitted once at this gate, the control
    // multiplier derived as L = Φ⁻¹(1 − α/2); a bare-L parameter beside the rate is the deleted form. The derived
    // limit itself is gated: a sub-representable rate rounds the Φ⁻¹ argument to 1 and lands +∞, so the mint
    // rejects any non-finite L — an infinite band can never breach and would silence the monitor forever.
    public static Fin<StreamMonitor> OfEwma(string monitorId, double lambda, double falseAlarm, int warmup) =>
        string.IsNullOrWhiteSpace(monitorId) || !double.IsFinite(lambda) || lambda is <= 0d or > 1d
            || !double.IsFinite(falseAlarm) || falseAlarm is <= 0d or >= 1d || warmup < 2
            ? Fin.Fail<StreamMonitor>(ComputeFault.Create($"<monitor-ewma-policy:{monitorId}>"))
            : Normal.InvCDF(0d, 1d, 1d - falseAlarm / 2d) is var controlL && double.IsFinite(controlL)
                ? Fin.Succ<StreamMonitor>(new Ewma(monitorId, lambda, controlL, warmup, Level: 0d, Baseline: None, Count: 0L))
                : Fin.Fail<StreamMonitor>(ComputeFault.Create($"<monitor-ewma-limit-nonfinite:{monitorId}:{falseAlarm:R}>"));

    public static Fin<StreamMonitor> OfQuantile(string monitorId, double probability, double limit) =>
        string.IsNullOrWhiteSpace(monitorId) || !double.IsFinite(probability) || probability is <= 0d or >= 1d || !double.IsFinite(limit)
            ? Fin.Fail<StreamMonitor>(ComputeFault.Create($"<monitor-quantile-policy:{monitorId}>"))
            : Fin.Succ<StreamMonitor>(new Quantile(monitorId, probability, limit, Heights: default, Positions: default, Count: 0L));

    public static Fin<StreamMonitor> OfDetector(string monitorId, int capacity, FittedModel detector) =>
        string.IsNullOrWhiteSpace(monitorId) || capacity < 8 || detector.Carrier is not EstimatorModel.Detector
            ? Fin.Fail<StreamMonitor>(ComputeFault.Create($"<monitor-detector-policy:{monitorId}>"))
            : Fin.Succ<StreamMonitor>(new Detector(monitorId, capacity, Seq<double>(), detector));
}

public sealed record MonitorVerdict(string MonitorId, string Statistic, double Level, double? Limit, bool Breach, int Window, Instant At) {
    public ComputeReceipt.Drift Receipt(CorrelationId correlation) =>
        new(MonitorId, Statistic, Level, Limit, Breach, Window) {
            Scope = new ReceiptScope.Process(correlation, AllocationClass.SpanStack),
        };
}

// Receipt-to-scalar extraction rows: the identical fact stream the dashboards fold, one extractor column per
// operational stream — a second receipt-scanning loop beside these rows is the deleted form.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MonitorChannel {
    public static readonly MonitorChannel SolveResidual = new("solve-residual",
        static fact => fact is ComputeReceipt.Solve solve ? Some(solve.Residual) : None);
    // Factorization residual is a SECOND stream, not a spelling of the first: a Solve fact reports physics
    // equilibrium against its convergence target, a Factorization fact the measured linear-algebra residual
    // against its ResidualCap. The column is nullable because a route that computed no witness reports none,
    // and Optional keeps that absence out of the moment stream instead of feeding it a fabricated zero.
    public static readonly MonitorChannel FactorResidual = new("factor-residual",
        static fact => fact is ComputeReceipt.Factorization factor ? Optional(factor.TrueResidual) : None);
    public static readonly MonitorChannel RemoteSeconds = new("remote-seconds",
        static fact => fact is ComputeReceipt.RemoteCall call ? call.Elapsed.Map(static elapsed => elapsed.TotalSeconds) : None);
    public static readonly MonitorChannel BackpressureDepth = new("backpressure-depth",
        static fact => fact is ComputeReceipt.Backpressure pressure ? Some((double)pressure.QueueDepth) : None);

    private readonly Func<ComputeReceipt, Option<double>> extract;

    public Seq<double> Extract(Seq<ComputeReceipt> facts) => facts.Choose(extract);
}

// --- [OPERATIONS] ------------------------------------------------------------------------------------------

public static class MonitorLane {
    public static Fin<(StreamMonitor Next, MonitorVerdict Verdict)> Advance(StreamMonitor monitor, double sample, IClock clock) =>
        !double.IsFinite(sample)
            ? Fin.Fail<(StreamMonitor, MonitorVerdict)>(ComputeFault.Create($"<monitor-sample-nonfinite:{monitor.Id}>"))
            : monitor.Switch(
                state: (Sample: sample, At: clock.GetCurrentInstant(), Key: Op.Of(nameof(Advance))),
                ewma: static (s, held) => Fin.Succ(Smoothed(held, s.Sample, s.At, s.Key)),
                quantile: static (s, held) => Fin.Succ(Sketched(held, s.Sample, s.At)),
                detector: static (s, held) => Detected(held, s.Sample, s.At));

    public static Fin<(StreamMonitor Settled, Seq<MonitorVerdict> Verdicts)> Observe(StreamMonitor monitor, MonitorChannel channel, Seq<ComputeReceipt> facts, IClock clock) =>
        Walk(monitor, channel.Extract(facts), clock);

    // Twin-detector projection: the capsule advances across calls in an Atom, and each evidence row lands one
    // score and one change flag so the Anomaly carrier satisfies the clash cardinality proof. Each exchange
    // installs the advanced capsule BESIDE the verdicts that transition emitted, so the swap function stays pure
    // and re-runs whole on a losing exchange while the caller still reads what the accepted transition produced —
    // a lock around a mutable local instead publishes a half-advanced capsule the next caller scores against.
    // `Walk` failure installs the unchanged capsule carrying the fault, so a refused sample never advances state.
    public static Func<Matrix<double>, Fin<Prediction>> AsDetector(StreamMonitor.Detector seed, IClock clock) {
        var cell = Atom((Held: seed, Emitted: Fin.Succ(Seq<MonitorVerdict>())));

        return evidence => evidence.ColumnCount != 1
            ? Fin.Fail<Prediction>(ComputeFault.Create($"<monitor-detector-columns:{evidence.ColumnCount}>"))
            : cell.Swap(state => Walk(state.Held, toSeq(Enumerable.Range(0, evidence.RowCount)).Map(row => evidence[row, 0]), clock)
                    .Match(
                        Succ: walked => (Held: (StreamMonitor.Detector)walked.Settled, Emitted: Fin.Succ(walked.Verdicts)),
                        Fail: fault => (state.Held, Fin.Fail<Seq<MonitorVerdict>>(fault))))
                .Emitted
                .Map(static verdicts => (Prediction)new Prediction.Anomaly(
                    Vector<double>.Build.DenseOfArray([.. verdicts.Map(static verdict => verdict.Level)]),
                    [.. verdicts.Map(static verdict => verdict.Breach)]));
    }

    static Fin<(StreamMonitor Settled, Seq<MonitorVerdict> Verdicts)> Walk(StreamMonitor monitor, Seq<double> samples, IClock clock) =>
        samples.Fold(
            Fin.Succ((Settled: monitor, Verdicts: Seq<MonitorVerdict>())),
            (acc, sample) => acc.Bind(held => Advance(held.Settled, sample, clock)
                .Map(advanced => (advanced.Next, held.Verdicts.Add(advanced.Verdict)))));

    static Fin<(StreamMonitor Next, MonitorVerdict Verdict)> Detected(StreamMonitor.Detector held, double sample, Instant at) {
        StreamMonitor.Detector next = held with { Window = (held.Window.Count >= held.Capacity ? held.Window.Tail : held.Window).Add(sample) };
        Matrix<double> evidence = Matrix<double>.Build.Dense(next.Window.Count, 1, (row, _) => next.Window[row]);
        return next.Model.Predict(evidence).Bind(outcome =>
            outcome is Prediction.Anomaly anomaly && anomaly.Scores.Count == next.Window.Count && anomaly.Changes.Length == next.Window.Count
                ? Fin.Succ(((StreamMonitor)next, new MonitorVerdict(
                    next.MonitorId,
                    "detector-score",
                    anomaly.Scores[anomaly.Scores.Count - 1],
                    null,
                    anomaly.Changes[^1],
                    next.Window.Count,
                    at)))
                : Fin.Fail<(StreamMonitor, MonitorVerdict)>(ComputeFault.Create($"<monitor-detector-carrier:{next.MonitorId}>")));
    }

    // Baseline rides the KERNEL's admitted moment summary advanced one sample at a time, not a local
    // mean/M2/count triple: `Stat.Update` carries the same Welford recurrence PLUS the `ValidityClaim.Finite`
    // screen that rejects the host `RhinoMath.UnsetValue` sentinel a bare `double.IsFinite` admits as an ordinary
    // value — a sentinel entering an operational baseline shifts every later control limit with no diagnostic. A
    // rejected sample advances neither the baseline nor the count, so the warmup admits only measured samples.
    static (StreamMonitor Next, MonitorVerdict Verdict) Smoothed(StreamMonitor.Ewma held, double sample, Instant at, Op key) {
        bool warm = held.Count >= held.Warmup;
        Option<Stat> advanced = warm
            ? held.Baseline
            : held.Baseline.Match(
                Some: prior => Stat.Update(prior, sample, key).ToOption(),
                None: () => Stat.Of(Seq(sample), key).ToOption());
        if (!warm && advanced.IsNone) {
            return (held, new MonitorVerdict(held.MonitorId, "ewma-rejected", sample, null, false, (int)Math.Min(held.Count, int.MaxValue), at));
        }

        long count = held.Count + 1L;
        double mean = advanced.Map(static baseline => baseline.Mean).IfNone(0d);
        double sigma = advanced.Map(static baseline => baseline.Count > 1 ? Math.Sqrt(baseline.Variance) : 0d).IfNone(0d);
        double level = warm ? held.Lambda * sample + (1d - held.Lambda) * held.Level : mean;
        double band = held.ControlL * sigma * Math.Sqrt(
            held.Lambda / (2d - held.Lambda) * (1d - Math.Pow(1d - held.Lambda, 2d * Math.Max(1L, count - held.Warmup))));
        bool breach = warm && sigma > 0d && Math.Abs(level - mean) > band;
        return (
            held with { Level = level, Baseline = advanced, Count = count },
            new MonitorVerdict(held.MonitorId, "ewma-level", level, mean + Math.Sign(level - mean) * band, breach, (int)Math.Min(count, int.MaxValue), at));
    }

    // Jain–Chlamtac P²: insertion-sorted five-sample seeding, then marker increment, desired-position drift, and
    // parabolic height adjustment with the linear fallback — O(1) state per tracked probability. Every buffer the
    // textbook spelling allocates per sample — seed array, working heights, working positions, desired positions —
    // is an inline-array copy, an in-place insert, or a switch arm here, because this runs once per receipt on the
    // lane's hottest path. Markers 0 and 4 are the running extrema and never drift, so only 1..3 carry a desired
    // position. Clamping the extrema BEFORE cell selection collapses the textbook's three-branch search to one
    // ordered comparison chain: the clamp already placed an out-of-range sample inside the outermost cell.
    static (StreamMonitor Next, MonitorVerdict Verdict) Sketched(StreamMonitor.Quantile held, double sample, Instant at) {
        long count = held.Count + 1L;
        MarkerRow q = held.Heights;
        MarkerRow n = held.Positions;
        if (held.Count < 5L) {
            int filled = (int)held.Count;
            int slot = filled;
            while (slot > 0 && q[slot - 1] > sample) { q[slot] = q[slot - 1]; slot--; }
            q[slot] = sample;
            for (int marker = 0; marker <= filled; marker++) { n[marker] = marker + 1d; }
            return (
                held with { Heights = q, Positions = n, Count = count },
                new MonitorVerdict(held.MonitorId, "p2-quantile", q[(int)Math.Round(held.Probability * filled)], held.Limit, Breach: false, (int)count, at));
        }
        if (sample < q[0]) { q[0] = sample; }
        if (sample > q[4]) { q[4] = sample; }
        int cell = sample < q[1] ? 0 : sample < q[2] ? 1 : sample < q[3] ? 2 : 3;
        for (int marker = cell + 1; marker < 5; marker++) { n[marker] += 1d; }
        for (int marker = 1; marker <= 3; marker++) {
            double drift = Desired(marker, held.Probability, count) - n[marker];
            if ((drift >= 1d && n[marker + 1] - n[marker] > 1d) || (drift <= -1d && n[marker - 1] - n[marker] < -1d)) {
                double direction = Math.Sign(drift);
                double parabolic = q[marker] + direction / (n[marker + 1] - n[marker - 1]) * (
                    (n[marker] - n[marker - 1] + direction) * (q[marker + 1] - q[marker]) / (n[marker + 1] - n[marker])
                    + (n[marker + 1] - n[marker] - direction) * (q[marker] - q[marker - 1]) / (n[marker] - n[marker - 1]));
                q[marker] = q[marker - 1] < parabolic && parabolic < q[marker + 1]
                    ? parabolic
                    : q[marker] + direction * (q[marker + (int)direction] - q[marker]) / (n[marker + (int)direction] - n[marker]);
                n[marker] += direction;
            }
        }
        return (
            held with { Heights = q, Positions = n, Count = count },
            new MonitorVerdict(held.MonitorId, "p2-quantile", q[2], held.Limit, q[2] > held.Limit, (int)Math.Min(count, int.MaxValue), at));
    }

    static double Desired(int marker, double probability, long count) => marker switch {
        1 => 1d + probability * (count - 1L) / 2d,
        2 => 1d + probability * (count - 1L),
        _ => 1d + (1d + probability) * (count - 1L) / 2d,
    };
}
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
