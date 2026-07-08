# [TS_UI_API_MAPLIBRE_GL]

`maplibre-gl` is the `viewer/geo/layers` basemap runtime: a `Map` owning one WebGL context, one camera, and the declarative vector style, over which `@deck.gl/mapbox`'s `MapboxOverlay` interleaves GPU layers as a maplibre `IControl` sharing that context and depth buffer, `turf` runs planar ops on decoded GeoJSON, and `@geoarrow/deck.gl-geoarrow` streams `apache-arrow` columns — the WKB→geometry decode staying in `wire`, never here. The seam is single-sourced: one GL context (deck.gl interleaves, never spins its own), one camera (`Map`'s `Camera` drives; deck.gl view-state syncs through `project`/`unproject` + camera getters), one event stream (`on`/`off`/`once` typed `Subscription`s folding into the panel atom). Framework-agnostic and imperative; React owns only mount/unmount and the atom binding. `scope:viewer` project-local, compile-time excluded from non-spatial apps.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `maplibre-gl`
- package: `maplibre-gl`
- license: `BSD-3-Clause`
- deps: framework-agnostic (`type: module` ESM, zero peer deps); re-exports the `@maplibre/maplibre-gl-style-spec` declarative style types
- catalog-verdict: KEEP
- runtime: `scope:viewer` project-local, `runtime:browser` — needs a DOM container + WebGL catalog; admitted by the `ui/viewer` Nx project alone; the container/context is a `browser`-provided port the viewer declares
- modules: `Map` (`= Map$1`, also `MapLibreMap`), the `Camera` base, source classes, control classes, `Marker`/`Popup`, handler classes, the event algebra, the geometry value types, the worker/plugin globals, and the re-exported `*Specification` style vocabulary

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: map root + camera
- rail: viewer/geo/layers, viewer/geo/project
- `Map extends Camera extends Evented`: the root is configured by one parameterized `MapOptions` record and drives all navigation through the inherited `Camera` vocabulary. `MapOptions` is the single knob surface — container/style/view + interaction gates + request/worker policy — never a wrapper-per-option.

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [CONSUMER_BOUNDARY] |
|:-----: |:-------------------------------------------------------------------------------- |:------------- |:---------------------------------------------------------------- |
| [01] | `Map` (`Map$1 as Map` / `MapLibreMap`) — `extends Camera extends Evented` | map root | the one basemap instance; `viewer/geo` binds it through the panel atom |
| [02] | `MapOptions` — `{ container, style, center, zoom, bearing, pitch, roll, projection, hash, interactive, transformRequest, transformCameraUpdate, locale, pixelRatio, maxBounds, min/maxZoom, min/maxPitch, renderWorldCopies, canvasContextAttributes, attributionControl, validateStyle, … }` | option record | one parameterized config; interaction handlers gated by boolean/opts fields |
| [03] | `Camera` — actions `jumpTo/easeTo/flyTo/fitBounds`, setters `setCenter/setZoom/setBearing/setPitch/setPadding/setRoll`, getters `getCenter/getZoom/getBearing/getPitch/getPadding/getRoll/getVerticalFieldOfView` | camera base | the single camera authority; deck.gl reads the getters each `move` to sync view-state |
| [04] | `CameraOptions` / `JumpToOptions` / `EaseToOptions` / `FlyToOptions` / `FitBoundsOptions` / `AnimationOptions` | camera intents | one option algebra: `jumpTo` instant, `easeTo`/`flyTo` animated via `AnimationOptions` |

