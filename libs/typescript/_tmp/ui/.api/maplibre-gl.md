# [API_CATALOGUE] maplibre-gl

`maplibre-gl` is the WebGL2 interactive-map engine and the `base`-substrate owner of the geo surface: `Map` (aliased `MapLibreMap`) extends an internal `Camera` extends `Evented`, driving camera animation, a style/source/layer graph typed by the full `@maplibre/maplibre-gl-style-spec` re-export, feature-state, terrain/sky/light/projection, DOM `Marker`/`Popup` overlays, an `IControl` widget slot, and a fully typed event system generic over the `MapEventType`/`MapLayerEventType` name-to-payload maps. In `ui` the `Map` is an `Effect.acquireRelease` resource under the `platform` `BrowserPlatform`, the `base` arm of the `GeoSeriesLayer` `$match` (`render/geo.md#GEO_SERIES_LAYER`); deck.gl overlays interleave through the `@deck.gl/mapbox` `MapboxOverlay` `IControl` (`deck.gl-mapbox.md`), and the surface owns no decode of its own.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `maplibre-gl`
- package / version: `maplibre-gl` @ `5.24.0`
- license: `BSD-3-Clause`
- module: ESM `dist/maplibre-gl.js` + types `dist/maplibre-gl.d.ts`; CSS `dist/maplibre-gl.css` (required for controls/overlays); UMD global `maplibregl` via `export as namespace maplibregl`
- asset: `node_modules/maplibre-gl/dist/maplibre-gl.d.ts` (TSDECL)
- exports: ~48 top-level classes (`Map`/`MapLibreMap`, `Evented`, the source/control/overlay/handler families), 18 module functions (protocol, RTL, worker, image-request, tile-mesh, version, time), the `config` global, `EXTENT = 8192`, `Point`, `Color`
- style-spec re-export: `export type * from "@maplibre/maplibre-gl-style-spec"` — the entire `StyleSpecification`/`LayerSpecification`/`SourceSpecification`/`FilterSpecification`/`ExpressionSpecification`/`*Specification` type surface flows through this package; import style types from `maplibre-gl`, never the spec package directly
- rail: viewport

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: map instance, event emitter, and widget contract
- rail: viewport

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY] | [NOTE]                                                                                            |
| :-----: | :-------------------- | :------------ | :------------------------------------------------------------------------------------------------ |
|  [01]   | `Map` / `MapLibreMap` | class         | the primary instance (internal `Map$1`, extends internal `Camera` extends `Evented`); both names are one class |
|  [02]   | `MapOptions`          | type          | constructor options; `container: HTMLElement \| string` required, plus `style`, `center`, `zoom`, `transformRequest`, `canvasContextAttributes`, `maxBounds`, `attributionControl`, … |
|  [03]   | `Evented`             | class         | the event-emitter base of `Map` and every source; owns `on`/`once`/`off`/`fire`/`listens`         |
|  [04]   | `Subscription`        | interface     | `{ unsubscribe(): void }` — the disposal handle `on`/`once` return; the finalizer for an event resource |
|  [05]   | `IControl`            | interface     | `{ onAdd(map): HTMLElement; onRemove(map): void; readonly getDefaultPosition?: () => ControlPosition }` — the map-widget contract |
|  [06]   | `ControlPosition`     | union         | `"top-left" \| "top-right" \| "bottom-left" \| "bottom-right"`                                    |
|  [07]   | `CameraOptions` / `AnimationOptions` / `PaddingOptions` | type | the camera/view-state shape — `CameraOptions` = `center`/`zoom`/`bearing`/`pitch`/`roll`/`elevation`; `padding` (`PaddingOptions`) rides the `jumpTo`/`easeTo`/`flyTo`/`fitBounds` transition options and `setPadding`, never bare `CameraOptions`. `AnimationOptions` carries the `duration`/`easing`/`essential` of a transition; maplibre exports **no** `ViewState` type — a view-state field types against `CameraOptions` |

[PUBLIC_TYPE_SCOPE]: geographic primitives and their `Like` input-normalization
- rail: viewport

