# [APPUI_RENDER_PIPELINE]

`RenderGraph` is the infinite viewport's GPU render pipeline: one pass-DAG drives every frame over a host-shared `GRContext` leased through the embed capsule, the `GpuBackend` rows carry the per-backend target-construction delegate (`Target`) over the composition-bound `GpuBinding` union so backend identity derives from the binding and a mismatched backend-factory pair is unrepresentable, the `ResolvePass` ladder selects the antialias-and-super-resolution resolve, `SimVisual` renders isosurface/volume/streamline/glyph/deformation fields off the Compute field receipts, and `Viewpoint` codecs camera, section, visibility, override, selection, and source-addressed measurements as one portable BCF-compatible receipt. This page owns the render-graph pass algebra, the backend vocabulary with its target factory column, the measured GPU-time evidence lane, the resolve ladder, the simulation render passes, the viewpoint receipt with its visibility-action and version-ghost folds, and the residency wire projection; the geometry-virtualization and residency owners live in `Render/meshlets`, the path-trace integrator in `Render/pathtrace`. Its substrate is SkiaSharp 3 GPU backends (`GRContext`, `GRMtlBackendContext`, `GRVkBackendContext`, `SKRuntimeEffect`) leased through `ISkiaSharpApiLease`, the `Silk.NET.WebGPU` wgpu/Dawn target factory, the Compute geometry and field receipts, and the AppHost clock, frame-budget, and receipt-sink ports. GPU passes share the host GPU context, and the `Software` 2D-Skia raster is the deterministic CPU floor.

## [01]-[INDEX]

- [02]-[RENDER_GRAPH]: Frame pass-DAG, the `GpuBackend` target-factory column over `GpuBinding`, resolve ladder, frame-budget invariant, fallback.
- [03]-[SIM_VISUAL]: Isosurface, volume, streamline, glyph, deformation field render passes off the Compute receipts.
- [04]-[VIEWPOINT_CODEC]: Camera, section-box, visibility, override, selection projecting onto the `Rasm.Bim` `BcfViewpoint` exchange contract; the isolate/hide/x-ray/reset action fold and the version-diff ghost projection on the one override vocabulary.
- [05]-[TS_PROJECTION]: Viewpoint, frame-evidence, and content-keyed geometry-residency wire contract.
- [06]-[GPU_AND_WIRE_BOUNDARY]: Viewport GPU lease law, the wgpu presentation arms, and the web residency mint.

## [02]-[RENDER_GRAPH]

