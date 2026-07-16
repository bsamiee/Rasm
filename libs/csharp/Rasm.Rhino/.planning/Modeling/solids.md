# [RASM_RHINO_MODELING_SOLIDS]

`Rasm.Rhino.Modeling` owns the Brep solid-construction rail and Modeling spine. One `SolidOp` union carries booleans with diagnostic side-channels, settings-driven fillet/chamfer/blend, offset/shell/pipe, primitive and conversion seeding, tapered extrusion, join/split/merge/match editing, and the `Extrusion` lightweight solid through `Solids.Build`. `ModelGate` supplies every sibling's typed borrow fold over leased `GeometryHandle` inputs and owned acquisition for fresh natives. `Built<TSlot>` and `BuildReceipt<TSlot>` carry products plus evidence through one receipt monoid specialized by each page's slot vocabulary. Native statics return command-fidelity geometry at the host boundary; host-neutral kernel booleans and offsets plus intersections, mass properties, bounds, contours, and analysis remain kernel-owned. Tolerance enters every arm through `Context`.

## [01]-[INDEX]

- [02]-[MODEL_GATE]: `ModelGate`, `Built<TSlot>`, `BuildBody`, `BuildFact<TSlot>`, `BuildReceipt<TSlot>` — the folder spine.
- [03]-[POLICY_FAMILY]: `BooleanVerb`, `FilletShape`, `EdgeFillet`, `MatchLaw`, `PipeLaw`, `SolidSeed`, `ExtrusionSeed`, `SolidEdit`, `TrimCutter`, `ConnectSeed` — the construction policies.
- [04]-[OPERATION_RAIL]: `SolidSlot`, `SolidOp`, and the `Solids.Build` entry.
- [05]-[SURFACE_LEDGER]: the page's owner table.

## [02]-[MODEL_GATE]

- Owner: `ModelGate` — the one custody kernel under every Modeling arm: `Borrow` projects a live native of the demanded kind out of a leased handle, `BorrowMany` nests borrow windows over a handle spread, `Own`/`OwnMany` mint owned leases for freshly constructed natives, and `Folded` is the batch fold every page entry runs; `Built<TSlot>` — products plus evidence as one value; `BuildBody` `[Union]` — the closed evidence payload vocabulary; `BuildReceipt<TSlot>` — the slot-generic fact-stream monoid.
- Law: a construction result is an acquisition, never a crossing — the native static's return is this rail's own owned material, so `Own` mints the owned lease directly and `GeometryCrossing.Cross` remains the entry for foreign or document geometry; a null single result and a null-or-empty array are the native failure signal folded to `InvalidResult`, and a mid-spread `OwnMany` failure disposes every handle it already minted.
- Law: one receipt algebra serves every Modeling page — `BuildReceipt<TSlot>` is generic over the page's slot vocabulary, so diagnostic points, uv rows, labels, index maps, segments, faces, components, region topology, tallies, codes, measures, flags, and texts are one `BuildBody` union with one `+` monoid; every typed reader derives from the one polymorphic `Project<T>(slot, select)` core, so the projection surface is total over the body vocabulary and a new evidence kind is one `BuildBody` case plus one `Project`-backed accessor every page gains at once.
- Law: the batch fold is failure-symmetric — `Folded` sums products and receipts monoidally and releases every product accumulated by earlier operations the moment a later operation faults, so a batch never half-leaks custody.
- Law: `Borrow` is the type gate — a handle whose live native is not the demanded kind refuses through `Unsupported` with both types named, so no arm ever pattern-matches raw geometry beyond its own dispatch.
- Growth: a new evidence payload is one `BuildBody` case; a new custody modality is one `ModelGate` member; sibling pages add zero spine surface.

```csharp
// --- [TYPES] ------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record BuildBody {
    private BuildBody() { }
    public sealed record Marks(Seq<Point3d> Points) : BuildBody;
    public sealed record UvRows(Seq<Point2d> Rows) : BuildBody;
    public sealed record Labels(Seq<(Point3d Location, string Text)> Rows) : BuildBody;
    public sealed record SourceMap(Seq<int> Rows) : BuildBody;
    public sealed record SourceGroups(Seq<Seq<int>> Groups) : BuildBody;
    public sealed record Tally(int Count) : BuildBody;
    public sealed record Measure(double Value) : BuildBody;
    public sealed record Code(int Value) : BuildBody;
    public sealed record Components(Seq<int> Indices) : BuildBody;
    public sealed record ComponentRows(Seq<ComponentIndex> Indices) : BuildBody;
    public sealed record Segments(Seq<Line> Lines) : BuildBody;
    public sealed record Faces(Seq<MeshFace> Rows) : BuildBody;
    public sealed record RegionSegments(Seq<(int Region, int Boundary, int Segment, int PlanarCurve, Interval Domain, bool Reversed)> Rows) : BuildBody;
    public sealed record Flag(bool Value) : BuildBody;
    public sealed record Text(string Value) : BuildBody;
}

// --- [MODELS] -----------------------------------------------------------------------------
public readonly record struct BuildFact<TSlot>(TSlot Slot, BuildBody Body) where TSlot : notnull;

public readonly record struct BuildReceipt<TSlot> where TSlot : notnull {
    private readonly Seq<BuildFact<TSlot>> facts;

    private BuildReceipt(Seq<BuildFact<TSlot>> facts) => this.facts = facts;

    public static BuildReceipt<TSlot> Empty { get; } = new(facts: Seq<BuildFact<TSlot>>());

    public Seq<BuildFact<TSlot>> Facts => facts;

    public static BuildReceipt<TSlot> operator +(BuildReceipt<TSlot> left, BuildReceipt<TSlot> right) =>
        new(facts: left.facts + right.facts);

    public static BuildReceipt<TSlot> Of(TSlot slot, BuildBody body) =>
        new(facts: Seq(new BuildFact<TSlot>(Slot: slot, Body: body)));

    public Seq<T> Project<T>(TSlot slot, Func<BuildBody, Option<T>> select) =>
        facts.Filter(fact => fact.Slot.Equals(slot)).Choose(fact => select(fact.Body));

    public Seq<Point3d> Points(TSlot slot) =>
        Project(slot, static body => body is BuildBody.Marks b ? Some(b.Points) : Option<Seq<Point3d>>.None).Bind(identity);

    public Seq<Point2d> UvRows(TSlot slot) =>
        Project(slot, static body => body is BuildBody.UvRows b ? Some(b.Rows) : Option<Seq<Point2d>>.None).Bind(identity);

    public Seq<(Point3d Location, string Text)> Labels(TSlot slot) =>
        Project(slot, static body => body is BuildBody.Labels b ? Some(b.Rows) : Option<Seq<(Point3d Location, string Text)>>.None).Bind(identity);

    public Seq<Seq<int>> Maps(TSlot slot) =>
        Project(slot, static body => body switch {
            BuildBody.SourceMap b => Some(Seq(b.Rows)),
            BuildBody.SourceGroups b => Some(b.Groups),
            _ => Option<Seq<Seq<int>>>.None,
        }).Bind(identity);

    public Seq<Line> Segments(TSlot slot) =>
        Project(slot, static body => body is BuildBody.Segments b ? Some(b.Lines) : Option<Seq<Line>>.None).Bind(identity);

    public Seq<MeshFace> Faces(TSlot slot) =>
        Project(slot, static body => body is BuildBody.Faces b ? Some(b.Rows) : Option<Seq<MeshFace>>.None).Bind(identity);

    public Seq<int> Components(TSlot slot) =>
        Project(slot, static body => body is BuildBody.Components b ? Some(b.Indices) : Option<Seq<int>>.None).Bind(identity);

    public Seq<ComponentIndex> ComponentRows(TSlot slot) =>
        Project(slot, static body => body is BuildBody.ComponentRows b ? Some(b.Indices) : Option<Seq<ComponentIndex>>.None).Bind(identity);

    public Seq<(int Region, int Boundary, int Segment, int PlanarCurve, Interval Domain, bool Reversed)> Regions(TSlot slot) =>
        Project(slot, static body => body is BuildBody.RegionSegments b
            ? Some(b.Rows)
            : Option<Seq<(int Region, int Boundary, int Segment, int PlanarCurve, Interval Domain, bool Reversed)>>.None).Bind(identity);

    public Seq<int> Tallies(TSlot slot) =>
        Project(slot, static body => body is BuildBody.Tally b ? Some(b.Count) : Option<int>.None);

    public Seq<int> Codes(TSlot slot) =>
        Project(slot, static body => body is BuildBody.Code b ? Some(b.Value) : Option<int>.None);

    public Seq<double> Measures(TSlot slot) =>
        Project(slot, static body => body is BuildBody.Measure b ? Some(b.Value) : Option<double>.None);

    public Seq<bool> Flags(TSlot slot) =>
        Project(slot, static body => body is BuildBody.Flag b ? Some(b.Value) : Option<bool>.None);

    public Seq<string> Texts(TSlot slot) =>
        Project(slot, static body => body is BuildBody.Text b ? Some(b.Value) : Option<string>.None);

    public int FactCount(TSlot slot) =>
        facts.Count(fact => fact.Slot.Equals(slot));
}

public sealed record Built<TSlot>(Seq<GeometryHandle> Products, BuildReceipt<TSlot> Evidence) where TSlot : notnull {
    public static readonly Built<TSlot> Empty = new(Products: Seq<GeometryHandle>(), Evidence: BuildReceipt<TSlot>.Empty);

    public static Built<TSlot> operator +(Built<TSlot> left, Built<TSlot> right) =>
        new(Products: left.Products + right.Products, Evidence: left.Evidence + right.Evidence);

    public Unit Release() => ignore(Products.Iter(static handle => handle.Dispose()));
}

// --- [OPERATIONS] -------------------------------------------------------------------------
internal static class ModelGate {
    internal static Fin<TResult> Borrow<TNative, TResult>(GeometryHandle handle, Op key, Func<TNative, Fin<TResult>> body)
        where TNative : GeometryBase =>
        Optional(handle).ToFin(Fail: key.InvalidInput()).Bind(active => active.With(
            key: key,
            project: geometry => Optional(geometry as TNative)
                .ToFin(Fail: key.Unsupported(geometryType: geometry.GetType(), outputType: typeof(TNative)))
                .Bind(body)));

    internal static Fin<TResult> BorrowMany<TNative, TResult>(
        Seq<GeometryHandle> handles, Op key, Func<Seq<TNative>, Fin<TResult>> body, bool allowEmpty = false)
        where TNative : GeometryBase =>
        handles.IsEmpty && !allowEmpty
            ? Fin.Fail<TResult>(error: key.InvalidInput())
            : Nested(handles: handles, borrowed: Seq<TNative>(), key: key, body: body);

    internal static Fin<GeometryHandle> Own(GeometryBase? built, Op key) =>
        Optional(built).ToFin(Fail: key.InvalidResult())
            .Map(fresh => new GeometryHandle(lease: new Lease<GeometryBase>.Owned(Value: fresh), mode: CrossingMode.Detach));

    internal static Fin<Seq<GeometryHandle>> OwnMany(IEnumerable<GeometryBase>? built, Op key, bool allowEmpty = false) =>
        Optional(built).Map(static values => toSeq(values)).ToFin(Fail: key.InvalidResult())
            .Bind(fresh => fresh.IsEmpty && !allowEmpty
                ? Fin.Fail<Seq<GeometryHandle>>(error: key.InvalidResult())
                : fresh.Fold(Fin.Succ(value: Seq<GeometryHandle>()), (state, value) => state.Bind(held =>
                    Own(built: value, key: key).Match(
                        Succ: handle => Fin.Succ(value: held.Add(value: handle)),
                        Fail: error => {
                            _ = held.Iter(static prior => prior.Dispose());
                            return Fin.Fail<Seq<GeometryHandle>>(error: error);
                        }))));

    internal static Fin<Built<TSlot>> Folded<TSlot, TOp>(
        Context context, Seq<TOp> operations, Func<TOp, Context, Fin<Built<TSlot>>> apply)
        where TSlot : notnull =>
        operations.Fold(Fin.Succ(value: Built<TSlot>.Empty), (state, operation) => state.Bind(held =>
            apply(operation, context).Match(
                Succ: next => Fin.Succ(value: held + next),
                Fail: error => {
                    _ = held.Release();
                    return Fin.Fail<Built<TSlot>>(error: error);
                })));

    private static Fin<TResult> Nested<TNative, TResult>(
        Seq<GeometryHandle> handles, Seq<TNative> borrowed, Op key, Func<Seq<TNative>, Fin<TResult>> body)
        where TNative : GeometryBase =>
        handles.Head.Case switch {
            GeometryHandle head => Borrow<TNative, TResult>(handle: head, key: key,
                body: native => Nested(handles: handles.Tail, borrowed: borrowed.Add(value: native), key: key, body: body)),
            _ => body(arg: borrowed),
        };
}
```

