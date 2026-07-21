# [APPUI_RENDER_PIPELINE]

The GPU render pipeline for the infinite viewport: one `RenderGraph` pass-DAG drives every frame over a host-shared `GRContext` leased through the embed capsule, the `GpuBackend` rows carry the per-backend target-construction delegate (`Target`) over the composition-bound `GpuBinding` union so backend identity derives from the binding and a mismatched backend-factory pair is unrepresentable, the `ResolvePass` ladder selects the antialias-and-super-resolution resolve, `SimVisual` renders isosurface/volume/streamline/glyph/deformation fields off the Compute field receipts, and `Viewpoint` codecs camera, section, visibility, override, selection, and source-addressed measurements as one portable BCF-compatible receipt. The page owns the render-graph pass algebra, the backend vocabulary with its target factory column, the measured GPU-time evidence lane, the resolve ladder, the simulation render passes, the viewpoint receipt with its visibility-action and version-ghost folds, and the residency wire projection; the geometry-virtualization and residency owners live in `Render/meshlets`, the path-trace integrator in `Render/pathtrace`. The substrate is SkiaSharp 3 GPU backends (`GRContext`, `GRMtlBackendContext`, `GRVkBackendContext`, `SKRuntimeEffect`) leased through `ISkiaSharpApiLease`, the `Silk.NET.WebGPU` wgpu/Dawn target factory, the Compute geometry and field receipts, and the AppHost clock, frame-budget, and receipt-sink ports. The GPU passes share the host GPU context, and the `Software` 2D-Skia raster is the deterministic CPU floor.

## [01]-[INDEX]

- [02]-[RENDER_GRAPH]: Frame pass-DAG, the `GpuBackend` target-factory column over `GpuBinding`, resolve ladder, frame-budget invariant, fallback.
- [03]-[SIM_VISUAL]: Isosurface, volume, streamline, glyph, deformation field render passes off the Compute receipts.
- [04]-[VIEWPOINT_CODEC]: Camera, section-box, visibility, override, selection projecting onto the `Rasm.Bim` `BcfViewpoint` exchange contract; the isolate/hide/x-ray/reset action fold and the version-diff ghost projection on the one override vocabulary.
- [05]-[TS_PROJECTION]: Viewpoint, frame-evidence, and content-keyed geometry-residency wire contract.
- [06]-[GPU_AND_WIRE_BOUNDARY]: Viewport GPU lease law, the wgpu presentation arms, and the web residency mint.

## [02]-[RENDER_GRAPH]

