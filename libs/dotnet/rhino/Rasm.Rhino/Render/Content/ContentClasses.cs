using System.Drawing;
using System.Text;
using Rasm.Imaging.Pixels;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.Render.Effects;
using Rasm.Rhino.UI.Assets;
using Rasm.Rhino.UI.Rows;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.PlugIns;
using Rhino.Render;

namespace Rasm.Rhino.Render.Content;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ContentPreviewRow(
    RenderPlugIn.PreviewRenderTypes Type,
    Func<CreatePreviewEventArgs, IO<Option<Bitmap>>> Content,
    Func<CreateTexture2dPreviewEventArgs, IO<Option<Bitmap>>> Texture);

// --- [SERVICES] ------------------------------------------------------------------------
public abstract class DefinedMaterial<TState> : RenderMaterial where TState : IStateRecord<TState> {
    // --- [FIELDS]
    protected DefinedMaterial() => _ = Callbacks.Answer(ContentRecords.Declare<TState>(this), static () => unit, CallbackSite.Of(this));

    public sealed override IEnumerable<string> FilesToEmbed => Callbacks.Answer(ContentRecords.Files<TState>(this), static () => Seq<string>(), CallbackSite.Of(this));

    protected sealed override uint CalculateRenderHash2(CrcRenderHashFlags flags, string[] excludeParameterNames) =>
        base.CalculateRenderHash2(flags, excludeParameterNames) switch {
            var seed => Callbacks.Answer(ContentRecords.Hash<TState>(this, toSeq(excludeParameterNames), seed), () => seed, CallbackSite.Of(this)),
        };

    // --- [VALUES]
    protected abstract string Caption { get; }

    protected abstract string Help { get; }

    protected abstract IGlyph Glyph { get; }

    protected abstract Material Simulation(TState state);

    // --- [IDENTITY]
    public sealed override string TypeName => RowText.Localize(Caption, table: Some<object>(IPlugInSink.Of(this))).Local;

    public sealed override string TypeDescription => RowText.Localize(Help, table: Some<object>(IPlugInSink.Of(this))).Local;

    public sealed override bool VirtualIcon(Size size, out Bitmap? bitmap) {
        bitmap = Callbacks.Answer(Icons.Raster(IPlugInSink.Of(this), Glyph, size).Map(static Bitmap? (image) => image), static () => null, CallbackSite.Of(this));
        return bitmap is not null;
    }

    // --- [SIMULATION]
    public sealed override void SimulateMaterial(ref Material simulation, RenderTexture.TextureGeneration tg) {
        Material target = simulation;
        if (!Callbacks.Succeeded(
                ContentRecords.Read<TState>(this).Bind(state => use(() => Simulation(state)).Bind(built => IO.lift(() => target.CopyFrom(built))).Bracket()),
                CallbackSite.Of(this)))
            base.SimulateMaterial(ref simulation, tg);
    }
}

public abstract class DefinedTexture<TState> : RenderTexture where TState : IStateRecord<TState> {
    // --- [FIELDS]
    protected DefinedTexture() => _ = Callbacks.Answer(ContentRecords.Declare<TState>(this), static () => unit, CallbackSite.Of(this));

    public sealed override IEnumerable<string> FilesToEmbed => Callbacks.Answer(ContentRecords.Files<TState>(this), static () => Seq<string>(), CallbackSite.Of(this));

    protected sealed override uint CalculateRenderHash2(CrcRenderHashFlags flags, string[] excludeParameterNames) =>
        base.CalculateRenderHash2(flags, excludeParameterNames) switch {
            var seed => Callbacks.Answer(ContentRecords.Hash<TState>(this, toSeq(excludeParameterNames), seed), () => seed, CallbackSite.Of(this)),
        };

    // --- [VALUES]
    protected abstract string Caption { get; }

    protected abstract string Help { get; }

    protected abstract IGlyph Glyph { get; }

    protected abstract Color4f Sample(TState state, Point3d uvw, Vector3d duvwdx, Vector3d duvwdy);

    // --- [IDENTITY]
    public sealed override string TypeName => RowText.Localize(Caption, table: Some<object>(IPlugInSink.Of(this))).Local;

    public sealed override string TypeDescription => RowText.Localize(Help, table: Some<object>(IPlugInSink.Of(this))).Local;

    public sealed override bool VirtualIcon(Size size, out Bitmap? bitmap) {
        bitmap = Callbacks.Answer(Icons.Raster(IPlugInSink.Of(this), Glyph, size).Map(static Bitmap? (image) => image), static () => null, CallbackSite.Of(this));
        return bitmap is not null;
    }

    // --- [SIMULATION]
    public sealed override TextureEvaluator? CreateEvaluator(TextureEvaluatorFlags evaluatorFlags) =>
        Callbacks.Answer(
            ContentRecords.Read<TState>(this).Map(state => (TextureEvaluator?)new Evaluator(
                evaluatorFlags, state, evaluatorFlags.HasFlag(TextureEvaluatorFlags.DisableLocalMapping) ? Transform.Identity : LocalMappingTransform, Sample)),
            static () => null,
            CallbackSite.Of(this));

    private sealed class Evaluator(TextureEvaluatorFlags flags, TState state, Transform mapping, Func<TState, Point3d, Vector3d, Vector3d, Color4f> sample)
        : TextureEvaluator(flags) {
        public override Color4f GetColor(Point3d uvw, Vector3d duvwdx, Vector3d duvwdy) => sample(state, mapping * uvw, mapping * duvwdx, mapping * duvwdy);
    }
}

public abstract class DefinedEnvironment<TState> : RenderEnvironment where TState : IStateRecord<TState> {
    // --- [FIELDS]
    protected DefinedEnvironment() => _ = Callbacks.Answer(ContentRecords.Declare<TState>(this), static () => unit, CallbackSite.Of(this));

