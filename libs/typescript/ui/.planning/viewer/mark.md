# [UI_MARK]

Selection owns one `HashSet<GlobalId>` written through `Replace`, `Add`, `Toggle`, `Subtract`, and `Clear`. Every seam — deck pick, 3D-tile batch, accelerated scene marquee, indexed lasso, basemap query — mints one value of a closed three-arm `Hit` vocabulary, and one batched fold resolves the whole set to `GlobalId` before any op; every visual echo reads the resulting set. BCF topics and viewpoints share the plane through pure pin, restore, board, and lifecycle projections — a pin anchors in the viewpoint's own world frame while each surface class answers its own placement, the board folds decoded topics against one supplied instant, and every write leaves as an intent; missing anchors remain evidence. Module: `ui/viewer/src/mark.ts`.

## [01]-[INDEX]

- [02]-[SELECTION_FOLD]: `GlobalId` set atom, closed op vocabulary, History ride; `Selection`.
- [03]-[PICK_PIPES]: closed hit vocabulary and one batched resolution — point, marquee, scene marquee, lasso; `Selection`.
- [04]-[ECHO_ROWS]: projections — GPU highlight, feature-state, grid selection, reveal; `Selection`.
- [05]-[ANCHOR_PINS]: topic pins — world anchors, per-surface placement, the element-hotspot adapter; `Mark`.
- [06]-[VIEWPOINT_RESTORE]: viewpoint → camera-intent + selection-op fold with anchor evidence; `Mark`.
- [07]-[TOPIC_BOARD]: status and priority vocabularies, board rows and census, write-egress intents; `Mark`.

## [02]-[SELECTION_FOLD]

[SELECTION_FOLD]:
- Owner: `Selection` — the op-driven fold: `Selection.Op` is a closed `Data.taggedEnum` and `Selection.apply(set, op)` the effectful total fold, each arm one `HashSet` combinator and every admitted op published once through `Selection.Echoes`; the live atom is `History.make(HashSet.empty())` so undo/redo is construction — writes mint `History.Op.Push` over the applied set and the `present` projection feeds every consumer.
- Packages: `effect` (`HashSet`, `Data`, `Schema`); `system/atom` (`History`).
- Law: `Selection.Id` is the ONE `GlobalId` brand and decode surface in the branch — the IFC 22-character base64 alphabet the corpus rule `len 22` beside its pattern proves on every `selectedGlobalIds` member — so the grid, the review board, and the viewpoint restore all read this owner and no page mints a second brand; the brand string is `Equal`-stable, so set membership is structural.
- Law: ops are the only writes — a marquee, a click, a viewpoint restore, and a grid row toggle all mint `Selection.Op` values; no consumer holds a second set or mutates through any other path.
- Law: modality lives in the op value — click maps to `Toggle` (modifier policy deciding `Replace` versus `Toggle` at the interaction row), marquee maps to `Add` or `Replace`, viewpoint restore maps to `Replace` — never a boolean knob on the fold.
- Growth: a new set behavior (invert, filter-to-visible) is one op case and one fold arm.

