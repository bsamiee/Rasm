# [COMPUTE_UNION]

`ComputeReceipt` is the package's only fact vocabulary for measured execution: every operational view folds over that one stream, one traversal answers both the instrument writes and the priced cost a fact owes, and the certification replay re-derives a stored payload against its own determinism class. Fingerprint-gated claims live at `Runtime/claims#CLAIM_ROW`, the tenant cost ledger at `Runtime/ledger#CHARGEBACK_EGRESS`, and the dashboard-and-hook descriptor at `Runtime/board#PANEL_PROJECTION` — three projections of this one stream, no second fact truth.

Kernel vocabulary arrives whole from the signal capsule — the causal frame, the instrument declaration mechanism, the trace band, and the tenancy primitive, each family's member roster owned at the capsule.

Several cases declare elsewhere — `Assessment` on the `Analysis/assessment` spine, `Quadrature` and `Trajectory` on the `Tensor/quadrature` integration lane, `Sampling` on the `Tensor/sampling` low-discrepancy lane — each a `partial` its owning page seats on this union while its `[JsonDerivedType]` row and wire projection stay here, so `ComputeWireContext` round-trips the whole union through the one Strict resolver: polymorphic `kind` discriminator, Thinktecture key scalars, `Seq<string>` intact, `UnmappedMemberHandling.Disallow` refusing drift at the edge.

## [01]-[INDEX]

- [02]-[RECEIPT_UNION]: Fact union (inline cases beside the `Analysis/assessment` and `Tensor/quadrature` partials), its closed payload vocabularies, its Strict-resolver round-trip context, the instrument roster, and sink-port emission.
- [03]-[TELEMETRY_PROJECTION]: The ONE receipt fold answering instrument writes and priced cost together, and the dispatch span spine over the kernel trace band.
- [04]-[FOLD_PROJECTIONS]: Operational views derive as folds over the fact stream; content-keyed verdicts re-derive and diff under the determinism stamp.
- [05]-[TS_PROJECTION]: Receipt payload union and envelope wire shapes.

## [02]-[RECEIPT_UNION]

- Owner: `ReceiptScope`, `SelectionDecision`, `SelectionMode`, `ShardRole`, `CacheOutcome`, `ConflictSubject`, `BackpressureVerdict`, `DeterminismTag`, `DeterminismStamp`, `ConstitutiveEvidence`, `ContactEvidence`, `ComputeReceipt`, `InvariantTextJsonConverter<T>`, `ComputeWireContext`, `ComputeInstrument`, `ReceiptSurface` — the scope and selection evidence families, the closed payload vocabularies the union's cases key on, the fact union itself, the strict serializer context, the Compute instrument roster over the kernel declaration mechanism, and the sink-bound emission surface.
- Cases: selection · tensor-run · model-load · warmup · model-run · remote-call · stream-segment · allocation · copy · cache · unit-projection · backpressure · drain · conflict · refusal · factorization · generate · embedding · discretization · solve · coupling · optimization · sweep · clash · twin · uncertainty · fit · governor · drift · assessment · quadrature · trajectory · sampling (the last four declared as partials on this owner — assessment by `Analysis/assessment`, the integration pair by `Tensor/quadrature`, sampling by `Tensor/sampling`)
- Entry: `public IO<ReceiptEnvelope> ReceiptSurface.Emit(ComputeReceipt fact)` — the surface binds sink, serializer, mounted instruments, and the rate table once at composition; `IO` carries the sink effect and returns the `ReceiptEnvelope` evidence.
- Auto: wire kind derives from the polymorphic metadata pinned on the union; the HLC stamp and `SkewBound` derive inside `Send`, and `Emit` reads `TenantContext.Current` exactly once — the same tenant prices the fact through the `[03]-[TELEMETRY_PROJECTION]` fold and rides into `Send` so the `ReceiptEnvelope` `Tenant` field partitions evidence by the kernel tenancy primitive; instrument rows register once at composition through `TelemetryContributorPort`, which carries the `Runtime/board#PANEL_PROJECTION` pack beside them so the composing root proves board and objectives against the set it mounts and this folder ships no probe entry; `Emit` folds every typed fact through the one measurement fold before serialization, and the `[03]-[TELEMETRY_PROJECTION]` `ComputeTraces` spine opens the dispatch span through the kernel `SpanBand` at the admitted `Dispatch` scope so receipt correlation joins the OTel rail with zero call-site ceremony.
- Receipt: union cases materialize at the sink edge only; hot-path capsules upstream stay allocation-free.
- Packages: Thinktecture.Runtime.Extensions, Thinktecture.Runtime.Extensions.Json, Riok.Mapperly (`AllocationMapper` — the one generated evidence lowering under `RequiredMappingStrategy.Both`), LanguageExt.Core, NodaTime, Rasm (project, kernel signal capsule), Rasm.AppHost (project), BCL inbox
- Growth: a new measured concern is one case row on `ComputeReceipt`, one `[JsonDerivedType]` row, one TS payload row, and one `[03]-[TELEMETRY_PROJECTION]` fold arm, zero new surface; a rail in another folder declares its case as a `partial` record on this owner while this owning index keeps the `[JsonDerivedType]` registration and the TS payload row, so the polymorphic registry stays single-sited.
- Growth: the `[JsonDerivedType]` roster is the ONE case correspondence — `ReceiptSurface.Kinds` projects it from the context's polymorphism metadata, the TS `ComputeReceiptKind` union generates from `Kinds` during descriptor emit under the suite schema hash, and `ReceiptSurface.Probe` proves roster-versus-case bijection at boot, so a parallel receipt union, a second discriminator registry, or a hand-maintained TS mirror that can silently go stale is the deleted form.
- Growth: `ComputeInstrument` is the ONE instrument correspondence — each row IS its own kernel `InstrumentSpec` carrying name, kind, measurement form, unit, description, dimension slots, and bounds, `Rows` derives from `Items`, and the mounted `InstrumentSet` and the `Runtime/board#PANEL_PROJECTION` panel projection both read that derivation, so a folder-local spec record, a parallel name-const roster, or a second panel-truth list is the deleted form.
- Law: a write addresses its ROW, never a name — the kernel write plane takes an `InstrumentSpec` (`Rasm/Domain/instrument#SPEC`), so the const-name roster and the hand-listed spec sequence that mirrored it collapse into one declaration whose constructor proves each row's name against its own key. NAMED LOSS: the name and the key state the same text twice at every row; the gain is that the pairing is proved at type init rather than by inspection, which is the kernel `KernelInstrument` form composed verbatim.
- Boundary: receipts are HLC-correlated through the `ReceiptEnvelope` and emit only through the sink-bound `ReceiptSurface`. `ReceiptScope.Execution` carries lane, substrate, allocation, and elapsed evidence, while `Process` carries only correlation and allocation; process facts never fabricate execution context or bypass the union. Every solver, statistical-learning, generative, residency, allocation, governance, and monitor-drift outcome rides this union. `Selection` projects ordered hops onto `SelectionDecision` and forced presence onto `SelectionMode`, avoiding parallel rosters and nullable policy. `Allocation` carries the complete `AllocationEvidence` projection, including typed `StagingEventKind`, requested/granted bytes, the typed grant `Lifetime` a dispose closes with, detail, allocator, reservation, and pool gauges. DECLARED REFUSAL: kernel-interior evidence — the quadrature witness, the trajectory march, the sampling replicate spread — rides its owning route's receipt columns and mints NO standalone union case, because a case demands the `(Lane, Substrate)` execution spine no interior fold carries (the same law that refuses a whole-graph `JobReceipt`).
- Boundary: ABSENCE is an `Option<T>` column on every case and never a nullable slot — the context registers `LanguageExtJsonConverterFactory` and declines the suite's `OmitAbsent` modifier, so an `Option<T>` slot stays PRESENT on the wire as an explicit null and the `| null` unions the TS mirrors spell are the agreement. A nullable column past this boundary is the deleted form, and the `Option`→nullable lowering codec that existed to serve one Mapperly seam goes with it.
- Boundary: a CLOSED payload vocabulary is a row set, never a `string` — `CacheOutcome`, `ConflictSubject`, `DeterminismTag`, and `BackpressureVerdict` name their own members, and `Clash` reads the `Solver/clash#CLASH_AND_TWIN` `AccelerationKind` owner rather than re-spelling its keys. `RemoteCall.Status` stays free text under a named discriminant: it is a PEER's status name crossing the boundary, whose row set no Compute owner declares, so `ReceiptSurface.OkStatus` is the one spelling the ok-test compares against.
- Boundary: a decomposed execution rides ONE `ShardRole` case — `Whole`, `Shard(Of, Node)`, or `Merge(Of)` — so a merge receipt naming a shard node is unrepresentable where three loose columns made eight states of a three-state axis. Modal effective-mass fractions ride ONE `Option<ModalParticipation>` triple — the `Solver/contract#SOLVE_REQUEST` carrier composed here and never re-declared, so this column and the `SolveResult.EffectiveMassShare` fold that mints it are one type — and a route cannot report two axes and leave the seismic gate reading a fabricated share on the third.
- Boundary: `Refusal` is process-scoped interior evidence and carries the originating `ComputeFault.Identity`; `Subject` carries high-cardinality detail that no meter fans on.
- Boundary: `Optimization.ReferenceDerived` and `Clash.Truncated` are declared columns whose producer write is the owning lane's — `Solver/optimizer` and `Solver/clash#CLASH_AND_TWIN` `ClashSurvey.Truncated` respectively — so the receipt states the fact and the mint site fills it; a column the union declares and no lane writes reads as a measured false at every consumer.
- Boundary: spine values serialize as Thinktecture key scalars and format without runtime format strings.

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

// The ONE decomposed-execution axis both numeric cases share. A merge receipt folds shard results and executes no
// shard of its own, so `Executes` is the discriminant the shard counter reads and `Merge` carrying a farm node is
// unrepresentable — the three loose columns this replaces admitted a merged receipt with a node set, and every
// convergence ratio over the stream then read a population the run never had.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
[JsonPolymorphic(TypeDiscriminatorPropertyName = "role")]
[JsonDerivedType(typeof(Whole), "whole")]
[JsonDerivedType(typeof(Shard), "shard")]
[JsonDerivedType(typeof(Merge), "merge")]
public abstract partial record ShardRole {
    private ShardRole() { }
    public sealed record Whole : ShardRole;
    public sealed record Shard(int Of, string Node) : ShardRole;
    public sealed record Merge(int Of) : ShardRole;

    public int Count => Switch(
        whole: static _ => 1,
        shard: static row => row.Of,
        merge: static row => row.Of);

    public bool Executes => this is not Merge;
}

// A material-nonlinear route runs a return map per integration point; a contact route solves an active set.
// Neither runs on a linear solve, so each is ONE optional measurement group rather than loose scalars a reader
// could half-fill. Producers: `Solver/constitutive#CONSTITUTIVE` `ConstitutiveResult.ModelKey`/
// `ReturnMapIterations`/`ReturnMapResidual`, and `ContactResult.ActiveSet`/`PenetrationResidual`/`Multipliers`.
[StructLayout(LayoutKind.Auto)]
public readonly record struct ConstitutiveEvidence(string Model, int ReturnMapIterations, double ReturnMapResidual);

