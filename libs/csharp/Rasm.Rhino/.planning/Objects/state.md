# [RASM_RHINO_OBJECTS_STATE]

Live document-object state belongs to `Rasm.Rhino.Objects`. One `StateAsk` family answers the catalogued state reads assigned to this owner — the whole-state snapshot, object and gumball frames, in-flight drag transforms, subobject rosters, batch tight extents, detached member pieces, planar sections, thickness slices, and clipping fills. One `Touch` family owns component selection and every highlight mutation absent from the table rail. Addressing composes `TableTarget`, every answer leaves the session grant detached, and `StateAnswer` owns every copied `ObjectPiece` until disposal. `ObjectReceipt<TFact>` is the one fact-stream receipt monoid and `ObjectSpine.Commit` the one commit entry every undo-recorded Objects mutation rail walks over the shared `DocumentCommit.Sealed` envelope — lights, materials, and history commits share this spine, so undo, redraw, and grant semantics cannot drift between rails. Immediate visual `Touch` mutations demand the session directly and open no commit envelope. `RhinoObject.CommitChanges` has no path: attribute mutation rides `TableOp.Amend`, mode mutation rides `TableOp.State`, and geometry mutation rides `TableOp.Replace`.

## [01]-[INDEX]

- [02]-[SNAPSHOT]: raw selection evidence, `HighlightState`, and the one-pass `ObjectSnapshot` read product.
- [03]-[FRAMES]: `FrameAsk`/`FramePose` — object frame, gumball frame, and drag-transform reads.
- [04]-[REACH_AND_TOUCH]: `Reach`, `Touch`, and the immediate component selection and highlight rail.
- [05]-[CUTS_AND_PIECES]: `SectionCut`, `ObjectPiece`, and the detached extraction custody.
- [06]-[ASK_ENTRY]: `StateAsk`/`StateAnswer`, `ObjectReceipt<TFact>`, `ObjectSpine`, and the `Objects` entry pair.
- [07]-[SURFACE_LEDGER]: the page's owner table.

## [02]-[SNAPSHOT]

- Owner: `SelectionGrade` owns the verified unselected, selected, and persistent `IsSelected(checkSubObjects: true)` grades; `HighlightState` preserves the undocumented highlight integer beside the highlighted-component roster; `ObjectSnapshot` closes identity, lifecycle, source-model, description, closed-status, frame, grip, memory, and history-link evidence in one detached value.
- Law: the snapshot reads once per object inside the session grant — every field lands in one pass over the resolved handle, so a consumer never re-enters the document to complete a partial read, and the product is detached the moment `Ask` returns.
- Law: selection and highlight never share a vocabulary. `SelectionGrade` maps the verified `0`/`1`/`2` contract, while highlight preserves its raw host integer and assigns no meaning to it.
- Law: `CommitChanges` never appears — the host member answers `true` only when a staged working copy actually flushed, and this package stages nothing on the live object: attribute writes travel `TableOp.Amend`, mode and visibility travel `TableOp.State`, geometry travels `TableOp.Replace`; the snapshot is the read face of that one-write-path law.
- Law: history linkage is one snapshot bool — `HistoryBound` carries `HasHistoryRecord()` as presence evidence, and every linkage read or mutation lives on the history page's `Chronicle`.
- Growth: a new host object fact is one snapshot field read in the same pass; a named native grade enters only after its values verify.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Threading;
using Rasm.Domain;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.UI.Gumball;

namespace Rasm.Rhino.Objects;

// --- [MODELS] -----------------------------------------------------------------------------
public readonly record struct HighlightState(int Native, Seq<ComponentIndex> Components);

[SmartEnum<int>]
public sealed partial class SelectionGrade {
    public static readonly SelectionGrade None = new(key: 0);
    public static readonly SelectionGrade Selected = new(key: 1);
    public static readonly SelectionGrade Persistent = new(key: 2);

    internal static Fin<SelectionGrade> Of(int native, Op key) =>
        TryGet(native, out SelectionGrade? grade) && grade is not null
            ? Fin.Succ(grade)
            : Fin.Fail<SelectionGrade>(key.InvalidResult(detail: native.ToString()));
}

