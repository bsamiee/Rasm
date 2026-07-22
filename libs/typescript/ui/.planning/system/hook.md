# [UI_HOOK]

Hook owns the `rasm.ui.<domain>.<point>` fact rail. Each plane contributes one typed `Points` row and one runtime policy; `veto` consults selected pre-commit facts, `observe` fans live facts, and `replay` warms late taps from a bounded window. Per-app registries isolate channels and tap faults. Existing broadcasts enter as adopted sources, so owners publish once. Module: `ui/src/system/hook.ts`.

## [01]-[CLUSTERS]

| [INDEX] | [CLUSTER]        | [OWNS]                                                                         | [PUBLIC] |
| :-----: | :--------------- | :----------------------------------------------------------------------------- | :------- |
|  [01]   | `POINT_REGISTRY` | the open point seam, the contribution law, the initial point census            | `Hook`   |
|  [02]   | `RAIL_CHANNELS`  | the per-app registry mint — modality channels, replay windows, adopted sources | `Hook`   |
|  [03]   | `FACT_PUBLISH`   | the one polymorphic publish and the veto-arbiter fold                          | `Hook`   |
|  [04]   | `TAP_ISOLATION`  | scoped taps, the subscriber-fault channel, the telemetry-as-tap bridge law     | `Hook`   |

## [02]-[POINT_REGISTRY]

[POINT_REGISTRY]:
- Owner: `Points` — the one open interface of the folder: each owning plane contributes its point row from its own module through `declare module`, one row per contribution, so a new point is a new file's row and never a central-table edit; `Hook.Point` derives as `keyof Points`, per-point payload and modality project by indexed access, and the merged-whole guard re-validates every contribution at this declaration. Row shape: `{ modality, payload }` — `modality` a `Hook.Modality` literal, `payload` the fact value the point carries.
- Law: point names are `rasm.ui.<domain>.<point>` template literals — the pattern is the row-key contract the guard enforces, so a foreign-scoped or flat point name fails at the seam; the census below is the initial contribution set, and growth is one contributed row and one runtime row at composition.
- Law: payload types cross strata type-only — a viewer plane contributes `Selection.Op` or a residency fact into `Points` through an erased augmentation without a value import upward; the runtime row arrives by registration at the composition root, never through a value edge into this floor module.
- Law: the census rows adopt the owners' standing facts — `rasm.ui.mark.op` carries mark's applied `Selection.Op` stream, `rasm.ui.scene.residency` carries the graft fold's arrival-and-refusal lanes, `rasm.ui.form.submit` consults veto arbiters before the mutation write, `rasm.ui.panel.egress` observes the control-sink egress records, `rasm.ui.overlay.present` observes overlay presentation and reason-keyed dismissal, `rasm.ui.vital.row` replays the vital plane's evidence rows.
- Boundary: contribution mechanics are the registry merge seam — type-plane only; which facts an owner mints stays the owner's law (`viewer/mark`, `viewer/scene`, `view/form`, `viewer/panel`, `view/overlay`, `system/vital`), and this page owns only the rail they meet on.

| [INDEX] | [POINT]                   | [OWNER_FACT]                                             | [MODALITY] | [DEPTH] |
| :-----: | :------------------------ | :------------------------------------------------------- | :--------- | :------ |
|  [01]   | `rasm.ui.mark.op`         | applied `Selection.Op` (`viewer/mark` echo channel)      | `replay`   | 64      |
|  [02]   | `rasm.ui.scene.residency` | graft arrival and refusal facts (`viewer/scene`)         | `observe`  | 32      |
|  [03]   | `rasm.ui.form.submit`     | submit pre-flight and outcome (`view/form`)              | `veto`     | 16      |
|  [04]   | `rasm.ui.panel.egress`    | control egress records (`viewer/panel`)                  | `observe`  | 32      |
|  [05]   | `rasm.ui.overlay.present` | overlay present and dismiss-with-reason (`view/overlay`) | `observe`  | 16      |
|  [06]   | `rasm.ui.vital.row`       | performance evidence rows (`system/vital`)               | `replay`   | 128     |