| [INDEX] | [SYMBOL]                                | [TYPE_FAMILY] | [NOTE]                                                                                            |
| :-----: | :-------------------------------------- | :------------ | :------------------------------------------------------------------------------------------------ |
|  [01]   | `LngLat` / `LngLatLike`                 | class + union | coordinate; `LngLatLike = LngLat \| { lng, lat } \| { lon, lat } \| [lng, lat]` normalized by static `LngLat.convert` |
|  [02]   | `LngLatBounds` / `LngLatBoundsLike`     | class + union | bounding box; `LngLatBoundsLike = LngLatBounds \| [sw, ne] \| [w, s, e, n]` normalized by `LngLatBounds.convert` |
|  [03]   | `MercatorCoordinate`                    | class         | web-mercator coordinate with `fromLngLat`/`toLngLat`/`meterInMercatorCoordinateUnits`             |
|  [04]   | `Point` / `PointLike`                   | class + union | 2D pixel point (`@mapbox/point-geometry`); `PointLike = Point \| [x, y]`                          |
|  [05]   | `EdgeInsets`                            | class         | padding rectangle backing `PaddingOptions`/`setPadding`                                           |

[PUBLIC_TYPE_SCOPE]: the parameterized event system — one name-to-payload map, not a fixed roster
- rail: viewport

| [INDEX] | [SYMBOL]                              | [TYPE_FAMILY] | [NOTE]                                                                                            |
| :-----: | :------------------------------------ | :------------ | :------------------------------------------------------------------------------------------------ |
|  [01]   | `MapEventType`                        | type map      | the map-level event dictionary (52 keys): `load`/`idle`/`render`/`remove`/`resize`, `error`, `data`/`dataloading`/`sourcedata`/`styledata`/`styleimagemissing`/`tiledataloading`, `move`/`movestart`/`moveend`, `zoom`/`rotate`/`pitch`/`drag` (+`start`/`end`), `wheel`, `mouse*`/`touch*`/`click`/`dblclick`/`contextmenu`, `terrain`/`projectiontransition`/`webglcontext*` — each key maps to its payload class |
|  [02]   | `MapLayerEventType`                    | type map      | the layer-scoped pointer dictionary: `click`/`dblclick`/`mouse*`/`contextmenu` → `MapLayerMouseEvent`, `touch*` → `MapLayerTouchEvent`; keyed when `on` is given a layer id |
|  [03]   | `MapMouseEvent` / `MapTouchEvent` / `MapWheelEvent` | class | pointer payloads carrying `point`, `lngLat`, `originalEvent`, `preventDefault()`                 |
|  [04]   | `MapLayerMouseEvent` / `MapLayerTouchEvent` | class   | layer-scoped payloads adding `features: MapGeoJSONFeature[]` hit under the layer                  |
|  [05]   | `MapLibreEvent` / `MapDataEvent` / `MapSourceDataEvent` / `MapStyleDataEvent` / `MapContextEvent` | class | lifecycle/data/style/context payloads |
|  [06]   | `ErrorEvent`                          | class         | the `error`-event payload wrapping the thrown `error`                                             |

[PUBLIC_TYPE_SCOPE]: sources — added by style-spec discriminant, queried as typed instances
- rail: viewport

| [INDEX] | [SYMBOL]                                                                              | [TYPE_FAMILY] | [NOTE]                                                                        |
| :-----: | :------------------------------------------------------------------------------------ | :------------ | :--------------------------------------------------------------------------- |
|  [01]   | `GeoJSONSource`                                                                        | class         | GeoJSON source; `setData`/`getData`/`updateData`/`getClusterExpansionZoom`/`getClusterChildren`/`getClusterLeaves` |
|  [02]   | `VectorTileSource` / `RasterTileSource` / `RasterDEMTileSource`                        | class         | tiled sources; `setTiles`/`setUrl` for runtime retargeting; DEM backs terrain |
|  [03]   | `ImageSource` / `CanvasSource` / `VideoSource`                                         | class         | media sources; `ImageSource.updateImage`, `CanvasSource`/`VideoSource.play`/`pause` |

[PUBLIC_TYPE_SCOPE]: controls, overlays, and interaction handlers — one `IControl` mount, one handler-property family
- rail: viewport

