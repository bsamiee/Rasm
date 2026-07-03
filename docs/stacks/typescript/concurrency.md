# [TS_CONCURRENCY]

Parallel work has a structural owner or it does not exist: every fiber parents to the fiber that forked it, to the `Scope` that admitted it, or — under an audited exemption — to the global scope, so an orphaned computation is unspellable rather than discouraged. Interruption is the third outcome riding `Cause`, never a value in the error channel and never a boolean flag: a deadline is a `timeout` member interrupting the loser, a shield is a masked window whose wait is restored, and compensation attaches through `Effect.onInterrupt` at the owner. Waiting is suspension on a primitive — `Deferred` handshake, `Latch` gate, `STM.check` — never a poll; channels are selected by consumption shape (`Queue` distributes, `PubSub` replicates, `Mailbox` ends); shared cells are selected by invariant span (`Ref` for one cell, `SynchronizedRef` for effectful update, STM for the multi-cell commit); keyed contention rides the `Cache`/`RcMap`/`Pool`/`RateLimiter` owners; and every fan-out declares its degree. This page owns who runs concurrently and who owns mutable state; the `Exit`/`Cause` fold and the `acquireRelease` bracket are `rails-and-effects.md`'s, channel-to-`Stream` ingress and `SubscriptionRef.changes` consumption are `streams.md`'s, and the root assembly that satisfies `Scope` and runtime services is `services-and-layers.md`'s.

## [01]-[PRIMITIVE_CHOOSER]

This table selects the owning primitive for a concurrent effect; when an effect matches several rows the most specific wins, and the ownership rows are read before the coordination rows.

| [INDEX] | [EFFECT_SIGNATURE]                         | [PRIMITIVE]                                    | [REJECTED_FORM]                            |
| :-----: | :----------------------------------------- | :--------------------------------------------- | :------------------------------------------ |
|  [01]   | child rejoins the owner, failure re-raises | `Effect.fork` + `Fiber.join`                   | `Effect.runFork` inside domain flow          |
|  [02]   | child lifetime rides a region              | `Effect.forkScoped`                            | `forkDaemon` plus a hand-tracked kill list   |
|  [03]   | one live fiber per key, restart supersedes | `FiberMap.run`                                 | a `Map<string, Fiber>` registry              |
|  [04]   | at most one background instance            | `FiberHandle.run`                              | an "already running" boolean flag            |
|  [05]   | commit survives interrupt, wait does not   | `Effect.uninterruptibleMask` + `restore`       | `Effect.uninterruptible` over the whole flow |
|  [06]   | compensation only on interrupt             | `Effect.onInterrupt`                           | `Effect.ensuring` firing on every exit       |
|  [07]   | expiry aborts into the fault family        | `Effect.timeoutFail`                           | a `Date.now()` deadline comparison           |
|  [08]   | expiry settles as a value                  | `Effect.timeoutOption`                         | `Cause.TimeoutException` caught downstream   |
|  [09]   | one-shot typed handoff                     | `Deferred`                                     | a polled `Ref` flag                          |
|  [10]   | each value reaches exactly one taker       | `Queue.bounded`/`sliding`/`dropping`           | a `PubSub` with one subscriber               |
|  [11]   | every subscriber sees every value          | `PubSub` + scoped `subscribe`                  | offers looped over N queues                  |
|  [12]   | producer must signal done or failure       | `Mailbox`                                      | a poison-pill sentinel on a `Queue`          |
|  [13]   | in-flight bound spanning call sites        | `Effect.makeSemaphore` + `withPermits`         | a counter `Ref` incremented by hand          |
|  [14]   | one cell, pure update, atomic report       | `Ref.modify`                                   | `Ref.get` then `Ref.set`                     |
|  [15]   | the update is itself an effect             | `SynchronizedRef.updateEffect`                 | a one-permit semaphore around a `Ref`        |
|  [16]   | invariant spans cells or waits on one      | `STM.gen` + `STM.check` + `STM.commit`         | lock ordering over two `Ref`s                |
|  [17]   | keyed value memoized under TTL             | `Cache.make`                                   | `HashMap` plus hand-rolled expiry            |
|  [18]   | keyed resource with refcounted lifetime    | `RcMap.make`                                   | a `Cache` holding connections                |
|  [19]   | throughput window per principal            | `RateLimiter.make`                             | `Effect.sleep` pacing between calls          |
|  [20]   | fan-out over a collection                  | explicit `{ concurrency }` on `Effect.forEach` | the option omitted — silent serialization    |

