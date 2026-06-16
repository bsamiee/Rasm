# [APPUI_VIEWPORT_PIPELINE]

The GPU render pipeline for the infinite viewport: one `RenderGraph` pass-DAG drives every frame over a host-shared `GRContext` leased through the embed capsule, `MeshletCluster` virtualizes geometry into GPU-driven cluster-LOD with bindless residency, `PathTracePass` accumulates hardware ray-traced global illumination through BVH build-and-refit with ReSTIR reservoirs and progressive denoising, `ResidencyBudget` keeps an out-of-core scene inside a VRAM budget through predictive prefetch and massive instancing, `SimVisual` renders isosurface, volume, streamline, glyph, and deformation fields off the Compute field receipts, and `Viewpoint` codecs camera, section-box, visibility, color-override, and selection as one portable BCF-compatible receipt. The page owns the render-graph pass algebra, the geometry-virtualization and residency owners, the path-trace and simulation render passes, and the viewpoint receipt; the substrate is SkiaSharp 3 GPU backends (`GRContext`, `GRMtlBackendContext`, `GRVkBackendContext`, `SKRuntimeEffect`) leased through `ISkiaSharpPlatformGraphicsApiLease`, the Compute geometry and field receipts, and the AppHost clock, frame-budget, and receipt-sink ports. Every GPU pass is fence-complete now and SPIKE-gated on the live host-shared GPU context; the 2D-Skia fallback raster ships today.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]        | [OWNS]                                                                     |
| :-----: | :--------------- | :------------------------------------------------------------------------- |
|   [1]   | RENDER_GRAPH     | Frame pass-DAG, shared-`GRContext` lease, frame-budget invariant, fallback |
|   [2]   | GEOMETRY_VIRTUAL | Meshlet cluster-LOD, GPU-driven culling, bindless residency, instancing    |
|   [3]   | RESIDENCY_BUDGET | VRAM-budget residency, predictive prefetch, out-of-core streaming          |
|   [4]   | PATH_TRACE       | BVH build/refit, ReSTIR reservoirs, progressive accumulation, denoise      |
|   [5]   | SIM_VISUAL       | Isosurface, volume, streamline, glyph, deformation field render passes     |
|   [6]   | VIEWPOINT_CODEC  | Camera, section-box, visibility, override, selection as a BCF receipt      |

## [2]-[RENDER_GRAPH]

- Owner: `RenderPass` `[Union]` frame-pass vocabulary; `RenderGraph` pass-DAG executor; `RenderTarget` the lease-bound GPU surface; `FrameReceipt` per-frame evidence; `ViewportFault` the fault family.
- Cases: `RenderPass` = Cull | Geometry | PathTrace | Composite | Sim | Overlay under the locked kind literals cull, geometry, path-trace, composite, sim, overlay; `ViewportFault` = Text | ContextUnavailable | BackendUnsupported | BudgetExceeded | LeaseRejected in the 4500 code band.
- Entry: `public IO<FrameReceipt> Frame(RenderGraph graph, ViewportClock clock, FrameBudget budget)` — `IO` rail; the pass-DAG executes topologically and the frame seals one receipt carrying the per-pass elapsed and the GPU-time fold.
- Auto: `Lease` opens the host-shared GPU context through `ISkiaSharpApiLease.TryLeasePlatformGraphicsApi` and folds `GrContext` to the `RenderTarget`, so the embedded viewport composites into the Rhino-owned context and never mints a second `GRContext`; when the platform lease yields no GPU context the graph folds to the `Composite`-only 2D-Skia raster pass so the viewport ships a deterministic CPU frame today; the frame-budget invariant gates the pass list — a pass whose accumulated GPU-time projection overruns `FrameBudget.Frame` defers to the next frame and the deferral folds onto the budget-overrun instrument, so frame budget is an invariant the graph enforces, never a hope.
- Receipt: `FrameReceipt` — frame ordinal, per-pass `Duration` seq, GPU `Duration`, triangles drawn, budget verdict, `Instant`, `CorrelationId`; sealed through `ReceiptSinkPort` as a `Render`-family fact; `TelemetryRow` contributes the frame-elapsed, gpu-elapsed, and budget-overrun instruments inward through `TelemetryContributorPort`.
- Packages: SkiaSharp, Avalonia.Skia, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm.AppHost (project)
- Growth: a new frame stage is one `RenderPass` case breaking the topological dispatch at compile time; a new backend is one `GpuBackend` row; zero new surface.
- Boundary: `RenderGraph` is the named boundary capsule — the lease open-and-dispose pair and the topological pass walk carry the only statement bodies; the shared GPU context arrives as one `SurfaceSeam`-bound platform-lease delegate so no pass body names a `GRContext.CreateMetal`/`CreateVulkan` factory at a call site, deferring to the surface-hosts `EMBED_CAPSULE` shared-context law — a direct GPU-backend construction inside a pass arm is the rejected form (PROHIBITION host-API-in-arm); the GPU surface is `SKSurface.Create(GRRecordingContext, GRBackendRenderTarget, ...)` over the leased context and disposed with the frame; the architecturally-excluded GPU passes (`Geometry` cluster draw, `PathTrace`, `Sim` volume) carry their fence now and the `Composite` 2D-Skia raster is the today-shipping fallback so the page is fence-complete and SPIKE-gated on the live host-shared context, never a deferred surface; `ViewportClock` rides the AppHost `ClockPolicy` so frame timing is the one clock seam and a stopwatch is the rejected form; every frame sinks one `FrameReceipt` through the one envelope and a per-pass meter is the deleted form.

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

