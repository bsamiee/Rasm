# [RUNTIME_LIVE]

This realtime serve plane: SSE and WebSocket endpoints over the branch's own feed values — the data wave's reactive reads, the fanout topics, the core presence fold — under the resume-token law that makes a replayable feed reconnect-exact and under an admission gate that is data in one channel-rule table. This SSE row is the serving mirror of `net/channel#FEED_SEAM`: the same `Sse` codec owns both directions of the dialect — `Sse.makeChannel` decodes on the consuming side, `Sse.encoder` frames on this side — so a hand-assembled `data:` string is unspellable anywhere in the branch. This socket row lifts `HttpServerRequest.upgrade` into one typed duplex channel: `Ndjson.duplexString` frames text lines, `ChannelSchema.duplexUnknown` types both directions, and the frame vocabularies are parameters so a new realtime feature is a frame case at its owner, never a socket edit. Admission guards exactly what the endpoints serve: prefix-matched channel rules resolve scope, presence service, fan cap, and lease policy in one Trie read; the stamp guard pins a decoded `Presence.Op` to the authenticated principal before it reaches the fold; the roster read is a pure verdict against a caller-minted horizon; and the admission plane is constructed ONCE per served app, so the per-principal fan cap holds across every session the principal opens. Foreign realtime protocols arrive through the `Mount` port the route assembly folds. This module ships on the `./server` exports subpath as `runtime/src/serve/live.ts`.

## [01]-[INDEX]

- [02]-[LIVE_FAULT]: the realtime refusal family, the resume brand, the resumable-source contract; `LiveFault`.
- [03]-[SSE_ROW]: the SSE endpoint fold: resume decode, encoder framing, heartbeat, lossless bound; `Realtime`.
- [04]-[SOCKET_ROW]: the WS upgrade fold: typed duplex framing over the socket channel; `Realtime`.
- [05]-[FEED_ROWS]: source adapters: reactive query reads, fanout topics, the presence roster stream; `Realtime`.
- [06]-[ADMISSION]: channel rules, subscription grant, stamp guard, roster read, fan registry; `Admission`.
- [07]-[MOUNT_PORT]: the foreign-protocol mount port and its one node-handler lift; `Mount`.

## [02]-[LIVE_FAULT]

[LIVE_FAULT]:
- Packages: `effect` (`Schema`, `Option`, `Stream`); `@rasm/ts/core` (`Fault.Class`).

```typescript signature
import { Reactivity, Sse } from "@effect/experimental"
import { ChannelSchema, type HttpApp, HttpServerRequest, HttpServerResponse, Ndjson, Socket } from "@effect/platform"
import {
  Channel, Chunk, Context, DateTime, Deferred, Duration, Effect, Either, HashMap, Layer, Option, Ref,
  Schedule, Schema, type Scope, Stream, Trie,
} from "effect"
import type { IncomingMessage, ServerResponse } from "node:http"
import { Clock, Fault, Fold, Identity, Presence } from "@rasm/ts/core"
import { Live } from "@rasm/ts/data"
import { Envelope, Fanout } from "../net/pubsub.ts"
import { Principal } from "./api.ts"

// One row per reason: the core kind alone. Retryability, blame, and the response code stay the core row
// table's and problem#STATUS_RECORD's, so no local policy column rides beside `class`.
const _live = Fault.Class.family(["denied", "shed", "lost", "closed"] as const, {
  denied: { class: "denied" },
  shed: { class: "unavailable" },
  lost: { class: "conflicted" },
  closed: { class: "unavailable" },
})

declare namespace LiveFault {
  type Reason = (typeof _live.reasons)[number]
}

class LiveFault extends Schema.TaggedError<LiveFault>()("LiveFault", {
  reason: _live.schema,
  detail: Schema.String,
}) {
  get class(): Fault.Class.Kind {
    return _live.classOf(this.reason)
  }
  override get message(): string {
    return `<live:${this.reason}> ${this.detail}`
  }
}

const _Resume = Schema.NonEmptyString.pipe(Schema.maxLength(256), Schema.pattern(/^[A-Za-z0-9_\-.:]+$/), Schema.brand("Resume"))

declare namespace Realtime {
  type Resume = typeof _Resume.Type
  type Source<A, E, R> = {
    readonly from: (resume: Option.Option<Resume>) => Stream.Stream<A, E, R>
    readonly token: (item: A) => Option.Option<Resume>
  }
  // Consumption-descriptor columns lead, realtime extensions follow; an `Option.none` is the lane declaring it
  // decides that coordinate NOTHING, never a zero a caller then reads as a real bound. `tenancy` carries no
  // column because neither lane resolves one — the cluster lead states the mechanism both ride.
  type Lane = {
    readonly fits: string
    readonly admit: string
    readonly lifetime: string
    readonly degrade: string
    readonly duplex: boolean
    readonly beat: Option.Option<Duration.Duration>
    readonly lag: Option.Option<number>
    readonly reconnect: Option.Option<Duration.Duration>
  }
}
```

