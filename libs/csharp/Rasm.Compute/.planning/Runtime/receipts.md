# [COMPUTE_UNION]

`ComputeReceipt` is the package's only fact vocabulary for measured execution: every operational view folds over that one stream, NodaTime-protobuf bridges own the temporal wire edge, fingerprint-gated claims decide every performance-motivated route, and the cost ledger and dashboard descriptor derive from that stream and the one spec roster — no second fact truth, no second panel truth.

Kernel vocabulary arrives whole from the signal capsule — the causal frame, the instrument mechanism, the trace band, the SLO algebra, and the hook vocabulary, each family's member roster owned at the capsule — beside the AppHost `ScheduleEntry` and the settled Persistence benchmark and artifact-index contracts.

Several cases declare elsewhere — `Assessment` on the `Analysis/assessment` spine, `Quadrature` and `Trajectory` on the `Tensor/quadrature` integration lane, `Sampling` on the `Tensor/sampling` low-discrepancy lane — each a `partial` its owning page seats on this union while its `[JsonDerivedType]` row and wire projection stay here, so `ComputeWireContext` round-trips the whole union through the one Strict resolver: polymorphic `kind` discriminator, Thinktecture key scalars, `Seq<string>` intact, `UnmappedMemberHandling.Disallow` refusing drift at the edge.

## [01]-[INDEX]

- [02]-[RECEIPT_UNION]: Fact union (inline cases beside the `Analysis/assessment` and `Tensor/quadrature` partials), its Strict-resolver round-trip context, the instrument roster, and sink-port emission.
- [03]-[TELEMETRY_PROJECTION]: Receipt-to-instrument fan and the dispatch span spine over the kernel trace band.
- [04]-[FOLD_PROJECTIONS]: Operational views derive as folds over the fact stream; content-keyed verdicts re-derive and diff under the determinism tag.
- [05]-[BENCHMARK_CLAIMS]: Fingerprint-gated claim rows decide performance routes.
- [06]-[HOOK_POINTS]: Compute hook roster on the kernel hook capsule — veto, observe, and replay points with subscriber-fault isolation.
- [07]-[TS_PROJECTION]: Receipt payload union, benchmark-claim, descriptor, and chargeback wire shapes.
- [08]-[COST_LEDGER]: Substrate-rated cost pricing, tenant-partitioned ledger folds over the envelope-joined journal, and the content-keyed chargeback egress.
- [09]-[DASHBOARD_DESCRIPTOR]: One kernel board pack over the instrument specs and the reliability objectives whose burn discipline, specs, and verdicts derive from the kernel SLO algebra.

## [02]-[RECEIPT_UNION]

- Owner: `ReceiptScope`, `SelectionDecision`, `SelectionMode`, `ComputeReceipt`, `ComputeWireContext`, `ReceiptSurface` — the scope and selection evidence families, fact union, strict serializer context, and the sink-bound emission-plus-telemetry surface carrying the Compute instrument roster, name vocabulary, and dimension slots over the kernel declaration mechanism.
- Cases: selection · tensor-run · model-load · warmup · model-run · remote-call · stream-segment · allocation · copy · cache · unit-projection · backpressure · drain · conflict · refusal · factorization · generate · embedding · discretization · solve · coupling · optimization · sweep · clash · twin · uncertainty · fit · governor · drift · assessment · quadrature · trajectory (the last three declared as partials on this owner — assessment by `Analysis/assessment`, the integration pair by `Tensor/quadrature`)
- Entry: `public IO<ReceiptEnvelope> ReceiptSurface.Emit(ComputeReceipt fact)` — the surface binds sink, serializer, and mounted-instrument aspects once at composition; `IO` carries the sink effect and returns the envelope evidence.
- Auto: wire kind derives from the polymorphic metadata pinned on the union; the HLC stamp and `SkewBound` derive inside `Send`, and `Emit` reads `TenantContext.Current` exactly once — the same tenant prices the fact through `ComputeInstrumentFan.Charge` and rides into `Send` so the envelope `Tenant` field partitions evidence by the kernel tenancy primitive; instrument rows register once at composition through `TelemetryContributorPort`, which carries the `[10]-[DASHBOARD_DESCRIPTOR]` pack beside them so the composing root proves board and objectives against the set it mounts and this folder ships no probe entry, `Emit` folds every typed fact through the `[03]-[TELEMETRY_PROJECTION]` fan before serialization, and the `[03]-[TELEMETRY_PROJECTION]` `ComputeTraces` spine opens the dispatch span through the kernel `SpanBand` at the admitted `Dispatch` scope so receipt correlation joins the OTel rail with zero call-site ceremony.
- Receipt: union cases materialize at the sink edge only; hot-path capsules upstream stay allocation-free.
- Packages: Thinktecture.Runtime.Extensions, Thinktecture.Runtime.Extensions.Json, Riok.Mapperly (`AllocationMapper` — the one generated evidence lowering under `RequiredMappingStrategy.Both`), LanguageExt.Core, NodaTime, Rasm (project, kernel signal capsule), Rasm.AppHost (project), BCL inbox
- Growth: a new measured concern is one case row on `ComputeReceipt`, one `[JsonDerivedType]` row, one TS payload row, and one `[03]-[TELEMETRY_PROJECTION]` projection arm, zero new surface; a rail in another folder declares its case as a `partial` record on this owner (the `Analysis/assessment` `Assessment` case, the `Tensor/quadrature` `Quadrature`/`Trajectory` pair, the `Tensor/sampling` `Sampling` case) while this owning index keeps the `[JsonDerivedType]` registration and the TS payload row so the polymorphic registry stays single-sited — the `[JsonDerivedType]` roster is the ONE primary correspondence: `ReceiptSurface.Kinds` projects it from the context's polymorphism metadata, the TS `ComputeReceiptKind` union generates from `Kinds` during descriptor emit under the suite schema hash, and `ReceiptSurface.Probe` proves roster-versus-case bijection at boot, so a parallel receipt union, a second discriminator registry, or a hand-maintained TS mirror that can silently go stale is the deleted form; `Specs` is the SECOND primary correspondence — one kernel `InstrumentSpec` row per instrument carrying name, unit, description, kind, measurement form, bounds, and dimension slots, from which the mounted `InstrumentSet` and the `[10]-[DASHBOARD_DESCRIPTOR]` panel projection both derive, so a bucket boundary reachable only inside a bind delegate is the repaired facade and a second panel-truth roster is the deleted form; a folder-local spec record or instrument-kind vocabulary beside it re-mints the mechanism the capsule owns.
- Boundary: receipts are HLC-correlated through the envelope and emit only through the sink-bound `ReceiptSurface`. `ReceiptScope.Execution` carries lane, substrate, allocation, and elapsed evidence, while `Process` carries only correlation and allocation; process facts never fabricate execution context or bypass the union. Every solver, statistical-learning, generative, residency, allocation, governance, and monitor-drift outcome rides this union. `Selection` projects ordered hops onto `SelectionDecision` and forced presence onto `SelectionMode`, avoiding parallel rosters and nullable policy. `Allocation` carries the complete `AllocationEvidence` projection, including typed `StagingEventKind`, requested/granted bytes, detail, allocator, reservation, and pool gauges. `Uncertainty` carries distribution moments, sensitivity indices, interactions, reliability search coordinates, the surrogate-fit calibration pair, and explicit null slots for every column a method does not estimate. `Factorization` optional wire evidence remains case-local, and both it and `Solve` carry the shard census — `Shards` (1 unsharded), `ShardNode` naming the farm node that ran THIS shard, `Merged` marking the one receipt folding shard results — the receipt counterpart of the `Runtime/wire#PROTO_VOCABULARY` `SolveRequest.shard_tile` column the row-block sub-solve dials. `Refusal` is the interior gate's evidence — process-scoped because a refusal precedes execution, minted from the typed `ComputeFault` itself so the reason is the fault's own slug rather than a hand-typed literal, and never a substitute for the rail: the fault still travels, the receipt is what makes it readable. Detail grammar `<slug:payload>` binds at the RAISING SITE, not a fold arm — every `ComputeFault` message leads with a bracketed bounded slug, so a detail without one is a defect at the raiser and lands here as a whole-message reason no panel can group. Spine values serialize as Thinktecture key scalars and format without runtime format strings.

