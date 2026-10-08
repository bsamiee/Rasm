using System.Drawing;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.Render;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Render.Content;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ChildSlot(Guid Id, string ChildSlotName, Option<string> ChildSlotDisplayName, bool ChildSlotOn, double ChildSlotAmount);

public sealed record ContentState(
    Guid Id,
    Guid TypeId,
    Guid GroupId,
    RenderContentKind Kind,
    Option<string> Name,
    Option<string> DisplayName,
    Option<string> TypeName,
    Option<string> TypeDescription,
    Option<string> Notes,
    Option<string> Tags,
    Option<string> Category,
    RenderContentStyles Styles,
    ProxyTypes ProxyType,
    LengthUnit ModelUnits,
    bool TopLevel,
    bool Hidden,
    bool Private,
    bool IsLocked,
    bool CanBeEdited,
    bool IsDefaultInstance,
    bool IsHiddenByAutoDelete,
    bool IsReference,
    int UseCount,
    Option<uint> DocumentOwner,
    Option<uint> DocumentAssoc,
    Option<Guid> Parent,
    Option<string> ChildSlotName,
    Seq<ChildSlot> Slots);

public sealed record MaterialClassification(
    bool SmellsLikePlaster,
    bool SmellsLikePaint,
    bool SmellsLikeMetal,
    bool SmellsLikePlastic,
    bool SmellsLikeGem,
    bool SmellsLikeGlass,
    bool SmellsLikeTexturedPlaster,
    bool SmellsLikeTexturedPaint,
    bool SmellsLikeTexturedMetal,
    bool SmellsLikeTexturedPlastic,
    bool SmellsLikeTexturedGem,
    bool SmellsLikeTexturedGlass);

public sealed record TextureState(
    TextureProjectionMode ProjectionMode,
    TextureWrapType WrapType,
    Vector3d Repeat,
    Vector3d Offset,
    Vector3d Rotation,
    int MappingChannel,
    TextureEnvironmentMappingMode EnvironmentMappingMode,
    bool RepeatLocked,
    bool OffsetLocked,
    bool PreviewIn3D,
    bool PreviewLocalMapping,
    bool DisplayInViewport,
    (double AmountU, double AmountV, double AmountW, TextureGraphInfo.Axis ActiveAxis, TextureGraphInfo.Channel ActiveChannel) GraphInfo);

public sealed record TextureTraits(
    Option<(int Width, int Height, int Depth)> PixelSize2,
    Transform LocalMappingTransform,
    RenderTexture.eLocalMappingType LocalMappingType,
    TextureEnvironmentMappingMode InternalEnvironmentMappingMode,
    bool IsHdrCapable,
    bool IsLinear,
    bool IsNormalMap,
    bool IsImageBased);

public sealed record SimulatedTextureState(
    Option<string> Filename,
    Vector2d Repeat,
    Vector2d Offset,
    double Rotation,
    bool Repeating,
    bool Filtered,
    SimulatedTexture.ProjectionModes ProjectionMode,
    int MappingChannel,
    Option<(Color4f Color, double Sensitivity)> Transparency);

public sealed record EnvironmentState(Color BackgroundColor, SimulatedEnvironment.BackgroundProjections BackgroundProjection, Option<string> BackgroundImageFilename);

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper]
public static partial class ContentMapper {
    [MapPropertyFromSource(nameof(ContentState.Kind), Use = nameof(@Conversions.KindOf))]
    [MapPropertyFromSource(nameof(ContentState.IsReference), Use = nameof(IsReference))]
    [MapPropertyFromSource(nameof(ContentState.UseCount), Use = nameof(UseCount))]
    [MapPropertyFromSource(nameof(ContentState.Slots), Use = nameof(Slots))]
    public static partial ContentState ToState(RenderContent content);

    public static partial MaterialClassification ToClassification(RenderMaterial material);

