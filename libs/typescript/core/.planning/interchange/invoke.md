# [CORE_INVOKE]

The capability plane of the interchange: both directions of the command contract under one page. Outbound — `CapabilityDescriptor`, the content-keyed command-shape identity admitted once at bind time through the plane's parity gate; `Dial`, the invocation service whose protocol axis (`connect` | `connect-http` | `grpc-web`) is a Config-decoded policy record over three lane rows — the two `@connectrpc/connect-web` fetch factories plus the Effect-native `./protocol-connect` transport assembled over the platform `HttpClient`, so the dense lane inherits the shared net-client retry/proxy/pooling/tracing posture with no fetch hop — and whose lane ladder is one `ExecutionPlan` value so transport failover is a policy ladder, never a recovery cascade; the Effect lifts wire fiber interruption to the call's `AbortSignal`, stamp W3C trace headers per call, charge a typed `ContextValues` carrier per call from the `Dial.Ambient` reference under the plane's own `ContextKey` vocabulary, and fold every caught value through the total `ConnectError` reconstruction into the codec `FaultDetail`; retry rides the `value` `Budget` schedules class-gated through the fault's own classification, with the row's `attempt`/`total` deadlines layered below and above the retry per the rails budget geometry, and every bound method and gateway dispatch emits through the plane's observability transformers — the span with the winning lane stamped, the fault frequency, and the `Exit`-folded outcome counter on every modality, the latency timer on the unary and gateway lanes — named by `observe/convention` rows. Inbound — `Gateway`, the verb dispatch over decoded `CommandPayload` verbs under the availability gate typed against `state` evidence, with the support-capture verb as one row delivering to the `SupportIntake` port, and `duplex`, the typed command/outcome channel whose frame row is a vocabulary lookup over the fused codec transformers — `MsgPack.duplexSchema`, `Ndjson.duplexSchema`, `Ndjson.duplexSchemaString` — each row owning the socket lift its frame demands, one stage from socket to typed duplex under an unchanged asymmetric schema seam. The module is `core/src/interchange/invoke.ts`; a fourth protocol is one lane row, a new verb is one handler row in the app's table, and a per-method retry posture is one budget row at the bind call.

## [01]-[CLUSTERS]

| [INDEX] | [CLUSTER]         | [OWNS]                                                                                            | [PUBLIC]                   |
| :-----: | :---------------- | :------------------------------------------------------------------------------------------------ | :------------------------- |
|  [01]   | `TRANSPORT_FAULT` | the total `ConnectError` fold into the codec fault vocabulary                                     | `Transport`                |
|  [02]   | `DIAL_AXIS`       | the Config policy record, the three-lane table, the failover plan, the ambient context, the lifts | `Dial`                     |
|  [03]   | `CAPABILITY_BIND` | descriptor admission, the kind-total SDK derivation, budget geometry, the invoke telemetry seam   | `Capability`               |
|  [04]   | `COMMAND_GATEWAY` | verb dispatch, the availability gate, support capture, the duplex channel                         | `Gateway`, `SupportIntake` |

## [02]-[TRANSPORT_FAULT]

