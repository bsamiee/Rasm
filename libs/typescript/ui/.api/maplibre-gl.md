# [API_CATALOGUE] maplibre-gl

`maplibre-gl` supplies the WebGL-accelerated interactive map engine: `Map` (exported also as `MapLibreMap`), camera/view controls, `LngLat`, `LngLatBounds`, `MercatorCoordinate`, event types, source classes (`GeoJSONSource`, `VectorTileSource`, `RasterTileSource`, `ImageSource`, `CanvasSource`), UI controls (`NavigationControl`, `AttributionControl`, `ScaleControl`, `GeolocateControl`, `Popup`, `Marker`), and the full `@maplibre/maplibre-gl-style-spec` type surface via `export type *`.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `maplibre-gl`
- package: `maplibre-gl`
- module: `maplibre-gl`
- asset: `dist/maplibre-gl.d.ts` (dts-bundle-generator v9.5.1)
- namespace: `maplibregl` (UMD ambient)
- rail: viewport

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: map and camera types
- rail: viewport

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY] | [RAIL]                                            |
| :-----: | :-------------------- | :------------ | :------------------------------------------------ |
|   [1]   | `Map` / `MapLibreMap` | class         | primary map instance (alias of internal `Map$1`)  |
|   [2]   | `MapOptions`          | type alias    | constructor options (`container` required)        |
|   [3]   | `IControl`            | interface     | `{ onAdd(map), onRemove(), getDefaultPosition? }` |
|   [4]   | `Evented`             | class         | base event emitter for map and sources            |

[PUBLIC_TYPE_SCOPE]: geographic primitives
- rail: viewport

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY] | [RAIL]                                         |
| :-----: | :------------------- | :------------ | :--------------------------------------------- |
|   [1]   | `LngLat`             | class         | longitude/latitude coordinate                  |
|   [2]   | `LngLatBounds`       | class         | geographic bounding box                        |
|   [3]   | `MercatorCoordinate` | class         | web-mercator tile coordinate                   |
|   [4]   | `Point`              | class         | 2D pixel point (from `@mapbox/point-geometry`) |

[PUBLIC_TYPE_SCOPE]: source classes
- rail: viewport

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY] | [RAIL]                 |
| :-----: | :-------------------- | :------------ | :--------------------- |
|   [1]   | `GeoJSONSource`       | class         | GeoJSON data source    |
|   [2]   | `VectorTileSource`    | class         | MVT vector tile source |
|   [3]   | `RasterTileSource`    | class         | raster tile source     |
|   [4]   | `RasterDEMTileSource` | class         | terrain DEM source     |
|   [5]   | `ImageSource`         | class         | static image source    |
|   [6]   | `CanvasSource`        | class         | HTML canvas source     |
|   [7]   | `VideoSource`         | class         | video source           |

[PUBLIC_TYPE_SCOPE]: UI controls and overlays
- rail: viewport

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY] | [RAIL]                      |
| :-----: | :------------------- | :------------ | :-------------------------- |
|   [1]   | `NavigationControl`  | class         | zoom and rotate control     |
|   [2]   | `AttributionControl` | class         | attribution display control |
|   [3]   | `ScaleControl`       | class         | distance scale bar          |
|   [4]   | `GeolocateControl`   | class         | geolocation trigger control |
|   [5]   | `LogoControl`        | class         | MapLibre logo control       |
|   [6]   | `FullscreenControl`  | class         | fullscreen toggle control   |
|   [7]   | `TerrainControl`     | class         | terrain on/off control      |
|   [8]   | `GlobeControl`       | class         | globe/map projection toggle |
|   [9]   | `Popup`              | class         | tooltip/info popup overlay  |
|  [10]   | `Marker`             | class         | DOM marker overlay          |

[PUBLIC_TYPE_SCOPE]: event types
- rail: viewport

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY] | [RAIL]                       |
| :-----: | :--------------- | :------------ | :--------------------------- |
|   [1]   | `MapMouseEvent`  | class         | mouse interaction on the map |
|   [2]   | `MapTouchEvent`  | class         | touch interaction on the map |
|   [3]   | `MapWheelEvent`  | class         | wheel/pinch zoom event       |
|   [4]   | `GeoJSONFeature` | class         | picked feature from the map  |

[PUBLIC_TYPE_SCOPE]: interaction handlers
- rail: viewport

| [INDEX] | [SYMBOL]                           | [TYPE_FAMILY] | [RAIL]                        |
| :-----: | :--------------------------------- | :------------ | :---------------------------- |
|   [1]   | `ScrollZoomHandler`                | class         | scroll/pinch zoom             |
|   [2]   | `BoxZoomHandler`                   | class         | shift-drag zoom box           |
|   [3]   | `DragRotateHandler`                | class         | right-drag / ctrl-drag rotate |
|   [4]   | `DragPanHandler`                   | class         | drag-pan interaction          |
|   [5]   | `KeyboardHandler`                  | class         | keyboard navigation           |
|   [6]   | `DoubleClickZoomHandler`           | class         | double-click zoom             |
|   [7]   | `TwoFingersTouchZoomRotateHandler` | class         | two-finger pinch/rotate       |
|   [8]   | `CooperativeGesturesHandler`       | class         | cooperative gesture mode      |