```csharp signature
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
[JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
[JsonDerivedType(typeof(Execution), "execution")]
[JsonDerivedType(typeof(Process), "process")]
public abstract partial record ReceiptScope {
    private ReceiptScope() { }

    public sealed record Execution(
        CorrelationId Correlation,
        WorkLane Lane,
        Substrate Substrate,
        AllocationClass AllocationClass,
        Duration Elapsed) : ReceiptScope;

    public sealed record Process(CorrelationId Correlation, AllocationClass AllocationClass) : ReceiptScope;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
[JsonPolymorphic(TypeDiscriminatorPropertyName = "outcome")]
[JsonDerivedType(typeof(Chosen), "chosen")]
[JsonDerivedType(typeof(Rejected), "rejected")]
public abstract partial record SelectionDecision {
    private SelectionDecision() { }
    public sealed record Chosen(Substrate Row) : SelectionDecision;
    public sealed record Rejected(Substrate Row, string Reason) : SelectionDecision;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
[JsonPolymorphic(TypeDiscriminatorPropertyName = "mode")]
[JsonDerivedType(typeof(Ranked), "ranked")]
[JsonDerivedType(typeof(Forced), "forced")]
public abstract partial record SelectionMode {
    private SelectionMode() { }
    public sealed record Ranked : SelectionMode;
    public sealed record Forced(Substrate Row) : SelectionMode;
}

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
[JsonDerivedType(typeof(Copy), "copy")]
[JsonDerivedType(typeof(Cache), "cache")]
[JsonDerivedType(typeof(UnitProjection), "unit-projection")]
[JsonDerivedType(typeof(Backpressure), "backpressure")]
[JsonDerivedType(typeof(Drain), "drain")]
[JsonDerivedType(typeof(Conflict), "conflict")]
[JsonDerivedType(typeof(Refusal), "refusal")]
[JsonDerivedType(typeof(Factorization), "factorization")]
[JsonDerivedType(typeof(Generate), "generate")]
[JsonDerivedType(typeof(Embedding), "embedding")]
[JsonDerivedType(typeof(Discretization), "discretization")]
[JsonDerivedType(typeof(Solve), "solve")]
[JsonDerivedType(typeof(Coupling), "coupling")]
[JsonDerivedType(typeof(Optimization), "optimization")]
[JsonDerivedType(typeof(Sweep), "sweep")]
[JsonDerivedType(typeof(Clash), "clash")]
[JsonDerivedType(typeof(Twin), "twin")]
[JsonDerivedType(typeof(Uncertainty), "uncertainty")]
[JsonDerivedType(typeof(Fit), "fit")]
[JsonDerivedType(typeof(Governor), "governor")]
[JsonDerivedType(typeof(Drift), "drift")]
[JsonDerivedType(typeof(Assessment), "assessment")]
[JsonDerivedType(typeof(Quadrature), "quadrature")]
[JsonDerivedType(typeof(Trajectory), "trajectory")]
[JsonDerivedType(typeof(Sampling), "sampling")]
public abstract partial record ComputeReceipt : ISpanFormattable, IUtf8SpanFormattable {
    private ComputeReceipt() { }

    public required ReceiptScope Scope { get; init; }

    [JsonIgnore]
    public CorrelationId Correlation => Scope.Switch(
        execution: static execution => execution.Correlation,
        process: static process => process.Correlation);

    [JsonIgnore]
    public Option<WorkLane> Lane => Scope.Switch(
        execution: static execution => Some(execution.Lane),
        process: static _ => None);

    [JsonIgnore]
    public Option<Substrate> Substrate => Scope.Switch(
        execution: static execution => Some(execution.Substrate),
        process: static _ => None);

    [JsonIgnore]
    public AllocationClass AllocationClass => Scope.Switch(
        execution: static execution => execution.AllocationClass,
        process: static process => process.AllocationClass);

    [JsonIgnore]
    public Option<Duration> Elapsed => Scope.Switch(
        execution: static execution => Some(execution.Elapsed),
        process: static _ => None);

    public sealed record Selection(Seq<SelectionDecision> Decisions, SelectionMode Mode, bool WarmAffinity) : ComputeReceipt {
        public static Fin<Selection> Of(SelectionReceipt receipt, AdmittedIntent admitted, Duration elapsed) =>
            receipt.Correlation != admitted.Correlation || elapsed < Duration.Zero
                ? Fin.Fail<Selection>(new ComputeFault.EquivalenceMiss($"<selection-context-mismatch:{receipt.Correlation}:{admitted.Correlation}:{elapsed}>"))
                : Fin.Succ(new Selection(
                receipt.Hops.Map(static hop => hop.Rejection.Match<SelectionDecision>(
                    Some: reason => new SelectionDecision.Rejected(hop.Row, reason),
                    None: () => new SelectionDecision.Chosen(hop.Row))),
                receipt.Forced.Match<SelectionMode>(
                    Some: static row => new SelectionMode.Forced(row),
                    None: static () => new SelectionMode.Ranked()),
                receipt.WarmAffinity) {
                Scope = new ReceiptScope.Execution(
                    receipt.Correlation,
                    // `Lane` reads off the `Spec` because `WorkLane` is AppHost-declared and crosses TYPED;
                    // `Allocation` reads off the admission because `AllocationClass` is Compute's own, so the
                    // `Spec` carries only its key and `Admit` resolved that key onto the typed column here.
                    admitted.Spec.Lane,
                    receipt.Route,
                    admitted.Allocation,
                    elapsed),
            });
    }

    public sealed record TensorRun(TensorOpFamily Family, string Dtype, long Elements, string SimdWidth, int Partitions) : ComputeReceipt;

    public sealed record ModelLoad(string ModelChecksum, string Source, ExecutionProvider Ep, long Version) : ComputeReceipt;

    public sealed record Warmup(string ModelChecksum, ExecutionProvider Ep, string Shape) : ComputeReceipt;

    public sealed record ModelRun(string ModelChecksum, ExecutionProvider Ep, string Mode, int BatchSize, long PeakBytes, string? ArenaAllocator, ProfileArtifact? Profile) : ComputeReceipt;

    public sealed record RemoteCall(string Transport, string Method, string Status, long RequestBytes, long ResponseBytes, DeadlineOutcome Outcome) : ComputeReceipt;

    public sealed record StreamSegment(string ArtifactId, int Segments, long Bytes) : ComputeReceipt;

    public sealed record Allocation(
        StagingEventKind Event,
        long RequestedBytes,
        long GrantedBytes,
        string? Detail,
        string? NativeAllocator,
        long? NativeReservedBytes,
        long? SmallPoolFreeBytes,
        long? LargePoolFreeBytes) : ComputeReceipt {
        // GENERATED lowering: the hand fold this replaces spelled the same Option Match eight times, and a ninth
        // evidence column compiled clean while the receipt silently dropped it — RequiredMappingStrategy.Both now
        // fails that build. Scope stamps through the post-`with`, never a whole-source [MapPropertyFromSource]
        // reader, so RMG020 keeps its source-side force.
        public static Allocation Of(AllocationEvidence evidence) =>
            AllocationMapper.Lower(evidence) with { Scope = new ReceiptScope.Process(evidence.Correlation, evidence.Class) };
    }

    public sealed record Copy(OrtResidency Gate, long Bytes, string Device) : ComputeReceipt;

    public sealed record Cache(string Outcome, string Key, long Bytes) : ComputeReceipt;

    public sealed record UnitProjection(string Family, string OriginalUnit, double OriginalValue, double CanonicalValue) : ComputeReceipt;

    public sealed record Backpressure(int QueueDepth, Duration Waited, string? Dropped) : ComputeReceipt;

    public sealed record Drain(int Drained, int Faulted, int Refused) : ComputeReceipt;

    // Option→nullable converters lift absence ONCE; Correlation and Class are consumed by the Scope stamp at the
    // call site, so they are declared-ignored source inventory rather than silently unmapped members.
    [Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Both)]
    [UseStaticMapper(typeof(OptionCodec))]
    public static partial class AllocationMapper {
        [MapProperty(nameof(AllocationEvidence.Kind), nameof(Allocation.Event))]
        [MapperIgnoreSource(nameof(AllocationEvidence.Correlation))]
        [MapperIgnoreSource(nameof(AllocationEvidence.Class))]
        [MapperIgnoreTarget(nameof(Allocation.Scope))]
        public static partial Allocation Lower(AllocationEvidence evidence);
    }

    public static class OptionCodec {
        public static string? Text(Option<string> value) => value.Match<string?>(Some: static v => v, None: static () => null);

        public static long? Count(Option<long> value) => value.Match<long?>(Some: static v => v, None: static () => null);
    }

    public sealed record Conflict(string Subject, string Evidence) : ComputeReceipt;

    // Compute-INTERIOR refusals — a warm-bucket cap, a bucket-shape conflict, an unmeasured partition census —
    // are typed faults the executing rail returns and no reader ever sees, so a fleet refusing every stage for
    // want of a warm pulse stays invisible to Compute's own board. Reason reads the fault's OWN leading slug and
    // Subject its residual payload off the `<slug:payload>` detail grammar every ComputeFault message spells, so
    // a refusing site names no reason literal and a renamed slug moves the panel with it. Reason ALONE tags the
    // instrument — the slug set is closed by the fault declarations — while Subject carries a bucket key or a
    // card id whose cardinality no meter may fan on, which is why it stays a payload column.
    public sealed record Refusal(string Reason, string Subject, int Code) : ComputeReceipt {
        // Exemption: the span split is the language-forced statement seam — a `ReadOnlySpan<char>` crosses no
        // lambda, so no fold expresses it; the rail resumes on the returned case. Scope is Process because a
        // refusal precedes execution and has no lane, substrate, or elapsed to report honestly.
        public static Refusal Of(ComputeFault fault, CorrelationId correlation, AllocationClass allocation) {
            ReadOnlySpan<char> detail = fault.Message.AsSpan().Trim("<>");
            int cut = detail.IndexOf(':');
            return new Refusal(
                Reason: (cut < 0 ? detail : detail[..cut]).ToString(),
                Subject: cut < 0 ? string.Empty : detail[(cut + 1)..].ToString(),
                Code: fault.Code) {
                Scope = new ReceiptScope.Process(correlation, allocation),
            };
        }
    }

    public sealed record Factorization(string Provider, string Decomposition, int Rows, int Cols, long Nnz, string Format) : ComputeReceipt {
        public string? RouteVariant { get; init; }
        public string? DeterminismTag { get; init; }
        public int? SymbolicFill { get; init; }
        public double? ResidualCap { get; init; }
        public double? TrueResidual { get; init; }
        public int Shards { get; init; } = 1;
        public string? ShardNode { get; init; }
        public bool Merged { get; init; }
    }

    public sealed record Generate(string ModelChecksum, ExecutionProvider Ep, string ModelType, string Mode, string? Adapter, int Tokens, double TokensPerSecond, GuidanceKind GuidanceKind, int ConstrainedTokens, int ToolCalls) : ComputeReceipt {
        // Staged multimodal token total read once off Generator.TokenCount() after SetInputs — measured, resolution-invariant,
        // linear in media count; null on a text-only run, so the column separates prompt cost from media cost per run.
        public int? StagedTokens { get; init; }
    }

    public sealed record Embedding(string ModelChecksum, string Encoding, int Dimension, long ByteLength) : ComputeReceipt;

    public sealed record Discretization(string Algorithm, string Element, long Nodes, long Elements, int BoundaryLayers, int RefineLevel, double WorstQuality, string Metric) : ComputeReceipt;

    // Sharded runs emit one receipt per shard and the merge receipt that folds them, so the shard columns are
    // what separates the two populations: `Shards` is the run's shard count (1 unsharded), `ShardNode` is the farm
    // node that ran THIS shard and stays null on the merge and on every unsharded solve, and `Merged` marks the
    // fold alone. Without the discriminant a sharded solve counts its own parts as independent solves and every
    // convergence ratio over the stream reads a population the run never had.
    public sealed record Solve(string Physics, string Method, long Dofs, int Iterations, double Residual, bool Converged) : ComputeReceipt {
        public int Shards { get; init; } = 1;
        public string? ShardNode { get; init; }
        public bool Merged { get; init; }
        // Modal routes alone fill the per-axis summed effective-mass fractions (Σ Γ_d² / TotalMass_d) the seismic
        // mass-participation floor gates on; null on every non-modal solve, so the gate never reads a fabricated share.
        public double? ParticipationX { get; init; }
        public double? ParticipationY { get; init; }
        public double? ParticipationZ { get; init; }
    }

    public sealed record Coupling(string Scheme, int Fields, int Transfers, int Rounds, double CouplingResidual, bool Converged) : ComputeReceipt;

    public sealed record Optimization(string Optimizer, int Generations, int Evaluations, int SurrogateHits, int FrontSize, double Hypervolume) : ComputeReceipt {
        // True when the hypervolume reference box was derived from the front rather than policy-supplied — a derived
        // reference moves between runs, so cross-run hypervolume comparison is legible only with this flag.
        public bool ReferenceDerived { get; init; }
    }

    // Unranked counts the axes the sensitivity fold could take no measure on, so a fully-unranked campaign never
    // reads as a fully-ranked one; Failed = materialized points minus Completed, published rather than derived
    // because fractional designs make GridPoints an estimate while Completed counts the materialized run.
    public sealed record Sweep(long GridPoints, int Completed, int OnFront, int Dominated, int Unranked, int Failed) : ComputeReceipt;

    public sealed record Clash(string IndexKind, int Candidates, int HardClashes, int ClearanceViolations, int TotalPairs) : ComputeReceipt {
        // True when the survey stopped at ClashPolicy.MaxPairs — a truncated census read as complete undercounts
        // every downstream clearance ratio, so the ceiling hit is receipt evidence, never a silent cap.
        public bool Truncated { get; init; }
    }

    public sealed record Twin(string SignalId, double Predicted, double Measured, double Residual, bool Anomaly, double ControlDelta) : ComputeReceipt;

    public sealed record Uncertainty(
        string Method,
        int Samples,
        double? Mean,
        double? Variance,
        double? Skewness,
        double? Kurtosis,
        Seq<double> Quantiles,
        Seq<double> SobolFirst,
        Seq<double> SobolTotal,
        Seq<double> Interaction,
        Seq<double> MostProbablePoint,
        // Surrogate calibration measured off the Vandermonde the spectral fit already built — one GEMV, never a
        // second solve. A sampling, reliability, or subset run fits no surrogate and carries null in both, and an
        // exactly-determined basis interpolates with no residual degrees of freedom, so its standard error is null
        // rather than an infinity: the absence IS the measurement.
        double? FitQuality,
        double? ResidualStandardError,
        double FailureProbability,
        double ReliabilityIndex) : ComputeReceipt;

    public sealed record Fit(string Family, string Method, long Parameters, int Iterations, double Residual, bool Converged, double Quality, string QualityMetric, int RetainedRank) : ComputeReceipt;

    public sealed record Governor(double CpuPercent, double MemoryPercent, int Workers, int ReaderCeiling, int PartitionCap, double MemoryScale, bool SpillPressure) : ComputeReceipt;

    public sealed record Drift(string MonitorId, string Statistic, double Level, double? Limit, bool Breach, int Window) : ComputeReceipt;

    public string ToString(string? format, IFormatProvider? formatProvider) => ToString();

    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider) =>
        destination.TryWrite($"{Correlation}:{Lane.Map(static row => row.Key).IfNone("process")}:{Substrate.Map(static row => row.Key).IfNone("process")}:{Elapsed.Map(static value => value.ToString()).IfNone("process")}", out charsWritten);

    public bool TryFormat(Span<byte> utf8Destination, out int bytesWritten, ReadOnlySpan<char> format, IFormatProvider? provider) =>
        Utf8.TryWrite(utf8Destination, $"{Correlation}:{Lane.Map(static row => row.Key).IfNone("process")}:{Substrate.Map(static row => row.Key).IfNone("process")}:{Elapsed.Map(static value => value.ToString()).IfNone("process")}", out bytesWritten);
}

public sealed class Int64StringJsonConverter : JsonConverter<long> {
    public override long Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.TokenType == JsonTokenType.String && long.TryParse(reader.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out long value)
            ? value
            : throw new JsonException("Expected an invariant Int64 string.");

    public override void Write(Utf8JsonWriter writer, long value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.ToString(CultureInfo.InvariantCulture));
}

public sealed class UInt128HexJsonConverter : JsonConverter<UInt128> {
    public override UInt128 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.TokenType == JsonTokenType.String && UInt128.TryParse(reader.GetString(), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out UInt128 value)
            ? value
            : throw new JsonException("Expected an invariant UInt128 hex string.");

    public override void Write(Utf8JsonWriter writer, UInt128 value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.ToString("x32", CultureInfo.InvariantCulture));
}

// `LanguageExtJsonConverterFactory` is the kernel `csharp:Rasm/Domain/rails#CARRIER_CODEC` carrier-space owner
// this stratum already carries: `Seq<ChargebackRow>` and `Option<Substrate>` cross here, and without the factory
// each would serialize its carrier's own members rather than its value. This context declines the suite's
// `OmitAbsent` modifier by contract — the `[08]-[TS_PROJECTION]` posture is that absent evidence crosses as
// EXPLICIT NULL, never as an omitted member — so an `Option<T>` slot stays present on the wire, its ctor
// parameter stays wire-required by design, and the `| null` unions the TS mirrors spell are the agreement.
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
    RespectNullableAnnotations = true,
    RespectRequiredConstructorParameters = true,
    Converters = [
        typeof(ThinktectureJsonConverterFactory), typeof(LanguageExtJsonConverterFactory),
        typeof(Int64StringJsonConverter), typeof(UInt128HexJsonConverter)])]
[JsonSerializable(typeof(ComputeReceipt))]
[JsonSerializable(typeof(ReceiptScope))]
[JsonSerializable(typeof(SelectionDecision))]
[JsonSerializable(typeof(SelectionMode))]
[JsonSerializable(typeof(ProfileArtifact))]
[JsonSerializable(typeof(BenchmarkClaim))]
[JsonSerializable(typeof(GraduationEvidence))] // Model/identity#GRADUATION_EVIDENCE offline bundle — Bundle() reads its JsonTypeInfo off this context.
[JsonSerializable(typeof(BenchmarkInput))]
[JsonSerializable(typeof(HostFingerprint))]
[JsonSerializable(typeof(PanelRow))]
[JsonSerializable(typeof(AlertSpec))]
[JsonSerializable(typeof(ChargebackDataset))]
public partial class ComputeWireContext : JsonSerializerContext;

