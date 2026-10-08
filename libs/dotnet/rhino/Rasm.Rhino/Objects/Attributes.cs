using System.Drawing;
using LanguageExt.ClassInstances;
using Rasm.Rhino.Document;
using Rasm.Rhino.Document.Tables;
using Rhino;
using Rhino.DocObjects;
using Rhino.FileIO;
using Rhino.Render;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Objects;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
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

[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct WireDensity : System.Numerics.IMinMaxValue<WireDensity> {
    // --- [LIMITS]
    public static WireDensity MinValue { get; } = new(-1);
    public static WireDensity MaxValue { get; } = new(int.MaxValue);

    // --- [FACTORY]
    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref int value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 ? null : new InvalidRhinoValue();
}

public sealed record DecalState(
    int Crc, bool IsVisible, Guid TextureInstanceId, DecalMapping Mapping, DecalProjection Projection, bool MapToInside, double Transparency,
    Point3d Origin, Vector3d VectorUp, Vector3d VectorAcross, double Height, double Radius,
    (double Start, double End) HorzSweep, (double Start, double End) VertSweep, (double MinU, double MinV, double MaxU, double MaxV) UvBounds);

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

public sealed record AttributePatch(
    Option<Option<string>> Name = default, Option<Option<string>> Url = default,
    Option<ObjectColorSource> ColorSource = default, Option<Color> ObjectColor = default,
    Option<ObjectPlotColorSource> PlotColorSource = default, Option<Color> PlotColor = default,
    Option<ObjectPlotWeightSource> PlotWeightSource = default, Option<PlotWeight> PlotWeight = default,
    Option<ObjectLinetypeSource> LinetypeSource = default, Option<double> LinetypePatternScale = default,
    Option<ObjectMaterialSource> MaterialSource = default, Option<ObjectSectionAttributesSource> SectionAttributesSource = default,
    Option<SectionLabelStyle> ClippingPlaneLabelStyle = default, Option<bool> CastsShadows = default, Option<bool> ReceivesShadows = default,
    Option<WireDensity> WireDensity = default, Option<int> DisplayOrder = default, Option<ObjectDecoration> ObjectDecoration = default,
    Option<ActiveSpace> Space = default, Option<Option<Guid>> ViewportId = default, Option<bool> DetailBackgroundVisible = default,
    Option<Color> HatchBackgroundFillColor = default, Option<Color> HatchBackgroundFillPrintColor = default,
    Option<bool> HatchBoundaryVisible = default, Option<Color> HatchBoundaryColor = default, Option<Color> HatchBoundaryPlotColor = default,
    Option<ItemColorSource> HatchBoundaryColorSource = default, Option<ItemColorSource> HatchBoundaryPlotColorSource = default,
    Option<Option<PlotWeight>> HatchBoundaryPlotWeight = default, Option<bool> EnableCustomMeshingParameters = default);

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
    internal static partial MaterialRefCreateParams ToParams(MaterialRefState row);

    // --- [ATTRIBUTES]
    [UserMapping(Default = false)]
    private static bool HasCustomLinetype(ObjectAttributes attributes) => DisposalOps.Present(attributes.GetCustomLinetype());

    [UserMapping(Default = false)]
    private static bool HasCustomSectionStyle(ObjectAttributes attributes) => DisposalOps.Present(attributes.GetCustomSectionStyle());

    [UserMapping(Default = false)]
    private static bool HasCustomMeshing(ObjectAttributes attributes) => DisposalOps.Present(attributes.CustomMeshingParameters);

    [UserMapping(Default = false)]
    private static Seq<int> Groups(ObjectAttributes attributes) => Conversions.Rows(attributes.GetGroupList());

    [UserMapping(Default = false)]
    private static Seq<Guid> HiddenInDetails(ObjectAttributes attributes) => Conversions.Rows(attributes.GetHideInDetailOverrides());

    [UserMapping(Default = false)]
    private static Option<(Seq<Guid> Viewports, bool Active)> ActiveInViewports(ObjectAttributes attributes) =>
        Callbacks.Found(attributes.GetActiveInViewportOverrides(out Guid[] ids, out bool active), (Viewports: Conversions.Rows(ids), Active: active));

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
    private static (double Start, double End) HorzSweep(Decal decal) {
        decal.HorzSweep(out double start, out double end);
        return (start, end);
    }

    [UserMapping(Default = false)]
    private static (double Start, double End) VertSweep(Decal decal) {
        decal.VertSweep(out double start, out double end);
        return (start, end);
    }

    [UserMapping(Default = false)]
    private static (double MinU, double MinV, double MaxU, double MaxV) UvBounds(Decal decal) {
        decal.GetUVBounds(out double minU, out double minV, out double maxU, out double maxV);
        return (minU, minV, maxU, maxV);
    }
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Source, AllowNullPropertyAssignment = false, AutoUserMappings = false)]
public static partial class AttributeEdits {
    // --- [PROPERTIES]
    [MapProperty(nameof(AttributePatch.HatchBoundaryPlotWeight), nameof(ObjectAttributes.HatchBoundaryPlotWeightMillimeters))]
    private static partial void UpdateFields(AttributePatch patch, ObjectAttributes staged);

