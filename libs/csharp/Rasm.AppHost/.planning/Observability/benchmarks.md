# [APPHOST_BENCHMARK_RAIL]

One `BenchmarkReceipt` family folds every benchmark outcome into the receipt fan: the corpus benchmark gate the kernel and compute pages cite anchors here as one typed pass-or-regress fold, on-demand deep capture rides the support-bundle contributor seam, and every gated run executes inside a minted span so the profiling rail links its flame graph to the exact case that regressed. Every speed claim in the C# corpus resolves to a `BenchmarkReceipt` this rail stamped.

Settled composition: `ReceiptSinkPort`, `ReceiptEnvelope`, and the `AppHostWireContext` roster row from Runtime/ports; `SupportArtifact.EventTrace`, `SupportContributorPort`, and `PerfMapLease` from Observability/bundles#CAPTURE_PIPELINE; `TelemetryIdentity.Mint` and the `PyroscopeSpanProcessor` profile linkage from Observability/telemetry; `FaultBand.Benchmark` from Runtime/lifecycle#FAULT_TABLES. BenchmarkDotNet binds in the branch test and benchmark projects per the Test Stack manifest tier, never this package's csproj; the gate compares against the Persistence reuse index by reference.

## [01]-[INDEX]

- [02]-[BENCHMARK_RECEIPT]: Receipt family, host evidence, relative evidence, and the corpus-gate fold.
- [03]-[CAPTURE_SEAM]: Deep-capture contributor rows riding the support-bundle fan.
- [04]-[PROFILE_CORRELATION]: Span-wrapped runs feeding the continuous-profiling linkage.
- [05]-[CLAIM_FIELD_MAP]: One family-to-receipt field map admitting every folder claim family.
- [06]-[RESEARCH]: Catalog-blocked external member spellings.

## [02]-[BENCHMARK_RECEIPT]

- Owner: `BenchmarkReceipt` — the typed run evidence; `BenchMeasurement` the harness-edge column carrier; `BenchmarkVerdict` `[SmartEnum<string>]` the gate-disposition vocabulary; `BenchmarkFault` `[Union]` deriving through `FaultBand.Benchmark`; `GatePolicy` the admitted threshold row; `BenchmarkGate` — the corpus pass-or-regress fold.
- Cases: `BenchmarkVerdict` = Unjudged | Pass | Regressed | HostMismatch; `BenchmarkFault` = Text | GateRegressed | HostMismatch | PolicyRejected.
- Entry: `BenchmarkReceipt.Of(suite, case, corpus, measured, correlation, reference, artifact)` is the one fresh mint every folder claim family reaches, stamping host evidence and holding the judging columns at their floor; `GatePolicy.Of(...)` admits finite positive budgets and a finite nonnegative optional speedup floor; `BenchmarkGate.Gate(ReceiptSinkPort sink, BenchmarkReceipt fresh, Option<BenchmarkReceipt> claim, GatePolicy policy)` judges the fresh run against the held claim, stamps the verdict row, fans the judged receipt through the sink under `InstrumentFan.BenchmarkKind`, and returns the gate rail; `BenchmarkGate.Judge(...)` is the pure verdict fold the entry composes.
- Auto: the three duration columns are three order statistics over ONE ascending sample — median, the p95 quantile, and the interquartile range `SortedArrayStatistics.InterquartileRange(sorted)` reads as `UpperQuartile - LowerQuartile` — so the spread costs one additional O(1) read on the array the two central columns already sorted, never a second pass or a second sketch; the spread is EVIDENCE rather than a gate input, because a widening distribution under a held median is a stability signal a reviewer reads, not a budget a run fails, and folding it into `Within` fails every legitimately noisier lane; `HostEvidence.Current()` stamps runtime, OS, and processor identity with one digest, so a claim binds only against a matching host and a cross-host comparison faults as `HostMismatch` rather than a phantom regression; a corpus-bound family stamps its input fingerprint on `Corpus`, so a corpus revision re-baselines structurally — a held claim over a different corpus never judges the fresh run; `ReferenceEvidence` carries a same-run scalar reference when a family claims relative speed, and `GatePolicy.SpeedupFloor` makes that ratio part of the same verdict fold; the receipt rides the HLC envelope like every spine fact, so benchmark history orders causally with the command log; a regressed run still fans, so the Observability/instruments#RECEIPT_PROJECTION benchmark arm projects duration and regression counts off every verdict, never the passing subset alone.
- Receipt: `BenchmarkReceipt` — suite, case, host evidence, corpus identity, median, p95, and interquartile wall duration, allocated bytes, operation count, optional same-run reference evidence, gate verdict, optional artifact key, correlation.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, System.IO.Hashing, BCL inbox (the producing edge reads its order statistics through MathNet `SortedArrayStatistics` at its own harness boundary; no MathNet type crosses onto this receipt).
- Growth: a new measured axis is one `BenchMeasurement` field threaded through `Of` and breaking the gate fold at compile time; a new verdict class is one `BenchmarkVerdict` row paired with its `BenchmarkFault` case; a new claim family is one `[05]-[CLAIM_FIELD_MAP]` row with its own `Of` call, never a positional receipt construction.
- Boundary: this rail is the corpus benchmark gate's owner — a kernel or compute page citing the BenchmarkDotNet gate cites this fold, and a hand-rolled kernel is admitted only after its receipt defeats the library route under `Judge`; the bench project folds raw harness results to receipts at its edge, resolving `ArtifactKey` there off `ExporterBase.GetArtifactFullName(Summary)` — the public path member every shipped exporter inherits, the same path `IExporter.ExportToFiles(Summary, ILogger)` returns after writing — and hands it across as a plain `string`, so the key rides the receipt while no BenchmarkDotNet type crosses into the spine and no libs-tier catalog carries a package no libs project references; the durable claim the gate compares against is the Persistence reuse-index row resolved by content fingerprint — measured facts mint here, the claim store persists them, and neither re-derives the other.

