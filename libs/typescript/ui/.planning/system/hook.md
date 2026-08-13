# [UI_HOOK]

Hook owns the `rasm.ui.<domain>.<point>` fact rail. Each plane contributes one typed `Points` row and one runtime policy; `veto` consults selected pre-commit facts, `observe` fans live facts, and `replay` warms late taps from a bounded window. Per-app registries isolate channels and tap faults. Existing broadcasts enter as adopted sources, so owners publish once. Module: `ui/src/system/hook.ts`.

## [01]-[INDEX]

- [02]-[POINT_REGISTRY]: `Points` opens the contribution seam — per-plane row law and the initial point census; `Hook`.
- [03]-[RAIL_CHANNELS]: `Hook.registry` mints per-app channels — modality policy, replay windows, adopted sources; `Hook`.
- [04]-[FACT_PUBLISH]: `Hook.publish` folds one polymorphic publish over the veto arbiters; `Hook`.
- [05]-[TAP_ISOLATION]: scoped taps, the subscriber-fault channel, the telemetry-as-tap bridge law; `Hook`.

## [02]-[POINT_REGISTRY]

[POINT_REGISTRY]:
- Owner: `Points` — the one open interface of the folder: each owning plane contributes its point row from its own module through `declare module`, one row per contribution, so a new point is a new file's row and never a central-table edit; `Hook.Point` derives as `keyof Points`, per-point payload and modality project by indexed access, and the merged-whole guard re-validates every contribution at this declaration. Row shape: `{ modality, payload }` — `modality` a `Hook.Modality` literal, `payload` the fact value the point carries.
- Law: point names are `rasm.ui.<domain>.<point>` template literals — the pattern is the row-key contract the guard enforces, so a foreign-scoped or flat point name fails at the seam; the census below is the initial contribution set, and growth is one contributed row and one runtime row at composition.
- Law: this rail is the package-keyed hook-point namespace Tier-0's grammar carve exempts, so its `<domain>` segment answers to no `Convention._domain` row.
- Law: a span deliberately named for its point rides the carve with it, which is what makes a hook fact and its span correlate by name.
- Law: a metric name never rides this rail — a series resolves against `Convention._domain` and the vocabulary owner names its instrument row.
- Law: payload types cross strata type-only — a viewer plane contributes `Selection.Op` or a residency fact into `Points` through an erased augmentation without a value import upward; the runtime row arrives by registration at the composition root, never through a value edge into this floor module.
- Law: the census rows adopt the owners' standing facts — `rasm.ui.mark.op` carries mark's applied `Selection.Op` stream, `rasm.ui.scene.residency` carries the graft fold's arrival-and-refusal lanes, `rasm.ui.form.submit` consults veto arbiters before the mutation write, `rasm.ui.panel.egress` observes the control-sink egress records, `rasm.ui.overlay.present` observes overlay presentation and reason-keyed dismissal, `rasm.ui.vital.row` replays the vital plane's evidence rows, `rasm.ui.content.commit` observes each commit trip's settled stage, and `rasm.ui.canvas.solve` observes every layout proposal's applied-or-superseded admission.
- Law: a replay point carries evidence in both directions — `rasm.ui.vital.row` is the one browser-evidence window, so the app bridge republishes the runtime plane's graded vital rows onto it while `system/vital` and `viewer/probe` publish local rows, and a board mounted mid-session reads one retained source without this package importing a peer.
- Boundary: contribution mechanics are the registry merge seam — type-plane only; which facts an owner mints stays the owner's law (`viewer/mark`, `viewer/scene`, `view/form`, `viewer/panel`, `view/overlay`, `system/vital`, `view/content`, `view/canvas`), and this page owns only the rail they meet on.

| [INDEX] | [POINT]                   | [OWNER_FACT]                                                           | [MODALITY] | [DEPTH] |
| :-----: | :------------------------ | :--------------------------------------------------------------------- | :--------- | :------ |
|  [01]   | `rasm.ui.mark.op`         | applied `Selection.Op` (`viewer/mark` echo channel)                    | `replay`   | 64      |
|  [02]   | `rasm.ui.scene.residency` | graft arrival and refusal facts (`viewer/scene`)                       | `observe`  | 32      |
|  [03]   | `rasm.ui.form.submit`     | submit pre-flight and outcome (`view/form`)                            | `veto`     | 16      |
|  [04]   | `rasm.ui.panel.egress`    | control egress records (`viewer/panel`)                                | `observe`  | 32      |
|  [05]   | `rasm.ui.overlay.present` | overlay present and dismiss-with-reason (`view/overlay`)               | `observe`  | 16      |
|  [06]   | `rasm.ui.vital.row`       | browser evidence rows (`system/vital`, `viewer/probe`, the app bridge) | `replay`   | 128     |
|  [07]   | `rasm.ui.content.commit`  | commit settlement by bounded stage (`view/content`)                    | `observe`  | 16      |
|  [08]   | `rasm.ui.canvas.solve`    | layout-solve admission accounting (`view/canvas`)                      | `observe`  | 32      |

