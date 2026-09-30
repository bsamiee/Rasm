using Rasm.Rhino.Document;
using Rhino;
using Rhino.DocObjects;
using Rhino.UI.Gumball;

namespace Rasm.Rhino.Objects;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ObjectState(
    Guid Id,
    uint RuntimeSerialNumber,
    ObjectType ObjectType,
    AttributeState Attributes,
    bool Visible,
    bool IsSelectable,
    bool IsDeletable,
    bool IsDeleted,
    bool IsReference,
    bool IsSolid,
    bool IsPictureFrame,
    bool IsInstanceDefinitionGeometry,
    bool HasDynamicTransform,
    bool HasHistoryRecord,
    bool CopyHistoryOnReplace,
    bool GripsOn,
    bool GripsSelected,
    Option<uint> ReferenceModelSerialNumber,
    Option<uint> InstanceDefinitionModelSerialNumber,
    int IsSelected,
    int IsHighlighted,
    Seq<ComponentIndex> SelectedSubObjects,
    Seq<ComponentIndex> HighlightedSubObjects,
    uint MemoryEstimate,
    string ShortDescriptionWithClosedStatus,
    int ClosedStatus);

public sealed record ComponentState(ComponentIndex Component, bool Selected, bool Selectable, bool SelectableIgnoringSelection, bool Highlighted);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SelectionOp {
    public sealed record SelectComponents(Seq<ComponentIndex> Components, bool On, bool SyncHighlight, bool Persistent) : SelectionOp;

    public sealed record UnselectComponents() : SelectionOp;

    public sealed record Highlight(bool On) : SelectionOp;

    public sealed record HighlightComponents(Seq<ComponentIndex> Components, bool On) : SelectionOp;

    public sealed record UnhighlightComponents() : SelectionOp;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PieceSource {
    public sealed record Sections(Plane Plane, string Name, double Tolerance) : PieceSource;

    public sealed record Slices(Plane Center, string Name, double Thickness, double Tolerance) : PieceSource;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class RhinoObjects {
    // --- [READS]
    public static IO<ObjectState> ReadState(RhinoObject o) =>
        from attributes in AttributeOps.ReadAttributes(o.Attributes)
        select new ObjectState(
            o.Id,
            o.RuntimeSerialNumber,
            o.ObjectType,
            attributes,
            o.Visible,
            o.IsSelectable(),
            o.IsDeletable,
            o.IsDeleted,
            o.IsReference,
            o.IsSolid,
            o.IsPictureFrame,
            o.IsInstanceDefinitionGeometry,
            o.HasDynamicTransform,
            o.HasHistoryRecord(),
            o.CopyHistoryOnReplace(),
            o.GripsOn,
            o.GripsSelected,
            Answers.Present(o.ReferenceModelSerialNumber),
            Answers.Present(o.InstanceDefinitionModelSerialNumber),
            o.IsSelected(checkSubObjects: true),
            o.IsHighlighted(checkSubObjects: true),
            toSeq(o.GetSelectedSubObjects()),
            toSeq(o.GetHighlightedSubObjects()),
            o.MemoryEstimate(),
            o.ShortDescriptionWithClosedStatus(prepend: false, plural: false, out int status),
            status);

    public static IO<Option<Plane>> ReadFrame(RhinoObject o, bool includeScale) =>
        IO.lift(() => Some(o.ObjectFrame(RhinoObject.ObjectFrameFlags.ReturnUnset | (includeScale ? RhinoObject.ObjectFrameFlags.IncludeScaleTransforms : RhinoObject.ObjectFrameFlags.Standard)))
            .Filter(static plane => plane.IsValid));

    public static IO<Option<GumballFrame>> ReadGumball(RhinoObject o, bool currentAlignment) =>
        IO.lift(() => currentAlignment
            ? Answers.Found(o.TryGetGumballFrameForCurrentAlignment(out GumballFrame aligned), aligned)
            : Answers.Found(o.TryGetGumballFrame(out GumballFrame repositioned), repositioned));

    public static IO<Option<Transform>> ReadDynamicTransform(RhinoObject o) =>
        IO.lift(() => Answers.Found(o.GetDynamicTransform(out Transform xform), xform));

    public static IO<ComponentState> ReadComponent(RhinoObject o, ComponentIndex c) =>
        IO.lift(() => new ComponentState(
            c,
            o.IsSubObjectSelected(c),
            o.IsSubObjectSelectable(c, ignoreSelectionState: false),
            o.IsSubObjectSelectable(c, ignoreSelectionState: true),
            o.IsSubObjectHighlighted(c)));