```csharp signature
public sealed record HostEvidence(string Runtime, string Os, int Processors, UInt128 Digest) {
    public static HostEvidence Current() {
        var runtime = RuntimeInformation.FrameworkDescription;
        var os = RuntimeInformation.OSDescription;
        var processors = Environment.ProcessorCount;
        var seed = $"{runtime}|{os}|{processors}|{RuntimeInformation.ProcessArchitecture}";
        return new(runtime, os, processors, XxHash128.HashToUInt128(Encoding.UTF8.GetBytes(seed)));
    }
}

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

// Harness edge shape: the five measured columns a run produces, carrying no identity and no verdict. Iqr
// carries SPREAD beside the two central reads — an interquartile range off the same ascending sample median
// and p95 already read, one extra O(1) order-statistic pair rather than a second pass. Median and p95 alone
// cannot state stability: a run whose median holds while its spread doubles reads as unchanged on both, which
// is exactly the regression a shared-machine or thermal-throttle lane produces, so the column is what makes
// that class visible on the receipt every claim resolves to.
public readonly record struct BenchMeasurement(Duration Median, Duration P95, Duration Iqr, long AllocatedBytes, long Operations);

public sealed record BenchmarkReceipt(
    string Suite,
    string Case,
    HostEvidence Host,
    Option<UInt128> Corpus,
    Duration Median,
    Duration P95,
    Duration Iqr,
    long AllocatedBytes,
    long Operations,
    Option<ReferenceEvidence> Reference,
    BenchmarkVerdict Verdict,
    Option<string> ArtifactKey,
    CorrelationId Correlation) {

    // ONE fresh mint every folder claim family reaches: the family supplies identity, the harness supplies
    // measurement, host stamps here, and the judging columns hold their floor until the gate advances them.
    // Positional construction at a folder re-decides those floors and drifts on the next field. `artifact`
    // carries the exporter path the bench edge already resolved, defaulting absent for a family whose run
    // exports nothing — a caller that never ran an exporter is the absent case, not a suppressed column.
    public static BenchmarkReceipt Of(
        string suite, string @case, Option<UInt128> corpus, BenchMeasurement measured,
        CorrelationId correlation, Option<ReferenceEvidence> reference = default,
        Option<string> artifact = default) =>
        new(Suite: suite, Case: @case, Host: HostEvidence.Current(), Corpus: corpus,
            Median: measured.Median, P95: measured.P95, Iqr: measured.Iqr, AllocatedBytes: measured.AllocatedBytes,
            Operations: measured.Operations, Reference: reference, Verdict: BenchmarkVerdict.Unjudged,
            ArtifactKey: artifact, Correlation: correlation);
}

public sealed record ReferenceEvidence(
    string Case,
    HostEvidence Host,
    Option<UInt128> Corpus,
    Duration Median);

[Union]
public abstract partial record BenchmarkFault : Expected, IValidationError<BenchmarkFault> {
    private BenchmarkFault(string detail, int code) : base(detail, code, None) { }
    public static BenchmarkFault Create(string message) => new Text(message);
    public sealed record Text : BenchmarkFault { public Text(string detail) : base(detail, FaultBand.Benchmark.Code(0)) { } }
    public sealed record GateRegressed : BenchmarkFault { public GateRegressed(string @case, string detail) : base($"{@case}: {detail}", FaultBand.Benchmark.Code(1)) { } }
    public sealed record HostMismatch : BenchmarkFault { public HostMismatch(string @case) : base(@case, FaultBand.Benchmark.Code(2)) { } }
    public sealed record PolicyRejected : BenchmarkFault { public PolicyRejected(string detail) : base(detail, FaultBand.Benchmark.Code(3)) { } }
}

public sealed record GatePolicy {
    private GatePolicy(double medianBudget, double p95Budget, double allocationBudget, Option<double> speedupFloor) =>
        (MedianBudget, P95Budget, AllocationBudget, SpeedupFloor) = (medianBudget, p95Budget, allocationBudget, speedupFloor);

    public double MedianBudget { get; }
    public double P95Budget { get; }
    public double AllocationBudget { get; }
    public Option<double> SpeedupFloor { get; }

    public static readonly GatePolicy Canonical = new(1.10d, 1.10d, 1.05d, None);

    public static Fin<GatePolicy> Of(double medianBudget, double p95Budget, double allocationBudget, Option<double> speedupFloor) =>
        double.IsFinite(medianBudget) && medianBudget > 0d
        && double.IsFinite(p95Budget) && p95Budget > 0d
        && double.IsFinite(allocationBudget) && allocationBudget > 0d
        && speedupFloor.Match(None: static () => true, Some: static floor => double.IsFinite(floor) && floor >= 0d)
            ? Fin.Succ(new GatePolicy(medianBudget, p95Budget, allocationBudget, speedupFloor))
            : Fin.Fail<GatePolicy>(new BenchmarkFault.PolicyRejected("gate thresholds must be finite; budgets must be positive and speedup floor nonnegative"));
}

public static class BenchmarkGate {
    public static Fin<BenchmarkReceipt> Judge(BenchmarkReceipt fresh, Option<BenchmarkReceipt> claim, GatePolicy policy) =>
        !ReferenceHostMatches(fresh, policy)
            ? Fin.Fail<BenchmarkReceipt>(new BenchmarkFault.HostMismatch(fresh.Case))
            : !MeetsSpeedup(fresh, policy)
                ? Fin.Fail<BenchmarkReceipt>(new BenchmarkFault.GateRegressed(fresh.Case, SpeedupDetail(fresh, policy)))
                : claim.Match(
                    None: () => Fin.Succ(fresh with { Verdict = BenchmarkVerdict.Pass }),
                    Some: held => held.Host.Digest != fresh.Host.Digest
                        ? Fin.Fail<BenchmarkReceipt>(new BenchmarkFault.HostMismatch(fresh.Case))
                        : held.Corpus != fresh.Corpus
                            ? Fin.Succ(fresh with { Verdict = BenchmarkVerdict.Pass })
                            : Within(fresh, held, policy)
                                ? Fin.Succ(fresh with { Verdict = BenchmarkVerdict.Pass })
                                : Fin.Fail<BenchmarkReceipt>(new BenchmarkFault.GateRegressed(fresh.Case,
                                    Detail(fresh, held, policy))));

    static bool Within(BenchmarkReceipt fresh, BenchmarkReceipt held, GatePolicy policy) =>
        fresh.Median <= held.Median * policy.MedianBudget
        && fresh.P95 <= held.P95 * policy.P95Budget
        && fresh.AllocatedBytes <= held.AllocatedBytes * policy.AllocationBudget;

    static bool ReferenceHostMatches(BenchmarkReceipt fresh, GatePolicy policy) =>
        policy.SpeedupFloor.Match(
            None: static () => true,
            Some: _ => fresh.Reference.Match(
                None: static () => true,
                Some: reference => reference.Host.Digest == fresh.Host.Digest));

    static bool MeetsSpeedup(BenchmarkReceipt fresh, GatePolicy policy) =>
        policy.SpeedupFloor.Match(
            None: static () => true,
            Some: floor => fresh.Reference.Match(
                None: static () => false,
                Some: reference => reference.Case == fresh.Case
                    && reference.Corpus == fresh.Corpus
                    && fresh.Median.TotalNanoseconds > 0d
                    && reference.Median.TotalNanoseconds / fresh.Median.TotalNanoseconds >= floor));

    static string Detail(BenchmarkReceipt fresh, BenchmarkReceipt held, GatePolicy policy) =>
        $"median {fresh.Median}/{held.Median} p95 {fresh.P95}/{held.P95} iqr {fresh.Iqr}/{held.Iqr} allocated {fresh.AllocatedBytes}/{held.AllocatedBytes} speedup {Speedup(fresh)}/{Floor(policy)}";

    static string SpeedupDetail(BenchmarkReceipt fresh, GatePolicy policy) =>
        $"speedup {Speedup(fresh)}/{Floor(policy)}";

    static string Speedup(BenchmarkReceipt fresh) =>
        fresh.Reference.Match(
            None: static () => "absent",
            Some: reference => fresh.Median.TotalNanoseconds > 0d
                ? (reference.Median.TotalNanoseconds / fresh.Median.TotalNanoseconds).ToString(CultureInfo.InvariantCulture)
                : "invalid");

    static string Floor(GatePolicy policy) =>
        policy.SpeedupFloor.Match(
            None: static () => "none",
            Some: floor => floor.ToString(CultureInfo.InvariantCulture));

    public static IO<Fin<BenchmarkReceipt>> Gate(ReceiptSinkPort sink, BenchmarkReceipt fresh, Option<BenchmarkReceipt> claim, GatePolicy policy) =>
        from gate in IO.pure(Judge(fresh, claim, policy))
        let judged = fresh with {
            Verdict = gate.Match(
                Succ: static passed => passed.Verdict,
                Fail: static fault => fault is BenchmarkFault.HostMismatch ? BenchmarkVerdict.HostMismatch : BenchmarkVerdict.Regressed),
        }
        from _ in sink.Send(judged.Correlation, TenantContext.Current, TelemetrySource.AppHost.Key, InstrumentFan.BenchmarkKind,
            JsonSerializer.SerializeToElement(judged, AppHostWireContext.Default.BenchmarkReceipt))
        select gate;
}
```

