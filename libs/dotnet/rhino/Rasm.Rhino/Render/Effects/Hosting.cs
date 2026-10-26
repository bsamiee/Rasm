using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.PlugIns;
using Rasm.Rhino.UI;
using Rasm.UI;
using Rhino.Render;
using Rhino.Render.PostEffects;

namespace Rasm.Rhino.Render.Effects;

// --- [SERVICES] ------------------------------------------------------------------------
public abstract class DefinedEffect<TState> : PostEffect where TState : IStateRecord<TState> {
    // --- [STATE]
    private static readonly HashMap<string, RecordEntry<TState>> Entries =
        toHashMap(RecordEntries.Items<TState>().Map(static entry => (EntryKey.Name(entry.Path), entry)));

    private readonly Atom<TState> held = Atom(TState.Default);

    protected DefinedEffect() => Owner = HostedTypes.Owner(GetType());

    protected IPlugInViews Owner { get; }

    protected TState State => held.Value;

    private string EntryOwner => EntryKey.TypeOwner(GetType().GUID);

    // --- [PARAMETERS]
    public sealed override bool GetParam(string param, ref object v) {
        Option<string> text = Entries.Find(param).Bind(entry => entry.Capture(held.Value));
        v = text.Map(static found => (object)found).IfNone(v);
        return text.IsSome;
    }

    public sealed override bool SetParam(string param, object v) =>
        Callbacks.Succeeded(
            Entries.Find(param).Map(entry => Changing(
                from step in IO.lift(() => new EntryKey(EntryOwner, entry.Path).Read(StoredText.Format(v), entry.Recall))
                from set in step
                from swapped in held.SwapIO(set)
                select unit)),
            static () => false,
            CallbackSite.Of(Owner, this));

    // --- [PERSISTENCE]
    public sealed override bool ReadState(PostEffectState state) =>
        Callbacks.Succeeded(
            from read in new FieldCodec<TState>(TState.Parameters, TState.Default)
                .Folded(EntryOwner, path => IO.lift(() => Callbacks.Found(state.TryGetValue(EntryKey.Name(path), out string text), text)))
            from written in writeIO(held, read.Value)
            from refused in unless(read.Refused.IsEmpty, IO.fail<Unit>(Error.Many(read.Refused))).As()
            select unit,
            CallbackSite.Of(Owner, this));

    public sealed override bool WriteState(ref PostEffectState state) {
        PostEffectState store = state;
        return Callbacks.Succeeded(
            Callbacks.Each(EffectRequest.Tuning.Of(Id, held.Value), tuning => store.SetValue(tuning.Key, tuning.Text), nameof(PostEffectState.SetValue)),
            CallbackSite.Of(Owner, this));
    }

    public sealed override void ResetToFactoryDefaults() =>
        Callbacks.Run(Changing(writeIO(held, TState.Default).Map(unit)), CallbackSite.Of(Owner, this));

    private IO<Unit> Changing(IO<Unit> next) =>
        DisposalOps.Within(() => { BeginChange(RenderContent.ChangeContexts.Program); return unit; },
            _ => next.Bind(_ => IO.lift(Changed)),
            _ => IO.lift(EndChange).Map(unit));

    // --- [UI]
    protected virtual Seq<Func<ValueStore<TState>, View.Section>> Sections => [];

    protected virtual Option<HelpTopic> Help => None;

    public sealed override void AddUISections(PostEffectUI ui) =>
        Callbacks.Run(
            Callbacks.Each(Sections.Map(build => DisposalOps.Acquire(IO.lift(() => new DefinedEffectSection<TState>(Owner, this, build)), ui.AddSection))).Map(unit),
            CallbackSite.Of(Owner, this));

    public sealed override bool DisplayHelp() =>
        Callbacks.Succeeded(Help.Map(static topic => topic.Show), static () => false, CallbackSite.Of(Owner, this));
}
