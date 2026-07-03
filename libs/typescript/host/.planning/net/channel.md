# [HOST_CHANNEL]

Framed stream transport is the second half of the branch net-policy plane: where `client.md` owns request/response egress, this page owns the long-lived byte channels — socket duplex under a closed frame vocabulary, and the server-sent event feed — both backpressured by construction, both typed at the seam, both selected beside a client lane by the same consumers (`ai` provider streaming, `work` runner discovery streams, OTLP stream egress, the `flag` live feed). A socket is capability: construction rides `Socket.makeWebSocket` against the `Socket.WebSocketConstructor` Tag the runtime binding satisfies, so one framed transport definition serves every runtime lane. A raw socket listener, a hand length-prefix or `data:`-line parser, and `JSON.stringify` written to a wire are the named defects.

## [1]-[INDEX]

- [01]-[FRAME_ROWS]: the duplex frame vocabulary — `ndjson | msgpack` rows fused with one schema seam over a socket byte channel.
- [02]-[FEED_SEAM]: the SSE ingress owner — the `Sse` codec family, the reattach cursor fold, the reconnect law.

## [2]-[FRAME_ROWS]

- Owner: `Duplex` — the framed-socket owner. `Duplex.framed(socket, frame, protocol)` lifts a `Socket.Socket` to a typed message channel: `Socket.toChannelWith<E>()` turns the socket into a byte `Channel`, and the frame row's fused combinator — `Ndjson.duplexSchema` (newline-delimited, `NdjsonError`) or `MsgPack.duplexSchema` (length-delimited binary, `MsgPackError`) — owns chunk reassembly and the schema seam in one step, so both directions are decoded values and backpressure is channel-native.
- Law: the frame is a row swap under an unchanged schema seam — the `_frames` table keys each dialect to its fused combinator (`ChannelSchema.duplexUnknown({ inputSchema, outputSchema })` composed over the frame's own duplex), dispatch is one keyed lookup, and `Duplex.Kind` derives from the table; moving a peer from ndjson to msgpack edits one argument and no consumer, and a new frame dialect is one row, zero arms.
- Law: the protocol pair is send/take symmetric evidence — `send` types the outbound seam, `take` the inbound seam, both usually one closed `Schema.Union` of tagged messages; an untyped frame crossing the channel is unspellable because the fused combinator is the only construction.
- Law: fault families arrive typed and stay separate — the frame's own error, `Socket.SocketError`, and `ParseError` each route on their own tag; none is re-wrapped.
- Boundary: socket construction is capability — `Socket.makeWebSocket(url)` demands the `WebSocketConstructor` Tag, satisfied by the runtime binding at the root (`BunSocket.layerWebSocketConstructor`, `BrowserSocket.layerWebSocketConstructor`; the node binding satisfies the same Tag); session lifetime, reconnect, and the pipeline geometry above the channel are the consumer's, composed from `Stream` law.
- Entry: `Duplex.framed(socket, frame, { send, take })`.
- Packages: `@effect/platform` (`Socket`, `Ndjson`, `MsgPack`), `effect` (`Channel`, `Chunk`, `Schema`).

```typescript
import type { Sse } from "@effect/experimental"
import { HttpClientRequest, MsgPack, Ndjson, Socket } from "@effect/platform"
import { type Channel, type Chunk, Context, Data, Option, type ParseResult, type Schema, type Stream, pipe } from "effect"

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

- Owner: `Feed` — the server-sent-event ingress port every SSE consumer in the branch shares (`flag/verdict` re-evaluation is the standing consumer; `edge/live` serves the mirror side with the same codec's `Sse.encoder`). `Feed.open(origin)` yields the live event stream: the session dials the `feed` client lane (no total budget, minimal pre-stream retry), reads the response body as bytes, and decodes frames through the admitted `Sse` codec — `Sse.makeChannel({ bufferSize })` over the body, `Sse.Event | Sse.Retry` as the closed frame family — so the `data:`/`event:`/`id:`/`retry:` line protocol is package capability, never a hand parser.
- Law: the reattach cursor is a fold, not a cell convention — every `Sse.Event` carrying an `id` advances the cursor, and a reconnect stamps it as the `last-event-id` request header, so an outage backfills by event id; the cursor lives in the session's own `Ref`, invisible to consumers.
- Law: reconnection is one policy value — the session re-registers through `Stream.retry` over the feed lane's pulse, the schedule resetting once events flow again; a `Sse.Retry` frame is observed as server cadence evidence; exhaustion mints `Feed.Fault` with the origin as evidence, the one fault consumers see.
- Law: the feed is transport only — it emits decoded `Sse.Event` frames and owns no payload vocabulary; the consumer's own Schema decodes `event.data` at its seam (`flag/verdict`'s delta family), so one feed serves every event dialect.
- Boundary: the `Feed.live` build composes `Client.dial("feed", …)` and the `Sse` channel; the byte-stream-to-channel application member is a branch `.api` research row — the contract below is settled law, the pump member spelling lands with the catalogue.
- Entry: `yield* Feed` then `feed.open(origin)`.
- Packages: `@effect/experimental` (`Sse`), `@effect/platform` (`HttpClientRequest`), `effect` (`Context`, `Data`, `Option`, `Ref`, `Stream`), `./client.ts` (`Client`).

```typescript
class FeedFault extends Data.TaggedError("FeedFault")<{
  readonly origin: string
}> {}

class Feed extends Context.Tag("host/Feed")<Feed, {
  readonly open: (origin: URL) => Stream.Stream<Sse.Event, FeedFault>
}>() {}

declare namespace Feed {
  type Fault = FeedFault
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

const _advanced = (cursor: Feed.Cursor, event: Sse.Event): Feed.Cursor =>
  Option.orElse(Option.fromNullable(event.id), () => cursor)

// --- [EXPORTS] --------------------------------------------------------------------------

export { Duplex, Feed }
```
