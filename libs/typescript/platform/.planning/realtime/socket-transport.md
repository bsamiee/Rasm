# [PLATFORM_SOCKET_TRANSPORT]

One page owns the browser bidirectional socket edge — `SocketTransport`, the single `Effect.Service` driving the per-connection `SocketDial` `realtime:transport-modality#TRANSPORT_MODALITY` `dialModality` yields over the verified `@effect/platform-browser` `BrowserSocket.layerWebSocketConstructor`, the raw inbound-byte frame `Queue` the dial's `inbound` `Stream` drains into plus the polymorphic `subscribe<A,I>(schema)` that folds it through `Schema.decodeUnknown` against a consumer-supplied `interchange`-owned schema exactly as the SSE ingress folds its frames, the outbound `send` over the live `dial.send` write half re-published per connection, and the connection-lifecycle reconnect riding the reused `projection:fold-core/stream-policy#STREAM_POLICY` `StreamPolicy` reconnect `Schedule` (`socketReconnect.reconnect`) rather than a bespoke retry loop. It is the single bidirectional-socket owner the SSE-only ingress (`feature-flags:flag-stream#FLAG_STREAM`, a one-directional SSE channel) structurally lacks — a host owner needing a server-initiated push AND a client write shares this one connection, never a second ad-hoc `globalThis.WebSocket`. The transport composes the closed modality axis `realtime:transport-modality#TRANSPORT_MODALITY` owns: the `WebSocket` case is the one realized modality here and the `WebTransport`/presence-heartbeat/CRDT-op-log-push-back legs land as new cases inside that closed family, never a parallel socket. Connection-lifecycle and close-code faults fold into the one closed `SocketFault` `Data.TaggedEnum` `transport-modality.md` owns and this page re-exports (the transport's own ceremony, mirroring `identity-session:auth-session#AUTH_SESSION`'s `AuthFault`), while a malformed inbound frame folds to the `interchange:faults/fault-family#FAULT_FAMILY` `FaultDetail.HopFault` rail exactly as `worker:decode-pool#DECODE_POOL` and `flag-stream` route their decode failures — the transport authors no wire shape and re-mints no fault vocabulary.

## [1]-[INDEX]

[SOCKET_TRANSPORT]: the `SocketTransport` `Effect.Service` driving the `dialModality` `SocketDial`, the inbound `Queue` drain and the polymorphic `subscribe` decode fold, the outbound write over the live re-published `dial.send`, the re-exported `SocketFault` connection-fault family, and the reused `StreamPolicy` reconnect at the session retry.

## [2]-[SOCKET_TRANSPORT]

