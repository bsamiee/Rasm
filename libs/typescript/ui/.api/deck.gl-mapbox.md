# [TS_UI_API_DECK_GL_MAPBOX]

`@deck.gl/mapbox` binds a `@deck.gl/core` `Deck` to a maplibre-gl `Map` as one `MapboxOverlay` implementing the map's `IControl`: `map.addControl(new MapboxOverlay({interleaved, layers}))` mounts deck's layers over the basemap, syncing `viewState` from the map camera each `move`. `interleaved: true` shares the map's WebGL2 context and depth buffer, slotting layers by `beforeId`; `interleaved: false` (default) draws deck in its own canvas above; the map owns the camera props while the atom state fold feeds `layers`/`effects`. One class, one props type; `scope:viewer` project-local.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@deck.gl/mapbox`
- package: `@deck.gl/mapbox`
- license: `MIT`
- abi: browser WebGL2; `interleaved:true` requires the map's `Map#getCanvas` context to be WebGL2 and shares it with the `Deck`
- peer (`~catalog`): `@deck.gl/core` (`Deck`, `DeckProps`, picking), `@luma.gl/core` (shared WebGL2 `Device` under `interleaved`), `@math.gl/web-mercator` (dep; camera-state → deck `viewState` conversion); `maplibre-gl ^5.x` is NOT a dep or peer — the `Map` is structural (deck ships its own `Map`/`IControl` types), passed in by the app; see `.api/maplibre-gl.md`
- catalog-verdict: KEEP — the sole deck↔maplibre binding; the alternative (manual `WebMercatorViewport` sync of a free `Deck`) is only for map-less surfaces
- runtime: `scope:viewer` project-local; the overlay is a map `IControl`, bracketed by `addControl`/`removeControl`
- modules: `MapboxOverlay`, `MapboxOverlayProps`

## [02]-[OVERLAY_BINDING]

[TYPE_SCOPE]: `MapboxOverlay` — the one `IControl` that owns a `Deck`; the map drives the camera, the overlay forwards props and picking.
- `MapboxOverlay` is constructed with `MapboxOverlayProps` and added to the map as a control; `onAdd` acquires (over-lays a canvas, or in interleaved mode registers a `CustomLayerInterface` per layer), the map's `move` events sync deck's camera, and `onRemove`/`finalize` releases. `setProps` patches the underlying `Deck` (partial, diffed). Picking is forwarded to `Deck`.
- `MapboxOverlayProps` = `Omit<DeckProps, 'viewState'|'initialViewState'|'controller'|'width'|'height'|'canvas'|'gl'|'parent'|'_customRender'> & {interleaved?: boolean}` — the map owns every camera prop; members below sit on `MapboxOverlay`.

| [INDEX] | [SYMBOL]                                   | [SIGNATURE]                              | [CONSUMER_BOUNDARY]                              |
| :-----: | :----------------------------------------- | :--------------------------------------- | :----------------------------------------------- |
|  [01]   | `MapboxOverlay`                            | `new MapboxOverlay(props)` `IControl`    | deck↔maplibre control; `addControl` mounts it    |
|  [02]   | `MapboxOverlay.setProps`                   | `(props: MapboxOverlayProps) => void`    | imperative sink; atom-derived `layers`/`effects` |
|  [03]   | `MapboxOverlay.onAdd` / `onRemove`         | `(map) => HTMLDivElement` / `() => void` | `IControl` lifecycle; acquire/release `Deck`     |
|  [04]   | `MapboxOverlay.finalize`                   | `() => void`                             | full release; frees GPU (`Scope` release)        |
|  [05]   | `MapboxOverlay.getCanvas`                  | `() => HTMLCanvasElement \| null`        | interleaved → map canvas; overlaid → deck canvas |
|  [06]   | `MapboxOverlay.pickObject` / `pickObjects` | forwarded `Deck` picking                 | pick/marquee over composed scene → `GlobalId`    |
|  [07]   | `MapboxOverlay.pickMultipleObjects`        | forwarded `Deck` picking                 | stacked pick → `GlobalId`                        |
|  [08]   | `MapboxOverlay.getDefaultPosition`         | `() => ControlPosition`                  | control placement (`'top-left'`…) overlaid       |
|  [09]   | `MapboxOverlayProps`                       | Omit of `DeckProps` (above)              | map owns camera; deck owns `layers`/`effects`    |

[TYPE_SCOPE]: the shipped structural map contract — deck's own minimal maplibre/mapbox-compatible types, so no hard typings dependency.
- these are internal-facing types (not re-exported from the index) that define the structural `Map` the overlay drives; they document the members the `maplibre-gl` `Map` must satisfy and the interleaved custom-layer contract.

| [INDEX] | [SYMBOL]               | [ROLE]                                                                                      |
| :-----: | :--------------------- | :------------------------------------------------------------------------------------------ |
|  [01]   | `IControl`             | control interface `MapboxOverlay` satisfies; `addControl` accepts it                        |
|  [02]   | `Map` (structural)     | camera-source read each `move` to sync deck `viewState`; maplibre satisfies it structurally |
|  [03]   | `CustomLayerInterface` | interleaved contract — one per deck layer in the map stack                                  |
|  [04]   | `LayerOverlayProps`    | interleaved z-slotting of a deck layer against basemap layers                               |
|  [05]   | `ControlPosition`      | overlaid control corner                                                                     |

