# [TS_UI_API_DECK_GL_CORE]

`@deck.gl/core` is the imperative WebGL2/WebGPU render engine the `ui/viewer/geo` plane drives: one `Deck` instance owns a luma.gl `Device`, an rAF `AnimationLoop`, a `LayerManager` diffing a declarative `layers` array into GPU attribute buffers, a `ViewManager` projecting `View`s to `Viewport`s, an `EffectManager` compositing lighting/post-process passes, and a `DeckPicker` reading a color-encoded picking buffer. It is a resource, not a rail — `new Deck(props)` / `setProps(props)` / `finalize()` is the acquire/update/release triple the `viewer` brackets in an Effect `Scope`, and its own render loop stays outside the Effect/atom fold. `@deck.gl/react` is deliberately unadmitted, so the binding is imperative-through-a-ref (React 19 ref callback), never `<DeckGL>`; `layers` is derived from the `@effect-atom` state fold and pushed at the `setProps` sink, with `updateTriggers` as deck's GPU-attribute memoization plane orthogonal to react-compiler's tree memoization. Every layer's `get*` prop is one `Accessor<In,Out>` (uniform value or per-object function), every screen↔world mapping is a `Viewport`, and every pick is a `PickingInfo` — the three parameterized surfaces the whole viewer composes. `scope:viewer` project-local: admitted only by the `ui/viewer` Nx project and compile-time excluded from the `ui` core.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@deck.gl/core`
- package: `@deck.gl/core`
- license: `MIT`
- abi: browser WebGL2 (default) / WebGPU via luma.gl `Device`; needs a `HTMLCanvasElement` and `requestAnimationFrame`
- peer-substrate (transitive, deck-owned, never imported directly by `viewer`): `@luma.gl/core`+`@luma.gl/engine` (GPU device/model/animation-loop), `@math.gl/core`+`@math.gl/web-mercator` (matrix/projection), `mjolnir.js` (pointer/gesture events), `@loaders.gl/core` (async data loaders), `@probe.gl/stats` (`DeckMetrics`)
- catalog-verdict: KEEP — the geo-plane engine; `@deck.gl/layers`/`@deck.gl/geo-layers`/`@deck.gl/mapbox` all peer-depend on it
- runtime: `scope:viewer` project-local; imperative resource bracketed in an Effect `Scope`, not an Effect service
- modules: `Deck`, `Layer`, `CompositeLayer`, `LayerExtension`, `View`/`MapView`/`OrbitView`/`OrthographicView`/`FirstPersonView`/`_GlobeView`, `Viewport`/`WebMercatorViewport`, `Controller` family, `LightingEffect`/`PostProcessEffect`, `Widget`, `FlyToInterpolator`/`LinearInterpolator`, `COORDINATE_SYSTEM`/`OPERATION`/`UNIT`
- absent (not admitted): `@deck.gl/react` (imperative ref binding instead), `@deck.gl/aggregation-layers`, `@deck.gl/widgets` (only the `Widget` base ships in core); `@deck.gl/extensions` ships only its `LayerExtension` base here — the concrete extension roster is admitted centrally in its own catalog (`.api/deck.gl-extensions.md`)

## [02]-[RENDER_ENGINE]

[TYPE_SCOPE]: the `Deck` root — one instance owns the canvas, device, render loop, and picker; `setProps` is the single reconciliation entry.
- `layers`, `effects`, `views`, `viewState`/`initialViewState`, and every callback are a partial patch — `Deck` diffs against the prior props and redraws only what changed. `viewState` (controlled) vs `initialViewState` (deck-tracked) is the camera-ownership discriminant; under `@deck.gl/mapbox` the map owns the camera and both are stripped.
- Async pick pair `pickObjectAsync({x,y,radius?,layerIds?,unproject3D?}) => Promise<PickingInfo|null>` and `pickObjectsAsync({x,y,width?,height?,layerIds?,maxObjects?}) => Promise<PickingInfo[]>`; each row `[SIGNATURE]` carries only the return.

