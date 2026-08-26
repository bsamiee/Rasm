# [APPHOST_BENCHMARK_HARNESS]

One `Benchmark` value carries every judged run: the corpus benchmark gate the kernel and compute pages cite anchors here as one typed pass-or-regress fold that writes its duration and regression rows at the gate, on-demand deep capture rides the support-bundle contributor port, and every gated run executes inside a minted span so the profiler links its flame graph to the exact case that regressed. Every speed claim in the .NET corpus resolves to a `Benchmark` this gate judged.

Settled composition: `AppHostMeasure.BenchmarkDuration`/`BenchmarkRegressions` and the composition `InstrumentSet` from Observability/instruments#INSTRUMENT_CATALOG; `HostFingerprint` and its `Current(stamps)` ambient mint from Runtime/determinism#DETERMINISM_KERNEL; `SupportArtifact.EventTrace` and `SupportContributorPort` from Observability/bundles#CAPTURE_PIPELINE; `TelemetryIdentity.Mint` and the `PyroscopeSpanProcessor` profile linkage from Observability/telemetry; `AppHostPoint`/`AppHostFact` and the composed `HookSet` from Observability/hooks#HOOK_ROSTER; `FaultBand.Benchmark` and `ContentHash.Of` from the kernel.

## [01]-[INDEX]

- [02]-[BENCHMARK]: `Benchmark` carries the judged run, gated columns, breach evidence, and corpus-gate fold.
- [03]-[CAPTURE_BOUNDARY]: Deep-capture contributor rows and the native-symbol lease riding the support-bundle fan.
- [04]-[PROFILE_CORRELATION]: One gated run driving span, labels, symbol lease, sample capture, and judgment.
- [05]-[CLAIM_FIELD_MAP]: One family-to-`Benchmark` field map admitting every folder claim family, and the corpus gate over the Compute-minted claim wire.

## [02]-[BENCHMARK]

- Owner: `Benchmark` — the judged run; `BenchMeasurement` the harness-edge carrier over one `Distribution<Elapsed>`; `ReferenceEvidence` the same-run relative baseline; `BenchmarkVerdict` `[SmartEnum<string>]` the gate-disposition vocabulary; `BudgetColumn` `[SmartEnum<string>]` the gated-column roster carrying each column's fresh read, baseline read, and policy budget as delegate columns; `BudgetBreach` the per-column overrun evidence; `BenchmarkFault` `[Union]` deriving through `FaultBand.Benchmark`; `GatePolicy` the admitted threshold row; `BenchmarkGate` — the corpus pass-or-regress fold.
- Cases: `BenchmarkVerdict` = Unjudged | Pass | Regressed | HostMismatch; `BudgetColumn` = Median | P95 | Allocation, each a ceiling against the held claim's own column; `BenchmarkFault` = GateRegressed | HostMismatch | ReferenceAbsent | PolicyRejected | MeasurementRejected.
- Entry: `BenchMeasurement.Of(spans, allocatedBytes, operations, key)` admits a bounded materialized span sample into exact order statistics; `Benchmark.Of(suite, case, corpus, measured, stamps, reference, artifact)` is the one fresh mint every folder claim family reaches, stamping `HostFingerprint.Current(stamps)` and holding the verdict at its floor; `GatePolicy.Of(...)` admits finite positive budgets and a finite nonnegative optional speedup floor; `BenchmarkGate.Gate(signals, fresh, claim, policy, key)` judges the fresh run against the held claim, stamps the verdict row, writes `AppHostMeasure.BenchmarkDuration` and — past budget — `BenchmarkRegressions` through the mounted set, and returns the accumulating gate result; `BenchmarkGate.Judge(...)` is the pure verdict fold the entry composes.
- Auto: the duration figures are ONE `Distribution<Elapsed>` over an ascending materialized sample — median, interquartile spread, median absolute deviation, and the `Quantiles` roster's p95 all read one sort, so a new figure is a percentile row rather than a second pass or a second carrier; the spread is EVIDENCE rather than a gate input, because a widening distribution under a held median is a stability signal a reviewer reads, not a budget a run fails, and folding it into a budget column fails every legitimately noisier lane; `HostFingerprint.Current(stamps)` stamps machine, OS, architecture, processor count, runtime, and the caller's stamp map with one ordered render, so a claim binds only against a matching host and a cross-host comparison faults as `HostMismatch` rather than a phantom regression; a corpus-bound family stamps its input fingerprint on `Corpus`, so a corpus revision re-baselines structurally — a held claim over a different corpus contributes no budget refusal; `ReferenceEvidence` carries a same-run scalar reference when a family claims relative speed, and `GatePolicy.SpeedupFloor` arms that ratio as its own gate leg; the durable claim is the Persistence reuse-index row the bench project persists off the judged value, so benchmark history is a persisted domain record and never a second stream; a regressed run still writes, so the duration and regression rows count every verdict, never the passing subset alone.
- Output: `Benchmark` — suite, case, host fingerprint, corpus identity, the measurement carrier, gate verdict, optional same-run reference evidence, optional artifact key; the run span carries the correlation.
- Packages: Rasm (`Distribution<Elapsed>`, `Elapsed`, `ContentHash`, `Op`, `FaultBand`), Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox.
- Growth: a new measured axis is one `BenchMeasurement` column threaded through `Of`; a new gated column is one `BudgetColumn` row with its three delegate columns and its `GatePolicy` budget, breaking neither the fold nor a consumer; a new verdict class is one `BenchmarkVerdict` row paired with its `BenchmarkFault` case; a new claim family is one `[05]-[CLAIM_FIELD_MAP]` row with its own `Of` call, never a positional construction.
- Boundary: the duration figures are EXACT order statistics over a bounded materialized sample, the form the kernel quantile roster at `Rasm/Domain/stats#ORDER_STATISTICS` admits this carrier under — a sketch estimator crossing into a gate comparison grades a value no run produced, which is why `QuantileSketch` reaches no `Benchmark` here; host identity is the spine's own `HostFingerprint` and never a second environment record, so the claim column the Persistence reuse index holds, the Compute claim wire, and this gate all read ONE render and a second host-identity string is the twin this page deleted; this page is the corpus benchmark gate's owner — a hand-rolled kernel is admitted only after its `Benchmark` defeats the library route under `Judge`; the durable claim the gate compares against is the Persistence reuse-index row resolved by content fingerprint — measured facts mint here, the claim store persists them, and neither re-derives the other.
- Boundary: the three gate legs ACCUMULATE — host identity, the relative claim, and the budget roster are independent evidence, so a host mismatch never masks a co-occurring budget breach and one run reports every reason it failed; each budget refusal names its column, its two values, and its budget as a `BudgetBreach` row, so `GateRegressed` carries structured evidence a reader filters rather than one rendered line a reader parses; an armed speedup floor with no admissible reference REFUSES as `ReferenceAbsent` because a relative claim without its baseline is a missing measurement, never an implicit pass; the verdict derives ONCE, on the fail arm, off the accumulated fault set — `Judge` stamps `Pass` itself and `Gate` re-decides nothing.

