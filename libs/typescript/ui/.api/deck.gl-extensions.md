# [TS_UI_API_DECK_GL_EXTENSIONS]

`@deck.gl/extensions` mints the concrete `LayerExtension` roster — one subclass per GPU capability injected into any layer's `extensions` array without subclassing the layer. Each extension bakes its shader path through a constructor option and toggles behavior per-frame through a runtime prop and its `Accessor`s under `updateTriggers`; the base, the `extensions` prop, and `Accessor`/`UpdateParameters` defer to `.api/deck.gl-core.md`. `scope:viewer` project-local.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@deck.gl/extensions`
- package: `@deck.gl/extensions` (MIT)
- abi: browser WebGL2/WebGPU through `@deck.gl/core`'s luma.gl `Device`; each extension injects a `@luma.gl/shadertools` `ShaderModule`
- runtime: `scope:viewer` project-local; each extension is a value in a layer's `extensions` array, resolved per-frame with the layer
- modules: `DataFilterExtension`, `BrushingExtension`, `PathStyleExtension`, `FillStyleExtension`, `CollisionFilterExtension`, `MaskExtension`, `ClipExtension`, `_TerrainExtension`, `project64`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: `LayerExtension<OptionsT>` subclasses — one per GPU capability, added to a layer's `extensions: LayerExtension[]`; each mirrors the layer lifecycle (`getShaders`/`initializeState`/`updateState`/`draw`/`finalizeState`) to inject a `ShaderModule`, attributes, and props.

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY] | [CAPABILITY]                                                                   |
| :-----: | :------------------------- | :------------ | :----------------------------------------------------------------------------- |
|  [01]   | `DataFilterExtension`      | class         | GPU show/hide by numeric range or category                                     |
|  [02]   | `BrushingExtension`        | class         | reveal objects within a pointer radius                                         |
|  [03]   | `PathStyleExtension`       | class         | dashed/offset strokes on path/scatterplot/text marks                           |
|  [04]   | `FillStyleExtension`       | class         | pattern-fill on `PolygonLayer`/`ScatterplotLayer`                              |
|  [05]   | `CollisionFilterExtension` | class         | hide overlapping objects (label/icon declutter)                                |
|  [06]   | `MaskExtension`            | class         | geofence via a mask layer                                                      |
|  [07]   | `ClipExtension`            | class         | rectangular clip of a layer's rendered region                                  |
|  [08]   | `_TerrainExtension`        | class         | drape/offset a vector layer onto the `TerrainLayer` surface                    |
|  [09]   | `project64`                | shader-module | internal fp64 projection `ShaderModule<{viewport}>`, not instantiated directly |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: each extension constructs as `new X(OptionsT)` and joins a layer's `extensions: LayerExtension[]`; the option set bakes the shader path — `DataFilterExtension{filterSize?:0-4, categorySize?:0-4, fp64?, countItems?}`, `PathStyleExtension{dash?, offset?, highPrecisionDash?}`, `FillStyleExtension{pattern?}`, the other five construct empty. Injected props below drive it per-frame off the host layer, each accessor prop memoized by `updateTriggers`.

- `DataFilterExtension`: `getFilterValue`/`getFilterCategory`; `filterEnabled`/`filterRange`/`filterSoftRange`/`filterCategories`/`filterTransformSize`/`filterTransformColor`; `onFilteredItemsChange`.
- `BrushingExtension`: `brushingEnabled`, `brushingRadius` (m), `brushingTarget:'source'|'target'|'source_target'|'custom'`, `getBrushingTarget`.
- `PathStyleExtension`: `getDashArray`/`getOffset`, `dashJustified`, `dashGapPickable`.
- `FillStyleExtension`: `fillPatternEnabled`/`fillPatternAtlas`/`fillPatternMapping`/`fillPatternMask`, `getFillPattern`/`getFillPatternScale`/`getFillPatternOffset`.
- `CollisionFilterExtension`: `collisionEnabled`, `collisionGroup`, `getCollisionPriority`, `collisionTestProps`.
- `MaskExtension`: `maskId`, `maskByInstance`, `maskInverted`.
- `ClipExtension`: `clipBounds:[left,bottom,right,top]`, `clipByInstance`.
- `_TerrainExtension`: `terrainDrawMode:'offset'|'drape'`.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `extensions: LayerExtension[]` is one seam on every layer: a cross-layer capability is an instance in that array, composed by concatenation — never a layer subclass or a forked layer prop.
- construction options bake a static shader-compilation switch once; injected props drive per-frame values through `setProps` — recompile with options, drive with props, never conflate.
- host-layer `updateTriggers` memoizes each extension accessor prop as a core `Accessor`; a filter accessor closing over an atom value carries its own `updateTriggers` key.

[STACKING]:
- `@deck.gl/core`(`.api/deck.gl-core.md`): the `LayerExtension<OptionsT>` base, the `extensions: LayerExtension[]` prop, and `Accessor`/`UpdateParameters`/`LayerContext` all live in core — core ships only the base, this pack is the roster the `extensions` prop resolves.
- `@deck.gl/layers`(`.api/deck.gl-layers.md`) + `@deck.gl/geo-layers`(`.api/deck.gl-geo-layers.md`): an extension in a `GeoJsonLayer`/`ScatterplotLayer`/`MVTLayer` `extensions` array gives GPU attribute/time-window filtering with zero data re-slice; `CollisionFilterExtension` declutters `TextLayer` labels; `MaskExtension.maskId` names a layer carrying `operation:'mask'` to geofence; `_TerrainExtension` drapes vector layers over `TerrainLayer`.
- `ui/viewer/geo`: folds the `extensions` array from an `@effect-atom` selector and pushes `filterRange`/`brushingRadius`/`clipBounds`/`maskId` at the `Deck.setProps` sink; an rAF-fed atom clock driving `filterRange` yields a GPU time-slider with no per-frame data churn.

[LOCAL_ADMISSION]:
- imported only inside `ui/viewer` (`scope:viewer`); each extension is a declarative value in a layer's `extensions` array, never a stateful service.
- required peer of `@deck.gl/geo-layers`; central admission in `pnpm-workspace.yaml` is what lets the `extensions` prop resolve concrete extensions in production.

[RAIL_LAW]:
- Package: `@deck.gl/extensions`
- Owns: the concrete `LayerExtension` roster — every GPU capability (data-filter, brushing, path-style, fill-style, collision-filter, mask, clip, terrain-drape) injected into a layer's `extensions` prop
- Accept: one `LayerExtension` instance per capability in `extensions`, the options-bake/props-toggle discriminant, accessor props under `updateTriggers`, multiple stacked extensions per layer, an atom-derived `extensions` array pushed at `setProps`
- Reject: a layer subclass where an extension owns the capability, conflating construction options with runtime props, an extension accessor without its `updateTriggers` key, instantiating `project64` directly, importing `@luma.gl/shadertools` directly instead of through the extension
