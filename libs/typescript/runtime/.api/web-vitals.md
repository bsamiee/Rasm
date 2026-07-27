# [TS_RUNTIME_API_WEB_VITALS]

`web-vitals` owns field measurement of the Core Web Vitals: five idempotent per-metric capture functions each observe one vital across the full page lifecycle — bfcache restore and visibility-change flush included — and report one normalized `Metric` per instance. Measurement is the whole charter; the library never transports, `otel/vital` folds each `Metric` into its own graded fact, and that owner alone mints the OTLP instruments.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `web-vitals`
- package: `web-vitals` (Apache-2.0)
- module: dual build — ESM `dist/web-vitals.js` (`default`), CJS `dist/web-vitals.umd.cjs` (`require`), declarations at `dist/modules/index.d.ts` via the `exports` `types` condition (no top-level `types`); subpaths `.` ships the standard build (five capture functions, metric types), `web-vitals/attribution` re-exports the five enriched with a diagnostic `attribution` object, and per-metric `web-vitals/onLCP.js` / `web-vitals/attribution/onINP.js` tree-shake to one probe.
- runtime: browser only — reads `PerformanceObserver`, Navigation/Paint/Event/LoAF timing, and Layout Shift entries; no Node surface, no framework coupling, one callback per metric with no scheduler.
- plane: `plane:runtime`; the S2 otel browser RUM lane, folder-local to `runtime` where the capture functions are composed.
- rail: `runtime` telemetry — the Core Web Vitals source, one function per metric and one `Metric` row shape across all five; `ui` carries the same dependency for the `[05]` global augmentation alone and imports no member.

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

`web-vitals/attribution` re-exports the five functions with each metric widened to a `*MetricWithAttribution` carrying an `attribution` object: the value's causal subparts, the element and resource it fell on, and the raw `*Entry` handles. Each roster below is whole and correlates with its own metric `name`, so one exhaustive switch is the only reader keeping that correlation. `AttributionReportOpts` adds `generateTarget`, a node-to-selector mapper; `INPAttributionReportOpts` further adds `includeProcessedEventEntries`.

[LCPATTRIBUTION]: `timeToFirstByte: number` `resourceLoadDelay: number` `resourceLoadDuration: number` `elementRenderDelay: number` `target?: string` `url?: string` `lcpEntry?: LargestContentfulPaint` `lcpResourceEntry?: PerformanceResourceTiming` `navigationEntry?: PerformanceNavigationTiming|PerformanceSoftNavigation`
[INPATTRIBUTION]: `inputDelay: number` `processingDuration: number` `presentationDelay: number` `loadState: LoadState` `processedEventEntries: PerformanceEventTiming[]` `longAnimationFrameEntries: PerformanceLongAnimationFrameTiming[]` `interactionTarget?: string` `interactionTime?: DOMHighResTimeStamp` `interactionType?: 'pointer'|'keyboard'` `nextPaintTime?: DOMHighResTimeStamp` `longestScript?: INPLongestScriptSummary` `totalScriptDuration?: number` `totalStyleAndLayoutDuration?: number` `totalPaintDuration?: number` `totalUnattributedDuration?: number`
[CLSATTRIBUTION]: `largestShiftValue?: number` `largestShiftTime?: DOMHighResTimeStamp` `largestShiftTarget?: string` `largestShiftSource?: LayoutShiftAttribution` `largestShiftEntry?: LayoutShift` `loadState?: LoadState`
[FCPATTRIBUTION]: `timeToFirstByte: number` `firstByteToFCP: number` `loadState: LoadState` `fcpEntry?: PerformancePaintTiming` `navigationEntry?: PerformanceNavigationTiming|PerformanceSoftNavigation`
[TTFBATTRIBUTION]: `waitingDuration: number` `cacheDuration: number` `dnsDuration: number` `connectionDuration: number` `requestDuration: number` `navigationEntry?: PerformanceNavigationTiming|PerformanceSoftNavigation`

- `LCPAttribution`, `FCPAttribution`, and `TTFBAttribution` carry every duration subpart unconditionally; a `CLS` visit with no dispatched shift entry and an `INP` interaction under the browser's reporting floor leave their optional subparts absent, so a consumer omits the key rather than reading a zero.
- `INPAttribution`'s four `total*` fields decompose the interaction against the Long Animation Frame timeline and read `undefined` where no LoAF entry intersects, the honest signal that the frame ran under 50 ms; `longestScript` names the single worst script and carries the span of it that INTERSECTED this interaction, never that script's whole duration.
- `generateTarget` fills the three `*target` members and the library's own selector generator fills them otherwise, so a consumer supplying the browser instrumentation's XPath generator reads one element spelling across the attributed fact and the span the same click opened.