- Owner: `RenderPass` `[Union]` frame-pass vocabulary; `RenderGraph` pass-DAG executor; `GpuBackend` `[SmartEnum]` the backend vocabulary whose rows CARRY the target-construction delegate; `GpuBinding` `[Union]` the composition-bound substrate each backend row folds; `RenderTarget` the lease-bound GPU surface; `WgpuFrameEvidence` the timestamp-query GPU-time lane; `FrameReceipt` per-frame evidence; `ViewportFault` the fault family; `ResolvePass` `[SmartEnum]` the antialias-and-super-resolution resolve ladder the `Composite` pass selects; `ResolvePolicy` the per-tier delegate-row binding.
- Cases: `RenderPass` = Cull | Geometry | PathTrace | Composite | Sim | Overlay under the locked kind literals cull, geometry, path-trace, composite, sim, overlay; `ResolvePass` = Msaa | Taa | Fsr | Smaa under the locked policy literals; `ViewportFault` = Text | ContextUnavailable | BackendUnsupported | BudgetExceeded | LeaseRejected — codes derive through the `AppUiFaultBand.Viewport` registry row (6100), shared with pathtrace.
- Entry: `public IO<FrameReceipt> Frame(ViewportClock clock, FrameBudget budget, int tierRank, Func<RenderPass, bool> passMask, ViewCamera camera)` on `RenderGraph` — `IO` rail; the pass-DAG executes topologically under the frame camera, the governor tier rank, and the verdict's `PassMask` disposition (the `Diagnostics/governor.md` `QualityTier.PassMask` column, so a degraded tier's pass set is data, never a caller convention), and the frame seals one receipt carrying the per-pass elapsed, the deferred-pass set, and the GPU-time fold.
- Auto: `Lease` opens the host-shared GPU context through `ISkiaSharpApiLease.TryLeasePlatformGraphicsApi` and folds the leased context to the `RenderTarget` through the bound `GpuBinding`'s own backend row (`binding.Backend.Target(binding, info)` — the `[UseDelegateFromConstructor]` factory column each row was constructed with), so a pass-emit body binds a backend-provided target rather than the single `GRContext`-plus-`SKRuntimeEffect` emit path and the embedded viewport composites into the Rhino-owned context and never mints a second `GRContext`; when the platform lease yields no GPU context (`LeaseRejected`/`ContextUnavailable`) the frame re-runs through the `Fallback` binding — `GpuBinding.Raster`, the `Software` row's CPU 2D-Skia floor with the pass list filtered to `Composite`/`Overlay` — so the viewport renders a deterministic CPU frame through the same fold and receipt; the frame-budget invariant executes inside the pass fold — a pass starting past `FrameBudget.Frame`, or landing on a triangle fold already past `MaxTriangles`, DEFERS to the next frame (recorded in the receipt's `Deferred` column, folded onto the budget-overrun instrument), and `WithinBudget` derives from the measured elapsed and the deferral set, never an initialized-true flag.
- Backend: each `GpuBackend` row is CONSTRUCTED with its target-construction delegate — `Metal`, `Vulkan`, and `OpenGl` fold the `GpuBinding.Ganesh` leased `GRContext` to `SKSurface.Create(GRRecordingContext, GRBackendRenderTarget, ...)`, `Software` folds `GpuBinding.Raster` to the CPU `SKSurface.Create(SKImageInfo)` floor, `Wgpu` folds `GpuBinding.Wgpu` — the `Silk.NET.WebGPU` wgpu/Dawn substrate (D3D12/Metal/Vulkan auto-negotiated through `BackendType`) whose `Adapter` matched the compositor adapter LUID/UUID at composition, its `Device`+`Queue` shared branch-wide, DISCRIMINATING the presentation arm on the binding's `WgpuPresentation` — the in-tree composited viewport IMPORTS the rendered texture through the compositor interop family (`ICompositionGpuInterop.ImportImage`/`ImportSemaphore` then `CompositionDrawingSurface.UpdateWithKeyedMutexAsync`/`UpdateWithSemaphoresAsync`/`UpdateWithTimelineSemaphoresAsync` per `GetSynchronizationCapabilities`; a second swapchain in composited mode is the DELETED form), while `SurfaceConfigure`/`SurfaceGetCurrentTexture` survives ONLY as the exclusive-fullscreen/headless arm — the wgpu mesh-shader/compute passes record through `CommandEncoder`/`RenderPassEncoder` and submit through `QueueSubmit`, never a managed scene wrapper — and `WebGpu` folds `GpuBinding.Browser`, the in-browser WebGPU surface the TS web leg consumes; `GpuBinding.Backend` DERIVES the backend row from the binding case, so `RenderGraph` holds bindings alone, a backend paired with a foreign substrate cannot be constructed, and a substrate swap is one backend row plus its binding case; the per-backend emit path (wgpu pipeline submit versus `SKRuntimeEffect` shader) diverges inside the row delegates, so the vocabulary owns the divergence and the CPU 2D-Skia fallback is the floor.
- Resolve: the `Composite` pass selects one `ResolvePass` policy row after the geometry and path-trace passes — `Taa` jitters the camera sub-pixel per frame and reprojects the prior frame (`ResolveState.History`/`Jitter` threaded into the `composite` delegate) through the motion-vector buffer under a neighborhood-clamp history rejection so a static scene converges and a moving scene ghosts no tail, `Smaa` runs the morphological edge AA, `Msaa` multi-samples the raster, and `Fsr` renders sub-resolution (`RenderScale` 0.6) under the `Render/meshlets` `ResidencyBudget` VRAM bound and spatially upscales to display resolution so a 4K viewport renders at a fraction of the pixel cost; `ResolvePolicy` binds each `PERF_BUDGET` `QualityTier` rank to its `ResolvePass` through the frozen `int -> ResolvePass` table (`ByTier`, ranks 4..0) so the governor steps the full ladder `Taa(4,3) -> Smaa(2) -> Msaa(1) -> Fsr(0)` on the same hysteresis band that degrades the render passes — the high tiers spend pixels on temporal quality and the floor tier trades resolution for budget; the ladder EXECUTES inside the frame fold: `Policy.For(tierRank)` selects the pass, `ResolvePass.Advance` steps the graph-held `ResolveState` (ordinal, Halton jitter, history, render scale, frame camera) once per frame — a camera that moved since the prior frame resets the history and the path-trace film in the same transition — and the `Composite` arm runs `ResolvePass.Resolve(target, state, raster)` so the composite delegate receives the jitter-and-history state it reprojects with; the `Taa` motion-vector buffer is ONE `Render/meshlets` `BindlessTable` slot, never a parallel motion-vector owner; the resolve is a `Composite` policy column and a parallel post-process engine is the deleted form.
- Receipt: `FrameReceipt` — frame ordinal, per-pass `Duration` seq, GPU `Duration`, triangles drawn, budget verdict, `Instant`, `CorrelationId`; the GPU column is MEASURED evidence off the `WgpuFrameEvidence` timestamp lane (`QueryType.Timestamp` `DeviceCreateQuerySet`, per-pass `RenderPassTimestampWrites`/`ComputePassTimestampWrites`, `CommandEncoderResolveQuerySet` into the read buffer, `BufferMapAsync`/`BufferGetMappedRange`/`BufferUnmap` readback, `QuerySetRelease` teardown), never the CPU elapsed re-labelled — a binding without the `timestamp-query` feature binds `None` and the column carries the honest `Duration.Zero`, while a FAILED readback keeps the zero and lands its fault on the receipt fault rail so unsupported and failed never conflate; the `Diagnostics/governor.md` `GpuTimeline.Migrate` deepens the same column from the lane-measured frame duration to per-pass resolved nanoseconds only when EVERY pass resolved its timestamp pair — a mixed projected/measured sum never enters the measured column; frame retirement rides `QueueSubmitForIndex` minting the `WrappedSubmissionIndex` that `DevicePoll` advances without a blocking fence, so cull-to-draw and readback never stall the queue; sealed through `ReceiptSinkPort` as a `Render`-family fact; `TelemetryRow` contributes the frame-elapsed, gpu-elapsed, and budget-overrun instruments inward through `TelemetryContributorPort`.
- Packages: SkiaSharp, Avalonia.Skia, Avalonia (compositor GPU interop), Silk.NET.WebGPU, Silk.NET.WebGPU.Native.WGPU, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm.AppHost (project)
- Growth: a new frame stage is one `RenderPass` case breaking the topological dispatch at compile time; a new backend is one `GpuBackend` row constructed with its target delegate plus its `GpuBinding` case — Skia Graphite re-admits as one `SkiaGraphite` row the moment SkiaSharp ships its Recorder/Context surface; zero new surface.
- Boundary: `RenderGraph` is the named boundary capsule — the lease open-and-dispose pair and the topological pass walk carry the only statement bodies; the shared GPU context arrives as one `SurfaceSeam`-bound platform-lease delegate so no pass body names a `GRContext.CreateMetal`/`CreateVulkan` factory at a call site, deferring to the surface-hosts `EMBED_CAPSULE` shared-context law — a direct GPU-backend construction inside a pass arm is the rejected form (PROHIBITION host-API-in-arm); the per-backend target construction LIVES ON the `GpuBackend` row as its constructor delegate — the `Metal`/`Vulkan`/`OpenGl` rows fold the leased `GRContext` to `SKSurface.Create(GRRecordingContext, GRBackendRenderTarget, ...)`, the `Wgpu` row folds the `Silk.NET.WebGPU` `Device`/`Queue`/`Surface` wgpu swapchain presenting through the compositor `ICompositionGpuInterop.ImportImage` seam, and the `WebGpu` row folds the browser surface — a detached factory record pairable with a foreign backend is the deleted form, so a pass-emit body never names a backend target factory at a call site and a substrate swap is one backend row; the GPU passes (`Geometry` cluster draw through the wgpu mesh-shader pipeline, `PathTrace` through the wgpu compute pass, `Sim` volume ray-march, the reality-capture `Splat`/`Point` composites) SPIKE-gate on the live host-shared context and the `Composite` 2D-Skia raster is the deterministic CPU fallback; `ViewportClock` rides the AppHost `ClockPolicy` so frame timing is the one clock seam and a stopwatch is the rejected form; the frame ordinal is a monotone `Interlocked.Increment` over the graph-local counter so each `FrameReceipt` carries a distinct ordinal the correlation join and the render-hash lane key on, and a hardcoded zero ordinal is the deleted form; the receipt carries the folded per-pass list, the deferred-pass set, and a `WithinBudget` verdict derived from the measured elapsed against `FrameBudget.Frame` plus the triangle ceiling, so an overrun frame seals `WithinBudget: false` with its deferrals named rather than an unconditional true, and every frame sinks one `FrameReceipt` through the one envelope and a per-pass meter is the deleted form; GPU validation on the `Wgpu` arm rides the error-scope rail — `DeviceSetUncapturedErrorCallback` installs once at device acquisition and `WgpuErrorScope` brackets suspect pass encoding through `DevicePushErrorScope`/`DevicePopErrorScope`, so a validation or out-of-memory error is a counted `ViewportFault` on the telemetry spine, never a swallowed native abort; the meshlet cluster the graph draws is the `Render/meshlets` owner and the path-trace pass the `Render/pathtrace` integrator, so the pipeline composes them and re-models neither.

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

// Backend rows CARRY their target construction: the [UseDelegateFromConstructor] column folds the
// composition-bound GpuBinding case to a RenderTarget, and a binding case a row does not own is the typed
// BackendUnsupported fault — behavior recovers from the selected row, never from a detached factory record.
[SmartEnum<string>]
public sealed partial class GpuBackend {
    public static readonly GpuBackend Metal = new("metal", GpuFamily.SkiaGanesh, GaneshTarget);
    public static readonly GpuBackend Vulkan = new("vulkan", GpuFamily.SkiaGanesh, GaneshTarget);
    public static readonly GpuBackend OpenGl = new("opengl", GpuFamily.SkiaGanesh, GaneshTarget);
    public static readonly GpuBackend Software = new("software", GpuFamily.SkiaRaster, RasterTarget);
    public static readonly GpuBackend Wgpu = new("wgpu", GpuFamily.Wgpu, WgpuTarget);
    public static readonly GpuBackend WebGpu = new("webgpu", GpuFamily.WebGpu, BrowserTarget);

    public GpuFamily Family { get; }

    public bool IsGpu => Family != GpuFamily.SkiaRaster;

    [UseDelegateFromConstructor]
    public partial Fin<RenderTarget> Target(GpuBinding binding, SKImageInfo info);

    private static Fin<RenderTarget> GaneshTarget(GpuBinding binding, SKImageInfo info) => binding switch {
        GpuBinding.Ganesh ganesh => ganesh.Lease(info),
        _ => Fin.Fail<RenderTarget>(new ViewportFault.BackendUnsupported($"{binding.Backend.Key}: not a Ganesh binding")),
    };

    private static Fin<RenderTarget> RasterTarget(GpuBinding binding, SKImageInfo info) => binding switch {
        GpuBinding.Raster => SKSurface.Create(info) switch {
            { } surface => Fin.Succ(new RenderTarget(Software, Some(surface), None, info, surface)),
            _ => Fin.Fail<RenderTarget>(new ViewportFault.ContextUnavailable("software: raster surface allocation failed")),
        },
        _ => Fin.Fail<RenderTarget>(new ViewportFault.BackendUnsupported($"{binding.Backend.Key}: not the raster floor")),
    };

    private static Fin<RenderTarget> WgpuTarget(GpuBinding binding, SKImageInfo info) => binding switch {
        GpuBinding.Wgpu wgpu => wgpu.Acquire(wgpu.Presentation, info),
        _ => Fin.Fail<RenderTarget>(new ViewportFault.BackendUnsupported($"{binding.Backend.Key}: not a wgpu binding")),
    };

    private static Fin<RenderTarget> BrowserTarget(GpuBinding binding, SKImageInfo info) => binding switch {
        GpuBinding.Browser browser => browser.Acquire(browser.Surface, info),
        _ => Fin.Fail<RenderTarget>(new ViewportFault.BackendUnsupported($"{binding.Backend.Key}: not a browser binding")),
    };
}

[SmartEnum<string>]
public sealed partial class GpuFamily {
    public static readonly GpuFamily SkiaGanesh = new("skia-ganesh");
    public static readonly GpuFamily SkiaRaster = new("skia-raster");
    public static readonly GpuFamily Wgpu = new("wgpu");
    public static readonly GpuFamily WebGpu = new("webgpu");
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
        private Ganesh(GpuBackend row, Func<SKImageInfo, Fin<RenderTarget>> lease) { Row = row; Lease = lease; }

        public GpuBackend Row { get; }
        public Func<SKImageInfo, Fin<RenderTarget>> Lease { get; }

        public static Fin<GpuBinding> Of(GpuBackend row, Func<SKImageInfo, Fin<RenderTarget>> lease) =>
            row.Family == GpuFamily.SkiaGanesh
                ? Fin.Succ<GpuBinding>(new Ganesh(row, lease))
                : Fin.Fail<GpuBinding>(new ViewportFault.BackendUnsupported($"{row.Key}: not a Ganesh row"));
    }

    public sealed record Raster : GpuBinding;

    public sealed record Wgpu(WgpuDevice Device, WgpuPresentation Presentation, Func<WgpuPresentation, SKImageInfo, Fin<RenderTarget>> Acquire) : GpuBinding;

    public sealed record Browser(nint Surface, Func<nint, SKImageInfo, Fin<RenderTarget>> Acquire) : GpuBinding;

    public GpuBackend Backend => this switch {
        Ganesh ganesh => ganesh.Row,
        Wgpu => GpuBackend.Wgpu,
        Browser => GpuBackend.WebGpu,
        _ => GpuBackend.Software,
    };

    public Fin<RenderTarget> Target(SKImageInfo info) => Backend.Target(this, info);
}

