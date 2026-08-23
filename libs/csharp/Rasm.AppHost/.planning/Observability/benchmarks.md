# [APPHOST_BENCHMARK_RAIL]

One `BenchmarkReceipt` family folds every benchmark outcome into the receipt fan: the corpus benchmark gate the kernel and compute pages cite anchors here as one typed pass-or-regress fold, on-demand deep capture rides the support-bundle contributor seam, and every gated run executes inside a minted span so the profiling rail links its flame graph to the exact case that regressed. Every speed claim in the C# corpus resolves to a `BenchmarkReceipt` this rail stamped.

Settled composition: `ReceiptSinkPort`, `ReceiptEnvelope`, and the `AppHostWireContext` roster row from Runtime/ports; `HostFingerprint` and its `Current(stamps)` ambient mint from Runtime/determinism#DETERMINISM_KERNEL; `SupportArtifact.EventTrace` and `SupportContributorPort` from Observability/bundles#CAPTURE_PIPELINE; `TelemetryIdentity.Mint` and the `PyroscopeSpanProcessor` profile linkage from Observability/telemetry; `AppHostPoint`/`AppHostFact` and the composed `HookRail` from Observability/hooks#HOOK_RAIL; `ReceiptKind.Benchmark` from Observability/instruments#RECEIPT_PROJECTION; `FaultBand.Benchmark` and `ContentHash.Of` from the kernel. BenchmarkDotNet binds in the branch test and benchmark projects per the Test Stack manifest tier, never this package's csproj; the gate compares against the Persistence reuse index by reference.

## [01]-[INDEX]

- [02]-[BENCHMARK_RECEIPT]: Receipt family, the gated-column roster, breach evidence, and the corpus-gate fold.
- [03]-[CAPTURE_SEAM]: Deep-capture contributor rows and the native-symbol lease riding the support-bundle fan.
- [04]-[PROFILE_CORRELATION]: One gated run driving span, labels, symbol lease, sample capture, and receipt.
- [05]-[CLAIM_FIELD_MAP]: One family-to-receipt field map admitting every folder claim family, and the corpus gate over the Compute-minted claim wire.
- [06]-[RESEARCH]: Catalog-blocked external member spellings.

## [02]-[BENCHMARK_RECEIPT]

- Owner: `BenchmarkReceipt` — the typed run evidence; `BenchMeasurement` the harness-edge carrier over one `Distribution<Elapsed>`; `ReferenceEvidence` the same-run relative baseline; `BenchmarkVerdict` `[SmartEnum<string>]` the gate-disposition vocabulary; `BudgetColumn` `[SmartEnum<string>]` the gated-column roster carrying each column's fresh read, baseline read, and policy budget as delegate columns; `BudgetBreach` the per-column overrun evidence; `BenchmarkFault` `[Union]` deriving through `FaultBand.Benchmark`; `GatePolicy` the admitted threshold row; `BenchmarkGate` — the corpus pass-or-regress fold.
- Cases: `BenchmarkVerdict` = Unjudged | Pass | Regressed | HostMismatch; `BudgetColumn` = Median | P95 | Allocation, each a ceiling against the held claim's own column; `BenchmarkFault` = GateRegressed | HostMismatch | ReferenceAbsent | PolicyRejected | MeasurementRejected.
- Entry: `BenchMeasurement.Of(spans, allocatedBytes, operations, key)` admits a bounded materialized span sample into exact order statistics; `BenchmarkReceipt.Of(suite, case, corpus, measured, correlation, stamps, reference, artifact)` is the one fresh mint every folder claim family reaches, stamping `HostFingerprint.Current(stamps)` and holding the verdict at its floor; `GatePolicy.Of(...)` admits finite positive budgets and a finite nonnegative optional speedup floor; `BenchmarkGate.Gate(sink, fresh, claim, policy, key)` judges the fresh run against the held claim, stamps the verdict row, fans the judged receipt through the sink under `ReceiptKind.Benchmark`, and returns the accumulating gate rail; `BenchmarkGate.Judge(...)` is the pure verdict fold the entry composes.
- Auto: the duration figures are ONE `Distribution<Elapsed>` over an ascending materialized sample — median, interquartile spread, median absolute deviation, and the `Quantiles` roster's p95 all read one sort, so a new figure is a percentile row rather than a second pass or a second carrier; the spread is EVIDENCE rather than a gate input, because a widening distribution under a held median is a stability signal a reviewer reads, not a budget a run fails, and folding it into a budget column fails every legitimately noisier lane; `HostFingerprint.Current(stamps)` stamps machine, OS, architecture, processor count, runtime, and the caller's stamp map with one ordered render, so a claim binds only against a matching host and a cross-host comparison faults as `HostMismatch` rather than a phantom regression; a corpus-bound family stamps its input fingerprint on `Corpus`, so a corpus revision re-baselines structurally — a held claim over a different corpus contributes no budget refusal; `ReferenceEvidence` carries a same-run scalar reference when a family claims relative speed, and `GatePolicy.SpeedupFloor` arms that ratio as its own gate leg; the receipt rides the HLC message envelope like every spine fact, so benchmark history orders causally with the command log; a regressed run still fans, so the Observability/instruments#RECEIPT_PROJECTION benchmark arm projects duration and regression counts off every verdict, never the passing subset alone.
- Receipt: `BenchmarkReceipt` — suite, case, host fingerprint, corpus identity, the measurement carrier, gate verdict, optional same-run reference evidence, optional artifact key, correlation.
- Packages: Rasm (`Distribution<Elapsed>`, `Elapsed`, `ContentHash`, `Op`, `FaultBand`), Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox.
- Growth: a new measured axis is one `BenchMeasurement` column threaded through `Of`; a new gated column is one `BudgetColumn` row with its three delegate columns and its `GatePolicy` budget, breaking neither the fold nor a consumer; a new verdict class is one `BenchmarkVerdict` row paired with its `BenchmarkFault` case; a new claim family is one `[05]-[CLAIM_FIELD_MAP]` row with its own `Of` call, never a positional receipt construction.
- Boundary: the duration figures are EXACT order statistics over a bounded materialized sample, the form the kernel quantile roster at `Rasm/Domain/stats#ORDER_STATISTICS` admits this carrier under — a sketch estimator crossing into a gate comparison grades a value no run produced, which is why `QuantileSketch` reaches no receipt here; host identity is the spine's own `HostFingerprint` and never a second environment record, so the claim column the Persistence reuse index holds, the Compute claim wire, and this gate all read ONE render and a second host-identity string is the twin this page deleted; this rail is the corpus benchmark gate's owner — a kernel or compute page citing the BenchmarkDotNet gate cites this fold, and a hand-rolled kernel is admitted only after its receipt defeats the library route under `Judge`; the bench project folds raw harness results to receipts at its edge, resolving `ArtifactKey` there off `ExporterBase.GetArtifactFullName(Summary)` — the public path member every shipped exporter inherits, the same path `IExporter.ExportToFiles(Summary, ILogger)` returns after writing — and hands it across as a plain `string`, so the key rides the receipt while no BenchmarkDotNet type crosses into the spine and no libs-tier catalog carries a package no libs project references; the durable claim the gate compares against is the Persistence reuse-index row resolved by content fingerprint — measured facts mint here, the claim store persists them, and neither re-derives the other.
- Boundary: the three gate legs ACCUMULATE — host identity, the relative claim, and the budget roster are independent evidence, so a host mismatch never masks a co-occurring budget breach and one run reports every reason it failed; each budget refusal names its column, its two values, and its budget as a `BudgetBreach` row, so `GateRegressed` carries structured evidence a reader filters rather than one rendered line a reader parses; an armed speedup floor with no admissible reference REFUSES as `ReferenceAbsent` because a relative claim without its baseline is a missing measurement, never an implicit pass; the verdict derives ONCE, on the fail arm, off the accumulated fault set — `Judge` stamps `Pass` itself and `Gate` re-decides nothing.

