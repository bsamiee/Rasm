# [RUNTIME_LIVE]

This realtime serve plane: SSE and WebSocket endpoints over the branch's own feed values — the data wave's reactive reads, the fanout topics, the core presence fold — under the resume-token law that makes a replayable feed reconnect-exact and under an admission gate that is data in one channel-rule table. This SSE row is the serving mirror of `net/channel#FEED_SEAM`: the same `Sse` codec owns both directions of the dialect — `Sse.makeChannel` decodes on the consuming side, `Sse.encoder` frames on this side — so a hand-assembled `data:` string is unspellable anywhere in the branch. This socket row lifts `HttpServerRequest.upgrade` into one typed duplex channel: `Ndjson.duplexString` frames text lines, `ChannelSchema.duplexUnknown` types both directions, and the frame vocabularies are parameters so a new realtime feature is a frame case at its owner, never a socket edit. Admission guards exactly what the endpoints serve: prefix-matched channel rules resolve scope, presence service, fan cap, and lease policy in one Trie read; the stamp guard pins a decoded `Presence.Op` to the authenticated principal before it reaches the fold; the roster read is a pure verdict against a caller-minted horizon; and the admission plane is constructed ONCE per served app, so the per-principal fan cap holds across every session the principal opens. Foreign realtime protocols arrive through the `Mount` port the route assembly folds. This module ships on the `./server` exports subpath as `runtime/src/serve/live.ts`.

## [01]-[INDEX]

- [02]-[LIVE_FAULT]: the realtime refusal family, the resume brand, the resumable-source contract; `LiveFault`.
- [03]-[SSE_ROW]: the SSE endpoint fold: resume decode, encoder framing, heartbeat, lossless bound; `Realtime`.
- [04]-[SOCKET_ROW]: the WS upgrade fold: typed duplex framing over the socket channel; `Realtime`.
- [05]-[FEED_ROWS]: source adapters: reactive query reads, fanout topics, the presence roster stream; `Realtime`.
- [06]-[ADMISSION]: channel rules, subscription grant, stamp guard, roster read, fan registry; `Admission`.
- [07]-[MOUNT_PORT]: the foreign-protocol mount port; `Mount`.

## [02]-[LIVE_FAULT]

[LIVE_FAULT]:
- Owner: `LiveFault` — the realtime reason family, its rows closed through the core `FaultClass.family` seam: `denied` (subscription refused by admission), `shed` (fan capacity refused), `lost` (resume coordinate no longer replayable — the client re-syncs from a snapshot), `closed` (channel retired or transport failed) — each row the core class alone, so the `problem` net renders an escaped instance at the governed status and no local rank, retry, or status column rides beside it; and `Realtime.Source` — the resumable-feed contract every endpoint takes: `from(resume)` opens the stream, `token(item)` mints the reattach coordinate as `Option` so a snapshot-shaped feed (each emission a fresh decoded read) is honestly tokenless and a journal-shaped feed is replay-exact.
- Law: `from(resume)` owns replay truth — `Option.none` starts live with the source's own warm-up, `Option.some(resume)` resumes after the attested coordinate, and a coordinate the source can no longer honor fails `lost` so the client re-syncs instead of silently missing a gap; the token travels as the SSE event `id`, so the browser's `Last-Event-ID` reconnect machinery carries the resume attestation with zero client code.
- Law: tokens are opaque and bounded — the `Resume` brand admits the wire form at the header seam; minting is the source's, and this plane never parses a token's interior.
- Packages: `effect` (`Schema`, `Option`, `Stream`); `@rasm/ts/core` (`FaultClass`).

