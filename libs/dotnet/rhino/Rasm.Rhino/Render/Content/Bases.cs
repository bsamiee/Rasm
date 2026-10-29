using System.Collections.Generic;
using System.Drawing;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.PlugIns;
using Rhino.Render;
using Rhino.UI;

namespace Rasm.Rhino.Render.Content;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ContentPreviewRow(
    RenderPlugIn.PreviewRenderTypes Type,
    Func<CreatePreviewEventArgs, IO<Option<Bitmap>>> Content,
    Func<CreateTexture2dPreviewEventArgs, IO<Option<Bitmap>>> Texture);

public sealed record ContentDefinition<TState>(string Caption, string Help, string Svg, TState Default) where TState : notnull {
    public Seq<ContentField<TState>> Fields { get; init; }
}

// --- [SERVICES] ------------------------------------------------------------------------
internal sealed class ContentBody<TState> where TState : notnull {
    private readonly ContentDefinition<TState> definition;

    public ContentBody(RenderContent content, ContentDefinition<TState> definition) {
        this.definition = definition;
        _ = definition.Fields.Iter(field => content.BindParameterToField(
            field.Key, field.Add(content.Fields, Conversions.Unset(field.Prompt.Map(static prompt => LocalText.Of(prompt).Local)), definition.Default),
            RenderContent.ChangeContexts.UI));
    }

    public TState State(RenderContent content) => definition.Fields.Fold(definition.Default, (state, row) => row.Read(content.Fields, state));

    public IO<RenderContent> Changed(RenderContent content, Func<TState, TState> change) =>
        Contents.Edit(content, RenderContent.ChangeContexts.Program, [
            Steps.Then<RenderContent>(changed => {
                TState next = change(State(changed));
                _ = definition.Fields.Iter(field => field.Write(changed.Fields, next));
                return changed;
            }),
        ]);

    public string TypeName => LocalText.Of(definition.Caption).Local;

    public string TypeDescription => LocalText.Of(definition.Help).Local;

    public IEnumerable<string> FilesToEmbed(RenderContent content) =>
        definition.Fields.Filter(static row => row.File).Choose(row => content.Fields.TryGetValue(row.Key, out string path) ? Conversions.Present(path) : None);

    public Option<Bitmap> Icon(Size size) => Optional(DrawingUtilities.BitmapFromSvg(definition.Svg, size.Width, size.Height, adjustForDarkMode: true));

    public void Sections(RenderContent content) =>
        _ = Session.Run(when(definition.Fields.Exists(static field => field.Prompt.IsSome), Products.Accepted(() => content.AddAutomaticUserInterfaceSection(TypeName, 0))).As());

    public string[] Excluded(string[] names) => [.. names, .. definition.Fields.Filter(static field => !field.Hashed).Map(static field => field.Key)];
}

public abstract class DefinedMaterial<TSelf, TState> : RenderMaterial
    where TSelf : DefinedMaterial<TSelf, TState>, IDefined<ContentDefinition<TState>> where TState : notnull {
    private readonly ContentBody<TState> body;

    protected DefinedMaterial() => body = new(this, TSelf.Definition);

    public TState State => body.State(this);

    public IO<RenderContent> Change(Func<TState, TState> change) => body.Changed(this, change);

    protected abstract Material Simulated(TState state);

    public sealed override string TypeName => body.TypeName;

    public sealed override string TypeDescription => body.TypeDescription;

    public sealed override IEnumerable<string> FilesToEmbed => body.FilesToEmbed(this);

    public sealed override bool VirtualIcon(Size size, out Bitmap? bitmap) => (bitmap = body.Icon(size).ValueUnsafe()) is not null;

    protected sealed override void OnAddUserInterfaceSections() => body.Sections(this);

    protected sealed override uint CalculateRenderHash2(CrcRenderHashFlags flags, string[] excludeParameterNames) => base.CalculateRenderHash2(flags, body.Excluded(excludeParameterNames));

    public sealed override void SimulateMaterial(ref Material simulation, RenderTexture.TextureGeneration tg) {
        Material target = simulation;
        if (!Session.Run((from built in Products.Of(() => Simulated(body.State(this))) from copied in IO.lift(() => target.CopyFrom(built)) select true).Bracket(), failed: false))
            base.SimulateMaterial(ref simulation, tg);
    }
}

