# [RUNTIME_CHANNEL]

Framed stream transport is the second half of the branch net plane: where `client` owns request/response egress, this page owns the long-lived byte channels — socket duplex under a closed frame vocabulary, and the server-sent event feed — both backpressured by construction, both typed at the seam. A socket is capability: construction rides `Socket.makeWebSocket` against the `Socket.WebSocketConstructor` Tag the runtime binding satisfies, so one framed transport definition serves every runtime lane, and the frame is a row swap under an unchanged schema seam. The SSE feed owns the full `Sse` codec: `Sse.makeChannel` decodes the `data:`/`event:`/`id:`/`retry:` line protocol as package capability, the reattach cursor advances as a fold and stamps `last-event-id` on reconnect, a `Sse.Retry` frame drives the next reconnect delay, silence folds through the core degradation ladder to pick probe cadence, and `Sse.encoder` is the mirror the serving edge composes so both directions of the dialect have one codec owner. A raw socket listener, a hand `data:`-line parser, and `JSON.stringify` written to a wire are the named defects. The module is `runtime/src/net/channel.ts`.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]    | [OWNS]                                                                          | [PUBLIC]  |
| :-----: | :----------- | :--------------------------------------------------------------------------------- | :-------- |
|  [01]   | `FRAME_ROWS` | the duplex frame vocabulary — `ndjson \| msgpack` rows fused with one schema seam  | `Duplex`  |
|  [02]   | `FEED_SEAM`  | the SSE session — codec, cursor fold, Retry-driven reconnect, silence ladder       | `Feed`    |

## [2]-[FRAME_ROWS]

[FRAME_ROWS]:
- Owner: `Duplex` — the framed-socket owner. `Duplex.framed(socket, frame, protocol)` lifts a `Socket.Socket` to a typed message channel: `Socket.toChannelWith<E>()` turns the socket into a byte `Channel`, and the frame row's fused combinator — `Ndjson.duplexSchema` (newline-delimited, `NdjsonError`) or `MsgPack.duplexSchema` (length-delimited binary, `MsgPackError`) — owns chunk reassembly and the schema seam in one step, so both directions are decoded values and backpressure is channel-native.
- Law: the frame is a row swap under an unchanged schema seam — the `_frames` table keys each dialect to its fused combinator, dispatch is one keyed lookup, and `Duplex.Kind` derives from the table; moving a peer from ndjson to msgpack edits one argument and no consumer, and a new frame dialect is one row, zero arms.
- Law: the protocol pair is send/take symmetric evidence — `send` types the outbound seam, `take` the inbound seam, both usually one closed `Schema.Union` of tagged messages; an untyped frame crossing the channel is unspellable because the fused combinator is the only construction.
- Law: fault families arrive typed and stay separate — the frame's own error, `Socket.SocketError`, and `ParseError` each route on their own tag; none is re-wrapped.
- Boundary: socket construction is capability — `Socket.makeWebSocket(url)` demands the `WebSocketConstructor` Tag, satisfied by the runtime binding at the root; session lifetime, reconnect, and the pipeline geometry above the channel are the consumer's, composed from `Stream` law.
- Entry: `Duplex.framed(socket, frame, { send, take })`.
- Packages: `@effect/platform` (`Socket`, `Ndjson`, `MsgPack`), `effect` (`Channel`, `Chunk`, `Schema`).

```typescript
import { Sse } from "@effect/experimental"
import { type HttpClient, HttpClientRequest, MsgPack, Ndjson, Socket } from "@effect/platform"
import { type Channel, type Chunk, Context, Data, Duration, Effect, Layer, Option, type ParseResult, Ref, Schedule, type Schema, Stream, pipe } from "effect"
import { Budget, Degrade, type FaultClass } from "@rasm/ts/core"
import { Client } from "./client.ts"

const _frames = { msgpack: MsgPack.duplexSchema, ndjson: Ndjson.duplexSchema } as const

declare namespace Duplex {
  type Kind = keyof typeof _frames
  type Fault = MsgPack.MsgPackError | Ndjson.NdjsonError
  type Protocol<Send, SendI, Take, TakeI> = {
    readonly send: Schema.Schema<Send, SendI>
    readonly take: Schema.Schema<Take, TakeI>
  }
}

const _framed = <Send, SendI, Take, TakeI>(
  socket: Socket.Socket,
  frame: Duplex.Kind,
  protocol: Duplex.Protocol<Send, SendI, Take, TakeI>,
): Channel.Channel<
  Chunk.Chunk<Take>,
  Chunk.Chunk<Send>,
  Duplex.Fault | ParseResult.ParseError | Socket.SocketError,
  ParseResult.ParseError,
  void,
  unknown
> =>
  Socket.toChannelWith<Duplex.Fault | ParseResult.ParseError>()(socket).pipe(
    _frames[frame]({ inputSchema: protocol.send, outputSchema: protocol.take }),
  )

const Duplex = { framed: _framed } as const
```

## [3]-[FEED_SEAM]

