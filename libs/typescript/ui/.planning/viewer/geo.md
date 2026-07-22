# [UI_GEO]

The one geospatial surface-and-camera owner: one maplibre `Map` owns the WebGL context, camera, and declarative style; one `MapboxOverlay` interleaves deck.gl layers into that same context through the `IControl` rail; the layer tree is a pure value derived from the atom fold and pushed at the single `setProps` sink; and camera authority is this page's `Camera` vocabulary — one `Camera.State` across every render backend, a closed intent family as the only write path, pure screen↔world math for derived anchors. GeoArrow layers stream `apache-arrow` columns zero-copy from the explicit IPC decode seam — the decoded `Table` doubling as the multi-surface bus the chart owner consumes — tile streaming rides one engine with vector/terrain/3D-tile payload rows behind a resilient TTL cache, the discrete-global-grid cell family is one scheme-keyed table, the extension pack is an eight-capability roster on any layer, 3D relief/sky/globe are scene-config rows on the map, position capability enters through the folder-declared `Position`/`Grant` ports, and `@turf/turf` runs planar ops as the NTS-equivalent browser peer over already-decoded GeoJSON — WKB decode stays behind `core/interchange/codec`'s `WkbParser` port, and this module never parses a geometry byte. The module is `ui/viewer/src/geo.ts`.

## [01]-[CLUSTERS]

| [INDEX] | [CLUSTER]        | [OWNS]                                                                                       | [PUBLIC] |
| :-----: | :--------------- | :------------------------------------------------------------------------------------------- | :------- |
|  [01]   | `SURFACE`        | scoped map + interleaved overlay, relief/sky/globe rows, control/glyph rails, position ports | `Geo`    |
|  [02]   | `CAMERA`         | the `Camera.State` vocabulary, the closed intent family, backend adapter rows                | `Camera` |
|  [03]   | `PROJECT`        | pure screen↔world math — anchors, mercator crossings, geometry-to-intent folds               | `Camera` |
|  [04]   | `LAYER_ROWS`     | the atom-derived layer vocabulary — GeoJSON, arrow fan, tiles, cells, trips, WMS             | `Geo`    |
|  [05]   | `EXTENSION_ROWS` | the eight-capability `LayerExtension` roster on any layer                                    | `Geo`    |
|  [06]   | `PLANAR_OPS`     | the turf peer law — planar compute over decoded features                                     | —        |
|  [07]   | `STYLE_DATA`     | declarative style rows and the feature-state echo                                            | `Geo`    |

## [02]-[SURFACE]

[SURFACE]:
- Owner: `Geo.surface` — one scoped acquisition: `new MapLibreMap(options)` over the app-provided container, `new MapboxOverlay({ interleaved: true })` added through `map.addControl` (deck registers a `CustomLayerInterface` per layer into the shared context and depth buffer, so 3D deck geometry occludes against basemap layers); release removes the control — deck's full teardown rides the `IControl.onRemove` hook — then `map.remove()`: one context, one camera, one teardown order.
- Packages: `maplibre-gl` (`Map` as `MapLibreMap`, `MapOptions`, the `addSource`/`addLayer`/`addControl` rails, the scene-config verbs); `@deck.gl/mapbox` (`MapboxOverlay`); `effect` (`Effect.acquireRelease`, `Scope`).
- Law: relief, sky, and globe are scene-config rows on the one map — a `raster-dem` source through the `addSource` rail feeds `setTerrain({ source, exaggeration })`, `setSky` and `setLight` take `*Specification` data, and `setProjection({ type: "globe" })` swaps the projection without touching a layer; each row is a live re-config, never a map rebuild.
- Law: controls are rows of one `addControl(control, position)` rail — `NavigationControl`, `ScaleControl`, `TerrainControl`, `GlobeControl`, `GeolocateControl` join exactly as the overlay does; a hand-built DOM widget over a shipped control row is the named defect.
- Law: position capability is a typed port beside the DOM control — this module declares the `Position` Tag (`watch` as a coordinate stream, gated by the `Grant` permission port) and the browser composition satisfies both from the platform geolocation/permissions layers; `GeolocateControl` stays the map-chrome affordance while the port feeds the camera-follow atom and any effect-rail consumer, and a `navigator.geolocation` read in a row is the named defect.
- Law: symbol glyphs are registered material — `addImage(id, image)`/`loadImage(url)`/`addSprite(id, url)` own every custom icon and pattern a symbol layer or `mark` pin references; an inline data-URI glyph beside the registry is the named defect.
- Law: events fold into atoms — `map.on(...)` returns `Subscription`s registered as scope finalizers, and each handler body rides `useEffectEvent` where it closes over changing values so the subscription never re-binds per render; React owns only mount/unmount and the imperative map lifecycle never leaks into render. Module-level worker policy (`prewarm`, `addProtocol` for authed tile transport) is app-composition material set before the first `Map`; `transformRequest` routes tile URLs through the app's auth boundary.
- Growth: a second viewport is a second `surface` call with its own scope; the module never holds a singleton map.

