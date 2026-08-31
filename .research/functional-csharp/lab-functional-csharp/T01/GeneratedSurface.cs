namespace Lab.T01;

internal sealed record UnknownTier() : Expected("tier is not basic or plus", 1), IValidationError<UnknownTier> {
    public static UnknownTier Create(string message) => new();
}

[SmartEnum<string>]
[ValidationError<UnknownTier>]
internal sealed partial class Tier {
    public static readonly Tier Basic = new("basic");
    public static readonly Tier Plus = new("plus");
}

internal static class Lookup {
    public static Option<T> Find<T, TKey>(TKey key) where T : ISmartEnum<TKey, T, ValidationError> where TKey : notnull =>
        T.TryGet(key, out T? item) ? Some(item) : None;
    public static Fin<T> Admit<T, TKey, TError>(TKey key) where T : ISmartEnum<TKey, T, TError> where TKey : notnull where TError : Error, IValidationError<TError> =>
        T.Validate(key, CultureInfo.InvariantCulture, out T? item) is { } error ? error : item!;
}
