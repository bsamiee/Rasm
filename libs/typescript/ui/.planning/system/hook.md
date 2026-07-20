# [UI_HOOK]

The typed hook registry standing the `rasm.ui.<domain>.<point>` fact rail under every ui plane: domain facts publish once at their owning fold and every consumer — probe evidence, history capture, the app OTel bridge — subscribes a registry row, so no emit call ever scatters into owner code and no consumer mints a bespoke subscription protocol. Points are rows on ONE open registry seam each owning plane contributes type-only, modality is the row discriminant (`veto` consults arbiters before the owner commits, `observe` fans the fact, `replay` hands late subscribers the bounded window), subscriber faults isolate onto the registry's own fault channel without touching the publishing owner, and the registry value is minted per app composition — scoped, never process-global, so two apps never contend over one point. An owner that already broadcasts (mark's op echoes, scene's arrival lanes) is ADOPTED as a row source rather than re-published: the owner keeps its single publish path and the registry pumps the broadcast into the row channel. The module is `ui/src/system/hook.ts`.

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
- Law: point names are `rasm.ui.<domain>.<point>` template literals — the pattern is the row-key contract the guard enforces, so a foreign-scoped or flat point name fails at the seam; the census below is the initial contribution set, and growth is one contributed row plus one runtime row at composition.
- Law: payload types cross strata type-only — a viewer plane contributes `Selection.Op` or a residency fact into `Points` through an erased augmentation, which is legal exactly where a value import upward would not be; the runtime row arrives by registration at the composition root, never through a value edge into this floor module.
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
  // the one open interface: owning planes contribute rows via declare module, one row per module
  readonly "rasm.ui.vital.row": { readonly modality: "replay"; readonly payload: { readonly label: string; readonly value: number; readonly unit: string } }
}

declare namespace Hook {
  type Modality = "veto" | "observe" | "replay"
  type Point = keyof Points
  type Payload<P extends Point> = Points[P]["payload"]
  type _Rows<T extends Record<`rasm.ui.${string}.${string}`, { readonly modality: Hook.Modality; readonly payload: unknown }> = Points> = T // merged-whole guard: a malformed or foreign-named contribution fails here
}
```

```typescript
// an owning plane's contribution, authored in that plane's own module — type-plane only, so the strata DAG holds
declare module "./hook.ts" {
  interface Points {
    readonly "rasm.ui.mark.op": { readonly modality: "replay"; readonly payload: Selection.Op }
  }
}
```

## [03]-[RAIL_CHANNELS]

[RAIL_CHANNELS]:
- Owner: `Hook.registry(rows)` — the per-app mint: one scoped construction builds a channel per contributed point from its runtime row (`modality`, `depth`, optional adopted `source`), the fault channel beside them, and the veto gate cells; the registry dies with the composition scope, so channels, pumps, and taps release together and a second app mints its own value.
- Packages: `effect` (`Chunk`, `Effect`, `HashMap`, `Option`, `PubSub`, `Ref`, `Stream`).
- Law: modality selects the channel policy — `observe` and `veto` rows mint `PubSub.bounded(depth)`, `replay` rows mint `PubSub.bounded({ capacity: depth, replay: depth })` so a late subscriber (a history capture, a probe board mounted mid-session) receives the window before live delivery; depth is the row's policy value, never a per-tap knob.
- Law: an owner that already broadcasts is adopted, never re-published — a row carrying `source` gets one scoped pump fiber draining the owner's stream into the row channel, so mark's `Selection.echoes` and scene's arrival lanes keep their single publish path and the registry is one more broadcast consumer under the owners' own laws.
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
  type Row<P extends Hook.Point> = {
    readonly modality: Points[P]["modality"]
    readonly depth: number
    readonly source: Option.Option<Stream.Stream<Hook.Payload<P>>>
  }
  type Rows = { readonly [P in Hook.Point]: Hook.Row<P> } // one runtime row per contributed point: a missing row fails the mint at compile time
  type Gate<P extends Hook.Point> = (payload: Hook.Payload<P>) => Effect.Effect<boolean>
  type Registry = {
    readonly channels: HashMap.HashMap<Hook.Point, PubSub.PubSub<unknown>>
    readonly gates: HashMap.HashMap<Hook.Point, Ref.Ref<Chunk.Chunk<Hook.Gate<Hook.Point>>>>
    readonly faults: PubSub.PubSub<HookFault>
  }
}

const _FAULTS = { depth: 64 } as const

const _channel = (row: { readonly modality: Hook.Modality; readonly depth: number }): Effect.Effect<PubSub.PubSub<unknown>> =>
  _MODALITY[row.modality].replayed
    ? PubSub.bounded({ capacity: row.depth, replay: row.depth })
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
        Effect.map(Ref.make(Chunk.empty<Hook.Gate<Hook.Point>>()), (cell) => [point, cell] as const)),
    )
    yield* Effect.forEach(entries, ([point, row]) =>
      Option.match(row.source, {
        // adopted owner broadcast: one scoped pump per sourced row, the owner publishes exactly once
        onNone: () => Effect.void,
        onSome: (source) =>
          Effect.asVoid(Effect.forkScoped(Stream.runForEach(source, (fact) => _publishRaw({ channels, gates, faults }, point, fact)))),
      }), { discard: true })
    return { channels, gates, faults }
  })
```

