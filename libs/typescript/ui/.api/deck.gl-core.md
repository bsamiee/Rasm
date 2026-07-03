# [@deck.gl/core] — GPU layer engine the geo plane composes; scope:viewer project-local

`@deck.gl/core` is the imperative WebGL2/WebGPU render engine the `ui/viewer/geo` plane drives: one `Deck` instance owns a luma.gl `Device`, an rAF `AnimationLoop`, a `LayerManager` diffing a declarative `layers` array into GPU attribute buffers, a `ViewManager` projecting `View`s to `Viewport`s, an `EffectManager` compositing lighting/post-process passes, and a `DeckPicker` reading a color-encoded picking buffer. It is a resource, not a rail — `new Deck(props)` / `setProps(props)` / `finalize()` is the acquire/update/release triple the `viewer` brackets in an Effect `Scope`, and its own render loop stays outside the Effect/atom fold. `@deck.gl/react` is deliberately unadmitted, so the binding is imperative-through-a-ref (React 19 ref callback), never `<DeckGL>`; `layers` is derived from the `@effect-atom` state fold and pushed at the `setProps` sink, with `updateTriggers` as deck's GPU-attribute memoization plane orthogonal to react-compiler's tree memoization. Every layer's `get*` prop is one `Accessor<In,Out>` (uniform value or per-object function), every screen↔world mapping is a `Viewport`, and every pick is a `PickingInfo` — the three parameterized surfaces the whole viewer composes. `scope:viewer` project-local: admitted only by the `ui/viewer` Nx project and compile-time excluded from the `ui` core.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@deck.gl/core`
- package: `@deck.gl/core`
- version: `9.3.5`
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

| [INDEX] | [SYMBOL] | [SIGNATURE] | [CONSUMER / BOUNDARY] |
| :-----: | :------- | :---------- | :-------------------- |
| [01] | `Deck<ViewsT>` | `new Deck(props: DeckProps<ViewsT>)` | the viewer render root; `ViewsT` types `viewState` shape per view |
| [02] | `Deck.setProps` | `(props: DeckProps<ViewsT>) => void` | the imperative sink; `layers`/`viewState`/`effects` derived from atom state land here |
| [03] | `Deck.finalize` | `() => void` | `Scope` release — disposes device, loop, buffers |
| [04] | `Deck.pickObjectAsync` | `(opts: {x,y,radius?,layerIds?,unproject3D?}) => Promise<PickingInfo \| null>` | top object at a point → `mark/selection` `GlobalId` |
| [05] | `Deck.pickObjectsAsync` | `(opts: {x,y,width?,height?,layerIds?,maxObjects?}) => Promise<PickingInfo[]>` | marquee box → `GlobalId[]` selection set |
| [06] | `Deck.{pickObject,pickObjects,pickMultipleObjects}` | sync mirrors, `@deprecated` WebGL-only | legacy sync picking; prefer the async pair under WebGPU |
| [07] | `Deck.redraw` / `needsRedraw` | `(reason?: string) => void` / `({clearRedrawFlags}) => false\|string` | force/query a frame; `_animate` forces every-frame (TripsLayer) |
| [08] | `Deck.getViewports` / `getViews` / `getView` | `(rect?) => Viewport[]` / `() => View[]` / `(id) => View?` | screen↔world math + BCF/overlay anchor projection |
| [09] | `Deck.getCanvas` / `isInitialized` | `() => HTMLCanvasElement \| null` / `boolean` | ref wiring + `onLoad` gate before public calls |
| [10] | `DeckProps<ViewsT>` | props record (below) | the full prop contract `MapboxOverlayProps` re-uses minus camera props |
| [11] | `DeckMetrics` | `{fps,gpuTime,cpuTime,pickTime,gpuMemory,…}` | `_onMetrics` perf sink → `probe/benchmark` |

