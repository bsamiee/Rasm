# [APPUI_TASKLOG]

Open and closed work for the product UI engine, distilled from the ideas in `IDEAS.md`. Each open card carries a status marker — `[QUEUED]`, `[ACTIVE]`, or `[BLOCKED]` — plus the capability or file to build, the external packages to integrate, the integration points and boundaries or wires, and the key considerations. Live-host probes and tool-gated arms are tracked as blocked tasks against the substrate that unblocks them.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with. `Atomic` flags a minor-scope task so a later session sizes its turn correctly and does not overscope a batch of small items.

## [01]-[OPEN]

<!-- source-only: open idea card template:
[ID]-[STATUS]: <ambitious concise thesis>.
- Capability: <higher-order concept, invariant, or owner capability>.
- Shape: <what the idea becomes as a system, product, owner, or feature set(s)>.
- Unlocks: <new branch, package, workflow, proof, user, or agent capability made possible>.
- Anchors: <owners, seams, packages, doctrines, or techniques that make the idea plausible>.
- Tension: <only when an unresolved constraint, boundary, bet, or dependency shapes the idea>.
- Ripple: <origin/counterpart card this entry pairs with across folders, as `pkg` `[SLUG]`; present only on a cross-folder ripple counterpart card>.
- Atomic: <present only on a minor-scope task; one short phrase naming the small unit so a later session does not overscope its turn>.
-->

[EMBED-PROBE]-[BLOCKED]: live Rhino embedding proves the shared AppUi GPU seam.
- Capability: live-host validation for `SurfaceHost.RhinoPanel`, `EmbedCapsule`, `SurfaceScheduler.Affinity`, and `RenderGraph.Lease` against the Rhino-owned AppKit run-loop and GPU pipeline.
- Shape: one `SurfaceSeam`-bound GPU lease carries `ISkiaSharpApiLease.TryLeasePlatformGraphicsApi`, `GRMtlBackendContext`/`GRVkBackendContext`, and `SKRuntimeEffect` into the `GpuBackend` `RenderTargetFactory` rows without per-host render arms.
- Unlocks: meshlet, path-trace, live `Taa`/`Fsr`, and embedded-panel frame-receipt proof while preserving the CPU/2D-Skia fallback already de-risked against a windowed `GRContext`.
- Anchors: `.planning/Shell/hosts.md#07research`, `.planning/Render/viewport.md#09research`, `.api/api-avalonia-skia.md`, `.api/api-skiasharp.md`, `.api/api-avalonia-gpu-interop.md`, `SurfaceSeam.OnUiThread`, `GpuBackend`, and `RenderTargetFactory`.
- Tension: no integrated running RhinoWIP host surface exists yet, so pump coexistence, resize sync, render-backend contention, and host-owned shared-context spelling remain blocked.

[INPUT-FABRIC-SDKS]-[BLOCKED]: composition-bound device SDKs complete the alternative-input fabric.
- Capability: per-device SDK admission for `InputDevice`, `DeviceOutput`, and `CaptionSource.Translated` delegate spellings across SpaceMouse, gamepad, gaze, switch access, speech, MIDI, CNC, robot, haptic, recognizer, and translation surfaces.
- Shape: each device or speech package lands as a `SurfaceSeam` or composition delegate column feeding `InputFabric.Map`, `InputFabric.Drive`, or `LiveCaption.Stream`; samples normalize to `DeviceAxis`, and commands fold through the single `CommandIntent` table.
- Unlocks: alternative input and device output, voice command routing, localized live captions, and haptic/robot/CNC feedback without per-device handlers or a second command grammar.
- Anchors: `.planning/Shell/input.md#07research`, `.planning/Theme/locale.md#06research`, `.planning/Shell/commands.md`, `InputDevice`, `DeviceOutput`, `InputFabric`, `DeviceAxis`, `CommandIntent`, and `CaptionSource.Translated`.
- Tension: host-bound device, recognizer, translation, MIDI, CNC, robot, and haptic package admissions are absent; vocabulary and folds are fence-complete, but member spellings remain unverified.

