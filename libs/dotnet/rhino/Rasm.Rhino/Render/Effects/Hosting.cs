using Rasm.Rhino.PlugIns;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.UI;
using Rhino.Render;
using Rhino.Render.PostEffects;

namespace Rasm.Rhino.Render.Effects;

// --- [SERVICES] ------------------------------------------------------------------------
public abstract class DefinedEffect<TState> : PostEffect where TState : notnull {
    // --- [STATE]
    private readonly Atom<TState> held;

    protected DefinedEffect(TState @default) => (Default, held, Owner) = (@default, Atom(@default), HostedTypes.Owner(GetType()));

    protected IPlugInViews Owner { get; }

    protected TState Default { get; }

    protected TState State => held.Value;

    // --- [PARAMETERS]
    public sealed override bool GetParam(string param, ref object v) {
        bool found = string.Equals(param, EffectList.StateKey, StringComparison.Ordinal);
        v = found ? EffectList.Encoded(held.Value) : v;
        return found;
    }

    public sealed override bool SetParam(string param, object v) =>
        Callbacks.Succeeded(
            Optional(v as string).Filter(_ => string.Equals(param, EffectList.StateKey, StringComparison.Ordinal))
                .Map(text => Changing(IO.lift(EffectList.Decoded<TState>(text)).Bind(state => writeIO(held, state)).Map(unit))),
            static () => false,
            CallbackSite.Of(Owner, this));

    // --- [PERSISTENCE]
    public sealed override bool ReadState(PostEffectState state) =>
        Callbacks.Succeeded(
            Callbacks.Found(state.TryGetValue(EffectList.StateKey, out string text), text)
                .Traverse(stored => IO.lift(EffectList.Decoded<TState>(stored)).Bind(read => writeIO(held, read))).As().Map(unit),
            CallbackSite.Of(Owner, this));

    public sealed override bool WriteState(ref PostEffectState state) =>
        Callbacks.Succeeded(IO.lift(Refused.Unless(state.SetValue(EffectList.StateKey, EffectList.Encoded(held.Value)), nameof(PostEffectState.SetValue))), CallbackSite.Of(Owner, this));

    public sealed override void ResetToFactoryDefaults() =>
        Callbacks.Run(Changing(writeIO(held, Default).Map(unit)), CallbackSite.Of(Owner, this));

    private IO<Unit> Changing(IO<Unit> next) =>
        DisposalOps.Within(() => { BeginChange(RenderContent.ChangeContexts.Program); return unit; },
            _ => next.Bind(_ => IO.lift(Changed)),
            _ => IO.lift(EndChange).Map(unit));

    // --- [UI]
    protected virtual Seq<Func<ValueStore<TState>, View.Section>> Sections => [];

    protected virtual Option<HelpTopic> Help => None;

    public sealed override void AddUISections(PostEffectUI ui) =>
        Callbacks.Run(
            Callbacks.Each(Sections.Map(build => DisposalOps.Acquire(IO.lift(() => new DefinedEffectSection<TState>(Owner, this, Default, build)), ui.AddSection))).Map(unit),
            CallbackSite.Of(Owner, this));

    public sealed override bool DisplayHelp() =>
        Callbacks.Succeeded(Help.Map(static topic => topic.Show), static () => false, CallbackSite.Of(Owner, this));
}
