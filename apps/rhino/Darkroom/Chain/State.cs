using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Darkroom.Catalog;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Pixels;
using Rasm.RhinoCommon;
using Rasm.RhinoCommon.Document;
using Rasm.RhinoCommon.Persistence.Stores;
using Rasm.RhinoCommon.Render.Effects;
using Rhino;
using Rhino.Render.PostEffects;

namespace Darkroom.Chain;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ChainEntry(
    Guid Kind, bool On,
    [property: Display(Name = "Mix", Description = "Blends the kind's output over its input, 0% leaves the input unchanged")][property: Range(0d, 1d)] float Mix);

public sealed record EffectRow<TState> where TState : IEffectState<TState> {
    public TState Default { get; init; } = TState.Default;

    public Option<float> Mix { get; init; } = Some(1f);

    public Seq<(string Name, Func<TState, TState> Look)> Looks { get; init; }

    public bool Listed { get; init; }
}

public sealed record EffectKind(
    Guid Id, Type State, PostEffectType Stage, Family Family, bool Listed, Option<float> Mix,
    Seq<(string Name, Func<ValueSet, Fin<ValueSet>> Look)> Looks,
    Func<ValueSet, float, PassContext, IO<(Option<PixelPass> Pass, PassContext Context)>> Step,
    Func<ValueSet, IO<Seq<GuideChannel>>> Channels,
    Func<RhinoDoc, ValueBinding> Binding) {
    public string Owner => EntryKey.TypeOwner(Id);

    // --- [DECLARATION]
    public static EffectKind Of<TState>(Guid id, PostEffectType stage, Family family, EffectRow<TState> row) where TState : IPixelStage<TState> =>
        Declared(id, stage, family, row, TState.Pass, TState.Continued, TState.Channels);

    public static EffectKind Job<TState, TResource>(Guid id, PostEffectType stage, Family family, EffectRow<TState> row)
        where TState : IEffectState<TState>
        where TResource : IFrameJob<TResource, TState> =>
        Declared(id, stage, family, row,
            static (state, context) => context.Rendering ? None : Some<PixelPass>(new PixelPass.Frame((frame, rows) =>
                context.Derivations.Held(unit, static _ => TResource.Open(), resource => resource.Run(state, frame, context.Guides, rows).Bind(result => Copied(result, frame))))),
            static (_, _, context) => context,
            TResource.Channels);

    private static EffectKind Declared<TState>(
        Guid id, PostEffectType stage, Family family, EffectRow<TState> row,
        Func<TState, PassContext, Option<PixelPass>> pass, Func<TState, float, PassContext, PassContext> continued, Func<TState, Seq<GuideChannel>> channels)
        where TState : IEffectState<TState> =>
        EntryKey.TypeOwner(id) switch {
            var owner => new(
                id, typeof(TState), stage, family, row.Listed, row.Mix,
                row.Looks.Map(look => (look.Name, (Func<ValueSet, Fin<ValueSet>>)(values => values.Recalled(owner, row.Default).Map(state => ValueSet.Fields(owner, look.Look(state)))))),
                (values, mix, context) =>
                    from state in IO.lift(values.Recalled(owner, row.Default))
                    let staged = pass(state, context).Bind(found => Blend.Mixed(found, BlendingMode.Mix, mix, [], context.Working))
                    select (staged, staged.IsSome ? continued(state, mix, context) : context),
                values => IO.lift(values.Recalled(owner, row.Default)).Map(channels),
                doc => ValueBinding.Of(owner, row.Default, ValueStores.Defaulted(
                    ChainState.Read(doc).Bind(chain => IO.lift(chain.Values.Recalled(owner, row.Default))),
                    IO.pure(row.Default),
                    state => new ChainEdit.Tune(id, ValueSet.Fields(owner, state)).Committed(doc).Map(unit),
                    EqualityComparer<TState>.Default.Equals, Applied.Live, Some(new EffectSource.Document(doc).Changed)))),
        };

    private static Fin<Unit> Copied(PixelFrame result, PixelFrame frame) =>
        RhinoFault.Unless(result.Size == frame.Size).Map(_ => {
            result.View.Span.CopyTo(frame.View.Span);
            return unit;
        });
}

public sealed record ChainState(ImmutableArray<ChainEntry> Entries, ValueSet Values) {
    private static readonly Lazy<(HashMap<Guid, (EffectKind Kind, int Index)> Kinds, ChainState Default)> Declared = new(static () => (
        toHashMap(Kinds.Ordered.Map(static (kind, index) => (kind.Id, (kind, index)))),
        new ChainState([.. Kinds.Ordered.Filter(static kind => kind.Listed).Map(static kind => new ChainEntry(kind.Id, On: true, kind.Mix.IfNone(1f)))], ValueSet.Empty)));

