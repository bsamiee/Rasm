# [RUNTIME_LIVE]

This realtime serve plane: SSE and WebSocket endpoints over the branch's own feed values — the data wave's reactive reads, the fanout topics, the core presence fold — under the resume-token law that makes a replayable feed reconnect-exact and under an admission gate that is data in one channel-rule table. This SSE row is the serving mirror of `channel#FEED_SEAM`: the same `Sse` codec owns both directions of the dialect — `Sse.makeChannel` decodes on the consuming side, `Sse.encoder` frames on this side — so a hand-assembled `data:` string is unspellable anywhere in the branch. This socket row lifts `HttpServerRequest.upgrade` into one typed duplex channel: `Ndjson.duplexString` frames text lines, `ChannelSchema.duplexUnknown` types both directions, and the frame vocabularies are parameters so a new realtime feature is a frame case at its owner, never a socket edit. Admission guards exactly what the endpoints serve: prefix-matched channel rules resolve scope, presence service, fan cap, and lease policy in one Trie read; the stamp guard pins a decoded `Presence.Op` to the authenticated principal before it reaches the fold; the roster read is a pure verdict against a caller-minted horizon; and the admission plane is constructed ONCE per served app, so the per-principal fan cap holds across every session the principal opens. Foreign realtime protocols arrive through the `Mount` port the route assembly folds. This module ships on the `./server` exports subpath as `runtime/src/serve/live.ts`.

## [01]-[CLUSTERS]

| [INDEX] | [CLUSTER]    | [OWNS]                                                                           | [PUBLIC]    |
| :-----: | :----------- | :------------------------------------------------------------------------------- | :---------- |
|  [01]   | `LIVE_FAULT` | the realtime refusal family, the resume brand, the resumable-source contract     | `LiveFault` |
|  [02]   | `SSE_ROW`    | the SSE endpoint fold: resume decode, encoder framing, heartbeat, lossless bound | `Realtime`  |
|  [03]   | `SOCKET_ROW` | the WS upgrade fold: typed duplex framing over the socket channel                | `Realtime`  |
|  [04]   | `FEED_ROWS`  | source adapters: reactive query reads, fanout topics, the presence roster stream | `Realtime`  |
|  [05]   | `ADMISSION`  | channel rules, subscription grant, stamp guard, roster read, fan registry        | `Admission` |
|  [06]   | `MOUNT_PORT` | the foreign-protocol mount port                                                  | `Mount`     |

## [02]-[LIVE_FAULT]

[LIVE_FAULT]:
- Owner: `LiveFault` — the realtime reason family: `denied` (subscription refused by admission), `shed` (fan capacity refused), `lost` (resume coordinate no longer replayable — the client re-syncs from a snapshot), `closed` (channel retired or transport failed) — rows carrying the core class so the `problem` net renders an escaped instance at its own status; and `Realtime.Source` — the resumable-feed contract every endpoint takes: `from(resume)` opens the stream, `token(item)` mints the reattach coordinate as `Option` so a snapshot-shaped feed (each emission a fresh decoded read) is honestly tokenless and a journal-shaped feed is replay-exact.
- Law: `from(resume)` owns replay truth — `Option.none` starts live with the source's own warm-up, `Option.some(resume)` resumes after the attested coordinate, and a coordinate the source can no longer honor fails `lost` so the client re-syncs instead of silently missing a gap; the token travels as the SSE event `id`, so the browser's `Last-Event-ID` reconnect machinery carries the resume attestation with zero client code.
- Law: tokens are opaque and bounded — the `Resume` brand admits the wire form at the header seam; minting is the source's, and this plane never parses a token's interior.
- Packages: `effect` (`Schema`, `Option`, `Stream`); `@rasm/ts/core` (`FaultClass`).

