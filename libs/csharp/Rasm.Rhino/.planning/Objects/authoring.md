# [RASM_RHINO_OBJECTS_AUTHORING]

Custom-object and grip authoring belongs to `Rasm.Rhino.Objects`. Host subclassing stays inside the custom geometry and grip adapters, every verified virtual forwards to an immutable program, and live grip editing resolves value facts inside a document grant. `Display/interaction.md` exclusively owns in-viewport widgets, registration, hit testing, and widget event streams.

## [01]-[INDEX]

- [02]-[OBJECT_PROGRAM]: `ObjectProgram` and the forwarding kernel every adapter shares.
- [03]-[ADAPTERS]: the `ClassId`-ready host derivations.
- [04]-[GRIP_PROGRAM]: `GripSeed`, `GripProgram`, `RasmGrip`, `RasmGrips`, and the enabler rig.
- [05]-[GRIP_EDIT]: `GripMove`, `GripEdit`, `GripFacts`, and the `Grips` entry pair.
- [06]-[SURFACE_LEDGER]: the page's owner table.

## [02]-[OBJECT_PROGRAM]

- Owner: `ObjectProgram` carries every verified draw, duplicate, transform, morph, document, pick, selection, viewport, bounding-box, and tight-bounds hook. `HostForward` centralizes exception lifting, fallback behavior, and pick capture.
- Law: the hook roster is closed by decompile — `OnDraw`, `GetBoundingBox`, `GetTightBoundingBox`, `OnDuplicate`, `OnDeleteFromDocument`, `OnAddToDocument`, `OnPick`, `OnPicked`, `OnSelectionChanged`, `OnTransform`, `OnSpaceMorph` is the complete protected-virtual set, plus the public-virtual `IsActiveInViewport`; a program field with no host virtual behind it is a phantom the page refuses.
- Law: a hook fault logs and never escapes — every `Fin` refusal writes through `RhinoApp.WriteLine` and the callback returns, because a throw inside a host virtual is swallowed by the host's exception report and vanishes silently; the log is the one observable channel.
- Law: picked objects cross as captures — `OnPicked` projects every reference through the selection page's `Picks.Capture` before the program sees it, so no `ObjRef` survives into program state; the `Pick` hook sifts candidate references inside the callback window and returns a subset, never new references.
- Law: base runs first — every forwarding override invokes the host base before its hook, so standard drawing, transform application, and pick behavior survive an inert program, and a program augments rather than re-implements; suppression of base behavior is a genuinely new adapter, never a program flag.
- Growth: a new host virtual is one program field plus one forwarding line per adapter.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Collections.Generic;
using System.Linq;
using Rasm.Domain;
using Rasm.Rhino.Commands;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.DocObjects.Custom;
using Rhino.Geometry;

namespace Rasm.Rhino.Objects;

// --- [MODELS] -----------------------------------------------------------------------------
public readonly record struct TightExtent(BoundingBox Current, bool Grow, Transform Motion, bool BaseAnswered);

public sealed record ObjectProgram(
    Option<Func<DrawEventArgs, Fin<Unit>>> Draw = default,
    Option<Func<RhinoObject, Fin<Unit>>> Duplicated = default,
    Option<Func<Transform, Fin<Unit>>> Moved = default,
    Option<Func<SpaceMorph, Fin<Unit>>> Morphed = default,
    Option<Func<RhinoDoc, Fin<Unit>>> Entered = default,
    Option<Func<RhinoDoc, Fin<Unit>>> Left = default,
    Option<Func<PickContext, Seq<ObjRef>, Fin<Seq<ObjRef>>>> Pick = default,
    Option<Func<PickContext, Seq<PickCapture>, Fin<Unit>>> Picked = default,
    Option<Func<Fin<Unit>>> SelectionChanged = default,
    Option<Func<RhinoViewport, bool, Fin<bool>>> Viewable = default,
    Option<Func<RhinoViewport, BoundingBox, Fin<BoundingBox>>> Bounds = default,
    Option<Func<TightExtent, Fin<BoundingBox>>> TightBounds = default) {
    public static readonly ObjectProgram Inert = new();
}

// --- [OPERATIONS] -------------------------------------------------------------------------
internal static class HostForward {
    internal static Unit Logged(this Fin<Unit> outcome) =>
        outcome.IfFail(error => {
            RhinoApp.WriteLine(message: error.Message);
            return unit;
        });

    internal static Unit Run<TArgs>(this Option<Func<TArgs, Fin<Unit>>> hook, TArgs args) =>
        hook.Map(run => Op.Of().Catch(() => run(args)).Logged()).IfNone(noneValue: unit);

    internal static Seq<ObjRef> Sift(this ObjectProgram program, PickContext context, Seq<ObjRef> candidates) =>
        program.Pick.Match(
            Some: filter => Op.Of().Catch(() => filter(context, candidates)
                .Bind(chosen => guard(chosen.ForAll(item => candidates.Exists(candidate =>
                        ReferenceEquals(candidate, item))), Op.Of().InvalidResult()).ToFin()
                    .Map(_ => chosen))).Match(
                Succ: static chosen => chosen,
                Fail: error => {
                    RhinoApp.WriteLine(message: error.Message);
                    return candidates;
                }),
            None: () => candidates);

