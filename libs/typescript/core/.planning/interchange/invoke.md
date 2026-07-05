# [CORE_INVOKE]

The capability plane of the interchange: both directions of the command contract under one page. Outbound — `CapabilityDescriptor`, the content-keyed command-shape identity admitted once at bind time through the plane's parity gate; `Dial`, the invocation service whose protocol axis (`connect` | `grpc-web`) is a Config-decoded policy record over the two `@connectrpc/connect-web` factories, threaded with the instrumented `fetch` and interceptor chain the composition root builds over the platform `HttpClient`, and whose lane ladder is one `ExecutionPlan` value so transport failover is a policy ladder, never a recovery cascade; the Effect lifts wiring fiber interruption to the call's `AbortSignal`, stamping W3C trace headers per call, and folding every caught value through the total `ConnectError` reconstruction into the codec `FaultDetail`; retry rides the `value` `Budget` schedules, class-gated through the fault's own classification with zero adapter. Inbound — `Gateway`, the verb dispatch over decoded `CommandPayload` verbs under the availability gate typed against `state` evidence, with the support-capture verb as one row delivering to the `SupportIntake` port, and `duplex`, the typed command/outcome channel whose frame row selects the fused codec transformer — `MsgPack.duplexSchema`, `Ndjson.duplexSchema`, or the JSON-safe `Ndjson.duplexSchemaString` text lane — one stage from byte channel to typed duplex under an unchanged asymmetric schema seam. The module is `core/src/interchange/invoke.ts`; a third protocol is one lane row, a new verb is one handler row in the app's table, and a per-method retry posture is one budget row at the bind call.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]          | [OWNS]                                                                  | [PUBLIC]                     |
| :-----: | :----------------- | :------------------------------------------------------------------------ | :--------------------------- |
|  [01]   | `TRANSPORT_FAULT`  | the total `ConnectError` fold into the codec fault vocabulary             | `Transport`                  |
|  [02]   | `DIAL_AXIS`        | the Config policy record, the lane table, the failover plan, the lifts    | `Dial`                       |
|  [03]   | `CAPABILITY_BIND`  | descriptor admission and the kind-total SDK derivation with budget rows   | `Capability`                 |
|  [04]   | `COMMAND_GATEWAY`  | verb dispatch, the availability gate, support capture, the duplex channel | `Gateway`, `SupportIntake`   |

## [2]-[TRANSPORT_FAULT]

[TRANSPORT_FAULT]:
- Owner: `Transport`, the transport-fault fold — `fault(caught)` normalizes any caught value through `ConnectError.from`, maps the closed sixteen-value `Code` enum through the codec `Hops` projection, decodes a server-attached `FaultDetailWire` detail through `findDetails` against the format registry, and reconstructs the codec `FaultDetail` with the local edge hop appended — so the error channel is one reconstructed wire fault end to end and no `ConnectError`, bare code, or raw `try`/`catch` reaches domain flow.
- Law: the fold is total — an `AbortError`/`TimeoutError` lands as `Canceled` through `ConnectError.from`, `Hops.fromCode` maps with `unknown` as the residue row, and a carried detail's hop chain merges ahead of the local edge hop so the first hop stays the origin; no ladder inspects codes.
- Law: the carried detail decodes through `FaultDetail.FromWire` — the codec's tagged-landing twin — so the untagged proto detail reconstructs whole and a decode miss degrades to the edge-only fault, never a dropped call.
- Law: this is the second and last `FaultDetail` construction site — the first is the codec registry's decode row; a third anywhere in the branch is the altitude defect the architecture suite audits.
- Growth: a new evidence axis follows the C# shape at the codec landing; this fold inherits it field-for-field with zero edits.
- Boundary: `Hops`, `FaultDetail`, and the enricher Layer are `codec#LANDING_WIRE`'s owners; the `Code` and `ConnectError` spellings are the `@connectrpc/connect` surface.
- Packages: `@connectrpc/connect` (`Code`, `ConnectError`); `effect` (`Array`, `Duration`, `Option`, `Schema`); `./codec.ts` (`FaultDetail`, `Hops`); `./format.ts` (`Proto`).

