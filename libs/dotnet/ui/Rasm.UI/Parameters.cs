using System.Globalization;
using System.Numerics;
using System.Text.Json;
using Rasm.UI.Styling;
using SkiaSharp;

namespace Rasm.UI;

// --- [TYPES] ---------------------------------------------------------------------------
public interface ICaptioned {
    public string Key { get; }
    public string Caption => Captions.Of(Key);
}

public interface IStateParameterVisitor<TRecord, out TResult> {
    public TResult Numeric<TValue, TKey>(Lens<TRecord, TValue> lens, NumericDomain<TValue, TKey> domain)
        where TValue : notnull
        where TKey : struct, INumber<TKey>;

    public TResult OptionalNumeric<TValue, TKey>(Lens<TRecord, Gated<TValue>> lens, NumericDomain<TValue, TKey> domain)
        where TValue : notnull
        where TKey : struct, INumber<TKey>;

    public TResult Choice<TValue>(Lens<TRecord, TValue> lens) where TValue : ISmartEnum<string, TValue, ValidationError>;

    public TResult OptionalChoice<TValue>(Lens<TRecord, Option<TValue>> lens, string absent) where TValue : ISmartEnum<string, TValue, ValidationError>;

    public TResult Variant<TValue>(Lens<TRecord, TValue> lens) where TValue : class;

    public TResult Enumerated<TEnum>(Lens<TRecord, TEnum> lens) where TEnum : struct, Enum;

    public TResult Record<TNested>(Lens<TRecord, TNested> lens) where TNested : IStateRecord<TNested>;

    public TResult OptionalRecord<TNested>(Lens<TRecord, Gated<TNested>> lens) where TNested : IStateRecord<TNested>;

    public TResult Toggle(Lens<TRecord, bool> lens);

    public TResult Raw<TRaw>(Lens<TRecord, TRaw> lens) where TRaw : notnull, ISpanParsable<TRaw>;

    public TResult OptionalRaw<TRaw>(Lens<TRecord, Option<TRaw>> lens) where TRaw : notnull, ISpanParsable<TRaw>;

    public TResult Color(Lens<TRecord, SKColorF> lens);

    public TResult OptionalColor(Lens<TRecord, Gated<SKColorF>> lens);

    public TResult Gradient(Lens<TRecord, ColorRamp> lens);

    public TResult Colors<TValue>(Lens<TRecord, TValue> lens) where TValue : IObjectFactory<TValue, Seq<SKColorF>, ValidationError>, IConvertible<Seq<SKColorF>>;

    public TResult Keyed<TValue, TRaw>(Lens<TRecord, TValue> lens)
        where TValue : IObjectFactory<TValue, TRaw, ValidationError>, IConvertible<TRaw>
        where TRaw : notnull, ISpanParsable<TRaw>;

    public TResult OptionalKeyed<TValue, TRaw>(Lens<TRecord, Option<TValue>> lens)
        where TValue : IObjectFactory<TValue, TRaw, ValidationError>, IConvertible<TRaw>
        where TRaw : notnull, ISpanParsable<TRaw>;

    public TResult Loaded<TValue, TKey, TRaw>(Lens<TRecord, Option<TValue>> lens, Func<TKey, IO<TValue>> load, Func<TValue, TKey> key)
        where TKey : IObjectFactory<TKey, TRaw, ValidationError>, IConvertible<TRaw>
        where TRaw : notnull, ISpanParsable<TRaw>;

    public TResult Opaque<TValue>(Lens<TRecord, TValue> lens) where TValue : notnull;
}

public interface IStateCaseVisitor<TUnion, out TResult> where TUnion : class {
    public TResult Case<TCase>() where TCase : class, TUnion, IStateRecord<TCase>;
}

public interface IStateParameter<TRecord> {
    public string Key { get; }
    public StateParameter<TRecord> Kind { get; }
    public ParameterText Text { get; }
}

