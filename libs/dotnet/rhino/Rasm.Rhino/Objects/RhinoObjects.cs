using System.Runtime.CompilerServices;
using Rasm.Rhino.Document.Tables;
using Rasm.Rhino.Document.Units;
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
public sealed record Cut(GeometryBase Geometry, ObjectAttributes Attributes, object Owner) : IDisposable {
    public void Dispose() => _ = Owner.Run(IO.lift(Geometry.Dispose) >> IO.lift(Attributes.Dispose));
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
            Conversions.Found(o.TryGetGumballFrame(out GumballFrame repositioned), repositioned),
            Conversions.Found(o.TryGetGumballFrameForCurrentAlignment(out GumballFrame aligned), aligned),
            Conversions.Found(o.GetDynamicTransform(out Transform dragged), dragged),
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
        (from log in use(static () => new TextLog())
         from described in IO.lift(() => {
             o.Description(log);
             return log.ToString();
         })
         select described).Bracket();

    // --- [SELECTION]
    public static IO<Seq<ObjectSelection>> SelectSubObjects(RhinoObject o, Seq<ComponentIndex> components, ObjectSelection selection, bool syncHighlight) =>
        IO.lift(() => Callbacks.Each(components, (component, index) =>
            (ObjectSelection)o.SelectSubObject(component, selection is not ObjectSelection.Unselected, syncHighlight, selection is ObjectSelection.Persistent) switch {
                var state => RhinoFault.Unless(state == selection, index).Map(_ => state),
            }));

    public static IO<Unit> HighlightSubObjects(RhinoObject o, Seq<ComponentIndex> components, bool highlight) =>
        IO.lift(() => Callbacks.Each(components, (component, index) => RhinoFault.Unless(o.HighlightSubObject(component, highlight) == highlight, index))).Map(unit);

    // --- [CUTS]
    public static IO<Seq<Cut>> CreateCuts(Tolerances tolerances, RhinoObject o, Plane plane, string name, Option<double> thickness, object owner) =>
        from localized in IO.lift(() => RowText.Localize(name, owner).Local)
        from cuts in IO.lift(() => thickness.Match(
            Some: value => Cuts(o.CreateSlices(plane, localized, value, tolerances.Absolute, out ObjectAttributes[] rows), rows, owner),
            None: () => Cuts(o.CreateSections(plane, localized, tolerances.Absolute, out ObjectAttributes[] rows), rows, owner)))
        from owned in cuts.TraverseM(static cut => use(() => cut)).As()
        select owned;

    private static Fin<Seq<Cut>> Cuts(GeometryBase[]? results, ObjectAttributes[]? rows, object owner, [CallerArgumentExpression(nameof(results))] string call = "") =>
        (Conversions.Found(results, call: call), Conversions.Found(rows, call: call))
            .Apply((kept, attributes) => toSeq(kept).Zip(toSeq(attributes), (geometry, row) => new Cut(geometry, row, owner)).Strict())
            .As();

    // --- [REPLACEMENT]
    public static IO<TResult> ReplaceGeometry<T, TResult>(RhinoDoc doc, Guid id, Func<T, IO<(T Geometry, TResult Result)>> edit) where T : GeometryBase =>
        from resolved in ObjectTarget.Resolve<RhinoObject, T>(doc, id)
        from result in (from reference in use(() => new ObjRef(resolved.Object))
                        from target in Products.Made(reference.Geometry() as T)
                        from privated in IO.lift(target.EnsurePrivateCopy)
                        from edited in edit(target)
                        from landed in TableOps.Apply(doc, new TableOp.Replace(id, edited.Geometry, IgnoreModes: false))
                        select edited.Result).Bracket()
        select result;
}
