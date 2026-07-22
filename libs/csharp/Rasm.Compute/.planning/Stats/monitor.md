# [COMPUTE_MONITOR]

Rasm.Compute stats monitor scores operational streams online: `StreamMonitor` is the closed stateful-capsule family — EWMA control limits, a P²-class quantile sketch, and a bounded window carrying one fitted `Stats/estimator` detector — advanced one sample at a time by `MonitorLane`, every verdict a typed fact the receipt rail folds. Batch changepoint detectors stay `Stats/estimator#ESTIMATOR_LANE` rows (`TemporalSpec` Cusum · BayesianOnline · CorrelatedResidual over `EstimatorModel.Detector`); the detector capsule calls its admitted `FittedModel.Predict` entry and never re-derives a recursion.

`MonitorChannel` rows extract scalar streams from the identical `Seq<ComputeReceipt>` fact stream the `Runtime/receipts#FOLD_PROJECTIONS` views fold — solve-residual drift, remote-latency drift, backpressure cadence — so operational drift detection consumes the standing telemetry with zero new emit path, and a breach lands the `Runtime/receipts#RECEIPT_UNION` `Drift` case the `ComputeInstrumentFan` projects onto `rasm.compute.monitor.breaches`. `MonitorLane.AsDetector` projects a seeded capsule onto the `Solver/clash#CLASH_AND_TWIN` injected-detector slot, so the twin loop gains control-chart discipline through the seam it already holds. `ComputeReceipt`, `CorrelationId`, `AllocationClass`, NodaTime `IClock` (the App-owned `ClockPolicy` stays at composition), MathNet `Normal.InvCDF`, and the `ComparerAccessors.StringOrdinal` accessor arrive settled. Page is HOST-LOCAL.

## [01]-[INDEX]

- [01]-[MONITOR_LANE]: stateful monitor capsules — EWMA control limits, P² quantile sketch, composed estimator detector; receipt-channel extraction rows; the drift verdict and the twin detector projection.

## [02]-[MONITOR_LANE]

- Owner: `StreamMonitor` `[Union]` the stateful capsule family whose cases carry their own advance state; `MonitorVerdict` the per-sample verdict carrier minting the `Drift` receipt case; `MonitorChannel` `[SmartEnum<string>]` the receipt-to-scalar extraction rows; `MonitorLane` the advance fold, stream observation, and the twin-detector projection.
- Cases: `StreamMonitor` ewma (λ-smoothed level against time-varying control limits over a Welford baseline) · quantile (five-marker P² sketch tracking one probability against a policy bound) · detector (bounded residual window carrying one fitted `Stats/estimator` detector); `MonitorChannel` solve-residual · remote-seconds · backpressure-depth.
- Entry: `StreamMonitor.OfEwma(string monitorId, double lambda, double falseAlarm, int warmup)`, `OfQuantile(string monitorId, double probability, double limit)`, and `OfDetector(string monitorId, int capacity, FittedModel detector)` — validated mints, `Fin<T>` aborting an out-of-range policy or a non-detector carrier; `MonitorLane.Advance(StreamMonitor monitor, double sample, IClock clock)` — one sample in, the advanced capsule and its `MonitorVerdict` out through the generated total `Switch`; `MonitorLane.Observe(StreamMonitor monitor, MonitorChannel channel, Seq<ComputeReceipt> facts, IClock clock)` — extracts the channel and folds `Advance` across it; `MonitorLane.AsDetector(StreamMonitor.Detector seed, IClock clock)` — the stateful `Func<Matrix<double>, Fin<Prediction>>` projection the clash twin injects, admitting the detector capsule and exactly one evidence column before indexing.
- Auto: the EWMA arm runs Welford mean/variance through its warmup, then smooths `level = λ·x + (1−λ)·level` and derives the time-varying limit `L·σ·√(λ/(2−λ)·(1−(1−λ)^{2t}))` so early samples meet tighter bands and the asymptote is the textbook control limit; `OfEwma` admits a two-sided false-alarm rate and derives `L = Φ⁻¹(1 − α/2)` through `Normal.InvCDF` at the gate, rejecting a non-finite derived limit (a sub-representable rate rounds the quantile argument to 1 and lands `+∞`), so the knob is a probability admitted exactly once on the `Fin` rail, never a bare multiplier; the quantile arm runs the five-marker P² update — sorted seeding over the first five samples, then marker increment, desired-position drift, and parabolic (linear-fallback) height adjustment — tracking the running quantile with O(1) state; the detector arm pushes into its bounded window and delegates scoring to the injected detector, reading the LAST row's score and change flag exactly as the twin does.
- Receipt: `Drift` — monitor id, statistic, optional policy limit, level, breach flag, and window count, minted by `MonitorVerdict.Receipt` under the caller's correlation; detector verdicts carry `null` because their fitted model owns classification without exposing a scalar boundary. `[02]-[RECEIPT_UNION]` counts breaches onto `rasm.compute.monitor.breaches`, and `ReceiptFolds.Breaches` is the operational view, so monitor evidence rides the standing stream.
- Packages: MathNet.Numerics (`Normal.InvCDF` static quantile), Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox
- Growth: a new online statistic is one `StreamMonitor` case whose arm the `Advance` `Switch` demands at compile time; a new operational stream is one `MonitorChannel` row with its extractor column; a new false-alarm posture is one `OfEwma` rate value at composition; zero new surface — an `EwmaMonitor`/`QuantileTracker`/`DriftDetector` sibling family, a second receipt-scanning loop beside `MonitorChannel`, or a monitor-local CUSUM recursion re-deriving the estimator rows is the rejected form.
- Boundary: capsules are immutable — `Advance` returns the next capsule, the caller owns placement (an `Atom<StreamMonitor>` at a session boundary, a threaded fold in a batch view) per the cell-and-thread law; changepoint state, thresholds, and anomaly classification for the detector arm live on its fitted `Stats/estimator` model, while the monitor holds only the bounded evidence window. `AsDetector` emits one score and one change flag per evidence row, and the closure-held capsule advances across calls. Extraction reads the same `Seq<ComputeReceipt>` the dashboards fold — a monitor that taps `ReceiptSurface.Emit` directly or mints a second fact stream is the deleted form; warmup samples score `Breach: false` because a limit over an unestimated baseline is noise, and the verdict still lands so cadence stays observable.

