# [RASM_FABRICATION_IMPLICIT]

The implicit additive owner: ONE `Implicit.Voxelize` rail over the PicoGK sidecar-native kernel, absorbing conformal TPMS/gyroid/cellular infill, `Lattice`-realized support scaffolds, shell/offset lightweighting, and grayscale/`.cli` layer egress without widening `FabricationResult`. `ImplicitOp` is the full discriminant: field families, lattice scaffold, morphology, and layer-stack modes land as cases and rows on this surface. `Voxels` stays a plane-local native handle. Cross-plane material moves through `VoxelWire`, the content-keyed mesh-to-voxel seam, and machine egress moves through `CliStack` with `ContentKey.Of(EgressKind.Cli, bytes)`. The Rhino plugin domain never references PicoGK; Verify/removal and mesh consumers compose the same wire and never mint a second native posture.

Wire posture: HOST-LOCAL. `Voxels` never crosses a result case, browser wire, or peer boundary; `ContentKey` and layer counts are the only egress payloads.

## [01]-[INDEX]

- [01]-[IMPLICIT]: owns `ImplicitOp`, `ImplicitCell`, `VoxelMorphology`, `CliMode`, `ImplicitPolicy`, `VoxelWire`, `CliStack`, the `VoxelRuntime` `Library` owner, and the two public entries `Implicit.Voxelize(ImplicitOp)` plus `Implicit.Cli(Voxels, ImplicitPolicy)` over PicoGK `IImplicit`/`Voxels`/`Lattice`/`CliIo`/`Vdb2Cli`.

## [02]-[IMPLICIT]

- Owner: `ImplicitOp` the additive voxel discriminant (`Tpms` · `Gyroid` · `Cellular` · `LatticeSupport` · `ShellOffset` · `Grayscale` · `CliVector`); `ImplicitCell`, `VoxelMorphology`, and `CliMode` the field, morphology, and egress rows; `ImplicitPolicy` the typed budget and raster extent carrier; `VoxelWire` the content-keyed mesh-to-`Voxels` seam; `CliStack` the egress receipt; `Implicit` the static surface.
- Cases: `Tpms`/`Gyroid`/`Cellular` rasterize `IImplicit` fields with `Voxels(IImplicit, BBox3)` and intersect the envelope; `LatticeSupport` maps `SupportPlan.Tree` to `Lattice.AddBeam`, `Voxels(Lattice)`, and `BoolSubtract`; `ShellOffset` runs mutating morphology verbs; `Grayscale` uses `imgAllocateSlice` + `GetVoxelSlice`; `CliVector` uses `oVectorize`; `vdb-cli` uses `Vdb2Cli.Convert`.
- Entry: `public static Fin<Voxels> Voxelize(ImplicitOp op)` and `public static CliStack Cli(Voxels voxels, ImplicitPolicy policy)` — the first owns all voxel-producing cases and routes `FabricationFault.VoxelFault(op, budget)` 2715; the second routes the policy's `CliMode.Build` row over a finished plane-local `Voxels`, so grayscale masks, layer height, and VDB-CLI requests ride the SAME policy the voxels were built under.
- Auto: `Voxelize` guards `VoxelBudget`, resolves the ONE ambient PicoGK `Library` from `VoxelBudget.VoxelSize` through the `VoxelRuntime` owner BEFORE any field constructor runs (a size change re-binds a fresh `Library`; the two native exceptions lower onto the rail as `VoxelFault`), dispatches the union, and returns live `Voxels` only inside the additive plane as a plane-local lease the consumer disposes after wire egress. Field cases instantiate `PeriodicImplicit`, rasterize, intersect the content-keyed envelope, and apply morphology. Support cases consume `TreeNode` graphs; egress cases call `CliMode.Build`, mint `CliStack`, and return source voxels for verify/removal composition.
- Receipt: `CliStack` carries the vector-layer count, the `.cli` `ContentKey`, and grayscale mask keys; `VoxelWire` carries the mesh/voxel content key and conversion functions; `FabricationResult.AdditiveResult` receives only moves, layer count, and `Seq<ContentKey>`, never `Voxels`, `Lattice`, `Mesh`, `OpenVdbFile`, or `Library`.
- Packages: `api-picogk.md` (`IImplicit`, `Voxels`, `Lattice`, `Mesh`, `OpenVdbFile`, `PolySliceStack`, `CliIo`, `Vdb2Cli`, `ImageGrayScale`, `Library`, `BBox3`), `Process/owner#FABRICATION_OWNER` (`EgressKind.Cli`, `ContentKey.Of`, `FabricationResult.AdditiveResult`), `Process/faults#FAULT_BAND` (`VoxelFault` 2715, `VoxelBudget`), `Additive/support#SUPPORT` (`TreeNode`, `SupportPlan`), `Query/cache#ARTIFACT_BLOB_INDEX` (`cli` artifact enrollment through content key), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new implicit field is one `ImplicitOp` case plus one `FieldKind` row in `PeriodicImplicit.fSignedDistance`; a new voxel morphology is one `VoxelMorphology` row; a new layer egress is one `CliMode` row; a new source/sink form is one `VoxelWire` adapter; zero new entrypoint, zero result widening, zero Rhino-domain PicoGK reference.
- Boundary: PicoGK remains the net9 sidecar-native ALC firebreak and a plugin-domain reference is the deleted form; `Voxels` is plane-local and a result payload is the named ruling-5 defect; the content-keyed mesh-to-voxel seam is the only Verify/removal bridge; `ContentKey.Of(EgressKind.Cli, ...)` is the only `.cli`/mask identity mint; `Support` owns tree search and planar support algebra; `Scan` owns LPBF metal vectors; this page owns resin/powder voxel grayscale and `.cli` stacks.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------------------------------------------------------------
using System.Buffers;
using System.Buffers.Binary;
using System.Numerics;
using LanguageExt;
using LanguageExt.Common;
using PicoGK;
using Rasm.Fabrication.Process;
using Rhino.Geometry;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Additive;