- Owner: `RenderPass` `[Union]` frame-pass vocabulary; `RenderGraph` pass-DAG executor; `GpuBackend` `[SmartEnum]` the backend vocabulary whose rows CARRY the target-construction delegate; `GpuBinding` `[Union]` the composition-bound substrate each backend row folds; `RenderTargetRequest` the resolve-row-derived allocation request (display extent, render scale, sample count); `RenderTarget` the lease-bound GPU surface carrying its request and its GRANTED sample count; `FrameView` the render-time view value pairing the view-state camera, this frame's NDC jitter, and the governor verdict; `WgpuFrameEvidence` the timestamp-query GPU-time lane; `FrameReceipt` per-frame evidence; `ViewportFault` the fault family; `ResolvePass` `[SmartEnum]` the antialias-and-super-resolution resolve ladder the `Composite` pass selects; `ResolvePolicy` the per-tier delegate-row binding.
- Cases: `RenderPass` = Cull | Geometry | PathTrace | Composite | Sim | Overlay under the locked kind literals cull, geometry, path-trace, composite, sim, overlay; `ResolvePass` = Msaa | Taa | Fsr | Smaa under the locked policy literals; `ViewportFault` = Text | ContextUnavailable | BackendUnsupported | BudgetExceeded | LeaseRejected — codes derive through the `AppUiFaultBand.Viewport` registry row (6100), shared with pathtrace.
- Entry: `public IO<FrameReceipt> Frame(ViewportClock clock, FrameBudget budget, QualityVerdict quality, ViewCamera camera)` on `RenderGraph` — `IO` rail; the pass-DAG executes topologically under the frame camera and the ONE governor verdict, whose `Tier.Rank` selects the resolve row, whose `PassMask` filters the DAG (the `Diagnostics/governor.md` `QualityTier.PassMask` column, so a degraded tier's pass set is data, never a caller convention), and whose `LodPixelScale` is the cut's own error multiplier — three facts one authority derives, so a rank and a mask arriving as separate arguments is the deleted form; the frame seals one receipt carrying the per-pass elapsed, the deferred-pass set, and the GPU-time fold.
- Auto: `Lease` opens the host-shared GPU context through `ISkiaSharpApiLease.TryLeasePlatformGraphicsApi` and folds the leased context to the `RenderTarget` through the bound `GpuBinding`'s own backend row (`binding.Backend.Target(binding, request)` — the `[UseDelegateFromConstructor]` factory column each row was constructed with), so a pass-emit body binds a backend-provided target rather than the single `GRContext`-plus-`SKRuntimeEffect` emit path and the embedded viewport composites into the Rhino-owned context and never mints a second `GRContext`; composition owns the DISPLAY extent alone and hands it to the `RenderTargetRequest` builder the graph threads in, so the render extent and the sample count derive from the resolve row the tier selected and no caller can allocate a target the ladder did not ask for; when the platform lease yields no GPU context (`LeaseRejected`/`ContextUnavailable`) the frame re-runs through the `Fallback` binding — `GpuBinding.Raster`, the `Software` row's CPU 2D-Skia floor with the pass list filtered to `Composite`/`Overlay` — so the viewport renders a deterministic CPU frame through the same fold and receipt; the frame-budget invariant executes inside the pass fold — a pass starting past `FrameBudget.Frame`, or whose own declared `Charge` carries the fold past `MaxTriangles`, DEFERS to the next frame (recorded in the receipt's `Deferred` column, folded onto the budget-overrun instrument), and `WithinBudget` derives from the measured elapsed and the deferral set, never an initialized-true flag.
- Law: each `GpuBackend` row is CONSTRUCTED with its target-construction delegate, and every delegate takes the frame's `RenderTargetRequest` so the sample count and the render extent reach the allocation rather than the resolve row alone — `Metal`, `Vulkan`, and `OpenGl` fold the `GpuBinding.Ganesh` leased `GRContext` to `SKSurface.Create(GRRecordingContext, budgeted, request.Info, request.Samples, GRSurfaceOrigin)` for an offscreen target, or wrap the host framebuffer as a `GRBackendRenderTarget` and read `SampleCount` back as the granted column, `Software` folds `GpuBinding.Raster` to the CPU `SKSurface.Create(SKImageInfo)` floor, which takes no sample count and therefore grants one, `Wgpu` folds `GpuBinding.Wgpu` — whose target texture carries `TextureDescriptor.SampleCount` and whose pipeline multisample state must match it — the `Silk.NET.WebGPU` wgpu/Dawn substrate (D3D12/Metal/Vulkan auto-negotiated through `BackendType`) whose `Adapter` matched the compositor adapter LUID/UUID at composition, its `Device`+`Queue` shared branch-wide, DISCRIMINATING the presentation arm on the binding's `WgpuPresentation` — the in-tree composited viewport IMPORTS the rendered texture through the compositor interop family (`ICompositionGpuInterop.ImportImage`/`ImportSemaphore` then `CompositionDrawingSurface.UpdateWithKeyedMutexAsync`/`UpdateWithSemaphoresAsync`/`UpdateWithTimelineSemaphoresAsync` per `GetSynchronizationCapabilities`; a second swapchain in composited mode is the DELETED form), while `SurfaceConfigure`/`SurfaceGetCurrentTexture` survives ONLY as the exclusive-fullscreen/headless arm — the wgpu mesh-shader/compute passes record through `CommandEncoder`/`RenderPassEncoder` and submit through `QueueSubmit`, never a managed scene wrapper — and `WebGpu` folds `GpuBinding.Browser`, the in-browser WebGPU surface the TS web leg consumes; `GpuBinding.Backend` DERIVES the backend row from the binding case, so `RenderGraph` holds bindings alone, a backend paired with a foreign substrate cannot be constructed, and a substrate swap is one backend row with its binding case; the per-backend emit path (wgpu pipeline submit versus `SKRuntimeEffect` shader) diverges inside the row delegates, so the vocabulary owns the divergence and the CPU 2D-Skia fallback is the floor.
- Law: the `Composite` pass selects one `ResolvePass` policy row after the geometry and path-trace passes — `Taa` jitters the camera sub-pixel per frame and reprojects the prior frame through the motion-vector buffer under a neighborhood-clamp history rejection so a static scene converges and a moving scene ghosts no tail, `Smaa` runs the morphological edge AA, `Msaa` multi-samples the raster, and `Fsr` renders sub-resolution (`RenderScale` 0.6) under the `Render/meshlets` `ResidencyBudget` VRAM bound and spatially upscales to display resolution so a 4K viewport renders at a fraction of the pixel cost; `ResolvePolicy` binds each `PERF_BUDGET` `QualityTier` rank to its `ResolvePass` through the frozen `int -> ResolvePass` table (`ByTier`, ranks 4..0) so the governor steps the full ladder `Taa(4,3) -> Smaa(2) -> Msaa(1) -> Fsr(0)` on the same hysteresis band that degrades the render passes — the high tiers spend pixels on temporal quality and the floor tier trades resolution for budget; the ladder EXECUTES ahead of the lease: `Policy.For(quality.Tier.Rank)` selects the pass and `ResolvePass.Advance` steps the graph-held `ResolveState` (ordinal, Halton jitter, history, render scale, frame camera) once per frame — a camera that moved since the prior frame resets the history and the path-trace film in the same transition.
- Law: each of the row's three resolve columns reaches the surface it governs, and a column no allocation or pass consumes is the deleted form. `RenderScale` and `Samples` mint the frame's `RenderTargetRequest`, so an `Fsr` frame allocates at `round(display * 0.6)` and an `Msaa` frame asks its backend for four samples, while `RenderTarget.Samples` publishes what the allocation GRANTED — the raster floor grants one and says so. `Jitter` becomes a CAMERA fact: `FrameView.Of` converts the signed sub-pixel offset to NDC against the target the frame actually allocated, the `Cull` and `Geometry` arms take that `FrameView` exactly as the cull arm already took the frame camera, and the geometry draw adds `NdcJitter` to its projection's third column. Cull and LOD read `FrameView.Camera` and `FrameView.LodScale` — the governor's own degrade lever, which a composition-closed `lodScale` left with no reader, because a sub-pixel offset moves no cull decision; the `PathTrace` arm reads `Camera` too, and that is load-bearing — a jittered lens differs from the prior frame's every frame, so `AccumulationTarget.Reset` would fire on every frame and the film would never converge past one sample. The `Composite` arm runs `ResolvePass.Resolve(target, state, raster)`, which hands the composite the target's own `RenderTargetRequest` beside the jitter-and-history state, so the upscale factor it must undo is read off the surface it reads from rather than assumed. The `Taa` motion-vector buffer is ONE `Render/meshlets` `BindlessTable` slot, never a parallel motion-vector owner; the resolve is a `Composite` policy column and a parallel post-process engine is the deleted form.
- Law: the triangle column is a MEASURED draw count over one contract, and the contract is the frame's CUT. `RenderPass.Geometry` carries three members — `Phase`, the `Render/meshlets` `CutPhase` row naming which slice of the cut this row draws; `Charge`, the budget projection the pre-charge gate reads against that slice; and `Draw`, which returns the triangles the pass recorded — and every other case contributes zero to the fold, so a cull selection, a path-trace batch, a sim path sweep, a composite, and an overlay each publish the honest nothing they drew. The cut itself rides the pass fold: the `Cull` arm publishes the `CullResult` it produced, the geometry arms read it, and a geometry pass scheduled ahead of any cull reads `CullResult.Empty` and draws nothing. `Render/meshlets`' draw takes `CutPhase.Prior` then `CutPhase.Retest` — the two-phase HZB ladder is two rows over one cut, and it is unschedulable when the second phase's view list never leaves the cull arm; the `Render/shading` shade mount and the `Render/reality` capture composites take `CutPhase.Whole`, charge zero, and report zero, re-shading and re-compositing geometry another pass already drew. Handing a geometry delegate the un-narrowed cluster set, or returning a cluster count, a path-point count, or the whole scene's total from either half, makes `FrameReceipt.Triangles` a fabricated measure and `budget.MaxTriangles` defer on cost nothing spends — all deleted forms.
- Law: `TelemetryRow(version, budget)` carries the `Diagnostics/evidence#TELEMETRY_SPINE` `ViewportObjectives.Pack` over those same three series.
- Receipt: `FrameReceipt` — frame ordinal, per-pass `Duration` seq, GPU `Duration`, the drawing passes' own reported triangles, budget verdict, `Instant`, `CorrelationId`; the GPU column is MEASURED evidence off the `WgpuFrameEvidence` timestamp lane (`QueryType.Timestamp` `DeviceCreateQuerySet`, per-pass `RenderPassTimestampWrites`/`ComputePassTimestampWrites`, `CommandEncoderResolveQuerySet` into the read buffer, `BufferMapAsync`/`BufferGetMappedRange`/`BufferUnmap` readback, `QuerySetRelease` teardown), never the CPU elapsed re-labelled — a binding without the `timestamp-query` feature binds `None` and the column carries the honest `Duration.Zero`, while a FAILED readback keeps the zero and lands its fault on the receipt fault rail so unsupported and failed never conflate; the `Diagnostics/governor.md` `GpuTimeline.Migrate` deepens the same column from the lane-measured frame duration to per-pass resolved nanoseconds only when EVERY pass resolved its timestamp pair — a mixed projected/measured sum never enters the measured column; frame retirement rides `QueueSubmitForIndex` minting the `WrappedSubmissionIndex` that `DevicePoll` advances without a blocking fence, so cull-to-draw and readback never stall the queue; sealed through `ReceiptSinkPort` as a `Render`-family fact; `TelemetryRow` contributes the frame-elapsed, gpu-elapsed, and budget-overrun instruments inward through `TelemetryContributorPort`, and `RenderGraph.Observe` is where the frame's accepted `Render/meshlets` `ResidencyPlan` retires too — it is the sole binder of `ResidencyBudget.Observe`, so the evict, prefetch, and pool gauges read the plan THIS frame drew rather than a plan some other frame held.
- Packages: SkiaSharp, Avalonia.Skia, Avalonia (compositor GPU interop), Silk.NET.WebGPU, Silk.NET.WebGPU.Extensions.WGPU, Silk.NET.WebGPU.Native.WGPU, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm (project — `Deterministic.RadicalInverse` the TAA jitter sequence), Rasm.AppHost (project)
- Growth: a new frame stage is one `RenderPass` case breaking the topological dispatch at compile time; a new resolve column is one `ResolvePass` column plus its read on `RenderTargetRequest.Of` or `FrameView.Of`; a new backend is one `GpuBackend` row constructed with its target delegate and its `GpuBinding` case — Skia Graphite re-admits as one `SkiaGraphite` row the moment SkiaSharp ships its Recorder/Context surface; a new backend FAMILY is one `GpuFamily` row declaring its `Skia`/`Chained`/`Accelerated` columns, and every consumer picks it up with no edit because none enumerates families; zero new surface.
- Growth: a new viewport reliability indicator is one `ViewportObjectives` row on the evidence page, carried here with no edit.
- Boundary: `RenderGraph` is the named boundary capsule — the lease open-and-dispose pair and the topological pass walk carry the only statement bodies; the shared GPU context arrives as one `SurfaceSeam`-bound platform-lease delegate so no pass body names a `GRContext.CreateMetal`/`CreateVulkan` factory at a call site, deferring to the surface-hosts `EMBED_CAPSULE` shared-context law — a direct GPU-backend construction inside a pass arm is the rejected form (PROHIBITION host-API-in-arm); the per-backend target construction LIVES ON the `GpuBackend` row as its constructor delegate and every row reads the one `RenderTargetRequest` — the `Metal`/`Vulkan`/`OpenGl` rows fold the leased `GRContext` to a sample-counted `SKSurface.Create`, the `Wgpu` row folds the `Silk.NET.WebGPU` `Device`/`Queue`/`Surface` wgpu swapchain presenting through the compositor `ICompositionGpuInterop.ImportImage` seam, and the `WebGpu` row folds the browser surface — a detached factory record pairable with a foreign backend is the deleted form, so a pass-emit body never names a backend target factory at a call site and a substrate swap is one backend row; the GPU passes (`Geometry` cluster draw through the wgpu mesh-shader pipeline, `PathTrace` through the wgpu compute pass, `Sim` volume ray-march, the reality-capture `Splat`/`Point` composites) SPIKE-gate on the live host-shared context and the `Composite` 2D-Skia raster is the deterministic CPU fallback; `ViewportClock` rides the AppHost `ClockPolicy` so frame timing is the one clock seam and a stopwatch is the rejected form; the frame ordinal is a monotone `Interlocked.Increment` over the graph-local counter so each `FrameReceipt` carries a distinct ordinal the correlation join and the render-hash lane key on, and a hardcoded zero ordinal is the deleted form; the receipt carries the folded per-pass list, the deferred-pass set, and a `WithinBudget` verdict derived from the measured elapsed against `FrameBudget.Frame` and the triangle ceiling, so an overrun frame seals `WithinBudget: false` with its deferrals named rather than an unconditional true, and every frame sinks one `FrameReceipt` through the one envelope and a per-pass meter is the deleted form; GPU validation on the `Wgpu` arm rides the error-scope rail — `DeviceSetUncapturedErrorCallback` installs once at device acquisition and `WgpuErrorScope` brackets suspect pass encoding through `DevicePushErrorScope`/`DevicePopErrorScope`, so a validation or out-of-memory error is a counted `ViewportFault` on the telemetry spine, never a swallowed native abort; the meshlet cluster the graph draws is the `Render/meshlets` owner and the path-trace pass the `Render/pathtrace` integrator, so the pipeline composes them and re-models neither.

```csharp signature
[Union]
public abstract partial record ViewportFault : Expected, IValidationError<ViewportFault> {
    private ViewportFault(string detail, int code) : base(detail, code, None) { }

    public static ViewportFault Create(string message) => new Text(message);

    public sealed record Text : ViewportFault { public Text(string detail) : base(detail, AppUiFaultBand.Viewport.Code(0)) { } }
    public sealed record ContextUnavailable : ViewportFault { public ContextUnavailable(string detail) : base(detail, AppUiFaultBand.Viewport.Code(1)) { } }
    public sealed record BackendUnsupported : ViewportFault { public BackendUnsupported(string detail) : base(detail, AppUiFaultBand.Viewport.Code(2)) { } }
    public sealed record BudgetExceeded : ViewportFault { public BudgetExceeded(string detail) : base(detail, AppUiFaultBand.Viewport.Code(3)) { } }
    public sealed record LeaseRejected : ViewportFault { public LeaseRejected(string detail) : base(detail, AppUiFaultBand.Viewport.Code(4)) { } }
}

// One target request carries every allocation fact the resolve ladder owns: the DISPLAY extent the composition
// leases at, the row's RenderScale, and the row's sample count. Info derives the sub-resolution extent rather
// than storing it, so the render extent and the display extent cannot disagree and the composite reads its own
// upscale factor off the target it drew into. A caller-minted extent beside the resolve row is the deleted
// form — it is what left `Fsr`'s 0.6 scale and `Msaa`'s four samples with no allocation to reach.
public readonly record struct RenderTargetRequest(SKImageInfo Display, double RenderScale, int Samples) {
    public SKImageInfo Info =>
        RenderScale >= 1d
            ? Display
            : Display.WithSize(
                Math.Max((int)Math.Round(Display.Width * RenderScale), 1),
                Math.Max((int)Math.Round(Display.Height * RenderScale), 1));

    public static RenderTargetRequest Of(SKImageInfo display, ResolvePass resolve) =>
        new(display, resolve.RenderScale, resolve.Samples);
}

// Everything one frame's passes read about HOW to look at the scene, as one value: the portable view-state
// lens, THIS frame's sub-pixel projection offset, and the governor verdict that scales the cut. The jitter
// stays off ViewCamera because ViewCamera is the saved-viewpoint value the BCF codec and the wire project — a
// per-frame jitter column there would ride into every stored view — and because a sub-pixel jitter is a
// projection shear, not an eye or target move, so folding it into CameraFrame would spell a rotation the
// raster does not want. Cull and LOD read Camera and LodScale (a sub-pixel offset moves no cull decision);
// the geometry draw adds NdcJitter to its projection's third column; the path tracer reads Camera alone,
// because a per-frame jitter would trip AccumulationTarget.Reset every frame and the film would never
// converge past one sample. Threading the lens and the verdict as two arguments is what let a composition
// close over its own lodScale and leave the governor's degrade lever with no reader.
public readonly record struct FrameView(ViewCamera Camera, (double X, double Y) NdcJitter, QualityVerdict Quality) {
    public double LodScale => Quality.LodPixelScale;

    // Pixels to NDC against the target the frame actually allocated: two NDC units span the full extent, and
    // NDC Y rises where the target's Y falls. Converting HERE — once, where the extent is known — is what
    // keeps a sub-resolution FSR target from jittering by a display-sized offset.
    public static FrameView Of(ViewCamera camera, (double X, double Y) jitterPixels, SKImageInfo info, QualityVerdict quality) =>
        new(camera, (2d * jitterPixels.X / Math.Max(info.Width, 1), -2d * jitterPixels.Y / Math.Max(info.Height, 1)), quality);
}

// Backend rows CARRY their target construction: the [UseDelegateFromConstructor] column folds the
// composition-bound GpuBinding case and the resolve row's own RenderTargetRequest to a RenderTarget, and a
// binding case a row does not own is the typed BackendUnsupported fault — behavior recovers from the selected
// row, never from a detached factory record. Every arm returns the GRANTED sample count beside the request:
// a Ganesh target answers GRBackendRenderTarget.SampleCount, the raster floor answers one, and publishing the
// requested count would forge a multi-sample measurement the allocation never took.
[SmartEnum<string>]
public sealed partial class GpuBackend {
    public static readonly GpuBackend Metal = new("metal", GpuFamily.SkiaGanesh, GaneshTarget);
    public static readonly GpuBackend Vulkan = new("vulkan", GpuFamily.SkiaGanesh, GaneshTarget);
    public static readonly GpuBackend OpenGl = new("opengl", GpuFamily.SkiaGanesh, GaneshTarget);
    public static readonly GpuBackend Software = new("software", GpuFamily.SkiaRaster, RasterTarget);
    public static readonly GpuBackend Wgpu = new("wgpu", GpuFamily.Wgpu, WgpuTarget);
    public static readonly GpuBackend WebGpu = new("webgpu", GpuFamily.WebGpu, BrowserTarget);

    public GpuFamily Family { get; }

    public bool IsGpu => Family.Accelerated;

    [UseDelegateFromConstructor]
    public partial Fin<RenderTarget> Target(GpuBinding binding, RenderTargetRequest request);

    private static Fin<RenderTarget> GaneshTarget(GpuBinding binding, RenderTargetRequest request) => binding switch {
        GpuBinding.Ganesh ganesh => ganesh.Lease(request),
        _ => Fin.Fail<RenderTarget>(new ViewportFault.BackendUnsupported($"{binding.Backend.Key}: not a Ganesh binding")),
    };

    // SKSurface.Create(SKImageInfo) takes no sample count, so the CPU raster is single-sampled by construction:
    // the floor GRANTS one sample whatever the resolve row asked for, and the Msaa row's multi-sample claim is
    // an accelerated-target property the fallback frame honestly reports itself as not holding.
    private const int RasterSamples = 1;

    private static Fin<RenderTarget> RasterTarget(GpuBinding binding, RenderTargetRequest request) => binding switch {
        GpuBinding.Raster => SKSurface.Create(request.Info) switch {
            { } surface => Fin.Succ(new RenderTarget(Software, Some(surface), None, request, RasterSamples, surface)),
            _ => Fin.Fail<RenderTarget>(new ViewportFault.ContextUnavailable("software: raster surface allocation failed")),
        },
        _ => Fin.Fail<RenderTarget>(new ViewportFault.BackendUnsupported($"{binding.Backend.Key}: not the raster floor")),
    };

    private static Fin<RenderTarget> WgpuTarget(GpuBinding binding, RenderTargetRequest request) => binding switch {
        GpuBinding.Wgpu wgpu => wgpu.Acquire(wgpu.Presentation, request),
        _ => Fin.Fail<RenderTarget>(new ViewportFault.BackendUnsupported($"{binding.Backend.Key}: not a wgpu binding")),
    };

    private static Fin<RenderTarget> BrowserTarget(GpuBinding binding, RenderTargetRequest request) => binding switch {
        GpuBinding.Browser browser => browser.Acquire(browser.Surface, request),
        _ => Fin.Fail<RenderTarget>(new ViewportFault.BackendUnsupported($"{binding.Backend.Key}: not a browser binding")),
    };
}

// Each family DECLARES the three facts its consumers otherwise re-enumerate as disjunctions: Skia holds where
// the row draws through an SKCanvas (Ganesh and the raster floor alike), Chained holds where the row binds a
// native shader chain, and Accelerated is false only on the CPU floor. A consumer asks a column, never a
// `Family == A || Family == B` list — a fifth family added to the roster falls out of every hand-written
// disjunction silently and lands on whichever arm the last `else` happens to be, which is the deleted form.
[SmartEnum<string>]
public sealed partial class GpuFamily {
    public static readonly GpuFamily SkiaGanesh = new("skia-ganesh", skia: true, chained: false, accelerated: true);
    public static readonly GpuFamily SkiaRaster = new("skia-raster", skia: true, chained: false, accelerated: false);
    public static readonly GpuFamily Wgpu = new("wgpu", skia: false, chained: true, accelerated: true);
    public static readonly GpuFamily WebGpu = new("webgpu", skia: false, chained: true, accelerated: true);

    public bool Skia { get; }
    public bool Chained { get; }
    public bool Accelerated { get; }
}

// Composition-bound GPU substrate: each case pins the state its backend row folds, Backend DERIVES the row
// from the case, and the Ganesh admission gate rejects a non-Ganesh row — so the graph holds bindings alone
// and a backend paired with a foreign substrate never constructs. Ganesh closes over the embed-capsule
// platform lease, Wgpu over the shared device and its presentation arm, Raster is the CPU floor, Browser the
// TS web leg's surface.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record GpuBinding {
    private GpuBinding() { }

    public sealed record Ganesh : GpuBinding {
        private Ganesh(GpuBackend row, Func<RenderTargetRequest, Fin<RenderTarget>> lease) { Row = row; Lease = lease; }

        public GpuBackend Row { get; }
        public Func<RenderTargetRequest, Fin<RenderTarget>> Lease { get; }

        public static Fin<GpuBinding> Of(GpuBackend row, Func<RenderTargetRequest, Fin<RenderTarget>> lease) =>
            row.Family == GpuFamily.SkiaGanesh
                ? Fin.Succ<GpuBinding>(new Ganesh(row, lease))
                : Fin.Fail<GpuBinding>(new ViewportFault.BackendUnsupported($"{row.Key}: not a Ganesh row"));
    }

    public sealed record Raster : GpuBinding;

    public sealed record Wgpu(WgpuDevice Device, WgpuPresentation Presentation, Func<WgpuPresentation, RenderTargetRequest, Fin<RenderTarget>> Acquire) : GpuBinding;

    public sealed record Browser(nint Surface, Func<nint, RenderTargetRequest, Fin<RenderTarget>> Acquire) : GpuBinding;

    public GpuBackend Backend => this switch {
        Ganesh ganesh => ganesh.Row,
        Wgpu => GpuBackend.Wgpu,
        Browser => GpuBackend.WebGpu,
        _ => GpuBackend.Software,
    };

    public Fin<RenderTarget> Target(RenderTargetRequest request) => Backend.Target(this, request);
}

// WGPU presentation dispatch the Wgpu backend row routes through: Composited imports the
// externally-rendered texture through the compositor interop family (a second swapchain in composited
// mode cannot type), Swapchain survives ONLY as the exclusive-fullscreen arm, Headless renders offscreen.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record WgpuPresentation {
    private WgpuPresentation() { }

    public sealed record Composited(
        ICompositionGpuInterop Interop,
        CompositionDrawingSurface Surface,
        ICompositionImportedGpuImage Image,
        CompositionGpuImportedImageSynchronizationCapabilities Sync,
        Option<(ICompositionImportedGpuSemaphore Wait, ICompositionImportedGpuSemaphore Signal)> Semaphores) : WgpuPresentation;

    public sealed record Swapchain(nint WgpuSurface, Func<nint, IO<Unit>> Submit) : WgpuPresentation;

    // Headless carries no extent of its own: the frame's RenderTargetRequest is the one allocation fact, and a
    // presentation-held SKImageInfo beside it is a second extent authority that drifts the moment the resolve
    // ladder changes RenderScale.
    public sealed record Headless : WgpuPresentation;

    // Composited construction: the wgpu device is created against the compositor adapter (DeviceLuid/
    // DeviceUuid pin), the shared texture imports ONCE, and the synchronization arm derives from the
    // interop's own capability probe — never assumed.
    public static Fin<WgpuPresentation> CompositedOf(
        ICompositionGpuInterop interop,
        CompositionDrawingSurface surface,
        IPlatformHandle sharedTexture,
        PlatformGraphicsExternalImageProperties shape,
        string handleType,
        Func<CompositionGpuImportedImageSynchronizationCapabilities, Option<(ICompositionImportedGpuSemaphore Wait, ICompositionImportedGpuSemaphore Signal)>> semaphores) =>
        interop.GetSynchronizationCapabilities(handleType) switch {
            var sync => Fin.Succ<WgpuPresentation>(new Composited(
                interop, surface, interop.ImportImage(sharedTexture, shape), sync, semaphores(sync))),
        };

    // Per-frame refresh awaits import completion, rejects lost interop/image/semaphore handles, then uses the
    // capability-discriminated keyed-mutex, timeline, binary-semaphore, or Automatic update arm.
    public IO<Unit> Present(uint acquireIndex, uint releaseIndex, ulong waitValue, ulong signalValue) => this switch {
        Composited c => IO.liftAsync(async () => {
            await c.Image.ImportCompleted;
            if (c.Interop.IsLost || c.Image.IsLost || c.Semaphores.Exists(pair => pair.Wait.IsLost || pair.Signal.IsLost)) {
                throw ((Error)new ViewportFault.LeaseRejected("wgpu/present: compositor import lost")).ToException();
            }
            await (c switch {
                { Sync: var sync } when sync.HasFlag(CompositionGpuImportedImageSynchronizationCapabilities.KeyedMutex) =>
                    c.Surface.UpdateWithKeyedMutexAsync(c.Image, acquireIndex, releaseIndex),
                { Sync: var sync, Semaphores.Case: (ICompositionImportedGpuSemaphore wait, ICompositionImportedGpuSemaphore signal) }
                    when sync.HasFlag(CompositionGpuImportedImageSynchronizationCapabilities.TimelineSemaphores) =>
                    c.Surface.UpdateWithTimelineSemaphoresAsync(c.Image, wait, waitValue, signal, signalValue),
                { Sync: var sync, Semaphores.Case: (ICompositionImportedGpuSemaphore wait, ICompositionImportedGpuSemaphore signal) }
                    when sync.HasFlag(CompositionGpuImportedImageSynchronizationCapabilities.Semaphores) =>
                    c.Surface.UpdateWithSemaphoresAsync(c.Image, wait, signal),
                { Sync: var sync } when sync.HasFlag(CompositionGpuImportedImageSynchronizationCapabilities.Automatic) =>
                    c.Surface.UpdateAsync(c.Image),
                _ => throw ((Error)new ViewportFault.BackendUnsupported("wgpu/present: imported image exposes no supported synchronization mode")).ToException(),
            });
            return unit;
        }),
        Swapchain swapchain => swapchain.Submit(swapchain.WgpuSurface),
        Headless => IO.pure(unit),
    };
}

// GPU validation ingress on the shared device: DeviceSetUncapturedErrorCallback installs once at device
// acquisition; the push/pop pair brackets suspect pass encoding so a validation or OOM error is a counted
// ViewportFault on the telemetry spine, never a swallowed native abort.
public sealed record WgpuErrorScope(Action Push, Func<Option<string>> Pop) {
    public Fin<T> Guarded<T>(Func<Fin<T>> encode) {
        Push();
        Fin<T> outcome = encode();
        return Pop().Match(
            Some: static error => Fin.Fail<T>(new ViewportFault.Text($"wgpu/validation: {error}")),
            None: () => outcome);
    }
}

// Composition-bound GPU-time lane over the shared device: Measure brackets one frame — a
// QueryType.Timestamp DeviceCreateQuerySet (gated on the timestamp-query feature at composition), per-pass
// RenderPassTimestampWrites/ComputePassTimestampWrites, CommandEncoderResolveQuerySet into the read buffer,
// QueueSubmitForIndex minting the WrappedSubmissionIndex that DevicePoll retires without a blocking fence,
// BufferMapAsync/BufferGetMappedRange/BufferUnmap folding (lastTick - firstTick) x queue period to Duration,
// QuerySetRelease at teardown. The graph binds None on a device without the feature, so FrameReceipt.Gpu is
// measured evidence or the honest zero, never the CPU pass elapsed re-labelled as GPU time.
public sealed record WgpuFrameEvidence(Func<Fin<Duration>> Measure);

// Key threads through the base positional parameter (the ControlIntent pattern) — a base computed
// Key => Switch beside same-named case positionals suppresses property synthesis and recurses (CS8907).
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RenderPass(string Key) {
    public sealed record Cull(string Key, Func<RenderTarget, MeshletCluster, FrameView, Fin<(MeshletCluster Cluster, CullResult Result)>> Visible) : RenderPass(Key);
    // Geometry is the ONLY pass that draws triangles, so it alone carries the frame's triangle contract as two
    // members of one row: Charge is the budget projection the pre-charge gate reads BEFORE the pass runs, and Draw
    // returns the triangle count its own draw recorded. Both read the frame's own CUT — the `Render/meshlets`
    // DrawCut narrowed to this row's CutPhase — never the un-narrowed cluster set: a draw handed the whole payload
    // submits geometry the cull ladder already rejected, and the two-phase HZB ladder cannot schedule its retest
    // draw at all when the second phase's view list never leaves the cull arm. A meshlet draw charges the cut it is
    // about to submit and reports what it submitted; a shade mount and a capture composite take the whole-cut phase,
    // charge zero, and report zero, because they re-shade geometry another pass already drew. A cluster count, a path
    // point count, or the whole scene's total returned from either member publishes a fabricated
    // FrameReceipt.Triangles and defers passes on a budget nothing spends.
    public sealed record Geometry(string Key, CutPhase Phase, Func<DrawCut, long> Charge, Func<RenderTarget, FrameView, DrawCut, Fin<long>> Draw) : RenderPass(Key);
    public sealed record PathTrace(string Key, PathTracePass Pass, Atom<AccumulationTarget> Film, LightRig Rig, int SampleBudget, long Seed, CancelScope Scope) : RenderPass(Key);
    public sealed record Sim(string Key, Func<RenderTarget, Fin<int>> Draw) : RenderPass(Key);
    public sealed record Composite(string Key, Func<SKCanvas, RenderTargetRequest, ResolveState, Fin<Unit>> Raster) : RenderPass(Key);
    public sealed record Overlay(string Key, Func<SKCanvas, Fin<Unit>> Draw) : RenderPass(Key);
}

public readonly record struct ResolveState(
    long Ordinal,
    (double X, double Y) Jitter,
    Option<SKImage> History,
    double RenderScale,
    Option<ViewCamera> Camera);

[SmartEnum<string>]
public sealed partial class ResolvePass {
    public static readonly ResolvePass Msaa = new("msaa", samples: 4, renderScale: 1.0, reproject: false);
    public static readonly ResolvePass Taa = new("taa", samples: 1, renderScale: 1.0, reproject: true);
    public static readonly ResolvePass Fsr = new("fsr", samples: 1, renderScale: 0.6, reproject: false);
    public static readonly ResolvePass Smaa = new("smaa", samples: 1, renderScale: 1.0, reproject: false);

    public int Samples { get; }
    public double RenderScale { get; }
    public bool Reproject { get; }

    // TAA sub-pixel jitter is the Halton (2,3) sequence READ off the kernel equidistribution owner — the
    // base-2 leg through the bit-reversal fast path, the base-3 leg through the radix-parameterized radical
    // inverse — so the sequence is generable at any phase and a hand-transcribed sample table cannot drift
    // from the generator. Eight phases keep the reprojection history window coherent; the period is policy.
    private const int JitterPeriod = 8;

    // Centred on the pixel: the radical inverse lands in [0, 1) and a sub-pixel camera offset is signed
    // about the pixel centre, so the half-shift is the projection offset itself, never a consumer-side
    // correction one consumer would apply and the next would forget.
    private static (double X, double Y) HaltonJitter(long ordinal) =>
        (uint)((ordinal % JitterPeriod) + 1L) switch {
            var phase => (Deterministic.RadicalInverse(bits: phase) - 0.5,
                          Deterministic.RadicalInverse(index: phase, radix: 3) - 0.5),
        };

    // Camera motion invalidates temporal history: a moved camera re-seeds the ordinal and drops History
    // in the same transition, so TAA never reprojects a stale frame and accumulation resets coherently.
    public ResolveState Advance(ResolveState prior, ViewCamera camera) =>
        (prior.Camera.Map(held => held == camera).IfNone(false) ? prior : prior with { Ordinal = 0L, History = None }) switch {
            var seeded => Reproject
                ? seeded with {
                    Ordinal = seeded.Ordinal + 1,
                    Jitter = HaltonJitter(ordinal: seeded.Ordinal + 1),
                    History = seeded.History,
                    RenderScale = RenderScale,
                    Camera = Some(camera),
                }
                : seeded with { Ordinal = seeded.Ordinal + 1, Jitter = (0d, 0d), History = None, RenderScale = RenderScale, Camera = Some(camera) },
        };

    // The composite receives the TARGET'S OWN request beside the temporal state, so the upscale it performs is
    // read off the surface it is reading from: request.Info is the sub-resolution extent the frame rendered at,
    // request.Display the extent it composites to, and request.RenderScale their ratio. An Fsr composite that
    // cannot see the scale it must undo is the shape that left the 0.6 render scale with no consumer.
    public Fin<Unit> Resolve(RenderTarget target, ResolveState state, Func<SKCanvas, RenderTargetRequest, ResolveState, Fin<Unit>> composite) =>
        target.Surface.Match(
            Some: surface => composite(surface.Canvas, target.Request, state),
            None: () => Fin.Fail<Unit>(new ViewportFault.ContextUnavailable($"resolve/{Key}: no resolve surface")));
}

// The rank extremes are the TABLE'S own, seated once at construction. A literal 0..4 clamp couples this ladder
// to contiguous zero-based tier ranks, and the one growth move the `Diagnostics/governor.md` vocabulary declares
// is a grade at either extreme — which turns the clamp into a lookup for a rank the table never mapped and
// answers `Msaa` on the tier that most needs `Fsr`. `QualityTier.Ranked` clamps against its own extremes for
// exactly this reason; a ladder keyed on those ranks holds the same rule.
public sealed record ResolvePolicy {
    private ResolvePolicy(FrozenDictionary<int, ResolvePass> byTier, int floor, int ceiling) =>
        (ByTier, Floor, Ceiling) = (byTier, floor, ceiling);

    public FrozenDictionary<int, ResolvePass> ByTier { get; }
    public int Floor { get; }
    public int Ceiling { get; }

    public static ResolvePolicy Of(params ReadOnlySpan<(int Rank, ResolvePass Pass)> rows) =>
        rows.ToArray().ToFrozenDictionary(static row => row.Rank, static row => row.Pass) switch {
            var table => new ResolvePolicy(table, table.Keys.Min(), table.Keys.Max()),
        };

    public static readonly ResolvePolicy Default = Of(
        (4, ResolvePass.Taa), (3, ResolvePass.Taa), (2, ResolvePass.Smaa), (1, ResolvePass.Msaa), (0, ResolvePass.Fsr));

    // A rank inside the extremes that the table skipped falls to the single-sampled `Msaa` floor rather than
    // throwing: an unmapped grade renders, and the gap is a policy-table defect the composition owns.
    public ResolvePass For(int tierRank) =>
        ByTier.TryGetValue(Math.Clamp(tierRank, Floor, Ceiling), out ResolvePass? pass) ? pass : ResolvePass.Msaa;
}

// Samples is the GRANTED count, read back off the allocation — GRBackendRenderTarget.SampleCount on the Ganesh
// arm, the TextureDescriptor.SampleCount the wgpu arm asked for and the device honoured, one on the raster
// floor — never the requested column echoed. A target republishing its request as its measurement spells a
// multi-sample resolve on a surface that took one sample.
public sealed record RenderTarget(GpuBackend Backend, Option<SKSurface> Surface, Option<GRContext> Context, RenderTargetRequest Request, int Samples, IDisposable Native) : IDisposable {
    public void Dispose() => Native.Dispose();
}

// Every gated axis carries its OWN ceiling. A phase share gated on the whole-frame duration is unreachable —
// layout inside a frame can exceed the frame budget only once the frame already has — and an eviction count
// gated at zero degrades on the normal operation of a byte-budgeted cache. GPU is the one axis that can
// legitimately exceed its frame share, because it runs a frame behind the CPU that queued it.
public sealed record FrameBudget(Duration Frame, Duration Gpu, Duration Layout, long VramBytes, long EvictsPerFrame, int MaxTriangles) {
    public static readonly FrameBudget Sixty = new(
        Duration.FromMilliseconds(16.667), Duration.FromMilliseconds(14.0), Duration.FromMilliseconds(4.0),
        1_073_741_824L, 8L, 20_000_000);

    public static readonly FrameBudget Thirty = new(
        Duration.FromMilliseconds(33.333), Duration.FromMilliseconds(28.0), Duration.FromMilliseconds(8.0),
        536_870_912L, 16L, 8_000_000);
}

public sealed record ViewportClock(ClockPolicy Clocks, CorrelationId Correlation);

// Fault and Deferred are LOCAL egress columns (trailing, defaulted): the FrameReceiptWire projection
// omits both, so the frozen web wire is untouched while in-process consumers distinguish a failed frame
// from fallback and read which passes the budget invariant deferred.
public sealed record FrameReceipt(
    long Ordinal,
    GpuBackend Backend,
    Seq<(string Pass, Duration Elapsed)> Passes,
    Duration Gpu,
    long Triangles,
    bool WithinBudget,
    Instant At,
    CorrelationId Correlation,
    Option<Error> Fault = default,
    Seq<string> Deferred = default) {
    public const string Kind = "frame";
}

public sealed record RenderGraph(
    Seq<RenderPass> Passes,
    Atom<MeshletCluster> Cluster,
    GpuBinding Binding,
    GpuBinding.Raster Fallback, // the CPU floor is structural: a non-raster fallback binding is unrepresentable
    ResolvePolicy Policy,
    Option<WgpuFrameEvidence> GpuTime,
    Func<GpuBinding, Func<SKImageInfo, RenderTargetRequest>, Func<RenderTarget, Fin<FrameReceipt>>, Fin<FrameReceipt>> Lease,
    Func<FrameReceipt, IO<Unit>> Sink) {
    private long ordinal;
    private readonly Atom<ResolveState> resolve = Atom(new ResolveState(0L, (0d, 0d), None, 1.0, None));

    // Interlocked ordinal threads through EVERY arm — the GPU fold, the fallback re-lease, and the
    // Empty fault path — so no receipt is ever constructed with a literal zero ordinal. A lease-class
    // fault re-runs the frame through the Fallback binding over the Composite/Overlay passes, so the
    // software floor is a reachable arm of this fold, never an inert constructor field.
    // Every arm seals its OWN instant and correlation off the one clock: a post-fold `with` re-stamp would
    // date every receipt at the sink rather than at the frame, and it is what forced the ghost `default`
    // identity a receipt constructor must never mint.
    // The governor verdict rides IN WHOLE rather than as a rank plus a mask: the tier's rank selects the
    // resolve row, its PassMask filters the DAG, and its LodPixelScale is the cut's own error multiplier, so
    // three facts one authority derives cannot arrive from three callers disagreeing.
    public IO<FrameReceipt> Frame(ViewportClock clock, FrameBudget budget, QualityVerdict quality, ViewCamera camera) =>
        from next in IO.lift(() => Interlocked.Increment(ref ordinal))
        from receipt in IO.lift(() => Render(next, clock, budget, quality, camera, Binding, Passes.Filter(quality.PassMask))
            .BindFail(fault => fault is ViewportFault.LeaseRejected or ViewportFault.ContextUnavailable
                ? Render(next, clock, budget, quality, camera, Fallback,
                    Passes.Filter(static pass => pass is RenderPass.Composite or RenderPass.Overlay))
                : Fin.Fail<FrameReceipt>(fault))
            .IfFail(fault => Empty(next, clock, fault)))
        from _ in Sink(receipt)
        select receipt;

    // The resolve transition runs BEFORE the lease because the lease needs its answer: the row's RenderScale
    // and sample count are what the target is minted at, so the composition hands its display extent to the
    // request builder and the resolve ladder — not the caller — fixes the allocation.
    private Fin<FrameReceipt> Render(long next, ViewportClock clock, FrameBudget budget, QualityVerdict quality, ViewCamera camera, GpuBinding binding, Seq<RenderPass> passes) {
        ResolvePass resolvePass = Policy.For(quality.Tier.Rank);
        ResolveState prior = resolve.Value;
        bool moved = prior.Camera.Map(held => held != camera).IfNone(true);
        if (moved || !resolvePass.Reproject) { prior.History.Iter(static image => image.Dispose()); }
        ResolveState state = resolve.Swap(held => resolvePass.Advance(held, camera));
        return Lease(
            binding,
            display => RenderTargetRequest.Of(display, resolvePass),
            target => FrameView.Of(camera, state.Jitter, target.Request.Info, quality) switch {
                var view => passes
                    .Fold(
                        Fin.Succ(PassFold.Empty),
                        (rail, pass) => rail.Bind(fold => Execute(pass, target, clock.Clocks, budget, view, moved, resolvePass, state, fold)))
                    .Map(folded => Seal(next, clock, budget, binding, target, resolvePass, folded)),
            });
    }

    // Frame retirement: the reprojection history snapshots off the target the fold just drew, then the receipt
    // seals. The Gpu column carries ONLY completed measurements: an absent timestamp lane is the honest zero,
    // and a FAILED readback keeps zero while its fault lands on the receipt fault rail — unsupported and failed
    // stay distinguishable in frame evidence.
    private FrameReceipt Seal(long next, ViewportClock clock, FrameBudget budget, GpuBinding binding, RenderTarget target, ResolvePass resolvePass, PassFold folded) {
        SnapshotHistory(resolvePass, target);
        (Duration gpu, Option<Error> gpuFault) = GpuTime.Match(
            Some: lane => lane.Measure().Match(
                Succ: static measured => (measured, Option<Error>.None),
                Fail: static fault => (Duration.Zero, Some(fault))),
            None: static () => (Duration.Zero, Option<Error>.None));
        return new FrameReceipt(
            next, binding.Backend, folded.Passes,
            gpu,
            folded.Triangles,
            folded.Deferred.IsEmpty && folded.Elapsed <= budget.Frame && folded.Triangles <= budget.MaxTriangles,
            clock.Clocks.Now, clock.Correlation, gpuFault, folded.Deferred);
    }

    private void SnapshotHistory(ResolvePass pass, RenderTarget target) {
        if (!pass.Reproject) { return; }
        target.Surface.Iter(surface => {
            SKImage next = surface.Snapshot();
            resolve.Swap(state => {
                state.History.Iter(static image => image.Dispose());
                return state with { History = Some(next) };
            });
        });
    }

    // The frame's cut rides the FOLD, not a second cell: the cull arm writes it, the geometry arms read it, and
    // it cannot outlive the frame that produced it — which is what an Atom beside the cluster cell could not
    // promise. A geometry pass scheduled ahead of any cull reads CullResult.Empty and draws the honest nothing;
    // handing it the un-narrowed cluster set instead is the shape that made the cull ladder decorative.
    private readonly record struct PassFold(Seq<(string Pass, Duration Elapsed)> Passes, Seq<string> Deferred, long Triangles, CullResult Cut) {
        public static readonly PassFold Empty = new(Seq<(string, Duration)>(), Seq<string>(), 0L, CullResult.Empty);

        public Duration Elapsed => Passes.Map(static row => row.Elapsed).Fold(Duration.Zero, static (sum, next) => sum + next);
    }

    // Budget invariant executes HERE: a pass whose start would overrun the frame duration, or whose own declared
    // charge carries the fold past the triangle ceiling, defers — recorded, never executed — so the sealed
    // verdict derives from measured elapsed evidence. Every arm returns its DRAWN TRIANGLES and nothing else: the
    // cull arm advances the cluster cell, publishes its cut onto the fold, and draws none; the pathTrace arm reads
    // the UNJITTERED lens, resets the film on camera motion, and swaps the advanced AccumulationTarget back into
    // its cell drawing none; the sim arm keeps its own path-point return as a pass-local measure the triangle fold
    // never reads; composite and overlay draw none. Only the geometry arm answers a triangle count, and it answers
    // the one its own draw recorded over the cut phase its row selected.
    private Fin<PassFold> Execute(
        RenderPass pass, RenderTarget target, ClockPolicy clocks, FrameBudget budget,
        FrameView view, bool moved, ResolvePass resolvePass, ResolveState state, PassFold fold) =>
        fold.Elapsed >= budget.Frame || fold.Triangles + EstimatedTriangles(pass, fold.Cut) > budget.MaxTriangles
            ? Fin.Succ(fold with { Deferred = fold.Deferred.Add(pass.Key) })
            : clocks.Mark() switch {
                var mark => pass.Switch(
                        // The CELL rides, never a snapshot beside it: the cull arm swaps the advanced cull state in
                        // and the geometry arms read the cell fresh, so the two can never disagree about which
                        // cluster owner the frame is drawing from.
                        state: (Target: target, ClusterCell: Cluster, View: view, Moved: moved,
                                Resolve: resolvePass, State: state, Cut: fold.Cut),
                        // Each arm answers the SAME pair — the cut it leaves on the fold and the triangles it drew —
                        // so publishing a cut and reporting a draw are one contract no arm can half-satisfy. The
                        // cull arm alone rewrites the cut; every other arm passes the one it received through.
                        cull: static (ctx, c) => c.Visible(ctx.Target, ctx.ClusterCell.Value, ctx.View)
                            .Map(next => (Cut: ctx.ClusterCell.Swap(_ => next.Cluster) switch { _ => next.Result }, Drawn: 0L)),
                        geometry: static (ctx, g) => g.Draw(ctx.Target, ctx.View, new DrawCut(ctx.ClusterCell.Value, g.Phase.Select(ctx.Cut)))
                            .Map(drawn => (Cut: ctx.Cut, Drawn: drawn)),
                        pathTrace: static (ctx, p) => (ctx.Moved ? p.Film.Swap(static film => film.Reset()) : p.Film.Value) switch {
                            var film => p.Pass.Accumulate(film, ctx.View.Camera, p.Rig, p.SampleBudget, p.Seed, p.Scope)
                                .Map(advanced => (Cut: p.Film.Swap(_ => advanced) switch { _ => ctx.Cut }, Drawn: 0L)),
                        },
                        sim: static (ctx, s) => s.Draw(ctx.Target).Map(_ => (Cut: ctx.Cut, Drawn: 0L)),
                        composite: static (ctx, c) => ctx.Resolve.Resolve(ctx.Target, ctx.State, c.Raster).Map(_ => (Cut: ctx.Cut, Drawn: 0L)),
                        overlay: static (ctx, o) => ctx.Target.Surface.Match(
                            Some: surface => o.Draw(surface.Canvas).Map(_ => (Cut: ctx.Cut, Drawn: 0L)),
                            None: () => Fin.Succ((Cut: ctx.Cut, Drawn: 0L))))
                    .Map(answer => fold with {
                        Passes = fold.Passes.Add((pass.Key, clocks.Elapsed(mark))),
                        Triangles = fold.Triangles + answer.Drawn,
                        Cut = answer.Cut,
                    }),
            };

    // Pre-charge reads the pass's OWN charge against the FRAME'S CUT, never the scene total for every
    // geometry-shaped pass: N materials mean N shade mounts, and charging each of them the whole scene defers on a
    // budget nothing spends while the receipt reads that deferral as a legitimate overrun. Charging the cut is also
    // what makes the estimate honest — the whole cluster set overruns the ceiling on any scene the LOD cut exists
    // to make drawable.
    private long EstimatedTriangles(RenderPass pass, CullResult cut) =>
        pass is RenderPass.Geometry geometry ? geometry.Charge(new DrawCut(Cluster.Value, geometry.Phase.Select(cut))) : 0L;

    // Failed frame is DISTINGUISHABLE from a healthy software fallback: the fault threads onto the
    // receipt's Fault column and no fabricated pass row exists — zero passes executed is the honest fact.
    private FrameReceipt Empty(long next, ViewportClock clock, Error fault) =>
        new(next, GpuBackend.Software, Seq<(string Pass, Duration Elapsed)>(), Duration.Zero, 0L, false, clock.Clocks.Now, clock.Correlation, Some(fault));

    public const string FrameInstrument = "rasm.appui.viewport.frame.elapsed";
    public const string GpuInstrument = "rasm.appui.viewport.gpu.elapsed";
    public const string OverrunInstrument = "rasm.appui.viewport.budget.overrun";

    // Budget rides IN because viewport reliability policy names these three rows and nothing else: handing the
    // pack down here puts panels, objectives, and their series on ONE port, so `Mount`'s existing fold proves
    // widget resolution and objective-name distinctness against the set this call binds. Contributing the rows
    // bare strands the pack with no carrier, leaving a reliability policy no mount ever admits.
    public static TelemetryContributorPort TelemetryRow(string version, FrameBudget budget) =>
        AppUiTelemetry.Contribute(version, ViewportObjectives.Pack(budget),
            InstrumentSpec.Advised(FrameInstrument, "s", "frame wall duration", MeasureForm.Real, Buckets.UiFrameSeconds, AppUiTelemetry.BackendSlot),
            InstrumentSpec.Advised(GpuInstrument, "s", "measured GPU duration per frame", MeasureForm.Real, Buckets.UiFrameSeconds,
                AppUiTelemetry.PassSlot, AppUiTelemetry.UnmeasuredSlot),
            InstrumentSpec.Count(OverrunInstrument, "{frame}", "frames exceeding the frame budget", MeasureForm.Whole));

    // Frame timing rides the direct rail: composition binds this projection at the retire site where the typed
    // receipt AND the frame's accepted residency plan are both in hand, so the per-frame path never serializes an
    // envelope; the gpu instrument stays the evidence fan's gpu-frame arm target off the governor timeline. The
    // plan threads through HERE because this IS the frame's seal point, and it is the one binder
    // `Render/meshlets#RESIDENCY_BUDGET` `ResidencyBudget.Observe` has: residency gauges written anywhere else
    // report a plan some other frame drew, and written nowhere they publish three levels no writer ever fills.
    public static Fin<Unit> Observe(InstrumentSet set, FrameReceipt receipt, ResidencyPlan plan) =>
        set.Write(FrameInstrument,
                receipt.Passes.Fold(Duration.Zero, static (total, pass) => total + pass.Elapsed).TotalSeconds,
                new KeyValuePair<string, object?>(AppUiTelemetry.BackendSlot, receipt.Backend.Key))
            .Bind(_ => receipt.WithinBudget ? Fin.Succ(unit) : set.Write(OverrunInstrument, 1L))
            .Bind(_ => ResidencyBudget.Observe(set, plan))
            .Map(static _ => unit);
}
```

```mermaid
---
config:
  layout: elk
  flowchart:
    curve: linear
    padding: 25
---
flowchart LR
    accTitle: Render graph frame flow
    accDescr: A leased render target flows through cull, geometry, path tracing, simulation, composite, and receipt sealing.
    RenderGraph -->|Lease| RenderTarget
    RenderTarget --> Cull
    Cull --> Geometry
    Geometry --> PathTrace
    PathTrace --> Sim
    Sim --> Composite
    Composite --> FrameReceipt
    FrameReceipt --> ReceiptSinkPort
```

## [03]-[SIM_VISUAL]

- Owner: `SimField` the Compute field receipt projection; `SimVisual` `[Union]` the simulation render-pass family; `TransferFunction` the volume opacity-and-color map.
- Cases: `SimVisual` = Isosurface | Volume | Streamline | Glyph | Deformation | MeshQuality | ParallelCoords under the locked kind literals isosurface, volume, streamline, glyph, deformation, mesh-quality, parallel-coords.
- Entry: `public Fin<RenderPass> Pass(SimField field)` dispatches every visualization case into an executable `RenderPass.Sim`; the transient-playback frame is a field index, never a wall-clock tick.
- Auto: the isosurface case marching-cubes-extracts the level set at the threshold, the volume case ray-marches the scalar field through the `TransferFunction` opacity-color map, the streamline case integrates the vector field through Runge-Kutta seeds, the glyph case places oriented arrow or tensor glyphs at the sample points, the deformation case warps the mesh by the displacement field at the playback frame, the mesh-quality case emits one `VisualStroke` per cell band inked from that cell's scaled-Jacobian or aspect-ratio metric (the Charts stroke owner — a single accumulated path flattens the field to one pigment), and the parallel-coords case routes its multi-dimensional cells onto the `CustomVisual.ParallelCoordinates` fold so a parameter sweep reads one analytical chart; transient playback scrubs a field-index sequence so a deformation or transient field animates by frame index under the deterministic motion clock.
- Packages: SkiaSharp, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm.Compute (project)
- Growth: a new field visualization is one `SimVisual` case; a new transfer-function ramp is one `Colormap` row consumed here; zero new surface.
- Boundary: field geometry projects from Compute receipts and never re-computes a simulation. `TransferFunction` samples the Theme-owned perceptual `Colormap` rail, and malformed ranges or samples fail on `Fin`. Deformation and transient fields advance by deterministic frame index. GPU volume and isosurface passes bind through the render-graph lease, while CPU marching cubes and ray marching provide the deterministic reference path.

```csharp signature
public sealed record SimField(
    string Key,
    int DimX,
    int DimY,
    int DimZ,
    Seq<double> Scalars,
    Seq<(double X, double Y, double Z)> Vectors,
    Seq<(double X, double Y, double Z)> Displacement,
    Seq<double> CellQuality,
    int FrameIndex);

public sealed record TransferFunction {
    private TransferFunction(Colormap ramp, double floor, double ceiling, double opacityGamma) =>
        (Ramp, Floor, Ceiling, OpacityGamma) = (ramp, floor, ceiling, opacityGamma);

    public Colormap Ramp { get; }
    public double Floor { get; }
    public double Ceiling { get; }
    public double OpacityGamma { get; }

    public static readonly TransferFunction Default = new(Colormap.Viridis, floor: 0d, ceiling: 1d, opacityGamma: 2d);

    public static Fin<TransferFunction> Of(Colormap ramp, double floor, double ceiling, double opacityGamma) =>
        double.IsFinite(floor) && double.IsFinite(ceiling) && double.IsFinite(opacityGamma) && floor < ceiling && opacityGamma > 0d
            ? Fin.Succ(new TransferFunction(ramp, floor, ceiling, opacityGamma))
            : Fin.Fail<TransferFunction>(new ViewportFault.Text("transfer-function: finite floor < ceiling and positive opacity gamma required"));

    public Fin<(Color Color, double Opacity)> Sample(double scalar) =>
        double.IsFinite(scalar)
            ? Math.Clamp((scalar - Floor) / (Ceiling - Floor), 0d, 1d) switch {
                var t => Ramp.Sample(t).Map(color => (color, Math.Pow(t, OpacityGamma))),
            }
            : Fin.Fail<(Color, double)>(new ViewportFault.Text("transfer-function: finite scalar required"));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SimVisual(string Key) {
    public sealed record Isosurface(string Key, double Threshold, Func<SimField, double, Fin<SKPath>> March) : SimVisual(Key);
    public sealed record Volume(string Key, TransferFunction Transfer, Func<RenderTarget, SimField, TransferFunction, Fin<int>> RayMarch) : SimVisual(Key);
    public sealed record Streamline(string Key, int Seeds, double StepSize, Func<SimField, int, double, Fin<SKPath>> Integrate) : SimVisual(Key);
    public sealed record Glyph(string Key, double Scale, Func<SimField, double, Fin<SKPath>> Place) : SimVisual(Key);
    public sealed record Deformation(string Key, double Magnify, Func<SimField, double, int, Fin<SKPath>> Warp) : SimVisual(Key);
    // Shade emits one stroke PER CELL BAND inked from the cell's own quality value — a single accumulated path
    // flattens the per-cell field to one pigment, the monochromatic defect the Charts stroke owner deletes.
    public sealed record MeshQuality(string Key, Colormap Ramp, Func<SimField, Fin<Seq<VisualStroke>>> Shade) : SimVisual(Key);
    public sealed record ParallelCoords(string Key, Func<SimField, Fin<CustomVisualData>> Project, Func<RenderTarget, CustomVisualData, Fin<int>> Draw) : SimVisual(Key);

    public Fin<RenderPass> Pass(SimField field) => Switch(
        state: field,
        isosurface: static (f, i) => Fin.Succ<RenderPass>(new RenderPass.Sim(i.Key, target => Path(target, i.March(f, i.Threshold)))),
        volume: static (f, v) => Fin.Succ<RenderPass>(new RenderPass.Sim(v.Key, target => v.RayMarch(target, f, v.Transfer))),
        streamline: static (f, s) => Fin.Succ<RenderPass>(new RenderPass.Sim(s.Key, target => Path(target, s.Integrate(f, s.Seeds, s.StepSize)))),
        glyph: static (f, g) => Fin.Succ<RenderPass>(new RenderPass.Sim(g.Key, target => Path(target, g.Place(f, g.Scale)))),
        deformation: static (f, d) => Fin.Succ<RenderPass>(new RenderPass.Sim(d.Key, target => Path(target, d.Warp(f, d.Magnify, f.FrameIndex)))),
        meshQuality: static (f, m) => Fin.Succ<RenderPass>(new RenderPass.Sim(m.Key, target => Strokes(target, m.Shade(f), m.Ramp))),
        parallelCoords: static (f, p) => Fin.Succ<RenderPass>(new RenderPass.Sim(
            p.Key,
            target =>
                from data in p.Project(f)
                from count in p.Draw(target, data)
                select count)));

    private static Fin<int> Path(RenderTarget target, Fin<SKPath> path) =>
        target.Surface.ToFin(new ViewportFault.ContextUnavailable("sim/path: target has no Skia surface"))
            .Bind(surface => path.Map(scoped => {
                using SKPath owned = scoped;
                using SKPaint paint = new() { Style = SKPaintStyle.Stroke, IsAntialias = true };
                surface.Canvas.DrawPath(owned, paint);
                return owned.PointCount;
            }));

    // Strokes mirrors the Charts owner's Record walk: ascending distinct-ink bands, one ramp-resolved paint
    // per band, every path disposed after its draw — the strict seq is the stroke mint's own law.
    private static Fin<int> Strokes(RenderTarget target, Fin<Seq<VisualStroke>> strokes, Colormap ramp) =>
        target.Surface.ToFin(new ViewportFault.ContextUnavailable("sim/strokes: target has no Skia surface"))
            .Bind(surface => strokes.Bind(seq =>
                toSeq(seq.Map(static stroke => stroke.Ink).Distinct().OrderBy(static ink => (double)ink))
                    .Fold(Fin.Succ(0), (acc, ink) => acc.Bind(drawn =>
                        // Theme colormap rows are byte-sRGB, so the byte SKColor path states the truth here — the
                        // wide-gamut SKColorF entry is the Charts owner's, whose ramp is float end-to-end.
                        ramp.Sample((double)ink).Map(color => {
                            using SKPaint paint = new() { Style = SKPaintStyle.Stroke, IsAntialias = true, Color = new SKColor(color.R, color.G, color.B, color.A) };
                            int points = 0;
                            foreach (VisualStroke stroke in seq.Filter(s => s.Ink == ink)) {
                                using SKPath owned = stroke.Path;
                                surface.Canvas.DrawPath(owned, paint);
                                points += owned.PointCount;
                            }
                            return drawn + points;
                        })))));
}
```

## [04]-[VIEWPOINT_CODEC]

- Owner: `CameraFrame` the common eye/target/up product; `ViewCamera` `[Union]` the perspective-or-orthographic lens; `Viewpoint` the portable view-state receipt; optional `SectionBox` clip volume; `ViewMeasurement` the source-addressed snapped-measurement markup; `VisibilityOverride` the per-element visibility-and-color row; `VisibilityAction` `[SmartEnum]` the isolate/hide/x-ray/reset interaction fold over the one override vocabulary; `DiffClass` `[SmartEnum]` with `VersionGhost` the version-compare ghost projection onto the same rows; `ViewpointCodec` the projection binding the receipt to the `cs:Rasm.Bim/Review/issues#BCF_ARCHIVE` `BcfViewpoint` exchange contract, never an AppUi-local BCF viewpoint schema.
- Entry: `public string Encode(JsonSerializerOptions wire)` — serializes the camera, section box, visibility set, color overrides, and selection into one portable JSON receipt; `public static Fin<Viewpoint> Decode(string blob, JsonSerializerOptions wire)` — round-trips a stored or shared viewpoint.
- Auto: a viewpoint captures the full reproducible view state in one receipt — one `ViewCamera` case carries only its live lens scalar, `Option<SectionBox>` distinguishes absence from a real clip volume, visibility and color overrides key by element guid, and `ViewMeasurement` preserves the capture payload key and point-sample index behind every vertex. BCF projection maps the camera onto `BcfCamera`, visibility onto `VisibilityExceptions`/`DefaultVisibility`, color onto `BcfColoring`, section bounds onto six `BcfClippingPlane` rows, and measurement segments onto `BcfLine` rows.
- Receipt: `Viewpoint` serializes through the package wire context as a versioned portable receipt the dashboard, the markup, and the cross-process coordination consume.
- Law: `VisibilityAction` folds a selection onto the one `VisibilityOverride` vocabulary — `Isolate` hides every unselected element, `Hide` hides the selection, `Xray` ghosts the unselected rest through the transparency channel, `Reset` clears the override set — each row constructed with its fold delegate so the interactive state, the saved viewpoint, and the animation visibility track speak one visibility language and a viewer-local visibility model is the deleted form; the Shell verb binding raising these folds as `CommandIntent`s is `Shell/commands.md`'s row.
- Law: `VersionGhost.Project` maps a version-compare element classification (the Persistence `ReplayWindow`/commit-DAG fold arriving as `(ElementId, DiffClass)` values — AppUi runs no ledger read) onto diff-classed `VisibilityOverride` rows — `Added` tints, `Removed` ghosts at high transparency, `Modified` tints distinctly, `Unchanged` passes — so an A/B model comparison renders through the same override channel a viewpoint carries and a parallel ghost-overlay owner never exists.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, UnitsNet, NodaTime, Rasm.Bim (project), BCL inbox
- Growth: a new camera projection is one `ViewCamera` case; a new view-state field is one `Viewpoint` member; a new measurement attribute is one `ViewMeasurement` column; a new override channel is one `VisibilityOverride` column; a new interaction verb is one `VisibilityAction` row; a new diff classification is one `DiffClass` row; zero new surface.
- Boundary: `Viewpoint` is the one portable view-state owner for camera, optional section, visibility, color, selection, and measurements. `ViewpointCodec` projects onto Bim's `BcfViewpoint` family and preserves source snapshot, line, bitmap, index, setup-hint, visibility-convention, and arbitrary clipping-plane columns during re-encode. Arbitrary BCF plane sets do not counterfeit an axis-aligned `SectionBox`; decode carries `None` while the source record retains those planes. Render, collaboration issues, saved tours, and the browser residency wire consume the same receipt.

```csharp signature
public readonly record struct CameraFrame(
    System.Numerics.Vector3 Eye,
    System.Numerics.Vector3 Target,
    System.Numerics.Vector3 Up);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ViewCamera(CameraFrame Frame) {
    public sealed record Perspective(CameraFrame Frame, double FieldOfViewDeg) : ViewCamera(Frame);
    public sealed record Orthographic(CameraFrame Frame, double ViewHeight) : ViewCamera(Frame);

    // An XR eye frustum is asymmetric — four signed angles off the view axis in radians (left and
    // down negative, the OpenXR Fovf convention), never one symmetric field-of-view scalar — so the
    // immersive eye pass states its lens in the graph's own vocabulary rather than smuggling a Fovf
    // through a symmetric case that spells it wrong.
    public sealed record Asymmetric(CameraFrame Frame, double AngleLeft, double AngleRight, double AngleUp, double AngleDown) : ViewCamera(Frame);
}

public readonly record struct SectionBox(
    double MinX, double MinY, double MinZ,
    double MaxX, double MaxY, double MaxZ);

public readonly record struct VisibilityOverride(string ElementId, bool Visible, Option<uint> ColorArgb, double Transparency);

public readonly record struct ViewMeasurementPoint(UInt128 SourceKey, int SampleIndex, System.Numerics.Vector3 Position);

public sealed record ViewMeasurement(
    string Key,
    Seq<ViewMeasurementPoint> Vertices,
    UnitsNet.Length Total,
    Seq<UnitsNet.Angle> Angles);

public sealed record Viewpoint(
    string Key,
    int Version,
    ViewCamera Camera,
    Option<SectionBox> Section,
    Seq<VisibilityOverride> Overrides,
    Seq<string> Selection,
    Seq<ViewMeasurement> Measurements,
    Instant At) {
    public const int Schema = 2;

    public static Fin<Viewpoint> Capture(
        string key,
        ViewCamera camera,
        Option<SectionBox> section,
        Seq<VisibilityOverride> overrides,
        Seq<string> selection,
        Seq<ViewMeasurement> measurements,
        ClockPolicy clocks) =>
        overrides.Map(static o => o.ElementId).Distinct().Count == overrides.Count
            ? Fin.Succ(new Viewpoint(key, Schema, camera, section, overrides, selection, measurements, clocks.Now))
            : Fin.Fail<Viewpoint>(new ViewportFault.Text($"viewpoint/duplicate-override:{key}"));

    public string Encode(JsonSerializerOptions wire) => JsonSerializer.Serialize(this, wire);

    public static Fin<Viewpoint> Decode(string blob, JsonSerializerOptions wire) =>
        Optional(JsonSerializer.Deserialize<Viewpoint>(blob, wire)) is { IsSome: true, Case: Viewpoint view } && view.Version == Schema
            ? Fin.Succ(view)
            : Fin.Fail<Viewpoint>(new ViewportFault.Text("viewpoint/decode: blob version mismatch or malformed"));
}

// Isolate/hide/x-ray/reset interaction fold — each row constructed with its override-set delegate over
// (scene ids, selection), so the core BIM viewer loop is four rows on the ONE VisibilityOverride vocabulary the
// viewpoint captures and the animation visibility track steps; Shell/commands raises the verbs.
[SmartEnum<string>]
public sealed partial class VisibilityAction {
    public static readonly VisibilityAction Isolate = new("isolate", static (scene, picked) =>
        scene.Filter(id => !picked.Contains(id)).Map(static id => new VisibilityOverride(id, false, None, 0d)));
    public static readonly VisibilityAction Hide = new("hide", static (_, picked) =>
        picked.Map(static id => new VisibilityOverride(id, false, None, 0d)));
    public static readonly VisibilityAction Xray = new("xray", static (scene, picked) =>
        scene.Filter(id => !picked.Contains(id)).Map(static id => new VisibilityOverride(id, true, None, 0.85d)));
    public static readonly VisibilityAction Reset = new("reset", static (_, _) => Seq<VisibilityOverride>());

    [UseDelegateFromConstructor]
    public partial Seq<VisibilityOverride> Fold(Seq<string> scene, LanguageExt.HashSet<string> picked);
}

// Version-compare ghosting on the same override channel: each DiffClass row carries its tint and
// transparency as row DATA; the (ElementId, DiffClass) classification arrives as values off the Persistence
// version-compare fold, and Project maps it 1:1 onto VisibilityOverride rows the viewport already renders.
[SmartEnum<string>]
public sealed partial class DiffClass {
    public static readonly DiffClass Added = new("added", Some(0xFF2E7D32u), 0d);
    public static readonly DiffClass Removed = new("removed", Some(0xFFB71C1Cu), 0.7d);
    public static readonly DiffClass Modified = new("modified", Some(0xFFF9A825u), 0d);
    public static readonly DiffClass Unchanged = new("unchanged", Option<uint>.None, 0.6d);

    public Option<uint> TintArgb { get; }

    public double Transparency { get; }
}

public static class VersionGhost {
    public static Seq<VisibilityOverride> Project(Seq<(string ElementId, DiffClass Class)> classified) =>
        classified.Map(static row => new VisibilityOverride(row.ElementId, true, row.Class.TintArgb, row.Class.Transparency));
}

// Viewpoint <-> BCF projection binds the portable `Viewpoint` receipt to the one
// `Rasm.Bim.Coordination.BcfViewpoint` exchange contract the Bim owner mints — AppUi re-mints no
// BCF viewpoint schema: the camera crosses as the `BcfCamera` union (direction = target - eye),
// visibility rows where `Visible != DefaultVisibility` cross as `VisibilityExceptions`, colour as
// ARGB-hex `BcfColoring` rows, the present section box as its six outward axis planes; a `source`-
// carried re-encode `with`-preserves `Snapshot`/`Lines`/`Bitmaps`/`Index`/`ViewSetupHints` and the
// source visibility convention. Transparency stays render-only; inbound arbitrary clipping planes
// exceed the axis-box receipt, so decode disables the section and the re-encode keeps the planes.
public static class ViewpointCodec {
    public static Rasm.Bim.Coordination.BcfViewpoint ToBcf(string guid, Viewpoint view, Option<Rasm.Bim.Coordination.BcfViewpoint> source = default) =>
        (Frame: view.Camera.Frame,
         Default: source.Match(Some: static row => row.DefaultVisibility, None: static () => false),
         Aspect: source.Match(Some: static row => row.Camera.Switch(perspective: static p => p.AspectRatio, orthogonal: static o => o.AspectRatio), None: static () => 0d)) switch {
            var b => view.Camera.Switch(
                state: b,
                perspective: static (state, camera) => (Rasm.Bim.Coordination.BcfCamera)new Rasm.Bim.Coordination.BcfCamera.Perspective(
                    state.Frame.Eye, state.Frame.Target - state.Frame.Eye, state.Frame.Up, camera.FieldOfViewDeg, state.Aspect),
                orthographic: static (state, camera) => new Rasm.Bim.Coordination.BcfCamera.Orthogonal(
                    state.Frame.Eye, state.Frame.Target - state.Frame.Eye, state.Frame.Up, camera.ViewHeight, state.Aspect),
                // BCF carries no asymmetric frustum; an XR eye lens crosses as its symmetric vertical
                // envelope (AngleDown is signed negative, so the extent is the difference, converted
                // to the schema's degrees) — the exchange keeps the pose while the asymmetry stays a
                // live-session fact.
                asymmetric: static (state, camera) => new Rasm.Bim.Coordination.BcfCamera.Perspective(
                    state.Frame.Eye, state.Frame.Target - state.Frame.Eye, state.Frame.Up,
                    (camera.AngleUp - camera.AngleDown) * (180d / Math.PI), state.Aspect)) switch {
                var camera => source.Match(
                    Some: row => row with {
                        Camera = camera,
                        SelectedGlobalIds = view.Selection,
                        VisibilityExceptions = view.Overrides.Filter(o => o.Visible != b.Default).Map(static o => o.ElementId),
                        Coloring = ColoringOf(view.Overrides),
                        Lines = (row.Lines + LinesOf(view.Measurements)).Distinct(),
                        ClippingPlanes = view.Section.Match(PlanesOf, () => row.ClippingPlanes),
                    },
                    None: () => new Rasm.Bim.Coordination.BcfViewpoint(
                        guid, camera, view.Selection,
                        view.Overrides.Filter(o => o.Visible != b.Default).Map(static o => o.ElementId),
                        Option<ReadOnlyMemory<byte>>.None,
                        Coloring: ColoringOf(view.Overrides),
                        Lines: LinesOf(view.Measurements),
                        ClippingPlanes: view.Section.Match(PlanesOf, static () => Seq<Rasm.Bim.Coordination.BcfClippingPlane>()))),
            },
        };

    public static Viewpoint FromBcf(string key, Rasm.Bim.Coordination.BcfViewpoint bcf, ClockPolicy clocks) =>
        new(
            key, Viewpoint.Schema,
            bcf.Camera.Switch(
                perspective: static p => (ViewCamera)new ViewCamera.Perspective(
                    new CameraFrame(p.Position, p.Position + p.Direction, p.Up), p.FieldOfViewDeg),
                orthogonal: static o => new ViewCamera.Orthographic(
                    new CameraFrame(o.Position, o.Position + o.Direction, o.Up), o.ViewToWorldScale)),
            Option<SectionBox>.None,
            toSeq(bcf.Coloring.Fold(
                bcf.VisibilityExceptions.Fold(
                    HashMap<string, VisibilityOverride>(),
                    (acc, id) => acc.AddOrUpdate(id, new VisibilityOverride(id, !bcf.DefaultVisibility, None, 0d))),
                (acc, coloring) => uint.TryParse(coloring.Color.TrimStart('#'), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out uint argb)
                    ? coloring.GlobalIds.Fold(acc, (rows, id) => rows.AddOrUpdate(id, rows.Find(id).Match(
                        Some: row => row with { ColorArgb = Some(argb) },
                        None: () => new VisibilityOverride(id, bcf.DefaultVisibility, Some(argb), 0d))))
                    : acc))
                .Map(static entry => entry.Value),
            bcf.SelectedGlobalIds, Seq<ViewMeasurement>(), clocks.Now);

    private static Seq<Rasm.Bim.Coordination.BcfColoring> ColoringOf(Seq<VisibilityOverride> overrides) =>
        toSeq(overrides.Fold(
            HashMap<uint, Seq<string>>(),
            static (acc, o) => o.ColorArgb.Match(
                Some: argb => acc.AddOrUpdate(argb, acc.Find(argb).Match(Some: ids => ids.Add(o.ElementId), None: () => Seq(o.ElementId))),
                None: () => acc)))
            .Map(static row => new Rasm.Bim.Coordination.BcfColoring(
                row.Key.ToString("X8", System.Globalization.CultureInfo.InvariantCulture), row.Value));

    private static Seq<Rasm.Bim.Coordination.BcfLine> LinesOf(Seq<ViewMeasurement> measurements) =>
        measurements.Bind(static measurement => measurement.Vertices
            .Zip(measurement.Vertices.Tail)
            .Map(static pair => new Rasm.Bim.Coordination.BcfLine(pair.Item1.Position, pair.Item2.Position)));

    private static Seq<Rasm.Bim.Coordination.BcfClippingPlane> PlanesOf(SectionBox s) => Seq(
        new Rasm.Bim.Coordination.BcfClippingPlane(new System.Numerics.Vector3((float)s.MinX, 0f, 0f), new System.Numerics.Vector3(-1f, 0f, 0f)),
        new Rasm.Bim.Coordination.BcfClippingPlane(new System.Numerics.Vector3((float)s.MaxX, 0f, 0f), new System.Numerics.Vector3(1f, 0f, 0f)),
        new Rasm.Bim.Coordination.BcfClippingPlane(new System.Numerics.Vector3(0f, (float)s.MinY, 0f), new System.Numerics.Vector3(0f, -1f, 0f)),
        new Rasm.Bim.Coordination.BcfClippingPlane(new System.Numerics.Vector3(0f, (float)s.MaxY, 0f), new System.Numerics.Vector3(0f, 1f, 0f)),
        new Rasm.Bim.Coordination.BcfClippingPlane(new System.Numerics.Vector3(0f, 0f, (float)s.MinZ), new System.Numerics.Vector3(0f, 0f, -1f)),
        new Rasm.Bim.Coordination.BcfClippingPlane(new System.Numerics.Vector3(0f, 0f, (float)s.MaxZ), new System.Numerics.Vector3(0f, 0f, 1f)));
}
```

## [05]-[TS_PROJECTION]

- Owner: `ViewpointWire`, `ViewCameraWire`, `SectionBoxWire`, `VisibilityOverrideWire`, `FrameReceiptWire`, `GeometryResidencyWire`, `ResidencyTileWire`, `MeshoptStreamWire`, `MeshletWire` — the viewpoint, frame-evidence, and content-keyed geometry-residency wire contract a WebGPU web viewer and a cross-process coordination tool consume; `ResidencyManifest` the single C# mint of the `WEB_GEOMETRY_RESIDENCY_WIRE` portable scene-graph + kind-discriminated residency-tile manifest, each tile a 1:1 projection of one Compute `csharp:Rasm.Compute/Runtime/payload#RESIDENCY` `ResidencyPayload`; `ResidencyMarshal` the projection algebra folding each resident payload into its EXT_meshopt_compression wire row; the GPU pass internals and the suite `XxHash128` content key (minted by Compute, never re-computed here) never cross the wire.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm.Compute (project), BCL inbox
- Growth: one wire member row per new viewpoint field, frame-receipt field, or residency-tile field; a new residency kind or stream is one already-discriminated `ResidencyTileWire.kind` value or one `MeshoptStreamWire` row, never a new tile type; one `ResidencyMarshal` projection arm per new manifest member; zero new surface.
- Boundary: the wire emits strict camel-case JSON. `ViewCameraWire.projection` selects one live `scale` meaning, `section` is nullable, and `ViewMeasurementWire` carries content-keyed sample identities with unit-normalized evidence. Tile bounds cross as `[x, y, z, radius]`, content keys as `:x32` text, instants through `InstantPattern.ExtendedIso`, and durations as round-trip text. `ResidencyMarshal` projects every payload and viewpoint member at one seam; `ResidencyManifest.Encode` rejects unmapped members and enforces nullable annotations and required constructors.

```ts signature
type ViewCameraWire = {
  readonly projection: "perspective" | "orthographic";
  readonly eye: readonly [number, number, number];
  readonly target: readonly [number, number, number];
  readonly up: readonly [number, number, number];
  readonly scale: number;
};

interface SectionBoxWire {
  readonly min: readonly [number, number, number];
  readonly max: readonly [number, number, number];
}

interface VisibilityOverrideWire {
  readonly elementId: string;
  readonly visible: boolean;
  readonly colorArgb: number | null;
  readonly transparency: number;
}

interface ViewMeasurementWire {
  readonly key: string;
  readonly vertices: readonly {
    readonly sourceKey: string;
    readonly sampleIndex: number;
    readonly position: readonly [number, number, number];
  }[];
  readonly totalMeters: number;
  readonly anglesDegrees: readonly number[];
}

interface ViewpointWire {
  readonly key: string;
  readonly version: number;
  readonly camera: ViewCameraWire;
  readonly section: SectionBoxWire | null;
  readonly overrides: readonly VisibilityOverrideWire[];
  readonly selection: readonly string[];
  readonly measurements: readonly ViewMeasurementWire[];
  readonly at: string;
}

interface FrameReceiptWire {
  readonly ordinal: number;
  readonly backend: string;
  readonly passes: readonly { readonly pass: string; readonly elapsed: string }[];
  readonly gpu: string;
  readonly triangles: number;
  readonly withinBudget: boolean;
  readonly at: string;
  readonly correlation: string;
}

interface MeshoptStreamWire {
  readonly stream: string;
  readonly mode: "ATTRIBUTES" | "TRIANGLES" | "INDICES" | "RAW";
  readonly filter: "NONE" | "OCTAHEDRAL" | "QUATERNION" | "EXPONENTIAL";
  readonly byteOffset: number;
  readonly byteLength: number;
  readonly count: number;
  readonly byteStride: number;
  readonly codecVersion: number;
}

interface MeshletWire {
  readonly vertexOffset: number;
  readonly triangleOffset: number;
  readonly vertexCount: number;
  readonly triangleCount: number;
  readonly center: readonly [number, number, number];
  readonly radius: number;
  readonly coneApex: readonly [number, number, number];
  readonly coneAxis: readonly [number, number, number];
  readonly coneCutoff: number;
  readonly level: number;
  readonly parent: number;
  readonly shell: number;
  readonly error: number;
  readonly parentError: number;
}

interface ResidencyTileWire {
  readonly kind: "meshlet-cluster" | "quantized-vertex" | "point-splat" | "gaussian-splat";
  readonly contentKey: string;
  readonly blobKey: string;
  readonly bytes: number;
  readonly residentCount: number;
  readonly harmonicDegree: number;
  readonly bounds: readonly [number, number, number, number];
  readonly streams: readonly MeshoptStreamWire[];
  readonly meshlets: readonly MeshletWire[];
}

interface GeometryResidencyWire {
  readonly version: number;
  readonly viewpoint: ViewpointWire;
  readonly tiles: readonly ResidencyTileWire[];
  readonly vramBudget: number;
}
```

`ResidencyManifest` is the single C# mint of `WEB_GEOMETRY_RESIDENCY_WIRE`; the TypeScript worker consumes it by content key and never re-mints identity. `ResidencyMarshal` projects each admitted Compute `ResidencyPayload` kind through one total fold, and `KeyHex` renders the producer-owned `UInt128` as the shared `:x32` wire value.

```csharp signature
public readonly record struct ViewCameraWire(
    string Projection,
    System.Collections.Immutable.ImmutableArray<double> Eye,
    System.Collections.Immutable.ImmutableArray<double> Target,
    System.Collections.Immutable.ImmutableArray<double> Up,
    double Scale);

public readonly record struct SectionBoxWire(
    System.Collections.Immutable.ImmutableArray<double> Min,
    System.Collections.Immutable.ImmutableArray<double> Max);

public readonly record struct VisibilityOverrideWire(string ElementId, bool Visible, uint? ColorArgb, double Transparency);

public sealed record ViewMeasurementPointWire(
    string SourceKey,
    int SampleIndex,
    System.Collections.Immutable.ImmutableArray<double> Position);

public sealed record ViewMeasurementWire(
    string Key,
    Seq<ViewMeasurementPointWire> Vertices,
    double TotalMeters,
    Seq<double> AnglesDegrees);

public sealed record ViewpointWire(
    string Key,
    int Version,
    ViewCameraWire Camera,
    SectionBoxWire? Section,
    Seq<VisibilityOverrideWire> Overrides,
    Seq<string> Selection,
    Seq<ViewMeasurementWire> Measurements,
    string At);

public readonly record struct MeshoptStreamWire(string Stream, string Mode, string Filter, int ByteOffset, int ByteLength, int Count, int ByteStride, int CodecVersion);

public sealed record MeshletWire(
    int VertexOffset, int TriangleOffset, int VertexCount, int TriangleCount,
    System.Collections.Immutable.ImmutableArray<double> Center,
    double Radius,
    System.Collections.Immutable.ImmutableArray<double> ConeApex,
    System.Collections.Immutable.ImmutableArray<double> ConeAxis,
    double ConeCutoff,
    int Level, int Parent, int Shell, double Error, double ParentError);

public sealed record ResidencyTileWire(
    string Kind, string ContentKey, string BlobKey, long Bytes, int ResidentCount,
    int HarmonicDegree,
    System.Collections.Immutable.ImmutableArray<double> Bounds,
    Seq<MeshoptStreamWire> Streams,
    Seq<MeshletWire> Meshlets);

public static class ResidencyMarshal {
    public static string KeyHex(UInt128 content) => content.ToString("x32", System.Globalization.CultureInfo.InvariantCulture);

    public static string BlobKeyOf(UInt128 content) => $"geo/{KeyHex(content)}";

    public static ViewpointWire ViewpointOf(Viewpoint view) =>
        new(view.Key, view.Version,
            view.Camera.Switch(
                perspective: static camera => new ViewCameraWire(
                    "perspective",
                    [camera.Frame.Eye.X, camera.Frame.Eye.Y, camera.Frame.Eye.Z],
                    [camera.Frame.Target.X, camera.Frame.Target.Y, camera.Frame.Target.Z],
                    [camera.Frame.Up.X, camera.Frame.Up.Y, camera.Frame.Up.Z],
                    camera.FieldOfViewDeg),
                orthographic: static camera => new ViewCameraWire(
                    "orthographic",
                    [camera.Frame.Eye.X, camera.Frame.Eye.Y, camera.Frame.Eye.Z],
                    [camera.Frame.Target.X, camera.Frame.Target.Y, camera.Frame.Target.Z],
                    [camera.Frame.Up.X, camera.Frame.Up.Y, camera.Frame.Up.Z],
                    camera.ViewHeight),
                // The wire's one lens scalar carries the symmetric vertical envelope in degrees; the
                // browser leg renders a review view, never a stereo eye, so the asymmetry stays local.
                asymmetric: static camera => new ViewCameraWire(
                    "perspective",
                    [camera.Frame.Eye.X, camera.Frame.Eye.Y, camera.Frame.Eye.Z],
                    [camera.Frame.Target.X, camera.Frame.Target.Y, camera.Frame.Target.Z],
                    [camera.Frame.Up.X, camera.Frame.Up.Y, camera.Frame.Up.Z],
                    (camera.AngleUp - camera.AngleDown) * (180d / Math.PI))),
            view.Section.Map(static section => new SectionBoxWire(
                [section.MinX, section.MinY, section.MinZ],
                [section.MaxX, section.MaxY, section.MaxZ])).ToNullable(),
            view.Overrides.Map(static o => new VisibilityOverrideWire(o.ElementId, o.Visible, o.ColorArgb.ToNullable(), o.Transparency)),
            view.Selection,
            view.Measurements.Map(static measurement => new ViewMeasurementWire(
                measurement.Key,
                measurement.Vertices.Map(static point => new ViewMeasurementPointWire(
                    KeyHex(point.SourceKey), point.SampleIndex, [point.Position.X, point.Position.Y, point.Position.Z])),
                measurement.Total.Meters,
                measurement.Angles.Map(static angle => angle.Degrees))),
            InstantPattern.ExtendedIso.Format(view.At));

    // Compute owns every decode parameter, including the per-stream codec version.
    public static MeshoptStreamWire StreamWire(ResidencyStream stream, StreamSpan span) =>
        new(stream.Key, span.Mode.Key, span.Filter.Key, span.Offset, span.Length, span.Count, span.ByteStride, span.CodecVersion);

    public static MeshletWire MeshletWireOf(ResidencyMeshlet m) =>
        new(m.VertexOffset, m.TriangleOffset, m.VertexCount, m.TriangleCount,
            [m.Center.X, m.Center.Y, m.Center.Z], m.Radius,
            [m.ConeApex.X, m.ConeApex.Y, m.ConeApex.Z], [m.ConeAxis.X, m.ConeAxis.Y, m.ConeAxis.Z], m.ConeCutoff,
            m.Level, m.Parent, m.Shell, m.Error, m.ParentError);

    // one residency tile wire row = one Compute ResidencyPayload projected 1:1 — the content/blob key is the payload's
    // own XxHash128 (never re-hashed off raw positions), the EXT_meshopt_compression bufferViews are the payload's
    // StreamSpan layout, the cone-cull clusters are its meshopt-built ResidencyMeshlet set (vertex-table + triangle
    // split), and the bounds is its self-described sphere. The tile carries ONE identity — the producer's own
    // UInt128 content key — rendered to hex once at this wire edge; a second string placement key beside it would be
    // the same value under two names and a residency map keyed on it would key on a value no residency owner mints.
    public static ResidencyTileWire TileOf(ResidencyPayload payload) =>
        new(payload.Kind.Key, KeyHex(payload.ContentKey), BlobKeyOf(payload.ContentKey),
            payload.EncodedBytes, payload.ResidentCount, payload.HarmonicDegree,
            [payload.Center.X, payload.Center.Y, payload.Center.Z, payload.Radius],
            toSeq(payload.Layout.OrderBy(static slot => slot.Value.Offset).Select(static slot => StreamWire(slot.Key, slot.Value))),
            payload.Clusters.Map(MeshletWireOf));
}

public sealed record ResidencyManifest(
    int Version,
    ViewpointWire Viewpoint,
    Seq<ResidencyTileWire> Tiles,
    long VramBudget) {
    public const int Schema = 2;

    // Mint joins the AppUi residency decision (which content-addressed payloads are resident)
    // with the Compute ResidencyPayload codec (the EXT_meshopt_compression streams, clusters, bounds, content key) —
    // a pure projection of the Compute payload, never re-deriving geometry, content keys, or streams from
    // AppUi-internal owners; a resident scene tile with no matching payload is dropped, never re-hashed.
    public static ResidencyManifest Mint(
        Viewpoint viewpoint,
        ResidencyPlan plan,
        HashMap<UInt128, ResidencyPayload> payloads,
        long vramBudget) =>
        new(Schema, ResidencyMarshal.ViewpointOf(viewpoint),
            plan.Resident.Choose(tile => payloads.Find(tile.ContentKey).Map(ResidencyMarshal.TileOf)),
            vramBudget);

    public string Encode() => JsonSerializer.Serialize(this, ResidencyWireContext.Default.ResidencyManifest);
}

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
    RespectNullableAnnotations = true,
    RespectRequiredConstructorParameters = true)]
[JsonSerializable(typeof(ResidencyManifest))]
public partial class ResidencyWireContext : JsonSerializerContext;
```

## [06]-[GPU_AND_WIRE_BOUNDARY]

- [VIEWPORT_GPU]: `GpuBackend.Target` absorbs Ganesh, raster, Wgpu, and browser target construction over the closed `GpuBinding` union, every arm reading the one `RenderTargetRequest` the resolve row derived and answering the sample count its allocation GRANTED. `RenderGraph` advances `ResolveState`, leases one active target at the requested extent, threads one `FrameView` into the cull and geometry arms, executes the pass DAG over one fold-carried cut, and seals measured `WgpuFrameEvidence`; meshlet, path-trace, resolve, and simulation acceleration remain pass delegates under that lease and create no parallel device or target owner.
- [WGPU_BACKEND]: `WgpuPresentation` discriminates exclusive swapchain presentation from compositor import; its composited arm selects the sync mode `GetSynchronizationCapabilities` grants, awaits `ImportCompleted`, rejects every `IsLost` state, and submits through the matched `CompositionDrawingSurface.UpdateWith*Async` member. Timestamp resolve, buffer map, queue submission, and device polling retire through the one `WgpuFrameEvidence` lane.
- [WEB_RESIDENCY]: `ResidencyManifest` is the single C# mint of the browser residency wire. `ResidencyMarshal` projects Compute `ResidencyPayload` stream spans, meshlet hierarchy, bounds, content keys, and admitted splat tiles into one content-addressed manifest; the browser consumes that wire and never re-mints payload identity, hierarchy, or blob keys.

## [07]-[RESEARCH]

(none)
