# [COMPUTE_UNION]

`ComputeReceipt` is the package's only fact vocabulary for measured execution: every operational view derives as a fold over that one stream, NodaTime-protobuf bridges own the instant/duration/calendar wire edge, and fingerprint-gated benchmark claims decide every performance-motivated route in the suite. Cases declare inline here save `Assessment`, a `partial` the `Analysis/assessment` discipline page declares on this owner while this owning index keeps its `[JsonDerivedType]` registration and widened wire projection so the whole union round-trips through the one Strict resolver.

`ComputeWireContext` joins the suite Strict-resolver merge: the polymorphic `kind` discriminator and every Thinktecture key-scalar spine field round-trip through one shared `JsonSerializerOptions`, `Seq<string>` collections intact, `UnmappedMemberHandling.Disallow` rejecting drift at the edge. `ReceiptSinkPort`, `ReceiptEnvelope`, `ScheduleEntry`, `TelemetryContributorPort`, `TenantContext`, the AppHost `InstrumentSet` capsule and minted `(ActivitySource, Meter)` pair, and the Persistence benchmark and artifact-index contracts arrive settled.

## [01]-[INDEX]

- [01]-[RECEIPT_UNION]: the fact union (inline cases and the `Analysis/assessment` `Assessment` partial), its Strict-resolver round-trip context, and sink-port emission.
- [02]-[TELEMETRY_PROJECTION]: instrument roster, bucket advice, the receipt-to-instrument fan, and the dispatch activity spine.
- [03]-[FOLD_PROJECTIONS]: operational views derive as folds over the fact stream; content-keyed verdicts re-derive and diff under the determinism tag.
- [04]-[WIRE_STAMPS]: NodaTime-protobuf bridges own the temporal wire edge.
- [05]-[BENCHMARK_CLAIMS]: fingerprint-gated claim rows decide performance routes.
- [06]-[TS_PROJECTION]: receipt payload union and benchmark-claim wire shapes.

## [02]-[RECEIPT_UNION]