[PUBLIC_TYPE_SCOPE]: geometry value types
- rail: viewer/geo/project
- The lnglat↔pixel↔mercator value types; `*Like` unions accept plain-array/object literals so callers pass raw coordinates, the class instances carry the methods.

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [CONSUMER_BOUNDARY] |
|:-----: |:-------------------------------------------------------------- |:----------------- |:---------------------------------------------------------------- |
| [01] | `LngLat` / `LngLatLike` | geographic point | the coordinate value; `LngLatLike` accepts `[lng, lat]`/`{lng,lat}` |
| [02] | `LngLatBounds` / `LngLatBoundsLike` | bbox | `fitBounds`/`cameraForBounds`/`maxBounds` input |
| [03] | `MercatorCoordinate` | web-mercator unit | deck.gl coordinate-system bridge; altitude-aware |
| [04] | `Point` / `PointLike` | screen pixel | `project` output / `unproject` input; `queryRenderedFeatures` geometry |
| [05] | `EdgeInsets` / `PaddingOptions` | viewport padding | camera padding for panel-occluded viewports |

[PUBLIC_TYPE_SCOPE]: source + layer + style vocabulary
- rail: viewer/geo/layers
- Sources and layers are parameterized rails, not fixed rosters: every `Source` implementer is a row of `addSource(id, spec)` discriminated by `spec.type`; every layer is `addLayer(AddLayerObject)` where `AddLayerObject = LayerSpecification | CustomLayerInterface`. `CustomLayerInterface` is the GPU interleave hook deck.gl registers behind the scenes. The `*Specification` types are declarative data (expressions are data, not code) re-exported from `@maplibre/maplibre-gl-style-spec`.

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [CONSUMER_BOUNDARY] |
|:-----: |:-------------------------------------------------------------------------------- |:----------------- |:---------------------------------------------------------------- |
| [01] | `Source` impls — `GeoJSONSource` / `VectorTileSource` / `RasterTileSource` / `RasterDEMTileSource` / `ImageSource` / `CanvasSource` / `VideoSource` | source rows | rows of one `addSource(id, SourceSpecification)` rail, keyed by `spec.type` |
| [02] | `addSourceType(name, SourceClass)` / `SourceClass` | source extension | register a custom source type — the vocabulary is open, not fixed |
| [03] | `CustomLayerInterface` — `{ id, type: "custom", renderingMode?, render, prerender?, onAdd?(map, gl) }` | GPU interleave hook | the `AddLayerObject` variant deck.gl interleaving registers; draws into the shared GL context |
| [04] | `StyleSpecification` / `LayerSpecification` / `SourceSpecification` / `FilterSpecification` / `PropertyValueSpecification` / `AddLayerObject` | declarative style | style-as-data; filters/paint/layout are `PropertyValueSpecification` expression data, authored never as code |
| [05] | `GeoJSONFeature` / `MapGeoJSONFeature` / `FeatureIdentifier` | query result | `queryRenderedFeatures`/`querySourceFeatures` output; feature-state key |

[PUBLIC_TYPE_SCOPE]: control + DOM-overlay rows
- rail: viewer/geo/layers, viewer/mark
- Controls are one rail: every `IControl` implementer is added via `addControl(control, position?)`. `MapboxOverlay` (`@deck.gl/mapbox`, `.api/deck.gl-mapbox.md`) implements `IControl` and joins through the same rail. `Marker`/`Popup` are DOM overlays (HTML anchored at a `LngLat`), distinct from deck.gl's GPU overlays.

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [CONSUMER_BOUNDARY] |
|:-----: |:-------------------------------------------------------------------------------- |:----------------- |:---------------------------------------------------------------- |
| [01] | `IControl` — `{ onAdd(map): HTMLElement; onRemove(map): void; getDefaultPosition?() }` | control contract | the one `addControl` contract; `MapboxOverlay` satisfies it |
| [02] | `NavigationControl` / `ScaleControl` / `FullscreenControl` / `GeolocateControl` / `AttributionControl` / `LogoControl` / `TerrainControl` / `GlobeControl` | control rows | rows of one `addControl(control, ControlPosition)` rail |
| [03] | `ControlPosition` — `"top-left" \| "top-right" \| "bottom-left" \| "bottom-right"` | position enum | the `addControl` position argument |
| [04] | `Marker` (`extends Evented`) / `Popup` (`extends Evented`) | DOM overlay | `viewer/mark` BCF/selection HTML anchors at a `LngLat`; draggable marker |

