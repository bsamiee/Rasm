# [CORE_MACHINE]

The state-machine owner: a closed transition system is a vocabulary table — one `Transition.Table` keyed by phase whose rows carry `(next, emit)` per signal, with the mapped contract demanding the full matrix so a new phase or signal is a row, never a branch — and the same table drives three altitudes from one declaration: the pure Mealy `step`, the `drive` fold over a signal batch, and the serializable actor booted from the `@effect/experimental` `Machine` — one state, tagged requests serialized on one fiber, phase-keyed watchdog timers as policy rows, defect recovery as a `Schedule` value, and `snapshot`/`restore` carrying the machine across process restarts. THE ALTITUDE RULING: `Machine` is the in-process serializable actor — live state, request-serialized mutation, snapshot-grade durability; a machine whose steps demand durable-execution replay, activity memoization, compensation, or cross-process sharding is the runtime branch's workflow altitude, and promoting a transition system there re-homes the table, never re-shapes it — the two altitudes share this vocabulary and nothing else. The module is `core/src/state/machine.ts`; a new machine is a spec value, a new deadline is a watch row, a new request beyond feed/poll is a workflow-altitude signal.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]           | [OWNS]                                                             | [PUBLIC]                                     |
| :-----: | :------------------ | :---------------------------------------------------------------------- | :----------------------------------------------- |
|  [01]   | `TRANSITION_TABLE`  | the phase/signal/verdict vocabulary contract and the watch policy rows | `Transition.Table`, `Transition.Watch`, `Transition.Spec` |
|  [02]   | `STEP_DRIVE`        | the pure Mealy step and the batch drive fold                            | `Transition.step`, `Transition.drive`             |
|  [03]   | `ACTOR`             | the serializable actor: boot, feed, poll, freeze, restore, recovery     | `Transition.boot`, `Transition.restore`           |

## [2]-[TRANSITION_TABLE]

[TRANSITION_TABLE]:
- Owner: `Transition.Spec<P, S, V>` — the machine as one value: `name` (the actor identity telemetry keys on), the three literal schemas (`phase`, `signal`, `verdict` — the same anchors that decode a wire-carried phase and encode the snapshot), `rows` (the transition matrix), `watch` (the phase-keyed deadline rows), `recover` (the defect re-initialization `Schedule`).
- Law: the table is the mapped contract `{ [Phase in P]: { [Signal in S]: Row<P, V> } }` — every phase covers every signal with a typed row, so a missing arm is a compile error at the spec value and dispatch is table lookup, never a `switch` advancing a mutable phase.
- Law: `watch` rows are deadline policy as data — a phase row arms a delayed self-signal (`after`, `signal`) when entered, and a phase without a row disarms the incumbent — so timeout transitions are vocabulary, recoverable from the spec, never a timer fiber hand-managed beside the machine.
- Law: the schemas and the rows anchor one vocabulary — the literal tuples that feed `Schema.Literal` derive the phase/signal/verdict unions the table is typed over, so the wire arm, the type plane, and the matrix cannot drift.
- Growth: a new phase or signal is one row the mapped contract demands everywhere at once; a new deadline is one watch row; a new machine is one spec value binding existing vocabulary.
- Packages: `@effect/experimental` (`Machine`); `effect` (`Array`, `Duration`, `Effect`, `Option`, `ParseResult`, `Schedule`, `Schema`, `Scope`).