| [INDEX] | [SYMBOL]                   | [SIGNATURE]                              | [CONSUMER_BOUNDARY]                                          |
| :-----: | :------------------------- | :--------------------------------------- | :----------------------------------------------------------- |
|  [01]   | `Deck<ViewsT>`             | `new Deck(props: DeckProps<ViewsT>)`     | viewer render root; `ViewsT` types `viewState` per view      |
|  [02]   | `Deck.setProps`            | `(props: DeckProps<ViewsT>) => void`     | imperative sink; atom-derived `layers`/`viewState`/`effects` |
|  [03]   | `Deck.finalize`            | `() => void`                             | `Scope` release — disposes device, loop, buffers             |
|  [04]   | `Deck.pickObjectAsync`     | `(opts) => Promise<PickingInfo \| null>` | top object at a point → `mark/selection` `GlobalId`          |
|  [05]   | `Deck.pickObjectsAsync`    | `(opts) => Promise<PickingInfo[]>`       | marquee box → `GlobalId[]` selection set                     |
|  [06]   | `Deck.pickObject`          | sync single-point, `@deprecated`         | WebGL-only sync top-object; async pair replaces it           |
|  [07]   | `Deck.pickObjects`         | sync region, `@deprecated`               | WebGL-only sync marquee; async pair replaces it              |
|  [08]   | `Deck.pickMultipleObjects` | sync stacked, `@deprecated`              | WebGL-only stacked pick; async pair replaces it              |
|  [09]   | `Deck.redraw`              | `(reason?: string) => void`              | force a frame; `_animate` forces every-frame (TripsLayer)    |
|  [10]   | `Deck.needsRedraw`         | `({clearRedrawFlags}) => false\|string`  | query the redraw flag                                        |
|  [11]   | `Deck.getViewports`        | `(rect?) => Viewport[]`                  | screen↔world math + BCF/overlay anchor projection            |
|  [12]   | `Deck.getViews`            | `() => View[]`                           | active view list                                             |
|  [13]   | `Deck.getView`             | `(id) => View?`                          | one view by id                                               |
|  [14]   | `Deck.getCanvas`           | `() => HTMLCanvasElement \| null`        | ref wiring                                                   |
|  [15]   | `Deck.isInitialized`       | `boolean`                                | `onLoad` gate before public calls                            |
|  [16]   | `DeckProps<ViewsT>`        | props record (below)                     | full prop contract; `MapboxOverlayProps` minus camera        |
|  [17]   | `DeckMetrics`              | full 19-member payload, [17]             | `_onMetrics` payload → `probe/benchmark` perf sink           |

- [17]-[METRICS_PAYLOAD]: the shipped payload is exactly the typed member set — timing `fps`/`gpuTime`/`gpuTimePerFrame`/`cpuTime`/`cpuTimePerFrame`/`pickTime`/`setPropsTime`/`updateAttributesTime`, counters `framesRedrawn`/`pickCount`/`pickLayersCount`/`updateAttributesCount`/`layersCount`/`drawLayersCount`/`updateLayersCount`, memory `gpuMemory` split into `bufferMemory`/`textureMemory`/`renderbufferMemory`; no untyped members ride the callback.

[TYPE_SCOPE]: `DeckProps` axes — canvas/device, data-flow, interaction, and lifecycle callbacks; one callback family carries `PickingInfo`.
- interaction callbacks `onHover`/`onClick`/`onDragStart`/`onDrag`/`onDragEnd` all receive `(info: PickingInfo, event: MjolnirEvent)` — one pointer-event family, not five shapes. `getTooltip(info) => TooltipContent` and `getCursor(state) => string` are the declarative HUD hooks. `onViewStateChange(p: ViewStateChangeParameters) => ViewStateT|null|void` intercepts camera changes.

