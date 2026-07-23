# [TS_UI_API_VISX_SCALE]

[PACKAGE_SURFACE]:
- package: `@visx/scale` (MIT)
- module: dual — conditional `exports` (`import` → `esm/index.js`, `require` → `lib/index.js`, `types` → `lib/index.d.ts`); no side effects, no React peer (the one visx package that is pure computation — `react`/`@types/react` peers absent).
- asset: single dep `@visx/vendor` — the pinned d3 bundle (`d3-scale`, `d3-interpolate`, `d3-time`, `d3-array`, `internmap` among its rows); no raw `d3-*` dep and no `prop-types` at catalog-bound.
- plane: `plane:runtime` (W4 `ui`); rail: the visx chart spine — scale construction under `.api/visx-shape.md` / `.api/visx-axis.md`.

`@visx/scale` re-shapes `d3-scale`'s chained-mutator factories into ONE config-object form: every factory takes a typed config value (`scaleLinear({ domain, range, nice, clamp, zero })`) and returns the REAL d3 scale object — nothing is wrapped or renamed downstream, so the scale that feeds a visx `Axis` or a `LinePath` accessor is the same object `.api/d3.md` documents (`ticks`/`tickFormat`/`invert`/`bandwidth`/`copy` all live). The config keys are typed PER SCALE TYPE (`'clamp' | 'interpolate' | 'nice' | 'round' | 'zero'` for linear, band padding keys for band, …), so an inapplicable knob is a compile error, and `createScale`/`updateScale` give the polymorphic entry — construct from a `ScaleConfig` discriminated on `type`, immutably re-derive on data change.

## [01]-[SURFACE]

Per-family config keys, the reflection utils, and the type vocabulary carry to the keyed list below; every factory returns the real d3 scale object.

| [INDEX] | [SURFACE]                                                                     | [FAMILY]        | [CAPABILITY]                          |
| :-----: | :---------------------------------------------------------------------------- | :-------------- | :------------------------------------ |
|  [01]   | `scaleLinear` `scaleLog` `scaleSymlog` `scalePower` `scaleSqrt` `scaleRadial` | continuous      | quantitative position/size            |
|  [02]   | `scaleTime` `scaleUtc`                                                        | temporal        | date domains; `nice` takes `NiceTime` |
|  [03]   | `scaleBand` `scalePoint`                                                      | ordinal-spatial | categorical; `bandwidth()`/`step()`   |
|  [04]   | `scaleOrdinal` `scaleQuantile` `scaleQuantize` `scaleThreshold`               | discrete-map    | quantile/uniform/threshold bucketing  |
|  [05]   | `createScale(config: ScaleConfig)` / `updateScale(scale, config)`             | polymorphic     | discriminated build + rederive        |

- [01]-[CONTINUOUS]: config `domain` `range` `nice` `clamp` `zero` `round` `interpolate`, plus `base`/`constant`/`exponent` per sub-type.
- [03]-[BAND]: padding/align/round keys; `bandwidth()`/`step()` feed bar geometry.
- [UTILS]: `inferScaleType(scale)` `coerceNumber` `getTicks(scale, count?)` `scaleCanBeZeroed(config)` `toString` — scale reflection, tick extraction, zero-eligibility.
- [TYPES]: `ScaleConfig` `ScaleType` `PickScaleConfig` `AnyD3Scale` `D3Scale` `PickD3Scale` `ScaleInput` `InferD3ScaleOutput` `NiceTime` — the vocabulary generic chart components constrain against.

[SURFACES]: `y = scaleLinear<number>({domain:[0,max],range:[innerHeight,0],nice:true,zero:true})` `x = scaleBand<string>({domain:keys,range:[0,innerWidth],padding:0.2})` `next = updateScale(y,{domain:[0,nextMax]})`

There is NO `scaleSequential` row — `ScaleType` excludes it; a sequential/diverging colormap scale constructs directly from the d3 substrate (`.api/d3.md` `scaleSequential(interpolateViridis)`) and passes wherever `AnyD3Scale` is accepted.

## [02]-[INTEGRATION]

[STACK: the visx set (`.api/visx-shape.md`, `.api/visx-axis.md`, `.api/visx-responsive.md`)] — the scale is the shared value: `useParentSize` yields dimensions, the scale config's `range` derives from them, shape accessors close over `x`/`y` scales, and `Axis` receives the same object — one scale instance per axis per chart, constructed in the chart fold, never re-created per child.

[STACK: the d3 substrate (`.api/d3.md`)] — domain derivation is `d3-array` (`extent`/`max`/`nice`-adjacent folds) beside the config; anything the config surface does not expose (custom interpolators, sequential scales, `tickFormat` specifiers) is the underlying d3 object's own method — reach through, never wrap.

[BOUNDARY] — Plot (`.api/observablehq-plot.md`) infers its own scales from channels; a visx scale never feeds a Plot spec. Color scales obey the token law: position/size scales are free, categorical color ranges resolve from `system/token`, data-value colormaps from the d3 chromatic family.

## [03]-[RAIL_LAW]

- Owns: typed config-object scale construction for the visx set — the factory roster, `createScale`/`updateScale` polymorphic entry, scale reflection utils, and the `ScaleConfig`/`AnyD3Scale` type vocabulary.
- Accept: one scale per axis built in the chart fold; `updateScale` for data-driven rederive; reach-through to d3 scale methods for ticks/format/invert; d3-direct construction for sequential/diverging scales typed as `AnyD3Scale`.
- Reject: chained d3 mutator construction where the config form exists; per-render scale reconstruction; wrapping the returned scale; a `scaleSequential` spelling expected here; hardcoded ranges where `responsive` dimensions parameterize them.