[PUBLIC_TYPE_SCOPE]: event algebra + interaction handlers
- rail: viewer/geo/layers, act/gesture
- Events are one typed rail: `on`/`once`/`off` over the `MapEventType`/`MapLayerEventType` maps return a `Subscription`; a layer-scoped overload filters by layer id. Interaction handlers are one enable/disable vocabulary reached as `map.<handler>` — a uniform `enable`/`disable`/`isEnabled` surface, gated at construction by `MapOptions`.

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [CONSUMER_BOUNDARY] |
|:-----: |:-------------------------------------------------------------------------------- |:----------------- |:---------------------------------------------------------------- |
| [01] | `Evented` — `on`/`once`/`off` typed by `MapEventType` / `MapLayerEventType` | event base | the one event rail; `Subscription` is the unsubscribe handle |
| [02] | `MapMouseEvent` / `MapTouchEvent` / `MapWheelEvent` / `MapLibreEvent` / `MapDataEvent` / `MapLayerMouseEvent` | event payloads | typed by event name; `queryRenderedFeatures` rides the pointer payload |
| [03] | handler rows — `ScrollZoomHandler` / `DragPanHandler` / `DragRotateHandler` / `BoxZoomHandler` / `KeyboardHandler` / `DoubleClickZoomHandler` / `TwoFingersTouch*Handler` / `CooperativeGesturesHandler` | interaction vocab | `map.scrollZoom`/`map.dragPan`/… — one `enable`/`disable`/`isEnabled` surface per handler |
| [04] | `Subscription` — `{ unsubscribe() }` | handle | the `on`/`once` return; the atom's cleanup detaches it |