    [MapPropertyFromSource(nameof(SimulatedTextureState.Transparency), Use = nameof(Transparency))]
    internal static partial SimulatedTextureState ToState(SimulatedTexture simulation);

    [MapperRequiredMapping(RequiredMappingStrategy.Both)]
    [MapProperty(nameof(@SimulatedTextureState.Transparency.IsSome), nameof(SimulatedTexture.HasTransparentColor))]
    [MapperIgnoreTarget(nameof(SimulatedTexture.TransparentColor), Justification = "Written with its sensitivity by ContentKinds.WriteSimulation")]
    [MapperIgnoreTarget(nameof(SimulatedTexture.TransparentColorSensitivity), Justification = "Written with its color by ContentKinds.WriteSimulation")]
    internal static partial void Update(SimulatedTextureState state, SimulatedTexture simulation);

    internal static partial EnvironmentState ToState(SimulatedEnvironment simulation);

    [MapperRequiredMapping(RequiredMappingStrategy.Both)]
    [MapperIgnoreSource(nameof(EnvironmentState.BackgroundImageFilename), Justification = "Written by ContentKinds.WriteEnvironment through the lent background image")]
    [MapperIgnoreTarget(nameof(SimulatedEnvironment.BackgroundImage), Justification = "Its Filename written by ContentKinds.WriteEnvironment")]
    internal static partial void Update(EnvironmentState state, SimulatedEnvironment simulation);

    [UserMapping]
    private static Option<Guid> Id(RenderContent? content) => Optional(content).Map(static found => found.Id);

    private static bool IsReference(RenderContent content) => content.IsReference();

    private static int UseCount(RenderContent content) => content.UseCount();

    private static Seq<ChildSlot> Slots(RenderContent content) =>
        toSeq(LanguageExt.List.unfold(content.FirstChild, static child => Optional(child).Map(static found => (found, found.NextSibling))))
            .Map(child => new ChildSlot(
                child.Id,
                child.ChildSlotName,
                Conversions.Present(child.ChildSlotDisplayName),
                content.ChildSlotOn(child.ChildSlotName),
                content.ChildSlotAmount(child.ChildSlotName)))
            .Strict();

    private static Option<(Color4f Color, double Sensitivity)> Transparency(SimulatedTexture simulation) =>
        simulation.HasTransparentColor ? Some((simulation.TransparentColor, simulation.TransparentColorSensitivity)) : None;
}

public static class ContentKinds {
    // --- [READS]
    public static IO<Option<ChildSlot>> ReadSlot(RenderMaterial material, RenderMaterial.StandardChildSlots slot) =>
        IO.lift(() => Optional(material.GetTextureFromUsage(slot)).Map(texture => new ChildSlot(
            texture.Id,
            material.TextureChildSlotName(slot),
            Conversions.Present(texture.ChildSlotDisplayName),
            material.GetTextureOnFromUsage(slot),
            material.GetTextureAmountFromUsage(slot))));

    public static IO<TextureState> ReadTexture(RenderTexture texture) =>
        use(static () => new TextureGraphInfo()).Map(info => {
            texture.GraphInfo(ref info);
            return new TextureState(
                texture.GetProjectionMode(),
                texture.GetWrapType(),
                texture.GetRepeat(),
                texture.GetOffset(),
                texture.GetRotation(),
                texture.GetMappingChannel(),
                texture.GetEnvironmentMappingMode(),
                texture.GetRepeatLocked(),
                texture.GetOffsetLocked(),
                texture.GetPreviewIn3D(),
                texture.GetPreviewLocalMapping(),
                texture.GetDisplayInViewport(),
                (info.AmountU(), info.AmountV(), info.AmountW(), info.ActiveAxis(), info.ActiveChannel()));
        }).Bracket();