[StructLayout(LayoutKind.Auto)]
public readonly record struct ContactEvidence(int ActiveSet, double PenetrationResidual, int Multipliers);

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CacheOutcome {
    public static readonly CacheOutcome Hit = new("hit");
    public static readonly CacheOutcome Miss = new("miss");
    public static readonly CacheOutcome Store = new("store");
    public static readonly CacheOutcome Evict = new("evict");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ConflictSubject {
    public static readonly ConflictSubject RetryOwner = new("retry-owner");
    public static readonly ConflictSubject ContractChecksum = new("contract-checksum");
}

// The queue gate's verdict is a CASE, not a nullable reason beside a boolean: an admitted verdict has no reason to
// carry and a shed one always does, so the ratio objective's breach predicate is a case test and neither the fan
// arm nor the burn fold branches on an absent string.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
[JsonPolymorphic(TypeDiscriminatorPropertyName = "outcome")]
[JsonDerivedType(typeof(Admitted), "admitted")]
[JsonDerivedType(typeof(Shed), "shed")]
public abstract partial record BackpressureVerdict {
    private BackpressureVerdict() { }
    public sealed record Admitted : BackpressureVerdict;
    public sealed record Shed(string Reason) : BackpressureVerdict;
}

// The replay comparison CLASS and the provider triple licensing it are two facts one string carried, so a
// `StartsWith` probe over free text stood in for a roster read and one shared ceiling const contradicted the
// per-class law beside it. The class is closed and owns its own relative-defect ceiling; the triple stays free
// text because a provider/version/device rendering is the host's, never this package's.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DeterminismTag {
    public static readonly DeterminismTag Bitwise = new("bit", ceiling: 0d);
    public static readonly DeterminismTag Envelope = new("envelope", ceiling: 1e-9d);
    public static readonly DeterminismTag DeviceWgpu = new("device-wgpu", ceiling: 1e-6d);

    public double Ceiling { get; }

    public bool Exact => Ceiling == 0d;
}

public sealed record DeterminismStamp(DeterminismTag Class, string Provider) : ISpanFormattable {
    public string ToString(string? format, IFormatProvider? formatProvider) => ToString();

    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider) =>
        destination.TryWrite($"{Class.Key}:{Provider}", out charsWritten);
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

    public sealed record Warmup(
        string ModelChecksum, ExecutionProvider Ep, string Shape,
        Option<int> Partitions, Option<Duration> Elapsed, Option<Instant> WarmedAt) : ComputeReceipt;

    public sealed record ModelRun(
        string ModelChecksum,
        ExecutionProvider Ep,
        string Mode,
        int BatchSize,
        long PeakBytes,
        Option<string> ArenaAllocator,
        Option<ProfileArtifact> Profile) : ComputeReceipt;

    public sealed record RemoteCall(string Transport, string Method, string Status, long RequestBytes, long ResponseBytes, DeadlineOutcome Outcome) : ComputeReceipt;

    // The tileset census the manifest emit measured — node count, max depth, leaf count — rides the emit receipt
    // because a manifest's shape is a property of the RUN that wrote it. Producer: `Runtime/tiles#TILE_PARTITION`
    // `TilesetCensus`, read off `TilesetExport.Census`; a non-tileset segment stream carries `None`.
    public sealed record StreamSegment(
        string ArtifactId, int Segments, long Bytes, Option<TilesetCensus> Census) : ComputeReceipt;

    public sealed record Allocation(
        StagingEventKind Event,
        long RequestedBytes,
        long GrantedBytes,
        Option<string> Detail,
        Option<string> NativeAllocator,
        Option<long> NativeReservedBytes,
        Option<long> SmallPoolFreeBytes,
        Option<long> LargePoolFreeBytes) : ComputeReceipt {
        // GENERATED lowering: the hand fold this replaces spelled the same Option Match eight times, and a ninth
        // evidence column compiled clean while the receipt silently dropped it — RequiredMappingStrategy.Both now
        // fails that build. Both sides carry `Option<T>` since the receipt stopped lowering absence to null, so no
        // carrier codec sits in the seam. Scope stamps through the post-`with`, never a whole-source
        // [MapPropertyFromSource] reader, so RMG020 keeps its source-side force.
        public static Allocation Of(AllocationEvidence evidence) =>
            AllocationMapper.Lower(evidence) with { Scope = new ReceiptScope.Process(evidence.Correlation, evidence.Class) };
    }

    public sealed record Copy(OrtResidency Gate, long Bytes, string Device) : ComputeReceipt;

    // The error-bounded and delta-coded encode stamps ride HERE because a cache fact is where a re-derivable
    // artifact's reuse is judged: `Residual` is the achieved max-residual against the residence bound
    // (`Runtime/field#FIELD_RESULT_CODEC` `FieldArtifact.MaxResidual`) and `Delta` the base/delta byte pair a geometry
    // delta reduced (`Runtime/codecs#GEOMETRY_DELTA` `GeometryDelta.BaseBytes`/`DeltaBytes`). An exact encode and
    // a whole-artifact store carry neither, so a consumer reading a zero ratio never grades an encode nothing coded.
    public sealed record Cache(
        CacheOutcome Outcome,
        string Key,
        long Bytes,
        Option<double> Residual,
        Option<DeltaStamp> Delta) : ComputeReceipt;

    public sealed record UnitProjection(string Family, string OriginalUnit, double OriginalValue, double CanonicalValue) : ComputeReceipt;

    public sealed record Backpressure(int QueueDepth, Duration Waited, BackpressureVerdict Verdict) : ComputeReceipt;

    public sealed record Drain(int Drained, int Faulted, int Refused) : ComputeReceipt;

    // Correlation and Class are consumed by the Scope stamp at the call site, so they are declared-ignored source
    // inventory rather than silently unmapped members.
    [Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Both)]
    public static partial class AllocationMapper {
        [MapProperty(nameof(AllocationEvidence.Kind), nameof(Allocation.Event))]
        [MapperIgnoreSource(nameof(AllocationEvidence.Correlation))]
        [MapperIgnoreSource(nameof(AllocationEvidence.Class))]
        [MapperIgnoreTarget(nameof(Allocation.Scope))]
        public static partial Allocation Lower(AllocationEvidence evidence);
    }

    public sealed record Conflict(ConflictSubject Subject, string Evidence) : ComputeReceipt;

    // Compute-INTERIOR refusals — a warm-bucket cap, a bucket-shape conflict, an unmeasured partition census —
    // are typed faults the executing rail returns and no reader ever sees, so a fleet refusing every stage for
    // want of a warm pulse stays invisible to Compute's own board. Identity is copied from the typed fault, so no
    // reason literal, category roster, or second code table can disagree. `Subject` carries a bucket key or a card id whose cardinality
    // no meter may fan on, which is why it stays a payload column.
    public sealed record Refusal(FaultId Identity, string Subject) : ComputeReceipt {
        public static Refusal Of(ComputeFault fault, CorrelationId correlation, AllocationClass allocation) =>
            new(Identity: fault.Identity, Subject: fault.Message) {
                Scope = new ReceiptScope.Process(correlation, allocation),
            };
    }

    public sealed record Factorization(string Provider, string Decomposition, int Rows, int Cols, long Nnz, string Format) : ComputeReceipt {
        public Option<string> RouteVariant { get; init; }
        public Option<DeterminismStamp> Determinism { get; init; }
        public Option<int> SymbolicFill { get; init; }
        public Option<double> ResidualCap { get; init; }
        public Option<double> TrueResidual { get; init; }
        public ShardRole Shards { get; init; } = new ShardRole.Whole();
    }

    public sealed record Generate(
        string ModelChecksum,
        ExecutionProvider Ep,
        string ModelType,
        string Mode,
        Option<string> Adapter,
        int Tokens,
        double TokensPerSecond,
        GuidanceKind GuidanceKind,
        int ConstrainedTokens,
        int ToolCalls,
        Option<int> Seed) : ComputeReceipt {
        // Staged multimodal token total read once off Generator.TokenCount() after SetInputs — measured,
        // resolution-invariant, linear in media count; absent on a text-only run, so the column separates prompt
        // cost from media cost per run.
        public Option<int> StagedTokens { get; init; }
    }

    public sealed record Embedding(string ModelChecksum, string Encoding, int Dimension, long ByteLength) : ComputeReceipt;

    public sealed record Discretization(string Algorithm, string Element, long Nodes, long Elements, int BoundaryLayers, int RefineLevel, double WorstQuality, string Metric) : ComputeReceipt;

    // `Method` reads the `Solver/contract#SOLVE_REQUEST` `SolveRoute` case key, so the route the run took and the
    // route the receipt names are one value. Sharded runs emit one receipt per shard and the merge receipt that
    // folds them, and `Shards` is the discriminant separating those two populations. `Participation` carries that
    // same owner's `ModalParticipation` triple — the per-axis effective-mass FRACTION its `EffectiveMassShare`
    // fold mints, declared there and carried here, because the three axes are measured together or not at all.
    public sealed record Solve(string Physics, string Method, long Dofs, int Iterations, double Residual, bool Converged) : ComputeReceipt {
        public ShardRole Shards { get; init; } = new ShardRole.Whole();
        public Option<ModalParticipation> Participation { get; init; }
        public Option<ConstitutiveEvidence> Constitutive { get; init; }
        public Option<ContactEvidence> Contact { get; init; }
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

    public sealed record Clash(AccelerationKind IndexKind, int Candidates, int HardClashes, int ClearanceViolations, int TotalPairs) : ComputeReceipt {
        // True when the survey stopped at ClashPolicy.MaxPairs — a truncated census read as complete undercounts
        // every downstream clearance ratio, so the ceiling hit is receipt evidence, never a silent cap.
        public bool Truncated { get; init; }
    }

    public sealed record Twin(string SignalId, double Predicted, double Measured, double Residual, bool Anomaly, double ControlDelta) : ComputeReceipt;

    public sealed record Uncertainty(
        string Method,
        int Samples,
        Option<double> Mean,
        Option<double> Variance,
        Option<double> Skewness,
        Option<double> Kurtosis,
        Seq<double> Quantiles,
        Seq<double> SobolFirst,
        Seq<double> SobolTotal,
        Seq<double> Interaction,
        Seq<double> MostProbablePoint,
        // Surrogate calibration measured off the Vandermonde the spectral fit already built — one GEMV, never a
        // second solve. A sampling, reliability, or subset run fits no surrogate and carries `None` in both, and an
        // exactly-determined basis interpolates with no residual degrees of freedom, so its standard error is
        // absent rather than an infinity: the absence IS the measurement.
        Option<double> FitQuality,
        Option<double> ResidualStandardError,
        double FailureProbability,
        double ReliabilityIndex) : ComputeReceipt;

    public sealed record Fit(string Family, string Method, long Parameters, int Iterations, double Residual, bool Converged, double Quality, string QualityMetric, Option<int> RetainedRank) : ComputeReceipt;

    public sealed record Governor(double CpuPercent, double MemoryPercent, int Workers, int ReaderCeiling, int PartitionCap, double MemoryScale, bool SpillPressure) : ComputeReceipt;

    public sealed record Drift(string MonitorId, MonitorStatistic Statistic, double Level, Option<double> Limit, bool Breach, int Window) : ComputeReceipt;

    public string ToString(string? format, IFormatProvider? formatProvider) => ToString();

    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider) =>
        destination.TryWrite($"{Correlation}:{Lane.Map(static row => row.Key).IfNone("process")}:{Substrate.Map(static row => row.Key).IfNone("process")}:{Elapsed.Map(static value => value.ToString()).IfNone("process")}", out charsWritten);

    public bool TryFormat(Span<byte> utf8Destination, out int bytesWritten, ReadOnlySpan<char> format, IFormatProvider? provider) =>
        Utf8.TryWrite(utf8Destination, $"{Correlation}:{Lane.Map(static row => row.Key).IfNone("process")}:{Substrate.Map(static row => row.Key).IfNone("process")}:{Elapsed.Map(static value => value.ToString()).IfNone("process")}", out bytesWritten);
}

