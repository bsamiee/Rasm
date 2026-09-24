using LanguageExt.UnsafeValueAccess;
using Rasm.Rhino.Commands;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.DocObjects.Custom;
using Rhino.DocObjects.Tables;
using Rhino.Input.Custom;

namespace Rasm.Rhino.Objects;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record TightBoundingBoxOptions(BoundingBox Box, bool Grow, Transform Xform, bool BaseResult);

public sealed record ObjectCallbacks(
    Action<Error> Reject,
    Option<Func<DrawEventArgs, IO<Unit>>> Draw,
    Option<Func<Option<RhinoViewport>, BoundingBox, IO<BoundingBox>>> Bounds,
    Option<Func<TightBoundingBoxOptions, IO<Option<BoundingBox>>>> TightBounds,
    Option<Func<RhinoObject, IO<Unit>>> Duplicated,
    Option<Func<RhinoDoc, IO<Unit>>> Added,
    Option<Func<RhinoDoc, IO<Unit>>> Deleted,
    Option<Func<Seq<PickCapture>, IO<Seq<int>>>> Pick,
    Option<Func<Seq<PickCapture>, IO<Unit>>> Picked,
    Option<IO<Unit>> SelectionChanged,
    Option<Func<Transform, IO<Unit>>> Transformed,
    Option<Func<SpaceMorph, IO<Unit>>> Morphed,
    Option<Func<RhinoViewport, bool, IO<bool>>> ActiveInViewport,
    Option<Func<MeshType, bool, IO<bool>>> Meshable,
    Option<Func<MeshType, Option<MeshingParameters>, int, IO<int>>> MeshCount,
    Option<Func<MeshType, MeshingParameters, bool, int, IO<int>>> CreateMeshes,
    Option<Func<MeshType, Seq<Mesh>, IO<Seq<Mesh>>>> GetMeshes,
    Option<Func<MeshType, IO<Unit>>> DestroyMeshes);

// --- [SERVICES] ------------------------------------------------------------------------
public abstract class CallbackBrepObject : CustomBrepObject {
    protected CallbackBrepObject() { }

    protected CallbackBrepObject(Brep brep) : base(brep) { }

    protected abstract ObjectCallbacks Callbacks { get; }

    protected sealed override void OnDraw(DrawEventArgs e) {
        base.OnDraw(e);
        _ = Answers.Answer(Callbacks.Draw.Map(draw => draw(e)), Callbacks.Reject, static () => unit);
    }

    protected sealed override BoundingBox GetBoundingBox(RhinoViewport viewport) =>
        CustomObjects.Bounds(Callbacks, viewport, base.GetBoundingBox(viewport));

    protected sealed override bool GetTightBoundingBox(ref BoundingBox tightBox, bool growBox, Transform xform) {
        bool answer = base.GetTightBoundingBox(ref tightBox, growBox, xform);
        (tightBox, bool valid) = CustomObjects.TightBounds(Callbacks, new TightBoundingBoxOptions(tightBox, growBox, xform, answer));
        return valid;
    }

    protected sealed override void OnDuplicate(RhinoObject source) {
        base.OnDuplicate(source);
        _ = Answers.Answer(Callbacks.Duplicated.Map(duplicated => duplicated(source)), Callbacks.Reject, static () => unit);
    }

    protected sealed override void OnAddToDocument(RhinoDoc doc) {
        base.OnAddToDocument(doc);
        _ = Answers.Answer(Callbacks.Added.Map(added => added(doc)), Callbacks.Reject, static () => unit);
    }

    protected sealed override void OnDeleteFromDocument(RhinoDoc doc) {
        base.OnDeleteFromDocument(doc);
        _ = Answers.Answer(Callbacks.Deleted.Map(deleted => deleted(doc)), Callbacks.Reject, static () => unit);
    }

    protected sealed override IEnumerable<ObjRef>? OnPick(PickContext context) =>
        CustomObjects.Picked(Callbacks, base.OnPick(context));

    protected sealed override void OnPicked(PickContext context, IEnumerable<ObjRef> pickedItems) {
        base.OnPicked(context, pickedItems);
        _ = Answers.Answer(Callbacks.Picked.Map(picked => CustomObjects.Captured(pickedItems).Bind(picked)), Callbacks.Reject, static () => unit);
    }

    protected sealed override void OnSelectionChanged() {
        base.OnSelectionChanged();
        _ = Answers.Answer(Callbacks.SelectionChanged, Callbacks.Reject, static () => unit);
    }

    protected sealed override void OnTransform(Transform transform) {
        base.OnTransform(transform);
        _ = Answers.Answer(Callbacks.Transformed.Map(transformed => transformed(transform)), Callbacks.Reject, static () => unit);
    }