// --- [TYPES] --------------------------------------------------------------------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class VoxelMorphology {
    public static readonly VoxelMorphology Offset = new(
        "offset",
        static (voxels, step) => { voxels.Offset((float)step.PrimaryMm); return unit; });

    public static readonly VoxelMorphology Shell = new(
        "shell",
        static (voxels, step) => { voxels.Shell((float)step.PrimaryMm); return unit; });

    public static readonly VoxelMorphology OverOffset = new(
        "over-offset",
        static (voxels, step) => { voxels.OverOffset((float)step.PrimaryMm, (float)step.SecondaryMm); return unit; });

    public static readonly VoxelMorphology Smoothen = new(
        "smoothen",
        static (voxels, step) => { voxels.Smoothen((float)step.PrimaryMm); return unit; });

    [UseDelegateFromConstructor]
    public partial Unit Apply(Voxels voxels, VoxelMorphologyStep step);
}

[SmartEnum<string>]
public sealed partial class CliMode {
    public static readonly CliMode Grayscale = new("grayscale", GrayscaleStack);
    public static readonly CliMode CliVector = new("cli-vector", VectorStack);
    public static readonly CliMode VdbCli = new("vdb-cli", VdbCliStack);

    [UseDelegateFromConstructor]
    public partial CliStack Build(Voxels voxels, ImplicitPolicy policy);

    static CliStack VectorStack(Voxels voxels, ImplicitPolicy policy) {
        PolySliceStack slices = voxels.oVectorize((float)policy.LayerHeightMm);
        byte[] bytes = ImplicitCanonical.Cli(slices);
        return new CliStack(slices.nCount, ContentKey.Of(EgressKind.Cli, bytes), Seq<ContentKey>());
    }