```csharp
namespace Rasm.AppHost.Observability;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class BenchmarkVerdict {
    public static readonly BenchmarkVerdict Unjudged = new("unjudged");
    public static readonly BenchmarkVerdict Pass = new("pass");
    public static readonly BenchmarkVerdict Regressed = new("regressed");
    public static readonly BenchmarkVerdict HostMismatch = new("host-mismatch");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class BudgetColumn {
    public static readonly BudgetColumn Median = new("median",
        read: static run => Some(run.Measured.Figures.Median.To()),
        budget: static policy => policy.MedianBudget);
    public static readonly BudgetColumn P95 = new("p95",
        read: static run => run.Measured.At(BenchMeasurement.P95).Map(static value => value.To()),
        budget: static policy => policy.P95Budget);
    public static readonly BudgetColumn Allocation = new("allocation",
        read: static run => Some((double)run.Measured.AllocatedBytes),
        budget: static policy => policy.AllocationBudget);

    [UseDelegateFromConstructor] public partial Option<double> Read(Benchmark run);
    [UseDelegateFromConstructor] public partial double Budget(GatePolicy policy);

    public Option<BudgetBreach> Judge(Benchmark fresh, Benchmark held, GatePolicy policy) =>
        from measured in Read(fresh)
        from baseline in Read(held)
        where measured > baseline * Budget(policy)
        select new BudgetBreach(Key, measured, baseline, Budget(policy));
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct BudgetBreach(string Column, double Fresh, double Held, double Budget);

public readonly record struct BenchMeasurement(Distribution<Elapsed> Figures, long AllocatedBytes, long Operations) {
    public const double P95 = 95d;
    public static readonly Seq<double> Quantiles = Seq(P95);

    public static Fin<BenchMeasurement> Of(Seq<Duration> spans, long allocatedBytes, long operations, Op key) =>
        allocatedBytes >= 0L && operations > 0L
            ? spans.Traverse(Elapsed.OfDuration).As()
                .Bind(rows => Distribution<Elapsed>.Of(values: rows, percentiles: Quantiles, key: key))
                .Map(figures => new BenchMeasurement(figures, allocatedBytes, operations))
            : Fin.Fail<BenchMeasurement>(new BenchmarkFault.MeasurementRejected(
                "a benchmark measurement carries nonnegative allocated bytes and a positive operation count"));

    public Option<Elapsed> At(double percentile) =>
        Figures.Percentiles.Find(row => row.Percentile == percentile).Map(static row => row.Value);
}

public sealed record Benchmark(
    string Suite,
    string Case,
    HostFingerprint Host,
    BenchMeasurement Measured,
    BenchmarkVerdict Verdict,
    Option<UInt128> Corpus = default,
    Option<ReferenceEvidence> Reference = default,
    Option<string> ArtifactKey = default) {

    public static Benchmark Of(
        string suite, string @case, Option<UInt128> corpus, BenchMeasurement measured,
        FrozenDictionary<string, string> stamps,
        Option<ReferenceEvidence> reference = default, Option<string> artifact = default) =>
        new(Suite: suite, Case: @case, Host: HostFingerprint.Current(stamps), Measured: measured,
            Verdict: BenchmarkVerdict.Unjudged, Corpus: corpus, Reference: reference, ArtifactKey: artifact);

    public UInt128 ClaimKey =>
        ContentHash.Of(this, static (row, writer) => writer
            .String(row.Suite).String(row.Case).String(row.Host.ToString())
            .Optional(row.Corpus, static (corpus, held) => held.U128(corpus)));
}

public sealed record ReferenceEvidence(
    string Case,
    HostFingerprint Host,
    Elapsed Median,
    Option<UInt128> Corpus = default);

// --- [ERRORS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record BenchmarkFault : Fault {
    private static readonly FaultBand FamilyBand = FaultBand.Benchmark;
    private BenchmarkFault(string detail) => Detail = detail;
    public string Detail { get; }
    public sealed override string Message => Detail;

    [FaultCase(0)]
    public sealed partial record GateRegressed : BenchmarkFault {
        public GateRegressed(Op key, Seq<BudgetBreach> breaches) : base(key.ToString()) => Breaches = breaches;
        public Seq<BudgetBreach> Breaches { get; }
    }
    [FaultCase(1)]
    public sealed partial record HostMismatch : BenchmarkFault { public HostMismatch(string @case) : base(@case) { } }
    [FaultCase(2)]
    public sealed partial record ReferenceAbsent : BenchmarkFault { public ReferenceAbsent(string @case) : base(@case) { } }
    [FaultCase(3)]
    public sealed partial record PolicyRejected : BenchmarkFault { public PolicyRejected(string detail) : base(detail) { } }
    [FaultCase(4)]
    public sealed partial record MeasurementRejected : BenchmarkFault { public MeasurementRejected(string detail) : base(detail) { } }
}

// --- [POLICIES] ------------------------------------------------------------------------
[ComplexValueObject]
[ValidationError]
public readonly partial struct GatePolicy {
    public double MedianBudget { get; }
    public double P95Budget { get; }
    public double AllocationBudget { get; }
    public Option<double> SpeedupFloor { get; }

    public static readonly GatePolicy Canonical = Create(1.10d, 1.10d, 1.05d, None);

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref double medianBudget, ref double p95Budget, ref double allocationBudget, ref Option<double> speedupFloor) =>
        validationError =
            double.IsFinite(medianBudget) && medianBudget > 0d
            && double.IsFinite(p95Budget) && p95Budget > 0d
            && double.IsFinite(allocationBudget) && allocationBudget > 0d
            && speedupFloor.Match(None: static () => true, Some: static floor => double.IsFinite(floor) && floor >= 0d)
                ? null
                : new ValidationError(string.Join(" | ", new object?[] { "gate thresholds must be finite; budgets must be positive and the speedup floor nonnegative" }));

    public static Fin<GatePolicy> Of(double medianBudget, double p95Budget, double allocationBudget, Option<double> speedupFloor) =>
        Op.Of().AcceptValidated<GatePolicy>(
            fault: Validate(medianBudget, p95Budget, allocationBudget, speedupFloor, out GatePolicy value),
            admitted: value);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class BenchmarkGate {
    public static Validation<Error, Benchmark> Judge(
        Benchmark fresh, Option<Benchmark> claim, GatePolicy policy, Op key) =>
        (Hosts(fresh, claim, policy), Relative(fresh, policy, key), Budgets(fresh, claim, policy, key))
            .Apply(static (_, _, _) => unit)
            .Map(_ => fresh with { Verdict = BenchmarkVerdict.Pass })
            .As();

    static Validation<Error, Unit> Hosts(Benchmark fresh, Option<Benchmark> claim, GatePolicy policy) =>
        (policy.SpeedupFloor.IsNone || fresh.Reference.Match(None: static () => true, Some: row => row.Host == fresh.Host))
        && claim.Match(None: static () => true, Some: held => held.Host == fresh.Host)
            ? Validation<Error, Unit>.Success(unit)
            : Validation<Error, Unit>.Fail(new BenchmarkFault.HostMismatch(fresh.Case));

    static Validation<Error, Unit> Relative(Benchmark fresh, GatePolicy policy, Op key) =>
        policy.SpeedupFloor.Match(
            None: static () => Validation<Error, Unit>.Success(unit),
            Some: floor => Admissible(fresh).Match(
                None: () => Validation<Error, Unit>.Fail(new BenchmarkFault.ReferenceAbsent(fresh.Case)),
                Some: baseline => fresh.Measured.Figures.Median.To() is var measured
                    && measured > 0d && baseline.To() / measured >= floor
                        ? Validation<Error, Unit>.Success(unit)
                        : Validation<Error, Unit>.Fail(new BenchmarkFault.GateRegressed(
                            key, Seq(new BudgetBreach("speedup", measured, baseline.To(), floor))))));

    static Option<Elapsed> Admissible(Benchmark fresh) =>
        fresh.Reference
            .Filter(row => row.Case == fresh.Case && row.Corpus == fresh.Corpus)
            .Map(static row => row.Median);

    static Validation<Error, Unit> Budgets(
        Benchmark fresh, Option<Benchmark> claim, GatePolicy policy, Op key) =>
        claim.Filter(held => held.Corpus == fresh.Corpus)
            .Map(held => toSeq(BudgetColumn.Items).Bind(column => column.Judge(fresh, held, policy).ToSeq()).Strict())
            .IfNone(Seq<BudgetBreach>()) is var breaches && breaches.IsEmpty
            ? Validation<Error, Unit>.Success(unit)
            : Validation<Error, Unit>.Fail(new BenchmarkFault.GateRegressed(key, breaches));

    public static IO<Validation<Error, Benchmark>> Gate(
        InstrumentSet signals, Benchmark fresh, Option<Benchmark> claim, GatePolicy policy, Op key) =>
        from judged in IO.pure(Judge(fresh, claim, policy, key))
        let stamped = fresh with { Verdict = Settled(judged) }
        let tags = InstrumentSet.Tags(
            (AppHostSlot.Suite, stamped.Suite),
            (AppHostSlot.Case, stamped.Case))
        from observed in IO.lift(() => (
            signals.Write(AppHostMeasure.BenchmarkDuration.Row, stamped.Measured.Figures.Median.To(), in tags),
            stamped.Verdict == BenchmarkVerdict.Pass
                ? Fin.Succ(unit)
                : signals.Write(AppHostMeasure.BenchmarkRegressions.Row, 1L,
                    InstrumentSet.Tags(
                        (AppHostSlot.Suite, stamped.Suite),
                        (AppHostSlot.Case, stamped.Case),
                        (AppHostSlot.Verdict, stamped.Verdict.Key))))
            .Apply(static (_, _) => unit)
            .As()
            .ToValidation()
            .Bind(_ => judged.Map(_ => stamped).As()))
        select observed;

    static BenchmarkVerdict Settled(Validation<Error, Benchmark> judged) =>
        judged.Match(
            Succ: static passed => passed.Verdict,
            Fail: static faults => faults.AsIterable().Exists(static fault => fault is BenchmarkFault.HostMismatch)
                ? BenchmarkVerdict.HostMismatch
                : BenchmarkVerdict.Regressed);
}
```

