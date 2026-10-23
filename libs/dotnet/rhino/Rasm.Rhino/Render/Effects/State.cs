using Rasm.Imaging.Pixels;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.Render.Settings;
using Rasm.Rhino.UI;
using Rhino;
using Rhino.Render.PostEffects;

namespace Rasm.Rhino.Render.Effects;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record EffectRow<TState, TParameter>
    where TState : IStateRecord<TState>
    where TParameter : class, IStateParameter<TState> {
    public TState Default { get; init; } = TState.Default;

    public Option<AxisFraction> Amount { get; init; } = Some(EffectStates.Amount.Default);

    public Option<HelpTopic> Help { get; init; }

    public Seq<(string Name, EffectLook<TState, TParameter> Look)> Looks { get; init; }
}

public sealed record EffectValue<TState>(TState Record, Option<AxisFraction> Amount);

[Union]
public abstract partial record EffectLook<TRecord, TParameter>
    where TRecord : IStateRecord<TRecord>
    where TParameter : class, IStateParameter<TRecord> {
    public ValueSet Capture(string owner) =>
        Switch(owner,
            whole: static (held, whole) => ValueSet.Fields(held, whole.Record),
            partial: static (held, partial) => ValueSet.Fields(held, partial.Record, partial.Keys.Map<IStateParameter<TRecord>>(static key => key)));

    public sealed record Whole(TRecord Record) : EffectLook<TRecord, TParameter>;

    public sealed record Partial(TRecord Record, Seq<TParameter> Keys) : EffectLook<TRecord, TParameter>;
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class EffectState<TState>(Guid effect, TState factory, Option<AxisFraction> amount) where TState : IStateRecord<TState> {
    // --- [STATE]
    private static readonly HashMap<string, FieldText<TState>> Fields =
        toHashMap(FieldTexts.Items<TState>().Map(static field => (EntryKey.Name(field.Path), field)));

    private readonly Ref<EffectValue<TState>> held = Ref(new EffectValue<TState>(factory, amount));

    private readonly AsyncLocal<Option<Func<string, Option<IO<Unit>>>>> writing = new();

    public string Owner => EntryKey.TypeOwner(effect);

    public EffectValue<TState> Default => new(factory, amount);

    public EffectValue<TState> Value => held.Value;

    public TState Current => held.Value.Record;

    // --- [PARAMETERS]
    public Option<string> GetParam(string key) =>
        Fields.Find(key).Bind(field => field.Capture(held.Value.Record)) | Mixed(key).Map(EffectStates.Amount.Text);

    public Option<IO<Unit>> SetParam(string key, string text, Func<IO<Unit>, IO<Unit>> change) =>
        writing.Value.Match(
            Some: prepared => prepared(key).Map(change),
            None: () => (Fields.Find(key).Map(field =>
                    from step in IO.lift(new EntryKey(Owner, field.Path).Read(text, field.Recall))
                    from set in step
                    from swapped in IO.lift(() => atomic(() => held.Swap(value => value with { Record = set(value.Record) })))
                    select unit)
                | Mixed(key).Map(_ =>
                    from mix in IO.lift(EffectStates.AmountOf(Owner, text))
                    from swapped in IO.lift(() => atomic(() => held.Swap(value => value with { Amount = Some(mix) })))
                    select unit)).Map(change));

    internal IO<Change<TState>> Write(TState target, Func<string, string, Fin<Unit>> set) =>
        EffectStates.Captured(target) switch {
            var entries => Writing(
                entries.Map(static entry => entry.Key),
                key => Fields.Find(key).Map<Func<EffectValue<TState>, EffectValue<TState>>>(field => value => value with { Record = field.Taken(target, value.Record) }),
                value => Some(value.Record).Filter(record => !EqualityComparer<TState>.Default.Equals(record, factory)),
                () => Callbacks.Each(entries, (entry, _) => set(entry.Key, entry.Text)).Map(static _ => unit)),
        };

    internal IO<Change<AxisFraction>> Write(AxisFraction target, Func<string, string, Fin<Unit>> set) =>
        Writing(
            [EffectStates.Amount.Name],
            _ => Some<Func<EffectValue<TState>, EffectValue<TState>>>(value => value with { Amount = Some(target) }),
            value => value.Amount.Filter(mix => !amount.Exists(initial => initial == mix)),
            () => set(EffectStates.Amount.Name, EffectStates.Amount.Text(target)));

    private IO<Change<A>> Writing<A>(
        Seq<string> keys,
        Func<string, Option<Func<EffectValue<TState>, EffectValue<TState>>>> prepare,
        Func<EffectValue<TState>, Option<A>> project, Func<Fin<Unit>> native) =>
        IO.lift(() => {
            EffectValue<TState> before = held.Value;
            Change<A> change = ValueStore.Between(project(before), project(keys.Choose(prepare).Fold(before, static (value, update) => update(value))));
            Func<string, Option<IO<Unit>>> apply = key => prepare(key).Map(update =>
                IO.lift(() => atomic(() => held.Swap(update))).Map(unit));
            return DisposalOps.Within(
                () => {
                    Option<Func<string, Option<IO<Unit>>>> prior = writing.Value;
                    writing.Value = Some(apply);
                    return prior;
                },
                _ => IO.lift(native).Map(change),
                prior => IO.lift(() => { writing.Value = prior; }));
        }).Bind(static next => next);

    private Option<AxisFraction> Mixed(string key) => held.Value.Amount.Filter(_ => string.Equals(key, EffectStates.Amount.Name, StringComparison.Ordinal));

    // --- [PERSISTENCE]
    public IO<Unit> ReadState(PostEffectState store) =>
        from read in EffectStates.Folded(Owner, path => IO.lift(() => Callbacks.Found(store.TryGetValue(EntryKey.Name(path), out string text), text)), factory, amount)
        from _ in IO.lift(() => atomic(() => held.Swap(_ => read.Value)))
        from __ in unless(read.Refused.IsEmpty, IO.fail<Unit>(Error.Many(read.Refused))).As()
        select unit;

    public IO<Unit> WriteState(PostEffectState store) =>
        held.ValueIO.Bind(value => IO.lift(() => Callbacks.Each(
            EffectStates.Captured(value.Record) + value.Amount.Map(static mix => (Key: EffectStates.Amount.Name, Text: EffectStates.Amount.Text(mix))).ToSeq(),
            entry => store.SetValue(entry.Key, entry.Text),
            nameof(PostEffectState.SetValue))));

    public IO<Unit> ResetToFactoryDefaults() =>
        IO.lift(() => atomic(() => held.Swap(_ => Default))).Map(unit);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class EffectStates {
    // --- [AMOUNT]
    public static readonly ValueKey<AxisFraction, float> Amount = AmountKey(AxisFraction.MaxValue);

    internal static readonly Seq<string> AmountPath = [Amount.Name];

    public static ValueKey<AxisFraction, float> AmountKey(AxisFraction factory) =>
        ValueKey.Numeric("mix", "Mix", "Blends effect output over input, 0% leaves input unchanged", factory,
            NumericDomain.Bounded<AxisFraction, float>(AxisFraction.Presentation with { Origin = Some<float>(factory) }));

    internal static Fin<AxisFraction> AmountOf(string owner, string text) => new EntryKey(owner, AmountPath).Read(text, Amount.Read);

    // --- [KEYS]
    public static Seq<(string Key, string Text)> Captured<TState>(TState record) where TState : IStateRecord<TState> =>
        FieldTexts.Items<TState>().Choose(field => field.Capture(record).Map(text => (Key: EntryKey.Name(field.Path), Text: text)));

    // --- [RECALL]
    public static IO<EffectValue<TState>> Recalled<TState>(string owner, ValueSet set, Option<AxisFraction> amount, TState factory) where TState : IStateRecord<TState> =>
        Folded(owner, set.Texts(owner), factory, amount).Bind(static read => read.Refused.IsEmpty
            ? IO.pure(read.Value)
            : IO.fail<EffectValue<TState>>(Error.Many(read.Refused)));

    internal static IO<(EffectValue<TState> Value, Seq<Error> Refused)> Folded<TState>(
        string owner, Func<Seq<string>, IO<Option<string>>> stored, TState factory, Option<AxisFraction> amount) where TState : IStateRecord<TState> =>
        from parts in FieldTexts.Folded(owner, stored, factory)
        from mixes in amount.ToSeq().Map<K<IO, AxisFraction>>(fallback =>
                from entry in stored(AmountPath)
                from mix in IO.lift(() => entry.Traverse(text => AmountOf(owner, text)).As())
                select mix.IfNone(fallback))
            .PartitionFallible().As()
        select (new EffectValue<TState>(parts.Record, mixes.Succs.Head | amount), parts.Refused + mixes.Fails);

    // --- [BINDING]
    public static Seq<ValueBinding> Binding<TEffect, TState, TParameter>(RhinoDoc doc, EffectRow<TState, TParameter> row)
        where TEffect : PostEffect
        where TState : IStateRecord<TState, TParameter>
        where TParameter : class, IStateParameter<TState>, ISmartEnum<string, TParameter, ValidationError> =>
        ValueBinding.Fields(
                EntryKey.TypeOwner(typeof(TEffect).GUID),
                ValueStore.Of(
                    RenderSettingsSources.Read(doc, scope => EffectRegistry.State<TEffect, TState>(scope.Settings, Some(row.Default)))
                        .Map(record => Some(record).Filter(held => !EqualityComparer<TState>.Default.Equals(held, row.Default))),
                    record => RenderSettingsSources.Edit(doc, scope => EffectRegistry.Apply(scope.Settings, EffectRequest.Tuning.Of<TEffect, TState>(record.IfNone(row.Default)))).Map(unit),
                    Applied.Live,
                    None),
                row.Default,
                TState.Parameters)
            .Cons(row.Amount.Map(factory => ValueBinding.Key(EntryKey.TypeOwner(typeof(TEffect).GUID), AmountKey(factory), ValueStore.Of(
                RenderSettingsSources.Read(doc, static scope => EffectRegistry.Amount<TEffect>(scope.Settings)).Map(found => found.Filter(mix => mix != factory)),
                mix => RenderSettingsSources.Edit(doc, scope => EffectRegistry.Apply(scope.Settings, [EffectRequest.Tuning.Amount<TEffect>(mix.IfNone(factory))])).Map(unit),
                Applied.Live,
                None))).ToSeq());
}
