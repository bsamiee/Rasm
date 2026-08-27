# [RASM_RHINO_OBJECTS_STATE]

`Rasm.Rhino.Objects` owns object state. `StateAsk` closes snapshots, frames, transforms, component rosters, extents, pieces, sections, slices, and clipping fills. `Touch` owns selection and highlight mutation. Addresses compose `TableTarget`; answers detach from the session; `StateAnswer` owns pieces until its result-typed release. `ObjectSpine.Commit` carries undo through `DocumentCommit.Sealed`; immediate `Touch` opens no bracket. Attributes ride `TableOp.Amend`, mode rides `TableOp.State`, geometry rides `TableOp.Replace`; `RhinoObject.CommitChanges` has no path.

## [01]-[INDEX]

- [02]-[SNAPSHOT]: `SelectionGrade` and `HighlightGrade` closing the two host state contracts, `HighlightState`, `ObjectTrait`, `GripStance`, `SourceModel`, and the one-pass `ObjectSnapshot` read product.
- [03]-[FRAMES]: `FrameAsk`/`FramePose` — object frame, gumball frame, and drag-transform reads.
- [04]-[REACH_AND_TOUCH]: `Reach`, `Touch`, and the immediate component selection and highlight pipeline.
- [05]-[CUTS_AND_PIECES]: `SectionCut`, `ObjectPiece`, and the detached extraction custody.
- [06]-[ASK_ENTRY]: `StateAsk`/`StateAnswer`, `ObjectSpine`, the `DocumentCensus` analytics census, and the `Objects` entries.
- [07]-[SURFACE_LEDGER]: owner rows for every surface this page declares.

## [02]-[SNAPSHOT]

- Owner: `SelectionGrade` owns the verified unselected, selected, and persistent `IsSelected(checkSubObjects: true)` grades; `HighlightGrade` owns the host's own three-value `IsHighlighted(checkSubObjects: true)` contract and `HighlightState.Of` pairs it with the highlighted-component roster for snapshots, touch capture, and touch results; `ObjectTrait` is the condition vocabulary every combinable object fact rides; `GripStance` closes the grip pair as a ladder; `SourceModel` closes the three reference-provenance serials; `ObjectSnapshot` closes identity, lifecycle, source-model, description, closed-status, frame, grip, memory, and history-link evidence in one detached value.
- Law: the snapshot reads once per object inside the session grant — every field lands in one pass over the resolved handle, so a consumer never re-enters the document to complete a partial read, and the product is detached the moment `Ask` returns.
- Law: selection and highlight never share a vocabulary — two host contracts, two closed grades. `SelectionGrade` maps the verified `0`/`1`/`2` selection contract; `HighlightGrade` maps the host's documented `0` unhighlighted, `1` whole-object, `3` proper-sub-objects highlight contract, so rollback restores a captured grade rather than re-deriving meaning from a raw integer, and an unmapped host value is a typed refusal instead of a silent third state.
- Law: both whole-object grade writes answer the RESULTING state, not a change flag — the host returns "the object is now highlighted", so a highlight guard compares the return against the requested signal directly, while `SelectSubObject` returns a count the selection path discards before re-reading `IsSubObjectSelected`. Asymmetry is the host's, and each path reads its own member's contract.
- Law: `CommitChanges` never appears — the host member answers `true` only when a staged working copy flushed, and this package stages nothing on the live object: attribute writes travel `TableOp.Amend`, mode and visibility travel `TableOp.State`, geometry travels `TableOp.Replace`; the snapshot is the read face of that one-write-path law.
- Law: object mode is ONE row, not four predicates. `Normal`, `Hidden`, `Locked`, and `DefinitionGeometry` are four derived reads of the single host `Mode` word, so the snapshot carries the attributes page's `ObjectStance` and reads the word once; four bool columns re-derived it four times per object and admitted the corner — hidden AND locked at once — the word cannot hold. NAMED LOSS: `snapshot.Hidden` as a property; the WITNESS is `snapshot.Stance == ObjectStance.Hidden` at the census fold, where a mode tally is now one keyed reduction over the row instead of a filter per predicate.
- Law: every remaining combinable object fact rides ONE `CapabilitySet<ObjectTrait>` whose rows own their host reads — visibility, selectability, deletability, deletion, reference membership, solidity, picture-frame shape, an in-flight drag, a history record, and history survival across replace. NAMED LOSS: per-column compile-time exhaustiveness, bought back by the set's printable `Wire` on every snapshot and `AdmitsAll` at a consumer boundary demanding a trait; a new host predicate is one row and no consumer signature moves.
- Law: grips are a LADDER, not two bools — `GripsSelected` without `GripsOn` is a corner no live object holds, so `GripStance` admits the pair and refuses the impossible answer at the read instead of publishing it as state a consumer must re-screen.
- Law: reference provenance carries absence as absence — the host spells "not from a worksession, reference file, or linked definition" as serial `0` on three separate `uint` columns, so `SourceModel` projects each through `Option<uint>` and no consumer compares a magic serial.
- Law: history linkage is presence evidence — `ObjectTrait.HistoryRecord` carries `HasHistoryRecord()`, and every linkage read or mutation lives on the history page's `Chronicle`.
- Law: the snapshot carries no open discriminant. `ObjectType` decomposes through `ObjectKinds.OfMask` because a host row's type word is a MASK, `ActiveSpace` and `ObjectMaterialSource` read their spine and attribute owners, the layer index takes `ResourceIndex.Admit` and the material index takes `ResourceIndex.Maybe` — a live object always holds a layer, while the host's `-1` material spells the ordinary by-layer absence — and the closed-status integer takes the `ClosedStatus` rows — so no consumer re-derives a host contract from a bare number and an unmapped value refuses at the read.
- Growth: a new host object fact is one snapshot field read in the same pass, or one `ObjectTrait` row where the fact is a condition; a named native grade enters only after its values verify.
- Packages: Thinktecture.Runtime.Extensions (`libs/dotnet/.api/api-thinktecture-runtime-extensions.md` — `[SmartEnum<TKey>]`, `[ComplexValueObject]`, `[Union]`, `[ValidationError]`, `[UseDelegateFromConstructor]`, `[KeyMemberEqualityComparer<TAccessor, TKey>]`, `ComparerAccessors`); LanguageExt.Core (`api-languageext.md` — `Fin`, `Option`, `Seq`, `Traverse`/`TraverseM`, `Fold`, `BindFail`, `guard`); kernel `Domain/validation` (`ICapability`, `CapabilitySet`, `FactoryBridge.Accept`), `Domain/results` (`HostEdge.Text`, `Try.lift`), `Domain/context` (`Context`, `ToleranceLane`); `Document/session` (`DraftFault`, `DocumentSession`, `SessionNeed`, `session.Demand`), `Document/commit` (`DocumentCommit.Sealed`, `UndoSerial`, `RedrawPolicy`), `Document/tables` (`TableTarget`, `ResourceIndex`, `SelectionAxis`), `Document/layers` (`Layers.Ask`, `LayerTree`), `Document/geometry` (`GeometryCrossing`, `CrossingMode`, `GeometryHandle`), `Commands/selection` (`PartIndex`), `Blocks` (`BlockGraph`, `GraphSource`); RhinoCommon objects (`Rasm.Rhino/.api/api-rhinocommon-objects.md:35,57-62,81-91,138-144` — the mode word, the state and structural discriminants, selection and grip reads, the dynamic-transform and history probes) and `Rhino.UI.Gumball` (`api-rhino-ui.md` — `GumballFrame`, `GumballScaleMode`).

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Globalization;
using System.Linq;
using System.Threading;
using Rasm.Domain;
using Rasm.Rhino.Blocks;
using Rasm.Rhino.Commands;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.UI.Gumball;