    // --- [CATALOG]
    public static ChainState Default => Declared.Value.Default;

    public static string Owner => EntryKey.TypeOwner(typeof(DarkroomEffect).GUID);

    public static Fin<EffectKind> Kind(Guid id) =>
        Declared.Value.Kinds.Find(id).Map(static held => held.Kind).ToFin(new DarkroomFault($"effect kind {id}", DarkroomReason.Unlisted));

    // --- [CHAIN]
    public Seq<(EffectKind Kind, float Mix)> Running =>
        toSeq(toSeq(Entries).Filter(static entry => entry.On).Choose(static entry => Kind(entry.Kind).ToOption().Map(kind => (Kind: kind, entry.Mix))).OrderBy(static run => run.Kind.Stage));

    public IO<Seq<GuideChannel>> Needs =>
        Running.TraverseM(run => run.Kind.Channels(Values)).As().Map(static needs => needs.Flatten().Distinct());

    public IO<(Seq<(EffectKind Kind, PixelPass Pass)> Passes, PassContext Context)> Passes(PassContext context) =>
        Running.Fold(IO.pure((Passes: Seq<(EffectKind Kind, PixelPass Pass)>(), Context: context)), (previous, run) =>
            from held in previous
            from staged in run.Kind.Step(Values, run.Mix, held.Context)
            select (Passes: held.Passes + staged.Pass.Map(pass => (run.Kind, Pass: pass)).ToSeq(), staged.Context));

    public IO<Seq<PixelPass.Color>> Baked(PassContext context) =>
        Passes(context).Map(static chain => chain.Passes
            .Filter(step => chain.Context.Formed || step.Kind.Stage is PostEffectType.Early)
            .Choose(static step => step.Pass is PixelPass.Color color ? Some(color) : None));

    public bool Equals([NotNullWhen(true)] ChainState? other) => other is not null && Entries.SequenceEqual(other.Entries) && Values == other.Values;

    public override int GetHashCode() => HashCode.Combine(Entries.Length, Values);

    // --- [TRANSITIONS]
    public Fin<ChainState> Added(Guid kind) =>
        (Kind(kind), Placed(kind)).Apply((held, place) => toSeq(Entries).Exists(entry => entry.Kind == kind) ? this
            : this with { Entries = Inserted(toSeq(Entries), new ChainEntry(kind, On: true, held.Mix.IfNone(1f)), entry => Placed(entry.Kind).Exists(other => other.CompareTo(place) > 0)) }).As();

    public Fin<ChainState> Removed(Guid kind) =>
        Present(kind).Map(_ => this with { Entries = [.. toSeq(Entries).Filter(entry => entry.Kind != kind)] });

    public Fin<ChainState> Toggled(Guid kind, bool on) =>
        Present(kind).Map(_ => this with { Entries = [.. toSeq(Entries).Map(entry => entry.Kind == kind ? entry with { On = on } : entry)] });

    public Fin<ChainState> Mixed(Guid kind, float mix) =>
        (Present(kind), Kind(kind).Bind(held => held.Mix.ToFin(new DarkroomFault($"effect kind {kind}", DarkroomReason.Unmixed))))
            .Apply((entry, _) => this with { Entries = [.. toSeq(Entries).Map(other => other == entry ? entry with { Mix = mix } : other)] }).As();

    public Fin<ChainState> Moved(Guid kind, Option<Guid> before) =>
        from found in (Present(kind), Placed(kind)).Apply(static (entry, place) => (Entry: entry, Place: place)).As()
        from target in before.Traverse(other => Present(other).Bind(_ => Placed(other)).Bind(at =>
            at.Stage == found.Place.Stage ? Fin.Succ(other) : new DarkroomFault($"effect kind {other}", DarkroomReason.Staged))).As()
        select this with {
            Entries = Inserted(toSeq(Entries).Filter(held => held.Kind != kind), found.Entry, target.Match<Func<ChainEntry, bool>>(
                static other => held => held.Kind == other,
                () => held => Placed(held.Kind).Exists(at => at.Stage > found.Place.Stage))),
        };

    public Fin<ChainState> Tuned(Guid kind, ValueSet values) =>
        Kind(kind).Map(held => this with {
            Values = new(Values.Entries.Filter((key, _) => !string.Equals(key.Owner, held.Owner, StringComparison.Ordinal))
                .AddOrUpdateRange(values.Entries.Filter((key, _) => string.Equals(key.Owner, held.Owner, StringComparison.Ordinal)).AsIterable())),
        });