```csharp signature
namespace Rasm.AppHost.Observability; // the namespace every benchmark-peer `using Rasm.AppHost.Observability;` prelude resolves

// --- [TYPES] --------------------------------------------------------------------------------
// Fresh receipts mint Unjudged at the bench edge; only the gate fold advances the verdict.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class BenchmarkVerdict {
    public static readonly BenchmarkVerdict Unjudged = new("unjudged");
    public static readonly BenchmarkVerdict Pass = new("pass");
    public static readonly BenchmarkVerdict Regressed = new("regressed");
    public static readonly BenchmarkVerdict HostMismatch = new("host-mismatch");
}

// One row per gated column: the fresh read, the held read, and the policy budget are DELEGATE columns, so the
// judgement fold walks `Items` and a fourth gated column costs one row rather than a fourth comparison, a
// fourth formatter, and a fourth branch in a nested ternary.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class BudgetColumn {
    public static readonly BudgetColumn Median = new("median",
        read: static receipt => Some(receipt.Measured.Figures.Median.To()),
        budget: static policy => policy.MedianBudget);
    public static readonly BudgetColumn P95 = new("p95",
        read: static receipt => receipt.Measured.At(BenchMeasurement.P95).Map(static value => value.To()),
        budget: static policy => policy.P95Budget);
    public static readonly BudgetColumn Allocation = new("allocation",
        read: static receipt => Some((double)receipt.Measured.AllocatedBytes),
        budget: static policy => policy.AllocationBudget);

    [UseDelegateFromConstructor] public partial Option<double> Read(BenchmarkReceipt receipt);
    [UseDelegateFromConstructor] public partial double Budget(GatePolicy policy);

    // A column whose fresh or held value is unreadable contributes NO verdict: absence is not an overrun, and
    // fabricating one from a missing quantile grades a run on a figure the sample never carried.
    public Option<BudgetBreach> Judge(BenchmarkReceipt fresh, BenchmarkReceipt held, GatePolicy policy) =>
        from measured in Read(fresh)
        from baseline in Read(held)
        where measured > baseline * Budget(policy)
        select new BudgetBreach(Key, measured, baseline, Budget(policy));
}

// --- [MODELS] -------------------------------------------------------------------------------
// The gated columns AND the relative leg both land here, so one breach roster carries every reason a gate
// refused and a reader filters by `Column` instead of parsing a rendered line.
public readonly record struct BudgetBreach(string Column, double Fresh, double Held, double Budget);

// Duration figures are ONE exact distribution over a bounded materialized sample: median, IQR, MAD, and the
// `Quantiles` roster all read one sort, so a new figure is a percentile row rather than a second column.
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

// The three `Option<T>` columns tail the positional list carrying `= default`: the suite's `OmitAbsent` modifier
// drops an absent one at write, so a slot without a default reads back wire-required under
// `RespectRequiredConstructorParameters` and fails the decode of the payload this producer emitted.
public sealed record BenchmarkReceipt(
    string Suite,
    string Case,
    HostFingerprint Host,
    BenchMeasurement Measured,
    BenchmarkVerdict Verdict,
    CorrelationId Correlation,
    Option<UInt128> Corpus = default,
    Option<ReferenceEvidence> Reference = default,
    Option<string> ArtifactKey = default) {

    // ONE fresh mint every folder claim family reaches: the family supplies identity, the harness supplies
    // measurement, the host stamps here, and the verdict holds its floor until the gate advances it. `artifact`
    // defaults absent for a family whose run exports nothing — a caller that never ran an exporter is the
    // absent case, not a suppressed column.
    public static BenchmarkReceipt Of(
        string suite, string @case, Option<UInt128> corpus, BenchMeasurement measured,
        CorrelationId correlation, FrozenDictionary<string, string> stamps,
        Option<ReferenceEvidence> reference = default, Option<string> artifact = default) =>
        new(Suite: suite, Case: @case, Host: HostFingerprint.Current(stamps), Measured: measured,
            Verdict: BenchmarkVerdict.Unjudged, Correlation: correlation,
            Corpus: corpus, Reference: reference, ArtifactKey: artifact);

    // Case identity keys the claim store, so the preimage is FRAMED through the kernel writer: a suite or case
    // text carrying a separator would otherwise shift two field splits onto one digest.
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

// --- [ERRORS] ---------------------------------------------------------------------------
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

// --- [POLICIES] -----------------------------------------------------------------------------
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

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class BenchmarkGate {
    // Three INDEPENDENT evidence legs accumulate: a host mismatch and a budget breach are two facts about one
    // run, and sequencing them reports the first and hides the rest.
    public static Validation<Error, BenchmarkReceipt> Judge(
        BenchmarkReceipt fresh, Option<BenchmarkReceipt> claim, GatePolicy policy, Op key) =>
        (Hosts(fresh, claim, policy), Relative(fresh, policy, key), Budgets(fresh, claim, policy, key))
            .Apply(static (_, _, _) => unit)
            .Map(_ => fresh with { Verdict = BenchmarkVerdict.Pass })
            .As();

    // Both host claims answer one fault: the same-run reference and the held claim each bind to the fresh
    // fingerprint, and a digest comparison is the whole test because the render is the fingerprint's own.
    static Validation<Error, Unit> Hosts(BenchmarkReceipt fresh, Option<BenchmarkReceipt> claim, GatePolicy policy) =>
        (policy.SpeedupFloor.IsNone || fresh.Reference.Match(None: static () => true, Some: row => row.Host == fresh.Host))
        && claim.Match(None: static () => true, Some: held => held.Host == fresh.Host)
            ? Validation<Error, Unit>.Success(unit)
            : Validation<Error, Unit>.Fail(new BenchmarkFault.HostMismatch(fresh.Case));

    // An armed floor with no admissible reference REFUSES: a relative claim whose baseline never landed is a
    // missing measurement, and passing it certifies a speedup nothing measured.
    static Validation<Error, Unit> Relative(BenchmarkReceipt fresh, GatePolicy policy, Op key) =>
        policy.SpeedupFloor.Match(
            None: static () => Validation<Error, Unit>.Success(unit),
            Some: floor => Admissible(fresh).Match(
                None: () => Validation<Error, Unit>.Fail(new BenchmarkFault.ReferenceAbsent(fresh.Case)),
                Some: baseline => fresh.Measured.Figures.Median.To() is var measured
                    && measured > 0d && baseline.To() / measured >= floor
                        ? Validation<Error, Unit>.Success(unit)
                        : Validation<Error, Unit>.Fail(new BenchmarkFault.GateRegressed(
                            key, Seq(new BudgetBreach("speedup", measured, baseline.To(), floor))))));

    // Case and corpus must both match, so a reference lane measured over a different corpus reads absent
    // rather than baselining a run it never shared inputs with.
    static Option<Elapsed> Admissible(BenchmarkReceipt fresh) =>
        fresh.Reference
            .Filter(row => row.Case == fresh.Case && row.Corpus == fresh.Corpus)
            .Map(static row => row.Median);

    // A corpus revision RE-BASELINES structurally: the held claim measured a different program, so it
    // contributes no column rather than a regression against inputs the fresh run never ran.
    static Validation<Error, Unit> Budgets(
        BenchmarkReceipt fresh, Option<BenchmarkReceipt> claim, GatePolicy policy, Op key) =>
        claim.Filter(held => held.Corpus == fresh.Corpus)
            .Map(held => toSeq(BudgetColumn.Items).Bind(column => column.Judge(fresh, held, policy).ToSeq()).Strict())
            .IfNone(Seq<BudgetBreach>()) is var breaches && breaches.IsEmpty
            ? Validation<Error, Unit>.Success(unit)
            : Validation<Error, Unit>.Fail(new BenchmarkFault.GateRegressed(key, breaches));

    public static IO<Validation<Error, BenchmarkReceipt>> Gate(
        ReceiptSinkPort sink, BenchmarkReceipt fresh, Option<BenchmarkReceipt> claim, GatePolicy policy, Op key) =>
        from judged in IO.pure(Judge(fresh, claim, policy, key))
        let stamped = fresh with { Verdict = Settled(judged) }
        from _ in sink.Send(stamped.Correlation, TenantContext.Current, TelemetrySource.AppHost,
            ReceiptKind.Benchmark.Key, JsonSerializer.SerializeToElement(stamped, SuiteContracts.Host))
        select judged.Map(_ => stamped).As();

    // The verdict derives ONCE and only where `Judge` failed: a host mismatch outranks a budget breach because
    // it disqualifies the comparison the breach was measured under.
    static BenchmarkVerdict Settled(Validation<Error, BenchmarkReceipt> judged) =>
        judged.Match(
            Succ: static passed => passed.Verdict,
            Fail: static faults => faults.Exists(static fault => fault is BenchmarkFault.HostMismatch)
                ? BenchmarkVerdict.HostMismatch
                : BenchmarkVerdict.Regressed);
}
```

