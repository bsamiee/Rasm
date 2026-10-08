using System.Globalization;

namespace Rasm.Rhino.Persistence.Stores;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ValueKey<TValue, TRaw, TError> where TRaw : notnull {
    internal ValueKey(string name, string caption, string help, TValue @default, Func<TRaw, Fin<TValue>> from, Func<TValue, TRaw> toRaw, Func<string, Fin<TValue>> read) =>
        (Name, Caption, Help, Default, From, ToRaw, Read) = (name, caption, help, @default, from, toRaw, read);

    public string Name { get; }
    public string Caption { get; }
    public string Help { get; }
    public TValue Default { get; }
    public Func<TRaw, Fin<TValue>> From { get; }
    public Func<TValue, TRaw> ToRaw { get; }
    public Func<string, Fin<TValue>> Read { get; }

    public string Text(TValue value) => StoredText.Format(ToRaw(value));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ValueKey {
    public static ValueKey<TValue, TRaw, TError> Of<TValue, TRaw, TError>(string name, string caption, string help, TValue @default)
        where TValue : IObjectFactory<TValue, TRaw, TError>, IConvertible<TRaw>
        where TRaw : notnull, ISpanParsable<TRaw>
        where TError : Error, IValidationError<TError> =>
        new(name, caption, help, @default, Conversions.Validated<TValue, TRaw, TError>, static value => value.ToValue(), StoredText.Recall<TValue, TRaw, TError>);

    public static ValueKey<TRaw, TRaw, InvalidRhinoValue> Raw<TRaw>(string name, string caption, string help, TRaw @default) where TRaw : notnull, ISpanParsable<TRaw> =>
        new(name, caption, help, @default, Fin.Succ, identity, StoredText.Parse<TRaw>);

    public static ValueKey<TEnum, TEnum, InvalidRhinoValue> Enum<TEnum>(string name, string caption, string help, TEnum @default) where TEnum : struct, Enum =>
        new(name, caption, help, @default, static raw => System.Enum.IsDefined(raw) ? raw : new InvalidRhinoValue(), identity, StoredText.Member<TEnum>);

    public static ValueKey<Option<TValue>, string, TError> Optional<TValue, TError>(string name, string caption, string help)
        where TValue : IObjectFactory<TValue, string, TError>, IConvertible<string>
        where TError : Error, IValidationError<TError> =>
        fun(static (string text) => Conversions.Present(text).Traverse(Conversions.Validated<TValue, string, TError>).As()) switch {
            var recall => new(name, caption, help, Option<TValue>.None, recall, static value => Conversions.Unset(value.Map(static held => held.ToValue())), recall),
        };
}

public static class StoredText {
    public static string Format<TRaw>(TRaw raw) where TRaw : notnull =>
$"{raw}";

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