## [02]-[FIBER_OWNERSHIP]

A fork is an ownership statement, not a scheduling detail: the parent selected at the fork site decides when the child dies, and the handle member selected at the observation site decides whose failure it becomes. Registries of fibers are owned surfaces, never hand-rolled maps.

[FORK_PARENT]:
- Law: `Effect.fork` parents the child to the forking fiber — the child is interrupted when the parent completes, so lifetime is an inherited fact, never a tracked variable; `Effect.forkScoped` re-parents to the `Scope` in the requirement channel and `Effect.forkIn(scope)` pins an explicit one, so a region rather than a caller ends the fiber; `Effect.forkDaemon` parents to the global scope and is admitted only where the lifetime provably exceeds every scope in the program and the disposition still reaches an observer through a channel — an unobserved daemon is the leak this page exists to prevent, and `Effect.forkAll` fans a collection under one parent decision.
- Law: the handle algebra is disposition-selected: `Fiber.join` re-raises the child's failure inside the owner and is the default rejoin; `Fiber.await` returns the `Exit` and never fails the observer — the supervision read; `Fiber.interrupt` returns only after the child's finalizers ran; `Fiber.interruptFork` returns immediately — the teardown that must not block the owner's hot path. `Fiber.joinAll`/`Fiber.awaitAll`/`Fiber.interruptAll` fold the collection forms.
- Law: `Effect.supervised` reports children to a `Supervisor.track` snapshot, `Effect.awaitAllChildren` holds the owner until transitive children settle, and `Effect.daemonChildren` re-parents every fork inside a region to the global scope — the escape is declared once at the owner, never sprinkled per fork site.
- Reject: `Effect.runFork` inside domain flow — running is the program edge's verb; `forkDaemon` by reflex; a `for` loop of joins where `Fiber.joinAll` folds it.

[HANDLE_REGISTRY]:
- Law: a fiber registry is an owned surface: `FiberHandle` owns the at-most-one slot, `FiberMap` the one-per-key family, `FiberSet` the homogeneous fan — each is `Scope`-bound at `make`, self-evicts on fiber completion, and interrupts every member when the scope closes, so a stale handle and a leaked registry are structurally impossible.
- Law: `run` on a keyed owner supersedes — the previous holder is interrupted before the replacement forks — and `{ onlyIfMissing: true }` flips supersede into dedupe, so restart-versus-join is an option value at one entrypoint, never two registry types; `join` propagates the first member failure into the owner, which makes crash-the-supervisor a one-combinator policy, and `awaitEmpty` is the quiescence read.
- Reject: `Map<string, Fiber.Fiber<void, never>>` with hand eviction; an "already running" boolean beside a fork; per-member interrupt loops where closing the owning scope already interrupts the set.

```typescript
import { Data, Effect, Fiber, FiberHandle, FiberMap } from "effect"
import type { Exit, Scope } from "effect"

class SlotFault extends Data.TaggedError("SlotFault")<{ readonly key: string }> {}

type Spawned = {
  readonly enroll: (key: string, work: Effect.Effect<void, SlotFault>) => Effect.Effect<Fiber.RuntimeFiber<void, SlotFault>>
  readonly pulse: Effect.Effect<Exit.Exit<void, SlotFault>>
  readonly settle: Effect.Effect<void, SlotFault>
}

const spawned = (
  feed: Effect.Effect<void, SlotFault>,
  sweep: Effect.Effect<void, SlotFault>,
): Effect.Effect<Spawned, never, Scope.Scope> =>
  Effect.gen(function* () {
    const registry = yield* FiberMap.make<string, void, SlotFault>()          // keyed family: scope-bound, self-evicting
    const janitor = yield* FiberHandle.make<void, SlotFault>()
    const feeder = yield* Effect.forkScoped(feed)                             // region-parented: the Scope, not the caller, ends it
    yield* FiberHandle.run(janitor, { onlyIfMissing: true })(sweep)           // singleton slot: a second run is a no-op, not a twin
    return {
      enroll: (key, work) => FiberMap.run(registry, key, work),               // supersede: the prior holder is interrupted first
      pulse: Fiber.await(feeder),                                             // Exit disposition: observation never fails the observer
      settle: FiberMap.join(registry).pipe(Effect.ensuring(Fiber.interruptFork(feeder))),  // first child failure fails the owner; teardown never blocks
    }
  })

// --- [EXPORTS] ---------------------------------------------------------------------------

export { SlotFault, spawned }
export type { Spawned }
```