## [03]-[CAPTURE_SEAM]

- Owner: `BenchmarkArtifacts` — the contributor rows a benchmark session lends the support-bundle fan; `PerfMapLease` — the native-symbol lease bracketing a profiled window.
- Entry: `BenchmarkArtifacts.Contributor(Duration window, Dimension circularBufferMiB)` — one `SupportContributorPort` registration at the bench composition root, with the runtime EventPipe buffer bound carried as an admitted count; `PerfMapLease.Open(PerfMapType kind)` opens the emission window and disposal closes it.
- Auto: the row composes the settled `SupportArtifact.EventTrace` factory with the benchmark provider set — the sample profiler and runtime GC/JIT providers — so an on-demand capture during a regressed run lands inside the bundle's caps, redaction, and truncation law with zero new capture machinery; the lease's `kind` is caller policy from the verified `PerfMapType` cases and disposal always disables, so a window that faults mid-run still stops emitting.
- Packages: Rasm (kernel `Dimension`), Microsoft.Diagnostics.NETCore.Client, Microsoft.Diagnostics.Tracing.TraceEvent, LanguageExt.Core, NodaTime.
- Growth: a new capture depth is one `EventPipeProvider` row in the provider seq; a new artifact kind is one `SupportArtifact` factory row on the bundles owner, contributed here by seam; a new symbol posture is one `PerfMapType` case the lease already admits.
- Boundary: the bundle fan owns freeze, redact, cap, and zip — this page contributes rows and never opens a second capture window; `.gcdump` heap capture stays the `dotnet-gcdump` tool boundary the bundles page pins; `PerfMapLease` DECLARES here rather than at Observability/bundles because all three of its consumers are on this page — the gated-run bracket, the symbolizer that reads the emitted map, and the flame graph the profile rail links — and a lease declared beside a support-bundle owner that never opens one is the mis-seating this move repairs; native frames in the profiled window resolve through this lease alone, so a supplied `ProfileSymbolizer` reads a map the run itself opened.

