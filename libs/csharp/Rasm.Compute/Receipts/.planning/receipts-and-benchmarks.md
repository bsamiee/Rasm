# [COMPUTE_RECEIPTS_AND_BENCHMARKS]

One fifteen-case `ComputeReceipt` union is the package's only fact vocabulary for measured execution: every operational view derives as a fold over that stream, NodaTime-protobuf bridges own the instant, duration, and calendar wire edge, and fingerprint-gated benchmark claims decide every performance-motivated route in the suite. The page owns the receipt union with its Strict-resolver round-trip emission surface, the fold-projection family, the wire-stamp bridges, the claim table with its host fingerprint, and `ComputeWireContext` — composing `ReceiptSinkPort`, `ReceiptEnvelope`, `ClockPolicy`, `ScheduleEntry`, `TelemetryContributorPort`, and the Persistence benchmark and artifact index contracts as settled vocabulary.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]        | [OWNS]                                                        |
| :-----: | :--------------- | :------------------------------------------------------------ |
|   [1]   | RECEIPT_UNION    | Fifteen-case fact union; one wire context; sink-port emission |
|   [2]   | FOLD_PROJECTIONS | Operational views derive as folds over the fact stream        |
|   [3]   | WIRE_STAMPS      | NodaTime-protobuf bridges own the temporal wire edge          |
|   [4]   | BENCHMARK_CLAIMS | Fingerprint-gated claim rows decide performance routes        |
|   [5]   | TS_PROJECTION    | Receipt payload union and benchmark-claim wire shapes         |

## [2]-[RECEIPT_UNION]

- Owner: `ComputeReceipt`, `ComputeWireContext`, `ReceiptSurface` — the fact union, the package's one `JsonSerializerContext` partial joining the suite Strict resolver merge, and the emission-plus-telemetry surface.
- Cases: selection · tensor-run · model-load · warmup · model-run · remote-call · stream-segment · allocation · cache · unit-projection · backpressure · drain · conflict · factorization · generate · discretization · solve · optimization · sweep · clash · twin
- Entry: `public IO<ReceiptEnvelope> Emit(ReceiptSinkPort sink, JsonSerializerOptions wire)` — `IO` carries the sink effect; the returned envelope is the emission evidence.
- Auto: the wire kind derives from the polymorphic metadata pinned on the union, the HLC stamp and `SkewBound` derive inside `Send`, the ambient `TenantContext.Current` threads into `Send` so the envelope `Tenant` field partitions evidence by the boot-minted tenancy primitive AppHost owns — Compute reads `TenantContext` as settled vocabulary and never re-mints it, and the instrument rows register once at composition through `TelemetryContributorPort` — `TelemetrySource.Compute` mints the activity spine so receipt correlation joins the OTel rail with zero call-site ceremony; `ComputeWireContext` joins the suite Strict resolver merge so the polymorphic `kind` discriminator round-trips through one shared `JsonSerializerOptions`, every Thinktecture spine field crosses as its key scalar through the merged Thinktecture resolver, and the union deserializes back to the exact case with `Seq<string>` collections intact — `UnmappedMemberHandling.Disallow` rejects any drifted field at the consuming edge rather than dropping it.
- Receipt: union cases materialize at the sink edge only; hot-path capsules upstream stay allocation-free and the envelope is the sole cross-process causal carrier.
- Packages: Thinktecture.Runtime.Extensions, Thinktecture.Runtime.Extensions.Json, LanguageExt.Core, NodaTime, Rasm.AppHost (project), BCL inbox
- Growth: a new measured concern is one case row on `ComputeReceipt` plus one `[JsonDerivedType]` row and one TS payload row, zero new surface.
- Boundary: receipts are process-local and HLC-correlated, never globally shared — the envelope stamp is the only cross-process causal primitive; a generic IReceipt/ledger abstraction, a second correlation or HLC stamp, and emission bypassing the sink port are the rejected forms; the `Conflict` case is the receipt complement of the wire fault projection, carrying retry-owner and contract-checksum evidence; the `Factorization` case carries the numeric-lane dense and sparse solve evidence (provider, decomposition kind, dimensions, nnz, storage format) and the `Generate` case the generative-run evidence (model checksum, EP, model type, token count, tokens-per-second, `GuidanceKind` dimension, constrained-token count, tool-call count) — a `SolveReceipt`, `GenerationReceipt`, or `BatchReceipt` standalone type is the rejected form, a batch result is a `Seq<ComputeReceipt>` under one correlation tag and an `Embed` run rides the `Generate` family slot; the `Discretization`/`Solve`/`Optimization`/`Sweep`/`Clash`/`Twin` cases carry the solver-and-optimization lane evidence (mesh algorithm + element + node/element counts + worst-element quality; physics + method + DOF count + iteration count + residual + converged flag; optimizer + generation/evaluation/surrogate-hit counts + front size + hypervolume; grid-point + completed + front/dominated split; index kind + candidate + hard/clearance counts; signal id + predicted-versus-measured residual + anomaly flag + control delta) — a `MeshReceipt`, `OptimizerReceipt`, `ClashReceipt`, or `TwinReceipt` standalone type is the rejected form, every solver outcome rides the one union under its correlation; `TensorRun.Partitions` carries the `ParallelHelper` partition count so a partitioned kernel is operator-attributable on the existing receipt; spine fields serialize as Thinktecture key scalars and the format members make every receipt span-writable without runtime format strings.

