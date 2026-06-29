# [APPUI_RENDER_PIPELINE]

The GPU render pipeline for the infinite viewport: one `RenderGraph` pass-DAG drives every frame over a host-shared `GRContext` leased through the embed capsule, the `GpuBackend` `RenderTargetFactory` column owns per-backend target construction, the `ResolvePass` ladder selects the antialias-and-super-resolution resolve, `SimVisual` renders isosurface/volume/streamline/glyph/deformation fields off the Compute field receipts, and `Viewpoint` codecs camera/section/visibility/override/selection as one portable BCF-compatible receipt. The page owns the render-graph pass algebra, the per-backend target factory, the resolve ladder, the simulation render passes, the viewpoint receipt, and the residency wire projection; the geometry-virtualization and residency owners live in `Render/meshlets`, the path-trace integrator in `Render/pathtrace`. The substrate is SkiaSharp 3 GPU backends (`GRContext`, `GRMtlBackendContext`, `GRVkBackendContext`, `SKRuntimeEffect`) leased through `ISkiaSharpApiLease`, the `Silk.NET.WebGPU` wgpu/Dawn target factory, the Compute geometry and field receipts, and the AppHost clock, frame-budget, and receipt-sink ports. The GPU passes SPIKE-gate on the live host-shared GPU context and the `Software` 2D-Skia raster is the deterministic CPU floor.

## [01]-[INDEX]

- [01]-[RENDER_GRAPH]: Frame pass-DAG, per-backend `RenderTargetFactory`, resolve ladder, frame-budget invariant, fallback.
- [02]-[SIM_VISUAL]: Isosurface, volume, streamline, glyph, deformation field render passes off the Compute receipts.
- [03]-[VIEWPOINT_CODEC]: Camera, section-box, visibility, override, selection projecting onto the `Rasm.Bim` `BcfViewpoint` exchange contract.
- [04]-[TS_PROJECTION]: Viewpoint, frame-evidence, and content-keyed geometry-residency wire contract.

## [02]-[RENDER_GRAPH]

