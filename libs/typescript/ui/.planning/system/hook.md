# [UI_HOOK]

Hook is this folder's registrar on core's one hook rail. Each plane contributes one typed `Points` row and one runtime policy; `veto` consults selected pre-commit facts, `observe` fans live facts, and `replay` warms late taps from the point's own retained window. `Tap.Rail` owns channels, arbitration, seating, isolation, and the breach account; this page owns the point roster, the adopted sources, and the consult selector. Existing broadcasts enter as adopted sources, so owners publish once. Module: `ui/src/system/hook.ts`.

## [01]-[INDEX]

- [02]-[POINT_REGISTRY]: `Points` opens the contribution seam — per-plane row law and the initial point census; `Hook`.
- [03]-[RAIL_SEAT]: `Hook.registry` declares the folder's points and seats the app's `Tap.Rail` — runtime rows, adopted sources; `Hook`.
- [04]-[FACT_PUBLISH]: `Hook.publish` — the typed facade answering the closed `Tap.Verdict`; `Hook`.
- [05]-[TAP_MOUNT]: `Hook.seat` and `Hook.mount` — the consult-folded arbiter and the verdict-returning mount; `Hook`.

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
  type Text<P extends Point = Point> = P & Tap.Text
  type Payload<P extends Point> = Points[P]["payload"]
  type Handler<P extends Point, E = Tap.Fault> = Points[P]["modality"] extends infer M extends Tap.Modality
    ? Extract<Tap.Handler<Payload<P>, E>, { readonly _tag: M }>
    : never
  type _Rows<T extends Record<`rasm.ui.${string}.${string}`, { readonly modality: Hook.Modality; readonly payload: unknown }> = Points> = T
}
```

## [03]-[RAIL_SEAT]

[RAIL_SEAT]:
- Owner: `Hook.registry(app, rows, policy)` — the per-app mint: every contributed point becomes one `Tap.point` declaration off its runtime row (`modality`, `depth`), the app's rail seats on that roster, and each `source`-carrying row gets its scoped pump fiber; the seat dies with the composition scope, so channels, pumps, and seats release together and a second app mints its own value.
- Packages: `effect` (`Array`, `Effect`, `Either`, `Option`, `Record`, `Schema`, `Stream`); `@rasm/core` (`Identity`, `Tap`).
- Law: channels, arbitration, seating, isolation, and the breach account are `Tap.Rail`'s whole — this page declares points, adopts sources, and selects which facts arbitrate, and a folder-local hub table double-accounts every drop against a census nothing can add.
- Law: the runtime row IS the point's declaration — `modality` and `depth` are exactly what `Tap.point` admits, retention follows from the modality row alone, and no policy here re-derives a replay window.
- Law: the ui rail carries in-process facts, so a point binds `Schema.Unknown` and every payload type re-narrows at this page's own typed facades — the erased roster is what lets one rail seat eight unrelated payload families.
- Law: point admission accumulates onto ONE `Tap.Fault` and no partial seat lands — a refused row would otherwise leave a rail whose roster silently omits a point every publisher still names.
- Law: an owner that already publishes is adopted, never re-published — a row carrying `source` gets one scoped pump fiber draining the owner's stream onto the point, so mark's retained `Selection.echoes` and scene's settled residency queue keep their single publish path and the seat is one more consumer under the owners' own laws.
- Law: the runtime rows record is annotation-governed — `Hook.Rows` demands one runtime row per contributed point, so a plane that contributes a type row and forgets its composition row breaks the app root loudly at the mint.
- Boundary: registration placement is the composition root's — this module exports the mint and never calls it; per-app scoping is the direct consequence of the mint living inside the app scope.

```typescript signature
import { type Identity, Tap } from "@rasm/core"
import { Array, Effect, Either, Option, Record, Schema, type Scope, Stream } from "effect"

declare namespace Hook {
  type Row<P extends Hook.Point> = P extends Hook.Point ? {
    readonly depth: number
    readonly source: Option.Option<Stream.Stream<Hook.Payload<P>>>
  } & (Points[P]["modality"] extends "veto"
    ? { readonly modality: "veto"; readonly consult: (payload: Hook.Payload<P>) => boolean }
    : { readonly modality: Points[P]["modality"] }) : never
  type Rows = { readonly [P in Hook.Point]: Hook.Row<P> }
  type Roster = { readonly [P in Hook.Point]: Tap.Point<unknown> }
  type Registry = {
    readonly points: Hook.Roster
    readonly rail: Tap.Rail
    readonly rows: Hook.Rows
  }
}

