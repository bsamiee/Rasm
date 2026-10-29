using System.Drawing;
using CommunityToolkit.HighPerformance;
using Rasm.Rhino.Persistence;
using Rasm.Rhino.UI;
using Rhino.Render;
using Rhino.Render.PostEffects;

namespace Rasm.Rhino.Render.Effects;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record EffectFrame(Rectangle Rect, HashMap<RenderWindow.StandardChannels, ReadOnlyMemory2D<float>> Inputs, Memory2D<float> Rgba);

public sealed record EffectDefinition<TState>(TState Default, Action<TState, EffectFrame> Execute) where TState : notnull {
    public Seq<RenderWindow.StandardChannels> Channels { get; init; }
    public bool Threaded { get; init; }
    public Seq<Func<Preference<TState>, View.Section>> Sections { get; init; }
    public Option<HelpTopic> Help { get; init; }
}

// --- [SERVICES] ------------------------------------------------------------------------
public abstract class DefinedEffect<TSelf, TState> : PostEffect
    where TSelf : DefinedEffect<TSelf, TState>, IDefined<EffectDefinition<TState>> where TState : notnull {
    // --- [STATE]
    private readonly Atom<TState> held = Atom(TSelf.Definition.Default);

    // --- [EXECUTION]
    public sealed override Guid[] RequiredChannels => Channels.Required(TSelf.Definition.Channels);

    public sealed override bool Execute(PostEffectPipeline pipeline, Rectangle rect) =>
        Session.Run(EffectJob.Run(this, pipeline, rect, held.Value, TSelf.Definition).Map(static _ => true), failed: false);

    // --- [PARAMETERS]
    public sealed override bool GetParam(string param, ref object v) {
        bool found = string.Equals(param, EffectList.StateKey, StringComparison.Ordinal);
        v = found ? EffectList.Encoded(held.Value) : v;
        return found;
    }

    public sealed override bool SetParam(string param, object v) =>
        Session.Run(
            Optional(v as string).Filter(_ => string.Equals(param, EffectList.StateKey, StringComparison.Ordinal))
                .Map(text => Changing(IO.lift(EffectList.Decoded<TState>(text))).Map(static _ => true))
                .IfNone(IO.pure(value: false)),
            failed: false);

    // --- [PERSISTENCE]
    public sealed override bool ReadState(PostEffectState state) =>
        Session.Run(
            Conversions.Present(state.TryGetValue(EffectList.StateKey, out string text), text)
                .Traverse(stored => IO.lift(EffectList.Decoded<TState>(stored)).Bind(read => writeIO(held, read))).As().Map(static _ => true),
            failed: false);

    public sealed override bool WriteState(ref PostEffectState state) {
        PostEffectState target = state;
        return Session.Run(Products.Accepted(() => target.SetValue(EffectList.StateKey, EffectList.Encoded(held.Value))).Map(static _ => true), failed: false);
    }

    public sealed override void ResetToFactoryDefaults() => _ = Session.Run(Changing(IO.pure(TSelf.Definition.Default)));

    private IO<Unit> Changing(IO<TState> next) =>
        IO.lift(() => BeginChange(RenderContent.ChangeContexts.Program))
        >> next.Bind(state => writeIO(held, state)).Bind(_ => IO.lift(Changed)).Finally(IO.lift(() => _ = EndChange()));

    // --- [UI]
    public sealed override void AddUISections(PostEffectUI ui) =>
        _ = Session.Run(Products.Each(TSelf.Definition.Sections.Map(build => IO.lift(() => ui.AddSection(new DefinedEffectSection<TState>(this, TSelf.Definition.Default, build))))));

    public sealed override bool DisplayHelp() =>
        Session.Run(TSelf.Definition.Help.Map(static topic => topic.Show.Map(static _ => true)).IfNone(IO.pure(value: false)), failed: false);
}
