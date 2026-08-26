# [COMPUTE_MONITOR]

Rasm.Compute stats monitor scores operational streams online: `StreamMonitor` is the closed stateful-capsule family — EWMA control limits, a kernel `QuantileSketch` marker sketch, and a bounded `Stats/estimator#ESTIMATOR_LANE` `ResidualWindow` carrying one fitted detector — advanced per sample by `MonitorLane`. Batch changepoint detectors stay `Stats/estimator#ESTIMATOR_LANE` rows (`TemporalSpec` Cusum · BayesianOnline · CorrelatedResidual over `EstimatorModel.Detector`); the detector capsule calls its admitted `FittedModel.Predict` entry and never re-derives a recursion.

`MonitorLane.Observe` advances a caller-supplied scalar stream through one stateful monitor and returns the settled capsule with its typed verdicts. `MonitorLane.AsDetector` projects a seeded detector capsule onto the `Solver/clash#CLASH_AND_TWIN` injected-detector slot.

## [01]-[INDEX]

- [02]-[MONITOR_LANE]: stateful monitor capsules — EWMA control limits, kernel quantile sketch, composed estimator detector; the admitted policy scalars; typed verdicts and the twin detector projection.

## [02]-[MONITOR_LANE]

- Owner: `MonitorKey`, `Smoothing`, `FalseAlarm`, `Warmup`, and `Threshold` are the admitted policy scalars whose bands are unrepresentable-invalid rather than gate conditions; `StreamMonitor` `[Union]` is the stateful capsule family whose cases carry their own advance state; `MonitorStatistic` `[SmartEnum<string>]` closes the verdict-label vocabulary; `MonitorVerdict` is the per-sample verdict carrier; `MonitorObservation` carries the settled capsule and verdicts; `MonitorLane` owns the advance fold, stream observation, and twin-detector projection.
- Cases: `StreamMonitor` ewma (λ-smoothed level against time-varying control limits over an admitted kernel `Stat<Scalar>` baseline) · quantile (kernel `QuantileSketch` tracking one probability against a policy bound) · detector (bounded `ResidualWindow` carrying one fitted `Stats/estimator` detector); `MonitorStatistic` ewma-level · p2-quantile · detector-score · sample-rejected.
- Entry: `StreamMonitor.OfEwma(string monitorId, double lambda, double falseAlarm, int warmup)`, `OfQuantile(string monitorId, double probability, double limit)`, and `OfDetector(string monitorId, int capacity, FittedModel detector)` accumulate independent admission faults; `MonitorLane.Advance(StreamMonitor monitor, double sample, IClock clock)` advances one sample; `MonitorLane.Observe(StreamMonitor monitor, Seq<double> samples, IClock clock)` returns the settled capsule and verdicts; `MonitorLane.AsDetector(StreamMonitor.Detector seed, IClock clock)` projects the stateful detector consumed by the twin loop.
- Auto: sample admission is ONE kernel screen at the lane boundary — `Scalar.From` composes `ValidityClaim.Finite`, which rejects the host `RhinoMath.UnsetValue` sentinel a bare `double.IsFinite` admits as an ordinary value — so every arm inherits it and a refused sample lands a `sample-rejected` verdict rather than failing the batch, exactly the `Rejected`-column posture the kernel moment fold declares. The EWMA arm advances the kernel `Stat<Scalar>` moment summary one admitted sample at a time through its warmup, then smooths `level = λ·x + (1−λ)·level` and derives the time-varying limit `L·σ·√(λ/(2−λ)·(1−(1−λ)^{2t}))` so early samples meet tighter bands and the asymptote is the textbook control limit; the control multiplier `L = Φ⁻¹(1 − α/2)` derives inside `FalseAlarm`'s own admission through `Normal.InvCDF`, so a sub-representable rate whose quantile argument rounds to 1 and lands `+∞` is a rate no caller can construct; the quantile arm advances the kernel `QuantileSketch` and reads `Estimate()`; the detector arm pushes into its `ResidualWindow` and delegates scoring to the injected detector, reading the LAST row's score and change flag exactly as the twin does.
- Result: `MonitorVerdict` carries monitor id, statistic key, optional policy limit, level, breach flag, window count, and semantic instant; `MonitorObservation` returns the settled capsule beside its verdict sequence.
- Packages: MathNet.Numerics (`Normal.InvCDF` static quantile), Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm (project — `Scalar`/`Stat<Scalar>` the one admitted moment summary and its sentinel screen, `QuantileSketch` the one P² marker walk, `Evidence<T>` the three-state probe result), BCL inbox
- Growth: a new online statistic is one `StreamMonitor` case and one `MonitorStatistic` row; callers select the scalar stream before entry, so monitor growth never couples to an unrelated result family.
- Boundary: capsules are immutable — `Advance` returns the next capsule and the caller owns placement (an `Atom<StreamMonitor>` at a session boundary, a threaded fold in a batch view) per the cell-and-thread law. The quantile arm carries the kernel `QuantileSketch` whole: `Rasm/Domain/stats#ORDER_STATISTICS` already holds the `[InlineArray(5)]` marker pair, the insertion-sorted seeding, the desired-position table, and the parabolic-with-linear-fallback adjustment, and its Boundary NAMES this lane as the composer, so a second Jain–Chlamtac body here is the re-implementation that owner exists to forbid. NAMED LOSS on the collapse: this page's own `Probability`/`Count` columns and its verdict-side rounding retire into `QuantileSketch.Fraction`/`Count`/`Estimate()`, which reads the exact order statistic below five samples where the deleted body indexed a rounded fraction into a partially-seeded row.
- Boundary: changepoint state, thresholds, and anomaly classification for the detector arm live on its fitted `Stats/estimator` model, while the monitor holds only the bounded `ResidualWindow`. That window is the estimator's own carrier, not a local ring: `Solver/clash#CLASH_AND_TWIN` `TwinWindow` folds the identical `(Count >= Capacity ? Tail : Held).Add(v)` law under the identical capacity floor, and one owner beside `EstimatorModel.Detector` is what stops `AsDetector` injected into `DigitalTwin.Score` from windowing the same evidence twice.
- Boundary: `AsDetector` emits one score and one change flag per evidence row over a shared `Atom` cell that installs the advanced capsule BESIDE the verdicts that transition emitted — the CAS function stays pure and re-runs whole on a losing exchange, where a lock around a mutable local publishes a torn advance the second caller reads as its own. The kernel `Rasm/Domain/results#TRANSITION` `Cell` shapes do NOT serve here and the discriminant is the answer's shape: every `Cell` member returns a `Transition<TState>` over STATE alone, while this transition's answer is the verdict SEQUENCE the accepted step produced, which therefore rides IN the installed value — `DECISION_UNDERIVABLE_FROM_STATE` satisfied by construction. `Walk` failure installs the unchanged capsule carrying the fault, so a refused sample never advances state.
- Boundary: warmup samples score `Breach: false` because no estimated baseline bounds them; solver and factorization residuals remain separate caller-selected streams.
- Boundary: this lane's sketch serves live scalar streams alone. Exact small-sample batch quantiles are the kernel `Rasm/Domain/stats#ORDER_STATISTICS` `Distribution.Of` fold over a bounded materialized sample, and the branch three-form quantile law binds both — an operational sketch crossing into a bounded-sample reading grades a value no run produced. The marker walk remains one body at one owner.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------

