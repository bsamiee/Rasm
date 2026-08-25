# [RUNTIME_EXEC]

This process substrate: a runtime is a row, a bun swap is a Layer selection in the app root, and a child process is a declarative value. This keyed `node | bun` binding table carries the full surface a process needs — the `runMain` boot edge, the aggregate platform context, the HTTP client and server bindings, the worker pool and runner bindings, the leaderless cluster runner, the filesystem key-value binding — every member satisfying the same abstract `@effect/platform` Tags, so every service types against the contract and only the boot module reads a row. Subprocess execution is one `Proc.Spec` Schema class — command, arguments, environment, working directory, shell posture, stdin feed, pipeline stages, budget, exit demand, and a closed defaulted `capture` vocabulary — with one entry whose return follows the capture discriminant, and the scoped live-handle modality for interactive children. Signals are structural, never handled: process-level `SIGINT`/`SIGTERM` drain is the row's `runMain` fact, and a child's teardown is the executor's bracket — a budget expiry interrupts the fiber and the interrupt kills the child, so no kill call, signal listener, or orphan process is spellable. Measurement is the same posture one altitude up: a benchmark is a caller's effect handed to mitata's state-free sampling kernel, whose generated loop owns warmup, batching, convergence, and the rung ladder, and whose gc, heap, and hardware-counter bands accumulate inside that loop where no post-hoc fold can reach them — this owner declares the knobs, supplies the handles those bands need, and spells the absence when a host cannot answer. This module ships on the `./server` subpath — browser resolution never reaches a row. This module is `runtime/src/proc/exec.ts`.

## [01]-[INDEX]

- [02]-[RUNTIME_ROWS]: the keyed `node | bun` binding table — one row owns every runtime-specific member; `Runtime`.
- [03]-[ROOT_SELECT]: the boot law: one `main` per process, `Layer.launch` vs `ManagedRuntime`, the fence; `Runtime`.
- [04]-[COMMAND_SPEC]: the `Proc.Spec` record, capture-polymorphic entry, live handle, exit/budget faults; `Proc`, `ExecFault`.
- [05]-[MEASURED_RUN]: the benchmark owner routing a caller's effect through mitata's sampling engine into a claim; `Trial`.

## [02]-[RUNTIME_ROWS]

