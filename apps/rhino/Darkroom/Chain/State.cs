using System.Globalization;
using System.Xml;
using Darkroom.Catalog;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Pixels;
using Rasm.Rhino;
using Rasm.Rhino.Document;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.Render.Effects;
using Rasm.UI;
using Rhino;
using Rhino.Render.PostEffects;

namespace Darkroom.Chain;

// --- [TYPES] ---------------------------------------------------------------------------
public enum ChainFault {
    Unlisted,
    Absent,
    Staged,
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ChainEntry(Guid Kind, bool On);

[ObjectFactory<string>]
public sealed partial record ChainEntries(Seq<ChainEntry> Items) : IConvertible<string> {
    public const string Key = "entries";

    public static ParameterText Text { get; } = new("Entries", "Effect kinds of the chain in processing order, each on or off");

    public string ToValue() => string.Join(',', Items.Map(static entry => string.Create(CultureInfo.InvariantCulture, $"{entry.Kind:D}:{entry.On}")));

    public static ValidationError? Validate(string? value, IFormatProvider? provider, out ChainEntries? item) {
        item = (from text in Optional(value)
                from entries in toSeq(text.Split(',', StringSplitOptions.RemoveEmptyEntries)).Traverse(static entry => Parsed(entry)).As()
                where toHashSet(entries.Map(static entry => entry.Kind)).Count == entries.Count
                select new ChainEntries(entries)).ValueUnsafe();
        return value is not null && item is null ? new ValidationError($"chain entries '{value}' must list distinct '<kind id>:<on>' pairs") : null;
    }

    private static Option<ChainEntry> Parsed(string text) =>
        text.Split(':') is [var kind, var running]
            ? from id in parseGuid(kind) from flag in parseBool(running) select new ChainEntry(id, flag)
            : None;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record EffectLook<TRecord> where TRecord : IStateRecord<TRecord> {
    public ValueSet Captured(string owner) =>
        Switch(owner,
            whole: static (held, whole) => ValueSet.Fields(held, whole.Record),
            partial: static (held, partial) => ValueSet.Fields(held, partial.Record, partial.Keys));

    public sealed record Whole(TRecord Record) : EffectLook<TRecord>;

    public sealed record Partial(TRecord Record, Seq<IStateParameter<TRecord>> Keys) : EffectLook<TRecord>;
}

public sealed record EffectRow<TState> where TState : IStateRecord<TState> {
    public TState Default { get; init; } = TState.Default;

    public Option<Fraction> Mix { get; init; } = Some(Fraction.MaxValue);

    public Seq<(string Name, EffectLook<TState> Look)> Looks { get; init; }

