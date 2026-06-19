# [API_CATALOGUE] maplibre-gl

`maplibre-gl` supplies the WebGL-accelerated interactive map engine: `Map` (exported also as `MapLibreMap`), camera/view controls, `LngLat`, `LngLatBounds`, `MercatorCoordinate`, event types, source classes (`GeoJSONSource`, `VectorTileSource`, `RasterTileSource`, `ImageSource`, `CanvasSource`), UI controls (`NavigationControl`, `AttributionControl`, `ScaleControl`, `GeolocateControl`, `Popup`, `Marker`), and the full `@maplibre/maplibre-gl-style-spec` type surface via `export type *`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `maplibre-gl`
- package: `maplibre-gl`
- module: `maplibre-gl`
- asset: `dist/maplibre-gl.d.ts` (dts-bundle-generator v9.5.1)
- namespace: `maplibregl` (UMD ambient)
- rail: viewport

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: map and camera types
- rail: viewport

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY] | [RAIL]                                            |
| :-----: | :-------------------- | :------------ | :------------------------------------------------ |
|  [01]   | `Map` / `MapLibreMap` | class         | primary map instance (alias of internal `Map$1`)  |
|  [02]   | `MapOptions`          | type alias    | constructor options (`container` required)        |
|  [03]   | `IControl`            | interface     | `{ onAdd(map), onRemove(), getDefaultPosition? }` |
|  [04]   | `Evented`             | class         | base event emitter for map and sources            |

[PUBLIC_TYPE_SCOPE]: geographic primitives
- rail: viewport

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY] | [RAIL]                                         |
| :-----: | :------------------- | :------------ | :--------------------------------------------- |
|  [01]   | `LngLat`             | class         | longitude/latitude coordinate                  |
|  [02]   | `LngLatBounds`       | class         | geographic bounding box                        |
|  [03]   | `MercatorCoordinate` | class         | web-mercator tile coordinate                   |
|  [04]   | `Point`              | class         | 2D pixel point (from `@mapbox/point-geometry`) |

[PUBLIC_TYPE_SCOPE]: source classes
- rail: viewport

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY] | [RAIL]                 |
| :-----: | :-------------------- | :------------ | :--------------------- |
|  [01]   | `GeoJSONSource`       | class         | GeoJSON data source    |
|  [02]   | `VectorTileSource`    | class         | MVT vector tile source |
|  [03]   | `RasterTileSource`    | class         | raster tile source     |
|  [04]   | `RasterDEMTileSource` | class         | terrain DEM source     |
|  [05]   | `ImageSource`         | class         | static image source    |
|  [06]   | `CanvasSource`        | class         | HTML canvas source     |
|  [07]   | `VideoSource`         | class         | video source           |

[PUBLIC_TYPE_SCOPE]: UI controls and overlays
- rail: viewport

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY] | [RAIL]                      |
| :-----: | :------------------- | :------------ | :-------------------------- |
|  [01]   | `NavigationControl`  | class         | zoom and rotate control     |
|  [02]   | `AttributionControl` | class         | attribution display control |
|  [03]   | `ScaleControl`       | class         | distance scale bar          |
|  [04]   | `GeolocateControl`   | class         | geolocation trigger control |
|  [05]   | `LogoControl`        | class         | MapLibre logo control       |
|  [06]   | `FullscreenControl`  | class         | fullscreen toggle control   |
|  [07]   | `TerrainControl`     | class         | terrain on/off control      |
|  [08]   | `GlobeControl`       | class         | globe/map projection toggle |
|  [09]   | `Popup`              | class         | tooltip/info popup overlay  |
|  [10]   | `Marker`             | class         | DOM marker overlay          |

[PUBLIC_TYPE_SCOPE]: event types
- rail: viewport

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY] | [RAIL]                       |
| :-----: | :--------------- | :------------ | :--------------------------- |
|  [01]   | `MapMouseEvent`  | class         | mouse interaction on the map |
|  [02]   | `MapTouchEvent`  | class         | touch interaction on the map |
|  [03]   | `MapWheelEvent`  | class         | wheel/pinch zoom event       |
|  [04]   | `GeoJSONFeature` | class         | picked feature from the map  |

[PUBLIC_TYPE_SCOPE]: interaction handlers
- rail: viewport

