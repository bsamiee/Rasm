# [TS_UI_API_MAPLIBRE_GL]

`maplibre-gl` owns the vector basemap: one `Map` binds one WebGL2 context, one camera, one typed event stream, and the declarative style, driving populate/navigate/query through parameterized source/layer/control/handler rails. deck.gl interleaves GPU layers through the `addControl`/`CustomLayerInterface` rail into that one context — never a peer camera — while React owns mount and unmount alone and the imperative lifecycle folds into the `viewer` panel atom; `scope:viewer` project-local.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `maplibre-gl`
- package: `maplibre-gl` (BSD-3-Clause)
- module: `type: module` ESM, zero peer deps; re-exports the `@maplibre/maplibre-gl-style-spec` `*Specification` style vocabulary
- runtime: browser — a DOM container and a WebGL2 context, both `browser`-provided ports the `viewer` declares; `scope:viewer`
- rail: the basemap render surface `viewer/geo/layers` binds; one `Map` per viewport
- modules: `Map` (`Map$1`, alias `MapLibreMap`) forwarding the composed camera API; source/control/handler classes; `Marker`/`Popup`; the event classes over generic `Evented<EventType>`; the geometry value types; the worker/plugin globals; the re-exported `*Specification` vocabulary

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: map root + camera
- `Map extends Evented<MapEventType>` composes an internal `Camera` — `jumpTo`/`easeTo`/`flyTo` and the `get*`/`set*` camera verbs are `Map` methods, and `Camera` never exports. One `MapOptions` record is the whole knob surface: container/style/view, interaction gates, request/worker policy. `MapOptions`, `CustomLayerInterface`, and `IControl` field shapes ride the inline records below.

| [INDEX] | [SYMBOL]                                                 | [TYPE_FAMILY]  | [CAPABILITY]                               |
| :-----: | :------------------------------------------------------- | :------------- | :----------------------------------------- |
|  [01]   | `Map` (`Map$1 as Map` / `MapLibreMap`)                   | map root       | the one basemap instance; panel-atom bound |
|  [02]   | `MapOptions`                                             | option record  | the single knob config; fields below       |
|  [03]   | `CameraOptions` / `JumpToOptions` / `EaseToOptions`      | camera intents | `jumpTo` instant intent options            |
|  [04]   | `FlyToOptions` / `FitBoundsOptions` / `AnimationOptions` | camera intents | `easeTo`/`flyTo` animated intent options   |

[MAP_OPTIONS]: `container` `style` `center` `zoom` `bearing` `pitch` `roll` `hash` `interactive` `transformRequest` `transformCameraUpdate` `locale` `pixelRatio` `maxBounds` `minZoom` `maxZoom` `minPitch` `maxPitch` `renderWorldCopies` `canvasContextAttributes` `attributionControl` `validateStyle` `terrainSkirtLength` `zoomLevelsToOverscale`
[CUSTOM_LAYER_INTERFACE]: `id` `type:"custom"` `renderingMode` `render(gl,matrix)` `prerender(gl,matrix)` `onAdd(map,gl)` `onRemove(map,gl)`
[ICONTROL]: `onAdd(map) -> HTMLElement` `onRemove(map)` `getDefaultPosition()`

[PUBLIC_TYPE_SCOPE]: geometry value types
- lnglat↔pixel↔mercator value types; the `*Like` unions accept plain-array or object literals, so callers pass raw coordinates and the class instances carry the methods.

| [INDEX] | [SYMBOL]                            | [TYPE_FAMILY]     | [CAPABILITY]                                                           |
| :-----: | :---------------------------------- | :---------------- | :--------------------------------------------------------------------- |
|  [01]   | `LngLat` / `LngLatLike`             | geographic point  | the coordinate value; `LngLatLike` accepts `[lng, lat]`/`{lng,lat}`    |
|  [02]   | `LngLatBounds` / `LngLatBoundsLike` | bbox              | `fitBounds`/`cameraForBounds`/`maxBounds` input                        |
|  [03]   | `MercatorCoordinate`                | web-mercator unit | deck.gl coordinate-system bridge; altitude-aware                       |
|  [04]   | `Point` / `PointLike`               | screen pixel      | `project` output / `unproject` input; `queryRenderedFeatures` geometry |
|  [05]   | `EdgeInsets` / `PaddingOptions`     | viewport padding  | camera padding for panel-occluded viewports                            |