    internal static bool Active(this ObjectProgram program, RhinoViewport viewport, bool inherited) =>
        program.Viewable.Match(
            Some: judge => Op.Of().Catch(() => judge(viewport, inherited)).Match(
                Succ: static verdict => verdict,
                Fail: error => {
                    RhinoApp.WriteLine(message: error.Message);
                    return inherited;
                }),
            None: () => inherited);

    internal static BoundingBox Box(this ObjectProgram program, RhinoViewport viewport, BoundingBox inherited) =>
        program.Bounds.Match(
            Some: grow => Op.Of().Catch(() => grow(viewport, inherited)
                .Bind(answer => guard(answer.IsValid, Op.Of().InvalidResult()).ToFin().Map(_ => answer))).Match(
                Succ: static bounds => bounds,
                Fail: error => {
                    RhinoApp.WriteLine(message: error.Message);
                    return inherited;
                }),
            None: () => inherited);

    internal static (bool Answered, BoundingBox Bounds) Tight(
        this ObjectProgram program,
        BoundingBox bounds,
        bool grow,
        Transform motion,
        bool inherited) =>
        program.TightBounds.Match(
            Some: refine => Op.Of().Catch(() => refine(new TightExtent(
                    Current: bounds,
                    Grow: grow,
                    Motion: motion,
                    BaseAnswered: inherited))
                .Bind(answer => guard(answer.IsValid, Op.Of().InvalidResult()).ToFin().Map(_ => answer)))
                .Match(
                    Succ: answer => (answer.IsValid, answer),
                    Fail: error => {
                        RhinoApp.WriteLine(message: error.Message);
                        return (inherited, bounds);
                    }),
            None: () => (inherited, bounds));

    internal static Unit Captured(this ObjectProgram program, PickContext context, IEnumerable<ObjRef> picked) =>
        program.Picked.Map(consume => {
            Op op = Op.Of(name: nameof(ObjectProgram));
            return op.Catch(() => toSeq(picked)
                    .TraverseM(reference => Picks.Capture(reference: reference, key: op)).As()
                    .Bind(captures => consume(context, captures)))
                .Logged();
        }).IfNone(noneValue: unit);
}
```

## [03]-[ADAPTERS]

- Owner: abstract host derivations map the catalogued brep, curve, mesh, and point custom bases onto one program contract; no custom SubD, extrusion, hatch, or annotation base exists.
- Law: a concrete package class supplies `[ClassId("<guid>")]`, `Program`, and its geometry-seeded constructor pass-through. Extra state or overrides bypass the duplication contract.
- Law: `RasmCurveObject` alone carries `SetCurve` — the host gives only the curve kind a restage member, surfaced as a protected pass-through; the other kinds replace geometry through the table rail like any object.
- Law: adapters register nothing themselves — placement is `TableOp.Add` with the constructed instance as source, read-back is the state page's window, and the `ClassId` guid is what rehydrates the subclass when a document reopens.

```csharp signature
// --- [SERVICES] ---------------------------------------------------------------------------
public abstract class RasmBrepObject : CustomBrepObject {
    protected RasmBrepObject() { }
    protected RasmBrepObject(Brep brep) : base(brep) { }

    protected abstract ObjectProgram Program { get; }

    protected sealed override void OnDraw(DrawEventArgs e) { base.OnDraw(e); _ = Program.Draw.Run(args: e); }
    protected sealed override void OnDuplicate(RhinoObject source) { base.OnDuplicate(source); _ = Program.Duplicated.Run(args: source); }
    protected sealed override void OnTransform(Transform transform) { base.OnTransform(transform); _ = Program.Moved.Run(args: transform); }
    protected sealed override void OnSpaceMorph(SpaceMorph morph) { base.OnSpaceMorph(morph); _ = Program.Morphed.Run(args: morph); }
    protected sealed override void OnAddToDocument(RhinoDoc doc) { base.OnAddToDocument(doc); _ = Program.Entered.Run(args: doc); }
    protected sealed override void OnDeleteFromDocument(RhinoDoc doc) { base.OnDeleteFromDocument(doc); _ = Program.Left.Run(args: doc); }
    protected sealed override IEnumerable<ObjRef> OnPick(PickContext context) => Program.Sift(context: context, candidates: toSeq(base.OnPick(context)));
    protected sealed override void OnPicked(PickContext context, IEnumerable<ObjRef> pickedItems) { base.OnPicked(context, pickedItems); _ = Program.Captured(context: context, picked: pickedItems); }
    protected sealed override void OnSelectionChanged() { base.OnSelectionChanged(); _ = Program.SelectionChanged.Map(static run => Op.Of().Catch(run).Logged()).IfNone(noneValue: unit); }
    public sealed override bool IsActiveInViewport(RhinoViewport viewport) => Program.Active(viewport: viewport, inherited: base.IsActiveInViewport(viewport));
    protected sealed override BoundingBox GetBoundingBox(RhinoViewport viewport) => Program.Box(viewport: viewport, inherited: base.GetBoundingBox(viewport));
    protected sealed override bool GetTightBoundingBox(ref BoundingBox tightBox, bool growBox, Transform xform) {
        bool inherited = base.GetTightBoundingBox(ref tightBox, growBox, xform);
        (bool answered, BoundingBox bounds) = Program.Tight(
            bounds: tightBox,
            grow: growBox,
            motion: xform,
            inherited: inherited);
        tightBox = bounds;
        return answered;
    }
}

