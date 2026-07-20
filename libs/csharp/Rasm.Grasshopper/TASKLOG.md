# [RASM_GRASSHOPPER_TASKLOG]

Grasshopper host boundary's open and closed work, distilled from ideas and design-page RESEARCH residuals.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with. `Atomic` flags a minor-scope task so a later session sizes its turn correctly and does not overscope a batch of small items.

## [01]-[OPEN]

<!-- source-only: open task card template:
[ID]-[STATUS]: <ambitious concise thesis>.
- Capability: <higher-order concept, invariant, or owner capability>.
- Shape: <what the idea becomes as a system, product, owner, or feature set(s)>.
- Unlocks: <new branch, package, workflow, proof, user, or agent capability made possible>.
- Anchors: <owners, seams, packages, doctrines, or techniques that make the idea plausible>.
- Tension: <only when an unresolved constraint, boundary, bet, or dependency shapes the idea>.
- Ripple: <origin/counterpart card this entry pairs with across folders, as `pkg` `[SLUG]`; present only on a cross-folder ripple counterpart card>.
- Atomic: <present only on a minor-scope task; one short phrase naming the small unit so a later session does not overscope its turn>.
-->

[0003]-[QUEUED]: Receipt-to-instrument mapping pins every `rasm.grasshopper.*` row.
- Capability: exact field-to-instrument table — `PaintReceipt` elapsed/drawn/culled, `FrameWindow` draw window, `SessionReceipt` acknowledgement latency, `RunPulse`/`RunEvidence` counts and progress, drain drop counts — each with UCUM unit and its `gh-doc` plus plugin-identity tag set.
- Shape: instrument roster and kind table on `libs/csharp/Rasm.Grasshopper/.planning/Shell/telemetry.md`.
- Unlocks: idea [0001] lands with a closed roster, and the AppHost arm mount consumes fixed kind spellings.
- Anchors: receipt owners on `Canvas/paint.md`, `Canvas/motion.md`, `Shell/session.md`, `Document/solution.md`; branch `api-diagnostics-metrics.md`.

[0004]-[QUEUED]: Per-ALC meter custody pins the plugin-root composition seam.
- Capability: `IMeterFactory` injection shape, meter flush and disposal on `AssemblyLoadContext.Unloading`, and the contributed-arm handshake with the AppHost mount.
- Shape: custody rows on `libs/csharp/Rasm.Grasshopper/.planning/Shell/telemetry.md`, with app-root obligations recorded beside the existing session-cache obligation pattern.
- Unlocks: Rhino plugin unload never strands instruments; idea [0001] composes at any app shape.
- Anchors: AppHost `PluginTelemetryHost.Open`; TASKLOG [0001] app-root obligation precedent.
- Atomic: one custody row set.

[0005]-[QUEUED]: Hook-point census rules veto capability per host surface.
- Capability: enumerated `rasm.grasshopper.<domain>.<point>` rows across the document transaction gate, solution lifecycle, interaction verdicts, and paint phases — each ruled veto-capable or observe-only from the host event's actual cancellation surface.
- Shape: point registry on `libs/csharp/Rasm.Grasshopper/.planning/Shell/hooks.md`.
- Unlocks: idea [0002] lands with a closed, host-truthful registry.
- Anchors: `Shell/events.md` `UiSource` rows; `.api/api-gh2-document.md` and `.api/api-gh2-canvas.md` event members.

[0006]-[QUEUED]: Bounded drain policy pins capacity, drop accounting, and the observe-to-channel bridge.
- Capability: `Channel.CreateBounded` options (capacity, single-reader, full-mode) with `itemDropped` accounting surfaced as drop evidence, and one bridge from the `UiEvents.Observe` publish callback into the channel writer.
- Shape: drain rows on `libs/csharp/Rasm.Grasshopper/.planning/Shell/events.md`.
- Unlocks: idea [0003]'s journal reads a lossless-or-accounted stream.
- Anchors: branch `api-bcl-channels.md` bounded factory with drop callback.
- Atomic: one policy row set and one bridge.

[0007]-[QUEUED]: Journal fold shape pins rows, partitions, and export projection.
- Capability: monotone-stamped journal rows keyed per document, folding facts and receipts, with an export projection for support bundles and analytics.
- Shape: journal owner on the new page `libs/csharp/Rasm.Grasshopper/.planning/Shell/journal.md`.
- Unlocks: idea [0003] session journal.
- Anchors: `SolutionTrace` monotone-claim fold; `DocumentToken` identity; `MonotonicTimeline`.

[0008]-[QUEUED]: Budget rows and violation evidence pin the frame-budget rail.
- Capability: per-phase and per-drive budget rows judging `FrameWindow`/`FramePulse`/`PaintReceipt` costs, violation receipts shaped to the estate benchmark-claim families, and the host-free kernel list for corpus rows (tree algebra, route geometry, mark culling).
- Shape: budget owners on `libs/csharp/Rasm.Grasshopper/.planning/Canvas/motion.md` and `libs/csharp/Rasm.Grasshopper/.planning/Canvas/paint.md`.
- Unlocks: idea [0004].
- Anchors: `FlexDrive.Window`; `PaintReceipt`; estate `BenchmarkReceipt` families.

