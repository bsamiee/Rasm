using System.Collections.Generic;
using Rasm.Rhino.Persistence;
using Rhino;
using Rhino.Collections;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.DocObjects.Custom;
using Rhino.Input.Custom;

namespace Rasm.Rhino.Objects;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ObjectDefinition<TState, TGeometry>(DictionaryCodec<TState> Codec, Func<TState, Fin<TGeometry>> Build, Func<TState, Transform, TState> Transformed)
    where TState : notnull where TGeometry : GeometryBase {
    public Option<Func<TState, string>> Description { get; init; }

    public Option<Action<TState, DrawEventArgs>> Draw { get; init; }

    public Option<Func<TState, BoundingBox>> Extent { get; init; }

    public Option<Func<TState, ObjRef, bool>> Pickable { get; init; }

    public Option<Func<TState, SpaceMorph, TState>> Morphed { get; init; }

    public Option<(Func<TState, Seq<GripPoint>> Points, Func<TState, Seq<GripPoint>, TState> Moved)> Grips { get; init; }
}

// --- [SERVICES] ------------------------------------------------------------------------
internal sealed class ObjectBody<TState, TGeometry>(ObjectDefinition<TState, TGeometry> definition) where TState : notnull where TGeometry : GeometryBase {
    // --- [STATE]
    private readonly Atom<Option<TState>> decoded = Atom(Option<TState>.None);

    internal Fin<TState> State(RhinoObject owner) =>
        decoded.Value.Match(
            Some: static state => state,
            None: () =>
                from stored in Conversions.Present(owner.Geometry.UserDictionary.TryGetDictionary(definition.Codec.Name, out ArchivableDictionary held), held)
                    .ToFin(new RhinoFault(definition.Codec.Name, RhinoReason.Missing))
                from record in definition.Codec.FromDictionary(stored)
                from state in record.Present.ToFin(definition.Codec.Refusal(stored))
                select decoded.Swap(_ => Some(state)).IfNone(state));

    private Fin<TGeometry> Built(TState state) =>
        from geometry in definition.Build(state)
        from stamped in Stamped(geometry, state)
        select geometry;

    private Fin<Unit> Stamped(GeometryBase geometry, TState state) =>
        definition.Codec.ToDictionary(state).Bind(encoded => RhinoFault.Unless(geometry.UserDictionary.Set(definition.Codec.Name, encoded), RhinoReason.Unwritable, definition.Codec.Name));

    // --- [DOCUMENT]
    internal IO<Guid> Added(RhinoDoc doc, TState state, Option<ObjectAttributes> attributes, Func<TGeometry, IO<RhinoObject>> add) =>
        from built in IO.lift(() => Built(state))
        from added in use(() => built).Bind(add).Map(static fresh => fresh.Id).Bracket()
        from id in IO.lift(() => Conversions.Required(Conversions.Present(added)))
        from written in attributes.Traverse(held => Products.Accepted(() => doc.Objects.ModifyAttributes(id, held, quiet: true))).As()
        select id;

    internal IO<Unit> Edited(RhinoObject owner, RhinoDoc doc, Func<TState, TState> change) =>
        from built in IO.lift(() => State(owner).Map(change).Bind(Built))
        from replaced in use(() => built).Bind(geometry => Products.Accepted(() => doc.Objects.Replace(owner.Id, geometry, ignoreModes: false))).Bracket()
        select unit;

    // --- [CALLBACKS]
    internal void Draw(RhinoObject owner, DrawEventArgs e, Action<DrawEventArgs> host) {
        host(e);
        _ = definition.Draw.Iter(draw => State(owner).Iter(state => draw(state, e)));
    }

    internal IEnumerable<ObjRef>? Picked(RhinoObject owner, IEnumerable<ObjRef>? candidates) =>
        (from keep in definition.Pickable
         from state in State(owner).ToOption()
         select (Keep: keep, State: state)).Match(Some: held => candidates?.Where(reference => held.Keep(held.State, reference)), None: () => candidates);

    internal void Transformed(RhinoObject owner, Transform transform) => Restamped(owner, state => definition.Transformed(state, transform));

    internal void Morphed(RhinoObject owner, SpaceMorph morph) => _ = definition.Morphed.Iter(morphed => Restamped(owner, state => morphed(state, morph)));

    private void Restamped(RhinoObject owner, Func<TState, TState> change) =>
        _ = (from state in State(owner)
             let moved = change(state)
             from stamped in Stamped(owner.Geometry, moved)
             select ignore(decoded.Swap(_ => Some(moved)))).IfFail(static error => Session.Failed(error));

