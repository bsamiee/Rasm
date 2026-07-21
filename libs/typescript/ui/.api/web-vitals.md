# [TS_UI_API_WEB_VITALS]

[PACKAGE_SURFACE]:
- package: `web-vitals` · license `Apache-2.0` · zero dependencies
- module: dual build — `dist/web-vitals.js` (`module`, ESM), `dist/web-vitals.umd.cjs` (`require`); types at `dist/modules/index.d.ts` via the `exports` `types` condition, no top-level `types` field.
- subpath: `.` ships the standard build (five capture functions + metric types); `web-vitals/attribution` ships the attribution build (same five functions re-exported, each metric enriched with a diagnostic `attribution` object); per-metric subpaths (`web-vitals/onLCP.js`, `web-vitals/attribution/onINP.js`, …) tree-shake to one probe.
- runtime: browser only — reads `PerformanceObserver`, Navigation/Paint/Event/LoAF timing, and Layout Shift entries; no Node surface, no framework coupling, callback-per-metric with no scheduler.
- plane: `plane:runtime` (W4 `ui`); folder-local to `ui`, the `[VITAL_PLANE]` browser performance-evidence lane feeding `system/vital.md`.
- rail: `ui` telemetry — the canonical Core Web Vitals source, one function per metric, one `Metric` row shape across all five.

`web-vitals` is the field-measurement end of the ui telemetry spectrum: five idempotent capture functions (`onLCP` `onCLS` `onINP` `onFCP` `onTTFB`) each observe one Core Web Vital across the full page lifecycle — bfcache restore and visibility-change flush included — reporting one normalized `Metric` per instance.

Its contract is ONE metric shape (`Metric`: `name`/`value`/`delta`/`rating`/`id`/`entries`/`navigationType`), FIVE narrowings fixing `name` and the `entries` element type, and ONE opt bag (`reportAllChanges` streaming versus terminal report). Measurement is the whole charter — the library never transports; a reporter folds each `Metric` into the probe row shape and the app plane owns OTLP egress.

Every metric carries a stable `id` for dedupe and a running `delta` so an analytics sink can either sum deltas into a session total or report the terminal value once; `rating` pre-buckets the value against the metric's `MetricRatingThresholds`, so a board colors good/needs-improvement/poor without re-deriving the cutoffs.

## [01]-[METRIC_MODEL]

One base `Metric` interface with a closed `name` union; each metric narrows `name` to its literal and `entries` to the concrete `PerformanceEntry` subtype it computes from. `MetricType` is the discriminated union a single reporter switches on; `MetricRatingThresholds` is the `[good, needs-improvement]` cutoff pair each `*Thresholds` constant fills.

```ts signature
interface Metric {
  name: 'CLS' | 'FCP' | 'INP' | 'LCP' | 'TTFB'
  value: number                                                          // metric value in the metric's own unit (ms, or unitless CLS score)
  rating: 'good' | 'needs-improvement' | 'poor'                          // pre-bucketed against the metric's thresholds
  delta: number                                                          // change since last report; equals value on first report
  id: string                                                             // unique per instance — dedupe key; a new id mints on bfcache restore
  entries: PerformanceEntry[]                                            // source entries; may be empty (e.g. CLS of 0)
  navigationType: 'navigate' | 'reload' | 'back-forward' | 'back-forward-cache' | 'prerender' | 'restore'
}
interface CLSMetric  extends Metric { name: 'CLS';  entries: LayoutShift[] }
interface FCPMetric  extends Metric { name: 'FCP';  entries: PerformancePaintTiming[] }
interface INPMetric  extends Metric { name: 'INP';  entries: PerformanceEventTiming[] }
interface LCPMetric  extends Metric { name: 'LCP';  entries: LargestContentfulPaint[] }
interface TTFBMetric extends Metric { name: 'TTFB'; entries: PerformanceNavigationTiming[] }
type MetricType = CLSMetric | FCPMetric | INPMetric | LCPMetric | TTFBMetric
type MetricRatingThresholds = [number, number]                          // ≦[0] good · >[0] and ≦[1] needs-improvement · >[1] poor
type LoadState = 'loading' | 'dom-interactive' | 'dom-content-loaded' | 'complete'
```

## [02]-[CAPTURE_FUNCTIONS]

Five capture functions, one per vital; each takes a metric-narrowed callback and an optional opt bag, returns `void`, and is idempotent per page (calling twice does not double-register). `reportAllChanges: true` fires the callback on every intermediate change; the default fires once when the value finalizes. `onINP` widens its opts with `durationThreshold` (the `event-timing` floor, default `40`). Each `*Thresholds` export is the metric's rating cutoff pair.

| [INDEX] | [FN]     | [METRIC]     | [OPTS]          | [CUTOFFS]        | [CAPTURES]                                     |
| :-----: | :------- | :----------- | :-------------- | :--------------- | :--------------------------------------------- |
|  [01]   | `onLCP`  | `LCPMetric`  | `ReportOpts`    | `LCPThresholds`  | largest-contentful-paint render time           |
|  [02]   | `onCLS`  | `CLSMetric`  | `ReportOpts`    | `CLSThresholds`  | cumulative layout-shift score, session windows |
|  [03]   | `onINP`  | `INPMetric`  | `INPReportOpts` | `INPThresholds`  | worst interaction-to-next-paint latency        |
|  [04]   | `onFCP`  | `FCPMetric`  | `ReportOpts`    | `FCPThresholds`  | first-contentful-paint time                    |
|  [05]   | `onTTFB` | `TTFBMetric` | `ReportOpts`    | `TTFBThresholds` | time-to-first-byte from navigation start       |

