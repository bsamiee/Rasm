using System.Globalization;
using Rasm.Imaging.Pixels;
using Rasm.Rhino.Document.Notation;

namespace Rasm.Rhino.Persistence.Stores;

// --- [TYPES] ---------------------------------------------------------------------------
public interface IFieldKindVisitor<TRecord, out TResult> {
    public TResult Leaf<TLeaf>(StateParameter<TLeaf> kind, Func<TRecord, TLeaf> project);
}

// --- [MODELS] --------------------------------------------------------------------------
public abstract class FieldKind<TRecord> {
    private FieldKind() { }

    public abstract TResult Accept<TResult>(IFieldKindVisitor<TRecord, TResult> visitor);
    internal abstract FieldKind<TOuter> Within<TOuter>(Func<TOuter, TRecord> chosen);

    public sealed class Leaf<TLeaf>(StateParameter<TLeaf> kind, Func<TRecord, TLeaf> project) : FieldKind<TRecord> {
        public override TResult Accept<TResult>(IFieldKindVisitor<TRecord, TResult> visitor) => visitor.Leaf(kind, project);
        internal override FieldKind<TOuter> Within<TOuter>(Func<TOuter, TRecord> chosen) => new FieldKind<TOuter>.Leaf<TLeaf>(kind, outer => project(chosen(outer)));
    }
}

public sealed record FieldText<TRecord>(Seq<string> Path, Func<TRecord, Option<string>> Capture, Func<string, Fin<IO<Func<TRecord, TRecord>>>> Recall) {
    public required FieldKind<TRecord> Kind { get; init; }
    public required Func<TRecord, TRecord, TRecord> Taken { get; init; }
    public Option<string> Absent { get; init; }
    public Seq<FieldText<TRecord>> Children { get; init; }
    public Func<TRecord, bool> Active { get; init; } = static _ => true;
    public Func<string, string> Shown { get; init; } = static text => text;

    public Option<Func<string, Option<string>>> File { get; init; }

    internal FieldText<TRecord> Under(string key) => this with { Path = key.Cons(Path), Children = Children.Map(field => field.Under(key)) };

    internal Seq<FieldText<TRecord>> Flattened => this.Cons(Children.Bind(static @field => @field.Flattened));

    internal FieldText<TOuter> Within<TOuter>(Func<TOuter, Option<TRecord>> held, Func<TOuter, TRecord> chosen, Func<TOuter, TRecord, TOuter> set) =>
        new(Path, outer => held(outer).Bind(Capture), text => Recall(text).Map(step => step.Map<Func<TOuter, TOuter>>(inner => outer => set(outer, inner(chosen(outer)))))) {
            Kind = Kind.Within(chosen), Taken = (from, into) => set(into, Taken(chosen(from), chosen(into))), Absent = Absent,
            Children = Children.Map(field => field.Within(held, chosen, set)), Active = outer => held(outer).Exists(Active), Shown = Shown, File = File,
        };
}

public sealed record KeyParameter<TValue>(string Key, StateParameter<TValue> Kind) : IStateParameter<TValue>;

