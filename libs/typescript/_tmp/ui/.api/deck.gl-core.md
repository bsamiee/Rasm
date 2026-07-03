# [API_CATALOGUE] @deck.gl/core

`@deck.gl/core` is the rendering engine every `deck.gl` layer package binds against: `Deck` (the canvas/WebGL2 orchestrator and event loop), the `Layer`/`CompositeLayer`/`LayerExtension` model, the parameterized View/Viewport/Controller family (Map, Orbit, Orthographic, FirstPerson, Globe), the `Effect` contract (lighting + post-process), transition interpolators, `Widget`, and the canonical `LayerProps`/accessor/`PickingInfo`/`LayersList` type set the layer packages consume. Layers themselves live in the sibling packages (`@deck.gl/layers`, `@deck.gl/geo-layers`, `@geoarrow/deck.gl-geoarrow`); core owns orchestration, projection, picking, and effects, and — via the sibling `@deck.gl/mapbox` `MapboxOverlay` — interleaves into the `maplibre-gl` WebGL2 stack (`render/geo.md#GEO_SERIES_LAYER`).

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@deck.gl/core`
- package / version: `@deck.gl/core` @ `9.3.5`
- license: `MIT`
- module: `type: module` ESM `dist/index.js` + CJS `dist/index.cjs`; typings `dist/index.d.ts`
- substrate deps: `@luma.gl/*` (WebGL2/WebGPU device, engine, shadertools), `@loaders.gl/*` (async data/image loaders), `@math.gl/*` (`core`/`web-mercator`/`sun`), `mjolnir.js` (pointer events), `@probe.gl/*` (log/stats)
- rail: viewport

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: core rendering classes
- rail: viewport

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]  | [NOTE]                                                        |
| :-----: | :----------------------- | :------------- | :----------------------------------------------------------- |
|  [01]   | `Deck<ViewsT>`           | class          | top-level canvas orchestrator, render loop, and event manager|
|  [02]   | `Layer<PropsT>`          | abstract class | base layer — extend for custom primitives                    |
|  [03]   | `CompositeLayer<PropsT>` | abstract class | composite layer — `renderLayers()` returns sub-`LayersList`  |
|  [04]   | `LayerExtension`         | class          | mixin feature: injects `getShaders`/`initializeState`/`updateState`/`draw`/`getPickingInfo` into a host layer |
|  [05]   | `LayerManager`           | class          | layer lifecycle owner (advanced)                             |
|  [06]   | `DeckRenderer`           | class          | low-level render orchestrator                                |
|  [07]   | `AttributeManager` / `Attribute` | classes | GPU attribute buffer manager / single attribute accessor     |
|  [08]   | `Tesselator`             | class          | geometry tessellation base for path/polygon-style custom layers |
|  [09]   | `Widget`                 | class          | base interactive map widget (`WidgetProps`, `WidgetPlacement`) |

[PUBLIC_TYPE_SCOPE]: view family — one parameterized matrix keyed by view kind
- rail: viewport

| [KIND]                | [VIEW]            | [VIEWPORT]             | [CONTROLLER]                        | [VIEW_STATE]            | [VIEW_PROPS]            |
| :-------------------- | :---------------- | :--------------------- | :---------------------------------- | :---------------------- | :---------------------- |
| map (web-mercator)    | `MapView`         | `WebMercatorViewport`  | `MapController` / `TerrainController`| `MapViewState`          | `MapViewProps`          |
| orbit (3D turntable)  | `OrbitView`       | `OrbitViewport`        | `OrbitController`                   | `OrbitViewState`        | `OrbitViewProps`        |
| orthographic (2D)     | `OrthographicView`| `OrthographicViewport` | `OrthographicController`            | `OrthographicViewState` | `OrthographicViewProps` |
| first-person          | `FirstPersonView` | `FirstPersonViewport`  | `FirstPersonController`             | `FirstPersonViewState`  | `FirstPersonViewProps`  |
| globe (non-mercator)  | `_GlobeView`      | `_GlobeViewport`       | `_GlobeController`                  | `GlobeViewState`        | `GlobeViewProps`        |
|  (base)               | `View`            | `Viewport`             | `Controller` (`ControllerProps`)    | —                       | —                       |

[PUBLIC_TYPE_SCOPE]: layer props, accessor, and lifecycle types
- rail: viewport

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY] | [NOTE]                                                             |
| :-----: | :-------------------------- | :------------ | :--------------------------------------------------------------- |
|  [01]   | `LayerProps` / `CompositeLayerProps` | type   | base props (`id`, `data`, `visible`, `opacity`, `pickable`, `coordinateSystem`, `coordinateOrigin`, `modelMatrix`, `extensions`, `loaders`, `loadOptions`, …) |
|  [02]   | `DefaultProps<PropsT>`      | type          | the `defaultProps` static shape a custom layer declares            |
|  [03]   | `LayersList`                | type          | flat/nested array of `Layer \| null \| false` — the `DeckProps.layers` and `renderLayers()` return shape |
|  [04]   | `LayerContext`              | type          | the shared layer runtime context (device, viewport, timeline, …)   |
|  [05]   | `ChangeFlags`               | type          | `updateState` diff flags (`dataChanged`, `propsChanged`, `viewportChanged`, …) |
|  [06]   | `Accessor<In, Out>`         | type          | `Out \| AccessorFunction<In, Out>`                                 |
|  [07]   | `AccessorFunction<In, Out>` | type          | `(object, objectInfo: AccessorContext<In>) => Out`                 |
|  [08]   | `AccessorContext<T>`        | type          | `{ index, data, target }` — write into `target` and return it to avoid GC |
|  [09]   | `LayerData<T>` / `LayerDataSource<T>` | type| resolved iterable/`{length, attributes?}` / the accepted source union: array, string URL, `AsyncIterable`, `Promise`, `null` |
|  [10]   | `Position` / `Color`        | tuple         | `[lng, lat, alt?]` / `[r, g, b, a?]` (deck's `Color`, distinct from the `colorjs.io` class) |
|  [11]   | `Unit` / `Operation` / `CoordinateSystem` | union | `'meters'\|'common'\|'pixels'` / `'draw'\|'mask'\|'terrain'` / the `COORDINATE_SYSTEM` value type |
|  [12]   | `Material` / `TextureSource` / `BinaryAttribute` | type | lighting material / `Texture\|HTMLImageElement\|ImageData\|…` / typed-array attribute descriptor |

[PUBLIC_TYPE_SCOPE]: picking, effect, controller, and constant types
- rail: viewport

| [INDEX] | [SYMBOL]                                         | [TYPE_FAMILY] | [NOTE]                                                             |
| :-----: | :----------------------------------------------- | :------------ | :--------------------------------------------------------------- |
|  [01]   | `PickingInfo` / `GetPickingInfoParams`           | type          | `{ layer, index, object, x, y, coordinate, picked, … }` / `getPickingInfo` override params |
|  [02]   | `UpdateParameters<L>`                            | type          | `{ props, oldProps, context, changeFlags }` — the `updateState` argument |
|  [03]   | `Effect` / `EffectContext` / `PreRenderOptions` / `PostRenderOptions` | interface | the render-effect contract `LightingEffect`/`PostProcessEffect` implement; extend for a custom screen-space effect |
|  [04]   | `LightingEffectProps` + light option types       | type          | `AmbientLightOptions` / `DirectionalLightOptions` / `PointLightOptions` / `SunLightOptions` |
|  [05]   | `ControllerProps` / `ViewStateChangeParameters` / `InteractionState` | type | controller config (`inertia`, `dragPan`, `scrollZoom`, …) / the `onViewStateChange` payload / drag/zoom flags |
|  [06]   | `WidgetProps` / `WidgetPlacement`                | type          | widget config / `'top-left'\|'top-right'\|…` placement            |
|  [07]   | `COORDINATE_SYSTEM` / `OPERATION` / `UNIT` / `TRANSITION_EVENTS` | const enum-like | coordinate-system, render-operation, unit, and transition-event discriminants |
|  [08]   | `DeckProps<ViewsT>` / `ViewStateMap<ViewsT>` / `VERSION` | type / const | full `Deck` constructor props / the per-view view-state map / the build version string |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `Deck` construction (`DeckProps`)
- rail: viewport

| [INDEX] | [SURFACE]                     | [ENTRY_FAMILY] | [NOTE]                                                             |
| :-----: | :---------------------------- | :------------- | :--------------------------------------------------------------- |
|  [01]   | `new Deck<ViewsT>(props)`      | constructor    | mounts the WebGL2 canvas + event loop from `DeckProps`            |
|  [02]   | `layers` / `effects` / `views`| props          | `LayersList` / `Effect[]` (lighting, post-process) / a `View` or array (defaults to one `MapView`) |
|  [03]   | `controller`                  | prop           | `boolean` \| `{ type: ControllerClass, ...ControllerProps }` \| a `Controller` instance |
|  [04]   | `viewState` / `initialViewState` | props       | controlled vs uncontrolled camera; `ViewStateMap<ViewsT>`         |
|  [05]   | `onViewStateChange`           | prop           | `(params: ViewStateChangeParameters) => ViewStateT \| null \| void` — the camera-sync seam |
|  [06]   | `onHover` / `onClick`         | props          | `(info: PickingInfo, event) => void` — pointer picking callbacks   |
|  [07]   | `getTooltip` / `getCursor`    | props          | `(info: PickingInfo) => TooltipContent` / `(state) => string`     |
|  [08]   | `parameters` / `useDevicePixels` / `pickingRadius` / `layerFilter` / `device` | props | GPU params / retina ratio (default `true`) / pick tolerance / `(FilterContext) => boolean` / a shared luma `Device` |
|  [09]   | `onLoad` / `onError` / `onResize` / `onBeforeRender` / `onAfterRender` | props | lifecycle hooks |

[ENTRYPOINT_SCOPE]: `Deck` instance operations
- rail: viewport

| [INDEX] | [SURFACE]                     | [ENTRY_FAMILY] | [NOTE]                                                             |
| :-----: | :---------------------------- | :------------- | :--------------------------------------------------------------- |
|  [01]   | `setProps(props)`             | prop update    | merges a partial `DeckProps`; re-renders when a `layers`/`viewState` reference changes |
|  [02]   | `finalize()`                  | lifecycle      | destroys the WebGL context and event listeners                    |
|  [03]   | `pickObject(params)` / `pickMultipleObjects(params)` / `pickObjects(params)` | picking | one object at a point / the stack at a point / all objects in a rectangle — all return `PickingInfo` |
|  [04]   | `getCanvas()` / `redraw(reason)` | query/render| `HTMLCanvasElement \| null` / force a redraw with a reason string  |

[ENTRYPOINT_SCOPE]: shader and utility surface
- rail: viewport

| [INDEX] | [SURFACE]                                         | [ENTRY_FAMILY] | [NOTE]                                                             |
| :-----: | :------------------------------------------------ | :------------- | :--------------------------------------------------------------- |
|  [01]   | `getShaderAssembler()`                            | shader         | the global shader assembler for registering shader modules         |
|  [02]   | `color` / `picking` / `project` / `project32`     | shader modules | built-in GLSL modules (color, picking, projection, fp64 projection)|
|  [03]   | `gouraudMaterial` / `phongMaterial` / `shadow`    | shader modules | lighting shader modules                                            |
|  [04]   | `createIterable(data, options?)` / `fp64LowPart(x)`| utility       | normalize `LayerData<T>` to an iterable / low-order fp64 bits      |
|  [05]   | `log` / `assert`                                  | utility        | the `@probe.gl` logger handle / assertion helper                   |
|  [06]   | `_`-prefixed advanced surface                     | experimental   | `_GlobeView`/`_GlobeViewport`/`_GlobeController`, `_SunLight`/`_CameraLight`, `_LayersPass`/`_PickLayersPass`, `_Component`/`_ComponentState`, and the `_fillArray`/`_flatten`/`_count`/`_deepEqual`/`_memoize`/`_compareProps` internals — stable-but-experimental, prefer the public surface |

## [04]-[IMPLEMENTATION_LAW]

[VIEWPORT_TOPOLOGY]:
- `DeckProps.layers` is a `LayersList` (flat or nested array of `Layer \| null \| false`); the deck re-renders when the reference changes, so build a fresh array each frame from an immutable fold rather than mutating in place
- `DeckProps.views` defaults to one `MapView`; the view family is parameterized — pick a `{ View, Viewport, Controller, ViewState }` row by kind, and `_GlobeView` for non-mercator globe rendering
- `DeckProps.controller` accepts `boolean`, `{ type: ControllerClass, ...ControllerProps }`, or a `Controller` instance; `ControllerProps` (`inertia`, `dragPan`, `scrollZoom`, `dragRotate`, …) tune interaction and `onViewStateChange` receives `ViewStateChangeParameters`
- base `LayerProps` fields (`data`, `visible`, `opacity`, `pickable`, `coordinateSystem`, `coordinateOrigin`, `modelMatrix`, `extensions`, `loaders`, `loadOptions`) apply to every layer; `COORDINATE_SYSTEM.LNGLAT` (default) maps `[lng, lat, alt]`, `CARTESIAN` for non-geographic data
- `AccessorFunction` receives `(object, { index, data, target })` — write the result into `target` and return it to avoid per-frame GC pressure
- `CompositeLayer.renderLayers()` returns a `LayersList`; sub-layer `id`s are namespaced automatically; a custom layer declares a `DefaultProps` static and reacts through `UpdateParameters`/`ChangeFlags` in `updateState`
- `LayerExtension` injects `getShaders`/`initializeState`/`updateState`/`draw`/`getPickingInfo` into a host layer; the `Effect` interface (`preRender`/`postRender` over `EffectContext`) is the screen-space-effect contract `LightingEffect`/`PostProcessEffect` implement
- `LayerDataSource` accepts an array, URL string, `Promise`, or `AsyncIterable`, resolved through the `@loaders.gl` `loaders`/`loadOptions` — out-of-core data never round-trips through an in-memory array

[STACKING]:
- layer composition (`render/geo.md#GEO_SERIES_LAYER`): core owns `Deck`/`View`/`Viewport`/`Controller`/picking/effects; the drawn primitives come from the sibling packages (`@deck.gl/layers` `GeoJsonLayer`, `@deck.gl/geo-layers` `TileLayer`/`MVTLayer`/`Tile3DLayer`, `@geoarrow/deck.gl-geoarrow` Arrow-native layers) — a closed `effect` `Data.TaggedEnum` `$match`/`Match.value` fold selects the layer set and returns a `LayersList`, keyed off one source/feature-kind discriminant
- maplibre interleave: the sibling `@deck.gl/mapbox` `MapboxOverlay` wraps a `Deck` as a maplibre `IControl` — `{ interleaved: true, layers }` shares the `maplibre-gl` WebGL2 context and syncs the camera automatically, so deck layers composite in the maplibre depth buffer rather than on a separate canvas
- resource discipline: hold the maplibre `Map` (and any `Deck` created directly) as an `effect` `Effect.acquireRelease` resource under the `platform` `BrowserPlatform`, `finalize()` on release — never a free React ref
- picking → projection: route `onHover`/`onClick` `PickingInfo` into the `projection` store through the `binding/atom.md#ATOM_BINDING` cell; a selection is a `PickingInfo.object`/`index` read, never a manual hit-test loop outside `pickObject*`
- async data: bind `LayerProps.data` to a `LayerDataSource` (`Promise`/`AsyncIterable`/URL) resolved by `@loaders.gl`, discharged through an `effect` rail so a load failure is a typed fault, not an uncaught rejection

[LOCAL_ADMISSION]:
- wire `Deck` through a framework integration (`@deck.gl/mapbox` `MapboxOverlay` when co-rendering with maplibre) rather than managing the canvas directly
- keep `useDevicePixels: true` (default) for retina; pass a number only to pin the pixel ratio
- author a custom primitive by extending `Layer`/`CompositeLayer` with a `DefaultProps` static and `updateState(UpdateParameters)` diffing; author a cross-layer feature as a `LayerExtension`, a screen-space pass as an `Effect`

[RAIL_LAW]:
- package: `@deck.gl/core`
- owns: WebGL2 layer orchestration, the parameterized view/viewport/controller family, picking, lighting/post-process effects, transitions, shader assembly, and the canonical `LayerProps`/`LayersList`/`PickingInfo` type set
- accept: `Deck` for all canvas management; `Layer`/`CompositeLayer`/`LayerExtension` for primitives; a `View` + `Controller` row for viewport control; `Effect[]` for lighting/post-process; `LayerDataSource` for async data
- reject: manual WebGL context management; a custom picking loop outside `Deck.pickObject*`; a mutated `layers` array in place of a fresh immutable fold; a free-ref maplibre `Map` outside an `acquireRelease` resource