- Owner: `RenderPass` `[Union]` frame-pass vocabulary; `RenderGraph` pass-DAG executor; `RenderTarget` the lease-bound GPU surface; `FrameReceipt` per-frame evidence; `ViewportFault` the fault family; `ResolvePass` `[SmartEnum]` the antialias-and-super-resolution resolve ladder the `Composite` pass selects; `ResolvePolicy` the per-tier delegate-row binding.
- Cases: `RenderPass` = Cull | Geometry | PathTrace | Composite | Sim | Overlay under the locked kind literals cull, geometry, path-trace, composite, sim, overlay; `ResolvePass` = Msaa | Taa | Fsr | Smaa under the locked policy literals; `ViewportFault` = Text | ContextUnavailable | BackendUnsupported | BudgetExceeded | LeaseRejected in the 4500 code band.
- Entry: `public IO<FrameReceipt> Frame(RenderGraph graph, ViewportClock clock, FrameBudget budget)` — `IO` rail; the pass-DAG executes topologically and the frame seals one receipt carrying the per-pass elapsed and the GPU-time fold.
- Auto: `Lease` opens the host-shared GPU context through `ISkiaSharpApiLease.TryLeasePlatformGraphicsApi` and folds the leased context to the `RenderTarget` through the `GpuBackend`'s own `RenderTargetFactory` column, so a pass-emit body binds a backend-provided target factory rather than the single `GRContext`-plus-`SKRuntimeEffect` emit path and the embedded viewport composites into the Rhino-owned context and never mints a second `GRContext`; when the platform lease yields no GPU context the graph folds to the `Software` backend's CPU 2D-Skia factory and the `Composite`-only raster pass so the viewport renders a deterministic CPU frame; the frame-budget invariant gates the pass list — a pass whose accumulated GPU-time projection overruns `FrameBudget.Frame` defers to the next frame and the deferral folds onto the budget-overrun instrument, so frame budget is an invariant the graph enforces, never a hope.
- Backend: `GpuBackend` carries the `RenderTargetFactory` delegate column per backend row — `Metal`, `Vulkan`, `OpenGl`, and `Software` bind the SkiaSharp Ganesh `GRContext` target factory, `Wgpu` binds the `Silk.NET.WebGPU` wgpu/Dawn target factory (D3D12/Metal/Vulkan auto-negotiated through `BackendType`) acquiring an `Adapter` matched to the compositor adapter LUID/UUID, requesting a `Device`+`Queue`, configuring a `Surface` swapchain, and presenting the rendered `Texture` into the Avalonia compositor through `ICompositionGpuInterop.ImportImage` — the wgpu mesh-shader/compute passes record through `CommandEncoder`/`RenderPassEncoder` and submit through `QueueSubmit`, never a managed scene wrapper — and `WebGpu` binds the in-browser WebGPU factory the TS web leg consumes — so the `Lease` and every `RenderPass`/`CapturePass`/`CustomVisual` emit body binds a backend-provided target factory and a substrate swap is one backend row, the render-graph pass algebra staying backend-agnostic above the factory; the per-backend emit path (wgpu pipeline submit versus `SKRuntimeEffect` shader) diverges below the `RenderTargetFactory`, so the factory column owns the divergence and the CPU 2D-Skia fallback is the floor.
- Resolve: the `Composite` pass selects one `ResolvePass` policy row after the geometry and path-trace passes — `Taa` jitters the camera sub-pixel per frame and reprojects the prior frame (`ResolveState.History`/`Jitter` threaded into the `composite` delegate) through the motion-vector buffer under a neighborhood-clamp history rejection so a static scene converges and a moving scene ghosts no tail, `Smaa` runs the morphological edge AA, `Msaa` multi-samples the raster, and `Fsr` renders sub-resolution (`RenderScale` 0.6) under the `Render/meshlets` `ResidencyBudget` VRAM bound and spatially upscales to display resolution so a 4K viewport renders at a fraction of the pixel cost; `ResolvePolicy` binds each `PERF_BUDGET` `QualityTier` rank to its `ResolvePass` through the frozen `int -> ResolvePass` table (`ByTier`, ranks 4..0) so the governor steps the full ladder `Taa(4,3) -> Smaa(2) -> Msaa(1) -> Fsr(0)` on the same hysteresis band that degrades the render passes — the high tiers spend pixels on temporal quality and the floor tier trades resolution for budget; the `Taa` motion-vector buffer is ONE `Render/meshlets` `BindlessTable` slot, never a parallel motion-vector owner; the resolve is a `Composite` policy column and a parallel post-process engine is the deleted form.
- Receipt: `FrameReceipt` — frame ordinal, per-pass `Duration` seq, GPU `Duration`, triangles drawn, budget verdict, `Instant`, `CorrelationId`; sealed through `ReceiptSinkPort` as a `Render`-family fact; `TelemetryRow` contributes the frame-elapsed, gpu-elapsed, and budget-overrun instruments inward through `TelemetryContributorPort`.
- Packages: SkiaSharp, Avalonia.Skia, Avalonia (compositor GPU interop), Silk.NET.WebGPU, Silk.NET.WebGPU.Native.WGPU, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm.AppHost (project)
- Growth: a new frame stage is one `RenderPass` case breaking the topological dispatch at compile time; a new backend is one `GpuBackend` row carrying its `RenderTargetFactory` column — Skia Graphite re-admits as one `SkiaGraphite` row the moment SkiaSharp ships its Recorder/Context surface; zero new surface.
- Boundary: `RenderGraph` is the named boundary capsule — the lease open-and-dispose pair and the topological pass walk carry the only statement bodies; the shared GPU context arrives as one `SurfaceSeam`-bound platform-lease delegate so no pass body names a `GRContext.CreateMetal`/`CreateVulkan` factory at a call site, deferring to the surface-hosts `EMBED_CAPSULE` shared-context law — a direct GPU-backend construction inside a pass arm is the rejected form (PROHIBITION host-API-in-arm); the per-backend target construction is the `GpuBackend` `RenderTargetFactory` column so the `Metal`/`Vulkan`/`OpenGl`/`Software` rows fold the leased `GRContext` to `SKSurface.Create(GRRecordingContext, GRBackendRenderTarget, ...)`, the `Wgpu` row folds the `Silk.NET.WebGPU` `Device`/`Queue`/`Surface` wgpu swapchain presenting through the compositor `ICompositionGpuInterop.ImportImage` seam, and the `WebGpu` row folds the browser surface, so a pass-emit body never names a backend target factory at a call site and a substrate swap is one backend row; the GPU passes (`Geometry` cluster draw through the wgpu mesh-shader pipeline, `PathTrace` through the wgpu compute pass, `Sim` volume ray-march, the reality-capture `Splat`/`Point` composites) SPIKE-gate on the live host-shared context and the `Composite` 2D-Skia raster is the deterministic CPU fallback; `ViewportClock` rides the AppHost `ClockPolicy` so frame timing is the one clock seam and a stopwatch is the rejected form; the frame ordinal is a monotone `Interlocked.Increment` over the graph-local counter so each `FrameReceipt` carries a distinct ordinal the correlation join and the render-hash lane key on, and a hardcoded zero ordinal is the deleted form; the receipt carries the folded per-pass list and the fold's `Budget` verdict so an overrun frame seals `WithinBudget: false` rather than an unconditional true, and every frame sinks one `FrameReceipt` through the one envelope and a per-pass meter is the deleted form; the meshlet cluster the graph draws is the `Render/meshlets` owner and the path-trace pass the `Render/pathtrace` integrator, so the pipeline composes them and re-models neither.

