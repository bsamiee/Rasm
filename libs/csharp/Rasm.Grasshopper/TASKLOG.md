# [RASM_GRASSHOPPER_TASKLOG]

Grasshopper host boundary's open and closed work, distilled from ideas and design-page RESEARCH residuals.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with. `Atomic` flags a minor-scope task so a later session sizes its turn correctly and does not overscope a batch of small items.

## [01]-[OPEN]

<!-- source-only: open task card template:
[ID]-[STATUS]: <ambitious concise thesis — the capability outcome, never the landing motion>.
- Capability: <the higher-order invariant, owner capability, or concept established — concept grain only, never a page path, row list, or member spelling>.
- Shape: <where the work lands and at what grain — repo-relative page with section/row, or a new-page path; the concrete surface, so Capability never names it>.
- Unlocks: <the downstream capability at the consumer grain — a task narrows its parent idea's Unlocks to THIS slice as `IDEAS.md [SLUG] — consequence`; a set-completion card states the completeness bar that is its acceptance contract>.
- Anchors: <owners, seams, packages, catalogs, doctrines, and techniques making the work plausible — anchors, never procedures>.
- Arms: <BLOCKED or gated cards only; the exact observable flipping it actionable — catalog row landing, member query evidence, package admitted>.
- Route: <present only on a probe, research, or member-pin card; the ordered verification path run before any fence lands>.
- Tension: <only when an unresolved constraint, boundary, or bet shapes the work — the genuine bet, never the arming condition Arms carries>.
- Ripple: <counterpart — cross-folder `pkg` `[SLUG]`, same-folder prerequisite `[SLUG]`; load-bearing build order prefixes follows/precedes/mirrors>.
- Atomic: <present only on a minor-scope task; names the small unit so a later session sizes its turn>.
Capability, Shape, Unlocks, and Anchors are required on every open card, Atomic included; statuses closed — `ACTIVE|QUEUED|BLOCKED` open, `COMPLETE|DROPPED` closed; IDs are SEMANTIC UPPERCASE_SNAKE slugs carrying meaning — never numeric (`[0007]`-class NNNN IDs are a defect), for cards AND research tokens alike; a hyphenated slug anywhere is a defect; repo-relative paths only. Design pages carry the terminal `[RESEARCH]` section always — `(none)` marks empty, absence is an error. Tasks state landing-grain work decomposing an idea.
-->

(none)

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