```typescript
interface Points {
}

declare namespace Hook {
  type Modality = "veto" | "observe" | "replay"
  type Point = keyof Points
  type Payload<P extends Point> = Points[P]["payload"]
  type _Rows<T extends Record<`rasm.ui.${string}.${string}`, { readonly modality: Hook.Modality; readonly payload: unknown }> = Points> = T // merged-whole guard: a malformed or foreign-named contribution fails here
}
```

## [03]-[RAIL_CHANNELS]

[RAIL_CHANNELS]:
- Owner: `Hook.registry(rows)` — the per-app mint: one scoped construction builds a channel per contributed point from its runtime row (`modality`, `depth`, optional adopted `source`, and a veto-row `consult` predicate), the fault channel beside them, and the veto gate cells; the registry dies with the composition scope, so channels, pumps, and taps release together and a second app mints its own value.
- Packages: `effect` (`Chunk`, `Effect`, `HashMap`, `Option`, `PubSub`, `Ref`, `Stream`).
- Law: modality selects the channel policy — `observe` and `veto` rows mint `PubSub.bounded(depth)`, while `replay` rows mint `PubSub.sliding({ capacity: depth, replay: depth })` so a late subscriber (a history capture, a probe board mounted mid-session) receives the retained window before live delivery; depth is the row's policy value, never a per-tap knob. A veto row's `consult` predicate selects the pre-commit payloads arbiters may refuse, so settled facts on the same point always fan.
- Law: an owner that already publishes is adopted, never re-published — a row carrying `source` gets one scoped pump fiber draining the owner's stream into the row channel, so mark's retained `Selection.echoes` and scene's settled residency fact queue keep their single publish path and the registry is one more consumer under the owners' own laws.
- Law: the runtime rows record is annotation-governed — `Hook.Rows` demands one runtime row per contributed point, so a plane that contributes a type row and forgets its composition row breaks the app root loudly at the registry mint.
- Boundary: registration placement is the composition root's — this module exports the mint and never calls it; per-app scoping is the direct consequence of the mint living inside the app scope.

```typescript
import { Chunk, Effect, HashMap, Option, PubSub, Ref, type Scope, Stream } from "effect"

const _MODALITY = {
  veto: { consulted: true, replayed: false },
  observe: { consulted: false, replayed: false },
  replay: { consulted: false, replayed: true },
} as const

declare namespace Hook {
  type Arbiter = {
    readonly token: symbol
    readonly gate: (payload: unknown) => Effect.Effect<boolean>
  }
  type Row<P extends Hook.Point> = P extends Hook.Point ? {
    readonly depth: number
    readonly source: Option.Option<Stream.Stream<Hook.Payload<P>>>
  } & (Points[P]["modality"] extends "veto"
    ? { readonly modality: "veto"; readonly consult: (payload: Hook.Payload<P>) => boolean }
    : { readonly modality: Points[P]["modality"] }) : never
  type Rows = { readonly [P in Hook.Point]: Hook.Row<P> } // one runtime row per contributed point: a missing row fails the mint at compile time
  type VetoPoint = { readonly [P in Hook.Point]: Points[P]["modality"] extends "veto" ? P : never }[Hook.Point]
  type Gate<P extends Hook.VetoPoint> = (payload: Hook.Payload<P>) => Effect.Effect<boolean>
  type Registry = {
    readonly channels: HashMap.HashMap<Hook.Point, PubSub.PubSub<unknown>>
    readonly gates: HashMap.HashMap<Hook.Point, Ref.Ref<Chunk.Chunk<Hook.Arbiter>>>
    readonly consults: HashMap.HashMap<Hook.Point, (payload: unknown) => boolean>
    readonly faults: PubSub.PubSub<HookFault>
  }
}

const _FAULTS = { depth: 64 } as const

const _channel = (row: { readonly modality: Hook.Modality; readonly depth: number }): Effect.Effect<PubSub.PubSub<unknown>> =>
  _MODALITY[row.modality].replayed
    ? PubSub.sliding({ capacity: row.depth, replay: row.depth })
    : PubSub.bounded(row.depth)

const _registry = (rows: Hook.Rows): Effect.Effect<Hook.Registry, never, Scope.Scope> =>
  Effect.gen(function* () {
    const faults = yield* PubSub.bounded<HookFault>(_FAULTS.depth)
    const entries = Object.entries(rows) as ReadonlyArray<readonly [Hook.Point, Hook.Row<Hook.Point>]> // BOUNDARY ADAPTER: the mapped record erases to entry pairs once at the mint
    const channels = HashMap.fromIterable(
      yield* Effect.forEach(entries, ([point, row]) => Effect.map(_channel(row), (hub) => [point, hub] as const)),
    )
    const gates = HashMap.fromIterable(
      yield* Effect.forEach(entries, ([point]) =>
        Effect.map(Ref.make(Chunk.empty<Hook.Arbiter>()), (cell) => [point, cell] as const)),
    )
    const consults = HashMap.fromIterable(entries.map(([point, row]) => [point, row.modality === "veto"
      ? (payload: unknown) => row.consult(payload as never)
      : () => false] as const)) // BOUNDARY ADAPTER: the mapped row's modality proves the veto payload
    yield* Effect.forEach(entries, ([point, row]) =>
      Option.match(row.source, {
        // adopted owner broadcast: one scoped pump per sourced row, the owner publishes exactly once
        onNone: () => Effect.void,
        onSome: (source) =>
          Effect.asVoid(Effect.forkScoped(Stream.runForEach(source, (fact) => _publishRaw({ channels, gates, consults, faults }, point, fact)))),
      }), { discard: true })
    return { channels, gates, consults, faults }
  })
```

