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

// --- [SERVICES] ------------------------------------------------------------------------
public sealed record Cut(GeometryBase Geometry, ObjectAttributes Attributes) : IDisposable {
    public void Dispose() {
        Geometry.Dispose();
        Attributes.Dispose();
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class RhinoObjects {
    // --- [READS]
    public static IO<ObjectState> ReadState(RhinoObject o) =>
        from attributes in IO.lift(() => o.Attributes).Bind(AttributeOps.Read)
        select new ObjectState(
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

    // --- [SELECTION]
    public static IO<Seq<ObjectSelection>> SelectSubObjects(RhinoObject o, Seq<ComponentIndex> components, ObjectSelection selection, bool syncHighlight) =>
        IO.lift(() => Callbacks.Each(components, (component, index) =>
            (ObjectSelection)o.SelectSubObject(component, selection != ObjectSelection.Unselected, syncHighlight, selection == ObjectSelection.Persistent) switch {
                var state => RefusedElement.Unless(state == selection, state, nameof(RhinoObject.SelectSubObject), index),
            }));

    public static IO<Unit> HighlightSubObjects(RhinoObject o, Seq<ComponentIndex> components, bool highlight) =>
        IO.lift(() => Callbacks.Each(components, component => o.HighlightSubObject(component, highlight) == highlight, nameof(RhinoObject.HighlightSubObject)));

    // --- [CUTS]
    public static IO<Seq<Cut>> CreateCuts(RhinoObject o, Plane plane, string name, Option<double> thickness, Tolerances tolerances, IPlugInSink sink) =>
        from localized in IO.lift(() => RowText.Localize(name, table: Some<object>(sink)).Local)
        from answer in Copies.Owned(
            IO.lift(() => thickness.Match(
                Some: value => (Results: o.CreateSlices(plane, localized, value, tolerances.Absolute, out ObjectAttributes[] rows), Rows: rows, Member: nameof(RhinoObject.CreateSlices)),
                None: () => (Results: o.CreateSections(plane, localized, tolerances.Absolute, out ObjectAttributes[] rows), Rows: rows, Member: nameof(RhinoObject.CreateSections)))),
            static made => Measurements.Valid(toSeq<GeometryBase?>(made.Results), made.Member).Map(kept => (Geometry: kept, Attributes: toSeq(made.Rows))),
            static made => DisposalOps.Release(Conversions.Rows<IDisposable>([.. made.Results, .. made.Rows])))
        select answer.Geometry.Zip(answer.Attributes, static (geometry, attributes) => new Cut(geometry, attributes));

    // --- [REPLACEMENT]
    public static IO<TResult> ReplaceGeometry<T, TResult>(RhinoDoc doc, Guid id, Func<T, IO<TResult>> edit) where T : GeometryBase =>
        (from resolved in ObjectTarget.Resolve<RhinoObject, T>(doc, id)
         from copy in use(Copies.DuplicateShallow(resolved.Geometry))
         from edited in edit(copy)
         from landed in TableOps.Apply(doc, new TableOp.Replace(id, copy, IgnoreModes: false))
         select edited).Bracket();
}
