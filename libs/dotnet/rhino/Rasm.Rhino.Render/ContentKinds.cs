using System.Drawing;
using LanguageExt.UnsafeValueAccess;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;
using Rhino.Render;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Render;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RenderMaterialSource {
    public sealed record FromMaterial() : RenderMaterialSource;

    public sealed record Basic() : RenderMaterialSource;

    public sealed record Imported(bool Reference) : RenderMaterialSource;
}

public sealed record TextureState(
    TextureProjectionMode Projection,
    TextureWrapType Wrap,
    Vector3d Repeat,
    Vector3d Offset,
    Vector3d Rotation,
    int MappingChannel,
    TextureEnvironmentMappingMode EnvironmentMode,
    bool RepeatLocked,
    bool OffsetLocked,
    bool PreviewIn3D,
    bool PreviewLocalMapping,
    bool DisplayInViewport,
    (double U, double V, double W, TextureGraphInfo.Axis Axis, TextureGraphInfo.Channel Channel) Graph);

public sealed record TextureTraits(
    Option<(int Width, int Height, int Depth)> PixelSize2,
    Transform LocalMapping,
    RenderTexture.eLocalMappingType LocalMappingType,
    TextureEnvironmentMappingMode InternalEnvironmentMode,
    bool IsHdrCapable,
    bool IsLinear,
    bool IsNormalMap,
    bool IsImageBased);

public sealed record SimulatedTextureState(
    string Filename,
    Vector2d Repeat,
    Vector2d Offset,
    double Rotation,
    bool Repeating,
    bool Filtered,
    SimulatedTexture.ProjectionModes Projection,
    int MappingChannel,
    Option<(Color4f Color, double Sensitivity)> Transparency);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TextureSource {
    public sealed record FromBitmap(Bitmap Image) : TextureSource;

    public sealed record Simulated(SimulatedTextureState State) : TextureSource;
}

public sealed record EnvironmentState(Color Background, SimulatedEnvironment.BackgroundProjections Projection, Option<SimulatedTextureState> Image);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ContentKinds {
    // --- [MATERIALS]
    public static IO<TValue> WithMaterial<TValue>(RhinoDoc doc, int materialIndex, RenderMaterialSource source, Func<RenderContent, IO<TValue>> body) =>
        IO.lift(() => Missing.Unless(doc.Materials.FindIndex(materialIndex), nameof(MaterialTable.FindIndex)))
            .Bind(material => Disposal.Using(
                IO.lift(() => source.Switch(
                    (Doc: doc, Material: material),
                    fromMaterial: static (state, _) => Missing.Unless(RenderMaterial.FromMaterial(state.Material, state.Doc), nameof(RenderMaterial.FromMaterial)),
                    basic: static (state, _) => Missing.Unless(RenderMaterial.CreateBasicMaterial(state.Material, state.Doc), nameof(RenderMaterial.CreateBasicMaterial)),
                    imported: static (state, imported) =>
                        Missing.Unless(RenderMaterial.CreateImportedMaterial(state.Material, state.Doc, imported.Reference), nameof(RenderMaterial.CreateImportedMaterial)))),
                body));

    public static IO<TValue> WithBakedMaterial<TValue>(RenderMaterial material, RenderTexture.TextureGeneration textureGeneration, Func<Material, IO<TValue>> body) =>
        Disposal.Using(() => material.ToMaterial(textureGeneration), body);

    public static IO<TValue> WithPhysicallyBased<TValue>(RenderMaterial material, RenderTexture.TextureGeneration textureGeneration, Func<Material, IO<TValue>> body) =>
        Disposal.Using(() => material.ConvertToPhysicallyBased(textureGeneration).Material, body);

    public static IO<Option<SlotState>> ReadSlot(RenderMaterial material, RenderMaterial.StandardChildSlots slot) =>
        IO.lift(() => Optional(material.GetTextureFromUsage(slot)).Map(texture => new SlotState(
            material.TextureChildSlotName(slot),
            texture.Id,
            texture.ChildSlotDisplayName,
            material.GetTextureOnFromUsage(slot),
            material.GetTextureAmountFromUsage(slot))));

