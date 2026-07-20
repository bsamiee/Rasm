# [APPHOST_BENCHMARK_RAIL]

One `BenchmarkReceipt` family folds every benchmark outcome into the receipt fan: the corpus benchmark gate the kernel and compute pages cite anchors here as one typed pass-or-regress fold, on-demand deep capture rides the support-bundle contributor seam, and every gated run executes inside a minted span so the profiling rail links its flame graph to the exact case that regressed. A speed claim anywhere in the C# corpus resolves to a `BenchmarkReceipt` this rail stamped.

Settled composition: `ReceiptSinkPort`, `ReceiptEnvelope`, and the `AppHostWireContext` roster row from Runtime/ports; `SupportArtifact.EventTrace` and `SupportContributorPort` from Observability/bundles#CAPTURE_PIPELINE; `TelemetryIdentity.Mint` and the `PyroscopeSpanProcessor` profile linkage from Observability/telemetry; `FaultBand.Benchmark` from Runtime/lifecycle#FAULT_TABLES. BenchmarkDotNet binds in the branch test and benchmark projects per the Test Stack manifest tier, never this package's csproj; the durable claim store the gate compares against is the Persistence reuse index, read by reference.

## [01]-[INDEX]

- [01]-[BENCHMARK_RECEIPT]: Receipt family, host evidence, and the corpus-gate fold.
- [02]-[CAPTURE_SEAM]: Deep-capture contributor rows riding the support-bundle fan.
- [03]-[PROFILE_CORRELATION]: Span-wrapped runs feeding the continuous-profiling linkage.

## [02]-[BENCHMARK_RECEIPT]

- Owner: `BenchmarkReceipt` — the typed run evidence; `BenchmarkVerdict` `[SmartEnum<string>]` the gate-disposition vocabulary; `BenchmarkFault` `[Union]` deriving through `FaultBand.Benchmark`; `BenchmarkGate` — the corpus pass-or-regress fold.
- Cases: `BenchmarkVerdict` = Unjudged | Pass | Regressed | HostMismatch; `BenchmarkFault` = Text | GateRegressed | HostMismatch.
- Entry: `BenchmarkGate.Gate(ReceiptSinkPort sink, BenchmarkReceipt fresh, Option<BenchmarkReceipt> claim, GatePolicy policy)` — the one gated entry: judges the fresh run against the held claim under the regression budget, stamps the verdict row onto the receipt, fans the judged receipt through the sink under `InstrumentFan.BenchmarkKind`, and returns the gate rail; `BenchmarkGate.Judge(...)` is the pure verdict fold the entry composes.
- Auto: `HostEvidence.Current()` stamps runtime, OS, and processor identity with one digest, so a claim binds only against a matching host and a cross-host comparison faults as `HostMismatch` rather than a phantom regression; the receipt rides the HLC envelope like every spine fact, so benchmark history orders causally with the command log; a regressed run still fans, so the Observability/instruments#RECEIPT_PROJECTION benchmark arm projects duration and regression counts off every verdict, never the passing subset alone.
- Receipt: `BenchmarkReceipt` — suite, case, host evidence, median and p95 wall duration, allocated bytes, operation count, gate verdict, artifact key of the exported run artifact, correlation.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, System.IO.Hashing, BCL inbox.
- Growth: a new measured axis is one field on the receipt breaking the gate fold at compile time; a new verdict class is one `BenchmarkVerdict` row paired with its `BenchmarkFault` case.
- Boundary: this rail is the corpus benchmark gate's owner — a kernel or compute page citing the BenchmarkDotNet gate cites this fold, and a hand-rolled kernel is admitted only after its receipt defeats the library route under `Judge`; the bench project produces the raw BenchmarkDotNet artifacts and folds each case to one receipt at its edge, so BenchmarkDotNet types never cross into the spine; the durable claim the gate compares against is the Persistence reuse-index row resolved by content fingerprint — measured facts mint here, the claim store persists them, and neither re-derives the other.

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
    Duration Median,
    Duration P95,
    long AllocatedBytes,
    long Operations,
    BenchmarkVerdict Verdict,
    Option<string> ArtifactKey,
    CorrelationId Correlation);

[Union]
public abstract partial record BenchmarkFault : Expected, IValidationError<BenchmarkFault> {
    private BenchmarkFault(string detail, int code) : base(detail, code, None) { }
    public static BenchmarkFault Create(string message) => new Text(message);
    public sealed record Text : BenchmarkFault { public Text(string detail) : base(detail, FaultBand.Benchmark.Code(0)) { } }
    public sealed record GateRegressed : BenchmarkFault { public GateRegressed(string @case, string detail) : base($"{@case}: {detail}", FaultBand.Benchmark.Code(1)) { } }
    public sealed record HostMismatch : BenchmarkFault { public HostMismatch(string @case) : base(@case, FaultBand.Benchmark.Code(2)) { } }
}