[TRANSPORT_FAULT]:
- Owner: `Transport`, the transport-fault fold — `fault(caught)` normalizes any caught value through `ConnectError.from`, maps the closed sixteen-value `Code` enum through the codec `Hops` projection, resolves every server-attached detail through `findDetails(Proto.registry)` so the whole registered detail vocabulary decodes in one call, reconstructs the codec `FaultDetail` from the `FaultDetailWire` member with the local edge hop appended, and `expired(surface, detail)` mints the locally-originated deadline fault the budget timeouts consume — so the error channel is one reconstructed wire fault end to end and no `ConnectError`, bare code, or raw `try`/`catch` reaches domain flow.
- Law: the fold is total — an `AbortError`/`TimeoutError` lands as `Canceled` through `ConnectError.from`, `Hops.fromCode` maps with `unknown` as the residue row, and a carried detail's hop chain merges ahead of the local edge hop so the first hop stays the origin; no ladder inspects codes.
- Law: detail resolution is registry-wide — `findDetails(Proto.registry)` decodes every `Any`-wrapped detail the format registry names, `isMessage(detail, Proto.suite.FaultDetailWire)` selects the fault member, and the carried detail decodes through `FaultDetail.FromWire` — the codec's tagged-landing twin — so the untagged proto detail reconstructs whole and a decode miss degrades to the edge-only fault, never a dropped call; a second registered detail family lands as one more `isMessage` probe over the same resolved set.
- Law: these are the second and last `FaultDetail` construction sites — the first is the codec registry's decode row; a third anywhere in the branch is the altitude defect the architecture suite audits, and the deadline mint rides `expired` so a timeout is never a hand-assembled fault at a call site.
- Growth: a new evidence axis follows the C# shape at the codec landing; this fold inherits it field-for-field with zero edits.
- Boundary: `Hops`, `FaultDetail`, and the enricher Layer are `codec#LANDING_WIRE`'s owners; the `Code` and `ConnectError` spellings are the `@connectrpc/connect` surface.
- Packages: `@connectrpc/connect` (`Code`, `ConnectError`); `@bufbuild/protobuf` (`isMessage`); `effect` (`Array`, `Duration`, `Option`, `Schema`, `pipe`); `./codec.ts` (`FaultDetail`, `Hops`); `./format.ts` (`Proto`).