public sealed record ObjectSnapshot(
    Guid Id,
    uint Serial,
    Option<string> Name,
    ObjectType Kind,
    ActiveSpace Space,
    bool Normal,
    bool Locked,
    bool Hidden,
    bool Deleted,
    bool Reference,
    bool Visible,
    bool Solid,
    bool Deletable,
    bool PictureFrame,
    bool DefinitionGeometry,
    uint Worksession,
    uint ReferenceModel,
    uint DefinitionModel,
    SelectionGrade Selection,
    HighlightState Highlight,
    bool Selectable,
    bool GripsOn,
    bool GripsSelected,
    bool DynamicTransform,
    bool HistoryBound,
    bool HistoryCopiesOnReplace,
    uint MemoryBytes,
    string Description,
    string ClosedDescription,
    int ClosedStatus) : IDetachedDocumentResult {
    internal static Fin<ObjectSnapshot> Of(RhinoObject native, Op key) =>
        from grade in key.Catch(() => SelectionGrade.Of(native.IsSelected(checkSubObjects: true), key))
        from closed in key.Catch(() => Fin.Succ(value: (
            Text: native.ShortDescriptionWithClosedStatus(prepend: false, plural: false, status: out int status),
            Status: status)))
        from snapshot in key.Catch(() => Fin.Succ(value: new ObjectSnapshot(
            Id: native.Id,
            Serial: native.RuntimeSerialNumber,
            Name: Optional(native.Name).Filter(static text => text.Length > 0),
            Kind: native.ObjectType,
            Space: native.Attributes.Space,
            Normal: native.IsNormal,
            Locked: native.IsLocked,
            Hidden: native.IsHidden,
            Deleted: native.IsDeleted,
            Reference: native.IsReference,
            Visible: native.Visible,
            Solid: native.IsSolid,
            Deletable: native.IsDeletable,
            PictureFrame: native.IsPictureFrame,
            DefinitionGeometry: native.IsInstanceDefinitionGeometry,
            Worksession: native.WorksessionReferenceSerialNumber,
            ReferenceModel: native.ReferenceModelSerialNumber,
            DefinitionModel: native.InstanceDefinitionModelSerialNumber,
            Selection: grade,
            Highlight: new HighlightState(
                Native: native.IsHighlighted(checkSubObjects: false),
                Components: toSeq(native.GetHighlightedSubObjects())),
            Selectable: native.IsSelectable(),
            GripsOn: native.GripsOn,
            GripsSelected: native.GripsSelected,
            DynamicTransform: native.HasDynamicTransform,
            HistoryBound: native.HasHistoryRecord(),
            HistoryCopiesOnReplace: native.CopyHistoryOnReplace(),
            MemoryBytes: native.MemoryEstimate(),
            Description: native.ShortDescription(plural: false),
            ClosedDescription: closed.Text,
            ClosedStatus: closed.Status)))
        select snapshot;
}
```

## [03]-[FRAMES]

- Owner: `FrameAsk` `[Union]` closes anchor, gumball, and drag questions; `GumballAlignment` owns each standard/current probe as row behavior; `FramePose` `[Union]` owns one typed pose per question.
- Law: frame reads are object-side only — `RhinoObject.ObjectFrame` reads and the obsolete `RhinoObject.SetObjectFrame` overloads are dead, so every frame write is the attributes page's `Anchor` edit committed through the table rail's `Amend`, and this page never mutates a frame.
- Law: an unset frame is absence — the anchor read always forces `RhinoObject.ObjectFrameFlags.ReturnUnset`, so an object carrying no explicit frame yields an invalid plane the fold projects to `None`; the request exposes only the `ObjectSignal Scale` axis toggling `IncludeScaleTransforms`, never the raw host `[Flags]`. A failed gumball probe projects to `None`, a drag probe answers `None` outside an active drag, and no consumer branches on `Plane.Unset`.
- Law: the gumball pose crosses detached — `GumballFrame` is a host struct whose `Plane`, `ScaleGripDistance`, and `ScaleMode` copy into the pose value, and `GumballScaleMode` rides the pose as a seam discriminant.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum]
public sealed partial class GumballAlignment {
    public static readonly GumballAlignment Standard = new(read: static native =>
        native.TryGetGumballFrame(frame: out GumballFrame frame) ? Some(frame) : Option<GumballFrame>.None);
    public static readonly GumballAlignment Current = new(read: static native =>
        native.TryGetGumballFrameForCurrentAlignment(frame: out GumballFrame frame) ? Some(frame) : Option<GumballFrame>.None);

    internal Func<RhinoObject, Option<GumballFrame>> Read { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FrameAsk {
    private FrameAsk() { }
    public sealed record Anchor(ObjectSignal Scale) : FrameAsk;
    public sealed record Gumball(GumballAlignment Alignment) : FrameAsk;
    public sealed record Drag : FrameAsk;

    internal Fin<FrameAsk> Admit(Op op) =>
        Switch(
            op,
            anchor: static (key, ask) => key.Need(ask.Scale).Map(_ => (FrameAsk)ask),
            gumball: static (key, ask) => key.Need(ask.Alignment)
                .Map(alignment => (FrameAsk)new Gumball(Alignment: alignment)),
            drag: static (_, ask) => Fin.Succ<FrameAsk>(ask));

    internal Option<FramePose> Read(RhinoObject native) =>
        Switch(
            native,
            anchor: static (live, ask) => Optional(live.ObjectFrame(flags: RhinoObject.ObjectFrameFlags.ReturnUnset
                    | (ask.Scale.On ? RhinoObject.ObjectFrameFlags.IncludeScaleTransforms : RhinoObject.ObjectFrameFlags.Standard)))
                .Filter(static plane => plane.IsValid)
                .Map(static plane => (FramePose)new FramePose.Placed(Frame: plane)),
            gumball: static (live, ask) => ask.Alignment.Read(live)
                .Map(static held => (FramePose)new FramePose.Handled(
                    Frame: held.Plane, ScaleGrip: held.ScaleGripDistance, Mode: held.ScaleMode)),
            drag: static (live, _) => live.HasDynamicTransform && live.GetDynamicTransform(transform: out Transform motion)
                ? Some((FramePose)new FramePose.Dragging(Motion: motion))
                : Option<FramePose>.None);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FramePose : IDetachedDocumentResult {
    private FramePose() { }
    public sealed record Placed(Plane Frame) : FramePose;
    public sealed record Handled(Plane Frame, Vector3d ScaleGrip, GumballScaleMode Mode) : FramePose;
    public sealed record Dragging(Transform Motion) : FramePose;
}
```

## [04]-[REACH_AND_TOUCH]

