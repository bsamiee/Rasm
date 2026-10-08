using Rasm.Imaging.Grade;
using Rasm.Imaging.Pixels;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.Render.Scenes;
using Rhino;
using Rhino.Render.PostEffects;

namespace Rasm.Rhino.Render.Effects;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ParameterText(
    string Caption, string Help, HashMap<string, (string Caption, HashMap<string, ParameterText> Parameters)> Items, Option<string> Absent) {
    public HashMap<string, ParameterText> Parameters { get; init; }

    public static ParameterText Of(string caption, string help) => new(caption, help, [], None);

    public static ParameterText Choice<TValue, TError>(string caption, string help, Func<TValue, string> items)
        where TValue : ISmartEnum<string, TValue, TError>
        where TError : Error, IValidationError<TError> =>
        Variant<TValue, TError>(caption, help, item => (items(item), HashMap<string, ParameterText>()));

    public static ParameterText Choice<TValue, TError>(string caption, string help, string absent, Func<TValue, string> items)
        where TValue : ISmartEnum<string, TValue, TError>
        where TError : Error, IValidationError<TError> =>
        Choice<TValue, TError>(caption, help, items) with { Absent = absent };

    public static ParameterText Variant<TCase, TError>(
        string caption, string help, Func<TCase, (string Caption, HashMap<string, ParameterText> Parameters)> items)
        where TCase : ISmartEnum<string, TCase, TError>
        where TError : Error, IValidationError<TError> =>
        new(caption, help, toHashMap(toSeq(TCase.Items).Map(item => (item.ToValue(), items(item)))), None);

    public static ParameterText Enumeration<TEnum>(string caption, string help, Func<TEnum, string> members) where TEnum : struct, Enum =>
        new(caption, help, toHashMap(toSeq(Enum.GetValues<TEnum>()).Map(member => (StoredText.Format(member), (members(member), HashMap<string, ParameterText>())))), None);

    public static HashMap<string, ParameterText> Fields<TState, TParameter, TError>(Func<TParameter, ParameterText> text)
        where TState : IStateRecord<TState, TParameter, TError>
        where TParameter : class, IStateParameter<TState>, ISmartEnum<string, TParameter, TError>
        where TError : Error, IValidationError<TError> =>
        toHashMap(Join<TState, TParameter, TError>(text).Map(static pair => (pair.Parameter.Key, pair.Text)));

    public static Seq<(TParameter Parameter, ParameterText Text)> Join<TState, TParameter, TError>(Func<TParameter, ParameterText> text)
        where TState : IStateRecord<TState, TParameter, TError>
        where TParameter : class, IStateParameter<TState>, ISmartEnum<string, TParameter, TError>
        where TError : Error, IValidationError<TError> =>
        toSeq(TParameter.Items).Map(parameter => (Parameter: parameter, Text: text(parameter))).Strict();
}

public sealed record EffectValue<TState>(TState Record, Option<Mix> Amount);

[Union]
public abstract partial record EffectLook<TState, TParameter> where TParameter : class, IStateParameter<TState> {
    public sealed record Whole(TState Record) : EffectLook<TState, TParameter>;