public abstract class RasmCurveObject : CustomCurveObject {
    protected RasmCurveObject() { }
    protected RasmCurveObject(Curve curve) : base(curve) { }

    protected abstract ObjectProgram Program { get; }

    protected Curve Restage(Curve curve) => SetCurve(curve: curve);

    protected sealed override void OnDraw(DrawEventArgs e) { base.OnDraw(e); _ = Program.Draw.Run(args: e); }
    protected sealed override void OnDuplicate(RhinoObject source) { base.OnDuplicate(source); _ = Program.Duplicated.Run(args: source); }
    protected sealed override void OnTransform(Transform transform) { base.OnTransform(transform); _ = Program.Moved.Run(args: transform); }
    protected sealed override void OnSpaceMorph(SpaceMorph morph) { base.OnSpaceMorph(morph); _ = Program.Morphed.Run(args: morph); }
    protected sealed override void OnAddToDocument(RhinoDoc doc) { base.OnAddToDocument(doc); _ = Program.Entered.Run(args: doc); }
    protected sealed override void OnDeleteFromDocument(RhinoDoc doc) { base.OnDeleteFromDocument(doc); _ = Program.Left.Run(args: doc); }
    protected sealed override IEnumerable<ObjRef> OnPick(PickContext context) => Program.Sift(context: context, candidates: toSeq(base.OnPick(context)));
    protected sealed override void OnPicked(PickContext context, IEnumerable<ObjRef> pickedItems) { base.OnPicked(context, pickedItems); _ = Program.Captured(context: context, picked: pickedItems); }
    protected sealed override void OnSelectionChanged() { base.OnSelectionChanged(); _ = Program.SelectionChanged.Map(static run => Op.Of().Catch(run).Logged()).IfNone(noneValue: unit); }
    public sealed override bool IsActiveInViewport(RhinoViewport viewport) => Program.Active(viewport: viewport, inherited: base.IsActiveInViewport(viewport));
    protected sealed override BoundingBox GetBoundingBox(RhinoViewport viewport) => Program.Box(viewport: viewport, inherited: base.GetBoundingBox(viewport));
    protected sealed override bool GetTightBoundingBox(ref BoundingBox tightBox, bool growBox, Transform xform) {
        bool inherited = base.GetTightBoundingBox(ref tightBox, growBox, xform);
        (bool answered, BoundingBox bounds) = Program.Tight(
            bounds: tightBox,
            grow: growBox,
            motion: xform,
            inherited: inherited);
        tightBox = bounds;
        return answered;
    }
}

public abstract class RasmMeshObject : CustomMeshObject {
    protected RasmMeshObject() { }
    protected RasmMeshObject(Mesh mesh) : base(mesh) { }

    protected abstract ObjectProgram Program { get; }

    protected sealed override void OnDraw(DrawEventArgs e) { base.OnDraw(e); _ = Program.Draw.Run(args: e); }
    protected sealed override void OnDuplicate(RhinoObject source) { base.OnDuplicate(source); _ = Program.Duplicated.Run(args: source); }
    protected sealed override void OnTransform(Transform transform) { base.OnTransform(transform); _ = Program.Moved.Run(args: transform); }
    protected sealed override void OnSpaceMorph(SpaceMorph morph) { base.OnSpaceMorph(morph); _ = Program.Morphed.Run(args: morph); }
    protected sealed override void OnAddToDocument(RhinoDoc doc) { base.OnAddToDocument(doc); _ = Program.Entered.Run(args: doc); }
    protected sealed override void OnDeleteFromDocument(RhinoDoc doc) { base.OnDeleteFromDocument(doc); _ = Program.Left.Run(args: doc); }
    protected sealed override IEnumerable<ObjRef> OnPick(PickContext context) => Program.Sift(context: context, candidates: toSeq(base.OnPick(context)));
    protected sealed override void OnPicked(PickContext context, IEnumerable<ObjRef> pickedItems) { base.OnPicked(context, pickedItems); _ = Program.Captured(context: context, picked: pickedItems); }
    protected sealed override void OnSelectionChanged() { base.OnSelectionChanged(); _ = Program.SelectionChanged.Map(static run => Op.Of().Catch(run).Logged()).IfNone(noneValue: unit); }
    public sealed override bool IsActiveInViewport(RhinoViewport viewport) => Program.Active(viewport: viewport, inherited: base.IsActiveInViewport(viewport));
    protected sealed override BoundingBox GetBoundingBox(RhinoViewport viewport) => Program.Box(viewport: viewport, inherited: base.GetBoundingBox(viewport));
    protected sealed override bool GetTightBoundingBox(ref BoundingBox tightBox, bool growBox, Transform xform) {
        bool inherited = base.GetTightBoundingBox(ref tightBox, growBox, xform);
        (bool answered, BoundingBox bounds) = Program.Tight(
            bounds: tightBox,
            grow: growBox,
            motion: xform,
            inherited: inherited);
        tightBox = bounds;
        return answered;
    }
}

