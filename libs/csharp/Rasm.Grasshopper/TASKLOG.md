# [RASM_GRASSHOPPER_TASKLOG]

Grasshopper host boundary's open and closed work, distilled from ideas and design-page RESEARCH residuals.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with. `Atomic` flags a minor-scope task so a later session sizes its turn correctly and does not overscope a batch of small items.

## [01]-[OPEN]

<!-- source-only: open task card template:
[ID]-[STATUS]: <ambitious concise thesis — the capability outcome, never the landing motion>.
- Capability: <the higher-order invariant, owner capability, or concept established — altitude only, never a page path, row list, or member spelling>.
- Shape: <where the work lands and at what grain — repo-relative page with section/row, or a new-page path; the concrete surface, so Capability never names it>.
- Unlocks: <the downstream capability at the consumer grain — a task narrows its parent idea's Unlocks to THIS slice as `IDEAS.md [SLUG] — consequence`; a set-completion card states the completeness bar that is its acceptance contract>.
- Anchors: <owners, seams, packages, catalogs, doctrines, and techniques making the work plausible — anchors, never procedures>.
- Arms: <present only on a BLOCKED or gated card; the exact observable that flips it actionable — a catalog row landing, a member query returning evidence, a package admitted>.
- Route: <present only on a probe, research, or member-pin card; the ordered verification path run before any fence lands>.
- Tension: <only when an unresolved constraint, boundary, or bet shapes the work — the genuine bet, never the arming condition Arms carries>.
- Ripple: <counterpart card — cross-folder as `pkg` `[SLUG]` or a same-folder prerequisite `[SLUG]`, prefixed follows/precedes/mirrors when build order is load-bearing>.
- Atomic: <present only on a minor-scope task; names the small unit so a later session sizes its turn>.
Capability, Shape, Unlocks, and Anchors are required on every open card, Atomic included; statuses closed — `ACTIVE|QUEUED|BLOCKED` open, `COMPLETE|DROPPED` closed; IDs are SEMANTIC UPPERCASE_SNAKE slugs carrying meaning — never numeric (`[0007]`-class NNNN IDs are a defect), for cards AND research tokens alike; a hyphenated slug anywhere is a defect; repo-relative paths only. Design pages carry the terminal `[RESEARCH]` section always — `(none)` marks empty, absence is an error. Tasks state landing-grain work decomposing an idea.
-->

[HYBRID_CACHE_ROOT]-[QUEUED]: GH plugin-root `HybridCache` registration discharges the session-cache app-root obligations.
- Capability: one composition-root cache profile for the GH plugin — raster-byte `IHybridCacheSerializer` admission, `MaximumPayloadBytes` sized to the largest admitted canvas raster, `ReportTagMetrics` enabled with `gh-doc:{documentId:N}` as the per-document hit/miss dimension.
- Shape: one `AddHybridCache`/`IHybridCacheBuilder` registration block at the plugin root composing the app-root obligations the `libs/csharp/.api/api-hybrid-cache.md` Grasshopper row carries.
- Unlocks: L1-only residency, sized raster caching, and per-document cache observability for every `SessionCache` consumer with zero folder edits.
- Anchors: `Shell/session.md` `SlotPolicy` rows; the branch `libs/csharp/.api/api-hybrid-cache.md` registration surface.
- Tension: the APP stratum owning the GH plugin root carries no landed planning folder; this card holds the obligation until that stratum lands.
- Atomic: one registration block.

[SHELL_LOG_CLASSIFY]-[QUEUED]: Classify the log partial roster — sensitivity tags land on every boundary-produced payload parameter.
- Capability: each `[LoggerMessage]` partial's payload parameters carry their `DataClassification` values, and the sweep proves no partial emits an untagged sensitive parameter.
- Shape: classification attributes across the `PaintLog`/`UiEventsLog`/`RuntimeLog`/`NativeLog`/`CaptureLog` partials and the taxonomy clause on `libs/csharp/Rasm.Grasshopper/.planning/Shell/telemetry.md`.
- Unlocks: IDEAS.md `[LOG_CLASSIFICATION_SWEEP]` — the redactor's fail-closed guarantee becomes real for GH egress.
- Anchors: `libs/csharp/.planning/RULINGS.md` boundary-classification row; the landed log partials; the Rhino `LogProperties` seam as the sibling discipline.
- Ripple: mirrors `Rasm.Rhino` `HostSensitivity` classification law.

[PLUGIN_IDENTITY_ADMISSION]-[QUEUED]: Telemetry's plugin discriminator admits through the typed plugin key.
- Capability: one typed plugin identity feeds every per-plugin surface — hook namespaces and the telemetry resource discriminator share one key space by construction.
- Shape: `libs/csharp/Rasm.Grasshopper/.planning/Platform/composition.md` `[07]` — `PlatformTelemetry.Open`'s raw `string plugin` parameter re-types to `HookScope` admission, projecting its value into the `rasm.plugin` resource attribute.
- Unlocks: `RULINGS.md` single-typed plugin-identity row holds with zero raw-string surfaces.
- Anchors: `Shell/hooks.md` `HookScope` `[ValueObject<string>]`; the Rhino `PluginKey` sibling discipline.
- Atomic: one parameter re-type.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

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
