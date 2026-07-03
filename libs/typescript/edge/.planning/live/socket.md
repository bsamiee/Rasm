# [EDGE_SOCKET]

`live/socket.ts` is the realtime entry family: SSE and WebSocket endpoints serving `state` Subscribables and change feeds, the resume-token law that makes every SSE feed reconnect-exact, and the protocol-handler mount port — an `HttpApp` port Tag the app root satisfies (the `store` EventLog sync server is the standing satisfier) so foreign realtime protocols mount beside the owned endpoints without an edge import. The source contract is the page's spine: a resumable feed is a value pairing `from(resume)` with `token(item)`, so the resume coordinate lives in the source (the stream doctrine's law), the endpoint replays from exactly where the client attests, and a downstream dedup set never exists. Frames are Schema-typed in both directions — inbound client frames decode INTO `state` vocabulary at this seam, outbound frames encode from the fold's own change stream — and every fault is a `LiveFault` whose class rows the `problem` altitude folds.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]     | [OWNS]                                                                  | [PUBLIC]              |
| :-----: | :------------ | :-------------------------------------------------------------------------- | :-------------------- |
|  [01]   | [LIVE_FAULT]  | the realtime refusal family and the resumable-source contract               | `LiveFault`           |
|  [02]   | [SSE_ROW]     | the SSE endpoint fold: resume decode, event framing, heartbeat, retry hint  | `Realtime`            |
|  [03]   | [SOCKET_ROW]  | the WS upgrade fold: typed duplex framing over the socket channel           | `Realtime`            |
|  [04]   | [MOUNT_PORT]  | the protocol-handler mount port                                             | `Mount`               |

## [2]-[LIVE_FAULT]

