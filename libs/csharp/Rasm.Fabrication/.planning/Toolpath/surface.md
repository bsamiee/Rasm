# [RASM_FABRICATION_SURFACE]

The surface toolpath owner closes 3D finishing over `SurfaceStrategy` and the one `SurfacePath.Sample` entry: strategy rows carry kernel-produced path layout, roughness-derived stepover, chip-load engagement, residual stock, and OpenCAMLib operation selection, while OpenCAMLib owns cutter-location positioning through one dynamic-link P/Invoke lifecycle. The page keeps layout on the kernel on-mesh machinery, maps `CutterForm` to the `Cyl`/`Ball`/`Bull`/`Cone`/`Composite` cutter edge, SIMD-lowers engagement rasters through span-backed grids, and routes empty or stalled cutter-location output to `FabricationFault.SampleStalled(strategy, iteration).ToError()`.

## [01]-[INDEX]

- [01]-[SURFACE_PATH]: the surface strategy vocabulary, kernel layout carrier, engagement budget, and `SurfacePath.Sample` entry.
- [02]-[OPENCAM_BOUNDARY]: the OpenCAMLib operation lifecycle, cutter map, native shim, wire content key, and SIMD raster seam.

## [02]-[SURFACE_PATH]

- Owner: `SurfaceStrategy` owns the waterline, scallop, pencil, rest, indexed, swarf, thread-mill, and drill-family finishing rows; `SurfacePolicy` owns roughness, sampling, and engagement; `SurfaceDriveSet` admits only kernel layout output; `SurfacePath` owns the one public sample fold returning owner-atom `Move` rows.
- Cases: `SurfaceStrategy` cases `Waterline` · `Scallop` · `Pencil` · `Rest` · `ThreePlusTwo` · `Swarf` · `ThreadMill` · `DrillFamily`; `SurfaceLayoutKind` rows `GeodesicParallel` · `ConstantStepover` · `Flowline` · `Morph` · `CrossField`.
- Entry: `public static Fin<Seq<Move>> Sample(SurfaceStrategy strategy, MeshSpace mesh, CutterForm cutter)` is the only surface finishing entry and the exact ruled seam consumed by the `Cam` fold.
- Auto: `Tolerance.ScallopStep(policy.Finish, cutter)` derives stepover; `SurfaceEngagement` carries the subtractive `RemovalBudget` admitted from `RemovalParameter.Budget`; rest machining reads input-carried `ResidualStock`; kernel layout arrives as `SurfaceDriveSet`; OpenCAMLib positions the cutter at those samples.
- Receipt: `SurfaceSampleReceipt` stays plane-local and carries `OpenCamLocation` rows plus the OpenCAMLib iteration count; the public surface returns only atoms-safe `Move` rows.
- Packages: `Process/owner#FABRICATION_OWNER` (`CutterForm`, `Move`, `ResidualStock`), `Spec/tolerance#TOLERANCE` (`RaTarget`, `Tolerance.ScallopStep`), `Process/physics#CUT_PARAMETER` (`RemovalBudget.Subtractive`), `Process/faults#FAULT_BAND` (`SampleStalled` 2713), LanguageExt.Core, Thinktecture.Runtime.Extensions, Rhino.Geometry, `Rasm.Meshing`.
- Growth: a finishing family lands as one `SurfaceStrategy` case plus one `OpenCamOperationKind` mapping row; 5-axis simultaneous refinement extends the `Swarf` row through tool-axis payload, not a second sampler.
- Boundary: the kernel owns geodesic, isoline, streamline, and cross-field layout; a Fabrication-side on-mesh solver is the `[V2]` defect. The kernel SDF drop-cutter over K8 stays the recorded admission-abandonment fallback row and never becomes a parallel implementation.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------------------------------------------------------------
using LanguageExt;
using Rasm.Fabrication.Process;
using Rasm.Fabrication.Spec;
using Rasm.Meshing;
using Rhino.Geometry;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Toolpath;

// --- [TYPES] --------------------------------------------------------------------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class SurfaceLayoutKind {
    public static readonly SurfaceLayoutKind GeodesicParallel = new("geodesic-parallel");
    public static readonly SurfaceLayoutKind ConstantStepover = new("constant-stepover");
    public static readonly SurfaceLayoutKind Flowline = new("flowline");
    public static readonly SurfaceLayoutKind Morph = new("morph");
    public static readonly SurfaceLayoutKind CrossField = new("cross-field");
}

