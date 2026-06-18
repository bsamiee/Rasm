# [PLATFORM_TASKLOG]

The folder's open and closed work, distilled from `IDEAS.md`. Each task is a card whose leader carries a status marker — `[QUEUED]`/`[ACTIVE]`/`[BLOCKED]` open; `[COMPLETE]`/`[DROPPED]` closed — plus three to four bullets: the capability or file to build, the external packages to integrate, the integration points and boundaries/wires, and the key considerations. One idea spawns one or more tasks; each names the exact sub-domain/file it lands in.

## [1]-[OPEN]

[QUEUED] Re-found the router on the Navigation API in `routing/app-router.md`.
- Build: the `navigate`-event interception ingress replacing the `popstate` `SubscriptionRef` and the manual `pushState`/`replaceState` + scroll-offset `Ref` map; `event.intercept` with its scroll option owns scroll restoration; `navigation.entries` owns the entry history.
- Packages: `effect` `Stream.asyncScoped`/`Effect.acquireRelease` scoped `window.navigation` `navigate` ingress (the `navigate` event is absent from `WindowEventMap`, so `fromEventListenerWindow` does not reach it); `effect` `Schema.Literal` route axis, `SubscriptionRef` location cell; `nuqs` query-state codec inside `RouteParamCodec`. No router package admitted.
- Wires: the guard fold in `routing/navigation-guard.md` (auth + projection availability); a route transition triggers an intent only through the `interchange` `CommandGateway`; route-resident state surfaces through the `ui` `AtomBinding`.
- Considerations: the `NavigateEvent` `signal` owns pending/abort transition state natively; `canIntercept`/`hashChange` gate the ingress; the location cell becomes the one coupling seam `web-vitals` reads for per-route reset.

[QUEUED] Add the view-transition fold to `routing/app-router.md`.
- Build: one `viewTransition` combinator wrapping the guard-admitted location commit as a scoped Effect resource (start, await ready, await finished), with a reduced-motion gate degrading to an instant swap.
- Packages: native `document.startViewTransition`; `effect` `Effect.acquireRelease` for the scoped transition. Zero animation dependency.
- Wires: composes with the navigation-guard pending state inside the one navigation pipeline; pairs with the `navigate`-event intercept handler.
- Considerations: cross-document and same-document View Transitions reached interop in 2025; the reduced-motion gate is one row, never scattered media-query checks in `ui` components.

[QUEUED] Reset and flush web-vitals per soft navigation in `web-vitals/performance-budget.md`.
- Build: per-soft-navigation reset of the CLS sum `Ref`, the INP running-max, and the `(vital, route)` dedupe set on each guard-admitted route commit; a `visibilitychange`-to-hidden ingress flushing terminal CLS/INP to the `MetricRegistry` gauges plus a final breach check.
- Packages: native `PerformanceObserver` and the `visibilitychange` event; `effect` `Ref`/`HashSet` capture state. No `web-vitals` package.
- Wires: reads the `routing/app-router.md` location cell for the reset key (never mutates it); ships the breach span through the `observability` `MetricRegistry` `span` over the `SelfTelemetry` edge.
- Considerations: the 2025 field-data contract requires CLS and INP reported on `visibilityState` hidden; continuous accumulation under-reports per-route regressions and ships no terminal value.

[QUEUED] Author the `session-replay` sub-domain page and recorder owner.
- Build: `session-replay/session-recorder.md` — an Effect-scoped sampled DOM/interaction recorder gated behind a `RemoteConfig` flag, redacting `Redacted`/input fields at capture, emitting the replay-window id as a span attribute on the closed `MetricRegistry` span axis.
- Packages: `effect` scoped resource + `Stream` capture; the `observability` `MetricRegistry` span attribute surface; reuse the `fault-capture` `CrashReport` sanitization rules. No vendor replay SDK.
- Wires: binds to the `observability` `SelfTelemetry` trace context so a shipped `CrashReport` and a `web.vital.breach` span each carry the replay-window id; gated by the `feature-flags` `RemoteConfig` flag.
- Considerations: sampling rate and redaction are owned in-platform; no third telemetry path — the replay window is a span attribute, never a parallel collector.