```typescript
import { Code, ConnectError } from "@connectrpc/connect"
import { Array, Duration, Option, Schema } from "effect"
import { FaultDetail, Hops } from "./codec.ts"
import { Proto } from "./format.ts"

const _edge = (error: ConnectError): FaultDetail.Hop =>
  new FaultDetail.Hop({ site: "<local-edge>", reason: Hops.fromCode(error.code), elapsed: Duration.zero })

const Transport: {
  readonly fault: (caught: unknown) => FaultDetail
} = {
  fault: (caught) => {
    const error = ConnectError.from(caught, Code.Unknown)
    const carried = Array.head(error.findDetails(Proto.suite.FaultDetailWire))
    return Option.match(
      Option.flatMap(carried, (wire) => Option.getRight(Schema.decodeUnknownEither(FaultDetail.FromWire)(wire))),
      {
        onNone: () =>
          new FaultDetail({
            reason: Hops.fromCode(error.code),
            surface: "<transport>",
            detail: error.rawMessage,
            hops: [_edge(error)],
            tenant: Option.none(),
          }),
        onSome: (detail) => new FaultDetail({ ...detail, hops: [...detail.hops, _edge(error)] }),
      },
    )
  },
}
```

## [3]-[DIAL_AXIS]

[DIAL_AXIS]:
- Owner: `Dial`, the invocation service — `Dial.Config` is the Schema the composition root decodes (protocol lanes in failover order, `baseUrl`, `useBinaryFormat`, `timeoutMs`); the Layer factory takes the decoded policy plus the `fetch` and `interceptors` seam values the root builds over the platform `HttpClient`, constructs one `Transport` per configured lane by vocabulary lookup, and exposes `client(service)` (the lane-keyed prebuilt `createClient` record), `plan` (the `ExecutionPlan` ladder over the `Lane` Tag), and the two lifts `unary`/`stream`.
- Law: the transports honor the catalog admission — every factory call passes the instrumented `fetch` and the shared `Interceptor` chain, so net policy and propagation ride every arm uniformly and a bare transport is unspellable; `baseUrl`, `timeoutMs`, and format arrive Config-decoded, and binary is the default posture for the C#-emitted services.
- Law: failover is one plan value — the primary lane engages first, later lanes engage only while the fault's classification is retryable (`FaultClass.retryable` over the reconstructed fault), each lane under its own attempt bound; a `catchAll` re-dial cascade is the deleted spelling, and a third protocol is one `_lanes` row plus one config literal.
- Law: interruption crosses with the call — `Effect.tryPromise`'s evaluator receives the fiber-wired `AbortSignal` handed to `CallOptions.signal`, so a scope close or race loss aborts the in-flight RPC as `Canceled` and no controller is hand-managed.
- Law: trace identity is per-call headers, never a fetch rewrite — the current span reads off the fiber, `HttpTraceContext.toHeaders` spells the W3C pair, absence of a span sends no header, and propagation is uniform across lanes because it rides `CallOptions`; tenant and auth context ride the root-supplied interceptor chain.
- Law: transports construct once at the Layer — per-call construction re-mints connection state and defeats interceptor identity; the service's scoped life owns every lane.
- Growth: a per-call context axis is one line in `_stamped`; a lane-policy axis (per-lane attempt bound, a lane gate) is one field on the lane row.
- Boundary: the factory option records are the `@connectrpc/connect-web` surface; a lane needing XHR upload progress bypasses this axis for the platform XHR client, stated at the consumer; the root's `fetch` construction over its `HttpClient` is the runtime wave's composition.
- Packages: `@connectrpc/connect` (`createClient`, `Client`, `Interceptor`, `Transport`); `@connectrpc/connect-web` (`createConnectTransport`, `createGrpcWebTransport`); `@bufbuild/protobuf` (`DescService`); `@effect/platform` (`Headers`, `HttpTraceContext`); `effect` (`Context`, `Effect`, `ExecutionPlan`, `Layer`, `Record`, `Stream`); `../value/fault.ts` (`FaultClass`).

