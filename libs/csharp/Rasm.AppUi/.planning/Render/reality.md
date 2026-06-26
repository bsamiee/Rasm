# [APPUI_REALITY_CAPTURE]

The reality-capture rail projects scanned existing-conditions geometry into the viewport beside BIM: `SplatSource` carries a Gaussian-splat ellipsoid set decoded off a Compute splat payload, `PointCloudSource` carries a massive point set decoded off a Compute point payload, `CapturePass` is the new `RenderPass` case family the render graph executes through the backend target factory, `MeasureOverlay` anchors a LiDAR-measurable annotation onto the `Viewpoint`, and `CaptureClip` scrubs a time-based capture frame on the animation playhead. The page owns the splat and point sources, the splat rasterization pass and point pass, the measurable overlay, and the capture-frame clip; the substrate is the `T-BACKEND-PORT` `GpuBackend` `RenderTarget` factory, the Compute point/splat payload at the interchange wire, the `Viewpoint` codec, and the animation `Track`/`Scrub` playhead. AppUi consumes the decoded point and splat payload at the wire and never decodes LAZ — the offline scan decode is the Python companion's geometry producer crossing as a Compute payload. The 3DGS rasterization is a distinct render path from triangle meshlets, SPIKE-gated on the live host-shared GPU context.

## [01]-[INDEX]

- [01]-[SPLAT_SOURCE]: SOG/PLY ellipsoid set off the Compute splat payload; radix-sort residency.
- [02]-[POINT_SOURCE]: LAZ-decoded point set off the Compute point payload; octree residency.
- [03]-[CAPTURE_PASS]: Splat and point `RenderPass` cases over the backend target factory.
- [04]-[MEASURE_OVERLAY]: LiDAR-anchored measurable annotation bound to the `Viewpoint`.
- [05]-[CAPTURE_CLIP]: Time-based capture-frame playback on the animation playhead.

## [02]-[SPLAT_SOURCE]

- Owner: `SplatEllipsoid` the single anisotropic 3D-Gaussian; `SplatSource` the decoded ellipsoid set; `SplatSort` the view-dependent radix-sort fold; `CaptureFault` the fault family in the 4900 band.
- Cases: `CaptureFault` = Text | PayloadMalformed | SortOverflow | BackendUnsupported | DecodeDeferred in the 4900 code band.
- Entry: `public static Fin<SplatSource> Decode(GpuBackend backend, SplatPayload payload, ResidencyBudget budget)` — projects the canonical Compute splat payload into the residency-keyed ellipsoid set; the page never decodes SOG/PLY or LAZ, it admits the decoded payload at the interchange wire.
- Auto: each ellipsoid carries its mean position, the three scale magnitudes, the rotation quaternion, the spherical-harmonic color coefficients, and the opacity, so a `SplatSource` is the decoded SOG (self-organizing-gaussian) or PLY ellipsoid set the Compute payload streams; `SplatSort` radix-sorts the ellipsoids back-to-front per view by their projected depth so the alpha-composited rasterization composites in order — the 3DGS draw demands depth-sorted ellipsoids and the radix sort is the per-view fold the pass re-runs on a camera change; the ellipsoid bytes stream from the Persistence blob lane through the residency budget exactly as the meshlet tiles do, so a massive splat scene stays VRAM-bounded; each ellipsoid's content key folds its mean, scale, and rotation through `XxHash128` so residency keys the splat tile identically to the meshlet tile.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, System.IO.Hashing, Rasm.Compute (project)
- Growth: a new splat attribute is one `SplatEllipsoid` field; a new sort policy is one `SplatSort` value; a new fault is one `CaptureFault` case; zero new surface.
- Boundary: the splat source projects off the canonical Compute splat payload through the `SplatPayload` boundary record — the page never decodes SOG/PLY/LAZ and never re-models a splat, the proto-to-`SplatSource` projection is the cross-package wire boundary resolved under CAPTURE_PAYLOAD, and the ellipsoid set consumes the projected payload so the no-re-decode law holds exactly as the meshlet build never re-tessellates; the radix sort is the one view-dependent depth order and a painter's-algorithm per-ellipsoid sort is the deleted form; the residency keying rides the `RESIDENCY_BUDGET` owner so the splat tile and the meshlet tile share one residency manager and a second splat-residency owner is the rejected form; the GPU ellipsoid rasterization binds the `T-BACKEND-PORT` `RenderTarget` factory through the render-graph lease — the per-backend splat-rasterizer compute kernel resolves under CAPTURE_GPU and a CPU ellipsoid-projection oracle is the floor for the 2D-fallback point preview while the GPU dispatch is the SPIKE.

