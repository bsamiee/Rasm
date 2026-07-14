# [RASM_FABRICATION_IMPLICIT]

The implicit additive owner is ONE `Implicit.Voxelize` rail over the PicoGK sidecar-native kernel. `ImplicitOp.Field` composes a delegate-owned `FieldKind`, a payload-correct `TpmsForm`, `ImplicitCell`, the envelope wire, and policy. Six named `FieldKind` seeds cover established periodic families, while `FieldKind.Generated` admits any collision-free keyed SDF delegate through the same calibration and raster rail. `FieldCalibration` samples one periodic cell and maps local relative density through exact ordered quantiles after `TensorPrimitives.Min`/`Max` finite-range reduction; solid morphology owns no dead wall knob, while sheet morphology carries its physical minimum wall. `ScalarField` and `VectorField` leases optionally drive graded density and anisotropic axes at world samples, with the constant cell values as total fallbacks. `Voxels` remains a plane-local native lease, and canonical `.cli`/mask bytes are explicitly little-endian and payload-complete. `CliMode.VdbCli` is the voxel-free egress row: the provider-native `Vdb2Cli` bridge converts VDB to `.cli` with zero native materialization, so no unrelated allocation, morphology, or budget failure can block it.

Wire posture: HOST-LOCAL. `Voxels` never crosses a result case, browser wire, or peer boundary; `ContentKey` and layer counts are the only egress payloads.

## [01]-[INDEX]

- [01]-[IMPLICIT]: owns `ImplicitOp`, `FieldKind`, `TpmsForm`, `ImplicitCell`, the payload-bearing `VoxelMorphologyStep` union, `CliMode`, `ImplicitPolicy`, `VoxelWire`, `CliStack`, the `VoxelRuntime` `Library.GlobalInstance` owner, `PeriodicImplicit`, `ImplicitCanonical`, and the two public rails `Implicit.Voxelize(ImplicitOp)` plus `Implicit.Cli(ImplicitOp)` over PicoGK `IImplicit`/`Voxels`/`Lattice`/`ScalarField`/`VectorField`/`CliIo`/`Vdb2Cli`.

## [02]-[IMPLICIT]

- Owner: `ImplicitOp` the additive voxel discriminant; `FieldKind` the delegate-backed periodic-field vocabulary; `TpmsForm` the payload-bearing solid/sheet union; `ImplicitCell` the period, relative-density, and axis carrier; `FieldCalibration` the density-to-threshold operation; `VoxelMorphologyStep` the payload-bearing native morphology union; `CliMode` the payload-bearing egress union; `ImplicitPolicy`, `VoxelWire`, and `CliStack` the policy, wire, and receipt; `Implicit` the static surface.
- Cases: `Field(FieldKind, TpmsForm, ImplicitCell, VoxelWire, Morphology, ImplicitPolicy)` rasterizes the row-owned SDF with `new Voxels(IImplicit, BBox3)` and intersects the envelope; `FieldKind` carries six named seeds plus the parameterized `Generated(key, sample)` admission, so one case spans the complete topology vocabulary. `LatticeSupport` accepts any `SupportPlan` with nonempty `TreeNodes`, lowers every parent edge to `Lattice.AddBeam`, optionally subtracts the part, and applies its own morphology; `Source` covers imported or prebuilt voxel wires and applies its own morphology before egress. `VoxelMorphologyStep` cases 7 — `Offset`, `Shell`, `OverOffset`, `Smoothen`, `Fillet`, `DoubleOffset`, and `TripleOffset` — each case carries only the operands its verified PicoGK verb reads; `Shell` swaps the lease through the copy-only `voxShell`. `CliMode` cases 3 — `Grayscale`, `CliVector(Option<string> Path)`, and `VdbCli(VdbCliRequest)` — so mode-specific filesystem payloads cannot drift as global optional knobs; `VdbCli` composes only with a morphology-free `Source` and never takes a voxel lease.
- Entry: `public static Fin<Voxels> Voxelize(ImplicitOp op)` and `public static Fin<CliStack> Cli(ImplicitOp op)` — the first owns all voxel-producing cases and routes `FabricationFault.VoxelFault(op, budget)` 2715; the second routes the policy's mode row: leased modes voxelize under the full budget guard and dispose on every exit, while `VdbCli` reaches `Vdb2Cli.Convert` with zero voxel materialization — both lowering only admitted provider and filesystem boundary failures.
- Auto: `Voxelize` validates the budget and operation payload, binds one process-lifetime PicoGK ambient resolution, acquires optional `ScalarField` and `VectorField` leases, realizes the envelope, and releases every intermediate on both rails. `PeriodicImplicit` transforms through the inverse sampled-axis frame and consumes `FieldCalibration`: solid uses the sampled relative-density quantile as its isovalue, while sheet uses the density quantile of absolute deviations subject to its physical minimum wall. The budget bounds derive the PicoGK `BBox3`; no duplicate bounds knob can drift. Every mask scalar writes little-endian bytes rather than host-endian memory.
- Receipt: `CliStack` carries the vector-layer count, the `.cli` `ContentKey`, and grayscale mask keys; `VoxelWire` carries the mesh/voxel content key and conversion functions; `FabricationResult.AdditiveResult` receives only moves, layer count, and `Seq<ContentKey>`, never `Voxels`, `Lattice`, `Mesh`, `OpenVdbFile`, or `Library`.
- Packages: `api-picogk.md` (`IImplicit`, `IBoundedImplicit`, `Voxels`, `Lattice`, `ScalarField.bGetValue`, `VectorField.bGetValue`, `PolySliceStack`, `ImageGrayScale`, `CliIo`, `Vdb2Cli`, `Library.GlobalInstance`, `BBox3`), `Process/owner`, `Process/faults`, `Additive/support`, CommunityToolkit.HighPerformance (`ArrayPoolBufferWriter<byte>`), System.Numerics.Tensors (`TensorPrimitives.Min`/`Max`), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: an established periodic topology may become one named `FieldKind` seed, while experimental, imported, or generative topology is one `FieldKind.Generated(key, sample)` value over the same calibration rail; a new solid/sheet morphology is one payload-correct `TpmsForm` case; a graded-density or anisotropic-axis law is an `ImplicitCell` field factory, not a multiplied topology case; a new native voxel operation is one payload-correct `VoxelMorphologyStep` case on the existing fold; a new egress encoding is one payload-correct `CliMode` case; zero new entrypoint.
- Boundary: PicoGK's `net9.0` vendor assembly remains inside the `net10.0` companion-side native ALC firebreak and a plugin-domain reference is the deleted form; `Voxels` is plane-local and a result payload is the named ruling-5 defect; a content key minted over metadata alone — a layer index without its raster, a Z position without its contours — is the byte-identity defect this page's canonical folds foreclose; the content-keyed mesh-to-voxel seam is the only Verify/removal bridge; `ContentKey.Of(EgressKind.Cli, ...)` is the only `.cli`/mask identity mint; `Support` owns tree search and planar support algebra; `Scan` owns LPBF metal vectors; this page owns resin/powder voxel grayscale and `.cli` stacks.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------------------------------------------------------------
using System.Numerics;
using System.Numerics.Tensors;
using System.Threading;
using CommunityToolkit.HighPerformance.Buffers;
using LanguageExt;
using LanguageExt.Common;
using PicoGK;
using Rasm.Fabrication.Process;
using Rasm.Numerics;
using Rhino.Geometry;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Additive;

