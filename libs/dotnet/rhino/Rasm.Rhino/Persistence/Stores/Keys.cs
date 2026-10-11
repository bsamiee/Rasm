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
    public static ValueKey<TValue, TKey> Numeric<TValue, TKey>(string name, string caption, string help, TValue @default, Func<TKey, Fin<TValue>> create, Func<TValue, TKey> key, (TKey Low, TKey High) bounds, Presentation<TKey> presentation)
        where TValue : notnull
        where TKey : struct, INumber<TKey> =>
        fun((TKey raw) => raw >= bounds.Low && raw <= bounds.High ? create(raw) : new InvalidRhinoValue()) switch {
            var accepted => new(name, caption, help, @default, new StateParameter<TValue>.Numeric<TValue, TKey>(Lens.identity<TValue>(), accepted, key, bounds, presentation),
                accepted, key, text => StoredText.Recall(text, accepted, Some<Func<TValue, Fin<TValue>>>(value => presentation.Admit(value, bounds)))),
        };

    public static ValueKey<TValue, TRaw> Of<TValue, TRaw, TError>(string name, string caption, string help, TValue @default)
        where TValue : IObjectFactory<TValue, TRaw, TError>, IConvertible<TRaw>
        where TRaw : notnull, ISpanParsable<TRaw>
        where TError : Error, IValidationError<TError> =>
        new(name, caption, help, @default, new StateParameter<TValue>.Keyed<TValue, TRaw, TError>(Lens.identity<TValue>()),
            Conversions.Validated<TValue, TRaw, TError>, static value => value.ToValue(), StoredText.Recall<TValue, TRaw, TError>);

    public static ValueKey<TRaw, TRaw> Raw<TRaw>(string name, string caption, string help, TRaw @default) where TRaw : notnull, ISpanParsable<TRaw> =>
        new(name, caption, help, @default, new StateParameter<TRaw>.Raw<TRaw>(Lens.identity<TRaw>()), Fin.Succ, identity, StoredText.Parse<TRaw>);

    public static ValueKey<TEnum, TEnum> Enum<TEnum>(string name, string caption, string help, TEnum @default) where TEnum : struct, Enum =>
        new(name, caption, help, @default, new StateParameter<TEnum>.Enumerated<TEnum>(Lens.identity<TEnum>()),
            static raw => System.Enum.IsDefined(raw) ? raw : new InvalidRhinoValue(), identity, StoredText.Member<TEnum>);

    public static ValueKey<Option<TValue>, string> Optional<TValue, TError>(string name, string caption, string help)
        where TValue : IObjectFactory<TValue, string, TError>, IConvertible<string>
        where TError : Error, IValidationError<TError> =>
        fun(static (string text) => Conversions.Present(text).Traverse(Conversions.Validated<TValue, string, TError>).As()) switch {
            var recall => new(name, caption, help, Option<TValue>.None, new StateParameter<Option<TValue>>.OptionalKeyed<TValue, string, TError>(Lens.identity<Option<TValue>>()),
                recall, static value => Conversions.Unset(value.Map(static held => held.ToValue())), recall),
        };
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
        }).Run().MapFail(error => new UnreadText(text, typeof(TValue), Some(error)));

    public static string Capture<TValue, TKey>(TValue value, Func<TValue, TKey> key) where TKey : notnull =>
        typeof(IQuantity).IsAssignableFrom(typeof(TValue)) ? Serialize(value) : Format(key(value));

    public static Fin<TValue> Recall<TValue, TKey>(string text, Func<TKey, Fin<TValue>> create, Option<Func<TValue, Fin<TValue>>> admitted = default) where TKey : ISpanParsable<TKey> =>
        typeof(IQuantity).IsAssignableFrom(typeof(TValue))
            ? Deserialize<TValue>(text).Bind(value => admitted.Map(admit => admit(value)).IfNone(Fin.Succ(value)))
            : Parse<TKey>(text).Bind(create);

    public static string Format<TRaw>(TRaw raw) where TRaw : notnull =>
        Convert.ToString(raw, CultureInfo.InvariantCulture)!;

    public static Fin<TRaw> Parse<TRaw>(string text) where TRaw : ISpanParsable<TRaw> =>
        TRaw.TryParse(text, CultureInfo.InvariantCulture, out TRaw? raw) ? raw : new UnreadText(text, typeof(TRaw));

    public static Fin<TEnum> Member<TEnum>(string text) where TEnum : struct, Enum =>
        Enum.GetNames<TEnum>().Contains(text, StringComparer.Ordinal) ? Enum.Parse<TEnum>(text) : new UnreadText(text, typeof(TEnum));

    public static string Capture<TValue, TRaw>(TValue value)
        where TValue : IConvertible<TRaw>
        where TRaw : notnull, ISpanParsable<TRaw> =>
        Format(value.ToValue());

    public static Fin<TValue> Recall<TValue, TRaw, TError>(string text)
        where TValue : IObjectFactory<TValue, TRaw, TError>
        where TRaw : notnull, ISpanParsable<TRaw>
        where TError : Error, IValidationError<TError> =>
        Parse<TRaw>(text).Bind(Conversions.Validated<TValue, TRaw, TError>);
}
