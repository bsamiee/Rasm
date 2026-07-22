# [UI_MARK]

Selection owns one `HashSet<GlobalId>` written through `Replace`, `Add`, `Toggle`, `Subtract`, and `Clear`. Every backend pick decodes to `GlobalId` before the fold, and every visual echo reads that set. BCF topics and viewpoints share the plane through pure pin, restore, and lifecycle projections; missing anchors remain evidence. Module: `ui/viewer/src/mark.ts`.

## [01]-[CLUSTERS]

| [INDEX] | [CLUSTER]           | [OWNS]                                                                    | [PUBLIC]    |
| :-----: | :------------------ | :------------------------------------------------------------------------ | :---------- |
|  [01]   | `SELECTION_FOLD`    | the `GlobalId` set atom, the closed op vocabulary, the History ride       | `Selection` |
|  [02]   | `PICK_PIPES`        | native-hit → `GlobalId` resolution per backend — point, marquee, lasso    | `Selection` |
|  [03]   | `ECHO_ROWS`         | the projections — GPU highlight, feature-state, grid selection, reveal    | `Selection` |
|  [04]   | `ANCHOR_PINS`       | topic pins — world anchors projected to screen, per-surface DOM anchoring | `Mark`      |
|  [05]   | `VIEWPOINT_RESTORE` | the viewpoint → camera-intent + selection-op fold with anchor evidence    | `Mark`      |
|  [06]   | `TOPIC_BOARD`       | lifecycle tone vocabularies, board rows, the write-egress boundary        | `Mark`      |

## [02]-[SELECTION_FOLD]

[SELECTION_FOLD]:
- Owner: `Selection` — the op-driven fold: `Selection.Op` is a closed `Data.taggedEnum` and `Selection.apply(set, op)` the effectful total fold, each arm one `HashSet` combinator and every admitted op published once through `Selection.Echoes`; the live atom is `History.make(HashSet.empty())` so undo/redo is construction — writes mint `History.Op.Push` over the applied set and the `present` projection feeds every consumer.
- Packages: `effect` (`HashSet`, `Data`, `Schema`); `@rasm/ts/core` (`BcfViewpoint` — `BcfViewpoint.GlobalId` is the one brand and decode surface; the brand string is `Equal`-stable so set membership is structural); `system/atom` (`History`).
- Law: ops are the only writes — a marquee, a click, a viewpoint restore, and a grid row toggle all mint `Selection.Op` values; no consumer holds a second set or mutates through any other path.
- Law: modality lives in the op value — click maps to `Toggle` (modifier policy deciding `Replace` versus `Toggle` at the interaction row), marquee maps to `Add` or `Replace`, viewpoint restore maps to `Replace` — never a boolean knob on the fold.
- Growth: a new set behavior (invert, filter-to-visible) is one op case and one fold arm.

```typescript
import { BcfViewpoint } from "@rasm/ts/core"
import { Data, HashSet, Option, Schema } from "effect"

type GlobalId = typeof BcfViewpoint.GlobalId.Type

declare namespace Selection {
  type Set = HashSet.HashSet<GlobalId>
  type Op = Data.TaggedEnum<{
    Replace: { readonly ids: ReadonlyArray<GlobalId> }
    Add: { readonly ids: ReadonlyArray<GlobalId> }
    Toggle: { readonly id: GlobalId }
    Subtract: { readonly ids: ReadonlyArray<GlobalId> }
    Clear: {}
  }>
}

const _Op = Data.taggedEnum<Selection.Op>()

const _decode: (raw: unknown) => Option.Option<GlobalId> = Schema.decodeUnknownOption(BcfViewpoint.GlobalId)

const _step = (set: Selection.Set, op: Selection.Op): Selection.Set =>
  _Op.$match(op, {
    Replace: ({ ids }) => HashSet.fromIterable(ids),
    Add: ({ ids }) => HashSet.union(set, HashSet.fromIterable(ids)),
    Toggle: ({ id }) => HashSet.toggle(set, id),
    Subtract: ({ ids }) => HashSet.difference(set, HashSet.fromIterable(ids)),
    Clear: () => HashSet.empty<GlobalId>(),
  })
```

## [03]-[PICK_PIPES]

