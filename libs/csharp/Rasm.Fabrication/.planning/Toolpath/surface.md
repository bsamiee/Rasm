# [RASM_FABRICATION_SURFACE]

The surface toolpath owner closes 3D finishing over `SurfaceStrategy` and the one `SurfacePath.Sample` entry: strategy rows carry their layout discriminant, roughness-derived stepover, and physics-admitted engagement, while OpenCAMLib owns cutter-location positioning through one capsule-owned dynamic-link P/Invoke lifecycle. Layout is PRODUCED, never admitted: `SurfaceDriveSet` is minted only by `SurfaceLayout.Produce` — the planar raster arm locally, the on-mesh arms through the injected kernel seam — so a caller-fabricated vertex bag can no longer satisfy the layout contract and every drive set carries its producing `SurfaceLayoutKind` as provenance. Each admitted OpenCAMLib operation receives its catalogued geometry input before `run` — the path for `PathDropCutter`, seeded points for `BatchDropCutter`, z-levels for `Waterline`, fibers for `BatchPushCutter` — and both native handles live in `SafeHandle` capsules, so construction, execution, and release failures all land typed on the `Fin` rail.

## [01]-[INDEX]

- [01]-[SURFACE_PATH]: the surface strategy vocabulary with its base-threaded policy column, the layout production seam, and the `SurfacePath.Sample` entry.
- [02]-[OPENCAM_BOUNDARY]: the OpenCAMLib operation lifecycle with per-operation geometry input, the cutter constructor map, the capsule handles, the full-axis wire content key, and the SIMD raster seam.

## [02]-[SURFACE_PATH]

- Owner: `SurfaceStrategy` owns the waterline, scallop, pencil, rest, indexed, swarf, thread-mill, and drill-family finishing rows over one base-positional `SurfacePolicy` column — `SurfaceStrategyFold` is deleted, `strategy.Policy` is direct; `SurfacePolicy` composes sampling, the motion-minted `EngagementPolicy` (budget, finish, engagement bounds), and the optional on-mesh layout seam; `SurfaceLayout` owns drive production; `SurfacePath` owns the one public sample fold returning owner-atom `Move` rows.
- Cases: `SurfaceStrategy` cases `Waterline` · `Scallop` · `Pencil` · `Rest` · `ThreePlusTwo` · `Swarf` · `ThreadMill` · `DrillFamily`; `SurfaceLayoutKind` rows `GeodesicParallel` · `ConstantStepover` · `Flowline` · `Morph` · `CrossField` — each strategy carries the KIND, and `SurfaceLayout.Produce` turns the kind into drives.
- Entry: `public static Fin<Seq<Move>> Sample(SurfaceStrategy strategy, MeshSpace mesh, CutterForm cutter)` is the only surface finishing entry and the exact ruled seam consumed by the `Cam` fold.
- Auto: `Tolerance.ScallopStep(strategy.Policy.Engagement.Finish, cutter)` derives stepover; feed derives once from `EngagementPolicy.Feed` — the budget-case projection, never a literal; `SurfaceLayout.Produce` dispatches the layout kind — `ConstantStepover` is the local planar raster over the mesh buffer's XY extents (drop-cutter supplies Z), the on-mesh kinds (`GeodesicParallel`/`Flowline`/`Morph`/`CrossField`) ride `SurfacePolicy.Layout`'s injected kernel column bound at the composition root to `GeodesicKernel.GeodesicTangentAt(space, sources, sample, key)` tangent sampling traced through `FlowKernel.Trace<Polyline>` — the cross-package injected-delegate seam; an on-mesh kind with no bound seam routes `GeometryFault.DegenerateInput`, never a profile-vertex fake; rest machining reads input-carried `ResidualStock`; OpenCAMLib positions the cutter at the produced samples.
- Receipt: `SurfaceSampleReceipt` stays plane-local and carries the projected `OpenCamLocation` rows, the executed native operation code, and the final native status; the public surface returns only atom-safe `Move` rows.
- Packages: `Process/owner#FABRICATION_OWNER` (`CutterForm`, `CutterFamily`, `Move`, `ResidualStock`, `ContentKey`, `EgressKind`), `Spec/tolerance#TOLERANCE` (`RaTarget`, `Tolerance.ScallopStep`), `Toolpath/motion#CAM_MOTION` (`EngagementPolicy`), `Process/faults#FAULT_BAND` (`SampleStalled` 2713), kernel `Rasm` (`GeodesicKernel.GeodesicTangentAt`, `FlowKernel.Trace<Polyline>` — the layout seam binding), LanguageExt.Core, Thinktecture.Runtime.Extensions, Rhino.Geometry, `Rasm.Meshing`.
- Growth: a finishing family lands as one `SurfaceStrategy` case plus one `OpenCamOperationKind` mapping arm; a new layout is one `SurfaceLayoutKind` row plus one `Produce` arm; 5-axis simultaneous refinement extends the `Swarf` row through the drive `ToolAxis` column, not a second sampler.
- Boundary: the kernel owns on-mesh layout machinery; a Fabrication-side on-mesh solver is the `[V2]` defect, and the injected seam column is how the kernel entries bind without a phantom local re-derivation. The kernel SDF drop-cutter over K8 stays the recorded admission-abandonment fallback row and never becomes a parallel implementation. A `SurfaceDriveSet` constructed from raw profile vertices at a caller is the deleted form the production-only admission forecloses.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------------------------------------------------------------
using LanguageExt;
using Rasm.Fabrication.Process;
using Rasm.Fabrication.Spec;
using Rasm.Meshing;
using Rasm.Numerics;
using Rhino.Geometry;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Toolpath;