## [04]-[FACT_PUBLISH]

[FACT_PUBLISH]:
- Owner: `Hook.publish(registry, point, payload)` — the one polymorphic publish: the payload type follows the point (`Hook.Payload<P>`), the veto fold runs first where the row's modality consults, and the returned verdict is `true` on admission; `observe` and `replay` points always admit, so only a veto point's publisher reads the verdict, and a refused publish emits nothing into the channel.
- Law: veto arbiters are registered gates, never inline predicates — `Hook.arbiter(registry, point, gate)` appends the gate under the composition scope and removes it on release; publish folds every gate and any `false` refuses, so the pre-flight decision is recoverable from the registered gate set, not from publisher-side branching.
- Law: the publisher folds refusal into its own plane — a refused `rasm.ui.form.submit` lands in the form error sink exactly like a validation failure; the registry carries no refusal channel because refusal is the publisher's evidence, not a rail fact.
- Law: publish is fire-and-bounded — channel capacity is the row's depth and the bounded `PubSub` suspends the publisher at saturation, so a slow tap backpressures instead of silently dropping facts; a point whose facts may shed under load declares that by depth, never by an unbounded channel.

```typescript
const _publishRaw = (registry: Hook.Registry, point: Hook.Point, payload: unknown): Effect.Effect<boolean> =>
  Effect.gen(function* () {
    const gates = yield* Option.match(HashMap.get(registry.gates, point), {
      onNone: () => Effect.succeed(Chunk.empty<Hook.Gate<Hook.Point>>()),
      onSome: Ref.get,
    })
    const verdicts = yield* Effect.forEach(gates, (gate) => gate(payload as Hook.Payload<Hook.Point>)) // BOUNDARY ADAPTER: the keyed read proves the payload's point
    const admitted = Chunk.every(Chunk.fromIterable(verdicts), (verdict) => verdict)
    return yield* admitted
      ? Option.match(HashMap.get(registry.channels, point), {
          onNone: () => Effect.succeed(false),
          onSome: (hub) => PubSub.publish(hub, payload),
        })
      : Effect.succeed(false)
  })

const _publish = <P extends Hook.Point>(registry: Hook.Registry, point: P, payload: Hook.Payload<P>): Effect.Effect<boolean> =>
  _publishRaw(registry, point, payload)

const _arbiter = <P extends Hook.Point>(registry: Hook.Registry, point: P, gate: Hook.Gate<P>): Effect.Effect<void, never, Scope.Scope> =>
  Option.match(HashMap.get(registry.gates, point), {
    onNone: () => Effect.void,
    onSome: (cell) =>
      Effect.acquireRelease(
        Ref.update(cell, Chunk.append(gate as Hook.Gate<Hook.Point>)),
        () => Ref.update(cell, Chunk.filter((held) => held !== gate)),
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
