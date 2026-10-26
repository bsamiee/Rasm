using Rasm.Rhino.Document.Tables;
using Rasm.Rhino.Document.Units;
using Rasm.Rhino.Modeling;
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
    public void Dispose() => DisposalOps.Release<IDisposable>(Geometry, Attributes).Run();
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
            o.IsHighlighted(checkSubObjects: false) is not 0,
            Conversions.Rows(o.GetHighlightedSubObjects()),
            o.ObjectFrame(),
            o.ObjectFrame(RhinoObject.ObjectFrameFlags.IncludeScaleTransforms),
            Callbacks.Found(o.TryGetGumballFrame(out GumballFrame repositioned), repositioned),
            Callbacks.Found(o.TryGetGumballFrameForCurrentAlignment(out GumballFrame aligned), aligned),
            Callbacks.Found(o.GetDynamicTransform(out Transform dragged), dragged),
            o.MemoryEstimate(),
            o.ShortDescriptionWithClosedStatus(prepend: false, plural: false, out int status),
            Some(status).Filter(static closed => closed is not 0).Map(static closed => closed is 2));

    public static IO<SubObjectState> ReadSubObject(RhinoObject o, ComponentIndex componentIndex) =>
        IO.lift(() => new SubObjectState(
            componentIndex,
            o.IsSubObjectSelected(componentIndex),
            o.IsSubObjectSelectable(componentIndex, ignoreSelectionState: false),
            o.IsSubObjectSelectable(componentIndex, ignoreSelectionState: true),
            o.IsSubObjectHighlighted(componentIndex)));

    public static IO<string> Description(RhinoObject o) =>
        DisposalOps.Within(static () => new TextLog(), log => IO.lift(() => { o.Description(log); return log.ToString(); }));

    // --- [SELECTION]
    public static IO<Seq<ObjectSelection>> SelectSubObjects(RhinoObject o, Seq<ComponentIndex> components, ObjectSelection selection, bool syncHighlight) =>
        IO.lift(() => Callbacks.Each(components, (component, index) =>
            (ObjectSelection)o.SelectSubObject(component, selection is not ObjectSelection.Unselected, syncHighlight, selection is ObjectSelection.Persistent) switch {
                var state => Refused.Unless(state == selection, state, nameof(RhinoObject.SelectSubObject), index),
            }));

    public static IO<Unit> HighlightSubObjects(RhinoObject o, Seq<ComponentIndex> components, bool highlight) =>
        IO.lift(() => Callbacks.Each(components, component => o.HighlightSubObject(component, highlight) == highlight, nameof(RhinoObject.HighlightSubObject)));

    // --- [CUTS]
    public static IO<Seq<Cut>> CreateCuts(Tolerances tolerances, RhinoObject o, Plane plane, string name, Option<double> thickness, IPlugInSink sink, Action<IDisposable> retain) =>
        from localized in IO.lift(() => RowText.Localize(name, sink).Local)
        from cuts in thickness.Match(
            Some: value => Copies.Acquire(nameof(RhinoObject.CreateSlices), _ => Cuts(o.CreateSlices(plane, localized, value, tolerances.Absolute, out ObjectAttributes[] rows), rows), retain),
            None: () => Copies.Acquire(nameof(RhinoObject.CreateSections), _ => Cuts(o.CreateSections(plane, localized, tolerances.Absolute, out ObjectAttributes[] rows), rows), retain))
        select cuts;

    private static Answer<Seq<Cut>> Cuts(GeometryBase[]? results, ObjectAttributes[]? rows) =>
        (Answer.Many(results), Answer.Many(rows)).Apply(static (kept, attributes) => kept.Zip(attributes, static (geometry, row) => new Cut(geometry, row))).As();

    // --- [REPLACEMENT]
    public static IO<TResult> ReplaceGeometry<T, TResult>(RhinoDoc doc, Guid id, Func<T, Action<IDisposable>, IO<(T Geometry, TResult Result)>> edit) where T : GeometryBase =>
        from resolved in ObjectTarget.Resolve<RhinoObject, T>(doc, id)
        from result in DisposalOps.Within(() => new ObjRef(resolved.Object), reference => DisposalOps.Within(
            (Action<IDisposable> held) => from target in Copies.Acquire(nameof(ObjRef.Geometry), _ => Answer.One(reference.Geometry() as T), held)
                                          from privated in IO.lift(target.EnsurePrivateCopy)
                                          from edited in edit(target, held)
                                          from landed in TableOps.Apply(doc, new TableOp.Replace(id, edited.Geometry, IgnoreModes: false))
                                          select edited.Result,
            DisposalOps.Release))
        select result;
}
