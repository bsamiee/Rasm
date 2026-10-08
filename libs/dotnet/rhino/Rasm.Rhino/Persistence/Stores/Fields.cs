using System.Globalization;
using Rasm.Imaging.Pixels;
using Rasm.Rhino.Document.Notation;

namespace Rasm.Rhino.Persistence.Stores;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record FieldText<TRecord>(Seq<string> Path, Func<TRecord, Option<string>> Capture, Func<string, Fin<IO<Func<TRecord, TRecord>>>> Recall) {
    public Func<string, string> Shown { get; init; } = static text => text;

    public Option<Func<string, Option<string>>> File { get; init; }

    internal FieldText<TRecord> Under(string key) => this with { Path = key.Cons(Path) };

    internal FieldText<TOuter> Within<TOuter>(Func<TOuter, Option<TRecord>> held, Func<TOuter, TRecord> chosen, Func<TOuter, TRecord, TOuter> set) =>
        new(Path, outer => held(outer).Bind(Capture), text => Recall(text).Map(step => step.Map<Func<TOuter, TOuter>>(inner => outer => set(outer, inner(chosen(outer)))))) { Shown = Shown, File = File };
}

public sealed record KeyParameter<TValue>(string Key, StateParameter<TValue> Kind) : IStateParameter<TValue>;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class FieldTexts<TRecord> where TRecord : IStateRecord<TRecord> {
    public static readonly Seq<FieldText<TRecord>> Items = toSeq(TRecord.Parameters).Bind(FieldTexts.Of).Strict();
}

public static class FieldTexts {
    public static Seq<FieldText<TRecord>> Of<TRecord>(IStateParameter<TRecord> parameter) =>
        parameter.Kind.Accept(Crossing<TRecord>.Instance).Map(text => text.Under(parameter.Key));

    public static (Func<TKey, string> Shown, Func<TKey, string> Entry) Number<TValue, TKey>(Presentation<TValue, TKey> presentation)
        where TValue : System.Numerics.IMinMaxValue<TValue>, IConvertible<TKey>
        where TKey : struct, System.Numerics.INumber<TKey> {
        ScalarDisplay display = Quantities.Scalar(presentation);
        string digits = display.Decimals == 0 ? "0" : $"0.{new string('0', display.Decimals)}";
        string format = presentation.Soft.Low < TKey.Zero ? $"+{digits};-{digits};{digits}" : digits;
        Func<TKey, CultureInfo, string> number = (key, culture) => display.Shown(double.CreateChecked(key)).ToString(format, culture);
        return (
            key => display.Symbol.Match(Some: symbol => $"{number(key, RowText.Culture)} {symbol}", None: () => number(key, RowText.Culture)),
            key => number(key, CultureInfo.InvariantCulture));
    }

    public static IO<(TRecord Record, Seq<Error> Refused)> Folded<TRecord>(string owner, Func<Seq<string>, Option<string>> stored) where TRecord : IStateRecord<TRecord> =>
        from parts in FieldTexts<TRecord>.Items
            .Map<K<IO, Option<Func<TRecord, TRecord>>>>(item =>
                IO.lift(() => stored(item.Path).Traverse(text => new EntryKey(owner, item.Path).Read(text, item.Recall)).As()).Bind(static found => found.Traverse(static step => step).As()))
            .PartitionFallible()
            .As()
        select (parts.Succs.Somes().Fold(TRecord.Default, static (record, set) => set(record)), parts.Fails);

    public static IO<TRecord> Recalled<TRecord>(string owner, Func<Seq<string>, Option<string>> stored) where TRecord : IStateRecord<TRecord> =>
        from parts in Folded<TRecord>(owner, stored)
        from _ in unless(parts.Refused.IsEmpty, IO.fail<Unit>(Error.Many(parts.Refused))).As()
        select parts.Record;

    private sealed class Crossing<TRecord> : IStateParameterVisitor<TRecord, Seq<FieldText<TRecord>>> {
        public static readonly Crossing<TRecord> Instance = new();

        public Seq<FieldText<TRecord>> Bounded<TValue, TKey, TError>(Lens<TRecord, TValue> lens, Presentation<TValue, TKey> presentation)
            where TValue : IObjectFactory<TValue, TKey, TError>, IConvertible<TKey>, System.Numerics.IMinMaxValue<TValue>
            where TKey : struct, System.Numerics.INumber<TKey>
            where TError : Error, IValidationError<TError> =>
            [Field(lens, StoredText.Capture<TValue, TKey>, StoredText.Recall<TValue, TKey, TError>) with { Shown = Numbered(presentation) }];

