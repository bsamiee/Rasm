using Rasm.Rhino.Document;
using Rasm.Rhino.Document.Tables;
using Rasm.Rhino.Persistence.Stores;
using Rhino;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Objects;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<double>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct ClipDepth : System.Numerics.IMinMaxValue<ClipDepth> {
    public static ClipDepth MinValue { get; } = new(0d);
    public static ClipDepth MaxValue { get; } = new(double.MaxValue);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref double value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidRhinoValue();
}

public sealed record ClipParticipation(Seq<Guid> ObjectIds, Seq<int> LayerIndices, bool IsExclusionList);

public sealed record ClipSettings(Override<ClipParticipation> Participation, Override<ClipDepth> Depth, Override<ComponentRef<DimensionStyle>> DimensionStyle) {
    public static ClipSettings Unchanged { get; } = new(new Override<ClipParticipation>.Keep(), new Override<ClipDepth>.Keep(), new Override<ComponentRef<DimensionStyle>>.Keep());
}

public sealed record ClipEdit(Seq<ViewportSet> Attach, Seq<ViewportSet> Detach, ClipSettings Settings);

public sealed record ClipSpec(PlaneSurface Surface, ViewportSet Viewports, ClipSettings Settings, Option<ObjectAttributes> Attributes);

public sealed record ClipState(Seq<Guid> ViewportIds, Option<ClipParticipation> Participation, Option<ClipDepth> Depth, Option<Guid> DimensionStyleId);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ClippingPlanes {
    // --- [CREATION]
    public static IO<Guid> Add(RhinoDoc doc, ClipSpec spec) =>
        (from style in Styled(doc, spec.Settings.DimensionStyle)
         from rows in Acquire(doc, Seq(spec.Viewports))
         from id in (from surface in use(() => new ClippingPlaneSurface(spec.Surface))
                     from staged in IO.lift(() => Staged(surface, rows, Seq<ViewportRef>(), spec.Settings, style))
                     from added in TableOps.Add(doc, new GeometryPair(surface, spec.Attributes), None, reference: false)
                     select added).Bracket()
         select id).Bracket();

    // --- [EDITS]
    public static IO<Seq<Guid>> Edit(RhinoDoc doc, ObjectTarget planes, ClipEdit edit) =>
        !edit.Attach.IsEmpty || !edit.Detach.IsEmpty || edit.Settings.Participation.Changes || edit.Settings.Depth.Changes || edit.Settings.DimensionStyle.Changes
            ? (from style in Styled(doc, edit.Settings.DimensionStyle)
               from found in planes.Objects(doc)
               from attach in Acquire(doc, edit.Attach)
               from edited in (from detach in Acquire(doc, edit.Detach)
                               from committed in Apply(doc, found, attach, detach, edit.Settings, style)
                               select committed).Bracket()
               select edited).Bracket()
            : IO.pure(Seq<Guid>());

    public static IO<Seq<Guid>> Prune(RhinoDoc doc, Seq<ViewportSet> viewports) =>
        (from rows in Acquire(doc, viewports)
         from planes in IO.lift(() => toSeq(rows.Bind(row => toSeq(doc.Objects.FindClippingPlanesForViewport(row.Viewport, includeHidden: true))).DistinctBy(static plane => plane.Id)).Strict())
         from pruned in Apply(doc, planes, Seq<ViewportRef>(), rows, ClipSettings.Unchanged, style: null)
         select pruned).Bracket();

    private static IO<Seq<Guid>> Apply<TObject>(RhinoDoc doc, Seq<TObject> found, Seq<ViewportRef> attach, Seq<ViewportRef> detach, ClipSettings settings, Guid? style) where TObject : RhinoObject =>
        found.Map(static (target, index) => (Target: target, Index: index)).TraverseM(row => Editable(doc, row.Target, row.Index,
            from plane in ObjectTarget.Resolve<ClippingPlaneObject, ClippingPlaneSurface>(doc, row.Target.Id)
            from staged in IO.lift(() => Staged(plane.Geometry, attach, detach, settings, style))
            from committed in IO.lift(() => RefusedElement.Unless(plane.Object.CommitChanges(), row.Target.Id, nameof(RhinoObject.CommitChanges), row.Index))
            select committed)).As();