[RENDER_LAYEREDBSDF_PATHTRACE]-[QUEUED]: Rasm.AppUi/Render owns the path tracer that shades FROM the LayeredBsdf.Sample/Evaluate/Pdf, the SlabStack lowering, and the SurfaceShade graph sink, never re-deriving lobe math — the renderer boundary the bsdf and graph pages name as the downstream consumer at the PATH_TRACE seam.
- Capability: The app-platform path-tracer consuming the one LayeredBsdf the SlabStack.ToLayered produces (post-split surface#OPENPBR_SLAB) and the SurfaceShade the MaterialGraph.Evaluate sink assembles, driving the per-bounce world ray through ShadingFrame.ToWorld and the MIS-balanced lobe sample, with the position-free multi-scatter random walk as the high-fidelity path over the Kulla-Conty fast path, and the SPECTRAL_REFLECTANCE_GROUNDING per-wavelength conductor curve as the high-fidelity conductor path.
- Shape: A Rasm.AppUi/Render integrator reading the Materials LayeredBsdf/SlabStack/SurfaceShade at the wire/runtime boundary and shading the scene, the consumer end of the Appearance/bsdf and Appearance/graph -> Render boundary seams.
- Unlocks: The renderer shades every material as a weighting of the closed seven-lobe set with zero per-material code, the OpenPBR slab stack lowers to one LayeredBsdf the integrator reads, and the position-free multi-scatter route admits as the high-fidelity path.
- Anchors: bsdf.md the renderer shades from LayeredBsdf and never re-derives lobe math (line 3), the path-tracer at the Rasm.AppUi/Render/viewport#PATH_TRACE seam; graph.md the SurfaceShade sink shaded by the path tracer; ARCHITECTURE.md [02]-[SEAMS] the bsdf/graph -> Render boundary (lines 51-52).
- Ripple: counterpart of `Rasm.Materials` `[SPECTRAL_REFLECTANCE_GROUNDING]` idea + `[BSDF_PAGE_SPLIT]` task (path tracer shades from `LayeredBsdf`/`SurfaceShade`).

[APPUI_VIEWPORT_SILHOUETTE_CONSUMPTION]-[QUEUED]: Confirm the AppUi Render drafting Viewport2D consumes the Fabrication HiddenLineResult silhouette edge set as a first-class style class once the CSG watertight-silhouette arm lands, so the two-sided consumption seam carries the silhouette set the projection page produces and the drafting page already tags as an emphasized EdgeStyle.
- Capability: AppUi Render/drafting Viewport2D composes the Fabrication Posting/projection HiddenLineSeam delegate running Hlr.Solve and reads the world-space (Visible, Hidden, Silhouette) Edge3 sets (owner.md FabricationResult.HiddenLineResult line 88), tagging each with an EdgeStyle; the drafting page already declares the silhouette-emphasized style. The ripple confirms the consumption contract for the silhouette set the CSG arm deepens: when the CSG_WATERTIGHT_SILHOUETTE arm composes the kernel arrangement to produce exact watertight-solid outlines, the Viewport2D silhouette-emphasized style consumes that improved set with no AppUi change, the seam staying a pure consumption boundary.
- Shape: A Rasm.AppUi Render/drafting.md page note confirming the Viewport2D HiddenLineSeam consumes the Silhouette Edge3 set as the emphasized EdgeStyle and that the Fabrication CSG silhouette deepening flows through the same delegate with no AppUi-side change, reconciling the drafting page's stated EdgeStyle tagging against the projection page's three-set output.
- Unlocks: A confirmed consumption contract so the Fabrication CSG silhouette improvement (exact arrangement outlines vs per-facet silhouettes) is read by the AppUi viewport with zero coupling change.
- Anchors: csharp:Rasm.AppUi/Render/drafting#PROJECTION Viewport2D/HiddenLineSeam/EdgeStyle; Fabrication/Posting/projection#PROJECTION_HIDDEN_LINE the (Visible/Hidden/Silhouette) sets (FabricationResult.HiddenLineResult owner.md line 88); the CSG silhouette arm; ARRANGEMENT_REALIZED_OWNER_LANDING upstream gate
- Tension: The CSG silhouette arm is forward-blocked on the realized ARRANGEMENT kernel .cs owner — the AppUi consumption contract confirms the seam shape but the silhouette-set deepening it consumes does not land until the kernel arrangement owner is implemented, so the ripple states the consumption contract as forward-ready, not active.
- Ripple: counterpart of `Rasm.Fabrication` Posting `projection` `HiddenLineResult` + `[CSG_SILHOUETTE]` idea (Viewport2D consumes the silhouette edge set).

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

(none)