[PUBLIC_TYPE_SCOPE]: request and protocol types
- rail: viewport

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [RAIL]                                                                                                    |
| :-----: | :------------------ | :------------ | :-------------------------------------------------------------------------------------------------------- |
|   [1]   | `RequestParameters` | type alias    | `{ url, headers?, method?, body?, type?, credentials?, cache?, referrerPolicy?, collectResourceTiming? }` |
|   [2]   | `AddProtocolAction` | type alias    | `(req, abortController) => Promise<GetResourceResponse<any>>`                                             |
|   [3]   | `AJAXError`         | class         | HTTP error with `status`, `statusText`, `url`, `body`                                                     |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: map construction
- rail: viewport

| [INDEX] | [SURFACE]          | [ENTRY_FAMILY] | [RAIL]                              |
| :-----: | :----------------- | :------------- | :---------------------------------- |
|   [1]   | `new Map(options)` | constructor    | `MapOptions` — `container` required |

[ENTRYPOINT_SCOPE]: camera operations (on `Map` instance)
- rail: viewport

| [INDEX] | [SURFACE]                                   | [ENTRY_FAMILY]  | [RAIL]                           |
| :-----: | :------------------------------------------ | :-------------- | :------------------------------- |
|   [1]   | `setCenter(lnglat)` / `getCenter()`         | camera mutator  | current map center               |
|   [2]   | `setZoom(zoom)` / `getZoom()`               | camera mutator  | current zoom level               |
|   [3]   | `setBearing(bearing)` / `getBearing()`      | camera mutator  | current bearing in degrees       |
|   [4]   | `setPitch(pitch)` / `getPitch()`            | camera mutator  | current pitch in degrees         |
|   [5]   | `fitBounds(bounds, options?)`               | camera mutator  | fits map to given `LngLatBounds` |
|   [6]   | `flyTo(options)`                            | animated camera | smooth fly animation             |
|   [7]   | `easeTo(options)`                           | animated camera | eased camera transition          |
|   [8]   | `jumpTo(options)`                           | instant camera  | instant camera reposition        |
|   [9]   | `panTo(lnglat, options?)`                   | animated camera | pan to coordinate                |
|  [10]   | `zoomTo(zoom, options?)` / `zoomIn/zoomOut` | animated camera | zoom to level or step            |
|  [11]   | `rotateTo(bearing, options?)`               | animated camera | rotate to bearing                |

[ENTRYPOINT_SCOPE]: layer and source management (on `Map` instance)
- rail: viewport

| [INDEX] | [SURFACE]                               | [ENTRY_FAMILY]  | [RAIL]                          |
| :-----: | :-------------------------------------- | :-------------- | :------------------------------ |
|   [1]   | `addSource(id, source)`                 | source mutation | adds data source                |
|   [2]   | `removeSource(id)`                      | source mutation | removes source                  |
|   [3]   | `getSource(id)`                         | source query    | `Source \| undefined`           |
|   [4]   | `addLayer(layer, beforeId?)`            | layer mutation  | adds style layer                |
|   [5]   | `removeLayer(id)`                       | layer mutation  | removes style layer             |
|   [6]   | `getLayer(id)`                          | layer query     | `StyleLayer \| undefined`       |
|   [7]   | `moveLayer(id, beforeId?)`              | layer mutation  | reorders layer in stack         |
|   [8]   | `setPaintProperty(layer, name, value)`  | style mutation  | updates paint property          |
|   [9]   | `setLayoutProperty(layer, name, value)` | style mutation  | updates layout property         |
|  [10]   | `setFilter(layer, filter?)`             | style mutation  | updates layer filter expression |

[ENTRYPOINT_SCOPE]: query and event operations (on `Map` instance)
- rail: viewport

| [INDEX] | [SURFACE]                                 | [ENTRY_FAMILY] | [RAIL]                         |
| :-----: | :---------------------------------------- | :------------- | :----------------------------- |
|   [1]   | `queryRenderedFeatures(point, options?)`  | feature query  | features at pixel point        |
|   [2]   | `querySourceFeatures(sourceId, options?)` | feature query  | features from source           |
|   [3]   | `project(lnglat)` / `unproject(point)`    | coordinate     | geographic ↔ pixel conversion  |
|   [4]   | `addControl(control, position?)`          | UI control     | adds `IControl` to map         |
|   [5]   | `removeControl(control)`                  | UI control     | removes `IControl`             |
|   [6]   | `on(type, listener)` / `off`              | event wiring   | map event subscription         |
|   [7]   | `once(type, listener)`                    | event wiring   | single-fire event subscription |

[ENTRYPOINT_SCOPE]: global utilities
- rail: viewport

| [INDEX] | [SURFACE]                                | [ENTRY_FAMILY] | [RAIL]                         |
| :-----: | :--------------------------------------- | :------------- | :----------------------------- |
|   [1]   | `addProtocol(customProtocol, fn)`        | protocol       | registers custom tile protocol |
|   [2]   | `removeProtocol(customProtocol)`         | protocol       | removes custom tile protocol   |
|   [3]   | `setRTLTextPlugin(url, lazy)`            | plugin         | configures right-to-left text  |
|   [4]   | `getRTLTextPluginStatus()`               | plugin         | current plugin load status     |
|   [5]   | `prewarm()`                              | worker         | pre-initializes worker pool    |
|   [6]   | `clearPrewarmedResources()`              | worker         | releases prewarmed resources   |
|   [7]   | `getWorkerCount()` / `setWorkerCount(n)` | worker         | WebWorker pool size control    |
|   [8]   | `getVersion()`                           | metadata       | library version string         |

## [4]-[IMPLEMENTATION_LAW]

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
