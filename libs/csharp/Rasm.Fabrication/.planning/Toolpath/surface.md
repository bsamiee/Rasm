# [RASM_FABRICATION_SURFACE]

The surface owner closes analytic 3-axis cutter positioning over one `SurfacePath.Sample` entry. Layout drives are produced inside the owner or through the injected kernel-layout function carried by `SurfacePolicy`; callers cannot fabricate a drive set. Every drive, waterline loop, and push-cutter fiber survives as one `CutElement`, so native output grouping reaches `Link.Route` and no implicit feed chord joins independent paths.

OpenCAMLib crosses through an authored `extern "C"` shim because the verified upstream surface is C++ only. Each operation capsule owns its cutter, mesh, sampling, and operation-specific input. A path capsule remains alive through `setPath`, `run`, and `getCLPoints`; path operations execute once per drive instead of overwriting the operation's path repeatedly before one run. Waterlines use loop-count and per-loop point-count queries, and push-cutter reads fiber groups, so provider topology is never decoded as a flat `CLPoint` stream.

Wire posture: HOST-LOCAL. `Seq<CutElement>` crosses to `Cam.Generate`; native handles stay file-local to the boundary, and the run, drive-set, and receipt carriers that cross between the sampling and boundary files stay package-`internal`, never public.

## [01]-[INDEX]

- [01]-[SURFACE_PATH]: owns `SurfaceStrategy`, layout production, policy admission, and `SurfacePath.Sample → Fin<Seq<CutElement>>`.
- [02]-[OPENCAM_BOUNDARY]: owns operation/cutter row maps, mesh/path lowering, capsule lifetimes, grouped size-then-fill reads, typed native-status routing, the authored `ocl_shim.cpp` extern C body covering every declared entry point, and the shim build/RID asset matrix.

## [02]-[SURFACE_PATH]

- Owner: `SurfaceStrategy` is the payload-bearing request family over one base `SurfacePolicy`; `SurfaceLayoutKind` is a public structural layout value whose named values seed the injected generator instead of closing its vocabulary; `WaterlineMode` is the standard/adaptive operation policy; `SurfaceLayout` produces drives; `SurfacePath` exposes the sole entry. The injected layout function is an explicitly admitted policy field, not a defaulted ghost or one-field wrapper beside the policy owner.
- Cases: `Waterline`, `Scallop`, `Pencil`, `Rest`, `FiberSlice`, `ThreePlusTwo`, `Swarf`, and `DrillFamily`. The layout vocabulary spans planar raster, geodesic, flowline, morph, cross-field, iso-parametric, radial, spiral, projected-curve, boundary/contour-parallel, constant-cusp, steep, shallow, principal-curvature, texture-field, and drive-surface fields through one injected generator. `FiberSlice` admits endpoint-pair drives into `BatchPushCutter`; `ThreadMill` is not a surface case because `CutStrategy.ThreadMill` is owned by the motion helix. `ThreePlusTwo` and `Swarf` retain the demanded axis payload but fail admission until the owner-atom motion seam carries orientation; emitting Z-only moves for either request is forbidden.
- Entry: `public static Fin<Seq<CutElement>> Sample(SurfaceStrategy strategy, MeshSpace mesh, CutterForm cutter)` is the only surface entry. It validates the mesh, cutter, sampling bounds, policy-derived stepover, strategy payload, produced drives, and every native output group before returning.
- Auto: `Tolerance.ScallopStep` derives stepover. Constant-stepover raster reads the mesh bounds once, rejects a degenerate width, interpolates row ordinates with `TensorPrimitives.Lerp`, and materializes serpentine endpoints in parallel through `ParallelHelper.For2D<TAction>` with its required struct action. On-mesh layouts invoke the injected kernel function through a `Try.lift` trap so a thrown layout callback enters the typed rail. Pencil contact angle tightens the adaptive cosine limit. Rest layout intersects each drive's spans with the input-carried residual regions through `ClipOpen` and re-emits every connected in-region interval as an independent drive, so no conditioned drive crosses an excluded interval. Drill-family centers feed `BatchDropCutter`; an empty center set is the admitted no-op only after policy admission. Independent request defects accumulate before any dependent layout or native execution. The engagement feed comes from the admitted motion policy and never from native code.
- Receipt: `SurfaceSampleReceipt` is package-internal and preserves `Arr<OpenCamLocation>` groups plus typed operation identity; a nonzero native status can exist only as a typed `SampleStalled` failure, so the receipt carries no reconstructible status column. `ToElements` lowers each non-empty group to one `CutElement`.
- Packages: OpenCAMLib (`BatchDropCutter`, `PathDropCutter`, `AdaptivePathDropCutter`, `BatchPushCutter`, `Waterline`, `AdaptiveWaterline`, their verified setters and grouped outputs), System.Numerics.Tensors (`TensorPrimitives.Lerp`), CommunityToolkit.HighPerformance (`ParallelHelper.For2D<TAction>`, `IAction2D`), `Rasm.Meshing`, `Spec/tolerance.md` (`Tolerance`), `Toolpath/link.md` (`CutElement`), `Toolpath/motion.md` (`EngagementPolicy`), LanguageExt.Core, Thinktecture.Runtime.Extensions, RhinoCommon, source-generated interop, BCL inbox.
- Growth: a new 3-axis operation is one strategy case, one operation row mapping, and one operation-specific capsule arm. Simultaneous orientation lands only after `Move` and the machine solve carry an axis frame.
- Boundary: a caller-built drive set, path disposed before `run`, repeated `setPath` followed by one run, integer-code redispatch, flat loop/fiber decoding, unchecked output multiplication, non-finite native point, ignored contact-angle or residual payload, ambient thread count, or Z-only multi-axis claim is a deleted form.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------------------------------------------------------------
using System.Numerics.Tensors;
using System.Runtime.InteropServices;
using CommunityToolkit.HighPerformance.Helpers;
using LanguageExt;
using LanguageExt.Common;
using Rasm.Fabrication.Geometry2D;
using Rasm.Fabrication.Process;
using Rasm.Fabrication.Spec;
using Rasm.Meshing;
using Rasm.Numerics;
using Rhino.Geometry;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Toolpath;

// --- [TYPES] --------------------------------------------------------------------------------------------------------------------------------------
public sealed record SurfaceLayoutKind(string Key, bool OnMesh) {
    public static readonly SurfaceLayoutKind GeodesicParallel = new("geodesic-parallel", OnMesh: true);
    public static readonly SurfaceLayoutKind ConstantStepover = new("constant-stepover", OnMesh: false);
    public static readonly SurfaceLayoutKind Flowline = new("flowline", OnMesh: true);
    public static readonly SurfaceLayoutKind Morph = new("morph", OnMesh: true);
    public static readonly SurfaceLayoutKind CrossField = new("cross-field", OnMesh: true);
    public static readonly SurfaceLayoutKind IsoParametric = new("iso-parametric", OnMesh: true);
    public static readonly SurfaceLayoutKind Radial = new("radial", OnMesh: true);
    public static readonly SurfaceLayoutKind Spiral = new("spiral", OnMesh: true);
    public static readonly SurfaceLayoutKind ProjectedCurve = new("projected-curve", OnMesh: true);
    public static readonly SurfaceLayoutKind BoundaryParallel = new("boundary-parallel", OnMesh: true);
    public static readonly SurfaceLayoutKind ContourParallel = new("contour-parallel", OnMesh: true);
    public static readonly SurfaceLayoutKind ConstantCusp = new("constant-cusp", OnMesh: true);
    public static readonly SurfaceLayoutKind Steep = new("steep", OnMesh: true);
    public static readonly SurfaceLayoutKind Shallow = new("shallow", OnMesh: true);
    public static readonly SurfaceLayoutKind PrincipalCurvature = new("principal-curvature", OnMesh: true);
    public static readonly SurfaceLayoutKind TextureField = new("texture-field", OnMesh: true);
    public static readonly SurfaceLayoutKind DriveSurface = new("drive-surface", OnMesh: true);
}

