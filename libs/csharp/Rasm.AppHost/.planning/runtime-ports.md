# [APPHOST_RUNTIME_PORTS]

Rasm.AppHost exposes exactly seven typed port records as its only cross-package seam; siblings adapt to them and the dependency never reverses. The owned axes are the port-record family with the HLC receipt envelope, the suite JSON wire law with its contract-merge rule, and the TS tooling map. Drain bands, deadline rows, phase vocabulary, the classification taxonomy, and the degradation vocabulary arrive settled from the package spine and compose here as port payloads.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]     | [OWNS]                                                                         |
| :-----: | :------------ | :----------------------------------------------------------------------------- |
|   [1]   | PORT_RECORDS  | Seven inward port records; HLC envelope as sole cross-process causal primitive |
|   [2]   | WIRE_LAW      | One Strict context per package; app roots merge resolvers, emit schemas        |
|   [3]   | TS_PROJECTION | Tooling map and the envelope wire contract the TS dashboard consumes           |

## [2]-[PORT_RECORDS]

- Owner: `ReceiptEnvelope`, `ReceiptSinkPort`, `TelemetryContributorPort`, `DrainParticipantPort`, `HostAttachPort`, `UiSchedulerPort`, `SupportContributorPort`, `HealthContributorPort` — sealed records of delegates and policy values; zero interfaces, zero inheritance contracts, zero provider-branded vocabulary.
- Entry: `public IO<ReceiptEnvelope> Send(CorrelationId correlation, string package, string kind, JsonElement payload)` — `IO` carries the sink effect; the returned envelope is the emission evidence.
- Receipt: `ReceiptEnvelope` is the only cross-process causal primitive — receipts, support bundles, and degradation stay process-local and correlate across processes solely through the HLC stamp; `SkewBound` derives at stamp time as the wall-clock lag the advance observed, so the evidence view surfaces skew with zero configuration.
- Packages: LanguageExt.Core, NodaTime, BCL inbox
- Growth: a new cross-package seam lands as one registration row on an existing port, zero new surface; an eighth port record is the named defect.
- Boundary: every registration enters as a `TryAddEnumerable` ordered `ServiceDescriptor` row and subscriptions return disposable detachers composed LIFO; the contributor ports carry the settled row vocabularies — instrument rows, artifact rows, probe rows — never re-spelled fields; no sibling assembly enters the AppHost graph, and an aggregate store, compute, companion, or outbound-client port is the rejected form — that content decomposes into rows on these seven.

```csharp signature
public sealed record ReceiptEnvelope(
    CorrelationId Correlation,
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

    public IO<ReceiptEnvelope> Send(CorrelationId correlation, string package, string kind, JsonElement payload) =>
        IO.lift(() => Clock.GetCurrentInstant())
            .Map(wall => (Wall: wall, Cell: Hlc.Swap(last => Advance(last, wall))))
            .Map(state => new ReceiptEnvelope(
                correlation, package, kind, payload,
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
- Growth: a new wire record lands as one `[JsonSerializable]` row on its package context plus one `[JsonDerivedType]` row per polymorphic leaf, zero new surface; a new semantic-time edge is one `NodaPatterns` row; an owner that must decline span-based deserialization is one predicate branch in the factory's `Func<Type, bool>` opt-out, never a second factory; CORS and grpc-web middleware land as one app-root row each when a cross-origin deployment exists.
- Boundary: converter precedence is settled — `ConfigureForNodaTime` runs last in the `Wire` expression, after `TypeInfoResolver` binds the combined source-gen metadata, so the NodaTime per-type converters for `Instant`, `OffsetDateTime`, `ZonedDateTime`, and `Interval` resolve ahead of any source-gen `JsonTypeInfo` for those types — converter resolution precedes resolver metadata in System.Text.Json, and the call order in `Wire` is the precedence law, never a hand-assembled converter list; `OffsetDateTimePattern.Rfc3339` carries the exported offset stamp and `ZonedDateTimePattern` binds `WithZoneProvider`/`WithResolver` against the Tzdb provider with the strict resolver, so an ambiguous or skipped local time is a typed parse failure, never a silent shift; generated Thinktecture converters own every value-object, smart-enum, and keyed-union wire form, and a hand-written converter beside them is the named defect; the one options-level `ThinktectureJsonConverterFactory(skipObjectsWithJsonConverterAttribute: true, ...)` covers any generated owner that carries no `[JsonConverter]` attribute while honoring the attribute on those that do, so attribute wiring and options registration never double-bind one owner, and the `Func<Type, bool>` span opt-out callback declines span-based deserialization per type — `NoSpanDeserialization` is the suite default that holds the validated-rail read path uniform across owners, the per-type flip being the growth axis; PipeReader deserialization is the consumer-edge inbound decode shape, never a staging axis.

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
public partial class AppHostWireContext : JsonSerializerContext;

public static class SuiteContracts
{
    static readonly Func<Type, bool> NoSpanDeserialization = static _ => false;

    public static JsonSerializerOptions Wire(params ReadOnlySpan<JsonSerializerContext> contexts) =>
        new JsonSerializerOptions(JsonSerializerOptions.Strict)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            TypeInfoResolver = JsonTypeInfoResolver.Combine([.. contexts]),
            Converters = { new ThinktectureJsonConverterFactory(skipObjectsWithJsonConverterAttribute: true, NoSpanDeserialization) },
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

- Owner: `RasmPackage`, `HlcStampWire`, `ReceiptEnvelopeWire` — the suite-level TS contract; per-record wire shapes ride their owning wire surfaces and bind here as `TPayload`.
- Entry: `ReceiptEnvelopeWire<TPayload>` binds at the codec edge where `SuiteContracts.Wire` emits the runtime record and `SuiteContracts.Schema` derives its TS type; every wire payload reconstructs through this one envelope, never a hand-mirrored interface.
- Packages: BCL inbox
- Growth: a new wire payload lands as one payload row bound through `ReceiptEnvelopeWire`, zero new surface; the tooling map gains one tool row per new wire codec.
- Boundary: `logical` resets to zero on every physical advance, so the counter never approaches the JSON number precision envelope; `physical` and `skewBound` cross as NodaTime ISO-8601 and roundtrip-pattern strings; Thinktecture keyed owners cross as their key scalars while polymorphic leafs cross with the kind literals their polymorphic metadata pins, reconstructed in TS as literal-discriminated unions.

```ts contract
type RasmPackage = "Rasm.AppHost" | "Rasm.Persistence" | "Rasm.Compute" | "Rasm.AppUi";

interface HlcStampWire {
  physical: string;
  logical: number;
  skewBound: string;
}

interface ReceiptEnvelopeWire<TPayload> extends HlcStampWire {
  correlation: string;
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

## [5]-[RESEARCH]

- [ZONED_PATTERN_GRAMMAR]: the `ZonedDateTimePattern.CreateWithInvariantCulture` format-string literal for the exported zoned timestamp, and the named ISO singleton if one supplants the format-string construction.