    internal string Description(RhinoObject owner, string fallback) =>
        (from describe in definition.Description
         from state in State(owner).ToOption()
         select describe(state)).IfNone(fallback);

    internal BoundingBox Bounds(RhinoObject owner, BoundingBox host, Transform xform) =>
        (from extent in definition.Extent
         from state in State(owner).ToOption()
         from box in Some(extent(state)).Filter(static box => box.IsValid)
         select BoundingBox.Union(host, new BoundingBox(box.GetCorners().Select(corner => xform * corner)))).IfNone(host);

    internal bool TightBounds(RhinoObject owner, ref BoundingBox tight, bool grow, Transform xform, Func<BoundingBox, (bool Found, BoundingBox Box)> host) {
        (bool found, BoundingBox box) = host(tight);
        BoundingBox united = Bounds(owner, found ? box : grow ? tight : BoundingBox.Empty, xform);
        tight = united;
        return united.IsValid;
    }

    internal Option<GripDefinition> Grips(RhinoObject owner) =>
        from grips in definition.Grips
        from state in State(owner).ToOption()
        select new GripDefinition(new GripControl.Free(DragLine: true), grips.Points(state), moved => Built(grips.Moved(state, moved)).Map(static GeometryBase (geometry) => geometry));
}

public abstract class DefinedCurveObject<TSelf, TState> : CustomCurveObject
    where TSelf : DefinedCurveObject<TSelf, TState>, IDefined<ObjectDefinition<TState, Curve>>, new() where TState : notnull {
    // --- [STATE]
    private readonly ObjectBody<TState, Curve> body = new(TSelf.Definition);

    public Fin<TState> State => body.State(this);

    public Option<GripDefinition> GripDefinition => body.Grips(this);

    // --- [DOCUMENT]
    public static IO<Guid> Add(RhinoDoc doc, TState state, Option<ObjectAttributes> attributes, Option<HistoryRecord> history) =>
        new ObjectBody<TState, Curve>(TSelf.Definition).Added(doc, state, attributes, curve => Fresh(curve).Map(fresh => {
            doc.Objects.AddRhinoObject(fresh, history.ValueUnsafe());
            return (RhinoObject)fresh;
        }));

    public IO<Unit> Edit(RhinoDoc doc, Func<TState, TState> change) => body.Edited(this, doc, change);

    private static IO<TSelf> Fresh(Curve curve) =>
        use(static () => new TSelf()).Map(fresh => {
            _ = fresh.SetCurve(curve);
            return fresh;
        });

    // --- [CALLBACKS]
    protected sealed override void OnDraw(DrawEventArgs e) => body.Draw(this, e, base.OnDraw);

    protected sealed override IEnumerable<ObjRef>? OnPick(PickContext context) => body.Picked(this, base.OnPick(context));

    protected sealed override void OnTransform(Transform transform) => body.Transformed(this, transform);

    protected sealed override BoundingBox GetBoundingBox(RhinoViewport viewport) => body.Bounds(this, base.GetBoundingBox(viewport), Transform.Identity);

    protected sealed override bool GetTightBoundingBox(ref BoundingBox tightBox, bool growBox, Transform xform) =>
        body.TightBounds(this, ref tightBox, growBox, xform, box => (base.GetTightBoundingBox(ref box, growBox, xform), box));

    public sealed override string ShortDescription(bool plural) => body.Description(this, base.ShortDescription(plural));
}

public abstract class DefinedMeshObject<TSelf, TState> : CustomMeshObject
    where TSelf : DefinedMeshObject<TSelf, TState>, IDefined<ObjectDefinition<TState, Mesh>>, new() where TState : notnull {
    // --- [STATE]
    private readonly ObjectBody<TState, Mesh> body = new(TSelf.Definition);

    public Fin<TState> State => body.State(this);

    public Option<GripDefinition> GripDefinition => body.Grips(this);

    // --- [DOCUMENT]
    public static IO<Guid> Add(RhinoDoc doc, TState state, Option<ObjectAttributes> attributes, Option<HistoryRecord> history) =>
        new ObjectBody<TState, Mesh>(TSelf.Definition).Added(doc, state, attributes, mesh => Fresh(mesh).Map(fresh => {
            doc.Objects.AddRhinoObject(fresh, history.ValueUnsafe());
            return (RhinoObject)fresh;
        }));

    public IO<Unit> Edit(RhinoDoc doc, Func<TState, TState> change) => body.Edited(this, doc, change);

    private static IO<TSelf> Fresh(Mesh mesh) =>
        use(static () => new TSelf()).Map(fresh => {
            _ = fresh.SetMesh(mesh);
            return fresh;
        });

    // --- [CALLBACKS]
    protected sealed override void OnDraw(DrawEventArgs e) => body.Draw(this, e, base.OnDraw);

    protected sealed override IEnumerable<ObjRef>? OnPick(PickContext context) => body.Picked(this, base.OnPick(context));

    protected sealed override void OnTransform(Transform transform) => body.Transformed(this, transform);

    protected sealed override void OnSpaceMorph(SpaceMorph morph) => body.Morphed(this, morph);

    protected sealed override BoundingBox GetBoundingBox(RhinoViewport viewport) => body.Bounds(this, base.GetBoundingBox(viewport), Transform.Identity);

    protected sealed override bool GetTightBoundingBox(ref BoundingBox tightBox, bool growBox, Transform xform) =>
        body.TightBounds(this, ref tightBox, growBox, xform, box => (base.GetTightBoundingBox(ref box, growBox, xform), box));

    public sealed override string ShortDescription(bool plural) => body.Description(this, base.ShortDescription(plural));
}