- Owner: `ReceiptScope`, `SelectionDecision`, `SelectionMode`, `ComputeReceipt`, `ComputeWireContext`, `ReceiptSurface` — the scope and selection evidence families, fact union, strict serializer context, and sink-bound emission-plus-telemetry surface.
- Cases: selection · tensor-run · model-load · warmup · model-run · remote-call · stream-segment · allocation · copy · cache · unit-projection · backpressure · drain · conflict · factorization · generate · embedding · discretization · solve · coupling · optimization · sweep · clash · twin · uncertainty · fit · assessment (the last declared as a partial on this owner by `Analysis/assessment`)
- Entry: `public IO<ReceiptEnvelope> ReceiptSurface.Emit(ComputeReceipt fact)` — the surface binds sink, serializer, and mounted-instrument aspects once at composition; `IO` carries the sink effect and returns the envelope evidence.
- Auto: wire kind derives from the polymorphic metadata pinned on the union; the HLC stamp and `SkewBound` derive inside `Send`, and ambient `TenantContext.Current` threads into `Send` so the envelope `Tenant` field partitions evidence by the AppHost tenancy primitive; instrument rows register once at composition through `TelemetryContributorPort`, `Emit` folds every typed fact through the `[02]` fan before serialization, and the `[02]` `ComputeTraces` spine opens the dispatch span on the minted `TelemetrySource.Compute` source so receipt correlation joins the OTel rail with zero call-site ceremony.
- Receipt: union cases materialize at the sink edge only; hot-path capsules upstream stay allocation-free.
- Packages: Thinktecture.Runtime.Extensions, Thinktecture.Runtime.Extensions.Json, LanguageExt.Core, NodaTime, Rasm.AppHost (project), BCL inbox
- Growth: a new measured concern is one case row on `ComputeReceipt`, one `[JsonDerivedType]` row, one TS payload row, and one `[02]` projection arm, zero new surface; a discipline rail in another folder declares its case as a `partial` record on this owner (the `Analysis/assessment` `Assessment` case) while this owning index keeps the `[JsonDerivedType]` registration and the TS payload row so the polymorphic registry stays single-sited — the `[JsonDerivedType]` roster is the ONE primary correspondence: `ReceiptSurface.Kinds` projects it from the context's polymorphism metadata, the TS `ComputeReceiptKind` union generates from `Kinds` during descriptor emit under the suite schema hash, and `ReceiptSurface.Probe` proves roster-versus-case bijection at boot, so a parallel receipt union, a second discriminator registry, or a hand-maintained TS mirror that can silently go stale is the deleted form.
- Boundary: receipts are HLC-correlated through the envelope and emit only through the sink-bound `ReceiptSurface`. `ReceiptScope.Execution` carries lane, substrate, allocation, and elapsed evidence, while `Process` carries only correlation and allocation; process facts never fabricate execution context or bypass the union. Every solver, statistical-learning, generative, residency, and allocation outcome rides this union. `Selection` projects ordered hops onto `SelectionDecision` and forced presence onto `SelectionMode`, avoiding parallel rosters and nullable policy. `Allocation` carries the complete `AllocationEvidence` projection, including typed `StagingEventKind`, requested/granted bytes, detail, allocator, reservation, and pool gauges. `Uncertainty` carries distribution moments, sensitivity indices, interactions, reliability search coordinates, and explicit null moments for methods that do not estimate them. `Factorization` optional wire evidence remains case-local. Spine values serialize as Thinktecture key scalars and format without runtime format strings.

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
[JsonDerivedType(typeof(Assessment), "assessment")]
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
                ? Fin.Fail<Selection>(new ComputeFault.EquivalenceMiss($"selection-context:{receipt.Correlation}:{admitted.Correlation}:{elapsed}"))
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
                    admitted.Spec.Lane,
                    receipt.Route,
                    admitted.Spec.Allocation,
                    elapsed),
            });
    }

    public sealed record TensorRun(TensorOpFamily Family, string Dtype, long Elements, string SimdWidth, int Partitions) : ComputeReceipt;

    public sealed record ModelLoad(string ModelChecksum, string Source, ExecutionProvider Ep, long Version) : ComputeReceipt;

    public sealed record Warmup(string ModelChecksum, ExecutionProvider Ep, string Shape) : ComputeReceipt;

    public sealed record ModelRun(string ModelChecksum, ExecutionProvider Ep, string Mode, int BatchSize, long PeakBytes, string? ArenaAllocator, string? ProfileArtifact) : ComputeReceipt;

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
        public static Allocation Of(AllocationEvidence evidence) =>
            new(
                evidence.Kind,
                evidence.RequestedBytes,
                evidence.GrantedBytes,
                evidence.Detail.Match(Some: static value => value, None: static () => null),
                evidence.NativeAllocator.Match(Some: static value => value, None: static () => null),
                evidence.NativeReservedBytes.Match<long?>(Some: static value => value, None: static () => null),
                evidence.SmallPoolFreeBytes.Match<long?>(Some: static value => value, None: static () => null),
                evidence.LargePoolFreeBytes.Match<long?>(Some: static value => value, None: static () => null)) {
                Scope = new ReceiptScope.Process(evidence.Correlation, evidence.Class),
            };
    }

    public sealed record Copy(OrtResidency Gate, long Bytes, string Device) : ComputeReceipt;

    public sealed record Cache(string Outcome, string Key, long Bytes) : ComputeReceipt;

    public sealed record UnitProjection(string Family, string OriginalUnit, double OriginalValue, double CanonicalValue) : ComputeReceipt;

    public sealed record Backpressure(int QueueDepth, Duration Waited, string? Dropped) : ComputeReceipt;

    public sealed record Drain(int Drained, int Faulted, int Refused) : ComputeReceipt;

    public sealed record Conflict(string Subject, string Evidence) : ComputeReceipt;

    public sealed record Factorization(string Provider, string Decomposition, int Rows, int Cols, long Nnz, string Format) : ComputeReceipt {
        public string? RouteVariant { get; init; }
        public string? DeterminismTag { get; init; }
        public int? SymbolicFill { get; init; }
        public double? ResidualCap { get; init; }
        public double? TrueResidual { get; init; }
    }

    public sealed record Generate(string ModelChecksum, ExecutionProvider Ep, string ModelType, string Mode, string? Adapter, int Tokens, double TokensPerSecond, GuidanceKind GuidanceKind, int ConstrainedTokens, int ToolCalls) : ComputeReceipt;

    public sealed record Embedding(string ModelChecksum, string Encoding, int Dimension, long ByteLength) : ComputeReceipt;

    public sealed record Discretization(string Algorithm, string Element, long Nodes, long Elements, int BoundaryLayers, int RefineLevel, double WorstQuality, string Metric) : ComputeReceipt;

    public sealed record Solve(string Physics, string Method, long Dofs, int Iterations, double Residual, bool Converged) : ComputeReceipt;

    public sealed record Coupling(string Scheme, int Fields, int Transfers, int Rounds, double CouplingResidual, bool Converged) : ComputeReceipt;

    public sealed record Optimization(string Optimizer, int Generations, int Evaluations, int SurrogateHits, int FrontSize, double Hypervolume) : ComputeReceipt;

    public sealed record Sweep(long GridPoints, int Completed, int OnFront, int Dominated) : ComputeReceipt;

    public sealed record Clash(string IndexKind, int Candidates, int HardClashes, int ClearanceViolations, int TotalPairs) : ComputeReceipt;

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
        double FailureProbability,
        double ReliabilityIndex) : ComputeReceipt;

    public sealed record Fit(string Family, string Method, long Parameters, int Iterations, double Residual, bool Converged, double Quality, string QualityMetric, int RetainedRank) : ComputeReceipt;

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

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
    RespectNullableAnnotations = true,
    RespectRequiredConstructorParameters = true,
    Converters = [typeof(ThinktectureJsonConverterFactory), typeof(Int64StringJsonConverter)])]
[JsonSerializable(typeof(ComputeReceipt))]
[JsonSerializable(typeof(ReceiptScope))]
[JsonSerializable(typeof(SelectionDecision))]
[JsonSerializable(typeof(SelectionMode))]
[JsonSerializable(typeof(BenchmarkClaim))]
[JsonSerializable(typeof(BenchmarkInput))]
[JsonSerializable(typeof(HostFingerprint))]
public partial class ComputeWireContext : JsonSerializerContext;

public sealed class ReceiptSurface(ReceiptSinkPort sink, ComputeWireContext wire, InstrumentSet instruments) {
    private static Seq<(Type Type, string Kind)> Rows =>
        ComputeWireContext.Default.ComputeReceipt.PolymorphismOptions is { } options
            ? toSeq(options.DerivedTypes).Choose(static row => row.TypeDiscriminator is string kind ? Some((row.DerivedType, kind)) : None)
            : Seq<(Type, string)>();

    private static readonly FrozenDictionary<Type, string> KindByCase =
        Rows.ToFrozenDictionary(static row => row.Type, static row => row.Kind);

    public static Seq<string> Kinds => Rows.Map(static row => row.Kind);

    public static string KindOf(ComputeReceipt fact) => KindByCase[fact.GetType()];

