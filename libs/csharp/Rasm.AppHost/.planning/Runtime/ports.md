# [APPHOST_RUNTIME_PORTS]

Rasm.AppHost exposes exactly seven typed port records as its only INWARD cross-package seam; siblings adapt to them and no sibling assembly enters the AppHost graph. `[WIRE_VOCABULARY]` rows carry settled OUTWARD vocabularies at the boundary without reversing an interior dependency. Owned axes are the port-record family with the HLC receipt envelope, the HLC two-half fan-in law, the boot-minted `TenantContext`, the suite JSON wire law, and the TS tooling map. Drain bands, deadline rows, phase vocabulary, classification, and degradation arrive settled as port payloads.

## [01]-[INDEX]

- [01]-[PORT_RECORDS]: Seven inward port records; `HLC` envelope and `TenantContext` cross-process primitives.
- [02]-[HLC_FANIN]: Two-half stamp law — one mint, fixed half order, cross-runtime parity.
- [03]-[WIRE_LAW]: One `Strict` context per package; app roots merge resolvers and emit schemas.
- [04]-[TS_PROJECTION]: Tooling map and the envelope wire contract the TS dashboard consumes.

## [02]-[PORT_RECORDS]

- Owner: `ReceiptSinkPort`, `TelemetryContributorPort`, `DrainParticipantPort`, `HostAttachPort`, `UiSchedulerPort`, `SupportContributorPort`, `HealthContributorPort` — the seven sealed records of delegates and policy values (`TelemetryContributorPort` declares at the kernel signal capsule so every stratum mints one; the other six declare here); zero interfaces, zero inheritance contracts, zero provider-branded vocabulary. `ReceiptEnvelope` is the receipt value the sink port emits, not a port; `TenantContext` and `TenantId` are the tenancy primitives stamped on that value, owned here and consumed by every sibling as settled vocabulary, never ports.
- Cases: the capability axis is `PortCardinality` — five DRIVEN ports the host calls outward into the package interior (`ReceiptSinkPort`, `TelemetryContributorPort`, `DrainParticipantPort`, `SupportContributorPort`, `HealthContributorPort`) and two DRIVING host-affine ports the host implements at the boundary (`HostAttachPort` injects phase transitions and surfaces the document, `UiSchedulerPort` marshals onto the host UI loop and carries the `ProfileSamples` feed row delivering published `ProfileSample` values to the AppUi devloop flame fold); `ReceiptSinkPort` is the identity port whose HLC two-half stamp is the sole cross-process correlation, with `TenantContext` partitioning each stamped value. `TenantContext.Root` is the single-tenant ambient default (`TenantId` zero, slug `root`); a multi-tenant host mints one row per admitted tenant at boot from its tenant-feed configuration.
- Entry: `public IO<ReceiptEnvelope> Send(CorrelationId correlation, TenantContext tenant, string package, string kind, JsonElement payload)` — `IO` carries the sink effect; the returned envelope is the emission evidence carrying both cross-process primitives; `TenantContext.Stamp()` returns the restoring scope over `Ambient` and `Baggage.Current`, so deferred work brackets both ambient stores as one tenancy value.
- Receipt: `ReceiptEnvelope` carries the one causal frame — the HLC two-half stamp orders evidence and the `Tenant` field partitions it, threaded together so every receipt and every content key composes the identical (tenant, physical, logical) frame; the HLC stamp is the canonical `(Instant Physical, ulong Logical)` pair in fixed half order — physical half first as the NodaTime `Instant` Unix-tick count, logical half second as the monotone `ulong` counter, both little-endian on the wire, byte-identical to `Compute/Runtime/codecs#CONTENT_ADDRESSING` `InterchangeIdentity.Compose` so a content key and a causal stamp seal one frame the peers re-derive from the same two-half order; `ReceiptSinkPort.Advance` is the single mint — every stamp draws from the one `Atom<(Instant Physical, ulong Logical)>` cell so no second HLC source exists, the logical half resets to zero on a physical advance and increments on a same-instant repeat, and `SkewBound` derives at stamp time as the wall-clock lag the advance observed, so the evidence view surfaces skew with zero configuration; receipts, support bundles, and degradation stay process-local and correlate across processes solely through the HLC stamp.
- Packages: LanguageExt.Core, NodaTime, Thinktecture.Runtime.Extensions, BCL inbox
- Growth: a new cross-package seam lands as one registration row on an existing port, zero new surface; a new tenant is one ambient `TenantContext` value minted at boot, never a second tenancy owner.
- Boundary: `PortCardinality` is the conserved invariant — exactly seven ports, and an eighth port record is the named defect: a new aggregate store, compute, companion, or outbound-client port is the rejected form, that content decomposing into rows on these seven, and a content carrier (the `ReceiptEnvelope` value, a `TenantContext` tenancy primitive, a `SecretLease` row, a `FencingToken` value object) is never promoted to a port. Spine owners constructor-injected as settled vocabulary are never ports: `ClockPolicy` (the clock pair), `SchedulePort` (a static fold over `ScheduleEntry` rows despite the `-Port` suffix), `CancelScope` (the cancellation provenance tree), the determinism RNG, the `HopPolicy` outbound rows, and the `CacheLane` L2 cache surface — each is a record or static surface threaded through composition, not a delegate-bearing inward seam. Every registration enters as a `TryAddEnumerable` ordered `ServiceDescriptor` row and subscriptions return disposable detachers composed LIFO; the contributor ports carry the settled row vocabularies — instrument rows, artifact rows, probe rows — never re-spelled fields, and `TelemetryContributorPort.SchemaUrl` is the semconv schema coordinate `TelemetryIdentity.Mint` stamps as `MeterOptions.TelemetrySchemaUrl` at every contributor mint; no sibling assembly enters the AppHost graph. `TenantContext` is the 4th cross-package primitive peer to the HLC stamp, the content-address `Hash`, and the boot-minted `CorrelationId`, and it is the canonical tenant carrier riding the one causal frame — `TenantContext.Stamp` threads the tenant onto `TenantContext.Ambient` and the receipt sink threads the HLC two-half pair, so the stamped `ReceiptEnvelope` carries tenant and HLC together as the one frame every receipt and every `Compute/Runtime/codecs#CONTENT_ADDRESSING` content key composes; AppHost mints and threads it, the Persistence tenancy owner `Element/identity` consumes `TenantId` as the `current_setting('rasm.tenant')::uuid` RLS predicate and never re-mints it, `Query/cache#L2_CONTRIBUTION` partitions the content-address cache key by `TenantId`, and `TenantSlot` (`rasm.tenant`) is the single GUC and meter-tag spelling every consumer reads; `TenantContext.Tag` rides the `*`-wildcard capped view at Observability/telemetry#SIGNAL_GOVERNANCE `Views` so the per-tenant meter dimension caps at the governed series ceiling and never fans unbounded; `TenantId` crosses the wire as a `UInt128`-keyed Thinktecture value object so the RLS uuid cast and the cache-key partition read one identity, never a string parse beside it.