// Refusal cell is a ctor param from the owning composition, matching the capsule's own evidence-cell law —
// two compositions hold two cells and no instrument defect crosses between them.
public sealed class ReceiptSurface(
    ReceiptSinkPort sink, ComputeWireContext wire, InstrumentSet instruments, CostPolicy costs, Atom<Seq<Error>> refusals) {
    // Polymorphic metadata is the ONE kind roster, read once at type init: a per-access re-read walks the
    // serializer contract on every `Kinds`, `KindOf`, and `Probe` call, and the drift proof then compares two
    // independent readings of a registry that must be one.
    private static readonly (int Declared, Seq<(Type Case, string Kind)> Rows) Registry =
        ComputeWireContext.Default.ComputeReceipt.PolymorphismOptions is { } options
            ? (options.DerivedTypes.Count,
               toSeq(options.DerivedTypes)
                   .Choose(static row => row.TypeDiscriminator is string kind ? Some((row.DerivedType, kind)) : None)
                   .Strict())
            : (0, Seq<(Type, string)>());

    private static readonly FrozenDictionary<Type, string> KindByCase =
        Registry.Rows.ToFrozenDictionary(static row => row.Case, static row => row.Kind);

    public static readonly Seq<string> Kinds = Registry.Rows.Map(static row => row.Kind).Strict();

    // One lookup, two call shapes: an instance names its own kind, a case type names the kind a typed
    // selector binds before any fact exists.
    public static string KindOf(Type @case) => KindByCase[@case];

    public static string KindOf(ComputeReceipt fact) => KindOf(fact.GetType());

    public static Fin<Unit> Probe() {
        FrozenSet<Type> cases = typeof(ComputeReceipt)
            .GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic)
            .Where(static nested => nested.IsAssignableTo(typeof(ComputeReceipt)) && !nested.IsAbstract)
            .ToFrozenSet();
        return Registry.Declared == Registry.Rows.Count
            && Kinds.ToFrozenSet(StringComparer.Ordinal).Count == Kinds.Count
            && cases.SetEquals(Registry.Rows.Map(static row => row.Case).ToFrozenSet())
                ? Fin.Succ(unit)
                : Fin.Fail<Unit>(ComputeFault.Create(
                    $"<kind-registry-drift:cases={cases.Count}:rows={Registry.Rows.Count}:declared={Registry.Declared}>"));
    }

    public static readonly TelemetrySource Source = TelemetrySource.Compute;

    // Trace plane admitted into the kernel band: substrates ride the span NAME under this one scope, so no
    // dispatch bracket ever mints a per-route source.
    public static readonly TraceScope Dispatch = TraceScope.Create("rasm.compute.dispatch");

    public const string OkStatus = "ok";
    public const string HardClash = "hard";
    public const string ClearanceClash = "clearance";

    // --- [INSTRUMENT_NAMES] — one spelling each, read by the declaration row AND every writer recording it.
    public const string ReceiptsEmitted = "rasm.compute.receipts.emitted";
    public const string ClaimsBound = "rasm.compute.claims.bound";
    public const string SolveFactorizations = "rasm.compute.solve.factorizations";
    public const string SolveResidual = "rasm.compute.solve.residual";
    public const string SolveIterations = "rasm.compute.solve.iterations";
    public const string SolveRuns = "rasm.compute.solve.runs";
    public const string SolveConverged = "rasm.compute.solve.converged";
    public const string SolveShards = "rasm.compute.solve.shards";
    public const string GenerateTokens = "rasm.compute.generate.tokens";
    public const string OptimizeHypervolume = "rasm.compute.optimize.hypervolume";
    public const string MeshElements = "rasm.compute.mesh.elements";
    public const string ClashConfirmed = "rasm.compute.clash.confirmed";
    public const string TwinVerdicts = "rasm.compute.twin.verdicts";
    public const string TwinNominal = "rasm.compute.twin.nominal";
    public const string RemoteDuration = "rasm.compute.remote.duration";
    public const string RemoteCalls = "rasm.compute.remote.calls";
    public const string RemoteOk = "rasm.compute.remote.ok";
    public const string BackpressureVerdicts = "rasm.compute.backpressure.verdicts";
    public const string BackpressureAdmitted = "rasm.compute.backpressure.admitted";
    public const string AssessmentVerdicts = "rasm.compute.assessment.verdicts";
    public const string AssessmentRatio = "rasm.compute.assessment.ratio";
    public const string TrajectoryRuns = "rasm.compute.trajectory.runs";
    public const string TrajectoryResolved = "rasm.compute.trajectory.resolved";
    public const string TrajectorySteps = "rasm.compute.trajectory.steps";
    public const string QuadratureSkips = "rasm.compute.quadrature.skips";
    public const string ProgressMarks = "rasm.compute.progress.marks";
    public const string ProgressCadence = "rasm.compute.progress.cadence";
    public const string CostUnits = "rasm.compute.cost.units";
    public const string MonitorBreaches = "rasm.compute.monitor.breaches";
    public const string Refusals = "rasm.compute.refusals";

    // --- [DIMENSION_SLOTS] — the declared Dimensions column and the fan's tag keys are this one vocabulary,
    // so the governance view derives its tag-key set off the mounted row. The roster declares the key a write MAY
    // carry: an absent fact omits its key and the point exports untagged on that axis, which the kernel row reads
    // as the declaration's own absence arm exactly as it reads an untenanted write against TenantSlot.
    public const string KindSlot = "rasm.compute.kind";
    public const string SubstrateSlot = "rasm.compute.substrate";
    public const string LaneSlot = "rasm.compute.lane";
    public const string TransportSlot = "rasm.compute.transport";
    public const string StatusSlot = "rasm.compute.status";
    public const string ProviderSlot = "rasm.compute.provider";
    public const string DecompositionSlot = "rasm.compute.decomposition";
    public const string ModeSlot = "rasm.compute.run.mode";
    public const string AdapterSlot = "rasm.compute.lora.adapter";
    public const string GuidanceSlot = "rasm.compute.guidance";
    public const string AlgorithmSlot = "rasm.compute.algorithm";
    public const string RefineSlot = "rasm.compute.refine";
    public const string PhysicsSlot = "rasm.compute.physics";
    public const string MethodSlot = "rasm.compute.method";
    public const string ConvergedSlot = "rasm.compute.converged";
    public const string SeveritySlot = "rasm.compute.clash.severity";
    public const string IndexSlot = "rasm.compute.clash.index";
    public const string SignalSlot = "rasm.compute.signal";
    public const string MonitorSlot = "rasm.compute.monitor";
    public const string DisciplineSlot = "rasm.compute.discipline";
    public const string VerdictSlot = "rasm.compute.verdict";
    public const string PhaseSlot = "rasm.compute.phase";
    public const string ReasonSlot = "rasm.compute.refusal.reason";
    public const string TerminalSlot = "rasm.compute.trajectory.terminal";

    // Ratio indicators need BOTH series mounted as counters, so each SLO subject carries a population row and
    // a good row beside its shape measure; the fold in [10] and the store-side burn rule then read one truth.
    // Both rows of a pair declare the IDENTICAL dimension set so the burn rule divides series against series,
    // and the outcome axis that discriminates them rides the shape measures alone — carried on the good row it
    // is a constant column, and carried on the population row alone it makes the good row derivable.
    public static readonly Seq<InstrumentSpec> Specs = Seq(
        InstrumentSpec.Count(ReceiptsEmitted, "{receipt}", "receipts emitted through the sink port by kind", MeasureForm.Whole, KindSlot),
        InstrumentSpec.Level(ClaimsBound, "{claim}", "benchmark claims bound for the current host fingerprint", MeasureForm.Whole),
        InstrumentSpec.Count(SolveFactorizations, "{factorization}", "dense and sparse factorizations by provider and kind", MeasureForm.Whole, ProviderSlot, DecompositionSlot),
        InstrumentSpec.Advised(SolveResidual, "1", "iterative-solver convergence residual", MeasureForm.Real, Buckets.ResidualDecades, PhysicsSlot, MethodSlot, ConvergedSlot),
        InstrumentSpec.Advised(SolveIterations, "{iteration}", "iterative-solve iteration counts to convergence or frame-budget stop", MeasureForm.Whole, Buckets.IterationCounts, PhysicsSlot, MethodSlot, ConvergedSlot),
        InstrumentSpec.Count(SolveRuns, "{solve}", "iterative solves attempted by physics and method", MeasureForm.Whole, PhysicsSlot, MethodSlot),
        InstrumentSpec.Count(SolveConverged, "{solve}", "iterative solves reaching their convergence criterion", MeasureForm.Whole, PhysicsSlot, MethodSlot),
        InstrumentSpec.Count(SolveShards, "{shard}", "solve and factorization shards executed per substrate", MeasureForm.Whole, SubstrateSlot),
        InstrumentSpec.Count(GenerateTokens, "{token}", "tokens emitted through the generative run loop by run mode, adapter, and guidance", MeasureForm.Whole, ModeSlot, AdapterSlot, GuidanceSlot),
        InstrumentSpec.Advised(OptimizeHypervolume, "1", "Pareto-front hypervolume indicator per optimizer generation", MeasureForm.Real, Buckets.Hypervolume, MethodSlot),
        InstrumentSpec.Count(MeshElements, "{element}", "volumetric elements generated per discretization and refinement level", MeasureForm.Whole, AlgorithmSlot, RefineSlot),
        InstrumentSpec.Count(ClashConfirmed, "{clash}", "confirmed clashes by severity and federated-index kind", MeasureForm.Whole, SeveritySlot, IndexSlot),
        InstrumentSpec.Count(TwinVerdicts, "{verdict}", "digital-twin verdicts evaluated against the ROM error bound", MeasureForm.Whole, SignalSlot),
        InstrumentSpec.Count(TwinNominal, "{verdict}", "digital-twin verdicts inside the ROM error bound", MeasureForm.Whole, SignalSlot),
        InstrumentSpec.Advised(RemoteDuration, "s", "remote transport wall duration per call by transport and status", MeasureForm.Real, Buckets.RemoteSeconds, TransportSlot, StatusSlot),
        InstrumentSpec.Count(RemoteCalls, "{call}", "remote transport calls attempted by transport", MeasureForm.Whole, TransportSlot),
        InstrumentSpec.Count(RemoteOk, "{call}", "remote transport calls landing an ok status", MeasureForm.Whole, TransportSlot),
        InstrumentSpec.Count(BackpressureVerdicts, "{verdict}", "backpressure verdicts evaluated at the queue gate", MeasureForm.Whole),
        InstrumentSpec.Count(BackpressureAdmitted, "{verdict}", "backpressure verdicts admitting queued work without shedding", MeasureForm.Whole),
        InstrumentSpec.Count(AssessmentVerdicts, "{assessment}", "discipline assessments by discipline and verdict", MeasureForm.Whole, DisciplineSlot, VerdictSlot),
        InstrumentSpec.Advised(AssessmentRatio, "1", "governing utilization ratio per assessment by discipline", MeasureForm.Real, Buckets.GoverningRatio, DisciplineSlot),
        InstrumentSpec.Count(TrajectoryRuns, "{trajectory}", "trajectories traced through the adaptive integration driver", MeasureForm.Whole),
        InstrumentSpec.Count(TrajectoryResolved, "{trajectory}", "trajectories reaching their horizon inside every driver budget", MeasureForm.Whole),
        InstrumentSpec.Advised(TrajectorySteps, "{step}", "accepted trajectory steps per run by terminal marker", MeasureForm.Whole, Buckets.IterationCounts, TerminalSlot),
        InstrumentSpec.Count(QuadratureSkips, "{evaluation}", "integrand evaluations the kernel quadrature guard skipped as non-finite", MeasureForm.Whole),
        InstrumentSpec.Count(ProgressMarks, "{mark}", "progress marks delivered through the cadence gate by phase", MeasureForm.Whole, PhaseSlot),
        InstrumentSpec.Advised(ProgressCadence, "s", "interval between consecutive delivered progress marks by phase", MeasureForm.Real, Buckets.CadenceSeconds, PhaseSlot),
        InstrumentSpec.Advised(CostUnits, "1", "priced cost units attributed per tenant and substrate", MeasureForm.Real, Buckets.CostUnitDecades, TenantContext.TenantSlot, SubstrateSlot),
        InstrumentSpec.Count(MonitorBreaches, "{breach}", "streaming-monitor control-limit and drift breaches by monitor", MeasureForm.Whole, MonitorSlot),
        InstrumentSpec.Count(Refusals, "{refusal}", "typed refusals raised on the Compute interior rails by reason", MeasureForm.Whole, ReasonSlot));

    // Rows and the `[10]` pack derived from them leave as ONE downward fact, so the mounting root proves the
    // pack in the fold that binds these handles; the pack's own init reads this roster and never this method.
    public static TelemetryContributorPort Telemetry(string version) =>
        new(Scope: Source.Key, Version: version, Instruments: Specs, Planes: Seq(Dispatch), Board: ComputeDescriptors.Board);

    // Instrument refusals are evidence, never an emit abort: the envelope IS the truth and the instrument
    // stream is the lossy channel beside it, so a mount defect parks and the receipt still seals.
    public IO<ReceiptEnvelope> Emit(ComputeReceipt fact) =>
        IO.lift(() => TenantContext.Current)
            .Bind(tenant => IO.lift(() => Park(Metered(fact, tenant))).Map(_ => tenant))
            .Bind(tenant => IO.lift(() => JsonSerializer.SerializeToElement(fact, wire.ComputeReceipt))
                .Bind(payload => sink.Send(fact.Correlation, tenant, Source.Key, KindOf(fact), payload)));

    private Fin<Unit> Metered(ComputeReceipt fact, TenantContext tenant) =>
        ComputeInstrumentFan.Project(instruments, fact)
            .Bind(_ => ComputeInstrumentFan.Charge(instruments, costs, tenant, fact));

    private Unit Park(Fin<Unit> written) =>
        written.Match(Succ: static _ => unit, Fail: error => ignore(refusals.Swap(held => held.Add(error))));
}
```

## [03]-[TELEMETRY_PROJECTION]

- Owner: `ComputeInstrumentFan` — the one receipt-to-instrument projection over the typed union and the one priced cost write, its advice rows the kernel `Buckets` and its levels the composition `LevelCells`; `ComputeTraces` — the dispatch spine composing the kernel `SpanBand` at the admitted `rasm.compute.dispatch` scope. Instrument custody is one-per-composition: `ReceiptSurface.Telemetry` is the whole declaration and the composing app materializes it, so the fan holds writes alone and mints neither meter nor set.
- Entry: `ComputeInstrumentFan.Project(InstrumentSet set, ComputeReceipt fact)` folds one typed fact onto the write rail through the generated total `Switch`; `ComputeInstrumentFan.Charge(InstrumentSet set, CostPolicy costs, TenantContext tenant, ComputeReceipt fact)` records the one cost-unit write for a non-zero-priced fact under the tenancy tag set beside the substrate fact an execution-scoped receipt carries; `ComputeInstrumentFan.Bound(InstrumentSet set, BenchmarkRank ranks)` swaps the boot-frozen claim count onto its level cell through the kernel pulled gate; `ComputeTraces.Traced(DispatchTable table, SpanBand band)` decorates every dispatch arm with the substrate-named span.
- Auto: `ReceiptSurface.Emit` runs `Project` on the typed fact before serialization, so every emitted receipt projects with zero call-site metering and the kind space partitions cleanly — the AppHost fan owns AppHost kinds, this `Switch` owns Compute kinds, and one envelope kind projects in exactly one fan; every histogram row ships `InstrumentAdvice<T>` explicit-bucket boundaries as the fallback a backend without base2-exponential histograms reads; the trace-based exemplar filter at the provider joins any measurement recorded inside the live dispatch span to its trace and span ids with zero wiring; the composition root taps `Bound` where the boot-frozen `BenchmarkRank` projection resolves, folding the fingerprint-matched claim count into the level cell the `rasm.compute.claims.bound` gauge reads.
- Receipt: none — the fan is a projection of receipts; an instrument write beside it is a second truth. Every write returns the kernel rail and a refusal parks on the composition's cell at `ReceiptSurface.Emit`, so a mount defect is evidence rather than a silently dropped measurement or an emit abort.
- Packages: LanguageExt.Core, Rasm (kernel signal capsule), BCL inbox
- Growth: a new receipt case breaks the projection `Switch` at compile time, so every new kind decides its instrument writes or returns the succeeding rail explicitly; a new instrument is one `[02]-[RECEIPT_UNION]` roster row and one arm edit; a level-shaped fact is one `set.Level` write and one `Level` declaration row.
- Boundary: instruments stay curated aggregates, never 1:1 with cases — remote latency reads the scope `Elapsed` off `RemoteCall`, clash counts split hard and clearance on a severity tag, assessment rows carry discipline and verdict dimensions, and every tag fan rides the AppHost tenant cardinality cap — the refusal reason is bounded by the fault declarations that spell each slug, so its fan is closed by construction while the refusal subject carries a bucket key or a card id and stays a payload column no meter reads; a ratio indicator's population and good series are BOTH mounted counters written from the same arm, so the in-process burn fold and the store-side burn rule read one truth and neither re-derives the other's numerator; the span opens at dispatch and closes on the arm's own rail through the band's IO bracket, `SetStatus(ActivityStatusCode.Error)` the typed verdict on the fail leg, and a package-local `ActivitySource` beside the band is the deleted form; profile correlation rides trace identity — the AppHost-composed `PyroscopeSpanProcessor` stamps `pyroscope.profile.id` on the trace's root span alone and clears the profiler span context at `OnEnd`, so a `rasm.compute.dispatch.*` `ActivityKind.Internal` span joins its flame-graph slice through the `TraceId` it shares with that stamped root, and Compute stamps no profile tag and holds zero OTel package reference — a Compute-side re-stamp of interior spans is the deleted form; receipt payload identifiers — checksums, content keys, artifact ids, provider names — sit in the `Operational`/`Internal` classification tiers whose redactor rows pass, so per-field `DataClassification` attributes never land on receipt cases and redaction custody stays at the AppHost egress seam; the cost write is the lossy channel — the `[09]-[COST_LEDGER]` ledger folds over the envelope-joined journal stay billing truth, a zero-priced fact skips the write, and the tenant tag rides the same AppHost cardinality cap every tag fan obeys; both cost dimensions are ABSENCE-BEARING and neither fills — the root frame contributes no tenant tag and a process-scoped receipt contributes no substrate fact, so the point exports untagged on that axis, which is what the declared `Dimensions` roster means by naming the key a write may carry, and a scope word substituted for the absent `Substrate` row exports a sixth substrate the vocabulary never spells; every tag set mints through the kernel `InstrumentSet.Tags` `TagList` projection the `in TagList` write overload consumes, so an arm allocates no heap tag array and a folder-local tag fold beside it is the deleted re-mint.

```csharp signature
public static class ComputeInstrumentFan {
    public static Fin<Unit> Bound(InstrumentSet set, BenchmarkRank ranks) =>
        set.Level(ReceiptSurface.ClaimsBound, ranks.Ranks.Count);

    public static Fin<Unit> Charge(InstrumentSet set, CostPolicy costs, TenantContext tenant, ComputeReceipt fact) =>
        costs.Price(fact) is { Total: > 0d } priced
            ? set.Write(ReceiptSurface.CostUnits, priced.Total, Attributed(tenant, fact.Substrate))
            : Fin.Succ(unit);

    // BOTH cost dimensions are absence-bearing on one write and neither invents a filler: `TenantContext.Tags` is
    // empty for the root frame, and a process-scoped receipt — a refusal, a drain, an allocation — carries no
    // substrate, so its fact is omitted whole. The `Substrate` row keys are the WHOLE value space of this axis, so
    // a scope word standing in for an absent row exports a sixth substrate no vocabulary spells and every backend
    // grouping cost by substrate reads a phantom row; the untagged point is the honest one, and the scope a charge
    // ran under is already the receipt's own `ReceiptScope` case.
    static TagList Attributed(TenantContext tenant, Option<Substrate> substrate) =>
        substrate.Match(
            Some: row => InstrumentSet.Tags(tenant, (ReceiptSurface.SubstrateSlot, row.Key)),
            None: () => InstrumentSet.Tags(tenant));

