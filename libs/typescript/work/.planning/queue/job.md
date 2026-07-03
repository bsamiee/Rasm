# [WORK_JOB]

A durable job family is one `DurableQueue` declaration under one lane row: `Job.family` mints the queue from its payload schema and dedupe key, binds the lane's retry budget into every processing attempt, and returns the enqueue/replay surface with dead-lettering as typed evidence — so a new job kind is a family declaration, a new throughput class is a lane row, and no `pg-boss`-shaped framework, hand poll loop, or Promise queue ever exists beside the engine. Priority is lane selection, not a per-item field: `express`, `steady`, and `bulk` rows carry worker lanes and the kernel budget kind their processing retries under, and a family binds one row at declaration. Dedupe is the declared `idempotencyKey` projection — equal keys collapse to one durable item. Dead-lettering is the quarantine divert, not a dropped element: an item whose processing budget exhausts returns to its enqueuing workflow as a typed `Job.Dead` left carrying the payload and its fault classification, durable by the workflow's own persistence, and `replay` re-enqueues from that evidence. Per-key ordered processing is deliberately absent — that is the entity mailbox's law.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]    | [OWNS]                                                              |
| :-----: | :----------- | :--------------------------------------------------------------------- |
|  [01]   | [LANE_ROWS]  | the priority lane vocabulary — worker lanes, retry budget, drain law    |
|  [02]   | [JOB_FAMILY] | the family mint: queue, enqueue, dead evidence, replay                  |

## [2]-[LANE_ROWS]

