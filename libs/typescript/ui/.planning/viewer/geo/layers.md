# [UI_LAYERS]

`viewer/geo/layers.ts` is the geospatial surface: one maplibre `Map` owns the WebGL context, camera, and declarative style; one `MapboxOverlay` interleaves deck.gl layers into that same context through the `IControl` rail; the layer tree is a pure value derived from the atom fold and pushed at the single `setProps` sink; GeoArrow layers stream `apache-arrow` columns zero-copy (one layer per `RecordBatch`, one hoisted earcut pool); tile streaming rides `TileLayer`/`MVTLayer` rows keyed by the wire tile-grid coordinate; and `@turf/turf` runs the planar ops as the NTS-equivalent browser peer over already-decoded GeoJSON — WKB decode stays in `wire` (BM:77), and this module never parses a byte.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]         | [OWNS]                                                                     |
| :-----: | :---------------- | :---------------------------------------------------------------------------- |
|   [1]   | `SURFACE_ACQUIRE` | the map + interleaved overlay as one `Scope`-bracketed resource                |
|   [2]   | `LAYER_ROWS`      | the atom-derived layer vocabulary — GeoJSON, GeoArrow fan, tile rows, extensions |
|   [3]   | `PLANAR_OPS`      | the turf peer law — planar compute over decoded features                       |
|   [4]   | `STYLE_DATA`      | declarative style rows and feature-state echo                                  |

## [2]-[SURFACE_ACQUIRE]

- Owner: `GeoLayers.surface` — one scoped acquisition: `new MapLibreMap(options)` over the app-provided container, `new MapboxOverlay({ interleaved: true })` added through `map.addControl` (deck registers a `CustomLayerInterface` per layer into the shared context and depth buffer, so 3D deck geometry occludes against basemap layers); release removes the control, calls `overlay.finalize()`, then `map.remove()` — one context, one camera, one teardown order.
- Packages: `maplibre-gl` (`Map` as `MapLibreMap`, `MapOptions`, the `addSource`/`addLayer`/`addControl` rails), `@deck.gl/mapbox` (`MapboxOverlay` — `MapboxOverlayProps` structurally forbids the camera-owning props), `effect` (`Effect.acquireRelease`, `Scope`).
- Law: the map owns the camera — the overlay syncs deck's view state from the maplibre `Camera` each `move`; a `viewState` prop, a second `Deck`, or a peer context beside the overlay is the named defect (camera intents live at `viewer/geo/project`).
- Law: events fold into atoms — `map.on(...)` returns `Subscription`s registered as scope finalizers; React owns only mount/unmount and the imperative map lifecycle never leaks into render.
- Law: module-level worker policy (`prewarm`, `addProtocol` for authed tile transport) is app-composition material set before the first `Map`; `transformRequest` routes tile URLs through the app's auth boundary.
- Growth: a second viewport is a second `surface` call with its own scope; the module never holds a singleton map.

```typescript
import { MapboxOverlay } from "@deck.gl/mapbox"
import { Effect } from "effect"
import { Map as MapLibreMap, type MapOptions } from "maplibre-gl"

declare namespace GeoLayers {
  type Surface = { readonly map: MapLibreMap; readonly overlay: MapboxOverlay }
}

const _surface = (options: MapOptions) =>
  Effect.acquireRelease(
    Effect.sync(() => {
      const map = new MapLibreMap(options)
      const overlay = new MapboxOverlay({ interleaved: true, layers: [] })
      map.addControl(overlay)
      return { map, overlay } satisfies GeoLayers.Surface
    }),
    (surface) =>
      Effect.sync(() => {
        surface.map.removeControl(surface.overlay)
        surface.overlay.finalize()
        surface.map.remove()
      }),
  )
```

## [3]-[LAYER_ROWS]

