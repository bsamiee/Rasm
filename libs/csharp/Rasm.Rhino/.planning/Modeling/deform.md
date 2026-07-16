# [RASM_RHINO_MODELING_DEFORM]

`Rasm.Rhino.Modeling` owns nonlinear deformation and host-fidelity flattening. One `DeformOp` union carries every `SpaceMorph`, persistent curve-pair control, mesh-pair vertex mapping, developable unrolling, stress-relaxed squishing, inverse mark mapping, and uv unwrapping through `Deforms.Apply`. Drivers enter as leased `GeometryHandle`s, engines remain bracketed inside their borrow windows, and every mutating native runs on a working duplicate. Document motion and kernel DEC flattening remain upstream owners.

## [01]-[INDEX]

- [02]-[MORPH_FAMILY]: `MorphKind`, `StretchLaw`, `MorphTuning` — the deformation vocabulary and its one rig-and-morph fold.
- [03]-[FLATTEN_FAMILY]: `Following`, `UnrollLaw`, `SquishLaw` — the flattening policies and their native engine seams.
- [04]-[OPERATION_RAIL]: `DeformSlot`, `DeformOp`, and the `Deforms.Apply` entry.
- [05]-[SURFACE_LEDGER]: the page's owner table.

## [02]-[MORPH_FAMILY]

- Owner: `MorphKind` `[Union]` — bend, flow, maelstrom, splop, sporph, stretch, taper, twist, mesh-cage, and the persistent curve-pair control as one closed family; `StretchLaw` `[Union]` — the stretch terminal as to-length or to-point; `MorphTuning` — the two shared engine knobs as one policy value.
- Law: one fold deforms every kind — each case borrows its drivers, constructs its native engine inside the borrow window, and hands it to the one `Deformed` kernel, so tolerance wiring, the static `SpaceMorph.IsMorphable` gate, duplication, the `Morph` confirm, engine disposal, and product custody are written once for ten deformations.
- Law: tolerance is the session regime — the engine `Tolerance` reads `context.Absolute.Value` inside `Deformed`, never a bare double on a case; `QuickPreview` and `PreserveStructure` ride `MorphTuning` because they are consumer intent, not geometry.
- Law: drivers stay leased and engines die in their arm — a flow rail, splop surface, or cage mesh is live only inside its borrow window, the disposable native engine is constructed, consumed, and disposed within that window, and no case retains a native driver past its arm; `Sporph` carries its optional `ConstrainNormal` as case payload written onto the engine before the morph.
- Law: the control morph is a case, not a second rail — `MorphControl` drives through the same duplicate-confirm-own kernel via its own knob spellings (`SpaceMorphTolerance` beside the shared pair), so a persistent origin-to-target deformer and a one-shot morph answer one vocabulary.
- Growth: a new host deformation is one `MorphKind` case with its rig arm; tuning, gating, custody, and every consumer read it with zero new surface.