// The WGPU presentation dispatch the Wgpu backend row routes through: Composited imports the
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

    public sealed record Headless(SKImageInfo Info) : WgpuPresentation;

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

    // Per-frame refresh awaits import completion, rejects lost interop/image/semaphore handles, then uses
    // the capability-discriminated keyed-mutex, timeline, binary-semaphore, or Automatic update arm.
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

// The composition-bound GPU-time lane over the shared device: Measure brackets one frame — a
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
    public sealed record Cull(string Key, Func<RenderTarget, MeshletCluster, ViewCamera, Fin<(MeshletCluster Cluster, CullResult Result)>> Visible) : RenderPass(Key);
    public sealed record Geometry(string Key, Func<RenderTarget, MeshletCluster, int, Fin<int>> Draw) : RenderPass(Key);
    public sealed record PathTrace(string Key, PathTracePass Pass, Atom<AccumulationTarget> Film, LightRig Rig, int SampleBudget, long Seed) : RenderPass(Key);
    public sealed record Sim(string Key, Func<RenderTarget, Fin<int>> Draw) : RenderPass(Key);
    public sealed record Composite(string Key, Func<SKCanvas, ResolveState, Fin<Unit>> Raster) : RenderPass(Key);
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

    private static readonly (double X, double Y)[] HaltonJitter =
        [(0.5, 0.333), (0.25, 0.667), (0.75, 0.111), (0.125, 0.444), (0.625, 0.778), (0.375, 0.222), (0.875, 0.556), (0.0625, 0.889)];

    // Camera motion invalidates temporal history: a moved camera re-seeds the ordinal and drops History
    // in the same transition, so TAA never reprojects a stale frame and accumulation resets coherently.
    public ResolveState Advance(ResolveState prior, ViewCamera camera) =>
        (prior.Camera.Map(held => held == camera).IfNone(false) ? prior : prior with { Ordinal = 0L, History = None }) switch {
            var seeded => Reproject
                ? seeded with {
                    Ordinal = seeded.Ordinal + 1,
                    Jitter = HaltonJitter[(int)((seeded.Ordinal + 1) % HaltonJitter.Length)],
                    History = seeded.History,
                    RenderScale = RenderScale,
                    Camera = Some(camera),
                }
                : seeded with { Ordinal = seeded.Ordinal + 1, Jitter = (0d, 0d), History = None, RenderScale = RenderScale, Camera = Some(camera) },
        };

    public Fin<Unit> Resolve(RenderTarget target, ResolveState state, Func<SKCanvas, ResolveState, Fin<Unit>> composite) =>
        target.Surface.Match(
            Some: surface => composite(surface.Canvas, state),
            None: () => Fin.Fail<Unit>(new ViewportFault.ContextUnavailable($"resolve/{Key}: no resolve surface")));
}

