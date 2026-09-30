using System.Drawing;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.DocObjects;
using Rhino.Render;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Objects;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record DecalState(
    int Crc,
    bool IsVisible,
    Guid TextureInstanceId,
    DecalMapping Mapping,
    DecalProjection Projection,
    Point3d Origin,
    Vector3d VectorUp,
    Vector3d VectorAcross,
    double Transparency,
    bool MapToInside,
    double Height,
    double Radius,
    (double Start, double End) HorzSweep,
    (double Start, double End) VertSweep,
    (double MinU, double MinV, double MaxU, double MaxV) UvBounds);

public sealed record MaterialRefState(
    Guid PlugInId,
    ObjectMaterialSource MaterialSource,
    Option<Guid> FrontFaceMaterialId,
    Option<Guid> BackFaceMaterialId,
    Option<int> FrontFaceMaterialIndex,
    Option<int> BackFaceMaterialIndex);

public sealed record AttributeState(
    Guid Id,
    Option<string> Name,
    Option<string> Url,
    int LayerIndex,
    Option<LinetypeRef> Linetype,
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
    bool EnableCustomMeshingParameters,
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

    public sealed record LayerIndex(LayerRef Address) : AttributeEdit;

    public sealed record Set(Action<ObjectAttributes> Write) : AttributeEdit;

    public sealed record PlotWeight(ObjectPlotWeightSource Source, Option<Document.PlotWeight> Weight) : AttributeEdit;

    public sealed record Linetype(ObjectLinetypeSource Source, LinetypeRef Pattern) : AttributeEdit;

    public sealed record MaterialIndex(ObjectMaterialSource Source, Option<ComponentRef> Address) : AttributeEdit;

    public sealed record RenderMaterial(global::Rhino.Render.RenderMaterial Material) : AttributeEdit;

    public sealed record WireDensity(int Density) : AttributeEdit;

    public sealed record Space(ActiveSpace Value, Option<Guid> ViewportId) : AttributeEdit;

    public sealed record Groups(RowsEdit<ComponentRef, ComponentRef> Edit) : AttributeEdit;

    public sealed record DisplayModeOverride(Option<Guid> Viewport, Option<Guid> Mode) : AttributeEdit;

    public sealed record HideInDetail(RowsEdit<Guid, Guid> Edit) : AttributeEdit;

    public sealed record ActiveInViewport(RowsEdit<Guid, Guid> Edit, bool Active) : AttributeEdit;

    public sealed record SectionStyleIndex(Option<ComponentRef> Address) : AttributeEdit;

    public sealed record HatchBoundaryPlotWeight(Option<Document.PlotWeight> Weight) : AttributeEdit;

    public sealed record PlaneFrame(Plane Value) : AttributeEdit;

    public sealed record TransformFrame(Transform Value) : AttributeEdit;

    public sealed record CustomMeshing(Option<MeshingParameters> Parameters, bool Enabled) : AttributeEdit;

    public sealed record Decals(RowsEdit<DecalCreateParams, int> Edit) : AttributeEdit;

    public sealed record UserStrings(UserStringEdit Edit) : AttributeEdit;

    public sealed record MaterialRefs(RowsEdit<MaterialRefCreateParams, Guid> Edit) : AttributeEdit;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper]
internal static partial class AttributeMapper {
    [MapProperty(nameof(ObjectAttributes.ObjectId), nameof(AttributeState.Id))]
    [MapProperty(nameof(ObjectAttributes.LinetypeIndex), nameof(AttributeState.Linetype), Use = nameof(@LinetypeRef.FromHost))]
    [MapProperty(nameof(ObjectAttributes.PlotWeight), nameof(AttributeState.PlotWeight), Use = nameof(@PlotWeight.FromHost))]
    [MapProperty(nameof(ObjectAttributes.HatchBoundaryPlotWeightMillimeters), nameof(AttributeState.HatchBoundaryPlotWeight), Use = nameof(@PlotWeight.FromInheritable))]
    [MapPropertyFromSource(nameof(AttributeState.HiddenInDetails), Use = nameof(HiddenInDetails))]
    [MapPropertyFromSource(nameof(AttributeState.ActiveOverrides), Use = nameof(ActiveOverrides))]
    [MapPropertyFromSource(nameof(AttributeState.HasCustomSectionStyle), Use = nameof(HasCustomSectionStyle))]
    [MapPropertyFromSource(nameof(AttributeState.HasCustomLinetype), Use = nameof(HasCustomLinetype))]
    [MapPropertyFromSource(nameof(AttributeState.CustomMeshing), Use = nameof(CustomMeshing))]
    [MapProperty(nameof(@ObjectAttributes.File3dmMeshModifiers.Displacement), nameof(AttributeState.HasDisplacement), Use = nameof(Held))]
    [MapProperty(nameof(@ObjectAttributes.File3dmMeshModifiers.EdgeSoftening), nameof(AttributeState.HasEdgeSoftening), Use = nameof(Held))]
    [MapProperty(nameof(@ObjectAttributes.File3dmMeshModifiers.Thickening), nameof(AttributeState.HasThickening), Use = nameof(Held))]
    [MapProperty(nameof(@ObjectAttributes.File3dmMeshModifiers.CurvePiping), nameof(AttributeState.HasCurvePiping), Use = nameof(Held))]
    [MapProperty(nameof(@ObjectAttributes.File3dmMeshModifiers.ShutLining), nameof(AttributeState.HasShutLining), Use = nameof(Held))]
    [MapPropertyFromSource(nameof(AttributeState.Groups), Use = nameof(Groups))]
    [MapPropertyFromSource(nameof(AttributeState.ObjectFrame), Use = nameof(ObjectFrame))]
    [MapPropertyFromSource(nameof(AttributeState.Decals), Use = nameof(Decals))]
    [MapPropertyFromSource(nameof(AttributeState.MaterialRefs), Use = nameof(MaterialRefs))]
    internal static partial AttributeState ToState(ObjectAttributes a, HashMap<string, string> userStrings);