// Base-versus-delta byte pair with its reduction ratio derived, never stored: two numbers and their quotient are
// one measurement, and a stored third column is the copy a re-encode forgets to move.
[StructLayout(LayoutKind.Auto)]
public readonly record struct DeltaStamp(long BaseBytes, long DeltaBytes) {
    public double Ratio => BaseBytes <= 0L ? 1d : (double)DeltaBytes / BaseBytes;
}

// ONE invariant-text carrier for every scalar the wire spells as a string. The two per-type converters this
// replaces differed by their format literal and their number style alone, so generic math carries both as policy
// values and a third scalar lands as one static field rather than a fourth class.
public sealed class InvariantTextJsonConverter<T>(NumberStyles styles, string format) : JsonConverter<T>
    where T : ISpanParsable<T>, ISpanFormattable, INumberBase<T> {
    public static readonly InvariantTextJsonConverter<T> Decimal = new(NumberStyles.Integer, "G");
    public static readonly InvariantTextJsonConverter<T> Hex32 = new(NumberStyles.HexNumber, "x32");

    // Exemption: `boundaries.md [06] [CONVERTER_OWNER]` names the converter throw channel — `JsonConverter<T>.Read`
    // has no rail. The message carries the `<slug:payload>` grammar every refusal on this page spells, so a
    // serializer fault reads by the same rule a `ComputeFault` detail does.
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.TokenType == JsonTokenType.String
        && T.TryParse(reader.GetString(), styles, CultureInfo.InvariantCulture, out T? value)
        && value is { } admitted
            ? admitted
            : throw new JsonException($"<invariant-text-expected:{typeof(T).Name}:{format}>");

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.ToString(format, CultureInfo.InvariantCulture));
}

// `LanguageExtJsonConverterFactory` is the kernel `csharp:Rasm/Domain/rails#CARRIER_CODEC` carrier-space owner
// this stratum already carries: every `Option<T>` receipt column and every `Seq<T>` payload crosses here, and
// without the factory each would serialize its carrier's own members rather than its value. This context declines
// the suite's `OmitAbsent` modifier by contract — the `[05]-[TS_PROJECTION]` posture is that absent evidence
// crosses as EXPLICIT NULL, never as an omitted member — so an `Option<T>` slot stays present on the wire, its
// ctor parameter stays wire-required by design, and the `| null` unions the TS mirrors spell are the agreement.
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
    RespectNullableAnnotations = true,
    RespectRequiredConstructorParameters = true,
    Converters = [
        typeof(ThinktectureJsonConverterFactory), typeof(LanguageExtJsonConverterFactory),
        typeof(InvariantTextJsonConverter<long>), typeof(InvariantTextJsonConverter<UInt128>)])]
[JsonSerializable(typeof(ComputeReceipt))]
[JsonSerializable(typeof(ReceiptScope))]
[JsonSerializable(typeof(SelectionDecision))]
[JsonSerializable(typeof(SelectionMode))]
[JsonSerializable(typeof(ShardRole))]
[JsonSerializable(typeof(BackpressureVerdict))]
[JsonSerializable(typeof(ProfileArtifact))]
[JsonSerializable(typeof(BenchmarkClaim))]
[JsonSerializable(typeof(GraduationEvidence))] // Model/identity#GRADUATION_EVIDENCE offline bundle — Bundle() reads its JsonTypeInfo off this context.
[JsonSerializable(typeof(BenchmarkInput))]
[JsonSerializable(typeof(HostFingerprint))]
[JsonSerializable(typeof(PanelRow))]
[JsonSerializable(typeof(AlertSpec))]
[JsonSerializable(typeof(ChargebackDataset))]
public partial class ComputeWireContext : JsonSerializerContext;