```csharp signature
[Union]
public abstract partial record ViewportFault : Expected, IValidationError<ViewportFault> {
    private ViewportFault(string detail, int code) : base(detail, code, None) { }

    public static ViewportFault Create(string message) => new Text(message);

    public sealed record Text : ViewportFault { public Text(string detail) : base(detail, 4500) { } }
    public sealed record ContextUnavailable : ViewportFault { public ContextUnavailable(string detail) : base(detail, 4501) { } }
    public sealed record BackendUnsupported : ViewportFault { public BackendUnsupported(string detail) : base(detail, 4502) { } }
    public sealed record BudgetExceeded : ViewportFault { public BudgetExceeded(string detail) : base(detail, 4503) { } }
    public sealed record LeaseRejected : ViewportFault { public LeaseRejected(string detail) : base(detail, 4504) { } }
}

public sealed record RenderTargetFactory(GpuBackend Backend, Func<SKImageInfo, Fin<RenderTarget>> Target) {
    public Fin<RenderTarget> Lease(SKImageInfo info) => Target(info);
}

[SmartEnum<string>]
public sealed partial class GpuBackend {
    public static readonly GpuBackend Metal = new("metal", GpuFamily.SkiaGanesh);
    public static readonly GpuBackend Vulkan = new("vulkan", GpuFamily.SkiaGanesh);
    public static readonly GpuBackend OpenGl = new("opengl", GpuFamily.SkiaGanesh);
    public static readonly GpuBackend Software = new("software", GpuFamily.SkiaRaster);
    public static readonly GpuBackend Wgpu = new("wgpu", GpuFamily.Wgpu);
    public static readonly GpuBackend WebGpu = new("webgpu", GpuFamily.WebGpu);

    public GpuFamily Family { get; }

    public bool IsGpu => Family != GpuFamily.SkiaRaster;
}

public enum GpuFamily { SkiaGanesh, SkiaRaster, Wgpu, WebGpu }

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RenderPass {
    private RenderPass() { }
    public sealed record Cull(string Key, Func<RenderTarget, MeshletCluster, Fin<int>> Visible) : RenderPass;
    public sealed record Geometry(string Key, Func<RenderTarget, MeshletCluster, int, Fin<int>> Draw) : RenderPass;
    public sealed record PathTrace(string Key, PathTracePass Pass) : RenderPass;
    public sealed record Sim(string Key, SimVisual Visual, SimField Field) : RenderPass;
    public sealed record Composite(string Key, Func<SKCanvas, Fin<Unit>> Raster) : RenderPass;
    public sealed record Overlay(string Key, Func<SKCanvas, Fin<Unit>> Draw) : RenderPass;

    public string Key => Switch(
        cull: static c => c.Key, geometry: static g => g.Key, pathTrace: static p => p.Key,
        sim: static s => s.Key, composite: static c => c.Key, overlay: static o => o.Key);
}

public readonly record struct ResolveState(
    long Ordinal,
    (double X, double Y) Jitter,
    Option<RenderTarget> History,
    double RenderScale);

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

    public ResolveState Advance(ResolveState prior, RenderTarget target) =>
        Reproject
            ? prior with {
                Ordinal = prior.Ordinal + 1,
                Jitter = HaltonJitter[(int)((prior.Ordinal + 1) % HaltonJitter.Length)],
                History = Some(target),
                RenderScale = RenderScale,
            }
            : prior with { Ordinal = prior.Ordinal + 1, Jitter = (0d, 0d), History = None, RenderScale = RenderScale };

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
        ByTier.TryGetValue(Math.Clamp(tierRank, 0, 4), out var pass) ? pass : ResolvePass.Msaa;
}

public sealed record RenderTarget(GpuBackend Backend, Option<SKSurface> Surface, Option<GRContext> Context, SKImageInfo Info, IDisposable Native) : IDisposable {
    public void Dispose() => Native.Dispose();
}

public sealed record FrameBudget(Duration Frame, long VramBytes, int MaxTriangles) {
    public static readonly FrameBudget Sixty = new(Duration.FromMilliseconds(16.667), 1_073_741_824L, 20_000_000);
    public static readonly FrameBudget Thirty = new(Duration.FromMilliseconds(33.333), 536_870_912L, 8_000_000);
}

public sealed record ViewportClock(ClockPolicy Clocks, CorrelationId Correlation);

public sealed record FrameReceipt(
    long Ordinal,
    GpuBackend Backend,
    Seq<(string Pass, Duration Elapsed)> Passes,
    Duration Gpu,
    long Triangles,
    bool WithinBudget,
    Instant At,
    CorrelationId Correlation) {
    public const string Kind = "frame";
}

public sealed record RenderGraph(
    Seq<RenderPass> Passes,
    MeshletCluster Cluster,
    RenderTargetFactory Factory,
    RenderTargetFactory Fallback,
    Func<Func<RenderTarget, Fin<FrameReceipt>>, Fin<FrameReceipt>> Lease,
    Func<FrameReceipt, IO<Unit>> Sink) {
    private long ordinal;

    public IO<FrameReceipt> Frame(ViewportClock clock, FrameBudget budget) =>
        from start in IO.lift(clock.Clocks.Mark)
        from next in IO.lift(() => Interlocked.Increment(ref ordinal))
        from frame in IO.lift(() => Render(clock.Clocks, budget).IfFail(fault => Empty(clock, fault)))
        from receipt in IO.pure(frame with { Ordinal = next, At = clock.Clocks.Now, Correlation = clock.Correlation })
        from _ in Sink(receipt)
        select receipt;

    private Fin<FrameReceipt> Render(ClockPolicy clocks, FrameBudget budget) =>
        Lease(target => Passes
            .Fold(
                Fin.Succ(new PassFold(Seq<(string, Duration)>(), 0L, true)),
                (rail, pass) => rail.Bind(state => Execute(pass, target, clocks, budget, state)))
            .Map(folded => new FrameReceipt(
                0L, Factory.Backend, folded.Passes,
                folded.Passes.Map(static p => p.Elapsed).Fold(Duration.Zero, static (a, d) => a + d),
                folded.Triangles, folded.Budget, default, default)));

    private readonly record struct PassFold(Seq<(string Pass, Duration Elapsed)> Passes, long Triangles, bool Budget);

    private Fin<PassFold> Execute(RenderPass pass, RenderTarget target, ClockPolicy clocks, FrameBudget budget, PassFold state) =>
        state.Triangles > budget.MaxTriangles
            ? Fin.Fail<PassFold>(new ViewportFault.BudgetExceeded(pass.Key))
            : clocks.Mark() switch {
                var mark => pass.Switch(
                        state: (Target: target, Cluster),
                        cull: static (ctx, c) => c.Visible(ctx.Target, ctx.Cluster),
                        geometry: static (ctx, g) => g.Draw(ctx.Target, ctx.Cluster, ctx.Cluster.Meshlets.Count),
                        pathTrace: static (ctx, p) => p.Pass.Accumulate(ctx.Target, 1, 0L).Map(static drawn => (int)drawn),
                        sim: static (ctx, s) => s.Visual.Geometry(s.Field).Map(static path => path.PointCount),
                        composite: static (ctx, c) => ctx.Target.Surface.Match(Some: surface => c.Raster(surface.Canvas).Map(static _ => 0), None: () => Fin.Succ(0)),
                        overlay: static (ctx, o) => ctx.Target.Surface.Match(Some: surface => o.Draw(surface.Canvas).Map(static _ => 0), None: () => Fin.Succ(0)))
                    .Map(drawn => state with {
                        Passes = state.Passes.Add((pass.Key, clocks.Elapsed(mark))),
                        Triangles = state.Triangles + drawn,
                    }),
            };

    private FrameReceipt Empty(ViewportClock clock, Error fault) =>
        new(0L, GpuBackend.Software, Seq(("composite", Duration.Zero)), Duration.Zero, 0L, false, clock.Clocks.Now, clock.Correlation);

    public const string FrameInstrument = "rasm.appui.viewport.frame-elapsed";
    public const string GpuInstrument = "rasm.appui.viewport.gpu-elapsed";
    public const string OverrunInstrument = "rasm.appui.viewport.budget-overrun";

    public static TelemetryContributorPort TelemetryRow(string version) =>
        AppUiTelemetry.Contribute(version, FrameInstrument, GpuInstrument, OverrunInstrument);
}
```

