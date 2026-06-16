# [API_CATALOGUE] effect-platform-browser

Grounded from installed `node_modules` type declarations (`@effect/platform-browser` 0.76.0;
peers `@effect/platform` ^0.96.0, `effect` ^3.21.0). Covers the full public surface of the
package — ten namespace modules re-exported from the package root. These are the browser-tier
platform bindings: the runtime entrypoint (`runMain`), the `HttpClient`/`Socket`/`Worker`/
`KeyValueStore` layer drivers that satisfy the platform-neutral service tags inside the browser
bundle, the DOM event-stream constructors, and three browser-API service capsules
(`Clipboard`, `Geolocation`, `Permissions`). Owner-symbol consumers: `CompositionRoot` /
`BrowserHost` (runtime-host), `WireTransport` (WebSocket leg), `SnapshotFeed` /
`PreferenceStore` (storage), and the worker-offload and capability-service rows on the
browser tier. Versions live only in the root `pnpm-workspace.yaml` / lockfile; this page carries
the reflected version as evidence.

The package root (`index.d.ts`) re-exports each file as a namespace:
`BrowserHttpClient`, `BrowserKeyValueStore`, `BrowserRuntime`, `BrowserSocket`,
`BrowserStream`, `BrowserWorker`, `BrowserWorkerRunner`, `Clipboard`, `Geolocation`,
`Permissions`. All symbols below are accessed namespace-qualified (e.g.
`BrowserRuntime.runMain`, `BrowserSocket.layerWebSocket`).

---

## [BrowserRuntime]

### runMain

```typescript
// @effect/platform-browser/BrowserRuntime
import type { RunMain } from "@effect/platform/Runtime";

export declare const runMain: RunMain;
```

The single browser application entrypoint. `RunMain` (from `@effect/platform/Runtime`) executes
the top-level `Effect`, wires teardown/interruption to the browser lifecycle, and reports
defects. `CompositionRoot` runs the fully-provided root effect through `runMain`; it is the
browser-tier analogue of the node `runMain` and the sole point where the Effect program meets
the host.

---

## [BrowserHttpClient]

### layerXMLHttpRequest

```typescript
// @effect/platform-browser/BrowserHttpClient
export declare const layerXMLHttpRequest: Layer.Layer<HttpClient.HttpClient>;
```

Provides the platform-neutral `HttpClient.HttpClient` tag backed by `XMLHttpRequest`. The XHR
driver (rather than `fetch`) is what unlocks the `arraybuffer` response-type and streaming
progress used by binary transport. This is the layer satisfying any `HttpClient`-requiring
effect inside the browser bundle.

### XMLHttpRequest

```typescript
declare const XMLHttpRequest_base: Context.TagClass<
  XMLHttpRequest,
  "@effect/platform-browser/BrowserHttpClient/XMLHttpRequest",
  LazyArg<globalThis.XMLHttpRequest>
>;

export declare class XMLHttpRequest extends XMLHttpRequest_base {}
```

A `Context.Tag` whose service value is a `LazyArg<globalThis.XMLHttpRequest>` — the factory the
client uses to construct each request object. Override this tag to inject a test double or a
non-global XHR constructor.

### currentXHRResponseType

```typescript
export declare const currentXHRResponseType: FiberRef.FiberRef<"text" | "arraybuffer">;
```

The `FiberRef` controlling the `responseType` of XHR requests on the current fiber. Defaults to
`"text"`; flip to `"arraybuffer"` for binary payloads.

### withXHRArrayBuffer

```typescript
export declare const withXHRArrayBuffer: <A, E, R>(effect: Effect<A, E, R>) => Effect<A, E, R>;
```

Scoped setter for `currentXHRResponseType` — runs `effect` with the XHR response type pinned to
`"arraybuffer"`. This is the load-bearing combinator for the binary wire leg: wrap any
`HttpClient` call carrying proto/msgpack bytes so the XHR returns an `ArrayBuffer` instead of a
decoded string.

---

## [BrowserSocket]

### layerWebSocket

```typescript
// @effect/platform-browser/BrowserSocket
export declare const layerWebSocket: (
  url: string,
  options?: {
    readonly closeCodeIsError?: (code: number) => boolean;
  },
) => Layer.Layer<Socket.Socket>;
```