// --- [INSTRUMENT_ROSTER] — one row per instrument, each carrying its OWN kernel declaration. The write plane
// addresses the ROW (`Rasm/Domain/instrument#SPEC`), so the name const and the spec row a name const paired with
// are one member here, and `ValidateConstructorArguments` proves the pairing at type init.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ComputeInstrument {
    // The declared Dimensions column and the fan's tag keys are this ONE vocabulary, so the governance view derives
    // its tag-key set off the mounted row. The roster declares the key a write MAY carry: an absent fact omits its
    // key and the point exports untagged on that axis, which the kernel row reads as the declaration's own absence
    // arm exactly as it reads an untenanted write against `TenantContext.TenantSlot`.
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
    public const string PointSlot = "rasm.compute.hook.point";

    // Ratio indicators need BOTH series mounted as counters, so each SLO subject carries a population row and a
    // good row beside its shape measure; the `Runtime/board#OBJECTIVES` fold and the store-side burn rule then read
    // one truth. Both rows of a pair declare the IDENTICAL dimension set so the burn rule divides series against
    // series, and the outcome axis that discriminates them rides the shape measures alone.
    public static readonly ComputeInstrument ReceiptsEmitted = new(
        "rasm.compute.receipts.emitted",
        InstrumentSpec.Create("rasm.compute.receipts.emitted", InstrumentKind.Count, MeasureForm.Whole, "{receipt}",
            "Receipts emitted through the sink port by kind.", Seq(KindSlot), None, None, None));

    public static readonly ComputeInstrument SolveFactorizations = new(
        "rasm.compute.solve.factorizations",
        InstrumentSpec.Create("rasm.compute.solve.factorizations", InstrumentKind.Count, MeasureForm.Whole, "{factorization}",
            "Dense and sparse factorizations by provider and kind.", Seq(ProviderSlot, DecompositionSlot), None, None, None));

    public static readonly ComputeInstrument SolveResidual = new(
        "rasm.compute.solve.residual",
        InstrumentSpec.Create("rasm.compute.solve.residual", InstrumentKind.Distribution, MeasureForm.Real, "1",
            "Iterative-solver convergence residual.", Seq(PhysicsSlot, MethodSlot, ConvergedSlot), Some(Buckets.ResidualDecades), None, None));

    public static readonly ComputeInstrument SolveIterations = new(
        "rasm.compute.solve.iterations",
        InstrumentSpec.Create("rasm.compute.solve.iterations", InstrumentKind.Distribution, MeasureForm.Whole, "{iteration}",
            "Iterative-solve iteration counts to convergence or frame-budget stop.", Seq(PhysicsSlot, MethodSlot, ConvergedSlot), Some(Buckets.IterationCounts), None, None));

    public static readonly ComputeInstrument SolveRuns = new(
        "rasm.compute.solve.runs",
        InstrumentSpec.Create("rasm.compute.solve.runs", InstrumentKind.Count, MeasureForm.Whole, "{solve}",
            "Iterative solves attempted by physics and method.", Seq(PhysicsSlot, MethodSlot), None, None, None));

    public static readonly ComputeInstrument SolveConverged = new(
        "rasm.compute.solve.converged",
        InstrumentSpec.Create("rasm.compute.solve.converged", InstrumentKind.Count, MeasureForm.Whole, "{solve}",
            "Iterative solves reaching their convergence criterion.", Seq(PhysicsSlot, MethodSlot), None, None, None));

    public static readonly ComputeInstrument SolveShards = new(
        "rasm.compute.solve.shards",
        InstrumentSpec.Create("rasm.compute.solve.shards", InstrumentKind.Count, MeasureForm.Whole, "{shard}",
            "Solve and factorization shards executed per substrate.", Seq(SubstrateSlot), None, None, None));

    public static readonly ComputeInstrument GenerateTokens = new(
        "rasm.compute.generate.tokens",
        InstrumentSpec.Create("rasm.compute.generate.tokens", InstrumentKind.Count, MeasureForm.Whole, "{token}",
            "Tokens emitted through the generative run loop by run mode, adapter, and guidance.", Seq(ModeSlot, AdapterSlot, GuidanceSlot), None, None, None));

    public static readonly ComputeInstrument OptimizeHypervolume = new(
        "rasm.compute.optimize.hypervolume",
        InstrumentSpec.Create("rasm.compute.optimize.hypervolume", InstrumentKind.Distribution, MeasureForm.Real, "1",
            "Pareto-front hypervolume indicator per optimizer generation.", Seq(MethodSlot), Some(Buckets.Hypervolume), None, None));

    public static readonly ComputeInstrument MeshElements = new(
        "rasm.compute.mesh.elements",
        InstrumentSpec.Create("rasm.compute.mesh.elements", InstrumentKind.Count, MeasureForm.Whole, "{element}",
            "Volumetric elements generated per discretization and refinement level.", Seq(AlgorithmSlot, RefineSlot), None, None, None));

    public static readonly ComputeInstrument ClashConfirmed = new(
        "rasm.compute.clash.confirmed",
        InstrumentSpec.Create("rasm.compute.clash.confirmed", InstrumentKind.Count, MeasureForm.Whole, "{clash}",
            "Confirmed clashes by severity and federated-index kind.", Seq(SeveritySlot, IndexSlot), None, None, None));

    public static readonly ComputeInstrument TwinVerdicts = new(
        "rasm.compute.twin.verdicts",
        InstrumentSpec.Create("rasm.compute.twin.verdicts", InstrumentKind.Count, MeasureForm.Whole, "{verdict}",
            "Digital-twin verdicts evaluated against the ROM error bound.", Seq(SignalSlot), None, None, None));

    public static readonly ComputeInstrument TwinNominal = new(
        "rasm.compute.twin.nominal",
        InstrumentSpec.Create("rasm.compute.twin.nominal", InstrumentKind.Count, MeasureForm.Whole, "{verdict}",
            "Digital-twin verdicts inside the ROM error bound.", Seq(SignalSlot), None, None, None));

    public static readonly ComputeInstrument RemoteDuration = new(
        "rasm.compute.remote.duration",
        InstrumentSpec.Create("rasm.compute.remote.duration", InstrumentKind.Distribution, MeasureForm.Real, "s",
            "Remote transport wall duration per call by transport and status.", Seq(TransportSlot, StatusSlot), Some(Buckets.RemoteSeconds), None, None));

    public static readonly ComputeInstrument RemoteCalls = new(
        "rasm.compute.remote.calls",
        InstrumentSpec.Create("rasm.compute.remote.calls", InstrumentKind.Count, MeasureForm.Whole, "{call}",
            "Remote transport calls attempted by transport.", Seq(TransportSlot), None, None, None));

    public static readonly ComputeInstrument RemoteOk = new(
        "rasm.compute.remote.ok",
        InstrumentSpec.Create("rasm.compute.remote.ok", InstrumentKind.Count, MeasureForm.Whole, "{call}",
            "Remote transport calls landing an ok status.", Seq(TransportSlot), None, None, None));

    public static readonly ComputeInstrument BackpressureVerdicts = new(
        "rasm.compute.backpressure.verdicts",
        InstrumentSpec.Create("rasm.compute.backpressure.verdicts", InstrumentKind.Count, MeasureForm.Whole, "{verdict}",
            "Backpressure verdicts evaluated at the queue gate.", Seq<string>(), None, None, None));

    public static readonly ComputeInstrument BackpressureAdmitted = new(
        "rasm.compute.backpressure.admitted",
        InstrumentSpec.Create("rasm.compute.backpressure.admitted", InstrumentKind.Count, MeasureForm.Whole, "{verdict}",
            "Backpressure verdicts admitting queued work without shedding.", Seq<string>(), None, None, None));

    public static readonly ComputeInstrument AssessmentVerdicts = new(
        "rasm.compute.assessment.verdicts",
        InstrumentSpec.Create("rasm.compute.assessment.verdicts", InstrumentKind.Count, MeasureForm.Whole, "{assessment}",
            "Discipline assessments by discipline and verdict.", Seq(DisciplineSlot, VerdictSlot), None, None, None));

    public static readonly ComputeInstrument AssessmentRatio = new(
        "rasm.compute.assessment.ratio",
        InstrumentSpec.Create("rasm.compute.assessment.ratio", InstrumentKind.Distribution, MeasureForm.Real, "1",
            "Governing utilization ratio per assessment by discipline.", Seq(DisciplineSlot), Some(Buckets.GoverningRatio), None, None));

    public static readonly ComputeInstrument TrajectoryRuns = new(
        "rasm.compute.trajectory.runs",
        InstrumentSpec.Create("rasm.compute.trajectory.runs", InstrumentKind.Count, MeasureForm.Whole, "{trajectory}",
            "Trajectories traced through the adaptive integration driver.", Seq<string>(), None, None, None));

    public static readonly ComputeInstrument TrajectoryResolved = new(
        "rasm.compute.trajectory.resolved",
        InstrumentSpec.Create("rasm.compute.trajectory.resolved", InstrumentKind.Count, MeasureForm.Whole, "{trajectory}",
            "Trajectories reaching their horizon inside every driver budget.", Seq<string>(), None, None, None));

    public static readonly ComputeInstrument TrajectorySteps = new(
        "rasm.compute.trajectory.steps",
        InstrumentSpec.Create("rasm.compute.trajectory.steps", InstrumentKind.Distribution, MeasureForm.Whole, "{step}",
            "Accepted trajectory steps per run by terminal marker.", Seq(TerminalSlot), Some(Buckets.IterationCounts), None, None));

    public static readonly ComputeInstrument QuadratureSkips = new(
        "rasm.compute.quadrature.skips",
        InstrumentSpec.Create("rasm.compute.quadrature.skips", InstrumentKind.Count, MeasureForm.Whole, "{evaluation}",
            "Integrand evaluations the kernel quadrature guard skipped as non-finite.", Seq<string>(), None, None, None));

    public static readonly ComputeInstrument ProgressMarks = new(
        "rasm.compute.progress.marks",
        InstrumentSpec.Create("rasm.compute.progress.marks", InstrumentKind.Count, MeasureForm.Whole, "{mark}",
            "Progress marks delivered through the cadence gate by phase.", Seq(PhaseSlot), None, None, None));

    public static readonly ComputeInstrument ProgressCadence = new(
        "rasm.compute.progress.cadence",
        InstrumentSpec.Create("rasm.compute.progress.cadence", InstrumentKind.Distribution, MeasureForm.Real, "s",
            "Interval between consecutive delivered progress marks by phase.", Seq(PhaseSlot), Some(Buckets.CadenceSeconds), None, None));

    public static readonly ComputeInstrument CostUnits = new(
        "rasm.compute.cost.units",
        InstrumentSpec.Create("rasm.compute.cost.units", InstrumentKind.Distribution, MeasureForm.Real, "1",
            "Priced cost units attributed per tenant and substrate.", Seq(TenantContext.TenantSlot, SubstrateSlot), Some(Buckets.CostUnitDecades), None, None));

    public static readonly ComputeInstrument MonitorBreaches = new(
        "rasm.compute.monitor.breaches",
        InstrumentSpec.Create("rasm.compute.monitor.breaches", InstrumentKind.Count, MeasureForm.Whole, "{breach}",
            "Streaming-monitor control-limit and drift breaches by monitor.", Seq(MonitorSlot), None, None, None));

    public static readonly ComputeInstrument Refusals = new(
        "rasm.compute.refusals",
        InstrumentSpec.Create("rasm.compute.refusals", InstrumentKind.Count, MeasureForm.Whole, "{refusal}",
            "Typed refusals raised on the Compute interior rails by reason.", Seq(ReasonSlot), None, None, None));

    public static readonly ComputeInstrument ClaimsBound = new(
        "rasm.compute.claims.bound",
        InstrumentSpec.Create("rasm.compute.claims.bound", InstrumentKind.Level, MeasureForm.Whole, "{claim}",
            "Benchmark claims bound for the current host fingerprint.", Seq<string>(), None, None, None));

    // The hook capsule's isolation cell is a PULLED reading, so the roster carries its row here beside the pushed
    // ones and `Runtime/board#HOOK_POINTS` binds the cell as its probe — the parked-fault census then reaches the
    // board through the same derivation every other row does, with no descriptor edit.
    public static readonly ComputeInstrument HookIsolated = new(
        "rasm.compute.hook.isolated",
        InstrumentSpec.Create("rasm.compute.hook.isolated", InstrumentKind.Level, MeasureForm.Whole, "{fault}",
            "Subscriber faults parked on the Compute hook rail's isolation cell.", Seq<string>(), None, None, None));

    public InstrumentSpec Row { get; }

    public static Seq<InstrumentSpec> Rows => toSeq(Items).Map(static row => row.Row).Strict();

    static partial void ValidateConstructorArguments(ref string key, ref InstrumentSpec row) {
        if (!string.Equals(key, row.Name, StringComparison.Ordinal)) {
            throw new ArgumentException($"<compute-instrument:{key}:{row.Name}>", nameof(row));
        }
    }
}

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
    // selector binds before any fact exists — and the frozen lookup THROWS on an unregistered case, so a selector
    // over a case the roster never declared has no construction path.
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
            // The generated WRITE half proves beside the kind bijection: a message-envelope root whose
            // `SerializeHandler` is null fell back to the reflection writer, which the Strict resolver posture
            // exists to forbid, and no kind census detects it.
            && ComputeWireContext.Default.ComputeReceipt.SerializeHandler is not null
                ? Fin.Succ(unit)
                : Fin.Fail<Unit>(new ComputeFault.EquivalenceMiss(
                    $"<kind-registry-drift:cases={cases.Count}:rows={Registry.Rows.Count}:declared={Registry.Declared}>"));
    }

    public static readonly TelemetrySource Source = TelemetrySource.Compute;

    // Trace plane admitted into the kernel band: substrates ride the span NAME under this one scope, so no
    // dispatch bracket ever mints a per-route source.
    public static readonly TraceScope Dispatch = TraceScope.Create(value: "rasm.compute.dispatch");

    // A PEER's status name, not a Compute vocabulary — the row set is the transport's and no owner here declares
    // it, so the ok-test compares against this one spelling rather than a roster nothing can close.
    public const string OkStatus = "ok";
    public const string HardClash = "hard";
    public const string ClearanceClash = "clearance";

    // Rows and the `Runtime/board#PANEL_PROJECTION` pack derived from them leave as ONE downward fact, so the
    // mounting root proves the pack in the fold that binds these handles; the pack's own init reads the roster and
    // never this method, and `TelemetryContributorPort.Admit` is the whole proof.
    public static TelemetryContributorPort Telemetry(string version) =>
        new(Scope: Source, Version: version, Instruments: ComputeInstrument.Rows,
            Planes: Seq(Dispatch), Board: Some(ComputeDescriptors.Board));

    // Instrument refusals are evidence, never an emit abort: the envelope IS the truth and the instrument
    // stream is the lossy channel beside it, so a mount defect parks and the receipt still seals. One fold answers
    // both the writes and the price, so the two 33-arm traversals this emit ran back to back are one.
    public IO<ReceiptEnvelope> Emit(ComputeReceipt fact) =>
        IO.lift(() => TenantContext.Current)
            .Bind(tenant => IO.lift(() => Park(ComputeInstrumentFan.Project(instruments, costs, tenant, fact)))
                .Bind(_ => IO.lift(() => JsonSerializer.SerializeToElement(fact, wire.ComputeReceipt))
                    .Bind(payload => sink.Send(fact.Correlation, tenant, Source, KindOf(fact), payload))));

    private Unit Park(Fin<Unit> written) =>
        written.Match(Succ: static _ => unit, Fail: error => ignore(refusals.Swap(held => held.Add(error))));
}
```

## [03]-[TELEMETRY_PROJECTION]

- Owner: `FactMeasure` — what one fact OWES, as data: the instrument writes it earns and the cost it prices at; `ComputeInstrumentFan` — the one fold answering that pair over the typed union, its advice rows the kernel `Buckets` and its levels the composition `LevelCells`; `ComputeTraces` — the dispatch spine composing the kernel `SpanBand` at the admitted `rasm.compute.dispatch` scope. Instrument custody is one-per-composition: `ReceiptSurface.Telemetry` is the whole declaration and the composing app materializes it, so the fan holds writes alone and mints neither meter nor set.
- Entry: `ComputeInstrumentFan.Measure(CostPolicy costs, ComputeReceipt fact)` folds one typed fact onto the write-and-price pair through the generated total `Switch`; `ComputeInstrumentFan.Project(InstrumentSet set, CostPolicy costs, TenantContext tenant, ComputeReceipt fact)` spends that pair — the kind counter, every earned write, and the one priced cost-unit write under the tenancy tag set beside the substrate fact an execution-scoped receipt carries; `ComputeInstrumentFan.Bound(InstrumentSet set, BenchmarkRank ranks)` swaps the boot-frozen claim count onto its level cell through the kernel pulled gate; `ComputeTraces.Traced(DispatchTable table, SpanBand band)` decorates every dispatch arm with the substrate-named span.
- Auto: `ReceiptSurface.Emit` runs `Project` on the typed fact before serialization, so every emitted receipt projects and prices with zero call-site ceremony and the kind space partitions cleanly — the AppHost fan owns AppHost kinds, this `Switch` owns Compute kinds, and one `ReceiptEnvelope` kind projects in exactly one fan; every histogram row ships `InstrumentAdvice<T>` explicit-bucket boundaries as the fallback a backend without base2-exponential histograms reads; the trace-based exemplar filter at the provider joins any measurement recorded inside the live dispatch span to its trace and span ids with zero wiring; the composition root taps `Bound` where the boot-frozen `BenchmarkRank` projection resolves, folding the fingerprint-matched claim count into the level cell the `rasm.compute.claims.bound` gauge reads.
- Receipt: none — the fan is a projection of receipts; an instrument write beside it is a second truth. Every write returns the kernel rail and a refusal parks on the composition's cell at `ReceiptSurface.Emit`, so a mount defect is evidence rather than a silently dropped measurement or an emit abort.
- Packages: LanguageExt.Core, Rasm (kernel signal capsule), BCL inbox
- Growth: a new receipt case breaks the one fold at compile time, so every new kind decides its instrument writes AND its cost axes in ONE arm or answers `FactMeasure.Silent` explicitly; a new instrument is one `[02]-[RECEIPT_UNION]` roster row and one arm edit; a new cost axis is one `Runtime/ledger#COST_ALGEBRA` field and the arms it touches.
- Law: metering and pricing are TWO READINGS OF ONE FACT, so they are one fold. The two 33-arm total `Switch`es this replaces walked the same union back to back at every emit — 18 identity arms on one and 28 on the other — and a landed case had to answer both or silently answer neither. NAMED LOSS: the write half now materializes as a `Seq` of write descriptions where the direct fold allocated none, and a cost-only edit touches this page rather than the ledger. WITNESS: the kernel's own `SignalFan` answers `Seq<(InstrumentSpec Row, double Value)>` and spends it afterwards (`Rasm/Domain/telemetry#COST`); the 18 silent arms return the empty `Seq` singleton, so the billing journal's day-long fold over mostly-silent facts allocates nothing to read the cost half.
- Boundary: instruments stay curated aggregates, never 1:1 with cases. Refusal metrics use the bounded numeric fault identity; the high-cardinality subject remains payload only.
- Boundary: a ratio indicator's population and good series are BOTH mounted counters written from the same arm, so the in-process burn fold and the store-side burn rule read one truth and neither re-derives the other's numerator.
- Boundary: both cost dimensions are ABSENCE-BEARING and neither fills — the root frame contributes no tenant tag and a process-scoped receipt contributes no substrate fact, so the point exports untagged on that axis, which is what the declared `Dimensions` roster means by naming the key a write may carry. The `Substrate` row keys are the WHOLE value space of that axis, so a scope word substituted for an absent row exports a sixth substrate the vocabulary never spells and every backend grouping cost by substrate reads a phantom; the untagged point is the honest one, and the scope a charge ran under is already the receipt's own `ReceiptScope` case.
- Boundary: every tag set mints through the kernel `InstrumentSet.Tags` `TagList` projection the `in TagList` write overload consumes, so an arm allocates no heap tag array and a folder-local tag fold beside it is the deleted re-mint.
- Boundary: the span opens at dispatch and closes on the arm's own rail through the band's IO bracket, `SetStatus(ActivityStatusCode.Error)` the typed verdict on the fail leg, and a package-local `ActivitySource` beside the band is the deleted form; the arm's own value is the `Fin<ComputeReceipt>` the `Runtime/admission#DISPATCH_SPINE` table publishes, so the bracket carries the evidence out rather than erasing it to `Unit`.
- Boundary: profile correlation rides trace identity — the AppHost-composed `PyroscopeSpanProcessor` stamps `pyroscope.profile.id` on the trace's root span alone and clears the profiler span context at `OnEnd`, so a `rasm.compute.dispatch.*` `ActivityKind.Internal` span joins its flame-graph slice through the `TraceId` it shares with that stamped root, and Compute stamps no profile tag and holds zero OTel package reference.
- Boundary: receipt payload identifiers — checksums, content keys, artifact ids, provider names — sit in the `Operational`/`Internal` classification tiers whose redactor rows pass, so per-field `DataClassification` attributes never land on receipt cases and redaction custody stays at the AppHost egress seam.
- Boundary: the cost write is the lossy channel — the `Runtime/ledger#CHARGEBACK_EGRESS` folds over the `ReceiptEnvelope`-joined journal stay billing truth, a zero-priced fact skips the write, and the tenant tag rides the same AppHost cardinality cap every tag fan obeys.

