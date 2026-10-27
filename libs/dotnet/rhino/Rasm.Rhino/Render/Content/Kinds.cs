using System.Diagnostics;
using Rhino.DocObjects;
using Rhino.Render;

namespace Rasm.Rhino.Render.Content;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ChildSlot(Guid Id, string ChildSlotName, Option<string> ChildSlotDisplayName, bool ChildSlotOn, double ChildSlotAmount);

public sealed record TextureGraph(double AmountU, double AmountV, double AmountW, TextureGraphInfo.Axis ActiveAxis, TextureGraphInfo.Channel ActiveChannel);

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
    TextureGraph GraphInfo);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ContentKinds {
    // --- [KINDS]
    public static TResult ByKind<TResult>(RenderContent content, Func<RenderMaterial, TResult> material, Func<RenderEnvironment, TResult> environment, Func<RenderTexture, TResult> texture) =>
        content switch {
            RenderMaterial found => material(found),
            RenderEnvironment found => environment(found),
            RenderTexture found => texture(found),
            _ => throw new UnreachableException(),
        };

    public static RenderContentKind KindOf(RenderContent content) =>
        ByKind(content, static _ => RenderContentKind.Material, static _ => RenderContentKind.Environment, static _ => RenderContentKind.Texture);

    // --- [READS]
    public static IO<Option<ChildSlot>> ReadSlot(RenderMaterial material, RenderMaterial.StandardChildSlots slot) =>
        IO.lift(() => Optional(material.GetTextureFromUsage(slot)).Map(texture => new ChildSlot(
            texture.Id,
            material.TextureChildSlotName(slot),
            Conversions.Present(texture.ChildSlotDisplayName),
            material.GetTextureOnFromUsage(slot),
            material.GetTextureAmountFromUsage(slot))));

    public static IO<TextureState> ReadTexture(RenderTexture texture) =>
        use(static () => new TextureGraphInfo()).Bind(info => IO.lift(() => {
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
                new TextureGraph(info.AmountU(), info.AmountV(), info.AmountW(), info.ActiveAxis(), info.ActiveChannel()));
        })).Bracket();

    public static IO<T> ReadSimulation<T>(
        RenderTexture texture, RenderTexture.TextureGeneration generation, Option<int> size, Option<RhinoObject> obj, Func<SimulatedTexture, IO<T>> body) =>
        unitIO.Bind(_ => size.Match(
                Some: pixels => Products.Made(texture.SimulatedTexture(generation, pixels, obj.ValueUnsafe())),
                None: () => Products.Made(texture.SimulatedTexture(generation, obj: obj.ValueUnsafe()))))
            .Bind(body)
            .Bracket();

    public static IO<T> ReadEnvironment<T>(RenderEnvironment environment, bool isForDataOnly, Func<SimulatedEnvironment, Texture, IO<T>> body) =>
        (from simulation in unitIO.Bind(_ => Products.Made(environment.SimulateEnvironment(isForDataOnly)))
         from texture in Products.Made(simulation.BackgroundImage?.Texture())
         from value in body(simulation, texture)
         select value).Bracket();

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

    // --- [SAMPLING]
    public static IO<T> WithEvaluator<T>(RenderTexture texture, RenderTexture.TextureEvaluatorFlags flags, Func<TextureEvaluator, IO<T>> body) =>
        (from evaluator in unitIO.Bind(_ => Products.Made(texture.CreateEvaluator(flags)))
         from initialized in IO.lift(() => RhinoFault.Unless(evaluator.Initialize()))
         from value in body(evaluator)
         select value).Bracket();
}