public sealed record ResolvePolicy(FrozenDictionary<int, ResolvePass> ByTier) {
    public static readonly ResolvePolicy Default = new(new[] {
        KeyValuePair.Create(4, ResolvePass.Taa),
        KeyValuePair.Create(3, ResolvePass.Taa),
        KeyValuePair.Create(2, ResolvePass.Smaa),
        KeyValuePair.Create(1, ResolvePass.Msaa),
        KeyValuePair.Create(0, ResolvePass.Fsr),
    }.ToFrozenDictionary());

    public ResolvePass For(int tierRank) =>
        ByTier.TryGetValue(Math.Clamp(tierRank, 0, 4), out ResolvePass? pass) ? pass : ResolvePass.Msaa;
}

public sealed record RenderTarget(GpuBackend Backend, Option<SKSurface> Surface, Option<GRContext> Context, SKImageInfo Info, IDisposable Native) : IDisposable {
    public void Dispose() => Native.Dispose();
}

public sealed record FrameBudget(Duration Frame, long VramBytes, int MaxTriangles) {
    public static readonly FrameBudget Sixty = new(Duration.FromMilliseconds(16.667), 1_073_741_824L, 20_000_000);
    public static readonly FrameBudget Thirty = new(Duration.FromMilliseconds(33.333), 536_870_912L, 8_000_000);
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
    Func<GpuBinding, Func<RenderTarget, Fin<FrameReceipt>>, Fin<FrameReceipt>> Lease,
    Func<FrameReceipt, IO<Unit>> Sink) {
    private long ordinal;
    private readonly Atom<ResolveState> resolve = Atom(new ResolveState(0L, (0d, 0d), None, 1.0, None));

    // The interlocked ordinal threads through EVERY arm — the GPU fold, the fallback re-lease, and the
    // Empty fault path — so no receipt is ever constructed with a literal zero ordinal. A lease-class
    // fault re-runs the frame through the Fallback binding over the Composite/Overlay passes, so the
    // software floor is a reachable arm of this fold, never an inert constructor field.
    public IO<FrameReceipt> Frame(ViewportClock clock, FrameBudget budget, int tierRank, Func<RenderPass, bool> passMask, ViewCamera camera) =>
        from next in IO.lift(() => Interlocked.Increment(ref ordinal))
        from frame in IO.lift(() => Render(next, clock.Clocks, budget, tierRank, camera, Binding, Passes.Filter(passMask))
            .BindFail(fault => fault is ViewportFault.LeaseRejected or ViewportFault.ContextUnavailable
                ? Render(next, clock.Clocks, budget, tierRank, camera, Fallback,
                    Passes.Filter(static pass => pass is RenderPass.Composite or RenderPass.Overlay))
                : Fin.Fail<FrameReceipt>(fault))
            .IfFail(fault => Empty(next, clock, fault)))
        from receipt in IO.pure(frame with { At = clock.Clocks.Now, Correlation = clock.Correlation })
        from _ in Sink(receipt)
        select receipt;

    private Fin<FrameReceipt> Render(long next, ClockPolicy clocks, FrameBudget budget, int tierRank, ViewCamera camera, GpuBinding binding, Seq<RenderPass> passes) =>
        Lease(binding, target => {
            ResolvePass resolvePass = Policy.For(tierRank);
            ResolveState prior = resolve.Value;
            bool moved = prior.Camera.Map(held => held != camera).IfNone(true);
            if (moved || !resolvePass.Reproject) { prior.History.Iter(static image => image.Dispose()); }
            ResolveState state = resolve.Swap(held => resolvePass.Advance(held, camera));
            return passes
                .Fold(
                    Fin.Succ(new PassFold(Seq<(string, Duration)>(), Seq<string>(), 0L)),
                    (rail, pass) => rail.Bind(fold => Execute(pass, target, clocks, budget, camera, moved, resolvePass, state, fold)))
                .Map(folded => {
                    SnapshotHistory(resolvePass, target);
                    // The Gpu column carries ONLY completed measurements: an absent timestamp lane is the
                    // honest zero, and a FAILED readback keeps zero while its fault lands on the receipt
                    // fault rail — unsupported and failed stay distinguishable in frame evidence.
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
                        default, gpuFault, default, folded.Deferred);
                });
        });

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

    private readonly record struct PassFold(Seq<(string Pass, Duration Elapsed)> Passes, Seq<string> Deferred, long Triangles) {
        public Duration Elapsed => Passes.Map(static row => row.Elapsed).Fold(Duration.Zero, static (sum, next) => sum + next);
    }

    // The budget invariant executes HERE: a pass whose start would overrun the frame duration, or whose
    // projected geometry total exceeds the triangle ceiling, defers — recorded, never executed — so the sealed verdict
    // derives from measured elapsed evidence. The pathTrace arm resets the film on camera motion, then
    // swaps the advanced AccumulationTarget back into its cell; samples draw zero triangles honestly.
    private Fin<PassFold> Execute(RenderPass pass, RenderTarget target, ClockPolicy clocks, FrameBudget budget, ViewCamera camera, bool moved, ResolvePass resolvePass, ResolveState state, PassFold fold) =>
        fold.Elapsed >= budget.Frame || fold.Triangles + EstimatedTriangles(pass) > budget.MaxTriangles
            ? Fin.Succ(fold with { Deferred = fold.Deferred.Add(pass.Key) })
            : clocks.Mark() switch {
                var mark => pass.Switch(
                        state: (Target: target, Cluster: Cluster.Value, ClusterCell: Cluster, Camera: camera, Moved: moved, Resolve: resolvePass, State: state),
                        cull: static (ctx, c) => c.Visible(ctx.Target, ctx.Cluster, ctx.Camera)
                            .Map(next => (ctx.ClusterCell.Swap(_ => next.Cluster), next.Result.Draw.Count).Item2),
                        geometry: static (ctx, g) => g.Draw(ctx.Target, ctx.Cluster, ctx.Cluster.Clusters.Count),
                        pathTrace: static (ctx, p) => (ctx.Moved ? p.Film.Swap(static film => film.Reset()) : p.Film.Value) switch {
                            var film => p.Pass.Accumulate(film, ctx.Camera, p.Rig, p.SampleBudget, p.Seed)
                                .Map(advanced => (p.Film.Swap(_ => advanced), 0).Item2),
                        },
                        sim: static (ctx, s) => s.Draw(ctx.Target),
                        composite: static (ctx, c) => ctx.Resolve.Resolve(ctx.Target, ctx.State, c.Raster).Map(static _ => 0),
                        overlay: static (ctx, o) => ctx.Target.Surface.Match(Some: surface => o.Draw(surface.Canvas).Map(static _ => 0), None: () => Fin.Succ(0)))
                    .Map(drawn => fold with {
                        Passes = fold.Passes.Add((pass.Key, clocks.Elapsed(mark))),
                        Triangles = fold.Triangles + drawn,
                    }),
            };

    private long EstimatedTriangles(RenderPass pass) =>
        pass is RenderPass.Geometry ? Cluster.Value.Triangles : 0L;

    // A failed frame is DISTINGUISHABLE from a healthy software fallback: the fault threads onto the
    // receipt's Fault column and no fabricated pass row exists — zero passes executed is the honest fact.
    private FrameReceipt Empty(long next, ViewportClock clock, Error fault) =>
        new(next, GpuBackend.Software, Seq<(string Pass, Duration Elapsed)>(), Duration.Zero, 0L, false, clock.Clocks.Now, clock.Correlation, Some(fault));

    public const string FrameInstrument = "rasm.appui.viewport.frame.elapsed";
    public const string GpuInstrument = "rasm.appui.viewport.gpu.elapsed";
    public const string OverrunInstrument = "rasm.appui.viewport.budget.overrun";

    public static TelemetryContributorPort TelemetryRow(string version, string schemaUrl) =>
        AppUiTelemetry.Contribute(version, schemaUrl,
            new(FrameInstrument, InstrumentKind.Distribution, "s", "frame wall duration", Buckets.UiFrameSeconds),
            new(GpuInstrument, InstrumentKind.Distribution, "s", "measured GPU duration per frame", Buckets.UiFrameSeconds),
            new(OverrunInstrument, InstrumentKind.Count, "{frame}", "frames exceeding the frame budget"));

    // Frame timing rides the direct rail: composition binds this projection at the retire site where
    // the typed receipt is in hand, so the per-frame path never serializes an envelope; the gpu
    // instrument stays the evidence fan's gpu-frame arm target off the governor timeline.
    public static Unit Observe(InstrumentSet set, FrameReceipt receipt) =>
        (ignore(set.Record(FrameInstrument,
             receipt.Passes.Fold(Duration.Zero, static (total, pass) => total + pass.Elapsed).TotalSeconds,
             new KeyValuePair<string, object?>("backend", receipt.Backend))),
         receipt.WithinBudget ? unit : ignore(set.Count(OverrunInstrument, 1L))).Item2;
}
```

```mermaid
---
config:
  theme: base
  look: classic
  layout: elk
  flowchart:
    curve: linear
    padding: 25
  themeVariables:
    darkMode: true
    fontFamily: "SF Mono, Menlo, Cascadia Mono, Segoe UI Mono, Consolas, monospace"
    useGradient: false
    dropShadow: "none"
    background: "#282A36"
    primaryColor: "#44475A"
    primaryTextColor: "#F8F8F2"
    primaryBorderColor: "#BD93F9"
    lineColor: "#FF79C6"
    textColor: "#F8F8F2"
    edgeLabelBackground: "#21222C"
    labelBackgroundColor: "#21222C"
  themeCSS: ".nodeLabel{font-size:13px;font-weight:500}.edgeLabel{font-size:12px;font-weight:500}.cluster-label .nodeLabel{font-size:13.5px;font-weight:700;letter-spacing:.08em}.edge-thickness-normal{stroke-width:2px}.edge-thickness-thick{stroke-width:3px}.edge-pattern-dashed,.edge-pattern-dotted{stroke-width:1.5px;stroke-dasharray:4 6}.node rect,.node circle,.node polygon,.node path,.node .outer-path{stroke-width:1.5px;filter:none!important}.cluster rect{stroke-width:1px!important;stroke-dasharray:5 4!important;filter:none!important}.marker path{transform:scale(.8);transform-origin:5px 5px}.marker circle{transform:scale(.48);transform-origin:5px 5px}.edgeLabel rect{transform-box:fill-box;transform-origin:center;transform:scale(1.1,1.2)}"
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
    linkStyle 3,4 stroke:#FF79C6,color:#F8F8F2
    linkStyle 6,7 stroke:#50FA7B,color:#F8F8F2
    classDef primary fill:#44475A,stroke:#FF79C6,color:#F8F8F2
    classDef data fill:#FFB86CBF,stroke:#FFB86C,color:#282A36
    classDef boundary fill:#282A36,stroke:#BD93F9,color:#F8F8F2
    class RenderGraph,Cull,Geometry,PathTrace,Sim,Composite primary
    class RenderTarget data
    class FrameReceipt,ReceiptSinkPort boundary