[ValueObject<string>]
public readonly partial struct MonitorKey {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        value = value?.Trim() ?? string.Empty;
        validationError = value.Length > 0 ? null : new ValidationError("<monitor-id-blank>");
    }

    public static Fin<MonitorKey> From(string repr) => Validate(repr, null, out MonitorKey value) is { } fault ? Fin.Fail<MonitorKey>(fault) : value;
}

[ValueObject<double>]
public readonly partial struct Smoothing {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) =>
        validationError = double.IsFinite(value) && value is > 0.0 and <= 1.0
            ? null
            : new ValidationError($"<monitor-lambda:{value:R}>");

    public static Fin<Smoothing> From(double repr) => Validate(repr, null, out Smoothing value) is { } fault ? Fin.Fail<Smoothing>(fault) : value;
}

[ValueObject<double>]
public readonly partial struct FalseAlarm {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) =>
        validationError = double.IsFinite(value) && value is > 0.0 and < 1.0 && double.IsFinite(Multiplier(value))
            ? null
            : new ValidationError($"<monitor-false-alarm:{value:R}>");

    public static Fin<FalseAlarm> From(double repr) => Validate(repr, null, out FalseAlarm value) is { } fault ? Fin.Fail<FalseAlarm>(fault) : value;

    public double ControlLimit => Multiplier(Value);

    private static double Multiplier(double rate) => Normal.InvCDF(0d, 1d, 1d - rate / 2d);
}