public sealed record FieldCodec<TValue>(Func<TValue, HashMap<Seq<string>, string>> Capture, Func<HashMap<Seq<string>, string>, IO<TValue>> Recall) {
    public FieldCodec<TValue> Under(string key) =>
        new(value => toHashMap(Capture(value).AsIterable().Map(entry => (key.Cons(entry.Key), entry.Value))),
            entries => Recall(toHashMap(entries.AsIterable().Filter(entry => entry.Key.Head.Exists(head => string.Equals(head, key, StringComparison.Ordinal)))
                .Map(static entry => (entry.Key.Tail, entry.Value)))));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class FieldTexts {
    public static Seq<FieldText<TRecord>> Items<TRecord>() where TRecord : IStateRecord<TRecord> =>
        Roots<TRecord>().Bind(static field => field.Flattened);

    private static Seq<FieldText<TRecord>> Roots<TRecord>() where TRecord : IStateRecord<TRecord> =>
        toSeq(TRecord.Parameters).Bind(Root).Strict();

    public static FieldCodec<TValue> Leaf<TValue>(Func<TValue, string> capture, Func<string, IO<TValue>> recall) =>
        new(value => HashMap((Seq<string>(), capture(value))),
            entries => IO.lift(entries.Find(Seq<string>()).ToFin(new MissingField(typeof(TValue)))).Bind(recall));

    public static FieldCodec<Option<TValue>> Optional<TValue>(Func<TValue, string> capture, Func<string, IO<TValue>> recall) =>
        Leaf(value => value.Map(capture).IfNone(Marker), text => Unmarked(text).Traverse(recall).As());

    public static FieldCodec<Gated<TValue>> Gate<TValue>(FieldCodec<TValue> value) where TValue : notnull {
        FieldCodec<bool> enabled = Leaf(StoredText.Format, static text => IO.lift(StoredText.Parse<bool>(text))).Under(Gated<TValue>.EnabledEntry.Key);
        FieldCodec<TValue> kept = value.Under(Gated<TValue>.ValueEntry.Key);
        return new(held => enabled.Capture(held.Enabled).Union(kept.Capture(held.Value)), entries =>
            (enabled.Recall(entries).Try().ToValidation(), kept.Recall(entries).Try().ToValidation())
                .Apply(static (enabled, item) => new Gated<TValue>(enabled, item)).Run().As()
                .Bind(static held => IO.lift(held.ToFin())));
    }

    public static FieldCodec<TRecord> Record<TRecord>() where TRecord : IStateRecord<TRecord> =>
        new(static record => toHashMap(Items<TRecord>().Choose(field => field.Capture(record).Map(text => (field.Path, text)))),
            static entries => Recalled(typeof(TRecord).ToString(), toSeq(TRecord.Parameters), path => IO.pure(entries.Find(path)), TRecord.Default));

    public static FieldCodec<TValue> Variant<TValue>() where TValue : class =>
        new(static value => StateCase<TValue>.Items.Single(item => item.Holds(value)) switch {
            var item => item.Accept(CodecCases<TValue>.Instance).Under(item.Key).Capture(value)
                .Add(Seq(StateCases.SelectionKey), item.Key),
        }, static entries =>
            from selected in IO.lift(entries.Find(Seq(StateCases.SelectionKey)).ToFin(new Missing(StateCases.SelectionKey)))
            from item in IO.lift(StateCase<TValue>.Items.Find(item => string.Equals(item.Key, selected, StringComparison.Ordinal)).ToFin(new UnreadText(selected, typeof(TValue))))
            from value in item.Accept(CodecCases<TValue>.Instance).Under(item.Key).Recall(entries)
            select value);

    public static Seq<FieldText<TRecord>> Of<TRecord>(IStateParameter<TRecord> parameter) =>
        Root(parameter).Bind(static field => field.Flattened);

    internal static Seq<FieldText<TRecord>> Root<TRecord>(IStateParameter<TRecord> parameter) =>
        parameter.Kind.Accept(new Crossing<TRecord>(parameter.Kind)).Map(text => text.Under(parameter.Key));

    public static (Func<TKey, string> Shown, Func<TKey, string> Entry) Number<TKey>(Presentation<TKey> presentation, (TKey Low, TKey High) bounds)
        where TKey : struct, System.Numerics.INumber<TKey> {
        ScalarDisplay display = Quantities.Scalar(presentation, bounds);
        string digits = display.Decimals == 0 ? "0" : $"0.{new string('0', display.Decimals)}";
        string format = presentation.Soft.IfNone(bounds).Low < TKey.Zero ? $"+{digits};-{digits};{digits}" : digits;
        Func<TKey, CultureInfo, string> number = (key, culture) => display.Shown(double.CreateChecked(key)).ToString(format, culture);
        return (
            key => display.Symbol.Match(Some: symbol => $"{number(key, RowText.Culture)} {symbol}", None: () => number(key, RowText.Culture)),
            key => display.Shown(double.CreateChecked(key)).ToString("R", CultureInfo.InvariantCulture));
    }

    public static IO<(TRecord Record, Seq<Error> Refused)> Folded<TRecord>(string owner, Func<Seq<string>, IO<Option<string>>> stored, TRecord factory) where TRecord : IStateRecord<TRecord> =>
        from parts in Read(Roots<TRecord>(), owner, stored, factory)
        select (parts.Accepted.Fold(factory, static (record, set) => set(record)), parts.Refused);

    private static IO<(Seq<Func<TRecord, TRecord>> Accepted, Seq<Error> Refused)> Read<TRecord>(
        Seq<FieldText<TRecord>> fields, string owner, Func<Seq<string>, IO<Option<string>>> stored, TRecord factory) =>
        from parts in fields.Map<K<IO, (Seq<Func<TRecord, TRecord>> Accepted, Seq<Error> Refused)>>(item =>
                from entry in stored(item.Path)
                from found in IO.lift(() => entry.Traverse(text => new EntryKey(owner, item.Path).Read(text, item.Recall)).As())
                from loaded in found.Traverse(static step => step).As()
                let current = loaded.Match(Some: set => set(factory), None: () => factory)
                from children in Read(item.Children.Filter(field => field.Active(current)), owner, stored, current)
                select (loaded.ToSeq() + children.Accepted, children.Refused))
            .PartitionFallible()
            .As()
        select (parts.Succs.Bind(static part => part.Accepted), parts.Fails + parts.Succs.Bind(static part => part.Refused));

    public static IO<TRecord> Recalled<TRecord>(string owner, Seq<IStateParameter<TRecord>> parameters, Func<Seq<string>, IO<Option<string>>> stored, TRecord factory) =>
        from parts in Read(parameters.Bind(Root), owner, stored, factory)
        from _ in unless(parts.Refused.IsEmpty, IO.fail<Unit>(Error.Many(parts.Refused))).As()
        select parts.Accepted.Fold(factory, static (record, set) => set(record));

    private sealed class Crossing<TRecord>(StateParameter<TRecord> kind) : IStateParameterVisitor<TRecord, Seq<FieldText<TRecord>>> {
        private FieldKind<TRecord> Kind => new FieldKind<TRecord>.Leaf<TRecord>(kind, static record => record);

        public Seq<FieldText<TRecord>> Numeric<TValue, TKey>(Lens<TRecord, TValue> lens, Func<TKey, Fin<TValue>> create, Func<TValue, TKey> key, (TKey Low, TKey High) bounds, Presentation<TKey> presentation)
            where TValue : notnull
            where TKey : struct, System.Numerics.INumber<TKey> =>
            [Field(lens, value => StoredText.Capture(value, key), text => StoredText.Recall(text, create, Some<Func<TValue, Fin<TValue>>>(value => presentation.Admit(value, bounds)))) with { Shown = Numbered(presentation, bounds, create, key) }];

        public Seq<FieldText<TRecord>> OptionalNumeric<TValue, TKey>(Lens<TRecord, Gated<TValue>> lens, Func<TKey, Fin<TValue>> create, Func<TValue, TKey> key, (TKey Low, TKey High) bounds, Presentation<TKey> presentation)
            where TValue : notnull
            where TKey : struct, System.Numerics.INumber<TKey> =>
            Gate(lens, value => new StateParameter<TRecord>.Numeric<TValue, TKey>(value, create, key, bounds, presentation));

        public Seq<FieldText<TRecord>> Choice<TValue, TError>(Lens<TRecord, TValue> lens)
            where TValue : ISmartEnum<string, TValue, TError>
            where TError : Error, IValidationError<TError> =>
            [Field(lens, StoredText.Capture<TValue, string>, StoredText.Recall<TValue, string, TError>)];

        public Seq<FieldText<TRecord>> OptionalChoice<TValue, TError>(Lens<TRecord, Option<TValue>> lens)
            where TValue : ISmartEnum<string, TValue, TError>
            where TError : Error, IValidationError<TError> =>
            [OptionalField(lens, StoredText.Capture<TValue, string>, StoredText.Recall<TValue, string, TError>)];

        public Seq<FieldText<TRecord>> Variant<TValue>(Lens<TRecord, TValue> lens) where TValue : class =>
            [new([StateCases.SelectionKey], record => Some(StateCase<TValue>.Items.Single(item => item.Holds(lens.Get(record))).Key),
                text => StateCase<TValue>.Items.Find(item => string.Equals(item.Key, text, StringComparison.Ordinal))
                    .ToFin(new UnreadText(text, typeof(TValue)))
                    .Map(item => IO.pure<Func<TRecord, TRecord>>(record => item.Holds(lens.Get(record)) ? record : lens.Set(item.Default, record)))) {
                Kind = Kind,
                Taken = (from, into) => StateCase<TValue>.Items.Single(item => item.Holds(lens.Get(from))) switch {
                    var item => item.Holds(lens.Get(into)) ? into : lens.Set(item.Default, into),
                },
                Children = from item in StateCase<TValue>.Items
                           from field in item.Accept(Cases<TValue>.Instance)
                           select Focused(field, lens).Under(item.Key),
            }];

        public Seq<FieldText<TRecord>> Enumerated<TEnum>(Lens<TRecord, TEnum> lens) where TEnum : struct, Enum =>
            [Field(lens, StoredText.Format, StoredText.Member<TEnum>)];

        public Seq<FieldText<TRecord>> Record<TNested>(Lens<TRecord, TNested> lens) where TNested : IStateRecord<TNested> =>
            Roots<TNested>().Map(field => Focused(field, lens));

        public Seq<FieldText<TRecord>> OptionalRecord<TNested>(Lens<TRecord, Gated<TNested>> lens) where TNested : IStateRecord<TNested> =>
            Gate(lens, static value => new StateParameter<TRecord>.Record<TNested>(value));

        public Seq<FieldText<TRecord>> Toggle(Lens<TRecord, bool> lens) =>
            [Field(lens, StoredText.Format, StoredText.Parse<bool>)];

        public Seq<FieldText<TRecord>> Raw<TRaw>(Lens<TRecord, TRaw> lens) where TRaw : notnull, ISpanParsable<TRaw> =>
            [Field(lens, StoredText.Format, StoredText.Parse<TRaw>)];

        public Seq<FieldText<TRecord>> OptionalRaw<TRaw>(Lens<TRecord, Option<TRaw>> lens) where TRaw : notnull, ISpanParsable<TRaw> =>
            [OptionalField(lens, StoredText.Format, StoredText.Parse<TRaw>)];

        public Seq<FieldText<TRecord>> Color(Lens<TRecord, Swatch> lens) =>
            [Field(lens, StoredText.Capture<Swatch, int>, StoredText.Recall<Swatch, int, InvalidPixelValue>) with { Shown = Hex }];

        public Seq<FieldText<TRecord>> OptionalColor(Lens<TRecord, Gated<Swatch>> lens) =>
            Gate(lens, static value => new StateParameter<TRecord>.Color(value));

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
                text => Unmarked(text).Traverse(StoredText.Recall<TKey, TRaw, TError>).As().Map(found => found.Traverse(load).As().Bind(Put(lens)))) {
                    Kind = Kind, Taken = (from, into) => lens.Set(lens.Get(from), into), Absent = Some(Marker), File = Some(Unmarked),
                }];