// --- [TYPES] --------------------------------------------------------------------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class SurfaceLayoutKind {
    public static readonly SurfaceLayoutKind GeodesicParallel = new("geodesic-parallel", onMesh: true);
    public static readonly SurfaceLayoutKind ConstantStepover = new("constant-stepover", onMesh: false);
    public static readonly SurfaceLayoutKind Flowline = new("flowline", onMesh: true);
    public static readonly SurfaceLayoutKind Morph = new("morph", onMesh: true);
    public static readonly SurfaceLayoutKind CrossField = new("cross-field", onMesh: true);

    public bool OnMesh { get; }
}

// --- [MODELS] -------------------------------------------------------------------------------------------------------------------------------------
public readonly record struct SurfaceSampling(double MinimumStepMm, double MaximumStepMm, double CosLimit, int Threads, int MaxIterations);

// The injected kernel layout column (ARCHITECTURE [04]): bound at the composition root to
// GeodesicKernel.GeodesicTangentAt tangent sampling traced through FlowKernel.Trace<Polyline>.
public sealed record LayoutSeam(Func<MeshSpace, SurfaceLayoutKind, double, Fin<Seq<SurfaceDrive>>> OnMesh);

public sealed record SurfacePolicy(SurfaceSampling Sampling, EngagementPolicy Engagement, Option<LayoutSeam> Layout = default);

public readonly record struct SurfaceDrive(Arr<Point3d> Points, Option<Vector3d> ToolAxis, double Parameter);

public sealed record SurfaceDriveSet(SurfaceLayoutKind Kind, Seq<SurfaceDrive> Drives, double StepOverMm);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SurfaceStrategy(SurfacePolicy Policy) {
    public sealed record Waterline(SurfacePolicy Policy, Arr<double> Levels, bool Adaptive) : SurfaceStrategy(Policy);
    public sealed record Scallop(SurfacePolicy Policy, SurfaceLayoutKind Layout) : SurfaceStrategy(Policy);
    public sealed record Pencil(SurfacePolicy Policy, SurfaceLayoutKind Layout, double ContactAngleDeg) : SurfaceStrategy(Policy);
    public sealed record Rest(SurfacePolicy Policy, SurfaceLayoutKind Layout, ResidualStock Stock) : SurfaceStrategy(Policy);
    public sealed record ThreePlusTwo(SurfacePolicy Policy, SurfaceLayoutKind Layout, Arr<ProjectionDir> IndexedViews) : SurfaceStrategy(Policy);
    public sealed record Swarf(SurfacePolicy Policy, SurfaceLayoutKind Layout, ProjectionDir ToolAxis, double FlankOffsetMm) : SurfaceStrategy(Policy);
    public sealed record ThreadMill(SurfacePolicy Policy, SurfaceLayoutKind Layout, double PitchMm, double DepthMm) : SurfaceStrategy(Policy);
    public sealed record DrillFamily(SurfacePolicy Policy, Arr<Point3d> Centers, double PeckMm) : SurfaceStrategy(Policy);
}

public sealed record SurfaceRun(
    SurfaceStrategy Strategy,
    MeshSpace Mesh,
    CutterForm Cutter,
    double StepOverMm,
    OpenCamOperationKind Operation,
    OpenCamCutterKind CutterKind,
    SurfaceSampling Sampling,
    Option<SurfaceDriveSet> Drives) {
    public static Fin<SurfaceRun> Of(SurfaceStrategy strategy, MeshSpace mesh, CutterForm cutter) =>
        Tolerance.ScallopStep(strategy.Policy.Engagement.Finish, cutter) is var step && step <= 0.0
            ? Fin.Fail<SurfaceRun>(GeometryFault.DegenerateInput("surface:non-positive-stepover").ToError())
            : SurfaceLayout.Produce(strategy, mesh, step).Map(drives =>
                new SurfaceRun(strategy, mesh, cutter, step, OpenCamOperationKind.Of(strategy), OpenCamCutterKind.Of(cutter), strategy.Policy.Sampling, drives));
}

