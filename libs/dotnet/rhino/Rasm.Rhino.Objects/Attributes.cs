using System.Drawing;
using LanguageExt.UnsafeValueAccess;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.DocObjects;
using Rhino.Render;

namespace Rasm.Rhino.Objects;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record AttributeState(
    Guid Id,
    Option<string> Name,
    Option<string> Url,
    int LayerIndex,
    int LinetypeIndex,
    Option<int> MaterialIndex,
    Option<int> SectionStyleIndex,
    Option<Guid> ViewportId,
    ActiveSpace Space,
    ObjectMode Mode,
    bool Visible,
    bool IsInstanceDefinitionObject,
    ObjectColorSource ColorSource,
    Color ObjectColor,
    ObjectPlotColorSource PlotColorSource,
    Color PlotColor,
    ObjectPlotWeightSource PlotWeightSource,
    PlotWeight PlotWeight,
    ObjectLinetypeSource LinetypeSource,
    double LinetypePatternScale,
    ObjectMaterialSource MaterialSource,
    ObjectSectionAttributesSource SectionAttributesSource,
    bool CastsShadows,
    bool ReceivesShadows,
    int WireDensity,
    int DisplayOrder,
    ObjectDecoration ObjectDecoration,
    SectionLabelStyle ClippingPlaneLabelStyle,
    bool DetailBackgroundVisible,
    bool HasMapping,
    Color HatchBackgroundFillColor,
    Color HatchBackgroundFillPrintColor,
    bool HatchBoundaryVisible,
    Color HatchBoundaryColor,
    Color HatchBoundaryPlotColor,
    ItemColorSource HatchBoundaryColorSource,
    ItemColorSource HatchBoundaryPlotColorSource,
    Option<PlotWeight> HatchBoundaryPlotWeight,
    Seq<Guid> HiddenInDetails,
    Option<(Seq<Guid> Viewports, bool Active)> ActiveOverrides,
    bool HasCustomSectionStyle,
    bool HasCustomLinetype,
    Option<MeshingParameters> CustomMeshing,
    bool CustomMeshingEnabled,
    bool HasDisplacement,
    bool HasEdgeSoftening,
    bool HasThickening,
    bool HasCurvePiping,
    bool HasShutLining,
    Seq<int> Groups,
    HashMap<string, string> UserStrings,
    Option<Plane> ObjectFrame,
    Seq<DecalState> Decals,
    Seq<MaterialRefState> MaterialRefs);

public sealed record DecalState(
    int Crc,
    bool Visible,
    Guid Texture,
    DecalMapping Mapping,
    DecalProjection Projection,
    Point3d Origin,
    Vector3d Up,
    Vector3d Across,
    double Transparency,
    bool MapToInside,
    double Height,
    double Radius,
    double HorzStart,
    double HorzEnd,
    double VertStart,
    double VertEnd,
    double MinU,
    double MinV,
    double MaxU,
    double MaxV);

public sealed record MaterialRefState(Guid PlugIn, ObjectMaterialSource Source, Option<Guid> FrontId, Option<Guid> BackId, Option<int> FrontIndex, Option<int> BackIndex);

public sealed record EffectiveDisplay(Color Draw, Color Plot, PlotWeight Weight, Option<Guid> DisplayMode, Option<bool> Active);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RowsEdit<TRow, TKey> {
    public sealed record Replace(Seq<TRow> Rows) : RowsEdit<TRow, TKey>;

    public sealed record Add(Seq<TRow> Rows) : RowsEdit<TRow, TKey>;

    public sealed record Remove(Seq<TKey> Keys) : RowsEdit<TRow, TKey>;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record AttributeEdit {
    public sealed record Name(Option<string> Value) : AttributeEdit;

    public sealed record Url(Option<string> Value) : AttributeEdit;

    public sealed record LayerIndex(int Index) : AttributeEdit;

    public sealed record ObjectColor(ObjectColorSource Source, Option<Color> Value) : AttributeEdit;

