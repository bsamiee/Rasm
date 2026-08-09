# [CORE_INVOKE]

`Invoke` owns typed Connect clients, descriptor admission, command dispatch, and framed socket serving.

## [01]-[INDEX]

- [02]-[TRANSPORT_FAULT]: Connect failure normalization; `Invoke.Transport`.
- [03]-[DIAL_AXIS]: typed client lanes and execution plan; `Invoke.Dial`.
- [04]-[CAPABILITY_BIND]: descriptor-gated SDK derivation; `Invoke.Capability`.
- [05]-[COMMAND_GATEWAY]: table-derived invocation, dispatch, and framed serving; `Invoke.Gateway`.

## [02]-[TRANSPORT_FAULT]

- Owner: `Invoke.Transport` maps Connect failures and registered details into `Wire.FaultDetail`.
- Law: one total code table preserves remote hops and appends the local edge.
- Boundary: Connect supplies error/detail decoding; `Wire` owns hop and fault vocabularies.

```typescript signature
import type { Client, ContextValues, Interceptor, Transport as ConnectTransport } from "@connectrpc/connect"
import { Code, ConnectError, createClient, createContextKey, createContextValues } from "@connectrpc/connect"
import type { CommonTransportOptions, Compression, UniversalClientFn } from "@connectrpc/connect/protocol"
import { createFetchClient } from "@connectrpc/connect/protocol"
import { createTransport as createConnectProtocol } from "@connectrpc/connect/protocol-connect"
import { createTransport as createGrpcProtocol } from "@connectrpc/connect/protocol-grpc"
import { createTransport as createGrpcWebProtocol } from "@connectrpc/connect/protocol-grpc-web"
import { isMessage, type DescService, type MessageInitShape } from "@bufbuild/protobuf"
import { Headers, HttpClient, HttpClientRequest, MsgPack, Ndjson, Socket } from "@effect/platform"
import {
  Array,
  Cause,
  Channel,
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
  Queue,
  Record,
  Runtime,
  Schema,
  Scope,
  Stream,
  pipe,
} from "effect"
import { Convention } from "../observe/convention.ts"
import { Evidence } from "../state/evidence.ts"
import { Clock } from "../value/clock.ts"
import { Digest } from "../value/contentKey.ts"
import { Fault } from "../value/fault.ts"
import { Identity } from "../value/identity.ts"
import { Shape } from "../value/schema.ts"
import { Carrier } from "./carrier.ts"
import { Wire } from "./codec.ts"
import { Contract } from "./contract.ts"
import { Format } from "./format.ts"

const _edge = (reason: Wire.Hops.Reason): InstanceType<typeof Wire.FaultDetail.Hop> =>
  new Wire.FaultDetail.Hop({ site: "<local-edge>", reason, elapsed: Duration.zero })

const Transport: {
  readonly expired: (surface: string, detail: string) => Wire.FaultDetail
  readonly fault: (caught: unknown) => Wire.FaultDetail
} = {
  expired: (surface, detail) =>
    new Wire.FaultDetail({ reason: "deadline", surface, detail, hops: [_edge("deadline")], tenant: Option.none() }),
  fault: (caught) =>
    pipe(ConnectError.from(caught, Code.Unknown), (error) =>
      Option.match(
        pipe(
          Array.findFirst(error.findDetails(Format.proto.registry), (detail) => isMessage(detail, Format.proto.suite.FaultDetail)),
          Option.flatMap((wire) => Option.getRight(Schema.decodeUnknownEither(Wire.FaultDetail.FromWire)(wire))),
        ),
        {
          onNone: () =>
            new Wire.FaultDetail({
              reason: Wire.Hops.fromCode(error.code),
              surface: "<transport>",
              detail: error.rawMessage,
              hops: [_edge(Wire.Hops.fromCode(error.code))],
              tenant: Option.none(),
            }),
          onSome: (detail) => new Wire.FaultDetail({ ...detail, hops: [...detail.hops, _edge(Wire.Hops.fromCode(error.code))] }),
        },
      )),
}
```

## [03]-[DIAL_AXIS]