    public static IO<TextureTraits> ReadTraits(RenderTexture texture) =>
        IO.lift(() => new TextureTraits(
            Optional(texture.PixelSize2),
            texture.LocalMappingTransform,
            texture.GetLocalMappingType(),
            texture.GetInternalEnvironmentMappingMode(),
            texture.IsHdrCapable(),
            texture.IsLinear(),
            texture.IsNormalMap(),
            texture.IsImageBased()));

    public static IO<(SimulatedTextureState State, Option<string> OriginalFilename, Transform LocalMappingTransform)> ReadSimulation(
        RenderTexture texture, RenderTexture.TextureGeneration generation, Option<int> size, Option<RhinoObject> obj) =>
        use(() => size.Match(
                Some: pixels => texture.SimulatedTexture(generation, pixels, obj.ValueUnsafe()),
                None: () => texture.SimulatedTexture(generation, obj: obj.ValueUnsafe())))
            .Map(static simulation => (ContentMapper.ToState(simulation), Conversions.Present(simulation.OriginalFilename), simulation.LocalMappingTransform))
            .Bracket();

    public static IO<EnvironmentState> ReadEnvironment(RenderEnvironment environment, bool isForDataOnly) =>
        use(() => environment.SimulateEnvironment(isForDataOnly)).Map(ContentMapper.ToState).Bracket();

    // --- [WRITES]
    public static IO<Unit> WriteTexture(RenderTexture texture, TextureState state, RenderContent.ChangeContexts cc) =>
        Contents.WithinContentChange(texture, cc, use(static () => new TextureGraphInfo()).Bind(info => IO.lift(() => {
            texture.SetProjectionMode(state.ProjectionMode, cc);
            texture.SetWrapType(state.WrapType, cc);
            texture.SetRepeat(state.Repeat, cc);
            texture.SetOffset(state.Offset, cc);
            texture.SetRotation(state.Rotation, cc);
            texture.SetMappingChannel(state.MappingChannel, cc);
            texture.SetEnvironmentMappingMode(state.EnvironmentMappingMode, cc);
            texture.SetRepeatLocked(state.RepeatLocked, cc);
            texture.SetOffsetLocked(state.OffsetLocked, cc);
            texture.SetPreviewIn3D(state.PreviewIn3D, cc);
            texture.SetPreviewLocalMapping(state.PreviewLocalMapping, cc);
            texture.SetDisplayInViewport(state.DisplayInViewport, cc);
            info.SetAmountU(state.GraphInfo.AmountU);
            info.SetAmountV(state.GraphInfo.AmountV);
            info.SetAmountW(state.GraphInfo.AmountW);
            info.SetActiveAxis(state.GraphInfo.ActiveAxis);
            info.SetActiveChannel(state.GraphInfo.ActiveChannel);
            texture.SetGraphInfo(info);
        })).Bracket());

    public static IO<Unit> WriteSimulation(SimulatedTextureState state, SimulatedTexture simulation) =>
        IO.lift(() => {
            ContentMapper.Update(state, simulation);
            _ = state.Transparency.Iter(transparency => (simulation.TransparentColor, simulation.TransparentColorSensitivity) = transparency);
        });

    public static IO<Unit> WriteEnvironment(EnvironmentState state, SimulatedEnvironment simulation) =>
        IO.lift(() => {
            ContentMapper.Update(state, simulation);
            simulation.BackgroundImage.Filename = Conversions.Unset(state.BackgroundImageFilename);
        });

    // --- [SAMPLING]
    public static IO<A> WithEvaluator<A>(RenderTexture texture, RenderTexture.TextureEvaluatorFlags flags, Func<TextureEvaluator, IO<A>> body) =>
        (from evaluator in use(IO.lift(() => Missing.Unless(texture.CreateEvaluator(flags), nameof(RenderTexture.CreateEvaluator))))
         from _ in IO.lift(() => Refused.Unless(evaluator.Initialize(), nameof(TextureEvaluator.Initialize)))
         from value in body(evaluator)
         select value).Bracket();
}