[TYPE_SCOPE]: `DeckProps` axes — canvas/device, data-flow, interaction, and lifecycle callbacks; one callback family carries `PickingInfo`.
- interaction callbacks `onHover`/`onClick`/`onDragStart`/`onDrag`/`onDragEnd` all receive `(info: PickingInfo, event: MjolnirEvent)` — one pointer-event family, not five shapes. `getTooltip(info) => TooltipContent` and `getCursor(state) => string` are the declarative HUD hooks.

| [INDEX] | [PROP] | [SIGNATURE] | [ROLE] |
| :-----: | :----- | :---------- | :----- |
| [01] | `layers` | `LayersList` (nested/falsy-tolerant) | the declarative layer tree diffed each `setProps` |
| [02] | `effects` | `Effect[]` | lighting/post-process pass list; empty adds a default `LightingEffect` |
| [03] | `views` | `ViewsT` (`View \| View[]`, default `new MapView()`) | viewport partition; multi-view = split/inset panels |
| [04] | `viewState` / `initialViewState` | `ViewStateMap<ViewsT>` | controlled vs deck-tracked camera |
| [05] | `controller` | `View['props']['controller']` | `ControllerOptions` shorthand for the default view |
| [06] | `device` / `deviceProps` | `Device` / `CreateDeviceProps` | share an existing luma device or spec `webgpu`/`webgl` |
| [07] | `parameters` | luma `Parameters` | GPU state (blend, depth) set before each frame |
| [08] | `layerFilter` | `(ctx: FilterContext) => boolean` | per-view/per-pass layer gating |
| [09] | `useDevicePixels` / `pickingRadius` | `boolean\|number` / `number` | DPR control + pick tolerance |
| [10] | `onViewStateChange` | `(p: ViewStateChangeParameters) => ViewStateT\|null\|void` | camera-change interception (constrain/redirect) |
| [11] | `onHover`/`onClick`/`onDrag*` | `(info, event) => void` | one `PickingInfo`-carrying pointer family |
| [12] | `getTooltip` / `getCursor` | `(info) => TooltipContent` / `(state) => string` | declarative HUD/cursor |
| [13] | `onLoad`/`onError`/`onResize`/`onBeforeRender`/`onAfterRender`/`onDeviceInitialized` | lifecycle callbacks | boot gate, error rail, frame hooks |
| [14] | `_animate` / `_pickable` / `_framebuffer` / `_typedArrayManagerProps` | experimental | force-animate (trips), disable picking, offscreen target, attribute memory |

## [03]-[LAYER_MODEL]

[TYPE_SCOPE]: the `Layer`/`CompositeLayer`/`LayerExtension` lattice — every layer is `_XxxProps<DataT> & (Layer|Composite)Props`, generic over the row type; one lifecycle, one accessor mechanism, one extension hook.
- primitive `Layer` renders GPU models; `CompositeLayer` renders other layers via `renderLayers()` and bubbles picking through `getSubLayers()`/`getSubLayerProps()`. `LayerExtension` injects shaders/state/props into any layer — the polymorphic capability-injection seam the viewer uses instead of subclassing.

