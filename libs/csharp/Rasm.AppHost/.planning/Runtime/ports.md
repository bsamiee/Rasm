# [APPHOST_RUNTIME_PORTS]

Rasm.AppHost exposes exactly seven typed port records as its only INWARD cross-package seam; siblings adapt to them and no sibling assembly enters the AppHost graph. Outward vocabularies cross the same boundary as settled row payloads, never reversing an interior dependency. Owned axes are the port-record family under its cardinality invariant, the boot tenancy mint, the suite JSON wire law, and the TS tooling map. Drain bands, deadline rows, phase vocabulary, classification, and degradation arrive settled as port payloads.

Settled composition: `CorrelationId`, `TenantId`/`TenantContext`, `TenantMirror`, `ReceiptEnvelope`, and `ReceiptSinkPort` with its one HLC mint arrive from the kernel signal capsule `Rasm/Domain/telemetry#CAUSAL_FRAME`, and `TelemetryContributorPort` from `Rasm/Domain/telemetry#INSTRUMENT_MECHANISM`. This page seats those two kernel-declared records inside the cardinality invariant, mints the boot tenancy value from its tenant-feed configuration, and projects the stamped message envelope onto the suite wire; `Observability/telemetry#SIGNAL_GOVERNANCE` registers the OTel `Baggage.Current` store as the composition `TenantMirror` row, so a kernel caller spells `Stamp()` bare and threads no mirror per call site.

## [01]-[INDEX]

- [02]-[PORT_RECORDS]: Seven inward port records, five declared here and two at the kernel capsule.
- [03]-[WIRE_LAW]: One `Strict` context per package; app roots merge resolvers and emit schemas.
- [04]-[TS_PROJECTION]: Tooling map and the message-envelope wire contract the TS dashboard consumes.

## [02]-[PORT_RECORDS]