```ts signature
interface ReportOpts { reportAllChanges?: boolean }
interface INPReportOpts extends ReportOpts { durationThreshold?: number } // event-timing floor; default 40
const onLCP: (onReport: (metric: LCPMetric) => void, opts?: ReportOpts) => void
const onCLS: (onReport: (metric: CLSMetric) => void, opts?: ReportOpts) => void
const onINP: (onReport: (metric: INPMetric) => void, opts?: INPReportOpts) => void
const onFCP: (onReport: (metric: FCPMetric) => void, opts?: ReportOpts) => void
const onTTFB: (onReport: (metric: TTFBMetric) => void, opts?: ReportOpts) => void
const LCPThresholds: MetricRatingThresholds  // [2500, 4000]
const CLSThresholds: MetricRatingThresholds  // [0.1, 0.25]
const INPThresholds: MetricRatingThresholds  // [200, 500]
const FCPThresholds: MetricRatingThresholds  // [1800, 3000]
const TTFBThresholds: MetricRatingThresholds // [800, 1800]
```

## [03]-[ATTRIBUTION_BUILD]

`web-vitals/attribution` re-exports the same five functions with each metric widened to a `*MetricWithAttribution` carrying a diagnostic `attribution` object — the field-debugging payload pinpointing WHICH element, subpart, or timing phase drove a poor value. `AttributionReportOpts` adds `generateTarget` (a custom node→selector mapper); `INPAttributionReportOpts` further adds `includeProcessedEventEntries`. Each shape decomposes its metric into causal subparts; a `*target` selector (`interactionTarget`/`largestShiftTarget`) and the raw `*Entry` handles ride alongside the timing splits below.

| [INDEX] | [SHAPE]           | [PRINCIPAL_SUBPARTS]                                                              |
| :-----: | :---------------- | :------------------------------------------------------------------------------- |
|  [01]   | `LCPAttribution`  | `timeToFirstByte` `resourceLoadDelay` `resourceLoadDuration` `elementRenderDelay` |
|  [02]   | `INPAttribution`  | `inputDelay` `processingDuration` `presentationDelay` `longestScript` `loadState` |
|  [03]   | `CLSAttribution`  | `largestShiftValue` `largestShiftTime` `largestShiftSource` `loadState`           |
|  [04]   | `FCPAttribution`  | `timeToFirstByte` `firstByteToFCP` `loadState` `fcpEntry`                         |
|  [05]   | `TTFBAttribution` | `waitingDuration` `dnsDuration` `connectionDuration` `requestDuration`            |

```ts signature
type MetricWithAttribution = CLSMetricWithAttribution | FCPMetricWithAttribution | INPMetricWithAttribution | LCPMetricWithAttribution | TTFBMetricWithAttribution
interface AttributionReportOpts extends ReportOpts { generateTarget?: (el: Node | null) => string | undefined }
interface INPAttributionReportOpts extends AttributionReportOpts { durationThreshold?: number; includeProcessedEventEntries?: boolean }
interface INPLongestScriptSummary { entry: PerformanceScriptTiming; subpart: 'input-delay' | 'processing-duration' | 'presentation-delay'; intersectingDuration: number }
```

## [04]-[PERFORMANCE_GLOBALS]

Its types build augments the DOM lib with the not-yet-standard performance interfaces the metrics read, so a consumer types raw entries without a second `@types` package. `INPAttribution.longAnimationFrameEntries` surfaces the Long Animation Frame API directly — the same LoAF stream the `[VITAL_PLANE]` card names for long-task and render profiling beyond the five headline vitals.

| [INDEX] | [GLOBAL]                              | [FIELDS_USED]                                                                |
| :-----: | :------------------------------------ | :-------------------------------------------------------------------------- |
|  [01]   | `LayoutShift`                         | `value` `sources: LayoutShiftAttribution[]` `hadRecentInput`                |
|  [02]   | `LargestContentfulPaint`              | `renderTime` `loadTime` `size` `id` `url` `element`                         |
|  [03]   | `PerformanceEventTiming`              | `duration` `interactionId` `targetSelector`                                 |
|  [04]   | `PerformanceLongAnimationFrameTiming` | `renderStart` `styleAndLayoutStart` `blockingDuration` `scripts`            |
|  [05]   | `PerformanceScriptTiming`             | `invokerType` `invoker` `executionStart` `sourceURL` `forcedStyleAndLayoutDuration` |

## [05]-[INTEGRATION]

Each `Metric` maps field-for-field onto the probe row shape (`viewer/probe.md`): `name` keys the metric family, `value`/`delta` fold into the seeded bounded window, `rating` colors the claim-versus-local board, and `id` dedupes across bfcache restores. A reporter registers all five functions once at composition and folds every callback into one fact stream; the library never imports a collector, so OTLP minting and egress stay at the app/runtime plane per the `probe.md:24` altitude boundary.

LoAF entries from `INPAttribution.longAnimationFrameEntries` and the augmented `PerformanceScriptTiming` feed the long-task and render-profiling rows `[VITAL_PLANE]` folds beside the vitals; `three`/deck.gl GPU capture already owns the render-frame lane those rows extend.