    public static IO<Unit> Assign(RhinoDoc doc, RenderMaterial material, Seq<Guid> objects, RenderMaterial.AssignToSubFaceChoices subFaces, RenderMaterial.AssignToBlockChoices blocks) =>
        Disposal.Using(
            Disposal.AcquireAll(objects.Map(id => IO.lift(() => new ObjRef(doc, id)))),
            refs => IO.lift(() => Refused.Unless(material.AssignTo(refs, subFaces, blocks, bInteractive: false), nameof(RenderMaterial.AssignTo))));

    // --- [TEXTURES]
    public static IO<TextureState> ReadTexture(RenderTexture texture) =>
        Disposal.Using(static () => new TextureGraphInfo(), info => IO.lift(() => {
            TextureGraphInfo graph = info;
            texture.GraphInfo(ref graph);
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
                (graph.AmountU(), graph.AmountV(), graph.AmountW(), graph.ActiveAxis(), graph.ActiveChannel()));
        }));

    public static IO<TValue> WithEvaluator<TValue>(RenderTexture texture, RenderTexture.TextureEvaluatorFlags flags, Func<TextureEvaluator, IO<TValue>> body) =>
        Disposal.Using(IO.lift(() => Missing.Unless(texture.CreateEvaluator(flags), nameof(RenderTexture.CreateEvaluator))), evaluator =>
            from initialized in IO.lift(() => Refused.Unless(evaluator.Initialize(), nameof(TextureEvaluator.Initialize)))
            from value in body(evaluator)
            select value);

    public static IO<TValue> WithSimulated<TValue>(RenderTexture texture, RenderTexture.TextureGeneration generation, Option<int> size, Option<RhinoObject> obj, Func<SimulatedTexture, IO<TValue>> body) =>
        Disposal.Using(() => size.Match(
            Some: pixels => texture.SimulatedTexture(generation, pixels, obj.ValueUnsafe()),
            None: () => texture.SimulatedTexture(generation, obj: obj.ValueUnsafe())), body);

    public static IO<Unit> WriteTexture(RenderTexture texture, TextureState state, RenderContent.ChangeContexts cc) =>
        Contents.WithinContentChange(
            texture,
            cc,
            from written in IO.lift(() => {
                texture.SetProjectionMode(state.Projection, cc);
                texture.SetWrapType(state.Wrap, cc);
                texture.SetRepeat(state.Repeat, cc);
                texture.SetOffset(state.Offset, cc);
                texture.SetRotation(state.Rotation, cc);
                texture.SetMappingChannel(state.MappingChannel, cc);
                texture.SetEnvironmentMappingMode(state.EnvironmentMode, cc);
                texture.SetRepeatLocked(state.RepeatLocked, cc);
                texture.SetOffsetLocked(state.OffsetLocked, cc);
                texture.SetPreviewIn3D(state.PreviewIn3D, cc);
                texture.SetPreviewLocalMapping(state.PreviewLocalMapping, cc);
                texture.SetDisplayInViewport(state.DisplayInViewport, cc);
            })
            from graphed in Disposal.Using(static () => new TextureGraphInfo(), info => IO.lift(() => {
                info.SetAmountU(state.Graph.U);
                info.SetAmountV(state.Graph.V);
                info.SetAmountW(state.Graph.W);
                info.SetActiveAxis(state.Graph.Axis);
                info.SetActiveChannel(state.Graph.Channel);
                texture.SetGraphInfo(info);
            }))
            select graphed);

    public static IO<TextureTraits> ReadTraits(RenderTexture texture) =>
        IO.lift(() => new TextureTraits(
            Optional(texture.PixelSize2).Map(static size => (Width: size.width, Height: size.height, Depth: size.depth)),
            texture.LocalMappingTransform,
            texture.GetLocalMappingType(),
            texture.GetInternalEnvironmentMappingMode(),
            texture.IsHdrCapable(),
            texture.IsLinear(),
            texture.IsNormalMap(),
            texture.IsImageBased()));

    public static IO<TValue> WithTexture<TValue>(RhinoDoc doc, TextureSource source, Func<RenderContent, IO<TValue>> body) =>
        Disposal.Using(source.Switch(
            doc,
            fromBitmap: static (document, bitmap) => IO.lift(() => Missing.Unless(RenderTexture.NewBitmapTexture(bitmap.Image, document), nameof(RenderTexture.NewBitmapTexture))),
            simulated: static (document, simulated) => Simulated(document, simulated.State)), body);

    public static IO<Unit> ExportTexture(RenderTexture texture, string path, int width, int height, int depth) =>
        Answers.QualifiedPath(path).Bind(target => IO.lift(() => Refused.Unless(texture.SaveAsImage(target, width, height, depth), nameof(RenderTexture.SaveAsImage))));

    private static IO<RenderTexture> Simulated(RhinoDoc doc, SimulatedTextureState state) =>
        Disposal.Using(() => new SimulatedTexture(doc), simulation =>
            from written in WriteSimulated(simulation, state)
            from content in IO.lift(() => Missing.Unless(RenderTexture.NewBitmapTexture(simulation, doc), nameof(RenderTexture.NewBitmapTexture)))
            select content);

    private static IO<Unit> WriteSimulated(SimulatedTexture target, SimulatedTextureState state) =>
        IO.lift(() => {
            ContentKindMapper.Update(state, target);
            target.HasTransparentColor = state.Transparency.IsSome;
            _ = state.Transparency.Iter(transparency => {
                target.TransparentColor = transparency.Color;
                target.TransparentColorSensitivity = transparency.Sensitivity;
            });
        });

    // --- [ENVIRONMENTS]
    public static IO<EnvironmentState> ReadEnvironment(RenderEnvironment environment, bool isForDataOnly) =>
        Disposal.Using(() => environment.SimulateEnvironment(isForDataOnly), static simulation =>
            Disposal.Using(() => simulation.BackgroundImage, texture => IO.lift(() => ContentKindMapper.ToState(simulation, texture))));

    public static IO<TValue> WithEnvironment<TValue>(RhinoDoc doc, EnvironmentState state, Func<RenderContent, IO<TValue>> body) =>
        Disposal.Using(static () => new SimulatedEnvironment(), simulation =>
            from shaded in IO.lift(() => {
                simulation.BackgroundColor = state.Background;
                simulation.BackgroundProjection = state.Projection;
            })
            from value in state.Image.Match(
                Some: image => Disposal.Using(() => new SimulatedTexture(doc), texture => WithBackground(doc, simulation, texture, image, body)),
                None: () => WithBasicEnvironment(doc, simulation, body))
            select value);

    private static IO<TValue> WithBackground<TValue>(RhinoDoc doc, SimulatedEnvironment simulation, SimulatedTexture texture, SimulatedTextureState image, Func<RenderContent, IO<TValue>> body) =>
        from written in WriteSimulated(texture, image)
        from assigned in IO.lift(() => { simulation.BackgroundImage = texture; })
        from created in WithBasicEnvironment(doc, simulation, body)
        select created;

    private static IO<TValue> WithBasicEnvironment<TValue>(RhinoDoc doc, SimulatedEnvironment simulation, Func<RenderContent, IO<TValue>> body) =>
        Disposal.Using(IO.lift(() => Missing.Unless(RenderEnvironment.NewBasicEnvironment(simulation, doc), nameof(RenderEnvironment.NewBasicEnvironment))), body);
}