## [03]-[INTERRUPTION_RAIL]

Interruption is the third outcome — distinct from success and from every fault — and it is a rail of its own: shields are windows, deadlines are interruptions, and compensation is declared at the owner.

[SHIELDED_WINDOW]:
- Law: interruption travels `Cause`, never `E` — `Effect.catchAll` cannot see it, no fault case represents it, and a "cancelled" domain fault may be minted only where an `Exit` folds at the boundary, a fold `rails-and-effects.md` owns.
- Law: the shield is a masked window, not a mode: `Effect.uninterruptibleMask((restore) => ...)` keeps the commit shielded while `restore` re-opens the wait, so external interrupts and deadlines land exactly in the window designed to absorb them; a bare `Effect.uninterruptible` over a composed flow is the unbounded shield that silently disables cancellation, and `Effect.interruptible` inside a mask is the same claim made without the mask's discipline.
- Law: `Effect.onInterrupt` attaches interrupt-only compensation at the owner declaration; `Effect.ensuring` fires on every exit and `Effect.onExit` folds the disposition — the exit class the cleanup must see selects the combinator, never a flag inspected inside one catch-all finalizer.
- Reject: a cancelled flag polled in the body; `AbortController` threaded through domain flow — the signal converts once at the platform seam (`boundaries.md`); catching the interrupt to return a value.

[DEADLINE_FAMILY]:
- Law: a deadline interrupts the loser — it is a `timeout` member composed at the owner, never a `Date.now()` comparison or a deadline parameter threaded inward; the expiry disposition selects the member: `Effect.timeout` aborts with `Cause.TimeoutException`, `Effect.timeoutFail` mints the domain fault at the seam, `Effect.timeoutOption` settles expiry as `Option.none`, and `Effect.timeoutTo` folds both arms into one carrier in a single declaration.
- Law: an uninterruptible interior outlives its deadline by design — the timeout on a shielded region waits, which is correct and is the reason the shield must be minimal; where the deadline must settle on time regardless, `Effect.disconnect` severs the interior onto its own fiber so the owner's clock is honored while the shielded work finishes in background.
- Reject: `Cause.TimeoutException` caught downstream where `timeoutFail` mints the typed fault at the owner; racing a hand-rolled `Effect.sleep` against work a `timeout` member already owns.

```typescript
import { Data, Effect } from "effect"
import type { Duration, Option } from "effect"

class ExpiredFault extends Data.TaggedError("ExpiredFault")<{ readonly stage: string }> {}

const committed = <A, E, R>(options: {
  readonly claim: Effect.Effect<A, E, R>
  readonly persist: (row: A) => Effect.Effect<void, E, R>
  readonly vacate: Effect.Effect<void, never, R>
  readonly budget: Duration.DurationInput
}): Effect.Effect<A, E | ExpiredFault, R> =>
  Effect.uninterruptibleMask((restore) =>
    restore(
      options.claim.pipe(Effect.timeoutFail({ onTimeout: () => new ExpiredFault({ stage: "<claim>" }), duration: options.budget })),
    ).pipe(
      Effect.onInterrupt(() => options.vacate),                            // interrupt-only compensation for the abandoned wait
      Effect.flatMap((row) => options.persist(row).pipe(Effect.as(row))),  // shielded commit: runs to completion once the claim lands
    ),
  )

const flushed = <A, E, R>(drain: Effect.Effect<A, E, R>, patience: Duration.DurationInput): Effect.Effect<Option.Option<A>, E, R> =>
  drain.pipe(Effect.uninterruptible, Effect.disconnect, Effect.timeoutOption(patience))  // deadline settles on time; the shielded drain finishes in background

// --- [EXPORTS] ---------------------------------------------------------------------------

export { ExpiredFault, committed, flushed }
```

