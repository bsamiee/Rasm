# [RASM_GRASSHOPPER_IDEAS]

Forward pool of higher-order concepts for the Grasshopper host boundary — Grasshopper2 component, document, and canvas capture with native Eto UI composition over the `Rasm` kernel.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with.

## [01]-[OPEN]

<!-- source-only: open idea card template:
[ID]-[STATUS]: <ambitious concise thesis — the capability outcome, never the landing motion>.
- Capability: <the higher-order invariant, owner capability, or concept established — concept grain only, never a page path, row list, or member spelling>.
- Shape: <where the work lands and at what grain — repo-relative page with section/row, or a new-page path; the concrete surface, so Capability never names it>.
- Unlocks: <the downstream capability at the consumer grain — a task narrows its parent idea's Unlocks to THIS slice as `IDEAS.md [SLUG] — consequence`; a set-completion card states the completeness bar that is its acceptance contract>.
- Anchors: <owners, seams, packages, catalogs, doctrines, and techniques making the work plausible — anchors, never procedures>.
- Arms: <BLOCKED or gated cards only; the exact observable flipping it actionable — catalog row landing, member query evidence, package admitted>.
- Route: <present only on a probe, research, or member-pin card; the ordered verification path run before any fence lands>.
- Tension: <only when an unresolved constraint, boundary, or bet shapes the work — the genuine bet, never the arming condition Arms carries>.
- Ripple: <counterpart — cross-folder `pkg` `[SLUG]`, same-folder prerequisite `[SLUG]`; load-bearing build order prefixes follows/precedes/mirrors>.
Capability, Shape, Unlocks, and Anchors are required on every open card; statuses closed — `ACTIVE|QUEUED|BLOCKED` open, `COMPLETE|DROPPED` closed; IDs are SEMANTIC UPPERCASE_SNAKE slugs carrying meaning — never numeric (`[0007]`-class NNNN IDs are a defect), for cards AND research tokens alike; a hyphenated slug anywhere is a defect; repo-relative paths only. Design pages carry the terminal `[RESEARCH]` section always — `(none)` marks empty, absence is an error. Ideas state higher-order concepts, never landing-grain tasks.
-->

[LOG_CLASSIFICATION_SWEEP]-[QUEUED]: Classified log egress — every GH boundary log line carries its sensitivity class to the app-root redactor.
- Capability: log payloads embedding user content, document names, plugin paths, or host identity leave the boundary classification-tagged, so the fail-closed app-root redactor sees every sensitive value instead of passing unclassified lines through unredacted.
- Shape: a sensitivity taxonomy and total classification sweep over the folder's log partial roster, at parity with the Rhino `HostSensitivity` discipline; lands across `libs/csharp/Rasm.Grasshopper/.planning/Shell/telemetry.md` and the retaining log-partial owners.
- Unlocks: one redaction posture across both host boundaries; a GH document title or user path stops crossing the export seam invisible.
- Anchors: `libs/csharp/.planning/RULINGS.md` boundary-classification row; the landed `GhLog` admission and per-ALC custody; the app-root `DataClassification` value set.
- Ripple: `[SHELL_LOG_CLASSIFY]` decomposes this.

[LAYER_OVERLAY_CONSUMER]-[QUEUED]: Canvas overlays composite through the CoreAnimation layer graph instead of repainting every frame.
- Capability: canvas-owned decoration that survives a frame without a repaint — badges, focus rings, drag ghosts, and progress chrome ride a mounted layer tree the compositor animates, so motion costs no paint pass and wide-colour fill stays Display-P3 end to end.
- Shape: a canvas-side composer over `libs/csharp/Rasm.Grasshopper/.planning/Platform/composition.md` `Compose.Mount` and `LayerPaint`, seated on `libs/csharp/Rasm.Grasshopper/.planning/Canvas/paint.md` beside `PaintPlan` and anchored through `Platform/native.md` `MacAnchor`.
- Unlocks: the layer graph, its style mint, its glide attachment, and its effect family gain their first consumer; a decorated canvas stops paying a full mark walk per animated frame.
- Anchors: `Platform/composition.md` `LayerNode`/`LayerMount`/`LayerPaint`/`Glides`/`Effects`; `Canvas/paint.md` `PathSpec.Build` supplying the Eto path `LayerPaint.Stroked` converts; `Canvas/motion.md` `CanvasPacer` for the frame edge.
- Tension: the strata law points S2 Canvas down at S1 Platform, so the composer seats on the canvas side; a Platform-side canvas composer inverts the one forbidden direction.

## [02]-[CLOSED]

<!-- source-only: closed idea card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

[GH_TELEMETRY_FAN]-[COMPLETE]: `Shell/telemetry.md` landed `GhTelemetry`/`GhInstruments`/`GhEvidence` — the typed pre-envelope fold with per-ALC custody; the app root mints the string-scoped kernel `TelemetryContributorPort` and admits the `Rasm.Grasshopper` meter by name, so no app-root adapter member exists.
[GH_HOOKS_REGISTRY]-[DROPPED]: invalidated by the substrate-homing collapse — the `(point, scope, token)`-keyed registry with `Tap`/`Faults`/`LastFault` was per-folder hook machinery the kernel signal capsule owns; `Shell/hooks.md` now composes `GhHooks` as scoped `(point, scope)` kernel `HookPoint<HookSignal>` instances with the shared `IsolatedFault` evidence cell.
[DRAIN_AND_JOURNAL]-[COMPLETE]: `Shell/events.md` [05]-[DRAIN] landed `EvidenceDrain` (bounded, `itemDropped`-accounted) and `Shell/journal.md` landed `SessionJournal` with `Mount`/`Append`/`Export` — document attribution keys `DocumentCase` ids only.
[BUDGET_GATE_JUDGE]-[COMPLETE]: `Canvas/motion.md` [06]-[BUDGET] landed `BudgetRow`/`BudgetSubject`/`BudgetGate.Judge` with `BudgetBreach` evidence, and `Canvas/paint.md` carries the read-time judgment law under the `paint.pass` row.
[DISPATCH_PULSE_WATCH]-[COMPLETE]: `Eto/runtime.md` [02]-[DISPATCH] landed `PulseLane`/`StallPolicy`/`DispatchPulse` with `Watch`/`Tune`/`LastStall`; the gauge's own `Op.Catch` sits inside the timestamp pair so every body exit mints one pulse.
[GENERATED_LOG_EMISSION]-[COMPLETE]: generated emission landed as `PaintLog`, `UiEventsLog`, `RuntimeLog`, `NativeLog`, and `CaptureLog` partials beside their retaining owners, with `GhLog` per-ALC admission on `Shell/telemetry.md`.
[SESSION_CAPTURE_FAMILY]-[COMPLETE]: `Platform/capture.md` landed `SessionCapture`/`CaptureScout`/`PaintProof` over the ScreenCaptureKit family TASKLOG `[SCREENCAPTURE_DECOMPILE_PROOF]` verified in the shipped assembly; the tension resolved — ScreenCaptureKit is the admitted family, the legacy CG pair rejected.
[GH_BOARD_PACK_PROVENANCE]-[COMPLETE]: `GhInstruments.Board` carries `grasshopper.fan` as the pack's own first column and the iac `_PACKS` tuple seats that key, so both ends spell one wire and the shell's canvas, solve, and dispatch series compile onto the estate board plane through the shared producer-pack ingest arm.
