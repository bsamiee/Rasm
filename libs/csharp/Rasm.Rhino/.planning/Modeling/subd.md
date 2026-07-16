# [RASM_RHINO_MODELING_SUBD]

`Rasm.Rhino.Modeling` owns command-fidelity SubD construction and value-semantic editing. One `SubDOp` union carries mesh conversion, surface fitting, lofting, sweep construction, primitives, joining, subdivision, interpolation, offsetting, coplanar merging, face packing, component transformation, tag authoring, brep conversion, and edge extraction through `SubDs.Build`. `SubDCreationLaw` carries every `SubDCreationOptions` preset and property; `SubDBrepLaw` carries the complete `SubDToBrepOptions` surface. Kernel stencil subdivision and limit evaluation remain kernel-owned.

## [01]-[INDEX]

- [02]-[CREATION_POLICY]: `CreasePreset`, `SubDCreationLaw`, `SubDBrepLaw`, `SubDSeed`, `SubDSweepKind` — the conversion and primitive policies.
- [03]-[OPERATION_RAIL]: `SubDSlot`, `SubDEditVerb`, `SubDOp`, and the `SubDs.Build` entry.
- [04]-[SURFACE_LEDGER]: the page's owner table.

## [02]-[CREATION_POLICY]

- Owner: `CreasePreset` `[SmartEnum<int>]` — the four host option presets as rows carrying their factory columns; `SubDCreationLaw` `[Union]` — preset row or the full custom `SubDCreationOptions` surface; `SubDBrepLaw` — pack-faces plus the extraordinary-vertex process as one value; `SubDSeed` `[Union]` — the four sphere topologies and the capped cylinder; `SubDSweepKind` `[Union]` — one-rail with its roadlike grant versus two-rail.
- Law: `Rig` is the one site naming `SubDCreationOptions` — interior-crease, convex-corner, concave-corner, and texture tests, the corner edge-count and angle bounds, and vertex interpolation write once; the rigged carrier is disposable and dies in the consuming arm.
- Law: sphere topology is a case, never a parameter — quad, globe, tri, and icosahedron carry exactly the counts their native constructor demands, so an inapplicable count is unconstructible.