[PUBLIC_TYPE_SCOPE]: source + layer + style vocabulary
- Every `Source` implementer is a row of `addSource(id, spec)` keyed by `spec.type`; every layer is `addLayer(AddLayerObject)` where `AddLayerObject = LayerSpecification | CustomLayerInterface`. `*Specification` types are declarative data re-exported from `@maplibre/maplibre-gl-style-spec`.

| [INDEX] | [SYMBOL]                                        | [TYPE_FAMILY]       | [CAPABILITY]                                              |
| :-----: | :---------------------------------------------- | :------------------ | :-------------------------------------------------------- |
|  [01]   | `GeoJSONSource` / `VectorTileSource`            | source rows         | geojson + vector tile sources, keyed by `spec.type`       |
|  [02]   | `RasterTileSource` / `RasterDEMTileSource`      | source rows         | raster + raster-DEM tile sources                          |
|  [03]   | `ImageSource` / `CanvasSource` / `VideoSource`  | source rows         | image / canvas / video sources                            |
|  [04]   | `addSourceType(name, SourceClass)`              | source extension    | register a custom source type — the vocabulary is open    |
|  [05]   | `CustomLayerInterface`                          | GPU interleave hook | the `AddLayerObject` variant deck.gl registers            |
|  [06]   | `StyleSpecification` / `LayerSpecification`     | declarative style   | root style + per-layer JSON                               |
|  [07]   | `SourceSpecification` / `FilterSpecification`   | declarative style   | source + filter JSON; expressions are data                |
|  [08]   | `PropertyValueSpecification` / `AddLayerObject` | declarative style   | paint/layout value data; `Layer` or `Custom` layer object |
|  [09]   | `GeoJSONFeature` / `MapGeoJSONFeature`          | query result        | `queryRenderedFeatures`/`querySourceFeatures` output      |
|  [10]   | `FeatureIdentifier`                             | feature key         | the feature-state key                                     |

[PUBLIC_TYPE_SCOPE]: control + DOM-overlay rows
- Every `IControl` implementer is added via `addControl(control, position?)`; `MapboxOverlay` satisfies it and joins the same rail. `Marker`/`Popup` are DOM overlays anchored at a `LngLat`, distinct from deck.gl's GPU overlays.

| [INDEX] | [SYMBOL]                                    | [TYPE_FAMILY]    | [CAPABILITY]                                                |
| :-----: | :------------------------------------------ | :--------------- | :---------------------------------------------------------- |
|  [01]   | `IControl`                                  | control contract | the one `addControl` contract; `MapboxOverlay` satisfies it |
|  [02]   | `NavigationControl` / `ScaleControl`        | control rows     | rows of one `addControl(control, ControlPosition)` rail     |
|  [03]   | `FullscreenControl` / `GeolocateControl`    | control rows     | fullscreen + geolocate controls                             |
|  [04]   | `AttributionControl` / `LogoControl`        | control rows     | attribution + logo controls                                 |
|  [05]   | `TerrainControl` / `GlobeControl`           | control rows     | terrain + globe controls                                    |
|  [06]   | `ControlPosition`                           | position enum    | `"top-left"`/`"top-right"`/`"bottom-left"`/`"bottom-right"` |
|  [07]   | `Marker` / `Popup` (both `extends Evented`) | DOM overlay      | `viewer/mark` HTML anchors at a `LngLat`; draggable marker  |