    public bool Listed { get; init; }
}

public sealed record EffectKind(
    Guid Id, Type State, PostEffectType Stage, Family Family, string Caption, bool Listed, Option<Fraction> Mix, Seq<EntryText> Slots, ValueSet Defaults,
    Seq<(string Name, ValueSet Values)> Looks,
    Func<ValueSet, PassContext, IO<(Option<PixelPass> Pass, PassContext Context)>> Step,
    Func<ValueSet, IO<Seq<GuideChannel>>> Channels,
    Func<RhinoDoc, ValueBinding> Binding) {
    public static ValueKey<Fraction> MixKey { get; } =
        new("mix", StateParameter.Bounded(Lens.identity<Fraction>()), new("Mix", "Blends the kind's output over its input, 0% leaves the input unchanged"), Fraction.MaxValue);

    public string Owner => EntryKey.TypeOwner(Id);

    // --- [DECLARATION]
    public static EffectKind Of<TState>(Guid id, PostEffectType stage, Family family, EffectRow<TState> row) where TState : IPixelStage<TState> =>
        Declared(id, stage, family, row, TState.Pass, TState.Continued, TState.Channels);

    public static EffectKind Job<TState, TResource>(Guid id, PostEffectType stage, Family family, EffectRow<TState> row)
        where TState : IStateRecord<TState>
        where TResource : IFrameJob<TResource, TState> =>
        Declared(id, stage, family, row,
            static (state, context) => context.Rendering ? None : Some<PixelPass>(new PixelPass.Frame((frame, rows) =>
                context.Derivations.Held(unit, static _ => TResource.Open(), resource => resource.Run(state, frame, context.Guides, rows).Bind(result => Copied(result, frame))))),
            static (_, _, context) => context,
            TResource.Channels);

    private static EffectKind Declared<TState>(
        Guid id, PostEffectType stage, Family family, EffectRow<TState> row,
        Func<TState, PassContext, Option<PixelPass>> pass, Func<TState, Fraction, PassContext, PassContext> continued, Func<TState, Seq<GuideChannel>> channels)
        where TState : IStateRecord<TState> =>
        (Owner: EntryKey.TypeOwner(id), Fields: new FieldCodec<TState>(TState.Parameters, row.Default)) switch {
            var (owner, fields) => new(
                id, typeof(TState), stage, family, TState.Caption, row.Listed, row.Mix,
                fields.Texts + row.Mix.Map(static _ => new EntryText([MixKey.Key], MixKey.Text)).ToSeq(),
                row.Mix.Map(mix => MixCaptured(owner, mix)).IfNone(ValueSet.Empty).Over(ValueSet.Fields(owner, row.Default), []),
                row.Looks.Map(look => (look.Name, look.Look.Captured(owner))),
                (values, context) =>
                    from state in values.Recalled(owner, row.Default)
                    from mix in row.Mix.Match(Some: held => MixRecalled(owner, held, values), None: static () => IO.pure(Fraction.MaxValue))
                    let staged = pass(state, context).Bind(found => Blend.Mixed(found, BlendingMode.Mix, (float)mix, []))
                    select (staged, staged.IsSome ? continued(state, mix, context) : context),
                values => values.Recalled(owner, row.Default).Map(channels),
                doc => ValueBinding.Of(owner, fields, ValueStore.Defaulted(
                    ChainState.Read(doc).Bind(chain => chain.Values.Recalled(owner, row.Default)),
                    IO.pure(row.Default),
                    state => new ChainEdit.Tune(id, ValueSet.Fields(owner, state)).Committed(doc).Map(unit),
                    EqualityComparer<TState>.Default.Equals, Applied.Live, Some(new EffectSource.Document(doc).Changed)))),
        };

    // --- [MIX]
    public static ValueSet MixCaptured(string owner, Fraction mix) => ValueSet.Fields<Fraction>(owner, mix, [MixKey]);

    public static IO<Fraction> MixRecalled(string owner, Fraction mix, ValueSet values) => (MixKey with { Default = mix }).Codec.Recalled(owner, values.Texts(owner));

    private static Fin<Unit> Copied(PixelFrame result, PixelFrame frame) {
        if (result.Size != frame.Size)
            return new Refused(nameof(IFrameJob<,>.Run), Refused.Invalid);
        result.View.Span.CopyTo(frame.View.Span);
        return unit;
    }
}

public sealed record ChainState(Seq<ChainEntry> Entries, ValueSet Values) : IStateRecord<ChainState> {
    private static readonly IStateParameter<ChainState> Listing = new ChainParameter(
        ChainEntries.Key,
        new StateParameter<ChainState>.Keyed<ChainEntries, string>(Lens<ChainState, ChainEntries>.New(
            static state => new(state.Entries), static entries => state => state with { Entries = entries.Items })),
        ChainEntries.Text);

    private static readonly Lazy<Declaration> Declared = new(static () => Declaration.Of(Kinds.Ordered));

    // --- [CATALOG]
    public static ChainState Default => Declared.Value.Default;

    public static Seq<IStateParameter<ChainState>> Parameters => Declared.Value.Parameters;

    public static Option<IStateParameter<ChainState>> FindParameter(string key) => Declared.Value.Named.Find(key);

    public static string Owner => EntryKey.TypeOwner(typeof(DarkroomEffect).GUID);

    public static Fin<EffectKind> Kind(Guid id) =>
        Declared.Value.Kinds.Find(id).Map(static held => held.Kind).ToFin(new ChainRefused(id, ChainFault.Unlisted));

    // --- [CHAIN]
    public Seq<EffectKind> Running =>
        toSeq(Entries.Filter(static entry => entry.On).Choose(static entry => Kind(entry.Kind).ToOption()).OrderBy(static kind => kind.Stage));

    public IO<Seq<GuideChannel>> Needs =>
        Running.TraverseM(kind => kind.Channels(Values)).As().Map(static needs => needs.Flatten().Distinct());

    public IO<(Seq<(EffectKind Kind, PixelPass Pass)> Passes, PassContext Context)> Passes(PassContext context) =>
        Running.Fold(IO.pure((Passes: Seq<(EffectKind Kind, PixelPass Pass)>(), Context: context)), (previous, kind) =>
            previous.Bind(held => kind.Step(Values, held.Context).Map(staged =>
                (Passes: held.Passes + staged.Pass.Map(pass => (Kind: kind, Pass: pass)).ToSeq(), staged.Context))));

    public IO<Seq<PixelPass.Color>> Baked(PassContext context) =>
        Passes(context).Map(static chain => chain.Passes
            .Filter(step => chain.Context.Formed || step.Kind.Stage is PostEffectType.Early)
            .Choose(static step => step.Pass is PixelPass.Color color ? Some(color) : None));