## [03]-[CAPTURE_BOUNDARY]

- Owner: `BenchmarkArtifacts` — the contributor rows a benchmark session lends the support-bundle fan; `PerfMapLease` — the native-symbol lease bracketing a profiled window.
- Entry: `BenchmarkArtifacts.Contributor(Duration window, Dimension circularBufferMiB)` — one `SupportContributorPort` registration at the bench composition root, with the runtime EventPipe buffer bound carried as an admitted count; `PerfMapLease.Open(PerfMapType kind)` opens the emission window and disposal closes it.
- Auto: the row composes the settled `SupportArtifact.EventTrace` factory with the benchmark provider set — the sample profiler and runtime GC/JIT providers — so an on-demand capture during a regressed run lands inside the bundle's caps, redaction, and truncation law with zero new capture machinery; the lease's `kind` is caller policy from the verified `PerfMapType` cases and disposal always disables, so a window that faults mid-run still stops emitting.
- Packages: Rasm (kernel `Dimension`), Microsoft.Diagnostics.NETCore.Client, Microsoft.Diagnostics.Tracing.TraceEvent, LanguageExt.Core, NodaTime.
- Growth: a new capture depth is one `EventPipeProvider` row in the provider seq; a new artifact kind is one `SupportArtifact` factory row on the bundles owner, contributed here through that port; a new symbol posture is one `PerfMapType` case the lease already admits.
- Boundary: the bundle fan owns freeze, redact, cap, and zip — this page contributes rows and never opens a second capture window; `.gcdump` heap capture stays the `dotnet-gcdump` tool boundary the bundles page pins; `PerfMapLease` DECLARES here rather than at Observability/bundles because all three of its consumers are on this page — the gated-run bracket, the symbolizer that reads the emitted map, and the flame graph the profiler links — and a lease declared beside a support-bundle owner that never opens one is the mis-seating this move repairs; native frames in the profiled window resolve through this lease alone, so a supplied `ProfileSymbolizer` reads a map the run itself opened.