    public sealed record PlotColor(ObjectPlotColorSource Source, Option<Color> Value) : AttributeEdit;

    public sealed record PlotWeight(ObjectPlotWeightSource Source, Option<Document.PlotWeight> Weight) : AttributeEdit;

    public sealed record LinetypeIndex(ObjectLinetypeSource Source, Option<int> Index, Option<double> PatternScale) : AttributeEdit;

    public sealed record CustomLinetype(Option<Linetype> Value) : AttributeEdit;

    public sealed record MaterialIndex(ObjectMaterialSource Source, Option<int> Index) : AttributeEdit;

    public sealed record Shadowing(bool Casts, bool Receives) : AttributeEdit;

    public sealed record WireDensity(int Density) : AttributeEdit;

    public sealed record DisplayOrder(int Value) : AttributeEdit;

    public sealed record ObjectDecoration(global::Rhino.DocObjects.ObjectDecoration Value) : AttributeEdit;

    public sealed record Space(ActiveSpace Value, Option<Guid> ViewportId) : AttributeEdit;

    public sealed record Groups(RowsEdit<int, int> Edit) : AttributeEdit;

    public sealed record DisplayModeOverride(Option<Guid> Viewport, Option<Guid> Mode) : AttributeEdit;

    public sealed record HideInDetail(RowsEdit<Guid, Guid> Edit) : AttributeEdit;

    public sealed record DetailBackground(bool Visible) : AttributeEdit;

    public sealed record ActiveInViewport(RowsEdit<Guid, Guid> Edit, bool Active) : AttributeEdit;

    public sealed record SectionAttributesSource(ObjectSectionAttributesSource Value) : AttributeEdit;

    public sealed record SectionStyleIndex(Option<int> Index) : AttributeEdit;

    public sealed record CustomSectionStyle(Option<SectionStyle> Style) : AttributeEdit;

    public sealed record ClippingPlaneLabelStyle(SectionLabelStyle Style) : AttributeEdit;

    public sealed record HatchBackgroundFill(Option<Color> HatchBackgroundFillColor, Option<Color> HatchBackgroundFillPrintColor) : AttributeEdit;

    public sealed record HatchBoundary(
        Option<bool> HatchBoundaryVisible,
        Option<Color> HatchBoundaryColor,
        Option<Color> HatchBoundaryPlotColor,
        Option<ItemColorSource> HatchBoundaryColorSource,
        Option<ItemColorSource> HatchBoundaryPlotColorSource,
        Option<Option<Document.PlotWeight>> HatchBoundaryPlotWeight) : AttributeEdit;

    public sealed record PlaneFrame(Plane Value) : AttributeEdit;

    public sealed record TransformFrame(Transform Value) : AttributeEdit;

    public sealed record CustomMeshing(Option<MeshingParameters> Parameters, bool Enabled) : AttributeEdit;

    public sealed record ClearRenderingAttributes() : AttributeEdit;

    public sealed record Decals(RowsEdit<DecalCreateParams, int> Edit) : AttributeEdit;

    public sealed record UserStrings(UserStringEdit Edit) : AttributeEdit;