// --- [MODELS] -------------------------------------------------------------------------------------------------------------------------------------
public readonly record struct SurfaceSampling(double MinimumStepMm, double MaximumStepMm, double CosLimit, int Threads, int MaxIterations);
public readonly record struct SurfaceEngagement(
    RemovalBudget.Subtractive Budget,
    EngagementPolicy Policy,
    double RadialLimit,
    double AxialLimit);
public sealed record SurfacePolicy(RaTarget Finish, SurfaceSampling Sampling, SurfaceEngagement Engagement);
public readonly record struct SurfaceDrive(SurfaceLayoutKind Kind, Arr<Point3d> Points, Option<Vector3d> ToolAxis, double Parameter);
public sealed record SurfaceDriveSet(SurfaceLayoutKind Kind, Seq<SurfaceDrive> Drives, double StepOverMm);
public sealed record SurfaceSampleReceipt(Arr<OpenCamLocation> Locations, int Iterations) {
    public Seq<Move> ToMoves() =>
        Locations.Map(static point => new Move(point.Location, point.Rapid, point.Feed, point.Arc)).ToSeq();
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SurfaceStrategy {
    private SurfaceStrategy() { }

    public sealed record Waterline(SurfacePolicy Policy, Arr<double> Levels, bool Adaptive) : SurfaceStrategy;
    public sealed record Scallop(SurfacePolicy Policy, SurfaceDriveSet Layout) : SurfaceStrategy;
    public sealed record Pencil(SurfacePolicy Policy, SurfaceDriveSet Layout, double ContactAngleDeg) : SurfaceStrategy;
    public sealed record Rest(SurfacePolicy Policy, SurfaceDriveSet Layout, ResidualStock Stock) : SurfaceStrategy;
    public sealed record ThreePlusTwo(SurfacePolicy Policy, SurfaceDriveSet Layout, Arr<ProjectionDir> IndexedViews) : SurfaceStrategy;
    public sealed record Swarf(SurfacePolicy Policy, SurfaceDriveSet Layout, ProjectionDir ToolAxis, double FlankOffsetMm) : SurfaceStrategy;
    public sealed record ThreadMill(SurfacePolicy Policy, SurfaceDriveSet Layout, double PitchMm, double DepthMm) : SurfaceStrategy;
    public sealed record DrillFamily(SurfacePolicy Policy, Arr<Point3d> Centers, double PeckMm) : SurfaceStrategy;
}

public sealed record SurfaceRun(
    SurfaceStrategy Strategy,
    MeshSpace Mesh,
    CutterForm Cutter,
    double StepOverMm,
    OpenCamOperationKind Operation,
    OpenCamCutterKind CutterKind,
    SurfaceSampling Sampling) {
    public static Fin<SurfaceRun> Of(SurfaceStrategy strategy, MeshSpace mesh, CutterForm cutter) =>
        Fin.Succ(new SurfaceRun(
            strategy,
            mesh,
            cutter,
            Tolerance.ScallopStep(SurfaceStrategyFold.Policy(strategy).Finish, cutter),
            OpenCamOperationKind.Of(strategy),
            OpenCamCutterKind.Of(cutter),
            SurfaceStrategyFold.Policy(strategy).Sampling));
}

// --- [OPERATIONS] ---------------------------------------------------------------------------------------------------------------------------------
public static class SurfaceStrategyFold {
    public static SurfacePolicy Policy(SurfaceStrategy strategy) =>
        strategy.Switch(
            waterline:     static row => row.Policy,
            scallop:       static row => row.Policy,
            pencil:        static row => row.Policy,
            rest:          static row => row.Policy,
            threePlusTwo:  static row => row.Policy,
            swarf:         static row => row.Policy,
            threadMill:    static row => row.Policy,
            drillFamily:   static row => row.Policy);

