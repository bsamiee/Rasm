using Rasm.Rhino.Document;
using Rasm.Rhino.Document.Tables;
using Rasm.Rhino.Persistence.Stores;
using Rhino;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;

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

    public bool Changes => Participation.Changes || Depth.Changes || DimensionStyle.Changes;
}

public sealed record ClipEdit(Seq<ViewportSet> Attach, Seq<ViewportSet> Detach, ClipSettings Settings) {
    public bool Changes => !Attach.IsEmpty || !Detach.IsEmpty || Settings.Changes;
}

public sealed record ClipSpec(PlaneSurface Surface, ViewportSet Viewports, ClipSettings Settings, Option<ObjectAttributes> Attributes);

public sealed record ClipState(Seq<Guid> ViewportIds, Option<ClipParticipation> Participation, Option<ClipDepth> Depth, Option<Guid> DimensionStyleId) {
    public static Fin<ClipState> Of(ClippingPlaneSurface surface) {
        surface.GetClipParticipation(out IEnumerable<Guid> objectIds, out IEnumerable<int> layerIndices, out bool isExclusionList);
        return Callbacks.Found(surface.PlaneDepthEnabled, surface.PlaneDepth)
            .Traverse(Conversions.Validated<ClipDepth, double, InvalidRhinoValue>)
            .As()
            .Map(depth => new ClipState(
                toSeq(surface.ViewportIds()),
                Callbacks.Found(surface.ParticipationListsEnabled, new ClipParticipation(toSeq(objectIds), toSeq(layerIndices), isExclusionList)),
                depth,
                Conversions.Present(surface.DimensionStyleId)));
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ClippingPlanes {
    // --- [CREATION]
    public static IO<Guid> Add(RhinoDoc doc, ClipSpec spec) =>
        from style in Styled(doc, spec.Settings.DimensionStyle)
        from id in Scoped(doc, Seq(spec.Viewports), rows =>
            (from surface in use(() => new ClippingPlaneSurface(spec.Surface))
             from staged in IO.lift(() => Staged(surface, rows, Seq<ViewportRef>(), spec.Settings, style))
             from added in TableOps.Add(doc, new GeometryPair(surface, spec.Attributes), None, reference: false)
             select added).Bracket())
        select id;

    // --- [EDITS]
    public static IO<Seq<Guid>> Edit(RhinoDoc doc, ObjectTarget planes, ClipEdit edit) =>
        edit.Changes
            ? from style in Styled(doc, edit.Settings.DimensionStyle)
              from found in planes.Objects(doc)
              from edited in Scoped(doc, edit.Attach, attach => Scoped(doc, edit.Detach, detach => Apply(doc, found, attach, detach, edit.Settings, style)))
              select edited
            : IO.pure(Seq<Guid>());

    public static IO<Seq<Guid>> Prune(RhinoDoc doc, Seq<ViewportSet> viewports) =>
        Scoped(doc, viewports, rows =>
            IO.lift(() => toSeq<RhinoObject>(rows.Bind(row => toSeq(doc.Objects.FindClippingPlanesForViewport(row.Viewport, includeHidden: true))).DistinctBy(static plane => plane.Id)).Strict())
                .Bind(planes => Apply(doc, planes, Seq<ViewportRef>(), rows, ClipSettings.Unchanged, new Override<Guid>.Keep())));

    private static IO<Seq<Guid>> Apply(RhinoDoc doc, Seq<RhinoObject> found, Seq<ViewportRef> attach, Seq<ViewportRef> detach, ClipSettings settings, Override<Guid> style) =>
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

    private static void Staged(ClippingPlaneSurface surface, Seq<ViewportRef> attach, Seq<ViewportRef> detach, ClipSettings settings, Override<Guid> style) {
        _ = attach.Iter(row => surface.AddClipViewportId(row.Viewport.Id));
        _ = detach.Iter(row => surface.RemoveClipViewportId(row.Viewport.Id));
        settings.Participation.Switch(surface,
            keep: static (_, _) => { },
            set: static (target, set) => {
                target.ParticipationListsEnabled = true;
                target.SetClipParticipation(set.Value.ObjectIds, set.Value.LayerIndices, set.Value.IsExclusionList);
            },
            clear: static (target, _) => target.ParticipationListsEnabled = false);
        settings.Depth.Switch(surface,
            keep: static (_, _) => { },
            set: static (target, set) => {
                target.PlaneDepthEnabled = true;
                target.PlaneDepth = set.Value;
            },
            clear: static (target, _) => target.PlaneDepthEnabled = false);
        style.Switch(surface,
            keep: static (_, _) => { },
            set: static (target, set) => target.DimensionStyleId = set.Value,
            clear: static (target, _) => target.DimensionStyleId = Guid.Empty);
    }

    // --- [READS]
    public static IO<ClipState> Read(RhinoDoc doc, Guid planeId) =>
        ObjectTarget.Resolve<ClippingPlaneObject, ClippingPlaneSurface>(doc, planeId).Bind(static plane => IO.lift(() => ClipState.Of(plane.Geometry)));

    public static IO<Seq<ClippingPlaneObject>> Clipping(RhinoDoc doc, ViewportTarget target) =>
        use(Viewports.ResolveViewport(doc, target)).Bind(row => IO.lift(() => toSeq(doc.Objects.FindClippingPlanesForViewport(row.Viewport, includeHidden: true)))).Bracket();

    private static IO<A> Scoped<A>(RhinoDoc doc, Seq<ViewportSet> sets, Func<Seq<ViewportRef>, IO<A>> body) =>
        DisposalOps.AcquireAll(sets.Map(set => Viewports.ResolveViewports(doc, set)), static held => DisposalOps.Release(held.Flatten()))
            .Map(static held => held.Flatten())
            .Bracket(Use: body, Fin: DisposalOps.Release);

    private static IO<Override<Guid>> Styled(RhinoDoc doc, Override<ComponentRef<DimensionStyle>> style) =>
        style.Switch(doc,
            keep: static (_, _) => IO.pure<Override<Guid>>(new Override<Guid>.Keep()),
            set: static (document, set) => TableOps.Find(document.DimStyles, set.Value, includeDeleted: false).Map(static row => (Override<Guid>)new Override<Guid>.Set(row.Id)),
            clear: static (_, _) => IO.pure<Override<Guid>>(new Override<Guid>.Clear()));
}
