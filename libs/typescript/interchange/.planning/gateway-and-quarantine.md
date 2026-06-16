# [INTERCHANGE_GATEWAY_AND_QUARANTINE]

One page owns the wire boundary's tolerance terminal and its outbound-verb face — the quarantine fold every decode passes through, the contract-inventory and versioning fences, and the command gateway and intent registry that are the outbound-dial face of `WireClients`. The gateway is generated-client unary calls plus a receipt fold, so it is wire-consumption, not state: the domain that owns `WireTransport` owns every outbound dial. The owning C# `#TS_PROJECTION` fence is the authoritative wire shape; this page transcribes only `#TS_PROJECTION` fences and authors no shape.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]                | [OWNS]                                                            |
| :-----: | :----------------------- | :--------------------------------------------------------------- |
|   [1]   | CONTRACT_INVENTORY       | the eleven-cluster contract map and the codec-posture and versioning fences |
|   [2]   | GATEWAY_AND_QUARANTINE    | the tolerance fold plus the command gateway and intent registry  |
|   [3]   | TS_PROJECTION            | the command, availability, and envelope wire shapes the gateway binds |

## [2]-[CONTRACT_INVENTORY]

- Owner: the inventory of every wire contract the branch consumes, naming each owning .NET page and its codec; the `#TS_PROJECTION` fence on that page is the authoritative shape, and three suite-level shapes anchor the whole wire: `MethodShape` keying every proto rpc (transport.md), `TenantContextWire` partitioning every receipt by tenancy, and the W3C TraceContext call-spine constants stamping every outbound proto call. The versioning law is encoded here as the drift-tolerance obligations each enforced by its `codec-rails.md#CODEC_RAILS` `SchemaRefinement` row rather than a prose check.
- Cases: the eleven clusters are AppHost lifecycle-and-drain, health-and-degradation, support-bundles, and runtime-ports (json-stj); Persistence snapshot-codecs and sync-collaboration (messagepack); Compute remote-lane and progress-and-observation (proto) and receipts-and-benchmarks (json-stj); AppUi commands-availability and diagnostics-evidence (json-stj). `TenantContextWire` rides the `tenant` member of every `ReceiptEnvelopeWire` with `tenantId` crossing as the `UInt128` decimal-string the .NET `TenantId` value object emits, so a TS dashboard reads the identical tenancy identity the .NET RLS predicate reads, never a re-minted client tenant key. The .NET side classifies drift as Identical, Additive (tolerated), or Breaking (typed rejection); reserved field numbers never return to use and generated parsers retain unknown fields — proto fields add numbered fields only, JSON members add additive members, kind literals add one literal folding an unknown to the quarantine case, key scalars carry the ordinal-string brand, identity fields carry the guid and 16-byte content-key brand, and breaking drift surfaces as a typed `FaultDetail` through the fault rail.
- Entry: every json-stj receipt payload binds as the `TPayload` type parameter on `ReceiptEnvelopeWire`, the envelope `kind` mirroring the payload discriminator and the envelope `tenant`/`correlation`/HLC stamp carrying the cross-process primitives; the evidence timeline carries envelopes whole. W3C TraceContext is the call-spine stamp axis: `rasm-correlation` and `traceparent` are the two ambient header rows the outbound interceptor writes on every proto call, `traceparent` authored from the active span context and `tracestate` read-through on the .NET edge only.
- Packages: `@bufbuild/protobuf` and `@connectrpc/connect` for the descriptor and method-shape surface; `@msgpack/msgpack` for the messagepack surface; `effect` for the `Schema.Class` codec surface and the brand/filter primitives.
- Growth: a new wire contract lands as one inventory row under its owning package and codec; a new suite anchor lands as one cross-cluster binding row; a new versioning invariant lands as one `SchemaRefinement` row on `codec-rails.md#CODEC_RAILS`, never a prose assertion here; the four host-local .NET pages carry no `#TS_PROJECTION` cluster and never enter the inventory.
- Boundary: telemetry crosses no wire contract — the AppHost OTLP exporter ships signals from the app roots to the collector and dashboards read the collector; the four host-local .NET pages reconstruct solely through `ReceiptEnvelopeWire` and author no second wire shape; the branch stamps no tenancy onto an outbound call — tenancy is a server-minted receipt dimension the TS side only reads; every invariant names the `SchemaRefinement` row that enforces it rather than restating it untyped.

