# [RASM_FABRICATION_IMPLICIT]

The implicit additive owner: ONE `Implicit.Voxelize` rail over the PicoGK sidecar-native kernel, absorbing conformal TPMS/gyroid/cellular infill, `Lattice`-realized support scaffolds, shell/offset lightweighting, and grayscale/`.cli` layer egress without widening `FabricationResult`. `ImplicitOp` is the full discriminant: the periodic-field family collapses to ONE `Field` case whose `FieldKind` delegate rows OWN their signed-distance laws (`gyroid`/`schwarz-p`/`schwarz-d`/`neovius`/`lidinoid`/`cellular` — a new lattice topology is one row carrying its own SDF, never an if-chain arm), the `TpmsForm` axis selects solid (`f − iso`) versus sheet (`|f| − t/2`) morphology, and `ImplicitCell.Density` maps to the iso-threshold while `Axis` orients the periodic frame — every declared cell field drives the evaluated geometry. `Voxels` stays a plane-local native handle. Cross-plane material moves through `VoxelWire`, the content-keyed mesh-to-voxel seam, and machine egress moves through `CliStack` with `ContentKey.Of(EgressKind.Cli, bytes)` minted over the COMPLETE payload — every grayscale raster byte and every vectorized contour vertex reaches the digest, so two distinct masks or contour stacks can never share a key. The Rhino plugin domain never references PicoGK; Verify/removal and mesh consumers compose the same wire and never mint a second native posture.

Wire posture: HOST-LOCAL. `Voxels` never crosses a result case, browser wire, or peer boundary; `ContentKey` and layer counts are the only egress payloads.

## [01]-[INDEX]

- [01]-[IMPLICIT]: owns `ImplicitOp`, `FieldKind`, `TpmsForm`, `ImplicitCell`, `VoxelMorphology`, `CliMode`, `ImplicitPolicy`, `VoxelWire`, `CliStack`, the `VoxelRuntime` `Library.GlobalInstance` owner, `PeriodicImplicit`, `ImplicitCanonical`, and the two public entries `Implicit.Voxelize(ImplicitOp)` plus `Implicit.Cli(Voxels, ImplicitPolicy)` over PicoGK `IImplicit`/`Voxels`/`Lattice`/`CliIo`/`Vdb2Cli`.

## [02]-[IMPLICIT]