namespace Rasm.Rhino.Objects;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class HighlightGrade {
    public static readonly HighlightGrade None = new(key: 0);
    public static readonly HighlightGrade Whole = new(key: 1);
    public static readonly HighlightGrade Parts = new(key: 3);
}

public sealed record HighlightState {
    private HighlightState(HighlightGrade grade, Seq<ComponentIndex> components) =>
        (Grade, Components) = (grade, components);

    public HighlightGrade Grade { get; }
    public Seq<ComponentIndex> Components { get; }

    internal static Fin<HighlightState> Of(RhinoObject native) =>
        FactoryBridge.Row<int, HighlightGrade>(native.IsHighlighted(checkSubObjects: true))
            .Map(grade => new HighlightState(
                grade: grade,
                components: Optional(native.GetHighlightedSubObjects())
                    .Map(static rows => toSeq(rows)).IfNone(Seq<ComponentIndex>())));
}

[SmartEnum<int>]
public sealed partial class SelectionGrade {
    public static readonly SelectionGrade None = new(key: 0);
    public static readonly SelectionGrade Selected = new(key: 1);
    public static readonly SelectionGrade Persistent = new(key: 2);
}

[SmartEnum<int>]
public sealed partial class ClosedStatus {
    public static readonly ClosedStatus NotApplicable = new(key: 0);
    public static readonly ClosedStatus Open = new(key: 1);
    public static readonly ClosedStatus Closed = new(key: 2);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ObjectTrait : ICapability<ObjectTrait> {
    public static readonly ObjectTrait Visible = new(key: "visible", held: static native => native.Visible);
    public static readonly ObjectTrait Selectable = new(key: "selectable", held: static native => native.IsSelectable());
    public static readonly ObjectTrait Deletable = new(key: "deletable", held: static native => native.IsDeletable);
    public static readonly ObjectTrait Deleted = new(key: "deleted", held: static native => native.IsDeleted);
    public static readonly ObjectTrait Reference = new(key: "reference", held: static native => native.IsReference);
    public static readonly ObjectTrait Solid = new(key: "solid", held: static native => native.IsSolid);
    public static readonly ObjectTrait PictureFrame = new(key: "picture-frame", held: static native => native.IsPictureFrame);
    public static readonly ObjectTrait Dragging = new(key: "dragging", held: static native => native.HasDynamicTransform);
    public static readonly ObjectTrait HistoryRecord = new(key: "history-record", held: static native => native.HasHistoryRecord());
    public static readonly ObjectTrait HistoryCopies = new(key: "history-copies", held: static native => native.CopyHistoryOnReplace());

    [UseDelegateFromConstructor]
    public partial bool Held(RhinoObject native);

    internal static CapabilitySet<ObjectTrait> Of(RhinoObject native) =>
        CapabilitySet<ObjectTrait>.Of([.. toSeq(Items).Filter(row => row.Held(native: native))]);
}

[SmartEnum<int>]
public sealed partial class GripStance {
    public static readonly GripStance Off = new(key: 0);
    public static readonly GripStance Shown = new(key: 1);
    public static readonly GripStance Selected = new(key: 2);