```mermaid
flowchart LR
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
- Entry: `public Fin<RenderPass> Pass(SimField field, ColorSpaceAxis space)` — projects the field into one render pass; the transient-playback frame is a field index, never a wall-clock tick.
- Auto: the isosurface case marching-cubes-extracts the level set at the threshold, the volume case ray-marches the scalar field through the `TransferFunction` opacity-color map, the streamline case integrates the vector field through Runge-Kutta seeds, the glyph case places oriented arrow or tensor glyphs at the sample points, the deformation case warps the mesh by the displacement field at the playback frame, the mesh-quality case shades each cell by its scaled-Jacobian or aspect-ratio metric, and the parallel-coords case routes its multi-dimensional cells onto the `CustomVisual.ParallelCoordinates` fold so a parameter sweep reads one analytical chart; transient playback scrubs a field-index sequence so a deformation or transient field animates by frame index under the deterministic motion clock.
- Packages: SkiaSharp, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm.Compute (project)
- Growth: a new field visualization is one `SimVisual` case; a new transfer-function ramp is one `Colormap` row consumed here; zero new surface.
- Boundary: the field geometry projects off the Compute field receipts — the page never re-computes a simulation, it renders the receipt; the volume transfer function samples the perceptually-uniform `Colormap` catalog so a scalar field maps through one lightness-monotone scale and a hand-rolled rainbow ramp is the deleted form; the kinematic viewport is the deformation case scrubbed by playback frame and the transient field scrub is the same field-index sequence, both under the deterministic motion clock so a wall-clock animation is the rejected form; the GPU volume ray-march and isosurface tessellation bind through the render-graph lease under VIEWPORT_GPU and a CPU marching-cubes isosurface plus a CPU ray-march are the floor while the GPU dispatch is the SPIKE; this pass is SPIKE-gated on the `Render/meshlets` GPU surface.

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

public sealed record TransferFunction(Colormap Ramp, double Floor, double Ceiling, double OpacityGamma) {
    public static readonly TransferFunction Default = new(Colormap.Viridis, Floor: 0d, Ceiling: 1d, OpacityGamma: 2.0);

    public (Color Color, double Opacity) Sample(double scalar, Func<Color, Color, double, Color> mix) =>
        Math.Clamp((scalar - Floor) / (Ceiling - Floor), 0d, 1d) switch {
            var t => (Ramp.Sample(t, mix), Math.Pow(t, OpacityGamma)),
        };
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SimVisual {
    private SimVisual() { }
    public sealed record Isosurface(string Key, double Threshold, Func<SimField, double, Fin<SKPath>> March) : SimVisual;
    public sealed record Volume(string Key, TransferFunction Transfer, Func<SimField, TransferFunction, Fin<Unit>> RayMarch) : SimVisual;
    public sealed record Streamline(string Key, int Seeds, double StepSize, Func<SimField, int, double, Fin<SKPath>> Integrate) : SimVisual;
    public sealed record Glyph(string Key, double Scale, Func<SimField, double, Fin<SKPath>> Place) : SimVisual;
    public sealed record Deformation(string Key, double Magnify, Func<SimField, double, int, Fin<SKPath>> Warp) : SimVisual;
    public sealed record MeshQuality(string Key, Colormap Ramp, Func<SimField, Colormap, Fin<SKPath>> Shade) : SimVisual;
    public sealed record ParallelCoords(string Key, Func<SimField, Fin<CustomVisualData>> Project) : SimVisual;

    public string Key => Switch(
        isosurface: static i => i.Key, volume: static v => v.Key, streamline: static s => s.Key,
        glyph: static g => g.Key, deformation: static d => d.Key, meshQuality: static m => m.Key, parallelCoords: static p => p.Key);

    public Fin<SKPath> Geometry(SimField field) => Switch(
        state: field,
        isosurface: static (f, i) => i.March(f, i.Threshold),
        volume: static (_, _) => Fin.Fail<SKPath>(new ViewportFault.Text("sim/volume-ray-march: volume renders to surface, not path")),
        streamline: static (f, s) => s.Integrate(f, s.Seeds, s.StepSize),
        glyph: static (f, g) => g.Place(f, g.Scale),
        deformation: static (f, d) => d.Warp(f, d.Magnify, f.FrameIndex),
        meshQuality: static (f, m) => m.Shade(f, m.Ramp),
        parallelCoords: static (_, _) => Fin.Fail<SKPath>(new ViewportFault.Text("sim/parallel-coords: routes to CustomVisual")));
}
```

## [04]-[VIEWPOINT_CODEC]