Constructs a `Socket.Socket` layer bound to a single WebSocket `url`, backed by
`globalThis.WebSocket`. `closeCodeIsError` classifies which close codes surface as a `Socket`
error versus a clean close (default treats abnormal codes as errors). This is the browser-tier
driver for the streaming transport leg — `WireTransport`'s WebSocket subscriptions resolve
their `Socket` requirement here.

### layerWebSocketConstructor

```typescript
export declare const layerWebSocketConstructor: Layer.Layer<Socket.WebSocketConstructor>;
```

Provides the `Socket.WebSocketConstructor` tag backed by `globalThis.WebSocket`. Lower-level than
`layerWebSocket`: it supplies only the constructor, letting downstream code build sockets to
dynamic URLs. Override to inject a polyfilled or instrumented WebSocket constructor.

---

## [BrowserStream]

### fromEventListenerWindow

```typescript
// @effect/platform-browser/BrowserStream
export declare const fromEventListenerWindow: <K extends keyof WindowEventMap>(
  type: K,
  options?:
    | boolean
    | {
        readonly capture?: boolean;
        readonly passive?: boolean;
        readonly once?: boolean;
        readonly bufferSize?: number | "unbounded" | undefined;
      }
    | undefined,
) => Stream.Stream<WindowEventMap[K], never, never>;
```

Lifts `window.addEventListener(type, ...)` into a typed `Stream`. The element type is resolved
from `WindowEventMap[K]`, so `fromEventListenerWindow("resize")` yields `Stream<UIEvent>`,
`"online"` yields `Stream<Event>`, etc. `bufferSize` caps the back-pressure buffer (default
bounded; `"unbounded"` opts out). Registration and teardown are scoped to the stream lifetime.

### fromEventListenerDocument

```typescript
export declare const fromEventListenerDocument: <K extends keyof DocumentEventMap>(
  type: K,
  options?:
    | boolean
    | {
        readonly capture?: boolean;
        readonly passive?: boolean;
        readonly once?: boolean;
        readonly bufferSize?: number | "unbounded" | undefined;
      }
    | undefined,
) => Stream.Stream<DocumentEventMap[K], never, never>;
```

Same construction against `document.addEventListener`, typed via `DocumentEventMap[K]`
(e.g. `"visibilitychange"`, `"keydown"`). These two are the canonical DOM-event ingress for the
browser tier — UI event sources enter the Effect world as a `Stream` rather than imperative
listener callbacks.

---

## [BrowserKeyValueStore]

### layerLocalStorage

```typescript
// @effect/platform-browser/BrowserKeyValueStore
export declare const layerLocalStorage: Layer.Layer<KeyValueStore.KeyValueStore>;
```

Provides the platform-neutral `KeyValueStore.KeyValueStore` tag backed by
`window.localStorage` — values persist across sessions. The durable preference / cached-state
row resolves its `KeyValueStore` requirement here.

### layerSessionStorage

```typescript
export declare const layerSessionStorage: Layer.Layer<KeyValueStore.KeyValueStore>;
```

Same tag backed by `window.sessionStorage` — values live only for the current browser session.
The two layers are interchangeable at the tag level; the choice is persistence scope (session
versus cross-session).

---

## [BrowserWorker]

### layerManager

```typescript
// @effect/platform-browser/BrowserWorker
export declare const layerManager: Layer.Layer<Worker.WorkerManager>;
```

Provides the `Worker.WorkerManager` tag — the pool manager that routes work items across spawned
workers. Composes over a `Worker.Spawner` (supplied by `layer` / `layerPlatform`).

### layerWorker

```typescript
export declare const layerWorker: Layer.Layer<Worker.PlatformWorker>;
```

Provides the `Worker.PlatformWorker` tag — the browser-specific worker driver (message-port
plumbing) that backs the platform-neutral worker protocol.

### layer

```typescript
export declare const layer: (
  spawn: (id: number) => Worker | SharedWorker | MessagePort,
) => Layer.Layer<Worker.WorkerManager | Worker.Spawner>;
```

The high-level worker layer: takes a `spawn` factory (returning a `Worker`, `SharedWorker`, or
`MessagePort` per worker id) and yields both `WorkerManager` and `Spawner` together. This is the
entrypoint for the worker-offload row — heavy compute dispatched off the main thread provides
this layer with a `spawn` that constructs the project's worker entry module.

### layerPlatform