    internal static Fin<GripStance> Of(RhinoObject native) => (native.GripsOn, native.GripsSelected) switch {
        (false, false) => Fin.Succ(value: Off),
        (true, false) => Fin.Succ(value: Shown),
        (true, true) => Fin.Succ(value: Selected),
        _ => Fin.Fail<GripStance>(error: new KernelFault.InvalidResult(Detail: Some(nameof(GripStance)))),
    };
}

public readonly record struct SourceModel(Option<uint> Worksession, Option<uint> Reference, Option<uint> Definition) {
    internal static SourceModel Of(RhinoObject native) => new(
        Worksession: Present(serial: native.WorksessionReferenceSerialNumber),
        Reference: Present(serial: native.ReferenceModelSerialNumber),
        Definition: Present(serial: native.InstanceDefinitionModelSerialNumber));

    private static Option<uint> Present(uint serial) => serial is 0u ? Option<uint>.None : Some(serial);
}

public sealed record ObjectSnapshot(
    Guid Id,
    uint Serial,
    Option<string> Name,
    ObjectKinds Kind,
    ActiveSpaceUse Space,
    ResourceIndex Layer,
    Option<ResourceIndex> Material,
    MaterialOrigin MaterialSource,
    ObjectStance Stance,
    CapabilitySet<ObjectTrait> Traits,
    GripStance Grips,
    SourceModel Source,
    SelectionGrade Selection,
    HighlightState Highlight,
    uint MemoryBytes,
    string Description,
    string ClosedDescription,
    ClosedStatus ClosedStatus) : IDetachedDocumentResult {
    internal static Fin<ObjectSnapshot> Of(RhinoObject native) =>
        from grade in Try.lift(() => FactoryBridge.Row<int, SelectionGrade>(native.IsSelected(checkSubObjects: true))).Run().Bind(static inner => inner)
        from closed in Try.lift(() => Fin.Succ(value: (
            Text: native.ShortDescriptionWithClosedStatus(prepend: false, plural: false, status: out int status),
            Status: status))).Run().Bind(static inner => inner)
        from closure in FactoryBridge.Row<int, ClosedStatus>(closed.Status)
        from kind in ObjectKinds.OfMask(mask: native.ObjectType)
        from layer in ResourceIndex.Admit(value: native.Attributes.LayerIndex)
        let material = ResourceIndex.Maybe(value: native.Attributes.MaterialIndex)
        from highlight in Try.lift(() => HighlightState.Of(native: native)).Run().Bind(static inner => inner)
        from grips in Try.lift(() => GripStance.Of(native: native)).Run().Bind(static inner => inner)
        from snapshot in Try.lift(() => Fin.Succ(value: new ObjectSnapshot(
            Id: native.Id,
            Serial: native.RuntimeSerialNumber,
            Name: HostEdge.Text(native.Name),
            Kind: kind,
            Space: ActiveSpaceUse.Get(key: native.Attributes.Space),
            Layer: layer,
            Material: material,
            MaterialSource: MaterialOrigin.Get(key: native.Attributes.MaterialSource),
            Stance: ObjectStance.Get(key: native.Attributes.Mode),
            Traits: ObjectTrait.Of(native: native),
            Grips: grips,
            Source: SourceModel.Of(native: native),
            Selection: grade,
            Highlight: highlight,
            MemoryBytes: native.MemoryEstimate(),
            Description: native.ShortDescription(plural: false),
            ClosedDescription: closed.Text,
            ClosedStatus: closure))).Run().Bind(static inner => inner)
        select snapshot;
}
```

## [03]-[FRAMES]

- Owner: `FrameAsk` `[Union]` closes anchor, gumball, and drag questions; `GumballAlignment` owns each standard/current probe as row behavior; `FramePose` `[Union]` owns one typed pose per question.
- Law: frame reads are object-side only — `RhinoObject.ObjectFrame` reads, while both `RhinoObject.SetObjectFrame` overloads carry the host's own `[Obsolete("Use Attributes.SetObjectFrame instead")]` marking and forward to that member before committing changes, so every frame write is the attributes page's `Anchor` edit committed through the table pipeline's `Amend` and this page never mutates a frame.
- Law: an unset frame is absence — the anchor read always forces `RhinoObject.ObjectFrameFlags.ReturnUnset`, so an object carrying no explicit frame yields an invalid plane the fold projects to `None`; the request exposes only the `FrameScale` axis, whose two rows CARRY the host flag they contribute, so the read composes one flag union and never branches on a signal. Gumball probe failure projects to `None`, a drag probe answers `None` outside an active drag, and no consumer branches on `Plane.Unset`.
- Law: `ObjectSignal` states ENABLEMENT and nothing else. Scale inclusion on an anchor read and unclipped-fill inclusion on a section cut are inclusion axes, not enablement, so each carries its own two-row vocabulary — `FrameScale`, `FillSpan` — and the shared signal stays on the select and highlight verbs where turning a state on IS the question.
- Law: the gumball pose crosses detached — `GumballFrame` is a host struct whose `Plane`, `ScaleGripDistance`, and `ScaleMode` copy into the pose value, and `GumballScaleMode` rides the pose as a boundary discriminant.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum]
public sealed partial class GumballAlignment {
    public static readonly GumballAlignment Standard = new(read: static native =>
        native.TryGetGumballFrame(frame: out GumballFrame frame) ? Some(frame) : Option<GumballFrame>.None);
    public static readonly GumballAlignment Current = new(read: static native =>
        native.TryGetGumballFrameForCurrentAlignment(frame: out GumballFrame frame) ? Some(frame) : Option<GumballFrame>.None);

    [UseDelegateFromConstructor]
    internal partial Option<GumballFrame> Read(RhinoObject native);
}

[SmartEnum<bool>]
public sealed partial class FrameScale {
    public static readonly FrameScale Excluded = new(key: false, flags: RhinoObject.ObjectFrameFlags.Standard);
    public static readonly FrameScale Included = new(key: true, flags: RhinoObject.ObjectFrameFlags.IncludeScaleTransforms);

    internal RhinoObject.ObjectFrameFlags Flags { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FrameAsk {
    private FrameAsk() { }
    public sealed record Anchor(FrameScale Scale) : FrameAsk;
    public sealed record Gumball(GumballAlignment Alignment) : FrameAsk;
    public sealed record Drag : FrameAsk;

    internal Fin<FrameAsk> Admit() =>
        Switch(anchor: static (key, ask) => Admit.Need(ask.Scale).Map(_ => (FrameAsk)ask),
            gumball: static (key, ask) => Admit.Need(ask.Alignment)
                .Map(alignment => (FrameAsk)new Gumball(Alignment: alignment)),
            drag: static (_, ask) => Fin.Succ<FrameAsk>(ask));

    internal Option<FramePose> Read(RhinoObject native) =>
        Switch(
            native,
            anchor: static (live, ask) => Optional(live.ObjectFrame(
                    flags: RhinoObject.ObjectFrameFlags.ReturnUnset | ask.Scale.Flags))
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
- Law: the reach split closes the selection ownership — whole-object id-set selection is the table pipeline's `TableOp.Select`, so `Touch.Select` refuses `Whole` at the factory and owns component reach alone; `Touch.Highlight` owns every reach because the table pipeline carries no highlight member.
- Law: an all-parts sweep is directional — `EveryPart` with `ObjectSignal.Disabled` runs `UnselectAllSubObjects`/`UnhighlightAllSubObjects`, while `ObjectSignal.Enabled` is refused because no host member selects every component in one call.
- Law: touch is immediate visual state — no undo record opens, the entry demands `SessionNeed.Mutate` alone, and redraw stays caller policy. `Touch` preflights and captures every target before mutation, applies the batch fail-fast, and restores the complete captured roster through one accumulating compensation fold on refusal. Each mutation reads its final native grade before returning, so multi-component results never derive from a peak fold over per-call return values.
- Law: selection conduct is the table pipeline's own `CapabilitySet<SelectionAxis>` — the component select reads `SyncHighlight` and `Persistent` off the same set `TableOp.Select` reads, so a whole-object select and a component select cannot disagree on posture, and a page-local policy product naming the same bits twice is the deleted form.
- Growth: a new component verb is one `Touch` case dispatched in the same fold; a new reach shape is one `Reach` case every verb arm reads.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
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

    internal Fin<Reach> Admit() =>
        Switch(whole: static (_, reach) => Fin.Succ<Reach>(reach),
            part: static (_, reach) => Fin.Succ<Reach>(reach),
            parts: static (key, reach) => guard(!reach.Components.IsEmpty, new KernelFault.InvalidInput()).ToFin()
                .Map(_ => (Reach)new Parts(Components: reach.Components.Distinct())),
            everyPart: static (_, reach) => Fin.Succ<Reach>(reach));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Touch {
    private Touch() { }

    private sealed record SelectCase(Reach Scope, ObjectSignal Signal, CapabilitySet<SelectionAxis> Policy) : Touch;
    private sealed record HighlightCase(Reach Scope, ObjectSignal Signal) : Touch;
    private sealed record TouchState(
        RhinoObject Native,
        SelectionGrade Selection,
        Seq<ComponentIndex> Selected,
        HighlightState Highlight);

    public static Fin<Touch> Select(Reach scope, ObjectSignal signal, CapabilitySet<SelectionAxis> policy) {
        return from address in Admit.Need(scope).Bind(value => value.Admit())
               from state in Admit.Need(signal)
               from _ in guard(address is not Reach.Whole && !(address is Reach.EveryPart && state.On), new KernelFault.InvalidInput())
               select (Touch)new SelectCase(Scope: address, Signal: state, Policy: policy);
    }

    public static Fin<Touch> Highlight(Reach scope, ObjectSignal signal) {
        return from address in Admit.Need(scope).Bind(value => value.Admit())
               from state in Admit.Need(signal)
               from _ in guard(!(address is Reach.EveryPart && state.On), new KernelFault.InvalidInput())
               select (Touch)new HighlightCase(Scope: address, Signal: state);
    }

    internal Fin<Seq<TouchResult>> Transact(Seq<RhinoObject> natives) =>
        natives.TraverseM(native => Capture(native: native)).As()
            .Bind(states => ApplyCaptured(states: states));

    private Fin<TouchState> Capture(RhinoObject native) {
        Touch self = this;
        return Try.lift(() =>
            from selection in FactoryBridge.Row<int, SelectionGrade>(native.IsSelected(checkSubObjects: true))
    }

    private Fin<Seq<TouchResult>> ApplyCaptured(Seq<TouchState> states) {
        Fin<Seq<TouchResult>> primary = states
            .TraverseM(state => Apply(native: state.Native))
            .As();
        return primary.Rollback(release: () => Restore(states: states));
    }

    private Fin<TouchResult> Apply(RhinoObject native) =>
        Switch(
            native,
            selectCase: static (context, touch) => touch.Scope switch {
                Reach.EveryPart => Try.lift(() => {
                    _ = context.UnselectAllSubObjects();
                    return FactoryBridge.Row<int, SelectionGrade>(context.IsSelected(checkSubObjects: true))
                        .Map(grade => (TouchResult)new TouchResult.Selected(Id: context.Id, Grade: grade));
                }).Run().Bind(static inner => inner),
                var scoped => scoped.Roster.TraverseM(component => Try.lift(() => {
                        _ = context.SelectSubObject(
                            componentIndex: component,
                            select: touch.Signal.On,
                            syncHighlight: touch.Policy.Admits(capability: SelectionAxis.SyncHighlight),
                            persistentSelect: touch.Policy.Admits(capability: SelectionAxis.Persistent));
                        return guard(
                            context.IsSubObjectSelected(componentIndex: component) == touch.Signal.On,
                            new KernelFault.InvalidResult()).ToFin();
                    }).Run().Bind(static inner => inner)).As()
                    .Bind(_ => FactoryBridge.Row<int, SelectionGrade>(context.IsSelected(checkSubObjects: true)))
                    .Map(grade => (TouchResult)new TouchResult.Selected(Id: context.Id, Grade: grade)),
            },
            highlightCase: static (context, touch) => touch.Scope switch {
                Reach.Whole => Try.lift(() => guard(
                        context.Highlight(enable: touch.Signal.On) == touch.Signal.On,
                        new KernelFault.InvalidResult()).ToFin()).Run().Bind(static inner => inner)
                    .Bind(_ => Highlighted(native: context)),
                Reach.EveryPart => Try.lift(() => Fin.Succ(value: context.UnhighlightAllSubObjects())).Run().Bind(static inner => inner)
                    .Bind(_ => Highlighted(native: context)),
                var scoped => scoped.Roster.TraverseM(component => Try.lift(() => guard(
                        context.HighlightSubObject(componentIndex: component, highlight: touch.Signal.On) == touch.Signal.On,
                        new KernelFault.InvalidResult()).ToFin()).Run().Bind(static inner => inner)).As()
                    .Bind(_ => Highlighted(native: context)),
            });

    private static Fin<TouchResult> Highlighted(RhinoObject native) =>
        HighlightState.Of(native: native)
            .Map(state => (TouchResult)new TouchResult.Highlighted(Id: native.Id, State: state));

    private static Fin<Unit> Restore(Seq<TouchState> states) =>
        states.Traverse(state => Restore(state: state).ToValidation()).As()
            .ToFin()
            .Map(static _ => unit);

    private static Fin<Unit> Restore(TouchState state) {
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
                    new KernelFault.InvalidResult()).ToFin();
            })),
            () => {
                _ = state.Native.UnhighlightAllSubObjects();
                return Fin.Succ(value: unit);
            },
            .. state.Highlight.Components.Map(component => (Func<Fin<Unit>>)(() =>
                guard(
                    state.Native.HighlightSubObject(componentIndex: component, highlight: true),
                    new KernelFault.InvalidResult()).ToFin()
            )),
            () => guard(
                state.Native.Highlight(enable: state.Highlight.Grade == HighlightGrade.Whole)
                    == (state.Highlight.Grade == HighlightGrade.Whole),
                new KernelFault.InvalidResult()).ToFin(),
        ];
        return steps.Traverse(step => Try.lift(step).Run().Bind(static inner => inner).ToValidation())
            .As()
            .ToFin()
            .Map(static _ => unit);
    }
}

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum]
public sealed partial class ObjectSignal {
    public static readonly ObjectSignal Disabled = new(on: false);
    public static readonly ObjectSignal Enabled = new(on: true);

    internal bool On { get; }

    internal static ObjectSignal Of(bool on) => on ? Enabled : Disabled;
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
- Law: plural extraction is transactional custody. `ObjectPiece.Custody` mints in order and releases the complete accumulated prefix on the first refusal; `DetachAll` and `Acquire` are its two instantiations. Successful piece rosters expose `Fin<Unit> Release()` so disposer failures remain on the result; `IDisposable` and `ignore` cleanup are deleted forms.
- Law: the section gate is the model's own tolerance lane, never a caller argument. `CreateSections` and `CreateSlices` take a model distance, so `Extract` reads `ToleranceLane.PlaneDistance` off the kernel `Context` the document mints, and a coarser or finer cut is that context's override row — the authority a per-call double bypassed. NAMED LOSS: the per-call tolerance argument; the WITNESS is `Context.Override`, which states the lane once for every operation a session runs instead of per call site.
- Law: dissection rides the same product — `GetSubObjects` explodes the object into detached members the caller owns, so member geometry detaches onto handles with duplicated attributes and never re-enters as live state; the census consumer reads the piece roster, never a host array.
- Law: fill resolution demands live clipping planes — each requested id resolves through `FindId` to a `ClippingPlaneObject` inside the grant, and a non-plane id is a typed refusal, never a silent skip.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<bool>]
public sealed partial class FillSpan {
    public static readonly FillSpan Clipped = new(key: false);
    public static readonly FillSpan Unclipped = new(key: true);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SectionCut {
    private SectionCut() { }

    private sealed record ProfileCase(Plane Plane, string Name) : SectionCut;
    private sealed record SlabCase(Plane Center, string Name, double Thickness) : SectionCut;
    private sealed record FillCase(Seq<Guid> ClippingPlanes, FillSpan Fills) : SectionCut;

    public static Fin<SectionCut> Profile(Plane plane, string name) {
        return from frame in Acceptance.Input(value: plane)
               from label in Acceptance.Text(value: name)
               select (SectionCut)new ProfileCase(Plane: frame, Name: label);
    }

    public static Fin<SectionCut> Slab(Plane center, string name, double thickness) {
        return from frame in Acceptance.Input(value: center)
               from label in Acceptance.Text(value: name)
               from depth in Admit.Positive(value: thickness)
               select (SectionCut)new SlabCase(Center: frame, Name: label, Thickness: depth);
    }

    public static Fin<SectionCut> Fill(Seq<Guid> clippingPlanes, FillSpan fills) {
        return from ids in clippingPlanes.TraverseM(id => id != Guid.Empty
                   ? Fin.Succ(value: id)
                   : Fin.Fail<Guid>(error: new KernelFault.InvalidInput())).As()
               from span in Admit.Need(fills)
               from _ in guard(!ids.IsEmpty, new KernelFault.InvalidInput())
               select (SectionCut)new FillCase(ClippingPlanes: ids.Distinct(), Fills: span);
    }

    internal Fin<Seq<ObjectPiece>> Extract(RhinoDoc document, RhinoObject native, Context domain) =>
        Switch(
            (Document: document, Native: native, Gate: domain.For(lane: ToleranceLane.PlaneDistance).Value),
            profileCase: static (context, cut) => Try.lift(() => ObjectPiece.Paired(
                geometry: context.Native.CreateSections(
                    plane: cut.Plane, name: cut.Name, tolerance: context.Gate,
                    objectAttributes: out ObjectAttributes[] attributes),
                attributes: attributes)).Run().Bind(static inner => inner),
            slabCase: static (context, cut) => Try.lift(() => ObjectPiece.Paired(
                geometry: context.Native.CreateSlices(
                    centerPlane: cut.Center, name: cut.Name, thickness: cut.Thickness, tolerance: context.Gate,
                    objectAttributes: out ObjectAttributes[] attributes),
                attributes: attributes)).Run().Bind(static inner => inner),
            fillCase: static (context, cut) =>
                from planes in cut.ClippingPlanes.TraverseM(id =>
                    Optional(context.Document.Objects.FindId(id)).ToFin(Fail: new KernelFault.MissingContext())
                        .Bind(found => Admit.Need(found as ClippingPlaneObject))).As()
                from pieces in Try.lift(() => ObjectPiece.Paired(
                    geometry: RhinoObject.GetFillSurfaces(
                        rhinoObject: context.Native, clippingPlaneObjects: planes.AsIterable(), unclippedFills: cut.Fills.Key),
                    attributes: null)).Run().Bind(static inner => inner)
                select pieces);

}

// --- [MODELS] --------------------------------------------------------------------------
public sealed class ObjectPiece {

    internal static Fin<Seq<ObjectPiece>> Paired(GeometryBase[]? geometry, ObjectAttributes[]? attributes) {
        Fin<Seq<ObjectPiece>> result =
            from shapes in Optional(geometry).ToFin(Fail: new KernelFault.InvalidResult()).Map(static values => toSeq(values).Strict())
            from _ in guard(attributes is null || attributes.Length == shapes.Count, new KernelFault.InvalidResult())
            from pieces in ObjectPiece.DetachAll(
                rows: shapes.Map((shape, index) => (shape, Optional(attributes).Bind(paired => Optional(paired[index])))))
            select pieces;
        return result.Settled(
            held: Seq((Geometry: geometry, Attributes: attributes)),
            release: row => ObjectPiece.Release(geometry: row.Geometry, attributes: row.Attributes));
    }
    private int released;

    private ObjectPiece(GeometryHandle geometry, Option<ObjectAttributes> attributes) {
        Geometry = geometry;
        Attributes = attributes;
    }

    public GeometryHandle Geometry { get; }
    public Option<ObjectAttributes> Attributes { get; }

    internal static Fin<ObjectPiece> Detach(GeometryBase geometry, Option<ObjectAttributes> attributes) =>
        from handle in GeometryCrossing.Cross(source: geometry, mode: CrossingMode.Detach)
        from metadata in Try.lift(() => Fin.Succ(value: attributes.Map(static value => value.Duplicate()))).Run().Bind(static inner => inner)
            .Rollback(handle)
        select new ObjectPiece(geometry: handle, attributes: metadata);

    private static Fin<Seq<TProduct>> Custody<TSource, TProduct>(
        Seq<TSource> rows, Func<TSource, Fin<TProduct>> mint, Func<TProduct, Fin<Unit>> release) =>
        rows.Fold(
            Fin.Succ(value: Seq<TProduct>()),
            (state, row) => state.Bind(held => mint(arg: row)
                .Map(held.Add)
                .Rollback(held: held, release: release)));

    internal static Fin<Seq<ObjectPiece>> DetachAll(
        Seq<(GeometryBase Shape, Option<ObjectAttributes> Attributes)> rows) =>
        Custody(
            rows: rows,
            mint: row => Detach(geometry: row.Shape, attributes: row.Attributes),
            release: piece => piece.Release());

    internal static Fin<Seq<(Guid Id, Seq<ObjectPiece> Products)>> Acquire(
        Seq<RhinoObject> natives,
        Func<RhinoObject, Fin<Seq<ObjectPiece>>> detach) =>
        Custody(
            rows: natives,
            mint: native => detach(arg: native).Map(products => (native.Id, products)),
            release: row => Custody.Release(
                held: row.Products,
                release: piece => piece.Release()));

    internal static Fin<Unit> Release(GeometryBase[]? geometry, ObjectAttributes[]? attributes) =>
        Custody.Release(
            releases: Seq<Func<Fin<Unit>>>(
                () => Custody.Release(
                    held: Optional(attributes).Map(static rows => toSeq(rows).Choose(static row => Optional(row))).IfNone(Seq<ObjectAttributes>()),
                    release: metadata => Try.lift(() => Fin.Succ(value: HostEdge.Side(metadata.Dispose))).Run().Bind(static inner => inner)),
                () => Custody.Release(
                    held: Optional(geometry).Map(static rows => toSeq(rows).Choose(static row => Optional(row))).IfNone(Seq<GeometryBase>()),
                    release: shape => Try.lift(() => Fin.Succ(value: HostEdge.Side(shape.Dispose))).Run().Bind(static inner => inner))));

    internal static Fin<Unit> Release(Seq<ObjectPiece> pieces) =>
        Custody.Release(held: pieces, release: piece => piece.Release());

    internal static Fin<Unit> Release(Seq<(Guid Id, Seq<ObjectPiece> Products)> rows) =>
        Release(pieces: rows.Bind(static row => row.Products));

    internal static Fin<Unit> Release(Seq<(Guid Id, ObjectPiece Product)> rows) =>
        Release(pieces: rows.Map(static row => row.Product));

    public Fin<Unit> Release() {
        if (Interlocked.Exchange(location1: ref released, value: 1) is not 0) { return Fin.Succ(unit); }
        return Custody.Release(
            releases: Seq<Func<Fin<Unit>>>(
                () => Try.lift(() => Fin.Succ(value: HostEdge.Side(Geometry.Dispose))).Run().Bind(static inner => inner),
                () => Attributes.TraverseM(value => Try.lift(() => Fin.Succ(HostEdge.Side(value.Dispose))).Run().Bind(static inner => inner))
                    .As().Map(static _ => unit)));
    }

}
```

## [06]-[ASK_ENTRY]

- Owner: `StateAsk` `[Union]` closes snapshot, frame, component-roster, targeted part-state, extent, member, and cut reads; `StateAnswer` `[Union]` owns the corresponding detached products; `ObjectSpine` is the result-generic commit entry over one demand and the Document spine's `DocumentCommit.Sealed`; `Objects.Ask` and `Objects.Touch` are the polymorphic read and immediate-state entries; `Objects.Resolve` is the shared one-hop object window.
- Entry: `Objects.Ask(DocumentSession, TableTarget, StateAsk) : Fin<StateAnswer>` demands `SessionNeed.Read`; `Objects.Touch(DocumentSession, TableTarget, Touch) : Fin<Seq<TouchResult>>` demands `SessionNeed.Mutate` — immediate touch opens no bracket, so no undo column rides its answer; both resolve the target once and fold per object inside one grant window. `Objects.Census(DocumentSession, TableTarget) : Fin<DocumentCensus>` opens one outer read demand and composes the object fold, layers tree, and block topology through their own entries while that pinned grant remains active.
- Law: the spine is the one undo-recorded bracket owner for the namespace — light, material, and history commits supply their canonical result fold to `ObjectSpine.Commit`; immediate visual `Objects.Touch` remains outside the bracket by contract.
- Law: resolution is the table vocabulary — `TableTarget.Resolve` answers the id set and `FindId` lifts each to the live handle typed, so explicit ids, runtime pairs, and admitted queries address the object window identically; a deleted id is `MissingContext`, never a null propagated inward.
- Law: batch extent composes the host batch member — `Extent` runs the static `GetTightBoundingBox` over the whole resolved roster in one native call, with the plane overload selected by the ask's optional frame; a per-object union re-derived from single boxes is the deleted form.
- Law: answers embed identity — every per-object row carries the object guid beside its payload, and `PartState` admits its own `PartIndex` from the raw host component at its one factory; component eligibility records both current-state and ignore-selection answers, so `IsSubObjectSelectable(ComponentIndex, bool)` keeps its host boolean at the boundary instead of exporting a request knob. `PartState` takes that name because `Document/events` declares a `ComponentState` of its own and this plane imports that namespace — the local declaration shadowed it silently, and two unrelated types under one name in one scope is a defect no compiler reports.
- Boundary: visual-analysis attachment — `EnableVisualAnalysisMode`, its active-mode queries, and the `AnalysisModeChanged` static event — is the display page's analysis-mode extension; this window carries no analysis case and composes that boundary's outcomes where an ask needs the fact.
- Owner: `DocumentCensus` is the one analytics-ready document census — object counts by kind, space, and mode, the annotation tally, memory total, layer-tree shape from `Layers.Ask`, per-layer and per-material usage histograms with material-source distribution from the attribute anchors, block-closure metrics from `BlockGraph.Ask` (definition count, placement count with completeness evidence, cycle groups), and on-disk archive size from the document path — every dimension detached, so the analytics egress lands one stable shape into the data plane and no consumer walks live tables.
- Law: the census composes owners, never re-measures — one outer `DocumentSession.Demand` retains the session gate and host stack from the first `Objects.Ask` through `Layers.Ask` and every `BlockGraph.Ask`. Re-entrancy is the session owner's declared contract, not this page's assumption: `Demand` is re-entrant on the demanding thread through its reentrant lock and demand-depth counter, each nesting proving its own grants against its own fresh snapshot, so every nested owner read re-enters the same pinned document grant and no sibling session operation can interleave. Object rows come from the canonical snapshot window, the layer dimension is the `LayerTree` the layers page already mints — its `Count` AND its `Depth`, both measured at that mint off the topological order the tree proved, never re-walked here — the block dimension is three `BlockGraphAsk` questions over `GraphSource.Live`, and every histogram, mode tally included, folds through the one-pass `CountBy` keyed reduction rather than a filter per named value. Archive extent opens the candidate once and reads length from that handle; an unsaved or concurrently removed path projects absence, while directory and access refusals stay failures. the app root's observe tap writes the `rasm.rhino.document.census.*` rows `Document/events#TELEMETRY_TAP` declares off this census.
- Law: the census builds the block topology ONCE. Three separate `GraphSource.Live` values made the graph owner rebuild the same definition graph three times over one pinned grant, and the three answers disagree whenever a definition moves between builds — a census reporting a placement count from one topology beside a cycle count from another. One source value, three questions.
- Growth: a new read is one ask case with its answer case; a new census dimension is one `DocumentCensus` field folded from an existing owner; the dispatch, the entries, and every consumer read it with zero new surface.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
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