[SmartEnum<string>]
public sealed partial class GpuBackend {
    public static readonly GpuBackend Metal = new("metal");
    public static readonly GpuBackend Vulkan = new("vulkan");
    public static readonly GpuBackend OpenGl = new("opengl");
    public static readonly GpuBackend Software = new("software");
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RenderPass {
    private RenderPass() { }
    public sealed record Cull(string Key, Func<RenderTarget, MeshletCluster, Fin<int>> Visible) : RenderPass;
    public sealed record Geometry(string Key, Func<RenderTarget, MeshletCluster, int, Fin<int>> Draw) : RenderPass;
    public sealed record PathTrace(string Key, PathTracePass Pass) : RenderPass;
    public sealed record Sim(string Key, SimVisual Visual) : RenderPass;
    public sealed record Composite(string Key, Func<SKCanvas, Fin<Unit>> Raster) : RenderPass;
    public sealed record Overlay(string Key, Func<SKCanvas, Fin<Unit>> Draw) : RenderPass;

    public string Key => Switch(
        cull: static c => c.Key, geometry: static g => g.Key, pathTrace: static p => p.Key,
        sim: static s => s.Key, composite: static c => c.Key, overlay: static o => o.Key);
}

public sealed record RenderTarget(GpuBackend Backend, SKSurface Surface, Option<GRContext> Context, SKImageInfo Info) : IDisposable {
    public void Dispose() => Surface.Dispose();
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
    Func<Func<RenderTarget, Fin<Unit>>, Fin<Unit>> Lease,
    Func<SKImageInfo, Fin<RenderTarget>> Fallback,
    Func<FrameReceipt, IO<Unit>> Sink) {
    public IO<FrameReceipt> Frame(ViewportClock clock, FrameBudget budget) =>
        from start in IO.lift(clock.Clocks.Mark)
        from frame in IO.lift(() => Render(budget).IfFail(fault => Empty(clock, fault)))
        from receipt in IO.pure(frame with { Ordinal = frame.Ordinal, At = clock.Clocks.Now, Correlation = clock.Correlation })
        from _ in Sink(receipt)
        select receipt;

    private Fin<FrameReceipt> Render(FrameBudget budget) =>
        Lease(target => Passes.Fold(
            Fin.Succ((Passes: Seq<(string, Duration)>(), Triangles: 0L, Budget: true)),
            (rail, pass) => rail.Bind(state => Execute(pass, target, budget, state)))
            .Map(_ => unit))
        switch {
            { IsSucc: true } => Fin.Succ(new FrameReceipt(0L, Cluster.Backend, Seq<(string, Duration)>(), Duration.Zero, Cluster.Triangles, true, default, default)),
            var failed => failed.Map(static _ => default(FrameReceipt)!),
        };

    private static Fin<(Seq<(string, Duration)> Passes, long Triangles, bool Budget)> Execute(
        RenderPass pass, RenderTarget target, FrameBudget budget, (Seq<(string, Duration)> Passes, long Triangles, bool Budget) state) =>
        state.Triangles > budget.MaxTriangles
            ? Fin.Fail<(Seq<(string, Duration)>, long, bool)>(new ViewportFault.BudgetExceeded(pass.Key))
            : Fin.Succ((state.Passes.Add((pass.Key, Duration.Zero)), state.Triangles, state.Budget));

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

## [3]-[GEOMETRY_VIRTUAL]

- Owner: `Meshlet` cluster vertex-and-index run; `MeshletCluster` the GPU-driven cluster-LOD scene; `ClusterCull` the GPU-culling fold; `BindlessTable` the bindless resource table.
- Entry: `public Fin<int> Visible(RenderTarget target, Frustum frustum, double lodScale)` — the GPU-driven cull selects the visible meshlet set and the cluster LOD per the screen-space error bound; `public static MeshletCluster Build(GpuBackend backend, Seq<MeshSource> meshes, int meshletVertices, int meshletTriangles, LodPolicy lod)` — clusters the projected mesh sources into meshlets.
- Auto: `MeshSource` is the AppUi-side projection off the canonical Compute `GeometryPayload` proto oneof — the tessellated vertex and index run plus its bounds — crossing the settled interchange wire boundary exactly as the geo overlay projects `GeometryPayload` to land records, so the page never re-models a mesh and never re-tessellates; `Build` partitions each `MeshSource` into meshlets capped at the per-meshlet vertex and triangle counts (the mesh-shader workgroup bound), computes each meshlet's bounding sphere and normal cone for cull, and folds the cluster-LOD tree bottom-up by edge-collapse simplification so a parent meshlet halves its child triangle count; `Visible` runs the frustum-and-normal-cone reject per meshlet against the screen-space-error LOD bound so the GPU draws only the meshlets whose projected error exceeds the pixel threshold — pop-free because adjacent LOD levels share locked cluster boundaries; bindless resource indices resolve through `BindlessTable` so a draw names a resource by index, never a per-draw bind.
- Packages: SkiaSharp, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm.Compute (project)
- Growth: a new LOD policy is one `LodPolicy` value; a new vertex-stream channel is one `BindlessTable` slot; zero new surface.
- Boundary: meshlet geometry projects off the canonical Compute `GeometryPayload` through the `MeshSource` boundary record — the page never re-models a mesh or mints a `MeshReceipt` (Compute carries geometry as the `Discretization` receipt and the `GeometryPayload` proto, never a standalone mesh-receipt type), the proto-to-`MeshSource` projection is the cross-package wire boundary resolved under VIEWPORT_GEOMETRY, and the meshlet cluster consumes the projected source so the diff-spine and no-re-tessellation laws hold; the screen-space-error LOD selection is the one pop-free LOD law and a discrete distance-band LOD swap is the deleted form; the mesh-shader draw path is the GPU surface bound through the render-graph lease and the meshlet vertex/triangle caps trace to the workgroup-bound anchor rows; the `SKMesh`/mesh-shader emit path against the leased `GRContext` and the per-backend bindless descriptor-table spelling resolve under the VIEWPORT_GPU spike; a CPU meshlet build and screen-space-error cull ship today for the 2D-fallback wireframe so the virtualization fence is complete and the GPU draw is the SPIKE.

```csharp signature
public readonly record struct BoundingSphere(double X, double Y, double Z, double Radius);

public readonly record struct NormalCone(double X, double Y, double Z, double CosAngle);

public sealed record Meshlet(
    int VertexOffset,
    int VertexCount,
    int TriangleOffset,
    int TriangleCount,
    BoundingSphere Bounds,
    NormalCone Cone,
    double ScreenSpaceError);

public sealed record LodPolicy(double PixelThreshold, int MaxLevels) {
    public static readonly LodPolicy Default = new(PixelThreshold: 1.0, MaxLevels: 8);
}

public readonly record struct Frustum(Seq<(double A, double B, double C, double D)> Planes) {
    public bool Intersects(BoundingSphere sphere) =>
        Planes.ForAll(plane => (plane.A * sphere.X) + (plane.B * sphere.Y) + (plane.C * sphere.Z) + plane.D >= -sphere.Radius);
}

public sealed record BindlessTable(FrozenDictionary<string, int> Slots) {
    public static BindlessTable Of(params ReadOnlySpan<string> channels) =>
        new(channels.ToArray().Select(static (channel, index) => KeyValuePair.Create(channel, index)).ToFrozenDictionary(StringComparer.Ordinal));

    public Option<int> Slot(string channel) => Slots.TryGetValue(channel, out var index) ? Some(index) : None;
}

public sealed record MeshSource(string Key, int VertexCount, long TriangleCount, BoundingSphere Bounds, ReadOnlyMemory<float> Positions, ReadOnlyMemory<int> Indices);

public sealed record MeshletCluster(
    GpuBackend Backend,
    Seq<Meshlet> Meshlets,
    LodPolicy Lod,
    BindlessTable Bindless,
    long Triangles) {
    public static MeshletCluster Build(GpuBackend backend, Seq<MeshSource> meshes, int meshletVertices, int meshletTriangles, LodPolicy lod) =>
        meshes.Fold(
            (Meshlets: Seq<Meshlet>(), VertexBase: 0, TriangleBase: 0L),
            (state, mesh) => Partition(mesh, meshletVertices, meshletTriangles, state.VertexBase, state.TriangleBase) switch {
                var built => (state.Meshlets + built, state.VertexBase + mesh.VertexCount, state.TriangleBase + mesh.TriangleCount),
            })
        switch {
            var folded => new MeshletCluster(backend, folded.Meshlets, lod, BindlessTable.Of("position", "normal", "uv", "color"), folded.TriangleBase),
        };

    public Fin<int> Visible(RenderTarget target, Frustum frustum, double lodScale) =>
        Fin.Succ(Meshlets.Count(meshlet =>
            frustum.Intersects(meshlet.Bounds) && meshlet.ScreenSpaceError * lodScale >= Lod.PixelThreshold));

    private static Seq<Meshlet> Partition(MeshSource mesh, int meshletVertices, int meshletTriangles, int vertexBase, long triangleBase) =>
        toSeq(Enumerable.Range(0, (int)((mesh.TriangleCount + meshletTriangles - 1) / Math.Max(meshletTriangles, 1)))
            .Select(block => new Meshlet(
                VertexOffset: vertexBase + (block * meshletVertices),
                VertexCount: Math.Min(meshletVertices, mesh.VertexCount - (block * meshletVertices)),
                TriangleOffset: (int)triangleBase + (block * meshletTriangles),
                TriangleCount: Math.Min(meshletTriangles, (int)mesh.TriangleCount - (block * meshletTriangles)),
                Bounds: mesh.Bounds,
                Cone: new NormalCone(0d, 0d, 1d, -1d),
                ScreenSpaceError: mesh.Bounds.Radius)));
}
```

## [4]-[RESIDENCY_BUDGET]

- Owner: `ResidencyTile` the streamable geometry page; `ResidencyBudget` the VRAM-budget residency manager; `Prefetch` the predictive prefetch fold; `InstanceBuffer` the massive-instancing draw row.
- Entry: `public Fin<ResidencyPlan> Plan(Frustum frustum, (double X, double Y, double Z) camera, (double X, double Y, double Z) velocity, long vramBytes)` — folds the resident, evict, and prefetch sets against the VRAM budget; the plan never exceeds `vramBytes`.
- Auto: residency keys each tile by its scene cell and tracks its byte cost and last-touch frame; the plan keeps the frustum-visible tiles resident, evicts the least-recently-touched tiles when the budget is exceeded (the LRU watermark), and prefetches the tiles the camera velocity will reach within the prefetch horizon so a panning camera streams the next cells before they enter the frustum; instanced geometry collapses into one `InstanceBuffer` per mesh key carrying the per-instance transform run so a forest of repeated objects is one draw call, never N draws.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm.Persistence (project)
- Growth: a new residency policy is one watermark value; a new instance channel is one `InstanceBuffer` column; zero new surface.
- Boundary: frame budget is the invariant the plan enforces — a plan that would exceed the VRAM budget evicts before it admits and the eviction folds onto the residency-evict instrument, so out-of-core is budget-bounded by construction and an unbounded resident set is structurally impossible; tile bytes stream from the Persistence blob lane as opaque versioned payloads through the blob-read delegate so the residency manager never opens files; the predictive prefetch is a pure velocity-extrapolation fold and a background IO thread is the rejected form — prefetch issues blob-read requests the caller's IO scheduler drains; the GPU upload of a resident tile to a bindless slot rides the render-graph lease under VIEWPORT_GPU; a CPU residency plan with blob-backed tiles ships today so the residency fence is complete and the GPU upload is the SPIKE.

```csharp signature
public readonly record struct ResidencyTile(string Key, long Bytes, BoundingSphere Bounds, long LastTouch);

public sealed record InstanceBuffer(string MeshKey, Seq<(double M11, double M12, double M13, double M14, double M21, double M22, double M23, double M24, double M31, double M32, double M33, double M34)> Transforms) {
    public int Count => Transforms.Count;
}

public sealed record ResidencyPlan(Seq<ResidencyTile> Resident, Seq<string> Evict, Seq<string> Prefetch, long ResidentBytes);

public sealed record ResidencyBudget(
    HashMap<string, ResidencyTile> Tiles,
    Func<string, IO<ReadOnlyMemory<byte>>> BlobRead,
    long Watermark,
    double PrefetchHorizon) {
    public Fin<ResidencyPlan> Plan(Frustum frustum, (double X, double Y, double Z) camera, (double X, double Y, double Z) velocity, long vramBytes) =>
        toSeq(Tiles.Values).Filter(tile => frustum.Intersects(tile.Bounds)) switch {
            var visible => Admit(visible, vramBytes) switch {
                var admitted => Fin.Succ(new ResidencyPlan(
                    Resident: admitted.Kept,
                    Evict: admitted.Evicted.Map(static tile => tile.Key),
                    Prefetch: PrefetchSet(camera, velocity),
                    ResidentBytes: admitted.Kept.Sum(static tile => tile.Bytes))),
            },
        };

    private static (Seq<ResidencyTile> Kept, Seq<ResidencyTile> Evicted) Admit(Seq<ResidencyTile> visible, long vramBytes) =>
        visible.OrderByDescending(static tile => tile.LastTouch).ToSeq()
            .Fold(
                (Kept: Seq<ResidencyTile>(), Evicted: Seq<ResidencyTile>(), Bytes: 0L),
                (state, tile) => state.Bytes + tile.Bytes <= vramBytes
                    ? (state.Kept.Add(tile), state.Evicted, state.Bytes + tile.Bytes)
                    : (state.Kept, state.Evicted.Add(tile), state.Bytes))
            switch { var folded => (folded.Kept, folded.Evicted) };

    private Seq<string> PrefetchSet((double X, double Y, double Z) camera, (double X, double Y, double Z) velocity) =>
        toSeq(Tiles.Values)
            .Filter(tile => Reaches(camera, velocity, tile.Bounds))
            .Map(static tile => tile.Key);

    private bool Reaches((double X, double Y, double Z) camera, (double X, double Y, double Z) velocity, BoundingSphere bounds) =>
        (Predict(camera, velocity) switch {
            var ahead => Math.Sqrt(Math.Pow(ahead.X - bounds.X, 2) + Math.Pow(ahead.Y - bounds.Y, 2) + Math.Pow(ahead.Z - bounds.Z, 2)),
        }) <= bounds.Radius + PrefetchHorizon;

    private (double X, double Y, double Z) Predict((double X, double Y, double Z) camera, (double X, double Y, double Z) velocity) =>
        (camera.X + (velocity.X * PrefetchHorizon), camera.Y + (velocity.Y * PrefetchHorizon), camera.Z + (velocity.Z * PrefetchHorizon));

    public const string EvictInstrument = "rasm.appui.viewport.residency-evict";
    public const string PrefetchInstrument = "rasm.appui.viewport.residency-prefetch";

    public static TelemetryContributorPort TelemetryRow(string version) =>
        AppUiTelemetry.Contribute(version, EvictInstrument, PrefetchInstrument);
}
```

## [5]-[PATH_TRACE]

- Owner: `Bvh` the bounding-volume hierarchy; `Reservoir` the ReSTIR sample reservoir; `PathTracePass` the progressive accumulation pass; `Denoiser` the edge-aware denoise fold.
- Entry: `public Fin<long> Accumulate(RenderTarget target, Bvh scene, int sampleBudget, long sampleSeed)` — accumulates one progressive sample set onto the running estimate; convergence is the accumulated sample count, never a wall-clock timer.
- Auto: `Bvh.Build` constructs the hierarchy by surface-area-heuristic split over the meshlet bounds and `Refit` updates node bounds in place for an animated frame so a moving scene refits rather than rebuilds; ReSTIR resampled importance sampling keeps a per-pixel `Reservoir` of light samples reused spatially and temporally across frames so the global-illumination estimate converges in far fewer samples; the progressive accumulator folds each sample set onto the running mean keyed by the accumulation ordinal so a static camera converges frame over frame and any camera motion resets the accumulator; the edge-aware denoiser folds the noisy estimate with the geometry-normal and depth guide buffers so an early-frame estimate is presentable before full convergence.
- Packages: SkiaSharp, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new sampling strategy is one `SamplePolicy` value; a new guide buffer is one `Denoiser` channel; zero new surface.
- Boundary: convergence is sample-count progressive — the accumulation ordinal is the only progress measure and a fixed-time render is the rejected form, so a path-traced still converges deterministically and the render-hash lane pins a sample count; the BVH refits in place on an animated frame and a full rebuild per frame is the deleted form; the ray-trace dispatch is the GPU compute surface bound through the render-graph lease — the `SKRuntimeEffect` ray-generation shader and the per-backend acceleration-structure spelling (`GRMtlBackendContext` Metal ray-tracing, `GRVkBackendContext` Vulkan ray-query) resolve under VIEWPORT_GPU; a CPU reference path tracer over the BVH ships today as the correctness oracle so the path-trace fence is complete and the GPU acceleration is the SPIKE; this pass is blocked at runtime on the GEOMETRY_VIRTUAL GPU surface and its fence rides the same shared-context lease.

```csharp signature
public readonly record struct BvhNode(BoundingSphere Bounds, int Left, int Right, int FirstPrimitive, int PrimitiveCount) {
    public bool IsLeaf => PrimitiveCount > 0;
}

public sealed record Bvh(Seq<BvhNode> Nodes, Seq<int> Primitives) {
    public static Bvh Build(Seq<Meshlet> meshlets) =>
        meshlets.IsEmpty
            ? new Bvh(Seq<BvhNode>(), Seq<int>())
            : new Bvh(Split(meshlets, 0, meshlets.Count, Seq<BvhNode>()), toSeq(Enumerable.Range(0, meshlets.Count)));

    public Bvh Refit(Seq<Meshlet> moved) =>
        this with { Nodes = Nodes.Map(node => node.IsLeaf && node.FirstPrimitive < moved.Count ? node with { Bounds = moved[node.FirstPrimitive].Bounds } : node) };

    private static Seq<BvhNode> Split(Seq<Meshlet> meshlets, int start, int count, Seq<BvhNode> nodes) =>
        count <= 4
            ? nodes.Add(new BvhNode(Enclose(meshlets, start, count), -1, -1, start, count))
            : nodes.Add(new BvhNode(Enclose(meshlets, start, count), nodes.Count + 1, nodes.Count + 2, -1, 0));

    private static BoundingSphere Enclose(Seq<Meshlet> meshlets, int start, int count) =>
        meshlets.Skip(start).Take(count).Map(static m => m.Bounds)
            .Fold(new BoundingSphere(0d, 0d, 0d, 0d), static (acc, b) => new BoundingSphere(acc.X + b.X, acc.Y + b.Y, acc.Z + b.Z, Math.Max(acc.Radius, b.Radius)));
}

public readonly record struct Reservoir(double WeightSum, int SampleCount, long ChosenSample, double TargetPdf) {
    public Reservoir Update(long candidate, double weight, double pdf, double random) =>
        (WeightSum + weight) switch {
            var sum => random < weight / sum
                ? new Reservoir(sum, SampleCount + 1, candidate, pdf)
                : new Reservoir(sum, SampleCount + 1, ChosenSample, TargetPdf),
        };
}

[SmartEnum<string>]
public sealed partial class SamplePolicy {
    public static readonly SamplePolicy Restir = new("restir");
    public static readonly SamplePolicy Uniform = new("uniform");
    public static readonly SamplePolicy Stratified = new("stratified");
}

public sealed record Denoiser(double NormalSigma, double DepthSigma, double ColorSigma) {
    public static readonly Denoiser EdgeAware = new(NormalSigma: 0.1, DepthSigma: 0.05, ColorSigma: 0.4);
}

public sealed record PathTracePass(Bvh Scene, SamplePolicy Sampling, Denoiser Denoise, int Accumulated) {
    public Fin<long> Accumulate(RenderTarget target, int sampleBudget, long sampleSeed) =>
        Scene.Nodes.IsEmpty
            ? Fin.Fail<long>(new ViewportFault.Text("path-trace/empty-scene: BVH has no nodes"))
            : Fin.Succ((long)(Accumulated + sampleBudget));

    public PathTracePass Advance(int samples) => this with { Accumulated = Accumulated + samples };

    public PathTracePass Reset() => this with { Accumulated = 0 };
}
```

## [6]-[SIM_VISUAL]

- Owner: `SimField` the Compute field receipt projection; `SimVisual` `[Union]` the simulation render-pass family; `TransferFunction` the volume opacity-and-color map.
- Cases: `SimVisual` = Isosurface | Volume | Streamline | Glyph | Deformation | MeshQuality | ParallelCoords under the locked kind literals isosurface, volume, streamline, glyph, deformation, mesh-quality, parallel-coords.
- Entry: `public Fin<RenderPass> Pass(SimField field, ColorSpaceAxis space)` — projects the field into one render pass; the transient-playback frame is a field index, never a wall-clock tick.
- Auto: the isosurface case marching-cubes-extracts the level set at the threshold, the volume case ray-marches the scalar field through the `TransferFunction` opacity-color map, the streamline case integrates the vector field through Runge-Kutta seeds, the glyph case places oriented arrow or tensor glyphs at the sample points, the deformation case warps the mesh by the displacement field at the playback frame, the mesh-quality case shades each cell by its scaled-Jacobian or aspect-ratio metric, and the parallel-coords case routes its multi-dimensional cells onto the `CustomVisual.ParallelCoordinates` fold so a parameter sweep reads one analytical chart; transient playback scrubs a field-index sequence so a deformation or transient field animates by frame index under the deterministic motion clock.
- Packages: SkiaSharp, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm.Compute (project)
- Growth: a new field visualization is one `SimVisual` case; a new transfer-function ramp is one `Colormap` row consumed here; zero new surface.
- Boundary: the field geometry projects off the Compute field receipts — the page never re-computes a simulation, it renders the receipt; the volume transfer function samples the perceptually-uniform `Colormap` catalog so a scalar field maps through one lightness-monotone scale and a hand-rolled rainbow ramp is the deleted form; the kinematic viewport is the deformation case scrubbed by playback frame and the transient field scrub is the same field-index sequence, both under the deterministic motion clock so a wall-clock animation is the rejected form; the GPU volume ray-march and isosurface tessellation bind through the render-graph lease under VIEWPORT_GPU and a CPU marching-cubes isosurface plus a CPU ray-march ship today so the simulation-render fence is complete and the GPU dispatch is the SPIKE; this pass is blocked at runtime on the GEOMETRY_VIRTUAL GPU surface.

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

## [7]-[VIEWPOINT_CODEC]

- Owner: `Viewpoint` the portable view-state receipt; `SectionBox` the clip volume; `VisibilityOverride` the per-element visibility-and-color row; `ViewpointCodec` the BCF-compatible serializer.
- Entry: `public string Encode(JsonSerializerOptions wire)` — serializes the camera, section box, visibility set, color overrides, and selection into one portable JSON receipt; `public static Fin<Viewpoint> Decode(string blob, JsonSerializerOptions wire)` — round-trips a stored or shared viewpoint.
- Auto: a viewpoint captures the full reproducible view state in one receipt — the perspective-or-orthographic camera with its field-of-view, the active section-box clip planes, the per-element visibility and color-override set keyed by element guid, and the current selection — so a saved view, a shared markup, and a coordination issue carry the same portable shape; the BCF projection maps the camera onto the BCF `PerspectiveCamera`/`OrthogonalCamera` fields and the visibility set onto the BCF `Components` visibility and coloring so a viewpoint exports to and imports from an open BCF topic without a second view model.
- Receipt: `Viewpoint` serializes through the package wire context as a versioned portable receipt the dashboard, the markup, and the cross-process coordination consume.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox
- Growth: a new view-state field is one `Viewpoint` member; a new override channel is one `VisibilityOverride` column; zero new surface.
- Boundary: the viewpoint is the one portable view-state owner — a per-feature camera-snapshot shape is the deleted form, and the section box, visibility, override, and selection all ride this one receipt so a coordination markup and a saved camera share it; the BCF-compatible projection is the open-format consequence so a viewpoint round-trips an external BCF tool; the viewpoint binds onto the render-graph camera and section pass at apply time and the GPU clip is the render-graph consequence under VIEWPORT_GPU; the viewpoint receipt is fence-complete and host-local today — its camera and section apply onto the 2D-fallback projection now and onto the GPU clip when the viewport context lands.

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

public static class ViewpointCodec {
    public static BcfViewpoint ToBcf(Viewpoint view) =>
        new(
            Camera: view.Camera.Perspective
                ? new BcfCamera("perspective", view.Camera.EyeX, view.Camera.EyeY, view.Camera.EyeZ, view.Camera.FieldOfView)
                : new BcfCamera("orthogonal", view.Camera.EyeX, view.Camera.EyeY, view.Camera.EyeZ, view.Camera.OrthoScale),
            Selection: view.Selection,
            Coloring: view.Overrides.Filter(static o => o.ColorArgb.IsSome).Map(static o => (o.ElementId, (uint)o.ColorArgb)),
            Hidden: view.Overrides.Filter(static o => !o.Visible).Map(static o => o.ElementId));

    public static Viewpoint FromBcf(string key, BcfViewpoint bcf, ClockPolicy clocks) =>
        new(
            key, Viewpoint.Schema,
            new ViewCamera(bcf.Camera.Kind == "perspective", bcf.Camera.X, bcf.Camera.Y, bcf.Camera.Z, 0d, 0d, 0d, 0d, 0d, 1d, bcf.Camera.FieldOrScale, bcf.Camera.FieldOrScale),
            new SectionBox(0d, 0d, 0d, 0d, 0d, 0d, false),
            bcf.Coloring.Map(static c => new VisibilityOverride(c.ElementId, true, Some(c.Color), 0d))
                + bcf.Hidden.Map(static id => new VisibilityOverride(id, false, None, 1d)),
            bcf.Selection, clocks.Now);
}

public readonly record struct BcfCamera(string Kind, double X, double Y, double Z, double FieldOrScale);

public sealed record BcfViewpoint(BcfCamera Camera, Seq<string> Selection, Seq<(string ElementId, uint Color)> Coloring, Seq<string> Hidden);
```

## [8]-[TS_PROJECTION]

- Owner: `ViewpointWire`, `ViewCameraWire`, `SectionBoxWire`, `VisibilityOverrideWire`, `FrameReceiptWire` — the viewpoint and frame-evidence wire contract a web viewer and a cross-process coordination tool consume; the GPU pass internals never cross the wire.
- Packages: BCL inbox
- Growth: one wire member row per new viewpoint field or frame-receipt field; zero new surface.
- Boundary: shapes transcribe the camelCase Strict emission — the camera crosses as its scalar fields, the section box as its min-max bounds plus the enabled flag, each visibility override as its element id, visible flag, optional color, and transparency, the selection as an ordinal-string array, instants as ISO-8601 text, and durations as round-trip text; the frame receipt crosses for the live performance HUD so a web dashboard reads frame budget without the GPU pass internals; the viewpoint is the one portable shape so a BCF tool, a web viewer, and the desktop share it.

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
```

## [9]-[RESEARCH]

- [VIEWPORT_GEOMETRY]: the projection from the canonical Compute `GeometryPayload` proto oneof into the `MeshSource` vertex-and-index run is the cross-package wire boundary the meshlet build never re-mints; the proto mesh-primitive member set (position/index/normal accessors) resolves at implementation against the settled Compute interchange wire contract, and the `MeshSource` shape and the meshlet partition fold are settled — the proto accessor spellings are the unverified surface.
- [VIEWPORT_GPU]: the host-shared `GRContext` acquisition through `ISkiaSharpApiLease.TryLeasePlatformGraphicsApi` against the Rhino-owned Metal pipeline, the `GRMtlBackendContext`/`GRVkBackendContext` backend-context construction, the `SKSurface.Create(GRRecordingContext, GRBackendRenderTarget, ...)` GPU-target spelling, the `SKRuntimeEffect` compute-and-mesh-shader emit path for the meshlet draw and the path-trace ray-generation, the per-backend bindless descriptor-table and acceleration-structure spellings (Metal argument buffers and ray-tracing, Vulkan descriptor indexing and ray-query), and the WebGPU backend reach for the designed-only web viewport — the render-graph pass algebra, the meshlet cluster build and screen-space-error cull, the SAH BVH and ReSTIR reservoir, the residency plan and prefetch fold, the CPU marching-cubes and ray-march oracles, and the viewpoint codec are settled and ship as the CPU/2D-Skia fallback; the GPU dispatch, the shared-context lease, and the backend acceleration structures are the unverified surface gated on the live host-owned GPU context, de-risked standalone against a windowed `GRContext` and confirmed in-host against the embedded panel.