    public Fin<ChainState> Looked(Guid kind, string name) =>
        from held in Kind(kind)
        from look in held.Looks.Find(kept => string.Equals(kept.Name, name, StringComparison.Ordinal)).ToFin(new DarkroomFault($"look {name}", DarkroomReason.Missing))
        from added in Added(kind)
        from values in look.Look(added.Values)
        from looked in added.Tuned(kind, values)
        select looked;

    private Fin<ChainEntry> Present(Guid kind) =>
        toSeq(Entries).Find(entry => entry.Kind == kind).ToFin(new DarkroomFault($"effect kind {kind}", DarkroomReason.Unchained));

    private static Fin<(PostEffectType Stage, int Index)> Placed(Guid kind) =>
        Declared.Value.Kinds.Find(kind).Map(static held => (held.Kind.Stage, held.Index)).ToFin(new DarkroomFault($"effect kind {kind}", DarkroomReason.Unlisted));

    private static ImmutableArray<ChainEntry> Inserted(Seq<ChainEntry> entries, ChainEntry entry, Func<ChainEntry, bool> after) =>
        [.. entries.TakeWhile(held => !after(held)), entry, .. entries.SkipWhile(held => !after(held))];

    // --- [DOCUMENT]
    public static IO<ChainState> Read(RhinoDoc doc) => EffectList.State(new EffectSource.Document(doc), typeof(DarkroomEffect).GUID, Default);

    public static BindingGroup Bindings(RhinoDoc doc) =>
        new(Kinds.Ordered.Map(kind => kind.Binding(doc)).Add(ValueBinding.Of(Owner, Default.Entries, ValueStores.Defaulted(
            Read(doc).Map(static state => state.Entries),
            IO.pure(Default.Entries),
            entries => Read(doc).Bind(state => new ChainEdit.Restore(state with { Entries = entries }).Committed(doc)).Map(unit),
            static (held, next) => held.SequenceEqual(next), Applied.Live, Some(new EffectSource.Document(doc).Changed)))));
}

[SmartEnum<string>]
public sealed partial class ChainMembership {
    public static readonly ChainMembership Add = new("Add", static (state, kind) => state.Added(kind));
    public static readonly ChainMembership Remove = new("Remove", static (state, kind) => state.Removed(kind));
    public Func<ChainState, Guid, Fin<ChainState>> Changed { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ChainEdit {
    public IO<ChainState> Committed(RhinoDoc doc) =>
        from prior in ChainState.Read(doc)
        from next in IO.lift(() => Applied(prior))
        from committed in when(next.State != prior, Commits.Commit(doc, $"{GetType().Name} {next.Target}", new RedrawPolicy.AllViews(Deferred: true),
            EffectList.Tune(new EffectSource.Document(doc), [EffectRequest.Tuning.Of(typeof(DarkroomEffect).GUID, next.State)])).Map(unit)).As()
        select next.State;

    private Fin<(ChainState State, string Target)> Applied(ChainState prior) =>
        Switch(prior,
            membership: static (state, edit) => Named(edit.Kind, edit.Change.Changed(state, edit.Kind)).Map(done => done with { Target = $"{edit.Change} {done.Target}" }),
            toggle: static (state, edit) => Named(edit.Kind, state.Toggled(edit.Kind, edit.On)),
            move: static (state, edit) => Named(edit.Kind, state.Moved(edit.Kind, edit.Before)),
            tune: static (state, edit) => Named(edit.Kind, state.Tuned(edit.Kind, edit.Values)),
            mix: static (state, edit) => Named(edit.Kind, state.Mixed(edit.Kind, edit.Value)),
            applyLook: static (state, edit) => Named(edit.Kind, state.Looked(edit.Kind, edit.Name)).Map(done => done with { Target = $"{edit.Name} {done.Target}" }),
            restore: static (_, edit) => Fin.Succ((edit.State, nameof(ChainState))));

    private static Fin<(ChainState State, string Target)> Named(Guid kind, Fin<ChainState> next) =>
        (next, ChainState.Kind(kind)).Apply(static (state, held) => (state, held.State.Name)).As();

    public sealed record Membership(Guid Kind, ChainMembership Change) : ChainEdit;

    public sealed record Toggle(Guid Kind, bool On) : ChainEdit;

    public sealed record Move(Guid Kind, Option<Guid> Before) : ChainEdit;

    public sealed record Tune(Guid Kind, ValueSet Values) : ChainEdit;

    public sealed record Mix(Guid Kind, float Value) : ChainEdit;

    public sealed record ApplyLook(Guid Kind, string Name) : ChainEdit;

    public sealed record Restore(ChainState State) : ChainEdit;
}