    public sealed record MaterialRefs(RowsEdit<MaterialRefCreateParams, Guid> Edit) : AttributeEdit;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class AttributeOps {
    // --- [READS]
    public static IO<AttributeState> ReadAttributes(ObjectAttributes a) =>
        from userStrings in GeometryOps.ReadUserStrings(GeometryOps.UserStrings(a))
        from state in IO.lift(() => new AttributeState(
            a.ObjectId,
            Answers.Present(a.Name),
            Answers.Present(a.Url),
            a.LayerIndex,
            a.LinetypeIndex,
            Answers.Present(a.MaterialIndex),
            Answers.Present(a.SectionStyleIndex),
            Answers.Present(a.ViewportId),
            a.Space,
            a.Mode,
            a.Visible,
            a.IsInstanceDefinitionObject,
            a.ColorSource,
            a.ObjectColor,
            a.PlotColorSource,
            a.PlotColor,
            a.PlotWeightSource,
            PlotWeight.FromHost(a.PlotWeight),
            a.LinetypeSource,
            a.LinetypePatternScale,
            a.MaterialSource,
            a.SectionAttributesSource,
            a.CastsShadows,
            a.ReceivesShadows,
            a.WireDensity,
            a.DisplayOrder,
            a.ObjectDecoration,
            a.ClippingPlaneLabelStyle,
            a.DetailBackgroundVisible,
            a.HasMapping,
            a.HatchBackgroundFillColor,
            a.HatchBackgroundFillPrintColor,
            a.HatchBoundaryVisible,
            a.HatchBoundaryColor,
            a.HatchBoundaryPlotColor,
            a.HatchBoundaryColorSource,
            a.HatchBoundaryPlotColorSource,
            PlotWeight.FromHatchBoundary(a.HatchBoundaryPlotWeightMillimeters),
            toSeq(a.GetHideInDetailOverrides()),
            Answers.Found(a.GetActiveInViewportOverrides(out Guid[] viewports, out bool active), (Viewports: toSeq(viewports), Active: active)),
            Optional(a.GetCustomSectionStyle()).Do(static style => style.Dispose()).IsSome,
            Optional(a.GetCustomLinetype()).Do(static linetype => linetype.Dispose()).IsSome,
            Optional(a.CustomMeshingParameters),
            a.EnableCustomMeshingParameters,
            a.File3dmMeshModifiers.Displacement is not null,
            a.File3dmMeshModifiers.EdgeSoftening is not null,
            a.File3dmMeshModifiers.Thickening is not null,
            a.File3dmMeshModifiers.CurvePiping is not null,
            a.File3dmMeshModifiers.ShutLining is not null,
            toSeq(a.GetGroupList()),
            userStrings,
            Some(a.ObjectFrame()).Filter(static plane => plane.IsValid),
            toSeq(a.Decals).Map(static decal => {
                decal.HorzSweep(out double horzStart, out double horzEnd);
                decal.VertSweep(out double vertStart, out double vertEnd);
                decal.GetUVBounds(out double minU, out double minV, out double maxU, out double maxV);
                return new DecalState(
                    decal.CRC,
                    decal.IsVisible,
                    decal.TextureInstanceId,
                    decal.Mapping,
                    decal.Projection,
                    decal.Origin,
                    decal.VectorUp,
                    decal.VectorAcross,
                    decal.Transparency,
                    decal.MapToInside,
                    decal.Height,
                    decal.Radius,
                    horzStart,
                    horzEnd,
                    vertStart,
                    vertEnd,
                    minU,
                    minV,
                    maxU,
                    maxV);
            }).Strict(),
            toSeq(a.MaterialRefs.Values).Map(static row => new MaterialRefState(
                row.PlugInId,
                row.MaterialSource,
                Answers.Present(row.FrontFaceMaterialId),
                Answers.Present(row.BackFaceMaterialId),
                Answers.Present(row.FrontFaceMaterialIndex),
                Answers.Present(row.BackFaceMaterialIndex))).Strict()))
        select state;

    public static IO<EffectiveDisplay> ReadEffective(ObjectAttributes a, RhinoDoc doc, Option<Guid> viewport) =>
        IO.lift(() => viewport.Match(
            Some: id => new EffectiveDisplay(
                a.DrawColor(doc, id),
                a.ComputedPlotColor(doc, id),
                PlotWeight.FromHost(a.ComputedPlotWeight(doc, id)),
                Overridden(a, id),
                Answers.Found(a.HasActiveInViewportOverride(id, out bool active), active)),
            None: () => new EffectiveDisplay(
                a.DrawColor(doc),
                a.ComputedPlotColor(doc),
                PlotWeight.FromHost(a.ComputedPlotWeight(doc)),
                Overridden(a, Guid.Empty),
                Option<bool>.None)));

    private static Option<Guid> Overridden(ObjectAttributes a, Guid viewport) =>
        a.HasDisplayModeOverride(viewport) ? Some(a.GetDisplayModeOverride(viewport)) : Option<Guid>.None;