```

## [03]-[SIM_VISUAL]

- Owner: `SimField` the Compute field receipt projection; `SimVisual` `[Union]` the simulation render-pass family; `TransferFunction` the volume opacity-and-color map.
- Cases: `SimVisual` = Isosurface | Volume | Streamline | Glyph | Deformation | MeshQuality | ParallelCoords under the locked kind literals isosurface, volume, streamline, glyph, deformation, mesh-quality, parallel-coords.
- Entry: `public Fin<RenderPass> Pass(SimField field)` dispatches every visualization case into an executable `RenderPass.Sim`; the transient-playback frame is a field index, never a wall-clock tick.
- Auto: the isosurface case marching-cubes-extracts the level set at the threshold, the volume case ray-marches the scalar field through the `TransferFunction` opacity-color map, the streamline case integrates the vector field through Runge-Kutta seeds, the glyph case places oriented arrow or tensor glyphs at the sample points, the deformation case warps the mesh by the displacement field at the playback frame, the mesh-quality case shades each cell by its scaled-Jacobian or aspect-ratio metric, and the parallel-coords case routes its multi-dimensional cells onto the `CustomVisual.ParallelCoordinates` fold so a parameter sweep reads one analytical chart; transient playback scrubs a field-index sequence so a deformation or transient field animates by frame index under the deterministic motion clock.
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
    public sealed record MeshQuality(string Key, Colormap Ramp, Func<SimField, Colormap, Fin<SKPath>> Shade) : SimVisual(Key);
    public sealed record ParallelCoords(string Key, Func<SimField, Fin<CustomVisualData>> Project, Func<RenderTarget, CustomVisualData, Fin<int>> Draw) : SimVisual(Key);

    public Fin<RenderPass> Pass(SimField field) => Switch(
        state: field,
        isosurface: static (f, i) => Fin.Succ<RenderPass>(new RenderPass.Sim(i.Key, target => Path(target, i.March(f, i.Threshold)))),
        volume: static (f, v) => Fin.Succ<RenderPass>(new RenderPass.Sim(v.Key, target => v.RayMarch(target, f, v.Transfer))),
        streamline: static (f, s) => Fin.Succ<RenderPass>(new RenderPass.Sim(s.Key, target => Path(target, s.Integrate(f, s.Seeds, s.StepSize)))),
        glyph: static (f, g) => Fin.Succ<RenderPass>(new RenderPass.Sim(g.Key, target => Path(target, g.Place(f, g.Scale)))),
        deformation: static (f, d) => Fin.Succ<RenderPass>(new RenderPass.Sim(d.Key, target => Path(target, d.Warp(f, d.Magnify, f.FrameIndex)))),
        meshQuality: static (f, m) => Fin.Succ<RenderPass>(new RenderPass.Sim(m.Key, target => Path(target, m.Shade(f, m.Ramp)))),
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
}
```