public abstract class DefinedBrepObject<TSelf, TState> : CustomBrepObject
    where TSelf : DefinedBrepObject<TSelf, TState>, IDefined<ObjectDefinition<TState, Brep>>, new() where TState : notnull {
    // --- [STATE]
    private readonly ObjectBody<TState, Brep> body = new(TSelf.Definition);

    public Fin<TState> State => body.State(this);

    public Option<GripDefinition> GripDefinition => body.Grips(this);

    // --- [DOCUMENT]
    public static IO<Guid> Add(RhinoDoc doc, TState state, Option<ObjectAttributes> attributes) =>
        new ObjectBody<TState, Brep>(TSelf.Definition).Added(doc, state, attributes, brep => use(static () => new TSelf()).Map(fresh => {
            doc.Objects.AddRhinoObject(fresh, brep);
            return (RhinoObject)fresh;
        }));

    public IO<Unit> Edit(RhinoDoc doc, Func<TState, TState> change) => body.Edited(this, doc, change);

    // --- [CALLBACKS]
    protected sealed override void OnDraw(DrawEventArgs e) => body.Draw(this, e, base.OnDraw);

    protected sealed override IEnumerable<ObjRef>? OnPick(PickContext context) => body.Picked(this, base.OnPick(context));

    protected sealed override void OnTransform(Transform transform) => body.Transformed(this, transform);

    protected sealed override BoundingBox GetBoundingBox(RhinoViewport viewport) => body.Bounds(this, base.GetBoundingBox(viewport), Transform.Identity);

    protected sealed override bool GetTightBoundingBox(ref BoundingBox tightBox, bool growBox, Transform xform) =>
        body.TightBounds(this, ref tightBox, growBox, xform, box => (base.GetTightBoundingBox(ref box, growBox, xform), box));

    public sealed override string ShortDescription(bool plural) => body.Description(this, base.ShortDescription(plural));
}

public abstract class DefinedPointObject<TSelf, TState> : CustomPointObject
    where TSelf : DefinedPointObject<TSelf, TState>, IDefined<ObjectDefinition<TState, Point>>, new() where TState : notnull {
    // --- [STATE]
    private readonly ObjectBody<TState, Point> body = new(TSelf.Definition);

    public Fin<TState> State => body.State(this);

    public Option<GripDefinition> GripDefinition => body.Grips(this);

    // --- [DOCUMENT]
    public static IO<Guid> Add(RhinoDoc doc, TState state, Option<ObjectAttributes> attributes) =>
        new ObjectBody<TState, Point>(TSelf.Definition).Added(doc, state, attributes, point => use(static () => new TSelf()).Map(fresh => {
            doc.Objects.AddRhinoObject(fresh, point);
            return (RhinoObject)fresh;
        }));

    public IO<Unit> Edit(RhinoDoc doc, Func<TState, TState> change) => body.Edited(this, doc, change);

    // --- [CALLBACKS]
    protected sealed override void OnDraw(DrawEventArgs e) => body.Draw(this, e, base.OnDraw);

    protected sealed override IEnumerable<ObjRef>? OnPick(PickContext context) => body.Picked(this, base.OnPick(context));

    protected sealed override void OnTransform(Transform transform) => body.Transformed(this, transform);

    protected sealed override BoundingBox GetBoundingBox(RhinoViewport viewport) => body.Bounds(this, base.GetBoundingBox(viewport), Transform.Identity);

    protected sealed override bool GetTightBoundingBox(ref BoundingBox tightBox, bool growBox, Transform xform) =>
        body.TightBounds(this, ref tightBox, growBox, xform, box => (base.GetTightBoundingBox(ref box, growBox, xform), box));

    public sealed override string ShortDescription(bool plural) => body.Description(this, base.ShortDescription(plural));
}