| [INDEX] | [SYMBOL] | [SIGNATURE] | [CONSUMER / BOUNDARY] |
| :-----: | :------- | :---------- | :-------------------- |
| [01] | `Layer<PropsT>` | abstract; `initializeState`/`updateState`/`shouldUpdateState`/`finalizeState`/`draw`/`getPickingInfo` | base for `@deck.gl/layers` primitives; custom layers subclass |
| [02] | `Layer.project` / `unproject` / `projectPosition` | `(xyz:number[]) => number[]` (+ coord-system opts) | layer-space↔screen inside custom draw |
| [03] | `Layer.getBounds` / `getNumInstances` / `getModels` | data extent / instance count / GPU models | fit-bounds + custom-render internals |
| [04] | `CompositeLayer<PropsT>` | abstract; `renderLayers(): Layer\|LayersList`, `getSubLayerProps`, `getSubLayerAccessor`, `getSubLayerClass`, `getSubLayerRow` | `GeoJsonLayer`/`PolygonLayer`/`TileLayer` base; the cell family |
| [05] | `LayerExtension<OptionsT>` | abstract; `getShaders`/`getSubLayerProps`/`initializeState`/`updateState`/`draw`/`finalizeState`/`onNeedsRedraw` | the `extensions` prop injection point (core ships only the base; concrete pack in `.api/deck.gl-extensions.md`) |
| [06] | `UpdateParameters<L>` | `{props,oldProps,context,changeFlags}` | the diff payload every `updateState` receives |
| [07] | `_Component` / `_ComponentState` | prop-diff lifecycle base | internal; underpins `Layer` and `View` prop resolution |
| [08] | shader substrate: `project`/`project32`/`picking`/`color`/`gouraudMaterial`/`phongMaterial`/`shadow` + `getShaderAssembler` | luma.gl `ShaderModule`s | included in a custom `Layer.getShaders`/`LayerExtension.getShaders` so GPU code shares deck's projection + picking |
| [09] | tesselation/precision: `Tesselator` / `fp64LowPart` / `createIterable` | geometry helpers | base for a custom extruded/path domain layer (e.g. BIM element layer) needing fp64 precision |

[TYPE_SCOPE]: `LayerProps` shared axes — one base every layer inherits; the accessor + unit-normalization axes are shared mechanisms, not per-layer props.
- `Accessor<In,Out> = Out | ((object:In, ctx:AccessorContext<In>) => Out)` — one type owns every `get*` prop across every layer; the function form receives `{index,data,target}` where `target` is a reusable output buffer to skip GC. `updateTriggers` names which accessors to re-evaluate when a closed-over value changes (react-compiler memoizes the tree; `updateTriggers` memoizes GPU attributes — orthogonal planes). The `*Units`/`*Scale`/`*MinPixels`/`*MaxPixels` quartet is one radius/width normalization axis reused by scatterplot/path/polygon/arc/line/column, not a per-layer invention.

| [INDEX] | [SYMBOL] | [TYPE] | [ROLE] |
| :-----: | :------- | :----- | :----- |
| [01] | `Accessor<In,Out>` / `AccessorFunction<In,Out>` / `AccessorContext<T>` | `Out \| fn` / `(obj,ctx)=>Out` / `{index,data,target}` | THE `get*` parameterization; `target` = GC-free reuse |
| [02] | `Position` / `Color` | `[n,n,n?] \| Float32/64Array` / `[r,g,b,a?] \| Uint8(Clamped)Array` | accessor output vocab |
| [03] | `Unit` / `Operation` / `Material` | `'meters'\|'common'\|'pixels'` / `'draw'\|'mask'\|'terrain'` (+`+`-join) / lighting settings | dimension unit, render op, PBR material |
| [04] | `LayerDataSource<T>` / `LayerData<T>` | `Iterable\|{length,attributes}\|string(URL)\|Promise\|AsyncIterable\|null` | `data` accepts URL/promise/async-batch/binary-columnar |
| [05] | `data` / `dataComparator` / `dataTransform` / `_dataDiff` / `fetch` | value + hooks | async data plumbing; `_dataDiff` = partial GPU update |
| [06] | `updateTriggers` | `Record<string,any>` | accessor re-eval discriminant |
| [07] | `pickable` / `autoHighlight` / `highlightedObjectIndex` / `highlightColor` | `boolean\|'3d'` / GPU highlight | picking + hover highlight (GPU, no re-render) |
| [08] | `coordinateSystem` / `coordinateOrigin` / `modelMatrix` / `wrapLongitude` / `positionFormat` | projection controls | `COORDINATE_SYSTEM.*`, 180°-meridian, `XY`/`XYZ` |
| [09] | `transitions` | `Record<string,any>` | per-prop GPU interpolation on value change |
| [10] | `extensions` / `loaders` / `loadOptions` | `LayerExtension[]` / `Loader[]` | capability injection + loaders.gl parse config |
| [11] | `onDataLoad` / `onError` / `onHover` / `onClick` / `onDrag*` | callbacks | per-layer data + pointer family (mirrors `Deck` level) |
| [12] | `CoordinateSystem` / `COORDINATE_SYSTEM` / `OPERATION` / `UNIT` | `'lnglat'\|'cartesian'\|'meter-offsets'\|…` + const maps | string unions are the public API; const maps `@deprecated` |