    public static Fin<Unit> Probe() {
        FrozenSet<Type> cases = typeof(ComputeReceipt)
            .GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic)
            .Where(static nested => nested.IsAssignableTo(typeof(ComputeReceipt)) && !nested.IsAbstract)
            .ToFrozenSet();
        FrozenSet<Type> registered = Rows.Map(static row => row.Type).ToFrozenSet();
        bool uniqueKinds = Kinds.ToFrozenSet(StringComparer.Ordinal).Count == Kinds.Count;
        bool stringDiscriminators = ComputeWireContext.Default.ComputeReceipt.PolymorphismOptions is { } options
            && options.DerivedTypes.Count == Rows.Count;
        return stringDiscriminators && uniqueKinds && cases.SetEquals(registered)
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(ComputeFault.Create($"<kind-registry-drift:cases={cases.Count}:types={registered.Count}:kinds={Kinds.Count}>"));
    }

    public static readonly Seq<InstrumentRow> Instruments = Seq(
        new InstrumentRow(TelemetrySource.Compute, "rasm.compute.receipts.emitted", "{receipt}", "receipts emitted through the sink port by kind",
            static (meter, name, unit, text) => meter.CreateCounter<long>(name, unit, text)),
        new InstrumentRow(TelemetrySource.Compute, "rasm.compute.claims.bound", "{claim}", "benchmark claims bound for the current host fingerprint",
            static (meter, name, unit, text) => meter.CreateObservableGauge(name, static () => ComputeLevelCells.Live.ClaimsBound.Value, unit, text)),
        new InstrumentRow(TelemetrySource.Compute, "rasm.compute.solve.factorizations", "{factorization}", "dense and sparse factorizations by provider and kind",
            static (meter, name, unit, text) => meter.CreateCounter<long>(name, unit, text)),
        new InstrumentRow(TelemetrySource.Compute, "rasm.compute.solve.residual", "1", "iterative-solver convergence residual",
            static (meter, name, unit, text) => ComputeBuckets.Advised(meter, name, unit, text, ComputeBuckets.ResidualDecades)),
        new InstrumentRow(TelemetrySource.Compute, "rasm.compute.solve.iterations", "{iteration}", "iterative-solve iteration counts to convergence or frame-budget stop",
            static (meter, name, unit, text) => ComputeBuckets.Advised(meter, name, unit, text, ComputeBuckets.IterationCounts)),
        new InstrumentRow(TelemetrySource.Compute, "rasm.compute.generate.tokens", "{token}", "tokens emitted through the generative run loop by run mode, adapter, and guidance",
            static (meter, name, unit, text) => meter.CreateCounter<long>(name, unit, text)),
        new InstrumentRow(TelemetrySource.Compute, "rasm.compute.optimize.hypervolume", "1", "Pareto-front hypervolume indicator per optimizer generation",
            static (meter, name, unit, text) => ComputeBuckets.Advised(meter, name, unit, text, ComputeBuckets.Hypervolume)),
        new InstrumentRow(TelemetrySource.Compute, "rasm.compute.mesh.elements", "{element}", "volumetric elements generated per discretization and refinement level",
            static (meter, name, unit, text) => meter.CreateCounter<long>(name, unit, text)),
        new InstrumentRow(TelemetrySource.Compute, "rasm.compute.clash.confirmed", "{clash}", "confirmed clashes by severity and federated-index kind",
            static (meter, name, unit, text) => meter.CreateCounter<long>(name, unit, text)),
        new InstrumentRow(TelemetrySource.Compute, "rasm.compute.twin.anomalies", "{anomaly}", "digital-twin anomaly verdicts exceeding the ROM error bound",
            static (meter, name, unit, text) => meter.CreateCounter<long>(name, unit, text)),
        new InstrumentRow(TelemetrySource.Compute, "rasm.compute.remote.duration", "s", "remote transport wall duration per call by transport and status",
            static (meter, name, unit, text) => ComputeBuckets.Advised(meter, name, unit, text, ComputeBuckets.RemoteSeconds)),
        new InstrumentRow(TelemetrySource.Compute, "rasm.compute.assessment.verdicts", "{assessment}", "discipline assessments by discipline and verdict",
            static (meter, name, unit, text) => meter.CreateCounter<long>(name, unit, text)),
        new InstrumentRow(TelemetrySource.Compute, "rasm.compute.assessment.ratio", "1", "governing utilization ratio per assessment by discipline",
            static (meter, name, unit, text) => ComputeBuckets.Advised(meter, name, unit, text, ComputeBuckets.GoverningRatio)),
        new InstrumentRow(TelemetrySource.Compute, "rasm.compute.progress.marks", "{mark}", "progress marks delivered through the cadence gate by phase",
            static (meter, name, unit, text) => meter.CreateCounter<long>(name, unit, text)),
        new InstrumentRow(TelemetrySource.Compute, "rasm.compute.progress.cadence", "s", "interval between consecutive delivered progress marks by phase",
            static (meter, name, unit, text) => ComputeBuckets.Advised(meter, name, unit, text, ComputeBuckets.CadenceSeconds)));

    public static TelemetryContributorPort Telemetry(string version) => new(TelemetrySource.Compute, version, Instruments);

    public IO<ReceiptEnvelope> Emit(ComputeReceipt fact) =>
        IO.lift(() => ComputeInstrumentFan.Project(instruments, fact))
            .Bind(_ => IO.lift(() => JsonSerializer.SerializeToElement(fact, wire.ComputeReceipt)))
            .Bind(payload => sink.Send(fact.Correlation, TenantContext.Current, TelemetrySource.Compute.Key, KindOf(fact), payload));
}
```

## [03]-[TELEMETRY_PROJECTION]

- Owner: `ComputeBuckets` — the explicit-bucket advice rows and the generic advised-histogram factory; `ComputeLevelCells` — the level atoms observable gauges read at collection cadence; `ComputeInstrumentFan` — the one receipt-to-instrument projection over the typed union; `ComputeTraces` — the dispatch activity spine over the minted `TelemetrySource.Compute` source.
- Entry: `ComputeInstrumentFan.Mount(Meter meter)` materializes the `[01]` roster once over the minted meter into the AppHost `InstrumentSet` capsule; `ComputeInstrumentFan.Project(InstrumentSet set, ComputeReceipt fact)` folds one typed fact into instrument writes through the generated total `Switch`; `ComputeTraces.Traced(DispatchTable table, ActivitySource source)` decorates every dispatch arm with the substrate-keyed span.
- Auto: `ReceiptSurface.Emit` runs `Project` on the typed fact before serialization, so every emitted receipt projects with zero call-site metering and the kind space partitions cleanly — the AppHost fan owns AppHost kinds, this `Switch` owns Compute kinds, and one envelope kind projects in exactly one fan; every histogram row ships `InstrumentAdvice<T>` explicit-bucket boundaries as the fallback a backend without base2-exponential histograms reads; the trace-based exemplar filter at the provider joins any measurement recorded inside the live dispatch span to its trace and span ids with zero wiring; the composition root taps `Bound` where the boot-frozen `Runtime/admission#SUBSTRATE_AXIS` `BenchmarkRank` projection resolves, folding the fingerprint-matched claim count into the level cell the `rasm.compute.claims.bound` gauge reads.
- Receipt: none — the fan is a projection of receipts; an instrument write beside it is a second truth.
- Packages: LanguageExt.Core, Rasm.AppHost (project), BCL inbox
- Growth: a new receipt case breaks the projection `Switch` at compile time, so every new kind decides its instrument writes or returns `unit` explicitly; a new instrument is one `[01]` roster row and one arm edit; a level-shaped fact is one `ComputeLevelCells` atom and one observable-gauge row.
- Boundary: instruments stay curated aggregates, never 1:1 with cases — remote latency reads the scope `Elapsed` off `RemoteCall`, clash counts split hard and clearance on a severity tag, assessment rows carry discipline and verdict dimensions, and every tag fan rides the AppHost tenant cardinality cap; the span opens at dispatch and closes on the arm's own rail through the bracket, `SetStatus(ActivityStatusCode.Error)` the typed verdict on the fail leg; receipt payload identifiers — checksums, content keys, artifact ids, provider names — sit in the `Operational`/`Internal` classification tiers whose redactor rows pass, so per-field `DataClassification` attributes never land on receipt cases and redaction custody stays at the AppHost egress seam.

