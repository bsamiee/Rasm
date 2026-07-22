# [RUNTIME_COORDINATE]

Distributed coordination is one engine-blind port beside the fanout plane: `Accord` owns the mutual-exclusion lease, leader election, revision-guarded shared state, and the coordination census that keep many processes — and many tabs — agreeing without a second store. The engines are rows: the `kv` row rides `@nats-io/kv` revision-CAS over the same `Broker` connection the fanout engine holds — `create` is the claim mint, `update` at a read revision is the only write that can win a race, and every held claim lives under the bucket's own TTL clock refreshed by a scoped heartbeat, so a crashed holder expires by the server's clock and no name is ever permanently busy; the `locks` row rides the browser's own `navigator.locks` arbiter for cross-tab exclusion, where the ledger members honestly answer their absence. Every hold returns a receipt whose fencing token is the claim revision, so a downstream guarded write can prove seniority at the ledger and a stale holder's write loses structurally. Every read is a versioned fact — value plus revision — never a bare value, so compare-and-swap is spellable by construction and last-writer-wins is a deliberate row choice made elsewhere. A polled `get` waiting for absence, a hand lock file, a nonexpiring claim, a second dial beside `Broker`, and a fanout topic bent into a mutex are the named defects; the bucket is bounded coordination state, never the system of record. The module ships the `kv` row on the `./server` subpath; the `locks` row is the browser condition. The module is `runtime/src/net/coordinate.ts`.

## [01]-[CLUSTERS]

| [INDEX] | [CLUSTER]    | [OWNS]                                                                                     | [PUBLIC]                |
| :-----: | :----------- | :----------------------------------------------------------------------------------------- | :---------------------- |
|  [01]   | `PORT_SHAPE` | the engine-neutral port — lease, elect, cas, read, watch, census — receipts and the faults | `Accord`, `AccordFault` |
|  [02]   | `KV_ROW`     | the distributed engine: TTL-clocked claims, heartbeat holds, revision-CAS, watch tail      | `Accord.kv`             |
|  [03]   | `LOCKS_ROW`  | the browser engine: Web Locks arbiter bridge, arbiter census, honest ledger degradation    | `Accord.locks`          |

## [02]-[PORT_SHAPE]

