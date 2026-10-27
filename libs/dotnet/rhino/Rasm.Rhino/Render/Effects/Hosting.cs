using Rasm.RhinoCommon.PlugIns;
using Rasm.RhinoCommon.Persistence.Stores;
using Rasm.RhinoCommon.UI;
using Rhino.Render;
using Rhino.Render.PostEffects;

namespace Rasm.RhinoCommon.Render.Effects;

// --- [SERVICES] ------------------------------------------------------------------------
public abstract class DefinedEffect<TState> : PostEffect where TState : notnull {
    // --- [STATE]
    private readonly Atom<TState> held;

    protected DefinedEffect(TState @default) => (Default, held, Owner) = (@default, Atom(@default), PlugInOwners.Owner(GetType()));

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
        Owner.Answer(
            Optional(v as string).Filter(_ => string.Equals(param, EffectList.StateKey, StringComparison.Ordinal))
                .Map(text => Changing(IO.lift(EffectList.Decoded<TState>(text)).Bind(state => writeIO(held, state)).Map(unit)).Map(value: true))
                .IfNone(IO.pure(value: false)),
            failed: false);

    // --- [PERSISTENCE]
    public sealed override bool ReadState(PostEffectState state) =>
        Owner.Answer(
            Conversions.Found(state.TryGetValue(EffectList.StateKey, out string text), text)
                .Traverse(stored => IO.lift(EffectList.Decoded<TState>(stored)).Bind(read => writeIO(held, read))).As().Map(value: true),
            failed: false);

    public sealed override bool WriteState(ref PostEffectState state) =>
        Owner.Answer(IO.lift(RhinoFault.Unless(state.SetValue(EffectList.StateKey, EffectList.Encoded(held.Value)))).Map(value: true), failed: false);

    public sealed override void ResetToFactoryDefaults() => _ = Owner.Run(Changing(writeIO(held, Default).Map(unit)));

    private IO<Unit> Changing(IO<Unit> next) =>
        IO.lift(() => BeginChange(RenderContent.ChangeContexts.Program)) >> next.Bind(_ => IO.lift(Changed)).Finally(IO.lift(EndChange));

    // --- [UI]
    protected virtual Seq<Func<ValueStore<TState>, View.Section>> Sections => [];

    protected virtual Option<HelpTopic> Help => None;

    public sealed override void AddUISections(PostEffectUI ui) =>
        _ = Owner.Run(Callbacks.Each(Sections.Map(build => IO.lift(() => ui.AddSection(new DefinedEffectSection<TState>(Owner, this, Default, build))))));

    public sealed override bool DisplayHelp() =>
        Owner.Answer(Help.Map(static topic => topic.Show.Map(value: true)).IfNone(IO.pure(value: false)), failed: false);
}