- Owner: `SocketTransport`, the single bidirectional-socket owner — the `TransportModality.WebSocket({ url })` value built once from the `RuntimeConfig` `socketUrl`, the per-connection `SocketDial` `dialModality(modality)` yields over the `BrowserSocket.layerWebSocketConstructor` `WebSocketConstructor`, the `inbound` raw-byte `Stream` draining the bounded `Queue` the dial's stream feeds, the `subscribe<A,I>(schema)` polymorphic decode fold over that one stream, the `send` outbound write over the live `dial.send` re-published per connection, and the `connection` `SubscriptionRef` carrying the live `ConnectionState`. The duplex dial is the one realtime vocabulary and a second `globalThis.WebSocket` constructed outside `transport-modality.md` is the named parallel-socket defect; the inbound frame is decoded once through `subscribe` and a second ad-hoc decode of the same bytes is the named double-decode defect; the transport is payload-agnostic so a consumer supplies the `interchange`-owned frame schema and the transport authors none.
- Cases: `SocketTransport` builds `TransportModality.WebSocket({ url })` from the `RuntimeConfig`-resolved `socketUrl` and calls `dialModality(modality)` inside the per-session `Effect.gen` so a reconnect re-dials a fresh `SocketDial` rather than baking the endpoint into a single `layerWebSocket` — `transport-modality.md` owns the `Socket.makeWebSocket(url)` construction, the `socket.runRaw` read-side lift through `Stream.asyncScoped`, and the `socket.writer` outbound half, so this page composes the `{ inbound, send }` dial without re-spelling the socket mechanic. The session drains `dial.inbound` through `Stream.runForEach` into the bounded `Queue.sliding(256)`, the `inbound` `Stream` is `Stream.fromQueue(frames)`, and the polymorphic `subscribe<A,I>(schema)` folds each frame through `Schema.decodeUnknown(schema)` against the consumer-supplied `interchange`-owned frame schema — the SAME decode discipline the SSE/wire ingress uses, the transport authoring no frame shape of its own — so a duplex push frame and an SSE delta frame share one decode rail and a decode failure folds to `FaultDetail.HopFault({ reason: "wire" })` and drops the one frame rather than tearing the connection; the `connection` `SubscriptionRef` advances through the closed `ConnectionState` `Data.TaggedEnum` (`Connecting`/`Open`/`Closed`) as the dial opens, runs, and closes so a consumer greys a send affordance off the cell rather than probing the raw socket. The outbound `send` reads the live `dial.send` write function from the `sink` `SubscriptionRef` cell the session re-publishes per connection — the inbound stream and the outbound write share the one dial, never a second channel — a `send` issued before the dial is open or after a close folds to `SocketFault.WriteFailed` (the `Option.none` cell arm), and a `SocketFault` raised on the dial inbound (a `SocketCloseError` folded to `SocketFault.Closed` by `dialFaultOf` in `transport-modality.md`) ends the session so the retry reattaches through the policy rather than spawning a parallel socket.
- Auto: the connection is forked `Effect.forkScoped` for the runtime lifetime — the `session` effect dials a fresh `SocketDial` through `dialModality(modality)`, publishes its `send` into the `sink` `SubscriptionRef` cell (cleared by an `Effect.addFinalizer` on session-scope exit so a stale writer never survives a closed dial), sets the `connection` cell to `Open`, and drains `dial.inbound` into the bounded `Queue`; the whole `Effect.scoped(session)` retries on the reused `projection` `StreamPolicy` `interactive` row reconnect `Schedule` (`socketReconnect.reconnect`) so a dial `SocketFault` re-dials on the one `250ms`-exponential-capped-by-`30s` `Schedule`, the `connection` cell flipping `Closed`->`Connecting`->`Open` across the redial and the `sink` cell re-publishing the fresh write function, never a bespoke `Stream.retry` re-spelled per leg. The inbound `Queue.sliding(256)` is the stable buffer the consumer reads across reconnects — the sliding strategy drops oldest under a stalled consumer rather than back-pressuring the dial read — so the reconnect operator lives on the session retry and the buffer on the queue, the two `StreamPolicy` concerns decomposed to their two correct sites.
- Packages: `@effect/platform-browser` `BrowserSocket.layerWebSocketConstructor` for the `Socket.WebSocketConstructor` the `dialModality` dial composes (provided as the service dependency); `effect` `Schema.decodeUnknown` for the inbound frame decode, `Queue.sliding` for the bounded inbound buffer, `SubscriptionRef` for the `ConnectionState` and live-`sink` cells, `Option` for the writer-present arm, `Stream.fromQueue`/`Stream.runForEach`/`Stream.mapEffect` for the queue drain and decode fold, `Effect.scoped`/`Effect.addFinalizer`/`Effect.retry`/`Effect.forkScoped` for the retried duplex session, and `Data.taggedEnum` for the `ConnectionState` family; the `realtime` `TransportModality`/`dialModality`/`SocketFault`/`SocketDial`/`socketReconnect` for the modality axis, the per-connection dial, the re-exported fault family, and the reconnect `Schedule` (`socketReconnect.reconnect`); the `interchange` `FaultDetail` for the decode-failure rail (the inbound frame schema arrives from the consumer at `subscribe`, never re-authored here); `RuntimeConfig` for the `socketUrl`.
- Growth: the `WebTransport` push leg lands as one `TransportModality` case `dialModality` already projects into the SAME `SocketDial` `{ inbound, send }` shape (a `WebTransport` `datagrams.readable`/`writable` pair lifting its read side into the one inbound `Queue` and its write side into the `sink` cell), the duplex contract and the `subscribe` decode rail unchanged; a presence/heartbeat leg lands as one outbound `send` cadence over the one writer, never a second socket; the CRDT op-log push-back leg rides this one duplex transport (or the catalogued `@effect/experimental` `EventLogRemote.layerWebSocketBrowser` op-log surface where that owns the sync semantics) as one modality case, so the duplex transport is minted once here and never twice; a new server-push payload lands as one consumer `subscribe(schema)` call against the `interchange`-owned shape, never a transport-side frame schema; a new connection-lifecycle fault lands as one `SocketFault` case, never a flat `reason` literal; a new reconnect cadence lands as one `StreamPolicy` row, never an inline `Schedule`.
- Boundary: `SocketTransport` is the single bidirectional-socket owner — a host owner needing server-push plus a client write reads/writes this one duplex resource and a second `globalThis.WebSocket` anywhere is the named parallel-socket defect; the inbound frame decodes once through `subscribe` against the consumer-supplied `interchange`-owned schema and the transport authors no wire vocabulary (a transport-side frame schema is the named drift defect graded against the `interchange` owner), a decode failure folding to `FaultDetail.HopFault({ reason: "wire" })` exactly as the worker and flag-stream decode failures route; the transport-level connection faults fold to the one `SocketFault` `Data.TaggedEnum` `transport-modality.md` owns and this page re-exports, and a flat `reason` string union is the deleted form; the reconnect rides the reused `projection` `StreamPolicy` and an improvised retry loop is the deleted form; `SocketTransport` reads its endpoint through `RuntimeConfig`, never a direct `import.meta.env` read; it emits no command (a mutation rides the `interchange` `CommandGateway`, never an unframed socket write bypassing it) and a pushed frame reaches a component through the `ui` `AtomBinding`, never a second state binding — `ui` never imports `platform`.