## [04]-[VIEW_AND_CAMERA]

[TYPE_SCOPE]: `View`→`Viewport` projection + `Controller` interaction + interpolators — the camera the `viewer/geo/project` seam syncs.
- a `View` (declarative viewport spec + controller config) makes a `Viewport` (immutable coordinate-transform snapshot) from a `viewState`. `Viewport` is accessor-only: mutate by constructing a new one. `WebMercatorViewport` is the geospatial transform whose `project`/`unproject`/`fitBounds` the overlay-mark and camera-sync rows call. Free-standing `Deck` drives the camera from atom state through `FlyToInterpolator`/`LinearInterpolator`; under `@deck.gl/mapbox` the map owns it.

| [INDEX] | [SYMBOL] | [SIGNATURE] | [CONSUMER / BOUNDARY] |
| :-----: | :------- | :---------- | :-------------------- |
| [01] | `View` | abstract; `makeViewport({width,height,viewState})`, `clone`, `equals`; `CommonViewProps` = `{id,x,y,width,height,padding,clear,controller,viewState}` | multi-view partition + inset panels |
| [02] | `MapView` / `MapViewState` / `MapViewProps` | geospatial view; state `{longitude,latitude,zoom,pitch,bearing,min/maxZoom,min/maxPitch,position,nearZ,farZ}`; props `{repeat,orthographic,altitude,fovy,nearZMultiplier,farZMultiplier}` | the default view + `WebMercatorViewport` producer |
| [03] | `OrbitView` / `OrthographicView` / `FirstPersonView` / `_GlobeView` | non-geo + globe views | CAD-style orbit, 2D plan, walkthrough, 3D globe (`_` experimental) |
| [04] | `Viewport` | `project(xyz,{topLeft?})`/`unproject(xyz,{topLeft?,targetZ?})`/`projectFlat`/`unprojectFlat`/`getBounds`/`getFrustumPlanes`/`containsPixel`/`getDistanceScales` | immutable screen↔world; BCF/overlay anchor math |
| [05] | `WebMercatorViewport` | + `fitBounds([[lng,lat],[lng,lat]],{padding,maxZoom,minExtent,offset})`, `addMetersToLngLat`, `panByPosition3D` | geospatial camera fit + meter offsets |
| [06] | `OrbitViewport` / `OrthographicViewport` / `FirstPersonViewport` / `_GlobeViewport` | per-view transforms | matching non-geo viewports |
| [07] | `Controller` / `MapController` / `OrbitController` / `TerrainController` / `_GlobeController` / `FirstPersonController` | `ControllerOptions` = `{scrollZoom,dragPan,dragRotate,doubleClickZoom,touchZoom,touchRotate,keyboard,dragMode,inertia,maxBounds}` | interaction config per view |
| [08] | `InteractionState` / `ViewStateChangeParameters` | `{isDragging,isPanning,isRotating,isZooming,inTransition}` / `{viewId,viewState,oldViewState,interactionState}` | `onViewStateChange` payload → atom camera fold |
| [09] | `FlyToInterpolator` / `LinearInterpolator` / `TransitionInterpolator` | `new FlyToInterpolator({curve,speed,maxDuration})` / `new LinearInterpolator({transitionProps,around})` | camera transitions driven from atom state (`transitionInterpolator` on view state) |
| [10] | `TRANSITION_EVENTS` | `{BREAK:1,SNAP_TO_END:2,IGNORE:3}` | in-flight transition interruption policy |

## [05]-[PICKING_EFFECTS_WIDGETS]