[RUNTIME_ROWS]:
- Owner: `Runtime` — one bare `as const` row table keyed `node | bun`, companion types riding its merged hub; each row carries `main` (the `RunMain` boot edge), `context` (the aggregate platform binding), `client`, `serve` (the listener plus the duplicate-preserving inbound-header capability), `worker`, `runner`, `cluster`, `socket` (`NodeSocket.layerWebSocketConstructor` / `BunSocket.layerWebSocketConstructor` satisfying the abstract `Socket.WebSocketConstructor` Tag), `nats` (the `@nats-io/transport-node` native TCP/TLS `connect` both server rows share — the broker engine consumes this binding as its dial, so the browser lane's `wsconnect` and the server lanes' TCP dial are one root selection, never an engine fork), and `kv`; the row is the only site that names a binding package, and every consumer yields the abstract Tag.
- Law: serve residency is row data, never a caller assumption — `Bind` is the tagged residency family (`tcp`, `unix`, `tls`), each row's `residency` column names what it admits and answers an ops read of the same, and its `serve` takes only those binds, so a secure listener asked of a row that cannot honour it fails at the call rather than inside a platform option record; both rows answer all three while `Core` holds the floor at the two, because a floor raised to what every present row happens to reach refuses the first row that binds less.
- Law: the node row serves TLS through `node:https` — `@types/node` merges an `interface Server extends http.Server<Request, Response>` onto the `class Server extends tls.Server`, so an `https.Server` IS the `Http.Server` parameter `NodeHttpServer.layer` demands and the secure listener needs no second binding; the constructor differs alone, so the row selects it on the bind's own tag and hands the same `Net.ListenOptions` projection to both.
- Law: SPIKE — the binding table stays `node | bun`, and the deterministic floor is that pair with `_Rows` proving the full `Core` complement on each. No published `@effect/platform-*` package completes a third row: `platform-browser` binds `main`, `client`, `worker`, `runner`, `socket`, and `kv` yet publishes no aggregate `context` and no cluster binding, `platform-node-shared` is the interior both server rows compose rather than a host, `platform-deno` ships on the next major's prerelease line alone, and no workerd or Cloudflare binding exists. This table therefore leaves a fetch host unrepresentable rather than half-modelled as a listener-free residency, because `Bind` names what a row LISTENS on and a host with no listener carries no bind to name; a third row admits when one package answers `context` and `cluster` beside the members browser already binds.
- Law: the row guard closes the member set at the contract's own Layer bounds — `_Rows` proves every row carries the full `Core` complement and that `context` provides the aggregate platform Tags, `client` an `HttpClient`, `serve` an `HttpServer` beside the `HttpPlatform` and `Etag.Generator` every asset route spends, `worker` a spawn-factory pool binding, `runner` a `PlatformRunner`, and `kv` a `KeyValueStore`, so a new runtime missing a member and a mis-wired binding are both compile errors at this declaration; the guard states each factory member at its common supertype (`worker`'s spawn parameter and `cluster`'s options are row-specific, so the guard proves presence and Layer shape) while row-specific extras (dispatcher tuning, serve options, cluster storage rows) stay precisely typed by inference because consumers index the table, never the guard, and the table itself is the kind set — no parallel contract restates it.
- Law: the cluster row is the same altitude as every binding — `NodeClusterSocket.layer` (with `layerDispatcherK8s` and the discovery-only `layerK8sHttpClient` beside it) and the `BunClusterSocket.layer` peer are selected at the app root through the row exactly like `serve`, with `NodeClusterHttp.layer` / `BunClusterHttp.layer` as the HTTP-transport alternates the root may pin instead — the frozen `@effect/cluster-node` family stays unadmitted; the work owners type against the `MessageStorage`/`Sharding` Tags and never import a binding, so runner transport is root data.
- Law: undici dispatcher tuning is row-interior — connection ceilings, proxy posture, and TLS pin through `NodeHttpClient.dispatcherLayer`/`dispatcherLayerGlobal`/`makeDispatcher` beneath the node row's `client`; the egress policy composed over any client is `net/client#LANE_ROWS`'s and never forks per runtime.
- Law: duplicate-preserving request headers are a runtime capability, not a claim over platform `Headers` — Node reads `IncomingMessage.headersDistinct` before `@effect/platform` joins repeated values, while Bun's Fetch `Headers` source has already erased field-line identity and its row refuses the capability. A Bun root can serve every ordinary route, but composing `serve/route#LAYER_ROUTES`'s strict webhook intake against that row fails each intake request closed until the binding publishes a raw-header member; splitting comma-joined values would corrupt legal single fields and is forbidden.
- Boundary: this module imports `node:http` and `node:https` for the serve row and `node:v8` for the trial's heap reader — the process substrate's sanctioned FFI seam; elsewhere a `node:*` or binding-package import is admitted only on a server-lane module whose composed package contract demands the host type (`Buffer` for the mail, broker, and archive engines), and one on a runtime-neutral or browser module is the defect the architecture audit catches.
- Entry: `Runtime.node` / `Runtime.bun`, read by the boot module only.
- Packages: `@effect/platform-node`, `@effect/platform-bun`, `@effect/platform` (`FetchHttpClient`), `@nats-io/transport-node` (`connect`).