```csharp signature
public static class ComputeBuckets {
    public static readonly ImmutableArray<double> ResidualDecades = [1e-9, 1e-8, 1e-7, 1e-6, 1e-5, 1e-4, 1e-3, 1e-2, 1e-1, 1];
    public static readonly ImmutableArray<long> IterationCounts = [1, 2, 5, 10, 25, 50, 100, 250, 500, 1000, 2500];
    public static readonly ImmutableArray<double> Hypervolume = [0.05, 0.1, 0.2, 0.35, 0.5, 0.65, 0.8, 0.9, 0.95, 1];
    public static readonly ImmutableArray<double> GoverningRatio = [0.25, 0.5, 0.75, 0.9, 1, 1.1, 1.25, 1.5, 2, 4];
    public static readonly ImmutableArray<double> RemoteSeconds = [0.005, 0.01, 0.025, 0.05, 0.1, 0.25, 0.5, 1, 2.5, 5, 10, 30];
    public static readonly ImmutableArray<double> CadenceSeconds = [0.05, 0.1, 0.25, 0.5, 1, 2.5, 5, 15, 60];

    public static Histogram<T> Advised<T>(Meter meter, string name, string unit, string text, ImmutableArray<T> bounds)
        where T : struct =>
        meter.CreateHistogram<T>(name, unit, text, tags: null, advice: new InstrumentAdvice<T> { HistogramBucketBoundaries = bounds });
}

public sealed record ComputeLevelCells(Atom<long> ClaimsBound) {
    public static readonly ComputeLevelCells Live = new(Atom(0L));
}

public static class ComputeInstrumentFan {
    public static InstrumentSet Mount(Meter meter) =>
        new(ReceiptSurface.Instruments.ToFrozenDictionary(
            static row => row.Name, row => row.Bind(meter, row.Name, row.Unit, row.Description), StringComparer.Ordinal));

    public static Unit Bound(BenchmarkRank ranks) =>
        ignore(ComputeLevelCells.Live.ClaimsBound.Swap(_ => ranks.Ranks.Count));

    public static Unit Project(InstrumentSet set, ComputeReceipt fact) {
        ignore(set.Count("rasm.compute.receipts.emitted", 1L, new KeyValuePair<string, object?>("kind", ReceiptSurface.KindOf(fact))));
        return fact.Switch(
            state: set,
            selection: static (_, _) => unit,
            tensorRun: static (_, _) => unit,
            modelLoad: static (_, _) => unit,
            warmup: static (_, _) => unit,
            modelRun: static (_, _) => unit,
            remoteCall: static (s, r) => r.Elapsed.Match(
                Some: elapsed => s.Record("rasm.compute.remote.duration", elapsed.TotalSeconds,
                    new KeyValuePair<string, object?>("transport", r.Transport), new KeyValuePair<string, object?>("status", r.Status)),
                None: static () => unit),
            streamSegment: static (_, _) => unit,
            allocation: static (_, _) => unit,
            copy: static (_, _) => unit,
            cache: static (_, _) => unit,
            unitProjection: static (_, _) => unit,
            backpressure: static (_, _) => unit,
            drain: static (_, _) => unit,
            conflict: static (_, _) => unit,
            factorization: static (s, f) => s.Count("rasm.compute.solve.factorizations", 1L,
                new KeyValuePair<string, object?>("provider", f.Provider), new KeyValuePair<string, object?>("decomposition", f.Decomposition)),
            generate: static (s, g) => s.Count("rasm.compute.generate.tokens", g.Tokens,
                new KeyValuePair<string, object?>("run.mode", g.Mode),
                new KeyValuePair<string, object?>("lora.adapter", g.Adapter),
                new KeyValuePair<string, object?>("guidance", g.GuidanceKind.Key)),
            embedding: static (_, _) => unit,
            discretization: static (s, d) => s.Count("rasm.compute.mesh.elements", d.Elements,
                new KeyValuePair<string, object?>("algorithm", d.Algorithm), new KeyValuePair<string, object?>("refine", d.RefineLevel)),
            solve: static (s, v) => Solved(s, v),
            coupling: static (_, _) => unit,
            optimization: static (s, o) => s.Record("rasm.compute.optimize.hypervolume", o.Hypervolume,
                new KeyValuePair<string, object?>("optimizer", o.Optimizer)),
            sweep: static (_, _) => unit,
            clash: static (s, c) => Clashed(s, c),
            twin: static (s, t) => t.Anomaly
                ? s.Count("rasm.compute.twin.anomalies", 1L, new KeyValuePair<string, object?>("signal", t.SignalId))
                : unit,
            uncertainty: static (_, _) => unit,
            fit: static (_, _) => unit,
            assessment: static (s, a) => Assessed(s, a));
    }

    static Unit Solved(InstrumentSet set, ComputeReceipt.Solve solve) {
        KeyValuePair<string, object?>[] tags =
            [new("physics", solve.Physics), new("method", solve.Method), new("converged", solve.Converged)];
        ignore(set.Record("rasm.compute.solve.residual", solve.Residual, tags));
        return set.Record("rasm.compute.solve.iterations", (long)solve.Iterations, tags);
    }

    static Unit Clashed(InstrumentSet set, ComputeReceipt.Clash clash) {
        ignore(set.Count("rasm.compute.clash.confirmed", clash.HardClashes,
            new KeyValuePair<string, object?>("severity", "hard"), new KeyValuePair<string, object?>("index", clash.IndexKind)));
        return set.Count("rasm.compute.clash.confirmed", clash.ClearanceViolations,
            new KeyValuePair<string, object?>("severity", "clearance"), new KeyValuePair<string, object?>("index", clash.IndexKind));
    }

    static Unit Assessed(InstrumentSet set, ComputeReceipt.Assessment assessment) {
        ignore(set.Count("rasm.compute.assessment.verdicts", 1L,
            new KeyValuePair<string, object?>("discipline", assessment.Discipline), new KeyValuePair<string, object?>("verdict", assessment.Verdict)));
        return set.Record("rasm.compute.assessment.ratio", assessment.GoverningRatio,
            new KeyValuePair<string, object?>("discipline", assessment.Discipline));
    }
}

public static class ComputeTraces {
    public static DispatchTable Traced(DispatchTable table, ActivitySource source) => new(
        CpuTensor: Spanned(source, Substrate.CpuTensor, table.CpuTensor),
        DeviceWgpu: Spanned(source, Substrate.DeviceWgpu, table.DeviceWgpu),
        Onnx: Spanned(source, Substrate.Onnx, table.Onnx),
        GenAi: Spanned(source, Substrate.GenAi, table.GenAi),
        RemoteGrpc: Spanned(source, Substrate.RemoteGrpc, table.RemoteGrpc));

    static Func<AdmittedIntent, IO<Unit>> Spanned(ActivitySource source, Substrate route, Func<AdmittedIntent, IO<Unit>> arm) =>
        admitted => IO.lift(() => Started(source, route, admitted)).Bracket(
            span => (arm(admitted) | @catch<IO, Unit>(static _ => true, error => IO.fail<Unit>(Marked(span, error)))).As(),
            static span => IO.lift(() => ignore(span.Iter(static live => live.Dispose()))));

    static Option<Activity> Started(ActivitySource source, Substrate route, AdmittedIntent admitted) =>
        source.HasListeners()
            ? Optional(source.StartActivity($"rasm.compute.dispatch.{route.Key}", ActivityKind.Internal))
                .Map(span => span
                    .SetTag(nameof(CorrelationId), admitted.Correlation.ToString())
                    .SetTag("rasm.compute.lane", admitted.Spec.Lane.Key)
                    .SetTag("rasm.compute.substrate", route.Key))
            : None;

    static Error Marked(Option<Activity> span, Error error) =>
        (span.Iter(live => ignore(live.SetStatus(ActivityStatusCode.Error, error.Message))), error).Item2;
}
```