```csharp signature
// --- [TYPES] -----------------------------------------------------------------------------------------------

// Stateful capsule family: each case carries exactly its own advance state — EWMA level over a Welford
// baseline, P² five-marker sketch, bounded detector window — so a knob record shared across modalities is
// unrepresentable and a new online statistic is one case plus one Advance arm.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record StreamMonitor {
    private StreamMonitor() { }

    public sealed record Ewma(string MonitorId, double Lambda, double ControlL, int Warmup, double Level, double Mean, double M2, long Count) : StreamMonitor;

    public sealed record Quantile(string MonitorId, double Probability, double Limit, ImmutableArray<double> Heights, ImmutableArray<double> Positions, long Count) : StreamMonitor;

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
                ? Fin.Succ<StreamMonitor>(new Ewma(monitorId, lambda, controlL, warmup, Level: 0d, Mean: 0d, M2: 0d, Count: 0L))
                : Fin.Fail<StreamMonitor>(ComputeFault.Create($"<monitor-ewma-limit-nonfinite:{monitorId}:{falseAlarm:R}>"));

    public static Fin<StreamMonitor> OfQuantile(string monitorId, double probability, double limit) =>
        string.IsNullOrWhiteSpace(monitorId) || !double.IsFinite(probability) || probability is <= 0d or >= 1d || !double.IsFinite(limit)
            ? Fin.Fail<StreamMonitor>(ComputeFault.Create($"<monitor-quantile-policy:{monitorId}>"))
            : Fin.Succ<StreamMonitor>(new Quantile(monitorId, probability, limit, [], [], Count: 0L));

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
                state: (Sample: sample, At: clock.GetCurrentInstant()),
                ewma: static (s, held) => Fin.Succ(Smoothed(held, s.Sample, s.At)),
                quantile: static (s, held) => Fin.Succ(Sketched(held, s.Sample, s.At)),
                detector: static (s, held) => Detected(held, s.Sample, s.At));

    public static Fin<(StreamMonitor Settled, Seq<MonitorVerdict> Verdicts)> Observe(StreamMonitor monitor, MonitorChannel channel, Seq<ComputeReceipt> facts, IClock clock) =>
        Walk(monitor, channel.Extract(facts), clock);

    // Twin-detector projection: the closure-held capsule advances across calls, and each evidence row lands one
    // score and one change flag so the Anomaly carrier satisfies the clash cardinality proof.
    public static Func<Matrix<double>, Fin<Prediction>> AsDetector(StreamMonitor.Detector seed, IClock clock) {
        var gate = new object();
        StreamMonitor.Detector held = seed;

        Fin<Prediction> Score(Matrix<double> evidence) {
            lock (gate) {
                return Walk(held, toSeq(Enumerable.Range(0, evidence.RowCount)).Map(row => evidence[row, 0]), clock)
                    .Map(walked => {
                        held = (StreamMonitor.Detector)walked.Settled;
                        return (Prediction)new Prediction.Anomaly(
                            Vector<double>.Build.DenseOfArray([.. walked.Verdicts.Map(static verdict => verdict.Level)]),
                            [.. walked.Verdicts.Map(static verdict => verdict.Breach)]);
                    });
            }
        }

        return evidence => evidence.ColumnCount == 1
            ? Score(evidence)
            : Fin.Fail<Prediction>(ComputeFault.Create($"<monitor-detector-columns:{evidence.ColumnCount}>"));
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

    static (StreamMonitor Next, MonitorVerdict Verdict) Smoothed(StreamMonitor.Ewma held, double sample, Instant at) {
        long count = held.Count + 1L;
        bool warm = count > held.Warmup;
        double delta = warm ? 0d : sample - held.Mean;
        double mean = warm ? held.Mean : held.Mean + delta / count;
        double m2 = warm ? held.M2 : held.M2 + delta * (sample - mean);
        double level = warm ? held.Lambda * sample + (1d - held.Lambda) * held.Level : mean;
        long baselineCount = Math.Min(count, held.Warmup);
        double sigma = baselineCount > 1 ? Math.Sqrt(m2 / (baselineCount - 1)) : 0d;
        double band = held.ControlL * sigma * Math.Sqrt(
            held.Lambda / (2d - held.Lambda) * (1d - Math.Pow(1d - held.Lambda, 2d * Math.Max(1L, count - held.Warmup))));
        bool breach = warm && sigma > 0d && Math.Abs(level - mean) > band;
        return (
            held with { Level = level, Mean = mean, M2 = m2, Count = count },
            new MonitorVerdict(held.MonitorId, "ewma-level", level, mean + Math.Sign(level - mean) * band, breach, (int)Math.Min(count, int.MaxValue), at));
    }

    // Jain–Chlamtac P²: sorted five-sample seeding, then marker increment, desired-position drift, and
    // parabolic height adjustment with the linear fallback — O(1) state per tracked probability.
    static (StreamMonitor Next, MonitorVerdict Verdict) Sketched(StreamMonitor.Quantile held, double sample, Instant at) {
        long count = held.Count + 1L;
        if (held.Heights.Length < 5) {
            double[] seeded = [.. held.Heights.Add(sample).OrderBy(static value => value)];
            double estimate = seeded[(int)Math.Round(held.Probability * (seeded.Length - 1))];
            return (
                held with { Heights = [.. seeded], Positions = [.. Enumerable.Range(1, seeded.Length).Select(static position => (double)position)], Count = count },
                new MonitorVerdict(held.MonitorId, "p2-quantile", estimate, held.Limit, Breach: false, (int)Math.Min(count, int.MaxValue), at));
        }
        double[] q = [.. held.Heights];
        double[] n = [.. held.Positions];
        int cell = sample < q[0] ? 0 : sample >= q[4] ? 3 : Enumerable.Range(0, 4).First(index => sample >= q[index] && sample < q[index + 1]);
        if (sample < q[0]) { q[0] = sample; }
        if (sample > q[4]) { q[4] = sample; }
        for (int marker = cell + 1; marker < 5; marker++) { n[marker] += 1d; }
        double[] desired = [
            1d,
            1d + held.Probability * (count - 1) / 2d,
            1d + held.Probability * (count - 1),
            1d + (1d + held.Probability) * (count - 1) / 2d,
            count,
        ];
        for (int marker = 1; marker <= 3; marker++) {
            double drift = desired[marker] - n[marker];
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
        bool breach = q[2] > held.Limit;
        return (
            held with { Heights = [.. q], Positions = [.. n], Count = count },
            new MonitorVerdict(held.MonitorId, "p2-quantile", q[2], held.Limit, breach, (int)Math.Min(count, int.MaxValue), at));
    }
}
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
