# [RUNTIME_COORDINATE]

Distributed coordination is one engine-blind port beside the fanout plane: `Accord` owns the mutual-exclusion lease, leader election, and revision-guarded shared state that keep many processes — and many tabs — agreeing without a second store. The engines are rows: the `kv` row rides `@nats-io/kv` revision-CAS over the same `Wire` connection the fanout engine holds — `create` is the claim mint, `update` at a read revision is the only write that can win a race, a leader's seat survives through a marker-TTL heartbeat so a crashed holder expires instead of deadlocking the fleet; the `locks` row rides the browser's own `navigator.locks` arbiter for cross-tab exclusion, where the ledger members honestly answer their absence. Every read is a versioned fact — value plus revision — never a bare value, so compare-and-swap is spellable by construction and last-writer-wins is a deliberate row choice made elsewhere. A polled `get` waiting for absence, a hand lock file, a second dial beside `Wire`, and a fanout topic bent into a mutex are the named defects; the bucket is bounded coordination state, never the system of record. The module ships the `kv` row on the `./server` subpath; the `locks` row is the browser condition. The module is `runtime/src/net/coordinate.ts`.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]    | [OWNS]                                                                            | [PUBLIC]               |
| :-----: | :----------- | :--------------------------------------------------------------------------------- | :--------------------- |
|  [01]   | `PORT_SHAPE` | the engine-neutral port — lease, elect, cas, read, watch — the fact and the faults | `Accord`, `AccordFault` |
|  [02]   | `KV_ROW`     | the distributed engine: claim mint, TTL-heartbeat seat, revision-CAS, watch tail   | `Engines.kv`           |
|  [03]   | `LOCKS_ROW`  | the browser engine: Web Locks arbiter bridge, honest ledger degradation            | `Engines.locks`        |

## [2]-[PORT_SHAPE]