    [MapProperty(nameof(Decal.CRC), nameof(DecalState.Crc))]
    [MapPropertyFromSource(nameof(DecalState.HorzSweep), Use = nameof(HorzSweep))]
    [MapPropertyFromSource(nameof(DecalState.VertSweep), Use = nameof(VertSweep))]
    [MapPropertyFromSource(nameof(DecalState.UvBounds), Use = nameof(UvBounds))]
    internal static partial DecalState ToState(Decal decal);

    internal static partial MaterialRefState ToState(MaterialRef row);

    private static Seq<Guid> HiddenInDetails(ObjectAttributes a) => toSeq(a.GetHideInDetailOverrides());

    private static Option<(Seq<Guid> Viewports, bool Active)> ActiveOverrides(ObjectAttributes a) =>
        Answers.Found(a.GetActiveInViewportOverrides(out Guid[] viewports, out bool active), (Viewports: toSeq(viewports), Active: active));

    private static bool HasCustomSectionStyle(ObjectAttributes a) => DisposalOps.Present(a.GetCustomSectionStyle());

    private static bool HasCustomLinetype(ObjectAttributes a) => DisposalOps.Present(a.GetCustomLinetype());

    private static Option<MeshingParameters> CustomMeshing(ObjectAttributes a) => Optional(a.CustomMeshingParameters);

    private static bool Held<T>(T? modifier) where T : class => modifier is not null;

    private static Seq<int> Groups(ObjectAttributes a) => toSeq(a.GetGroupList());

    private static Option<Plane> ObjectFrame(ObjectAttributes a) => Some(a.ObjectFrame()).Filter(static plane => plane.IsValid);

    private static Seq<DecalState> Decals(ObjectAttributes a) => toSeq(a.Decals).Map(static decal => ToState(decal)).Strict();

    private static Seq<MaterialRefState> MaterialRefs(ObjectAttributes a) => toSeq(a.MaterialRefs.Values).Map(static row => ToState(row)).Strict();

    private static (double Start, double End) HorzSweep(Decal decal) {
        decal.HorzSweep(out double start, out double end);
        return (start, end);
    }

    private static (double Start, double End) VertSweep(Decal decal) {
        decal.VertSweep(out double start, out double end);
        return (start, end);
    }

    private static (double MinU, double MinV, double MaxU, double MaxV) UvBounds(Decal decal) {
        decal.GetUVBounds(out double minU, out double minV, out double maxU, out double maxV);
        return (minU, minV, maxU, maxV);
    }
}

public static class AttributeOps {
    // --- [READS]
    public static IO<AttributeState> ReadAttributes(ObjectAttributes a) =>
        GeometryOps.ReadUserStrings(GeometryOps.UserStrings(a)).Map(userStrings => AttributeMapper.ToState(a, userStrings));

    public static IO<EffectiveDisplay> ReadEffective(ObjectAttributes a, RhinoDoc doc, Option<Guid> viewport) =>
        IO.lift(() => viewport.Match(
            Some: id => new EffectiveDisplay(
                a.DrawColor(doc, id),
                a.ComputedPlotColor(doc, id),
                PlotWeight.FromHost(a.ComputedPlotWeight(doc, id)),
                Answers.Present(a.GetDisplayModeOverride(id)),
                Answers.Found(a.HasActiveInViewportOverride(id, out bool active), active)),
            None: () => new EffectiveDisplay(
                a.DrawColor(doc),
                a.ComputedPlotColor(doc),
                PlotWeight.FromHost(a.ComputedPlotWeight(doc)),
                Answers.Present(a.GetDisplayModeOverride(Guid.Empty)),
                Option<bool>.None)));