## [03]-[CAPTURE_SEAM]

- Owner: `BenchmarkArtifacts` — the contributor rows a benchmark session lends the support-bundle fan.
- Entry: `BenchmarkArtifacts.Contributor(Duration window, Dimension circularBufferMiB)` — one `SupportContributorPort` registration at the bench composition root, with the runtime EventPipe buffer bound carried as an admitted count.
- Auto: the row composes the settled `SupportArtifact.EventTrace` factory with the benchmark provider set — the sample profiler and runtime GC/JIT providers — so an on-demand capture during a regressed run lands inside the bundle's caps, redaction, and truncation law with zero new capture machinery.
- Packages: Rasm (kernel `Dimension`), Microsoft.Diagnostics.NETCore.Client, LanguageExt.Core, NodaTime.
- Growth: a new capture depth is one `EventPipeProvider` row in the provider seq; a new artifact kind is one `SupportArtifact` factory row on the bundles owner, contributed here by seam.
- Boundary: the bundle fan owns freeze, redact, cap, and zip — this page contributes rows and never opens a second capture window; `.gcdump` heap capture stays the `dotnet-gcdump` tool boundary the bundles page pins; native frames in the profiled window resolve through the Observability/bundles#CAPTURE_PIPELINE `PerfMapLease` the bench root opens around the gated run, so the flame graph the profile rail links carries jitted-frame symbols.