- Owner: `ReceiptSinkPort`, `TelemetryContributorPort`, `DrainParticipantPort`, `HostAttachPort`, `UiSchedulerPort`, `SupportContributorPort`, `HealthContributorPort` — the seven sealed records of delegates and policy values, five declared here and two at the kernel signal capsule so every stratum mints one without an upward reference; zero interfaces, zero inheritance contracts, zero provider-branded vocabulary. `ReceiptEnvelope` is the receipt value the sink port emits, not a port; `TenantContext` and `TenantId` are the kernel tenancy primitives stamped on that value, minted here at boot and consumed by every sibling as settled vocabulary, never ports.
- Cases: the capability axis is `PortCardinality` — five DRIVEN ports the host calls outward into the package interior (`ReceiptSinkPort`, `TelemetryContributorPort`, `DrainParticipantPort`, `SupportContributorPort`, `HealthContributorPort`) and two DRIVING host-affine ports the host implements at the boundary (`HostAttachPort` injects phase transitions and surfaces the document, `UiSchedulerPort` marshals onto the host UI loop and carries the `ProfileSamples` feed row delivering published `ProfileSample` values to the AppUi devloop flame fold); `ReceiptSinkPort` is the identity port whose HLC two-half stamp is the sole cross-process correlation, with `TenantContext` partitioning each stamped value.
- Entry: every registration enters as a `TryAddEnumerable` ordered `ServiceDescriptor` row admitted through `PortCardinality.Of(port)` — the `Runtime/modules#SCAN_AND_DECORATE` `Contributed` leg is that admission's one execution site, folding each contributor descriptor's service-type name through `Of` before it joins the ordered set — and every subscribing port returns disposable detachers composed LIFO, so a port is registered, never resolved by lookup; `TenantContext.Root` is the single-tenant ambient default (`TenantId` zero, slug `root`) and a multi-tenant host mints one row per admitted tenant at boot from its tenant-feed configuration.
- Receipt: `ReceiptEnvelope` carries the one causal frame — the kernel HLC two-half stamp orders evidence and the `Tenant` field partitions it, so every receipt and every content key composes the identical `(tenant, physical, logical)` frame; receipts, support bundles, and degradation stay process-local and correlate across processes solely through that stamp.
- Packages: Rasm, LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox
- Growth: a new cross-package seam lands as one registration row on an existing port, zero new surface; a new tenant is one ambient `TenantContext` value minted at boot, never a second tenancy owner.
- Boundary: `PortCardinality` is the conserved invariant — its two direction rows hold every admitted port name and an eighth port record is the named defect: a new aggregate store, compute, companion, or outbound-client port is the rejected form, that content decomposing into rows on these seven, and a content carrier (the `ReceiptEnvelope` value, a `TenantContext` tenancy primitive, a `SecretLease` row, a `FencingToken` value object) is never promoted to a port. Spine owners constructor-injected as settled vocabulary are never ports: `ClockPolicy` (the clock pair), `SchedulePort` (a static fold over `ScheduleEntry` rows despite the `-Port` suffix), `CancelScope` (the cancellation provenance tree), the determinism RNG, the `HopPolicy` outbound rows, and the `CacheLane` L2 cache surface — each is a record or static surface threaded through composition, not a delegate-bearing inward seam. Contributor ports carry the settled row vocabularies — instrument rows, artifact rows, probe rows — never re-spelled fields, and `TelemetryContributorPort.SchemaUrl` is the semconv schema coordinate `TelemetryIdentity.Mint` stamps as `MeterOptions.TelemetrySchemaUrl` at every contributor mint; no sibling assembly enters the AppHost graph. `TenantContext` is a cross-package primitive beside the HLC stamp, the content-address `Hash`, and the boot-minted `CorrelationId`, and this platform is its one minting site: AppHost mints and threads it, the Persistence tenancy owner `Element/identity` stores the canonical `TenantId.Text` render in a `text` column and compares it against `current_setting('rasm.tenant')` bare — a `::uuid` provider cast is the deleted form that forks one identity into two alphabets — `Query/cache#L2_CONTRIBUTION` partitions the content-address cache key by `TenantId`, and `TenantSlot` (`rasm.tenant`) is the single GUC and meter-tag spelling every consumer reads; `TenantContext.Tags` rides the one per-instrument view projection at Observability/telemetry#SIGNAL_GOVERNANCE `Views`, which admits `TenantSlot` beside each row's declared dimensions under the governed series ceiling so the per-tenant meter dimension survives the tag projection and never fans unbounded, and the root row contributes no dimension at all so an absent `rasm.tenant` reads single-tenant everywhere; `TenantId` crosses the wire as a `UInt128`-keyed Thinktecture value object whose one `Text` render the RLS predicate, the cache-key partition, and the meter tag all compare byte-identically, never a string parse beside it.