```typescript signature
interface Points {
}

declare namespace Hook {
  type Modality = Tap.Modality
  type Point = keyof Points
  type Name<P extends Point = Point> = P & Tap.Name
  type Payload<P extends Point> = Points[P]["payload"]
  type Handler<P extends Point, E = Tap.Fault> = Points[P]["modality"] extends infer M extends Tap.Modality
    ? Extract<Tap.Handler<Payload<P>, E>, { readonly _tag: M }>
    : never
  type _Rows<T extends Record<`rasm.ui.${string}.${string}`, { readonly modality: Hook.Modality; readonly payload: unknown }> = Points> = T // merged-whole guard: a malformed or foreign-named contribution fails here
}
```

## [03]-[RAIL_CHANNELS]

[RAIL_CHANNELS]:
- Owner: `Hook.registry(rows)` — the per-app mint: one scoped construction builds a channel per contributed point from its runtime row (`modality`, `depth`, optional adopted `source`, and a veto-row `consult` predicate), the fault channel beside them, and the veto gate cells; the registry dies with the composition scope, so channels, pumps, and taps release together and a second app mints its own value.
- Packages: `effect` (`Chunk`, `Effect`, `HashMap`, `Option`, `PubSub`, `Ref`, `Stream`); `@rasm/ts/core` (`Tap`).
- Law: modality selects the channel policy — `observe` and `veto` rows mint `PubSub.bounded(depth)`, while `replay` rows mint `PubSub.sliding({ capacity: depth, replay: depth })` so a late subscriber (a history capture, a probe board mounted mid-session) receives the retained window before live delivery; depth is the row's policy value, never a per-tap knob. `consult` on a veto row selects the pre-commit payloads arbiters refuse against, so settled facts on the same point always fan.
- Law: an owner that already publishes is adopted, never re-published — a row carrying `source` gets one scoped pump fiber draining the owner's stream into the row channel, so mark's retained `Selection.echoes` and scene's settled residency fact queue keep their single publish path and the registry is one more consumer under the owners' own laws.
- Law: the runtime rows record is annotation-governed — `Hook.Rows` demands one runtime row per contributed point, so a plane that contributes a type row and forgets its composition row breaks the app root loudly at the registry mint.
- Boundary: registration placement is the composition root's — this module exports the mint and never calls it; per-app scoping is the direct consequence of the mint living inside the app scope.

```typescript signature
import { Tap } from "@rasm/ts/core"
import { Chunk, Effect, HashMap, Option, PubSub, Ref, Schema, type Scope, Stream } from "effect"

declare namespace Hook {
  type Arbiter = {
    readonly token: symbol
    readonly gate: (payload: unknown) => Option.Option<Tap.Veto>
  }
  type Row<P extends Hook.Point> = P extends Hook.Point ? {
    readonly depth: number
    readonly source: Option.Option<Stream.Stream<Hook.Payload<P>>>
  } & (Points[P]["modality"] extends "veto"
    ? { readonly modality: "veto"; readonly consult: (payload: Hook.Payload<P>) => boolean }
    : { readonly modality: Points[P]["modality"] }) : never
  type Rows = { readonly [P in Hook.Point]: Hook.Row<P> } // one runtime row per contributed point: a missing row fails the mint at compile time
  type VetoPoint = { readonly [P in Hook.Point]: Points[P]["modality"] extends "veto" ? P : never }[Hook.Point]
  type Registry = {
    readonly channels: HashMap.HashMap<Hook.Point, PubSub.PubSub<unknown>>
    readonly gates: HashMap.HashMap<Hook.Point, Ref.Ref<Chunk.Chunk<Hook.Arbiter>>>
    readonly consults: HashMap.HashMap<Hook.Point, (payload: unknown) => boolean>
    readonly faults: PubSub.PubSub<Tap.Breach>
  }
}

const _FAULTS = { depth: 64 } as const

const _channel = (row: { readonly modality: Hook.Modality; readonly depth: number }): Effect.Effect<PubSub.PubSub<unknown>> =>
  Tap.Modality.at(row.modality).buffered
    ? PubSub.sliding({ capacity: row.depth, replay: row.depth })
    : PubSub.bounded(row.depth)

const _registry = (rows: Hook.Rows): Effect.Effect<Hook.Registry, never, Scope.Scope> =>
  Effect.gen(function* () {
    const faults = yield* PubSub.bounded<Tap.Breach>(_FAULTS.depth)
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
- Owner: `Hook.publish` returns `Option<Tap.Veto>`; absence admits and a veto suppresses channel delivery.
- Law: publishers fold veto evidence into their own fault rail; the registry carries no parallel refusal channel.
- Law: row depth bounds delivery, replay replaces the oldest retained fact, and transport booleans never substitute for veto evidence.

```typescript signature
const _publishRaw = (registry: Hook.Registry, point: Hook.Point, payload: unknown): Effect.Effect<Option.Option<Tap.Veto>> =>
  Effect.gen(function* () {
    const consulted = Option.match(HashMap.get(registry.consults, point), { onNone: () => false, onSome: (select) => select(payload) })
    const gates = consulted
      ? yield* Option.match(HashMap.get(registry.gates, point), {
          onNone: () => Effect.succeed(Chunk.empty<Hook.Arbiter>()),
          onSome: Ref.get,
        })
      : Chunk.empty<Hook.Arbiter>()
    const veto = Chunk.reduce(
      gates,
      Option.none<Tap.Veto>(),
      (held, registration) => Option.orElse(held, () => registration.gate(payload)),
    )
    return yield* Option.isNone(veto)
      ? Option.match(HashMap.get(registry.channels, point), {
          onNone: () => Effect.succeed(veto),
          onSome: (hub) => Effect.as(PubSub.publish(hub, payload), veto),
        })
      : Effect.succeed(veto)
  })

