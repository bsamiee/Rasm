# [TS_UI_API_WEB_VITALS]

`web-vitals` owns field measurement of the Core Web Vitals: five idempotent per-metric capture functions each observe one vital across the full page lifecycle — bfcache restore and visibility-change flush included — and report one normalized `Metric` per instance. Measurement is the whole charter; the library never transports, a reporter folds each `Metric` onto the `viewer/probe` row shape, and the app plane owns OTLP egress.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `web-vitals`
- package: `web-vitals` (Apache-2.0)
- module: dual build — ESM `dist/web-vitals.js` (`default`), CJS `dist/web-vitals.umd.cjs` (`require`), declarations at `dist/modules/index.d.ts` via the `exports` `types` condition (no top-level `types`); subpaths `.` ships the standard build (five capture functions, metric types), `web-vitals/attribution` re-exports the five enriched with a diagnostic `attribution` object, and per-metric `web-vitals/onLCP.js` / `web-vitals/attribution/onINP.js` tree-shake to one probe.
- runtime: browser only — reads `PerformanceObserver`, Navigation/Paint/Event/LoAF timing, and Layout Shift entries; no Node surface, no framework coupling, one callback per metric with no scheduler.
- plane: `plane:runtime` (W4 `ui`); the `[VITAL_PLANE]` browser performance-evidence lane, folder-local to `ui`.
- rail: `ui` telemetry — the Core Web Vitals source, one function per metric and one `Metric` row shape across all five.

## [02]-[METRIC_MODEL]

One base `Metric` interface carries the closed `name` union and the reporting fields every metric shares; each narrowing fixes `name` to its literal and `entries` to the concrete `PerformanceEntry` subtype it computes from. `MetricType` is the discriminated union a single reporter switches on; `MetricRatingThresholds` is the `[good, needs-improvement]` cutoff pair each `*Thresholds` constant fills.

[METRIC]: `Metric.name: 'CLS'|'FCP'|'INP'|'LCP'|'TTFB'` `Metric.value: number` `Metric.rating: 'good'|'needs-improvement'|'poor'` `Metric.delta: number` `Metric.id: string` `Metric.entries: PerformanceEntry[]` `Metric.navigationType: 'navigate'|'reload'|'back-forward'|'back-forward-cache'|'prerender'|'restore'|'soft-navigation'` `Metric.navigationId: number` `Metric.navigationInteractionId?: number` `Metric.navigationStartTime?: number` `Metric.navigationURL?: string`
[CLSMETRIC]: `CLSMetric.name: 'CLS'` `CLSMetric.entries: LayoutShift[]`
[FCPMETRIC]: `FCPMetric.name: 'FCP'` `FCPMetric.entries: PerformancePaintTiming[]`
[INPMETRIC]: `INPMetric.name: 'INP'` `INPMetric.entries: PerformanceEventTiming[]`
[LCPMETRIC]: `LCPMetric.name: 'LCP'` `LCPMetric.entries: LargestContentfulPaint[]`
[TTFBMETRIC]: `TTFBMetric.name: 'TTFB'` `TTFBMetric.entries: PerformanceNavigationTiming[]|PerformanceSoftNavigation[]`
[METRIC_TYPE]: `MetricType = CLSMetric|FCPMetric|INPMetric|LCPMetric|TTFBMetric`
[METRIC_RATING_THRESHOLDS]: `MetricRatingThresholds = [number,number]`
[LOAD_STATE]: `LoadState = 'loading'|'dom-interactive'|'dom-content-loaded'|'complete'`

## [03]-[CAPTURE_FUNCTIONS]

Five module-level capture functions, one per vital; each takes a metric-narrowed callback and an optional opt bag, returns `void`, and is idempotent per page. `reportAllChanges: true` fires on every intermediate change while the default reports once at finalize; `reportSoftNavs: true` reports against soft navigations where the browser exposes the entries. `durationThreshold` floors `event-timing` at `40` ms on the base bag, and `onINP`'s `INPReportOpts` re-declares it.

