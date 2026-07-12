# [TS_UI_API_VISX_RESPONSIVE]

[PACKAGE_SURFACE]:
- package: `@visx/responsive` · license `MIT`
- module: dual ESM/CJS via conditional `exports`; peers `react` + `@types/react` 18||19; ZERO runtime deps — native `ResizeObserver` is the basis, an injectable polyfill slot exists but nothing is bundled.
- asset: the catalog-bound `ParentSizeModern`/`withParentSizeModern` split is GONE at catalog-bound — `ParentSize`/`useParentSize` is the single implementation.
- plane: `plane:runtime` (W4 `ui`); rail: the visx chart spine — the dimension source every scale range derives from.

`@visx/responsive` answers the one question SVG charts cannot answer alone: how big is my container. `useParentSize` observes a container with `ResizeObserver` and returns live `{ width, height }` plus the ref; `ParentSize` is the same capability as a render-prop component; `useScreenSize`/`withScreenSize` swap the observed subject for the viewport; `ScaleSVG` is the no-JS recovery — a `viewBox`-scaled SVG that stretches instead of re-laying-out. Debounce is a config row (`debounceTime`, `enableDebounceLeadingCall`), not a wrapper, and the exported `debounce` is the same primitive standalone. Dimensions flow ONE way: measurement → margin fold → scale ranges → render; a chart never measures itself mid-render.

## [01]-[SURFACE]

| [INDEX] | [SURFACE]                                                                                         | [FAMILY]     | [CAPABILITY]                                                                                                                                                                   |
| :-----: | :------------------------------------------------------------------------------------------------ | :----------- | :----------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `useParentSize(config?: UseParentSizeConfig)` → `{ parentRef, width, height, top, left, resize }` | hook         | the primary seam — ResizeObserver on the ref'd container; `initialSize`, `debounceTime`, `enableDebounceLeadingCall`, `ignoreDimensions`, `resizeObserverPolyfill` config rows |
|  [02]   | `ParentSize` (`ParentSizeProps` → children `ParentSizeProvidedProps`)                             | render-prop  | `[01]` as a component — children receive `{ width, height, top, left, ref, resize }`                                                                                           |
|  [03]   | `useScreenSize(config?: UseScreenSizeConfig)` / `withScreenSize`                                  | viewport     | window-resize dimensions for full-viewport surfaces                                                                                                                            |
|  [04]   | `withParentSize` (`WithParentSizeProvidedProps`)                                                  | HOC          | retired composition form of `[01]` — the hook is the authored path                                                                                                             |
|  [05]   | `ScaleSVG` (`ScaleSVGProps`)                                                                      | recovery row | `viewBox`-proportional scaling — stretch-not-relayout for static/embedded charts                                                                                               |
|  [06]   | `debounce` (`DebouncedFunction`)                                                                  | primitive    | the standalone debounce the configs compose                                                                                                                                    |

```ts contract
const { parentRef, width, height } = useParentSize({ debounceTime: 120, ignoreDimensions: ["top", "left"] })
// width/height gate the render — a zero-sized first frame renders nothing, never a NaN-ranged scale.
return <div ref={parentRef} className="size-full">{width > 0 && <Chart width={width} height={height} />}</div>
```

## [02]-[INTEGRATION]

[STACK: the visx set (`.api/visx-scale.md`, `.api/visx-group.md`)] — the dimension fold: `useParentSize` → margins → inner width/height → `updateScale` range rederive → the `Group` frame. Resize churn is bounded by `debounceTime` policy, and `ignoreDimensions` drops position-only moves so charts re-render on real size change only.

[STACK: container-query CSS] — Tailwind catalog-bound `@container` variants own STYLING responsiveness (typography, spacing, visibility at container breakpoints); this package owns GEOMETRY responsiveness (scale ranges need pixel numbers). The two compose on one container — classes adapt, scales re-range — and neither substitutes for the other.

[BOUNDARY: the sibling engines] — Plot receives `width`/`height` in its options (the same `useParentSize` numbers feed the spec rebuild); uplot receives them via `setSize` inside its imperative bracket. The measurement seam is shared; only the application differs — so a panel measures ONCE at its container and fans dimensions to whichever engine owns each chart.

## [03]-[RAIL_LAW]

- Owns: chart-container measurement — ResizeObserver dimensions as hook/render-prop, viewport dimensions, debounce policy, and the `viewBox` scaling recovery.
- Accept: `useParentSize` as the authored seam; zero-size render gates; `debounceTime`/`ignoreDimensions` as policy rows; one measurement per panel fanned to all resident charts; `ScaleSVG` only for static/embedded stretch cases.
- Reject: hand-rolled ResizeObserver-in-effect measurement; `withParentSize` in new code (hook exists); the removed `*Modern` spellings; measuring per chart what the panel already measured; CSS container queries expected to produce scale-range numbers.