    // --- [EDITS]
    public static IO<Unit> Apply(ObjectAttributes a, AttributeEdit edit) =>
        edit.Switch(
            a,
            name: static (target, name) => IO.lift(() => { target.Name = name.Value.IfNone(""); }),
            url: static (target, url) => IO.lift(() => { target.Url = url.Value.IfNone(""); }),
            layerIndex: static (target, layer) =>
                from index in IO.lift(() => Answers.NonNegative(layer.Index, nameof(ObjectAttributes.LayerIndex)))
                from written in IO.lift(() => { target.LayerIndex = index; })
                select written,
            objectColor: static (target, color) => IO.lift(() => {
                target.ColorSource = color.Source;
                _ = color.Value.Iter(value => target.ObjectColor = value);
            }),
            plotColor: static (target, color) => IO.lift(() => {
                target.PlotColorSource = color.Source;
                _ = color.Value.Iter(value => target.PlotColor = value);
            }),
            plotWeight: static (target, plot) => IO.lift(() => {
                target.PlotWeightSource = plot.Source;
                _ = plot.Weight.Iter(weight => target.PlotWeight = weight.ToHost());
            }),
            linetypeIndex: static (target, linetype) =>
                from index in IO.lift(() => linetype.Index.Traverse(static value => Answers.NonNegative(value, nameof(ObjectAttributes.LinetypeIndex))).As())
                from written in IO.lift(() => {
                    target.LinetypeSource = linetype.Source;
                    _ = index.Iter(value => target.LinetypeIndex = value);
                    _ = linetype.PatternScale.Iter(scale => target.LinetypePatternScale = scale);
                })
                select written,
            customLinetype: static (target, custom) => IO.lift(() => custom.Value.Match(Some: target.SetCustomLinetype, None: target.RemoveCustomLinetype)),
            materialIndex: static (target, material) =>
                from index in IO.lift(() => material.Index.Traverse(static value => Answers.NonNegative(value, nameof(ObjectAttributes.MaterialIndex))).As())
                from written in IO.lift(() => {
                    target.MaterialSource = material.Source;
                    target.MaterialIndex = index.IfNone(-1);
                })
                select written,
            shadowing: static (target, shadows) => IO.lift(() => {
                target.CastsShadows = shadows.Casts;
                target.ReceivesShadows = shadows.Receives;
            }),
            wireDensity: static (target, wires) =>
                from valid in IO.lift(() => Limits.AtLeast(-1).Check(wires.Density, nameof(ObjectAttributes.WireDensity)))
                from written in IO.lift(() => { target.WireDensity = wires.Density; })
                select written,
            displayOrder: static (target, order) => IO.lift(() => { target.DisplayOrder = order.Value; }),
            objectDecoration: static (target, decoration) => IO.lift(() => { target.ObjectDecoration = decoration.Value; }),
            space: static (target, space) => IO.lift(() => {
                target.Space = space.Value;
                target.ViewportId = space.ViewportId.IfNone(Guid.Empty);
            }),
            groups: static (target, groups) => Edited(
                groups.Edit,
                IO.lift(target.RemoveFromAllGroups),
                index => Grouped(index, target.AddToGroup, nameof(ObjectAttributes.AddToGroup)),
                index => Grouped(index, target.RemoveFromGroup, nameof(ObjectAttributes.RemoveFromGroup))),
            displayModeOverride: static (target, mode) => mode.Mode.Case is Guid id
                ? Viewports.WithMode(id, description => IO.lift(() => Refused.Unless(
                    mode.Viewport.Case is Guid viewport ? target.SetDisplayModeOverride(description, viewport) : target.SetDisplayModeOverride(description),
                    nameof(ObjectAttributes.SetDisplayModeOverride))))
                : IO.lift(() => mode.Viewport.Match(Some: target.RemoveDisplayModeOverride, None: target.RemoveDisplayModeOverride)),
            hideInDetail: static (target, hide) =>
                from present in IO.lift(() => toSet(target.GetHideInDetailOverrides()))
                from changed in hide.Edit.Switch(
                    (Target: target, Present: present),
                    replace: static (state, replace) => Hidden(state.Target, toSet(replace.Rows).Except(state.Present).ToSeq(), state.Present.Except(replace.Rows).ToSeq()),
                    add: static (state, add) => Hidden(state.Target, toSet(add.Rows).Except(state.Present).ToSeq(), Seq<Guid>()),
                    remove: static (state, remove) => Hidden(state.Target, Seq<Guid>(), state.Present.Intersect(remove.Keys).ToSeq()))
                select changed,
            detailBackground: static (target, detail) => IO.lift(() => { target.DetailBackgroundVisible = detail.Visible; }),
            activeInViewport: static (target, active) => active.Edit.Switch(
                (Target: target, active.Active),
                replace: static (state, replace) =>
                    IO.lift(() => Refused.Unless(state.Target.SetActiveInViewportOverrides([.. replace.Rows], state.Active), nameof(ObjectAttributes.SetActiveInViewportOverrides))),
                add: static (state, add) => Each(add.Rows, id => state.Target.AddActiveInViewportOverride(id, state.Active), nameof(ObjectAttributes.AddActiveInViewportOverride)),
                remove: static (state, remove) => Each(
                    remove.Keys,
                    id => !state.Target.HasActiveInViewportOverride(id, out bool stored) || state.Target.RemoveActiveInViewportOverride(id, stored),
                    nameof(ObjectAttributes.RemoveActiveInViewportOverride))),
            sectionAttributesSource: static (target, section) => IO.lift(() => { target.SectionAttributesSource = section.Value; }),
            sectionStyleIndex: static (target, style) =>
                from index in IO.lift(() => style.Index.Traverse(static value => Answers.NonNegative(value, nameof(ObjectAttributes.SectionStyleIndex))).As())
                from written in IO.lift(() => { target.SectionStyleIndex = index.IfNone(-1); })
                select written,
            customSectionStyle: static (target, custom) => IO.lift(() => custom.Style.Match(Some: target.SetCustomSectionStyle, None: target.RemoveCustomSectionStyle)),
            clippingPlaneLabelStyle: static (target, label) => IO.lift(() => { target.ClippingPlaneLabelStyle = label.Style; }),
            hatchBackgroundFill: static (target, fill) => IO.lift(() => {
                _ = fill.HatchBackgroundFillColor.Iter(color => target.HatchBackgroundFillColor = color);
                _ = fill.HatchBackgroundFillPrintColor.Iter(color => target.HatchBackgroundFillPrintColor = color);
            }),
            hatchBoundary: static (target, boundary) => IO.lift(() => {
                _ = boundary.HatchBoundaryVisible.Iter(visible => target.HatchBoundaryVisible = visible);
                _ = boundary.HatchBoundaryColor.Iter(color => target.HatchBoundaryColor = color);
                _ = boundary.HatchBoundaryPlotColor.Iter(color => target.HatchBoundaryPlotColor = color);
                _ = boundary.HatchBoundaryColorSource.Iter(source => target.HatchBoundaryColorSource = source);
                _ = boundary.HatchBoundaryPlotColorSource.Iter(source => target.HatchBoundaryPlotColorSource = source);
                _ = boundary.HatchBoundaryPlotWeight.Iter(weight => target.HatchBoundaryPlotWeightMillimeters = PlotWeight.ToHatchBoundary(weight));
            }),
            planeFrame: static (target, frame) =>
                from valid in IO.lift(() => Invalid.Unless(frame.Value.IsValid, nameof(Plane.IsValid)))
                from written in IO.lift(() => target.SetObjectFrame(frame.Value))
                select written,
            transformFrame: static (target, frame) =>
                from valid in IO.lift(() => Invalid.Unless(frame.Value.IsValid, nameof(Transform.IsValid)))
                from written in IO.lift(() => target.SetObjectFrame(frame.Value))
                select written,
            customMeshing: static (target, meshing) =>
                from written in IO.lift(() => { target.CustomMeshingParameters = meshing.Parameters.ValueUnsafe(); })
                from enabled in IO.lift(() => { target.EnableCustomMeshingParameters = meshing.Enabled; })
                select enabled,
            clearRenderingAttributes: static (target, _) => IO.lift(target.ClearRenderingAttributes),
            decals: static (target, decals) => Edited(
                decals.Edit,
                IO.lift(target.Decals.RemoveAllDecals),
                row => AddedDecal(target, row),
                crc => RemovedDecal(target, crc)),
            userStrings: static (target, strings) => GeometryOps.EditUserStrings(GeometryOps.UserStrings(target), strings.Edit),
            materialRefs: static (target, refs) => Edited(
                refs.Edit,
                IO.lift(target.MaterialRefs.Clear),
                row => AddedMaterialRef(target.MaterialRefs, row),
                key => IO.lift(() => { _ = target.MaterialRefs.Remove(key); })));

