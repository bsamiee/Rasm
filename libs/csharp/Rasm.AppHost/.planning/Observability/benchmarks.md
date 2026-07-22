# [APPHOST_BENCHMARK_RAIL]

One `BenchmarkReceipt` family folds every benchmark outcome into the receipt fan: the corpus benchmark gate the kernel and compute pages cite anchors here as one typed pass-or-regress fold, on-demand deep capture rides the support-bundle contributor seam, and every gated run executes inside a minted span so the profiling rail links its flame graph to the exact case that regressed. A speed claim anywhere in the C# corpus resolves to a `BenchmarkReceipt` this rail stamped.

Settled composition: `ReceiptSinkPort`, `ReceiptEnvelope`, and the `AppHostWireContext` roster row from Runtime/ports; `SupportArtifact.EventTrace` and `SupportContributorPort` from Observability/bundles#CAPTURE_PIPELINE; `TelemetryIdentity.Mint` and the `PyroscopeSpanProcessor` profile linkage from Observability/telemetry; `FaultBand.Benchmark` from Runtime/lifecycle#FAULT_TABLES. BenchmarkDotNet binds in the branch test and benchmark projects per the Test Stack manifest tier, never this package's csproj; the durable claim store the gate compares against is the Persistence reuse index, read by reference.

## [01]-[INDEX]

- [02]-[BENCHMARK_RECEIPT]: Receipt family, host evidence, relative evidence, and the corpus-gate fold.
- [03]-[CAPTURE_SEAM]: Deep-capture contributor rows riding the support-bundle fan.
- [04]-[PROFILE_CORRELATION]: Span-wrapped runs feeding the continuous-profiling linkage.
- [05]-[CLAIM_FIELD_MAP]: One family-to-receipt field map admitting every folder claim family.
- [06]-[RESEARCH]: Catalog-blocked external member spellings.

## [02]-[BENCHMARK_RECEIPT]

- Owner: `BenchmarkReceipt` — the typed run evidence; `BenchmarkVerdict` `[SmartEnum<string>]` the gate-disposition vocabulary; `BenchmarkFault` `[Union]` deriving through `FaultBand.Benchmark`; `GatePolicy` the admitted threshold row; `BenchmarkGate` — the corpus pass-or-regress fold.
- Cases: `BenchmarkVerdict` = Unjudged | Pass | Regressed | HostMismatch; `BenchmarkFault` = Text | GateRegressed | HostMismatch | PolicyRejected.
- Entry: `GatePolicy.Of(...)` admits finite positive budgets and a finite nonnegative optional speedup floor; `BenchmarkGate.Gate(ReceiptSinkPort sink, BenchmarkReceipt fresh, Option<BenchmarkReceipt> claim, GatePolicy policy)` judges the fresh run against the held claim, stamps the verdict row, fans the judged receipt through the sink under `InstrumentFan.BenchmarkKind`, and returns the gate rail; `BenchmarkGate.Judge(...)` is the pure verdict fold the entry composes.
- Auto: `HostEvidence.Current()` stamps runtime, OS, and processor identity with one digest, so a claim binds only against a matching host and a cross-host comparison faults as `HostMismatch` rather than a phantom regression; a corpus-bound family stamps its input fingerprint on `Corpus`, so a corpus revision re-baselines structurally — a held claim over a different corpus never judges the fresh run; `ReferenceEvidence` carries a same-run scalar reference when a family claims relative speed, and `GatePolicy.SpeedupFloor` makes that ratio part of the same verdict fold; the receipt rides the HLC envelope like every spine fact, so benchmark history orders causally with the command log; a regressed run still fans, so the Observability/instruments#RECEIPT_PROJECTION benchmark arm projects duration and regression counts off every verdict, never the passing subset alone.
- Receipt: `BenchmarkReceipt` — suite, case, host evidence, corpus identity, median and p95 wall duration, allocated bytes, operation count, optional same-run reference evidence, gate verdict, optional artifact key, correlation.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, System.IO.Hashing, BCL inbox.
- Growth: a new measured axis is one field on the receipt breaking the gate fold at compile time; a new verdict class is one `BenchmarkVerdict` row paired with its `BenchmarkFault` case.
- Boundary: this rail is the corpus benchmark gate's owner — a kernel or compute page citing the BenchmarkDotNet gate cites this fold, and a hand-rolled kernel is admitted only after its receipt defeats the library route under `Judge`; the bench project folds raw harness results to receipts at its edge, and `ArtifactKey` stays `None` until the exact BenchmarkDotNet exporter member enters an applicable API catalog, so no unverified member or BenchmarkDotNet type crosses into the spine; the durable claim the gate compares against is the Persistence reuse-index row resolved by content fingerprint — measured facts mint here, the claim store persists them, and neither re-derives the other.

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

