using Rasm.Rhino.Document.Notation;
using Rasm.Rhino.Document.Shapes;
using Rasm.Rhino.Document.Tables;
using Rhino;
using Rhino.DocObjects;
using Rhino.FileIO;
using Rhino.UI.Gumball;

namespace Rasm.Rhino.Objects;

// --- [TYPES] ---------------------------------------------------------------------------
public enum ObjectSelection { Unselected = 0, Selected = 1, Persistent = 2 }

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
    bool HasHistoryRecord,
    bool CopyHistoryOnReplace,
    bool GripsOn,
    bool GripsSelected,
    Option<uint> ReferenceModelSerialNumber,
    Option<uint> InstanceDefinitionModelSerialNumber,
    ObjectSelection IsSelected,
    Seq<ComponentIndex> SelectedSubObjects,
    bool IsHighlighted,
    Seq<ComponentIndex> HighlightedSubObjects,
    Plane ObjectFrame,
    Plane ScaledObjectFrame,
    Option<GumballFrame> GumballFrame,
    Option<GumballFrame> GumballFrameForCurrentAlignment,
    Option<Transform> DynamicTransform,
    uint MemoryEstimate,
    string ShortDescription,
    Option<bool> IsClosed);

public sealed record SubObjectState(ComponentIndex ComponentIndex, bool IsSelected, bool IsSelectable, bool IsSelectableIgnoringSelection, bool IsHighlighted);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SelectionOp {
    public sealed record SelectSubObjects(Seq<ComponentIndex> Components, bool Select, bool SyncHighlight, bool PersistentSelect) : SelectionOp;

    public sealed record UnselectAllSubObjects() : SelectionOp;

    public sealed record Highlight(bool Enable) : SelectionOp;

    public sealed record HighlightSubObjects(Seq<ComponentIndex> Components, bool Highlight) : SelectionOp;

    public sealed record UnhighlightAllSubObjects() : SelectionOp;

    public IO<Unit> Apply(RhinoObject target) =>
        IO.lift(() => Switch<RhinoObject, Fin<Unit>>(
            target,
            selectSubObjects: static (o, select) => Callbacks.Each(
                select.Components,
                component => (o.SelectSubObject(component, select.Select, select.SyncHighlight, select.PersistentSelect) != 0) == select.Select,
                nameof(RhinoObject.SelectSubObject)),
            unselectAllSubObjects: static (o, _) => ignore(o.UnselectAllSubObjects()),
            highlight: static (o, highlight) => Refused.Unless(o.Highlight(highlight.Enable) == highlight.Enable, nameof(RhinoObject.Highlight)),
            highlightSubObjects: static (o, highlight) => Callbacks.Each(
                highlight.Components,
                component => o.HighlightSubObject(component, highlight.Highlight) == highlight.Highlight,
                nameof(RhinoObject.HighlightSubObject)),
            unhighlightAllSubObjects: static (o, _) => ignore(o.UnhighlightAllSubObjects())));
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed record Cut(GeometryBase Geometry, ObjectAttributes Attributes) : IDisposable {
    public GeometryPair Pair => new(Geometry, Some(Attributes));

    public void Dispose() {
        Geometry.Dispose();
        Attributes.Dispose();
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class RhinoObjects {
    // --- [READS]
    public static IO<ObjectState> ReadState(RhinoObject o) =>
        from attributes in IO.lift(() => o.Attributes).Bind(static held => AttributeOps.Read(held))
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
            o.HasHistoryRecord(),
            o.CopyHistoryOnReplace(),
            o.GripsOn,
            o.GripsSelected,
            Conversions.Present(o.ReferenceModelSerialNumber),
            Conversions.Present(o.InstanceDefinitionModelSerialNumber),
            (ObjectSelection)o.IsSelected(checkSubObjects: false),
            Conversions.Rows(o.GetSelectedSubObjects()),
            o.IsHighlighted(checkSubObjects: false) != 0,
            Conversions.Rows(o.GetHighlightedSubObjects()),
            o.ObjectFrame(RhinoObject.ObjectFrameFlags.Standard),
            o.ObjectFrame(RhinoObject.ObjectFrameFlags.IncludeScaleTransforms),
            Callbacks.Found(o.TryGetGumballFrame(out GumballFrame repositioned), repositioned),
            Callbacks.Found(o.TryGetGumballFrameForCurrentAlignment(out GumballFrame aligned), aligned),
            Callbacks.Found(o.GetDynamicTransform(out Transform dragged), dragged),
            o.MemoryEstimate(),
            o.ShortDescriptionWithClosedStatus(prepend: false, plural: false, out int status),
            Some(status).Filter(static closed => closed != 0).Map(static closed => closed == 2));

    public static IO<SubObjectState> ReadSubObject(RhinoObject o, ComponentIndex componentIndex) =>
        IO.lift(() => new SubObjectState(
            componentIndex,
            o.IsSubObjectSelected(componentIndex),
            o.IsSubObjectSelectable(componentIndex, ignoreSelectionState: false),
            o.IsSubObjectSelectable(componentIndex, ignoreSelectionState: true),
            o.IsSubObjectHighlighted(componentIndex)));

    public static IO<string> Description(RhinoObject o) =>
        (from log in use(static () => new TextLog())
         from written in IO.lift(() => o.Description(log))
         select log.ToString()).Bracket();

    // --- [CUTS]
    public static IO<Seq<Cut>> CreateSections(RhinoObject o, Plane plane, string name, Tolerances tolerances, IPlugInSink sink) =>
        Cuts(() => (o.CreateSections(plane, RowText.Localize(name, table: Some<object>(sink)).Local, tolerances.Absolute, out ObjectAttributes[] rows), rows), nameof(RhinoObject.CreateSections));

    public static IO<Seq<Cut>> CreateSlices(RhinoObject o, Plane centerPlane, string name, double thickness, Tolerances tolerances, IPlugInSink sink) =>
        Cuts(() => (o.CreateSlices(centerPlane, RowText.Localize(name, table: Some<object>(sink)).Local, thickness, tolerances.Absolute, out ObjectAttributes[] rows), rows), nameof(RhinoObject.CreateSlices));

    private static IO<Seq<Cut>> Cuts(Func<(GeometryBase[] Results, ObjectAttributes[] Rows)> create, string member) =>
        Copies.Owned(
            IO.lift(create),
            answer => Measurements.Valid(toSeq<GeometryBase?>(answer.Results), member).Map(kept => kept.Zip(toSeq(answer.Rows), static (geometry, row) => new Cut(geometry, row))),
            static answer => DisposalOps.Release(Conversions.Rows<IDisposable>([.. answer.Results, .. answer.Rows])));

    // --- [REPLACEMENT]
    public static IO<Unit> ReplaceGeometry<T>(RhinoDoc doc, Guid id, Func<T, IO<Unit>> edit) where T : GeometryBase =>
        from resolved in ObjectTarget.Resolve<RhinoObject, T>(doc, id)
        from replaced in (
            from copy in use(Copies.DuplicateShallow(resolved.Geometry))
            from edited in edit(copy)
            from landed in TableOps.Apply(doc, new TableOp.Replace(id, copy, IgnoreModes: false))
            select unit).Bracket()
        select replaced;
}