## [04]-[COORDINATION_TOPOLOGY]

Fibers coordinate through typed primitives whose topology is fixed at construction: the handshake is one-shot and typed both ways, the channel's consumption shape and capacity policy are declared where it is built, and the permit is the only cross-site concurrency bound.

[HANDSHAKE_SIGNAL]:
- Law: a one-shot typed handoff is a `Deferred<A, E>` — `Deferred.await` suspends every taker, `Deferred.succeed`/`Deferred.fail` settle exactly once and report a late settle as a `false` return, never an error; readiness that carries evidence rides the success value, the failing arm is typed, and `Deferred.poll` reads without suspending.
- Law: `Deferred.complete` memoizes one evaluation of its effect across all takers; `Deferred.completeWith` hands each taker the unevaluated effect — per-taker evaluation is a semantic choice made at the settle site, not an optimization accident discovered later.
- Law: a value-less reusable gate is `Effect.makeLatch` — `whenOpen` gates a section, `open` releases every waiter and stays open, `close` re-arms; a set-once gate that would carry a value is a `Deferred`, never a `Latch` beside a `Ref`.
- Reject: a `Ref` flag polled under `Effect.repeat`; a `Queue` of one sentinel standing for a handshake.

[CHANNEL_SELECT]:
- Law: topology is consumption shape: `Queue` distributes — each value reaches exactly one taker, the work-distribution channel with `take`/`takeAll`/`takeBetween` on the drain side; `PubSub` replicates — every subscriber's scoped `PubSub.subscribe` dequeue sees every `publish`, and unsubscription is the subscriber's scope closing, never a removal call; `Mailbox` is the single-consumer channel that ends — `offer`/`end`/`fail` on the producer side, `takeAll`/`takeN` returning `[Chunk, done]` on the consumer side — so producer completion and producer failure arrive typed at the drain seam, never as a sentinel value.
- Law: capacity policy is fixed at construction: `bounded` suspends the producer — backpressure as the default contract; `sliding` drops oldest for freshest-wins feeds; `dropping` refuses newest for first-wins admission; `unbounded` is admitted only with a bound proven at the producer. `Mailbox.into` runs a producer into the channel — success ends it, failure fails it — collapsing the hand-written match at every producer edge; draining a channel as a pipeline is `streams.md`'s ingress.
- Reject: a `PubSub` with one subscriber standing for a `Queue`; `Queue.shutdown` as a completion signal — it interrupts takers and carries no verdict; a poison-pill value where `Mailbox.end` is the verdict.

[PERMIT_DISCIPLINE]:
- Law: a concurrency bound spanning call sites is `Effect.makeSemaphore` — `withPermits(k)` brackets acquire and release around the section on success, failure, and interrupt alike; the permit count is a cost model, so a double-weight section takes `withPermits(2)` against the same semaphore rather than a second semaphore per weight class; `withPermitsIfAvailable` is the load-shed arm settling `Option.none` under saturation, and `resize` retunes the bound live.
- Reject: a counter `Ref` incremented by hand; bare `take`/`release` pairs where `withPermits` brackets them; a semaphore standing where the bound belongs to one fan-out's `{ concurrency }` option.

```typescript
import { Data, Deferred, Effect, Mailbox } from "effect"
import type { Chunk, Scope } from "effect"

class FeedFault extends Data.TaggedError("FeedFault")<{ readonly at: string }> {}

type Drained = { readonly expected: number; readonly rows: Chunk.Chunk<string>; readonly sealed: boolean }

const bridged = (
  keys: ReadonlyArray<string>,
  mint: (key: string) => Effect.Effect<string, FeedFault>,
): Effect.Effect<Drained, FeedFault, Scope.Scope> =>
  Effect.gen(function* () {
    const box = yield* Mailbox.make<string, FeedFault>({ capacity: 16, strategy: "suspend" })  // backpressure declared at construction
    const bound = yield* Deferred.make<number>()
    const gate = yield* Effect.makeSemaphore(4)                                // cross-site bound; the forEach degree is the local fan

    yield* Deferred.succeed(bound, keys.length).pipe(                          // handshake first: the consumer proceeds on typed evidence
      Effect.andThen(
        Effect.forEach(keys, (key) => gate.withPermits(1)(mint(key)).pipe(Effect.tap((row) => box.offer(row))), {
          concurrency: 8,
          discard: true,
        }),
      ),
      Mailbox.into(box),                                                       // success ends the channel, failure fails it — no sentinel
      Effect.forkScoped,
    )

    const expected = yield* Deferred.await(bound)
    const [rows, sealed] = yield* box.takeN(expected)                          // producer failure surfaces here, typed as FeedFault
    return { expected, rows, sealed }
  })

// --- [EXPORTS] ---------------------------------------------------------------------------

export { FeedFault, bridged }
export type { Drained }
```