## [04]-[VIEWPOINT_CODEC]

- Owner: `CameraFrame` the common eye/target/up product; `ViewCamera` `[Union]` the perspective-or-orthographic lens; `Viewpoint` the portable view-state receipt; optional `SectionBox` clip volume; `ViewMeasurement` the source-addressed snapped-measurement markup; `VisibilityOverride` the per-element visibility-and-color row; `VisibilityAction` `[SmartEnum]` the isolate/hide/x-ray/reset interaction fold over the one override vocabulary; `DiffClass` `[SmartEnum]` with `VersionGhost` the version-compare ghost projection onto the same rows; `ViewpointCodec` the projection binding the receipt to the `cs:Rasm.Bim/Review/issues#BCF_ARCHIVE` `BcfViewpoint` exchange contract, never an AppUi-local BCF viewpoint schema.
- Entry: `public string Encode(JsonSerializerOptions wire)` — serializes the camera, section box, visibility set, color overrides, and selection into one portable JSON receipt; `public static Fin<Viewpoint> Decode(string blob, JsonSerializerOptions wire)` — round-trips a stored or shared viewpoint.
- Auto: a viewpoint captures the full reproducible view state in one receipt — one `ViewCamera` case carries only its live lens scalar, `Option<SectionBox>` distinguishes absence from a real clip volume, visibility and color overrides key by element guid, and `ViewMeasurement` preserves the capture payload key and point-sample index behind every vertex. The BCF projection maps the camera onto `BcfCamera`, visibility onto `VisibilityExceptions`/`DefaultVisibility`, color onto `BcfColoring`, section bounds onto six `BcfClippingPlane` rows, and measurement segments onto `BcfLine` rows.
- Receipt: `Viewpoint` serializes through the package wire context as a versioned portable receipt the dashboard, the markup, and the cross-process coordination consume.
- Interaction: `VisibilityAction` folds a selection onto the one `VisibilityOverride` vocabulary — `Isolate` hides every unselected element, `Hide` hides the selection, `Xray` ghosts the unselected rest through the transparency channel, `Reset` clears the override set — each row constructed with its fold delegate so the interactive state, the saved viewpoint, and the animation visibility track speak one visibility language and a viewer-local visibility model is the deleted form; the Shell verb binding raising these folds as `CommandIntent`s is `Shell/commands.md`'s row.
- Diff: `VersionGhost.Project` maps a version-compare element classification (the Persistence `ReplayWindow`/commit-DAG fold arriving as `(ElementId, DiffClass)` values — AppUi runs no ledger read) onto diff-classed `VisibilityOverride` rows — `Added` tints, `Removed` ghosts at high transparency, `Modified` tints distinctly, `Unchanged` passes — so an A/B model comparison renders through the same override channel a viewpoint carries and a parallel ghost-overlay owner never exists.
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