    public static Option<SurfaceDriveSet> Layout(SurfaceStrategy strategy) =>
        strategy.Switch(
            waterline:     static _ => None,
            scallop:       static row => Some(row.Layout),
            pencil:        static row => Some(row.Layout),
            rest:          static row => Some(row.Layout),
            threePlusTwo:  static row => Some(row.Layout),
            swarf:         static row => Some(row.Layout),
            threadMill:    static row => Some(row.Layout),
            drillFamily:   static _ => None);
}

public static class SurfacePath {
    public static Fin<Seq<Move>> Sample(SurfaceStrategy strategy, MeshSpace mesh, CutterForm cutter) =>
        SurfaceRun.Of(strategy, mesh, cutter)
            .Bind(OpenCamLib.Position)
            .Bind(sample => sample.Locations.IsEmpty
                ? Fin.Fail<Seq<Move>>(FabricationFault.SampleStalled(strategy, sample.Iterations).ToError())
                : Fin.Succ(sample.ToMoves()));
}
```

## [03]-[OPENCAM_BOUNDARY]

- Owner: `OpenCamOperationKind` owns the drop, push, and waterline operation rows; `OpenCamCutterKind` owns the `CutterForm` constructor map; `OpenCamWire` keys the sidecar payload; `OpenCamNative` owns the source-generated `[LibraryImport]` C-shim; `SurfaceEngagementRaster` owns the SIMD engagement plane.
- Cases: OpenCAMLib rows `BatchDropCutter` · `PathDropCutter` · `AdaptivePathDropCutter` · `BatchPushCutter` · `Waterline` · `AdaptiveWaterline`; cutter rows `Cyl` · `Ball` · `Bull` · `Cone` · `Composite`.
- Entry: `internal static Fin<SurfaceSampleReceipt> Position(SurfaceRun run)` executes the one `setSTL` -> `setCutter` -> `setSampling` -> `setThreads` -> `run` -> `getCLPoints`/`getLoops` lifecycle.
- Auto: `Waterline` and `AdaptiveWaterline` read `getLoops`; drop-cutter rows read `getCLPoints`; `BatchPushCutter` reads fibers only as the swarf positioning carrier; every native buffer crosses as flat `double[]` plus count.
- Receipt: `OpenCamWire.Key` mints through `ContentKey.Of(EgressKind.Plan, bytes)` before sidecar exchange; `SurfaceSampleReceipt` folds only cutter-location rows and iteration count back into the domain rail.
- Packages: OpenCAMLib dynamic `libocl`, source-generated `[LibraryImport]`, System.Numerics.Tensors (`TensorPrimitives`), CommunityToolkit.HighPerformance (`Span2D<T>`), LanguageExt.Core, BCL inbox.
- Growth: a native operation enters as one `OpenCamOperationKind` row, one strategy mapping arm, and one shim function only when the `.api` catalogue admits the member.
- Boundary: OpenCAMLib owns cutter positioning, not path layout, hashing, residual computation, tolerance derivation, or mesh conditioning. Static-folding `libocl`, direct C++ ABI binding, raw provider handles crossing the rail, and a second content hasher are rejected boundary forms.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------------------------------------------------------------
using System;
using System.Buffers.Binary;
using System.Linq;
using System.Numerics.Tensors;
using System.Runtime.InteropServices;
using CommunityToolkit.HighPerformance;
using LanguageExt;
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
        strategy switch {
            SurfaceStrategy.Waterline waterline when waterline.Adaptive => AdaptiveWaterline,
            SurfaceStrategy.Waterline => Waterline,
            SurfaceStrategy.DrillFamily => BatchDropCutter,
            SurfaceStrategy.Swarf => BatchPushCutter,
            SurfaceStrategy.Scallop scallop when scallop.Policy.Sampling.CosLimit < 1.0 => AdaptivePathDropCutter,
            SurfaceStrategy.Pencil pencil when pencil.Policy.Sampling.CosLimit < 1.0 => AdaptivePathDropCutter,
            _ => PathDropCutter,
        };
}

[SmartEnum<string>]
public sealed partial class OpenCamCutterKind {
    public static readonly OpenCamCutterKind Cyl = new("cyl");
    public static readonly OpenCamCutterKind Ball = new("ball");
    public static readonly OpenCamCutterKind Bull = new("bull");
    public static readonly OpenCamCutterKind Cone = new("cone");
    public static readonly OpenCamCutterKind Composite = new("composite");

    public static OpenCamCutterKind Of(CutterForm cutter) =>
        cutter.Family == CutterFamily.Flat ? Cyl
        : cutter.Family == CutterFamily.Ball ? Ball
        : cutter.Family == CutterFamily.Bull ? Bull
        : cutter.Family == CutterFamily.Taper || cutter.Family == CutterFamily.Chamfer ? Cone
        : Composite;
}

// --- [MODELS] -------------------------------------------------------------------------------------------------------------------------------------
public readonly record struct OpenCamMeshBuffer(double[] Triangles, int TriangleCount) {
    public static OpenCamMeshBuffer Project(MeshSpace mesh) =>
        MeshSpaceOpenCam.Project(mesh);
}

public readonly record struct OpenCamDriveBuffer(double[] Points, int PointCount) {
    public static OpenCamDriveBuffer Project(SurfaceStrategy strategy) {
        Arr<Point3d> points = strategy switch {
            SurfaceStrategy.DrillFamily drill => drill.Centers,
            SurfaceStrategy.Waterline waterline => waterline.Levels.Map(static z => new Point3d(0.0, 0.0, z)).ToArr(),
            _ => SurfaceStrategyFold.Layout(strategy).Match(
                Some: static layout => layout.Drives.Bind(static drive => drive.Points).ToArr(),
                None: static () => Empty),
        };

        double[] buffer = new double[points.Count * 3];
        for (int index = 0; index < points.Count; index++) {
            Point3d point = points[index];
            buffer[(index * 3) + 0] = point.X;
            buffer[(index * 3) + 1] = point.Y;
            buffer[(index * 3) + 2] = point.Z;
        }

        return new OpenCamDriveBuffer(buffer, points.Count);
    }
}

public readonly record struct OpenCamWire(ContentKey Key, OpenCamMeshBuffer Mesh, OpenCamDriveBuffer Drive) {
    // key-over-PAYLOAD: the canonical preimage is step-over + the FULL mesh and drive coordinate buffers —
    // cardinality-only bytes collide distinct geometry with equal counts onto one plan key.
    public static OpenCamWire Project(SurfaceRun run) {
        OpenCamMeshBuffer mesh = OpenCamMeshBuffer.Project(run.Mesh);
        OpenCamDriveBuffer drive = OpenCamDriveBuffer.Project(run.Strategy);
        byte[] canonical = new byte[8 + (8 * mesh.Triangles.Length) + (8 * drive.Points.Length)];
        BinaryPrimitives.WriteDoubleLittleEndian(canonical, run.StepOverMm);
        MemoryMarshal.AsBytes<double>(mesh.Triangles).CopyTo(canonical.AsSpan(8));
        MemoryMarshal.AsBytes<double>(drive.Points).CopyTo(canonical.AsSpan(8 + (8 * mesh.Triangles.Length)));

        return new OpenCamWire(ContentKey.Of(EgressKind.Plan, canonical), mesh, drive);
    }
}

public readonly record struct OpenCamLocation(Point3d Location, bool Rapid, double Feed, Option<ArcCenter> Arc);
public sealed record OpenCamRequest(
    SurfaceStrategy Strategy,
    OpenCamOperationKind Operation,
    OpenCamCutterKind CutterKind,
    CutterForm Cutter,
    OpenCamWire Wire,
    SurfaceSampling Sampling);

// --- [SERVICES] -----------------------------------------------------------------------------------------------------------------------------------
internal static partial class OpenCamNative {
    internal const string Library = "ocl";

    [LibraryImport(Library, EntryPoint = "ocl_op_create")]
    internal static partial nint OperationCreate(int operation);
    [LibraryImport(Library, EntryPoint = "ocl_op_set_stl")]
    internal static partial int OperationSetStl(nint operation, double[] triangles, int triangleCount);
    [LibraryImport(Library, EntryPoint = "ocl_op_set_cutter")]
    internal static partial int OperationSetCutter(nint operation, nint cutter);
    [LibraryImport(Library, EntryPoint = "ocl_op_set_sampling")]
    internal static partial int OperationSetSampling(nint operation, double sampling);
    [LibraryImport(Library, EntryPoint = "ocl_op_set_threads")]
    internal static partial int OperationSetThreads(nint operation, int threads);
    [LibraryImport(Library, EntryPoint = "ocl_op_run")]
    internal static partial int OperationRun(nint operation);
    [LibraryImport(Library, EntryPoint = "ocl_op_get_clpoints")]
    internal static partial int OperationGetClPoints(nint operation, double[] output, ref int count);
    [LibraryImport(Library, EntryPoint = "ocl_op_get_loops")]
    internal static partial int OperationGetLoops(nint operation, double[] output, ref int count);
    [LibraryImport(Library, EntryPoint = "ocl_op_destroy")]
    internal static partial void OperationDestroy(nint operation);
    [LibraryImport(Library, EntryPoint = "ocl_cutter_cyl")]
    internal static partial nint CutterCyl(double diameter, double length);
    [LibraryImport(Library, EntryPoint = "ocl_cutter_ball")]
    internal static partial nint CutterBall(double diameter, double length);
    [LibraryImport(Library, EntryPoint = "ocl_cutter_bull")]
    internal static partial nint CutterBull(double diameter, double radius, double length);
    [LibraryImport(Library, EntryPoint = "ocl_cutter_cone")]
    internal static partial nint CutterCone(double diameter, double angle, double length);
}

// --- [OPERATIONS] ---------------------------------------------------------------------------------------------------------------------------------
internal static class OpenCamCutter {
    public static nint Create(OpenCamCutterKind kind, CutterForm cutter) =>
        kind == OpenCamCutterKind.Cyl ? OpenCamNative.CutterCyl(cutter.Diameter, cutter.FluteLength)
        : kind == OpenCamCutterKind.Ball ? OpenCamNative.CutterBall(cutter.Diameter, cutter.FluteLength)
        : kind == OpenCamCutterKind.Bull ? OpenCamNative.CutterBull(cutter.Diameter, cutter.CornerRadius, cutter.FluteLength)
        : kind == OpenCamCutterKind.Cone ? OpenCamNative.CutterCone(cutter.Diameter, cutter.TaperAngle, cutter.FluteLength)
        : OpenCamComposite.Create(cutter);
}

internal static class OpenCamComposite {
    public static nint Create(CutterForm cutter) => cutter.CornerRadius > 0.0 ? OpenCamNative.CutterBull(cutter.Diameter, cutter.CornerRadius, cutter.FluteLength) : OpenCamNative.CutterCone(cutter.Diameter, cutter.TaperAngle, cutter.FluteLength);
}

internal static class OpenCamRequestFactory {
    public static Fin<OpenCamRequest> Of(SurfaceRun run) =>
        Fin.Succ(new OpenCamRequest(run.Strategy, run.Operation, run.CutterKind, run.Cutter, OpenCamWire.Project(run), run.Sampling));
}

internal static class OpenCamLocationBuffer {
    public static Arr<OpenCamLocation> Project(double[] values, int count) =>
        Enumerable.Range(0, count / 5)
            .Map(index => new OpenCamLocation(
                new Point3d(values[(index * 5) + 0], values[(index * 5) + 1], values[(index * 5) + 2]),
                Rapid: false,
                Feed: values[(index * 5) + 3],
                Arc: Optional(new ArcCenter(new Point3d(values[(index * 5) + 0], values[(index * 5) + 1], values[(index * 5) + 4]), Clockwise: false))))
            .ToArr();
}

public static class SurfaceEngagementRaster {
    public static void Bound(
        ReadOnlySpan<double> commandedFeed,
        ReadOnlySpan<double> loadFraction,
        Span<double> boundedFeed,
        Span2D<double> engagement,
        RemovalBudget.Subtractive budget) {
        TensorPrimitives.Multiply(commandedFeed, loadFraction, boundedFeed);
        TensorPrimitives.Clamp(boundedFeed, 0.0, budget.FeedRate, boundedFeed);

        for (int row = 0; row < boundedFeed.Length; row++)
            engagement[row, 0] = boundedFeed[row];
    }
}

internal static class OpenCamLib {
    internal static Fin<SurfaceSampleReceipt> Position(SurfaceRun run) =>
        OpenCamRequestFactory.Of(run).Bind(Execute);

    static Fin<SurfaceSampleReceipt> Execute(OpenCamRequest request) {
        nint operation = OpenCamNative.OperationCreate(request.Operation.Code);
        nint cutter = OpenCamCutter.Create(request.CutterKind, request.Cutter);
        double[] output = new double[request.Wire.Drive.PointCount * 5];
        int count = output.Length;

        try {
            // Each native call gates the next: the FIRST nonzero status decides — a summed aggregate lets a
            // failed call hide behind later successes or opposite-signed codes canceling to zero.
            int status = Gate(
                () => OpenCamNative.OperationSetStl(operation, request.Wire.Mesh.Triangles, request.Wire.Mesh.TriangleCount),
                () => OpenCamNative.OperationSetCutter(operation, cutter),
                () => OpenCamNative.OperationSetSampling(operation, request.Sampling.MinimumStepMm),
                () => OpenCamNative.OperationSetThreads(operation, request.Sampling.Threads),
                () => OpenCamNative.OperationRun(operation));

            int read = status != 0
                ? status
                : request.Operation == OpenCamOperationKind.Waterline || request.Operation == OpenCamOperationKind.AdaptiveWaterline
                    ? OpenCamNative.OperationGetLoops(operation, output, ref count)
                    : OpenCamNative.OperationGetClPoints(operation, output, ref count);

            return read == 0
                ? Fin.Succ(new SurfaceSampleReceipt(OpenCamLocationBuffer.Project(output, count), request.Sampling.MaxIterations))
                : Fin.Fail<SurfaceSampleReceipt>(FabricationFault.SampleStalled(request.Strategy, request.Sampling.MaxIterations).ToError());
        }
        finally {
            OpenCamNative.OperationDestroy(operation);
        }
    }

    // Exemption: the sequential first-failure walk over native status codes is the platform-forced seam.
    static int Gate(params ReadOnlySpan<Func<int>> steps) {
        foreach (Func<int> step in steps) {
            int status = step();
            if (status != 0)
                return status;
        }
        return 0;
    }
}
```