    private static IO<A> Editable<A>(RhinoDoc doc, RhinoObject target, int index, IO<A> body) =>
        target.Attributes.Mode switch {
            ObjectMode.Hidden => IO.lift(() => RefusedElement.Unless(doc.Objects.Show(target.Id, ignoreLayerMode: true), nameof(ObjectTable.Show), index))
                .Bracket(Use: _ => body, Fin: _ => IO.lift(() => RefusedElement.Unless(doc.Objects.Hide(target.Id, ignoreLayerMode: true), nameof(ObjectTable.Hide), index))),
            ObjectMode.Locked => IO.lift(() => RefusedElement.Unless(doc.Objects.Unlock(target.Id, ignoreLayerMode: true), nameof(ObjectTable.Unlock), index))
                .Bracket(Use: _ => body, Fin: _ => IO.lift(() => RefusedElement.Unless(doc.Objects.Lock(target.Id, ignoreLayerMode: true), nameof(ObjectTable.Lock), index))),
            ObjectMode.Normal or ObjectMode.InstanceDefinitionObject => body,
        };

    private static void Staged(ClippingPlaneSurface surface, Seq<ViewportRef> attach, Seq<ViewportRef> detach, ClipSettings settings, Guid? style) {
        _ = attach.Iter(row => surface.AddClipViewportId(row.Viewport.Id));
        _ = detach.Iter(row => surface.RemoveClipViewportId(row.Viewport.Id));
        ClipMapper.Update((
            settings.Participation.Map<bool?>(keep: null, set: true, clear: false),
            settings.Depth.Map<bool?>(keep: null, set: true, clear: false),
            settings.Depth.Applied(None).Map(static depth => (double)depth).ToNullable(), style), surface);
        _ = settings.Participation.Applied(None).Iter(value => surface.SetClipParticipation(value.ObjectIds, value.LayerIndices, value.IsExclusionList));
    }

    // --- [READS]
    public static IO<ClipState> Read(RhinoDoc doc, Guid planeId) =>
        from plane in ObjectTarget.Resolve<ClippingPlaneObject, ClippingPlaneSurface>(doc, planeId)
        from state in IO.lift(() => {
            ClippingPlaneSurface surface = plane.Geometry;
            surface.GetClipParticipation(out IEnumerable<Guid> objectIds, out IEnumerable<int> layerIndices, out bool isExclusionList);
            return from depth in Callbacks.Found(surface.PlaneDepthEnabled, surface.PlaneDepth).Traverse(Conversions.Validated<ClipDepth, double, InvalidRhinoValue>).As()
                   select new ClipState(toSeq(surface.ViewportIds()),
                       Callbacks.Found(surface.ParticipationListsEnabled, new ClipParticipation(toSeq(objectIds), toSeq(layerIndices), isExclusionList)),
                       depth, Conversions.Present(surface.DimensionStyleId));
        })
        select state;

    public static IO<Seq<ClippingPlaneObject>> Clipping(RhinoDoc doc, ViewportTarget target) =>
        (from row in use(Viewports.ResolveViewport(doc, target))
         from planes in IO.lift(() => toSeq(doc.Objects.FindClippingPlanesForViewport(row.Viewport, includeHidden: true)))
         select planes).Bracket();

    private static IO<Seq<ViewportRef>> Acquire(RhinoDoc doc, Seq<ViewportSet> sets) =>
        use(DisposalOps.AcquireAll(sets.Map(set => Viewports.ResolveViewports(doc, set)), static held => DisposalOps.Release(held.Flatten()))
            .Map(static held => held.Flatten()), DisposalOps.Release);

    private static IO<Guid?> Styled(RhinoDoc doc, Override<ComponentRef<DimensionStyle>> style) =>
        style.Switch(doc,
            keep: static (_, _) => IO.pure<Guid?>(value: null),
            set: static (document, set) => TableOps.Find(document.DimStyles, set.Value, includeDeleted: false).Map(static row => (Guid?)row.Id),
            clear: static (_, _) => IO.pure<Guid?>(Guid.Empty));
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Source, AllowNullPropertyAssignment = false)]
internal static partial class ClipMapper {
    internal static partial void Update((bool? ParticipationListsEnabled, bool? PlaneDepthEnabled, double? PlaneDepth, Guid? DimensionStyleId) settings, ClippingPlaneSurface surface);
}
