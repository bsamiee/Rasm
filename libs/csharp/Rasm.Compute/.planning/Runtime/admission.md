# [COMPUTE_ADMISSION]

Rasm.Compute admits every substrate-routed execution request through one `ComputeIntent` union under the spine-declared `Spec` policy record it adopts whole, routes it over one `Substrate` axis (cpu-tensor, device-wgpu, onnx, genai, remote-grpc) whose capability needs, browser exclusion, provider gates, cost ranks, payload caps, and load tie-breaks are row columns, and dispatches through generated total Switches — selection folds over row data, never an if-ladder, and every walk lands a `SelectionReceipt`. Each intent's eligible chain IS its degrade order (device->cpu->remote, onnx->remote, genai->remote), so a vetoed row degrades to the next without a parallel per-row fallback successor. This owner holds the intent vocabulary, the substrate axis, the `ComputeFault` family in the 2200 code band, and the dispatch spine.

Discipline lanes own their own typed entry folds — `Solver/contract` `Solve`, `Stats/estimator` `Fit`, `Symbolic/expression` `Compile`, `Analysis/assessment` `Assess` — never re-entering this boundary; they rejoin the package only at the one `ComputeReceipt` union, the 2200-band `ComputeFault`, and the `Runtime/scheduling` `LaneRuntime`. Dispatch composes Thinktecture vocabularies, LanguageExt rails, NodaTime instants, and the settled AppHost vocabulary — `Spec`, `WorkLane`, `DeadlineClass`, and `SubscriptionPolicy` among them, each declared at the spine and reached through this package's legal upward reference; `ComputeIntent` never crosses the other way, so the platform compiles INTO this rail and never names the union it targets.

## [01]-[INDEX]

- [02]-[INTENT_FAMILY]: `ComputeIntent` closes the intent roster over the adopted `Spec` record and one boundary admission fold.
- [03]-[SUBSTRATE_AXIS]: five substrate rows (incl. device-wgpu GPGPU); capability needs, browser exclusion, provider gates, ranks, caps, load as columns.
- [04]-[DISPATCH_SPINE]: fault band 2200, ordered selection fold, total dispatch, selection receipt.

## [02]-[INTENT_FAMILY]

- Owner: `ComputeIntent` `[Union]` cases; `AdmittedIntent` the evidence carrier whose private constructor makes `Admit` the only mint — the admission fold lives ON the carrier, so an unadmitted intent structurally cannot reach `Plan`, `Enqueue`, or `DispatchTable.Run`, which all take `AdmittedIntent`. `Spec` is NOT declared here: the request policy a capability descriptor answers at projection declares at `Rasm.AppHost` `Agent/capability#DESCRIPTOR_AXIS` and this fold adopts it WHOLE onto the carrier — one record, one seat, so the descriptor's declared posture and the value this admission gates on can never be two shapes that agree by convention.
- Cases: TensorOp | ModelInfer | RemoteCall | UnitProject | SymbolicProject | SensorAdmit | Pipeline | Generate; the adopted `Spec` carries deadline row, lane row, allocation row, cache-policy row, payload caps, forced-substrate `Option`, progress-subscription `Option`, and one inseparable `(Allotted, Provenance)` override.
- Entry: `public static Fin<AdmittedIntent> AdmittedIntent.Admit(ComputeIntent intent, Spec spec, CorrelationId correlation, CancelScope parent, IClock clock, TimeProvider time)` — `Fin<T>` aborts; admission runs exactly once at the boundary and interiors never re-validate; the byte and element caps are independent gates, so `Bounded` accumulates both violations through the `Validation` applicative pair before `ToFin` widens once — a first-fail cap gate that hides the second breach is the rejected form.
- Auto: the intent digest derives from the operation symbol and payload bytes and feeds every selection receipt; the admitted `CancelScope` child binds the allotted deadline so expiry rides the linked token.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, System.IO.Hashing, Rasm.AppHost (project), BCL inbox
- Growth: one intent case breaks every total Switch at compile time; a new shared policy value lands as one column on the spine's `Spec` and reaches every fold here untouched; zero new surface.
- Boundary: arity discriminates on the case payload shape — one value, a buffered span handle, or a stream handle — so name suffixes and mode flags never arise; payload spans admit at the edge into `ReadOnlyMemory<byte>` handles owned by the declared allocation row; `Budget` couples every deadline override to non-empty provenance and admission rejects non-positive durations; a pipeline shares one `Spec`, digest, deadline, scope, and correlation while `Projected` re-measures each child for substrate payload gates without minting new boundary evidence; the adopted `Spec` crosses DOWNWARD only — this owner reads its columns and never widens them, so a Compute-only policy axis is a column on a Compute shape rather than a field on the platform's request record; the intent's model field is the XxHash128 checksum, its rich identity record a model-lane concern; `Generate` carries that checksum, the prompt, and the model-lane `GenerationPolicy` (search options, guidance constraint, prompt-assembly inputs) so token streaming admits through the one fold like every intent — a separate `GenerateRequest` path or a chat-client surface never arises; `IClock` and `TimeProvider` cross from the app composition as neutral clock primitives because the App-owned `ClockPolicy` record never crosses downward into this APP-PLATFORM owner.

