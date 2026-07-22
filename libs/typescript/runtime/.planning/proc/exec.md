# [RUNTIME_EXEC]

This process substrate: a runtime is a row, a bun swap is a Layer selection in the app root, and a child process is a declarative value. This keyed `node | bun` binding table carries the full surface a process needs — the `runMain` boot edge, the aggregate platform context, the HTTP client and server bindings, the worker pool and runner bindings, the leaderless cluster runner, the filesystem key-value binding — every member satisfying the same abstract `@effect/platform` Tags, so every service types against the contract and only the boot module reads a row. Subprocess execution is one `Proc.Spec` Schema class — command, arguments, environment, working directory, shell posture, stdin feed, pipeline stages, budget, exit demand, and a closed defaulted `capture` vocabulary — with one entry whose return follows the capture discriminant, and the scoped live-handle modality for interactive children. Signals are structural, never handled: process-level `SIGINT`/`SIGTERM` drain is the row's `runMain` fact, and a child's teardown is the executor's bracket — a budget expiry interrupts the fiber and the interrupt kills the child, so no kill call, signal listener, or orphan process is spellable. This module ships on the `./server` subpath — browser resolution never reaches a row. This module is `runtime/src/proc/exec.ts`.

## [01]-[CLUSTERS]

| [INDEX] | [CLUSTER]      | [OWNS]                                                                              | [PUBLIC]            |
| :-----: | :------------- | :---------------------------------------------------------------------------------- | :------------------ |
|  [01]   | `RUNTIME_ROWS` | the keyed `node \| bun` binding table — one row owns every runtime-specific member  | `Runtime`           |
|  [02]   | `ROOT_SELECT`  | the boot law: one `main` per process, `Layer.launch` vs `ManagedRuntime`, the fence | `Runtime`           |
|  [03]   | `COMMAND_SPEC` | the `Proc.Spec` record, capture-polymorphic entry, live handle, exit/budget faults  | `Proc`, `ExecFault` |
|  [04]   | `MEASURED_RUN` | the benchmark owner minting claim-shaped receipts over one bracketed sample fold    | `Trial`             |

## [02]-[RUNTIME_ROWS]

[RUNTIME_ROWS]:
- Owner: `Runtime` — one bare `as const` row table keyed `node | bun`, companion types riding its merged hub; each row carries `main` (the `RunMain` boot edge), `context` (the aggregate platform binding), `client`, `serve`, `worker`, `runner`, `cluster`, `socket` (`NodeSocket.layerWebSocketConstructor` / `BunSocket.layerWebSocketConstructor` satisfying the abstract `Socket.WebSocketConstructor` Tag), `nats` (the `@nats-io/transport-node` native TCP/TLS `connect` both server rows share — the broker engine consumes this binding as its dial, so the browser lane's `wsconnect` and the server lanes' TCP dial are one root selection, never an engine fork), and `kv`; the row is the only site that names a binding package, and every consumer yields the abstract Tag.
- Law: the row guard closes the member set at the contract's own Layer bounds — `_Rows` proves every row carries the full `Core` complement and that `context` provides the aggregate platform Tags, `client` an `HttpClient`, `serve` an `HttpServer`, `worker` a spawn-factory pool binding, `runner` a `PlatformRunner`, and `kv` a `KeyValueStore`, so a new runtime missing a member and a mis-wired binding are both compile errors at this declaration; the guard states each factory member at its common supertype (`worker`'s spawn parameter and `cluster`'s options are row-specific, so the guard proves presence and Layer shape) while row-specific extras (dispatcher tuning, serve options, cluster storage rows) stay precisely typed by inference because consumers index the table, never the guard, and the table itself is the kind set — no parallel contract restates it.
- Law: the cluster row is the same altitude as every binding — `NodeClusterSocket.layer` (with `layerDispatcherK8s` and the discovery-only `layerK8sHttpClient` beside it) and the `BunClusterSocket.layer` peer are selected at the app root through the row exactly like `serve`, with `NodeClusterHttp.layer` / `BunClusterHttp.layer` as the HTTP-transport alternates the root may pin instead — the frozen `@effect/cluster-node` family stays unadmitted; the work owners type against the `MessageStorage`/`Sharding` Tags and never import a binding, so runner transport is root data.
- Law: undici dispatcher tuning is row-interior — connection ceilings, proxy posture, and TLS pin through `NodeHttpClient.dispatcherLayer`/`dispatcherLayerGlobal`/`makeDispatcher` beneath the node row's `client`; the egress policy composed over any client is `client#LANE_ROWS`'s and never forks per runtime.
- Boundary: this module imports `node:http` for the serve row — the sanctioned FFI seam; a `node:*` or binding-package import anywhere else in the branch outside a row module is the defect the architecture audit catches.
- Entry: `Runtime.node` / `Runtime.bun`, read by the boot module only.
- Packages: `@effect/platform-node`, `@effect/platform-bun`, `@effect/platform` (`FetchHttpClient`), `@nats-io/transport-node` (`connect`).