- Owner: `ImplicitOp` the additive voxel discriminant (`Field` · `LatticeSupport` · `ShellOffset` · `Grayscale` · `CliVector`); `FieldKind` the delegate-backed periodic-field vocabulary whose rows carry their SDF laws; `TpmsForm` the solid/sheet morphology axis; `ImplicitCell` the cell/wall/density/axis carrier — every field consumed; `VoxelMorphology` the distance-morphology rows over the verified PicoGK verb pairs; `CliMode` the egress rows; `ImplicitPolicy` the typed budget, raster extent, and egress carrier; `VoxelWire` the content-keyed mesh-to-`Voxels` seam; `CliStack` the egress receipt; `Implicit` the static surface.
- Cases: `Field(FieldKind, TpmsForm, ImplicitCell, VoxelWire, ImplicitPolicy)` rasterizes the row-owned SDF with `new Voxels(IImplicit, BBox3)` and intersects the envelope — the three former per-topology cases are one case carrying the vocabulary row; `LatticeSupport` maps `SupportPlan.Tree` upward links to `Lattice.AddBeam`, `new Voxels(Lattice)`, and `BoolSubtract`; `ShellOffset` folds `VoxelMorphology` steps; `Grayscale`/`CliVector` return the source voxels untouched — the egress work happens ONCE, at `Implicit.Cli`, never built-then-discarded inside `Voxelize`; `VoxelMorphology` rows 7 — `offset`/`shell`/`over-offset`/`smoothen`/`fillet`/`double-offset`/`triple-offset`, each row the verified PicoGK verb (`Shell` exists only as the copy form `voxShell`, so that row swaps the lease: the copy returns, the source disposes); `CliMode` rows 3 — `grayscale`/`cli-vector`/`vdb-cli`.
- Entry: `public static Fin<Voxels> Voxelize(ImplicitOp op)` and `public static CliStack Cli(Voxels voxels, ImplicitPolicy policy)` — the first owns all voxel-producing cases and routes `FabricationFault.VoxelFault(op, budget)` 2715; the second routes the policy's `CliMode.Build` row over a finished plane-local `Voxels`, so grayscale masks, layer height, VDB requests, and the optional `.cli` machine-file emission ride the SAME policy the voxels were built under.
- Auto: `Voxelize` guards `VoxelBudget`, resolves the ONE ambient PicoGK `Library.GlobalInstance` from `VoxelBudget.VoxelSize` through the `VoxelRuntime` owner BEFORE any field constructor runs (a size change disposes the held instance and binds a fresh one — the class is `IDisposable`, so the rebind is a lease swap, never a leak; the two native exceptions lower onto the rail as `VoxelFault`), dispatches the union, and returns live `Voxels` only inside the additive plane as a plane-local lease the consumer disposes after wire egress. The `Field` case binds the envelope FIRST — `envelope.ToVoxels()` short-circuits before the field raster allocates, so a failed wire conversion never strands a native handle. `PeriodicImplicit` rotates the sample into the `Axis` frame, scales by `CellMm`, evaluates the row law, applies the `TpmsForm` morphology at the `Density`-derived iso-threshold, and clamps the wall by `WallMm`. Egress cases mint `CliStack` over `ImplicitCanonical` payload-complete bytes; `CliVector` additionally emits the true Common Layer Interface machine file through `CliIo.WriteSlicesToCliFile` when the policy carries a `CliPath`.
- Receipt: `CliStack` carries the vector-layer count, the `.cli` `ContentKey`, and grayscale mask keys; `VoxelWire` carries the mesh/voxel content key and conversion functions; `FabricationResult.AdditiveResult` receives only moves, layer count, and `Seq<ContentKey>`, never `Voxels`, `Lattice`, `Mesh`, `OpenVdbFile`, or `Library`.
- Packages: `api-picogk.md` (`IImplicit`, `Voxels`, `Lattice`, `Mesh`, `PolySliceStack`, `PolySlice.fZPos()`/`nContours()`/`oContourAt`, `PolyContour.nCount()`/`vecVertex`, `ImageGrayScale.fValue`/`m_afValues`, `CliIo.WriteSlicesToCliFile`, `Vdb2Cli.Convert`, `Library.GlobalInstance`, `BBox3`), `Process/owner#FABRICATION_OWNER` (`EgressKind.Cli`, `ContentKey.Of`, `FabricationResult.AdditiveResult`), `Process/faults#FAULT_BAND` (`VoxelFault` 2715, `VoxelBudget`), `Additive/support#SUPPORT` (`TreeNode`, `SupportPlan`), `Query/cache#ARTIFACT_BLOB_INDEX` (`cli` artifact enrollment through content key), CommunityToolkit.HighPerformance (`ArrayPoolBufferWriter<byte>` — the pooled canonical-bytes sink), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new periodic topology is one `FieldKind` row carrying its SDF delegate; a new field morphology is one `TpmsForm` row; a new voxel morphology is one `VoxelMorphology` row over a verified PicoGK verb; a new layer egress is one `CliMode` row; a new source/sink form is one `VoxelWire` adapter; a graded/anisotropic density driver is a PicoGK `VectorField`-sampled `ImplicitCell.Density` extension on the same row law; zero new entrypoint, zero result widening, zero Rhino-domain PicoGK reference.
- Boundary: PicoGK remains the net9 sidecar-native ALC firebreak and a plugin-domain reference is the deleted form; `Voxels` is plane-local and a result payload is the named ruling-5 defect; a content key minted over metadata alone — a layer index without its raster, a Z position without its contours — is the byte-identity defect this page's canonical folds foreclose; the content-keyed mesh-to-voxel seam is the only Verify/removal bridge; `ContentKey.Of(EgressKind.Cli, ...)` is the only `.cli`/mask identity mint; `Support` owns tree search and planar support algebra; `Scan` owns LPBF metal vectors; this page owns resin/powder voxel grayscale and `.cli` stacks.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------------------------------------------------------------
using System.Numerics;
using System.Runtime.InteropServices;
using CommunityToolkit.HighPerformance.Buffers;
using LanguageExt;
using LanguageExt.Common;
using PicoGK;
using Rasm.Fabrication.Process;
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
        static v => MathF.Min(MathF.Abs(MathF.Sin(v.X) * MathF.Sin(v.Y)), MathF.Abs(MathF.Sin(v.Y) * MathF.Sin(v.Z))));

    [UseDelegateFromConstructor]
    public partial float Sample(Vector3 scaled);
}