- Owner: `Viewpoint` the portable view-state receipt; `SectionBox` the clip volume; `VisibilityOverride` the per-element visibility-and-color row; `ViewpointCodec` the projection binding the receipt to the `cs:Rasm.Bim/Review/issues#BCF_ARCHIVE` `BcfViewpoint` exchange contract, never an AppUi-local BCF viewpoint schema.
- Entry: `public string Encode(JsonSerializerOptions wire)` — serializes the camera, section box, visibility set, color overrides, and selection into one portable JSON receipt; `public static Fin<Viewpoint> Decode(string blob, JsonSerializerOptions wire)` — round-trips a stored or shared viewpoint.
- Auto: a viewpoint captures the full reproducible view state in one receipt — the perspective-or-orthographic camera with its field-of-view, the active section-box clip planes, the per-element visibility and color-override set keyed by element guid, and the current selection — so a saved view, a shared markup, and a coordination issue carry the same portable shape; the BCF projection maps the camera onto the BCF `PerspectiveCamera`/`OrthogonalCamera` fields and the visibility set onto the BCF `Components` visibility and coloring so a viewpoint exports to and imports from an open BCF topic without a second view model.
- Receipt: `Viewpoint` serializes through the package wire context as a versioned portable receipt the dashboard, the markup, and the cross-process coordination consume.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm.Bim (project), BCL inbox
- Growth: a new view-state field is one `Viewpoint` member; a new override channel is one `VisibilityOverride` column; zero new surface.
- Boundary: the viewpoint is the one portable view-state owner — a per-feature camera-snapshot shape is the deleted form, and the section box, visibility, override, and selection all ride this one receipt so a coordination markup and a saved camera share it; `ViewpointCodec` projects the receipt onto the one `cs:Rasm.Bim/Review/issues#BCF_ARCHIVE` `BcfViewpoint` exchange contract (the host-free `System.Numerics.Vector3` camera triplet, the `SelectedGlobalIds`/`VisibleGlobalIds` GlobalId sets) so a viewpoint round-trips an external BCF tool through the Bim-owned record and an AppUi-local BCF viewpoint schema is the deleted form — the colour override is a render-only channel the receipt owns and the cross-tool viewpoint deliberately does not carry; the viewpoint binds onto the render-graph camera and section pass at apply time and the GPU clip is the render-graph consequence under VIEWPORT_GPU; the viewpoint receipt is host-local — its camera and section apply onto the 2D-fallback projection and onto the GPU clip when the viewport context lands; the `Editing/issues` board and the `Editing/tour` saved-viewpoints consume this one receipt so a coordination viewpoint mints no second camera-snapshot shape.

```csharp signature
public readonly record struct ViewCamera(
    bool Perspective,
    double EyeX, double EyeY, double EyeZ,
    double TargetX, double TargetY, double TargetZ,
    double UpX, double UpY, double UpZ,
    double FieldOfView,
    double OrthoScale);

public readonly record struct SectionBox(
    double MinX, double MinY, double MinZ,
    double MaxX, double MaxY, double MaxZ,
    bool Enabled);

public readonly record struct VisibilityOverride(string ElementId, bool Visible, Option<uint> ColorArgb, double Transparency);

public sealed record Viewpoint(
    string Key,
    int Version,
    ViewCamera Camera,
    SectionBox Section,
    Seq<VisibilityOverride> Overrides,
    Seq<string> Selection,
    Instant At) {
    public const int Schema = 1;

    public static Fin<Viewpoint> Capture(string key, ViewCamera camera, SectionBox section, Seq<VisibilityOverride> overrides, Seq<string> selection, ClockPolicy clocks) =>
        overrides.Map(static o => o.ElementId).Distinct().Count == overrides.Count
            ? Fin.Succ(new Viewpoint(key, Schema, camera, section, overrides, selection, clocks.Now))
            : Fin.Fail<Viewpoint>(new ViewportFault.Text($"viewpoint/duplicate-override:{key}"));

    public string Encode(JsonSerializerOptions wire) => JsonSerializer.Serialize(this, wire);

    public static Fin<Viewpoint> Decode(string blob, JsonSerializerOptions wire) =>
        Optional(JsonSerializer.Deserialize<Viewpoint>(blob, wire)) is { IsSome: true, Case: Viewpoint view } && view.Version == Schema
            ? Fin.Succ(view)
            : Fin.Fail<Viewpoint>(new ViewportFault.Text("viewpoint/decode: blob version mismatch or malformed"));
}

// The viewpoint <-> BCF projection binds the portable `Viewpoint` receipt to the one
// `Rasm.Bim.Coordination.BcfViewpoint` exchange contract the Bim owner mints — AppUi re-mints no
// BCF viewpoint schema, the camera triplet is the host-free `System.Numerics.Vector3` the Bim record
// carries (position = eye, direction = target - eye, up), and the selection/visibility ride the
// `SelectedGlobalIds`/`VisibleGlobalIds` GlobalId sets. The per-element colour override is a render-only
// channel the `Viewpoint` receipt owns and the cross-tool BCF viewpoint deliberately does not carry.
public static class ViewpointCodec {
    public static Rasm.Bim.Coordination.BcfViewpoint ToBcf(string guid, Viewpoint view) =>
        new(
            guid,
            new System.Numerics.Vector3((float)view.Camera.EyeX, (float)view.Camera.EyeY, (float)view.Camera.EyeZ),
            new System.Numerics.Vector3((float)(view.Camera.TargetX - view.Camera.EyeX), (float)(view.Camera.TargetY - view.Camera.EyeY), (float)(view.Camera.TargetZ - view.Camera.EyeZ)),
            new System.Numerics.Vector3((float)view.Camera.UpX, (float)view.Camera.UpY, (float)view.Camera.UpZ),
            view.Camera.Perspective ? view.Camera.FieldOfView : view.Camera.OrthoScale,
            view.Selection,
            view.Overrides.Filter(static o => o.Visible).Map(static o => o.ElementId),
            Option<ReadOnlyMemory<byte>>.None);

    public static Viewpoint FromBcf(string key, Rasm.Bim.Coordination.BcfViewpoint bcf, ClockPolicy clocks) =>
        new(
            key, Viewpoint.Schema,
            new ViewCamera(bcf.CameraDirection.LengthSquared() > 0f,
                bcf.CameraPosition.X, bcf.CameraPosition.Y, bcf.CameraPosition.Z,
                bcf.CameraPosition.X + bcf.CameraDirection.X, bcf.CameraPosition.Y + bcf.CameraDirection.Y, bcf.CameraPosition.Z + bcf.CameraDirection.Z,
                bcf.CameraUpVector.X, bcf.CameraUpVector.Y, bcf.CameraUpVector.Z, bcf.FieldOfView, bcf.FieldOfView),
            new SectionBox(0d, 0d, 0d, 0d, 0d, 0d, false),
            bcf.VisibleGlobalIds.Map(static id => new VisibilityOverride(id, true, None, 0d)),
            bcf.SelectedGlobalIds, clocks.Now);
}
```

