# [API_CATALOGUE] effect-platform-browser

`@effect/platform-browser` supplies the browser-tier platform bindings for the Effect ecosystem: the runtime entrypoint (`runMain`), `HttpClient`/`Socket`/`Worker`/`KeyValueStore` layer drivers that satisfy platform-neutral service tags inside the browser bundle, DOM event-stream constructors, and three browser-API service capsules (`Clipboard`, `Geolocation`, `Permissions`). The package root re-exports ten namespace modules — `BrowserHttpClient`, `BrowserKeyValueStore`, `BrowserRuntime`, `BrowserSocket`, `BrowserStream`, `BrowserWorker`, `BrowserWorkerRunner`, `Clipboard`, `Geolocation`, `Permissions` — all symbols are accessed namespace-qualified (e.g. `BrowserRuntime.runMain`, `BrowserSocket.layerWebSocket`).

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@effect/platform-browser`
- package: `@effect/platform-browser`
- version: `0.76.0`
- license: `MIT`
- effect-peer: `effect ^3.21.x`, `@effect/platform ^0.96.x` — the abstract service Tags this package's `layer*` values satisfy (`.api/effect.md`, branch-tier `libs/typescript/.api/effect-platform.md`)
- catalog-verdict: KEEP
- runtime: `runtime:browser` — the edge ledger bans `@effect/platform-node`/`@effect/platform-bun`/`node:*` inside this scope and bans this package inside `runtime:node`; `proof/gauge` audits subpath purity
- modules: `BrowserRuntime`, `BrowserHttpClient`, `BrowserSocket`, `BrowserStream`, `BrowserKeyValueStore`, `BrowserWorker`, `BrowserWorkerRunner`, `Clipboard`, `Geolocation`, `Permissions` (10, namespace re-exported from `index.d.ts`)

[TIER_SPLIT]: this folder overlay vs the branch-tier catalog
- The branch-tier `libs/typescript/.api/effect-platform-browser.md` owns the branch-level stacking map: the `browser/boot/*` seam names, the `runtime:browser` purity ledger, and the EventLog / OpenTelemetry composition (`@effect/experimental`, `@effect/opentelemetry`).
- This `platform`-folder overlay carries the full per-member signatures the folder owners bind against AND the native-DOM ingresses this package does NOT wrap (`navigator.storage`, `Notification.requestPermission`, the `PermissionStatus.change` EventTarget) that `Shell/capability` composes alongside it. The two are complementary lenses on one package, not a copy.

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

### [BROWSER_SOCKET_MODALITY_LAW]

- `layerWebSocket(url, { closeCodeIsError? })` is the SINGLE-URL duplex `Socket.Socket` layer: a host owner binding one fixed socket endpoint provides this layer directly and reads/writes the duplex `Socket` (the inbound frame `Stream` and the outbound write share one connection). The `closeCodeIsError` predicate classifies which close codes surface as a `Socket` error versus a clean close.
- `layerWebSocketConstructor` is the DYNAMIC-URL form: it supplies the `Socket.WebSocketConstructor` tag, so a `TransportModality` owner that resolves its socket url from `RuntimeConfig` at construction time (or re-dials a fresh url per reconnect) composes the constructor and builds the `Socket` per-connection, rather than baking the url into a single `layerWebSocket`. The duplex `Socket` is the effect `Channel` bidirectional primitive — one connection carries both the decoded inbound frame stream and the outbound write half.
- The `effect` `Channel`/`Socket` duplex primitive is the bidirectional frame seam: the inbound `Socket` read side folds through `Schema.decodeUnknown` exactly as the SSE/wire ingress does, and the outbound write rides the same scoped resource — never a second ad-hoc `globalThis.WebSocket` constructed outside the owner.

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

### PermissionStatus.change (native EventTarget note)

```typescript
// DOM lib (not an @effect surface)
interface PermissionStatus extends EventTarget {
  readonly name: string;
  readonly state: "granted" | "denied" | "prompt";
  onchange: ((this: PermissionStatus, ev: Event) => unknown) | null;
}
```

The `PermissionStatus` returned by `Permissions.query` is a DOM `EventTarget` whose `change`
event fires when the host-side grant flips (a user revokes a previously granted permission, or a
prompt resolves). `PermissionStatus` is absent from `WindowEventMap`, so
`BrowserStream.fromEventListenerWindow` does not reach it; the `change` ingress rides the
`scoped-event-stream` `scopedEventStream` generic listener bridge over the `PermissionStatus`
target. The `state` field re-read after each `change` is the live grant value the
`BrowserCapability` per-kind cell folds. There is no `@effect` binding for this ingress.

---

## [02]-[NATIVE_DOM_INGRESS]

`navigator.storage` (the `StorageManager`) has no `@effect/platform-browser` surface — the two
calls below are confined to the `Shell/capability` owner.

### persist

```typescript
// DOM lib (not an @effect surface)
interface StorageManager {
  persist(): Promise<boolean>;
  persisted(): Promise<boolean>;
  estimate(): Promise<StorageEstimate>;
}
interface StorageEstimate {
  quota?: number;
  usage?: number;
  usageDetails?: Record<string, number>;
}
```

`navigator.storage.persist()` requests durable (eviction-resistant) storage for the origin and
resolves to whether durability was granted — the `persistent-storage` grant the
`BrowserCapability` persistent-storage ceremony folds and `LocalPersistence` reads before relying
on durable IndexedDB quota; `persisted()` reads the current durability without prompting;
`estimate()` resolves the `{ quota, usage }` figures the offline cache reads for headroom. All
three are `Promise`-returning native calls lifted through `Effect.tryPromise`. `Notification`
`requestPermission()` (`Promise<NotificationPermission>` where `NotificationPermission` is
`"granted" | "denied" | "default"`) is the sibling native grant ceremony for the `notifications`
kind, equally confined to that owner.

---

## [03]-[IMPLEMENTATION_LAW]

[INTEGRATION_LAW]:
- Tag-satisfaction, not reimplementation: folder code types against `@effect/platform`'s abstract `KeyValueStore`/`Worker`/`HttpClient`/`Socket` Tags; this package's `layer*` values satisfy them with browser-DOM drivers, so the same folder code runs on node/bun when the app root selects that binding instead. `BrowserRuntime.runMain` is the single browser boot (`Runtime/composition.md`), shared in shape with `Node`/`Bun` `runMain`.
- XHR vs fetch: `BrowserHttpClient.layerXMLHttpRequest` is the transport when upload/download progress or an `arraybuffer` response is required (the binary-frame leg, forced via `withXHRArrayBuffer`); otherwise `@effect/platform`'s `FetchHttpClient.layer` is the default `HttpClient`. Both satisfy the same `HttpClient.HttpClient` Tag — the choice is a Layer selection, and the XHR client is also the transport the browser OTLP export rides.
- Stack with `@effect/experimental` EventLog (branch-tier catalog): `BrowserKeyValueStore.layerLocalStorage` satisfies the `KeyValueStore` the EventLog identity store requires, and `BrowserSocket.layerWebSocketConstructor` satisfies the `Socket.WebSocketConstructor` the EventLog remote WS sync requires — the browser EventLog client is those Layers merged.
- Stack with `interchange/codec` (`transport/socket` seam): the `BrowserSocket` duplex `Socket` inbound read side folds through `Schema.decodeUnknown` exactly as the SSE/wire ingress does; the `BrowserWorker.layer(spawn)` decode pool serializes its `TaggedRequest` protocol through the same codec. Never a second ad-hoc `globalThis.WebSocket`/`Worker` outside the owner.

[LOCAL_ADMISSION]:
- `@effect/platform-browser` satisfies `HttpClient`, `Socket`, `Socket.WebSocketConstructor`, `Worker.WorkerManager`, `PlatformWorker`, `Spawner`, `WorkerRunner.PlatformRunner`, and `KeyValueStore` with browser-DOM drivers, and adds three browser-only tags: `Clipboard`, `Geolocation`, `Permissions`.
- Layers are consumed at `Runtime/composition` `CompositionRoot`; no local wrapper re-exports these — planning owners reference namespace-qualified symbols directly, imported only inside `runtime:browser` subpaths.
- The `Clipboard`/`Geolocation`/`Permissions` capsules and the native `navigator.storage.persist`/`estimate`/`Notification.requestPermission` grant ceremonies compose only inside `Shell/capability` behind the one `CapabilityKind` axis; a native `navigator.notification`/`navigator.clipboard`/`navigator.storage`/`navigator.permissions` call at any other owner is the named ungated-native-call defect.

[RAIL_LAW]:
- Package: `@effect/platform-browser`
- Owns: browser-tier layer implementations for platform-neutral `@effect/platform` service tags; the `BrowserRuntime.runMain` boot; browser-only capability services (`Clipboard`, `Geolocation`, `Permissions`)
- Accept: `layer*` values satisfying abstract `@effect/platform` Tags at `CompositionRoot`; `BrowserSocket`/`BrowserKeyValueStore` as EventLog-client backings; the XHR client for OTLP/binary transport; browser-DOM APIs (`XMLHttpRequest`, `WebSocket`, `localStorage`, `navigator.*`, Web Worker globals)
- Reject: a second `runMain`; node-bundle inclusion (`browser`/`neutral`-tagged; node tier uses `@effect/platform-node`); this package inside `runtime:node`; `ui` importing it directly instead of through a declared port; hand-rolled Web-Storage/WebSocket/Worker wrappers