// --- [OPERATIONS] ---------------------------------------------------------------------------------------------------------------------------------
// Layout is PRODUCED here or not at all: the planar raster arm is local (drop-cutter supplies Z), the
// on-mesh arms ride the injected kernel seam; an unbound on-mesh request is a typed refusal, never a
// profile-vertex fake at the caller.
public static class SurfaceLayout {
    public static Fin<Option<SurfaceDriveSet>> Produce(SurfaceStrategy strategy, MeshSpace mesh, double stepOver) =>
        strategy.Switch(
            waterline:    static _ => Fin.Succ(Option<SurfaceDriveSet>.None),
            scallop:      row => Laid(row.Policy, row.Layout, mesh, stepOver),
            pencil:       row => Laid(row.Policy, row.Layout, mesh, stepOver),
            rest:         row => Laid(row.Policy, row.Layout, mesh, stepOver),
            threePlusTwo: row => Laid(row.Policy, row.Layout, mesh, stepOver),
            swarf:        row => Laid(row.Policy, row.Layout, mesh, stepOver),
            threadMill:   row => Laid(row.Policy, row.Layout, mesh, stepOver),
            drillFamily:  static _ => Fin.Succ(Option<SurfaceDriveSet>.None));

    static Fin<Option<SurfaceDriveSet>> Laid(SurfacePolicy policy, SurfaceLayoutKind kind, MeshSpace mesh, double stepOver) =>
        (kind.OnMesh
            ? policy.Layout.Match(
                Some: seam => seam.OnMesh(mesh, kind, stepOver),
                None: () => Fin.Fail<Seq<SurfaceDrive>>(GeometryFault.DegenerateInput($"surface-layout:unbound-seam:{kind.Key}").ToError()))
            : Raster(mesh, stepOver))
        .Map(drives => Optional(new SurfaceDriveSet(kind, drives, stepOver)));

    // Planar raster: parallel Y-lines over the mesh buffer's XY extents at stepover spacing — the classic
    // 3-axis raster layout; Z is the drop-cutter's, so the drive rides at the extent ceiling.
    static Fin<Seq<SurfaceDrive>> Raster(MeshSpace mesh, double stepOver) {
        OpenCamMeshBuffer buffer = OpenCamMeshBuffer.Project(mesh);
        if (buffer.TriangleCount == 0)
            return Fin.Fail<Seq<SurfaceDrive>>(GeometryFault.DegenerateInput("surface-layout:empty-mesh").ToError());

        (Point3d min, Point3d max, double top) = buffer.ExtentsXy();
        return Fin.Succ(
            Range(0, Math.Max(1, (int)Math.Ceiling((max.Y - min.Y) / Math.Max(stepOver, 1e-6))))
                .Map(row => min.Y + (row * stepOver))
                .Map(y => new SurfaceDrive(
                    Arr(new Point3d(min.X, y, top), new Point3d(max.X, y, top)),
                    Option<Vector3d>.None,
                    Parameter: y))
                .ToSeq());
    }
}

