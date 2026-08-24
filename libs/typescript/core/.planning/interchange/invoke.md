# [CORE_INVOKE]

`Invoke` owns typed Connect clients, descriptor admission, command dispatch, and framed socket serving.

## [01]-[INDEX]

- [02]-[TRANSPORT_FAULT]: Connect failure normalization; `Invoke.Transport`.
- [03]-[DIAL_AXIS]: typed client lanes and execution plan; `Invoke.Dial`.
- [04]-[CAPABILITY_BIND]: descriptor-gated SDK derivation; `Invoke.Capability`.
- [05]-[COMMAND_GATEWAY]: table-derived invocation, dispatch, and framed serving; `Invoke.Gateway`.

## [02]-[TRANSPORT_FAULT]

- Owner: `Invoke.Transport` maps each Connect failure into exactly one `Wire.InvokeFault` outcome.
- Law: one valid recognized detail becomes `Remote`; zero recognized details become `Transport`; malformed or multiple recognized details become terminal `MalformedDetail`.
- Law: the detail is the generated `rasm.contracts.fault.FaultDetail` read off `ConnectError.findDetails` against the one descriptor registry and decoded through `Wire.Remote.FromWire`; a `Transport` carries the class `Wire.Hops` rows for its code, the one table every hop grades through.
- Boundary: only `Transport(connectivity | deadline | ceiling)` may drive topology; remote recovery may retry only on the current lane.

```typescript signature
import type { Client, ContextValues, Interceptor, Transport as ConnectTransport } from "@connectrpc/connect"
import { Code, ConnectError, createClient, createContextKey, createContextValues } from "@connectrpc/connect"
import { createConnectTransport, createGrpcWebTransport } from "@connectrpc/connect-web"
import {
  isMessage,
  type DescMethod,
  type DescService,
  type JsonValue,
  type MessageInitShape,
  type MessageValidType,
} from "@bufbuild/protobuf"
import * as ui from "@rasm\/contracts/rasm/contracts/ui/commands_pb"
import { Headers, MsgPack, Ndjson, Socket } from "@effect/platform"
import {
  Array,
  Cause,
  Channel,
  type Chunk,
  Context,
  Data,
  Duration,
  Effect,
  Either,
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
import { Format } from "./format.ts"

const Transport: {
  readonly expired: (detail: string) => Wire.InvokeFault
  readonly fault: (caught: unknown) => Wire.InvokeFault
} = {
  expired: (detail) => new Wire.Transport({ kind: "deadline", class: Wire.Transport.classOf(Code.DeadlineExceeded), detail }),
  fault: (caught) => {
    const error = ConnectError.from(caught, Code.Unknown)
    const details = error.findDetails(Format.proto.registry)
      .filter((detail) => isMessage(detail, Format.proto.suite.FaultDetail))
    if (details.length === 0) {
      return new Wire.Transport({ kind: Wire.Transport.kindOf(error.code), class: Wire.Transport.classOf(error.code), detail: error.rawMessage })
    }
    if (details.length !== 1) {
      return new Wire.MalformedDetail({ detail: `<recognized-details:${details.length}>` })
    }
    return pipe(
      Schema.decodeUnknownEither(Wire.Remote.FromWire)(details[0]),
      Either.match({
        onLeft: (issue) => new Wire.MalformedDetail({ detail: issue.message }),
        onRight: (remote) => remote,
      }),
    )
  },
}
```

## [03]-[DIAL_AXIS]