// --- [TYPES] --------------------------------------------------------------------------------------------------------------------------------------
// Each row OWNS its periodic signed-distance law; a new lattice topology is one row, never an if-chain arm.
[SmartEnum<string>]
public sealed partial class FieldKind {
    public static readonly FieldKind Gyroid = new(
        "gyroid",
        static v => MathF.Sin(v.X) * MathF.Cos(v.Y) + MathF.Sin(v.Y) * MathF.Cos(v.Z) + MathF.Sin(v.Z) * MathF.Cos(v.X));

    public static readonly FieldKind SchwarzP = new(
        "schwarz-p",
        static v => MathF.Cos(v.X) + MathF.Cos(v.Y) + MathF.Cos(v.Z));

    public static readonly FieldKind SchwarzD = new(
        "schwarz-d",
        static v => MathF.Sin(v.X) * MathF.Sin(v.Y) * MathF.Sin(v.Z)
            + MathF.Sin(v.X) * MathF.Cos(v.Y) * MathF.Cos(v.Z)
            + MathF.Cos(v.X) * MathF.Sin(v.Y) * MathF.Cos(v.Z)
            + MathF.Cos(v.X) * MathF.Cos(v.Y) * MathF.Sin(v.Z));

    public static readonly FieldKind Neovius = new(
        "neovius",
        static v => 3.0f * (MathF.Cos(v.X) + MathF.Cos(v.Y) + MathF.Cos(v.Z))
            + 4.0f * MathF.Cos(v.X) * MathF.Cos(v.Y) * MathF.Cos(v.Z));

    public static readonly FieldKind Lidinoid = new(
        "lidinoid",
        static v => 0.5f * (MathF.Sin(2.0f * v.X) * MathF.Cos(v.Y) * MathF.Sin(v.Z)
                + MathF.Sin(2.0f * v.Y) * MathF.Cos(v.Z) * MathF.Sin(v.X)
                + MathF.Sin(2.0f * v.Z) * MathF.Cos(v.X) * MathF.Sin(v.Y))
            - 0.5f * (MathF.Cos(2.0f * v.X) * MathF.Cos(2.0f * v.Y)
                + MathF.Cos(2.0f * v.Y) * MathF.Cos(2.0f * v.Z)
                + MathF.Cos(2.0f * v.Z) * MathF.Cos(2.0f * v.X))
            + 0.15f);