## [05]-[TS_PROJECTION]

- Owner: `ViewpointWire`, `ViewCameraWire`, `SectionBoxWire`, `VisibilityOverrideWire`, `FrameReceiptWire`, `GeometryResidencyWire`, `ResidencyTileWire`, `MeshoptStreamWire`, `MeshletWire` — the viewpoint, frame-evidence, and content-keyed geometry-residency wire contract a WebGPU web viewer and a cross-process coordination tool consume; `ResidencyManifest` the single C# mint of the `WEB_GEOMETRY_RESIDENCY_WIRE` portable scene-graph + kind-discriminated residency-tile manifest, each tile a 1:1 projection of one Compute `csharp:Rasm.Compute/Runtime/payload#RESIDENCY` `ResidencyPayload`; `ResidencyMarshal` the projection algebra folding each resident payload into its EXT_meshopt_compression wire row; the GPU pass internals and the suite `XxHash128` content key (minted by Compute, never re-computed here) never cross the wire.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm.Compute (project), BCL inbox
- Growth: one wire member row per new viewpoint field, frame-receipt field, or residency-tile field; a new residency kind or stream is one already-discriminated `ResidencyTileWire.kind` value or one `MeshoptStreamWire` row, never a new tile type; one `ResidencyMarshal` projection arm per new manifest member; zero new surface.
- Boundary: shapes transcribe the camelCase Strict emission — the camera crosses as its `eye`/`target`/`up` three-axis tuples plus the field-of-view and ortho-scale scalars, the section box as its min/max three-axis tuples plus the enabled flag, each visibility override as its element id, visible flag, `number | null` color (the desktop `Option<uint>` projected through `ToNullable`), and transparency, the selection as an ordinal-string array, each tile/splat bounds as the `[x, y, z, radius]` four-tuple, the content key as the `:x32` 32-hex string (the desktop `UInt128` rendered through `KeyHex`, never the raw `UInt128` STJ would emit as a JSON number), instants as `InstantPattern.ExtendedIso` text, and durations as round-trip text; the frame receipt crosses for the live performance HUD so a web dashboard reads frame budget without the GPU pass internals; the viewpoint crosses as the projected `ViewpointWire`, never the desktop `Viewpoint`, so a BCF tool, a web viewer, and the desktop share one portable view-state; `ResidencyMarshal` carries every payload-to-wire projection as one fold so a wire member never marshals at a call site, `ResidencyManifest.Encode` serializes through the source-generated `ResidencyWireContext` (camelCase, disallow-unmapped, nullable-respecting) so the emission is exactly the shape the TS `GeometryResidencyWireSchema` decodes, and `ResidencyManifest.Mint` is the single producer of the `WEB_GEOMETRY_RESIDENCY_WIRE` — the AppUi `Render/meshlets` `ResidencyBudget` decides which content-addressed Compute `csharp:Rasm.Compute/Runtime/payload#RESIDENCY` `ResidencyPayload`s are resident at which scene cells, and `Mint` projects each resident payload 1:1 into a kind-discriminated `ResidencyTileWire` carrying the payload's EXT_meshopt_compression bufferViews (the `StreamSpan` mode/filter/count/stride per stream), its meshopt-built `ResidencyMeshlet` clusters (the vertex-table + triangle split), its tile bounding sphere, its harmonic degree, and its `ContentKey`, so the TypeScript worker decodes the meshopt-compressed blob and drives a WebGPU viewport off the same Compute payload the desktop and Persistence read; each tile keys by the Compute payload's own `ContentKey` (minted once at `csharp:Rasm.Compute/Runtime/payload#RESIDENCY` `InterchangeIdentity.Key` over the whole encoded blob under the suite `Runtime/codecs#CONTENT_ADDRESSING` law), rendered to the `:x32` wire string through `KeyHex` at the marshal seam — AppUi mints no second hash and never re-folds raw positions, so `ResidencyTileWire.contentKey` is the payload key and `ResidencyTileWire.blobKey` is its Persistence blob-lane address, both distinct from the scene-cell `key`, and the worker fetches tile bytes by the content-addressed blob key, never by the scene-cell name; the manifest crosses the cross-language wire only and no desktop owner reverses onto the web leg, so a second residency manifest on the TS side is the rejected form (the single-mint invariant graded at the cross-`libs/` master); the meshlet, quantized, and point residency arms resolve now off the present Compute `ResidencyPayload` (meshlet-cluster/quantized-vertex/point-splat kinds), the gaussian-splat tile arm projects the gaussian-splat `ResidencyPayload` now, and only the upstream Compute splat-payload decode that feeds the gaussian-splat payload stays honestly `[UPSTREAM-BLOCKED]` on the Python SOG/PLY/LAZ two-hop; depends on `T-BACKEND-PORT` (the `WebGpu` `GpuBackend` row the web viewport binds) and the Compute `csharp:Rasm.Compute/Runtime/payload#RESIDENCY` residency keying so the splat tile and the meshlet tile share one content-key scheme across the wire.

