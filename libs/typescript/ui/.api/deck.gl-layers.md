# [TS_UI_API_DECK_GL_LAYERS]

`@deck.gl/layers` is the primitive layer vocabulary the `ui/viewer/geo/layers` plane instantiates over the `@deck.gl/core` engine: point marks (`ScatterplotLayer`, `PointCloudLayer`, `IconLayer`, `TextLayer`, `ColumnLayer`), path marks (`PathLayer`, `LineLayer`, `ArcLayer`), area marks (`PolygonLayer`, `SolidPolygonLayer`), a raster mark (`BitmapLayer`), and the omnibus `GeoJsonLayer` that composes point/line/area sublayers from a single Feature stream. Every layer is `XxxLayerProps<DataT> = _XxxLayerProps<DataT> & (LayerProps | CompositeLayerProps)`, generic over the row type — one shape law, one inherited base. Styling is not per-layer invention: every `get*` prop is a core `Accessor<In,Out>` (uniform value or per-object function with a reusable `target`), and the `*Units`/`*Scale`/`*MinPixels`/`*MaxPixels` quartet is one radius/width normalization axis shared across scatterplot/path/polygon/arc/line/column. This catalog documents only the distinctive accessors and toggles each layer adds; the base `LayerProps` axes (`data`, `pickable`, `updateTriggers`, `transitions`, `extensions`, `coordinateSystem`, the pointer-callback family) and the `Accessor`/`Position`/`Color`/`Unit` types live in `.api/deck.gl-core.md`. `scope:viewer` project-local.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@deck.gl/layers`
- package: `@deck.gl/layers`
- license: `MIT`
- abi: browser WebGL2/WebGPU via `@deck.gl/core`'s luma.gl `Device`
- peer: `@deck.gl/core catalog` (the `Layer`/`CompositeLayer` bases, `Accessor`/`Position`/`Color`/`Unit`/`Material` types, picking), `@loaders.gl/core` (image/font/mesh loaders for `IconLayer`/`TextLayer`/`BitmapLayer`), `@luma.gl/core` (`Texture`/`SamplerProps`)
- catalog-verdict: KEEP — the base render vocabulary; `@deck.gl/geo-layers` composite layers extend these (`GeoCellLayer`→`PolygonLayer`, `TripsLayer`→`PathLayer`, `MVTLayer`→`GeoJsonLayer`)
- runtime: `scope:viewer` project-local; layer instances are declarative values in the `Deck.layers` array
- modules: `ScatterplotLayer`, `PointCloudLayer`, `IconLayer`, `TextLayer`, `ColumnLayer`, `GridCellLayer`, `PathLayer`, `LineLayer`, `ArcLayer`, `PolygonLayer`, `SolidPolygonLayer`, `BitmapLayer`, `GeoJsonLayer`

## [02]-[POINT_MARKS]

[TYPE_SCOPE]: position-anchored marks — one row per data object at a `getPosition`, styled by `getFillColor`/`getRadius`/`getColor` accessors; the radius/size unit-normalization axis is core-shared.
- `ScatterplotLayer` is the workhorse circle; `ColumnLayer` extrudes a regular polygon disk (with `GridCellLayer` fixing a square footprint); `IconLayer`/`TextLayer` are the atlas-backed marks (raster sprites, SDF glyphs) that own a loaders.gl-fed atlas manager; `PointCloudLayer` is the lit point primitive with per-point normals.

| [INDEX] | [SYMBOL]                       | [CONSUMER_BOUNDARY]                                  |
| :-----: | :----------------------------- | :--------------------------------------------------- |
|  [01]   | `ScatterplotLayer<DataT>`      | point features → circles; the default geo point mark |
|  [02]   | `PointCloudLayer<DataT>`       | lit 3D point clouds (LiDAR, scan)                    |
|  [03]   | `IconLayer<DataT>`             | raster sprite markers; `IconManager` owns the atlas  |
|  [04]   | `TextLayer<DataT>` (composite) | SDF glyph labels; `FontAtlasManager` owns glyphs     |
|  [05]   | `ColumnLayer<DataT>`           | extruded hex/cylinder columns (3D bar maps)          |
|  [06]   | `GridCellLayer<DataT>`         | square-footprint grid cells (aggregation viz)        |

[DISTINCTIVE_PROPS] by row (accessors + toggles each mark adds over the core base):
- [01]-[SCATTERPLOTLAYER]: `getPosition`, `getRadius`, `getFillColor`, `getLineColor`, `getLineWidth`, `getPixelOffset`; `stroked`/`filled`/`billboard`/`antialiasing`, `radius{Units,Scale,MinPixels,MaxPixels}`.
- [02]-[POINTCLOUDLAYER]: `getPosition`, `getNormal`, `getColor`; `pointSize`, `sizeUnits`, `material`.
- [03]-[ICONLAYER]: `getPosition`, `getIcon`, `getSize`, `getColor`, `getAngle`, `getPixelOffset`; `iconAtlas`/`iconMapping` or auto-packed atlas, `billboard`, `sizeUnits`.
- [04]-[TEXTLAYER]: `getText`, `getPosition`, `getColor`, `getSize`, `getAngle`, `getTextAnchor`, `getAlignmentBaseline`, `getPixelOffset`; `fontFamily`/`fontSettings`/`characterSet`, `background`/`outlineWidth`; renders `_MultiIconLayer`+`_TextBackgroundLayer`.
- [05]-[COLUMNLAYER]: `getPosition`, `getFillColor`, `getLineColor`, `getElevation`, `getLineWidth`; `diskResolution`, `radius`, `angle`, `vertices`, `coverage`, `extruded`/`stroked`/`filled`/`wireframe`/`flatShading`, `elevationScale`, `material`.
- [06]-[GRIDCELLLAYER]: `ColumnLayer` + `cellSize`.

## [03]-[PATH_AND_AREA_MARKS]

[TYPE_SCOPE]: multi-vertex marks — paths and polygons tesselated on the GPU; source/target-pair marks (`ArcLayer`/`LineLayer`) vs vertex-list marks (`PathLayer`/`PolygonLayer`).
- `PathLayer` extrudes mitered polylines; `LineLayer`/`ArcLayer` render straight/curved segments between a `getSourcePosition` and `getTargetPosition` pair. `PolygonLayer` (composite) draws fill + stroke + optional 3D extrusion by delegating to `SolidPolygonLayer` (the tesselated fill primitive) and `PathLayer` (the outline) — the fill/stroke separation the cell family and GeoJSON reuse.

| [INDEX] | [SYMBOL]                          | [CONSUMER_BOUNDARY]                                           |
| :-----: | :-------------------------------- | :------------------------------------------------------------ |
|  [01]   | `PathLayer<DataT>`                | extruded polylines; `PathTesselator` owns geometry            |
|  [02]   | `LineLayer<DataT>`                | straight great-line segments (OD flows)                       |
|  [03]   | `ArcLayer<DataT>`                 | curved arcs; `greatCircle:true` = shortest-path               |
|  [04]   | `PolygonLayer<DataT>` (composite) | fill+stroke+extrusion; base for `GeoCellLayer`/`GeoJsonLayer` |
|  [05]   | `SolidPolygonLayer<DataT>`        | tesselated fill primitive; `PolygonTesselator` (earcut)       |

[DISTINCTIVE_PROPS] by row:
- [01]-[PATHLAYER]: `getPath`, `getColor`, `getWidth`; `width{Units,Scale,MinPixels,MaxPixels}`, `capRounded`, `jointRounded`, `miterLimit`, `billboard`.
- [02]-[LINELAYER]: `getSourcePosition`, `getTargetPosition`, `getColor`, `getWidth`; `width{Units,Scale,MinPixels,MaxPixels}`.
- [03]-[ARCLAYER]: `getSourcePosition`, `getTargetPosition`, `getSourceColor`, `getTargetColor`, `getWidth`, `getHeight`, `getTilt`; `greatCircle`, `numSegments`, `width*` (supersedes `GreatCircleLayer`).
- [04]-[POLYGONLAYER]: `getPolygon`, `getFillColor`, `getLineColor`, `getLineWidth`, `getElevation`; `stroked`/`filled`/`extruded`/`wireframe`, `elevationScale`, `lineJointRounded`/`lineMiterLimit`, `material`, `_normalize`/`_windingOrder`.
- [05]-[SOLIDPOLYGONLAYER]: `getPolygon`, `getFillColor`, `getLineColor`, `getElevation`; `extruded`/`wireframe`, `elevationScale`, `material`, `_normalize`/`_windingOrder`/`_full3d`.

## [04]-[RASTER_AND_OMNIBUS]

[TYPE_SCOPE]: the image mark and the Feature-stream composite that dispatches to every mark above.
- `BitmapLayer` textures an image over a geographic quad. `GeoJsonLayer` is the omnibus composite: one GeoJSON/`Feature[]`/`BinaryFeatureCollection` stream fans out to point (`pointType: 'circle'|'icon'|'text'`), line, and fill sublayers, exposing each sublayer's accessors under a namespaced prefix (`getPointRadius`, `getLineColor`, `getFillColor`, `getElevation`). Its prop record composes six sub-groups (fill, stroke, 3D, pointCircle, iconPoint, textPoint) — the parameterized dispatch that turns one Feature collection into the full mark vocabulary.

| [INDEX] | [SYMBOL]                                   | [CONSUMER_BOUNDARY]                                        |
| :-----: | :----------------------------------------- | :--------------------------------------------------------- |
|  [01]   | `BitmapLayer`                              | raster overlays; `BitmapLayerPickingInfo` adds `bitmap` uv |
|  [02]   | `GeoJsonLayer<FeatProps>` (composite)      | primary geo Feature renderer; `wire` WKB→GeoJSON feeds it  |
|  [03]   | `BitmapBoundingBox`                        | rectangular or 4-corner (skewed) image placement           |
|  [04]   | `_MultiIconLayer` / `_TextBackgroundLayer` | glyph run + label background; not instantiated directly    |

[DISTINCTIVE_PROPS] by row:
- [01]-[BITMAPLAYER]: `image` (URL/`TextureSource`), `bounds: BitmapBoundingBox`, `getBounds`; `desaturate`, `transparentColor`, `tintColor`, `textureParameters`, `_imageCoordinateSystem`; `data: never`, `BitmapLayerPickingInfo` adds `bitmap` uv.
- [02]-[GEOJSONLAYER]: `data: GeoJSON|Feature[]|BinaryFeatureCollection`; `pointType: 'circle'|'icon'|'text'` (+`+`-join); fill (`getFillColor`,`filled`), stroke (`getLineColor`,`getLineWidth`,`stroked`,`lineWidthUnits`), 3D (`getElevation`,`extruded`,`wireframe`), point (`getPointRadius`,`pointRadius{Units,Scale,MinPixels,MaxPixels}`), icon (`getIcon`,`iconAtlas`), text (`getText`,`getTextSize`).
- [03]-[BITMAPBOUNDINGBOX]: `[left,bottom,right,top] | [Position,Position,Position,Position]`.
- [04]-[_MultiIconLayer / _TextBackgroundLayer]: `TextLayer` internals — glyph run + label background, not instantiated directly.

## [05]-[IMPLEMENTATION_LAW]

[VOCABULARY_TOPOLOGY]:
- one shape law: every layer is `_XxxProps<DataT> & (LayerProps | CompositeLayerProps)`, generic over `DataT`. A new mark is a `Layer`/`CompositeLayer` subclass with distinctive `get*` accessors, never a fork of an existing layer's props.
- accessor is the styling collapse: per-object color/size/position is one function `Accessor<In,Out>` closing over the row + `AccessorContext`; a data-driven style is `getFillColor: d => scale(d.value)` + an `updateTriggers.getFillColor` key, never a parallel prop. The unit quartet (`*Units`/`*Scale`/`*MinPixels`/`*MaxPixels`) is one normalization axis — do not re-derive per layer.
- fill/stroke separation: `PolygonLayer` = `SolidPolygonLayer` (GPU-tesselated fill) + `PathLayer` (mitered outline); the cell family and `GeoJsonLayer` inherit this split. Extrusion (`extruded` + `getElevation` + `material`) reuses the core `LightingEffect`.
- composite dispatch: `GeoJsonLayer` is the parameterized entry — one Feature stream, `pointType` discriminant, six accessor sub-groups fanning to the mark sublayers. Prefer it over hand-wiring per-geometry layers when the source is a mixed Feature collection.

[INTEGRATION_LAW]:
- Stack on `@deck.gl/core`: these layers ARE the `Layer`/`CompositeLayer` vocabulary; the base classes, `Accessor`/`Position`/`Color`/`Unit`/`Material` types, picking, and the `Deck` they render into all live in core. `data` accepts the core `LayerDataSource` (URL/promise/async-batch/binary-columnar) directly.
- Stack with `wire` decode + `@turf/turf` (`.api/turf-turf.md`): `wire` decodes WKB→GeoJSON (decode stays in `wire`); `@turf/turf` runs the CPU-side planar-op stage on that GeoJSON — `buffer`/`simplify`, the `union`/`intersect`/`difference` boolean overlay, `bbox`→`bboxPolygon` extent, `voronoi`/`isobands` surfaces — and the resulting `Feature`/`FeatureCollection` feeds `GeoJsonLayer.data` or `PolygonLayer.getPolygon`. Turf is the pre-GPU planar transform, deck the GPU rasterizer; the interaction-scale derived-overlay path, distinct from the bulk geoarrow columnar route.
- Stack with `@geoarrow/deck.gl-geoarrow` + `apache-arrow` (`.api/geoarrow-deck.gl-geoarrow.md`, `.api/apache-arrow.md`): for large columnar data, the GeoArrow layer mirrors (`GeoArrowScatterplotLayer`/`GeoArrowPathLayer`/`GeoArrowPolygonLayer`/`GeoArrowSolidPolygonLayer`) consume an Arrow `RecordBatch` (catalog-bound grain: `data` is one `RecordBatch`, the caller fans `Table.batches` to one layer each) with zero per-row JS objects — the same accessor semantics, columnar input. Reach for the geoarrow mirror when `data` is an Arrow `RecordBatch`, these plain layers when it is a JS object stream.
- Stack with `@effect-atom`: layer instances are pure values built inside a derived atom from the state fold; the `layers` array is recomputed on state change and pushed via `Deck.setProps`. `updateTriggers` keys track which closed-over atom values force GPU attribute recompute.
- Stack into `@deck.gl/geo-layers`: `TripsLayer` extends `PathLayer`, `MVTLayer` extends `GeoJsonLayer` (via `TileLayer`), the whole cell family extends `PolygonLayer` through `GeoCellLayer` — this vocabulary is the base those geospatial composites specialize.

[LOCAL_ADMISSION]:
- imported only inside `ui/viewer` (`scope:viewer`); a layer instance is a declarative value, never a stateful service.
- prefer `GeoJsonLayer`'s parameterized dispatch over hand-instantiating point+line+fill layers for a mixed Feature source; reach for the individual primitives only for single-geometry, performance-critical, or non-GeoJSON data.
- `GreatCircleLayer` (in `@deck.gl/geo-layers`) is superseded by `ArcLayer` with `greatCircle: true` — use the toggle, not the deprecated layer.
- atlas-backed layers (`IconLayer`/`TextLayer`) load sprites/fonts through loaders.gl; supply `iconAtlas`/`fontSettings` or let the auto-packer build them, but keep the atlas source in `viewer` assets, not inlined per-frame.

[RAIL_LAW]:
- Package: `@deck.gl/layers`
- Owns: the primitive mark vocabulary — point (`Scatterplot`/`PointCloud`/`Icon`/`Text`/`Column`/`GridCell`), path (`Path`/`Line`/`Arc`), area (`Polygon`/`SolidPolygon`), raster (`Bitmap`), and the `GeoJsonLayer` omnibus composite
- Accept: one layer subclass per mark with distinctive `get*` accessors over the core base, `GeoJsonLayer` as the parameterized Feature-stream dispatch, the fill/stroke/extrude split from `PolygonLayer`, `ArcLayer{greatCircle:true}` for geodesics, GeoArrow `RecordBatch` mirrors for columnar Arrow input
- Reject: forking a layer's props instead of subclassing, per-object styling as parallel props instead of one function accessor, re-deriving the unit-normalization quartet per layer, hand-wiring point+line+fill for a mixed Feature collection `GeoJsonLayer` already dispatches, the deprecated `GreatCircleLayer`/`strokeWidth`/`getColor`-on-scatterplot aliases