```csharp signature
[Union]
public abstract partial record CaptureFault : Expected, IValidationError<CaptureFault> {
    private CaptureFault(string detail, int code) : base(detail, code, None) { }

    public static CaptureFault Create(string message) => new Text(message);

    public sealed record Text : CaptureFault { public Text(string detail) : base(detail, 4900) { } }
    public sealed record PayloadMalformed : CaptureFault { public PayloadMalformed(string detail) : base(detail, 4901) { } }
    public sealed record SortOverflow : CaptureFault { public SortOverflow(string detail) : base(detail, 4902) { } }
    public sealed record BackendUnsupported : CaptureFault { public BackendUnsupported(string detail) : base(detail, 4903) { } }
    public sealed record DecodeDeferred : CaptureFault { public DecodeDeferred(string detail) : base(detail, 4904) { } }
}

public readonly record struct SplatEllipsoid(
    float MeanX, float MeanY, float MeanZ,
    float ScaleX, float ScaleY, float ScaleZ,
    float RotX, float RotY, float RotZ, float RotW,
    float Opacity,
    int HarmonicOffset) {
    public BoundingSphere Bounds =>
        new(MeanX, MeanY, MeanZ, MathF.Max(ScaleX, MathF.Max(ScaleY, ScaleZ)) * 3f);

    public UInt128 ContentKey =>
        System.Buffers.Binary.BinaryPrimitives.ReadUInt128LittleEndian(
            System.IO.Hashing.XxHash128.Hash(MemoryMarshal.AsBytes(
                (ReadOnlySpan<float>)stackalloc float[] { MeanX, MeanY, MeanZ, ScaleX, ScaleY, ScaleZ, RotX, RotY, RotZ, RotW })));
}

public sealed record SplatPayload(string Key, int Count, ReadOnlyMemory<byte> Ellipsoids, ReadOnlyMemory<float> Harmonics, int HarmonicDegree, BoundingSphere Bounds);

[SmartEnum<string>]
public sealed partial class SplatSort {
    public static readonly SplatSort RadixDepth = new("radix-depth");
    public static readonly SplatSort RadixTile = new("radix-tile");
}

public sealed record SplatSource(
    GpuBackend Backend,
    Seq<SplatEllipsoid> Ellipsoids,
    ReadOnlyMemory<float> Harmonics,
    SplatSort Sort,
    int HarmonicDegree,
    BoundingSphere Bounds) {
    public static Fin<SplatSource> Decode(GpuBackend backend, SplatPayload payload, ResidencyBudget budget) =>
        payload.Count <= 0
            ? Fin.Fail<SplatSource>(new CaptureFault.PayloadMalformed($"splat/empty:{payload.Key}"))
            : Fin.Succ(new SplatSource(backend, Project(payload), payload.Harmonics, SplatSort.RadixDepth, payload.HarmonicDegree, payload.Bounds));

    public Seq<SplatEllipsoid> Sorted((double X, double Y, double Z) eye) =>
        Ellipsoids
            .OrderByDescending(splat => Depth(splat, eye))
            .ToSeq();

    private static double Depth(SplatEllipsoid splat, (double X, double Y, double Z) eye) =>
        Math.Pow(splat.MeanX - eye.X, 2) + Math.Pow(splat.MeanY - eye.Y, 2) + Math.Pow(splat.MeanZ - eye.Z, 2);

    private static Seq<SplatEllipsoid> Project(SplatPayload payload) =>
        toSeq(MemoryMarshal.Cast<byte, SplatEllipsoid>(payload.Ellipsoids.Span).ToArray());
}
```

```mermaid
flowchart LR
    SplatPayload -->|Decode| SplatSource
    SplatSource -->|Sorted| SplatSort
    SplatSource --> CapturePass
    CapturePass --> RenderTarget
    RenderTarget --> FrameReceipt
```

## [03]-[POINT_SOURCE]

