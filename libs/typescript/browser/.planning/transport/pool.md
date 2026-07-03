# [BROWSER_POOL]

`transport/pool.ts` is the decode worker pool and the folder's kernel-delegating mint site (invariant 2): heavy byte work — frame reassembly, content-key verification, residency-manifest decode — runs off the main thread behind one closed `Schema.TaggedRequest` protocol over a serialized worker pool, with every band crossing zero-copy as a declared `Transferable`. The worker composes `wire`'s settled surfaces — `ArtifactFrame.reassembled` for the ordered fold-and-verify, `Residency.manifest` for the plan decode — and delegates its own cache-reverify mint to the one `kernel/identity` `contentKey`; a second content-address notion, a main-thread re-decode re-paying the offloaded cost, or an untyped `postMessage` arm is the named defect. `Depot` is the window-side residency scheduler: the ledger cell folded from `wire`'s manifest/delta protocol, receipts flipping `pending` to `resident`, and the budget-bounded haul that turns pending rows into verified octets — the surface the app composes into `ui`'s declared `GlbViewport` decode-worker port at the root, because `ui` and `browser` never import each other (seams `CO:69`, `AU:62`).

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]        | [OWNS]                                                              | [PUBLIC]           |
| :-----: | :--------------- | :------------------------------------------------------------------- | :----------------- |
|  [01]   | `WIRE_PROTOCOL`  | the request family, the serialized fault, the pool Tag and layer     | `Pool`, `PoolFault`|
|  [02]   | `DEPOT_SCHEDULER`| the residency ledger, the degree rows, the budget-bounded haul       | `Depot`            |
|  [03]   | `RUNNER_ENTRY`   | the worker-side boot module and its handler record                   | none               |

## [2]-[WIRE_PROTOCOL]

[WIRE_PROTOCOL]:
- Owner: `Pool` — the `Context.Tag` holding the `Worker.SerializedWorkerPool` over the closed request union, the request classes riding it as statics (`Pool.Assemble`: an artifact's frame bands in, the verified receipt plus octets out; `Pool.Verify`: cache-warmed octets re-proven against their declared key — the browser's own delegated mint site; `Pool.Chart`: residency-manifest bytes decoded off-thread), `Pool.protocol` (the union the runner checks its handlers against), and `Pool.layer(spawn, size)` (the pool layer over the app-supplied spawn factory — the worker script URL is app data, never a lib literal).
- Law: every band is a declared transfer — `Transferable.Uint8Array` on the `Assemble`/`Verify` payloads and on the `Assemble` success, so frames move to the worker and assembled octets move back with zero copies, and the marshal plan is recoverable from the request declarations alone.
- Law: `PoolFault` is `Schema.TaggedError` because it crosses the thread wire — reason rows `parity` (mint mismatch, both keys held as evidence), `sequence` (broken ordinal chain), `overrun` (band over the lane cap), `codec` (frame or manifest decode refusal) — reconstructing as the same tagged class on the window side so recovery dispatches one family; the worker folds `wire`'s `WireFault` evidence into these rows at the handler seam, never re-throwing it.
- Law: request identity is dedup identity — structural `Equal` over the payload fields is what collapses two identical `Verify` calls in one window; the fields carry exactly the coordinate, nothing incidental.
- Growth: a new offload concern is one request class plus one handler row — the union and the runner's compile-checked record break every site until both exist; a second pool per load profile restates what `Pool.layer`'s size row already carries.
- Boundary: the frame and manifest byte shapes are `wire`'s (`#vocab`); the mint is `kernel`'s; the pool executes, never re-models.
- Packages: `@effect/platform` (`Worker`, `Transferable`); `@effect/platform-browser` (`BrowserWorker`); `@rasm/ts/kernel` (`ContentKey`); `effect` (`Context`, `Layer`, `Schema`, `Option`).