```ts contract
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
import type { Scope } from "effect";
import { Data, Effect, Option, Queue, Schema, Stream, SubscriptionRef } from "effect";
import * as Socket from "@effect/platform/Socket";
import * as BrowserSocket from "@effect/platform-browser/BrowserSocket";
import { FaultDetail } from "../../interchange/faults/fault-family.ts";
import { RuntimeConfig } from "../runtime-config/runtime-config.ts";
import { TransportModality, SocketFault, dialModality, socketReconnect, type SocketDial } from "./transport-modality.ts";

// --- [TYPES] ---------------------------------------------------------------------------
type ConnectionState = Data.TaggedEnum<{
  readonly Connecting: object;
  readonly Open: object;
  readonly Closed: { readonly code: number };
}>;
const ConnectionState = Data.taggedEnum<ConnectionState>();

// --- [ERRORS] --------------------------------------------------------------------------
const closedCodeOf = (fault: SocketFault): number =>
  SocketFault.$match(fault, { Closed: ({ code }) => code, DialFailed: () => 1006, WriteFailed: () => 1006 });

// --- [SERVICES] ------------------------------------------------------------------------
interface SocketTransport {
  readonly modality: TransportModality;
  readonly connection: SubscriptionRef.SubscriptionRef<ConnectionState>;
  readonly inbound: Stream.Stream<Uint8Array>;
  readonly subscribe: <A, I>(schema: Schema.Schema<A, I>) => Stream.Stream<A, FaultDetail>;
  readonly send: (frame: Uint8Array) => Effect.Effect<void, SocketFault>;
}

// --- [COMPOSITION] ---------------------------------------------------------------------
class SocketTransportLive extends Effect.Service<SocketTransportLive>()("@rasm/ts/platform/SocketTransport", {
  scoped: Effect.gen(function* () {
    const config = yield* RuntimeConfig;
    const url = yield* config.socketUrl;
    const modality = TransportModality.WebSocket({ url });

    const connection = yield* SubscriptionRef.make<ConnectionState>(ConnectionState.Connecting());
    const frames = yield* Queue.sliding<Uint8Array>(256);
    const sink = yield* SubscriptionRef.make<Option.Option<SocketDial["send"]>>(Option.none());

    const session: Effect.Effect<void, SocketFault, Scope.Scope | Socket.WebSocketConstructor> = Effect.gen(function* () {
      yield* SubscriptionRef.set(connection, ConnectionState.Connecting());
      const dial = yield* dialModality(modality);
      yield* SubscriptionRef.set(sink, Option.some(dial.send));
      yield* Effect.addFinalizer(() => SubscriptionRef.set(sink, Option.none()));
      yield* SubscriptionRef.set(connection, ConnectionState.Open());
      yield* Stream.runForEach(dial.inbound, (bytes) => Queue.offer(frames, bytes));
    });

    yield* Effect.scoped(session).pipe(
      Effect.tapError((fault) => SubscriptionRef.set(connection, ConnectionState.Closed({ code: closedCodeOf(fault) }))),
      Effect.retry(socketReconnect.reconnect),
      Effect.forkScoped,
    );

    const inbound: Stream.Stream<Uint8Array> = Stream.fromQueue(frames);

    const subscribe = <A, I>(schema: Schema.Schema<A, I>): Stream.Stream<A, FaultDetail> =>
      inbound.pipe(
        Stream.mapEffect((bytes) =>
          Schema.decodeUnknown(schema)(bytes).pipe(
            Effect.catchTag("ParseError", () => Effect.fail(FaultDetail.HopFault({ reason: "wire", evidence: {} }))),
          ),
        ),
      );

    const send = (frame: Uint8Array): Effect.Effect<void, SocketFault> =>
      SubscriptionRef.get(sink).pipe(
        Effect.flatMap(Option.match({
          onNone: () => Effect.fail(SocketFault.WriteFailed({ cause: "socket-not-open" })),
          onSome: (write) => write(frame),
        })),
      );

    return { modality, connection, inbound, subscribe, send } satisfies SocketTransport;
  }),
  dependencies: [BrowserSocket.layerWebSocketConstructor],
}) {}

// --- [EXPORTS] -------------------------------------------------------------------------
export { type SocketTransport, type ConnectionState, ConnectionState, SocketTransportLive };
```