public interface IStateRecord<TSelf> where TSelf : IStateRecord<TSelf> {
    public static abstract TSelf Default { get; }
    public static virtual string Caption => Captions.Of(typeof(TSelf));
    public static virtual Seq<IStateParameter<TSelf>> Parameters => [];
    public static virtual Option<IStateParameter<TSelf>> FindParameter(string key) => None;
}

public interface IStateRecord<TSelf, TParameter> : IStateRecord<TSelf>
    where TSelf : IStateRecord<TSelf, TParameter>
    where TParameter : class, IStateParameter<TSelf>, ISmartEnum<string, TParameter, ValidationError> {
    static Seq<IStateParameter<TSelf>> IStateRecord<TSelf>.Parameters => [.. TParameter.Items];

    static Option<IStateParameter<TSelf>> IStateRecord<TSelf>.FindParameter(string key) =>
        TParameter.TryGet(key, out TParameter? parameter) ? Some<IStateParameter<TSelf>>(parameter) : None;
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ParameterText(string Caption, string Help);

public sealed record Gated<TValue>(bool Enabled, TValue Value) where TValue : notnull {
    public static readonly (string Key, Lens<Gated<TValue>, bool> Lens) EnabledEntry =
        ("enabled", Lens<Gated<TValue>, bool>.New(static gate => gate.Enabled, static enabled => gate => gate with { Enabled = enabled }));
    public static readonly (string Key, Lens<Gated<TValue>, TValue> Lens) ValueEntry =
        ("value", Lens<Gated<TValue>, TValue>.New(static gate => gate.Value, static value => gate => gate with { Value = value }));

    public Option<TValue> Active => Enabled ? Some(Value) : None;
}

public sealed record RecordEntry<TRecord>(Seq<string> Path, Func<TRecord, Option<string>> Capture, Func<string, Fin<IO<Func<TRecord, TRecord>>>> Recall) {
    public required Func<TRecord, TRecord, TRecord> Taken { get; init; }
    public Option<string> Absent { get; init; }
    public Seq<RecordEntry<TRecord>> Children { get; init; }
    public Func<TRecord, bool> Active { get; init; } = static _ => true;
    public Option<Func<string, Option<string>>> File { get; init; }

    internal Seq<RecordEntry<TRecord>> Flattened => this.Cons(Children.Bind(static entry => entry.Flattened));

    internal RecordEntry<TRecord> Under(string key) => this with { Path = key.Cons(Path), Children = Children.Map(entry => entry.Under(key)) };

    internal RecordEntry<TOuter> Within<TOuter>(Func<TOuter, Option<TRecord>> held, Func<TOuter, TRecord> chosen, Func<TOuter, TRecord, TOuter> set) =>
        new(Path, outer => held(outer).Bind(Capture), text => Recall(text).Map(step => step.Map<Func<TOuter, TOuter>>(inner => outer => set(outer, inner(chosen(outer)))))) {
            Taken = (from, into) => set(into, Taken(chosen(from), chosen(into))),
            Absent = Absent,
            Children = Children.Map(entry => entry.Within(held, chosen, set)),
            Active = outer => held(outer).Exists(Active),
            File = File,
        };
}

public abstract class StateCase<TUnion> where TUnion : class {
    private StateCase() { }

    public static readonly Seq<StateCase<TUnion>> Items =
        toSeq(typeof(TUnion).GetNestedTypes()).Filter(static type => type.IsAssignableTo(typeof(TUnion)))
            .Map(static type => (StateCase<TUnion>)Activator.CreateInstance(typeof(StateCase<>.Case<>).MakeGenericType(typeof(TUnion), type))!).Strict();

    public abstract string Key { get; }
    public abstract string Caption { get; }
    public abstract TUnion Default { get; }
    internal abstract Seq<RecordEntry<TUnion>> Entries { get; }

    public abstract bool Holds(TUnion value);

    public abstract TResult Accept<TResult>(IStateCaseVisitor<TUnion, TResult> visitor);

    public sealed class Case<TState> : StateCase<TUnion> where TState : class, TUnion, IStateRecord<TState> {
        public override string Key => typeof(TState).Name;
        public override string Caption => TState.Caption;
        public override TUnion Default => TState.Default;

        internal override Seq<RecordEntry<TUnion>> Entries =>
            RecordEntries.Roots<TState>().Map(entry => entry.Within<TUnion>(
                static value => Optional(value as TState), static value => value as TState ?? TState.Default, static (value, held) => value is TState ? held : value).Under(Key));

        public override bool Holds(TUnion value) => value is TState;

        public override TResult Accept<TResult>(IStateCaseVisitor<TUnion, TResult> visitor) => visitor.Case<TState>();
    }
}

public abstract class StateParameter<TRecord> {
    private const string Marker = "—";
    private const string Selector = "$case";

    private StateParameter() { }

    private protected abstract Seq<RecordEntry<TRecord>> Stored { get; }

    public abstract TResult Accept<TResult>(IStateParameterVisitor<TRecord, TResult> visitor);

    public abstract TRecord Taken(TRecord from, TRecord into);

    public Seq<RecordEntry<TRecord>> Entries(string key) => Stored.Map(entry => entry.Under(key));

    // --- [TEXT]
    private static string Written<TRaw>(TRaw raw) where TRaw : notnull => Convert.ToString(raw, CultureInfo.InvariantCulture)!;

    private static Fin<TRaw> Parsed<TRaw>(string text) where TRaw : ISpanParsable<TRaw> =>
        TRaw.TryParse(text, CultureInfo.InvariantCulture, out TRaw? raw) ? raw : Unparsed<TRaw>(text);

    private static Fin<TValue> Recalled<TValue, TRaw>(string text) where TValue : IObjectFactory<TValue, TRaw, ValidationError> where TRaw : notnull, ISpanParsable<TRaw> =>
        Parsed<TRaw>(text).Bind(ValidationResults.Validated<TValue, TRaw>);

    private static Error Unparsed<TValue>(string text) => Errors.ParseError($"'{text}' is no {typeof(TValue).Name}");

    private static Option<string> Present(string text) => Some(text).Filter(static held => !string.Equals(held, Marker, StringComparison.Ordinal));

    private static Func<Option<TValue>, string> Marked<TValue>(Func<TValue, string> write) => value => value.Map(write).IfNone(Marker);

    private static Func<string, Fin<Option<TValue>>> Unmarked<TValue>(Func<string, Fin<TValue>> read) => text => Present(text).Traverse(read).As();

    private static Seq<RecordEntry<TRecord>> Gate<TValue>(Lens<TRecord, Gated<TValue>> gate, Func<Lens<TRecord, TValue>, StateParameter<TRecord>> value) where TValue : notnull =>
        new Toggle(lens(gate, Gated<TValue>.EnabledEntry.Lens)).Entries(Gated<TValue>.EnabledEntry.Key)
        + value(lens(gate, Gated<TValue>.ValueEntry.Lens)).Entries(Gated<TValue>.ValueEntry.Key);

    // --- [CASES]
    public abstract class Focused<TFocus> : StateParameter<TRecord> {
        private protected Focused(Lens<TRecord, TFocus> lens) => Lens = lens;

        public Lens<TRecord, TFocus> Lens { get; }

        public sealed override TRecord Taken(TRecord from, TRecord into) => Lens.Set(Lens.Get(from), into);

        private protected IO<Func<TRecord, TRecord>> Put(TFocus value) => IO.pure<Func<TRecord, TRecord>>(record => Lens.Set(value, record));

        private protected RecordEntry<TRecord> Field(Func<TFocus, string> write, Func<string, Fin<TFocus>> read) =>
            new([], record => Some(write(Lens.Get(record))), text => read(text).Map(Put)) { Taken = Taken };

        private protected RecordEntry<TRecord> Focus(RecordEntry<TFocus> entry) => entry.Within(record => Some(Lens.Get(record)), Lens.Get, (record, focus) => Lens.Set(focus, record));
    }

    public sealed class Numeric<TValue, TKey>(Lens<TRecord, TValue> lens, NumericDomain<TValue, TKey> domain) : Focused<TValue>(lens)
        where TValue : notnull
        where TKey : struct, INumber<TKey> {
        private protected override Seq<RecordEntry<TRecord>> Stored =>
            [Field(value => Written(domain.Key(value)), text => Invariant.Number<TKey>(text).ToFin(Unparsed<TKey>(text)).Bind(domain.Create))];

        public override TResult Accept<TResult>(IStateParameterVisitor<TRecord, TResult> visitor) => visitor.Numeric(Lens, domain);
    }

    public sealed class OptionalNumeric<TValue, TKey>(Lens<TRecord, Gated<TValue>> lens, NumericDomain<TValue, TKey> domain) : Focused<Gated<TValue>>(lens)
        where TValue : notnull
        where TKey : struct, INumber<TKey> {
        private protected override Seq<RecordEntry<TRecord>> Stored => Gate(Lens, value => new Numeric<TValue, TKey>(value, domain));

        public override TResult Accept<TResult>(IStateParameterVisitor<TRecord, TResult> visitor) => visitor.OptionalNumeric(Lens, domain);
    }

    public sealed class Choice<TValue>(Lens<TRecord, TValue> lens) : Focused<TValue>(lens) where TValue : ISmartEnum<string, TValue, ValidationError> {
        private protected override Seq<RecordEntry<TRecord>> Stored => [Field(static value => value.ToValue(), ValidationResults.Validated<TValue, string>)];

        public override TResult Accept<TResult>(IStateParameterVisitor<TRecord, TResult> visitor) => visitor.Choice(Lens);
    }

    public sealed class OptionalChoice<TValue>(Lens<TRecord, Option<TValue>> lens, string absent) : Focused<Option<TValue>>(lens)
        where TValue : ISmartEnum<string, TValue, ValidationError> {
        private protected override Seq<RecordEntry<TRecord>> Stored =>
            [Field(Marked<TValue>(static value => value.ToValue()), Unmarked(ValidationResults.Validated<TValue, string>)) with { Absent = Some(Marker) }];

        public override TResult Accept<TResult>(IStateParameterVisitor<TRecord, TResult> visitor) => visitor.OptionalChoice(Lens, absent);
    }

    public sealed class Variant<TValue>(Lens<TRecord, TValue> lens) : Focused<TValue>(lens) where TValue : class {
        private protected override Seq<RecordEntry<TRecord>> Stored =>
            [new([Selector], record => Some(Held(Lens.Get(record)).Key), text => Named(text).Map(item => IO.pure<Func<TRecord, TRecord>>(record => Chosen(item, record)))) {
                Taken = (from, into) => Chosen(Held(Lens.Get(from)), into),
                Children = StateCase<TValue>.Items.Bind(static item => item.Entries).Map(Focus),
            }];

        public override TResult Accept<TResult>(IStateParameterVisitor<TRecord, TResult> visitor) => visitor.Variant(Lens);

        private static StateCase<TValue> Held(TValue value) => StateCase<TValue>.Items.First(item => item.Holds(value));

        private static Fin<StateCase<TValue>> Named(string text) =>
            StateCase<TValue>.Items.Find(item => string.Equals(item.Key, text, StringComparison.Ordinal)).ToFin(Unparsed<TValue>(text));

        private TRecord Chosen(StateCase<TValue> item, TRecord record) => item.Holds(Lens.Get(record)) ? record : Lens.Set(item.Default, record);
    }

    public sealed class Enumerated<TEnum>(Lens<TRecord, TEnum> lens) : Focused<TEnum>(lens) where TEnum : struct, Enum {
        private protected override Seq<RecordEntry<TRecord>> Stored =>
            [Field(Written, static text => Enum.TryParse(text, ignoreCase: false, out TEnum member) && Enum.IsDefined(member) ? member : Unparsed<TEnum>(text))];

        public override TResult Accept<TResult>(IStateParameterVisitor<TRecord, TResult> visitor) => visitor.Enumerated(Lens);
    }

    public sealed class Record<TNested>(Lens<TRecord, TNested> lens) : Focused<TNested>(lens) where TNested : IStateRecord<TNested> {
        private protected override Seq<RecordEntry<TRecord>> Stored => RecordEntries.Roots<TNested>().Map(Focus);

        public override TResult Accept<TResult>(IStateParameterVisitor<TRecord, TResult> visitor) => visitor.Record(Lens);
    }

    public sealed class OptionalRecord<TNested>(Lens<TRecord, Gated<TNested>> lens) : Focused<Gated<TNested>>(lens) where TNested : IStateRecord<TNested> {
        private protected override Seq<RecordEntry<TRecord>> Stored => Gate(Lens, static value => new Record<TNested>(value));

        public override TResult Accept<TResult>(IStateParameterVisitor<TRecord, TResult> visitor) => visitor.OptionalRecord(Lens);
    }

    public sealed class Toggle(Lens<TRecord, bool> lens) : Focused<bool>(lens) {
        private protected override Seq<RecordEntry<TRecord>> Stored => [Field(Written, Parsed<bool>)];

        public override TResult Accept<TResult>(IStateParameterVisitor<TRecord, TResult> visitor) => visitor.Toggle(Lens);
    }

    public sealed class Raw<TRaw>(Lens<TRecord, TRaw> lens) : Focused<TRaw>(lens) where TRaw : notnull, ISpanParsable<TRaw> {
        private protected override Seq<RecordEntry<TRecord>> Stored => [Field(Written, Parsed<TRaw>)];

        public override TResult Accept<TResult>(IStateParameterVisitor<TRecord, TResult> visitor) => visitor.Raw(Lens);
    }

    public sealed class OptionalRaw<TRaw>(Lens<TRecord, Option<TRaw>> lens) : Focused<Option<TRaw>>(lens) where TRaw : notnull, ISpanParsable<TRaw> {
        private protected override Seq<RecordEntry<TRecord>> Stored => [Field(Marked<TRaw>(Written), Unmarked(Parsed<TRaw>)) with { Absent = Some(Marker) }];

        public override TResult Accept<TResult>(IStateParameterVisitor<TRecord, TResult> visitor) => visitor.OptionalRaw(Lens);
    }

    public sealed class Color(Lens<TRecord, SKColorF> lens) : Focused<SKColorF>(lens) {
        private protected override Seq<RecordEntry<TRecord>> Stored => [Field(static color => new Ink.Fixed(color).ToValue(), Ink.Fixed.Parsed)];

        public override TResult Accept<TResult>(IStateParameterVisitor<TRecord, TResult> visitor) => visitor.Color(Lens);
    }

    public sealed class OptionalColor(Lens<TRecord, Gated<SKColorF>> lens) : Focused<Gated<SKColorF>>(lens) {
        private protected override Seq<RecordEntry<TRecord>> Stored => Gate(Lens, static value => new Color(value));

        public override TResult Accept<TResult>(IStateParameterVisitor<TRecord, TResult> visitor) => visitor.OptionalColor(Lens);
    }

    public sealed class Gradient(Lens<TRecord, ColorRamp> lens) : Focused<ColorRamp>(lens) {
        private protected override Seq<RecordEntry<TRecord>> Stored => [Field(static ramp => ramp.ToValue(), ValidationResults.Validated<ColorRamp, string>)];

        public override TResult Accept<TResult>(IStateParameterVisitor<TRecord, TResult> visitor) => visitor.Gradient(Lens);
    }

    public sealed class Colors<TValue>(Lens<TRecord, TValue> lens) : Focused<TValue>(lens)
        where TValue : IObjectFactory<TValue, Seq<SKColorF>, ValidationError>, IConvertible<Seq<SKColorF>> {
        private protected override Seq<RecordEntry<TRecord>> Stored =>
            [Field(static value => string.Join(", ", value.ToValue().Map(static color => new Ink.Fixed(color).ToValue())),
                static text => toSeq(Ink.Split(text, static letter => letter is ',')).Traverse(Ink.Fixed.Parsed).As().Bind(ValidationResults.Validated<TValue, Seq<SKColorF>>))];

        public override TResult Accept<TResult>(IStateParameterVisitor<TRecord, TResult> visitor) => visitor.Colors(Lens);
    }

    public sealed class Keyed<TValue, TRaw>(Lens<TRecord, TValue> lens) : Focused<TValue>(lens)
        where TValue : IObjectFactory<TValue, TRaw, ValidationError>, IConvertible<TRaw>
        where TRaw : notnull, ISpanParsable<TRaw> {
        private protected override Seq<RecordEntry<TRecord>> Stored => [Field(static value => Written(value.ToValue()), Recalled<TValue, TRaw>)];

        public override TResult Accept<TResult>(IStateParameterVisitor<TRecord, TResult> visitor) => visitor.Keyed<TValue, TRaw>(Lens);
    }

    public sealed class OptionalKeyed<TValue, TRaw>(Lens<TRecord, Option<TValue>> lens) : Focused<Option<TValue>>(lens)
        where TValue : IObjectFactory<TValue, TRaw, ValidationError>, IConvertible<TRaw>
        where TRaw : notnull, ISpanParsable<TRaw> {
        private protected override Seq<RecordEntry<TRecord>> Stored =>
            [Field(Marked<TValue>(static value => Written(value.ToValue())), Unmarked(Recalled<TValue, TRaw>)) with { Absent = Some(Marker) }];

        public override TResult Accept<TResult>(IStateParameterVisitor<TRecord, TResult> visitor) => visitor.OptionalKeyed<TValue, TRaw>(Lens);
    }

    public sealed class Loaded<TValue, TKey, TRaw>(Lens<TRecord, Option<TValue>> lens, Func<TKey, IO<TValue>> load, Func<TValue, TKey> key) : Focused<Option<TValue>>(lens)
        where TKey : IObjectFactory<TKey, TRaw, ValidationError>, IConvertible<TRaw>
        where TRaw : notnull, ISpanParsable<TRaw> {
        private protected override Seq<RecordEntry<TRecord>> Stored =>
            [new([], record => Some(Marked<TValue>(value => Written(key(value).ToValue()))(Lens.Get(record))),
                text => Unmarked(Recalled<TKey, TRaw>)(text).Map(found => found.Traverse(load).As().Bind(Put))) {
                Taken = Taken,
                Absent = Some(Marker),
                File = Some(Present),
            }];

        public override TResult Accept<TResult>(IStateParameterVisitor<TRecord, TResult> visitor) => visitor.Loaded<TValue, TKey, TRaw>(Lens, load, key);
    }

    public sealed class Opaque<TValue>(Lens<TRecord, TValue> lens) : Focused<TValue>(lens) where TValue : notnull {
        private protected override Seq<RecordEntry<TRecord>> Stored => [];

        public override TResult Accept<TResult>(IStateParameterVisitor<TRecord, TResult> visitor) => visitor.Opaque(Lens);
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Captions {
    public static string Of(Type type) => Of(type.Name);

    public static string Of<TValue>(TValue item) where TValue : ISmartEnum<string> => item is ICaptioned captioned ? captioned.Caption : Of(item.ToValue());

    public static string Of(string name) =>
        JsonNamingPolicy.KebabCaseLower.ConvertName(name).Replace('-', ' ') switch {
            { Length: 0 } empty => empty,
            var words => $"{char.ToUpperInvariant(words[0])}{words.AsSpan(1)}",
        };
}

public static class RecordEntries {
    public static Seq<RecordEntry<TRecord>> Of<TRecord>(IStateParameter<TRecord> parameter) => parameter.Kind.Entries(parameter.Key).Bind(static entry => entry.Flattened);

    public static Seq<RecordEntry<TRecord>> Items<TRecord>() where TRecord : IStateRecord<TRecord> => Roots<TRecord>().Bind(static entry => entry.Flattened);

    internal static Seq<RecordEntry<TRecord>> Roots<TRecord>() where TRecord : IStateRecord<TRecord> =>
        TRecord.Parameters.Bind(static parameter => parameter.Kind.Entries(parameter.Key)).Strict();
}
