# [APPUI_RENDER_MESHLETS]

The geometry-virtualization and residency owners for the infinite viewport: the cluster plane CONSUMES Compute — `ResidencyPayload.Clusters` delivers the meshopt-built, cone-carrying `ResidencyMeshlet` descriptors with monotonic error columns, and this page owns only the SELECTION algebra (LOD cut with hysteresis, the raised cull ladder) plus the bindless residency the draw rides — local clustering is DEAD (`[V5]`); `ResidencyBudget` keeps an out-of-core scene inside a VRAM budget through predictive prefetch and massive instancing. The page owns the payload-cluster decode view, the cull ladder (frustum -> wire-cone backface -> hysteresis LOD cut -> prior-frame HZB two-phase occlusion), the bindless resource table, the VRAM-budget residency manager, the predictive prefetch fold, and the instance-buffer draw row; the render-graph pass-DAG that draws the cluster lives in `Render/pipeline`, the path-trace integrator that builds its private BVH over the wire-decoded cluster bounds in `Render/pathtrace`. The substrate is the Compute `Runtime/payload.md` `meshlet-cluster` row (a settled contract naming AppUi the demanding consumer), the Persistence blob lane for out-of-core tile streaming, and the shared wgpu device bound through the render-graph lease.

## [01]-[INDEX]

- [02]-[CLUSTER_CONSUMPTION]: Payload-cluster decode; the LOD selection algebra; the raised cull ladder.
- [03]-[RESIDENCY_BUDGET]: VRAM-budget residency, predictive prefetch, out-of-core streaming.

## [02]-[CLUSTER_CONSUMPTION]

- Owner: `MeshletKey` the payload-local cluster identity; `ResidencyMeshletView` the decode-only projection of one Compute `ResidencyMeshlet` descriptor; `MeshletCluster` the cluster scene over the consumed payload; `ClusterCull` the cull-ladder fold; `HzbPyramid` the prior-frame depth pyramid; `BindlessTable` the bindless resource table.
- Entry: `public static Fin<MeshletCluster> FromPayload(GpuBackend backend, ResidencyPayload payload, LodPolicy lod)` projects the payload's cluster rows and rejects a non-cluster payload kind; `public Fin<(MeshletCluster Cluster, CullResult Result)> Visible(Frustum frustum, ViewCamera camera, double lodScale, Option<HzbPyramid> hzb, double nearPlane)` executes the full ladder and returns the advanced immutable cull owner with its receipt.
- Auto: the clusters arrive Compute-built — meshopt clustering, REAL per-cluster bounds, REAL cone apex/axis/cutoff, and encoded `Error`/`ParentError` columns that are monotonic BY CONSTRUCTION (`ParentError >= Error` on the `payload.md` row — the landed encode guarantee), so cut well-formedness (crack-free, no double-draw) rides the producer guarantee and this page re-verifies nothing; the LOD SELECTION ALGEBRA is AppUi's own: the per-cluster error bound projects to screen space under the camera row, the `LodPolicy` pixel threshold picks the cut (`Projected(Error) <= threshold < Projected(ParentError)` — exactly one cluster per subtree by monotonicity), and the hysteresis band on the same policy row keeps a prior-cut cluster selected until its error crosses the threshold by the band so a dolly move never flickers the cut; the cull ladder is RAISED past cone parity per the page's infinite-viewport charter: frustum -> wire-cone backface (a cluster whose cone faces away from the eye rejects; a cutoff of -1 never rejects, so degenerate cones stay drawable) -> LOD cut -> prior-frame depth-pyramid (HZB) two-phase occlusion — draw the prior-visible set first, test the remainder against the pyramid, and a cluster fully occluded by the prior frame draws nothing; bindless resource indices resolve through `BindlessTable` so a draw names a resource by index, never a per-draw bind.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm.Compute (project), Silk.NET.WebGPU
- Growth: a new LOD policy is one `LodPolicy` value; a new vertex-stream channel is one `BindlessTable` slot; a new cull phase is one ladder row; zero new surface.
- Boundary: cluster geometry is DECODE-ONLY consumption of the Compute `ResidencyPayload` — local `Build`/`Partition`, a `MeshSource` re-projection, a re-tessellation, or a `meshoptimizer` AppUi admission is the DELETED form (Compute owns clustering; the recorded roster REJECT stands); tiles and clusters key by the payload's own `ContentKey` per the single-mint law; the HZB pyramid build is ONE compute pass over the shared device (`CommandEncoderBeginComputePass`/`ComputePassEncoder`/`DispatchWorkgroups` — a mip-chain farthest-depth reduction), and `QueryType.Occlusion` is the fallback probe row where the pyramid lane is unavailable; the mesh-shader draw path is the GPU surface bound through the `Render/pipeline` render-graph lease, and the GPU-driven multi-draw rides the WGPU vendor rows — `RenderPassEncoderMultiDrawIndexedIndirectCount` lets the cull pass write the draw count GPU-side so no CPU readback sits between cull and draw, with per-draw constants through `RenderPassEncoderSetPushConstants`, submission retiring through the pipeline `WgpuFrameEvidence` lane (`QueueSubmitForIndex` minting the `WrappedSubmissionIndex` that `DevicePoll` advances without a blocking fence) and per-pass GPU timing riding the same lane's timestamp queries, so no meshlet-local fence, timer, or evidence owner exists — under the VIEWPORT_GPU spike; the `Render/pipeline` `Taa` motion-vector buffer is one `BindlessTable` slot here, never a parallel motion-vector owner; this crossing is a declared `[V9]` ledger row (`Render/meshlets` <- Compute `Runtime/payload.md` clusters).