    protected sealed override void OnSpaceMorph(SpaceMorph morph) {
        base.OnSpaceMorph(morph);
        _ = Answers.Answer(Callbacks.Morphed.Map(morphed => morphed(morph)), Callbacks.Reject, static () => unit);
    }

    public sealed override bool IsActiveInViewport(RhinoViewport viewport) =>
        CustomObjects.Active(Callbacks, viewport, base.IsActiveInViewport(viewport));

    public sealed override bool IsMeshable(MeshType meshType) =>
        CustomObjects.Meshable(Callbacks, meshType, base.IsMeshable(meshType));

    public sealed override int MeshCount(MeshType meshType, MeshingParameters parameters) =>
        CustomObjects.Counted(Callbacks, meshType, parameters, base.MeshCount(meshType, parameters));

    public sealed override int CreateMeshes(MeshType meshType, MeshingParameters parameters, bool ignoreCustomParameters) =>
        CustomObjects.Created(Callbacks, meshType, parameters, ignoreCustomParameters, base.CreateMeshes(meshType, parameters, ignoreCustomParameters));

    public sealed override Mesh[] GetMeshes(MeshType meshType) =>
        CustomObjects.Meshes(Callbacks, meshType, base.GetMeshes(meshType));

    public sealed override void DestroyMeshes(MeshType meshType) {
        base.DestroyMeshes(meshType);
        _ = Answers.Answer(Callbacks.DestroyMeshes.Map(destroy => destroy(meshType)), Callbacks.Reject, static () => unit);
    }
}

public abstract class CallbackCurveObject : CustomCurveObject {
    protected CallbackCurveObject() { }

    protected CallbackCurveObject(Curve curve) : base(curve) { }

    protected abstract ObjectCallbacks Callbacks { get; }

    protected sealed override void OnDraw(DrawEventArgs e) {
        base.OnDraw(e);
        _ = Answers.Answer(Callbacks.Draw.Map(draw => draw(e)), Callbacks.Reject, static () => unit);
    }

    protected sealed override BoundingBox GetBoundingBox(RhinoViewport viewport) =>
        CustomObjects.Bounds(Callbacks, viewport, base.GetBoundingBox(viewport));

    protected sealed override bool GetTightBoundingBox(ref BoundingBox tightBox, bool growBox, Transform xform) {
        bool answer = base.GetTightBoundingBox(ref tightBox, growBox, xform);
        (tightBox, bool valid) = CustomObjects.TightBounds(Callbacks, new TightBoundingBoxOptions(tightBox, growBox, xform, answer));
        return valid;
    }

    protected sealed override void OnDuplicate(RhinoObject source) {
        base.OnDuplicate(source);
        _ = Answers.Answer(Callbacks.Duplicated.Map(duplicated => duplicated(source)), Callbacks.Reject, static () => unit);
    }

    protected sealed override void OnAddToDocument(RhinoDoc doc) {
        base.OnAddToDocument(doc);
        _ = Answers.Answer(Callbacks.Added.Map(added => added(doc)), Callbacks.Reject, static () => unit);
    }

    protected sealed override void OnDeleteFromDocument(RhinoDoc doc) {
        base.OnDeleteFromDocument(doc);
        _ = Answers.Answer(Callbacks.Deleted.Map(deleted => deleted(doc)), Callbacks.Reject, static () => unit);
    }

    protected sealed override IEnumerable<ObjRef>? OnPick(PickContext context) =>
        CustomObjects.Picked(Callbacks, base.OnPick(context));

    protected sealed override void OnPicked(PickContext context, IEnumerable<ObjRef> pickedItems) {
        base.OnPicked(context, pickedItems);
        _ = Answers.Answer(Callbacks.Picked.Map(picked => CustomObjects.Captured(pickedItems).Bind(picked)), Callbacks.Reject, static () => unit);
    }

    protected sealed override void OnSelectionChanged() {
        base.OnSelectionChanged();
        _ = Answers.Answer(Callbacks.SelectionChanged, Callbacks.Reject, static () => unit);
    }

    protected sealed override void OnTransform(Transform transform) {
        base.OnTransform(transform);
        _ = Answers.Answer(Callbacks.Transformed.Map(transformed => transformed(transform)), Callbacks.Reject, static () => unit);
    }

    protected sealed override void OnSpaceMorph(SpaceMorph morph) {
        base.OnSpaceMorph(morph);
        _ = Answers.Answer(Callbacks.Morphed.Map(morphed => morphed(morph)), Callbacks.Reject, static () => unit);
    }