| [INDEX] | [SYMBOL]                                                                                                              | [TYPE_FAMILY] | [NOTE]                                                            |
| :-----: | :------------------------------------------------------------------------------------------------------------------- | :------------ | :--------------------------------------------------------------- |
|  [01]   | `NavigationControl` / `AttributionControl` / `ScaleControl` / `GeolocateControl` / `LogoControl` / `FullscreenControl` / `TerrainControl` / `GlobeControl` | class | `IControl` widgets, each with an `*Options` type; mounted via `addControl(control, position?)` |
|  [02]   | `Marker` / `MarkerOptions`                                                                                           | class         | draggable DOM marker; `setLngLat`/`getLngLat`/`setPopup`/`togglePopup`/`setDraggable`/`setRotation`/`setOffset`/`add`/`remove`/`toggleClassName` |
|  [03]   | `Popup` / `PopupOptions`                                                                                             | class         | DOM popup; `setLngLat`/`setHTML`/`setText`/`setDOMContent`/`setMaxWidth`/`addTo`/`isOpen`/`remove` |
|  [04]   | `ScrollZoomHandler` / `BoxZoomHandler` / `DragRotateHandler` / `DragPanHandler` / `KeyboardHandler` / `DoubleClickZoomHandler` / `CooperativeGesturesHandler` | class | camera-interaction handlers exposed as `map.<name>` properties with `enable()`/`disable()`/`isEnabled()` |
|  [05]   | `TwoFingersTouchZoomRotateHandler` / `TwoFingersTouchZoomHandler` / `TwoFingersTouchRotateHandler` / `TwoFingersTouchPitchHandler` | class | the touch-gesture handler family                                 |

[PUBLIC_TYPE_SCOPE]: features, feature-state, request/protocol, and style-mutation options
- rail: viewport

| [INDEX] | [SYMBOL]                                | [TYPE_FAMILY] | [NOTE]                                                                                            |
| :-----: | :-------------------------------------- | :------------ | :------------------------------------------------------------------------------------------------ |
|  [01]   | `MapGeoJSONFeature` / `GeoJSONFeature`  | type + class  | `MapGeoJSONFeature = GeoJSONFeature & { layer, source, sourceLayer, state }` — the `queryRenderedFeatures`/`querySourceFeatures` return |
|  [02]   | `FeatureIdentifier`                      | type          | `{ id, source, sourceLayer? }` — the `setFeatureState`/`getFeatureState`/`removeFeatureState` target |
|  [03]   | `RequestParameters` / `GetResourceResponse<T>` / `AddProtocolAction` | type | `AddProtocolAction = (req: RequestParameters, abortController: AbortController) => Promise<GetResourceResponse<any>>` — the custom-protocol fetcher |
|  [04]   | `AJAXError`                              | class         | HTTP error with `status`, `statusText`, `url`, `body`                                             |
|  [05]   | `StyleSetterOptions` / `StyleSwapOptions` | type        | `{ validate? }` on paint/layout/filter setters; `{ diff?, transformStyle? }` on `setStyle`        |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: construction and lifecycle (on `Map`)
- rail: viewport

| [INDEX] | [SURFACE]                                             | [ENTRY_FAMILY] | [NOTE]                                                            |
| :-----: | :---------------------------------------------------- | :------------- | :--------------------------------------------------------------- |
|  [01]   | `new Map(options: MapOptions)`                        | constructor    | `container` required; the acquire step of the resource            |
|  [02]   | `remove()` / `resize(eventData?)` / `redraw()`        | lifecycle      | teardown / relayout / force repaint; `remove()` is the release    |
|  [03]   | `getCanvas()` / `getContainer()` / `getCanvasContainer()` | DOM handle | the WebGL2 canvas and its wrappers                               |
|  [04]   | `loaded()` / `isStyleLoaded()` / `areTilesLoaded()`   | readiness      | first-render / style / tile readiness gates                       |

[ENTRYPOINT_SCOPE]: camera — mutators, animated transitions, and bounds derivation (on `Map`)
- rail: viewport