    public static readonly FieldKind Cellular = new(
        "cellular",
        static v => MathF.Min(
            MathF.Min(MathF.Abs(MathF.Sin(v.X) * MathF.Sin(v.Y)), MathF.Abs(MathF.Sin(v.Y) * MathF.Sin(v.Z))),
            MathF.Abs(MathF.Sin(v.Z) * MathF.Sin(v.X))));

    public static Fin<FieldKind> Generated(string key, Func<Vector3, float> sample) =>
        !string.IsNullOrWhiteSpace(key)
        && sample is not null
        && !Seq(Gyroid, SchwarzP, SchwarzD, Neovius, Lidinoid, Cellular)
            .Exists(seed => string.Equals(seed.Key, key, StringComparison.Ordinal))
            ? Fin.Succ(new FieldKind(key, sample))
            : Fin.Fail<FieldKind>(GeometryFault.DegenerateInput("implicit-field:invalid-generated-key-or-sample").ToError());

    [UseDelegateFromConstructor]
    public partial float Sample(Vector3 scaled);
}

// Solid carries no wall knob; Sheet carries the physical minimum wall while relative density calibrates the field threshold.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TpmsForm {
    private TpmsForm() { }

    public sealed record Solid : TpmsForm;
    public sealed record Sheet(double MinWallMm) : TpmsForm;

    public float Morph(float field, FieldThreshold threshold) => Switch(
        state: (field, threshold),
        solid: static (s, _) => s.field - s.threshold.Iso,
        sheet: static (s, _) => MathF.Abs(s.field - s.threshold.Iso) - 0.5f * s.threshold.WallScaled);
}

// Each case is one VERIFIED PicoGK verb with only its consumed operands. Shell swaps the native lease because
// PicoGK exposes only the copy-returning voxShell form; every other case mutates and returns the same handle.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record VoxelMorphologyStep {
    private VoxelMorphologyStep() { }

    public sealed record Offset(double DistanceMm) : VoxelMorphologyStep;
    public sealed record Shell(double DistanceMm) : VoxelMorphologyStep;
    public sealed record OverOffset(double FirstMm, double FinalSurfaceMm) : VoxelMorphologyStep;
    public sealed record Smoothen(double DistanceMm) : VoxelMorphologyStep;
    public sealed record Fillet(double RadiusMm) : VoxelMorphologyStep;
    public sealed record DoubleOffset(double FirstMm, double SecondMm) : VoxelMorphologyStep;
    public sealed record TripleOffset(double DistanceMm) : VoxelMorphologyStep;

    public Voxels Apply(Voxels voxels) => Switch(
        state: voxels,
        offset: static (held, step) => { held.Offset((float)step.DistanceMm); return held; },
        shell: static (held, step) => {
            Voxels shelled = held.voxShell((float)step.DistanceMm);
            held.Dispose();
            return shelled;
        },
        overOffset: static (held, step) => { held.OverOffset((float)step.FirstMm, (float)step.FinalSurfaceMm); return held; },
        smoothen: static (held, step) => { held.Smoothen((float)step.DistanceMm); return held; },
        fillet: static (held, step) => { held.Fillet((float)step.RadiusMm); return held; },
        doubleOffset: static (held, step) => { held.DoubleOffset((float)step.FirstMm, (float)step.SecondMm); return held; },
        tripleOffset: static (held, step) => { held.TripleOffset((float)step.DistanceMm); return held; });
}

// Pure egress-mode vocabulary: mode-specific filesystem payloads ride their case, never global optional knobs.
// The Implicit.Cli rail owns every row's fold — VdbCli is the voxel-FREE provider bridge, so no case here can
// ever receive (and drop) a live native lease.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CliMode {
    private CliMode() { }

    public sealed record Grayscale : CliMode;
    public sealed record CliVector(Option<string> Path = default) : CliMode;
    public sealed record VdbCli(VdbCliRequest Request) : CliMode;
}

// --- [MODELS] -------------------------------------------------------------------------------------------------------------------------------------
// Density ∈ (0,1] maps to the local iso-threshold; optional sampled fields grade density and axis without
// multiplying topology cases. Factories mint disposable field leases that Raster owns for one voxelization.
public readonly record struct ImplicitCell(
    double CellMm,
    double RelativeDensity,
    Vector3 Axis,
    Option<Func<Fin<ScalarField>>> DensityField = default,
    Option<Func<Fin<VectorField>>> AxisField = default);

public readonly record struct FieldThreshold(float Iso, float WallScaled);

public readonly record struct VdbCliRequest(string VdbPath, string FieldName, string CliPath);

public sealed record ImplicitPolicy(
    VoxelBudget Budget,
    double LayerHeightMm,
    CliMode Cli);

public sealed record VoxelWire(
    ContentKey Key,
    Func<Fin<Voxels>> ToVoxels,
    Func<Voxels, Fin<ContentKey>> FromVoxels);