```typescript
export declare const layerPlatform: (
  spawn: (id: number) => globalThis.Worker | globalThis.SharedWorker | MessagePort,
) => Layer.Layer<Worker.PlatformWorker | Worker.Spawner>;
```

Lower-level variant of `layer`: yields `PlatformWorker | Spawner` (the driver + spawner) without
the `WorkerManager`, for composing a custom manager on top.

---

## [BrowserWorkerRunner]

The dual of `BrowserWorker` — runs *inside* a worker/`MessagePort`, executing requests the
manager dispatches.

### make

```typescript
// @effect/platform-browser/BrowserWorkerRunner
export declare const make: (self: MessagePort | Window) => Runner.PlatformRunner;
```

Constructs a `Runner.PlatformRunner` from the worker's own `MessagePort` or `Window` (i.e.
`self` inside a dedicated or shared worker). This is the worker-side counterpart to the
main-thread `spawn`.

### layer

```typescript
export declare const layer: Layer.Layer<Runner.PlatformRunner>;
```

Provides `Runner.PlatformRunner` using the ambient worker global. The default worker-entry layer
when the runner runs in a dedicated worker with the standard `self` global.

### layerMessagePort

```typescript
export declare const layerMessagePort: (
  port: MessagePort | Window,
) => Layer.Layer<Runner.PlatformRunner>;
```

Same layer parameterised by an explicit `MessagePort | Window` — for shared workers or when the
port is not the ambient global.

### launch (re-export)

```typescript
export { launch } from "@effect/platform/WorkerRunner";
```

Re-exported from `@effect/platform/WorkerRunner`. Runs a configured `WorkerRunner` effect to
completion as the worker-entry main; the worker-side analogue of `BrowserRuntime.runMain`.

---

## [Clipboard]

A service capsule over `navigator.clipboard`.

### TypeId / ErrorTypeId

```typescript
// @effect/platform-browser/Clipboard
export declare const TypeId: unique symbol;
export type TypeId = typeof TypeId;
export declare const ErrorTypeId: unique symbol;
export type ErrorTypeId = typeof ErrorTypeId;
```

Nominal brands tagging the service interface and its error.

### Clipboard (interface + tag)

```typescript
export interface Clipboard {
  readonly [TypeId]: TypeId;
  readonly read: Effect.Effect<ClipboardItems, ClipboardError>;
  readonly readString: Effect.Effect<string, ClipboardError>;
  readonly write: (items: ClipboardItems) => Effect.Effect<void, ClipboardError>;
  readonly writeString: (text: string) => Effect.Effect<void, ClipboardError>;
  readonly writeBlob: (blob: Blob) => Effect.Effect<void, ClipboardError>;
  readonly clear: Effect.Effect<void, ClipboardError>;
}

export declare const Clipboard: Context.Tag<Clipboard, Clipboard>;
```

The service interface (six clipboard operations, all failing with `ClipboardError`) and the
`Context.Tag` carrying it. `clear`, `writeBlob`, and `read`/`write` are derived; only the four
primitive operations are required by `make` (see below).

### ClipboardError

```typescript
export declare class ClipboardError extends ClipboardError_base<{
  readonly message: string;
  readonly cause: unknown;
}> {}
// ClipboardError_base: YieldableError & Record<ErrorTypeId, ErrorTypeId> & { readonly _tag: "ClipboardError" }
```

Tagged, yieldable error (`_tag: "ClipboardError"`) carrying a `message` and the underlying
`cause`. Yieldable means it can be `yield*`-ed directly in an `Effect.gen`.

### make

```typescript
export declare const make: (impl: Omit<Clipboard, "clear" | "writeBlob" | TypeId>) => Clipboard;
```

Constructor: supply only `read`, `readString`, `write`, `writeString`; `clear`, `writeBlob`, and
the `TypeId` brand are filled in. The seam for a custom or mock clipboard implementation.

### layer

```typescript
export declare const layer: Layer.Layer<Clipboard>;
```

The production layer wired directly to `navigator.clipboard`.

---

## [Geolocation]

A service capsule over `navigator.geolocation`.

### TypeId / ErrorTypeId

```typescript
// @effect/platform-browser/Geolocation
export declare const TypeId: unique symbol;
export type TypeId = typeof TypeId;
export declare const ErrorTypeId: unique symbol;
export type ErrorTypeId = typeof ErrorTypeId;
```