    // --- [SELECTION]
    public static IO<Unit> ApplySelectionOp(RhinoObject o, SelectionOp op) =>
        IO.lift(() => op.Switch(
            o,
            selectComponents: static (target, select) => Answers.Each(
                select.Components,
                component => select.On == (target.SelectSubObject(component, select.On, select.SyncHighlight, select.Persistent) != 0),
                nameof(RhinoObject.SelectSubObject)),
            unselectComponents: static (target, _) => ignore(target.UnselectAllSubObjects()),
            highlight: static (target, highlight) => Refused.Unless(target.Highlight(highlight.On) == highlight.On, nameof(RhinoObject.Highlight)),
            highlightComponents: static (target, highlight) => Answers.Each(
                highlight.Components,
                component => target.HighlightSubObject(component, highlight.On) == highlight.On,
                nameof(RhinoObject.HighlightSubObject)),
            unhighlightComponents: static (target, _) => ignore(target.UnhighlightAllSubObjects())));

    // --- [BOUNDS]
    public static IO<BoundingBox> TightBoundingBox(Seq<RhinoObject> objects, Option<Plane> plane) =>
        from populated in IO.lift(() => Invalid.Unless(!objects.IsEmpty, nameof(objects)))
        from bounds in plane.Match(
            Some: aligned =>
                from framed in IO.lift(() => Invalid.Unless(aligned.IsValid, nameof(plane)))
                from measured in IO.lift(() => Measured(RhinoObject.GetTightBoundingBox(objects, aligned, out BoundingBox box), box))
                select measured,
            None: () => IO.lift(() => Measured(RhinoObject.GetTightBoundingBox(objects, out BoundingBox box), box)))
        select bounds;

    private static Fin<BoundingBox> Measured(bool answered, BoundingBox box) =>
        from accepted in Refused.Unless(answered, nameof(RhinoObject.GetTightBoundingBox))
        from valid in Invalid.Unless(box.IsValid, nameof(box))
        select box;

    // --- [PIECES]
    public static IO<TValue> WithPieces<TValue>(RhinoObject o, PieceSource source, Func<Seq<GeometryPair>, IO<TValue>> body) =>
        DisposalOps.Using(
            IO.lift(() => source.Switch(
                o,
                sections: static (target, sections) => (
                    toSeq(target.CreateSections(sections.Plane, sections.Name, sections.Tolerance, out ObjectAttributes[] attributes)),
                    toSeq(attributes)),
                slices: static (target, slices) => (
                    toSeq(target.CreateSlices(slices.Center, slices.Name, slices.Thickness, slices.Tolerance, out ObjectAttributes[] attributes)),
                    toSeq(attributes)))),
            source.Map(sections: nameof(RhinoObject.CreateSections), slices: nameof(RhinoObject.CreateSlices)),
            pieces => body(pieces.Map(static piece => new GeometryPair(piece.Left, Some(piece.Right)))));

    public static IO<TValue> WithSubObjects<TValue>(RhinoObject o, Func<Seq<RhinoObject>, IO<TValue>> body) =>
        DisposalOps.Using(IO.lift(() => toSeq(o.GetSubObjects())), body);

    public static IO<TValue> WithFills<TValue>(RhinoObject o, Seq<ClippingPlaneObject> planes, bool unclipped, Func<Seq<Brep>, IO<TValue>> body) =>
        from clipped in IO.lift(() => Invalid.Unless(!planes.IsEmpty, nameof(planes)))
        from value in DisposalOps.Using(
            IO.lift(() => Missing.Unless(RhinoObject.GetFillSurfaces(o, planes, unclipped), nameof(RhinoObject.GetFillSurfaces)).Map(static breps => toSeq(breps))),
            body)
        select value;

    // --- [REPLACEMENT]
    public static IO<Unit> ReplaceGeometry<T>(RhinoDoc doc, Guid id, Func<T, IO<Unit>> edit) where T : GeometryBase =>
        from resolved in Queries.Resolve<RhinoObject, T>(doc, id)
        from replaced in GeometryOps.WithGeometry(resolved.Geometry, copy =>
            from edited in edit(copy)
            from ids in TableOps.Apply(doc, new TableOp.Replace(id, copy, IgnoreModes: false))
            select unit)
        select replaced;
}
