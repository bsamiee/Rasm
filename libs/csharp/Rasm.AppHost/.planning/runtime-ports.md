# [APPHOST_RUNTIME_PORTS]

Rasm.AppHost exposes exactly seven typed port records as its only cross-package seam; siblings adapt to them and the dependency never reverses. The owned axes are the port-record family with the HLC receipt envelope, the boot-minted `TenantContext` tenancy primitive stamped on the envelope, the suite JSON wire law with its contract-merge rule, and the TS tooling map. Drain bands, deadline rows, phase vocabulary, the classification taxonomy, and the degradation vocabulary arrive settled from the package spine and compose here as port payloads.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]     | [OWNS]                                                                           |
| :-----: | :------------ | :------------------------------------------------------------------------------- |
|   [1]   | PORT_RECORDS  | Seven inward port records; HLC envelope + TenantContext cross-process primitives |
|   [2]   | WIRE_LAW      | One Strict context per package; app roots merge resolvers, emit schemas          |
|   [3]   | TS_PROJECTION | Tooling map and the envelope wire contract the TS dashboard consumes             |

## [2]-[PORT_RECORDS]

- Owner: `ReceiptEnvelope`, `ReceiptSinkPort`, `TelemetryContributorPort`, `DrainParticipantPort`, `HostAttachPort`, `UiSchedulerPort`, `SupportContributorPort`, `HealthContributorPort` — sealed records of delegates and policy values; zero interfaces, zero inheritance contracts, zero provider-branded vocabulary; `TenantContext` the boot-minted tenancy primitive stamped on the envelope beside the correlation id, owned here and consumed by every sibling as settled vocabulary.
- Cases: `TenantContext.Root` is the single-tenant ambient default (`TenantId` zero, slug `root`); a multi-tenant host mints one row per admitted tenant at boot from its tenant-feed configuration.
- Entry: `public IO<ReceiptEnvelope> Send(CorrelationId correlation, TenantContext tenant, string package, string kind, JsonElement payload)` — `IO` carries the sink effect; the returned envelope is the emission evidence carrying both cross-process primitives; `TenantContext.Ambient` is the `RuntimeContextSlot<TenantContext>` the boot mint stamps and deferred work restores.
- Receipt: `ReceiptEnvelope` carries the two cross-process causal primitives — the HLC stamp orders evidence and the `Tenant` field partitions it; receipts, support bundles, and degradation stay process-local and correlate across processes solely through the HLC stamp; `SkewBound` derives at stamp time as the wall-clock lag the advance observed, so the evidence view surfaces skew with zero configuration.
- Packages: LanguageExt.Core, NodaTime, Thinktecture.Runtime.Extensions, BCL inbox
- Growth: a new cross-package seam lands as one registration row on an existing port, zero new surface; an eighth port record is the named defect; a new tenant is one ambient `TenantContext` value minted at boot, never a second tenancy owner.
- Boundary: every registration enters as a `TryAddEnumerable` ordered `ServiceDescriptor` row and subscriptions return disposable detachers composed LIFO; the contributor ports carry the settled row vocabularies — instrument rows, artifact rows, probe rows — never re-spelled fields; no sibling assembly enters the AppHost graph, and an aggregate store, compute, companion, or outbound-client port is the rejected form — that content decomposes into rows on these seven; `TenantContext` is the 4th cross-package primitive peer to the HLC stamp, the content-address `Hash`, and the boot-minted `CorrelationId` — AppHost mints and threads it, Persistence/server-tier#TENANCY_RLS consumes `TenantId` as the `current_setting('rasm.tenant')::uuid` RLS predicate and never re-mints it, Persistence/cache-indexes#L2_CONTRIBUTION partitions the content-address cache key by `TenantId`, and `TenantSlot` (`rasm.tenant`) is the single GUC and meter-tag spelling every consumer reads; `TenantContext.Tag` rides the `*`-wildcard capped view at diagnostics-and-telemetry#SIGNAL_GOVERNANCE `Views` so the per-tenant meter dimension caps at the governed series ceiling and never fans unbounded; `TenantId` crosses the wire as a `UInt128`-keyed Thinktecture value object so the RLS uuid cast and the cache-key partition read one identity, never a string parse beside it.

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

    public TenantContext Stamp() {
        var prior = Current;
        Ambient.Set(this);
        ignore(Baggage.SetBaggage(TenantSlot, TenantId.Value.ToString()));
        return prior;
    }

    public KeyValuePair<string, object?> Tag => new(TenantSlot, TenantId.Value.ToString());
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

public sealed record TelemetryContributorPort(
    TelemetrySource Source,
    string Version,
    Seq<InstrumentRow> Instruments);

public sealed record DrainParticipantPort(
    string Name,
    DrainBand Band,
    int Rank,
    Func<CancellationToken, IO<Unit>> Drain);

public sealed record HostAttachPort(
    Func<RuntimePhase, Fin<PhaseReceipt>> Inject,
    Func<Option<string>> HostDocument,
    Func<Action, IDisposable> DocumentChanged);

public sealed record UiSchedulerPort(
    Func<Action<PhaseReceipt>, IDisposable> Phases,
    Func<Action<DegradationLevel>, IDisposable> Degradation,
    Func<Action, IO<Unit>> Marshal);

public sealed record SupportContributorPort(
    string Package,
    Seq<SupportArtifact> Rows);

public sealed record HealthContributorPort(
    string Package,
    Seq<HealthContributorRow> Rows);