```csharp
// --- [TYPES] ------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record StretchLaw {
    private StretchLaw() { }
    public sealed record ToLength(double Value) : StretchLaw;
    public sealed record ToPoint(Point3d Value) : StretchLaw;
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MorphKind {
    private MorphKind() { }
    public sealed record Bend(Point3d Start, Point3d End, Point3d Through, Option<double> Angle, bool Straight = false, bool Symmetric = false) : MorphKind;
    public sealed record Flow(GeometryHandle BaseRail, GeometryHandle TargetRail, bool ReverseBase = false, bool ReverseTarget = false, bool PreventStretching = false) : MorphKind;
    public sealed record Maelstrom(Plane Frame, double Radius0, double Radius1, double AngleRadians) : MorphKind;
    public sealed record Splop(Plane Frame, GeometryHandle Surface, Point2d SurfaceUv, double Scale = 1.0, double AngleRadians = 0.0) : MorphKind;
    public sealed record Sporph(GeometryHandle BaseSurface, GeometryHandle TargetSurface, Option<(Point2d BaseUv, Point2d TargetUv)> Alignment, Option<Vector3d> ConstrainNormal = default) : MorphKind;
    public sealed record Stretch(Point3d Start, Point3d End, StretchLaw Terminal) : MorphKind;
    public sealed record Taper(Point3d Start, Point3d End, double StartRadius, double EndRadius, bool Flat = false, bool Infinite = false) : MorphKind;
    public sealed record Twist(Line Axis, double AngleRadians, bool Infinite = false) : MorphKind;
    public sealed record Cage(GeometryHandle Reference, GeometryHandle Target) : MorphKind;
    public sealed record Control(GeometryHandle OriginCurve, GeometryHandle TargetCurve) : MorphKind;

    internal Fin<GeometryHandle> Morph(GeometryHandle target, MorphTuning tuning, Context context, Op key) =>
        Switch(
            context: (Target: target, Tuning: tuning, Domain: context, Op: key),
            bend: static (ctx, kind) => Deformed(
                mint: () => kind.Angle.Case switch {
                    double angle => new Morphs.BendSpaceMorph(start: kind.Start, end: kind.End, point: kind.Through, angle: angle, straight: kind.Straight, symmetric: kind.Symmetric),
                    _ => new Morphs.BendSpaceMorph(start: kind.Start, end: kind.End, point: kind.Through, straight: kind.Straight, symmetric: kind.Symmetric),
                },
                ctx: ctx),
            flow: static (ctx, kind) => ModelGate.Borrow<Curve, GeometryHandle>(handle: kind.BaseRail, key: ctx.Op, body: baseRail =>
                ModelGate.Borrow<Curve, GeometryHandle>(handle: kind.TargetRail, key: ctx.Op, body: targetRail => Deformed(
                    mint: () => new Morphs.FlowSpaceMorph(
                        curve0: baseRail, curve1: targetRail,
                        reverseCurve0: kind.ReverseBase, reverseCurve1: kind.ReverseTarget, preventStretching: kind.PreventStretching),
                    ctx: ctx))),
            maelstrom: static (ctx, kind) => Deformed(
                mint: () => new Morphs.MaelstromSpaceMorph(plane: kind.Frame, radius0: kind.Radius0, radius1: kind.Radius1, angle: kind.AngleRadians),
                ctx: ctx),
            splop: static (ctx, kind) => ModelGate.Borrow<Surface, GeometryHandle>(handle: kind.Surface, key: ctx.Op, body: surface => Deformed(
                mint: () => new Morphs.SplopSpaceMorph(plane: kind.Frame, surface: surface, surfaceParam: kind.SurfaceUv, scale: kind.Scale, angle: kind.AngleRadians),
                ctx: ctx)),
            sporph: static (ctx, kind) => ModelGate.Borrow<Surface, GeometryHandle>(handle: kind.BaseSurface, key: ctx.Op, body: baseSurface =>
                ModelGate.Borrow<Surface, GeometryHandle>(handle: kind.TargetSurface, key: ctx.Op, body: targetSurface => Deformed(
                    mint: () => {
                        Morphs.SporphSpaceMorph engine = kind.Alignment.Case switch {
                            (Point2d baseUv, Point2d targetUv) => new(surface0: baseSurface, surface1: targetSurface, surface0Param: baseUv, surface1Param: targetUv),
                            _ => new(surface0: baseSurface, surface1: targetSurface),
                        };
                        _ = kind.ConstrainNormal.Iter(normal => engine.ConstrainNormal = normal);
                        return engine;
                    },
                    ctx: ctx))),
            stretch: static (ctx, kind) => Deformed(
                mint: () => kind.Terminal.Switch(
                    state: kind,
                    toLength: static (held, law) => new Morphs.StretchSpaceMorph(start: held.Start, end: held.End, length: law.Value),
                    toPoint: static (held, law) => new Morphs.StretchSpaceMorph(start: held.Start, end: held.End, point: law.Value)),
                ctx: ctx),
            taper: static (ctx, kind) => Deformed(
                mint: () => new Morphs.TaperSpaceMorph(
                    start: kind.Start, end: kind.End, startRadius: kind.StartRadius, endRadius: kind.EndRadius,
                    bFlat: kind.Flat, infiniteTaper: kind.Infinite),
                ctx: ctx),
            twist: static (ctx, kind) => Deformed(
                mint: () => new Morphs.TwistSpaceMorph {
                    TwistAxis = kind.Axis,
                    TwistAngleRadians = kind.AngleRadians,
                    InfiniteTwist = kind.Infinite,
                },
                ctx: ctx),
            cage: static (ctx, kind) => ModelGate.Borrow<Mesh, GeometryHandle>(handle: kind.Reference, key: ctx.Op, body: reference =>
                ModelGate.Borrow<Mesh, GeometryHandle>(handle: kind.Target, key: ctx.Op, body: cageTarget => Deformed(
                    mint: () => new Morphs.MeshCageMorph(referenceMesh: reference, targetMesh: cageTarget),
                    ctx: ctx))),
            control: static (ctx, kind) => ModelGate.Borrow<NurbsCurve, GeometryHandle>(handle: kind.OriginCurve, key: ctx.Op, body: origin =>
                ModelGate.Borrow<NurbsCurve, GeometryHandle>(handle: kind.TargetCurve, key: ctx.Op, body: driven =>
                    ctx.Op.Catch(() => {
                        using MorphControl driver = new(originCurve: origin, targetCurve: driven);
                        driver.SpaceMorphTolerance = ctx.Domain.Absolute.Value;
                        driver.QuickPreview = ctx.Tuning.QuickPreview;
                        driver.PreserveStructure = ctx.Tuning.PreserveStructure;
                        return Duplicated(target: ctx.Target, morph: working => ctx.Op.Confirm(success: driver.Morph(geometry: working)), key: ctx.Op);
                    }))));

    private static Fin<GeometryHandle> Deformed<TMorph>(Func<TMorph> mint, (GeometryHandle Target, MorphTuning Tuning, Context Domain, Op Op) ctx)
        where TMorph : SpaceMorph, IDisposable =>
        ctx.Op.Catch(() => {
            using TMorph active = mint();
            active.Tolerance = ctx.Domain.Absolute.Value;
            active.QuickPreview = ctx.Tuning.QuickPreview;
            active.PreserveStructure = ctx.Tuning.PreserveStructure;
            return Duplicated(target: ctx.Target, morph: working => ctx.Op.Confirm(success: active.Morph(geometry: working)), key: ctx.Op);
        });

    private static Fin<GeometryHandle> Duplicated(GeometryHandle target, Func<GeometryBase, Fin<Unit>> morph, Op key) =>
        ModelGate.Borrow<GeometryBase, GeometryHandle>(handle: target, key: key, body: source =>
            from _ in guard(SpaceMorph.IsMorphable(geometry: source), key.Unsupported(geometryType: source.GetType(), outputType: typeof(GeometryBase)))
            from working in key.Catch(() => Optional(source.Duplicate()).ToFin(Fail: key.InvalidResult()))
            from morphed in morph(arg: working).Match(
                Succ: _ => ModelGate.Own(built: working, key: key),
                Fail: error => {
                    working.Dispose();
                    return Fin.Fail<GeometryHandle>(error: error);
                })
            select morphed);
}

// --- [MODELS] -----------------------------------------------------------------------------
public readonly record struct MorphTuning(bool QuickPreview = false, bool PreserveStructure = false);
```