```typescript
import { Code, ConnectError } from "@connectrpc/connect"
import { isMessage } from "@bufbuild/protobuf"
import { Array, Duration, Option, Schema, pipe } from "effect"
import { FaultDetail, Hops } from "./codec.ts"
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

[DIAL_AXIS]:
- Owner: `Dial`, the invocation service — `Dial.Config` is the Schema the composition root decodes (protocol lanes in failover order, `baseUrl`, `useBinaryFormat`, `timeoutMs`); the Layer factory takes the decoded policy plus the `fetch` and `interceptors` seam values the root builds, yields the platform `HttpClient` and the ambient runtime, constructs one `Transport` per configured lane by vocabulary lookup over the three-row lane table, and exposes `client(service)` (the lane-keyed prebuilt `createClient` record), `plan` (the `ExecutionPlan` ladder over the `Lane` Tag), and the two lifts `unary`/`stream`.
- Law: three lanes, one policy shape — `connect` and `grpc-web` ride the `@connectrpc/connect-web` fetch factories with the instrumented `fetch` and shared `Interceptor` chain, and `connect-http` is the Effect-native lane: `./protocol-connect` `createTransport` over a `UniversalClientFn` whose body runs the platform `HttpClient`, so the dense lane inherits the net-client retry/proxy/pooling/tracing posture directly and its read/write ceilings are the `Ingress` decode budget, never fresh literals. The `./protocol` surface is semver-internal under the exact catalog pin — the admission is deliberate and the pin owns drift.
- Law: the universal client is the plane's one promise-shaped platform seam — the captured `Runtime` re-enters the rail inside `_universal` and nowhere else, the response `Scope` is held open exactly as long as the body iterable drains, and the request's `AbortSignal` crosses into the run so connect's own deadline wiring interrupts the platform call.
- Law: failover is one plan value — the primary lane engages first, later lanes engage only while the reconstructed fault's own `retryable` projection holds, each lane under the `_LADDER` attempt rows; a `catchAll` re-dial cascade is the deleted spelling, and a fourth protocol is one `_lanes` row plus one config literal.
- Law: interruption crosses with the call — `Effect.tryPromise`'s evaluator receives the fiber-wired `AbortSignal` handed to `CallOptions.signal`, so a scope close or race loss aborts the in-flight RPC as `Canceled` and no controller is hand-managed.
- Law: per-call context is typed end to end — `Dial.Context` mints the plane's `ContextKey` vocabulary (`tenant`, `stamp`) once, `Dial.Ambient` is the `Context.Reference` carrying the fiber's tenancy and clock stamp (defaulting to `Option.none`, pinned by `Layer.succeed` at the root or a scoped `Effect.provideService` around one call), every lift charges a fresh `ContextValues` carrier from that reference before handing it to `CallOptions.contextValues`, and the root-supplied interceptors read and write the same keys instead of ambient state, so tenant and clock context cross the onion under one spelling; trace identity is per-call headers — the current span reads off the fiber, `HttpTraceContext.toHeaders` spells the W3C pair, absence of a span sends no header, and propagation is uniform across lanes because it rides `CallOptions`.
- Law: transports construct once at the Layer — per-call construction re-mints connection state and defeats interceptor identity; the service's scoped life owns every lane.
- Growth: a per-call context axis is one `Dial.Context` key plus its `Dial.Ambient` field; a lane-policy axis (per-lane attempt bound, a lane gate) is one field on the lane row.
- Boundary: the factory option records are the `@connectrpc/connect-web` surface and `CommonTransportOptions` the `./protocol` surface; a lane needing XHR upload progress bypasses this axis for the platform XHR client, stated at the consumer; the root's instrumented `fetch` construction is the runtime wave's composition.
- Packages: `@connectrpc/connect` (`createClient`, `Client`, `Interceptor`, `Transport`, `createContextKey`, `createContextValues`); `@connectrpc/connect/protocol` (`UniversalClientFn`); `@connectrpc/connect/protocol-connect` (`createTransport`); `@connectrpc/connect-web` (`createConnectTransport`, `createGrpcWebTransport`); `@bufbuild/protobuf` (`DescService`); `@effect/platform` (`Headers`, `HttpClient`, `HttpClientRequest`, `HttpTraceContext`); `effect` (`Context`, `Effect`, `ExecutionPlan`, `Exit`, `Layer`, `Record`, `Runtime`, `Scope`, `Stream`); `../value/clock.ts` (`Hlc`); `../value/identity.ts` (`TenantContext`); `../value/schema.ts` (`Ingress`).

```typescript
import type { Client, ContextValues, Interceptor, Transport as ConnectTransport } from "@connectrpc/connect"
import { createClient, createContextKey, createContextValues } from "@connectrpc/connect"
import type { UniversalClientFn } from "@connectrpc/connect/protocol"
import { createTransport as createConnectHttpTransport } from "@connectrpc/connect/protocol-connect"
import { createConnectTransport, createGrpcWebTransport } from "@connectrpc/connect-web"
import type { DescService } from "@bufbuild/protobuf"
import { Headers, HttpClient, HttpClientRequest, HttpTraceContext } from "@effect/platform"
import { Context, Effect, ExecutionPlan, Exit, Layer, Match, type ParseResult, Record, Runtime, Scope, Stream } from "effect"
import { Ingress } from "../value/schema.ts"
import { Hlc } from "../value/clock.ts"
import { TenantContext } from "../value/identity.ts"

const _protocols = ["connect", "connect-http", "grpc-web"] as const
const _LADDER = { primary: 1, standby: 2 } as const

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
        )
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
  Effect.option(Effect.currentSpan),
  Option.match({
    onNone: () => Headers.empty,
    onSome: (span) => HttpTraceContext.toHeaders(span),
  }),
)

const _unary = <O>(
  call: (options: { readonly contextValues: ContextValues; readonly headers: Headers.Headers; readonly signal: AbortSignal }) => Promise<O>,
): Effect.Effect<O, FaultDetail> =>
  Effect.flatMap(Effect.all({ context: _charged, headers: _stamped }), ({ context, headers }) =>
    Effect.tryPromise({ try: (signal) => call({ contextValues: context, headers, signal }), catch: Transport.fault }))

