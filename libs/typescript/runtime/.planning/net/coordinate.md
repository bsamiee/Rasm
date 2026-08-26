# [RUNTIME_COORDINATE]

Distributed coordination is one engine-blind port beside the fanout plane: `Accord` owns the mutual-exclusion lease, leader election, revision-guarded shared state, and the coordination census that keep many processes — and many tabs — agreeing without a second store. The engines are rows: the `kv` row rides `@nats-io/kv` revision-CAS over the same `Broker` connection the fanout engine holds — `create` is the claim mint, `update` at a read revision is the only write that can win a race, and every held claim lives under the bucket's own TTL clock refreshed by a scoped heartbeat, so a crashed holder expires by the server's clock and no name is ever permanently busy; the `locks` row rides the browser's own `navigator.locks` arbiter for cross-tab exclusion, where the ledger members honestly answer their absence. Every hold answers a `Lease` or `Seat` whose fencing token is the claim revision, so a downstream guarded write can prove seniority at the ledger and a stale holder's write loses structurally. Every read is a versioned fact — value plus revision — never a bare value, so compare-and-swap is spellable by construction and last-writer-wins is a deliberate row choice made elsewhere. A polled `get` waiting for absence, a hand lock file, a nonexpiring claim, a second dial beside `Broker`, and a fanout topic bent into a mutex are the named defects; the bucket is bounded coordination state, never the system of record. The module ships the `kv` row on the `./server` subpath; the `locks` row is the browser condition. The module is `runtime/src/net/coordinate.ts`.

## [01]-[INDEX]

- [02]-[PORT_SHAPE]: the engine-neutral port — lease, elect, cas, fence, read, watch, trail, census — the holds, the facts, and the faults; `Accord`, `AccordFault`.
- [03]-[KV_ROW]: the distributed engine: TTL-clocked claims, heartbeat holds, revision-CAS, watch tail; `Accord.kv`.
- [04]-[LOCKS_ROW]: the browser engine: Web Locks arbiter bridge, arbiter census, honest ledger degradation; `Accord.locks`.

## [02]-[PORT_SHAPE]