## [04]-[FOLD_PROJECTIONS]

- Owner: `ReceiptFolds` — every operational view is a pure fold over `Seq<ComputeReceipt>`; the fact stream is the single source and no projection accumulates mutably. `ReceiptReplay`/`ReplayVerdict` — the certification-grade re-derivation fold: a content-keyed verdict re-derives from its recorded inputs and diffs against the stored payload under the receipt's determinism tag, so a permit-submitted verdict is provable on demand instead of merely cached.
- Entry: `public HashMap<CorrelationId, Seq<ComputeReceipt>> Provenance` — the model-result provenance projection joining every receipt chain by correlation. `ReceiptReplay.Replay(UInt128 contentKey, ReadOnlyMemory<byte> stored, Option<string> determinismTag, Func<Fin<ReadOnlyMemory<byte>>> rederive)` — the caller composes `rederive` from the settled Persistence contracts (`Version/ledger` `OpLogEntry.Closure` resolves the input manifest, `Query/cache` `ModelResultIndex.Lookup` serves the stored payload) and the verdict states reproducibility as a typed fact.
- Auto: per-lane counts, route histograms, hot-path totals, leak indicators, conflict evidence, solver-divergence and twin-anomaly extractions, numeric-provider attribution, residency-gate crossings, and provenance chains derive on read from the identical stream the dashboards consume. Replay comparison mode derives from the CLOSED determinism-tag grammar — a `bit*` tag demands byte equality, an `envelope*`/`device-wgpu*` tag compares the payloads as little-endian double lanes under the relative defect the tag's provider triple licenses, and an unrecognized tag, an absent tag, or a failed re-derivation lands `Unreplayable` with its reason — never a caller-chosen comparison the tag contradicts and never a fabricated bitwise class.
- Packages: LanguageExt.Core, NodaTime, System.Numerics.Tensors (`TensorPrimitives.Distance`/`Norm` the envelope defect), BCL inbox (`BinaryPrimitives` lane decode)
- Growth: a new operational view is one fold member row over the same fact stream; a new determinism class is one comparison arm on `ReceiptReplay` keyed by its tag grammar; zero new surface.
- Boundary: leak indicators read `StagingEventKind.StreamDoubleDisposed` and `StreamFinalized`, while `Diagnostics` reads the row's `Diagnostic` column. `DiscardTaxonomy` folds `BufferDiscarded` detail into a reason-keyed count. Execution projections choose only facts carrying their `Option` spine values; process-scoped allocation evidence remains in provenance and diagnostic folds without a fabricated lane or route. Mutable accumulators, per-view repositories, and second fact streams reject. Replay never unfreezes a wire or fabricates inputs — an unresolvable closure, an absent tag where the payload is not byte-comparable, or a non-8-aligned envelope payload lands `Unreplayable` with its reason, never a coerced `Reproduced`.