```ts contract
interface ViewCameraWire {
  readonly perspective: boolean;
  readonly eye: readonly [number, number, number];
  readonly target: readonly [number, number, number];
  readonly up: readonly [number, number, number];
  readonly fieldOfView: number;
  readonly orthoScale: number;
}

interface SectionBoxWire {
  readonly min: readonly [number, number, number];
  readonly max: readonly [number, number, number];
  readonly enabled: boolean;
}

interface VisibilityOverrideWire {
  readonly elementId: string;
  readonly visible: boolean;
  readonly colorArgb: number | null;
  readonly transparency: number;
}

interface ViewpointWire {
  readonly key: string;
  readonly version: number;
  readonly camera: ViewCameraWire;
  readonly section: SectionBoxWire;
  readonly overrides: readonly VisibilityOverrideWire[];
  readonly selection: readonly string[];
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

The C# `ResidencyManifest` is the single mint of the `WEB_GEOMETRY_RESIDENCY_WIRE`; the TypeScript worker consumes the manifest by content key and never re-mints it. The content key is the suite raw `UInt128` owned at `csharp:Compute/Runtime/codecs#CONTENT_ADDRESSING` and consumed here from the Compute `csharp:Rasm.Compute/Runtime/payload#RESIDENCY` `ResidencyPayload.ContentKey` (not re-folded) — AppUi mints no second content-identity value object, the `:x32` form is the shared wire spelling, and `ResidencyMarshal` carries every payload→wire projection body as the law; the gaussian-splat tile arm projects a present Compute gaussian-splat `ResidencyPayload`, while the upstream Compute splat-payload decode that feeds it (the `Runtime/payload#RESIDENCY` `SplatScan` admission at the `Runtime/channels` `GaussianSplatScan` wire) stays honestly `[UPSTREAM-BLOCKED]`.