```csharp signature
public static class BenchmarkArtifacts {
    public static readonly Seq<EventPipeProvider> Providers = Seq(
        new EventPipeProvider("Microsoft-DotNETCore-SampleProfiler", EventLevel.Informational),
        new EventPipeProvider("Microsoft-Windows-DotNETRuntime", EventLevel.Informational, (long)(ClrTraceEventParser.Keywords.GC | ClrTraceEventParser.Keywords.Jit)));

    public static SupportContributorPort Contributor(Duration window, Dimension circularBufferMiB) =>
        new("Rasm.AppHost.Benchmarks", Seq(SupportArtifact.EventTrace(Providers, window, circularBufferMiB)));
}

// Native-symbol lease: perf-map emission spans exactly the profiled window so sample and eBPF profilers
// resolve jitted frames; the kind row is caller policy and disposal always disables.
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

- Owner: `BenchmarkRun` — the ONE gated-run capability, its `Session` composition row and `Case` identity row; `ProfileSignal` `[SmartEnum<string>]` the agent tracking vocabulary realizing `ICapability<ProfileSignal>` with each row's toggle as a delegate column; `ProfileTracking` the root arming fold; `ProfileLabels` the bounded label-scope surface partitioning continuous profiles by the dimensions instruments already carry; `ProfileSample` the correlation-keyed sample the hook rail fans; `ProfileFrameForm` the symbolization posture every sample stamps; `ProfileCapturePolicy` the admitted decode bounds; `ProfileDisposition` `[SmartEnum<string>]` the decode-outcome vocabulary; `ProfileCaptureReceipt` the keyed disposition table; `ProfileCapture` the sample-profiler decode producer and `ProfileCaptureLease` its detaching accounting handle.
- Cases: `ProfileSignal` = Cpu | Allocation | Exception | Contention; `ProfileFrameForm` = Address | Resolved, the row deriving from whether composition supplied a `ProfileSymbolizer`; `ProfileDisposition` = Published | Unpaired | Rejected | Unattributed | Truncated | Faulted.
- Entry: `BenchmarkRun.Execute(session, spec, harness, claim, policy)` returns `IO<Validation<Error, BenchmarkReceipt>>` — the ONE gated run: it arms the tracking set, opens the symbol lease and the sample capture under one bracket, runs the harness inside one activity and one label frame, mints the receipt off `HostFingerprint.Current(session.Stamps)`, and folds the gate; `BenchmarkRun.Relative(session, spec, reference, subject, claim, policy)` runs BOTH lanes under one session and mints the `ReferenceEvidence` the relative gate leg reads; `ProfileTracking.Apply(held)` seats the whole tracking set at the profiler root in one fold; `ProfileLabels.Scoped(tenant, command, level, body)` runs one body under one derived label frame; `ProfileCapturePolicy.Of(...)` admits the sample-class set, frame cap, and the weight floor, nominal, and ceiling; `ProfileCapture.Bind(source, policy, rail, attribute, symbolize, key)` subscribes the sample-profiler pair to a source another owner pumps and returns the lease carrying live `ProfileCaptureReceipt` counts.
- Auto: the run's activity carries suite and case tags at start so they participate in the sampling verdict, and the Observability/telemetry `PyroscopeSpanProcessor` stamps `pyroscope.profile.id` on the run's root span — a regressed case's flame graph is one click from its receipt, keyed by the shared correlation; `LabelsWrapper.Do(labels.Activate() -> body -> reset)` restores the prior frame on every exit, so a nested scope composes and an escaped label is structurally impossible; the tracking set arms by walking `ProfileSignal.Items` against the held capability set, so a signal added to the vocabulary arms and disarms without a fifth setter call; `Bind` pairs the two sample-profiler events by thread — `ThreadSample` parks its instant and class in the pending cell keyed by `ThreadID` and the `ThreadStackWalk` that follows consumes it — so a walk arriving with no parked sample counts `Unpaired` rather than publishing an unweighted stack, and each sample's weight is the elapsed span since that thread's previous sample clamped to the policy band with the first sample taking `Nominal`, so a capture never assumes a cadence the provider row does not state; both composition-supplied delegates cross one guarded invocation that folds a raise onto `Faulted` and answers absence, so a symbolizer fault degrades its frame to the address form and an attribution fault drops its sample as counted evidence rather than escaping the callback and killing the producer session; every disposition lands on ONE keyed table, so a seventh outcome is a `ProfileDisposition` row and no counter column, increment body, or seed literal moves.
- Receipt: `ProfileCaptureReceipt` — one `HashMap<ProfileDisposition, long>` whose `Ordered` projection publishes the roster's own declaration order, so a byte-deriving reader never enumerates the map; `BenchmarkReceipt` is the run's own evidence and the capture receipt rides the lease beside it.
- Packages: Pyroscope, Microsoft.Diagnostics.Tracing.TraceEvent, Rasm (`CapabilitySet`, `HookRail`, `Op`, `Dimension`), Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox.
- Growth: a new run dimension is one tag row at activity start; a new profile dimension is one `Add` row inside `Scoped`, its vocabulary bounded by an owning SmartEnum or the tenant roster; a new tracked signal is one `ProfileSignal` row carrying its toggle; a new sample consumer is one `HookTap` at composition, never a second feed; a new decode bound is one `ProfileCapturePolicy` column its `Of` admits; a new decode outcome is one `ProfileDisposition` row; a symbol source is one `ProfileSymbolizer` supplied on the session, never a second frame projection.
- Boundary: the seven owners this section declares are the STAGES of one capability — arm, bracket, run, capture, label, measure, receipt — and `BenchmarkRun.Execute` is the only entry that reaches them, so none stands alone and a caller composing them by hand re-decides the bracket order this fold owns; the wrapper composes the minted `Rasm.AppHost` source from `TelemetryIdentity.Mint` — a second `ActivitySource` for benchmarks is the process-static defect the telemetry page forecloses; profile egress stays the service-root Pyroscope seat, so a desktop bench run without a profiler endpoint runs the identical span with the linkage dormant and the label scopes no-op on the absent native agent; label cardinality shares the tenant-cap governor's budget — tenant ids come from the tenant roster, command families come from admitted `CapabilityDescriptor.Surface` values, and degradation levels come from their owning SmartEnum; `SetDynamicTag` never spells at a call site because `Scoped` owns the frame; sample delivery is the composed `HookRail` — a process-static subscription atom beside it is the second fan the hook-seat law deletes, and a tap that raises parks on the rail's own `FaultCell` as an `IsolatedFault` rather than vanishing into a discarded `Fin`; `Bind` subscribes to a source the `[03]-[CAPTURE_SEAM]` `SupportArtifact.EventTrace` row already opens and pumps, so the capture opens no session, calls no `Process()`, and a second `EventPipeEventSource` over one runtime is the deleted form; callbacks reuse one record per event, so every field projects to a value inside the callback and no `TraceEvent` reference outlives its dispatch; `InstructionPointer` yields a raw code address this package never symbolizes, so an absent `ProfileSymbolizer` stamps `ProfileFrameForm.Address` and renders hex — a sample presenting unresolved pointers under the `Resolved` row claims symbolization the run never had, and the `[03]` `PerfMapLease` the run brackets is what a supplied symbolizer reads; `FrameCount` derives from payload length so a truncated record yields a short count, and every read bounds against it beneath the policy cap; frames emit root-first because index 0 is the deepest frame and the AppUi flame fold grafts head-first from the root; AppUi consumes delivered samples through its own tap on the rail, and no profiler reference crosses downstream.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
// Per-signal tracking is one capability set over one vocabulary: the toggle rides the row as a delegate
// column, so arming is a fold over `Items` rather than four sequential setter statements and a bool quartet
// whose legal corners nothing states. Contention stays out of the canonical set because routine capture cost
// exceeds its value, which is a MEMBERSHIP fact rather than a column.
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

// Symbolization posture travels WITH the stack: this decoder resolves no symbols, so a consumer reads
// Frames under the row the capture stamped instead of assuming a call stack it never had.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ProfileFrameForm {
    public static readonly ProfileFrameForm Address = new("address");
    public static readonly ProfileFrameForm Resolved = new("resolved");
}

// Every decode disposition the pump can reach, so a thin flame graph reads as counted evidence rather than an
// absent feed. The roster IS the table's key space, so a seventh outcome moves no counter column.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ProfileDisposition {
    public static readonly ProfileDisposition Published = new("published");
    public static readonly ProfileDisposition Unpaired = new("unpaired");
    public static readonly ProfileDisposition Rejected = new("rejected");
    public static readonly ProfileDisposition Unattributed = new("unattributed");
    public static readonly ProfileDisposition Truncated = new("truncated");
    public static readonly ProfileDisposition Faulted = new("faulted");
}

// Composition supplies the symbol source — the PerfMapLease map or an Etlx-resolved index; absent one, every
// frame renders as its raw code address and the sample stamps ProfileFrameForm.Address.
public delegate Option<string> ProfileSymbolizer(ulong instructionPointer);

// EventPipe carries no correlation, so attribution is composition's: the profiled window maps an OS thread
// and instant onto the correlation that owns it. An unattributed sample is counted, never guessed.
public delegate Option<CorrelationId> ProfileAttribution(int threadId, Instant at);

// --- [MODELS] -------------------------------------------------------------------------------
// Frames are ROOT-FIRST; the walk's index 0 is the deepest frame and the flame fold grafts from the root.
public readonly record struct ProfileSample(
    CorrelationId Correlation,
    int ThreadId,
    ClrThreadSampleType Kind,
    ProfileFrameForm Form,
    ImmutableArray<string> Frames,
    long WeightMillis,
    Instant At);

// One keyed table replaces six counter columns: `Bump` is the whole mutation surface and `Ordered` publishes
// the roster's declaration order, so no reader derives bytes from hash-map enumeration.
public readonly record struct ProfileCaptureReceipt(HashMap<ProfileDisposition, long> Counts) {
    public static readonly ProfileCaptureReceipt Empty =
        new(toSeq(ProfileDisposition.Items).ToHashMap(static row => row, static _ => 0L));
    public long this[ProfileDisposition slot] => Counts.Find(slot).IfNone(0L);
    public Seq<(ProfileDisposition Slot, long Count)> Ordered =>
        toSeq(ProfileDisposition.Items).Map(row => (Slot: row, Count: this[row])).Strict();
    public ProfileCaptureReceipt Bump(ProfileDisposition slot, long by = 1L) =>
        new(Counts.AddOrUpdate(slot, held => held + by, by));
}

public sealed record ProfileCaptureLease(Atom<ProfileCaptureReceipt> Counts, Action Detach) : IDisposable {
    public ProfileCaptureReceipt Receipt => Counts.Value;
    public void Dispose() => Detach();
}

// --- [POLICIES] -----------------------------------------------------------------------------
[ComplexValueObject]
[ValidationError]
public readonly partial struct ProfileCapturePolicy {
    public FrozenSet<ClrThreadSampleType> Admits { get; }
    public Dimension FrameCap { get; }
    public Duration Nominal { get; }
    public Duration Floor { get; }
    public Duration Ceiling { get; }

    // Error samples carry no usable stack; the runtime's nominal sampling cadence seats the first sample on a
    // thread, and the band clamps a weight a descheduled thread would otherwise inflate.
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

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class ProfileTracking {
    public static readonly CapabilitySet<ProfileSignal> Canonical =
        CapabilitySet<ProfileSignal>.Of(ProfileSignal.Cpu, ProfileSignal.Allocation, ProfileSignal.Exception);

    // Every row arms from the SAME set read, so an omitted row disarms rather than retaining whatever the last
    // composition left on the process-static profiler.
    public static Unit Apply(CapabilitySet<ProfileSignal> held) =>
        ignore(toSeq(ProfileSignal.Items).Iter(row => row.Arm(held.Admits(row))));
}

public static class ProfileLabels {
    // One derived label frame per governed dimension set: LabelSet.Empty.BuildUpon() derives, the
    // state-threaded Do runs the body closure-free, and the finally-reset restores the prior frame.
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
    // Subscribe-only: the EventTrace artifact row owns the session, the source, and Process(). Binding a
    // second source over one runtime opens a second EventPipe session the runtime need not grant.
    public static ProfileCaptureLease Bind(
        EventPipeEventSource source, ProfileCapturePolicy policy,
        HookRail<AppHostPoint, AppHostFact, TelemetrySource> rail,
        ProfileAttribution attribute, Option<ProfileSymbolizer> symbolize, Op key) {
        var counts = Atom(ProfileCaptureReceipt.Empty);
        var pending = Atom(HashMap<int, (Instant At, ClrThreadSampleType Kind)>());
        var seen = Atom(HashMap<int, Instant>());
        var form = symbolize.Match(None: static () => ProfileFrameForm.Address, Some: static _ => ProfileFrameForm.Resolved);
        // Microsoft.Diagnostics.Tracing.EventPipe seats this parser and both records, apart from the
        // Parsers namespace the CLR and dynamic parsers use, so the prelude here carries both.
        var parser = new SampleProfilerTraceEventParser(source);

        // Records are pooled per event, so both arms project every field to a value before returning.
        void OnSample(ClrThreadSampleTraceData e) {
            var (thread, at, kind) = (e.ThreadID, Instant.FromDateTimeUtc(e.TimeStamp.ToUniversalTime()), e.Type);
            ignore(policy.Admits.Contains(kind)
                ? pending.Swap(held => held.AddOrUpdate(thread, (at, kind)))
                : Count(counts, ProfileDisposition.Rejected));
        }

        void OnWalk(ClrThreadStackWalkTraceData e) {
            var thread = e.ThreadID;
            ignore(pending.Value.Find(thread).Match(
                None: () => Count(counts, ProfileDisposition.Unpaired),
                Some: parked => Publish(counts, seen, policy, rail, attribute, form, key,
                    thread, parked, e.FrameCount, Frames(counts, e, policy, symbolize))));
            ignore(pending.Swap(held => held.Remove(thread)));
        }

        parser.ThreadSample += OnSample;
        parser.ThreadStackWalk += OnWalk;
        return new ProfileCaptureLease(counts, () => {
            parser.ThreadSample -= OnSample;
            parser.ThreadStackWalk -= OnWalk;
        });
    }

    // Kernel exemption: a bounded reverse walk over the pooled record. FrameCount derives from the payload
    // length, so a read at or above it reads adjacent event memory — the bound is the safety.
    static ImmutableArray<string> Frames(
        Atom<ProfileCaptureReceipt> counts, ClrThreadStackWalkTraceData walk, ProfileCapturePolicy policy, Option<ProfileSymbolizer> symbolize) {
        var depth = Math.Min(walk.FrameCount, policy.FrameCap.Value);
        var frames = ImmutableArray.CreateBuilder<string>(depth);
        for (var index = depth - 1; index >= 0; index--) {
            var pointer = walk.InstructionPointer(index);
            frames.Add(Guarded(counts, () => symbolize.Bind(resolve => resolve(pointer)))
                .IfNone(() => string.Create(CultureInfo.InvariantCulture, $"0x{pointer:x}")));
        }
        return frames.MoveToImmutable();
    }

    // Both composition-supplied delegates run INSIDE the EventPipe callback, where a raise escapes `OnWalk` and
    // aborts the producer session whole, taking every later sample with it.
    static Option<T> Guarded<T>(Atom<ProfileCaptureReceipt> counts, Func<Option<T>> call) =>
        Op.Of().Catch(() => Fin.Succ(call())).Match(
            Succ: static held => held,
            Fail: _ => (Count(counts, ProfileDisposition.Faulted), Option<T>.None).Item2);

    static Unit Publish(
        Atom<ProfileCaptureReceipt> counts, Atom<HashMap<int, Instant>> seen, ProfileCapturePolicy policy,
        HookRail<AppHostPoint, AppHostFact, TelemetrySource> rail, ProfileAttribution attribute,
        ProfileFrameForm form, Op key,
        int thread, (Instant At, ClrThreadSampleType Kind) parked, int available, ImmutableArray<string> frames) =>
        Guarded(counts, () => attribute(thread, parked.At)).Match(
            None: () => Count(counts, ProfileDisposition.Unattributed),
            Some: correlation => {
                var weight = Weight(seen, policy, thread, parked.At);
                ignore(seen.Swap(held => held.AddOrUpdate(thread, parked.At)));
                AppHostFact fact = new AppHostFact.Profile(new ProfileSample(
                    correlation, thread, parked.Kind, form, frames,
                    weight.ToTimeSpan().Ticks / TimeSpan.TicksPerMillisecond, parked.At));
                // The seat rides the FACT. A raising tap parks on the rail's own FaultCell, but what `Fire`
                // ANSWERS is the seat verdict, so a discarded Fin here would erase an unmounted point.
                return rail.Fire(at: fact.At, fact: fact, key: key).Match(
                    Succ: _ => available > frames.Length
                        ? Count(counts, ProfileDisposition.Truncated, ProfileDisposition.Published)
                        : Count(counts, ProfileDisposition.Published),
                    Fail: _ => Count(counts, ProfileDisposition.Faulted));
            });

    // Weight is OBSERVED, never assumed: the span since this thread's previous admitted sample, clamped to the
    // policy band so a descheduled thread's gap never inflates one leaf into the whole graph.
    static Duration Weight(Atom<HashMap<int, Instant>> seen, ProfileCapturePolicy policy, int thread, Instant at) =>
        seen.Value.Find(thread).Match(
            None: () => policy.Nominal,
            Some: previous => (at - previous) switch {
                var elapsed when elapsed < policy.Floor => policy.Floor,
                var elapsed when elapsed > policy.Ceiling => policy.Ceiling,
                var elapsed => elapsed,
            });

    // Variadic because a truncated publish is TWO dispositions of one sample: both land in one CAS, so the
    // table never reads a state where the truncation counted and the publish had not.
    static Unit Count(Atom<ProfileCaptureReceipt> counts, params ProfileDisposition[] slots) =>
        ignore(counts.Swap(held => toSeq(slots).Fold(held, static (table, slot) => table.Bump(slot))));
}

// --- [COMPOSITION] --------------------------------------------------------------------------
// The gated-run capability: arming, the symbol and capture brackets, the span, the label frame, the receipt
// mint, and the gate fold are ONE entry, so the seven owners above have exactly one composer and none is
// reachable half-configured.
public static class BenchmarkRun {
    public sealed record Session(
        ActivitySource Source,
        ReceiptSinkPort Sink,
        HookRail<AppHostPoint, AppHostFact, TelemetrySource> Rail,
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
        CorrelationId Correlation,
        Option<UInt128> Corpus = default,
        Option<string> Artifact = default);

    public static IO<Validation<Error, BenchmarkReceipt>> Execute(
        Session session, Case spec, Func<Fin<BenchMeasurement>> harness,
        Option<BenchmarkReceipt> claim, GatePolicy policy, Op key) =>
        Braced(session, spec, harness, key)
            .Bind(measured => measured.Match(
                Succ: figures => BenchmarkGate.Gate(
                    session.Sink,
                    BenchmarkReceipt.Of(spec.Suite, spec.Name, spec.Corpus, figures, spec.Correlation,
                        session.Stamps, artifact: spec.Artifact),
                    claim, policy, key),
                Fail: fault => IO.pure(Validation<Error, BenchmarkReceipt>.Fail((BenchmarkFault)fault))));

    // A relative claim measures BOTH lanes in one session, so the reference and the subject share a host, a
    // corpus, and one symbol window — a reference carried in from another process is the host mismatch the
    // gate's own leg then refuses.
    public static IO<Validation<Error, BenchmarkReceipt>> Relative(
        Session session, Case spec, Func<Fin<BenchMeasurement>> reference, Func<Fin<BenchMeasurement>> subject,
        Option<BenchmarkReceipt> claim, GatePolicy policy, Op key) =>
        from baseline in Braced(session, spec with { Name = $"{spec.Name}#reference" }, reference, key)
        from measured in Braced(session, spec, subject, key)
        from gated in (baseline, measured).Apply(static (held, fresh) => (Held: held, Fresh: fresh)).As().Match(
            Succ: pair => BenchmarkGate.Gate(
                session.Sink,
                BenchmarkReceipt.Of(spec.Suite, spec.Name, spec.Corpus, pair.Fresh, spec.Correlation, session.Stamps,
                    reference: Some(new ReferenceEvidence(spec.Name,
                        HostFingerprint.Current(session.Stamps), pair.Held.Figures.Median, spec.Corpus)),
                    artifact: spec.Artifact),
                claim, policy, key),
            Fail: faults => IO.pure(Validation<Error, BenchmarkReceipt>.Fail(faults)))
        select gated;

    // Release brackets the ACQUISITION, never the outcome: a harness that faults still closes the perf-map
    // window and detaches the sample parser.
    static IO<Validation<Error, BenchMeasurement>> Braced(
        Session session, Case spec, Func<Fin<BenchMeasurement>> harness, Op key) =>
        IO.Bracket(
            Use: IO.lift(() => Opened(session)),
            Catch: static error => IO.pure(Validation<Error, BenchMeasurement>.Fail(error)),
            Fin: static held => IO.lift(() => (held.Capture.Iter(static lease => lease.Dispose()), held.Symbols.Dispose(), unit).Item3))
        .Bind(held => IO.lift(() => Traced(session, spec, harness)));

    static (PerfMapLease Symbols, Option<ProfileCaptureLease> Capture) Opened(Session session) {
        ignore(ProfileTracking.Apply(session.Signals));
        return (PerfMapLease.Open(session.Symbols),
            session.Trace.Map(source => ProfileCapture.Bind(
                source, session.Capture, session.Rail, session.Attribute, session.Symbolize, Op.Of())));
    }

    // Suite and case tag the activity at START so they participate in the sampling verdict, and the label
    // frame wraps the harness so every captured sample carries the run's own governed dimensions.
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

- Owner: the claim-family field map — one admission table mapping every task-named folder claim family onto the `BenchmarkReceipt` fields; a family admits by one registered row and never mints a sibling verdict grammar. This section is the corpus GATE for the benchmark wire family and nothing more: `BenchmarkClaimWire` MINTS at `Rasm.Compute` `Runtime/claims#TS_PROJECTION` under `tests/contracts/manifest.json` `BENCHMARK_CLAIM`, so this branch grades claims against the rows below and declares no wire record of its own — a second minter would fork the shape both ends already decode.
- Cases: admitted kernel `BenchClaim`, Bim `BimBenchReceipt`, Fabrication `FabricationBenchClaims` rows, Rhino `BenchEvidence`, Persistence `BenchmarkRow`, Materials `BenchWorkload`, Compute `BenchmarkClaim`, and the two Grasshopper breach families.
- Law: host identity binds whole through `HostFingerprint` — machine, OS, architecture, processor count, runtime, and the ordered stamp map — never a bare host name; a custody column holding host identity as a string carries `HostFingerprint.ToString()`, the ONE render every claim store, claim wire, and gate comparison reads, so no row picks between two renders and no pair of rows disagrees about one host.
- Law: `Verdict` and `Correlation` never persist — judging is the gate fold's per run and correlation is the run message envelope's, so custody rows carry measurement and identity columns only and a persisted verdict is a stale-truth defect.
- Law: a relative claim carries `ReferenceEvidence` on the fresh receipt and its threshold through `GatePolicy.SpeedupFloor`; missing reference evidence refuses as `ReferenceAbsent`, never an implicit pass.
- Law: a single-sample family hands `BenchMeasurement.Of` its one measured span and its bound as `ReferenceEvidence.Median`, so a breach grades as a relative claim under `GatePolicy.SpeedupFloor` and no adapter fabricates a distribution one judgment never produced; the resulting `Iqr` is zero because a one-sample distribution HAS zero spread — a measured fact of that sample, not an absent measurement wearing a zero.
- Law: a divergent family field re-cuts at its family root instead of surviving as a sibling grammar — `Corpus` entered the receipt because the Bim family binds claims to input identity, and `ReferenceEvidence` entered because `BenchClaim` binds vectorized and reference lanes.
- Law: `BudgetBreach` DECLARES here as the branch's typed gate-overrun evidence — the Grasshopper capture and budget families, Materials, Fabrication, and AppUi all grade an overrun against a bound, so the row set is one vocabulary and a per-folder breach record is the twin this declaration forecloses.