        public Seq<FieldText<TRecord>> OptionalBounded<TValue, TKey, TError>(Lens<TRecord, Gated<TValue>> lens, Presentation<TValue, TKey> presentation)
            where TValue : IObjectFactory<TValue, TKey, TError>, IConvertible<TKey>, System.Numerics.IMinMaxValue<TValue>
            where TKey : struct, System.Numerics.INumber<TKey>
            where TError : Error, IValidationError<TError> =>
            Gate(lens, value => Bounded<TValue, TKey, TError>(value, presentation));

        public Seq<FieldText<TRecord>> Choice<TValue, TError>(Lens<TRecord, TValue> lens)
            where TValue : ISmartEnum<string, TValue, TError>
            where TError : Error, IValidationError<TError> =>
            [Field(lens, StoredText.Capture<TValue, string>, StoredText.Recall<TValue, string, TError>)];

        public Seq<FieldText<TRecord>> OptionalChoice<TValue, TError>(Lens<TRecord, Option<TValue>> lens)
            where TValue : ISmartEnum<string, TValue, TError>
            where TError : Error, IValidationError<TError> =>
            [OptionalField(lens, StoredText.Capture<TValue, string>, StoredText.Recall<TValue, string, TError>)];

        public Seq<FieldText<TRecord>> Variant<TValue, TCase, TError>(Lens<TRecord, TValue> lens)
            where TValue : class
            where TCase : class, IStateCase<TValue>, ISmartEnum<string, TCase, TError>
            where TError : Error, IValidationError<TError> =>
            from item in toSeq(TCase.Items)
            from field in item.Accept(Cases<TValue>.Instance)
            select Focused(field, lens).Under(item.ToValue());

        public Seq<FieldText<TRecord>> Enumerated<TEnum>(Lens<TRecord, TEnum> lens) where TEnum : struct, Enum =>
            [Field(lens, StoredText.Format, StoredText.Member<TEnum>)];

        public Seq<FieldText<TRecord>> Record<TNested>(Lens<TRecord, TNested> lens) where TNested : IStateRecord<TNested> =>
            FieldTexts<TNested>.Items.Map(field => Focused(field, lens));

        public Seq<FieldText<TRecord>> OptionalRecord<TNested>(Lens<TRecord, Gated<TNested>> lens) where TNested : IStateRecord<TNested> =>
            Gate(lens, Record);

        public Seq<FieldText<TRecord>> Toggle(Lens<TRecord, bool> lens) =>
            [Field(lens, StoredText.Format, StoredText.Parse<bool>)];

        public Seq<FieldText<TRecord>> Raw<TRaw>(Lens<TRecord, TRaw> lens) where TRaw : notnull, ISpanParsable<TRaw> =>
            [Field(lens, StoredText.Format, StoredText.Parse<TRaw>)];

        public Seq<FieldText<TRecord>> OptionalRaw<TRaw>(Lens<TRecord, Option<TRaw>> lens) where TRaw : notnull, ISpanParsable<TRaw> =>
            [OptionalField(lens, StoredText.Format, StoredText.Parse<TRaw>)];

        public Seq<FieldText<TRecord>> Color(Lens<TRecord, Swatch> lens) =>
            [Field(lens, StoredText.Capture<Swatch, int>, StoredText.Recall<Swatch, int, InvalidPixelValue>) with { Shown = Hex }];

        public Seq<FieldText<TRecord>> OptionalColor(Lens<TRecord, Gated<Swatch>> lens) =>
            Gate(lens, Color);

        public Seq<FieldText<TRecord>> Gradient(Lens<TRecord, Ramp> lens) =>
            [Field(lens, StoredText.Capture<Ramp, string>, StoredText.Recall<Ramp, string, InvalidPixelValue>)];

        public Seq<FieldText<TRecord>> Swatches<TValue, TError>(Lens<TRecord, TValue> lens)
            where TValue : IObjectFactory<TValue, Seq<Swatch>, TError>, IConvertible<Seq<Swatch>>, IObjectFactory<TValue, string, TError>, IConvertible<string>
            where TError : Error, IValidationError<TError> =>
            [Field(lens, StoredText.Capture<TValue, string>, StoredText.Recall<TValue, string, TError>) with { Shown = Listed<TValue, TError> }];

        public Seq<FieldText<TRecord>> Keyed<TValue, TRaw, TError>(Lens<TRecord, TValue> lens)
            where TValue : IObjectFactory<TValue, TRaw, TError>, IConvertible<TRaw>
            where TRaw : notnull, ISpanParsable<TRaw>
            where TError : Error, IValidationError<TError> =>
            [Field(lens, StoredText.Capture<TValue, TRaw>, StoredText.Recall<TValue, TRaw, TError>)];

        public Seq<FieldText<TRecord>> OptionalKeyed<TValue, TRaw, TError>(Lens<TRecord, Option<TValue>> lens)
            where TValue : IObjectFactory<TValue, TRaw, TError>, IConvertible<TRaw>
            where TRaw : notnull, ISpanParsable<TRaw>
            where TError : Error, IValidationError<TError> =>
            [OptionalField(lens, StoredText.Capture<TValue, TRaw>, StoredText.Recall<TValue, TRaw, TError>)];