[METRIC_WITH_ATTRIBUTION]: `MetricWithAttribution = CLSMetricWithAttribution|FCPMetricWithAttribution|INPMetricWithAttribution|LCPMetricWithAttribution|TTFBMetricWithAttribution`
[ATTRIBUTION_REPORT_OPTS]: `AttributionReportOpts extends ReportOpts` `AttributionReportOpts.generateTarget?: (el:Node|null)=>string|undefined`
[INPATTRIBUTION_REPORT_OPTS]: `INPAttributionReportOpts extends AttributionReportOpts` `INPAttributionReportOpts.durationThreshold?: number` `INPAttributionReportOpts.includeProcessedEventEntries?: boolean`
[INPLONGEST_SCRIPT_SUMMARY]: `INPLongestScriptSummary.entry: PerformanceScriptTiming` `INPLongestScriptSummary.subpart: 'input-delay'|'processing-duration'|'presentation-delay'` `INPLongestScriptSummary.intersectingDuration: number`

## [05]-[PERFORMANCE_GLOBALS]

`web-vitals` ships a types build augmenting `lib.dom` with the performance interfaces the metrics read but no shipped declaration carries, so a consumer types raw entries without a second `@types` package. Soft navigation adds `InteractionContentfulPaint` and `PerformanceSoftNavigation` — the entry types a `soft-navigation` metric computes from, keyed into the augmented `Performance.getEntriesByType`.

| [INDEX] | [GLOBAL]                              | [MEMBERS]                                                                              |
| :-----: | :------------------------------------ | :------------------------------------------------------------------------------------- |
|  [01]   | `LayoutShift`                         | `value` `sources: LayoutShiftAttribution[]` `hadRecentInput`                           |
|  [02]   | `LargestContentfulPaint`              | `renderTime` `loadTime` `size` `id` `url` `element`                                    |
|  [03]   | `PerformanceEventTiming`              | `duration` `interactionId` `targetSelector`                                            |
|  [04]   | `PerformanceLongAnimationFrameTiming` | `startTime` `duration` `renderStart` `styleAndLayoutStart` `blockingDuration`          |
|  [05]   | `PerformanceLongAnimationFrameTiming` | `firstUIEventTimestamp` `scripts: PerformanceScriptTiming[]` `name` `entryType`        |
|  [06]   | `PerformanceScriptTiming`             | `startTime` `duration` `executionStart` `pauseDuration` `forcedStyleAndLayoutDuration` |
|  [07]   | `PerformanceScriptTiming`             | `invokerType` `invoker` `sourceURL` `sourceFunctionName` `sourceCharPosition`          |
|  [08]   | `PerformanceScriptTiming`             | `windowAttribution` `window?` `name` `entryType`                                       |
|  [09]   | `InteractionContentfulPaint`          | `interactionId` `largestContentfulPaint?`                                              |
|  [10]   | `PerformanceSoftNavigation`           | `interactionId` `navigationType?` `paintTime?` `presentationTime?`                     |

- `PerformanceSoftNavigation.getLargestInteractionContentfulPaint?()` resolves that soft navigation's LICP entry.
- `ScriptInvokerType` and `ScriptWindowAttribution` are the two bounded value unions the script rows carry; `Document.prerendering`/`.wasDiscarded` and `PerformanceObserverInit.durationThreshold` round out the augmentation.
- `PerformanceLongAnimationFrameTiming` decomposes its frame across its own coordinates — `startTime` to `renderStart` spans the task work, `renderStart` to `styleAndLayoutStart` the render prologue, and `blockingDuration` the span past the 50 ms jank floor — while its `scripts` rows carry the per-script execution, forced style-and-layout, and paused durations a bounded fold sums.
- `PerformanceEventTiming` gains its augmented members here and inherits `processingStart`, `processingEnd`, and `cancelable` from the shipped lib, so the three-phase interaction split reads across both declarations.