```typescript signature
import { FetchHttpClient, HttpServerRequest } from '@effect/platform';
import type {
    CommandExecutor,
    Etag,
    FileSystem,
    HttpClient,
    HttpPlatform,
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
    NodeHttpServerRequest,
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
import { Context, Effect, Layer } from 'effect';
import { createServer as createHttpServer } from 'node:http';
import { createServer as createHttpsServer } from 'node:https';

const _RESIDENCIES = ['tcp', 'unix', 'tls'] as const;

declare namespace InboundHeaders {
    type Band = Readonly<Record<string, ReadonlyArray<string> | undefined>>;
    type Source = { readonly distinct: Effect.Effect<Band, InboundHeaderFault, HttpServerRequest.HttpServerRequest> };
}

class InboundHeaderFault extends Error {
    readonly _tag = 'InboundHeaderFault';
    constructor(readonly runtime: 'bun') {
        super(`${runtime} cannot preserve inbound header field-line identity`);
    }
}

class InboundHeaders extends Context.Tag('runtime/InboundHeaders')<InboundHeaders, InboundHeaders.Source>() {}

const _nodeHeaders = Layer.succeed(InboundHeaders, {
    distinct: Effect.map(HttpServerRequest.HttpServerRequest, (request) =>
        NodeHttpServerRequest.toIncomingMessage(request).headersDistinct),
});

const _bunHeaders = Layer.succeed(InboundHeaders, {
    distinct: Effect.fail(new InboundHeaderFault('bun')),
});

const _listen = (bind: Runtime.Bind): { readonly port?: number; readonly host?: string; readonly path?: string } =>
    bind.kind === 'unix' ? { path: bind.path } : { port: bind.port, host: bind.host };

const _served = (bind: Runtime.Bind): Bun.ServeOptions | Bun.TLSServeOptions | Bun.UnixServeOptions =>
    bind.kind === 'unix'
        ? { unix: bind.path }
        : bind.kind === 'tls'
          ? { port: bind.port, hostname: bind.host, tls: { cert: bind.cert, key: bind.key } }
          : { port: bind.port, hostname: bind.host };

const Runtime = {
    node: {
        main: NodeRuntime.runMain,
        context: NodeContext.layer,
        client: NodeHttpClient.layerUndici,
        residency: ['tcp', 'unix', 'tls'] as const,
        serve: (bind: Runtime.Bind) =>
            Layer.merge(
                NodeHttpServer.layer(
                    bind.kind === 'tls' ? () => createHttpsServer({ cert: bind.cert, key: bind.key }) : () => createHttpServer(),
                    _listen(bind),
                ),
                _nodeHeaders,
            ),
        worker: NodeWorker.layer,
        runner: NodeWorkerRunner.layer,
        cluster: NodeClusterSocket.layer,
        socket: NodeSocket.layerWebSocketConstructor,
        nats: connect,
        kv: (directory: string) => NodeKeyValueStore.layerFileSystem(directory),
    },
    bun: {
        main: BunRuntime.runMain,
        context: BunContext.layer,
        client: FetchHttpClient.layer,
        residency: ['tcp', 'unix', 'tls'] as const,
        serve: (bind: Runtime.Bind) => Layer.merge(BunHttpServer.layer(_served(bind)), _bunHeaders),
        worker: BunWorker.layer,
        runner: BunWorkerRunner.layer,
        cluster: BunClusterSocket.layer,
        socket: BunSocket.layerWebSocketConstructor,
        nats: connect,
        kv: (directory: string) => BunKeyValueStore.layerFileSystem(directory),
    },
} as const;

declare namespace Runtime {
    type Residency = (typeof _RESIDENCIES)[number];
    type Bind<K extends Residency = Residency> = Extract<
        | { readonly kind: 'tcp'; readonly port: number; readonly host?: string }
        | { readonly kind: 'unix'; readonly path: string }
        | { readonly kind: 'tls'; readonly port: number; readonly host?: string; readonly cert: string; readonly key: string },
        { readonly kind: K }
    >;
    type Kind = keyof typeof Runtime;
    type Main = typeof NodeRuntime.runMain;
    type Core = {
        readonly main: Main;
        readonly context: Layer.Layer<CommandExecutor.CommandExecutor | FileSystem.FileSystem | Path.Path | Terminal.Terminal | Worker.WorkerManager>;
        readonly client: Layer.Layer<HttpClient.HttpClient>;
        readonly residency: ReadonlyArray<Residency>;
        readonly serve: (bind: Bind<'tcp' | 'unix'>) => Layer.Layer<
            HttpServer.HttpServer | HttpPlatform.HttpPlatform | Etag.Generator | InboundHeaders,
            unknown
        >;
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
- Owner: `Proc` — spec-driven subprocess execution over one Schema authority. `Proc.Spec` is a `Schema.Class`: `command`, defaulted `args`, the closed `capture` modality vocabulary (`"receipt" | "text" | "lines" | "stream"`, defaulted `"receipt"` at the declaration so absence is unspellable in the interior), `Option`-admitted `env`, `cwd`, and `feed` (the closed stdin family folded through `Command.feed`/`Command.stdin`), defaulted `shell` (`Command.runInShell`), defaulted `stderr` (the capture posture folded through `Command.stderr`), defaulted `pipes` (pipeline stages folded through `Command.pipeTo`), `Option`-admitted `budget` and `demand` (the expected exit code); `Proc.run` is the one entry, its return following the spec's own `capture` discriminant — `"receipt"` yields the `Proc.Receipt` class (exit code and elapsed), `"text"` captured stdout, `"lines"` the `Command.lines` split, `"stream"` the live byte stream; `Proc.open` is the interactive modality — a scoped acquisition of the executor's live `Process` handle for a long-lived child a caller feeds and reads (the compute-host case), released by scope close as interruption; a `runText`/`runStream`/`spawn` sibling family is the deleted spelling.
- Law: the class is the admission seam and the constructor — raw spec material (an ops verb's arguments, a config-declared job) decodes once through `Schema.decodeUnknown(Proc.Spec)`, trusted interior construction rides `new Proc.Spec({ command })` running the same filters, and the executor consumes only admitted values: capture is a total literal read, absence is `Option`, and no execution arm re-validates or branches on `undefined`; `Proc.Receipt` is the same authority on the result side, so an ops surface encodes receipts through the derived wire twin with zero hand serialization.
- Law: the diagnostic is captured, never discarded — `_settled` takes the live `Process` handle instead of `Command.exitCode` and drains `child.stderr` CONCURRENTLY with the exit wait, because a child that fills its stderr pipe while the parent waits on exit deadlocks. `spec.stderr` is the two-arm posture over that: `capture` folds the drained text into the exit row's own diagnostic column, and `inherit` hands the stream to the parent's own stderr through `Command.stderr("inherit")` — the arm a long-lived child with unbounded diagnostic output takes so no buffer grows behind the wait.
- Law: `feed` is the platform's whole stdin vocabulary as a closed tagged family, never a text field — `Text` keeps the UTF-8 encode `Command.feed` owns, `Inherit` hands the child the parent's own stdin, and `Bytes` folds a live `Stream` through `Command.stdin` so a piped archive or a generated payload feeds a child the text arm cannot express; the stream arm declares `FromSelf` against the carrier's own `Stream.StreamTypeId`, so a process-bound value is admitted by nominal identity and never pretends to serialize.
- Law: teardown is interruption — the budget interrupt, a parent scope closing, and a race loss all release the child through the executor's own bracket; a hand `kill`, a PID ledger, and a signal listener beside the rail are rejected, and escalation policy (grace then hard) is the budget value itself.
- Law: `demand` rides the receipt modality only — text, lines, and stream captures are byte lanes whose consumer owns interpretation; `budget` rides the settled modalities only — receipt, text, and lines captures are bounded whole, while the live stream and the open handle outlive any spec deadline by nature. Receipt elapsed time derives from `Clock.currentTimeNanos`, so wall-clock adjustment cannot produce a negative or inflated process duration.
- Boundary: `CommandExecutor` arrives from the runtime row's `context`; stdio bridges (`NodeStream.stdin`, `NodeSink.stdout`) are row-tier members an ops verb composes at its own seam, never re-exported here.
- Entry: `Proc.run(spec)`; `Proc.open(spec)` under `Scope`; the executor requirement rides `R` to the root.

```typescript signature
import { Command, type CommandExecutor, type PlatformError } from '@effect/platform';
import { Array, Clock, Duration, Effect, Match, Option, Predicate, Schema, type Scope, Stream, pipe } from 'effect';
import { Fault } from '@rasm/core';

