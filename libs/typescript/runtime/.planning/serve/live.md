# [RUNTIME_LIVE]

This realtime serve plane: SSE and WebSocket endpoints over the branch's own feed values — the data wave's reactive reads, the fanout topics, the core presence fold — under the resume-token law that makes a replayable feed reconnect-exact and under an admission gate that is data in one channel-rule table. This SSE row is the serving mirror of `net/channel#FEED_SEAM`: the same `Sse` codec owns both directions of the dialect — `Sse.makeChannel` decodes on the consuming side, `Sse.encoder` frames on this side — so a hand-assembled `data:` string is unspellable anywhere in the branch. This socket row lifts `HttpServerRequest.upgrade` into one typed duplex channel: `Ndjson.duplexString` frames text lines, `ChannelSchema.duplexUnknown` types both directions, and the frame vocabularies are parameters so a new realtime feature is a frame case at its owner, never a socket edit. Admission guards exactly what the endpoints serve: prefix-matched channel rules resolve scope, presence service, fan cap, and lease policy in one Trie read; the stamp guard pins a decoded `Presence.Op` to the authenticated principal before it reaches the fold; the roster read is a pure verdict against a caller-minted horizon; and the admission plane is constructed ONCE per served app, so the per-principal fan cap holds across every session the principal opens. Foreign realtime protocols arrive through the `Mount` port the route assembly folds — a Connect router behind the same guard every route inherits, and the browser overlay's sync protocol over a storage port the data wave binds. This module ships on the `./server` exports subpath as `runtime/src/serve/live.ts`.

## [01]-[INDEX]

- [02]-[LIVE_FAULT]: `LiveFault` — the realtime refusal family, the resume brand, the resumable-source contract.
- [03]-[SSE_ROW]: `Realtime.sse` — resume decode, encoder framing, heartbeat, lossless bound.
- [04]-[SOCKET_ROW]: `Realtime.socket` — typed duplex framing over the socket channel at the WS upgrade.
- [05]-[FEED_ROWS]: source adapters: reactive query reads, fanout topics, the presence roster stream; `Realtime`.
- [06]-[ADMISSION]: channel rules, subscription grant, stamp guard, roster read, fan registry; `Admission`.
- [07]-[MOUNT_PORT]: `Mount` — the foreign-protocol mount port, its one node-handler lift, and the overlay sync row.

## [02]-[LIVE_FAULT]

[LIVE_FAULT]:
- Packages: `effect` (`Schema`, `Option`, `Stream`); `@rasm/ts/core` (`Fault.Class`).

```typescript signature
import { EventLogServer, Reactivity, Sse } from "@effect/experimental"
import { ChannelSchema, type HttpApp, HttpServerRequest, HttpServerResponse, Ndjson, Socket } from "@effect/platform"
import {
  Channel, Chunk, Context, DateTime, Deferred, Duration, Effect, Either, HashMap, Layer, Option, Ref,
  Schedule, Schema, type Scope, Stream, Trie,
} from "effect"
import type { IncomingMessage, ServerResponse } from "node:http"
import { Clock, Fault, Fold, Identity, Presence } from "@rasm/ts/core"
import { Live } from "@rasm/ts/data"
import { Profile } from "../otel/profile.ts"
import { Fanout } from "../net/pubsub.ts"
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
- Law: both lanes are PROFILE ANCHORS over one band roster this table owns — `realtime/sse` and `realtime/socket` are scoped spans ending with the request scope, which on a streamed response and on an upgraded request alike is the whole live endpoint, so a flamegraph window over either reads exactly the CPU one feed burned. `otel/profile#BANDS`'s effectful arm rides both and, by that owner's law, carries the correlation attribute ALONE: frames interleave across fibers and the engine's label set is thread-global, so no sample label can name a region from here; a genuinely synchronous kernel takes the band's synchronous arm under the same span, and the `span_id` label it writes is what closes the store join the attribute opened.
- Law: the band roster is a value on this page, so a channel outside the lane table and a step outside the three regions refuse at decode — and because both are constants here, that refusal is a code defect and dies rather than widening either endpoint's error channel with an outcome no request can produce.
- Boundary: which feeds exist and who attaches is `[06]`'s admission; the inbound SSE parser is `net/channel#FEED_SEAM`'s — this endpoint only emits.
- Packages: `@effect/experimental` (`Sse`); `@effect/platform` (`HttpServerRequest`, `HttpServerResponse`); `effect` (`Stream`, `Duration`, `Schedule`, `Option`, `Scope`); `../otel/profile.ts` (`Profile`).

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