public abstract class DefinedTexture<TSelf, TState> : RenderTexture
    where TSelf : DefinedTexture<TSelf, TState>, IDefined<ContentDefinition<TState>> where TState : notnull {
    private readonly ContentBody<TState> body;

    protected DefinedTexture() => body = new(this, TSelf.Definition);

    public TState State => body.State(this);

    public IO<RenderContent> Change(Func<TState, TState> change) => body.Changed(this, change);

    protected abstract Color4f Sample(TState state, Point3d uvw, Vector3d duvwdx, Vector3d duvwdy);

    public sealed override string TypeName => body.TypeName;

    public sealed override string TypeDescription => body.TypeDescription;

    public sealed override IEnumerable<string> FilesToEmbed => body.FilesToEmbed(this);

    public sealed override bool VirtualIcon(Size size, out Bitmap? bitmap) => (bitmap = body.Icon(size).ValueUnsafe()) is not null;

    protected sealed override void OnAddUserInterfaceSections() => body.Sections(this);

    protected sealed override uint CalculateRenderHash2(CrcRenderHashFlags flags, string[] excludeParameterNames) => base.CalculateRenderHash2(flags, body.Excluded(excludeParameterNames));

    public sealed override TextureEvaluator CreateEvaluator(TextureEvaluatorFlags evaluatorFlags) =>
        new Evaluator(evaluatorFlags, body.State(this), evaluatorFlags.HasFlag(TextureEvaluatorFlags.DisableLocalMapping) ? Transform.Identity : LocalMappingTransform, Sample);

    private sealed class Evaluator(TextureEvaluatorFlags flags, TState state, Transform mapping, Func<TState, Point3d, Vector3d, Vector3d, Color4f> sample) : TextureEvaluator(flags) {
        public override Color4f GetColor(Point3d uvw, Vector3d duvwdx, Vector3d duvwdy) => sample(state, mapping * uvw, mapping * duvwdx, mapping * duvwdy);
    }
}

public abstract class DefinedEnvironment<TSelf, TState> : RenderEnvironment
    where TSelf : DefinedEnvironment<TSelf, TState>, IDefined<ContentDefinition<TState>> where TState : notnull {
    private readonly ContentBody<TState> body;

    protected DefinedEnvironment() => body = new(this, TSelf.Definition);

    public TState State => body.State(this);

    public IO<RenderContent> Change(Func<TState, TState> change) => body.Changed(this, change);

    protected abstract IO<Unit> Simulated(TState state, SimulatedEnvironment simulation, bool isForDataOnly);

    public sealed override string TypeName => body.TypeName;

    public sealed override string TypeDescription => body.TypeDescription;

    public sealed override IEnumerable<string> FilesToEmbed => body.FilesToEmbed(this);

    public sealed override bool VirtualIcon(Size size, out Bitmap? bitmap) => (bitmap = body.Icon(size).ValueUnsafe()) is not null;

    protected sealed override void OnAddUserInterfaceSections() => body.Sections(this);

    protected sealed override uint CalculateRenderHash2(CrcRenderHashFlags flags, string[] excludeParameterNames) => base.CalculateRenderHash2(flags, body.Excluded(excludeParameterNames));

    public sealed override void SimulateEnvironment(ref SimulatedEnvironment simulation, bool isForDataOnly) {
        SimulatedEnvironment target = simulation;
        if (!Session.Run(Simulated(body.State(this), target, isForDataOnly).Map(static _ => true), failed: false))
            base.SimulateEnvironment(ref simulation, isForDataOnly);
    }
}