```typescript signature
import { FetchHttpClient } from '@effect/platform';
import type {
    CommandExecutor,
    FileSystem,
    HttpClient,
    HttpServer,
    KeyValueStore,
    Path,
    PlatformError,
    Socket,
    Terminal,
    Worker,
    WorkerRunner,
} from '@effect/platform';
import {
    NodeClusterSocket,
    NodeContext,
    NodeHttpClient,
    NodeHttpServer,
    NodeKeyValueStore,
    NodeRuntime,
    NodeSocket,
    NodeWorker,
    NodeWorkerRunner,
} from '@effect/platform-node';
import {
    BunClusterSocket,
    BunContext,
    BunHttpServer,
    BunKeyValueStore,
    BunRuntime,
    BunSocket,
    BunWorker,
    BunWorkerRunner,
} from '@effect/platform-bun';
import { connect, type NatsConnection, type NodeConnectionOptions } from '@nats-io/transport-node';
import type { Layer } from 'effect';
import { createServer } from 'node:http';

const Runtime = {
    node: {
        main: NodeRuntime.runMain,
        context: NodeContext.layer,
        client: NodeHttpClient.layerUndici,
        serve: (bind: Runtime.Bind) => NodeHttpServer.layer(() => createServer(), bind),
        worker: NodeWorker.layer,
        runner: NodeWorkerRunner.layer,
        cluster: NodeClusterSocket.layer,
        socket: NodeSocket.layerWebSocketConstructor,
        nats: connect, // native TCP/TLS dial: the broker engine consumes this row binding, never a package import of its own
        kv: (directory: string) => NodeKeyValueStore.layerFileSystem(directory),
    },
    bun: {
        main: BunRuntime.runMain,
        context: BunContext.layer,
        client: FetchHttpClient.layer,
        serve: (bind: Runtime.Bind) => BunHttpServer.layer(bind),
        worker: BunWorker.layer,
        runner: BunWorkerRunner.layer,
        cluster: BunClusterSocket.layer,
        socket: BunSocket.layerWebSocketConstructor,
        nats: connect, // transport-node's node-API dial serves the bun lane under its node compatibility surface
        kv: (directory: string) => BunKeyValueStore.layerFileSystem(directory),
    },
} as const;

declare namespace Runtime {
    type Bind = { readonly port: number };
    type Kind = keyof typeof Runtime;
    type Main = typeof NodeRuntime.runMain;
    type Core = {
        readonly main: Main;
        readonly context: Layer.Layer<CommandExecutor.CommandExecutor | FileSystem.FileSystem | Path.Path | Terminal.Terminal | Worker.WorkerManager>;
        readonly client: Layer.Layer<HttpClient.HttpClient>;
        readonly serve: (bind: Bind) => Layer.Layer<HttpServer.HttpServer, unknown>;
        readonly worker: (spawn: (id: number) => never) => Layer.Layer<Worker.WorkerManager | Worker.Spawner>;
        readonly runner: Layer.Layer<WorkerRunner.PlatformRunner>;
        readonly cluster: typeof NodeClusterSocket.layer | typeof BunClusterSocket.layer;
        readonly socket: Layer.Layer<Socket.WebSocketConstructor>;
        readonly nats: (opts?: NodeConnectionOptions) => Promise<NatsConnection>;
        readonly kv: (directory: string) => Layer.Layer<KeyValueStore.KeyValueStore, PlatformError.PlatformError>;
    };
    type Row<K extends Kind = Kind> = (typeof Runtime)[K];
    type _Rows<T extends Record<Kind, Core> = typeof Runtime> = T;
}
```