## [03]-[SSE_ROW]

[SSE_ROW]:
- Owner: `_lanes` — the two realtime lanes as rows over one consumption-descriptor column set beside the realtime extensions, so a caller reads selection, entry, ending owner, forfeit, and pacing off the same value the endpoint folds; `Realtime.lanes` publishes them.
- Law: neither lane resolves a tenant, so `tenancy` rides this lead rather than a column — `Seam.admission`'s one credential lift binds `TenantScope` around the dispatch and `[06]`'s roster read filters on that bound scope, whatever consumption row the root supplied.
- Law: `reconnect` emits as the FIRST frame — `Sse.Retry` is the `retry:` directive, the one lever a server holds over reconnect pacing, so a drain or a supersede storm returns at a paced interval instead of every engine's own default; the frame carries the resume token the request presented, so a reconnect after an idle gap re-declares where the client stood.
- Law: the `degrade` cell derives from the mechanism, not a judgement — the response flushes its status and headers before the first frame, so a mid-stream refusal drops the connection rather than rendering a `Problem`.
- Boundary: which feeds exist and who attaches is `[06]`'s admission; the inbound SSE parser is `net/channel#FEED_SEAM`'s — this endpoint only emits.
- Packages: `@effect/experimental` (`Sse`); `@effect/platform` (`HttpServerRequest`, `HttpServerResponse`); `effect` (`Stream`, `Duration`, `Schedule`, `Option`).

