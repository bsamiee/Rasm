# [APPUI_RENDER_PIPELINE]

`RenderGraph` is the infinite viewport's GPU render pipeline: one pass-DAG drives every frame over the platform's one compositor-owned `GRContext` leased through the embed capsule. The `GpuBackend` rows carry the per-backend target-construction delegate over the composition-bound `GpuBinding` union and a `CapabilitySet<GpuTrait>` column stating what each substrate can run, so backend identity derives from the binding and a mismatched backend-factory pair is unrepresentable; each `RenderPass` case declares the products it makes and the products it must follow, so the roster's topological order is PROVED at composition rather than assumed from a caller's `Seq`; the `ResolvePass` ladder selects the antialias-and-super-resolution resolve off the governor tier's own rank; and `SimVisual` renders isosurface, volume, streamline, glyph, deformation, mesh-quality, and parallel-coordinate fields off the Compute field results. This page owns the render-graph pass algebra with its scheduling proof, the backend vocabulary and its capability column, the measured GPU-time evidence lane, the resolve ladder, the simulation render passes, and the browser residency wire; the portable view state and named-view registry live in `Render/viewpoint`, the overlay-plane manipulators in `Render/measure`, the geometry-virtualization and residency owners in `Render/meshlets`, and the path-trace integrator in `Render/pathtrace`. Its substrate is SkiaSharp 3 GPU backends (`GRContext`, `GRMtlBackendContext`, `GRVkBackendContext`, `SKRuntimeEffect`) leased through `ISkiaSharpApiLease`, the `Silk.NET.WebGPU` wgpu/Dawn target factory, the Compute geometry and field results, and the AppHost frame-budget ports. GPU passes share the one leased compositor context, and the `Software` 2D-Skia raster is the deterministic CPU floor.

## [01]-[INDEX]

- [02]-[RENDER_GRAPH]: The proved pass-DAG, the `GpuBackend` capability and target-factory columns over `GpuBinding`, the resolve ladder, the frame-budget verdict, and the software fallback.
- [03]-[SIM_VISUAL]: Traced, volumetric, mesh-quality, and parallel-coordinate field render passes off the Compute results.
- [04]-[TS_PROJECTION]: The content-keyed geometry-residency wire contract and its one generated projection seam.
- [05]-[GPU_AND_WIRE_BOUNDARY]: Viewport GPU lease law, the wgpu presentation arms, and the web residency mint.

## [02]-[RENDER_GRAPH]