```csharp signature
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
[JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
[JsonDerivedType(typeof(Selection), "selection")]
[JsonDerivedType(typeof(TensorRun), "tensor-run")]
[JsonDerivedType(typeof(ModelLoad), "model-load")]
[JsonDerivedType(typeof(Warmup), "warmup")]
[JsonDerivedType(typeof(ModelRun), "model-run")]
[JsonDerivedType(typeof(RemoteCall), "remote-call")]
[JsonDerivedType(typeof(StreamSegment), "stream-segment")]
[JsonDerivedType(typeof(Allocation), "allocation")]
[JsonDerivedType(typeof(Cache), "cache")]
[JsonDerivedType(typeof(UnitProjection), "unit-projection")]
[JsonDerivedType(typeof(Backpressure), "backpressure")]
[JsonDerivedType(typeof(Drain), "drain")]
[JsonDerivedType(typeof(Conflict), "conflict")]
[JsonDerivedType(typeof(Factorization), "factorization")]
[JsonDerivedType(typeof(Generate), "generate")]
[JsonDerivedType(typeof(Discretization), "discretization")]
[JsonDerivedType(typeof(Solve), "solve")]
[JsonDerivedType(typeof(Optimization), "optimization")]
[JsonDerivedType(typeof(Sweep), "sweep")]
[JsonDerivedType(typeof(Clash), "clash")]
[JsonDerivedType(typeof(Twin), "twin")]
public abstract partial record ComputeReceipt : ISpanFormattable, IUtf8SpanFormattable {
    private ComputeReceipt() { }

    public required CorrelationId Correlation { get; init; }

    public required WorkLane Lane { get; init; }

    public required Substrate Substrate { get; init; }

    public required AllocationClass AllocationClass { get; init; }

    public required Duration Elapsed { get; init; }

    public sealed record Selection(Seq<string> Evaluated, Seq<string> Rejected, int FallbackHops, Substrate? Forced) : ComputeReceipt;

    public sealed record TensorRun(TensorOpFamily Family, string Dtype, long Elements, string SimdWidth, int Partitions) : ComputeReceipt;

    public sealed record ModelLoad(string ModelChecksum, string Source, ExecutionProvider Ep, int Opset) : ComputeReceipt;

    public sealed record Warmup(string ModelChecksum, ExecutionProvider Ep, string Shape) : ComputeReceipt;

    public sealed record ModelRun(string ModelChecksum, ExecutionProvider Ep, string Mode, int BatchSize, long PeakBytes, string? ArenaAllocator, string? ProfileArtifact) : ComputeReceipt;

    public sealed record RemoteCall(string Transport, string Method, string Status, long RequestBytes, long ResponseBytes, DeadlineOutcome Outcome) : ComputeReceipt;

    public sealed record StreamSegment(string ArtifactId, int Segments, long Bytes) : ComputeReceipt;

    public sealed record Allocation(string Event, long Bytes, string? Reason) : ComputeReceipt;

    public sealed record Cache(string Outcome, string Key, long Bytes) : ComputeReceipt;

    public sealed record UnitProjection(string Family, string OriginalUnit, double OriginalValue, double CanonicalValue) : ComputeReceipt;

    public sealed record Backpressure(int QueueDepth, Duration Waited, string? Dropped) : ComputeReceipt;

    public sealed record Drain(int Drained, int Faulted, int Refused) : ComputeReceipt;

    public sealed record Conflict(string Subject, string Evidence) : ComputeReceipt;

    public sealed record Factorization(string Provider, string Decomposition, int Rows, int Cols, long Nnz, string Format) : ComputeReceipt;

    public sealed record Generate(string ModelChecksum, ExecutionProvider Ep, string ModelType, int Tokens, double TokensPerSecond, string GuidanceKind, int ConstrainedTokens, int ToolCalls) : ComputeReceipt;

    public sealed record Discretization(string Algorithm, string Element, long Nodes, long Elements, int BoundaryLayers, int RefineLevel, double WorstQuality) : ComputeReceipt;

    public sealed record Solve(string Physics, string Method, long Dofs, int Iterations, double Residual, bool Converged) : ComputeReceipt;

    public sealed record Optimization(string Optimizer, int Generations, int Evaluations, int SurrogateHits, int FrontSize, double Hypervolume) : ComputeReceipt;

    public sealed record Sweep(long GridPoints, int Completed, int OnFront, int Dominated) : ComputeReceipt;

    public sealed record Clash(string IndexKind, int Candidates, int HardClashes, int ClearanceViolations, int TotalPairs) : ComputeReceipt;

    public sealed record Twin(string SignalId, double Predicted, double Measured, double Residual, bool Anomaly, double ControlDelta) : ComputeReceipt;

    public string ToString(string? format, IFormatProvider? formatProvider) => ToString();

    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider) =>
        destination.TryWrite($"{Correlation}:{Lane}:{Substrate}:{Elapsed}", out charsWritten);

    public bool TryFormat(Span<byte> utf8Destination, out int bytesWritten, ReadOnlySpan<char> format, IFormatProvider? provider) =>
        Utf8.TryWrite(utf8Destination, $"{Correlation}:{Lane}:{Substrate}:{Elapsed}", out bytesWritten);
}

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
    RespectNullableAnnotations = true,
    RespectRequiredConstructorParameters = true)]
[JsonSerializable(typeof(ComputeReceipt))]
[JsonSerializable(typeof(BenchmarkClaim))]
[JsonSerializable(typeof(HostFingerprint))]
public partial class ComputeWireContext : JsonSerializerContext;

public static class ReceiptSurface {
    public static readonly Seq<InstrumentRow> Instruments = Seq(
        new InstrumentRow(TelemetrySource.Compute, "rasm.compute.receipts.emitted", "{receipt}", "receipts emitted through the sink port",
            static (meter, name, unit, text) => meter.CreateCounter<long>(name, unit, text)),
        new InstrumentRow(TelemetrySource.Compute, "rasm.compute.claims.bound", "{claim}", "benchmark claims bound by fingerprint match",
            static (meter, name, unit, text) => meter.CreateCounter<long>(name, unit, text)),
        new InstrumentRow(TelemetrySource.Compute, "rasm.compute.solve.factorizations", "{factorization}", "dense and sparse factorizations by provider and kind",
            static (meter, name, unit, text) => meter.CreateCounter<long>(name, unit, text)),
        new InstrumentRow(TelemetrySource.Compute, "rasm.compute.solve.residual", "1", "iterative-solver convergence residual",
            static (meter, name, unit, text) => meter.CreateHistogram<double>(name, unit, text)),
        new InstrumentRow(TelemetrySource.Compute, "rasm.compute.solve.shards", "{shard}", "sharded solve sub-blocks by node",
            static (meter, name, unit, text) => meter.CreateCounter<long>(name, unit, text)),
        new InstrumentRow(TelemetrySource.Compute, "rasm.compute.generate.tokens", "{token}", "tokens emitted through the generative run loop",
            static (meter, name, unit, text) => meter.CreateCounter<long>(name, unit, text)),
        new InstrumentRow(TelemetrySource.Compute, "rasm.compute.solve.iterations", "{iteration}", "iterative-solve iteration counts to convergence or frame-budget stop",
            static (meter, name, unit, text) => meter.CreateHistogram<long>(name, unit, text)),
        new InstrumentRow(TelemetrySource.Compute, "rasm.compute.optimize.hypervolume", "1", "Pareto-front hypervolume indicator per optimizer generation",
            static (meter, name, unit, text) => meter.CreateHistogram<double>(name, unit, text)),
        new InstrumentRow(TelemetrySource.Compute, "rasm.compute.mesh.elements", "{element}", "volumetric elements generated per discretization and refinement level",
            static (meter, name, unit, text) => meter.CreateCounter<long>(name, unit, text)),
        new InstrumentRow(TelemetrySource.Compute, "rasm.compute.clash.confirmed", "{clash}", "confirmed hard and clearance clashes by federated-index kind",
            static (meter, name, unit, text) => meter.CreateCounter<long>(name, unit, text)),
        new InstrumentRow(TelemetrySource.Compute, "rasm.compute.twin.anomalies", "{anomaly}", "digital-twin anomaly verdicts exceeding the ROM error bound",
            static (meter, name, unit, text) => meter.CreateCounter<long>(name, unit, text)));

    public static TelemetryContributorPort Telemetry(string version) => new(TelemetrySource.Compute, version, Instruments);

    extension(ComputeReceipt fact) {
        public IO<ReceiptEnvelope> Emit(ReceiptSinkPort sink, JsonSerializerOptions wire) =>
            IO.lift(() => JsonSerializer.SerializeToElement(fact, typeof(ComputeReceipt), wire))
                .Bind(payload => sink.Send(fact.Correlation, TenantContext.Current, "Rasm.Compute", payload.GetProperty("kind").ToString(), payload));
    }
}
```