- Owner: `PointSample` the single LiDAR return; `PointCloudSource` the decoded point set; `PointOctree` the level-of-detail residency tree.
- Entry: `public static Fin<PointCloudSource> Decode(GpuBackend backend, PointPayload payload, ResidencyBudget budget)` — projects the canonical Compute point payload into the octree-keyed point set; AppUi consumes the decoded payload at the wire and never decodes LAZ.
- Auto: each point carries its position, the classification byte, the intensity, and the RGB color so a `PointCloudSource` is the decoded scan return set the Compute payload streams; `PointOctree` partitions the points into a spatial octree whose nodes carry their level-of-detail subsample so a massive cloud renders the coarse subsample at distance and the full density up close, pop-free because adjacent levels share locked node boundaries exactly as the meshlet cluster-LOD shares cluster boundaries; the octree nodes key into the residency budget by their cell so a billion-point cloud stays VRAM-bounded; the classification byte routes through the perceptually-uniform colormap so a class-colored cloud maps through one lightness-monotone scale.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm.Compute (project)
- Growth: a new point attribute is one `PointSample` field; a new LOD policy is one octree subsample value; zero new surface.
- Boundary: the point source projects off the canonical Compute point payload through the `PointPayload` boundary record — the offline LAZ/scan decode is the Python companion's geometry producer crossing as a Compute payload, so AppUi carries no LAZ-decode package and a `laszip`/`pdal` admission inside `realitycapture/` is the rejected form; the octree LOD is the one massive-cloud residency law and a flat point-array draw is the deleted form; the octree residency rides the `RESIDENCY_BUDGET` owner so the point node and the meshlet tile share one residency manager; the GPU point splatting binds the `RenderTarget` factory through the render-graph lease under CAPTURE_GPU and a CPU octree subsample is the floor for the 2D fallback while the GPU draw is the SPIKE.

```csharp signature
public readonly record struct PointSample(
    float X, float Y, float Z,
    byte Classification,
    ushort Intensity,
    byte R, byte G, byte B) {
    public (double X, double Y, double Z) Position => (X, Y, Z);
}

public sealed record PointPayload(string Key, long Count, ReadOnlyMemory<byte> Points, int LevelDepth, BoundingSphere Bounds);

public sealed record PointOctreeNode(string Cell, int Level, BoundingSphere Bounds, int SampleStride, long Count, long LastTouch);

public sealed record PointCloudSource(
    GpuBackend Backend,
    Seq<PointSample> Points,
    Seq<PointOctreeNode> Octree,
    Colormap ClassRamp,
    BoundingSphere Bounds) {
    public static Fin<PointCloudSource> Decode(GpuBackend backend, PointPayload payload, ResidencyBudget budget) =>
        payload.Count <= 0L
            ? Fin.Fail<PointCloudSource>(new CaptureFault.PayloadMalformed($"point/empty:{payload.Key}"))
            : Fin.Succ(new PointCloudSource(backend, Project(payload), Octree(payload), Colormap.Viridis, payload.Bounds));

    public Seq<PointOctreeNode> Visible(Frustum frustum, double lodScale) =>
        Octree.Filter(node => frustum.Intersects(node.Bounds) && node.Level <= (int)lodScale);

    private static Seq<PointSample> Project(PointPayload payload) =>
        toSeq(MemoryMarshal.Cast<byte, PointSample>(payload.Points.Span).ToArray());

    private static Seq<PointOctreeNode> Octree(PointPayload payload) =>
        toSeq(Enumerable.Range(0, int.Max(payload.LevelDepth, 1))
            .Select(level => new PointOctreeNode(
                $"{payload.Key}/{level}", level, payload.Bounds, 1 << (payload.LevelDepth - level), payload.Count >> level, 0L)));
}
```

## [04]-[CAPTURE_PASS]

