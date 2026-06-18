# [APPUI_IDEAS]

The forward concept pool for the product UI engine. Open ideas are higher-order folder concepts grounded in the AEC/Rhino purpose and current rendering, reality-capture, and coordination research; each drives one or more task cards in `TASKLOG.md`. A finished or dropped idea moves to closed with a one-line disposition so it is never re-litigated.

## [1]-[OPEN]

[GPU_BACKEND_PORT]:
- The existing `GpuBackend` SmartEnum carries only host-graphics families (Metal, Vulkan, OpenGl, Software); it grows a `RenderTarget` factory column per backend and admits a Vello/wgpu compute path and a Skia-Graphite row so the `Lease` and pass-emit bodies bind a backend-provided target factory rather than the single `GRContext`-plus-`SKRuntimeEffect` emit path the render graph hard-codes today.
- Unlocks a Vello-accelerated 2D dashboard path and a WebGPU projection without re-authoring any `RenderPass` or `CustomVisual` fold, and decouples the render-graph algebra from SkiaSharp's Ganesh-to-Graphite transition so a substrate swap is one backend row.
- Draws on VelloSharp's .NET wgpu bindings (DX12/Metal/Vulkan/OpenGL/WebGPU) with a SkiaSharp-compat shim; the render graph already discriminates passes on `GpuBackend` but every emit body assumes the leased `GRContext` and an `SKRuntimeEffect` shader, so a compute-rasterizer backend has no target-construction seam to bind.

[SPLAT_REALITY_CAPTURE]:
- A `realitycapture/` sub-domain adds a Gaussian-splat and point-cloud render source: a `SplatSource` (SOG/PLY ellipsoid set) and `PointCloudSource` (LAZ) projected as new viewport `RenderPass` cases off Compute point/splat payloads, with a LiDAR-anchored measurable overlay and capture-frame playback.
- Unlocks photoreal existing-conditions visualization navigable beside BIM geometry — the dominant 2025-2026 AEC reality-capture modality — measurable against the BCF `Viewpoint` and scrubbable through the animation timeline.
- Draws on radix-sorted alpha-composited 3DGS ellipsoid rasterization, a distinct render path from triangle meshlets; the viewport triad (meshlet/path-trace/sim-field) has no representation for splat ellipsoids or massive point clouds, now the primary scan-to-BIM deliverable.

[WEB_VIEWPORT_WIRE]:
- The viewport TS_PROJECTION deepens beyond the viewpoint/frame-receipt wire into a full web-render seam: a portable scene-graph plus meshlet/splat residency manifest the TypeScript branch consumes to drive a WebGPU viewport, so desktop and web share one geometry residency and viewpoint contract.
- Unlocks a browser viewport rendering the same virtualized scene the desktop does, off the same Compute `GeometryPayload`/residency manifest, closing the cross-language wire for 3D the architecture reserves for the TS web platform.
- Draws on WebGPU compute now being stable enough for in-browser cluster-LOD; the current wire crosses only the `Viewpoint` and `FrameReceipt`, leaving the meshlet cluster, residency plan, and splat sources desktop-local.

[COORDINATION_ISSUE_BOARD]:
- A `coordination/` sub-domain: an openBIM issue board where each issue composes the AppUi `Viewpoint` view-state, the `Rasm.Bim`-owned BCF topic/component/comment model consumed at the boundary, a CRDT op-log comment thread, and a snapshot tile — a first-class coordination UI surface that owns the board, never the BCF semantic schema.
- Unlocks in-app clash and issue coordination by composing existing owners: the `Viewpoint` codec, the notebook CRDT co-edit log, and the dashboard tiles, with zero new persistence owner and zero second BCF model in the app-platform leaf.
- Draws on BCF 3.0 carrying topics, components, comments, and viewpoints as one coordination unit; AppUi owns the `Viewpoint` receipt and the board projection while `Rasm.Bim` owns the openBIM topic/component exchange semantics, the two meeting only at the topic contract.

[PERF_BUDGET_GOVERNOR]:
- The scattered per-owner frame/VRAM/layout-elapsed instruments promote into one declarative `PerfBudget` governor that reads the `FrameBudget`/`ResidencyBudget`/layout instruments and degrades render passes, residency watermark, and motion tokens as one policy fold when a budget is breached.
- Unlocks a single adaptive-quality control surface — degrade path-trace samples, drop LOD, reduce motion, evict residency — driven by live telemetry, so the viewport holds frame budget on weak hardware without per-pass ad-hoc throttling.
- Draws on modern real-time renderers treating adaptive quality as one feedback owner over a perf budget; frame budget, VRAM watermark, and layout-elapsed are each enforced locally today with no cross-owner governor turning the evidence telemetry back into a quality policy.

[NOTEBOOK_REPRODUCIBILITY_TO_DETERMINISM]:
- The `notebook/notebook-document.md#REPLAY_BUNDLE` `Verify` re-derives a per-cell output-hash comparison locally and routes journal replay through `evidence/diagnostics-evidence.md#HEADLESS_DERIVATION` `ProofEngine.Replay`, while the AppHost `determinism-and-replay` owner already legislates reproducibility-proof: `DeterminismKernel.Reproduces` (environment-fingerprint match), `EventLog`/`ContentHash` (the content-addressed chain), and `ReplayVerify.Replay` (re-run a recorded log, prove each step's content hash). The notebook reproducibility model is a second spelling of the same content-hash-identity law.
- Realizes as: the `CapabilityPin` composes `DeterminismContext`/`EnvFingerprint` as its environment identity rather than a notebook-local checksum tuple, the `ReplayBundle.Verify` output-hash comparison composes `EventLog`/`ReplayVerify` content-hash identity (the `XxHash128` suite identity row both already cite) instead of a local `hash(output)` fold, and the cell DAG `RecomputePlan` aligns to the AppHost `RecomputeGraph` content-address node identity — one reproducibility-proof owner, the notebook a UI projection over it. The cross-language wire that carries the notebook reproducibility receipt then rides the existing `ReceiptEnvelopeWire`/`LogEntryWire` rather than a notebook-only shape.
- Draws on the AppHost determinism kernel being the suite's single reproducibility owner (`HostFingerprint`/content-hash/`ReplayVerify`); a UI computational document is the natural downstream consumer of that proof, not a parallel re-derivation, so the notebook reproducibility law extends the AppHost owner and lands as a composed pin rather than a second hash-comparison fold.

## [2]-[CLOSED]

(none)