## [05]-[SHARED_STATE]

Shared mutation is a cell with an owner: the update shape selects the cell, and an invariant that spans cells moves the cells into one transaction rather than adding a lock beside them.

[CELL_SELECT]:
- Law: `Ref` owns one cell under pure update — `Ref.modify` returns the report and commits the update in one atomic step, the take-a-ticket form; `SynchronizedRef.updateEffect`/`modifyEffect` serialize writers whose update is itself an effect; `SubscriptionRef` adds `changes` for observers — the stream is consumed under `streams.md`'s law; one composite immutable value in one `Ref` beats two cells guarded by discipline, because the composite commits as a unit.
- Law: `Ref.get` followed by `Ref.set` is a torn read-modify-write under contention — the pair collapses into `update`/`modify` without exception, and `getAndUpdate`/`updateAndGet` select which side of the transition the caller sees.
- Reject: a one-permit semaphore around a `Ref` where `SynchronizedRef` owns serialization; module-level mutable state; a `let` captured by concurrent closures.

[ATOMIC_COMMIT]:
- Law: an invariant spanning cells is one STM transaction: `TRef`/`TMap`/`TQueue` cells composed in `STM.gen` commit atomically at `STM.commit` — conflicting transactions re-run automatically, so the torn write and the lock-order deadlock are unspellable, and the STM body is pure by type: an effect cannot enter the transaction.
- Law: `STM.check` suspends the transaction until a participating cell changes and the predicate holds — wait-until-condition without a poll loop, with `STM.retry` as the raw suspend beneath it; the predicate closes over values already read in the same transaction, and the re-run re-reads them.
- Law: read/write-asymmetric sections ride `TReentrantLock` — `readLock`/`writeLock` are `Scope`-bound acquisitions, `withReadLock`/`withWriteLock` the bracketed forms — and transactional permits ride `TSemaphore.withPermits`; lock selection is STM-composed, never a JS mutex or a spin loop.
- Reject: two `Ref`s updated in sequence under a shared invariant; lock-ordering discipline where a transaction owns the cells; polling a cell for a threshold `STM.check` suspends on.

```typescript
import { Chunk, Effect, Ref, STM, TMap, TRef } from "effect"

type Claim = readonly [slot: string, weight: number]

const balanced = (
  claims: ReadonlyArray<Claim>,
  ceiling: number,
  work: (slot: string) => Effect.Effect<void>,
): Effect.Effect<{ readonly peak: number; readonly admitted: number }> =>
  Effect.gen(function* () {
    const load = yield* STM.commit(TRef.make(0))
    const crest = yield* STM.commit(TRef.make(0))
    const plan = yield* STM.commit(TMap.empty<string, number>())
    const tickets = yield* Ref.make(0)

    const admit = ([slot, weight]: Claim): Effect.Effect<void> =>
      STM.commit(
        STM.gen(function* () {
          const held = yield* TRef.get(load)
          yield* STM.check(() => held + weight <= ceiling)          // suspends until a cell changes; the re-run re-reads, never polls
          yield* TRef.set(load, held + weight)
          yield* TRef.update(crest, (peak) => Math.max(peak, held + weight))
          yield* TMap.set(plan, slot, weight)                       // three cells, one commit: a torn write is unspellable
        }),
      )

    const leased = (claim: Claim): Effect.Effect<number> =>
      Ref.modify(tickets, (next) => [next, next + 1] as const).pipe( // single cell: the report and the update commit together
        Effect.tap(() =>
          admit(claim).pipe(
            Effect.andThen(work(claim[0])),
            Effect.ensuring(STM.commit(TRef.update(load, (held) => held - claim[1]))),
          ),
        ),
      )

    yield* Effect.forEach(claims, leased, { concurrency: "unbounded", discard: true })  // the ceiling, not the degree, is the real bound
    return yield* STM.commit(STM.gen(function* () {
      return { peak: yield* TRef.get(crest), admitted: Chunk.size(yield* TMap.toChunk(plan)) }
    }))
  })

// --- [EXPORTS] ---------------------------------------------------------------------------

export { balanced }
export type { Claim }
```