```csharp signature
public readonly record struct ViewCameraWire(bool Perspective, double[] Eye, double[] Target, double[] Up, double FieldOfView, double OrthoScale);

public readonly record struct SectionBoxWire(double[] Min, double[] Max, bool Enabled);

public readonly record struct VisibilityOverrideWire(string ElementId, bool Visible, uint? ColorArgb, double Transparency);

public sealed record ViewpointWire(string Key, int Version, ViewCameraWire Camera, SectionBoxWire Section, Seq<VisibilityOverrideWire> Overrides, Seq<string> Selection, string At);

public readonly record struct MeshoptStreamWire(string Stream, string Mode, string Filter, int ByteOffset, int ByteLength, int Count, int ByteStride);

public sealed record MeshletWire(
    int VertexOffset, int TriangleOffset, int VertexCount, int TriangleCount,
    double[] Center, double Radius, double[] ConeApex, double[] ConeAxis, double ConeCutoff);

public sealed record ResidencyTileWire(
    string Key, string Kind, string ContentKey, string BlobKey, long Bytes, int ResidentCount,
    int HarmonicDegree, double[] Bounds, Seq<MeshoptStreamWire> Streams, Seq<MeshletWire> Meshlets);

public static class ResidencyMarshal {
    public static string KeyHex(UInt128 content) => $"{content:x32}";

    public static string BlobKeyOf(UInt128 content) => $"geo/{content:x32}";

    public static ViewpointWire ViewpointOf(Viewpoint view) =>
        new(view.Key, view.Version,
            new ViewCameraWire(view.Camera.Perspective,
                [view.Camera.EyeX, view.Camera.EyeY, view.Camera.EyeZ],
                [view.Camera.TargetX, view.Camera.TargetY, view.Camera.TargetZ],
                [view.Camera.UpX, view.Camera.UpY, view.Camera.UpZ],
                view.Camera.FieldOfView, view.Camera.OrthoScale),
            new SectionBoxWire(
                [view.Section.MinX, view.Section.MinY, view.Section.MinZ],
                [view.Section.MaxX, view.Section.MaxY, view.Section.MaxZ],
                view.Section.Enabled),
            view.Overrides.Map(static o => new VisibilityOverrideWire(o.ElementId, o.Visible, o.ColorArgb.ToNullable(), o.Transparency)),
            view.Selection,
            InstantPattern.ExtendedIso.Format(view.At));

    // one EXT_meshopt_compression bufferView wire row off one Compute StreamSpan: the meshopt decode mode + inverse
    // filter + byte window + element count/stride the web worker hands the meshopt decoder, never re-derived here.
    public static MeshoptStreamWire StreamWire(ResidencyStream stream, StreamSpan span) =>
        new(stream.Key, span.Mode.Key, span.Filter.Key, span.Offset, span.Length, span.Count, span.ByteStride);

    public static MeshletWire MeshletWireOf(ResidencyMeshlet m) =>
        new(m.VertexOffset, m.TriangleOffset, m.VertexCount, m.TriangleCount,
            [m.Center.X, m.Center.Y, m.Center.Z], m.Radius,
            [m.ConeApex.X, m.ConeApex.Y, m.ConeApex.Z], [m.ConeAxis.X, m.ConeAxis.Y, m.ConeAxis.Z], m.ConeCutoff);

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
    public const int Schema = 1;

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

## [06]-[RESEARCH]

- [VIEWPORT_GPU]: the host-shared `GRContext` acquisition through `ISkiaSharpApiLease.TryLeasePlatformGraphicsApi` against the Rhino-owned Metal pipeline, the `GRMtlBackendContext`/`GRVkBackendContext` backend-context construction, the `SKSurface.Create(GRRecordingContext, GRBackendRenderTarget, ...)` GPU-target spelling the `Metal`/`Vulkan`/`OpenGl` `RenderTargetFactory` rows fold, the `SKRuntimeEffect` compute-and-mesh-shader emit path for the meshlet draw and the path-trace ray-generation, the per-backend bindless descriptor-table and acceleration-structure spellings (Metal argument buffers and ray-tracing, Vulkan descriptor indexing and ray-query), the `ResolvePass` live dispatch (the `Taa` motion-vector-reprojection compute pass and history-clamp, the `Fsr` sub-resolution spatial-upscale `Silk.NET.WebGPU` `ComputePassEncoder`/`SKRuntimeEffect` pass, the `Smaa` morphological edge pass) below the `Composite` `RenderTargetFactory`, and the WebGPU backend reach for the designed-only web viewport — the render-graph pass algebra, the `GpuBackend` `RenderTargetFactory` column, the `ResolvePass` ladder and the `ResolvePolicy` tier table and the `ResolveState` jitter-and-history Fold, the simulation render passes, and the viewpoint codec are settled as the CPU/2D-Skia fallback (the `Msaa`/`Smaa`/single-sample resolve runs on the CPU raster); the GPU dispatch, the shared-context lease, the live `Taa`/`Fsr` compute resolve, and the backend acceleration structures are the unverified surface gated on the live host-owned GPU context, de-risked standalone against a windowed `GRContext` and confirmed in-host against the embedded panel.
- [WGPU_BACKEND]: the `Wgpu` `RenderTargetFactory` row binding the `Silk.NET.WebGPU` wgpu/Dawn surface — `WebGPU.GetApi()`, `CreateInstance`, `InstanceRequestAdapter` on the compositor adapter (LUID/UUID matched through `ICompositionGpuInterop.DeviceLuid`/`DeviceUuid`), `AdapterRequestDevice`+`DeviceGetQueue`, `SurfaceConfigure`/`SurfaceGetCurrentTexture` for the swapchain, the `CommandEncoder`/`RenderPassEncoder`/`ComputePassEncoder` recording, `QueueSubmit`, and the `CompositionDrawingSurface.UpdateWithExternalImageAsync` import of the rendered shared texture — resolve against the admitted `Silk.NET.WebGPU` 2.23 surface (`.api/api-silk-webgpu.md`) and the Avalonia 12 compositor interop (`.api/api-avalonia-gpu-interop.md`); the backend rows, the `RenderTargetFactory` column shape, and the factory-bound pass algebra are settled, the wgpu device acquisition, the shared-texture export-and-import handshake (D3D11 keyed-mutex / Vulkan external-memory / Metal `IOSurface`), and the wgpu-versus-Skia present-path divergence below the factory are the unverified surface gated on the live GPU device, with `Silk.NET.WebGPU` a stable pinnable .NET Foundation identity and the `Software` Skia Ganesh raster row the shippable floor. Skia Graphite is not yet shipped; no `SkiaGraphite` row is admitted until SkiaSharp exposes the Recorder/Context surface, at which point the row re-admits with no other change.
- [WEB_RESIDENCY]: the `ResidencyManifest` is the single C# mint of the `WEB_GEOMETRY_RESIDENCY_WIRE` and the TypeScript `libs/typescript/ui` worker is its sole consumer — the manifest, the `ResidencyMarshal` projection algebra, the `BlobKeyOf` blob-lane addressing, and the `StreamWire`/`MeshletWireOf`/`TileOf` projection off each Compute `csharp:Rasm.Compute/Runtime/payload#RESIDENCY` `ResidencyPayload` (its EXT_meshopt_compression `StreamSpan` bufferViews, `ResidencyMeshlet` clusters, bounds, and `ContentKey`) are built and settled now, so the worker decodes the meshopt-compressed blob and drives a WebGPU viewport off the content-keyed tiles against the same Compute `Runtime/codecs#CONTENT_ADDRESSING` keying the desktop and Persistence read; the single-mint invariant (one producer, no TS-side re-mint) is graded at the cross-libs master against the `typescript:ui/render/glb#GLB_VIEWPORT` consume-only manifest row, and the `:x32` content-key spelling is the shared wire form. The gaussian-splat tile manifest arm projects a present Compute gaussian-splat `ResidencyPayload` now; only the upstream Compute splat-payload decode that feeds it (the `Runtime/payload#RESIDENCY` `SplatScan` admission at the `Runtime/channels` `GaussianSplatScan` wire) stays `[UPSTREAM-BLOCKED]` on the Python SOG/PLY/LAZ scan-decode two-hop, and the Python content-key reproduction of the `:x32` form stays `[UPSTREAM-BLOCKED]` on the `xxhash` cp315/abi3 wheel the companion lacks below 3.15. The WebGPU cluster-LOD upload on the browser device is the remaining `[HOST-PROBE-DEFERRED]` surface gated on the live WebGPU device — depends on the `WebGpu` `GpuBackend` row and the Compute `csharp:Rasm.Compute/Runtime/payload#RESIDENCY` residency keying.