    internal Fin<StateAsk> Admit() =>
        Switch(snapshot: static (_, ask) => Fin.Succ<StateAsk>(ask),
            frames: static (key, ask) => Admit.Need(ask.Frame)
                .Bind(frame => frame.Admit())
                .Map(frame => (StateAsk)new Frames(Frame: frame)),
            selectedParts: static (_, ask) => Fin.Succ<StateAsk>(ask),
            highlightedParts: static (_, ask) => Fin.Succ<StateAsk>(ask),
            components: static (key, ask) => Admit.Need(ask.Scope)
                .Bind(scope => scope.Admit())
                .Bind(scope => guard(scope is Reach.Part or Reach.Parts, new KernelFault.InvalidInput()).ToFin().Map(_ => (StateAsk)new Components(scope))),
            extent: static (key, ask) => ask.Frame.Traverse(frame => Acceptance.Input(value: frame)).As()
                .Map(frame => (StateAsk)new Extent(Frame: frame)),
            members: static (_, ask) => Fin.Succ<StateAsk>(ask),
            cut: static (key, ask) => Admit.Need(ask.Section).Map(_ => (StateAsk)ask));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record StateAnswer : IDetachedDocumentResult {
    private StateAnswer() { }
    public sealed record States(Seq<ObjectSnapshot> Rows) : StateAnswer;
    public sealed record Posed(Seq<(Guid Id, Option<FramePose> Pose)> Rows) : StateAnswer;
    public sealed record PartRoster(Seq<(Guid Id, Seq<ComponentIndex> Components)> Rows) : StateAnswer;
    public sealed record PartStates(Seq<PartState> Rows) : StateAnswer;
    public sealed record Extent(BoundingBox Bounds) : StateAnswer;
    public sealed record Members(Seq<(Guid Id, Seq<ObjectPiece> Products)> Rows) : StateAnswer;
    public sealed record Sections(Seq<(Guid Id, Seq<ObjectPiece> Products)> Rows) : StateAnswer;

    public Fin<Unit> Release() =>
        Switch(states: static (_, _) => Fin.Succ(unit),
            posed: static (_, _) => Fin.Succ(unit),
            partRoster: static (_, _) => Fin.Succ(unit),
            partStates: static (_, _) => Fin.Succ(unit),
            extent: static (_, _) => Fin.Succ(unit),
            members: static (key, answer) => ObjectPiece.Release(answer.Rows),
            sections: static (key, answer) => ObjectPiece.Release(answer.Rows));
}

[ComplexValueObject]
[ValidationError]
public sealed partial class PartState {
    public Guid Id { get; }
    public PartIndex Component { get; }
    public bool Selected { get; }
    public bool Selectable { get; }
    public bool SelectableIgnoringSelection { get; }
    public bool Highlighted { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Guid id,
        ref PartIndex component,
        ref bool selected,
        ref bool selectable,
        ref bool selectableIgnoringSelection,
        ref bool highlighted) {
        Guid owner = id;
        validationError = FactoryValidation.Of(FactoryValidation.Violated(
                (owner == Guid.Empty, () => new ValidationClause(string.Join(" | ", new object?[] { op, nameof(Id) })))));
    }

