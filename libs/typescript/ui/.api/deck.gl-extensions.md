# [TS_UI_API_DECK_GL_EXTENSIONS]

`@deck.gl/extensions` is the concrete `LayerExtension` roster the `ui/viewer/geo` plane feeds into any layer's `extensions` prop — the polymorphic capability-injection seam `@deck.gl/core` ships ONLY the base of. Every export is one `LayerExtension<OptionsT>` subclass that injects a GPU capability (filter, brush, dash, pattern-fill, collision-declutter, mask, clip, terrain-drape) into ANY `@deck.gl/layers`/`@deck.gl/geo-layers` layer without subclassing it: `layer.props.extensions = [new DataFilterExtension({filterSize: 2}), new MaskExtension()]`. Options-vs-props is the one discriminant every extension shares: constructor `OptionsT` are static shader-compilation switches that bake the GPU code path (`filterSize`, `dash`, `pattern`, `categorySize`), while the runtime props (`filterRange`, `getDashArray`, `getFillPattern`, `maskId`) are per-frame values + core `Accessor`s read off the host layer and memoized by `updateTriggers` exactly like the layer's own accessors. So the pack is ten capability rows over one `LayerExtension` × any-layer seam, never ten layer variants. This catalog documents each extension's options + injected-props axis, deferring the `LayerExtension` base, the `extensions` prop, and `Accessor`/`UpdateParameters` to `.api/deck.gl-core.md`. `scope:viewer` project-local.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@deck.gl/extensions`
- package: `@deck.gl/extensions`
- license: `MIT`
- abi: browser WebGL2/WebGPU via `@deck.gl/core`'s luma.gl `Device`; each extension injects a `@luma.gl/shadertools` `ShaderModule`
- peer (`~catalog`): `@deck.gl/core` (the `LayerExtension` base, the `extensions` layer prop, `Accessor`, `Layer`, `LayerContext`, `UpdateParameters`), `@luma.gl/core`, `@luma.gl/engine`; deps `@luma.gl/shadertools` (`ShaderModule`), `@luma.gl/webgl`, `@math.gl/core`
- catalog-verdict: KEEP — the concrete `LayerExtension` roster core ships only the base of; required `@deck.gl/geo-layers` peer (the `extensions` prop pack); admitted centrally
- runtime: `scope:viewer` project-local; each extension is a value in a layer's `extensions` array, resolved per-frame with the layer
- modules: `DataFilterExtension`, `BrushingExtension`, `PathStyleExtension`, `FillStyleExtension`, `CollisionFilterExtension`, `MaskExtension`, `ClipExtension`, `Fp64Extension` (deprecated), `_TerrainExtension` (overlay), `project64`

## [02]-[EXTENSION_ROSTER]

[TYPE_SCOPE]: the `LayerExtension` capability rows — one class per GPU capability, each constructed with static option toggles and reading its runtime props off the host layer; add instances to any layer's `extensions: LayerExtension[]`.
- `LayerExtension<OptionsT>` (the core base) exposes `getShaders`/`initializeState`/`updateState`/`draw`/`finalizeState` the layer calls at each lifecycle phase; a subclass injects a `ShaderModule` + attributes + props. Options recompile the GPU path; props toggle at runtime.

| [INDEX] | [SYMBOL]                   | [CONSTRUCTOR_OPTIONS]                                   | [CONSUMER_BOUNDARY]                               |
| :-----: | :------------------------- | :------------------------------------------------------ | :------------------------------------------------ |
|  [01]   | `DataFilterExtension`      | `{filterSize?:0-4,categorySize?:0-4,fp64?,countItems?}` | GPU show/hide by range or category                |
|  [02]   | `BrushingExtension`        | `{}`                                                    | reveal objects within a radius of the pointer     |
|  [03]   | `PathStyleExtension`       | `{dash?, offset?, highPrecisionDash?}`                  | dashed/offset lines on path/scatterplot layers    |
|  [04]   | `FillStyleExtension`       | `{pattern?}`                                            | pattern-fill on `PolygonLayer`/`ScatterplotLayer` |
|  [05]   | `CollisionFilterExtension` | `{}`                                                    | hide overlapping objects (label/icon declutter)   |
|  [06]   | `MaskExtension`            | `{}`                                                    | geofence via a mask layer (`operation:'mask'`)    |
|  [07]   | `ClipExtension`            | `{}`                                                    | rectangular clip of a layer's rendered region     |
|  [08]   | `Fp64Extension`            | `{}` `@deprecated`                                      | superseded by `DataFilterExtension{fp64}`         |
|  [09]   | `_TerrainExtension`        | `{}` overlay                                            | drape/offset onto the `TerrainLayer` surface      |
|  [10]   | `project64`                | — (`ShaderModule<{viewport}>`)                          | fp64 shader `Fp64Extension` injects; internal     |