```csharp signature
// What one fact OWES, as DATA rather than an effect: the writes it earned and the cost it priced at. The emit path
// spends the writes onto its own set; the `Runtime/ledger#CHARGEBACK_EGRESS` journal fold reads `Cost` with no
// `InstrumentSet` in hand and never touches the write half.
public readonly record struct FactMeasure(
    Seq<(InstrumentSpec Row, double Value, TagList Tags)> Writes, CostVector Cost) {
    public static readonly FactMeasure Silent = new(Seq<(InstrumentSpec, double, TagList)>(), CostVector.Zero);

    public static FactMeasure Metered(params ReadOnlySpan<(InstrumentSpec Row, double Value, TagList Tags)> writes) =>
        new(toSeq(writes.ToArray()), CostVector.Zero);

    public FactMeasure Charging(CostVector cost) => this with { Cost = cost };
}

public static class ComputeInstrumentFan {
    public static Fin<Unit> Bound(InstrumentSet set, BenchmarkRank ranks) =>
        set.Level(ComputeInstrument.ClaimsBound.Row, ranks.Ranks.Count);

    // Spend order is fixed: the kind counter first so a fact is counted even where its own arm refuses, then the
    // earned writes, then the ONE priced write. Every arm returns the kernel rail, so a refused measurement
    // reaches `ReceiptSurface.Emit`'s parking cell instead of vanishing.
    public static Fin<Unit> Project(InstrumentSet set, CostPolicy costs, TenantContext tenant, ComputeReceipt fact) {
        FactMeasure measured = Measure(costs, fact);
        return set.Write(ComputeInstrument.ReceiptsEmitted.Row, 1L,
                InstrumentSet.Tags((ComputeInstrument.KindSlot, ReceiptSurface.KindOf(fact))))
            .Bind(_ => measured.Writes.TraverseM(row => set.Write(row.Row, row.Value, row.Tags)).As().Map(static _ => unit))
            .Bind(_ => measured.Cost.Total > 0d
                ? set.Write(ComputeInstrument.CostUnits.Row, measured.Cost.Total, Attributed(tenant, fact.Substrate))
                : Fin.Succ(unit));
    }

    // BOTH cost dimensions are absence-bearing on one write and neither invents a filler: `TenantContext.Tags` is
    // empty for the root frame, and a process-scoped receipt — a refusal, a drain, an allocation — carries no
    // substrate, so its fact is omitted whole and the untagged point is the honest one.
    static TagList Attributed(TenantContext tenant, Option<Substrate> substrate) =>
        substrate.Match(
            Some: row => InstrumentSet.Tags(tenant, (ComputeInstrument.SubstrateSlot, row.Key)),
            None: () => InstrumentSet.Tags(tenant));

    public static FactMeasure Measure(CostPolicy costs, ComputeReceipt fact) =>
        // Elapsed prices on the SCOPE, not the case: a substrate-rated wall second is the one axis every executed
        // fact shares, so it folds in once here and no arm re-derives it.
        Elapsed(costs, fact) is { } floor
        && fact.Switch(
            state: costs,
            selection: static (_, _) => FactMeasure.Silent,
            tensorRun: static (_, _) => FactMeasure.Silent,
            modelLoad: static (_, _) => FactMeasure.Silent,
            warmup: static (_, _) => FactMeasure.Silent,
            modelRun: static (_, _) => FactMeasure.Silent,
            remoteCall: static (policy, call) => Called(call).Charging(Remote(policy, call)),
            streamSegment: static (_, _) => FactMeasure.Silent,
            allocation: static (policy, staged) => staged.GrantedBytes > 0L
                ? FactMeasure.Silent.Charging(CostVector.Zero with { ByteUnits = staged.GrantedBytes * policy.StagedByteRate })
                : FactMeasure.Silent,
            copy: static (_, _) => FactMeasure.Silent,
            cache: static (_, _) => FactMeasure.Silent,
            unitProjection: static (_, _) => FactMeasure.Silent,
            backpressure: static (_, queued) => queued.Verdict is BackpressureVerdict.Admitted
                ? FactMeasure.Metered(
                    (ComputeInstrument.BackpressureVerdicts.Row, 1d, default),
                    (ComputeInstrument.BackpressureAdmitted.Row, 1d, default))
                : FactMeasure.Metered((ComputeInstrument.BackpressureVerdicts.Row, 1d, default)),
            drain: static (_, _) => FactMeasure.Silent,
            conflict: static (_, _) => FactMeasure.Silent,
            refusal: static (_, refused) => FactMeasure.Metered((
                ComputeInstrument.Refusals.Row, 1d,
                InstrumentSet.Tags((ComputeInstrument.ReasonSlot, refused.Reason.Key)))),
            factorization: static (_, factored) => Factored(factored),
            generate: static (policy, run) => FactMeasure.Metered((
                    ComputeInstrument.GenerateTokens.Row, run.Tokens, InstrumentSet.Tags(
                        (ComputeInstrument.ModeSlot, run.Mode),
                        (ComputeInstrument.AdapterSlot, run.Adapter.IfNoneUnsafe(() => null!)),
                        (ComputeInstrument.GuidanceSlot, run.GuidanceKind.Key))))
                .Charging(CostVector.Zero with { TokenUnits = run.Tokens * policy.TokenRate }),
            embedding: static (_, _) => FactMeasure.Silent,
            discretization: static (_, mesh) => FactMeasure.Metered((
                ComputeInstrument.MeshElements.Row, mesh.Elements, InstrumentSet.Tags(
                    (ComputeInstrument.AlgorithmSlot, mesh.Algorithm),
                    (ComputeInstrument.RefineSlot, mesh.RefineLevel)))),
            solve: static (_, solved) => Solved(solved),
            coupling: static (_, _) => FactMeasure.Silent,
            optimization: static (_, optimized) => FactMeasure.Metered((
                ComputeInstrument.OptimizeHypervolume.Row, optimized.Hypervolume,
                InstrumentSet.Tags((ComputeInstrument.MethodSlot, optimized.Optimizer)))),
            sweep: static (_, _) => FactMeasure.Silent,
            clash: static (_, clashed) => Clashed(clashed),
            twin: static (_, twin) => Twinned(twin),
            uncertainty: static (_, _) => FactMeasure.Silent,
            fit: static (_, _) => FactMeasure.Silent,
            governor: static (_, _) => FactMeasure.Silent,
            drift: static (_, drifted) => drifted.Breach
                ? FactMeasure.Metered((
                    ComputeInstrument.MonitorBreaches.Row, 1d,
                    InstrumentSet.Tags((ComputeInstrument.MonitorSlot, drifted.MonitorId))))
                : FactMeasure.Silent,
            assessment: static (_, assessed) => Assessed(assessed),
            // Fixed-order rows skip nothing, so the write gates on a MEASURED skip rather than recording a zero
            // that reads as coverage the kernel guard never granted. Both integration legs and every sampling
            // campaign are managed CPU folds the substrate-rated elapsed floor prices whole, so none carries a
            // token, staged-byte, or remote-node charge of its own.
            quadrature: static (_, run) => run.Skipped > 0
                ? FactMeasure.Metered((ComputeInstrument.QuadratureSkips.Row, run.Skipped, default))
                : FactMeasure.Silent,
            trajectory: static (_, run) => Traced(run),
            // Sampling facts carry campaign diagnostics — a net's discrepancy pair, a fit's centre count — and no
            // fleet aggregate: neither figure is comparable across families or dimensions, so a meter over either
            // exports one series mixing incomparable populations. The `ReceiptsEmitted` kind counter already
            // carries the run census, and the diagnostics stay payload columns a replay reads.
            sampling: static (_, _) => FactMeasure.Silent) is { } measured
            ? measured.Charging(measured.Cost + floor)
            : FactMeasure.Silent;