```csharp signature
[ValueObject<UInt128>(
    KeyMemberName = "Value",
    KeyMemberAccessModifier = AccessModifier.Public,
    ConversionToKeyMemberType = ConversionOperatorsGeneration.Implicit,
    ConversionFromKeyMemberType = ConversionOperatorsGeneration.None)]
public readonly partial struct TenantId;

public sealed record TenantContext(TenantId TenantId, string Slug)
{
    public const string TenantSlot = "rasm.tenant";

    public static readonly TenantContext Root = new(TenantId.Create(UInt128.Zero), "root");

    public static readonly RuntimeContextSlot<TenantContext> Ambient =
        RuntimeContext.RegisterSlot<TenantContext>(nameof(TenantContext));

    public static TenantContext Current => Ambient.Get() ?? Root;

    // Baggage is immutable; tenancy scopes the runtime slot and baggage as one restoring pair.
    public IDisposable Stamp() {
        var priorTenant = Ambient.Get();
        var priorBaggage = Baggage.Current;
        Ambient.Set(this);
        Baggage.Current = priorBaggage.SetBaggage(TenantSlot, TenantId.Value.ToString());
        return new TenantScope(priorTenant, priorBaggage);
    }

    public KeyValuePair<string, object?> Tag => new(TenantSlot, TenantId.Value.ToString());

    private sealed class TenantScope(TenantContext? priorTenant, Baggage priorBaggage) : IDisposable {
        private int disposed;

        public void Dispose() {
            if (Interlocked.Exchange(ref disposed, 1) != 0) return;
            try {
                Ambient.Set(priorTenant);
            } finally {
                Baggage.Current = priorBaggage;
            }
        }
    }
}

public sealed record ReceiptEnvelope(
    CorrelationId Correlation,
    TenantContext Tenant,
    string Package,
    string Kind,
    JsonElement Payload,
    Instant Physical,
    ulong Logical,
    Duration SkewBound);

public sealed record ReceiptSinkPort(
    IClock Clock,
    Atom<(Instant Physical, ulong Logical)> Hlc,
    Func<ReceiptEnvelope, IO<Unit>> Emit)
{
    public static (Instant Physical, ulong Logical) Advance(
        (Instant Physical, ulong Logical) last, Instant wall) =>
        wall > last.Physical ? (wall, 0UL) : (last.Physical, last.Logical + 1UL);

    public IO<ReceiptEnvelope> Send(CorrelationId correlation, TenantContext tenant, string package, string kind, JsonElement payload) =>
        IO.lift(() => Clock.GetCurrentInstant())
            .Map(wall => (Wall: wall, Cell: Hlc.Swap(last => Advance(last, wall))))
            .Map(state => new ReceiptEnvelope(
                correlation, tenant, package, kind, payload,
                state.Cell.Physical, state.Cell.Logical, state.Cell.Physical - state.Wall))
            .Bind(envelope => Emit(envelope).Map(_ => envelope));
}

// TelemetryContributorPort is the kernel signal capsule's record — string-scoped so every stratum mints
// one legally; it holds its seat in the seven-port cardinality with its declaration homed at L1.

public sealed record DrainParticipantPort(
    string Name,
    DrainBand Band,
    int Rank,
    Func<CancellationToken, IO<Unit>> Drain);

public sealed record HostAttachPort(
    Func<RuntimePhase, Fin<PhaseReceipt>> Inject,
    Func<Option<string>> HostDocument,
    Func<Action, IDisposable> DocumentChanged);

// ProfileSamples is the profile-sample feed registration row. AppUi subscribes here; the
// TraceEvent-backed producer stays gated by Observability/benchmarks#PROFILE_SAMPLE_CAPTURE.
public sealed record UiSchedulerPort(
    Func<Action<PhaseReceipt>, IDisposable> Phases,
    Func<Action<DegradationLevel>, IDisposable> Degradation,
    Func<Action<ProfileSample>, IDisposable> ProfileSamples,
    Func<Action, IO<Unit>> Marshal);

public sealed record SupportContributorPort(
    string Package,
    Seq<SupportArtifact> Rows);

public sealed record HealthContributorPort(
    string Package,
    Seq<HealthContributorRow> Rows);
```