[PICK_PIPES]:
- Law: every pipe resolves to `GlobalId` at its own seam through the one `_decode` — deck: `pickObjectsAsync({ x, y, width, height, maxObjects })` (the WebGPU-safe async pair; the deprecated sync mirrors never appear) yields `PickingInfo` whose `object` is a GeoArrow row proxy or a feature, and the pipe reads the id member and decodes; three: `Raycaster.setFromCamera` + `intersectObjects` over the residency graph, resolving hit nodes through `scene`'s graft ledger to content keys, then key → `GlobalId` through the element index the app composes from the decoded `ElementGraph`; maplibre: `queryRenderedFeatures(pointOrBox)` reading the feature's id property — one resolution law, three seams.
- Law: an unresolvable hit is absence, not a fault — a pick over empty space or a feature without the id member folds to `Option.none` and the op simply carries fewer ids; picking never mints faults.
- Law: the three-arm's key→`GlobalId` resolution batches — a marquee over a dense residency yields hundreds of content keys, so the element-index lookup rides one `Request.Class` request family under `RequestResolver.makeBatched` — N hits, one index traversal, de-duplicated by construction; a per-hit lookup loop is the named defect.
- Law: lasso is planar compute with a scale ladder — a freehand polygon hit-tests through `booleanPointInPolygon` against feature centroids with a `geojsonRbush` index making many-feature scenes sub-linear (`search(bbox)` prunes before the exact test); the index builds once per feature-set change, held in a derived atom.
- Packages: `@deck.gl/core` (`Deck`, `PickingInfo`); `three` (`Raycaster`); `maplibre-gl` (`queryRenderedFeatures`); `@turf/turf` (`booleanPointInPolygon`, `geojsonRbush`); `effect` (`Request`, `RequestResolver`).
- Boundary: the gesture drawing the marquee/lasso is `system/act#CONTINUOUS_OWNER`'s; pixel→world math is `geo#PROJECT`'s; which surfaces are pickable is the owning layer row's toggle.
- Growth: a million-feature lasso graduates to the GPU fold — a `typegpu` centroid-in-polygon kernel over a `d.arrayOf` centroid buffer adopting the scene-published device (`scene#BACKEND_SELECT`'s compute seam); the CPU rbush ladder stays the floor, and the kernel is one growth row, never a second lasso vocabulary.

```typescript
import type { Deck, PickingInfo } from "@deck.gl/core"
import { Array, Effect, Predicate, Request, RequestResolver } from "effect"

const _MARQUEE = { cap: 4096 } as const

class _ResolveId extends Request.Class<Option.Option<GlobalId>, never, { readonly key: string }> {}

const _resolveIds = (index: (keys: ReadonlyArray<string>) => ReadonlyArray<Option.Option<GlobalId>>) =>
  RequestResolver.makeBatched((requests: Array.NonEmptyArray<_ResolveId>) =>
    Effect.forEach(
      Array.zip(requests, index(Array.map(requests, (request) => request.key))),
      ([request, resolved]) => Request.succeed(request, resolved),
      { discard: true },
    ))

const _fromInfo = (info: PickingInfo): Option.Option<GlobalId> =>
  Option.fromNullable(info.object).pipe(
    Option.filter(Predicate.hasProperty("globalId")),
    Option.flatMap((row) => _decode(row.globalId)),
  )

const _marquee = (
  deck: Deck,
  box: { readonly x: number; readonly y: number; readonly width: number; readonly height: number },
): Effect.Effect<ReadonlyArray<GlobalId>> =>
  Effect.map(
    Effect.promise(() => deck.pickObjectsAsync({ ...box, maxObjects: _MARQUEE.cap })),
    (hits) => Array.getSomes(Array.map(hits, _fromInfo)),
  )
```

## [04]-[ECHO_ROWS]

[ECHO_ROWS]:
- Law: echoes are projections, never stores — deck layers read the set through a `DataFilterExtension` category or a color accessor keyed by membership (with `updateTriggers` naming the set's version); the batched scene arm flips `setVisibleAt`/tint rows through `scene#DRAW_COLLAPSE`; maplibre echoes through `setFeatureState(feature, { selected })` diffed against the previous set (enter/leave computed by `HashSet.difference` both ways); the grid's row selection (`view/table`) derives keyed by the same `GlobalId` strings and writes back through the same ops — one fold, many mirrors.
- Law: reveal is a camera intent — selecting from the grid emits `Camera.Intent.FitBounds`/`EaseTo` over the selected features' bbox; reveal never reaches into a map instance directly.
- Law: the selection count and id list surface through `system/primitive` rows — status text via `Message` plural forms, `announce` as polite SR feedback on large marquee results.
- Law: non-view echo consumers subscribe through one bounded replay channel — `Selection.echoes` is the memoized `Layer` constructing `PubSub.sliding<Selection.Op>({ capacity, replay })` once per app, and `Selection.Echoes` is the shared service every publisher and subscriber yields; late and live wire egress, probe evidence, and sibling mirrors consume the retained operation window without touching the atom registry, saturation replaces the oldest retained operation, and a second subscription protocol is the named defect.
- Law: `Selection.Echoes` is the adopted source behind the `rasm.ui.mark.op` hook point (`system/hook`, replay modality) — this page contributes the point row and `Selection.hook` yields the shared service, so the registry pumps this one channel and no second op publisher exists.

```typescript
import { Context, Effect, HashSet, Layer, Option, PubSub, Stream } from "effect"
import { Hook } from "../../src/system/hook.ts"

declare module "../../src/system/hook.ts" {
  interface Points {
    readonly "rasm.ui.mark.op": { readonly modality: "replay"; readonly payload: Selection.Op }
  }
}

class _Echoes extends Context.Tag("rasm.ui.mark.echoes")<_Echoes, PubSub.PubSub<Selection.Op>>() {}

const _echoes = Layer.effect(_Echoes, PubSub.sliding<Selection.Op>({ capacity: 64, replay: 64 }))

const _apply = Effect.fn("Selection.apply")(function* (set: Selection.Set, op: Selection.Op) {
  const next = _step(set, op)
  const echoes = yield* _Echoes
  yield* PubSub.publish(echoes, op)
  return next
})

const _hook: Effect.Effect<Hook.Row<"rasm.ui.mark.op">, never, _Echoes> = Effect.map(
  _Echoes,
  (echoes) => ({
    modality: "replay",
    depth: 64,
    source: Option.some(Stream.fromPubSub(echoes)),
  }),
)

const _diff = (previous: Selection.Set, next: Selection.Set): {
  readonly entered: ReadonlyArray<GlobalId>
  readonly left: ReadonlyArray<GlobalId>
} => ({
  entered: [...HashSet.difference(next, previous)],
  left: [...HashSet.difference(previous, next)],
})

declare namespace Selection {
  type Shape = {
    readonly Echoes: typeof _Echoes
    readonly Op: typeof _Op
    readonly decode: typeof _decode
    readonly apply: typeof _apply
    readonly diff: typeof _diff
    readonly echoes: typeof _echoes
    readonly hook: typeof _hook
    readonly resolveIds: typeof _resolveIds
    readonly fromInfo: typeof _fromInfo
    readonly marquee: typeof _marquee
  }
}

const Selection: Selection.Shape = {
  Echoes: _Echoes,
  Op: _Op,
  decode: _decode,
  apply: _apply,
  diff: _diff,
  echoes: _echoes,
  hook: _hook,
  resolveIds: _resolveIds,
  fromInfo: _fromInfo,
  marquee: _marquee,
}
```

## [05]-[ANCHOR_PINS]

[ANCHOR_PINS]:
- Owner: `Mark.pin` — the pin projection: each open topic's primary viewpoint yields a world anchor (the viewpoint camera target, or the first resolvable selection element's centroid), projected per camera settle through `Camera.anchor` (the pure viewport math on geo surfaces) or the live `map.project` seam; pins render as DOM anchors — a maplibre `Marker` on map surfaces, a floating-ui `VirtualElement` whose `getBoundingClientRect` wraps the projected point on scene surfaces — one pin mechanism per surface class, chosen by the surface row, never stacked.
- Packages: `@rasm/ts/core` (`BcfTopic`, `BcfViewpoint`); `maplibre-gl` (`Marker`); `@floating-ui/react` (`VirtualElement`); `geo` (`Camera.anchor`).
- Law: pins are projections of decoded topics — pin identity is the topic guid, a pin's screen position derives per camera settle from the anchor, and no pin holds its own position state. Look-at target anchors the pin: the eye is where the reviewer stood, never what the topic marks.
- Law: pin glyph and tone key off the lifecycle vocabulary — the `[7]` tone table is the single styling source; a status conditional in a pin row marks the table unused.
- Law: `<model-viewer>` surfaces anchor through the element's own ray — `positionAndNormalFromPoint` mints new anchors on authoring gestures, `updateHotspot`/`queryHotspot` carry pins as element hotspots — the embed adapter row, same vocabulary.
- Boundary: pin press interaction rides `system/act#DISCRETE_ROWS`; rich topic text sanitizes through `system/primitive`'s gate before any DOM sink.

```typescript
import type { BcfTopic, BcfViewpoint } from "@rasm/ts/core"
import { Option } from "effect"

declare namespace Mark {
  type Pin = {
    readonly guid: string
    readonly anchor: readonly [number, number]
    readonly status: BcfTopic["status"]
  }
}

const _target = (camera: BcfViewpoint["camera"]): readonly [number, number, number] => [
  camera.position[0] + camera.direction[0],
  camera.position[1] + camera.direction[1],
  camera.position[2] + camera.direction[2],
]

const _pin = (
  topic: BcfTopic,
  viewpoint: Option.Option<BcfViewpoint>,
  project: (world: readonly [number, number, number]) => readonly [number, number],
): Option.Option<Mark.Pin> =>
  Option.map(viewpoint, (held) => ({
    guid: topic.guid,
    anchor: project(_target(held.camera)),
    status: topic.status,
  }))
```

## [06]-[VIEWPOINT_RESTORE]

[VIEWPOINT_RESTORE]:
- Owner: `Mark.restore(viewpoint, resident, millis)` — one fold, two outputs and one receipt: the camera block (position/direction/up/fieldOfView — consume-only carriage per the wire law) mints one `Camera.Intent.LookAt` — eye from the position rows, target from position and direction, the ease duration as the caller's policy — that every surface class dispatches through `Camera.drive`; the `selection` array mints the existing `Selection.Op.Replace` case directly; and the anchor receipt reports which ids resolved against the live model — the partial-failure evidence the operator reads.
- Law: restore never re-derives — no view geometry computes beyond coordinate adaptation; the viewpoint IS the proof, and a restore that corrects the camera is the drift defect.
- Law: the receipt is data — `{ requested, resolved, missing }` counts and the missing id list; it renders as an evidence row (`Message` plural forms), never throws, and a fully-missing selection still restores the camera.
- Boundary: which elements are resident is `scene`'s graft ledger fact; intent dispatch is `geo#CAMERA`'s; the selection fold is `[2]`'s.

```typescript
import { Array, HashSet, pipe } from "effect"
import { Camera } from "./geo.ts"

declare namespace Restore {
  type Receipt = {
    readonly requested: number
    readonly resolved: number
    readonly missing: ReadonlyArray<GlobalId>
  }
}

const _restore = (
  viewpoint: BcfViewpoint,
  resident: HashSet.HashSet<GlobalId>,
  millis: number,
): { readonly intent: Camera.Intent; readonly op: Selection.Op; readonly receipt: Restore.Receipt } =>
  pipe(
    Array.partition(viewpoint.selection, (id) => HashSet.has(resident, id)),
    ([missing, resolved]) => ({
      intent: Camera.Intent.LookAt({
        eye: viewpoint.camera.position,
        target: _target(viewpoint.camera),
        millis,
      }),
      op: _Op.Replace({ ids: resolved }),
      receipt: { requested: viewpoint.selection.length, resolved: resolved.length, missing },
    }),
  )
```

## [07]-[TOPIC_BOARD]

[TOPIC_BOARD]:
- Owner: `Mark.tone` — the lifecycle styling vocabulary: one `as const` table keyed by the wire's closed `status` axis carrying tone and glyph rows (a `LucideIcon` per status — icon-as-identity), with `priority` as the second axis feeding a recipe variant; the board renders topics as roster-law list rows (`system/primitive#ROSTER_LAW`), comment threads at full depth from the decoded topic, and stamps through `Format.instant` (`system/intl`).
- Packages: `lucide-react` (the glyph rows); `@rasm/ts/core` (`BcfTopic`).
- Law: the vocabularies are closed AT THE WIRE — the tables key off `BcfTopic["status"]`/`["priority"]` so a wire vocabulary change breaks these rows loudly at compile time; a locally-widened status is the named defect.
- Law: writes are egress, not state — a comment draft or status change is an app action encoded at the wire; this module renders decoded truth and exposes intent callbacks, holding no authored BCF value.
- Growth: a new lifecycle presentation is one tone-table row; a new topic facet (labels, due dates — already carried on the decoded class) is one board row, zero shape changes.

```typescript
import type { LucideIcon } from "lucide-react"
import { CircleAlert, CircleCheck, CircleDot, CircleSlash } from "lucide-react"

const _tone = {
  open: { icon: CircleDot, tone: "accent" },
  "in-progress": { icon: CircleAlert, tone: "accent" },
  resolved: { icon: CircleCheck, tone: "success" },
  closed: { icon: CircleSlash, tone: "neutral" },
} as const satisfies Record<BcfTopic["status"], { readonly icon: LucideIcon; readonly tone: "neutral" | "accent" | "success" | "danger" }>

declare namespace Mark {
  type Shape = {
    readonly pin: typeof _pin
    readonly restore: typeof _restore
    readonly target: typeof _target
    readonly tone: typeof _tone
  }
}

const Mark: Mark.Shape = {
  pin: _pin,
  restore: _restore,
  target: _target,
  tone: _tone,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Mark, Selection }
```

## [08]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