    // Every arm returns the write rail, so a refused measurement reaches ReceiptSurface.Emit's parking cell
    // instead of vanishing; an arm with nothing to meter says so explicitly and the Switch stays total. Every tag
    // set mints through the kernel `InstrumentSet.Tags` projection the `in TagList` write overload consumes, so no
    // arm allocates a heap tag array and this folder re-spells no part of that one materialization.
    public static Fin<Unit> Project(InstrumentSet set, ComputeReceipt fact) =>
        set.Write(ReceiptSurface.ReceiptsEmitted, 1L, InstrumentSet.Tags((ReceiptSurface.KindSlot, ReceiptSurface.KindOf(fact))))
            .Bind(_ => fact.Switch(
                state: set,
                selection: static (_, _) => Fin.Succ(unit),
                tensorRun: static (_, _) => Fin.Succ(unit),
                modelLoad: static (_, _) => Fin.Succ(unit),
                warmup: static (_, _) => Fin.Succ(unit),
                modelRun: static (_, _) => Fin.Succ(unit),
                remoteCall: static (s, r) => Called(s, r),
                streamSegment: static (_, _) => Fin.Succ(unit),
                allocation: static (_, _) => Fin.Succ(unit),
                copy: static (_, _) => Fin.Succ(unit),
                cache: static (_, _) => Fin.Succ(unit),
                unitProjection: static (_, _) => Fin.Succ(unit),
                backpressure: static (s, b) => s.Write(ReceiptSurface.BackpressureVerdicts, 1L)
                    .Bind(_ => b.Dropped is null ? s.Write(ReceiptSurface.BackpressureAdmitted, 1L) : Fin.Succ(unit)),
                drain: static (_, _) => Fin.Succ(unit),
                conflict: static (_, _) => Fin.Succ(unit),
                refusal: static (s, r) => s.Write(ReceiptSurface.Refusals, 1L,
                    InstrumentSet.Tags((ReceiptSurface.ReasonSlot, r.Reason))),
                factorization: static (s, f) => Factored(s, f),
                generate: static (s, g) => s.Write(ReceiptSurface.GenerateTokens, (long)g.Tokens, InstrumentSet.Tags(
                    (ReceiptSurface.ModeSlot, g.Mode),
                    (ReceiptSurface.AdapterSlot, g.Adapter),
                    (ReceiptSurface.GuidanceSlot, g.GuidanceKind.Key))),
                embedding: static (_, _) => Fin.Succ(unit),
                discretization: static (s, d) => s.Write(ReceiptSurface.MeshElements, d.Elements, InstrumentSet.Tags(
                    (ReceiptSurface.AlgorithmSlot, d.Algorithm),
                    (ReceiptSurface.RefineSlot, d.RefineLevel))),
                solve: static (s, v) => Solved(s, v),
                coupling: static (_, _) => Fin.Succ(unit),
                optimization: static (s, o) => s.Write(ReceiptSurface.OptimizeHypervolume, o.Hypervolume,
                    InstrumentSet.Tags((ReceiptSurface.MethodSlot, o.Optimizer))),
                sweep: static (_, _) => Fin.Succ(unit),
                clash: static (s, c) => Clashed(s, c),
                twin: static (s, t) => Twinned(s, t),
                uncertainty: static (_, _) => Fin.Succ(unit),
                fit: static (_, _) => Fin.Succ(unit),
                governor: static (_, _) => Fin.Succ(unit),
                drift: static (s, d) => d.Breach
                    ? s.Write(ReceiptSurface.MonitorBreaches, 1L, InstrumentSet.Tags((ReceiptSurface.MonitorSlot, d.MonitorId)))
                    : Fin.Succ(unit),
                assessment: static (s, a) => Assessed(s, a),
                // Fixed-order rows skip nothing, so the write gates on a measured skip rather than recording a
                // zero that reads as coverage the kernel guard never granted.
                quadrature: static (s, q) => q.Skipped > 0
                    ? s.Write(ReceiptSurface.QuadratureSkips, (long)q.Skipped)
                    : Fin.Succ(unit),
                trajectory: static (s, t) => Traced(s, t),
                // Sampling facts carry campaign diagnostics — a net's discrepancy pair, a fit's centre count —
                // and no fleet aggregate: neither figure is comparable across families or dimensions, so a meter
                // over either exports one series mixing incomparable populations. The `ReceiptsEmitted` kind
                // counter already carries the run census, and the diagnostics stay payload columns a replay reads.
                sampling: static (_, _) => Fin.Succ(unit)));

    static Fin<Unit> Called(InstrumentSet set, ComputeReceipt.RemoteCall call) {
        TagList transport = InstrumentSet.Tags((ReceiptSurface.TransportSlot, call.Transport));
        return set.Write(ReceiptSurface.RemoteCalls, 1L, transport)
            .Bind(_ => StringComparer.Ordinal.Equals(call.Status, ReceiptSurface.OkStatus)
                ? set.Write(ReceiptSurface.RemoteOk, 1L, transport)
                : Fin.Succ(unit))
            .Bind(_ => call.Elapsed.Match(
                Some: elapsed => set.Write(ReceiptSurface.RemoteDuration, elapsed.TotalSeconds, InstrumentSet.Tags(
                    (ReceiptSurface.TransportSlot, call.Transport),
                    (ReceiptSurface.StatusSlot, call.Status))),
                None: static () => Fin.Succ(unit)));
    }

    // Subject tags the ratio pair, outcome tags the shape measures: the burn rule divides converged by runs
    // series against series, while residual and iteration distributions still split on the verdict.
    static Fin<Unit> Solved(InstrumentSet set, ComputeReceipt.Solve solve) {
        TagList subject = InstrumentSet.Tags(
            (ReceiptSurface.PhysicsSlot, solve.Physics),
            (ReceiptSurface.MethodSlot, solve.Method));
        TagList outcome = InstrumentSet.Tags(
            (ReceiptSurface.PhysicsSlot, solve.Physics),
            (ReceiptSurface.MethodSlot, solve.Method),
            (ReceiptSurface.ConvergedSlot, solve.Converged));
        return set.Write(ReceiptSurface.SolveRuns, 1L, subject)
            .Bind(_ => solve.Converged ? set.Write(ReceiptSurface.SolveConverged, 1L, subject) : Fin.Succ(unit))
            .Bind(_ => set.Write(ReceiptSurface.SolveResidual, solve.Residual, outcome))
            .Bind(_ => set.Write(ReceiptSurface.SolveIterations, (long)solve.Iterations, outcome))
            .Bind(_ => Sharded(set, solve.Shards, solve.Merged, solve.Substrate));
    }

    static Fin<Unit> Factored(InstrumentSet set, ComputeReceipt.Factorization factorization) =>
        set.Write(ReceiptSurface.SolveFactorizations, 1L, InstrumentSet.Tags(
                (ReceiptSurface.ProviderSlot, factorization.Provider),
                (ReceiptSurface.DecompositionSlot, factorization.Decomposition)))
            .Bind(_ => Sharded(set, factorization.Shards, factorization.Merged, factorization.Substrate));

    // Shard census over the ONE decomposed-execution axis both numeric cases share. The merge receipt folds shard
    // results and executes no shard of its own, so it is the row that records nothing here and the counter stays the
    // true execution count rather than the run count plus a fold. Substrate is absence-bearing on the same law the
    // cost write holds: a process-scoped receipt omits the key and the point exports untagged on that axis.
    static Fin<Unit> Sharded(InstrumentSet set, int shards, bool merged, Option<Substrate> substrate) =>
        merged
            ? Fin.Succ(unit)
            : substrate.Match(
                Some: row => set.Write(ReceiptSurface.SolveShards, (long)shards,
                    InstrumentSet.Tags((ReceiptSurface.SubstrateSlot, row.Key))),
                None: () => set.Write(ReceiptSurface.SolveShards, (long)shards));

    // Both severities are ALWAYS written, zero included: a confirmed-clash count of zero is a measured census over
    // a run that traversed the index, never an absent measurement, so the severity pair reads as a partition.
    static Fin<Unit> Clashed(InstrumentSet set, ComputeReceipt.Clash clash) =>
        Seq(
                (Severity: ReceiptSurface.HardClash, Count: (long)clash.HardClashes),
                (Severity: ReceiptSurface.ClearanceClash, Count: (long)clash.ClearanceViolations))
            .TraverseM(row => set.Write(ReceiptSurface.ClashConfirmed, row.Count, InstrumentSet.Tags(
                (ReceiptSurface.SeveritySlot, row.Severity),
                (ReceiptSurface.IndexSlot, clash.IndexKind))))
            .As().Map(static _ => unit);

    // Verdicts and nominal are the population/good counter pair on ONE signal dimension, so the burn rule divides
    // series against series and the anomaly flag never splits the denominator it is measured against.
    static Fin<Unit> Twinned(InstrumentSet set, ComputeReceipt.Twin twin) {
        TagList signal = InstrumentSet.Tags((ReceiptSurface.SignalSlot, twin.SignalId));
        return set.Write(ReceiptSurface.TwinVerdicts, 1L, signal)
            .Bind(_ => twin.Anomaly ? Fin.Succ(unit) : set.Write(ReceiptSurface.TwinNominal, 1L, signal));
    }

    // Population and good series are BOTH counters written from this one arm, so the burn fold and the store-side
    // rule divide one truth; the terminal marker tags the step distribution alone, where it discriminates rather
    // than splitting the ratio pair the burn rule divides series against series.
    static Fin<Unit> Traced(InstrumentSet set, ComputeReceipt.Trajectory run) =>
        set.Write(ReceiptSurface.TrajectoryRuns, 1L)
            .Bind(_ => run.Resolved ? set.Write(ReceiptSurface.TrajectoryResolved, 1L) : Fin.Succ(unit))
            .Bind(_ => set.Write(ReceiptSurface.TrajectorySteps, (long)run.Steps,
                InstrumentSet.Tags((ReceiptSurface.TerminalSlot, run.Terminal))));

    static Fin<Unit> Assessed(InstrumentSet set, ComputeReceipt.Assessment assessment) =>
        set.Write(ReceiptSurface.AssessmentVerdicts, 1L, InstrumentSet.Tags(
                (ReceiptSurface.DisciplineSlot, assessment.Discipline),
                (ReceiptSurface.VerdictSlot, assessment.Verdict)))
            .Bind(_ => set.Write(ReceiptSurface.AssessmentRatio, assessment.GoverningRatio,
                InstrumentSet.Tags((ReceiptSurface.DisciplineSlot, assessment.Discipline))));
}

public static class ComputeTraces {
    // Band composition owns the one bracket: substrates ride the span name under the admitted
    // `rasm.compute.dispatch` scope, and an arm's IO rail brackets through the band's own IO overload.
    public static DispatchTable Traced(DispatchTable table, SpanBand band) => new(
        CpuTensor: Spanned(band, Substrate.CpuTensor, table.CpuTensor),
        DeviceWgpu: Spanned(band, Substrate.DeviceWgpu, table.DeviceWgpu),
        Onnx: Spanned(band, Substrate.Onnx, table.Onnx),
        GenAi: Spanned(band, Substrate.GenAi, table.GenAi),
        RemoteGrpc: Spanned(band, Substrate.RemoteGrpc, table.RemoteGrpc));

    static Func<AdmittedIntent, IO<Unit>> Spanned(SpanBand band, Substrate route, Func<AdmittedIntent, IO<Unit>> arm) =>
        admitted => band.Traced(
            ReceiptSurface.Dispatch,
            Op.Of(name: route.Key),
            span => IO.lift(() => Tagged(span, route, admitted)).Bind(_ => arm(admitted)));

    static Unit Tagged(Activity? span, Substrate route, AdmittedIntent admitted) =>
        ignore(span?
            .SetTag(CorrelationId.Slot, admitted.Correlation.ToString())
            .SetTag(ReceiptSurface.LaneSlot, admitted.Spec.Lane.Key)
            .SetTag(ReceiptSurface.SubstrateSlot, route.Key));
}
```

## [04]-[FOLD_PROJECTIONS]

- Owner: `ReceiptFolds` — every operational view is a pure fold over `Seq<ComputeReceipt>`, and every tenant-partitioned ledger view is a pure fold over the envelope-joined `Seq<(TenantContext Tenant, ComputeReceipt Fact)>` journal — the tenant is an envelope fact, so it enters the fold INPUT through the `Journal` join, never a fabricated dimension over the bare fact stream; the fact stream is the single source and no projection accumulates mutably. `ReceiptReplay`/`ReplayVerdict` — the certification-grade re-derivation fold: a content-keyed verdict re-derives from its recorded inputs and diffs against the stored payload under the receipt's determinism tag, so a permit-submitted verdict is provable on demand instead of merely cached.
- Entry: `public HashMap<CorrelationId, Seq<ComputeReceipt>> Provenance` — the model-result provenance projection joining every receipt chain by correlation. `ReceiptFolds.Journal(Seq<ReceiptEnvelope> envelopes, ComputeWireContext wire)` — the one envelope-decode join: Compute-package envelopes rehydrate through the Strict wire context into `(Tenant, Fact)` rows, a decode refusal failing the rail rather than dropping a billed fact. `ReceiptReplay.Replay(UInt128 contentKey, ReadOnlyMemory<byte> stored, Option<string> determinismTag, Func<Fin<ReadOnlyMemory<byte>>> rederive)` — the caller composes `rederive` from the settled Persistence contracts (`Version/ledger` `OpLogEntry.Closure` resolves the input manifest, `Query/cache` `ModelResultIndex.Lookup` serves the stored payload) and the verdict states reproducibility as a typed fact.
- Auto: per-lane counts, route histograms, hot-path totals, leak indicators, conflict evidence, solver-divergence and twin-anomaly extractions, numeric-provider attribution, residency-gate crossings, and provenance chains derive on read from the identical stream the dashboards consume. Replay comparison mode derives from the CLOSED determinism-tag grammar — a `bit*` tag demands byte equality, an `envelope*`/`device-wgpu*` tag compares the payloads as little-endian double lanes under the relative defect the tag's provider triple licenses, and an unrecognized tag, an absent tag, or a failed re-derivation lands `Unreplayable` with its reason — never a caller-chosen comparison the tag contradicts and never a fabricated bitwise class.
- Packages: LanguageExt.Core, NodaTime, System.Numerics.Tensors (`TensorPrimitives.Distance`/`Norm` the envelope defect), BCL inbox (`BinaryPrimitives` lane decode)
- Growth: a new operational view is one fold member row over the same fact stream; a new tenant-partitioned view is one member on the journal extension; a new determinism class is one comparison arm on `ReceiptReplay` keyed by its tag grammar; zero new surface.
- Boundary: leak indicators read `StagingEventKind.StreamDoubleDisposed` and `StreamFinalized`, while `Diagnostics` reads the row's `Diagnostic` column. `DiscardTaxonomy` folds `BufferDiscarded` detail into a reason-keyed count. Execution projections choose only facts carrying their `Option` spine values; process-scoped allocation evidence remains in provenance and diagnostic folds without a fabricated lane or route. Mutable accumulators, per-view repositories, and second fact streams reject. Replay never unfreezes a wire or fabricates inputs — an unresolvable closure, an absent tag where the payload is not byte-comparable, or a non-8-aligned envelope payload lands `Unreplayable` with its reason, never a coerced `Reproduced`. Tenant partition enters through `Journal` alone — a tenant-keyed member over the bare fact stream fabricates a dimension its input never carried and rejects; process-scoped facts keep their priced cost under the tenant with no fabricated route, `TenantRouteCosts` covering only execution-scoped rows by construction.

```csharp signature
public static class ReceiptFolds {
    public static Fin<Seq<(TenantContext Tenant, ComputeReceipt Fact)>> Journal(Seq<ReceiptEnvelope> envelopes, ComputeWireContext wire) =>
        envelopes.Filter(static envelope => StringComparer.Ordinal.Equals(envelope.Package, ReceiptSurface.Source.Key))
            .TraverseM(envelope => Try.lift(() => envelope.Payload.Deserialize(wire.ComputeReceipt)).Run()
                .MapFail(error => (Error)ComputeFault.Create($"<journal-decode-rejected:{envelope.Kind}:{error.Message}>"))
                .Bind(fact => fact is null
                    ? Fin.Fail<(TenantContext, ComputeReceipt)>(ComputeFault.Create($"<journal-payload-null:{envelope.Kind}>"))
                    : Fin.Succ((envelope.Tenant, fact))))
            .As();

    extension(Seq<(TenantContext Tenant, ComputeReceipt Fact)> journal) {
        public HashMap<TenantId, CostVector> TenantCosts(CostPolicy costs) =>
            journal.Map(row => (Key: row.Tenant.TenantId, Priced: costs.Price(row.Fact)))
                .Fold(HashMap<TenantId, CostVector>(), static (acc, row) => acc.AddOrUpdate(row.Key, held => held + row.Priced, row.Priced));

        public HashMap<(TenantId Tenant, Substrate Route), CostVector> TenantRouteCosts(CostPolicy costs) =>
            journal.Choose(row => row.Fact.Substrate.Map(route => (Key: (row.Tenant.TenantId, route), Priced: costs.Price(row.Fact))))
                .Fold(HashMap<(TenantId, Substrate), CostVector>(), static (acc, row) => acc.AddOrUpdate(row.Key, held => held + row.Priced, row.Priced));

        public HashMap<TenantId, long> TenantFacts =>
            journal.Fold(HashMap<TenantId, long>(), static (acc, row) => acc.AddOrUpdate(row.Tenant.TenantId, static count => count + 1L, 1L));
    }

    extension(Seq<ComputeReceipt> facts) {
        public HashMap<WorkLane, long> LaneCounts =>
            facts.Choose(static fact => fact.Lane.Map(lane => (Lane: lane, Count: 1L)))
                .Fold(HashMap<WorkLane, long>(), static (acc, row) => acc.AddOrUpdate(row.Lane, static count => count + 1L, row.Count));

        public HashMap<Substrate, long> RouteHistogram =>
            facts.Choose(static fact => fact.Substrate.Map(route => (Route: route, Count: 1L)))
                .Fold(HashMap<Substrate, long>(), static (acc, row) => acc.AddOrUpdate(row.Route, static count => count + 1L, row.Count));

        public HashMap<Substrate, Duration> HotPathTotals =>
            facts.Choose(static fact => fact.Substrate.Bind(route => fact.Elapsed.Map(elapsed => (Route: route, Elapsed: elapsed))))
                .Fold(HashMap<Substrate, Duration>(), static (acc, row) => acc.AddOrUpdate(row.Route, total => total + row.Elapsed, row.Elapsed));