    // --- [EDITS]
    public static IO<Unit> Apply(RhinoDoc doc, ObjectAttributes a, AttributeEdit edit) =>
        edit.Switch(
            (Doc: doc, Target: a),
            name: static (state, name) => IO.lift(() => { state.Target.Name = Answers.Unset(name.Value); }),
            url: static (state, url) => IO.lift(() => { state.Target.Url = Answers.Unset(url.Value); }),
            layerIndex: static (state, layer) =>
                from row in Layers.ResolveLayer(state.Doc, layer.Address, includeDeleted: false)
                from written in IO.lift(() => { state.Target.LayerIndex = row.Index; })
                select written,
            set: static (state, set) => IO.lift(() => set.Write(state.Target)),
            plotWeight: static (state, plot) => IO.lift(() => {
                state.Target.PlotWeightSource = plot.Source;
                _ = plot.Weight.Iter(weight => state.Target.PlotWeight = weight.ToHost());
            }),
            linetype: static (state, linetype) =>
                from index in linetype.Pattern.Resolve(state.Doc)
                from written in IO.lift(() => {
                    state.Target.LinetypeSource = linetype.Source;
                    state.Target.LinetypeIndex = index;
                })
                select written,
            materialIndex: static (state, material) =>
                from index in TableOps.Index(state.Doc.Materials, material.Address)
                from written in IO.lift(() => {
                    state.Target.MaterialSource = material.Source;
                    state.Target.MaterialIndex = index;
                })
                select written,
            renderMaterial: static (state, render) =>
                from owned in IO.lift(() => Invalid.Unless(
                    render.Material.DocumentOwner?.RuntimeSerialNumber == state.Doc.RuntimeSerialNumber,
                    nameof(RenderContent.DocumentOwner)))
                from written in IO.lift(() => {
                    state.Target.MaterialIndex = Answers.Unset(toSeq(state.Doc.Materials).Find(row => !row.IsDeleted && row.RenderMaterialInstanceId == render.Material.Id).Map(static row => row.Index));
                    state.Target.RenderMaterial = render.Material;
                })
                select written,
            wireDensity: static (state, wires) =>
                from valid in IO.lift(() => Limits.AtLeast(-1).Check(wires.Density, nameof(ObjectAttributes.WireDensity)))
                from written in IO.lift(() => { state.Target.WireDensity = valid; })
                select written,
            space: static (state, space) => IO.lift(() => {
                state.Target.Space = space.Value;
                state.Target.ViewportId = Answers.Unset(space.ViewportId);
            }),
            groups: static (state, groups) => Edited(
                groups.Edit,
                IO.lift(state.Target.RemoveFromAllGroups),
                address => from row in TableOps.Find(state.Doc.Groups, address, includeDeleted: false)
                           from added in IO.lift(() => state.Target.AddToGroup(row.Index))
                           select added,
                address => from row in TableOps.Find(state.Doc.Groups, address, includeDeleted: false)
                           from removed in IO.lift(() => state.Target.RemoveFromGroup(row.Index))
                           select removed),
            displayModeOverride: static (state, mode) => mode.Mode.Case is Guid id
                ? Viewports.WithMode(id, description => IO.lift(() => Refused.Unless(
                    mode.Viewport.Case is Guid viewport ? state.Target.SetDisplayModeOverride(description, viewport) : state.Target.SetDisplayModeOverride(description),
                    nameof(ObjectAttributes.SetDisplayModeOverride))))
                : IO.lift(() => mode.Viewport.Match(Some: state.Target.RemoveDisplayModeOverride, None: state.Target.RemoveDisplayModeOverride)),
            hideInDetail: static (state, hide) =>
                from present in IO.lift(() => toSet(state.Target.GetHideInDetailOverrides()))
                from changed in hide.Edit.Switch(
                    (state.Target, Present: present),
                    replace: static (held, replace) => Hidden(held.Target, toSet(replace.Rows).Except(held.Present).ToSeq(), held.Present.Except(replace.Rows).ToSeq()),
                    add: static (held, add) => Hidden(held.Target, toSet(add.Rows).Except(held.Present).ToSeq(), Seq<Guid>()),
                    remove: static (held, remove) => Hidden(held.Target, Seq<Guid>(), held.Present.Intersect(remove.Keys).ToSeq()))
                select changed,
            activeInViewport: static (state, active) => IO.lift(() => active.Edit.Switch(
                (state.Target, active.Active),
                replace: static (held, replace) => Refused.Unless(held.Target.SetActiveInViewportOverrides([.. replace.Rows], held.Active), nameof(ObjectAttributes.SetActiveInViewportOverrides)),
                add: static (held, add) => Answers.Each(add.Rows, id => held.Target.AddActiveInViewportOverride(id, held.Active), nameof(ObjectAttributes.AddActiveInViewportOverride)),
                remove: static (held, remove) => Answers.Each(
                    remove.Keys,
                    id => !held.Target.HasActiveInViewportOverride(id, out bool stored) || held.Target.RemoveActiveInViewportOverride(id, stored),
                    nameof(ObjectAttributes.RemoveActiveInViewportOverride)))),
            sectionStyleIndex: static (state, style) =>
                from index in TableOps.Index(state.Doc.SectionStyles, style.Address)
                from written in IO.lift(() => { state.Target.SectionStyleIndex = index; })
                select written,
            hatchBoundaryPlotWeight: static (state, boundary) => IO.lift(() => { state.Target.HatchBoundaryPlotWeightMillimeters = PlotWeight.ToInheritable(boundary.Weight); }),
            planeFrame: static (state, frame) =>
                from valid in IO.lift(() => Invalid.Unless(frame.Value.IsValid, nameof(frame)))
                from written in IO.lift(() => state.Target.SetObjectFrame(frame.Value))
                select written,
            transformFrame: static (state, frame) =>
                from valid in IO.lift(() => Invalid.Unless(frame.Value.IsValid, nameof(frame)))
                from written in IO.lift(() => state.Target.SetObjectFrame(frame.Value))
                select written,
            customMeshing: static (state, meshing) => IO.lift(() => {
                state.Target.CustomMeshingParameters = meshing.Parameters.ValueUnsafe();
                state.Target.EnableCustomMeshingParameters = meshing.Enabled;
            }),
            decals: static (state, decals) => Edited(
                decals.Edit,
                IO.lift(state.Target.Decals.RemoveAllDecals),
                row => DisposalOps.Using(
                    IO.lift(() => Missing.Unless(Decal.Create(row), nameof(Decal.Create))),
                    decal => IO.lift(() => Refused.Unless(state.Target.Decals.Add(decal) != 0u, nameof(Decals.Add)))),
                crc => IO.lift(() => Answers.Each(toSeq(state.Target.Decals).Filter(decal => decal.CRC == crc).Strict(), state.Target.Decals.Remove, nameof(Decals.Remove)))),
            userStrings: static (state, strings) => GeometryOps.EditUserStrings(GeometryOps.UserStrings(state.Target), strings.Edit),
            materialRefs: static (state, refs) => Edited(
                refs.Edit,
                IO.lift(state.Target.MaterialRefs.Clear),
                row => from keyed in IO.lift(() => Answers.Present(row.PlugInId).ToFin(new Invalid(nameof(MaterialRefCreateParams.PlugInId))))
                       from added in DisposalOps.Using(() => state.Target.MaterialRefs.Create(row), staged => IO.lift(() => state.Target.MaterialRefs.Add(keyed, staged)))
                       select added,
                key => IO.lift(() => { _ = state.Target.MaterialRefs.Remove(key); })));