```typescript
import { Transferable, Worker, type WorkerError } from "@effect/platform"
import { BrowserWorker } from "@effect/platform-browser"
import { ContentKey } from "@rasm/ts/kernel"
import { Residency } from "@rasm/ts/wire"
import { Array, Chunk, Context, Effect, HashMap, Layer, Option, Order, type ParseResult, Schema, Stream, SubscriptionRef } from "effect"
import { Kv } from "../persist/kv.ts"
import { Opfs } from "../persist/opfs.ts"

const PoolFaultPolicy = {
  parity: { rank: 5, retry: false },
  sequence: { rank: 3, retry: true },
  overrun: { rank: 4, retry: false },
  codec: { rank: 4, retry: false },
} as const

declare namespace PoolFault {
  type Reason = keyof typeof PoolFaultPolicy
  type Row = { readonly rank: number; readonly retry: boolean }
  type _Rows<T extends Record<Reason, Row> = typeof PoolFaultPolicy> = T
}

class PoolFault extends Schema.TaggedError<PoolFault>()("PoolFault", {
  reason: Schema.Literal("parity", "sequence", "overrun", "codec"),
  detail: Schema.String,
  evidence: Schema.optionalWith(Schema.Struct({ actual: Schema.String, expected: Schema.String }), { as: "Option" }),
}) {
  get policy(): PoolFault.Row {
    return PoolFaultPolicy[this.reason]
  }
}

const _Receipt = Schema.Struct({
  key: ContentKey,
  extent: Schema.Int.pipe(Schema.nonNegative()),
})

class Assemble extends Schema.TaggedRequest<Assemble>()("Assemble", {
  payload: { key: ContentKey, frames: Schema.Array(Transferable.Uint8Array) },
  success: Schema.Struct({ ..._Receipt.fields, octets: Transferable.Uint8Array }),
  failure: PoolFault,
}) {}

class Verify extends Schema.TaggedRequest<Verify>()("Verify", {
  payload: { key: ContentKey, octets: Transferable.Uint8Array },
  success: _Receipt,
  failure: PoolFault,
}) {}

class Chart extends Schema.TaggedRequest<Chart>()("Chart", {
  payload: { bytes: Transferable.Uint8Array },
  success: Residency.Manifest,
  failure: PoolFault,
}) {}

const _protocol = Schema.Union(Assemble, Verify, Chart)

class Pool extends Context.Tag("browser/transport/Pool")<Pool, Worker.SerializedWorkerPool<Assemble | Verify | Chart>>() {
  static readonly Assemble = Assemble
  static readonly Verify = Verify
  static readonly Chart = Chart
  static readonly protocol = _protocol
  static readonly layer = (
    spawn: (id: number) => globalThis.Worker,
    size: number,
  ): Layer.Layer<Pool, WorkerError.WorkerError> =>
    Worker.makePoolSerializedLayer(Pool, { size, concurrency: 1 }).pipe(Layer.provide(BrowserWorker.layer(spawn)))
}
```

## [3]-[DEPOT_SCHEDULER]

[DEPOT_SCHEDULER]:
- Owner: `Depot`, one scoped `Effect.Service` over `Pool`, `Opfs`, and `Kv` — `ledger`, the residency cell (`wire`'s `Residency.Ledger`); `opened(manifest)` replacing it (the C# render side owns truth, a fresh manifest resets); `evolve(delta)` applying `Residency.folded`; `landed(receipt)` flipping the receipt's row to `resident`; `plan`, the pending rows as fetch orders — worst-first by LOD then smallest-first by extent so coarse geometry lands early; `haul(pull)`, one budget-bounded pass turning the current plan into verified arrivals.
- Law: the degree is a verdict row — `_DEGREES` maps `Opfs`'s pressure verdict to the haul's concurrency (`ample: 6`, `tight: 2`, `critical: 1`, `opaque: 2`), so storage pressure throttles intake with zero per-call knobs and the byte-feed policy stays `transport/fetch`'s flow row.
- Law: warm before fetch — each order first probes `persist/kv`'s `cache` domain under its content key; a hit is admitted material and re-proves off-thread through `Verify` (the browser's delegated mint) before it lands, a parity refusal evicts the poisoned band and falls through to the fetch path, and a fresh assemble writes the cache best-effort — cache faults never fail an arrival, they only forfeit warmth.
- Law: the haul quarantines per artifact — `Effect.partition` keeps every fault beside every arrival, a `parity` refusal never blocks its siblings, and a landed receipt folds into the ledger inside the same pass so a repeated haul cannot re-order what already arrived; deltas replayed across reconnects are idempotent by `wire`'s own fold law.
- Law: one pass per call — `haul` drives the plan as it stands; the continuous loop is app composition (`ledger.changes` debounced into repeated hauls), so the lib owns the pass and the app owns the cadence.
- Receipt: `haul` yields `[faults, arrivals]` — arrivals as receipt-plus-octets pairs the app forwards into `ui`'s declared viewport port at the root; the ledger cell is the port's residency read.
- Boundary: manifest and delta ingress arrive decoded from `wire` (`Residency.stream` over `transport/fetch` feeds); the GLB scene semantics are `ui/viewer`'s; this owner schedules and joins.
- Packages: `effect` (`SubscriptionRef`, `HashMap`, `Array`, `Order`, `Effect`, `Stream`); `@rasm/ts/wire` (`Residency`); `../persist/opfs.ts` (`Opfs`); `../persist/kv.ts` (`Kv`).

