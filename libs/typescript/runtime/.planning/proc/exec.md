# [RUNTIME_EXEC]

The process substrate: a runtime is a row, a bun swap is a Layer selection in the app root, and a child process is a declarative value. The keyed `node | bun` binding table carries the full surface a process needs — the `runMain` boot edge, the aggregate platform context, the HTTP client and server bindings, the worker pool and runner bindings, the filesystem key-value binding — every member satisfying the same abstract `@effect/platform` Tags, so every service types against the contract and only the boot module reads a row. Subprocess execution is one `Proc.Spec` record — command, arguments, environment, pipeline stages, budget, capture modality — with one entry whose return follows the capture shape, plus the scoped live-handle modality for interactive children. Signals are structural, never handled: process-level `SIGINT`/`SIGTERM` drain is the row's `runMain` fact, and a child's teardown is the executor's bracket — a budget expiry interrupts the fiber and the interrupt kills the child, so no kill call, signal listener, or orphan process is spellable. The module ships on the `./server` subpath — browser resolution never reaches a row. The module is `runtime/src/proc/exec.ts`.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]      | [OWNS]                                                                             | [PUBLIC]           |
| :-----: | :------------- | :----------------------------------------------------------------------------------- | :----------------- |
|  [01]   | `RUNTIME_ROWS` | the keyed `node \| bun` binding table — one row owns every runtime-specific member   | `Runtime`          |
|  [02]   | `ROOT_SELECT`  | the boot law: one `main` per process, `Layer.launch` vs `ManagedRuntime`, the fence  | `Runtime`          |
|  [03]   | `COMMAND_SPEC` | the `Proc.Spec` record, capture-polymorphic entry, live handle, exit/budget faults   | `Proc`, `ExecFault` |

## [2]-[RUNTIME_ROWS]

[RUNTIME_ROWS]:
- Owner: `Runtime` — one bare `as const` row table keyed `node | bun`, companion types riding its merged hub; each row carries `main` (the `RunMain` boot edge — `NodeRuntime.runMain` / `BunRuntime.runMain`, one shared shape derived as `typeof NodeRuntime.runMain`), `context` (the aggregate binding: `NodeContext.layer` / `BunContext.layer` satisfying `CommandExecutor | FileSystem | Path | Terminal | WorkerManager` in one Layer), `client` (`NodeHttpClient.layerUndici` with connection pooling / `FetchHttpClient.layer` on bun), `serve` (a bind-parameterized `HttpServer` Layer over `node:http` `createServer` / `Bun.serve`), `worker` (the spawn-factory pool binding `NodeWorker.layer` / `BunWorker.layer`), `runner` (the worker-side `PlatformRunner`: `NodeWorkerRunner.layer` / `BunWorkerRunner.layer`), and `kv` (`NodeKeyValueStore.layerFileSystem` / `BunKeyValueStore.layerFileSystem`); the row is the only site that names a binding package, and every consumer yields the abstract Tag.
- Law: the row guard closes the member set — `_Rows` proves every row carries the full `Core` complement, so a new runtime missing a member is a compile error at this declaration; row-specific extras (dispatcher tuning, serve options) stay precisely typed by inference because the guard constrains presence, never the binding's exact output union, and the table itself is the kind set — no parallel contract restates it.
- Law: cluster runners ride the same rows at the same altitude — `NodeClusterHttp.layer` / `NodeClusterSocket.layer` (with `layerDispatcherK8s` and the discovery-only `layerK8sHttpClient`) and the `BunClusterHttp.layer` / `BunClusterSocket.layer` peers are selected off the binding tier at the app root beside the row; the work owners type against the `MessageStorage`/`Sharding` Tags and never import a binding, so runner transport is root data.
- Law: undici dispatcher tuning is row-interior — connection ceilings, proxy posture, and TLS pin through `NodeHttpClient.dispatcherLayer`/`dispatcherLayerGlobal`/`makeDispatcher` beneath the node row's `client`; the egress policy composed over any client is `client#LANE_ROWS`'s and never forks per runtime.
- Boundary: this module imports `node:http` for the serve row — the sanctioned FFI seam; a `node:*` or binding-package import anywhere else in the branch outside a row module is the defect the architecture audit catches.
- Entry: `Runtime.node` / `Runtime.bun`, read by the boot module only.
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
```

## [3]-[ROOT_SELECT]

[ROOT_SELECT]:
- Owner: the boot law the row feeds — exactly one `main` call per process, in one boot module that exports nothing: a process whose whole life is the graph boots `row.main(Layer.launch(root))` — build, suspend, teardown as interruption, finalizers drained on `SIGINT`/`SIGTERM`; a graph carrying registered drain steps parks through `life#PHASE_SPINE`'s `parked` entry instead of bare `Layer.launch` — the same one-`main` law with the drain fold owning the interrupt; a host that calls in repeatedly holds `ManagedRuntime.make(root)` and chains `dispose`; several runtimes in one process share acquisitions through one `Layer.makeMemoMap` handed to each `ManagedRuntime.make(root, memo)`; a worker entry is a boot module under the same law: `WorkerRunner.launch(protocolLayer)` run beneath `row.runner` (`worker#RUNNER_BOOT`).
- Law: the row is selected by the boot module and appears nowhere else — the app `main.ts` merges its Layer families, provides `row.context` plus `row.client` once, and calls `row.main`; a second `runMain`, an `Effect.runPromise` heading a long-lived process, and a binding import inside a lib module are the named defects.
- Law: the fence is physical — this module ships on the `./server` exports subpath, so `runtime:browser` resolution cannot reach a row; the architecture suite audits the purity the exports map cannot express.
- Receipt: the root's stated annotation `Layer.Layer<Out>` plus the row's `main` pinning `R` to `never` are the boot proof — an unwired Tag fails at the boot line, at compile time.
- Packages: `effect` (`Layer`, `ManagedRuntime`), `@effect/platform` (`WorkerRunner.launch`).