## [03]-[POLICY_FAMILY]

- Owner: `BooleanVerb` `[SmartEnum<int>]` — union, intersection, difference, split as rows with a second-set demand column; `PlanarBooleanVerb` `[SmartEnum<int>]` — the planar subset without an illusory split row; `FilletShape` `[Union]` — the four `Brep.FilletSurfaceSettings` profile factories as cases, chamfer a profile beside the three fillet forms, never a sibling method; `SectionFilletProfile` `[Union]` — the verified `SurfaceFilletBase` section family with cubic, quartic, and quintic degree generated from one discriminant; `EdgeFillet` — one edge row pairing the index with a constant or parameter-profiled radius law; `MatchLaw` — the complete `MatchSrfSettings` carrier with context-rigged refinement; `PipeLaw` `[Union]` — constant, variable, thick, and thick-variable radius profiles; `SolidSeed` `[Union]` — analytic primitives and geometry conversions minting a first brep; `ExtrusionSeed` `[Union]` — the lightweight-solid factories; `SolidEdit` `[Union]` — the value-semantic in-place edit verbs; `TrimCutter`/`ConnectSeed` — the trim and face-extension discriminants.
- Law: the fillet profile is the settings factory — every `Brep[]`-returning fillet/chamfer overload is obsolete, so `FilletShape.Rig` is the only site naming `CreateRationalArcSettings`/`CreateNonRationalSettings`/`CreateG2BlendSettings`/`CreateChamferSettings`, the tolerance slot reads the regime, and `ContinueAcrossTangentFaces` rides every case as the one public post-factory knob.
- Law: section fillets generate the degree space — rational arcs and G2 chordal quintic are terminal cases, while non-rational arc and slider profiles admit cubic, quartic, or quintic degree; cubic consumes tangent only, quartic and quintic require tangent plus inner slider, and every other combination refuses before dispatch.
- Law: parallel arrays are rows — an edge fillet enters as `(Edge, Law)` rows and the arm splits all-constant rows onto `CreateFilletEdges` and any-profiled rows onto `CreateFilletEdgesVariableRadius` with `BrepEdgeFilletDistance` rows minted per profile point, so equal-cardinality is proven by construction and the two native members stay one case.
- Law: `MatchLaw` collapses the host's split configuration — constructor continuities, public property knobs, and the `EnableRefinement` seam rig in one member, so a match is one value and the `PreserveIsoCurveMethod`/`Continuity` vocabularies never leak past the rig.
- Law: seeds carry no custody unless the source is geometry — analytic primitive cases hold value structs; the surface, revolve, and mesh conversion cases hold leased handles borrowed only inside `Build`.
- Growth: a new profile is one `FilletShape` case; a new primitive is one `SolidSeed` case; a new edit verb is one `SolidEdit` case — the rail and every consumer read them with zero new surface.

```csharp
// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class BooleanVerb {
    public static readonly BooleanVerb Union = new(key: 0, requiresSecond: false);
    public static readonly BooleanVerb Intersection = new(key: 1, requiresSecond: true);
    public static readonly BooleanVerb Difference = new(key: 2, requiresSecond: true);
    public static readonly BooleanVerb Split = new(key: 3, requiresSecond: true);

    public bool RequiresSecond { get; }
}

[SmartEnum<int>]
public sealed partial class PlanarBooleanVerb {
    public static readonly PlanarBooleanVerb Union = new(key: 0, requiresSecond: false);
    public static readonly PlanarBooleanVerb Intersection = new(key: 1, requiresSecond: true);
    public static readonly PlanarBooleanVerb Difference = new(key: 2, requiresSecond: true);

    public bool RequiresSecond { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FilletShape {
    private FilletShape() { }
    public sealed record RationalArc(double Radius, bool Trim = true, bool Extend = false, bool AcrossTangents = false) : FilletShape;
    public sealed record NonRational(double Radius, int Degree, double TanSlider, double InnerSlider, bool Trim = true, bool Extend = false, bool AcrossTangents = false) : FilletShape;
    public sealed record G2Blend(double Radius, bool Trim = true, bool Extend = false, bool AcrossTangents = false) : FilletShape;
    public sealed record Chamfer(double Radius0, double Radius1, bool Trim = true, bool Extend = false, bool AcrossTangents = false) : FilletShape;

    internal Fin<Brep.FilletSurfaceSettings> Rig(Context domain, Op key) =>
        key.Catch(() => {
            Brep.FilletSurfaceSettings settings = Switch(
                state: domain,
                rationalArc: static (ctx, shape) => Brep.FilletSurfaceSettings.CreateRationalArcSettings(
                    radius: shape.Radius, tolerance: ctx.Absolute.Value, trim: shape.Trim, extend: shape.Extend),
                nonRational: static (ctx, shape) => Brep.FilletSurfaceSettings.CreateNonRationalSettings(
                    radius: shape.Radius, tolerance: ctx.Absolute.Value, degree: shape.Degree,
                    tanSlider: shape.TanSlider, innerSlider: shape.InnerSlider, trim: shape.Trim, extend: shape.Extend),
                g2Blend: static (ctx, shape) => Brep.FilletSurfaceSettings.CreateG2BlendSettings(
                    radius: shape.Radius, tolerance: ctx.Absolute.Value, trim: shape.Trim, extend: shape.Extend),
                chamfer: static (ctx, shape) => Brep.FilletSurfaceSettings.CreateChamferSettings(
                    radius0: shape.Radius0, radius1: shape.Radius1, tolerance: ctx.Absolute.Value, trim: shape.Trim, extend: shape.Extend));
            settings.ContinueAcrossTangentFaces = Switch(
                rationalArc: static shape => shape.AcrossTangents,
                nonRational: static shape => shape.AcrossTangents,
                g2Blend: static shape => shape.AcrossTangents,
                chamfer: static shape => shape.AcrossTangents);
            return Fin.Succ(value: settings);
        });
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SectionFilletProfile {
    private SectionFilletProfile() { }
    public sealed record RationalArcs : SectionFilletProfile;
    public sealed record NonRationalArcs(int Degree) : SectionFilletProfile;
    public sealed record NonRational(int Degree, double TangentSlider, Option<double> InnerSlider = default) : SectionFilletProfile;
    public sealed record G2ChordalQuintic : SectionFilletProfile;

    internal bool IsAdmitted => Switch(
        rationalArcs: static _ => true,
        nonRationalArcs: static profile => profile.Degree is 3 or 4 or 5,
        nonRational: static profile => profile.Degree switch {
            3 => profile.InnerSlider.IsNone,
            4 or 5 => profile.InnerSlider.IsSome,
            _ => false,
        },
        g2ChordalQuintic: static _ => true);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RadiusLaw {
    private RadiusLaw() { }
    public sealed record Constant(double Start, double End) : RadiusLaw;
    public sealed record Profiled(Seq<(double Parameter, double Distance)> Rows) : RadiusLaw;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PipeLaw {
    private PipeLaw() { }
    public sealed record Constant(double Radius) : PipeLaw;
    public sealed record Variable(Seq<(double Parameter, double Radius)> Rows) : PipeLaw;
    public sealed record Thick(double Radius0, double Radius1) : PipeLaw;
    public sealed record ThickVariable(Seq<(double Parameter, double Inner, double Outer)> Rows) : PipeLaw;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TrimCutter {
    private TrimCutter() { }
    public sealed record ByBrep(GeometryHandle Cutter) : TrimCutter;
    public sealed record ByPlane(Plane Cutter) : TrimCutter;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ConnectSeed {
    private ConnectSeed() { }
    public sealed record AtEdges(int FirstEdge, int SecondEdge) : ConnectSeed;
    public sealed record AtPoints(Point3d First, Point3d Second) : ConnectSeed;
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SolidSeed {
    private SolidSeed() { }
    public sealed record OfBox(Box Value) : SolidSeed;
    public sealed record OfBounds(BoundingBox Value) : SolidSeed;
    public sealed record OfCorners(Seq<Point3d> Eight) : SolidSeed;
    public sealed record OfCylinder(Cylinder Value, bool CapBottom = true, bool CapTop = true) : SolidSeed;
    public sealed record OfCone(Cone Value, bool CapBottom = true) : SolidSeed;
    public sealed record OfTorus(Torus Value) : SolidSeed;
    public sealed record OfSphere(Sphere Value) : SolidSeed;
    public sealed record QuadSphere(Sphere Value) : SolidSeed;
    public sealed record Baseball(Point3d Center, double Radius) : SolidSeed;
    public sealed record CornerPoints(Point3d A, Point3d B, Point3d C, Point3d D) : SolidSeed;
    public sealed record FromSurface(GeometryHandle Source) : SolidSeed;
    public sealed record FromRevolve(GeometryHandle Source, bool CapStart = true, bool CapEnd = true) : SolidSeed;
    public sealed record FromMesh(GeometryHandle Source, bool TrimmedTriangles = true) : SolidSeed;

    internal Fin<GeometryHandle> Build(Context domain, Op key) =>
        Switch(
            context: (Domain: domain, Op: key),
            ofBox: static (ctx, seed) => ctx.Op.Catch(() => ModelGate.Own(built: Brep.CreateFromBox(box: seed.Value), key: ctx.Op)),
            ofBounds: static (ctx, seed) => ctx.Op.Catch(() => ModelGate.Own(built: Brep.CreateFromBox(box: seed.Value), key: ctx.Op)),
            ofCorners: static (ctx, seed) =>
                from _ in guard(seed.Eight.Count == 8, ctx.Op.InvalidInput())
                from built in ctx.Op.Catch(() => ModelGate.Own(built: Brep.CreateFromBox(corners: seed.Eight.AsIterable()), key: ctx.Op))
                select built,
            ofCylinder: static (ctx, seed) => ctx.Op.Catch(() => ModelGate.Own(
                built: Brep.CreateFromCylinder(cylinder: seed.Value, capBottom: seed.CapBottom, capTop: seed.CapTop), key: ctx.Op)),
            ofCone: static (ctx, seed) => ctx.Op.Catch(() => ModelGate.Own(built: Brep.CreateFromCone(cone: seed.Value, capBottom: seed.CapBottom), key: ctx.Op)),
            ofTorus: static (ctx, seed) => ctx.Op.Catch(() => ModelGate.Own(built: Brep.CreateFromTorus(torus: seed.Value), key: ctx.Op)),
            ofSphere: static (ctx, seed) => ctx.Op.Catch(() => ModelGate.Own(built: Brep.CreateFromSphere(sphere: seed.Value), key: ctx.Op)),
            quadSphere: static (ctx, seed) => ctx.Op.Catch(() => ModelGate.Own(built: Brep.CreateQuadSphere(sphere: seed.Value), key: ctx.Op)),
            baseball: static (ctx, seed) => ctx.Op.Catch(() => ModelGate.Own(
                built: Brep.CreateBaseballSphere(center: seed.Center, radius: seed.Radius, tolerance: ctx.Domain.Absolute.Value), key: ctx.Op)),
            cornerPoints: static (ctx, seed) => ctx.Op.Catch(() => ModelGate.Own(
                built: Brep.CreateFromCornerPoints(corner1: seed.A, corner2: seed.B, corner3: seed.C, corner4: seed.D, tolerance: ctx.Domain.Absolute.Value), key: ctx.Op)),
            fromSurface: static (ctx, seed) => ModelGate.Borrow<Surface, GeometryHandle>(handle: seed.Source, key: ctx.Op,
                body: surface => ctx.Op.Catch(() => ModelGate.Own(built: Brep.CreateFromSurface(surface: surface), key: ctx.Op))),
            fromRevolve: static (ctx, seed) => ModelGate.Borrow<RevSurface, GeometryHandle>(handle: seed.Source, key: ctx.Op,
                body: surface => ctx.Op.Catch(() => ModelGate.Own(
                    built: Brep.CreateFromRevSurface(surface: surface, capStart: seed.CapStart, capEnd: seed.CapEnd), key: ctx.Op))),
            fromMesh: static (ctx, seed) => ModelGate.Borrow<Mesh, GeometryHandle>(handle: seed.Source, key: ctx.Op,
                body: mesh => ctx.Op.Catch(() => ModelGate.Own(built: Brep.CreateFromMesh(mesh: mesh, trimmedTriangles: seed.TrimmedTriangles), key: ctx.Op))));
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ExtrusionSeed {
    private ExtrusionSeed() { }
    public sealed record Profile(GeometryHandle PlanarProfile, double Height, bool Cap = true, Option<Plane> Frame = default) : ExtrusionSeed;
    public sealed record OfBox(Box Value, bool Cap = true) : ExtrusionSeed;
    public sealed record OfCylinder(Cylinder Value, bool CapBottom = true, bool CapTop = true) : ExtrusionSeed;
    public sealed record OfPipe(Cylinder Value, double OtherRadius, bool CapTop = true, bool CapBottom = true) : ExtrusionSeed;

    internal Fin<GeometryHandle> Build(Op key) =>
        Switch(
            context: key,
            profile: static (op, seed) => ModelGate.Borrow<Curve, GeometryHandle>(handle: seed.PlanarProfile, key: op,
                body: profile => op.Catch(() => ModelGate.Own(
                    built: seed.Frame.Case switch {
                        Plane frame => Extrusion.Create(curve: profile, plane: frame, height: seed.Height, cap: seed.Cap),
                        _ => Extrusion.Create(planarCurve: profile, height: seed.Height, cap: seed.Cap),
                    },
                    key: op))),
            ofBox: static (op, seed) => op.Catch(() => ModelGate.Own(built: Extrusion.CreateBoxExtrusion(box: seed.Value, cap: seed.Cap), key: op)),
            ofCylinder: static (op, seed) => op.Catch(() => ModelGate.Own(
                built: Extrusion.CreateCylinderExtrusion(cylinder: seed.Value, capBottom: seed.CapBottom, capTop: seed.CapTop), key: op)),
            ofPipe: static (op, seed) => op.Catch(() => ModelGate.Own(
                built: Extrusion.CreatePipeExtrusion(cylinder: seed.Value, otherRadius: seed.OtherRadius, capTop: seed.CapTop, capBottom: seed.CapBottom), key: op)));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SolidEdit {
    private SolidEdit() { }
    public sealed record Cap : SolidEdit;
    public sealed record JoinNaked : SolidEdit;
    public sealed record MergeCoplanar(Option<(int First, int Second)> Faces = default) : SolidEdit;
    public sealed record UnjoinEdges(Seq<int> Edges) : SolidEdit;
    public sealed record RemoveHoles(Seq<ComponentIndex> Loops) : SolidEdit;
    public sealed record RemoveFins : SolidEdit;
    public sealed record CullFaces : SolidEdit;
    public sealed record Repair : SolidEdit;
    public sealed record Reseam(int Face, int Direction, double Parameter) : SolidEdit;
}

// --- [MODELS] -----------------------------------------------------------------------------
public readonly record struct EdgeFillet(int Edge, RadiusLaw Law);

public sealed record SectionFilletLaw(
    double Radius,
    int RailDegree,
    SectionFilletProfile Profile,
    bool Trim = true,
    bool Extend = false);

public sealed record MatchLaw(
    Continuity Match,
    Continuity OtherEnd,
    bool Average = false,
    bool MatchClosestPoints = false,
    PreserveIsoCurveMethod PreserveIso = PreserveIsoCurveMethod.Automatic,
    bool ReverseDirection = false,
    bool ReverseAverageTarget = false,
    bool Refine = false) {
    internal Fin<MatchSrfSettings> Rig(Context domain, Op key) =>
        key.Catch(() => {
            MatchSrfSettings settings = new(match: Match, otherEnd: OtherEnd) {
                Average = Average,
                MatchClosestPoints = MatchClosestPoints,
                PreserveIso = PreserveIso,
                ReverseMatchDirection = ReverseDirection,
                ReverseAverageTargetDirection = ReverseAverageTarget,
            };
            settings.EnableRefinement(
                enabled: Refine,
                positionalTolerance: domain.Absolute.Value,
                angleToleranceRadians: domain.Angle.Value,
                curvatureTolerance: domain.Fractional);
            return Fin.Succ(value: settings);
        });
}
```