[PUBLIC_TYPE_SCOPE]: event algebra + interaction handlers
- `on`/`once`/`off` over the `MapEventType`/`MapLayerEventType`/`SourceEventType` maps return a `Subscription`, and a layer-id overload filters by layer. Generic abstract `Evented<EventType>` types the rail without re-declaring overloads; every event is a real class fired as an instance. Handlers share one `map.<handler>` `enable`/`disable`/`isEnabled` surface gated at construction by `MapOptions`.

| [INDEX] | [SYMBOL]                                                 | [TYPE_FAMILY]     | [CAPABILITY]                                            |
| :-----: | :------------------------------------------------------- | :---------------- | :------------------------------------------------------ |
|  [01]   | `Evented<EventType>`                                     | event base        | generic abstract base; typed `on`/`once`/`off`          |
|  [02]   | `MapMouseEvent` / `MapTouchEvent` / `MapWheelEvent`      | event payloads    | pointer/wheel payloads, typed by event name             |
|  [03]   | `MapLibreEvent` / `MapMovementEvent`                     | event payloads    | lifecycle base; camera-transition payloads incl. `roll` |
|  [04]   | `MapSourceDataEvent` / `MapStyleDataEvent`               | event payloads    | `data`/`dataloading` source vs style-data split         |
|  [05]   | `MapStyleLoadEvent` / `MapLayerMouseEvent`               | event payloads    | `style.load` ready; layer-scoped pointer payload        |
|  [06]   | `ScrollZoomHandler` / `DragPanHandler`                   | interaction vocab | `map.scrollZoom` / `map.dragPan`                        |
|  [07]   | `DragRotateHandler` / `BoxZoomHandler`                   | interaction vocab | `map.dragRotate` / `map.boxZoom`                        |
|  [08]   | `KeyboardHandler` / `DoubleClickZoomHandler`             | interaction vocab | `map.keyboard` / `map.doubleClickZoom`                  |
|  [09]   | `TwoFingersTouch*Handler` / `CooperativeGesturesHandler` | interaction vocab | touch rotate/zoom; cooperative gesture gate             |
|  [10]   | `Subscription`                                           | handle            | `{ unsubscribe() }` — the `on`/`once` return            |

[PUBLIC_TYPE_SCOPE]: worker + plugin globals
- Module-level functions configure the worker pool, RTL text shaping, and the request-protocol vocabulary; `addProtocol` is the open extension for custom tile transports (auth-wrapped, `pmtiles`, COG).

| [INDEX] | [SYMBOL]                                                   | [TYPE_FAMILY]      | [CAPABILITY]                                         |
| :-----: | :--------------------------------------------------------- | :----------------- | :--------------------------------------------------- |
|  [01]   | `addProtocol(scheme, loadFn)` / `removeProtocol(scheme)`   | protocol extension | custom tile transport (auth, `pmtiles`, COG)         |
|  [02]   | `AddProtocolAction`                                        | protocol type      | the `loadFn` loader signature                        |
|  [03]   | `setRTLTextPlugin(url, lazy)` / `getRTLTextPluginStatus()` | text shaping       | RTL/complex-script glyph shaping; lazy worker plugin |
|  [04]   | `prewarm()` / `clearPrewarmedResources()`                  | worker warmup      | pre-spin the worker pool for cold-start latency      |
|  [05]   | `getWorkerCount` / `setWorkerCount`                        | worker policy      | worker-pool sizing                                   |
|  [06]   | `getMaxParallelImageRequests` (+ setter)                   | worker policy      | parallel image-request cap                           |
|  [07]   | `getWorkerUrl` / `setWorkerUrl` / `importScriptInWorkers`  | worker policy      | worker script URL + injection                        |
|  [08]   | `getVersion()` / `config` / `EXTENT` (`= 8192`)            | runtime constants  | version probe; tile extent for custom-source math    |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: construct, populate, interleave, drive, query, tear down
- Lifecycle is imperative and single-sourced: `new Map(options)`, populate through `addSource`/`addLayer`/`addControl`, interleave deck.gl through `addControl`, drive with the camera vocabulary, read with `project`/`queryRenderedFeatures`, `remove()` at unmount. Populate and drive methods return `this` for fluent folds; queries and projections return values.