- Owner: `Invoke.Dial` builds one typed Connect client per configured supported adapter lane.
- Law: `Dial.sdk` takes `Capability.Admitted` and mints every client, so an unadmitted pin is a type error and never a guarded path.
- Law: no second entry reaches a transport.
- Law: `tenancy` carries no column on any Dial family because NEITHER decides it — a profile selects the row and tenancy realizes through `Carrier.promote`, so a cell here states by guess what a live owner already answers.
- Law: one discriminated adapter family closes supported pairs at construction — `web` admits Connect and gRPC-Web, `node` admits Connect, gRPC-Web, and gRPC; `web + grpc` has no schema arm, no type, and no runtime guard.
- Law: adapters expose the package's public `Transport` factories behind one typed capability; `web` binds `@connectrpc/connect-web`, while runtime supplies the scoped `node` capability built with `@connectrpc/connect-node` and its HTTP/2 manager.
- Law: a lane naming an adapter the seam did not hand in refuses at `Dial` construction as `drift` on the `adapter` arm — a capability refusal naming the axis, never a guarded path a lane falls through at first dial.
- Law: every adapter, protocol, and hop grades through `Wire.Hops` — the retry gate reads `Fault.Class.retryable` off that one row, so a `Code.Unauthenticated` spends no attempt and a second grading beside it is unspellable.
- Law: a lane names the `value/fault#RETRY_BUDGET` row it recovers on, so recovery geometry has one owner and the ladder spends that row's compiled schedule and attempt ceiling.
- Law: Deadlines end a call at the attempt bound and the plan at the total bound; a stream ends with the scope its caller opened.
- Law: protocol selection occurs once inside the chosen adapter's total factory record; no caller selects a package factory and no protocol × host knob matrix exists.
- Law: the Node adapter passes the one ingress ceiling to all three public factories; the web factories expose no frame-cap option, so no hand wrapper pretends to enforce one.
- Law: unary failover uses `ExecutionPlan`; server streams never retry after an emitted value.
- Law: remote recovery retries only in place; transport connectivity, deadline, or ceiling may advance the execution plan; malformed detail stops.
- Law: carrier headers, tenant scope, HLC, deadlines, and telemetry remain typed Effect context.
- Boundary: the root supplies browser/Bun `fetch`, the optional scoped Node adapter, and the interceptor chain. GET routing is foreclosed because no service this branch binds declares `NO_SIDE_EFFECTS`, so no `useHttpGet` knob exists to misread.

```typescript signature
const _webProtocols = ["connect", "grpc-web"] as const
const _nodeProtocols = ["connect", "grpc-web", "grpc"] as const

// The schema union IS the support matrix. There is no generic protocol field paired with a generic host field, so
// a browser or Bun lane can never carry `grpc`; adding a host or protocol widens one owned arm and breaks every
// adapter capability that has not supplied its public factory.
const _WebLane = Schema.Struct({
  adapter: Schema.Literal("web"),
  protocol: Schema.Literal(..._webProtocols),
  budget: Fault.Budget.schema,
})

const _NodeLane = Schema.Struct({
  adapter: Schema.Literal("node"),
  protocol: Schema.Literal(..._nodeProtocols),
  budget: Fault.Budget.schema,
})

const _PositiveDuration = Schema.DurationFromMillis.pipe(
  Schema.filter((duration) => Duration.toMillis(duration) > 0, { identifier: "PositiveDialDuration" }),
)

// Each lane names its retry OWNER and carries no curve: `budget` is the `value/fault#RETRY_BUDGET` row whose
// compiled schedule and attempt ceiling this ladder spends, so a proxy row and a direct row differ by which row
// they name.
const _LanePolicy = Schema.Union(_WebLane, _NodeLane)

const _TransportPolicy = Schema.Struct({
  attempt: _PositiveDuration,
  total: _PositiveDuration,
})

const _key = (lane: Dial.Lane): Dial.Key => `${lane.adapter}:${lane.protocol}`

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
  type WebProtocol = (typeof _webProtocols)[number]
  type NodeProtocol = (typeof _nodeProtocols)[number]
  type AdapterKind = "web" | "node"
  type Protocol<K extends AdapterKind> = K extends "web" ? WebProtocol : NodeProtocol
  type Lane = Schema.Schema.Type<typeof _LanePolicy>
  type Key = `${AdapterKind}:${WebProtocol | NodeProtocol}`
  type Policy = {
    readonly baseUrl: string
    readonly useBinaryFormat: boolean
    readonly interceptors: ReadonlyArray<Interceptor>
    readonly defaultTimeoutMs: number
    readonly readMaxBytes: number
    readonly writeMaxBytes: number
  }
  type Factories<K extends AdapterKind> = { readonly [P in Protocol<K>]: (policy: Policy) => ConnectTransport }
  type Adapter<K extends AdapterKind> = {
    readonly kind: K
    readonly factories: Factories<K>
  }
  type Config = Schema.Schema.Type<typeof _DialConfig>
  type Seam = {
    readonly web: Option.Option<Adapter<"web">>
    readonly node: Option.Option<Adapter<"node">>
    readonly interceptors: ReadonlyArray<Interceptor>
  }
}

