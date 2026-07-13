# [RUNTIME_CHANNEL]

Framed stream transport is the second half of the branch net plane: where `client` owns request/response egress, this page owns the long-lived byte channels — socket duplex under a closed frame vocabulary, and the server-sent event feed — both backpressured by construction, both typed at the seam. A socket is capability: construction rides `Socket.makeWebSocket` against the `Socket.WebSocketConstructor` Tag the runtime binding satisfies, so one framed transport definition serves every runtime lane, and the frame is a row swap under an unchanged schema seam. The SSE feed owns the full `Sse` codec: `Sse.makeChannel` decodes the `data:`/`event:`/`id:`/`retry:` line protocol as package capability — the `retry:` directive is absorbed in-channel, the parser sleeping the hinted delay in place — the reattach cursor advances as a fold and stamps `last-event-id` on every re-dial, a cleanly completed response reconnects exactly like a faulted one, silence folds through the core degradation ladder to pick probe cadence, and `Sse.encoder` is the mirror the serving edge composes so both directions of the dialect have one codec owner. A raw socket listener, a hand `data:`-line parser, a reconnect that observes only transport faults, and `JSON.stringify` written to a wire are the named defects. The module is `runtime/src/net/channel.ts`.

## [01]-[CLUSTERS]

| [INDEX] | [CLUSTER]    | [OWNS]                                                                            | [PUBLIC] |
| :-----: | :----------- | :-------------------------------------------------------------------------------- | :------- |
|  [01]   | `FRAME_ROWS` | the duplex frame vocabulary — `ndjson \| msgpack` rows fused with one schema seam | `Duplex` |
|  [02]   | `FEED_SEAM`  | the SSE session — codec, cursor fold, Retry-driven reconnect, silence ladder      | `Feed`   |

## [02]-[FRAME_ROWS]

[FRAME_ROWS]:

- Owner: `Duplex` — the framed-socket owner. `Duplex.framed(socket, frame, protocol)` lifts a `Socket.Socket` to a typed message channel: `Socket.toChannelWith<E>()` turns the socket into a byte `Channel`, and the frame row's fused combinator — `Ndjson.duplexSchema` (newline-delimited, `NdjsonError`) or `MsgPack.duplexSchema` (length-delimited binary, `MsgPackError`) — owns chunk reassembly and the schema seam in one step, so both directions are decoded values and backpressure is channel-native.
- Law: the frame is a row swap under an unchanged schema seam — the `_frames` table keys each dialect to its fused combinator, dispatch is one keyed lookup, and `Duplex.Kind` derives from the table; moving a peer from ndjson to msgpack edits one argument and no consumer, and a new frame dialect is one row, zero arms.
- Law: the protocol pair is send/take symmetric evidence — `send` types the outbound seam, `take` the inbound seam, both usually one closed `Schema.Union` of tagged messages; an untyped frame crossing the channel is unspellable because the fused combinator is the only construction.
- Law: fault families arrive typed and stay separate — the frame's own error, `Socket.SocketError`, and `ParseError` each route on their own tag; none is re-wrapped.
- Boundary: socket construction is capability — `Socket.makeWebSocket(url)` demands the `WebSocketConstructor` Tag, satisfied by the runtime binding at the root; session lifetime, reconnect, and the pipeline geometry above the channel are the consumer's, composed from `Stream` law.
- Entry: `Duplex.framed(socket, frame, { send, take })`.
- Packages: `@effect/platform` (`Socket`, `Ndjson`, `MsgPack`), `effect` (`Channel`, `Chunk`, `Schema`).