[INJECTED_PROPS] by row (runtime props + core `Accessor`s read off the host layer):
- [01]-[DATAFILTEREXTENSION]: `getFilterValue`/`getFilterCategory` accessors, `filterRange`/`filterSoftRange`/`filterEnabled`/`filterCategories`/`filterTransformSize`/`filterTransformColor`, `onFilteredItemsChange`.
- [02]-[BRUSHINGEXTENSION]: `brushingEnabled`, `brushingRadius` (m), `brushingTarget:'source'|'target'|'source_target'|'custom'`, `getBrushingTarget`.
- [03]-[PATHSTYLEEXTENSION]: `getDashArray`/`getOffset` accessors, `dashJustified`, `dashGapPickable`.
- [04]-[FILLSTYLEEXTENSION]: `fillPatternAtlas`/`fillPatternMapping`/`fillPatternEnabled`/`fillPatternMask`, `getFillPattern`/`getFillPatternScale`/`getFillPatternOffset`.
- [05]-[COLLISIONFILTEREXTENSION]: `collisionEnabled`, `collisionGroup`, `getCollisionPriority`, `collisionTestProps`.
- [06]-[MASKEXTENSION]: `maskId`, `maskByInstance`, `maskInverted`.
- [07]-[CLIPEXTENSION]: `clipBounds:[left,bottom,right,top]`, `clipByInstance`.
- [08]-[FP64EXTENSION]: none (deprecated).
- [09]-[_TerrainExtension]: `terrainDrawMode:'offset'|'drape'`.
- [10]-[PROJECT64]: none — the `ShaderModule<{viewport}>` `Fp64Extension` injects, not instantiated directly.

## [03]-[IMPLEMENTATION_LAW]

[EXTENSION_TOPOLOGY]:
- one seam, ten capabilities: the `extensions` prop is `LayerExtension[]` on every layer; a cross-layer capability (filter/brush/mask/clip/dash/pattern/collision/terrain) is an instance in that array, NEVER a layer subclass or a forked layer prop. Compose capabilities by concatenating extensions.
- options vs props is the discriminant: constructor `OptionsT` are static shader-compilation switches (`filterSize`/`dash`/`pattern`/`categorySize`) that bake the GPU path — set once at construction; the injected props (`filterRange`/`getDashArray`/`getFillPattern`/`maskId`) are per-frame runtime values toggled via `setProps`. Recompile capability with options, drive behavior with props.
- accessor props obey `updateTriggers`: an extension's accessor props (`getFilterValue`/`getDashArray`/`getFillPattern`/`getCollisionPriority`) are core `Accessor`s memoized by the same `updateTriggers` plane as the host layer's own accessors — a closed-over atom value in a filter accessor needs its `updateTriggers` key.
- extensions stack: multiple extensions coexist on one layer (`[new DataFilterExtension(...), new MaskExtension(), new PathStyleExtension({dash:true})]`); each owns a disjoint shader-module injection.

[INTEGRATION_LAW]:
- Stack on `@deck.gl/core` (`.api/deck.gl-core.md`): the `LayerExtension<OptionsT>` base, the `extensions: LayerExtension[]` layer prop, and `Accessor`/`UpdateParameters`/`LayerContext` all live in core — core ships ONLY the base; this pack is the concrete roster the `extensions` prop resolves.
- Stack with `@deck.gl/layers` + `@deck.gl/geo-layers` (`.api/deck.gl-layers.md`, `.api/deck.gl-geo-layers.md`): add `extensions:[new DataFilterExtension({filterSize:1})]` to a `GeoJsonLayer`/`ScatterplotLayer`/`MVTLayer` for GPU attribute/time-window filtering with zero data re-slice; `CollisionFilterExtension` declutters `TextLayer` labels; `MaskExtension` + a layer with `operation:'mask'` geofences a thematic layer; `_TerrainExtension` drapes vector layers over `TerrainLayer`.
- Stack with `@effect-atom` + a `Scope`: the `extensions` array is an atom-derived value; `filterRange`/`brushingRadius`/`clipBounds`/`maskId` are pushed via `Deck.setProps` from the state fold, and an rAF-fed atom clock driving `filterRange` yields a GPU time-slider with no per-frame data churn — the extension is the deck-side mirror of a `state` selector.
- Stack with mask/terrain siblings: `MaskExtension.maskId` points at a layer carrying the core `operation:'mask'` (`.api/deck.gl-core.md` `OPERATION`); `_TerrainExtension.terrainDrawMode` pairs with the `TerrainLayer` surface (`.api/deck.gl-geo-layers.md`).

[LOCAL_ADMISSION]:
- imported only inside `ui/viewer` (`scope:viewer`); each extension is a declarative value in a layer's `extensions` array, never a stateful service.
- one `LayerExtension` pattern keyed by capability — adding a capability is one extension in the array, never a layer subclass or a parallel prop.
- options (constructor) recompile the GPU path, props toggle at runtime — never conflate; a runtime toggle is a prop, a capability switch is a construction option.
- `Fp64Extension` is `@deprecated` (default precision + `DataFilterExtension{fp64}` supersede it); `_TerrainExtension` is overlay (underscore); `project64` is the internal fp64 shader module, not instantiated directly.
- required peer of `@deck.gl/geo-layers` (the `extensions` prop pack); admitting it centrally in `pnpm-workspace.yaml` is what lets the `extensions` prop resolve concrete extensions in production.

[RAIL_LAW]:
- Package: `@deck.gl/extensions`
- Owns: the concrete `LayerExtension` roster — data-filter, brushing, path-style (dash/offset), fill-style (pattern), collision-filter, mask, clip, terrain-drape, and the deprecated fp64 path — each a GPU capability injected into any layer's `extensions` prop
- Accept: one `LayerExtension` instance per capability in the `extensions` array, the options-vs-props discriminant (construction switches vs runtime props), accessor props under `updateTriggers`, multiple stacked extensions per layer, atom-derived `extensions` pushed at `setProps`
- Reject: a layer subclass where an extension owns the capability, conflating construction options with runtime props, an extension accessor without its `updateTriggers` key, the deprecated `Fp64Extension`, instantiating `project64` directly, importing luma.gl shadertools directly instead of through the extension