```typescript signature
import { Wire } from "@rasm/core"
import { Data, HashSet, Option, Schema } from "effect"

// the set's own element: a 22-character IFC GlobalId under the base64 alphabet the BCF schema fixes, branded ONCE
// here where the set that holds it lives, and read by every surface that keys on it
const _GlobalId = Schema.String.pipe(Schema.length(22), Schema.pattern(/^[0-9A-Za-z_$]{22}$/), Schema.brand("GlobalId"))
type GlobalId = typeof _GlobalId.Type

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

const _decode: (raw: unknown) => Option.Option<GlobalId> = Schema.decodeUnknownOption(_GlobalId)

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
- Owner: `Selection.Hit` and `Selection.picked` — one closed hit vocabulary and ONE resolution entrypoint across every seam. Pipes mint `Hit` values at their own boundary and never resolve an id themselves: `Row` carries an opaque foreign row (a deck `PickingInfo.object`, a GeoArrow row proxy, a lasso survivor, a basemap feature) whose id member decodes through the one `_decode`; `Tiled` carries a decoded 3D-tile content with the batch ordinal and the column its metadata keys on; `Grafted` carries a content key the element index resolves. Three arms because three RESOLUTIONS exist, not because four surfaces do — the deck, lasso, and basemap seams share one arm precisely because they share one answer.
- Law: the foreign row crosses as `unknown` and decodes at this seam — a `Predicate.hasProperty` probe then `_decode` is the whole admission, so no provider shape (`MapGeoJSONFeature`, a GeoArrow proxy, a plain `Feature`) is named in a signature and a new row-bearing surface needs no arm; a typed provider union here grows one member per surface for zero resolution difference.
- Law: an unresolvable hit is absence, not a fault — a pick over empty space, a row without the id member, or a batch column the tileset never carried folds to `Option.none` and the op carries fewer ids; picking never mints faults.
- Law: resolution is ONE batched fold over the whole hit set — `Selection.picked` sweeps every arm through `Effect.forEach(..., { batching: true })`, so a marquee's hundreds of graft keys collapse into one element-index traversal under the shared `RequestResolver.makeBatched` window while the row and tile arms answer in place at zero request cost; a per-hit lookup loop, or a second resolution path for the cheap arms, is the named defect.
- Law: the request family is tagged — the resolver's batch window keys on request identity, and a `Request.TaggedClass` carries the tag, the payload equality that de-duplicates two picks of one key, and the success and failure types in one declaration.
- Law: the tile arm reads the RAW metadata fields, never a convenience accessor the payload lacks — a decoded content carries `featureTableJson`/`featureTableBinary` and `batchTableJson`/`batchTableBinary` as four separate members, so the arm constructs `Tile3DBatchTable` from them with the feature-table `BATCH_LENGTH` as its count and reads the id column by name; the batch-table column is caller-supplied because it is the tileset's own authoring choice, never an estate constant.
- Law: the three-arm point pick descends the tree, never the triangle list — `acceleratedRaycast` patches `Mesh.prototype.raycast` at the one prototype-owner seam, and `firstHitOnly` on the raycaster makes each mesh answer its nearest hit and stop, which is the whole reason the pick stays interactive over a merged CAD assembly.
- Law: the marquee over the scene rides the package's OWN named specialization — `MeshBVH.intersectsBox(box, boxToMesh)` IS the `shapecast` descent under an oriented volume, so the row composes it rather than re-authoring the callback triple the library already specializes; the volume arrives as a box with its world placement (the screen rect un-projected), and each graft's own inverse world matrix carries it into that tree's frame.
- Law: the accelerated trees arrive stamped — a residency mutation or streamed-geometry edit rebuilds or refits at the graft, so a held descent re-reads the stamp rather than answering against a hierarchy the ledger already replaced; this page consumes the stamped structure and owns neither the build nor its invalidation.
- Law: lasso is planar compute with a scale ladder — a freehand polygon hit-tests through `booleanPointInPolygon` against feature centroids with a `geojsonRbush` index making many-feature scenes sub-linear (`search(bbox)` prunes to the candidate band before the exact test); the index builds once per feature-set change and is held in a derived atom, because rebuilding it per gesture frame spends the cost the index exists to remove.
- Packages: `@deck.gl/core` (`Deck`, `PickingInfo`); `@loaders.gl/3d-tiles` (`Tile3DBatchTable`, `Tiles3DTileContent`); `three` (`Raycaster`, `Matrix4`, the camera and intersection types); `three-mesh-bvh` (`MeshBVH.intersectsBox`, `acceleratedRaycast`, the `Raycaster.firstHitOnly` merge); `@turf/turf` (`bbox`, `booleanPointInPolygon`, `geojsonRbush`); `@types/geojson` (the feature value types); `effect` (`Array`, `Data`, `Effect`, `HashMap`, `Option`, `Predicate`, `Request`, `RequestResolver`).
- Boundary: the gesture drawing the marquee/lasso is `system/act#CONTINUOUS_OWNER`'s; pixel→world math is `geo#PROJECT`'s; which surfaces are pickable is the owning layer row's toggle; the BVH build, its stamp, and the one legal `three` prototype patch are `scene#RESIDENCY_GRAFT`'s.
- Growth: a new pickable surface is a `Hit.Row` producer and nothing else; a million-feature lasso graduates to the GPU fold — a `typegpu` centroid-in-polygon kernel over a `d.arrayOf` centroid buffer adopting the scene-published device (`scene#BACKEND_SELECT`'s compute seam); the CPU rbush ladder stays the floor, and the kernel is one growth row, never a second lasso vocabulary.

```typescript signature
import type { Deck, PickingInfo } from "@deck.gl/core"
import { Tile3DBatchTable, type Tiles3DTileContent } from "@loaders.gl/3d-tiles"
import { bbox, booleanPointInPolygon, geojsonRbush } from "@turf/turf"
import { Array, Data, Effect, HashMap, type ParseResult, Predicate, Request, RequestResolver } from "effect"
import type { Feature, GeoJsonProperties, Point, Polygon } from "geojson"
import { type Box3, type Camera as SceneCamera, type Intersection, Matrix4, type Object3D, Raycaster, type Vector2 } from "three"
import type { MeshBVH } from "three-mesh-bvh"

const _MARQUEE = { cap: 4096 } as const

declare namespace Selection {
  type Hit = Data.TaggedEnum<{
    Row: { readonly row: unknown } // the foreign row crosses opaque: deck object, arrow proxy, lasso survivor, basemap feature
    Tiled: { readonly content: Tiles3DTileContent; readonly batch: number; readonly column: string }
    Grafted: { readonly key: string }
  }>
}

declare namespace Mark {
  type Resident = { readonly tree: MeshBVH; readonly node: Object3D }
  // pick plane consumes the graft ledger: one accelerated tree per resident key, one reverse read, and the
  // stamp a rebuild or refit advances so a held descent never answers against a replaced hierarchy
  type Trees = {
    readonly stamp: number
    readonly held: HashMap.HashMap<string, Mark.Resident>
    readonly keyOf: (node: Object3D) => Option.Option<string>
  }
  type Volume = { readonly box: Box3; readonly toWorld: Matrix4 } // the screen rect un-projected: an axis-aligned box plus its world placement
  type Index<P extends GeoJsonProperties = GeoJsonProperties> = ReturnType<typeof geojsonRbush<Point, P>>
}

const _Hit = Data.taggedEnum<Selection.Hit>()

class _ResolveId extends Request.TaggedClass("ResolveId")<Option.Option<GlobalId>, never, { readonly key: string }> {}

const _resolveIds = (index: (keys: ReadonlyArray<string>) => ReadonlyArray<Option.Option<GlobalId>>) =>
  RequestResolver.makeBatched((requests: Array.NonEmptyArray<_ResolveId>) =>
    Effect.forEach(
      Array.zip(requests, index(Array.map(requests, (request) => request.key))),
      ([request, resolved]) => Request.succeed(request, resolved),
      { discard: true },
    ))

const _resolved = (hit: Selection.Hit, resolver: RequestResolver.RequestResolver<_ResolveId>): Effect.Effect<Option.Option<GlobalId>> =>
  _Hit.$match(hit, {
    Row: ({ row }) =>
      Effect.succeed(
        Option.fromNullable(row).pipe(
          Option.filter(Predicate.hasProperty("globalId")),
          Option.flatMap((held) => _decode(held.globalId)),
        ),
      ),
    Tiled: ({ batch, column, content }) =>
      Effect.succeed(
        // payload carries these four raw members; the accessor is constructed, never read off a field that does not exist
        _decode(
          new Tile3DBatchTable(
            content.batchTableJson,
            content.batchTableBinary,
            content.featureTableJson?.BATCH_LENGTH,
          ).getProperty(batch, column),
        ),
      ),
    Grafted: ({ key }) => Effect.request(new _ResolveId({ key }), resolver), // the only arm that costs a request; the window collapses every graft key in the sweep
  })

const _picked = (
  hits: ReadonlyArray<Selection.Hit>,
  resolver: RequestResolver.RequestResolver<_ResolveId>,
): Effect.Effect<ReadonlyArray<GlobalId>> =>
  Effect.map(Effect.forEach(hits, (hit) => _resolved(hit, resolver), { batching: true }), Array.getSomes)

const _marquee = (
  deck: Deck,
  box: { readonly x: number; readonly y: number; readonly width: number; readonly height: number },
): Effect.Effect<ReadonlyArray<Selection.Hit>> =>
  Effect.map(
    // deck answers through the WebGPU-safe async pair; the deprecated sync mirrors never appear
    Effect.promise(() => deck.pickObjectsAsync({ ...box, maxObjects: _MARQUEE.cap })),
    (hits) => Array.map(hits, (info: PickingInfo) => _Hit.Row({ row: info.object })),
  )

const _marqueeScene = (trees: Mark.Trees, volume: Mark.Volume): ReadonlyArray<Selection.Hit> =>
  Array.filterMap(HashMap.toEntries(trees.held), ([key, resident]) =>
    // one accelerated descent per graft under the same oriented volume, carried into each tree's own frame
    resident.tree.intersectsBox(volume.box, new Matrix4().copy(resident.node.matrixWorld).invert().multiply(volume.toWorld))
      ? Option.some(_Hit.Grafted({ key }))
      : Option.none())

const _pointer = (camera: SceneCamera, ndc: Vector2): Raycaster => {
  // BOUNDARY ADAPTER: the platform constructor-then-configure seam — firstHitOnly rides the three-mesh-bvh merge, so
  // each patched mesh answers its nearest hit and stops descending
  const raycaster = new Raycaster()
  raycaster.firstHitOnly = true
  raycaster.setFromCamera(ndc, camera)
  return raycaster
}

const _grafted = (trees: Mark.Trees, hits: ReadonlyArray<Intersection>): ReadonlyArray<Selection.Hit> =>
  Array.filterMap(hits, (hit) => Option.map(trees.keyOf(hit.object), (key) => _Hit.Grafted({ key })))

const _lasso = <P extends GeoJsonProperties>(index: Mark.Index<P>, polygon: Feature<Polygon>): ReadonlyArray<Selection.Hit> =>
  // index prunes to the polygon's own extent before any exact test runs; the scan it removes is the O(n) defect
  Array.filterMap(index.search(bbox(polygon)).features, (centroid) =>
    booleanPointInPolygon(centroid, polygon) ? Option.some(_Hit.Row({ row: centroid })) : Option.none())
```

## [04]-[ECHO_ROWS]

[ECHO_ROWS]:
- Law: echoes are projections, never stores — deck layers read the set through a `DataFilterExtension` category or a color accessor keyed by membership (with `updateTriggers` naming the set's version); the batched scene arm flips `setVisibleAt`/tint rows through `scene#DRAW_COLLAPSE`; maplibre echoes through `geo#STYLE_DATA`'s `Geo.echo`, which takes exactly the `{ entered, left }` pair `Selection.diff` computes by `HashSet.difference` both ways, so this page owns the diff and the basemap owner owns the feature-state write; the grid's row selection (`view/table`) derives keyed by the same `GlobalId` strings and writes back through the same ops — one fold, many mirrors.
- Law: reveal is a camera intent — selecting from the grid emits `Camera.Intent.FitBounds`/`EaseTo` over the selected features' bbox; reveal never reaches into a map instance directly.
- Law: the selection count and id list surface through `system/primitive` rows — status text via `Message` plural forms, `announce` as polite SR feedback on large marquee results.
- Law: non-view echo consumers subscribe through one bounded replay channel — `Selection.echoes` is the memoized `Layer` constructing `PubSub.sliding<Selection.Op>({ capacity, replay })` once per app, and `Selection.Echoes` is the shared service every publisher and subscriber yields; late and live wire egress, probe evidence, and sibling mirrors consume the retained operation window without touching the atom registry, saturation replaces the oldest retained operation, and a second subscription protocol is the named defect.
- Law: `Selection.Echoes` is the adopted source behind the `rasm.ui.mark.op` hook point (`system/hook`, replay modality) — this page contributes the point row and `Selection.hook` yields the shared service, so the registry pumps this one channel and no second op publisher exists.

```typescript signature
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
  type Id = GlobalId
  type Shape = {
    readonly Id: typeof _GlobalId
    readonly Echoes: typeof _Echoes
    readonly Op: typeof _Op
    readonly decode: typeof _decode
    readonly apply: typeof _apply
    readonly diff: typeof _diff
    readonly echoes: typeof _echoes
    readonly hook: typeof _hook
    readonly Hit: typeof _Hit
    readonly resolveIds: typeof _resolveIds
    readonly picked: typeof _picked
    readonly marquee: typeof _marquee
    readonly marqueeScene: typeof _marqueeScene
    readonly pointer: typeof _pointer
    readonly grafted: typeof _grafted
    readonly lasso: typeof _lasso
  }
}

const Selection: Selection.Shape = {
  Id: _GlobalId,
  Echoes: _Echoes,
  Op: _Op,
  decode: _decode,
  apply: _apply,
  diff: _diff,
  echoes: _echoes,
  hook: _hook,
  Hit: _Hit,
  resolveIds: _resolveIds,
  picked: _picked,
  marquee: _marquee,
  marqueeScene: _marqueeScene,
  pointer: _pointer,
  grafted: _grafted,
  lasso: _lasso,
}
```

## [05]-[ANCHOR_PINS]

[ANCHOR_PINS]:
- Owner: `Mark.pin` — the pin projection: each open topic's primary viewpoint yields the world anchor and the pin carries it beside its identity and its two lifecycle keys; screen placement is the surface's own answer, folded in through the projector parameter. Pins render as DOM anchors — a maplibre `Marker` on map surfaces, a `@floating-ui/react` `VirtualElement` whose `getBoundingClientRect` wraps the projected point on scene surfaces, a slotted element hotspot on `<model-viewer>` surfaces — one pin mechanism per surface class, chosen by the surface row, never stacked.
- Packages: `@rasm/core` (`BcfTopic`, `BcfViewpoint`); `@google/model-viewer` (`ModelViewerElement` — the one imported type the embed payload derives from); `effect` (`Option`, `pipe`).
- Boundary: the pin fold is pure and takes its projector as a parameter — the DOM anchor host is app composition and the projector itself is `geo#PROJECT`'s `Camera.anchor` on scene surfaces or `Mark.queried` on embed surfaces; this page mints the pin value and never the element.
- Law: pins are projections of decoded topics — pin identity is the topic guid, a pin's screen position derives per camera settle from the anchor, and no pin holds its own position state. Look-at target anchors the pin: the eye is where the reviewer stood, never what the topic marks.
- Law: the anchor is the viewpoint's own BCF world frame verbatim — every surface reads one frame and adaptation is the projector's, so a re-framed anchor (a lnglat crossing, a scene-space rebase) never lands on the pin; the model's own coordinates are what the GLB and every `<model-viewer>` hotspot were authored in, so the embed arm crosses nothing.
- Law: placement is real absence — `Mark.Placed` arrives as `Option` because two of the three surface classes position their own node: the maplibre arm's projector is `Function.constant(Option.none())` since the `Marker` owns the point, the embed arm's is `Mark.queried` and answers none for an unmounted slot and a degenerate projection alike, and only the scene arm computes one. Fabricated origins standing for unplaceable pins put every off-screen topic in the viewport's corner.
- Law: pin glyph, tone, and emphasis key off the lifecycle vocabularies — `[7]`'s status and priority tables are the single styling source and `Mark.variant` their one join; a status conditional in a pin row marks the tables unused.
- Law: `<model-viewer>` surfaces anchor through the element's own ray and its own hotspot slots — `Mark.ray` mints an anchor from an authoring gesture (`positionAndNormalFromPoint` for the model-space point and normal, `surfaceFromPoint` for the barycentric surface id that keeps the anchor glued while a clip plays) and `Mark.mounted`/`Mark.moved` carry pins as element hotspots; the embed adapter row, same vocabulary.
- Law: the hotspot mount is declarative and the move is imperative — the element registers a hotspot ONLY from a slotted child whose slot name begins with the prefix, reading `data-position`/`data-normal`/`data-surface`/`data-model-index` at attach, so `Mark.mounted` IS the pin's existence; those attributes are unobserved, so re-rendering the child with a fresh dataset moves nothing and a viewpoint replacement reaches the element only through `Mark.moved`, which silently no-ops on a slot no child mounted. Every hotspot sharing a slot name shares one location and the first definition wins until that move, which is why the slot name is minted from the topic guid and from nothing else.
- Law: back-facing dimming is a CSS variant, never a per-frame read — the mount record carries `data-visibility-attribute`, so the element toggles `data-facing` on the slotted node and dispatches its own `hotspot-visibility` event as the camera turns; `Mark.queried`'s `facing` serves the consumers outside that slot's subtree (a roster beside the viewport), and a per-frame `queryHotspot` sweep to style a marker is the named defect.
- Boundary: pin press interaction rides `system/act#DISCRETE_ROWS`; rich topic text sanitizes through `system/primitive`'s gate before any DOM sink; the live camera an app joins to a gesture-minted anchor to mint a viewpoint is `geo#CAMERA`'s embed adapter row.
- Growth: a new surface class is one projector with its own anchor host; a new anchor facet the wire gains (a clipping-derived offset) is one `Mark.Anchor` field carried through both projections.

```typescript signature
import type { ModelViewerElement } from "@google/model-viewer"
import { Wire } from "@rasm/core"
import { Option, pipe } from "effect"

// element admits on the prefix alone — a child whose slot name does not start with it is never
// registered — and `facing` is the suffix the element toggles back as `data-facing` on the slotted node
const _HOTSPOT = { prefix: "hotspot", facing: "facing" } as const

declare namespace Mark {
  type World = readonly [number, number, number]
  // normal, surface, and model index exist only on a gesture-minted anchor: a decoded viewpoint carries none, so
  // each rides an optional slot the element's own documented default fills, never a value no producer measured
  type Anchor = {
    readonly target: Mark.World
    readonly normal: Option.Option<Mark.World>
    readonly surface: Option.Option<string>
    readonly model: Option.Option<number>
  }
  type Sited = { readonly guid: string; readonly anchor: Mark.Anchor } // exactly what the hotspot write and the projector read both key on
  type Placed = { readonly at: readonly [number, number]; readonly facing: Option.Option<boolean> }
  type Project = (sited: Mark.Sited) => Option.Option<Mark.Placed>
  type Pin = Mark.Sited & {
    readonly title: string
    // both keys arrive ADMITTED, never raw: `Mark.variant` joins them against `[7]`'s tables and the open wire
    // priority has no row there, so the admission runs at the mint rather than at every styling read
    readonly status: Mark.Status
    readonly priority: Mark.Priority
    readonly placed: Option.Option<Mark.Placed>
  }
  // payload derives off the one imported element type; a second specifier into the package's interior
  // file layout, or a hand-declared twin beside it, is the parallel restatement this derivation deletes
  type Hotspot = Parameters<ModelViewerElement["updateHotspot"]>[0]
  type Mount = {
    readonly slot: string
    readonly "data-position": string
    readonly "data-visibility-attribute": string
    readonly "data-normal"?: string
    readonly "data-surface"?: string
    readonly "data-model-index"?: string
  }
}

type _Camera = NonNullable<Wire.BcfViewpoint["camera"]>
type _Vec = NonNullable<_Camera["position"]>

// wire camera carries `position` as `Point3` and `direction` as `UnitDirection3` while every projector and camera intent reads a
// tuple, so the one axis-ordered crossing lives here — an indexed read off the message takes `undefined` on all three
const _axes = (vec: _Vec): Mark.World => [vec.x, vec.y, vec.z]

// the two vectors a frame needs are message columns the corpus marks required, so the generated type still admits
// their absence; ONE read folds both presences, and a camera carrying neither frames nothing rather than a NaN eye
const _framed = (camera: _Camera): Option.Option<{ readonly eye: Mark.World; readonly target: Mark.World }> =>
  Option.map(
    Option.all({ position: Option.fromNullable(camera.position), direction: Option.fromNullable(camera.direction) }),
    ({ position, direction }) => ({
      eye: _axes(position),
      target: [position.x + direction.x, position.y + direction.y, position.z + direction.z],
    }),
  )

const _slot = (guid: string): string => `${_HOTSPOT.prefix}-${guid}`

// element parses both attributes in its camera-target grammar and its own Vector3D.toString() emits exactly
// this form, so one spelling serves the write and matches what a read hands back
const _metres = (world: Mark.World): string => `${world[0]}m ${world[1]}m ${world[2]}m`

const _hotspot = (sited: Mark.Sited): Mark.Hotspot => ({
  name: _slot(sited.guid),
  position: _metres(sited.anchor.target),
  ...(Option.isSome(sited.anchor.normal) && { normal: _metres(sited.anchor.normal.value) }),
  ...(Option.isSome(sited.anchor.surface) && { surface: sited.anchor.surface.value }),
  ...(Option.isSome(sited.anchor.model) && { modelIndex: sited.anchor.model.value }),
})

// mount record IS the pin's existence — the element creates a hotspot only from a slotted child and the move
// below cannot conjure one; both projections read the same two spelling owners, so neither end can drift
const _mounted = (sited: Mark.Sited): Mark.Mount => ({
  slot: _slot(sited.guid),
  "data-position": _metres(sited.anchor.target),
  "data-visibility-attribute": _HOTSPOT.facing,
  ...(Option.isSome(sited.anchor.normal) && { "data-normal": _metres(sited.anchor.normal.value) }),
  ...(Option.isSome(sited.anchor.surface) && { "data-surface": sited.anchor.surface.value }),
  ...(Option.isSome(sited.anchor.model) && { "data-model-index": String(sited.anchor.model.value) }),
})

// BOUNDARY ADAPTER: the dataset attributes are unobserved, so a re-rendered child moves nothing and this is the one
// write a viewpoint replacement reaches; an unmounted slot no-ops, which the mount record forecloses
const _moved = (element: ModelViewerElement, sited: Mark.Sited): void => element.updateHotspot(_hotspot(sited))

// element owns projection on this surface, so the screen point is a READ: null covers an unmounted slot and a
// non-finite canvas position alike, and `facingCamera` is the back-face verdict nothing outside the slot can reach
const _queried = (element: ModelViewerElement): Mark.Project => (sited) =>
  Option.map(Option.fromNullable(element.queryHotspot(_slot(sited.guid))), (held) => ({
    at: [held.canvasPosition.x, held.canvasPosition.y] as const,
    facing: Option.some(held.facingCamera),
  }))

// authoring gesture pays a second descent to buy `surface`: the barycentric id keeps the anchor on its triangle
// while a clip plays, where a fixed model-space point drifts off animated geometry
const _ray = (element: ModelViewerElement, pixel: readonly [number, number]): Option.Option<Mark.Anchor> =>
  Option.map(Option.fromNullable(element.positionAndNormalFromPoint(pixel[0], pixel[1])), (hit) => ({
    target: [hit.position.x, hit.position.y, hit.position.z] as const,
    normal: Option.some([hit.normal.x, hit.normal.y, hit.normal.z] as const),
    surface: Option.fromNullable(element.surfaceFromPoint(pixel[0], pixel[1])),
    model: Option.fromNullable(hit.modelIndex),
  }))

const _pin = (topic: Wire.BcfTopic, viewpoint: Option.Option<Wire.BcfViewpoint>, project: Mark.Project): Option.Option<Mark.Pin> =>
  Option.flatMap(viewpoint, (held) =>
    // a camera-less viewpoint anchors nothing spatial — it is a selection-only anchor and mints no pin
    Option.flatMap(Option.flatMap(Option.fromNullable(held.camera), _framed), (frame) =>
      Option.map(_keys(topic), (keys) =>
        pipe(
          { guid: topic.guid, anchor: { target: frame.target, normal: Option.none(), surface: Option.none(), model: Option.none() } },
          (sited: Mark.Sited) => ({ ...sited, title: topic.title, ...keys, placed: project(sited) }),
        ))))
```

## [06]-[VIEWPOINT_RESTORE]

[VIEWPOINT_RESTORE]:
- Owner: `Mark.restore(viewpoint, resident, millis)` admits `BcfViewpointWire` bytes through `Wire.decode`, then folds two outputs and one receipt: the wire's optional camera block (position/direction/up and the `lens` oneof — consume-only carriage per the wire law) mints one `Camera.Intent.LookAt` where present — eye from the position rows, target from position and direction, the ease duration as the caller's policy — that every surface class dispatches through `Camera.drive`, and answers `Option.none` on a camera-less viewpoint (a selection-only anchor `review#ECHO_ROWS` frames); the `selectedGlobalIds` array mints the existing `Selection.Op.Replace` case directly; and the anchor receipt reports which ids resolved against the live model — the partial-failure evidence the operator reads.
- Law: restore never re-derives — no view geometry computes beyond coordinate adaptation; the viewpoint IS the proof, and a restore that corrects the camera is the drift defect.
- Law: the receipt is data — `{ requested, resolved, missing }` counts and the missing id list; it renders as an evidence row (`Message` plural forms), never throws; a fully-missing selection still restores a carried camera, and a camera-less viewpoint restores selection alone.
- Boundary: which elements are resident is `scene`'s graft ledger fact; intent dispatch is `geo#CAMERA`'s; the selection fold is `[2]`'s.

```typescript signature
import { Array, HashSet, Option, pipe } from "effect"
import { Camera } from "./geo.ts"

declare namespace Restore {
  type Receipt = {
    readonly requested: number
    readonly resolved: number
    readonly missing: ReadonlyArray<GlobalId>
  }
}

// the wire's ids arrive as strings the corpus rule already proved 22-character base64; the brand decode is the
// typed narrowing onto the set's own element, never a second check
const _ids = (raw: ReadonlyArray<string>): ReadonlyArray<GlobalId> => Array.filterMap(raw, _decode)

const _restore = (
  viewpoint: Uint8Array,
  resident: HashSet.HashSet<GlobalId>,
  millis: number,
): Effect.Effect<
  { readonly intent: Option.Option<Camera.Intent>; readonly op: Selection.Op; readonly receipt: Restore.Receipt },
  Wire.Fault | ParseResult.ParseError
> =>
  Effect.map(
    Wire.decode("BcfViewpointWire", viewpoint),
    (admitted) =>
      pipe(
        Array.partition(_ids(admitted.selectedGlobalIds), (id) => HashSet.has(resident, id)),
        ([missing, resolved]) => ({
          // the camera is an optional message: a camera-less viewpoint restores selection alone (review#ECHO_ROWS frames it)
          intent: Option.map(Option.flatMap(Option.fromNullable(admitted.camera), _framed), (frame) =>
            Camera.Intent.LookAt({ eye: frame.eye, target: frame.target, millis })),
          op: _Op.Replace({ ids: resolved }),
          receipt: { requested: admitted.selectedGlobalIds.length, resolved: resolved.length, missing },
        }),
      ),
  )
```

## [07]-[TOPIC_BOARD]

[TOPIC_BOARD]:
- Owner: `Mark.status` and `Mark.priority` — the two lifecycle vocabularies keyed against the wire's own axes, `Mark.statuses` the defined `BcfStatus` roster and its guard, `Mark.defined` the one enum-narrowing seat every ui enum table reads: the status table carries the tone, the glyph (a `LucideIcon` per row — icon-as-identity), and the `live` column, while the priority table carries the glyph, the sort `rank`, the `escalated` column, and the emphasis class the marker recipe reads as its second variant axis; `Mark.board` admits each `BcfTopicWire` byte document through `Wire.decode` before folding the ordered rows, `Mark.census` projects the header counts, and `Mark.Intent` is the whole write surface.
- Packages: `lucide-react` (the glyph rows); `class-variance-authority` (`cva` — the marker recipe); `@rasm/core` (`Wire.BcfTopic`); `@rasm\/contracts/rasm/contracts/bcf/bcf_pb` (`BcfStatus`); `@bufbuild/protobuf/wkt` (`timestampMs`); `effect` (`Array`, `Data`, `DateTime`, `Duration`, `Either`, `Option`, `Order`, `Record`); `system/token` (`Theme` — the roster the tone axis derives from); `system/intl` (`Format.instant`); `system/primitive` (`Primitive.sanitize`, the roster law).
- Law: the two axes close differently and the tables say which — `status` is the generated `BcfStatus` enum, `defined_only` at the corpus, so the status table keys its rows on the enum's own members and proves BOTH directions through `Mark.Defined`, a corpus vocabulary change breaking these rows at compile time, while `priority` crosses as an open string that no guard can close. `Mark.keys` is that asymmetry's one seam: it carries the enum narrowing the landed TYPE still lacks (`UnknownEnum` and `UNSPECIFIED` stay in the generated union the rule already refused) and folds an unlisted producer priority to `normal` on the way into a row, so every table, census column, and recipe variant downstream keys on a member the ladder carries. Widening a status locally is the named defect.
- Law: the two axes are orthogonal by construction — status decides TONE and priority decides EMPHASIS, so `Mark.variant` is the one join both the pin and the board row read; a tone column on the priority table puts two palettes on one element and forks the roster the tone axis derives from.
- Law: the status row's `live` column is the ONLY place liveness is decided — the overdue verdict reads it here and every surface filtering live topics reads the same column instead of re-listing which statuses count as open, so a closed topic past its date is history rather than a breach; the priority row's `escalated` column carries attention the same way, and a conditional over status or priority names re-derives a column the row already states.
- Law: the due read is one `distanceDurationEither` — `Either.left` is time overdue and `Either.right` time remaining, so the verdict and its magnitude arrive together and no sentinel instant stands for an absent date; every wire instant crosses ONCE through `timestampMs` into `DateTime.Utc` at `_instant`, and the row keeps the decoded `due` `Option` beside it so the order sorts dated before undated with no forged bound.
- Law: the fold takes its instant as a PARAMETER — `now` arrives from the app's own clock read, so after byte admission the board is a pure projection re-folded on a tick the app owns and no ambient wall-clock read enters the module.
- Law: comment threads land at full depth and pass the gate once — the fold sorts the decoded comments on their own instant (stable, so a shared stamp keeps the producer's sequence), crosses the epoch exactly once through `Format.instant`, measures each comment's age against the same `now`, and stores the `Primitive.sanitize` result as the row's `body`, so the only body spelling a DOM sink can reach is the gated one; the locale-bound render of a stamp or an age is the consuming row's.
- Law: the board is roster-law list rows, never a second collection engine — `system/primitive#ROSTER_LAW` owns the RAC collection, its controlled `selectedKeys` binding, and its typeahead, so this page contributes the row set, the `Order` instance, the census, and the vocabularies; a bespoke row renderer or a second comparator beside `Mark.order` is the named defect.
- Law: writes are egress, not state — `Mark.Intent` is the closed two-case family every affordance mints (`Comment` appends, `Amend` patches), the app encodes it at the wire, and this module holds no authored BCF value; an amendment's keys are indexed access off `BcfTopic` field-for-field, its status the narrowed member, and its two absences are distinct — an omitted key is UNCHANGED while `Option.none()` in a present key is CLEARED.
- Boundary: which topics a session holds is the app's atom state; the wire encode of an intent is the core interchange plane's; a comment's `viewpoint` resolves through `[6]`'s restore, never a camera write here.
- Growth: a new lifecycle presentation is one corpus enum member with its status row; a new priority is one `Mark.priority` row the admission then carries with no wire edit at all; a new authored axis is one `Amendment` key; a new board facet — a clash count, a cost delta — is one row field and one arm in the same fold.

```typescript signature
import { type Timestamp, timestampMs } from "@bufbuild/protobuf/wkt"
import { BcfStatus } from "@rasm\/contracts/rasm/contracts/bcf/bcf_pb"
import { cva } from "class-variance-authority"
import { Array, Data, DateTime, type Duration, Either, Option, Order, Record, pipe } from "effect"
import type { LucideIcon } from "lucide-react"
import { CircleAlert, CircleCheck, CircleDot, CircleSlash, Flame, RotateCcw, SignalHigh, SignalLow, SignalMedium } from "lucide-react"
import { Format } from "../../src/system/intl.ts"
import { Primitive } from "../../src/system/primitive.ts"
import { Theme } from "../../src/system/token.ts"

// ONE narrowing seat for every generated enum the ui reads: the corpus rule `defined_only` beside `not_in: [0]`
// refused `UNSPECIFIED` and every foreign member at admission, and the generated TYPE still spells both, so this
// owner derives the defined member roster off the `as const` object protoc-gen-es emits and publishes the guard that
// carries the rule into the type. Every ui table keyed on an enum closes against `Mark.Defined<E>` and reads its
// column by the generated member, never a string token this end would mint.
type _Defined<E extends { readonly UNSPECIFIED: 0 }> = Exclude<E[keyof E], E["UNSPECIFIED"]>
const _defined = <E extends { readonly UNSPECIFIED: 0 }>(members: E): {
  readonly members: ReadonlyArray<_Defined<E>>
  readonly is: (raw: unknown) => raw is _Defined<E>
} => {
  const defined = Array.filter(Record.values(members), (member): member is _Defined<E> => member !== members.UNSPECIFIED)
  return { members: defined, is: Schema.is(Schema.Literal(...defined)) }
}

// every wire instant crosses here and nowhere else: the shipped `timestampMs` bridge folds seconds and nanos, and
// absence stays absence because the producer omits an unset stamp
const _instant = (stamp: Timestamp | undefined): Option.Option<DateTime.Utc> =>
  Option.map(Option.fromNullable(stamp), (held) => DateTime.unsafeMake(timestampMs(held)))

const _status = _defined(BcfStatus)
const _priorities = ["low", "normal", "high", "critical"] as const

const _statusRows = {
  [BcfStatus.OPEN]: { icon: CircleDot, tone: "accent", live: true },
  [BcfStatus.IN_PROGRESS]: { icon: CircleAlert, tone: "accent", live: true },
  [BcfStatus.RESOLVED]: { icon: CircleCheck, tone: "success", live: false },
  [BcfStatus.CLOSED]: { icon: CircleSlash, tone: "neutral", live: false },
  [BcfStatus.REOPENED]: { icon: RotateCcw, tone: "danger", live: true },
} as const satisfies { readonly [K in _Defined<typeof BcfStatus>]: Mark.StatusRow }

// ring column is the recipe's second axis and carries no colour: the status row decides tone, this row decides
// emphasis, so one element never resolves two competing palettes
const _priorityRows = {
  low: { icon: SignalLow, rank: 0, escalated: false, ring: "ring-0" },
  normal: { icon: SignalMedium, rank: 1, escalated: false, ring: "ring-1" },
  high: { icon: SignalHigh, rank: 2, escalated: true, ring: "ring-2" },
  critical: { icon: Flame, rank: 3, escalated: true, ring: "ring-2 ring-offset-2" },
} as const

declare namespace Mark {
  type Defined<E extends { readonly UNSPECIFIED: 0 }> = _Defined<E>
  type Status = _Defined<typeof BcfStatus>
  type Priorities = typeof _priorities
  type Priority = keyof typeof _priorityRows
  type StatusRow = { readonly icon: LucideIcon; readonly tone: Theme.Tone; readonly live: boolean }
  type PriorityRow = { readonly icon: LucideIcon; readonly rank: number; readonly escalated: boolean; readonly ring: string }
  type Keys = { readonly status: Mark.Status; readonly priority: Mark.Priority }
  // status closure runs BOTH directions: the table's `satisfies` refuses an excess row and this alias refuses a
  // generated member without one — a one-way guard admits the vocabulary that silently loses a case
  type _StatusGap<K extends keyof typeof _statusRows = Mark.Status> = K
  // priority carries no wire closure to prove against — `BcfTopic["priority"]` is an open string, so only the
  // widening direction is checkable here and `_keys` below is what keeps an unlisted producer word off the tables
  type _Priorities<K extends Wire.BcfTopic["priority"] = Mark.Priority> = K
  type _PriorityRows<T extends Record.ReadonlyRecord<Priorities[number], PriorityRow> = typeof _priorityRows> = T
}

// BOUNDARY ADAPTER: open wire axis, closed presentation ladder — the predicate carries the narrowing so no call site
// casts, and an unlisted producer priority folds to `normal` at this ONE seam rather than keying a table that has no row
const _known = (raw: Wire.BcfTopic["priority"]): raw is Mark.Priority => Object.hasOwn(_priorityRows, raw)

// the ONE admission of a topic's two axes: the status guard carries the corpus rule into the type — its `none` arm
// is the member the rule already refused and no producer reaches — and the priority fold admits the open axis
const _keys = (topic: Wire.BcfTopic): Option.Option<Mark.Keys> =>
  Option.map(Option.liftPredicate(topic.status, _status.is), (status) => ({
    status,
    priority: _known(topic.priority) ? topic.priority : "normal",
  }))

// tone axis DERIVES from the token roster's own keyed table, so a semantic added there lands on this recipe with
// zero edits, the class strings name only the generated slot utilities, and `VariantProps` keeps the literal tone
// union a tuple fold would widen to `string`; this page holds no palette
const _marker = cva("inline-flex items-center gap-1 rounded-full border px-2 py-0.5 outline-none", {
  variants: {
    tone: Record.map(Theme.Palette.rows, (_row, tone) => `border-${tone}-border bg-${tone}-surface text-${tone}-text`),
    priority: Record.map(_priorityRows, (row) => row.ring),
  },
  defaultVariants: { tone: "neutral", priority: "normal" },
})

// ONE join of the two axes lives here: every pin and every board row reads its variant pair, so no surface resolves a
// tone off a priority or an emphasis off a status
const _variant = (keys: Mark.Keys): {
  readonly tone: Theme.Tone
  readonly priority: Mark.Priority
} => ({ tone: _statusRows[keys.status].tone, priority: keys.priority })

declare namespace Mark {
  type Comment = {
    readonly author: string
    readonly stamp: Option.Option<Date> // the one epoch crossing; the locale-bound render belongs to the consuming row
    readonly age: Option.Option<Duration.Duration> // absent exactly where the producer omitted the comment's instant
    readonly body: string
    readonly viewpoint: Option.Option<string>
  }
  type Row = {
    readonly guid: string
    readonly title: string
    readonly status: Mark.Status
    readonly priority: Mark.Priority
    readonly labels: Wire.BcfTopic["labels"]
    readonly assignee: Wire.BcfTopic["assignedTo"]
    readonly due: Option.Option<DateTime.Utc>
    readonly remaining: Option.Option<Either.Either<Duration.Duration, Duration.Duration>> // left overdue by, right due in
    readonly overdue: boolean
    readonly comments: ReadonlyArray<Mark.Comment>
    readonly activity: Option.Option<Duration.Duration> // age of the newest comment; absent on an unanswered topic
  }
  type Census = {
    readonly statuses: HashMap.HashMap<Mark.Status, number> // enum members key a map, never a string-keyed record
    readonly priorities: Record.ReadonlyRecord<Mark.Priority, number>
    readonly escalated: number
    readonly overdue: number
    readonly unassigned: number
    readonly comments: number
  }
  // every key is the wire's OWN spelling under indexed access, so the authored surface cannot drift from the decoded
  // owner; an omitted key is unchanged and `Option.none()` in a present key clears the field
  type Amendment = {
    readonly title?: Wire.BcfTopic["title"]
    readonly status?: Mark.Status
    readonly priority?: Wire.BcfTopic["priority"]
    readonly labels?: Wire.BcfTopic["labels"]
    readonly assignedTo?: Wire.BcfTopic["assignedTo"]
    readonly dueDate?: Option.Option<DateTime.Utc>
  }
  type Intent = Data.TaggedEnum<{
    Comment: { readonly topic: string; readonly body: string; readonly viewpoint: Option.Option<string> }
    Amend: { readonly topic: string; readonly amendment: Mark.Amendment }
  }>
}

const _Intent = Data.taggedEnum<Mark.Intent>()

const _byInstant: Order.Order<Wire.BcfTopic["comments"][number]> = Order.mapInput(
  Option.getOrder(DateTime.Order),
  (note: Wire.BcfTopic["comments"][number]) => _instant(note.date),
)

const _order: Order.Order<Mark.Row> = Order.combineAll([
  Order.mapInput(Order.reverse(Order.number), (row: Mark.Row) => _priorityRows[row.priority].rank),
  Order.mapInput(Order.boolean, (row: Mark.Row) => Option.isNone(row.due)), // dated before undated: false sorts first
  Order.mapInput(Option.getOrder(DateTime.Order), (row: Mark.Row) => row.due), // soonest first among the dated
  Order.mapInput(Order.string, (row: Mark.Row) => row.guid),
])

const _thread = (topic: Wire.BcfTopic, now: DateTime.Utc): ReadonlyArray<Mark.Comment> =>
  Array.map(Array.sort(topic.comments, _byInstant), (note) =>
    pipe(_instant(note.date), (at) => ({
      author: note.author,
      stamp: Option.map(at, Format.instant),
      age: Option.map(at, (instant) => DateTime.distanceDuration(instant, now)),
      body: Primitive.sanitize(note.text), // gated once at the fold: the row's body is the only spelling a DOM sink reaches
      viewpoint: Option.fromNullable(note.viewpointGuid),
    })))

// the keys admission is the one arm that can drop a topic, and it drops exactly the member the corpus rule refused
// before this fold could see it — so the board is total over every document a producer emits
const _board = (
  topics: ReadonlyArray<Uint8Array>,
  now: DateTime.Utc,
): Effect.Effect<ReadonlyArray<Mark.Row>, Wire.Fault | ParseResult.ParseError> =>
  Effect.map(
    Effect.forEach(
      topics,
      (topic) => Wire.decode("BcfTopicWire", topic),
      { concurrency: "inherit" },
    ),
    (admitted) =>
      Array.sort(
        Array.filterMap(admitted, (topic) =>
          Option.map(_keys(topic), (keys) =>
            pipe(
              { comments: _thread(topic, now), due: _instant(topic.dueDate) },
              ({ comments, due }) =>
                pipe(Option.map(due, (instant) => DateTime.distanceDurationEither(now, instant)), (remaining) => ({
                  guid: topic.guid,
                  title: topic.title,
                  ...keys,
                  labels: topic.labels,
                  assignee: topic.assignedTo,
                  due,
                  remaining,
                  overdue: _statusRows[keys.status].live && Option.exists(remaining, Either.isLeft),
                  comments,
                  activity: Option.flatMap(Array.last(comments), (note) => note.age),
                })),
            ))),
        _order,
      ),
  )

// seed derives from the vocabularies themselves, so a new status or priority row lands in the census with no
// edit here and no key the header reads can go missing
const _ZERO: Mark.Census = {
  statuses: HashMap.fromIterable(Array.map(_status.members, (status) => [status, 0] as const)),
  priorities: Record.map(_priorityRows, () => 0),
  escalated: 0,
  overdue: 0,
  unassigned: 0,
  comments: 0,
}

// one seeded pass answers every count the board header reads; a filter-and-length walk per column over the same row
// set is the scatter this fold deletes, and each vocabulary row decides its own column exactly once
const _census = (rows: ReadonlyArray<Mark.Row>): Mark.Census =>
  Array.reduce(rows, _ZERO, (census, row) => ({
    statuses: HashMap.modify(census.statuses, row.status, (held) => held + 1),
    priorities: { ...census.priorities, [row.priority]: census.priorities[row.priority] + 1 },
    escalated: census.escalated + (_priorityRows[row.priority].escalated ? 1 : 0),
    overdue: census.overdue + (row.overdue ? 1 : 0),
    unassigned: census.unassigned + (row.assignee === "" ? 1 : 0), // the wire carries absence as the empty string, never an Option
    comments: census.comments + row.comments.length,
  }))

declare namespace Mark {
  type Shape = {
    readonly Intent: typeof _Intent
    readonly defined: typeof _defined
    readonly instant: typeof _instant
    readonly status: typeof _statusRows
    readonly statuses: typeof _status
    readonly priority: typeof _priorityRows
    readonly priorities: typeof _priorities
    readonly keys: typeof _keys
    readonly marker: typeof _marker
    readonly variant: typeof _variant
    readonly axes: typeof _axes
    readonly framed: typeof _framed
    readonly pin: typeof _pin
    readonly ray: typeof _ray
    readonly hotspot: typeof _hotspot
    readonly mounted: typeof _mounted
    readonly moved: typeof _moved
    readonly queried: typeof _queried
    readonly restore: typeof _restore
    readonly board: typeof _board
    readonly order: typeof _order
    readonly census: typeof _census
  }
}

const Mark: Mark.Shape = {
  Intent: _Intent,
  defined: _defined,
  instant: _instant,
  status: _statusRows,
  statuses: _status,
  priority: _priorityRows,
  priorities: _priorities,
  keys: _keys,
  marker: _marker,
  variant: _variant,
  axes: _axes,
  framed: _framed,
  pin: _pin,
  ray: _ray,
  hotspot: _hotspot,
  mounted: _mounted,
  moved: _moved,
  queried: _queried,
  restore: _restore,
  board: _board,
  order: _order,
  census: _census,
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
