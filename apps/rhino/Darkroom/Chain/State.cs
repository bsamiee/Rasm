using Darkroom.Catalog;
using Rasm.Imaging.Pixels;
using Rasm.Rhino.Document;
using Rasm.Rhino.Persistence;
using Rasm.Rhino.Render.Effects;
using Rhino;
using Rhino.Render.PostEffects;

namespace Darkroom.Chain;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ChainEntry(Guid Kind, bool On, float Mix);

public sealed record EffectRow<TState>(TState Default, Func<TState, PassContext, Fin<Option<PixelPass>>> Pass) {
    public string Name { get; init; } = typeof(TState).Name;
    public Seq<string> StatePath { get; init; }
    public Seq<(string Name, Func<TState, TState> Look)> Looks { get; init; }
    public Func<TState, Seq<GuideChannel>> Channels { get; init; } = static _ => [];
    public Func<TState, float, PassContext, PassContext> Continued { get; init; } = static (_, _, context) => context;
    public Option<float> Mix { get; init; } = Some(1f);
    public bool Listed { get; init; }
}

public sealed record EffectKind(
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

public sealed record ChainState(Seq<ChainEntry> Entries, ValueSet Values) {
    private static readonly Lazy<(HashMap<Guid, (EffectKind Kind, int Index)> Kinds, ChainState Default)> Declared = new(static () => (
        toHashMap(Kinds.Ordered.Map(static (kind, index) => (kind.Id, (kind, index)))),
        new ChainState([.. Kinds.Ordered.Filter(static kind => kind.Listed).Map(static kind => new ChainEntry(kind.Id, On: true, kind.Mix.IfNone(1f)))], ValueSet.Empty)));

    // --- [CATALOG]
    public static ChainState Default => Declared.Value.Default;

    public static Fin<EffectKind> Kind(Guid id) => Cataloged(id).Map(static held => held.Kind);

    // --- [CHAIN]
    private Fin<Seq<(EffectKind Kind, float Mix)>> Running =>
        Entries.Filter(static entry => entry.On).Traverse(static entry => Kind(entry.Kind).Map(kind => (Kind: kind, entry.Mix))).As()
            .Map(static runs => toSeq(runs.OrderBy(static run => run.Kind.Stage)));

    public Fin<Seq<GuideChannel>> Needs =>
        Running.Bind(runs => runs.Traverse(run => run.Kind.Channels(Values)).As()).Map(static needs => needs.Flatten().Distinct());

    public Fin<(Seq<(EffectKind Kind, PixelPass Pass)> Passes, PassContext Context)> Passes(PassContext context) =>
        Running.Bind(runs => runs.FoldM((Passes: Seq<(EffectKind Kind, PixelPass Pass)>(), Context: context), (held, run) =>
            run.Kind.Step(Values, run.Mix, held.Context).Map(staged =>
                (Passes: held.Passes + staged.Pass.Map(pass => (run.Kind, Pass: pass)).ToSeq(), staged.Context))).As());

    public Fin<Seq<PixelPass.Point>> Baked(PassContext context) =>
        Passes(context).Map(static chain => chain.Passes
            .Filter(step => chain.Context.Formed || step.Kind.Stage is PostEffectType.Early)
            .Choose(static step => step.Pass is PixelPass.Point point ? Some(point) : None));

    // --- [TRANSITIONS]
    public Fin<ChainState> Added(Guid kind) =>
        Cataloged(kind).Map(held => Entries.Exists(entry => entry.Kind == kind) ? this
            : this with { Entries = Inserted(Entries, new ChainEntry(kind, On: true, held.Kind.Mix.IfNone(1f)), entry => Placed(entry.Kind).Exists(other => other.CompareTo((held.Kind.Stage, held.Index)) > 0)) });

    public Fin<ChainState> Removed(Guid kind) =>
        Present(kind).Map(_ => this with { Entries = [.. Entries.Filter(entry => entry.Kind != kind)] });

    public Fin<ChainState> Toggled(Guid kind, bool on) =>
        Present(kind).Map(_ => this with { Entries = [.. Entries.Map(entry => entry.Kind == kind ? entry with { On = on } : entry)] });

    public Fin<ChainState> Mixed(Guid kind, float mix) =>
        (Present(kind), Kind(kind).Bind(held => held.Mix.ToFin(new DarkroomFault(kind, DarkroomReason.Unmixed))))
            .Apply((entry, _) => this with { Entries = [.. Entries.Map(other => other == entry ? entry with { Mix = mix } : other)] }).As();

    public Fin<ChainState> Moved(Guid kind, Option<Guid> before) =>
        from found in (Present(kind), Placed(kind)).Apply(static (entry, place) => (Entry: entry, Place: place)).As()
        from target in before.Traverse(other => Present(other).Bind(_ => Placed(other)).Bind(at =>
            at.Stage == found.Place.Stage ? Fin.Succ(other) : new DarkroomFault(other, DarkroomReason.Staged))).As()
        select target == Some(kind) ? this : this with {
            Entries = Inserted(Entries.Filter(held => held.Kind != kind), found.Entry, target.Match<Func<ChainEntry, bool>>(
                static other => held => held.Kind == other,
                () => held => Placed(held.Kind).Exists(at => at.Stage > found.Place.Stage))),
        };

    public Fin<ChainState> Tuned(Guid kind, ValueSet values) =>
        Kind(kind).Map(held => held.Owner switch {
            var owner => this with {
                Values = new(Values.Entries.Filter((key, _) => !string.Equals(key.Owner, owner, StringComparison.Ordinal))
                    .AddOrUpdateRange(values.Entries.Filter((key, _) => string.Equals(key.Owner, owner, StringComparison.Ordinal)).AsIterable())),
            },
        });

    public Fin<ChainState> Looked(Guid kind, string name) =>
        from held in Kind(kind)
        from look in held.Looks.Find(kept => string.Equals(kept.Name, name, StringComparison.Ordinal)).ToFin(new DarkroomFault(name, DarkroomReason.Missing))
        from added in Added(kind)
        from values in look.Look(added.Values)
        from looked in added.Tuned(kind, values)
        select looked;

    private Fin<ChainEntry> Present(Guid kind) =>
        Entries.Find(entry => entry.Kind == kind).ToFin(new DarkroomFault(kind, DarkroomReason.Unchained));

    private static Fin<(EffectKind Kind, int Index)> Cataloged(Guid kind) =>
        Declared.Value.Kinds.Find(kind).ToFin(new DarkroomFault(kind, DarkroomReason.Unlisted));

    private static Fin<(PostEffectType Stage, int Index)> Placed(Guid kind) => Cataloged(kind).Map(static held => (held.Kind.Stage, held.Index));

    private static Seq<ChainEntry> Inserted(Seq<ChainEntry> entries, ChainEntry entry, Func<ChainEntry, bool> after) =>
        entries.Span(held => !after(held)) switch { var (before, following) => [.. before, entry, .. following] };

    // --- [DOCUMENT]
    public static IO<ChainState> Read(RhinoDoc doc) => EffectList.State(new EffectSource.Document(doc), typeof(DarkroomEffect).GUID, Default);
}

[SmartEnum<string>]
public sealed partial class ChainMembership {
    public static readonly ChainMembership Add = new("add", static (state, kind) => state.Added(kind));
    public static readonly ChainMembership Remove = new("remove", static (state, kind) => state.Removed(kind));

    [UseDelegateFromConstructor]
    public partial Fin<ChainState> Changed(ChainState state, Guid kind);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ChainEdit {
    public sealed record Membership(Guid Kind, ChainMembership Change) : ChainEdit;

    public sealed record Toggle(Guid Kind, bool On) : ChainEdit;

    public sealed record Move(Guid Kind, Option<Guid> Before) : ChainEdit;

    public sealed record Tune(Guid Kind, ValueSet Values) : ChainEdit;

    public sealed record Mix(Guid Kind, float Value) : ChainEdit;

    public sealed record ApplyLook(Guid Kind, string Name) : ChainEdit;

    public sealed record Restore(ChainState State) : ChainEdit;

    // --- [COMMIT]
    private Option<Guid> Target => Switch<Option<Guid>>(
        membership: static edit => edit.Kind,
        toggle: static edit => edit.Kind,
        move: static edit => edit.Kind,
        tune: static edit => edit.Kind,
        mix: static edit => edit.Kind,
        applyLook: static edit => edit.Kind,
        restore: static _ => None);

    public IO<ChainState> Committed(RhinoDoc doc) =>
        from prior in ChainState.Read(doc)
        from next in IO.lift(Applied(prior))
        from committed in when(next != prior, Commits.Commit(doc, string.Join(' ', GetType().Name.Cons(Target.Bind(static id => ChainState.Kind(id).ToOption()).Map(static kind => kind.Name).ToSeq())),
            new RedrawPolicy.AllViews(Deferred: true), EffectList.Tune(new EffectSource.Document(doc), typeof(DarkroomEffect).GUID, next)).Map(static _ => unit)).As()
        select next;

    private Fin<ChainState> Applied(ChainState prior) =>
        Switch(prior,
            membership: static (state, edit) => edit.Change.Changed(state, edit.Kind),
            toggle: static (state, edit) => state.Toggled(edit.Kind, edit.On),
            move: static (state, edit) => state.Moved(edit.Kind, edit.Before),
            tune: static (state, edit) => state.Tuned(edit.Kind, edit.Values),
            mix: static (state, edit) => state.Mixed(edit.Kind, edit.Value),
            applyLook: static (state, edit) => state.Looked(edit.Kind, edit.Name),
            restore: static (_, edit) => edit.State);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class EffectRow {
    public static EffectRow<TState> Of<TState>() where TState : IPixelStage<TState> =>
        new(TState.Default, TState.Pass) { Looks = TState.Looks, Channels = TState.Channels, Continued = TState.Continued };
}