- Owner: `Reach` `[Union]` closes whole-object, one-component, component-set, and every-component addressing; `ObjectSignal` owns enabled/disabled intent; `Touch` `[Union]` closes component selection and whole-or-component highlight; `TouchResult` `[Union]` preserves each native result regime.
- Law: the reach split closes the selection ownership — whole-object id-set selection is the table rail's `TableOp.Select`, so `Touch.Select` refuses `Whole` at the factory and owns component reach alone; `Touch.Highlight` owns every reach because the table rail carries no highlight member.
- Law: an all-parts sweep is directional — `EveryPart` with `ObjectSignal.Disabled` runs `UnselectAllSubObjects`/`UnhighlightAllSubObjects`, while `ObjectSignal.Enabled` is refused because no host member selects every component in one call.
- Law: touch is immediate visual state — no undo record opens, the entry demands `SessionNeed.Mutate` alone, and redraw stays caller policy. `Touch` preflights and captures every target before mutation, applies the batch fail-fast, and restores the complete captured roster through one accumulating compensation rail on refusal. Each mutation reads its final native grade before returning, so multi-component results never derive from a peak fold over per-call return values.
- Growth: a new component verb is one `Touch` case dispatched in the same fold; a new reach shape is one `Reach` case every verb arm reads.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Reach {
    private Reach() { }
    public sealed record Whole : Reach;
    public sealed record Part(ComponentIndex Component) : Reach;
    public sealed record Parts(Seq<ComponentIndex> Components) : Reach;
    public sealed record EveryPart : Reach;

    public static Reach Of(params ReadOnlySpan<ComponentIndex> components) =>
        components switch {
            [] => new EveryPart(),
            [var only] => new Part(Component: only),
            _ => new Parts(Components: toSeq(components.ToArray())),
        };

    internal Seq<ComponentIndex> Roster => Switch(
        whole: static _ => Seq<ComponentIndex>(),
        part: static reach => Seq(reach.Component),
        parts: static reach => reach.Components,
        everyPart: static _ => Seq<ComponentIndex>());

    internal Fin<Reach> Admit(Op op) =>
        Switch(
            op,
            whole: static (_, reach) => Fin.Succ<Reach>(reach),
            part: static (_, reach) => Fin.Succ<Reach>(reach),
            parts: static (key, reach) => guard(!reach.Components.IsEmpty, key.InvalidInput()).ToFin()
                .Map(_ => (Reach)new Parts(Components: reach.Components.Distinct())),
            everyPart: static (_, reach) => Fin.Succ<Reach>(reach));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Touch {
    private Touch() { }

    private sealed record SelectCase(Reach Scope, ObjectSignal Signal, SelectionPolicy Policy) : Touch;
    private sealed record HighlightCase(Reach Scope, ObjectSignal Signal) : Touch;
    private sealed record TouchState(
        RhinoObject Native,
        SelectionGrade Selection,
        Seq<ComponentIndex> Selected,
        HighlightState Highlight);

    public static Fin<Touch> Select(Reach scope, ObjectSignal signal, SelectionPolicy policy) {
        Op op = Op.Of();
        return from address in op.Need(scope).Bind(value => value.Admit(op: op))
               from state in op.Need(signal)
               from behavior in op.Need(policy)
               from _ in guard(address is not Reach.Whole && !(address is Reach.EveryPart && state.On), op.InvalidInput()).ToFin()
               select (Touch)new SelectCase(Scope: address, Signal: state, Policy: behavior);
    }

    public static Fin<Touch> Highlight(Reach scope, ObjectSignal signal) {
        Op op = Op.Of();
        return from address in op.Need(scope).Bind(value => value.Admit(op: op))
               from state in op.Need(signal)
               from _ in guard(!(address is Reach.EveryPart && state.On), op.InvalidInput()).ToFin()
               select (Touch)new HighlightCase(Scope: address, Signal: state);
    }

    internal Fin<Seq<TouchResult>> Transact(Seq<RhinoObject> natives, Op key) =>
        natives.TraverseM(native => Capture(native: native, key: key)).As()
            .Bind(states => ApplyCaptured(states: states, key: key));

    private Fin<TouchState> Capture(RhinoObject native, Op key) {
        Touch self = this;
        return key.Catch(() =>
            from selection in SelectionGrade.Of(native.IsSelected(checkSubObjects: true), key)
            from _ in self.Switch(
                (Native: native, Op: key),
                selectCase: static (context, touch) => touch.Scope.Roster
                    .TraverseM(component => guard(
                        !touch.Signal.On || context.Native.IsSubObjectSelectable(
                            componentIndex: component, ignoreSelectionState: true),
                        context.Op.InvalidInput()).ToFin())
                    .As()
                    .Map(static _ => unit),
                highlightCase: static (_, _) => Fin.Succ(value: unit))
            select new TouchState(
                Native: native,
                Selection: selection,
                Selected: Optional(native.GetSelectedSubObjects())
                    .Map(static rows => toSeq(rows)).IfNone(Seq<ComponentIndex>()),
                Highlight: Highlight(native)));
    }

    private Fin<Seq<TouchResult>> ApplyCaptured(Seq<TouchState> states, Op key) {
        Fin<Seq<TouchResult>> primary = states.Fold(
            Fin.Succ(value: Seq<TouchResult>()),
            (flow, state) => flow.Bind(results => Apply(native: state.Native, key: key)
                .Map(result => results.Add(value: result))));
        return primary.BindFail(failure => Restore(states: states, key: key).Match(
            Succ: _ => Fin.Fail<Seq<TouchResult>>(error: failure),
            Fail: cleanup => Fin.Fail<Seq<TouchResult>>(error: failure + cleanup)));
    }

    private Fin<TouchResult> Apply(RhinoObject native, Op key) =>
        Switch(
            (Native: native, Op: key),
            selectCase: static (context, touch) => touch.Scope switch {
                Reach.EveryPart => context.Op.Catch(() => {
                    _ = context.Native.UnselectAllSubObjects();
                    return SelectionGrade.Of(context.Native.IsSelected(checkSubObjects: true), context.Op)
                        .Map(grade => (TouchResult)new TouchResult.Selected(Id: context.Native.Id, Grade: grade));
                }),
                var scoped => scoped.Roster.TraverseM(component => context.Op.Catch(() => {
                        _ = context.Native.SelectSubObject(
                            componentIndex: component,
                            select: touch.Signal.On,
                            syncHighlight: touch.Policy.Highlight,
                            persistentSelect: touch.Policy.Persistent);
                        return guard(
                            context.Native.IsSubObjectSelected(componentIndex: component) == touch.Signal.On,
                            context.Op.InvalidResult()).ToFin();
                    })).As()
                    .Bind(_ => SelectionGrade.Of(context.Native.IsSelected(checkSubObjects: true), context.Op))
                    .Map(grade => (TouchResult)new TouchResult.Selected(Id: context.Native.Id, Grade: grade)),
            },
            highlightCase: static (context, touch) => touch.Scope switch {
                Reach.Whole => context.Op.Catch(() => guard(
                        context.Native.Highlight(enable: touch.Signal.On) == touch.Signal.On,
                        context.Op.InvalidResult()).ToFin())
                    .Map(_ => (TouchResult)new TouchResult.Highlighted(
                        Id: context.Native.Id,
                        State: Highlight(context.Native))),
                Reach.EveryPart => context.Op.Catch(() => {
                    _ = context.Native.UnhighlightAllSubObjects();
                    return Fin.Succ<TouchResult>(value: new TouchResult.Highlighted(
                        Id: context.Native.Id,
                        State: Highlight(context.Native)));
                }),
                var scoped => scoped.Roster.TraverseM(component => context.Op.Catch(() => guard(
                        context.Native.HighlightSubObject(componentIndex: component, highlight: touch.Signal.On) == touch.Signal.On,
                        context.Op.InvalidResult()).ToFin())).As()
                    .Map(_ => (TouchResult)new TouchResult.Highlighted(
                        Id: context.Native.Id,
                        State: Highlight(context.Native))),
            });

    private static HighlightState Highlight(RhinoObject native) => new(
        Native: native.IsHighlighted(checkSubObjects: false),
        Components: Optional(native.GetHighlightedSubObjects())
            .Map(static rows => toSeq(rows)).IfNone(Seq<ComponentIndex>()));

    private static Fin<Unit> Restore(Seq<TouchState> states, Op key) =>
        states.Traverse(state => Restore(state: state, key: key).ToValidation()).As()
            .ToFin()
            .Map(static _ => unit);

    private static Fin<Unit> Restore(TouchState state, Op key) {
        Seq<Func<Fin<Unit>>> steps = [
            () => {
                _ = state.Native.UnselectAllSubObjects();
                return Fin.Succ(value: unit);
            },
            .. state.Selected.Map(component => (Func<Fin<Unit>>)(() => {
                _ = state.Native.SelectSubObject(
                    componentIndex: component,
                    select: true,
                    syncHighlight: false,
                    persistentSelect: state.Selection == SelectionGrade.Persistent);
                return guard(
                    state.Native.IsSubObjectSelected(componentIndex: component),
                    key.InvalidResult()).ToFin();
            })),
            () => {
                _ = state.Native.UnhighlightAllSubObjects();
                return Fin.Succ(value: unit);
            },
            .. state.Highlight.Components.Map(component => (Func<Fin<Unit>>)(() =>
                guard(
                    state.Native.HighlightSubObject(componentIndex: component, highlight: true),
                    key.InvalidResult()).ToFin()
            )),
            () => guard(
                state.Native.Highlight(enable: state.Highlight.Native != 0) == (state.Highlight.Native != 0),
                key.InvalidResult()).ToFin(),
        ];
        return steps.Traverse(step => key.Catch(step).ToValidation())
            .As()
            .ToFin()
            .Map(static _ => unit);
    }
}

// --- [MODELS] -----------------------------------------------------------------------------
[SmartEnum]
public sealed partial class ObjectSignal {
    public static readonly ObjectSignal Disabled = new(on: false);
    public static readonly ObjectSignal Enabled = new(on: true);

    internal bool On { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TouchResult {
    private TouchResult() { }
    public sealed record Selected(Guid Id, SelectionGrade Grade) : TouchResult;
    public sealed record Highlighted(Guid Id, HighlightState State) : TouchResult;
}
```

## [05]-[CUTS_AND_PIECES]

- Owner: `SectionCut` `[Union]` — the three extraction requests: `Profile` a planar section, `Slab` a thickness slice about a center plane, `Fill` the section-fill breps against resolved clipping planes; `ObjectPiece` — one detached extraction product: geometry under custody, attributes where the host pairs them.
- Law: extraction is read-shaped — `CreateSections`, `CreateSlices`, and `GetFillSurfaces` return detached geometry and never touch the table, so a cut inside a paused command pollutes no undo stack, and landing pieces in the document is the caller's `TableOp.Add` decision.
- Law: host-returned section, slice, and fill arrays are owned acquisitions. `ObjectPiece.Detach` duplicates both payloads, the source arrays release once through `ObjectPiece.Release` after the fold, and `DetachAll` disposes every accumulated piece before returning a mid-fold refusal.
- Law: plural extraction is transactional custody — `ObjectPiece.Acquire` releases every product accumulated from earlier objects when a later object fails, `ObjectPiece.DetachAll` is the one paired detach-or-rollback fold every extraction, member, cache, and harvest consumer composes, and `StateAnswer.Dispose` releases the complete successful piece roster idempotently.
- Law: dissection rides the same product — `GetSubObjects` explodes the object into detached members the caller owns, so member geometry detaches onto handles with duplicated attributes and never re-enters as live state; the census consumer reads the piece roster, never a host array.
- Law: fill resolution demands live clipping planes — each requested id resolves through `FindId` to a `ClippingPlaneObject` inside the grant, and a non-plane id is a typed refusal, never a silent skip.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SectionCut {
    private SectionCut() { }

    private sealed record ProfileCase(Plane Plane, string Name, double Tolerance) : SectionCut;
    private sealed record SlabCase(Plane Center, string Name, double Thickness, double Tolerance) : SectionCut;
    private sealed record FillCase(Seq<Guid> ClippingPlanes, ObjectSignal Unclipped) : SectionCut;

    public static Fin<SectionCut> Profile(Plane plane, string name, double tolerance) {
        Op op = Op.Of();
        return from frame in op.AcceptInput(value: plane)
               from label in op.AcceptText(value: name)
               from width in op.Positive(value: tolerance)
               select (SectionCut)new ProfileCase(Plane: frame, Name: label, Tolerance: width);
    }

    public static Fin<SectionCut> Slab(Plane center, string name, double thickness, double tolerance) {
        Op op = Op.Of();
        return from frame in op.AcceptInput(value: center)
               from label in op.AcceptText(value: name)
               from depth in op.Positive(value: thickness)
               from width in op.Positive(value: tolerance)
               select (SectionCut)new SlabCase(Center: frame, Name: label, Thickness: depth, Tolerance: width);
    }

    public static Fin<SectionCut> Fill(Seq<Guid> clippingPlanes, ObjectSignal unclipped) {
        Op op = Op.Of();
        return from ids in clippingPlanes.TraverseM(id => id != Guid.Empty
                   ? Fin.Succ(value: id)
                   : Fin.Fail<Guid>(error: op.InvalidInput())).As()
               from policy in op.Need(unclipped)
               from _ in guard(!ids.IsEmpty, op.InvalidInput()).ToFin()
               select (SectionCut)new FillCase(ClippingPlanes: ids.Distinct(), Unclipped: policy);
    }

    internal Fin<Seq<ObjectPiece>> Extract(RhinoDoc document, RhinoObject native, Op key) =>
        Switch(
            (Document: document, Native: native, Op: key),
            profileCase: static (context, cut) => context.Op.Catch(() => Paired(
                geometry: context.Native.CreateSections(
                    plane: cut.Plane, name: cut.Name, tolerance: cut.Tolerance,
                    objectAttributes: out ObjectAttributes[] attributes),
                attributes: attributes, key: context.Op)),
            slabCase: static (context, cut) => context.Op.Catch(() => Paired(
                geometry: context.Native.CreateSlices(
                    centerPlane: cut.Center, name: cut.Name, thickness: cut.Thickness, tolerance: cut.Tolerance,
                    objectAttributes: out ObjectAttributes[] attributes),
                attributes: attributes, key: context.Op)),
            fillCase: static (context, cut) =>
                from planes in cut.ClippingPlanes.TraverseM(id =>
                    Optional(context.Document.Objects.FindId(id)).ToFin(Fail: context.Op.MissingContext())
                        .Bind(found => Optional(found as ClippingPlaneObject).ToFin(Fail: context.Op.InvalidInput()))).As()
                from pieces in context.Op.Catch(() => Paired(
                    geometry: RhinoObject.GetFillSurfaces(
                        rhinoObject: context.Native, clippingPlaneObjects: planes.AsIterable(), unclippedFills: cut.Unclipped.On),
                    attributes: null, key: context.Op))
                select pieces);

    private static Fin<Seq<ObjectPiece>> Paired(GeometryBase[]? geometry, ObjectAttributes[]? attributes, Op key) {
        Fin<Seq<ObjectPiece>> result =
            from shapes in Optional(geometry).ToFin(Fail: key.InvalidResult()).Map(static values => toSeq(values))
            from _ in guard(attributes is null || attributes.Length == shapes.Count, key.InvalidResult()).ToFin()
            from pieces in ObjectPiece.DetachAll(
                rows: shapes.Map((shape, index) => (shape, Optional(attributes).Bind(paired => Optional(paired[index])))),
                key: key)
            select pieces;
        _ = ObjectPiece.Release(geometry: geometry, attributes: attributes);
        return result;
    }
}

// --- [MODELS] -----------------------------------------------------------------------------
public sealed class ObjectPiece : IDisposable {
    private int released;

    private ObjectPiece(GeometryHandle geometry, Option<ObjectAttributes> attributes) {
        Geometry = geometry;
        Attributes = attributes;
    }

    public GeometryHandle Geometry { get; }
    public Option<ObjectAttributes> Attributes { get; }

    internal static Fin<ObjectPiece> Detach(GeometryBase geometry, Option<ObjectAttributes> attributes, Op key) =>
        from handle in GeometryCrossing.Cross(source: geometry, mode: CrossingMode.Detach, key: key)
        from metadata in key.Catch(() => Fin.Succ(value: attributes.Map(static value => value.Duplicate())))
            .MapFail(error => {
                handle.Dispose();
                return error;
            })
        select new ObjectPiece(geometry: handle, attributes: metadata);

    internal static Fin<Seq<ObjectPiece>> DetachAll(
        Seq<(GeometryBase Shape, Option<ObjectAttributes> Attributes)> rows, Op key) =>
        rows.Fold(
            Fin.Succ(value: Seq<ObjectPiece>()),
            (state, row) => state.Bind(held => Detach(geometry: row.Shape, attributes: row.Attributes, key: key)
                .Map(piece => held.Add(value: piece))
                .MapFail(error => {
                    _ = held.Iter(static prior => prior.Dispose());
                    return error;
                })));

    internal static Fin<Seq<(Guid Id, Seq<ObjectPiece> Products)>> Acquire(
        Seq<RhinoObject> natives,
        Func<RhinoObject, Fin<Seq<ObjectPiece>>> detach) =>
        natives.Fold(
            Fin.Succ(value: Seq<(Guid Id, Seq<ObjectPiece> Products)>()),
            (state, native) => state.Bind(held => detach(native)
                .Map(products => held.Add(value: (native.Id, products)))
                .MapFail(error => {
                    _ = Release(held);
                    return error;
                })));

    internal static Unit Release(GeometryBase[]? geometry, ObjectAttributes[]? attributes) {
        _ = Optional(geometry).Iter(static rows => {
            foreach (GeometryBase? shape in rows) { shape?.Dispose(); }
        });
        _ = Optional(attributes).Iter(static rows => {
            foreach (ObjectAttributes? metadata in rows) { metadata?.Dispose(); }
        });
        return unit;
    }

    internal static Unit Release(Seq<(Guid Id, Seq<ObjectPiece> Products)> rows) =>
        rows.Fold(
            unit,
            static (state, row) => row.Products.Fold(state, static (held, piece) => {
                piece.Dispose();
                return held;
            }));

    internal static Unit Release(Seq<(Guid Id, ObjectPiece Product)> rows) =>
        rows.Fold(unit, static (state, row) => {
            row.Product.Dispose();
            return state;
        });

    public void Dispose() {
        if (Interlocked.Exchange(location1: ref released, value: 1) is not 0) { return; }
        Geometry.Dispose();
        _ = Attributes.Iter(static value => value.Dispose());
    }
}
```

## [06]-[ASK_ENTRY]

- Owner: `StateAsk` `[Union]` closes snapshot, frame, component-roster, targeted component-state, extent, member, and cut reads; `StateAnswer` `[Union]` owns the corresponding detached products; `ObjectReceipt<TFact>` is the one fact-plus-undo-serial receipt monoid every mutation rail returns; `ObjectSpine` is the one commit entry — needs derived through `SessionNeed.Mutation`, one demand, and the Document spine's `DocumentCommit.Sealed` envelope carrying the `ObjectReceipt<TFact>` fold and serial stamp; `Objects.Ask` and `Objects.Touch` are the polymorphic read and immediate-state entries; `Objects.Resolve` is the shared one-hop object window.
- Entry: `Objects.Ask(DocumentSession, TableTarget, StateAsk) : Fin<StateAnswer>` demands `SessionNeed.Read`; `Objects.Touch(DocumentSession, TableTarget, Touch) : Fin<ObjectReceipt<TouchResult>>` demands `SessionNeed.Mutate`; both resolve the target once and fold per object inside one grant window.
- Law: the spine is the one undo-recorded bracket owner for the namespace — light, material, and history commits walk `ObjectSpine.Commit` verbatim, each supplying only its fact fold; immediate visual `Objects.Touch` remains outside the bracket by contract; a recorded rail re-spelling the demand/envelope sequence or opening `UndoBracket.Begin` beside `Sealed` is the deleted form, and a rail's facts stay its own typed `TFact` union so no evidence flattens into a shared body vocabulary.
- Law: resolution is the table vocabulary — `TableTarget.Resolve` answers the id set and `FindId` lifts each to the live handle typed, so explicit ids, runtime pairs, and admitted queries address the object window identically; a deleted id is `MissingContext`, never a null propagated inward.
- Law: batch extent composes the host batch member — `Extent` runs the static `GetTightBoundingBox` over the whole resolved roster in one native call, with the plane overload selected by the ask's optional frame; a per-object union re-derived from single boxes is the deleted form.
- Law: answers embed identity — every per-object row carries the object guid beside its payload, and `ComponentState` also carries its `ComponentIndex`; component eligibility records both current-state and ignore-selection answers, so `IsSubObjectSelectable(ComponentIndex, bool)` keeps its host boolean at the boundary instead of exporting a request knob.
- Boundary: visual-analysis attachment — `EnableVisualAnalysisMode`, its active-mode queries, and the `AnalysisModeChanged` static event — is the display page's analysis-mode extension; this window carries no analysis case and composes that seam's receipts where an ask needs the fact.
- Growth: a new read is one ask case with its answer case; the dispatch, the entries, and every consumer read it with zero new surface.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record StateAsk {
    private StateAsk() { }
    public sealed record Snapshot : StateAsk;
    public sealed record Frames(FrameAsk Frame) : StateAsk;
    public sealed record SelectedParts : StateAsk;
    public sealed record HighlightedParts : StateAsk;
    public sealed record Components(Reach Scope) : StateAsk;
    public sealed record Extent(Option<Plane> Frame = default) : StateAsk;
    public sealed record Members : StateAsk;
    public sealed record Cut(SectionCut Section) : StateAsk;

    internal Fin<StateAsk> Admit(Op op) =>
        Switch(
            op,
            snapshot: static (_, ask) => Fin.Succ<StateAsk>(ask),
            frames: static (key, ask) => key.Need(ask.Frame)
                .Bind(frame => frame.Admit(key))
                .Map(frame => (StateAsk)new Frames(Frame: frame)),
            selectedParts: static (_, ask) => Fin.Succ<StateAsk>(ask),
            highlightedParts: static (_, ask) => Fin.Succ<StateAsk>(ask),
            components: static (key, ask) => key.Need(ask.Scope)
                .Bind(scope => scope.Admit(key))
                .Bind(scope => guard(scope is Reach.Part or Reach.Parts, key.InvalidInput()).ToFin().Map(_ => (StateAsk)new Components(scope))),
            extent: static (key, ask) => ask.Frame.Traverse(frame => key.AcceptInput(value: frame)).As()
                .Map(frame => (StateAsk)new Extent(Frame: frame)),
            members: static (_, ask) => Fin.Succ<StateAsk>(ask),
            cut: static (key, ask) => key.Need(ask.Section).Map(_ => (StateAsk)ask));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record StateAnswer : IDetachedDocumentResult, IDisposable {
    private StateAnswer() { }
    public sealed record States(Seq<ObjectSnapshot> Rows) : StateAnswer;
    public sealed record Posed(Seq<(Guid Id, Option<FramePose> Pose)> Rows) : StateAnswer;
    public sealed record PartRoster(Seq<(Guid Id, Seq<ComponentIndex> Components)> Rows) : StateAnswer;
    public sealed record ComponentStates(Seq<ComponentState> Rows) : StateAnswer;
    public sealed record Extent(BoundingBox Bounds) : StateAnswer;
    public sealed record Pieces(Seq<(Guid Id, Seq<ObjectPiece> Products)> Rows) : StateAnswer;

    public void Dispose() =>
        Switch(
            states: static _ => unit,
            posed: static _ => unit,
            partRoster: static _ => unit,
            componentStates: static _ => unit,
            extent: static _ => unit,
            pieces: static answer => ObjectPiece.Release(answer.Rows));
}

public readonly record struct ComponentState(
    Guid Id,
    ComponentIndex Component,
    bool Selected,
    bool Selectable,
    bool SelectableIgnoringSelection,
    bool Highlighted);

public readonly record struct ObjectReceipt<TFact>(Seq<TFact> Facts, Seq<uint> UndoSerials) : IDetachedDocumentResult {
    public static readonly ObjectReceipt<TFact> Empty = new(Facts: Seq<TFact>(), UndoSerials: Seq<uint>());

    public ObjectReceipt<TFact> Contribute(ObjectReceipt<TFact> contribution) =>
        new(Facts: Facts + contribution.Facts, UndoSerials: UndoSerials + contribution.UndoSerials);

    public static ObjectReceipt<TFact> Collect(params ReadOnlySpan<ObjectReceipt<TFact>> receipts) =>
        toSeq(receipts.ToArray()).Fold(Empty, static (held, receipt) => held.Contribute(receipt));
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class Objects {
    public static Fin<StateAnswer> Ask(DocumentSession session, TableTarget target, StateAsk ask) {
        Op op = Op.Of();
        return from active in op.Need(ask).Bind(value => value.Admit(op: op))
               from answer in session.Demand(
                   use: document =>
                       from natives in Resolve(document: document, target: target, key: op)
                       from folded in active.Switch(
                           (Document: document, Natives: natives, Op: op),
                           snapshot: static (ctx, _) => ctx.Natives
                               .TraverseM(native => ObjectSnapshot.Of(native: native, key: ctx.Op)).As()
                               .Map(static rows => (StateAnswer)new StateAnswer.States(Rows: rows)),
                           frames: static (ctx, ask) => ctx.Natives
                               .TraverseM(native => ctx.Op.Catch(() =>
                                   Fin.Succ(value: (native.Id, ask.Frame.Read(native: native))))).As()
                               .Map(static rows => (StateAnswer)new StateAnswer.Posed(Rows: rows)),
                           selectedParts: static (ctx, _) => ctx.Natives
                               .TraverseM(native => ctx.Op.Catch(() =>
                                   Fin.Succ(value: (native.Id, toSeq(native.GetSelectedSubObjects()))))).As()
                               .Map(static rows => (StateAnswer)new StateAnswer.PartRoster(Rows: rows)),
                           highlightedParts: static (ctx, _) => ctx.Natives
                               .TraverseM(native => ctx.Op.Catch(() =>
                                   Fin.Succ(value: (native.Id, toSeq(native.GetHighlightedSubObjects()))))).As()
                               .Map(static rows => (StateAnswer)new StateAnswer.PartRoster(Rows: rows)),
                           components: static (ctx, ask) => ctx.Natives
                               .Bind(native => ask.Scope.Roster.Map(component => (Native: native, Component: component)))
                               .TraverseM(row => ctx.Op.Catch(() => Fin.Succ(value: new ComponentState(
                                   Id: row.Native.Id,
                                   Component: row.Component,
                                   Selected: row.Native.IsSubObjectSelected(componentIndex: row.Component),
                                   Selectable: row.Native.IsSubObjectSelectable(
                                       componentIndex: row.Component, ignoreSelectionState: false),
                                   SelectableIgnoringSelection: row.Native.IsSubObjectSelectable(
                                       componentIndex: row.Component, ignoreSelectionState: true),
                                   Highlighted: row.Native.IsSubObjectHighlighted(componentIndex: row.Component))))).As()
                               .Map(static rows => (StateAnswer)new StateAnswer.ComponentStates(Rows: rows)),
                           extent: static (ctx, ask) => ctx.Op.Catch(() => {
                               BoundingBox bounds = BoundingBox.Unset;
                               bool answered = ask.Frame.Case switch {
                                   Plane frame => RhinoObject.GetTightBoundingBox(
                                       rhinoObjects: ctx.Natives.AsIterable(), plane: frame, boundingBox: out bounds),
                                   _ => RhinoObject.GetTightBoundingBox(
                                       rhinoObjects: ctx.Natives.AsIterable(), boundingBox: out bounds),
                               };
                               return answered && bounds.IsValid
                                   ? Fin.Succ(value: (StateAnswer)new StateAnswer.Extent(Bounds: bounds))
                                   : Fin.Fail<StateAnswer>(error: ctx.Op.InvalidResult());
                           }),
                           members: static (ctx, _) => ObjectPiece.Acquire(
                               natives: ctx.Natives,
                               detach: native => ctx.Op.Catch(() =>
                                   Optional(native.GetSubObjects()).ToFin(Fail: ctx.Op.InvalidResult())
                                       .Bind(parts => DetachMembers(members: parts, key: ctx.Op))))
                               .Map(static rows => (StateAnswer)new StateAnswer.Pieces(Rows: rows)),
                           cut: static (ctx, ask) => ObjectPiece.Acquire(
                               natives: ctx.Natives,
                               detach: native => ask.Section.Extract(
                                   document: ctx.Document, native: native, key: ctx.Op))
                               .Map(static rows => (StateAnswer)new StateAnswer.Pieces(Rows: rows)))
                       select folded,
                   key: op,
                   needs: [SessionNeed.Read])
               select answer;
    }

    public static Fin<ObjectReceipt<TouchResult>> Touch(DocumentSession session, TableTarget target, Touch touch) {
        Op op = Op.Of();
        return from active in op.Need(touch)
               from receipt in session.Demand(
                   use: document =>
                       from natives in Resolve(document: document, target: target, key: op)
                       from results in active.Transact(natives: natives, key: op)
                       select new ObjectReceipt<TouchResult>(Facts: results, UndoSerials: Seq<uint>()),
                   key: op,
                   needs: [SessionNeed.Mutate])
               select receipt;
    }

    internal static Fin<Seq<RhinoObject>> Resolve(RhinoDoc document, TableTarget target, Op key) =>
        from address in key.Need(target)
        from ids in address.Resolve(document: document, key: key)
        from natives in ids.TraverseM(id =>
            Optional(document.Objects.FindId(id)).ToFin(Fail: key.MissingContext())).As()
        select natives;

    private static Fin<Seq<ObjectPiece>> DetachMembers(RhinoObject[] members, Op key) {
        Fin<Seq<ObjectPiece>> result = toSeq(members)
            .TraverseM(member => Optional(member.Geometry).ToFin(Fail: key.InvalidResult())
                .Map(geometry => (geometry, Optional(member.Attributes)))).As()
            .Bind(rows => ObjectPiece.DetachAll(rows: rows, key: key));
        foreach (RhinoObject? member in members) { member?.Dispose(); }
        return result;
    }
}

internal static class ObjectSpine {
    internal static Fin<ObjectReceipt<TFact>> Commit<TFact>(
        DocumentSession session, string name, RedrawPolicy redraw,
        Func<RhinoDoc, Op, Fin<Seq<TFact>>> fold, Op op, bool recordsUndo = true) =>
        session.Demand(
            use: document => DocumentCommit.Sealed(
                document: document,
                name: name,
                recordsUndo: recordsUndo,
                redraw: redraw,
                run: () => fold(document, op)
                    .Map(static facts => new ObjectReceipt<TFact>(Facts: facts, UndoSerials: Seq<uint>())),
                stamp: static (receipt, serial) => receipt with {
                    UndoSerials = serial > 0u ? Seq(serial) : Seq<uint>(),
                },
                op: op),
            key: op,
            needs: SessionNeed.Mutation(undo: recordsUndo, redraw: redraw).ToArray());
}
```

## [07]-[SURFACE_LEDGER]

| [INDEX] | [CONCERN]           | [OWNER]               | [FORM]                                               | [ENTRY]                        |
| :-----: | :------------------ | :-------------------- | :--------------------------------------------------- | :----------------------------- |
|  [01]   | native evidence     | snapshot/results      | typed selection grade and raw highlight scope        | snapshot / `TouchResult`       |
|  [02]   | object state        | `ObjectSnapshot`      | one-pass read, host discriminants at the seam        | `StateAsk.Snapshot`            |
|  [03]   | frame reads         | `FrameAsk`            | object, gumball, and drag poses as one union         | `StateAsk.Frames`              |
|  [04]   | component reach     | `Reach`               | whole, part, parts, and every-part as one address    | `Reach.Of` / `Touch` payloads  |
|  [05]   | immediate touch     | `Touch`               | select and highlight verbs, table-rail split honored | `Objects.Touch`                |
|  [06]   | extraction custody  | `SectionCut`          | sections, slices, fills onto detached `ObjectPiece`  | `StateAsk.Cut` / `Members`     |
|  [07]   | detach fold         | `ObjectPiece`         | custody-compensated product fold                     | `DetachAll`                    |
|  [08]   | batch custody       | `ObjectPiece`         | cross-object rollback                                | `Acquire`                      |
|  [09]   | native release      | `ObjectPiece`         | host-array disposal                                  | `Release`                      |
|  [10]   | read dispatch       | `StateAsk`            | typed answer union                                   | `Objects.Ask`                  |
|  [11]   | object resolution   | `Objects`             | target-to-handle lift                                | `Resolve(document, target)`    |
|  [12]   | receipt monoid      | `ObjectReceipt<TFact>` | typed fact and undo fold                            | `Collect`                      |
|  [13]   | commit kernel       | `ObjectSpine`         | mutation-envelope composition                        | `Commit(session, name, ...)`   |
