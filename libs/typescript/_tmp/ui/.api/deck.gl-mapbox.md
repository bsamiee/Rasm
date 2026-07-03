# [API_CATALOGUE] @deck.gl/mapbox

`@deck.gl/mapbox` supplies `MapboxOverlay` — a MapLibre/Mapbox `IControl` wrapping a `Deck` instance that mounts deck.gl layers into a base map, either as an **overlaid** canvas above the map or **interleaved** inside the map's WebGL2 layer stack (`interleaved: true`). It synchronizes the deck.gl view state with the map camera automatically (no `viewState`/`controller` prop) and forwards picking through `pickObject`/`pickMultipleObjects`/`pickObjects`. This is the single integration surface for the `render/geo.md` `GeoSeriesLayer` `overlay` case over a `maplibre-gl` `Map`; `interleaved` mode is what lets deck.gl layers slot between map style layers via `beforeId`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@deck.gl/mapbox`
- package: `@deck.gl/mapbox`
- module: `@deck.gl/mapbox`
- asset: `dist/index.d.ts` (exports exactly `MapboxOverlay` + `MapboxOverlayProps`)
- baseline: deck.gl v9; interleave shares one WebGL2 context with the base map (`maplibre-gl` v3+ / Mapbox GL JS v2+)
- rail: viewport

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: overlay class and props
- rail: viewport

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY] | [RAIL]                                                  |
| :-----: | :------------------- | :------------ | :------------------------------------------------------ |
|  [01]   | `MapboxOverlay`      | class         | `IControl` wrapper around a `Deck` instance             |
|  [02]   | `MapboxOverlayProps` | type alias    | `Omit<DeckProps, excluded> & { interleaved?: boolean }` |

`MapboxOverlayProps` = `Omit<DeckProps, 'width' | 'height' | 'gl' | 'parent' | 'canvas' | '_customRender' | 'viewState' | 'initialViewState' | 'controller'> & { interleaved?: boolean }`. The overlay derives sizing, GL context, DOM host, and camera from the base map, so those keys are structurally removed; setting them is impossible at the type level. Everything else passes through: `layers`, `views`, `effects`, `parameters`, `layerFilter`, `pickingRadius`, `useDevicePixels`, `getTooltip`/`getCursor`, and the `onHover`/`onClick`/`onDrag*`/`onError`/`onLoad` callbacks.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: construction and IControl lifecycle
- rail: viewport

| [INDEX] | [SURFACE]                  | [ENTRY_FAMILY] | [RAIL]                                            |
| :-----: | :------------------------- | :------------- | :------------------------------------------------ |
|  [01]   | `new MapboxOverlay(props)` | constructor    | `MapboxOverlayProps` — creates the overlay control |
|  [02]   | `onAdd(map)`               | IControl       | called by `map.addControl`; returns `HTMLDivElement` |
|  [03]   | `onRemove()`               | IControl       | called by the map; detaches and cleans up         |
|  [04]   | `getDefaultPosition()`     | IControl       | returns the default `ControlPosition`             |
|  [05]   | `finalize()`               | lifecycle      | removes from the map and releases all GL resources |

[ENTRYPOINT_SCOPE]: prop update and picking forward
- rail: viewport

| [INDEX] | [SURFACE]                     | [ENTRY_FAMILY] | [RAIL]                                                      |
| :-----: | :---------------------------- | :------------- | :--------------------------------------------------------- |
|  [01]   | `setProps(props)`             | prop update    | partial `MapboxOverlayProps` merge (layers/effects/parameters/handlers) |
|  [02]   | `pickObject(params)`          | picking        | forwards `Deck.pickObject` (same params/return)            |
|  [03]   | `pickMultipleObjects(params)` | picking        | forwards `Deck.pickMultipleObjects`                        |
|  [04]   | `pickObjects(params)`         | picking        | forwards `Deck.pickObjects`                                |
|  [05]   | `getCanvas()`                 | DOM query      | base-map canvas when `interleaved`, else `Deck.getCanvas()` |

