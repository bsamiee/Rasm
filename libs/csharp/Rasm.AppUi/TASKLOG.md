# [APPUI_TASKLOG]

Open and closed work for the product UI engine, distilled from the ideas in `IDEAS.md`. Each open card carries a status marker — `[QUEUED]`, `[ACTIVE]`, or `[BLOCKED]` — plus the capability or file to build, the external packages to integrate, the integration points and boundaries or wires, and the key considerations. Live-host probes and tool-gated arms are tracked as blocked tasks against the substrate that unblocks them.

## [1]-[OPEN]

[T-BACKEND-PORT] [QUEUED] — GpuBackend port over the render substrate
- Extend the `GpuBackend` SmartEnum in `viewport/viewport-pipeline.md` with a `RenderTarget` factory column per backend and add Vello and Graphite rows, so `RenderGraph.Lease` and every `RenderPass`/`CustomVisual` emit body bind a backend-provided target factory instead of the leased `GRContext` plus `SKRuntimeEffect`; mirror the target-factory column into `visuals/visuals-offscreen.md` for the offscreen capsule.
- Integrate VelloSharp (wgpu DX12/Metal/Vulkan/OpenGL/WebGPU plus the SkiaSharp-compat shim) alongside the admitted SkiaSharp; VelloSharp is a new manifest admission this task lands in the one C# manifest.
- Wires internal to `viewport/` and `visuals/`; backend selection consumes the platform capability the `hosts/surface-hosts.md#EMBED_CAPSULE` lease already surfaces, aligned to that named seam and never a new host-coupled surface.
- Consider that the per-backend emit path (Vello scene encode versus `SKRuntimeEffect` shader) diverges below the `RenderTarget` factory, so the factory column owns the divergence and the render-graph pass algebra stays backend-agnostic; the CPU 2D-Skia fallback stays the today-shipping floor.

[T-REALITY-CAPTURE] [QUEUED] — realitycapture sub-domain with splat and point-cloud sources
- Author `realitycapture/reality-capture.md`: a `SplatSource` (SOG/PLY ellipsoid set) and `PointCloudSource` projected as new viewport `RenderPass` cases off a Compute point/splat payload, plus a LiDAR-anchored measurable overlay and a capture-frame playback row.
- Integrate the radix-sorted alpha-composited 3DGS ellipsoid rasterization path through the `T-BACKEND-PORT` `GpuBackend` factory; AppUi consumes the decoded point and splat payload at the wire and never decodes LAZ itself — the offline LAZ/scan decode is the Python companion's geometry producer crossing as a Compute payload, no AppUi scan-decode package.
- Wires to `viewport/viewport-pipeline.md` as new `RenderPass` cases and align to the Compute point/splat payload contract at the package edge; the measurable overlay binds the AppUi `Viewpoint` and the `animation/animation-timeline.md` scrub, never a parallel scene owner.
- Consider that 3DGS rasterization is a distinct render path from triangle meshlets, demanding its own residency keying and pass case; depends on `T-BACKEND-PORT` so the splat rasterizer binds a backend target factory rather than hard-coding a second `GRContext` substrate.

[T-WEB-VIEWPORT-WIRE] [QUEUED] — full web-render seam for the TS branch
- Deepen the viewport TS_PROJECTION cluster into a portable scene-graph plus meshlet/splat residency manifest the TypeScript branch consumes to drive a WebGPU viewport, sharing one geometry residency and viewpoint contract.
- Integrate the WebGPU backend reach through the `GpuBackend` port; the residency manifest serializes through the settled suite wire law.
- Wires to the cross-language wire only — the manifest and `Viewpoint`/`FrameReceipt` contract cross to the TS web platform through the settled suite wire law; no desktop owner reverses onto the web leg.
- Consider that the manifest references the same Compute `GeometryPayload` residency the desktop reads, so the residency keying stays one owner across the wire; depends on `T-BACKEND-PORT` (WebGPU backend row) and `T-REALITY-CAPTURE` (splat residency keying) before the wire is complete.

[T-COORDINATION-BOARD] [QUEUED] — openBIM issue board in a coordination sub-domain
- Author `coordination/issue-board.md`: an `IssueBoard` composing each issue from the AppUi `Viewpoint` view-state, a `Rasm.Bim` BCF topic/component model consumed at the boundary, a CRDT-op-log comment thread, and a snapshot tile; the board owns the UI projection and the issue-to-viewpoint binding, never a BCF semantic schema.
- Integrate no new AppUi package — the `Viewpoint` codec, the notebook CRDT op-log, and the dashboard tiles are admitted; the BCF topic/component/comment serialization is consumed from the `Rasm.Bim` openBIM owner, never re-minted in the app-platform leaf.
- Wires align to named boundaries: `viewport/viewport-pipeline.md#VIEWPOINT_CODEC` (view-state receipt), `notebook/notebook-document.md#CRDT_COEDIT` (comment thread), the charts dashboard tiles (issue list), and the `Rasm.Bim` BCF topic contract at the package edge; the round-trip persists through the Persistence op-log changefeed already owned, never a coupling into a sibling interior.
- Consider that the BCF topic/component schema is openBIM exchange semantics owned by `Rasm.Bim`; AppUi composes that contract plus its own `Viewpoint` and CRDT owners into the board, so a second BCF model or a direct BCF-XML writer inside `coordination/` is the rejected form.