[SmartEnum<string>]
public sealed partial class WaterlineMode {
    public static readonly WaterlineMode Standard = new("standard", usesAdaptiveOperation: false);
    public static readonly WaterlineMode Adaptive = new("adaptive", usesAdaptiveOperation: true);

    public bool UsesAdaptiveOperation { get; }
}

// --- [MODELS] -------------------------------------------------------------------------------------------------------------------------------------
public readonly record struct SurfaceSampling(
    double MinimumStepMm,
    double MaximumStepMm,
    double CosLimit,
    int Threads,
    int MaximumTriangles,
    int MaximumGroups,
    int MaximumPointsPerGroup);

public sealed record SurfacePolicy(
    SurfaceSampling Sampling,
    EngagementPolicy Engagement,
    Option<Func<MeshSpace, SurfaceLayoutKind, double, Fin<Seq<SurfaceDrive>>>> Layout);

public readonly record struct SurfaceDrive(Arr<Point3d> Points, double Parameter);

internal sealed record SurfaceDriveSet(SurfaceLayoutKind Kind, Seq<SurfaceDrive> Drives, double StepOverMm);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SurfaceStrategy(SurfacePolicy Policy) {
    public sealed record Waterline(SurfacePolicy Policy, Arr<double> Levels, WaterlineMode Mode) : SurfaceStrategy(Policy);
    public sealed record Scallop(SurfacePolicy Policy, SurfaceLayoutKind Layout) : SurfaceStrategy(Policy);
    public sealed record Pencil(SurfacePolicy Policy, SurfaceLayoutKind Layout, double ContactAngleDeg) : SurfaceStrategy(Policy);
    public sealed record Rest(SurfacePolicy Policy, SurfaceLayoutKind Layout, ResidualStock Stock) : SurfaceStrategy(Policy);
    public sealed record FiberSlice(SurfacePolicy Policy, SurfaceLayoutKind Layout) : SurfaceStrategy(Policy);
    public sealed record ThreePlusTwo(SurfacePolicy Policy, SurfaceLayoutKind Layout, Arr<ProjectionDir> IndexedViews) : SurfaceStrategy(Policy);
    public sealed record Swarf(SurfacePolicy Policy, SurfaceLayoutKind Layout, ProjectionDir ToolAxis, double FlankOffsetMm) : SurfaceStrategy(Policy);
    public sealed record DrillFamily(SurfacePolicy Policy, Arr<Point3d> Centers) : SurfaceStrategy(Policy);
}

internal sealed record SurfaceRun(
    SurfaceStrategy Strategy,
    MeshSpace Mesh,
    CutterForm Cutter,
    double StepOverMm,
    OpenCamOperationKind Operation,
    OpenCamCutterKind CutterKind,
    SurfaceSampling Sampling,
    Option<SurfaceDriveSet> Drives) {
    public static Fin<SurfaceRun> Of(SurfaceStrategy strategy, MeshSpace mesh, CutterForm cutter) {
        SurfaceSampling sampling = EffectiveSampling(strategy);
        Seq<Error> gates = Seq(
            (Ok: strategy is not (SurfaceStrategy.ThreePlusTwo or SurfaceStrategy.Swarf), Axis: "tool-axis-unrepresentable"),
            (Ok: ValidPayload(strategy), Axis: "strategy-payload"),
            (Ok: strategy.Policy.Engagement.Budget is ProcessBudget.Subtractive, Axis: "non-subtractive-budget"),
            (Ok: cutter.Diameter > 0.0 && double.IsFinite(cutter.Diameter), Axis: "cutter"),
            (Ok: Valid(sampling), Axis: "sampling"))
            .Filter(static row => !row.Ok)
            .Map(static row => GeometryFault.DegenerateInput($"surface:{row.Axis}").ToError());
        return gates.IsEmpty
            ? strategy.Policy.Engagement.Admit(cutter)
                .Bind(_ => Tolerance.ScallopStep(strategy.Policy.Engagement.Finish, cutter))
                .Bind(step => step <= 0.0 || !double.IsFinite(step)
                    ? Fin.Fail<SurfaceRun>(GeometryFault.DegenerateInput("surface:stepover").ToError())
                    : SurfaceLayout.Produce(strategy, mesh, step).Bind(drives => ValidDrives(strategy, drives)
                        ? Fin.Succ(new SurfaceRun(strategy, mesh, cutter, step, OpenCamOperationKind.Of(strategy, sampling), OpenCamCutterKind.Of(cutter), sampling, drives))
                        : Fin.Fail<SurfaceRun>(GeometryFault.DegenerateInput("surface:drive-payload").ToError())))
            : Fin.Fail<SurfaceRun>(Error.Many([.. gates]));
    }

    private static SurfaceSampling EffectiveSampling(SurfaceStrategy strategy) =>
        strategy is SurfaceStrategy.Pencil pencil
            ? pencil.Policy.Sampling with {
                CosLimit = Math.Min(
                    pencil.Policy.Sampling.CosLimit,
                    Math.Cos(Math.Clamp(pencil.ContactAngleDeg, 0.0, 90.0) * Math.PI / 180.0)),
            }
            : strategy.Policy.Sampling;

    private static bool Valid(SurfaceSampling sampling) =>
        sampling.MinimumStepMm > 0.0
        && sampling.MaximumStepMm >= sampling.MinimumStepMm
        && double.IsFinite(sampling.MinimumStepMm)
        && double.IsFinite(sampling.MaximumStepMm)
        && double.IsFinite(sampling.CosLimit)
        && sampling.CosLimit is >= -1.0 and <= 1.0
        && sampling.Threads >= 1
        && sampling.MaximumTriangles >= 1
        && sampling.MaximumTriangles <= Array.MaxLength / 9
        && sampling.MaximumGroups >= 1
        && sampling.MaximumPointsPerGroup >= 1
        && sampling.MaximumPointsPerGroup <= Array.MaxLength / 4;

    private static bool ValidPayload(SurfaceStrategy strategy) =>
        strategy.Switch(
            waterline:    static row => row.Mode is not null && !row.Levels.IsEmpty && row.Levels.All(double.IsFinite),
            scallop:      static row => Valid(row.Layout),
            pencil:       static row => Valid(row.Layout)
                && row.ContactAngleDeg is >= 0.0 and <= 90.0 && double.IsFinite(row.ContactAngleDeg),
            rest:         static row => Valid(row.Layout) && row.Stock.Uncut.All(static loop => loop.Closed && loop.Count >= 3),
            fiberSlice:   static row => Valid(row.Layout),
            threePlusTwo: static row => Valid(row.Layout) && !row.IndexedViews.IsEmpty
                && row.IndexedViews.All(static view => view is not null && view.Forward.IsValid),
            swarf:        static row => Valid(row.Layout) && row.ToolAxis is not null && row.ToolAxis.Forward.IsValid
                && row.FlankOffsetMm >= 0.0 && double.IsFinite(row.FlankOffsetMm),
            drillFamily:  static row => !row.Centers.IsEmpty && row.Centers.All(static center => center.IsValid));

    private static bool Valid(SurfaceLayoutKind layout) =>
        layout is not null && !string.IsNullOrWhiteSpace(layout.Key);

    private static bool ValidDrives(SurfaceStrategy strategy, Option<SurfaceDriveSet> drives) =>
        strategy is not SurfaceStrategy.FiberSlice
        || drives.Match(
            Some: static set => !set.Drives.IsEmpty && set.Drives.All(static drive => drive.Points.Count == 2),
            None: static () => false);
}