// The profile band's channel roster IS this table's keys, so a banded region names a lane this page actually serves or
// refuses before the engine sees it; the steps are the three regions a live endpoint owns, which bounds this page's
// profile series by its SHAPE rather than by its traffic — a per-frame step mints one series per frame case, the
// cardinality defect wearing a profile label.
const _BAND: Profile.BandVocabulary = {
  channel: ["sse", "socket"],
  step: ["upgrade", "frame", "fold"],
}

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
  R | R2 | HttpServerRequest.HttpServerRequest | Scope.Scope
> =>
  Profile.banded(_BAND, { channel: "sse", step: "frame" }, Effect.gen(function* () {
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
    // Admission fences the WHOLE frame stream, heartbeat included: a superseding subscribe settles it and
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
  })).pipe(
    // the band roster is a constant on this page, so its refusal is unreachable at runtime and dies rather than
    // widening every feed endpoint's error channel with a parse outcome no request can produce
    Effect.catchTag("ParseError", Effect.die),
    // OUTERMOST, so the span is current when the band reads it. The span ends with the request scope, which on a
    // streamed response is the whole feed — the writer drains inside it — so the profile window this anchor opens
    // covers every frame rather than the handler's own construction.
    Effect.withSpanScoped("realtime/sse"),
  )
```

## [04]-[SOCKET_ROW]

[SOCKET_ROW]:
- Law: `[06]`'s stamp guard pins every decoded op to the principal the `[03]` lead's credential lift bound, so an inbound frame carries no authority its connection did not already hold.
- Law: the `degrade` cell derives from three column facts — `reconnect` is `Option.none` because a peer paces its own backoff, no resume replay exists on this lane at all, and the frame vocabularies are parameters, so a dropped connection resumes only what its own frame owner rebuilds.
- Law: this lane is the branch's DUPLEX profile anchor and the `[03]` lead states the anchor law both lanes share — `realtime/socket` opens at the upgrade so the span covers the connection rather than the handshake, which is what makes the window a duplex's own CPU rather than its first frame's.
- Boundary: what the frames mean and which kernel folds them is the frame owner's; this row owns the span, the upgrade, and the duplex.
- Packages: `@effect/platform` (`Socket`, `Ndjson`, `ChannelSchema`, `HttpServerRequest`); `effect` (`Channel`, `Chunk`, `Deferred`, `Scope`); `../otel/profile.ts` (`Profile`).

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
  HttpServerRequest.HttpServerRequest | Scope.Scope
> =>
  Profile.banded(
    _BAND,
    { channel: "socket", step: "upgrade" },
    Effect.gen(function* () {
      const request = yield* HttpServerRequest.HttpServerRequest
      const socket = yield* request.upgrade.pipe(Effect.mapError(() => new LiveFault({ reason: "closed", detail: "upgrade refused" })))
      return Socket.toChannel(socket).pipe(
        Ndjson.duplexString(),
        ChannelSchema.duplexUnknown({ inputSchema: frames.inbound, outputSchema: frames.outbound }),
        Channel.mapError((cause) => (cause instanceof LiveFault ? cause : new LiveFault({ reason: "closed", detail: String(cause) }))),
        // Both endpoint rows honor this same admission fence: one supersede closes the duplex exactly as it closes an SSE
        Channel.interruptWhenDeferred(fence),
      )
    }),
  ).pipe(
    // the band roster is a constant one line up, so its refusal is unreachable at runtime and dies rather than widening
    // every socket endpoint's error channel with a parse outcome no request can produce
    Effect.catchTag("ParseError", Effect.die),
    // OUTERMOST, so the span is current when the band reads it: one span read feeds the correlation attribute here and
    // the sample labels of every synchronous kernel banded beneath it
    Effect.withSpanScoped("realtime/socket"),
  )
```

## [05]-[FEED_ROWS]

[FEED_ROWS]:
- Law: an adapter carries its source's own error channel and its own requirement — the reactive read fails with whatever its bound query raises and demands `Reactivity.Reactivity`, never a SQL client, because the owner re-runs every bound through the reactive bus and a relational binding touches no relation on an object-plane or in-memory bound.
- Growth: a new feed family (a flag-verdict stream, a vital fact stream) is one adapter over the same contract; the endpoints never change.