// The isolate/hide/x-ray/reset interaction fold — each row constructed with its override-set delegate over
// (scene ids, selection), so the core BIM viewer loop is four rows on the ONE VisibilityOverride vocabulary
// the viewpoint captures and the animation visibility track steps; Shell/commands raises the verbs.
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

// The viewpoint <-> BCF projection binds the portable `Viewpoint` receipt to the one
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
                    state.Frame.Eye, state.Frame.Target - state.Frame.Eye, state.Frame.Up, camera.ViewHeight, state.Aspect)) switch {
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
- Boundary: the wire emits strict camel-case JSON. `ViewCameraWire.projection` selects one live `scale` meaning, `section` is nullable, and `ViewMeasurementWire` carries content-keyed sample identities plus unit-normalized evidence. Tile bounds cross as `[x, y, z, radius]`, content keys as `:x32` text, instants through `InstantPattern.ExtendedIso`, and durations as round-trip text. `ResidencyMarshal` projects every payload and viewpoint member at one seam; `ResidencyManifest.Encode` rejects unmapped members and enforces nullable annotations and required constructors.

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
  readonly error: number;
  readonly parentError: number;
}

interface ResidencyTileWire {
  readonly key: string;
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

The C# `ResidencyManifest` is the single mint of `WEB_GEOMETRY_RESIDENCY_WIRE`; the TypeScript worker consumes it by content key and never re-mints identity. `ResidencyMarshal` projects each admitted Compute `ResidencyPayload` kind through one total fold, and `KeyHex` renders the producer-owned `UInt128` as the shared `:x32` wire value.

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
    int Level, int Parent, double Error, double ParentError);