```csharp signature
public readonly record struct BoundingSphere(double X, double Y, double Z, double Radius) {
    public double SurfaceArea() => 4d * Math.PI * Radius * Radius;
}

public readonly record struct NormalCone(double X, double Y, double Z, double CosCutoff);

public readonly record struct MeshletKey(UInt128 Payload, int Level, int VertexOffset, int TriangleOffset);

// Decode-only view of one Compute ResidencyMeshlet descriptor — every column reads from the wire,
// nothing recomputes; ParentError >= Error holds by the producer's encode guarantee.
public readonly record struct ResidencyMeshletView(
    int VertexOffset,
    int VertexCount,
    int TriangleOffset,
    int TriangleCount,
    BoundingSphere Bounds,
    NormalCone Cone,
    double Error,
    double ParentError,
    int Level,
    int Parent,
    MeshletKey Key);

public sealed record LodPolicy(double PixelThreshold, double HysteresisBand, int MaxLevels) {
    public static readonly LodPolicy Default = new(PixelThreshold: 1.0, HysteresisBand: 0.25, MaxLevels: 8);
}

public readonly record struct Frustum(Seq<(double A, double B, double C, double D)> Planes) {
    public bool Intersects(BoundingSphere sphere) =>
        Planes.ForAll(plane => (plane.A * sphere.X) + (plane.B * sphere.Y) + (plane.C * sphere.Z) + plane.D >= -sphere.Radius);
}

public sealed record BindlessTable(FrozenDictionary<string, int> Slots) {
    public static BindlessTable Of(params ReadOnlySpan<string> channels) =>
        new(channels.ToArray().Select(static (channel, index) => KeyValuePair.Create(channel, index)).ToFrozenDictionary(StringComparer.Ordinal));

    public Option<int> Slot(string channel) => Slots.TryGetValue(channel, out int index) ? Some(index) : None;
}

// Prior-frame depth pyramid: mip 0 is last frame's depth, each mip the FARTHEST-depth (max) reduction of
// the level below — occlusion is conservative only against the footprint's farthest occluder, a min
// reduction over-culls; built by ONE compute pass on the shared device; Occluded samples the mip whose
// texel covers the cluster's screen extent so one sample bounds the whole footprint.
public sealed record HzbPyramid(int Width, int Height, int MipLevels, Func<int, double, double, double> SampleFarDepth) {
    public bool Occluded(BoundingSphere bounds, ViewCamera camera, double nearPlane) {
        (double sx, double sy, double radiusPx, double depth) = ScreenExtent(bounds, camera, nearPlane);
        if (depth <= nearPlane) { return false; } // camera inside or crossing the sphere: never occluded
        int mip = Math.Clamp((int)Math.Ceiling(Math.Log2(Math.Max(radiusPx * 2d, 1d))), 0, MipLevels - 1);
        return depth > SampleFarDepth(mip, sx, sy); // sphere's nearest point behind the footprint's farthest occluder: fully hidden
    }

    // Camera-row projection kernel: view-basis transform of the sphere center, CONSERVATIVE depth (the
    // sphere's closest point, tested against the farthest-depth pyramid) and screen radius; the ortho arm
    // scales by OrthoScale px-per-unit directly, the perspective arm by the vertical field of view.
    (double X, double Y, double RadiusPx, double Depth) ScreenExtent(BoundingSphere bounds, ViewCamera camera, double nearPlane) {
        CameraFrame frame = camera.Frame;
        (double fx, double fy, double fz) = Normalize(frame.Target.X - frame.Eye.X, frame.Target.Y - frame.Eye.Y, frame.Target.Z - frame.Eye.Z);
        (double rx, double ry, double rz) = Normalize(Cross(fx, fy, fz, frame.Up.X, frame.Up.Y, frame.Up.Z));
        (double ux, double uy, double uz) = Cross(rx, ry, rz, fx, fy, fz);
        (double cx, double cy, double cz) = (bounds.X - frame.Eye.X, bounds.Y - frame.Eye.Y, bounds.Z - frame.Eye.Z);
        double z = (cx * fx) + (cy * fy) + (cz * fz);
        double x = (cx * rx) + (cy * ry) + (cz * rz);
        double y = (cx * ux) + (cy * uy) + (cz * uz);
        double depth = z - bounds.Radius;
        return camera.Switch(
            state: (Owner: this, X: x, Y: y, Z: z, Depth: depth, Radius: bounds.Radius, Near: nearPlane),
            perspective: static (state, lens) => {
                double half = Math.Tan(double.DegreesToRadians(lens.FieldOfViewDeg) / 2d);
                double safeZ = Math.Max(state.Z, state.Near);
                double aspect = state.Owner.Width / (double)state.Owner.Height;
                return (
                    (((state.X / (safeZ * half * aspect)) * 0.5) + 0.5) * state.Owner.Width,
                    (0.5 - ((state.Y / (safeZ * half)) * 0.5)) * state.Owner.Height,
                    (state.Radius / safeZ) * (state.Owner.Height / (2d * half)),
                    state.Depth);
            },
            orthographic: static (state, lens) => {
                double pxPerUnit = state.Owner.Height / Math.Max(lens.ViewHeight, 1e-6);
                return (
                    (state.X * pxPerUnit) + (state.Owner.Width / 2d),
                    (state.Owner.Height / 2d) - (state.Y * pxPerUnit),
                    state.Radius * pxPerUnit,
                    state.Depth);
            });
    }

    private static (double X, double Y, double Z) Normalize(double x, double y, double z) {
        double length = Math.Max(Math.Sqrt((x * x) + (y * y) + (z * z)), 1e-12);
        return (x / length, y / length, z / length);
    }

    private static (double X, double Y, double Z) Normalize((double X, double Y, double Z) v) => Normalize(v.X, v.Y, v.Z);

    private static (double X, double Y, double Z) Cross(double ax, double ay, double az, double bx, double by, double bz) =>
        ((ay * bz) - (az * by), (az * bx) - (ax * bz), (ax * by) - (ay * bx));
}

public sealed record CullState(LanguageExt.HashSet<MeshletKey> PriorCut, LanguageExt.HashSet<MeshletKey> PriorVisible);

public sealed record CullResult(Seq<ResidencyMeshletView> Draw, Seq<ResidencyMeshletView> OcclusionRetest, CullState Next);

public static class ClusterCull {
    // The raised ladder: frustum -> wire-cone backface -> hysteresis LOD cut -> two-phase HZB occlusion.
    public static CullResult Cull(
        Seq<ResidencyMeshletView> clusters,
        Frustum frustum,
        ViewCamera camera,
        double lodScale,
        LodPolicy lod,
        CullState prior,
        Option<HzbPyramid> hzb,
        double nearPlane) {
        Seq<ResidencyMeshletView> inFrustum = clusters.Filter(cluster => frustum.Intersects(cluster.Bounds));
        Seq<ResidencyMeshletView> facing = inFrustum.Filter(cluster => cluster.Level < lod.MaxLevels && !BackfaceReject(cluster, camera));
        Seq<ResidencyMeshletView> cut = facing.Filter(cluster => InCut(cluster, camera, lodScale, lod, prior.PriorCut));
        (Seq<ResidencyMeshletView> phase1, Seq<ResidencyMeshletView> retest) =
            hzb.Match(
                Some: pyramid => (
                    cut.Filter(cluster => prior.PriorVisible.Contains(cluster.Key)),
                    cut.Filter(cluster => !prior.PriorVisible.Contains(cluster.Key) && !pyramid.Occluded(cluster.Bounds, camera, nearPlane))),
                None: () => (cut, Seq<ResidencyMeshletView>()));
        return new CullResult(
            phase1 + retest,
            retest,
            new CullState(
                toHashSet(cut.Map(static c => c.Key)),
                toHashSet((phase1 + retest).Map(static c => c.Key))));
    }

    // Wire-cone backface: reject when every triangle in the cluster faces away — the meshopt cone test
    // dot(axis, normalize(center - eye)) >= cutoff; a cutoff of -1 never rejects.
    public static bool BackfaceReject(ResidencyMeshletView cluster, ViewCamera camera) {
        CameraFrame frame = camera.Frame;
        (double dx, double dy, double dz) = (cluster.Bounds.X - frame.Eye.X, cluster.Bounds.Y - frame.Eye.Y, cluster.Bounds.Z - frame.Eye.Z);
        double length = Math.Sqrt((dx * dx) + (dy * dy) + (dz * dz));
        if (length <= cluster.Bounds.Radius) { return false; }
        double dot = ((cluster.Cone.X * dx) + (cluster.Cone.Y * dy) + (cluster.Cone.Z * dz)) / length;
        return dot >= cluster.Cone.CosCutoff && cluster.Cone.CosCutoff > -1d;
    }

    // Hysteresis LOD cut: select where Projected(Error) <= threshold < Projected(ParentError) — exactly
    // one cluster per subtree by the monotonic columns; a prior-cut member holds until its error crosses
    // the threshold by the band, so a dolly move never flickers the cut.
    public static bool InCut(ResidencyMeshletView cluster, ViewCamera camera, double lodScale, LodPolicy lod, LanguageExt.HashSet<MeshletKey> priorCut) {
        double projectedError = Projected(cluster.Error, cluster.Bounds, camera) * lodScale;
        double projectedParent = Projected(cluster.ParentError, cluster.Bounds, camera) * lodScale;
        double threshold = lod.PixelThreshold;
        double band = priorCut.Contains(cluster.Key) ? threshold * lod.HysteresisBand : 0d;
        return projectedError <= threshold + band && projectedParent > threshold - band;
    }

    private static double Projected(double error, BoundingSphere bounds, ViewCamera camera) {
        CameraFrame frame = camera.Frame;
        (double dx, double dy, double dz) = (bounds.X - frame.Eye.X, bounds.Y - frame.Eye.Y, bounds.Z - frame.Eye.Z);
        double distance = Math.Max(Math.Sqrt((dx * dx) + (dy * dy) + (dz * dz)) - bounds.Radius, 1e-6);
        return error / distance; // pinhole small-angle projection; the viewport scale folds through lodScale
    }
}

public sealed record MeshletCluster(
    GpuBackend Backend,
    Seq<ResidencyMeshletView> Clusters,
    LodPolicy Lod,
    BindlessTable Bindless,
    long Triangles,
    CullState State) {
    public static Fin<MeshletCluster> FromPayload(GpuBackend backend, ResidencyPayload payload, LodPolicy lod) =>
        payload.Kind == ResidencyKind.MeshletCluster
            ? Fin.Succ(new MeshletCluster(
                backend,
                payload.Clusters.Map(static row => new ResidencyMeshletView(
                    row.VertexOffset, row.VertexCount, row.TriangleOffset, row.TriangleCount,
                    new BoundingSphere(row.Center.X, row.Center.Y, row.Center.Z, row.Radius),
                    new NormalCone(row.ConeAxis.X, row.ConeAxis.Y, row.ConeAxis.Z, row.ConeCutoff),
                    row.Error, row.ParentError, row.Level, row.Parent,
                    new MeshletKey(payload.ContentKey, row.Level, row.VertexOffset, row.TriangleOffset))),
                lod,
                BindlessTable.Of("position", "normal", "uv", "color", "motion-vector"),
                payload.Clusters.Sum(static row => (long)row.TriangleCount),
                new CullState([], [])))
            : Fin.Fail<MeshletCluster>(new ViewportFault.Text($"meshlets/payload-kind: {payload.Kind} is not meshlet-cluster"));

    public Fin<(MeshletCluster Cluster, CullResult Result)> Visible(Frustum frustum, ViewCamera camera, double lodScale, Option<HzbPyramid> hzb, double nearPlane) =>
        ClusterCull.Cull(Clusters, frustum, camera, lodScale, Lod, State, hzb, nearPlane) switch {
            CullResult result => Fin.Succ((this with { State = result.Next }, result)),
        };
}
```