### Geolocation (interface + tag)

```typescript
export interface Geolocation {
  readonly [TypeId]: TypeId;
  readonly getCurrentPosition: (
    options?: PositionOptions | undefined,
  ) => Effect.Effect<GeolocationPosition, GeolocationError>;
  readonly watchPosition: (
    options?: (PositionOptions & { readonly bufferSize?: number | undefined }) | undefined,
  ) => Stream.Stream<GeolocationPosition, GeolocationError>;
}

export declare const Geolocation: Context.Tag<Geolocation, Geolocation>;
```

One-shot `getCurrentPosition` (an `Effect`) and continuous `watchPosition` (a `Stream` whose
`bufferSize` caps back-pressure). Both yield the DOM `GeolocationPosition` and fail with
`GeolocationError`.

### GeolocationError

```typescript
export declare class GeolocationError extends GeolocationError_base<{
  readonly reason: "PositionUnavailable" | "PermissionDenied" | "Timeout";
  readonly cause: unknown;
}> {
  get message(): "PositionUnavailable" | "PermissionDenied" | "Timeout";
}
// GeolocationError_base: YieldableError & Record<ErrorTypeId, ErrorTypeId> & { readonly _tag: "GeolocationError" }
```

Tagged yieldable error with a closed `reason` union mirroring the W3C `GeolocationPositionError`
codes; `message` getter returns the `reason`.

### layer

```typescript
export declare const layer: Layer.Layer<Geolocation>;
```

Production layer over `navigator.geolocation`.

### watchPosition (accessor)

```typescript
export declare const watchPosition: (
  options?: (PositionOptions & { readonly bufferSize?: number | undefined }) | undefined,
) => Stream.Stream<GeolocationPosition, GeolocationError, Geolocation>;
```

Module-level accessor for the interface's `watchPosition` — note the third type parameter
`Geolocation`: it requires the service from context rather than holding it. The
`Stream`-of-positions ingress for live location tracking.

---

## [Permissions]

A service capsule over `navigator.permissions`.

### TypeId / ErrorTypeId

```typescript
// @effect/platform-browser/Permissions
export declare const TypeId: unique symbol;
export type TypeId = typeof TypeId;
export declare const ErrorTypeId: unique symbol;
export type ErrorTypeId = typeof ErrorTypeId;
```

### Permissions (interface + tag)

```typescript
export interface Permissions {
  readonly [TypeId]: TypeId;
  readonly query: <Name extends PermissionName>(
    name: Name,
  ) => Effect.Effect<Omit<PermissionStatus, "name"> & { name: Name }, PermissionsError>;
}

export declare const Permissions: Context.Tag<Permissions, Permissions>;
```

Single operation `query`, generic over the DOM `PermissionName`, returning a `PermissionStatus`
narrowed so its `name` field carries the literal queried name. Fails with `PermissionsError`.

### PermissionsError

```typescript
export declare class PermissionsError extends PermissionsError_base<{
  readonly reason: "InvalidStateError" | "TypeError";
  readonly cause: unknown;
}> {
  get message(): "InvalidStateError" | "TypeError";
}
// PermissionsError_base: YieldableError & Record<ErrorTypeId, ErrorTypeId> & { readonly _tag: "PermissionsError" }
```

Tagged yieldable error; `reason` is the closed union of the two exceptions the Permissions API
`query` can throw; `message` getter returns the `reason`.

### layer

```typescript
export declare const layer: Layer.Layer<Permissions>;
```

Production layer over `navigator.permissions`.

---

## [ADMISSION]

Browser-tier only. Under the Nx `@nx/enforce-module-boundaries` bundle fence,
`@effect/platform-browser` is a `browser`/`neutral`-tagged dependency and never enters the
node bundle (the node tier uses `@effect/platform-node`). It satisfies the platform-neutral
`@effect/platform` service tags (`HttpClient`, `Socket`, `Socket.WebSocketConstructor`,
`Worker.WorkerManager` / `PlatformWorker` / `Spawner`, `WorkerRunner.PlatformRunner`,
`KeyValueStore`) with browser-DOM drivers, and adds three browser-only capability tags
(`Clipboard`, `Geolocation`, `Permissions`). Layers are consumed at `CompositionRoot`; no local
wrapper re-exports these — planning owners reference the namespace-qualified symbols directly.