    static CliStack GrayscaleStack(Voxels voxels, ImplicitPolicy policy) {
        ImageGrayScale image = voxels.imgAllocateSlice(out int sliceCount);
        Seq<ContentKey> masks = toSeq(Enumerable.Range(0, sliceCount)).Map(n => {
            voxels.GetVoxelSlice(n, ref image);
            byte[] bytes = ImplicitCanonical.Image(n, image);
            return ContentKey.Of(EgressKind.Cli, bytes);
        });
        byte[] index = ImplicitCanonical.MaskIndex(masks);
        return new CliStack(sliceCount, ContentKey.Of(EgressKind.Cli, index), masks);
    }

    static CliStack VdbCliStack(Voxels voxels, ImplicitPolicy policy) {
        PolySliceStack slices = voxels.oVectorize((float)policy.LayerHeightMm);
        policy.VdbCli.IfSome(req => Vdb2Cli.Convert(req.VdbPath, (float)policy.LayerHeightMm, req.CliPath, req.FieldName, null));
        byte[] bytes = ImplicitCanonical.Cli(slices);
        return new CliStack(slices.nCount, ContentKey.Of(EgressKind.Cli, bytes), Seq<ContentKey>());
    }
}

// --- [MODELS] -------------------------------------------------------------------------------------------------------------------------------------
public readonly record struct ImplicitCell(double CellMm, double WallMm, double Density, Vector3 Axis);

public readonly record struct VoxelMorphologyStep(VoxelMorphology Morphology, double PrimaryMm, double SecondaryMm = 0.0);

public readonly record struct VdbCliRequest(string VdbPath, string FieldName, string CliPath);

public sealed record ImplicitPolicy(
    VoxelBudget Budget,
    BBox3 PicoBounds,
    double LayerHeightMm,
    CliMode Cli,
    Seq<VoxelMorphologyStep> Morphology,
    Option<VdbCliRequest> VdbCli = default);

public sealed record VoxelWire(
    ContentKey Key,
    Func<Fin<Voxels>> ToVoxels,
    Func<Voxels, Fin<ContentKey>> FromVoxels);

public sealed record CliStack(int Layers, ContentKey Key, Seq<ContentKey> Masks);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ImplicitOp {
    private ImplicitOp() { }

    public sealed record Tpms(ImplicitCell Cell, VoxelWire Envelope, ImplicitPolicy Policy) : ImplicitOp;
    public sealed record Gyroid(ImplicitCell Cell, VoxelWire Envelope, ImplicitPolicy Policy) : ImplicitOp;
    public sealed record Cellular(ImplicitCell Cell, VoxelWire Envelope, ImplicitPolicy Policy) : ImplicitOp;
    public sealed record LatticeSupport(SupportPlan Support, Option<VoxelWire> Part, ImplicitPolicy Policy) : ImplicitOp;
    public sealed record ShellOffset(VoxelWire Source, Seq<VoxelMorphologyStep> Steps, ImplicitPolicy Policy) : ImplicitOp;
    public sealed record Grayscale(VoxelWire Source, ImplicitPolicy Policy) : ImplicitOp;
    public sealed record CliVector(VoxelWire Source, ImplicitPolicy Policy) : ImplicitOp;

    public ImplicitPolicy Policy =>
        Switch(
            tpms:           static x => x.Policy,
            gyroid:         static x => x.Policy,
            cellular:       static x => x.Policy,
            latticeSupport: static x => x.Policy,
            shellOffset:    static x => x.Policy,
            grayscale:      static x => x.Policy,
            cliVector:      static x => x.Policy);
}

// --- [SERVICES] -----------------------------------------------------------------------------------------------------------------------------------
// The ONE PicoGK runtime owner: Voxelize resolves the ambient Library from VoxelBudget.VoxelSize BEFORE any
// parameterless field ctor runs, so every Voxels/Lattice/Mesh in the fold binds the budget's resolution — a
// different budget re-binds a fresh Library (changing voxel size means a new Library, the catalog lifecycle law)
// and mixed-resolution fields are unconstructable inside the additive plane. No sibling page touches Library.
static class VoxelRuntime {
    // Library binds float voxel sizes, so the same-budget test tolerates double round-trip noise — a
    // double.Epsilon compare is exact equality and re-binds the Library on every ambient wobble.
    const double SizeTolerance = 1e-9;