    private static IO<Unit> Edited<TRow, TKey>(RowsEdit<TRow, TKey> edit, IO<Unit> clear, Func<TRow, IO<Unit>> add, Func<TKey, IO<Unit>> remove) =>
        edit.Switch(
            (Clear: clear, Add: add, Remove: remove),
            replace: static (accessors, replace) => accessors.Clear.Bind(_ => replace.Rows.TraverseM(accessors.Add).As()).Map(static _ => unit),
            add: static (accessors, added) => added.Rows.TraverseM(accessors.Add).As().Map(static _ => unit),
            remove: static (accessors, removed) => removed.Keys.TraverseM(accessors.Remove).As().Map(static _ => unit));

    private static IO<Unit> Grouped(int index, Action<int> write, string member) =>
        IO.lift(() => Answers.NonNegative(index, member)).Bind(valid => IO.lift(() => write(valid)));

    private static IO<Unit> AddedMaterialRef(MaterialRefs refs, MaterialRefCreateParams row) =>
        from keyed in IO.lift(() => Answers.NonEmpty(row.PlugInId, nameof(MaterialRefCreateParams.PlugInId)))
        from added in Disposal.Using(() => refs.Create(row), staged => IO.lift(() => refs.Add(keyed, staged)))
        select added;

    private static IO<Unit> AddedDecal(ObjectAttributes a, DecalCreateParams row) =>
        from added in Disposal.Using(IO.lift(() => Missing.Unless(Decal.Create(row), nameof(Decal.Create))), decal => IO.lift(() => a.Decals.Add(decal)))
        from accepted in IO.lift(Refused.Unless(added != 0u, nameof(Decals.Add)))
        select accepted;

    private static IO<Unit> RemovedDecal(ObjectAttributes a, int crc) =>
        IO.lift(() => toSeq(a.Decals).Filter(decal => decal.CRC == crc).Strict()).Bind(matching => Each(matching, a.Decals.Remove, nameof(Decals.Remove)));

    private static IO<Unit> Each<T>(Seq<T> items, Func<T, bool> call, string member) =>
        items.TraverseM(item => IO.lift(() => Refused.Unless(call(item), member))).As().Map(static _ => unit);

    private static IO<Unit> Hidden(ObjectAttributes a, Seq<Guid> add, Seq<Guid> remove) =>
        Each(remove, a.RemoveHideInDetailOverride, nameof(ObjectAttributes.RemoveHideInDetailOverride))
            .Bind(_ => Each(add, a.AddHideInDetailOverride, nameof(ObjectAttributes.AddHideInDetailOverride)));

    // --- [COMMITS]
    public static IO<Seq<Guid>> Modify(RhinoDoc doc, ObjectTarget target, Seq<AttributeEdit> edits, bool quiet) =>
        TableOps.Apply(doc, new TableOp.ModifyAttributes(target, a => edits.TraverseM(edit => Apply(a, edit)).As().Map(static _ => unit), quiet));
}