| [INDEX] | [SYMBOL]                           | [TYPE_FAMILY] | [RAIL]                        |
| :-----: | :--------------------------------- | :------------ | :---------------------------- |
|  [01]   | `ScrollZoomHandler`                | class         | scroll/pinch zoom             |
|  [02]   | `BoxZoomHandler`                   | class         | shift-drag zoom box           |
|  [03]   | `DragRotateHandler`                | class         | right-drag / ctrl-drag rotate |
|  [04]   | `DragPanHandler`                   | class         | drag-pan interaction          |
|  [05]   | `KeyboardHandler`                  | class         | keyboard navigation           |
|  [06]   | `DoubleClickZoomHandler`           | class         | double-click zoom             |
|  [07]   | `TwoFingersTouchZoomRotateHandler` | class         | two-finger pinch/rotate       |
|  [08]   | `CooperativeGesturesHandler`       | class         | cooperative gesture mode      |

[PUBLIC_TYPE_SCOPE]: request and protocol types
- rail: viewport

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [RAIL]                                                                                                    |
| :-----: | :------------------ | :------------ | :-------------------------------------------------------------------------------------------------------- |
|  [01]   | `RequestParameters` | type alias    | `{ url, headers?, method?, body?, type?, credentials?, cache?, referrerPolicy?, collectResourceTiming? }` |
|  [02]   | `AddProtocolAction` | type alias    | `(req, abortController) => Promise<GetResourceResponse<any>>`                                             |
|  [03]   | `AJAXError`         | class         | HTTP error with `status`, `statusText`, `url`, `body`                                                     |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: map construction
- rail: viewport

| [INDEX] | [SURFACE]          | [ENTRY_FAMILY] | [RAIL]                              |
| :-----: | :----------------- | :------------- | :---------------------------------- |
|  [01]   | `new Map(options)` | constructor    | `MapOptions` — `container` required |

[ENTRYPOINT_SCOPE]: camera operations (on `Map` instance)
- rail: viewport

| [INDEX] | [SURFACE]                                   | [ENTRY_FAMILY]  | [RAIL]                           |
| :-----: | :------------------------------------------ | :-------------- | :------------------------------- |
|  [01]   | `setCenter(lnglat)` / `getCenter()`         | camera mutator  | current map center               |
|  [02]   | `setZoom(zoom)` / `getZoom()`               | camera mutator  | current zoom level               |
|  [03]   | `setBearing(bearing)` / `getBearing()`      | camera mutator  | current bearing in degrees       |
|  [04]   | `setPitch(pitch)` / `getPitch()`            | camera mutator  | current pitch in degrees         |
|  [05]   | `fitBounds(bounds, options?)`               | camera mutator  | fits map to given `LngLatBounds` |
|  [06]   | `flyTo(options)`                            | animated camera | smooth fly animation             |
|  [07]   | `easeTo(options)`                           | animated camera | eased camera transition          |
|  [08]   | `jumpTo(options)`                           | instant camera  | instant camera reposition        |
|  [09]   | `panTo(lnglat, options?)`                   | animated camera | pan to coordinate                |
|  [10]   | `zoomTo(zoom, options?)` / `zoomIn/zoomOut` | animated camera | zoom to level or step            |
|  [11]   | `rotateTo(bearing, options?)`               | animated camera | rotate to bearing                |

[ENTRYPOINT_SCOPE]: layer and source management (on `Map` instance)
- rail: viewport

| [INDEX] | [SURFACE]                               | [ENTRY_FAMILY]  | [RAIL]                          |
| :-----: | :-------------------------------------- | :-------------- | :------------------------------ |
|  [01]   | `addSource(id, source)`                 | source mutation | adds data source                |
|  [02]   | `removeSource(id)`                      | source mutation | removes source                  |
|  [03]   | `getSource(id)`                         | source query    | `Source \| undefined`           |
|  [04]   | `addLayer(layer, beforeId?)`            | layer mutation  | adds style layer                |
|  [05]   | `removeLayer(id)`                       | layer mutation  | removes style layer             |
|  [06]   | `getLayer(id)`                          | layer query     | `StyleLayer \| undefined`       |
|  [07]   | `moveLayer(id, beforeId?)`              | layer mutation  | reorders layer in stack         |
|  [08]   | `setPaintProperty(layer, name, value)`  | style mutation  | updates paint property          |
|  [09]   | `setLayoutProperty(layer, name, value)` | style mutation  | updates layout property         |
|  [10]   | `setFilter(layer, filter?)`             | style mutation  | updates layer filter expression |