        public Seq<ComputeReceipt.Allocation> Leaks =>
            facts.Bind(static fact => fact is ComputeReceipt.Allocation allocation
                && (allocation.Event == StagingEventKind.StreamDoubleDisposed || allocation.Event == StagingEventKind.StreamFinalized)
                ? Seq(allocation)
                : Seq<ComputeReceipt.Allocation>());

        public Seq<ComputeReceipt.Allocation> Diagnostics =>
            facts.Bind(static fact => fact is ComputeReceipt.Allocation { Event: { Diagnostic: true } } diagnostic
                ? Seq(diagnostic)
                : Seq<ComputeReceipt.Allocation>());

        public HashMap<string, long> DiscardTaxonomy =>
            facts.Choose(static fact => fact is ComputeReceipt.Allocation allocation
                    && allocation.Event == StagingEventKind.BufferDiscarded
                    && allocation.Detail is string reason
                    ? Some(reason)
                    : None)
                .Fold(HashMap<string, long>(), static (acc, reason) => acc.AddOrUpdate(reason, static count => count + 1L, 1L));

        public HashMap<OrtResidency, long> Crossings =>
            facts.Fold(HashMap<OrtResidency, long>(), static (acc, fact) =>
                fact is ComputeReceipt.Copy crossing ? acc.AddOrUpdate(crossing.Gate, static count => count + 1L, 1L) : acc);

        public Seq<ComputeReceipt.Conflict> Conflicts =>
            facts.Bind(static fact => fact is ComputeReceipt.Conflict conflict ? Seq(conflict) : Seq<ComputeReceipt.Conflict>());

        // Refusal taxonomy folds by the fault's own slug, the same reason-keyed shape DiscardTaxonomy takes over
        // pool discards — one view answering which interior gate is refusing, where the meter answers only how often.
        public HashMap<string, long> RefusalTaxonomy =>
            facts.Choose(static fact => fact is ComputeReceipt.Refusal refusal ? Some(refusal.Reason) : None)
                .Fold(HashMap<string, long>(), static (acc, reason) => acc.AddOrUpdate(reason, static count => count + 1L, 1L));

        public Seq<ComputeReceipt.Solve> Diverged =>
            facts.Bind(static fact => fact is ComputeReceipt.Solve { Converged: false } stalled ? Seq(stalled) : Seq<ComputeReceipt.Solve>());

        public Seq<ComputeReceipt.Fit> Nonconverged =>
            facts.Bind(static fact => fact is ComputeReceipt.Fit { Converged: false } stalled ? Seq(stalled) : Seq<ComputeReceipt.Fit>());

        public Seq<ComputeReceipt.Twin> Anomalies =>
            facts.Bind(static fact => fact is ComputeReceipt.Twin { Anomaly: true } flagged ? Seq(flagged) : Seq<ComputeReceipt.Twin>());

        public Seq<ComputeReceipt.Drift> Breaches =>
            facts.Bind(static fact => fact is ComputeReceipt.Drift { Breach: true } breached ? Seq(breached) : Seq<ComputeReceipt.Drift>());

        public HashMap<string, long> Providers =>
            facts.Fold(HashMap<string, long>(), static (acc, fact) =>
                fact is ComputeReceipt.Factorization factorization ? acc.AddOrUpdate(factorization.Provider, static count => count + 1L, 1L) : acc);

        public HashMap<CorrelationId, Seq<ComputeReceipt>> Provenance =>
            facts.Fold(HashMap<CorrelationId, Seq<ComputeReceipt>>(), static (acc, fact) => acc.AddOrUpdate(fact.Correlation, chain => chain.Add(fact), Seq(fact)));
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ReplayVerdict {
    private ReplayVerdict() { }
    public sealed record Reproduced(UInt128 ContentKey, string Mode) : ReplayVerdict;
    public sealed record Diverged(UInt128 ContentKey, string Mode, double Defect) : ReplayVerdict;
    public sealed record Unreplayable(UInt128 ContentKey, string Reason) : ReplayVerdict;
}

public static class ReceiptReplay {
    // Closed tag grammar: `bit*` compares byte-exact, `envelope*` and `device-wgpu*` compare little-endian double
    // lanes under the relative defect ceiling the tag's provider triple licenses. An unrecognized or absent tag is
    // Unreplayable with its reason, and a failed re-derivation is Unreplayable, never a failed rail or a coerced
    // comparison class the receipt never declared.
    const string BitTagPrefix = "bit";
    const string EnvelopeTagPrefix = "envelope";
    const string DeviceTagPrefix = "device-wgpu";
    const double EnvelopeDefectCeiling = 1e-9;

    public static ReplayVerdict Replay(UInt128 contentKey, ReadOnlyMemory<byte> stored, Option<string> determinismTag, Func<Fin<ReadOnlyMemory<byte>>> rederive) {
        ArgumentNullException.ThrowIfNull(rederive);
        return rederive().Match(
            Fail: error => (ReplayVerdict)new ReplayVerdict.Unreplayable(contentKey, $"<rederive:{error.Message}>"),
            Succ: fresh => determinismTag.Match(
                Some: tag => tag.StartsWith(BitTagPrefix, StringComparison.Ordinal) ? Bitwise(contentKey, stored, fresh, tag)
                    : tag.StartsWith(EnvelopeTagPrefix, StringComparison.Ordinal) || tag.StartsWith(DeviceTagPrefix, StringComparison.Ordinal)
                        ? Envelope(contentKey, stored, fresh, tag)
                        : new ReplayVerdict.Unreplayable(contentKey, $"<unrecognized-tag:{tag}>"),
                None: () => new ReplayVerdict.Unreplayable(contentKey, "<untagged>")));
    }

    static ReplayVerdict Bitwise(UInt128 key, ReadOnlyMemory<byte> stored, ReadOnlyMemory<byte> fresh, string tag) =>
        stored.Span.SequenceEqual(fresh.Span)
            ? new ReplayVerdict.Reproduced(key, tag)
            : new ReplayVerdict.Diverged(key, tag, Defect: stored.Length == fresh.Length ? 1.0 : double.PositiveInfinity);

    static ReplayVerdict Envelope(UInt128 key, ReadOnlyMemory<byte> stored, ReadOnlyMemory<byte> fresh, string tag) {
        if (stored.Length != fresh.Length || stored.Length % 8 != 0 || stored.Length == 0) {
            return new ReplayVerdict.Unreplayable(key, $"<envelope-shape:{stored.Length}/{fresh.Length}>");
        }
        double[] held = Lane(stored.Span);
        double[] derived = Lane(fresh.Span);
        double defect = TensorPrimitives.Distance<double>(held, derived) / Math.Max(TensorPrimitives.Norm<double>(held), double.Epsilon);
        return double.IsFinite(defect) && defect <= EnvelopeDefectCeiling
            ? new ReplayVerdict.Reproduced(key, tag)
            : new ReplayVerdict.Diverged(key, tag, defect);
    }

    static double[] Lane(ReadOnlySpan<byte> payload) {
        double[] lane = new double[payload.Length / 8];
        for (int i = 0; i < lane.Length; i++) { lane[i] = BinaryPrimitives.ReadDoubleLittleEndian(payload[(8 * i)..]); }
        return lane;
    }
}
```

## [05]-[BENCHMARK_CLAIMS]

- Owner: `BenchmarkInput`, `BenchmarkClaim`, `ProfileArtifact` — the admitted tensor-shape/stride/density class, measured claim row bound to the Persistence `BenchmarkFamily` and admitted `CacheToken`, and the typed profile-evidence family; a claim is data, never prose. Gating them is `Rasm.AppHost/Runtime/determinism#DETERMINISM_KERNEL` `HostFingerprint`, the effective-host identity COMPOSED here as the claim's `host` column through this package's legal reference and extended with the two members only this domain decides — never re-declared.
- Entry: `public Option<BenchmarkRow> Claim(ModelResultIndex index, Seq<BenchmarkRow> rows)` — delegates fingerprint and recency admission to the Persistence `ModelResultIndex.Claim` owner (its horizon and clock are closed inside the index; no call shape can omit or replace them); `None` is the fall-through to the static cost rank on the substrate row. `public Option<Duration> Forecast(ModelResultIndex index, Seq<BenchmarkClaim> claims, Substrate substrate, long payloadBytes)` is the ONE duration-forecast query — it narrows the claims to the substrate row and the `BandOf` payload band, hands the survivors' minted rows to that same `Claim` gate, and answers the winner's `Median`; `Runtime/admission#SUBSTRATE_AXIS` `SelectionContext.Forecast` binds it and re-derives no half of it.
- Auto: `BenchmarkInput.Admit` validates payload size, dtype, shape, strides, batch, and density, derives rank and contiguity, and classifies the payload band. `Key` includes the family, admitted case token, full input class, route, provider, and tolerance class, so claim admission refuses a zero-init case token — the struct value object's admission-bypassing ghost — beside the family check before identity forms. `Persist` delegates the durable mint to `BenchmarkFamily.Claim`, carrying operations, corpus, artifact key, timing, allocation, fingerprint, and timestamp without a parallel constructor; `Stale` compares the effective fingerprint through the spine record's generated structural equality, including the container-limited processor count `HostFingerprint.Effective` substitutes for the spine mint's ambient host count. `Sweep` registers the equivalence cadence row on `WorkLane.Benchmark`.
- Receipt: every sweep run emits `TensorRun`/`ModelRun` receipts beside the persisted row; artifacts — chrome-trace profiles, BenchmarkDotNet exports, EP-context caches — admit as content-keyed `ArtifactIndexRow`s on the blob lane and ride the claim as typed `ProfileArtifact` cases, each carrying the same `ContentAddress` the index row holds so evidence joins its blob in one hop; the `ChromeTrace` case carries the `InferenceSession.ProfilingStartTimeNs` epoch beside it so a trace viewer aligns receipt-relative timestamps without re-opening the session.
- Packages: BenchmarkDotNet, NodaTime, Generator.Equals (`[Equatable]`+`[OrderedEquality]`/`[IgnoreEquality]` — the BenchmarkInput diff rail; `HostFingerprint.EqualityComparer` read off the spine declaration), LanguageExt.Core, Rasm.AppHost (project — the declared `HostFingerprint` this claim composes), Rasm.Persistence (project), BCL inbox
- Growth: a new performance surface is one claim row; a new claim dimension is one column on `BenchmarkClaim`; a new host dimension is one column at the AppHost declaration, never a Compute-side mirror; zero new surface.
- Boundary: SIMD routes, compression, partitioning, DATAS values, and numeric-provider ranks bind only behind a winning claim whose full fingerprint and input class match. `Provider` carries the numeric-lane key while `Substrate` remains the execution discriminant. `Stamps` includes the provider determinism tag, admitted package versions, device identity, and runtime posture; every mint on this page goes through `HostFingerprint.Effective` so `Processors` carries `CpuBudget.Total`, never the ambient host count the spine's own `Current` reads under a container limit. Shape, strides, batch, density, route, and tolerance participate in identity, preventing a contiguous micro-vector claim from winning for a strided batched tensor. Samples, warmups, mean, deviation, median, and P95 remain claim evidence while Persistence owns recency. `ProfileArtifact` is the ONE profile-evidence vocabulary — `ChromeTrace` from the inference `EndProfiling` run, `BenchmarkExport` from a BenchmarkDotNet exporter, `EpContext` from the session fleet compile — replacing the loose path-string columns on `ModelRun` and `Artifacts` alike; identity is the `ContentAddress` the blob index mints, never the on-disk path, so a moved or re-materialized file cannot fork evidence, and continuous profiles join by span identity through the `[03]-[TELEMETRY_PROJECTION]` trace correlation law, never as a fourth artifact case.

```csharp signature
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
[JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
[JsonDerivedType(typeof(ChromeTrace), "chrome-trace")]
[JsonDerivedType(typeof(BenchmarkExport), "benchmark-export")]
[JsonDerivedType(typeof(EpContext), "ep-context")]
public abstract partial record ProfileArtifact {
    private ProfileArtifact(ContentAddress content) => Content = content;

    public ContentAddress Content { get; }

    public sealed record ChromeTrace(ContentAddress Content, ulong StartNs) : ProfileArtifact(Content);

    public sealed record BenchmarkExport(ContentAddress Content, string Exporter) : ProfileArtifact(Content);

    public sealed record EpContext(ContentAddress Content, string Ep) : ProfileArtifact(Content);
}

// `HostFingerprint` is DECLARED at `Rasm.AppHost/Runtime/determinism#DETERMINISM_KERNEL` (the `tests/contracts/`
// `[02.15]-[HOST_FINGERPRINT]` minter) and composed here through this package's legal reference. A Compute-side
// declaration closed the S1-to-S3 cycle the branch acyclicity law forbids, so the two members only this domain can
// decide land as extensions: the container-limited processor count and the Persistence index admission. Neither
// spelling can live at the spine — `CpuBudget` and `ModelResultIndex` never cross downward.
public static class HostClaims {
    extension(HostFingerprint host) {
        public Option<BenchmarkRow> Claim(ModelResultIndex index, Seq<BenchmarkRow> rows) => index.Claim(rows, host.ToString());

        // One duration-forecast query, and the member `Runtime/admission#SUBSTRATE_AXIS` binds onto
        // `SelectionContext.Forecast`. Narrowing lands HERE because substrate and payload band live on the CLAIM — the
        // durable row key carries family, case, and route alone — while fingerprint match and recency stay
        // closed inside `ModelResultIndex.Claim`, so neither gate is re-implemented on the selection side. Claims
        // whose mint refuses drop out rather than forecasting off a row persistence would never hold.
        public Option<Duration> Forecast(ModelResultIndex index, Seq<BenchmarkClaim> claims, Substrate substrate, long payloadBytes) =>
            host.Claim(index, Banded(claims, substrate, BenchmarkClaim.BandOf(payloadBytes))).Map(static row => row.Median);
    }

    extension(HostFingerprint) {
        // `Effective` substitutes the admitted budget: the spine's own `Current` reads the ambient host count, which
        // over-reports every cgroup-limited container and would let a claim measured under 4 cores win a 64-core row.
        public static HostFingerprint Effective(FrozenDictionary<string, string> stamps, CpuBudget budget) =>
            HostFingerprint.Current(stamps) with { Processors = budget.Total };
    }

    static Seq<BenchmarkRow> Banded(Seq<BenchmarkClaim> claims, Substrate substrate, string band) =>
        claims.Filter(claim => claim.Substrate == substrate && StringComparer.Ordinal.Equals(claim.Input.Band, band))
            .Choose(static claim => claim.Persist().ToOption());
}

// `[Equatable]` is the claim-input DIFF rail: two admitted input classes compare member-wise and Inequalities
// names the axis that moved between two claim generations; the derived Rank/Contiguous projections are IGNORED
// so no member compares twice. `Key()` stays the identity spelling — the comparer never keys a store.
[Equatable]
public sealed partial record BenchmarkInput {
    private BenchmarkInput(long payloadBytes, string band, string dtype, Seq<long> shape, Seq<long> strides, int batch, double density) {
        PayloadBytes = payloadBytes;
        Band = band;
        Dtype = dtype;
        Shape = shape;
        Strides = strides;
        Batch = batch;
        Density = density;
    }

    public long PayloadBytes { get; }
    public string Band { get; }
    public string Dtype { get; }
    [OrderedEquality]
    public Seq<long> Shape { get; }
    [OrderedEquality]
    public Seq<long> Strides { get; }
    public int Batch { get; }
    public double Density { get; }
    [IgnoreEquality]
    public int Rank => Shape.Count;
    [IgnoreEquality]
    public bool Contiguous => ContiguousShape(Shape, Strides);

    public static Fin<BenchmarkInput> Admit(long payloadBytes, string dtype, Seq<long> shape, Seq<long> strides, int batch, double density) {
        Seq<string> violations =
            (payloadBytes < 0L ? Seq("payload") : Seq<string>())
            + (string.IsNullOrWhiteSpace(dtype) ? Seq("dtype") : Seq<string>())
            + (shape.IsEmpty || shape.Exists(static dimension => dimension <= 0L) ? Seq("shape") : Seq<string>())
            + (shape.Count != strides.Count || strides.Exists(static stride => stride <= 0L) ? Seq("strides") : Seq<string>())
            + (!ExtentFits(shape) ? Seq("extent") : Seq<string>())
            + (batch <= 0 ? Seq("batch") : Seq<string>())
            + (!double.IsFinite(density) || density is <= 0d or > 1d ? Seq("density") : Seq<string>());
        return violations.IsEmpty
            ? Fin.Succ(new BenchmarkInput(payloadBytes, BenchmarkClaim.BandOf(payloadBytes), dtype, shape, strides, batch, density))
            : Fin.Fail<BenchmarkInput>(new ComputeFault.PayloadOverBounds($"<benchmark-input-rejected:{string.Join(',', violations)}>"));
    }