- Owner: `Invoke.Dial` builds one typed Connect client per configured protocol-and-carrier lane.
- Law: `Dial.sdk` admits a descriptor and mints every client; no second entry reaches a transport.
- Law: `tenancy` carries no column on either Dial family because NEITHER decides it — a profile selects the row and tenancy realizes through `Carrier.promote`, so a cell here states by guess what a live owner already answers.
- Law: protocol rows decide framing alone — `admit` is uniform (every row takes one `CommonTransportOptions` through `transport`) and `lifetime` belongs to the carrier row beneath, so each rides this lead rather than repeating itself or guessing down a column.
- Law: carrier rows answer `admit` and `lifetime` per row because both DIVERGE — the two carriers take different handles and end a connection under different owners, which is the whole difference the axis exists to state.
- Law: a lane names the `value/fault#RETRY_BUDGET` row it recovers on, so recovery geometry has one owner and the ladder spends that row's compiled schedule and attempt ceiling.
- Law: Deadlines end a call at the attempt bound and the plan at the total bound; a stream ends with the scope its caller opened.
- Law: protocol and carrier are orthogonal rows over one `CommonTransportOptions`; no lane branches on a name.
- Law: every lane reads one ingress ceiling, so failover never widens the bound a primary lane proved.
- Law: unary failover uses `ExecutionPlan`; server streams never retry after an emitted value.
- Law: a ceiling refusal never fails over — one bound holds on every lane, so a second dial re-proves the same arithmetic.
- Law: carrier headers, tenant scope, HLC, deadlines, and telemetry remain typed Effect context.
- Boundary: the runtime supplies fetch, interceptors, compression algorithms, and the platform HTTP client.

```typescript signature
// Protocol and carrier are ORTHOGONAL axes the package itself keeps apart: all three `createTransport` factories take
// ONE `CommonTransportOptions`, whose `httpClient` field IS the carrier. Folding them into one enumeration stranded
// every unbuilt pair, left gRPC no seat at all, and let one row alone read the ingress ceiling.
const _protocols = ["connect", "grpc", "grpc-web"] as const
const _carriers = ["fetch", "platform"] as const

// `fits` is the sentence a composition root selects on and `degrade` the forfeit it accepts by selecting; `httpGet`
// and `trailers` are the two capabilities the protocols genuinely differ on, so a caller reads a column instead of
// inferring framing from a name.
//
// ADMIT, LIFETIME, and TENANCY carry no column here. Every row admits exactly one `CommonTransportOptions` through
// its own `transport` member, so a column would repeat one answer three times rather than separate anything. A
// protocol row is a pure framing factory holding nothing across calls: connection lifetime rides `_carrierRows`
// below, which genuinely ends one, and tenancy realizes at `Carrier.promote`, which genuinely scopes one. Stating
// either here states a guess, and neither non-decision is a forfeit, so neither reaches `degrade`.
const _protocolRows = {
  connect: {
    transport: createConnectProtocol,
    httpGet: true,
    trailers: false,
    fits: "<browser-and-service-peers-speaking-connect>",
    degrade: "<no-grpc-peer-interop>",
  },
  grpc: {
    transport: createGrpcProtocol,
    httpGet: false,
    trailers: true,
    fits: "<service-peers-behind-an-http2-carrier>",
    degrade: "<carrier-must-carry-http2-trailers>",
  },
  "grpc-web": {
    transport: createGrpcWebProtocol,
    httpGet: false,
    trailers: true,
    fits: "<grpc-peers-reachable-only-through-a-web-proxy>",
    degrade: "<proxy-required-for-every-grpc-peer>",
  },
} as const satisfies { readonly [P in Dial.Protocol]: Dial.ProtocolRow }

// Carriers differ in ONE fact the options record spends, and each ends a connection under a different owner: `fetch`
// leaves pooling to the ambient global this package never closes, while `platform` re-enters the rail so the shared
// client's scope, retry, proxy, and tracing posture govern every dial. `admit` names the handle a composition root
// hands in, and it diverges exactly as `lifetime` does — one carrier takes what the host already carries and the
// other takes a constructed pair — so a root reading `lifetime` alone could not tell what it owes to get it.
// TENANCY carries no column: `Carrier.promote` scopes every dial and no row here decides one.
const _carrierRows = {
  fetch: {
    fits: "<bundles-that-must-not-load-the-platform-client>",
    admit: "<the-ambient-globalThis.fetch-the-host-already-carries>",
    lifetime: "<ambient-global-pool-this-package-never-ends>",
    degrade: "<no-shared-client-policy>",
  },
  platform: {
    fits: "<runtimes-composing-the-shared-http-client>",
    admit: "<a-composed-HttpClient-beside-the-runtime-this-package-re-enters-per-request>",
    lifetime: "<shared-client-scope-ends-it>",
    degrade: "<one-runtime-re-entry-per-request>",
  },
} as const satisfies { readonly [C in Dial.Carrier]: Dial.CarrierRow }

const _PositiveDuration = Schema.DurationFromMillis.pipe(
  Schema.filter((duration) => Duration.toMillis(duration) > 0, { identifier: "PositiveDialDuration" }),
)

// Each lane names its retry OWNER and carries no curve: `budget` is the `value/fault#RETRY_BUDGET` row whose
// compiled schedule and attempt ceiling this ladder spends, so a proxy row and a direct row differ by which row
// they name.
const _LanePolicy = Schema.Struct({
  protocol: Schema.Literal(..._protocols),
  carrier: Schema.Literal(..._carriers),
  budget: Fault.Budget.schema,
})