```typescript
import type { Client, Interceptor, Transport as ConnectTransport } from "@connectrpc/connect"
import { createClient } from "@connectrpc/connect"
import { createConnectTransport, createGrpcWebTransport } from "@connectrpc/connect-web"
import type { DescService } from "@bufbuild/protobuf"
import { Headers, HttpTraceContext } from "@effect/platform"
import { Context, Effect, ExecutionPlan, Layer, Match, type ParseResult, Record, Stream } from "effect"
import { FaultClass } from "../value/fault.ts"

const _protocols = ["connect", "grpc-web"] as const

const _DialConfig = Schema.Struct({
  lanes: Schema.NonEmptyArray(Schema.Literal(..._protocols)),
  baseUrl: Schema.NonEmptyString,
  useBinaryFormat: Schema.optionalWith(Schema.Boolean, { default: () => true }),
  timeoutMs: Schema.Int.pipe(Schema.between(1, 120000)),
})

declare namespace Dial {
  type Protocol = (typeof _protocols)[number]
  type Config = Schema.Schema.Type<typeof _DialConfig>
  type Seam = { readonly fetch: typeof globalThis.fetch; readonly interceptors: ReadonlyArray<Interceptor> }
  type _Rows<T extends Record<Protocol, (config: Config, seam: Seam) => ConnectTransport> = typeof _lanes> = T
}

const _lanes = {
  connect: (config: Dial.Config, seam: Dial.Seam): ConnectTransport =>
    createConnectTransport({
      baseUrl: config.baseUrl,
      useBinaryFormat: config.useBinaryFormat,
      defaultTimeoutMs: config.timeoutMs,
      fetch: seam.fetch,
      interceptors: [...seam.interceptors],
    }),
  "grpc-web": (config: Dial.Config, seam: Dial.Seam): ConnectTransport =>
    createGrpcWebTransport({
      baseUrl: config.baseUrl,
      useBinaryFormat: config.useBinaryFormat,
      defaultTimeoutMs: config.timeoutMs,
      fetch: seam.fetch,
      interceptors: [...seam.interceptors],
    }),
} as const

class Lane extends Context.Tag("@rasm/ts/core/Lane")<Lane, {
  readonly protocol: Dial.Protocol
  readonly transport: ConnectTransport
}>() {}

const _stamped: Effect.Effect<Headers.Headers> = Effect.map(
  Effect.option(Effect.currentSpan),
  Option.match({
    onNone: () => Headers.empty,
    onSome: (span) => HttpTraceContext.toHeaders(span),
  }),
)

const _unary = <O>(call: (signal: AbortSignal, headers: Headers.Headers) => Promise<O>): Effect.Effect<O, FaultDetail> =>
  Effect.flatMap(_stamped, (headers) =>
    Effect.tryPromise({ try: (signal) => call(signal, headers), catch: Transport.fault }))

const _streamed = <O>(open: (headers: Headers.Headers) => AsyncIterable<O>): Stream.Stream<O, FaultDetail> =>
  Stream.unwrap(Effect.map(_stamped, (headers) => Stream.fromAsyncIterable(open(headers), Transport.fault)))

class Dial extends Effect.Service<Dial>()("@rasm/ts/core/Dial", {
  effect: (config: Dial.Config, seam: Dial.Seam) =>
    Effect.sync(() => {
      const transports = Record.map(_lanes, (make) => make(config, seam))
      const ladder = Array.map(config.lanes, (protocol, rank) => ({
        provide: Layer.succeed(Lane, { protocol, transport: transports[protocol] }),
        attempts: rank === 0 ? 1 : 2,
        while: (fault: FaultDetail) => FaultClass.retryable(fault),
      }))
      return {
        plan: ExecutionPlan.make(Array.headNonEmpty(ladder), ...Array.tailNonEmpty(ladder)),
        client: <T extends DescService>(service: T): Readonly<Record<Dial.Protocol, Client<T>>> =>
          Record.map(transports, (transport) => createClient(service, transport)),
        unary: _unary,
        stream: _streamed,
      }
    }),
  accessors: true,
}) {
  static readonly Config: typeof _DialConfig = _DialConfig
  static readonly Lane: typeof Lane = Lane
}
```

