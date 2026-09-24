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

    public sealed record SubObjects() : PieceSource;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class RhinoObjects {
    // --- [READS]
    public static IO<ObjectState> ReadState(RhinoObject o) =>
        from attributes in AttributeOps.ReadAttributes(o.Attributes)
        from state in IO.lift(() => new ObjectState(
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
            status))
        select state;

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
    public static IO<int> ApplySelectionOp(RhinoObject o, SelectionOp op) =>
        op.Switch(
            o,
            selectComponents: static (target, select) => select.Components
                .TraverseM(component => IO.lift(() =>
                    Mismatch.Unless(select.On == (target.SelectSubObject(component, select.On, select.SyncHighlight, select.Persistent) != 0), nameof(RhinoObject.SelectSubObject))))
                .As()
                .Map(static touched => touched.Count),
            unselectComponents: static (target, _) => IO.lift(target.UnselectAllSubObjects),
            highlight: static (target, highlight) =>
                IO.lift(() => Mismatch.Unless(target.Highlight(highlight.On) == highlight.On, nameof(RhinoObject.Highlight)).Map(static _ => 1)),
            highlightComponents: static (target, highlight) => highlight.Components
                .TraverseM(component => IO.lift(() => Mismatch.Unless(target.HighlightSubObject(component, highlight.On) == highlight.On, nameof(RhinoObject.HighlightSubObject))))
                .As()
                .Map(static touched => touched.Count),
            unhighlightComponents: static (target, _) => IO.lift(target.UnhighlightAllSubObjects));

    // --- [BOUNDS]
    public static IO<BoundingBox> TightBoundingBox(Seq<RhinoObject> objects, Option<Plane> plane) =>
        from populated in IO.lift(() => Invalid.Unless(!objects.IsEmpty, nameof(RhinoObject.GetTightBoundingBox)))
        from bounds in plane.Match(
            Some: aligned =>
                from framed in IO.lift(() => Invalid.Unless(aligned.IsValid, nameof(Plane.IsValid)))
                from measured in IO.lift(() => Measured(RhinoObject.GetTightBoundingBox(objects, aligned, out BoundingBox box), box))
                select measured,
            None: () => IO.lift(() => Measured(RhinoObject.GetTightBoundingBox(objects, out BoundingBox box), box)))
        select bounds;

    private static Fin<BoundingBox> Measured(bool answered, BoundingBox box) =>
        from accepted in Refused.Unless(answered, nameof(RhinoObject.GetTightBoundingBox))
        from valid in Invalid.Unless(box.IsValid, nameof(BoundingBox.IsValid))
        select box;

    // --- [PIECES]
    public static IO<TValue> WithPieces<TValue>(RhinoObject o, PieceSource source, Func<Seq<GeometryPair>, IO<TValue>> body) =>
        from pieces in IO.lift(() => Pieces(o, source))
        from value in Disposal.Using(IO.pure(pieces.Owned), _ =>
            from pairs in IO.lift(pieces.Pairs)
            from value in body(pairs)
            select value)
        select value;

    public static IO<TValue> WithFills<TValue>(RhinoObject o, Seq<ClippingPlaneObject> planes, bool unclipped, Func<Seq<Brep>, IO<TValue>> body) =>
        from clipped in IO.lift(() => Invalid.Unless(!planes.IsEmpty, nameof(RhinoObject.GetFillSurfaces)))
        from value in Disposal.Using(
            IO.lift(() => Missing.Unless(RhinoObject.GetFillSurfaces(o, planes, unclipped), nameof(RhinoObject.GetFillSurfaces)).Map(static breps => toSeq(breps))),
            body)
        select value;

    private static (Seq<IDisposable> Owned, Fin<Seq<GeometryPair>> Pairs) Pieces(RhinoObject o, PieceSource source) =>
        source.Switch(
            o,
            sections: static (target, sections) =>
                Paired(target.CreateSections(sections.Plane, sections.Name, sections.Tolerance, out ObjectAttributes[] attributes), attributes, nameof(RhinoObject.CreateSections)),
            slices: static (target, slices) =>
                Paired(target.CreateSlices(slices.Center, slices.Name, slices.Thickness, slices.Tolerance, out ObjectAttributes[] attributes), attributes, nameof(RhinoObject.CreateSlices)),
            subObjects: static (target, _) => Optional(target.GetSubObjects()).Map(static rows => toSeq(rows)).Match<(Seq<IDisposable> Owned, Fin<Seq<GeometryPair>> Pairs)>(
                Some: static members => (
                    members.Map<IDisposable>(static member => member),
                    members.TraverseM(static member =>
                        from geometry in Missing.Unless(member.Geometry, nameof(RhinoObject.Geometry))
                        select new GeometryPair(geometry, Some(member.Attributes))).As()),
                None: static () => (Seq<IDisposable>(), new Refused(nameof(RhinoObject.GetSubObjects)))));

    private static (Seq<IDisposable> Owned, Fin<Seq<GeometryPair>> Pairs) Paired(GeometryBase[] geometry, ObjectAttributes[] attributes, string member) =>
        (toSeq(geometry.Concat<IDisposable>(attributes)),
         Mismatch.Unless(geometry.Length == attributes.Length, member)
             .Map(_ => toSeq(geometry).Zip(toSeq(attributes), static (piece, held) => new GeometryPair(piece, Some(held)))));

    // --- [REPLACEMENT]
    public static IO<Unit> ReplaceGeometry<T>(RhinoDoc doc, Guid id, Func<T, IO<Unit>> edit) where T : GeometryBase =>
        from resolved in Queries.Resolve<RhinoObject, T>(doc, id)
        from replaced in GeometryOps.WithGeometry(resolved.Geometry, DuplicateMode.Duplicate, copy =>
            from edited in edit(copy)
            from ids in TableOps.Apply(doc, new TableOp.Replace(id, copy, IgnoreModes: false))
            select unit)
        select replaced;
}
