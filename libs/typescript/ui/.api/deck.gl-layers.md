# [TS_UI_API_DECK_GL_LAYERS]

`@deck.gl/layers` owns the primitive GPU mark vocabulary the `ui/viewer/geo/layers` plane instantiates over the `@deck.gl/core` engine: point, path, area, and raster marks with the `GeoJsonLayer` omnibus that fans one Feature stream to every mark. One shape law generic over the row type governs every layer; `@deck.gl/core` owns the base prop axes and the `Accessor`/`Position`/`Color`/`Unit` types this surface styles with.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@deck.gl/layers`
- package: `@deck.gl/layers` (MIT)
- module: esm barrel re-exporting the layer classes and their `XxxLayerProps` types
- runtime: browser WebGL2/WebGPU through the `@deck.gl/core` luma.gl `Device`; `scope:viewer`
- rail: the primitive mark vocabulary a `Deck.layers` array instantiates
- depends: `@deck.gl/core`, `@loaders.gl/core`, `@luma.gl/core`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: mark classes in taxonomy order — point, path, area, then the `BitmapLayer` raster and the `GeoJsonLayer` omnibus; a `composite` renders sublayers, a `class` renders one primitive.

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY] | [CAPABILITY]              |
| :-----: | :------------------------- | :------------ | :------------------------ |
|  [01]   | `ScatterplotLayer<DataT>`  | class         | circle point mark         |
|  [02]   | `PointCloudLayer<DataT>`   | class         | lit 3D point cloud        |
|  [03]   | `IconLayer<DataT>`         | class         | raster sprite marks       |
|  [04]   | `TextLayer<DataT>`         | composite     | SDF glyph labels          |
|  [05]   | `ColumnLayer<DataT>`       | class         | extruded polygon columns  |
|  [06]   | `GridCellLayer<DataT>`     | class         | square grid cells         |
|  [07]   | `PathLayer<DataT>`         | class         | mitered polylines         |
|  [08]   | `LineLayer<DataT>`         | class         | straight segments         |
|  [09]   | `ArcLayer<DataT>`          | class         | curved arc segments       |
|  [10]   | `PolygonLayer<DataT>`      | composite     | fill-stroke-extrude area  |
|  [11]   | `SolidPolygonLayer<DataT>` | class         | tesselated fill primitive |
|  [12]   | `BitmapLayer`              | class         | raster image quad         |
|  [13]   | `GeoJsonLayer<FeatProps>`  | composite     | Feature-stream omnibus    |

- Aux exports: `BitmapBoundingBox` (bounds tuple `[left,bottom,right,top]` or four `Position` corners), `BitmapLayerPickingInfo` (`{bitmap, uv}`), `_MultiIconLayer`/`_TextBackgroundLayer` (`TextLayer` sublayers, not instantiated directly).

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: distinctive `get*` accessors and toggles each mark adds over the core base; the `*Units`/`*Scale`/`*MinPixels`/`*MaxPixels` unit quartet is one core axis, shown brace-compressed per layer.

- [01]-[SCATTERPLOTLAYER]: `getPosition` `getRadius` `getFillColor` `getLineColor` `getLineWidth` `getPixelOffset` `stroked` `filled` `billboard` `antialiasing` `radius{Units,Scale,MinPixels,MaxPixels}`
- [02]-[POINTCLOUDLAYER]: `getPosition` `getNormal` `getColor` `pointSize` `sizeUnits` `material`
- [03]-[ICONLAYER]: `getPosition` `getIcon` `getSize` `getColor` `getAngle` `getPixelOffset` `iconAtlas` `iconMapping` `billboard` `sizeUnits`
- [04]-[TEXTLAYER]: `getText` `getPosition` `getColor` `getSize` `getAngle` `getTextAnchor` `getAlignmentBaseline` `getPixelOffset` `fontFamily` `fontSettings` `characterSet` `background` `outlineWidth`
- [05]-[COLUMNLAYER]: `getPosition` `getFillColor` `getLineColor` `getElevation` `getLineWidth` `diskResolution` `radius` `angle` `vertices` `coverage` `extruded` `stroked` `filled` `wireframe` `flatShading` `elevationScale` `material`
- [06]-[GRIDCELLLAYER]: `ColumnLayer` accessors with `cellSize`
- [07]-[PATHLAYER]: `getPath` `getColor` `getWidth` `width{Units,Scale,MinPixels,MaxPixels}` `capRounded` `jointRounded` `miterLimit` `billboard`
- [08]-[LINELAYER]: `getSourcePosition` `getTargetPosition` `getColor` `getWidth` `width{Units,Scale,MinPixels,MaxPixels}`
- [09]-[ARCLAYER]: `getSourcePosition` `getTargetPosition` `getSourceColor` `getTargetColor` `getWidth` `getHeight` `getTilt` `greatCircle` `numSegments`
- [10]-[POLYGONLAYER]: `getPolygon` `getFillColor` `getLineColor` `getLineWidth` `getElevation` `stroked` `filled` `extruded` `wireframe` `elevationScale` `lineJointRounded` `lineMiterLimit` `material` `_normalize` `_windingOrder`
- [11]-[SOLIDPOLYGONLAYER]: `getPolygon` `getFillColor` `getLineColor` `getElevation` `extruded` `wireframe` `elevationScale` `material` `_normalize` `_windingOrder` `_full3d`
- [12]-[BITMAPLAYER]: `image` `bounds` `desaturate` `transparentColor` `tintColor` `textureParameters` `_imageCoordinateSystem`
- [13]-[GEOJSONLAYER]: `data` `pointType` `getFillColor` `filled` `getLineColor` `getLineWidth` `stroked` `lineWidthUnits` `getElevation` `extruded` `wireframe` `getPointRadius` `pointRadius{Units,Scale,MinPixels,MaxPixels}` `getIcon` `iconAtlas` `getText` `getTextSize`

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every layer is `XxxLayerProps<DataT> = _XxxLayerProps<DataT> & (LayerProps | CompositeLayerProps)`, generic over `DataT`; a new mark subclasses `Layer`/`CompositeLayer` with distinctive `get*` accessors, never forking another layer's props.
- Per-object style is one `Accessor<In,Out>` closing over the row and `AccessorContext` (`getFillColor: d => scale(d.value)` with an `updateTriggers.getFillColor` key), never a parallel prop; the `*Units`/`*Scale`/`*MinPixels`/`*MaxPixels` quartet is one normalization axis, shared and never re-derived per layer.
- `PolygonLayer` composes `SolidPolygonLayer` (GPU-tesselated fill) with `PathLayer` (mitered outline); the cell family and `GeoJsonLayer` inherit that fill/stroke split, and extrusion (`extruded` + `getElevation` + `material`) reuses the core `LightingEffect`.
- `GeoJsonLayer` is the parameterized entry: one Feature stream, a `pointType` discriminant, six accessor sub-groups fanning to the mark sublayers.

[STACKING]:
- `@deck.gl/core`(`.api/deck.gl-core.md`): these layers ARE the `Layer`/`CompositeLayer` vocabulary — base classes, `Accessor`/`Position`/`Color`/`Unit`/`Material`, picking, and the `Deck` host live in core, and `data` binds the core `LayerDataSource` (URL/promise/async-batch/binary-columnar) directly.
- `wire` + `@turf/turf`(`.api/turf-turf.md`): `wire` decodes WKB→GeoJSON, `@turf/turf` runs the CPU planar stage (`buffer`/`simplify`, `union`/`intersect`/`difference` overlay, `bbox`→`bboxPolygon`, `voronoi`/`isobands`), and its `Feature`/`FeatureCollection` feeds `GeoJsonLayer.data` or `PolygonLayer.getPolygon` — the interaction-scale derived-overlay path.
- `@geoarrow/deck.gl-geoarrow` + `apache-arrow`(`.api/geoarrow-deck.gl-geoarrow.md`, `.api/apache-arrow.md`): the GeoArrow mirrors (`GeoArrowScatterplotLayer`/`GeoArrowPathLayer`/`GeoArrowPolygonLayer`/`GeoArrowSolidPolygonLayer`) consume one Arrow `RecordBatch` per layer (the caller fans `Table.batches`) with zero per-row JS objects — same accessor semantics, columnar input, for the bulk route.
- `@effect-atom`(`.api/effect-atom-atom.md`): layer instances are pure values built inside a derived atom from the state fold; the `layers` array recomputes on state change and pushes via `Deck.setProps`, `updateTriggers` keys tracking which atom values force GPU attribute recompute.
- `@deck.gl/geo-layers`(`.api/deck.gl-geo-layers.md`): `TripsLayer` extends `PathLayer`, `MVTLayer` extends `GeoJsonLayer` through `TileLayer`, and the cell family extends `PolygonLayer` through `GeoCellLayer` — this vocabulary is the base those composites specialize.

[LOCAL_ADMISSION]:
- Import only inside `ui/viewer` (`scope:viewer`); a layer instance is a declarative value, never a stateful service.
- `GeoJsonLayer`'s parameterized dispatch renders a mixed Feature source; reach for the individual primitives only for single-geometry, performance-critical, or non-GeoJSON data.
- Atlas-backed marks (`IconLayer`/`TextLayer`) load sprites and fonts through loaders.gl; supply `iconAtlas`/`fontSettings` or let the auto-packer build them, keeping the atlas source in `viewer` assets, never inlined per frame.

[RAIL_LAW]:
- Package: `@deck.gl/layers`
- Owns: the primitive point, path, area, and raster mark vocabulary with the `GeoJsonLayer` Feature-stream omnibus.
- Accept: one subclass per mark with distinctive `get*` accessors over the core base, `GeoJsonLayer` as the Feature-stream dispatch, the fill/stroke/extrude split from `PolygonLayer`, `ArcLayer{greatCircle:true}` for geodesics, GeoArrow `RecordBatch` mirrors for columnar Arrow input.
- Reject: forking a layer's props instead of subclassing, per-object styling as parallel props instead of one accessor function, re-deriving the unit quartet per layer, hand-wiring point+line+fill for a Feature collection `GeoJsonLayer` already dispatches.