[FEED_SEAM]:
- Owner: `Feed` — the server-sent-event ingress port every SSE consumer in the branch shares (`flag#GATE_SERVICE` is the standing consumer; the serving edge composes the same codec's `Sse.encoder` for the mirror side). `Feed.open(origin)` yields the live event stream: the session dials the `feed` client lane (no total budget), reads the response body as bytes, and decodes frames by piping the body through `Sse.makeChannel({ bufferSize })` — `Sse.Event | Sse.Retry` is the closed frame family, so the line protocol is package capability, never a hand parser.
- Law: the reattach cursor is a fold, not a cell convention — every `Sse.Event` carrying an `id` advances the cursor, and a reconnect stamps it as the `last-event-id` request header, so an outage backfills by event id; the cursor lives in the session's own `Ref`, invisible to consumers.
- Law: the server drives its own reconnect cadence — a `Sse.Retry` frame settles into the session's cadence cell and the next re-dial sleeps the greater of the ledger delay and the server's hint, so reconnection is one fold over two evidence sources: the `feed` budget row's compiled pulse (`core/value/fault#RETRY_BUDGET`) owns the backoff envelope, the Retry hint floors it, and exhaustion mints `FeedFault` with the origin as evidence — the one fault consumers see, classed `unavailable` so the ledger gate re-drives a composing consumer.
- Law: silence is laddered, never polled — `Feed.cadence` folds an observed quiet span through `Degrade.level` to the rung's probe cadence a long-lived consumer schedules; the ladder folds the span it is handed, the consumer owns the measurement.
- RESEARCH: the `Sse.Retry` millisecond payload member the `_hinted` projection reads is unverified until the branch catalogue rows it; the frame class and the hint fold are settled, the field spelling is the research item.
- Law: the feed is transport only — it emits decoded `Sse.Event` frames and owns no payload vocabulary; the consumer's own Schema decodes `event.data` at its seam, so one feed serves every event dialect.
- Boundary: the serving mirror — `Sse.encoder` framing an outbound event stream over an HTTP response — is the edge wave's mount; this page owns the codec direction law so the dialect has one owner.
- Entry: `yield* Feed` then `feed.open(origin)`; `Feed.live` is the shipped Layer over the client lane.
- Packages: `@effect/experimental` (`Sse`), `@effect/platform` (`HttpClientRequest`), `effect` (`Context`, `Data`, `Duration`, `Option`, `Ref`, `Stream`), `./client.ts` (`Client`), `@rasm/ts/core` (`Budget`, `Degrade`).

```typescript
class FeedFault extends Data.TaggedError("FeedFault")<{
  readonly origin: string
}> {
  get class(): FaultClass.Kind {
    return "unavailable"
  }
}

class Feed extends Context.Tag("runtime/Feed")<Feed, {
  readonly open: (origin: URL) => Stream.Stream<Sse.Event, FeedFault>
}>() {
  static readonly live: Layer.Layer<Feed, never, HttpClient.HttpClient> = Layer.effect(
    Feed,
    Effect.map(Effect.context<HttpClient.HttpClient>(), (context) => ({
      open: (origin) => Stream.provideContext(_session(origin), context),
    })),
  )
  static readonly cadence = (silence: Duration.DurationInput): Duration.Duration =>
    Degrade[Degrade.level(silence)].cadence
}

declare namespace Feed {
  type Cursor = Option.Option<string>
}

const _reattached = (origin: URL, cursor: Feed.Cursor): HttpClientRequest.HttpClientRequest =>
  pipe(
    HttpClientRequest.get(origin.href).pipe(HttpClientRequest.setHeader("accept", "text/event-stream")),
    (base) =>
      Option.match(cursor, {
        onNone: () => base,
        onSome: (id) => base.pipe(HttpClientRequest.setHeader("last-event-id", id)),
      }),
  )

declare const _hinted: (frame: Sse.Retry) => Duration.Duration

const _pulled = (
  origin: URL,
  request: HttpClientRequest.HttpClientRequest,
): Stream.Stream<Sse.Event | Sse.Retry, FeedFault, HttpClient.HttpClient> =>
  Stream.unwrapScoped(
    Effect.map(Client.dial("feed", request), (response) =>
      Stream.pipeThroughChannel(response.stream, Sse.makeChannel({ bufferSize: 64 })),
    ),
  ).pipe(Stream.mapError(() => new FeedFault({ origin: origin.href })))

const _PULSE = Schedule.exponential(Budget.feed.base, Budget.feed.factor).pipe(
  Schedule.jittered,
  Schedule.resetAfter(Budget.feed.reset),
  Schedule.intersect(Schedule.recurs(Budget.feed.attempts)),
)

const _session = (origin: URL): Stream.Stream<Sse.Event, FeedFault, HttpClient.HttpClient> =>
  Stream.unwrap(
    Effect.gen(function* () {
      const cursor = yield* Ref.make<Feed.Cursor>(Option.none())
      const hint = yield* Ref.make(Option.none<Duration.Duration>())
      const attempt = Stream.unwrap(
        Effect.gen(function* () {
          yield* Effect.sleep(Option.getOrElse(yield* Ref.getAndSet(hint, Option.none()), () => Duration.zero))
          const held = yield* Ref.get(cursor)
          return _pulled(origin, _reattached(origin, held))
        }),
      ).pipe(
        Stream.tap((frame) =>
          frame._tag === "Retry"
            ? Ref.set(hint, Option.some(_hinted(frame)))
            : Ref.update(cursor, (held) => Option.orElse(Option.fromNullable(frame.id), () => held))),
        Stream.filter((frame): frame is Sse.Event => frame._tag === "Event"),
      )
      return attempt.pipe(Stream.retry(_PULSE))
    }),
  )

// --- [EXPORTS] --------------------------------------------------------------------------

export { Duplex, Feed, FeedFault }
```
