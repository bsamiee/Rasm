# [RASM_RHINO_OBJECTS_STATE]

Live document-object state belongs to `Rasm.Rhino.Objects`. One `StateAsk` family answers the catalogued state reads assigned to this owner — the whole-state snapshot, object and gumball frames, in-flight drag transforms, subobject rosters, batch tight extents, detached member pieces, planar sections, thickness slices, and clipping fills. One `Touch` family owns component selection and every highlight mutation absent from the table rail. Addressing composes `TableTarget`, every answer leaves the session grant detached, and every `ObjectPiece` owns its copied geometry and attributes until disposal. `RhinoObject.CommitChanges` has no path: attribute mutation rides `TableOp.Amend`, mode mutation rides `TableOp.State`, and geometry mutation rides `TableOp.Replace`.

## [01]-[INDEX]

- [02]-[SNAPSHOT]: `SelectionGrade`, `HighlightState`, and the one-pass `ObjectSnapshot` read product.
- [03]-[FRAMES]: `FrameAsk`/`FramePose` — object frame, gumball frame, and drag-transform reads.
- [04]-[REACH_AND_TOUCH]: `Reach`, `Touch`, and the immediate component selection and highlight rail.
- [05]-[CUTS_AND_PIECES]: `SectionCut`, `ObjectPiece`, and the detached extraction custody.
- [06]-[ASK_ENTRY]: `StateAsk`/`StateAnswer` and the `Objects` entry pair.
- [07]-[SURFACE_LEDGER]: the page's owner table.

## [02]-[SNAPSHOT]

- Owner: `SelectionGrade` `[SmartEnum<int>]` closes the catalogued selection return: `None`, `Held`, and `Persistent`. `HighlightState` carries the catalogued whole-object predicate and highlighted-component roster without assigning undocumented meaning to `IsHighlighted(checkSubObjects: true)` integers. `ObjectSnapshot` carries the remaining catalogued scalar state fields directly.
- Law: the snapshot reads once per object inside the session grant — every field lands in one pass over the resolved handle, so a consumer never re-enters the document to complete a partial read, and the product is detached the moment `Ask` returns.
- Law: selection and highlight never share a vocabulary. Selection closes the documented integer grade; highlight reads `IsHighlighted(checkSubObjects: false) > 0` plus `GetHighlightedSubObjects()` and exposes no undocumented integer.
- Law: `CommitChanges` never appears — the host member answers `true` only when a staged working copy actually flushed, and this package stages nothing on the live object: attribute writes travel `TableOp.Amend`, mode and visibility travel `TableOp.State`, geometry travels `TableOp.Replace`; the snapshot is the read face of that one-write-path law.
- Law: history linkage is one snapshot bool — `HistoryBound` carries `HasHistoryRecord()` as presence evidence, and every linkage read or mutation lives on the history page's `Chronicle`.
- Growth: a new host object fact is one snapshot field read in the same pass; a new native grade is one row on its owning vocabulary.

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

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class SelectionGrade {
    public static readonly SelectionGrade None = new(key: 0);
    public static readonly SelectionGrade Held = new(key: 1);
    public static readonly SelectionGrade Persistent = new(key: 2);

    internal static SelectionGrade Of(int native) => Get(native) ?? None;
}

