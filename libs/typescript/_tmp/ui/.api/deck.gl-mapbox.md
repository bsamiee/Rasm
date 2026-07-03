# [API_CATALOGUE] @deck.gl/mapbox

`@deck.gl/mapbox` supplies `MapboxOverlay` — a MapLibre/Mapbox `IControl` that mounts deck.gl layers into a base map's rendering pipeline, either as an overlay on top of the map canvas or interleaved inside the map's WebGL2 layer stack (`interleaved: true`). The overlay synchronizes the deck.gl view state with the map's camera automatically and forwards picking via `pickObject`, `pickMultipleObjects`, and `pickObjects`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@deck.gl/mapbox`
- package: `@deck.gl/mapbox`
- module: `@deck.gl/mapbox`
- asset: `dist/index.d.ts`
- rail: viewport

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: overlay class and props
- rail: viewport

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY] | [RAIL]                                                  |
| :-----: | :------------------- | :------------ | :------------------------------------------------------ |
|  [01]   | `MapboxOverlay`      | class         | `IControl` wrapper around a `Deck` instance             |
|  [02]   | `MapboxOverlayProps` | type alias    | `Omit<DeckProps, excluded> & { interleaved?: boolean }` |

`MapboxOverlayProps` omits the following `DeckProps` keys: `width`, `height`, `gl`, `parent`, `canvas`, `_customRender`, `viewState`, `initialViewState`, `controller`.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: construction and lifecycle
- rail: viewport

| [INDEX] | [SURFACE]                  | [ENTRY_FAMILY] | [RAIL]                                            |
| :-----: | :------------------------- | :------------- | :------------------------------------------------ |
|  [01]   | `new MapboxOverlay(props)` | constructor    | `MapboxOverlayProps` — creates overlay control    |
|  [02]   | `onAdd(map)`               | IControl       | called by map; returns `HTMLDivElement` container |
|  [03]   | `onRemove()`               | IControl       | called by map; detaches and cleans up             |
|  [04]   | `getDefaultPosition()`     | IControl       | returns default `ControlPosition`                 |
|  [05]   | `finalize()`               | lifecycle      | removes from map and releases all resources       |

[ENTRYPOINT_SCOPE]: prop and layer update
- rail: viewport

| [INDEX] | [SURFACE]            | [ENTRY_FAMILY] | [RAIL]                                                |
| :-----: | :------------------- | :------------- | :---------------------------------------------------- |
|  [01]   | `setProps(props)`    | prop update    | partial `MapboxOverlayProps` update                   |
|  [02]   | `filterProps(props)` | prop filter    | returns cleaned `MapboxOverlayProps` for internal use |

[ENTRYPOINT_SCOPE]: picking forwarding
- rail: viewport

| [INDEX] | [SURFACE]                     | [ENTRY_FAMILY] | [RAIL]                                             |
| :-----: | :---------------------------- | :------------- | :------------------------------------------------- |
|  [01]   | `pickObject(params)`          | picking        | forwards `Deck.pickObject` with same params/return |
|  [02]   | `pickMultipleObjects(params)` | picking        | forwards `Deck.pickMultipleObjects`                |
|  [03]   | `pickObjects(params)`         | picking        | forwards `Deck.pickObjects`                        |

[ENTRYPOINT_SCOPE]: canvas access
- rail: viewport

| [INDEX] | [SURFACE]     | [ENTRY_FAMILY] | [RAIL]                                                      |
| :-----: | :------------ | :------------- | :---------------------------------------------------------- |
|  [01]   | `getCanvas()` | DOM query      | base map canvas when `interleaved`, else `Deck.getCanvas()` |

## [04]-[IMPLEMENTATION_LAW]

[MAPBOX_TOPOLOGY]:
- `MapboxOverlay` implements the MapLibre/Mapbox `IControl` interface; add it via `map.addControl(overlay, position?)`
- `interleaved: false` (default) renders deck.gl on a separate canvas layered over the map; z-order is above all map layers
- `interleaved: true` inserts deck.gl layers into the map's own WebGL2 render pipeline; layers may appear between map layers by specifying `beforeId` in the map style; requires WebGL2 context shared between deck.gl and the base map
- Camera synchronization is automatic: `MapboxOverlay` listens to the base map's `move` event and updates the deck.gl view state to match longitude, latitude, zoom, bearing, and pitch
- `setProps` passes `DeckProps`-compatible fields through; `layers`, `effects`, `parameters`, `onHover`, `onClick` are the primary runtime-updated props
- `MapboxOverlayProps` excludes `viewState`, `initialViewState`, and `controller` because the overlay derives them from the map; setting these is a no-op

[LOCAL_ADMISSION]:
- Use `map.addControl(overlay)` after the map fires its `load` event to ensure the style is ready for interleaved insertion.
- For interleaved mode, insert deck.gl layers at a specific map layer position by passing the map layer's `id` as `beforeId` in each deck.gl `Layer` props.
- Call `overlay.finalize()` on component unmount to prevent WebGL context leaks.

[RAIL_LAW]:
- Package: `@deck.gl/mapbox`
- Owns: MapLibre/Mapbox `IControl` integration for deck.gl layers with automatic camera synchronization
- Accept: `MapboxOverlay` as the integration surface whenever deck.gl renders over a `maplibre-gl` or Mapbox GL JS map
- Reject: manual `Deck` instantiation alongside a base map without `MapboxOverlay`; direct WebGL context sharing outside of the interleaved mode contract
