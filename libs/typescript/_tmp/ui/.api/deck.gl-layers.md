# [API_CATALOGUE] @deck.gl/layers

`@deck.gl/layers` supplies the standard built-in layer catalogue over the `@deck.gl/core` engine: `ScatterplotLayer`, `GeoJsonLayer`, `PolygonLayer`, `SolidPolygonLayer`, `PathLayer`, `ArcLayer`, `LineLayer`, `ColumnLayer`, `GridCellLayer`, `TextLayer`, `IconLayer`, `BitmapLayer`, `PointCloudLayer`, plus each layer's typed props interface. Every layer is a `Layer` or `CompositeLayer` and speaks the `@deck.gl/core` accessor vocabulary (`Accessor`/`AccessorFunction`, `Position`, `Color`, `Unit`) — the per-layer surface is one geometry accessor, the shared color/size vocabulary, and a small distinct-accessor set. `GeoJsonLayer` is the `render/geo.md` `GeoSeriesLayer` `geojson`-arm renderer for bounded in-memory collections; out-of-core feature rendering is `@deck.gl/geo-layers` (`MVTLayer`) or `@geoarrow/deck.gl-geoarrow` (Arrow `RecordBatch`).

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@deck.gl/layers`
- package: `@deck.gl/layers`
- module: `@deck.gl/layers`
- asset: `dist/index.d.ts`
- baseline: deck.gl v9 (luma.gl v9, WebGL2 default context)
- rail: viewport

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: layer classes
- rail: viewport

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY]   | [RAIL]                                                        |
| :-----: | :-------------------- | :-------------- | :----------------------------------------------------------- |
|  [01]   | `ScatterplotLayer`    | layer class     | filled/stroked circles at positions                          |
|  [02]   | `GeoJsonLayer`        | composite class | GeoJSON features (points/lines/polygons); spawns sub-layers  |
|  [03]   | `PolygonLayer`        | composite class | filled + stroked/extruded polygons (wraps solid-polygon + path) |
|  [04]   | `SolidPolygonLayer`   | layer class     | solid filled/extruded polygons, no stroke                    |
|  [05]   | `PathLayer`           | layer class     | polyline paths                                               |
|  [06]   | `ArcLayer`            | layer class     | curved/great-circle arcs between source and target           |
|  [07]   | `LineLayer`           | layer class     | straight line segments                                       |
|  [08]   | `ColumnLayer`         | layer class     | 3D columns (n-gon disk, hexagonal by default)                |
|  [09]   | `GridCellLayer`       | layer class     | square-disk `ColumnLayer` subclass (grid cells)              |
|  [10]   | `TextLayer`           | composite class | text labels over a font atlas (`_MultiIconLayer` + background) |
|  [11]   | `IconLayer`           | layer class     | icon sprites at positions (atlas + mapping)                  |
|  [12]   | `BitmapLayer`         | layer class     | raster image over a bounding box                             |
|  [13]   | `PointCloudLayer`     | layer class     | 3D point cloud with normals                                  |
|  [14]   | `_MultiIconLayer`     | layer class     | per-character glyph layer under `TextLayer` (experimental)   |
|  [15]   | `_TextBackgroundLayer`| layer class     | text background quad layer under `TextLayer` (experimental)  |

[PUBLIC_TYPE_SCOPE]: layer props interfaces
- rail: viewport

| [INDEX] | [SYMBOL]                                | [TYPE_FAMILY] | [RAIL]                        |
| :-----: | :-------------------------------------- | :------------ | :---------------------------- |
|  [01]   | `ScatterplotLayerProps<DataT>`          | type alias    | extends `LayerProps`          |
|  [02]   | `GeoJsonLayerProps<FeaturePropertiesT>` | type alias    | extends `CompositeLayerProps` |
|  [03]   | `PolygonLayerProps<DataT>`              | type alias    | extends `CompositeLayerProps` |
|  [04]   | `SolidPolygonLayerProps<DataT>`         | type alias    | extends `LayerProps`          |
|  [05]   | `PathLayerProps<DataT>`                 | type alias    | extends `LayerProps`          |
|  [06]   | `ArcLayerProps<DataT>`                  | type alias    | extends `LayerProps`          |
|  [07]   | `LineLayerProps<DataT>`                 | type alias    | extends `LayerProps`          |
|  [08]   | `ColumnLayerProps<DataT>`               | type alias    | extends `LayerProps`          |
|  [09]   | `GridCellLayerProps<DataT>`             | type alias    | `_GridCellLayerProps & ColumnLayerProps<DataT>` |
|  [10]   | `TextLayerProps<DataT>`                 | type alias    | `_TextLayerProps<DataT> & LayerProps` |
|  [11]   | `IconLayerProps<DataT>`                 | type alias    | extends `LayerProps`          |
|  [12]   | `BitmapLayerProps`                      | type alias    | extends `LayerProps`          |
|  [13]   | `PointCloudLayerProps<DataT>`           | type alias    | extends `LayerProps`          |
|  [14]   | `MultiIconLayerProps<DataT>`            | type alias    | `IconLayerProps` + glyph layout (experimental sub-layer) |
|  [15]   | `TextBackgroundLayerProps<DataT>`       | type alias    | text background quad props (experimental sub-layer) |

[PUBLIC_TYPE_SCOPE]: picking info subtypes
- rail: viewport

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY] | [RAIL]                                                 |
| :-----: | :----------------------- | :------------ | :----------------------------------------------------- |
|  [01]   | `BitmapLayerPickingInfo` | type alias    | `PickingInfo & { bitmap: { pixel, size, uv } }`        |
|  [02]   | `BitmapBoundingBox`      | type alias    | `[minX, minY, maxX, maxY]` or four-corner `[[x,y],…]`  |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: shared accessor and uniform vocabulary (parameterized across every layer)
- rail: viewport
- law: the layer surface is not a per-layer accessor roster — it is four parameterized patterns from `@deck.gl/core`. Read a layer as `{ geometry accessor } + { color accessors } + { size-clamp quadruple for its dimension } + { toggle set }`, then add only its distinct-accessor row below.

| [INDEX] | [PATTERN]                                    | [ENTRY_FAMILY] | [RAIL]                                                                      |
| :-----: | :------------------------------------------- | :------------- | :-------------------------------------------------------------------------- |
|  [01]   | geometry accessor                            | accessor prop  | `getPosition` (point) / `getSourcePosition`+`getTargetPosition` (arc/line) / `getPath` / `getPolygon` — `Accessor<DataT, Position>` or `AccessorFunction<DataT, PathGeometry \| PolygonGeometry>` |
|  [02]   | color accessors                              | accessor props | `getFillColor` / `getLineColor` / `getColor` / `getSourceColor` / `getTargetColor` — all `Accessor<DataT, Color>` (`Color \| Color[]` for `PathLayer`) |
|  [03]   | size-clamp quadruple `{dim}{Units,Scale,MinPixels,MaxPixels}` | uniform props | `dim ∈ {radius, lineWidth, width, size, pointRadius}`: `{dim}Units: Unit`, `{dim}Scale: number`, `{dim}MinPixels`/`{dim}MaxPixels: number` — one quadruple per dimension a layer draws |
|  [04]   | toggle set                                   | toggle props   | `filled` / `stroked` / `extruded` / `wireframe` / `billboard` / `antialiasing` — plus base `pickable`/`visible`/`opacity` from `LayerProps` |

[ENTRYPOINT_SCOPE]: per-layer distinct accessors and props (only the delta over the shared vocabulary)
- rail: viewport

| [INDEX] | [LAYER]             | [DISTINCT SURFACE]                                                                              |
| :-----: | :------------------ | :--------------------------------------------------------------------------------------------- |
|  [01]   | `ScatterplotLayer`  | `getRadius: Accessor<DataT, number>`; `radius` quadruple + `lineWidth` quadruple; `filled`/`stroked`/`billboard`/`antialiasing` |
|  [02]   | `GeoJsonLayer`      | `pointType: 'circle'\|'icon'\|'text'` (`+`-joined); `getPointRadius`/`pointRadius` quadruple; icon sub-accessors (`getIcon`/`iconAtlas`/`iconMapping`), text sub-accessor (`getText`); `getElevation`/`extruded`/`wireframe`/`_full3d` |
|  [03]   | `PolygonLayer`      | `getPolygon`; `getElevation`; `filled`/`stroked`/`extruded`/`wireframe`/`elevationScale`; `_normalize`/`_windingOrder` winding controls |
|  [04]   | `ArcLayer`          | `getHeight`/`getTilt`/`getWidth`; `numSegments`; `greatCircle?: boolean` (folds in `GreatCircleLayer`); `width` quadruple |
|  [05]   | `LineLayer`         | `getWidth`; `width` quadruple                                                                   |
|  [06]   | `ColumnLayer`       | `getElevation`/`getFillColor`/`getLineColor`/`getLineWidth`; `diskResolution` (default `6`, hexagon)/`radius`/`angle`/`offset`/`coverage`/`vertices`; `extruded`/`wireframe`/`flatShading` |
|  [07]   | `TextLayer`         | `getText`; `getTextAnchor`/`getAlignmentBaseline`/`getPixelOffset`/`getAngle`; `fontFamily`/`fontWeight`/`characterSet`/`fontSettings`/`lineHeight`/`outlineWidth`/`outlineColor`/`wordBreak`/`maxWidth`; `background`+`getBackgroundColor`/`getBorderColor`/`getBorderWidth`; `size` quadruple |
|  [08]   | `IconLayer`         | `getIcon`/`getSize`/`getColor`/`getAngle`/`getPixelOffset`; `iconAtlas`/`iconMapping`; `billboard`/`alphaCutoff`; `size` quadruple |
|  [09]   | `PointCloudLayer`   | `getNormal: Accessor<DataT, [x,y,z]>`/`getColor`; `pointSize` (scalar)/`sizeUnits`; `material` |
|  [10]   | `BitmapLayer`       | `image: string \| TextureSource`; `bounds: BitmapBoundingBox`; `desaturate`/`transparentColor`/`tintColor`; `_imageCoordinateSystem` |

## [04]-[IMPLEMENTATION_LAW]

[LAYERS_TOPOLOGY]:
- Every exported layer class carries a static `defaultProps` and `layerName`; `id` (base `LayerProps`) is required for stable reconciliation across re-renders — a duplicate `id` in one `layers` array shadows a layer.
- Data is typed `LayerDataSource<DataT>` (from `@deck.gl/core`): arrays, iterables, `AsyncIterable`, string URLs, or Promises; `BinaryFeatureCollection` (`@loaders.gl/schema`) bypasses JavaScript-side feature iteration for high-volume data.
- `GeoJsonLayer` is a `CompositeLayer` spawning `ScatterplotLayer`/`IconLayer`/`TextLayer` (per `pointType`), `PathLayer`, and `SolidPolygonLayer` sub-layers by geometry type; `PolygonLayer` composes `SolidPolygonLayer` (fill/extrusion) + `PathLayer` (stroke).
- `ArcLayer` renders great-circle geodesics with `greatCircle: true` — `@deck.gl/geo-layers`' `GreatCircleLayer` is a deprecated alias of exactly this; prefer the prop.
- `ColumnLayer` renders an n-gon disk (`diskResolution: 6` → hexagon; raise for circles, `3` for triangles); `GridCellLayer` is its square-disk subclass, not a re-export.
- `BitmapLayer.bounds` accepts `[minX, minY, maxX, maxY]` or four-corner `[[x,y],…]`; the layer projects the image over the box in the active `coordinateSystem`.
- `TextLayer` builds a font atlas and draws through internal `_MultiIconLayer` (per-glyph) + `_TextBackgroundLayer` (background quad); set `fontFamily`/`fontWeight`/`characterSet`/`fontSettings` for custom typography.

[STACKING_LAW]:
- Every class here subclasses `@deck.gl/core`'s `Layer`/`CompositeLayer` (`deck.gl-core.md`) — these are drawn primitives, never a renderer. A layer enters a `LayersList` (a composite class returns one from `renderLayers(): Layer | null | LayersList`) that a `Deck` rasterizes under a `View`/`Viewport` from core; the `render/geo.md` `GeoSeriesLayer` `Data.TaggedEnum` `$match`/`Match.value(source)` fold builds that `LayersList` and mounts it under the `@deck.gl/mapbox` `MapboxOverlay` interleave — one overlay owns every layer, never a free `Deck`.
- `GeoJsonLayer` is the `render/geo.md` `GeoSeriesLayer` `geojson`-arm renderer, constructed inside the `effect` `Match.value(source)` dispatch for **bounded in-memory** `GeoJSON.FeatureCollection` collections fed by the `interchange` `GeometryRail` GeoJSON projection. Out-of-core rendering routes to `@deck.gl/geo-layers` `MVTLayer` (`tile` arm) or `@geoarrow/deck.gl-geoarrow` Arrow layers (`geoarrow` arm) — re-materializing an Arrow `RecordBatch` the `GeometryRail` already projected into a `GeoJsonLayer` is the named `geo.md` defect.
- Every layer's `Accessor`/`Color`/`Position`/`Unit` vocabulary is owned by `@deck.gl/core`; type against `@deck.gl/core`'s `Accessor<In, Out>` at the accessor boundary, never a local color/position alias.
- Accessor closures are re-run on the `AccessorFunction` path per datum; memoize accessors bound to `binding/atom.md` `AtomBinding` reactive state so an attribute rebuild fires only on real data change, and prefer the `updateTriggers` prop over identity churn.

[LOCAL_ADMISSION]:
- Always provide `id`; memoize expensive accessor functions and drive attribute invalidation through `updateTriggers` rather than new closure identities.
- For many-feature GeoJSON, prefer pre-loaded `Feature[]` or `BinaryFeatureCollection` over a URL string; escalate to `MVTLayer`/GeoArrow when the set is out-of-core.
- Read the shared vocabulary once (geometry accessor + color accessors + size-clamp quadruple + toggle set); reach for a distinct-accessor row only for the layer's genuine delta.

[RAIL_LAW]:
- Package: `@deck.gl/layers`
- Owns: the standard built-in layer catalogue (scatter, geojson, polygon, solid-polygon, path, arc, line, column, grid-cell, text, icon, bitmap, point-cloud) and each typed props interface
- Accept: layer props interfaces for all typing; `GeoJsonLayer` as the bounded-collection GeoJSON renderer; `ScatterplotLayer` for point/scatter; `ArcLayer` with `greatCircle: true` for geodesics
- Reject: reimplementing a geospatial primitive when a standard layer covers the visual; per-layer restatement of the shared color/size vocabulary; `GeoJsonLayer` for an out-of-core dataset that belongs on `MVTLayer` or a GeoArrow layer
