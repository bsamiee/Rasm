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
[SmartEnum]
public sealed partial class MeshModifier {
    public static readonly MeshModifier Displacement = new(static modifiers => modifiers.Displacement is not null);
    public static readonly MeshModifier EdgeSoftening = new(static modifiers => modifiers.EdgeSoftening is not null);
    public static readonly MeshModifier Thickening = new(static modifiers => modifiers.Thickening is not null);
    public static readonly MeshModifier CurvePiping = new(static modifiers => modifiers.CurvePiping is not null);
    public static readonly MeshModifier ShutLining = new(static modifiers => modifiers.ShutLining is not null);

    [UseDelegateFromConstructor]
    public partial bool IsAttached(File3dmMeshModifiers modifiers);
}

[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct WireDensity : System.Numerics.IMinMaxValue<WireDensity> {
    public static WireDensity MinValue { get; } = new(-1);
    public static WireDensity MaxValue { get; } = new(int.MaxValue);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref int value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidRhinoValue();
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

public sealed record EffectiveDisplay(Color DrawColor, Color ComputedPlotColor, PlotWeight ComputedPlotWeight, Option<Guid> DisplayModeOverride, Option<bool> ActiveInViewport);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RowsEdit<TRow, TKey> {
    public sealed record Replace(Seq<TRow> Rows) : RowsEdit<TRow, TKey>;

    public sealed record Add(Seq<TRow> Rows) : RowsEdit<TRow, TKey>;

    public sealed record Remove(Seq<TKey> Keys) : RowsEdit<TRow, TKey>;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper]
internal static partial class AttributeMapper {
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

    [MapProperty(nameof(MaterialRefState.FrontFaceMaterialId), nameof(MaterialRefCreateParams.BackFaceMaterialId))]
    [MapProperty(nameof(MaterialRefState.BackFaceMaterialId), nameof(MaterialRefCreateParams.FrontFaceMaterialId))]
    [MapProperty(nameof(MaterialRefState.FrontFaceMaterialIndex), nameof(MaterialRefCreateParams.BackFaceMaterialIndex))]
    [MapProperty(nameof(MaterialRefState.BackFaceMaterialIndex), nameof(MaterialRefCreateParams.FrontFaceMaterialIndex))]
    internal static partial MaterialRefCreateParams ToParams(MaterialRefState row);

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

public static class AttributeEdits {
    // --- [NAMING]
    public static Func<RhinoDoc, ObjectAttributes, IO<Unit>> Name(Option<string> value) => Assigned(staged => staged.Name = Conversions.Unset(value));

    public static Func<RhinoDoc, ObjectAttributes, IO<Unit>> Url(Option<string> value) => Assigned(staged => staged.Url = Conversions.Unset(value));

    public static Func<RhinoDoc, ObjectAttributes, IO<Unit>> LayerIndex(LayerRef address) =>
        (doc, staged) => Layers.Resolve(doc, address).Bind(layer => IO.lift(() => { staged.LayerIndex = layer.Index; }));

    public static Func<RhinoDoc, ObjectAttributes, IO<Unit>> SetUserStrings(HashMap<EqStringOrdinalIgnoreCase, string, Option<string>> edits) =>
        (_, staged) => IO.lift(() => UserStrings.Held(staged.GetUserStrings())).Bind(held => UserStrings.Write(held, staged.SetUserString, edits));

    // --- [AXES]
    public static Func<RhinoDoc, ObjectAttributes, IO<Unit>> ColorSource(ObjectColorSource value) => Assigned(staged => staged.ColorSource = value);

    public static Func<RhinoDoc, ObjectAttributes, IO<Unit>> ObjectColor(Color value) => Assigned(staged => staged.ObjectColor = value);

    public static Func<RhinoDoc, ObjectAttributes, IO<Unit>> PlotColorSource(ObjectPlotColorSource value) => Assigned(staged => staged.PlotColorSource = value);

    public static Func<RhinoDoc, ObjectAttributes, IO<Unit>> PlotColor(Color value) => Assigned(staged => staged.PlotColor = value);

    public static Func<RhinoDoc, ObjectAttributes, IO<Unit>> PlotWeightSource(ObjectPlotWeightSource value) => Assigned(staged => staged.PlotWeightSource = value);

    public static Func<RhinoDoc, ObjectAttributes, IO<Unit>> PlotWeight(PlotWeight value) => Assigned(staged => staged.PlotWeight = value.ToHost());

    public static Func<RhinoDoc, ObjectAttributes, IO<Unit>> LinetypeSource(ObjectLinetypeSource value) => Assigned(staged => staged.LinetypeSource = value);

    public static Func<RhinoDoc, ObjectAttributes, IO<Unit>> LinetypeIndex(LinetypeRef value) =>
        (doc, staged) => value.Resolve(doc).Bind(index => IO.lift(() => { staged.LinetypeIndex = index; }));

    public static Func<RhinoDoc, ObjectAttributes, IO<Unit>> LinetypePatternScale(double value) => Assigned(staged => staged.LinetypePatternScale = value);

    public static Func<RhinoDoc, ObjectAttributes, IO<Unit>> MaterialSource(ObjectMaterialSource value) => Assigned(staged => staged.MaterialSource = value);

    public static Func<RhinoDoc, ObjectAttributes, IO<Unit>> MaterialIndex(Option<ComponentRef<Material>> address) =>
        (doc, staged) => TableOps.Index(doc.Materials, address).Bind(index => IO.lift(() => { staged.MaterialIndex = index; }));

    // --- [SECTIONS]
    public static Func<RhinoDoc, ObjectAttributes, IO<Unit>> SectionAttributesSource(ObjectSectionAttributesSource value) => Assigned(staged => staged.SectionAttributesSource = value);

    public static Func<RhinoDoc, ObjectAttributes, IO<Unit>> SectionStyleIndex(Option<ComponentRef<SectionStyle>> address) =>
        (doc, staged) => TableOps.Index(doc.SectionStyles, address).Bind(index => IO.lift(() => { staged.SectionStyleIndex = index; }));

    public static Func<RhinoDoc, ObjectAttributes, IO<Unit>> ClippingPlaneLabelStyle(SectionLabelStyle value) => Assigned(staged => staged.ClippingPlaneLabelStyle = value);

    public static Func<RhinoDoc, ObjectAttributes, IO<Unit>> CustomLinetype(Option<Linetype> value) => Assigned(staged => staged.SetCustomLinetype(value.ValueUnsafe()));

    public static Func<RhinoDoc, ObjectAttributes, IO<Unit>> CustomSectionStyle(Option<SectionStyle> value) => Assigned(staged => staged.SetCustomSectionStyle(value.ValueUnsafe()));

    // --- [DISPLAY]
    public static Func<RhinoDoc, ObjectAttributes, IO<Unit>> CastsShadows(bool value) => Assigned(staged => staged.CastsShadows = value);

    public static Func<RhinoDoc, ObjectAttributes, IO<Unit>> ReceivesShadows(bool value) => Assigned(staged => staged.ReceivesShadows = value);

    public static Func<RhinoDoc, ObjectAttributes, IO<Unit>> WireDensity(WireDensity value) => Assigned(staged => staged.WireDensity = value);

    public static Func<RhinoDoc, ObjectAttributes, IO<Unit>> DisplayOrder(int value) => Assigned(staged => staged.DisplayOrder = value);

    public static Func<RhinoDoc, ObjectAttributes, IO<Unit>> ObjectDecoration(ObjectDecoration value) => Assigned(staged => staged.ObjectDecoration = value);

    // --- [PLACEMENT]
    public static Func<RhinoDoc, ObjectAttributes, IO<Unit>> Space(ActiveSpace value) => Assigned(staged => staged.Space = value);

    public static Func<RhinoDoc, ObjectAttributes, IO<Unit>> ViewportId(Option<Guid> value) => Assigned(staged => staged.ViewportId = Conversions.Unset(value));

    public static Func<RhinoDoc, ObjectAttributes, IO<Unit>> Groups(RowsEdit<ComponentRef<Group>, ComponentRef<Group>> edit) =>
        (doc, staged) => Edited(
            edit,
            IO.lift(staged.RemoveFromAllGroups),
            Membership(doc, staged.AddToGroup),
            keys => keys.TraverseM(Membership(doc, staged.RemoveFromGroup)).As().Map(static _ => unit));

    // --- [OVERRIDES]
    public static Func<RhinoDoc, ObjectAttributes, IO<Unit>> DisplayModeOverride(Guid viewport, Option<Guid> mode) =>
        (_, staged) => mode.Match(
            Some: id => Viewports.WithMode(id, description => IO.lift(() => Refused.Unless(staged.SetDisplayModeOverride(description, viewport), nameof(ObjectAttributes.SetDisplayModeOverride)))),
            None: () => IO.lift(() => staged.RemoveDisplayModeOverride(viewport)));

    public static Func<RhinoDoc, ObjectAttributes, IO<Unit>> ActiveInViewport(RowsEdit<Guid, Guid> edit, bool active) =>
        (_, staged) => IO.lift(() => edit.Switch(
            (Staged: staged, Active: active),
            replace: static (state, replace) => Refused.Unless(state.Staged.SetActiveInViewportOverrides([.. replace.Rows], state.Active), nameof(ObjectAttributes.SetActiveInViewportOverrides)),
            add: static (state, add) => Callbacks.Each(
                add.Rows.Filter(id => !state.Staged.HasActiveInViewportOverride(id, out _)).Strict(),
                id => state.Staged.AddActiveInViewportOverride(id, state.Active),
                nameof(ObjectAttributes.AddActiveInViewportOverride)),
            remove: static (state, remove) => Callbacks.Each(
                remove.Keys.Choose(id => Callbacks.Found(state.Staged.HasActiveInViewportOverride(id, out bool stored), (Id: id, Stored: stored))).Strict(),
                held => state.Staged.RemoveActiveInViewportOverride(held.Id, held.Stored),
                nameof(ObjectAttributes.RemoveActiveInViewportOverride))));

    public static Func<RhinoDoc, ObjectAttributes, IO<Unit>> HideInDetail(RowsEdit<Guid, Guid> edit) =>
        (_, staged) => IO.lift(() => edit.Switch(
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

    public static Func<RhinoDoc, ObjectAttributes, IO<Unit>> DetailBackgroundVisible(bool value) => Assigned(staged => staged.DetailBackgroundVisible = value);

    // --- [HATCH]
    public static Func<RhinoDoc, ObjectAttributes, IO<Unit>> HatchBackgroundFillColor(Color value) => Assigned(staged => staged.HatchBackgroundFillColor = value);

    public static Func<RhinoDoc, ObjectAttributes, IO<Unit>> HatchBackgroundFillPrintColor(Color value) => Assigned(staged => staged.HatchBackgroundFillPrintColor = value);

    public static Func<RhinoDoc, ObjectAttributes, IO<Unit>> HatchBoundaryVisible(bool value) => Assigned(staged => staged.HatchBoundaryVisible = value);

    public static Func<RhinoDoc, ObjectAttributes, IO<Unit>> HatchBoundaryColor(Color value) => Assigned(staged => staged.HatchBoundaryColor = value);

    public static Func<RhinoDoc, ObjectAttributes, IO<Unit>> HatchBoundaryPlotColor(Color value) => Assigned(staged => staged.HatchBoundaryPlotColor = value);

    public static Func<RhinoDoc, ObjectAttributes, IO<Unit>> HatchBoundaryColorSource(ItemColorSource value) => Assigned(staged => staged.HatchBoundaryColorSource = value);

    public static Func<RhinoDoc, ObjectAttributes, IO<Unit>> HatchBoundaryPlotColorSource(ItemColorSource value) => Assigned(staged => staged.HatchBoundaryPlotColorSource = value);

    public static Func<RhinoDoc, ObjectAttributes, IO<Unit>> HatchBoundaryPlotWeight(Option<PlotWeight> value) =>
        Assigned(staged => staged.HatchBoundaryPlotWeightMillimeters = Document.Tables.PlotWeight.ToInheritable(value));

    // --- [FRAME]
    public static Func<RhinoDoc, ObjectAttributes, IO<Unit>> ObjectFrame(Plane frame) =>
        (_, staged) => IO.lift(InvalidPlane.Unless(frame, nameof(ObjectAttributes.SetObjectFrame))).Bind(valid => IO.lift(() => staged.SetObjectFrame(valid)));

    public static Func<RhinoDoc, ObjectAttributes, IO<Unit>> Transform(Transform xform) =>
        (_, staged) => IO.lift(() => Refused.Unless(staged.Transform(xform), nameof(ObjectAttributes.Transform)));

    // --- [RENDERING]
    public static Func<RhinoDoc, ObjectAttributes, IO<Unit>> CustomMeshingParameters(Option<MeshingParameters> value) => Assigned(staged => staged.CustomMeshingParameters = value.ValueUnsafe());

    public static Func<RhinoDoc, ObjectAttributes, IO<Unit>> EnableCustomMeshingParameters(bool value) => Assigned(staged => staged.EnableCustomMeshingParameters = value);

    public static Func<RhinoDoc, ObjectAttributes, IO<Unit>> ClearRenderingAttributes { get; } = Assigned(static staged => staged.ClearRenderingAttributes());

    public static Func<RhinoDoc, ObjectAttributes, IO<Unit>> Decals(RowsEdit<DecalCreateParams, int> edit) =>
        (_, staged) => Edited(
            edit,
            IO.lift(() => staged.Decals.RemoveAllDecals()),
            row => use(IO.lift(() => Missing.Unless(Decal.Create(row), nameof(Decal.Create))))
                .Bind(decal => IO.lift(() => Conversions.Required(staged.Decals.Add(decal), nameof(staged.Decals.Add))))
                .Bracket(),
            keys => IO.lift(() => Conversions.Rows(staged.Decals)).Bracket(
                Use: held => IO.lift(() => Callbacks.Each(held.Filter(Matching(toHashSet(keys))).Strict(), staged.Decals.Remove, nameof(staged.Decals.Remove))),
                Fin: DisposalOps.Release));

    public static Func<RhinoDoc, ObjectAttributes, IO<Unit>> MaterialRefs(RowsEdit<MaterialRefState, Guid> edit) =>
        (_, staged) => Edited(
            edit,
            IO.lift(() => staged.MaterialRefs.Clear()),
            row => use(IO.lift(() => Callbacks.Thrown<ArgumentException, MaterialRef>(() => staged.MaterialRefs.Create(AttributeMapper.ToParams(row)), nameof(staged.MaterialRefs.Create))))
                .Bind(created => IO.lift(() => staged.MaterialRefs.Add(row.PlugInId, created)))
                .Bracket(),
            keys => IO.lift(() => keys.Iter(key => staged.MaterialRefs.Remove(key))));

    // --- [WRITES]
    private static Func<RhinoDoc, ObjectAttributes, IO<Unit>> Assigned(Action<ObjectAttributes> write) => (_, staged) => IO.lift(() => write(staged));

    private static IO<Unit> Edited<TRow, TKey, TAdded>(RowsEdit<TRow, TKey> edit, IO<Unit> clear, Func<TRow, IO<TAdded>> add, Func<Seq<TKey>, IO<Unit>> remove) =>
        edit.Switch(
            (Clear: clear, Add: add, Remove: remove),
            replace: static (moves, replace) => moves.Clear.Bind(_ => replace.Rows.TraverseM(moves.Add).As()).Map(static _ => unit),
            add: static (moves, added) => added.Rows.TraverseM(moves.Add).As().Map(static _ => unit),
            remove: static (moves, removed) => moves.Remove(removed.Keys));

    private static Func<ComponentRef<Group>, IO<Unit>> Membership(RhinoDoc doc, Action<int> write) =>
        address => TableOps.Find(doc.Groups, address, includeDeleted: false).Bind(group => IO.lift(() => write(group.Index)));

    private static Func<Decal, bool> Matching(LanguageExt.HashSet<int> crcs) => decal => crcs.Contains(decal.CRC);
}

public static class AttributeOps {
    // --- [READS]
    public static IO<AttributeState> Read(ObjectAttributes attributes) =>
        IO.lift(() => Conversions.Rows(attributes.Decals)).Bracket(
            Use: decals => IO.lift(() => AttributeMapper.ToState(attributes, UserStrings.Held(attributes.GetUserStrings()), decals.Map(static decal => AttributeMapper.ToState(decal)).Strict())),
            Fin: DisposalOps.Release);

    public static IO<EffectiveDisplay> ReadEffective(ObjectAttributes attributes, RhinoDoc doc, Guid viewport) =>
        IO.lift(() => new EffectiveDisplay(
            attributes.DrawColor(doc, viewport),
            attributes.ComputedPlotColor(doc, viewport),
            PlotWeight.FromHost(attributes.ComputedPlotWeight(doc, viewport)),
            Conversions.Present(attributes.GetDisplayModeOverride(viewport)),
            Callbacks.Found(attributes.HasActiveInViewportOverride(viewport, out bool active), active)));

    public static IO<A> WithComputedSectionStyle<A>(ObjectAttributes attributes, RhinoDoc doc, ObjectAttributes sectioner, bool computeColors, Guid viewport, Func<SectionStyle, IO<A>> body) =>
        use(IO.lift(() => Missing.Unless(attributes.ComputedSectionStyle(doc, sectioner, computeColors, viewport), nameof(ObjectAttributes.ComputedSectionStyle)))).Bind(body).Bracket();

    // --- [FOLD]
    public static IO<Unit> Apply(RhinoDoc doc, ObjectAttributes staged, Seq<Func<RhinoDoc, ObjectAttributes, IO<Unit>>> edits) =>
        edits.TraverseM(edit => edit(doc, staged)).As().Map(static _ => unit);

    public static TableOp Modify(RhinoDoc doc, ObjectTarget target, Seq<Func<RhinoDoc, ObjectAttributes, IO<Unit>>> edits, bool quiet) =>
        new TableOp.ModifyAttributes(target, staged => Apply(doc, staged, edits), quiet);
}