[0009]-[QUEUED]: `Graphics.Flush` seals raster completion into the paint receipt stamp.
- Capability: settlement stamp captured after an explicit flush so `PaintReceipt` elapsed covers real raster completion, never command buffering.
- Shape: one executor row on `libs/csharp/Rasm.Grasshopper/.planning/Canvas/paint.md`.
- Unlocks: honest paint-cost evidence for ideas [0001] and [0004].
- Anchors: `.api/api-eto-drawing.md` `void Flush()`; `PaintPlan.Execute` settlement capture.
- Atomic: one flush row before settlement.

[0010]-[QUEUED]: Stall watchdog pins lane capture points, threshold policy, and hang evidence.
- Capability: latency capture on each `EtoDispatch` lane, long-body threshold policy, and a hang-evidence receipt correlating stalled bodies with their `Op`.
- Shape: watchdog rows on `libs/csharp/Rasm.Grasshopper/.planning/Eto/runtime.md`.
- Unlocks: idea [0005].
- Anchors: dispatch lane vocabulary; `UiClock`; `SessionReceipt` latency precedent.

[0011]-[QUEUED]: Fault-cell census wires generated log emission per family.
- Capability: enumerated retention cells (`PaintHook.LastFault`, `UiSubscription.LastFault`, `GhFault` construction sites, native release faults) each mapped to one `[LoggerMessage]` partial with event id, level, and structured fields, under an injected app-neutral `ILoggerFactory` admission.
- Shape: emission partials beside each retaining owner on `libs/csharp/Rasm.Grasshopper/.planning/Canvas/paint.md`, `libs/csharp/Rasm.Grasshopper/.planning/Shell/events.md`, `libs/csharp/Rasm.Grasshopper/.planning/Eto/runtime.md`, and `libs/csharp/Rasm.Grasshopper/.planning/Platform/native.md`; shared admission on `libs/csharp/Rasm.Grasshopper/.planning/Shell/telemetry.md`.
- Unlocks: idea [0006].
- Anchors: branch `api-logging-abstractions.md` generated emission.

[0012]-[BLOCKED]: Which capture members does the shipped Microsoft.macOS assembly bind — ScreenCaptureKit (`SCStream`, `SCShareableContent`, `SCContentFilter`) or only `CGDisplayStream` and `CGWindowListCreateImage`?
- Capability: verified member spellings for the capture surface idea [0007] composes.
- Shape: verdict rows feeding `libs/csharp/Rasm.Grasshopper/.planning/Platform/capture.md` and the `.api/api-macos-native.md` catalog.
- Unlocks: idea [0007] page landing.
- Anchors: `tools/assay` api decompile over the RhinoWIP-bundled Microsoft.macOS assembly; `reference_microsoft_macos_in_rhinowip` memory route.

[0013]-[QUEUED]: Capture lease shape pins session custody and frame egress.
- Capability: leased capture session (acquire, frame callback, inverse release), timestamped raster egress, and the comparison seam for visual paint regression against `PaintReceipt` claims.
- Shape: owners on the new page `libs/csharp/Rasm.Grasshopper/.planning/Platform/capture.md`.
- Unlocks: idea [0007].
- Anchors: `MacGate` lease pattern; `Lease<T>` inverse lifecycle; journal correlation from idea [0003].

[0002]-[QUEUED]: Host `.api` catalogs for the Rhino-side assemblies close the folder's last unverified member routes.
- Capability: folder `.api/` catalogs for `RhinoCommon`, `Rhino.UI`, `GrasshopperIO`, and `System.Drawing.Common` at the admitted-seam depth the GH2/Eto catalogs hold.
- Shape: one catalog per assembly covering the members the design pages compose — `RhinoDoc`, `Rhino.UI.Dialogs.ShowEditBox`/`ShowNumberBox`, `EtoExtensions.UseRhinoStyle`, `IReader`/`IWriter`/`IStorable`, and the `Rhino.Geometry` port carriers.
- Unlocks: every fence member on those seams verifies against a folder catalog instead of estate memory, and the README registry claim closes.
- Anchors: `tools/assay` api decompile over the installed RhinoWIP assemblies; the landed `api-gh2-*`/`api-eto-*` catalog form.
- Atomic: four catalog files.

[0001]-[QUEUED]: GH plugin-root `HybridCache` registration discharges the session-cache app-root obligations.
- Capability: one composition-root cache profile for the GH plugin — raster-byte `IHybridCacheSerializer` admission, `MaximumPayloadBytes` sized to the largest admitted canvas raster, `ReportTagMetrics` enabled with `gh-doc:{documentId:N}` as the per-document hit/miss dimension.
- Shape: one `AddHybridCache`/`IHybridCacheBuilder` registration block at the plugin root composing the `.api/api-hybrid-cache.md` `[APP_ROOT_OBLIGATIONS]` rows.
- Unlocks: L1-only residency, sized raster caching, and per-document cache observability for every `SessionCache` consumer with zero folder edits.
- Anchors: `Shell/session.md` `SlotPolicy` rows; the folder `.api/api-hybrid-cache.md` overlay; the branch `libs/csharp/.api/api-hybrid-cache.md` registration surface.
- Tension: the APP stratum owning the GH plugin root carries no landed planning folder; this card holds the obligation until that stratum lands.
- Atomic: one registration block.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

(none)