const _TransportPolicy = Schema.Struct({
  attempt: _PositiveDuration,
  total: _PositiveDuration,
  compressMinBytes: Shape.Refined.OrdinalKey.pipe(Schema.positive()),
  useHttpGet: Schema.optionalWith(Schema.Boolean, { default: () => false }),
})

const _key = (lane: { readonly protocol: Dial.Protocol; readonly carrier: Dial.Carrier }): Dial.Key =>
  `${lane.protocol}:${lane.carrier}`

const _DialConfig = Schema.Struct({
  lanes: Schema.NonEmptyArray(_LanePolicy).pipe(
    Schema.filter(
      (lanes) => Array.dedupe(Array.map(lanes, _key)).length === lanes.length || "<duplicate-dial-lane>",
      { identifier: "DistinctDialLanes" },
    ),
  ),
  baseUrl: Schema.NonEmptyString,
  useBinaryFormat: Schema.optionalWith(Schema.Boolean, { default: () => true }),
  transport: _TransportPolicy,
})

declare namespace Dial {
  type Protocol = (typeof _protocols)[number]
  type Carrier = (typeof _carriers)[number]
  type Key = `${Protocol}:${Carrier}`
  type ProtocolRow = {
    readonly transport: (options: CommonTransportOptions) => ConnectTransport
    readonly httpGet: boolean
    readonly trailers: boolean
    readonly fits: string
    readonly degrade: string
  }
  type CarrierRow = {
    readonly fits: string
    readonly admit: string
    readonly lifetime: string
    readonly degrade: string
  }
  type Config = Schema.Schema.Type<typeof _DialConfig>
  type Seam = {
    readonly fetch: typeof globalThis.fetch
    readonly interceptors: ReadonlyArray<Interceptor>
    readonly compression: ReadonlyArray<Compression>
    readonly sendCompression: Option.Option<Compression>
  }
  type Gear = {
    readonly config: Config
    readonly seam: Seam
    readonly clients: Readonly<Record<Carrier, UniversalClientFn>>
    readonly timeoutMs: number
  }
  type _Protocols<T extends Record<Protocol, ProtocolRow> = typeof _protocolRows> = T
  type _Carriers<T extends Record<Carrier, CarrierRow> = typeof _carrierRows> = T
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

// One options record serves every pair, so a lane differs from its sibling in the two fields the axes name and in
// nothing else. Compression algorithms are host code the seam carries, and an empty roster is the option's own
// documented opt-out rather than a silent default.
const _options = (gear: Dial.Gear, protocol: Dial.Protocol, carrier: Dial.Carrier): CommonTransportOptions => ({
  httpClient: gear.clients[carrier],
  baseUrl: gear.config.baseUrl,
  useBinaryFormat: gear.config.useBinaryFormat,
  interceptors: [...gear.seam.interceptors],
  acceptCompression: [...gear.seam.compression],
  sendCompression: Option.getOrNull(gear.seam.sendCompression),
  compressMinBytes: gear.config.transport.compressMinBytes,
  // EVERY lane reads ONE ingress ceiling. A ladder whose fallback row keeps the package's ~4GiB default silently
  // removes the bound its primary row proved, so the failover itself becomes the way around the limit.
  readMaxBytes: Shape.Ingress.floor.bytes,
  writeMaxBytes: Shape.Ingress.floor.bytes,
  defaultTimeoutMs: gear.timeoutMs,
  // GET routing is Connect's capability alone, so the row column gates the policy and a caller never spells a protocol.
  useHttpGet: _protocolRows[protocol].httpGet && gear.config.transport.useHttpGet,
})

const _transport = (gear: Dial.Gear, protocol: Dial.Protocol, carrier: Dial.Carrier): ConnectTransport =>
  _protocolRows[protocol].transport(_options(gear, protocol, carrier))

const _built = (gear: Dial.Gear): HashMap.HashMap<Dial.Key, ConnectTransport> =>
  HashMap.fromIterable(
    Array.flatMap(_protocols, (protocol) =>
      Array.map(_carriers, (carrier) => [_key({ protocol, carrier }), _transport(gear, protocol, carrier)] as const)),
  )

class Lane extends Context.Tag("@rasm/ts/core/Lane")<Lane, {
  readonly key: Dial.Key
  readonly protocol: Dial.Protocol
  readonly carrier: Dial.Carrier
  readonly transport: ConnectTransport
}>() {}

const _CONTEXT = {
  stamp: createContextKey<Option.Option<Clock.Hlc>>(Option.none()),
  tenant: createContextKey<Option.Option<Identity.Tenant>>(Option.none()),
} as const

class Ambient extends Context.Reference<Ambient>()("@rasm/ts/core/Dial/Ambient", {
  defaultValue: (): { readonly stamp: Option.Option<Clock.Hlc>; readonly tenant: Option.Option<Identity.Tenant> } =>
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
): Effect.Effect<O, Wire.FaultDetail> =>
  Effect.flatMap(Effect.all({ context: _charged, headers: _stamped }), ({ context, headers }) =>
    Effect.tryPromise({ try: (signal) => call({ contextValues: context, headers, signal }), catch: Transport.fault }))

const _openStream = <O>(
  open: (options: { readonly contextValues: ContextValues; readonly headers: Headers.Headers; readonly signal: AbortSignal }) => AsyncIterable<O>,
): Stream.Stream<O, Wire.FaultDetail> =>
  Stream.unwrapScoped(
    Effect.acquireRelease(
      Effect.sync(() => new AbortController()),
      (controller) => Effect.sync(() => controller.abort()),
    ).pipe(
      Effect.flatMap((controller) =>
        Effect.flatMap(Effect.all({ context: _charged, headers: _stamped }), ({ context, headers }) =>
          Effect.map(Effect.try({
            try: () => open({ contextValues: context, headers, signal: controller.signal }),
            catch: Transport.fault,
          }), (messages) => Stream.fromAsyncIterable(messages, Transport.fault)))),
    ),
  )

// Failover and retry-in-place are DIFFERENT questions one retryability column cannot answer alone. A ceiling refusal
// raises the same `exhausted` reason a loaded server raises, yet every lane now carries one ingress bound, so dialing
// a second row re-proves one arithmetic fact and spends the whole budget doing it. Every other retryable reason names
// a condition a different peer path genuinely answers, so the ladder advances on those and stops on this one.
const _failsOver = (fault: Wire.FaultDetail): boolean => fault.retryable && fault.reason !== "exhausted"

// One verdict governs a ladder step: the budget recurs on exactly what the ladder advances on, so a lane can never
// re-drive a fault its own failover rule refuses. `Fault.Budget`'s own gate reads a `class` property the wire detail
// does not carry, so the gate is passed rather than inherited, and the instance test keeps the read O(1).
const _retries: Predicate.Predicate<unknown> = (fault) => fault instanceof Wire.FaultDetail && _failsOver(fault)

class Dial extends Effect.Service<Dial>()("@rasm/ts/core/Dial", {
  effect: (config: Dial.Config, seam: Dial.Seam) =>
    Effect.gen(function* () {
      const client = yield* HttpClient.HttpClient
      const runtime = yield* Effect.runtime<never>()
      const gear: Dial.Gear = {
        config,
        seam,
        clients: { fetch: createFetchClient(seam.fetch), platform: _universal(client, runtime) },
        timeoutMs: Duration.toMillis(config.transport.attempt),
      }
      const transports = _built(gear)
      const ladder = Array.map(config.lanes, (row) => ({
        provide: Layer.succeed(Lane, {
          key: _key(row),
          protocol: row.protocol,
          carrier: row.carrier,
          transport: Option.getOrElse(HashMap.get(transports, _key(row)), () => _transport(gear, row.protocol, row.carrier)),
        }),
        attempts: Fault.Budget.at(row.budget).attempts,
        schedule: Fault.Budget.schedule(row.budget, _retries),
        while: (fault: Wire.FaultDetail) => Effect.succeed(_failsOver(fault)),
      }))
      return {
        plan: ExecutionPlan.make(Array.headNonEmpty(ladder), ...Array.tailNonEmpty(ladder)),
        totalTimeout: config.transport.total,
        client: <T extends DescService>(service: T): HashMap.HashMap<Dial.Key, Client<T>> =>
          HashMap.map(transports, (transport) => createClient(service, transport)),
        unary: _unary,
        stream: _openStream,
      }
    }),
}) {
  static readonly Ambient: typeof Ambient = Ambient
  static readonly Config: typeof _DialConfig = _DialConfig
  static readonly Context: typeof _CONTEXT = _CONTEXT
  static readonly Lane: typeof Lane = Lane
  static readonly sdk = <T extends DescService>(service: T): Effect.Effect<
    Dial.Sdk<T>, ParseResult.ParseError | Wire.Fault, Dial
  > => _sdkOf(service)
}
```

## [04]-[CAPABILITY_BIND]

- Owner: `Invoke.Capability.admit` validates exact descriptor-pin bytes, while `Invoke.Dial.sdk` derives Connect members.
- Law: pin admission hashes the carried document bytes and validates count, descriptor order, and unit order.
- Law: parsed rows authorize semantic reads only; the original UTF-8 document remains the identity preimage.
- Law: Connect derivation reads only `DescService`; no descriptor row is inferred as a service or method coordinate.
- Law: each unary attempt uses the transport timeout, and one outer timeout bounds the complete execution plan.
- Law: server-stream fallback stops after the first emitted value.

```typescript signature
const _descriptorDocument = Format.json.schema(Schema.Array(Contract.Descriptor.Row))
const _descriptorBytes = new TextEncoder()

const _pinFault = (detail: string, actual: unknown, expected: unknown): Wire.Fault =>
  new Wire.Fault({
    family: "DescriptorPinWire",
    reason: "parity",
    detail,
    evidence: Option.some({ actual, expected }),
  })

const _admitCapability = ({ descriptor, pinned }: Capability.Source): Effect.Effect<
  Capability.Admitted,
  ParseResult.ParseError | Wire.Fault,
  Context.Tag.Service<typeof Contract.Descriptor>
> => Effect.gen(function* () {
  const pin = yield* Wire.decode("DescriptorPinWire", descriptor)
  const octets = _descriptorBytes.encode(pin.document)
  yield* Wire.Parity.verified("DescriptorPinWire", pin.digest, octets)
  yield* Wire.Parity.matched("DescriptorPinWire", pin.digest, pinned)
  const descriptors = yield* Schema.decodeUnknown(_descriptorDocument)(octets)
  if (descriptors.length !== pin.descriptors) {
    return yield* Effect.fail(_pinFault("<descriptor-count>", descriptors.length, pin.descriptors))
  }
  if (!descriptors.every((row, at) => at === 0 || descriptors[at - 1]!.descriptor < row.descriptor)) {
    return yield* Effect.fail(_pinFault("<descriptor-order>", descriptors, "<strictly-sorted-distinct>"))
  }
  return { pin, descriptors }
})

declare namespace Capability {
  type Source = { readonly descriptor: Uint8Array; readonly pinned: Digest.Key<"content"> }
  type Admitted = {
    readonly pin: Contract.Pin
    readonly descriptors: ReadonlyArray<Contract.Descriptor.Row>
  }
}

declare namespace Dial {
  type Outcome = Convention.Outcome<`rejected:${Wire.FaultDetail["reason"]}`>
  type Sdk<T extends DescService> = {
    readonly [K in keyof Client<T>]: Client<T>[K] extends (input: infer I, options?: infer _O) => Promise<infer O>
      ? (input: I) => Effect.Effect<O, Wire.FaultDetail>
      : Client<T>[K] extends (input: infer I, options?: infer _O) => AsyncIterable<infer O>
        ? (input: I) => Stream.Stream<O, Wire.FaultDetail>
        : never
  }
}

const _sdkSchema = <T extends DescService>(service: T): Schema.Schema<Dial.Sdk<T>> =>
  Schema.declare(
    (input: unknown): input is Dial.Sdk<T> =>
      Predicate.isRecord(input)
      && Array.every(service.methods, (method) => Predicate.isFunction(input[method.localName])),
    { identifier: `${service.typeName}Sdk` },
  )

// The reason roster preregisters, so a hop nothing has raised yet reports zero rather than leaving the panel a hole
const _calls = Convention.mount(Convention.metric.invokeCalls) // module scope: Effect keys an instrument by name and tag set, so a mount inside a fold re-derives one registry entry per call
const _clock = Convention.mount(Convention.metric.invokeDuration)
const _faults = Convention.mount(Convention.metric.invokeFault, Wire.Hops.reasons)

const _rejected = (fault: Wire.FaultDetail): `rejected:${Wire.Hops.Reason}` => `rejected:${fault.reason}` as const

// The rail lane composes the convention owner's aspect whole — mount, single emission point, and interrupt-first fold
// all live there, and this page supplies only its own reason projection.
const _counted = Convention.outcome(Convention.metric.invokeCalls, Convention.rasm.invokeOutcome, _rejected)

// Stream carries no `onExit` aspect, so the feed lane folds the SAME anchor at its own scope exit; `_rejected` is the
// one reason projection both lanes read, so the two exits can never disagree on a word.
const _streamOutcome = (exit: Exit.Exit<unknown, Wire.FaultDetail>): Dial.Outcome =>
  Exit.match(exit, {
    onFailure: (cause) =>
      Cause.isInterruptedOnly(cause)
        ? ("halted" as const)
        : Option.match(Cause.failureOption(cause), { onNone: () => "crashed" as const, onSome: _rejected }),
    onSuccess: () => "resolved" as const,
  })

const _observed = (span: string, tags: Convention.Attributes) =>
  <A, R>(self: Effect.Effect<A, Wire.FaultDetail, R>): Effect.Effect<A, Wire.FaultDetail, R> =>
    self.pipe(
      Metric.trackDuration(_clock),
      Metric.trackErrorWith(_faults, (fault: Wire.FaultDetail) => fault.reason),
      _counted,
      Effect.withSpan(span, { attributes: tags }),
      Effect.annotateLogs(tags),
    )

const _observedStream = (span: string, tags: Convention.Attributes) =>
  <A, R>(self: Stream.Stream<A, Wire.FaultDetail, R>): Stream.Stream<A, Wire.FaultDetail, R> =>
    self.pipe(
      Stream.tapError((fault) => Metric.update(_faults, fault.reason)),
      Stream.ensuringWith((exit) => Metric.increment(Metric.tagged(_calls, Convention.rasm.invokeOutcome, _streamOutcome(exit)))),
      Stream.withSpan(span, { attributes: tags }),
    )

const _unbindable = (kind: string, name: string): Wire.Fault =>
  new Wire.Fault({
    family: "FaultDetail",
    reason: "drift",
    detail: `<unbindable-kind:${kind}:${name}>`,
    evidence: Option.none(),
  })

const _laneClient = <T extends DescService>(
  service: T,
  clients: HashMap.HashMap<Dial.Key, Client<T>>,
): Effect.Effect<Client<T>, never, Lane> =>
  Effect.flatMap(Lane, (lane) =>
    Effect.as(
      Effect.annotateCurrentSpan(Convention.rasm.invokeLane, lane.key),
      // Plans provide only keys `_built` generated, so the memo answers every lane; the lane's own transport closes
      // the remaining branch, keeping resolution total without a die the execution plan could never report.
      Option.getOrElse(HashMap.get(clients, lane.key), () => createClient(service, lane.transport)),
    ))

const _sdkOf = <T extends DescService>(service: T): Effect.Effect<
  Dial.Sdk<T>, ParseResult.ParseError | Wire.Fault, Dial
> =>
    Effect.gen(function* () {
      const dial = yield* Dial
      const clients = dial.client(service)
      const rows = yield* Effect.forEach(service.methods, (method) =>
        Match.value(method).pipe(
          Match.discriminatorsExhaustive("methodKind")({
            unary: (unary) =>
              Effect.succeed([
                unary.localName,
                (input: MessageInitShape<typeof unary.input>) =>
                  Effect.withExecutionPlan(
                      Effect.flatMap(_laneClient(service, clients), (client) =>
                        dial.unary((options) => client[unary.localName](input, options))),
                      dial.plan,
                    ).pipe(
                    Effect.timeoutFail({
                      duration: dial.totalTimeout,
                      onTimeout: () => Transport.expired(unary.localName, "<total-budget>"),
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
                      Effect.map(_laneClient(service, clients), (client) =>
                        dial.stream((options) => client[streaming.localName](input, options))),
                    ),
                    Stream.withExecutionPlan(dial.plan, { preventFallbackOnPartialStream: true }),
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
      return yield* Schema.decodeUnknown(_sdkSchema(service))(Record.fromEntries(rows))
    })

const Capability = {
  admit: _admitCapability,
}
```

## [05]-[COMMAND_GATEWAY]

- Owner: `Gateway.make` compiles one command table into value, JSON-byte, MessagePack-socket, and NDJSON-socket adapters.
- Law: each row binds one payload schema, output schema, and handler; the row table derives both closed wire unions.
- Law: every adapter decodes an invocation, delegates to `dispatch`, and encodes the full `Granted | Refused` outcome.
- Law: command bodies contain only the five-arm payload; tenant and clock context remain on the carrier and interceptor rails.
- Boundary: `Invoke.AvailabilityGate` supplies verdicts, and the runtime supplies socket acquisition and serving lifetime.

```typescript signature
const CommandPayload = Schema.Union(
  Schema.Struct({ kind: Schema.Literal("none") }),
  Schema.Struct({ kind: Schema.Literal("single"), id: Schema.NonEmptyString }),
  Schema.Struct({
    kind: Schema.Literal("many"),
    ids: Schema.NonEmptyArray(Schema.NonEmptyString).pipe(
      Schema.filter((ids) =>
        ids.length <= Shape.Ingress.floor.collection && Array.dedupe(ids).length === ids.length
          || "<command-id-census>"),
    ),
  }),
  Schema.Struct({ kind: Schema.Literal("text"), value: Schema.String }),
  Schema.Struct({
    kind: Schema.Literal("fields"),
    values: Schema.Record({ key: Schema.NonEmptyString, value: Shape.Json }).pipe(
      Schema.filter((values) => {
        const members = Object.keys(values).length
        return members > 0 && members <= Shape.Ingress.floor.members || "<command-field-census>"
      }),
    ),
  }),
)

class CommandInvocation extends Schema.Class<CommandInvocation>("CommandInvocation")({
  key: Schema.NonEmptyString,
  payload: CommandPayload,
}) {
  static readonly Payload: typeof CommandPayload = CommandPayload
  static readonly FromBytes: Schema.Schema<CommandInvocation, Uint8Array> = Format.json.schema(CommandInvocation)
}

type Dispatched<A> = Data.TaggedEnum<{
  Granted: { readonly verb: string; readonly receipt: A }
  Refused: { readonly verb: string; readonly verdict: Evidence.Availability.Verdict }
}>
interface DispatchedDefinition extends Data.TaggedEnum.WithGenerics<1> {
  readonly taggedEnum: Dispatched<this["A"]>
}
const _Dispatched = Data.taggedEnum<DispatchedDefinition>() // interior constructor: the annotation gate reaches the merged name, so only the type exports

class AvailabilityGate extends Context.Tag("@rasm/ts/core/AvailabilityGate")<AvailabilityGate, {
  readonly admits: (verb: string) => Effect.Effect<Evidence.Availability.Verdict>
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
  static readonly captured = (report: SupportCapture): Effect.Effect<SupportReceipt, never, SupportIntake> =>
    Effect.gen(function* () {
      const intake = yield* SupportIntake
      return yield* intake.deliver(report)
    })
}

class SupportIntake extends Context.Tag("@rasm/ts/core/SupportIntake")<SupportIntake, {
  readonly deliver: (report: SupportCapture) => Effect.Effect<SupportReceipt>
}>() {}

declare namespace Gateway {
  type Row<B, A, I> = {
    readonly invocation: Schema.Schema<B, unknown>
    readonly output: Schema.Schema<A, I>
    readonly handler: (invocation: CommandInvocation, payload: B) => Effect.Effect<A, Wire.Fault>
  }
  type _Compiled<A> = {
    readonly dispatch: (invocation: CommandInvocation) => Effect.Effect<A, ParseResult.ParseError | Wire.Fault>
  }
  type Table<
    Kinds extends readonly [string, ...string[]],
    B extends { readonly [K in Kinds[number]]: Invoke.Payload },
    A extends { readonly [K in Kinds[number]]: unknown },
    I extends { readonly [K in Kinds[number]]: unknown },
  > = {
    readonly kinds: Kinds
    readonly rows: Shape.ExactRows<Kinds, { readonly [K in Kinds[number]]: Row<B[K], A[K], I[K]> }>
  }
  type Emission = "halted" | "crashed" | "invalid" | Dispatched<unknown>["_tag"] | `rejected:${Wire.Fault["reason"]}`
  type Frame = keyof typeof _frames
  type Duplex<A> = Channel.Channel<
    Chunk.Chunk<CommandInvocation>,
    Chunk.Chunk<Dispatched<A>>,
    MsgPack.MsgPackError | Ndjson.NdjsonError | ParseResult.ParseError | Socket.SocketError,
    ParseResult.ParseError,
    void,
    unknown
  >
  type Shape<A> = {
    readonly invocation: Schema.Schema<CommandInvocation, unknown>
    readonly output: Schema.Schema<Dispatched<A>, unknown>
    readonly dispatch: (invocation: CommandInvocation) => Effect.Effect<Dispatched<A>, ParseResult.ParseError | Wire.Fault>
    readonly bytes: (octets: Uint8Array) => Effect.Effect<Dispatched<A>, ParseResult.ParseError | Wire.Fault>
    readonly serve: (socket: Socket.Socket, frame: Frame) => Effect.Effect<
      void,
      MsgPack.MsgPackError | Ndjson.NdjsonError | ParseResult.ParseError | Socket.SocketError | Wire.Fault,
      Scope.Scope
    >
  }
}

const _frames = {
  msgpack: <A>(invocation: Schema.Schema<CommandInvocation, unknown>, output: Schema.Schema<Dispatched<A>, unknown>) =>
    (socket: Socket.Socket): Gateway.Duplex<A> =>
    MsgPack.duplexSchema({ inputSchema: output, outputSchema: invocation })(
      Socket.toChannel<MsgPack.MsgPackError | ParseResult.ParseError>(socket),
    ),
  ndjson: <A>(invocation: Schema.Schema<CommandInvocation, unknown>, output: Schema.Schema<Dispatched<A>, unknown>) =>
    (socket: Socket.Socket): Gateway.Duplex<A> =>
    Ndjson.duplexSchema({ inputSchema: output, outputSchema: invocation })(
      Socket.toChannel<Ndjson.NdjsonError | ParseResult.ParseError>(socket),
    ),
} as const

const _gatewayClock = Convention.mount(Convention.metric.gatewayDuration)
const _gatewayCommands = Convention.mount(Convention.metric.gatewayCommands)

const _emitted = <A>(exit: Exit.Exit<Dispatched<A>, ParseResult.ParseError | Wire.Fault>): Gateway.Emission =>
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

const _command = <const K extends string, B extends Invoke.Payload>(key: K, payload: Schema.Schema<B, unknown>) =>
  Schema.Struct({ key: Schema.Literal(key), payload }).pipe(Schema.compose(CommandInvocation, { strict: false }))

const _make = <
  const Kinds extends readonly [string, ...string[]],
  B extends { readonly [K in Kinds[number]]: Invoke.Payload },
  A extends { readonly [K in Kinds[number]]: unknown },
  I extends { readonly [K in Kinds[number]]: unknown },
>({ kinds: keys, rows }: Gateway.Table<Kinds, B, A, I>): Effect.Effect<
  Gateway.Shape<A[Kinds[number]]>,
  never,
  AvailabilityGate
> => Effect.gen(function* () {
    const gate = yield* AvailabilityGate
    const table: HashMap.HashMap<string, Gateway._Compiled<A[Kinds[number]]>> = HashMap.fromIterable(
      Array.map(keys, (key) => {
        const row = rows[key]
        const compiled: Gateway._Compiled<A[Kinds[number]]> = {
          dispatch: (invocation) => Effect.flatMap(Schema.decodeUnknown(row.invocation)(invocation.payload),
            (payload) => row.handler(invocation, payload)),
        }
        return [key, compiled] as const
      }),
    )
    const invocation = Schema.Union(...Array.map(keys, (key) => _command(key, rows[key].invocation)))
    const output: Schema.Schema<Dispatched<A[Kinds[number]]>, unknown> = Schema.Union(...Array.map(keys, (key) => Schema.Union(
      Schema.Struct({ _tag: Schema.Literal("Granted"), verb: Schema.Literal(key), receipt: rows[key].output }),
      Schema.Struct({ _tag: Schema.Literal("Refused"), verb: Schema.Literal(key), verdict: Evidence.Availability.Verdict }),
    )))
    const dispatch = Effect.fn("gateway/dispatch")(
      function* (command: CommandInvocation) {
        yield* Effect.annotateCurrentSpan(Convention.rasm.gatewayVerb, command.key)
        const row = yield* Option.match(HashMap.get(table, command.key), {
          onNone: () => Effect.fail(new Wire.Fault({
            family: "CommandPayloadWire",
            reason: "drift",
            detail: `<unknown-verb:${command.key}>`,
            evidence: Option.none(),
          })),
          onSome: Effect.succeed,
        })
        const verdict = yield* gate.admits(command.key)
        return yield* (verdict._tag === "Available"
          ? Effect.map(row.dispatch(command), (receipt) => _Dispatched.Granted({ verb: command.key, receipt }))
          : Effect.succeed(_Dispatched.Refused({ verb: command.key, verdict })))
      },
      (effect) => Metric.trackDuration(effect, _gatewayClock),
      (effect) => Effect.onExit(effect, (exit) =>
        Metric.increment(Metric.tagged(_gatewayCommands, Convention.rasm.gatewayOutcome, _emitted(exit)))),
    )
    return {
      invocation,
      output,
      dispatch,
      bytes: (octets) => Effect.flatMap(Schema.decodeUnknown(Format.json.schema(invocation))(octets), dispatch),
      serve: (socket, frame) => Effect.gen(function* () {
        yield* Effect.makeSpanScoped("gateway/serve", { attributes: { [Convention.rasm.gatewayFrame]: frame } })
        const responses = yield* Queue.bounded<Dispatched<A[Kinds[number]]>>(Shape.Ingress.floor.frames)
        const channel = Stream.toChannel(Stream.fromQueue(responses)).pipe(
          Channel.pipeTo(_frames[frame](invocation, output)(socket)),
        )
        yield* Stream.runForEach(Stream.fromChannel(channel), (command) =>
          Effect.flatMap(dispatch(command), (response) => Queue.offer(responses, response)))
      }),
    }
  })

const Gateway: {
  readonly Invocation: typeof CommandInvocation
  readonly make: typeof _make
} = {
  Invocation: CommandInvocation,
  make: _make,
}

const Invoke = {
  Transport,
  Dial,
  Capability,
  Gateway,
  AvailabilityGate,
  Support: {
    Capture: SupportCapture,
    Intake: SupportIntake,
  },
} as const

declare namespace Invoke {
  type Payload = Schema.Schema.Type<typeof CommandPayload>
  type Invocation = InstanceType<typeof CommandInvocation>
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Invoke }
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