## [06]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Each vital folds its `PerformanceObserver` entries into one normalized `Metric`; `reportAllChanges` streams intermediate values while the default reports the terminal value once, and a stable `id` lets a sink dedupe across bfcache restores or sum `delta` into a session total.
- `rating` pre-buckets `value` against the metric's `*Thresholds`, so a board colors good/needs-improvement/poor without re-deriving the cutoffs.
- Registrars claim their own entry families and publish no roster — `layout-shift`, `paint`, `largest-contentful-paint`, `event`, `long-animation-frame`, and `soft-navigation` are observed inside the package, so a consumer carving entry families around it goes stale on the next release; the browser serves every registered observer from one buffer, so a second READER of a claimed family costs a callback while a second CAPTURE forks the accounting.
- `durationThreshold` floors the `event-timing` observer this package registers, and that floor is per-observer — a sibling observer over the same family taking a different floor sees a different interaction set, which is why the value belongs to the composition and not to either row.

[STACKING]:
- `otel/vital`: each `Metric` folds field-for-field into the owner's sample shape — `name` keys the row through its `_LIBRARY` map, `value` lands as the accounted level because the library already folded it, `rating` becomes the fact's grade with no re-bucketing, `delta` accrues the whole-session total across the instances `id` distinguishes, and `navigationType` rides the fact; the five `*Thresholds` pairs project into the `_rows` budget columns, so the deploy-feed budgets and every dashboard panel read the standard's own cutoffs.
- `otel/vital` `_phases`: the same owner's exhaustive `name` switch projects every attribution roster onto the fact's two causal records — durations into the numeric `phases` (including `longestScript.intersectingDuration` and the four `total*` LoAF splits) and the `*target`, `url`, `interactionType`, `loadState`, `longestScript.subpart`, and the summary entry's `invokerType`/`sourceURL`/`sourceFunctionName` into the string `subject` — through one `Option.fromNullable` filter, so an undispatched subpart drops its key rather than minting a zero and a poor interaction resolves to the code path that spent it.
- `opentelemetry-sdk-trace-web.md` `getElementXPath`: satisfies `AttributionReportOpts.generateTarget`, so an INP or LCP attribution target spells identically to the DOM-event span targets the browser instrumentation rows open on the same trace.
- `ui:system/vital` `_ENTRY`: the `[05]` augmentation alone is what that floor imports — its LoAF row folds `duration`, `blockingDuration`, the task and render-prologue spans `startTime`/`renderStart`/`styleAndLayoutStart` carve, and the summed `duration`/`forcedStyleAndLayoutDuration`/`pauseDuration` of the frame's `scripts`, while its `event` row folds the shipped `processingStart`/`processingEnd` split under the same `durationThreshold` floor `otel/vital` hands `onINP`; both are families this package's own registrars observe, so the coupling is the shared floor, never a roster carve.
- `three`(`ui/.api/three.md`): those long-frame rows render beside three's `renderer.info` per-frame counters on the `viewer/probe` render-frame lane, one board over two evidence lanes sharing the `label`/`value`/`unit` row shape.

[LOCAL_ADMISSION]:
- Register all five capture functions once at composition and fold every callback into one fact stream; the library measures and never transports, so OTLP minting and egress stay at the `otel/vital` owner.
- `web-vitals/attribution` is the standing import — it re-exports the five registrars, the five `*Thresholds` pairs, and the whole type surface, so the enriched build costs one module choice and widens no signature; a per-metric subpath (`web-vitals/onLCP.js`) tree-shakes a consumer probing fewer than five.
- Capture functions admit no unregister — registration is page-lifetime and idempotent, so a scoped composition closes an emission gate rather than claiming a teardown the package does not offer.

[RAIL_LAW]:
- Package: `web-vitals`
- Owns: field measurement of the five Core Web Vitals — one idempotent capture function per vital, the `Metric` shape and its five narrowings, the `*Thresholds` rating cutoffs, the `attribution` diagnostic build, and the augmented DOM performance globals.
- Accept: one reporter registering all five functions at composition, each `Metric` folded into the `otel/vital` fact shape, `reportAllChanges` streaming versus terminal report, `reportSoftNavs` keyed on `navigationId`/`navigationURL`, `durationThreshold` flooring the event stream the INP estimator consumes AND every sibling observer over that family, every attribution subpart and target selector landing on the consuming owner's causal fact records, and the `[05]` augmentation imported alone by a package that windows raw entries and grades none.
- Reject: a second capture registration in any other package, re-deriving `rating` or its cutoffs a `*Thresholds` constant owns, a second `@types` package for the performance globals this build augments, a raw fold that re-derives a metric this package already normalizes, and a sibling observer over a claimed family taking its own `durationThreshold` literal.