    static readonly Atom<Option<double>> Bound = Atom(Option<double>.None);

    public static Fin<Unit> Resolve(VoxelBudget budget) =>
        Bound.Value.Filter(size => Math.Abs(size - budget.VoxelSize) < SizeTolerance).Match(
            Some: static _ => Fin.Succ(unit),
            None: () => Rebind(budget.VoxelSize));

    // PicoGKAllocException / PicoGKLibraryMismatchException lower onto the rail here — the two native fault doors.
    static Fin<Unit> Rebind(double voxelSizeMm) =>
        Try(() => {
                Library.GlobalInstance((float)voxelSizeMm);
                Bound.Swap(_ => Some(voxelSizeMm));
                return unit;
            })
            .ToFin();
}

// --- [OPERATIONS] ---------------------------------------------------------------------------------------------------------------------------------
public static class Implicit {
    // The returned live Voxels is a PLANE-LOCAL LEASE: the additive/verify consumer disposes it after its wire
    // egress (FromVoxels / Cli) — intermediates dispose inside the fold, the lease is the one surviving handle.
    public static Fin<Voxels> Voxelize(ImplicitOp op) =>
        op.Switch(
            tpms:           static x => Guard(x, x.Policy, () => Raster(x, new PeriodicImplicit(FieldKind.Tpms, x.Cell, x.Policy.PicoBounds), x.Envelope)),
            gyroid:         static x => Guard(x, x.Policy, () => Raster(x, new PeriodicImplicit(FieldKind.Gyroid, x.Cell, x.Policy.PicoBounds), x.Envelope)),
            cellular:       static x => Guard(x, x.Policy, () => Raster(x, new PeriodicImplicit(FieldKind.Cellular, x.Cell, x.Policy.PicoBounds), x.Envelope)),
            latticeSupport: static x => Guard(x, x.Policy, () => Lattice(x.Support, x.Part, x.Policy)),
            shellOffset:    static x => Guard(x, x.Policy, () => x.Source.ToVoxels().Bind(v => Morph(v, x.Steps))),
            grayscale:      static x => Guard(x, x.Policy, () => x.Source.ToVoxels().Map(v => { CliMode.Grayscale.Build(v, x.Policy); return v; })),
            cliVector:      static x => Guard(x, x.Policy, () => x.Source.ToVoxels().Map(v => { CliMode.CliVector.Build(v, x.Policy); return v; })));

    // The .cli egress rides the policy's CliMode row — grayscale masks, vector stacks, and VDB-CLI all through ONE
    // name; mode, layer height, and VDB request come from the SAME ImplicitPolicy the voxels were built under, so a
    // fixed default vectorization that drops the selected mode is unconstructable.
    public static CliStack Cli(Voxels voxels, ImplicitPolicy policy) =>
        policy.Cli.Build(voxels, policy);

    static Fin<Voxels> Guard(ImplicitOp op, ImplicitPolicy policy, Func<Fin<Voxels>> run) =>
        policy.Budget.VoxelSize <= 0.0 || policy.Budget.VoxelCap <= 0
            ? Fail(op, policy)
            : VoxelRuntime.Resolve(policy.Budget)
                .MapFail(_ => FabricationFault.VoxelFault(op, policy.Budget).ToError())
                .Bind(_ => run())
                .Bind(v => WithinBudget(v, op, policy));

    static Fin<Voxels> Raster(ImplicitOp op, IImplicit field, VoxelWire envelope) {
        Voxels voxels = new(field, op.Policy.PicoBounds);
        return envelope.ToVoxels().Bind(bound => {
            voxels.BoolIntersect(bound);
            bound.Dispose();
            return Morph(voxels, op.Policy.Morphology);
        });
    }

