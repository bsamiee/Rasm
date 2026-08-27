# [RASM_RHINO_MODELING_SUBD]

`SubDs.Build` owns SubD construction, surface-point interpolation, value-semantic editing, crease topology, edge extraction, and Brep egress. `SubDOp` admits raw handles once through the spine's `ModelClaim` fold, every policy enters through a generated owner, and every product exits through the spine's custody carrier.

`MeshOp.QuadRemesh` feeds `SubDOp.FromMesh`, `SweepFrameLaw` (`Modeling/lofting.md`) carries one-rail framing directly on `SubDOp.FromSweepOne`, and the spine's `ModelGate` comes from `Modeling/solids.md` while `ModelClaim` comes from `Modeling/curves.md`. Every handle here carries DETACHED geometry, which decides which host members this pipeline reaches at all.

## [01]-[INDEX]

- [02]-[ADMISSION]: `CreasePreset`, the native-projection rows, `SubDCreationSpec`/`SubDCreationLaw`, `SubDBrepLaw`, the two edge capability vocabularies, and `SubDEdgeSelection`.
- [03]-[ALGEBRA]: `SubDEditVerb`, `SubDOp`, and the `SubDs.Build` entry.

## [02]-[ADMISSION]

- Owner: `CreasePreset` — the four host creation presets behind one mint; `SubDClosure`, `SubDCorners`, `SubDLoftFeatures`, `SubDJoinPolicy`, `SubDFacePacking`, `SubDVertexInterpolation`, `SubDEdgeClamp` — the native-projection rows; `SubDCreationSpec` and `SubDCreationLaw` — the mesh-conversion policy and its preset-or-custom discriminant; `SubDBrepLaw` — the Brep-egress policy; `SubDEdgeScope` and `SubDEdgeCharacter` — the two edge-filter capability vocabularies; `SubDEdgeSelection` — the admitted edge filter.
- Law: a policy carrier owns its own disposal — both `Rig` members answer `Fin<Lease<T>>` through `Lease.Acquire`, so a throwing host mint funnels onto the carrier at the acquisition and the consuming arm's fallible `Use` releases on every exit path with the cleanup fault aggregated into the primary. `using` inside the arm is the retired form: when the body faults AND `Dispose` faults, the language keeps the disposal exception and drops the one that named the failure.
- Law: a two-state modality carrying a HOST projection column is a row, and `Modeling/curves.md`'s posture law settles it for this page — `SubDClosure`, `SubDCorners`, `SubDFacePacking`, `SubDVertexInterpolation`, and `SubDEdgeClamp` each name both corners at a call whose bare `true` carries its meaning nowhere, while `SubDLoftFeatures` and `SubDJoinPolicy` render a host PAIR from one name. `SubDEditVerb.Shell.Solid` carries no host projection column and no correlated partner, so it stays a named `bool` on its owning case.
- Law: the edge filter is TWO capability sets, never two exclusive options — `DuplicateEdgeCurves(boundaryOnly, interiorOnly, smoothOnly, sharpOnly, creaseOnly, clampEnds)` reads six INDEPENDENT filter bits, so a single-choice location beside a single-choice character cannot ask for boundary AND interior edges or for smooth AND crease edges, and refuses combinations the host serves. Each axis carries its own `CapabilityLaw.Forbidden(None)` because the empty corner is the one illegal one: six `false` flags let the native answer nothing but an empty spread.
- Law: a host enum with NO Rasm projection crosses as itself and is admitted for definedness at the boundary; one that drives a Rasm decision or renders a host column takes a row. `SubDFromSurfaceMethods`, `SubDVertexTag`, `SubDEdgeTag`, `SubDEndCapStyle`, `SubDComponentLocation`, and the four `SubDCreationOptions` option enums are the first form and reach `ModelClaim.Admits` as named definedness axes; every `Native` row on this page is the second.
- Law: every owner answers ONE admission fold — the generated factory hook and `IsValid` read the same static `Admits`, so an invalid instance is unconstructible and no consumer re-tests what construction already proved. Scalar, count, and coordinate claims are the kernel's rows; the spread claims are the spine's `ModelClaim` rows.
- Growth: a new creation knob is one `SubDCreationSpec` column with its claim; a new edge filter bit is one capability row the projection reads by name.
- Packages: RhinoCommon meshing (`.api/api-rhinocommon-meshing.md` — `SubD` construction `:255-266`, `SubD` edit `:273-282`, `SubD` topology `:307-314`, `SubD` config `:316-328`), kernel `Domain/results` (`Lease<T>.Acquire`/`Use`, `ValidityClaim`, `IValidityEvidence`, `Fin`), kernel `Domain/validation` (`ICapability`, `CapabilitySet`, `CapabilityLaw`), `Modeling/curves.md` (`ModelClaim`), Thinktecture.Runtime.Extensions, LanguageExt.Core.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System;
using System.Runtime.InteropServices;
using Rasm.Domain;
using Rasm.Rhino.Document;
using Rhino.Geometry;

