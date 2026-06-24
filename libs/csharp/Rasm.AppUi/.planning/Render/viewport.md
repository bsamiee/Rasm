# [APPUI_VIEWPORT_SPLIT]

The 750-line viewport god-page is split into three owners authored at their true granularity, capability conserved with no behavior change:

- [PIPELINE](pipeline.md) — `RenderGraph` pass-DAG, the per-backend `RenderTargetFactory`, the `ResolvePass`/`ResolvePolicy`/`ResolveState` ladder, the `SimVisual` field render passes, the `Viewpoint` BCF receipt, and the `ResidencyManifest` web-residency wire projection.
- [MESHLETS](meshlets.md) — `MeshletCluster` GPU-driven cluster-LOD with `BindlessTable` residency, and `ResidencyBudget` VRAM-budget residency with predictive prefetch and `InstanceBuffer` massive instancing.
- [PATHTRACE](pathtrace.md) — `Bvh` build/refit, the `Reservoir` ReSTIR sampler, the `PathTracePass` progressive accumulation and `Denoiser`, and the `BsdfShading` integrator shading FROM the `Rasm.Materials/Appearance` `LayeredBsdf`/`SlabStack`/`SurfaceShade` at the `PATH_TRACE` seam.

The three pages compose one viewport: `Render/pipeline` drives the frame and draws the `Render/meshlets` cluster and the `Render/pathtrace` integrator, the BVH builds over the meshlet bounds, and the residency set feeds the `ResidencyManifest` mint. The `Render/shading` shader-asset owner and the `Render/immersive` OpenXR surface mount beside this split. No `viewport.cs` node survives — the codemap carries `Pipeline.cs`, `Meshlets.cs`, and `PathTrace.cs`.