[TYPE_SCOPE]: `PickingInfo` (the pick result), the `Effect` compositing pipeline, and the `Widget` HUD base.
- `PickingInfo<DataT,ExtraInfo>` is generic — `object: DataT` is the picked row and layers extend `ExtraInfo` (e.g. `MVTLayerPickingInfo` adds the tile). `Effect` is the pre/post-render pass interface: `LightingEffect` composites lights + shadows, `PostProcessEffect` wraps a shadertools `ShaderPass` for screen-space effects. `Widget` is the imperative HUD base (core ships the base only; `@deck.gl/widgets` is unadmitted).

| [INDEX] | [SYMBOL] | [SIGNATURE] | [CONSUMER / BOUNDARY] |
| :-----: | :------- | :---------- | :-------------------- |
| [01] | `PickingInfo<DataT,ExtraInfo>` | `{object?:DataT,index,picked,layer,sourceLayer,coordinate?,pixel?,x,y,viewport?,pixelRatio,color} & ExtraInfo` | pick → `info.object` → `GlobalId`; `coordinate` = lng/lat under geo |
| [02] | `GetPickingInfoParams<DataT,ExtraInfo>` | `{info,mode,sourceLayer}` | the `Layer.getPickingInfo` input to enrich `info` |
| [03] | `Effect` | `{id,props,order?,useInPicking?,preRender,postRender?,getShaderModuleProps?,setup,setProps?,cleanup}` | the `effects` array pass contract |
| [04] | `LightingEffect` / `LightingEffectProps` | `new LightingEffect({ambient,dir1,point1,…})`; `Record<string,AmbientLight\|DirectionalLight\|PointLight>` | shadows + PBR shading for extruded/3D layers |
| [05] | `AmbientLight` / `DirectionalLight` / `PointLight` / `_CameraLight` / `_SunLight` | light instances (+ `*Options`) | light rig; `_SunLight` = geo sun position, `_CameraLight` = headlight |
| [06] | `PostProcessEffect<ShaderPassT>` | `new PostProcessEffect(module: ShaderPass, props)` | screen-space passes (blur, outline) over the framebuffer |
| [07] | `Widget<PropsT,ViewsT>` / `WidgetProps` / `WidgetPlacement` | abstract; `onAdd`/`onRemove`/`onRenderHTML`/`onViewportChange`/`onRedraw`/`onHover`/`onClick`; `{id,style,className,_container,placement}` | imperative HUD overlays (base only in core) |

## [06]-[IMPLEMENTATION_LAW]

[ENGINE_TOPOLOGY]:
- one instance, one loop: a `Deck` owns its luma.gl `Device`, `AnimationLoop`, and canvas; it renders outside the Effect/atom fold on its own rAF cadence. The viewer holds exactly one `Deck` (or one `MapboxOverlay` wrapping one `Deck`) per map surface — a second engine on the same canvas is the named defect.
- declarative in, imperative sink: `layers`/`viewState`/`effects` are pure values derived from the `@effect-atom` state fold; `Deck.setProps` is the single imperative sink where the value tree meets the GPU. The engine diffs against prior props and touches only changed attributes — never rebuild the `Deck`, always `setProps`.
- immutable viewports: `Viewport` is accessor-only; camera moves construct a new `WebMercatorViewport`. Coordinate math (`project`/`unproject`/`fitBounds`) is pure and side-effect-free — safe to call inside a derived atom for BCF/overlay anchor placement.
- accessor is the collapse: every `get*` across every layer is one `Accessor<In,Out>`; a "new styling rule" is a function accessor + an `updateTriggers` key, never a new prop or layer. Enumerated per-object variation lives in the accessor closure, not in parallel props.