```csharp signature
public static class BenchmarkArtifacts {
    public static readonly Seq<EventPipeProvider> Providers = Seq(
        new EventPipeProvider("Microsoft-DotNETCore-SampleProfiler", EventLevel.Informational),
        new EventPipeProvider("Microsoft-Windows-DotNETRuntime", EventLevel.Informational, (long)(ClrTraceEventParser.Keywords.GC | ClrTraceEventParser.Keywords.Jit)));

    public static SupportContributorPort Contributor(Duration window, Dimension circularBufferMiB) =>
        new("Rasm.AppHost.Benchmarks", Seq(SupportArtifact.EventTrace(Providers, window, circularBufferMiB)));
}
```

## [04]-[PROFILE_CORRELATION]

- Owner: `BenchmarkRun` — the span-wrapped run boundary; `ProfileTracking` the per-signal agent tracking policy; `ProfileLabels` the bounded label-scope surface partitioning continuous profiles by the dimensions instruments already carry; `ProfileSample` and `ProfileFeed` — the correlation-keyed delivery seat the `UiSchedulerPort.ProfileSamples` registration row exposes to the AppUi devloop flame fold; `ProfileFrameForm` the symbolization posture every sample stamps; `ProfileCapturePolicy` the admitted decode bounds; `ProfileCapture` the sample-profiler decode producer and `ProfileCaptureLease` its detaching accounting handle.
- Cases: `ProfileFrameForm` = Address | Resolved, the row deriving from whether composition supplied a `ProfileSymbolizer`.
- Entry: `BenchmarkRun.Traced(ActivitySource source, string suite, string @case, Func<Fin<BenchmarkReceipt>> run)` — every gated run executes inside one activity; `ProfileTracking.Apply()` seats the four `Profiler.Instance` tracking toggles once at the profiler root; `ProfileLabels.Scoped(TenantContext tenant, CapabilityDescriptor command, DegradationLevel level, Action body)` runs one body under one derived label frame; `ProfileFeed.Subscribe(Action<ProfileSample>)` returns a token-bearing detacher and `ProfileFeed.Publish(ProfileSample)` fans one captured sample over an immutable snapshot; `ProfileCapturePolicy.Of(...)` admits the sample-class set, frame cap, and the weight floor, nominal, and ceiling; `ProfileCapture.Bind(EventPipeEventSource source, ProfileCapturePolicy policy, ProfileAttribution attribute, Option<ProfileSymbolizer> symbolize)` subscribes the sample-profiler pair to a source another owner pumps and returns the lease carrying live `ProfileCaptureReceipt` counts.
- Auto: the activity carries suite and case tags at start so they participate in the sampling verdict, and the Observability/telemetry `PyroscopeSpanProcessor` stamps `pyroscope.profile.id` on the run's root span — a regressed case's flame graph is one click from its receipt, keyed by the shared correlation; `LabelsWrapper.Do(labels.Activate() -> body -> reset)` restores the prior frame on every exit, so a nested scope composes and an escaped label is structurally impossible; duplicate tap delegates receive distinct tokens, detachment removes one token only, and each tap runs under its own `Try` so a failed consumer never interrupts later taps or the producer; `Bind` pairs the two sample-profiler events by thread — `ThreadSample` parks its instant and class in the pending cell keyed by `ThreadID` and the `ThreadStackWalk` that follows consumes it — so a walk arriving with no parked sample counts `Unpaired` rather than publishing an unweighted stack, and each sample's weight is the elapsed span since that thread's previous sample clamped to the policy band with the first sample taking `Nominal`, so a capture never assumes a cadence the provider row does not state; both composition-supplied delegates cross one guarded invocation that folds a raise onto `Faulted` and answers absence, so a symbolizer fault degrades its frame to the address form and an attribution fault drops its sample as counted evidence rather than escaping the callback and killing the producer session.
- Packages: Pyroscope, Microsoft.Diagnostics.Tracing.TraceEvent, LanguageExt.Core, NodaTime, BCL inbox.
- Growth: a new run dimension is one tag row at activity start; a new profile dimension is one `Add` row inside `Scoped`, its vocabulary bounded by an owning SmartEnum or the tenant roster; a new tracked signal is one `ProfileTracking` column; a new sample consumer is one `Subscribe` registration, never a second feed; a new decode bound is one `ProfileCapturePolicy` column its `Of` admits; a symbol source is one `ProfileSymbolizer` supplied at `Bind`, never a second frame projection.
- Boundary: the wrapper composes the minted `Rasm.AppHost` source from `TelemetryIdentity.Mint` — a second `ActivitySource` for benchmarks is the process-static defect the telemetry page forecloses; profile egress stays the service-root Pyroscope seat, so a desktop bench run without a profiler endpoint runs the identical span with the linkage dormant and the label scopes no-op on the absent native agent; label cardinality shares the tenant-cap governor's budget — tenant ids come from the tenant roster, command families come from admitted `CapabilityDescriptor.Surface` values, and degradation levels come from their owning SmartEnum; `SetDynamicTag` never spells at a call site because `Scoped` owns the frame; `Bind` subscribes to a source the `[03]-[CAPTURE_SEAM]` `SupportArtifact.EventTrace` row already opens and pumps, so the capture opens no session, calls no `Process()`, and a second `EventPipeEventSource` over one runtime is the deleted form; callbacks reuse one record per event, so every field projects to a value inside the callback and no `TraceEvent` reference outlives its dispatch; `InstructionPointer` yields a raw code address this package never symbolizes, so an absent `ProfileSymbolizer` stamps `ProfileFrameForm.Address` and renders hex — a sample presenting unresolved pointers under the `Resolved` row claims symbolization the run never had, and the Observability/bundles `PerfMapLease` the bench root opens is what a supplied symbolizer reads; `FrameCount` derives from payload length so a truncated record yields a short count, and every read bounds against it beneath the policy cap; frames emit root-first because index 0 is the deepest frame and the AppUi flame fold grafts head-first from the root; AppUi consumes delivered samples through the port row, and no profiler reference crosses downstream.

