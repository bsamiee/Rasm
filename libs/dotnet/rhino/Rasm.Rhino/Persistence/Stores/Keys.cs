using System.Globalization;
using System.Numerics;
using Rasm.UI;

namespace Rasm.Rhino.Persistence.Stores;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ValueKey<TValue>(string Key, StateParameter<TValue> Kind, ParameterText Text, TValue Default) : IStateParameter<TValue> where TValue : notnull {
    public FieldCodec<TValue> Codec => new([this], Default);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ValueKey {
    public static ValueKey<TValue> Numeric<TValue, TKey>(string key, ParameterText text, TValue @default, NumericDomain<TValue, TKey> domain)
        where TValue : notnull
        where TKey : struct, INumber<TKey> =>
        new(key, new StateParameter<TValue>.Numeric<TValue, TKey>(Lens.identity<TValue>(), domain), text, @default);

    public static ValueKey<TValue> Keyed<TValue, TRaw>(string key, ParameterText text, TValue @default)
        where TValue : IObjectFactory<TValue, TRaw, ValidationError>, IConvertible<TRaw>
        where TRaw : notnull, ISpanParsable<TRaw> =>
        new(key, new StateParameter<TValue>.Keyed<TValue, TRaw>(Lens.identity<TValue>()), text, @default);

    public static ValueKey<TRaw> Raw<TRaw>(string key, ParameterText text, TRaw @default) where TRaw : notnull, ISpanParsable<TRaw> =>
        new(key, new StateParameter<TRaw>.Raw<TRaw>(Lens.identity<TRaw>()), text, @default);

    public static ValueKey<bool> Toggle(string key, ParameterText text, bool @default) =>
        new(key, new StateParameter<bool>.Toggle(Lens.identity<bool>()), text, @default);

    public static ValueKey<TEnum> Enumerated<TEnum>(string key, ParameterText text, TEnum @default) where TEnum : struct, Enum =>
        new(key, new StateParameter<TEnum>.Enumerated<TEnum>(Lens.identity<TEnum>()), text, @default);

    public static ValueKey<Option<TValue>> OptionalKeyed<TValue, TRaw>(string key, ParameterText text)
        where TValue : IObjectFactory<TValue, TRaw, ValidationError>, IConvertible<TRaw>
        where TRaw : notnull, ISpanParsable<TRaw> =>
        new(key, new StateParameter<Option<TValue>>.OptionalKeyed<TValue, TRaw>(Lens.identity<Option<TValue>>()), text, None);

    public static ValueKey<Option<TValue>> OptionalChoice<TValue>(string key, ParameterText text, string absent)
        where TValue : ISmartEnum<string, TValue, ValidationError> =>
        new(key, new StateParameter<Option<TValue>>.OptionalChoice<TValue>(Lens.identity<Option<TValue>>(), absent), text, None);
}

public static class StoredText {
    public static string Format<TRaw>(TRaw raw) where TRaw : notnull => Convert.ToString(raw, CultureInfo.InvariantCulture)!;

    public static Fin<TRaw> Parse<TRaw>(string text) where TRaw : ISpanParsable<TRaw> =>
        TRaw.TryParse(text, CultureInfo.InvariantCulture, out TRaw? raw) ? raw : Errors.ParseError($"{text} is no {typeof(TRaw).Name}");
}