```typescript
import { Machine } from "@effect/experimental"
import { Array, type Duration, Effect, Option, type ParseResult, pipe, type Schedule, Schema, type Scope } from "effect"

declare namespace Transition {
  type Row<P extends string, V extends string> = { readonly next: P; readonly emit: V }
  type Table<P extends string, S extends string, V extends string> = {
    readonly [Phase in P]: { readonly [Signal in S]: Row<P, V> }
  }
  type Watch<P extends string, S extends string> = {
    readonly [Phase in P]?: { readonly after: Duration.DurationInput; readonly signal: S }
  }
  type Spec<P extends string, S extends string, V extends string> = {
    readonly name: string
    readonly phase: Schema.Schema<P, P>
    readonly signal: Schema.Schema<S, S>
    readonly verdict: Schema.Schema<V, V>
    readonly rows: Table<P, S, V>
    readonly watch: Watch<P, S>
    readonly recover: Schedule.Schedule<unknown>
  }
  type Step<P extends string, S extends string, V extends string> = (phase: P, signal: S) => readonly [P, V]
  type Frozen = readonly [unknown, unknown]
  type Actor<P extends string, S extends string, V extends string> = {
    readonly feed: (signal: S) => Effect.Effect<V>
    readonly phase: Effect.Effect<P>
    readonly freeze: Effect.Effect<Frozen, ParseResult.ParseError>
  }
  type Shape = {
    readonly spec: <P extends string, S extends string, V extends string>(spec: Spec<P, S, V>) => Spec<P, S, V>
    readonly step: <P extends string, S extends string, V extends string>(rows: Table<P, S, V>) => Step<P, S, V>
    readonly drive: <P extends string, S extends string, V extends string>(
      rows: Table<P, S, V>,
    ) => (origin: P, signals: ReadonlyArray<S>) => [P, ReadonlyArray<V>]
    readonly boot: <P extends string, S extends string, V extends string>(
      spec: Spec<P, S, V>,
      origin: P,
    ) => Effect.Effect<Actor<P, S, V>, ParseResult.ParseError, Scope.Scope>
    readonly restore: <P extends string, S extends string, V extends string>(
      spec: Spec<P, S, V>,
      frozen: Frozen,
    ) => Effect.Effect<Actor<P, S, V>, ParseResult.ParseError, Scope.Scope>
  }
}
```

## [3]-[STEP_DRIVE]

[STEP_DRIVE]:
- Owner: `Transition.step` — the Mealy shape `(phase, signal) => [next, verdict]` read straight off the table, the exact step a stream lifts unchanged through `Stream.mapAccum`; `Transition.drive` threads a signal batch through `Array.mapAccum`, one traversal emitting a verdict per signal.
- Law: carried state decouples from emitted verdict — the step returns both and never mutates; a `let` advancing a phase beside a map, or a reply table declared beside the `(next, emit)` rows it duplicates, is the rejected form.
- Law: the pure fold and the actor run the same rows — the actor's handler reads `rows[state][signal]` identically, so a machine proven at the pure altitude behaves identically booted, and the proof harness exercises `drive` without an actor.
- Growth: a new pure read (reachability census over the rows, terminal-phase detection) is one member composing the same table.

```typescript
const _step = <P extends string, S extends string, V extends string>(
  rows: Transition.Table<P, S, V>,
): Transition.Step<P, S, V> =>
(phase, signal) => pipe(rows[phase][signal], (row) => [row.next, row.emit] as const)

const _drive = <P extends string, S extends string, V extends string>(rows: Transition.Table<P, S, V>) =>
(origin: P, signals: ReadonlyArray<S>): [P, ReadonlyArray<V>] =>
  Array.mapAccum(signals, origin, _step(rows))
```

## [4]-[ACTOR]

