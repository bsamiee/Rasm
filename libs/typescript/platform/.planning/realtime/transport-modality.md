# [PLATFORM_TRANSPORT_MODALITY]

One page owns the closed bidirectional-transport vocabulary the `realtime/socket-transport.md#SOCKET_TRANSPORT` `SocketTransport` owner dials through — `TransportModality`, the one `Data.TaggedEnum` whose cases (`WebSocket`/`WebTransport`) are the same duplex concept resolved against two host primitives, never two parallel socket classes. The modality is a value `SocketTransport` constructs from the `RuntimeConfig`-resolved url at connection time and folds through one total `TransportModality.$match` into a scoped `SocketDial` `{ inbound, send }` composing the verified `@effect/platform-browser` `BrowserSocket.layerWebSocketConstructor` (`folder:typescript/platform/.api/effect-platform-browser#BROWSER_SOCKET_MODALITY_LAW`), so a host that re-dials a fresh url per reconnect builds the `Socket` per-connection rather than baking a url into a single `layerWebSocket`. `dialModality` is a PER-CONNECTION dial — reconnect (re-dial) cannot live inside one dial, so the `socketReconnect` `interactive`-row `StreamPolicy` it exports carries the reconnect `Schedule` and the back-pressure posture that `realtime/socket-transport.md#SOCKET_TRANSPORT` applies at its retried session scope (`socketReconnect.reconnect`), never an `EventSource`-style retry loop or a bare `Stream.retry` re-spelled beside the dial. The page authors no decode, mints no second `globalThis.WebSocket`, and crosses no wire: it is the growth axis the WebTransport leg, the presence/heartbeat row, and the CRDT op-log push-back compose as new cases, and a second ad-hoc socket constructed anywhere is the named parallel-socket defect.

## [1]-[INDEX]

[TRANSPORT_MODALITY]: the closed `TransportModality` case family, the `TransportModality.$match` dial fold composing the verified `BrowserSocket` constructor, the `SocketFault` fault family and the `SocketDial` shape, the exported `socketReconnect` `StreamPolicy` the session applies, and the WebTransport / CRDT-op-log-push-back growth seam.

## [2]-[TRANSPORT_MODALITY]