| [INDEX] | [C# CLUSTER]                     | [CODEC]     | [TS RAIL/FOLD]                                       |
| :-----: | :------------------------------- | :---------- | :--------------------------------------------------- |
|   [1]   | AppHost/lifecycle-and-drain      | json-stj    | RuntimeFeed                                           |
|   [2]   | AppHost/health-and-degradation   | json-stj    | HealthStore                                           |
|   [3]   | AppHost/support-bundles          | json-stj    | CommandGateway capture-support                        |
|   [4]   | AppHost/runtime-ports            | json-stj    | ReceiptEnvelopeCarrier                                |
|   [5]   | Persistence/snapshot-codecs      | messagepack | GeometryRail + SnapshotFeed                           |
|   [6]   | Persistence/sync-collaboration   | messagepack | DecodeRail + ConflictPresenceStore                    |
|   [7]   | Compute/remote-lane              | proto       | WireClients + ArtifactFrameRail + FaultDetailRail     |
|   [8]   | Compute/progress-and-observation | proto       | ProgressStore                                         |
|   [9]   | Compute/receipts-and-benchmarks  | json-stj    | ReceiptStore + BenchmarkRoute                         |
|  [10]   | AppUi/commands-availability      | json-stj    | CommandGateway + AvailabilityStore                    |
|  [11]   | AppUi/diagnostics-evidence       | json-stj    | EvidenceFeed + EvidenceTimelineRoute                  |

## [3]-[GATEWAY_AND_QUARANTINE]

- Owner: `QuarantineFold`, the single tolerance terminal every decode passes through, plus `CommandGateway`, the single gateway over the control verbs as the outbound-dial face of `WireClients`, and `IntentRegistry`, the deep-link key vocabulary the gateway resolves. The gateway is generated-client unary calls and a receipt fold — wire-consumption, co-located with the transport-owning domain because dialing dispatch is its outbound-effect surface even though it reads the availability fold, never folded into the platform-neutral `@rasm/projection` interior where it would leak `@connectrpc/connect-web` into the fold tier.
- Cases: an unknown discriminant folds to a quarantine case so the stream survives; a disconnect marks the value stale with a typed retry; an additive member skip-decodes; a breaking drift surfaces as a typed fault through `FaultDetailRail`. `CommandGateway` turns a UI intent into a unary call and folds the receipt back toward the `@rasm/projection` `AvailabilityStore` fold (an intra-package module edge, the store provided through the `invoke` `R` channel); the three verbs — capture a support bundle, set a degradation level, reload options — dial the control-service verbs on `remote-lane.md#TS_PROJECTION` and the support capture verb against `support-bundles.md#TS_PROJECTION`, payloads and outcomes carried by the command wire shapes against `commands-availability.md#TS_PROJECTION`; each dial runs through `Effect.tryPromise` whose `catch` hands the `ConnectError` to `FaultDetailRail.fromTrailer` so the dial surfaces the typed fault the rail reconstructs rather than swallowing the `ConnectError` as an opaque defect, and a fault-decode failure folds to the typed `FaultDetail.Quarantine` case. `IntentRegistry` addresses command intents by stable string keys against `commands-availability.md#TS_PROJECTION` so deep links survive a reload; a resolved intent dispatches through `CommandGateway` carrying its payload.
- Entry: the exhaustive case-fold proves at the type level that every wire discriminant the C# side enumerated has a landing, the quarantine case being the landing for the not-yet-enumerated; `CommandGateway` reads the `AvailabilityStore` fold as a gate so a disabled command never fires and holds no domain state of its own — the `AvailabilityStore` requirement rides the `invoke` `R` channel explicitly so the capability is never silently dropped, the fold being provided at the SPA composition root, not closed over here; intents resolve from the `@rasm/web` `DeepLinkBinding` query string, the key being the stable identifier never a re-derived display string.
- Packages: `@connectrpc/connect-web` for the unary dial, `@bufbuild/protobuf` for the request message construction, and `effect` for the `Match` primitive and the gateway-and-registry `Effect.Service` composition.
- Growth: a new union case the C# side adds lands as one literal on the owning rail and one fold arm; a new control verb lands as one `CommandGateway` method, never a sibling gateway; a new addressable intent lands as one `IntentRegistry` key bound to one gateway verb.
- Boundary: tolerance is a consumption behavior layered over the settled shape, never a modification of it; the `runtime-ports.md#WIRE_LAW` anchor is read only for the converter-precedence and HLC-stamp envelope-payload-binding discipline, never for token grounding; a second gateway beside `CommandGateway` is the named defect, and the gateway reads availability as a gate but never holds it — `AvailabilityStore` lives in `@rasm/projection`; a transport interceptor reading `AuthSession` is the only credential path and `@rasm/interchange` owns no session state.

```ts contract
type StaleMarker = { readonly _tag: "Stale"; readonly since: number };

interface QuarantineFold<Domain> {
  readonly fold: (event: Either.Either<Domain, ParseResult.ParseError>) => Domain | StaleMarker;
}

type ControlVerb = "captureSupport" | "setDegradation" | "reloadOptions";

interface CommandGateway {
  readonly invoke: (verb: ControlVerb, payload: CommandPayloadWire) => Effect.Effect<CommandReceiptWire, FaultDetail, AvailabilityStore>;
}

interface IntentRegistry {
  readonly resolve: (key: string) => Option.Option<{ readonly verb: ControlVerb; readonly payload: CommandPayloadWire }>;
}

class CommandGatewayLive extends Effect.Service<CommandGatewayLive>()("@rasm/interchange/CommandGateway", {
  effect: Effect.gen(function* () {
    const clients = yield* WireClients;
    const rail = yield* FaultDetailRail;
    const dial = (call: () => Promise<CommandReceiptWire>) =>
      Effect.tryPromise({ try: call, catch: (e) => e as ConnectError }).pipe(
        Effect.catchAll((e) => rail.fromTrailer(e).pipe(Effect.flatMap(Effect.fail), Effect.catchTag("ParseError", () => Effect.fail(FaultDetail.Quarantine({ code: "fault-decode", evidence: {} }))))),
      );
    const invoke = (verb: ControlVerb, payload: CommandPayloadWire) =>
      Effect.flatMap(AvailabilityStore, (availability) =>
        availability.isEnabled(verb).pipe(
          Effect.filterOrFail((ok) => ok, () => FaultDetail.HopFault({ code: "command-disabled", evidence: { verb } })),
          Effect.flatMap(() =>
            Match.value(verb).pipe(
              Match.when("captureSupport", () => dial(() => clients.control.captureSupport(payload))),
              Match.when("setDegradation", () => dial(() => clients.control.setDegradation(payload))),
              Match.when("reloadOptions", () => dial(() => clients.control.reloadOptions(payload))),
              Match.exhaustive,
            )),
        ));
    return { invoke } satisfies CommandGateway;
  }),
}) {}
```

## [4]-[TS_PROJECTION]

- Owner: the command, availability, and envelope wire shapes the gateway binds — transcribed from `commands-availability.md#TS_PROJECTION` and `runtime-ports.md#TS_PROJECTION`; the diagnostics-evidence and support-bundle shapes ride their owning consumers.
- Entry: `CommandPayloadWire` is the 4-case payload union, `CommandReceiptWire` binds as the `TPayload` on `ReceiptEnvelopeWire`, and `CommandAvailabilityWire` is the gate row the `AvailabilityStore` folds; the support-bundle `SupportReceipt` is the capture-support reply.
- Packages: `effect` `Schema` for the codec surface.
- Growth: a new command payload case lands as one union arm; a new availability row lands as one field; the branch authors no shape absent from the C# fence.
- Boundary: every shape transcribes a C# `#TS_PROJECTION` fence verbatim.

```ts contract
const CommandPayloadWire = Schema.Union(
  Schema.Struct({ _tag: Schema.Literal("none") }),
  Schema.Struct({ _tag: Schema.Literal("single"), value: Schema.String }),
  Schema.Struct({ _tag: Schema.Literal("many"), values: Schema.Array(Schema.String) }),
  Schema.Struct({ _tag: Schema.Literal("text"), text: Schema.String }),
);
type CommandPayloadWire = Schema.Schema.Type<typeof CommandPayloadWire>;

const CommandOutcomeWire = Schema.Literal("completed", "cancelled", "rejected", "faulted");

const CommandAvailabilityWire = Schema.Struct({
  key: Schema.String,
  available: Schema.Boolean,
  level: Schema.String,
});
type CommandAvailabilityWire = Schema.Schema.Type<typeof CommandAvailabilityWire>;

const CommandReceiptWire = Schema.Struct({
  intentKey: Schema.String,
  outcome: CommandOutcomeWire,
  payload: CommandPayloadWire,
});
type CommandReceiptWire = Schema.Schema.Type<typeof CommandReceiptWire>;

const HlcStampWire = Schema.Struct({ physical: Schema.String, logical: Schema.Number, skewBound: Schema.String });
const TenantContextWire = Schema.Struct({ tenantId: Schema.String, slug: Schema.String });
const RasmPackage = Schema.Literal("Rasm.AppHost", "Rasm.Persistence", "Rasm.Compute", "Rasm.AppUi");
```