    public string Key() =>
        string.Create(CultureInfo.InvariantCulture, $"{Band}|{Dtype}|{string.Join("x", Shape)}|{string.Join("x", Strides)}|{Batch}|{Density:R}");

    private static bool ExtentFits(Seq<long> shape) =>
        Try.lift(() => shape.Fold(1L, static (extent, dimension) => checked(extent * dimension))).Run()
            .Match(Succ: static _ => true, Fail: static _ => false);

    private static bool ContiguousShape(Seq<long> shape, Seq<long> strides) =>
        Try.lift(() => shape.Rev().Zip(strides.Rev())
            .Fold(
                (Expected: 1L, Valid: true),
                static (state, axis) => (checked(state.Expected * axis.Item1), state.Valid && axis.Item2 == state.Expected))
            .Valid).Run()
            .Match(Succ: static valid => valid, Fail: static _ => false);
}

public sealed record BenchmarkClaim {
    private BenchmarkClaim(
        BenchmarkInput input,
        Substrate substrate,
        BenchmarkFamily family,
        CacheToken @case,
        string route,
        string provider,
        Duration mean,
        Duration median,
        Duration p95,
        Duration stdDev,
        int samples,
        int warmups,
        long allocatedBytes,
        long operations,
        Option<UInt128> corpus,
        Option<string> artifactKey,
        double equivalenceMaxDeviation,
        string toleranceClass,
        HostFingerprint fingerprint,
        Seq<ProfileArtifact> artifacts,
        Instant at) {
        Input = input;
        Substrate = substrate;
        Family = family;
        Case = @case;
        Route = route;
        Provider = provider;
        Mean = mean;
        Median = median;
        P95 = p95;
        StdDev = stdDev;
        Samples = samples;
        Warmups = warmups;
        AllocatedBytes = allocatedBytes;
        Operations = operations;
        Corpus = corpus;
        ArtifactKey = artifactKey;
        EquivalenceMaxDeviation = equivalenceMaxDeviation;
        ToleranceClass = toleranceClass;
        Fingerprint = fingerprint;
        Artifacts = artifacts;
        At = at;
    }

    public BenchmarkInput Input { get; }
    public Substrate Substrate { get; }
    public BenchmarkFamily Family { get; }
    public CacheToken Case { get; }
    public string Route { get; }
    public string Provider { get; }
    public Duration Mean { get; }
    public Duration Median { get; }
    public Duration P95 { get; }
    public Duration StdDev { get; }
    public int Samples { get; }
    public int Warmups { get; }
    public long AllocatedBytes { get; }
    public long Operations { get; }
    public Option<UInt128> Corpus { get; }
    public Option<string> ArtifactKey { get; }
    public double EquivalenceMaxDeviation { get; }
    public string ToleranceClass { get; }
    public HostFingerprint Fingerprint { get; }
    public Seq<ProfileArtifact> Artifacts { get; }
    public Instant At { get; }

    public static readonly Seq<(string Band, long MaxBytes)> Bands = Seq(
        ("micro", 4L << 10),
        ("small", 256L << 10),
        ("medium", 16L << 20),
        ("large", long.MaxValue));

    public static string BandOf(long payloadBytes) =>
        Bands.Find(row => payloadBytes <= row.MaxBytes).Map(static row => row.Band).IfNone("large");

    public static ScheduleEntry Sweep(Func<IO<Unit>> work) =>
        new("compute-equivalence-sweep", new OccurrenceSpec.Every(Duration.FromDays(7)), DeadlineClass.SupportWindow, None, work);

    public static Fin<BenchmarkClaim> Admit(
        BenchmarkInput input,
        Substrate substrate,
        BenchmarkFamily family,
        CacheToken @case,
        string route,
        string provider,
        Duration mean,
        Duration median,
        Duration p95,
        Duration stdDev,
        int samples,
        int warmups,
        long allocatedBytes,
        long operations,
        Option<UInt128> corpus,
        Option<string> artifactKey,
        double equivalenceMaxDeviation,
        string toleranceClass,
        HostFingerprint fingerprint,
        Seq<ProfileArtifact> artifacts,
        Instant at) {
        Seq<string> violations =
            (family is null ? Seq("family") : Seq<string>())
            // CacheToken is a struct value object, so null is unrepresentable — the ghost is zero-init: a
            // default(CacheToken) bypasses the admission gate with a blank key member, and identity (Key/Persist)
            // embeds the case, so the outer seam reads the key member here.
            + (string.IsNullOrWhiteSpace((string)@case) ? Seq("case") : Seq<string>())
            + (string.IsNullOrWhiteSpace(route) ? Seq("route") : Seq<string>())
            + (string.IsNullOrWhiteSpace(provider) ? Seq("provider") : Seq<string>())
            + (mean < Duration.Zero || median < Duration.Zero || p95 < median || stdDev < Duration.Zero ? Seq("distribution") : Seq<string>())
            + (samples < 2 || warmups < 0 ? Seq("protocol") : Seq<string>())
            + (allocatedBytes < 0L ? Seq("allocation") : Seq<string>())
            + (operations < 1L ? Seq("operations") : Seq<string>())
            + (artifactKey.Map(static key => !string.IsNullOrWhiteSpace(key)).IfNone(true) ? Seq<string>() : Seq("artifact-key"))
            + (!double.IsFinite(equivalenceMaxDeviation) || equivalenceMaxDeviation < 0d ? Seq("equivalence") : Seq<string>())
            + (string.IsNullOrWhiteSpace(toleranceClass) ? Seq("tolerance") : Seq<string>())
            + (fingerprint.Processors <= 0 ? Seq("fingerprint") : Seq<string>())
            + (artifacts.Exists(static artifact => artifact.Switch(
                chromeTrace: static _ => false,
                benchmarkExport: static export => string.IsNullOrWhiteSpace(export.Exporter),
                epContext: static context => string.IsNullOrWhiteSpace(context.Ep))) ? Seq("artifact") : Seq<string>());
        return violations.IsEmpty
            ? Fin.Succ(new BenchmarkClaim(
                input, substrate, family, @case, route, provider, mean, median, p95, stdDev, samples, warmups,
                allocatedBytes, operations, corpus, artifactKey, equivalenceMaxDeviation, toleranceClass, fingerprint, artifacts, at))
            : Fin.Fail<BenchmarkClaim>(new ComputeFault.EquivalenceMiss($"<benchmark-claim-rejected:{string.Join(',', violations)}>"));
    }

    public string Key() => string.Create(CultureInfo.InvariantCulture,
        $"{Family.Key}|{(string)Case}|{Input.Key()}|{Substrate.Key}|{Route}|{Provider}|{ToleranceClass}");

    // This family owns the durable mint and its refusals, so the rail is the family's own — a claim admitted here
    // can still fail the row invariants persistence holds, and swallowing that leaves a forecast reading a row
    // no store would accept.
    public Fin<BenchmarkRow> Persist() => Family.Claim(
        Case, Route, Median, P95, AllocatedBytes, Operations, Corpus, ArtifactKey, Fingerprint.ToString(), At);

