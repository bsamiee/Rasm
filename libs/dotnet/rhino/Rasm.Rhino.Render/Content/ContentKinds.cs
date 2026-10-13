using Rhino;
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

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper]
public static partial class ContentMapper {
    [MapPropertyFromSource(nameof(ContentState.Kind), Use = nameof(@Conversions.KindOf))]
    [MapPropertyFromSource(nameof(ContentState.IsReference), Use = nameof(IsReference))]
    [MapPropertyFromSource(nameof(ContentState.UseCount), Use = nameof(UseCount))]
    [MapPropertyFromSource(nameof(ContentState.Slots), Use = nameof(Slots))]
    public static partial ContentState ToState(RenderContent content);

    public static partial MaterialClassification ToClassification(RenderMaterial material);

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
        DisposalOps.Within(static () => new TextureGraphInfo(), info => IO.lift(() => {
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
        }), static info => IO.lift(info.Dispose));

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

    public static IO<T> ReadSimulation<T>(
        RenderTexture texture, RenderTexture.TextureGeneration generation, Option<int> size, Option<RhinoObject> obj, Func<SimulatedTexture, IO<T>> body) =>
        DisposalOps.Within(() => size.Match(
                Some: pixels => texture.SimulatedTexture(generation, pixels, obj.ValueUnsafe()),
                None: () => texture.SimulatedTexture(generation, obj: obj.ValueUnsafe())),
            body,
            static simulation => IO.lift(simulation.Dispose));

    public static IO<T> ReadEnvironment<T>(RenderEnvironment environment, bool isForDataOnly, Func<SimulatedEnvironment, Texture, IO<T>> body) =>
        DisposalOps.Within(() => environment.SimulateEnvironment(isForDataOnly),
            simulation => DisposalOps.Within(() => simulation.BackgroundImage.Texture(), texture => body(simulation, texture), static texture => IO.lift(texture.Dispose)),
            static simulation => IO.lift(simulation.Dispose));

    // --- [WRITES]
    public static IO<Unit> WriteTexture(RenderTexture texture, TextureState state, RenderContent.ChangeContexts cc) =>
        Contents.WithinContentChange(texture, cc, DisposalOps.Within(static () => new TextureGraphInfo(), info => IO.lift(() => {
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
        }), static info => IO.lift(info.Dispose)));

    // --- [SAMPLING]
    public static IO<T> WithEvaluator<T>(RenderTexture texture, RenderTexture.TextureEvaluatorFlags flags, Func<TextureEvaluator, IO<T>> body) =>
        DisposalOps.Within(() => Missing.Unless(texture.CreateEvaluator(flags), nameof(RenderTexture.CreateEvaluator)).ThrowIfFail(),
            evaluator => IO.lift(() => Refused.Unless(evaluator.Initialize(), nameof(TextureEvaluator.Initialize))).Bind(_ => body(evaluator)),
            static evaluator => IO.lift(evaluator.Dispose));
}
