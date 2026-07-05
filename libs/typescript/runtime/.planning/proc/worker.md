# [RUNTIME_WORKER]

Off-thread compute is one closed protocol and one pool — no wrapper, the platform surface at full depth. The message vocabulary is a `Schema.Union` of `Schema.TaggedRequest` classes — payload, success, and failure in one declaration, zero-copy crossings declared at the schema through the `Transferable` rows — and the pool executes decoded requests: `executeEffect` answers once, `execute` streams, `broadcast` fans to every member, the request's declared nature discriminating the modality so a parallel pool per shape is unspellable. Sizing is one policy row — fixed with utilization-driven growth or elastic with idle TTL — and backpressure is structural: pool `concurrency` bounds in-flight requests per member, a saturated pool suspends callers on the platform's own queue, and no depth counter or shed flag exists beside it. Failure crosses as the request's failure schema and reconstructs as the same tagged class on the caller, so one fault vocabulary spans the thread boundary. The worker side implements the identical protocol through a compiler-checked handler record and boots as a module that exports nothing. The module is `runtime/src/proc/worker.ts`.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]         | [OWNS]                                                                       | [PUBLIC]                |
| :-----: | :---------------- | :------------------------------------------------------------------------------ | :---------------------- |
|  [01]   | `PROTOCOL_FAMILY` | the tagged-request union, transfer declarations, the cross-thread fault law     | request classes         |
|  [02]   | `POOL_ROWS`       | sizing policy rows, the execution modalities, the backpressure law              | pool Tag + Layer        |
|  [03]   | `RUNNER_BOOT`     | the handler record, the worker boot module, the spawn seam                      | runner Layer            |

## [2]-[PROTOCOL_FAMILY]

[PROTOCOL_FAMILY]:
- Owner: the closed protocol union — each request a `Schema.TaggedRequest` class carrying payload, success, and failure schemas in one declaration; the union value is the one artifact both sides compile against, so a request the pool sends that the runner cannot answer is a compile error at the handler record, never a runtime miss.
- Law: zero-copy crossings are declared at the schema — `Transferable.Uint8Array` for byte payloads, `Transferable.MessagePort` for channel handoff, `Transferable.schema(shape, project)` for a composite whose transfer list projects from its own fields — so the marshal plan is recoverable from the message declaration and no call site carries a transferable list.
- Law: failure crosses as the request's failure schema — the caller receives the same tagged class the handler failed with, its `class: FaultClass.Kind` field intact, so budget gates and routing dispatch on the reconstructed value; a stringified error crossing the seam destroys the discriminant and is the named defect.
- Law: request statics carry the call surface — each class owns its `executed`/`streamed` static composing the pool Tag, so a consumer imports the request and reaches the whole seam; the stated union on the static is the marshal truth: domain fault, wire decode, worker transport.
- Growth: a new off-thread capability is one request class plus one union row plus one handler row — every consumer stays untouched, the missing handler breaks loudly.
- Packages: `@effect/platform` (`Transferable`, `Worker`), `effect` (`Schema`, `Effect`, `Stream`), `@rasm/ts/core` (`FaultClass`).

```typescript
import { Transferable, Worker, type WorkerError, WorkerRunner } from "@effect/platform"
import { Context, Effect, Layer, type ParseResult, Schema, Stream } from "effect"
import { FaultClass } from "@rasm/ts/core"

class PoolFault extends Schema.TaggedError<PoolFault>()("PoolFault", {
  reason: Schema.Literal("refused", "starved"),
  class: FaultClass.schema,
}) {}

class Grade extends Schema.TaggedRequest<Grade>()("Grade", {
  payload: { octets: Transferable.Uint8Array },
  success: Schema.Struct({ key: Schema.String, extent: Schema.Number }),
  failure: PoolFault,
}) {
  static readonly executed = (
    octets: Uint8Array,
  ): Effect.Effect<{ readonly key: string; readonly extent: number }, PoolFault | ParseResult.ParseError | WorkerError.WorkerError, Bench> =>
    Effect.flatMap(Bench, (pool) => pool.executeEffect(new Grade({ octets })))
}

class Sweep extends Schema.TaggedRequest<Sweep>()("Sweep", {
  payload: { keys: Schema.Array(Schema.String) },
  success: Schema.String,
  failure: PoolFault,
}) {
  static readonly streamed = (
    keys: ReadonlyArray<string>,
  ): Stream.Stream<string, PoolFault | ParseResult.ParseError | WorkerError.WorkerError, Bench> =>
    Stream.unwrap(Effect.map(Bench, (pool) => pool.execute(new Sweep({ keys }))))
}

class Drop extends Schema.TaggedRequest<Drop>()("Drop", {
  payload: { epoch: Schema.Int },
  success: Schema.Void,
  failure: PoolFault,
}) {
  static readonly announced = (
    epoch: number,
  ): Effect.Effect<void, PoolFault | ParseResult.ParseError | WorkerError.WorkerError, Bench> =>
    Effect.asVoid(Effect.flatMap(Bench, (pool) => pool.broadcast(new Drop({ epoch }))))
}

const _Protocol = Schema.Union(Grade, Sweep, Drop)
```

