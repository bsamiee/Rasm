using Rhino.DocObjects;
using Rhino.Render;

namespace Rasm.Rhino.Render.Content;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ChildSlot(Guid Id, string ChildSlotName, Option<string> ChildSlotDisplayName, bool ChildSlotOn, double ChildSlotAmount);

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

// --- [OPERATIONS] ----------------------------------------------------------------------
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
        }));

    public static IO<T> ReadSimulation<T>(
        RenderTexture texture, RenderTexture.TextureGeneration generation, Option<int> size, Option<RhinoObject> obj, Func<SimulatedTexture, IO<T>> body) =>
        DisposalOps.Within(() => size.Match(
                Some: pixels => texture.SimulatedTexture(generation, pixels, obj.ValueUnsafe()),
                None: () => texture.SimulatedTexture(generation, obj: obj.ValueUnsafe())), body);

    public static IO<T> ReadEnvironment<T>(RenderEnvironment environment, bool isForDataOnly, Func<SimulatedEnvironment, Texture, IO<T>> body) =>
        DisposalOps.Within(() => environment.SimulateEnvironment(isForDataOnly), simulation => DisposalOps.Within(() => simulation.BackgroundImage.Texture(), texture => body(simulation, texture)));

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
        })));

    // --- [SAMPLING]
    public static IO<T> WithEvaluator<T>(RenderTexture texture, RenderTexture.TextureEvaluatorFlags flags, Func<TextureEvaluator, IO<T>> body) =>
        DisposalOps.Found(() => texture.CreateEvaluator(flags), nameof(RenderTexture.CreateEvaluator), evaluator => IO.lift(() => Refused.Unless(evaluator.Initialize(), nameof(TextureEvaluator.Initialize))).Bind(_ => body(evaluator)));
}