public sealed record ResidencyTileWire(
    string Key, string Kind, string ContentKey, string BlobKey, long Bytes, int ResidentCount,
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
                    camera.ViewHeight)),
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
            m.Level, m.Parent, m.Error, m.ParentError);

    // one residency tile wire row = one Compute ResidencyPayload projected 1:1 — the content/blob key is the payload's
    // own XxHash128 (never re-hashed off raw positions), the EXT_meshopt_compression bufferViews are the payload's
    // StreamSpan layout, the cone-cull clusters are its meshopt-built ResidencyMeshlet set (vertex-table + triangle
    // split), the bounds is its self-described sphere, and the scene-cell placement key rides from the residency plan.
    public static ResidencyTileWire TileOf(string sceneKey, ResidencyPayload payload) =>
        new(sceneKey, payload.Kind.Key, KeyHex(payload.ContentKey), BlobKeyOf(payload.ContentKey),
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

    // Mint joins the AppUi residency decision (which content-addressed payloads are resident, at which scene cells)
    // with the Compute ResidencyPayload codec (the EXT_meshopt_compression streams, clusters, bounds, content key) —
    // a pure projection of the Compute payload, never re-deriving geometry, content keys, or streams from
    // AppUi-internal owners; a resident scene tile with no matching payload is dropped, never re-hashed.
    public static ResidencyManifest Mint(
        Viewpoint viewpoint,
        ResidencyPlan plan,
        HashMap<string, ResidencyPayload> payloads,
        long vramBudget) =>
        new(Schema, ResidencyMarshal.ViewpointOf(viewpoint),
            plan.Resident.Choose(tile => payloads.Find(tile.Key).Map(payload => ResidencyMarshal.TileOf(tile.Key, payload))),
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

- [VIEWPORT_GPU]: `GpuBackend.Target` absorbs Ganesh, raster, Wgpu, and browser target construction over the closed `GpuBinding` union. `RenderGraph` leases one active target, executes the pass DAG, advances `ResolveState`, and seals measured `WgpuFrameEvidence`; meshlet, path-trace, resolve, and simulation acceleration remain pass delegates under that lease and create no parallel device or target owner.
- [WGPU_BACKEND]: `WgpuPresentation` discriminates exclusive swapchain presentation from Avalonia compositor import. The composited arm selects keyed mutex, binary semaphore, timeline semaphore, or automatic synchronization from `GetSynchronizationCapabilities`, awaits `ImportCompleted`, rejects every `IsLost` state, and submits through the matched `CompositionDrawingSurface.UpdateWith*Async` member. Timestamp query resolve, buffer map, queue submission, and device polling retire through the one `WgpuFrameEvidence` lane.
- [WEB_RESIDENCY]: `ResidencyManifest` is the single C# mint of the browser residency wire. `ResidencyMarshal` projects Compute `ResidencyPayload` stream spans, meshlet hierarchy, bounds, content keys, and admitted splat tiles into one content-addressed manifest; the browser consumes that wire and never re-mints payload identity, hierarchy, or blob keys.

## [07]-[RESEARCH]

(none)