| [INDEX] | [PROP]                              | [SIGNATURE]                                      | [ROLE]                                          |
| :-----: | :---------------------------------- | :----------------------------------------------- | :---------------------------------------------- |
|  [01]   | `layers`                            | `LayersList` (nested/falsy-tolerant)             | layer tree diffed each `setProps`               |
|  [02]   | `effects`                           | `Effect[]`                                       | empty adds a default `LightingEffect`           |
|  [03]   | `widgets`                           | `Widget[]`                                       | HUD widgets mounted on the parent element       |
|  [04]   | `views`                             | `ViewsT` (`View \| View[]`, default `MapView`)   | viewport partition; multi-view = split/inset    |
|  [05]   | `viewState` / `initialViewState`    | `ViewStateMap<ViewsT>`                           | controlled vs deck-tracked camera               |
|  [06]   | `controller`                        | `View['props']['controller']`                    | `ControllerProps` for the default view          |
|  [07]   | `device` / `deviceProps`            | `Device` / `CreateDeviceProps`                   | share a luma device or spec `webgpu`/`webgl`    |
|  [08]   | `parameters`                        | luma `Parameters`                                | GPU state (blend, depth) set before each frame  |
|  [09]   | `layerFilter`                       | `(ctx: FilterContext) => boolean`                | per-view/per-pass layer gating                  |
|  [10]   | `useDevicePixels` / `pickingRadius` | `boolean\|number` / `number`                     | DPR control + pick tolerance                    |
|  [11]   | `pickAsync`                         | `'sync'\|'async'\|'auto'`                        | deck-managed pick policy (async where able)     |
|  [12]   | `onViewStateChange`                 | `(p) => ViewStateT\|null\|void`                  | camera-change interception (constrain/redirect) |
|  [13]   | `onHover`/`onClick`/`onDrag*`       | `(info, event) => void`                          | one `PickingInfo`-carrying pointer family       |
|  [14]   | `getTooltip` / `getCursor`          | `(info) => TooltipContent` / `(state) => string` | declarative HUD/cursor                          |
|  [15]   | `onLoad` / `onError`                | lifecycle callbacks                              | boot gate + error rail                          |
|  [16]   | `onResize`                          | `({width,height}) => void`                       | viewport-resize hook                            |
|  [17]   | `onBeforeRender` / `onAfterRender`  | frame callbacks                                  | per-frame hooks                                 |
|  [18]   | `onDeviceInitialized`               | `(device: Device) => void`                       | device-ready hook                               |
|  [19]   | `_animate`                          | `boolean`                                        | force every-frame redraw (trips)                |
|  [20]   | `_pickable`                         | `boolean`                                        | disable picking globally                        |
|  [21]   | `_framebuffer`                      | `Framebuffer`                                    | offscreen render target                         |
|  [22]   | `_typedArrayManagerProps`           | overlay                                          | attribute memory tuning                         |

## [03]-[LAYER_MODEL]

[TYPE_SCOPE]: the `Layer`/`CompositeLayer`/`LayerExtension` lattice — every layer is `_XxxProps<DataT> & (Layer|Composite)Props`, generic over the row type; one lifecycle, one accessor mechanism, one extension hook.
- primitive `Layer` renders GPU models; `CompositeLayer` renders other layers via `renderLayers()` and bubbles picking through `getSubLayers()`/`getSubLayerProps()`. `LayerExtension` injects shaders/state/props into any layer — the polymorphic capability-injection seam the viewer uses instead of subclassing.
- Layer lifecycle `initializeState`/`updateState`/`shouldUpdateState`/`finalizeState`/`draw`/`getPickingInfo`; `CompositeLayer` adds `renderLayers`/`getSubLayerProps`/`getSubLayerAccessor`/`getSubLayerClass`/`getSubLayerRow`, `LayerExtension` mirrors the lifecycle plus `getShaders`/`onNeedsRedraw`. Instance helpers `project`/`unproject`/`projectPosition`, `getBounds`/`getNumInstances`/`getModels` sit on `Layer`; custom `getShaders` include deck's shader modules `project`/`project32`/`picking`/`color`/`gouraudMaterial`/`phongMaterial`/`shadow` + `getShaderAssembler` so GPU code shares deck's projection + picking.