[T-PERF-GOVERNOR] [QUEUED] — declarative PerfBudget quality governor
- Promote the per-owner frame/VRAM/layout-elapsed instruments into one `PerfBudget` governor in `evidence/diagnostics-evidence.md` that folds telemetry into a quality policy degrading render passes, residency watermark, and motion tokens together.
- Integrate no new package; the governor reads the existing evidence receipt envelopes and the `FrameBudget`/`ResidencyBudget` instruments.
- Wires internal to `evidence/` and align to named sibling clusters: `viewport/viewport-pipeline.md#RENDER_GRAPH` and `#RESIDENCY_BUDGET` (pass degrade, residency evict), `motion/motion-tokens.md#REDUCED_MOTION` (token reduce), and the `evidence/diagnostics-evidence.md#DEV_LOOP` HUD; the governor consumes evidence and emits one quality verdict, never a second meter.
- Consider that adaptive quality is one feedback owner over a perf budget; the governor must degrade deterministically so render-hash lanes stay attributable under a budget breach.

[T-EMBED-PROBE] [BLOCKED] — live host-shared embedding and GPU lease
- Resolve the `hosts/surface-hosts.md` and `viewport/viewport-pipeline.md` RESEARCH rows under a running integrated host: Avalonia-in-Rhino NSView embedding (pump coexistence, resize sync, render-backend contention), the seam `OnUiThread` access-assertion under the Rhino-owned AppKit run-loop, and the host-shared `GRContext` lease through `ISkiaSharpApiLease.TryLeasePlatformGraphicsApi` for meshlet/path-trace emit.
- Integrate the SkiaSharp GPU backend-context (`GRMtlBackendContext`/`GRVkBackendContext`) and `SKRuntimeEffect` over the leased context bound through one `SurfaceSeam` delegate column.
- Wires bind only at the app root that composes a live host; AppUi references the abstract seam columns, never a host API.
- Blocked on the live RhinoWIP host surface and the host-owned GPU pipeline; the CPU/2D-Skia fallback ships today, de-risked standalone against a windowed `GRContext`.

[T-INPUT-FABRIC-SDKS] [BLOCKED] — alternative-input device SDK spellings
- Re-ground the `input/input-interaction.md` `InputDevice`/`DeviceOutput` per-device member spellings — SpaceMouse HID, gamepad API, gaze SDK, switch-access scan, speech recognition, MIDI, and CNC/robot/haptic transport — and the `localization/#RTL_MIRRORING` `LiveCaption.Translated` recognizer/translator spellings.
- Integrate each device SDK and the speech-and-translation package as `SurfaceSeam` delegate columns bound at composition.
- Wires fold every device sample onto the one `CommandIntent` table and symmetric output over normalized `DeviceAxis`; SDKs cross only as composition delegates.
- Blocked on the host-bound device/recognizer SDKs; the fabric vocabulary and fold are fence-complete now.

[T-EXPORT-WRITERS] [BLOCKED] — Office and DWG/DXF emit writer sets
- Re-ground the `visuals/#DOCUMENT_EXPORT` `OfficeExport` OOXML part-graph member set and the `drafting/#DRAFT_EMIT` DWG/DXF model-space entity-writer set; the PDF/SVG arms are settled on the `SKDocument`/`SKSvgCanvas` rail.
- Integrate DocumentFormat.OpenXml (XLSX/PPTX/DOCX), and ACadSharp or netDxf for the DWG/DXF arm, reusing the `FlowFold` break algebra for deterministic pagination.
- Wires deliver through the one `VisualDestination`; the writer pins land at admission, never a per-format export surface.
- Blocked on admitting the Office and CAD writer packages into the manifest; the export flow algebra is fence-complete.

[T-HEADLESS-PROOF] [BLOCKED] — headless render-hash and journal-replay lanes
- Admit the `Rasm.AppUi.Tests` node on the test rail so the `evidence/#HEADLESS_DERIVATION` proof matrix, render-hash lanes, and command-journal replay run under virtual time.
- Integrate Avalonia.Headless, Avalonia.Headless.XUnit, and Verify.XunitV3 for deterministic frame capture and `FrameHash` equality.
- Wires the headless `SurfaceHost.Headless` row through the deterministic `ForceRenderTimerTick` frames and the virtual `TimeProvider` from the test composition.
- Blocked on the test project node and the Verify catalogue; unblocks every spec and render-hash lane.

## [2]-[CLOSED]

(none)