        public Seq<FieldText<TRecord>> Opaque<TValue>(Lens<TRecord, TValue> lens) where TValue : notnull => [];

        private static Seq<FieldText<TRecord>> Gate<TValue>(Lens<TRecord, Gated<TValue>> gate, Func<Lens<TRecord, TValue>, StateParameter<TRecord>> value) where TValue : notnull =>
            Root(new KeyParameter<TRecord>(Gated<TValue>.EnabledEntry.Key, new StateParameter<TRecord>.Toggle(lens(gate, Gated<TValue>.EnabledEntry.Lens))))
            + Root(new KeyParameter<TRecord>(Gated<TValue>.ValueEntry.Key, value(lens(gate, Gated<TValue>.ValueEntry.Lens))));

        private FieldText<TRecord> Field<TValue>(Lens<TRecord, TValue> lens, Func<TValue, string> capture, Func<string, Fin<TValue>> recall) =>
            new([], record => Some(capture(lens.Get(record))), text => recall(text).Map(Put(lens))) { Kind = Kind, Taken = (from, into) => lens.Set(lens.Get(from), into) };

        private FieldText<TRecord> OptionalField<TValue>(Lens<TRecord, Option<TValue>> lens, Func<TValue, string> capture, Func<string, Fin<TValue>> recall) =>
            new([], record => Some(lens.Get(record).Map(capture).IfNone(Marker)), text => Unmarked(text).Traverse(recall).As().Map(Put(lens))) {
                Kind = Kind, Taken = (from, into) => lens.Set(lens.Get(from), into), Absent = Some(Marker),
            };