        public Seq<FieldText<TRecord>> Loaded<TValue, TKey, TRaw, TError>(Lens<TRecord, Option<TValue>> lens, Func<TKey, IO<TValue>> load, Func<TValue, TKey> key)
            where TKey : IObjectFactory<TKey, TRaw, TError>, IConvertible<TRaw>
            where TRaw : notnull, ISpanParsable<TRaw>
            where TError : Error, IValidationError<TError> =>
            [new([],
                record => Some(lens.Get(record).Map(value => StoredText.Capture<TKey, TRaw>(key(value))).IfNone(Marker)),
                text => Unmarked(text).Traverse(StoredText.Recall<TKey, TRaw, TError>).As().Map(found => found.Traverse(load).As().Bind(Put(lens)))) { File = Some(Unmarked) }];

        public Seq<FieldText<TRecord>> Opaque<TValue>(Lens<TRecord, TValue> lens) where TValue : notnull => [];

        private Seq<FieldText<TRecord>> Gate<TValue>(Lens<TRecord, Gated<TValue>> gate, Func<Lens<TRecord, TValue>, Seq<FieldText<TRecord>>> value) where TValue : notnull =>
            Toggle(lens(gate, Gated<TValue>.EnabledEntry.Lens)).Map(static field => field.Under(Gated<TValue>.EnabledEntry.Key))
            + value(lens(gate, Gated<TValue>.ValueEntry.Lens)).Map(static field => field.Under(Gated<TValue>.ValueEntry.Key));

        private static FieldText<TRecord> Field<TValue>(Lens<TRecord, TValue> lens, Func<TValue, string> capture, Func<string, Fin<TValue>> recall) =>
            new([], record => Some(capture(lens.Get(record))), text => recall(text).Map(Put(lens)));

        private static FieldText<TRecord> OptionalField<TValue>(Lens<TRecord, Option<TValue>> lens, Func<TValue, string> capture, Func<string, Fin<TValue>> recall) =>
            new([], record => Some(lens.Get(record).Map(capture).IfNone(Marker)), text => Unmarked(text).Traverse(recall).As().Map(Put(lens)));

        private static Func<TFocus, IO<Func<TRecord, TRecord>>> Put<TFocus>(Lens<TRecord, TFocus> lens) =>
            value => IO.pure<Func<TRecord, TRecord>>(record => lens.Set(value, record));

        private static FieldText<TRecord> Focused<TFocus>(FieldText<TFocus> field, Lens<TRecord, TFocus> lens) =>
            field.Within(record => Some(lens.Get(record)), lens.Get, (record, focus) => lens.Set(focus, record));

        private static Func<string, string> Numbered<TValue, TKey>(Presentation<TValue, TKey> presentation)
            where TValue : System.Numerics.IMinMaxValue<TValue>, IConvertible<TKey>
            where TKey : struct, System.Numerics.INumber<TKey> =>
            Number(presentation).Shown switch {
                var number => text => StoredText.Parse<TKey>(text).Map(number).IfFail(_ => text),
            };

        private static string Hex(string text) =>
            StoredText.Recall<Swatch, int, InvalidPixelValue>(text).Map(Coded).IfFail(_ => text);

        private static string Listed<TValue, TError>(string text)
            where TValue : IObjectFactory<TValue, string, TError>, IConvertible<Seq<Swatch>>
            where TError : Error, IValidationError<TError> =>
            StoredText.Recall<TValue, string, TError>(text).Map(static value => string.Join(' ', value.ToValue().Map(Coded))).IfFail(_ => text);

        private static string Coded(Swatch swatch) => $"#{(int)swatch:X6}";

        private sealed class Cases<TValue> : IStateCaseVisitor<TValue, Seq<FieldText<TValue>>> where TValue : class {
            public static readonly Cases<TValue> Instance = new();

            public Seq<FieldText<TValue>> Case<TState>() where TState : class, TValue, IStateRecord<TState> =>
                FieldTexts<TState>.Items.IsEmpty
                    ? [new([], static value => Callbacks.Found(value is TState, Marker), static text => Unmarked(text).Match(
                        Some: static held => new UnreadText(held, typeof(TState)),
                        None: static () => Fin.Succ(IO.pure<Func<TValue, TValue>>(static value => value as TState ?? TState.Default))))]
                    : FieldTexts<TState>.Items.Map(static field => field.Within<TValue>(static value => Optional(value as TState), static value => value as TState ?? TState.Default, static (_, held) => held));
        }
    }

    private const string Marker = "—";

    private static Option<string> Unmarked(string text) => Some(text).Filter(static held => !string.Equals(held, Marker, StringComparison.Ordinal));
}