public abstract class RasmPointObject : CustomPointObject {
    protected RasmPointObject() { }
    protected RasmPointObject(Point point) : base(point) { }

    protected abstract ObjectProgram Program { get; }

    protected sealed override void OnDraw(DrawEventArgs e) { base.OnDraw(e); _ = Program.Draw.Run(args: e); }
    protected sealed override void OnDuplicate(RhinoObject source) { base.OnDuplicate(source); _ = Program.Duplicated.Run(args: source); }
    protected sealed override void OnTransform(Transform transform) { base.OnTransform(transform); _ = Program.Moved.Run(args: transform); }
    protected sealed override void OnSpaceMorph(SpaceMorph morph) { base.OnSpaceMorph(morph); _ = Program.Morphed.Run(args: morph); }
    protected sealed override void OnAddToDocument(RhinoDoc doc) { base.OnAddToDocument(doc); _ = Program.Entered.Run(args: doc); }
    protected sealed override void OnDeleteFromDocument(RhinoDoc doc) { base.OnDeleteFromDocument(doc); _ = Program.Left.Run(args: doc); }
    protected sealed override IEnumerable<ObjRef> OnPick(PickContext context) => Program.Sift(context: context, candidates: toSeq(base.OnPick(context)));
    protected sealed override void OnPicked(PickContext context, IEnumerable<ObjRef> pickedItems) { base.OnPicked(context, pickedItems); _ = Program.Captured(context: context, picked: pickedItems); }
    protected sealed override void OnSelectionChanged() { base.OnSelectionChanged(); _ = Program.SelectionChanged.Map(static run => Op.Of().Catch(run).Logged()).IfNone(noneValue: unit); }
    public sealed override bool IsActiveInViewport(RhinoViewport viewport) => Program.Active(viewport: viewport, inherited: base.IsActiveInViewport(viewport));
    protected sealed override BoundingBox GetBoundingBox(RhinoViewport viewport) => Program.Box(viewport: viewport, inherited: base.GetBoundingBox(viewport));
    protected sealed override bool GetTightBoundingBox(ref BoundingBox tightBox, bool growBox, Transform xform) {
        bool inherited = base.GetTightBoundingBox(ref tightBox, growBox, xform);
        (bool answered, BoundingBox bounds) = Program.Tight(
            bounds: tightBox,
            grow: growBox,
            motion: xform,
            inherited: inherited);
        tightBox = bounds;
        return answered;
    }
}
```

## [04]-[GRIP_PROGRAM]

- Owner: `GripSeed` carries admitted index, origin, and weight. `GripProgram` owns seed and regrow functions plus every verified location, reset, mesh-update, topology, draw, and disposal hook. `RasmGrip` repairs the host weight sentinel and forwards location changes; `RasmGrips` forwards the set program; `GripRig` registers the enabler.
- Law: `NewGeometry` fires once at the end of a drag — the shim collects every grip's index and current location, hands them to `Regrow`, and a refusal logs and answers null so the host keeps existing geometry; per-frame rebuild gating stays on the host's own `NewLocation`/`GripsMoved` flags, and the global `Dragging()` probe is a static host fact, never an instance flag.
- Law: `Weight` must be carried by the shim — the custom grip base deliberately stubs the member with a sentinel getter (`-1.234...E+308`) and a no-op setter, so `RasmGrip` overrides both accessors over a real field seeded from `GripSeed.Weight`; an authored grip trusting the base member reads garbage.
- Law: the enabler keys on the grips type's `[Guid]` — `RegisterGripsEnabler` resolves `typeof(TGrips).GUID`, not `ClassIdAttribute`, re-registration replaces the prior enabler, and the enabler installs through `EnableCustomGrips` only when the mint answers `Some`; a non-`Some` candidate keeps standard host grips. Registration demands the declared `GuidAttribute` because the runtime synthesizes a fallback for an unattributed type, so `Type.GUID` is never empty and only the attribute probe proves a stable key.
- Law: the grip draw hook runs before base — the base `OnDraw` draws the grips themselves, so a program draws dynamic elements first and the shim calls base after. Reset and mesh-update hooks augment the completed base operation; disposal notifies before base releases the carrier.

```csharp signature
// --- [MODELS] -----------------------------------------------------------------------------
public sealed record GripSeed(int Index, Point3d Origin, double Weight = 1.0) {
    internal Fin<GripSeed> Admit(Op key) =>
        from origin in key.AcceptInput(value: Origin)
        from _ in guard(Index >= 0 && double.IsFinite(Weight) && Weight >= 0.0, key.InvalidInput()).ToFin()
        select this with { Origin = origin };
}