public sealed record CliStack(int Layers, ContentKey Key, Seq<ContentKey> Masks);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ImplicitOp {
    private ImplicitOp() { }

    public sealed record Field(
        FieldKind Kind,
        TpmsForm Form,
        ImplicitCell Cell,
        VoxelWire Envelope,
        Seq<VoxelMorphologyStep> Morphology,
        ImplicitPolicy Policy) : ImplicitOp;
    public sealed record LatticeSupport(
        SupportPlan Support,
        Option<VoxelWire> Part,
        Seq<VoxelMorphologyStep> Morphology,
        ImplicitPolicy Policy) : ImplicitOp;
    public sealed record Source(VoxelWire Wire, Seq<VoxelMorphologyStep> Morphology, ImplicitPolicy Policy) : ImplicitOp;

    public ImplicitPolicy Policy =>
        Switch(
            field:          static x => x.Policy,
            latticeSupport: static x => x.Policy,
            source:         static x => x.Policy);

    public Seq<VoxelMorphologyStep> Morphology =>
        Switch(
            field: static x => x.Morphology,
            latticeSupport: static x => x.Morphology,
            source: static x => x.Morphology);
}

// --- [SERVICES] -----------------------------------------------------------------------------------------------------------------------------------
// The ONE PicoGK runtime owner: Voxelize resolves the ambient library from VoxelBudget.VoxelSize BEFORE any
// parameterless field ctor runs, so every Voxels/Lattice/Mesh in the fold binds the budget's resolution.
// Library.GlobalInstance is process-ambient. The first admitted size binds for the sidecar lifetime; a later
// mismatch fails instead of disposing the library beneath live native handles. No sibling page touches Library.
file static class VoxelRuntime {
    // Library binds float voxel sizes, so the same-budget test tolerates double round-trip noise — an exact
    // double compare re-binds the library on every ambient wobble.
    private const double SizeTolerance = 1e-9;

    private static readonly Lock Gate = new();
    private static Option<(double Size, Library.GlobalInstance Lease)> _bound = None;

    public static Fin<Unit> Resolve(VoxelBudget budget) {
        lock (Gate) {
            if (_bound.Exists(held => Math.Abs(held.Size - budget.VoxelSize) < SizeTolerance)) return Fin.Succ(unit);
            if (_bound.IsSome) return Fin.Fail<Unit>(Error.New($"picogk:ambient-resolution:{budget.VoxelSize:R}"));
            try {
                _bound = Some((budget.VoxelSize, new Library.GlobalInstance((float)budget.VoxelSize)));
                return Fin.Succ(unit);
            }
            catch (PicoGKAllocException native) { return Fin.Fail<Unit>(Error.New(native.Message)); }
            catch (PicoGKLibraryMismatchException native) { return Fin.Fail<Unit>(Error.New(native.Message)); }
        }
    }
}

// --- [OPERATIONS] ---------------------------------------------------------------------------------------------------------------------------------
public static class Implicit {
    // The returned live Voxels is a PLANE-LOCAL LEASE: the wire consumer disposes it after its FromVoxels
    // egress — intermediates dispose inside the fold, the lease is the one surviving handle. The Cli rail never
    // leaks one: it owns materialization, digestion, and disposal end to end.
    public static Fin<Voxels> Voxelize(ImplicitOp op) =>
        op.Switch(
            field:          static x => Guard(x, x.Policy, () => Raster(x)),
            latticeSupport: static x => Guard(x, x.Policy, () => Lattice(x.Support, x.Part, x.Morphology)),
            source:         static x => Guard(x, x.Policy, () => x.Wire.ToVoxels().Map(voxels => Morph(voxels, x.Morphology))));

    // ONE egress rail: the mode row decides voxel demand BEFORE any materialization — mode, layer height, and
    // paths come from the SAME ImplicitPolicy, so a fixed default vectorization that drops the selected mode is
    // unconstructable. Leased modes voxelize under the full budget guard; VdbCli reaches Vdb2Cli.Convert direct.
    public static Fin<CliStack> Cli(ImplicitOp op) =>
        op.Policy.Cli.Switch(
            grayscale: () => Leased(op, static (voxels, policy) => GrayscaleStack(voxels, policy)),
            cliVector: mode => Leased(op, (voxels, policy) => VectorStack(voxels, policy, mode.Path)),
            vdbCli: mode => Composable(op, op.Policy) ? Direct(op, mode.Request) : Fail<CliStack>(op, op.Policy));

    // Leased egress: the voxel lease lives exactly as long as its mode consumes it — built under the full
    // budget/admission guard, digested, and disposed on every exit; only admitted boundary failures lower.
    private static Fin<CliStack> Leased(ImplicitOp op, Func<Voxels, ImplicitPolicy, CliStack> build) =>
        Voxelize(op).Bind(voxels => {
            try {
                return Fin.Succ(build(voxels, op.Policy));
            }
            catch (PicoGKAllocException) { return Fail<CliStack>(op, op.Policy); }
            catch (PicoGKLibraryMismatchException) { return Fail<CliStack>(op, op.Policy); }
            catch (System.IO.IOException) { return Fail<CliStack>(op, op.Policy); }
            catch (UnauthorizedAccessException) { return Fail<CliStack>(op, op.Policy); }
            finally {
                voxels.Dispose();
            }
        });