| [INDEX] | [SURFACE]                                  | [THRESHOLDS]     | [CAPABILITY]                                 |
| :-----: | :----------------------------------------- | :--------------- | :------------------------------------------- |
|  [01]   | `onLCP((LCPMetric)=>void, ReportOpts?)`    | `LCPThresholds`  | largest-contentful-paint render time         |
|  [02]   | `onCLS((CLSMetric)=>void, ReportOpts?)`    | `CLSThresholds`  | cumulative layout-shift over session windows |
|  [03]   | `onINP((INPMetric)=>void, INPReportOpts?)` | `INPThresholds`  | worst interaction-to-next-paint latency      |
|  [04]   | `onFCP((FCPMetric)=>void, ReportOpts?)`    | `FCPThresholds`  | first-contentful-paint time                  |
|  [05]   | `onTTFB((TTFBMetric)=>void, ReportOpts?)`  | `TTFBThresholds` | time-to-first-byte from navigation start     |

[REPORT_OPTS]: `ReportOpts.reportAllChanges?: boolean` `ReportOpts.durationThreshold?: number` `ReportOpts.reportSoftNavs?: boolean`
[INPREPORT_OPTS]: `INPReportOpts extends ReportOpts` `INPReportOpts.durationThreshold?: number`
[THRESHOLDS]: each `*Thresholds` export is a `MetricRatingThresholds` cutoff pair.

## [04]-[ATTRIBUTION_BUILD]

`web-vitals/attribution` re-exports the five functions with each metric widened to a `*MetricWithAttribution` carrying a diagnostic `attribution` object — the field-debugging payload decomposing the value into causal subparts, a `*target` selector (`interactionTarget`/`largestShiftTarget`), and the raw `*Entry` handles. `AttributionReportOpts` adds `generateTarget` (a custom node→selector mapper); `INPAttributionReportOpts` further adds `includeProcessedEventEntries`.

| [INDEX] | [SHAPE]           | [PRINCIPAL_SUBPARTS]                                                              |
| :-----: | :---------------- | :-------------------------------------------------------------------------------- |
|  [01]   | `LCPAttribution`  | `timeToFirstByte` `resourceLoadDelay` `resourceLoadDuration` `elementRenderDelay` |
|  [02]   | `INPAttribution`  | `inputDelay` `processingDuration` `presentationDelay` `longestScript` `loadState` |
|  [03]   | `CLSAttribution`  | `largestShiftValue` `largestShiftTime` `largestShiftSource` `loadState`           |
|  [04]   | `FCPAttribution`  | `timeToFirstByte` `firstByteToFCP` `loadState` `fcpEntry`                         |
|  [05]   | `TTFBAttribution` | `waitingDuration` `dnsDuration` `connectionDuration` `requestDuration`            |

[METRIC_WITH_ATTRIBUTION]: `MetricWithAttribution = CLSMetricWithAttribution|FCPMetricWithAttribution|INPMetricWithAttribution|LCPMetricWithAttribution|TTFBMetricWithAttribution`
[ATTRIBUTION_REPORT_OPTS]: `AttributionReportOpts extends ReportOpts` `AttributionReportOpts.generateTarget?: (el:Node|null)=>string|undefined`
[INPATTRIBUTION_REPORT_OPTS]: `INPAttributionReportOpts extends AttributionReportOpts` `INPAttributionReportOpts.durationThreshold?: number` `INPAttributionReportOpts.includeProcessedEventEntries?: boolean`
[INPLONGEST_SCRIPT_SUMMARY]: `INPLongestScriptSummary.entry: PerformanceScriptTiming` `INPLongestScriptSummary.subpart: 'input-delay'|'processing-duration'|'presentation-delay'` `INPLongestScriptSummary.intersectingDuration: number`