    public static IO<Unit> Update(ObjectAttributes staged, AttributePatch patch, bool clearRendering = false) =>
        IO.lift(() => {
            if (clearRendering)
                staged.ClearRenderingAttributes();
            UpdateFields(patch, staged);
        });

    [UserMapping(Default = true)]
    private static int Integer(Option<int> value, [MappingTargetOriginalValue] int original) => value.IfNone(original);

    [UserMapping(Default = true)]
    private static double Scalar(Option<double> value, [MappingTargetOriginalValue] double original) => value.IfNone(original);

    [UserMapping(Default = true)]
    private static Color Tint(Option<Color> value, [MappingTargetOriginalValue] Color original) => value.IfNone(original);

    [UserMapping]
    private static T Retained<T>(Option<T> value, [MappingTargetOriginalValue] T original) => value.IfNone(original);

    [UserMapping]
    private static string Text(Option<Option<string>> value, [MappingTargetOriginalValue] string original) => value.Map(Conversions.Unset).IfNone(original);

    [UserMapping]
    private static Guid Viewport(Option<Option<Guid>> value, [MappingTargetOriginalValue] Guid original) => value.Map(Conversions.Unset).IfNone(original);

    [UserMapping]
    private static int Density(Option<WireDensity> value, [MappingTargetOriginalValue] int original) => value.Map(static density => (int)density).IfNone(original);

    [UserMapping]
    private static double Weight(Option<PlotWeight> value, [MappingTargetOriginalValue] double original) => value.Map(static weight => weight.ToHost()).IfNone(original);

    [UserMapping]
    private static double InheritedWeight(Option<Option<PlotWeight>> value, [MappingTargetOriginalValue] double original) => value.Map(PlotWeight.ToInheritable).IfNone(original);

    // --- [REFERENCES]
    public static Func<RhinoDoc, ObjectAttributes, IO<Unit>> LayerIndex(LayerRef address) =>
        (doc, staged) => from layer in Layers.Resolve(doc, address)
                         from written in IO.lift(() => Indices((layer.Index, None, None, None), staged))
                         select written;

    public static Func<RhinoDoc, ObjectAttributes, IO<Unit>> LinetypeIndex(LinetypeRef address) =>
        (doc, staged) => from index in address.Resolve(doc)
                         from written in IO.lift(() => Indices((None, index, None, None), staged))
                         select written;

    public static Func<RhinoDoc, ObjectAttributes, IO<Unit>> MaterialIndex(Option<ComponentRef<Material>> address) =>
        (doc, staged) => from index in TableOps.Index(doc.Materials, address)
                         from written in IO.lift(() => Indices((None, None, index, None), staged))
                         select written;

    public static Func<RhinoDoc, ObjectAttributes, IO<Unit>> SectionStyleIndex(Option<ComponentRef<SectionStyle>> address) =>
        (doc, staged) => from index in TableOps.Index(doc.SectionStyles, address)
                         from written in IO.lift(() => Indices((None, None, None, index), staged))
                         select written;

    private static partial void Indices((Option<int> LayerIndex, Option<int> LinetypeIndex, Option<int> MaterialIndex, Option<int> SectionStyleIndex) indices, ObjectAttributes staged);

    public static IO<Unit> CustomLinetype(ObjectAttributes staged, Option<Linetype> value) => IO.lift(() => staged.SetCustomLinetype(value.ValueUnsafe()));

    public static IO<Unit> CustomSectionStyle(ObjectAttributes staged, Option<SectionStyle> value) => IO.lift(() => staged.SetCustomSectionStyle(value.ValueUnsafe()));

    // --- [METADATA]
    public static Func<RhinoDoc, ObjectAttributes, IO<Unit>> SetUserStrings(HashMap<EqStringOrdinalIgnoreCase, string, Option<string>> edits) =>
        (_, staged) => from held in IO.lift(() => UserStrings.Held(staged.GetUserStrings()))
                       from written in UserStrings.Write(held, staged.SetUserString, edits)
                       select written;