```typescript
import type { HttpClient } from "@effect/platform"
import { Effect, Layer, ManagedRuntime } from "effect"

declare const root: Layer.Layer<HttpClient.HttpClient>

Runtime.node.main(Layer.launch(Layer.mergeAll(root, Runtime.node.context)))

const _memo: Layer.MemoMap = Effect.runSync(Layer.makeMemoMap)
const _host: ManagedRuntime.ManagedRuntime<HttpClient.HttpClient, never> = ManagedRuntime.make(root, _memo)
const _halted = (): Promise<void> => _host.dispose()
```

## [4]-[COMMAND_SPEC]

[COMMAND_SPEC]:
- Owner: `Proc` — spec-driven subprocess execution. `Proc.Spec` carries `command`, `args`, `env`, `feed` (pipeline stages folded through `Command.pipeTo`), `budget`, and `demand` (the expected exit code); `Proc.run` is the one entry, its capture modality discriminated by the spec's own `capture` field: absent yields the `Proc.Receipt` (exit code plus elapsed), `"text"` yields captured stdout, `"stream"` yields the live byte stream; `Proc.open` is the interactive modality — a scoped acquisition of the executor's live `Process` handle for a long-lived child a caller feeds and reads (the compute-host case), released by scope close as interruption; a `runText`/`runStream`/`spawn` sibling family is the deleted spelling.
- Law: the fault surface is two families sized by routing — the platform's own `PlatformError` carries spawn and I/O failure untouched (re-wrapping a tagged family is ceremony), and `ExecFault` mints exactly the two causes the platform cannot: `exit` (a settled code refusing `demand`, the code as evidence) and `budget` (the expiry `Effect.timeoutFail` mints); the fault carries `class: FaultClass.Kind` so the core budget gate re-drives it — `budget` folds to `expired` (retryable), `exit` to `invalid` (terminal).
- Law: teardown is interruption — the budget interrupt, a parent scope closing, and a race loss all release the child through the executor's own bracket; a hand `kill`, a PID ledger, and a signal listener beside the rail are rejected, and escalation policy (grace then hard) is the budget value itself.
- Law: `demand` rides the receipt modality only — text and stream captures are byte lanes whose consumer owns interpretation; `budget` rides the settled modalities only — receipt and text captures are bounded whole, while the live stream and the open handle outlive any spec deadline by nature, so their signatures carry no `ExecFault` and a pull deadline is the consumer's own pipeline fact, never a spec knob the live lanes silently ignore.
- Boundary: `CommandExecutor` arrives from the runtime row's `context`; stdio bridges (`NodeStream.stdin`, `NodeSink.stdout`) are row-tier members an ops verb composes at its own seam, never re-exported here.
- Entry: `Proc.run(spec)`; `Proc.open(spec)` under `Scope`; the executor requirement rides `R` to the root.
- Packages: `@effect/platform` (`Command`, `CommandExecutor`), `effect` (`Clock`, `Data`, `Duration`, `Effect`, `Option`, `Stream`), `@rasm/ts/core` (`FaultClass`).