    public sealed override bool IsActiveInViewport(RhinoViewport viewport) =>
        CustomObjects.Active(Callbacks, viewport, base.IsActiveInViewport(viewport));

    public sealed override bool IsMeshable(MeshType meshType) =>
        CustomObjects.Meshable(Callbacks, meshType, base.IsMeshable(meshType));

    public sealed override int MeshCount(MeshType meshType, MeshingParameters parameters) =>
        CustomObjects.Counted(Callbacks, meshType, parameters, base.MeshCount(meshType, parameters));

    public sealed override int CreateMeshes(MeshType meshType, MeshingParameters parameters, bool ignoreCustomParameters) =>
        CustomObjects.Created(Callbacks, meshType, parameters, ignoreCustomParameters, base.CreateMeshes(meshType, parameters, ignoreCustomParameters));

    public sealed override Mesh[] GetMeshes(MeshType meshType) =>
        CustomObjects.Meshes(Callbacks, meshType, base.GetMeshes(meshType));

    public sealed override void DestroyMeshes(MeshType meshType) {
        base.DestroyMeshes(meshType);
        _ = Answers.Answer(Callbacks.DestroyMeshes.Map(destroy => destroy(meshType)), Callbacks.Reject, static () => unit);
    }
}

public abstract class CallbackMeshObject : CustomMeshObject {
    protected CallbackMeshObject() { }

    protected CallbackMeshObject(Mesh mesh) : base(mesh) { }

    protected abstract ObjectCallbacks Callbacks { get; }

    protected sealed override void OnDraw(DrawEventArgs e) {
        base.OnDraw(e);
        _ = Answers.Answer(Callbacks.Draw.Map(draw => draw(e)), Callbacks.Reject, static () => unit);
    }

    protected sealed override BoundingBox GetBoundingBox(RhinoViewport viewport) =>
        CustomObjects.Bounds(Callbacks, viewport, base.GetBoundingBox(viewport));

    protected sealed override bool GetTightBoundingBox(ref BoundingBox tightBox, bool growBox, Transform xform) {
        bool answer = base.GetTightBoundingBox(ref tightBox, growBox, xform);
        (tightBox, bool valid) = CustomObjects.TightBounds(Callbacks, new TightBoundingBoxOptions(tightBox, growBox, xform, answer));
        return valid;
    }

    protected sealed override void OnDuplicate(RhinoObject source) {
        base.OnDuplicate(source);
        _ = Answers.Answer(Callbacks.Duplicated.Map(duplicated => duplicated(source)), Callbacks.Reject, static () => unit);
    }

    protected sealed override void OnAddToDocument(RhinoDoc doc) {
        base.OnAddToDocument(doc);
        _ = Answers.Answer(Callbacks.Added.Map(added => added(doc)), Callbacks.Reject, static () => unit);
    }

    protected sealed override void OnDeleteFromDocument(RhinoDoc doc) {
        base.OnDeleteFromDocument(doc);
        _ = Answers.Answer(Callbacks.Deleted.Map(deleted => deleted(doc)), Callbacks.Reject, static () => unit);
    }

    protected sealed override IEnumerable<ObjRef>? OnPick(PickContext context) =>
        CustomObjects.Picked(Callbacks, base.OnPick(context));

    protected sealed override void OnPicked(PickContext context, IEnumerable<ObjRef> pickedItems) {
        base.OnPicked(context, pickedItems);
        _ = Answers.Answer(Callbacks.Picked.Map(picked => CustomObjects.Captured(pickedItems).Bind(picked)), Callbacks.Reject, static () => unit);
    }

    protected sealed override void OnSelectionChanged() {
        base.OnSelectionChanged();
        _ = Answers.Answer(Callbacks.SelectionChanged, Callbacks.Reject, static () => unit);
    }

    protected sealed override void OnTransform(Transform transform) {
        base.OnTransform(transform);
        _ = Answers.Answer(Callbacks.Transformed.Map(transformed => transformed(transform)), Callbacks.Reject, static () => unit);
    }

    protected sealed override void OnSpaceMorph(SpaceMorph morph) {
        base.OnSpaceMorph(morph);
        _ = Answers.Answer(Callbacks.Morphed.Map(morphed => morphed(morph)), Callbacks.Reject, static () => unit);
    }

    public sealed override bool IsActiveInViewport(RhinoViewport viewport) =>
        CustomObjects.Active(Callbacks, viewport, base.IsActiveInViewport(viewport));

    public sealed override bool IsMeshable(MeshType meshType) =>
        CustomObjects.Meshable(Callbacks, meshType, base.IsMeshable(meshType));