    // Generated comparer read: the spine declaration carries [Equatable] with its unordered Stamps roster, so
    // staleness is one structural compare and Digest comparisons stay their own axis.
    public bool Stale(HostFingerprint current) => !HostFingerprint.EqualityComparer.Default.Equals(Fingerprint, current);
}
```

## [06]-[HOOK_POINTS]

- Owner: `ComputeHookRail` — the five-point compute hook roster on the kernel hook capsule, one typed `HookPoint<TFact>` per point, declared once and mounted into the one `HookRegistry` at composition; `ConvergenceMark` — the solve-iteration evidence struct the replay point buffers.
- Cases: `Admit` `rasm.compute.runtime.admit` (Veto over `AdmittedIntent` — policy transform-or-reject before `Plan`) · `Dispatch` `rasm.compute.runtime.dispatch` (Observe over `SelectionReceipt` — substrate-keyed tap beside the `[03]-[TELEMETRY_PROJECTION]` dispatch span) · `Iteration` `rasm.compute.solve.iteration` (Replay over `ConvergenceMark`, depth 256 — a late UI subscriber drains the recent convergence window) · `Writeback` `rasm.compute.assessment.writeback` (Veto over `GraphDelta` — gate before the caller applies the assessment delta) · `Control` `rasm.compute.twin.control` (Veto over `TwinVerdict` — gate before the control suggestion crosses to the AppHost write-back as `ExternalValue`).
- Entry: `ComputeHookRail.Live()` mints the roster; `HookRegistry.Mount` at the composition root folds these points beside the AppHost `HookRail` rows into one frozen table, so a duplicate id dies structurally at composition and subscription reaches a point only through its declared rail field — a name-resolved lookup surface never exists.
- Auto: domain code fires evidence and observability subscribes — `Planned` runs the admit veto fold on the emitter's own rail before `SubstrateSelection.Plan` so the first refusing gate short-circuits with its typed fault and a transform threads forward; `Ran` fires the dispatch tap with the `SelectionReceipt` before `DispatchTable.Run` so the tap observes the identical evidence the `[03]-[TELEMETRY_PROJECTION]` span tags; `Marked` folds a `ConvergenceMark` into the bounded replay buffer through the same cadence gate the `rasm.compute.progress.cadence` law meters, so a hot solver never floods the buffer; `Applied` and `Suggested` run the writeback and control veto folds where the delta and the verdict leave the package.
- Receipt: none — a hook fire is the evidence event itself; the emitter's own receipt already carries the fact, and an instrument write for hook evidence subscribes as an observe tap on the mounted fan, never an emit call added in domain code.
- Packages: LanguageExt.Core, Thinktecture.Runtime.Extensions, Rasm (kernel signal capsule — `HookPoint<TFact>`, `HookId`, `HookModality`, `HookRegistry`, `IsolatedFault`), Rasm.Element (project — `GraphDelta`), BCL inbox.
- Growth: a new compute hook is one `HookPoint<TFact>` field on the rail record and one `Mount` row, its id admitted through the `HookId.Validate` four-segment grammar; a new subscriber is one `Observe`/`Veto` call at composition; zero new surface.
- Boundary: subscriber-fault isolation is the kernel capsule law composed whole — every observe delivery runs fork-shielded, so a throwing or failing tap parks as `IsolatedFault` on the roster's evidence cell and never touches the emitter's `Fire` result — a faulting UI subscriber structurally cannot fail a solve; a veto refusal is the point's contract, returned on the emitter's rail as the veto's own typed fault; payload types close at declaration — every `TFact` is a typed record already settled on its owning page, so a stringly payload cannot enter the rail; the rail adds no second emit path — `ReceiptSurface.Emit` stays the one sink leg and the `[03]-[TELEMETRY_PROJECTION]` fan stays the one instrument projection, hook taps observing evidence beside them; a tap that must never lose an event is a durable outbox consumer, never a hook subscriber; ids are registry-enforced unique, so two apps compose disjoint hook sets without collision.

```csharp signature
public readonly record struct ConvergenceMark(CorrelationId Correlation, string Physics, int Iteration, double Residual);

public sealed record ComputeHookRail(
    HookPoint<AdmittedIntent> Admit,
    HookPoint<SelectionReceipt> Dispatch,
    HookPoint<ConvergenceMark> Iteration,
    HookPoint<GraphDelta> Writeback,
    HookPoint<TwinVerdict> Control,
    Atom<Seq<IsolatedFault>> Faults) {
    public static ComputeHookRail Live() {
        var faults = Atom(Seq<IsolatedFault>());
        return new(
            new(HookId.Create("rasm.compute.runtime.admit"), HookModality.Veto, faults),
            new(HookId.Create("rasm.compute.runtime.dispatch"), HookModality.Observe, faults),
            new(HookId.Create("rasm.compute.solve.iteration"), HookModality.Replay, faults, depth: 256),
            new(HookId.Create("rasm.compute.assessment.writeback"), HookModality.Veto, faults),
            new(HookId.Create("rasm.compute.twin.control"), HookModality.Veto, faults),
            faults);
    }

    public Seq<IHookPoint> Points => Seq<IHookPoint>(Admit, Dispatch, Iteration, Writeback, Control);

    public Fin<Seq<SelectionReceipt>> Planned(AdmittedIntent admitted, SelectionContext context) =>
        Admit.Fire(admitted).Bind(gated => SubstrateSelection.Plan(gated, context));

    public IO<Unit> Ran(DispatchTable table, SelectionReceipt selection, AdmittedIntent admitted) =>
        IO.lift(() => ignore(Dispatch.Fire(selection))).Bind(_ => table.Run(selection, admitted));

    public Unit Marked(ConvergenceMark mark) => ignore(Iteration.Fire(mark));

    public Fin<GraphDelta> Applied(GraphDelta delta) => Writeback.Fire(delta);

    public Fin<TwinVerdict> Suggested(TwinVerdict verdict) => Control.Fire(verdict);
}
```

## [07]-[TS_PROJECTION]

- Owner: `ComputeReceiptKind`, `ComputeReceiptSpineWire`, `ComputeReceiptWire`, `ComputeReceiptEnvelopeWire`, `ProfileArtifactWire`, `BenchmarkClaimWire`, `ComputePanelWire`, `AlertSpecWire`, `CostVectorWire`, `ChargebackDatasetWire` — the receipt payload union, profile-evidence union, claim document with its subject union and band, descriptor rows, and chargeback rows as the dashboard and the composing app root consume them. `BenchmarkClaimWire.host` binds the AppHost-minted `HostFingerprintWire` (`tests/contracts/MANIFEST.md` `[02.15]-[HOST_FINGERPRINT]`) by import, never a mirrored declaration.
- Packages: BCL inbox
- Growth: a new receipt case lands as one payload row on `ComputeReceiptWire`; a new panel or alert axis lands as one field on its descriptor wire, and a new kernel `Sli`, `InstrumentKind`, `PanelKind`, `BurnRow`, or `AlertSeverity` row lands as one arm or key on the wire union mirroring it; zero new surface.
- Boundary: `ComputeReceiptKind` is a generated projection of `ReceiptSurface.Kinds` — emitted during the descriptor build and gated by the suite schema hash, never a hand-maintained mirror; payloads bind as `TPayload` through `ReceiptEnvelopeWire` with the envelope `kind` mirroring the payload discriminator; smart-enum spine fields cross as their key scalars, so `SliWire` spells one arm per kernel `[JsonDerivedType]` case and the `Saturation` polarity column crosses beside its bound — a union short an arm refuses a whole sink's alerts at the typed boundary, and a dropped polarity compiles every floor indicator as a ceiling breach; `long` values cross as invariant decimal strings through `Int64StringJsonConverter`, while `Instant` and `Duration` cross as ISO-8601 and roundtrip-pattern strings; `ProfileArtifactWire` mirrors the `ProfileArtifact` `[JsonDerivedType]` roster with `ContentAddress` crossing as its invariant hex string and `ulong StartNs` as a decimal string; absent evidence crosses as explicit null, never as an omitted member; the `[10]-[DASHBOARD_DESCRIPTOR]` descriptor rows and `[09]-[COST_LEDGER]` chargeback rows generate during the same descriptor build under the same schema hash, a panel row crossing its break keys beside its widget so the compile leg splits series without re-reading a meter it cannot reach, `UInt128` content keys cross as invariant hex strings through `UInt128HexJsonConverter`, the chargeback `tenant` mirrors the AppHost `TenantContextWire`, and a process-scoped chargeback row crosses its `route` as explicit null; `BenchmarkClaimWire` crosses as the one host-admitted document `tests/contracts/` `BENCHMARK_CLAIM` binds, so the fingerprint and the mint instant ride the document that one sweep produces, the subject union keeps the kernel coordinate off a bare probe row instead of widening every column to optional, each band rung crosses populated only where this sweep computes it and a peer grading an uncomputed rung refuses by axis rather than reading a fabricated value, and the distribution crosses as nanosecond numbers — the one carve on the `Duration` roundtrip-string law above, because a percentile ladder is arithmetic at every consumer.

```ts signature
type ReceiptScopeWire =
  | { kind: "execution"; correlation: string; lane: string; substrate: string; allocationClass: string; elapsed: string }
  | { kind: "process"; correlation: string; allocationClass: string };

// Spine PINS its own discriminator: each payload passes the literal its `[JsonDerivedType]` row declares, so that
// roster lives exactly once — in the payload set — and `ComputeReceiptKind` derives off the union below.
interface ComputeReceiptSpineWire<K extends string> { kind: K; scope: ReceiptScopeWire; }

type SelectionDecisionWire = { outcome: "chosen"; row: string } | { outcome: "rejected"; row: string; reason: string };
type SelectionModeWire = { mode: "ranked" } | { mode: "forced"; row: string };
interface SelectionWire extends ComputeReceiptSpineWire<"selection"> { decisions: SelectionDecisionWire[]; mode: SelectionModeWire; warmAffinity: boolean; }

interface TensorRunWire extends ComputeReceiptSpineWire<"tensor-run"> { family: string; dtype: string; elements: string; simdWidth: string; partitions: number; }

interface ModelLoadWire extends ComputeReceiptSpineWire<"model-load"> { modelChecksum: string; source: string; ep: string; version: string; }

interface WarmupWire extends ComputeReceiptSpineWire<"warmup"> { modelChecksum: string; ep: string; shape: string; }

type ProfileArtifactWire =
  | { kind: "chrome-trace"; content: string; startNs: string }
  | { kind: "benchmark-export"; content: string; exporter: string }
  | { kind: "ep-context"; content: string; ep: string };

interface ModelRunWire extends ComputeReceiptSpineWire<"model-run"> { modelChecksum: string; ep: string; mode: string; batchSize: number; peakBytes: string; arenaAllocator: string | null; profile: ProfileArtifactWire | null; }

interface RemoteCallWire extends ComputeReceiptSpineWire<"remote-call"> { transport: string; method: string; status: string; requestBytes: string; responseBytes: string; outcome: string; }

interface StreamSegmentWire extends ComputeReceiptSpineWire<"stream-segment"> { artifactId: string; segments: number; bytes: string; }

interface AllocationWire extends ComputeReceiptSpineWire<"allocation"> { event: string; requestedBytes: string; grantedBytes: string; detail: string | null; nativeAllocator: string | null; nativeReservedBytes: string | null; smallPoolFreeBytes: string | null; largePoolFreeBytes: string | null; }

interface CopyWire extends ComputeReceiptSpineWire<"copy"> { gate: string; bytes: string; device: string; }

interface CacheWire extends ComputeReceiptSpineWire<"cache"> { outcome: "hit" | "miss" | "store" | "evict"; key: string; bytes: string; }

interface UnitProjectionWire extends ComputeReceiptSpineWire<"unit-projection"> { family: string; originalUnit: string; originalValue: number; canonicalValue: number; }

interface BackpressureWire extends ComputeReceiptSpineWire<"backpressure"> { queueDepth: number; waited: string; dropped: string | null; }

interface DrainWire extends ComputeReceiptSpineWire<"drain"> { drained: number; faulted: number; refused: number; }

interface ConflictWire extends ComputeReceiptSpineWire<"conflict"> { subject: "retry-owner" | "contract-checksum"; evidence: string; }

interface RefusalWire extends ComputeReceiptSpineWire<"refusal"> { reason: string; subject: string; code: number; }

interface FactorizationWire extends ComputeReceiptSpineWire<"factorization"> { provider: string; decomposition: string; rows: number; cols: number; nnz: string; format: string; routeVariant: string | null; determinismTag: string | null; symbolicFill: number | null; residualCap: number | null; trueResidual: number | null; shards: number; shardNode: string | null; merged: boolean; }

interface GenerateWire extends ComputeReceiptSpineWire<"generate"> { modelChecksum: string; ep: string; modelType: string; mode: string; adapter: string | null; tokens: number; tokensPerSecond: number; guidanceKind: string; constrainedTokens: number; toolCalls: number; }

interface EmbeddingWire extends ComputeReceiptSpineWire<"embedding"> { modelChecksum: string; encoding: string; dimension: number; byteLength: string; }

interface DiscretizationWire extends ComputeReceiptSpineWire<"discretization"> { algorithm: string; element: string; nodes: string; elements: string; boundaryLayers: number; refineLevel: number; worstQuality: number; metric: string; }

interface SolveWire extends ComputeReceiptSpineWire<"solve"> { physics: string; method: string; dofs: string; iterations: number; residual: number; converged: boolean; shards: number; shardNode: string | null; merged: boolean; }

interface CouplingWire extends ComputeReceiptSpineWire<"coupling"> { scheme: string; fields: number; transfers: number; rounds: number; couplingResidual: number; converged: boolean; }

interface OptimizationWire extends ComputeReceiptSpineWire<"optimization"> { optimizer: string; generations: number; evaluations: number; surrogateHits: number; frontSize: number; hypervolume: number; }

interface SweepWire extends ComputeReceiptSpineWire<"sweep"> { gridPoints: string; completed: number; onFront: number; dominated: number; }

interface ClashWire extends ComputeReceiptSpineWire<"clash"> { indexKind: "bvh" | "octree" | "sdf"; candidates: number; hardClashes: number; clearanceViolations: number; totalPairs: number; }

interface TwinWire extends ComputeReceiptSpineWire<"twin"> { signalId: string; predicted: number; measured: number; residual: number; anomaly: boolean; controlDelta: number; }

interface UncertaintyWire extends ComputeReceiptSpineWire<"uncertainty"> { method: string; samples: number; mean: number | null; variance: number | null; skewness: number | null; kurtosis: number | null; quantiles: number[]; sobolFirst: number[]; sobolTotal: number[]; interaction: number[]; mostProbablePoint: number[]; fitQuality: number | null; residualStandardError: number | null; failureProbability: number; reliabilityIndex: number; }

interface FitWire extends ComputeReceiptSpineWire<"fit"> { family: string; method: string; parameters: string; iterations: number; residual: number; converged: boolean; quality: number; qualityMetric: string; retainedRank: number; }

interface GovernorWire extends ComputeReceiptSpineWire<"governor"> { cpuPercent: number; memoryPercent: number; workers: number; readerCeiling: number; partitionCap: number; memoryScale: number; spillPressure: boolean; }

interface DriftWire extends ComputeReceiptSpineWire<"drift"> { monitorId: string; statistic: string; level: number; limit: number | null; breach: boolean; window: number; }

interface AssessmentWire extends ComputeReceiptSpineWire<"assessment"> { discipline: string; route: string; key: string; verdict: string; governingRatio: number; admitted: boolean; phase: string | null; failureKind: string | null; transient: boolean; attempt: number; participation: number | null; combination: string | null; }

// Batch-worst error and cancellation channels cross as explicit null on a batch of fixed-order rows, where no
// route reported either — a zero would read as a measurement.
interface QuadratureWire extends ComputeReceiptSpineWire<"quadrature"> { domains: number; skipped: number; errorBound: number | null; conditioning: number | null; }

interface TrajectoryWire extends ComputeReceiptSpineWire<"trajectory"> { methodOrder: number; embeddedOrder: number | null; terminal: string; resolved: boolean; retryable: boolean; achieved: number; steps: number; rejects: number; rejectBudget: number; samples: number; lastError: number | null; }

// One case carries both sampling legs — an RQMC campaign and a fitted scattered field — so a column the serving
// leg never measured crosses as explicit null; a zero replicate count or a zero discrepancy reads as a measured
// net-quality figure and grades a fit against a bound nothing computed.
interface SamplingWire extends ComputeReceiptSpineWire<"sampling"> { family: string; dimensions: number; points: string; replicates: number | null; starDiscrepancy: number | null; worstProjection: number | null; }

type ComputeReceiptWire =
  | SelectionWire | TensorRunWire | ModelLoadWire | WarmupWire | ModelRunWire | RemoteCallWire | StreamSegmentWire
  | AllocationWire | CopyWire | CacheWire | UnitProjectionWire | BackpressureWire | DrainWire | ConflictWire | RefusalWire | FactorizationWire | GenerateWire | EmbeddingWire
  | DiscretizationWire | SolveWire | CouplingWire | OptimizationWire | SweepWire | ClashWire | TwinWire | UncertaintyWire | FitWire | GovernorWire | DriftWire | AssessmentWire
  | QuadratureWire | TrajectoryWire | SamplingWire;

// Derived off the payload union the descriptor build emits, never re-typed: a landed case widens the kind with no
// second list to edit, so the mirror that can silently go stale has nowhere to live.
type ComputeReceiptKind = ComputeReceiptWire["kind"];

type ComputeReceiptEnvelopeWire = ReceiptEnvelopeWire<ComputeReceiptWire>;

// `HostFingerprintWire` is the AppHost mint on the `host-fingerprint` seam, imported here. A second declaration
// beside the claim forks the frozen `[02.15]` column set the moment either side gains a column.

interface BenchmarkInputWire { payloadBytes: string; band: "micro" | "small" | "medium" | "large"; dtype: string; shape: string[]; strides: string[]; batch: number; density: number; rank: number; contiguous: boolean; }

type BenchmarkSubjectWire =
  | { subject: "probe" }
  | { subject: "kernel"; input: BenchmarkInputWire; substrate: string; family: string; case: string; route: string; provider: string; corpus: string | null; artifactKey: string | null; equivalenceMaxDeviation: number; toleranceClass: string; artifacts: ProfileArtifactWire[] };

interface BenchmarkAggregateWire { avg: number; min: number; max: number; total: number; }

type BenchmarkRungWire = "min" | "max" | "avg" | "p25" | "p50" | "p75" | "p95" | "p99" | "p999" | "stdDev";

interface BenchmarkBandWire { sampleCount: number; rungs: Partial<Record<BenchmarkRungWire, number>>; ticks: number | null; samples: number[] | null; gc: BenchmarkAggregateWire | null; heap: BenchmarkAggregateWire | null; counters: Record<string, number> | null; }

interface BenchmarkMetricWire { label: string; unit: string; modality: "fn" | "iter" | "yield"; subject: BenchmarkSubjectWire; band: BenchmarkBandWire; warmups: number | null; allocatedBytes: string | null; operations: string | null; }

interface BenchmarkClaimWire { suite: string; host: HostFingerprintWire; minted: string; metrics: BenchmarkMetricWire[]; }

type InstrumentKindWire = "count" | "delta" | "distribution" | "reading" | "total" | "balance" | "level" | "levels";
type PanelKindWire = "timeseries" | "stat" | "gauge" | "heatmap" | "logs" | "table" | "geomap" | "nodes";
interface ComputePanelWire { title: string; instrument: string; unit: string; measure: InstrumentKindWire; panel: PanelKindWire; by: string[]; bounds: number[]; }

type LevelBreachWire = "ceiling" | "floor";
type SliWire =
  | { sli: "ratio"; good: string; total: string }
  | { sli: "partition"; metric: string; by: string; good: string[] }
  | { sli: "latency"; metric: string; ceiling: string; quantile: number }
  | { sli: "saturation"; metric: string; bound: number; breach: LevelBreachWire }
  | { sli: "freshness"; metric: string; horizon: string };
type AlertSeverityWire = "page" | "ticket";
type BurnRowWire = "page-fast" | "page-slow" | "ticket-fast" | "ticket-slow";
interface AlertSpecWire {
  slug: string;
  burn: BurnRowWire;
  severity: AlertSeverityWire;
  sli: SliWire;
  target: number;
  spend: number;
  annotations: ReadonlyArray<{ key: string; value: string }>;
}

interface CostVectorWire { elapsedUnits: number; tokenUnits: number; byteUnits: number; remoteUnits: number; }
interface ChargebackRowWire { tenant: TenantContextWire; route: string | null; vector: CostVectorWire; facts: string; }
interface ChargebackDatasetWire { windowStart: string; windowEnd: string; rows: ChargebackRowWire[]; contentKey: string; }
```

## [08]-[COST_LEDGER]

- Owner: `CostPolicy` — the composition-admitted rate table pricing every measured fact; `CostVector` — the decomposed cost monoid; `ChargebackRow`/`ChargebackDataset` — the tenant-partitioned billing egress; the `[04]-[FOLD_PROJECTIONS]` journal folds and the `[03]-[TELEMETRY_PROJECTION]` `Charge` write consume this owner, so pricing has exactly one truth.
- Entry: `public static Fin<CostPolicy> CostPolicy.Admit(Seq<(Substrate Row, double SecondRate)> rates, double tokenRate, double stagedByteRate, double remoteNodeSecondRate)` — rates enter once at the composition root as external policy, admission proving `Substrate.Items` coverage exactly once per row and finite non-negative rates; `ChargebackDataset.Of(Instant windowStart, Instant windowEnd, Seq<(TenantContext Tenant, ComputeReceipt Fact)> journal, CostPolicy costs)` folds the envelope-joined journal into ordered per-`(tenant, route)` rows and mints the content key.
- Auto: `Price` folds one typed fact through the generated total `Switch`, so a new receipt case decides its cost axes at compile time or returns `CostVector.Zero` explicitly — elapsed seconds price by the scope substrate's rate, generated tokens by the token rate, granted staging bytes by the byte rate, and remote wall seconds by the node-second rate; the base elapsed term derives from the `Option` spine, so a process-scoped fact prices only its declared axes and fabricates no route.
- Receipt: none new — the ledger is a projection of the standing fact stream; `ChargebackDataset` rows are the billing truth and the `rasm.compute.cost.units` histogram is the lossy channel beside them.
- Packages: Thinktecture.Runtime.Extensions, Generator.Equals (`[Equatable]`+`[PrecisionEquality]` — the CostVector billing diff), LanguageExt.Core, NodaTime, System.IO.Hashing, Rasm (kernel signal capsule), BCL inbox
- Growth: a new cost axis is one `CostVector` field, one `CostPolicy` rate column, and the priced arms it touches — every untouched arm breaks loudly; a new rate posture is one admitted policy value at composition; zero new surface.
- Boundary: no rate literal lives in the package — `Admit` is the only mint and the composition root supplies the rows; the dataset content key folds window, tenant slug, route, vector lanes, and fact counts through the one `XxHash128` identity path so a re-derived dataset over identical evidence re-keys identically; grouping composes the BCL `AggregateBy` keyed fold, ordering is ordinal by slug then route so the key is order-stable; the envelope `Tenant` that `TenantContext.Stamp` promotes onto every registered mirror store is the same partition this ledger folds — the estate baggage-attribution law (`libs` `[COST_ATTRIBUTION_BAGGAGE]`) consumes this dataset, and a second attribution stream beside the receipt rail is the rejected form; the content-keyed `ChargebackDataset` projects columnar for the billing lake through the one `ArrowBatch.Chargeback` owner — the same `RecordBatch` construction path `Solver/sweep` `DoeDataset` folds — never a second columnar encoder.

```csharp signature
// `[Equatable]`+`[PrecisionEquality]` is the billing DIFF rail: two chargeback folds compare banded at the
// accumulation noise floor and Inequalities names the axis that moved. Every member is precision-banded, so
// GetHashCode covers nothing — a CostVector is NEVER a dictionary key; it rides HashMap VALUES alone.
[Equatable]
public readonly partial record struct CostVector(
    [property: PrecisionEquality(1e-9)] double ElapsedUnits,
    [property: PrecisionEquality(1e-9)] double TokenUnits,
    [property: PrecisionEquality(1e-9)] double ByteUnits,
    [property: PrecisionEquality(1e-9)] double RemoteUnits) {
    public static readonly CostVector Zero = new(0d, 0d, 0d, 0d);

    public double Total => ElapsedUnits + TokenUnits + ByteUnits + RemoteUnits;

    public static CostVector operator +(CostVector left, CostVector right) =>
        new(left.ElapsedUnits + right.ElapsedUnits,
            left.TokenUnits + right.TokenUnits,
            left.ByteUnits + right.ByteUnits,
            left.RemoteUnits + right.RemoteUnits);
}

public sealed record CostPolicy {
    private CostPolicy(FrozenDictionary<Substrate, double> secondRates, double tokenRate, double stagedByteRate, double remoteNodeSecondRate) {
        SecondRates = secondRates;
        TokenRate = tokenRate;
        StagedByteRate = stagedByteRate;
        RemoteNodeSecondRate = remoteNodeSecondRate;
    }

    public FrozenDictionary<Substrate, double> SecondRates { get; }