```typescript
import { MapboxOverlay } from "@deck.gl/mapbox"
import { Context, Data, Effect, type Stream } from "effect"
import { Map as MapLibreMap, type MapOptions } from "maplibre-gl"

declare namespace Geo {
  type Surface = { readonly map: MapLibreMap; readonly overlay: MapboxOverlay }
  type Fix = { readonly lnglat: readonly [number, number]; readonly accuracy: number }
}

class PositionFault extends Data.TaggedError("PositionFault")<{
  readonly reason: "denied" | "unavailable" | "timeout"
}> {}

class Position extends Context.Tag("ui/viewer/Position")<Position, {
  readonly current: Effect.Effect<Geo.Fix, PositionFault>
  readonly watch: Stream.Stream<Geo.Fix, PositionFault>
}>() {}

class Grant extends Context.Tag("ui/viewer/Grant")<Grant, {
  readonly query: (name: "geolocation" | "clipboard-read") => Effect.Effect<"granted" | "denied" | "prompt">
}>() {}

const _RELIEF = { source: "relief-dem", exaggeration: 1.2 } as const

const _surface = (options: MapOptions) =>
  Effect.acquireRelease(
    Effect.sync(() => {
      const map = new MapLibreMap(options)
      const overlay = new MapboxOverlay({ interleaved: true, layers: [] })
      map.addControl(overlay)
      return { map, overlay } satisfies Geo.Surface
    }),
    (surface) =>
      Effect.sync(() => {
        surface.map.removeControl(surface.overlay)
        surface.map.remove()
      }),
  )

const _relief = (surface: Geo.Surface, demTiles: string): Effect.Effect<void> =>
  Effect.sync(() => {
    surface.map.addSource(_RELIEF.source, { type: "raster-dem", url: demTiles })
    surface.map.setTerrain({ source: _RELIEF.source, exaggeration: _RELIEF.exaggeration })
  })

const _globe = (surface: Geo.Surface): Effect.Effect<void> =>
  Effect.sync(() => void surface.map.setProjection({ type: "globe" }))
```

## [03]-[CAMERA]