- Owner: `CapturePass` `[Union]` the reality-capture render-pass family; `CaptureVisual` the pass-to-`RenderPass` projection.
- Cases: `CapturePass` = Splat | Point under the locked kind literals splat, point.
- Entry: `public RenderPass Pass(RenderTargetFactory factory)` — projects the capture source into one viewport `RenderPass` case binding the `T-BACKEND-PORT` target factory; the splat pass radix-sorts and alpha-composites the ellipsoids, the point pass octree-subsamples and splats the points.
- Auto: the splat case binds the backend's splat-rasterizer kernel through the `RenderTargetFactory` so the ellipsoid draw composites into the same leased context the meshlet draw uses, never a second `GRContext`; the point case binds the backend's point-splat kernel through the same factory; both cases emit one `Geometry`-family `RenderPass` over the leased target so the render-graph pass algebra stays backend-agnostic and the per-backend divergence (wgpu `RenderPassEncoder` splat-rasterizer pipeline versus `SKRuntimeEffect` compute splat) lives below the factory column; the pass triangles-drawn projection reports the composited ellipsoid or point count so the frame receipt's budget verdict gates the capture pass identically to the geometry pass.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, SkiaSharp
- Growth: a new capture render path is one `CapturePass` case; zero new surface.
- Boundary: the capture pass is a viewport `RenderPass` case so the reality-capture geometry navigates beside the BIM geometry in one render graph and a parallel capture scene owner is the rejected form; the splat and point rasterization bind the `T-BACKEND-PORT` `RenderTargetFactory` so the capture pass binds a backend-provided target factory rather than hard-coding a second `GRContext` substrate, depending on `T-BACKEND-PORT` for the target-construction seam; the per-backend ellipsoid-rasterizer and point-splat kernels resolve under CAPTURE_GPU and a CPU ellipsoid-projection plus octree-subsample point draw are the floor for the 2D fallback while the GPU dispatch is the SPIKE; this pass is SPIKE-gated on the GEOMETRY_VIRTUAL GPU surface and rides the same shared-context lease.

```csharp signature
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CapturePass {
    private CapturePass() { }
    public sealed record Splat(string Key, SplatSource Source, Func<RenderTarget, SplatSource, Fin<int>> Composite) : CapturePass;
    public sealed record Point(string Key, PointCloudSource Source, Func<RenderTarget, PointCloudSource, Fin<int>> Splat) : CapturePass;

    public string Key => Switch(splat: static s => s.Key, point: static p => p.Key);

    public RenderPass Pass(RenderTargetFactory factory) => Switch(
        state: factory,
        splat: static (f, s) => (RenderPass)new RenderPass.Geometry(
            s.Key,
            (target, _, _) => f.Target(target.Info).Bind(bound => s.Composite(bound, s.Source))),
        point: static (f, p) => new RenderPass.Geometry(
            p.Key,
            (target, _, _) => f.Target(target.Info).Bind(bound => p.Splat(bound, p.Source))));
}
```

## [05]-[MEASURE_OVERLAY]

- Owner: `MeasurePoint` the LiDAR-anchored measurable vertex; `MeasureOverlay` the annotation set bound to the `Viewpoint`.
- Entry: `public Fin<MeasureOverlay> Anchor(MeasurePoint point)` — anchors a measurable vertex onto the capture cloud and folds the running distance and angle evidence; `public Viewpoint Bind(Viewpoint view)` — binds the overlay onto the viewpoint visibility set so a saved capture markup carries its measurements.
- Auto: each anchor snaps to the nearest LiDAR return so a measurement reads the scanned existing-conditions geometry, not a guessed point; the overlay folds the per-segment distance and per-vertex angle so a polyline measurement reads its total length and its turning angles; the overlay binds onto the `Viewpoint` so a measured markup rides the one portable view-state receipt the BCF codec and the coordination board consume — a capture measurement is a viewpoint annotation, not a second markup model.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, UnitsNet
- Growth: a new measurement kind is one `MeasurePoint` field; zero new surface.
- Boundary: the overlay anchors onto the LiDAR returns so a measurement is against scanned reality and a free-floating annotation is the deleted form; the overlay binds the `VIEWPOINT_CODEC` `Viewpoint` so the measurable markup rides the one portable view-state receipt and a parallel measurement-snapshot model is the rejected form — the same receipt a coordination issue and a saved camera carry; the distance and angle carry `UnitsNet` `Length` and `Angle` so a measurement reports in the model unit and a raw-double measurement is the deleted form.

```csharp signature
public readonly record struct MeasurePoint(double X, double Y, double Z, string SnappedReturn);

public sealed record MeasureSegment(MeasurePoint From, MeasurePoint To, UnitsNet.Length Distance);

public sealed record MeasureOverlay(string Key, Seq<MeasurePoint> Vertices, Seq<MeasureSegment> Segments) {
    public static MeasureOverlay Empty(string key) => new(key, Seq<MeasurePoint>(), Seq<MeasureSegment>());

    public Fin<MeasureOverlay> Anchor(MeasurePoint point) =>
        Vertices.LastOrNone().Match(
            None: () => Fin.Succ(this with { Vertices = Vertices.Add(point) }),
            Some: previous => Fin.Succ(this with {
                Vertices = Vertices.Add(point),
                Segments = Segments.Add(new MeasureSegment(previous, point, Span(previous, point))),
            }));

    public UnitsNet.Length Total =>
        Segments.Fold(UnitsNet.Length.Zero, static (sum, segment) => sum + segment.Distance);

    public Viewpoint Bind(Viewpoint view) =>
        view with { Overrides = view.Overrides + Vertices.Map(static v => new VisibilityOverride(v.SnappedReturn, true, None, 0d)) };

    private static UnitsNet.Length Span(MeasurePoint a, MeasurePoint b) =>
        UnitsNet.Length.FromMeters(Math.Sqrt(Math.Pow(b.X - a.X, 2) + Math.Pow(b.Y - a.Y, 2) + Math.Pow(b.Z - a.Z, 2)));
}
```

