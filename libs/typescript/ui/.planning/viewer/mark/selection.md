# [UI_SELECTION]

`viewer/mark/selection.ts` owns the `GlobalId` selection set (BM:67): one `HashSet<GlobalId>` atom written through a closed op vocabulary — `Replace`, `Add`, `Toggle`, `Subtract`, `Clear` — with every pick pipeline (deck async picking, three ray-casting, maplibre feature query, turf lasso) resolving its native hit into `GlobalId` values BEFORE the fold, so the set never holds a backend-shaped object. The set is the single truth every echo projects from: deck highlight, maplibre feature-state, table `RowSelectionState`, and the viewport reveal all read the same atom, and the whole fold is undoable by riding `atom/derive`'s `History`.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]        | [OWNS]                                                                  |
| :-----: | :--------------- | :--------------------------------------------------------------------------- |
|   [1]   | `SELECTION_FOLD` | the `GlobalId` set atom and the closed op vocabulary                          |
|   [2]   | `PICK_PIPES`     | native-hit → `GlobalId` resolution per backend, point and marquee and lasso   |
|   [3]   | `ECHO_ROWS`      | the projections — GPU highlight, feature-state, grid selection, reveal        |

## [2]-[SELECTION_FOLD]

- Owner: `Selection` — the op-driven fold: `Selection.Op` is a closed `Data.taggedEnum` and `Selection.step(set, op)` the total fold (`Match`-free — each arm is one `HashSet` combinator); the live atom is `History.make(HashSet.empty())` so undo/redo is construction, with writes minting `History.Op.Push` over the stepped set and the `present` projection feeding every consumer.
- Packages: `effect` (`HashSet`, `Data`), `@rasm/ts/wire/vocab` (`Bcf` — the `GlobalId` brand is wire-interior, so the element type derives as `_Viewpoint["selection"][number]`; the brand string is `Equal`-stable so set membership is structural), `atom/derive` (`History`).
- Law: ops are the only writes — a marquee, a click, a BCF viewpoint restore, and a table row toggle all mint `Selection.Op` values; no consumer holds a second set or mutates through any other path.
- Law: modality lives in the op value — click maps to `Toggle` (with modifier policy deciding `Replace` vs `Toggle` at the interaction row), marquee maps to `Add` or `Replace`, viewpoint restore maps to `Replace` — never a boolean knob on the fold.
- Growth: a new set behavior (invert, filter-to-visible) is one op case plus one fold arm.

```typescript
import type { Bcf } from "@rasm/ts/wire/vocab"
import { Data, HashSet, type Schema } from "effect"

type GlobalId = Schema.Schema.Type<typeof Bcf.Viewpoint>["selection"][number]

declare namespace Selection {
  type Set = HashSet.HashSet<GlobalId>
  type Decode = (raw: unknown) => Option.Option<GlobalId>
  type Op = Data.TaggedEnum<{
    Replace: { readonly ids: ReadonlyArray<GlobalId> }
    Add: { readonly ids: ReadonlyArray<GlobalId> }
    Toggle: { readonly id: GlobalId }
    Subtract: { readonly ids: ReadonlyArray<GlobalId> }
    Clear: {}
  }>
}

const _Op = Data.taggedEnum<Selection.Op>()

const _step = (set: Selection.Set, op: Selection.Op): Selection.Set =>
  _Op.$match(op, {
    Replace: ({ ids }) => HashSet.fromIterable(ids),
    Add: ({ ids }) => HashSet.union(set, HashSet.fromIterable(ids)),
    Toggle: ({ id }) => HashSet.toggle(set, id),
    Subtract: ({ ids }) => HashSet.difference(set, HashSet.fromIterable(ids)),
    Clear: () => HashSet.empty<GlobalId>(),
  })
```

## [3]-[PICK_PIPES]