## [04]-[IMPLEMENTATION_LAW]

[MAPBOX_TOPOLOGY]:
- `MapboxOverlay` implements the MapLibre/Mapbox `IControl` contract (`onAdd`/`onRemove`/`getDefaultPosition`); add it via `map.addControl(overlay, position?)` and it constructs its `Deck` against the map's shared context.
- `interleaved: false` (default) renders deck.gl on a separate canvas layered above the map — z-order is above all map style layers.
- `interleaved: true` inserts deck.gl layers into the map's own WebGL2 render pipeline; a `Layer` with a `beforeId` prop appears *between* named map style layers. It requires a WebGL2 context shared between deck.gl and the base map.
- Camera sync is automatic: `MapboxOverlay` follows the base map's `move` event and drives the deck.gl view state to match longitude/latitude/zoom/bearing/pitch — which is why `viewState`/`initialViewState`/`controller` are omitted from the props.
- `setProps` filters an incoming partial against the omitted-key set internally before merging into the live `Deck`; `layers`, `effects`, `parameters`, `onHover`, and `onClick` are the props updated per frame.

[STACKING_LAW]:
- This is the `render/geo.md` `GeoSeriesLayer` `overlay`-arm mount point: the `overlayMode` discriminant selects `interleaved` (this overlay inside the maplibre WebGL2 stack) vs. `overlaid` (a canvas above it). `MapboxOverlay` wraps a `@deck.gl/core` `Deck` behind the `IControl` (`MapboxOverlayProps = Omit<DeckProps, …>`), so the wrapped `Deck` owns the render loop, the `View`/`Viewport` projection, and the picking buffer while the overlay owns only the maplibre binding. Layer arrays the `GeoSeriesLayer` `Data.TaggedEnum` `$match`/`Match.value(source)` fold produces over `@deck.gl/layers`/`@deck.gl/geo-layers`/`@geoarrow/deck.gl-geoarrow` feed into `setProps({ layers })` as a core `LayersList` — one overlay owns them all, never a parallel `Deck`.
- The base map is `maplibre-gl` `Map` (see `maplibre-gl.md`), held as an `Effect.acquireRelease` resource under the `platform` `BrowserPlatform`; `map.addControl(overlay)` wires this control in, and `overlay.finalize()` runs in the resource release. `beforeId` interleave slots reference the map's own `getLayer`/`moveLayer` style-layer ids.
- Picking is forwarded, not re-implemented: `pickObject`/`pickObjects` proxy the wrapped `Deck`, so overlay hit-testing shares the deck.gl picking buffer with the `@deck.gl/core` `PickingInfo` the layer callbacks receive.

[LOCAL_ADMISSION]:
- Add the overlay after the map fires `load` so the style is ready for interleaved insertion; for interleaved z-ordering, set each deck.gl `Layer`'s `beforeId` to the target map style-layer id.
- Call `overlay.finalize()` on unmount (the `acquireRelease` release) to prevent WebGL context leaks; drive layer/handler updates through `setProps`, never by reconstructing the overlay.
- Type against `MapboxOverlayProps`; do not attempt to pass `viewState`/`controller` — they are structurally omitted and the camera is the map's.

[RAIL_LAW]:
- Package: `@deck.gl/mapbox`
- Owns: the MapLibre/Mapbox `IControl` integration for deck.gl layers with automatic camera synchronization and forwarded picking
- Accept: `MapboxOverlay` as the integration surface whenever deck.gl renders over a `maplibre-gl` or Mapbox GL JS map; `interleaved: true` for between-layer insertion; `setProps` for all runtime layer/effect/handler updates
- Reject: a manual `Deck` alongside a base map without `MapboxOverlay`; direct WebGL context sharing outside the interleaved contract; a second overlay/`Deck` when one `setProps({ layers })` carries the full layer set