// A fresh receipt mints Unjudged at the bench edge; only the gate fold advances the verdict.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class BenchmarkVerdict {
    public static readonly BenchmarkVerdict Unjudged = new("unjudged");
    public static readonly BenchmarkVerdict Pass = new("pass");
    public static readonly BenchmarkVerdict Regressed = new("regressed");
    public static readonly BenchmarkVerdict HostMismatch = new("host-mismatch");
}

public sealed record BenchmarkReceipt(
    string Suite,
    string Case,
    HostEvidence Host,
    Option<UInt128> Corpus,
    Duration Median,
    Duration P95,
    long AllocatedBytes,
    long Operations,
    Option<ReferenceEvidence> Reference,
    BenchmarkVerdict Verdict,
    Option<string> ArtifactKey,
    CorrelationId Correlation);

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
        $"median {fresh.Median}/{held.Median} p95 {fresh.P95}/{held.P95} allocated {fresh.AllocatedBytes}/{held.AllocatedBytes} speedup {Speedup(fresh)}/{Floor(policy)}";

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

- Owner: `BenchmarkRun` — the span-wrapped run boundary; `ProfileTracking` the per-signal agent tracking policy; `ProfileLabels` the bounded label-scope surface partitioning continuous profiles by the dimensions instruments already carry; `ProfileSample` and `ProfileFeed` — the correlation-keyed delivery seat the `UiSchedulerPort.ProfileSamples` registration row exposes to the AppUi devloop flame fold.
- Entry: `BenchmarkRun.Traced(ActivitySource source, string suite, string @case, Func<Fin<BenchmarkReceipt>> run)` — every gated run executes inside one activity; `ProfileTracking.Apply()` seats the four `Profiler.Instance` tracking toggles once at the profiler root; `ProfileLabels.Scoped(TenantContext tenant, CapabilityDescriptor command, DegradationLevel level, Action body)` runs one body under one derived label frame; `ProfileFeed.Subscribe(Action<ProfileSample>)` returns a token-bearing detacher and `ProfileFeed.Publish(ProfileSample)` fans one captured sample over an immutable snapshot.
- Auto: the activity carries suite and case tags at start so they participate in the sampling verdict, and the Observability/telemetry `PyroscopeSpanProcessor` stamps `pyroscope.profile.id` on the run's root span — a regressed case's flame graph is one click from its receipt, keyed by the shared correlation; `LabelsWrapper.Do(labels.Activate() -> body -> reset)` restores the prior frame on every exit, so a nested scope composes and an escaped label is structurally impossible; duplicate tap delegates receive distinct tokens, detachment removes one token only, and each tap runs under its own `Try` so a failed consumer never interrupts later taps or the producer.
- Packages: Pyroscope, LanguageExt.Core, NodaTime, BCL inbox.
- Growth: a new run dimension is one tag row at activity start; a new profile dimension is one `Add` row inside `Scoped`, its vocabulary bounded by an owning SmartEnum or the tenant roster; a new tracked signal is one `ProfileTracking` column; a new sample consumer is one `Subscribe` registration, never a second feed.
- Boundary: the wrapper composes the minted `Rasm.AppHost` source from `TelemetryIdentity.Mint` — a second `ActivitySource` for benchmarks is the process-static defect the telemetry page forecloses; profile egress stays the service-root Pyroscope seat, so a desktop bench run without a profiler endpoint runs the identical span with the linkage dormant and the label scopes no-op on the absent native agent; label cardinality shares the tenant-cap governor's budget — tenant ids come from the tenant roster, command families come from admitted `CapabilityDescriptor.Surface` values, and degradation levels come from their owning SmartEnum; `SetDynamicTag` never spells at a call site because `Scoped` owns the frame; the sample feed is the `[PROFILE_FLAME_JOIN]` delivery seam, but capture remains outside settled code until the terminal research route verifies the SampleProfiler callback and stack accessors; AppUi consumes delivered samples through the port row, and no profiler reference crosses downstream.

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
                .Add(TenantContext.TenantSlot, tenant.TenantId.Value.ToString())
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
public readonly record struct ProfileSample(
    CorrelationId Correlation,
    ImmutableArray<string> Frames,
    long WeightMillis,
    Instant At);

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
- Cases: admitted kernel `BenchClaim`, Bim `BimBenchReceipt`, Fabrication `FabricationBench`, Rhino `BenchEvidence`, and Persistence `BenchmarkRow` projections.
- Law: `HostEvidence` binds whole — runtime, OS, processors, digest — never a bare host name; a custody column holding host identity as a string carries either the Compute `HostFingerprint.ToString` render or the `HostEvidence` digest hex, one host-identity string per claim, and the two renders never mix inside one row.
- Law: `Verdict` and `Correlation` never persist — judging is the gate fold's per run and correlation is the run envelope's, so custody rows carry measurement and identity columns only and a persisted verdict is a stale-truth defect.
- Law: a relative claim carries `ReferenceEvidence` on the fresh receipt and its threshold through `GatePolicy.SpeedupFloor`; missing reference evidence is a regression, never an implicit pass.
- Law: a divergent family field re-cuts at its family root instead of surviving as a sibling grammar — `Corpus` entered the receipt because the Bim family binds claims to input identity, and `ReferenceEvidence` entered because `BenchClaim` binds vectorized and reference lanes.