// --- [MODELS] -----------------------------------------------------------------------------
public readonly record struct HighlightState(bool Whole, Seq<ComponentIndex> Components);

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
    string Description) : IDetachedDocumentResult {
    internal static Fin<ObjectSnapshot> Of(RhinoObject native, Op key) =>
        key.Catch(() => Fin.Succ(value: new ObjectSnapshot(
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
            Selection: SelectionGrade.Of(native: native.IsSelected(checkSubObjects: true)),
            Highlight: new HighlightState(
                Whole: native.IsHighlighted(checkSubObjects: false) > 0,
                Components: toSeq(native.GetHighlightedSubObjects())),
            Selectable: native.IsSelectable(),
            GripsOn: native.GripsOn,
            GripsSelected: native.GripsSelected,
            DynamicTransform: native.HasDynamicTransform,
            HistoryBound: native.HasHistoryRecord(),
            HistoryCopiesOnReplace: native.CopyHistoryOnReplace(),
            MemoryBytes: native.MemoryEstimate(),
            Description: native.ShortDescription(plural: false))));
}
```

## [03]-[FRAMES]

- Owner: `FrameAsk` `[Union]` — the three frame questions: `Anchor` the object frame under `ObjectFrameFlags`, `Gumball` the gumball frame with the current-alignment grant, `Drag` the in-flight dynamic transform; `FramePose` `[Union]` — one typed pose per question, absence projected where the host answers none.
- Law: frame reads are object-side only — `RhinoObject.ObjectFrame` reads and the obsolete `RhinoObject.SetObjectFrame` overloads are dead, so every frame write is the attributes page's `Anchor` edit committed through the table rail's `Amend`, and this page never mutates a frame.
- Law: an unset frame is absence — `ObjectFrameFlags.ReturnUnset` yields an invalid plane the fold projects to `None`, a failed gumball probe projects to `None`, and a drag probe answers `None` outside an active drag; no consumer branches on `Plane.Unset`.
- Law: the gumball pose crosses detached — `GumballFrame` is a host struct whose `Plane`, `ScaleGripDistance`, and `ScaleMode` copy into the pose value, and `GumballScaleMode` rides the pose as a seam discriminant.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FrameAsk {
    private FrameAsk() { }
    public sealed record Anchor(ObjectFrameFlags Flags = ObjectFrameFlags.Standard) : FrameAsk;
    public sealed record Gumball(bool CurrentAlignment = false) : FrameAsk;
    public sealed record Drag : FrameAsk;

    internal Option<FramePose> Read(RhinoObject native) =>
        Switch(
            state: native,
            anchor: static (live, ask) => Optional(live.ObjectFrame(flags: ask.Flags))
                .Filter(static plane => plane.IsValid)
                .Map(static plane => (FramePose)new FramePose.Placed(Frame: plane)),
            gumball: static (live, ask) => (ask.CurrentAlignment
                    ? live.TryGetGumballFrameForCurrentAlignment(frame: out GumballFrame aligned) ? Some(aligned) : Option<GumballFrame>.None
                    : live.TryGetGumballFrame(frame: out GumballFrame standard) ? Some(standard) : Option<GumballFrame>.None)
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

- Owner: `Reach` `[Union]` closes whole-object, one-component, component-set, and every-component addressing. `Touch` `[Union]` closes component selection and whole-or-component highlight. `TouchResult` `[Union]` preserves the distinct selection and highlight result regimes inside one receipt.
- Law: the reach split closes the selection ownership — whole-object id-set selection is the table rail's `TableOp.Select`, so `Touch.Select` refuses `Whole` at the factory and owns component reach alone; `Touch.Highlight` owns every reach because the table rail carries no highlight member.
- Law: an all-parts sweep is directional — `EveryPart` with `On: false` runs the host `UnselectAllSubObjects`/`UnhighlightAllSubObjects` clear members, and `EveryPart` with `On: true` is refused typed because no host member selects every component in one call; a caller wanting it names the components through `Parts`.
- Law: touch is immediate visual state — no undo record opens, the entry demands `SessionNeed.Mutate` alone, and redraw stays caller policy. Each mutation reads its final native grade before returning, so multi-component results never derive from a peak fold over per-call return values.
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
            state: op,
            whole: static (_, reach) => Fin.Succ<Reach>(reach),
            part: static (_, reach) => Fin.Succ<Reach>(reach),
            parts: static (key, reach) => guard(!reach.Components.IsEmpty, key.InvalidInput()).ToFin()
                .Map(_ => (Reach)new Parts(Components: reach.Components.Distinct())),
            everyPart: static (_, reach) => Fin.Succ<Reach>(reach));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Touch {
    private Touch() { }

    private sealed record SelectCase(Reach Scope, bool On, SelectionPolicy Policy) : Touch;
    private sealed record HighlightCase(Reach Scope, bool On) : Touch;

    public static Fin<Touch> Select(Reach scope, bool on, SelectionPolicy policy) {
        Op op = Op.Of();
        return from address in Optional(scope).ToFin(Fail: op.InvalidInput()).Bind(value => value.Admit(op: op))
               from behavior in Optional(policy).ToFin(Fail: op.InvalidInput())
               from _ in guard(address is not Reach.Whole && !(address is Reach.EveryPart && on), op.InvalidInput()).ToFin()
               select (Touch)new SelectCase(Scope: address, On: on, Policy: behavior);
    }

    public static Fin<Touch> Highlight(Reach scope, bool on) {
        Op op = Op.Of();
        return from address in Optional(scope).ToFin(Fail: op.InvalidInput()).Bind(value => value.Admit(op: op))
               from _ in guard(!(address is Reach.EveryPart && on), op.InvalidInput()).ToFin()
               select (Touch)new HighlightCase(Scope: address, On: on);
    }

    internal Fin<TouchResult> Apply(RhinoObject native, Op key) =>
        Switch(
            state: (Native: native, Op: key),
            selectCase: static (context, touch) => touch.Scope switch {
                Reach.EveryPart => context.Op.Catch(() => {
                    _ = context.Native.UnselectAllSubObjects();
                    return Fin.Succ<TouchResult>(value: new TouchResult.Selected(
                        Id: context.Native.Id,
                        Grade: SelectionGrade.Of(native: context.Native.IsSelected(checkSubObjects: true))));
                }),
                var scoped => scoped.Roster.TraverseM(component => context.Op.AcceptValue(
                        value: context.Native.SelectSubObject(
                            componentIndex: component, select: touch.On,
                            syncHighlight: touch.Policy.Highlight, persistentSelect: touch.Policy.Persistent))).As()
                    .Map(_ => (TouchResult)new TouchResult.Selected(
                        Id: context.Native.Id,
                        Grade: SelectionGrade.Of(native: context.Native.IsSelected(checkSubObjects: true)))),
            },
            highlightCase: static (context, touch) => touch.Scope switch {
                Reach.Whole => context.Op.Confirm(success: context.Native.Highlight(enable: touch.On))
                    .Map(_ => (TouchResult)new TouchResult.Highlighted(
                        Id: context.Native.Id,
                        State: Highlight(context.Native))),
                Reach.EveryPart => context.Op.Catch(() => {
                    _ = context.Native.UnhighlightAllSubObjects();
                    return Fin.Succ<TouchResult>(value: new TouchResult.Highlighted(
                        Id: context.Native.Id,
                        State: Highlight(context.Native)));
                }),
                var scoped => scoped.Roster.TraverseM(component => context.Op.Confirm(
                        success: context.Native.HighlightSubObject(componentIndex: component, highlight: touch.On))).As()
                    .Map(_ => (TouchResult)new TouchResult.Highlighted(
                        Id: context.Native.Id,
                        State: Highlight(context.Native))),
            });

    private static HighlightState Highlight(RhinoObject native) => new(
        Whole: native.IsHighlighted(checkSubObjects: false) > 0,
        Components: toSeq(native.GetHighlightedSubObjects()));
}

// --- [MODELS] -----------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TouchResult {
    private TouchResult() { }
    public sealed record Selected(Guid Id, SelectionGrade Grade) : TouchResult;
    public sealed record Highlighted(Guid Id, HighlightState State) : TouchResult;
}

public readonly record struct TouchReceipt(Seq<TouchResult> Results) : IDetachedDocumentResult {
    public static TouchReceipt operator +(TouchReceipt left, TouchReceipt right) =>
        new(Results: left.Results + right.Results);
}
```