| [INDEX] | [SURFACE]                                                      | [ENTRY_FAMILY]  | [CAPABILITY]                                         |
| :-----: | :------------------------------------------------------------- | :-------------- | :--------------------------------------------------- |
|  [01]   | `new Map(options: MapOptions)`                                 | construct       | the one basemap; `container` is a `browser` port     |
|  [02]   | `addSource(id, spec)` / `removeSource(id)`                     | source rail     | add/remove keyed by `spec.type`                      |
|  [03]   | `getSource<T>(id)` / `isSourceLoaded(id)`                      | source rail     | typed lookup; load probe                             |
|  [04]   | `addLayer(layer, beforeId?)` / `removeLayer` / `moveLayer`     | layer rail      | add/remove/reorder; `beforeId` orders it             |
|  [05]   | `getLayer` / `getLayersOrder` / `setLayerZoomRange`            | layer rail      | inspect order; per-layer zoom range                  |
|  [06]   | `setFilter` / `getFilter`                                      | style mutate    | expression-data filter edits                         |
|  [07]   | `setPaintProperty` / `setLayoutProperty` / `getLayoutProperty` | style mutate    | live paint/layout re-paint without a style swap      |
|  [08]   | `addControl` / `removeControl` / `hasControl`                  | control rail    | `NavigationControl`…; `MapboxOverlay` joins          |
|  [09]   | `jumpTo` / `easeTo` / `flyTo` / `fitBounds` / `stop`           | camera drive    | `jumpTo` instant, `easeTo`/`flyTo` animated          |
|  [10]   | `setCenter` / `setZoom` / `setBearing`                         | camera set      | imperative center/zoom/bearing setters               |
|  [11]   | `setPitch` / `setPadding`                                      | camera set      | imperative pitch/padding setters                     |
|  [12]   | `getCenter` / `getZoom` / `getBearing`                         | camera read     | deck.gl reads these each `move`                      |
|  [13]   | `getPitch` / `getPadding` / `getBounds`                        | camera read     | pitch/padding/bounds readback                        |
|  [14]   | `cameraForBounds` / `calculateCameraOptionsFromTo(...)`        | camera solve    | eye→target solve → `CameraOptions`                   |
|  [15]   | `on` / `once` / `off` → `Subscription`                         | event rail      | typed events; `on(type, layerId, fn)` filters by id  |
|  [16]   | `project(lnglat)` → `Point` / `unproject(point)` → `LngLat`    | coordinate sync | pixel↔lnglat for deck view-state + markers           |
|  [17]   | `queryRenderedFeatures(...)` / `querySourceFeatures(...)`      | feature query   | picking/selection; feeds `viewer/mark` sets          |
|  [18]   | `setStyle` / `setProjection` / `setTerrain`                    | scene config    | style swap, globe/mercator projection, 3D terrain    |
|  [19]   | `setSky` / `setLight`                                          | scene config    | sky + light scene config                             |
|  [20]   | `setFeatureState` / `getFeatureState` / `removeFeatureState`   | feature state   | data-driven hover/select styling, no re-add          |
|  [21]   | `addImage` / `addSprite` / `loadImage` / `updateImage`         | image + sprite  | load custom icons/patterns/sprites for symbol layers |
|  [22]   | `hasImage` / `removeImage` / `setMissingStyleImageResolver`    | image + sprite  | inspect/remove icons; resolver fills missing images  |
|  [23]   | `addProtocol` / `setRTLTextPlugin` / `prewarm`                 | module policy   | custom transport, RTL shaping, worker warmup         |
|  [24]   | `resize()` / `remove()` / `redraw()` / `triggerRepaint()`      | lifecycle       | `remove()` frees the GL context at unmount           |
|  [25]   | `getCanvas()` / `getContainer()`                               | lifecycle       | the DOM/canvas handles                               |