## [3]-[FOLD_PROJECTIONS]

- Owner: `ReceiptFolds` — every operational view is a pure fold over `Seq<ComputeReceipt>`; the fact stream is the single source and no projection accumulates mutably.
- Entry: `public HashMap<CorrelationId, Seq<ComputeReceipt>> Provenance` — the model-result provenance projection joining every receipt chain by correlation.
- Auto: per-lane counts, route histograms, hot-path totals, leak indicators, conflict evidence, and provenance chains derive on read from the identical stream the dashboards consume.
- Packages: LanguageExt.Core, NodaTime, BCL inbox
- Growth: a new operational view is one fold member row over the same fact stream, zero new surface.
- Boundary: leak indicators read the double-dispose and finalized allocation events as diagnostics, never log noise; `DiscardTaxonomy` folds the stream-pool `BufferDiscardedEventArgs.Reason` carried on the discard `Allocation.Reason` field into a reason-keyed count so an `EnoughFree`-dominant tally reads as caps-below-workload and an oversize-dominant tally as payloads-exceeding-`MaximumBufferSize` — opposite tuning moves separated by the fold, never a merged counter; `Conflicts` surfaces retry-owner and contract-checksum evidence toward the operator; a mutable accumulator, a per-view repository, and a second fact stream are the deleted forms.