    public sealed override IEnumerable<string> FilesToEmbed => Callbacks.Answer(ContentRecords.Files<TState>(this), static () => Seq<string>(), CallbackSite.Of(this));

    protected sealed override uint CalculateRenderHash2(CrcRenderHashFlags flags, string[] excludeParameterNames) =>
        base.CalculateRenderHash2(flags, excludeParameterNames) switch {
            var seed => Callbacks.Answer(ContentRecords.Hash<TState>(this, toSeq(excludeParameterNames), seed), () => seed, CallbackSite.Of(this)),
        };

    // --- [VALUES]
    protected abstract string Caption { get; }

    protected abstract string Help { get; }

    protected abstract IGlyph Glyph { get; }

    protected abstract EnvironmentState Simulation(TState state);

    // --- [IDENTITY]
    public sealed override string TypeName => RowText.Localize(Caption, table: Some<object>(IPlugInSink.Of(this))).Local;

    public sealed override string TypeDescription => RowText.Localize(Help, table: Some<object>(IPlugInSink.Of(this))).Local;

    public sealed override bool VirtualIcon(Size size, out Bitmap? bitmap) {
        bitmap = Callbacks.Answer(Icons.Raster(IPlugInSink.Of(this), Glyph, size).Map(static Bitmap? (image) => image), static () => null, CallbackSite.Of(this));
        return bitmap is not null;
    }

    // --- [SIMULATION]
    public sealed override void SimulateEnvironment(ref SimulatedEnvironment simulation, bool isForDataOnly) {
        SimulatedEnvironment target = simulation;
        if (!Callbacks.Succeeded(ContentRecords.Read<TState>(this).Bind(state => ContentKinds.WriteEnvironment(Simulation(state), target)), CallbackSite.Of(this)))
            base.SimulateEnvironment(ref simulation, isForDataOnly);
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ContentRecords {
    // --- [DECLARATIONS]
    public static IO<Unit> Declare<TState>(RenderContent content) where TState : IStateRecord<TState> =>
        Callbacks.Each(FieldTexts<TState>.Items.Map(field => (Name: EntryKey.Name(field.Path), Text: Conversions.Unset(field.Capture(TState.Default))) switch {
            var held =>
                from added in IO.lift(() => field.File.IsSome ? content.Fields.AddFilename(held.Name, held.Text, "", 0) : content.Fields.Add(held.Name, held.Text))
                from bound in ContentFields.BindParameterToField(content, held.Name, added)
                select bound,
        })).Map(static _ => unit);

    // --- [READS]
    public static IO<TState> Read<TState>(RenderContent content) where TState : IStateRecord<TState> =>
        FieldTexts.Recalled<TState>(EntryKey.TypeOwner(content.TypeId), path => Stored(content, EntryKey.Name(path)));

    public static IO<uint> Hash<TState>(RenderContent content, Seq<string> excluded, uint seed) where TState : IStateRecord<TState> =>
        IO.lift(() => FieldTexts<TState>.Items
            .Map(static field => EntryKey.Name(field.Path))
            .Filter(name => !excluded.Exists(held => string.Equals(held, name, StringComparison.Ordinal)))
            .Fold(seed, (crc, name) => Stored(content, name).Fold(crc, static (sum, text) => RhinoMath.CRC32(sum, Encoding.UTF8.GetBytes(text)))));

    public static IO<Seq<string>> Files<TState>(RenderContent content) where TState : IStateRecord<TState> =>
        IO.lift(() => FieldTexts<TState>.Items.Choose(field =>
            from file in field.File
            from text in Stored(content, EntryKey.Name(field.Path))
            from path in file(text)
            select path).Strict());

    private static Option<string> Stored(RenderContent content, string name) =>
        Callbacks.Found(content.Fields.TryGetValue(name, out string text), text).Bind(static held => Conversions.Present(held));

    // --- [WRITES]
    public static IO<Unit> Write<TState>(RenderContent content, RenderContent.ChangeContexts context, TState next) where TState : IStateRecord<TState> =>
        Contents.WithinContentChange(content, context, Callbacks.Each(FieldTexts<TState>.Items.Map(field => Written(content, EntryKey.Name(field.Path), Conversions.Unset(field.Capture(next))))).Map(static _ => unit));

    public static ValueStore<TState> Store<TState>(RenderContent content) where TState : IStateRecord<TState> =>
        ValueStore.Of(
            Read<TState>(content).Map(static state => Some(state).Filter(static held => !EqualityComparer<TState>.Default.Equals(held, TState.Default))),
            value => Write(content, RenderContent.ChangeContexts.UI, value.IfNone(TState.Default)),
            Applied.Live,
            None);

    private static IO<Unit> Written(RenderContent content, string name, string text) =>
        IO.lift(() => Stored(content, name) != Conversions.Present(text)).Bind(differs => when(differs, ContentFields.Set(content.Fields, name, text)).As());

    // --- [ROWS]
    public static Seq<ControlRow> Rows<TContent, TState, TParameter, TError>(Func<TParameter, ParameterText> text)
        where TContent : RenderContent
        where TState : IStateRecord<TState, TParameter, TError>
        where TParameter : class, IStateParameter<TState>, ISmartEnum<string, TParameter, TError>
        where TError : Error, IValidationError<TError> =>
        ParameterText.Join<TState, TParameter, TError>(text) switch {
            var texts => SectionRows.Of(
                SectionRows.Source(EntryKey.TypeOwner(typeof(TContent).GUID), texts, static scope => scope.Contents(Store<TState>)),
                texts,
                SectionRows.Unstaged,
                static _ => RowRules.Always),
        };
}