    public static Func<RhinoDoc, ObjectAttributes, IO<Unit>> Groups(RowsEdit<ComponentRef<Group>, ComponentRef<Group>> edit) =>
        (doc, staged) => Edited(edit, IO.lift(() => staged.RemoveFromAllGroups()),
            Membership(doc, staged.AddToGroup), keys => keys.TraverseM(Membership(doc, staged.RemoveFromGroup)).As().Map(static _ => unit))
            .Map(static _ => unit);

    private static Func<ComponentRef<Group>, IO<Unit>> Membership(RhinoDoc doc, Action<int> write) =>
        address => from found in TableOps.Find(doc.Groups, address, includeDeleted: false)
                   from written in IO.lift(() => write(found.Index))
                   select written;

    // --- [OVERRIDES]
    public static IO<Unit> DisplayModeOverride(ObjectAttributes staged, Guid viewport, Option<Guid> mode) =>
        mode.Match(
            Some: id => Viewports.WithMode(id, description => IO.lift(() => Refused.Unless(staged.SetDisplayModeOverride(description, viewport), nameof(ObjectAttributes.SetDisplayModeOverride)))),
            None: () => IO.lift(() => staged.RemoveDisplayModeOverride(viewport)));

    public static IO<Unit> ActiveInViewport(ObjectAttributes staged, RowsEdit<Guid, Guid> edit, bool active) =>
        IO.lift(() => edit.Switch(
state: (staged, active), replace: static (state, replace) => Refused.Unless(state.staged.SetActiveInViewportOverrides([.. replace.Rows], state.active), nameof(ObjectAttributes.SetActiveInViewportOverrides)), add: static (state, add) => Callbacks.Each(add.Rows, Fin<Unit> (id, index) => state.staged.HasActiveInViewportOverride(id, out _)
                ? unit : RefusedElement.Unless(state.staged.AddActiveInViewportOverride(id, state.active), nameof(ObjectAttributes.AddActiveInViewportOverride), index)).Map(static _ => unit), remove: static (state, remove) => Callbacks.Each(remove.Keys, Fin<Unit> (id, index) => state.staged.HasActiveInViewportOverride(id, out bool stored)
                ? RefusedElement.Unless(state.staged.RemoveActiveInViewportOverride(id, stored), nameof(ObjectAttributes.RemoveActiveInViewportOverride), index) : unit).Map(static _ => unit)));

    public static IO<Unit> HideInDetail(ObjectAttributes staged, RowsEdit<Guid, Guid> edit) =>
        IO.lift(() => edit.Switch(
                toSet(staged.GetHideInDetailOverrides()),
                replace: static (held, replace) => (Hide: toSeq(toSet(replace.Rows).Except(held)), Show: toSeq(held.Except(replace.Rows))),
                add: static (held, add) => (Hide: toSeq(toSet(add.Rows).Except(held)), Show: Seq<Guid>()),
                remove: static (held, remove) => (Hide: Seq<Guid>(), Show: toSeq(held.Intersect(remove.Keys))))
            switch {
                var (hide, show) =>
                    (Callbacks.Each(show, staged.RemoveHideInDetailOverride, nameof(ObjectAttributes.RemoveHideInDetailOverride)).ToValidation(),
                     Callbacks.Each(hide, staged.AddHideInDetailOverride, nameof(ObjectAttributes.AddHideInDetailOverride)).ToValidation())
                        .Apply(static (_, _) => unit).As().ToFin(),
            });

    // --- [FRAME]
    public static IO<Unit> ObjectFrame(ObjectAttributes staged, Plane frame) =>
        from valid in IO.lift(Fin<Plane> () => frame.IsValid ? frame : new InvalidPlane(nameof(ObjectAttributes.SetObjectFrame)))
        from written in IO.lift(() => staged.SetObjectFrame(valid))
        select written;

    public static IO<Unit> Transform(ObjectAttributes staged, Transform xform) =>
        IO.lift(() => Refused.Unless(staged.Transform(xform), nameof(ObjectAttributes.Transform)));

    // --- [RENDERING]
    [MapPropertyFromSource(nameof(ObjectAttributes.CustomMeshingParameters), Use = nameof(Meshing))]
    private static partial void SetMeshing(Option<MeshingParameters> value, ObjectAttributes staged);

    public static IO<Unit> CustomMeshingParameters(ObjectAttributes staged, Option<MeshingParameters> value) =>
        IO.lift(() => SetMeshing(value, staged));

