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

- Owner: `AppHostWireContext`, `SuiteContracts` — the package wire context and the app-root merge surface.
- Entry: `public static JsonSerializerOptions Wire(params ReadOnlySpan<JsonSerializerContext> contexts)` — one merge per app root; every JSON wire surface reads and writes through the merged options value, which seals on first use.
- Packages: NodaTime.Serialization.SystemTextJson, Thinktecture.Runtime.Extensions.Json, BCL inbox
- Growth: a new wire record lands as one `[JsonSerializable]` row on its package context plus one `[JsonDerivedType]` row per polymorphic leaf, zero new surface; CORS and grpc-web middleware land as one app-root row each when a cross-origin deployment exists.
- Boundary: `ConfigureForNodaTime` owns NodaTime converter ordering — Interval and ZonedDateTime converters bind after the Instant converter, so a hand-assembled converter list is the rejected form; generated Thinktecture converters own every value-object, smart-enum, and keyed-union wire form, and a hand-written converter beside them is the named defect; PipeReader deserialization is the consumer-edge inbound decode shape, never a staging axis.

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
    public static JsonSerializerOptions Wire(params ReadOnlySpan<JsonSerializerContext> contexts) =>
        new JsonSerializerOptions(JsonSerializerOptions.Strict)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            TypeInfoResolver = JsonTypeInfoResolver.Combine([.. contexts]),
        }.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);

    public static JsonNode Schema(JsonSerializerOptions wire, Type record) =>
        JsonSchemaExporter.GetJsonSchemaAsNode(wire, record, new JsonSchemaExporterOptions
        {
            TreatNullObliviousAsNonNullable = true,
        });
}
```

Codec residence is fixed per wire surface; exactly one codec owns each surface and a second codec on one surface is a conflict, not a fallback:

| [INDEX] | [WIRE_SURFACE]                                                                                            | [CODEC]                                    | [PRODUCER]                                     | [CONSUMER]                      |
| :-----: | :-------------------------------------------------------------------------------------------------------- | :----------------------------------------- | :--------------------------------------------- | :------------------------------ |
|   [1]   | runtime records — phase, fault, health snapshot, degradation, support manifest, receipt envelope          | STJ Strict source-gen JSON                 | per-package context merged at app roots        | TS dashboard, companion upload  |
|   [2]   | discovery manifest — pid, socketPath, startInstant, contractChecksum, storeEpoch                          | STJ Strict JSON, atomic file write         | app-root boot                                  | attaching peer process          |
|   [3]   | service verbs and streams — ComputeService, DocumentService, ControlService, ArtifactSync, grpc.health.v1 | protobuf descriptors over gRPC             | app roots compiling GrpcServices=Server        | connect-es generated clients    |
|   [4]   | fault unions on the wire                                                                                  | FaultDetail into google.rpc.Status details | wire-edge projection                           | TS typed-failure reconstruction |
|   [5]   | snapshot blobs                                                                                            | MessagePack                                | snapshot codec rows                            | @msgpack/msgpack                |
|   [6]   | telemetry signals                                                                                         | OTLP                                       | exporter pinned at companion and web app roots | any OTLP collector              |
|   [7]   | JSON contract schemas                                                                                     | JsonSchemaExporter emission                | schema emission at app roots                   | schema-derived TS types         |

## [4]-[TS_PROJECTION]

- Owner: `RasmPackage`, `HlcStampWire`, `ReceiptEnvelopeWire` — the suite-level TS contract; per-record wire shapes ride their owning wire surfaces and bind here as `TPayload`.
- Packages: BCL inbox
- Growth: a new wire payload lands as one payload row bound through `ReceiptEnvelopeWire`, zero new surface; the tooling map gains one tool row per new wire codec.
- Boundary: `logical` resets to zero on every physical advance, so the counter never approaches the JSON number precision envelope; `physical` and `skewBound` cross as NodaTime ISO-8601 and roundtrip-pattern strings; Thinktecture keyed owners cross as their key scalars while polymorphic leafs cross with the locked kind literals their polymorphic metadata pins, reconstructed in TS as literal-discriminated unions.

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

Each tool row names the surface it consumes, its activation point, and the spelling it deletes:

| [INDEX] | [TOOL]                            | [SURFACE]                                                         | [ACTIVATION]                                             | [DELETES]                                   |
| :-----: | :-------------------------------- | :---------------------------------------------------------------- | :------------------------------------------------------- | :------------------------------------------ |
|   [1]   | connect-es with protoc-gen-es     | suite service descriptors, binary format, unary and server-stream | pnpm workspace bootstrap over the emitted descriptor set | hand-written wire clients and request types |
|   [2]   | @msgpack/msgpack with useBigInt64 | binary snapshot blobs                                             | dashboard snapshot import                                | a second binary codec on the TS side        |
|   [3]   | OTLP ingestion                    | telemetry signals                                                 | collector endpoint bound through OTEL_EXPORTER_OTLP_*    | bespoke telemetry wire formats              |
|   [4]   | schema-derived TS types           | JSON runtime records via emitted contract schemas                 | app-root schema emission feeding the TS build            | hand-mirrored TS interfaces that drift      |

## [5]-[RESEARCH]

| [INDEX] | [ITEM]                                                                                       | [PROOF]                                                       | [GATE]   |
| :-----: | :------------------------------------------------------------------------------------------- | :------------------------------------------------------------ | :------- |
|   [1]   | NodaTime converter precedence over combined source-gen contract metadata in the Strict merge | `uv run python -m tools.assay test run --target Rasm.AppHost` | WIRE_LAW |