```typescript signature
const _lanes = {
  sse: {
    fits: "<server-pushed-feed:browser-consumes:no-client-frame>",
    admit: "<route-match+last-event-id-decode>",
    lifetime: "<the-response:server-ends-alone-through-drain-or-supersede-fence>",
    degrade: "<no-client-frame:server-ends-alone:post-flush-refusal-drops-connection>",
    duplex: false,
    beat: Option.some(Duration.seconds(25)),
    lag: Option.some(64),
    reconnect: Option.some(Duration.seconds(5)),
  },
  // Socket peers pace their own backoff and frame their own pings, so this lane DECIDES neither; `Ndjson.duplexString`
  // backpressures the duplex natively, which is why no lag capacity rides here either.
  socket: {
    fits: "<feature-framing-both-directions>",
    admit: "<route-match+HttpServerRequest.upgrade>",
    lifetime: "<the-connection:either-peer-ends-or-the-supersede-fence-settles>",
    degrade: "<either-peer-ends:no-resume-replay:no-server-paced-reconnect>",
    duplex: true,
    beat: Option.none<Duration.Duration>(),
    lag: Option.none<number>(),
    reconnect: Option.none<Duration.Duration>(),
  },
} as const satisfies Record<string, Realtime.Lane>

const _ResumeHeader = Schema.Struct({
  "last-event-id": Schema.optionalWith(_Resume, { as: "Option" }),
})

const _BEAT: Sse.Event = { _tag: "Event", event: "ping", id: undefined, data: "{}" }

// `Sse.AnyEvent` is the encoder's own union, so the retry directive and the data frames ride ONE stream and one
// writer; a hand-assembled `retry:` line beside the encoder forks the dialect this page exists to own.
const _encoded = <E, R>(frames: Stream.Stream<Sse.AnyEvent, E, R>): Stream.Stream<Uint8Array, E, R> =>
  Stream.encodeText(Stream.map(frames, (event) => Sse.encoder.write(event)))

const _retry = (resume: Option.Option<Realtime.Resume>): Stream.Stream<Sse.AnyEvent> =>
  Option.match(_lanes.sse.reconnect, {
    onNone: () => Stream.empty,
    onSome: (duration) =>
      Stream.make(new Sse.Retry({ duration, lastEventId: Option.getOrUndefined(resume) })),
  })

const _sse = <A, I, E, R, R2>(
  name: string,
  item: Schema.Schema<A, I, R2>,
  source: Realtime.Source<A, E, R>,
  fence: Deferred.Deferred<void>,
): Effect.Effect<
  HttpServerResponse.HttpServerResponse,
  LiveFault,
  R | R2 | HttpServerRequest.HttpServerRequest
> =>
  Effect.gen(function* () {
    const attested = yield* HttpServerRequest.schemaHeaders(_ResumeHeader).pipe(
      Effect.mapError(() => new LiveFault({ reason: "lost", detail: "resume token refused" })),
    )
    const context = yield* Effect.context<R | R2>()
    const encode = Schema.encode(item)
    const events = source.from(attested["last-event-id"]).pipe(
      Stream.mapEffect((held) =>
        encode(held).pipe(
          Effect.map((body): Sse.Event => ({
            _tag: "Event",
            event: name,
            id: Option.getOrUndefined(source.token(held)),
            data: JSON.stringify(body),
          })),
          Effect.orDie,
        )),
      Stream.mapError((cause) => (cause instanceof LiveFault ? cause : new LiveFault({ reason: "closed", detail: String(cause) }))),
      Stream.buffer({ capacity: Option.getOrElse(_lanes.sse.lag, () => 1), strategy: "suspend" }),
    )
    // the admission fence bounds the WHOLE frame stream, heartbeat included: a superseding subscribe settles it and
    // this response ends, so the plane-level slot the successor took is never held by two live feeds at once
    const beat = Option.match(_lanes.sse.beat, {
      onNone: () => Stream.empty as Stream.Stream<Sse.AnyEvent>,
      onSome: (every) => Stream.repeatEffectWithSchedule(Effect.succeed(_BEAT), Schedule.spaced(every)),
    })
    const framed = Stream.merge(events, beat, { haltStrategy: "left" }).pipe(Stream.interruptWhenDeferred(fence))
    // Directives lead the response: a client reconnecting off a dropped feed reads its pacing before any datum,
    // so the interval holds even when the feed refuses on its first pull
    const opened = Stream.concat(_retry(attested["last-event-id"]), framed)
    return HttpServerResponse.stream(Stream.provideContext(_encoded(opened), context)).pipe(
      HttpServerResponse.setHeaders({ "content-type": "text/event-stream", "cache-control": "no-cache", connection: "keep-alive" }),
    )
  })
```

## [04]-[SOCKET_ROW]

[SOCKET_ROW]:
- Law: `[06]`'s stamp guard pins every decoded op to the principal the `[03]` lead's credential lift bound, so an inbound frame carries no authority its connection did not already hold.
- Law: the `degrade` cell derives from three column facts — `reconnect` is `Option.none` because a peer paces its own backoff, no resume replay exists on this lane at all, and the frame vocabularies are parameters, so a dropped connection resumes only what its own frame owner rebuilds.
- Packages: `@effect/platform` (`Socket`, `Ndjson`, `ChannelSchema`, `HttpServerRequest`); `effect` (`Channel`, `Chunk`, `Deferred`).

```typescript signature
const _socket = <In, IEnc, Out, OEnc, RIn, ROut>(
  frames: {
    readonly inbound: Schema.Schema<In, IEnc, RIn>
    readonly outbound: Schema.Schema<Out, OEnc, ROut>
  },
  fence: Deferred.Deferred<void>,
): Effect.Effect<
  Channel.Channel<Chunk.Chunk<In>, Chunk.Chunk<Out>, LiveFault, unknown, void, unknown, RIn | ROut>,
  LiveFault,
  HttpServerRequest.HttpServerRequest
> =>
  Effect.gen(function* () {
    const request = yield* HttpServerRequest.HttpServerRequest
    const socket = yield* request.upgrade.pipe(Effect.mapError(() => new LiveFault({ reason: "closed", detail: "upgrade refused" })))
    return Socket.toChannel(socket).pipe(
      Ndjson.duplexString(),
      ChannelSchema.duplexUnknown({ inputSchema: frames.inbound, outputSchema: frames.outbound }),
      Channel.mapError((cause) => (cause instanceof LiveFault ? cause : new LiveFault({ reason: "closed", detail: String(cause) }))),
      // the same admission fence both endpoint rows honor: one supersede closes the duplex exactly as it closes an SSE
      Channel.interruptWhenDeferred(fence),
    )
  })
```