## [03]-[RESIDENCY_BUDGET]

- Owner: `ResidencyTile` the streamable geometry page; `ResidencyBudget` the VRAM-budget residency manager; `Prefetch` the predictive prefetch fold; `InstanceBuffer` the massive-instancing draw row.
- Entry: `public Fin<ResidencyPlan> Plan(Frustum frustum, (double X, double Y, double Z) camera, (double X, double Y, double Z) velocity, long vramBytes, long frame, ResidencyPlan prior)` — one state transition per frame: the prior plan IS the resident-set state, and the next plan accounts for every resident, visible, evicted, and prefetched tile in one fold; the resident-plus-prefetch byte total never exceeds `vramBytes`.
- Auto: residency keys each tile by the payload's own `ContentKey` and tracks its byte cost and last-touch frame; the transition touches every frustum-visible tile at `frame`, carries the prior plan's out-of-frustum residents forward at their old touch, admits the union in touch-recency order under the byte budget (visible tiles admit first by construction because their touch is current), and EVICTS every tile that was resident in the prior plan and is not resident in the next — a tile that left the frustum either survives as a carried resident or lands in `Evict`, so no resident tile can persist outside the reported residency state; prefetch admits the velocity-reachable non-resident tiles greedily into the remaining byte headroom only, its bytes carried on `PrefetchBytes`, so the budget governs resident and prefetch admissions from one derivable total; instanced geometry collapses into one `InstanceBuffer` per mesh key carrying the per-instance transform run so a forest of repeated objects is one draw call, never N draws.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm.Persistence (project)
- Growth: a new residency policy is one watermark value; a new instance channel is one `InstanceBuffer` column; zero new surface.
- Boundary: frame budget is the invariant the plan enforces — a plan that overruns the VRAM budget evicts before it admits and the eviction folds onto the residency-evict instrument, so out-of-core is budget-bounded by construction and an unbounded resident set is structurally impossible; tile bytes stream from the Persistence blob lane as opaque versioned payloads through the blob-read delegate so the residency manager never opens files; the predictive prefetch is a pure velocity-extrapolation fold and a background IO thread is the rejected form — prefetch issues blob-read requests the caller's IO scheduler drains; the GPU upload of a resident tile to a bindless slot rides the `Render/pipeline` render-graph lease under VIEWPORT_GPU; the residency manifest the web leg consumes projects through the `Render/pipeline` `ResidencyManifest.Mint` off the resident set, so the residency owner mints no second wire; the watermark scales by the `Diagnostics/governor.md` `QualityVerdict.WatermarkFactor` — one quality authority.