## [04]-[OPERATION_RAIL]

- Owner: `SolidSlot` `[SmartEnum<int>]` — the consequence vocabulary; `SolidOp` `[Union]` — the whole verified solid-construction verb roster; `Solids` — the one entry folding any operation spread into one `Built<SolidSlot>`.
- Law: every native side-channel is a fact — union diagnostics land as three `Marks` facts (naked edges, bad intersections, non-manifold edges), the difference index map and join groups land as `SourceMap`/`SourceGroups`, split tolerance escalation lands as `Flag`, and offset blends and walls cross as products behind per-class tallies so the flat product seq partitions by count.
- Law: the rail is value-semantic — `Edit` duplicates its borrowed brep, runs the in-place host member on the working copy, and owns the copy (or the member's returned brep, disposing the copy) as the product; no operation mutates the geometry behind an input handle.
- Law: booleans read the verb's demand column — `Union` refuses a non-empty second set, the pairwise verbs demand one, planar booleans additionally pin the shared plane, and difference always runs the index-map member so source correspondence is never discarded.
- Law: face-addressed operations index inside the borrow — a case carries the handle plus face or edge indices, the arm guards the index against the live `Faces`/`Edges` count, and no `BrepFace` or `BrepEdge` ever crosses a case payload.
- Boundary: sweep, loft, and patch construction is the lofting page's rail; freeform surface and curve construction is the surfaces and curves pages'; this rail owns the solid-topology verbs alone.
- Growth: a new host solid verb is one case with its arm; the spine, the receipt, and every consumer read it with zero new surface.

```csharp
// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class SolidSlot {
    public static readonly SolidSlot Booled = new(key: 0);
    public static readonly SolidSlot NakedEdges = new(key: 1);
    public static readonly SolidSlot BadIntersections = new(key: 2);
    public static readonly SolidSlot NonManifoldEdges = new(key: 3);
    public static readonly SolidSlot Solidified = new(key: 4);
    public static readonly SolidSlot Filleted = new(key: 5);
    public static readonly SolidSlot Trimmed0 = new(key: 6);
    public static readonly SolidSlot Trimmed1 = new(key: 7);
    public static readonly SolidSlot Blended = new(key: 8);
    public static readonly SolidSlot Sectioned = new(key: 9);
    public static readonly SolidSlot Offsets = new(key: 10);
    public static readonly SolidSlot Blends = new(key: 11);
    public static readonly SolidSlot Walls = new(key: 12);
    public static readonly SolidSlot Shelled = new(key: 13);
    public static readonly SolidSlot Piped = new(key: 14);
    public static readonly SolidSlot Seeded = new(key: 15);
    public static readonly SolidSlot Tapered = new(key: 16);
    public static readonly SolidSlot Planar = new(key: 17);
    public static readonly SolidSlot EdgeSurfaced = new(key: 18);
    public static readonly SolidSlot PlaneTrimmed = new(key: 19);
    public static readonly SolidSlot Joined = new(key: 20);
    public static readonly SolidSlot Merged = new(key: 21);
    public static readonly SolidSlot Matched = new(key: 22);
    public static readonly SolidSlot Extended = new(key: 23);
    public static readonly SolidSlot SplitApart = new(key: 24);
    public static readonly SolidSlot Cut = new(key: 25);
    public static readonly SolidSlot Edited = new(key: 26);
    public static readonly SolidSlot Simplified = new(key: 27);
    public static readonly SolidSlot Extruded = new(key: 28);
    public static readonly SolidSlot FilletFace0 = new(key: 29);
    public static readonly SolidSlot FilletFace1 = new(key: 30);
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SolidOp {
    private SolidOp() { }
    public sealed record Boolean(BooleanVerb Verb, Seq<GeometryHandle> First, Seq<GeometryHandle> Second, bool ManifoldOnly = false) : SolidOp;
    public sealed record PlanarBoolean(PlanarBooleanVerb Verb, Plane Plane, Seq<GeometryHandle> First, Seq<GeometryHandle> Second) : SolidOp;
    public sealed record Solidify(Seq<GeometryHandle> Open) : SolidOp;
    public sealed record FilletEdges(GeometryHandle Target, Seq<EdgeFillet> Edges, BlendType Blend, RailType Rail, bool Setback = false) : SolidOp;
    public sealed record FaceFillet(GeometryHandle First, int FirstFace, Point2d FirstUv, GeometryHandle Second, int SecondFace, Point2d SecondUv, FilletShape Shape) : SolidOp;
    public sealed record FaceCurveFillet(GeometryHandle Host, int Face, Point2d Uv, GeometryHandle Along, double Parameter, FilletShape Shape) : SolidOp;
    public sealed record SectionFillet(GeometryHandle First, int FirstFace, Point2d FirstUv, GeometryHandle Second, int SecondFace, Point2d SecondUv, SectionFilletLaw Law) : SolidOp;
    public sealed record BlendSurface(
        GeometryHandle First, int FirstFace, int FirstEdge, Interval FirstDomain, bool FirstReversed, BlendContinuity FirstContinuity,
        GeometryHandle Second, int SecondFace, int SecondEdge, Interval SecondDomain, bool SecondReversed, BlendContinuity SecondContinuity) : SolidOp;
    public sealed record BlendSection(
        GeometryHandle First, int FirstFace, int FirstEdge, double FirstT, bool FirstReversed, BlendContinuity FirstContinuity,
        GeometryHandle Second, int SecondFace, int SecondEdge, double SecondT, bool SecondReversed, BlendContinuity SecondContinuity) : SolidOp;
    public sealed record OffsetSolid(GeometryHandle Target, double Distance, bool Solid, bool Extend, bool Shrink = false) : SolidOp;
    public sealed record FaceOffset(GeometryHandle Target, int Face, double Distance, bool BothSides = false, bool Solid = true) : SolidOp;
    public sealed record Shell(GeometryHandle Target, Seq<int> FacesToRemove, double Distance) : SolidOp;
    public sealed record Pipe(GeometryHandle Rail, PipeLaw Law, bool LocalBlending, PipeCapMode Cap, bool FitRail = false) : SolidOp;
    public sealed record Seed(SolidSeed Value) : SolidOp;
    public sealed record TaperedExtrude(GeometryHandle Profile, double Distance, Vector3d Direction, Point3d BasePoint, double DraftAngleRadians, ExtrudeCornerType Corner) : SolidOp;
    public sealed record TaperedExtrudeRef(GeometryHandle Profile, Vector3d Direction, double Distance, double DraftAngleRadians, Plane Reference) : SolidOp;
    public sealed record PlanarFill(Seq<GeometryHandle> Loops) : SolidOp;
    public sealed record EdgeSurface(Seq<GeometryHandle> Rails) : SolidOp;
    public sealed record TrimmedPlane(Plane Frame, Seq<GeometryHandle> Curves) : SolidOp;
    public sealed record Join(Seq<GeometryHandle> Targets) : SolidOp;
    public sealed record JoinEdges(GeometryHandle First, int FirstEdge, GeometryHandle Second, int SecondEdge) : SolidOp;
    public sealed record Merge(Seq<GeometryHandle> Targets) : SolidOp;
    public sealed record MergeFaces(GeometryHandle First, GeometryHandle Second, Option<(Point2d First, Point2d Second, double Roundness, bool Smooth)> Seam = default) : SolidOp;
    public sealed record Match(GeometryHandle Target, int Edge, Seq<GeometryHandle> TargetCurves, MatchLaw Law) : SolidOp;
    public sealed record ExtendToConnect(GeometryHandle First, int FirstFace, GeometryHandle Second, int SecondFace, ConnectSeed At) : SolidOp;
    public sealed record SplitPieces(GeometryHandle Target) : SolidOp;
    public sealed record SplitBy(GeometryHandle Target, Seq<GeometryHandle> Cutters) : SolidOp;
    public sealed record Trim(GeometryHandle Target, TrimCutter Cutter) : SolidOp;
    public sealed record CutUp(GeometryHandle Source, Seq<GeometryHandle> Curves, bool Flip) : SolidOp;
    public sealed record CopyTrims(GeometryHandle TrimSource, int Face, GeometryHandle SurfaceSource) : SolidOp;
    public sealed record Edit(GeometryHandle Target, SolidEdit Verb) : SolidOp;
    public sealed record Simplify(GeometryHandle Target) : SolidOp;
    public sealed record Lite(ExtrusionSeed Value) : SolidOp;
    public sealed record LiteProfiled(GeometryHandle Target, GeometryHandle Outer, Seq<GeometryHandle> Inners, bool Cap, Option<(Point3d A, Point3d B, Vector3d Up)> Path = default) : SolidOp;
    public sealed record LiteHeavy(GeometryHandle Target, bool SplitKinkyFaces = true) : SolidOp;

    internal Fin<Built<SolidSlot>> Apply(Context domain) =>
        Switch(
            context: domain,
            boolean: static (model, edit) => {
                Op op = Op.Of(name: nameof(Boolean));
                return ModelGate.BorrowMany<Brep, Built<SolidSlot>>(handles: edit.First, key: op, body: first =>
                    ModelGate.BorrowMany<Brep, Built<SolidSlot>>(handles: edit.Second, key: op, allowEmpty: !edit.Verb.RequiresSecond, body: second =>
                        from _ in guard(edit.Verb.RequiresSecond == !second.IsEmpty, op.InvalidInput())
                        from built in edit.Verb.Switch(
                            state: (First: first, Second: second, Manifold: edit.ManifoldOnly, Tolerance: model.Absolute.Value, Op: op),
                            union: static ctx => ctx.Op.Catch(() => {
                                Brep[] products = Brep.CreateBooleanUnion(
                                    breps: ctx.First.AsIterable(), tolerance: ctx.Tolerance, manifoldOnly: ctx.Manifold,
                                    nakedEdgePoints: out Point3d[] naked, badIntersectionPoints: out Point3d[] bad, nonManifoldEdgePoints: out Point3d[] nonManifold);
                                return ModelGate.OwnMany(built: products, key: ctx.Op).Map(owned => new Built<SolidSlot>(
                                    Products: owned,
                                    Evidence: BuildReceipt<SolidSlot>.Of(slot: SolidSlot.Booled, body: new BuildBody.Tally(Count: owned.Count))
                                        + BuildReceipt<SolidSlot>.Of(slot: SolidSlot.NakedEdges, body: new BuildBody.Marks(Points: toSeq(naked ?? [])))
                                        + BuildReceipt<SolidSlot>.Of(slot: SolidSlot.BadIntersections, body: new BuildBody.Marks(Points: toSeq(bad ?? [])))
                                        + BuildReceipt<SolidSlot>.Of(slot: SolidSlot.NonManifoldEdges, body: new BuildBody.Marks(Points: toSeq(nonManifold ?? [])))));
                            }),
                            intersection: static ctx => Booled(ctx.Op, () => Brep.CreateBooleanIntersection(
                                firstSet: ctx.First.AsIterable(), secondSet: ctx.Second.AsIterable(), tolerance: ctx.Tolerance, manifoldOnly: ctx.Manifold)),
                            difference: static ctx => ctx.Op.Catch(() => {
                                Brep[] products = Brep.CreateBooleanDifferenceWithIndexMap(
                                    firstSet: ctx.First.AsIterable(), secondSet: ctx.Second.AsIterable(),
                                    tolerance: ctx.Tolerance, manifoldOnly: ctx.Manifold, indexMap: out int[] map);
                                return ModelGate.OwnMany(built: products, key: ctx.Op).Map(owned => new Built<SolidSlot>(
                                    Products: owned,
                                    Evidence: BuildReceipt<SolidSlot>.Of(slot: SolidSlot.Booled, body: new BuildBody.Tally(Count: owned.Count))
                                        + BuildReceipt<SolidSlot>.Of(slot: SolidSlot.Booled, body: new BuildBody.SourceMap(Rows: toSeq(map ?? [])))));
                            }),
                            split: static ctx => Booled(ctx.Op, () => Brep.CreateBooleanSplit(
                                firstSet: ctx.First.AsIterable(), secondSet: ctx.Second.AsIterable(), tolerance: ctx.Tolerance)))
                        select built));
            },
            planarBoolean: static (model, edit) => {
                Op op = Op.Of(name: nameof(PlanarBoolean));
                return ModelGate.BorrowMany<Brep, Built<SolidSlot>>(handles: edit.First, key: op, body: first =>
                    ModelGate.BorrowMany<Brep, Built<SolidSlot>>(handles: edit.Second, key: op, allowEmpty: !edit.Verb.RequiresSecond, body: second =>
                        from _ in guard(edit.Verb.RequiresSecond == !second.IsEmpty, op.InvalidInput())
                        from built in edit.Verb.Switch(
                            state: (First: first, Second: second, Plane: edit.Plane, Tolerance: model.Absolute.Value, Op: op),
                            union: static ctx => Booled(ctx.Op, () => Brep.CreatePlanarUnion(
                                breps: ctx.First.AsIterable(), plane: ctx.Plane, tolerance: ctx.Tolerance)),
                            intersection: static ctx => Paired(ctx.First, ctx.Second, ctx.Op, (a, b) => Booled(ctx.Op, () =>
                                Brep.CreatePlanarIntersection(b0: a, b1: b, plane: ctx.Plane, tolerance: ctx.Tolerance))),
                            difference: static ctx => Paired(ctx.First, ctx.Second, ctx.Op, (a, b) => Booled(ctx.Op, () =>
                                Brep.CreatePlanarDifference(b0: a, b1: b, plane: ctx.Plane, tolerance: ctx.Tolerance))))
                        select built));
            },
            solidify: static (model, edit) => {
                Op op = Op.Of(name: nameof(Solidify));
                return ModelGate.BorrowMany<Brep, Built<SolidSlot>>(handles: edit.Open, key: op, body: open =>
                    Many(op, SolidSlot.Solidified, () => Brep.CreateSolid(breps: open.AsIterable(), tolerance: model.Absolute.Value)));
            },
            filletEdges: static (model, edit) => {
                Op op = Op.Of(name: nameof(FilletEdges));
                return ModelGate.Borrow<Brep, Built<SolidSlot>>(handle: edit.Target, key: op, body: target =>
                    from _ in guard(!edit.Edges.IsEmpty && edit.Edges.ForAll(row => row.Edge >= 0 && row.Edge < target.Edges.Count), op.InvalidInput())
                    from built in edit.Edges.Exists(static row => row.Law is RadiusLaw.Profiled)
                        ? Many(op, SolidSlot.Filleted, () => Brep.CreateFilletEdgesVariableRadius(
                            brep: target,
                            edgeIndices: edit.Edges.Map(static row => row.Edge).AsIterable(),
                            edgeDistances: edit.Edges.Fold(
                                new System.Collections.Generic.Dictionary<int, System.Collections.Generic.IList<BrepEdgeFilletDistance>>(),
                                (map, row) => {
                                    map[row.Edge] = row.Law switch {
                                        RadiusLaw.Profiled profiled => [.. profiled.Rows.Map(static point =>
                                            new BrepEdgeFilletDistance(edgeParameter: point.Parameter, filletDistance: point.Distance))],
                                        RadiusLaw.Constant constant => [
                                            new BrepEdgeFilletDistance(edgeParameter: target.Edges[row.Edge].Domain.Min, filletDistance: constant.Start),
                                            new BrepEdgeFilletDistance(edgeParameter: target.Edges[row.Edge].Domain.Max, filletDistance: constant.End)],
                                        _ => [],
                                    };
                                    return map;
                                }),
                            blendType: edit.Blend, railType: edit.Rail, setbackFillets: edit.Setback,
                            tolerance: model.Absolute.Value, angleTolerance: model.Angle.Value))
                        : Many(op, SolidSlot.Filleted, () => Brep.CreateFilletEdges(
                            brep: target,
                            edgeIndices: edit.Edges.Map(static row => row.Edge).AsIterable(),
                            startRadii: edit.Edges.Map(static row => ((RadiusLaw.Constant)row.Law).Start).AsIterable(),
                            endRadii: edit.Edges.Map(static row => ((RadiusLaw.Constant)row.Law).End).AsIterable(),
                            blendType: edit.Blend, railType: edit.Rail, setbackFillets: edit.Setback,
                            tolerance: model.Absolute.Value, angleTolerance: model.Angle.Value))
                    select built);
            },
            faceFillet: static (model, edit) => {
                Op op = Op.Of(name: nameof(FaceFillet));
                return ModelGate.Borrow<Brep, Built<SolidSlot>>(handle: edit.First, key: op, body: first =>
                    ModelGate.Borrow<Brep, Built<SolidSlot>>(handle: edit.Second, key: op, body: second =>
                        from _ in guard(edit.FirstFace >= 0 && edit.FirstFace < first.Faces.Count && edit.SecondFace >= 0 && edit.SecondFace < second.Faces.Count, op.InvalidInput())
                        from settings in edit.Shape.Rig(domain: model, key: op)
                        from built in op.Catch(() =>
                            op.Confirm(success: Brep.CreateFilletSurface(
                                face0: first.Faces[edit.FirstFace], uv0: edit.FirstUv,
                                face1: second.Faces[edit.SecondFace], uv1: edit.SecondUv,
                                settings: settings, results: out Brep.FilletSurfaceResults results))
                            .Bind(_ => Harvested(results: results, op: op)))
                        select built));
            },
            faceCurveFillet: static (model, edit) => {
                Op op = Op.Of(name: nameof(FaceCurveFillet));
                return ModelGate.Borrow<Brep, Built<SolidSlot>>(handle: edit.Host, key: op, body: host =>
                    ModelGate.Borrow<Curve, Built<SolidSlot>>(handle: edit.Along, key: op, body: along =>
                        from _ in guard(edit.Face >= 0 && edit.Face < host.Faces.Count, op.InvalidInput())
                        from settings in edit.Shape.Rig(domain: model, key: op)
                        from built in op.Catch(() =>
                            op.Confirm(success: Brep.CreateFilletSurfaceCurve(
                                face: host.Faces[edit.Face], uv: edit.Uv, curve: along, t: edit.Parameter,
                                settings: settings, results: out Brep.FilletSurfaceResults results))
                            .Bind(_ => Harvested(results: results, op: op)))
                        select built));
            },
            sectionFillet: static (model, edit) => {
                Op op = Op.Of(name: nameof(SectionFillet));
                return ModelGate.Borrow<Brep, Built<SolidSlot>>(handle: edit.First, key: op, body: first =>
                    ModelGate.Borrow<Brep, Built<SolidSlot>>(handle: edit.Second, key: op, body: second =>
                        from _ in guard(
                            edit.FirstFace >= 0 && edit.FirstFace < first.Faces.Count
                            && edit.SecondFace >= 0 && edit.SecondFace < second.Faces.Count
                            && edit.Law.Profile.IsAdmitted,
                            op.InvalidInput())
                        from built in SectionFilleted(
                            first: first.Faces[edit.FirstFace], firstUv: edit.FirstUv,
                            second: second.Faces[edit.SecondFace], secondUv: edit.SecondUv,
                            law: edit.Law, tolerance: model.Absolute.Value, op: op)
                        select built));
            },
            blendSurface: static (_, edit) => {
                Op op = Op.Of(name: nameof(BlendSurface));
                return ModelGate.Borrow<Brep, Built<SolidSlot>>(handle: edit.First, key: op, body: first =>
                    ModelGate.Borrow<Brep, Built<SolidSlot>>(handle: edit.Second, key: op, body: second =>
                        from _ in guard(
                            edit.FirstFace >= 0 && edit.FirstFace < first.Faces.Count && edit.FirstEdge >= 0 && edit.FirstEdge < first.Edges.Count
                            && edit.SecondFace >= 0 && edit.SecondFace < second.Faces.Count && edit.SecondEdge >= 0 && edit.SecondEdge < second.Edges.Count,
                            op.InvalidInput())
                        from built in Many(op, SolidSlot.Blended, () => Brep.CreateBlendSurface(
                            face0: first.Faces[edit.FirstFace], edge0: first.Edges[edit.FirstEdge], domain0: edit.FirstDomain, rev0: edit.FirstReversed, continuity0: edit.FirstContinuity,
                            face1: second.Faces[edit.SecondFace], edge1: second.Edges[edit.SecondEdge], domain1: edit.SecondDomain, rev1: edit.SecondReversed, continuity1: edit.SecondContinuity))
                        select built));
            },
            blendSection: static (_, edit) => {
                Op op = Op.Of(name: nameof(BlendSection));
                return ModelGate.Borrow<Brep, Built<SolidSlot>>(handle: edit.First, key: op, body: first =>
                    ModelGate.Borrow<Brep, Built<SolidSlot>>(handle: edit.Second, key: op, body: second =>
                        from _ in guard(
                            edit.FirstFace >= 0 && edit.FirstFace < first.Faces.Count && edit.FirstEdge >= 0 && edit.FirstEdge < first.Edges.Count
                            && edit.SecondFace >= 0 && edit.SecondFace < second.Faces.Count && edit.SecondEdge >= 0 && edit.SecondEdge < second.Edges.Count,
                            op.InvalidInput())
                        from built in Single(op, SolidSlot.Sectioned, () => Brep.CreateBlendShape(
                            face0: first.Faces[edit.FirstFace], edge0: first.Edges[edit.FirstEdge], t0: edit.FirstT, rev0: edit.FirstReversed, continuity0: edit.FirstContinuity,
                            face1: second.Faces[edit.SecondFace], edge1: second.Edges[edit.SecondEdge], t1: edit.SecondT, rev1: edit.SecondReversed, continuity1: edit.SecondContinuity))
                        select built));
            },
            offsetSolid: static (model, edit) => {
                Op op = Op.Of(name: nameof(OffsetSolid));
                return ModelGate.Borrow<Brep, Built<SolidSlot>>(handle: edit.Target, key: op, body: target =>
                    op.Catch(() => {
                        Brep[] offsets = Brep.CreateOffsetBrep(
                            brep: target, distance: edit.Distance, solid: edit.Solid, extend: edit.Extend, shrink: edit.Shrink,
                            tolerance: model.Absolute.Value, outBlends: out Brep[] blends, outWalls: out Brep[] walls);
                        return
                            from primary in ModelGate.OwnMany(built: offsets, key: op)
                            from blended in ModelGate.OwnMany(built: blends, key: op, allowEmpty: true).MapFail(error => { _ = primary.Iter(static h => h.Dispose()); return error; })
                            from walled in ModelGate.OwnMany(built: walls, key: op, allowEmpty: true).MapFail(error => {
                                _ = primary.Iter(static h => h.Dispose());
                                _ = blended.Iter(static h => h.Dispose());
                                return error;
                            })
                            select new Built<SolidSlot>(
                                Products: primary + blended + walled,
                                Evidence: BuildReceipt<SolidSlot>.Of(slot: SolidSlot.Offsets, body: new BuildBody.Tally(Count: primary.Count))
                                    + BuildReceipt<SolidSlot>.Of(slot: SolidSlot.Blends, body: new BuildBody.Tally(Count: blended.Count))
                                    + BuildReceipt<SolidSlot>.Of(slot: SolidSlot.Walls, body: new BuildBody.Tally(Count: walled.Count)));
                    }));
            },
            faceOffset: static (model, edit) => {
                Op op = Op.Of(name: nameof(FaceOffset));
                return ModelGate.Borrow<Brep, Built<SolidSlot>>(handle: edit.Target, key: op, body: target =>
                    from _ in guard(edit.Face >= 0 && edit.Face < target.Faces.Count, op.InvalidInput())
                    from built in Single(op, SolidSlot.Offsets, () => Brep.CreateFromOffsetFace(
                        face: target.Faces[edit.Face], offsetDistance: edit.Distance,
                        offsetTolerance: model.Absolute.Value, bothSides: edit.BothSides, createSolid: edit.Solid))
                    select built);
            },
            shell: static (model, edit) => {
                Op op = Op.Of(name: nameof(Shell));
                return ModelGate.Borrow<Brep, Built<SolidSlot>>(handle: edit.Target, key: op, body: target =>
                    from _ in guard(edit.FacesToRemove.ForAll(face => face >= 0 && face < target.Faces.Count), op.InvalidInput())
                    from built in Many(op, SolidSlot.Shelled, () => Brep.CreateShell(
                        brep: target, facesToRemove: edit.FacesToRemove.AsIterable(), distance: edit.Distance, tolerance: model.Absolute.Value))
                    select built);
            },
            pipe: static (model, edit) => {
                Op op = Op.Of(name: nameof(Pipe));
                return ModelGate.Borrow<Curve, Built<SolidSlot>>(handle: edit.Rail, key: op, body: rail =>
                    edit.Law.Switch(
                        state: (Rail: rail, Edit: edit, Tolerance: model.Absolute.Value, Angle: model.Angle.Value, Op: op),
                        constant: static (ctx, law) => Many(ctx.Op, SolidSlot.Piped, () => Brep.CreatePipe(
                            rail: ctx.Rail, radius: law.Radius, localBlending: ctx.Edit.LocalBlending, cap: ctx.Edit.Cap,
                            fitRail: ctx.Edit.FitRail, absoluteTolerance: ctx.Tolerance, angleToleranceRadians: ctx.Angle)),
                        variable: static (ctx, law) => Many(ctx.Op, SolidSlot.Piped, () => Brep.CreatePipe(
                            rail: ctx.Rail, railRadiiParameters: law.Rows.Map(static row => row.Parameter).AsIterable(),
                            radii: law.Rows.Map(static row => row.Radius).AsIterable(), localBlending: ctx.Edit.LocalBlending,
                            cap: ctx.Edit.Cap, fitRail: ctx.Edit.FitRail, absoluteTolerance: ctx.Tolerance, angleToleranceRadians: ctx.Angle)),
                        thick: static (ctx, law) => Many(ctx.Op, SolidSlot.Piped, () => Brep.CreateThickPipe(
                            rail: ctx.Rail, radius0: law.Radius0, radius1: law.Radius1, localBlending: ctx.Edit.LocalBlending,
                            cap: ctx.Edit.Cap, fitRail: ctx.Edit.FitRail, absoluteTolerance: ctx.Tolerance, angleToleranceRadians: ctx.Angle)),
                        thickVariable: static (ctx, law) => Many(ctx.Op, SolidSlot.Piped, () => Brep.CreateThickPipe(
                            rail: ctx.Rail, railRadiiParameters: law.Rows.Map(static row => row.Parameter).AsIterable(),
                            radii0: law.Rows.Map(static row => row.Inner).AsIterable(), radii1: law.Rows.Map(static row => row.Outer).AsIterable(),
                            localBlending: ctx.Edit.LocalBlending, cap: ctx.Edit.Cap, fitRail: ctx.Edit.FitRail,
                            absoluteTolerance: ctx.Tolerance, angleToleranceRadians: ctx.Angle))));
            },
            seed: static (model, edit) => {
                Op op = Op.Of(name: nameof(Seed));
                return edit.Value.Build(domain: model, key: op).Map(product => new Built<SolidSlot>(
                    Products: Seq(product),
                    Evidence: BuildReceipt<SolidSlot>.Of(slot: SolidSlot.Seeded, body: new BuildBody.Tally(Count: 1))));
            },
            taperedExtrude: static (model, edit) => {
                Op op = Op.Of(name: nameof(TaperedExtrude));
                return ModelGate.Borrow<Curve, Built<SolidSlot>>(handle: edit.Profile, key: op, body: profile =>
                    Many(op, SolidSlot.Tapered, () => Brep.CreateFromTaperedExtrude(
                        curveToExtrude: profile, distance: edit.Distance, direction: edit.Direction, basePoint: edit.BasePoint,
                        draftAngleRadians: edit.DraftAngleRadians, cornerType: edit.Corner,
                        tolerance: model.Absolute.Value, angleToleranceRadians: model.Angle.Value)));
            },
            taperedExtrudeRef: static (model, edit) => {
                Op op = Op.Of(name: nameof(TaperedExtrudeRef));
                return ModelGate.Borrow<Curve, Built<SolidSlot>>(handle: edit.Profile, key: op, body: profile =>
                    Many(op, SolidSlot.Tapered, () => Brep.CreateFromTaperedExtrudeWithRef(
                        curve: profile, direction: edit.Direction, distance: edit.Distance,
                        draftAngle: edit.DraftAngleRadians, plane: edit.Reference, tolerance: model.Absolute.Value)));
            },
            planarFill: static (model, edit) => {
                Op op = Op.Of(name: nameof(PlanarFill));
                return ModelGate.BorrowMany<Curve, Built<SolidSlot>>(handles: edit.Loops, key: op, body: loops =>
                    Many(op, SolidSlot.Planar, () => Brep.CreatePlanarBreps(inputLoops: loops.AsIterable(), tolerance: model.Absolute.Value)));
            },
            edgeSurface: static (_, edit) => {
                Op op = Op.Of(name: nameof(EdgeSurface));
                return ModelGate.BorrowMany<Curve, Built<SolidSlot>>(handles: edit.Rails, key: op, body: rails =>
                    from _ in guard(rails.Count >= 2 && rails.Count <= 4, op.InvalidInput())
                    from built in Single(op, SolidSlot.EdgeSurfaced, () => Brep.CreateEdgeSurface(curves: rails.AsIterable()))
                    select built);
            },
            trimmedPlane: static (_, edit) => {
                Op op = Op.Of(name: nameof(TrimmedPlane));
                return ModelGate.BorrowMany<Curve, Built<SolidSlot>>(handles: edit.Curves, key: op, body: curves =>
                    Single(op, SolidSlot.PlaneTrimmed, () => Brep.CreateTrimmedPlane(plane: edit.Frame, curves: curves.AsIterable())));
            },
            join: static (model, edit) => {
                Op op = Op.Of(name: nameof(Join));
                return ModelGate.BorrowMany<Brep, Built<SolidSlot>>(handles: edit.Targets, key: op, body: targets =>
                    op.Catch(() => {
                        Brep[] joined = Brep.JoinBreps(
                            brepsToJoin: targets.AsIterable(), tolerance: model.Absolute.Value,
                            angleTolerance: model.Angle.Value,
                            indexMap: out System.Collections.Generic.List<int[]> map);
                        return ModelGate.OwnMany(built: joined, key: op).Map(owned => new Built<SolidSlot>(
                            Products: owned,
                            Evidence: BuildReceipt<SolidSlot>.Of(slot: SolidSlot.Joined, body: new BuildBody.Tally(Count: owned.Count))
                                + BuildReceipt<SolidSlot>.Of(slot: SolidSlot.Joined, body: new BuildBody.SourceGroups(
                                    Groups: toSeq(map ?? []).Map(static rows => toSeq(rows))))));
                    }));
            },
            joinEdges: static (model, edit) => {
                Op op = Op.Of(name: nameof(JoinEdges));
                return ModelGate.Borrow<Brep, Built<SolidSlot>>(handle: edit.First, key: op, body: first =>
                    ModelGate.Borrow<Brep, Built<SolidSlot>>(handle: edit.Second, key: op, body: second =>
                        from _ in guard(
                            edit.FirstEdge >= 0 && edit.FirstEdge < first.Edges.Count && edit.SecondEdge >= 0 && edit.SecondEdge < second.Edges.Count,
                            op.InvalidInput())
                        from built in Single(op, SolidSlot.Joined, () => Brep.CreateFromJoinedEdges(
                            brep0: first, edgeIndex0: edit.FirstEdge, brep1: second, edgeIndex1: edit.SecondEdge, joinTolerance: model.Absolute.Value))
                        select built));
            },
            merge: static (model, edit) => {
                Op op = Op.Of(name: nameof(Merge));
                return ModelGate.BorrowMany<Brep, Built<SolidSlot>>(handles: edit.Targets, key: op, body: targets =>
                    Single(op, SolidSlot.Merged, () => Brep.MergeBreps(brepsToMerge: targets.AsIterable(), tolerance: model.Absolute.Value)));
            },
            mergeFaces: static (model, edit) => {
                Op op = Op.Of(name: nameof(MergeFaces));
                return ModelGate.Borrow<Brep, Built<SolidSlot>>(handle: edit.First, key: op, body: first =>
                    ModelGate.Borrow<Brep, Built<SolidSlot>>(handle: edit.Second, key: op, body: second =>
                        Single(op, SolidSlot.Merged, () => edit.Seam.Case switch {
                            (Point2d p0, Point2d p1, double roundness, bool smooth) => Brep.MergeSurfaces(
                                brep0: first, brep1: second, tolerance: model.Absolute.Value, angleToleranceRadians: model.Angle.Value,
                                point0: p0, point1: p1, roundness: roundness, smooth: smooth),
                            _ => Brep.MergeSurfaces(brep0: first, brep1: second, tolerance: model.Absolute.Value, angleToleranceRadians: model.Angle.Value),
                        })));
            },
            match: static (model, edit) => {
                Op op = Op.Of(name: nameof(Match));
                return ModelGate.Borrow<Brep, Built<SolidSlot>>(handle: edit.Target, key: op, body: target =>
                    ModelGate.BorrowMany<Curve, Built<SolidSlot>>(handles: edit.TargetCurves, key: op, body: curves =>
                        from _ in guard(edit.Edge >= 0 && edit.Edge < target.Edges.Count, op.InvalidInput())
                        from settings in edit.Law.Rig(domain: model, key: op)
                        from built in op.Catch(() =>
                            op.Confirm(success: Brep.CreateFromMatch(
                                edge: target.Edges[edit.Edge], targetCurves: curves.AsIterable(), settings: settings,
                                matched: out Brep matched, target: out Brep matchTarget))
                            .Bind(_ =>
                                from owned in ModelGate.Own(built: matched, key: op)
                                from other in ModelGate.Own(built: matchTarget, key: op).MapFail(error => { owned.Dispose(); return error; })
                                select new Built<SolidSlot>(
                                    Products: Seq(owned, other),
                                    Evidence: BuildReceipt<SolidSlot>.Of(slot: SolidSlot.Matched, body: new BuildBody.Tally(Count: 2)))))
                        select built));
            },
            extendToConnect: static (model, edit) => {
                Op op = Op.Of(name: nameof(ExtendToConnect));
                return ModelGate.Borrow<Brep, Built<SolidSlot>>(handle: edit.First, key: op, body: first =>
                    ModelGate.Borrow<Brep, Built<SolidSlot>>(handle: edit.Second, key: op, body: second =>
                        from _ in guard(edit.FirstFace >= 0 && edit.FirstFace < first.Faces.Count && edit.SecondFace >= 0 && edit.SecondFace < second.Faces.Count, op.InvalidInput())
                        from __ in edit.At.Case switch {
                            ConnectSeed.AtEdges at => guard(
                                at.FirstEdge >= 0 && at.FirstEdge < first.Edges.Count && at.SecondEdge >= 0 && at.SecondEdge < second.Edges.Count,
                                op.InvalidInput()),
                            _ => Fin.Succ(value: unit),
                        }
                        from built in op.Catch(() => {
                            Brep extended0 = null!;
                            Brep extended1 = null!;
                            bool connected = edit.At.Switch(
                                state: (First: first.Faces[edit.FirstFace], Second: second.Faces[edit.SecondFace], Tol: model.Absolute.Value, Angle: model.Angle.Value),
                                atEdges: (ctx, at) => Brep.ExtendBrepFacesToConnect(
                                    Face0: ctx.First, edgeIndex0: at.FirstEdge, Face1: ctx.Second, edgeIndex1: at.SecondEdge,
                                    tol: ctx.Tol, angleTol: ctx.Angle, outBrep0: out extended0, outBrep1: out extended1),
                                atPoints: (ctx, at) => Brep.ExtendBrepFacesToConnect(
                                    Face0: ctx.First, f0_sel_pt: at.First, Face1: ctx.Second, f1_sel_pt: at.Second,
                                    tol: ctx.Tol, angleTol: ctx.Angle, outBrep0: out extended0, outBrep1: out extended1));
                            return op.Confirm(success: connected).Bind(_ =>
                                from owned in ModelGate.Own(built: extended0, key: op)
                                from other in ModelGate.Own(built: extended1, key: op).MapFail(error => { owned.Dispose(); return error; })
                                select new Built<SolidSlot>(
                                    Products: Seq(owned, other),
                                    Evidence: BuildReceipt<SolidSlot>.Of(slot: SolidSlot.Extended, body: new BuildBody.Tally(Count: 2))));
                        })
                        select built));
            },
            splitPieces: static (_, edit) => {
                Op op = Op.Of(name: nameof(SplitPieces));
                return ModelGate.Borrow<Brep, Built<SolidSlot>>(handle: edit.Target, key: op, body: target =>
                    op.Catch(() => {
                        Brep[] pieces = Brep.SplitDisjointPieces(brep: target, indexMap: out System.Collections.Generic.List<int[]> map);
                        return ModelGate.OwnMany(built: pieces, key: op).Map(owned => new Built<SolidSlot>(
                            Products: owned,
                            Evidence: BuildReceipt<SolidSlot>.Of(slot: SolidSlot.SplitApart, body: new BuildBody.Tally(Count: owned.Count))
                                + BuildReceipt<SolidSlot>.Of(slot: SolidSlot.SplitApart, body: new BuildBody.SourceGroups(
                                    Groups: toSeq(map ?? []).Map(static rows => toSeq(rows))))));
                    }));
            },
            splitBy: static (model, edit) => {
                Op op = Op.Of(name: nameof(SplitBy));
                return ModelGate.Borrow<Brep, Built<SolidSlot>>(handle: edit.Target, key: op, body: target =>
                    ModelGate.BorrowMany<Brep, Built<SolidSlot>>(handles: edit.Cutters, key: op, body: cutters =>
                        cutters.Count == 1
                            ? op.Catch(() => {
                                Brep[] pieces = target.Split(cutter: cutters[0], intersectionTolerance: model.Absolute.Value, toleranceWasRaised: out bool raised);
                                return ModelGate.OwnMany(built: pieces, key: op).Map(owned => new Built<SolidSlot>(
                                    Products: owned,
                                    Evidence: BuildReceipt<SolidSlot>.Of(slot: SolidSlot.SplitApart, body: new BuildBody.Tally(Count: owned.Count))
                                        + BuildReceipt<SolidSlot>.Of(slot: SolidSlot.SplitApart, body: new BuildBody.Flag(Value: raised))));
                            })
                            : Many(op, SolidSlot.SplitApart, () => target.Split(cutters: cutters.AsIterable(), intersectionTolerance: model.Absolute.Value))));
            },
            trim: static (model, edit) => {
                Op op = Op.Of(name: nameof(Trim));
                return ModelGate.Borrow<Brep, Built<SolidSlot>>(handle: edit.Target, key: op, body: target =>
                    edit.Cutter.Switch(
                        state: (Target: target, Tolerance: model.Absolute.Value, Op: op),
                        byBrep: static (ctx, cutter) => ModelGate.Borrow<Brep, Built<SolidSlot>>(handle: cutter.Cutter, key: ctx.Op,
                            body: blade => Many(ctx.Op, SolidSlot.Cut, () => ctx.Target.Trim(cutter: blade, intersectionTolerance: ctx.Tolerance))),
                        byPlane: static (ctx, cutter) => Many(ctx.Op, SolidSlot.Cut, () => ctx.Target.Trim(cutter: cutter.Cutter, intersectionTolerance: ctx.Tolerance))));
            },
            cutUp: static (model, edit) => {
                Op op = Op.Of(name: nameof(CutUp));
                return ModelGate.Borrow<Surface, Built<SolidSlot>>(handle: edit.Source, key: op, body: surface =>
                    ModelGate.BorrowMany<Curve, Built<SolidSlot>>(handles: edit.Curves, key: op, body: curves =>
                        Many(op, SolidSlot.Cut, () => Brep.CutUpSurface(
                            surface: surface, curves: curves.AsIterable(), flip: edit.Flip,
                            fitTolerance: model.Absolute.Value, keepTolerance: model.Absolute.Value))));
            },
            copyTrims: static (model, edit) => {
                Op op = Op.Of(name: nameof(CopyTrims));
                return ModelGate.Borrow<Brep, Built<SolidSlot>>(handle: edit.TrimSource, key: op, body: source =>
                    ModelGate.Borrow<Surface, Built<SolidSlot>>(handle: edit.SurfaceSource, key: op, body: surface =>
                        from _ in guard(edit.Face >= 0 && edit.Face < source.Faces.Count, op.InvalidInput())
                        from built in Single(op, SolidSlot.Cut, () => Brep.CopyTrimCurves(
                            trimSource: source.Faces[edit.Face], surfaceSource: surface, tolerance: model.Absolute.Value))
                        select built);
            },
            edit: static (model, request) => {
                Op op = Op.Of(name: nameof(Edit));
                return ModelGate.Borrow<Brep, Built<SolidSlot>>(handle: request.Target, key: op, body: source =>
                    op.Catch(() => Optional(source.DuplicateBrep()).ToFin(Fail: op.InvalidResult()).Bind(working =>
                        Edited(working: working, verb: request.Verb, domain: model, op: op).MapFail(error => {
                            working.Dispose();
                            return error;
                        }))));
            },
            simplify: static (_, edit) => {
                Op op = Op.Of(name: nameof(Simplify));
                return ModelGate.Borrow<GeometryBase, Built<SolidSlot>>(handle: edit.Target, key: op, body: source =>
                    Single(op, SolidSlot.Simplified, () => Brep.TryConvertBrep(geometry: source)));
            },
            lite: static (_, edit) => {
                Op op = Op.Of(name: nameof(Lite));
                return edit.Value.Build(key: op).Map(product => new Built<SolidSlot>(
                    Products: Seq(product),
                    Evidence: BuildReceipt<SolidSlot>.Of(slot: SolidSlot.Extruded, body: new BuildBody.Tally(Count: 1))));
            },
            liteProfiled: static (_, edit) => {
                Op op = Op.Of(name: nameof(LiteProfiled));
                return ModelGate.Borrow<Extrusion, Built<SolidSlot>>(handle: edit.Target, key: op, body: source =>
                    ModelGate.Borrow<Curve, Built<SolidSlot>>(handle: edit.Outer, key: op, body: outer =>
                        ModelGate.BorrowMany<Curve, Built<SolidSlot>>(handles: edit.Inners, key: op, allowEmpty: true, body: inners =>
                            op.Catch(() => Optional(source.Duplicate() as Extrusion).ToFin(Fail: op.InvalidResult()).Bind(working => (
                                from _ in edit.Path.Case switch {
                                    (Point3d a, Point3d b, Vector3d up) => op.Confirm(success: working.SetPathAndUp(a: a, b: b, up: up)),
                                    _ => Fin.Succ(value: unit),
                                }
                                from __ in op.Confirm(success: working.SetOuterProfile(outerProfile: outer, cap: edit.Cap))
                                from ___ in inners.Fold(Fin.Succ(value: unit), (state, inner) =>
                                    state.Bind(_ => op.Confirm(success: working.AddInnerProfile(innerProfile: inner))))
                                from owned in ModelGate.Own(built: working, key: op)
                                select new Built<SolidSlot>(
                                    Products: Seq(owned),
                                    Evidence: BuildReceipt<SolidSlot>.Of(slot: SolidSlot.Extruded, body: new BuildBody.Tally(Count: 1))))
                                .MapFail(error => {
                                    working.Dispose();
                                    return error;
                                })))));
            },
            liteHeavy: static (_, edit) => {
                Op op = Op.Of(name: nameof(LiteHeavy));
                return ModelGate.Borrow<Extrusion, Built<SolidSlot>>(handle: edit.Target, key: op, body: source =>
                    Single(op, SolidSlot.Extruded, () => source.ToBrep(splitKinkyFaces: edit.SplitKinkyFaces)));
            });

    private static Fin<Built<SolidSlot>> Booled(Op op, Func<Brep[]> run) =>
        Many(op, SolidSlot.Booled, run);

    private static Fin<Built<SolidSlot>> Many(Op op, SolidSlot slot, Func<System.Collections.Generic.IEnumerable<Brep>> run) =>
        op.Catch(() => ModelGate.OwnMany(built: run(), key: op).Map(owned => new Built<SolidSlot>(
            Products: owned,
            Evidence: BuildReceipt<SolidSlot>.Of(slot: slot, body: new BuildBody.Tally(Count: owned.Count)))));

    private static Fin<Built<SolidSlot>> Single(Op op, SolidSlot slot, Func<GeometryBase?> run) =>
        op.Catch(() => ModelGate.Own(built: run(), key: op).Map(owned => new Built<SolidSlot>(
            Products: Seq(owned),
            Evidence: BuildReceipt<SolidSlot>.Of(slot: slot, body: new BuildBody.Tally(Count: 1)))));

    private static Fin<Built<SolidSlot>> Paired(Seq<Brep> first, Seq<Brep> second, Op op, Func<Brep, Brep, Fin<Built<SolidSlot>>> run) =>
        first.Count == 1 && second.Count == 1
            ? run(first[0], second[0])
            : Fin.Fail<Built<SolidSlot>>(error: op.InvalidInput());

    private static Fin<Built<SolidSlot>> Harvested(Brep.FilletSurfaceResults results, Op op) =>
        from face0 in FilletFace(face: results.Face0, op: op)
        from face1 in FilletFace(face: results.Face1, op: op).MapFail(error => { _ = face0.Iter(static h => h.Dispose()); return error; })
        from fillets in ModelGate.OwnMany(built: results.Fillets, key: op).MapFail(error => {
            _ = face0.Iter(static h => h.Dispose());
            _ = face1.Iter(static h => h.Dispose());
            return error;
        })
        from trimmed0 in ModelGate.OwnMany(built: results.OutBreps0, key: op, allowEmpty: true).MapFail(error => {
            _ = face0.Iter(static h => h.Dispose());
            _ = face1.Iter(static h => h.Dispose());
            _ = fillets.Iter(static h => h.Dispose());
            return error;
        })
        from trimmed1 in ModelGate.OwnMany(built: results.OutBreps1, key: op, allowEmpty: true).MapFail(error => {
            _ = face0.Iter(static h => h.Dispose());
            _ = face1.Iter(static h => h.Dispose());
            _ = fillets.Iter(static h => h.Dispose());
            _ = trimmed0.Iter(static h => h.Dispose());
            return error;
        })
        select new Built<SolidSlot>(
            Products: face0 + face1 + fillets + trimmed0 + trimmed1,
            Evidence: BuildReceipt<SolidSlot>.Of(slot: SolidSlot.Filleted, body: new BuildBody.Tally(Count: fillets.Count))
                + BuildReceipt<SolidSlot>.Of(slot: SolidSlot.FilletFace0, body: new BuildBody.Tally(Count: face0.Count))
                + BuildReceipt<SolidSlot>.Of(slot: SolidSlot.FilletFace1, body: new BuildBody.Tally(Count: face1.Count))
                + BuildReceipt<SolidSlot>.Of(slot: SolidSlot.Trimmed0, body: new BuildBody.Tally(Count: trimmed0.Count))
                + BuildReceipt<SolidSlot>.Of(slot: SolidSlot.Trimmed1, body: new BuildBody.Tally(Count: trimmed1.Count)));

    private static Fin<Built<SolidSlot>> SectionFilleted(
        BrepFace first, Point2d firstUv, BrepFace second, Point2d secondUv,
        SectionFilletLaw law, double tolerance, Op op) =>
        op.Catch(() => {
            System.Collections.Generic.List<Brep> trimmed0 = [];
            System.Collections.Generic.List<Brep> trimmed1 = [];
            System.Collections.Generic.List<Brep> fillets = [];
            bool created = law.Profile.Switch(
                state: (First: first, FirstUv: firstUv, Second: second, SecondUv: secondUv, Law: law, Tolerance: tolerance, Trimmed0: trimmed0, Trimmed1: trimmed1, Fillets: fillets),
                rationalArcs: static ctx => SurfaceFilletBase.CreateRationalArcsFilletSrf(
                    ctx.First, ctx.FirstUv, ctx.Second, ctx.SecondUv, ctx.Law.Radius, ctx.Tolerance,
                    ctx.Trimmed0, ctx.Trimmed1, ctx.Law.RailDegree, ctx.Law.Trim, ctx.Law.Extend, ctx.Fillets),
                nonRationalArcs: static (ctx, profile) => profile.Degree switch {
                    3 => SurfaceFilletBase.CreateNonRationalCubicArcsFilletSrf(
                        ctx.First, ctx.FirstUv, ctx.Second, ctx.SecondUv, ctx.Law.Radius, ctx.Tolerance,
                        ctx.Trimmed0, ctx.Trimmed1, ctx.Law.RailDegree, ctx.Law.Trim, ctx.Law.Extend, ctx.Fillets),
                    4 => SurfaceFilletBase.CreateNonRationalQuarticArcsFilletSrf(
                        ctx.First, ctx.FirstUv, ctx.Second, ctx.SecondUv, ctx.Law.Radius, ctx.Tolerance,
                        ctx.Trimmed0, ctx.Trimmed1, ctx.Law.RailDegree, ctx.Law.Trim, ctx.Law.Extend, ctx.Fillets),
                    5 => SurfaceFilletBase.CreateNonRationalQuinticArcsFilletSrf(
                        ctx.First, ctx.FirstUv, ctx.Second, ctx.SecondUv, ctx.Law.Radius, ctx.Tolerance,
                        ctx.Trimmed0, ctx.Trimmed1, ctx.Law.RailDegree, ctx.Law.Trim, ctx.Law.Extend, ctx.Fillets),
                    _ => false,
                },
                nonRational: static (ctx, profile) => profile.Degree switch {
                    3 => SurfaceFilletBase.CreateNonRationalCubicFilletSrf(
                        ctx.First, ctx.FirstUv, ctx.Second, ctx.SecondUv, ctx.Law.Radius, ctx.Tolerance,
                        ctx.Trimmed0, ctx.Trimmed1, ctx.Law.RailDegree, profile.TangentSlider, ctx.Law.Trim, ctx.Law.Extend, ctx.Fillets),
                    4 => SurfaceFilletBase.CreateNonRationalQuarticFilletSrf(
                        ctx.First, ctx.FirstUv, ctx.Second, ctx.SecondUv, ctx.Law.Radius, ctx.Tolerance,
                        ctx.Trimmed0, ctx.Trimmed1, ctx.Law.RailDegree, profile.TangentSlider, profile.InnerSlider.IfNone(0.0),
                        ctx.Law.Trim, ctx.Law.Extend, ctx.Fillets),
                    5 => SurfaceFilletBase.CreateNonRationalQuinticFilletSrf(
                        ctx.First, ctx.FirstUv, ctx.Second, ctx.SecondUv, ctx.Law.Radius, ctx.Tolerance,
                        ctx.Trimmed0, ctx.Trimmed1, ctx.Law.RailDegree, profile.TangentSlider, profile.InnerSlider.IfNone(0.0),
                        ctx.Law.Trim, ctx.Law.Extend, ctx.Fillets),
                    _ => false,
                },
                g2ChordalQuintic: static ctx => SurfaceFilletBase.CreateG2ChordalQuinticFilletSrf(
                    ctx.First, ctx.FirstUv, ctx.Second, ctx.SecondUv, ctx.Law.Radius, ctx.Tolerance,
                    ctx.Trimmed0, ctx.Trimmed1, ctx.Law.RailDegree, ctx.Law.Trim, ctx.Law.Extend, ctx.Fillets));
            return op.Confirm(success: created).Bind(_ => Harvested(
                fillets: fillets, trimmed0: trimmed0, trimmed1: trimmed1, op: op));
        });

    private static Fin<Built<SolidSlot>> Harvested(
        System.Collections.Generic.IEnumerable<Brep> fillets,
        System.Collections.Generic.IEnumerable<Brep> trimmed0,
        System.Collections.Generic.IEnumerable<Brep> trimmed1,
        Op op) =>
        from builtFillets in ModelGate.OwnMany(built: fillets, key: op)
        from built0 in ModelGate.OwnMany(built: trimmed0, key: op, allowEmpty: true).MapFail(error => {
            _ = builtFillets.Iter(static handle => handle.Dispose());
            return error;
        })
        from built1 in ModelGate.OwnMany(built: trimmed1, key: op, allowEmpty: true).MapFail(error => {
            _ = builtFillets.Iter(static handle => handle.Dispose());
            _ = built0.Iter(static handle => handle.Dispose());
            return error;
        })
        select new Built<SolidSlot>(
            Products: builtFillets + built0 + built1,
            Evidence: BuildReceipt<SolidSlot>.Of(slot: SolidSlot.Filleted, body: new BuildBody.Tally(Count: builtFillets.Count))
                + BuildReceipt<SolidSlot>.Of(slot: SolidSlot.Trimmed0, body: new BuildBody.Tally(Count: built0.Count))
                + BuildReceipt<SolidSlot>.Of(slot: SolidSlot.Trimmed1, body: new BuildBody.Tally(Count: built1.Count)));

    private static Fin<Seq<GeometryHandle>> FilletFace(BrepFace? face, Op op) =>
        Optional(face).Case switch {
            BrepFace live => op.Catch(() => ModelGate.Own(built: live.Duplicate(), key: op).Map(handle => Seq(handle))),
            _ => Fin.Succ(value: Seq<GeometryHandle>()),
        };

    private static Fin<Built<SolidSlot>> Edited(Brep working, SolidEdit verb, Context domain, Op op) =>
        verb.Switch(
            state: (Working: working, Domain: domain, Op: op),
            cap: static ctx => Owned(ctx.Op, ctx.Working, () => ctx.Working.CapPlanarHoles(tolerance: ctx.Domain.Absolute.Value)),
            joinNaked: static ctx => ctx.Op.Catch(() => Fin.Succ(value: ctx.Working.JoinNakedEdges(tolerance: ctx.Domain.Absolute.Value)))
                .Bind(count => Kept(ctx.Op, ctx.Working, extra: BuildReceipt<SolidSlot>.Of(slot: SolidSlot.Edited, body: new BuildBody.Tally(Count: count)))),
            mergeCoplanar: static (ctx, edit) =>
                from _ in edit.Faces.Case switch {
                    (int a, int b) => guard(
                        a >= 0 && a < ctx.Working.Faces.Count && b >= 0 && b < ctx.Working.Faces.Count,
                        ctx.Op.InvalidInput()),
                    _ => Fin.Succ(value: unit),
                }
                from __ in ctx.Op.Confirm(success: edit.Faces.Case switch {
                    (int a, int b) => ctx.Working.MergeCoplanarFaces(faceIndex0: a, faceIndex1: b, tolerance: ctx.Domain.Absolute.Value, angleTolerance: ctx.Domain.Angle.Value),
                    _ => ctx.Working.MergeCoplanarFaces(tolerance: ctx.Domain.Absolute.Value, angleTolerance: ctx.Domain.Angle.Value),
                })
                from built in Kept(ctx.Op, ctx.Working)
                select built,
            unjoinEdges: static (ctx, edit) =>
                from _ in guard(edit.Edges.ForAll(edge => edge >= 0 && edge < ctx.Working.Edges.Count), ctx.Op.InvalidInput())
                from built in ctx.Op.Catch(() =>
                    ModelGate.OwnMany(built: ctx.Working.UnjoinEdges(edgesToUnjoin: edit.Edges.AsIterable()), key: ctx.Op).Map(owned => {
                        ctx.Working.Dispose();
                        return new Built<SolidSlot>(
                            Products: owned,
                            Evidence: BuildReceipt<SolidSlot>.Of(slot: SolidSlot.Edited, body: new BuildBody.Tally(Count: owned.Count)));
                    }))
                select built,
            removeHoles: static (ctx, edit) => Owned(ctx.Op, ctx.Working, () => ctx.Working.RemoveHoles(loops: edit.Loops.AsIterable(), tolerance: ctx.Domain.Absolute.Value)),
            removeFins: static ctx => ctx.Op.Confirm(success: ctx.Working.RemoveFins()).Bind(_ => Kept(ctx.Op, ctx.Working)),
            cullFaces: static ctx => ctx.Op.Confirm(success: ctx.Working.CullUnusedFaces()).Bind(_ => Kept(ctx.Op, ctx.Working)),
            repair: static ctx => ctx.Op.Confirm(success: ctx.Working.Repair(tolerance: ctx.Domain.Absolute.Value)).Bind(_ => Kept(ctx.Op, ctx.Working)),
            reseam: static (ctx, edit) =>
                from _ in guard(edit.Face >= 0 && edit.Face < ctx.Working.Faces.Count && edit.Direction is 0 or 1, ctx.Op.InvalidInput())
                from built in Owned(ctx.Op, ctx.Working, () => Brep.ChangeSeam(
                    face: ctx.Working.Faces[edit.Face], direction: edit.Direction, parameter: edit.Parameter, tolerance: ctx.Domain.Absolute.Value))
                select built);

    private static Fin<Built<SolidSlot>> Kept(Op op, Brep working) =>
        ModelGate.Own(built: working, key: op).Map(owned => new Built<SolidSlot>(
            Products: Seq(owned),
            Evidence: BuildReceipt<SolidSlot>.Of(slot: SolidSlot.Edited, body: new BuildBody.Tally(Count: 1))));

    private static Fin<Built<SolidSlot>> Kept(Op op, Brep working, BuildReceipt<SolidSlot> extra) =>
        ModelGate.Own(built: working, key: op).Map(owned => new Built<SolidSlot>(
            Products: Seq(owned),
            Evidence: BuildReceipt<SolidSlot>.Of(slot: SolidSlot.Edited, body: new BuildBody.Tally(Count: 1)) + extra));

    private static Fin<Built<SolidSlot>> Owned(Op op, Brep working, Func<Brep?> run) =>
        op.Catch(() => ModelGate.Own(built: run(), key: op).Map(owned => {
            working.Dispose();
            return new Built<SolidSlot>(
                Products: Seq(owned),
                Evidence: BuildReceipt<SolidSlot>.Of(slot: SolidSlot.Edited, body: new BuildBody.Tally(Count: 1)));
        }));
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class Solids {
    public static Fin<Built<SolidSlot>> Build(Context context, params ReadOnlySpan<SolidOp> operations) {
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

| [INDEX] | [CONCERN]              | [OWNER]               | [FORM]                                             | [ENTRY]                        |
| :-----: | :--------------------- | :-------------------- | :-------------------------------------------------- | :----------------------------- |
|  [01]   | custody kernel         | `ModelGate`           | typed borrow fold, owned acquisition, batch fold   | `Borrow` / `Own` / `Folded`    |
|  [02]   | product carrier        | `Built<TSlot>`        | owned handles + receipt as one monoid              | every page entry               |
|  [03]   | evidence algebra       | `BuildReceipt<TSlot>` | slot-generic fact stream with projections          | `Of` / `+` / projections       |
|  [04]   | boolean verbs          | `BooleanVerb` / `PlanarBooleanVerb` | host-admitted rows with a second-set demand column | `SolidOp.Boolean` / `Planar` |
|  [05]   | fillet profile         | `FilletShape`         | four settings factories as one union               | `Rig(domain, key)`             |
|  [06]   | section fillet profile | `SectionFilletProfile` | verified profile family generated by degree       | `SolidOp.SectionFillet`        |
|  [07]   | edge radius law        | `EdgeFillet`          | constant or profiled rows, arrays paired           | `SolidOp.FilletEdges`          |
|  [08]   | surface match          | `MatchLaw`            | one value rigging `MatchSrfSettings`               | `SolidOp.Match`                |
|  [09]   | pipe profile           | `PipeLaw`             | constant, variable, thick, thick-variable          | `SolidOp.Pipe`                 |
|  [10]   | primitive seeding      | `SolidSeed`           | analytic values + leased conversions               | `SolidOp.Seed`                 |
|  [11]   | lightweight solid      | `ExtrusionSeed`       | profile and analytic extrusion factories           | `SolidOp.Lite` / `LiteHeavy`   |
|  [12]   | value-semantic edit    | `SolidEdit`           | duplicate-edit-own verbs                           | `SolidOp.Edit`                 |
|  [13]   | solid verbs            | `SolidOp`             | one flat `[Union]`, total generated dispatch       | `Solids.Build`                 |