| [INDEX] | [FAMILY]                 | [RECEIPT_PROJECTION]                                                                             |
| :-----: | :----------------------- | :----------------------------------------------------------------------------------------------- |
|  [01]   | `BenchClaim`             | `Claim` → case; lanes → fresh/reference cases; `SpeedupFloor` → policy                           |
|  [02]   | `BimBenchReceipt`        | claim → case; corpus fingerprint → corpus; the measured spans fold through `BenchMeasurement.Of` |
|  [03]   | `FabricationBenchClaims` | `BenchClaim.Claim` keys `{Suite}/{Case}`; harness result supplies measurements; corpus is absent |
|  [04]   | `BenchEvidence`          | operation → case; batch spans → figures; allocation and host map directly                        |
|  [05]   | `BenchmarkRow`           | key splits suite/case; custody keeps measures, corpus, artifact, host, and route                 |
|  [06]   | `BenchWorkload`          | `BenchKernel.Suite` → suite; `MaterialsBench.CaseOf` → case; `ContentKey` → corpus               |
|  [07]   | `BenchmarkClaim`         | `Key` → suite/case; band rungs → figures; corpus, artifact, route → custody; host render → host  |
|  [08]   | `BudgetBreach`           | `Column` → case; `Fresh` → the measured span; `Held` → reference median; corpus is absent        |
|  [09]   | `CaptureBreach`          | `Operation` → case; `Lag` → the measured span; `Bound` → reference median; `Drawn` → operations  |

Grasshopper feeds two claim families and both carry their producing bound — the budget row's own bound and the two-period capture window — so each adapter grades overrun off the row it holds and re-derives no threshold from a policy it never sees.

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