```typescript signature
import { Sse } from "@effect/experimental"
import { ChannelSchema, type HttpApp, HttpServerRequest, HttpServerResponse, Ndjson, Socket } from "@effect/platform"
import { SqlClient, type SqlError } from "@effect/sql"
import {
  Channel, Chunk, Context, DateTime, Deferred, Duration, Effect, Either, HashMap, Layer, Option, type ParseResult, Ref,
  Schedule, Schema, type Scope, Stream, Trie,
} from "effect"
import { FaultClass, type Fold, Hlc, Presence } from "@rasm/ts/core"
import { Live } from "@rasm/ts/data"
import { Envelope, Fanout } from "../net/pubsub.ts"
import { Principal } from "./api.ts"

// One row per reason: the core kind alone. Retryability, blame, and the response code stay the core row
// table's and problem#STATUS_RECORD's, so no local policy column rides beside `class`.
const _live = FaultClass.family(["denied", "shed", "lost", "closed"] as const, {
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
  get class(): FaultClass.Kind {
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
}
```

## [03]-[SSE_ROW]

[SSE_ROW]:
- Owner: `Realtime.sse` — one endpoint fold for every SSE feed in the branch: decode `Last-Event-ID` through the `Resume` brand (absence is a fresh attach, never a fault), open `source.from(resume)`, encode each item through its schema into an `Sse.Event` whose `id` is the item's own token, and merge the heartbeat cadence so proxies never reap an idle feed. This emitted family is exactly the `Sse.Event` family `net/channel#FEED_SEAM` decodes, so neither endpoint invents a second frame arm.
- Law: `_SSE` is the policy row — `beat` (heartbeat cadence) and `lag` (the buffer bound between the fold and a slow consumer, `"suspend"` so pressure stops the producer before any frame is lost) — one value tuned per app, threaded nowhere. Dropping and sliding buffers are forbidden on a resumable stream because it creates an in-connection gap the browser's reconnect token cannot attest.
- Law: the encode seam is the codec's own — frames lower to response bytes through `Sse.encoder`, the heartbeat is a named `ping` event clients ignore by name, and a tokenless item writes no `id` so the browser attests only coordinates the source honors.
- Law: a source's own `LiveFault` passes the seam intact; any foreign source fault normalizes to `closed` at the one `Stream.mapError` seam — the same one-seam fold the socket row runs.
- Law: the fold TAKES `[06]`'s reservation fence and never reaches the admission plane itself — `Stream.interruptWhenDeferred` over the whole frame stream is what makes the per-principal cap a fact about live responses, so a supersede or a response-scope close ends this feed and frees the slot in one motion.
- Boundary: which feeds exist and who attaches is `[06]`'s admission; the inbound SSE parser is `net/channel#FEED_SEAM`'s — this endpoint only emits.
- Packages: `@effect/experimental` (`Sse`); `@effect/platform` (`HttpServerRequest`, `HttpServerResponse`); `effect` (`Stream`, `Schedule`, `Duration`, `Deferred`).

```typescript signature
const _SSE = {
  beat: Duration.seconds(25),
  lag: 64,
} as const

const _ResumeHeader = Schema.Struct({
  "last-event-id": Schema.optionalWith(_Resume, { as: "Option" }),
})

const _BEAT: Sse.Event = { _tag: "Event", event: "ping", id: undefined, data: "{}" }

const _encoded = <E, R>(frames: Stream.Stream<Sse.Event, E, R>): Stream.Stream<Uint8Array, E, R> =>
  Stream.encodeText(Stream.map(frames, (event) => Sse.encoder.write(event)))

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
      Stream.buffer({ capacity: _SSE.lag, strategy: "suspend" }),
    )
    // the admission fence bounds the WHOLE frame stream, heartbeat included: a superseding subscribe settles it and
    // this response ends, so the plane-level slot the successor took is never held by two live feeds at once
    const framed = Stream.merge(events, Stream.repeatEffectWithSchedule(Effect.succeed(_BEAT), Schedule.spaced(_SSE.beat)), {
      haltStrategy: "left",
    }).pipe(Stream.interruptWhenDeferred(fence))
    return HttpServerResponse.stream(Stream.provideContext(_encoded(framed), context)).pipe(
      HttpServerResponse.setHeaders({ "content-type": "text/event-stream", "cache-control": "no-cache", connection: "keep-alive" }),
    )
  })
```