| [INDEX] | [SURFACE]                                                                                | [ENTRY_FAMILY]  | [NOTE]                                                            |
| :-----: | :--------------------------------------------------------------------------------------- | :-------------- | :--------------------------------------------------------------- |
|  [01]   | `setCenter`/`getCenter`, `setZoom`/`getZoom`, `setBearing`/`getBearing`, `setPitch`/`getPitch`, `setRoll`, `setPadding` | camera mutator  | atomic camera-field set/get; accept `LngLatLike`/`PointLike`     |
|  [02]   | `flyTo(options)` / `easeTo(options)` / `panTo(lnglat, opts?)` / `panBy(offset, opts?)` / `rotateTo(bearing, opts?)` / `zoomTo`/`zoomIn`/`zoomOut` | animated camera | eased/curved transitions sharing `AnimationOptions`+`CameraOptions` |
|  [03]   | `jumpTo(options)`                                                                         | instant camera  | non-animated multi-field reposition                              |
|  [04]   | `fitBounds(bounds, opts?)` / `fitScreenCoordinates(p0, p1, bearing, opts?)`               | fit             | frame a `LngLatBoundsLike` / a screen rectangle                  |
|  [05]   | `cameraForBounds(bounds, opts?)` / `calculateCameraOptionsFromTo(from, altFrom, to, altTo?)` | camera solve | derive `CameraOptions` without moving; the deck.gl view-state seed |
|  [06]   | `setMaxBounds`/`getBounds`, `setMinZoom`/`setMaxZoom`, `setMinPitch`/`setMaxPitch`        | constraint      | camera envelope limits                                            |
|  [07]   | `project(lnglat)` / `unproject(point)` / `queryTerrainElevation(lnglat)`                  | coordinate      | geographic ↔ pixel; terrain-sampled elevation                    |

[ENTRYPOINT_SCOPE]: style, sources, and layers (on `Map`)
- rail: viewport

| [INDEX] | [SURFACE]                                                                                | [ENTRY_FAMILY]  | [NOTE]                                                            |
| :-----: | :--------------------------------------------------------------------------------------- | :-------------- | :--------------------------------------------------------------- |
|  [01]   | `setStyle(style, opts?: StyleSwapOptions)` / `getStyle()`                                 | style           | swap or diff a `StyleSpecification \| string`; `transformStyle` intercepts |
|  [02]   | `addSource(id, source: SourceSpecification)` / `getSource(id)` / `removeSource(id)`       | source          | `getSource` returns the typed instance keyed by the spec `type` discriminant |
|  [03]   | `addLayer(layer, beforeId?)` / `getLayer(id)` / `removeLayer(id)` / `moveLayer(id, beforeId?)` | layer      | style layers render in stack order; `beforeId` inserts before a named layer |
|  [04]   | `setPaintProperty` / `setLayoutProperty` / `setFilter` / `getFilter` / `setLayerZoomRange` | style mutation | per-layer paint/layout/filter/zoom-range edits, all `StyleSetterOptions`-gated |

[ENTRYPOINT_SCOPE]: images, rendering environment, feature-state, and query (on `Map`)
- rail: viewport

| [INDEX] | [SURFACE]                                                                                | [ENTRY_FAMILY]  | [NOTE]                                                            |
| :-----: | :--------------------------------------------------------------------------------------- | :-------------- | :--------------------------------------------------------------- |
|  [01]   | `addImage`/`addImages`/`updateImage`/`getImage`/`hasImage`/`removeImage`/`listImages`/`loadImage` | image | the sprite/icon registry; `loadImage` fetches, `styleimagemissing` lazily supplies |
|  [02]   | `setTerrain`/`getTerrain`, `setSky`/`getSky`, `setLight`/`getLight`, `setProjection`/`getProjection`, `setPixelRatio` | environment | 3D terrain (needs a `RasterDEMTileSource`), sky, light, globe/mercator projection |
|  [03]   | `setFeatureState(target: FeatureIdentifier, state)` / `getFeatureState(target)` / `removeFeatureState(target, key?)` | feature-state | data-driven styling state (hover/select) read by style expressions |
|  [04]   | `queryRenderedFeatures(geometry?, opts?)` / `querySourceFeatures(sourceId, opts?)`        | feature query   | `MapGeoJSONFeature[]`; rendered = visible tiles, source = loaded tiles |

[ENTRYPOINT_SCOPE]: events and controls (on `Map`)
- rail: viewport