const _publish = <P extends Hook.Point>(registry: Hook.Registry, point: P, payload: Hook.Payload<P>): Effect.Effect<Option.Option<Tap.Veto>> =>
  _publishRaw(registry, point, payload)

const _veto = <P extends Hook.VetoPoint>(
  registry: Hook.Registry,
  point: P,
  handler: Extract<Hook.Handler<P>, { readonly _tag: "veto" }>,
): Effect.Effect<void, never, Scope.Scope> =>
  Option.match(HashMap.get(registry.gates, point), {
    onNone: () => Effect.void,
    onSome: (cell) =>
      Effect.acquireRelease(
        Effect.sync((): Hook.Arbiter => ({
          token: Symbol(point),
          gate: handler.handle as (payload: unknown) => Option.Option<Tap.Veto>,
        })).pipe(Effect.tap((registration) => Ref.update(cell, Chunk.append(registration)))),
        (registration) => Ref.update(cell, Chunk.filter((held) => held.token !== registration.token)),
      ).pipe(Effect.asVoid),
  })
```

## [05]-[TAP_ISOLATION]

[TAP_ISOLATION]:
- Packages: `effect` (`Cause`, `PubSub`, `Schema`, `Stream`); `@rasm/ts/core` (`Tap`).
- Law: telemetry is a tap — the app OTel bridge subscribes points and maps facts onto the branch observe combinators at the app plane; this library imports zero collector and mints zero instrument, so browser traces join the estate fabric the moment an app composes the bridge over the same rows probe already renders.
- Law: replay taps read history from the rail — a history capture or a probe board attaching mid-session receives the replay window before live facts, so evidence and undo lanes share one source of truth with live consumers and no owner replays state on demand.
- Boundary: the atom bridge (`system/atom#LIVE_BRIDGE`) binds any row a component must render — `Stream.fromPubSub` through `Atom.pull`, or an app-held `Subscribable` — and the component never subscribes a channel directly.

```typescript signature
const _observed = <P extends Hook.Point, E>(
  registry: Hook.Registry,
  point: P,
  label: string,
  handler: Exclude<Hook.Handler<P, E>, { readonly _tag: "veto" }>,
): Effect.Effect<void, never, Scope.Scope> =>
  Option.match(HashMap.get(registry.channels, point), {
    onNone: () => Effect.void,
    onSome: (hub) =>
      Effect.asVoid(
        Effect.forkScoped(
          Stream.runForEach(Stream.fromPubSub(hub), (payload) =>
            handler.handle(payload as Hook.Payload<P>).pipe(
              // BOUNDARY ADAPTER: the keyed channel proves the payload's point
              Effect.catchAllCause((cause) =>
                Option.match(Tap.isolated(Schema.decodeSync(Tap.schema)(point), label)(cause), {
                  onNone: () => Effect.void,
                  onSome: (breach) => Effect.asVoid(PubSub.publish(registry.faults, breach)),
                })),
            )),
        ),
      ),
  })

const _subscribe = <P extends Hook.Point, E>(
  registry: Hook.Registry,
  point: P,
  label: string,
  handler: Hook.Handler<P, E>,
): Effect.Effect<void, never, Scope.Scope> =>
  handler._tag === "veto"
    ? _veto(registry, point as Hook.VetoPoint, handler as Extract<Hook.Handler<Hook.VetoPoint>, { readonly _tag: "veto" }>)
    : _observed(registry, point, label, handler)

declare namespace Hook {
  type Shape = {
    readonly registry: typeof _registry
    readonly publish: typeof _publish
    readonly subscribe: typeof _subscribe
    readonly faults: (registry: Hook.Registry) => Stream.Stream<Tap.Breach>
  }
}

const Hook: Hook.Shape = {
  registry: _registry,
  publish: _publish,
  subscribe: _subscribe,
  faults: (registry) => Stream.fromPubSub(registry.faults),
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Hook }
export type { Points }
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