## [4]-[CAPABILITY_BIND]

[CAPABILITY_BIND]:
- Owner: `Capability` — `CapabilityDescriptor`, the decoded capability identity (name, qualified service name, the content key of the command shape, the mint instant) with `admit` comparing the runtime-shipped key against the build pin through the codec parity gate; `Sdk<T>`, the mapped type computing every method's Effect signature from the promise `Client<T>`; and `bind`, the kind-total fold walking `service.methods` by `methodKind` under the failover plan, wrapping each method through the `Dial` lifts with per-method `Budget` retry rows.
- Law: content-keyed admission — the key covers the command shape's canonical bytes, branded-key equality is bare `===` under the one mint, the check runs once at bind time, and a diverging key refuses with `parity` evidence because a capability whose command shape moved is a different capability.
- Law: the SDK is a descriptor, never a hand-written client — `Sdk<T>` maps `Client<T>`'s own member types into Effect carriers, the runtime record builds from the descriptor's own `methods` walk, and the two derive from one descriptor; a parallel interface per capability is the second-truth defect.
- Law: the derivation is kind-total over the shipped axis through `Match.discriminatorsExhaustive("methodKind")` — `unary` and `server_streaming` bind; `client_streaming` and `bidi_streaming` refuse at bind time as `drift` evidence because the C# emitter does not mint them; a fifth method kind breaks the fold loudly at compile time, and silence over an unbindable method strands its caller at runtime — the catch-all ternary that would absorb it is the deleted spelling.
- Law: retry policy is the branch budget vocabulary — a method with a budget row composes `Effect.retry(Budget.schedule(kind))`, class-gated through the fault's own `class` projection so a terminal reason never re-drives; a method without a row never retries, the safe default for non-idempotent verbs; the per-attempt deadline is the transport's `timeoutMs` below the retry while the budget's window bounds the whole call above it.
- Law: every bound method runs under the failover plan — `Effect.withExecutionPlan` attaches the `Dial` ladder so a retryable transport fault walks the lane ladder before the budget schedule re-drives, and the lane choice is invisible at every call site.
- Exemption: the `_slots` client pin, the walk's slot reads, and the terminal `Object.fromEntries` assembly are the page's one sanctioned assertion cluster — the mapped type computes from `Client<T>` while the runtime record builds from the descriptor's `methods` walk, so the pin, the proven-slot reads, and the assembly each state that one-descriptor correspondence, confined under the `// BOUNDARY ADAPTER` mark.
- Growth: a new method appears in the SDK at regeneration with zero edits here; a method gaining idempotency is one budget row at the caller.
- Boundary: the emitted `DescService` consts are build artifacts the app's capability modules import; the `CapabilityDescriptorWire` census row homes here; `Budget` rows are `value/fault.ts` vocabulary.
- Packages: `@connectrpc/connect` (`Client`); `@bufbuild/protobuf` (`DescService`); `effect` (`Effect`, `Match`, `Stream`, `Option`, `Schema`); `./codec.ts` (`Parity`, `WireFault`); `./format.ts` (`Proto`); `../value/contentKey.ts` (`ContentKey`, `Digest`); `../value/fault.ts` (`Budget`).