| [INDEX] | [SURFACE]                                                                                | [ENTRY_FAMILY]  | [NOTE]                                                            |
| :-----: | :--------------------------------------------------------------------------------------- | :-------------- | :--------------------------------------------------------------- |
|  [01]   | `on<T extends keyof MapEventType>(type, listener): Subscription`                          | event wiring    | typed map-level subscription; `listener` receives `MapEventType[T]` |
|  [02]   | `on<T extends keyof MapLayerEventType>(type, layer \| layerIds, listener): Subscription`  | event wiring    | layer-scoped overload; payload adds `features`                   |
|  [03]   | `once<T>(type, listener?)` / `off(type, listener)`                                        | event wiring    | single-fire (returns `this \| Promise`) and detach               |
|  [04]   | `addControl(control: IControl, position?: ControlPosition)` / `removeControl(control)` / `hasControl(control)` | control | the one widget-mount slot — every control and the deck.gl `MapboxOverlay` enters here |

[ENTRYPOINT_SCOPE]: module globals — protocol, RTL, worker pool, image requests, and metadata
- rail: viewport

| [INDEX] | [SURFACE]                                                                                | [ENTRY_FAMILY]  | [NOTE]                                                            |
| :-----: | :--------------------------------------------------------------------------------------- | :-------------- | :--------------------------------------------------------------- |
|  [01]   | `addProtocol(scheme, action: AddProtocolAction)` / `removeProtocol(scheme)`              | protocol        | register/unregister a custom tile/resource fetcher               |
|  [02]   | `setRTLTextPlugin(url, lazy)` / `getRTLTextPluginStatus()`                                | plugin          | right-to-left text shaping                                       |
|  [03]   | `prewarm()` / `clearPrewarmedResources()` / `getWorkerCount()`/`setWorkerCount(n)` / `getWorkerUrl()`/`setWorkerUrl(v)` / `importScriptInWorkers(url)` | worker | worker-pool warmup and sizing                                   |
|  [04]   | `getMaxParallelImageRequests()` / `setMaxParallelImageRequests(n)`                        | network         | image-fetch concurrency cap                                      |
|  [05]   | `config` / `EXTENT` / `getVersion()` / `createTileMesh(options, forceIndicesSize?)`       | metadata        | the global `Config`, the `8192` tile extent, the version, and a tile-mesh builder |

## [04]-[IMPLEMENTATION_LAW]

[VIEWPORT_TOPOLOGY]:
- `Map` extends `Camera` extends `Evented`; interaction handlers (`scrollZoom`, `boxZoom`, `dragRotate`, `dragPan`, `keyboard`, `doubleClickZoom`, `touchZoomRotate`, `touchPitch`, `cooperativeGestures`) are instance properties after construction, each with `enable()`/`disable()`/`isEnabled()`
- `MapOptions.container` accepts an `HTMLElement` or a DOM `id`; the element must be empty; `canvasContextAttributes.contextType` defaults to `'webgl2withfallback'`, forced to `'webgl2'`/`'webgl'` when the target requires it
- sources are added before the layers that reference them by `id`; style layers render in declaration/stack order and `addLayer(layer, beforeId)` inserts before a named layer
- `IControl.onAdd(map)` returns the widget container `HTMLElement`, `onRemove(map)` detaches; `getDefaultPosition()` is optional; every widget — first-party control, `MapboxOverlay`, or a custom one — mounts through `addControl`

[EVENT_SYSTEM]:
- `on`/`once`/`off` are generic over `MapEventType`/`MapLayerEventType`, so `map.on('click', e => …)` infers `e: MapMouseEvent` and `map.on('click', 'layer-id', e => …)` infers `e: MapLayerMouseEvent` with `e.features`; the event surface is the typed name-to-payload map, never a fixed enumeration of a few event classes
- `on`/`once` return a `Subscription`; the payload classes carry `preventDefault()` and (for pointer events) `point`/`lngLat`/`originalEvent`; `error` events route through `MapEventType.error` -> `ErrorEvent` rather than throwing