## [06]-[CONTENTION_OWNERS]

Keyed contention is owned by four scoped surfaces selected on one axis — value, identity, fungible, window — and the policy that tunes each owner is a value declared beside it, never arithmetic spread through call sites.

[KEYED_LIFETIME]:
- Law: the keyed axis selects the owner: `Cache.make({ capacity, timeToLive, lookup })` memoizes values and collapses concurrent misses on one key into one in-flight lookup — the stampede is structurally impossible, not guarded; `RcMap.make({ lookup, idleTimeToLive })` owns per-key resources — `RcMap.get` is a scoped acquisition that refcounts, the resource releases `idleTimeToLive` after the last holder, and the `capacity` variant widens the error channel with `Cause.ExceededCapacityException` so saturation is a typed fault, never a silent drop; `Pool.make({ acquire, size })` and `Pool.makeWithTTL({ acquire, min, max, timeToLive, timeToLiveStrategy })` own fungible resources — `"creation"` versus `"usage"` anchors the TTL clock — and `KeyedPool` is the per-key pool family.
- Law: staleness policy is member-selected: `refresh` replaces in place with no eviction gap, `invalidate` evicts now, `Pool.invalidate` retires a broken member for replacement, `RcMap.touch` re-arms the idle clock — one owner, four verbs, never a parallel cache per staleness mode.
- Reject: a `HashMap` plus hand TTL arithmetic; a `Cache` holding connections — values are returned, resources are released; a free-list array with manual checkout.

[THROUGHPUT_WINDOW]:
- Law: a throughput bound is a scoped `RateLimiter.make({ limit, interval, algorithm })` owner — `"token-bucket"` smooths calls across the interval and absorbs bursts against accumulated tokens, `"fixed-window"` enforces hard per-interval quotas — and the algorithm is a policy row selected by the provider's contract, not a tuning afterthought; the limiter attaches at the owner declaration so every call site inherits the window.
- Law: weighted calls compose `RateLimiter.withCost` inside the limited task — cost is declared at the call's owner, never a second limiter per weight class; permits bound concurrency, windows bound throughput, and conflating the two is the named selection error.
- Reject: `Effect.sleep` pacing between calls; a semaphore standing for a rate window; per-call-site limiter construction where one scoped owner is the window.

```typescript
import { Cache, Duration, Effect, RateLimiter, RcMap } from "effect"
import type { Scope } from "effect"

const _PLANE = {                                             // interior policy row: no export reaches the anchor, so the expression-seam satisfies check rides it
  verdicts: { capacity: 512, timeToLive: Duration.minutes(5) },
  sessions: { idleTimeToLive: Duration.seconds(45) },
  egress: { limit: 90, interval: Duration.minutes(1), algorithm: "token-bucket" },
} as const satisfies {
  readonly verdicts: { readonly capacity: number; readonly timeToLive: Duration.Duration }
  readonly sessions: { readonly idleTimeToLive: Duration.Duration }
  readonly egress: { readonly limit: number; readonly interval: Duration.Duration; readonly algorithm: "fixed-window" | "token-bucket" }
}

type Plane<A, E, S> = {
  readonly verdict: (key: string) => Effect.Effect<A, E>
  readonly session: (key: string) => Effect.Effect<S, E, Scope.Scope>
  readonly evict: (key: string) => Effect.Effect<void>
}

const contended = <A, E, S, R>(
  mint: (key: string) => Effect.Effect<A, E, R>,
  open: (key: string) => Effect.Effect<S, E, R>,
): Effect.Effect<Plane<A, E, S>, never, Scope.Scope | R> =>
  Effect.gen(function* () {
    const limit = yield* RateLimiter.make(_PLANE.egress)
    const verdicts = yield* Cache.make({
      ..._PLANE.verdicts,
      lookup: (key: string) => limit(mint(key).pipe(RateLimiter.withCost(2))),  // window inherited by every miss; weight declared at the owner
    })
    const sessions = yield* RcMap.make({ lookup: open, ..._PLANE.sessions })
    return {
      verdict: (key) => verdicts.get(key),                          // concurrent misses on one key collapse to one lookup
      session: (key) => RcMap.get(sessions, key),                   // scoped acquisition: refcount up, idle clock after last release
      evict: (key) => verdicts.invalidate(key).pipe(Effect.andThen(RcMap.invalidate(sessions, key))),
    }
  })

// --- [EXPORTS] ---------------------------------------------------------------------------

export { contended }
export type { Plane }
```