## [03]-[ROOT_SELECT]

[ROOT_SELECT]:
- Owner: the boot law the row feeds — exactly one `main` call per process, in one boot module that exports nothing: a process whose whole life is the graph boots `row.main(Layer.launch(root))` — build, suspend, teardown as interruption, finalizers drained on `SIGINT`/`SIGTERM`; a graph carrying registered drain steps parks through `life#PHASE_SPINE`'s `parked` entry instead of bare `Layer.launch` — the same one-`main` law with the drain fold owning the interrupt; a host that calls in repeatedly holds `ManagedRuntime.make(root)` and chains `dispose`; several runtimes in one process share acquisitions through one `Layer.makeMemoMap` handed to each `ManagedRuntime.make(root, memo)`; a worker entry is a boot module under the same law: `WorkerRunner.launch(protocolLayer)` run beneath `row.runner` (`worker#RUNNER_BOOT`).
- Law: the row is selected by the boot module and appears nowhere else — the app `main.ts` merges its Layer families, provides `row.context` and `row.client` once, and calls `row.main`; a second `runMain`, an `Effect.runPromise` heading a long-lived process, and a binding import inside a lib module are the named defects.
- Law: the fence is physical — this module ships on the `./server` exports subpath, so `runtime:browser` resolution cannot reach a row; the architecture suite audits the purity the exports map cannot express.
- Receipt: the root's stated annotation `Layer.Layer<Out>` and the row's `main` pinning `R` to `never` are the boot proof — an unwired Tag fails at the boot line, at compile time.
- Packages: `effect` (`Layer`, `ManagedRuntime`), `@effect/platform` (`WorkerRunner.launch`).

```typescript signature
import type { HttpClient } from '@effect/platform';
import { Effect, Layer, ManagedRuntime } from 'effect';

declare const root: Layer.Layer<HttpClient.HttpClient>;

Runtime.node.main(Layer.launch(Layer.mergeAll(root, Runtime.node.context)));

const _memo: Layer.MemoMap = Effect.runSync(Layer.makeMemoMap);
const _host: ManagedRuntime.ManagedRuntime<HttpClient.HttpClient, never> = ManagedRuntime.make(root, _memo);
const _halted = (): Promise<void> => _host.dispose();
```

## [04]-[COMMAND_SPEC]