    // The provider-native VDB bridge: file to .cli with ZERO managed voxel materialization — success depends
    // only on the admitted request and the direct provider/filesystem boundary, never on budget or morphology.
    private static Fin<CliStack> Direct(ImplicitOp op, VdbCliRequest request) {
        try {
            Vdb2Cli.Convert(request.VdbPath, (float)op.Policy.LayerHeightMm, request.CliPath, request.FieldName);
            PolySliceStack slices = CliIo.oSlicesFromCliFile(request.CliPath).oSlices;
            return Fin.Succ(new CliStack(slices.nCount(), ContentKey.Of(EgressKind.Cli, ImplicitCanonical.Cli(slices)), Seq<ContentKey>()));
        }
        catch (PicoGKAllocException) { return Fail<CliStack>(op, op.Policy); }
        catch (PicoGKLibraryMismatchException) { return Fail<CliStack>(op, op.Policy); }
        catch (System.IO.IOException) { return Fail<CliStack>(op, op.Policy); }
        catch (UnauthorizedAccessException) { return Fail<CliStack>(op, op.Policy); }
    }

    // The content key covers the FULL vectorized geometry; the case-local optional Path additionally lands the true
    // Common Layer Interface machine file through the PicoGK emitter — one row, both duties.
    private static CliStack VectorStack(Voxels voxels, ImplicitPolicy policy, Option<string> path) {
        PolySliceStack slices = voxels.oVectorize((float)policy.LayerHeightMm);
        path.IfSome(target => CliIo.WriteSlicesToCliFile(slices, target, CliIo.EFormat.FirstLayerWithContent));
        return new CliStack(slices.nCount(), ContentKey.Of(EgressKind.Cli, ImplicitCanonical.Cli(slices)), Seq<ContentKey>());
    }

    // Strict: each rasterized layer digests BEFORE the shared image buffer mutates for the next slice.
    private static CliStack GrayscaleStack(Voxels voxels, ImplicitPolicy policy) {
        ImageGrayScale image = voxels.imgAllocateSlice(out int sliceCount);
        Seq<ContentKey> masks = toSeq(Enumerable.Range(0, sliceCount)).Map(n => {
            voxels.GetVoxelSlice(n, ref image);
            return ContentKey.Of(EgressKind.Cli, ImplicitCanonical.Image(n, image));
        }).Strict();
        return new CliStack(sliceCount, ContentKey.Of(EgressKind.Cli, ImplicitCanonical.MaskIndex(masks)), masks);
    }

    private static Fin<Voxels> Guard(ImplicitOp op, ImplicitPolicy policy, Func<Fin<Voxels>> run) {
        if (!Admitted(op, policy)) return Fail<Voxels>(op, policy);
        try {
            return VoxelRuntime.Resolve(policy.Budget)
                .MapFail(_ => FabricationFault.VoxelFault(op, policy.Budget).ToError())
                .Bind(_ => run())
                .Bind(v => WithinBudget(v, op, policy));
        }
        catch (PicoGKAllocException) {
            return Fail<Voxels>(op, policy);
        }
        catch (PicoGKLibraryMismatchException) {
            return Fail<Voxels>(op, policy);
        }
    }

    // Voxel-lane admission: budget and morphology clauses gate ONLY materializing rails — Cli's direct VDB arm
    // consults Composable alone, so an unrelated budget defect cannot veto a file-to-file conversion.
    private static bool Admitted(ImplicitOp op, ImplicitPolicy policy) =>
        policy.Budget.Bounds.IsValid
        && policy.Budget.VoxelSize > 0.0
        && policy.Budget.VoxelCap > 0
        && op.Morphology.ForAll(ValidMorphology)
        && Composable(op, policy);

    // The shared op/mode composition law: VdbCli composes only with a morphology-FREE Source — a raster or a
    // morphed lease the direct bridge would silently ignore is unconstructable on either rail.
    private static bool Composable(ImplicitOp op, ImplicitPolicy policy) =>
        policy.LayerHeightMm > 0.0
        && policy.Cli.Switch(
            grayscale: static () => true,
            cliVector: static mode => mode.Path.ForAll(static path => !string.IsNullOrWhiteSpace(path)),
            vdbCli: static mode => !string.IsNullOrWhiteSpace(mode.Request.VdbPath)
                && !string.IsNullOrWhiteSpace(mode.Request.FieldName)
                && !string.IsNullOrWhiteSpace(mode.Request.CliPath))
        && (policy.Cli is not CliMode.VdbCli || (op is ImplicitOp.Source && op.Morphology.IsEmpty))
        && op.Switch(
            field: static field => field.Cell.CellMm > 0.0
                && field.Cell.RelativeDensity is > 0.0 and < 1.0
                && field.Cell.Axis.LengthSquared() > 1e-12f
                && field.Form is not TpmsForm.Sheet { MinWallMm: <= 0.0 },
            latticeSupport: static lattice => !lattice.Support.TreeNodes.IsEmpty,
            source: static _ => true);