```csharp signature
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ComputeIntent {
    private ComputeIntent() { }

    public sealed record TensorOp(TensorOpFamily Family, ReadOnlyMemory<byte> Operands, ImmutableArray<nint> Shape) : ComputeIntent;

    public sealed record ModelInfer(UInt128 Model, ReadOnlyMemory<byte> Input, ImmutableArray<nint> Shape) : ComputeIntent;

    public sealed record RemoteCall(ComputeEndpoint Endpoint, string Method, ReadOnlyMemory<byte> Payload) : ComputeIntent;

    public sealed record UnitProject(QuantityFamily Family, double Value, string Unit, string TargetUnit) : ComputeIntent;

    // Symbolic quantity projection: a unit-bearing FORMULA (not a flat scalar) enters the same intent rail — the
    // expression, its per-symbol dimension declarations, the numeric bindings, and the target unit — dispatched
    // onto the Symbolic lane's dimension proof + compiled evaluation + unit projection chain.
    public sealed record SymbolicProject(SymbolicExpr Formula, Map<string, string> Dimensions, Map<string, double> Bindings, string TargetUnit) : ComputeIntent;

    // Broker-decoded sensor sample: the twin's telemetry crossing enters the ONE admission gate exactly as a
    // tensor op does, so its deadline budget, element cap, cancel scope, and correlation bind before the
    // CaptureIngest channel holds it and a DropOldest shed lands as Backpressure evidence carrying the
    // dropped sample's own correlation. `Runtime/transport#BROKER_INGEST` mints the case off the pump and
    // `Solver/clash#CLASH_AND_TWIN` `TwinLoop.Ingest` is the bound lane dispatch — a raw envelope pushed
    // onto a channel beside `AdmittedIntent` forks the lane's admission law and strands that evidence.
    public sealed record SensorAdmit(SensorEnvelope<TwinSignal> Envelope) : ComputeIntent;

    public sealed record Pipeline(Seq<ComputeIntent> Stages) : ComputeIntent;

    public sealed record Generate(UInt128 Model, string Prompt, GenerationPolicy Policy) : ComputeIntent;
}

public sealed record AdmittedIntent {
    private AdmittedIntent(
        ComputeIntent intent,
        Spec spec,
        AllocationClass allocation,
        CachePolicy cache,
        Option<Substrate> forced,
        UInt128 digest,
        long payloadBytes,
        Instant deadlineAt,
        CorrelationId correlation,
        CancelScope scope) {
        Intent = intent;
        Spec = spec;
        Allocation = allocation;
        Cache = cache;
        Forced = forced;
        Digest = digest;
        PayloadBytes = payloadBytes;
        DeadlineAt = deadlineAt;
        Correlation = correlation;
        Scope = scope;
    }

    public ComputeIntent Intent { get; }

    public Spec Spec { get; }

    // The three posture columns the spine could only declare as KEYS, RESOLVED: the spine owns the dispatch
    // posture and this package owns the allocation, cache, and substrate rosters, so the keys cross down and
    // admission is the one seat that can refuse a key no roster issued. Resolving here and carrying the rows
    // is what keeps every later reader — selection, allocation, the cache lane, the receipt projection — off
    // a string it would have to re-resolve, each with its own idea of what an unknown key means.
    public AllocationClass Allocation { get; }

    public CachePolicy Cache { get; }

    public Option<Substrate> Forced { get; }

    public UInt128 Digest { get; }

    public long PayloadBytes { get; }

    public Instant DeadlineAt { get; }

    public CorrelationId Correlation { get; }

    public CancelScope Scope { get; }

    public static Fin<AdmittedIntent> Admit(
        ComputeIntent intent,
        Spec spec,
        CorrelationId correlation,
        CancelScope parent,
        IClock clock,
        TimeProvider time) =>
        from measured in Measured(intent)
        from bytes in Bounded(measured, spec)
        from allotted in Budgeted(spec)
        from allocation in Keyed<AllocationClass>(nameof(Spec.Allocation), spec.Allocation)
        from cache in Keyed<CachePolicy>(nameof(Spec.Cache), spec.Cache)
        from forced in spec.Forced.Match(
            Some: static key => Substrate.Admit(key).Map(Some),
            None: static () => Fin.Succ(Option<Substrate>.None))
        select new AdmittedIntent(
            intent,
            spec,
            allocation,
            cache,
            forced,
            Derived(intent),
            bytes,
            clock.GetCurrentInstant() + allotted,
            correlation,
            parent.Derive(nameof(AdmittedIntent), time, Some(allotted)));

    // ONE key-decode rail for every posture vocabulary the spine crosses as text: the generated owners each
    // publish the static-abstract `IObjectFactory<T, string, ValidationError>.Validate`, so a fourth posture
    // axis is one more call rather than a second decode shape with its own idea of a bad key. The substrate
    // key keeps its own `Substrate.Admit` because an unadmitted substrate is a first-class compute fault the
    // selection receipt already names, while an unknown posture key is a spec-decode refusal.
    private static Fin<T> Keyed<T>(string axis, string key)
        where T : IObjectFactory<T, string, ValidationError> =>
        T.Validate(key, provider: null, out T? row) is null && row is { } admitted
            ? Fin.Succ(admitted)
            : Fin.Fail<T>(new ComputeFault.Text($"<spec-key-unrostered:{axis}:{key}>"));