```typescript signature
// Every bound's error channel crosses verbatim: a feed carries whatever its query fails with, so hardcoding one
// relational pair here narrowed every non-relational bound to a shape it never raises. Its requirement is the
// reactive bus, not a SQL client — the owner re-runs through `Reactivity.stream`, so a serving Layer satisfying this
// feed with a client alone provides a Tag the stream never asks for and omits the one it does.
const _query = <A, E, R>(
  bound: Live.Bound<A, E, R>,
): Realtime.Source<A, E, Exclude<R, Scope.Scope> | Reactivity.Reactivity> => ({
  from: () => bound.changes,
  // Emission identity projects off the bound: a durable coordinate (a lane's AsOf sequence) rides as the event
  // id and a coordinate-free bound answers none — a DEDUPE token the client proves its rendered state against,
  // never a replay cursor, because every emission already carries the complete answer
  token: (value) => Option.flatMap(bound.coordinate(value), (id) => Schema.decodeOption(_Resume)(id)),
})

const _topic = (topic: string): Realtime.Source<Fanout.Announced, LiveFault, Fanout> => ({
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
                // Slots free on response-scope close, and only while this fence still owns the key — a
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
- Owner: `Mount` — the foreign-protocol rows a served app folds, `Mount.node` the branch's ONE lift from a raw node handler into the `HttpApp.Default` a row carries, and `Mount.overlay` the one row this page mints itself. Fetch-shaped foreign apps need no lift at all, since `HttpApp.fromWebHandler` already answers one, and a second member beside it forks the contract this port exists to hold.
- Law: the lift is a two-part shape because the node pair is runtime capability — `Mount.node(handler)` is the published member every consumer names, and `Mount.Lift` is the port a runtime row satisfies with `NodeHttpServerRequest.toIncomingMessage`/`toServerResponse`; that pair casts `request.source` unchecked, so a fetch-shaped runtime binding it hands the handler an object it cannot drive, and an unbound port reads as refused capability rather than a crash.
- Law: the mounted handler OWNS the raw response, so the lift awaits its close and answers `HttpServerResponse.empty()` — the platform's own writer short-circuits on `writableEnded`, which is what keeps one response from being written twice; a lift returning a body instead writes headers a handler already sent.
- Law: `serve/route#LAYER_ROUTES` composes this same member for the tus rail's node dispatchers, so the adapter has one spelling and `Router.RailMount` keeps the Layer assembly — prefix routing and groom scheduling — that no lift decides.
- Law: a row's app may demand `Scope.Scope`, because a mounted protocol that upgrades its request lives for the CONNECTION and its fibers must die with it; the router provides the request scope to every route it matches, so the widened row type costs a non-upgrading row nothing and an upgrading row needs no second row family.
- Law: a Connect row carries its own RPC-level cover beside the HTTP-level one — `ConnectNodeAdapterOptions` declares `routes`, `fallback`, `requestPathPrefix`, and `contextValues` alone, and reaches `interceptors`, `readMaxBytes`, `writeMaxBytes`, `compressMinBytes`, `maxTimeoutMs`, `shutdownSignal`, and `requireConnectProtocolHeader` by extending `ConnectRouterOptions extends Partial<UniversalHandlerOptions>` — so per-procedure policy rides the adapter while `Seam.guard` covers every mounted row alike from above the catch-all.
- Law: the inheritance is a RE-PROVE-ON-BUMP trap, not a field — `UniversalHandlerOptions` carries the package's own internal-and-outside-semver marker and exports from the `@connectrpc/connect/protocol` subpath alone, and the member that actually carries a row's `interceptors` into the protocol handlers is `validateUniversalHandlerOptions`'s own copy; a bump that stops copying drops every server interceptor with no type error, so the composing fence re-proves that copy rather than trusting the option's presence.
- Law: the Connect row continues NO context of its own — `Seam.guard` already ran `Current.traced` over the same HTTP headers this adapter reads, above the catch-all and before the handler, so the inbound W3C hop crossed once already; a Connect-side `Carrier.extract` interceptor mints a SECOND continuation of one hop, which is the two-trace law's forbidden fold rather than its admitted pair. Egress print is `core:interchange/invoke#DIAL_AXIS`'s, so no interceptor pair rides this row in either direction.
- Law: a server interceptor wraps the IMPLEMENTATION invocation, never the HTTP exchange — it runs after protocol negotiation and message decode, its `req.header` IS the inbound header bag and its `req.contextValues` IS the `ContextValues` the handler then reads, and on a streaming method `next` settles when the response iterable is CONSTRUCTED rather than consumed; so an interceptor seats per-call policy and can never bracket a streaming body, and a region spanning one belongs to the handler.
- Law: `contextValues` is the EARLIER seam and the one a principal crosses — it runs against the raw node request before any handler dispatch, so the credential and tenancy `Seam.admission` already bound reach `HandlerContext` there rather than through an interceptor that runs a decode later.
- Law: a row's `prefix` and its adapter's `requestPathPrefix` are ONE value — the router mounts the row under `${prefix}/*` and the adapter matches `prefix + requestPath`, so a row spelling them apart serves a path no client can reach and answers the adapter's own 404 for every call.
- Law: the adapter never rejects into the lift — a handler fault renders as a Connect wire error on the response and a transport fault reaches the package's own console sink, so this row's `LiveFault` rail carries LIFT faults alone and no `Problem` renders for a matched RPC path; the mount's error contract is the connection, not the call.
- Law: `Mount.overlay(prefix)` serves the browser overlay's sync protocol at the front door — `EventLogServer.makeHandlerHttp` is an acquisition over `EventLogServer.Storage` yielding the app, so the row is an EFFECT and the server's own remote identity mints once per process rather than once per socket; the app upgrades its request and answers `HttpServerResponse.empty()`, which is why the row type admits the request scope.
- Law: `EventLogServer.Storage` is a PORT this page names and never binds — the composition root satisfies it from the data wave's own `SqlClient` scope, and the memory row is the spec seat alone. A serve-side relation, a second store, or a storage arm spelled here is the boundary breach `browser/persist#OVERLAY_AND_LANE`'s overlay law already forecloses from the other end.
- Law: the mount is ZERO-KNOWLEDGE by the protocol's own shape — the storage port moves `PersistedEntry` (entry id, iv, ciphertext) and answers encrypted remote entries, so no key material and no plaintext crosses this row and the browser holds the only key; a server-side decrypt is unspellable rather than merely refused.
- Law: the sync path is ONE value across the two ends — this row's `prefix` is exactly the URL `browser/persist#OVERLAY_AND_LANE`'s `Overlay.sync` dials under a socket scheme, so the root derives both from one origin row and neither end hand-types the other's path.
- Boundary: upgrade mechanics inside the mounted app are the satisfier's; this page owns the Tag, the lift, the row roster, and their contract.
- Packages: `@effect/platform` (`HttpApp`, `HttpServerResponse`); `effect` (`Context`, `Layer`, `Scope`); `node:http` types alone; each satisfier's own packages stay at the app root.