public readonly record struct GripMotion(bool NewLocation, bool Moved, bool Dragging);

public sealed class GripProgram {
    private GripProgram(
        Func<GeometryBase, Fin<Seq<GripSeed>>> seeds,
        Func<Seq<(int Index, Point3d Location)>, Fin<GeometryBase>> regrow,
        Option<Func<int, Point3d, Fin<Unit>>> locationChanged,
        Option<Func<GripsDrawEventArgs, Fin<Unit>>> draw,
        Option<Func<Fin<Unit>>> reset,
        Option<Func<Fin<Unit>>> resetMeshes,
        Option<Func<MeshType, GripMotion, Fin<Unit>>> updateMesh,
        Option<Func<int, int, int, int, bool, Fin<Option<GripObject>>>> neighbor,
        Option<Func<int, int, Fin<Option<GripObject>>>> surfaceGrip,
        Option<Func<Fin<Option<NurbsSurface>>>> surface,
        Option<Func<bool, Fin<Unit>>> disposing) {
        Seeds = seeds;
        Regrow = regrow;
        LocationChanged = locationChanged;
        Draw = draw;
        Reset = reset;
        ResetMeshes = resetMeshes;
        UpdateMesh = updateMesh;
        Neighbor = neighbor;
        SurfaceGrip = surfaceGrip;
        Surface = surface;
        Disposing = disposing;
    }

    internal Func<GeometryBase, Fin<Seq<GripSeed>>> Seeds { get; }
    internal Func<Seq<(int Index, Point3d Location)>, Fin<GeometryBase>> Regrow { get; }
    internal Option<Func<int, Point3d, Fin<Unit>>> LocationChanged { get; }
    internal Option<Func<GripsDrawEventArgs, Fin<Unit>>> Draw { get; }
    internal Option<Func<Fin<Unit>>> Reset { get; }
    internal Option<Func<Fin<Unit>>> ResetMeshes { get; }
    internal Option<Func<MeshType, GripMotion, Fin<Unit>>> UpdateMesh { get; }
    internal Option<Func<int, int, int, int, bool, Fin<Option<GripObject>>>> Neighbor { get; }
    internal Option<Func<int, int, Fin<Option<GripObject>>>> SurfaceGrip { get; }
    internal Option<Func<Fin<Option<NurbsSurface>>>> Surface { get; }
    internal Option<Func<bool, Fin<Unit>>> Disposing { get; }

    public static Fin<GripProgram> Of(
        Func<GeometryBase, Fin<Seq<GripSeed>>> seeds,
        Func<Seq<(int Index, Point3d Location)>, Fin<GeometryBase>> regrow,
        Option<Func<int, Point3d, Fin<Unit>>> locationChanged = default,
        Option<Func<GripsDrawEventArgs, Fin<Unit>>> draw = default,
        Option<Func<Fin<Unit>>> reset = default,
        Option<Func<Fin<Unit>>> resetMeshes = default,
        Option<Func<MeshType, GripMotion, Fin<Unit>>> updateMesh = default,
        Option<Func<int, int, int, int, bool, Fin<Option<GripObject>>>> neighbor = default,
        Option<Func<int, int, Fin<Option<GripObject>>>> surfaceGrip = default,
        Option<Func<Fin<Option<NurbsSurface>>>> surface = default,
        Option<Func<bool, Fin<Unit>>> disposing = default) {
        Op op = Op.Of(name: nameof(GripProgram));
        return from seed in Optional(seeds).ToFin(Fail: op.InvalidInput())
               from grow in Optional(regrow).ToFin(Fail: op.InvalidInput())
               select new GripProgram(seed, grow, locationChanged, draw, reset, resetMeshes, updateMesh, neighbor, surfaceGrip, surface, disposing);
    }
}

// --- [SERVICES] ---------------------------------------------------------------------------
public sealed class RasmGrip : CustomGripObject {
    private double weight;
    private readonly Option<Func<int, Point3d, Fin<Unit>>> locationChanged;

    internal RasmGrip(GripSeed seed, Option<Func<int, Point3d, Fin<Unit>>> locationChanged) {
        Index = seed.Index;
        OriginalLocation = seed.Origin;
        weight = seed.Weight;
        this.locationChanged = locationChanged;
    }

    // Base CustomGripObject.Weight is a sentinel getter and a no-op setter; the shim owns the real value.
    public override double Weight {
        get => weight;
        set => weight = value;
    }

    public sealed override void NewLocation() {
        base.NewLocation();
        _ = locationChanged.Map(run => Op.Of().Catch(() => run(Index, CurrentLocation)).Logged()).IfNone(noneValue: unit);
    }
}