    private static Fin<Duration> Budgeted(Spec spec) =>
        spec.Budget.Match(
            Some: static budget => budget.Allotted <= Duration.Zero || string.IsNullOrWhiteSpace(budget.Provenance)
                ? Fin.Fail<Duration>(new ComputeFault.Text($"<budget-invalid:{budget.Provenance}:{budget.Allotted}>"))
                : Fin.Succ(budget.Allotted),
            None: () => Fin.Succ(spec.Deadline.Allotted));

    // A projected stage inherits the RESOLVED rows rather than re-decoding the parent's keys: the parent's
    // admission already refused every key no roster issued, so a second decode can only agree or disagree.
    internal Fin<AdmittedIntent> Projected(ComputeIntent child) =>
        Measured(child).Map(measured => new AdmittedIntent(
            child, Spec, Allocation, Cache, Forced, Digest, measured.Bytes, DeadlineAt, Correlation, Scope));

    private static Fin<long> Bounded((long Bytes, long Elements) measured, Spec spec) =>
        (Gate(spec.ByteCap, measured.Bytes, "bytes"), Gate(spec.ElementCap, measured.Elements, "elements"))
            .Apply(static (bytes, _) => bytes).As().ToFin();

    private static K<Validation<Error>, long> Gate(Option<long> cap, long measured, string axis) =>
        cap.Match(
            Some: bound => bound < 0L
                ? Fin.Fail<long>(new ComputeFault.PayloadOverBounds($"<cap-negative:{axis}:{bound}>")).ToValidation()
                : measured > bound
                    ? Fin.Fail<long>(new ComputeFault.PayloadOverBounds($"<payload-over-cap:{axis}:{measured}:{bound}>")).ToValidation()
                    : Fin.Succ(measured).ToValidation(),
            None: () => Fin.Succ(measured).ToValidation());

    private static Fin<(long Bytes, long Elements)> Measured(ComputeIntent intent) =>
        intent.Switch(
            tensorOp: static op => Shaped(op.Operands.Length, op.Shape),
            modelInfer: static op => Shaped(op.Input.Length, op.Shape),
            remoteCall: static op => Fin.Succ(((long)op.Payload.Length, 0L)),
            unitProject: static _ => Fin.Succ((0L, 1L)),
            symbolicProject: static op => Fin.Succ((0L, (long)Math.Max(1, op.Bindings.Count))),
            generate: static op => Fin.Succ(((long)Encoding.UTF8.GetByteCount(op.Prompt), 0L)),
            // Broker decode consumed the body, so no byte figure survives to cap here; element count carries
            // sample width — operating-point dimensions beside one measurement — which is what an element cap
            // bounds when a mis-shaped publisher floods the capture lane.
            sensorAdmit: static op => Fin.Succ((0L, (long)op.Envelope.Data.OperatingPoint.Length + 1L)),
            pipeline: static line => line.Stages.IsEmpty
                ? Fin.Fail<(long, long)>(new ComputeFault.PayloadOverBounds("<pipeline-empty>"))
                : line.Stages.TraverseM(static child => Measured(child)).As().Bind(Summed));

    private static Fin<(long Bytes, long Elements)> Shaped(int bytes, ImmutableArray<nint> shape) =>
        Try.lift(() => (
            Bytes: (long)bytes,
            Elements: shape.Aggregate(1L, static (product, dimension) =>
                dimension > 0
                    ? checked(product * (long)dimension)
                    : throw new InvalidDataException($"non-positive:{dimension}")))).Run()
            .MapFail(static error => new ComputeFault.PayloadOverBounds($"<shape-rejected:{error.Message}>"));

    private static Fin<(long Bytes, long Elements)> Summed(Seq<(long Bytes, long Elements)> measured) =>
        Try.lift(() => measured.Fold(
            (Bytes: 0L, Elements: 0L),
            static (sum, next) => (checked(sum.Bytes + next.Bytes), checked(sum.Elements + next.Elements)))).Run()
            .MapFail(static error => new ComputeFault.PayloadOverBounds($"<pipeline-overflow:{error.Message}>"));

    private static UInt128 Derived(ComputeIntent intent) =>
        intent.Switch(
            tensorOp: static op => Seeded(op.Family.Key, op.Operands.Span),
            modelInfer: static op => Seeded(op.Model.ToString("x32", CultureInfo.InvariantCulture), op.Input.Span),
            remoteCall: static op => Seeded(op.Method, op.Payload.Span),
            unitProject: static op => Scalar(op),
            // Formula identity folds the canonical expression content key, the ordinal-sorted declarations and
            // bindings, and the target unit — two structurally identical projections share one digest.
            symbolicProject: static op => Seeded(
                $"{op.Formula.ContentKey:x32}|{string.Join(',', toSeq(op.Dimensions.OrderBy(static d => d.Key, StringComparer.Ordinal)).Map(static d => $"{d.Key}={d.Value}"))}>{op.TargetUnit}",
                MemoryMarshal.AsBytes<double>([.. toSeq(op.Bindings.OrderBy(static b => b.Key, StringComparer.Ordinal)).Map(static b => CanonicalForm.Scalar(b.Value))])),
            generate: static op => Seeded(op.Model.ToString("x32", CultureInfo.InvariantCulture), Encoding.UTF8.GetBytes(op.Prompt)),
            // Sensor identity is the sample's own content — signal row, instant, and the canonical
            // operating-point vector beside the measurement — so a broker replay carries the digest its first
            // delivery carried and the job graph's content-keyed cone re-scores nothing.
            sensorAdmit: static op => Seeded(
                $"{op.Envelope.Data.SignalId}@{op.Envelope.Data.At}",
                MemoryMarshal.AsBytes<double>([
                    .. op.Envelope.Data.OperatingPoint.Select(CanonicalForm.Scalar),
                    CanonicalForm.Scalar(op.Envelope.Data.Measured)])),
            pipeline: static line => Combined(line.Stages.Map(Derived)));