    internal static Fin<PartState> Of(
        Guid id,
        ComponentIndex component,
        bool selected,
        bool selectable,
        bool selectableIgnoringSelection,
        bool highlighted) =>
        from part in FactoryBridge.Accept<PartIndex, ComponentIndex>(candidate: component)
        from row in FactoryBridge.Accept<PartState>(
            fault: Validate(
                id, part, selected, selectable, selectableIgnoringSelection, highlighted,
                out PartState? admitted),
            admitted: admitted)
        select row;
}

[ComplexValueObject]
[ValidationError]
public sealed partial class ArchiveExtent {
    public DocumentPath Path { get; }
    public long Bytes { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref DocumentPath path,
        ref long bytes) {
        (DocumentPath Path, long Bytes) extent = (path, bytes);
        validationError = FactoryValidation.Of(FactoryValidation.Violated(
                (extent.Path == default, () => new ValidationClause(string.Join(" | ", new object?[] { op, nameof(Path) }))),
                (extent.Bytes < 0L, () => new ValidationClause(string.Join(" | ", new object?[] { op, nameof(Bytes), extent.Bytes, "a non-negative byte count" })))));
    }
}

public sealed record DocumentCensus(
    Seq<(ObjectKinds Kind, int Count)> Kinds,
    Seq<(ActiveSpaceUse Space, int Count)> Spaces,
    Seq<(ObjectStance Stance, int Count)> Stances,
    int Annotations,
    ulong MemoryBytes,
    int LayerCount,
    int LayerDepth,
    Seq<(ResourceIndex Layer, int Count)> LayerUsage,
    Seq<(ResourceIndex Material, int Count)> MaterialUsage,
    Seq<(MaterialOrigin Source, int Count)> MaterialSources,
    int BlockDefinitions,
    int BlockPlacements,
    bool BlockEvidenceComplete,
    int BlockCycleGroups,
    Option<ArchiveExtent> Archive) : IDetachedDocumentResult;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Objects {
    public static Fin<StateAnswer> Ask(DocumentSession session, TableTarget target, StateAsk ask) {
        return from active in Admit.Need(ask).Bind(value => value.Admit())
               from answer in session.Demand(
                   use: document =>
                       from natives in Resolve(document: document, target: target)
                       from folded in active.Switch(
                           (Document: document, Natives: natives),
                           snapshot: static (ctx, _) => ctx.Natives
                               .TraverseM(native => ObjectSnapshot.Of(native: native)).As()
                               .Map(static rows => (StateAnswer)new StateAnswer.States(Rows: rows)),
                           frames: static (ctx, ask) => ctx.Natives
                               .TraverseM(native => Try.lift(() =>
                                   Fin.Succ(value: (native.Id, ask.Frame.Read(native: native)))).Run().Bind(static inner => inner)).As()
                               .Map(static rows => (StateAnswer)new StateAnswer.Posed(Rows: rows)),
                           selectedParts: static (ctx, _) => ctx.Natives
                               .TraverseM(native => Try.lift(() => Fin.Succ(value: (
                                   native.Id,
                                   Optional(native.GetSelectedSubObjects())
                                       .Map(static rows => toSeq(rows))
                                       .IfNone(Seq<ComponentIndex>())))).Run().Bind(static inner => inner)).As()
                               .Map(static rows => (StateAnswer)new StateAnswer.PartRoster(Rows: rows)),
                           highlightedParts: static (ctx, _) => ctx.Natives
                               .TraverseM(native => Try.lift(() => Fin.Succ(value: (
                                   native.Id,
                                   Optional(native.GetHighlightedSubObjects())
                                       .Map(static rows => toSeq(rows))
                                       .IfNone(Seq<ComponentIndex>())))).Run().Bind(static inner => inner)).As()
                               .Map(static rows => (StateAnswer)new StateAnswer.PartRoster(Rows: rows)),
                           components: static (ctx, ask) => ctx.Natives
                               .Bind(native => ask.Scope.Roster.Map(component => (Native: native, Component: component)))
                               .TraverseM(row => Try.lift(() => PartState.Of(
                                   id: row.Native.Id,
                                   component: row.Component,
                                   selected: row.Native.IsSubObjectSelected(componentIndex: row.Component),
                                   selectable: row.Native.IsSubObjectSelectable(
                                       componentIndex: row.Component, ignoreSelectionState: false),
                                   selectableIgnoringSelection: row.Native.IsSubObjectSelectable(
                                       componentIndex: row.Component, ignoreSelectionState: true),
                                   highlighted: row.Native.IsSubObjectHighlighted(componentIndex: row.Component))).Run().Bind(static inner => inner)).As()
                               .Map(static rows => (StateAnswer)new StateAnswer.PartStates(Rows: rows)),
                           extent: static (ctx, ask) => Try.lift(() => {
                               BoundingBox bounds = BoundingBox.Unset;
                               bool answered = ask.Frame.Case switch {
                                   Plane frame => RhinoObject.GetTightBoundingBox(
                                       rhinoObjects: ctx.Natives.AsIterable(), plane: frame, boundingBox: out bounds),
                                   _ => RhinoObject.GetTightBoundingBox(
                                       rhinoObjects: ctx.Natives.AsIterable(), boundingBox: out bounds),
                               };
                               return answered && bounds.IsValid
                                   ? Fin.Succ(value: (StateAnswer)new StateAnswer.Extent(Bounds: bounds))
                                   : Fin.Fail<StateAnswer>(error: new KernelFault.InvalidResult());
                           }).Run().Bind(static inner => inner),
                           members: static (ctx, _) => ObjectPiece.Acquire(
                               natives: ctx.Natives,
                               detach: native => Try.lift(() =>
                                   Optional(native.GetSubObjects()).ToFin(Fail: new KernelFault.InvalidResult())
                                       .Bind(parts => DetachMembers(members: parts))).Run().Bind(static inner => inner))
                               .Map(static rows => (StateAnswer)new StateAnswer.Members(Rows: rows)),
                           cut: static (ctx, ask) =>
                               from domain in Rasm.Domain.Context.Of(doc: ctx.Document).ToFin()
                               from rows in ObjectPiece.Acquire(
                                   natives: ctx.Natives,
                                   detach: native => ask.Section.Extract(
                                       document: ctx.Document, native: native, domain: domain))
                               select (StateAnswer)new StateAnswer.Sections(Rows: rows))
                       select folded,
                   needs: [SessionNeed.Read])
               select answer;
    }

    public static Fin<Seq<TouchResult>> Touch(DocumentSession session, TableTarget target, Touch touch) {
        return from active in Admit.Need(touch)
               from results in session.Demand(
                   use: document =>
                       from natives in Resolve(document: document, target: target)
                       from folded in active.Transact(natives: natives)
                       select folded,
                   needs: [SessionNeed.Mutate])
               select results;
    }

    public static Fin<DocumentCensus> Census(DocumentSession session, TableTarget target) {
        return session.Demand(
            use: document => CensusPinned(session: session, target: target, document: document),
            needs: [SessionNeed.Read]);
    }

    private static Fin<DocumentCensus> CensusPinned(
        DocumentSession session,
        TableTarget target,
        RhinoDoc document) =>
        from answer in Ask(session: session, target: target, ask: new StateAsk.Snapshot())
               from usage in answer is StateAnswer.States states
                   ? Fin.Succ(value: states.Rows.Strict())
                   : Fin.Fail<Seq<ObjectSnapshot>>(error: new KernelFault.InvalidResult())
               from path in Try.lift(() => Fin.Succ(value: HostEdge.Text(document.Path))).Run().Bind(static inner => inner)
               from tree in Layers.Ask(session: session)
               let topology = new GraphSource.Live(Session: session)
               from definitions in BlockGraph.Ask(source: topology, question: new BlockGraphAsk.Definitions())
                   .Bind(answer => answer is BlockGraphAnswer.Nodes nodes
                       ? Fin.Succ(value: nodes.Values.Count)
                       : Fin.Fail<int>(error: new KernelFault.InvalidResult()))
               from placed in BlockGraph.Ask(source: topology, question: new BlockGraphAsk.Placed())
                   .Bind(answer => answer is BlockGraphAnswer.Placements placements
                       ? Fin.Succ(value: (Count: placements.Values.Count, Complete: placements.Evidence.IsComplete))
                       : Fin.Fail<(int, bool)>(error: new KernelFault.InvalidResult()))
               from cycles in BlockGraph.Ask(source: topology, question: new BlockGraphAsk.Condensation())
                   .Bind(answer => answer is BlockGraphAnswer.Condensed condensed
                       ? Fin.Succ(value: condensed.Components.Filter(static component => component.Count > 1).Count)
                       : Fin.Fail<int>(error: new KernelFault.InvalidResult()))
               from archive in path
                   .TraverseM(value => OpenArchive(path: value))
                   .As()
                   .Map(static opened => opened.Bind(static extent => extent))
               select new DocumentCensus(
                   Kinds: toSeq(usage.AsEnumerable().CountBy(static row => row.Kind)
                       .Select(static pair => (pair.Key, pair.Value))),
                   Spaces: toSeq(usage.AsEnumerable().CountBy(static row => row.Space)
                       .Select(static pair => (pair.Key, pair.Value))),
                   Stances: toSeq(usage.AsEnumerable().CountBy(static row => row.Stance)
                       .Select(static pair => (pair.Key, pair.Value))),
                   Annotations: usage.Filter(static row => row.Kind.Values.Contains(ObjectKind.Annotation)).Count,
                   MemoryBytes: usage.Fold(0UL, static (sum, row) => sum + row.MemoryBytes),
                   LayerCount: tree.Count,
                   LayerDepth: tree.Depth,
                   LayerUsage: toSeq(usage.AsEnumerable().CountBy(static row => row.Layer)
                       .Select(static pair => (pair.Key, pair.Value))),
                   MaterialUsage: toSeq(usage.AsEnumerable().CountBy(static row => row.Material)
                       .Select(static pair => (pair.Key, pair.Value))),
                   MaterialSources: toSeq(usage.AsEnumerable().CountBy(static row => row.MaterialSource)
                       .Select(static pair => (pair.Key, pair.Value))),
                   BlockDefinitions: definitions,
                   BlockPlacements: placed.Count,
                   BlockEvidenceComplete: placed.Complete,
                   BlockCycleGroups: cycles,
                   Archive: archive);

    private static Fin<Option<ArchiveExtent>> OpenArchive(string path) =>
        Try.lift(() => from admitted in DocumentPath.Of(value: path)
                       from extent in Fin.Succ(value: OpenLength(path: path))
                       select Some(ArchiveExtent.Create(path: admitted, bytes: extent))).Run().Bind(static inner => inner)
            .BindFail(static error =>
            error.Exception.Case is System.IO.FileNotFoundException or System.IO.DirectoryNotFoundException
                ? Fin.Succ(Option<ArchiveExtent>.None)
                : Fin.Fail<Option<ArchiveExtent>>(error));

    private static long OpenLength(string path) {
        using Microsoft.Win32.SafeHandles.SafeFileHandle handle = System.IO.File.OpenHandle(path: path);
        return System.IO.RandomAccess.GetLength(handle: handle);
    }

    internal static Fin<Seq<RhinoObject>> Resolve(RhinoDoc document, TableTarget target) =>
        from address in Admit.Need(target)
        from ids in address.Resolve(document: document)
        from natives in ids.TraverseM(id =>
            Optional(document.Objects.FindId(id)).ToFin(Fail: new KernelFault.MissingContext())).As()
        select natives;

    private static Fin<Seq<ObjectPiece>> DetachMembers(RhinoObject[] members) {
        Fin<Seq<ObjectPiece>> result = toSeq(members)
            .TraverseM(member => Optional(member.Geometry).ToFin(Fail: new KernelFault.InvalidResult())
                .Map(geometry => (geometry, Optional(member.Attributes)))).As()
            .Bind(rows => ObjectPiece.DetachAll(rows: rows));
        return result.Settled(
            held: toSeq(members),
            release: member => Try.lift(() => Fin.Succ(value: HostEdge.SideWhen(member is not null, member.Dispose))).Run().Bind(static inner => inner));
    }
}

internal static class ObjectSpine {
    internal static Fin<TResult> Commit<TResult>(
        DocumentSession session, string name, RedrawPolicy redraw,
        Func<RhinoDoc, Fin<TResult>> run, bool recordsUndo = true) =>
        session.Demand(
            use: document => DocumentCommit.Sealed(
                document: document,
                name: name,
                recordsUndo: recordsUndo,
                redraw: redraw,
                run: () => run(document),
                project: Fin.Succ),
            needs: SessionNeed.Mutation(undo: recordsUndo, redraw: redraw).ToArray());
}
```

## [07]-[SURFACE_LEDGER]

| [INDEX] | [CONCERN]          | [OWNER]          | [FORM]                                                   | [ENTRY]                       |
| :-----: | :----------------- | :--------------- | :------------------------------------------------------- | :---------------------------- |
|  [01]   | native evidence    | snapshot/results | typed selection grade and raw highlight scope            | snapshot / `TouchResult`      |
|  [02]   | object state       | `ObjectSnapshot` | one-pass read, host discriminants at the boundary        | `StateAsk.Snapshot`           |
|  [03]   | object conditions  | `ObjectTrait`    | one held set, each row owning its host read              | `snapshot.Traits.Admits`      |
|  [04]   | frame reads        | `FrameAsk`       | object, gumball, and drag poses as one union             | `StateAsk.Frames`             |
|  [05]   | component reach    | `Reach`          | whole, part, parts, and every-part as one address        | `Reach.Of` / `Touch` payloads |
|  [06]   | immediate touch    | `Touch`          | select and highlight verbs, table-pipeline split honored | `Objects.Touch`               |
|  [07]   | extraction custody | `SectionCut`     | sections, slices, fills onto detached `ObjectPiece`      | `StateAsk.Cut` / `Members`    |
|  [08]   | custody fold       | `ObjectPiece`    | one prefix-releasing fold under two instantiations       | `DetachAll` / `Acquire`       |
|  [09]   | native release     | `ObjectPiece`    | host-array and product-roster disposal                   | `Release`                     |
|  [10]   | read dispatch      | `StateAsk`       | typed answer union                                       | `Objects.Ask`                 |
|  [11]   | object resolution  | `Objects`        | target-to-handle lift                                    | `Resolve(document, target)`   |
|  [12]   | commit kernel      | `ObjectSpine`    | result-generic sealed document commit                    | `Commit<TResult>`             |
|  [13]   | analytics census   | `DocumentCensus` | detached multi-owner document census                     | `Objects.Census`              |

## [08]-[RESEARCH]

(none)