## [04]-[SOCKET_ROW]

[SOCKET_ROW]:
- Owner: `Realtime.socket` — the WS upgrade fold: `HttpServerRequest.upgrade` yields the peer socket, `Socket.toChannelWith` lifts it to a byte channel, `Ndjson.duplexString` frames text lines, and `ChannelSchema.duplexUnknown({ inputSchema, outputSchema })` types both directions in one composition — a live session is one typed duplex channel whose inbound decodes INTO the caller's vocabulary (`Presence.Op`, subscribe intents) and whose outbound is the encoded frame family, backpressure inherited from the channel stack; the binary peer swaps `Ndjson` for the `net/channel#FRAME_ROWS` msgpack row with an unchanged schema seam.
- Law: frame vocabularies are parameters — this row owns transport and typing, never the frame family; `[06]` supplies the inbound admission fold and the outbound feeds, so a new realtime feature is a frame case at its owner, not a socket edit.
- Law: a decode failure on any inbound frame ends the session typed — a malformed client frame is a `LiveFault`, never a silent drop; the channel's error fold normalizes every transport, frame, and parse fault into the family at the one `Channel.mapError` seam.
- Law: this row honors `[06]`'s fence on the identical member family the SSE row does — `Channel.interruptWhenDeferred` — so one admission plane governs both transports and a duplex cannot outlive the reservation that admitted it.
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
    return Socket.toChannelWith<LiveFault>()(socket).pipe(
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
- Owner: the source adapters — the three branch feed families lifted into the one `Source` contract so every endpoint fold serves them unchanged. `Realtime.query(bound)` serves a data `Live.Bound`: `changes` is the push stream re-running on every overlapping mutation, each emission a fresh decoded read, so a reconnect re-reads current state and misses nothing by construction; the bound's own `coordinate` projection answers the token — a lane carrying a durable emission identity (its `AsOf` sequence) serves it as the event `id`, a DEDUPE coordinate the client proves its rendered state against and never a replay cursor, while a coordinate-free bound is honestly tokenless — and the pull twin stays the consumer-side `mailbox`, never an SSE concern; `Realtime.topic(topic)` serves a fanout subject: a fresh attach warms from the topic row's replay window, and a caller holding its own sequence ledger opens `Fanout.replay(topic, anchor)` instead — the anchor is the caller's evidence, so the adapter mints no token it cannot honor; `Realtime.roster(feed, lease)` serves presence: the folded table stream projects through `Presence.roster` against a horizon minted per emission, so liveness is a read-time verdict and no timer fiber sweeps anything.
- Law: a feed value arrives bound, never rebuilt — the adapter composes `bound.changes`, `fanout.subscribe`, or the app-wired fold handle; re-running a query, caching a copy, or subscribing twice restates delivery the owning page already guarantees.
- Law: fault normalization is the endpoint's one seam — `SqlError`/`ParseError` on a query feed and `FanoutFault` on a topic feed pass as the stream's own error into the endpoint fold, which normalizes foreign faults to `closed`; a `horizon` fanout fault maps to `lost` first because the client must re-sync, the one evidence-preserving arm.
- Growth: a new feed family (a flag-verdict stream, a vital fact stream) is one adapter over the same contract; the endpoints never change.
- Packages: `@rasm/ts/data` (`Live`); `@rasm/ts/core` (`Presence`, `Hlc`); `../net/pubsub.ts` (`Fanout`); `effect` (`Stream`, `Option`, `DateTime`, `Schema`).

```typescript signature
const _query = <A, R>(
  bound: Live.Bound<A, R>,
): Realtime.Source<A, SqlError.SqlError | ParseResult.ParseError, R | SqlClient.SqlClient> => ({
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
  feed: Stream.Stream<Fold.Table<Presence.Actor, Presence.State>, E, R>,
  lease: Presence.Lease,
): Realtime.Source<HashMap.HashMap<Presence.Actor, Presence.Status>, E, R> => ({
  from: () =>
    Stream.mapEffect(feed, (table) =>
      Effect.map(DateTime.now, (now) =>
        Presence.roster(table, Hlc.tick(Hlc.genesis, Hlc.physicalOf(now)), lease))),
  token: () => Option.none(),
})
```

## [06]-[ADMISSION]

[ADMISSION]:
- Owner: `Admission.make(rules)` — one constructor over the app's channel-rule rows, built ONCE per served app and held by the serving Layer, never per session: each row keys a branded channel prefix and carries an `Admission.Rule` `Schema.Class` with `scope` (the `Principal` scope a subscriber must hold, `Option` for public channels), `presence` (whether the channel serves a roster), positive `fan` (the per-principal live-subscription cap), and `lease` (the presence liveness windows) — held in a `Trie` so `Trie.longestPrefixOf` resolves any concrete channel to its most specific family row in one read, and a channel family is one row, never one row per channel.
- Law: admission is a two-gate fold — the channel must resolve to a rule (an unmatched channel is `denied`, never a default-open), and the rule's scope, when present, must pass `Principal.allows` — producing a `Grant` that carries the resolved rule, so every later decision (roster serving, fan cap, lease) reads the grant's own row and nothing downstream re-looks anything up.
- Law: the stamp guard pins identity — a decoded `Presence.Op` reaches the fold only when its `actor` equals the authenticated principal's subject AND the grant's channel serves presence; a mismatched actor is `denied` with the op discarded, so presence forgery is refused at this plane and the core fold never carries an authorization concern; forwarding is a supplied sink, so where the fold runs is composition, never law here.
- Law: the fan bound has one authoritative scope — the PRINCIPAL — and the registry is plane-level: one cell keyed `subject:channel` holds every live subscription across every session, so a second session of one principal draws from the same cap and cannot mint a fresh allowance; presence in the cell IS the held slot and the value is that holder's own FENCE, so the census read, the supersede swap, and the release are one atomic `Ref.modify` apiece and no epoch counter exists to drift. A subscription past the grant's `fan` refuses as `shed` before any stream opens.
- Law: the reservation is what the served response HOLDS, never a fiber beside it — `subscribe` is a scoped acquisition answering the fence, the endpoint folds it through `Stream.interruptWhenDeferred` / `Channel.interruptWhenDeferred`, and the finalizer releases the slot only while that fence still owns the key; so a supersede settles the predecessor's fence and its RESPONSE closes, a client disconnect closes the response scope and the slot frees, and the cap counts live responses rather than bookkeeping fibers a served stream never rides. A registry forking its own fiber holds a count nothing can interrupt and leaves the cap tracking a ghost.
- Law: the rule table is app data under a lib shape — which channels exist is composition material, so two apps with different channel maps share every line of this module.
- Growth: a new admission axis (a payload ceiling, a rate row) is one `Rule` field read at its gate; a new channel family is one app-side row.
- Packages: `effect` (`Trie`, `Option`, `Either`, `Deferred`, `HashMap`, `Ref`, `Scope`); `@rasm/ts/core` (`Presence`, `Hlc`); `./api.ts` (`Principal`).

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
  lease: Schema.Struct({ idle: Schema.DurationFromSelf, gone: Schema.DurationFromSelf }),
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
      : op.actor !== principal.subject
        ? Effect.fail(new LiveFault({ reason: "denied", detail: op.actor }))
        : forward(op)

// one atomic verdict over the whole cell: `left` is the refused cap, `right` carries the superseded incumbent's
// fence when a held key re-reserved and `none` when the slot was fresh — three outcomes one boolean cannot spell
const _reserved = (
  cell: Ref.Ref<HashMap.HashMap<string, Deferred.Deferred<void>>>,
  key: string,
  subject: string,
  fan: number,
  fence: Deferred.Deferred<void>,
): Effect.Effect<Either.Either<Option.Option<Deferred.Deferred<void>>, void>> =>
  Ref.modify(cell, (slots) =>
    Option.match(HashMap.get(slots, key), {
      // a held key re-reserves from ANY session of this principal and never charges the cap twice
      onSome: (incumbent) => [Either.right(Option.some(incumbent)), HashMap.set(slots, key, fence)] as const,
      onNone: () =>
        HashMap.size(HashMap.filter(slots, (_, held) => held.startsWith(`${subject}:`))) >= fan
          ? ([Either.left<void>(undefined), slots] as const)
          : ([Either.right(Option.none<Deferred.Deferred<void>>()), HashMap.set(slots, key, fence)] as const),
    }))

const _make = (rows: ReadonlyArray<readonly [prefix: Admission.Channel, rule: Admission.Rule]>) =>
  Effect.gen(function* () {
    const rules = Trie.fromIterable(rows)
    const cell = yield* Ref.make(HashMap.empty<string, Deferred.Deferred<void>>())
    const admit = _admit(rules)
    const subscribe = (
      grant: Admission.Grant,
      principal: Principal.Shape,
    ): Effect.Effect<Deferred.Deferred<void>, LiveFault, Scope.Scope> =>
      Effect.gen(function* () {
        const key = `${principal.subject}:${grant.channel}`
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
                      onSome: (current) => (current === fence ? HashMap.remove(slots, key) : slots),
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
- Owner: `Mount` — the foreign-protocol port: one `Context.Tag` carrying an immutable row collection where each row is `{ prefix, app }` and `app` is a complete `HttpApp.Default` implementing a foreign realtime protocol; `route#LAYER_ROUTES` folds the collection and mounts every app at its prefix — presence-as-data, an unwired port serves nothing, and the standing satisfier is the data wave's EventLog sync server provided at the app root.
- Law: the port is the ledger's answer — this plane never imports the satisfier; `Mount.of(...rows)` supplies any number of protocols through one Layer, each app assembly owns a unique-prefix row set, and a second foreign protocol is a row against the same Tag, never a second port.
- Boundary: upgrade mechanics inside the mounted app are the satisfier's; this page owns the Tag and its contract.
- Packages: `@effect/platform` (`HttpApp`); `effect` (`Context`, `Layer`); each satisfier's own packages stay at the app root.

```typescript signature
declare namespace Mount {
  type Row = { readonly prefix: `/${string}`; readonly app: HttpApp.Default }
}

class Mount extends Context.Tag("runtime/serve/Mount")<Mount, ReadonlyArray<Mount.Row>>() {
  static readonly of = (...rows: ReadonlyArray<Mount.Row>): Layer.Layer<Mount> => Layer.succeed(Mount, rows)
}

const Realtime = {
  Resume: _Resume,
  policy: _SSE,
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

- [CONNECT_MOUNT_LIFT]-[BLOCKED]: which owner publishes the node-handler lift a `Mount.Row` needs — `connectNodeAdapter(options)` answers a `NodeHandlerFn` and `HttpApp` exposes `fromWebHandler` over a FETCH handler alone, so nothing lifts a `NodeHandlerFn` into the `HttpApp.Default` the row carries, while `serve/route#LAYER_ROUTES` already drives a raw node handler through `NodeHttpServerRequest.toIncomingMessage`/`toServerResponse` without publishing that pair as a member, and `ConnectNodeAdapterOptions` declares no own `interceptors` field — the option reaches a caller only by inheritance from the package-internal `UniversalHandlerOptions`, so no stable adapter spelling carries it and `Seam.guard` covers every mounted row regardless, the router attaching it once above the catch-all; route through `libs/typescript/runtime/.planning/serve/route.md` `Router.RailMount`, `libs/typescript/.api/effect-platform-node.md`, and `libs/typescript/runtime/.api/connectrpc-connect-node.md`; arm when one published lift serves both the rail row and this port, since two spellings of one adapter fork the Mount contract.