```typescript
import { Budget } from "../value/fault.ts"
import { type ContentKey, Digest } from "../value/contentKey.ts"
import { Parity, WireFault } from "./codec.ts"

class CapabilityDescriptor extends Schema.Class<CapabilityDescriptor>("CapabilityDescriptor")({
  name: Schema.NonEmptyString,
  service: Schema.NonEmptyString,
  key: Digest.FromBytes,
  minted: Schema.DateTimeUtc,
}) {
  static readonly FromBytes: Schema.Schema<CapabilityDescriptor, Uint8Array> = Proto.family(
    Proto.suite.CapabilityDescriptorWire,
    CapabilityDescriptor,
  )
  static readonly admit = (
    octets: Uint8Array,
    pinned: ContentKey,
  ): Effect.Effect<CapabilityDescriptor, ParseResult.ParseError | WireFault> =>
    Effect.tap(
      Schema.decodeUnknown(CapabilityDescriptor.FromBytes)(octets),
      (descriptor) => Parity.matched("CapabilityDescriptorWire", descriptor.key, pinned),
    )
}

declare namespace Capability {
  type Sdk<T extends DescService> = {
    readonly [K in keyof Client<T>]: Client<T>[K] extends (input: infer I, options?: infer _O) => Promise<infer O>
      ? (input: I) => Effect.Effect<O, FaultDetail>
      : Client<T>[K] extends (input: infer I, options?: infer _O) => AsyncIterable<infer O>
        ? (input: I) => Stream.Stream<O, FaultDetail>
        : never
  }
  type Budgets = Readonly<Record<string, Budget.Kind>>
  type _Slot = (
    input: unknown,
    options?: { readonly signal?: AbortSignal; readonly headers?: Headers.Headers },
  ) => Promise<unknown> & AsyncIterable<unknown>
}

const _slots = <T extends DescService>(client: Client<T>): Readonly<Record<string, Capability._Slot>> =>
  client as unknown as Readonly<Record<string, Capability._Slot>>

const _unbindable = (kind: string, name: string): WireFault =>
  new WireFault({
    family: "CapabilityDescriptorWire",
    reason: "drift",
    detail: `<unbindable-kind:${kind}:${name}>`,
    evidence: Option.none(),
  })

const Capability: {
  readonly Descriptor: typeof CapabilityDescriptor
  readonly bind: <T extends DescService>(service: T, budgets?: Capability.Budgets) => Effect.Effect<Capability.Sdk<T>, WireFault, Dial>
} = {
  Descriptor: CapabilityDescriptor,
  bind: <T extends DescService>(service: T, budgets?: Capability.Budgets) =>
    Effect.gen(function* () {
      const dial = yield* Dial
      const slots = Record.map(dial.client(service), _slots)
      const rows = yield* Effect.forEach(service.methods, (method) =>
        Match.value(method).pipe(
          Match.discriminatorsExhaustive("methodKind")({
            unary: (unary) =>
              Effect.succeed([
                unary.localName,
                (input: unknown) => {
                  const call = Effect.withExecutionPlan(
                    Effect.flatMap(Lane, (lane) =>
                      dial.unary((signal, headers) => slots[lane.protocol][unary.localName]!(input, { signal, headers }))),
                    dial.plan,
                  )
                  const kind = budgets?.[unary.localName]
                  return kind === undefined ? call : Effect.retry(call, Budget.schedule(kind))
                },
              ] as const),
            server_streaming: (streaming) =>
              Effect.succeed([
                streaming.localName,
                (input: unknown) =>
                  Stream.unwrap(
                    Effect.withExecutionPlan(
                      Effect.map(Lane, (lane) =>
                        dial.stream((headers) => slots[lane.protocol][streaming.localName]!(input, { headers }))),
                      dial.plan,
                    ),
                  ),
              ] as const),
            client_streaming: (refused) => Effect.fail(_unbindable(refused.methodKind, refused.localName)),
            bidi_streaming: (refused) => Effect.fail(_unbindable(refused.methodKind, refused.localName)),
          }),
        ))
      return Object.fromEntries(rows) as Capability.Sdk<T>
    }),
}
```

## [5]-[COMMAND_GATEWAY]