```csharp signature
// ReceiptSinkPort and TelemetryContributorPort declare at the kernel signal capsule — both are string-scoped
// so an emitter outside this platform's reach mints one legally, and both hold their seats in the seven-port
// cardinality with their declarations homed at S0. The five records below are this page's own.

// Conserved seven ride as DATA: each row carries the port names its direction governs, so the cardinality
// invariant is a set membership a registration fold proves rather than a count a reader keeps by hand.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PortCardinality {
    public static readonly PortCardinality Driven = new("driven", Names(
        nameof(ReceiptSinkPort), nameof(TelemetryContributorPort), nameof(DrainParticipantPort),
        nameof(SupportContributorPort), nameof(HealthContributorPort)));
    public static readonly PortCardinality Driving = new("driving", Names(
        nameof(HostAttachPort), nameof(UiSchedulerPort)));

    public FrozenSet<string> Ports { get; }

    // Eighth port record reaches composition as a service type naming no row, refused at the module fold's
    // contributor leg while the collection is still editable rather than discovered later as a leaked inward
    // dependency. The refusal accumulates there, so one boot names every foreign service type at once.
    public static Fin<PortCardinality> Of(string port) =>
        toSeq(Items).Find(row => row.Ports.Contains(port))
            .ToFin(new Fault.InvalidValue(Label: port, Requirement: "a declared inward port row"));

    static FrozenSet<string> Names(params ReadOnlySpan<string> ports) =>
        ports.ToArray().ToFrozenSet(StringComparer.Ordinal);
}

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
// TraceEvent-backed producer stays gated by Observability/benchmarks#PROFILE_CORRELATION.
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

## [03]-[WIRE_LAW]

- Owner: `AppHostWireContext`, `SuiteContracts` — the package wire context, the app-root merge surface, and the composition-bound `Host` producer context the merge seats; `NodaPatterns` the pattern-derived text codec table; the `OmitAbsent` contract modifier that makes absence omit. The carrier-space codec for the LanguageExt collections the suite wires cross is COMPOSED, never declared here — `Rasm/Domain/rails#CARRIER_CODEC` owns `LanguageExtJsonConverterFactory` at the kernel so the S2 private mints reach the same one type this merge registers.
- Entry: `public static JsonSerializerOptions Wire(params ReadOnlySpan<JsonSerializerContext> contexts)` — one merge per app root; every JSON wire surface reads and writes through the merged options value, which seals on first use.
- Packages: NodaTime.Serialization.SystemTextJson, NodaTime, Thinktecture.Runtime.Extensions.Json, LanguageExt.Core, BCL inbox
- Law: this roster IS the `apphost-wire` seam's producer half — the runtime-evidence family set crosses as one registration, so a family gains its `[JsonSerializable]` row here and its census row at the consuming branch in the same motion, and carriage is JSON by this page's own wire law, never a protobuf arm no descriptor source exists for. A family a sibling seam entry already owns registers at that entry and carries its row here only where this producer serializes it: the HLC two-half stamp's byte layout is the `hlc-two-half` seam's shape and rides `ReceiptEnvelope` as a field rather than a row of its own, `HostFingerprintWire` is the `host-fingerprint` seam's shape and rides here because this producer mints it, and the benchmark claim's minter is `Rasm.Compute`, so a row spelling it here mis-names the producer.
- Growth: a new wire record lands as one `[JsonSerializable]` row on its package context and one `[JsonDerivedType]` row per polymorphic leaf, zero new surface; a new semantic-time edge is one `NodaPatterns` row; an owner that must decline the factory carries its own `[JsonConverter]` attribute that `skipObjectsWithJsonConverterAttribute` honors, never a second factory; a new LanguageExt carrier is one `Carriers` row at the kernel owner (`Rasm/Domain/rails#CARRIER_CODEC`), never a per-member `[JsonConverter]` attribute or a mint-local converter; a new contract-level emission posture is one `OmitAbsent`-class modifier on the resolver chain, never a second options identity; CORS and grpc-web middleware land as one app-root row each when a cross-origin deployment exists.
- Boundary: converter precedence is settled — `ConfigureForNodaTime` runs last in the `Wire` expression, after `TypeInfoResolver` binds the combined source-gen metadata, so the NodaTime per-type converters for `Instant`, `OffsetDateTime`, `ZonedDateTime`, and `Interval` resolve ahead of any source-gen `JsonTypeInfo` for those types — converter resolution precedes resolver metadata in System.Text.Json, and the call order in `Wire` is the precedence law, never a hand-assembled converter list; `OffsetDateTimePattern.Rfc3339` carries the exported offset stamp and `ZonedDateTimePattern` binds `WithZoneProvider`/`WithResolver` against the Tzdb provider with the strict resolver, so an ambiguous or skipped local time is a typed parse failure, never a silent shift; the `System.Text.Json.Schema.JsonSchemaExporter.GetJsonSchemaAsNode(options, type, exporterOptions)` member and `JsonSchemaExporterOptions.TreatNullObliviousAsNonNullable` that `Schema` binds are catalogued at the substrate tier; `Wire` freezes the merged options with `MakeReadOnly()` before returning, so the suite is one immutable wire identity, `IsReadOnly` is the composition's audit bit, and a root appending a converter after the mint throws at the append rather than forking the wire; generated Thinktecture converters own every value-object, smart-enum, and keyed-union wire form, and a hand-written converter beside them is the named defect; the registry above IS the audit surface — every producer wire crossing anywhere in the corpus resolves against a `[JsonSerializable]` row on a merged context (`assay`-gated; a crossing without a row is a phantom) — while the WRITE itself rides the composition-bound `SuiteContracts.Host` options handle, never a context: a `JsonTypeInfo` off `AppHostWireContext.Default` carries the context's own options so neither `OmitAbsent` nor the registered factories engage (a `None` serializes its own struct members), and a context INSTANCE constructed over the merge is the equally-refuted twin — the ctor rebinds the options' resolver to the context itself and silently drops the modifier — so `.Default` survives only as the merge's resolver arguments and for type-init metadata roster reads (`PolymorphismOptions` inspection), the same carve `Rasm.AppUi/Diagnostics/evidence` rules for its own context; `Wire/livewire#TS_PROJECTION`'s `LiveWireContext` FOLDS into this one merge as a context argument, as do the kernel `Rasm/Drawing/pack#SCHEMA_AND_EVIDENCE` `PackWireContext` and the `Rasm.AppUi` `AppUiWireContext` (`SuiteContracts.Wire(AppHostWireContext.Default, LiveWireContext.Default, PackWireContext.Default, AppUiWireContext.Default)` at the app root — every package context whose shapes cross the suite wire is one merge argument, which is exactly what makes the merged chain able to resolve them) — the standalone `LiveWireOptions.Json` is the deleted form, and livewire's one declared serializer divergence (`DefaultIgnoreCondition.WhenWritingNull`) rides THE MERGE ROW: the merged options carry `WhenWritingNull` as a declared suite-wide emission posture, so optional wire slots omit rather than null-fill and no private context survives the divergence; the one options-level `ThinktectureJsonConverterFactory(skipObjectsWithJsonConverterAttribute: true)` covers any generated owner that carries no `[JsonConverter]` attribute while honoring the attribute on those that do, so attribute wiring and options registration never double-bind one owner; converter ORDER within the list decides nothing here because the three registered spaces are disjoint by construction — the Thinktecture factory admits by generated conversion metadata, `LanguageExtJsonConverterFactory` by open-generic carrier row, and the NodaTime registration by closed semantic-time type — and both factories seat inside the initializer, so factories precede the per-type converters `ConfigureForNodaTime` appends and every registration lands before `MakeReadOnly()`; the kernel `Rasm/Domain/rails#CARRIER_CODEC` factory is the ONE carrier-space owner and this mint REGISTERS it, so a per-member `[JsonConverter(typeof(SeqJsonConverter<…>))]` on a wire record and a second factory declaration at any mint are the deleted forms — the freeze that once forced member-level binding is downstream of this initializer, and the kernel home is what lets the S2 private mints (`ElementJson`, `FabricationWireContext`) register the same type their `{Rasm, Rasm.Element}` reference set can reach; every carrier converter routes its elements back through the same options, so nesting composes and a carrier holding generated owners or semantic-time values needs no second registration — a `HashMap<string, Set<string>>` resolves the map converter and its value carrier off one registration; absence OMITS rather than null-fills and the mechanism is the resolver's own `OmitAbsent` modifier, never the converter list — `WhenWritingNull` reaches reference and `Nullable<T>` slots alone, `Option<T>` is a struct that condition never sees, and a converter cannot drop the member it is called for, so the write predicate is a contract decision that rides `WithAddedModifier` and reads presence off the non-generic `IOptional` the boxed value already carries; the paired decode law is that this predicate makes an `Option<T>` constructor parameter carrying no default wire-required under `RespectRequiredConstructorParameters`, so every such parameter carries `= default` for the omitted-property read while every construction still answers it explicitly; PipeReader deserialization is the consumer-edge inbound decode shape, never a staging axis.

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
[JsonSerializable(typeof(DegradationState))]
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
[JsonSerializable(typeof(CommandArguments))]
[JsonSerializable(typeof(CommandReceipt))]
[JsonSerializable(typeof(DescriptorPin))]
[JsonSerializable(typeof(DescriptorReceipt))]
[JsonSerializable(typeof(DiscoveryResult))]
[JsonSerializable(typeof(SandboxReceipt))]
[JsonSerializable(typeof(UpdateReceipt))]
[JsonSerializable(typeof(SupplyChainReceipt))]
[JsonSerializable(typeof(AlertReceipt))]
[JsonSerializable(typeof(BenchmarkReceipt))]
[JsonSerializable(typeof(LogEntry))]
[JsonSerializable(typeof(SecretReceipt))]
// The runtime-evidence wire families the `apphost-wire` seam registers as ONE producer roster: each family is
// one row here and one census row at the consumer, and a family a sibling seam entry already owns registers
// THERE — `HlcStampWire`'s byte layout is the two-half seam's shape (this roster carries it as an envelope
// field), `HostFingerprintWire` the host-fingerprint seam's, and the benchmark claim's minter is Compute, so
// no row here spells it.
[JsonSerializable(typeof(CredentialPemWire))]
[JsonSerializable(typeof(CommandAvailabilityWire))]
[JsonSerializable(typeof(FlagVerdictWire))]
[JsonSerializable(typeof(SupportCaptureWire))]
[JsonSerializable(typeof(HostFingerprintWire))]
[JsonSerializable(typeof(ModalityReceipt))]
[JsonSerializable(typeof(ToolAudit))]
[JsonSerializable(typeof(MembershipReceipt))]
[JsonSerializable(typeof(LeadershipReceipt))]
[JsonSerializable(typeof(LockReceipt))]
public partial class AppHostWireContext : JsonSerializerContext;