## [03]-[FLATTEN_FAMILY]

- Owner: `Following` — leased curves, bare points, and dot rows carried into the flat frame; `UnrollLaw` — output explosion and spacing; `SquishLaw` — flattening algorithm, deformation mode, boundary preservation, deformation constants, optional spring bias, topology and mapping grants, absolute limit, and flat-net capture.
- Law: `SquishLaw.Rig` writes every `SquishParameters` property, calls `SetDeformation`, optionally calls `SetSpringConstants`, and the squish arm records `GetSpringConstants`; the disposable carrier dies inside that arm.
- Law: dot followers are rows, never leased text objects — a dot enters as `(Location, Text)` through the host's own row overload, and the flattened dots return as the same row shape on the receipt with their natives disposed inside the arm.
- Law: flattening tolerance is the session regime — `Unroller.AbsoluteTolerance` and `RelativeTolerance` read `Context.Absolute` and `Context.Fractional`.
- Boundary: the squisher's inverse mapping is a static probe — `Is2dPatternSquished` gates and `SquishBack2dMarks` maps without a retained engine, so the restore modality needs no live `Squisher` custody.

```csharp
// --- [MODELS] -----------------------------------------------------------------------------
public sealed record Following(
    Seq<GeometryHandle> Curves,
    Seq<Point3d> Points,
    Seq<(Point3d Location, string Text)> Dots) {
    public static readonly Following None = new(Curves: Seq<GeometryHandle>(), Points: Seq<Point3d>(), Dots: Seq<(Point3d, string)>());
}

public readonly record struct UnrollLaw(bool Explode = true, double Spacing = 2.0);

public sealed record SquishLaw(
    SquishFlatteningAlgorithm Algorithm,
    SquishDeformation Mode,
    bool PreserveBoundary,
    double BoundaryStretch,
    double BoundaryCompress,
    double InteriorStretch,
    double InteriorCompress,
    bool PreserveTopology,
    bool SaveMapping,
    double AbsoluteLimit,
    Option<(double BoundaryBias, double DeformationBias)> SpringBias = default,
    bool CaptureNets = false) {
    public static readonly SquishLaw Standard = new(
        Algorithm: SquishFlatteningAlgorithm.Geometric,
        Mode: SquishDeformation.Free,
        PreserveBoundary: true,
        BoundaryStretch: 1.0,
        BoundaryCompress: 1.0,
        InteriorStretch: 1.0,
        InteriorCompress: 1.0,
        PreserveTopology: false,
        SaveMapping: true,
        AbsoluteLimit: 1.0);

    internal Fin<SquishParameters> Rig(Op key) =>
        key.Catch(() => {
            SquishParameters parameters = SquishParameters.Default;
            parameters.Algorithm = Algorithm;
            parameters.PreserveTopology = PreserveTopology;
            parameters.SaveMapping = SaveMapping;
            parameters.AbsoluteLimit = AbsoluteLimit;
            parameters.SetDeformation(
                deformation: Mode,
                bPreserveBoundary: PreserveBoundary,
                boundaryStretchConstant: BoundaryStretch,
                boundaryCompressConstant: BoundaryCompress,
                interiorStretchConstant: InteriorStretch,
                interiorCompressConstant: InteriorCompress);
            _ = SpringBias.Iter(bias => parameters.SetSpringConstants(
                boundaryBias: bias.BoundaryBias, deformationBias: bias.DeformationBias));
            return Fin.Succ(value: parameters);
        });
}
```