```csharp signature
public static class ReceiptFolds {
    extension(Seq<ComputeReceipt> facts) {
        public HashMap<WorkLane, int> LaneCounts =>
            facts.Fold(HashMap<WorkLane, int>(), static (acc, fact) => acc.AddOrUpdate(fact.Lane, static n => n + 1, 1));

        public HashMap<Substrate, int> RouteHistogram =>
            facts.Fold(HashMap<Substrate, int>(), static (acc, fact) => acc.AddOrUpdate(fact.Substrate, static n => n + 1, 1));

        public HashMap<Substrate, Duration> HotPathTotals =>
            facts.Fold(HashMap<Substrate, Duration>(), static (acc, fact) => acc.AddOrUpdate(fact.Substrate, total => total + fact.Elapsed, fact.Elapsed));

        public Seq<ComputeReceipt.Allocation> Leaks =>
            facts.Bind(static fact => fact is ComputeReceipt.Allocation { Event: "double-dispose" or "finalized" } leak
                ? Seq(leak)
                : Seq<ComputeReceipt.Allocation>());

        public HashMap<string, int> DiscardTaxonomy =>
            facts.Fold(HashMap<string, int>(), static (acc, fact) =>
                fact is ComputeReceipt.Allocation { Event: "discard", Reason: { } reason }
                    ? acc.AddOrUpdate(reason, static n => n + 1, 1)
                    : acc);

        public Seq<ComputeReceipt.Conflict> Conflicts =>
            facts.Bind(static fact => fact is ComputeReceipt.Conflict conflict ? Seq(conflict) : Seq<ComputeReceipt.Conflict>());

        public HashMap<CorrelationId, Seq<ComputeReceipt>> Provenance =>
            facts.Fold(HashMap<CorrelationId, Seq<ComputeReceipt>>(), static (acc, fact) => acc.AddOrUpdate(fact.Correlation, chain => chain.Add(fact), Seq(fact)));
    }
}
```