- Owner: `ViewportFault` the direct generated `[Union]` with one `[FaultCase]` leaf per viewport failure; `GpuTrait` the substrate capability vocabulary; `GpuBackend` `[SmartEnum]` the backend rows carrying that capability set and their target-construction delegate; `GpuBinding` `[Union]` the composition-bound substrate each backend row folds; `SyncArm` `[SmartEnum]` the closed compositor synchronization vocabulary; `WgpuPresentation` `[Union]` the present dispatch; `WgpuErrorScope` the GPU validation bracket; `WgpuFrameEvidence` the timestamp-query GPU-time lane; `PassProduct` the frame-artifact vocabulary a pass makes and follows; `PassContract` the four-column declaration one `RenderPass` case answers; `RenderPass` `[Union]` the frame-pass vocabulary; `RenderTargetRequest` the resolve-row-derived allocation request; `RenderTarget` the lease-bound GPU surface; `SurfaceLease` the named platform-lease seam; `FrameView` the render-time view value; `ResolvePass` `[SmartEnum]` the resolve ladder whose `MinRank` column IS the tier binding; `ResolveState`/`ResolveStep` the temporal cell state; `BudgetVerdict` `[Union]` the frame's own budget answer; `FrameBudget` the per-axis ceilings; `ViewportClock` the timeline, clock, and correlation triple; `PassAnswer` the one shape every pass arm fills; `FrameRender` the per-frame result; `RenderGraph` the admitted frame executor.
- Cases: `RenderPass` = Cull | Geometry | PathTrace | Composite | Sim | Overlay; `ResolvePass` = Msaa | Taa | Fsr | Smaa; `PassProduct` = cut | depth | colour | film | frame; `GpuTrait` = accelerated | skia-canvas | native-pipeline; `SyncArm` = keyed-mutex | timeline | semaphores | automatic; `BudgetVerdict` = Within | Overran; `ViewportFault` = ContextUnavailable | BackendUnsupported | BudgetExceeded | LeaseRejected | Contended.
- Entry: `RenderGraph.Of(passes, cluster, binding, fallback, gpuTime, validation, lease)` — the ONE mint, which proves the pass roster's keys distinct, every declared need produced, and the dependency graph acyclic, and stores the topological order; `RenderGraph.Draw(ViewportClock clock, FrameBudget budget, QualityVerdict quality, ViewCamera camera)` — `IO` rail, one frame over that proved order under the frame camera and the ONE governor verdict, returning its `FrameRender`; `RenderGraph.Observe(InstrumentSet set, FrameRender frame, ResidencyPlan plan)` — the frame-retire projection that writes the frame instruments and retires the accepted residency plan together.
- Auto: `Lease` opens the compositor's own GPU context through `ISkiaSharpApiLease.TryLeasePlatformGraphicsApi` and folds the leased context to the `RenderTarget` through the bound `GpuBinding`'s own backend row, so a pass-emit body binds a backend-provided target rather than the single `GRContext`-plus-`SKRuntimeEffect` emit path; composition owns the DISPLAY extent alone and hands it to the request builder the graph threads in, so the render extent and the sample count derive from the resolve row the tier selected; a lease-class fault re-runs the frame through the `Fallback` raster binding, so the CPU floor is a reachable arm of the same fold; the frame-budget invariant executes inside the pass fold — a pass starting past `FrameBudget.Frame`, or whose own declared charge carries the fold past `MaxTriangles`, DEFERS to the next frame — and the sealed verdict names the axis that breached rather than collapsing three facts to one bit.
- Law: the pass DAG is PROVED, not assumed. Each `RenderPass` case answers one `PassContract` naming the substrate traits it demands, the products it MAKES, the products it must follow (`After`), and the subset of those it cannot run without (`Needs`) — so `RenderGraph.Of` builds the edge set through one `GraphExtensions.ToAdjacencyGraph` fold, refuses a duplicate key, refuses a need no scheduled pass produces, refuses a cycle by name, and stores `AlgorithmExtensions.TopologicalSort`'s order. A caller-supplied `Seq` order is the deleted form and it is what let a geometry pass scheduled ahead of its cull read `CullResult.Empty` and silently draw nothing while every frame result reported success. `Needs` and `After` are two columns because a composite legitimately runs with no colour producer — the raster fallback roster is exactly `Composite` and `Overlay` — while a geometry draw with no cull is the named defect.
- Law: each `GpuBackend` row is CONSTRUCTED with its target-construction delegate and CARRIES the trait set its substrate admits, so `IsGpu` and every `Family == A || Family == B` disjunction become one `Traits.Admits` read and a pass roster narrows from `pass.Contract.Demands` against `binding.Backend.Traits`. `Metal`, `Vulkan`, and `OpenGl` fold the `GpuBinding.Ganesh` leased `GRContext` to `SKSurface.Create(GRRecordingContext, budgeted, request.Info, request.Samples, GRSurfaceOrigin)` for an offscreen target, or wrap the host framebuffer as a `GRBackendRenderTarget` and read `SampleCount` back as the granted column; `Software` folds `GpuBinding.Raster` to the CPU `SKSurface.Create(SKImageInfo)` floor, which takes no sample count and therefore grants one; `Wgpu` folds `GpuBinding.Wgpu` — whose target texture carries `TextureDescriptor.SampleCount` and whose pipeline multisample state must match it — over the `Silk.NET.WebGPU` wgpu/Dawn substrate (D3D12/Metal/Vulkan auto-negotiated through `BackendType`) whose `Adapter` matched the compositor adapter LUID/UUID at composition, its `Device`+`Queue` shared branch-wide; and `WebGpu` folds `GpuBinding.Browser`, the in-browser WebGPU surface the TS web leg consumes. `GpuBinding.Backend` DERIVES the backend row from the binding case, so `RenderGraph` holds bindings alone and a substrate swap is one backend row with its binding case.
- Law: the compositor synchronization mode is a CLOSED ROW admitted ONCE, at `WgpuPresentation.CompositedOf`, where the interop's `GetSynchronizationCapabilities` probe is already read — so `Present` dispatches over `SyncArm` totally and the five `HasFlag` probes and the unsupported-mode `throw` beside them have no spelling. The composited arm imports the rendered texture through the compositor interop family (`ICompositionGpuInterop.ImportImage`/`ImportSemaphore` then the arm's own `UpdateWith*Async` member); a second swapchain in composited mode is the DELETED form, and `SurfaceConfigure`/`SurfaceGetCurrentTexture` survives ONLY as the exclusive-fullscreen and headless arm. The wgpu mesh-shader and compute passes record through `CommandEncoder`/`RenderPassEncoder` and submit through `QueueSubmit`, never a managed scene wrapper.
- Law: a lost compositor import and a refused lease are TRANSIENT by the fault row's own `Retriability` column, so the re-drive is the kernel `RedrivePolicy(Schedule, Bound)` the composition elects and the graph's own fallback arm reads `error.Retriability` rather than a type-pattern disjunction over the fault union. A hand `catch`-and-rewrap, a bare `Option<Instant> Retry`, and a spelled-out `fault is LeaseRejected or ContextUnavailable` list are the three deleted forms.
- Law: the resolve ladder is ONE authority — `ResolvePass.MinRank` — read against `QualityTier`'s own rank roster. The per-tier table proves its coverage at type initialization exactly as `QualityTier.Ranked` proves its contiguity, so `ResolvePass.For(tier)` is TOTAL by construction and the absent-key fall to `Msaa` that answered the floor tier's most-degraded frame with a four-sample resolve has no spelling; a hand `(rank, pass)` roster beside the tier roster was a second authority over one ladder. `Taa` jitters the camera sub-pixel per frame and reprojects the prior frame through the motion-vector buffer under a neighborhood clamp, `Smaa` runs morphological edge AA, `Msaa` multi-samples the raster, and `Fsr` renders sub-resolution and spatially upscales, so the governor steps the whole ladder on the same hysteresis band that degrades the render passes.
- Law: each of the resolve row's three columns reaches the surface it governs. `RenderScale` and `Samples` mint the frame's `RenderTargetRequest`, so an `Fsr` frame allocates at `round(display * 0.6)` and an `Msaa` frame asks its backend for four samples, while `RenderTarget.Samples` publishes what the allocation GRANTED. `Jitter` becomes a CAMERA fact: `FrameView.Of` converts the signed sub-pixel offset to NDC against the target the frame allocated, and the geometry draw adds `NdcJitter` to its projection's third column. Cull and LOD read `FrameView.Camera` and `FrameView.LodScale` — the governor's own degrade lever — because a sub-pixel offset moves no cull decision; the `PathTrace` arm reads `Camera` too, and that is load-bearing: a jittered lens differs from the prior frame's every frame, so `AccumulationTarget.Reset` would fire on every frame and the film would never converge past one sample. The `Taa` motion-vector buffer is ONE `Render/meshlets` `BindlessTable` slot, never a parallel motion-vector owner.
- Law: the triangle column is a MEASURED draw count over one contract, and the contract is the frame's CUT. `RenderPass.Geometry` carries `Phase`, the `Render/meshlets` `CutPhase` row naming which slice of the cut it draws; `Charge`, the budget projection the pre-charge gate reads against that slice; and `Draw`, which returns the triangles it recorded. The cut is minted ONCE per geometry pass and charged then drawn, so the pre-charge estimate and the actual submission read one value; every other case contributes zero triangles, while the sim arm answers its swept field points and the pathTrace arm the film's shade-fault level on their own columns — measures with their own instruments, kept out of the triangle ceiling because a marched volume and a failed scatter are not triangles. `Render/meshlets`' `ClusterCull.DrawRows` mints BOTH meshlet geometry rows off one submit arrow — `CutPhase.Prior` then `CutPhase.Retest` — and the DAG orders them by their declared depth product rather than by their arrival order in a caller's seq.
- Output: `FrameRender` carries the frame ordinal, backend, per-pass durations, folded elapsed total, GPU duration, drawn triangles, swept sim points, film shade-fault level, typed budget verdict, deferred-pass set, instant, correlation, and fault. The producer folds elapsed once into the result, `RenderGraph.Observe` projects its measured columns onto instruments beside the accepted residency plan, and `Diagnostics/governor.md` consumes it directly as `PerfSample`. `WgpuFrameEvidence` fills the GPU column from timestamp-query readback and places a failed readback on the result's fault rail; `GpuTimeline.Deepen` replaces that duration only when every pass resolved, then fires `AppUiFact.GpuFrame` through the AppUi hook rail.
- Packages: SkiaSharp, Avalonia.Skia, Avalonia (compositor GPU interop), Silk.NET.WebGPU, Silk.NET.WebGPU.Extensions.WGPU, Silk.NET.WebGPU.Native.WGPU, QuikGraph, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm (project — `Deterministic.RadicalInverse` the TAA jitter sequence, `Cell.Commit` the resolve transition, `Custody.Bracket` the target lease), Rasm.AppHost (project)
- Growth: a new frame stage is one `RenderPass` case with its `PassContract` row, which the schedule proof orders with no edit; a new resolve column is one `ResolvePass` column plus its read on `RenderTargetRequest.Of` or `FrameView.Of`; a new resolve row is one `MinRank` declaration the ladder table picks up; a new backend is one `GpuBackend` row constructed with its trait set, its target delegate, and its `GpuBinding` case — Skia Graphite re-admits as one `SkiaGraphite` row the moment SkiaSharp ships its Recorder/Context surface; a new substrate capability is one `GpuTrait` row every consumer reads by column; a new fault case is one `[FaultCase]` leaf; zero new surface.
- Growth: a new viewport reliability indicator is one `ViewportObjectives` row on the evidence page, carried here with no edit.
- Boundary: `RenderGraph` is the named boundary capsule and a sealed CLASS, because a `with` copy shares the resolve cell by reference while duplicating the frame ordinal, and the mint is `Fin`-admitted because the pass DAG's soundness is proved once at composition rather than re-derived per frame. The resolve transition is the FRAME's and taken exactly once through kernel `Cell.Commit`, so a lease-rejected frame re-enters the fallback with the SAME `ResolveStep` — stepping the reprojection ordinal or re-jittering on the re-entry is the deleted form that made every GPU refusal skip a TAA sample; the transition body is PURE and names the history image it superseded on the state it installs, and the one drain releases that image after the exchange, because the commit body re-runs on every contended attempt and a release inside it frees a handle the winning state still holds. The frame ordinal is a monotone `Interlocked.Increment` over the graph-local counter — a per-graph IDENTITY the correlation join and the render-hash lane key on, not a gauged span, which is why it reads no timeline; per-pass DURATION is `MonotonicTimeline.Capture`/`Elapsed`, and an `IGaugeLane` roster beside it is refused by name because a pass has exactly one ceiling, the live `FrameBudget` the governor owns, and a static lane bound would be a second budget authority. The lease brackets its target through kernel `Custody.Bracket`, so a mid-fold fault releases the native rather than leaking one target per refused frame. Frame retirement PRESENTS through the binding's own `WgpuPresentation` with the four synchronization indices derived from the frame ordinal, so a declared-but-uninvoked presentation arm and a caller-supplied index pair are both deleted forms. The shared GPU context arrives as one `SurfaceLease` over the `Shell/hosts#EMBED_CAPSULE` platform-lease seam so no pass body names a `GRContext.CreateMetal`/`CreateVulkan` factory at a call site — a direct GPU-backend construction inside a pass arm is the rejected form. GPU validation on the `Wgpu` arm rides the error-scope rail: `DeviceSetUncapturedErrorCallback` installs once at device acquisition and `WgpuErrorScope` brackets every ACCELERATED pass encoding inside the fold, so a validation or out-of-memory error is a counted `ViewportFault` on the telemetry spine rather than a declared bracket nothing ever entered. The meshlet cluster the graph draws is the `Render/meshlets` owner and the path-trace pass the `Render/pathtrace` integrator, so the pipeline composes them and re-models neither.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ViewportFault : Fault {
    private static readonly FaultBand FamilyBand = FaultBand.Viewport;
    private ViewportFault(string detail) { Detail = detail; }

    public string Detail { get; }

    public override string Message => Detail;

    [FaultCase(0)]
    public sealed partial record ContextUnavailable(string Detail) : ViewportFault(Detail) {
        public override Retriability Retriability => Retriability.Transient;
    }
    [FaultCase(1)]
    public sealed partial record BackendUnsupported(string Detail) : ViewportFault(Detail);
    [FaultCase(2)]
    public sealed partial record BudgetExceeded(string Detail) : ViewportFault(Detail);
    [FaultCase(3)]
    public sealed partial record LeaseRejected(string Detail) : ViewportFault(Detail) {
        public override Retriability Retriability => Retriability.Transient;
    }
    [FaultCase(4)]
    public sealed partial record Contended(string Detail) : ViewportFault(Detail) {
        public override Retriability Retriability => Retriability.Transient;
    }
}

[NoReorder]
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GpuTrait : ICapability<GpuTrait> {
    public static readonly GpuTrait Accelerated = new("accelerated");
    public static readonly GpuTrait SkiaCanvas = new("skia-canvas");
    public static readonly GpuTrait NativePipeline = new("native-pipeline");
}

[NoReorder]
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PassProduct : ICapability<PassProduct> {
    public static readonly PassProduct Cut = new("cut");
    public static readonly PassProduct Depth = new("depth");
    public static readonly PassProduct Colour = new("colour");
    public static readonly PassProduct Film = new("film");
    public static readonly PassProduct Frame = new("frame");
}