    public sealed record Partial(TState Record, Seq<TParameter> Keys) : EffectLook<TState, TParameter>;
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class EffectState<TState>(Guid effect, Option<Mix> amount) where TState : IStateRecord<TState> {
    // --- [STATE]
    private static readonly HashMap<string, FieldText<TState>> Fields =
        toHashMap(FieldTexts<TState>.Items.Map(static field => (EffectCollection.ParameterName(field.Path), field)));

    private readonly Atom<EffectValue<TState>> held = Atom(new EffectValue<TState>(TState.Default, amount));

    public string Owner => EntryKey.TypeOwner(effect);

    public EffectValue<TState> Value => held.Value;

    public TState Current => held.Value.Record;

    // --- [PARAMETERS]
    public Option<string> GetParam(string key) =>
        Fields.Find(key).Bind(field => field.Capture(held.Value.Record)) | Mixed(key).Map(EffectStates.Amount.Text);

    public Option<IO<Unit>> SetParam(string key, string text) =>
        Fields.Find(key).Map(field =>
            from step in IO.lift(new EntryKey(Owner, field.Path).Read(text, field.Recall))
            from set in step
            from _ in held.SwapIO(value => value with { Record = set(value.Record) })
            select unit)
        | Mixed(key).Map(_ =>
            from mix in IO.lift(new EntryKey(Owner, EffectStates.AmountPath).Read(text, EffectStates.Amount.Read))
            from _ in held.SwapIO(value => value with { Amount = Some(mix) })
            select unit);

    private Option<Mix> Mixed(string key) => held.Value.Amount.Filter(_ => string.Equals(key, EffectStates.Amount.Name, StringComparison.Ordinal));

    // --- [PERSISTENCE]
    public IO<Unit> ReadState(PostEffectState store) =>
        from read in EffectStates.Folded<TState>(Owner, path => Stored(store, EffectCollection.ParameterName(path)))
        from _ in held.SwapIO(value => new EffectValue<TState>(read.Record, value.Amount.Map(_ => read.Mix)))
        from __ in unless(read.Refused.IsEmpty, IO.fail<Unit>(Error.Many(read.Refused))).As()
        select unit;

    public IO<Unit> WriteState(PostEffectState store) =>
        held.ValueIO.Bind(value => IO.lift(() => Callbacks.Each(
            EffectStates.Captured(value.Record) + value.Amount.Map(static mix => (Key: EffectStates.Amount.Name, Text: EffectStates.Amount.Text(mix))).ToSeq(),
            entry => store.SetValue(entry.Key, entry.Text),
            nameof(PostEffectState.SetValue))));

    public IO<Unit> ResetToFactoryDefaults() =>
        held.SwapIO(static value => new EffectValue<TState>(TState.Default, value.Amount.Map(static _ => EffectStates.Amount.Default))).Map(static _ => unit);

    private static Option<string> Stored(PostEffectState store, string key) => Callbacks.Found(store.TryGetValue(key, out string text), text);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class EffectStates {
    // --- [AMOUNT]
    public static readonly ValueKey<Mix, float, InvalidGrade> Amount =
        ValueKey.Of<Mix, float, InvalidGrade>("mix", "Mix", "Blends the effect's result over its input, the input alone at 0", Mix.Full);

    internal static readonly Seq<string> AmountPath = [Amount.Name];

    public static Option<Mix> Mixed(PostEffectType stage) => Callbacks.Found(stage is not PostEffectType.ToneMapping, Amount.Default);

    // --- [KEYS]
    public static Seq<(string Key, string Text)> Captured<TState>(TState record) where TState : IStateRecord<TState> =>
        FieldTexts<TState>.Items.Choose(field => field.Capture(record).Map(text => (Key: EffectCollection.ParameterName(field.Path), Text: text)));

    // --- [RECALL]
    public static IO<EffectValue<TState>> Recalled<TState>(string owner, ValueSet set, Option<Mix> amount) where TState : IStateRecord<TState> =>
        Folded<TState>(owner, path => set.Entries.Find(new EntryKey(owner, path))).Bind(read => read.Refused.IsEmpty
            ? IO.pure(new EffectValue<TState>(read.Record, amount.Map(_ => read.Mix)))
            : IO.fail<EffectValue<TState>>(Error.Many(read.Refused)));

    internal static IO<(TState Record, Mix Mix, Seq<Error> Refused)> Folded<TState>(string owner, Func<Seq<string>, Option<string>> stored) where TState : IStateRecord<TState> =>
        from parts in FieldTexts.Folded<TState>(owner, stored)
        from mix in IO.lift(() => stored(AmountPath).Map(text => new EntryKey(owner, AmountPath).Read(text, Amount.Read)))
        select (
            parts.Record,
            mix.Bind(static read => read.ToOption()).IfNone(Amount.Default),
            parts.Refused + mix.ToSeq().Bind(static read => read.Match(Succ: static _ => Seq<Error>(), Fail: static error => Seq(error))));

    // --- [BINDING]
    public static ValueBinding Binding<TEffect, TState, TParameter, TError>(RhinoDoc doc, Option<Mix> amount, Func<TParameter, ParameterText> text)
        where TEffect : DefinedEffect<TEffect, TState, TParameter, TError>
        where TState : IStateRecord<TState, TParameter, TError>
        where TParameter : class, IStateParameter<TState>, ISmartEnum<string, TParameter, TError>
        where TError : Error, IValidationError<TError> =>
        EntryKey.TypeOwner(typeof(TEffect).GUID) switch {
            var owner => Joined(
                ValueBinding.Fields(
                    owner,
                    ValueStore.Of(
                        Sources.Read(doc, static window => EffectCollection.State<TEffect, TState, TParameter, TError>(window.Settings)).Map(static record => Some(record)),
                        record => Sources.Edit(doc, window => EffectCollection.Apply(window.Settings, Captured(record.IfNone(TState.Default))
                            .Map<EffectRequest>(static entry => EffectRequest.Tuning.Of<TEffect, TState, TParameter, TError>(entry.Key, entry.Text)))).Map(static _ => unit),
                        Applied.Live,
                        None),
                    TState.Default,
                    ParameterText.Join<TState, TParameter, TError>(text).Map<(IStateParameter<TState> Parameter, string Caption)>(static pair => (pair.Parameter, pair.Text.Caption))),
                amount.Map(_ => ValueBinding.Key(owner, Amount, ValueStore.Of(
                    Sources.Read(doc, static window => EffectCollection.Amount<TEffect>(window.Settings)),
                    mix => Sources.Edit(doc, window => EffectCollection.Apply(window.Settings, [EffectRequest.Tuning.Amount<TEffect>(mix.IfNone(Amount.Default))])).Map(static _ => unit),
                    Applied.Live,
                    None)))),
        };

    private static ValueBinding Joined(ValueBinding record, Option<ValueBinding> amount) =>
        amount.Match(
            Some: mix => new ValueBinding(
                record.Owner,
                record.Texts + mix.Texts,
                (record.Capture, mix.Capture).Apply(static (fields, held) => fields + held).As(),
                entries => (record.Recall(entries), mix.Recall(entries)).Apply(static (fields, held) => fields.Bind(_ => held)).As()),
            None: () => record);

    // --- [LOOKS]
    public static Seq<(TPreset Item, EffectLook<TState, TParameter> Look)> Partial<TState, TParameter, TPreset>(Seq<TPreset> items, Func<TPreset, TState> record, Seq<TParameter> keys)
        where TParameter : class, IStateParameter<TState> =>
        items.Map<(TPreset Item, EffectLook<TState, TParameter> Look)>(item => (item, new EffectLook<TState, TParameter>.Partial(record(item), keys)));
}