[SHAPE] by row (the structural members each type declares):
- [01]-[ICONTROL]: `{onAdd(map):HTMLElement, onRemove(map), getDefaultPosition?}`.
- [02]-[MAP]: `addControl`/`removeControl`/`hasControl`, camera getters `getCenter`/`getZoom`/`getBearing`/`getPitch`/`getPadding`/`getRenderWorldCopies` (last drives `viewState.repeat`), optional `getProjection?`/`getTerrain?`/`getFreeCameraOptions?` (`getFreeCameraOptions` mapbox-only, absent on maplibre → deck falls back to `getCameraTargetElevation`), `addSource`/`addLayer`/`moveLayer`/`removeLayer` (interleaved custom-layer insertion), `getCanvas`/`getContainer`, `isStyleLoaded`/`triggerRepaint`, `on`/`off`/`once`.
- [03]-[CUSTOMLAYERINTERFACE]: `{id, type:'custom', renderingMode?, render(gl,matrix), onAdd?, onRemove?, prerender?}`.
- [04]-[LAYEROVERLAYPROPS]: `{slot?: 'bottom'|'middle'|'top', beforeId?: string}`.
- [05]-[CONTROLPOSITION]: `'top-left'|'top-right'|'bottom-left'|'bottom-right'`.

## [03]-[IMPLEMENTATION_LAW]

[BINDING_TOPOLOGY]:
- one overlay, one deck, one map: `MapboxOverlay` owns exactly one `Deck`; it is added to exactly one `Map` via `addControl`. A second overlay on the same map, or a free `Deck` beside an overlay on one canvas, is the named defect.
- Map owns the camera: `MapboxOverlayProps` structurally forbids `viewState`/`initialViewState`/`controller`/`width`/`height`/`canvas`/`gl` — the map's pan/zoom/pitch is the single camera authority, mirrored into deck automatically on each `move`. Never sync the camera by hand under `MapboxOverlay`; that is the free-`Deck` path only.
- interleave is a mode, not a fork: `interleaved:true` shares the map's WebGL2 context + depth buffer and registers a `CustomLayerInterface` per deck layer (z-sorted with `beforeId`/`slot` per `LayerOverlayProps`, so 3D deck geometry occludes correctly against basemap layers); `interleaved:false` overlays a separate canvas. One class, one `interleaved` boolean — not two overlay types.
- props diff through `setProps`: like `Deck`, the overlay is patched, never rebuilt; `setProps({layers})` is the sink for the atom-derived layer tree.

[INTEGRATION_LAW]:
- Stack with `maplibre-gl` (`.api/maplibre-gl.md`): maplibre `Map` satisfies the structural `Map`/`IControl` contract, so `map.addControl(new MapboxOverlay({interleaved:true, layers}))` mounts it; deck reads `getCenter`/`getZoom`/`getBearing`/`getPitch`/`getPadding` (+ `getProjection?` for globe) each `move` to sync `viewState`. Screen↔lngLat for overlay marks routes through maplibre `Map.project`/`unproject` or deck's `WebMercatorViewport` — the `viewer/geo/project` seam is satisfied here.
- Stack with `@deck.gl/core` as a `Scope` resource: the `viewer` acquires the overlay in an Effect `acquireRelease` — `onAdd` (via `addControl`) acquires, `finalize` (or `removeControl`) releases — and holds it in a React 19 ref; an atom-derived `layers`/`effects` fold calls `setProps` on change. Map instance itself is a sibling ref the overlay is added to.
- Stack with `@deck.gl/layers` + `@deck.gl/geo-layers`: the `layers` prop is any deck layer; a `TileLayer`/`MVTLayer` basemap-overlay or a `GeoJsonLayer`/cell-family thematic layer composes over the maplibre basemap with automatic camera sync. Interleaved mode is required when deck 3D geometry must occlude/sort against basemap 3D (buildings, terrain).
- Stack with picking→selection: `MapboxOverlay.pickObjects` (marquee) / a deck-layer `onClick`→`PickingInfo.object` is the boundary where a feature over the composed map becomes a `mark/selection` `GlobalId`; the overlay forwards to the underlying `Deck` unchanged.

[LOCAL_ADMISSION]:
- imported only inside `ui/viewer` (`scope:viewer`); the overlay is a map control, acquired/released with the map.
- use `MapboxOverlay` for every map-backed surface; reach for a free-standing `Deck` + manual `WebMercatorViewport` camera sync only when there is no basemap (pure-deck orthographic/first-person scenes).
- do not import maplibre-gl's TypeScript types to type the overlay — deck's structural `Map`/`IControl` is the contract; the maplibre `Map` satisfies it by shape.
- `MapboxOverlayProps` deliberately omits camera props; passing `viewState`/`controller` is a type error, not a runtime override — the map owns them.

[RAIL_LAW]:
- Package: `@deck.gl/mapbox`
- Owns: the single `MapboxOverlay` `IControl` binding a `Deck` to a maplibre/mapbox `Map`, with automatic camera sync, `interleaved` context-sharing + z-slotting, forwarded picking, and the shipped structural `Map`/`IControl`/`CustomLayerInterface`/`LayerOverlayProps` contract
- Accept: one overlay per map added via `addControl`, the map as the sole camera authority, `interleaved` as a boolean mode, `setProps` as the atom-derived `layers` sink, forwarded `pickObjects`→`GlobalId`, the structural map contract in place of maplibre typings
- Reject: manual camera sync under the overlay, passing camera props in `MapboxOverlayProps`, a second overlay or free `Deck` on one map canvas, two overlay classes where one `interleaved` flag suffices, importing maplibre-gl types to satisfy the overlay's `Map` parameter, rebuilding the overlay instead of `setProps`