// Solid keeps f − iso (skeletal lattice); Sheet keeps |f| − t/2 (wall lattice) — the two TPMS realizations.
[SmartEnum<string>]
public sealed partial class TpmsForm {
    public static readonly TpmsForm Solid = new("solid", static (f, iso, wall) => f - iso);
    public static readonly TpmsForm Sheet = new("sheet", static (f, iso, wall) => MathF.Abs(f - iso) - 0.5f * wall);

    [UseDelegateFromConstructor]
    public partial float Morph(float field, float iso, float wall);
}

// Every row is a VERIFIED PicoGK verb; Shell has no mutating form, so its row swaps the lease — the voxShell
// copy returns and the source disposes. Apply therefore returns the surviving handle, never void.
[SmartEnum<string>]
public sealed partial class VoxelMorphology {
    public static readonly VoxelMorphology Offset = new(
        "offset", static (voxels, step) => { voxels.Offset((float)step.PrimaryMm); return voxels; });

    public static readonly VoxelMorphology Shell = new(
        "shell",
        static (voxels, step) => {
            Voxels shelled = voxels.voxShell((float)step.PrimaryMm);
            voxels.Dispose();
            return shelled;
        });

    public static readonly VoxelMorphology OverOffset = new(
        "over-offset", static (voxels, step) => { voxels.OverOffset((float)step.PrimaryMm, (float)step.SecondaryMm); return voxels; });

    public static readonly VoxelMorphology Smoothen = new(
        "smoothen", static (voxels, step) => { voxels.Smoothen((float)step.PrimaryMm); return voxels; });

    public static readonly VoxelMorphology Fillet = new(
        "fillet", static (voxels, step) => { voxels.Fillet((float)step.PrimaryMm); return voxels; });

    public static readonly VoxelMorphology DoubleOffset = new(
        "double-offset", static (voxels, step) => { voxels.DoubleOffset((float)step.PrimaryMm, (float)step.SecondaryMm); return voxels; });

    public static readonly VoxelMorphology TripleOffset = new(
        "triple-offset", static (voxels, step) => { voxels.TripleOffset((float)step.PrimaryMm); return voxels; });

    [UseDelegateFromConstructor]
    public partial Voxels Apply(Voxels voxels, VoxelMorphologyStep step);
}

[SmartEnum<string>]
public sealed partial class CliMode {
    public static readonly CliMode Grayscale = new("grayscale", GrayscaleStack);
    public static readonly CliMode CliVector = new("cli-vector", VectorStack);
    public static readonly CliMode VdbCli = new("vdb-cli", VdbCliStack);

    [UseDelegateFromConstructor]
    public partial CliStack Build(Voxels voxels, ImplicitPolicy policy);

    // The content key covers the FULL vectorized geometry; the optional CliPath additionally lands the true
    // Common Layer Interface machine file through the PicoGK emitter — one row, both duties.
    static CliStack VectorStack(Voxels voxels, ImplicitPolicy policy) {
        PolySliceStack slices = voxels.oVectorize((float)policy.LayerHeightMm);
        policy.CliPath.IfSome(path => CliIo.WriteSlicesToCliFile(slices, path, CliIo.EFormat.FirstLayerWithContent));
        return new CliStack(slices.nCount(), ContentKey.Of(EgressKind.Cli, ImplicitCanonical.Cli(slices)), Seq<ContentKey>());
    }

    static CliStack GrayscaleStack(Voxels voxels, ImplicitPolicy policy) {
        ImageGrayScale image = voxels.imgAllocateSlice(out int sliceCount);
        Seq<ContentKey> masks = toSeq(Enumerable.Range(0, sliceCount)).Map(n => {
            voxels.GetVoxelSlice(n, ref image);
            return ContentKey.Of(EgressKind.Cli, ImplicitCanonical.Image(n, image));
        });
        return new CliStack(sliceCount, ContentKey.Of(EgressKind.Cli, ImplicitCanonical.MaskIndex(masks)), masks);
    }