[PORT_SHAPE]:
- Owner: the `Accord` Tag — five members over the coordination name. `lease(name, mode)` is the scoped exclusive hold: the scope opens holding the lock and closing it releases, `mode` selecting `wait` (suspend until granted), `try` (fail `busy` instantly), or `steal` (evict the holder — the operator's recovery arm); `elect(name)` is the scoped seat claim answering `"leader" | "follower"` — a leader keeps the seat alive for the scope's lifetime and a follower composes `watch` to react to succession; `cas(key, expected, next)` writes shared state only when the ledger still holds the expected fact (`Option.none()` is create-if-absent), answering the settled fact; `read(key)` answers the current fact as `Option`; `watch(key)` tails the fact's changes as a stream.
- Law: the fault family is one reason-discriminated class — `dial` (the engine's transport is unreachable, class `unavailable`), `busy` (a `try` lease found the lock held, class `unavailable` — retryable by the caller's own schedule), `stale` (a CAS lost its race, class `conflicted` — re-read then re-fold, never blind retry), `ledger` (the engine carries no state ledger, class `absent` — the locks row's honest answer to `cas`/`read`/`watch`) — so the core budget gate re-drives the transient rows and a lost CAS routes to a re-read.
- Law: state is versioned facts — `Accord.Fact` is value plus revision; a caller that writes without a prior fact spells `Option.none()` and gets create-if-absent semantics, so an unguarded overwrite is unspellable through this port.
- Law: the port is engine-blind and identity-scoped — no member names NATS or the Web Locks API; a per-tenant or per-app lease is a name prefix, so thousands of apps coordinate on one plane without a surface change.
- Entry: `yield* Accord` then the five members; engines land as `Engines.kv(bucket)` / `Engines.locks()` root Layers.
- Packages: `effect` (`Context`, `Data`, `Option`, `Stream`), `@rasm/ts/core` (`FaultClass`).

```typescript
import { Context, Data, Deferred, Duration, Effect, Layer, Option, Random, Ref, Schedule, type Scope, Stream } from "effect"
import { type KV, Kvm } from "@nats-io/kv"
import type { FaultClass } from "@rasm/ts/core"
import { Wire } from "./pubsub.ts"

class AccordFault extends Data.TaggedError("AccordFault")<{
  readonly reason: "dial" | "busy" | "stale" | "ledger"
  readonly name: string
}> {
  get class(): FaultClass.Kind {
    return this.reason === "stale" ? "conflicted" : this.reason === "ledger" ? "absent" : "unavailable"
  }
}

declare namespace Accord {
  type Mode = "wait" | "try" | "steal"
  type Seat = "leader" | "follower"
  type Fact = { readonly value: Uint8Array; readonly revision: number }
}

class Accord extends Context.Tag("runtime/Accord")<Accord, {
  readonly lease: (name: string, mode?: Accord.Mode) => Effect.Effect<void, AccordFault, Scope.Scope>
  readonly elect: (name: string) => Effect.Effect<Accord.Seat, AccordFault, Scope.Scope>
  readonly cas: (key: string, expected: Option.Option<Accord.Fact>, next: Uint8Array) => Effect.Effect<Accord.Fact, AccordFault>
  readonly read: (key: string) => Effect.Effect<Option.Option<Accord.Fact>, AccordFault>
  readonly watch: (key: string) => Stream.Stream<Option.Option<Accord.Fact>, AccordFault>
}>() {}
```

## [3]-[KV_ROW]

[KV_ROW]:
- Owner: `Engines.kv(bucket)` — the distributed engine over one `Kvm(nc).create(bucket)` bucket riding the shared `Wire` connection. A lease is a `create` claim released by `purge` under the scope bracket: `try` surfaces the claim conflict as `busy`, `wait` parks on the key's `watch` tail and re-claims when a tombstone lands (event-driven, never a polled `get`), `steal` purges then claims. A seat is a marker-TTL claim plus a scoped heartbeat: `elect` claims with `_LEASE.ttl`, a winner forks a scoped refresh that `update`s at the tracked revision every half-TTL so a crashed leader expires by the server's clock and the fleet re-elects off the watch tail — no session daemon, no lock server.
- Law: CAS is the write mode — `cas` compiles `Option.none()` to `create` and a held fact to `update(key, next, revision)`; the server rejects a stale revision and the engine folds it to `stale`, so the caller re-reads and re-folds; a blind `put` is not reachable through this engine.
- Law: reads are facts — `get` folds `null` and tombstone operations (`DEL`, `PURGE`) to `Option.none()`, a live entry to `{ value, revision }`; `watch` lifts the bucket's ordered iterator through `Stream.fromAsyncIterable` under a scoped bracket and projects the same fold, so the tail and the point read agree on one shape.
- Law: bucket ensure is Layer construction — `kvm.create(bucket)` at engine build from the root's bucket name; bucket shape never lives beside a call site, and the bucket is bounded coordination state whose history depth is a bucket option, never an audit log.
- Boundary: the connection is `pubsub#JETSTREAM_ROW`'s `Wire` — this engine never dials; the ordered watch iterator carries no ack surface, exactly as the fanout ordered lane.
- Packages: `@nats-io/kv` (`Kvm`, `KV`), `effect` (`Effect`, `Layer`, `Ref`, `Schedule`, `Stream`, `Random`, `Duration`), `./pubsub.ts` (`Wire`).

```typescript
const _LEASE = { ttl: Duration.seconds(30) } as const

const _fact = (entry: { readonly value: Uint8Array; readonly revision: number; readonly operation: string } | null): Option.Option<Accord.Fact> =>
  entry === null || entry.operation !== "PUT"
    ? Option.none()
    : Option.some({ value: entry.value, revision: entry.revision })

const _kv = (bucket: string): Layer.Layer<Accord, AccordFault, Wire> =>
  Layer.scoped(
    Accord,
    Effect.gen(function* () {
      const nc = yield* Wire
      const kv: KV = yield* Effect.tryPromise({
        try: () => new Kvm(nc).create(bucket),
        catch: () => new AccordFault({ reason: "dial", name: bucket }),
      })
      const nonce = Effect.map(Random.nextInt, (seed) => new TextEncoder().encode(seed.toString(36)))

      const tailed = (key: string): Stream.Stream<Option.Option<Accord.Fact>, AccordFault> =>
        Stream.unwrapScoped(
          Effect.map(
            Effect.acquireRelease(
              Effect.tryPromise({
                try: () => kv.watch({ key }),
                catch: () => new AccordFault({ reason: "dial", name: key }),
              }),
              (live) => Effect.sync(() => live.stop()),
            ),
            (iterator) =>
              Stream.map(
                Stream.fromAsyncIterable(iterator, () => new AccordFault({ reason: "dial", name: key })),
                _fact,
              ),
          ),
        )

      const claimed = (name: string, id: Uint8Array, ttl?: Duration.Duration): Effect.Effect<number, AccordFault> =>
        Effect.tryPromise({
          try: () => kv.create(name, id, ttl === undefined ? undefined : Duration.toMillis(ttl)),
          catch: () => new AccordFault({ reason: "busy", name }),
        })

      const freed = (name: string): Effect.Effect<void> =>
        Effect.ignore(Effect.tryPromise({ try: () => kv.purge(name), catch: () => new AccordFault({ reason: "dial", name }) }))

      const parked = (name: string): Effect.Effect<void, AccordFault> =>
        Effect.asVoid(Stream.runHead(Stream.filter(tailed(name), Option.isNone)))

      const holding = (name: string, mode: Accord.Mode): Effect.Effect<number, AccordFault> =>
        mode === "steal"
          ? Effect.flatMap(nonce, (id) => Effect.zipRight(freed(name), claimed(name, id)))
          : mode === "try"
            ? Effect.flatMap(nonce, (id) => claimed(name, id))
            : Effect.flatMap(nonce, (id) => {
                const attempt: Effect.Effect<number, AccordFault> = Effect.catchIf(
                  claimed(name, id),
                  (fault) => fault.reason === "busy",
                  () => Effect.zipRight(parked(name), Effect.suspend(() => attempt)),
                )
                return attempt
              })

      return {
        lease: (name, mode = "wait") =>
          Effect.asVoid(Effect.acquireRelease(holding(name, mode), () => freed(name))),
        elect: (name) =>
          Effect.gen(function* () {
            const id = yield* nonce
            const seat = yield* claimed(name, id, _LEASE.ttl).pipe(
              Effect.map(Option.some),
              Effect.catchIf((fault) => fault.reason === "busy", () => Effect.succeed(Option.none<number>())),
            )
            return yield* Option.match(seat, {
              onNone: () => Effect.succeed("follower" as const),
              onSome: (revision) =>
                Effect.gen(function* () {
                  const held = yield* Ref.make(revision)
                  yield* Effect.addFinalizer(() => freed(name))
                  yield* Effect.forkScoped(
                    Effect.repeat(
                      Effect.flatMap(Ref.get(held), (at) =>
                        Effect.flatMap(
                          Effect.tryPromise({
                            try: () => kv.update(name, id, at),
                            catch: () => new AccordFault({ reason: "stale", name }),
                          }),
                          (next) => Ref.set(held, next),
                        )),
                      Schedule.spaced(Duration.times(_LEASE.ttl, 0.5)),
                    ),
                  )
                  return "leader" as const
                }),
            })
          }),
        cas: (key, expected, next) =>
          Effect.map(
            Option.match(expected, {
              onNone: () =>
                Effect.tryPromise({
                  try: () => kv.create(key, next),
                  catch: () => new AccordFault({ reason: "stale", name: key }),
                }),
              onSome: (fact) =>
                Effect.tryPromise({
                  try: () => kv.update(key, next, fact.revision),
                  catch: () => new AccordFault({ reason: "stale", name: key }),
                }),
            }),
            (revision) => ({ value: next, revision }),
          ),
        read: (key) =>
          Effect.map(
            Effect.tryPromise({ try: () => kv.get(key), catch: () => new AccordFault({ reason: "dial", name: key }) }),
            _fact,
          ),
        watch: tailed,
      }
    }),
  )
```

## [4]-[LOCKS_ROW]

[LOCKS_ROW]:
- Owner: `Engines.locks()` — the browser engine over the origin's own lock arbiter. A lease bridges `navigator.locks.request(name, { mode: "exclusive", ifAvailable, steal }, grant)` to the scope: the grant callback settles a granted `Deferred` and then parks on a release `Deferred` the scope's finalizer resolves, so the platform holds the lock exactly as long as the scope lives and an orphaned hold is unspellable; a `try` miss (the callback receives `null`) folds to `busy`. `elect` is the `try` lease read as a seat — the arbiter's own queue is the succession order, so a follower simply re-elects when its own later request is granted.
- Law: the ledger members answer honestly — `cas`, `read`, and `watch` fold to the `ledger` fault because the arbiter holds no state; a browser workload needing shared facts dials the `kv` row over websockets, and a session cell is `browser/persist`'s concern, never this port's.
- Law: the callback seam is the platform-forced boundary — the grant callback runs `Effect.runPromise` over pure `Deferred` settles only (no capability, no domain logic crosses), the sanctioned bridge spelling. Exemption: the grant callback is the one statement kernel.
- Boundary: cross-tab exclusion only — the arbiter scopes to the origin's agent cluster; process-plane coordination is the `kv` row's.
- Packages: `effect` (`Deferred`, `Effect`, `Layer`), the host `navigator.locks` Web API at the sanctioned FFI seam.

```typescript
const _locks = (): Layer.Layer<Accord> =>
  Layer.succeed(
    Accord,
    (() => {
      const held = (name: string, mode: Accord.Mode): Effect.Effect<void, AccordFault, Scope.Scope> =>
        Effect.gen(function* () {
          const granted = yield* Deferred.make<boolean>()
          const gate = yield* Deferred.make<void>()
          yield* Effect.acquireRelease(
            Effect.sync(() =>
              void navigator.locks.request(
                name,
                { mode: "exclusive", ifAvailable: mode === "try", steal: mode === "steal" },
                (lock) =>
                  Effect.runPromise(
                    Effect.zipRight(
                      Deferred.succeed(granted, lock !== null),
                      lock === null ? Effect.void : Deferred.await(gate),
                    ),
                  ),
              )),
            () => Deferred.succeed(gate, void 0),
          )
          return (yield* Deferred.await(granted))
            ? yield* Effect.void
            : yield* new AccordFault({ reason: "busy", name })
        })
      const absent = (name: string) => new AccordFault({ reason: "ledger", name })
      return {
        lease: (name, mode = "wait") => held(name, mode),
        elect: (name) =>
          held(name, "try").pipe(
            Effect.as("leader" as const),
            Effect.catchIf((fault) => fault.reason === "busy", () => Effect.succeed("follower" as const)),
          ),
        cas: (key) => Effect.fail(absent(key)),
        read: (key) => Effect.fail(absent(key)),
        watch: (key) => Stream.fail(absent(key)),
      }
    })(),
  )

const Engines = { kv: _kv, locks: _locks } as const

// --- [EXPORTS] --------------------------------------------------------------------------

export { Accord, AccordFault, Engines }
```