## [3]-[POOL_ROWS]

[POOL_ROWS]:
- Owner: the pool form — `Worker.makePoolSerializedLayer(Tag, sizing)` against one `Context.Tag` holding the `SerializedWorkerPool` of the union; execution modalities ride the pool surface (`executeEffect` one-shot, `execute` streaming, `broadcast` fan-out) and the request's declared nature discriminates, never a parallel pool.
- Law: sizing is one policy row — `fixed` (`{ size, concurrency, targetUtilization }`: a standing pool whose member count is capacity planning and whose `targetUtilization` sets the load fraction that spreads work toward idle members) or `elastic` (`{ minSize, maxSize, timeToLive, concurrency }`: growth to `maxSize` under demand, members retired after `timeToLive` idle) — chosen at the pool layer; a second pool per load profile restates what the row already carries.
- Law: backpressure is the platform's — `concurrency` bounds in-flight requests per member, a saturated pool suspends the caller on the pool's own queue, and cancellation is fiber interruption crossing the seam (an interrupted caller interrupts the in-flight worker request); a depth gauge, shed flag, or hand queue beside the pool re-derives what the option row states.
- Law: the pool Tag is the consumer surface — callers hold the Tag through request statics; the spawner and manager (`Worker.Spawner | Worker.WorkerManager`) ride the Layer's `R` tail to the boot edge, so the requirement annotation names the runtime rows only the root satisfies.
- Entry: `BenchLive` merged at the root; consumers compose `Grade.executed` / `Sweep.streamed` / `Drop.announced`.
- Packages: `@effect/platform` (`Worker`), `effect` (`Context`, `Layer`).

```typescript
class Bench extends Context.Tag("runtime/Bench")<Bench, Worker.SerializedWorkerPool<Grade | Sweep | Drop>>() {}

const _SIZING = {
  fixed: { size: 4, concurrency: 2, targetUtilization: 0.8 },
  elastic: { minSize: 1, maxSize: 8, timeToLive: "60 seconds", concurrency: 2 },
} as const

const BenchLive: Layer.Layer<Bench, WorkerError.WorkerError, Worker.Spawner | Worker.WorkerManager> =
  Worker.makePoolSerializedLayer(Bench, _SIZING.fixed)
```

## [4]-[RUNNER_BOOT]

[RUNNER_BOOT]:
- Owner: the worker side — `WorkerRunner.layerSerialized(protocol, handlers)` with the handler record compiler-checked against the union: an `Effect` handler answers once, a `Stream` handler streams, a `broadcast` request reaches every member's handler; the record is the whole worker program.
- Law: a worker entry is a boot module under the one-`main` law — `WorkerRunner.launch(RunnerLive)` run beneath the runtime row's `runner` binding (`exec#RUNTIME_ROWS`), exporting nothing; the spawn factory feeding `row.worker` is the only site naming a worker entry URL, composed at the app root.
- Law: handler capability rides the runner Layer's own graph — a handler needing filesystem or telemetry composes those Layers beneath `RunnerLive` in the worker boot module, so the worker graph is a root proof exactly like the host graph and an unwired handler Tag fails at the worker's boot line.
- Boundary: browser worker spawn and the `BrowserWorker` binding are `browser/fetch#BINDING_ROWS`'s rows under `browser/boot`'s root; this page owns the protocol and the node/bun seam.
- Packages: `@effect/platform` (`WorkerRunner`), `effect` (`Effect`, `Stream`, `Layer`).

```typescript
const RunnerLive: Layer.Layer<never, WorkerError.WorkerError, WorkerRunner.PlatformRunner> =
  WorkerRunner.layerSerialized(_Protocol, {
    Grade: ({ octets }) => Effect.succeed({ key: "<value-a>", extent: octets.byteLength }),
    Sweep: ({ keys }) => Stream.fromIterable(keys),
    Drop: () => Effect.void,
  })

// --- [EXPORTS] --------------------------------------------------------------------------

export { Bench, BenchLive, Drop, Grade, PoolFault, RunnerLive, Sweep }
```