- Law: every pipe resolves to `GlobalId` at its own seam through ONE app-wired `Selection.Decode` — the brand schema is `wire`-interior today, so the decoder value arrives as a parameter composed from `wire#vocab` and a locally-minted brand is the cross-language re-brand defect (RESEARCH: the decodable `GlobalId` export row on the `wire` vocab is the open cross-folder item); deck: `pickObjectsAsync({ x, y, width, height })` (the WebGPU-safe async pair; the deprecated sync mirrors never appear) yields `PickingInfo` whose `object` is a GeoArrow `StructRowProxy` or a feature — the pipe reads the id column/property and decodes; three: `Raycaster.setFromCamera` + `intersectObjects` over the residency graph, resolving hit `Object3D`s through the graft ledger's key mapping (mesh content key → element ids arrive with the manifest); maplibre: `queryRenderedFeatures(pointOrBox)` reading the feature's id property — one resolution law, three seams.
- Law: an unresolvable hit is absence, not a fault — a pick over empty space or a feature without the id property folds to `Option.none` and the op simply carries fewer ids; picking never mints faults.
- Law: lasso is planar compute — a freehand polygon hit-tests through `booleanPointInPolygon` against feature centroids with a `geojsonRbush` index making many-feature scenes sub-linear (`search(bbox)` prunes before the exact test); the index builds once per feature-set change, held in a derived atom.
- Boundary: the gesture that draws the marquee/lasso is `act/gesture`'s; the pixel→world math is `viewer/geo/project`'s; which surfaces are pickable is the owning layer's `pickable` row.

```typescript
import type { Deck, PickingInfo } from "@deck.gl/core"
import { Array, Effect, Option, Predicate } from "effect"

const _MARQUEE = { cap: 4096 } as const

const _fromInfo = (decode: Selection.Decode) =>
  (info: PickingInfo): Option.Option<GlobalId> =>
    Option.fromNullable(info.object).pipe(
      Option.filter(Predicate.hasProperty("globalId")),
      Option.flatMap((row) => decode(row.globalId)),
    )

const _marquee = (
  deck: Deck,
  decode: Selection.Decode,
  box: { readonly x: number; readonly y: number; readonly width: number; readonly height: number },
): Effect.Effect<ReadonlyArray<GlobalId>> =>
  Effect.map(
    Effect.promise(() => deck.pickObjectsAsync({ ...box, maxObjects: _MARQUEE.cap })),
    (hits) => Array.getSomes(Array.map(hits, _fromInfo(decode))),
  )
```

## [4]-[ECHO_ROWS]

- Law: echoes are projections, never stores — deck layers read the set through a `getFilterCategory`/`DataFilterExtension` category or a color accessor keyed by membership (with `updateTriggers` naming the set's version); maplibre echoes through `setFeatureState(feature, { selected })` diffed against the previous set (enter/leave sets computed by `HashSet.difference` both ways); the grid's `RowSelectionState` (`view/compose`) derives keyed by the same `GlobalId` strings and writes back through the same ops — one fold, many mirrors.
- Law: reveal is a camera intent — selecting from the table emits `Camera.Intent.FitBounds`/`EaseTo` over the selected features' bbox, and the grid scrolls its own echo through `scrollToIndex(align: "center")`; reveal never reaches into a map instance directly.
- Law: the selection count and id list surface through `view/primitive` rows (status text via `intl/message` plural forms; `announce` on large marquee results as polite SR feedback).

```typescript
import { HashSet } from "effect"

const _diff = (previous: Selection.Set, next: Selection.Set): {
  readonly entered: ReadonlyArray<GlobalId>
  readonly left: ReadonlyArray<GlobalId>
} => ({
  entered: [...HashSet.difference(next, previous)],
  left: [...HashSet.difference(previous, next)],
})

const Selection: {
  readonly Op: typeof _Op
  readonly step: typeof _step
  readonly diff: typeof _diff
  readonly fromInfo: typeof _fromInfo
  readonly marquee: typeof _marquee
} = {
  Op: _Op,
  step: _step,
  diff: _diff,
  fromInfo: _fromInfo,
  marquee: _marquee,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Selection }
```