[PORT_SHAPE]:
- Owner: the `Accord` Tag — eight members over the coordination name. `lease(name, mode)` is the scoped exclusive hold answering an `Accord.Lease`: the scope opens holding the lock and closing it releases, `mode` selecting `wait` (suspend until granted), `try` (fail `busy` instantly), or `steal` (evict the holder — the operator's recovery arm); `elect(name)` is the scoped seat claim answering an `Accord.Seat` — a leader's seat stays alive for the scope's lifetime and a follower composes `watch` to react to succession; `cas(key, expected, next)` writes shared state only when the ledger still holds the expected fact (`Option.none()` is create-if-absent), answering the settled fact; `fence(key, lease, next)` is the same write under a holder's token, refusing a claimant the ledger has already outranked; `read(key, at?)` answers a fact as `Option`, the optional revision pinning which generation is read; `watch(key)` tails the fact's forward changes and `trail(key)` reads its recorded succession, both as the same `Option<Fact>` stream; `census(filter)` answers the coordinated names beside the engine's own health — the doctor read `serve/cli` ops verbs consume.
- Law: seniority is proved by CARRYING the token into the write, never by pinning a read — `fence(key, lease, next)` stores the holder's token beside the value under one length-framed layout and refuses `stale` when the ledger already holds a HIGHER one, so a slow holder whose lease expired loses structurally against its successor. `Lease.token` and a leader `Seat.token` hold the claim revision as `Option` (the `locks` arbiter mints none), and bucket revisions are one monotonic stream sequence, which is exactly what makes a plain comparison total. Consuming a lease as a bare grant with no fenced write is legal; minting a fenced write from anything but the hold's own token is not.
- Law: `read(key, at)` pins a generation of THAT KEY and answers absence for anything else — the bucket numbers every revision out of one stream sequence, and the point read re-checks the fetched message's key against the one asked for, so a revision belonging to a different key reads as `Option.none()` rather than a foreign fact. Holders therefore prove seniority through `fence` and never through a pinned read of the state key, which answers absence on every call.
- Law: succession is auditable, not inferred — `trail(key)` is the bucket's recorded history for one key, so a leadership handover reads as the sequence of claims and tombstones that produced it; the forward tail cannot answer a question about the past, and reconstructing one from a downstream projection re-derives evidence the bucket already holds under its own history depth.
- Law: the census answers health or honest absence — an engine holding a state ledger reports its own bucket facts (values, history depth, byte size, TTL) so an operator reads saturation and retention rather than a name list, and an engine holding none answers `Option.none()`, the same honest absence its hold token already carries; a zero-filled health record would publish a measurement no engine took.
- Law: the fault family is one reason-discriminated class — `dial` (the engine's transport is unreachable, class `unavailable`), `busy` (a `try` lease found the lock held, class `unavailable` — retryable by the caller's own schedule), `stale` (a CAS lost its race, class `conflicted` — re-read then re-fold, never blind retry), `ledger` (the engine carries no state ledger, class `absent` — the locks row's honest answer to `cas`/`read`/`watch`/`trail`) — so the core budget gate re-drives the transient rows and a lost CAS routes to a re-read.
- Law: contention and outage never share a reason — a write rejection is `busy` or `stale` only where the engine proves the race, and every other rejection is `dial`; folding both into the contention reason makes a broker outage read as a lost CAS a caller re-reads forever, and makes a dead broker read as a permanently held lock.
- Law: state is versioned facts — `Accord.Fact` is value plus revision; a caller that writes without a prior fact spells `Option.none()` and gets create-if-absent semantics, so an unguarded overwrite is unspellable through this port.
- Law: the port is engine-blind — no member names NATS or the Web Locks API; a per-app lease is a name prefix and `census(filter)` scopes the same way, so a surface change never follows a new namespace.
- Law: `_ENGINES` answers the consumption descriptor per engine as data — `fits`, `admit`, `tenancy`, `lifetime`, `degrade` — and the cells are where the two engines part: a bucket fences tenants by name prefix under one server clock, an origin's arbiter fences nothing beyond the origin and holds no clock at all, so one value repeated across both marks a row that stopped reading its engine; where an engine cannot express a coordinate its cell records the divergence on `degrade` rather than dropping the column.
- Entry: `yield* Accord` then the eight members; engines land as `Accord.kv(bucket, window)` / `Accord.locks()` root Layers.
- Packages: `effect` (`Context`, `Option`, `Schema`, `Stream`), `@rasm/core` (`Fault.Class`).

```typescript
import { Chunk, Context, Deferred, Duration, Effect, Layer, Option, Random, Ref, Schedule, Schema, type Scope, Stream } from "effect"
import { type KV, Kvm } from "@nats-io/kv"
import { JetStreamApiCodes, JetStreamApiError } from "@nats-io/jetstream"
import { Fault, type Identity } from "@rasm/core"
import { Broker } from "./pubsub.ts"

const _AccordSubject = Schema.Struct({ name: Schema.String })

const _family = Fault.Class.family(["dial", "busy", "stale", "ledger"] as const, {
  dial: Fault.Class.row({
    class: "unavailable",
    leg: "accord",
    detail: _AccordSubject,
    render: ({ name }) => `${name} is unreachable at the coordination engine`,
  }),
  busy: Fault.Class.row({
    class: "unavailable",
    leg: "accord",
    detail: _AccordSubject,
    render: ({ name }) => `${name} is held by another claimant`,
  }),
  stale: Fault.Class.row({
    class: "conflicted",
    leg: "accord",
    detail: _AccordSubject,
    render: ({ name }) => `${name} moved beneath the fact this write expected`,
  }),
  ledger: Fault.Class.row({
    class: "absent",
    leg: "accord",
    detail: _AccordSubject,
    render: ({ name }) => `${name} names no state ledger on this engine`,
  }),
})

declare namespace AccordFault {
  type Reason = (typeof _family.kinds)[number]
}

class AccordFault extends Schema.TaggedError<AccordFault>()("AccordFault", {
  case: _family.payload,
}) {
  get class(): Fault.Class.Kind {
    return _family.classOf(this.case.reason)
  }
  override get message(): string {
    return _family.render(this.case)
  }
}

declare namespace Accord {
  type Mode = "wait" | "try" | "steal"
  type Role = "leader" | "follower"
  type Engine = keyof typeof _ENGINES
  type Descriptor = {
    readonly fits: string
    readonly admit: string
    readonly tenancy: Identity.Tenancy
    readonly lifetime: string
    readonly degrade: string
  }
  type Seat = _Seat
  type Lease = _Lease
  type Window = _Window
  type Fact = _Fact
  type Health = _Health
  type Census = _Census
}

class _Window extends Schema.Class<_Window>("Accord/Window")({
  ttl: Schema.optionalWith(Schema.Duration, { default: () => Duration.seconds(30) }),
}) {
  get beat(): Duration.Duration {
    return Duration.times(this.ttl, 0.5)
  }
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

class _Health extends Schema.Class<_Health>("Accord/Health")({
  bucket: Schema.NonEmptyString,
  values: Schema.NonNegativeInt,
  depth: Schema.NonNegativeInt,
  bytes: Schema.NonNegativeInt,
  ttl: Schema.Duration,
}) {}

class _Census extends Schema.Class<_Census>("Accord/Census")({
  names: Schema.Array(Schema.NonEmptyString),
  health: Schema.optionalWith(_Health, { as: "Option" }),
}) {}

const _ENGINES = {
  kv: {
    fits: "<cross-process-claims-needing-server-clocked-expiry-and-a-fencing-token>",
    admit: "<Accord.kv(bucket)-riding-the-shared-Broker-connection;-this-engine-opens-no-second-dial>",
    tenancy: "multi",
    lifetime: "<the-bucket's-own-ttl-ends-a-hold;-a-half-ttl-heartbeat-renews-while-the-holder-lives>",
    degrade: "<bounded-coordination-state,-never-the-record-of-truth:-history-depth-is-a-bucket-option-that-ages-old-revisions-out,-and-a-name-prefix-is-the-only-tenant-fence-inside-one-bucket>",
  },
  locks: {
    fits: "<cross-tab-exclusion-inside-one-origin's-agent-cluster>",
    admit: "<Accord.locks()-over-the-host-navigator.locks-arbiter;-no-dial-and-no-configuration>",
    tenancy: "single",
    lifetime: "<host-owned-and-expiry-free:-context-teardown-by-the-agent-cluster-is-the-only-release-this-package-can-name>",
    degrade: "<no-ledger-at-all-—-cas,-fence,-read,-watch,-and-trail-fold-`ledger`,-the-hold-token-is-none-so-no-fenced-write-is-spellable,-census-health-is-none,-and-the-origin-is-an-isolation-no-prefix-widens>",
  },
} as const satisfies { readonly [Name: string]: Accord.Descriptor }

class Accord extends Context.Tag("runtime/Accord")<Accord, {
  readonly lease: (name: string, mode?: Accord.Mode) => Effect.Effect<Accord.Lease, AccordFault, Scope.Scope>
  readonly elect: (name: string) => Effect.Effect<Accord.Seat, AccordFault, Scope.Scope>
  readonly cas: (key: string, expected: Option.Option<Accord.Fact>, next: Uint8Array) => Effect.Effect<Accord.Fact, AccordFault>
  readonly fence: (key: string, lease: Accord.Lease | Accord.Seat, next: Uint8Array) => Effect.Effect<Accord.Fact, AccordFault>
  readonly read: (key: string, at?: number) => Effect.Effect<Option.Option<Accord.Fact>, AccordFault>
  readonly watch: (key: string) => Stream.Stream<Option.Option<Accord.Fact>, AccordFault>
  readonly trail: (key: string) => Stream.Stream<Option.Option<Accord.Fact>, AccordFault>
  readonly census: (filter?: string) => Effect.Effect<Accord.Census, AccordFault>
}>() {
  static readonly Fact = _Fact
  static readonly Health = _Health
  static readonly Census = _Census
  static readonly Lease = _Lease
  static readonly Seat = _Seat
  static readonly engines = _ENGINES
  static readonly Window = _Window
  static readonly kv = (bucket: string, window: Accord.Window = new _Window({})): Layer.Layer<Accord, AccordFault, Broker> => _kv(bucket, window)
  static readonly locks = (): Layer.Layer<Accord> => _locks()
}
```

## [03]-[KV_ROW]

[KV_ROW]:
- Owner: `Accord.kv(bucket)` — the distributed engine over one `Kvm(nc).create(bucket, { ttl })` bucket riding the shared `Broker` connection. Expiry is the bucket's clock, never a per-claim flag: the bucket-level `ttl` limit ages every message, so a claim not refreshed within the lease window vacates by the server's clock — holder loss can never leave a name permanently busy. Every winning claim — lease and seat alike — runs one `_seated` kernel: `create` mints the claim, a scope finalizer deletes only the revision it still owns, and a scoped half-TTL heartbeat `update`s at the tracked revision, resetting the message age while the holder lives; the hold's fencing token is the claim revision. `try` surfaces the claim conflict as `busy`; `steal` purges then claims; `wait` parks on the key's `watch` tail and re-claims when a tombstone lands, racing the park against a TTL-cadence re-claim because a limit-expired key can vacate without a watch notification — the event tail is the fast path, the cadence retry the liveness proof, and neither is a polled `get`.
- Law: ownership is revision-guarded at both ends — a heartbeat that loses its revision stops renewing, and the scope finalizer issues `delete(name, { previousSeq })` at its latest revision, so a stolen or expired holder cannot purge its successor. A REVISION-lost heartbeat interrupts its fiber because two writers at one name is exactly what the revision guard exists to prevent, while a transport-lost beat keeps trying: the server clock owns expiry, so surrendering a still-held claim over a blip hands the name away for nothing. A release refusal is explicitly ignored because the server TTL remains the authoritative release fallback.
- Law: every write's refusal reads its own evidence — one `_refused` projector folds the rejection through `JetStreamApiError.code` against the server's wrong-last-sequence pair, so `create` answers `busy` and `update` answers `stale` only where the server proved the race, and `dial` carries every transport failure; the claim, the CAS, and the heartbeat share that one discriminator, so an outage can never masquerade as contention on any of the three.
- Law: CAS is the write mode — `cas` compiles `Option.none()` to `create` and a held fact to `update(key, next, revision)`; the server rejects a stale revision and the engine folds it to `stale`, so the caller re-reads and re-folds; a blind `put` is not reachable through this engine.
- Law: `fence` needs BOTH guards and states why — the token guard refuses a holder the ledger already outranked, the revision guard refuses a concurrent write that landed between the read and this one. Checking the token alone lets two writes from one holder race; checking the revision alone lets an expired holder overwrite its successor, which is the exact failure a fencing token exists to make structural. Tokens ride an eight-byte big-endian prefix ahead of the value so seniority and payload settle in ONE guarded write, since a sibling key holding the token takes two writes the bucket cannot make atomic and a successor lands between them; absence and an unfenced value both read as token zero, which every live holder outranks. Holds carrying no token — the `locks` arbiter's — fold `ledger` rather than writing unfenced.
- Law: reads are facts — `get` folds `null` and tombstone operations (`DEL`, `PURGE`) to `Option.none()`, a live entry to `{ value, revision }`, and a supplied revision pins `get(key, { revision })` to that key's own generation, because the bucket numbers every revision out of ONE stream sequence and the point read re-checks the fetched message's key, so a revision belonging to another key answers absence rather than a foreign fact; `watch` and `trail` are one `iterated` bracket over two bucket iterators — the live tail and the recorded history — so the forward feed, the backward feed, and the point read agree on one shape and one teardown; `census` lifts `kv.keys(filter)` the same way and joins `kv.status()`, so a bounded key enumeration and the bucket's own facts land in one read, never a value scan.
- Law: bucket ensure is Layer construction — `kvm.create(bucket, { ttl, markerTTL })` at engine build from the root's bucket name and lease row: `ttl` arms the lease clock, `markerTTL` keeps removals notifying the watch tail; bucket shape never lives beside a call site, and the bucket is bounded coordination state whose history depth is a bucket option, never an audit log.
- Law: the lease window is a ROW the root supplies, never a literal — it must outlive the worst pause any holder takes between heartbeats, which is a deployment fact no library knows, and a frozen constant silently vacates every claim on a machine slower than the one it was written for. Its heartbeat cadence is DERIVED at half the window rather than carried beside it: two numbers admit a combination where the beat never lands inside the window, and one number cannot.
- Law: this row's cells in `_ENGINES` are the caller's recovery contract — the bucket `ttl` this engine sets bounds every hold, the server's own clock ends it, and a tenant fence is a name prefix inside one bucket, so recovery reasoning reads the row rather than the holder's own liveness.
- Boundary: the connection is `pubsub#JETSTREAM_ROW`'s `Broker` — this engine never dials; the ordered watch iterator carries no ack surface, exactly as the fanout ordered lane.
- Packages: `@nats-io/kv` (`Kvm`, `KV`), `@nats-io/jetstream` (`JetStreamApiCodes`, `JetStreamApiError`), `effect` (`Chunk`, `Duration`, `Effect`, `Layer`, `Random`, `Ref`, `Schedule`, `Stream`), `./pubsub.ts` (`Broker`).

```typescript
const _FENCE = { width: 8 } as const

const _fenced = (token: number, value: Uint8Array): Uint8Array => {
  const framed = new Uint8Array(_FENCE.width + value.byteLength)
  new DataView(framed.buffer).setBigUint64(0, BigInt(token))
  framed.set(value, _FENCE.width)
  return framed
}

const _held = (fact: Option.Option<Accord.Fact>): number =>
  Option.match(fact, {
    onNone: () => 0,
    onSome: ({ value }) =>
      value.byteLength < _FENCE.width
        ? 0
        : Number(new DataView(value.buffer, value.byteOffset, value.byteLength).getBigUint64(0)),
  })

const _fact = (
  entry: { readonly value: Uint8Array; readonly revision: number; readonly operation: "PUT" | "DEL" | "PURGE" } | null,
): Option.Option<Accord.Fact> =>
  entry === null || entry.operation !== "PUT"
    ? Option.none()
    : Option.some(new _Fact({ value: entry.value, revision: entry.revision }))

const _raced = (cause: unknown): boolean =>
  cause instanceof JetStreamApiError &&
  (cause.code === JetStreamApiCodes.StreamWrongLastSequence || cause.code === JetStreamApiCodes.StreamWrongLastSequenceUnknown)

const _refused = (name: string, lost: AccordFault.Reason) => (cause: unknown): AccordFault =>
  new AccordFault({ case: { reason: _raced(cause) ? lost : "dial", name } })

const _kv = (bucket: string, window: Accord.Window): Layer.Layer<Accord, AccordFault, Broker> =>
  Layer.scoped(
    Accord,
    Effect.gen(function* () {
      const nc = yield* Broker
      const kv: KV = yield* Effect.tryPromise({
        try: () => new Kvm(nc).create(bucket, { ttl: Duration.toMillis(window.ttl), markerTTL: Duration.toMillis(window.ttl) }),
        catch: () => new AccordFault({ case: { reason: "dial", name: bucket } }),
      })
      const nonce = Effect.map(Random.nextInt, (seed) => new TextEncoder().encode(seed.toString(36)))

      const iterated = (
        key: string,
        open: () => Promise<{ readonly stop: () => void } & AsyncIterable<Parameters<typeof _fact>[0]>>,
      ): Stream.Stream<Option.Option<Accord.Fact>, AccordFault> =>
        Stream.unwrapScoped(
          Effect.map(
            Effect.acquireRelease(
              Effect.tryPromise({ try: open, catch: () => new AccordFault({ case: { reason: "dial", name: key } }) }),
              (live) => Effect.sync(() => live.stop()),
            ),
            (iterator) =>
              Stream.map(
                Stream.fromAsyncIterable(iterator, () => new AccordFault({ case: { reason: "dial", name: key } })),
                _fact,
              ),
          ),
        )

      const tailed = (key: string): Stream.Stream<Option.Option<Accord.Fact>, AccordFault> =>
        iterated(key, () => kv.watch({ key }))

      const claimed = (name: string, id: Uint8Array): Effect.Effect<number, AccordFault> =>
        Effect.tryPromise({ try: () => kv.create(name, id), catch: _refused(name, "busy") })

      const evicted = (name: string): Effect.Effect<void, AccordFault> =>
        Effect.tryPromise({ try: () => kv.purge(name), catch: () => new AccordFault({ case: { reason: "dial", name } }) })

      const freed = (name: string, revision: number): Effect.Effect<void> =>
        Effect.tryPromise({
          try: () => kv.delete(name, { previousSeq: revision }),
          catch: () => new AccordFault({ case: { reason: "dial", name } }),
        }).pipe(Effect.catchTag("AccordFault", () => Effect.void))

      const parked = (name: string): Effect.Effect<void, AccordFault> =>
        Effect.asVoid(Stream.runHead(Stream.filter(tailed(name), Option.isNone)))

      const seated = (name: string, id: Uint8Array, revision: number): Effect.Effect<number, never, Scope.Scope> =>
        Effect.gen(function* () {
          const held = yield* Ref.make(revision)
          yield* Effect.addFinalizer(() => Effect.flatMap(Ref.get(held), (at) => freed(name, at)))
          yield* Effect.forkScoped(
            Effect.repeat(
              Effect.flatMap(Ref.get(held), (at) =>
                Effect.flatMap(
                  Effect.tryPromise({ try: () => kv.update(name, id, at), catch: _refused(name, "stale") }),
                  (next) => Ref.set(held, next),
                )).pipe(
                  Effect.catchIf((fault) => fault.case.reason === "stale", () => Effect.interrupt),
                  Effect.catchTag("AccordFault", () => Effect.void),
                ),
              Schedule.spaced(window.beat),
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
                return Effect.catchIf(
                  claimed(name, id),
                  (fault) => fault.case.reason === "busy",
                  () => Effect.zipRight(Effect.race(parked(name), Effect.sleep(window.ttl)), Effect.suspend(attempt)),
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
              Effect.catchIf((fault) => fault.case.reason === "busy", () => Effect.succeed(Option.none<number>())),
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
              onNone: () => Effect.tryPromise({ try: () => kv.create(key, next), catch: _refused(key, "stale") }),
              onSome: (fact) => Effect.tryPromise({ try: () => kv.update(key, next, fact.revision), catch: _refused(key, "stale") }),
            }),
            (revision) => new _Fact({ value: next, revision }),
          ),
        fence: (key, lease, next) =>
          Effect.flatMap(
            Option.match(lease.token, {
              onNone: () => Effect.fail(new AccordFault({ case: { reason: "ledger", name: key } })),
              onSome: Effect.succeed,
            }),
            (token) =>
              Effect.flatMap(
                Effect.map(
                  Effect.tryPromise({ try: () => kv.get(key), catch: () => new AccordFault({ case: { reason: "dial", name: key } }) }),
                  _fact,
                ),
                (current) =>
                  _held(current) > token
                    ? Effect.fail(new AccordFault({ case: { reason: "stale", name: key } }))
                    : Effect.map(
                        Option.match(current, {
                          onNone: () =>
                            Effect.tryPromise({ try: () => kv.create(key, _fenced(token, next)), catch: _refused(key, "stale") }),
                          onSome: (fact) =>
                            Effect.tryPromise({
                              try: () => kv.update(key, _fenced(token, next), fact.revision),
                              catch: _refused(key, "stale"),
                            }),
                        }),
                        (revision) => new _Fact({ value: _fenced(token, next), revision }),
                      ),
              ),
          ),
        read: (key, at) =>
          Effect.map(
            Effect.tryPromise({
              try: () => (at === undefined ? kv.get(key) : kv.get(key, { revision: at })),
              catch: () => new AccordFault({ case: { reason: "dial", name: key } }),
            }),
            _fact,
          ),
        watch: tailed,
        trail: (key) => iterated(key, () => kv.history({ key })),
        census: (filter) =>
          Effect.all({
            keys: Effect.flatMap(
              Effect.tryPromise({ try: () => kv.keys(filter), catch: () => new AccordFault({ case: { reason: "dial", name: bucket } }) }),
              (keys) =>
                Effect.map(
                  Stream.runCollect(Stream.fromAsyncIterable(keys, () => new AccordFault({ case: { reason: "dial", name: bucket } }))),
                  Chunk.toReadonlyArray,
                ),
            ),
            status: Effect.tryPromise({ try: () => kv.status(), catch: () => new AccordFault({ case: { reason: "dial", name: bucket } }) }),
          }).pipe(
            Effect.map(({ keys, status }) =>
              new _Census({
                names: keys,
                health: Option.some(
                  new _Health({
                    bucket: status.bucket,
                    values: status.values,
                    depth: status.history,
                    bytes: status.size,
                    ttl: Duration.millis(status.ttl),
                  }),
                ),
              })),
          ),
      }
    }),
  )
```

## [04]-[LOCKS_ROW]

[LOCKS_ROW]:
- Owner: `Accord.locks()` — the browser engine over the origin's own lock arbiter. A lease bridges `navigator.locks.request(name, { mode: "exclusive", ifAvailable, steal }, grant)` to the scope: the grant callback settles a granted `Deferred` and then parks on a release `Deferred` the scope's finalizer resolves, so the platform holds the lock exactly as long as the scope lives and an orphaned hold is unspellable; a `try` miss (the callback receives `null`) folds to `busy`. `elect` is the `try` lease read as a seat — the arbiter's own queue is the succession order, so a follower simply re-elects when its own later request is granted. The `Lease` carries a tab-minted holder id and `Option.none()` for the token, because the arbiter mints no revision — a fenced write from this row is honestly unspellable.
- Law: the ledger members answer honestly — `cas`, `fence`, `read`, `watch`, and `trail` fold to the `ledger` fault because the arbiter holds no state and records no history, exactly as this row's `_ENGINES` `degrade` cell declares; `census` still answers truthfully from `navigator.locks.query()` — held and pending names filtered by prefix, health `Option.none()` because no bucket exists to be healthy — so the doctor read works on both rows.
- Law: a workload this row's cells refuse dials the other engine — shared facts, a fencing token, or a claim vacating on a declared schedule are the `kv` row's over websockets, and a session cell is `browser/persist`'s concern, never this port's; the arbiter holds a name until the agent cluster tears its context down, so a wedged tab is the honest cost of the expiry-free lifetime.
- Law: the callback adapter is the platform-forced boundary — the grant callback runs `Effect.runPromise` over pure `Deferred` settles only (no capability, no domain logic crosses), the sanctioned bridge spelling. Exemption: the grant callback is the one statement kernel.
- Boundary: cross-tab exclusion only — the arbiter scopes to the origin's agent cluster; process-plane coordination is the `kv` row's.
- Packages: `effect` (`Deferred`, `Effect`, `Layer`), the host `navigator.locks` Web API at the sanctioned FFI boundary.

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
            : yield* new AccordFault({ case: { reason: "busy", name } })
        })
      const absent = (name: string) => new AccordFault({ case: { reason: "ledger", name } })
      return {
        lease: (name, mode = "wait") =>
          Effect.as(held(name, mode), new _Lease({ name, holder: crypto.randomUUID(), token: Option.none<number>() })),
        elect: (name) =>
          held(name, "try").pipe(
            Effect.as<Accord.Seat>(new _Seat({ role: "leader", token: Option.none() })),
            Effect.catchIf(
              (fault) => fault.case.reason === "busy",
              () => Effect.succeed<Accord.Seat>(new _Seat({ role: "follower", token: Option.none() })),
            ),
          ),
        cas: (key) => Effect.fail(absent(key)),
        fence: (key) => Effect.fail(absent(key)),
        read: (key) => Effect.fail(absent(key)),
        watch: (key) => Stream.fail(absent(key)),
        trail: (key) => Stream.fail(absent(key)),
        census: (filter) =>
          Effect.map(
            Effect.tryPromise({
              try: () => navigator.locks.query(),
              catch: () => new AccordFault({ case: { reason: "dial", name: filter ?? "" } }),
            }),
            (snapshot) =>
              new _Census({
                names: [...(snapshot.held ?? []), ...(snapshot.pending ?? [])]
                  .flatMap((lock) => (lock.name === undefined ? [] : [lock.name]))
                  .filter((name) => filter === undefined || name.startsWith(filter)),
                health: Option.none(),
              }),
          ),
      }
    })(),
  )

// --- [EXPORTS] -------------------------------------------------------------------------

export { Accord, AccordFault }
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
