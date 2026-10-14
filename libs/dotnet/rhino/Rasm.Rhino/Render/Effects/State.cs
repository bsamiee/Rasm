using Rasm.Imaging.Grade;
using Rasm.Imaging.Pixels;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.Render.Settings;
using Rasm.Rhino.UI;
using Rasm.Rhino.UI.Rows;
using Rhino;
using Rhino.Render.PostEffects;
using UnitsNet.Units;

namespace Rasm.Rhino.Render.Effects;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record EffectRow<TState, TParameter>(Func<TParameter, ParameterText> Text)
    where TState : IStateRecord<TState>
    where TParameter : class, IStateParameter<TState> {
    public TState Default { get; init; } = TState.Default;

    public Option<Mix> Amount { get; init; } = Some(EffectStates.Amount.Default);

    public Option<HelpTopic> Help { get; init; }

    public Seq<(string Name, EffectLook<TState, TParameter> Look)> Looks { get; init; }
}

public sealed record EffectValue<TState>(TState Record, Option<Mix> Amount);

[Union]
public abstract partial record EffectLook<TRecord, TParameter>
    where TRecord : IStateRecord<TRecord>
    where TParameter : class, IStateParameter<TRecord> {
    public ValueSet Capture(string owner) =>
        Switch(owner,
            whole: static (held, whole) => ValueSet.Fields(held, whole.Record, toSeq(TRecord.Parameters)),
            partial: static (held, partial) => ValueSet.Fields(held, partial.Record, partial.Keys.Map<IStateParameter<TRecord>>(static key => key)));

    public sealed record Whole(TRecord Record) : EffectLook<TRecord, TParameter>;

    public sealed record Partial(TRecord Record, Seq<TParameter> Keys) : EffectLook<TRecord, TParameter>;
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class EffectState<TState>(Guid effect, TState factory, Option<Mix> amount) where TState : IStateRecord<TState> {
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
                    from mix in IO.lift(new EntryKey(Owner, EffectStates.AmountPath).Read(text, EffectStates.Amount.Read))
                    from swapped in IO.lift(() => atomic(() => held.Swap(value => value with { Amount = Some(mix) })))
                    select unit)).Map(change));

    internal IO<Change<TState>> Write(TState target, Func<Seq<(string Key, string Text)>, Fin<Unit>> native) =>
        EffectStates.Captured(target) switch {
            var entries => Writing(
                entries.Map(static entry => entry.Key),
                key => Fields.Find(key).Map<Func<EffectValue<TState>, EffectValue<TState>>>(field => value => value with { Record = field.Taken(target, value.Record) }),
                value => Some(value.Record).Filter(record => !EqualityComparer<TState>.Default.Equals(record, factory)),
                () => native(entries)),
        };

    internal IO<Change<Mix>> Write(Mix target, Func<string, Fin<Unit>> native) =>
        Writing(
            [EffectStates.Amount.Name],
            _ => Some<Func<EffectValue<TState>, EffectValue<TState>>>(value => value with { Amount = Some(target) }),
            value => value.Amount.Filter(mix => !amount.Exists(initial => initial == mix)),
            () => native(EffectStates.Amount.Text(target)));

    private IO<Change<A>> Writing<A>(
        Seq<string> keys,
        Func<string, Option<Func<EffectValue<TState>, EffectValue<TState>>>> prepare,
        Func<EffectValue<TState>, Option<A>> project, Func<Fin<Unit>> native) =>
        IO.lift(() => {
            EffectValue<TState> before = held.Value;
            Change<A> change = ValueStore.Between(project(before), project(keys.Choose(prepare).Fold(before, static (value, update) => update(value))));
            Func<string, Option<IO<Unit>>> apply = key => prepare(key).Map(update =>
                IO.lift(() => atomic(() => held.Swap(update))).Map(static _ => unit));
            return DisposalOps.Within(
                () => {
                    Option<Func<string, Option<IO<Unit>>>> prior = writing.Value;
                    writing.Value = Some(apply);
                    return prior;
                },
                _ => IO.lift(native).Map(_ => change),
                prior => IO.lift(() => { writing.Value = prior; }));
        }).Bind(static next => next);

    private Option<Mix> Mixed(string key) => held.Value.Amount.Filter(_ => string.Equals(key, EffectStates.Amount.Name, StringComparison.Ordinal));

    // --- [PERSISTENCE]
    public IO<Unit> ReadState(PostEffectState store) =>
        from read in EffectStates.Folded(Owner, path => IO.lift(() => Stored(store, EntryKey.Name(path))), factory, amount)
        from _ in IO.lift(() => atomic(() => held.Swap(_ => read.Value)))
        from __ in unless(read.Refused.IsEmpty, IO.fail<Unit>(Error.Many(read.Refused))).As()
        select unit;

    public IO<Unit> WriteState(PostEffectState store) =>
        held.ValueIO.Bind(value => IO.lift(() => Callbacks.Each(
            EffectStates.Captured(value.Record) + value.Amount.Map(static mix => (Key: EffectStates.Amount.Name, Text: EffectStates.Amount.Text(mix))).ToSeq(),
            entry => store.SetValue(entry.Key, entry.Text),
            nameof(PostEffectState.SetValue))));

    public IO<Unit> ResetToFactoryDefaults() =>
        IO.lift(() => atomic(() => held.Swap(_ => Default))).Map(static _ => unit);

    private static Option<string> Stored(PostEffectState store, string key) => Callbacks.Found(store.TryGetValue(key, out string text), text);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class EffectStates {
    // --- [AMOUNT]
    public static readonly ValueKey<Mix, float> Amount = AmountKey(Mix.Full);

    internal static readonly Seq<string> AmountPath = [Amount.Name];

    public static ValueKey<Mix, float> AmountKey(Mix factory) =>
        ValueKey.Numeric<Mix, float>("mix", "Mix", "Blends the effect's result over its input, the input alone at 0", factory,
            Conversions.Validated<Mix, float, InvalidGrade>, static value => value, (Mix.MinValue, Mix.MaxValue),
            new() { Origin = Some<float>(factory), Unit = UnitsNet.Quantity.GetUnitInfo(RatioUnit.DecimalFraction) });

    // --- [KEYS]
    public static Seq<(string Key, string Text)> Captured<TState>(TState record) where TState : IStateRecord<TState> =>
        FieldTexts.Items<TState>().Choose(field => field.Capture(record).Map(text => (Key: EntryKey.Name(field.Path), Text: text)));

    // --- [RECALL]
    public static IO<EffectValue<TState>> Recalled<TState>(string owner, ValueSet set, Option<Mix> amount, TState factory) where TState : IStateRecord<TState> =>
        Folded(owner, path => IO.pure(set.Entries.Find(new EntryKey(owner, path))), factory, amount).Bind(static read => read.Refused.IsEmpty
            ? IO.pure(read.Value)
            : IO.fail<EffectValue<TState>>(Error.Many(read.Refused)));

    internal static IO<(EffectValue<TState> Value, Seq<Error> Refused)> Folded<TState>(
        string owner, Func<Seq<string>, IO<Option<string>>> stored, TState factory, Option<Mix> amount) where TState : IStateRecord<TState> =>
        from parts in FieldTexts.Folded(owner, stored, factory)
        from mixes in amount.ToSeq().Map<K<IO, Mix>>(fallback =>
                from entry in stored(AmountPath)
                from mix in IO.lift(() => entry.Traverse(text => new EntryKey(owner, AmountPath).Read(text, Amount.Read)).As())
                select mix.IfNone(fallback))
            .PartitionFallible().As()
        select (new EffectValue<TState>(parts.Record, mixes.Succs.Head | amount), parts.Refused + mixes.Fails);

    // --- [BINDING]
    public static ValueBinding Binding<TEffect, TState, TParameter, TError>(RhinoDoc doc, EffectRow<TState, TParameter> row)
        where TEffect : PostEffect
        where TState : IStateRecord<TState, TParameter, TError>
        where TParameter : class, IStateParameter<TState>, ISmartEnum<string, TParameter, TError>
        where TError : Error, IValidationError<TError> =>
        EntryKey.TypeOwner(typeof(TEffect).GUID) switch {
            var owner => Joined(
                ValueBinding.Fields(
                    owner,
                    ValueStore.Of(
                        RenderSettingsSources.Read(doc, window => EffectRegistry.State<TEffect, TState>(window.Settings, Some(row.Default)))
                            .Map(record => Some(record).Filter(held => !EqualityComparer<TState>.Default.Equals(held, row.Default))),
                        record => RenderSettingsSources.Edit(doc, window => EffectRegistry.Apply(window.Settings, EffectRequest.Tuning.Of<TEffect, TState>(record.IfNone(row.Default)))).Map(static _ => unit),
                        Applied.Live,
                        None),
                    row.Default,
                    ParameterText.Join<TState, TParameter, TError>(row.Text).Map<(IStateParameter<TState> Parameter, string Caption)>(static pair => (pair.Parameter, pair.Text.Caption))),
                row.Amount.Map(factory => ValueBinding.Key(owner, AmountKey(factory), ValueStore.Of(
                    RenderSettingsSources.Read(doc, static window => EffectRegistry.Amount<TEffect>(window.Settings)).Map(found => found.Filter(mix => mix != factory)),
                    mix => RenderSettingsSources.Edit(doc, window => EffectRegistry.Apply(window.Settings, [EffectRequest.Tuning.Amount<TEffect>(mix.IfNone(factory))])).Map(static _ => unit),
                    Applied.Live,
                    None)))),
        };

    private static ValueBinding Joined(ValueBinding record, Option<ValueBinding> amount) =>
        amount.Match(
            Some: mix => new ValueBinding(
                record.Owner,
                record.Texts + mix.Texts,
                () => record.Defaults() + mix.Defaults(),
                (record.Capture, mix.Capture).Apply(static (fields, held) => fields + held).As(),
                entries =>
                    from prepared in Seq<K<IO, IO<ValueDiff>>>(record.Prepare(entries), mix.Prepare(entries)).PartitionFallible().As()
                    from accepted in unless(prepared.Fails.IsEmpty, IO.fail<Unit>(Error.Many(prepared.Fails))).As()
                    select Callbacks.Each(prepared.Succs).Map(ValueDiff.Together)),
            None: () => record);
}