```csharp
public static class BenchmarkArtifacts {
    public static readonly Seq<EventPipeProvider> Providers = Seq(
        new EventPipeProvider("Microsoft-DotNETCore-SampleProfiler", EventLevel.Informational),
        new EventPipeProvider("Microsoft-Windows-DotNETRuntime", EventLevel.Informational, (long)(ClrTraceEventParser.Keywords.GC | ClrTraceEventParser.Keywords.Jit)));

    public static SupportContributorPort Contributor(Duration window, Dimension circularBufferMiB) =>
        new("Rasm.AppHost.Benchmarks", Seq(SupportArtifact.EventTrace(Providers, window, circularBufferMiB)));
}

public sealed record PerfMapLease(DiagnosticsClient Client) : IDisposable {
    public static PerfMapLease Open(PerfMapType kind) {
        var client = new DiagnosticsClient(Environment.ProcessId);
        client.EnablePerfMap(kind);
        return new(client);
    }

    public void Dispose() => Client.DisablePerfMap();
}
```

## [04]-[PROFILE_CORRELATION]

- Owner: `BenchmarkRun` — the ONE gated-run capability, its `Session` composition row and `Case` identity row; `ProfileSignal` `[SmartEnum<string>]` the agent tracking vocabulary realizing `ICapability<ProfileSignal>` with each row's toggle as a delegate column; `ProfileTracking` the root arming fold; `ProfileLabels` the bounded label-scope surface partitioning continuous profiles by the dimensions instruments already carry; `ProfileSample` the correlation-keyed sample the dispatcher fans; `ProfileFrameForm` the symbolization posture every sample stamps; `ProfileCapturePolicy` the admitted decode bounds; `ProfileCapture` the sample-profiler decode producer.
- Cases: `ProfileSignal` = Cpu | Allocation | Exception | Contention; `ProfileFrameForm` = Address | Resolved, the row deriving from whether composition supplied a `ProfileSymbolizer`.
- Entry: `BenchmarkRun.Execute(session, spec, harness, claim, policy)` returns `IO<Validation<Error, Benchmark>>` — the ONE gated run: it arms the tracking set, opens the symbol lease and the sample capture under one bracket, runs the harness inside one activity and one label frame, constructs the benchmark off `HostFingerprint.Current(session.Stamps)`, and folds the gate; `BenchmarkRun.Relative(session, spec, reference, subject, claim, policy)` runs BOTH lanes under one session and mints the `ReferenceEvidence` the relative gate leg reads; `ProfileTracking.Apply(held)` seats the whole tracking set at the profiler root in one fold; `ProfileLabels.Scoped(tenant, command, level, body)` runs one body under one derived label frame; `ProfileCapturePolicy.Of(...)` admits the sample-class set, frame cap, and the weight floor, nominal, and ceiling; `ProfileCapture.Bind(source, policy, hooks, attribute, symbolize, key)` subscribes the sample-profiler pair to a source another owner pumps and returns its detacher.
- Auto: the run's activity carries suite and case tags at start so they participate in the sampling verdict, and the Observability/telemetry `PyroscopeSpanProcessor` stamps `pyroscope.profile.id` on the run's root span; `LabelsWrapper.Do(labels.Activate() -> body -> reset)` restores the prior frame on every exit, so a nested scope composes and an escaped label is structurally impossible; the tracking set arms by walking `ProfileSignal.Items` against the held capability set, so a signal added to the vocabulary arms and disarms without a fifth setter call; `Bind` pairs the two sample-profiler events by thread — `ThreadSample` parks its instant and class in the pending cell keyed by `ThreadID` and the `ThreadStackWalk` that follows consumes it — so a walk arriving with no parked sample publishes nothing, and each sample's weight is the elapsed span since that thread's previous sample clamped to the policy band with the first sample taking `Nominal`, so a capture never assumes a cadence the provider row does not state; both composition-supplied delegates cross one guarded invocation that answers absence on a raise, so a symbolizer fault degrades its frame to the address form and an attribution fault drops its sample without escaping the callback and killing the producer session.
- Output: `Benchmark` is the judged run; profile samples travel through the existing dispatcher.
- Packages: Pyroscope, Microsoft.Diagnostics.Tracing.TraceEvent, Rasm (`CapabilitySet`, `HookSet`, `Op`, `Dimension`), Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox.
- Growth: a new run dimension is one tag row at activity start; a new profile dimension is one `Add` row inside `Scoped`, its vocabulary bounded by an owning SmartEnum or the tenant roster; a new tracked signal is one `ProfileSignal` row carrying its toggle; a new sample consumer is one `HookTap` at composition, never a second feed; a new decode bound is one `ProfileCapturePolicy` column its `Of` admits; a symbol source is one `ProfileSymbolizer` supplied on the session, never a second frame projection.
- Boundary: the seven owners this section declares are the STAGES of one capability — arm, bracket, run, capture, label, measure, and judge — and `BenchmarkRun.Execute` is the only entry that reaches them, so none stands alone and a caller composing them by hand re-decides the bracket order this fold owns; the wrapper composes the minted `Rasm.AppHost` source from `TelemetryIdentity.Mint` — a second `ActivitySource` for benchmarks is the process-static defect the telemetry page forecloses; profile egress stays the service-root Pyroscope seat, so a desktop bench run without a profiler endpoint runs the identical span with the linkage dormant and the label scopes no-op on the absent native agent; label cardinality shares the tenant-cap governor's budget — tenant ids come from the tenant roster, command families come from admitted `CapabilityDescriptor.Surface` values, and degradation levels come from their owning SmartEnum; `SetDynamicTag` never spells at a call site because `Scoped` owns the frame; sample delivery is the composed `HookSet` — a process-static subscription atom beside it is the second fan the hook-seat law deletes, and a tap that raises parks on the bus's own `FaultCell` as an `IsolatedFault` rather than vanishing into a discarded `Fin`; `Bind` subscribes to a source the `[03]-[CAPTURE_BOUNDARY]` `SupportArtifact.EventTrace` row already opens and pumps, so the capture opens no session, calls no `Process()`, and a second `EventPipeEventSource` over one runtime is the deleted form; callbacks reuse one record per event, so every field projects to a value inside the callback and no `TraceEvent` reference outlives its dispatch; `InstructionPointer` yields a raw code address this package never symbolizes, so an absent `ProfileSymbolizer` stamps `ProfileFrameForm.Address` and renders hex — a sample presenting unresolved pointers under the `Resolved` row claims symbolization the run never had, and the `[03]` `PerfMapLease` the run brackets is what a supplied symbolizer reads; `FrameCount` derives from payload length so a truncated record yields a short count, and every read bounds against it beneath the policy cap; frames emit root-first because index 0 is the deepest frame and the AppUi flame fold grafts head-first from the root; AppUi consumes delivered samples through its own tap on the bus, and no profiler reference crosses downstream.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ProfileSignal : ICapability<ProfileSignal> {
    public static readonly ProfileSignal Cpu = new("cpu", rank: 0,
        arm: static enabled => Profiler.Instance.SetCPUTrackingEnabled(enabled));
    public static readonly ProfileSignal Allocation = new("allocation", rank: 1,
        arm: static enabled => Profiler.Instance.SetAllocationTrackingEnabled(enabled));
    public static readonly ProfileSignal Exception = new("exception", rank: 2,
        arm: static enabled => Profiler.Instance.SetExceptionTrackingEnabled(enabled));
    public static readonly ProfileSignal Contention = new("contention", rank: 3,
        arm: static enabled => Profiler.Instance.SetContentionTrackingEnabled(enabled));

    public int Rank { get; }
    [UseDelegateFromConstructor] public partial void Arm(bool enabled);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ProfileFrameForm {
    public static readonly ProfileFrameForm Address = new("address");
    public static readonly ProfileFrameForm Resolved = new("resolved");
}

public delegate Option<string> ProfileSymbolizer(ulong instructionPointer);

public delegate Option<CorrelationId> ProfileAttribution(int threadId, Instant at);

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct ProfileSample(
    CorrelationId Correlation,
    int ThreadId,
    ClrThreadSampleType Kind,
    ProfileFrameForm Form,
    ImmutableArray<string> Frames,
    long WeightMillis,
    Instant At);

// --- [POLICIES] ------------------------------------------------------------------------
[ComplexValueObject]
[ValidationError]
public readonly partial struct ProfileCapturePolicy {
    public FrozenSet<ClrThreadSampleType> Admits { get; }
    public Dimension FrameCap { get; }
    public Duration Nominal { get; }
    public Duration Floor { get; }
    public Duration Ceiling { get; }

    public static readonly ProfileCapturePolicy Canonical = Create(
        FrozenSet.ToFrozenSet([ClrThreadSampleType.Managed, ClrThreadSampleType.External]), Dimension.Create(256),
        Duration.FromMilliseconds(10d), Duration.FromMilliseconds(1d), Duration.FromMilliseconds(250d));

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref FrozenSet<ClrThreadSampleType> admits, ref Dimension frameCap,
        ref Duration nominal, ref Duration floor, ref Duration ceiling) =>
        validationError = admits.Count > 0 && floor > Duration.Zero && floor <= nominal && nominal <= ceiling
            ? null
            : new ValidationError(string.Join(" | ", new object?[] { "profile capture requires a non-empty sample-class set and a positive floor <= nominal <= ceiling" }));

    public static Fin<ProfileCapturePolicy> Of(
        FrozenSet<ClrThreadSampleType> admits, Dimension frameCap, Duration nominal, Duration floor, Duration ceiling) =>
        Op.Of().AcceptValidated<ProfileCapturePolicy>(
            fault: Validate(admits, frameCap, nominal, floor, ceiling, out ProfileCapturePolicy value),
            admitted: value);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ProfileTracking {
    public static readonly CapabilitySet<ProfileSignal> Canonical =
        CapabilitySet<ProfileSignal>.Of(ProfileSignal.Cpu, ProfileSignal.Allocation, ProfileSignal.Exception);

    public static Unit Apply(CapabilitySet<ProfileSignal> held) =>
        ignore(toSeq(ProfileSignal.Items).Iter(row => row.Arm(held.Admits(row))));
}

public static class ProfileLabels {
    public static Unit Scoped(TenantContext tenant, CapabilityDescriptor command, DegradationLevel level, Action body) {
        LabelsWrapper.Do(
            LabelSet.Empty.BuildUpon()
                .Add(TenantContext.TenantSlot, tenant.Entry)
                .Add("command.family", command.Surface)
                .Add("degradation.level", level.Key)
                .Build(),
            static run => run(),
            body);
        return unit;
    }
}

public static class ProfileCapture {
    public static Action Bind(
        EventPipeEventSource source, ProfileCapturePolicy policy,
        HookSet<AppHostPoint, AppHostFact, TelemetrySource> hooks,
        ProfileAttribution attribute, Option<ProfileSymbolizer> symbolize, Op key) {
        var pending = Atom(HashMap<int, (Instant At, ClrThreadSampleType Kind)>());
        var seen = Atom(HashMap<int, Instant>());
        var form = symbolize.Match(None: static () => ProfileFrameForm.Address, Some: static _ => ProfileFrameForm.Resolved);
        var parser = new SampleProfilerTraceEventParser(source);

        void OnSample(ClrThreadSampleTraceData e) {
            var (thread, at, kind) = (e.ThreadID, Instant.FromDateTimeUtc(e.TimeStamp.ToUniversalTime()), e.Type);
            if (policy.Admits.Contains(kind))
                ignore(pending.Swap(held => held.AddOrUpdate(thread, (at, kind))));
        }

        void OnWalk(ClrThreadStackWalkTraceData e) {
            var thread = e.ThreadID;
            ignore(pending.Value.Find(thread).Match(
                None: static () => unit,
                Some: parked => Publish(seen, policy, hooks, attribute, form, key,
                    thread, parked, Frames(e, policy, symbolize))));
            ignore(pending.Swap(held => held.Remove(thread)));
        }

        parser.ThreadSample += OnSample;
        parser.ThreadStackWalk += OnWalk;
        return () => {
            parser.ThreadSample -= OnSample;
            parser.ThreadStackWalk -= OnWalk;
        };
    }

    static ImmutableArray<string> Frames(
        ClrThreadStackWalkTraceData walk, ProfileCapturePolicy policy, Option<ProfileSymbolizer> symbolize) {
        var depth = Math.Min(walk.FrameCount, policy.FrameCap.Value);
        var frames = ImmutableArray.CreateBuilder<string>(depth);
        for (var index = depth - 1; index >= 0; index--) {
            var pointer = walk.InstructionPointer(index);
            frames.Add(Guarded(() => symbolize.Bind(resolve => resolve(pointer)))
                .IfNone(() => string.Create(CultureInfo.InvariantCulture, $"0x{pointer:x}")));
        }
        return frames.MoveToImmutable();
    }

    static Option<T> Guarded<T>(Func<Option<T>> call) =>
        Op.Of().Catch(() => Fin.Succ(call()))
            .IfFail(static _ => Option<T>.None);

    static Unit Publish(
        Atom<HashMap<int, Instant>> seen, ProfileCapturePolicy policy,
        HookSet<AppHostPoint, AppHostFact, TelemetrySource> hooks, ProfileAttribution attribute,
        ProfileFrameForm form, Op key,
        int thread, (Instant At, ClrThreadSampleType Kind) parked, ImmutableArray<string> frames) =>
        Guarded(() => attribute(thread, parked.At)).Match(
            None: static () => unit,
            Some: correlation => {
                var weight = Weight(seen, policy, thread, parked.At);
                ignore(seen.Swap(held => held.AddOrUpdate(thread, parked.At)));
                AppHostFact fact = new AppHostFact.Profile(new ProfileSample(
                    correlation, thread, parked.Kind, form, frames,
                    weight.ToTimeSpan().Ticks / TimeSpan.TicksPerMillisecond, parked.At));
                ignore(hooks.Fire(at: fact.At, fact: fact, key: key));
                return unit;
            });

    static Duration Weight(Atom<HashMap<int, Instant>> seen, ProfileCapturePolicy policy, int thread, Instant at) =>
        seen.Value.Find(thread).Match(
            None: () => policy.Nominal,
            Some: previous => (at - previous) switch {
                var elapsed when elapsed < policy.Floor => policy.Floor,
                var elapsed when elapsed > policy.Ceiling => policy.Ceiling,
                var elapsed => elapsed,
            });

}

// --- [COMPOSITION] ---------------------------------------------------------------------
public static class BenchmarkRun {
    public sealed record Session(
        ActivitySource Source,
        InstrumentSet Instruments,
        HookSet<AppHostPoint, AppHostFact, TelemetrySource> Hooks,
        CapabilitySet<ProfileSignal> Signals,
        ProfileCapturePolicy Capture,
        PerfMapType Symbols,
        FrozenDictionary<string, string> Stamps,
        ProfileAttribution Attribute,
        Option<EventPipeEventSource> Trace = default,
        Option<ProfileSymbolizer> Symbolize = default);

    public sealed record Case(
        string Suite,
        string Name,
        TenantContext Tenant,
        CapabilityDescriptor Command,
        DegradationLevel Level,
        Option<UInt128> Corpus = default,
        Option<string> Artifact = default);

    public static IO<Validation<Error, Benchmark>> Execute(
        Session session, Case spec, Func<Fin<BenchMeasurement>> harness,
        Option<Benchmark> claim, GatePolicy policy, Op key) =>
        Braced(session, spec, harness, key)
            .Bind(measured => measured.Match(
                Succ: figures => BenchmarkGate.Gate(
                    session.Instruments,
                    Benchmark.Of(spec.Suite, spec.Name, spec.Corpus, figures,
                        session.Stamps, artifact: spec.Artifact),
                    claim, policy, key),
                Fail: fault => IO.pure(Validation<Error, Benchmark>.Fail((BenchmarkFault)fault))));

    public static IO<Validation<Error, Benchmark>> Relative(
        Session session, Case spec, Func<Fin<BenchMeasurement>> reference, Func<Fin<BenchMeasurement>> subject,
        Option<Benchmark> claim, GatePolicy policy, Op key) =>
        from baseline in Braced(session, spec with { Name = $"{spec.Name}#reference" }, reference, key)
        from measured in Braced(session, spec, subject, key)
        from gated in (baseline, measured).Apply(static (held, fresh) => (Held: held, Fresh: fresh)).As().Match(
            Succ: pair => BenchmarkGate.Gate(
                session.Instruments,
                Benchmark.Of(spec.Suite, spec.Name, spec.Corpus, pair.Fresh, session.Stamps,
                    reference: Some(new ReferenceEvidence(spec.Name,
                        HostFingerprint.Current(session.Stamps), pair.Held.Figures.Median, spec.Corpus)),
                    artifact: spec.Artifact),
                claim, policy, key),
            Fail: faults => IO.pure(Validation<Error, Benchmark>.Fail(faults)))
        select gated;

    static IO<Validation<Error, BenchMeasurement>> Braced(
        Session session, Case spec, Func<Fin<BenchMeasurement>> harness, Op key) =>
        IO.Bracket(
            Use: IO.lift(() => Opened(session)),
            Catch: static error => IO.pure(Validation<Error, BenchMeasurement>.Fail(error)),
            Fin: static held => IO.lift(() => (held.Capture.Iter(static detach => detach()), held.Symbols.Dispose(), unit).Item3))
        .Bind(held => IO.lift(() => Traced(session, spec, harness)));

    static (PerfMapLease Symbols, Option<Action> Capture) Opened(Session session) {
        ignore(ProfileTracking.Apply(session.Signals));
        return (PerfMapLease.Open(session.Symbols),
            session.Trace.Map(source => ProfileCapture.Bind(
                source, session.Capture, session.Hooks, session.Attribute, session.Symbolize, Op.Of())));
    }

    static Validation<Error, BenchMeasurement> Traced(Session session, Case spec, Func<Fin<BenchMeasurement>> harness) {
        using var activity = session.Source.StartActivity($"benchmark {spec.Suite}/{spec.Name}", ActivityKind.Internal);
        ignore(activity?.SetTag("benchmark.suite", spec.Suite));
        ignore(activity?.SetTag("benchmark.case", spec.Name));
        var measured = Fin<BenchMeasurement>.Fail(new BenchmarkFault.MeasurementRejected("the benchmark body never ran"));
        ignore(ProfileLabels.Scoped(spec.Tenant, spec.Command, spec.Level, () => measured = harness()));
        return measured
            .Do(figures => ignore(activity?.SetTag("benchmark.median.s", figures.Figures.Median.To())))
            .ToValidation();
    }
}
```

## [05]-[CLAIM_FIELD_MAP]

- Cases: admitted kernel `BenchClaim`, Fabrication `FabricationBenchClaims` rows, Rhino `BenchEvidence`, Persistence `BenchmarkRow`, Materials `BenchWorkload`, Compute `BenchmarkClaim`, and the two Grasshopper breach families.
- Law: host identity binds whole through `HostFingerprint` — machine, OS, architecture, processor count, runtime, and the ordered stamp map — never a bare host name; a custody column holding host identity as a string carries `HostFingerprint.ToString()`, the ONE render every claim store, claim wire, and gate comparison reads, so no row picks between two renders and no pair of rows disagrees about one host.
- Law: `Verdict` and `Correlation` never persist — judging is the gate fold's per run and correlation is the run message envelope's, so custody rows carry measurement and identity columns only and a persisted verdict is a stale-truth defect.
- Law: a relative claim carries `ReferenceEvidence` on the fresh benchmark and its threshold through `GatePolicy.SpeedupFloor`; missing reference evidence refuses as `ReferenceAbsent`, never an implicit pass.
- Law: a single-sample family hands `BenchMeasurement.Of` its one measured span and its bound as `ReferenceEvidence.Median`, so a breach grades as a relative claim under `GatePolicy.SpeedupFloor` and no adapter fabricates a distribution one judgment never produced; the resulting `Iqr` is zero because a one-sample distribution HAS zero spread — a measured fact of that sample, not an absent measurement wearing a zero.
- Law: a divergent family field re-cuts at its family root instead of surviving as a sibling grammar — `Corpus` entered `Benchmark` because the Bim family binds claims to input identity, and `ReferenceEvidence` entered because `BenchClaim` binds vectorized and reference lanes.
- Law: `BudgetBreach` DECLARES here as the branch's typed gate-overrun evidence — the Grasshopper capture and budget families, Materials, Fabrication, and AppUi all grade an overrun against a bound, so the row set is one vocabulary and a per-folder breach record is the twin this declaration forecloses.

| [INDEX] | [FAMILY]                 | [BENCHMARK_FIELDS]                                                                                         |
| :-----: | :----------------------- | :--------------------------------------------------------------------------------------------------------- |
|  [01]   | `BenchClaim`             | `Claim` → case; lanes → fresh/reference cases; `Corpus` slug → corpus fingerprint; `SpeedupFloor` → policy |
|  [02]   | `FabricationBenchClaims` | `BenchClaim.Claim` keys `{Suite}/{Case}`; harness result supplies measurements; corpus is absent           |
|  [03]   | `BenchEvidence`          | operation → case; batch spans → figures; allocation and host map directly                                  |
|  [04]   | `BenchmarkRow`           | key splits suite/case; custody keeps measures, corpus, artifact, host, and route                           |
|  [05]   | `BenchWorkload`          | `BenchKernel.Suite` → suite; `MaterialsBench.CaseOf` → case; `ContentKey` → corpus                         |
|  [06]   | `BenchmarkClaim`         | `Key` → suite/case; band rungs → figures; corpus, artifact, route → custody; host render → host            |
|  [07]   | `BudgetBreach`           | `Column` → case; `Fresh` → the measured span; `Held` → reference median; corpus is absent                  |
|  [08]   | `CaptureBreach`          | `Operation` → case; `Lag` → the measured span; `Bound` → reference median; `Drawn` → operations            |

Grasshopper feeds two claim families and both carry their producing bound — the budget row's own bound and the two-period capture window — so each adapter grades overrun off the row it holds and re-derives no threshold from a policy it never sees.

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
