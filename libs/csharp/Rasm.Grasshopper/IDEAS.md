# [RASM_GRASSHOPPER_IDEAS]

Forward pool of higher-order concepts for the Grasshopper host boundary — Grasshopper2 component, document, and canvas capture with native Eto UI composition over the `Rasm` kernel.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with.

## [01]-[OPEN]

<!-- source-only: open idea card template:
[ID]-[STATUS]: <ambitious concise thesis>.
- Capability: <higher-order concept, invariant, or owner capability>.
- Shape: <what the idea becomes as a system, product, owner, or feature set(s)>.
- Unlocks: <new branch, package, workflow, proof, user, or agent capability made possible>.
- Anchors: <owners, seams, packages, doctrines, or techniques that make the idea plausible>.
- Tension: <only when an unresolved constraint, boundary, bet, or dependency shapes the idea>.
- Ripple: <origin/counterpart card this entry pairs with across folders, as `pkg` `[SLUG]`; present only on a cross-folder ripple counterpart card>.
-->

[0001]-[QUEUED]: One telemetry projection fan turns every boundary receipt into `rasm.grasshopper.*` instruments with per-document cost attribution.
- Capability: library-altitude metric projection — a Grasshopper instrument fan folds the folder's receipt families (`PaintReceipt`, `FrameWindow`/`FramePulse`, `SessionReceipt`, `RunPulse`/`RunEvidence`/`SolutionTrace`, session-cache tag metrics) into UCUM-named instruments minted through an injected `IMeterFactory`, zero OTel reference, every write tagged with `gh-doc:{documentId:N}` and composing-plugin identity so multi-document, multi-plugin sessions attribute frame and solve cost without collision.
- Shape: one new page `libs/csharp/Rasm.Grasshopper/.planning/Shell/telemetry.md` owning the kind table, instrument roster, attribution tag law, and per-ALC custody (meter flush and disposal on `AssemblyLoadContext.Unloading`); emitting pages pass receipts, never meter calls.
- Unlocks: GH frame/solve/paint dashboards, the AppHost contributed-arm mount, and cost-per-document analytics with zero emit-calls in domain code.
- Anchors: `libs/csharp/.api/api-diagnostics-metrics.md` `IMeterFactory.Create(MeterOptions)`; AppHost `InstrumentFan.Mount` contributed-arm seam beside `PluginTelemetryHost.Open`; wire-law `rasm.<domain>.<measure>` UCUM naming; `Shell/session.md` `ReportTagMetrics` dimension.

[0002]-[QUEUED]: A typed hook rail gives the boundary veto/observe/replay points with per-app scoping and subscriber-fault isolation.
- Capability: `rasm.grasshopper.<domain>.<point>` registry over document mutation, solution lifecycle, interaction verdicts, and paint phases — veto where the host admits refusal, observe everywhere, replay riding `HistoryLedger`; a faulting subscriber lands on `GhFault` and never poisons siblings; hook namespaces scope per composing plugin so two apps on one Rhino never collide.
- Shape: one new page `libs/csharp/Rasm.Grasshopper/.planning/Shell/hooks.md` owning the point registry, modality rows, and the tap seam the telemetry fan and log rail subscribe through; `Shell/events.md` `UiEvents` stays the raw host-event gate underneath.
- Unlocks: telemetry-as-tap with no scattered emit calls, scripted governance over document mutation, and deterministic replay capture.
- Anchors: AppHost `HookRail` registry shape; `Shell/events.md` `UiEvents.Observe` transactional attachment; `Document/document.md` transaction gate; `Document/history.md` replay; app-neutrality law.

[0003]-[QUEUED]: A bounded off-thread evidence drain feeds a typed session journal — the boundary's analytics egress.
- Capability: UI-thread facts leave through one bounded channel with drop accounting (`Channel.CreateBounded` with `itemDropped`), and a monotone-stamped, per-document session journal folds facts and receipts into an exportable record for post-mortem, support bundles, and analytics.
- Shape: drain rows extend `libs/csharp/Rasm.Grasshopper/.planning/Shell/events.md`; journal ownership lands as one new page `libs/csharp/Rasm.Grasshopper/.planning/Shell/journal.md` with the export projection.
- Unlocks: off-UI-thread consumers at zero publish cost on the paint path, drop evidence as a first-class metric, and session-replay grounding.
- Anchors: `libs/csharp/.api/api-bcl-channels.md` bounded factory with drop callback; `UiEvents` publish-callback boundary; `SolutionTrace` monotone-stamp fold; `MonotonicTimeline`.