    static CostVector Elapsed(CostPolicy costs, ComputeReceipt fact) =>
        fact.Substrate
            .Bind(route => fact.Elapsed.Map(elapsed => CostVector.Zero with { ElapsedUnits = elapsed.TotalSeconds * costs.SecondRate(route) }))
            .IfNone(CostVector.Zero);

    static CostVector Remote(CostPolicy costs, ComputeReceipt.RemoteCall call) =>
        call.Elapsed
            .Map(elapsed => CostVector.Zero with { RemoteUnits = elapsed.TotalSeconds * costs.RemoteNodeSecondRate })
            .IfNone(CostVector.Zero);

    static FactMeasure Called(ComputeReceipt.RemoteCall call) {
        TagList transport = InstrumentSet.Tags((ComputeInstrument.TransportSlot, call.Transport));
        return new FactMeasure(
            Seq((ComputeInstrument.RemoteCalls.Row, 1d, transport))
                + (StringComparer.Ordinal.Equals(call.Status, ReceiptSurface.OkStatus)
                    ? Seq((ComputeInstrument.RemoteOk.Row, 1d, transport))
                    : Seq<(InstrumentSpec, double, TagList)>())
                + call.Elapsed.Match(
                    Some: elapsed => Seq((ComputeInstrument.RemoteDuration.Row, elapsed.TotalSeconds, InstrumentSet.Tags(
                        (ComputeInstrument.TransportSlot, call.Transport),
                        (ComputeInstrument.StatusSlot, call.Status)))),
                    None: static () => Seq<(InstrumentSpec, double, TagList)>()),
            CostVector.Zero);
    }

    // Subject tags the ratio pair, outcome tags the shape measures: the burn rule divides converged by runs
    // series against series, while residual and iteration distributions still split on the verdict.
    static FactMeasure Solved(ComputeReceipt.Solve solve) {
        TagList subject = InstrumentSet.Tags(
            (ComputeInstrument.PhysicsSlot, solve.Physics),
            (ComputeInstrument.MethodSlot, solve.Method));
        TagList outcome = InstrumentSet.Tags(
            (ComputeInstrument.PhysicsSlot, solve.Physics),
            (ComputeInstrument.MethodSlot, solve.Method),
            (ComputeInstrument.ConvergedSlot, solve.Converged));
        return new FactMeasure(
            Seq((ComputeInstrument.SolveRuns.Row, 1d, subject))
                + (solve.Converged ? Seq((ComputeInstrument.SolveConverged.Row, 1d, subject)) : Seq<(InstrumentSpec, double, TagList)>())
                + Seq(
                    (ComputeInstrument.SolveResidual.Row, solve.Residual, outcome),
                    (ComputeInstrument.SolveIterations.Row, (double)solve.Iterations, outcome))
                + Sharded(solve.Shards, solve.Substrate),
            CostVector.Zero);
    }

    static FactMeasure Factored(ComputeReceipt.Factorization factorization) =>
        new(Seq((ComputeInstrument.SolveFactorizations.Row, 1d, InstrumentSet.Tags(
                    (ComputeInstrument.ProviderSlot, factorization.Provider),
                    (ComputeInstrument.DecompositionSlot, factorization.Decomposition))))
                + Sharded(factorization.Shards, factorization.Substrate),
            CostVector.Zero);

    // Shard census over the ONE decomposed-execution axis both numeric cases share. The merge receipt folds shard
    // results and executes no shard of its own, so `Executes` is the row that records nothing here and the counter
    // stays the true execution count rather than the run count plus a fold. Substrate is absence-bearing on the
    // same law the cost write holds: a process-scoped receipt omits the key and the point exports untagged.
    static Seq<(InstrumentSpec Row, double Value, TagList Tags)> Sharded(ShardRole role, Option<Substrate> substrate) =>
        role.Executes
            ? Seq((ComputeInstrument.SolveShards.Row, (double)role.Count, substrate.Match(
                Some: row => InstrumentSet.Tags((ComputeInstrument.SubstrateSlot, row.Key)),
                None: static () => default)))
            : Seq<(InstrumentSpec, double, TagList)>();

    // Both severities are ALWAYS written, zero included: a confirmed-clash count of zero is a measured census over
    // a run that traversed the index, never an absent measurement, so the severity pair reads as a partition.
    static FactMeasure Clashed(ComputeReceipt.Clash clash) =>
        FactMeasure.Metered(
            (ComputeInstrument.ClashConfirmed.Row, clash.HardClashes, InstrumentSet.Tags(
                (ComputeInstrument.SeveritySlot, ReceiptSurface.HardClash),
                (ComputeInstrument.IndexSlot, clash.IndexKind.Key))),
            (ComputeInstrument.ClashConfirmed.Row, clash.ClearanceViolations, InstrumentSet.Tags(
                (ComputeInstrument.SeveritySlot, ReceiptSurface.ClearanceClash),
                (ComputeInstrument.IndexSlot, clash.IndexKind.Key))));

    // Verdicts and nominal are the population/good counter pair on ONE signal dimension, so the burn rule divides
    // series against series and the anomaly flag never splits the denominator it is measured against.
    static FactMeasure Twinned(ComputeReceipt.Twin twin) {
        TagList signal = InstrumentSet.Tags((ComputeInstrument.SignalSlot, twin.SignalId));
        return new FactMeasure(
            Seq((ComputeInstrument.TwinVerdicts.Row, 1d, signal))
                + (twin.Anomaly ? Seq<(InstrumentSpec, double, TagList)>() : Seq((ComputeInstrument.TwinNominal.Row, 1d, signal))),
            CostVector.Zero);
    }

    // Population and good series are BOTH counters from this one arm, so the burn fold and the store-side rule
    // divide one truth; the terminal marker tags the step distribution alone, where it discriminates rather than
    // splitting the ratio pair.
    static FactMeasure Traced(ComputeReceipt.Trajectory run) =>
        new(Seq((ComputeInstrument.TrajectoryRuns.Row, 1d, default))
                + (run.Resolved ? Seq((ComputeInstrument.TrajectoryResolved.Row, 1d, default)) : Seq<(InstrumentSpec, double, TagList)>())
                + Seq((ComputeInstrument.TrajectorySteps.Row, (double)run.Steps,
                    InstrumentSet.Tags((ComputeInstrument.TerminalSlot, run.Terminal)))),
            CostVector.Zero);

    static FactMeasure Assessed(ComputeReceipt.Assessment assessment) =>
        FactMeasure.Metered(
            (ComputeInstrument.AssessmentVerdicts.Row, 1d, InstrumentSet.Tags(
                (ComputeInstrument.DisciplineSlot, assessment.Discipline),
                (ComputeInstrument.VerdictSlot, assessment.Verdict))),
            (ComputeInstrument.AssessmentRatio.Row, assessment.GoverningRatio,
                InstrumentSet.Tags((ComputeInstrument.DisciplineSlot, assessment.Discipline))));
}

public static class ComputeTraces {
    // Band composition owns the one bracket: substrates ride the span name under the admitted
    // `rasm.compute.dispatch` scope, and an arm's IO rail brackets through the band's own IO overload carrying the
    // arm's `Fin<ComputeReceipt>` verdict out unerased.
    public static DispatchTable Traced(DispatchTable table, SpanBand band) => new(
        CpuTensor: Spanned(band, Substrate.CpuTensor, table.CpuTensor),
        DeviceWgpu: Spanned(band, Substrate.DeviceWgpu, table.DeviceWgpu),
        Onnx: Spanned(band, Substrate.Onnx, table.Onnx),
        GenAi: Spanned(band, Substrate.GenAi, table.GenAi),
        RemoteGrpc: Spanned(band, Substrate.RemoteGrpc, table.RemoteGrpc));

    static Func<AdmittedIntent, IO<Fin<ComputeReceipt>>> Spanned(
        SpanBand band, Substrate route, Func<AdmittedIntent, IO<Fin<ComputeReceipt>>> arm) =>
        admitted => band.Traced(
            ReceiptSurface.Dispatch,
            Op.Of(name: route.Key),
            span => IO.lift(() => Tagged(span, route, admitted)).Bind(_ => arm(admitted)));

    static Unit Tagged(Activity? span, Substrate route, AdmittedIntent admitted) =>
        ignore(span?
            .SetTag(CorrelationId.Slot, admitted.Correlation.ToString())
            .SetTag(ComputeInstrument.LaneSlot, admitted.Spec.Lane.Key)
            .SetTag(ComputeInstrument.SubstrateSlot, route.Key));
}
```

## [04]-[FOLD_PROJECTIONS]

- Owner: `ReceiptFolds` — every operational view is a pure fold over `Seq<ComputeReceipt>`, and every tenant-partitioned view is a pure fold over the `ReceiptEnvelope`-joined `Seq<(TenantContext Tenant, ComputeReceipt Fact)>` journal — the tenant is a `ReceiptEnvelope` fact, so it enters the fold INPUT through the `Journal` join, never a fabricated dimension over the bare fact stream. `ReceiptReplay`/`ReplayVerdict` — the certification-grade re-derivation fold: a content-keyed verdict re-derives from its recorded inputs and diffs against the stored payload under the receipt's determinism stamp, so a permit-submitted verdict is provable on demand instead of merely cached.
- Entry: `ReceiptFolds.Journal(Seq<ReceiptEnvelope> envelopes, ComputeWireContext wire)` — the one `ReceiptEnvelope`-decode join: Compute-package message envelopes rehydrate through the Strict wire context into `(Tenant, Fact)` rows, a decode refusal failing the rail rather than dropping a billed fact. `Cases<TCase>(Func<TCase,bool>?)` and `Tally<K>(Func<ComputeReceipt, Option<K>>)` are the TWO fold primitives every named view projects. `ReceiptReplay.Replay(UInt128 contentKey, ReadOnlyMemory<byte> stored, Option<DeterminismStamp> stamp, Func<Fin<ReadOnlyMemory<byte>>> rederive)` — the caller composes `rederive` from the settled Persistence contracts (`Version/ledger` `OpLogEntry.Closure` resolves the input manifest, `Query/cache` `ModelResultIndex.Lookup` serves the stored payload) and the verdict states reproducibility as a typed fact.
- Auto: per-lane counts, route histograms, hot-path totals, leak indicators, conflict evidence, solver-divergence and twin-anomaly extractions, numeric-provider attribution, residency-gate crossings, and provenance chains derive on read from the identical stream the dashboards consume — each one a key selector or a case predicate handed to the two primitives. Replay comparison mode derives from the stamp's own CLASS row: an exact class demands byte equality, an inexact one compares the payloads as little-endian double lanes under that row's relative-defect ceiling, and an absent stamp, an unreadable payload shape, or a failed re-derivation lands `Unreplayable` with its exact `Error` evidence.
- Packages: LanguageExt.Core, NodaTime, System.Numerics.Tensors (`TensorPrimitives.Distance`/`Norm` the inexact-class defect), BCL inbox (`MemoryMarshal` lane reinterpretation)
- Growth: a new operational view is one key selector or one case predicate over the two primitives; a new tenant-partitioned view is one member on the journal extension; a new determinism class is one `DeterminismTag` row carrying its own ceiling; zero new surface.
- Law: `Cases<TCase>` and `Tally<K>` are the ONE selection and the ONE census. The seven predicate members this replaces each re-spelled `Bind`-into-two-`Seq` around their own case test, and the seven count members each re-spelled `Fold(HashMap(), AddOrUpdate(key, c => c + 1, 1))` around their own key selector — the selector and the predicate were the whole difference. `Runtime/board#FACT_SELECTION` `FactSelector.Of<TCase>` is the erased twin of `Cases`, minted where a predicate must survive as a value.
- Law: the replay stamp is READ before the re-derivation runs, so an untagged payload never pays a re-derive it could not judge; the unrecognized-class refusal it replaces charged that cost and then declined.
- Boundary: leak indicators read `StagingEventKind.StreamDoubleDisposed` and `StreamFinalized`, while `Diagnostics` reads the row's `Diagnostic` column. `DiscardTaxonomy` folds `BufferDiscarded` detail into a reason-keyed count. Execution projections choose only facts carrying their `Option` spine values; process-scoped allocation evidence remains in provenance and diagnostic folds without a fabricated lane or route. Mutable accumulators, per-view repositories, and second fact streams reject.
- Boundary: replay never unfreezes a wire or fabricates inputs — an unresolvable closure, an absent stamp, or a payload whose byte length is empty, mismatched, or unaligned to the lane width lands `Unreplayable` with its exact `Error` evidence, never a coerced `Reproduced`. A BITWISE divergence carries no magnitude, because the payloads differ and nothing measured by how much; only the inexact class produces a defect figure, and it is the measured relative distance.
- Boundary: tenant partition enters through `Journal` alone — a tenant-keyed member over the bare fact stream fabricates a dimension its input never carried and rejects; the envelope's `Package` is the kernel `TelemetrySource` row, so the filter compares rows rather than rendered text.