## [03]-[HLC_FANIN]

- Owner: the HLC two-half fan-in law — every process-local stamp folds into one causal order through the single `ReceiptSinkPort.Advance` mint; no second HLC source exists anywhere in the federation.
- Entry: `Advance((Instant Physical, ulong Logical) last, Instant wall)` — the one advance: the logical half resets to zero on a physical advance and increments on a same-instant repeat; `SkewBound` derives at stamp time as the wall-clock lag the advance observed.
- Packages: NodaTime, LanguageExt.Core, BCL inbox
- Growth: a new stamped surface draws from the same `Atom<(Instant Physical, ulong Logical)>` cell; zero new surface.
- Boundary: the half order is FIXED and load-bearing — physical half first as the NodaTime `Instant` Unix-tick `long`, logical half second as the monotone `ulong`, both little-endian on the wire, byte-identical to `Compute/Runtime/codecs#CONTENT_ADDRESSING` `InterchangeIdentity.Compose` so a content key and a causal stamp seal one frame every peer re-derives from the same two-half order; an off-by-one-half pack corrupts the whole causal order, so the python `hlc-two-half` reproduction fixture and the kernel reconciliation corpus pin the pack/unpack parity against this owner; the packed projection is `physical_ticks<<64 | logical` as one `UInt128`, the layout the python `Hlc.packed` peer holds bit-identical; the fence lives at `[PORT_RECORDS]` — this owner states the law, the port states the mechanics.

## [04]-[WIRE_LAW]