    static CliStack VdbCliStack(Voxels voxels, ImplicitPolicy policy) {
        PolySliceStack slices = voxels.oVectorize((float)policy.LayerHeightMm);
        policy.VdbCli.IfSome(req => Vdb2Cli.Convert(req.VdbPath, (float)policy.LayerHeightMm, req.CliPath, req.FieldName));
        return new CliStack(slices.nCount(), ContentKey.Of(EgressKind.Cli, ImplicitCanonical.Cli(slices)), Seq<ContentKey>());
    }
}

// --- [MODELS] -------------------------------------------------------------------------------------------------------------------------------------
// Density ∈ (0,1] maps to the iso-threshold shifting the level set toward the target volume fraction;
// Axis orients the periodic frame — both consumed by PeriodicImplicit, never metadata.
public readonly record struct ImplicitCell(double CellMm, double WallMm, double Density, Vector3 Axis);

public readonly record struct VoxelMorphologyStep(VoxelMorphology Morphology, double PrimaryMm, double SecondaryMm = 0.0);

public readonly record struct VdbCliRequest(string VdbPath, string FieldName, string CliPath);

public sealed record ImplicitPolicy(
    VoxelBudget Budget,
    BBox3 PicoBounds,
    double LayerHeightMm,
    CliMode Cli,
    Seq<VoxelMorphologyStep> Morphology,
    Option<string> CliPath = default,
    Option<VdbCliRequest> VdbCli = default);

public sealed record VoxelWire(
    ContentKey Key,
    Func<Fin<Voxels>> ToVoxels,
    Func<Voxels, Fin<ContentKey>> FromVoxels);

public sealed record CliStack(int Layers, ContentKey Key, Seq<ContentKey> Masks);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ImplicitOp {
    private ImplicitOp() { }

    public sealed record Field(FieldKind Kind, TpmsForm Form, ImplicitCell Cell, VoxelWire Envelope, ImplicitPolicy Policy) : ImplicitOp;
    public sealed record LatticeSupport(SupportPlan Support, Option<VoxelWire> Part, ImplicitPolicy Policy) : ImplicitOp;
    public sealed record ShellOffset(VoxelWire Source, Seq<VoxelMorphologyStep> Steps, ImplicitPolicy Policy) : ImplicitOp;
    public sealed record Grayscale(VoxelWire Source, ImplicitPolicy Policy) : ImplicitOp;
    public sealed record CliVector(VoxelWire Source, ImplicitPolicy Policy) : ImplicitOp;

    public ImplicitPolicy Policy =>
        Switch(
            field:          static x => x.Policy,
            latticeSupport: static x => x.Policy,
            shellOffset:    static x => x.Policy,
            grayscale:      static x => x.Policy,
            cliVector:      static x => x.Policy);
}

// --- [SERVICES] -----------------------------------------------------------------------------------------------------------------------------------
// The ONE PicoGK runtime owner: Voxelize resolves the ambient library from VoxelBudget.VoxelSize BEFORE any
// parameterless field ctor runs, so every Voxels/Lattice/Mesh in the fold binds the budget's resolution.
// Library.GlobalInstance is an IDisposable CLASS — the rebind disposes the held instance and constructs a
// fresh one (changing voxel size means a new Library, the catalog lifecycle law); mixed-resolution fields
// are unconstructable inside the additive plane. No sibling page touches Library.
static class VoxelRuntime {
    // Library binds float voxel sizes, so the same-budget test tolerates double round-trip noise — an exact
    // double compare re-binds the library on every ambient wobble.
    const double SizeTolerance = 1e-9;

    static readonly Atom<Option<(double Size, Library.GlobalInstance Lease)>> Bound = Atom(Option<(double, Library.GlobalInstance)>.None);