```typescript
const _DEGREES = { ample: 6, tight: 2, critical: 1, opaque: 2 } as const

const _byOrder: Order.Order<Residency.Row> = Order.combine(
  Order.mapInput(Order.number, (row: Residency.Row) => row.lod),
  Order.mapInput(Order.number, (row: Residency.Row) => row.extent),
)

class Depot extends Effect.Service<Depot>()("browser/transport/Depot", {
  scoped: Effect.gen(function* () {
    const pool = yield* Pool
    const opfs = yield* Opfs
    const kv = yield* Kv
    const ledger = yield* SubscriptionRef.make(HashMap.empty<ContentKey, Residency.Row>())
    const plan: Effect.Effect<ReadonlyArray<Residency.Row>> = Effect.map(SubscriptionRef.get(ledger), (held) =>
      Array.sort(
        Array.filter(Array.fromIterable(HashMap.values(held)), (row) => row.state === "pending"),
        _byOrder,
      ),
    )
    const landed = (receipt: { readonly key: ContentKey; readonly extent: number }): Effect.Effect<void> =>
      SubscriptionRef.update(ledger, (held) =>
        HashMap.modifyAt(held, receipt.key, (slot) =>
          Option.map(slot, (row) => ({ ...row, extent: receipt.extent, state: "resident" as const })),
        ),
      )
    const haul = <E, R>(
      pull: (row: Residency.Row) => Stream.Stream<Uint8Array, E, R>,
    ): Effect.Effect<
      readonly [
        ReadonlyArray<E | PoolFault | WorkerError.WorkerError | ParseResult.ParseError>,
        ReadonlyArray<readonly [typeof _Receipt.Type, Uint8Array]>,
      ],
      never,
      R
    > =>
      Effect.gen(function* () {
        const orders = yield* plan
        const budget = yield* opfs.budget
        const _warmed = (order: Residency.Row) =>
          kv.read("cache", order.mesh).pipe(
            Effect.orElseSucceed(Option.none<Uint8Array>),
            Effect.flatMap(
              Option.match({
                onNone: () => Effect.succeedNone,
                onSome: (band) =>
                  pool.executeEffect(new Verify({ key: order.mesh, octets: band })).pipe(
                    Effect.map((receipt) => Option.some([receipt, band] as const)),
                    Effect.catchAll(() => Effect.as(Effect.ignore(kv.drop("cache", order.mesh)), Option.none())),
                  ),
              }),
            ),
          )
        const _hauledOne = (order: Residency.Row) =>
          Effect.flatMap(_warmed(order), (warm) =>
            Option.match(warm, {
              onSome: Effect.succeed,
              onNone: () =>
                Stream.runCollect(pull(order)).pipe(
                  Effect.flatMap((bands) =>
                    pool.executeEffect(new Assemble({ key: order.mesh, frames: Chunk.toReadonlyArray(bands) })),
                  ),
                  Effect.tap(({ key, octets }) => Effect.ignore(kv.write("cache", key, octets))),
                  Effect.map(({ extent, key, octets }) => [{ key, extent }, octets] as const),
                ),
            }),
          )
        return yield* Effect.partition(orders, (order) => Effect.tap(_hauledOne(order), ([receipt]) => landed(receipt)), {
          concurrency: _DEGREES[budget.verdict],
        })
      })
    return {
      ledger,
      plan,
      landed,
      haul,
      opened: (manifest: Residency.Manifest) => SubscriptionRef.set(ledger, Residency.opened(manifest)),
      evolve: (delta: Residency.Delta) => SubscriptionRef.update(ledger, (held) => Residency.folded(held, delta)),
    }
  }),
}) {}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Depot, Pool, PoolFault }
```

## [4]-[RUNNER_ENTRY]

