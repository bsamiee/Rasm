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
        from style in Styled(doc, spec.Settings.DimensionStyle)
        from id in Acquired(doc, Seq(spec.Viewports), rows =>
            (from surface in use(() => new ClippingPlaneSurface(spec.Surface))
             from staged in IO.lift(() => Staged(surface, rows, Seq<ViewportRef>(), spec.Settings, style))
             from added in TableOps.Add(doc, new GeometryPair(surface, spec.Attributes), None, reference: false)
             select added).Bracket())
        select id;

    // --- [EDITS]
    public static IO<Seq<Guid>> Edit(RhinoDoc doc, ObjectTarget planes, ClipEdit edit) =>
        !edit.Attach.IsEmpty || !edit.Detach.IsEmpty || edit.Settings.Participation.Changes || edit.Settings.Depth.Changes || edit.Settings.DimensionStyle.Changes
            ? from style in Styled(doc, edit.Settings.DimensionStyle)
              from found in planes.Objects(doc)
              from edited in Acquired(doc, edit.Attach, attach => Acquired(doc, edit.Detach, detach => Apply(doc, found, attach, detach, edit.Settings, style)))
              select edited
            : IO.pure(Seq<Guid>());

    public static IO<Seq<Guid>> Prune(RhinoDoc doc, Seq<ViewportSet> viewports) =>
        Acquired(doc, viewports, rows =>
            from planes in IO.lift(() => toSeq(rows.Bind(row => toSeq(doc.Objects.FindClippingPlanesForViewport(row.Viewport, includeHidden: true))).DistinctBy(static plane => plane.Id)).Strict())
            from pruned in Apply(doc, planes, Seq<ViewportRef>(), rows, ClipSettings.Unchanged, style: None)
            select pruned);

    private static IO<Seq<Guid>> Apply<TObject>(RhinoDoc doc, Seq<TObject> found, Seq<ViewportRef> attach, Seq<ViewportRef> detach, ClipSettings settings, Option<Guid> style) where TObject : RhinoObject =>
        found.Map(static (target, index) => (Target: target, Index: index)).TraverseM(row => Editable(doc, row.Target, row.Index,
            from plane in ObjectTarget.Resolve<ClippingPlaneObject, ClippingPlaneSurface>(doc, row.Target.Id)
            from staged in IO.lift(() => Staged(plane.Geometry, attach, detach, settings, style))
            from committed in IO.lift(() => RefusedElement.Unless(plane.Object.CommitChanges(), row.Target.Id, nameof(RhinoObject.CommitChanges), row.Index))
            select committed)).As();

    private static IO<T> Editable<T>(RhinoDoc doc, RhinoObject target, int index, IO<T> body) =>
        target.Attributes.Mode switch {
            ObjectMode.Hidden => IO.lift(() => RefusedElement.Unless(doc.Objects.Show(target.Id, ignoreLayerMode: true), nameof(ObjectTable.Show), index))
                .Bracket(Use: _ => body, Fin: _ => IO.lift(() => RefusedElement.Unless(doc.Objects.Hide(target.Id, ignoreLayerMode: true), nameof(ObjectTable.Hide), index))),
            ObjectMode.Locked => IO.lift(() => RefusedElement.Unless(doc.Objects.Unlock(target.Id, ignoreLayerMode: true), nameof(ObjectTable.Unlock), index))
                .Bracket(Use: _ => body, Fin: _ => IO.lift(() => RefusedElement.Unless(doc.Objects.Lock(target.Id, ignoreLayerMode: true), nameof(ObjectTable.Lock), index))),
            ObjectMode.Normal or ObjectMode.InstanceDefinitionObject => body,
        };

    private static void Staged(ClippingPlaneSurface surface, Seq<ViewportRef> attach, Seq<ViewportRef> detach, ClipSettings settings, Option<Guid> style) {
        _ = attach.Iter(row => surface.AddClipViewportId(row.Viewport.Id));
        _ = detach.Iter(row => surface.RemoveClipViewportId(row.Viewport.Id));
        ClipMapper.Update((
            settings.Participation.Map<Option<bool>>(keep: None, set: true, clear: false),
            settings.Depth.Map<Option<bool>>(keep: None, set: true, clear: false),
            settings.Depth.Applied(None).Map(static depth => (double)depth), style), surface);
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

    private static IO<T> Acquired<T>(RhinoDoc doc, Seq<ViewportSet> sets, Func<Seq<ViewportRef>, IO<T>> body) =>
        DisposalOps.AcquireAll(sets.Map(set => Viewports.ResolveViewports(doc, set)), static held => DisposalOps.Release(held.Flatten()))
            .Map(static held => held.Flatten())
            .Bracket(Use: body, Fin: DisposalOps.Release);

    private static IO<Option<Guid>> Styled(RhinoDoc doc, Override<ComponentRef<DimensionStyle>> style) =>
        style.Switch(doc,
            keep: static (_, _) => IO.pure(Option<Guid>.None),
            set: static (document, set) => TableOps.Find(document.DimStyles, set.Value, includeDeleted: false).Map(static row => Some(row.Id)),
            clear: static (_, _) => IO.pure(Some(Guid.Empty)));
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Source)]
internal static partial class ClipMapper {
    internal static partial void Update((Option<bool> ParticipationListsEnabled, Option<bool> PlaneDepthEnabled, Option<double> PlaneDepth, Option<Guid> DimensionStyleId) settings, ClippingPlaneSurface surface);

    [UserMapping(Default = true)]
    private static bool Flag(Option<bool> value, [MappingTargetOriginalValue] bool original) => value.IfNone(original);

    [UserMapping(Default = true)]
    private static double Scalar(Option<double> value, [MappingTargetOriginalValue] double original) => value.IfNone(original);

    [UserMapping(Default = true)]
    private static Guid Id(Option<Guid> value, [MappingTargetOriginalValue] Guid original) => value.IfNone(original);
}