| [INDEX] | [SYMBOL]                                      | [SIGNATURE]                              | [CONSUMER_BOUNDARY]                          |
| :-----: | :-------------------------------------------- | :--------------------------------------- | :------------------------------------------- |
|  [01]   | `Layer<PropsT>`                               | abstract; lifecycle above                | base for `@deck.gl/layers` primitives        |
|  [02]   | `project` / `unproject` / `projectPosition`   | `(xyz) => number[]` (+coord opts)        | layer-space↔screen inside custom draw        |
|  [03]   | `getBounds` / `getNumInstances` / `getModels` | extent / instance count / GPU models     | fit-bounds + custom-render internals         |
|  [04]   | `CompositeLayer<PropsT>`                      | abstract; renders sub-layers             | `GeoJsonLayer`/`PolygonLayer`/`TileLayer`    |
|  [05]   | `LayerExtension<OptionsT>`                    | abstract; + `getShaders`/`onNeedsRedraw` | the `extensions` injection point             |
|  [06]   | `UpdateParameters<L>`                         | `{props,oldProps,context,changeFlags}`   | diff payload every `updateState` receives    |
|  [07]   | `_Component` / `_ComponentState`              | prop-diff lifecycle base                 | internal; underpins `Layer`/`View`           |
|  [08]   | shader modules                                | luma.gl `ShaderModule`s (above)          | shared in custom `getShaders`                |
|  [09]   | `Tesselator` / `createIterable`               | geometry helpers                         | `fp64LowPart` for custom extruded/path (BIM) |

[TYPE_SCOPE]: `LayerProps` shared axes — one base every layer inherits; the accessor + unit-normalization axes are shared mechanisms, not per-layer props.
- `Accessor<In,Out> = Out | ((object:In, ctx:AccessorContext<In>) => Out)` — one type owns every `get*` prop across every layer; the function form receives `{index,data,target}` where `target` is a reusable output buffer to skip GC. `updateTriggers` names which accessors to re-evaluate when a closed-over value changes (react-compiler memoizes the tree; `updateTriggers` memoizes GPU attributes — orthogonal planes). `*Units`/`*Scale`/`*MinPixels`/`*MaxPixels` form one radius/width normalization axis reused by scatterplot/path/polygon/arc/line/column, not a per-layer invention.
- `data` accepts `LayerDataSource<T>` = `LayerData<T> | string(URL) | Promise | AsyncIterable | null`, where `LayerData<T>` = `Iterable<T> | {length, attributes}`; the `{length, attributes}` form is the Arrow columnar fast path. Projection vocab: `coordinateSystem` (`CoordinateSystem` string), `coordinateOrigin`, `modelMatrix`, `wrapLongitude` (180° meridian), `positionFormat` (`XY`/`XYZ`), `colorFormat` (`RGBA`/`RGB`); the string unions `CoordinateSystem`/`Operation`/`Unit` are the public API, the `COORDINATE_SYSTEM`/`OPERATION` const maps `@deprecated` in favor of them, `UNIT` a numeric-code map.

| [INDEX] | [SYMBOL]                                    | [TYPE]                                    | [ROLE]                                      |
| :-----: | :------------------------------------------ | :---------------------------------------- | :------------------------------------------ |
|  [01]   | `Accessor<In,Out>`                          | `Out \| ((obj,ctx)=>Out)`                 | the `get*` parameterization; `target` reuse |
|  [02]   | `Position` / `Color`                        | `[n,n,n?]` / `[r,g,b,a?]` (+typed arrays) | accessor output vocab                       |
|  [03]   | `Unit` / `Operation` / `Material`           | string unions + lighting settings         | dimension unit, render op, PBR material     |
|  [04]   | `data`                                      | `LayerDataSource<T>` (above)              | async/URL/promise/binary-columnar source    |
|  [05]   | `dataComparator` / `dataTransform`          | hooks                                     | change detection + pre-render transform     |
|  [06]   | `_dataDiff` / `fetch`                       | hooks                                     | partial GPU update + custom loader          |
|  [07]   | `updateTriggers`                            | `Record<string,any>`                      | accessor re-eval discriminant               |
|  [08]   | `pickable` / `autoHighlight`                | `boolean\|'3d'` / `boolean`               | picking + GPU hover highlight               |
|  [09]   | `highlightedObjectIndex` / `highlightColor` | `number` / `Color`                        | forced highlight target + color             |
|  [10]   | `transitions`                               | `Record<string,any>`                      | per-prop GPU interpolation on change        |
|  [11]   | `extensions` / `loaders` / `loadOptions`    | `LayerExtension[]` / `Loader[]`           | capability injection + loaders.gl config    |
|  [12]   | `onDataLoad` / `onError`                    | data callbacks                            | load-complete + error rail                  |
|  [13]   | `onHover` / `onClick` / `onDrag*`           | `(info, event) => void`                   | per-layer pointer family                    |

## [04]-[VIEW_AND_CAMERA]

