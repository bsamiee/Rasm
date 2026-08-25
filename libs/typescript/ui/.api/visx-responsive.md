# [TS_UI_API_VISX_RESPONSIVE]

`@visx/responsive` owns container measurement for the visx chart spine — the pixel dimensions a chart cannot derive from its own render, delivered as a hook, render-prop, or viewport source that every scale range reads.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the measurement config, result, and provided-prop shapes.

[RESPONSIVE_TYPES]: `UseParentSizeConfig` `UseParentSizeResult` `ParentSizeState` `UseScreenSizeConfig` `ParentSizeProps` `ParentSizeProvidedProps` `ScaleSVGProps` `WithParentSizeProvidedProps` `WithScreenSizeProvidedProps` `DebouncedFunction`

- `UseParentSizeConfig` types the hook input and `UseParentSizeResult` its `{ width, height, top, left, parentRef, node, resize }` output; `ParentSizeState` carries the measured `width`/`height`/`top`/`left`, the `*ProvidedProps` shapes type what the render-prop and HOCs inject, and `DebouncedFunction` the cancelable primitive the resize configs compose.

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the measurement seams — hook, render-prop, viewport source, HOCs, `viewBox` recovery, and the standalone debounce.

| [INDEX] | [FAMILY]    | [SURFACE]                                                    | [CAPABILITY]                                           |
| :-----: | :---------- | :----------------------------------------------------------- | :----------------------------------------------------- |
|  [01]   | hook        | `useParentSize(UseParentSizeConfig?)`                        | primary seam — `ResizeObserver` on the ref'd container |
|  [02]   | render-prop | `ParentSize` (`ParentSizeProps` → `ParentSizeProvidedProps`) | `[01]` as render-prop args; measured shape per child   |
|  [03]   | viewport    | `useScreenSize(UseScreenSizeConfig?)` `withScreenSize`       | window-resize `{ width, height }` for full-viewport    |
|  [04]   | HOC         | `withParentSize` (`WithParentSizeProvidedProps`)             | parent dimensions into a wrapped class component       |
|  [05]   | recovery    | `ScaleSVG` (`ScaleSVGProps`)                                 | `viewBox`-proportional scaling; stretch, not relayout  |
|  [06]   | primitive   | `debounce` (`DebouncedFunction`)                             | the standalone debounce the resize configs compose     |

- [CONFIG]: `initialSize` `resizeObserverPolyfill` `ignoreDimensions` `externalRef` and the debounce keys; result `{ width, height, top, left, parentRef, node, resize }`.
- [HOC]: `withParentSize` injects `{ parentWidth, parentHeight }` and `withScreenSize` `{ screenWidth, screenHeight }` — prop names distinct from the hooks' `width`/`height`.
- [DEBOUNCE]: `debounceTime` sets the trailing delay and `enableDebounceLeadingCall` the leading edge; `debounce(func, wait, { leading })` is the same primitive standalone, its result carrying `.cancel()`.

[COMPOSITION]: `div(parentRef, className)` `Chart(width, height)`

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Dimensions flow one way — measurement to margin fold to scale `range` to render — so a chart never measures itself mid-render, and one panel measurement fans to every resident chart and every engine.

[STACKING]:
- `@visx/scale`(`.api/visx-scale.md`) + `@visx/group`(`.api/visx-group.md`): `useParentSize` dimensions minus margins give inner width and height, `updateScale` re-derives each scale `range`, and the `Group` frame offsets the plot; `debounceTime` bounds resize churn and `ignoreDimensions` drops position-only moves, so scales re-range on real size change only.
- `@observablehq/plot`(`.api/observablehq-plot.md`) + `uplot`(`.api/uplot.md`): Plot takes `width`/`height` in its options and uplot via `setSize({width,height})`; the same `useParentSize` numbers feed both, so one measurement seam serves every engine and only the application differs.
- container-query CSS: Tailwind `@container` variants own STYLING responsiveness — typography, spacing, visibility at container breakpoints — while this package owns GEOMETRY responsiveness, since scale ranges need pixel numbers; both fold onto one container.
- within-lib: a `ui` panel measures once through `useParentSize` and fans the dimensions to whichever engine owns each chart.