public sealed record GatePolicy(double MedianBudget, double AllocationBudget) {
    public static readonly GatePolicy Canonical = new(MedianBudget: 1.10d, AllocationBudget: 1.05d);
}

public static class BenchmarkGate {
    public static Fin<BenchmarkReceipt> Judge(BenchmarkReceipt fresh, Option<BenchmarkReceipt> claim, GatePolicy policy) =>
        claim.Match(
            None: () => Fin.Succ(fresh with { Verdict = BenchmarkVerdict.Pass }),
            Some: held => held.Host.Digest != fresh.Host.Digest
                ? Fin.Fail<BenchmarkReceipt>(new BenchmarkFault.HostMismatch(fresh.Case))
                : fresh.Median <= held.Median * policy.MedianBudget && fresh.AllocatedBytes <= held.AllocatedBytes * policy.AllocationBudget
                    ? Fin.Succ(fresh with { Verdict = BenchmarkVerdict.Pass })
                    : Fin.Fail<BenchmarkReceipt>(new BenchmarkFault.GateRegressed(fresh.Case,
                        $"median {fresh.Median}/{held.Median} allocated {fresh.AllocatedBytes}/{held.AllocatedBytes}")));

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
- Entry: `BenchmarkArtifacts.Contributor(Duration window)` — one `SupportContributorPort` registration at the bench composition root.
- Auto: the row composes the settled `SupportArtifact.EventTrace` factory with the benchmark provider set — the sample profiler and runtime GC/JIT providers — so an on-demand capture during a regressed run lands inside the bundle's caps, redaction, and truncation law with zero new capture machinery.
- Packages: Microsoft.Diagnostics.NETCore.Client, LanguageExt.Core, NodaTime.
- Growth: a new capture depth is one `EventPipeProvider` row in the provider seq; a new artifact kind is one `SupportArtifact` factory row on the bundles owner, contributed here by seam.
- Boundary: the bundle fan owns freeze, redact, cap, and zip — this page contributes rows and never opens a second capture window; `.gcdump` heap capture stays the `dotnet-gcdump` tool boundary the bundles page pins; native frames in the profiled window resolve through the Observability/bundles#CAPTURE_PIPELINE `PerfMapLease` the bench root opens around the gated run, so the flame graph the profile rail links carries jitted-frame symbols.

```csharp signature
public static class BenchmarkArtifacts {
    public static readonly Seq<EventPipeProvider> Providers = Seq(
        new EventPipeProvider("Microsoft-DotNETCore-SampleProfiler", EventLevel.Informational),
        new EventPipeProvider("Microsoft-Windows-DotNETRuntime", EventLevel.Informational, (long)(ClrTraceEventParser.Keywords.GC | ClrTraceEventParser.Keywords.Jit)));

    public static SupportContributorPort Contributor(Duration window) =>
        new("Rasm.AppHost.Benchmarks", Seq(SupportArtifact.EventTrace(Providers, window)));
}
```

## [04]-[PROFILE_CORRELATION]

- Owner: `BenchmarkRun` — the span-wrapped run boundary.
- Entry: `BenchmarkRun.Traced(ActivitySource source, string suite, string @case, Func<Fin<BenchmarkReceipt>> run)` — every gated run executes inside one activity.
- Auto: the activity carries suite and case tags at start so they participate in the sampling verdict, and the Observability/telemetry `PyroscopeSpanProcessor` stamps `pyroscope.profile.id` on the run's root span — a regressed case's flame graph is one click from its receipt, keyed by the shared correlation.
- Packages: LanguageExt.Core, BCL inbox.
- Growth: a new run dimension is one tag row at activity start.
- Boundary: the wrapper composes the minted `Rasm.AppHost` source from `TelemetryIdentity.Mint` — a second `ActivitySource` for benchmarks is the process-static defect the telemetry page forecloses; profile egress stays the service-root Pyroscope seat, so a desktop bench run without a profiler endpoint runs the identical span with the linkage dormant.

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
```

## [05]-[RESEARCH]

- [CLR_KEYWORDS_SPELLING]-[OPEN]: `ClrTraceEventParser.Keywords.GC | Jit` as the runtime-provider keyword mask for the benchmark EventPipe row — exact enum spelling against the admitted TraceEvent assembly; verify in `api-traceevent.md` before the bench root composes the provider seq.
- [ARTIFACT_EXPORT_ROUTE]-[OPEN]: BenchmarkDotNet's exporter surface landing the per-case artifact whose key `BenchmarkReceipt.ArtifactKey` carries — chrome-trace versus full-json export at the bench-project edge; verify against the Test Stack BenchmarkDotNet catalog when the bench project lands.
