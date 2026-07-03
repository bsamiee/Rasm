# [HOST_RUNTIME]

A runtime is a row, and a bun swap is a Layer selection in the app root, never a fork. This page owns the branch's one runtime vocabulary: a keyed table whose `node` and `bun` rows each carry the full binding surface a process needs — the `runMain` boot edge, the aggregate platform context, the HTTP client and server bindings, the worker pool and runner bindings, the filesystem key-value binding — every member satisfying the same abstract `@effect/platform` Tags, so every service in the branch types against the contract and only the boot module reads a row. The module lives on the `./server` subpath: browser resolution never reaches it, and the browser lane's boot is `browser/boot`'s law, not a row here. A new runtime is one table row; nothing else in the branch changes.

## [1]-[INDEX]

- [01]-[RUNTIME_ROWS]: the keyed `node | bun` binding table — one row owns every runtime-specific member.
- [02]-[ROOT_SELECT]: the selection law — one `runMain` per process, `Layer.launch` versus `ManagedRuntime`, shared acquisitions, the subpath fence.

## [2]-[RUNTIME_ROWS]

- Owner: `Runtime` — one bare `as const` row table keyed `node | bun`, companion types riding its merged hub; each row carries `main` (the `RunMain` boot edge — `NodeRuntime.runMain` / `BunRuntime.runMain`, one shared shape derived as `typeof NodeRuntime.runMain`), `context` (the aggregate binding: `NodeContext.layer` / `BunContext.layer` satisfying `CommandExecutor | FileSystem | Path | Terminal | WorkerManager` in one Layer), `client` (`NodeHttpClient.layerUndici` with connection pooling and HTTP/2 / `FetchHttpClient.layer` on bun), `serve` (a bind-parameterized `HttpServer` Layer over `node:http` `createServer` / `Bun.serve`), `worker` (the spawn-factory pool binding `NodeWorker.layer` / `BunWorker.layer`), `runner` (the worker-side `PlatformRunner`: `NodeWorkerRunner.layer` / `BunWorkerRunner.layer`), and `kv` (`NodeKeyValueStore.layerFileSystem` / `BunKeyValueStore.layerFileSystem`). The row is the only site that names a binding package; every consumer yields the abstract Tag.
- Law: the row guard closes the member set — `_Rows` proves every row carries the full `Core` complement, so a new runtime missing a member is a compile error at this declaration; row-specific extras (dispatcher tuning, serve options) stay precisely typed by inference because the guard constrains presence, never the binding's exact output union, and the table itself is the kind set — no parallel contract restates it.
- Law: cluster runners ride the same rows at the same altitude — `NodeClusterHttp.layer` / `NodeClusterSocket.layer` (with `layerDispatcherK8s` and the discovery-only `layerK8sHttpClient`) and `BunClusterHttp.layer({ transport, serialization, storage })` / `BunClusterSocket.layer` are selected off the binding tier at the app root beside the row; `work` types against the `MessageStorage`/`Sharding` Tags and never imports a binding, so runner transport is root data.
- Law: dispatcher tuning is row-interior — proxy posture, connection ceilings, and TLS pin through `NodeHttpClient.dispatcherLayer`/`makeDispatcher` beneath the node row's `client`; the policy the branch composes over any client is `../net/client.md`'s and never forks per runtime.
- Boundary: this module imports `node:http` for the serve row — the sanctioned FFI seam; a `node:*` or binding-package import anywhere else in the branch outside a row module is the defect the architecture audit catches.
- Entry: `Runtime.node` / `Runtime.bun` read by the boot module only.
- Packages: `@effect/platform-node`, `@effect/platform-bun`, `@effect/platform` (`FetchHttpClient`).