```csharp signature
public static class ReceiptFolds {
    public static Fin<Seq<(TenantContext Tenant, ComputeReceipt Fact)>> Journal(Seq<ReceiptEnvelope> envelopes, ComputeWireContext wire) =>
        envelopes.Filter(static envelope => envelope.Package == ReceiptSurface.Source)
            .TraverseM(envelope => Op.Of(name: "receipt.journal-decode").Catch(() => Fin.Succ(envelope.Payload.Deserialize(wire.ComputeReceipt)))
                .Bind(fact => fact is null
                    ? Fin.Fail<(TenantContext, ComputeReceipt)>(new ComputeFault.Violation(ComputeArea.Runtime, new ComputeViolation.Required(ComputeSubject.Input)))
                    : Fin.Succ((envelope.Tenant, fact))))
            .As();

    extension(Seq<(TenantContext Tenant, ComputeReceipt Fact)> journal) {
        public HashMap<TenantId, CostVector> TenantCosts(CostPolicy costs) =>
            journal.Fold(HashMap<TenantId, CostVector>(), (acc, row) =>
                ComputeInstrumentFan.Measure(costs, row.Fact).Cost is { } priced
                    ? acc.AddOrUpdate(row.Tenant.TenantId, held => held + priced, priced)
                    : acc);

        public HashMap<(TenantId Tenant, Substrate Route), CostVector> TenantRouteCosts(CostPolicy costs) =>
            journal.Choose(row => row.Fact.Substrate.Map(route =>
                    (Key: (row.Tenant.TenantId, route), Priced: ComputeInstrumentFan.Measure(costs, row.Fact).Cost)))
                .Fold(HashMap<(TenantId, Substrate), CostVector>(), static (acc, row) => acc.AddOrUpdate(row.Key, held => held + row.Priced, row.Priced));

        public HashMap<TenantId, long> TenantFacts =>
            journal.Fold(HashMap<TenantId, long>(), static (acc, row) => acc.AddOrUpdate(row.Tenant.TenantId, static count => count + 1L, 1L));
    }

    extension(Seq<ComputeReceipt> facts) {
        // ONE typed selection over the union. Every named view below is this call plus its own predicate.
        public Seq<TCase> Cases<TCase>(Func<TCase, bool>? holds = null) where TCase : ComputeReceipt =>
            facts.Choose(fact => fact is TCase held && (holds is null || holds(held)) ? Some(held) : None).Strict();

        // ONE keyed census. The key selector answers `None` for a fact outside the population, so a filter and a
        // projection are the same pass and no view walks the stream twice.
        public HashMap<K, long> Tally<K>(Func<ComputeReceipt, Option<K>> keyOf) =>
            facts.Choose(keyOf).Fold(HashMap<K, long>(), static (acc, key) => acc.AddOrUpdate(key, static count => count + 1L, 1L));

        public HashMap<WorkLane, long> LaneCounts => facts.Tally(static fact => fact.Lane);

        public HashMap<Substrate, long> RouteHistogram => facts.Tally(static fact => fact.Substrate);

        public HashMap<Substrate, Duration> HotPathTotals =>
            facts.Choose(static fact => fact.Substrate.Bind(route => fact.Elapsed.Map(elapsed => (Route: route, Elapsed: elapsed))))
                .Fold(HashMap<Substrate, Duration>(), static (acc, row) => acc.AddOrUpdate(row.Route, total => total + row.Elapsed, row.Elapsed));

        public Seq<ComputeReceipt.Allocation> Leaks =>
            facts.Cases<ComputeReceipt.Allocation>(static row =>
                row.Event == StagingEventKind.StreamDoubleDisposed || row.Event == StagingEventKind.StreamFinalized);

        public Seq<ComputeReceipt.Allocation> Diagnostics =>
            facts.Cases<ComputeReceipt.Allocation>(static row => row.Event.Diagnostic);

        public Seq<ComputeReceipt.Conflict> Conflicts => facts.Cases<ComputeReceipt.Conflict>();

        public Seq<ComputeReceipt.Solve> Diverged => facts.Cases<ComputeReceipt.Solve>(static row => !row.Converged);

        public Seq<ComputeReceipt.Fit> Nonconverged => facts.Cases<ComputeReceipt.Fit>(static row => !row.Converged);

        public Seq<ComputeReceipt.Twin> Anomalies => facts.Cases<ComputeReceipt.Twin>(static row => row.Anomaly);

        public Seq<ComputeReceipt.Drift> Breaches => facts.Cases<ComputeReceipt.Drift>(static row => row.Breach);

        public HashMap<string, long> DiscardTaxonomy =>
            facts.Tally(static fact => fact is ComputeReceipt.Allocation { Event.Key: var kind } row
                && StringComparer.Ordinal.Equals(kind, StagingEventKind.BufferDiscarded.Key)
                    ? row.Detail
                    : None);

        public HashMap<OrtResidency, long> Crossings =>
            facts.Tally(static fact => fact is ComputeReceipt.Copy crossing ? Some(crossing.Gate) : None);

        // Refusal taxonomy folds by the fault's own numeric identity, the same reason-keyed shape `DiscardTaxonomy`
        // takes over pool discards — one view answering which interior gate is refusing, where the meter answers
        // only how often.
        public HashMap<FaultId, long> RefusalTaxonomy =>
            facts.Tally(static fact => fact is ComputeReceipt.Refusal refusal ? Some(refusal.Identity) : None);

        public HashMap<string, long> Providers =>
            facts.Tally(static fact => fact is ComputeReceipt.Factorization factorization ? Some(factorization.Provider) : None);

        public HashMap<CorrelationId, Seq<ComputeReceipt>> Provenance =>
            facts.Fold(HashMap<CorrelationId, Seq<ComputeReceipt>>(), static (acc, fact) => acc.AddOrUpdate(fact.Correlation, chain => chain.Add(fact), Seq(fact)));
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ReplayVerdict {
    private ReplayVerdict() { }

    public sealed record Reproduced(UInt128 ContentKey, DeterminismStamp Stamp) : ReplayVerdict;

    // A bitwise divergence has NO magnitude: the payloads differ and nothing measured by how much, so this case
    // carries none. The `1.0`-or-infinity placeholder it replaces published a fabricated defect on every byte
    // mismatch and an infinity on every length mismatch, and neither figure was ever measured.
    public sealed record BitwiseDiverged(UInt128 ContentKey, DeterminismStamp Stamp) : ReplayVerdict;

    public sealed record NumericDiverged(UInt128 ContentKey, DeterminismStamp Stamp, double Defect) : ReplayVerdict;

    public sealed record Unreplayable(UInt128 ContentKey, Error Evidence) : ReplayVerdict;
}

public static class ReceiptReplay {
    public static ReplayVerdict Replay(
        UInt128 contentKey,
        ReadOnlyMemory<byte> stored,
        Option<DeterminismStamp> stamp,
        Func<Fin<ReadOnlyMemory<byte>>> rederive) =>
        stamp.Match(
            Some: row => rederive().Match(
                Succ: fresh => row.Class.Exact ? Bitwise(contentKey, stored, fresh, row) : Envelope(contentKey, stored, fresh, row),
                Fail: error => (ReplayVerdict)new ReplayVerdict.Unreplayable(contentKey, error)),
            None: () => new ReplayVerdict.Unreplayable(contentKey, new ComputeFault.Violation(
                ComputeArea.Runtime,
                new ComputeViolation.Contract(ComputeContract.Witnessed, new ContractEvidence.Digest(contentKey)))));

    static ReplayVerdict Bitwise(UInt128 key, ReadOnlyMemory<byte> stored, ReadOnlyMemory<byte> fresh, DeterminismStamp stamp) =>
        stored.Span.SequenceEqual(fresh.Span)
            ? new ReplayVerdict.Reproduced(key, stamp)
            : new ReplayVerdict.BitwiseDiverged(key, stamp);

    // Shape is decided over the (stored, fresh) length product, so an empty, mismatched, or unaligned payload names
    // WHICH claim it broke rather than reporting one fused refusal three conditions could produce.
    static ReplayVerdict Envelope(UInt128 key, ReadOnlyMemory<byte> stored, ReadOnlyMemory<byte> fresh, DeterminismStamp stamp) =>
        (stored.Length, fresh.Length) switch {
            (0, _) or (_, 0) => new ReplayVerdict.Unreplayable(key, new ComputeFault.Violation(
                ComputeArea.Runtime,
                new ComputeViolation.Required(ComputeSubject.Payload))),
            var (held, derived) when held != derived => new ReplayVerdict.Unreplayable(key, new ComputeFault.Violation(
                ComputeArea.Runtime,
                new ComputeViolation.Shape(ShapeRequirement.Dimensions, new ShapeEvidence.Count(held, derived)))),
            var (held, _) when held % sizeof(double) != 0 => new ReplayVerdict.Unreplayable(key, new ComputeFault.Violation(
                ComputeArea.Runtime,
                new ComputeViolation.Shape(ShapeRequirement.Dimensions, new ShapeEvidence.Alignment(held, sizeof(double))))),
            _ => Compared(key, stored.Span, fresh.Span, stamp),
        };

    // Exemption: the span reinterpretation is the language-forced statement seam — a `ReadOnlySpan<T>` crosses no
    // lambda, so no fold expresses it; the rail resumes on the returned verdict. Little-endian is the estate's ONE
    // interchange byte order (`Rasm/Domain/frame#RECEIPT_PORT` freezes it for the causal stamp), so the lanes
    // reinterpret in place where the per-element read this replaces copied every lane twice.
    static ReplayVerdict Compared(UInt128 key, ReadOnlySpan<byte> stored, ReadOnlySpan<byte> fresh, DeterminismStamp stamp) {
        ReadOnlySpan<double> held = MemoryMarshal.Cast<byte, double>(stored);
        ReadOnlySpan<double> derived = MemoryMarshal.Cast<byte, double>(fresh);
        double defect = TensorPrimitives.Distance<double>(held, derived) / Math.Max(TensorPrimitives.Norm<double>(held), double.Epsilon);
        return double.IsFinite(defect) && defect <= stamp.Class.Ceiling
            ? new ReplayVerdict.Reproduced(key, stamp)
            : new ReplayVerdict.NumericDiverged(key, stamp, defect);
    }
}
```

## [05]-[TS_PROJECTION]

- Owner: `ComputeReceiptKind`, `ComputeReceiptSpineWire`, `ComputeReceiptWire`, `ComputeReceiptEnvelopeWire` — the receipt payload union and its envelope as the dashboard and the composing app root consume them. The claim, ledger, and descriptor wires live with their owners at `Runtime/claims#TS_PROJECTION`, `Runtime/ledger#TS_PROJECTION`, and `Runtime/board#TS_PROJECTION`.
- Packages: BCL inbox
- Growth: a new receipt case lands as one payload row on `ComputeReceiptWire`; zero new surface.
- Boundary: `ComputeReceiptKind` derives from `ReceiptSurface.Kinds` under the suite schema hash, and `ReceiptEnvelopeWire.kind` mirrors the payload discriminator.
- Boundary: a decode-only mirror carries the producer's column set ARM FOR ARM. Five payload rows drifted from live C# producers here — `sweep.unranked`/`failed`, `solve.participation`, `generate.stagedTokens`, `optimization.referenceDerived`, `clash.truncated` — and survived because no consumer census re-proved them; the payload union owes a `tests/contracts` registration so the census exists.
- Boundary: smart-enum fields cross as keys, and a closed C# vocabulary crosses as the literal union of its row keys — `cache.outcome`, `conflict.subject`, and `clash.indexKind` mirror `CacheOutcome`, `ConflictSubject`, and the `Solver/clash#CLASH_AND_TWIN` `AccelerationKind` roster exactly, so a row landed at either owner breaks the other side.
- Boundary: long values cross as decimal strings; instants and durations use their invariant textual forms.
- Boundary: optional evidence crosses as explicit null.
- Boundary: this payload union is ONE-ENDED BY DECLARATION — its producers and its dashboard/app-root consumers are all C#, `typescript:core` decodes `ReceiptEnvelopeWire` with the payload as `Schema.Unknown` (`libs/typescript/core/.planning/interchange/codec.md`) and `BenchmarkClaimWire` under `tests/contracts` `[02.13]`/`[02.14]` alone, so NO `tests/contracts` family row is owed for `ComputeReceiptWire` — a registration would assert a peer decoder no branch declares, the exact stranded state the `MachineObservationWire` withdrawal at MANIFEST `[02.13]` names; the census that holds these mirrors is the C# `[JsonDerivedType]` roster plus this section's arm-for-arm law, and a future ts payload decoder enters WITH its MANIFEST family row.