- `Map.setMissingStyleImageResolver`: supplies an absent style image on demand, sync or async; the `styleimagemissing` event is notify-only.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- One `Map` owns the WebGL2 context and forwards its composed camera; generic `Evented<EventType>` owns the typed event rail. deck.gl interleaves — never a second context or peer camera.
- Style is data: `StyleSpecification`/`LayerSpecification`/`FilterSpecification`/`PropertyValueSpecification` are declarative JSON the `addLayer`/`setFilter`/`setPaintProperty` rails consume, and maplibre evaluates expressions rather than hand-written render code.
- Sources, layers, controls, and handlers are parameterized rails: `addSource` discriminates on `spec.type`, `addLayer` on `LayerSpecification | CustomLayerInterface`, `addControl` on `IControl`, handlers on one `enable`/`disable`/`isEnabled` surface — a new capability is a spec row or an implementer, never a method family.

[STACKING]:
- `@deck.gl/mapbox` `MapboxOverlay` (`.api/deck.gl-mapbox.md`): `map.addControl(overlay)` mounts it; `interleaved: true` registers deck layers as `CustomLayerInterface` entries drawing into the shared GL context and depth buffer, so 3D deck geometry occludes; `overlaid` composites on a separate canvas above.
- Overlay view-state syncs from the camera each `move` through `getCenter`/`getZoom`/`getBearing`/`getPitch`/`getPadding` (+ `getProjection` for globe), `project`/`unproject` supplying overlay-mark screen↔lngLat; deck's `getFreeCameraOptions?()` is mapbox-only, so under maplibre deck derives altitude from `getCameraTargetElevation()`.
- `@geoarrow/deck.gl-geoarrow` + `apache-arrow` (`.api/geoarrow-deck.gl-geoarrow.md`, `.api/apache-arrow.md`): GeoArrow layers consume `apache-arrow` `RecordBatch`es column-wise, one per layer fanned from `Table.batches`; WKB→geometry decode stays in `wire`, so maplibre and deck.gl receive decoded columnar geometry.
- `@turf/turf` (`.api/turf-turf.md`): planar ops (buffer/simplify/intersect) run on decoded GeoJSON in-browser, feeding a `GeoJSONSource.setData` or a deck layer — turf computes, maplibre renders.
- `@effect-atom` + `Stream`/`Match` (`libs/typescript/.api/effect.md`, `.api/effect-atom-atom.md`): the `Map` and camera state bind through the one panel atom; `on('moveend'|'idle'|'click', …)` `Subscription`s fold in as `Stream`-from-events, `Match` dispatches typed payloads, and React owns only `new Map` and `remove()`.
- `browser` runtime port: the DOM container, WebGL context, and worker pool are `browser`-provided; `transformRequest` routes tile URLs through the app's auth boundary.

[LOCAL_ADMISSION]:
- `scope:viewer` project-local; the core `ui` never imports it, keeping heavy render deps out of the non-spatial majority.
- WKB decode stays in `wire`, style is authored as `*Specification` data, and coordinate math goes through `project`/`unproject`.

[RAIL_LAW]:
- Package: `maplibre-gl`
- Owns: the vector basemap `Map`, its single WebGL2 context + `Camera` + typed event rail, the declarative style vocabulary, the parameterized source/layer/control/handler rails, the DOM `Marker`/`Popup` overlays, and the worker/RTL/protocol globals
- Accept: one `new Map` per viewport; the `addSource`/`addLayer`/`addControl` rails; `MapboxOverlay` interleaving through `addControl` into the shared context; the `Camera` as the single navigation authority whose getters feed deck's per-`move` view-state; `project`/`unproject` for coordinate sync; `on`→`Subscription` folded into the panel atom; `*Specification` style data; `addProtocol`/`transformRequest` for transport
- Reject: a second GL context or peer camera, WKB decode in `ui`, style authored as code, hand-rolled projection math, importing it into the core `ui`, a React wrapper owning the map lifecycle beyond mount/unmount