## [4]-[WIRE_STAMPS]

- Owner: `WireStamps` — the NodaTime-protobuf bridge family is the only temporal crossing between the receipt rail and the proto wire, spanning the well-known `Timestamp`/`Duration` edge and the `Google.Type` calendar-date and time-of-day edge.
- Entry: `public Timestamp WirePhysical` — the envelope's HLC physical stamp projected onto the well-known wire type.
- Packages: NodaTime.Serialization.Protobuf, Google.Protobuf, NodaTime, Rasm.AppHost (project)
- Growth: a new temporal wire field is one extension row over the bridge family, zero new surface.
- Boundary: BCL `DateTime` never appears between wire and rail; pre-epoch instants and out-of-window durations are boundary faults the call edge projects onto the typed rail, and an invalid time-of-day or out-of-range day-of-week is the same boundary fault on the calendar edge; the desktop and the web dashboard consume the identical receipt stream — envelope JSON and the proto contracts are two encodings of one stream — and `SkewBound` surfaces in the evidence view with zero configuration; a sweep occurrence's calendar date, time-of-day, and day-of-week cross as `Google.Type.Date`/`TimeOfDay`/`DayOfWeek` so a dashboard groups runs by calendar day and weekday without re-deriving the instant, and `ToBase64` projects a frame or artifact checksum onto a text-safe field for the evidence view without a second hashing pass.

```csharp signature
public static class WireStamps {
    extension(ReceiptEnvelope envelope) {
        public Timestamp WirePhysical => envelope.Physical.ToTimestamp();

        public Google.Protobuf.WellKnownTypes.Duration WireSkew => envelope.SkewBound.ToProtobufDuration();
    }

    extension(LocalDate date) {
        public Google.Type.Date WireDate => date.ToDate();
    }

    extension(LocalTime time) {
        public Google.Type.TimeOfDay WireTimeOfDay => time.ToTimeOfDay();
    }

    extension(IsoDayOfWeek day) {
        public Google.Type.DayOfWeek WireDayOfWeek => day.ToProtobufDayOfWeek();
    }

    extension(ByteString checksum) {
        public string Base64 => checksum.ToBase64();
    }

    extension(Timestamp wire) {
        public Instant Rail => wire.ToInstant();
    }

    extension(Google.Protobuf.WellKnownTypes.Duration wire) {
        public Duration Rail => wire.ToNodaDuration();
    }

    extension(Google.Type.Date wire) {
        public LocalDate RailDate => wire.ToLocalDate();
    }

    extension(Google.Type.TimeOfDay wire) {
        public LocalTime RailTime => wire.ToLocalTime();
    }

    extension(Google.Type.DayOfWeek wire) {
        public IsoDayOfWeek RailDayOfWeek => wire.ToIsoDayOfWeek();
    }
}
```

## [5]-[BENCHMARK_CLAIMS]