```csharp signature
public static class BenchmarkRun {
    public static Fin<BenchmarkReceipt> Traced(ActivitySource source, string suite, string @case, Func<Fin<BenchmarkReceipt>> run) {
        using var activity = source.StartActivity($"benchmark {suite}/{@case}", ActivityKind.Internal);
        ignore(activity?.SetTag("benchmark.suite", suite));
        ignore(activity?.SetTag("benchmark.case", @case));
        return run().Map(receipt => {
            ignore(activity?.SetTag("benchmark.median.ns", receipt.Median.TotalNanoseconds));
            return receipt;
        });
    }
}

// Per-signal tracking is one root policy; contention stays off because routine capture cost exceeds value.
public sealed record ProfileTracking(bool Cpu, bool Allocation, bool Exception, bool Contention) {
    public static readonly ProfileTracking Canonical = new(Cpu: true, Allocation: true, Exception: true, Contention: false);

    public Unit Apply() {
        Profiler.Instance.SetCPUTrackingEnabled(Cpu);
        Profiler.Instance.SetAllocationTrackingEnabled(Allocation);
        Profiler.Instance.SetExceptionTrackingEnabled(Exception);
        Profiler.Instance.SetContentionTrackingEnabled(Contention);
        return unit;
    }
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

// Correlation-keyed sample feed reaches UiSchedulerPort.ProfileSamples: AppHost owns
// capture, AppUi folds delivered samples into FlameNode children keyed by correlation.
// Frames are ROOT-FIRST; the walk's index 0 is the deepest frame and the flame fold grafts from the root.
public readonly record struct ProfileSample(
    CorrelationId Correlation,
    int ThreadId,
    ClrThreadSampleType Kind,
    ProfileFrameForm Form,
    ImmutableArray<string> Frames,
    long WeightMillis,
    Instant At);

// Symbolization posture travels WITH the stack: this decoder resolves no symbols, so a consumer
// reads Frames under the row the capture stamped instead of assuming a call stack it never had.
[SmartEnum<string>]
public sealed partial class ProfileFrameForm {
    public static readonly ProfileFrameForm Address = new("address");
    public static readonly ProfileFrameForm Resolved = new("resolved");
}

// Composition supplies the symbol source — the PerfMapLease map or an Etlx-resolved index; absent
// one, every frame renders as its raw code address and the sample stamps ProfileFrameForm.Address.
public delegate Option<string> ProfileSymbolizer(ulong instructionPointer);

// EventPipe carries no correlation, so attribution is composition's: the profiled window maps an OS
// thread and instant onto the correlation that owns it. An unattributed sample is counted, never guessed.
public delegate Option<CorrelationId> ProfileAttribution(int threadId, Instant at);

public sealed record ProfileCapturePolicy {
    private ProfileCapturePolicy(Seq<ClrThreadSampleType> admits, Dimension frameCap, Duration nominal, Duration floor, Duration ceiling) =>
        (Admits, FrameCap, Nominal, Floor, Ceiling) = (admits, frameCap, nominal, floor, ceiling);

    public Seq<ClrThreadSampleType> Admits { get; }
    public Dimension FrameCap { get; }
    public Duration Nominal { get; }
    public Duration Floor { get; }
    public Duration Ceiling { get; }

    // Error samples carry no usable stack; the runtime's nominal sampling cadence seats the first
    // sample on a thread, and the band clamps a weight a descheduled thread would otherwise inflate.
    public static readonly ProfileCapturePolicy Canonical = new(
        Seq(ClrThreadSampleType.Managed, ClrThreadSampleType.External), Dimension.Create(256),
        Duration.FromMilliseconds(10d), Duration.FromMilliseconds(1d), Duration.FromMilliseconds(250d));

    public static Fin<ProfileCapturePolicy> Of(
        Seq<ClrThreadSampleType> admits, Dimension frameCap, Duration nominal, Duration floor, Duration ceiling) =>
        !admits.IsEmpty && floor > Duration.Zero && floor <= nominal && nominal <= ceiling
            ? Fin.Succ(new ProfileCapturePolicy(admits.Distinct().ToSeq().Strict(), frameCap, nominal, floor, ceiling))
            : Fin.Fail<ProfileCapturePolicy>(new BenchmarkFault.PolicyRejected(
                "profile capture requires a non-empty sample-class set and a positive floor <= nominal <= ceiling"));
}

// Every decode disposition the pump can reach, so a thin flame graph reads as counted evidence
// rather than an absent feed: an unpaired walk, a rejected class, a lost correlation, and a raising
// composition delegate each name themselves.
public sealed record ProfileCaptureReceipt(long Published, long Unpaired, long Rejected, long Unattributed, long Truncated, long Faulted) {
    public static readonly ProfileCaptureReceipt Empty = new(0L, 0L, 0L, 0L, 0L, 0L);
}

public sealed record ProfileCaptureLease(Atom<ProfileCaptureReceipt> Counts, Action Detach) : IDisposable {
    public ProfileCaptureReceipt Receipt => Counts.Value;
    public void Dispose() => Detach();
}

public static class ProfileCapture {
    // Subscribe-only: the EventTrace artifact row owns the session, the source, and Process(). Binding
    // a second source over one runtime opens a second EventPipe session the runtime need not grant.
    public static ProfileCaptureLease Bind(
        EventPipeEventSource source, ProfileCapturePolicy policy, ProfileAttribution attribute, Option<ProfileSymbolizer> symbolize) {
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
            ignore(policy.Admits.Exists(admitted => admitted == kind)
                ? pending.Swap(held => held.AddOrUpdate(thread, (at, kind)))
                : Count(counts, static row => row with { Rejected = row.Rejected + 1L }));
        }

        void OnWalk(ClrThreadStackWalkTraceData e) {
            var thread = e.ThreadID;
            ignore(pending.Value.Find(thread).Match(
                None: () => Count(counts, static row => row with { Unpaired = row.Unpaired + 1L }),
                Some: parked => Publish(counts, seen, policy, attribute, form, thread, parked, e.FrameCount, Frames(counts, e, policy, symbolize))));
            ignore(pending.Swap(held => held.Remove(thread)));
        }

        parser.ThreadSample += OnSample;
        parser.ThreadStackWalk += OnWalk;
        return new ProfileCaptureLease(counts, () => {
            parser.ThreadSample -= OnSample;
            parser.ThreadStackWalk -= OnWalk;
        });
    }

    // Kernel exemption: a bounded reverse walk over the pooled record. FrameCount derives from the
    // payload length, so a read at or above it reads adjacent event memory — the bound is the safety.
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
    // aborts the producer session whole, taking every later sample with it. One guard folds each fault onto the
    // receipt's own column and answers absence, so a broken symbolizer degrades that frame to its address form
    // and a broken attribution drops that sample — both counted evidence, never a dead pump.
    static Option<T> Guarded<T>(Atom<ProfileCaptureReceipt> counts, Func<Option<T>> call) =>
        Try.lift(call).Run().Match(
            Succ: static held => held,
            Fail: _ => (Count(counts, static row => row with { Faulted = row.Faulted + 1L }), Option<T>.None).Item2);

    static Unit Publish(
        Atom<ProfileCaptureReceipt> counts, Atom<HashMap<int, Instant>> seen, ProfileCapturePolicy policy,
        ProfileAttribution attribute, ProfileFrameForm form,
        int thread, (Instant At, ClrThreadSampleType Kind) parked, int available, ImmutableArray<string> frames) =>
        Guarded(counts, () => attribute(thread, parked.At)).Match(
            None: () => Count(counts, static row => row with { Unattributed = row.Unattributed + 1L }),
            Some: correlation => {
                var weight = Weight(seen, policy, thread, parked.At);
                ignore(seen.Swap(held => held.AddOrUpdate(thread, parked.At)));
                ignore(ProfileFeed.Publish(new ProfileSample(
                    correlation, thread, parked.Kind, form, frames, weight.ToTimeSpan().Ticks / TimeSpan.TicksPerMillisecond, parked.At)));
                return Count(counts, row => row with {
                    Published = row.Published + 1L,
                    Truncated = row.Truncated + (available > frames.Length ? 1L : 0L),
                });
            });

    // Weight is OBSERVED, never assumed: the span since this thread's previous admitted sample, clamped
    // to the policy band so a descheduled thread's gap never inflates one leaf into the whole graph.
    static Duration Weight(Atom<HashMap<int, Instant>> seen, ProfileCapturePolicy policy, int thread, Instant at) =>
        seen.Value.Find(thread).Match(
            None: () => policy.Nominal,
            Some: previous => (at - previous) switch {
                var elapsed when elapsed < policy.Floor => policy.Floor,
                var elapsed when elapsed > policy.Ceiling => policy.Ceiling,
                var elapsed => elapsed,
            });

    static Unit Count(Atom<ProfileCaptureReceipt> counts, Func<ProfileCaptureReceipt, ProfileCaptureReceipt> step) =>
        ignore(counts.Swap(step));
}

public static class ProfileFeed {
    static readonly Atom<Seq<ProfileSubscription>> Taps = Atom(Seq<ProfileSubscription>());

    public static IDisposable Subscribe(Action<ProfileSample> tap) {
        var subscription = new ProfileSubscription(Guid.CreateVersion7(), tap);
        ignore(Taps.Swap(held => held.Add(subscription)));
        return new ProfileDetacher(subscription.Token, token =>
            ignore(Taps.Swap(held => held.Filter(row => row.Token != token).ToSeq().Strict())));
    }

    public static Unit Publish(ProfileSample sample) =>
        ignore(Taps.Value.Iter(row => ignore(Try.lift(() => row.Tap(sample)).Run())));
}

file sealed record ProfileSubscription(Guid Token, Action<ProfileSample> Tap);

file sealed record ProfileDetacher(Guid Token, Action<Guid> Release) : IDisposable {
    public void Dispose() => Release(Token);
}
```