    private static bool ValidMorphology(VoxelMorphologyStep step) =>
        step.Switch(
            offset: static value => double.IsFinite(value.DistanceMm),
            shell: static value => double.IsFinite(value.DistanceMm) && value.DistanceMm != 0.0,
            overOffset: static value => double.IsFinite(value.FirstMm) && double.IsFinite(value.FinalSurfaceMm),
            smoothen: static value => double.IsFinite(value.DistanceMm) && value.DistanceMm > 0.0,
            fillet: static value => double.IsFinite(value.RadiusMm) && value.RadiusMm > 0.0,
            doubleOffset: static value => double.IsFinite(value.FirstMm) && double.IsFinite(value.SecondMm),
            tripleOffset: static value => double.IsFinite(value.DistanceMm));

    // Driver factories bind before the envelope, and every subsequent failure releases those leases before the
    // error returns. The raster allocates only after the content-keyed envelope has materialized successfully.
    private static Fin<Voxels> Raster(ImplicitOp.Field op) =>
        from density in Acquire(op.Cell.DensityField)
        from axis in Acquire(op.Cell.AxisField)
            .MapFail(error => Released(density, Option<VectorField>.None, error))
        from bound in Envelope(op.Envelope, density, axis)
        from raster in RasterBounded(op, bound, density, axis)
        select raster;

    private static Fin<Voxels> Envelope(
        VoxelWire wire,
        Option<ScalarField> density,
        Option<VectorField> axis) {
        bool ownershipTransferred = false;
        try {
            Fin<Voxels> result = wire.ToVoxels().MapFail(error => Released(density, axis, error));
            ownershipTransferred = true;
            return result;
        }
        finally {
            if (!ownershipTransferred) Release(density, axis);
        }
    }

    private static Fin<Voxels> RasterBounded(
        ImplicitOp.Field op,
        Voxels bound,
        Option<ScalarField> density,
        Option<VectorField> axis) {
        Voxels? allocated = null;
        try {
            return FieldCalibration.Of(op.Kind).Map(calibration => {
                BBox3 bounds = PicoBounds(op.Policy.Budget.Bounds);
                allocated = new Voxels(new PeriodicImplicit(op.Kind, op.Form, op.Cell, bounds, density, axis, calibration), bounds);
                allocated.BoolIntersect(bound);
                Voxels admitted = allocated;
                allocated = null;
                return Morph(admitted, op.Morphology);
            });
        }
        finally {
            allocated?.Dispose();
            bound.Dispose();
            Release(density, axis);
        }
    }

    private static Fin<Voxels> Lattice(SupportPlan support, Option<VoxelWire> part, Seq<VoxelMorphologyStep> morphology) {
        Seq<TreeNode> nodes = support.TreeNodes;
        HashMap<int, TreeNode> byId = toHashMap(nodes.Map(static n => (n.Id, n)));
        return from edges in nodes
                   .Bind(node => node.Parents.Map(parentId => (Node: node, ParentId: parentId)))
                   .Map(edge => byId.Find(edge.ParentId)
                       .ToFin(GeometryFault.DegenerateInput($"implicit-lattice:missing-parent:{edge.ParentId}").ToError())
                       .Map(parent => (edge.Node, Parent: parent)))
                   .Sequence()
               let scaffold = LatticeVoxels(edges)
               from clipped in part
                   .Map(wire => PartSubtract(scaffold, wire))
                   .IfNone(Fin.Succ(scaffold))
               select Morph(clipped, morphology);
    }

    private static Fin<Voxels> PartSubtract(Voxels scaffold, VoxelWire wire) {
        bool ownershipTransferred = false;
        try {
            Fin<Voxels> source = wire.ToVoxels();
            Fin<Voxels> result = source
                .Bind(model => Subtract(scaffold, model))
                .MapFail(error => Released(scaffold, error));
            ownershipTransferred = true;
            return result;
        }
        finally {
            if (!ownershipTransferred) scaffold.Dispose();
        }
    }

    private static Voxels LatticeVoxels(Seq<(TreeNode Node, TreeNode Parent)> edges) {
        using PicoGK.Lattice lattice = new();
        edges.Iter(edge => lattice.AddBeam(
            ToVector(edge.Parent.At),
            (float)edge.Parent.Radius,
            ToVector(edge.Node.At),
            (float)edge.Node.Radius,
            bRoundCap: true));
        return new Voxels(lattice);
    }

