using System.Collections.Generic;
using Rasm.Rhino.Document.Tables;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.Viewport;
using Rhino;
using Rhino.DocObjects;

namespace Rasm.Rhino.Objects;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ClipParticipation(Seq<Guid> ObjectIds, Seq<int> LayerIndices, bool IsExclusionList);

public sealed record ClipSettings(Override<ClipParticipation> Participation, Override<double> Depth, Override<ComponentRef<DimensionStyle>> DimensionStyle) {
    public static ClipSettings Unchanged { get; } = new(new Override<ClipParticipation>.Keep(), new Override<double>.Keep(), new Override<ComponentRef<DimensionStyle>>.Keep());
}

public sealed record ClipEdit(Seq<ViewportSet> Attach, Seq<ViewportSet> Detach, ClipSettings Settings);

public sealed record ClipSpec(PlaneSurface Surface, ViewportSet Viewports, ClipSettings Settings, Option<ObjectAttributes> Attributes);

public sealed record ClipState(Seq<Guid> ViewportIds, Option<ClipParticipation> Participation, Option<double> Depth, Option<Guid> DimensionStyleId);

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
            from committed in IO.lift(() => RhinoFault.Unless(plane.Object.CommitChanges(), row.Index))
            select row.Target.Id)).As();

    private static IO<T> Editable<T>(RhinoDoc doc, RhinoObject target, int index, IO<T> body) =>
        target.Attributes.Mode switch {
            ObjectMode.Hidden => IO.lift(() => RhinoFault.Unless(doc.Objects.Show(target.Id, ignoreLayerMode: true), index))
                >> body.Finally(IO.lift(() => RhinoFault.Unless(doc.Objects.Hide(target.Id, ignoreLayerMode: true), index))),
            ObjectMode.Locked => IO.lift(() => RhinoFault.Unless(doc.Objects.Unlock(target.Id, ignoreLayerMode: true), index))
                >> body.Finally(IO.lift(() => RhinoFault.Unless(doc.Objects.Lock(target.Id, ignoreLayerMode: true), index))),
            ObjectMode.Normal or ObjectMode.InstanceDefinitionObject => body,
        };

    private static void Staged(ClippingPlaneSurface surface, Seq<ViewportRef> attach, Seq<ViewportRef> detach, ClipSettings settings, Option<Guid> style) {
        _ = attach.Iter(row => surface.AddClipViewportId(row.Viewport.Id));
        _ = detach.Iter(row => surface.RemoveClipViewportId(row.Viewport.Id));
        _ = settings.Participation.Map<Option<bool>>(keep: None, set: true, clear: false).Iter(enabled => surface.ParticipationListsEnabled = enabled);
        _ = settings.Depth.Map<Option<bool>>(keep: None, set: true, clear: false).Iter(enabled => surface.PlaneDepthEnabled = enabled);
        _ = settings.Depth.Applied(None).Iter(depth => surface.PlaneDepth = depth);
        _ = style.Iter(id => surface.DimensionStyleId = id);
        _ = settings.Participation.Applied(None).Iter(value => surface.SetClipParticipation(value.ObjectIds, value.LayerIndices, value.IsExclusionList));
    }

    // --- [READS]
    public static IO<ClipState> Read(RhinoDoc doc, Guid planeId) =>
        from plane in ObjectTarget.Resolve<ClippingPlaneObject, ClippingPlaneSurface>(doc, planeId)
        from state in IO.lift(() => {
            ClippingPlaneSurface surface = plane.Geometry;
            surface.GetClipParticipation(out IEnumerable<Guid> objectIds, out IEnumerable<int> layerIndices, out bool isExclusionList);
            return new ClipState(toSeq(surface.ViewportIds()),
                Conversions.Found(surface.ParticipationListsEnabled, new ClipParticipation(toSeq(objectIds), toSeq(layerIndices), isExclusionList)),
                Conversions.Found(surface.PlaneDepthEnabled, surface.PlaneDepth), Conversions.Present(surface.DimensionStyleId));
        })
        select state;

    public static IO<Seq<ClippingPlaneObject>> Clipping(RhinoDoc doc, ViewportTarget target) =>
        Viewports.With(doc, target, row => IO.lift(() => toSeq(doc.Objects.FindClippingPlanesForViewport(row.Viewport, includeHidden: true))));

    private static IO<T> Acquired<T>(RhinoDoc doc, Seq<ViewportSet> sets, Func<Seq<ViewportRef>, IO<T>> body) =>
        sets.TraverseM(set => Viewports.Resolve(doc, set)).As().Map(static rows => rows.Flatten()).Bind(body).Bracket();

    private static IO<Option<Guid>> Styled(RhinoDoc doc, Override<ComponentRef<DimensionStyle>> style) =>
        style.Switch(doc,
            keep: static (_, _) => IO.pure(Option<Guid>.None),
            set: static (document, set) => TableOps.Find(document.DimStyles, set.Value, includeDeleted: false).Map(static row => Some(row.Id)),
            clear: static (_, _) => IO.pure(Some(Guid.Empty)));
}