const _web = (fetch: typeof globalThis.fetch): Dial.Adapter<"web"> => ({
  kind: "web",
  factories: {
    connect: (policy) => createConnectTransport({
      baseUrl: policy.baseUrl,
      useBinaryFormat: policy.useBinaryFormat,
      interceptors: [...policy.interceptors],
      defaultTimeoutMs: policy.defaultTimeoutMs,
      fetch,
    }),
    "grpc-web": (policy) => createGrpcWebTransport({
      baseUrl: policy.baseUrl,
      useBinaryFormat: policy.useBinaryFormat,
      interceptors: [...policy.interceptors],
      defaultTimeoutMs: policy.defaultTimeoutMs,
      fetch,
    }),
  },
})

const _unserved = (lane: Dial.Lane): Wire.Fault =>
  new Wire.Fault({
    family: "FaultDetail",
    case: { reason: "drift", divergence: { subject: "adapter", lane: _key(lane) } },
  })

class Lane extends Context.Tag("@rasm/core/Lane")<Lane, {
  readonly key: Dial.Key
  readonly lane: Dial.Lane
  readonly transport: ConnectTransport
}>() {}

const _CONTEXT = {
  stamp: createContextKey<Option.Option<Clock.Hlc>>(Option.none()),
  tenant: createContextKey<Option.Option<Identity.Tenant>>(Option.none()),
} as const

