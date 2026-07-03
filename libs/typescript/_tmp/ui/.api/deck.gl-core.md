# [API_CATALOGUE] @deck.gl/core

`@deck.gl/core` supplies the rendering engine for `deck.gl`: `Deck` (the canvas/WebGL orchestrator), `Layer`/`CompositeLayer`/`LayerExtension` (the layer model), `View`/`Viewport` family, `Controller` family, lighting effects, transition interpolators, `Widget`, and the canonical `LayerProps`/accessor/picking type set consumed by all layer packages.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@deck.gl/core`
- package: `@deck.gl/core`
- module: `@deck.gl/core`
- asset: `dist/index.d.ts`
- rail: viewport

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: core rendering classes
- rail: viewport

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]  | [RAIL]                                          |
| :-----: | :----------------------- | :------------- | :---------------------------------------------- |
|  [01]   | `Deck`                   | class          | top-level canvas orchestrator and event manager |
|  [02]   | `Layer<PropsT>`          | abstract class | base layer (extend for custom layers)           |
|  [03]   | `CompositeLayer<PropsT>` | abstract class | composite layer (wraps sub-layers)              |
|  [04]   | `LayerExtension`         | class          | mixin-style layer feature extension             |
|  [05]   | `LayerManager`           | class          | manages layer lifecycle (advanced use)          |
|  [06]   | `DeckRenderer`           | class          | low-level render orchestrator                   |
|  [07]   | `AttributeManager`       | class          | manages GPU attribute buffers                   |
|  [08]   | `Attribute`              | class          | single GPU attribute accessor                   |

[PUBLIC_TYPE_SCOPE]: view and viewport family
- rail: viewport

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY] | [RAIL]                                    |
| :-----: | :--------------------- | :------------ | :---------------------------------------- |
|  [01]   | `View`                 | class         | base view descriptor                      |
|  [02]   | `MapView`              | class         | web-mercator map view                     |
|  [03]   | `OrbitView`            | class         | orbit (3D turntable) view                 |
|  [04]   | `OrthographicView`     | class         | orthographic (2D) view                    |
|  [05]   | `FirstPersonView`      | class         | first-person perspective view             |
|  [06]   | `Viewport`             | class         | base viewport (projection + unprojection) |
|  [07]   | `WebMercatorViewport`  | class         | web-mercator projection viewport          |
|  [08]   | `OrbitViewport`        | class         | orbit projection viewport                 |
|  [09]   | `OrthographicViewport` | class         | orthographic projection viewport          |
|  [10]   | `FirstPersonViewport`  | class         | first-person projection viewport          |

[PUBLIC_TYPE_SCOPE]: controller family
- rail: viewport

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY] | [RAIL]                       |
| :-----: | :----------------------- | :------------ | :--------------------------- |
|  [01]   | `Controller`             | class         | base input event controller  |
|  [02]   | `MapController`          | class         | pan/zoom/rotate for map view |
|  [03]   | `OrbitController`        | class         | orbit view controller        |
|  [04]   | `OrthographicController` | class         | orthographic view controller |
|  [05]   | `FirstPersonController`  | class         | first-person controller      |
|  [06]   | `TerrainController`      | class         | terrain-aware map controller |

[PUBLIC_TYPE_SCOPE]: layer props and accessor types
- rail: viewport

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY] | [RAIL]                                                 |
| :-----: | :-------------------------- | :------------ | :----------------------------------------------------- |
|  [01]   | `LayerProps`                | type alias    | base layer props (id, data, opacity, visible, …)       |
|  [02]   | `CompositeLayerProps`       | type alias    | `LayerProps` for composite layers                      |
|  [03]   | `Accessor<In, Out>`         | type alias    | `Out \| AccessorFunction<In, Out>`                     |
|  [04]   | `AccessorFunction<In, Out>` | type alias    | `(object, objectInfo) => Out`                          |
|  [05]   | `AccessorContext<T>`        | type alias    | `{ index, data, target }`                              |
|  [06]   | `LayerData<T>`              | type alias    | iterable or `{ length, attributes? }`                  |
|  [07]   | `LayerDataSource<T>`        | type alias    | data \| string URL \| AsyncIterable \| Promise \| null |
|  [08]   | `Position`                  | type alias    | `[lng, lat, alt?]` numeric tuple                       |
|  [09]   | `Color`                     | type alias    | `[r, g, b, a?]` numeric tuple                          |
|  [10]   | `Unit`                      | type alias    | `'meters' \| 'common' \| 'pixels'`                     |
|  [11]   | `Operation`                 | type alias    | `'draw' \| 'mask' \| 'terrain'`                        |
|  [12]   | `Material`                  | type alias    | lighting material settings                             |
|  [13]   | `TextureSource`             | type alias    | `Texture \| HTMLImageElement \| ImageData \| …`        |

[PUBLIC_TYPE_SCOPE]: picking types
- rail: viewport

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY] | [RAIL]                                          |
| :-----: | :--------------------- | :------------ | :---------------------------------------------- |
|  [01]   | `PickingInfo`          | type alias    | `{ layer, index, object, x, y, coordinate, … }` |
|  [02]   | `GetPickingInfoParams` | type alias    | parameters for `getPickingInfo` override        |
|  [03]   | `UpdateParameters<L>`  | type alias    | `{ props, oldProps, context, changeFlags }`     |

[PUBLIC_TYPE_SCOPE]: effects and transitions
- rail: viewport

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY] | [RAIL]                       |
| :-----: | :----------------------- | :------------ | :--------------------------- |
|  [01]   | `LightingEffect`         | class         | scene lighting effect        |
|  [02]   | `AmbientLight`           | class         | ambient light source         |
|  [03]   | `DirectionalLight`       | class         | directional light source     |
|  [04]   | `PointLight`             | class         | point light source           |
|  [05]   | `PostProcessEffect`      | class         | post-process shader effect   |
|  [06]   | `LinearInterpolator`     | class         | linear view-state transition |
|  [07]   | `FlyToInterpolator`      | class         | fly-to camera transition     |
|  [08]   | `TransitionInterpolator` | class         | base transition interpolator |
|  [09]   | `Widget`                 | class         | base interactive map widget  |

[PUBLIC_TYPE_SCOPE]: constants and view-state types
- rail: viewport

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]   | [RAIL]                                       |
| :-----: | :---------------------- | :-------------- | :------------------------------------------- |
|  [01]   | `COORDINATE_SYSTEM`     | const enum-like | coordinate system discriminant               |
|  [02]   | `OPERATION`             | const enum-like | layer render operation                       |
|  [03]   | `UNIT`                  | const enum-like | dimension unit constants                     |
|  [04]   | `TRANSITION_EVENTS`     | const           | view-state transition event tokens           |
|  [05]   | `MapViewState`          | type alias      | `{ longitude, latitude, zoom, … }`           |
|  [06]   | `OrbitViewState`        | type alias      | `{ target, rotationX, rotationOrbit, zoom }` |
|  [07]   | `OrthographicViewState` | type alias      | `{ target, zoom, … }`                        |
|  [08]   | `DeckProps<ViewsT>`     | type alias      | full `Deck` constructor props                |
|  [09]   | `CoordinateSystem`      | type alias      | `COORDINATE_SYSTEM` values                   |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: Deck construction
- rail: viewport

| [INDEX] | [SURFACE]         | [ENTRY_FAMILY] | [RAIL]                                         |
| :-----: | :---------------- | :------------- | :--------------------------------------------- |
|  [01]   | `new Deck(props)` | constructor    | `DeckProps` — mounts WebGL canvas + event loop |

[ENTRYPOINT_SCOPE]: Deck instance operations
- rail: viewport

| [INDEX] | [SURFACE]                     | [ENTRY_FAMILY] | [RAIL]                                     |
| :-----: | :---------------------------- | :------------- | :----------------------------------------- |
|  [01]   | `setProps(props)`             | prop update    | merges partial `DeckProps` update          |
|  [02]   | `finalize()`                  | lifecycle      | destroys WebGL context and event listeners |
|  [03]   | `pickObject(params)`          | picking        | single-object pick at point                |
|  [04]   | `pickMultipleObjects(params)` | picking        | picks all objects at point                 |
|  [05]   | `pickObjects(params)`         | picking        | picks all objects in rectangle             |
|  [06]   | `getCanvas()`                 | DOM query      | `HTMLCanvasElement \| null`                |
|  [07]   | `redraw(reason)`              | render         | forces a redraw with reason string         |

[ENTRYPOINT_SCOPE]: coordinate and shader utilities
- rail: viewport

| [INDEX] | [SURFACE]                                    | [ENTRY_FAMILY] | [RAIL]                                      |
| :-----: | :------------------------------------------- | :------------- | :------------------------------------------ |
|  [01]   | `getShaderAssembler()`                       | shader         | global shader assembler instance            |
|  [02]   | `color`, `project`, `project32`              | shader modules | built-in GLSL shader modules                |
|  [03]   | `gouraudMaterial`, `phongMaterial`, `shadow` | shader modules | lighting shader modules                     |
|  [04]   | `createIterable(data, options?)`             | utility        | normalizes `LayerData<T>` to iterable       |
|  [05]   | `fp64LowPart(x)`                             | utility        | low-order bits of double for fp64 emulation |

## [04]-[IMPLEMENTATION_LAW]

[VIEWPORT_TOPOLOGY]:
- `DeckProps.layers` accepts `LayersList` (flat or nested array of `Layer` instances or `null/false`); the deck re-renders when the reference changes
- `DeckProps.views` accepts a single `View` or an array; defaults to a single `MapView`
- `DeckProps.controller` may be `boolean`, `{type: ControllerClass, ...controllerProps}`, or a pre-instantiated `Controller`
- `Layer.props` is the resolved `LayerProps & PropsT`; `data`, `visible`, `opacity`, `pickable`, `coordinateSystem`, `coordinateOrigin`, `modelMatrix`, `extensions`, `loaders`, `loadOptions` are base fields
- `COORDINATE_SYSTEM.LNGLAT` (default) maps positions as `[longitude, latitude, altitude]`; `CARTESIAN` is used for non-geographic data
- `AccessorFunction` receives `(object, { index, data, target })` — write result into `target` and return it to avoid GC pressure
- `CompositeLayer.renderLayers()` must return `Layer[]` or `LayersList`; the sublayer `id` is namespaced automatically
- `LayerExtension` injects `getShaders`, `initializeState`, `updateState`, `draw`, and `getPickingInfo` hooks into the host layer

[LOCAL_ADMISSION]:
- Wire `Deck` inside a framework integration (e.g., `@deck.gl/mapbox`'s `MapboxOverlay`) rather than managing the canvas directly when co-rendering with a base map.
- Pass `useDevicePixels: true` (default) for retina display support; pass a number to control the pixel ratio manually.
- Picking callbacks (`onHover`, `onClick`, `onDragStart`, `onDrag`, `onDragEnd`) receive `PickingInfo`.

[RAIL_LAW]:
- Package: `@deck.gl/core`
- Owns: WebGL layer orchestration, view/viewport model, picking, lighting, transitions, shader assembly
- Accept: `Deck` for all canvas management; `Layer`/`CompositeLayer` for rendering primitives; `View` + `Controller` for viewport control
- Reject: manual WebGL context management; custom picking loops outside `Deck.pickObject*`