[QUEUED] Author the streamed flag-delta ingress in a new `feature-flags/flag-stream.md`.
- Build: `feature-flags/flag-stream.md` — a long-lived SSE flag-delta channel marshalled through `Stream.asyncScoped` into a delta fold that patches the one `RemoteConfig` `FlagSet` `SubscriptionRef` in place, decoding each frame once through `Schema.decodeUnknown`; the existing fixed-interval poll `Schedule` in `remote-config.md` demotes to the reconnect/backfill fallback under one retry policy.
- Packages: `effect` `Stream.asyncScoped`/`Effect.acquireRelease` for the scoped `EventSource` ingress, `Schema.decodeUnknown` for the delta decode, `SubscriptionRef` for the in-place cell patch, `Schedule` for the reconnect/backfill; the browser native `EventSource` for the SSE channel; `RuntimeConfig` for the stream endpoint.
- Wires: patches the one `RemoteConfig.flags` cell `remote-config.md` owns (the `FlagSet` shape and the `services`-owned bucket/variant vocabulary stay settled, never re-authored); reuses the branch `projection` `StreamPolicy` reconnect shape rather than a bespoke retry; a flag value still reaches a component only through the `ui` `AtomBinding`.
- Considerations: the delta decode failure folds to the same `FaultDetail.ConfigError` rail and retains the last-good `FlagSet`, never clearing flags; the poll fallback and the stream feed one fold, so SSE absence is graceful degradation, never a parallel flag source; the deterministic local bucket evaluation is untouched.

[QUEUED] Carry the live service-worker-lifecycle and redial-drain acceptance signals on `offline-cache`.
- Build: the install/activate/`skipWaiting` transition and offline-first navigation-fallback acceptance signal on `service-worker-host.md`, plus the offline-queue redial-drain convergence signal on `background-sync-replay.md`.
- Packages: `workbox-window` registration lifecycle; the native `SyncManager` background wake — both already on the fences.
- Wires: the drain reads the one `local-persistence` `LocalPersistence.offlineQueue` resolved-intent pair into the gateway and re-enqueues over the one `interchange` `FaultDetail` channel.
- Considerations: the fence is body-complete against the widened resolved-intent element shape owned by `local-persistence`; the acceptance signal asserts the live install/activate transition and the redial convergence, not the static wiring.

[QUEUED] Carry the live global-crash-capture acceptance signals on `fault-capture`.
- Build: the acceptance signal on `crash-telemetry.md` that a real uncaught `throw` and a real rejected `Promise` round-trip through `crashFaultOf` into a single shipped `MetricRegistry.span("crash.report", ...)` with its breadcrumb trail, the session dedupe, and the recovery-cell re-mount affordance.
- Packages: `@effect/platform-browser` `BrowserStream` global ingress; `react-error-boundary` render integration — both already on the fences.
- Wires: reconstructs into the `interchange` `FaultDetail` family; ships over the `observability` `SelfTelemetry` collector edge; the recovery cell surfaces through the `ui` `AtomBinding`.
- Considerations: the `forkDaemon` global capture outlives `bootSpa`'s scope; the live residual is the native global-handler attribution, not the projection.

[QUEUED] Carry the live Core-Web-Vitals-attribution acceptance signals on `web-vitals`.
- Build: the acceptance signal on `performance-budget.md` that native `PerformanceObserver` LCP/INP/CLS/TTFB/FCP attribution feeds the `MetricRegistry` Core-Web-Vitals instrument rows, including the 40ms INP `durationThreshold` floor and the soft-navigation reset.
- Packages: native `PerformanceObserver`; `effect` `Stream.asyncScoped` ingress — both already on the fence. No `web-vitals` package.
- Wires: records into the `observability` `MetricRegistry` gauges; ships the breach span over the `SelfTelemetry` edge; reads the `routing` location cell for per-route attribution.
- Considerations: Firefox 114 added `interactionId` so native INP is cross-browser-measurable, validating the no-package stance; the acceptance signal asserts the attribution feed, not the threshold table.

## [2]-[CLOSED]

None.