## [05]-[CLAIM_FIELD_MAP]

- Owner: the claim-family field map — one admission table mapping every task-named folder claim family onto the `BenchmarkReceipt` fields; a family admits by one registered row and never mints a sibling verdict grammar.
- Cases: admitted kernel `BenchClaim`, Bim `BimBenchReceipt`, Fabrication `FabricationBenchClaims` rows, Rhino `BenchEvidence`, Persistence `BenchmarkRow`, Materials `BenchWorkload`, Compute `BenchmarkClaim`, and the two Grasshopper breach families `BudgetBreach` and `CaptureBreach`.
- Law: `HostEvidence` binds whole — runtime, OS, processors, digest — never a bare host name; a custody column holding host identity as a string carries either the Compute `HostFingerprint.ToString` render or the `HostEvidence` digest hex, one host-identity string per claim, and the two renders never mix inside one row.
- Law: `Verdict` and `Correlation` never persist — judging is the gate fold's per run and correlation is the run envelope's, so custody rows carry measurement and identity columns only and a persisted verdict is a stale-truth defect.
- Law: a relative claim carries `ReferenceEvidence` on the fresh receipt and its threshold through `GatePolicy.SpeedupFloor`; missing reference evidence is a regression, never an implicit pass.
- Law: a single-sample family fills median and p95 from its one measured cost and its bound as `ReferenceEvidence.Median`, so a breach grades as a relative claim under `GatePolicy.SpeedupFloor` and no adapter fabricates a distribution one judgment never produced; its `Iqr` is `Duration.Zero` because a one-sample distribution HAS zero spread — a measured fact of that sample, not an absent measurement wearing a zero.
- Law: a divergent family field re-cuts at its family root instead of surviving as a sibling grammar — `Corpus` entered the receipt because the Bim family binds claims to input identity, and `ReferenceEvidence` entered because `BenchClaim` binds vectorized and reference lanes.