```ts signature
type ReceiptScopeWire =
  | { kind: "execution"; correlation: string; lane: string; substrate: string; allocationClass: string; elapsed: string }
  | { kind: "process"; correlation: string; allocationClass: string };

// Spine PINS its own discriminator: each payload passes the literal its `[JsonDerivedType]` row declares, so that
// roster lives exactly once — in the payload set — and `ComputeReceiptKind` derives off the union below.
interface ComputeReceiptSpineWire<K extends string> { kind: K; scope: ReceiptScopeWire; }

type SelectionDecisionWire = { outcome: "chosen"; row: string } | { outcome: "rejected"; row: string; reason: string };
type SelectionModeWire = { mode: "ranked" } | { mode: "forced"; row: string };
type ShardRoleWire = { role: "whole" } | { role: "shard"; of: number; node: string } | { role: "merge"; of: number };
type BackpressureVerdictWire = { outcome: "admitted" } | { outcome: "shed"; reason: string };
interface ModalParticipationWire { x: number; y: number; z: number; }
interface DeterminismStampWire { class: "bit" | "envelope" | "device-wgpu"; provider: string; }
interface DeltaStampWire { baseBytes: string; deltaBytes: string; }
interface TilesetCensusWire { nodes: number; leaves: number; maxDepth: number; geometricErrorRoot: number; metadataColumns: number; }
interface ConstitutiveEvidenceWire { model: string; returnMapIterations: number; returnMapResidual: number; }
interface ContactEvidenceWire { activeSet: number; penetrationResidual: number; multipliers: number; }

interface SelectionWire extends ComputeReceiptSpineWire<"selection"> { decisions: SelectionDecisionWire[]; mode: SelectionModeWire; warmAffinity: boolean; }

interface TensorRunWire extends ComputeReceiptSpineWire<"tensor-run"> { family: string; dtype: string; elements: string; simdWidth: string; partitions: number; }

interface ModelLoadWire extends ComputeReceiptSpineWire<"model-load"> { modelChecksum: string; source: string; ep: string; version: string; }

interface WarmupWire extends ComputeReceiptSpineWire<"warmup"> { modelChecksum: string; ep: string; shape: string; partitions: number | null; elapsed: string | null; warmedAt: string | null; }

interface ModelRunWire extends ComputeReceiptSpineWire<"model-run"> { modelChecksum: string; ep: string; mode: string; batchSize: number; peakBytes: string; arenaAllocator: string | null; profile: ProfileArtifactWire | null; }

interface RemoteCallWire extends ComputeReceiptSpineWire<"remote-call"> { transport: string; method: string; status: string; requestBytes: string; responseBytes: string; outcome: string; }

interface StreamSegmentWire extends ComputeReceiptSpineWire<"stream-segment"> { artifactId: string; segments: number; bytes: string; census: TilesetCensusWire | null; }

interface AllocationWire extends ComputeReceiptSpineWire<"allocation"> { event: string; requestedBytes: string; grantedBytes: string; lifetime: string | null; detail: string | null; nativeAllocator: string | null; nativeReservedBytes: string | null; smallPoolFreeBytes: string | null; largePoolFreeBytes: string | null; }

interface CopyWire extends ComputeReceiptSpineWire<"copy"> { gate: string; bytes: string; device: string; }

interface CacheWire extends ComputeReceiptSpineWire<"cache"> { outcome: "hit" | "miss" | "store" | "evict"; key: string; bytes: string; residual: number | null; delta: DeltaStampWire | null; }

interface UnitProjectionWire extends ComputeReceiptSpineWire<"unit-projection"> { family: string; originalUnit: string; originalValue: number; canonicalValue: number; }

interface BackpressureWire extends ComputeReceiptSpineWire<"backpressure"> { queueDepth: number; waited: string; verdict: BackpressureVerdictWire; }

interface DrainWire extends ComputeReceiptSpineWire<"drain"> { drained: number; faulted: number; refused: number; }

interface ConflictWire extends ComputeReceiptSpineWire<"conflict"> { subject: "retry-owner" | "contract-checksum"; evidence: string; }

interface RefusalWire extends ComputeReceiptSpineWire<"refusal"> { reason: string; subject: string; code: number; }

interface FactorizationWire extends ComputeReceiptSpineWire<"factorization"> { provider: string; decomposition: string; rows: number; cols: number; nnz: string; format: string; routeVariant: string | null; determinism: DeterminismStampWire | null; symbolicFill: number | null; residualCap: number | null; trueResidual: number | null; shards: ShardRoleWire; }

interface GenerateWire extends ComputeReceiptSpineWire<"generate"> { modelChecksum: string; ep: string; modelType: string; mode: string; adapter: string | null; tokens: number; tokensPerSecond: number; guidanceKind: string; constrainedTokens: number; toolCalls: number; seed: number | null; stagedTokens: number | null; }

interface EmbeddingWire extends ComputeReceiptSpineWire<"embedding"> { modelChecksum: string; encoding: string; dimension: number; byteLength: string; }

interface DiscretizationWire extends ComputeReceiptSpineWire<"discretization"> { algorithm: string; element: string; nodes: string; elements: string; boundaryLayers: number; refineLevel: number; worstQuality: number; metric: string; }

interface SolveWire extends ComputeReceiptSpineWire<"solve"> { physics: string; method: string; dofs: string; iterations: number; residual: number; converged: boolean; shards: ShardRoleWire; participation: ModalParticipationWire | null; constitutive: ConstitutiveEvidenceWire | null; contact: ContactEvidenceWire | null; }

interface CouplingWire extends ComputeReceiptSpineWire<"coupling"> { scheme: string; fields: number; transfers: number; rounds: number; couplingResidual: number; converged: boolean; }

interface OptimizationWire extends ComputeReceiptSpineWire<"optimization"> { optimizer: string; generations: number; evaluations: number; surrogateHits: number; frontSize: number; hypervolume: number; referenceDerived: boolean; }

interface SweepWire extends ComputeReceiptSpineWire<"sweep"> { gridPoints: string; completed: number; onFront: number; dominated: number; unranked: number; failed: number; }

interface ClashWire extends ComputeReceiptSpineWire<"clash"> { indexKind: "bvh" | "octree"; candidates: number; hardClashes: number; clearanceViolations: number; totalPairs: number; truncated: boolean; }

interface TwinWire extends ComputeReceiptSpineWire<"twin"> { signalId: string; predicted: number; measured: number; residual: number; anomaly: boolean; controlDelta: number; }

interface UncertaintyWire extends ComputeReceiptSpineWire<"uncertainty"> { method: string; samples: number; mean: number | null; variance: number | null; skewness: number | null; kurtosis: number | null; quantiles: number[]; sobolFirst: number[]; sobolTotal: number[]; interaction: number[]; mostProbablePoint: number[]; fitQuality: number | null; residualStandardError: number | null; failureProbability: number; reliabilityIndex: number; }

interface FitWire extends ComputeReceiptSpineWire<"fit"> { family: string; method: string; parameters: string; iterations: number; residual: number; converged: boolean; quality: number; qualityMetric: string; retainedRank: number | null; }

interface GovernorWire extends ComputeReceiptSpineWire<"governor"> { cpuPercent: number; memoryPercent: number; workers: number; readerCeiling: number; partitionCap: number; memoryScale: number; spillPressure: boolean; }

interface DriftWire extends ComputeReceiptSpineWire<"drift"> { monitorId: string; statistic: string; level: number; limit: number | null; breach: boolean; window: number; }

interface AssessmentWire extends ComputeReceiptSpineWire<"assessment"> { discipline: string; route: string; key: string; verdict: string; governingRatio: number | null; admitted: boolean; phase: string | null; failureKind: string | null; transient: boolean; attempt: number; participation: number | null; combination: string | null; }

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
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