```csharp signature
public static class ReceiptFolds {
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

        public Seq<ComputeReceipt.Solve> Diverged =>
            facts.Bind(static fact => fact is ComputeReceipt.Solve { Converged: false } stalled ? Seq(stalled) : Seq<ComputeReceipt.Solve>());

        public Seq<ComputeReceipt.Fit> Nonconverged =>
            facts.Bind(static fact => fact is ComputeReceipt.Fit { Converged: false } stalled ? Seq(stalled) : Seq<ComputeReceipt.Fit>());

        public Seq<ComputeReceipt.Twin> Anomalies =>
            facts.Bind(static fact => fact is ComputeReceipt.Twin { Anomaly: true } flagged ? Seq(flagged) : Seq<ComputeReceipt.Twin>());

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

## [05]-[WIRE_STAMPS]

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

## [06]-[BENCHMARK_CLAIMS]

- Owner: `BenchmarkInput`, `BenchmarkClaim`, `HostFingerprint` — the admitted tensor-shape/stride/density class, measured claim row, and effective-host identity that gates it; a claim is data, never prose.
- Entry: `public Option<BenchmarkRow> Claim(ModelResultIndex index, Seq<BenchmarkRow> rows)` — delegates fingerprint and recency admission to the Persistence `ModelResultIndex.Claim` owner (its horizon and clock are closed inside the index; no call shape can omit or replace them); `None` is the fall-through to the static cost rank on the substrate row.
- Auto: `BenchmarkInput.Admit` validates payload size, dtype, shape, strides, batch, and density, derives rank and contiguity, and classifies the payload band. `Key` includes the full input class, route, provider, and tolerance class. `Persist` projects the claim onto the persisted row; `Stale` compares the full effective fingerprint, including container-limited processors. `Sweep` registers the equivalence cadence row on `WorkLane.Benchmark`.
- Receipt: every sweep run emits `TensorRun`/`ModelRun` receipts beside the persisted row; artifacts — chrome-trace profiles, BenchmarkDotNet exports, EP-context caches — land as `ArtifactIndexRow` paths on the blob lane and ride the `Artifacts` rows on the claim.
- Packages: BenchmarkDotNet, NodaTime, LanguageExt.Core, Rasm.AppHost (project), Rasm.Persistence (project), BCL inbox
- Growth: a new performance surface is one claim row; a new claim dimension is one column on `BenchmarkClaim`; zero new surface.
- Boundary: SIMD routes, compression, partitioning, DATAS values, and numeric-provider ranks bind only behind a winning claim whose full fingerprint and input class match. `Provider` carries the numeric-lane key while `Substrate` remains the execution discriminant. `Stamps` includes the provider determinism tag, admitted package versions, device identity, and runtime posture; `Processors` uses `CpuBudget.Total`, never ambient host count under a container limit. Shape, strides, batch, density, route, and tolerance participate in identity, preventing a contiguous micro-vector claim from winning for a strided batched tensor. Samples, warmups, mean, deviation, median, and P95 remain claim evidence while Persistence owns recency.

```csharp signature
public sealed record HostFingerprint(string Os, string Arch, int Processors, FrozenDictionary<string, string> Stamps) : ISpanFormattable, IUtf8SpanFormattable {
    public static HostFingerprint Current(FrozenDictionary<string, string> stamps, CpuBudget budget) =>
        new(RuntimeInformation.OSDescription, RuntimeInformation.ProcessArchitecture.ToString(), budget.Total, stamps);

    public Option<BenchmarkRow> Claim(ModelResultIndex index, Seq<BenchmarkRow> rows) => index.Claim(rows, ToString());

    public string StampLine() =>
        string.Join(',', Stamps.OrderBy(static pair => pair.Key, StringComparer.Ordinal).Select(static pair => $"{pair.Key}={pair.Value}"));

    public override string ToString() => string.Create(CultureInfo.InvariantCulture, $"{Os}|{Arch}|{Processors}|{StampLine()}");

    public string ToString(string? format, IFormatProvider? formatProvider) => ToString();

    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider) =>
        destination.TryWrite($"{Os}|{Arch}|{Processors}|{StampLine()}", out charsWritten);

    public bool TryFormat(Span<byte> utf8Destination, out int bytesWritten, ReadOnlySpan<char> format, IFormatProvider? provider) =>
        Utf8.TryWrite(utf8Destination, $"{Os}|{Arch}|{Processors}|{StampLine()}", out bytesWritten);
}