[RUNNER_ENTRY]:
- Owner: the worker-side boot module — a separate entry the app's spawn factory names, composing `WorkerRunner.layerSerialized(Pool.protocol, handlers)` under `BrowserWorkerRunner.layer` and launching it as its whole life; the module exports nothing, the structural proof it is terminal. A worker thread is its own process under the boot-edge law, so its `runMain` is the thread's one boot and never a second main-thread boot.
- Law: the handler record is compile-checked against the union — `Assemble` decodes each band through `wire`'s `ArtifactFrame.frame` byte schema, drives the settled `ArtifactFrame.reassembled` fold (ordering, join, and the wire-site mint all inside `wire`'s law), demands exactly one completed artifact, and refuses a key mismatch with the declared key as evidence; `Verify` is the browser's delegated mint — `contentKey` over the presented octets compared to the declared key, `parity` with both keys on refusal; `Chart` decodes the manifest bytes through `Residency.manifest`.
- Law: fault folding is total at this seam — a `WireFault` from the wire fold maps reason-to-reason into `PoolFault` with its evidence stringified, and a `ParseError` folds to `codec`; the worker never throws across the boundary because the request's failure schema is the only exit.
- Law: the handlers run one request at a time per worker (`concurrency: 1` at the pool) because reassembly is memory-bound, and the pool's `size` row is the parallelism — worker count, not handler interleaving, scales throughput.
- Boundary: the spawn factory, the script URL, and the bundler wiring are app build material; this module is the script's content law.
- Packages: `@effect/platform` (`WorkerRunner`); `@effect/platform-browser` (`BrowserWorkerRunner`); `@rasm/ts/kernel` (`contentKey`); `@rasm/ts/wire` (`ArtifactFrame`, `Residency`).

```typescript
import { WorkerRunner } from "@effect/platform"
import { BrowserRuntime, BrowserWorkerRunner } from "@effect/platform-browser"
import { contentKey, type ContentKey } from "@rasm/ts/kernel"
import { ArtifactFrame, Residency, type WireFault } from "@rasm/ts/wire"
import { Chunk, Effect, Either, Layer, Option, Schema, Stream } from "effect"
import { Pool, PoolFault } from "./pool.ts"

const _folded = (fault: WireFault): PoolFault =>
  new PoolFault({
    reason: fault.reason === "parity" ? "parity" : fault.reason === "sequence" ? "sequence" : "overrun",
    detail: fault.detail,
    evidence: Option.map(fault.evidence, ({ actual, expected }) => ({ actual: String(actual), expected: String(expected) })),
  })

const _codec = (detail: string): PoolFault =>
  new PoolFault({ reason: "codec", detail, evidence: Option.none() })

const _assembled = (
  key: ContentKey,
  frames: ReadonlyArray<Uint8Array>,
): Effect.Effect<{ readonly key: ContentKey; readonly extent: number; readonly octets: Uint8Array }, PoolFault> =>
  Stream.fromIterable(frames).pipe(
    Stream.mapEffect((band) => Effect.mapError(Schema.decode(ArtifactFrame.frame)(band), (fault) => _codec(String(fault))), { concurrency: 1 }),
    ArtifactFrame.reassembled,
    Stream.runCollect,
    Effect.flatMap((emitted) =>
      Option.match(Chunk.head(emitted), {
        onNone: () => Effect.fail(_codec("<incomplete-artifact>")),
        onSome: Either.match({
          onLeft: (fault) => Effect.fail(_folded(fault)),
          onRight: ([artifact, octets]) =>
            artifact.key === key
              ? Effect.succeed({ key, extent: artifact.extent, octets })
              : Effect.fail(new PoolFault({ reason: "parity", detail: "<declared-key-mismatch>", evidence: Option.some({ actual: artifact.key, expected: key }) })),
        }),
      }),
    ),
  )

const _handlers = WorkerRunner.layerSerialized(Pool.protocol, {
  Assemble: ({ frames, key }) => _assembled(key, frames),
  Verify: ({ key, octets }) =>
    Effect.flatMap(contentKey(octets), (minted) =>
      minted === key
        ? Effect.succeed({ key, extent: octets.length })
        : Effect.fail(new PoolFault({ reason: "parity", detail: "<reverify-mismatch>", evidence: Option.some({ actual: minted, expected: key }) })),
    ),
  Chart: ({ bytes }) => Effect.mapError(Schema.decode(Residency.manifest)(bytes), (fault) => _codec(String(fault))),
})

BrowserRuntime.runMain(WorkerRunner.launch(Layer.provide(_handlers, BrowserWorkerRunner.layer)))

// --- [EXPORTS] --------------------------------------------------------------------------

export {}
```