- Owner: `BenchmarkClaim`, `HostFingerprint` — the input-class claim row and the host identity that gates it; a claim is a row, never prose.
- Cases: micro · small · medium · large payload-size bands, declared as the `Bands` rows.
- Entry: `public Option<BenchmarkRow> Claim(Seq<BenchmarkRow> rows)` — `Option` carries fingerprint admission; `None` is the fall-through to the static cost rank on the substrate row.
- Auto: `BandOf` classifies payload bytes onto the band rows; `Persist` projects the claim onto the persisted benchmark row; `Stale` derives from stamp drift so any admitted-package bump marks dependent claims stale as operator-visible evidence; `Sweep` registers the compute-equivalence-sweep cadence row whose runs execute on the `WorkLane.Benchmark` row.
- Receipt: every sweep run emits `TensorRun`/`ModelRun` receipts beside the persisted row; artifacts — chrome-trace profiles, BenchmarkDotNet exports, EP-context caches — land as `ArtifactIndexRow` paths on the blob lane and ride the `Artifacts` rows on the claim.
- Packages: BenchmarkDotNet, NodaTime, LanguageExt.Core, Rasm.AppHost (project), Rasm.Persistence (project), BCL inbox
- Growth: a new performance surface is one claim row; a new claim dimension is one column on `BenchmarkClaim`; zero new surface.
- Boundary: the claim-gate law — a SIMD default route, compression enable, ParallelHelper partitioning, a DATAS knob, or a numeric BLAS provider rank (managed vs native-openblas) binds only behind a winning claim row whose fingerprint matches the running host; the `Provider` column carries the winning numeric-lane `LinearProvider` key so a native-BLAS selection is fingerprint-gated exactly like a SIMD route and the `Substrate` column stays the substrate discriminant; the `LinearProvider.DeterminismTag` (active `ILinearAlgebraProvider` type plus degree-of-parallelism) enters the `HostFingerprint.Stamps` map as a `blas-provider` stamp, so a provider swap or a degree-of-parallelism change marks dependent claims `Stale` through the same `StampLine` comparison that catches an admitted-package bump — a benchmark claimed under managed never wins on a host that resolved native-MKL because the determinism tag drifts the fingerprint, and the provider tag is the bridge between the benchmark gate and the numeric-lane `SolveDedupKey` so the speed claim and the result-cache key partition on the same provider identity; tolerance classes arrive settled from the operation-family rows and loosening one to pass equivalence is the named production-slack defect; `Claim` resolves the most-recent fingerprint-matching `BenchmarkRow` against the recency horizon read by reference from the Persistence `ModelResultKey` index owner — the single horizon owner — so a stale benchmark never wins and Compute never mints a second `Duration horizon` beside the claim; a second benchmark store, BenchmarkDotNet profiler add-ons, and prose performance claims are the rejected forms.

```csharp signature
public sealed record HostFingerprint(string Os, string Arch, int Processors, FrozenDictionary<string, string> Stamps) : ISpanFormattable, IUtf8SpanFormattable {
    public static HostFingerprint Current(FrozenDictionary<string, string> stamps) =>
        new(RuntimeInformation.OSDescription, RuntimeInformation.ProcessArchitecture.ToString(), Environment.ProcessorCount, stamps);

    public Option<BenchmarkRow> Claim(Seq<BenchmarkRow> rows) => BenchmarkRow.Claim(rows, ToString());

    public string StampLine() =>
        string.Join(',', Stamps.OrderBy(static pair => pair.Key, StringComparer.Ordinal).Select(static pair => $"{pair.Key}={pair.Value}"));

    public override string ToString() => string.Create(CultureInfo.InvariantCulture, $"{Os}|{Arch}|{Processors}|{StampLine()}");

    public string ToString(string? format, IFormatProvider? formatProvider) => ToString();

    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider) =>
        destination.TryWrite($"{Os}|{Arch}|{Processors}|{StampLine()}", out charsWritten);

    public bool TryFormat(Span<byte> utf8Destination, out int bytesWritten, ReadOnlySpan<char> format, IFormatProvider? provider) =>
        Utf8.TryWrite(utf8Destination, $"{Os}|{Arch}|{Processors}|{StampLine()}", out bytesWritten);
}

public sealed record BenchmarkClaim(
    string Band,
    string Dtype,
    Substrate Substrate,
    string Family,
    string Route,
    string Provider,
    Duration Median,
    Duration P95,
    long AllocatedBytes,
    double EquivalenceMaxDeviation,
    string ToleranceClass,
    HostFingerprint Fingerprint,
    Seq<string> Artifacts,
    Instant At) {
    public static readonly Seq<(string Band, long MaxBytes)> Bands = Seq(
        ("micro", 4L << 10),
        ("small", 256L << 10),
        ("medium", 16L << 20),
        ("large", long.MaxValue));

    public static string BandOf(long payloadBytes) =>
        Bands.Find(row => payloadBytes <= row.MaxBytes).Map(static row => row.Band).IfNone("large");

    public static ScheduleEntry Sweep(Func<IO<Unit>> work) =>
        new("compute-equivalence-sweep", new OccurrenceSpec.Every(Duration.FromDays(7)), DeadlineClass.SupportWindow, None, work);

    public string Key() => string.Create(CultureInfo.InvariantCulture, $"{Band}|{Dtype}|{Substrate}|{Family}|{Provider}");

    public BenchmarkRow Persist() => new(Key(), Route, Median, P95, AllocatedBytes, Fingerprint.ToString(), BenchmarkRow.BenchmarkRowClass, At);

    public bool Stale(HostFingerprint current) => !StringComparer.Ordinal.Equals(Fingerprint.StampLine(), current.StampLine());
}
```