## [05]-[CUTS_AND_PIECES]

- Owner: `SectionCut` `[Union]` — the three extraction requests: `Profile` a planar section, `Slab` a thickness slice about a center plane, `Fill` the section-fill breps against resolved clipping planes; `ObjectPiece` — one detached extraction product: geometry under custody, attributes where the host pairs them.
- Law: extraction is read-shaped — `CreateSections`, `CreateSlices`, and `GetFillSurfaces` return detached geometry and never touch the table, so a cut inside a paused command pollutes no undo stack, and landing pieces in the document is the caller's `TableOp.Add` decision.
- Law: host-returned section, slice, and fill arrays are owned acquisitions. `ObjectPiece.Detach` duplicates both payloads, the source arrays release once after the fold, and a mid-fold failure disposes every accumulated piece before returning its refusal.
- Law: dissection rides the same product — `GetSubObjects` explodes the object into detached members the caller owns, so member geometry detaches onto handles with duplicated attributes and never re-enters as live state; the census consumer reads the piece roster, never a host array.
- Law: fill resolution demands live clipping planes — each requested id resolves through `FindId` to a `ClippingPlaneObject` inside the grant, and a non-plane id is a typed refusal, never a silent skip.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SectionCut {
    private SectionCut() { }

    private sealed record ProfileCase(Plane Plane, string Name, double Tolerance) : SectionCut;
    private sealed record SlabCase(Plane Center, string Name, double Thickness, double Tolerance) : SectionCut;
    private sealed record FillCase(Seq<Guid> ClippingPlanes, bool Unclipped) : SectionCut;

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

    public static Fin<SectionCut> Fill(Seq<Guid> clippingPlanes, bool unclipped = false) {
        Op op = Op.Of();
        return from ids in clippingPlanes.TraverseM(id => id != Guid.Empty
                   ? Fin.Succ(value: id)
                   : Fin.Fail<Guid>(error: op.InvalidInput())).As()
               from _ in guard(!ids.IsEmpty, op.InvalidInput()).ToFin()
               select (SectionCut)new FillCase(ClippingPlanes: ids.Distinct(), Unclipped: unclipped);
    }

    internal Fin<Seq<ObjectPiece>> Extract(RhinoDoc document, RhinoObject native, Op key) =>
        Switch(
            state: (Document: document, Native: native, Op: key),
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
                        rhinoObject: context.Native, clippingPlaneObjects: planes.AsIterable(), unclippedFills: cut.Unclipped),
                    attributes: null, key: context.Op))
                select pieces);

    private static Fin<Seq<ObjectPiece>> Paired(GeometryBase[]? geometry, ObjectAttributes[]? attributes, Op key) {
        Fin<Seq<ObjectPiece>> result =
            from shapes in Optional(geometry).ToFin(Fail: key.InvalidResult()).Map(static values => toSeq(values))
            from _ in guard(attributes is null || attributes.Length == shapes.Count, key.InvalidResult()).ToFin()
            from pieces in shapes.Map((shape, index) => (Shape: shape, Index: index)).Fold(
                Fin.Succ(value: Seq<ObjectPiece>()),
                (state, row) => state.Bind(held =>
                    ObjectPiece.Detach(
                            geometry: row.Shape,
                            attributes: Optional(attributes).Bind(paired => Optional(paired[row.Index])),
                            key: key)
                        .Map(piece => held.Add(value: piece))
                        .MapFail(error => {
                            _ = held.Iter(static prior => prior.Dispose());
                            return error;
                        })))
            select pieces;
        _ = Optional(geometry).Iter(static rows => {
            foreach (GeometryBase? shape in rows) { shape?.Dispose(); }
        });
        _ = Optional(attributes).Iter(static rows => {
            foreach (ObjectAttributes? metadata in rows) { metadata?.Dispose(); }
        });
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

    public void Dispose() {
        if (Interlocked.Exchange(location1: ref released, value: 1) is not 0) { return; }
        Geometry.Dispose();
        _ = Attributes.Iter(static value => value.Dispose());
    }
}
```

## [06]-[ASK_ENTRY]

- Owner: `StateAsk` `[Union]` — the read questions: `Snapshot`, `Frames`, `SelectedParts`, `HighlightedParts`, `Extent`, `Members`, `Cut`; `StateAnswer` `[Union]` — one typed result per question; `Objects` — the two entries: `Ask` the read dispatch, `Touch` the immediate component rail; the shared `Resolve` fold every sibling page's object window composes.
- Entry: `Objects.Ask(DocumentSession, TableTarget, StateAsk) : Fin<StateAnswer>` demands `SessionNeed.Read`; `Objects.Touch(DocumentSession, TableTarget, Touch) : Fin<TouchReceipt>` demands `SessionNeed.Mutate`; both resolve the target once and fold per object inside one grant window.
- Law: resolution is the table vocabulary — `TableTarget.Resolve` answers the id set and `FindId` lifts each to the live handle typed, so explicit ids, runtime pairs, and admitted queries address the object window identically; a deleted id is `MissingContext`, never a null propagated inward.
- Law: batch extent composes the host batch member — `Extent` runs the static `GetTightBoundingBox` over the whole resolved roster in one native call, with the plane overload selected by the ask's optional frame; a per-object union re-derived from single boxes is the deleted form.
- Law: answers embed identity — every per-object row carries the object guid beside its payload, so a plural ask's answer joins back to its targets without positional trust.
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
    public sealed record Extent(Option<Plane> Frame = default) : StateAsk;
    public sealed record Members : StateAsk;
    public sealed record Cut(SectionCut Section) : StateAsk;

    internal Fin<StateAsk> Admit(Op op) =>
        Switch(
            context: op,
            snapshot: static (_, ask) => Fin.Succ<StateAsk>(ask),
            frames: static (key, ask) => Optional(ask.Frame).ToFin(Fail: key.InvalidInput()).Map(_ => (StateAsk)ask),
            selectedParts: static (_, ask) => Fin.Succ<StateAsk>(ask),
            highlightedParts: static (_, ask) => Fin.Succ<StateAsk>(ask),
            extent: static (key, ask) => ask.Frame.Traverse(frame => key.AcceptInput(value: frame)).As()
                .Map(frame => (StateAsk)new Extent(Frame: frame)),
            members: static (_, ask) => Fin.Succ<StateAsk>(ask),
            cut: static (key, ask) => Optional(ask.Section).ToFin(Fail: key.InvalidInput()).Map(_ => (StateAsk)ask));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record StateAnswer : IDetachedDocumentResult {
    private StateAnswer() { }
    public sealed record States(Seq<ObjectSnapshot> Rows) : StateAnswer;
    public sealed record Posed(Seq<(Guid Id, Option<FramePose> Pose)> Rows) : StateAnswer;
    public sealed record PartRoster(Seq<(Guid Id, Seq<ComponentIndex> Components)> Rows) : StateAnswer;
    public sealed record Extent(BoundingBox Bounds) : StateAnswer;
    public sealed record Pieces(Seq<(Guid Id, Seq<ObjectPiece> Products)> Rows) : StateAnswer;
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class Objects {
    public static Fin<StateAnswer> Ask(DocumentSession session, TableTarget target, StateAsk ask) {
        Op op = Op.Of();
        return from active in Optional(ask).ToFin(Fail: op.InvalidInput()).Bind(value => value.Admit(op: op))
               from answer in session.Demand(
                   use: document =>
                       from natives in Resolve(document: document, target: target, key: op)
                       from folded in active.Switch(
                           context: (Document: document, Natives: natives, Op: op),
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
                           members: static (ctx, _) => ctx.Natives
                               .TraverseM(native =>
                                   from pieces in ctx.Op.Catch(() =>
                                       Optional(native.GetSubObjects()).ToFin(Fail: ctx.Op.InvalidResult())
                                           .Bind(parts => DetachMembers(members: parts, key: ctx.Op)))
                                   select (native.Id, pieces)).As()
                               .Map(static rows => (StateAnswer)new StateAnswer.Pieces(Rows: rows)),
                           cut: static (ctx, ask) => ctx.Natives
                               .TraverseM(native => ask.Section
                                   .Extract(document: ctx.Document, native: native, key: ctx.Op)
                                   .Map(products => (native.Id, products))).As()
                               .Map(static rows => (StateAnswer)new StateAnswer.Pieces(Rows: rows)))
                       select folded,
                   key: op,
                   needs: [SessionNeed.Read])
               select answer;
    }

    public static Fin<TouchReceipt> Touch(DocumentSession session, TableTarget target, Touch touch) {
        Op op = Op.Of();
        return from active in Optional(touch).ToFin(Fail: op.InvalidInput())
               from receipt in session.Demand(
                   use: document =>
                       from natives in Resolve(document: document, target: target, key: op)
                       from results in natives.TraverseM(native => active.Apply(native: native, key: op)).As()
                       select new TouchReceipt(Results: results),
                   key: op,
                   needs: [SessionNeed.Mutate])
               select receipt;
    }

    internal static Fin<Seq<RhinoObject>> Resolve(RhinoDoc document, TableTarget target, Op key) =>
        from address in Optional(target).ToFin(Fail: key.InvalidInput())
        from ids in address.Resolve(document: document, key: key)
        from natives in ids.TraverseM(id =>
            Optional(document.Objects.FindId(id)).ToFin(Fail: key.MissingContext())).As()
        select natives;

    private static Fin<Seq<ObjectPiece>> DetachMembers(RhinoObject[] members, Op key) {
        Fin<Seq<ObjectPiece>> result = toSeq(members).Fold(
            Fin.Succ(value: Seq<ObjectPiece>()),
            (state, member) => state.Bind(held =>
                (from geometry in Optional(member.Geometry).ToFin(Fail: key.InvalidResult())
                 from piece in ObjectPiece.Detach(
                     geometry: geometry,
                     attributes: Optional(member.Attributes),
                     key: key)
                 select piece)
                .Map(piece => held.Add(value: piece))
                .MapFail(error => {
                    _ = held.Iter(static prior => prior.Dispose());
                    return error;
                })));
        foreach (RhinoObject? member in members) { member?.Dispose(); }
        return result;
    }
}
```

