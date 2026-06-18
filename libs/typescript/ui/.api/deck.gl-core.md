# [API_CATALOGUE] @deck.gl/core

`@deck.gl/core` supplies the rendering engine for `deck.gl`: `Deck` (the canvas/WebGL orchestrator), `Layer`/`CompositeLayer`/`LayerExtension` (the layer model), `View`/`Viewport` family, `Controller` family, lighting effects, transition interpolators, `Widget`, and the canonical `LayerProps`/accessor/picking type set consumed by all layer packages.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@deck.gl/core`
- package: `@deck.gl/core`
- module: `@deck.gl/core`
- asset: `dist/index.d.ts`
- rail: viewport

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: core rendering classes
- rail: viewport

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]  | [RAIL]                                          |
| :-----: | :----------------------- | :------------- | :---------------------------------------------- |
|   [1]   | `Deck`                   | class          | top-level canvas orchestrator and event manager |
|   [2]   | `Layer<PropsT>`          | abstract class | base layer (extend for custom layers)           |
|   [3]   | `CompositeLayer<PropsT>` | abstract class | composite layer (wraps sub-layers)              |
|   [4]   | `LayerExtension`         | class          | mixin-style layer feature extension             |
|   [5]   | `LayerManager`           | class          | manages layer lifecycle (advanced use)          |
|   [6]   | `DeckRenderer`           | class          | low-level render orchestrator                   |
|   [7]   | `AttributeManager`       | class          | manages GPU attribute buffers                   |
|   [8]   | `Attribute`              | class          | single GPU attribute accessor                   |

[PUBLIC_TYPE_SCOPE]: view and viewport family
- rail: viewport

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY] | [RAIL]                                    |
| :-----: | :--------------------- | :------------ | :---------------------------------------- |
|   [1]   | `View`                 | class         | base view descriptor                      |
|   [2]   | `MapView`              | class         | web-mercator map view                     |
|   [3]   | `OrbitView`            | class         | orbit (3D turntable) view                 |
|   [4]   | `OrthographicView`     | class         | orthographic (2D) view                    |
|   [5]   | `FirstPersonView`      | class         | first-person perspective view             |
|   [6]   | `Viewport`             | class         | base viewport (projection + unprojection) |
|   [7]   | `WebMercatorViewport`  | class         | web-mercator projection viewport          |
|   [8]   | `OrbitViewport`        | class         | orbit projection viewport                 |
|   [9]   | `OrthographicViewport` | class         | orthographic projection viewport          |
|  [10]   | `FirstPersonViewport`  | class         | first-person projection viewport          |

[PUBLIC_TYPE_SCOPE]: controller family
- rail: viewport

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY] | [RAIL]                       |
| :-----: | :----------------------- | :------------ | :--------------------------- |
|   [1]   | `Controller`             | class         | base input event controller  |
|   [2]   | `MapController`          | class         | pan/zoom/rotate for map view |
|   [3]   | `OrbitController`        | class         | orbit view controller        |
|   [4]   | `OrthographicController` | class         | orthographic view controller |
|   [5]   | `FirstPersonController`  | class         | first-person controller      |
|   [6]   | `TerrainController`      | class         | terrain-aware map controller |

[PUBLIC_TYPE_SCOPE]: layer props and accessor types
- rail: viewport

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY] | [RAIL]                                                 |
| :-----: | :-------------------------- | :------------ | :----------------------------------------------------- |
|   [1]   | `LayerProps`                | type alias    | base layer props (id, data, opacity, visible, …)       |
|   [2]   | `CompositeLayerProps`       | type alias    | `LayerProps` for composite layers                      |
|   [3]   | `Accessor<In, Out>`         | type alias    | `Out \| AccessorFunction<In, Out>`                     |
|   [4]   | `AccessorFunction<In, Out>` | type alias    | `(object, objectInfo) => Out`                          |
|   [5]   | `AccessorContext<T>`        | type alias    | `{ index, data, target }`                              |
|   [6]   | `LayerData<T>`              | type alias    | iterable or `{ length, attributes? }`                  |
|   [7]   | `LayerDataSource<T>`        | type alias    | data \| string URL \| AsyncIterable \| Promise \| null |
|   [8]   | `Position`                  | type alias    | `[lng, lat, alt?]` numeric tuple                       |
|   [9]   | `Color`                     | type alias    | `[r, g, b, a?]` numeric tuple                          |
|  [10]   | `Unit`                      | type alias    | `'meters' \| 'common' \| 'pixels'`                     |
|  [11]   | `Operation`                 | type alias    | `'draw' \| 'mask' \| 'terrain'`                        |
|  [12]   | `Material`                  | type alias    | lighting material settings                             |
|  [13]   | `TextureSource`             | type alias    | `Texture \| HTMLImageElement \| ImageData \| …`        |

[PUBLIC_TYPE_SCOPE]: picking types
- rail: viewport

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY] | [RAIL]                                          |
| :-----: | :--------------------- | :------------ | :---------------------------------------------- |
|   [1]   | `PickingInfo`          | type alias    | `{ layer, index, object, x, y, coordinate, … }` |
|   [2]   | `GetPickingInfoParams` | type alias    | parameters for `getPickingInfo` override        |
|   [3]   | `UpdateParameters<L>`  | type alias    | `{ props, oldProps, context, changeFlags }`     |

[PUBLIC_TYPE_SCOPE]: effects and transitions
- rail: viewport

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY] | [RAIL]                       |
| :-----: | :----------------------- | :------------ | :--------------------------- |
|   [1]   | `LightingEffect`         | class         | scene lighting effect        |
|   [2]   | `AmbientLight`           | class         | ambient light source         |
|   [3]   | `DirectionalLight`       | class         | directional light source     |
|   [4]   | `PointLight`             | class         | point light source           |
|   [5]   | `PostProcessEffect`      | class         | post-process shader effect   |
|   [6]   | `LinearInterpolator`     | class         | linear view-state transition |
|   [7]   | `FlyToInterpolator`      | class         | fly-to camera transition     |
|   [8]   | `TransitionInterpolator` | class         | base transition interpolator |
|   [9]   | `Widget`                 | class         | base interactive map widget  |

[PUBLIC_TYPE_SCOPE]: constants and view-state types
- rail: viewport

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]   | [RAIL]                                       |
| :-----: | :---------------------- | :-------------- | :------------------------------------------- |
|   [1]   | `COORDINATE_SYSTEM`     | const enum-like | coordinate system discriminant               |
|   [2]   | `OPERATION`             | const enum-like | layer render operation                       |
|   [3]   | `UNIT`                  | const enum-like | dimension unit constants                     |
|   [4]   | `TRANSITION_EVENTS`     | const           | view-state transition event tokens           |
|   [5]   | `MapViewState`          | type alias      | `{ longitude, latitude, zoom, … }`           |
|   [6]   | `OrbitViewState`        | type alias      | `{ target, rotationX, rotationOrbit, zoom }` |
|   [7]   | `OrthographicViewState` | type alias      | `{ target, zoom, … }`                        |
|   [8]   | `DeckProps<ViewsT>`     | type alias      | full `Deck` constructor props                |
|   [9]   | `CoordinateSystem`      | type alias      | `COORDINATE_SYSTEM` values                   |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: Deck construction
- rail: viewport

| [INDEX] | [SURFACE]         | [ENTRY_FAMILY] | [RAIL]                                         |
| :-----: | :---------------- | :------------- | :--------------------------------------------- |
|   [1]   | `new Deck(props)` | constructor    | `DeckProps` — mounts WebGL canvas + event loop |

[ENTRYPOINT_SCOPE]: Deck instance operations
- rail: viewport

| [INDEX] | [SURFACE]                     | [ENTRY_FAMILY] | [RAIL]                                     |
| :-----: | :---------------------------- | :------------- | :----------------------------------------- |
|   [1]   | `setProps(props)`             | prop update    | merges partial `DeckProps` update          |
|   [2]   | `finalize()`                  | lifecycle      | destroys WebGL context and event listeners |
|   [3]   | `pickObject(params)`          | picking        | single-object pick at point                |
|   [4]   | `pickMultipleObjects(params)` | picking        | picks all objects at point                 |
|   [5]   | `pickObjects(params)`         | picking        | picks all objects in rectangle             |
|   [6]   | `getCanvas()`                 | DOM query      | `HTMLCanvasElement \| null`                |
|   [7]   | `redraw(reason)`              | render         | forces a redraw with reason string         |

[ENTRYPOINT_SCOPE]: coordinate and shader utilities
- rail: viewport

| [INDEX] | [SURFACE]                                    | [ENTRY_FAMILY] | [RAIL]                                      |
| :-----: | :------------------------------------------- | :------------- | :------------------------------------------ |
|   [1]   | `getShaderAssembler()`                       | shader         | global shader assembler instance            |
|   [2]   | `color`, `project`, `project32`              | shader modules | built-in GLSL shader modules                |
|   [3]   | `gouraudMaterial`, `phongMaterial`, `shadow` | shader modules | lighting shader modules                     |
|   [4]   | `createIterable(data, options?)`             | utility        | normalizes `LayerData<T>` to iterable       |
|   [5]   | `fp64LowPart(x)`                             | utility        | low-order bits of double for fp64 emulation |

## [4]-[IMPLEMENTATION_LAW]

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
