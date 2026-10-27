using System.Drawing;
using LanguageExt.ClassInstances;
using Rasm.Rhino.Display;
using Rasm.Rhino.Document.Tables;
using Rasm.Rhino.Persistence;
using Rasm.Rhino.Viewport;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.FileIO;
using Rhino.Render;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Objects;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum(SwitchMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class MeshModifier {
    // --- [KINDS]
    public static readonly MeshModifier Displacement = new(static modifiers => modifiers.Displacement is not null);
    public static readonly MeshModifier EdgeSoftening = new(static modifiers => modifiers.EdgeSoftening is not null);
    public static readonly MeshModifier Thickening = new(static modifiers => modifiers.Thickening is not null);
    public static readonly MeshModifier CurvePiping = new(static modifiers => modifiers.CurvePiping is not null);
    public static readonly MeshModifier ShutLining = new(static modifiers => modifiers.ShutLining is not null);

    // --- [PRESENCE]
    [UseDelegateFromConstructor]
    public partial bool IsAttached(File3dmMeshModifiers modifiers);
}

public sealed record DecalState(
    int Crc, bool IsVisible, Guid TextureInstanceId, DecalMapping Mapping, DecalProjection Projection, bool MapToInside, double Transparency,
    Point3d Origin, Vector3d VectorUp, Vector3d VectorAcross, double Height, double Radius,
    Interval HorzSweep, Interval VertSweep, (Interval U, Interval V) UvBounds);

public sealed record MaterialRefState(
    Guid PlugInId, ObjectMaterialSource MaterialSource,
    Option<Guid> FrontFaceMaterialId, Option<Guid> BackFaceMaterialId, Option<int> FrontFaceMaterialIndex, Option<int> BackFaceMaterialIndex);

public sealed record AttributeState(
    Guid Id, Option<string> Name, Option<string> Url, int LayerIndex, ObjectMode Mode, bool Visible, bool IsInstanceDefinitionObject,
    ActiveSpace Space, Option<Guid> ViewportId,
    ObjectColorSource ColorSource, Color ObjectColor, ObjectPlotColorSource PlotColorSource, Color PlotColor,
    ObjectPlotWeightSource PlotWeightSource, PlotWeight PlotWeight,
    ObjectLinetypeSource LinetypeSource, Option<LinetypeRef> Linetype, double LinetypePatternScale, bool HasCustomLinetype,
    ObjectMaterialSource MaterialSource, Option<int> MaterialIndex,
    ObjectSectionAttributesSource SectionAttributesSource, Option<int> SectionStyleIndex, SectionLabelStyle ClippingPlaneLabelStyle, bool HasCustomSectionStyle,
    bool CastsShadows, bool ReceivesShadows, int WireDensity, int DisplayOrder, ObjectDecoration ObjectDecoration,
    Seq<int> Groups, Seq<Guid> HiddenInDetails, Option<(Seq<Guid> Viewports, bool Active)> ActiveInViewports, bool DetailBackgroundVisible,
    Color HatchBackgroundFillColor, Color HatchBackgroundFillPrintColor, bool HatchBoundaryVisible, Color HatchBoundaryColor, Color HatchBoundaryPlotColor,
    ItemColorSource HatchBoundaryColorSource, ItemColorSource HatchBoundaryPlotColorSource, Option<PlotWeight> HatchBoundaryPlotWeight,
    Option<Plane> ObjectFrame, bool HasCustomMeshing, bool EnableCustomMeshingParameters, Seq<MeshModifier> MeshModifiers, bool HasMapping,
    Seq<DecalState> Decals, Seq<MaterialRefState> MaterialRefs, HashMap<EqStringOrdinalIgnoreCase, string, string> UserStrings);

public sealed record EffectiveDisplay(Color DrawColor, Color ComputedPlotColor, PlotWeight ComputedPlotWeight, Option<Guid> DisplayModeOverride, Option<bool> ActiveInViewport);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record RowsEdit<TRow, TKey> {
    public sealed record Replace(Seq<TRow> Rows) : RowsEdit<TRow, TKey>;

    public sealed record Add(Seq<TRow> Rows) : RowsEdit<TRow, TKey>;

    public sealed record Remove(Seq<TKey> Keys) : RowsEdit<TRow, TKey>;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper]
internal static partial class AttributeMapper {
    // --- [SNAPSHOTS]
    [MapProperty(nameof(ObjectAttributes.ObjectId), nameof(AttributeState.Id))]
    [MapProperty(nameof(ObjectAttributes.LinetypeIndex), nameof(AttributeState.Linetype), Use = nameof(@LinetypeRef.FromHost))]
    [MapProperty(nameof(ObjectAttributes.PlotWeight), nameof(AttributeState.PlotWeight), Use = nameof(@PlotWeight.FromHost))]
    [MapProperty(nameof(ObjectAttributes.HatchBoundaryPlotWeightMillimeters), nameof(AttributeState.HatchBoundaryPlotWeight), Use = nameof(@PlotWeight.FromInheritable))]
    [MapPropertyFromSource(nameof(AttributeState.HasCustomLinetype), Use = nameof(HasCustomLinetype))]
    [MapPropertyFromSource(nameof(AttributeState.HasCustomSectionStyle), Use = nameof(HasCustomSectionStyle))]
    [MapPropertyFromSource(nameof(AttributeState.HasCustomMeshing), Use = nameof(HasCustomMeshing))]
    [MapPropertyFromSource(nameof(AttributeState.Groups), Use = nameof(Groups))]
    [MapPropertyFromSource(nameof(AttributeState.HiddenInDetails), Use = nameof(HiddenInDetails))]
    [MapPropertyFromSource(nameof(AttributeState.ActiveInViewports), Use = nameof(ActiveInViewports))]
    [MapPropertyFromSource(nameof(AttributeState.ObjectFrame), Use = nameof(ObjectFrame))]
    [MapPropertyFromSource(nameof(AttributeState.MeshModifiers), Use = nameof(MeshModifiers))]
    [MapPropertyFromSource(nameof(AttributeState.MaterialRefs), Use = nameof(MaterialRefs))]
    internal static partial AttributeState ToState(ObjectAttributes attributes, HashMap<EqStringOrdinalIgnoreCase, string, string> userStrings, Seq<DecalState> decals);

    [MapProperty(nameof(Decal.CRC), nameof(DecalState.Crc))]
    [MapPropertyFromSource(nameof(DecalState.HorzSweep), Use = nameof(HorzSweep))]
    [MapPropertyFromSource(nameof(DecalState.VertSweep), Use = nameof(VertSweep))]
    [MapPropertyFromSource(nameof(DecalState.UvBounds), Use = nameof(UvBounds))]
    internal static partial DecalState ToState(Decal decal);

    internal static partial MaterialRefState ToState(MaterialRef row);

    // --- [MATERIALS]
    [MapProperty(nameof(MaterialRefState.FrontFaceMaterialId), nameof(MaterialRefCreateParams.BackFaceMaterialId))]
    [MapProperty(nameof(MaterialRefState.BackFaceMaterialId), nameof(MaterialRefCreateParams.FrontFaceMaterialId))]
    [MapProperty(nameof(MaterialRefState.FrontFaceMaterialIndex), nameof(MaterialRefCreateParams.BackFaceMaterialIndex))]
    [MapProperty(nameof(MaterialRefState.BackFaceMaterialIndex), nameof(MaterialRefCreateParams.FrontFaceMaterialIndex))]
    internal static partial MaterialRefCreateParams CrossedFaces(MaterialRefState row);

    // --- [ATTRIBUTES]
    [UserMapping(Default = false)]
    private static bool HasCustomLinetype(ObjectAttributes attributes) {
        using Linetype? held = attributes.GetCustomLinetype();
        return held is not null;
    }

    [UserMapping(Default = false)]
    private static bool HasCustomSectionStyle(ObjectAttributes attributes) {
        using SectionStyle? held = attributes.GetCustomSectionStyle();
        return held is not null;
    }

    [UserMapping(Default = false)]
    private static bool HasCustomMeshing(ObjectAttributes attributes) {
        using MeshingParameters? held = attributes.CustomMeshingParameters;
        return held is not null;
    }

    [UserMapping(Default = false)]
    private static Seq<int> Groups(ObjectAttributes attributes) => Conversions.Rows(attributes.GetGroupList());

    [UserMapping(Default = false)]
    private static Seq<Guid> HiddenInDetails(ObjectAttributes attributes) => Conversions.Rows(attributes.GetHideInDetailOverrides());

    [UserMapping(Default = false)]
    private static Option<(Seq<Guid> Viewports, bool Active)> ActiveInViewports(ObjectAttributes attributes) =>
        Conversions.Found(attributes.GetActiveInViewportOverrides(out Guid[] ids, out bool active), (Viewports: Conversions.Rows(ids), Active: active));

    [UserMapping(Default = false)]
    private static Option<Plane> ObjectFrame(ObjectAttributes attributes) => Conversions.Present(attributes.ObjectFrame());

    [UserMapping(Default = false)]
    private static Seq<MeshModifier> MeshModifiers(ObjectAttributes attributes) =>
        toSeq(MeshModifier.Items).Filter(modifier => modifier.IsAttached(attributes.File3dmMeshModifiers)).Strict();

    [UserMapping(Default = false)]
    private static Seq<MaterialRefState> MaterialRefs(ObjectAttributes attributes) =>
        Conversions.Rows(attributes.MaterialRefs.Values).Map(static row => ToState(row)).Strict();

    // --- [DECALS]
    [UserMapping(Default = false)]
    private static Interval HorzSweep(Decal decal) {
        decal.HorzSweep(out double start, out double end);
        return new(start, end);
    }

    [UserMapping(Default = false)]
    private static Interval VertSweep(Decal decal) {
        decal.VertSweep(out double start, out double end);
        return new(start, end);
    }

    [UserMapping(Default = false)]
    private static (Interval U, Interval V) UvBounds(Decal decal) {
        decal.GetUVBounds(out double minU, out double minV, out double maxU, out double maxV);
        return (new(minU, maxU), new(minV, maxV));
    }
}

public static class AttributeEdits {
    // --- [REFERENCES]
    public static Func<RhinoDoc, ObjectAttributes, IO<Unit>> LayerIndex(LayerRef address) =>
        (doc, staged) => from layer in Layers.Resolve(doc, address)
                         from written in IO.lift(() => { staged.LayerIndex = layer.Index; })
                         select written;

    public static Func<RhinoDoc, ObjectAttributes, IO<Unit>> LinetypeIndex(LinetypeRef address) =>
        (doc, staged) => from index in address.Resolve(doc)
                         from written in IO.lift(() => { staged.LinetypeIndex = index; })
                         select written;

    public static Func<RhinoDoc, ObjectAttributes, IO<Unit>> MaterialIndex(Option<ComponentRef<Material>> address) =>
        (doc, staged) => from index in TableOps.Index(doc.Materials, address)
                         from written in IO.lift(() => { staged.MaterialIndex = index; })
                         select written;

    public static Func<RhinoDoc, ObjectAttributes, IO<Unit>> SectionStyleIndex(Option<ComponentRef<SectionStyle>> address) =>
        (doc, staged) => from index in TableOps.Index(doc.SectionStyles, address)
                         from written in IO.lift(() => { staged.SectionStyleIndex = index; })
                         select written;

    public static IO<Unit> CustomLinetype(ObjectAttributes staged, Option<Linetype> value) => IO.lift(() => staged.SetCustomLinetype(value.ValueUnsafe()));

    public static IO<Unit> CustomSectionStyle(ObjectAttributes staged, Option<SectionStyle> value) => IO.lift(() => staged.SetCustomSectionStyle(value.ValueUnsafe()));

    // --- [METADATA]
    public static Func<RhinoDoc, ObjectAttributes, IO<Unit>> SetUserStrings(HashMap<EqStringOrdinalIgnoreCase, string, Option<string>> edits) =>
        (_, staged) => from held in IO.lift(() => UserStrings.Held(staged.GetUserStrings()))
                       from written in UserStrings.Write(held, staged.SetUserString, edits)
                       select written;

    public static Func<RhinoDoc, ObjectAttributes, IO<Unit>> Groups(RowsEdit<ComponentRef<Group>, ComponentRef<Group>> edit) =>
        (doc, staged) => Edited(edit, IO.lift(() => staged.RemoveFromAllGroups()),
            Membership(doc, staged.AddToGroup), keys => keys.TraverseM(Membership(doc, staged.RemoveFromGroup)).As().Map(unit))
            .Map(static _ => unit);

    private static Func<ComponentRef<Group>, IO<Unit>> Membership(RhinoDoc doc, Action<int> write) =>
        address => from found in TableOps.Find(doc.Groups, address, includeDeleted: false)
                   from written in IO.lift(() => write(found.Index))
                   select written;

    // --- [OVERRIDES]
    public static Func<RhinoDoc, ObjectAttributes, IO<Unit>> DisplayModeOverride(Option<ViewportTarget> viewport, Option<Guid> mode) =>
        (doc, staged) => Slot(doc, viewport).Bind(slot => DisplayModeOverride(staged, slot, mode));

    internal static IO<Unit> DisplayModeOverride(ObjectAttributes staged, Guid slot, Option<Guid> mode) =>
        mode.Match(
            Some: id => DisplayModes.With(id, description =>
                from allowed in IO.lift(() => Assignable(description))
                from written in IO.lift(() => { _ = staged.SetDisplayModeOverride(description, slot); })
                select written),
            None: () => slot == Guid.Empty
                ? IO.lift(() => DisplayMaterialRefs.AllViewportsRemoved(staged, Guid.NewGuid()))
                : IO.lift(() => staged.RemoveDisplayModeOverride(slot)));

    internal static IO<Guid> Slot(RhinoDoc doc, Option<ViewportTarget> viewport) =>
        viewport.Match(Some: target => Viewports.With(doc, target, static row => IO.pure(row.Viewport.Id)), None: static () => IO.pure(Guid.Empty));

    internal static Fin<Unit> Assignable(DisplayModeDescription mode) => RhinoFault.Unless(mode.AllowObjectAssignment);

    public static IO<Unit> ActiveInViewport(ObjectAttributes staged, RowsEdit<Guid, Guid> edit, bool active) =>
        IO.lift(() => edit.Switch(
            (Staged: staged, Active: active),
            replace: static (state, replace) => RhinoFault.Unless(state.Staged.SetActiveInViewportOverrides([.. replace.Rows], state.Active)),
            add: static (state, add) => Callbacks.Each(add.Rows, Fin<Unit> (id, index) => state.Staged.HasActiveInViewportOverride(id, out _)
                ? unit
                : RhinoFault.Unless(state.Staged.AddActiveInViewportOverride(id, state.Active), index)).Map(static _ => unit),
            remove: static (state, remove) => Callbacks.Each(remove.Keys, Fin<Unit> (id, index) => state.Staged.HasActiveInViewportOverride(id, out bool stored)
                ? RhinoFault.Unless(state.Staged.RemoveActiveInViewportOverride(id, stored), index)
                : unit).Map(static _ => unit)));

    public static IO<Unit> HideInDetail(ObjectAttributes staged, RowsEdit<Guid, Guid> edit) =>
        IO.lift(() => edit.Switch(
                toSet(staged.GetHideInDetailOverrides()),
                replace: static (held, replace) => (Hide: toSeq(toSet(replace.Rows).Except(held)), Show: toSeq(held.Except(replace.Rows))),
                add: static (held, add) => (Hide: toSeq(toSet(add.Rows).Except(held)), Show: Seq<Guid>()),
                remove: static (held, remove) => (Hide: Seq<Guid>(), Show: toSeq(held.Intersect(remove.Keys))))
            switch {
                var (hide, show) =>
                    (Callbacks.Each(show, (id, index) => RhinoFault.Unless(staged.RemoveHideInDetailOverride(id), index)),
                     Callbacks.Each(hide, (id, index) => RhinoFault.Unless(staged.AddHideInDetailOverride(id), index)))
                        .Apply(static (_, _) => unit).As(),
            });

    // --- [FRAME]
    public static IO<Unit> ObjectFrame(ObjectAttributes staged, Plane frame) =>
        from valid in IO.lift(() => RhinoFault.Unless(frame.IsValid))
        from written in IO.lift(() => staged.SetObjectFrame(frame))
        select written;

    public static IO<Unit> Transform(ObjectAttributes staged, Transform xform) =>
        IO.lift(() => RhinoFault.Unless(staged.Transform(xform)));

    // --- [RENDERING]
    public static IO<Unit> CustomMeshingParameters(ObjectAttributes staged, Option<MeshingParameters> value) =>
        IO.lift(() => { staged.CustomMeshingParameters = value.ValueUnsafe(); });

    public static IO<Seq<uint>> Decals(ObjectAttributes staged, RowsEdit<DecalCreateParams, int> edit) =>
        Edited(edit, IO.lift(staged.Decals.RemoveAllDecals),
            row => (from decal in Products.Made(Decal.Create(row))
                    from crc in IO.lift(() => Conversions.Found(Conversions.Present(staged.Decals.Add(decal))))
                    select crc).Bracket(),
            keys => toHashSet(keys) switch {
                var crcs => (from decals in Products.Made(staged.Decals)
                             from removed in IO.lift(() => Callbacks.Each(decals.Filter(decal => crcs.Contains(decal.CRC)).Strict(),
                                 (decal, index) => RhinoFault.Unless(staged.Decals.Remove(decal), index)))
                             select unit).Bracket(),
            });

    public static IO<Unit> MaterialRefs(ObjectAttributes staged, RowsEdit<MaterialRefState, Guid> edit) =>
        Edited(edit, IO.lift(staged.MaterialRefs.Clear),
            row => (from created in use(() => staged.MaterialRefs.Create(AttributeMapper.CrossedFaces(row)))
                    from added in IO.lift(() => staged.MaterialRefs.Add(row.PlugInId, created))
                    select added).Bracket(),
            keys => IO.lift(() => keys.Iter(key => staged.MaterialRefs.Remove(key))))
            .Map(unit);

    private static IO<Seq<TAdded>> Edited<TRow, TKey, TAdded>(RowsEdit<TRow, TKey> edit, IO<Unit> clear, Func<TRow, IO<TAdded>> add, Func<Seq<TKey>, IO<Unit>> remove) =>
        edit.Switch(
            state: (Clear: clear, Add: add, Remove: remove),
            replace: static (state, replace) =>
                from cleared in state.Clear
                from added in replace.Rows.TraverseM(state.Add).As()
                select added,
            add: static (state, added) => added.Rows.TraverseM(state.Add).As(),
            remove: static (state, removed) => state.Remove(removed.Keys).Map(Seq<TAdded>()));
}

public static class AttributeOps {
    // --- [READS]
    public static IO<AttributeState> Read(ObjectAttributes attributes) =>
        (from decals in Products.Made(attributes.Decals)
         select AttributeMapper.ToState(attributes, UserStrings.Held(attributes.GetUserStrings()), decals.Map(static decal => AttributeMapper.ToState(decal)).Strict())).Bracket();

    public static IO<EffectiveDisplay> ReadEffective(ObjectAttributes attributes, RhinoDoc doc, Guid viewport) =>
        IO.lift(() => new EffectiveDisplay(
            attributes.DrawColor(doc, viewport),
            attributes.ComputedPlotColor(doc, viewport),
            PlotWeight.FromHost(attributes.ComputedPlotWeight(doc, viewport)),
            Conversions.Present(attributes.GetDisplayModeOverride(viewport)),
            Conversions.Found(attributes.HasActiveInViewportOverride(viewport, out bool active), active)));

    public static IO<T> WithComputedSectionStyle<T>(ObjectAttributes attributes, RhinoDoc doc, ObjectAttributes sectioner, bool computeColors, Guid viewport, Func<SectionStyle, IO<T>> body) =>
        Products.Made(attributes.ComputedSectionStyle(doc, sectioner, computeColors, viewport)).Bind(body).Bracket();

    // --- [FOLD]
    public static IO<Unit> Apply(RhinoDoc doc, ObjectAttributes staged, Seq<Func<RhinoDoc, ObjectAttributes, IO<Unit>>> edits) =>
        edits.TraverseM(edit => edit(doc, staged)).As().Map(unit);

    public static TableOp Modify(RhinoDoc doc, ObjectTarget target, Seq<Func<RhinoDoc, ObjectAttributes, IO<Unit>>> edits, bool quiet) =>
        new TableOp.ModifyAttributes(target, staged => Apply(doc, staged, edits), quiet);
}