[TYPE_SCOPE]: `View`→`Viewport` projection + `Controller` interaction + interpolators — the camera the `viewer/geo/project` seam syncs.
- a `View` (declarative viewport spec + controller config) makes a `Viewport` (immutable coordinate-transform snapshot) from a `viewState`. `Viewport` is accessor-only: mutate by constructing a new one. `WebMercatorViewport` is the geospatial transform whose `project`/`unproject`/`fitBounds` the overlay-mark and camera-sync rows call. Free-standing `Deck` drives the camera from atom state through `FlyToInterpolator`/`LinearInterpolator`; under `@deck.gl/mapbox` the map owns it.
- `CommonViewProps` = `{id,x,y,width,height,padding,clear,controller,viewState}`; every `View` exposes `makeViewport({width,height,viewState})`, `clone`, `equals`. `MapViewState` = `{longitude,latitude,zoom,pitch,bearing,min/maxZoom,min/maxPitch,position,nearZ,farZ}`, `MapViewProps` adds `{repeat,orthographic,altitude,fovy,nearZMultiplier,farZMultiplier}`.
- `Viewport` methods `project(xyz,{topLeft?})`/`unproject(xyz,{topLeft?,targetZ?})`/`projectFlat`/`unprojectFlat`/`getBounds`/`getFrustumPlanes`/`containsPixel`/`getDistanceScales`; `WebMercatorViewport` adds `fitBounds([[lng,lat],[lng,lat]],{padding,maxZoom,minExtent,offset})`, `addMetersToLngLat`, `panByPosition`/`panByPosition3D`. `ControllerProps` (the exported controller config) folds the interaction toggles `{scrollZoom,dragPan,dragRotate,doubleClickZoom,touchZoom,touchRotate,keyboard,dragMode,inertia,maxBounds}` plus transition props; concrete controllers `MapController`/`OrbitController`/`OrthographicController`/`TerrainController`/`_GlobeController`/`FirstPersonController` extend `Controller`.
- `InteractionState` = `{isDragging,isPanning,isRotating,isZooming,inTransition}`; `ViewStateChangeParameters` = `{viewId,viewState,oldViewState,interactionState}`; camera transitions `FlyToInterpolator({curve,speed,maxDuration})` and `LinearInterpolator({transitionProps,around})` under `TransitionInterpolator`, driven from atom state.

| [INDEX] | [SYMBOL]                                        | [SIGNATURE]                        | [CONSUMER_BOUNDARY]                             |
| :-----: | :---------------------------------------------- | :--------------------------------- | :---------------------------------------------- |
|  [01]   | `View`                                          | `CommonViewProps` (above)          | abstract base; multi-view + inset panels        |
|  [02]   | `MapView` / `MapViewState` / `MapViewProps`     | fields above                       | default view; `WebMercatorViewport` producer    |
|  [03]   | `OrbitView` / `OrbitViewport`                   | orbit transform                    | CAD-style orbit                                 |
|  [04]   | `OrthographicView` / `OrthographicViewport`     | flat transform                     | 2D plan view                                    |
|  [05]   | `FirstPersonView` / `FirstPersonViewport`       | eye transform                      | walkthrough                                     |
|  [06]   | `_GlobeView` / `_GlobeViewport`                 | globe transform                    | 3D globe (`_` overlay)                          |
|  [07]   | `Viewport`                                      | methods above                      | immutable screen↔world; BCF/overlay anchor math |
|  [08]   | `WebMercatorViewport`                           | + `fitBounds` (above)              | geospatial camera fit + meter offsets           |
|  [09]   | `Controller`                                    | `ControllerProps` (above)          | interaction config per view                     |
|  [10]   | `InteractionState`                              | fields above                       | drag/pan/rotate/zoom flags                      |
|  [11]   | `ViewStateChangeParameters`                     | fields above                       | `onViewStateChange` payload → atom camera fold  |
|  [12]   | `FlyToInterpolator`                             | ctor above                         | animated geo camera transition                  |
|  [13]   | `LinearInterpolator` / `TransitionInterpolator` | ctor above                         | linear transition + base class                  |
|  [14]   | `TRANSITION_EVENTS`                             | `{BREAK:1,SNAP_TO_END:2,IGNORE:3}` | transition interruption policy                  |