const _streamed = <O>(
  open: (options: { readonly contextValues: ContextValues; readonly headers: Headers.Headers }) => AsyncIterable<O>,
): Stream.Stream<O, FaultDetail> =>
  Stream.unwrap(Effect.map(Effect.all({ context: _charged, headers: _stamped }), ({ context, headers }) =>
    Stream.fromAsyncIterable(open({ contextValues: context, headers }), Transport.fault)))

class Dial extends Effect.Service<Dial>()("@rasm/ts/core/Dial", {
  effect: (config: Dial.Config, seam: Dial.Seam) =>
    Effect.gen(function* () {
      const client = yield* HttpClient.HttpClient
      const runtime = yield* Effect.runtime<never>()
      const transports = Record.map(_lanes, (make) => make({ config, seam, universal: _universal(client, runtime) }))
      const ladder = Array.map(config.lanes, (protocol, rank) => ({
        provide: Layer.succeed(Lane, { protocol, transport: transports[protocol] }),
        attempts: rank === 0 ? _LADDER.primary : _LADDER.standby,
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

[CAPABILITY_BIND]:
- Owner: `Capability` — `CapabilityDescriptor`, the decoded capability identity (name, qualified service name, the content key of the command shape, the mint instant) with `admit` comparing the runtime-shipped key against the build pin through the codec parity gate; `Sdk<T>`, the mapped type computing every method's Effect signature from the promise `Client<T>`; and `bind`, the kind-total fold walking `service.methods` by `methodKind` under the failover plan, wrapping each method through the `Dial` lifts with per-method `Budget` geometry and the plane's observability transformers.
- Law: content-keyed admission — the key covers the command shape's canonical bytes, branded-key equality is bare `===` under the one mint, the check runs once at bind time, and a diverging key refuses with `parity` evidence because a capability whose command shape moved is a different capability.
- Law: the SDK is a descriptor, never a hand-written client — `Sdk<T>` maps `Client<T>`'s own member types into Effect carriers, the runtime record builds from the descriptor's own `methods` walk, and the two derive from one descriptor; a parallel interface per capability is the second-truth defect.
- Law: the derivation is kind-total over the shipped axis through `Match.discriminatorsExhaustive("methodKind")` — `unary` and `server_streaming` bind; `client_streaming` and `bidi_streaming` refuse at bind time as `drift` evidence because the C# emitter does not mint them; a fifth method kind breaks the fold loudly at compile time, and silence over an unbindable method strands its caller at runtime — the catch-all ternary that absorbs it is the deleted spelling.
- Law: budget geometry is the rails layering law realized — a method with a budget row composes `Effect.timeoutFail(Budget[kind].attempt)` below `Effect.retry(Budget.schedule(kind))` and `Effect.timeoutFail(Budget[kind].total)` above it, both deadlines minting through `Transport.expired` so the whole-call and per-try budgets live in the row and the transport `timeoutMs` is only the lane-level floor; the schedule is class-gated through the fault's own classification so a terminal reason never re-drives, and a method without a row never retries — the safe default for non-idempotent verbs. A streaming method with a budget row composes `Stream.retry(Budget.schedule(kind))` — re-registration semantics, the resume coordinate living in the source.
- Law: every bound method runs under the failover plan and inside the plane's telemetry transformers — `Effect.withExecutionPlan` attaches the `Dial` ladder on the unary lane and `Stream.withExecutionPlan` with `preventFallbackOnPartialStream` attaches it on the streaming lane, so a retryable fault walks the ladder mid-stream included, a partially-emitted feed never re-emits from a fresh lane — the budget `Stream.retry` above the plan owns resumption because the resume coordinate lives in the source — and the winning lane stamps the open span as the `invokeLane` row per engaged lane, so lane choice is a span dimension, never a call-site fact; `_observed` stacks the latency timer, the fault-reason frequency, the `Exit`-folded outcome counter, the span, and the log annotation on the unary lane, and `_observedStream` stacks the fault frequency, the same outcome counter over the stream's own scope exit through `Stream.ensuringWith`, and the span on the streaming lane — names and tag keys are `observe/convention` rows, the outcome dimension is the interrupt-first `Exit` fold — so the branch's hottest invocation surface is traced and measured by construction on both modalities.
- Exemption: the `_slots` client pin and the terminal `Object.fromEntries` assembly are the page's one sanctioned assertion cluster — the mapped type computes from `Client<T>` while the runtime record builds from the descriptor's `methods` walk; the pin with its proven-slot read lives inside `_slots` under the `// BOUNDARY ADAPTER` mark, and the assembly states the same one-descriptor correspondence at the return.
- Growth: a new method appears in the SDK at regeneration with zero edits here; a method gaining idempotency is one budget row at the caller; a new outcome dimension widens the `Exit` fold's anchored union, never an arm.
- Boundary: the emitted `DescService` consts are build artifacts the app's capability modules import — the composition root hands the same consts to the contract gate and sequences `DescriptorGate.admitted("CapabilityDescriptorWire")` ahead of `bind`, so RPC method drift refuses before any client pins; the `CapabilityDescriptorWire` census row homes here; `Budget` rows are `value/fault.ts` vocabulary; the instrument names are `observe/convention` rows — the interchange plane's one import of the vocabulary spine.
- Packages: `@connectrpc/connect` (`Client`); `@bufbuild/protobuf` (`DescService`); `effect` (`Cause`, `Effect`, `Exit`, `Match`, `Metric`, `Option`, `Schema`, `Stream`, `pipe`); `./codec.ts` (`Parity`, `WireFault`); `./format.ts` (`Proto`); `../observe/convention.ts` (`Convention`); `../value/contentKey.ts` (`ContentKey`, `Digest`); `../value/fault.ts` (`Budget`).

```typescript
import { Cause, Metric } from "effect"
import { Budget } from "../value/fault.ts"
import { type ContentKey, Digest } from "../value/contentKey.ts"
import { Convention } from "../observe/convention.ts"
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
  type Outcome = "halted" | "crashed" | "resolved" | `rejected:${FaultDetail["reason"]}` // anchored on the Hops reason axis: a new reason widens it here
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
    options?: { readonly contextValues?: ContextValues; readonly headers?: Headers.Headers; readonly signal?: AbortSignal },
  ) => Promise<unknown> & AsyncIterable<unknown>
}

const _slots = <T extends DescService>(client: Client<T>) => (name: string): Capability._Slot =>
  (client as unknown as Readonly<Record<string, Capability._Slot>>)[name]! // BOUNDARY ADAPTER: the one-descriptor correspondence pin — the walk names only members the descriptor mints

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

const _observed = (span: string, tags: { readonly [key: string]: string }) =>
  <A, R>(self: Effect.Effect<A, FaultDetail, R>): Effect.Effect<A, FaultDetail, R> =>
    self.pipe(
      Metric.trackDuration(_clock),
      Metric.trackErrorWith(_faults, (fault: FaultDetail) => fault.reason),
      Effect.onExit((exit) => Metric.increment(Metric.tagged(_calls, Convention.rasm.invokeOutcome, _outcome(exit)))),
      Effect.withSpan(span, { attributes: tags }),
      Effect.annotateLogs(tags),
    )

const _observedStream = (span: string, tags: { readonly [key: string]: string }) =>
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
                (input: unknown) =>
                  pipe(
                    Effect.withExecutionPlan(
                      Effect.flatMap(Lane, (lane) =>
                        Effect.zipRight(
                          Effect.annotateCurrentSpan(Convention.rasm.invokeLane, lane.protocol),
                          dial.unary((options) => slots[lane.protocol](unary.localName)(input, options)),
                        )),
                      dial.plan,
                    ),
                    (call) =>
                      Option.match(Option.fromNullable(budgets?.[unary.localName]), {
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
                (input: unknown) =>
                  pipe(
                    Stream.unwrap(
                      Effect.flatMap(Lane, (lane) =>
                        Effect.as(
                          Effect.annotateCurrentSpan(Convention.rasm.invokeLane, lane.protocol),
                          dial.stream((options) => slots[lane.protocol](streaming.localName)(input, options)),
                        )),
                    ),
                    Stream.withExecutionPlan(dial.plan, { preventFallbackOnPartialStream: true }), // the plan governs the LIVE feed: a mid-stream fault walks the lane ladder, and a partially-emitted feed never re-emits from a fresh lane
                    (feed) =>
                      Option.match(Option.fromNullable(budgets?.[streaming.localName]), {
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
      return Object.fromEntries(rows) as Capability.Sdk<T> // BOUNDARY ADAPTER: descriptor-walk assembly meets the mapped type
    }),
}
```

## [05]-[COMMAND_GATEWAY]

[COMMAND_GATEWAY]:
- Owner: `Gateway`, the inbound half of the capability plane — `CommandPayload`, the decoded command class (verb, body carriage, tenant, `Hlc` stamp); the `Effect.Service` whose Layer factory takes the app's verb row table so the gateway is one generic dispatch and a verb set is data; `Dispatched`, the Granted/Refused outcome family carrying the `state` verdict whole; `AvailabilityGate`, the port this page declares and the app root satisfies from `state`-fed evidence; `SupportCapture` with its receipt and the `SupportIntake` port as the support verb's delivery; and `duplex`, the typed command/outcome channel whose frame row is a `_frames` vocabulary lookup over the fused `duplexSchema` transformers.
- Law: the row table is the dispatch — one indexed lookup over the app-supplied record; an unknown verb is `drift` evidence because the shell and the app were built against one verb set; handlers receive the decoded payload and each row decodes its own body band, the second-admission law for nested unknown material.
- Law: `submit` is an `Effect.fn` definition seam — the span opens per dispatch, the gateway timer and the outcome counter ride the declaration tail with names from `observe/convention` rows, the verb stamps the current span, and the outcome tag is the `Exit`-folded `Gateway.Emission` union: the `Dispatched` tags by derivation, `rejected:` keyed by the `WireFault` reason axis, `invalid` for decode skew, `halted`/`crashed` from the interrupt-first cause fold — so every dispatch lands in the counter exactly once, faulted dispatches included, and inbound telemetry policy is recoverable from the declaration.
- Law: the gate types against `state` vocabulary, never re-declares it — the port answers `Availability.Verdict`, refusal transports the verdict whole (`Gated` keeps its `until`, `Withheld` its level), and gating is read-then-dispatch: staleness policy belongs to the providing Layer.
- Law: the support verb is one row on the same plane — the evidence band crosses opaque, interpretation belongs to the intake's consumer, and the port is declared here and satisfied at the root so the observe unit and this plane stay ledger-clean; the receipt travels back to the reporter so a support report is never fire-and-forget.
- Law: the duplex channel is one fused schema seam over the `_frames` row table — `MsgPack.duplexSchema`/`Ndjson.duplexSchema`/`Ndjson.duplexSchemaString` collapse the frame codec and the asymmetric schema pair (commands inbound, outcomes outbound, backpressure carried) into one channel transformer, and each row owns the socket lift its frame demands — `Socket.toChannelWith` under the byte frames, `Socket.toChannelString` under the text frame, because the string transformer types a `Chunk<string>` channel a byte lift can never satisfy — so the two-stage frame-then-`ChannelSchema` sandwich and the frame ternary are both deleted spellings; the socket arrives through the runtime-satisfied constructor Tag, and raw listeners and hand framing are unspellable above it.
- Law: the msgpack row is the command lane's standing frame — the `Hlc` halves are `bigint` and JSON owns no bigint spelling, so both ndjson rows are legal only over JSON-safe encoded schemas on both directions; `ndjson-text` is `Ndjson.duplexSchemaString` over the string channel, the text-frame lane for text-only transports, under the same JSON-safety law — a serving edge that must frame commands as text lands its JSON stamp spelling first, and swapping a row under a bigint-carrying schema is the precision defect the frame discriminant cannot absorb.
- Growth: a new verb is one row in the app's table; a new outcome kind is one tagged case every exhaustive consumer breaks on; a fourth frame row is one `_frames` row.
- Boundary: the `CommandPayloadWire`/`SupportCaptureWire` census rows home here; the availability vocabulary and the total `admits` fallback are `state/evidence.ts`'s; the socket Layer and the serving loop are the runtime wave's.
- Packages: `@effect/platform` (`MsgPack`, `Ndjson`, `Socket`); `effect` (`Cause`, `Context`, `Data`, `Effect`, `Exit`, `Metric`, `Option`, `Schema`); `./codec.ts` (`WireFault`); `./format.ts` (`Proto`); `../observe/convention.ts` (`Convention`); `../value/clock.ts` (`Hlc`); `../value/identity.ts` (`TenantContext`); `../state/evidence.ts` (`Availability`).

```typescript
import { MsgPack, Ndjson, Socket } from "@effect/platform"
import { type Channel, type Chunk, Data } from "effect"
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
  type Emission = "halted" | "crashed" | "invalid" | Dispatched["_tag"] | `rejected:${WireFault["reason"]}` // anchored on the family tags and the codec reason axis: a new reason widens it here
  type Frame = keyof typeof _frames
  type Duplex<A> = Channel.Channel<
    Chunk.Chunk<CommandPayload>,
    Chunk.Chunk<A>,
    MsgPack.MsgPackError | Ndjson.NdjsonError | ParseResult.ParseError | Socket.SocketError,
    ParseResult.ParseError,
    void,
    unknown
  >
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

const _emitted: (exit: Exit.Exit<Dispatched, ParseResult.ParseError | WireFault>) => Gateway.Emission = Exit.match({
  onFailure: (cause) =>
    Cause.isInterruptedOnly(cause)
      ? ("halted" as const)
      : Option.match(Cause.failureOption(cause), {
          onNone: () => "crashed" as const,
          onSome: (fault) => (fault._tag === "WireFault" ? (`rejected:${fault.reason}` as const) : ("invalid" as const)),
        }),
  onSuccess: (outcome) => outcome._tag,
})

class Gateway extends Effect.Service<Gateway>()("@rasm/ts/core/Gateway", {
  effect: (handlers: Gateway.Handlers) =>
    Effect.map(AvailabilityGate, (gate) => ({
      submit: Effect.fn("gateway.submit")(
        function* (octets: Uint8Array) {
          const payload = yield* Schema.decodeUnknown(CommandPayload.FromBytes)(octets)
          yield* Effect.annotateCurrentSpan(Convention.rasm.gatewayVerb, payload.verb)
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
          return yield* (verdict._tag === "Available"
            ? Effect.map(handle(payload), (receipt) => Dispatched.Granted({ verb: payload.verb, receipt }))
            : Effect.succeed(Dispatched.Refused({ verb: payload.verb, verdict })))
        },
        (effect) => Metric.trackDuration(effect, _gatewayClock),
        (effect) =>
          Effect.onExit(effect, (exit) =>
            Metric.increment(Metric.tagged(_gatewayCommands, Convention.rasm.gatewayOutcome, _emitted(exit)))),
      ),
    })),
  accessors: true,
}) {
  static readonly Payload: typeof CommandPayload = CommandPayload
  static readonly Outcome: Data.TaggedEnum.Constructor<Dispatched> = Dispatched
  static duplex<A, I>(socket: Socket.Socket, frame: Gateway.Frame, outcome: Schema.Schema<A, I>): Gateway.Duplex<A> {
    return _frames[frame](outcome)(socket)
  }
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { AvailabilityGate, Capability, Dial, Gateway, SupportCapture, SupportIntake, Transport }
```