[PUBLIC_TYPE_SCOPE]: worker + plugin globals
- rail: viewer/geo/layers (module-level runtime policy)
- Module-level functions configuring the worker pool, RTL text shaping, and the request-protocol vocabulary; `addProtocol` is the open extension point for custom tile transports (auth-wrapped, `pmtiles`, COG).

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [CONSUMER_BOUNDARY] |
|:-----: |:-------------------------------------------------------------------------------- |:----------------- |:---------------------------------------------------------------- |
| [01] | `addProtocol(scheme, loadFn)` / `removeProtocol(scheme)` / `AddProtocolAction` | protocol extension | custom tile transport (auth, `pmtiles`, COG) — one registration rail |
| [02] | `setRTLTextPlugin(url, lazy)` / `getRTLTextPluginStatus()` | text shaping | RTL/complex-script glyph shaping; lazy-loaded worker plugin |
| [03] | `prewarm()` / `clearPrewarmedResources()` | worker warmup | pre-spin the worker pool before the first `Map` for cold-start latency |
| [04] | `getWorkerCount`/`setWorkerCount` / `getMaxParallelImageRequests`/`set…` / `getWorkerUrl`/`set…` / `importScriptInWorkers` | worker policy | worker-pool sizing/URL — a `browser`-runtime concern set once |
| [05] | `getVersion()` / `config` / `EXTENT` (`= 8192`) | runtime constants | version probe; tile extent constant for custom-source math |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: construct, populate, interleave, drive, query, tear down
- rail: viewer/geo/layers
- The lifecycle is imperative and single-sourced: one `new Map(options)`, populate with the `addSource`/`addLayer`/`addControl` rails, interleave deck.gl through the `addControl` rail, drive with the `Camera` vocabulary, read with `project`/`queryRenderedFeatures`, and `remove()` at unmount. Every populate/drive method returns `this` for fluent folds; queries and projections return values.

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY] |
|:-----: |:-------------------------------------------------------------------------------- |:-------------- |:---------------------------------------------------------------- |
| [01] | `new Map(options: MapOptions)` | construct | the one basemap; `container` is the `browser`-provided DOM port |
| [02] | `addSource(id, spec)` / `getSource<T>(id)` / `removeSource(id)` / `isSourceLoaded(id)` | source rail | one rail over every `Source` type, keyed by `spec.type` |
| [03] | `addLayer(layer, beforeId?)` / `moveLayer` / `removeLayer` / `getLayer` / `getLayersOrder` / `setLayerZoomRange` | layer rail | `LayerSpecification` or `CustomLayerInterface`; `beforeId` orders vs the basemap |
| [04] | `setFilter` / `getFilter` / `setPaintProperty` / `setLayoutProperty` / `getLayoutProperty` | style mutate | expression-data style edits; live re-paint without a style swap |
| [05] | `addControl(control, position?)` / `removeControl` / `hasControl` | control + interleave | `NavigationControl`… AND `MapboxOverlay` — deck.gl joins here |
| [06] | drive `<Camera>.jumpTo` / `easeTo` / `flyTo` / `fitBounds` / `setCenter` / `setZoom` / `setBearing` / `setPitch` / `setPadding` / `stop`; read `getCenter` / `getZoom` / `getBearing` / `getPitch` / `getPadding` / `getBounds` / `cameraForBounds`; solve `calculateCameraOptionsFromTo(from, altitudeFrom, to, altitudeTo?)` → `CameraOptions` | camera drive + read | single camera authority — the eye→target solve derives center/zoom/bearing/pitch in the map's own camera model, deleting the hand tangent-plane fold; deck.gl reads the getters each `move` to sync view-state (maplibre has no `getFreeCameraOptions`, so deck derives altitude from the map's `transform.elevation`) |
| [07] | `on(type, listener)` / `on(type, layerId, listener)` / `once` / `off` → `Subscription` | event rail | typed events; layer-scoped overload filters by layer id |
| [08] | `project(lnglat)` → `Point` / `unproject(point)` → `LngLat` | coordinate sync | the pixel↔lnglat crossing deck.gl view-state and marker anchoring use |
| [09] | `queryRenderedFeatures(geomOrOpts?)` / `querySourceFeatures(sourceId, params?)` | feature query | picking/selection; feeds `viewer/mark` `GlobalId` selection sets |
| [10] | `setStyle(style, opts?)` / `setProjection(proj)` / `setTerrain(opts\|null)` / `setSky` / `setLight` | scene config | style swap, globe/mercator projection, 3D terrain, sky/light |
| [11] | `setFeatureState(feature, state)` / `getFeatureState` / `removeFeatureState` | feature state | data-driven styling (hover/select) without re-adding the source |
| [12] | `addImage(id, image)` / `loadImage(url)` / `updateImage` / `hasImage` / `removeImage` / `addSprite(id, url)` | image + sprite | custom icons/patterns for symbol layers — BCF/marker glyphs |
| [13] | `addProtocol(scheme, loadFn)` / `setRTLTextPlugin(url, lazy)` / `prewarm()` | module policy | custom transport, RTL shaping, worker warmup — set before/around `Map` |
| [14] | `resize()` / `remove()` / `redraw()` / `triggerRepaint()` / `getCanvas()` / `getContainer()` | lifecycle | `remove()` at unmount frees the GL context; `resize` on layout change |

## [04]-[IMPLEMENTATION_LAW]

[GEO_TOPOLOGY]:
- one context, one camera, one event stream: the `Map` owns the WebGL2 context, the `Camera` base owns navigation, and `Evented` owns the typed event rail. deck.gl does not create a second context or a peer camera — it interleaves.
- style is data, not code: `StyleSpecification`/`LayerSpecification`/`FilterSpecification`/`PropertyValueSpecification` are declarative JSON the `addLayer`/`setFilter`/`setPaintProperty` rails consume; expressions are evaluated by maplibre, never hand-written as render code.
- sources/layers/controls/handlers are parameterized rails, not rosters: `addSource` discriminates on `spec.type`, `addLayer` on `LayerSpecification | CustomLayerInterface`, `addControl` on the `IControl` contract, and handlers share one `enable`/`disable`/`isEnabled` surface gated at construction by `MapOptions` — a new capability is a spec row or an `IControl`/`Source` implementer, never a new method family.

