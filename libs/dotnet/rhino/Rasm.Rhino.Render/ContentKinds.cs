using System.Drawing;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.DocObjects.Tables;
using Rhino.Render;

namespace Rasm.Rhino.Render;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RenderMaterialSource {
    public sealed record FromMaterial() : RenderMaterialSource;

    public sealed record Basic() : RenderMaterialSource;

    public sealed record Imported(bool Reference) : RenderMaterialSource;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TextureSource {
    public sealed record FromBitmap(Bitmap Image) : TextureSource;

    public sealed record Simulated(SimulatedTextureState State) : TextureSource;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ContentKinds {
    // --- [MATERIALS]
    public static IO<TValue> WithMaterial<TValue>(RhinoDoc doc, int materialIndex, RenderMaterialSource source, Func<RenderContent, IO<TValue>> body) =>
        IO.lift(() => Missing.Unless(doc.Materials.FindIndex(materialIndex), nameof(MaterialTable.FindIndex)))
            .Bind(material => DisposalOps.Using(
                IO.lift(() => source.Switch(
                    (Doc: doc, Material: material),
                    fromMaterial: static (state, _) => Missing.Unless(RenderMaterial.FromMaterial(state.Material, state.Doc), nameof(RenderMaterial.FromMaterial)),
                    basic: static (state, _) => Missing.Unless(RenderMaterial.CreateBasicMaterial(state.Material, state.Doc), nameof(RenderMaterial.CreateBasicMaterial)),
                    imported: static (state, imported) =>
                        Missing.Unless(RenderMaterial.CreateImportedMaterial(state.Material, state.Doc, imported.Reference), nameof(RenderMaterial.CreateImportedMaterial)))),
                body));

    // --- [TEXTURES]
    public static IO<TValue> WithTexture<TValue>(RhinoDoc doc, TextureSource source, Func<RenderContent, IO<TValue>> body) =>
        DisposalOps.Using(source.Switch(
            doc,
            fromBitmap: static (document, bitmap) => IO.lift(() => Missing.Unless(RenderTexture.NewBitmapTexture(bitmap.Image, document), nameof(RenderTexture.NewBitmapTexture))),
            simulated: static (document, simulated) => Simulated(document, simulated.State)), body);

    public static IO<Unit> ExportTexture(RenderTexture texture, string path, int width, int height, int depth) =>
        Answers.QualifiedPath(path).Bind(target => IO.lift(() => Refused.Unless(texture.SaveAsImage(target, width, height, depth), nameof(RenderTexture.SaveAsImage))));

    private static IO<RenderTexture> Simulated(RhinoDoc doc, SimulatedTextureState state) =>
        DisposalOps.Using(() => new SimulatedTexture(doc), simulation =>
            from written in WriteSimulated(simulation, state)
            from content in IO.lift(() => Missing.Unless(RenderTexture.NewBitmapTexture(simulation, doc), nameof(RenderTexture.NewBitmapTexture)))
            select content);

    // --- [ENVIRONMENTS]
    public static IO<TValue> WithEnvironment<TValue>(RhinoDoc doc, EnvironmentState state, Func<RenderContent, IO<TValue>> body) =>
        DisposalOps.Using(static () => new SimulatedEnvironment(), simulation =>
            (from shaded in IO.lift(() => ContentKindMapper.Update(state, simulation))
             from background in state.Image.Match(
                 Some: image =>
                     from texture in use(() => new SimulatedTexture(doc))
                     from written in WriteSimulated(texture, image)
                     from assigned in IO.lift(() => { simulation.BackgroundImage = texture; })
                     select assigned,
                 None: static () => IO.pure(unit))
             from value in DisposalOps.Using(IO.lift(() => Missing.Unless(RenderEnvironment.NewBasicEnvironment(simulation, doc), nameof(RenderEnvironment.NewBasicEnvironment))), body)
             select value)
            .Bracket());
}