## [04]-[FACT_PUBLISH]

[FACT_PUBLISH]:
- Owner: `Hook.publish(registry, point, payload)` — the one polymorphic publish: the payload type follows the point (`Hook.Payload<P>`), the veto fold runs first where the veto row's `consult` predicate admits the payload to arbitration, and the returned verdict is `true` on admission; non-consulted payloads always admit, so only a pre-commit publisher reads the verdict, and a refused publish emits nothing into the channel.
- Law: veto arbiters are registered gates, never inline predicates — `Hook.arbiter(registry, point, gate)` appends one tokenized registration under the composition scope and removes exactly that token on release, so repeated acquisition of the same stable gate remains independently leased. Publish folds every gate and any `false` refuses, so the pre-flight decision is recoverable from the registered gate set, not from publisher-side branching.
- Law: the publisher folds refusal into its own plane — a refused `rasm.ui.form.submit` lands in the form error sink exactly like a validation failure; the registry carries no refusal channel because refusal is the publisher's evidence, not a rail fact.
- Law: publish is bounded — channel capacity is the row's depth; `observe` and `veto` publishers suspend at saturation, while `replay` replaces the oldest retained fact under its declared window. `PubSub.publish` reports delivery, not arbitration, so `Hook.publish` discards that transport boolean and returns the independently settled veto verdict.

