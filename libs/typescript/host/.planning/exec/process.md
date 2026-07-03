# [HOST_PROCESS]

Subprocess and off-thread execution are declarative values: a child process is one `Proc.Spec` — command, arguments, environment, pipeline stages, budget, and capture modality in one record — executed by one entry whose return follows the capture shape; a worker pool is one `Schema.TaggedRequest` protocol union executed by a `SerializedWorkerPool` whose spawner and runner arrive from the runtime row. Signals are structural, never handled: the process-level `SIGINT`/`SIGTERM` drain is the row's `runMain` fact, and a child's teardown is the executor's bracket — a budget expiry interrupts the fiber and the interrupt kills the child, so no kill call, signal listener, or orphan process is spellable. `work` runner choreography and any ops verb execute over these surfaces; neither re-learns the platform.

## [1]-[INDEX]

- [01]-[COMMAND_SPEC]: the `Proc.Spec` record, the capture-polymorphic entry, the exit/budget fault family.
- [02]-[WORKER_POOL]: the serialized protocol form, sizing policy rows, and the worker boot law.

## [2]-[COMMAND_SPEC]

- Owner: `Proc` — spec-driven subprocess execution. `Proc.Spec` carries `command`, `args`, `env`, `feed` (pipeline stages folded through `Command.pipeTo`), `budget`, and `demand` (the expected exit code); `Proc.run` is the one entry, its capture modality discriminated by the spec's own `capture` field: absent yields the `Proc.Receipt` (exit code plus elapsed), `"text"` yields captured stdout, `"stream"` yields the live byte stream. Each shape's return is an overload signature; a `runText`/`runStream` sibling family is the deleted spelling.
- Law: the fault surface is two families, sized by routing — the platform's own `PlatformError` carries spawn and I/O failure untouched (re-wrapping a tagged family is ceremony), and `ExecFault` mints exactly the two causes the platform cannot: `exit` (a settled code refusing `demand`, the code as evidence) and `budget` (the expiry `Effect.timeoutFail` mints). `fault.retryable` projects the recovery posture — budget yes, exit no.
- Law: teardown is interruption — the budget interrupt, a parent scope closing, and a race loss all release the child through the executor's own bracket; a hand `kill`, a PID ledger, and a signal listener beside the rail are rejected. Escalation policy (grace then hard) is the budget value itself.
- Law: `demand` rides the receipt modality only — text and stream captures are byte lanes whose consumer owns interpretation; a gate over captured text marks a receipt call that should have been made.
- Boundary: `CommandExecutor` arrives from the runtime row's `context`; stdio bridges (`NodeStream.stdin`, `NodeSink.stdout`) are row-tier members an ops verb composes at its own seam, never re-exported here.
- Entry: `Proc.run(spec)`; the executor requirement rides `R` to the root.
- Packages: `@effect/platform` (`Command`, `CommandExecutor`), `effect` (`Clock`, `Data`, `Duration`, `Effect`, `Option`, `Stream`).

```typescript
import { Command, type CommandExecutor, type Error } from "@effect/platform"
import { Array, Clock, Data, Duration, Effect, Option, type Stream, pipe } from "effect"

class ExecFault extends Data.TaggedError("ExecFault")<{
  readonly reason: "exit" | "budget"
  readonly command: string
  readonly code: Option.Option<number>
}> {
  get retryable(): boolean {
    return this.reason === "budget"
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
  type Faults = ExecFault | Error.PlatformError
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
function run(spec: Proc.Spec & { readonly capture: "stream" }): Stream.Stream<Uint8Array, Error.PlatformError, CommandExecutor.CommandExecutor>
function run(spec: Proc.Spec): Effect.Effect<Proc.Receipt, Proc.Faults, CommandExecutor.CommandExecutor>
function run(spec: Proc.Spec & { readonly capture?: Proc.Capture }) {
  return spec.capture === "stream"
    ? _staged(spec).pipe(Command.stream)
    : spec.capture === "text"
      ? _staged(spec).pipe(Command.string, _budgeted(spec))
      : _settled(spec)
}

const Proc = { run } as const

// --- [EXPORTS] --------------------------------------------------------------------------

export { ExecFault, Proc }
```

## [3]-[WORKER_POOL]

- Owner: the pool form — no wrapper, the platform surface at full depth. The message vocabulary is a closed union of `Schema.TaggedRequest` classes (payload, success, failure in one declaration; zero-copy crossings declared with `Transferable` at the schema); the pool is `Worker.makePoolSerializedLayer(Tag, sizing)` executing decoded requests (`executeEffect` one-shot, `execute` streaming, `broadcast` fan-out — the request's declared nature discriminates, never a parallel pool); the worker side is `WorkerRunner.layerSerialized(protocol, handlers)` with the handler record compiler-checked against the union, launched by `WorkerRunner.launch` beneath the runtime row's `runner` — a worker entry is a boot module exporting nothing.
- Law: sizing is one policy row — fixed `{ size, concurrency, targetUtilization }` or elastic `{ minSize, maxSize, timeToLive }` — chosen at the pool layer; a second pool per load profile restates what the row carries, and the spawn factory feeding `row.worker` is the only site naming a worker entry URL.
- Law: failure crosses as the request's failure schema and reconstructs as the same tagged class on the caller — one fault vocabulary spans the thread boundary; a stringified error crossing the seam destroys the discriminant.
- Boundary: pool consumers hold the pool Tag; the spawner (`Runtime.node.worker(spawn)` / `Runtime.bun.worker(spawn)`) and runner binding are root rows — a lib module importing either is the fenced defect.
- Packages: `@effect/platform` (`Worker`, `WorkerRunner`, `Transferable`), `effect` (`Schema`, `Context`, `Layer`).

```typescript
import { Transferable, Worker, type WorkerError, WorkerRunner } from "@effect/platform"
import { Context, Effect, type Layer, Schema, Stream } from "effect"

class PoolFault extends Schema.TaggedError<PoolFault>()("PoolFault", {
  reason: Schema.Literal("refused", "starved"),
}) {}

class Grade extends Schema.TaggedRequest<Grade>()("Grade", {
  payload: { octets: Transferable.Uint8Array },
  success: Schema.Struct({ key: Schema.String, extent: Schema.Number }),
  failure: PoolFault,
}) {}

class Sweep extends Schema.TaggedRequest<Sweep>()("Sweep", {
  payload: { keys: Schema.Array(Schema.String) },
  success: Schema.String,
  failure: PoolFault,
}) {}

const _Protocol = Schema.Union(Grade, Sweep)

class Bench extends Context.Tag("host/Bench")<Bench, Worker.SerializedWorkerPool<Grade | Sweep>>() {}

const _SIZING = {
  fixed: { size: 4, concurrency: 2, targetUtilization: 0.8 },
  elastic: { minSize: 1, maxSize: 8, timeToLive: "60 seconds" },
} as const

const BenchLive: Layer.Layer<Bench, WorkerError.WorkerError, Worker.WorkerManager | Worker.Spawner> =
  Worker.makePoolSerializedLayer(Bench, _SIZING.fixed)

const RunnerLive: Layer.Layer<never, WorkerError.WorkerError, WorkerRunner.PlatformRunner> =
  WorkerRunner.layerSerialized(_Protocol, {
    Grade: ({ octets }) => Effect.succeed({ key: "<value-a>", extent: octets.byteLength }),
    Sweep: ({ keys }) => Stream.fromIterable(keys),
  })

// --- [EXPORTS] --------------------------------------------------------------------------

export {}
```