```typescript
import { FetchHttpClient } from "@effect/platform"
import {
  NodeContext, NodeHttpClient, NodeHttpServer, NodeKeyValueStore, NodeRuntime, NodeWorker, NodeWorkerRunner,
} from "@effect/platform-node"
import {
  BunContext, BunHttpServer, BunKeyValueStore, BunRuntime, BunWorker, BunWorkerRunner,
} from "@effect/platform-bun"
import { createServer } from "node:http"

const Runtime = {
  node: {
    main: NodeRuntime.runMain,
    context: NodeContext.layer,
    client: NodeHttpClient.layerUndici,
    serve: (bind: Runtime.Bind) => NodeHttpServer.layer(() => createServer(), bind),
    worker: NodeWorker.layer,
    runner: NodeWorkerRunner.layer,
    kv: (directory: string) => NodeKeyValueStore.layerFileSystem(directory),
  },
  bun: {
    main: BunRuntime.runMain,
    context: BunContext.layer,
    client: FetchHttpClient.layer,
    serve: (bind: Runtime.Bind) => BunHttpServer.layer(bind),
    worker: BunWorker.layer,
    runner: BunWorkerRunner.layer,
    kv: (directory: string) => BunKeyValueStore.layerFileSystem(directory),
  },
} as const

declare namespace Runtime {
  type Bind = { readonly port: number }
  type Kind = keyof typeof Runtime
  type Main = typeof NodeRuntime.runMain
  type Core = {
    readonly main: Main
    readonly context: unknown
    readonly client: unknown
    readonly serve: (bind: Bind) => unknown
    readonly worker: unknown
    readonly runner: unknown
    readonly kv: (directory: string) => unknown
  }
  type Row<K extends Kind = Kind> = (typeof Runtime)[K]
  type _Rows<T extends Record<Kind, Core> = typeof Runtime> = T
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Runtime }
```

## [3]-[ROOT_SELECT]

- Owner: the boot law the row feeds. Exactly one `main` call per process, in one boot module that exports nothing: a process whose whole life is the graph boots `row.main(Layer.launch(root))` — build, suspend, teardown as interruption, finalizers drained on `SIGINT`/`SIGTERM`; a graph carrying registered drain steps parks through `../life/cycle.md`'s `parked` entry instead of bare `Layer.launch` — the same one-`main` law with the drain fold owning the interrupt; a host that calls in repeatedly holds `ManagedRuntime.make(root)` and chains `dispose`; several runtimes in one process share acquisitions through one `Layer.makeMemoMap` handed to each `ManagedRuntime.make(root, memo)`. A worker entry is a boot module under the same law: `WorkerRunner.launch(protocolLayer)` run beneath `row.runner`.
- Law: the row is selected by the boot module and appears nowhere else — the ~30-line app `main.ts` merges its Layer families, provides `row.context` plus `row.client` once, and calls `row.main`; a second `runMain`, an `Effect.runPromise` heading a long-lived process, and a binding import inside a lib module are the named defects.
- Law: the fence is physical — this module ships on the `./server` exports subpath, so `runtime:browser` resolution cannot reach a row; the architecture suite audits the purity the exports map cannot express.
- Receipt: the root's stated annotation `Layer.Layer<Out>` plus the row's `main` pinning `R` to `never` are the boot proof — an unwired Tag fails at the boot line, at compile time.
- Packages: `effect` (`Layer.launch`, `Layer.makeMemoMap`, `ManagedRuntime`), `@effect/platform` (`WorkerRunner.launch`).

```typescript
import type { HttpClient } from "@effect/platform"
import { Effect, Layer, ManagedRuntime } from "effect"
import { Runtime } from "./runtime.ts"

declare const root: Layer.Layer<HttpClient.HttpClient>

Runtime.node.main(Layer.launch(Layer.mergeAll(root, Runtime.node.context)))

const _memo: Layer.MemoMap = Effect.runSync(Layer.makeMemoMap)
const _host: ManagedRuntime.ManagedRuntime<HttpClient.HttpClient, never> = ManagedRuntime.make(root, _memo)
const _halted = (): Promise<void> => _host.dispose()

// --- [EXPORTS] --------------------------------------------------------------------------

export {}
```