// --- [OPERATIONS] ---------------------------------------------------------------------------------------------------------------------------------
file static class SurfaceLayout {
    public static Fin<Option<SurfaceDriveSet>> Produce(SurfaceStrategy strategy, MeshSpace mesh, double stepOver) =>
        strategy.Switch(
            waterline:    static _ => Fin.Succ(Option<SurfaceDriveSet>.None),
            scallop:      row => Laid(row.Policy, row.Layout, mesh, stepOver),
            pencil:       row => Laid(row.Policy, row.Layout, mesh, stepOver),
            rest:         row => Laid(row.Policy, row.Layout, mesh, stepOver).Bind(set => Rested(set, row.Stock)),
            fiberSlice:   row => Laid(row.Policy, row.Layout, mesh, stepOver),
            threePlusTwo: static _ => Fin.Fail<Option<SurfaceDriveSet>>(GeometryFault.DegenerateInput("surface-layout:tool-axis").ToError()),
            swarf:        static _ => Fin.Fail<Option<SurfaceDriveSet>>(GeometryFault.DegenerateInput("surface-layout:tool-axis").ToError()),
            drillFamily:  static _ => Fin.Succ(Option<SurfaceDriveSet>.None));

    private static Fin<Option<SurfaceDriveSet>> Laid(SurfacePolicy policy, SurfaceLayoutKind kind, MeshSpace mesh, double stepOver) =>
        (kind.OnMesh
            ? policy.Layout.Match(
                Some: layout => Try.lift(() => layout(mesh, kind, stepOver)).Run()
                    .MapFail(error => GeometryFault.DegenerateInput($"surface-layout:thrown:{kind.Key}:{error.Message}").ToError())
                    .Bind(identity),
                None: () => Fin.Fail<Seq<SurfaceDrive>>(GeometryFault.DegenerateInput($"surface-layout:unbound:{kind.Key}").ToError()))
            : Raster(mesh, stepOver))
        .Bind(drives => drives.IsEmpty || drives.Exists(static drive => drive.Points.Count < 2 || drive.Points.Exists(static point => !point.IsValid))
            ? Fin.Fail<Option<SurfaceDriveSet>>(GeometryFault.DegenerateInput($"surface-layout:invalid:{kind.Key}").ToError())
            : Fin.Succ(Optional(new SurfaceDriveSet(kind, drives, stepOver))));

    // Rest conditioning is SPAN intersection, never endpoint containment: each drive's polyline splits against
    // the residual regions through ClipOpen, and every connected in-region interval re-emits as its OWN drive —
    // a span crossing an interior residual loop with neither endpoint retained still cuts, and retained samples
    // never rejoin as one chord across excluded stock.
    private static Fin<Option<SurfaceDriveSet>> Rested(Option<SurfaceDriveSet> set, ResidualStock stock) =>
        set.Match(
            None: static () => Fin.Succ(Option<SurfaceDriveSet>.None),
            Some: layout => stock.Uncut.IsEmpty
                ? Fin.Succ(Some(layout with { Drives = Seq<SurfaceDrive>() }))
                : layout.Drives.TraverseM(drive =>
                        PolygonAlgebra.ClipOpen(
                            toSeq(Enumerable.Range(0, drive.Points.Count - 1))
                                .Map(index => new Edge3(drive.Points[index], drive.Points[index + 1])),
                            stock.Uncut.ToSeq(),
                            PolygonFill.NonZero)
                        .Map(split => Chains(split.Inside, stock.Uncut[0].Tolerance.Absolute.Value)
                            .Map(chain => new SurfaceDrive(chain, drive.Parameter))))
                    .As()
                    .Map(drives => Some(layout with {
                        Drives = drives.Bind(identity).Filter(static drive => drive.Points.Count >= 2),
                    })));

    // Contiguous in-region runs: consecutive clipped edges weld into one chain at the region tolerance, and a
    // gap starts the next independent drive.
    private static Seq<Arr<Point3d>> Chains(Seq<Edge3> inside, double weld) =>
        inside.Fold(Seq<Seq<Point3d>>(), (chains, edge) => chains.LastOrNone()
                .Filter(chain => chain.Last.DistanceTo(edge.A) <= weld)
                .Match(
                    Some: chain => chains.Init.Add(chain.Add(edge.B)),
                    None: () => chains.Add(Seq(edge.A, edge.B))))
            .Map(static chain => chain.ToArr());

    private static Fin<Seq<SurfaceDrive>> Raster(MeshSpace mesh, double stepOver) {
        BoundingBox box = mesh.Native.GetBoundingBox(accurate: true);
        double width = box.Max.X - box.Min.X;
        if (!box.IsValid || width <= 0.0 || !double.IsFinite(width) || !double.IsFinite(box.Max.Y - box.Min.Y))
            return Fin.Fail<Seq<SurfaceDrive>>(GeometryFault.DegenerateInput("surface-layout:degenerate-raster").ToError());

        int rows = Math.Max(1, (int)Math.Ceiling((box.Max.Y - box.Min.Y) / stepOver)) + 1;
        double[] lower = new double[rows];
        double[] upper = new double[rows];
        double[] fractions = Range(0, rows).Map(row => (double)row / (rows - 1)).ToArray();
        double[] ordinates = new double[rows];
        Array.Fill(lower, box.Min.Y);
        Array.Fill(upper, box.Max.Y);
        TensorPrimitives.Lerp(lower, upper, fractions, ordinates);
        Point3d[] points = new Point3d[rows * 2];
        RasterPointAction action = new(box.Min.X, box.Max.X, ordinates, box.Max.Z, points);
        ParallelHelper.For2D<RasterPointAction>(0, rows, 0, 2, action);
        SurfaceDrive[] drives = new SurfaceDrive[rows];
        for (int row = 0; row < rows; row++)
            drives[row] = new SurfaceDrive(Arr(points[row * 2], points[(row * 2) + 1]), Parameter: ordinates[row]);
        return Fin.Succ(drives.ToSeq());
    }

    // Serpentine parity: even (row+column) starts at MinX, so consecutive drives alternate direction with no per-slot table.
    private readonly struct RasterPointAction(double minX, double maxX, double[] y, double z, Point3d[] points) : IAction2D {
        public void Invoke(int row, int column) =>
            points[(row * 2) + column] = new Point3d((row + column) % 2 == 0 ? minX : maxX, y[row], z);
    }
}

