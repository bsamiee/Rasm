# [RUNTIME_WORKER]

Off-thread compute is one closed protocol and one pool — no wrapper, the platform surface at full depth. The message vocabulary is a `Schema.Union` of `Schema.TaggedRequest` classes — payload, success, and failure in one declaration, zero-copy crossings declared at the schema through the `Transferable` rows — and the pool executes decoded requests: `executeEffect` answers once, `execute` streams when the handler answers a `Stream`, `broadcast` fans to every member, the request's declared nature discriminating the modality so a parallel pool per shape is unspellable. The roster is real, never illustrative: `Render` is the CPU-bound document offload `work/report` dials, `Drop` is the memo-epoch invalidation fan, and a new off-thread capability is one class plus one union row plus one handler row. Sizing is one policy row — fixed with utilization-driven growth or elastic with idle TTL — and backpressure is structural: pool `concurrency` bounds in-flight requests per member, a saturated pool suspends callers on the platform's own queue, and no depth counter or shed flag exists beside it. Failure crosses as the request's failure schema and reconstructs as the same tagged class on the caller, so one fault vocabulary spans the thread boundary. This module owns the protocol, the pool, and the runner Layer; the worker ENTRY is its own boot module — `WorkerRunner.launch(RunnerLive)` run through the runtime row, exporting nothing. The module is `runtime/src/proc/worker.ts`.

## [01]-[CLUSTERS]

| [INDEX] | [CLUSTER]         | [OWNS]                                                                      | [PUBLIC]         |
| :-----: | :---------------- | :-------------------------------------------------------------------------- | :--------------- |
|  [01]   | `PROTOCOL_FAMILY` | the tagged-request union, transfer declarations, the cross-thread fault law | request classes  |
|  [02]   | `POOL_ROWS`       | sizing policy rows, the execution modalities, the backpressure law          | pool Tag + Layer |
|  [03]   | `RUNNER_BOOT`     | the handler record, the worker boot module, the spawn seam                  | runner Layer     |

## [02]-[PROTOCOL_FAMILY]

[PROTOCOL_FAMILY]:

- Owner: the closed protocol union — each request a `Schema.TaggedRequest` class carrying payload, success, and failure schemas in one declaration; the union value is the one artifact both sides compile against, so a request the pool sends that the runner cannot answer is a compile error at the handler record, never a runtime miss.
- Law: zero-copy crossings are declared at the schema — `Transferable.Uint8Array` for byte payloads, `Transferable.MessagePort` for channel handoff, `Transferable.schema(shape, project)` for a composite whose transfer list projects from its own fields — so the marshal plan is recoverable from the message declaration and no call site carries a transferable list.
- Law: failure crosses as the request's failure schema — the caller receives the same tagged class the handler failed with, its `class: FaultClass.Kind` field intact, so budget gates and routing dispatch on the reconstructed value; a stringified error crossing the seam destroys the discriminant and is the named defect.
- Law: request statics carry the call surface — each class owns its call static composing the pool Tag (`Render.rendered`, `Drop.announced`), so a consumer imports the request and reaches the whole seam; the stated union on the static is the marshal truth: domain fault, wire decode, worker transport.
- Law: `Render` is the CPU-bound document offload row — an encoded render plan crosses zero-copy, the produced bytes cross back zero-copy, and `report#SPEC_FOLD` dials `Render.rendered` above its off-thread ceiling; the plan codec is the report owner's, so this row carries octets and a `kind` discriminant, never a document shape.
- Law: `Drop` is the memo-epoch invalidation fan — the host broadcasts the rotated epoch and every member drops the worker-held memos it stamped under an older one, so a config or ruleset flip propagates to every thread without a per-member registry; the roster carries only rows with a named branch consumer, and the streaming modality (`execute` over a `Stream`-answering handler) is documented law whose first row lands with its consumer, never a placeholder class.
- Growth: a new off-thread capability is one request class plus one union row plus one handler row — every consumer stays untouched, the missing handler breaks loudly.
- Packages: `@effect/platform` (`Transferable`, `Worker`), `effect` (`Schema`, `Effect`), `@rasm/ts/core` (`FaultClass`).

```typescript signature
import { Transferable, Worker, type WorkerError, WorkerRunner } from '@effect/platform';
import { Context, Effect, Layer, type ParseResult, Schema } from 'effect';
import { FaultClass } from '@rasm/ts/core';

class BenchFault extends Schema.TaggedError<BenchFault>()('BenchFault', {
    reason: Schema.Literal('refused', 'starved'),
    class: FaultClass.schema,
}) {}

class Drop extends Schema.TaggedRequest<Drop>()('Drop', {
    payload: { epoch: Schema.Int },
    success: Schema.Void,
    failure: BenchFault,
}) {
    static readonly announced = (epoch: number): Effect.Effect<void, BenchFault | ParseResult.ParseError | WorkerError.WorkerError, Bench> =>
        Effect.asVoid(Effect.flatMap(Bench, (pool) => pool.broadcast(new Drop({ epoch }))));
}

class Render extends Schema.TaggedRequest<Render>()('Render', {
    payload: { kind: Schema.Literal('pdf', 'bundle'), plan: Transferable.Uint8Array },
    success: Transferable.Uint8Array,
    failure: BenchFault,
}) {
    static readonly rendered = (
        kind: 'pdf' | 'bundle',
        plan: Uint8Array,
    ): Effect.Effect<Uint8Array, BenchFault | ParseResult.ParseError | WorkerError.WorkerError, Bench> =>
        Effect.flatMap(Bench, (pool) => pool.executeEffect(new Render({ kind, plan })));
}

const _Protocol = Schema.Union(Drop, Render);
```