public static class SuiteContracts
{
    // Composition-bound producer surface: `Wire` seats it as the ONE merged options identity, so every
    // producer write (`JsonSerializer.SerializeToElement(receipt, SuiteContracts.Host)`) resolves through
    // the combined resolver chain — the registered factories AND the `OmitAbsent` modifier. Two refuted
    // forms, both probe-witnessed: a `JsonTypeInfo` off `AppHostWireContext.Default` carries the context's
    // own options (no factory, no modifier — a `None` serializes its own struct members), and a context
    // INSTANCE constructed over the merge rebinds the options' resolver to the context itself, silently
    // dropping the modifier — so `.Default` survives only as a merge argument and for type-init metadata
    // roster reads, and no context instance ever fronts the merge.
    public static JsonSerializerOptions Host {
        get => field ?? throw new InvalidOperationException("SuiteContracts.Wire seats Host at the app-root mint.");
        private set;
    }

    // Absence OMITS. `DefaultIgnoreCondition.WhenWritingNull` reaches reference and `Nullable<T>` slots alone
    // and `Option<T>` is a struct that is never null, so the ignore condition never sees one and no converter
    // can drop its own member — omission is a CONTRACT decision, which is why it rides the resolver rather
    // than the converter list. `IOptional` is the non-generic presence read every closed `Option<A>` carries,
    // so the boxed value the predicate receives answers without a per-write generic probe. Paired law: the
    // predicate makes an `Option<T>` constructor parameter carrying no default WIRE-REQUIRED under
    // `RespectRequiredConstructorParameters`, so `= default` on such a parameter is the decode half of
    // omission and never a slot a construction may leave unanswered.
    static void OmitAbsent(JsonTypeInfo contract) {
        foreach (JsonPropertyInfo property in contract.Properties) {
            if (property.PropertyType.IsGenericType
                && property.PropertyType.GetGenericTypeDefinition() == typeof(Option<>)) {
                property.ShouldSerialize = static (_, value) => value is IOptional { IsSome: true };
            }
        }
    }