## [05]-[FEED_ROWS]

[FEED_ROWS]:
- Law: an adapter carries its source's own error channel and its own requirement — the reactive read fails with whatever its bound query raises and demands `Reactivity.Reactivity`, never a SQL client, because the owner re-runs every bound through the reactive bus and a relational binding touches no relation on an object-plane or in-memory bound.
- Growth: a new feed family (a flag-verdict stream, a vital fact stream) is one adapter over the same contract; the endpoints never change.

```typescript signature
// The bound's own error channel crosses verbatim: a feed carries whatever its query fails with, so hardcoding one
// relational pair here narrowed every non-relational bound to a shape it never raises. Its requirement is the
// reactive bus, not a SQL client — the owner re-runs through `Reactivity.stream`, so a serving Layer satisfying this
// feed with a client alone provides a Tag the stream never asks for and omits the one it does.
const _query = <A, E, R>(
  bound: Live.Bound<A, E, R>,
): Realtime.Source<A, E, Exclude<R, Scope.Scope> | Reactivity.Reactivity> => ({
  from: () => bound.changes,
  // the bound's own emission-identity projection: a durable coordinate (a lane's AsOf sequence) rides as the event
  // id and a coordinate-free bound answers none — a DEDUPE token the client proves its rendered state against,
  // never a replay cursor, because every emission already carries the complete answer
  token: (value) => Option.flatMap(bound.coordinate(value), (id) => Schema.decodeOption(_Resume)(id)),
})

const _topic = (topic: string): Realtime.Source<Envelope, LiveFault, Fanout> => ({
  from: (resume) =>
    Stream.unwrap(
      Effect.map(Fanout, (fanout) =>
        Option.match(resume, {
          onNone: () => fanout.subscribe(topic),
          onSome: () => Stream.fail(new LiveFault({ reason: "lost", detail: topic })),
        })),
    ).pipe(
      Stream.mapError((fault) =>
        fault instanceof LiveFault
          ? fault
          : fault.reason === "horizon"
            ? new LiveFault({ reason: "lost", detail: topic })
            : new LiveFault({ reason: "closed", detail: topic })),
    ),
  token: () => Option.none(),
})

const _roster = <E, R>(
  feed: Stream.Stream<Fold.Table<Presence.Key, Presence.State>, E, R>,
  tenant: Identity.Tenant.Scope,
  lease: Presence.Lease,
): Realtime.Source<HashMap.HashMap<Presence.Actor, Presence.Status>, E, R> => ({
  from: () =>
    Stream.mapEffect(feed, (table) =>
      Effect.map(DateTime.now, (now) => {
        const horizon = Clock.Hlc.tick(Clock.Hlc.genesis, Clock.Hlc.physicalOf(now))
        return HashMap.reduce(
          Presence.roster(HashMap.filter(table, (_state, [scope]) => scope === tenant), horizon, lease),
          HashMap.empty<Presence.Actor, Presence.Status>(),
          (roster, status, [, actor]) => HashMap.set(roster, actor, status),
        )
      })),
  token: () => Option.none(),
})
```

## [06]-[ADMISSION]

[ADMISSION]:
- Growth: a new admission axis (a payload ceiling, a rate row) is one `Rule` field read at its gate; a new channel family is one app-side row.