[COMMAND_SPEC]:
- Owner: `Proc` — spec-driven subprocess execution over one Schema authority. `Proc.Spec` is a `Schema.Class`: `command`, defaulted `args`, the closed `capture` modality vocabulary (`"receipt" | "text" | "lines" | "stream"`, defaulted `"receipt"` at the declaration so absence is unspellable in the interior), `Option`-admitted `env`, `cwd`, and `feed` (stdin text folded through `Command.feed`), defaulted `shell` (`Command.runInShell`), defaulted `pipes` (pipeline stages folded through `Command.pipeTo`), `Option`-admitted `budget` and `demand` (the expected exit code); `Proc.run` is the one entry, its return following the spec's own `capture` discriminant — `"receipt"` yields the `Proc.Receipt` class (exit code and elapsed), `"text"` captured stdout, `"lines"` the `Command.lines` split, `"stream"` the live byte stream; `Proc.open` is the interactive modality — a scoped acquisition of the executor's live `Process` handle for a long-lived child a caller feeds and reads (the compute-host case), released by scope close as interruption; a `runText`/`runStream`/`spawn` sibling family is the deleted spelling.
- Law: the class is the admission seam and the constructor — raw spec material (an ops verb's arguments, a config-declared job) decodes once through `Schema.decodeUnknown(Proc.Spec)`, trusted interior construction rides `new Proc.Spec({ command })` running the same filters, and the executor consumes only admitted values: capture is a total literal read, absence is `Option`, and no execution arm re-validates or branches on `undefined`; `Proc.Receipt` is the same authority on the result side, so an ops surface encodes receipts through the derived wire twin with zero hand serialization.
- Law: the fault surface is two families sized by routing — the platform's own `PlatformError` carries spawn and I/O failure untouched (re-wrapping a tagged family is ceremony), and `ExecFault` mints exactly the two causes the platform cannot: `exit` (a settled code refusing `demand`, the code as evidence) and `budget` (the expiry `Effect.timeoutFail` mints); the fault carries `class: FaultClass.Kind` so the core budget gate re-drives it — `budget` folds to `expired` (retryable), `exit` to `invalid` (terminal).
- Law: teardown is interruption — the budget interrupt, a parent scope closing, and a race loss all release the child through the executor's own bracket; a hand `kill`, a PID ledger, and a signal listener beside the rail are rejected, and escalation policy (grace then hard) is the budget value itself.
- Law: `demand` rides the receipt modality only — text, lines, and stream captures are byte lanes whose consumer owns interpretation; `budget` rides the settled modalities only — receipt, text, and lines captures are bounded whole, while the live stream and the open handle outlive any spec deadline by nature. Receipt elapsed time derives from `Clock.currentTimeNanos`, so wall-clock adjustment cannot produce a negative or inflated process duration.
- Boundary: `CommandExecutor` arrives from the runtime row's `context`; stdio bridges (`NodeStream.stdin`, `NodeSink.stdout`) are row-tier members an ops verb composes at its own seam, never re-exported here.
- Entry: `Proc.run(spec)`; `Proc.open(spec)` under `Scope`; the executor requirement rides `R` to the root.
- Packages: `@effect/platform` (`Command`, `CommandExecutor`), `effect` (`Clock`, `Data`, `Duration`, `Effect`, `Option`, `Schema`, `Stream`), `@rasm/ts/core` (`FaultClass`).

```typescript signature
import { Command, type CommandExecutor, type PlatformError } from '@effect/platform';
import { Array, Clock, Data, DateTime, Duration, Effect, Option, Schema, type Scope, type Stream, pipe } from 'effect';
import { Claim, type FaultClass } from '@rasm/ts/core';

class ExecFault extends Data.TaggedError('ExecFault')<{
    readonly reason: 'exit' | 'budget';
    readonly command: string;
    readonly code: Option.Option<number>;
}> {
    get class(): FaultClass.Kind {
        return this.reason === 'budget' ? 'expired' : 'invalid';
    }
}

class Spec extends Schema.Class<Spec>('Proc/Spec')({
    command: Schema.NonEmptyString,
    args: Schema.optionalWith(Schema.Array(Schema.String), { default: () => [] }),
    capture: Schema.optionalWith(Schema.Literal('receipt', 'text', 'lines', 'stream'), { default: () => 'receipt' as const }),
    env: Schema.optionalWith(Schema.Record({ key: Schema.String, value: Schema.String }), { as: 'Option' }),
    cwd: Schema.optionalWith(Schema.String, { as: 'Option' }),
    shell: Schema.optionalWith(Schema.Boolean, { default: () => false }),
    feed: Schema.optionalWith(Schema.String, { as: 'Option' }),
    pipes: Schema.optionalWith(Schema.Array(Schema.Tuple(Schema.NonEmptyString, Schema.Array(Schema.String))), { default: () => [] }),
    budget: Schema.optionalWith(Schema.Duration, { as: 'Option' }),
    demand: Schema.optionalWith(Schema.Int, { as: 'Option' }),
}) {}

class Receipt extends Schema.Class<Receipt>('Proc/Receipt')({
    command: Schema.NonEmptyString,
    code: Schema.Int,
    elapsed: Schema.DurationFromMillis,
}) {}

declare namespace Proc {
    type Capture = Spec['capture'];
    type Faults = ExecFault | PlatformError.PlatformError;
}

const _staged = (spec: Spec): Command.Command =>
    pipe(
        Command.make(spec.command, ...spec.args),
        (head) => Option.match(spec.env, { onNone: () => head, onSome: (env) => head.pipe(Command.env(env)) }),
        (homed) => Option.match(spec.cwd, { onNone: () => homed, onSome: (cwd) => homed.pipe(Command.workingDirectory(cwd)) }),
        (placed) => (spec.shell ? placed.pipe(Command.runInShell(true)) : placed),
        (shaped) => Option.match(spec.feed, { onNone: () => shaped, onSome: (text) => shaped.pipe(Command.feed(text)) }),
        (fed) => Array.reduce(spec.pipes, fed, (acc, [command, args]) => acc.pipe(Command.pipeTo(Command.make(command, ...args)))),
    );

const _budgeted =
    (spec: Spec) =>
    <A, E, R>(self: Effect.Effect<A, E, R>): Effect.Effect<A, E | ExecFault, R> =>
        Option.match(spec.budget, {
            onNone: () => self,
            onSome: (budget) =>
                Effect.timeoutFail(self, {
                    duration: budget,
                    onTimeout: () => new ExecFault({ reason: 'budget', command: spec.command, code: Option.none() }),
                }),
        });

const _settled = (spec: Spec): Effect.Effect<Receipt, Proc.Faults, CommandExecutor.CommandExecutor> =>
    Effect.gen(function* () {
        const opened = yield* Clock.currentTimeNanos;
        const code = yield* _staged(spec).pipe(Command.exitCode);
        const closed = yield* Clock.currentTimeNanos;
        const refused = Option.filter(spec.demand, (demanded) => code !== demanded);
        return Option.isSome(refused)
            ? yield* new ExecFault({ reason: 'exit', command: spec.command, code: Option.some(code) })
            : new Receipt({ command: spec.command, code, elapsed: Duration.nanos(closed - opened) });
    }).pipe(_budgeted(spec));

function run(spec: Spec & { readonly capture: 'text' }): Effect.Effect<string, Proc.Faults, CommandExecutor.CommandExecutor>;
function run(spec: Spec & { readonly capture: 'lines' }): Effect.Effect<ReadonlyArray<string>, Proc.Faults, CommandExecutor.CommandExecutor>;
function run(spec: Spec & { readonly capture: 'stream' }): Stream.Stream<Uint8Array, PlatformError.PlatformError, CommandExecutor.CommandExecutor>;
function run(spec: Spec & { readonly capture: 'receipt' }): Effect.Effect<Receipt, Proc.Faults, CommandExecutor.CommandExecutor>;
function run(
    spec: Spec,
):
    | Effect.Effect<Receipt | string | ReadonlyArray<string>, Proc.Faults, CommandExecutor.CommandExecutor>
    | Stream.Stream<Uint8Array, PlatformError.PlatformError, CommandExecutor.CommandExecutor>;
function run(spec: Spec) {
    return spec.capture === 'stream'
        ? _staged(spec).pipe(Command.stream)
        : spec.capture === 'text'
          ? _staged(spec).pipe(Command.string, _budgeted(spec))
          : spec.capture === 'lines'
            ? pipe(Command.lines(_staged(spec)), _budgeted(spec))
            : _settled(spec);
}

const _opened = (spec: Spec): Effect.Effect<CommandExecutor.Process, PlatformError.PlatformError, CommandExecutor.CommandExecutor | Scope.Scope> =>
    _staged(spec).pipe(Command.start);

const Proc = { Spec, Receipt, run, open: _opened } as const;
```

## [05]-[MEASURED_RUN]

[MEASURED_RUN]:
- Owner: `Trial` — the in-product benchmark owner minting the core wire claim directly: `Trial.Spec` owns suite, label, modality, and positive warmup and iteration rows; `Trial.run(host, spec, body)` brackets the declared iterations at `Proc.Receipt`-grade timing (`Clock.currentTimeNanos` around every sample, so wall-clock adjustment cannot skew a duration) and folds the sample set through one quantile kernel into the imported `Claim` authority.
- Law: the claim shape has one owner — `Trial.Claim` is core `Claim`, not a runtime twin; its `Host`, `Band`, metric modality, raw samples, full percentile ladder, and mint instant therefore cross `BenchmarkClaimWire` without a projection or parallel schema. `Trial.Host` is required input from the selected runtime row, so Node and Bun report the core fingerprint fields and this owner reads no ambient process globals.
- Law: warmup is discarded by construction — the bracket runs `spec.warmup` unrecorded iterations before the first sample, so cold-path jit noise never enters a quantile; the quantile kernel sorts once and reads rank positions, a marked measured kernel.
- Law: bodies execute on the calling fiber and must be trusted non-blocking effects. Blocking kernels sit outside this API and enter through the worker plane's `Bench` protocol before a caller measures that round-trip as the body.
- Boundary: the tests tier owns corpus benchmarking — this owner mints in-product claims on live workloads, and the two never share a harness; claim board join and rendering are the ui viewer probe's.
- Entry: `Trial.run(host, spec, body)`.
- Growth: a new measured case is one `Trial.Spec`; optional GC, heap, and counter enrichments remain absent until `[06]-[RESEARCH]` settles the engine declarations that produce them.
- Packages: `effect` (`Clock`, `DateTime`, `Effect`, `Option`, `Schema`), `@rasm/ts/core` (`Claim`).

```typescript signature
class TrialSpec extends Schema.Class<TrialSpec>('Trial/Spec')({
    suite: Schema.NonEmptyString,
    label: Schema.NonEmptyString,
    kind: Schema.Literal('fn', 'iter', 'yield'),
    warmup: Schema.optionalWith(Schema.Int.pipe(Schema.positive()), { default: () => 16 }),
    iterations: Schema.optionalWith(Schema.Int.pipe(Schema.positive()), { default: () => 256 }),
}) {}

const _band = (samples: ReadonlyArray<number>): typeof Claim.Band.Type => {
    // BOUNDARY ADAPTER: measured quantile kernel — one sort, rank reads, the draft dies at the return
    const sorted = [...samples].sort((a, b) => a - b);
    const rank = (q: number): number => sorted[Math.max(0, Math.ceil(q * sorted.length) - 1)]!;
    return {
        ticks: sorted.length,
        samples,
        min: sorted[0]!,
        max: sorted[sorted.length - 1]!,
        avg: sorted.reduce((total, held) => total + held, 0) / sorted.length,
        p25: rank(0.25),
        p50: rank(0.5),
        p75: rank(0.75),
        p99: rank(0.99),
        p999: rank(0.999),
        gc: Option.none(),
        heap: Option.none(),
        counters: Option.none(),
    };
};

const _bracketed = <A, E, R>(
    host: typeof Claim.Host.Type,
    spec: TrialSpec,
    body: Effect.Effect<A, E, R>,
): Effect.Effect<Claim, E, R> =>
    Effect.gen(function* () {
        yield* Effect.forEach(Array.range(1, spec.warmup), () => body, { concurrency: 1, discard: true });
        const samples = yield* Effect.forEach(Array.range(1, spec.iterations), () =>
            Effect.gen(function* () {
                const opened = yield* Clock.currentTimeNanos;
                yield* body;
                const closed = yield* Clock.currentTimeNanos;
                return Number(closed - opened);
            }),
            { concurrency: 1 },
        );
        const minted = yield* DateTime.now;
        return new Claim({
            suite: spec.suite,
            metrics: [{ label: spec.label, unit: 'ns', kind: spec.kind, band: _band(samples) }],
            host,
            minted,
        });
    });

const Trial = { Claim, Host: Claim.Host, Spec: TrialSpec, run: _bracketed } as const;

// --- [EXPORTS] --------------------------------------------------------------------------

export { ExecFault, Proc, Runtime, Trial };
```

## [06]-[RESEARCH]

- [TRIAL_ENGINE]-[BLOCKED]: which exact `measure` and `do_not_optimize` declarations, result fields, batch controls, and GC controls support a typed deep-sampling modality without an `Unknown` evidence bag; route first through `libs/typescript/runtime/.api/mitata.md`, then `libs/typescript/.api/mitata.md`; arm only when either catalog carries exact rows for every composed member.
