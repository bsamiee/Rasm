# [TS_UI_API_VISX_SCALE]

`@visx/scale` re-shapes `d3-scale`'s chained-mutator factories into one config-object form: each factory takes a typed `ScaleConfig` and returns the real d3 scale object `.api/d3.md` owns, unwrapped, so an inapplicable knob is a compile error and `createScale`/`updateScale` discriminate on `type` to build or immutably re-derive on data change.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@visx/scale`
- package: `@visx/scale` (MIT)
- module: dual ESM/CJS via conditional `exports`; no side effects
- runtime: framework-agnostic pure computation, no DOM or React peer; sole dep `@visx/vendor` (the pinned d3 bundle) this surface re-shapes
- plane: `plane:runtime` (W4 `ui`)
- rail: the visx chart spine — scale construction under `.api/visx-shape.md` and `.api/visx-axis.md`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the vocabulary generic chart components constrain against.

[SCALE_TYPES]: `ScaleConfig` `ScaleType` `PickScaleConfig` `AnyD3Scale` `D3Scale` `PickD3Scale` `ScaleInput` `InferD3ScaleOutput` `NiceTime`

- `ScaleConfig` is the discriminated config union `createScale` reads and `ScaleType` its `type` tag; `PickScaleConfig`/`PickD3Scale` project one scale's config and d3 result by tag, `AnyD3Scale`/`D3Scale` gate the returned scale object, `ScaleInput`/`InferD3ScaleOutput` recover a scale's input and output types, and `NiceTime` bounds the temporal `nice` interval.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the config-object factory roster and the polymorphic build entry; every factory returns the real d3 scale object.

| [INDEX] | [FAMILY]        | [SURFACE]                                                                     | [CAPABILITY]                         |
| :-----: | :-------------- | :---------------------------------------------------------------------------- | :----------------------------------- |
|  [01]   | continuous      | `scaleLinear` `scaleLog` `scaleSymlog` `scalePower` `scaleSqrt` `scaleRadial` | quantitative position/size           |
|  [02]   | temporal        | `scaleTime` `scaleUtc`                                                        | date/time domains                    |
|  [03]   | ordinal-spatial | `scaleBand` `scalePoint`                                                      | categorical position                 |
|  [04]   | discrete-map    | `scaleOrdinal` `scaleQuantile` `scaleQuantize` `scaleThreshold`               | quantile/uniform/threshold bucketing |
|  [05]   | polymorphic     | `createScale(ScaleConfig)` `updateScale(AnyD3Scale, ScaleConfig)`             | discriminated build + rederive       |

- [01]-[CONTINUOUS]: config `domain` `range` `nice` `clamp` `zero` `round` `interpolate`, and per sub-type `base` (log), `constant` (symlog), `exponent` (pow).
- [02]-[TEMPORAL]: `nice` accepts a `NiceTime` interval or `{ interval, step }`; log, time, and utc scales reject `zero`.
- [03]-[BAND]: `padding`/`paddingInner`/`paddingOuter`/`align`/`round` keys drive the `bandwidth()`/`step()` bar geometry.
- [UTILS]: `inferScaleType(D3Scale) -> ScaleType` `getTicks(Scale, number?)` `scaleCanBeZeroed(ScaleConfig)` `coerceNumber` `toString` — scale reflection, tick extraction, and zero-eligibility guard.

[COMPOSITION]: `y = scaleLinear<number>({domain:[0,max],range:[innerHeight,0],nice:true,zero:true})` `x = scaleBand<string>({domain:keys,range:[0,innerWidth],padding:0.2})` `next = updateScale(y,{domain:[0,nextMax]})`

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every factory returns the unwrapped d3 scale object, so one config value per axis builds the scale in the chart fold and `updateScale` re-derives it immutably on data change, never a chained mutator or a per-render reconstruction.

[STACKING]:
- `@visx/shape`(`.api/visx-shape.md`) + `@visx/axis`(`.api/visx-axis.md`): a shape's `x`/`y` accessors close over the returned scale object and `Axis` reads the same instance — one scale per axis per chart, built in the chart fold, never re-created per child.
- `@visx/responsive`(`.api/visx-responsive.md`): `useParentSize` dimensions set the config `range` and `updateScale` re-derives the scale on resize, no path arithmetic re-authored.
- `@visx/vendor` d3 bundle (`.api/d3.md`): domain derivation folds `d3-array` (`extent`/`max`) beside the config; a capability the config omits — custom interpolators, `tickFormat` specifiers, a `scaleSequential(interpolateViridis)` colormap — is the returned d3 object's own method, reached through and typed as `AnyD3Scale`, never wrapped.
- within-lib: the `ui` chart rows build one scale per axis in the chart fold and thread it into every shape and axis.

[LOCAL_ADMISSION]:
- Plot (`.api/observablehq-plot.md`) infers its own scales from channels, so a visx scale never feeds a Plot spec. Position and size scales are free; categorical color ranges resolve from `system/token` and data-value colormaps from the d3 chromatic family, per the token law.

[RAIL_LAW]:
- Package: `@visx/scale`
- Owns: typed config-object scale construction for the visx set — the factory roster, the `createScale`/`updateScale` polymorphic entry, the reflection utils, and the `ScaleConfig`/`AnyD3Scale` type vocabulary.
- Accept: one scale per axis built in the chart fold; `updateScale` for data-driven rederive; reach-through to d3 scale methods for ticks/format/invert; d3-direct construction for sequential/diverging scales typed as `AnyD3Scale`.
- Reject: chained d3 mutator construction where the config form exists; per-render scale reconstruction; wrapping the returned scale; a `scaleSequential` spelling expected on this surface; hardcoded ranges where `responsive` dimensions parameterize them.