public sealed record BenchmarkInput {
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
    public Seq<long> Shape { get; }
    public Seq<long> Strides { get; }
    public int Batch { get; }
    public double Density { get; }
    public int Rank => Shape.Count;
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
            : Fin.Fail<BenchmarkInput>(new ComputeFault.PayloadOverBounds($"benchmark-input:{string.Join(',', violations)}"));
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
        string family,
        string route,
        string provider,
        Duration mean,
        Duration median,
        Duration p95,
        Duration stdDev,
        int samples,
        int warmups,
        long allocatedBytes,
        double equivalenceMaxDeviation,
        string toleranceClass,
        HostFingerprint fingerprint,
        Seq<string> artifacts,
        Instant at) {
        Input = input;
        Substrate = substrate;
        Family = family;
        Route = route;
        Provider = provider;
        Mean = mean;
        Median = median;
        P95 = p95;
        StdDev = stdDev;
        Samples = samples;
        Warmups = warmups;
        AllocatedBytes = allocatedBytes;
        EquivalenceMaxDeviation = equivalenceMaxDeviation;
        ToleranceClass = toleranceClass;
        Fingerprint = fingerprint;
        Artifacts = artifacts;
        At = at;
    }

    public BenchmarkInput Input { get; }
    public Substrate Substrate { get; }
    public string Family { get; }
    public string Route { get; }
    public string Provider { get; }
    public Duration Mean { get; }
    public Duration Median { get; }
    public Duration P95 { get; }
    public Duration StdDev { get; }
    public int Samples { get; }
    public int Warmups { get; }
    public long AllocatedBytes { get; }
    public double EquivalenceMaxDeviation { get; }
    public string ToleranceClass { get; }
    public HostFingerprint Fingerprint { get; }
    public Seq<string> Artifacts { get; }
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
        string family,
        string route,
        string provider,
        Duration mean,
        Duration median,
        Duration p95,
        Duration stdDev,
        int samples,
        int warmups,
        long allocatedBytes,
        double equivalenceMaxDeviation,
        string toleranceClass,
        HostFingerprint fingerprint,
        Seq<string> artifacts,
        Instant at) {
        Seq<string> violations =
            (string.IsNullOrWhiteSpace(family) ? Seq("family") : Seq<string>())
            + (string.IsNullOrWhiteSpace(route) ? Seq("route") : Seq<string>())
            + (string.IsNullOrWhiteSpace(provider) ? Seq("provider") : Seq<string>())
            + (mean < Duration.Zero || median < Duration.Zero || p95 < median || stdDev < Duration.Zero ? Seq("distribution") : Seq<string>())
            + (samples < 2 || warmups < 0 ? Seq("protocol") : Seq<string>())
            + (allocatedBytes < 0L ? Seq("allocation") : Seq<string>())
            + (!double.IsFinite(equivalenceMaxDeviation) || equivalenceMaxDeviation < 0d ? Seq("equivalence") : Seq<string>())
            + (string.IsNullOrWhiteSpace(toleranceClass) ? Seq("tolerance") : Seq<string>())
            + (fingerprint.Processors <= 0 ? Seq("fingerprint") : Seq<string>())
            + (artifacts.Exists(string.IsNullOrWhiteSpace) ? Seq("artifact") : Seq<string>());
        return violations.IsEmpty
            ? Fin.Succ(new BenchmarkClaim(
                input, substrate, family, route, provider, mean, median, p95, stdDev, samples, warmups,
                allocatedBytes, equivalenceMaxDeviation, toleranceClass, fingerprint, artifacts, at))
            : Fin.Fail<BenchmarkClaim>(new ComputeFault.EquivalenceMiss($"benchmark-claim:{string.Join(',', violations)}"));
    }

    public string Key() => string.Create(CultureInfo.InvariantCulture, $"{Input.Key()}|{Substrate.Key}|{Family}|{Route}|{Provider}|{ToleranceClass}");

    public BenchmarkRow Persist() => new(Key(), Route, Median, P95, AllocatedBytes, Fingerprint.ToString(), At);

    public bool Stale(HostFingerprint current) => !StringComparer.Ordinal.Equals(Fingerprint.ToString(), current.ToString());
}
```

## [07]-[TS_PROJECTION]

- Owner: `ComputeReceiptKind`, `ComputeReceiptSpineWire`, `ComputeReceiptWire`, `ComputeReceiptEnvelopeWire`, `HostFingerprintWire`, `BenchmarkClaimWire` — the receipt payload union and the claim row as the dashboard consumes them.
- Packages: BCL inbox
- Growth: a new receipt case lands as one payload row on `ComputeReceiptWire`, zero new surface.
- Boundary: `ComputeReceiptKind` is a generated projection of `ReceiptSurface.Kinds` — emitted during the descriptor build and gated by the suite schema hash, never a hand-maintained mirror; payloads bind as `TPayload` through `ReceiptEnvelopeWire` with the envelope `kind` mirroring the payload discriminator; smart-enum spine fields cross as their key scalars; `long` values cross as invariant decimal strings through `Int64StringJsonConverter`, while `Instant` and `Duration` cross as ISO-8601 and roundtrip-pattern strings; absent evidence crosses as explicit null, never as an omitted member.

```ts signature
type ComputeReceiptKind =
  | "selection" | "tensor-run" | "model-load" | "warmup" | "model-run" | "remote-call"
  | "stream-segment" | "allocation" | "copy" | "cache" | "unit-projection" | "backpressure" | "drain" | "conflict"
  | "factorization" | "generate" | "embedding"
  | "discretization" | "solve" | "coupling" | "optimization" | "sweep" | "clash" | "twin" | "uncertainty" | "fit" | "assessment";

type ReceiptScopeWire =
  | { kind: "execution"; correlation: string; lane: string; substrate: string; allocationClass: string; elapsed: string }
  | { kind: "process"; correlation: string; allocationClass: string };

interface ComputeReceiptSpineWire { kind: ComputeReceiptKind; scope: ReceiptScopeWire; }

type SelectionDecisionWire = { outcome: "chosen"; row: string } | { outcome: "rejected"; row: string; reason: string };
type SelectionModeWire = { mode: "ranked" } | { mode: "forced"; row: string };
interface SelectionWire extends ComputeReceiptSpineWire { kind: "selection"; decisions: SelectionDecisionWire[]; mode: SelectionModeWire; warmAffinity: boolean; }

interface TensorRunWire extends ComputeReceiptSpineWire { kind: "tensor-run"; family: string; dtype: string; elements: string; simdWidth: string; partitions: number; }

interface ModelLoadWire extends ComputeReceiptSpineWire { kind: "model-load"; modelChecksum: string; source: string; ep: string; version: string; }

interface WarmupWire extends ComputeReceiptSpineWire { kind: "warmup"; modelChecksum: string; ep: string; shape: string; }