## [05]-[PICKING_EFFECTS_WIDGETS]

[TYPE_SCOPE]: `PickingInfo` (the pick result), the `Effect` compositing pipeline, and the `Widget` HUD base.
- `PickingInfo<DataT,ExtraInfo>` is generic — `object: DataT` is the picked row and layers extend `ExtraInfo` (e.g. `MVTLayerPickingInfo` adds the tile). `Effect` is the pre/post-render pass interface: `LightingEffect` composites lights + shadows, `PostProcessEffect` wraps a shadertools `ShaderPass` for screen-space effects. `Widget` is the imperative HUD base (core ships the base only; `@deck.gl/widgets` is unadmitted).
- `PickingInfo` fields `ExtraInfo & {object?:DataT,index,picked,layer,sourceLayer?,coordinate?,pixel?,devicePixel?,x,y,viewport?,pixelRatio,color}`; `Effect` fields `{id,props,order?,useInPicking?,preRender,postRender?,getShaderModuleProps?,setup,setProps?,cleanup}`; `Widget<PropsT,ViewsT>` lifecycle `onAdd`/`onRemove`/`onRenderHTML`/`onViewportChange`/`onRedraw`/`onHover`/`onClick`/`onDrag`/`onDragStart`/`onDragEnd`, props `WidgetProps` = `{id,style,className,_container}` with `placement:WidgetPlacement`/`className` as abstract class members. `LightingEffect({ambient,dir1,point1,…})` builds the light rig from a `Record<string,Light>`.

| [INDEX] | [SYMBOL]                                    | [SIGNATURE]                            | [CONSUMER_BOUNDARY]                           |
| :-----: | :------------------------------------------ | :------------------------------------- | :-------------------------------------------- |
|  [01]   | `PickingInfo<DataT,ExtraInfo>`              | fields above                           | pick → `info.object` → `GlobalId`             |
|  [02]   | `GetPickingInfoParams<DataT,ExtraInfo>`     | `{info,mode,sourceLayer}`              | `Layer.getPickingInfo` input to enrich `info` |
|  [03]   | `Effect`                                    | fields above                           | the `effects` array pass contract             |
|  [04]   | `LightingEffect` / `LightingEffectProps`    | ctor above                             | shadows + PBR for extruded/3D layers          |
|  [05]   | `AmbientLight` / `DirectionalLight`         | light instances (+ `*Options`)         | ambient + directional rig                     |
|  [06]   | `PointLight` / `_SunLight` / `_CameraLight` | light instances                        | point, geo sun, headlight                     |
|  [07]   | `PostProcessEffect<ShaderPassT>`            | `new PostProcessEffect(module, props)` | screen-space passes (blur, outline)           |
|  [08]   | `Widget<PropsT,ViewsT>`                     | lifecycle above                        | imperative HUD overlays (base only)           |

## [06]-[IMPLEMENTATION_LAW]

[ENGINE_TOPOLOGY]:
- one instance, one loop: a `Deck` owns its luma.gl `Device`, `AnimationLoop`, and canvas; it renders outside the Effect/atom fold on its own rAF cadence. `viewer` holds exactly one `Deck` (or one `MapboxOverlay` wrapping one `Deck`) per map surface — a second engine on the same canvas is the named defect.
- declarative in, imperative sink: `layers`/`viewState`/`effects` are pure values derived from the `@effect-atom` state fold; `Deck.setProps` is the single imperative sink where the value tree meets the GPU. `Deck` diffs against prior props and touches only changed attributes — never rebuild it, always `setProps`.
- immutable viewports: `Viewport` is accessor-only; camera moves construct a new `WebMercatorViewport`. Coordinate math (`project`/`unproject`/`fitBounds`) is pure and side-effect-free — safe to call inside a derived atom for BCF/overlay anchor placement.
- accessor is the collapse: every `get*` across every layer is one `Accessor<In,Out>`; a "new styling rule" is a function accessor + an `updateTriggers` key, never a new prop or layer. Enumerated per-object variation lives in the accessor closure, not in parallel props.