```typescript
import { Sse } from "@effect/experimental"
import { ChannelSchema, type HttpApp, HttpServerRequest, HttpServerResponse, Ndjson, Socket } from "@effect/platform"
import { SqlClient, type SqlError } from "@effect/sql"
import {
  Channel, Chunk, Context, DateTime, Duration, Effect, FiberMap, HashMap, Layer, Option, type ParseResult, Ref, Schedule,
  Schema, type Scope, Stream, Trie,
} from "effect"
import { type FaultClass, type Fold, Hlc, Presence } from "@rasm/ts/core"
import { Live } from "@rasm/ts/data"
import { Envelope, Fanout } from "../net/pubsub.ts"
import { Principal } from "./api.ts"

const _reasons = ["denied", "shed", "lost", "closed"] as const

const _faults = {
  denied: { class: "denied" },
  shed: { class: "unavailable" },
  lost: { class: "conflicted" },
  closed: { class: "unavailable" },
} as const

declare namespace LiveFault {
  type Reason = keyof typeof _faults
  type _Rows<T extends Record<Reason, { readonly class: FaultClass.Kind }> = typeof _faults> = T
}

class LiveFault extends Schema.TaggedError<LiveFault>()("LiveFault", {
  reason: Schema.Literal(..._reasons),
  detail: Schema.String,
}) {
  get class(): FaultClass.Kind {
    return _faults[this.reason].class
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
- Owner: `Realtime.sse` — one endpoint fold for every SSE feed in the branch: decode `Last-Event-ID` through the `Resume` brand (absence is a fresh attach, never a fault), open `source.from(resume)`, encode each item through its schema into an `Sse.Event` whose `id` is the item's own token, and merge the heartbeat cadence so proxies never reap an idle feed. This emitted family is exactly the `Sse.Event` family `channel#FEED_SEAM` decodes, so neither endpoint invents a second frame arm.
- Law: `_SSE` is the policy row — `beat` (heartbeat cadence) and `lag` (the buffer bound between the fold and a slow consumer, `"suspend"` so pressure stops the producer before any frame is lost) — one value tuned per app, threaded nowhere. A dropping or sliding buffer is forbidden on a resumable stream because it creates an in-connection gap the browser's reconnect token cannot attest.
- Law: the encode seam is the codec's own — frames lower to response bytes through `Sse.encoder`, the heartbeat is a named `ping` event clients ignore by name, and a tokenless item writes no `id` so the browser attests only coordinates the source honors.
- Law: a source's own `LiveFault` passes the seam intact; any foreign source fault normalizes to `closed` at the one `Stream.mapError` seam — the same one-seam fold the socket row runs.
- Boundary: which feeds exist and who attaches is `[06]`'s admission; the inbound SSE parser is `channel#FEED_SEAM`'s — this endpoint only emits.
- Packages: `@effect/experimental` (`Sse`); `@effect/platform` (`HttpServerRequest`, `HttpServerResponse`); `effect` (`Stream`, `Schedule`, `Duration`).

```typescript
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
    const framed = Stream.merge(events, Stream.repeatEffectWithSchedule(Effect.succeed(_BEAT), Schedule.spaced(_SSE.beat)), {
      haltStrategy: "left",
    })
    return HttpServerResponse.stream(Stream.provideContext(_encoded(framed), context)).pipe(
      HttpServerResponse.setHeaders({ "content-type": "text/event-stream", "cache-control": "no-cache", connection: "keep-alive" }),
    )
  })
```

## [04]-[SOCKET_ROW]

[SOCKET_ROW]:
- Owner: `Realtime.socket` — the WS upgrade fold: `HttpServerRequest.upgrade` yields the peer socket, `Socket.toChannelWith` lifts it to a byte channel, `Ndjson.duplexString` frames text lines, and `ChannelSchema.duplexUnknown({ inputSchema, outputSchema })` types both directions in one composition — a live session is one typed duplex channel whose inbound decodes INTO the caller's vocabulary (`Presence.Op`, subscribe intents) and whose outbound is the encoded frame family, backpressure inherited from the channel stack; the binary peer swaps `Ndjson` for the `channel#FRAME_ROWS` msgpack row with an unchanged schema seam.
- Law: frame vocabularies are parameters — this row owns transport and typing, never the frame family; `[06]` supplies the inbound admission fold and the outbound feeds, so a new realtime feature is a frame case at its owner, not a socket edit.
- Law: a decode failure on any inbound frame ends the session typed — a malformed client frame is a `LiveFault`, never a silent drop; the channel's error fold normalizes every transport, frame, and parse fault into the family at the one `Channel.mapError` seam.
- Packages: `@effect/platform` (`Socket`, `Ndjson`, `ChannelSchema`, `HttpServerRequest`); `effect` (`Channel`, `Chunk`).

```typescript
const _socket = <In, IEnc, Out, OEnc, RIn, ROut>(
  frames: {
    readonly inbound: Schema.Schema<In, IEnc, RIn>
    readonly outbound: Schema.Schema<Out, OEnc, ROut>
  },
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
    )
  })
```

## [05]-[FEED_ROWS]