[ValueObject<int>]
public readonly partial struct Warmup {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) =>
        validationError = value >= 2 ? null : new ValidationError($"<monitor-warmup:{value}>");

    public static Fin<Warmup> From(int repr) => Validate(repr, null, out Warmup value) is { } fault ? Fin.Fail<Warmup>(fault) : value;
}

[ValueObject<double>]
public readonly partial struct Threshold {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) =>
        validationError = double.IsFinite(value) ? null : new ValidationError($"<monitor-threshold:{value:R}>");

    public static Fin<Threshold> From(double repr) => Validate(repr, null, out Threshold value) is { } fault ? Fin.Fail<Threshold>(fault) : value;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MonitorStatistic {
    public static readonly MonitorStatistic EwmaLevel = new("ewma-level");
    public static readonly MonitorStatistic P2Quantile = new("p2-quantile");
    public static readonly MonitorStatistic DetectorScore = new("detector-score");
    public static readonly MonitorStatistic SampleRejected = new("sample-rejected");
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record StreamMonitor {
    private StreamMonitor() { }

    public sealed record Ewma(MonitorKey Id, Smoothing Lambda, double ControlL, Warmup Warmup, double Level, Evidence<Stat<Scalar>> Baseline, long Count) : StreamMonitor;

    public sealed record Quantile(MonitorKey Id, Threshold Limit, QuantileSketch Sketch) : StreamMonitor;

    public sealed record Detector(MonitorKey Id, ResidualWindow Window, FittedModel Model) : StreamMonitor;

    public MonitorKey Key => Switch(
        ewma: static monitor => monitor.Id,
        quantile: static monitor => monitor.Id,
        detector: static monitor => monitor.Id);

    public static Fin<StreamMonitor> OfEwma(string monitorId, double lambda, double falseAlarm, int warmup) =>
        (MonitorKey.From(monitorId).ToValidation(),
         Smoothing.From(lambda).ToValidation(),
         FalseAlarm.From(falseAlarm).ToValidation(),
         Warmup.From(warmup).ToValidation())
            .Apply(static (id, smoothing, alarm, warm) => (StreamMonitor)new Ewma(
                id, smoothing, alarm.ControlLimit, warm, Level: 0d, Baseline: new Evidence<Stat<Scalar>>.Absent(), Count: 0L))
            .ToFin();

    public static Fin<StreamMonitor> OfQuantile(string monitorId, double probability, double limit) =>
        (MonitorKey.From(monitorId).ToValidation(),
         Threshold.From(limit).ToValidation(),
         QuantileSketch.Of(probability, Op.Of(name: nameof(OfQuantile))).ToValidation())
            .Apply(static (id, bound, sketch) => (StreamMonitor)new Quantile(id, bound, sketch))
            .ToFin();

    public static Fin<StreamMonitor> OfDetector(string monitorId, int capacity, FittedModel detector) =>
        (MonitorKey.From(monitorId).ToValidation(),
         WindowCapacity.From(capacity).ToValidation(),
         (detector.Carrier is EstimatorModel.Detector
             ? Fin.Succ(detector)
             : Fin.Fail<FittedModel>(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.Type(detector.Carrier.GetType()))))).ToValidation())
            .Apply(static (id, capacity, model) => (StreamMonitor)new Detector(id, ResidualWindow.Of(capacity), model))
            .ToFin();
}

// --- [MODELS] --------------------------------------------------------------------------

public sealed record MonitorVerdict(MonitorKey Monitor, MonitorStatistic Statistic, double Level, Option<double> Limit, bool Breach, int Window, Instant At);