    // --- [TRANSITIONS]
    public Fin<ChainState> Added(Guid kind) =>
        Placed(kind).Map(place => Entries.Exists(entry => entry.Kind == kind) ? this
            : this with { Entries = Inserted(Entries, new ChainEntry(kind, On: true), entry => Placed(entry.Kind).Exists(other => other.CompareTo(place) > 0)) });

    public Fin<ChainState> Removed(Guid kind) =>
        Present(kind).Map(_ => this with { Entries = Entries.Filter(entry => entry.Kind != kind) });

    public Fin<ChainState> Toggled(Guid kind, bool on) =>
        Present(kind).Map(_ => this with { Entries = Entries.Map(entry => entry.Kind == kind ? entry with { On = on } : entry) });

    public Fin<ChainState> Moved(Guid kind, Option<Guid> before) =>
        from entry in Present(kind)
        from place in Placed(kind)
        from target in before.Traverse(other => Present(other).Bind(_ => Placed(other)).Bind(at =>
            at.Stage == place.Stage ? Fin.Succ(other) : new ChainRefused(other, ChainFault.Staged))).As()
        select this with {
            Entries = Inserted(Entries.Filter(held => held.Kind != kind), entry, target.Match<Func<ChainEntry, bool>>(
                other => held => held.Kind == other,
                () => held => Placed(held.Kind).Exists(at => at.Stage > place.Stage))),
        };

    public Fin<ChainState> Tuned(Guid kind, ValueSet values) =>
        Kind(kind).Map(held => this with {
            Values = new(Values.Entries.Filter((key, _) => !Fielded(held, key)).AddOrUpdateRange(values.Entries.Filter((key, _) => Fielded(held, key)).AsIterable())),
        });

    public Fin<ChainState> Mixed(Guid kind, Fraction mix) =>
        Kind(kind).Map(held => this with { Values = EffectKind.MixCaptured(held.Owner, mix).Over(Values, []) });

    public Fin<ChainState> Looked(Guid kind, string name) =>
        from held in Kind(kind)
        from look in held.Looks.Find(kept => string.Equals(kept.Name, name, StringComparison.Ordinal)).ToFin(new NotFound("look", name))
        from added in Added(kind)
        select added with { Values = look.Values.Over(added.Values, []) };

    private Fin<ChainEntry> Present(Guid kind) =>
        Entries.Find(entry => entry.Kind == kind).ToFin(new ChainRefused(kind, ChainFault.Absent));

    private static Fin<(PostEffectType Stage, int Index)> Placed(Guid kind) =>
        Declared.Value.Kinds.Find(kind).Map(static held => (held.Kind.Stage, held.Index)).ToFin(new ChainRefused(kind, ChainFault.Unlisted));

    private static Seq<ChainEntry> Inserted(Seq<ChainEntry> entries, ChainEntry entry, Func<ChainEntry, bool> after) =>
        entries.TakeWhile(held => !after(held)) + entry.Cons(entries.SkipWhile(held => !after(held)));

    private static bool Fielded(EffectKind kind, EntryKey key) =>
        string.Equals(key.Owner, kind.Owner, StringComparison.Ordinal) && !key.Path.Equals(Seq(EffectKind.MixKey.Key));

    // --- [DOCUMENT]
    public static IO<ChainState> Read(RhinoDoc doc) => EffectCollection.State<ChainState>(new EffectSource.Document(doc), typeof(DarkroomEffect).GUID);

    public static BindingGroup Bindings(RhinoDoc doc) =>
        new(Kinds.Ordered.Bind(kind => kind.Binding(doc).Cons(kind.Mix.Map(mix => Blended(doc, kind, mix)).ToSeq()))
            .Add(ValueBinding.Of(Owner, ValueKey.Keyed<ChainEntries, string>(ChainEntries.Key, ChainEntries.Text, new(Default.Entries)).Codec, ValueStore.Defaulted(
                Read(doc).Map(static state => new ChainEntries(state.Entries)),
                IO.pure(new ChainEntries(Default.Entries)),
                entries => Read(doc).Bind(state => new ChainEdit.Restore(state with { Entries = entries.Items }).Committed(doc)).Map(unit),
                EqualityComparer<ChainEntries>.Default.Equals, Applied.Live, Some(new EffectSource.Document(doc).Changed)))));

    private static ValueBinding Blended(RhinoDoc doc, EffectKind kind, Fraction mix) =>
        ValueBinding.Of(kind.Owner, (EffectKind.MixKey with { Default = mix }).Codec, ValueStore.Defaulted(
            Read(doc).Bind(state => EffectKind.MixRecalled(kind.Owner, mix, state.Values)),
            IO.pure(mix),
            value => new ChainEdit.Mix(kind.Id, value).Committed(doc).Map(unit),
            EqualityComparer<Fraction>.Default.Equals, Applied.Live, Some(new EffectSource.Document(doc).Changed)));