    public static Fin<Unit> Resolve(VoxelBudget budget) =>
        Bound.Value.Filter(held => Math.Abs(held.Size - budget.VoxelSize) < SizeTolerance).Match(
            Some: static _ => Fin.Succ(unit),
            None: () => Rebind(budget.VoxelSize));

    // PicoGKAllocException / PicoGKLibraryMismatchException lower onto the rail here — the two native fault doors.
    static Fin<Unit> Rebind(double voxelSizeMm) =>
        Try(() => {
                Library.GlobalInstance fresh = new((float)voxelSizeMm);
                Bound.Swap(held => {
                    held.IfSome(static prior => prior.Lease.Dispose());
                    return Some((voxelSizeMm, fresh));
                });
                return unit;
            })
            .ToFin();
}

// --- [OPERATIONS] ---------------------------------------------------------------------------------------------------------------------------------
public static class Implicit {
    // The returned live Voxels is a PLANE-LOCAL LEASE: the additive/verify consumer disposes it after its wire
    // egress (FromVoxels / Cli) — intermediates dispose inside the fold, the lease is the one surviving handle.
    // Grayscale/CliVector return the source untouched: the egress work happens ONCE, at Implicit.Cli.
    public static Fin<Voxels> Voxelize(ImplicitOp op) =>
        op.Switch(
            field:          static x => Guard(x, x.Policy, () => Raster(x)),
            latticeSupport: static x => Guard(x, x.Policy, () => Lattice(x.Support, x.Part, x.Policy)),
            shellOffset:    static x => Guard(x, x.Policy, () => x.Source.ToVoxels().Map(v => Morph(v, x.Steps))),
            grayscale:      static x => Guard(x, x.Policy, x.Source.ToVoxels),
            cliVector:      static x => Guard(x, x.Policy, x.Source.ToVoxels));

    // The .cli egress rides the policy's CliMode row — grayscale masks, vector stacks, VDB-CLI, and the optional
    // machine-file emission all through ONE name; mode, layer height, and paths come from the SAME ImplicitPolicy
    // the voxels were built under, so a fixed default vectorization that drops the selected mode is unconstructable.
    public static CliStack Cli(Voxels voxels, ImplicitPolicy policy) =>
        policy.Cli.Build(voxels, policy);

    static Fin<Voxels> Guard(ImplicitOp op, ImplicitPolicy policy, Func<Fin<Voxels>> run) =>
        policy.Budget.VoxelSize <= 0.0 || policy.Budget.VoxelCap <= 0
            ? Fail(op, policy)
            : VoxelRuntime.Resolve(policy.Budget)
                .MapFail(_ => FabricationFault.VoxelFault(op, policy.Budget).ToError())
                .Bind(_ => run())
                .Bind(v => WithinBudget(v, op, policy));

    // The envelope binds FIRST: a failed wire conversion short-circuits before the field raster allocates,
    // so no native handle exists to strand on the fail rail.
    static Fin<Voxels> Raster(ImplicitOp.Field op) =>
        op.Envelope.ToVoxels().Map(bound => {
            Voxels voxels = new(new PeriodicImplicit(op.Kind, op.Form, op.Cell, op.Policy.PicoBounds), op.Policy.PicoBounds);
            voxels.BoolIntersect(bound);
            bound.Dispose();
            return Morph(voxels, op.Policy.Morphology);
        });

    static Fin<Voxels> Lattice(SupportPlan support, Option<VoxelWire> part, ImplicitPolicy policy) {
        using PicoGK.Lattice lattice = new();
        HashMap<int, TreeNode> byId = toHashMap(support.Tree.Map(static n => (n.Id, n)));
        support.Tree.Filter(static n => n.Parent >= 0).Iter(node => {
            TreeNode parent = byId.Find(node.Parent).IfNone(node);
            lattice.AddBeam(ToVector(parent.At), (float)parent.Radius, ToVector(node.At), (float)node.Radius, bRoundCap: true);
        });
        Voxels scaffold = new(lattice);
        // Released rides the fail rail: a ToVoxels short-circuit before Subtract consumes the scaffold
        // must release the plane-local lease, never leak the native field.
        Fin<Voxels> clipped = part
            .Map(w => w.ToVoxels().Bind(model => Subtract(scaffold, model)).MapFail(error => Released(scaffold, error)))
            .IfNone(Fin.Succ(scaffold));
        return clipped.Map(v => Morph(v, policy.Morphology));
    }