## [07]-[DEGREE_AND_RACE]

Fan-out degree and racing are declared decisions on the combinator, never emergent behavior: an unstated degree is a silent serialization, and a race is a statement about which settlement wins and what happens to the loser.

[EXPLICIT_DEGREE]:
- Law: every fan-out declares its degree — `{ concurrency: N | "unbounded" | "inherit" }` on `Effect.forEach`/`Effect.all`; the omitted option executes sequentially, so the absent knob is itself a decision made silently, and `"unbounded"` is a written claim that a downstream owner — semaphore, limiter, pool — carries the real bound.
- Law: `"inherit"` parameterizes the owner: the fan-out reads the fiber's budget and `Effect.withConcurrency` sets it at the boundary — degree becomes configuration one root composes, consumers never re-assert it per call, and a `degree` parameter threaded through signatures re-states what the fiber already carries; `{ discard: true }` drops result assembly when the fan-out is effect-only.
- Reject: `Promise.all` at domain altitude; a fork-per-item loop where `forEach` owns the fan; per-call degree literals where one boundary owns the budget.

[RACE_SELECT]:
- Law: the race member is selected by what settles it: `Effect.race` — first success wins and the loser is interrupted; `Effect.raceAll` — first success across a collection, the hedge over redundant lanes; `Effect.raceFirst` — first completion wins including failure, the fail-fast pairing; `Effect.raceWith` — the general finisher fold whose `onSelfDone`/`onOtherDone` receive the `Exit` and the other fiber, selected when the loser's disposition is itself material.
- Law: losers are interrupted, so a raced effect is admissible only when its teardown is already owned by a bracket or scope — racing un-bracketed acquisition leaks on every losing arm, and the hedge's cost is the loser's finalizers running to completion.
- Reject: `Promise.race` at the seam; a hand-rolled first-wins where both arms settle one `Deferred` — `race` already owns loser interruption; a race against `Effect.sleep` where the deadline family owns expiry.

```typescript
import { Effect, Fiber } from "effect"
import type { Exit } from "effect"

const hedged = <A, E, R>(
  lanes: ReadonlyArray<(key: string) => Effect.Effect<A, E, R>>,
): ((keys: ReadonlyArray<string>) => Effect.Effect<ReadonlyArray<A>, E, R>) =>
(keys) =>
  Effect.forEach(keys, (key) => Effect.raceAll(lanes.map((lane) => lane(key))), { concurrency: "inherit" })  // degree deferred to the boundary

const staged = <A, E, R>(
  lanes: ReadonlyArray<(key: string) => Effect.Effect<A, E, R>>,
  keys: ReadonlyArray<string>,
): Effect.Effect<ReadonlyArray<A>, E, R> =>
  hedged(lanes)(keys).pipe(Effect.withConcurrency(8))               // the boundary owns the budget; the owner stays degree-agnostic

const shadowed = <A, E, R, R2>(
  live: Effect.Effect<A, E, R>,
  probe: Effect.Effect<A, E, R>,
  note: (exit: Exit.Exit<A, E>) => Effect.Effect<void, never, R2>,
): Effect.Effect<A, E, R | R2> =>
  Effect.raceWith(live, probe, {
    onSelfDone: (exit, other) => Fiber.interruptFork(other).pipe(Effect.andThen(exit)),   // live wins: the shadow dies without blocking
    onOtherDone: (exit, self) => note(exit).pipe(Effect.andThen(Fiber.join(self))),       // shadow settles first: record it, keep waiting on live
  })

// --- [EXPORTS] ---------------------------------------------------------------------------

export { hedged, shadowed, staged }
```