[CAMERA]:
- Owner: `Camera` — the camera vocabulary spanning every backend: `Camera.State` (center `[lng, lat]`, `zoom`, `bearing`, `pitch` — the shape both the maplibre getters and deck's `MapViewState` speak), the intent family `Camera.Intent` as a closed `Data.taggedEnum` (`JumpTo` instant, `EaseTo` animated, `FlyTo` curved, `FitBounds` extent-driven, `LookAt` eye/target — the 3D viewpoint carriage `mark`'s restore mints), and the fold pair: `Camera.drive(map, intent)` dispatches onto the maplibre `Camera` verbs, `Camera.settled(map)` reads the getters into a `State` — the `moveend` subscription writes it to the atom so the store always holds the authority's last settled truth.
- Packages: `maplibre-gl` (`jumpTo`/`easeTo`/`flyTo`/`fitBounds`/`calculateCameraOptionsFromTo`, the getters); `@rasm/ts/core` (`GeoFeature.Extent` as the bounds carriage); `effect` (`Data`); `@effect-atom/atom-react` (the camera atom rides `system/atom#STORE_ROOT`).
- Law: one authority per surface — under `MapboxOverlay` the map owns pan/zoom/pitch and deck's view state syncs automatically; hand-syncing deck's camera under an overlay is the named defect; a map-less free `Deck` drives `viewState` from the same atom.
- Law: intents are the only write path — a gesture (`system/act#CONTINUOUS_OWNER`), a viewpoint restore, and a fit-to-selection all mint `Camera.Intent` values on every surface class; nothing calls a map verb outside `Camera.drive`, so camera motion is replayable and undo is `system/atom#HISTORY_FOLD` over the camera atom by construction.
- Law: intent payloads speak canonical shapes only — `FitBounds` carries the `GeoFeature.Extent` quadruple, never a maplibre bounds dialect; the maplibre arm alone respells the readonly quadruple into the map's mutable bounds at the drive boundary — the one boundary adaptation this fold carries.
- Law: `LookAt` grounds on the map through the map's own solve — `calculateCameraOptionsFromTo(eye, eyeAltitude, target, targetAltitude)` derives center, zoom, bearing, AND pitch in the map's camera model, the camera landing at the eye because zoom derives from the eye→target distance against metre altitudes; the arm spreads the solved options into `easeTo`, and a hand tangent-plane fold beside this member is the named reimplementation defect.
- Law: backend adapters translate, never own — the three arm folds control state into the atom on the `change` dispatch of the ONE control class the surface earns (`OrbitControls` for object inspection, `ArcballControls` for trackball-precision review, `MapControls` for plan-view pan-first navigation — one row each, `controls.target` follows a `LookAt` so orbit resumes around the looked-at point, position sets from `eye` through `Object3D.lookAt`); the model-viewer arm reads `getCameraOrbit()`/`getCameraTarget()` on `camera-change`, writes `cameraOrbit`/`cameraTarget`, and `jumpCameraToGoal()` settles — the element's own interpolation is respected, never fought per frame. Policy (bounds clamps, zoom limits) lives in the intent fold once, so every backend inherits it; `center` carries scene coordinates on non-geo surfaces under the same `State` shape.
- Law: the settled state publishes for cross-app taps — the camera atom is the one truth, and a non-atom consumer (a wire egress, a sibling app's probe) observes it through `Atom.toStream(camera)` — never a second `moveend` subscription and never a mirrored cell; per-app soundness holds because each app's registry scopes its own camera stream.
- Growth: a new motion kind (an orbit-around) is one intent case plus one dispatch arm per backend — consumers break loudly at the missing arm; a new control temperament is one adapter row, never a fourth camera vocabulary.

```typescript
import type { GeoFeature } from "@rasm/ts/core"
import { Data, pipe } from "effect"
import type { Map as MapLibreMap } from "maplibre-gl"

declare namespace Camera {
  type State = {
    readonly center: readonly [number, number]
    readonly zoom: number
    readonly bearing: number
    readonly pitch: number
  }
  type Eye = readonly [number, number, number]
  type Intent = Data.TaggedEnum<{
    JumpTo: { readonly state: Partial<Camera.State> }
    EaseTo: { readonly state: Partial<Camera.State>; readonly millis: number }
    FlyTo: { readonly state: Partial<Camera.State>; readonly speed: number }
    FitBounds: { readonly bounds: GeoFeature.Extent; readonly padding: number }
    LookAt: { readonly eye: Camera.Eye; readonly target: Camera.Eye; readonly millis: number }
  }>
}

const _Intent = Data.taggedEnum<Camera.Intent>()

const _payload = (state: Partial<Camera.State>) => ({
  ...(state.center !== undefined && { center: [state.center[0], state.center[1]] satisfies [number, number] }),
  ...(state.zoom !== undefined && { zoom: state.zoom }),
  ...(state.bearing !== undefined && { bearing: state.bearing }),
  ...(state.pitch !== undefined && { pitch: state.pitch }),
})

const _drive = (map: MapLibreMap, intent: Camera.Intent): void =>
  _Intent.$match(intent, {
    JumpTo: ({ state }) => void map.jumpTo(_payload(state)),
    EaseTo: ({ state, millis }) => void map.easeTo({ ..._payload(state), duration: millis }),
    FlyTo: ({ state, speed }) => void map.flyTo({ ..._payload(state), speed }),
    FitBounds: ({ bounds, padding }) => void map.fitBounds([bounds[0], bounds[1], bounds[2], bounds[3]], { padding }),
    LookAt: ({ eye, millis, target }) =>
      void map.easeTo({
        ...map.calculateCameraOptionsFromTo([eye[0], eye[1]], eye[2], [target[0], target[1]], target[2]),
        duration: millis,
      }),
  })

const _settled = (map: MapLibreMap): Camera.State =>
  pipe(map.getCenter(), (center) => ({
    center: [center.lng, center.lat] as const,
    zoom: map.getZoom(),
    bearing: map.getBearing(),
    pitch: map.getPitch(),
  }))
```

## [04]-[PROJECT]

[PROJECT]:
- Law: screen↔world is pure math — `map.project(lnglat)`/`map.unproject(point)` for live-surface reads; `WebMercatorViewport` (constructed from a `Camera.State` snapshot plus surface extent) for derived-atom anchor math — `project`/`unproject`/`fitBounds` on the immutable viewport compute pin positions and marquee extents with no live instance in the derivation; a projected point is a fixed 2-tuple, so the marked adapter asserts the bound rather than fabricating a fallback coordinate.
- Law: mercator crossings are turf rows — `toMercator`/`toWgs84` convert whole geometries at the boundary where planar compute meets the geographic camera; a hand-rolled projection formula anywhere is the named defect.
- Law: fit intents derive from geometry — `bbox(featureOrCollection)` feeds `Camera.Intent.FitBounds`, centroid targets feed `EaseTo`; geometry-to-camera is a fold from decoded features to intent values, and the wire extent is fit material as-is: an antimeridian crossing (`west > east`, the wire's own law) survives the fit because `cameraForBounds` adjusts the east limb by +360 before the camera solve. Tile-to-extent conversion happens here, never in `core`.
- Packages: `@deck.gl/core` (`WebMercatorViewport`); `@turf/turf` (`bbox`, `toMercator`, `toWgs84`); `effect` (`pipe`).

```typescript
import { WebMercatorViewport } from "@deck.gl/core"

declare namespace Anchor {
  type Extent = { readonly width: number; readonly height: number }
}

const _anchor = (state: Camera.State, extent: Anchor.Extent, lnglat: readonly [number, number]): readonly [number, number] => {
  // BOUNDARY ADAPTER
  const projected = new WebMercatorViewport({
    longitude: state.center[0],
    latitude: state.center[1],
    zoom: state.zoom,
    bearing: state.bearing,
    pitch: state.pitch,
    width: extent.width,
    height: extent.height,
  }).project([lnglat[0], lnglat[1]]) as [number, number]
  return [projected[0], projected[1]] as const
}

declare namespace Camera {
  type Shape = {
    readonly Intent: typeof _Intent
    readonly drive: typeof _drive
    readonly settled: typeof _settled
    readonly anchor: typeof _anchor
  }
}

const Camera: Camera.Shape = {
  Intent: _Intent,
  drive: _drive,
  settled: _settled,
  anchor: _anchor,
}
```

## [05]-[LAYER_ROWS]

[LAYER_ROWS]:
- Owner: `Geo.push` — the one imperative sink: the layer tree is an atom-derived `LayersList` (deck layer instances are declarative descriptors) and every change lands as `overlay.setProps({ layers })`; the overlay diffs and touches only changed GPU attributes. Two memoization planes stay orthogonal: react-compiler memoizes the tree, deck's `updateTriggers` memoizes GPU attributes — an accessor closing over an atom value names its `updateTriggers` key.
- Packages: `@deck.gl/layers` (`GeoJsonLayer`, `BitmapLayer`); `@deck.gl/geo-layers` (the tiling engine, the cell family, `TripsLayer`, `_WMSLayer`); `@geoarrow/deck.gl-geoarrow` (`GeoArrowPolygonLayer`, `initEarcutPool`); `apache-arrow` (`tableFromIPC`, `Table`, `RecordBatch`); `@rasm/ts/core` (`GeoFeature`); `effect`.
- Law: the IPC decode seam is explicit and owned here — a columnar frame enters as `tableFromIPC(bytes)` behind `Geo.decoded`, heavy frames decode off-thread on the runtime worker pool with the `Table` transferred back, and a malformed frame is a `GeoFault` on the rail, never a bare throw; downstream code holds `Table`/`RecordBatch` values only.
- Law: columnar geometry rides the GeoArrow fan — `data` per GeoArrow layer is ONE `RecordBatch`, so a chunked `Table` fans through `Table.batches` into per-batch layers; `initEarcutPool` hoists ONE pool shared by every polygon layer via `earcutWorkerPool`; picking returns the zero-copy row proxy `mark` resolves to `GlobalId`. Decoded GeoJSON features render through `GeoJsonLayer` — `pointType` and the fill/stroke/3D accessor sub-groups fan one feature stream to the whole mark vocabulary.
- Law: the decoded `Table` is a multi-surface bus — the SAME frame `Geo.decoded` mints feeds the GeoArrow layers here, the pivot engine, and the aligned-series projection at `view/chart#REGIME_LAW`; a second IPC decode of one frame is the named defect.
- Law: tile streaming is one engine with payload rows — `TileLayer.getTileData({ index, signal })` speaks the wire `GeoFeature.Tile` coordinate, the fetch rides the app's authed transport honoring the abort signal, and `renderSubLayers` projects each tile into ordinary rows bounded by the tile header's `boundingBox` (a proven `[[west,south],[east,north]]` pair — the marked adapter asserts the tuple); `MVTLayer` is the vector specialization (`binary: true`, cross-tile highlight by `uniqueIdProperty`), `TerrainLayer` reconstructs relief from an `elevationDecoder` row, `Tile3DLayer` streams 3D-tile hierarchies rendering mesh content through `scene#INSTANCED_ROWS`' pair, and `_WMSLayer` binds OGC image services — payload rows on one engine, never a second tiling machine; cache and throttle (`maxCacheByteSize`, `maxRequests`, `refinementStrategy`) are policy values.
- Law: tile fetches ride a resilient TTL cache above deck's byte cache — `Cache.make({ capacity, timeToLive, lookup })` fronts the authed transport so a pan-return re-renders from the decoded cache instead of re-fetching, retry/backoff policy composes on the lookup rail as one `Schedule` value, and deck's `maxCacheByteSize` remains the GPU-side budget — two caches, two altitudes, one lookup path; cache keys cross as `Data.struct` values so lookup identity is structural — a plain tile literal hashes referentially and turns the cache into a permanent miss.
- Law: the DGGS cell family is ONE scheme-keyed table — `S2Layer.getS2Token`, `QuadkeyLayer.getQuadkey`, `GeohashLayer.getGeohash`, `A5Layer.getPentagon`, `H3ClusterLayer.getHexagons` specialize one `GeoCellLayer` pattern by index accessor, with `H3HexagonLayer` the high-precision GPU sibling; a new grid is one table row, and the GeoArrow cell mirrors take over when the index column is an Arrow batch.
- Law: motion is an animated row — `TripsLayer` binds `getTimestamps` against a `currentTime` driven by the one rAF-fed atom clock with `_animate` set on the overlay; `trailLength`/`fadeTrail` are the decay policy, and `scene#INSTANCED_ROWS`' `_animations` rides the same clock — one animation clock across the surface.
- Law: layer assembly admits by `Crs` row — a `geographic` feature feeds a layer directly, a `projected` row crosses `toWgs84` exactly once at the assembly boundary, and an SRID `GeoFeature.Crs.of` cannot resolve renders nothing and surfaces as evidence; per-feature projection inside an accessor is the named defect.
- Growth: a new payload format is one `getTileData`/`renderSubLayers` pair; a new grid is one cell-table row; a new mark shape is one accessor sub-group on the owning row.

```typescript
import type { LayersList } from "@deck.gl/core"
import {
  A5Layer, GeohashLayer, H3ClusterLayer, H3HexagonLayer, MVTLayer, QuadkeyLayer, S2Layer,
  TerrainLayer, TileLayer, TripsLayer, _WMSLayer,
} from "@deck.gl/geo-layers"
import { BitmapLayer, GeoJsonLayer } from "@deck.gl/layers"
import { GeoArrowPolygonLayer, type initEarcutPool } from "@geoarrow/deck.gl-geoarrow"
import type { GeoFeature } from "@rasm/ts/core"
import { tableFromIPC, type RecordBatch, type Table } from "apache-arrow"
import { Array, Cache, Data, type Duration, Effect, type Schedule } from "effect"
import type { FeatureCollection } from "geojson"

class GeoFault extends Data.TaggedError("GeoFault")<{
  readonly reason: "frame-refused" | "crs-unresolved"
  readonly detail: string
}> {}

type _EarcutPool = Awaited<ReturnType<typeof initEarcutPool>>

const _decoded = (frame: Uint8Array): Effect.Effect<Table, GeoFault> =>
  Effect.try({
    try: () => tableFromIPC(frame),
    catch: (defect) => new GeoFault({ reason: "frame-refused", detail: String(defect) }),
  })

const _POINT = { radius: 4, units: "pixels" } as const

const _DECAY = { trail: 300, fade: true } as const

const _features = (id: string, collection: FeatureCollection): GeoJsonLayer =>
  new GeoJsonLayer({
    id,
    data: collection,
    pickable: true,
    stroked: true,
    filled: true,
    pointType: "circle",
    getPointRadius: _POINT.radius,
    pointRadiusUnits: _POINT.units,
  })

const _arrowFan = (id: string, table: Table, pool: _EarcutPool): LayersList =>
  Array.map(table.batches, (batch: RecordBatch, rank: number) =>
    new GeoArrowPolygonLayer({
      id: `${id}/${rank}`,
      data: batch,
      pickable: true,
      earcutWorkerPool: pool,
    }))

const _tileCache = (
  fetched: (tile: GeoFeature.Tile, signal?: AbortSignal) => Promise<ImageBitmap>,
  policy: { readonly capacity: number; readonly ttl: Duration.DurationInput; readonly retry: Schedule.Schedule<unknown, GeoFault> },
): Effect.Effect<Cache.Cache<GeoFeature.Tile, ImageBitmap, GeoFault>> =>
  Cache.make({
    capacity: policy.capacity,
    timeToLive: policy.ttl,
    lookup: (tile: GeoFeature.Tile) =>
      Effect.tryPromise({
        try: (signal) => fetched(tile, signal),
        catch: (defect) => new GeoFault({ reason: "frame-refused", detail: String(defect) }),
      }).pipe(Effect.retry(policy.retry)),
  })

const _rasterTiles = (
  id: string,
  cache: Cache.Cache<GeoFeature.Tile, ImageBitmap, GeoFault>,
  run: <A>(effect: Effect.Effect<A, GeoFault>) => Promise<A>,
): TileLayer<ImageBitmap> =>
  new TileLayer<ImageBitmap>({
    id,
    getTileData: ({ index }) => run(cache.get(Data.struct({ zoom: index.z, x: index.x, y: index.y }))),
    renderSubLayers: (props) => {
      // BOUNDARY ADAPTER
      const box = props.tile.boundingBox as [[number, number], [number, number]]
      return new BitmapLayer({
        id: `${props.id}/frame`,
        image: props.data,
        bounds: [box[0][0], box[0][1], box[1][0], box[1][1]],
      })
    },
  })

const _vectorTiles = (id: string, template: string, idProperty: string): MVTLayer =>
  new MVTLayer({ id, data: template, binary: true, pickable: true, uniqueIdProperty: idProperty })

declare namespace Cell {
  type Scheme = keyof typeof _cells
  type Row<DataT> = { readonly id: string; readonly data: ReadonlyArray<DataT>; readonly index: (row: DataT) => string }
}

const _cells = {
  s2: <D>(row: Cell.Row<D>) => new S2Layer<D>({ id: row.id, data: row.data, getS2Token: row.index, pickable: true }),
  quadkey: <D>(row: Cell.Row<D>) => new QuadkeyLayer<D>({ id: row.id, data: row.data, getQuadkey: row.index, pickable: true }),
  geohash: <D>(row: Cell.Row<D>) => new GeohashLayer<D>({ id: row.id, data: row.data, getGeohash: row.index, pickable: true }),
  a5: <D>(row: Cell.Row<D>) => new A5Layer<D>({ id: row.id, data: row.data, getPentagon: row.index, pickable: true }),
  h3: <D>(row: Cell.Row<D>) => new H3HexagonLayer<D>({ id: row.id, data: row.data, getHexagon: row.index, pickable: true }),
  h3Cluster: <D>(row: Cell.Row<D> & { readonly indexes: (r: D) => ReadonlyArray<string> }) =>
    new H3ClusterLayer<D>({ id: row.id, data: row.data, getHexagons: (r) => [...row.indexes(r)], pickable: true }),
} as const

const _trips = (
  id: string,
  paths: ReadonlyArray<{ readonly path: ReadonlyArray<readonly [number, number]>; readonly stamps: ReadonlyArray<number> }>,
  now: number,
): TripsLayer =>
  new TripsLayer({
    id,
    data: paths,
    getPath: (row) => row.path.map((point): [number, number] => [point[0], point[1]]),
    getTimestamps: (row) => [...row.stamps],
    currentTime: now,
    trailLength: _DECAY.trail,
    fadeTrail: _DECAY.fade,
  })

const _imagery = (id: string, endpoint: string, layers: ReadonlyArray<string>): _WMSLayer =>
  new _WMSLayer({ id, data: endpoint, serviceType: "wms", layers: [...layers] })

const _push = (surface: Geo.Surface, layers: LayersList): void =>
  surface.overlay.setProps({ layers })
```

## [06]-[EXTENSION_ROWS]

[EXTENSION_ROWS]:
- Owner: `Geo.extensions` — the capability roster: one `LayerExtension` instance per GPU capability joins any layer's `extensions` array — `DataFilterExtension` (time-window/range filtering through `filterRange` driven by the atom clock), `BrushingExtension` (`brushingRadius` pointer reveal), `PathStyleExtension` (`getDashArray`/`getOffset` dash and offset), `FillStyleExtension` (`getFillPattern` pattern fill), `CollisionFilterExtension` (label declutter by `getCollisionPriority`), `MaskExtension` (geofence keyed by `maskId` to a layer carrying `operation: "mask"`), `ClipExtension` (`clipBounds` rectangular clip), `_TerrainExtension` (`terrainDrawMode` drape onto the relief surface).
- Packages: `@deck.gl/extensions` (the full roster).
- Law: options versus props is the discriminant — constructor options are static shader-compilation switches set once at construction; the injected props are per-frame runtime values pushed through the same `setProps` sink, and an extension accessor closing over an atom value names its `updateTriggers` key exactly like a layer's own accessor.
- Law: a cross-layer capability is an extension instance in the array, NEVER a layer subclass or a forked prop; extensions stack — each owns a disjoint shader-module injection.
- Growth: a new capability is one factory row here; every layer inherits it by concatenation.

```typescript
import {
  BrushingExtension, ClipExtension, CollisionFilterExtension, DataFilterExtension,
  FillStyleExtension, MaskExtension, PathStyleExtension, _TerrainExtension,
} from "@deck.gl/extensions"

const _extensions = {
  filter: () => new DataFilterExtension({ filterSize: 1 }),
  brush: () => new BrushingExtension(),
  dash: () => new PathStyleExtension({ dash: true }),
  pattern: () => new FillStyleExtension({ pattern: true }),
  declutter: () => new CollisionFilterExtension(),
  mask: () => new MaskExtension(),
  clip: () => new ClipExtension(),
  drape: () => new _TerrainExtension(),
} as const
```

## [07]-[PLANAR_OPS]

[PLANAR_OPS]:
- Law: turf is the planar compute peer, render surfaces are the sink — `buffer`/`simplify`/`convex` derive overlay polygons, `union`/`intersect`/`difference` are the NTS-peer boolean overlay, and results feed a `GeoJsonLayer` row or a `GeoJSONSource.setData`; the DE-9IM predicates and hit-test rows (`booleanPointInPolygon`, `geojsonRbush`) are consumed by `mark` as settled law.
- Law: planar ONLY — turf never re-derives a spatial relation the C# side owns as authority; the two meet at the WKB/GeoJSON wire behind `WkbParser`, and a relation computed on both sides that diverges is the cross-language drift defect.
- Law: traversal rides the substrate — `coordEach`/`geomEach`/`featureEach` folds and the `getCoord`/`getGeom` accessors replace every hand coordinate loop; measurement units are the bounded `{ units }` option, never a suffixed sibling.

## [08]-[STYLE_DATA]

[STYLE_DATA]:
- Law: basemap styling is `*Specification` data — `addLayer(LayerSpecification)`, `setPaintProperty`, `setFilter` consume expression data authored as values; style edits are live re-paints, never style-swap rebuilds, and no render code hand-evaluates an expression.
- Law: hover/select echo is feature-state — `setFeatureState(feature, { selected })` drives data-driven paint without re-adding sources; the selection atom (`mark`) is the state source and this row is its basemap echo.
- Law: DOM anchors (`Marker`/`Popup`) survive only for HTML-bearing overlays (`mark`'s pins); GPU marks belong to deck rows.

```typescript
declare namespace Geo {
  type Shape = {
    readonly surface: typeof _surface
    readonly relief: typeof _relief
    readonly globe: typeof _globe
    readonly decoded: typeof _decoded
    readonly features: typeof _features
    readonly arrowFan: typeof _arrowFan
    readonly tileCache: typeof _tileCache
    readonly rasterTiles: typeof _rasterTiles
    readonly vectorTiles: typeof _vectorTiles
    readonly cells: typeof _cells
    readonly trips: typeof _trips
    readonly imagery: typeof _imagery
    readonly extensions: typeof _extensions
    readonly push: typeof _push
  }
}

const Geo: Geo.Shape = {
  surface: _surface,
  relief: _relief,
  globe: _globe,
  decoded: _decoded,
  features: _features,
  arrowFan: _arrowFan,
  tileCache: _tileCache,
  rasterTiles: _rasterTiles,
  vectorTiles: _vectorTiles,
  cells: _cells,
  trips: _trips,
  imagery: _imagery,
  extensions: _extensions,
  push: _push,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Camera, Geo, GeoFault, Grant, Position, PositionFault }
```

## [09]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