```csharp signature
public readonly record struct ResidencyTile(UInt128 ContentKey, long Bytes, BoundingSphere Bounds, long LastTouch);

public sealed record InstanceBuffer(string MeshKey, Seq<(double M11, double M12, double M13, double M14, double M21, double M22, double M23, double M24, double M31, double M32, double M33, double M34)> Transforms) {
    public int Count => Transforms.Count;
}

// The plan IS the cross-frame residency state: Boot seeds it, every frame folds it forward, and the
// resident/evict/prefetch sets plus both byte totals are recoverable from the value alone.
public sealed record PrefetchRequest(UInt128 ContentKey, long Bytes, IO<ReadOnlyMemory<byte>> Read);

public sealed record ResidencyPlan(Seq<ResidencyTile> Resident, Seq<UInt128> Evict, Seq<PrefetchRequest> Prefetch, long ResidentBytes, long PrefetchBytes, long Frame) {
    public static readonly ResidencyPlan Boot = new(Seq<ResidencyTile>(), Seq<UInt128>(), Seq<PrefetchRequest>(), 0L, 0L, 0L);
}

public sealed record ResidencyBudget(
    HashMap<UInt128, ResidencyTile> Tiles,
    Func<UInt128, IO<ReadOnlyMemory<byte>>> BlobRead,
    long Watermark,
    double PrefetchHorizon) {
    // The effective budget is min(vramBytes, Watermark): the governor-scaled watermark is a CONSUMED bound
    // on every admission and prefetch headroom, never a decorative field beside the plan.
    public Fin<ResidencyPlan> Plan(Frustum frustum, (double X, double Y, double Z) camera, (double X, double Y, double Z) velocity, long vramBytes, long frame, ResidencyPlan prior) =>
        from budget in FinSucc(Math.Min(vramBytes, Watermark))
        let candidates = Candidates(frustum, frame, prior)
        let admitted = Admit(candidates, budget)
        let kept = toHashSet(admitted.Kept.Map(static tile => tile.ContentKey))
        let prefetch = PrefetchSet(camera, velocity, kept, budget - admitted.Bytes)
        select new ResidencyPlan(
            Resident: admitted.Kept,
            Evict: prior.Resident.Map(static tile => tile.ContentKey).Filter(key => !kept.Contains(key)),
            Prefetch: prefetch.Requests,
            ResidentBytes: admitted.Bytes,
            PrefetchBytes: prefetch.Bytes,
            Frame: frame);

    // Candidate set = visible tiles touched NOW + prior residents carried at their old touch; one union,
    // deduped by content key with the fresh touch winning.
    private Seq<ResidencyTile> Candidates(Frustum frustum, long frame, ResidencyPlan prior) =>
        toSeq(toSeq(Tiles.Values)
            .Filter(tile => frustum.Intersects(tile.Bounds))
            .Map(tile => tile with { LastTouch = frame })
            .Concat(prior.Resident)
            .Fold(HashMap<UInt128, ResidencyTile>(), static (held, tile) => held.Find(tile.ContentKey).IsSome ? held : held.Add(tile.ContentKey, tile))
            .Values);

    private static (Seq<ResidencyTile> Kept, long Bytes) Admit(Seq<ResidencyTile> candidates, long vramBytes) =>
        candidates.OrderByDescending(static tile => tile.LastTouch).ToSeq()
            .Fold(
                (Kept: Seq<ResidencyTile>(), Bytes: 0L),
                (state, tile) => state.Bytes + tile.Bytes <= vramBytes
                    ? (state.Kept.Add(tile), state.Bytes + tile.Bytes)
                    : state);

    // Prefetch is budget-bounded: velocity-reachable non-resident tiles admit greedily into the byte
    // headroom the resident admission left; an unbudgeted prefetch cannot type its way onto the plan.
    private (Seq<PrefetchRequest> Requests, long Bytes) PrefetchSet(
        (double X, double Y, double Z) camera,
        (double X, double Y, double Z) velocity,
        LanguageExt.HashSet<UInt128> resident,
        long headroom) =>
        toSeq(Tiles.Values)
            .Filter(tile => !resident.Contains(tile.ContentKey) && Reaches(camera, velocity, tile.Bounds))
            .Fold(
                (Requests: Seq<PrefetchRequest>(), Bytes: 0L),
                (state, tile) => state.Bytes + tile.Bytes <= headroom
                    ? (state.Requests.Add(new PrefetchRequest(tile.ContentKey, tile.Bytes, BlobRead(tile.ContentKey))), state.Bytes + tile.Bytes)
                    : state);

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
    accTitle: Meshlet residency and culling flow
    accDescr: Compute residency payloads project into meshlet clusters, cull state, draw cuts, and residency plans.
    Payload["Compute ResidencyPayload.Clusters"] -->|FromPayload decode| MeshletCluster
    MeshletCluster --> ClusterCull
    ClusterCull -->|frustum, cone, LOD cut, HZB| CullResult
    HzbPyramid --> ClusterCull
    MeshletCluster --> BindlessTable
    ResidencyBudget -->|Plan| ResidencyPlan
    ResidencyPlan --> InstanceBuffer
    ResidencyBudget --> BlobRead
    classDef primary fill:#44475A,stroke:#FF79C6,color:#F8F8F2
    classDef data fill:#FFB86CBF,stroke:#FFB86C,color:#282A36
    classDef boundary fill:#282A36,stroke:#BD93F9,color:#F8F8F2
    class MeshletCluster,ClusterCull,ResidencyBudget primary
    class Payload,HzbPyramid,BindlessTable,InstanceBuffer,BlobRead data
    class CullResult,ResidencyPlan boundary
```

## [04]-[GPU_BOUNDARY]

- [VIEWPORT_GPU]: the shared-device lease owns bindless upload, the HZB farthest-depth reduction, `RenderPassEncoderMultiDrawIndexedIndirectCount`, and `RenderPassEncoderSetPushConstants`. Submission and timing compose the pipeline `WgpuFrameEvidence` lane, so meshlet selection owns no fence, timer, query set, or device lifetime.
- [PAYLOAD_COLUMNS]: `ResidencyMeshlet` supplies `VertexOffset`, `TriangleOffset`, `VertexCount`, `TriangleCount`, `Center`, `Radius`, `ConeAxis`, `ConeCutoff`, `Level`, `Parent`, `Error`, and `ParentError`. `MeshletKey` composes `ResidencyPayload.ContentKey` with level and stream offsets, and hierarchy, hysteresis, residency, and wire projection consume that producer identity unchanged.