    private static UInt128 Seeded(string operation, ReadOnlySpan<byte> payload) =>
        XxHash128.HashToUInt128(payload, unchecked((long)XxHash3.HashToUInt64(Encoding.UTF8.GetBytes(operation))));

    private static UInt128 Scalar(ComputeIntent.UnitProject op) {
        Span<byte> payload = stackalloc byte[sizeof(double)];
        BinaryPrimitives.WriteDoubleLittleEndian(payload, CanonicalForm.Scalar(op.Value));
        return Seeded($"{op.Family.Key}|{op.Unit}>{op.TargetUnit}", payload);
    }

    private static UInt128 Combined(Seq<UInt128> digests) {
        byte[] payload = new byte[digests.Count * 16];
        ignore(digests.Fold(0, (offset, digest) => {
            BinaryPrimitives.WriteUInt128LittleEndian(payload.AsSpan(offset, 16), digest);
            return offset + 16;
        }));
        return XxHash128.HashToUInt128(payload);
    }
}
```

## [03]-[SUBSTRATE_AXIS]

- Owner: `Substrate` `[SmartEnum<string>]` rows under the `ComparerAccessors.StringOrdinal` accessor, each carrying the capability-need, browser-exclusion, provider-gate, rank, sheddable, and payload-cap columns its one derived `Veto` folds; `SelectionContext` resolved selection inputs; `BenchmarkRank` boot-frozen rank projection.
- Cases: cpu-tensor, device-wgpu (GPGPU compute-shader dispatch over the shared `ONE_WGPU_DEVICE`, ordered before `cpu-tensor` in the tensor-op eligible chain), onnx (one EP-parameterized row — EP variance is model-lane row data, never substrate-row twins), genai (token-streaming over the model-lane GenAI session), remote-grpc.
- Entry: `public Option<string> Veto(SelectionContext context)` — `Option<T>` carries the rejection reason, `None` admits; one derived body folds the browser-exclusion, capability-need, and provider-gate columns so the five rows share one veto and onnx/device/genai availability is the one `!Providers.Contains(Key)` shape, never five parallel delegates.
- Auto: `EffectiveRank` reads the boot-frozen `BenchmarkRank` projection, falling through to the static cost rank on a host-fingerprint mismatch; `SelectionContext.Providers` arrives boot-frozen from the host probe — the ORT probe contributes `onnx` when the runtime reports an execution provider, the device boot `device-wgpu`, the GenAI dylib probe `genai`; warm-start affinity reorders the eligible chain so a cold companion routes to the node holding the matching EP-context blob, one column picking host-vs-companion-vs-farm exactly as it picks cpu-vs-onnx, never an `if (warm)` branch; `LoadRank` is the third tie-break key (rank -> warm-affinity -> load), reading per-node load from the AppHost `PeerRoster` health so the least-loaded of rank-equal-and-warm nodes wins; `Forecast` is the duration-forecast column the composition root binds to the one query owner, `Runtime/receipts#BENCHMARK_CLAIMS` `HostClaims.Forecast(index, claims, row, admitted.PayloadBytes)` — band by `BenchmarkClaim.BandOf`, substrate by row key, fingerprint and recency closed inside `ModelResultIndex.Claim` — so `DeadlineVeto` answers "can this finish inside its allotment" before dispatch and an unmeetable local row degrades down the same chain every other veto rides.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, Microsoft.ML.OnnxRuntime, BCL inbox
- Growth: one substrate row — key, capability need, browser exclusion, provider gate, rank, payload cap, sheddable flag — absorbs a new execution substrate; `device-wgpu` is exactly that one row (ordered before `cpu-tensor` in the tensor-op chain, sheddable, provider-gated on its `Providers` key), so the device thrust spawns no parallel device-state machine and no second `SelectionReceipt` — admission, dispatch, and receipt read device-ness from the same `OrtResidency.DeviceResident` discriminant the CPU path uses; warm-start affinity and `LoadRank` are columns the fold already reads, so farm load-and-offload needs no `FarmRouter`; zero new surface.
- Boundary: wasm is a platform predicate column — `OperatingSystem.IsBrowser` excludes the onnx and device-wgpu rows while cpu-tensor and remote-grpc admit it, so a wasm substrate row never arises; the boot-frozen `Providers` set carries the available keys (`onnx` iff the ORT runtime reports an execution provider, `device-wgpu` iff the shared `ONE_WGPU_DEVICE` adapter resolves, `genai` iff the GenAI dylib loads), so those rows share the one `!Providers.Contains(Key)` gate and a differently-shaped set read never arises; each provider-gated row vetoes itself when its key is absent and a second health probe beside that gate is the named defect, so a device-unavailable tensor intent degrades to the CPU GEMM and a genai-unavailable token stream degrades remote — both through the same ordered `Chain` fold, the tensor chain ordering `device-wgpu` before `cpu-tensor` and the generate chain ordering `genai` before `remote-grpc` and never `cpu-tensor`, keeping the degrade total.
- Boundary: `SubstrateSelection` consumes the one per-`WorkLane` `ShedVerdict` the AppHost `LaneGuard` mints from the atomic `DegradationReading` (the `Runtime/admission ← csharp:Rasm.AppHost` `ONE_DEGRADATION_SHED_VERDICT` seam) — resolved once by the governor for the admitted `Spec.Lane` and carried on `SelectionContext.Shed` exactly as `DegradationLevel` rides `SelectionContext.Level`, so the seam couples to the `ShedVerdict(WorkLane, DegradationLevel, bool Shed, CircuitState Breaker)` shape and the interior reads `Shed`/`Lane`/`Level`, never the `DegradationCell` it derives from (governor interior stays AppHost-side); `Sheddable` marks the local-compute rows (cpu-tensor, device-wgpu), and `SelectionContext.ShedVeto` folds the lane-shed-AND-sheddable veto into the same `Routed` composition the `Veto`/`VetoPayload` rejections ride, carrying lane and level into the hop reason (`shed:{Lane}:{Level}`) as receipt evidence, so a shed lane degrades a sheddable device op to `remote-grpc` or, when no row admits, reuses `SubstrateUnavailable` with the full hop trail — a device-only backpressure path, a whole-op short-circuit that discards the chain evidence, a bare-`bool` projection that drops the lane/level facts, and a Compute-side re-derivation of the shed all reject, the verdict minted once at the governor and consumed here as a column, never an `if (shed)` ladder.
- Boundary: the same device descriptor gates the ONNX Runtime Mac execution-provider residency so a model-lane device tensor and a tensor-lane device kernel resolve one allocator on one physical device; substrate predicates read the retained `Capability` set so remote health rides the AppHost degradation fold — Rhino-absent folds to `DegradationLevel.LocalOnly` and the remote row vetoes through `Capability.RemoteCompute`; the remote payload cap composes `GrpcChannelPolicy.Canonical.MaxSendBytes`, never a re-declared literal; warm-start affinity reorders only within the rank-equal tier (a tie-breaker, never a rank override) and `LoadRank` breaks ties only beneath affinity.
- Boundary: the spine's `Spec` crosses its allocation, cache, and substrate posture as smart-enum KEYS because those three rosters are this package's, so `AdmittedIntent.Admit` is the one seat that decodes them — `Substrate.Admit` lifts the generated `TryGet` onto `Fin<Substrate>` for the forced selector and the one `Keyed<T>` rail lifts the static-abstract `IObjectFactory<T, string, ValidationError>.Validate` for the other two — and the resolved rows ride the admitted intent, so a reader taking `Spec.Allocation`/`Spec.Cache`/`Spec.Forced` as a typed value is the deleted form that re-decodes a key admission already refused.