namespace Rasm.Rhino.Modeling;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class CreasePreset {
    public static readonly CreasePreset Smooth = new(key: 0, static () => SubDCreationOptions.Smooth);
    public static readonly CreasePreset InteriorCreases = new(key: 1, static () => SubDCreationOptions.InteriorCreases);
    public static readonly CreasePreset ConvexCorners = new(key: 2, static () => SubDCreationOptions.ConvexCornersAndInteriorCreases);
    public static readonly CreasePreset AllCorners = new(key: 3, static () => SubDCreationOptions.ConvexAndConcaveCornersAndInteriorCreases);

    [UseDelegateFromConstructor]
    internal partial SubDCreationOptions Mint();
}

[SmartEnum<int>]
public sealed partial class SubDClosure {
    public static readonly SubDClosure Open = new(key: 0, native: false);
    public static readonly SubDClosure Closed = new(key: 1, native: true);

    internal bool Native { get; }
}

[SmartEnum<int>]
public sealed partial class SubDCorners {
    public static readonly SubDCorners Smooth = new(key: 0, native: false);
    public static readonly SubDCorners Cornered = new(key: 1, native: true);

    internal bool Native { get; }
}

[SmartEnum<int>]
public sealed partial class SubDLoftFeatures {
    public static readonly SubDLoftFeatures Smooth = new(key: 0, native: (false, false));
    public static readonly SubDLoftFeatures Cornered = new(key: 1, native: (true, false));
    public static readonly SubDLoftFeatures Creased = new(key: 2, native: (false, true));
    public static readonly SubDLoftFeatures CorneredAndCreased = new(key: 3, native: (true, true));

    internal (bool Corners, bool Creases) Native { get; }
}

[SmartEnum<int>]
public sealed partial class SubDJoinPolicy {
    public static readonly SubDJoinPolicy Smooth = new(key: 0, native: (false, true));
    public static readonly SubDJoinPolicy Creased = new(key: 1, native: (true, true));
    public static readonly SubDJoinPolicy SmoothAsymmetric = new(key: 2, native: (false, false));
    public static readonly SubDJoinPolicy CreasedAsymmetric = new(key: 3, native: (true, false));

    internal (bool Creases, bool PreserveSymmetry) Native { get; }
}

[SmartEnum<int>]
public sealed partial class SubDFacePacking {
    public static readonly SubDFacePacking Unpacked = new(key: 0, native: false);
    public static readonly SubDFacePacking Packed = new(key: 1, native: true);

    internal bool Native { get; }
}

[SmartEnum<int>]
public sealed partial class SubDVertexInterpolation {
    public static readonly SubDVertexInterpolation ControlNet = new(key: 0, native: false);
    public static readonly SubDVertexInterpolation LimitSurface = new(key: 1, native: true);

    internal bool Native { get; }
}

[SmartEnum<int>]
public sealed partial class SubDEdgeClamp {
    public static readonly SubDEdgeClamp Natural = new(key: 0, native: false);
    public static readonly SubDEdgeClamp Clamped = new(key: 1, native: true);