## [03]-[POOL_ROWS]

[POOL_ROWS]:

- Owner: the pool form — `Worker.makePoolSerializedLayer(Tag, sizing)` against one `Context.Tag` holding the `SerializedWorkerPool` of the union; execution modalities ride the pool surface (`executeEffect` one-shot, `execute` streaming, `broadcast` fan-out) and the request's declared nature discriminates, never a parallel pool.
- Law: sizing is the platform's parameterized policy union — fixed (`{ size, concurrency, targetUtilization }`) or elastic (`{ minSize, maxSize, timeToLive, concurrency, targetUtilization }`) arrives intact at `BenchLive(policy)` from the root, with `Setting.serve.extent` supplying the deployment-sized fixed row; no profile roster freezes two named configurations or hides the platform's full option surface.
- Law: backpressure is the platform's — `concurrency` bounds in-flight requests per member, a saturated pool suspends the caller on the pool's own queue, and cancellation is fiber interruption crossing the seam (an interrupted caller interrupts the in-flight worker request); a depth gauge, shed flag, or hand queue beside the pool re-derives what the option row states.
- Law: the pool Tag is the consumer surface — callers hold the Tag through request statics; the spawner and manager (`Worker.Spawner | Worker.WorkerManager`) ride the Layer's `R` tail to the boot edge, so the requirement annotation names the runtime rows only the root satisfies.
- Entry: `BenchLive(policy)` merged at the root; consumers compose `Render.rendered` / `Drop.announced`.
- Packages: `@effect/platform` (`Worker`), `effect` (`Context`, `Layer`).

```typescript signature
declare namespace Bench {
    type Protocol = Drop | Render;
    type Handlers = WorkerRunner.SerializedRunner.Handlers<Protocol>;
    type Policy = Worker.SerializedWorkerPool.Options<Protocol>;
}

class Bench extends Context.Tag('runtime/Bench')<Bench, Worker.SerializedWorkerPool<Bench.Protocol>>() {}

const BenchLive = (policy: Bench.Policy): Layer.Layer<Bench, WorkerError.WorkerError, Worker.Spawner | Worker.WorkerManager> =>
    Worker.makePoolSerializedLayer(Bench, policy);
```

## [04]-[RUNNER_BOOT]

[RUNNER_BOOT]:

- Owner: the worker side — `WorkerRunner.layerSerialized(protocol, handlers)` with the handler record compiler-checked against the union: an `Effect` handler answers once, a `Stream` handler streams, a `broadcast` request reaches every member's handler; the record is the whole worker program, and `RunnerLive(handlers)` is the one runner-Layer generator, so the protocol owner never substitutes annotation or identity bodies for the domain implementation.
- Law: the worker entry is its own boot module under the one-`main` law — a separate module runs `row.main(WorkerRunner.launch(RunnerLive(Report.worker)))` beneath the runtime row's `runner` binding (`exec#RUNTIME_ROWS`) and exports nothing; `Report.worker` owns the real render and invalidation handlers beside the renderer state they mutate, the protocol module owns their exhaustive type, and the spawn factory feeding `row.worker` is the only site naming the entry module's URL.
- Law: handler capability rides the runner Layer's own graph — a handler needing filesystem or telemetry composes those Layers beneath `RunnerLive` in the worker boot module, so the worker graph is a root proof exactly like the host graph and an unwired handler Tag fails at the worker's boot line.
- Boundary: browser worker spawn and the `BrowserWorker` binding are `browser/fetch#BINDING_ROWS`'s rows under `browser/boot`'s root; this page owns the protocol and the node/bun seam.
- Packages: `@effect/platform` (`WorkerRunner`), `effect` (`Effect`, `Layer`).

```typescript signature
const RunnerLive = <const Handlers extends Bench.Handlers>(
    handlers: Handlers,
): Layer.Layer<never, WorkerError.WorkerError, WorkerRunner.PlatformRunner | WorkerRunner.SerializedRunner.HandlersContext<Handlers>> =>
    WorkerRunner.layerSerialized(_Protocol, handlers);

// --- [EXPORTS] --------------------------------------------------------------------------

export { Bench, BenchFault, BenchLive, Drop, Render, RunnerLive };
```

The worker entry module — `runtime/src/proc/worker.main.ts`, the exports-nothing boot seam the spawn factory names:

```typescript signature
import { WorkerRunner } from '@effect/platform';
import { Effect, Layer } from 'effect';
import { Runtime } from './exec.ts';
import { RunnerLive } from './worker.ts';
import { Report } from '../work/report.ts';

Runtime.node.main(Effect.provide(WorkerRunner.launch(RunnerLive(Report.worker)), Layer.mergeAll(Runtime.node.runner, Runtime.node.context)));

// --- [EXPORTS] --------------------------------------------------------------------------

export {};
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
