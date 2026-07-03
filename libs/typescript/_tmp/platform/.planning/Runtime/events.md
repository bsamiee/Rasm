# [PLATFORM_EVENTS]

One page owns the generic `addEventListener`/`removeEventListener` Stream bridge for the non-`WindowEventMap` event targets — `scopedEventStream`, one `Stream.asyncScoped` over an `Effect.acquireRelease` listener pair generic over the target's event type, the single ingress every host owner whose source is not `window`/`document` composes. `@effect/platform-browser` `BrowserStream.fromEventListenerWindow`/`fromEventListenerDocument` are keyed over `WindowEventMap`/`DocumentEventMap` and never reach `window.navigation`'s `navigate`, the `workbox-window` `Workbox` lifecycle target, or a `PerformanceObserver`; this owner is the one scoped resource those three name rather than three hand-rolled `acquireRelease` listener blocks. The page authors no decode and crosses no wire.

## [01]-[INDEX]

- [01]-[SCOPED_EVENT_STREAM]: the generic scoped `addEventListener`->`removeEventListener` `Stream` bridge over a non-`WindowEventMap` target.

## [02]-[SCOPED_EVENT_STREAM]

- Owner: `scopedEventStream`, the one `Stream.asyncScoped` bridge lifting an `addEventListener`/`removeEventListener` pair on an arbitrary `EventTarget`-shaped source into a back-pressured `Stream`, the listener registration and teardown owned by one `Effect.acquireRelease` so scope closure removes the listener exactly once.
- Cases: the bridge is generic over the emitted event type `E` and the listener shape, so `window.navigation`'s `navigate` ingress (`NavigateEvent`, absent from `WindowEventMap`), the `workbox-window` `Workbox` lifecycle target (`WorkboxLifecycleEvent`), and a `PerformanceObserver` registration each compose this one owner instead of re-deriving the `Stream.asyncScoped`+`Effect.acquireRelease` skeleton; the `add`/`remove` pair arrives as a closure over the precise target so the element type stays exact — `Session/router.md`'s `navigate` stream, `Shell/serviceworker.md`'s `SwLifecycle` stream, and `Observability/vitals.md`'s vital stream each lift their target's native event shape through this bridge with zero `as any` and zero `EventListenerOrEventListenerObject` widening.
- Auto: `scopedEventStream` takes a `register: (emit: (event: E) => void) => Listener` acquire closure that wires the native listener and returns the handle, and a `release: (listener: Listener) => void` teardown closure, lifting them through `Effect.acquireRelease` inside `Stream.asyncScoped`'s callback so the `emit.single` push carries each event into the stream's bounded buffer and the listener detaches on scope exit; the `bufferSize` knob caps the back-pressure window so a fast native source never grows an unbounded queue, mirroring the `BrowserStream` window-listener back-pressure contract for the targets `BrowserStream` cannot type.
- Packages: `effect` `Stream.asyncScoped` for the scoped async ingress and `Effect.acquireRelease` for the listener lifetime; no `@effect/platform-browser` `BrowserStream` member reaches a non-`WindowEventMap`/`DocumentEventMap` target, so the three host owners compose this generic bridge rather than the keyed window/document constructors.
- Growth: a new non-`WindowEventMap` event source lands as one `scopedEventStream` call with its `register`/`release` pair, never a fourth hand-rolled `Stream.asyncScoped` listener block; a buffered-versus-unbounded back-pressure choice lands as the `bufferSize` argument, never a parallel constructor.
- Boundary: `scopedEventStream` owns the non-`WindowEventMap` listener bridge only — a `window`/`document` event source composes `BrowserStream.fromEventListenerWindow`/`fromEventListenerDocument` (keyed over the native event maps), never this generic form, so a `BrowserStream`-reachable source routed through `scopedEventStream` is the named redundant-ingress defect; the bridge authors no decode, holds no domain state, and the `BOUNDARY ADAPTER` listener-registration body is the platform-forced statement seam wiring the foreign event handler.

```ts contract
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
import { Effect, Stream } from "effect";

// --- [OPERATIONS] ----------------------------------------------------------------------
const scopedEventStream = <E, Listener>(
  register: (emit: (event: E) => void) => Listener,
  release: (listener: Listener) => void,
  bufferSize: number | "unbounded" = 64,
): Stream.Stream<E, never, never> =>
  Stream.asyncScoped<E>(
    (emit) =>
      Effect.acquireRelease(
        Effect.sync(() => register((event) => emit.single(event))), // BOUNDARY ADAPTER: native listener attach
        (listener) => Effect.sync(() => release(listener)),
      ),
    bufferSize,
  );

// --- [EXPORTS] -------------------------------------------------------------------------
export { scopedEventStream };
```