[INTEGRATION_LAW]:
- Stack with `@deck.gl/mapbox` `MapboxOverlay` (`.api/deck.gl-mapbox.md`): the overlay is added via `map.addControl(overlay)`; in `interleaved: true` mode deck.gl registers its layers as maplibre `CustomLayerInterface` entries drawing into the shared GL context and depth buffer, so 3D deck.gl geometry occludes correctly against basemap layers; `overlaid` mode composites deck.gl on a separate canvas above. The overlay's view-state is not authored — it syncs from the maplibre `Camera` each `move` through the required getters `getCenter`/`getZoom`/`getBearing`/`getPitch`/`getPadding` (+ `getProjection` for globe), with `project`/`unproject` supplying overlay-mark screen↔lngLat; deck's `getFreeCameraOptions?()` is mapbox-only and guarded, so under maplibre deck derives camera altitude from the map's internal `transform.elevation`. One context, one camera.
- Stack with `@geoarrow/deck.gl-geoarrow` + `apache-arrow` (`.api/geoarrow-deck.gl-geoarrow.md`, `.api/apache-arrow.md`): GeoArrow deck.gl layers consume `apache-arrow` `RecordBatch`es column-wise (catalog-bound grain: one `RecordBatch` per layer, fanned from `Table.batches`); the WKB→geometry decode stays in `wire` (the seam law), so maplibre/deck.gl receive decoded columnar geometry and never re-parse WKB here.
- Stack with `@turf/turf` (`.api/turf-turf.md`): planar ops (buffer/simplify/intersect) run on decoded GeoJSON in-browser as the NTS-equivalent peer, feeding a `GeoJSONSource` `setData` or a deck.gl layer — turf is the compute peer, maplibre the render surface.
- Stack with `@effect-atom` + `Stream`/`Match` (universal, `.api/effect.md`, `.api/effect-atom-atom.md`): the `Map` instance and camera state bind through the one panel atom (`ONE_FOLD_ONE_BINDING`); `on('moveend'|'idle'|'click', …)` `Subscription`s fold into the atom (the `Stream`-from-events idiom), and `Match` dispatches typed event payloads. React owns only mount (`new Map`) and unmount (`remove()`); the imperative map lifecycle never leaks into render.
- Stack with the `browser` runtime port: the DOM container, WebGL context, and worker pool are `browser`-provided — `viewer` declares the viewport/worker port and `browser` supplies it at app composition; `transformRequest` routes tile URLs through the app's auth boundary.

[LOCAL_ADMISSION]:
- `scope:viewer` project-local; the core `ui` never imports it — heavy render deps stay compile-time excluded from the non-spatial majority.
- one GL context and one camera: deck.gl interleaves through `addControl`/`CustomLayerInterface`, never a peer context; a second camera source is the defect.
- WKB decode stays in `wire`; style is authored as `*Specification` data; coordinate math goes through `project`/`unproject`, never a hand-rolled projection.

[RAIL_LAW]:
- Package: `maplibre-gl`
- Owns: the vector basemap `Map`, the single WebGL context + `Camera` + typed event rail, the declarative style vocabulary, the parameterized source/layer/control/handler rails, the DOM `Marker`/`Popup` overlays, and the worker/RTL/protocol globals
- Accept: one `new Map` per viewport, the `addSource`/`addLayer`/`addControl` rails, `MapboxOverlay` interleaving through `addControl` into the shared context, the `Camera` as the single navigation authority with its getters (`getCenter`/`getZoom`/`getBearing`/`getPitch`/`getPadding`) as deck's per-`move` view-state source, `project`/`unproject` for coordinate sync, `on`→`Subscription` folded into the panel atom, `*Specification` style data, `addProtocol`/`transformRequest` for transport
- Reject: a second GL context or peer camera, WKB decode in `ui`, style authored as code instead of `*Specification` data, hand-rolled projection math, importing it into the core `ui`, a React wrapper owning the imperative map lifecycle beyond mount/unmount