```typescript signature
declare namespace Mount {
  // Structural shape every foreign node adapter already answers — `connectNodeAdapter` returns exactly it — so this
  // page names no adapter package and each satisfier keeps its own at the root.
  type NodeHandler = (request: IncomingMessage, response: ServerResponse) => void
  type Row = { readonly prefix: `/${string}`; readonly app: HttpApp.Default<LiveFault, Mount.Lift | Scope.Scope> }
  type Lift = _Lift
}

class _Lift extends Context.Tag("runtime/serve/Mount/Lift")<_Lift, {
  readonly node: (handler: Mount.NodeHandler) => Effect.Effect<void, LiveFault, HttpServerRequest.HttpServerRequest>
}>() {}

// Rows raising nothing and demanding nothing still inhabit the widened row type, so admitting the lift costs an
// existing mount no edit while a lifted row stops needing a parallel row family of its own.
const _mounted = (handler: Mount.NodeHandler): HttpApp.Default<LiveFault, Mount.Lift> =>
  Effect.as(Effect.flatMap(_Lift, (lift) => lift.node(handler)), HttpServerResponse.empty())

// The remote identity and the chunk counter are SERVER facts the built handler closes over, so this row folds the
// acquisition once and hands the built app to every connection: a row spelled as a per-request `makeHandlerHttp`
// re-reads `storage.getId` on each socket and restarts the counter beside it. Storage stays a PORT — the row names
// the Tag and the composition root binds it from the data wave, so no relation is spelled here.
const _overlay = (prefix: `/${string}`): Effect.Effect<Mount.Row, never, EventLogServer.Storage> =>
  Effect.map(EventLogServer.makeHandlerHttp, (served): Mount.Row => ({
    prefix,
    app: Effect.mapError(served, (cause) => new LiveFault({ reason: "closed", detail: String(cause) })),
  }))

class Mount extends Context.Tag("runtime/serve/Mount")<Mount, ReadonlyArray<Mount.Row>>() {
  static readonly Lift = _Lift
  static readonly node = _mounted
  static readonly overlay = _overlay
  // Rows are EFFECTS uniformly, because a row that acquires — one server identity, one built handler — cannot be a
  // value the root already holds, and a pure row answers `Effect.succeed`. Two constructors here would fork the Tag's
  // one binding into an acquiring form and a pure form the root then has to choose between.
  static readonly of = <R>(...rows: ReadonlyArray<Effect.Effect<Mount.Row, never, R>>): Layer.Layer<Mount, never, R> =>
    Layer.effect(Mount, Effect.all(rows))
}

const Realtime = {
  Resume: _Resume,
  // the anchors' own roster publishes beside the lane table, so a frame owner's synchronous kernel bands under the
  // same vocabulary its endpoint span already declared and the store joins one region name rather than two
  band: _BAND,
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