    [UserMapping]
    private static MeshingParameters? Meshing(Option<MeshingParameters> value) => value.ValueUnsafe();

    public static IO<Seq<uint>> Decals(ObjectAttributes staged, RowsEdit<DecalCreateParams, int> edit) =>
        Edited(edit, IO.lift(staged.Decals.RemoveAllDecals),
            row => (from decal in use(IO.lift(() => Missing.Unless(Decal.Create(row), nameof(Decal.Create))))
                    from crc in IO.lift(() => Conversions.Required(staged.Decals.Add(decal), nameof(staged.Decals.Add)))
                    select crc).Bracket(),
            keys => toHashSet(keys) switch {
                var crcs => IO.lift(() => Conversions.Rows(staged.Decals)).Bracket(
                    Use: held => IO.lift(() => Callbacks.Each(held.Filter(decal => crcs.Contains(decal.CRC)).Strict(), staged.Decals.Remove, nameof(staged.Decals.Remove))),
                    Fin: DisposalOps.Release),
            });

    public static IO<Unit> MaterialRefs(ObjectAttributes staged, RowsEdit<MaterialRefState, Guid> edit) =>
        Edited(edit, IO.lift(staged.MaterialRefs.Clear),
            row => (from created in use(IO.lift(() => Callbacks.Thrown<ArgumentException, MaterialRef>(() => staged.MaterialRefs.Create(AttributeMapper.ToParams(row)), nameof(staged.MaterialRefs.Create))))
                    from added in IO.lift(() => staged.MaterialRefs.Add(row.PlugInId, created))
                    select added).Bracket(),
            keys => IO.lift(() => keys.Iter(key => staged.MaterialRefs.Remove(key))))
            .Map(static _ => unit);

    private static IO<Seq<TAdded>> Edited<TRow, TKey, TAdded>(RowsEdit<TRow, TKey> edit, IO<Unit> clear, Func<TRow, IO<TAdded>> add, Func<Seq<TKey>, IO<Unit>> remove) =>
        edit.Switch(
            state: (Clear: clear, Add: add, Remove: remove),
            replace: static (state, replace) =>
                from cleared in state.Clear
                from added in replace.Rows.TraverseM(state.Add).As()
                select added,
            add: static (state, added) => added.Rows.TraverseM(state.Add).As(),
            remove: static (state, removed) => state.Remove(removed.Keys).Map(static _ => Seq<TAdded>()));
}

public static class AttributeOps {
    // --- [READS]
    public static IO<AttributeState> Read(ObjectAttributes attributes) =>
        IO.lift(() => Conversions.Rows(attributes.Decals)).Bracket(
            Use: decals => IO.lift(() => AttributeMapper.ToState(attributes, UserStrings.Held(attributes.GetUserStrings()), decals.Map(AttributeMapper.ToState).Strict())),
            Fin: DisposalOps.Release);

    public static IO<EffectiveDisplay> ReadEffective(ObjectAttributes attributes, RhinoDoc doc, Guid viewport) =>
        IO.lift(() => new EffectiveDisplay(
            attributes.DrawColor(doc, viewport),
            attributes.ComputedPlotColor(doc, viewport),
            PlotWeight.FromHost(attributes.ComputedPlotWeight(doc, viewport)),
            Conversions.Present(attributes.GetDisplayModeOverride(viewport)),
            Callbacks.Found(attributes.HasActiveInViewportOverride(viewport, out bool active), active)));

    public static IO<T> WithComputedSectionStyle<T>(ObjectAttributes attributes, RhinoDoc doc, ObjectAttributes sectioner, bool computeColors, Guid viewport, Func<SectionStyle, IO<T>> body) =>
        use(IO.lift(() => Missing.Unless(attributes.ComputedSectionStyle(doc, sectioner, computeColors, viewport), nameof(ObjectAttributes.ComputedSectionStyle)))).Bind(body).Bracket();

    // --- [FOLD]
    public static IO<Unit> Apply(RhinoDoc doc, ObjectAttributes staged, Seq<Func<RhinoDoc, ObjectAttributes, IO<Unit>>> edits) =>
        edits.TraverseM(edit => edit(doc, staged)).As().Map(static _ => unit);

    public static TableOp Modify(RhinoDoc doc, ObjectTarget target, Seq<Func<RhinoDoc, ObjectAttributes, IO<Unit>>> edits, bool quiet) =>
        new TableOp.ModifyAttributes(target, staged => Apply(doc, staged, edits), quiet);
}