```typescript
import { Command, type CommandExecutor, type PlatformError } from "@effect/platform"
import { Array, Clock, Data, Duration, Effect, Option, type Scope, type Stream, pipe } from "effect"
import type { FaultClass } from "@rasm/ts/core"

class ExecFault extends Data.TaggedError("ExecFault")<{
  readonly reason: "exit" | "budget"
  readonly command: string
  readonly code: Option.Option<number>
}> {
  get class(): FaultClass.Kind {
    return this.reason === "budget" ? "expired" : "invalid"
  }
}

declare namespace Proc {
  type Capture = "text" | "stream"
  type Spec = {
    readonly command: string
    readonly args?: ReadonlyArray<string>
    readonly env?: Record<string, string>
    readonly feed?: ReadonlyArray<readonly [command: string, args: ReadonlyArray<string>]>
    readonly budget?: Duration.DurationInput
    readonly demand?: number
  }
  type Receipt = { readonly command: string; readonly code: number; readonly elapsed: Duration.Duration }
  type Faults = ExecFault | PlatformError.PlatformError
}

const _staged = (spec: Proc.Spec): Command.Command =>
  pipe(
    Command.make(spec.command, ...(spec.args ?? [])),
    (head) => (spec.env === undefined ? head : head.pipe(Command.env(spec.env))),
    (fed) =>
      Array.reduce(spec.feed ?? [], fed, (acc, [command, args]) =>
        acc.pipe(Command.pipeTo(Command.make(command, ...args)))),
  )

const _budgeted = (spec: Proc.Spec) =>
  <A, E, R>(self: Effect.Effect<A, E, R>): Effect.Effect<A, E | ExecFault, R> =>
    spec.budget === undefined
      ? self
      : Effect.timeoutFail(self, {
          duration: spec.budget,
          onTimeout: () => new ExecFault({ reason: "budget", command: spec.command, code: Option.none() }),
        })

const _settled = (spec: Proc.Spec): Effect.Effect<Proc.Receipt, Proc.Faults, CommandExecutor.CommandExecutor> =>
  Effect.gen(function* () {
    const opened = yield* Clock.currentTimeMillis
    const code = yield* _staged(spec).pipe(Command.exitCode)
    const closed = yield* Clock.currentTimeMillis
    return spec.demand !== undefined && code !== spec.demand
      ? yield* new ExecFault({ reason: "exit", command: spec.command, code: Option.some(code) })
      : { command: spec.command, code, elapsed: Duration.millis(closed - opened) }
  }).pipe(_budgeted(spec))

function run(spec: Proc.Spec & { readonly capture: "text" }): Effect.Effect<string, Proc.Faults, CommandExecutor.CommandExecutor>
function run(spec: Proc.Spec & { readonly capture: "stream" }): Stream.Stream<Uint8Array, PlatformError.PlatformError, CommandExecutor.CommandExecutor>
function run(spec: Proc.Spec): Effect.Effect<Proc.Receipt, Proc.Faults, CommandExecutor.CommandExecutor>
function run(spec: Proc.Spec & { readonly capture?: Proc.Capture }) {
  return spec.capture === "stream"
    ? _staged(spec).pipe(Command.stream)
    : spec.capture === "text"
      ? _staged(spec).pipe(Command.string, _budgeted(spec))
      : _settled(spec)
}

const _opened = (
  spec: Proc.Spec,
): Effect.Effect<CommandExecutor.Process, PlatformError.PlatformError, CommandExecutor.CommandExecutor | Scope.Scope> =>
  _staged(spec).pipe(Command.start)

const Proc = { run, open: _opened } as const

// --- [EXPORTS] --------------------------------------------------------------------------

export { ExecFault, Proc, Runtime }
```