| [INDEX] | [FAMILY]                 | [RECEIPT_PROJECTION]                                                                               |
| :-----: | :----------------------- | :------------------------------------------------------------------------------------------------- |
|  [01]   | `BenchClaim`             | `Claim` → case; lanes → fresh/reference cases; `SpeedupFloor` → policy                             |
|  [02]   | `BimBenchReceipt`        | claim → case; corpus fingerprint → corpus; all four measurements map directly                      |
|  [03]   | `FabricationBenchClaims` | `BenchClaim.Claim` keys `{Suite}/{Case}`; harness result supplies measurements; corpus is absent   |
|  [04]   | `BenchEvidence`          | operation → case; batch duration → median/p95; allocation and host map directly                    |
|  [05]   | `BenchmarkRow`           | key splits suite/case; custody keeps measures, corpus, artifact, host, and route                   |
|  [06]   | `BenchWorkload`          | `BenchKernel.Suite` → suite; `MaterialsBench.CaseOf` → case; `ContentKey` → corpus                 |
|  [07]   | `BenchmarkClaim`         | `Key` → suite/case; band rungs → median/p95; corpus, artifact, route → custody; host render → host |
|  [08]   | `BudgetBreach`           | `Row.Key` → case; `Cost` → median and p95; `Bound` → reference median; corpus is absent            |
|  [09]   | `CaptureBreach`          | `Operation` → case; `Lag` → median and p95; `Bound` → reference median; `Drawn` → operations       |

Grasshopper feeds two claim families and both carry their producing bound — `BudgetBreach` its `BudgetRow` bound, `CaptureBreach` the two-period capture window — so each adapter grades overrun off the row it holds and re-derives no threshold from a policy it never sees.

## [06]-[RESEARCH]

(none)