    public sealed override int MeshCount(MeshType meshType, MeshingParameters parameters) =>
        CustomObjects.Counted(Callbacks, meshType, parameters, base.MeshCount(meshType, parameters));

    public sealed override int CreateMeshes(MeshType meshType, MeshingParameters parameters, bool ignoreCustomParameters) =>
        CustomObjects.Created(Callbacks, meshType, parameters, ignoreCustomParameters, base.CreateMeshes(meshType, parameters, ignoreCustomParameters));

    public sealed override Mesh[] GetMeshes(MeshType meshType) =>
        CustomObjects.Meshes(Callbacks, meshType, base.GetMeshes(meshType));

    public sealed override void DestroyMeshes(MeshType meshType) {
        base.DestroyMeshes(meshType);
        _ = Answers.Answer(Callbacks.DestroyMeshes.Map(destroy => destroy(meshType)), Callbacks.Reject, static () => unit);
    }
}

public abstract class CallbackPointObject : CustomPointObject {
    protected CallbackPointObject() { }

    protected CallbackPointObject(Point point) : base(point) { }

    protected abstract ObjectCallbacks Callbacks { get; }

    protected sealed override void OnDraw(DrawEventArgs e) {
        base.OnDraw(e);
        _ = Answers.Answer(Callbacks.Draw.Map(draw => draw(e)), Callbacks.Reject, static () => unit);
    }

    protected sealed override BoundingBox GetBoundingBox(RhinoViewport viewport) =>
        CustomObjects.Bounds(Callbacks, viewport, base.GetBoundingBox(viewport));

    protected sealed override bool GetTightBoundingBox(ref BoundingBox tightBox, bool growBox, Transform xform) {
        bool answer = base.GetTightBoundingBox(ref tightBox, growBox, xform);
        (tightBox, bool valid) = CustomObjects.TightBounds(Callbacks, new TightBoundingBoxOptions(tightBox, growBox, xform, answer));
        return valid;
    }

    protected sealed override void OnDuplicate(RhinoObject source) {
        base.OnDuplicate(source);
        _ = Answers.Answer(Callbacks.Duplicated.Map(duplicated => duplicated(source)), Callbacks.Reject, static () => unit);
    }

    protected sealed override void OnAddToDocument(RhinoDoc doc) {
        base.OnAddToDocument(doc);
        _ = Answers.Answer(Callbacks.Added.Map(added => added(doc)), Callbacks.Reject, static () => unit);
    }

    protected sealed override void OnDeleteFromDocument(RhinoDoc doc) {
        base.OnDeleteFromDocument(doc);
        _ = Answers.Answer(Callbacks.Deleted.Map(deleted => deleted(doc)), Callbacks.Reject, static () => unit);
    }

    protected sealed override IEnumerable<ObjRef>? OnPick(PickContext context) =>
        CustomObjects.Picked(Callbacks, base.OnPick(context));

    protected sealed override void OnPicked(PickContext context, IEnumerable<ObjRef> pickedItems) {
        base.OnPicked(context, pickedItems);
        _ = Answers.Answer(Callbacks.Picked.Map(picked => CustomObjects.Captured(pickedItems).Bind(picked)), Callbacks.Reject, static () => unit);
    }

    protected sealed override void OnSelectionChanged() {
        base.OnSelectionChanged();
        _ = Answers.Answer(Callbacks.SelectionChanged, Callbacks.Reject, static () => unit);
    }

    protected sealed override void OnTransform(Transform transform) {
        base.OnTransform(transform);
        _ = Answers.Answer(Callbacks.Transformed.Map(transformed => transformed(transform)), Callbacks.Reject, static () => unit);
    }

    protected sealed override void OnSpaceMorph(SpaceMorph morph) {
        base.OnSpaceMorph(morph);
        _ = Answers.Answer(Callbacks.Morphed.Map(morphed => morphed(morph)), Callbacks.Reject, static () => unit);
    }

    public sealed override bool IsActiveInViewport(RhinoViewport viewport) =>
        CustomObjects.Active(Callbacks, viewport, base.IsActiveInViewport(viewport));

    public sealed override bool IsMeshable(MeshType meshType) =>
        CustomObjects.Meshable(Callbacks, meshType, base.IsMeshable(meshType));

    public sealed override int MeshCount(MeshType meshType, MeshingParameters parameters) =>
        CustomObjects.Counted(Callbacks, meshType, parameters, base.MeshCount(meshType, parameters));

    public sealed override int CreateMeshes(MeshType meshType, MeshingParameters parameters, bool ignoreCustomParameters) =>
        CustomObjects.Created(Callbacks, meshType, parameters, ignoreCustomParameters, base.CreateMeshes(meshType, parameters, ignoreCustomParameters));