[LANE_ROWS]:
- Owner: the lane rows riding the exported `Job` owner — `express` (interactive jobs: wide worker concurrency, the `pulse` budget), `steady` (the workhorse row: moderate lanes, the `lease` budget), `bulk` (batch work: narrow lanes, the `bulk` budget). Each row carries `lanes` (the worker's bounded concurrency) and `retry` (the kernel `Budget` kind compiled into the processing `retrySchedule`), so a family's whole throughput-and-resilience posture is one row reference.
- Law: priority is partition, not a sortable field — the engine's queue has no priority column, so contention classes separate by family-per-lane: an urgent family binds `express` and drains under more lanes; inventing an item-level priority would demand a scan the store never pays.
- Law: the worker registers on the package surface directly — `DurableQueue.worker(family.queue, handle, { concurrency: Job.steady.lanes })` — because a member that merely renamed `worker` would forward without adding law; the lane row is what this folder owns, and the drain reads it by name.
- Law: retry rows gate on classification — the compiled schedule re-drives only faults the kernel classifies transient (the folder fault convention's `class` column), so a malformed payload dead-letters immediately while a saturated provider re-drives under jittered backoff, with zero per-family predicates.
- Boundary: the budget rows and their compilation are `kernel`'s; which lane a family binds is the declaring page's policy choice.
- Growth: a new throughput class is one row; a new lane axis (a drain window, a weight) is one `Row` field.

## [3]-[JOB_FAMILY]

[JOB_FAMILY]:
- Owner: `Job.family` — the family mint. `family({ name, payload, key, lane, success, error })` declares the `DurableQueue` (the payload schema, the `idempotencyKey` from the `key` projection, the typed settlement schemas) and assembles the caller surface: `enqueue(payload)` runs `DurableQueue.process` under the lane's retry schedule and suspends the enqueuing workflow until a worker settles the item — success flows the item's typed value as `Either.right`, exhaustion diverts to the `Job.Dead` left carrying the payload, the family, the dedupe key, and the dominant fault classification; `replay(dead)` re-enqueues straight from dead evidence, so quarantine review is a fold over values, never a table surgery.
- Law: dedupe is the key projection — `key` maps the payload to its business identity, equal keys collapse to one durable item, and a batch of duplicate triggers costs one execution; a family whose key includes a timestamp has silently opted out of dedupe and that choice must read at the declaration.
- Law: the dead divert preserves evidence on the success channel — `enqueue` returns `Either<A, Dead<P>>`, so the enqueuing workflow's own durability persists the quarantine verdict, a saga arm compensates on it, and a report folds it; swallowing exhaustion into a default destroys the replay path, and letting it propagate bare strands the payload.
- Law: dead evidence is a value, not a fault — it travels the success channel by the quarantine law, carries `class` from the folder convention so dominance folds and budget gates read it structurally, and its payload stays fully typed for `replay`.
- Law: batch collapse is upstream vocabulary — N identical lookups inside one processing body ride the request-resolver window, and windowed multi-item drains belong to the entity mailbox form; the queue family stays item-shaped so its dedupe and replay semantics remain exact.
- Boundary: the persisted backing rides the engine's storage row (`engine/storage.md`); the workflow that enqueues and holds dead evidence is `flow/durable.md`'s; scheduled enqueueing is `queue/schedule.md`'s cron law.
- Entry: `const shipment = Job.family({ name, payload, key, lane: "steady", success, error })`; `yield* shipment.enqueue(value)` inside a workflow body; `DurableQueue.worker(shipment.queue, handle, { concurrency: Job.steady.lanes })` at the root.
- Growth: a new job kind is one `family` call; a new settlement shape is a field on `Dead`.
- Packages: `@effect/workflow` (`DurableQueue`, `WorkflowEngine`), `effect` (`Effect`, `Either`, `Schema`, `Types`), `@rasm/ts/kernel` (`Budget`, `FaultClass`).

```typescript
import { DurableQueue, type WorkflowEngine } from "@effect/workflow"
import { Budget, FaultClass } from "@rasm/ts/kernel"
import { Effect, Either, type Schema, type Types } from "effect"

const _kinds = ["express", "steady", "bulk"] as const
const _lanes = {
  express: { lanes: 16, retry: "pulse" },
  steady: { lanes: 8, retry: "lease" },
  bulk: { lanes: 2, retry: "bulk" },
} as const

declare namespace Job {
  type Kinds = typeof _kinds
  type Lane = keyof typeof _lanes
  type Row = { readonly lanes: number; readonly retry: Budget.Kind }
  type Dead<P> = {
    readonly family: string
    readonly key: string
    readonly payload: P
    readonly class: FaultClass.Kind
    readonly detail: string
  }
  type Spec<P extends Schema.Schema.Any, A, AI, E, EI> = {
    readonly name: string
    readonly payload: P
    readonly key: (payload: Schema.Schema.Type<P>) => string
    readonly lane: Lane
    readonly success: Schema.Schema<A, AI>
    readonly error: Schema.Schema<E, EI>
  }
  type Family<P extends Schema.Schema.Any, A, E> = {
    readonly queue: DurableQueue.DurableQueue<Schema.Schema.Type<P>, A, E>
    readonly enqueue: (
      payload: Schema.Schema.Type<P>,
    ) => Effect.Effect<Either.Either<A, Dead<Schema.Schema.Type<P>>>, never, WorkflowEngine.WorkflowEngine>
    readonly replay: (
      dead: Dead<Schema.Schema.Type<P>>,
    ) => Effect.Effect<Either.Either<A, Dead<Schema.Schema.Type<P>>>, never, WorkflowEngine.WorkflowEngine>
  }
  type Shape = Types.Simplify<
    typeof _lanes & {
      readonly kinds: Kinds
      readonly family: <P extends Schema.Schema.Any, A, AI, E, EI>(spec: Spec<P, A, AI, E, EI>) => Family<P, A, E>
    }
  >
  type _Rows<T extends Record<Lane, Row> = typeof _lanes> = T
  type _Keys<K extends keyof typeof _lanes = Lane> = K
}

const _family = <P extends Schema.Schema.Any, A, AI, E, EI>(
  spec: Job.Spec<P, A, AI, E, EI>,
): Job.Family<P, A, E> => {
  const queue = DurableQueue.make({
    name: spec.name,
    payload: spec.payload,
    idempotencyKey: spec.key,
    success: spec.success,
    error: spec.error,
  })
  const enqueue = (payload: Schema.Schema.Type<P>) =>
    DurableQueue.process(queue, payload, { retrySchedule: Budget.schedule(_lanes[spec.lane].retry) }).pipe(
      Effect.map(Either.right<A>),
      Effect.catchAll((fault) =>
        Effect.succeed(
          Either.left({
            family: spec.name,
            key: spec.key(payload),
            payload,
            class: FaultClass.of(fault),
            detail: String(fault),
          }),
        ),
      ),
    )
  return { queue, enqueue, replay: (dead) => enqueue(dead.payload) }
}

const Job: Job.Shape = { ..._lanes, kinds: _kinds, family: _family }

// --- [EXPORTS] --------------------------------------------------------------------------

export { Job }
```