    public static JsonSerializerOptions Wire(params ReadOnlySpan<JsonSerializerContext> contexts) {
        JsonSerializerOptions wire = new JsonSerializerOptions(JsonSerializerOptions.Strict)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            TypeInfoResolver = JsonTypeInfoResolver.Combine([.. contexts]).WithAddedModifier(OmitAbsent),
            // Both factories seat BEFORE the freeze and their type spaces are disjoint — the Thinktecture
            // factory admits by generated conversion metadata, the kernel carrier factory
            // (`Rasm/Domain/rails#CARRIER_CODEC`) by open-generic carrier row — so neither shadows the
            // other and their relative order decides nothing.
            Converters = {
                new ThinktectureJsonConverterFactory(skipObjectsWithJsonConverterAttribute: true),
                new LanguageExtJsonConverterFactory(),
            },
        }.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
        wire.MakeReadOnly();   // freeze at the mint; a post-mint converter or resolver edit throws instead of forking the suite
        Host = wire;   // the one producer surface; every write resolves through the merged chain
        return wire;
    }

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

## [04]-[TS_PROJECTION]

- Owner: `RasmPackage`, `HlcStampWire`, `TenantContextWire`, `ReceiptEnvelopeWire` — the suite-level TS contract; per-record wire shapes ride their owning wire surfaces and bind here as `TPayload`.
- Entry: `ReceiptEnvelopeWire<TPayload>` binds at the codec edge where `SuiteContracts.Wire` emits the runtime record and `SuiteContracts.Schema` derives its TS type; every wire payload reconstructs through this one message envelope, never a hand-mirrored interface.
- Packages: BCL inbox
- Growth: a new wire payload lands as one payload row bound through `ReceiptEnvelopeWire`, zero new surface; the tooling map gains one tool row per new wire codec.
- Boundary: the suite contract is the `apphost-wire` seam's TS half and the stamp interface is the `hlc-two-half` seam's — `HlcStampWire` is that seam's shape and this projection is one of its three co-equal minters, so the layout is shared while carriage stays per-branch and a decoder reads the halves in the declared order or corrupts a fresh op as stale; every other family projects at the page that mints it and binds here only as `TPayload`, so a per-record interface beside this message envelope is the deleted form. `RasmPackage` mirrors the kernel `TelemetrySource` key roster one-to-one at the decode seam, so a package minted there reaches the dashboard without a second admission and a package absent there cannot spell a message envelope; `logical` resets to zero on every physical advance, so the counter never approaches the JSON number precision limit; `physical` and `skewBound` cross as NodaTime ISO-8601 and roundtrip-pattern strings for the dashboard read, while the content-key seal and the cross-runtime parity fixture compose the physical half as the `Instant.ToUnixTimeTicks` `long` in the fixed (physical, logical) little-endian order the kernel capsule `Rasm/Domain/telemetry#CAUSAL_FRAME` declares and `Rasm.Compute/Runtime/codecs#CONTENT_ADDRESSING` `InterchangeIdentity.Compose` seals — the ISO string is the human projection, the tick-count `long` is the hash-canonical half, and the packed form is `physical_ticks<<64 | logical` as one `UInt128`, the layout the python `Hlc.packed` peer holds bit-identical — so a peer that seals the frame hashes the tick count, never the ISO text; Thinktecture keyed owners cross as their key scalars while polymorphic leafs cross with the kind literals their polymorphic metadata pins, reconstructed in TS as literal-discriminated unions; `tenantId` crosses as the `UInt128` decimal-string the `TenantId` value object emits so the TS dashboard partitions evidence by the same tenancy identity the RLS predicate reads, never a re-minted client tenant key.

```ts signature
type RasmPackage =
  | "rasm.kernel" | "Rasm.Element" | "Rasm.AppHost" | "Rasm.Materials"
  | "Rasm.Bim" | "Rasm.Fabrication" | "Rasm.Persistence" | "Rasm.Compute"
  | "Rasm.Generation" | "Rasm.AppUi" | "Rasm.Rhino" | "Rasm.Grasshopper";

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

## [05]-[RESEARCH]

(none)