- Owner: `TransportModality`, the single bidirectional-transport vocabulary — one closed `Data.TaggedEnum` whose `WebSocket` case carries the `{ url }` the constructor dials and whose `WebTransport` case carries the `{ url, congestionControl }` the HTTP/3 datagram session opens; `dialModality`, the one total `TransportModality.$match` fold projecting a case into a scoped `SocketDial` `{ inbound: Stream.Stream<Uint8Array, SocketFault>, send: (frame) => Effect<void, SocketFault> }` bound to one scoped `Socket.Socket` (or `WebTransport`) resource; `SocketFault`, the one closed `Data.TaggedEnum` (`DialFailed`/`Closed`/`WriteFailed`) the dial raises and `socket-transport.md` re-exports; and `socketReconnect`, the `interactive`-row `StreamPolicy` whose reconnect `Schedule` (`socketReconnect.reconnect`) and back-pressure posture `socket-transport.md` applies at its retried session scope. The `TransportModality` family is the one transport axis `SocketTransport` discriminates on and a second transport class beside this family is the named parallel-socket defect.
- Cases: `TransportModality` is `WebSocket` (the Baseline duplex case — `dialWebSocket` builds one `Socket.Socket` through `Socket.makeWebSocket(url)` resolving the `Socket.WebSocketConstructor` from the `BrowserSocket.layerWebSocketConstructor` tag, so a `RuntimeConfig`-resolved or per-reconnect-fresh url builds the socket at dial time rather than baking into a single `layerWebSocket` — the read side lifts `socket.runRaw` through `Stream.asyncScoped` into the inbound `Stream<Uint8Array, SocketFault>`, normalizing a `string` chunk through `TextEncoder`, and the `socket.writer` acquired once is the outbound half over the one duplex resource) and `WebTransport` (the HTTP/3 growth case — `dialWebTransport` opens one native `WebTransport(url, { congestionControl })` session under `Effect.acquireRelease`, awaits `ready`, acquires the `datagrams.writable` writer once under its own `acquireRelease`, and lifts `datagrams.readable` through `Stream.fromAsyncIterable` into the same `Stream<Uint8Array, SocketFault>` shape, the `WebTransport` global being absent from the duplex `Socket` driver so this case is the one sanctioned native session — never a second parallel owner); `dialModality` is one `TransportModality.$match` over the two cases producing the identical `SocketDial` `{ inbound, send }` shape, so `SocketTransport` reads one dial surface regardless of modality and a new transport lands as one case plus one `$match` arm, never a parallel dial path. A transport-level failure folds through `dialFaultOf(modality)`: a `Socket.SocketCloseError` (verified `Socket.SocketCloseError.is`) becomes `SocketFault.Closed({ code })`, any other dial/read cause becomes `SocketFault.DialFailed({ modality, cause })`, and a write rejection becomes `SocketFault.WriteFailed({ cause })` — the one `SocketFault` family `socket-transport.md` re-exports.
- Entry: `dialModality(modality)` is the sole ingress `SocketTransport` calls inside its retried session scope — it builds the per-connection `Socket`/`WebTransport` resource (scope-released on the constructor path, `transport.close()`/`writer.releaseLock()` on the `WebTransport` path), returns the `SocketDial` `{ inbound, send }`, and the session drains `dial.inbound` into the transport's bounded `Queue` while re-publishing `dial.send`; the reconnect (re-dial) lives at the `socket-transport.md` session retry on `socketReconnect.reconnect`, not in the per-connection dial, and the `Schema.decodeUnknown` frame fold runs in `socket-transport.md` over the drained queue.
- Packages: `effect` `Data.taggedEnum` for the closed `TransportModality`/`SocketFault` families and the total `$match` fold, `Stream.asyncScoped`/`Stream.fromAsyncIterable`/`Effect.acquireRelease`/`Scope` for the per-connection inbound source and outbound write, and `Schedule`/`Duration` only transitively through the reused `StreamPolicy`; `@effect/platform-browser` `BrowserSocket.layerWebSocketConstructor` for the verified dynamic-url `Socket.WebSocketConstructor` tag and `@effect/platform` `Socket` (`Socket.makeWebSocket`/`socket.runRaw`/`socket.writer`/`Socket.SocketCloseError.is`) for the duplex read/write over the one scoped resource; `projection` `fold-core/stream-policy` `streamPolicy` for the reused reconnect-and-back-pressure `StreamPolicy` (applied at the `socket-transport.md` session, not piped here); `runtime-config` `RuntimeConfig` for the dialed url (read at `socket-transport.md`); the native `WebTransport` global for the HTTP/3 case (absent from the `Socket` driver, confined to this owner).
- Growth: a new bidirectional transport lands as one `TransportModality` case carrying its dial payload plus one `dialModality` `$match` arm projecting it into the identical `SocketDial` shape — the case break propagates to every `$match` site at compile time, never a parallel dial path; a presence/heartbeat leg lands as one outbound `send` row over the existing write half (a periodic `Schedule`-driven control frame), never a second socket; the CRDT op-log push-back rides the admitted `@effect/experimental` `EventLogRemote.layerWebSocketBrowser` (the verified browser sync layer that itself requires only `Socket.WebSocketConstructor` and folds the browser socket plus subtle-crypto) composed UNDER the same `BrowserSocket.layerWebSocketConstructor` the `WebSocket` case dials — so the op-log sync surface and the frame transport share one constructor and the duplex is never minted twice — a fresh `SocketTransport` modality for op-log sync is the rejected double-mint; a reconnect-cadence change is one `PolicyProfile` row number on the reused `StreamPolicy`, never a per-modality `Schedule`.
- Boundary: `TransportModality` is the closed transport axis only — it owns no frame `Schema`, no inbound decode, and no flag/auth/geometry vocabulary; the `Schema.decodeUnknown` inbound frame fold and the `SocketTransport` `Effect.Service` lifetime stay settled at `realtime/socket-transport.md#SOCKET_TRANSPORT` and a re-declared frame schema here is the named anti-spam defect; the `WebTransport` native session and the `EventLogRemote` op-log sync compose only inside this owner behind the one `TransportModality` axis, so a `new WebSocket(...)` or a bare `new WebTransport(...)` at any other owner is the named ungated-native-socket defect; the reused `StreamPolicy` is referenced from `projection`, never re-baked; `TransportModality` emits no command and dials no transport beyond the one scoped `Socket`/session, and a decoded frame reaches a component through the `ui` `AtomBinding`, never a second state binding.