- Owner: `AppHostWireContext`, `SuiteContracts` — the package wire context and the app-root merge surface; `NodaPatterns` the pattern-derived text codec table.
- Entry: `public static JsonSerializerOptions Wire(params ReadOnlySpan<JsonSerializerContext> contexts)` — one merge per app root; every JSON wire surface reads and writes through the merged options value, which seals on first use.
- Packages: NodaTime.Serialization.SystemTextJson, NodaTime, Thinktecture.Runtime.Extensions.Json, BCL inbox
- Growth: a new wire record lands as one `[JsonSerializable]` row on its package context and one `[JsonDerivedType]` row per polymorphic leaf, zero new surface; a new semantic-time edge is one `NodaPatterns` row; an owner that must decline the factory carries its own `[JsonConverter]` attribute that `skipObjectsWithJsonConverterAttribute` honors, never a second factory; CORS and grpc-web middleware land as one app-root row each when a cross-origin deployment exists.
- Boundary: converter precedence is settled — `ConfigureForNodaTime` runs last in the `Wire` expression, after `TypeInfoResolver` binds the combined source-gen metadata, so the NodaTime per-type converters for `Instant`, `OffsetDateTime`, `ZonedDateTime`, and `Interval` resolve ahead of any source-gen `JsonTypeInfo` for those types — converter resolution precedes resolver metadata in System.Text.Json, and the call order in `Wire` is the precedence law, never a hand-assembled converter list; `OffsetDateTimePattern.Rfc3339` carries the exported offset stamp and `ZonedDateTimePattern` binds `WithZoneProvider`/`WithResolver` against the Tzdb provider with the strict resolver, so an ambiguous or skipped local time is a typed parse failure, never a silent shift; the `System.Text.Json.Schema.JsonSchemaExporter.GetJsonSchemaAsNode(options, type, exporterOptions)` member and `JsonSchemaExporterOptions.TreatNullObliviousAsNonNullable` that `Schema` binds are catalogued at the substrate tier; generated Thinktecture converters own every value-object, smart-enum, and keyed-union wire form, and a hand-written converter beside them is the named defect; the registry above IS the audit surface — every `AppHostWireContext.Default.X` dereference anywhere in the corpus resolves against a `[JsonSerializable]` row here (`assay`-gated; a dereference without a row is a phantom); `Wire/livewire#TS_PROJECTION`'s `LiveWireContext` FOLDS into this one merge as a context argument (`SuiteContracts.Wire(AppHostWireContext.Default, LiveWireContext.Default)` at the app root) — the standalone `LiveWireOptions.Json` is the deleted form, and livewire's one declared serializer divergence (`DefaultIgnoreCondition.WhenWritingNull`) rides THE MERGE ROW: the merged options carry `WhenWritingNull` as a declared suite-wide emission posture, so optional wire slots omit rather than null-fill and no private context survives the divergence; the one options-level `ThinktectureJsonConverterFactory(skipObjectsWithJsonConverterAttribute: true)` covers any generated owner that carries no `[JsonConverter]` attribute while honoring the attribute on those that do, so attribute wiring and options registration never double-bind one owner; PipeReader deserialization is the consumer-edge inbound decode shape, never a staging axis.

```csharp signature
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
    RespectNullableAnnotations = true,
    RespectRequiredConstructorParameters = true)]
[JsonSerializable(typeof(PhaseReceipt))]
[JsonSerializable(typeof(BootMarker))]
[JsonSerializable(typeof(FaultRecord))]
[JsonSerializable(typeof(DrainReceipt))]
[JsonSerializable(typeof(HealthSnapshot))]
[JsonSerializable(typeof(SupportManifest))]
[JsonSerializable(typeof(SupportReceipt))]
[JsonSerializable(typeof(DumpTriage))]
[JsonSerializable(typeof(DiscoveryManifest))]
[JsonSerializable(typeof(ReceiptEnvelope))]
[JsonSerializable(typeof(TenantContext))]
[JsonSerializable(typeof(RosterReceipt))]
[JsonSerializable(typeof(FleetRollReceipt))]
[JsonSerializable(typeof(RollAnnotationWire))]
[JsonSerializable(typeof(HopReceiptWire))]
[JsonSerializable(typeof(DeliveryReceiptWire))]
[JsonSerializable(typeof(OutboxSweepReceipt))]
[JsonSerializable(typeof(CommandReceipt))]
[JsonSerializable(typeof(DescriptorReceipt))]
[JsonSerializable(typeof(SandboxReceipt))]
[JsonSerializable(typeof(UpdateReceipt))]
[JsonSerializable(typeof(SupplyChainReceipt))]
[JsonSerializable(typeof(AlertReceipt))]
[JsonSerializable(typeof(BenchmarkReceipt))]
[JsonSerializable(typeof(LogEntry))]
[JsonSerializable(typeof(SecretReceipt))]
[JsonSerializable(typeof(CredentialPemWire))]
public partial class AppHostWireContext : JsonSerializerContext;

public static class SuiteContracts
{
    public static JsonSerializerOptions Wire(params ReadOnlySpan<JsonSerializerContext> contexts) =>
        new JsonSerializerOptions(JsonSerializerOptions.Strict)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            TypeInfoResolver = JsonTypeInfoResolver.Combine([.. contexts]),
            Converters = { new ThinktectureJsonConverterFactory(skipObjectsWithJsonConverterAttribute: true) },
        }.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);

    public static JsonNode Schema(JsonSerializerOptions wire, Type record) =>
        JsonSchemaExporter.GetJsonSchemaAsNode(wire, record, new JsonSchemaExporterOptions
        {
            TreatNullObliviousAsNonNullable = true,
        });
}

public static class NodaPatterns
{
    public static readonly IPattern<Instant> Instant = InstantPattern.ExtendedIso;

    public static readonly IPattern<OffsetDateTime> Offset = OffsetDateTimePattern.Rfc3339;

    public static IPattern<ZonedDateTime> Zoned(string format) =>
        ZonedDateTimePattern.CreateWithInvariantCulture(format, DateTimeZoneProviders.Tzdb)
            .WithZoneProvider(DateTimeZoneProviders.Tzdb)
            .WithResolver(Resolvers.StrictResolver);
}
```