[INTEGRATION_LAW]:
- Stack under `@effect-atom` + a `Scope`: the `viewer` acquires the `Deck`/`MapboxOverlay` in an Effect `acquireRelease` (`onAdd`→acquire, `finalize`→release), holds it in a ref, and subscribes an atom-derived `layers`/`viewState` fold that calls `setProps` on change. React-compiler compiles the surrounding React tree; deck's `updateTriggers` governs GPU attribute recompute — two orthogonal memoization planes, never conflated.
- Stack with `@deck.gl/mapbox`: `MapboxOverlay` re-uses `DeckProps` minus the camera-owning props (`viewState`/`initialViewState`/`controller`/`width`/`height`/`canvas`/`gl`) because the maplibre map owns the camera and syncs it into `Deck` automatically — the `viewer/geo/project` camera-sync seam. Free-standing `Deck` instead drives `viewState` from atom state with `FlyToInterpolator`/`LinearInterpolator`.
- Stack with `@deck.gl/layers` + `@deck.gl/geo-layers`: those packages are the `Layer`/`CompositeLayer` vocabulary; core owns the base classes, accessor types, picking, effects, and the `Deck` they render into. `data` accepts the `wire`-decoded value directly (`LayerDataSource` covers Arrow columnar `{length,attributes}`).
- Stack with `@geoarrow/deck.gl-geoarrow` + `apache-arrow` (`.api/apache-arrow.md`): `LayerData<T>` includes the binary `{length, attributes}` columnar form, so an Arrow `Table` decoded in `wire` (WKB→GeoArrow) fans to per-`RecordBatch` GeoArrow layers (catalog-bound grain: each layer's `data` is one `RecordBatch`, the caller iterates `Table.batches` and mounts one layer per batch) with zero per-row JS objects — the GPU-columnar fast path core's data contract already admits.
- Stack with picking→selection: `Deck.pickObjectsAsync` (marquee) / `onClick`→`PickingInfo.object` is the boundary where a rendered feature becomes a `mark/selection` `GlobalId`; the async pair is the WebGPU-safe form, the sync `pickObject*` are `@deprecated`.

[LOCAL_ADMISSION]:
- imported only inside the `ui/viewer` Nx project (`scope:viewer`); the `ui` core never resolves it — heavy GPU deps stay compile-time excluded from non-spatial apps.
- `Deck` is a bracketed resource, never an Effect service — its render loop is imperative by construction; wrapping the loop itself in Effect is a category error.
- `@deck.gl/react` is unadmitted on purpose: bind imperatively through a React 19 ref callback, never reach for `<DeckGL>`. Core ships only the `LayerExtension`/`Widget` bases; `@deck.gl/extensions` is admitted centrally as the concrete `LayerExtension` roster (`.api/deck.gl-extensions.md`), while `@deck.gl/aggregation-layers`/`@deck.gl/widgets` stay absent.
- peer substrate (`luma.gl`/`math.gl`/`mjolnir.js`/`loaders.gl`) is deck-owned and never imported directly; touch it only through deck props (`device`, `parameters`, `loaders`, `loadOptions`).

[RAIL_LAW]:
- Package: `@deck.gl/core`
- Owns: the `Deck` render engine + reconciliation, the `Layer`/`CompositeLayer`/`LayerExtension` lattice, the `Accessor`/`Position`/`Color`/`Unit` data vocab, `View`/`Viewport`/`WebMercatorViewport` projection + `Controller` interaction + camera interpolators, `PickingInfo` + async picking, the `Effect`/`LightingEffect`/`PostProcessEffect` pass pipeline, and the `Widget` HUD base
- Accept: one `Deck` per surface as a `Scope`-bracketed resource, `setProps` as the single imperative sink for atom-derived `layers`/`viewState`/`effects`, `Accessor` functions + `updateTriggers` as the styling-variation collapse, `WebMercatorViewport` math for anchor/camera work, `pickObjectsAsync`/`onClick` as the pick→`GlobalId` boundary, `LayerExtension` for cross-layer capability injection
- Reject: a second `Deck` on one canvas, rebuilding `Deck` instead of `setProps`, `<DeckGL>`/`@deck.gl/react`, wrapping the render loop in Effect, importing `luma.gl`/`math.gl` directly, the deprecated sync `pickObject*` under WebGPU, per-object styling as parallel props instead of one function accessor, mutating a `Viewport`