```typescript signature
const _Channel = Schema.NonEmptyString.pipe(Schema.maxLength(128), Schema.pattern(/^[a-z0-9][a-z0-9:_-]*$/), Schema.brand("Channel"))

declare namespace Admission {
  type Channel = typeof _Channel.Type
  type Rule = _Rule
  type Grant = { readonly channel: Channel; readonly rule: Rule }
}

class _Rule extends Schema.Class<_Rule>("Admission/Rule")({
  scope: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
  presence: Schema.Boolean,
  fan: Schema.Int.pipe(Schema.positive()),
  lease: Presence.Lease,
}) {}

const _admit = (rules: Trie.Trie<Admission.Rule>) =>
  (principal: Principal.Shape, channel: Admission.Channel): Effect.Effect<Admission.Grant, LiveFault> =>
    Option.match(Trie.longestPrefixOf(rules, channel), {
      onNone: () => Effect.fail(new LiveFault({ reason: "denied", detail: channel })),
      onSome: ([, rule]) =>
        Option.match(rule.scope, {
          onNone: () => Effect.succeed({ channel, rule }),
          onSome: (scope) =>
            Principal.allows(principal, scope)
              ? Effect.succeed({ channel, rule })
              : Effect.fail(new LiveFault({ reason: "denied", detail: scope })),
        }),
    })

const _guard = (
  grant: Admission.Grant,
  principal: Principal.Shape,
  forward: (op: Presence.Op) => Effect.Effect<void>,
) =>
  (op: Presence.Op): Effect.Effect<void, LiveFault> =>
    !grant.rule.presence
      ? Effect.fail(new LiveFault({ reason: "denied", detail: grant.channel }))
      : op.actor !== principal.subject || !Option.exists(principal.tenant, (tenant) => tenant === op.tenant.tenant)
        ? Effect.fail(new LiveFault({ reason: "denied", detail: op.actor }))
        : forward(op)

// one atomic verdict over the whole cell: `left` is the refused cap, `right` carries the superseded incumbent's
// fence when a held key re-reserved and `none` when the slot was fresh — three outcomes one boolean cannot spell
const _reserved = (
  cell: Ref.Ref<HashMap.HashMap<Fold.Cell, readonly [subject: string, fence: Deferred.Deferred<void>]>>,
  key: Fold.Cell,
  subject: string,
  fan: number,
  fence: Deferred.Deferred<void>,
): Effect.Effect<Either.Either<Option.Option<Deferred.Deferred<void>>, void>> =>
  Ref.modify(cell, (slots) =>
    Option.match(HashMap.get(slots, key), {
      // a held key re-reserves from ANY session of this principal and never charges the cap twice
      onSome: ([, incumbent]) => [Either.right(Option.some(incumbent)), HashMap.set(slots, key, [subject, fence])] as const,
      onNone: () =>
        HashMap.size(HashMap.filter(slots, ([owner]) => owner === subject)) >= fan
          ? ([Either.left<void>(undefined), slots] as const)
          : ([Either.right(Option.none<Deferred.Deferred<void>>()), HashMap.set(slots, key, [subject, fence])] as const),
    }))

const _make = (rows: ReadonlyArray<readonly [prefix: Admission.Channel, rule: Admission.Rule]>) =>
  Effect.gen(function* () {
    const rules = Trie.fromIterable(rows)
    const cell = yield* Ref.make(
      HashMap.empty<Fold.Cell, readonly [subject: string, fence: Deferred.Deferred<void>]>()
    )
    const admit = _admit(rules)
    const subscribe = (
      grant: Admission.Grant,
      principal: Principal.Shape,
    ): Effect.Effect<Deferred.Deferred<void>, LiveFault, Scope.Scope> =>
      Effect.gen(function* () {
        const key = Fold.cell([principal.subject, grant.channel])
        const fence = yield* Deferred.make<void>()
        const verdict = yield* _reserved(cell, key, principal.subject, grant.rule.fan, fence)
        return yield* Either.match(verdict, {
          onLeft: () => Effect.fail(new LiveFault({ reason: "shed", detail: key })),
          onRight: (incumbent) =>
            Effect.as(
              Effect.zipRight(
                // settling the predecessor's fence interrupts ITS served stream, so a supersede closes a response
                // rather than leaving a slot the cap still counts and nothing can reach
                Option.match(incumbent, { onNone: () => Effect.void, onSome: (spent) => Deferred.succeed(spent, undefined) }),
                // the slot frees on response-scope close, and only while this fence still owns the key — a
                // successor that already superseded keeps its own reservation
                Effect.addFinalizer(() =>
                  Ref.update(cell, (slots) =>
                    Option.match(HashMap.get(slots, key), {
                      onNone: () => slots,
                      onSome: ([, current]) => (current === fence ? HashMap.remove(slots, key) : slots),
                    }))),
              ),
              fence,
            ),
        })
      })
    return { admit, guard: _guard, subscribe } as const
  })

const Admission = { Channel: _Channel, Rule: _Rule, make: _make } as const
```