```typescript signature
import { Sse } from '@effect/experimental';
import { type HttpClient, HttpClientRequest, MsgPack, Ndjson, Socket } from '@effect/platform';
import { type Channel, type Chunk, Context, Data, Duration, Effect, Layer, Option, type ParseResult, Ref, type Schema, Stream, pipe } from 'effect';
import { Budget, Degrade, type FaultClass } from '@rasm/ts/core';
import { Client } from './client.ts';

const _frames = { msgpack: MsgPack.duplexSchema, ndjson: Ndjson.duplexSchema } as const;

declare namespace Duplex {
    type Kind = keyof typeof _frames;
    type Fault = MsgPack.MsgPackError | Ndjson.NdjsonError;
    type Protocol<Send, SendI, Take, TakeI> = {
        readonly send: Schema.Schema<Send, SendI>;
        readonly take: Schema.Schema<Take, TakeI>;
    };
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
    );

const Duplex = { framed: _framed } as const;
```

## [03]-[FEED_SEAM]

[FEED_SEAM]:

- Owner: `Feed` — the server-sent-event ingress port every SSE consumer in the branch shares (`flag#GATE_SERVICE` is the standing consumer; the serving edge composes the same codec's `Sse.encoder` for the mirror side). `Feed.open(session)` yields the live event stream from one admitted `Feed.Session` carrier holding the origin, request headers, parser buffer bound, and clean-redial floor: the session dials the `feed` client lane (no total budget), admits status and `text/event-stream`, decodes the response body's bytes to text at the seam (`Stream.decodeText` — the channel consumes text lines), and pipes it through `Sse.makeChannel({ bufferSize })`, so authentication material, media admission, buffering, reconnect posture, and the line protocol remain one session fold and the consumer sees only decoded `Sse.Event` frames.
- Law: the `retry:` directive is package-absorbed pacing — `Sse.makeChannel` never emits `Sse.Retry` downstream: on a `retry:` frame the channel sleeps the hinted `duration` in place and resumes the same response, so server-driven pacing within a live connection is the codec's own behavior and no session cell shadows it.
- Law: reconnection covers every ending — the session is one `Stream.forever` over the dial, so a cleanly completed response re-dials after `session.redial` (a server that closes immediately cannot hot-loop), and a faulted response re-dials under `Budget.schedule("feed")` — the ledger owner's gate-modal compile, attempts, reset, jitter, and elapsed window intact, its `FaultClass.retryable` default gate passing because every retry-channel error is `FeedFault`, classed `unavailable` — with exhaustion minting `FeedFault` carrying the origin as evidence, the one fault consumers see.
- Law: the reattach cursor is a fold, not a cell convention — every `Sse.Event` carrying an `id` advances the cursor, and every re-dial — clean or faulted — stamps it as the `last-event-id` request header, so an outage backfills by event id; the cursor lives in the session's own `Ref`, invisible to consumers and surviving every re-dial.
- Law: silence is laddered, never polled — `Feed.cadence` is the one-hop `Degrade.cadence` fold from an observed quiet span to the rung's probe cadence a long-lived consumer schedules; the ladder folds the span it is handed, the consumer owns the measurement, and no consumer indexes a degradation row to reconstruct cadence.
- Law: the feed is transport only — it emits decoded `Sse.Event` frames and owns no payload vocabulary; the consumer's own Schema decodes `event.data` at its seam, so one feed serves every event dialect.
- Boundary: the serving mirror — `Sse.encoder` framing an outbound event stream over an HTTP response — is the edge wave's mount; this page owns the codec direction law so the dialect has one owner.
- Entry: `yield* Feed` then `feed.open(origin)`; `Feed.live` is the shipped Layer over the client lane.
- Packages: `@effect/experimental` (`Sse`), `@effect/platform` (`HttpClientRequest`), `effect` (`Context`, `Data`, `Duration`, `Option`, `Ref`, `Stream`), `./client.ts` (`Client`), `@rasm/ts/core` (`Budget`, `Degrade`).

```typescript signature
class FeedFault extends Data.TaggedError('FeedFault')<{
    readonly origin: string;
    readonly reason: 'transport' | 'media';
    readonly status: Option.Option<number>;
    readonly cursor: Option.Option<string>;
}> {
    get class(): FaultClass.Kind {
        return 'unavailable';
    }
}

class _Session extends Schema.Class<_Session>('Feed/Session')({
    origin: Schema.URLFromSelf,
    headers: Schema.optionalWith(Schema.Record({ key: Schema.String, value: Schema.String }), { default: () => ({}) }),
    buffer: Schema.optionalWith(Schema.Int.pipe(Schema.positive()), { default: () => 64 }),
    redial: Schema.optionalWith(Schema.Duration, { default: () => Duration.millis(500) }),
}) {}

class Feed extends Context.Tag('runtime/Feed')<
    Feed,
    {
        readonly open: (session: URL | Feed.Session) => Stream.Stream<Sse.Event, FeedFault>;
    }
>() {
    static readonly live: Layer.Layer<Feed, never, HttpClient.HttpClient> = Layer.effect(
        Feed,
        Effect.map(Effect.context<HttpClient.HttpClient>(), (context) => ({
            open: (session) => Stream.provideContext(_session(session instanceof URL ? new _Session({ origin: session }) : session), context),
        })),
    );
    static readonly cadence = (silence: Duration.DurationInput): Duration.Duration => Degrade.cadence(silence);
    static readonly Session = _Session;
}

declare namespace Feed {
    type Cursor = Option.Option<string>;
    type Session = _Session;
}

const _reattached = (session: Feed.Session, cursor: Feed.Cursor): HttpClientRequest.HttpClientRequest =>
    pipe(
        HttpClientRequest.get(session.origin.href).pipe(
            HttpClientRequest.setHeaders(session.headers),
            HttpClientRequest.setHeader('accept', 'text/event-stream'),
        ),
        (base) =>
            Option.match(cursor, {
                onNone: () => base,
                onSome: (id) => base.pipe(HttpClientRequest.setHeader('last-event-id', id)),
            }),
    );

const _pulled = (
    session: Feed.Session,
    request: HttpClientRequest.HttpClientRequest,
    cursor: Feed.Cursor,
): Stream.Stream<Sse.Event, FeedFault, HttpClient.HttpClient> =>
    Stream.unwrapScoped(
        Effect.flatMap(Client.dial('feed', request), (response) =>
            response.headers['content-type']?.split(';', 1)[0]?.trim().toLowerCase() === 'text/event-stream'
                ? Effect.succeed(
                      response.stream.pipe(Stream.decodeText(), Stream.pipeThroughChannel(Sse.makeChannel({ bufferSize: session.buffer }))),
                  )
                : Effect.fail(new FeedFault({ origin: session.origin.href, reason: 'media', status: Option.some(response.status), cursor })),
        ),
    ).pipe(
        Stream.mapError((fault) =>
            fault instanceof FeedFault ? fault : new FeedFault({ origin: session.origin.href, reason: 'transport', status: Option.none(), cursor }),
        ),
    );

const _session = (session: Feed.Session): Stream.Stream<Sse.Event, FeedFault, HttpClient.HttpClient> =>
    Stream.unwrap(
        Effect.map(Ref.make<Feed.Cursor>(Option.none()), (cursor) =>
            Stream.unwrap(Effect.map(Ref.get(cursor), (held) => _pulled(session, _reattached(session, held), held))).pipe(
                Stream.tap((event) => Ref.update(cursor, (held) => Option.orElse(Option.fromNullable(event.id), () => held))),
                Stream.concat(Stream.drain(Stream.fromEffect(Effect.sleep(session.redial)))), // the clean-EOF floor paces the next dial
                Stream.forever, // every ending re-dials: the cursor Ref outlives each response, so reattach rides last-event-id
                Stream.retry(Budget.schedule('feed')),
            ),
        ),
    );

// --- [EXPORTS] --------------------------------------------------------------------------

export { Duplex, Feed, FeedFault };
```