    public double TokenRate { get; }

    public double StagedByteRate { get; }

    public double RemoteNodeSecondRate { get; }

    public static Fin<CostPolicy> Admit(Seq<(Substrate Row, double SecondRate)> rates, double tokenRate, double stagedByteRate, double remoteNodeSecondRate) {
        Seq<string> violations =
            (rates.Count != Substrate.Items.Count || rates.Map(static rate => rate.Row).ToFrozenSet().Count != Substrate.Items.Count ? Seq("coverage") : Seq<string>())
            + (rates.Exists(static rate => !double.IsFinite(rate.SecondRate) || rate.SecondRate < 0d) ? Seq("second-rate") : Seq<string>())
            + (!double.IsFinite(tokenRate) || tokenRate < 0d ? Seq("token-rate") : Seq<string>())
            + (!double.IsFinite(stagedByteRate) || stagedByteRate < 0d ? Seq("byte-rate") : Seq<string>())
            + (!double.IsFinite(remoteNodeSecondRate) || remoteNodeSecondRate < 0d ? Seq("remote-rate") : Seq<string>());
        return violations.IsEmpty
            ? Fin.Succ(new CostPolicy(
                rates.ToFrozenDictionary(static rate => rate.Row, static rate => rate.SecondRate),
                tokenRate, stagedByteRate, remoteNodeSecondRate))
            : Fin.Fail<CostPolicy>(ComputeFault.Create($"<cost-policy-rejected:{string.Join(',', violations)}>"));
    }

    public CostVector Price(ComputeReceipt fact) =>
        new CostVector(
            ElapsedUnits: fact.Substrate.Bind(route => fact.Elapsed.Map(elapsed => elapsed.TotalSeconds * SecondRates[route])).IfNone(0d),
            TokenUnits: 0d,
            ByteUnits: 0d,
            RemoteUnits: 0d)
        + fact.Switch(
            state: this,
            selection: static (_, _) => CostVector.Zero,
            tensorRun: static (_, _) => CostVector.Zero,
            modelLoad: static (_, _) => CostVector.Zero,
            warmup: static (_, _) => CostVector.Zero,
            modelRun: static (_, _) => CostVector.Zero,
            remoteCall: static (policy, call) => call.Elapsed
                .Map(elapsed => CostVector.Zero with { RemoteUnits = elapsed.TotalSeconds * policy.RemoteNodeSecondRate })
                .IfNone(CostVector.Zero),
            streamSegment: static (_, _) => CostVector.Zero,
            allocation: static (policy, staged) => staged.GrantedBytes > 0L
                ? CostVector.Zero with { ByteUnits = staged.GrantedBytes * policy.StagedByteRate }
                : CostVector.Zero,
            copy: static (_, _) => CostVector.Zero,
            cache: static (_, _) => CostVector.Zero,
            unitProjection: static (_, _) => CostVector.Zero,
            backpressure: static (_, _) => CostVector.Zero,
            drain: static (_, _) => CostVector.Zero,
            conflict: static (_, _) => CostVector.Zero,
            refusal: static (_, _) => CostVector.Zero,
            factorization: static (_, _) => CostVector.Zero,
            generate: static (policy, run) => CostVector.Zero with { TokenUnits = run.Tokens * policy.TokenRate },
            embedding: static (_, _) => CostVector.Zero,
            discretization: static (_, _) => CostVector.Zero,
            solve: static (_, _) => CostVector.Zero,
            coupling: static (_, _) => CostVector.Zero,
            optimization: static (_, _) => CostVector.Zero,
            sweep: static (_, _) => CostVector.Zero,
            clash: static (_, _) => CostVector.Zero,
            twin: static (_, _) => CostVector.Zero,
            uncertainty: static (_, _) => CostVector.Zero,
            fit: static (_, _) => CostVector.Zero,
            governor: static (_, _) => CostVector.Zero,
            drift: static (_, _) => CostVector.Zero,
            assessment: static (_, _) => CostVector.Zero,
            // Both integration legs are managed CPU folds, so the substrate-rated elapsed prefix prices them whole
            // and neither carries a token, staged-byte, or remote-node charge of its own. Sampling campaigns share the
            // shape: the point count is a workload figure, never a billed unit, so pricing a draw beside the
            // elapsed prefix charges one run twice on the one axis its substrate rate already covers.
            quadrature: static (_, _) => CostVector.Zero,
            trajectory: static (_, _) => CostVector.Zero,
            sampling: static (_, _) => CostVector.Zero);
}

public sealed record ChargebackRow(TenantContext Tenant, Option<Substrate> Route, CostVector Vector, long Facts);

public sealed record ChargebackDataset(Instant WindowStart, Instant WindowEnd, Seq<ChargebackRow> Rows, UInt128 ContentKey) {
    public static ChargebackDataset Of(Instant windowStart, Instant windowEnd, Seq<(TenantContext Tenant, ComputeReceipt Fact)> journal, CostPolicy costs) {
        Seq<ChargebackRow> rows = toSeq(journal
            .AggregateBy(
                static row => (row.Tenant, Route: row.Fact.Substrate),
                (Vector: CostVector.Zero, Facts: 0L),
                (held, row) => (held.Vector + costs.Price(row.Fact), held.Facts + 1L))
            .Select(static slot => new ChargebackRow(slot.Key.Tenant, slot.Key.Route, slot.Value.Vector, slot.Value.Facts))
            .OrderBy(static row => row.Tenant.Slug, StringComparer.Ordinal)
            .ThenBy(static row => row.Route.Map(static route => route.Key).IfNone("process"), StringComparer.Ordinal));
        return new ChargebackDataset(windowStart, windowEnd, rows, Keyed(windowStart, windowEnd, rows));
    }

    private static UInt128 Keyed(Instant start, Instant end, Seq<ChargebackRow> rows) =>
        XxHash128.HashToUInt128(Encoding.UTF8.GetBytes(string.Create(CultureInfo.InvariantCulture,
            $"{start}|{end}|{string.Join(';', rows.Map(static row =>
                $"{row.Tenant.Slug}|{row.Route.Map(static route => route.Key).IfNone("process")}|{row.Vector.ElapsedUnits:R}|{row.Vector.TokenUnits:R}|{row.Vector.ByteUnits:R}|{row.Vector.RemoteUnits:R}|{row.Facts}"))}")));
}
```

## [09]-[DASHBOARD_DESCRIPTOR]

- Owner: `FactSelector`, `ComputeObjective`, `PanelRow`, `ComputeDescriptors` — the typed dashboard-and-alert contribution the composing app root encodes into provisioned boards and rule groups; data rows, never rendered JSON, and never a deploy-plane import. Indicator shapes, the burn table, severity routing, the verdict fold, the alert spec, and both descriptor carriers — `PanelSpec` and `BoardPack` — are the kernel SLO algebra composed whole, so `PanelRow` projects the pack outward and mints no second panel truth.
- Entry: `ComputeDescriptors.Board` — the kernel `BoardPack` carrying one `PanelSpec` per `[02]-[RECEIPT_UNION]` `InstrumentSpec` beside every objective; `ComputeDescriptors.Panels` — the wire projection of that pack, each row carrying its title, break keys, widget, unit, and bucket edges; `ComputeDescriptors.Alerts` — the pack's compilation-ready specs; `ComputeObjective.Verdict` — the in-process burn verdict over a windowed fact slice. Admission is the kernel pack's whole and reaches the composing root on `[02]-[RECEIPT_UNION]`'s contributor port, so this owner exposes no probe entry.
- Auto: panels derive from `ReceiptSurface.Specs`, so descriptor truth structurally cannot drift from the mounted roster — a new spec row is a new panel with zero descriptor edit — and each panel's break keys are its declaring row's `Dimensions`, so the tag vocabulary a writer spells is the vocabulary the board splits on; each objective binds a mounted counter pair as its ratio indicator and a typed selector pair as its in-process sampler, so the store-side burn rule and the live verdict divide the same evidence; the four multiwindow burn rows, both severities, the budget share, and every annotation derive from the kernel table, so a factor change moves verdict, alert, and dashboard in one edit at one owner.
- Receipt: none — the descriptor is a projection of the spec roster and the fact vocabulary; a hand-authored board beside it is the drift the projection deletes.
- Packages: LanguageExt.Core, NodaTime, Rasm (kernel signal capsule), BCL inbox
- Growth: a new panel is the `[02]-[RECEIPT_UNION]` spec row it derives from; a panel wanting a non-default widget or a narrower break set overrides on its own `PanelSpec` row; a new objective is one `Bound<TCase>` row naming its scored receipt case, indicator series, target, and breach predicate; a fifth indicator shape is one kernel `Sli` case breaking every consumer at compile time; zero new surface.
- Boundary: descriptor rows emit during the descriptor build under the suite schema hash beside `ReceiptSurface.Kinds` and cross on the `[08]-[TS_PROJECTION]` wires; the ts-iac compile leg (`typescript:iac` `[0014]`) owns turning rows into Foundation-SDK dashboards and rule groups — Compute owns no IaC surface and renders nothing; selectors are typed case predicates rather than payload field matchers, so they never cross the wire, a field rename fails the build instead of quietly sampling nothing, and an unregistered case fails where the selector is constructed rather than at a boot probe restating it; `BoardPack.Admit` carries every claim this pack owes — panel widgets and break keys, indicator series and partition keys, and objective-name distinctness across the alert namespace — so an alert can never name a series the meter never mounts, a panel can never break on a key its row never declares, and a folder-local probe restating any of them is the deleted form; three further proofs are structural and probing them tests nothing, since the `Advised` factory is the only path carrying bounds and `Distribution` the only path without, `FactSelector.Of` resolves its kind through the frozen registry so an objective naming an unregistered case has no construction path, and `ComputeObjective.Of` mints the population-and-breach pair off ONE type argument behind a private constructor so a pair spanning two cases — whose breach then samples a population it can never intersect and reports a permanent zero — has none either; a hand-typed window, factor, or severity beside the kernel table is the forked form that silently diverges from every sibling descriptor plane on the next tuning.

```csharp signature
// Typed case selection replaces a stringly field matcher: the population IS a receipt case and the predicate
// reads that case's own fields, so a renamed payload field breaks at compile time instead of silently
// selecting nothing, and the kind travels only so the boot probe can prove the case is on the wire roster.
public sealed record FactSelector(string Kind, Func<ComputeReceipt, bool> Holds) {
    public static FactSelector Of<TCase>(Func<TCase, bool>? holds = null) where TCase : ComputeReceipt =>
        new(ReceiptSurface.KindOf(typeof(TCase)), fact => fact is TCase held && (holds is null || holds(held)));

    public long Count(Seq<ComputeReceipt> facts) => facts.Filter(Holds).Count;
}

// Compute's evidence plane is its own fact stream, so the sampler is a selector pair while the burn windows,
// factors, severity routing, budget share, and spec derivation stay the kernel's single discipline.
public sealed record ComputeObjective {
    private ComputeObjective(Objective objective, FactSelector population, FactSelector breach) =>
        (Objective, Population, Breach) = (objective, population, breach);

    public Objective Objective { get; }

    public FactSelector Population { get; }

    public FactSelector Breach { get; }

    // ONE type argument mints BOTH views, so population and breach are two predicates over one receipt case by
    // construction: a pair naming two cases samples a breach against a population it can never intersect and
    // reports a permanent zero — a green indicator over an unmeasured objective, the worst reading this plane can
    // produce. The private constructor leaves this the only path, so the mismatch has no construction site rather
    // than a runtime refusal restating it — the third structural proof beside the bounds factory and the frozen
    // kind registry. `within` narrows the population when an objective scores a slice of its case.
    public static ComputeObjective Of<TCase>(
        Objective objective, Func<TCase, bool> breached, Func<TCase, bool>? within = null)
        where TCase : ComputeReceipt =>
        new(objective, FactSelector.Of<TCase>(within), FactSelector.Of<TCase>(breached));

    // Breaching filters the ALREADY-filtered population, so the sample's Breaching <= Total claim holds by
    // construction at the one seam that mints it and no consumer re-proves it.
    public SloSample Sample(Seq<ComputeReceipt> window) {
        Seq<ComputeReceipt> total = window.Filter(Population.Holds);
        return new SloSample(Breaching: Breach.Count(total), Total: total.Count);
    }

    public SloVerdict Verdict(Func<Duration, Seq<ComputeReceipt>> window) =>
        Slo.Evaluate(Objective, row => new BurnReading(Long: Sample(window(row.Long)), Short: Sample(window(row.Short))));

    public Seq<AlertSpec> Specs => Slo.Specs(Objective);
}

// Wire projection of one pack panel: the kernel `PanelSpec` carries the policy half — which instrument, broken
// on which keys, under which widget — and these columns carry the instrument facts a renderer needs beside it,
// so the deploy plane renders from one row and resolves nothing against a meter it cannot reach.
public sealed record PanelRow(
    string Title, string Instrument, string Unit, InstrumentKind Measure, PanelKind Panel, Seq<string> By, ImmutableArray<double> Bounds);

public static class ComputeDescriptors {
    public static readonly Seq<ComputeObjective> Objectives = Seq(
        Bound<ComputeReceipt.Solve>("compute.solve-convergence",
            new Sli.Ratio(ReceiptSurface.SolveConverged, ReceiptSurface.SolveRuns), 0.99d,
            static solve => !solve.Converged),
        Bound<ComputeReceipt.RemoteCall>("compute.remote-call",
            new Sli.Ratio(ReceiptSurface.RemoteOk, ReceiptSurface.RemoteCalls), 0.999d,
            static call => !StringComparer.Ordinal.Equals(call.Status, ReceiptSurface.OkStatus)),
        Bound<ComputeReceipt.Backpressure>("compute.backpressure",
            new Sli.Ratio(ReceiptSurface.BackpressureAdmitted, ReceiptSurface.BackpressureVerdicts), 0.999d,
            static shed => shed.Dropped is not null),
        Bound<ComputeReceipt.Twin>("compute.twin-anomaly",
            new Sli.Ratio(ReceiptSurface.TwinNominal, ReceiptSurface.TwinVerdicts), 0.95d,
            static twin => twin.Anomaly),
        Bound<ComputeReceipt.Trajectory>("compute.trajectory-resolution",
            new Sli.Ratio(ReceiptSurface.TrajectoryResolved, ReceiptSurface.TrajectoryRuns), 0.99d,
            static run => !run.Resolved));

    // One derivation from the spec roster: the kernel policy row and its wire projection are two reads of the
    // same pair, so a panel cannot sit on the pack and be missing from the wire or carry a different widget on
    // each side. Every panel breaks on its declaring row's own `Dimensions`, so the break vocabulary IS the
    // declaration and a hand-kept break list beside it has nothing to hold.
    static readonly Seq<(PanelSpec Panel, InstrumentSpec Row)> Descriptors =
        ReceiptSurface.Specs.Map(static row => (PanelSpec.Of(row.Description, row.Name, [.. row.Dimensions]), row)).Strict();

    // Panels and objectives travel as one kernel pack, so a roster change re-derives panels, alerts, and the
    // whole admission proof in one diff and no descriptor plane re-mints a panel carrier.
    public static readonly BoardPack Board = new(
        Wire: "compute.receipt", // the provenance key the deploy tuple admits this projection under; pack and key are one value
        Panels: Descriptors.Map(static entry => entry.Panel).Strict(),
        Objectives: Objectives.Map(static row => row.Objective).Strict());

    public static Seq<AlertSpec> Alerts => Board.Alerts;

    public static Seq<PanelRow> Panels =>
        Descriptors.Map(static entry => new PanelRow(
            entry.Panel.Title, entry.Panel.Instrument, entry.Row.Unit, entry.Row.Kind,
            entry.Panel.Widget.IfNone(PanelKind.For(entry.Row.Kind)), entry.Panel.By, entry.Row.Bounds.IfNone([]))).Strict();

    // Omitting the window canonicalizes it at the kernel to the estate compliance default, so no calendar
    // literal lands in a descriptor row and a shortened window still refuses below the longest burn row. The
    // scored case is the ONE type argument every row states, so the objective's two selectors cannot name two cases.
    static ComputeObjective Bound<TCase>(
        string name, Sli sli, double target, Func<TCase, bool> breached, Func<TCase, bool>? within = null)
        where TCase : ComputeReceipt =>
        ComputeObjective.Of(Objective.Create(name: name, sli: sli, target: target, window: default), breached, within);
}
```

## [10]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