[PORT_SHAPE]:
- Owner: the `Accord` Tag — six members over the coordination name. `lease(name, mode)` is the scoped exclusive hold answering an `Accord.Lease` receipt: the scope opens holding the lock and closing it releases, `mode` selecting `wait` (suspend until granted), `try` (fail `busy` instantly), or `steal` (evict the holder — the operator's recovery arm); `elect(name)` is the scoped seat claim answering an `Accord.Seat` — a leader's seat stays alive for the scope's lifetime and a follower composes `watch` to react to succession; `cas(key, expected, next)` writes shared state only when the ledger still holds the expected fact (`Option.none()` is create-if-absent), answering the settled fact; `read(key)` answers the current fact as `Option`; `watch(key)` tails the fact's changes as a stream; `census(filter)` enumerates the names currently coordinated — the doctor read `serve/cli` ops verbs consume.
- Law: the receipts carry fencing evidence — `Lease.token` and a leader `Seat.token` hold the claim revision as `Option` (the `locks` arbiter mints none), so a guarded downstream write spells `cas(key, expected, next)` seniority off the token and a holder that lost its seat cannot win a ledger race; a lease consumed as a bare grant with no fenced write is legal, a fenced write minted from anything but the receipt is not.
- Law: the fault family is one reason-discriminated class — `dial` (the engine's transport is unreachable, class `unavailable`), `busy` (a `try` lease found the lock held, class `unavailable` — retryable by the caller's own schedule), `stale` (a CAS lost its race, class `conflicted` — re-read then re-fold, never blind retry), `ledger` (the engine carries no state ledger, class `absent` — the locks row's honest answer to `cas`/`read`/`watch`) — so the core budget gate re-drives the transient rows and a lost CAS routes to a re-read.
- Law: state is versioned facts — `Accord.Fact` is value plus revision; a caller that writes without a prior fact spells `Option.none()` and gets create-if-absent semantics, so an unguarded overwrite is unspellable through this port.
- Law: the port is engine-blind and identity-scoped — no member names NATS or the Web Locks API; a per-tenant or per-app lease is a name prefix, so thousands of apps coordinate on one plane without a surface change, and `census(filter)` scopes the same way.
- Entry: `yield* Accord` then the six members; engines land as `Accord.kv(bucket)` / `Accord.locks()` root Layers.
- Packages: `effect` (`Context`, `Data`, `Option`, `Stream`), `@rasm/ts/core` (`FaultClass`).

```typescript
import { Context, Data, Deferred, Duration, Effect, Layer, Option, Random, Ref, Schedule, Schema, type Scope, Stream } from "effect"
import { type KV, Kvm } from "@nats-io/kv"
import type { FaultClass } from "@rasm/ts/core"
import { Broker } from "./pubsub.ts"

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
  type Role = "leader" | "follower"
  type Seat = _Seat
  type Lease = _Lease
  type Fact = _Fact
}

class _Seat extends Schema.Class<_Seat>("Accord/Seat")({
  role: Schema.Literal("leader", "follower"),
  token: Schema.optionalWith(Schema.Int.pipe(Schema.positive()), { as: "Option" }),
}) {}

class _Lease extends Schema.Class<_Lease>("Accord/Lease")({
  name: Schema.NonEmptyString,
  holder: Schema.NonEmptyString,
  token: Schema.optionalWith(Schema.Int.pipe(Schema.positive()), { as: "Option" }),
}) {}

class _Fact extends Schema.Class<_Fact>("Accord/Fact")({
  value: Schema.Uint8ArrayFromSelf,
  revision: Schema.Int.pipe(Schema.positive()),
}) {}

class Accord extends Context.Tag("runtime/Accord")<Accord, {
  readonly lease: (name: string, mode?: Accord.Mode) => Effect.Effect<Accord.Lease, AccordFault, Scope.Scope>
  readonly elect: (name: string) => Effect.Effect<Accord.Seat, AccordFault, Scope.Scope>
  readonly cas: (key: string, expected: Option.Option<Accord.Fact>, next: Uint8Array) => Effect.Effect<Accord.Fact, AccordFault>
  readonly read: (key: string) => Effect.Effect<Option.Option<Accord.Fact>, AccordFault>
  readonly watch: (key: string) => Stream.Stream<Option.Option<Accord.Fact>, AccordFault>
  readonly census: (filter?: string) => Effect.Effect<ReadonlyArray<string>, AccordFault>
}>() {
  static readonly Fact = _Fact
  static readonly Lease = _Lease
  static readonly Seat = _Seat
  static readonly kv = (bucket: string): Layer.Layer<Accord, AccordFault, Broker> => _kv(bucket)
  static readonly locks = (): Layer.Layer<Accord> => _locks()
}
```

## [03]-[KV_ROW]

[KV_ROW]:
- Owner: `Accord.kv(bucket)` — the distributed engine over one `Kvm(nc).create(bucket, { ttl })` bucket riding the shared `Broker` connection. Expiry is the bucket's clock, never a per-claim flag: the bucket-level `ttl` limit ages every message, so a claim not refreshed within the lease window vacates by the server's clock — holder loss can never leave a name permanently busy. Every winning claim — lease and seat alike — runs one `_seated` kernel: `create` mints the claim, a scope finalizer deletes only the revision it still owns, and a scoped half-TTL heartbeat `update`s at the tracked revision, resetting the message age while the holder lives; the receipt's fencing token is the claim revision. `try` surfaces the claim conflict as `busy`; `steal` purges then claims; `wait` parks on the key's `watch` tail and re-claims when a tombstone lands, racing the park against a TTL-cadence re-claim because a limit-expired key can vacate without a watch notification — the event tail is the fast path, the cadence retry the liveness proof, and neither is a polled `get`.
- Law: ownership is revision-guarded at both ends — a heartbeat that loses its revision stops renewing, and the scope finalizer issues `delete(name, { previousSeq })` at its latest revision, so a stolen or expired holder cannot purge its successor. A rejected heartbeat interrupts its fiber because two writers at one name is exactly what the revision guard exists to prevent. A release refusal is explicitly ignored because the server TTL remains the authoritative release fallback.
- Law: CAS is the write mode — `cas` compiles `Option.none()` to `create` and a held fact to `update(key, next, revision)`; the server rejects a stale revision and the engine folds it to `stale`, so the caller re-reads and re-folds; a blind `put` is not reachable through this engine.
- Law: reads are facts — `get` folds `null` and tombstone operations (`DEL`, `PURGE`) to `Option.none()`, a live entry to `{ value, revision }`; `watch` lifts the bucket's ordered iterator through `Stream.fromAsyncIterable` under a scoped bracket and projects the same fold, so the tail and the point read agree on one shape; `census` lifts `kv.keys(filter)` the same way — a bounded key enumeration, never a value scan.
- Law: bucket ensure is Layer construction — `kvm.create(bucket, { ttl, markerTTL })` at engine build from the root's bucket name: `ttl` arms the lease clock, `markerTTL` keeps removals notifying the watch tail; bucket shape never lives beside a call site, and the bucket is bounded coordination state whose history depth is a bucket option, never an audit log.
- Boundary: the connection is `pubsub#JETSTREAM_ROW`'s `Broker` — this engine never dials; the ordered watch iterator carries no ack surface, exactly as the fanout ordered lane.
- Packages: `@nats-io/kv` (`Kvm`, `KV`), `effect` (`Effect`, `Layer`, `Ref`, `Schedule`, `Stream`, `Random`, `Duration`), `./pubsub.ts` (`Broker`).

```typescript
const _LEASE = { ttl: Duration.seconds(30), beat: Duration.seconds(15) } as const

const _fact = (
  entry: { readonly value: Uint8Array; readonly revision: number; readonly operation: "PUT" | "DEL" | "PURGE" } | null,
): Option.Option<Accord.Fact> =>
  entry === null || entry.operation !== "PUT"
    ? Option.none()
    : Option.some(new _Fact({ value: entry.value, revision: entry.revision }))

const _kv = (bucket: string): Layer.Layer<Accord, AccordFault, Broker> =>
  Layer.scoped(
    Accord,
    Effect.gen(function* () {
      const nc = yield* Broker
      const kv: KV = yield* Effect.tryPromise({
        // ttl arms the server-clocked lease expiry; markerTTL keeps TTL removals notifying the watch tail
        try: () => new Kvm(nc).create(bucket, { ttl: Duration.toMillis(_LEASE.ttl), markerTTL: Duration.toMillis(_LEASE.ttl) }),
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

      const claimed = (name: string, id: Uint8Array): Effect.Effect<number, AccordFault> =>
        Effect.tryPromise({ try: () => kv.create(name, id), catch: () => new AccordFault({ reason: "busy", name }) })

      const evicted = (name: string): Effect.Effect<void, AccordFault> =>
        Effect.tryPromise({ try: () => kv.purge(name), catch: () => new AccordFault({ reason: "dial", name }) })

      const freed = (name: string, revision: number): Effect.Effect<void> =>
        Effect.tryPromise({
          try: () => kv.delete(name, { previousSeq: revision }),
          catch: () => new AccordFault({ reason: "dial", name }),
        }).pipe(Effect.catchTag("AccordFault", () => Effect.void))

      const parked = (name: string): Effect.Effect<void, AccordFault> =>
        Effect.asVoid(Stream.runHead(Stream.filter(tailed(name), Option.isNone)))

      const seated = (name: string, id: Uint8Array, revision: number): Effect.Effect<number, never, Scope.Scope> =>
        Effect.gen(function* () {
          const held = yield* Ref.make(revision)
          yield* Effect.addFinalizer(() => Effect.flatMap(Ref.get(held), (at) => freed(name, at)))
          yield* Effect.forkScoped(
            // The heartbeat resets the claim's TTL age at the tracked revision; a lost revision stops renewal, and the server clock expires the seat.
            Effect.repeat(
              Effect.flatMap(Ref.get(held), (at) =>
                Effect.flatMap(
                  Effect.tryPromise({
                    try: () => kv.update(name, id, at),
                    catch: () => new AccordFault({ reason: "stale", name }),
                  }),
                  (next) => Ref.set(held, next),
                )).pipe(Effect.catchTag("AccordFault", () => Effect.interrupt))),
              Schedule.spaced(_LEASE.beat),
            ),
          )
          return revision
        })

      const holding = (name: string, id: Uint8Array, mode: Accord.Mode): Effect.Effect<number, AccordFault> =>
        mode === "steal"
          ? Effect.zipRight(evicted(name), claimed(name, id))
          : mode === "try"
            ? claimed(name, id)
            : Effect.suspend(function attempt(): Effect.Effect<number, AccordFault> {
                // The tombstone tail is the fast path; the TTL-cadence retry covers a limit expiry the watch never notified.
                return Effect.catchIf(
                  claimed(name, id),
                  (fault) => fault.reason === "busy",
                  () => Effect.zipRight(Effect.race(parked(name), Effect.sleep(_LEASE.ttl)), Effect.suspend(attempt)),
                )
              })

      return {
        lease: (name, mode = "wait") =>
          Effect.gen(function* () {
            const id = yield* nonce
            const revision = yield* holding(name, id, mode)
            const token = yield* seated(name, id, revision)
            return new _Lease({ name, holder: new TextDecoder().decode(id), token: Option.some(token) })
          }),
        elect: (name) =>
          Effect.gen(function* () {
            const id = yield* nonce
            const seat = yield* claimed(name, id).pipe(
              Effect.map(Option.some),
              Effect.catchIf((fault) => fault.reason === "busy", () => Effect.succeed(Option.none<number>())),
            )
            return yield* Option.match(seat, {
              onNone: () => Effect.succeed<Accord.Seat>(new _Seat({ role: "follower", token: Option.none() })),
              onSome: (revision) =>
                Effect.map(seated(name, id, revision), (token): Accord.Seat => new _Seat({ role: "leader", token: Option.some(token) })),
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
            (revision) => new _Fact({ value: next, revision }),
          ),
        read: (key) =>
          Effect.map(
            Effect.tryPromise({ try: () => kv.get(key), catch: () => new AccordFault({ reason: "dial", name: key }) }),
            _fact,
          ),
        watch: tailed,
        census: (filter) =>
          Effect.flatMap(
            Effect.tryPromise({ try: () => kv.keys(filter), catch: () => new AccordFault({ reason: "dial", name: bucket }) }),
            (keys) => Stream.runCollect(Stream.fromAsyncIterable(keys, () => new AccordFault({ reason: "dial", name: bucket }))).pipe(
              Effect.map((held) => Array.from(held)),
            ),
          ),
      }
    }),
  )
```

## [04]-[LOCKS_ROW]

[LOCKS_ROW]:
- Owner: `Accord.locks()` — the browser engine over the origin's own lock arbiter. A lease bridges `navigator.locks.request(name, { mode: "exclusive", ifAvailable, steal }, grant)` to the scope: the grant callback settles a granted `Deferred` and then parks on a release `Deferred` the scope's finalizer resolves, so the platform holds the lock exactly as long as the scope lives and an orphaned hold is unspellable; a `try` miss (the callback receives `null`) folds to `busy`. `elect` is the `try` lease read as a seat — the arbiter's own queue is the succession order, so a follower simply re-elects when its own later request is granted. The receipt carries a tab-minted holder id and `Option.none()` for the token, because the arbiter mints no revision — a fenced write from this row is honestly unspellable.
- Law: the ledger members answer honestly — `cas`, `read`, and `watch` fold to the `ledger` fault because the arbiter holds no state; `census` answers truthfully from `navigator.locks.query()` — held and pending names filtered by prefix — so the doctor read works on both rows; a browser workload needing shared facts dials the `kv` row over websockets, and a session cell is `browser/persist`'s concern, never this port's.
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
        lease: (name, mode = "wait") =>
          Effect.as(held(name, mode), new _Lease({ name, holder: crypto.randomUUID(), token: Option.none<number>() })),
        elect: (name) =>
          held(name, "try").pipe(
            Effect.as<Accord.Seat>(new _Seat({ role: "leader", token: Option.none() })),
            Effect.catchIf(
              (fault) => fault.reason === "busy",
              () => Effect.succeed<Accord.Seat>(new _Seat({ role: "follower", token: Option.none() })),
            ),
          ),
        cas: (key) => Effect.fail(absent(key)),
        read: (key) => Effect.fail(absent(key)),
        watch: (key) => Stream.fail(absent(key)),
        census: (filter) =>
          Effect.map(
            Effect.tryPromise({
              try: () => navigator.locks.query(),
              catch: () => new AccordFault({ reason: "dial", name: filter ?? "" }),
            }),
            (snapshot) =>
              [...(snapshot.held ?? []), ...(snapshot.pending ?? [])]
                .flatMap((lock) => (lock.name === undefined ? [] : [lock.name]))
                .filter((name) => filter === undefined || name.startsWith(filter)),
          ),
      }
    })(),
  )

// --- [EXPORTS] --------------------------------------------------------------------------

export { Accord, AccordFault }
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