```typescript
const _publishRaw = (registry: Hook.Registry, point: Hook.Point, payload: unknown): Effect.Effect<boolean> =>
  Effect.gen(function* () {
    const consulted = Option.match(HashMap.get(registry.consults, point), { onNone: () => false, onSome: (select) => select(payload) })
    const gates = consulted
      ? yield* Option.match(HashMap.get(registry.gates, point), {
          onNone: () => Effect.succeed(Chunk.empty<Hook.Arbiter>()),
          onSome: Ref.get,
        })
      : Chunk.empty<Hook.Arbiter>()
    const verdicts = yield* Effect.forEach(gates, (registration) => registration.gate(payload))
    const admitted = Chunk.every(Chunk.fromIterable(verdicts), (verdict) => verdict)
    return yield* admitted
      ? Option.match(HashMap.get(registry.channels, point), {
          onNone: () => Effect.succeed(true),
          onSome: (hub) => Effect.as(PubSub.publish(hub, payload), true),
        })
      : Effect.succeed(false)
  })

const _publish = <P extends Hook.Point>(registry: Hook.Registry, point: P, payload: Hook.Payload<P>): Effect.Effect<boolean> =>
  _publishRaw(registry, point, payload)

const _arbiter = <P extends Hook.VetoPoint>(registry: Hook.Registry, point: P, gate: Hook.Gate<P>): Effect.Effect<void, never, Scope.Scope> =>
  Option.match(HashMap.get(registry.gates, point), {
    onNone: () => Effect.void,
    onSome: (cell) =>
      Effect.acquireRelease(
        Effect.sync((): Hook.Arbiter => ({
          token: Symbol(point),
          gate: gate as (payload: unknown) => Effect.Effect<boolean>,
        })).pipe(Effect.tap((registration) => Ref.update(cell, Chunk.append(registration)))),
        (registration) => Ref.update(cell, Chunk.filter((held) => held.token !== registration.token)),
      ).pipe(Effect.asVoid),
  })
```

## [05]-[TAP_ISOLATION]

[TAP_ISOLATION]:
- Owner: `Hook.tap(registry, point, label, handler)` — the scoped tap: one forked drain per tap consumes the row channel, runs the handler per fact, and folds any handler cause into a `HookFault` row on the registry's fault channel; the drain survives its own faults, the publishing owner never observes them, and the tap fiber dies with the composition scope.
- Law: a tap fault is evidence, never propagation — `HookFault` carries the point, the tap label, and the pretty-printed cause; the fault channel is itself tappable (the probe board renders tap health as evidence rows), and a fault that must gate anything is a veto arbiter, not an observe tap.
- Law: telemetry is a tap — the app OTel bridge subscribes points and maps facts onto the branch observe combinators at the app plane; this library imports zero collector and mints zero instrument, so browser traces join the estate fabric the moment an app composes the bridge over the same rows probe already renders.
- Law: replay taps read history from the rail — a history capture or a probe board attaching mid-session receives the replay window before live facts, so evidence and undo lanes share one source of truth with live consumers and no owner replays state on demand.
- Boundary: the atom bridge (`system/atom#LIVE_BRIDGE`) binds any row a component must render — `Stream.fromPubSub` through `Atom.pull`, or an app-held `Subscribable` — and the component never subscribes a channel directly.

```typescript
import { Cause, Data } from "effect"

class HookFault extends Data.TaggedError("HookFault")<{
  readonly point: Hook.Point
  readonly tap: string
  readonly detail: string
}> {}

const _tap = <P extends Hook.Point, E>(
  registry: Hook.Registry,
  point: P,
  label: string,
  handler: (payload: Hook.Payload<P>) => Effect.Effect<void, E>,
): Effect.Effect<void, never, Scope.Scope> =>
  Option.match(HashMap.get(registry.channels, point), {
    onNone: () => Effect.void,
    onSome: (hub) =>
      Effect.asVoid(
        Effect.forkScoped(
          Stream.runForEach(Stream.fromPubSub(hub), (payload) =>
            handler(payload as Hook.Payload<P>).pipe(
              // BOUNDARY ADAPTER: the keyed channel proves the payload's point
              Effect.catchAllCause((cause) =>
                Effect.asVoid(PubSub.publish(registry.faults, new HookFault({ point, tap: label, detail: Cause.pretty(cause) })))),
            )),
        ),
      ),
  })

declare namespace Hook {
  type Shape = {
    readonly modality: typeof _MODALITY
    readonly registry: typeof _registry
    readonly publish: typeof _publish
    readonly arbiter: typeof _arbiter
    readonly tap: typeof _tap
    readonly faults: (registry: Hook.Registry) => Stream.Stream<HookFault>
  }
}

const Hook: Hook.Shape = {
  modality: _MODALITY,
  registry: _registry,
  publish: _publish,
  arbiter: _arbiter,
  tap: _tap,
  faults: (registry) => Stream.fromPubSub(registry.faults),
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Hook, HookFault }
export type { Points }
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