    private static IO<Unit> Edited<TRow, TKey>(RowsEdit<TRow, TKey> edit, IO<Unit> clear, Func<TRow, IO<Unit>> add, Func<TKey, IO<Unit>> remove) =>
        edit.Switch(
            (Clear: clear, Add: add, Remove: remove),
            replace: static (accessors, replace) => accessors.Clear.Bind(_ => replace.Rows.TraverseM(accessors.Add).As()).Map(static _ => unit),
            add: static (accessors, added) => added.Rows.TraverseM(accessors.Add).As().Map(static _ => unit),
            remove: static (accessors, removed) => removed.Keys.TraverseM(accessors.Remove).As().Map(static _ => unit));

    private static IO<Unit> Hidden(ObjectAttributes a, Seq<Guid> add, Seq<Guid> remove) =>
        IO.lift(() => Answers.Each(remove, a.RemoveHideInDetailOverride, nameof(ObjectAttributes.RemoveHideInDetailOverride))
            .Bind(_ => Answers.Each(add, a.AddHideInDetailOverride, nameof(ObjectAttributes.AddHideInDetailOverride))));

    // --- [COMMITS]
    public static IO<Seq<Guid>> Modify(RhinoDoc doc, ObjectTarget target, Seq<AttributeEdit> edits, bool quiet) =>
        TableOps.Apply(doc, new TableOp.ModifyAttributes(target, a => edits.TraverseM(edit => Apply(doc, a, edit)).As().Map(static _ => unit), quiet));
}
