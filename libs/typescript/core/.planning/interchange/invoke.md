# [CORE_INVOKE]

Interchange capability owns both directions of the command contract. Outbound `CapabilityDescriptor` admits content-keyed command identity at bind time, while `Dial` derives its Connect protocol axis and failover `ExecutionPlan` from policy rows. Per-call lifts carry interruption, typed ambient values, W3C context, classified retry budgets, reconstructed `FaultDetail`, and `observe/convention` telemetry on one Effect rail. Inbound `Gateway` folds each verb row's body, receipt, handler, and duplex frame into one total dispatch surface that refuses unknown verbs as drift. Fused MsgPack and NDJSON transformers preserve the asymmetric schema seam from socket to typed duplex. Module `core/src/interchange/invoke.ts` admits a protocol as one lane row, a verb as one app-table row, and retry posture as one bind-time budget row.

## [01]-[CLUSTERS]

| [INDEX] | [CLUSTER]         | [OWNS]                                                                           | [PUBLIC]                   |
| :-----: | :---------------- | :-------------------------------------------------------------------------------- | :------------------------- |
|  [01]   | `TRANSPORT_FAULT` | the total `ConnectError` fold into the codec fault vocabulary                    | `Transport`                |
|  [02]   | `DIAL_AXIS`       | Config policy record, three-lane table, failover plan, ambient context, lifts    | `Dial`                     |
|  [03]   | `CAPABILITY_BIND` | descriptor admission, kind-total SDK derivation, budget geometry, telemetry seam | `Capability`               |
|  [04]   | `COMMAND_GATEWAY` | the verb-row contract, the availability gate, support capture, the duplex        | `Gateway`, `SupportIntake` |

## [02]-[TRANSPORT_FAULT]

- Owner: `Transport`, the transport-fault fold — `fault(caught)` normalizes any caught value through `ConnectError.from`, maps the closed sixteen-value `Code` enum through the codec `Hops` projection, resolves every server-attached detail through `findDetails(Proto.registry)` so the whole registered detail vocabulary decodes in one call, reconstructs the codec `FaultDetail` from the `FaultDetailWire` member with the local edge hop appended, and `expired(surface, detail)` mints the locally-originated deadline fault the budget timeouts consume — so the error channel is one reconstructed wire fault end to end and no `ConnectError`, bare code, or raw `try`/`catch` reaches domain flow.
- Law: the fold is total — an `AbortError`/`TimeoutError` lands as `Canceled` through `ConnectError.from`, `Hops.fromCode` maps with `unknown` as the residue row, and a carried detail's hop chain merges ahead of the local edge hop so the first hop stays the origin; no ladder inspects codes.
- Law: detail resolution is registry-wide — `findDetails(Proto.registry)` decodes every `Any`-wrapped detail the format registry names, `isMessage(detail, Proto.suite.FaultDetailWire)` selects the fault member, and the carried detail decodes through `FaultDetail.FromWire` — the codec's tagged-landing twin — so the untagged proto detail reconstructs whole and a decode miss degrades to the edge-only fault, never a dropped call; a second registered detail family lands as one more `isMessage` probe over the same resolved set.
- Law: these are the second and last `FaultDetail` construction sites — the first is the codec registry's decode row; a third anywhere in the branch is the altitude defect the architecture suite audits, and the deadline mint rides `expired` so a timeout is never a hand-assembled fault at a call site.
- Growth: a new evidence axis follows the C# shape at the codec landing; this fold inherits it field-for-field with zero edits.
- Boundary: `Hops`, `FaultDetail`, and the enricher Layer are `codec#LANDING_WIRE`'s owners; the `Code` and `ConnectError` spellings are the `@connectrpc/connect` surface.
- Packages: `@connectrpc/connect` (`Code`, `ConnectError`); `@bufbuild/protobuf` (`isMessage`); `effect` (`Array`, `Duration`, `Option`, `Schema`, `pipe`); `./codec.ts` (`FaultDetail`, `Hops`); `./format.ts` (`Proto`).

```typescript signature
import type { Client, ContextValues, Interceptor, Transport as ConnectTransport } from "@connectrpc/connect"
import { Code, ConnectError, createClient, createContextKey, createContextValues } from "@connectrpc/connect"
import type { UniversalClientFn } from "@connectrpc/connect/protocol"
import { createTransport as createConnectHttpTransport } from "@connectrpc/connect/protocol-connect"
import { createConnectTransport, createGrpcWebTransport } from "@connectrpc/connect-web"
import { isMessage, type DescService, type MessageInitShape } from "@bufbuild/protobuf"
import { Headers, HttpClient, HttpClientRequest, MsgPack, Ndjson, Socket } from "@effect/platform"
import {
  Array,
  Cause,
  type Channel,
  type Chunk,
  Context,
  Data,
  Duration,
  Effect,
  ExecutionPlan,
  Exit,
  HashMap,
  Layer,
  Match,
  Metric,
  Option,
  type ParseResult,
  Predicate,
  Record,
  Runtime,
  Schema,
  Scope,
  Stream,
  Struct,
  pipe,
} from "effect"
import { Convention } from "../observe/convention.ts"
import type { Availability } from "../state/evidence.ts"
import { Hlc } from "../value/clock.ts"
import { type ContentKey, Digest } from "../value/contentKey.ts"
import { Budget } from "../value/fault.ts"
import { TenantContext } from "../value/identity.ts"
import { Ingress } from "../value/schema.ts"
import { Carrier } from "./carrier.ts"
import { FaultDetail, Hops, Parity, WireFault } from "./codec.ts"
import { Proto } from "./format.ts"

const _edge = (reason: Hops.Reason): FaultDetail.Hop =>
  new FaultDetail.Hop({ site: "<local-edge>", reason, elapsed: Duration.zero })

const Transport: {
  readonly expired: (surface: string, detail: string) => FaultDetail
  readonly fault: (caught: unknown) => FaultDetail
} = {
  expired: (surface, detail) =>
    new FaultDetail({ reason: "deadline", surface, detail, hops: [_edge("deadline")], tenant: Option.none() }),
  fault: (caught) =>
    pipe(ConnectError.from(caught, Code.Unknown), (error) =>
      Option.match(
        pipe(
          Array.findFirst(error.findDetails(Proto.registry), (detail) => isMessage(detail, Proto.suite.FaultDetailWire)),
          Option.flatMap((wire) => Option.getRight(Schema.decodeUnknownEither(FaultDetail.FromWire)(wire))),
        ),
        {
          onNone: () =>
            new FaultDetail({
              reason: Hops.fromCode(error.code),
              surface: "<transport>",
              detail: error.rawMessage,
              hops: [_edge(Hops.fromCode(error.code))],
              tenant: Option.none(),
            }),
          onSome: (detail) => new FaultDetail({ ...detail, hops: [...detail.hops, _edge(Hops.fromCode(error.code))] }),
        },
      )),
}
```