    private static Voxels Morph(Voxels voxels, Seq<VoxelMorphologyStep> steps) {
        Voxels held = voxels;
        bool ownershipTransferred = false;
        try {
            steps.Iter(step => held = step.Apply(held));
            ownershipTransferred = true;
            return held;
        }
        finally {
            if (!ownershipTransferred) held.Dispose();
        }
    }

    private static Fin<Voxels> Subtract(Voxels scaffold, Voxels model) {
        try {
            scaffold.BoolSubtract(model);
            return Fin.Succ(scaffold);
        }
        finally {
            model.Dispose();
        }
    }

    // The over-cap voxel field is already allocated when the gate rejects: the lease releases on the
    // fail rail so the budget fault never strands native memory.
    private static Fin<Voxels> WithinBudget(Voxels voxels, ImplicitOp op, ImplicitPolicy policy) =>
        voxels.nMemUsage() <= policy.Budget.VoxelCap
            ? Fin.Succ(voxels)
            : Fail<Voxels>(op, policy).MapFail(error => Released(voxels, error));

    private static Fin<T> Fail<T>(ImplicitOp op, ImplicitPolicy policy) =>
        Fin.Fail<T>(FabricationFault.VoxelFault(op, policy.Budget).ToError());

    private static Error Released(Voxels voxels, Error error) {
        voxels.Dispose();
        return error;
    }

    private static Error Released(
        Option<ScalarField> density,
        Option<VectorField> axis,
        Error error) {
        Release(density, axis);
        return error;
    }

    private static Fin<Option<T>> Acquire<T>(Option<Func<Fin<T>>> source) where T : class, IDisposable {
        try {
            return source.Match(
                None: () => Fin.Succ(Option<T>.None),
                Some: factory => factory().Map(Some));
        }
        catch (PicoGKAllocException native) {
            return Fin.Fail<Option<T>>(Error.New(native.Message));
        }
        catch (PicoGKLibraryMismatchException native) {
            return Fin.Fail<Option<T>>(Error.New(native.Message));
        }
    }

    private static Unit Release(Option<ScalarField> density, Option<VectorField> axis) {
        density.Iter(static field => field.Dispose());
        axis.Iter(static field => field.Dispose());
        return unit;
    }

    private static Vector3 ToVector(Point3d p) => new((float)p.X, (float)p.Y, (float)p.Z);

    private static BBox3 PicoBounds(BoundingBox bounds) => new(ToVector(bounds.Min), ToVector(bounds.Max));
}

// --- [IMPLICIT_FIELDS] ----------------------------------------------------------------------------------------------------------------------------
// The one IImplicit realization: sampled fields optionally grade density and orientation at world position; the
// constant cell values are total fallbacks. Calibration stays topology-local and threshold lookup stays O(1).
file sealed class PeriodicImplicit(
    FieldKind kind,
    TpmsForm form,
    ImplicitCell cell,
    BBox3 bounds,
    Option<ScalarField> densityField,
    Option<VectorField> axisField,
    FieldCalibration calibration) : IImplicit, IBoundedImplicit {

    public BBox3 oBounds => bounds;

    public float fSignedDistance(in Vector3 vec) {
        float density = densityField.Bind(field => Sample(field, vec)).IfNone((float)cell.RelativeDensity);
        Vector3 axis = axisField.Bind(field => Sample(field, vec)).IfNone(cell.Axis);
        Quaternion frame = Quaternion.Conjugate(Quaternion.Normalize(FrameOf(axis)));
        float scale = (float)(Math.Tau / Math.Max(cell.CellMm, 1e-6));
        Vector3 oriented = Vector3.Transform(vec, frame) * scale;
        return form.Morph(kind.Sample(oriented), calibration.Threshold(form, cell.CellMm, density));
    }

    private static Option<float> Sample(ScalarField field, Vector3 at) =>
        field.bGetValue(at, out float value) && float.IsFinite(value) ? Some(value) : None;

    private static Option<Vector3> Sample(VectorField field, Vector3 at) =>
        field.bGetValue(at, out Vector3 value)
        && float.IsFinite(value.X)
        && float.IsFinite(value.Y)
        && float.IsFinite(value.Z)
        && value.LengthSquared() > 1e-12f
            ? Some(value)
            : None;

    private static Quaternion FrameOf(Vector3 axis) {
        Vector3 direction = Vector3.Normalize(axis);
        float dot = Math.Clamp(Vector3.Dot(Vector3.UnitZ, direction), -1.0f, 1.0f);
        return dot > 1.0f - 1e-6f ? Quaternion.Identity
            : dot < -1.0f + 1e-6f ? Quaternion.CreateFromAxisAngle(Vector3.UnitX, MathF.PI)
            : Quaternion.CreateFromAxisAngle(Vector3.Normalize(Vector3.Cross(Vector3.UnitZ, direction)), MathF.Acos(dot));
    }
}

