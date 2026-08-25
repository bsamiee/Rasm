# [TS_UI_API_DECK_GL_MAPBOX]

`@deck.gl/mapbox` binds one `@deck.gl/core` `Deck` to a mapbox/maplibre `Map` as a single `MapboxOverlay` `IControl`: `addControl` mounts deck's layers over the basemap and syncs `viewState` from the map camera each `move`. `interleaved` shares the map's WebGL2 context and depth buffer to z-slot layers by `beforeId`; the map owns every camera prop, the atom fold owns `layers`/`effects`.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the overlay, its camera-stripped props, and the structural map contract deck ships so no maplibre typings dependency is needed.

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY] | [CAPABILITY]                                  |
| :-----: | :--------------------- | :------------ | :-------------------------------------------- |
|  [01]   | `MapboxOverlay`        | class         | `IControl` binding one `Deck` to one map      |
|  [02]   | `MapboxOverlayProps`   | type          | camera-stripped `DeckProps` + `interleaved?`  |
|  [03]   | `Map`                  | interface     | structural camera and layer source deck reads |
|  [04]   | `IControl`             | interface     | control contract `MapboxOverlay` satisfies    |
|  [05]   | `CustomLayerInterface` | interface     | interleaved per-layer render contract         |
|  [06]   | `LayerOverlayProps`    | type          | interleaved z-slot placement                  |
|  [07]   | `ControlPosition`      | union         | overlaid control corner                       |

[SHAPE]: structural members each contract declares.
- `[MAP]`: `addControl`/`removeControl`/`hasControl`, camera getters `getCenter`/`getZoom`/`getBearing`/`getPitch`/`getPadding`/`getRenderWorldCopies` (drives `viewState.repeat`), optional `getProjection?`/`getTerrain?`/`getFreeCameraOptions?`, interleaved layer ops `addSource`/`addLayer`/`moveLayer`/`removeLayer`, `getCanvas`/`getContainer`, `isStyleLoaded`/`triggerRepaint`, `on`/`off`/`once`.
- `[CUSTOMLAYERINTERFACE]`: `id`, `type:'custom'`, `renderingMode?`, `render(gl,matrix)`, `onAdd?`, `onRemove?`, `prerender?`.
- `[LAYEROVERLAYPROPS]`: `slot?: 'bottom'|'middle'|'top'`, `beforeId?`.
- `[CONTROLPOSITION]`: `'top-left'|'top-right'|'bottom-left'|'bottom-right'`.

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: members on `MapboxOverlay`; the map drives the camera, the overlay forwards props and picking to its `Deck`.

| [INDEX] | [SURFACE]                                  | [SHAPE]  | [CAPABILITY]                                   |
| :-----: | :----------------------------------------- | :------- | :--------------------------------------------- |
|  [01]   | `new MapboxOverlay(props)`                 | ctor     | wrap one `Deck` as an `IControl`               |
|  [02]   | `MapboxOverlay.setProps(props)`            | instance | patch the `Deck`, partial and diffed           |
|  [03]   | `MapboxOverlay.filterProps(props)`         | instance | strip camera props before `Deck`               |
|  [04]   | `MapboxOverlay.onAdd` / `onRemove`         | instance | `IControl` acquire and release                 |
|  [05]   | `MapboxOverlay.finalize()`                 | instance | release the `Deck` and free GPU                |
|  [06]   | `MapboxOverlay.getCanvas()`                | instance | interleaved map canvas, else deck canvas       |
|  [07]   | `MapboxOverlay.pickObject` / `pickObjects` | instance | forwarded single and marquee pick → `GlobalId` |
|  [08]   | `MapboxOverlay.pickMultipleObjects`        | instance | forwarded stacked pick → `GlobalId`            |
|  [09]   | `MapboxOverlay.getDefaultPosition()`       | instance | `ControlPosition` placement                    |

- `MapboxOverlay.filterProps`: strips `useDevicePixels` on every path and gates on the live `interleaved` mode rather than the passed prop, so the map owns device-pixel ratio under interleave and a caller-supplied value never reaches the shared context.
- `new MapboxOverlay`: overlaid construction pins `deviceProps.createCanvasContext.pixelSizeSource: 'css-dpr'` over the caller's `deviceProps` to align the deck canvas with the basemap, overriding that one key and passing the rest through.

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- one overlay owns one `Deck` added to one `Map` via `addControl`; a second overlay or a free `Deck` on the same canvas is the defect.
- `MapboxOverlayProps` omits `viewState`/`initialViewState`/`controller`/`width`/`height`/`canvas`/`gl`, so the map alone drives the camera: pan/zoom/pitch mirror into deck each `move`, and hand-syncing under the overlay is the free-`Deck` path only.
- `interleaved` is one boolean mode: `true` shares the map's WebGL2 context and depth buffer and registers a `CustomLayerInterface` per layer (z-sorted by `beforeId`/`slot`, so 3D deck geometry occludes basemap layers), `false` overlays a separate canvas — never two overlay types.
- `interleaved` picks the device-pixel authority along with the compositing target: a shared context renders at the map's own `pixelRatio` and a separate deck canvas at the CSS device-pixel ratio the overlay pins, so whichever surface owns the pixels owns their ratio and no deck prop arbitrates.

[STACKING]:
- `maplibre-gl`(`.api/maplibre-gl.md`): its `Map` satisfies the structural `Map`/`IControl` by shape, so `addControl(new MapboxOverlay({interleaved, layers}))` mounts it; deck reads `getCenter`/`getZoom`/`getBearing`/`getPitch`/`getPadding` (+ `getProjection?` for globe) each `move`, and screen↔lngLat marks route through `Map.project`/`unproject`, satisfying the `viewer/geo/project` seam. `MapOptions.pixelRatio` on `new Map` governs the device-pixel ratio of the basemap and of every interleaved deck layer sharing that context.
- `@deck.gl/core`(`.api/deck.gl-core.md`): the `viewer` acquires the overlay in an Effect `acquireRelease` — `addControl`/`onAdd` acquire, `finalize`/`removeControl` release — holds it in a React ref, and a derived atom folds `layers`/`effects` into `setProps` on change.
- `@deck.gl/layers`(`.api/deck.gl-layers.md`) + `@deck.gl/geo-layers`(`.api/deck.gl-geo-layers.md`): the `layers` prop takes any deck layer — a `TileLayer`/`MVTLayer` basemap overlay or a `GeoJsonLayer`/cell thematic layer composes over the basemap with automatic camera sync; interleave is required when deck 3D must occlude basemap 3D.
- picking→selection: `pickObjects` (marquee) or a layer `onClick`→`PickingInfo.object` is where a composed-map feature becomes a `mark/selection` `GlobalId`; the overlay forwards to `Deck` unchanged.

[LOCAL_ADMISSION]:
- imported only inside `ui/viewer` (`scope:viewer`), acquired and released with the map.
- `MapboxOverlay` binds every map-backed surface; a free `Deck` + manual `WebMercatorViewport` sync serves only map-less scenes (orthographic, first-person).
