using System.Globalization;
using System.Numerics;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using Rasm.Imaging.Pixels;
using UnitsNet;

namespace Rasm.Rhino.Persistence.Stores;

// --- [TYPES] ---------------------------------------------------------------------------
public interface IValueKeyVisitor<out TResult> {
    public TResult Visit<TValue, TRaw>(ValueKey<TValue, TRaw> key) where TValue : notnull where TRaw : notnull;
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ValueKey<TValue, TRaw>(string Name, string Caption, string Help, TValue Default, StateParameter<TValue> Kind, Func<TRaw, Fin<TValue>> From, Func<TValue, TRaw> ToRaw, Func<string, Fin<TValue>> Read) where TRaw : notnull {
    public string Text(TValue value) => StoredText.Capture(value, ToRaw);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ValueKey {
    public static ValueKey<TValue, TKey> Numeric<TValue, TKey>(string name, string caption, string help, TValue @default, NumericDomain<TValue, TKey> domain)
        where TValue : notnull
        where TKey : struct, INumber<TKey> =>
        new(name, caption, help, @default, new StateParameter<TValue>.Numeric<TValue, TKey>(Lens.identity<TValue>(), domain),
            domain.Create, domain.Key, text => StoredText.Recall(text, domain.Create, domain.Key));

    public static ValueKey<TValue, TRaw> Of<TValue, TRaw>(string name, string caption, string help, TValue @default)
        where TValue : IObjectFactory<TValue, TRaw, ValidationError>, IConvertible<TRaw>
        where TRaw : notnull, ISpanParsable<TRaw> =>
        new(name, caption, help, @default, new StateParameter<TValue>.Keyed<TValue, TRaw>(Lens.identity<TValue>()),
            ValidationResults.Validated<TValue, TRaw>, static value => value.ToValue(), StoredText.Recall<TValue, TRaw>);

    public static ValueKey<TRaw, TRaw> Raw<TRaw>(string name, string caption, string help, TRaw @default) where TRaw : notnull, ISpanParsable<TRaw> =>
        new(name, caption, help, @default, new StateParameter<TRaw>.Raw<TRaw>(Lens.identity<TRaw>()), Fin.Succ, identity, StoredText.Parse<TRaw>);

    public static ValueKey<bool, bool> Raw(string name, string caption, string help, bool @default) =>
        new(name, caption, help, @default, new StateParameter<bool>.Toggle(Lens.identity<bool>()), Fin.Succ, identity, StoredText.Parse<bool>);

    public static ValueKey<TEnum, TEnum> Enum<TEnum>(string name, string caption, string help, TEnum @default, Func<TEnum, string> members) where TEnum : struct, Enum =>
        new(name, caption, help, @default, new StateParameter<TEnum>.Enumerated<TEnum>(Lens.identity<TEnum>(), members),
            static raw => System.Enum.IsDefined(raw) ? raw : Error.New(ErrorCodes.ValidationFailed, $"{raw} is no defined {typeof(TEnum).Name}"), identity, StoredText.Member<TEnum>);

    public static ValueKey<Option<TValue>, string> Optional<TValue>(string name, string caption, string help)
        where TValue : IObjectFactory<TValue, string, ValidationError>, IConvertible<string> =>
        fun(static (string text) => Conversions.Present(text).Traverse(ValidationResults.Validated<TValue, string>).As()) switch {
            var recall => new(name, caption, help, Option<TValue>.None, new StateParameter<Option<TValue>>.OptionalKeyed<TValue, string>(Lens.identity<Option<TValue>>()),
                recall, static value => Conversions.Unset(value.Map(static held => held.ToValue())), recall),
        };

    public static ValueKey<Option<TValue>, string> OptionalChoice<TValue>(string name, string caption, string help, string absent)
        where TValue : ISmartEnum<string, TValue, ValidationError>, ICaptioned =>
        Optional<TValue>(name, caption, help) with { Kind = new StateParameter<Option<TValue>>.OptionalChoice<TValue>(Lens.identity<Option<TValue>>(), absent) };
}

public static class StoredText {
    public static string Serialize<TValue>(TValue value) {
        StringBuilder text = new();
        using XmlWriter writer = XmlWriter.Create(text);
        new DataContractSerializer(typeof(TValue)).WriteObject(writer, value);
        writer.Flush();
        return text.ToString();
    }

    public static Fin<TValue> Deserialize<TValue>(string text) =>
        Try.lift(() => {
            using StringReader input = new(text);
            using XmlReader reader = XmlReader.Create(input);
            return (TValue)new DataContractSerializer(typeof(TValue)).ReadObject(reader)!;
        }).Run().MapFail(error => Error.New(ErrorCodes.ParseError, $"{text} is no {typeof(TValue).Name}", error));

    public static string Capture<TValue, TKey>(TValue value, Func<TValue, TKey> key) where TKey : notnull =>
        typeof(IQuantity).IsAssignableFrom(typeof(TValue)) ? Serialize(value) : Format(key(value));

    public static Fin<TValue> Recall<TValue, TKey>(string text, Func<TKey, Fin<TValue>> create, Func<TValue, TKey> key) where TKey : ISpanParsable<TKey> =>
        typeof(IQuantity).IsAssignableFrom(typeof(TValue))
            ? from value in Deserialize<TValue>(text)
              from _ in create(key(value))
              select value
            : Parse<TKey>(text).Bind(create);

    public static string Format<TRaw>(TRaw raw) where TRaw : notnull =>
        Convert.ToString(raw, CultureInfo.InvariantCulture)!;

    public static Fin<TRaw> Parse<TRaw>(string text) where TRaw : ISpanParsable<TRaw> =>
        TRaw.TryParse(text, CultureInfo.InvariantCulture, out TRaw? raw) ? raw : Errors.ParseError($"{text} is no {typeof(TRaw).Name}");

    public static Fin<TEnum> Member<TEnum>(string text) where TEnum : struct, Enum =>
        Enum.GetNames<TEnum>().Contains(text, StringComparer.Ordinal) ? Enum.Parse<TEnum>(text) : Errors.ParseError($"{text} is no {typeof(TEnum).Name}");

    public static string Capture<TValue, TRaw>(TValue value)
        where TValue : IConvertible<TRaw>
        where TRaw : notnull, ISpanParsable<TRaw> =>
        Format(value.ToValue());

    public static Fin<TValue> Recall<TValue, TRaw>(string text)
        where TValue : IObjectFactory<TValue, TRaw, ValidationError>
        where TRaw : notnull, ISpanParsable<TRaw> =>
        Parse<TRaw>(text).Bind(ValidationResults.Validated<TValue, TRaw>);
}