## [04]-[OPERATION_RAIL]

- Owner: `DeformSlot` `[SmartEnum<int>]` — the consequence vocabulary; `DeformOp` `[Union]` — morph, vertex map, unroll, squish, squish-back, and unwrap as one verb family; `Deforms` — the one entry folding any operation spread into one `Built<DeformSlot>`.
- Law: products carry custody, receipts carry facts — every flattened brep, mapped mark, and unwrapped mesh crosses as an owned `GeometryHandle` in `Built.Products`, while unrolled points, dot rows, mapped vertices, and per-class tallies ride the `BuildReceipt` fact stream; product ordering follows the slot tallies (primary products, then followed or mapped geometry), so a consumer partitions the flat product seq by the receipt's counts.
- Law: squish dispatches on the borrowed native — a `Surface` target runs `SquishSurface` onto a flat `Brep`, a `Mesh` target runs `SquishMesh`, any other kind refuses typed; the caller-owned mark output list drains into crossed products under `MarkMapped`, and `CaptureNets` additionally crosses the flat and source triangulations under `Netted`.
- Law: the vertex map is evidence, not custody — `MapVertices` drives the mesh-pair `MeshMorphMesh` engine over a bare point spread and answers the mapped positions as one `Marks` fact, because the host engine transforms coordinates rather than geometry.
- Law: unwrap is value-semantic per mesh — each leased mesh duplicates, the one `MeshUnwrapper` writes texture coordinates onto the duplicates under the optional symmetry plane, and the duplicates cross as products; a `false` unwrap disposes every duplicate before the fault leaves.
- Law: the entry is arity-polymorphic — one `params ReadOnlySpan<DeformOp>` absorbs the singular and batch call, the fold sums products and receipts monoidally, and a mid-batch failure releases every product accumulated by earlier operations before the fault leaves.
- Growth: a new deformation verb is one `DeformOp` case with its arm; a new consequence class is one slot row.