Codec residence is fixed per wire surface; producer and consumer cells name endpoints only, not alternate codecs.

| [INDEX] | [WIRE_SURFACE]       | [CODEC]                          | [PRODUCER]       | [CONSUMER]           |
| :-----: | :------------------- | :------------------------------- | :--------------- | :------------------- |
|  [01]   | runtime records      | STJ Strict source-gen JSON       | package contexts | dashboard and upload |
|  [02]   | discovery manifest   | STJ Strict atomic JSON           | app-root boot    | attaching peer       |
|  [03]   | service verbs        | protobuf over gRPC               | app roots        | connect-es clients   |
|  [04]   | wire fault unions    | `google.rpc.Status` details      | wire projection  | TS fault projection  |
|  [05]   | snapshot blobs       | MessagePack                      | snapshot rows    | @msgpack/msgpack     |
|  [06]   | telemetry signals    | OTLP                             | exporters        | OTLP collector       |
|  [07]   | contract schemas     | JsonSchemaExporter               | schema emission  | schema-derived TS    |
|  [08]   | semantic-time fields | `NodaPatterns` + Noda converters | `Wire` options   | ISO/RFC-3339 strings |

## [05]-[TS_PROJECTION]

- Owner: `RasmPackage`, `HlcStampWire`, `TenantContextWire`, `ReceiptEnvelopeWire` — the suite-level TS contract; per-record wire shapes ride their owning wire surfaces and bind here as `TPayload`.
- Entry: `ReceiptEnvelopeWire<TPayload>` binds at the codec edge where `SuiteContracts.Wire` emits the runtime record and `SuiteContracts.Schema` derives its TS type; every wire payload reconstructs through this one envelope, never a hand-mirrored interface.
- Packages: BCL inbox
- Growth: a new wire payload lands as one payload row bound through `ReceiptEnvelopeWire`, zero new surface; the tooling map gains one tool row per new wire codec.
- Boundary: `logical` resets to zero on every physical advance, so the counter never approaches the JSON number precision envelope; `physical` and `skewBound` cross as NodaTime ISO-8601 and roundtrip-pattern strings for the dashboard read, while the content-key seal and the cross-runtime parity fixture compose the physical half as the `Instant.ToUnixTimeTicks` `long` in the fixed (physical, logical) little-endian order `Compute/Runtime/codecs#CONTENT_ADDRESSING` `InterchangeIdentity.Compose` pins — the ISO string is the human projection and the tick-count `long` is the hash-canonical half, so a peer that seals the frame hashes the tick count, never the ISO text; Thinktecture keyed owners cross as their key scalars while polymorphic leafs cross with the kind literals their polymorphic metadata pins, reconstructed in TS as literal-discriminated unions; `tenantId` crosses as the `UInt128` decimal-string the `TenantId` value object emits so the TS dashboard partitions evidence by the same tenancy identity the RLS predicate reads, never a re-minted client tenant key.

```ts signature
type RasmPackage = "Rasm.AppHost" | "Rasm.Persistence" | "Rasm.Compute" | "Rasm.AppUi";

interface HlcStampWire {
  physical: string;
  logical: number;
  skewBound: string;
}

interface TenantContextWire {
  tenantId: string;
  slug: string;
}

interface ReceiptEnvelopeWire<TPayload> extends HlcStampWire {
  correlation: string;
  tenant: TenantContextWire;
  package: RasmPackage;
  kind: string;
  payload: TPayload;
}
```

Each tool row names the consumed surface, activation point, and spelling it deletes.

| [INDEX] | [TOOL]            | [CONSUMES]          | [ACTIVATION]    | [DELETES]              |
| :-----: | :---------------- | :------------------ | :-------------- | :--------------------- |
|  [01]   | connect-es        | service descriptors | pnpm bootstrap  | hand-written clients   |
|  [02]   | @msgpack/msgpack  | snapshot blobs      | snapshot import | second TS binary codec |
|  [03]   | OTLP ingestion    | telemetry signals   | OTLP endpoint   | bespoke telemetry wire |
|  [04]   | schema-derived TS | JSON schemas        | TS build input  | mirrored interfaces    |

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