    // --- [PARAMETERS]
    private static IStateParameter<ChainState> Slot(EffectKind kind, EntryText slot) =>
        new EntryKey(kind.Owner, slot.Path) switch {
            var entry => new ChainParameter(
                XmlConvert.EncodeLocalName(string.Create(CultureInfo.InvariantCulture, $"k{kind.Id:N}.{EntryKey.Name(slot.Path)}")),
                new StateParameter<ChainState>.OptionalRaw<string>(Lens<ChainState, Option<string>>.New(
                    state => state.Values.Entries.Find(entry),
                    text => state => state with {
                        Values = new(text.Match(Some: held => state.Values.Entries.AddOrUpdate(entry, held), None: () => state.Values.Entries.Remove(entry))),
                    })),
                slot.Text),
        };

    private sealed record ChainParameter(string Key, StateParameter<ChainState> Kind, ParameterText Text) : IStateParameter<ChainState>;

    private sealed record Declaration(
        HashMap<Guid, (EffectKind Kind, int Index)> Kinds, ChainState Default, Seq<IStateParameter<ChainState>> Parameters, HashMap<string, IStateParameter<ChainState>> Named) {
        public static Declaration Of(Seq<EffectKind> catalog) =>
            Listing.Cons(catalog.Bind(static kind => kind.Slots.Map(slot => Slot(kind, slot)))) switch {
                var parameters => new(
                    toHashMap(catalog.Map(static (kind, index) => (kind.Id, (kind, index)))),
                    new ChainState(
                        catalog.Filter(static kind => kind.Listed).Map(static kind => new ChainEntry(kind.Id, On: true)),
                        new ValueSet(toHashMap(catalog.Bind(static kind => toSeq(kind.Defaults.Entries.AsIterable()))))),
                    parameters,
                    toHashMap(parameters.Map(static parameter => (parameter.Key, parameter)))),
            };
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ChainEdit {
    public IO<ChainState> Committed(RhinoDoc doc) =>
        from prior in ChainState.Read(doc)
        from next in IO.lift(() => Applied(prior))
        let tunings = Changed(prior, next.State)
        from committed in when(!tunings.IsEmpty, Commits.Commit(doc, $"{Captions.Of(GetType())} {next.Target}", new RedrawPolicy.AllViews(Deferred: true),
            EffectCollection.Tune(new EffectSource.Document(doc), tunings)).Map(unit)).As()
        select next.State;

    private Fin<(ChainState State, string Target)> Applied(ChainState prior) =>
        Switch(prior,
            add: static (state, edit) => Named(edit.Kind, state.Added(edit.Kind)),
            remove: static (state, edit) => Named(edit.Kind, state.Removed(edit.Kind)),
            toggle: static (state, edit) => Named(edit.Kind, state.Toggled(edit.Kind, edit.On)),
            move: static (state, edit) => Named(edit.Kind, state.Moved(edit.Kind, edit.Before)),
            tune: static (state, edit) => Named(edit.Kind, state.Tuned(edit.Kind, edit.Values)),
            mix: static (state, edit) => Named(edit.Kind, state.Mixed(edit.Kind, edit.Value)),
            applyLook: static (state, edit) => Named(edit.Kind, state.Looked(edit.Kind, edit.Name)).Map(done => done with { Target = $"{edit.Name} {done.Target}" }),
            restore: static (_, edit) => Fin.Succ((edit.State, Captions.Of(typeof(ChainState)))));

    private static Fin<(ChainState State, string Target)> Named(Guid kind, Fin<ChainState> next) =>
        from state in next
        from held in ChainState.Kind(kind)
        select (state, held.Caption);

    private static Seq<EffectRequest.Tuning> Changed(ChainState prior, ChainState next) =>
        toHashSet(EffectRequest.Tuning.Of(typeof(DarkroomEffect).GUID, prior).Map(static tuning => (tuning.Key, tuning.Text))) switch {
            var held => EffectRequest.Tuning.Of(typeof(DarkroomEffect).GUID, next).Filter(tuning => !held.Contains((tuning.Key, tuning.Text))),
        };

    public sealed record Add(Guid Kind) : ChainEdit;

    public sealed record Remove(Guid Kind) : ChainEdit;

    public sealed record Toggle(Guid Kind, bool On) : ChainEdit;

    public sealed record Move(Guid Kind, Option<Guid> Before) : ChainEdit;

    public sealed record Tune(Guid Kind, ValueSet Values) : ChainEdit;

    public sealed record Mix(Guid Kind, Fraction Value) : ChainEdit;

    public sealed record ApplyLook(Guid Kind, string Name) : ChainEdit;

    public sealed record Restore(ChainState State) : ChainEdit;
}

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record ChainRefused(Guid Kind, ChainFault Fault) : Expected($"effect kind {Kind} is {Fault}", 0);
