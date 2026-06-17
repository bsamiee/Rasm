# [PLATFORM_IDEAS]

The forward pool of higher-order platform concepts, each a card with a bracketed slug leader plus a few bullets — the capability, what it unlocks, and the gap or modern technique it draws on. An idea drives one or more `TASKLOG.md` tasks; a finished or dropped idea moves to `[2]-[CLOSED]` with a one-line disposition.

## [1]-[OPEN]

[NAVIGATION_API_ROUTER]: Re-found `AppRouter` on the now-Baseline Navigation API.
- The capability: one `navigate`-event interception on `window.navigation` lifted into an Effect `Stream` through a scoped `addEventListener`/`removeEventListener` resource owns link clicks, form submits, back/forward, and programmatic navigation in one ingress; `event.intercept` with its scroll option owns scroll restoration natively; `navigation.entries` replaces the hand-maintained scroll-offset `Ref` map. The `navigate` event fires on the `Navigation` interface, so `fromEventListenerWindow` (typed over `WindowEventMap`) cannot reach it — the scoped `Stream.asyncScoped` bridge is the ingress.
- The gap: the prior routing design hand-rolled the History-API pattern (a `popstate` `SubscriptionRef` plus imperative `pushState`/`replaceState` plus a manual scroll-offset map), exactly the fragmentation the Navigation API replaces; the API reached Baseline Newly Available (Safari 26.2, Firefox 147, Chrome/Edge).
- What it unlocks: one unified navigation ingress with no split between `popstate` and programmatic transitions, native scroll restoration, native pending/abort transition state via the `navigate` event signal, and a clean integration seam for the View Transitions API.

[VIEW_TRANSITION_FOLD]: A route-transition wrapper driving `document.startViewTransition` around the location commit.
- The capability: one `viewTransition` combinator in the `routing` sub-domain wraps the guard-admitted location commit; the transition is a scoped Effect resource (start, await ready, await finished) folded into the same navigation pipeline, with a reduced-motion gate that degrades to an instant swap.
- The gap: the routing design has no view-transition concept; cross-document and same-document View Transitions reached interop in 2025 (Firefox 144, Safari 18+, Chrome 111+) and pair directly with the Navigation API.
- What it unlocks: native GPU-composited route transitions with zero animation dependency admitted; a single fold owns the transition lifecycle so it composes with the navigation-guard pending state; reduced-motion accessibility as one gate row, not scattered media-query checks in components.

[SOFT_NAV_VITALS]: Reset and re-attribute INP and CLS per soft (SPA) navigation, keyed off the `routing` location cell.
- The capability: the capture-fiber-local accumulators (the CLS sum `Ref` and the INP running-max) reset on each guard-admitted route commit; a `visibilitychange`-to-hidden ingress flushes the last sample to the `MetricRegistry` gauges and ships a final breach check.
- The gap: continuous-accumulation web-vitals with no soft-navigation reset and no hidden-state flush under-reports per-route regressions and never ships a terminal value; the 2025 reporting contract requires CLS and INP reported on `visibilityState` hidden, with soft-navigation attribution resetting both per SPA navigation.
- What it unlocks: per-route vital attribution (which SPA view regressed, not a session blur), correct terminal CLS/INP matching field-data tooling, and a clean coupling seam between `routing` and `web-vitals` through the one location cell rather than a parallel route tracker.

[TRACE_CORRELATED_REPLAY]: A new `session-replay` sub-domain — a sampled DOM/interaction recorder bound to the SelfTelemetry trace context.
- The capability: an Effect-scoped recorder gated behind a `RemoteConfig` flag, sampling on a fixed rate, redacting `Redacted`/input fields at capture (reusing the `CrashReport` sanitization rules), and emitting the replay-window id as a span attribute on the closed `MetricRegistry` span axis; no third telemetry path, no vendor replay SDK.
- The gap: the folder has crash telemetry and web-vitals breach spans but no way to reconstruct what the user did before a fault; the breadcrumb ring is a coarse event list, not a replay. 2025 frontend observability pairs OpenTelemetry traces with session replay via a SpanProcessor correlation; the platform already owns the WebSdk trace edge and the sanitization rules.
- What it unlocks: root-cause reconstruction for shipped crashes and INP/CLS breaches correlated to the exact trace and route, with sampling and redaction owned in-platform; closes the `session-replay` planned-but-empty gap without a commercial replay vendor.

[STREAMING_FLAG_DELIVERY]: Re-found `RemoteConfig` ingestion on a streamed flag-delta channel rather than the fixed-interval poll.
- The capability: the `RemoteConfig` document ingress folds from a long-lived server-sent stream (`EventSource`/SSE marshalled into an Effect `Stream` through a scoped `Stream.asyncScoped` resource) carrying flag-delta frames decoded once through `Schema.decodeUnknown` and folded into the one `FlagSet` `SubscriptionRef`, with the poll `Schedule` demoted to the reconnect/backfill fallback under one `StreamPolicy`-shaped retry; a delta frame patches the `FlagSet` cell in place so a server-side flag flip propagates in seconds, not the next poll window.
- The gap: the current read-side decodes a whole-document snapshot on a 5-minute fixed `Schedule`, so a flip is invisible for up to a poll period and every refresh re-ships and re-decodes the entire flag set; 2025 SPA flag delivery moved to streamed deltas over a CDN-fronted SSE channel for near-real-time propagation with bounded payloads.
- What it unlocks: near-real-time flag propagation without a reload, a bounded delta payload instead of a whole-document re-fetch, a single reconnect/backfill policy reusing the branch `StreamPolicy` shape rather than a bespoke retry, and a deepened `feature-flags/` sub-domain (the streamed-ingress owner is a second real page beside `remote-config`, dissolving the one-file thinness) — all while the `services`-owned bucket/variant vocabulary and the deterministic local bucket stay unchanged.

## [2]-[CLOSED]

[OPENFEATURE_PROVIDER_SHAPE]: DROPPED — a thin `resolveBooleanEvaluation`-style projection of the existing total `Match` dispatch is a task-sized shaping anchored on one named external standard, below the higher-order-folder-idea bar; the OpenFeature reason/variant alignment, if it earns admission, is one growth row on `remote-config`'s existing `evaluate`, not a folder idea.