```csharp
// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class CreasePreset {
    public static readonly CreasePreset Smooth = new(key: 0, static () => SubDCreationOptions.Smooth);
    public static readonly CreasePreset InteriorCreases = new(key: 1, static () => SubDCreationOptions.InteriorCreases);
    public static readonly CreasePreset ConvexCorners = new(key: 2, static () => SubDCreationOptions.ConvexCornersAndInteriorCreases);
    public static readonly CreasePreset AllCorners = new(key: 3, static () => SubDCreationOptions.ConvexAndConcaveCornersAndInteriorCreases);

    [UseDelegateFromConstructor]
    internal partial SubDCreationOptions Mint();
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SubDCreationLaw {
    private SubDCreationLaw() { }
    public sealed record Preset(CreasePreset Row) : SubDCreationLaw;
    public sealed record Custom(
        SubDCreationOptions.InteriorCreaseOption InteriorCrease,
        SubDCreationOptions.ConvexCornerOption ConvexCorner,
        SubDCreationOptions.ConcaveCornerOption ConcaveCorner,
        SubDCreationOptions.TextureCoordinateOption TextureCoordinates,
        uint MaximumConvexCornerEdgeCount = 2,
        double MaximumConvexCornerAngleRadians = 2.0943951023931953,
        double MinimumConcaveCornerAngleRadians = 4.1887902047863905,
        uint MinimumConcaveCornerEdgeCount = 4,
        bool InterpolateMeshVertices = false) : SubDCreationLaw;

    internal Fin<SubDCreationOptions> Rig(Op key) =>
        key.Catch(() => Fin.Succ(value: Switch(
            preset: static law => law.Row.Mint(),
            custom: static law => new SubDCreationOptions {
                InteriorCreaseTest = law.InteriorCrease,
                ConvexCornerTest = law.ConvexCorner,
                ConcaveCornerTest = law.ConcaveCorner,
                TextureCoordinateTest = law.TextureCoordinates,
                MaximumConvexCornerEdgeCount = law.MaximumConvexCornerEdgeCount,
                MaximumConvexCornerAngleRadians = law.MaximumConvexCornerAngleRadians,
                MinimumConcaveCornerAngleRadians = law.MinimumConcaveCornerAngleRadians,
                MinimumConcaveCornerEdgeCount = law.MinimumConcaveCornerEdgeCount,
                InterpolateMeshVertices = law.InterpolateMeshVertices,
            })));
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SubDSeed {
    private SubDSeed() { }
    public sealed record QuadSphere(Sphere Value, SubDComponentLocation VertexLocation, uint SubdivisionLevel) : SubDSeed;
    public sealed record GlobeSphere(Sphere Value, SubDComponentLocation VertexLocation, uint AxialFaceCount, uint EquatorialFaceCount) : SubDSeed;
    public sealed record TriSphere(Sphere Value, SubDComponentLocation VertexLocation, uint SubdivisionLevel) : SubDSeed;
    public sealed record Icosahedron(Sphere Value, SubDComponentLocation VertexLocation) : SubDSeed;
    public sealed record OfCylinder(Cylinder Value, uint CircumferenceFaceCount, uint HeightFaceCount, SubDEndCapStyle EndCap, SubDEdgeTag EndCapEdgeTag, SubDComponentLocation RadiusLocation) : SubDSeed;

    internal Fin<GeometryHandle> Build(Op key) =>
        Switch(
            context: key,
            quadSphere: static (op, seed) => op.Catch(() => ModelGate.Own(built: SubD.CreateQuadSphere(
                sphere: seed.Value, vertexLocation: seed.VertexLocation, quadSubdivisionLevel: seed.SubdivisionLevel), key: op)),
            globeSphere: static (op, seed) => op.Catch(() => ModelGate.Own(built: SubD.CreateGlobeSphere(
                sphere: seed.Value, vertexLocation: seed.VertexLocation,
                axialFaceCount: seed.AxialFaceCount, equatorialFaceCount: seed.EquatorialFaceCount), key: op)),
            triSphere: static (op, seed) => op.Catch(() => ModelGate.Own(built: SubD.CreateTriSphere(
                sphere: seed.Value, vertexLocation: seed.VertexLocation, triSubdivisionLevel: seed.SubdivisionLevel), key: op)),
            icosahedron: static (op, seed) => op.Catch(() => ModelGate.Own(built: SubD.CreateIcosahedron(
                sphere: seed.Value, vertexLocation: seed.VertexLocation), key: op)),
            ofCylinder: static (op, seed) => op.Catch(() => ModelGate.Own(built: SubD.CreateFromCylinder(
                cylinder: seed.Value, circumferenceFaceCount: seed.CircumferenceFaceCount, heightFaceCount: seed.HeightFaceCount,
                endCapStyle: seed.EndCap, endCapEdgeTag: seed.EndCapEdgeTag, radiusLocation: seed.RadiusLocation), key: op)));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SubDSweepKind {
    private SubDSweepKind() { }
    public sealed record OneRail(GeometryHandle Rail, SweepFrameLaw Frame) : SubDSweepKind;
    public sealed record TwoRail(GeometryHandle Rail1, GeometryHandle Rail2) : SubDSweepKind;
}

// --- [MODELS] -----------------------------------------------------------------------------
public readonly record struct SubDBrepLaw(bool PackFaces = true, SubDToBrepOptions.ExtraordinaryVertexProcessOption VertexProcess = SubDToBrepOptions.ExtraordinaryVertexProcessOption.LocalG1x) {
    internal Fin<SubDToBrepOptions> Rig(Op key) =>
        key.Catch(() => Fin.Succ(value: new SubDToBrepOptions(packFaces: PackFaces, vertexProcess: VertexProcess)));
}
```

## [03]-[OPERATION_RAIL]

- Owner: `SubDSlot` `[SmartEnum<int>]` — the consequence vocabulary; `SubDEditVerb` `[Union]` — the value-semantic edit verbs applied to one working duplicate; `SubDOp` `[Union]` — conversion, primitive, join, edit, brep, and extraction verbs as one family; `SubDs` — the one entry folding any operation spread into one `Built<SubDSlot>`.
- Law: edits are value-semantic — `Edit` duplicates the borrowed subd once, dispatches the verb on the working copy, refreshes tags and the surface mesh cache after a tag or interpolation edit, and owns the copy or the member's returned subd; component transform and pack tallies land as `Tally` facts.
- Law: tags author creases — vertex and edge tag rows write through the component lists' `SetVertexTags`/`SetEdgeTags`, then `UpdateAllTagsAndSectorCoefficients` and `UpdateSurfaceMeshCache` run unconditionally, so an authored crease or corner is limit-surface-true before the product crosses.
- Law: interpolation constrains by presence — a bare point spread solves all control points, a vertex-index spread constrains the named subset, and unequal cardinality refuses before the native call.
- Law: the quad-remesh route composes — a subd from quad-remeshed geometry is the meshing rail's `QuadRemesh` product fed to `FromMesh`; no second remesh entry exists here.
- Growth: a new subd constructor or edit verb is one case with its arm; the spine and every consumer read it with zero new surface.