    static Voxels Morph(Voxels voxels, Seq<VoxelMorphologyStep> steps) =>
        steps.Fold(voxels, static (held, step) => step.Morphology.Apply(held, step));

    static Fin<Voxels> Subtract(Voxels scaffold, Voxels model) {
        scaffold.BoolSubtract(model);
        model.Dispose();
        return Fin.Succ(scaffold);
    }

    // The over-cap voxel field is already allocated when the gate rejects: the lease releases on the
    // fail rail so the budget fault never strands native memory.
    static Fin<Voxels> WithinBudget(Voxels voxels, ImplicitOp op, ImplicitPolicy policy) =>
        voxels.nMemUsage() <= policy.Budget.VoxelCap
            ? Fin.Succ(voxels)
            : Fail(op, policy).MapFail(error => Released(voxels, error));

    static Fin<Voxels> Fail(ImplicitOp op, ImplicitPolicy policy) =>
        Fin.Fail<Voxels>(FabricationFault.VoxelFault(op, policy.Budget).ToError());

    static Error Released(Voxels voxels, Error error) {
        voxels.Dispose();
        return error;
    }

    static Vector3 ToVector(Point3d p) => new((float)p.X, (float)p.Y, (float)p.Z);
}

// --- [IMPLICIT_FIELDS] ----------------------------------------------------------------------------------------------------------------------------
// The one IImplicit realization: rotate the sample into the Axis frame, scale by the cell period, evaluate the
// row law, morph by TpmsForm at the Density-derived iso-threshold. Every ImplicitCell field is load-bearing.
public sealed class PeriodicImplicit(FieldKind kind, TpmsForm form, ImplicitCell cell, BBox3 bounds) : IImplicit, IBoundedImplicit {
    readonly Quaternion frame = Quaternion.Normalize(FrameOf(cell.Axis));
    readonly float iso = 1.5f * (1.0f - 2.0f * (float)Math.Clamp(cell.Density, 0.05, 1.0));

    public BBox3 oBounds => bounds;

    public float fSignedDistance(in Vector3 vec) {
        float scale = (float)(Math.Tau / Math.Max(cell.CellMm, 1e-6));
        Vector3 oriented = Vector3.Transform(vec, frame) * scale;
        return form.Morph(kind.Sample(oriented), iso, (float)cell.WallMm * scale);
    }

    static Quaternion FrameOf(Vector3 axis) =>
        axis.LengthSquared() < 1e-12f || Vector3.Normalize(axis) == Vector3.UnitZ
            ? Quaternion.Identity
            : Quaternion.CreateFromAxisAngle(
                Vector3.Normalize(Vector3.Cross(Vector3.UnitZ, Vector3.Normalize(axis))),
                MathF.Acos(Math.Clamp(Vector3.Dot(Vector3.UnitZ, Vector3.Normalize(axis)), -1.0f, 1.0f)));
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
        writer.Write(MemoryMarshal.AsBytes(image.m_afValues.AsSpan()));
        return writer.WrittenSpan.ToArray();
    }

    public static byte[] MaskIndex(Seq<ContentKey> masks) {
        using ArrayPoolBufferWriter<byte> writer = new();
        Int32(writer, masks.Count);
        masks.Iter(key => UInt128Value(writer, key.Digest));
        return writer.WrittenSpan.ToArray();
    }

    static void Int32(ArrayPoolBufferWriter<byte> writer, int value) {
        System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(writer.GetSpan(sizeof(int)), value);
        writer.Advance(sizeof(int));
    }

    static void Float64(ArrayPoolBufferWriter<byte> writer, double value) {
        System.Buffers.Binary.BinaryPrimitives.WriteDoubleLittleEndian(writer.GetSpan(sizeof(double)), value);
        writer.Advance(sizeof(double));
    }

    static void UInt128Value(ArrayPoolBufferWriter<byte> writer, UInt128 value) {
        System.Buffers.Binary.BinaryPrimitives.WriteUInt128BigEndian(writer.GetSpan(16), value);
        writer.Advance(16);
    }
}
```