public static class SurfacePath {
    public static Fin<Seq<CutElement>> Sample(SurfaceStrategy strategy, MeshSpace mesh, CutterForm cutter) =>
        strategy is SurfaceStrategy.DrillFamily { Centers.IsEmpty: true }
            ? strategy.Policy.Engagement.Admit(cutter).Map(static _ => Seq<CutElement>())
            : SurfaceRun.Of(strategy, mesh, cutter)
            .Bind(OpenCamLib.Position)
            .Bind(receipt => receipt.Paths.IsEmpty
                ? strategy is SurfaceStrategy.Rest
                    ? Fin.Succ(Seq<CutElement>())
                    : Fin.Fail<Seq<CutElement>>(FabricationFault.SampleStalled(strategy, 0).ToError())
                : strategy.Policy.Engagement.Budget is ProcessBudget.Subtractive budget
                    ? receipt.ToElements(budget.FeedRate)
                    : Fin.Fail<Seq<CutElement>>(GeometryFault.DegenerateInput("surface:non-subtractive-budget").ToError()));
}
```

## [03]-[OPENCAM_BOUNDARY]

- Owner: `OpenCamOperationKind` binds strategy to operation identity; `OpenCamCutterKind` binds cutter form to one verified constructor delegate; `OpenCamNative` declares only the local C-shim ABI; the three `SafeHandle` capsules own native lifetime; `OpenCamLib` executes grouped units.
- Cases: operations `BatchDropCutter`, `PathDropCutter`, `AdaptivePathDropCutter`, `BatchPushCutter`, `Waterline`, `AdaptiveWaterline`; cutters `Cyl`, `Ball`, `Bull`, `Cone`, `Composite`.
- Entry: file-local `OpenCamLib.Position(SurfaceRun)` creates one complete native capsule per independent path/level/fiber or one batch capsule for unordered drill centers. Each capsule performs common setup, operation-specific setup, `run`, and the matching grouped read before disposal.
- Auto: path drives create and retain `OclPathHandle` through execution; waterline levels read loops; push drives read fibers and select X/Y scanning from the drive vector; batch points preserve input/output independence as one singleton element per location, and the returned location census must equal the admitted center census. Count queries bound allocations, and fill results reject negative, excessive, empty, non-finite, or partial rows.
- Receipt: the first nonzero native status routes `SampleStalled` with the exact status and a thrown native boundary outcome enters the same typed rail, so a receipt exists only for all-clean executions and never re-records status. `OpenCamLocation.Contact` retains `CCType` classification as plane-local evidence.
- Assets: `vendor/ocl_shim/ocl_shim.cpp` is the package-owned `extern "C"` body — one shim export per declared `[LibraryImport]` entry point, its status vocabulary the exact integers `Gate` lifts into `SampleStalled`; `vendor/ocl_shim/CMakeLists.txt` is the build owner, linking the shim SHARED against the shipped SHARED `libocl` per the LGPL dynamic-link law; the RID matrix rides `vendor/runtimes/<rid>/native/` — per RID the SHIM artifact the `Library` constant resolves (`win-x64/ocl_shim.dll`, `linux-x64/libocl_shim.so`, `osx-arm64/libocl_shim.dylib`) beside the upstream SHARED `libocl` it links — through the folder `.csproj`'s `Exists`-gated `Content` group.
- Boundary: the upstream library has no C ABI. The shim is the only place allowed to flatten C++ vectors and expose opaque handles; raw handles, C++ mangled entry points, and unmanaged ownership never reach domain code; `libocl` stays dynamically linked and is never folded statically into the shim.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------------------------------------------------------------
using System.Runtime.InteropServices;
using LanguageExt;
using Microsoft.Win32.SafeHandles;
using Rasm.Fabrication.Process;
using Rhino.Geometry;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Toolpath;

// --- [TYPES] --------------------------------------------------------------------------------------------------------------------------------------
[SmartEnum<string>]
internal sealed partial class OpenCamOperationKind {
    public static readonly OpenCamOperationKind BatchDropCutter = new("batch-drop-cutter", 1);
    public static readonly OpenCamOperationKind PathDropCutter = new("path-drop-cutter", 2);
    public static readonly OpenCamOperationKind AdaptivePathDropCutter = new("adaptive-path-drop-cutter", 3);
    public static readonly OpenCamOperationKind BatchPushCutter = new("batch-push-cutter", 4);
    public static readonly OpenCamOperationKind Waterline = new("waterline", 5);
    public static readonly OpenCamOperationKind AdaptiveWaterline = new("adaptive-waterline", 6);

    public int Code { get; }

    public static OpenCamOperationKind Of(SurfaceStrategy strategy, SurfaceSampling sampling) =>
        strategy.Switch(
            waterline:    static row => row.Mode.UsesAdaptiveOperation ? AdaptiveWaterline : Waterline,
            scallop:      _ => sampling.CosLimit < 1.0 ? AdaptivePathDropCutter : PathDropCutter,
            pencil:       _ => sampling.CosLimit < 1.0 ? AdaptivePathDropCutter : PathDropCutter,
            rest:         static _ => PathDropCutter,
            fiberSlice:   static _ => BatchPushCutter,
            threePlusTwo: static _ => PathDropCutter,
            swarf:        static _ => BatchPushCutter,
            drillFamily:  static _ => BatchDropCutter);
}

[SmartEnum<string>]
internal sealed partial class OpenCamCutterKind {
    public static readonly OpenCamCutterKind Cyl = new("cyl", MintCyl);
    public static readonly OpenCamCutterKind Ball = new("ball", MintBall);
    public static readonly OpenCamCutterKind Bull = new("bull", MintBull);
    public static readonly OpenCamCutterKind Cone = new("cone", MintCone);
    public static readonly OpenCamCutterKind Composite = new("composite", MintComposite);

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

    private static OclCutterHandle MintCyl(CutterForm cutter) => OpenCamNative.CutterCyl(cutter.Diameter, cutter.FluteLength);
    private static OclCutterHandle MintBall(CutterForm cutter) => OpenCamNative.CutterBall(cutter.Diameter, cutter.FluteLength);
    private static OclCutterHandle MintBull(CutterForm cutter) => OpenCamNative.CutterBull(cutter.Diameter, cutter.CornerRadius, cutter.FluteLength);
    private static OclCutterHandle MintCone(CutterForm cutter) => OpenCamNative.CutterCone(cutter.Diameter, cutter.TaperAngle, cutter.FluteLength);
    private static OclCutterHandle MintComposite(CutterForm cutter) =>
        OpenCamNative.CutterBullCone(cutter.Diameter, cutter.CornerRadius, cutter.FluteLength, cutter.TaperAngle);
}

// --- [MODELS] -------------------------------------------------------------------------------------------------------------------------------------
internal readonly record struct OpenCamMeshBuffer(double[] Triangles, int TriangleCount) {
    public static Fin<OpenCamMeshBuffer> Project(MeshSpace mesh, int maximumTriangles) {
        Mesh native = mesh.Native;
        long triangleCount = native.Faces.Sum(static face => face.IsQuad ? 2L : 1L);
        if (triangleCount <= 0L || triangleCount > maximumTriangles)
            return Fin.Fail<OpenCamMeshBuffer>(GeometryFault.DegenerateInput("opencam:mesh-capacity").ToError());
        int[] corners = [.. native.Faces.SelectMany(face => face.IsQuad
            ? new[] { face.A, face.B, face.C, face.A, face.C, face.D }
            : new[] { face.A, face.B, face.C })];
        if (corners.Length != triangleCount * 3L || corners.Exists(index => index < 0 || index >= native.Vertices.Count))
            return Fin.Fail<OpenCamMeshBuffer>(GeometryFault.DegenerateInput("opencam:mesh-indices").ToError());

        double[] buffer = new double[corners.Length * 3];
        for (int index = 0; index < corners.Length; index++) {
            Point3f vertex = native.Vertices[corners[index]];
            buffer[index * 3] = vertex.X;
            buffer[(index * 3) + 1] = vertex.Y;
            buffer[(index * 3) + 2] = vertex.Z;
        }
        return buffer.Exists(static value => !double.IsFinite(value))
            ? Fin.Fail<OpenCamMeshBuffer>(GeometryFault.DegenerateInput("opencam:mesh-finite").ToError())
            : Fin.Succ(new OpenCamMeshBuffer(buffer, checked((int)triangleCount)));
    }
}

internal readonly record struct OpenCamLocation(Point3d Location, int Contact);

file delegate int OpenCamGroupFill(
    OclOperationHandle operation,
    int group,
    double[] output,
    int capacity,
    out int written);

internal sealed record SurfaceSampleReceipt(Seq<Arr<OpenCamLocation>> Paths, OpenCamOperationKind Operation) {
    public Fin<Seq<CutElement>> ToElements(double feed) =>
        !ValidTopology()
            ? Fin.Fail<Seq<CutElement>>(GeometryFault.DegenerateInput("opencam:receipt-topology").ToError())
            : feed > 0.0 && double.IsFinite(feed)
                ? Paths.Traverse(path => path.IsEmpty
                ? Fin.Fail<CutElement>(GeometryFault.DegenerateInput("opencam:empty-path").ToError())
                : CutElement.Of(path.Map(point => (Move)new Move.Linear(point.Location, feed)).ToSeq()))
                : Fin.Fail<Seq<CutElement>>(GeometryFault.DegenerateInput("opencam:feed").ToError());

    private bool ValidTopology() =>
        Operation is not null
        && Paths.All(path => Operation == OpenCamOperationKind.BatchDropCutter
            ? path.Count == 1
            : Operation == OpenCamOperationKind.Waterline || Operation == OpenCamOperationKind.AdaptiveWaterline
                ? path.Count >= 3
                : path.Count >= 2);
}

// --- [SERVICES] -----------------------------------------------------------------------------------------------------------------------------------
internal sealed class OclOperationHandle : SafeHandleZeroOrMinusOneIsInvalid {
    public OclOperationHandle() : base(ownsHandle: true) { }
    protected override bool ReleaseHandle() { OpenCamNative.OperationDestroy(handle); return true; }
}

internal sealed class OclCutterHandle : SafeHandleZeroOrMinusOneIsInvalid {
    public OclCutterHandle() : base(ownsHandle: true) { }
    protected override bool ReleaseHandle() { OpenCamNative.CutterDestroy(handle); return true; }
}

internal sealed class OclPathHandle : SafeHandleZeroOrMinusOneIsInvalid {
    public OclPathHandle() : base(ownsHandle: true) { }
    protected override bool ReleaseHandle() { OpenCamNative.PathDestroy(handle); return true; }
}

// Library resolves the SHIM artifact (ocl_shim.dll / libocl_shim.{so,dylib}); the upstream SHARED libocl rides
// beside it and stays a separate dynamically-linked archive.
internal static partial class OpenCamNative {
    internal const string Library = "ocl_shim";

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
    [LibraryImport(Library, EntryPoint = "ocl_op_cl_count")]
    internal static partial int OperationClCount(OclOperationHandle operation);
    [LibraryImport(Library, EntryPoint = "ocl_op_get_clpoints")]
    internal static partial int OperationGetClPoints(OclOperationHandle operation, double[] output, int capacity, out int written);
    [LibraryImport(Library, EntryPoint = "ocl_op_loop_count")]
    internal static partial int OperationLoopCount(OclOperationHandle operation);
    [LibraryImport(Library, EntryPoint = "ocl_op_loop_point_count")]
    internal static partial int OperationLoopPointCount(OclOperationHandle operation, int loop);
    [LibraryImport(Library, EntryPoint = "ocl_op_get_loop")]
    internal static partial int OperationGetLoop(OclOperationHandle operation, int loop, double[] output, int capacity, out int written);
    [LibraryImport(Library, EntryPoint = "ocl_op_fiber_count")]
    internal static partial int OperationFiberCount(OclOperationHandle operation);
    [LibraryImport(Library, EntryPoint = "ocl_op_fiber_point_count")]
    internal static partial int OperationFiberPointCount(OclOperationHandle operation, int fiber);
    [LibraryImport(Library, EntryPoint = "ocl_op_get_fiber")]
    internal static partial int OperationGetFiber(OclOperationHandle operation, int fiber, double[] output, int capacity, out int written);
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
    internal static Fin<SurfaceSampleReceipt> Position(SurfaceRun run) =>
        Try.lift<Fin<SurfaceSampleReceipt>>(() => PositionNative(run)).Run()
            .MapFail(_ => FabricationFault.SampleStalled(run.Strategy, -3).ToError())
            .Bind(identity);

    private static Fin<SurfaceSampleReceipt> PositionNative(SurfaceRun run) =>
        OpenCamMeshBuffer.Project(run.Mesh, run.Sampling.MaximumTriangles).Bind(mesh =>
            run.Strategy.Switch(
                waterline: row => row.Levels.Traverse(level => Unit(
                        run,
                        mesh,
                        operation => Gate(run, () => OpenCamNative.OperationSetZ(operation, level)),
                        operation => ReadGroups(operation, run, OpenCamNative.OperationLoopCount, OpenCamNative.OperationLoopPointCount, OpenCamNative.OperationGetLoop)))
                    .Map(groups => Receipt(run, groups.Bind(identity))),
                scallop:      _ => Paths(run, mesh),
                pencil:       _ => Paths(run, mesh),
                rest:         _ => Paths(run, mesh),
                fiberSlice:   _ => Fibers(run, mesh),
                threePlusTwo: static _ => Fin.Fail<SurfaceSampleReceipt>(GeometryFault.DegenerateInput("opencam:tool-axis").ToError()),
                swarf:        static _ => Fin.Fail<SurfaceSampleReceipt>(GeometryFault.DegenerateInput("opencam:tool-axis").ToError()),
                drillFamily:  row => Points(run, mesh, row.Centers)));

    private static Fin<SurfaceSampleReceipt> Paths(SurfaceRun run, OpenCamMeshBuffer mesh) =>
        run.Drives.Match(
            None: () => Fin.Fail<SurfaceSampleReceipt>(GeometryFault.DegenerateInput("opencam:path-without-drives").ToError()),
            Some: set => set.Drives.Traverse(drive => Path(run, mesh, drive)).Map(groups => Receipt(
                run,
                run.Strategy is SurfaceStrategy.Rest ? groups.Filter(static group => !group.IsEmpty) : groups)));

    private static Fin<Arr<OpenCamLocation>> Path(SurfaceRun run, OpenCamMeshBuffer mesh, SurfaceDrive drive) {
        using OclPathHandle path = OpenCamNative.PathCreate();
        return path.IsInvalid || drive.Points.Count < 2
            ? Fin.Fail<Arr<OpenCamLocation>>(GeometryFault.DegenerateInput("opencam:path").ToError())
            : Range(0, drive.Points.Count - 1).Fold(
                Fin.Succ(0),
                (state, index) => state.Bind(_ => Gate(run, () => OpenCamNative.PathAppendLine(
                    path,
                    drive.Points[index].X, drive.Points[index].Y, drive.Points[index].Z,
                    drive.Points[index + 1].X, drive.Points[index + 1].Y, drive.Points[index + 1].Z))))
              .Bind(_ => Unit(
                  run,
                  mesh,
                  operation => Gate(run, () => OpenCamNative.OperationSetPath(operation, path)),
                  operation => ReadLocations(operation, run)));
    }

    private static Fin<SurfaceSampleReceipt> Fibers(SurfaceRun run, OpenCamMeshBuffer mesh) =>
        run.Drives.Match(
            None: () => Fin.Fail<SurfaceSampleReceipt>(GeometryFault.DegenerateInput("opencam:fiber-without-drives").ToError()),
            Some: set => set.Drives.Traverse(drive => Unit(
                    run,
                    mesh,
                    operation => {
                        Point3d from = drive.Points[0];
                        Point3d to = drive.Points[drive.Points.Count - 1];
                        Func<int> direction = Math.Abs(to.X - from.X) >= Math.Abs(to.Y - from.Y)
                            ? () => OpenCamNative.OperationSetXDirection(operation)
                            : () => OpenCamNative.OperationSetYDirection(operation);
                        return Gate(
                            run,
                            () => OpenCamNative.OperationAppendFiber(operation, from.X, from.Y, from.Z, to.X, to.Y, to.Z),
                            direction);
                    },
                    operation => ReadGroups(operation, run, OpenCamNative.OperationFiberCount, OpenCamNative.OperationFiberPointCount, OpenCamNative.OperationGetFiber)))
                .Map(groups => Receipt(run, groups.Bind(identity))));

    private static Fin<SurfaceSampleReceipt> Points(SurfaceRun run, OpenCamMeshBuffer mesh, Arr<Point3d> centers) =>
        centers.IsEmpty
            ? Fin.Fail<SurfaceSampleReceipt>(GeometryFault.DegenerateInput("opencam:points-empty").ToError())
            : Unit(
                run,
                mesh,
                operation => centers.Fold(
                    Fin.Succ(0),
                    (state, point) => state.Bind(_ => Gate(run, () => OpenCamNative.OperationAppendPoint(operation, point.X, point.Y, point.Z)))),
                operation => ReadLocations(operation, run))
              .Bind(rows => rows.Count == centers.Count
                  ? Fin.Succ(Receipt(run, rows.Map(static row => Arr(row)).ToSeq()))
                  : Fin.Fail<SurfaceSampleReceipt>(FabricationFault.SampleStalled(run.Strategy, rows.Count).ToError()));

    private static Fin<T> Unit<T>(
        SurfaceRun run,
        OpenCamMeshBuffer mesh,
        Func<OclOperationHandle, Fin<int>> configure,
        Func<OclOperationHandle, Fin<T>> read) {
        using OclOperationHandle operation = OpenCamNative.OperationCreate(run.Operation.Code);
        using OclCutterHandle cutter = run.CutterKind.Mint(run.Cutter);
        return operation.IsInvalid || cutter.IsInvalid
            ? Fin.Fail<T>(FabricationFault.SampleStalled(run.Strategy, -1).ToError())
            : Gate(
                run,
                () => OpenCamNative.OperationSetStl(operation, mesh.Triangles, mesh.TriangleCount),
                () => OpenCamNative.OperationSetCutter(operation, cutter),
                () => OpenCamNative.OperationSetSampling(operation, run.Sampling.MaximumStepMm),
                () => OpenCamNative.OperationSetMinSampling(operation, run.Sampling.MinimumStepMm),
                () => OpenCamNative.OperationSetCosLimit(operation, run.Sampling.CosLimit),
                () => OpenCamNative.OperationSetThreads(operation, run.Sampling.Threads))
              .Bind(_ => configure(operation))
              .Bind(_ => Gate(run, () => OpenCamNative.OperationRun(operation)))
              .Bind(_ => read(operation));
    }

    private static Fin<Arr<OpenCamLocation>> ReadLocations(OclOperationHandle operation, SurfaceRun run) {
        int count = OpenCamNative.OperationClCount(operation);
        return Count(run, count, minimum: 0, maximum: run.Sampling.MaximumPointsPerGroup).Bind(valid => {
            if (valid == 0)
                return Fin.Succ(Arr<OpenCamLocation>.Empty);
            double[] output = new double[valid * 4];
            int written = 0;
            return Gate(run, () => OpenCamNative.OperationGetClPoints(operation, output, output.Length, out written))
                .Bind(_ => Written(run, written, valid))
                .Bind(_ => Decode(run, output, valid));
        });
    }

    private static Fin<Seq<Arr<OpenCamLocation>>> ReadGroups(
        OclOperationHandle operation,
        SurfaceRun run,
        Func<OclOperationHandle, int> groupCount,
        Func<OclOperationHandle, int, int> pointCount,
        OpenCamGroupFill fill) =>
        Count(run, groupCount(operation), minimum: 0, maximum: run.Sampling.MaximumGroups).Bind(groups =>
            Range(0, groups).Traverse(group => Count(
                run,
                pointCount(operation, group),
                minimum: 1,
                maximum: run.Sampling.MaximumPointsPerGroup).Bind(points => {
                double[] output = new double[points * 4];
                int written = 0;
                return Gate(run, () => fill(operation, group, output, output.Length, out written))
                    .Bind(_ => Written(run, written, points))
                    .Bind(_ => Decode(run, output, points));
            })));

    private static Fin<int> Count(SurfaceRun run, int count, int minimum, int maximum) =>
        count >= minimum && count <= maximum
            ? Fin.Succ(count)
            : Fin.Fail<int>(FabricationFault.SampleStalled(run.Strategy, count).ToError());

    private static Fin<int> Written(SurfaceRun run, int written, int expected) =>
        written == expected
            ? Fin.Succ(written)
            : Fin.Fail<int>(FabricationFault.SampleStalled(run.Strategy, written).ToError());

    private static Fin<Arr<OpenCamLocation>> Decode(SurfaceRun run, double[] output, int count) =>
        output.Length == count * 4
        && output.All(static value => double.IsFinite(value))
        && Range(0, count).All(index => {
            double contact = output[(index * 4) + 3];
            return contact >= 0.0 && contact <= int.MaxValue && contact == Math.Truncate(contact);
        })
            ? Fin.Succ(Range(0, count).Map(index => new OpenCamLocation(
                new Point3d(output[index * 4], output[(index * 4) + 1], output[(index * 4) + 2]),
                (int)output[(index * 4) + 3])).ToArr())
            : Fin.Fail<Arr<OpenCamLocation>>(FabricationFault.SampleStalled(run.Strategy, -2).ToError());

    private static SurfaceSampleReceipt Receipt(SurfaceRun run, Seq<Arr<OpenCamLocation>> paths) =>
        new(paths, run.Operation);

    private static Fin<int> Gate(SurfaceRun run, params ReadOnlySpan<Func<int>> steps) {
        foreach (Func<int> step in steps) {
            int status = step();
            if (status != 0)
                return Fin.Fail<int>(FabricationFault.SampleStalled(run.Strategy, status).ToError());
        }
        return Fin.Succ(0);
    }
}
```