- Owner: `GeoLayers.push` — the one imperative sink: the layer tree is an atom-derived `LayersList` (pure values — deck layer instances are declarative descriptors) and every change lands as `overlay.setProps({ layers })`; the overlay diffs and touches only changed GPU attributes — the overlay is never rebuilt. Two memoization planes stay orthogonal: react-compiler memoizes the tree, deck's `updateTriggers` memoizes GPU attributes — an accessor closing over an atom value names its `updateTriggers` key.
- Law: decoded features render through `GeoJsonLayer` — the `wire` geo row delivers GeoJSON (`GeoFeature.geometry` resolved behind wire's `WkbParser` port), `pointType` and the fill/stroke/3D accessor sub-groups fan one Feature stream to the whole mark vocabulary; per-object styling is one function `Accessor` plus its trigger key, never a parallel prop.
- Law: columnar geometry rides the GeoArrow fan — `data` per GeoArrow layer is ONE `arrow.RecordBatch` (v0.4 grain), so a chunked `Table` fans through `Table.batches` into a `LayersList` of per-batch layers; `initEarcutPool` hoists ONE pool shared by every polygon layer via `earcutWorkerPool`, and its type derives as `Awaited<ReturnType<typeof initEarcutPool>>` — the transitive `threads` package is never imported; picking returns the zero-copy `StructRowProxy` the selection page resolves to `GlobalId`.
- Law: cross-layer capability is an extension row — `DataFilterExtension` (GPU time-window/range filtering driven by an atom clock through `filterRange`), `MaskExtension` (geofence keyed to a mask layer), `CollisionFilterExtension` (label declutter) join any layer's `extensions` array; constructor options bake the shader path once, runtime props ride `setProps`.
- Law: tile streaming is a layer row over the wire tile grid — `TileLayer.getTileData({ index, signal })` speaks the wire `GeoFeature.Tile` coordinate (`{zoom: z, x, y}`), the fetch rides the app's authed transport honoring the abort signal, and `renderSubLayers` projects each loaded tile into ordinary rows bounded by the tile header's `boundingBox`; vector tiles are the `MVTLayer` specialization (`binary: true`, cross-tile highlight by `uniqueIdProperty`), and `Tile3DLayer`/`TerrainLayer` join as further payload rows under the same engine — never a second tiling machine.
- Law: layer assembly admits by `Crs` row — a `geographic` feature feeds a layer directly, a `projected` row crosses `toWgs84` exactly once at the assembly boundary, and an SRID `Crs.of` cannot resolve renders nothing and surfaces as evidence; per-feature projection inside an accessor is the named defect.

```typescript
import type { LayersList } from "@deck.gl/core"
import { MVTLayer, TileLayer } from "@deck.gl/geo-layers"
import { BitmapLayer, GeoJsonLayer } from "@deck.gl/layers"
import { GeoArrowPolygonLayer, type initEarcutPool } from "@geoarrow/deck.gl-geoarrow"
import type { GeoFeature } from "@rasm/ts/wire/vocab"
import { Array } from "effect"
import type { RecordBatch, Table } from "apache-arrow"
import type { FeatureCollection } from "@turf/turf"

type _EarcutPool = Awaited<ReturnType<typeof initEarcutPool>>

const _features = (id: string, collection: FeatureCollection): GeoJsonLayer =>
  new GeoJsonLayer({
    id,
    data: collection,
    pickable: true,
    stroked: true,
    filled: true,
    pointType: "circle",
    getPointRadius: 4,
    pointRadiusUnits: "pixels",
  })

const _arrowFan = (id: string, table: Table, pool: _EarcutPool): LayersList =>
  Array.map(table.batches, (batch: RecordBatch, rank: number) =>
    new GeoArrowPolygonLayer({
      id: `${id}/${rank}`,
      data: batch,
      pickable: true,
      earcutWorkerPool: pool,
    }))

const _rasterTiles = (id: string, fetched: (tile: GeoFeature.Tile, signal?: AbortSignal) => Promise<ImageBitmap>): TileLayer<ImageBitmap> =>
  new TileLayer<ImageBitmap>({
    id,
    getTileData: ({ index, signal }) => fetched({ zoom: index.z, x: index.x, y: index.y }, signal), // the deck index speaks the wire tile coordinate; the abort signal crosses with the fetch
    renderSubLayers: (props) =>
      new BitmapLayer({
        id: `${props.id}/frame`,
        image: props.data,
        bounds: [props.tile.boundingBox[0][0]!, props.tile.boundingBox[0][1]!, props.tile.boundingBox[1][0]!, props.tile.boundingBox[1][1]!], // BOUNDARY ADAPTER: the header's min/max pair is the bounds evidence the checker cannot carry
      }),
  })

const _vectorTiles = (id: string, template: string, idProperty: string): MVTLayer =>
  new MVTLayer({ id, data: template, binary: true, pickable: true, uniqueIdProperty: idProperty })

const _push = (surface: GeoLayers.Surface, layers: LayersList): void =>
  surface.overlay.setProps({ layers })
```

## [4]-[PLANAR_OPS]

- Law: turf is the planar compute peer, render surfaces are the sink — `buffer`/`simplify`/`convex` derive overlay polygons, `union`/`intersect`/`difference` are the NTS-peer boolean overlay, and results feed a `GeoJsonLayer` row or a `GeoJSONSource.setData`; the DE-9IM predicates and hit-test rows (`booleanPointInPolygon`, `geojsonRbush`) are consumed by `viewer/mark/selection` as settled law.
- Law: planar ONLY — turf never re-derives a spatial relation the C# side owns as authority; the two meet at the WKB/GeoJSON wire, and a relation computed on both sides that could diverge is a cross-language drift defect (BM:77's label).
- Law: traversal rides the substrate — `coordEach`/`geomEach`/`featureEach` folds and the `getCoord`/`getGeom` accessors replace every hand coordinate loop; measurement units are the bounded `{ units }` option, never a suffixed sibling.

## [5]-[STYLE_DATA]

- Law: basemap styling is `*Specification` data — `addLayer(LayerSpecification)`, `setPaintProperty`, `setFilter` consume expression data authored as values; style edits are live re-paints, never style-swap rebuilds, and no render code hand-evaluates an expression.
- Law: hover/select echo is feature-state — `setFeatureState(feature, { selected })` drives data-driven paint without re-adding sources; the selection atom (`viewer/mark/selection`) is the state source and this row is its basemap echo.
- Law: DOM anchors (`Marker`/`Popup`) survive only for HTML-bearing overlays (BCF pins at `viewer/mark/bcf`); GPU marks belong to deck rows.

```typescript
const GeoLayers: {
  readonly surface: typeof _surface
  readonly features: typeof _features
  readonly arrowFan: typeof _arrowFan
  readonly rasterTiles: typeof _rasterTiles
  readonly vectorTiles: typeof _vectorTiles
  readonly push: typeof _push
} = {
  surface: _surface,
  features: _features,
  arrowFan: _arrowFan,
  rasterTiles: _rasterTiles,
  vectorTiles: _vectorTiles,
  push: _push,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { GeoLayers }
```
