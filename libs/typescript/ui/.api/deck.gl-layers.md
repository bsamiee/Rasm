# [API_CATALOGUE] @deck.gl/layers

`@deck.gl/layers` supplies the standard built-in layer catalogue: `ScatterplotLayer`, `GeoJsonLayer`, `PolygonLayer`, `PathLayer`, `ArcLayer`, `LineLayer`, `ColumnLayer`, `TextLayer`, `IconLayer`, `BitmapLayer`, `PointCloudLayer`, `SolidPolygonLayer`, `GridCellLayer`, plus each layer's typed props interface. All layers extend `@deck.gl/core`'s `Layer` or `CompositeLayer`.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@deck.gl/layers`
- package: `@deck.gl/layers`
- module: `@deck.gl/layers`
- asset: `dist/index.d.ts`
- rail: viewport

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: layer classes
- rail: viewport

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]   | [RAIL]                                          |
| :-----: | :------------------ | :-------------- | :---------------------------------------------- |
|   [1]   | `ScatterplotLayer`  | layer class     | filled/stroked circles at positions             |
|   [2]   | `GeoJsonLayer`      | composite class | GeoJSON features (points/lines/polygons)        |
|   [3]   | `PolygonLayer`      | layer class     | filled/extruded polygons                        |
|   [4]   | `PathLayer`         | layer class     | polyline paths                                  |
|   [5]   | `ArcLayer`          | layer class     | curved arcs between source and target positions |
|   [6]   | `LineLayer`         | layer class     | straight line segments                          |
|   [7]   | `ColumnLayer`       | layer class     | 3D columns (hexagonal/custom shape)             |
|   [8]   | `GridCellLayer`     | layer class     | rectangular grid cells (alias of `ColumnLayer`) |
|   [9]   | `TextLayer`         | composite class | text labels with font atlas                     |
|  [10]   | `IconLayer`         | layer class     | icon sprites at positions                       |
|  [11]   | `BitmapLayer`       | layer class     | raster image over a bounding box                |
|  [12]   | `PointCloudLayer`   | layer class     | 3D point cloud                                  |
|  [13]   | `SolidPolygonLayer` | layer class     | solid filled polygons (no stroke)               |

[PUBLIC_TYPE_SCOPE]: layer props interfaces
- rail: viewport

| [INDEX] | [SYMBOL]                                | [TYPE_FAMILY] | [RAIL]                        |
| :-----: | :-------------------------------------- | :------------ | :---------------------------- |
|   [1]   | `ScatterplotLayerProps<DataT>`          | type alias    | extends `LayerProps`          |
|   [2]   | `GeoJsonLayerProps<FeaturePropertiesT>` | type alias    | extends `CompositeLayerProps` |
|   [3]   | `PolygonLayerProps<DataT>`              | type alias    | extends `LayerProps`          |
|   [4]   | `PathLayerProps<DataT>`                 | type alias    | extends `LayerProps`          |
|   [5]   | `ArcLayerProps<DataT>`                  | type alias    | extends `LayerProps`          |
|   [6]   | `LineLayerProps<DataT>`                 | type alias    | extends `LayerProps`          |
|   [7]   | `ColumnLayerProps<DataT>`               | type alias    | extends `LayerProps`          |
|   [8]   | `GridCellLayerProps<DataT>`             | type alias    | extends `ColumnLayerProps`    |
|   [9]   | `TextLayerProps<DataT>`                 | type alias    | extends `CompositeLayerProps` |
|  [10]   | `IconLayerProps<DataT>`                 | type alias    | extends `LayerProps`          |
|  [11]   | `BitmapLayerProps`                      | type alias    | extends `LayerProps`          |
|  [12]   | `PointCloudLayerProps<DataT>`           | type alias    | extends `LayerProps`          |
|  [13]   | `SolidPolygonLayerProps<DataT>`         | type alias    | extends `LayerProps`          |

[PUBLIC_TYPE_SCOPE]: picking info subtypes
- rail: viewport

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY] | [RAIL]                                                 |
| :-----: | :----------------------- | :------------ | :----------------------------------------------------- |
|   [1]   | `BitmapLayerPickingInfo` | type alias    | `PickingInfo & { bitmap: { pixel, size, uv } }`        |
|   [2]   | `BitmapBoundingBox`      | type alias    | `[minLng, minLat, maxLng, maxLat]` or four-corner form |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: ScatterplotLayer key accessors
- rail: viewport

| [INDEX] | [SURFACE]                            | [ENTRY_FAMILY] | [RAIL]                                    |
| :-----: | :----------------------------------- | :------------- | :---------------------------------------- |
|   [1]   | `getPosition` accessor               | accessor prop  | `Accessor<DataT, Position>` — item center |
|   [2]   | `getRadius` accessor                 | accessor prop  | `Accessor<DataT, number>` — circle radius |
|   [3]   | `getFillColor` accessor              | accessor prop  | `Accessor<DataT, Color>`                  |
|   [4]   | `getLineColor` accessor              | accessor prop  | `Accessor<DataT, Color>` — stroke color   |
|   [5]   | `radiusUnits`, `radiusScale`         | uniform props  | size unit and scale multiplier            |
|   [6]   | `radiusMinPixels`, `radiusMaxPixels` | uniform props  | pixel-space size clamp                    |
|   [7]   | `stroked`, `filled`                  | toggle props   | stroke/fill rendering flags               |
|   [8]   | `lineWidthUnits`, `lineWidthScale`   | uniform props  | stroke width unit and scale               |

[ENTRYPOINT_SCOPE]: GeoJsonLayer key accessors
- rail: viewport

| [INDEX] | [SURFACE]                          | [ENTRY_FAMILY] | [RAIL]                                                         |
| :-----: | :--------------------------------- | :------------- | :------------------------------------------------------------- |
|   [1]   | `data` prop                        | data prop      | GeoJSON, `Feature[]`, BinaryFeatureCollection, URL, or Promise |
|   [2]   | `pointType` prop                   | dispatch prop  | `'circle' \| 'icon' \| 'text'` (default `'circle'`)            |
|   [3]   | `getFillColor` accessor            | accessor prop  | `Accessor<Feature, Color>`                                     |
|   [4]   | `getLineColor` accessor            | accessor prop  | `Accessor<Feature, Color>`                                     |
|   [5]   | `getLineWidth` accessor            | accessor prop  | `Accessor<Feature, number>`                                    |
|   [6]   | `getElevation` accessor            | accessor prop  | `Accessor<Feature, number>` — 3D extrusion                     |
|   [7]   | `filled`, `stroked`, `extruded`    | toggle props   | fill/stroke/3D extrusion flags                                 |
|   [8]   | `lineWidthUnits`, `lineWidthScale` | uniform props  | stroke width unit and scale                                    |

[ENTRYPOINT_SCOPE]: ArcLayer and LineLayer key accessors
- rail: viewport

| [INDEX] | [SURFACE]                           | [ENTRY_FAMILY] | [RAIL]                                      |
| :-----: | :---------------------------------- | :------------- | :------------------------------------------ |
|   [1]   | `getSourcePosition` accessor        | accessor prop  | `Accessor<DataT, Position>` — arc start     |
|   [2]   | `getTargetPosition` accessor        | accessor prop  | `Accessor<DataT, Position>` — arc end       |
|   [3]   | `getSourceColor` / `getTargetColor` | accessor prop  | `Accessor<DataT, Color>` — endpoint colors  |
|   [4]   | `getWidth` accessor                 | accessor prop  | `Accessor<DataT, number>` — stroke width    |
|   [5]   | `numSegments` prop                  | uniform prop   | arc tessellation resolution (ArcLayer only) |

## [4]-[IMPLEMENTATION_LAW]

[LAYERS_TOPOLOGY]:
- Every exported layer class ships with a static `defaultProps` and `layerName` on the class; pass `id` (required base `LayerProps`) for stable reconciliation
- `ScatterplotLayerProps.data` is typed as `LayerDataSource<DataT>` — accepts arrays, iterables, URLs, and Promises
- `GeoJsonLayer` is a `CompositeLayer` that spawns `ScatterplotLayer`, `PathLayer`, and `PolygonLayer` sub-layers per geometry type; `pointType` selects point sub-layer variant
- Binary data (`BinaryFeatureCollection` from `@loaders.gl/schema`) bypasses JavaScript-side feature iteration for high-volume GeoJSON
- `BitmapLayer.bounds` accepts `[minLng, minLat, maxLng, maxLat]` or four-corner `[[lng, lat], …]`; the layer projects the image over the bounding box using `WebMercatorViewport`
- `TextLayer` consumes font atlas; set `fontFamily`, `fontWeight`, `characterSet`, and `fontSettings` for custom typography
- `ColumnLayer` renders hexagons by default (`diskResolution: 6`); set `diskResolution` higher for circles, lower for polygons; `GridCellLayer` is a rectangular alias

[LOCAL_ADMISSION]:
- Always provide `id` on each layer for reconciliation across re-renders; duplicate IDs in the same `layers` array cause one layer to shadow the other.
- Memoize expensive accessor functions to prevent unnecessary attribute rebuilds on re-renders.
- For GeoJSON with many features, prefer pre-loaded `Feature[]` or `BinaryFeatureCollection` over a URL string to avoid redundant network fetches.

[RAIL_LAW]:
- Package: `@deck.gl/layers`
- Owns: standard built-in layer catalogue (scatter, geojson, polygon, path, arc, line, column, text, icon, bitmap, point-cloud, solid-polygon)
- Accept: layer props interfaces for all typing; `GeoJsonLayer` as the primary GeoJSON renderer; `ScatterplotLayer` for point clouds and scatter plots
- Reject: reimplementing geospatial primitive rendering outside these layer classes when the standard layers cover the visual