    static Fin<Voxels> Lattice(SupportPlan support, Option<VoxelWire> part, ImplicitPolicy policy) {
        using PicoGK.Lattice lattice = new();
        foreach (TreeNode node in support.Tree.Filter(static n => n.Parent >= 0)) {
            TreeNode parent = support.Tree.Find(p => p.Id == node.Parent).IfNone(node);
            lattice.AddBeam(ToVector(parent.At), (float)parent.Radius, ToVector(node.At), (float)node.Radius, bRoundCap: true);
        }
        Voxels scaffold = new(lattice);
        // Released rides the fail rail: a ToVoxels short-circuit before Subtract consumes the scaffold
        // must release the plane-local lease, never leak the native field.
        Fin<Voxels> clipped = part
            .Map(w => w.ToVoxels().Bind(model => Subtract(scaffold, model)).MapFail(error => Released(scaffold, error)))
            .IfNone(Fin.Succ(scaffold));
        return clipped.Bind(v => Morph(v, policy.Morphology));
    }

    static Fin<Voxels> Morph(Voxels voxels, Seq<VoxelMorphologyStep> steps) {
        steps.Iter(step => step.Morphology.Apply(voxels, step));
        return Fin.Succ(voxels);
    }

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
[SmartEnum<string>]
public sealed partial class FieldKind {
    public static readonly FieldKind Tpms = new("tpms");
    public static readonly FieldKind Gyroid = new("gyroid");
    public static readonly FieldKind Cellular = new("cellular");
}

public sealed class PeriodicImplicit(FieldKind kind, ImplicitCell cell, BBox3 bounds) : IImplicit, IBoundedImplicit {
    public BBox3 oBounds => bounds;

    public float fSignedDistance(in Vector3 vec) {
        float scale = (float)(Math.Tau / Math.Max(cell.CellMm, 1e-6));
        float x = vec.X * scale;
        float y = vec.Y * scale;
        float z = vec.Z * scale;
        float gyroid = MathF.Sin(x) * MathF.Cos(y) + MathF.Sin(y) * MathF.Cos(z) + MathF.Sin(z) * MathF.Cos(x);
        float schwarz = MathF.Cos(x) + MathF.Cos(y) + MathF.Cos(z);
        float cellular = MathF.Min(MathF.Abs(MathF.Sin(x) * MathF.Sin(y)), MathF.Abs(MathF.Sin(y) * MathF.Sin(z)));
        float value =
            kind == FieldKind.Gyroid ? gyroid :
            kind == FieldKind.Cellular ? cellular :
            schwarz;
        return value - (float)cell.WallMm;
    }
}

// --- [CANONICAL_BYTES] ----------------------------------------------------------------------------------------------------------------------------
public static class ImplicitCanonical {
    public static byte[] Cli(PolySliceStack slices) {
        ArrayBufferWriter<byte> writer = new();
        Write(writer, slices.nCount);
        for (int n = 0; n < slices.nCount; n++) {
            PolySlice slice = slices.oSliceAt(n);
            Write(writer, (double)slice.fZPos);
            Write(writer, slice.bIsEmpty() ? 0 : 1);
        }
        return writer.WrittenSpan.ToArray();
    }

    public static byte[] Image(int n, ImageGrayScale image) {
        _ = image;
        ArrayBufferWriter<byte> writer = new();
        Write(writer, n);
        return writer.WrittenSpan.ToArray();
    }

    public static byte[] MaskIndex(Seq<ContentKey> masks) {
        ArrayBufferWriter<byte> writer = new();
        Write(writer, masks.Count);
        masks.Iter(key => Write(writer, key.Digest));
        return writer.WrittenSpan.ToArray();
    }

    static void Write(ArrayBufferWriter<byte> writer, int value) {
        BinaryPrimitives.WriteInt32LittleEndian(writer.GetSpan(4), value);
        writer.Advance(4);
    }

    static void Write(ArrayBufferWriter<byte> writer, double value) {
        BinaryPrimitives.WriteDoubleLittleEndian(writer.GetSpan(8), value);
        writer.Advance(8);
    }

    static void Write(ArrayBufferWriter<byte> writer, UInt128 value) {
        BinaryPrimitives.WriteUInt128BigEndian(writer.GetSpan(16), value);
        writer.Advance(16);
    }
}
```