## [6]-[TS_PROJECTION]

- Owner: `ComputeReceiptKind`, `ComputeReceiptSpineWire`, `ComputeReceiptWire`, `ComputeReceiptEnvelopeWire`, `HostFingerprintWire`, `BenchmarkClaimWire` — the receipt payload union and the claim row as the dashboard consumes them.
- Packages: BCL inbox
- Growth: a new receipt case lands as one payload row on `ComputeReceiptWire`, zero new surface.
- Boundary: payloads bind as `TPayload` through `ReceiptEnvelopeWire` with the envelope `kind` mirroring the payload discriminator; smart-enum spine fields cross as their key scalars; `Instant` and `Duration` values cross as ISO-8601 and roundtrip-pattern strings; absent evidence crosses as explicit null, never as an omitted member.

```ts contract
type ComputeReceiptKind =
  | "selection" | "tensor-run" | "model-load" | "warmup" | "model-run" | "remote-call"
  | "stream-segment" | "allocation" | "cache" | "unit-projection" | "backpressure" | "drain" | "conflict"
  | "factorization" | "generate"
  | "discretization" | "solve" | "optimization" | "sweep" | "clash" | "twin";

interface ComputeReceiptSpineWire { kind: ComputeReceiptKind; correlation: string; lane: string; substrate: string; allocationClass: string; elapsed: string; }

interface SelectionWire extends ComputeReceiptSpineWire { kind: "selection"; evaluated: string[]; rejected: string[]; fallbackHops: number; forced: string | null; }

interface TensorRunWire extends ComputeReceiptSpineWire { kind: "tensor-run"; family: string; dtype: string; elements: number; simdWidth: string; partitions: number; }

interface ModelLoadWire extends ComputeReceiptSpineWire { kind: "model-load"; modelChecksum: string; source: string; ep: string; opset: number; }

interface WarmupWire extends ComputeReceiptSpineWire { kind: "warmup"; modelChecksum: string; ep: string; shape: string; }

interface ModelRunWire extends ComputeReceiptSpineWire { kind: "model-run"; modelChecksum: string; ep: string; mode: string; batchSize: number; peakBytes: number; arenaAllocator: string | null; profileArtifact: string | null; }

interface RemoteCallWire extends ComputeReceiptSpineWire { kind: "remote-call"; transport: string; method: string; status: string; requestBytes: number; responseBytes: number; outcome: string; }

interface StreamSegmentWire extends ComputeReceiptSpineWire { kind: "stream-segment"; artifactId: string; segments: number; bytes: number; }

interface AllocationWire extends ComputeReceiptSpineWire { kind: "allocation"; event: string; bytes: number; reason: string | null; }

interface CacheWire extends ComputeReceiptSpineWire { kind: "cache"; outcome: "hit" | "miss" | "store" | "evict"; key: string; bytes: number; }

interface UnitProjectionWire extends ComputeReceiptSpineWire { kind: "unit-projection"; family: string; originalUnit: string; originalValue: number; canonicalValue: number; }

interface BackpressureWire extends ComputeReceiptSpineWire { kind: "backpressure"; queueDepth: number; waited: string; dropped: string | null; }

interface DrainWire extends ComputeReceiptSpineWire { kind: "drain"; drained: number; faulted: number; refused: number; }

interface ConflictWire extends ComputeReceiptSpineWire { kind: "conflict"; subject: "retry-owner" | "contract-checksum"; evidence: string; }

interface FactorizationWire extends ComputeReceiptSpineWire { kind: "factorization"; provider: string; decomposition: string; rows: number; cols: number; nnz: number; format: string; }

interface GenerateWire extends ComputeReceiptSpineWire { kind: "generate"; modelChecksum: string; ep: string; modelType: string; tokens: number; tokensPerSecond: number; guidanceKind: string; constrainedTokens: number; toolCalls: number; }

interface DiscretizationWire extends ComputeReceiptSpineWire { kind: "discretization"; algorithm: string; element: string; nodes: number; elements: number; boundaryLayers: number; refineLevel: number; worstQuality: number; }

interface SolveWire extends ComputeReceiptSpineWire { kind: "solve"; physics: string; method: string; dofs: number; iterations: number; residual: number; converged: boolean; }

interface OptimizationWire extends ComputeReceiptSpineWire { kind: "optimization"; optimizer: string; generations: number; evaluations: number; surrogateHits: number; frontSize: number; hypervolume: number; }

interface SweepWire extends ComputeReceiptSpineWire { kind: "sweep"; gridPoints: number; completed: number; onFront: number; dominated: number; }

interface ClashWire extends ComputeReceiptSpineWire { kind: "clash"; indexKind: "bvh" | "octree" | "sdf"; candidates: number; hardClashes: number; clearanceViolations: number; totalPairs: number; }

interface TwinWire extends ComputeReceiptSpineWire { kind: "twin"; signalId: string; predicted: number; measured: number; residual: number; anomaly: boolean; controlDelta: number; }

type ComputeReceiptWire =
  | SelectionWire | TensorRunWire | ModelLoadWire | WarmupWire | ModelRunWire | RemoteCallWire | StreamSegmentWire
  | AllocationWire | CacheWire | UnitProjectionWire | BackpressureWire | DrainWire | ConflictWire | FactorizationWire | GenerateWire
  | DiscretizationWire | SolveWire | OptimizationWire | SweepWire | ClashWire | TwinWire;

type ComputeReceiptEnvelopeWire = ReceiptEnvelopeWire<ComputeReceiptWire>;

interface HostFingerprintWire { os: string; arch: string; processors: number; stamps: Record<string, string>; }

interface BenchmarkClaimWire { band: "micro" | "small" | "medium" | "large"; dtype: string; substrate: string; family: string; route: string; provider: string; median: string; p95: string; allocatedBytes: number; equivalenceMaxDeviation: number; toleranceClass: string; fingerprint: HostFingerprintWire; artifacts: string[]; at: string; }
```

## [7]-[RESEARCH]

- [WIRE_EMISSION]: the implementation-time round-trip confirms the merged resolver chain orders `ComputeWireContext` after the Thinktecture key-scalar resolver so the polymorphic `kind` discriminator and every smart-enum spine field serialize and deserialize to the identical case, and that `Seq<string>` collection members on the `Selection` case survive the merge without a `JsonConverter` for the LanguageExt collection.
- [CALENDAR_BRIDGE]: the `Google.Type.Date`/`TimeOfDay`/`DayOfWeek` calendar wire types and the `Google.Api.CommonProtos` assembly that carries them, with the `ToDate`/`ToLocalDate`/`ToTimeOfDay`/`ToLocalTime`/`ToProtobufDayOfWeek`/`ToIsoDayOfWeek` conversion members the calendar-bridge wire-stamp columns bind.