[FEED_ROWS]:
- Owner: the source adapters — the three branch feed families lifted into the one `Source` contract so every endpoint fold serves them unchanged. `Realtime.query(bound)` serves a data `Live.Bound`: `changes` is the push stream re-running on every overlapping mutation, each emission a fresh decoded read, so the source is honestly tokenless — a reconnect re-reads current state and misses nothing by construction — and the pull twin stays the consumer-side `mailbox`, never an SSE concern; `Realtime.topic(topic)` serves a fanout subject: a fresh attach warms from the topic row's replay window, and a caller holding its own sequence ledger opens `Fanout.replay(topic, anchor)` instead — the anchor is the caller's evidence, so the adapter mints no token it cannot honor; `Realtime.roster(feed, lease)` serves presence: the folded table stream projects through `Presence.roster` against a horizon minted per emission, so liveness is a read-time verdict and no timer fiber sweeps anything.
- Law: a feed value arrives bound, never rebuilt — the adapter composes `bound.changes`, `fanout.subscribe`, or the app-wired fold handle; re-running a query, caching a copy, or subscribing twice restates delivery the owning page already guarantees.
- Law: fault normalization is the endpoint's one seam — `SqlError`/`ParseError` on a query feed and `FanoutFault` on a topic feed pass as the stream's own error into the endpoint fold, which normalizes foreign faults to `closed`; a `horizon` fanout fault maps to `lost` first because the client must re-sync, the one evidence-preserving arm.
- Growth: a new feed family (a flag-verdict stream, a vital fact stream) is one adapter over the same contract; the endpoints never change.
- Packages: `@rasm/ts/data` (`Live`); `@rasm/ts/core` (`Presence`, `Hlc`); `../net/pubsub.ts` (`Fanout`); `effect` (`Stream`, `Option`, `DateTime`).

```typescript
const _query = <A, R>(
  bound: Live.Bound<A, R>,
): Realtime.Source<A, SqlError.SqlError | ParseResult.ParseError, R | SqlClient.SqlClient> => ({
  from: () => bound.changes,
  token: () => Option.none(),
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
- Law: the fan bound has one authoritative scope — the PRINCIPAL — and the registry is plane-level: one `FiberMap` keyed `subject:channel` holds every live subscription across every session, so a second session of one principal draws from the same cap and cannot mint a fresh allowance; the census is an epoch-counted key ledger — reserve increments the key, the subscription's `ensuring` releases it, and a supersede on a held key (a re-subscribe from any session) interrupts the predecessor whose own release balances the ledger — so distinct held channels per subject is the cap read, no counter drifts, and scope close releases every slot; a subscription past the grant's `fan` refuses as `shed` before any stream opens.
- Law: the rule table is app data under a lib shape — which channels exist is composition material, so two apps with different channel maps share every line of this module.
- Growth: a new admission axis (a payload ceiling, a rate row) is one `Rule` field read at its gate; a new channel family is one app-side row.
- Packages: `effect` (`Trie`, `Option`, `FiberMap`, `HashMap`, `Ref`, `Scope`); `@rasm/ts/core` (`Presence`, `Hlc`); `./api.ts` (`Principal`).

```typescript
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

const _make = (rows: ReadonlyArray<readonly [prefix: Admission.Channel, rule: Admission.Rule]>) =>
  Effect.gen(function* () {
    const rules = Trie.fromIterable(rows)
    const held = yield* FiberMap.make<string>()
    const census = yield* Ref.make(HashMap.empty<string, number>())
    const admit = _admit(rules)
    const release = (key: string): Effect.Effect<void> =>
      Ref.update(census, (counts) =>
        HashMap.modifyAt(counts, key, (slot) =>
          Option.flatMap(slot, (epoch) => (epoch <= 1 ? Option.none() : Option.some(epoch - 1)))))
    const subscribe = <E, R>(
      grant: Admission.Grant,
      principal: Principal.Shape,
      run: Effect.Effect<void, E, R>,
    ): Effect.Effect<void, E | LiveFault, R | Scope.Scope> =>
      Effect.gen(function* () {
        const key = `${principal.subject}:${grant.channel}`
        // reserve is atomic over the principal's distinct held channels; a held key re-reserves (supersede), and the
        // interrupted predecessor's own ensuring balances the epoch — so the cap spans every session of one principal
        const admitted = yield* Ref.modify(census, (counts) => {
          const mine = HashMap.size(HashMap.filter(counts, (_, k) => k.startsWith(`${principal.subject}:`)))
          const supersede = Option.isSome(HashMap.get(counts, key))
          return !supersede && mine >= grant.rule.fan
            ? ([false, counts] as const)
            : ([
                true,
                HashMap.modifyAt(counts, key, Option.match({ onNone: () => Option.some(1), onSome: (epoch) => Option.some(epoch + 1) })),
              ] as const)
        })
        yield* admitted
          ? FiberMap.run(held, key, Effect.ensuring(run, release(key)))
          : Effect.fail(new LiveFault({ reason: "shed", detail: key }))
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

```typescript
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

- [CONNECT_MOUNT]-[BLOCKED]: which exact adapter fold lifts `connectNodeAdapter(options): NodeHandlerFn` into `HttpApp.Default` without bypassing `Seam.guard`, and which server interceptor option continues W3C context before the handler; route first through `libs/typescript/runtime/.api/connectrpc-connect-node.md` `[03]-[ENTRYPOINTS]`, then `libs/typescript/.api/connectrpc-connect-node.md`, with the host interop rows at `libs/typescript/.api/effect-platform-node.md`; arm when the handler lift and interceptor pair are both exact and one mounted fence composes them.