```csharp
// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class SubDSlot {
    public static readonly SubDSlot Converted = new(key: 0);
    public static readonly SubDSlot Fitted = new(key: 1);
    public static readonly SubDSlot Lofted = new(key: 2);
    public static readonly SubDSlot Swept = new(key: 3);
    public static readonly SubDSlot Seeded = new(key: 4);
    public static readonly SubDSlot Joined = new(key: 5);
    public static readonly SubDSlot Edited = new(key: 6);
    public static readonly SubDSlot Brepped = new(key: 7);
    public static readonly SubDSlot EdgeCurves = new(key: 8);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SubDEditVerb {
    private SubDEditVerb() { }
    public sealed record SubdivideAll(int Count = 1) : SubDEditVerb;
    public sealed record SubdivideFaces(Seq<int> Faces) : SubDEditVerb;
    public sealed record Interpolate(Seq<Point3d> SurfacePoints, Seq<uint> Vertices = default) : SubDEditVerb;
    public sealed record SetVertexPoint(uint Vertex, Point3d SurfacePoint) : SubDEditVerb;
    public sealed record Shell(double Distance, bool Solidify) : SubDEditVerb;
    public sealed record MergeCoplanar : SubDEditVerb;
    public sealed record Pack : SubDEditVerb;
    public sealed record Flip : SubDEditVerb;
    public sealed record TagVertices(Seq<int> Vertices, SubDVertexTag Tag) : SubDEditVerb;
    public sealed record TagEdges(Seq<int> Edges, SubDEdgeTag Tag) : SubDEditVerb;
    public sealed record MoveComponents(Seq<ComponentIndex> Components, Transform Motion, SubDComponentLocation Location) : SubDEditVerb;
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SubDOp {
    private SubDOp() { }
    public sealed record FromMesh(GeometryHandle Source, SubDCreationLaw Law) : SubDOp;
    public sealed record FromSurface(GeometryHandle Source, SubDFromSurfaceMethods Method, bool Corners) : SubDOp;
    public sealed record FromLoft(Seq<GeometryHandle> Shapes, bool Closed, bool AddCorners, bool AddCreases, int Divisions) : SubDOp;
    public sealed record FromSweep(SubDSweepKind Kind, Seq<GeometryHandle> Shapes, bool Closed, bool AddCorners) : SubDOp;
    public sealed record Seed(SubDSeed Value) : SubDOp;
    public sealed record Join(Seq<GeometryHandle> Targets, bool JoinedEdgesAreCreases, bool PreserveSymmetry = true) : SubDOp;
    public sealed record Edit(GeometryHandle Target, SubDEditVerb Verb) : SubDOp;
    public sealed record ToBrep(GeometryHandle Target, SubDBrepLaw Law) : SubDOp;
    public sealed record EdgeCurves(GeometryHandle Target, bool BoundaryOnly = false, bool InteriorOnly = false, bool SmoothOnly = false, bool SharpOnly = false, bool CreaseOnly = false, bool ClampEnds = false) : SubDOp;

    internal Fin<Built<SubDSlot>> Apply(Context domain) =>
        Switch(
            context: domain,
            fromMesh: static (_, edit) => {
                Op op = Op.Of(name: nameof(FromMesh));
                return ModelGate.Borrow<Mesh, Built<SubDSlot>>(handle: edit.Source, key: op, body: mesh =>
                    from options in edit.Law.Rig(key: op)
                    from built in op.Catch(() => {
                        using SubDCreationOptions live = options;
                        return Single(op, SubDSlot.Converted, () => SubD.CreateFromMesh(mesh: mesh, options: live));
                    })
                    select built);
            },
            fromSurface: static (_, edit) => {
                Op op = Op.Of(name: nameof(FromSurface));
                return ModelGate.Borrow<Surface, Built<SubDSlot>>(handle: edit.Source, key: op, body: surface =>
                    Single(op, SubDSlot.Fitted, () => SubD.CreateFromSurface(surface: surface, method: edit.Method, corners: edit.Corners)));
            },
            fromLoft: static (_, edit) => {
                Op op = Op.Of(name: nameof(FromLoft));
                return ModelGate.BorrowMany<NurbsCurve, Built<SubDSlot>>(handles: edit.Shapes, key: op, body: shapes =>
                    Single(op, SubDSlot.Lofted, () => SubD.CreateFromLoft(
                        curves: shapes.AsIterable(), closed: edit.Closed, addCorners: edit.AddCorners,
                        addCreases: edit.AddCreases, divisions: edit.Divisions)));
            },
            fromSweep: static (_, edit) => {
                Op op = Op.Of(name: nameof(FromSweep));
                return ModelGate.BorrowMany<NurbsCurve, Built<SubDSlot>>(handles: edit.Shapes, key: op, body: shapes =>
                    edit.Kind.Switch(
                        state: (Shapes: shapes, Edit: edit, Op: op),
                        oneRail: static (ctx, kind) => ModelGate.Borrow<NurbsCurve, Built<SubDSlot>>(handle: kind.Rail, key: ctx.Op, body: rail => {
                            (SweepFrame frame, Vector3d normal) = kind.Frame.Native;
                            return Single(ctx.Op, SubDSlot.Swept, () => SubD.CreateFromSweep(
                                rail1: rail, shapes: ctx.Shapes.AsIterable(), closed: ctx.Edit.Closed, addCorners: ctx.Edit.AddCorners,
                                roadlikeFrame: frame == SweepFrame.Roadlike, roadlikeNormal: normal));
                        }),
                        twoRail: static (ctx, kind) => ModelGate.Borrow<NurbsCurve, Built<SubDSlot>>(handle: kind.Rail1, key: ctx.Op, body: rail1 =>
                            ModelGate.Borrow<NurbsCurve, Built<SubDSlot>>(handle: kind.Rail2, key: ctx.Op, body: rail2 =>
                                Single(ctx.Op, SubDSlot.Swept, () => SubD.CreateFromSweep(
                                    rail1: rail1, rail2: rail2, shapes: ctx.Shapes.AsIterable(),
                                    closed: ctx.Edit.Closed, addCorners: ctx.Edit.AddCorners))))));
            },
            seed: static (_, edit) => {
                Op op = Op.Of(name: nameof(Seed));
                return edit.Value.Build(key: op).Map(product => new Built<SubDSlot>(
                    Products: Seq(product),
                    Evidence: BuildReceipt<SubDSlot>.Of(slot: SubDSlot.Seeded, body: new BuildBody.Tally(Count: 1))));
            },
            join: static (model, edit) => {
                Op op = Op.Of(name: nameof(Join));
                return ModelGate.BorrowMany<SubD, Built<SubDSlot>>(handles: edit.Targets, key: op, body: targets =>
                    op.Catch(() => ModelGate.OwnMany(built: SubD.JoinSubDs(
                            subdsToJoin: targets.AsIterable(), tolerance: model.Absolute.Value,
                            joinedEdgesAreCreases: edit.JoinedEdgesAreCreases, preserveSymmetry: edit.PreserveSymmetry), key: op)
                        .Map(owned => new Built<SubDSlot>(
                            Products: owned,
                            Evidence: BuildReceipt<SubDSlot>.Of(slot: SubDSlot.Joined, body: new BuildBody.Tally(Count: owned.Count))))));
            },
            edit: static (model, request) => {
                Op op = Op.Of(name: nameof(Edit));
                return ModelGate.Borrow<SubD, Built<SubDSlot>>(handle: request.Target, key: op, body: source =>
                    op.Catch(() => {
                        SubD working = (SubD)source.Duplicate();
                        return Edited(working: working, verb: request.Verb, domain: model, op: op).MapFail(error => {
                            working.Dispose();
                            return error;
                        });
                    }));
            },
            toBrep: static (_, edit) => {
                Op op = Op.Of(name: nameof(ToBrep));
                return ModelGate.Borrow<SubD, Built<SubDSlot>>(handle: edit.Target, key: op, body: subd =>
                    from options in edit.Law.Rig(key: op)
                    from built in op.Catch(() => {
                        using SubDToBrepOptions live = options;
                        return Single(op, SubDSlot.Brepped, () => subd.ToBrep(options: live));
                    })
                    select built);
            },
            edgeCurves: static (_, edit) => {
                Op op = Op.Of(name: nameof(EdgeCurves));
                return ModelGate.Borrow<SubD, Built<SubDSlot>>(handle: edit.Target, key: op, body: subd =>
                    op.Catch(() => ModelGate.OwnMany(built: subd.DuplicateEdgeCurves(
                            boundaryOnly: edit.BoundaryOnly, interiorOnly: edit.InteriorOnly, smoothOnly: edit.SmoothOnly,
                            sharpOnly: edit.SharpOnly, creaseOnly: edit.CreaseOnly, clampEnds: edit.ClampEnds), key: op, allowEmpty: true)
                        .Map(owned => new Built<SubDSlot>(
                            Products: owned,
                            Evidence: BuildReceipt<SubDSlot>.Of(slot: SubDSlot.EdgeCurves, body: new BuildBody.Tally(Count: owned.Count))))));
            });

    private static Fin<Built<SubDSlot>> Edited(SubD working, SubDEditVerb verb, Context domain, Op op) =>
        verb.Switch(
            state: (Working: working, Domain: domain, Op: op),
            subdivideAll: static (ctx, edit) => ctx.Op.Confirm(success: ctx.Working.Subdivide(count: edit.Count))
                .Bind(_ => Refreshed(ctx.Op, ctx.Working)),
            subdivideFaces: static (ctx, edit) => ctx.Op.Confirm(success: ctx.Working.Subdivide(faceIndices: edit.Faces.AsIterable()))
                .Bind(_ => Refreshed(ctx.Op, ctx.Working)),
            interpolate: static (ctx, edit) =>
                from _ in guard(edit.Vertices.IsEmpty || edit.Vertices.Count == edit.SurfacePoints.Count, ctx.Op.InvalidInput())
                from __ in ctx.Op.Confirm(success: edit.Vertices.IsEmpty
                    ? ctx.Working.InterpolateSurfacePoints(surfacePoints: edit.SurfacePoints.ToArray())
                    : ctx.Working.InterpolateSurfacePoints(vertexIndices: edit.Vertices.ToArray(), surfacePoints: edit.SurfacePoints.ToArray()))
                from built in Refreshed(ctx.Op, ctx.Working)
                select built,
            setVertexPoint: static (ctx, edit) => ctx.Op
                .Confirm(success: ctx.Working.SetVertexSurfacePoint(vertexIndex: edit.Vertex, surfacePoint: edit.SurfacePoint))
                .Bind(_ => Refreshed(ctx.Op, ctx.Working)),
            shell: static (ctx, edit) => ctx.Op.Catch(() =>
                ModelGate.Own(built: ctx.Working.Offset(distance: edit.Distance, solidify: edit.Solidify), key: ctx.Op).Map(owned => {
                    ctx.Working.Dispose();
                    return new Built<SubDSlot>(
                        Products: Seq(owned),
                        Evidence: BuildReceipt<SubDSlot>.Of(slot: SubDSlot.Edited, body: new BuildBody.Tally(Count: 1)));
                })),
            mergeCoplanar: static ctx => ctx.Op
                .Confirm(success: ctx.Working.MergeAllCoplanarFaces(tolerance: ctx.Domain.Absolute.Value, angleTolerance: ctx.Domain.Angle.Value))
                .Bind(_ => Refreshed(ctx.Op, ctx.Working)),
            pack: static ctx => ctx.Op.Catch(() => {
                uint packed = ctx.Working.PackFaces();
                return Kept(ctx.Op, ctx.Working, extra: BuildReceipt<SubDSlot>.Of(slot: SubDSlot.Edited, body: new BuildBody.Tally(Count: (int)packed)));
            }),
            flip: static ctx => ctx.Op.Confirm(success: ctx.Working.Flip()).Bind(_ => Refreshed(ctx.Op, ctx.Working)),
            tagVertices: static (ctx, edit) => ctx.Op.Catch(() => {
                ctx.Working.Vertices.SetVertexTags(vertexIndices: edit.Vertices.AsIterable(), tag: edit.Tag);
                return Refreshed(ctx.Op, ctx.Working);
            }),
            tagEdges: static (ctx, edit) => ctx.Op.Catch(() => {
                ctx.Working.Edges.SetEdgeTags(edgeIndices: edit.Edges.AsIterable(), tag: edit.Tag);
                return Refreshed(ctx.Op, ctx.Working);
            }),
            moveComponents: static (ctx, edit) => ctx.Op.Catch(() => {
                uint moved = ctx.Working.TransformComponents(
                    components: edit.Components.AsIterable(), xform: edit.Motion, componentLocation: edit.Location);
                return moved > 0
                    ? Refreshed(ctx.Op, ctx.Working).Map(built => built with {
                        Evidence = built.Evidence + BuildReceipt<SubDSlot>.Of(slot: SubDSlot.Edited, body: new BuildBody.Tally(Count: (int)moved)),
                    })
                    : Fin.Fail<Built<SubDSlot>>(error: ctx.Op.InvalidResult());
            }));

    private static Fin<Built<SubDSlot>> Refreshed(Op op, SubD working) =>
        op.Catch(() => {
            _ = working.UpdateAllTagsAndSectorCoefficients();
            _ = working.UpdateSurfaceMeshCache(lazyUpdate: false);
            return Kept(op, working);
        });

    private static Fin<Built<SubDSlot>> Kept(Op op, SubD working) =>
        ModelGate.Own(built: working, key: op).Map(owned => new Built<SubDSlot>(
            Products: Seq(owned),
            Evidence: BuildReceipt<SubDSlot>.Of(slot: SubDSlot.Edited, body: new BuildBody.Tally(Count: 1))));

    private static Fin<Built<SubDSlot>> Kept(Op op, SubD working, BuildReceipt<SubDSlot> extra) =>
        ModelGate.Own(built: working, key: op).Map(owned => new Built<SubDSlot>(
            Products: Seq(owned),
            Evidence: BuildReceipt<SubDSlot>.Of(slot: SubDSlot.Edited, body: new BuildBody.Tally(Count: 1)) + extra));

    private static Fin<Built<SubDSlot>> Single(Op op, SubDSlot slot, Func<GeometryBase?> run) =>
        op.Catch(() => ModelGate.Own(built: run(), key: op).Map(owned => new Built<SubDSlot>(
            Products: Seq(owned),
            Evidence: BuildReceipt<SubDSlot>.Of(slot: slot, body: new BuildBody.Tally(Count: 1)))));
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class SubDs {
    public static Fin<Built<SubDSlot>> Build(Context context, params ReadOnlySpan<SubDOp> operations) {
        Op op = Op.Of();
        return from domain in Optional(context).ToFin(Fail: op.MissingContext())
               from _ in guard(operations.Length > 0, op.InvalidInput())
               from built in ModelGate.Folded(
                   context: domain,
                   operations: toSeq(operations.ToArray()),
                   apply: static (operation, model) => operation.Apply(domain: model))
               select built;
    }
}
```