[COMMAND_GATEWAY]:
- Owner: `Gateway`, the inbound half of the capability plane — `CommandPayload`, the decoded command class (verb, body carriage, tenant, `Hlc` stamp); the `Effect.Service` whose Layer factory takes the app's verb row table so the gateway is one generic dispatch and a verb set is data; `Dispatched`, the Granted/Refused outcome family carrying the `state` verdict whole; `AvailabilityGate`, the port this page declares and the app root satisfies from `state`-fed evidence; `SupportCapture` with its receipt and the `SupportIntake` port as the support verb's delivery; and `duplex`, the typed command/outcome channel over the fused `duplexSchema` transformers.
- Law: the row table is the dispatch — one indexed lookup over the app-supplied record; an unknown verb is `drift` evidence because the shell and the app were built against one verb set; handlers receive the decoded payload and each row decodes its own body band, the second-admission law for nested unknown material.
- Law: the gate types against `state` vocabulary, never re-declares it — the port answers `Availability.Verdict`, refusal transports the verdict whole (`Gated` keeps its `until`, `Withheld` its level), and gating is read-then-dispatch: staleness policy belongs to the providing Layer.
- Law: the support verb is one row on the same plane — the evidence band crosses opaque, interpretation belongs to the intake's consumer, and the port is declared here and satisfied at the root so the observe unit and this plane stay ledger-clean; the receipt travels back to the reporter so a support report is never fire-and-forget.
- Law: the duplex channel is one fused schema seam over a swappable frame row — `MsgPack.duplexSchema`/`Ndjson.duplexSchema` collapse the frame codec and the asymmetric schema pair (commands inbound, outcomes outbound, backpressure carried) into one channel transformer, so the two-stage frame-then-`ChannelSchema` sandwich is the deleted spelling; the socket arrives as a byte channel through the runtime-satisfied constructor Tag, and raw listeners and hand framing are unspellable above it.
- Law: the msgpack row is the command lane's standing frame — the `Hlc` halves are `bigint` and JSON owns no bigint spelling, so both ndjson rows are legal only over JSON-safe encoded schemas on both directions; `ndjson-text` is `Ndjson.duplexSchemaString`, the string-frame lane for text-only transports, under the same JSON-safety law — a serving edge that must frame commands as text lands its JSON stamp spelling first, and swapping a row under a bigint-carrying schema is the precision defect the frame discriminant cannot absorb.
- Growth: a new verb is one row in the app's table; a new outcome kind is one tagged case every exhaustive consumer breaks on; a fourth frame row is one arm on the `Frame` discriminant.
- Boundary: the `CommandPayloadWire`/`SupportCaptureWire` census rows home here; the availability vocabulary and the total `admits` fallback are `state/evidence.ts`'s; the socket Layer and the serving loop are the runtime wave's.
- Packages: `@effect/platform` (`MsgPack`, `Ndjson`, `Socket`); `effect` (`Context`, `Data`, `Effect`, `Schema`, `Option`); `./codec.ts` (`WireFault`); `./format.ts` (`Proto`); `../value/clock.ts` (`Hlc`); `../value/identity.ts` (`TenantContext`); `../state/evidence.ts` (`Availability`).