public sealed record MonitorObservation(StreamMonitor Settled, Seq<MonitorVerdict> Verdicts);

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class MonitorLane {
    public static Fin<(StreamMonitor Next, MonitorVerdict Verdict)> Advance(StreamMonitor monitor, double sample, IClock clock) =>
        Scalar.From(sample).Match(
            Succ: admitted => monitor.Switch(
                state: (Sample: admitted, At: clock.GetCurrentInstant(), Key: Op.Of(name: nameof(Advance))),
                ewma: static (s, held) => Smoothed(held, s.Sample, s.At, s.Key),
                quantile: static (s, held) => Sketched(held, s.Sample, s.At, s.Key),
                detector: static (s, held) => Detected(held, s.Sample, s.At)),
            Fail: _ => Fin.Succ((monitor, new MonitorVerdict(
                monitor.Key, MonitorStatistic.SampleRejected, sample, None, Breach: false, Windowed(0L), clock.GetCurrentInstant()))));

    public static Fin<MonitorObservation> Observe(StreamMonitor monitor, Seq<double> samples, IClock clock) =>
        Walk(monitor, samples, clock).Map(static walked => new MonitorObservation(walked.Settled, walked.Verdicts));

    public static Func<Matrix<double>, Fin<Prediction>> AsDetector(StreamMonitor.Detector seed, IClock clock) {
        Atom<(StreamMonitor.Detector Held, Fin<Seq<MonitorVerdict>> Emitted)> cell =
            Atom((Held: seed, Emitted: Fin.Succ(Seq<MonitorVerdict>())));

        return evidence => evidence.ColumnCount != 1
            ? Fin.Fail<Prediction>(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Shape(ShapeRequirement.Arity, new ShapeEvidence.Count(evidence.ColumnCount, 1L))))
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

    static int Windowed(long count) => (int)Math.Min(count, int.MaxValue);

    static Fin<(StreamMonitor Next, MonitorVerdict Verdict)> Detected(StreamMonitor.Detector held, Scalar sample, Instant at) {
        StreamMonitor.Detector next = held with { Window = held.Window.Push(sample.To()) };
        return next.Model.Predict(next.Window.Evidence).Bind(outcome =>
            outcome is Prediction.Anomaly anomaly && anomaly.Scores.Count == next.Window.Count && anomaly.Changes.Length == next.Window.Count
                ? Fin.Succ(((StreamMonitor)next, new MonitorVerdict(
                    next.Id, MonitorStatistic.DetectorScore, anomaly.Scores[^1], None, anomaly.Changes[^1], next.Window.Count, at)))
                : Fin.Fail<(StreamMonitor, MonitorVerdict)>(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.Key(next.Id.Value)))));
    }

    static Fin<(StreamMonitor Next, MonitorVerdict Verdict)> Smoothed(StreamMonitor.Ewma held, Scalar sample, Instant at, Op key) {
        bool warm = held.Count >= held.Warmup.Value;
        Evidence<Stat<Scalar>> advanced = warm
            ? held.Baseline
            : held.Baseline.Switch(
                measured: prior => Evidence.Of(Stat<Scalar>.Update(prior.Value, sample, key: key)),
                refused: static failed => (Evidence<Stat<Scalar>>)failed,
                absent: _ => Evidence.Of(Stat<Scalar>.Of(Seq(sample), key)));

        return advanced.Switch(
            refused: static failed => Fin.Fail<(StreamMonitor, MonitorVerdict)>(failed.Cause),
            absent: _ => Fin.Fail<(StreamMonitor, MonitorVerdict)>(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Required(ComputeSubject.Input))),
            measured: baseline => {
                long count = held.Count + 1L;
                double mean = baseline.Value.Mean;
                double level = warm ? held.Lambda.Value * sample.To() + (1d - held.Lambda.Value) * held.Level : mean;
                Option<double> band = warm
                    ? Some(held.ControlL * baseline.Value.Deviation(MomentNormalizer.Sample) * Math.Sqrt(
                        held.Lambda.Value / (2d - held.Lambda.Value) * (1d - Math.Pow(1d - held.Lambda.Value, 2d * (count - held.Warmup.Value)))))
                    : None;
                return Fin.Succ((
                    (StreamMonitor)(held with { Level = level, Baseline = advanced, Count = count }),
                    new MonitorVerdict(
                        held.Id, MonitorStatistic.EwmaLevel, level,
                        band.Map(width => mean + Math.Sign(level - mean) * width),
                        band.Map(width => Math.Abs(level - mean) > width).IfNone(false),
                        Windowed(count), at)));
            });
    }

    static Fin<(StreamMonitor Next, MonitorVerdict Verdict)> Sketched(StreamMonitor.Quantile held, Scalar sample, Instant at, Op key) =>
        QuantileSketch.Update(held.Sketch, sample.To(), key).Bind(advanced => advanced.Estimate()
            .ToFin(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.Key(held.Id.Value))))
            .Map(estimate => (
                (StreamMonitor)(held with { Sketch = advanced }),
                new MonitorVerdict(
                    held.Id, MonitorStatistic.P2Quantile, estimate, Some(held.Limit.Value),
                    estimate > held.Limit.Value, Windowed(advanced.Count), at))));
}
```