```csharp
// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class DeformSlot {
    public static readonly DeformSlot Morphed = new(key: 0);
    public static readonly DeformSlot Mapped = new(key: 1);
    public static readonly DeformSlot Unrolled = new(key: 2);
    public static readonly DeformSlot Followed = new(key: 3);
    public static readonly DeformSlot Squished = new(key: 4);
    public static readonly DeformSlot MarkMapped = new(key: 5);
    public static readonly DeformSlot Netted = new(key: 6);
    public static readonly DeformSlot Restored = new(key: 7);
    public static readonly DeformSlot Unwrapped = new(key: 8);
    public static readonly DeformSlot Mesh2dEdges = new(key: 9);
    public static readonly DeformSlot Mesh3dEdges = new(key: 10);
    public static readonly DeformSlot Length2d = new(key: 11);
    public static readonly DeformSlot Length3d = new(key: 12);
    public static readonly DeformSlot AreaConstraints = new(key: 13);
    public static readonly DeformSlot SpringConstants = new(key: 14);
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DeformOp {
    private DeformOp() { }
    public sealed record Morph(GeometryHandle Target, MorphKind Kind, MorphTuning Tuning = default) : DeformOp;
    public sealed record MapVertices(Seq<Point3d> Vertices, GeometryHandle StartMesh, GeometryHandle AdjustedMesh) : DeformOp;
    public sealed record Unroll(GeometryHandle Target, Following Followers, UnrollLaw Law) : DeformOp;
    public sealed record Squish(GeometryHandle Target, SquishLaw Law, Seq<GeometryHandle> Marks) : DeformOp;
    public sealed record SquishBack(GeometryHandle Pattern, Seq<GeometryHandle> Marks) : DeformOp;
    public sealed record Unwrap(Seq<GeometryHandle> Meshes, MeshUnwrapMethod Method, Option<Plane> Symmetry = default) : DeformOp;

    internal Fin<Built<DeformSlot>> Apply(Context domain) =>
        Switch(
            context: domain,
            morph: static (model, edit) => {
                Op op = Op.Of(name: nameof(Morph));
                return edit.Kind.Morph(target: edit.Target, tuning: edit.Tuning, context: model, key: op)
                    .Map(product => new Built<DeformSlot>(
                        Products: Seq(product),
                        Evidence: BuildReceipt<DeformSlot>.Of(slot: DeformSlot.Morphed, body: new BuildBody.Tally(Count: 1))));
            },
            mapVertices: static (_, edit) => {
                Op op = Op.Of(name: nameof(MapVertices));
                return ModelGate.Borrow<Mesh, Built<DeformSlot>>(handle: edit.StartMesh, key: op, body: start =>
                    ModelGate.Borrow<Mesh, Built<DeformSlot>>(handle: edit.AdjustedMesh, key: op, body: adjusted =>
                        from _ in guard(!edit.Vertices.IsEmpty, op.InvalidInput())
                        from mapped in op.Catch(() => {
                            using Morphs.MeshMorphMesh engine = new(referenceMesh: start, meshToMorph: adjusted);
                            Point3d[] working = edit.Vertices.ToArray();
                            return op.Confirm(success: engine.Apply(vertices: working, startMesh: start, adjustedMesh: adjusted))
                                .Map(_ => toSeq(working));
                        })
                        select new Built<DeformSlot>(
                            Products: Seq<GeometryHandle>(),
                            Evidence: BuildReceipt<DeformSlot>.Of(slot: DeformSlot.Mapped, body: new BuildBody.Marks(Points: mapped)))));
            },
            unroll: static (model, edit) => {
                Op op = Op.Of(name: nameof(Unroll));
                return ModelGate.Borrow<GeometryBase, Built<DeformSlot>>(handle: edit.Target, key: op, body: source =>
                    ModelGate.BorrowMany<Curve, Built<DeformSlot>>(handles: edit.Followers.Curves, key: op, allowEmpty: true, body: followers =>
                        op.Catch(() => {
                            Fin<Unroller> admitted = source switch {
                                Brep brep => Fin.Succ(value: new Unroller(brep: brep)),
                                Surface surface => Fin.Succ(value: new Unroller(surface: surface)),
                                _ => Fin.Fail<Unroller>(error: op.Unsupported(geometryType: source.GetType(), outputType: typeof(Unroller))),
                            };
                            return admitted.Bind(active => {
                                active.ExplodeOutput = edit.Law.Explode;
                                active.ExplodeSpacing = edit.Law.Spacing;
                                active.AbsoluteTolerance = model.Absolute.Value;
                                active.RelativeTolerance = model.Fractional.Value;
                                _ = Op.SideWhen(!followers.IsEmpty, () => active.AddFollowingGeometry(curves: followers.AsIterable()));
                                _ = Op.SideWhen(!edit.Followers.Points.IsEmpty, () => active.AddFollowingGeometry(points: edit.Followers.Points.AsIterable()));
                                _ = edit.Followers.Dots.Iter(row => active.AddFollowingGeometry(dotLocation: row.Location, dotText: row.Text));
                                return
                                    from flat in ModelGate.OwnMany(built: active.PerformUnroll(
                                        unrolledCurves: out Curve[] flatCurves, unrolledPoints: out Point3d[] flatPoints, unrolledDots: out TextDot[] flatDots), key: op)
                                    from carried in ModelGate.OwnMany(built: flatCurves, key: op, allowEmpty: true).MapFail(error => {
                                        _ = flat.Iter(static prior => prior.Dispose());
                                        return error;
                                    })
                                    let rows = toSeq(flatDots ?? []).Map(static dot => (dot.Point, dot.Text))
                                    let _ = toSeq(flatDots ?? []).Iter(static dot => dot.Dispose())
                                    select new Built<DeformSlot>(
                                        Products: flat + carried,
                                        Evidence: BuildReceipt<DeformSlot>.Of(slot: DeformSlot.Unrolled, body: new BuildBody.Tally(Count: flat.Count))
                                            + BuildReceipt<DeformSlot>.Of(slot: DeformSlot.Followed, body: new BuildBody.Tally(Count: carried.Count))
                                            + BuildReceipt<DeformSlot>.Of(slot: DeformSlot.Followed, body: new BuildBody.Marks(Points: toSeq(flatPoints ?? [])))
                                            + BuildReceipt<DeformSlot>.Of(slot: DeformSlot.Followed, body: new BuildBody.Labels(Rows: rows)));
                            });
                        })));
            },
            squish: static (_, edit) => {
                Op op = Op.Of(name: nameof(Squish));
                return ModelGate.Borrow<GeometryBase, Built<DeformSlot>>(handle: edit.Target, key: op, body: source =>
                    ModelGate.BorrowMany<GeometryBase, Built<DeformSlot>>(handles: edit.Marks, key: op, allowEmpty: true, body: marks =>
                        from parameters in edit.Law.Rig(key: op)
                        from flattened in op.Catch(() => {
                            using SquishParameters sp = parameters;
                            using Squisher engine = new();
                            _ = sp.GetSpringConstants(boundaryBias: out double boundaryBias, deformationBias: out double deformationBias);
                            System.Collections.Generic.List<GeometryBase> mapped = [];
                            Fin<GeometryHandle> flat = source switch {
                                Surface surface => ModelGate.Own(
                                    built: engine.SquishSurface(sp: sp, surface: surface, marks: marks.AsIterable(), squished_marks_out: mapped), key: op),
                                Mesh mesh => ModelGate.Own(
                                    built: engine.SquishMesh(sp: sp, mesh3d: mesh, marks: marks.AsIterable(), squished_marks_out: mapped), key: op),
                                _ => Fin.Fail<GeometryHandle>(error: op.Unsupported(geometryType: source.GetType(), outputType: typeof(Squisher))),
                            };
                            return flat.Bind(primary => (
                                from crossed in ModelGate.OwnMany(built: mapped, key: op, allowEmpty: true)
                                from nets in (edit.Law.CaptureNets
                                    ? from flat2d in ModelGate.Own(built: engine.Get2dMesh(), key: op)
                                      from flat3d in ModelGate.Own(built: engine.Get3dMesh(), key: op).MapFail(error => {
                                          flat2d.Dispose();
                                          return error;
                                      })
                                      select Seq(flat2d, flat3d)
                                    : Fin.Succ(value: Seq<GeometryHandle>())).MapFail(error => {
                                        _ = crossed.Iter(static handle => handle.Dispose());
                                        return error;
                                    })
                                select new Built<DeformSlot>(
                                    Products: Seq(primary) + crossed + nets,
                                    Evidence: BuildReceipt<DeformSlot>.Of(slot: DeformSlot.Squished, body: new BuildBody.Tally(Count: 1))
                                        + BuildReceipt<DeformSlot>.Of(slot: DeformSlot.MarkMapped, body: new BuildBody.Tally(Count: crossed.Count))
                                        + BuildReceipt<DeformSlot>.Of(slot: DeformSlot.Netted, body: new BuildBody.Tally(Count: nets.Count))
                                        + BuildReceipt<DeformSlot>.Of(slot: DeformSlot.Mesh2dEdges, body: new BuildBody.Segments(Lines: toSeq(engine.GetMesh2dEdges() ?? [])))
                                        + BuildReceipt<DeformSlot>.Of(slot: DeformSlot.Mesh3dEdges, body: new BuildBody.Segments(Lines: toSeq(engine.GetMesh3dEdges() ?? [])))
                                        + BuildReceipt<DeformSlot>.Of(slot: DeformSlot.Length2d, body: new BuildBody.Segments(Lines: toSeq(engine.GetLengthConstrained2dLines() ?? [])))
                                        + BuildReceipt<DeformSlot>.Of(slot: DeformSlot.Length3d, body: new BuildBody.Segments(Lines: toSeq(engine.GetLengthConstrained3dLines() ?? [])))
                                        + BuildReceipt<DeformSlot>.Of(slot: DeformSlot.AreaConstraints, body: new BuildBody.Faces(Rows: toSeq(engine.GetAreaConstrainedTrianglesIndices() ?? [])))
                                        + BuildReceipt<DeformSlot>.Of(slot: DeformSlot.SpringConstants, body: new BuildBody.Measure(Value: boundaryBias))
                                        + BuildReceipt<DeformSlot>.Of(slot: DeformSlot.SpringConstants, body: new BuildBody.Measure(Value: deformationBias))))
                                .MapFail(error => {
                                    primary.Dispose();
                                    return error;
                                }));
                        })
                        select flattened));
            },
            squishBack: static (_, edit) => {
                Op op = Op.Of(name: nameof(SquishBack));
                return ModelGate.Borrow<GeometryBase, Built<DeformSlot>>(handle: edit.Pattern, key: op, body: pattern =>
                    ModelGate.BorrowMany<GeometryBase, Built<DeformSlot>>(handles: edit.Marks, key: op, body: marks =>
                        from _ in op.Confirm(success: Squisher.Is2dPatternSquished(geometry: pattern))
                        from restored in op.Catch(() => ModelGate.OwnMany(
                            built: Squisher.SquishBack2dMarks(squishedGeometry: pattern, marks: marks.AsIterable()), key: op))
                        select new Built<DeformSlot>(
                            Products: restored,
                            Evidence: BuildReceipt<DeformSlot>.Of(slot: DeformSlot.Restored, body: new BuildBody.Tally(Count: restored.Count)))));
            },
            unwrap: static (_, edit) => {
                Op op = Op.Of(name: nameof(Unwrap));
                return ModelGate.BorrowMany<Mesh, Built<DeformSlot>>(handles: edit.Meshes, key: op, body: sources =>
                    op.Catch(() => {
                        return DuplicateMeshes(sources: sources, op: op).Bind(working => {
                            return op.Catch(() => {
                                using MeshUnwrapper engine = new(meshes: working.AsIterable());
                                _ = edit.Symmetry.Iter(plane => engine.SymmetryPlane = plane);
                                return op.Confirm(success: engine.Unwrap(method: edit.Method))
                                    .Bind(_ => ModelGate.OwnMany(built: working, key: op).Map(products => new Built<DeformSlot>(
                                        Products: products,
                                        Evidence: BuildReceipt<DeformSlot>.Of(slot: DeformSlot.Unwrapped, body: new BuildBody.Tally(Count: products.Count)))));
                            }).MapFail(error => {
                                _ = working.Iter(static mesh => mesh.Dispose());
                                return error;
                            });
                        });
                    }));
            });

    private static Fin<Seq<Mesh>> DuplicateMeshes(Seq<Mesh> sources, Op op) =>
        sources.Fold(Fin.Succ(value: Seq<Mesh>()), (state, source) => state.Bind(held =>
            op.Catch(() => Optional(source.Duplicate() as Mesh).ToFin(Fail: op.InvalidResult())).Match(
                Succ: copy => Fin.Succ(value: held.Add(value: copy)),
                Fail: error => {
                    _ = held.Iter(static mesh => mesh.Dispose());
                    return Fin.Fail<Seq<Mesh>>(error: error);
                })));
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class Deforms {
    public static Fin<Built<DeformSlot>> Apply(Context context, params ReadOnlySpan<DeformOp> operations) {
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

## [05]-[SURFACE_LEDGER]

| [INDEX] | [CONCERN]         | [OWNER]       | [FORM]                                            | [ENTRY]                   |
| :-----: | :---------------- | :------------ | :------------------------------------------------ | :------------------------ |
|  [01]   | deformation kinds | `MorphKind`   | one union, ten rig arms, one `Deformed` kernel    | `DeformOp.Morph`          |
|  [02]   | engine tuning     | `MorphTuning` | preview and structure grants as one value         | `Morph` payload           |
|  [03]   | vertex mapping    | `DeformOp`    | mesh-pair engine over a bare point spread         | `DeformOp.MapVertices`    |
|  [04]   | unroll policy     | `UnrollLaw`   | explode, spacing, relative-tolerance override     | `DeformOp.Unroll`         |
|  [05]   | carried geometry  | `Following`   | leased curves, bare points, dot rows              | `Unroll` payload          |
|  [06]   | squish policy     | `SquishLaw`   | one value rigging the split native surface        | `DeformOp.Squish` / `Rig` |
|  [07]   | inverse mapping   | `DeformOp`    | static pattern probe plus mark restore            | `DeformOp.SquishBack`     |
|  [08]   | uv unwrap         | `DeformOp`    | value-semantic duplicate set under one unwrapper  | `DeformOp.Unwrap`         |
|  [09]   | deform verbs      | `DeformOp`    | one flat `[Union]`, total generated dispatch      | `Deforms.Apply`           |