[Mapper]
internal static partial class ContentKindMapper {
    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    [MapProperty(nameof(SimulatedTextureState.Projection), nameof(SimulatedTexture.ProjectionMode))]
    [MapperIgnoreSource(nameof(SimulatedTextureState.Transparency), Justification = "Written through HasTransparentColor and its color pair")]
    internal static partial void Update(SimulatedTextureState state, SimulatedTexture texture);

    [MapProperty(nameof(SimulatedTexture.ProjectionMode), nameof(SimulatedTextureState.Projection))]
    [MapPropertyFromSource(nameof(SimulatedTextureState.Transparency), Use = nameof(Transparency))]
    internal static partial SimulatedTextureState ToState(SimulatedTexture texture, string filename);

    [MapProperty(nameof(SimulatedEnvironment.BackgroundColor), nameof(EnvironmentState.Background))]
    [MapProperty(nameof(SimulatedEnvironment.BackgroundProjection), nameof(EnvironmentState.Projection))]
    internal static partial EnvironmentState ToState(SimulatedEnvironment simulation, SimulatedTexture image);

    [UserMapping]
    private static Option<SimulatedTextureState> Image(SimulatedTexture texture) =>
        Answers.Present(texture.Filename).Map(filename => ToState(texture, filename));

    private static Option<(Color4f Color, double Sensitivity)> Transparency(SimulatedTexture texture) =>
        texture.HasTransparentColor ? Some((Color: texture.TransparentColor, Sensitivity: texture.TransparentColorSensitivity)) : None;
}