## [04]-[SURFACE_LEDGER]

| [INDEX] | [CONCERN]           | [OWNER]           | [FORM]                                              | [ENTRY]                    |
| :-----: | :------------------ | :---------------- | :-------------------------------------------------- | :------------------------- |
|  [01]   | crease policy       | `SubDCreationLaw` | preset rows or full `SubDCreationOptions` surface   | `SubDOp.FromMesh` / `Rig`  |
|  [02]   | brep conversion     | `SubDBrepLaw`     | packing + extraordinary-vertex process as one value | `SubDOp.ToBrep` / `Rig`    |
|  [03]   | primitive seeding   | `SubDSeed`        | four sphere topologies and the capped cylinder      | `SubDOp.Seed`              |
|  [04]   | sweep modality      | `SubDSweepKind`   | one-rail with roadlike grant versus two-rail        | `SubDOp.FromSweep`         |
|  [05]   | value-semantic edit | `SubDEditVerb`    | duplicate-edit-refresh-own verbs                    | `SubDOp.Edit`              |
|  [06]   | crease authoring    | `SubDEditVerb`    | tag rows + unconditional tag/sector refresh         | `TagVertices` / `TagEdges` |
|  [07]   | edge extraction     | `SubDOp`          | filtered edge curves as products                    | `SubDOp.EdgeCurves`        |
|  [08]   | subd verbs          | `SubDOp`          | one flat `[Union]`, total generated dispatch        | `SubDs.Build`              |