public abstract class RasmGrips : CustomObjectGrips {
    protected abstract GripProgram Program { get; }

    protected Fin<Unit> Sow(GeometryBase geometry) {
        Op op = Op.Of(name: nameof(RasmGrips));
        return from source in Optional(geometry).ToFin(Fail: op.InvalidInput())
               from seeds in op.Catch(() => Program.Seeds(source))
               from admitted in seeds.TraverseM(seed => Optional(seed).ToFin(Fail: op.InvalidInput())
                   .Bind(value => value.Admit(key: op))).As()
               from _ in guard(admitted.Map(static seed => seed.Index).Distinct().Count == admitted.Count, op.InvalidInput()).ToFin()
               from __ in admitted.TraverseM(seed => op.Catch(() => {
                   AddGrip(grip: new RasmGrip(seed: seed, locationChanged: Program.LocationChanged));
                   return Fin.Succ(value: unit);
               })).As()
               select unit;
    }

    protected sealed override GeometryBase NewGeometry() =>
        Op.Of(name: nameof(RasmGrips)).Catch(() => Program.Regrow(toSeq(Enumerable.Range(start: 0, count: GripCount))
                    .Map(index => Grip(index: index))
                    .Map(static grip => (grip.Index, grip.CurrentLocation)))
                .Bind(grown => Optional(grown).ToFin(Fail: Op.Of(name: nameof(RasmGrips)).InvalidResult())))
            .Match(
                Succ: static grown => grown,
                Fail: error => {
                    RhinoApp.WriteLine(message: error.Message);
                    return null!;
                });

    protected sealed override void OnDraw(GripsDrawEventArgs args) {
        _ = Program.Draw.Run(args: args);
        base.OnDraw(args);
    }

    protected sealed override void OnReset() {
        base.OnReset();
        _ = Program.Reset.Map(run => Op.Of().Catch(run).Logged()).IfNone(noneValue: unit);
    }

    protected sealed override void OnResetMeshes() {
        base.OnResetMeshes();
        _ = Program.ResetMeshes.Map(run => Op.Of().Catch(run).Logged()).IfNone(noneValue: unit);
    }

    protected sealed override void OnUpdateMesh(MeshType meshType) {
        base.OnUpdateMesh(meshType);
        _ = Program.UpdateMesh.Map(run => Op.Of().Catch(() => run(
                meshType,
                new GripMotion(NewLocation: NewLocation, Moved: GripsMoved, Dragging: Dragging())))
            .Map(_ => ignore(NewLocation = false))
            .Logged()).IfNone(noneValue: unit);
    }

    protected sealed override GripObject NeighborGrip(int gripIndex, int dr, int ds, int dt, bool wrap) =>
        Program.Neighbor.Match(
            Some: find => Op.Of().Catch(() => find(gripIndex, dr, ds, dt, wrap)).Match(
                Succ: found => found.IfNone(() => base.NeighborGrip(gripIndex, dr, ds, dt, wrap)),
                Fail: error => {
                    RhinoApp.WriteLine(message: error.Message);
                    return base.NeighborGrip(gripIndex, dr, ds, dt, wrap);
                }),
            None: () => base.NeighborGrip(gripIndex, dr, ds, dt, wrap));

    protected sealed override GripObject NurbsSurfaceGrip(int i, int j) =>
        Program.SurfaceGrip.Match(
            Some: find => Op.Of().Catch(() => find(i, j)).Match(
                Succ: found => found.IfNone(() => base.NurbsSurfaceGrip(i, j)),
                Fail: error => {
                    RhinoApp.WriteLine(message: error.Message);
                    return base.NurbsSurfaceGrip(i, j);
                }),
            None: () => base.NurbsSurfaceGrip(i, j));

    protected sealed override NurbsSurface NurbsSurface() =>
        Program.Surface.Match(
            Some: find => Op.Of().Catch(find).Match(
                Succ: found => found.IfNone(base.NurbsSurface),
                Fail: error => {
                    RhinoApp.WriteLine(message: error.Message);
                    return base.NurbsSurface();
                }),
            None: base.NurbsSurface);