public static class SurfacePath {
    public static Fin<Seq<Move>> Sample(SurfaceStrategy strategy, MeshSpace mesh, CutterForm cutter) =>
        SurfaceRun.Of(strategy, mesh, cutter)
            .Bind(OpenCamLib.Position)
            .Bind(sample => sample.Locations.IsEmpty
                ? Fin.Fail<Seq<Move>>(FabricationFault.SampleStalled(strategy, sample.Status).ToError())
                : Fin.Succ(sample.ToMoves(strategy.Policy.Engagement.Feed)));
}
```

## [03]-[OPENCAM_BOUNDARY]

- Owner: `OpenCamOperationKind` owns the drop, push, and waterline operation rows; `OpenCamCutterKind` owns the `CutterFamily`-total cutter constructor map with composite arbitration; `OpenCamWire` keys the sidecar payload over EVERY determining axis; `OpenCamNative` owns the source-generated `[LibraryImport]` C-shim; `OclOperationHandle`/`OclCutterHandle`/`OclPathHandle` are the `SafeHandle` capsules; `SurfaceEngagementRaster` owns the SIMD engagement plane.
- Cases: OpenCAMLib rows `BatchDropCutter` · `PathDropCutter` · `AdaptivePathDropCutter` · `BatchPushCutter` · `Waterline` · `AdaptiveWaterline`; cutter rows `Cyl` · `Ball` · `Bull` · `Cone` · `Composite`.
- Entry: `internal static Fin<SurfaceSampleReceipt> Position(SurfaceRun run)` executes the operation-complete lifecycle: `setSTL` → `setCutter` → `setSampling`/`setMinSampling`/`setCosLimit`/`setThreads` → the OPERATION-SPECIFIC geometry input (`setPath` per drive for path drops, `appendPoint` per center for batch drops, `setZ` per level for waterlines, `appendFiber` + `setXDirection`/`setYDirection` for push fibers) → `run` → `getCLPoints`/`getLoops`, each output sized by the count-query shim call before the fill.
- Auto: `Waterline`/`AdaptiveWaterline` fold their level set through `setZ` + `run` + `getLoops` with `reset` between levels; drop-cutter rows read `getCLPoints`; `BatchPushCutter` reads `getFibers`-backed intervals lowered to CL rows by the shim; every native buffer crosses as flat `double[]` plus count; adaptive rows carry `setMinSampling`/`setCosLimit` from `SurfaceSampling` so the Min AND Max/limit axes both cross.
- Receipt: `OpenCamWire.Key` mints through `ContentKey.Of(EgressKind.Plan, bytes)` over the FULL preimage — operation code, cutter scalars, sampling, stepover, mesh buffer, drive buffer, level set — so two runs differing on any determining axis never collide; `SurfaceSampleReceipt` folds the location rows plus the executed op code and final native status back into the domain rail.
- Packages: OpenCAMLib dynamic `libocl` (verified upstream members: `setSTL`/`setCutter`/`setSampling`/`setMinSampling`/`setCosLimit`/`setThreads`/`setZ`/`appendPoint`/`setPath`/`appendFiber`/`setXDirection`/`setYDirection`/`run`/`reset`/`getCLPoints`/`getLoops`/`getFibers`; cutters `CylCutter`/`BallCutter`/`BullCutter`/`ConeCutter`/`CylConeCutter`/`BullConeCutter`), source-generated `[LibraryImport]`, System.Numerics.Tensors (`TensorPrimitives`), CommunityToolkit.HighPerformance (`Span2D<T>`, `GetRowSpan`), LanguageExt.Core, BCL inbox.
- Growth: a native operation enters as one `OpenCamOperationKind` row, one strategy mapping arm, and its shim functions only when the `.api` catalogue admits the member; a new cutter row is one `OpenCamCutterKind` arm over the existing shim ctor set.
- Boundary: OpenCAMLib owns cutter positioning, not path layout, hashing, residual computation, tolerance derivation, or mesh conditioning. Static-folding `libocl`, direct C++ ABI binding, raw `nint` handles outside the `SafeHandle` capsules, a leaked cutter or path handle, an output buffer sized from an unrelated count, a partial-axis wire key, and a second content hasher are rejected boundary forms.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------------------------------------------------------------
using System;
using System.Buffers.Binary;
using System.Linq;
using System.Numerics.Tensors;
using System.Runtime.InteropServices;
using CommunityToolkit.HighPerformance;
using LanguageExt;
using Microsoft.Win32.SafeHandles;
using Rasm.Fabrication.Process;
using Rhino.Geometry;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Toolpath;

// --- [TYPES] --------------------------------------------------------------------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class OpenCamOperationKind {
    public static readonly OpenCamOperationKind BatchDropCutter = new("batch-drop-cutter", 1);
    public static readonly OpenCamOperationKind PathDropCutter = new("path-drop-cutter", 2);
    public static readonly OpenCamOperationKind AdaptivePathDropCutter = new("adaptive-path-drop-cutter", 3);
    public static readonly OpenCamOperationKind BatchPushCutter = new("batch-push-cutter", 4);
    public static readonly OpenCamOperationKind Waterline = new("waterline", 5);
    public static readonly OpenCamOperationKind AdaptiveWaterline = new("adaptive-waterline", 6);

    public int Code { get; }

    public static OpenCamOperationKind Of(SurfaceStrategy strategy) =>
        strategy.Switch(
            waterline:    static row => row.Adaptive ? AdaptiveWaterline : Waterline,
            scallop:      static row => row.Policy.Sampling.CosLimit < 1.0 ? AdaptivePathDropCutter : PathDropCutter,
            pencil:       static row => row.Policy.Sampling.CosLimit < 1.0 ? AdaptivePathDropCutter : PathDropCutter,
            rest:         static _ => PathDropCutter,
            threePlusTwo: static _ => PathDropCutter,
            swarf:        static _ => BatchPushCutter,
            threadMill:   static _ => PathDropCutter,
            drillFamily:  static _ => BatchDropCutter);
}

// CutterFamily-total constructor map: drill and chamfer tips are cones, thread mills present a cylinder,
// and a form carrying BOTH corner radius and taper arbitrates to the composite bull-cone.
[SmartEnum<string>]
public sealed partial class OpenCamCutterKind {
    public static readonly OpenCamCutterKind Cyl = new("cyl", 1, MintCyl);
    public static readonly OpenCamCutterKind Ball = new("ball", 2, MintBall);
    public static readonly OpenCamCutterKind Bull = new("bull", 3, MintBull);
    public static readonly OpenCamCutterKind Cone = new("cone", 4, MintCone);
    public static readonly OpenCamCutterKind Composite = new("composite", 5, MintComposite);

    public int Code { get; }

    [UseDelegateFromConstructor]
    public partial OclCutterHandle Mint(CutterForm cutter);

    public static OpenCamCutterKind Of(CutterForm cutter) =>
        cutter is { CornerRadius: > 0.0, TaperAngle: > 0.0 }
            ? Composite
            : cutter.Family.Switch(
                flat:       static () => Cyl,
                ball:       static () => Ball,
                bull:       static () => Bull,
                taper:      static () => Cone,
                drill:      static () => Cone,
                chamfer:    static () => Cone,
                threadMill: static () => Cyl);

    static OclCutterHandle MintCyl(CutterForm c) => OpenCamNative.CutterCyl(c.Diameter, c.FluteLength);
    static OclCutterHandle MintBall(CutterForm c) => OpenCamNative.CutterBall(c.Diameter, c.FluteLength);
    static OclCutterHandle MintBull(CutterForm c) => OpenCamNative.CutterBull(c.Diameter, c.CornerRadius, c.FluteLength);
    static OclCutterHandle MintCone(CutterForm c) => OpenCamNative.CutterCone(c.Diameter, c.TaperAngle, c.FluteLength);
    static OclCutterHandle MintComposite(CutterForm c) => OpenCamNative.CutterBullCone(c.Diameter, c.CornerRadius, c.FluteLength, c.TaperAngle);
}

// --- [MODELS] -------------------------------------------------------------------------------------------------------------------------------------
public readonly record struct OpenCamMeshBuffer(double[] Triangles, int TriangleCount) {
    // Quad faces split on the A-C diagonal; the buffer is 9 doubles per triangle over the snapshot's
    // Native vertices — the one MeshSpace-to-libocl lowering.
    public static OpenCamMeshBuffer Project(MeshSpace mesh) {
        Mesh native = mesh.Native;
        int[] corners = [.. native.Faces.SelectMany(face => face.IsQuad
            ? new[] { face.A, face.B, face.C, face.A, face.C, face.D }
            : new[] { face.A, face.B, face.C })];
        double[] buffer = new double[corners.Length * 3];
        for (int i = 0; i < corners.Length; i++) {                           // Exemption: flat marshalling fill — the one boundary copy into the native buffer
            Point3f vertex = native.Vertices[corners[i]];
            buffer[i * 3] = vertex.X;
            buffer[(i * 3) + 1] = vertex.Y;
            buffer[(i * 3) + 2] = vertex.Z;
        }
        return new OpenCamMeshBuffer(buffer, corners.Length / 3);
    }

    // XY extents plus the Z ceiling, folded over the flat vertex buffer — the raster layout's frame.
    public (Point3d Min, Point3d Max, double Top) ExtentsXy() =>
        Enumerable.Range(0, Triangles.Length / 3).Aggregate(
            (Min: new Point3d(double.MaxValue, double.MaxValue, 0.0), Max: new Point3d(double.MinValue, double.MinValue, 0.0), Top: double.MinValue),
            (state, i) => (
                new Point3d(Math.Min(state.Min.X, Triangles[i * 3]), Math.Min(state.Min.Y, Triangles[(i * 3) + 1]), 0.0),
                new Point3d(Math.Max(state.Max.X, Triangles[i * 3]), Math.Max(state.Max.Y, Triangles[(i * 3) + 1]), 0.0),
                Math.Max(state.Top, Triangles[(i * 3) + 2])));
}

public readonly record struct OpenCamDriveBuffer(double[] Points, int PointCount) {
    public static OpenCamDriveBuffer Project(SurfaceRun run) {
        Arr<Point3d> points = run.Strategy.Switch(
            waterline:    static row => row.Levels.Map(static z => new Point3d(0.0, 0.0, z)).ToArr(),
            scallop:      _ => DrivePoints(run),
            pencil:       _ => DrivePoints(run),
            rest:         _ => DrivePoints(run),
            threePlusTwo: _ => DrivePoints(run),
            swarf:        _ => DrivePoints(run),
            threadMill:   _ => DrivePoints(run),
            drillFamily:  static row => row.Centers);

        double[] buffer = new double[points.Count * 3];
        for (int index = 0; index < points.Count; index++) {                 // Exemption: flat marshalling fill — the one boundary copy into the native buffer
            buffer[(index * 3) + 0] = points[index].X;
            buffer[(index * 3) + 1] = points[index].Y;
            buffer[(index * 3) + 2] = points[index].Z;
        }
        return new OpenCamDriveBuffer(buffer, points.Count);
    }

    static Arr<Point3d> DrivePoints(SurfaceRun run) =>
        run.Drives.Map(static set => set.Drives.Bind(static drive => drive.Points.ToSeq()).ToArr()).IfNone(Arr<Point3d>.Empty);
}

// key-over-EVERY-DETERMINING-AXIS: operation code, cutter scalars, sampling, stepover, mesh and drive
// buffers — a partial preimage collides plans that differ on an omitted axis onto one cache key.
public readonly record struct OpenCamWire(ContentKey Key, OpenCamMeshBuffer Mesh, OpenCamDriveBuffer Drive) {
    public static OpenCamWire Project(SurfaceRun run) {
        OpenCamMeshBuffer mesh = OpenCamMeshBuffer.Project(run.Mesh);
        OpenCamDriveBuffer drive = OpenCamDriveBuffer.Project(run);
        Span<double> axes = [
            run.Operation.Code, run.CutterKind.Code, run.Cutter.Diameter, run.Cutter.CornerRadius, run.Cutter.TaperAngle, run.Cutter.FluteLength,
            run.Sampling.MinimumStepMm, run.Sampling.MaximumStepMm, run.Sampling.CosLimit, run.StepOverMm,
        ];
        byte[] canonical = new byte[(8 * axes.Length) + (8 * mesh.Triangles.Length) + (8 * drive.Points.Length)];
        MemoryMarshal.AsBytes(axes).CopyTo(canonical);
        MemoryMarshal.AsBytes<double>(mesh.Triangles).CopyTo(canonical.AsSpan(8 * axes.Length));
        MemoryMarshal.AsBytes<double>(drive.Points).CopyTo(canonical.AsSpan((8 * axes.Length) + (8 * mesh.Triangles.Length)));
        return new OpenCamWire(ContentKey.Of(EgressKind.Plan, canonical), mesh, drive);
    }
}

// CL rows are position + contact classification; feed is assigned by the DOMAIN at ToMoves from the
// admitted budget — the native engine positions, it never prices motion.
public readonly record struct OpenCamLocation(Point3d Location, int Contact);

public sealed record SurfaceSampleReceipt(Arr<OpenCamLocation> Locations, int OpCode, int Status) {
    public Seq<Move> ToMoves(double feed) =>
        Locations.Map(point => new Move(point.Location, Rapid: false, Feed: feed)).ToSeq();
}

// --- [SERVICES] -----------------------------------------------------------------------------------------------------------------------------------
public sealed class OclOperationHandle : SafeHandleZeroOrMinusOneIsInvalid {
    public OclOperationHandle() : base(ownsHandle: true) { }
    protected override bool ReleaseHandle() { OpenCamNative.OperationDestroy(handle); return true; }
}

public sealed class OclCutterHandle : SafeHandleZeroOrMinusOneIsInvalid {
    public OclCutterHandle() : base(ownsHandle: true) { }
    protected override bool ReleaseHandle() { OpenCamNative.CutterDestroy(handle); return true; }
}

public sealed class OclPathHandle : SafeHandleZeroOrMinusOneIsInvalid {
    public OclPathHandle() : base(ownsHandle: true) { }
    protected override bool ReleaseHandle() { OpenCamNative.PathDestroy(handle); return true; }
}

internal static partial class OpenCamNative {
    internal const string Library = "ocl";

    [LibraryImport(Library, EntryPoint = "ocl_op_create")]
    internal static partial OclOperationHandle OperationCreate(int operation);
    [LibraryImport(Library, EntryPoint = "ocl_op_set_stl")]
    internal static partial int OperationSetStl(OclOperationHandle operation, double[] triangles, int triangleCount);
    [LibraryImport(Library, EntryPoint = "ocl_op_set_cutter")]
    internal static partial int OperationSetCutter(OclOperationHandle operation, OclCutterHandle cutter);
    [LibraryImport(Library, EntryPoint = "ocl_op_set_sampling")]
    internal static partial int OperationSetSampling(OclOperationHandle operation, double sampling);
    [LibraryImport(Library, EntryPoint = "ocl_op_set_min_sampling")]
    internal static partial int OperationSetMinSampling(OclOperationHandle operation, double sampling);
    [LibraryImport(Library, EntryPoint = "ocl_op_set_cos_limit")]
    internal static partial int OperationSetCosLimit(OclOperationHandle operation, double cosLimit);
    [LibraryImport(Library, EntryPoint = "ocl_op_set_threads")]
    internal static partial int OperationSetThreads(OclOperationHandle operation, int threads);
    [LibraryImport(Library, EntryPoint = "ocl_op_set_z")]
    internal static partial int OperationSetZ(OclOperationHandle operation, double z);
    [LibraryImport(Library, EntryPoint = "ocl_op_append_point")]
    internal static partial int OperationAppendPoint(OclOperationHandle operation, double x, double y, double z);
    [LibraryImport(Library, EntryPoint = "ocl_op_set_path")]
    internal static partial int OperationSetPath(OclOperationHandle operation, OclPathHandle path);
    [LibraryImport(Library, EntryPoint = "ocl_op_append_fiber")]
    internal static partial int OperationAppendFiber(OclOperationHandle operation, double x1, double y1, double z1, double x2, double y2, double z2);
    [LibraryImport(Library, EntryPoint = "ocl_op_set_x_direction")]
    internal static partial int OperationSetXDirection(OclOperationHandle operation);
    [LibraryImport(Library, EntryPoint = "ocl_op_set_y_direction")]
    internal static partial int OperationSetYDirection(OclOperationHandle operation);
    [LibraryImport(Library, EntryPoint = "ocl_op_run")]
    internal static partial int OperationRun(OclOperationHandle operation);
    [LibraryImport(Library, EntryPoint = "ocl_op_reset")]
    internal static partial int OperationReset(OclOperationHandle operation);
    [LibraryImport(Library, EntryPoint = "ocl_op_cl_count")]
    internal static partial int OperationClCount(OclOperationHandle operation);
    [LibraryImport(Library, EntryPoint = "ocl_op_get_clpoints")]
    internal static partial int OperationGetClPoints(OclOperationHandle operation, double[] output, int capacity);
    [LibraryImport(Library, EntryPoint = "ocl_op_get_loops")]
    internal static partial int OperationGetLoops(OclOperationHandle operation, double[] output, int capacity);
    [LibraryImport(Library, EntryPoint = "ocl_op_destroy")]
    internal static partial void OperationDestroy(nint operation);
    [LibraryImport(Library, EntryPoint = "ocl_path_create")]
    internal static partial OclPathHandle PathCreate();
    [LibraryImport(Library, EntryPoint = "ocl_path_append_line")]
    internal static partial int PathAppendLine(OclPathHandle path, double x1, double y1, double z1, double x2, double y2, double z2);
    [LibraryImport(Library, EntryPoint = "ocl_path_destroy")]
    internal static partial void PathDestroy(nint path);
    [LibraryImport(Library, EntryPoint = "ocl_cutter_cyl")]
    internal static partial OclCutterHandle CutterCyl(double diameter, double length);
    [LibraryImport(Library, EntryPoint = "ocl_cutter_ball")]
    internal static partial OclCutterHandle CutterBall(double diameter, double length);
    [LibraryImport(Library, EntryPoint = "ocl_cutter_bull")]
    internal static partial OclCutterHandle CutterBull(double diameter, double radius, double length);
    [LibraryImport(Library, EntryPoint = "ocl_cutter_cone")]
    internal static partial OclCutterHandle CutterCone(double diameter, double angle, double length);
    [LibraryImport(Library, EntryPoint = "ocl_cutter_bullcone")]
    internal static partial OclCutterHandle CutterBullCone(double diameter, double radius, double length, double angle);
    [LibraryImport(Library, EntryPoint = "ocl_cutter_destroy")]
    internal static partial void CutterDestroy(nint cutter);
}

// --- [OPERATIONS] ---------------------------------------------------------------------------------------------------------------------------------
internal static class OpenCamLib {
    internal static Fin<SurfaceSampleReceipt> Position(SurfaceRun run) {
        using OclOperationHandle operation = OpenCamNative.OperationCreate(run.Operation.Code);
        using OclCutterHandle cutter = run.CutterKind.Mint(run.Cutter);
        OpenCamWire wire = OpenCamWire.Project(run);

        return operation.IsInvalid || cutter.IsInvalid
            ? Fin.Fail<SurfaceSampleReceipt>(FabricationFault.SampleStalled(run.Strategy, -1).ToError())
            : Gate(run,
                () => OpenCamNative.OperationSetStl(operation, wire.Mesh.Triangles, wire.Mesh.TriangleCount),
                () => OpenCamNative.OperationSetCutter(operation, cutter),
                () => OpenCamNative.OperationSetSampling(operation, run.Sampling.MaximumStepMm),
                () => OpenCamNative.OperationSetMinSampling(operation, run.Sampling.MinimumStepMm),
                () => OpenCamNative.OperationSetCosLimit(operation, run.Sampling.CosLimit),
                () => OpenCamNative.OperationSetThreads(operation, run.Sampling.Threads))
              .Bind(_ => Geometry(operation, run, wire))
              .Bind(_ => Harvest(operation, run));
    }

    // Operation-specific geometry input: the selected row's catalogued carrier reaches the native side
    // BEFORE run — a code-only dispatch that never feeds setPath/appendPoint/setZ/appendFiber is the
    // deleted form the catalog refutes.
    static Fin<int> Geometry(OclOperationHandle operation, SurfaceRun run, OpenCamWire wire) =>
        run.Operation.Code switch {
            1 => Gate(run, ForPoints(operation, wire)),
            2 or 3 => run.Drives.Match(
                Some: set => set.Drives.Fold(Fin.Succ(0), (state, drive) => state.Bind(_ => PathOf(operation, drive))),
                None: () => Fin.Fail<int>(GeometryFault.DegenerateInput("opencam:path-without-drives").ToError())),
            4 => run.Drives.Match(
                Some: set => Gate(run, [.. set.Drives.Filter(static d => d.Points.Count >= 2).Map(drive => Fiber(operation, drive)), () => OpenCamNative.OperationSetXDirection(operation)]),
                None: () => Fin.Fail<int>(GeometryFault.DegenerateInput("opencam:fiber-without-drives").ToError())),
            _ => Fin.Succ(0),
        };

    // The waterline pair folds its level set with reset between levels; every other operation runs once.
    // The single-case union probe (`is Waterline`) is the sanctioned case extraction at this boundary.
    static Fin<SurfaceSampleReceipt> Harvest(OclOperationHandle operation, SurfaceRun run) =>
        (run.Operation.Code is 5 or 6
            ? run.Strategy is SurfaceStrategy.Waterline waterline
                ? waterline.Levels.Fold(
                    Fin.Succ(Arr<OpenCamLocation>.Empty),
                    (state, level) => state.Bind(acc =>
                        Gate(run, () => OpenCamNative.OperationReset(operation), () => OpenCamNative.OperationSetZ(operation, level), () => OpenCamNative.OperationRun(operation))
                            .Bind(_ => Read(operation, run, loops: true))
                            .Map(rows => acc.Concat(rows).ToArr())))
                : Fin.Fail<Arr<OpenCamLocation>>(GeometryFault.DegenerateInput("opencam:loop-op-mismatch").ToError())
            : Gate(run, () => OpenCamNative.OperationRun(operation)).Bind(_ => Read(operation, run, loops: false)))
        .Map(rows => new SurfaceSampleReceipt(rows, run.Operation.Code, Status: 0));

    // Two-call size-then-fill: the count query owns the capacity — sizing output from drive cardinality
    // under-allocates every loop-producing operation.
    static Fin<Arr<OpenCamLocation>> Read(OclOperationHandle operation, SurfaceRun run, bool loops) {
        int count = OpenCamNative.OperationClCount(operation);
        if (count < 0)
            return Fin.Fail<Arr<OpenCamLocation>>(FabricationFault.SampleStalled(run.Strategy, count).ToError());

        double[] output = new double[count * 4];
        int status = loops
            ? OpenCamNative.OperationGetLoops(operation, output, output.Length)
            : OpenCamNative.OperationGetClPoints(operation, output, output.Length);
        return status != 0
            ? Fin.Fail<Arr<OpenCamLocation>>(FabricationFault.SampleStalled(run.Strategy, status).ToError())
            : Fin.Succ(toArr(Enumerable.Range(0, count).Select(i =>
                new OpenCamLocation(new Point3d(output[i * 4], output[(i * 4) + 1], output[(i * 4) + 2]), (int)output[(i * 4) + 3]))));
    }

    static Fin<int> PathOf(OclOperationHandle operation, SurfaceDrive drive) {
        using OclPathHandle path = OpenCamNative.PathCreate();
        return path.IsInvalid || drive.Points.Count < 2
            ? Fin.Fail<int>(GeometryFault.DegenerateInput("opencam:path").ToError())
            : toSeq(Enumerable.Range(0, drive.Points.Count - 1))
                .Fold(Fin.Succ(0), (state, i) => state.Bind(_ => Status(OpenCamNative.PathAppendLine(
                    path,
                    drive.Points[i].X, drive.Points[i].Y, drive.Points[i].Z,
                    drive.Points[i + 1].X, drive.Points[i + 1].Y, drive.Points[i + 1].Z))))
                .Bind(_ => Status(OpenCamNative.OperationSetPath(operation, path)));
    }

    static Func<int> Fiber(OclOperationHandle operation, SurfaceDrive drive) =>
        () => OpenCamNative.OperationAppendFiber(
            operation,
            drive.Points[0].X, drive.Points[0].Y, drive.Points[0].Z,
            drive.Points[drive.Points.Count - 1].X, drive.Points[drive.Points.Count - 1].Y, drive.Points[drive.Points.Count - 1].Z);

    static Func<int>[] ForPoints(OclOperationHandle operation, OpenCamWire wire) =>
        [.. Enumerable.Range(0, wire.Drive.PointCount).Select<int, Func<int>>(i =>
            () => OpenCamNative.OperationAppendPoint(operation, wire.Drive.Points[i * 3], wire.Drive.Points[(i * 3) + 1], wire.Drive.Points[(i * 3) + 2]))];

    static Fin<int> Status(int code) =>
        code == 0 ? Fin.Succ(0) : Fin.Fail<int>(GeometryFault.DegenerateInput($"opencam:status:{code}").ToError());

    // Exemption: the sequential first-failure walk over native status codes is the platform-forced seam —
    // a summed aggregate lets a failed call hide behind later successes or opposite-signed codes.
    static Fin<int> Gate(SurfaceRun run, params ReadOnlySpan<Func<int>> steps) {
        foreach (Func<int> step in steps) {
            int status = step();
            if (status != 0)
                return Fin.Fail<int>(FabricationFault.SampleStalled(run.Strategy, status).ToError());
        }
        return Fin.Succ(0);
    }
}

// The engagement plane is TOTAL: every row of the raster receives its bounded-feed result — rows are
// passes, columns are stations; a single-column write under a 2D contract is the deleted form.
public static class SurfaceEngagementRaster {
    public static void Bound(
        ReadOnlySpan2D<double> commandedFeed,
        ReadOnlySpan2D<double> loadFraction,
        Span2D<double> boundedFeed,
        RemovalBudget.Subtractive budget) {
        for (int row = 0; row < boundedFeed.Height; row++) {                 // Exemption: per-row SIMD sweep — TensorPrimitives runs each contiguous row span
            TensorPrimitives.Multiply(commandedFeed.GetRowSpan(row), loadFraction.GetRowSpan(row), boundedFeed.GetRowSpan(row));
            TensorPrimitives.Clamp(boundedFeed.GetRowSpan(row), 0.0, budget.FeedRate, boundedFeed.GetRowSpan(row));
        }
    }
}
```