```ts contract
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
import { Data, Effect, Scope, Stream } from "effect";
import * as Socket from "@effect/platform/Socket";
import * as BrowserSocket from "@effect/platform-browser/BrowserSocket";
import { streamPolicy, type StreamPolicy } from "../../projection/fold-core/stream-policy.ts";

// --- [TYPES] ---------------------------------------------------------------------------
type TransportModality = Data.TaggedEnum<{
  readonly WebSocket: { readonly url: string };
  readonly WebTransport: { readonly url: string; readonly congestionControl: "default" | "throughput" | "low-latency" };
}>;
const TransportModality = Data.taggedEnum<TransportModality>();

type SocketDial = {
  readonly inbound: Stream.Stream<Uint8Array, SocketFault>;
  readonly send: (frame: Uint8Array) => Effect.Effect<void, SocketFault>;
};

// --- [ERRORS] --------------------------------------------------------------------------
type SocketFault = Data.TaggedEnum<{
  readonly DialFailed: { readonly modality: ModalityTag; readonly cause: unknown };
  readonly Closed: { readonly code: number };
  readonly WriteFailed: { readonly cause: unknown };
}>;
const SocketFault = Data.taggedEnum<SocketFault>();

type ModalityTag = TransportModality["_tag"];

const dialFaultOf =
  (modality: ModalityTag) =>
  (cause: unknown): SocketFault =>
    Socket.SocketCloseError.is(cause) ? SocketFault.Closed({ code: cause.code }) : SocketFault.DialFailed({ modality, cause });

// --- [CONSTANTS] -----------------------------------------------------------------------
// The reconnect Schedule + back-pressure posture is one StreamPolicy row; re-dial lives at the
// SocketTransport session scope (socketReconnect.reconnect), never re-baked here.
const socketReconnect: StreamPolicy = streamPolicy("interactive");

// --- [OPERATIONS] ----------------------------------------------------------------------
const dialWebSocket = (url: string): Effect.Effect<SocketDial, SocketFault, Scope.Scope | Socket.WebSocketConstructor> =>
  Effect.gen(function* () {
    const socket = yield* Socket.makeWebSocket(url);
    const write = yield* socket.writer;
    const inbound: Stream.Stream<Uint8Array, SocketFault> = Stream.asyncScoped<Uint8Array, SocketFault>((emit) =>
      socket
        .runRaw((chunk) => {
          void emit.single(typeof chunk === "string" ? new TextEncoder().encode(chunk) : chunk);
        })
        .pipe(
          Effect.catchAll((cause) => Effect.promise(() => emit.fail(dialFaultOf("WebSocket")(cause)))),
          Effect.forkScoped,
        ),
    );
    const send = (frame: Uint8Array): Effect.Effect<void, SocketFault> =>
      write(frame).pipe(Effect.mapError((cause) => SocketFault.WriteFailed({ cause }) as SocketFault));
    return { inbound, send } satisfies SocketDial;
  });

const dialWebTransport = (
  url: string,
  congestionControl: "default" | "throughput" | "low-latency",
): Effect.Effect<SocketDial, SocketFault, Scope.Scope> =>
  Effect.gen(function* () {
    const transport = yield* Effect.acquireRelease(
      Effect.tryPromise({
        try: () => {
          const session = new WebTransport(url, { congestionControl }); // BOUNDARY ADAPTER: native HTTP/3 session
          return session.ready.then(() => session);
        },
        catch: dialFaultOf("WebTransport"),
      }),
      (session) => Effect.sync(() => session.close()),
    );
    const writer = yield* Effect.acquireRelease(
      Effect.sync(() => transport.datagrams.writable.getWriter()),
      (w) => Effect.sync(() => w.releaseLock()),
    );
    const inbound: Stream.Stream<Uint8Array, SocketFault> = Stream.fromAsyncIterable(
      transport.datagrams.readable as unknown as AsyncIterable<Uint8Array>,
      dialFaultOf("WebTransport"),
    );
    const send = (frame: Uint8Array): Effect.Effect<void, SocketFault> =>
      Effect.tryPromise({ try: () => writer.write(frame), catch: (cause) => SocketFault.WriteFailed({ cause }) });
    return { inbound, send } satisfies SocketDial;
  });

const dialModality = (
  modality: TransportModality,
): Effect.Effect<SocketDial, SocketFault, Scope.Scope | Socket.WebSocketConstructor> =>
  TransportModality.$match(modality, {
    WebSocket: ({ url }) => dialWebSocket(url),
    WebTransport: ({ url, congestionControl }) => dialWebTransport(url, congestionControl),
  });

// --- [COMPOSITION] ---------------------------------------------------------------------
const socketConstructor = BrowserSocket.layerWebSocketConstructor;

// --- [EXPORTS] -------------------------------------------------------------------------
export { type ModalityTag, type SocketDial, type SocketFault, SocketFault, TransportModality, dialModality, socketConstructor, socketReconnect };
```