const _registry = (
  app: Identity.App.Key,
  rows: Hook.Rows,
  policy: Tap.Policy,
): Effect.Effect<Either.Either<Hook.Registry, Tap.Fault>, never, Scope.Scope> =>
  Effect.gen(function* () {
    const entries = Object.entries(rows) as ReadonlyArray<readonly [Hook.Text, Hook.Row<Hook.Point>]>
    const [refused, minted] = Array.partitionMap(entries, ([point, row]) =>
      Either.map(
        Tap.point({ name: point, modalities: [row.modality], depth: row.depth }, Schema.Unknown),
        (declared) => [point, declared] as const,
      ))
    const issues = Array.flatMap(refused, (fault) => fault.issues)
    if (Array.isNonEmptyReadonlyArray(issues)) return Either.left(new Tap.Fault({ issues }))
    const points = Record.fromEntries(minted) as Hook.Roster
    const rail = yield* Tap.rail(app, Array.map(minted, ([, declared]) => declared), policy)
    yield* Effect.forEach(entries, ([point, row]) =>
      Option.match(row.source, {
        onNone: () => Effect.void,
        onSome: (source) =>
          Effect.asVoid(Effect.forkScoped(Stream.runForEach(source, (fact) => Tap.publish(rail, points[point], fact)))),
      }), { discard: true })
    return Either.right({ points, rail, rows })
  })
```

## [04]-[FACT_PUBLISH]

[FACT_PUBLISH]:
- Owner: `Hook.publish` — the typed facade over `Tap.publish`, binding the point's contributed payload type and answering the closed `Tap.Verdict`.
- Packages: `@rasm/core` (`Tap`).
- Law: publishers fold refusal evidence off the verdict's own `veto` — this page carries no parallel refusal channel, and a transport boolean never stands in for arbitration evidence.
- Law: the verdict is a value with three arms every publisher breaks on — `fanned` carries delivering arity and the point's census, `vetoed` the arbiter's refusal, `unrostered` a point no rail slot names.

```typescript signature
const _publish = <P extends Hook.Point>(
  registry: Hook.Registry,
  point: P,
  payload: Hook.Payload<P>,
): Effect.Effect<Tap.Verdict> => Tap.publish(registry.rail, registry.points[point], payload)
```

## [05]-[TAP_MOUNT]

[TAP_MOUNT]:
- Owner: `Hook.seat` and `Hook.mount` — the typed subscription bind and the one mount answering `Tap.Mount`, whose `mounted` arm carries the release handle and whose `refused` arm carries the accumulated admission fault.
- Packages: `effect` (`Either`, `Option`); `@rasm/core` (`Tap`).
- Law: `consult` is this folder's one arbitration axis and it folds INTO the seated arbiter — a veto point carries pre-commit AND settled facts while `Tap` arbitrates every fact a seated arbiter sees, so the selector answers absence on a settled fact rather than standing as a second gate beside the rail.
- Law: a mount answers its verdict and its release token — a `void` mount admits a duplicate label that double-counts every emitter and leaves the subscriber no detach.
- Law: admission and seating refuse on ONE arm — a modality mismatch and a duplicate label both read as `refused`, so a caller reads one verdict rather than an `Either` beside an effect.
- Law: subscriber faults never reach the publisher — `Tap` isolates each delivery and charges the breach onto its own accounted ring, which `Tap.breaches(registry.rail)` reads as a stream.
- Law: telemetry is a tap — the app OTel bridge mounts point subscriptions and maps facts onto the branch observe combinators at the app plane; this library imports zero collector and mints zero instrument, so browser traces join the estate fabric the moment an app composes the bridge over the same rows probe already renders.
- Law: replay taps read history from the rail — a history capture or a probe board attaching mid-session drains the point's own retained window before live facts, so evidence and undo lanes share one source of truth and no owner replays state on demand.
- Boundary: the atom bridge (`system/atom#LIVE_BRIDGE`) binds any row a component must render — an app-held `Subscribable` over a mounted observe seat — and the component never subscribes a channel directly.

```typescript signature
const _consulted = <P extends Hook.Point, E>(row: Hook.Row<P>, handler: Hook.Handler<P, E>): Tap.Handler<unknown, E> =>
  row.modality === "veto"
    ? Tap.Handler.veto({
      handle: (fact) =>
        row.consult(fact as Hook.Payload<P>)
          ? (handler as Extract<Tap.Handler<Hook.Payload<P>, E>, { readonly _tag: "veto" }>).handle(fact as Hook.Payload<P>)
          : Option.none(),
    })
    : handler as Tap.Handler<unknown, E>

const _seat = <P extends Hook.Point, E>(
  registry: Hook.Registry,
  point: P,
  handler: Hook.Handler<P, E>,
): Tap.Subscription<unknown, E> => Tap.subscription(registry.points[point], _consulted(registry.rows[point], handler))

const _mount = <T extends Record<string, Tap.Subscription<unknown, unknown>>>(
  registry: Hook.Registry,
  seats: T,
): Effect.Effect<Tap.Mount, never, Scope.Scope> =>
  Either.match(Tap.registry(registry.rail.app, seats), {
    onLeft: (fault) => Effect.succeed(Tap.Mount.refused({ fault })),
    onRight: (rows) => Tap.mount(registry.rail, rows),
  })

declare namespace Hook {
  type Shape = {
    readonly mount: typeof _mount
    readonly publish: typeof _publish
    readonly registry: typeof _registry
    readonly seat: typeof _seat
  }
}

const Hook: Hook.Shape = {
  mount: _mount,
  publish: _publish,
  registry: _registry,
  seat: _seat,
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Hook }
export type { Points }
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