```typescript
import { MsgPack, Ndjson, Socket } from "@effect/platform"
import { type Channel, type Chunk, Data } from "effect"
import { Hlc } from "../value/clock.ts"
import { TenantContext } from "../value/identity.ts"
import type { Availability } from "../state/evidence.ts"

class CommandPayload extends Schema.Class<CommandPayload>("CommandPayload")({
  verb: Schema.NonEmptyString,
  body: Schema.Unknown,
  tenant: TenantContext,
  stamp: Hlc,
}) {
  static readonly FromBytes: Schema.Schema<CommandPayload, Uint8Array> = Proto.family(
    Proto.suite.CommandPayloadWire,
    CommandPayload,
  )
}

type Dispatched = Data.TaggedEnum<{
  Granted: { readonly verb: string; readonly receipt: unknown }
  Refused: { readonly verb: string; readonly verdict: Availability.Verdict }
}>
const Dispatched: Data.TaggedEnum.Constructor<Dispatched> = Data.taggedEnum<Dispatched>()

class AvailabilityGate extends Context.Tag("@rasm/ts/core/AvailabilityGate")<AvailabilityGate, {
  readonly admits: (verb: string) => Effect.Effect<Availability.Verdict>
}>() {}

const _kinds = ["crash", "bug", "feedback"] as const

class SupportReceipt extends Schema.Class<SupportReceipt>("SupportReceipt")({
  reference: Schema.NonEmptyString,
  kind: Schema.Literal(..._kinds),
  at: Schema.DateTimeUtc,
}) {}

class SupportCapture extends Schema.Class<SupportCapture>("SupportCapture")({
  kind: Schema.Literal(..._kinds),
  note: Schema.NonEmptyString,
  fingerprint: Schema.NonEmptyString,
  evidence: Schema.Uint8ArrayFromSelf,
  at: Schema.DateTimeUtc,
}) {
  static readonly Receipt: typeof SupportReceipt = SupportReceipt
  static readonly FromBytes: Schema.Schema<SupportCapture, Uint8Array> = Proto.family(
    Proto.suite.SupportCaptureWire,
    SupportCapture,
  )
  static readonly captured = (octets: Uint8Array): Effect.Effect<SupportReceipt, ParseResult.ParseError, SupportIntake> =>
    Schema.decodeUnknown(SupportCapture.FromBytes)(octets).pipe(
      Effect.flatMap((report) => Effect.flatMap(SupportIntake, (intake) => intake.deliver(report))),
    )
}

class SupportIntake extends Context.Tag("@rasm/ts/core/SupportIntake")<SupportIntake, {
  readonly deliver: (report: SupportCapture) => Effect.Effect<SupportReceipt>
}>() {}

declare namespace Gateway {
  type Handlers = Readonly<Record<string, (payload: CommandPayload) => Effect.Effect<unknown, WireFault>>>
  type Frame = "msgpack" | "ndjson" | "ndjson-text"
}

class Gateway extends Effect.Service<Gateway>()("@rasm/ts/core/Gateway", {
  effect: (handlers: Gateway.Handlers) =>
    Effect.map(AvailabilityGate, (gate) => ({
      submit: (octets: Uint8Array): Effect.Effect<Dispatched, ParseResult.ParseError | WireFault> =>
        Effect.gen(function* () {
          const payload = yield* Schema.decodeUnknown(CommandPayload.FromBytes)(octets)
          const handle = yield* Option.match(Option.fromNullable(handlers[payload.verb]), {
            onNone: () =>
              new WireFault({
                family: "CommandPayloadWire",
                reason: "drift",
                detail: `<unknown-verb:${payload.verb}>`,
                evidence: Option.none(),
              }),
            onSome: Effect.succeed,
          })
          const verdict = yield* gate.admits(payload.verb)
          return yield* verdict._tag === "Available"
            ? Effect.map(handle(payload), (receipt) => Dispatched.Granted({ verb: payload.verb, receipt }))
            : Effect.succeed(Dispatched.Refused({ verb: payload.verb, verdict }))
        }),
    })),
  accessors: true,
}) {
  static readonly Payload: typeof CommandPayload = CommandPayload
  static readonly Outcome: Data.TaggedEnum.Constructor<Dispatched> = Dispatched
  static duplex<A, I>(
    socket: Socket.Socket,
    frame: Gateway.Frame,
    outcome: Schema.Schema<A, I>,
  ): Channel.Channel<
    Chunk.Chunk<CommandPayload>,
    Chunk.Chunk<A>,
    MsgPack.MsgPackError | Ndjson.NdjsonError | ParseResult.ParseError | Socket.SocketError,
    ParseResult.ParseError,
    void,
    unknown
  > {
    const bytes = Socket.toChannelWith<ParseResult.ParseError>()(socket)
    const seam = { inputSchema: outcome, outputSchema: CommandPayload } as const
    return frame === "msgpack"
      ? bytes.pipe(MsgPack.duplexSchema(seam))
      : frame === "ndjson"
        ? bytes.pipe(Ndjson.duplexSchema(seam))
        : bytes.pipe(Ndjson.duplexSchemaString(seam))
  }
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { AvailabilityGate, Capability, Dial, Gateway, SupportCapture, SupportIntake, Transport }
```