```

## [3]-[WIRE_LAW]

- Owner: `AppHostWireContext`, `SuiteContracts` — the package wire context and the app-root merge surface; `NodaPatterns` the pattern-derived text codec table.
- Entry: `public static JsonSerializerOptions Wire(params ReadOnlySpan<JsonSerializerContext> contexts)` — one merge per app root; every JSON wire surface reads and writes through the merged options value, which seals on first use.
- Packages: NodaTime.Serialization.SystemTextJson, NodaTime, Thinktecture.Runtime.Extensions.Json, BCL inbox
- Growth: a new wire record lands as one `[JsonSerializable]` row on its package context plus one `[JsonDerivedType]` row per polymorphic leaf, zero new surface; a new semantic-time edge is one `NodaPatterns` row; an owner that must decline the factory carries its own `[JsonConverter]` attribute that `skipObjectsWithJsonConverterAttribute` honors, never a second factory; CORS and grpc-web middleware land as one app-root row each when a cross-origin deployment exists.
- Boundary: converter precedence is settled — `ConfigureForNodaTime` runs last in the `Wire` expression, after `TypeInfoResolver` binds the combined source-gen metadata, so the NodaTime per-type converters for `Instant`, `OffsetDateTime`, `ZonedDateTime`, and `Interval` resolve ahead of any source-gen `JsonTypeInfo` for those types — converter resolution precedes resolver metadata in System.Text.Json, and the call order in `Wire` is the precedence law, never a hand-assembled converter list; `OffsetDateTimePattern.Rfc3339` carries the exported offset stamp and `ZonedDateTimePattern` binds `WithZoneProvider`/`WithResolver` against the Tzdb provider with the strict resolver, so an ambiguous or skipped local time is a typed parse failure, never a silent shift; generated Thinktecture converters own every value-object, smart-enum, and keyed-union wire form, and a hand-written converter beside them is the named defect; the one options-level `ThinktectureJsonConverterFactory(skipObjectsWithJsonConverterAttribute: true)` covers any generated owner that carries no `[JsonConverter]` attribute while honoring the attribute on those that do, so attribute wiring and options registration never double-bind one owner; PipeReader deserialization is the consumer-edge inbound decode shape, never a staging axis.

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
[JsonSerializable(typeof(DiscoveryManifest))]
[JsonSerializable(typeof(ReceiptEnvelope))]
[JsonSerializable(typeof(TenantContext))]
[JsonSerializable(typeof(RosterReceipt))]
[JsonSerializable(typeof(FleetRollReceipt))]
[JsonSerializable(typeof(CommandReceipt))]
[JsonSerializable(typeof(DescriptorReceipt))]
[JsonSerializable(typeof(SandboxReceipt))]
[JsonSerializable(typeof(WriteReceipt))]
[JsonSerializable(typeof(AlertReceipt))]
[JsonSerializable(typeof(LogEntry))]
public partial class AppHostWireContext : JsonSerializerContext;

public static class SuiteContracts
{
    public static JsonSerializerOptions Wire(params ReadOnlySpan<JsonSerializerContext> contexts) =>
        new JsonSerializerOptions(JsonSerializerOptions.Strict)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
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
|   [1]   | runtime records      | STJ Strict source-gen JSON       | package contexts | dashboard and upload |
|   [2]   | discovery manifest   | STJ Strict atomic JSON           | app-root boot    | attaching peer       |
|   [3]   | service verbs        | protobuf over gRPC               | app roots        | connect-es clients   |
|   [4]   | wire fault unions    | `google.rpc.Status` details      | wire projection  | TS fault projection  |
|   [5]   | snapshot blobs       | MessagePack                      | snapshot rows    | @msgpack/msgpack     |
|   [6]   | telemetry signals    | OTLP                             | exporters        | OTLP collector       |
|   [7]   | contract schemas     | JsonSchemaExporter               | schema emission  | schema-derived TS    |
|   [8]   | semantic-time fields | `NodaPatterns` + Noda converters | `Wire` options   | ISO/RFC-3339 strings |

## [4]-[TS_PROJECTION]

- Owner: `RasmPackage`, `HlcStampWire`, `TenantContextWire`, `ReceiptEnvelopeWire` — the suite-level TS contract; per-record wire shapes ride their owning wire surfaces and bind here as `TPayload`.
- Entry: `ReceiptEnvelopeWire<TPayload>` binds at the codec edge where `SuiteContracts.Wire` emits the runtime record and `SuiteContracts.Schema` derives its TS type; every wire payload reconstructs through this one envelope, never a hand-mirrored interface.
- Packages: BCL inbox
- Growth: a new wire payload lands as one payload row bound through `ReceiptEnvelopeWire`, zero new surface; the tooling map gains one tool row per new wire codec.
- Boundary: `logical` resets to zero on every physical advance, so the counter never approaches the JSON number precision envelope; `physical` and `skewBound` cross as NodaTime ISO-8601 and roundtrip-pattern strings; Thinktecture keyed owners cross as their key scalars while polymorphic leafs cross with the kind literals their polymorphic metadata pins, reconstructed in TS as literal-discriminated unions; `tenantId` crosses as the `UInt128` decimal-string the `TenantId` value object emits so the TS dashboard partitions evidence by the same tenancy identity the RLS predicate reads, never a re-minted client tenant key.

```ts contract
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
|   [1]   | connect-es        | service descriptors | pnpm bootstrap  | hand-written clients   |
|   [2]   | @msgpack/msgpack  | snapshot blobs      | snapshot import | second TS binary codec |
|   [3]   | OTLP ingestion    | telemetry signals   | OTLP endpoint   | bespoke telemetry wire |
|   [4]   | schema-derived TS | JSON schemas        | TS build input  | mirrored interfaces    |