| [INDEX] | [FAMILY]           | [RECEIPT_PROJECTION]                                                             |
| :-----: | :----------------- | :------------------------------------------------------------------------------- |
|  [01]   | `BenchClaim`       | `Claim` → case; lanes → fresh/reference cases; `SpeedupFloor` → policy           |
|  [02]   | `BimBenchReceipt`  | claim → case; corpus fingerprint → corpus; all four measurements map directly    |
|  [03]   | `FabricationBench` | `Suite` + `Key`; harness result supplies measurements; corpus is absent          |
|  [04]   | `BenchEvidence`    | operation → case; batch duration → median/p95; allocation and host map directly  |
|  [05]   | `BenchmarkRow`     | key splits suite/case; custody keeps measures, corpus, artifact, host, and route |

Grasshopper's `BudgetBreach` carries `Bound`, but `CaptureBreach` omits the two-period bound that produced it. Its row remains blocked from settled ingestion until the owner carries that value; the receipt adapter never re-derives a threshold from capture pace.

## [06]-[RESEARCH]

- [BENCHMARK_EXPORTER_MEMBER]-[BLOCKED]: Which exact BenchmarkDotNet exporter member writes the run artifact whose key `BenchmarkReceipt.ArtifactKey` carries? Route: `libs/csharp/Rasm.AppHost/.api/`, then `libs/csharp/.api/`, package `BenchmarkDotNet`; keep `ArtifactKey = None` until one tier carries the exact row.
- [PROFILE_SAMPLE_CAPTURE]-[BLOCKED]: Which exact `SampleProfilerTraceEventParser` subscription member and sample stack/frame accessors project `EventPipeEventSource` events into `ProfileSample` rows before `ProfileFeed.Publish`? Route: `libs/csharp/Rasm.AppHost/.api/api-traceevent.md`, then `libs/csharp/.api/`, package `Microsoft.Diagnostics.Tracing.TraceEvent`; keep the capture producer out of settled code until one tier carries every declaration.
- [CLAIM_FAMILY_ADMISSION]-[BLOCKED]: Which owner changes complete the Materials and Grasshopper claim projections? Route Materials through `libs/csharp/Rasm.Materials/.api/api-rasm-apphost.md`, then `libs/csharp/.api/api-rasm-apphost.md`, until every AppHost benchmark member named by `libs/csharp/Rasm.Materials/IDEAS.md#[KERNEL_BENCH_PROFILE_CORPUS]` is catalogued; route Grasshopper through `libs/csharp/Rasm.Grasshopper/.planning/Shell/telemetry.md` until `CaptureBreach` carries its producing two-period bound. Keep both families out of settled ingestion until their owner route closes.