        private static Func<TFocus, IO<Func<TRecord, TRecord>>> Put<TFocus>(Lens<TRecord, TFocus> lens) =>
            value => IO.pure<Func<TRecord, TRecord>>(record => lens.Set(value, record));

        private static FieldText<TRecord> Focused<TFocus>(FieldText<TFocus> field, Lens<TRecord, TFocus> lens) =>
            field.Within(record => Some(lens.Get(record)), lens.Get, (record, focus) => lens.Set(focus, record));

        private static Func<string, string> Numbered<TValue, TKey>(Presentation<TKey> presentation, (TKey Low, TKey High) bounds, Func<TKey, Fin<TValue>> create, Func<TValue, TKey> key)
            where TKey : struct, System.Numerics.INumber<TKey> =>
            Number(presentation, bounds).Shown switch {
                var number => text => StoredText.Recall(text, create).Map(value => number(key(value))).IfFail(_ => text),
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
                Roots<TState>().Map(static field => field.Within<TValue>(
                    static value => Prelude.Optional(value as TState), static value => value as TState ?? TState.Default,
                    static (value, held) => value is TState ? held : value));
        }
    }

    private const string Marker = "—";

    private sealed class CodecCases<TValue> : IStateCaseVisitor<TValue, FieldCodec<TValue>> where TValue : class {
        public static readonly CodecCases<TValue> Instance = new();

        public FieldCodec<TValue> Case<TState>() where TState : class, TValue, IStateRecord<TState> =>
            Record<TState>() switch {
                var codec => new(value => codec.Capture((TState)value), entries => codec.Recall(entries).Map<TValue>(static value => value)),
            };
    }

    private static Option<string> Unmarked(string text) => Some(text).Filter(static held => !string.Equals(held, Marker, StringComparison.Ordinal));
}