    public sealed override Mesh[] GetMeshes(MeshType meshType) =>
        CustomObjects.Meshes(Callbacks, meshType, base.GetMeshes(meshType));

    public sealed override void DestroyMeshes(MeshType meshType) {
        base.DestroyMeshes(meshType);
        _ = Answers.Answer(Callbacks.DestroyMeshes.Map(destroy => destroy(meshType)), Callbacks.Reject, static () => unit);
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class CustomObjects {
    // --- [OVERRIDES]
    internal static BoundingBox Bounds(ObjectCallbacks callbacks, RhinoViewport? viewport, BoundingBox box) =>
        Answers.Answer(callbacks.Bounds.Map(bounds => bounds(Optional(viewport), box)), callbacks.Reject, () => box);

    internal static (BoundingBox Box, bool Valid) TightBounds(ObjectCallbacks callbacks, TightBoundingBoxOptions options) =>
        Answers.Answer(
            callbacks.TightBounds.Map(tight => tight(options).Map(answer => answer.Match(
                Some: static box => (box, box.IsValid),
                None: () => (options.Box, options.BaseResult)))),
            callbacks.Reject,
            () => (options.Box, options.BaseResult));

    internal static IEnumerable<ObjRef>? Picked(ObjectCallbacks callbacks, IEnumerable<ObjRef>? candidates) =>
        Answers.Answer(callbacks.Pick.Map(pick => Filtered(Answers.Present(candidates), pick).Map(static kept => (IEnumerable<ObjRef>?)kept)), callbacks.Reject, () => candidates);

    internal static IO<Seq<PickCapture>> Captured(IEnumerable<ObjRef> references) =>
        toSeq(references).TraverseM(Selections.Capture).As();

    internal static bool Active(ObjectCallbacks callbacks, RhinoViewport viewport, bool answer) =>
        Answers.Answer(callbacks.ActiveInViewport.Map(active => active(viewport, answer)), callbacks.Reject, () => answer);

    internal static bool Meshable(ObjectCallbacks callbacks, MeshType type, bool answer) =>
        Answers.Answer(callbacks.Meshable.Map(meshable => meshable(type, answer)), callbacks.Reject, () => answer);

    internal static int Counted(ObjectCallbacks callbacks, MeshType type, MeshingParameters? parameters, int answer) =>
        Answers.Answer(callbacks.MeshCount.Map(count => count(type, Optional(parameters), answer)), callbacks.Reject, () => answer);

    internal static int Created(ObjectCallbacks callbacks, MeshType type, MeshingParameters parameters, bool ignoreCustom, int answer) =>
        Answers.Answer(callbacks.CreateMeshes.Map(create => create(type, parameters, ignoreCustom, answer)), callbacks.Reject, () => answer);

    internal static Mesh[] Meshes(ObjectCallbacks callbacks, MeshType type, Mesh[] answer) =>
        Answers.Answer(callbacks.GetMeshes.Map(meshes => meshes(type, toSeq(answer)).Map(static rows => rows.ToArray())), callbacks.Reject, () => answer);

    private static IO<Seq<ObjRef>> Filtered(Seq<ObjRef> candidates, Func<Seq<PickCapture>, IO<Seq<int>>> pick) =>
        from captures in Captured(candidates)
        from indices in pick(captures)
        from kept in IO.lift(() => indices.TraverseM(index => candidates.At(index).ToFin(new IndexOutOfRange(nameof(ObjectCallbacks.Pick), index, candidates.Count))).As())
        select kept;

    // --- [WRITES]
    public static IO<Guid> AddCustom(RhinoDoc doc, RhinoObject custom, Option<HistoryRecord> history) =>
        from added in Added(doc, custom, history.ValueUnsafe())
        from owned in IO.lift(() => Refused.Unless(custom.Document is not null, nameof(ObjectTable.AddRhinoObject)))
        select custom.Id;

    private static IO<Unit> Added(RhinoDoc doc, RhinoObject custom, HistoryRecord? history) =>
        custom switch {
            CustomBrepObject brep => IO.lift(() => doc.Objects.AddRhinoObject(brep, history)),
            CustomCurveObject curve => IO.lift(() => doc.Objects.AddRhinoObject(curve, history)),
            CustomMeshObject mesh => IO.lift(() => doc.Objects.AddRhinoObject(mesh, history)),
            CustomPointObject point => IO.lift(() => doc.Objects.AddRhinoObject(point, history)),
            _ => IO.fail<Unit>(new Invalid(nameof(ObjectTable.AddRhinoObject))),
        };
}
