using System.Collections.Generic;
using System.Drawing;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.PlugIns;
using Rhino.Render;
using Rhino.Render.Fields;
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
    private readonly Seq<(Field Field, ContentField<TState> Row)> fields;

    public ContentBody(RenderContent content, ContentDefinition<TState> definition) {
        this.definition = definition;
        fields = definition.Fields.Map(row => (Field: row.Add(content.Fields, Conversions.Unset(row.Prompt.Map(static prompt => LocalText.Of(prompt).Local)), definition.Default), Row: row)).Strict();
        _ = fields.Iter(bound => content.BindParameterToField(bound.Field.Name, bound.Field, RenderContent.ChangeContexts.UI));
    }

    public TState State() => fields.Fold(definition.Default, static (state, bound) => bound.Row.Read(bound.Field, state));

    public IO<RenderContent> Changed(RenderContent content, Func<TState, TState> change) =>
        Contents.Edit(content, RenderContent.ChangeContexts.Program, [
            Steps.Then<RenderContent>(changed => {
                TState next = change(State());
                _ = fields.Iter(bound => changed.SetParameter(bound.Field.Name, bound.Row.Value(next)));
                return changed;
            }),
        ]);

    public string TypeName => LocalText.Of(definition.Caption).Local;

    public string TypeDescription => LocalText.Of(definition.Help).Local;

    public IEnumerable<string> FilesToEmbed =>
        fields.Filter(static bound => bound.Row.File).Choose(static bound => Conversions.Present(bound.Field.GetValue<string>()));

    public Option<Bitmap> Icon(Size size) => Optional(DrawingUtilities.BitmapFromSvg(definition.Svg, size.Width, size.Height, adjustForDarkMode: true));

    public void Sections(RenderContent content) =>
        _ = Session.Run(when(definition.Fields.Exists(static field => field.Prompt.IsSome), Products.Accepted(() => content.AddAutomaticUserInterfaceSection(TypeName, 0))).As());

    public string[] Excluded(string[] names) => [.. names, .. definition.Fields.Filter(static field => !field.Hashed).Map(static field => field.Key)];
}

public abstract class DefinedMaterial<TSelf, TState> : RenderMaterial
    where TSelf : DefinedMaterial<TSelf, TState>, IDefined<ContentDefinition<TState>> where TState : notnull {
    private readonly ContentBody<TState> body;

    protected DefinedMaterial() => body = new(this, TSelf.Definition);

    public TState State => body.State();

    public IO<RenderContent> Change(Func<TState, TState> change) => body.Changed(this, change);

    protected abstract Material Simulated(TState state);

    public sealed override string TypeName => body.TypeName;

    public sealed override string TypeDescription => body.TypeDescription;

    public sealed override IEnumerable<string> FilesToEmbed => body.FilesToEmbed;

    public sealed override bool VirtualIcon(Size size, out Bitmap? bitmap) => (bitmap = body.Icon(size).ValueUnsafe()) is not null;

    protected sealed override void OnAddUserInterfaceSections() => body.Sections(this);

    protected sealed override uint CalculateRenderHash2(CrcRenderHashFlags flags, string[] excludeParameterNames) => base.CalculateRenderHash2(flags, body.Excluded(excludeParameterNames));

    public sealed override void SimulateMaterial(ref Material simulation, RenderTexture.TextureGeneration tg) {
        Material target = simulation;
        if (!Session.Run((from built in Products.Of(() => Simulated(body.State())) from copied in IO.lift(() => target.CopyFrom(built)) select true).Bracket(), failed: false))
            base.SimulateMaterial(ref simulation, tg);
    }
}

public abstract class DefinedTexture<TSelf, TState> : RenderTexture
    where TSelf : DefinedTexture<TSelf, TState>, IDefined<ContentDefinition<TState>> where TState : notnull {
    private readonly ContentBody<TState> body;

    protected DefinedTexture() => body = new(this, TSelf.Definition);

    public TState State => body.State();

    public IO<RenderContent> Change(Func<TState, TState> change) => body.Changed(this, change);

    protected abstract Color4f Sample(TState state, Point3d uvw, Vector3d duvwdx, Vector3d duvwdy);

    public sealed override string TypeName => body.TypeName;

    public sealed override string TypeDescription => body.TypeDescription;

    public sealed override IEnumerable<string> FilesToEmbed => body.FilesToEmbed;

    public sealed override bool VirtualIcon(Size size, out Bitmap? bitmap) => (bitmap = body.Icon(size).ValueUnsafe()) is not null;

    protected sealed override void OnAddUserInterfaceSections() => body.Sections(this);

    protected sealed override uint CalculateRenderHash2(CrcRenderHashFlags flags, string[] excludeParameterNames) => base.CalculateRenderHash2(flags, body.Excluded(excludeParameterNames));

    public sealed override TextureEvaluator CreateEvaluator(TextureEvaluatorFlags evaluatorFlags) =>
        new Evaluator(evaluatorFlags, body.State(), evaluatorFlags.HasFlag(TextureEvaluatorFlags.DisableLocalMapping) ? Transform.Identity : LocalMappingTransform, Sample);

    private sealed class Evaluator(TextureEvaluatorFlags flags, TState state, Transform mapping, Func<TState, Point3d, Vector3d, Vector3d, Color4f> sample) : TextureEvaluator(flags) {
        public override Color4f GetColor(Point3d uvw, Vector3d duvwdx, Vector3d duvwdy) => sample(state, mapping * uvw, mapping * duvwdx, mapping * duvwdy);
    }
}

public abstract class DefinedEnvironment<TSelf, TState> : RenderEnvironment
    where TSelf : DefinedEnvironment<TSelf, TState>, IDefined<ContentDefinition<TState>> where TState : notnull {
    private readonly ContentBody<TState> body;

    protected DefinedEnvironment() => body = new(this, TSelf.Definition);

    public TState State => body.State();

    public IO<RenderContent> Change(Func<TState, TState> change) => body.Changed(this, change);

    protected abstract IO<Unit> Simulated(TState state, SimulatedEnvironment simulation, bool isForDataOnly);

    public sealed override string TypeName => body.TypeName;

    public sealed override string TypeDescription => body.TypeDescription;

    public sealed override IEnumerable<string> FilesToEmbed => body.FilesToEmbed;

    public sealed override bool VirtualIcon(Size size, out Bitmap? bitmap) => (bitmap = body.Icon(size).ValueUnsafe()) is not null;

    protected sealed override void OnAddUserInterfaceSections() => body.Sections(this);

    protected sealed override uint CalculateRenderHash2(CrcRenderHashFlags flags, string[] excludeParameterNames) => base.CalculateRenderHash2(flags, body.Excluded(excludeParameterNames));

    public sealed override void SimulateEnvironment(ref SimulatedEnvironment simulation, bool isForDataOnly) {
        SimulatedEnvironment target = simulation;
        if (!Session.Run(Simulated(body.State(), target, isForDataOnly).Map(static _ => true), failed: false))
            base.SimulateEnvironment(ref simulation, isForDataOnly);
    }
}