// The calibration samples one periodic cell once per implicit object. Tensor reductions establish the finite range;
// exact ordered quantiles then map relative density to the solid threshold or sheet half-width.
file sealed class FieldCalibration {
    private const int Resolution = 16;
    private readonly float[] _samples;

    private FieldCalibration(float[] samples) => _samples = samples;

    public static Fin<FieldCalibration> Of(FieldKind kind) {
        float[] samples = toSeq(Enumerable.Range(0, Resolution * Resolution * Resolution))
            .Map(i => {
                int x = i % Resolution;
                int y = i / Resolution % Resolution;
                int z = i / (Resolution * Resolution);
                float s = MathF.Tau / Resolution;
                return kind.Sample(new Vector3((x + 0.5f) * s, (y + 0.5f) * s, (z + 0.5f) * s));
            })
            .ToArray();
        float minimum = TensorPrimitives.Min(samples);
        float maximum = TensorPrimitives.Max(samples);
        if (!float.IsFinite(minimum) || !float.IsFinite(maximum) || minimum >= maximum) {
            return Fin.Fail<FieldCalibration>(GeometryFault.DegenerateInput("implicit-field-calibration").ToError());
        }

        Array.Sort(samples);
        return Fin.Succ(new FieldCalibration(samples));
    }

    public FieldThreshold Threshold(TpmsForm form, double cellMm, double relativeDensity) {
        double density = Math.Clamp(relativeDensity, 0.01, 0.99);
        return form.Switch(
            state: (Samples: _samples, Density: density, CellMm: cellMm),
            solid: static (s, _) => new FieldThreshold(s.Samples[(int)Math.Round(s.Density * (s.Samples.Length - 1))], 0.0f),
            sheet: static (s, sheet) => Sheet(s.Samples, s.Density, (float)(Math.Tau * sheet.MinWallMm / Math.Max(s.CellMm, 1e-6))));
    }

    private static FieldThreshold Sheet(float[] samples, double density, float minimumWall) {
        float iso = samples[samples.Length / 2];
        float[] deviations = samples.Map(value => MathF.Abs(value - iso)).ToArray();
        Array.Sort(deviations);
        float halfWidth = deviations[(int)Math.Round(density * (deviations.Length - 1))];
        return new FieldThreshold(iso, MathF.Max(minimumWall, 2.0f * halfWidth));
    }
}

// --- [CANONICAL_BYTES] ----------------------------------------------------------------------------------------------------------------------------
// Payload-complete canonical folds: every contour vertex and every raster value reaches the digest — a key
// minted over metadata alone (index without raster, Z without contours) is the byte-identity defect.
public static class ImplicitCanonical {
    public static byte[] Cli(PolySliceStack slices) {
        using ArrayPoolBufferWriter<byte> writer = new();
        Int32(writer, slices.nCount());
        for (int n = 0; n < slices.nCount(); n++) {
            PolySlice slice = slices.oSliceAt(n);
            Float64(writer, slice.fZPos());
            Int32(writer, slice.nContours());
            for (int c = 0; c < slice.nContours(); c++) {
                PolyContour contour = slice.oContourAt(c);
                Int32(writer, contour.nCount());
                Int32(writer, (int)contour.eWinding());
                for (int v = 0; v < contour.nCount(); v++) {
                    Vector2 vertex = contour.vecVertex(v);
                    Float64(writer, vertex.X);
                    Float64(writer, vertex.Y);
                }
            }
        }
        return writer.WrittenSpan.ToArray();
    }

    public static byte[] Image(int n, ImageGrayScale image) {
        using ArrayPoolBufferWriter<byte> writer = new();
        Int32(writer, n);
        Int32(writer, image.nWidth);
        Int32(writer, image.nHeight);
        image.m_afValues.Iter(value => Float32(writer, value));
        return writer.WrittenSpan.ToArray();
    }

    public static byte[] MaskIndex(Seq<ContentKey> masks) {
        using ArrayPoolBufferWriter<byte> writer = new();
        Int32(writer, masks.Count);
        masks.Iter(key => UInt128Value(writer, key.Digest));
        return writer.WrittenSpan.ToArray();
    }

    private static void Int32(ArrayPoolBufferWriter<byte> writer, int value) {
        System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(writer.GetSpan(sizeof(int)), value);
        writer.Advance(sizeof(int));
    }

    private static void Float64(ArrayPoolBufferWriter<byte> writer, double value) {
        System.Buffers.Binary.BinaryPrimitives.WriteDoubleLittleEndian(writer.GetSpan(sizeof(double)), value);
        writer.Advance(sizeof(double));
    }

    private static void Float32(ArrayPoolBufferWriter<byte> writer, float value) {
        System.Buffers.Binary.BinaryPrimitives.WriteSingleLittleEndian(writer.GetSpan(sizeof(float)), value);
        writer.Advance(sizeof(float));
    }

    private static void UInt128Value(ArrayPoolBufferWriter<byte> writer, UInt128 value) {
        System.Buffers.Binary.BinaryPrimitives.WriteUInt128LittleEndian(writer.GetSpan(16), value);
        writer.Advance(16);
    }
}
```