## [07]-[MOUNT_PORT]

[MOUNT_PORT]:
- Owner: `Mount` — the foreign-protocol rows a served app folds, and `Mount.node` the branch's ONE lift from a raw node handler into the `HttpApp.Default` a row carries. Fetch-shaped foreign apps need no lift at all, since `HttpApp.fromWebHandler` already answers one, and a second member beside it forks the contract this port exists to hold.
- Law: the lift is a two-part shape because the node pair is runtime capability — `Mount.node(handler)` is the published member every consumer names, and `Mount.Lift` is the port a runtime row satisfies with `NodeHttpServerRequest.toIncomingMessage`/`toServerResponse`; that pair casts `request.source` unchecked, so a fetch-shaped runtime binding it hands the handler an object it cannot drive, and an unbound port reads as refused capability rather than a crash.
- Law: the mounted handler OWNS the raw response, so the lift awaits its close and answers `HttpServerResponse.empty()` — the platform's own writer short-circuits on `writableEnded`, which is what keeps one response from being written twice; a lift returning a body instead writes headers a handler already sent.
- Law: `serve/route#LAYER_ROUTES` composes this same member for the tus rail's node dispatchers, so the adapter has one spelling and `Router.RailMount` keeps the Layer assembly — prefix routing and groom scheduling — that no lift decides.
- Law: a Connect row carries its own RPC-level cover beside the HTTP-level one — `ConnectNodeAdapterOptions` reaches `interceptors`, `readMaxBytes`, `writeMaxBytes`, and `compressMinBytes` through `@connectrpc/connect/protocol`'s exported `UniversalHandlerOptions`, so per-procedure policy rides the adapter while `Seam.guard` covers every mounted row alike from above the catch-all.
- Boundary: upgrade mechanics inside the mounted app are the satisfier's; this page owns the Tag, the lift, and their contract.
- Packages: `@effect/platform` (`HttpApp`, `HttpServerResponse`); `effect` (`Context`, `Layer`); `node:http` types alone; each satisfier's own packages stay at the app root.

```typescript signature
declare namespace Mount {
  // Structural shape every foreign node adapter already answers — `connectNodeAdapter` returns exactly it — so this
  // page names no adapter package and each satisfier keeps its own at the root.
  type NodeHandler = (request: IncomingMessage, response: ServerResponse) => void
  type Row = { readonly prefix: `/${string}`; readonly app: HttpApp.Default<LiveFault, Mount.Lift> }
  type Lift = _Lift
}

class _Lift extends Context.Tag("runtime/serve/Mount/Lift")<_Lift, {
  readonly node: (handler: Mount.NodeHandler) => Effect.Effect<void, LiveFault, HttpServerRequest.HttpServerRequest>
}>() {}

// Rows raising nothing and demanding nothing still inhabit the widened row type, so admitting the lift costs an
// existing mount no edit while a lifted row stops needing a parallel row family of its own.
const _mounted = (handler: Mount.NodeHandler): HttpApp.Default<LiveFault, Mount.Lift> =>
  Effect.as(Effect.flatMap(_Lift, (lift) => lift.node(handler)), HttpServerResponse.empty())

class Mount extends Context.Tag("runtime/serve/Mount")<Mount, ReadonlyArray<Mount.Row>>() {
  static readonly Lift = _Lift
  static readonly node = _mounted
  static readonly of = (...rows: ReadonlyArray<Mount.Row>): Layer.Layer<Mount> => Layer.succeed(Mount, rows)
}

const Realtime = {
  Resume: _Resume,
  lanes: _lanes,
  query: _query,
  roster: _roster,
  socket: _socket,
  sse: _sse,
  topic: _topic,
} as const

// --- [EXPORTS] --------------------------------------------------------------------------

export { Admission, LiveFault, Mount, Realtime }
```

## [08]-[RESEARCH]

(none)