interface ModelRunWire extends ComputeReceiptSpineWire { kind: "model-run"; modelChecksum: string; ep: string; mode: string; batchSize: number; peakBytes: string; arenaAllocator: string | null; profileArtifact: string | null; }

interface RemoteCallWire extends ComputeReceiptSpineWire { kind: "remote-call"; transport: string; method: string; status: string; requestBytes: string; responseBytes: string; outcome: string; }

interface StreamSegmentWire extends ComputeReceiptSpineWire { kind: "stream-segment"; artifactId: string; segments: number; bytes: string; }

interface AllocationWire extends ComputeReceiptSpineWire { kind: "allocation"; event: string; requestedBytes: string; grantedBytes: string; detail: string | null; nativeAllocator: string | null; nativeReservedBytes: string | null; smallPoolFreeBytes: string | null; largePoolFreeBytes: string | null; }

interface CopyWire extends ComputeReceiptSpineWire { kind: "copy"; gate: string; bytes: string; device: string; }

interface CacheWire extends ComputeReceiptSpineWire { kind: "cache"; outcome: "hit" | "miss" | "store" | "evict"; key: string; bytes: string; }

interface UnitProjectionWire extends ComputeReceiptSpineWire { kind: "unit-projection"; family: string; originalUnit: string; originalValue: number; canonicalValue: number; }

interface BackpressureWire extends ComputeReceiptSpineWire { kind: "backpressure"; queueDepth: number; waited: string; dropped: string | null; }

interface DrainWire extends ComputeReceiptSpineWire { kind: "drain"; drained: number; faulted: number; refused: number; }

interface ConflictWire extends ComputeReceiptSpineWire { kind: "conflict"; subject: "retry-owner" | "contract-checksum"; evidence: string; }

interface FactorizationWire extends ComputeReceiptSpineWire { kind: "factorization"; provider: string; decomposition: string; rows: number; cols: number; nnz: string; format: string; routeVariant: string | null; determinismTag: string | null; symbolicFill: number | null; residualCap: number | null; trueResidual: number | null; }

interface GenerateWire extends ComputeReceiptSpineWire { kind: "generate"; modelChecksum: string; ep: string; modelType: string; mode: string; adapter: string | null; tokens: number; tokensPerSecond: number; guidanceKind: string; constrainedTokens: number; toolCalls: number; }

interface EmbeddingWire extends ComputeReceiptSpineWire { kind: "embedding"; modelChecksum: string; encoding: string; dimension: number; byteLength: string; }

interface DiscretizationWire extends ComputeReceiptSpineWire { kind: "discretization"; algorithm: string; element: string; nodes: string; elements: string; boundaryLayers: number; refineLevel: number; worstQuality: number; metric: string; }

interface SolveWire extends ComputeReceiptSpineWire { kind: "solve"; physics: string; method: string; dofs: string; iterations: number; residual: number; converged: boolean; }

interface CouplingWire extends ComputeReceiptSpineWire { kind: "coupling"; scheme: string; fields: number; transfers: number; rounds: number; couplingResidual: number; converged: boolean; }

interface OptimizationWire extends ComputeReceiptSpineWire { kind: "optimization"; optimizer: string; generations: number; evaluations: number; surrogateHits: number; frontSize: number; hypervolume: number; }

interface SweepWire extends ComputeReceiptSpineWire { kind: "sweep"; gridPoints: string; completed: number; onFront: number; dominated: number; }

interface ClashWire extends ComputeReceiptSpineWire { kind: "clash"; indexKind: "bvh" | "octree" | "sdf"; candidates: number; hardClashes: number; clearanceViolations: number; totalPairs: number; }

interface TwinWire extends ComputeReceiptSpineWire { kind: "twin"; signalId: string; predicted: number; measured: number; residual: number; anomaly: boolean; controlDelta: number; }

interface UncertaintyWire extends ComputeReceiptSpineWire { kind: "uncertainty"; method: string; samples: number; mean: number | null; variance: number | null; skewness: number | null; kurtosis: number | null; quantiles: number[]; sobolFirst: number[]; sobolTotal: number[]; interaction: number[]; mostProbablePoint: number[]; failureProbability: number; reliabilityIndex: number; }

interface FitWire extends ComputeReceiptSpineWire { kind: "fit"; family: string; method: string; parameters: string; iterations: number; residual: number; converged: boolean; quality: number; qualityMetric: string; retainedRank: number; }

interface AssessmentWire extends ComputeReceiptSpineWire { kind: "assessment"; discipline: string; route: string; key: string; verdict: string; governingRatio: number; admitted: boolean; phase: string | null; failureKind: string | null; transient: boolean; attempt: number; participation: number | null; combination: string | null; }

type ComputeReceiptWire =
  | SelectionWire | TensorRunWire | ModelLoadWire | WarmupWire | ModelRunWire | RemoteCallWire | StreamSegmentWire
  | AllocationWire | CopyWire | CacheWire | UnitProjectionWire | BackpressureWire | DrainWire | ConflictWire | FactorizationWire | GenerateWire | EmbeddingWire
  | DiscretizationWire | SolveWire | CouplingWire | OptimizationWire | SweepWire | ClashWire | TwinWire | UncertaintyWire | FitWire | AssessmentWire;

type ComputeReceiptEnvelopeWire = ReceiptEnvelopeWire<ComputeReceiptWire>;

interface HostFingerprintWire { os: string; arch: string; processors: number; stamps: Record<string, string>; }

interface BenchmarkInputWire { payloadBytes: string; band: "micro" | "small" | "medium" | "large"; dtype: string; shape: string[]; strides: string[]; batch: number; density: number; rank: number; contiguous: boolean; }

interface BenchmarkClaimWire { input: BenchmarkInputWire; substrate: string; family: string; route: string; provider: string; mean: string; median: string; p95: string; stdDev: string; samples: number; warmups: number; allocatedBytes: string; equivalenceMaxDeviation: number; toleranceClass: string; fingerprint: HostFingerprintWire; artifacts: string[]; at: string; }
```

## [08]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