## [06]-[CAPTURE_CLIP]

- Owner: `CaptureFrame` the time-stamped capture epoch; `CaptureClip` the capture-frame playback bound to the animation playhead.
- Entry: `public FieldIndexTrack OnTimeline(string key)` — projects the capture epochs onto an animation `FieldIndex` track so a multi-epoch scan scrubs on the one playhead; the capture frame is a field index, never a wall-clock tick.
- Auto: each capture frame carries its epoch instant and its payload key so a multi-epoch reality capture (a construction-progress scan series) reads one frame per epoch; the clip projects the epochs onto an animation `FieldIndex` track so the capture-frame scrub rides the one deterministic playhead the kinematic camera and the transient field scrub share — a construction-progress scrub and a camera fly-through animate on the same timeline; the frame index selects the active `SplatSource`/`PointCloudSource` payload key so scrubbing the playhead swaps the rendered capture epoch.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime
- Growth: a new capture epoch is one `CaptureFrame` row; zero new surface.
- Boundary: the capture-frame scrub is an animation `FieldIndex` track so the capture playback rides the one playhead and a second capture timeline is the deleted form — the same frame-indexed deterministic clock the transient field scrub uses; the frame index selects the active capture payload so a wall-clock capture playback is the rejected form; the capture clip mints no second scrub owner and the animation `Scrub` drives it.

```csharp signature
public readonly record struct CaptureFrame(int Index, Instant Epoch, string PayloadKey);

public sealed record CaptureClip(string Key, Seq<CaptureFrame> Frames) {
    public Option<CaptureFrame> At(int index) => Frames.Find(frame => frame.Index == index);

    public Animation.Track.FieldIndex OnTimeline(string key) =>
        new(key, Frames.Map(frame => new Animation.Keyframe<int>(
            Duration.FromTimeSpan(frame.Epoch - Frames.Head.Epoch), frame.Index, MotionToken.Standard)).ToSeq());
}
```

## [07]-[RESEARCH]

- [CAPTURE_PAYLOAD]: the projection from the canonical Compute splat and point payloads (`SplatPayload`, `PointPayload`) into the `SplatEllipsoid` and `PointSample` interleaved runs is the cross-package wire boundary the capture sources never re-mint; the proto splat-primitive member set (mean/scale/rotation/spherical-harmonic accessors) and the point-primitive member set (position/classification/intensity/color accessors) resolve at implementation against the settled Compute interchange wire contract — the `SplatSource`/`PointCloudSource` shapes, the radix-sort and octree folds, and the residency keying are settled, the proto accessor spellings are the unverified surface.
- [CAPTURE_GPU]: the per-backend Gaussian-splat ellipsoid-rasterizer compute kernel (radix-sorted alpha-composited 3DGS over the `T-BACKEND-PORT` `RenderTargetFactory`), the point-splat compute kernel, and the bindless residency upload of a splat or point tile to a backend slot resolve under VIEWPORT_GPU against the live host-shared GPU context — the splat decode and radix sort, the point decode and octree LOD, the measurable overlay, and the capture-frame clip are settled and ship as the CPU/2D-fallback point preview, the GPU ellipsoid rasterization and point splatting are the unverified surface gated on the live host-owned GPU context and the backend target factory.
- [CAPTURE_DECODE]: the offline LAZ/E57/SOG scan decode is the Python companion's geometry producer ([UPSTREAM-BLOCKED] on the companion scan-decode pipeline) and crosses to AppUi exclusively as the decoded Compute `SplatPayload`/`PointPayload` at the interchange wire; AppUi admits the decoded payload and carries no scan-decode package, so the decode dependency is external and the AppUi consume-at-wire boundary is settled.