## [03]-[DIAL_AXIS]

- Owner: `Dial`, the invocation service — `Dial.Config` is the Schema the composition root decodes (`lanes` as a non-empty, protocol-unique ordered row set carrying each lane's attempt count, `baseUrl`, `useBinaryFormat`, and `timeoutMs`); the Layer factory takes decoded policy and the root's `fetch`/`interceptors` seam, constructs every `Transport` once, and exposes the lane-keyed `client(service)` record, the `ExecutionPlan`, and the `unary`/`stream` lifts.
- Law: three lanes, one policy shape — `connect` and `grpc-web` ride the `@connectrpc/connect-web` fetch factories with the instrumented `fetch` and shared `Interceptor` chain, and `connect-http` is the Effect-native lane: `./protocol-connect` `createTransport` over a `UniversalClientFn` whose body runs the platform `HttpClient`, so the dense lane inherits the net-client retry/proxy/pooling/tracing posture directly and its read/write ceilings are the `Ingress` decode budget, never fresh literals. Semver-internal `./protocol` stays admitted under the exact catalog pin, and the pin owns drift.
- Law: the universal client is the plane's one promise-shaped platform seam — the captured `Runtime` re-enters the rail inside `_universal` and nowhere else, the response `Scope` is held open exactly as long as the body iterable drains, and the request's `AbortSignal` crosses into the run so connect's own deadline wiring interrupts the platform call.
- Law: failover is one plan value — the primary lane engages first, later lanes engage only while the reconstructed fault's own `retryable` projection holds, each lane under the `_LADDER` attempt rows; a `catchAll` re-dial cascade is the deleted spelling, and a fourth protocol is one `_lanes` row and one config literal.
- Law: interruption crosses with the call — both promise lifts receive `Effect.tryPromise`'s fiber-wired `AbortSignal`; unary calls pass it to `Transport.unary`, streaming opens pass it to `Transport.stream`, and the returned iterable remains owned by `Stream.fromAsyncIterable`, whose finalization closes the iterator on downstream interruption.
- Law: per-call context is typed end to end — `Dial.Context` mints the plane's `ContextKey` vocabulary (`tenant`, `stamp`) once, `Dial.Ambient` is the `Context.Reference` carrying the fiber's tenancy and clock stamp (defaulting to `Option.none`, pinned by `Layer.succeed` at the root or a scoped `Effect.provideService` around one call), every lift charges a fresh `ContextValues` carrier from that reference before handing it to `CallOptions.contextValues`, and the root-supplied interceptors read and write the same keys instead of ambient state, so tenant and clock context cross the onion under one spelling. Trace identity and tenancy share the carrier rail — `Carrier.current` lifts the fiber's current span, `Carrier.promote` seats the ambient tenant when present, and `Carrier.inject("connect", ...)` prints the complete W3C triple into per-call headers for every lane; absence of either context axis omits only its headers. An interceptor attaching a typed protobuf metadata family (`TenantContextWire`, `HlcStampWire`) spells it through `Carrier.bin` on the `-bin` name rows, never a hand `encodeBinaryHeader` call at a lane. Transport factories own the onion — every lane row hands the root's `Interceptor` chain to its factory options, and `applyInterceptors` remains the kit spelling solely for a hand-assembled lane; re-wrapping calls outside the factories re-implements what the options row already carries.
- Law: transports construct once at the Layer — per-call construction re-mints connection state and defeats interceptor identity; the service's scoped life owns every lane.
- Growth: a per-call context axis is one `Dial.Context` key and its `Dial.Ambient` field; a lane-policy axis (per-lane attempt bound, a lane gate) is one field on the lane row.
- Boundary: the factory option records are the `@connectrpc/connect-web` surface and `CommonTransportOptions` the `./protocol` surface; a lane needing XHR upload progress bypasses this axis for the platform XHR client, stated at the consumer; the root's instrumented `fetch` construction is the runtime wave's composition.
- Packages: `@connectrpc/connect` (`createClient`, `Client`, `Interceptor`, `Transport`, `createContextKey`, `createContextValues`); `@connectrpc/connect/protocol` (`UniversalClientFn`); `@connectrpc/connect/protocol-connect` (`createTransport`); `@connectrpc/connect-web` (`createConnectTransport`, `createGrpcWebTransport`); `@bufbuild/protobuf` (`DescService`); `@effect/platform` (`Headers`, `HttpClient`, `HttpClientRequest`); `effect` (`Context`, `Effect`, `ExecutionPlan`, `Exit`, `Layer`, `Record`, `Runtime`, `Scope`, `Stream`); `./carrier.ts` (`Carrier`); `../value/clock.ts` (`Hlc`); `../value/identity.ts` (`TenantContext`); `../value/schema.ts` (`Ingress`).

```typescript signature
const _protocols = ["connect", "connect-http", "grpc-web"] as const

const _LanePolicy = Schema.Struct({
  attempts: Schema.Int.pipe(Schema.positive()),
  protocol: Schema.Literal(..._protocols),
})

const _DialConfig = Schema.Struct({
  lanes: Schema.NonEmptyArray(_LanePolicy).pipe(
    Schema.filter(
      (lanes) => Array.dedupe(Array.map(lanes, (lane) => lane.protocol)).length === lanes.length || "<duplicate-protocol-lane>",
      { identifier: "DistinctProtocolLanes" },
    ),
  ),
  baseUrl: Schema.NonEmptyString,
  useBinaryFormat: Schema.optionalWith(Schema.Boolean, { default: () => true }),
  timeoutMs: Schema.Int.pipe(Schema.between(1, 120000)),
})

declare namespace Dial {
  type Protocol = (typeof _protocols)[number]
  type Config = Schema.Schema.Type<typeof _DialConfig>
  type Seam = { readonly fetch: typeof globalThis.fetch; readonly interceptors: ReadonlyArray<Interceptor> }
  type Gear = { readonly config: Config; readonly seam: Seam; readonly universal: UniversalClientFn }
  type _Rows<T extends Record<Protocol, (gear: Gear) => ConnectTransport> = typeof _lanes> = T
}

const _universal = (client: HttpClient.HttpClient, runtime: Runtime.Runtime<never>): UniversalClientFn =>
  (request) => // BOUNDARY ADAPTER: connect's UniversalClientFn is promise-shaped — the captured runtime re-enters the rail here and nowhere else
    Runtime.runPromise(runtime)(
      Effect.gen(function* () {
        const scope = yield* Scope.make()
        const opened = HttpClientRequest.post(request.url, { headers: Headers.fromInput(request.header) })
        const response = yield* Scope.extend(
          client.execute(
            request.body === undefined
              ? opened
              : HttpClientRequest.bodyStream(opened, Stream.fromAsyncIterable(request.body, (defect) => defect)),
          ),
          scope,
        ).pipe(Effect.onError((cause) => Scope.close(scope, Exit.failCause(cause))))
        return {
          status: response.status,
          header: new globalThis.Headers(Object.entries(response.headers)),
          body: Stream.toAsyncIterableRuntime(Stream.ensuring(response.stream, Scope.close(scope, Exit.void)), runtime),
          trailer: new globalThis.Headers(),
        }
      }),
      { signal: request.signal },
    )

const _lanes = {
  connect: ({ config, seam }: Dial.Gear): ConnectTransport =>
    createConnectTransport({
      baseUrl: config.baseUrl,
      useBinaryFormat: config.useBinaryFormat,
      defaultTimeoutMs: config.timeoutMs,
      fetch: seam.fetch,
      interceptors: [...seam.interceptors],
    }),
  "connect-http": ({ config, seam, universal }: Dial.Gear): ConnectTransport =>
    createConnectHttpTransport({
      httpClient: universal,
      baseUrl: config.baseUrl,
      useBinaryFormat: config.useBinaryFormat,
      defaultTimeoutMs: config.timeoutMs,
      interceptors: [...seam.interceptors],
      acceptCompression: [],
      sendCompression: null,
      compressMinBytes: 1024,
      readMaxBytes: Ingress.floor.maxAssembledBytes, // the decode ceiling is the Ingress budget, never a fresh literal
      writeMaxBytes: Ingress.floor.maxAssembledBytes,
    }),
  "grpc-web": ({ config, seam }: Dial.Gear): ConnectTransport =>
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

const _CONTEXT = {
  stamp: createContextKey<Option.Option<Hlc>>(Option.none()),
  tenant: createContextKey<Option.Option<TenantContext>>(Option.none()),
} as const

class Ambient extends Context.Reference<Ambient>()("@rasm/ts/core/Dial/Ambient", {
  defaultValue: (): { readonly stamp: Option.Option<Hlc>; readonly tenant: Option.Option<TenantContext> } =>
    ({ stamp: Option.none(), tenant: Option.none() }),
}) {}

const _charged: Effect.Effect<ContextValues> = Effect.map(Ambient, (ambient) =>
  createContextValues().set(_CONTEXT.tenant, ambient.tenant).set(_CONTEXT.stamp, ambient.stamp))

const _stamped: Effect.Effect<Headers.Headers> = Effect.map(
  Effect.all({ ambient: Ambient, context: Carrier.current }),
  ({ ambient, context }) =>
    Carrier.inject(
      "connect",
      Option.match(ambient.tenant, {
        onNone: () => context,
        onSome: (tenant) => Carrier.promote(context, tenant),
      }),
      Headers.empty,
    ),
)

const _unary = <O>(
  call: (options: { readonly contextValues: ContextValues; readonly headers: Headers.Headers; readonly signal: AbortSignal }) => Promise<O>,
): Effect.Effect<O, FaultDetail> =>
  Effect.flatMap(Effect.all({ context: _charged, headers: _stamped }), ({ context, headers }) =>
    Effect.tryPromise({ try: (signal) => call({ contextValues: context, headers, signal }), catch: Transport.fault }))

const _streamed = <O>(
  open: (options: { readonly contextValues: ContextValues; readonly headers: Headers.Headers; readonly signal: AbortSignal }) => Promise<AsyncIterable<O>>,
): Stream.Stream<O, FaultDetail> =>
  Stream.unwrap(
    Effect.flatMap(Effect.all({ context: _charged, headers: _stamped }), ({ context, headers }) =>
      Effect.map(
        Effect.tryPromise({ try: (signal) => open({ contextValues: context, headers, signal }), catch: Transport.fault }),
        (messages) => Stream.fromAsyncIterable(messages, Transport.fault),
      )),
  )

class Dial extends Effect.Service<Dial>()("@rasm/ts/core/Dial", {
  effect: (config: Dial.Config, seam: Dial.Seam) =>
    Effect.gen(function* () {
      const client = yield* HttpClient.HttpClient
      const runtime = yield* Effect.runtime<never>()
      const transports = Record.map(_lanes, (make) => make({ config, seam, universal: _universal(client, runtime) }))
      const ladder = Array.map(config.lanes, (row) => ({
        provide: Layer.succeed(Lane, { protocol: row.protocol, transport: transports[row.protocol] }),
        attempts: row.attempts,
        while: (fault: FaultDetail) => fault.retryable,
      }))
      return {
        plan: ExecutionPlan.make(Array.headNonEmpty(ladder), ...Array.tailNonEmpty(ladder)),
        client: <T extends DescService>(service: T): Readonly<Record<Dial.Protocol, Client<T>>> =>
          Record.map(transports, (transport) => createClient(service, transport)),
        unary: _unary,
        stream: _streamed,
      }
    }),
}) {
  static readonly Ambient: typeof Ambient = Ambient
  static readonly Config: typeof _DialConfig = _DialConfig
  static readonly Context: typeof _CONTEXT = _CONTEXT
  static readonly Lane: typeof Lane = Lane
}
```

## [04]-[CAPABILITY_BIND]

- Owner: `Capability` — `CapabilityDescriptor`, the decoded capability identity with parity admission; `Sdk<T>`, the mapped Effect/Stream projection of `Client<T>`; and `bind`, the method-kind fold that calls the selected lane's typed `Transport.unary`/`Transport.stream` directly from each `DescMethod`, preserving descriptor/input/output correspondence without a dynamic client-member assertion, then applies per-method `Budget` geometry and the plane's observability transformers.
- Law: content-keyed admission — the key covers the command shape's canonical bytes, branded-key equality is bare `===` under the one mint, `bind(service, source)` admits the descriptor bytes and proves `descriptor.service === service.typeName` before constructing a method, and a diverging key or service refuses through the typed wire-fault rail because a capability whose command shape moved is a different capability.
- Law: the SDK is a descriptor, never a hand-written client — `Sdk<T>` maps `Client<T>`'s own member types into Effect carriers, the runtime record builds from the descriptor's own `methods` walk, and the two derive from one descriptor; a parallel interface per capability is the second-truth defect.
- Law: the derivation is kind-total over the shipped axis through `Match.discriminatorsExhaustive("methodKind")` — `unary` and `server_streaming` bind; `client_streaming` and `bidi_streaming` refuse at bind time as `drift` evidence because the C# emitter does not mint them; a fifth method kind breaks the fold loudly at compile time, and silence over an unbindable method strands its caller at runtime — the catch-all ternary that absorbs it is the deleted spelling.
- Law: budget geometry is the rails layering law realized — a method with a budget row composes `Effect.timeoutFail(Budget[kind].attempt)` below `Effect.retry(Budget.schedule(kind))` and `Effect.timeoutFail(Budget[kind].total)` above it, both deadlines minting through `Transport.expired` so the whole-call and per-try budgets live in the row and the transport `timeoutMs` is only the lane-level floor; the schedule is class-gated through the fault's own classification so a terminal reason never re-drives, and a method without a row never retries — the safe default for non-idempotent verbs. A streaming method with a budget row composes `Stream.retry(Budget.schedule(kind))` — re-registration semantics, the resume coordinate living in the source.
- Law: every bound method runs under the failover plan and inside the plane's telemetry transformers — `Effect.withExecutionPlan` attaches the `Dial` ladder on the unary lane and `Stream.withExecutionPlan` with `preventFallbackOnPartialStream` attaches it on the streaming lane, so a retryable fault walks the ladder mid-stream included, a partially-emitted feed never re-emits from a fresh lane — the budget `Stream.retry` above the plan owns resumption because the resume coordinate lives in the source — and the winning lane stamps the open span as the `invokeLane` row per engaged lane, so lane choice is a span dimension, never a call-site fact; `_observed` stacks the latency timer, the fault-reason frequency, the `Exit`-folded outcome counter, the span, and the log annotation on the unary lane, and `_observedStream` stacks the fault frequency, the same outcome counter over the stream's own scope exit through `Stream.ensuringWith`, and the span on the streaming lane — the tag records type `Convention.Attributes` so a key outside the vocabulary cannot ride a span or log, names and tag keys are `observe/convention` rows, and the outcome dimension is the interrupt-first `Exit` fold — so the branch's hottest invocation surface is traced and measured by construction on both modalities.
- Law: the descriptor walk seals through `_sdk(service)` — its runtime predicate proves every descriptor method has a constructed function before `.make` returns the mapped `Sdk<T>`; no assertion, non-null pin, or client-member index survives the assembly boundary.
- Growth: a new method appears in the SDK at regeneration with zero edits here; a method gaining idempotency is one budget row at the caller; a new outcome dimension widens the `Exit` fold's anchored union, never an arm.
- Boundary: the emitted `DescService` consts are build artifacts the app's capability modules import — the composition root hands the same consts to the contract gate and sequences `DescriptorGate.admitted("CapabilityDescriptorWire")` ahead of `bind`, so RPC method drift refuses before any client pins; the `CapabilityDescriptorWire` census row homes here; `Budget` rows are `value/fault.ts` vocabulary; the instrument names are `observe/convention` rows — the interchange plane's one import of the vocabulary spine.
- Packages: `@connectrpc/connect` (`Client`); `@bufbuild/protobuf` (`DescService`); `effect` (`Cause`, `Effect`, `Exit`, `Match`, `Metric`, `Option`, `Schema`, `Stream`, `pipe`); `./codec.ts` (`Parity`, `WireFault`); `./format.ts` (`Proto`); `../observe/convention.ts` (`Convention`); `../value/contentKey.ts` (`ContentKey`, `Digest`); `../value/fault.ts` (`Budget`).

```typescript signature
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
  type Outcome = "halted" | "crashed" | "resolved" | `rejected:${FaultDetail["reason"]}` // anchored on the Hops reason axis: a new reason widens it here
  type Sdk<T extends DescService> = {
    readonly [K in keyof Client<T>]: Client<T>[K] extends (input: infer I, options?: infer _O) => Promise<infer O>
      ? (input: I) => Effect.Effect<O, FaultDetail>
      : Client<T>[K] extends (input: infer I, options?: infer _O) => AsyncIterable<infer O>
        ? (input: I) => Stream.Stream<O, FaultDetail>
        : never
  }
  type Budgets = Readonly<Record<string, Budget.Kind>>
  type Source = { readonly descriptor: Uint8Array; readonly pinned: ContentKey; readonly budgets?: Budgets }
}

const _one = async function* <A>(value: A): AsyncIterable<A> {
  yield value
}

const _sdk = <T extends DescService>(service: T): Schema.Schema<Capability.Sdk<T>> =>
  Schema.declare(
    (input: unknown): input is Capability.Sdk<T> =>
      Predicate.isRecord(input)
      && Array.every(service.methods, (method) => Predicate.isFunction(input[method.localName])),
    { identifier: `${service.typeName}Sdk` },
  )

const _BOUNDS = [25, 100, 400, 1600, 6400] as const            // latency ladder in millis: x4 per rung across the budget attempt ceilings

const _calls = Metric.counter(Convention.metric.invokeCalls, { incremental: true })
const _faults = Metric.frequency(Convention.metric.invokeFault)
const _clock = Metric.timerWithBoundaries(Convention.metric.invokeDuration, [..._BOUNDS])

const _outcome: (exit: Exit.Exit<unknown, FaultDetail>) => Capability.Outcome = Exit.match({
  onFailure: (cause) =>
    Cause.isInterruptedOnly(cause)
      ? ("halted" as const)
      : Option.match(Cause.failureOption(cause), {
          onNone: () => "crashed" as const,
          onSome: (fault) => `rejected:${fault.reason}` as const,
        }),
  onSuccess: () => "resolved" as const,
})

const _observed = (span: string, tags: Convention.Attributes) =>
  <A, R>(self: Effect.Effect<A, FaultDetail, R>): Effect.Effect<A, FaultDetail, R> =>
    self.pipe(
      Metric.trackDuration(_clock),
      Metric.trackErrorWith(_faults, (fault: FaultDetail) => fault.reason),
      Effect.onExit((exit) => Metric.increment(Metric.tagged(_calls, Convention.rasm.invokeOutcome, _outcome(exit)))),
      Effect.withSpan(span, { attributes: tags }),
      Effect.annotateLogs(tags),
    )

const _observedStream = (span: string, tags: Convention.Attributes) =>
  <A, R>(self: Stream.Stream<A, FaultDetail, R>): Stream.Stream<A, FaultDetail, R> =>
    self.pipe(
      Stream.tapError((fault) => Metric.update(_faults, fault.reason)),
      Stream.ensuringWith((exit) => Metric.increment(Metric.tagged(_calls, Convention.rasm.invokeOutcome, _outcome(exit)))),
      Stream.withSpan(span, { attributes: tags }),
    )

const _unbindable = (kind: string, name: string): WireFault =>
  new WireFault({
    family: "CapabilityDescriptorWire",
    reason: "drift",
    detail: `<unbindable-kind:${kind}:${name}>`,
    evidence: Option.none(),
  })

const _descriptorMismatch = (descriptor: CapabilityDescriptor, service: DescService): WireFault =>
  new WireFault({
    family: "CapabilityDescriptorWire",
    reason: "drift",
    detail: `<service:${descriptor.service}->${service.typeName}>`,
    evidence: Option.none(),
  })

const Capability: {
  readonly Descriptor: typeof CapabilityDescriptor
  readonly bind: <T extends DescService>(service: T, source: Capability.Source) => Effect.Effect<Capability.Sdk<T>, ParseResult.ParseError | WireFault, Dial>
} = {
  Descriptor: CapabilityDescriptor,
  bind: <T extends DescService>(service: T, source: Capability.Source) =>
    Effect.gen(function* () {
      const descriptor = yield* CapabilityDescriptor.admit(source.descriptor, source.pinned)
      yield* Effect.filterOrFail(
        Effect.succeed(descriptor),
        (admitted) => admitted.service === service.typeName,
        () => _descriptorMismatch(descriptor, service),
      )
      const dial = yield* Dial
      const rows = yield* Effect.forEach(service.methods, (method) =>
        Match.value(method).pipe(
          Match.discriminatorsExhaustive("methodKind")({
            unary: (unary) =>
              Effect.succeed([
                unary.localName,
                (input: MessageInitShape<typeof unary.input>) =>
                  pipe(
                    Effect.withExecutionPlan(
                      Effect.flatMap(Lane, (lane) =>
                        Effect.zipRight(
                          Effect.annotateCurrentSpan(Convention.rasm.invokeLane, lane.protocol),
                          dial.unary((options) =>
                            lane.transport.unary(
                              unary,
                              options.signal,
                              undefined,
                              options.headers,
                              input,
                              options.contextValues,
                            ).then((response) => response.message)),
                        )),
                      dial.plan,
                    ),
                    (call) =>
                      Option.match(Option.fromNullable(source.budgets?.[unary.localName]), {
                        onNone: () => call,
                        onSome: (kind) =>
                          call.pipe(
                            Effect.timeoutFail({ duration: Budget[kind].attempt, onTimeout: () => Transport.expired(unary.localName, "<attempt-budget>") }),
                            Effect.retry(Budget.schedule(kind)),
                            Effect.timeoutFail({ duration: Budget[kind].total, onTimeout: () => Transport.expired(unary.localName, "<total-budget>") }),
                          ),
                      }),
                    _observed(`invoke/${service.typeName}/${unary.localName}`, {
                      [Convention.rasm.invokeMethod]: unary.localName,
                      [Convention.rasm.invokeService]: service.typeName,
                    }),
                  ),
              ] as const),
            server_streaming: (streaming) =>
              Effect.succeed([
                streaming.localName,
                (input: MessageInitShape<typeof streaming.input>) =>
                  pipe(
                    Stream.unwrap(
                      Effect.flatMap(Lane, (lane) =>
                        Effect.as(
                          Effect.annotateCurrentSpan(Convention.rasm.invokeLane, lane.protocol),
                          dial.stream((options) =>
                            lane.transport.stream(
                              streaming,
                              options.signal,
                              undefined,
                              options.headers,
                              _one(input),
                              options.contextValues,
                            ).then((response) => response.message)),
                        )),
                    ),
                    Stream.withExecutionPlan(dial.plan, { preventFallbackOnPartialStream: true }), // the plan governs the LIVE feed: a mid-stream fault walks the lane ladder, and a partially-emitted feed never re-emits from a fresh lane
                    (feed) =>
                      Option.match(Option.fromNullable(source.budgets?.[streaming.localName]), {
                        onNone: () => feed,
                        onSome: (kind) => Stream.retry(feed, Budget.schedule(kind)), // re-registration: the resume coordinate lives in the source
                      }),
                    _observedStream(`invoke/${service.typeName}/${streaming.localName}`, {
                      [Convention.rasm.invokeMethod]: streaming.localName,
                      [Convention.rasm.invokeService]: service.typeName,
                    }),
                  ),
              ] as const),
            client_streaming: (refused) => Effect.fail(_unbindable(refused.methodKind, refused.localName)),
            bidi_streaming: (refused) => Effect.fail(_unbindable(refused.methodKind, refused.localName)),
          }),
        ))
      return _sdk(service).make(Object.fromEntries(rows))
    }),
}
```

## [05]-[COMMAND_GATEWAY]

- Owner: `Gateway`, the inbound half of the capability plane — `CommandPayload`, the decoded command class (verb, body carriage, tenant, `Hlc` stamp); `Gateway.Row`, the verb-row contract correlating one verb's body schema, receipt schema, and handler; `Gateway.make`, the one generic fold over the app's verb-row table yielding the built dispatch surface — `submit`, the derived `outbound` receipt-union schema, and `duplex`; `Dispatched<A>`, the Granted/Refused outcome family carrying the typed receipt and the `state` verdict whole; `AvailabilityGate`, the port this page declares and the app root satisfies from `state`-fed evidence; and `SupportCapture` with its receipt and the `SupportIntake` port as the support verb's delivery.
- Law: the table is the contract — the fold is reverse-mapped over three correlated maps (per-verb body type, receipt type, receipt wire type), so a row's handler receives exactly the body its own schema decoded and returns exactly the receipt its own schema declares, the handler census is total over the verb vocabulary because the table IS the vocabulary, and the body band decodes through the row's schema inside the fold — the second-admission law enforced structurally, never by handler discipline. An unknown verb refuses as `drift` evidence through an explicit `Effect.fail`, because the shell and the app were built against one verb set; the verb string itself stays the C#-minted wire spelling on `CommandPayload`, and the typed verb set lives on the table.
- Law: the gateway's shape is app-parameterized, so the capability arrives as a constructor, not a Tag — `Gateway.make(rows)` is an Effect requiring `AvailabilityGate`, and the app root wraps the built value in its own Layer against its own Tag; core owns the fold, the contract, and the frame vocabulary, never the serving edge's service identity — one table type per app is a fact no fixed core Tag can carry without erasing the receipt union back to `unknown`.
- Law: `submit` is an `Effect.fn` definition seam — the span opens per dispatch, the gateway timer and the outcome counter ride the declaration tail with names from `observe/convention` rows, the verb stamps the current span, and the outcome tag is the `Exit`-folded `Gateway.Emission` union: the `Dispatched` tags by derivation, `rejected:` keyed by the `WireFault` reason axis, `invalid` for decode skew on the envelope or the body band, `halted`/`crashed` from the interrupt-first cause fold — so every dispatch lands in the counter exactly once, faulted dispatches included, and inbound telemetry policy is recoverable from the declaration.
- Law: the gate types against `state` vocabulary, never re-declares it — the port answers `Availability.Verdict`, refusal transports the verdict whole (`Gated` keeps its `until`, `Withheld` its level), and gating is read-then-dispatch: staleness policy belongs to the providing Layer.
- Law: the support verb is one row on the same plane — the evidence band crosses opaque, interpretation belongs to the intake's consumer, and the port is declared here and satisfied at the root so the observe unit and this plane stay ledger-clean; the receipt travels back to the reporter so a support report is never fire-and-forget, and the delivered capture is the branch's `rasm.core.interchange.support` tap point — the `observe/tap` name row a subscription targets.
- Law: the duplex channel derives from the same contract — `outbound` is the `Schema.Union` over the table's own receipt schemas, `duplex(socket, frame)` takes no caller-supplied schema, and the frame row is a `_frames` vocabulary lookup over the fused transformers — `MsgPack.duplexSchema`/`Ndjson.duplexSchema`/`Ndjson.duplexSchemaString` collapse the frame codec and the asymmetric schema pair (commands inbound, receipts outbound, backpressure carried) into one channel transformer, and each row owns the socket lift its frame demands — `Socket.toChannelWith` under the byte frames, `Socket.toChannelString` under the text frame, because the string transformer types a `Chunk<string>` channel a byte lift can never satisfy — so the two-stage frame-then-`ChannelSchema` sandwich, the frame ternary, and the free outbound schema are all deleted spellings; refusal delivery over a wire is the serving edge's own outcome spelling over these values.
- Law: the duplex lives under one scoped span — `duplex` is a scoped acquisition: `Effect.makeSpanScoped` opens `gateway/duplex` with the frame row stamped as the `Convention.rasm.gatewayFrame` attribute and the channel returns beside it, so the span ends with the serving scope that acquired it — the long-lived correlation anchor profile links and tap facts annotate while per-dispatch `submit` spans stay the request-grain trace; the serving edge acquires the channel inside its own socket scope, so no second boot edge and no hand span pair exists.
- Law: the msgpack row is the command lane's standing frame — the `Hlc` halves are `bigint` and JSON owns no bigint spelling, so both ndjson rows are legal only over JSON-safe encoded schemas on both directions; `ndjson-text` is `Ndjson.duplexSchemaString` over the string channel, the text-frame lane for text-only transports, under the same JSON-safety law — a serving edge that must frame commands as text lands its JSON stamp spelling first, and swapping a row under a bigint-carrying receipt is the precision defect the frame discriminant cannot absorb.
- Law: the verb correlation seals before key erasure — `_make` compiles each mapped row into one uniform closure that captures its body decoder and handler together; the runtime table stores those closures, so lookup needs neither an assertion nor a non-null pin and cannot pair one verb's decoder with another verb's handler.
- Growth: a new verb is one row in the app's table — body schema, receipt schema, handler — with the outbound union and the emission counter inheriting it; a new outcome kind is one tagged case every exhaustive consumer breaks on; a fourth frame row is one `_frames` row.
- Boundary: the `CommandPayloadWire`/`SupportCaptureWire` census rows home here; the availability vocabulary and the total `admits` fallback are `state/evidence.ts`'s; the socket Layer and the serving loop are the runtime wave's.
- Packages: `@effect/platform` (`MsgPack`, `Ndjson`, `Socket`); `effect` (`Cause`, `Context`, `Data`, `Effect`, `Exit`, `HashMap`, `Metric`, `Option`, `Schema`, `Scope`, `Struct`); `./codec.ts` (`WireFault`); `./format.ts` (`Proto`); `../observe/convention.ts` (`Convention`); `../value/clock.ts` (`Hlc`); `../value/identity.ts` (`TenantContext`); `../state/evidence.ts` (`Availability`).

```typescript signature
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

type Dispatched<A> = Data.TaggedEnum<{
  Granted: { readonly verb: string; readonly receipt: A }
  Refused: { readonly verb: string; readonly verdict: Availability.Verdict }
}>
interface DispatchedDefinition extends Data.TaggedEnum.WithGenerics<1> {
  readonly taggedEnum: Dispatched<this["A"]>
}
const _Dispatched = Data.taggedEnum<DispatchedDefinition>() // interior constructor: the annotation gate reaches the merged name, so only the type exports

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
    Effect.gen(function* () {
      const report = yield* Schema.decodeUnknown(SupportCapture.FromBytes)(octets)
      const intake = yield* SupportIntake
      return yield* intake.deliver(report)
    })
}

class SupportIntake extends Context.Tag("@rasm/ts/core/SupportIntake")<SupportIntake, {
  readonly deliver: (report: SupportCapture) => Effect.Effect<SupportReceipt>
}>() {}

declare namespace Gateway {
  type Row<B, A, I> = {
    readonly body: Schema.Schema<B, unknown>
    readonly receipt: Schema.Schema<A, I>
    readonly handle: (payload: CommandPayload, body: B) => Effect.Effect<A, WireFault>
  }
  type _Compiled<A, I> = {
    readonly dispatch: (payload: CommandPayload) => Effect.Effect<A, ParseResult.ParseError | WireFault>
    readonly receipt: Schema.Schema<A, I>
  }
  type Emission = "halted" | "crashed" | "invalid" | Dispatched<unknown>["_tag"] | `rejected:${WireFault["reason"]}` // anchored on the family tags and the codec reason axis: a new reason widens it here
  type Frame = keyof typeof _frames
  type Duplex<A> = Channel.Channel<
    Chunk.Chunk<CommandPayload>,
    Chunk.Chunk<A>,
    MsgPack.MsgPackError | Ndjson.NdjsonError | ParseResult.ParseError | Socket.SocketError,
    ParseResult.ParseError,
    void,
    unknown
  >
  type Shape<A, I> = {
    readonly duplex: (socket: Socket.Socket, frame: Frame) => Effect.Effect<Duplex<A>, never, Scope.Scope>
    readonly outbound: Schema.Schema<A, I>
    readonly submit: (octets: Uint8Array) => Effect.Effect<Dispatched<A>, ParseResult.ParseError | WireFault>
  }
}

const _frames = {
  msgpack: <A, I>(outcome: Schema.Schema<A, I>) => (socket: Socket.Socket): Gateway.Duplex<A> =>
    MsgPack.duplexSchema({ inputSchema: outcome, outputSchema: CommandPayload })(
      Socket.toChannelWith<MsgPack.MsgPackError | ParseResult.ParseError>()(socket),
    ),
  ndjson: <A, I>(outcome: Schema.Schema<A, I>) => (socket: Socket.Socket): Gateway.Duplex<A> =>
    Ndjson.duplexSchema({ inputSchema: outcome, outputSchema: CommandPayload })(
      Socket.toChannelWith<Ndjson.NdjsonError | ParseResult.ParseError>()(socket),
    ),
  "ndjson-text": <A, I>(outcome: Schema.Schema<A, I>) => (socket: Socket.Socket): Gateway.Duplex<A> =>
    Ndjson.duplexSchemaString({ inputSchema: outcome, outputSchema: CommandPayload })(
      Socket.toChannelString()<Ndjson.NdjsonError | ParseResult.ParseError>(socket), // the text frame types a string channel: the lift is the row's own, never the byte lift
    ),
} as const

const _gatewayClock = Metric.timerWithBoundaries(Convention.metric.gatewayDuration, [..._BOUNDS])
const _gatewayCommands = Metric.counter(Convention.metric.gatewayCommands, { incremental: true })

const _emitted = <A>(exit: Exit.Exit<Dispatched<A>, ParseResult.ParseError | WireFault>): Gateway.Emission =>
  Exit.match(exit, {
    onFailure: (cause) =>
      Cause.isInterruptedOnly(cause)
        ? ("halted" as const)
        : Option.match(Cause.failureOption(cause), {
            onNone: () => "crashed" as const,
            onSome: (fault) => (fault._tag === "WireFault" ? (`rejected:${fault.reason}` as const) : ("invalid" as const)),
          }),
    onSuccess: (outcome) => outcome._tag,
  })

const _make = <
  B extends Record<string, unknown>,
  A extends { readonly [K in keyof B]: unknown },
  I extends { readonly [K in keyof B]: unknown },
>(rows: { readonly [K in keyof B]: Gateway.Row<B[K], A[K], I[K]> }): Effect.Effect<
  Gateway.Shape<A[keyof B], I[keyof B]>,
  never,
  AvailabilityGate
> =>
  Effect.map(AvailabilityGate, (gate) => {
    const table: HashMap.HashMap<string, Gateway._Compiled<A[keyof B], I[keyof B]>> = HashMap.fromIterable(
      Array.map(Struct.keys(rows), (verb) => {
        const row = rows[verb]
        const compiled: Gateway._Compiled<A[keyof B], I[keyof B]> = {
          dispatch: (payload) => Effect.flatMap(Schema.decodeUnknown(row.body)(payload.body), (body) => row.handle(payload, body)),
          receipt: row.receipt,
        }
        return [verb, compiled] as const
      }),
    )
    const outbound: Schema.Schema<A[keyof B], I[keyof B]> = Schema.Union(
      ...Array.map(Struct.keys(rows), (verb) => rows[verb].receipt),
    ) // the duplex outbound IS the table's receipt column: no caller-supplied schema exists to disagree with dispatch
    return {
      outbound,
      duplex: (socket: Socket.Socket, frame: Gateway.Frame): Effect.Effect<Gateway.Duplex<A[keyof B]>, never, Scope.Scope> =>
        Effect.as(
          Effect.makeSpanScoped("gateway/duplex", { attributes: { [Convention.rasm.gatewayFrame]: frame } }),
          _frames[frame](outbound)(socket),
        ), // the scoped acquisition: the lifetime span ends with the serving scope, the channel rides beside it
      submit: Effect.fn("gateway.submit")(
        function* (octets: Uint8Array) {
          const payload = yield* Schema.decodeUnknown(CommandPayload.FromBytes)(octets)
          yield* Effect.annotateCurrentSpan(Convention.rasm.gatewayVerb, payload.verb)
          const row = yield* Option.match(HashMap.get(table, payload.verb), {
            onNone: () =>
              Effect.fail(
                new WireFault({
                  family: "CommandPayloadWire",
                  reason: "drift",
                  detail: `<unknown-verb:${payload.verb}>`,
                  evidence: Option.none(),
                }),
              ),
            onSome: Effect.succeed,
          })
          const verdict = yield* gate.admits(payload.verb)
          return yield* (verdict._tag === "Available"
            ? Effect.map(row.dispatch(payload), (receipt) => _Dispatched.Granted({ verb: payload.verb, receipt }))
            : Effect.succeed(_Dispatched.Refused({ verb: payload.verb, verdict })))
        },
        (effect) => Metric.trackDuration(effect, _gatewayClock),
        (effect) =>
          Effect.onExit(effect, (exit) =>
            Metric.increment(Metric.tagged(_gatewayCommands, Convention.rasm.gatewayOutcome, _emitted(exit)))),
      ),
    }
  })

const Gateway: {
  readonly Payload: typeof CommandPayload
  readonly make: typeof _make
} = {
  Payload: CommandPayload,
  make: _make,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { AvailabilityGate, Capability, Dial, Gateway, SupportCapture, SupportIntake, Transport }
export type { Dispatched }
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