const _LEG = 'command';

const _exec = Fault.Class.family(['budget', 'exit'] as const, {
    budget: Fault.Class.row({
        class: 'expired',
        leg: _LEG,
        detail: Schema.Struct({ command: Schema.NonEmptyString, budget: Schema.Duration }),
        render: ({ budget, command }) => `${command} outlived its ${Duration.toMillis(budget)}ms budget`,
    }),
    exit: Fault.Class.row({
        class: 'invalid',
        leg: _LEG,
        detail: Schema.Struct({
            command: Schema.NonEmptyString,
            code: Schema.Int,
            demanded: Schema.Int,
            detail: Schema.String,
        }),
        render: ({ code, command, demanded, detail }) =>
            `${command} exited ${code} against a demanded ${demanded}${detail === '' ? '' : ` — ${detail}`}`,
    }),
});

class ExecFault extends Schema.TaggedError<ExecFault>()('ExecFault', {
    case: _exec.payload,
}) {
    get class(): Fault.Class.Kind {
        return _exec.classOf(this.case.reason);
    }
    override get message(): string {
        return _exec.render(this.case);
    }
}

class Spec extends Schema.Class<Spec>('Proc/Spec')({
    command: Schema.NonEmptyString,
    args: Schema.optionalWith(Schema.Array(Schema.String), { default: () => [] }),
    capture: Schema.optionalWith(Schema.Literal('receipt', 'text', 'lines', 'stream'), { default: () => 'receipt' as const }),
    env: Schema.optionalWith(Schema.Record({ key: Schema.String, value: Schema.String }), { as: 'Option' }),
    cwd: Schema.optionalWith(Schema.String, { as: 'Option' }),
    shell: Schema.optionalWith(Schema.Boolean, { default: () => false }),
    feed: Schema.optionalWith(
        Schema.Union(
            Schema.TaggedStruct('Text', { text: Schema.String }),
            Schema.TaggedStruct('Inherit', {}),
            Schema.TaggedStruct('Bytes', {
                stream: Schema.declare(
                    (input): input is Stream.Stream<Uint8Array, PlatformError.PlatformError> =>
                        Predicate.hasProperty(input, Stream.StreamTypeId),
                    { identifier: 'ProcStdin' },
                ),
            }),
        ),
        { as: 'Option' },
    ),
    stderr: Schema.optionalWith(Schema.Literal('capture', 'inherit'), { default: () => 'capture' as const }),
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
        (shaped) =>
            Option.match(spec.feed, {
                onNone: () => shaped,
                onSome: (input) =>
                    Match.valueTags(input, {
                        Bytes: ({ stream }) => shaped.pipe(Command.stdin(stream)),
                        Inherit: () => shaped.pipe(Command.stdin('inherit')),
                        Text: ({ text }) => shaped.pipe(Command.feed(text)),
                    }),
            }),
        (fed) => (spec.stderr === 'inherit' ? fed.pipe(Command.stderr('inherit')) : fed),
        (wired) => Array.reduce(spec.pipes, wired, (acc, [command, args]) => acc.pipe(Command.pipeTo(Command.make(command, ...args)))),
    );

const _budgeted =
    (spec: Spec) =>
    <A, E, R>(self: Effect.Effect<A, E, R>): Effect.Effect<A, E | ExecFault, R> =>
        Option.match(spec.budget, {
            onNone: () => self,
            onSome: (budget) =>
                Effect.timeoutFail(self, {
                    duration: budget,
                    onTimeout: () => new ExecFault({ case: { reason: 'budget', command: spec.command, budget } }),
                }),
        });

const _settled = (spec: Spec): Effect.Effect<Receipt, Proc.Faults, CommandExecutor.CommandExecutor> =>
    Effect.scoped(
        Effect.gen(function* () {
            const opened = yield* Clock.currentTimeNanos;
            const child = yield* Command.start(_staged(spec));
            const [code, detail] = yield* Effect.zip(
                child.exitCode,
                spec.stderr === 'inherit'
                    ? Effect.succeed('')
                    : Effect.map(Stream.mkString(Stream.decodeText(child.stderr)), (text) => text.trim()),
                { concurrent: true },
            );
            const closed = yield* Clock.currentTimeNanos;
            const refused = Option.filter(spec.demand, (demanded) => code !== demanded);
            return Option.isSome(refused)
                ? yield* new ExecFault({ case: { reason: 'exit', command: spec.command, code, demanded: refused.value, detail } })
                : new Receipt({ command: spec.command, code, elapsed: Duration.nanos(closed - opened) });
        }),
    ).pipe(_budgeted(spec));

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
- Owner: `Trial` — a caller's effect routed through mitata's state-free sampling kernel and folded to a claim by `Board.Bench.fromMitata`, the seam the core claim owner already holds. `Trial.Spec` is the engine's own knob set decoded once — the convergence triple (`minSamples`, `maxSamples`, `minCpuTime`), the warmup pair, the batching triple, the trim gate, `concurrency`, and the three band switches — every default read off the package's exported `k_*` anchors rather than restated as a literal here, so a package retune reaches this owner without an edit. Sample count, warmup spend, batch unroll, the eight-rung ladder, and the outlier trim are the engine's law; this owner declares the knobs, supplies the handles the enrichment bands need, and states the absence when a band cannot be measured.
- Law: the rung ladder is the engine's, never a second kernel — mitata sorts its own samples, trims two from each tail past `samplesThreshold`, and reads `min`/`max`/`avg`/`p25`/`p50`/`p75`/`p99`/`p999` off floor-indexed rank positions. A second kernel reading the same samples answers the same eight questions under a different definition while leaving `gc`, `heap`, and `counters` unfillable forever, because those three bands accumulate INSIDE the generated loop and no post-hoc fold over the sample array can recover them; `p95` and a standard deviation stay unmeasured on every mitata-minted claim, which is why the claim's rung record is partial rather than total.
- Law: band availability is a modality fact the engine fixes, not a caller option — the `fn` sampler accumulates `gc`, `heap`, and `counters` inside its generated loop, and the generator sampler inherits all three by delegating to it, while the `iter` sampler's result carries no band field under any knob. This owner therefore routes the zero-arity `fn` overload alone: a caller's effect is a closure over the runtime, so the iterator modality buys nothing here and would silently cost every enrichment band the claim declares.
- Law: each band states the handle it needs, so an unset handle reads as absence rather than as a measurement of zero. The heap band exists only where a byte reader was supplied — bare sampling defaults the reader to null and omits the key entirely — so `_HEAP` is the reader this owner hands the engine. The GC band exists only where `innerGc` is set AND the resolved collector is a real hook, because the engine's fallback collector is a gigabyte allocation whose timing is not a collection; that fallback still runs and still costs, so `gc` defaults off and a caller enabling it under a runtime without `--expose-gc` or bun buys the allocation and no band.
- Law: a present band is not a measured band — the heap accumulator counts only samples whose delta was non-negative and divides by that count, so a workload the collector interleaved returns `Infinity`/`-Infinity`/`NaN` in a band that is structurally present. `_honest` strips exactly that shape before the fold, because the claim's band value is declared finite and non-negative and a sentinel crossing it publishes a heap figure no sample took.
- Law: counter absence is spelled on the host, never in the band — hardware counters need an optional native addon, a supported platform, and process privilege the addon alone can refuse, and the answer is a property of the measuring HOST rather than of any one metric. `_plane` resolves that verdict and stamps it onto the claim's own host fingerprint, so a reader distinguishes "this host cannot count" from "this metric was not counted"; the band itself stays absent and each unresolved leaf drops its key, so no counter series ever carries a fabricated point.
- Law: the counter band's every leaf is an average and nothing else, so `benchBand` is constant across the whole block and `Convention.rasm.benchCounterKind` is the only axis separating the series — a counter point emitted without the leaf stamp collapses five independent measures onto one line. The leaf vocabulary is the claim owner's counter-path table, and the band key IS the axis value, so a leaf added there joins the series with no edit here.
- Law: the error channel survives the engine's Promise seam — the engine awaits a bare closure, so a failing effect would surface as a rejected promise and reach the caller as a defect with its typed fault erased. The sampled closure runs `Effect.exit`, holds the FIRST failure in the operation's own cell, and the trial re-raises it after the run, so a fault that entered the sample fold leaves it as the same typed value; the sunk exit also feeds the engine's dead-code barrier, without which an effect whose result no one reads is eliminated and the trial measures an empty loop.
- Law: the measured span includes the effect's own fiber boot, by construction and not by oversight — an effect's cost is inseparable from the runtime that discharges it, and the engine's noop baseline that would subtract the boot is minted by the fenced registration surface this owner may not reach. A trial therefore compares effects against effects, which is the only comparison its claims are ever asked to grade.
- Boundary: the substrate boundary is physical — this owner imports the state-free `mitata/src/lib.mjs` kernel, so the registration list and render surface that the root module carries are never loaded into a domain process; benchmark registration and report rendering stay in the bench lane under `tests/`, and the run context that names the CPU and the noop baseline is minted there alone. The tests tier owns corpus benchmarking, this owner mints in-product claims on live workloads, and claim board join and rendering are the ui viewer probe's.
- Receipt: the claim is the receipt and it widens in place — `Board.Bench.fromMitata` fills the rung ladder, the tick count, the raw samples, and the three enrichment bands off one `stats` record, and this owner supplies the mint fields the engine cannot know. `allocatedBytes` fills from the measured per-operation heap delta where the band survived the honesty strip; `warmups` stays absent because the engine spends warmup conditionally on its own threshold gate and reports no count, so the declared ceiling published there would be a figure no run took.
- Entry: `Trial.run(host, spec, body)`.
- Growth: a new measured case is one `Trial.Spec`; a new enrichment band is one handle on `_tuned` plus one row in the `_points` fact stream.
- Packages: `mitata` (`mitata/src/lib.mjs`), `effect` (`Metric`, `Exit`, `Runtime`), `@rasm/core` (`Board`, `Convention`), `node:v8` (`getHeapStatistics`).

```typescript signature
import { Board, Convention } from '@rasm/core';
import { Array, DateTime, Effect, Exit, Metric, Option, Record, Runtime as EffectRuntime, Schema } from 'effect';
import {
    do_not_optimize as MitataSink,
    k_batch_samples,
    k_batch_threshold,
    k_batch_unroll,
    k_concurrency,
    k_max_samples,
    k_min_cpu_time,
    k_min_samples,
    k_samples_threshold,
    k_warmup_samples,
    k_warmup_threshold,
    measure as MitataMeasure,
} from 'mitata/src/lib.mjs';
import { getHeapStatistics } from 'node:v8';

const _AGGREGATES = ['avg', 'max', 'min', 'total'] as const;
const _PLANES = ['available', 'declined', 'unsupported', 'denied', 'absent'] as const;

const _STAMP = 'bench.counter.plane';

const _GAUGES = {
    counter: Convention.mount(Convention.metric.benchCounter),
    gc: Convention.mount(Convention.metric.benchGc),
    heap: Convention.mount(Convention.metric.benchHeap),
    time: Convention.mount(Convention.metric.benchTime),
} as const;

const _HEAP = (): number => {
    const held = getHeapStatistics();
    return held.used_heap_size + (held.malloced_memory ?? 0);
};

class TrialSpec extends Schema.Class<TrialSpec>('Trial/Spec')({
    suite: Schema.NonEmptyString,
    label: Schema.NonEmptyString,
    minSamples: Schema.optionalWith(Schema.Int.pipe(Schema.positive()), { default: () => k_min_samples }),
    maxSamples: Schema.optionalWith(Schema.Int.pipe(Schema.positive()), { default: () => k_max_samples }),
    minCpuTime: Schema.optionalWith(Schema.Int.pipe(Schema.positive()), { default: () => k_min_cpu_time }),
    warmupSamples: Schema.optionalWith(Schema.Int.pipe(Schema.nonNegative()), { default: () => k_warmup_samples }),
    warmupThreshold: Schema.optionalWith(Schema.Int.pipe(Schema.positive()), { default: () => k_warmup_threshold }),
    batchSamples: Schema.optionalWith(Schema.Int.pipe(Schema.positive()), { default: () => k_batch_samples }),
    batchUnroll: Schema.optionalWith(Schema.Int.pipe(Schema.positive()), { default: () => k_batch_unroll }),
    batchThreshold: Schema.optionalWith(Schema.Int.pipe(Schema.positive()), { default: () => k_batch_threshold }),
    samplesThreshold: Schema.optionalWith(Schema.Int.pipe(Schema.positive()), { default: () => k_samples_threshold }),
    concurrency: Schema.optionalWith(Schema.Int.pipe(Schema.positive()), { default: () => k_concurrency }),
    gc: Schema.optionalWith(Schema.Boolean, { default: () => false }),
    heap: Schema.optionalWith(Schema.Boolean, { default: () => true }),
    counters: Schema.optionalWith(Schema.Boolean, { default: () => true }),
}) {}

declare namespace Trial {
    type Stats = Awaited<ReturnType<typeof MitataMeasure>>;
    type Knobs = NonNullable<Parameters<typeof MitataMeasure>[1]> & { readonly $counters?: unknown };
    type Plane = (typeof _PLANES)[number];
    type Counters = { readonly plane: Plane; readonly handle: Option.Option<unknown> };
    type Point = {
        readonly gauge: keyof typeof _GAUGES;
        readonly band: string;
        readonly leaf: Option.Option<string>;
        readonly value: number;
    };
}

const _plane = (spec: TrialSpec): Effect.Effect<Trial.Counters> =>
    !spec.counters
        ? Effect.succeed({ plane: 'declined' as const, handle: Option.none() })
        : !['darwin', 'linux'].includes(process.platform)
          ? Effect.succeed({ plane: 'unsupported' as const, handle: Option.none() })
          :
            process.platform === 'darwin' && process.getuid?.() !== 0
            ? Effect.succeed({ plane: 'denied' as const, handle: Option.none() })
            : Effect.match(Effect.tryPromise(() => import('@mitata/counters')), {
                  onFailure: (cause) => ({
                      plane: String(cause).includes('PermissionDenied') ? ('denied' as const) : ('absent' as const),
                      handle: Option.none(),
                  }),
                  onSuccess: (addon) => ({ plane: 'available' as const, handle: Option.some(addon) }),
              });

const _tuned = (spec: TrialSpec, counters: Trial.Counters): Trial.Knobs => ({
    concurrency: spec.concurrency,
    inner_gc: spec.gc,
    min_samples: spec.minSamples,
    max_samples: spec.maxSamples,
    min_cpu_time: spec.minCpuTime,
    batch_unroll: spec.batchUnroll,
    batch_samples: spec.batchSamples,
    warmup_samples: spec.warmupSamples,
    batch_threshold: spec.batchThreshold,
    warmup_threshold: spec.warmupThreshold,
    samples_threshold: spec.samplesThreshold,
    heap: spec.heap ? _HEAP : undefined,
    ...Option.match(counters.handle, { onNone: () => ({}), onSome: (handle) => ({ $counters: handle }) }),
});

const _honest = (stats: Trial.Stats): Trial.Stats =>
    stats.heap === undefined || Number.isFinite(stats.heap.avg + stats.heap.min + stats.heap.max)
        ? stats
        : { ...stats, heap: undefined };

const _sampled = <A, E, R>(
    spec: TrialSpec,
    body: Effect.Effect<A, E, R>,
    knobs: Trial.Knobs,
): Effect.Effect<readonly [Trial.Stats, Option.Option<Exit.Exit<never, E>>], never, R> =>
    Effect.gen(function* () {
        const invoke = EffectRuntime.runPromise(yield* Effect.runtime<R>());
        let held: Option.Option<Exit.Exit<never, E>> = Option.none();
        const stats = yield* Effect.promise(() =>
            MitataMeasure(async () => {
                const exit = await invoke(Effect.exit(body));
                if (Exit.isFailure(exit) && Option.isNone(held)) held = Option.some(exit);
                MitataSink(exit);
            }, knobs),
        );
        return [_honest(stats), held] as const;
    });

const _points = (metric: Board.Bench.Metric): ReadonlyArray<Trial.Point> => [
    ...Array.filterMap(Record.toEntries(metric.band.rungs), ([band, value]) =>
        value === undefined ? Option.none() : Option.some({ band, gauge: 'time' as const, leaf: Option.none(), value })),
    ...Array.flatMap(['gc', 'heap'] as const, (gauge) =>
        Option.match(metric.band[gauge], {
            onNone: () => [],
            onSome: (aggregate) => Array.map(_AGGREGATES, (band) => ({ band, gauge, leaf: Option.none(), value: aggregate[band] })),
        })),
    ...Option.match(metric.band.counters, {
        onNone: () => [],
        onSome: (counters) =>
            Array.map(Record.toEntries(counters), ([leaf, value]) => ({ band: 'avg', gauge: 'counter' as const, leaf: Option.some(leaf), value })),
    }),
];

const _emitted = (claim: Board.Claim): Effect.Effect<void> =>
    Effect.forEach(
        claim.metrics,
        (metric) =>
            Effect.forEach(
                _points(metric),
                (point) =>
                    Metric.set(
                        Record.reduce(
                            {
                                [Convention.rasm.benchBand]: point.band,
                                [Convention.rasm.benchLabel]: metric.label,
                                [Convention.rasm.benchSuite]: claim.suite,
                                ...Option.match(point.leaf, {
                                    onNone: () => ({}),
                                    onSome: (leaf) => ({ [Convention.rasm.benchCounterKind]: leaf }),
                                }),
                            },
                            _GAUGES[point.gauge],
                            (gauge, value, key) => Metric.tagged(gauge, key, value),
                        ),
                        point.value,
                    ),
                { discard: true },
            ),
        { discard: true },
    );

const _bracketed = <A, E, R>(
    host: typeof Board.Claim.Host.Type,
    spec: TrialSpec,
    body: Effect.Effect<A, E, R>,
): Effect.Effect<Board.Claim, E, R> =>
    Effect.gen(function* () {
        const counters = yield* _plane(spec);
        const [stats, held] = yield* _sampled(spec, body, _tuned(spec, counters));
        yield* Option.match(held, { onNone: () => Effect.void, onSome: (exit) => exit });
        const minted = yield* DateTime.now;
        const claim = Board.Bench.fromMitata(stats, {
            suite: spec.suite,
            label: spec.label,
            unit: 'ns',
            polarity: 'minimize',
            subject: { subject: 'probe' },
            host: new Board.Claim.Host({ ...host, stamps: { ...host.stamps, [_STAMP]: counters.plane } }),
            minted,
            warmups: Option.none(),
            allocatedBytes: Option.map(Option.fromNullable(stats.heap), (band) => BigInt(Math.round(band.avg))),
            operations: Option.none(),
        });
        return yield* Effect.as(_emitted(claim), claim);
    });

const Trial = { Spec: TrialSpec, run: _bracketed } as const;

// --- [EXPORTS] -------------------------------------------------------------------------

export { ExecFault, InboundHeaderFault, InboundHeaders, Proc, Runtime, Trial };
```

## [06]-[RESEARCH]

(none)