class Ambient extends Context.Reference<Ambient>()("@rasm/core/Dial/Ambient", {
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
): Effect.Effect<O, Wire.InvokeFault> =>
  Effect.flatMap(Effect.all({ context: _charged, headers: _stamped }), ({ context, headers }) =>
    Effect.tryPromise({ try: (signal) => call({ contextValues: context, headers, signal }), catch: Transport.fault }))

const _openStream = <O>(
  open: (options: { readonly contextValues: ContextValues; readonly headers: Headers.Headers; readonly signal: AbortSignal }) => AsyncIterable<O>,
): Stream.Stream<O, Wire.InvokeFault> =>
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

// Remote recovery spends attempts on the current lane only. Transport is the sole topology signal, while malformed
// detail is terminal; no remote domain case is interpreted as a route decision. A transport hop re-drives on the
// ONE code table's class — `Wire.Hops` — so a caller-blamed code spends no attempt on any lane.
const _failsOver = (fault: Wire.InvokeFault): boolean => fault instanceof Wire.Transport
const _retries: Predicate.Predicate<unknown> = (fault) =>
  (fault instanceof Wire.Remote && fault.retryable)
  || (fault instanceof Wire.Transport && Fault.Class.retryable(fault.class))

const _policy = (config: Dial.Config, seam: Dial.Seam): Dial.Policy => ({
  baseUrl: config.baseUrl,
  useBinaryFormat: config.useBinaryFormat,
  interceptors: seam.interceptors,
  defaultTimeoutMs: Duration.toMillis(config.transport.attempt),
  readMaxBytes: Shape.Ingress.floor.bytes,
  writeMaxBytes: Shape.Ingress.floor.bytes,
})

const _selected = (
  policy: Dial.Policy,
  seam: Dial.Seam,
  lane: Dial.Lane,
): Option.Option<ConnectTransport> =>
  Match.value(lane).pipe(
    Match.discriminatorsExhaustive("adapter")({
      web: (selected) => Option.map(seam.web, (adapter) => adapter.factories[selected.protocol](policy)),
      node: (selected) => Option.map(seam.node, (adapter) => adapter.factories[selected.protocol](policy)),
    }),
  )

class Dial extends Effect.Service<Dial>()("@rasm/core/Dial", {
  effect: (config: Dial.Config, seam: Dial.Seam) =>
    Effect.gen(function* () {
      const policy = _policy(config, seam)
      const resolved = yield* Effect.forEach(config.lanes, (row) =>
        Option.match(_selected(policy, seam, row), {
          onNone: () => Effect.fail(_unserved(row)),
          onSome: (transport) => Effect.succeed([row, transport] as const),
        }))
      const transports = HashMap.fromIterable(Array.map(resolved, ([row, transport]) => [_key(row), transport] as const))
      const ladder = Array.map(resolved, ([row, transport]) => ({
        provide: Layer.succeed(Lane, { key: _key(row), lane: row, transport }),
        attempts: Fault.Budget.at(row.budget).attempts,
        schedule: Fault.Budget.schedule(row.budget, _retries),
        while: (fault: Wire.InvokeFault) => Effect.succeed(_failsOver(fault)),
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
  static readonly web: typeof _web = _web
  // `admitted` rides FIRST because it is the precondition, not a decoration: a caller holding
  // only raw descriptor bytes cannot reach this entry at all, so the canonical-pin proof rides in through the value's
  // own provenance rather than as a guard every composition root must remember to run.
  static readonly sdk = <T extends DescService>(admitted: Capability.Admitted, service: T): Effect.Effect<
    Dial.Sdk<T>, ParseResult.ParseError | Wire.Fault, Dial
  > => _sdkOf(admitted, service)
}

type _DialAdapterKind = Dial.AdapterKind
type _DialAdapter<K extends _DialAdapterKind> = Dial.Adapter<K>
type _DialConfig = Dial.Config
type _DialLane = Dial.Lane
type _DialPolicy = Dial.Policy
type _DialSeam = Dial.Seam
```

## [04]-[CAPABILITY_BIND]

- Owner: `Invoke.Capability.admit` mints `Capability.Admitted`, the one value `Invoke.Dial.sdk` derives Connect members from under the generated contract identity handshake.
- Law: pin admission hashes the carried document bytes, and admission order is a type fact rather than a caller convention.
- Law: `Capability.Admitted` is the ONE canonical-document gate — its construction filter relates the pin's descriptor count and roster order to the rows it addresses, so no consumer re-checks either and unit order stays the row's own refinement.
- Law: the pin lands through `Wire.decode("DescriptorPinWire", …)` and this owner re-derives its digest over the document bytes, so the codec landing states shape alone and admission adjudicates here.
- Law: `Capability.Source.contract` records the peer's advertised protobuf package and service family; `Dial.sdk` compares both with the generated `DescService`, and the admitted pin digest names the generation in any refusal.
- Law: parsed rows authorize semantic reads only; the original UTF-8 document remains the identity preimage.
- Law: Connect derivation reads only `DescService`; no descriptor row is inferred as a service or method coordinate.
- Law: derivation spends the admitted pin as PROVENANCE alone — the derived decoder's identifier and every binding refusal name that digest.
- Law: each unary attempt uses the transport timeout, and one outer timeout bounds the complete execution plan.
- Law: server-stream fallback stops after the first emitted value.

```typescript signature
const _capabilityEffects = ["pure", "read", "write", "external", "irreversible"] as const
const _capabilityIdempotency = ["idempotent", "keyed", "single-shot", "non-idempotent"] as const
const _capabilityUnits = ["cpu-millis", "wall-millis", "bytes-egress", "model-tokens", "calls"] as const

const _strictlyOrdered = (values: ReadonlyArray<string>): boolean =>
  Array.every(Array.zip(values, Array.drop(values, 1)), ([was, is]) => was < is)

// One canonical capability-document row: per-row unit order is the ROW's own filter, so no consumer restates it.
const _DescriptorRow = Schema.Struct({
  descriptor: Schema.NonEmptyString,
  surface: Schema.NonEmptyString,
  effect: Schema.Literal(..._capabilityEffects),
  idempotency: Schema.Literal(..._capabilityIdempotency),
  scope: Schema.NonEmptyString,
  units: Schema.Array(Schema.Literal(..._capabilityUnits)).pipe(
    Schema.filter((units) => _strictlyOrdered(units) || "<capability-units-order>"),
  ),
})

// DOCUMENT-level half of canonicality — the pin's own descriptor count and the roster ordering no single row can
// witness; the per-row half is `_DescriptorRow`'s own filter, so the admission class below spends each half once.
const _canonical = (pin: Wire.Decoded<"DescriptorPinWire">, rows: ReadonlyArray<typeof _DescriptorRow.Type>): boolean =>
  rows.length === pin.descriptors && _strictlyOrdered(Array.map(rows, (row) => row.descriptor))

const _descriptorDocument = Format.json.schema(Schema.Array(_DescriptorRow))
const _descriptorBytes = new TextEncoder()

// Every field arrives ALREADY decoded — the pin through `Wire.decode`, the rows through `_descriptorDocument`, the
// contract as the peer's advertised package and service family — so each re-reads its owner's TYPE side rather than minting
// a second encoded shape nothing writes. This value is an in-process admission witness that crosses no wire, and
// `typeSchema` states that at the field instead of in prose.
const _admission = Schema.Struct({
  pin: Schema.typeSchema(Wire.schema("DescriptorPinWire")),
  descriptors: Schema.Array(Schema.typeSchema(_DescriptorRow)),
  contract: Schema.Struct({ package: Schema.NonEmptyString, family: Schema.NonEmptyString }),
})

// THE resolved type. Evaluation takes this owner and nothing else, so a descriptor that never met the network step is
// unrepresentable at `Dial.sdk` rather than refused by a guard a caller could skip. The class is the nominal identity
// this branch admits — `docs/stacks/typescript/shapes.md` makes a standalone exported brand the named defect, so the
// admission fact rides the owner, whose constructor and decode both re-run the filter set below.
class CapabilityAdmitted extends Schema.Class<CapabilityAdmitted>("Capability.Admitted")(
  // Canonicality has ONE owner: `_canonical` relates the pin's count and ordering to the rows, so this class spends it
  // once at construction and at decode alike and restates no predicate.
  _admission.pipe(Schema.filter(
    ({ descriptors, pin }) => _canonical(pin, descriptors) || "<noncanonical-descriptor-pin>",
    { identifier: "CanonicalAdmission" },
  )),
) {}

const _admitCapability = ({ contract, descriptor, pinned }: Capability.Source): Effect.Effect<
  Capability.Admitted,
  ParseResult.ParseError | Wire.Fault
> => Effect.gen(function* () {
  const pin = yield* Wire.decode("DescriptorPinWire", descriptor)
  const octets = _descriptorBytes.encode(pin.document)
  yield* Wire.Parity.verified("DescriptorPinWire", pin.digest, octets)
  yield* Wire.Parity.matched("DescriptorPinWire", pin.digest, pinned)
  const descriptors = yield* Schema.decodeUnknown(_descriptorDocument)(octets)
  return yield* Schema.decode(CapabilityAdmitted)({ contract, descriptors, pin })
})

declare namespace Capability {
  type Source = {
    readonly contract: { readonly package: string; readonly family: string }
    readonly descriptor: Uint8Array
    readonly pinned: Digest.Key<"content">
  }
  type Admitted = CapabilityAdmitted
}

declare namespace Dial {
  type Outcome = Convention.Outcome<Wire.InvokeReason>
  type Sdk<T extends DescService> = {
    readonly [K in keyof Client<T>]: Client<T>[K] extends (input: infer I, options?: infer _O) => Promise<infer O>
      ? (input: I) => Effect.Effect<O, Wire.InvokeFault>
      : Client<T>[K] extends (input: infer I, options?: infer _O) => AsyncIterable<infer O>
        ? (input: I) => Stream.Stream<O, Wire.InvokeFault>
        : never
  }
}

// `identifier` carries the derivation's PROVENANCE: a service name alone answers for every generation that ever
// shipped it, so a binding refusal could not say which pinned document its members were derived against.
const _sdkSchema = <T extends DescService>(admitted: Capability.Admitted, service: T): Schema.Schema<Dial.Sdk<T>> =>
  Schema.declare(
    (input: unknown): input is Dial.Sdk<T> =>
      Predicate.isRecord(input)
      && Array.every(service.methods, (method) => Predicate.isFunction(input[method.localName])),
    { identifier: `${service.typeName}Sdk@${admitted.pin.digest}` },
  )

// Closed boundary rosters preregister, so a posture nothing has raised yet reports zero rather than a panel hole.
const _calls = Convention.mount(Convention.metric.invokeCalls) // module scope: Effect keys an instrument by name and tag set, so a mount inside a fold re-derives one registry entry per call
const _clock = Convention.mount(Convention.metric.invokeDuration)
// `owner` is the ARM that raised, read off the same closed union the reason projection dispatches on, so a governed
// dimension carries a rostered word rather than a coinage: a fourth arm breaks HERE instead of stamping `admission`,
// a name no member of `Wire.InvokeFault` answers to and no query resolves.
const _faultOwner = (fault: Wire.InvokeFault): "malformed-detail" | "remote" | "transport" =>
  fault instanceof Wire.Remote ? "remote" : fault instanceof Wire.Transport ? "transport" : "malformed-detail"

// `domain` plus `case` is the generated fault identity and `posture` the producer's own re-drive verdict, so the
// compact detail publishes exactly the columns a reader dispatches or re-drives on and nothing beside them. Connect
// `Code` remains the transport classification on `Wire.Transport` and never aliases either semantic identity column.
// Every key comes off `Convention.rasm`, so an unrostered `rasm.fault.*` spelling is unspellable at this raise and the
// record types as `Convention.Attributes` rather than as a free string map a sink cannot resolve.
const _faultEvidence = (fault: Wire.InvokeFault): Convention.Attributes =>
  fault instanceof Wire.Remote
    ? {
        [Convention.rasm.faultCase]: fault.detail.case,
        [Convention.rasm.faultDomain]: fault.detail.domain,
        [Convention.rasm.faultOwner]: _faultOwner(fault),
        [Convention.rasm.faultPosture]: fault.recovery.kind,
      }
    : { [Convention.rasm.faultOwner]: _faultOwner(fault) }

// Both census aspects take the raiser's OWN published roster and mount INSIDE the aspect, so no page spells the
// tracking operator beside its own mount and a word no arm produces has no spelling here. `Wire.invokeReason` is the
// one projection every lane reads: the `rejected:` prefix it used to wear said nothing the roster does not already
// say, since `Convention.Fused` proves these words disjoint from the three exit rows rather than a reader stripping
// a decoration to compare them.
const _tracked = Convention.tracked(Convention.metric.invokeFault, Wire.invokeReasons, Wire.invokeReason)
const _counted = Convention.outcome(
  Convention.metric.invokeCalls,
  Convention.rasm.invokeOutcome,
  Wire.invokeReasons,
  Wire.invokeReason,
)

// Stream carries no `onExit` aspect, so the feed lane folds the SAME anchor at its own scope exit; one reason
// projection serves both lanes, so the two exits can never disagree on a word.
const _streamOutcome = (exit: Exit.Exit<unknown, Wire.InvokeFault>): Dial.Outcome =>
  Exit.match(exit, {
    onFailure: (cause) =>
      Cause.isInterruptedOnly(cause)
        ? ("halted" as const)
        : Option.match(Cause.failureOption(cause), { onNone: () => "crashed" as const, onSome: Wire.invokeReason }),
    onSuccess: () => "resolved" as const,
  })

const _observed = (span: string, tags: Convention.Attributes) =>
  <A, R>(self: Effect.Effect<A, Wire.InvokeFault, R>): Effect.Effect<A, Wire.InvokeFault, R> =>
    self.pipe(
      Metric.trackDuration(_clock),
      _tracked,
      Effect.tapError((fault) => Effect.annotateLogs(_faultEvidence(fault))),
      _counted,
      Effect.withSpan(span, { attributes: tags }),
      Effect.annotateLogs(tags),
    )

const _observedStream = (span: string, tags: Convention.Attributes) =>
  <A, R>(self: Stream.Stream<A, Wire.InvokeFault, R>): Stream.Stream<A, Wire.InvokeFault, R> =>
    self.pipe(
      // Stream holds no error-census aspect, so the tap re-offers each fault as a one-shot effect through the SAME
      // `Convention.tracked` operator the rail lane spends — one mount, one projection, no second instrument here.
      Stream.tapError((fault) => Effect.all([
        Effect.ignore(_tracked(Effect.fail(fault))),
        Effect.annotateLogs(_faultEvidence(fault)),
      ])),
      Stream.ensuringWith((exit) => Metric.increment(Metric.tagged(_calls, Convention.rasm.invokeOutcome, _streamOutcome(exit)))),
      Stream.withSpan(span, { attributes: tags }),
    )

// Method kinds Connect cannot bind are drift between the generated service and the capability that admitted it, so the
// refusal names BOTH coordinates. Naming the method alone leaves an operator holding the failure without the pinned
// document that declared the surface it failed against, which is the one fact a re-pin acts on.
const _unbindable = (admitted: Capability.Admitted, method: DescMethod): Wire.Fault =>
  new Wire.Fault({
    family: "FaultDetail",
    case: {
      reason: "drift",
      divergence: {
        subject: "binding",
        actual: { kind: method.methodKind, method: method.localName, service: method.parent.typeName },
        expected: { kinds: "<unary|server-streaming>", pin: admitted.pin.digest },
      },
    },
  })

// Contract identity is package plus service family under the admitted generation digest. Both coordinates compare
// before one method derives; every peer regenerates from the one live shape, so no descriptor walk or CloudEvents
// `dataschema` vocabulary enters this handshake.
const _diverged = (admitted: Capability.Admitted, service: DescService): Wire.Fault =>
  new Wire.Fault({
    family: "FaultDetail",
    case: {
      reason: "drift",
      divergence: {
        subject: "contract",
        advertised: admitted.contract,
        generated: { package: service.file.proto.package, family: service.typeName },
        generation: admitted.pin.digest,
      },
    },
  })

const _laneClient = <T extends DescService>(
  clients: HashMap.HashMap<Dial.Key, Client<T>>,
): Effect.Effect<Client<T>, Wire.Fault, Lane> =>
  Effect.flatMap(Lane, (lane) =>
    Option.match(HashMap.get(clients, lane.key), {
      onNone: () => Effect.fail(_unserved(lane.lane)),
      onSome: (client) => Effect.as(
        Effect.annotateCurrentSpan(Convention.rasm.invokeLane, lane.key),
        client,
      ),
    }))

const _sdkOf = <T extends DescService>(admitted: Capability.Admitted, service: T): Effect.Effect<
  Dial.Sdk<T>, ParseResult.ParseError | Wire.Fault, Dial
> =>
    Effect.gen(function* () {
      yield* Effect.filterOrFail(
        Effect.succeed(service),
        (dialed) => dialed.file.proto.package === admitted.contract.package && dialed.typeName === admitted.contract.family,
        () => _diverged(admitted, service),
      )
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
                      Effect.flatMap(_laneClient(clients), (client) =>
                        dial.unary((options) => client[unary.localName](input, options))),
                      dial.plan,
                    ).pipe(
                    Effect.timeoutFail({
                      duration: dial.totalTimeout,
                      onTimeout: () => Transport.expired(`<total-budget:${unary.localName}>`),
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
                      Effect.map(_laneClient(clients), (client) =>
                        dial.stream((options) => client[streaming.localName](input, options))),
                    ),
                    Stream.withExecutionPlan(dial.plan, { preventFallbackOnPartialStream: true }),
                    _observedStream(`invoke/${service.typeName}/${streaming.localName}`, {
                      [Convention.rasm.invokeMethod]: streaming.localName,
                      [Convention.rasm.invokeService]: service.typeName,
                    }),
                  ),
              ] as const),
            client_streaming: (refused) => Effect.fail(_unbindable(admitted, refused)),
            bidi_streaming: (refused) => Effect.fail(_unbindable(admitted, refused)),
          }),
        ))
      return yield* Schema.decodeUnknown(_sdkSchema(admitted, service))(Record.fromEntries(rows))
    })

// `Capability` publishes its resolved type beside its mint: a caller with no live peer — a test double, a loopback —
// declares a fixture through `Capability.Admitted` and dials on it, which is a visible act at a named decode rather
// than an admission step quietly skipped.
abstract class Capability {
  static readonly Admitted: typeof CapabilityAdmitted = CapabilityAdmitted
  static readonly admit: typeof _admitCapability = _admitCapability
}
```

## [05]-[COMMAND_GATEWAY]

- Owner: `Gateway.make` compiles one command table into value, JSON-byte, MessagePack-socket, and NDJSON-socket adapters.
- Law: each row binds one payload schema, output schema, and handler; the row table derives both closed wire unions.
- Law: every adapter decodes an invocation, delegates to `dispatch`, and encodes the full `Granted | Refused` outcome.
- Law: command bodies are the generated `CommandInvocation` — the corpus's five-arm `CommandPayloadWire` and nothing beside it; tenant and clock context remain on the carrier and interceptor rails.
- Boundary: `Invoke.AvailabilityGate` supplies verdicts, and the runtime supplies socket acquisition and serving lifetime.

```typescript signature
// The invocation is the GENERATED `CommandInvocation` — its payload the corpus's five-arm `CommandPayloadWire`
// oneof, whose `fields` arm is the one open `Struct` on this plane — decoded through its generated descriptor under
// the JSON posture every contract document shares. A row's own payload schema decodes that generated face into the
// row's domain, so the hand five-arm union that once restated the oneof is gone and a sixth arm lands at the corpus.
const CommandInvocation = ui.CommandInvocationSchema
type CommandInvocation = MessageValidType<typeof CommandInvocation>
type CommandPayload = MessageValidType<typeof ui.CommandPayloadWireSchema>

type Dispatched<A> = Data.TaggedEnum<{
  Granted: { readonly verb: string; readonly receipt: A }
  Refused: { readonly verb: string; readonly verdict: Evidence.Availability.Verdict }
}>
interface DispatchedDefinition extends Data.TaggedEnum.WithGenerics<1> {
  readonly taggedEnum: Dispatched<this["A"]>
}
const _Dispatched = Data.taggedEnum<DispatchedDefinition>() // interior constructor: the annotation gate reaches the merged name, so only the type exports

class AvailabilityGate extends Context.Tag("@rasm/core/AvailabilityGate")<AvailabilityGate, {
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

class SupportIntake extends Context.Tag("@rasm/core/SupportIntake")<SupportIntake, {
  readonly deliver: (report: SupportCapture) => Effect.Effect<SupportReceipt>
}>() {}

declare namespace Gateway {
  type Row<B, A, I> = {
    // the row decodes the generated payload face; an unset oneof arm refuses here because the row's own schema
    // admits no `undefined`, and the corpus rule already refused it at the frame
    readonly invocation: Schema.Schema<B, CommandPayload>
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
    readonly invocation: Schema.Schema<CommandInvocation, JsonValue>
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

// Socket frames carry the invocation as its ProtoJSON tree — the same document the bytes arm reads — so one
// generated codec serves the byte seam and both socket dialects.
const _frames = {
  msgpack: <A>(invocation: Schema.Schema<CommandInvocation, JsonValue>, output: Schema.Schema<Dispatched<A>, unknown>) =>
    (socket: Socket.Socket): Gateway.Duplex<A> =>
    MsgPack.duplexSchema({ inputSchema: output, outputSchema: invocation })(
      Socket.toChannel<MsgPack.MsgPackError | ParseResult.ParseError>(socket),
    ),
  ndjson: <A>(invocation: Schema.Schema<CommandInvocation, JsonValue>, output: Schema.Schema<Dispatched<A>, unknown>) =>
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
    // ONE decoder for every verb: the verb roster gates at `dispatch`, where an unrostered key is the `drift` fault
    // below, so the invocation schema admits the generated message and re-spells no per-verb union beside it.
    const invocation: Schema.Schema<CommandInvocation, JsonValue> = Format.proto.json(CommandInvocation)
    const output: Schema.Schema<Dispatched<A[Kinds[number]]>, unknown> = Schema.Union(...Array.map(keys, (key) => Schema.Union(
      Schema.Struct({ _tag: Schema.Literal("Granted"), verb: Schema.Literal(key), receipt: rows[key].output }),
      Schema.Struct({ _tag: Schema.Literal("Refused"), verb: Schema.Literal(key), verdict: Evidence.Availability.Verdict }),
    )))
    const dispatch = Effect.fn("gateway/dispatch")(
      function* (command: CommandInvocation) {
        yield* Effect.annotateCurrentSpan(Convention.rasm.gatewayVerb, command.key)
        const row = yield* Option.match(HashMap.get(table, command.key), {
          onNone: () => Effect.fail(new Wire.Fault({
            family: "CommandInvocation",
            case: { reason: "drift", divergence: { subject: "verb", key: command.key } },
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
      bytes: (octets) => Effect.flatMap(Schema.decodeUnknown(Format.proto.frame(CommandInvocation, "json"))(octets), dispatch),
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
  namespace Dial {
    type AdapterKind = _DialAdapterKind
    type Adapter<K extends AdapterKind> = _DialAdapter<K>
    type Config = _DialConfig
    type Lane = _DialLane
    type Policy = _DialPolicy
    type Seam = _DialSeam
  }
  type Payload = CommandPayload
  type Invocation = CommandInvocation
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