[0004]-[QUEUED]: Frame-budget admission turns paint and motion receipts into a standing benchmark rail.
- Capability: budget rows per paint phase and motion drive judge `FrameWindow`/`FramePulse`/`PaintReceipt` costs at read time — violations become typed regression evidence shaped to the estate benchmark-claim families; host-free kernels (tree algebra, route geometry, mark culling) gain corpus benchmark rows.
- Shape: budget owners extend `libs/csharp/Rasm.Grasshopper/.planning/Canvas/motion.md` and `libs/csharp/Rasm.Grasshopper/.planning/Canvas/paint.md`; corpus rows ride the tests estate.
- Unlocks: perf regressions caught as evidence rather than user-visible jank, and dashboard burn-rate feeds from budget violations.
- Anchors: `FlexDrive.Window` draw-window timing; `PaintReceipt` elapsed/drawn/culled evidence; estate `BenchmarkReceipt` families; `MonotonicTimeline`.

[0005]-[QUEUED]: Dispatch-lane stall profiling makes UI-thread health a measured surface.
- Capability: per-lane latency capture on `EtoDispatch` (blocking, awaitable, queued, pump), long-body detection over a threshold policy, and main-thread hang-evidence receipts correlating stalled bodies with their `Op`; span-profile correlation composes at the app root, never here.
- Shape: stall watchdog extends `libs/csharp/Rasm.Grasshopper/.planning/Eto/runtime.md` beside the clock; evidence projects through the [0001] fan.
- Unlocks: hang diagnosis on live sessions, dispatch-latency histograms, and watchdog-driven degradation contribution at the app root.
- Anchors: `EtoDispatch` lane vocabulary; `UiClock`; `SessionReceipt` acknowledgement-latency precedent; AppHost Pyroscope correlation.

[0006]-[QUEUED]: Generated log emission drains every retained fault cell through one library-altitude rail.
- Capability: `[LoggerMessage]` partials per fault family (`GhFault`, paint `LastFault`, subscription `LastFault`, native release faults) emit structured logs through an injected `ILoggerFactory` — app-neutral, no process-global provider, dual-provider composition staying at the app root.
- Shape: emission partials land beside each retaining owner on `libs/csharp/Rasm.Grasshopper/.planning/Canvas/paint.md`, `libs/csharp/Rasm.Grasshopper/.planning/Shell/events.md`, `libs/csharp/Rasm.Grasshopper/.planning/Eto/runtime.md`, and `libs/csharp/Rasm.Grasshopper/.planning/Platform/native.md`, with shared logger admission on `libs/csharp/Rasm.Grasshopper/.planning/Shell/telemetry.md`.
- Unlocks: retained faults become observable without polling `LastFault` cells, closing four-signal completeness for the boundary.
- Anchors: `libs/csharp/.api/api-logging-abstractions.md` `[LoggerMessage]` generated emission; fault-retention cells across the four pages; four-signal law.

[0007]-[QUEUED]: Native capture deepens Platform to a real fourth owner — leased canvas recording for replay and visual regression.
- Capability: a capture gate mints leased display/window capture sessions over the macOS capture surface, producing timestamped frame rasters for visual paint regression, session recording, and journal-correlated replay.
- Shape: one new page `libs/csharp/Rasm.Grasshopper/.planning/Platform/capture.md` beside composition, handlers, and native — satisfying the four-file floor with real capability.
- Unlocks: pixel-truth paint regression against `PaintReceipt` claims, and recorded sessions beside the [0003] journal.
- Anchors: `MacGate` admission and lease inverse-lifecycle pattern; `.api/api-macos-native.md` AppKit/CoreGraphics members; Platform stub ruling.
- Tension: exact capture binding surface (ScreenCaptureKit versus `CGDisplayStream`) inside the shipped Microsoft.macOS assembly is unverified — TASKLOG [0012] pins it before the page lands.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

(none)