[LIVE_FAULT]:
- Owner: `LiveFault` — the realtime reason family: `denied` (subscription refused by admission), `shed` (fan capacity refused), `lost` (resume coordinate no longer replayable — the client must re-sync from a snapshot), `closed` (channel retired) — rows carrying the kernel class so `problem/detail.ts` folds an escaped instance, and `Realtime.Source` — the resumable-feed contract every SSE endpoint takes.
- Law: `Source.from(resume)` owns replay truth — `Option.none` starts live-with-warmup (the wave's replay window), `Option.some(resume)` resumes after the attested coordinate, and a coordinate the source can no longer honor fails `lost` so the client re-syncs instead of silently missing a gap; the token travels as the SSE event `id`, so the browser's own `Last-Event-ID` reconnect machinery carries the resume attestation with zero client code.
- Law: tokens are opaque and bounded — the `Resume` brand admits the wire form at the header seam; minting is the source's (`token(item)`), and the edge never parses a token's interior.
- Packages: `effect` (`Schema`, `Option`, `Stream`); `kernel/fault/classify` (`FaultClass`).

```typescript
import { FaultClass } from "@rasm/ts/kernel"
import { Sse } from "@effect/experimental"
import { ChannelSchema, HttpApp, HttpServerRequest, HttpServerResponse, Ndjson, Socket } from "@effect/platform"
import { Channel, Chunk, Context, Duration, Effect, Option, Schedule, Schema, Stream } from "effect"

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
```

## [3]-[SSE_ROW]

[SSE_ROW]:
- Owner: `Realtime.sse` — one endpoint fold: decode `Last-Event-ID` through the `Resume` brand (absence is a fresh attach, never a fault), open `source.from(resume)`, encode each item through its schema into an SSE event whose `id` is the item's own token, and merge the heartbeat cadence so proxies never reap an idle feed — one declaration, every SSE feed in the branch; the client reconnect backoff is server policy carried by the codec's `Sse.Retry` directive at the head of the encode seam.
- Law: `_SSE` is the policy row — `retry` (client reconnect hint), `beat` (heartbeat cadence), `lag` (the buffer bound between the fold and a slow consumer, `"sliding"` so a stalled client sheds oldest frames instead of backpressuring the fold) — one value tuned per app, threaded nowhere.
- Law: the encode seam is the experimental `Sse` codec — events flow as `Sse.Event` values with the token as `id`, the heartbeat as comment traffic, and the retry directive as `Sse.Retry`; the channel lowers to response bytes through the codec's own encoder, and a hand-assembled `data:` string is the named defect.
- Law: change feeds arrive as `state` values — a table view serves `Live.feed(handle)` (the per-row change wave), a scalar view serves `subscribable.changes` — and this row encodes them; it never re-runs a fold, caches a copy, or subscribes twice (the state lens law, inherited).
- Boundary: which feeds exist and who may attach is `live/presence.ts`'s admission; the SSE parser (inbound) belongs to `host/net` client rows — this endpoint only emits.
- Packages: `@effect/experimental` (`Sse`); `@effect/platform` (`HttpServerRequest`, `HttpServerResponse`); `effect` (`Stream`, `Schedule`, `Duration`).

```typescript
const _SSE = {
  retry: Duration.seconds(3),
  beat: Duration.seconds(25),
  lag: 64,
} as const

declare namespace Realtime {
  type Resume = typeof _Resume.Type
  type Source<A, E, R> = {
    readonly from: (resume: Option.Option<Resume>) => Stream.Stream<A, E, R>
    readonly token: (item: A) => Resume
  }
}

const _ResumeHeader = Schema.Struct({
  "last-event-id": Schema.optionalWith(_Resume, { as: "Option" }),
})

const _BEAT: Sse.Event = { _tag: "Event", event: "ping", id: undefined, data: "{}" }

const _encoded = <E, R>(frames: Stream.Stream<Sse.AnyEvent, E, R>): Stream.Stream<Uint8Array, E, R> =>
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
          Effect.map((body): Sse.AnyEvent => ({ _tag: "Event", event: name, id: source.token(held), data: JSON.stringify(body) })),
          Effect.orDie,
        )),
      Stream.mapError((cause) => new LiveFault({ reason: "lost", detail: String(cause) })),
      Stream.buffer({ capacity: _SSE.lag, strategy: "sliding" }),
    )
    const framed = Stream.concat(
      Stream.make(new Sse.Retry({ duration: _SSE.retry, lastEventId: undefined })),
      Stream.merge(events, Stream.repeatEffectWithSchedule(Effect.succeed(_BEAT), Schedule.spaced(_SSE.beat)), { haltStrategy: "left" }),
    )
    return HttpServerResponse.stream(Stream.provideContext(_encoded(framed), context)).pipe(
      HttpServerResponse.setHeaders({ "content-type": "text/event-stream", "cache-control": "no-cache", connection: "keep-alive" }),
    )
  })
```

## [4]-[SOCKET_AND_MOUNT]

[SOCKET_ROW]:
- Owner: `Realtime.socket` — the WS upgrade fold: `HttpServerRequest.upgrade` yields the peer socket, `Socket.toChannelWith` lifts it to a byte channel, `Ndjson.duplexString` frames text lines, and `ChannelSchema.duplexUnknown({ inputSchema, outputSchema })` types both directions in one composition — so a live session is one typed duplex channel value whose inbound decodes INTO the caller's vocabulary (`Presence.Op`, subscribe intents) and whose outbound is the encoded frame family, with backpressure inherited from the channel stack.
- Law: frame vocabularies are parameters — this row owns transport and typing, never the frame family; `live/presence.ts` supplies the inbound admission fold and the outbound feeds, so a new realtime feature is a frame case at its owner, not a socket edit.
- Law: the drive seam runs the duplex channel against the handler's stream transform, and a decode failure on any inbound frame fails the session typed — a malformed client frame ends the session as a `LiveFault`, never a silent drop; the channel's error fold normalizes every transport, frame, and parse fault into the family at the one `Channel.mapError` seam.

[MOUNT_PORT]:
- Owner: `Mount` — the protocol-handler port: a `Context.Tag` carrying `{ prefix, app }` where `app` is a complete `HttpApp.Default` implementing a foreign realtime protocol; the app's router assembly folds `Effect.serviceOption(Mount)` and mounts the app at its prefix (`api/serve.ts` states the fold's placement), so mounting is presence-as-data — the `store` EventLog sync server is the standing satisfier at the app root, and an unwired port serves nothing.
- Law: the port is the ledger's answer — `edge` may not import `store`, so the sync server arrives as a value satisfying an edge-declared shape; a second foreign protocol is a second Layer against the same Tag composed at a different prefix, never a second port.
- Boundary: upgrade mechanics inside the mounted app are the satisfier's; this page owns the Tag and the mount fold's contract.
- Packages: `@effect/platform` (`Socket`, `Ndjson`, `ChannelSchema`, `HttpApp`, `HttpServerRequest`); `effect` (`Context`, `Channel`).

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
      Channel.mapError((cause) => cause instanceof LiveFault ? cause : new LiveFault({ reason: "closed", detail: String(cause) })),
    )
  })

class Mount extends Context.Tag("edge/live/Mount")<Mount, {
  readonly prefix: `/${string}`
  readonly app: HttpApp.Default
}>() {}

const Realtime: {
  readonly Resume: typeof _Resume
  readonly policy: typeof _SSE
  readonly sse: typeof _sse
  readonly socket: typeof _socket
} = {
  Resume: _Resume,
  policy: _SSE,
  sse: _sse,
  socket: _socket,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { LiveFault, Mount, Realtime }
```