    protected sealed override void Dispose(bool disposing) {
        _ = Program.Disposing.Run(args: disposing);
        base.Dispose(disposing);
    }
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class GripRig {
    public static Fin<Unit> Register<TGrips>(Func<RhinoObject, Option<TGrips>> mint) where TGrips : CustomObjectGrips {
        Op op = Op.Of(name: nameof(GripRig));
        return from factory in Optional(mint).ToFin(Fail: op.InvalidInput())
               from __ in guard(
                   typeof(TGrips).IsDefined(typeof(System.Runtime.InteropServices.GuidAttribute), inherit: false),
                   op.InvalidInput()).ToFin()
               from _ in op.Catch(() => {
                   CustomObjectGrips.RegisterGripsEnabler(
                       enabler: candidate => {
                           Fin<Unit> enabled = op.Catch(() => factory(candidate).Match(
                               Some: grips => op.Confirm(success: candidate.EnableCustomGrips(customGrips: grips))
                                   .MapFail(error => {
                                       grips.Dispose();
                                       return error;
                                   }),
                               None: () => Fin.Succ(value: unit)));
                           _ = enabled.Logged();
                       },
                       customGripsType: typeof(TGrips));
                   return Fin.Succ(value: unit);
               })
               select unit;
    }
}
```

## [05]-[GRIP_EDIT]

- Owner: `GripMove` `[Union]` — the relocation verbs: absolute point, delta vector, transform, and single-step undo; `GripEdit` `[Union]` — the two grip mutations: `Rig` toggles `GripsOn`, `Move` relocates one indexed grip or every grip through a `GripMove` verb; `GripFacts` — the whole grip read in one pass: identity, positions, movement state, weight, local frame, and the surface, curve, and cage parameter coordinates with their control-vertex indices, each projected as absence where the grip kind carries none; `GripReceipt`/`GripCensus` — the detached results; `Grips` — the two entries: `Census` the read, `Touch` the immediate mutation.
- Law: grips resolve from their owner — `GripEdit.Rig` toggles `GripsOn`, `Census` and `GripEdit.Move` read `GetGrips` inside the grant, and a grip index addresses into that roster; no `GripObject` leases outward, because grip lifetime ends when the owner's grips turn off.
- Law: parameter reads are capability probes — `GetSurfaceParameters`, `GetCurveParameters`, `GetCageParameters`, and the CV-index members answer `false` or empty on grips of another kind, and the facts project absence rather than faulting, so one census serves every grip kind.
- Law: movement is immediate visual state under the host's drag machinery — `Move` and `UndoMove` mutate the grip, `Touch` opens no undo record, and the geometry consequence lands when the host drives the owner's grip pipeline; a program wanting transactional geometry replacement routes the regrown value through `TableOp.Replace`.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record GripMove {
    private GripMove() { }
    public sealed record To(Point3d Location) : GripMove;
    public sealed record By(Vector3d Delta) : GripMove;
    public sealed record Via(Transform Motion) : GripMove;
    public sealed record Back : GripMove;

    internal Fin<GripMove> Admit(Op op) =>
        Switch(
            context: op,
            to: static (key, move) => key.AcceptInput(value: move.Location).Map(_ => (GripMove)move),
            by: static (key, move) => key.AcceptInput(value: move.Delta).Map(_ => (GripMove)move),
            via: static (key, move) => key.AcceptInput(value: move.Motion).Map(_ => (GripMove)move),
            back: static (_, move) => Fin.Succ<GripMove>(move));

    internal Fin<Unit> Apply(GripObject grip, Op op) =>
        Switch(
            context: (Grip: grip, Op: op),
            to: static (context, move) => context.Op.Catch(() => { context.Grip.Move(newLocation: move.Location); return Fin.Succ(value: unit); }),
            by: static (context, move) => context.Op.Catch(() => { context.Grip.Move(delta: move.Delta); return Fin.Succ(value: unit); }),
            via: static (context, move) => context.Op.Catch(() => { context.Grip.Move(xform: move.Motion); return Fin.Succ(value: unit); }),
            back: static (context, _) => context.Op.Catch(() => { context.Grip.UndoMove(); return Fin.Succ(value: unit); }));
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record GripEdit {
    private GripEdit() { }
    public sealed record Rig(bool On) : GripEdit;
    public sealed record Move(Option<int> Index, GripMove Motion) : GripEdit;

    internal Fin<GripEdit> Admit(Op op) =>
        Switch(
            context: op,
            rig: static (_, edit) => Fin.Succ<GripEdit>(edit),
            move: static (key, edit) =>
                from motion in Optional(edit.Motion).ToFin(Fail: key.InvalidInput()).Bind(value => value.Admit(op: key))
                from _ in guard(edit.Index.Map(static value => value >= 0).IfNone(noneValue: true), key.InvalidInput()).ToFin()
                select (GripEdit)new Move(Index: edit.Index, Motion: motion));
}

// --- [MODELS] -----------------------------------------------------------------------------
public readonly record struct GripReceipt(Seq<Guid> Ids) : IDetachedDocumentResult;

public sealed record GripCensus(Seq<(Guid Owner, Seq<GripFacts> Rows)> Rows) : IDetachedDocumentResult;

public sealed record GripFacts(
    int Index,
    Guid OwnerId,
    Point3d Current,
    Point3d Origin,
    bool Moved,
    double Weight,
    Option<(Vector3d U, Vector3d V, Vector3d Normal)> Directions,
    Option<Point2d> SurfaceUv,
    Option<double> CurveT,
    Option<Point3d> CageUvw,
    Seq<int> CurveCvs,
    Seq<(int I, int J)> SurfaceCvs) : IDetachedDocumentResult {
    internal static Fin<GripFacts> Of(GripObject grip, Op key) =>
        key.Catch(() => {
            bool framed = grip.GetGripDirections(u: out Vector3d u, v: out Vector3d v, normal: out Vector3d normal);
            bool onSurface = grip.GetSurfaceParameters(u: out double su, v: out double sv);
            bool onCurve = grip.GetCurveParameters(t: out double t);
            bool inCage = grip.GetCageParameters(u: out double cu, v: out double cv, w: out double cw);
            int curveCount = grip.GetCurveCVIndices(cvIndices: out int[] curveCvs);
            int surfaceCount = grip.GetSurfaceCVIndices(cvIndices: out Tuple<int, int>[] surfaceCvs);
            return Fin.Succ(value: new GripFacts(
                Index: grip.Index,
                OwnerId: grip.OwnerId,
                Current: grip.CurrentLocation,
                Origin: grip.OriginalLocation,
                Moved: grip.Moved,
                Weight: grip.Weight,
                Directions: framed ? Some((u, v, normal)) : Option<(Vector3d, Vector3d, Vector3d)>.None,
                SurfaceUv: onSurface ? Some(new Point2d(x: su, y: sv)) : Option<Point2d>.None,
                CurveT: onCurve ? Some(t) : Option<double>.None,
                CageUvw: inCage ? Some(new Point3d(x: cu, y: cv, z: cw)) : Option<Point3d>.None,
                CurveCvs: curveCount > 0 ? toSeq(curveCvs) : Seq<int>(),
                SurfaceCvs: surfaceCount > 0
                    ? toSeq(surfaceCvs).Map(static pair => (pair.Item1, pair.Item2))
                    : Seq<(int, int)>()));
        });
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class Grips {
    public static Fin<GripCensus> Census(DocumentSession session, TableTarget target) {
        Op op = Op.Of();
        return session.Demand(
            use: document =>
                from natives in Objects.Resolve(document: document, target: target, key: op)
                from rows in natives.TraverseM(native => op.Catch(() =>
                    Optional(native.GetGrips()).Map(static held => toSeq(held)).IfNone(Seq<GripObject>())
                        .TraverseM(grip => GripFacts.Of(grip: grip, key: op)).As()
                        .Map(facts => (native.Id, facts)))).As()
                select new GripCensus(Rows: rows),
            key: op,
            needs: [SessionNeed.Read]);
    }

    public static Fin<GripReceipt> Touch(DocumentSession session, TableTarget target, GripEdit edit) {
        Op op = Op.Of();
        return from active in Optional(edit).ToFin(Fail: op.InvalidInput()).Bind(value => value.Admit(op: op))
               from receipt in session.Demand(
                   use: document =>
                       from natives in Objects.Resolve(document: document, target: target, key: op)
                       from ids in natives.TraverseM(native => active.Switch(
                           context: (Native: native, Op: op),
                           rig: static (ctx, edit) => ctx.Op.Catch(() => {
                               ctx.Native.GripsOn = edit.On;
                               return Fin.Succ(value: ctx.Native.Id);
                           }),
                           move: static (ctx, edit) =>
                               from roster in ctx.Op.Catch(() => Fin.Succ(value: Optional(ctx.Native.GetGrips())
                                   .Map(static held => toSeq(held)).IfNone(Seq<GripObject>())))
                               from chosen in edit.Index.Case switch {
                                   int at => roster.Filter(grip => grip.Index == at) switch {
                                       [var only] => Fin.Succ(value: Seq(only)),
                                       _ => Fin.Fail<Seq<GripObject>>(error: ctx.Op.MissingContext()),
                                   },
                                   _ => Fin.Succ(value: roster),
                               }
                               from _ in guard(!chosen.IsEmpty, ctx.Op.MissingContext()).ToFin()
                               from __ in chosen.TraverseM(grip => edit.Motion.Apply(grip: grip, op: ctx.Op)).As()
                               select ctx.Native.Id)).As()
                       select new GripReceipt(Ids: ids),
                   key: op,
                   needs: [SessionNeed.Mutate])
               select receipt;
    }
}
```

## [06]-[SURFACE_LEDGER]

| [INDEX] | [CONCERN]        | [OWNER]                | [FORM]                                                 | [ENTRY]                         |
| :-----: | :--------------- | :--------------------- | :----------------------------------------------------- | :------------------------------ |
|  [01]   | override program | `ObjectProgram`        | optional `Fin` hooks over the complete verified roster | adapter `Program` slots         |
|  [02]   | host derivations | `Rasm*Object`          | sealed forwarding over one shared kernel, base-first   | `[ClassId]` concrete subclasses |
|  [03]   | grip authoring   | `GripProgram`          | admitted seed/regrow core plus verified optional hooks | `RasmGrips` overrides           |
|  [04]   | grip shims       | `RasmGrip`/`RasmGrips` | sentinel-weight repair and roster forwarding           | `GripRig.Register<TGrips>`      |
|  [05]   | grip value edits | `GripEdit`             | rig and move over `GripMove` verbs, detached receipts  | `Grips.Touch` / `Census`        |