## [05]-[PERFORMANCE_GLOBALS]

`web-vitals` ships a types build augmenting `lib.dom` with the performance interfaces the metrics read but no shipped declaration carries, so a consumer types raw entries without a second `@types` package. Soft navigation adds `InteractionContentfulPaint` and `PerformanceSoftNavigation` — the entry types a `soft-navigation` metric computes from, keyed into the augmented `Performance.getEntriesByType`.

| [INDEX] | [GLOBAL]                              | [FIELDS_USED]                                                                       |
| :-----: | :------------------------------------ | :---------------------------------------------------------------------------------- |
|  [01]   | `LayoutShift`                         | `value` `sources: LayoutShiftAttribution[]` `hadRecentInput`                        |
|  [02]   | `LargestContentfulPaint`              | `renderTime` `loadTime` `size` `id` `url` `element`                                 |
|  [03]   | `PerformanceEventTiming`              | `duration` `interactionId` `targetSelector`                                         |
|  [04]   | `PerformanceLongAnimationFrameTiming` | `renderStart` `styleAndLayoutStart` `blockingDuration` `scripts`                    |
|  [05]   | `PerformanceScriptTiming`             | `invokerType` `invoker` `executionStart` `sourceURL` `forcedStyleAndLayoutDuration` |
|  [06]   | `InteractionContentfulPaint`          | `interactionId` `largestContentfulPaint?`                                           |
|  [07]   | `PerformanceSoftNavigation`           | `interactionId` `navigationType?` `paintTime?` `presentationTime?`                  |

- `PerformanceSoftNavigation.getLargestInteractionContentfulPaint?()` resolves that soft navigation's LICP entry.

## [06]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Each vital folds its `PerformanceObserver` entries into one normalized `Metric`; `reportAllChanges` streams intermediate values while the default reports the terminal value once, and a stable `id` lets a sink dedupe across bfcache restores or sum `delta` into a session total.
- `rating` pre-buckets `value` against the metric's `*Thresholds`, so a board colors good/needs-improvement/poor without re-deriving the cutoffs.

[STACKING]:
- `viewer/probe`: each `Metric` folds field-for-field onto the probe row shape — `name` keys the metric family, `value`/`delta` fold into the seeded bounded window, `rating` colors the claim-versus-local board, `id` dedupes across restores; under `reportSoftNavs` the reporter keys the fact stream on `navigationId`/`navigationURL` when a metric arrives against a prior URL.
- `three`(`.api/three.md`): `INPAttribution.longAnimationFrameEntries` LoAF rows and the augmented `PerformanceScriptTiming` feed the long-task and render-profiling rows that fold beside three's `renderer.info` per-frame counters in the `viewer/probe/receipt` render-frame lane.

[LOCAL_ADMISSION]:
- Register all five capture functions once at composition and fold every callback into one fact stream; the library measures and never transports, so OTLP minting and egress stay at the app/runtime plane.
- Import a per-metric subpath (`web-vitals/onLCP.js`) to tree-shake to the probed vitals, and import `web-vitals/attribution` only where field debugging needs the `attribution` payload.

[RAIL_LAW]:
- Package: `web-vitals`
- Owns: field measurement of the five Core Web Vitals — one idempotent capture function per vital, the `Metric` shape and its five narrowings, the `*Thresholds` rating cutoffs, the `attribution` diagnostic build, and the augmented DOM performance globals.
- Accept: one reporter registering all five functions at composition, each `Metric` folded onto the `viewer/probe` row shape, `reportAllChanges` streaming versus terminal report, `reportSoftNavs` keyed on `navigationId`/`navigationURL`, attribution subparts routed to field debugging.
- Reject: transporting or minting OTLP inside `ui`, re-deriving `rating` cutoffs a `*Thresholds` constant owns, a second `@types` package for the performance globals this build augments, hand-rolled `PerformanceObserver` wiring the capture functions own.
