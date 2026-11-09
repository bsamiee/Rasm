using Darkroom.Catalog;
using Rasm.Imaging.Pixels;
using Rasm.Rhino.Document;
using Rasm.Rhino.Persistence;
using Rasm.Rhino.Render.Effects;
using Rhino;
using Rhino.Render.PostEffects;

namespace Darkroom.Chain;

// --- [MODELS] --------------------------------------------------------------------------

internal sealed record ChainEntry(Guid Kind, bool On, float Mix);

internal sealed record EffectRow<TState>(TState Default, Func<TState, PassContext, Fin<Option<PixelPass>>> Pass) {
    public string Name { get; init; } = typeof(TState).Name;
    public Seq<string> StatePath { get; init; }
    public Seq<(string Name, Func<TState, TState> Look)> Looks { get; init; }
    public Func<TState, Seq<GuideChannel>> Channels { get; init; } = static _ => [];
    public Func<TState, float, PassContext, PassContext> Continued { get; init; } = static (_, _, context) => context;
    public Option<float> Mix { get; init; } = Some(1f);
    public bool Listed { get; init; }
}

internal sealed record EffectKind(
    Guid Id, string Name, PostEffectType Stage, bool Listed, Option<float> Mix,
    Seq<(string Name, Func<ValueSet, Fin<ValueSet>> Look)> Looks,
    Func<ValueSet, float, PassContext, Fin<(Option<PixelPass> Pass, PassContext Context)>> Step,
    Func<ValueSet, Fin<Seq<GuideChannel>>> Channels) {
    public string Owner => EntryKey.TypeOwner(Id);

    public static EffectKind Of<TState>(Guid id, PostEffectType stage, EffectRow<TState> row) =>
        EntryKey.TypeOwner(id) switch {
            var owner => new(
                id, row.Name, stage, row.Listed, row.Mix,
                row.Looks.Map(look => (look.Name, (Func<ValueSet, Fin<ValueSet>>)(values => values.Recalled(owner, row.Default, row.StatePath).Map(state => ValueSet.Fields(owner, look.Look(state), row.StatePath))))),
                (values, mix, context) =>
                    from state in values.Recalled(owner, row.Default, row.StatePath)
                    from found in row.Pass(state, context with { Derivations = context.Derivations.Owned(id) })
                    let staged = found.Bind(pass => BlendingMode.Normal.Mixed(pass, mix, []))
                    select (staged, staged.IsSome ? row.Continued(state, mix, context) : context),
                values => values.Recalled(owner, row.Default, row.StatePath).Map(row.Channels)),
        };
}

internal sealed record ChainState(Seq<ChainEntry> Entries, ValueSet Values) {

    // --- [CATALOG]

    private static readonly Lazy<(HashMap<Guid, (EffectKind Kind, int Index)> Kinds, ChainState Default)> Declared = new(static () => (
        toHashMap(Kinds.Ordered.Map(static (kind, index) => (kind.Id, (kind, index)))),
        new ChainState([.. Kinds.Ordered.Filter(static kind => kind.Listed).Map(static kind => new ChainEntry(kind.Id, On: true, kind.Mix.IfNone(1f)))], ValueSet.Empty)));

    internal static ChainState Default => Declared.Value.Default;

    internal static Fin<EffectKind> Kind(Guid id) => Cataloged(id).Map(static held => held.Kind);

    // --- [CHAINS]

    private Fin<Seq<(EffectKind Kind, float Mix)>> Running =>
        Entries.Filter(static entry => entry.On).Traverse(static entry => Kind(entry.Kind).Map(kind => (Kind: kind, entry.Mix))).As()
            .Map(static runs => toSeq(runs.OrderBy(static run => run.Kind.Stage)));

    internal Fin<Seq<GuideChannel>> Needs =>
        Running.Bind(runs => runs.Traverse(run => run.Kind.Channels(Values)).As()).Map(static needs => needs.Flatten().Distinct());

    internal Fin<(Seq<(EffectKind Kind, PixelPass Pass)> Passes, PassContext Context)> Passes(PassContext context) =>
        Running.Bind(runs => runs.FoldM((Passes: Seq<(EffectKind Kind, PixelPass Pass)>(), Context: context), (held, run) =>
            run.Kind.Step(Values, run.Mix, held.Context).Map(staged =>
                (Passes: held.Passes + staged.Pass.Map(pass => (run.Kind, Pass: pass)).ToSeq(), staged.Context))).As());

    internal Fin<Seq<PixelPass.Point>> Baked(PassContext context) =>
        Passes(context).Map(static chain => chain.Passes
            .Filter(step => chain.Context.Formed || step.Kind.Stage is PostEffectType.Early)
            .Choose(static step => step.Pass is PixelPass.Point point ? Some(point) : None));

    private static Fin<(EffectKind Kind, int Index)> Cataloged(Guid kind) =>
        Declared.Value.Kinds.Find(kind).ToFin(new DarkroomFault(kind, DarkroomReason.Unlisted));

    // --- [DOCUMENTS]

    internal static IO<ChainState> Read(RhinoDoc doc) => EffectList.State<DarkroomEffect, ChainState>(new EffectSource.Document(doc));

    internal IO<ChainState> Restored(RhinoDoc doc) =>
        from prior in Read(doc)
        from committed in when(this != prior, Commits.Commit(doc, nameof(Restored), new RedrawPolicy.AllViews(Deferred: true), EffectList.Tune<DarkroomEffect, ChainState>(new EffectSource.Document(doc), this)).Map(static _ => unit)).As()
        select this;
}

// --- [OPERATIONS] ----------------------------------------------------------------------

internal static class EffectRow {
    public static EffectRow<TState> Of<TState>() where TState : IPixelStage<TState> =>
        new(TState.Default, TState.Pass) { Looks = TState.Looks, Channels = TState.Channels, Continued = TState.Continued };
}