## [07]-[SURFACE_LEDGER]

| [INDEX] | [CONCERN]           | [OWNER]          | [FORM]                                              | [ENTRY]                        |
| :-----: | :------------------ | :--------------- | :--------------------------------------------------- | :----------------------------- |
|  [01]   | native grades       | grade vocabularies | selection persistence and highlight scope separated  | snapshot / `TouchResult`       |
|  [02]   | object state        | `ObjectSnapshot` | one-pass read, host discriminants at the seam        | `StateAsk.Snapshot`            |
|  [03]   | frame reads         | `FrameAsk`       | object, gumball, and drag poses as one union         | `StateAsk.Frames`              |
|  [04]   | component reach     | `Reach`          | whole, part, parts, and every-part as one address    | `Reach.Of` / `Touch` payloads  |
|  [05]   | immediate touch     | `Touch`          | select and highlight verbs, table-rail split honored | `Objects.Touch`                |
|  [06]   | extraction custody  | `SectionCut`     | sections, slices, fills onto detached `ObjectPiece`  | `StateAsk.Cut` / `Members`     |
|  [07]   | read dispatch       | `StateAsk`       | one union, typed `StateAnswer` per case              | `Objects.Ask`                  |
|  [08]   | object resolution   | `Objects`        | `TableTarget` ids lifted to live handles per grant   | `Resolve(document, target)`    |