```csharp signature
public sealed record BenchmarkRank(string HostFingerprint, HashMap<string, int> Ranks) {
    public Option<int> For(Substrate row, string fingerprint) =>
        string.Equals(HostFingerprint, fingerprint, StringComparison.Ordinal) ? Ranks.Find(row.Key) : None;
}

public sealed record SelectionContext(
    DegradationLevel Level,
    ShedVerdict Shed,
    FrozenSet<string> Providers,
    string Fingerprint,
    Option<BenchmarkRank> Ranks,
    FrozenSet<string> WarmAffinity,
    FrozenDictionary<string, double> Loads,
    Func<Substrate, AdmittedIntent, Option<Duration>> Forecast,
    IClock Clock) {
    public int EffectiveRank(Substrate row) => Ranks.Bind(ranks => ranks.For(row, Fingerprint)).IfNone(row.Rank);

    public int AffinityRank(Substrate row) => WarmAffinity.Contains(row.Key) ? 0 : 1;

    public double LoadRank(Substrate row) =>
        Loads.TryGetValue(row.Key, out double load) && double.IsFinite(load) && load >= 0d ? load : double.PositiveInfinity;

    public Option<string> ShedVeto(Substrate row) =>
        Shed.Shed && row.Sheddable ? Some($"shed:{Shed.Lane}:{Shed.Level.Key}") : None;

    public Option<string> DeadlineVeto(Substrate row, AdmittedIntent admitted) {
        Duration remaining = admitted.DeadlineAt - Clock.GetCurrentInstant();
        return remaining <= Duration.Zero
            ? Some($"deadline:expired:{remaining}")
            : Forecast(row, admitted).Filter(median => median > Duration.Zero && remaining < median)
                .Map(median => $"deadline:forecast:{median}:remaining:{remaining}");
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Substrate {
    public static readonly Substrate CpuTensor = new("cpu-tensor", needs: Capability.LocalCompute, browserExcluded: false, providerGated: false, rank: 0, payloadCapBytes: null, sheddable: true);
    public static readonly Substrate DeviceWgpu = new("device-wgpu", needs: Capability.LocalCompute, browserExcluded: true, providerGated: true, rank: 0, payloadCapBytes: null, sheddable: true);
    public static readonly Substrate Onnx = new("onnx", needs: Capability.LocalCompute, browserExcluded: true, providerGated: true, rank: 1, payloadCapBytes: null, sheddable: false);
    public static readonly Substrate GenAi = new("genai", needs: Capability.LocalCompute, browserExcluded: true, providerGated: true, rank: 1, payloadCapBytes: null, sheddable: false);
    public static readonly Substrate RemoteGrpc = new("remote-grpc", needs: Capability.RemoteCompute, browserExcluded: false, providerGated: false, rank: 2, payloadCapBytes: GrpcChannelPolicy.Canonical.MaxSendBytes, sheddable: false);

    private readonly long? payloadCapBytes;

    public Capability Needs { get; }

    public bool BrowserExcluded { get; }

    public bool ProviderGated { get; }

    public int Rank { get; }

    public bool Sheddable { get; }

    public Option<long> PayloadCap => Optional(payloadCapBytes);

    public static Fin<Substrate> Admit(string key) =>
        TryGet(key, out Substrate? row) && row is { } admitted
            ? Fin.Succ(admitted)
            : Fin.Fail<Substrate>(new ComputeFault.SubstrateUnavailable($"<substrate-unrostered:{key}>"));

    public Option<string> Veto(SelectionContext context) =>
        BrowserExcluded && OperatingSystem.IsBrowser() ? Some(nameof(OperatingSystem.IsBrowser))
        : !context.Level.Permits(Needs) ? Some(Needs.Key)
        : ProviderGated && !context.Providers.Contains(Key) ? Some(Key)
        : None;

    public Option<string> VetoPayload(long bytes) =>
        PayloadCap is { IsSome: true, Case: long cap } && bytes > cap ? Some($"{bytes}:{cap}") : None;
}
```