```cpp signature
// vendor/ocl_shim/ocl_shim.cpp — the ONE extern "C" boundary over the C++-mangled libocl surface. One opaque
// operation struct owns kind, borrowed cutter/path handles, seeds, policy, and the cached grouped result; Run
// dispatches on kind against the shared ocl::Operation lifecycle; every export returns 0 on success or a
// negative status the managed Gate lifts into SampleStalled, and no exception crosses the ABI (trap → -9).
// The include roster and member spellings mirror the Boost.Python binding blueprint (ocl_cutters.cpp /
// ocl_dropcutter.cpp / ocl_algo.cpp / ocl_geometry.cpp); libocl links SHARED, never folded statically.
#include <array>
#include <vector>
#include <opencamlib/stlsurf.hpp>
#include <opencamlib/triangle.hpp>
#include <opencamlib/point.hpp>
#include <opencamlib/clpoint.hpp>
#include <opencamlib/fiber.hpp>
#include <opencamlib/path.hpp>
#include <opencamlib/millingcutter.hpp>
#include <opencamlib/cylcutter.hpp>
#include <opencamlib/ballcutter.hpp>
#include <opencamlib/bullcutter.hpp>
#include <opencamlib/conecutter.hpp>
#include <opencamlib/bullconecutter.hpp>
#include <opencamlib/batchdropcutter.hpp>
#include <opencamlib/pathdropcutter.hpp>
#include <opencamlib/adaptivepathdropcutter.hpp>
#include <opencamlib/batchpushcutter.hpp>
#include <opencamlib/waterline.hpp>
#include <opencamlib/adaptivewaterline.hpp>

#if defined(_WIN32)
  #define OCL_SHIM_EXPORT extern "C" __declspec(dllexport)
#else
  #define OCL_SHIM_EXPORT extern "C" __attribute__((visibility("default")))
#endif

namespace {

constexpr int kOk = 0, kBadHandle = -1, kBadBuffer = -2, kBadState = -4, kTrapped = -9;

using Row = std::array<double, 4>;                                  // x, y, z, CCType ordinal — the managed 4-wide decode row

struct OclShimOperation {
    int kind = 0;                                                   // the OpenCamOperationKind codes 1..6
    ocl::STLSurf surface;
    ocl::MillingCutter* cutter = nullptr;                           // borrowed — ocl_cutter_destroy owns release
    ocl::Path* path = nullptr;                                      // borrowed — ocl_path_destroy owns release
    double sampling = 0.0, minSampling = 0.0, cosLimit = 1.0, z = 0.0;
    int threads = 1;
    bool yDirection = false;
    std::vector<ocl::CLPoint> seeds;                                // BatchDropCutter inputs
    std::vector<ocl::Fiber> fibers;                                 // BatchPushCutter inputs
    std::vector<std::vector<Row>> groups;                           // flat CL rides groups[0]; loops/fibers one group each
};

OclShimOperation* Op(void* handle) { return static_cast<OclShimOperation*>(handle); }

template <typename Body>
int Trap(void* handle, Body body) {
    if (handle == nullptr) return kBadHandle;
    try { return body(*Op(handle)); } catch (...) { return kTrapped; }
}

Row Of(ocl::CLPoint& cl) { return {cl.x, cl.y, cl.z, static_cast<double>(cl.getCC().type)}; }

int Fill(const std::vector<Row>& rows, double* output, int capacity, int* written) {
    if (output == nullptr || capacity < static_cast<int>(rows.size()) * 4) return kBadBuffer;
    for (size_t row = 0; row < rows.size(); ++row)
        for (size_t slot = 0; slot < 4; ++slot)
            output[(row * 4) + slot] = rows[row][slot];
    *written = static_cast<int>(rows.size());
    return kOk;
}

// One dispatch per operation kind over the shared lifecycle: setSTL · setCutter · policy · run · grouped read.
int Run(OclShimOperation& op) {
    if (op.cutter == nullptr) return kBadState;
    op.groups.clear();
    switch (op.kind) {
        case 1: {                                                   // BatchDropCutter — unordered CL cloud, one flat group
            ocl::BatchDropCutter unit;
            unit.setSTL(op.surface); unit.setCutter(op.cutter); unit.setThreads(op.threads);
            for (ocl::CLPoint& seed : op.seeds) unit.appendPoint(seed);
            unit.run();
            op.groups.emplace_back();
            for (ocl::CLPoint& cl : *unit.getCLPoints()) op.groups[0].push_back(Of(cl));
            return kOk;
        }
        case 2: case 3: {                                           // PathDropCutter / AdaptivePathDropCutter — one flat group per bound path
            if (op.path == nullptr) return kBadState;
            if (op.kind == 2) {
                ocl::PathDropCutter unit;
                unit.setSTL(op.surface); unit.setCutter(op.cutter); unit.setSampling(op.sampling); unit.setZ(op.z);
                unit.setPath(op.path);
                unit.run();
                op.groups.emplace_back();
                for (ocl::CLPoint& cl : unit.getPoints()) op.groups[0].push_back(Of(cl));
                return kOk;
            }
            ocl::AdaptivePathDropCutter unit;
            unit.setSTL(op.surface); unit.setCutter(op.cutter); unit.setSampling(op.sampling);
            unit.setMinSampling(op.minSampling); unit.setCosLimit(op.cosLimit); unit.setZ(op.z);
            unit.setPath(op.path);
            unit.run();
            op.groups.emplace_back();
            for (ocl::CLPoint& cl : unit.getPoints()) op.groups[0].push_back(Of(cl));
            return kOk;
        }
        case 4: {                                                   // BatchPushCutter — one group per fiber, interval endpoints as rows
            ocl::BatchPushCutter unit;
            unit.setSTL(op.surface); unit.setCutter(op.cutter); unit.setThreads(op.threads);
            if (op.yDirection) unit.setYDirection(); else unit.setXDirection();
            for (ocl::Fiber& fiber : op.fibers) unit.appendFiber(fiber);
            unit.run();
            for (ocl::Fiber& fiber : *unit.getFibers()) {
                std::vector<Row> group;
                for (ocl::Interval& interval : fiber.ints) {
                    ocl::Point lower = fiber.point(interval.lower);
                    ocl::Point upper = fiber.point(interval.upper);
                    group.push_back({lower.x, lower.y, lower.z, 0.0});
                    group.push_back({upper.x, upper.y, upper.z, 0.0});
                }
                if (!group.empty()) op.groups.push_back(group);
            }
            return kOk;
        }
        case 5: case 6: {                                           // Waterline / AdaptiveWaterline — one group per closed loop
            if (op.kind == 5) {
                ocl::Waterline unit;
                unit.setSTL(op.surface); unit.setCutter(op.cutter); unit.setSampling(op.sampling); unit.setZ(op.z);
                unit.run();
                for (auto& loop : unit.getLoops()) {
                    std::vector<Row> group;
                    for (ocl::Point& point : loop) group.push_back({point.x, point.y, point.z, 0.0});
                    op.groups.push_back(group);
                }
                return kOk;
            }
            ocl::AdaptiveWaterline unit;
            unit.setSTL(op.surface); unit.setCutter(op.cutter); unit.setSampling(op.sampling);
            unit.setMinSampling(op.minSampling); unit.setZ(op.z);
            unit.run();
            for (auto& loop : unit.getLoops()) {
                std::vector<Row> group;
                for (ocl::Point& point : loop) group.push_back({point.x, point.y, point.z, 0.0});
                op.groups.push_back(group);
            }
            return kOk;
        }
        default: return kBadState;
    }
}

}  // namespace

OCL_SHIM_EXPORT void* ocl_op_create(int operation) {
    return operation >= 1 && operation <= 6 ? new OclShimOperation{operation} : nullptr;
}
OCL_SHIM_EXPORT int ocl_op_set_stl(void* op, const double* triangles, int triangleCount) {
    return Trap(op, [&](OclShimOperation& unit) {
        if (triangles == nullptr || triangleCount <= 0) return kBadBuffer;
        for (int index = 0; index < triangleCount; ++index) {
            const double* t = triangles + (index * 9);
            unit.surface.addTriangle(ocl::Triangle(
                ocl::Point(t[0], t[1], t[2]), ocl::Point(t[3], t[4], t[5]), ocl::Point(t[6], t[7], t[8])));
        }
        return kOk;
    });
}
OCL_SHIM_EXPORT int ocl_op_set_cutter(void* op, void* cutter) {
    return Trap(op, [&](OclShimOperation& unit) {
        if (cutter == nullptr) return kBadHandle;
        unit.cutter = static_cast<ocl::MillingCutter*>(cutter);
        return kOk;
    });
}
OCL_SHIM_EXPORT int ocl_op_set_sampling(void* op, double sampling) {
    return Trap(op, [&](OclShimOperation& unit) { unit.sampling = sampling; return kOk; });
}
OCL_SHIM_EXPORT int ocl_op_set_min_sampling(void* op, double sampling) {
    return Trap(op, [&](OclShimOperation& unit) { unit.minSampling = sampling; return kOk; });
}
OCL_SHIM_EXPORT int ocl_op_set_cos_limit(void* op, double cosLimit) {
    return Trap(op, [&](OclShimOperation& unit) { unit.cosLimit = cosLimit; return kOk; });
}
OCL_SHIM_EXPORT int ocl_op_set_threads(void* op, int threads) {
    return Trap(op, [&](OclShimOperation& unit) { unit.threads = threads > 0 ? threads : 1; return kOk; });
}
OCL_SHIM_EXPORT int ocl_op_set_z(void* op, double z) {
    return Trap(op, [&](OclShimOperation& unit) { unit.z = z; return kOk; });
}
OCL_SHIM_EXPORT int ocl_op_append_point(void* op, double x, double y, double z) {
    return Trap(op, [&](OclShimOperation& unit) { unit.seeds.emplace_back(x, y, z); return kOk; });
}
OCL_SHIM_EXPORT int ocl_op_set_path(void* op, void* path) {
    return Trap(op, [&](OclShimOperation& unit) {
        if (path == nullptr) return kBadHandle;
        unit.path = static_cast<ocl::Path*>(path);
        return kOk;
    });
}
OCL_SHIM_EXPORT int ocl_op_append_fiber(void* op, double x1, double y1, double z1, double x2, double y2, double z2) {
    return Trap(op, [&](OclShimOperation& unit) {
        unit.fibers.emplace_back(ocl::Point(x1, y1, z1), ocl::Point(x2, y2, z2));
        return kOk;
    });
}
OCL_SHIM_EXPORT int ocl_op_set_x_direction(void* op) {
    return Trap(op, [&](OclShimOperation& unit) { unit.yDirection = false; return kOk; });
}
OCL_SHIM_EXPORT int ocl_op_set_y_direction(void* op) {
    return Trap(op, [&](OclShimOperation& unit) { unit.yDirection = true; return kOk; });
}
OCL_SHIM_EXPORT int ocl_op_run(void* op) {
    return Trap(op, [&](OclShimOperation& unit) { return Run(unit); });
}
OCL_SHIM_EXPORT int ocl_op_cl_count(void* op) {
    return op == nullptr || Op(op)->groups.empty() ? 0 : static_cast<int>(Op(op)->groups[0].size());
}
OCL_SHIM_EXPORT int ocl_op_get_clpoints(void* op, double* output, int capacity, int* written) {
    return Trap(op, [&](OclShimOperation& unit) {
        return unit.groups.empty() ? kBadState : Fill(unit.groups[0], output, capacity, written);
    });
}
OCL_SHIM_EXPORT int ocl_op_loop_count(void* op) {
    return op == nullptr ? 0 : static_cast<int>(Op(op)->groups.size());
}
OCL_SHIM_EXPORT int ocl_op_loop_point_count(void* op, int loop) {
    return op == nullptr || loop < 0 || loop >= static_cast<int>(Op(op)->groups.size())
        ? 0 : static_cast<int>(Op(op)->groups[loop].size());
}
OCL_SHIM_EXPORT int ocl_op_get_loop(void* op, int loop, double* output, int capacity, int* written) {
    return Trap(op, [&](OclShimOperation& unit) {
        return loop < 0 || loop >= static_cast<int>(unit.groups.size())
            ? kBadState : Fill(unit.groups[loop], output, capacity, written);
    });
}
OCL_SHIM_EXPORT int ocl_op_fiber_count(void* op) { return ocl_op_loop_count(op); }
OCL_SHIM_EXPORT int ocl_op_fiber_point_count(void* op, int fiber) { return ocl_op_loop_point_count(op, fiber); }
OCL_SHIM_EXPORT int ocl_op_get_fiber(void* op, int fiber, double* output, int capacity, int* written) {
    return ocl_op_get_loop(op, fiber, output, capacity, written);
}
OCL_SHIM_EXPORT void ocl_op_destroy(void* op) { delete Op(op); }
OCL_SHIM_EXPORT void* ocl_path_create() { return new ocl::Path(); }
OCL_SHIM_EXPORT int ocl_path_append_line(void* path, double x1, double y1, double z1, double x2, double y2, double z2) {
    if (path == nullptr) return kBadHandle;
    try {
        static_cast<ocl::Path*>(path)->append(ocl::Line(ocl::Point(x1, y1, z1), ocl::Point(x2, y2, z2)));
        return kOk;
    } catch (...) { return kTrapped; }
}
OCL_SHIM_EXPORT void ocl_path_destroy(void* path) { delete static_cast<ocl::Path*>(path); }
OCL_SHIM_EXPORT void* ocl_cutter_cyl(double diameter, double length) { return new ocl::CylCutter(diameter, length); }
OCL_SHIM_EXPORT void* ocl_cutter_ball(double diameter, double length) { return new ocl::BallCutter(diameter, length); }
OCL_SHIM_EXPORT void* ocl_cutter_bull(double diameter, double radius, double length) { return new ocl::BullCutter(diameter, radius, length); }
OCL_SHIM_EXPORT void* ocl_cutter_cone(double diameter, double angle, double length) { return new ocl::ConeCutter(diameter, angle, length); }
OCL_SHIM_EXPORT void* ocl_cutter_bullcone(double diameter, double radius, double length, double angle) {
    return new ocl::BullConeCutter(diameter, radius, length, angle);
}
OCL_SHIM_EXPORT void ocl_cutter_destroy(void* cutter) { delete static_cast<ocl::MillingCutter*>(cutter); }
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
  accTitle: Surface sampling ownership
  accDescr: Strategy-specific layouts preserve independent drives, levels, and centers through native operation capsules until each output group becomes one routable cutting element.
  Strategy["SurfaceStrategy + policy"] --> Layout["SurfaceLayout.Produce"]
  LayoutKind["SurfaceLayoutKind structural value"] --> Layout
  Layout --> Drives["independent SurfaceDrive rows"]
  Drives --> Capsule["one native capsule per drive/fiber"]
  Levels["waterline levels"] --> Capsule
  Centers["drill centers"] --> Batch["one BatchDropCutter capsule"]
  Capsule --> Grouped["grouped CL paths · loops · fibers"]
  Batch --> Grouped
  Grouped --> Elements["one group → one CutElement"]
  Elements --> Link["Link.Route owns travel"]
  classDef primary fill:#44475A,stroke:#FF79C6,color:#F8F8F2
  classDef boundary fill:#282A36,stroke:#BD93F9,color:#F8F8F2
  classDef external fill:#8BE9FDBF,stroke:#8BE9FD,color:#282A36
  classDef data fill:#FFB86CBF,stroke:#FFB86C,color:#282A36
  class Strategy,LayoutKind,Levels,Centers boundary
  class Layout,Drives,Elements,Link primary
  class Capsule,Batch external
  class Grouped data
```