[HYBRID_CACHE_ROOT]-[COMPLETE]: `Shell/session.md` `[05]-[ROOT]` landed `PlatformCache.Compose` — the plugin-root registration block (raster `ImmutableArray<byte>` codec admitted ahead of the substrate seed, `MaximumPayloadBytes` from the raster ceiling, `ReportTagMetrics` armed) discharging the `libs/csharp/.api/api-hybrid-cache.md` Grasshopper obligations at a landed composition point; `CacheSlot` gained the 64-column key ceiling that keeps every composed key below the silent-bypass floor.
[SHELL_LOG_CLASSIFY]-[COMPLETE]: classification attributes landed on all ten `[LoggerMessage]` methods across the five partials (`PaintLog`/`UiEventsLog`/`RuntimeLog`/`NativeLog`/`CaptureLog`) with the taxonomy clause, the two-rule coverage law, and the five-owner roster on `Shell/telemetry.md`; realizes IDEAS `[LOG_CLASSIFICATION_SWEEP]`.
[PLUGIN_IDENTITY_ADMISSION]-[COMPLETE]: `PlatformTelemetry.Open`, `GhTelemetry.Of`, and `GhInstruments.Of` re-typed `string plugin` to `Shell/hooks.md` `HookScope`; the inline trim/nonblank re-derivations are deleted, the default-struct hole gates at every scope-taking entry, and the `RULINGS.md` single-typed row holds with zero raw-string plugin surfaces.
[EXPLICIT_SET_TWINS]-[COMPLETE]: every selection-scoped `GraphTransact` case carries a `Seq<IDocumentObject>` payload whose emptiness selects the selection verb and whose contents ride the host `*Objects` twin (`GroupCase`/`ChainCase`/`ClusterCase`/`PostureCase` via the six twinned `SelectionPosture` rows/`DressCase`/`DeleteCase`), the four twin-less pin-side rows refuse an explicit payload typed, and `NudgeCase` closed the `MoveSelection` gap — the full twin grid live-verified on the shipped assembly.
[WRAP_PREFLIGHT_GATE]-[COMPLETE]: chain and cluster arms preflight `CanCreateChain`/`CanCreateCluster` on the same roster the mint consumes inside one marshal window (empty payload preflights `ObjectList.SelectedObjects`), a refusal settles the new `GateOutcome.RefusedCase(whyNot)` with no seal — bind-and-invoke proven live ("Chain may not be empty." / "A cluster requires at least one object.").
[DATA_CLEAR_AXIS]-[COMPLETE]: `DeleteDepth` landed as the second delete axis — `Graph`/`Data` rows pairing selection verb, explicit twin, and typed outcome (`CountCase` removed vs the new `ClearedCase` cleared), a `Data`+wires payload refusing at admission because `DeleteObjectData` takes no wire span by host design.
[HOST_CATALOG_TRIAD]-[COMPLETE]: Rhino-side host catalogs landed — `api-rhino-common.md`, `api-rhino-ui.md`, `api-gh2-io.md` at admitted-seam depth, every member decompile-verified via `tools.assay`; `System.Drawing.Common` ruled a compile-time GH1 carrier and folded into `api-gh2-standard-components.md`, never a stub catalog.
[TELEMETRY_ROSTER_TABLE]-[COMPLETE]: `Shell/telemetry.md` [03]-[ROSTER] landed the fifteen-row field-to-instrument kind table with UCUM units and `gh.doc`/`gh.plugin` tag sets.
[METER_CUSTODY]-[COMPLETE]: `Shell/telemetry.md` [02]-[CUSTODY] landed per-ALC `IMeterFactory` custody and unload flush; the app root mints the string-scoped kernel `TelemetryContributorPort` over the roster and admits the meter by name — no app-root adapter member.
[HOOK_POINT_ROWS]-[COMPLETE]: `Shell/hooks.md` [02]-[POINTS] landed the ten-row `rasm.grasshopper.<domain>.<point>` census, each modality ruled from the host cancellation surface.
[EVIDENCE_DRAIN_LAND]-[COMPLETE]: `Shell/events.md` [05]-[DRAIN] landed `DrainPolicy`/`EvidenceDrain` with `itemDropped` accounting and the `Observe`-publish bridge.
[SESSION_JOURNAL_LAND]-[COMPLETE]: `Shell/journal.md` landed `JournalFact`/`JournalRow`/`SessionJournal`/`JournalExport` with per-document ring partitions and the export projection.
[BUDGET_GATE_LAND]-[COMPLETE]: `Canvas/motion.md` [06]-[BUDGET] landed `BudgetRow`/`BudgetSubject`/`BudgetGate` with the host-free kernel corpus law; `Canvas/paint.md` carries the read-time judgment law.
[PAINT_FLUSH_SETTLEMENT]-[COMPLETE]: `Canvas/paint.md` `PaintPlan.Execute` flushes `Graphics.Content.Flush` before the settlement capture, so `PaintReceipt.Latency` covers raster completion.
[DISPATCH_PULSE_LAND]-[COMPLETE]: `Eto/runtime.md` [02]-[DISPATCH] landed `PulseLane` budgets, `StallPolicy`, `DispatchPulse`, `Watch`, and `LastStall` hang evidence over an in-gauge `Op.Catch`.
[LOG_PARTIALS_LAND]-[COMPLETE]: `PaintLog`/`UiEventsLog`/`RuntimeLog`/`NativeLog`/`CaptureLog` `[LoggerMessage]` partials landed beside their retention cells under `GhLog` admission.
[SCREENCAPTURE_DECOMPILE_PROOF]-[COMPLETE]: ilspycmd over `/Applications/RhinoWIP.app/Contents/Frameworks/RhCore.framework/Versions/A/Resources/Microsoft.macOS.dll` proved the full ScreenCaptureKit binding (`SCStream`, `SCShareableContent`, `SCContentFilter`, `SCStreamConfiguration`, `SCScreenshotManager`, `ISCStreamOutput`/`ISCStreamDelegate`) beside the legacy `CGDisplayStream`/`CGWindowListCreateImage` names; verdict rows landed in `.api/api-macos-native.md`, ScreenCaptureKit admitted, the legacy pair rejected.
[SESSION_CAPTURE_LAND]-[COMPLETE]: `Platform/capture.md` landed `SessionCapture` custody (open, frame callback, inverse release chain), stamped raster egress, and the `PaintProof.Judge`/`Correlate` regression and journal seams.