    internal bool Native { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SubDEdgeScope : ICapability<SubDEdgeScope> {
    public static readonly SubDEdgeScope Boundary = new(key: "boundary");
    public static readonly SubDEdgeScope Interior = new(key: "interior");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SubDEdgeCharacter : ICapability<SubDEdgeCharacter> {
    public static readonly SubDEdgeCharacter Smooth = new(key: "smooth");
    public static readonly SubDEdgeCharacter Sharp = new(key: "sharp");
    public static readonly SubDEdgeCharacter Crease = new(key: "crease");
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SubDCreationLaw : IValidityEvidence {
    private SubDCreationLaw() { }
    public sealed record Preset(CreasePreset Row) : SubDCreationLaw;
    public sealed record Custom(SubDCreationSpec Value) : SubDCreationLaw;

    public bool IsValid => Switch(
        preset: static law => (ValidityClaim)(law.Row is not null),
        custom: static law => (ValidityClaim)law.Value.IsValid);

    internal Fin<Lease<SubDCreationOptions>> Rig() =>
        Lease<SubDCreationOptions>.Acquire(
            mint: () => Switch(
                preset: static law => law.Row.Mint(),
                custom: static law => new SubDCreationOptions {
                    InteriorCreaseTest = law.Value.InteriorCrease,
                    ConvexCornerTest = law.Value.ConvexCorner,
                    ConcaveCornerTest = law.Value.ConcaveCorner,
                    TextureCoordinateTest = law.Value.TextureCoordinates,
                    MaximumConvexCornerEdgeCount = law.Value.MaximumConvexCornerEdgeCount,
                    MaximumConvexCornerAngleRadians = law.Value.MaximumConvexCornerAngleRadians,
                    MinimumConcaveCornerAngleRadians = law.Value.MinimumConcaveCornerAngleRadians,
                    MinimumConcaveCornerEdgeCount = law.Value.MinimumConcaveCornerEdgeCount,
                    InterpolateMeshVertices = law.Value.VertexInterpolation.Native,
                }));
}

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct SubDCreationSpec : IValidityEvidence {
    public SubDCreationOptions.InteriorCreaseOption InteriorCrease { get; }
    public SubDCreationOptions.ConvexCornerOption ConvexCorner { get; }
    public SubDCreationOptions.ConcaveCornerOption ConcaveCorner { get; }
    public SubDCreationOptions.TextureCoordinateOption TextureCoordinates { get; }
    public uint MaximumConvexCornerEdgeCount { get; }
    public double MaximumConvexCornerAngleRadians { get; }
    public double MinimumConcaveCornerAngleRadians { get; }
    public uint MinimumConcaveCornerEdgeCount { get; }
    public SubDVertexInterpolation VertexInterpolation { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref SubDCreationOptions.InteriorCreaseOption interiorCrease,
        ref SubDCreationOptions.ConvexCornerOption convexCorner,
        ref SubDCreationOptions.ConcaveCornerOption concaveCorner,
        ref SubDCreationOptions.TextureCoordinateOption textureCoordinates,
        ref uint maximumConvexCornerEdgeCount,
        ref double maximumConvexCornerAngleRadians,
        ref double minimumConcaveCornerAngleRadians,
        ref uint minimumConcaveCornerEdgeCount,
        ref SubDVertexInterpolation vertexInterpolation) {
        if (!Admits(
            interiorCrease, convexCorner, concaveCorner, textureCoordinates,
            maximumConvexCornerEdgeCount, maximumConvexCornerAngleRadians,
            minimumConcaveCornerAngleRadians, minimumConcaveCornerEdgeCount, vertexInterpolation)) {
            validationError = new ValidationError("SubD corner thresholds are inconsistent.");
        }
    }

    public bool IsValid => Admits(
        InteriorCrease, ConvexCorner, ConcaveCorner, TextureCoordinates,
        MaximumConvexCornerEdgeCount, MaximumConvexCornerAngleRadians,
        MinimumConcaveCornerAngleRadians, MinimumConcaveCornerEdgeCount, VertexInterpolation);

    private static bool Admits(
        SubDCreationOptions.InteriorCreaseOption interiorCrease,
        SubDCreationOptions.ConvexCornerOption convexCorner,
        SubDCreationOptions.ConcaveCornerOption concaveCorner,
        SubDCreationOptions.TextureCoordinateOption textureCoordinates,
        uint maximumConvexCornerEdgeCount,
        double maximumConvexCornerAngleRadians,
        double minimumConcaveCornerAngleRadians,
        uint minimumConcaveCornerEdgeCount,
        SubDVertexInterpolation? vertexInterpolation) =>
        ValidityClaim.All(
            Enum.IsDefined(interiorCrease),
            Enum.IsDefined(convexCorner),
            Enum.IsDefined(concaveCorner),
            Enum.IsDefined(textureCoordinates),
            vertexInterpolation is not null,
            maximumConvexCornerEdgeCount >= 2u,
            minimumConcaveCornerEdgeCount >= 3u,
            ValidityClaim.Positive(value: maximumConvexCornerAngleRadians),
            ValidityClaim.Finite(value: minimumConcaveCornerAngleRadians),
            minimumConcaveCornerAngleRadians > maximumConvexCornerAngleRadians);
}

[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct SubDBrepLaw : IValidityEvidence {
    public SubDFacePacking Packing { get; }
    public SubDToBrepOptions.ExtraordinaryVertexProcessOption VertexProcess { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref SubDFacePacking packing,
        ref SubDToBrepOptions.ExtraordinaryVertexProcessOption vertexProcess) {
        if (!Admits(packing, vertexProcess)) {
            validationError = new ValidationError("SubD Brep policy is invalid.");
        }
    }

    public bool IsValid => Admits(Packing, VertexProcess);

    internal Fin<Lease<SubDToBrepOptions>> Rig() =>
        Lease<SubDToBrepOptions>.Acquire(
            mint: () => new SubDToBrepOptions(packFaces: Packing.Native, vertexProcess: VertexProcess));

    private static bool Admits(
        SubDFacePacking? packing,
        SubDToBrepOptions.ExtraordinaryVertexProcessOption vertexProcess) =>
        ValidityClaim.All(packing is not null, Enum.IsDefined(vertexProcess));
}

[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct SubDEdgeSelection : IValidityEvidence {
    private static readonly CapabilityLaw<SubDEdgeScope> ScopeLaw =
        CapabilityLaw<SubDEdgeScope>.Forbidden(barred: Seq(CapabilitySet<SubDEdgeScope>.None));
    private static readonly CapabilityLaw<SubDEdgeCharacter> CharacterLaw =
        CapabilityLaw<SubDEdgeCharacter>.Forbidden(barred: Seq(CapabilitySet<SubDEdgeCharacter>.None));

    public CapabilitySet<SubDEdgeScope> Scopes { get; }
    public CapabilitySet<SubDEdgeCharacter> Characters { get; }
    public SubDEdgeClamp Ends { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref CapabilitySet<SubDEdgeScope> scopes,
        ref CapabilitySet<SubDEdgeCharacter> characters,
        ref SubDEdgeClamp ends) {
        if (!Admits(scopes, characters, ends)) {
            validationError = new ValidationError(
                "SubD edge selection needs at least one scope and one character; all-false host flags select nothing.");
        }
    }

    public bool IsValid => Admits(Scopes, Characters, Ends);

    internal (bool Boundary, bool Interior, bool Smooth, bool Sharp, bool Crease, bool Clamp) Native => (
        Scopes.Admits(capability: SubDEdgeScope.Boundary),
        Scopes.Admits(capability: SubDEdgeScope.Interior),
        Characters.Admits(capability: SubDEdgeCharacter.Smooth),
        Characters.Admits(capability: SubDEdgeCharacter.Sharp),
        Characters.Admits(capability: SubDEdgeCharacter.Crease),
        Ends.Native);

    private static bool Admits(
        CapabilitySet<SubDEdgeScope> scopes,
        CapabilitySet<SubDEdgeCharacter> characters,
        SubDEdgeClamp? ends) =>
        ValidityClaim.All(
            ScopeLaw.Admit(held: scopes).IsSucc,
            CharacterLaw.Admit(held: characters).IsSucc,
            ends is not null);
}
```

## [03]-[ALGEBRA]

- Owner: `SubDEditVerb` `[Union]` — the value-semantic edit algebra, each verb carrying its own arm; `SubDOp` `[Union]` — the sole construction algebra; `SubDs` — the one entry folding any operation spread into one product sequence.
- Law: `SubDSurfaceInterpolator` is UNREACHABLE from this pipeline, so no scoped-interpolation family stands here. All four factories set `ContextId` from `subd.ParentRhinoObject().Id`, and `GeometryBase.Duplicate` mints its copy with a null parent, so every value-semantic edit — duplicating by construction — meets a null dereference before the solver exists; `SubD.InterpolateSurfacePoints(uint[], Point3d[])` builds that same solver internally and fails identically. `SubD.InterpolateSurfacePoints(Point3d[])` reaches the native directly and is the one detached-safe interpolation, so `Interpolate` runs whole-surface and gates its arity against the live vertex roster. `SetVertexPoint` remains the one id-addressed surface-point write, and RhinoCommon rules it unsuited to topologically near vertices, which is why no batch sibling stands beside it.
- Law: each edit verb carries its OWN operation key — a refusal on `TagEdges` and a refusal on `Pack` reported one indistinguishable `Edit` key, so `` on the verb roster names the verb in the fault while the borrow window stays keyed to `Edit`.
- Law: id space and index space never mix — `uint` is a host component ID (`SetVertexSurfacePoint`) and `int` is an offset into a live roster (`Subdivide(faceIndices)`, `SetVertexTags`, `SetEdgeTags`), while `ComponentIndex` is its own third space. No id takes an index bound, because `Vertices.Count` both refuses live ids above the roster length and admits dead ones below it, so the host's own `bool` is the only sound id gate; an index DOES take one, and that bound is the arm's guard against the live roster.
- Law: quad-remesh composition feeds the meshing pipeline's `QuadRemesh` product to `FromMesh`; no second remesh entry exists here, and one- and two-rail sweeps are direct `SubDOp` cases because their admission, identity, timing, and consumer coincide.
- Law: admission NAMES its axis — `Admitted` dispatches the generated `Switch` into the spine's `ModelClaim.Admits`, so a request breaching several constraints answers one keyed fault per breached axis and a new case breaks the compile instead of falling through a catch-all to a silent refusal.
- Law: `SubDs.Build` is `ModelGate.Entry` — capture, the non-empty guard, accumulating admission, and the custody-safe product fold remain folder-spine concerns. Construction arms own fresh geometry, the edit arm duplicates exactly once and rolls the duplicate back on failure, and the extraction arm detaches edge curves.
- Growth: a new subd constructor is one `SubDOp` case with its arm; a new edit is one `SubDEditVerb` case with its arm and its generated key.
- Packages: RhinoCommon meshing (`.api/api-rhinocommon-meshing.md` — `SubD` construction `:255-266` incl. `CreateFromMesh`/`CreateFromSurface`/`CreateFromLoft`/`CreateFromSweep`/`CreateQuadSphere`/`CreateGlobeSphere`/`CreateTriSphere`/`CreateIcosahedron`/`CreateFromCylinder`/`JoinSubDs`, `SubD` edit `:273-282` incl. `Subdivide`/`Offset`/`ToBrep`/`InterpolateSurfacePoints`/`MergeAllCoplanarFaces`/`PackFaces`/`Flip`/`TransformComponents`/`SetVertexSurfacePoint`/`UpdateAllTagsAndSectorCoefficients`/`UpdateSurfaceMeshCache`, `SubD` topology `:307-314` incl. `DuplicateEdgeCurves`/`SetVertexTags`/`SetEdgeTags`), `Modeling/lofting.md` (`SweepFrameLaw`), `Modeling/curves.md` (`ModelClaim`), `Modeling/solids.md` (`ModelGate`), kernel `Domain/results` (`Admit.Confirm`, `Try.lift`, `Lease<T>.Use`, `Fin`), kernel `Domain/context` (`Context.Absolute`, `Context.Angle`), Thinktecture.Runtime.Extensions, LanguageExt.Core.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SubDEditVerb : IValidityEvidence {
    private SubDEditVerb() { }
    public sealed record SubdivideAll(int Count = 1) : SubDEditVerb;
    public sealed record SubdivideFaces(Seq<int> Faces) : SubDEditVerb;
    public sealed record Interpolate(Seq<Point3d> SurfacePoints) : SubDEditVerb;
    public sealed record SetVertexPoint(uint VertexId, Point3d SurfacePoint) : SubDEditVerb;
    public sealed record Shell(double Distance, bool Solid) : SubDEditVerb;
    public sealed record MergeCoplanar : SubDEditVerb;
    public sealed record Pack : SubDEditVerb;
    public sealed record Flip : SubDEditVerb;
    public sealed record TagVertices(Seq<int> Vertices, SubDVertexTag Tag) : SubDEditVerb;
    public sealed record TagEdges(Seq<int> Edges, SubDEdgeTag Tag) : SubDEditVerb;
    public sealed record MoveComponents(Seq<ComponentIndex> Components, Transform Motion, SubDComponentLocation Location) : SubDEditVerb;

    public bool IsValid => Switch(
        subdivideAll: static edit => ValidityClaim.CountAtLeast(count: edit.Count, floor: 1),
        subdivideFaces: static edit => ModelClaim.Rows(
            rows: edit.Faces, claim: static face => ValidityClaim.CountAtLeast(count: face, floor: 0)),
        interpolate: static edit => ModelClaim.Points(points: edit.SurfacePoints),
        setVertexPoint: static edit => ValidityClaim.Finite(value: edit.SurfacePoint),
        shell: static edit => ValidityClaim.All(ValidityClaim.Finite(value: edit.Distance), edit.Distance != 0.0),
        mergeCoplanar: static () => (ValidityClaim)true,
        pack: static () => (ValidityClaim)true,
        flip: static () => (ValidityClaim)true,
        tagVertices: static edit => ValidityClaim.All(
            ModelClaim.Rows(rows: edit.Vertices, claim: static vertex => ValidityClaim.CountAtLeast(count: vertex, floor: 0)),
            Enum.IsDefined(edit.Tag)),
        tagEdges: static edit => ValidityClaim.All(
            ModelClaim.Rows(rows: edit.Edges, claim: static edge => ValidityClaim.CountAtLeast(count: edge, floor: 0)),
            Enum.IsDefined(edit.Tag)),
        moveComponents: static edit => ValidityClaim.All(
            ValidityClaim.CountAtLeast(count: edit.Components.Count, floor: 1),
            edit.Motion.IsValid && !edit.Motion.IsZero,
            Enum.IsDefined(edit.Location)));

    internal Fin<Seq<GeometryHandle>> Apply(SubD working, Context domain) =>
        Switch(
            (Working: working, Domain: domain),
            subdivideAll: static (ctx, edit) =>
                from _ in Admit.Confirm(success: ctx.Working.Subdivide(count: edit.Count))
                from built in Refreshed(working: ctx.Working)
                select built,
            subdivideFaces: static (ctx, edit) =>
                from _ in guard(edit.Faces.ForAll(face => face < ctx.Working.Faces.Count),
                    new KernelFault.InvalidInput(Axis: Some(nameof(edit.Faces))))
                from __ in Admit.Confirm(success: ctx.Working.Subdivide(faceIndices: edit.Faces.AsIterable()))
                from built in Refreshed(working: ctx.Working)
                select built,
            interpolate: static (ctx, edit) =>
                from _ in guard(edit.SurfacePoints.Count == ctx.Working.Vertices.Count,
                    new KernelFault.InvalidInput(Axis: Some(nameof(edit.SurfacePoints))))
                from __ in Admit.Confirm(
                    success: ctx.Working.InterpolateSurfacePoints(surfacePoints: edit.SurfacePoints.ToArray()))
                from built in Refreshed(working: ctx.Working)
                select built,
            setVertexPoint: static (ctx, edit) =>
                from _ in Admit.Confirm(success: ctx.Working.SetVertexSurfacePoint(
                    vertexIndex: edit.VertexId, surfacePoint: edit.SurfacePoint))
                from built in Refreshed(working: ctx.Working)
                select built,
            shell: static (ctx, edit) => ModelGate.Owned(ctx.Working,
                () => ctx.Working.Offset(distance: edit.Distance, solidify: edit.Solid)),
            mergeCoplanar: static ctx =>
                from _ in Admit.Confirm(success: ctx.Working.MergeAllCoplanarFaces(
                    tolerance: ctx.Domain.Absolute.Value, angleTolerance: ctx.Domain.Angle.Value))
                from built in Refreshed(working: ctx.Working)
                select built,
            pack: static ctx => Try.lift(() => {
                _ = ctx.Working.PackFaces();
                return ModelGate.Kept(ctx.Working);
            }).Run().Bind(static inner => inner),
            flip: static ctx =>
                from _ in Admit.Confirm(success: ctx.Working.Flip())
                from built in Refreshed(working: ctx.Working)
                select built,
            tagVertices: static (ctx, edit) =>
                from _ in guard(edit.Vertices.ForAll(vertex => vertex < ctx.Working.Vertices.Count),
                    new KernelFault.InvalidInput(Axis: Some(nameof(edit.Vertices))))
                from built in Try.lift(() => {
                    ctx.Working.Vertices.SetVertexTags(vertexIndices: edit.Vertices.AsIterable(), tag: edit.Tag);
                    return Refreshed(working: ctx.Working);
                }).Run().Bind(static inner => inner)
                select built,
            tagEdges: static (ctx, edit) =>
                from _ in guard(edit.Edges.ForAll(edge => edge < ctx.Working.Edges.Count),
                    new KernelFault.InvalidInput(Axis: Some(nameof(edit.Edges))))
                from built in Try.lift(() => {
                    ctx.Working.Edges.SetEdgeTags(edgeIndices: edit.Edges.AsIterable(), tag: edit.Tag);
                    return Refreshed(working: ctx.Working);
                }).Run().Bind(static inner => inner)
                select built,
            moveComponents: static (ctx, edit) => Try.lift(() =>
                ctx.Working.TransformComponents(
                        components: edit.Components.AsIterable(), xform: edit.Motion,
                        componentLocation: edit.Location) is uint moved && moved > 0u
                    ? Refreshed(working: ctx.Working)
                    : Fin.Fail<Seq<GeometryHandle>>(
                        error: new KernelFault.InvalidResult(Detail: Some("no addressed component moved")))).Run().Bind(static inner => inner));

    private static Fin<Seq<GeometryHandle>> Refreshed(SubD working) =>
        Try.lift(() => {
            _ = working.UpdateAllTagsAndSectorCoefficients();
            _ = working.UpdateSurfaceMeshCache(lazyUpdate: false);
            return ModelGate.Kept(working);
        }).Run().Bind(static inner => inner);
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SubDOp {
    private SubDOp() { }
    public sealed record FromMesh(GeometryHandle Source, SubDCreationLaw Law) : SubDOp;
    public sealed record FromSurface(GeometryHandle Source, SubDFromSurfaceMethods Method, SubDCorners Corners) : SubDOp;
    public sealed record FromLoft(Seq<GeometryHandle> Shapes, SubDClosure Closure, SubDLoftFeatures Features, int Divisions) : SubDOp;
    public sealed record FromSweepOne(GeometryHandle Rail, SweepFrameLaw Frame, Seq<GeometryHandle> Shapes, SubDClosure Closure, SubDCorners Corners) : SubDOp;
    public sealed record FromSweepTwo(GeometryHandle Rail1, GeometryHandle Rail2, Seq<GeometryHandle> Shapes, SubDClosure Closure, SubDCorners Corners) : SubDOp;
    public sealed record SeedQuadSphere(Sphere Value, SubDComponentLocation VertexLocation, uint SubdivisionLevel) : SubDOp;
    public sealed record SeedGlobeSphere(Sphere Value, SubDComponentLocation VertexLocation, uint AxialFaceCount, uint EquatorialFaceCount) : SubDOp;
    public sealed record SeedTriSphere(Sphere Value, SubDComponentLocation VertexLocation, uint SubdivisionLevel) : SubDOp;
    public sealed record SeedIcosahedron(Sphere Value, SubDComponentLocation VertexLocation) : SubDOp;
    public sealed record SeedCylinder(Cylinder Value, uint CircumferenceFaceCount, uint HeightFaceCount, SubDEndCapStyle EndCap, SubDEdgeTag EndCapEdgeTag, SubDComponentLocation RadiusLocation) : SubDOp;
    public sealed record Join(Seq<GeometryHandle> Targets, SubDJoinPolicy Policy) : SubDOp;
    public sealed record Edit(GeometryHandle Target, SubDEditVerb Verb) : SubDOp;
    public sealed record ToBrep(GeometryHandle Target, SubDBrepLaw Law) : SubDOp;
    public sealed record EdgeCurves(GeometryHandle Target, SubDEdgeSelection Selection) : SubDOp;

    internal Fin<SubDOp> Admitted() =>
        Switch(
            context: key,
            fromMesh: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Source), ModelClaim.Handle(handle: row.Source)), (nameof(row.Law), row.Law is { IsValid: true })),
            fromSurface: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Source), ModelClaim.Handle(handle: row.Source)),
                (nameof(row.Method), Enum.IsDefined(row.Method)), (nameof(row.Corners), row.Corners is not null)),
            fromLoft: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Shapes), ValidityClaim.All(
                    ModelClaim.Handles(handles: row.Shapes),
                    ValidityClaim.CountAtLeast(count: row.Shapes.Count, floor: row.Closure is { Native: true } ? 3 : 1))),
                (nameof(row.Closure), row.Closure is not null),
                (nameof(row.Features), row.Features is not null),
                (nameof(row.Divisions), ValidityClaim.CountAtLeast(count: row.Divisions, floor: 1))),
            fromSweepOne: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Rail), ModelClaim.Handle(handle: row.Rail)),
                (nameof(row.Frame), row.Frame is { IsValid: true }),
                (nameof(row.Shapes), ModelClaim.Handles(handles: row.Shapes)),
                (nameof(row.Closure), row.Closure is not null), (nameof(row.Corners), row.Corners is not null)),
            fromSweepTwo: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Rail1), ModelClaim.Handle(handle: row.Rail1)),
                (nameof(row.Rail2), ModelClaim.Handle(handle: row.Rail2)),
                (nameof(row.Shapes), ModelClaim.Handles(handles: row.Shapes)),
                (nameof(row.Closure), row.Closure is not null), (nameof(row.Corners), row.Corners is not null)),
            seedQuadSphere: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Value), row.Value.IsValid), (nameof(row.VertexLocation), Enum.IsDefined(row.VertexLocation))),
            seedGlobeSphere: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Value), row.Value.IsValid), (nameof(row.VertexLocation), Enum.IsDefined(row.VertexLocation)),
                (nameof(row.AxialFaceCount), row.AxialFaceCount > 0u),
                (nameof(row.EquatorialFaceCount), row.EquatorialFaceCount >= 3u)),
            seedTriSphere: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Value), row.Value.IsValid), (nameof(row.VertexLocation), Enum.IsDefined(row.VertexLocation))),
            seedIcosahedron: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Value), row.Value.IsValid), (nameof(row.VertexLocation), Enum.IsDefined(row.VertexLocation))),
            seedCylinder: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Value), row.Value.IsValid),
                (nameof(row.CircumferenceFaceCount), row.CircumferenceFaceCount >= 3u),
                (nameof(row.HeightFaceCount), row.HeightFaceCount > 0u),
                (nameof(row.EndCap), Enum.IsDefined(row.EndCap)),
                (nameof(row.EndCapEdgeTag), Enum.IsDefined(row.EndCapEdgeTag)),
                (nameof(row.RadiusLocation), Enum.IsDefined(row.RadiusLocation))),
            join: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Targets), ModelClaim.Handles(handles: row.Targets)),
                (nameof(row.Policy), row.Policy is not null)),
            edit: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Target), ModelClaim.Handle(handle: row.Target)), (nameof(row.Verb), row.Verb is { IsValid: true })),
            toBrep: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Target), ModelClaim.Handle(handle: row.Target)), (nameof(row.Law), row.Law.IsValid)),
            edgeCurves: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Target), ModelClaim.Handle(handle: row.Target)), (nameof(row.Selection), row.Selection.IsValid)));

    internal Fin<Seq<GeometryHandle>> Apply(Context domain) =>
        Switch(
            context: domain,
            fromMesh: static (_, edit) => ModelGate.Borrow<Mesh, Seq<GeometryHandle>>(
                handle: edit.Source, body: mesh =>
                    from held in edit.Law.Rig()
                    from built in held.Use(
                        body: options => ModelGate.Single(() => SubD.CreateFromMesh(mesh: mesh, options: options)))
                    select built),
            fromSurface: static (_, edit) => ModelGate.Borrow<Surface, Seq<GeometryHandle>>(
                handle: edit.Source, body: surface =>
                    ModelGate.Single(() => SubD.CreateFromSurface(
                        surface: surface, method: edit.Method, corners: edit.Corners.Native))),
            fromLoft: static (_, edit) => ModelGate.BorrowMany<NurbsCurve, Seq<GeometryHandle>>(
                handles: edit.Shapes, body: shapes => {
                    (bool corners, bool creases) = edit.Features.Native;
                    return ModelGate.Single(() => SubD.CreateFromLoft(
                        curves: shapes.AsIterable(), closed: edit.Closure.Native, addCorners: corners,
                        addCreases: creases, divisions: edit.Divisions));
                }),
            fromSweepOne: static (_, edit) => ModelGate.BorrowMany<NurbsCurve, Seq<GeometryHandle>>(
                handles: edit.Shapes, body: shapes =>
                    ModelGate.Borrow<NurbsCurve, Seq<GeometryHandle>>(handle: edit.Rail, body: rail => {
                        (SweepFrame frame, Vector3d normal) = edit.Frame.Native;
                        return ModelGate.Single(() => SubD.CreateFromSweep(
                            rail1: rail, shapes: shapes.AsIterable(), closed: edit.Closure.Native,
                            addCorners: edit.Corners.Native, roadlikeFrame: frame == SweepFrame.Roadlike,
                            roadlikeNormal: normal));
                    })),
            fromSweepTwo: static (_, edit) => ModelGate.BorrowMany<NurbsCurve, Seq<GeometryHandle>>(
                handles: edit.Shapes, body: shapes =>
                    ModelGate.Borrow<NurbsCurve, Seq<GeometryHandle>>(handle: edit.Rail1, body: rail1 =>
                        ModelGate.Borrow<NurbsCurve, Seq<GeometryHandle>>(handle: edit.Rail2, body: rail2 =>
                            ModelGate.Single(() => SubD.CreateFromSweep(
                                rail1: rail1, rail2: rail2, shapes: shapes.AsIterable(),
                                closed: edit.Closure.Native, addCorners: edit.Corners.Native))))),
            seedQuadSphere: static (_, edit) => ModelGate.Single(() => SubD.CreateQuadSphere(sphere: edit.Value, vertexLocation: edit.VertexLocation,
                    quadSubdivisionLevel: edit.SubdivisionLevel)),
            seedGlobeSphere: static (_, edit) => ModelGate.Single(() => SubD.CreateGlobeSphere(sphere: edit.Value, vertexLocation: edit.VertexLocation,
                    axialFaceCount: edit.AxialFaceCount, equatorialFaceCount: edit.EquatorialFaceCount)),
            seedTriSphere: static (_, edit) => ModelGate.Single(() => SubD.CreateTriSphere(sphere: edit.Value, vertexLocation: edit.VertexLocation,
                    triSubdivisionLevel: edit.SubdivisionLevel)),
            seedIcosahedron: static (_, edit) => ModelGate.Single(() => SubD.CreateIcosahedron(sphere: edit.Value, vertexLocation: edit.VertexLocation)),
            seedCylinder: static (_, edit) => ModelGate.Single(() => SubD.CreateFromCylinder(cylinder: edit.Value, circumferenceFaceCount: edit.CircumferenceFaceCount,
                    heightFaceCount: edit.HeightFaceCount, endCapStyle: edit.EndCap,
                    endCapEdgeTag: edit.EndCapEdgeTag, radiusLocation: edit.RadiusLocation)),
            join: static (model, edit) => ModelGate.BorrowMany<SubD, Seq<GeometryHandle>>(
                handles: edit.Targets, body: targets => {
                    (bool creases, bool preserveSymmetry) = edit.Policy.Native;
                    return ModelGate.Many(() => SubD.JoinSubDs(
                        subdsToJoin: targets.AsIterable(), tolerance: model.Absolute.Value,
                        joinedEdgesAreCreases: creases, preserveSymmetry: preserveSymmetry));
                }),
            edit: static (model, request) => ModelGate.Borrow<SubD, Seq<GeometryHandle>>(
                handle: request.Target, body: source =>
                    Try.lift(() => {
                        SubD working = (SubD)source.Duplicate();
                        return request.Verb.Apply(working: working, domain: model).Rollback(working);
                    }).Run().Bind(static inner => inner)),
            toBrep: static (_, edit) => ModelGate.Borrow<SubD, Seq<GeometryHandle>>(
                handle: edit.Target, body: subd =>
                    from held in edit.Law.Rig()
                    from built in held.Use(
                        body: options => ModelGate.Single(() => subd.ToBrep(options: options)))
                    select built),
            edgeCurves: static (_, edit) => ModelGate.Borrow<SubD, Seq<GeometryHandle>>(
                handle: edit.Target, body: subd => {
                    (bool boundary, bool interior, bool smooth, bool sharp, bool crease, bool clamp) = edit.Selection.Native;
                    return ModelGate.Many(() => subd.DuplicateEdgeCurves(
                            boundaryOnly: boundary, interiorOnly: interior, smoothOnly: smooth,
                            sharpOnly: sharp, creaseOnly: crease, clampEnds: clamp),
                        allowEmpty: true);
                }));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class SubDs {
    public static Eff<ModelRuntime, Seq<GeometryHandle>> Build(params ReadOnlySpan<SubDOp> operations) {
        Seq<SubDOp> captured = toSeq(operations.ToArray());
        return Eff.runtime<ModelRuntime>().Bind(runtime =>
            ModelGate.Entry(
                runtime: runtime,
                operations: captured,
                admit: static (operation, key) => operation.Admitted(),
                apply: static (operation, model) => operation.Apply(domain: model)).ToEff());
    }
}
```

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