[ACTOR]:
- Owner: `Transition.boot` — the serializable actor: `Machine.makeSerializable({ state, input }, initialize)` over the spec's phase schema, one `Feed` request carrying a signal and one `Poll` request reading the phase, each handler `({ request, state })` returning `[reply, state]` on the rail, booted scoped so the actor and its keyed watchers die with the region; `Transition.restore` boots a successor from a frozen snapshot, resuming exactly where the first life stopped.
- Law: every transition re-arms or disarms the deadline — the handler reads the entered phase's watch row and `forkReplace`s the keyed watcher with a delayed self-`Feed`, or replaces it with `Effect.void` when the phase carries no row — so at most one watcher lives per actor, stacked signals cannot stack timers, and the watchdog is recoverable from the spec.
- Law: recovery and durability are definition facts — `Machine.retry(spec.recover)` re-initializes through the same initialize slot carrying the last live state (`previous ?? origin`), and `freeze` is `Machine.snapshot` — the schema-encoded `[input, state]` pair carried opaque; `restore` re-admits it through the machine's own schemas, so a forged snapshot fails as a `ParseError`, never a corrupted boot.
- Law: the request surface is closed at feed/poll — signals ARE the protocol, so a machine never grows verb-shaped procedures; a request that demands its own payload and reply contract is evidence the concern outgrew the transition altitude and belongs to the runtime branch's workflow plane.
- Law: the booted actor serializes every request against one state on one fiber — concurrent feeds queue, never interleave — and the live phase binds to view atoms through the actor's subscribable state at the ui altitude; `Poll` is the rail-side read.
- Boundary: the `Persistence`-backed snapshot store, the workflow altitude (durable-execution replay, activities, compensation), and cluster sharding are runtime-branch concerns; this owner fixes the in-process actor and its vocabulary.
- Growth: a phase-entry side effect (a notification, a metric) is a tap composed at the consumer on the verdict stream, never a fourth handler concern.

```typescript
const _compiled = <P extends string, S extends string, V extends string>(spec: Transition.Spec<P, S, V>) => {
  class Feed extends Schema.TaggedRequest<Feed>()("Feed", {
    failure: Schema.Never,
    success: spec.verdict,
    payload: { signal: spec.signal },
  }) {}
  class Poll extends Schema.TaggedRequest<Poll>()("Poll", {
    failure: Schema.Never,
    success: spec.phase,
    payload: {},
  }) {}
  const machine = Machine.makeSerializable({ state: spec.phase, input: spec.phase }, (origin, previous) =>
    Machine.serializable.make(previous ?? origin).pipe(
      Machine.serializable.add(Feed, ({ forkReplace, request, send, state }) =>
        Effect.gen(function* () {
          const row = spec.rows[state][request.signal]
          yield* forkReplace(
            Option.match(Option.fromNullable(spec.watch[row.next]), {
              onNone: () => Effect.void,
              onSome: (armed) => Effect.delay(send(new Feed({ signal: armed.signal })), armed.after),
            }),
            "watch",
          )
          return [row.emit, row.next] as const
        })),
      Machine.serializable.add(Poll, ({ state }) => Effect.succeed([state, state] as const)),
    ),
  ).pipe(Machine.retry(spec.recover))
  return { machine, Feed, Poll }
}

const _surfaced = <P extends string, S extends string, V extends string>(
  compiled: ReturnType<typeof _compiled<P, S, V>>,
  actor: Machine.SerializableActor<ReturnType<typeof _compiled<P, S, V>>["machine"]>,
): Transition.Actor<P, S, V> => ({
  feed: (signal) => actor.send(new compiled.Feed({ signal })),
  phase: actor.send(new compiled.Poll()),
  freeze: Machine.snapshot(actor),
})

const _boot = <P extends string, S extends string, V extends string>(
  spec: Transition.Spec<P, S, V>,
  origin: P,
): Effect.Effect<Transition.Actor<P, S, V>, ParseResult.ParseError, Scope.Scope> =>
  Effect.gen(function* () {
    const compiled = _compiled(spec)
    const actor = yield* Machine.boot(compiled.machine, origin)
    return _surfaced(compiled, actor)
  })

const _restore = <P extends string, S extends string, V extends string>(
  spec: Transition.Spec<P, S, V>,
  frozen: Transition.Frozen,
): Effect.Effect<Transition.Actor<P, S, V>, ParseResult.ParseError, Scope.Scope> =>
  Effect.gen(function* () {
    const compiled = _compiled(spec)
    const actor = yield* Machine.restore(compiled.machine, frozen)
    return _surfaced(compiled, actor)
  })

const Transition: Transition.Shape = {
  spec: (spec) => spec,
  step: _step,
  drive: _drive,
  boot: _boot,
  restore: _restore,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Transition }
```