## [04]-[DISPATCH_SPINE]

- Owner: `ComputeFault` fault family on the doctrine `Expected` shape with the dual-tier `Create` contract in the 2200 code band beside LifecycleFault 1200 and HopFault 4500; `SelectionHop` and `SelectionReceipt` evidence records; `SubstrateSelection` ordered-predicate fold; `DispatchTable` total row dispatch.
- Cases: Text with the twelve domain cases SubstrateUnavailable | PayloadOverBounds | DeadlineExpired | Cancelled | ShutdownDrained | ModelRejected | ExtensionAssetMissing | EndpointUnreachable | RetryOwnerConflict | AllocationOverClass | EquivalenceMiss | CacheCorrupt — this owner declares the 2200..2212 core; discipline pages extend the SAME band as partial `ComputeFault` records on this owner, never a parallel fault union.
- Law: 2200..2212 core (here); 2213..2216 Symbolic lane (`Symbolic/expression` `SymbolicFault` ParseRejected/SymbolUndefined/NonDifferentiable 2213..2215 + `Symbolic/dimensional` DimensionMismatch 2216); 2217..2219 analysis lane (`Analysis/assessment` AssessmentInputMissing/ToolchainUnresolved/AnalysisFailed); 2220..2225 scheduling lane (`Runtime/scheduling` GraphCyclic/GraphRejected/GraphStalled/CheckpointRejected/LaneSaturated/LaneUnprofiled); next-free 2226. `Runtime/wire#FAULT_PROJECTION` mirrors every band row.
- Law: an arm declares in the fence of the lane whose fold RAISES it, never on this owner because this owner mints the family — `Expected` equality is by code alone, so an arm seated on a foreign lane's contiguous block makes one code answer two recoveries and the wire packs it under the wrong lane name.
- Law: the Remote `WireFault` 4520..4532 wire sub-band (`Runtime/wire#FAULT_PROJECTION`) is Compute's SECOND custody — distinct from this 2200 band and from the AppHost `HopFault` 4500 hop band — recorded here beside the primary map and pinned reciprocally in the sibling registries.
- Law: HDF5 archive refusals mint NO band arm — every `Runtime/codecs#HDF_ARCHIVE` fault rides an existing case (`ModelRejected` the open/dtype/order refusals, `PayloadOverBounds` the exchange reads and writes) wearing the `<hdf5-…:payload>` slug grammar, so the archive integration grew the band frontier by zero and a raw library message dressed as a Compute verdict stays the named defect.
- Foreign neighborhoods (PINNED mirror rows — a foreign band change is a row edit on both ends, never prose): AppHost 1xxx lifecycle + 4100..4810 wire/coordination (its `CoordinationFault` re-banded to 4540 around Compute's 4520..4532); AppUi 6xxx; Persistence 5xxx / 771x / 82xx..83xx; the AEC 23xx..27xx registry. A sibling registry reserves Compute's two custodies as DECADE-WHOLE spans — 2200..2299 and the 4520..4532 wire sub-band — so cross-package disjointness is checkable from both ends while the live sub-band frontier inside the decade stays this page's alone and an appended arm moves no sibling row.
- Entry: `public static Fin<Seq<SelectionReceipt>> Plan(AdmittedIntent admitted, SelectionContext context)` — `Fin<T>` aborts; the pipeline case folds its stages sequentially with short-circuit and the stage receipts share the parent correlation and digest.
- Auto: every selection walk materializes one `SelectionReceipt` — evaluated rows, rejection reasons, fallback hops, forced bypass, warm-affinity influence, final route — and the receipts page carries it to the sink as the Selection case of the package receipt union, so a farm hop proves itself on the same receipt rail every other hop rides; the composition root threads the `Runtime/receipts#HOOK_POINTS` `ComputeHookRail` around this spine — `Planned` runs the `rasm.compute.runtime.admit` veto fold over the `AdmittedIntent` before `Plan` so an app-composed policy gate transforms or refuses on the emitter's own rail, and `Ran` fires the `rasm.compute.runtime.dispatch` observe tap with the `SelectionReceipt` before `DispatchTable.Run` — domain code fires evidence, subscribers attach at composition, and a subscriber fault lands on the AppHost hook fault band, never on this spine.
- Receipt: `SelectionReceipt` — correlation, digest, route, hop evidence, forced `Option`, warm-affinity flag, `Instant` stamp.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox
- Growth: one fault case breaks every total Switch at compile time; one new substrate row costs one delegate field on `DispatchTable` and the generated row Switch breaks until it exists; zero new surface.
- Boundary: every fault case projects through the remote-lane FaultDetail wire family at the server edge, never a bare status-code-plus-string terminal; cancellation classifies in one conversion arm from `CancelScope` provenance and the deadline instant so user cancel, deadline expiry, and shutdown drain stay distinct, drain-derived scopes carrying `RuntimePhase.Draining.Key` as a provenance segment; a detected second retry owner raises RetryOwnerConflict toward the Conflict receipt — the AppHost keyed Polly hop owns retry, stacking never occurs here; forced substrate replaces the ordered preference chain but still rides every capability, shed, payload, and deadline veto, so policy cannot bypass safety; dispatch delegates bind at composition through `DispatchTable` because execution capsules carry runtime state no static row column owns; substrate ranking chooses the execution family only — `Runtime/transport#TRANSPORT_AXIS` owns endpoint selection inside `remote-grpc`, and substrate-keyed load or affinity never claims node-level farm routing.

```csharp signature
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ComputeFault : Expected, IValidationError<ComputeFault> {
    private ComputeFault(string detail, int code) : base(detail, code, None) { }

    public static ComputeFault Create(string message) => new Text(message);

    public static ComputeFault OfCancellation(CancelScope scope, Instant deadlineAt, Instant now) =>
        now >= deadlineAt ? new DeadlineExpired(scope.Provenance)
        : scope.Provenance.Contains(RuntimePhase.Draining.Key, StringComparison.Ordinal) ? new ShutdownDrained(scope.Provenance)
        : new Cancelled(scope.Provenance);

    public sealed record Text : ComputeFault { public Text(string detail) : base(detail, 2200) { } }
    public sealed record SubstrateUnavailable : ComputeFault { public SubstrateUnavailable(string detail) : base(detail, 2201) { } }
    public sealed record PayloadOverBounds : ComputeFault { public PayloadOverBounds(string detail) : base(detail, 2202) { } }
    public sealed record DeadlineExpired : ComputeFault { public DeadlineExpired(string provenance) : base(provenance, 2203) { } }
    public sealed record Cancelled : ComputeFault { public Cancelled(string provenance) : base(provenance, 2204) { } }
    public sealed record ShutdownDrained : ComputeFault { public ShutdownDrained(string provenance) : base(provenance, 2205) { } }
    public sealed record ModelRejected : ComputeFault { public ModelRejected(string detail) : base(detail, 2206) { } }
    public sealed record ExtensionAssetMissing : ComputeFault { public ExtensionAssetMissing(string detail) : base(detail, 2207) { } }
    public sealed record EndpointUnreachable : ComputeFault { public EndpointUnreachable(string detail) : base(detail, 2208) { } }
    public sealed record RetryOwnerConflict : ComputeFault { public RetryOwnerConflict(string detail) : base(detail, 2209) { } }
    public sealed record AllocationOverClass : ComputeFault { public AllocationOverClass(string detail) : base(detail, 2210) { } }
    public sealed record EquivalenceMiss : ComputeFault { public EquivalenceMiss(string detail) : base(detail, 2211) { } }
    public sealed record CacheCorrupt : ComputeFault { public CacheCorrupt(string detail) : base(detail, 2212) { } }
}

public readonly record struct SelectionHop(Substrate Row, Option<string> Rejection);

public sealed record SelectionReceipt(
    CorrelationId Correlation,
    UInt128 Digest,
    Substrate Route,
    Seq<SelectionHop> Hops,
    Option<Substrate> Forced,
    bool WarmAffinity,
    Instant At);

public static class SubstrateSelection {
    // Every chain is CONSTANT per case and three local folds share one, so each materializes once at type init
    // and generated `Map` projects a precomputed row — a per-arm lambda re-allocates its chain on every walk.
    static readonly Seq<Substrate> TensorChain = Seq(Substrate.DeviceWgpu, Substrate.CpuTensor, Substrate.RemoteGrpc);
    static readonly Seq<Substrate> ModelChain = Seq(Substrate.Onnx, Substrate.RemoteGrpc);
    static readonly Seq<Substrate> GenerateChain = Seq(Substrate.GenAi, Substrate.RemoteGrpc);
    static readonly Seq<Substrate> RemoteChain = Seq(Substrate.RemoteGrpc);
    static readonly Seq<Substrate> LocalChain = Seq(Substrate.CpuTensor);
    static readonly Seq<Substrate> NoChain = Seq<Substrate>();

    // Intent-specific eligibility owns fallback membership; row policy owns ordering within that closed set. A
    // decoded sensor sample folds on the twin loop's own host, so it rides the local chain beside the unit and
    // symbolic projections — shipping one measurement to a farm costs more than the fold it asks for.
    public static Seq<Substrate> Eligible(ComputeIntent intent) =>
        intent.Map(
            tensorOp: TensorChain,
            modelInfer: ModelChain,
            remoteCall: RemoteChain,
            unitProject: LocalChain,
            symbolicProject: LocalChain,
            generate: GenerateChain,
            sensorAdmit: LocalChain,
            pipeline: NoChain);

    public static Fin<Seq<SelectionReceipt>> Plan(AdmittedIntent admitted, SelectionContext context) =>
        admitted.Intent is ComputeIntent.Pipeline line
            ? line.Stages.TraverseM(stage => admitted.Projected(stage).Bind(projected => Plan(projected, context))).As()
                .Map(static nested => nested.Fold(Seq<SelectionReceipt>(), static (acc, stage) => acc + stage))
            : Select(admitted, context).Map(static receipt => Seq(receipt));

    public static Fin<SelectionReceipt> Select(AdmittedIntent admitted, SelectionContext context) =>
        Routed(
            admitted,
            context,
            admitted.Forced.Map(static forced => Seq(forced)).IfNone(() => Chain(Eligible(admitted.Intent), context)),
            admitted.Forced);

    private static Seq<Substrate> Chain(Seq<Substrate> eligible, SelectionContext context) =>
        toSeq(eligible.OrderBy(context.EffectiveRank).ThenBy(context.AffinityRank).ThenBy(context.LoadRank));

    private static Fin<SelectionReceipt> Routed(
        AdmittedIntent admitted,
        SelectionContext context,
        Seq<Substrate> chain,
        Option<Substrate> forced) =>
        Receipted(admitted, context, forced, chain.Fold(
            (Route: Option<Substrate>.None, Hops: Seq<SelectionHop>()),
            (acc, row) => acc.Route.IsSome ? acc
                : (row.Veto(context) | context.ShedVeto(row) | row.VetoPayload(admitted.PayloadBytes) | context.DeadlineVeto(row, admitted)) is { IsSome: true, Case: string reason }
                    ? (acc.Route, acc.Hops.Add(new SelectionHop(row, Some(reason))))
                    : (Some(row), acc.Hops.Add(new SelectionHop(row, None)))));

    private static Fin<SelectionReceipt> Receipted(
        AdmittedIntent admitted,
        SelectionContext context,
        Option<Substrate> forced,
        (Option<Substrate> Route, Seq<SelectionHop> Hops) walked) =>
        walked.Route
            .ToFin(new ComputeFault.SubstrateUnavailable($"<substrate-chain-exhausted:{string.Join(',', walked.Hops.Map(static hop => hop.Row.Key))}>"))
            .Map(route => new SelectionReceipt(
                admitted.Correlation,
                admitted.Digest,
                route,
                walked.Hops,
                forced,
                context.AffinityRank(route) == 0,
                context.Clock.GetCurrentInstant()));
}

public sealed record DispatchTable(
    Func<AdmittedIntent, IO<Unit>> CpuTensor,
    Func<AdmittedIntent, IO<Unit>> DeviceWgpu,
    Func<AdmittedIntent, IO<Unit>> Onnx,
    Func<AdmittedIntent, IO<Unit>> GenAi,
    Func<AdmittedIntent, IO<Unit>> RemoteGrpc) {
    public IO<Unit> Run(SelectionReceipt selection, AdmittedIntent admitted) =>
        selection.Route.Switch(
            state: (Table: this, Work: admitted),
            cpuTensor: static s => s.Table.CpuTensor(s.Work),
            deviceWgpu: static s => s.Table.DeviceWgpu(s.Work),
            onnx: static s => s.Table.Onnx(s.Work),
            genAi: static s => s.Table.GenAi(s.Work),
            remoteGrpc: static s => s.Table.RemoteGrpc(s.Work));
}
```

```mermaid
---
config:
  layout: elk
  flowchart:
    curve: linear
    padding: 25
---
flowchart LR
    accTitle: Compute intent admission and substrate selection
    accDescr: Admitted intents plan through the hook rail into a substrate selection, and every bound refusal rails a compute fault.
    ComputeIntent -- Admit --> AdmittedIntent
    AdmittedIntent -- PayloadOverBounds --> ComputeFault
    AdmittedIntent -- Planned --> ComputeHookRail
    ComputeHookRail -- Plan --> SubstrateSelection
    Substrate -- Veto --> SubstrateSelection
    SubstrateSelection -- SubstrateUnavailable --> ComputeFault
    SubstrateSelection -- Select --> SelectionReceipt
    SelectionReceipt -- Ran --> ComputeHookRail
    SelectionReceipt -- Run --> DispatchTable
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