[INTEGRATION_LAW]:
- Stack under `@effect-atom` + a `Scope`: the `viewer` acquires the `Deck`/`MapboxOverlay` in an Effect `acquireRelease` (`onAdd`→acquire, `finalize`→release), holds it in a ref, and subscribes an atom-derived `layers`/`viewState` fold that calls `setProps` on change. React-compiler compiles the surrounding React tree; deck's `updateTriggers` governs GPU attribute recompute — two orthogonal memoization planes, never conflated.
- Stack with `@deck.gl/mapbox`: `MapboxOverlay` re-uses `DeckProps` minus the camera-owning props (`viewState`/`initialViewState`/`controller`/`width`/`height`/`canvas`/`gl`) because the maplibre map owns the camera and syncs it into `Deck` automatically — the `viewer/geo/project` camera-sync seam. Free-standing `Deck` instead drives `viewState` from atom state with `FlyToInterpolator`/`LinearInterpolator`.
- Stack with `@deck.gl/layers` + `@deck.gl/geo-layers`: those packages are the `Layer`/`CompositeLayer` vocabulary; core owns the base classes, accessor types, picking, effects, and the `Deck` they render into. `data` accepts the `wire`-decoded value directly (`LayerDataSource` covers Arrow columnar `{length,attributes}`).
- Stack with `@geoarrow/deck.gl-geoarrow` + `apache-arrow` (`.api/apache-arrow.md`): `LayerData<T>` includes the binary `{length, attributes}` columnar form, so an Arrow `Table` decoded in `wire` (WKB→GeoArrow) fans to per-`RecordBatch` GeoArrow layers (v0.4 grain: each layer's `data` is one `RecordBatch`, the caller iterates `Table.batches` and mounts one layer per batch) with zero per-row JS objects — the GPU-columnar fast path core's data contract already admits.
- Stack with picking→selection: `Deck.pickObjectsAsync` (marquee) / `onClick`→`PickingInfo.object` is the boundary where a rendered feature becomes a `mark/selection` `GlobalId`; the async pair is the WebGPU-safe form, the sync `pickObject*` are `@deprecated`.

[LOCAL_ADMISSION]:
- imported only inside the `ui/viewer` Nx project (`scope:viewer`); the `ui` core never resolves it — heavy GPU deps stay compile-time excluded from non-spatial apps.
- the `Deck` is a bracketed resource, never an Effect service — its render loop is imperative by construction; wrapping the loop itself in Effect is a category error.
- `@deck.gl/react` is unadmitted on purpose: bind imperatively through a React 19 ref callback, never reach for `<DeckGL>`. Core ships only the `LayerExtension`/`Widget` bases; `@deck.gl/extensions` is admitted centrally as the concrete `LayerExtension` roster (`.api/deck.gl-extensions.md`), while `@deck.gl/aggregation-layers`/`@deck.gl/widgets` stay absent.
- peer substrate (`luma.gl`/`math.gl`/`mjolnir.js`/`loaders.gl`) is deck-owned and never imported directly; touch it only through deck props (`device`, `parameters`, `loaders`, `loadOptions`).

[RAIL_LAW]:
- Package: `@deck.gl/core`
- Owns: the `Deck` render engine + reconciliation, the `Layer`/`CompositeLayer`/`LayerExtension` lattice, the `Accessor`/`Position`/`Color`/`Unit` data vocab, `View`/`Viewport`/`WebMercatorViewport` projection + `Controller` interaction + camera interpolators, `PickingInfo` + async picking, the `Effect`/`LightingEffect`/`PostProcessEffect` pass pipeline, and the `Widget` HUD base
- Accept: one `Deck` per surface as a `Scope`-bracketed resource, `setProps` as the single imperative sink for atom-derived `layers`/`viewState`/`effects`, `Accessor` functions + `updateTriggers` as the styling-variation collapse, `WebMercatorViewport` math for anchor/camera work, `pickObjectsAsync`/`onClick` as the pick→`GlobalId` boundary, `LayerExtension` for cross-layer capability injection
- Reject: a second `Deck` on one canvas, rebuilding `Deck` instead of `setProps`, `<DeckGL>`/`@deck.gl/react`, wrapping the render loop in Effect, importing `luma.gl`/`math.gl` directly, the deprecated sync `pickObject*` under WebGPU, per-object styling as parallel props instead of one function accessor, mutating a `Viewport`