[INPUT_NORMALIZATION]:
- geographic inputs are `*Like` unions normalized by one static `convert` per primitive: accept `LngLatLike`/`LngLatBoundsLike`/`PointLike` at every call site (`setCenter`, `fitBounds`, `project`) and let `LngLat.convert`/`LngLatBounds.convert` own the array/object/instance shapes — never branch on input form at the call site
- style, source, layer, and filter shapes are the `@maplibre/maplibre-gl-style-spec` types re-exported from this package; type `styleSpec`, `addSource`, `addLayer`, `setFilter` against `StyleSpecification`/`SourceSpecification`/`LayerSpecification`/`FilterSpecification` — these are interior render types, never a re-decoded wire

[STACKING]:
- `Effect.acquireRelease` resource (canonical): acquire `new Map(options)`, gate on the `load` event, release `map.remove()`; bind the resource under `@effect/platform-browser` `BrowserRuntime`/`BrowserPlatform` (`effect-platform-browser.md`), so the map lifecycle is a scoped effect and never a free React ref (`render/geo.md#GEO_SERIES_LAYER`) — the `base` `$match` arm draws the `styleSpec`, the `overlay` arm mounts deck.gl
- deck.gl interleave: mount `new MapboxOverlay({ interleaved: true, layers })` through `map.addControl` (`deck.gl-mapbox.md`) — the overlay is an `IControl` that inserts deck.gl layers into this WebGL2 stack, auto-syncs the deck.gl view state to the map camera off the `move` event, and releases through `overlay.finalize()`; `cameraForBounds`/`calculateCameraOptionsFromTo` seed the initial deck.gl view state
- GeoArrow zero-decode: the `geoarrow`/`cell-index` arms bind `@geoarrow/deck.gl-geoarrow` `RecordBatch` layers to the `interchange` `GeometryRail` Arrow projection through the `MapboxOverlay` `layers` prop — maplibre owns the base substrate and camera, deck.gl owns the overlay, and no GeoJSON round-trip of the Arrow batch occurs (`render/geo.md`); the `base` arm's `styleSpec` cartography and the overlay geometry share one camera
- event → binding: wrap `map.on(type, listener): Subscription` as an `Effect.acquireRelease`/`Stream.async` whose finalizer calls `subscription.unsubscribe()`, feeding the typed `MapEventType[T]` payload (a `moveend` center/zoom, a layer `click` `features`) into the `binding/atom.md` `AtomBinding` view-state atom; `setCenter`/`setZoom` write back from the same atom — the map camera and the URL-resident `Atom.searchParam` state stay one source
- custom transport protocol: register the `interchange` transport as a `custom://` scheme via `addProtocol(scheme, action)` where `action: AddProtocolAction` returns `Promise<GetResourceResponse>` under the supplied `AbortController`, so tiles and sprites ride the same rail as the rest of the wire; `MapOptions.transformRequest` injects auth headers per request without a custom protocol
- feature-state selection: project a `Rasm.Bim` `GlobalId` selection set onto `setFeatureState(target: FeatureIdentifier, state)` and let style expressions read the state for hover/select highlighting, keyed by the same id the `overlay/bcf.md` selection carries — no parallel highlight layer

[LOCAL_ADMISSION]:
- hold the `Map` as an `Effect.acquireRelease` resource under `BrowserPlatform`, never a free React ref; gate first paint on `load`
- accept `*Like` inputs and let the static `convert` normalize; type all style/source/layer/filter work against the re-exported style-spec types
- mount every widget and deck.gl overlay through `addControl`; register custom fetching through `addProtocol`, never a fetch call inside a render leaf
- ship `dist/maplibre-gl.css`; controls, `Marker`, and `Popup` render blank without it

[RAIL_LAW]:
- package: `maplibre-gl`
- owns: WebGL2 base-map rendering — camera, the style/source/layer graph, feature-state, terrain/sky/light/projection, DOM overlays, the `IControl` slot, the typed event system, and custom protocol handling
- accept: `Map` for all base-substrate rendering; `IControl` for every widget and the deck.gl `MapboxOverlay`; the re-exported style-spec types for layer/source/filter definitions; `*Like` inputs; `addProtocol` for custom transports
- reject: a `Map` held as a free React ref instead of an `Effect.acquireRelease` resource; hand-rolled tile fetching or manual WebGL canvas management; a custom geo-projection outside `MercatorCoordinate`/`setProjection`; a GeoJSON round-trip of an Arrow batch the `GeometryRail` already projects; branching on `LngLatLike` input shape instead of `convert`