public readonly record struct PassContract(
    CapabilitySet<GpuTrait> Demands,
    CapabilitySet<PassProduct> Needs,
    CapabilitySet<PassProduct> After,
    CapabilitySet<PassProduct> Makes);

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ResolvePass {
    public static readonly ResolvePass Fsr = new("fsr", samples: 1, renderScale: 0.6, reproject: false, minRank: 0);
    public static readonly ResolvePass Msaa = new("msaa", samples: 4, renderScale: 1.0, reproject: false, minRank: 1);
    public static readonly ResolvePass Smaa = new("smaa", samples: 1, renderScale: 1.0, reproject: false, minRank: 2);
    public static readonly ResolvePass Taa = new("taa", samples: 1, renderScale: 1.0, reproject: true, minRank: 3);

    public int Samples { get; }
    public double RenderScale { get; }
    public bool Reproject { get; }

    public int MinRank { get; }

    private static readonly Lazy<FrozenDictionary<int, ResolvePass>> Ladder = new(static () =>
        QualityTier.Items.ToFrozenDictionary(
            static tier => tier.Rank,
            static tier => toSeq(Items)
                .Filter(row => row.MinRank <= tier.Rank)
                .OrderByDescending(static row => row.MinRank)
                .Head
                .IfNone(() => throw new InvalidOperationException(
                    $"ResolvePass rows must cover QualityTier rank {tier.Rank}."))));

    public static ResolvePass For(QualityTier tier) => Ladder.Value[tier.Rank];

    private const int JitterPeriod = 8;

    private static (double X, double Y) HaltonJitter(long ordinal) =>
        (uint)((ordinal % JitterPeriod) + 1L) switch {
            var phase => (Deterministic.RadicalInverse(bits: phase) - 0.5,
                          Deterministic.RadicalInverse(index: phase, radix: 3) - 0.5),
        };

    public ResolveState Advance(ResolveState prior, ViewCamera camera) =>
        prior.Camera.Map(held => held != camera).IfNone(true) switch {
            var moved => (Reproject && !moved) switch {
                var keeps => prior with {
                    Ordinal = keeps ? prior.Ordinal + 1 : 1L,
                    Jitter = Reproject ? HaltonJitter(ordinal: keeps ? prior.Ordinal + 1 : 1L) : (0d, 0d),
                    History = keeps ? prior.History : None,
                    Retired = keeps ? None : prior.History,
                    RenderScale = RenderScale,
                    Camera = Some(camera),
                    Moved = moved,
                },
            },
        };

    public Fin<Unit> Resolve(RenderTarget target, ResolveState state, Func<SKCanvas, RenderTargetRequest, ResolveState, Fin<Unit>> composite) =>
        target.Surface.Match(
            Some: surface => composite(surface.Canvas, target.Request, state),
            None: () => Fin.Fail<Unit>(new ViewportFault.ContextUnavailable($"resolve/{Key}: no resolve surface")));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SyncArm {
    public static readonly SyncArm KeyedMutex = new("keyed-mutex",
        static (c, indices, values) => c.Surface.UpdateWithKeyedMutexAsync(c.Image, indices.Acquire, indices.Release));
    public static readonly SyncArm Timeline = new("timeline",
        static (c, indices, values) => c.Semaphores.Match(
            Some: pair => c.Surface.UpdateWithTimelineSemaphoresAsync(c.Image, pair.Wait, values.Wait, pair.Signal, values.Signal),
            None: () => c.Surface.UpdateAsync(c.Image)));
    public static readonly SyncArm Semaphores = new("semaphores",
        static (c, indices, values) => c.Semaphores.Match(
            Some: pair => c.Surface.UpdateWithSemaphoresAsync(c.Image, pair.Wait, pair.Signal),
            None: () => c.Surface.UpdateAsync(c.Image)));
    public static readonly SyncArm Automatic = new("automatic",
        static (c, indices, values) => c.Surface.UpdateAsync(c.Image));

    public static SyncArm Of(CompositionGpuImportedImageSynchronizationCapabilities probe) =>
        probe.HasFlag(CompositionGpuImportedImageSynchronizationCapabilities.KeyedMutex) ? KeyedMutex
        : probe.HasFlag(CompositionGpuImportedImageSynchronizationCapabilities.TimelineSemaphores) ? Timeline
        : probe.HasFlag(CompositionGpuImportedImageSynchronizationCapabilities.Semaphores) ? Semaphores
        : Automatic;

    [UseDelegateFromConstructor]
    public partial Task Update(WgpuPresentation.Composited composited, (uint Acquire, uint Release) indices, (ulong Wait, ulong Signal) values);
}

// --- [MODELS] --------------------------------------------------------------------------
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

public readonly record struct FrameView(ViewCamera Camera, (double X, double Y) NdcJitter, QualityVerdict Quality) {
    public double LodScale => Quality.Tier.LodPixelScale;

    public static FrameView Of(ViewCamera camera, (double X, double Y) jitterPixels, SKImageInfo info, QualityVerdict quality) =>
        new(camera, (2d * jitterPixels.X / Math.Max(info.Width, 1), -2d * jitterPixels.Y / Math.Max(info.Height, 1)), quality);
}

public sealed record RenderTarget(GpuBackend Backend, Option<SKSurface> Surface, Option<GRContext> Context, RenderTargetRequest Request, int Samples, IDisposable Native) : IDisposable {
    public void Dispose() => Native.Dispose();
}

public sealed record SurfaceLease(Func<GpuBinding, Func<SKImageInfo, RenderTargetRequest>, Fin<RenderTarget>> Acquire) {
    public Fin<T> Under<T>(GpuBinding binding, Func<SKImageInfo, RenderTargetRequest> request, Func<RenderTarget, Fin<T>> body) =>
        Acquire(binding, request).Bind(target => Custody.Bracket(() => body(target), target));
}

public readonly record struct ResolveState(
    long Ordinal,
    (double X, double Y) Jitter,
    Option<SKImage> History,
    double RenderScale,
    Option<ViewCamera> Camera,
    Option<SKImage> Retired,
    bool Moved);

public readonly record struct ResolveStep(ResolvePass Pass, ResolveState State);

public sealed record FrameBudget(Duration Frame, Duration Gpu, Duration Layout, long VramBytes, long EvictsPerFrame, int MaxTriangles) {
    public static readonly FrameBudget Sixty = new(
        Duration.FromMilliseconds(16.667), Duration.FromMilliseconds(14.0), Duration.FromMilliseconds(4.0),
        1_073_741_824L, 8L, 20_000_000);

    public static readonly FrameBudget Thirty = new(
        Duration.FromMilliseconds(33.333), Duration.FromMilliseconds(28.0), Duration.FromMilliseconds(8.0),
        536_870_912L, 16L, 8_000_000);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record BudgetVerdict {
    private BudgetVerdict() { }

    public sealed record Within : BudgetVerdict;
    public sealed record Overran(BudgetAxis Axis, double Measured, double Ceiling) : BudgetVerdict;

    public bool Held => this is Within;

    public Option<BudgetAxis> Breach => Switch(
        within: static _ => Option<BudgetAxis>.None,
        overran: static row => Some(row.Axis));

    public static BudgetVerdict Of(FrameBudget budget, Duration elapsed, Duration gpu) =>
        elapsed > budget.Frame ? new Overran(BudgetAxis.Frame, elapsed.ToTimeSpan().TotalNanoseconds, budget.Frame.ToTimeSpan().TotalNanoseconds)
        : gpu > budget.Gpu ? new Overran(BudgetAxis.Gpu, gpu.ToTimeSpan().TotalNanoseconds, budget.Gpu.ToTimeSpan().TotalNanoseconds)
        : new Within();
}

public sealed record ViewportClock(MonotonicTimeline Line, IClock Clock, CorrelationId Correlation);

public readonly record struct PassAnswer(CullResult Cut, long Triangles, long Points, long Faults) {
    public static PassAnswer Through(CullResult cut) => new(cut, 0L, 0L, 0L);

    public PassAnswer Drew(long triangles) => this with { Triangles = triangles };

    public PassAnswer Marched(long points) => this with { Points = points };

    public PassAnswer Faulted(long faults) => this with { Faults = faults };
}

public sealed record FrameRender(
    long Ordinal,
    GpuBackend Backend,
    Seq<(string Pass, Duration Elapsed)> Passes,
    Duration Elapsed,
    Duration Gpu,
    long Triangles,
    BudgetVerdict Budget,
    Instant At,
    CorrelationId Correlation,
    Option<Error> Fault = default,
    Seq<string> Deferred = default,
    long SimPoints = 0L,
    long FilmFaults = 0L) {
    public bool WithinBudget => Budget.Held && Deferred.IsEmpty;
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed record WgpuErrorScope(Action Push, Func<Option<string>> Pop) {
    public Fin<T> Guarded<T>(Func<Fin<T>> encode) {
        Push();
        Fin<T> outcome = encode();
        return Pop().Match(
            Some: static error => Fin.Fail<T>(new ViewportFault.ContextUnavailable($"wgpu/validation: {error}")),
            None: () => outcome);
    }
}

public sealed record WgpuFrameEvidence(Func<Fin<Duration>> Measure);

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GpuBackend {
    public static readonly GpuBackend Metal = new("metal", Ganesh, GaneshTarget);
    public static readonly GpuBackend Vulkan = new("vulkan", Ganesh, GaneshTarget);
    public static readonly GpuBackend OpenGl = new("opengl", Ganesh, GaneshTarget);
    public static readonly GpuBackend Software = new("software", Raster, RasterTarget);
    public static readonly GpuBackend Wgpu = new("wgpu", Native, WgpuTarget);
    public static readonly GpuBackend WebGpu = new("webgpu", Native, BrowserTarget);

    private static CapabilitySet<GpuTrait> Ganesh => CapabilitySet<GpuTrait>.Of(GpuTrait.Accelerated, GpuTrait.SkiaCanvas);
    private static CapabilitySet<GpuTrait> Raster => CapabilitySet<GpuTrait>.Of(GpuTrait.SkiaCanvas);
    private static CapabilitySet<GpuTrait> Native => CapabilitySet<GpuTrait>.Of(GpuTrait.Accelerated, GpuTrait.NativePipeline);

    public CapabilitySet<GpuTrait> Traits { get; }

    [UseDelegateFromConstructor]
    public partial Fin<RenderTarget> Target(GpuBinding binding, RenderTargetRequest request);

    private static Fin<RenderTarget> GaneshTarget(GpuBinding binding, RenderTargetRequest request) => binding switch {
        GpuBinding.Ganesh ganesh => ganesh.Lease(request),
        _ => Fin.Fail<RenderTarget>(new ViewportFault.BackendUnsupported($"{binding.Backend.Key}: not a Ganesh binding")),
    };

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

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record GpuBinding {
    private GpuBinding() { }

    public sealed record Ganesh : GpuBinding {
        private Ganesh(GpuBackend row, Func<RenderTargetRequest, Fin<RenderTarget>> lease) { Row = row; Lease = lease; }

        public GpuBackend Row { get; }
        public Func<RenderTargetRequest, Fin<RenderTarget>> Lease { get; }

        private static readonly CapabilitySet<GpuTrait> Demand =
            CapabilitySet<GpuTrait>.Of(GpuTrait.Accelerated, GpuTrait.SkiaCanvas);

        public static Fin<GpuBinding> Of(GpuBackend row, Func<RenderTargetRequest, Fin<RenderTarget>> lease) =>
            row.Traits.Require(Demand, missing => new ViewportFault.BackendUnsupported($"{row.Key}: lacks {missing}"))
                .Map(_ => (GpuBinding)new Ganesh(row, lease));
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

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record WgpuPresentation {
    private WgpuPresentation() { }

    public sealed record Composited(
        ICompositionGpuInterop Interop,
        CompositionDrawingSurface Surface,
        ICompositionImportedGpuImage Image,
        SyncArm Sync,
        Option<(ICompositionImportedGpuSemaphore Wait, ICompositionImportedGpuSemaphore Signal)> Semaphores) : WgpuPresentation;

    public sealed record Swapchain(nint WgpuSurface, Func<nint, IO<Unit>> Submit) : WgpuPresentation;

    public sealed record Headless : WgpuPresentation;

    public static WgpuPresentation CompositedOf(
        ICompositionGpuInterop interop,
        CompositionDrawingSurface surface,
        IPlatformHandle sharedTexture,
        PlatformGraphicsExternalImageProperties shape,
        string handleType,
        Func<CompositionGpuImportedImageSynchronizationCapabilities, Option<(ICompositionImportedGpuSemaphore Wait, ICompositionImportedGpuSemaphore Signal)>> semaphores) =>
        interop.GetSynchronizationCapabilities(handleType) switch {
            var probe => new Composited(
                interop, surface, interop.ImportImage(sharedTexture, shape), SyncArm.Of(probe), semaphores(probe)),
        };

    public IO<Unit> Present((uint Acquire, uint Release) indices, (ulong Wait, ulong Signal) values) => this switch {
        Composited c =>
            IO.liftAsync(async () => {
                await c.Image.ImportCompleted;
                return c.Interop.IsLost || c.Image.IsLost || c.Semaphores.Exists(static pair => pair.Wait.IsLost || pair.Signal.IsLost);
            }).Bind(lost => lost
                ? IO.fail<Unit>((Error)new ViewportFault.LeaseRejected("wgpu/present: compositor import lost"))
                : IO.liftAsync(async () => { await c.Sync.Update(c, indices, values); return unit; })),
        Swapchain swapchain => swapchain.Submit(swapchain.WgpuSurface),
        Headless => IO.pure(unit),
    };
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RenderPass(string Key) {
    public sealed record Cull(string Key, Func<RenderTarget, MeshletCluster, FrameView, Fin<(MeshletCluster Cluster, CullResult Result)>> Visible) : RenderPass(Key);
    public sealed record Geometry(string Key, CutPhase Phase, Func<DrawCut, long> Charge, Func<RenderTarget, FrameView, DrawCut, Fin<long>> Draw) : RenderPass(Key);
    public sealed record PathTrace(string Key, PathTracePass Pass, Atom<AccumulationTarget> Film, LightRig Rig, int SampleBudget, long Seed, CancelScope Scope) : RenderPass(Key);
    public sealed record Sim(string Key, Func<RenderTarget, Fin<int>> Draw) : RenderPass(Key);
    public sealed record Composite(string Key, Func<SKCanvas, RenderTargetRequest, ResolveState, Fin<Unit>> Raster) : RenderPass(Key);
    public sealed record Overlay(string Key, Func<SKCanvas, Fin<Unit>> Draw) : RenderPass(Key);

    public PassContract Contract => Switch(
        cull: static _ => new PassContract(Accelerated, NoProducts, NoProducts, Products(PassProduct.Cut)),
        geometry: static row => row.Phase.Step.Match(
            Some: step => new PassContract(
                Accelerated,
                Products(PassProduct.Cut),
                step > 0 ? Products(PassProduct.Cut, PassProduct.Depth) : Products(PassProduct.Cut),
                Products(PassProduct.Colour, PassProduct.Depth)),
            None: static () => new PassContract(
                Accelerated, Products(PassProduct.Cut), Products(PassProduct.Cut), Products(PassProduct.Colour))),
        pathTrace: static _ => new PassContract(
            Accelerated, Products(PassProduct.Cut), Products(PassProduct.Cut), Products(PassProduct.Film)),
        sim: static _ => new PassContract(Accelerated, NoProducts, NoProducts, Products(PassProduct.Colour)),
        composite: static _ => new PassContract(
            NoTraits, NoProducts, Products(PassProduct.Colour, PassProduct.Film), Products(PassProduct.Frame)),
        overlay: static _ => new PassContract(
            NoTraits, NoProducts, Products(PassProduct.Frame), NoProducts));

    private static CapabilitySet<GpuTrait> Accelerated => CapabilitySet<GpuTrait>.Of(GpuTrait.Accelerated);

    private static CapabilitySet<GpuTrait> NoTraits => CapabilitySet<GpuTrait>.None;

    private static CapabilitySet<PassProduct> NoProducts => CapabilitySet<PassProduct>.None;

    private static CapabilitySet<PassProduct> Products(params ReadOnlySpan<PassProduct> rows) =>
        CapabilitySet<PassProduct>.Of(rows);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public sealed class RenderGraph {
    private RenderGraph(
        Seq<RenderPass> passes, Atom<MeshletCluster> cluster, GpuBinding binding, GpuBinding.Raster fallback,
        Option<WgpuFrameEvidence> gpuTime, Option<WgpuErrorScope> validation, SurfaceLease lease) {
        (Passes, Cluster, Binding, Fallback, GpuTime, Validation, Lease) =
            (passes, cluster, binding, fallback, gpuTime, validation, lease);
    }

    public Seq<RenderPass> Passes { get; }
    public Atom<MeshletCluster> Cluster { get; }
    public GpuBinding Binding { get; }
    public GpuBinding.Raster Fallback { get; }
    public Option<WgpuFrameEvidence> GpuTime { get; }
    public Option<WgpuErrorScope> Validation { get; }
    public SurfaceLease Lease { get; }

    private long ordinal;
    private readonly Atom<ResolveState> resolve = Atom(new ResolveState(0L, (0d, 0d), None, 1.0, None, None, false));

    private static readonly Op ScheduleOp = Op.Of(name: "appui.render.schedule");
    private static readonly Op PassOp = Op.Of(name: "appui.render.pass");

    public static Fin<RenderGraph> Of(
        Seq<RenderPass> passes, Atom<MeshletCluster> cluster, GpuBinding binding, GpuBinding.Raster fallback,
        Option<WgpuFrameEvidence> gpuTime, Option<WgpuErrorScope> validation, SurfaceLease lease) =>
        Scheduled(passes).Map(ordered => new RenderGraph(ordered, cluster, binding, fallback, gpuTime, validation, lease));

    private static Fin<Seq<RenderPass>> Scheduled(Seq<RenderPass> roster) =>
        Made(roster) switch {
            var made => toSeq(roster.Bind(pass => toSeq(pass.Contract.Needs.Missing(made).Held)).Distinct()) switch {
                var orphaned =>
                    (Col(toSeq(roster.Map(static pass => pass.Key).Distinct()).Count == roster.Count, "distinct pass keys"),
                     Col(orphaned.IsEmpty, $"a producer for every declared need, missing {orphaned.Map(static row => row.Key)}"))
                    .Apply((_, _) => roster).ToFin().Bind(Ordered),
            },
        };

    private static CapabilitySet<PassProduct> Made(Seq<RenderPass> roster) =>
        CapabilitySet<PassProduct>.Of(toSeq(roster.Bind(static pass => toSeq(pass.Contract.Makes.Held)).Distinct()).ToArray());

    private static Fin<Seq<RenderPass>> Ordered(Seq<RenderPass> roster) =>
        roster.ToHashMap(static pass => pass.Key, static pass => pass) switch {
            var byKey => ScheduleOp.Catch(() => Fin.Succ(toSeq(
                    GraphExtensions.ToAdjacencyGraph<string, SEdge<string>>(
                        roster.Map(static pass => pass.Key),
                        key => Downstream(byKey[key], roster))
                    .TopologicalSort())))
                .Map(order => order.Choose(byKey.Find)),
        };

    private static Seq<SEdge<string>> Downstream(RenderPass pass, Seq<RenderPass> roster) =>
        roster
            .Filter(consumer => consumer.Key != pass.Key && pass.Contract.Makes.Held.Any(consumer.Contract.After.Admits))
            .Map(consumer => new SEdge<string>(pass.Key, consumer.Key));

    private static Validation<Error, Unit> Col(bool holds, string requirement) =>
        holds ? Validation<Error, Unit>.Success(unit) : Validation<Error, Unit>.Fail((Error)new ViewportFault.ContextUnavailable($"render-graph: {requirement}"));

    public IO<FrameRender> Draw(ViewportClock clock, FrameBudget budget, QualityVerdict quality, ViewCamera camera) =>
        from next in IO.lift(() => Interlocked.Increment(ref ordinal))
        from frame in IO.lift(() =>
            Resolved(quality, camera)
                .Bind(step => Render(next, clock, budget, quality, camera, Binding, step)
                    .BindFail(fault => fault is Fault { Retriability: Retriability.TransientCase }
                        ? Render(next, clock, budget, quality, camera, Fallback, step)
                        : Fin.Fail<FrameRender>(fault)))
                .IfFail(fault => Empty(next, clock, fault)))
        from _present in Presented(frame)
        select frame;

    private Fin<ResolveStep> Resolved(QualityVerdict quality, ViewCamera camera) =>
        ResolvePass.For(quality.Tier) switch {
            var pass => Cell.Commit(resolve, held => pass.Advance(held, camera)) switch {
                Transition<ResolveState>.Committed landed => Fin.Succ(new ResolveStep(pass, Drained(landed.State))),
                var contended => Fin.Fail<ResolveStep>(new ViewportFault.Contended(
                    $"render/resolve: the cell spent its swap budget at ordinal {contended.Current.Ordinal}")),
            },
        };

    private static ResolveState Drained(ResolveState installed) =>
        installed.Retired.Iter(static image => image.Dispose()) switch {
            _ => installed with { Retired = None },
        };

    private IO<Unit> Presented(FrameRender frame) =>
        Binding is GpuBinding.Wgpu wgpu && frame.Fault.IsNone && frame.Backend == wgpu.Backend
            ? wgpu.Presentation.Present(
                ((uint)frame.Ordinal, (uint)(frame.Ordinal + 1L)),
                ((ulong)frame.Ordinal, (ulong)(frame.Ordinal + 1L)))
            : IO.pure(unit);

    private Seq<RenderPass> Schedulable(GpuBinding binding, QualityVerdict quality) =>
        Passes
            .Filter(quality.Tier.Cut.Admits)
            .Filter(pass => binding.Backend.Traits.AdmitsAll(pass.Contract.Demands));

    private Fin<FrameRender> Render(long next, ViewportClock clock, FrameBudget budget, QualityVerdict quality, ViewCamera camera, GpuBinding binding, ResolveStep step) {
        (ResolvePass resolvePass, ResolveState state) = step;
        Seq<RenderPass> passes = Schedulable(binding, quality);
        return Lease.Under(
            binding,
            display => RenderTargetRequest.Of(display, resolvePass),
            target => FrameView.Of(camera, state.Jitter, target.Request.Info, quality) switch {
                var view => passes
                    .Fold(
                        Fin.Succ(PassFold.Empty),
                        (rail, pass) => rail.Bind(fold => Execute(pass, target, clock.Line, budget, view, resolvePass, state, fold)))
                    .Map(folded => Complete(next, clock, budget, binding, resolvePass, target, folded)),
            });
    }

    private FrameRender Complete(long next, ViewportClock clock, FrameBudget budget, GpuBinding binding, ResolvePass resolvePass, RenderTarget target, PassFold folded) {
        ignore(SnapshotHistory(resolvePass, target));
        (Duration gpu, Option<Error> gpuFault) = GpuTime.Match(
            Some: static lane => lane.Measure().Match(
                Succ: static measured => (measured, Option<Error>.None),
                Fail: static fault => (Duration.Zero, Some(fault))),
            None: static () => (Duration.Zero, Option<Error>.None));
        return new FrameRender(
            next, binding.Backend, folded.Passes, folded.Elapsed, gpu, folded.Triangles,
            BudgetVerdict.Of(budget, folded.Elapsed, gpu),
            clock.Clock.GetCurrentInstant(), clock.Correlation,
            gpuFault, folded.Deferred, folded.Points, folded.Faults);
    }

    private Unit SnapshotHistory(ResolvePass pass, RenderTarget target) =>
        pass.Reproject
            ? target.Surface.Match(
                Some: surface => surface.Snapshot() switch {
                    var image => Cell.Commit(resolve, Seated(image)) switch {
                        Transition<ResolveState>.Committed landed => ignore(Drained(landed.State)),
                        _ => Discard(image),
                    },
                },
                None: static () => unit)
            : unit;

    private static Unit Discard(SKImage image) { image.Dispose(); return unit; }

    private static Func<ResolveState, ResolveState> Seated(SKImage next) =>
        held => held with { History = Some(next), Retired = held.History };

    private readonly record struct PassFold(
        Seq<(string Pass, Duration Elapsed)> Passes, Seq<string> Deferred, Duration Elapsed,
        long Triangles, long Points, long Faults, CullResult Cut) {
        public static readonly PassFold Empty = new(
            Seq<(string, Duration)>(), Seq<string>(), Duration.Zero, 0L, 0L, 0L, CullResult.Empty);

        public PassFold Deferring(string key) => this with { Deferred = Deferred.Add(key) };

        public PassFold Ran(string key, Duration elapsed, PassAnswer answer) => this with {
            Passes = Passes.Add((key, elapsed)),
            Elapsed = Elapsed + elapsed,
            Triangles = Triangles + answer.Triangles,
            Points = Points + answer.Points,
            Faults = Math.Max(Faults, answer.Faults),
            Cut = answer.Cut,
        };
    }

    private Fin<PassFold> Execute(
        RenderPass pass, RenderTarget target, MonotonicTimeline line, FrameBudget budget,
        FrameView view, ResolvePass resolvePass, ResolveState state, PassFold fold) =>
        Charged(pass, fold.Cut) switch {
            var charge when fold.Elapsed >= budget.Frame || fold.Triangles + charge.Estimate > budget.MaxTriangles =>
                Fin.Succ(fold.Deferring(pass.Key)),
            var charge =>
                from start in line.Capture(PassOp)
                from answer in Guarded(pass, () => Run(pass, target, view, resolvePass, state, fold.Cut, charge.Cut))
                from end in line.Capture(PassOp)
                from elapsed in line.Elapsed(start, end, PassOp)
                select fold.Ran(pass.Key, Duration.FromTimeSpan(elapsed), answer),
        };

    private (long Estimate, Option<DrawCut> Cut) Charged(RenderPass pass, CullResult cut) =>
        pass is RenderPass.Geometry geometry
            ? new DrawCut(Cluster.Value, geometry.Phase.Select(cut)) switch {
                var drawn => (geometry.Charge(drawn), Some(drawn)),
            }
            : (0L, None);

    private Fin<PassAnswer> Guarded(RenderPass pass, Func<Fin<PassAnswer>> encode) =>
        pass.Contract.Demands.Admits(GpuTrait.Accelerated)
            ? Validation.Match(Some: scope => scope.Guarded(encode), None: encode)
            : encode();

    private Fin<PassAnswer> Run(
        RenderPass pass, RenderTarget target, FrameView view, ResolvePass resolvePass,
        ResolveState state, CullResult cut, Option<DrawCut> charged) =>
        pass.Switch(
            state: (Target: target, ClusterCell: Cluster, View: view, Moved: state.Moved,
                    Resolve: resolvePass, State: state, Cut: cut, Charged: charged),
            cull: static (ctx, c) => c.Visible(ctx.Target, ctx.ClusterCell.Value, ctx.View)
                .Map(next => PassAnswer.Through(ctx.ClusterCell.Swap(_ => next.Cluster) switch { _ => next.Result })),
            geometry: static (ctx, g) => ctx.Charged
                .ToFin(new ViewportFault.ContextUnavailable($"geometry/{g.Key}: charged without a cut"))
                .Bind(drawn => g.Draw(ctx.Target, ctx.View, drawn))
                .Map(triangles => PassAnswer.Through(ctx.Cut).Drew(triangles)),
            pathTrace: static (ctx, p) => (ctx.Moved ? p.Film.Swap(static film => film.Reset()) : p.Film.Value) switch {
                var film => p.Pass.Accumulate(film, ctx.View.Camera, p.Rig, p.SampleBudget, p.Seed, p.Scope)
                    .Map(advanced => PassAnswer.Through(p.Film.Swap(_ => advanced) switch { _ => ctx.Cut })
                        .Faulted(advanced.Faults)),
            },
            sim: static (ctx, s) => s.Draw(ctx.Target).Map(swept => PassAnswer.Through(ctx.Cut).Marched(swept)),
            composite: static (ctx, c) => ctx.Resolve.Resolve(ctx.Target, ctx.State, c.Raster)
                .Map(_ => PassAnswer.Through(ctx.Cut)),
            overlay: static (ctx, o) => ctx.Target.Surface.Match(
                Some: surface => o.Draw(surface.Canvas).Map(_ => PassAnswer.Through(ctx.Cut)),
                None: () => Fin.Succ(PassAnswer.Through(ctx.Cut))));

    private FrameRender Empty(long next, ViewportClock clock, Error fault) =>
        new(next, GpuBackend.Software, Seq<(string Pass, Duration Elapsed)>(), Duration.Zero, Duration.Zero, 0L,
            new BudgetVerdict.Within(), clock.Clock.GetCurrentInstant(), clock.Correlation, Some(fault));

    // --- [INSTRUMENTS]

    public static readonly InstrumentSpec Frame = InstrumentSpec.Create(
        "rasm.appui.viewport.frame.elapsed", InstrumentKind.Distribution, MeasureForm.Real, "s",
        "frame wall duration", Seq(AppUiTelemetry.BackendSlot), Some(Buckets.UiFrameSeconds), None, None);

    public static readonly InstrumentSpec Gpu = InstrumentSpec.Create(
        "rasm.appui.viewport.gpu.elapsed", InstrumentKind.Distribution, MeasureForm.Real, "s",
        "measured GPU duration per frame", Seq(AppUiTelemetry.PassSlot, AppUiTelemetry.UnmeasuredSlot),
        Some(Buckets.UiFrameSeconds), None, None);

    public static readonly InstrumentSpec Overrun = InstrumentSpec.Create(
        "rasm.appui.viewport.budget.overrun", InstrumentKind.Count, MeasureForm.Whole, "{frame}",
        "frames exceeding a declared budget axis", Seq(AppUiTelemetry.CauseSlot), None, None, None);

    public static readonly InstrumentSpec SimPoints = InstrumentSpec.Create(
        "rasm.appui.viewport.sim.points", InstrumentKind.Count, MeasureForm.Whole, "{point}",
        "field-visual path points the sim passes swept", Seq<string>(), None, None, None);

    public static readonly InstrumentSpec FilmFaults = InstrumentSpec.Create(
        "rasm.appui.viewport.film.faults", InstrumentKind.Level, MeasureForm.Whole, "{fault}",
        "path-trace shade faults on the current film", Seq<string>(), None, None, None);

    public static TelemetryContributorPort TelemetryRow(string version, FrameBudget budget) =>
        AppUiTelemetry.Contribute(version, ViewportObjectives.Pack(budget), Frame, Gpu, Overrun, SimPoints, FilmFaults);

    public static Fin<Unit> Observe(InstrumentSet set, FrameRender frame, ResidencyPlan plan) =>
        set.Write(Frame, frame.Elapsed.TotalSeconds,
                InstrumentSet.Tags((AppUiTelemetry.BackendSlot, frame.Backend.Key)))
            .Bind(_ => frame.Budget.Breach.Match(
                Some: axis => set.Write(Overrun, 1L, InstrumentSet.Tags((AppUiTelemetry.CauseSlot, axis.Key))),
                None: static () => Fin.Succ(unit)))
            .Bind(_ => set.Write(SimPoints, frame.SimPoints))
            .Bind(_ => set.Level(FilmFaults, frame.FilmFaults))
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
    accDescr: A proved pass order drives a bracketed render target through cull, geometry, path tracing, simulation, composite, and frame completion.
    PassContract -->|products| Schedule["TopologicalSort"]
    Schedule --> RenderGraph
    RenderGraph -->|SurfaceLease.Under| RenderTarget
    RenderTarget --> Cull
    Cull -->|cut| Geometry
    Cull -->|cut| PathTrace
    Geometry -->|depth| Composite
    PathTrace -->|film| Composite
    Sim -->|colour| Composite
    Composite -->|frame| Overlay
    Composite --> FrameRender
    FrameRender -->|Observe| InstrumentSet
    FrameRender -->|project| Governor["Diagnostics governor PerfSample"]
```

## [03]-[SIM_VISUAL]

- Owner: `SimField` the Compute field-result projection; `FieldSites` the closed where-to-sample axis; `TransferFunction` the volume opacity-and-color map; `SimVisual` `[Union]` the simulation render-pass family.
- Cases: `SimVisual` = Traced | Volume | MeshQuality | ParallelCoords under the locked kind literals traced, volume, mesh-quality, parallel-coords.
- Entry: `SimVisual.Pass(SimField field)` dispatches every visualization case into an executable `RenderPass.Sim`; the transient-playback frame is a field index, never a wall-clock tick.
- Auto: the traced case renders any path-producing field visualization — a marching-cubes level set, a Runge-Kutta streamline integration, oriented arrow or tensor glyph placement, or a displacement warp at the playback frame — each closing its own scalar over the trace delegate while DECLARING the `FieldSites` it samples at, so the site policy stays readable to the pass key, the frame result, and a viewpoint diff; the volume case ray-marches the scalar field through the `TransferFunction` opacity-color map; the mesh-quality case emits one `VisualStroke` per cell band inked from that cell's scaled-Jacobian or aspect-ratio metric and draws it through the Charts owner's own band walk; the parallel-coords case routes its multi-dimensional cells onto the `CustomVisual.ParallelCoordinates` fold so a parameter sweep reads one analytical chart; transient playback scrubs a field-index sequence so a deformation or transient field animates by frame index under the deterministic motion clock.
- Law: four case rows collapsed to ONE. Isosurface, streamline, glyph, and deformation shared an identity regime, an admission path, a payload timing, a consumer, and a return type — each was `(Key, closed scalars, Func<…, Fin<SKPath>>)` and the dispatch already closed its scalars at the pass mint. The surviving discriminant a caller needs is WHERE the visualization samples, which is `FieldSites` and is declared on the row; the per-case scalar names (threshold, step size, glyph scale, magnify factor) are the NAMED LOSS, and they now live at the composition that closes them, where the pass key already spells which visualization the row is.
- Packages: SkiaSharp, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm.Compute (project)
- Growth: a new path-producing field visualization is one composed `Traced` row, not a case; a new DRAW REGIME — a second volumetric integrator, a second chart routing — is one case; a new transfer-function ramp is one `Colormap` row consumed here; a new way of choosing WHERE to sample is one `FieldSites` row; zero new surface. A flow-topology visualization is therefore not a case at all: a Morse atlas renders as its separatrix arcs and its classified fixed points through two `Traced` rows fed `FieldSites.Declared`.
- Boundary: field geometry projects from Compute results and never re-computes a simulation, and the same law governs field ANALYSIS: the kernel `Rasm/Processing/flow` atlas reached through `VectorIntent.Atlas` is projected by the caller and crosses as `FieldSites.Declared` coordinates, so this surface never mints a `FlowPartition`, a `TopologyPolicy`, or an integration step of its own. `TransferFunction` samples the Theme-owned perceptual `Colormap` rail and its admission ACCUMULATES, so a malformed ramp names every breached column at once. Deformation and transient fields advance by deterministic frame index. GPU volume and isosurface passes bind through the render-graph lease, while CPU marching cubes and ray marching provide the deterministic reference path.

```csharp
// --- [MODELS] --------------------------------------------------------------------------
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

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FieldSites {
    private FieldSites() { }
    public sealed record Sampled(int Count) : FieldSites;
    public sealed record Declared(Seq<(double X, double Y, double Z)> Points) : FieldSites;
}

public sealed record TransferFunction {
    private TransferFunction(Colormap ramp, double floor, double ceiling, double opacityGamma) =>
        (Ramp, Floor, Ceiling, OpacityGamma) = (ramp, floor, ceiling, opacityGamma);

    public Colormap Ramp { get; }
    public double Floor { get; }
    public double Ceiling { get; }
    public double OpacityGamma { get; }

    public static readonly TransferFunction Default = new(Colormap.Viridis, floor: 0d, ceiling: 1d, opacityGamma: 2d);

    public static Fin<TransferFunction> Of(Colormap ramp, double floor, double ceiling, double opacityGamma) =>
        (Col(double.IsFinite(floor), $"a finite floor, saw {floor}"),
         Col(double.IsFinite(ceiling), $"a finite ceiling, saw {ceiling}"),
         Col(floor < ceiling, "floor below ceiling"),
         Col(double.IsFinite(opacityGamma) && opacityGamma > 0d, $"a positive finite opacity gamma, saw {opacityGamma}"))
        .Apply((_, _, _, _) => new TransferFunction(ramp, floor, ceiling, opacityGamma))
        .ToFin();

    private static Validation<Error, Unit> Col(bool holds, string requirement) =>
        holds ? Validation<Error, Unit>.Success(unit) : Validation<Error, Unit>.Fail((Error)new ViewportFault.ContextUnavailable($"transfer-function: {requirement}"));

    public Fin<(Color Color, double Opacity)> Sample(double scalar) =>
        double.IsFinite(scalar)
            ? Math.Clamp((scalar - Floor) / (Ceiling - Floor), 0d, 1d) switch {
                var t => Ramp.Sample(t).Map(color => (color, Math.Pow(t, OpacityGamma))),
            }
            : Fin.Fail<(Color, double)>(new ViewportFault.ContextUnavailable("transfer-function: finite scalar required"));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SimVisual(string Key) {
    public sealed record Traced(string Key, Option<FieldSites> Sites, Func<SimField, Fin<SKPath>> Trace) : SimVisual(Key);
    public sealed record Volume(string Key, TransferFunction Transfer, Func<RenderTarget, SimField, TransferFunction, Fin<int>> RayMarch) : SimVisual(Key);
    public sealed record MeshQuality(string Key, Colormap Ramp, float Width, Func<SimField, Fin<Seq<VisualStroke>>> Shade) : SimVisual(Key);
    public sealed record ParallelCoords(string Key, Func<SimField, Fin<CustomVisualData>> Project, Func<RenderTarget, CustomVisualData, Fin<int>> Draw) : SimVisual(Key);

    public Fin<RenderPass> Pass(SimField field) => Switch(
        state: field,
        traced: static (f, t) => Fin.Succ<RenderPass>(new RenderPass.Sim(t.Key, target => Path(target, t.Trace(f)))),
        volume: static (f, v) => Fin.Succ<RenderPass>(new RenderPass.Sim(v.Key, target => v.RayMarch(target, f, v.Transfer))),
        meshQuality: static (f, m) => Fin.Succ<RenderPass>(new RenderPass.Sim(m.Key, target => Strokes(target, m.Shade(f), m.Ramp, m.Width))),
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

    private static Fin<int> Strokes(RenderTarget target, Fin<Seq<VisualStroke>> strokes, Colormap ramp, float width) =>
        target.Surface.ToFin(new ViewportFault.ContextUnavailable("sim/strokes: target has no Skia surface"))
            .Bind(surface => strokes.Bind(seq => seq.Exists(static stroke => stroke.Pigment.IsSome)
                ? Fin.Fail<int>(new ViewportFault.ContextUnavailable("sim/strokes: explicit pigment is the legend plane's, not the field plane's"))
                : Banded(surface, seq, ramp, width)));

    private static Fin<int> Banded(SKSurface surface, Seq<VisualStroke> seq, Colormap ramp, float width) {
        using SKPaint paint = new() { IsAntialias = true };
        Fin<int> drawn = toSeq(seq
            .GroupBy(static stroke => (stroke.Ink, stroke.Style, stroke.Pigment))
            .OrderBy(static band => band.Key.Ink.Value))
            .Fold(Fin.Succ(0), (rail, band) => rail.Bind(points =>
                ramp.Sample(band.Key.Ink.Value).Map(colour => {
                    paint.Color = new SKColor(colour.R, colour.G, colour.B, colour.A);
                    Option<SKPathEffect> dash = band.Key.Style.Write(paint, width);
                    int swept = band.Fold(0, (count, stroke) => {
                        surface.Canvas.DrawPath(stroke.Path, paint);
                        return count + stroke.Path.PointCount;
                    });
                    paint.PathEffect = null;
                    dash.Iter(static effect => effect.Dispose());
                    return points + swept;
                })));
        seq.Iter(static stroke => stroke.Path.Dispose());
        return drawn;
    }
}
```

## [04]-[TS_PROJECTION]

- Entry: `ResidencyMap.Mint(viewpoint, plan, payloads, vramBudget)` returns `Fin<GeometryResidency>` after admitting the AppUi resident set against Compute's payload census; `ResidencyMap.Json` renders that same admitted projection through the shared AppHost `WireJson.Formatter`.
- Boundary: protobuf presence is the only absence spelling: optional scalar setters run only on `Some`, optional messages stay unset on `None`, and repeated fields fill on the generated collection surface. The payload's semantic XXH3 key selects it in the residency census while its independent `ArtifactContent` SHA-256 identity and encoded extent cross together as `ArtifactRef`; storage paths remain behind the app resolver and never leak into the semantic contract. Instants cross as protobuf `Timestamp`, and every closed Compute row maps totally onto its generated enum. The schema carries the producer descriptor whole — `Parent`, `ParentError`, `Curvature`, and `Cut` remain producer facts — while `GeometryResidency` replaces as one message per emission. Every resident key must resolve in the payload census; missing keys accumulate as `ViewportFault.ContextUnavailable`, so no successful contract can omit part of its admitted resident set. ProtoJSON leaves only through `WireJson.Formatter`; no package-local formatter, STJ context, or manifest wrapper participates. Render evidence remains the generated `EvidenceTimelineWire` render arm rather than a standalone frame family.

```csharp
using NodaTime.Serialization.Protobuf;
using Rasm.AppHost.Runtime;
// Contracts are retired from this logic.

// --- [BOUNDARIES] ----------------------------------------------------------------------
public static class ResidencyMap {
    public static Fin<string> Json(
        Viewpoint viewpoint,
        ResidencyPlan plan,
        HashMap<UInt128, ResidencyPayload> payloads,
        long vramBudget) =>
        Mint(viewpoint, plan, payloads, vramBudget)
            .Map(static wire => WireJson.Formatter.Format(wire));

    public static Fin<Host.GeometryResidency> Mint(
        Viewpoint viewpoint,
        ResidencyPlan plan,
        HashMap<UInt128, ResidencyPayload> payloads,
        long vramBudget) {
        Validation<Error, Seq<Host.ResidencyTileWire>> tiles = plan.Resident
            .Traverse(tile => payloads.Find(tile.ContentKey)
                .ToValidation<Error>(new ViewportFault.ContextUnavailable(
                    $"residency/payload: resident {ContentHash.Hex(tile.ContentKey)} is absent from the payload census"))
                .Map(Tile))
            .As();
        Validation<Error, ulong> budget = vramBudget >= 0L
            ? Success<Error, ulong>(checked((ulong)vramBudget))
            : Fail<Error, ulong>(new ViewportFault.BudgetExceeded(
                $"residency/budget: wire budget {vramBudget}b is negative"));
        return (tiles, budget).Apply((resident, admittedBudget) => {
            Host.GeometryResidency wire = new() {
                Viewpoint = View(viewpoint),
                VramBudget = admittedBudget,
            };
            wire.Tiles.Add(resident);
            return wire;
        }).As().ToFin();
    }

    public static Host.ViewpointWire View(Viewpoint view) {
        Host.ViewpointWire wire = new() {
            Key = view.Key,
            Version = checked((uint)view.Version),
            Camera = Camera(view.Camera),
            At = view.At.ToTimestamp(),
        };
        view.Section.Iter(section => wire.Section = Section(section));
        wire.Overrides.Add(view.Overrides.Map(Visibility));
        wire.Selection.Add(view.Selection);
        wire.Measurements.Add(view.Measurements.Map(Measurement));
        return wire;
    }

    public static Fin<Viewpoint> ParseView(string json) {
        Op key = Op.Of(name: "appui.viewpoint.decode");
        return key.Catch(() => View(WireJson.Parser.Parse<Host.ViewpointWire>(json), key));
    }

    private static Fin<Viewpoint> View(Host.ViewpointWire wire, Op key) {
        return
            from camera in Camera(wire.Camera)
            from measurements in toSeq(wire.Measurements).TraverseM(row => Measurement(row, key)).As()
            from at in Optional(wire.At)
                .ToFin(new ViewportFault.ContextUnavailable("viewpoint/decode: timestamp is absent"))
                .Bind(value => key.Catch(() => Fin.Succ(value.ToInstant())))
            from view in Viewpoint.Capture(
                wire.Key,
                checked((int)wire.Version),
                camera,
                Optional(wire.Section).Map(Section),
                toSeq(wire.Overrides).Map(Visibility),
                toSeq(wire.Selection),
                measurements,
                at)
            select view;
    }

    private static Host.ViewCameraWire Camera(ViewCamera camera) => camera.Switch(
        perspective: static value => new Host.ViewCameraWire {
            Frame = Frame(value.Frame),
            Perspective = new Host.ViewCameraWire.Types.Perspective {
                FieldOfViewDegrees = value.FieldOfViewDeg,
            },
        },
        orthographic: static value => new Host.ViewCameraWire {
            Frame = Frame(value.Frame),
            Orthographic = new Host.ViewCameraWire.Types.Orthographic {
                ViewHeight = value.ViewHeight,
                RetainedFieldDegrees = value.RetainedFieldDeg,
            },
        },
        asymmetric: static value => new Host.ViewCameraWire {
            Frame = Frame(value.Frame),
            Asymmetric = new Host.ViewCameraWire.Types.Asymmetric {
                AngleLeftRadians = value.AngleLeft,
                AngleRightRadians = value.AngleRight,
                AngleUpRadians = value.AngleUp,
                AngleDownRadians = value.AngleDown,
            },
        });

    private static Fin<ViewCamera> Camera(Host.ViewCameraWire camera) =>
        camera.LensCase switch {
            Host.ViewCameraWire.LensOneofCase.Perspective => Fin.Succ<ViewCamera>(
                new ViewCamera.Perspective(Frame(camera.Frame), camera.Perspective.FieldOfViewDegrees)),
            Host.ViewCameraWire.LensOneofCase.Orthographic => Fin.Succ<ViewCamera>(
                new ViewCamera.Orthographic(
                    Frame(camera.Frame),
                    camera.Orthographic.ViewHeight,
                    camera.Orthographic.RetainedFieldDegrees)),
            Host.ViewCameraWire.LensOneofCase.Asymmetric => Fin.Succ<ViewCamera>(
                new ViewCamera.Asymmetric(
                    Frame(camera.Frame),
                    camera.Asymmetric.AngleLeftRadians,
                    camera.Asymmetric.AngleRightRadians,
                    camera.Asymmetric.AngleUpRadians,
                    camera.Asymmetric.AngleDownRadians)),
            _ => Fin.Fail<ViewCamera>(new ViewportFault.ContextUnavailable(
                $"viewpoint/decode: camera lens {camera.LensCase} is not admitted")),
        };

    private static Host.SectionBoxWire Section(SectionBox box) =>
        new() {
            Min = Point(box.MinX, box.MinY, box.MinZ),
            Max = Point(box.MaxX, box.MaxY, box.MaxZ),
        };

    private static SectionBox Section(Host.SectionBoxWire box) =>
        new(box.Min.X, box.Min.Y, box.Min.Z, box.Max.X, box.Max.Y, box.Max.Z);

    private static Host.VisibilityOverrideWire Visibility(VisibilityOverride row) {
        Host.VisibilityOverrideWire wire = new() {
            ElementId = row.ElementId,
            Visible = row.Visible,
            Transparency = row.Transparency,
        };
        row.ColorArgb.Iter(value => wire.ColorArgb = value);
        return wire;
    }

    private static VisibilityOverride Visibility(Host.VisibilityOverrideWire row) =>
        new(row.ElementId, OverrideState.Of(
            row.Visible,
            row.HasColorArgb ? Some(row.ColorArgb) : Option<uint>.None,
            row.Transparency));

    private static Host.ViewMeasurementWire Measurement(ViewMeasurement measurement) {
        Host.ViewMeasurementWire wire = new() {
            Key = measurement.Key,
            TotalMeters = measurement.Total.Meters,
        };
        wire.Vertices.Add(measurement.Vertices.Map(Point));
        wire.AnglesDegrees.Add(measurement.Angles.Map(static angle => angle.Degrees));
        return wire;
    }

    private static Fin<ViewMeasurement> Measurement(Host.ViewMeasurementWire wire, Op key) =>
        toSeq(wire.Vertices).TraverseM(point => Point(point, key)).As().Map(vertices => new ViewMeasurement(
            wire.Key,
            vertices,
            UnitsNet.Length.FromMeters(wire.TotalMeters),
            toSeq(wire.AnglesDegrees).Map(static angle => UnitsNet.Angle.FromDegrees(angle))));

    private static Host.ViewMeasurementPointWire Point(ViewMeasurementPoint point) =>
        new() {
            SourceKey = ContentHash.Wire(point.SourceKey),
            SampleIndex = checked((uint)point.SampleIndex),
            Position = Point(point.Position),
        };

    private static Fin<ViewMeasurementPoint> Point(Host.ViewMeasurementPointWire point, Op key) =>
        ContentHash.Admit(point.SourceKey.Span, key).Map(source => new ViewMeasurementPoint(
            source,
            checked((int)point.SampleIndex),
            Point(point.Position)));

    private static Host.ResidencyTileWire Tile(ResidencyPayload payload) {
        Host.ResidencyTileWire wire = new() {
            Kind = Kind(payload.Kind),
            Artifact = new Rasm.Contracts.Artifact.ArtifactRef {
                Sha256 = ByteString.CopyFrom(Convert.FromHexString(payload.Artifact.Sha256)),
                ArtifactBytes = payload.Artifact.Bytes,
            },
            ResidentCount = checked((uint)payload.ResidentCount),
            HarmonicDegree = checked((uint)payload.HarmonicDegree),
            Bounds = new Host.SphereWire {
                Center = Point(payload.Center),
                Radius = payload.Radius,
            },
        };
        wire.Streams.Add(payload.Layout.OrderBy(static slot => slot.Value.Offset)
            .Select(static slot => Stream(slot.Key, slot.Value)));
        wire.Meshlets.Add(payload.Clusters.Map(Meshlet));
        return wire;
    }

    private static Host.MeshoptStream Stream(ResidencyStream stream, StreamSpan span) =>
        new() {
            Stream = stream.Key,
            Mode = Mode(span.Mode),
            Filter = Filter(span.Filter),
            ByteOffset = checked((ulong)span.Offset),
            ByteLength = checked((ulong)span.Length),
            Count = checked((ulong)span.Count),
            ByteStride = checked((uint)span.ByteStride),
            CodecVersion = checked((uint)span.CodecVersion),
        };

    private static Host.Meshlet Meshlet(ResidencyMeshlet cluster) {
        Host.Meshlet wire = new() {
            VertexOffset = checked((uint)cluster.VertexOffset),
            TriangleOffset = checked((uint)cluster.TriangleOffset),
            VertexCount = checked((uint)cluster.VertexCount),
            TriangleCount = checked((uint)cluster.TriangleCount),
            Center = Point(cluster.Center),
            Radius = cluster.Radius,
            ConeApex = Point(cluster.ConeApex),
            ConeAxis = Direction(cluster.ConeAxis),
            ConeCutoff = cluster.ConeCutoff,
            Level = checked((uint)cluster.Level),
            Shell = checked((uint)cluster.Shell),
            Error = cluster.Error,
            Curvature = cluster.Curvature,
            Cut = checked((uint)cluster.Cut),
        };
        cluster.Parent.Iter(value => wire.Parent = checked((uint)value));
        cluster.ParentError.Iter(value => wire.ParentError = value);
        return wire;
    }

    private static Host.ResidencyKind Kind(ResidencyKind value) => value.Switch(
        state: unit,
        meshletCluster: static _ => Host.ResidencyKind.MeshletCluster,
        quantizedVertex: static _ => Host.ResidencyKind.QuantizedVertex,
        pointSplat: static _ => Host.ResidencyKind.PointSplat,
        gaussianSplat: static _ => Host.ResidencyKind.GaussianSplat);

    private static Host.StreamMode Mode(StreamMode value) => value.Switch(
        state: unit,
        attributes: static _ => Host.StreamMode.Attributes,
        triangles: static _ => Host.StreamMode.Triangles,
        indices: static _ => Host.StreamMode.Indices,
        raw: static _ => Host.StreamMode.Raw);

    private static Host.StreamFilter Filter(StreamFilter value) => value.Switch(
        state: unit,
        none: static _ => Host.StreamFilter.None,
        octahedral: static _ => Host.StreamFilter.Octahedral,
        quaternion: static _ => Host.StreamFilter.Quaternion,
        exponential: static _ => Host.StreamFilter.Exponential);

    private static Rasm.Contracts.Spatial.Point3 Point(System.Numerics.Vector3 value) =>
        Point(value.X, value.Y, value.Z);

    private static Rasm.Contracts.Spatial.UnitDirection3 Direction(System.Numerics.Vector3 value) =>
        new() { X = value.X, Y = value.Y, Z = value.Z };

    private static Host.ViewCameraWire.Types.Frame Frame(CameraFrame frame) =>
        new() { Eye = Point(frame.Eye), Target = Point(frame.Target), Up = Direction(frame.Up) };

    private static CameraFrame Frame(Host.ViewCameraWire.Types.Frame frame) =>
        new(Point(frame.Eye), Point(frame.Target), Direction(frame.Up));

    private static System.Numerics.Vector3 Point(Rasm.Contracts.Spatial.Point3 value) =>
        new((float)value.XM, (float)value.YM, (float)value.ZM);

    private static System.Numerics.Vector3 Direction(Rasm.Contracts.Spatial.UnitDirection3 value) =>
        new((float)value.X, (float)value.Y, (float)value.Z);

    private static Rasm.Contracts.Spatial.Point3 Point(double x, double y, double z) =>
        new() { XM = x, YM = y, ZM = z };
}
```

## [05]-[GPU_AND_WIRE_BOUNDARY]

- [VIEWPORT_GPU]: `GpuBackend.Target` absorbs Ganesh, raster, Wgpu, and browser target construction over the closed `GpuBinding` union, every arm reading the one `RenderTargetRequest` the resolve row derived and answering the sample count its allocation GRANTED, while `GpuBackend.Traits` states what each substrate can run so a pass roster narrows on set algebra rather than a case list. `RenderGraph` proves its pass order once at composition, advances `ResolveState` through the kernel commit, brackets one leased target at the requested extent, threads one `FrameView` into the cull and geometry arms, executes the proved order over one fold-carried cut, and seals measured `WgpuFrameEvidence`; meshlet, path-trace, resolve, and simulation acceleration remain pass delegates under that lease and create no parallel device or target owner.
- [WGPU_BACKEND]: `WgpuPresentation` discriminates exclusive swapchain presentation from compositor import; its composited arm elects its `SyncArm` row ONCE from `GetSynchronizationCapabilities`, awaits `ImportCompleted`, answers a transient refusal on every `IsLost` state, and submits through the row's own `UpdateWith*Async` member. Timestamp resolve, buffer map, queue submission, and device polling retire through the one `WgpuFrameEvidence` lane, and `WgpuErrorScope` brackets every accelerated pass encoding inside the frame fold.
- [WEB_RESIDENCY]: `ResidencyMap.Mint` projects Compute `ResidencyPayload` stream spans, meshlet hierarchy, bounds, content keys, and admitted splat tiles directly into generated `Render.GeometryResidency`; the browser imports the same generated schema, and ProtoJSON crosses through AppHost `WireJson.Formatter` with no hand manifest, interface, or codec posture beside it.

## [06]-[RESEARCH]

(none)