[ENTRYPOINT_SCOPE]: query and event operations (on `Map` instance)
- rail: viewport

| [INDEX] | [SURFACE]                                 | [ENTRY_FAMILY] | [RAIL]                         |
| :-----: | :---------------------------------------- | :------------- | :----------------------------- |
|  [01]   | `queryRenderedFeatures(point, options?)`  | feature query  | features at pixel point        |
|  [02]   | `querySourceFeatures(sourceId, options?)` | feature query  | features from source           |
|  [03]   | `project(lnglat)` / `unproject(point)`    | coordinate     | geographic ↔ pixel conversion  |
|  [04]   | `addControl(control, position?)`          | UI control     | adds `IControl` to map         |
|  [05]   | `removeControl(control)`                  | UI control     | removes `IControl`             |
|  [06]   | `on(type, listener)` / `off`              | event wiring   | map event subscription         |
|  [07]   | `once(type, listener)`                    | event wiring   | single-fire event subscription |

[ENTRYPOINT_SCOPE]: global utilities
- rail: viewport

| [INDEX] | [SURFACE]                                | [ENTRY_FAMILY] | [RAIL]                         |
| :-----: | :--------------------------------------- | :------------- | :----------------------------- |
|  [01]   | `addProtocol(customProtocol, fn)`        | protocol       | registers custom tile protocol |
|  [02]   | `removeProtocol(customProtocol)`         | protocol       | removes custom tile protocol   |
|  [03]   | `setRTLTextPlugin(url, lazy)`            | plugin         | configures right-to-left text  |
|  [04]   | `getRTLTextPluginStatus()`               | plugin         | current plugin load status     |
|  [05]   | `prewarm()`                              | worker         | pre-initializes worker pool    |
|  [06]   | `clearPrewarmedResources()`              | worker         | releases prewarmed resources   |
|  [07]   | `getWorkerCount()` / `setWorkerCount(n)` | worker         | WebWorker pool size control    |
|  [08]   | `getVersion()`                           | metadata       | library version string         |

## [04]-[IMPLEMENTATION_LAW]

[VIEWPORT_TOPOLOGY]:
- `Map` extends `Camera` extends `Evented`; handler instances (`scrollZoom`, `boxZoom`, `dragRotate`, `dragPan`, `keyboard`, `doubleClickZoom`, `touchZoomRotate`) are accessible as properties after construction
- `MapOptions.container` accepts either an `HTMLElement` or a DOM element string `id`; the element must be empty
- Sources must be added before referencing them in layer definitions; layers reference sources by `id`
- Style layers are rendered in stack order; `addLayer(layer, beforeId)` inserts before the named layer
- `queryRenderedFeatures` returns `GeoJSONFeature[]` from currently visible tiles; not all source features are guaranteed to be in the render tree
- `IControl` contract requires `onAdd(map): HTMLElement` (returns control container) and `onRemove()`: implementing `getDefaultPosition()` is optional
- `canvasContextAttributes.contextType` defaults to `'webgl2withfallback'`; force `'webgl2'` or `'webgl'` when the target platform requires it

[LOCAL_ADMISSION]:
- All style specification types (`LayerSpecification`, `SourceSpecification`, `StyleSpecification`, `FilterSpecification`, etc.) are admitted via `export type * from "@maplibre/maplibre-gl-style-spec"` — use them for paint/layout/filter typing.
- `LngLat` and `LngLatBounds` accept multiple input forms via static `convert()` methods; accept `LngLatLike` / `LngLatBoundsLike` at call sites.
- `transformRequest` in `MapOptions` enables request interception for custom auth headers.

[RAIL_LAW]:
- Package: `maplibre-gl`
- Owns: WebGL interactive map rendering, camera, sources, layers, controls, and protocol handling
- Accept: `Map` for all viewport rendering; `IControl` for all map UI widgets; style-spec types for layer/source/filter definitions
- Reject: hand-rolled tile fetching, manual WebGL canvas management, custom geo-projection outside `MercatorCoordinate`
